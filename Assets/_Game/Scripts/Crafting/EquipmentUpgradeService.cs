namespace CindarsHope.Crafting
{
    /// <summary>Porta de inventário do upgrade (mesma disciplina de ITemperingInventory): consome materiais.</summary>
    public interface IUpgradeInventory
    {
        bool HasItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
    }

    /// <summary>Porta de carteira do upgrade (gasta ouro; nunca devolve em upgrade).</summary>
    public interface IUpgradeWallet
    {
        int CurrentGold { get; }
        bool TrySpendGold(int amount);
    }

    /// <summary>Parâmetros canônicos da instância a melhorar (BaseValue do item + banda de material).</summary>
    public readonly struct UpgradeTarget
    {
        public readonly string ItemInstanceId;
        public readonly int ItemBaseValue;
        public readonly MaterialBand Band;

        public UpgradeTarget(string itemInstanceId, int itemBaseValue, MaterialBand band)
        {
            ItemInstanceId = itemInstanceId ?? string.Empty;
            ItemBaseValue = itemBaseValue;
            Band = band;
        }
    }

    /// <summary>Resultado de uma tentativa de upgrade (motivo de falha estável para testes/feedback).</summary>
    public readonly struct UpgradeResult
    {
        public readonly bool Success;
        public readonly string FailReason;
        public readonly int NewLevel;
        public readonly UpgradeFocus Focus;
        public readonly int GoldSpent;
        public readonly int MaterialSpent;

        private UpgradeResult(bool success, string failReason, int newLevel, UpgradeFocus focus, int goldSpent, int materialSpent)
        {
            Success = success;
            FailReason = failReason;
            NewLevel = newLevel;
            Focus = focus;
            GoldSpent = goldSpent;
            MaterialSpent = materialSpent;
        }

        public static UpgradeResult Ok(int newLevel, UpgradeFocus focus, int goldSpent, int materialSpent)
            => new UpgradeResult(true, null, newLevel, focus, goldSpent, materialSpent);

        public static UpgradeResult Fail(string reason)
            => new UpgradeResult(false, reason, 0, UpgradeFocus.None, 0, 0);
    }

    /// <summary>
    /// fable_49 — serviço PURO de upgrade de equipamento (+1/+2/+3 focado, §35). Espelha TemperingService:
    /// portas injetadas (inventário/carteira), zero refs Unity, 100% testável em EditMode. Aplica UM foco
    /// por nível, respeita teto +3, valida e consome custo (material-âncora da banda + ouro pela fórmula
    /// Decision 2.11) exatamente 1×, grava {level, focus} no registro aditivo e devolve o resultado. NÃO
    /// recalcula derivados aqui (§45): quem lê o registro recalcula stats/durabilidade no fluxo existente.
    ///
    /// Diferente da têmpera (elemental, F22): upgrade é numérico focado — os dois NUNCA se misturam.
    /// </summary>
    public sealed class EquipmentUpgradeService
    {
        public const string FailNoTarget = "NO_TARGET";
        public const string FailUnknownBand = "UNKNOWN_BAND";
        public const string FailMaxLevel = "MAX_LEVEL";
        public const string FailNoFocus = "NO_FOCUS";
        public const string FailFocusMismatch = "FOCUS_MISMATCH";
        public const string FailNoMaterial = "NO_MATERIAL";
        public const string FailNoGold = "NO_GOLD";

        private readonly EquipmentUpgradeRegistry _registry;
        private readonly IUpgradeInventory _inventory;
        private readonly IUpgradeWallet _wallet;

        public EquipmentUpgradeService(EquipmentUpgradeRegistry registry, IUpgradeInventory inventory, IUpgradeWallet wallet)
        {
            _registry = registry;
            _inventory = inventory;
            _wallet = wallet;
        }

        /// <summary>Custo (ouro/material) do PRÓXIMO nível de upgrade desta instância, sem aplicar nada.</summary>
        public bool TryPreviewNextCost(UpgradeTarget target, out int goldCost, out int materialCost, out int nextLevel)
        {
            goldCost = 0;
            materialCost = 0;
            nextLevel = 0;

            if (target.Band == MaterialBand.None) return false;

            int current = _registry != null ? _registry.GetLevel(target.ItemInstanceId) : 0;
            nextLevel = current + 1;
            if (!HighTierGearCanon.IsValidUpgradeLevel(nextLevel)) return false;

            int bandValue = HighTierGearCanon.MaterialBandValue(target.Band);
            goldCost = HighTierGearCanon.UpgradeGoldCost(nextLevel, target.ItemBaseValue, bandValue);
            materialCost = HighTierGearCanon.UpgradeMaterialCost(nextLevel);
            return true;
        }

        /// <summary>
        /// Aplica o próximo nível de upgrade na instância com o foco pedido. Valida na ordem: alvo, banda,
        /// teto +3, foco único (não pode trocar de foco entre níveis), material e ouro. Só consome custo
        /// (1×) e grava se TUDO passar — falha não consome nada.
        /// </summary>
        public UpgradeResult TryUpgrade(UpgradeTarget target, UpgradeFocus focus)
        {
            if (string.IsNullOrWhiteSpace(target.ItemInstanceId)) return UpgradeResult.Fail(FailNoTarget);
            if (target.Band == MaterialBand.None) return UpgradeResult.Fail(FailUnknownBand);
            if (focus == UpgradeFocus.None) return UpgradeResult.Fail(FailNoFocus);

            var existing = _registry != null ? _registry.Get(target.ItemInstanceId) : EquipmentUpgrade.None;
            int nextLevel = existing.Level + 1;
            if (!HighTierGearCanon.IsValidUpgradeLevel(nextLevel)) return UpgradeResult.Fail(FailMaxLevel);

            // §35: um foco por item. Após o primeiro nível, os seguintes mantêm o MESMO foco.
            if (existing.IsActive && existing.Focus != focus) return UpgradeResult.Fail(FailFocusMismatch);

            int bandValue = HighTierGearCanon.MaterialBandValue(target.Band);
            int goldCost = HighTierGearCanon.UpgradeGoldCost(nextLevel, target.ItemBaseValue, bandValue);
            int materialCost = HighTierGearCanon.UpgradeMaterialCost(nextLevel);
            string materialId = HighTierGearCanon.MaterialBandItemId(target.Band);

            // Checagem antes de consumir (nunca consome em falha).
            if (_inventory == null || string.IsNullOrWhiteSpace(materialId) || !_inventory.HasItem(materialId, materialCost))
                return UpgradeResult.Fail(FailNoMaterial);
            if (_wallet == null || _wallet.CurrentGold < goldCost)
                return UpgradeResult.Fail(FailNoGold);

            // Consome material primeiro; se o ouro falhar logo após, devolve o material (atomicidade).
            if (!_inventory.RemoveItem(materialId, materialCost))
                return UpgradeResult.Fail(FailNoMaterial);
            if (!_wallet.TrySpendGold(goldCost))
                return UpgradeResult.Fail(FailNoGold);

            _registry?.Set(target.ItemInstanceId, new EquipmentUpgrade(nextLevel, focus));
            return UpgradeResult.Ok(nextLevel, focus, goldCost, materialCost);
        }
    }
}
