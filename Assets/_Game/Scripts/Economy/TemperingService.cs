using System;

namespace CindarsHope.Economy
{
    /// <summary>fable_22 — porta de inventário (consumir/devolver essências). Adaptada ao InventoryManager real.</summary>
    public interface ITemperingInventory
    {
        bool HasItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
        bool AddItem(string itemId, int amount);
    }

    /// <summary>fable_22 — porta de carteira (gastar ouro). Adaptada ao PlayerManager real.</summary>
    public interface ITemperingWallet
    {
        int CurrentGold { get; }
        bool TrySpendGold(int amount);
    }

    /// <summary>
    /// fable_22 — porta de tipo do item temperado. O service precisa saber se o instanceId é uma
    /// ARMA (T1 e T2) ou FERRAMENTA (só T1, efeitos utilitários) — sem acoplar à resolução real
    /// de WeaponDataSO/ItemDataSO (resolvida pelo adapter de runtime).
    /// </summary>
    public interface ITemperingItemClassifier
    {
        bool IsWeaponInstance(string itemInstanceId);
        bool IsToolInstance(string itemInstanceId);
    }

    /// <summary>Resultado determinístico de uma têmpera.</summary>
    public sealed class TemperingResult
    {
        public bool Success { get; private set; }
        public string FailReason { get; private set; }
        public WeaponInfusion Applied { get; private set; }
        public TemperingElement RefundedElement { get; private set; }
        public int RefundedEssences { get; private set; }

        public static TemperingResult Fail(string reason) =>
            new TemperingResult { Success = false, FailReason = reason };

        public static TemperingResult Ok(WeaponInfusion applied, TemperingElement refundedElement, int refundedEssences) =>
            new TemperingResult
            {
                Success = true,
                Applied = applied,
                RefundedElement = refundedElement,
                RefundedEssences = refundedEssences
            };
    }

    /// <summary>
    /// fable_22 — Têmpera de Essência (forja permanente do Brumdar). Serviço PURO e determinístico:
    /// valida gate + tipo do item, cobra custos canônicos C3 (essências + ouro), grava a infusão
    /// {element, tier} no <see cref="WeaponInfusionRegistry"/> e devolve METADE das essências (nunca
    /// ouro) ao re-temperar. NÃO cria multiplicador de dano: a tag de gume entra no matching F06 pelo
    /// ponto único de combate (regra de não-duplicação). NÃO faz busca global nem toca o save core —
    /// a persistência é aditiva via DTO de equipment (capturado pelo EquipmentManager).
    /// </summary>
    public sealed class TemperingService
    {
        private readonly WeaponInfusionRegistry _registry;
        private readonly ITemperingInventory _inventory;
        private readonly ITemperingWallet _wallet;
        private readonly ITemperingItemClassifier _classifier;
        private readonly Action<WeaponInfusion, string> _onTempered; // (infusion, instanceId) → evento/toast

        public TemperingService(
            WeaponInfusionRegistry registry,
            ITemperingInventory inventory,
            ITemperingWallet wallet,
            ITemperingItemClassifier classifier,
            Action<WeaponInfusion, string> onTempered = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));
            _onTempered = onTempered;
        }

        /// <summary>
        /// Gate narrativo da têmpera (CA-3): cadeia do Brumdar concluída E Ato 1 alcançado.
        /// Mantido puro recebendo os dois predicados já resolvidos pelo chamador (QuestFlagService +
        /// MainProgression), para ser testável com flags sintéticas.
        /// </summary>
        public static bool IsGateOpen(bool brumdarChainDone, bool act1Reached)
        {
            return brumdarChainDone && act1Reached;
        }

        /// <summary>
        /// Aplica (ou substitui) a têmpera de <paramref name="element"/>/<paramref name="tier"/> na
        /// instância. Cobra C3, respeita invariante 1-elemento, devolve metade das essências antigas
        /// na troca. <paramref name="gateOpen"/> deve vir do <see cref="IsGateOpen"/> resolvido.
        /// </summary>
        public TemperingResult ApplyTempering(string itemInstanceId, TemperingElement element, int tier, bool gateOpen)
        {
            if (!gateOpen)
                return TemperingResult.Fail("GATE_CLOSED");

            if (string.IsNullOrWhiteSpace(itemInstanceId))
                return TemperingResult.Fail("NO_INSTANCE");

            if (element == TemperingElement.None)
                return TemperingResult.Fail("NO_ELEMENT");

            if (!TemperingCanon.IsValidTier(tier))
                return TemperingResult.Fail("INVALID_TIER");

            bool isWeapon = _classifier.IsWeaponInstance(itemInstanceId);
            bool isTool = _classifier.IsToolInstance(itemInstanceId);

            if (!isWeapon && !isTool)
                return TemperingResult.Fail("NOT_TEMPERABLE");

            // Ferramentas aceitam SÓ T1 (C3: efeitos utilitários).
            if (isTool && !isWeapon && tier != 1)
                return TemperingResult.Fail("TOOL_T1_ONLY");

            // Custos canônicos C3.
            string essenceId = TemperingCanon.EssenceItemId(element);
            int essenceCost = TemperingCanon.ElementEssenceCostForTier(tier);
            int voidCost = TemperingCanon.VoidCatalystCostForTier(tier);
            int goldCost = TemperingCanon.GoldCostForTier(tier);

            // Pré-checagem de fundos (não consome nada se faltar — evita débito parcial).
            int totalElementNeeded = essenceCost;
            if (voidCost > 0 && string.Equals(essenceId, TemperingCanon.VoidCatalystItemId, StringComparison.Ordinal))
            {
                // T2 de VOID consome a essência do elemento E o catalisador void no MESMO item.
                totalElementNeeded += voidCost;
            }

            if (!_inventory.HasItem(essenceId, totalElementNeeded))
                return TemperingResult.Fail("NO_ESSENCE");

            if (voidCost > 0 && !string.Equals(essenceId, TemperingCanon.VoidCatalystItemId, StringComparison.Ordinal)
                && !_inventory.HasItem(TemperingCanon.VoidCatalystItemId, voidCost))
                return TemperingResult.Fail("NO_VOID_CATALYST");

            if (_wallet.CurrentGold < goldCost)
                return TemperingResult.Fail("NO_GOLD");

            // Refund de metade das essências da têmpera ANTERIOR (apenas essências, nunca ouro).
            var previous = _registry.Get(itemInstanceId);
            TemperingElement refundedElement = TemperingElement.None;
            int refundedAmount = 0;
            if (previous.IsActive)
            {
                refundedElement = previous.Element;
                refundedAmount = TemperingCanon.ElementEssenceCostForTier(previous.Tier) / 2;
            }

            // Cobrança (a pré-checagem garante sucesso; checa retorno por robustez).
            if (!_inventory.RemoveItem(essenceId, totalElementNeeded))
                return TemperingResult.Fail("ESSENCE_REMOVE_FAILED");

            if (voidCost > 0 && !string.Equals(essenceId, TemperingCanon.VoidCatalystItemId, StringComparison.Ordinal))
            {
                if (!_inventory.RemoveItem(TemperingCanon.VoidCatalystItemId, voidCost))
                {
                    // rollback da essência principal para não deixar débito parcial
                    _inventory.AddItem(essenceId, totalElementNeeded);
                    return TemperingResult.Fail("VOID_REMOVE_FAILED");
                }
            }

            if (goldCost > 0 && !_wallet.TrySpendGold(goldCost))
            {
                _inventory.AddItem(essenceId, totalElementNeeded);
                if (voidCost > 0 && !string.Equals(essenceId, TemperingCanon.VoidCatalystItemId, StringComparison.Ordinal))
                    _inventory.AddItem(TemperingCanon.VoidCatalystItemId, voidCost);
                return TemperingResult.Fail("GOLD_SPEND_FAILED");
            }

            // Devolução de metade das essências antigas (após a cobrança da nova).
            if (refundedAmount > 0 && refundedElement != TemperingElement.None)
            {
                _inventory.AddItem(TemperingCanon.EssenceItemId(refundedElement), refundedAmount);
            }

            // Grava a infusão (invariante 1-elemento garantida pelo registry: SUBSTITUI).
            var applied = new WeaponInfusion(element, tier);
            _registry.Set(itemInstanceId, applied);

            _onTempered?.Invoke(applied, itemInstanceId);

            return TemperingResult.Ok(applied, refundedElement, refundedAmount);
        }
    }
}
