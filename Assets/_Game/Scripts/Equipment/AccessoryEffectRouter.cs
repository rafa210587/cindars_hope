using System.Collections.Generic;

namespace CindarsHope.Equipment
{
    /// <summary>
    /// fable_23 — agrega os efeitos dos 3 slots tipo-acessório (Ring1/Ring2/Accessory) aplicando a
    /// regra canônica de NÃO-STACK: o MESMO <see cref="AccessoryEffectType"/> não soma entre slots —
    /// o MAIOR valor vale (CA-3). É a fonte ÚNICA de consulta para os hooks pontuais (sem <c>if</c>
    /// espalhado): cada sistema-alvo lê <see cref="GetModifier"/> num ponto único nomeado.
    ///
    /// Pure C# (testável fora do Unity). O dono (EquipmentManager) recomputa via <see cref="Rebuild"/>
    /// a cada equip/unequip e publica a instância no acessor estático <see cref="Active"/> — o ponto
    /// único de leitura de cada hook a consome sem busca global de cena (mesmo idioma do
    /// WeaponInfusionRegistry.Active / PlayerVitalsApplier.*Source).
    ///
    /// Regra de 1 relíquia (CA-3) e per-god (F23/F68): só 1 relíquia equipada; o bônus de relíquia de
    /// um deus não soma com outra fonte do mesmo deus — o maior vale (garantido naturalmente pela
    /// agregação não-stack por tipo de efeito, já que cada deus tem efeito de relíquia próprio).
    /// </summary>
    public sealed class AccessoryEffectRouter
    {
        // Valor agregado (maior) por tipo de efeito. Ausência = 0 (efeito inativo).
        private readonly Dictionary<AccessoryEffectType, float> _modifiers = new Dictionary<AccessoryEffectType, float>();
        private int _relicCount;

        /// <summary>
        /// Instância ativa para os pontos únicos de leitura dos hooks (registrada pelo dono em runtime).
        /// Null em teste puro / cenas sem acessórios => hooks tratam como neutro (sem efeito).
        /// </summary>
        public static AccessoryEffectRouter Active { get; set; }

        /// <summary>Quantas relíquias estão atualmente agregadas (invariante: 0 ou 1).</summary>
        public int EquippedRelicCount => _relicCount;

        /// <summary>
        /// Recomputa os modificadores a partir dos itens equipados nos slots tipo-acessório.
        /// Itens não-acessório/desconhecidos são ignorados. Não-stack: maior valor por tipo.
        /// </summary>
        public void Rebuild(IEnumerable<string> equippedAccessoryInstanceIds)
        {
            _modifiers.Clear();
            _relicCount = 0;
            if (equippedAccessoryInstanceIds == null) return;

            foreach (var instanceId in equippedAccessoryInstanceIds)
            {
                var def = AccessoryCatalog.ResolveFromInstanceId(instanceId);
                if (def == null) continue;
                if (def.IsRelic) _relicCount++;

                foreach (var effect in def.Effects)
                {
                    if (effect.Type == AccessoryEffectType.None) continue;
                    // NÃO-STACK: mantém o MAIOR valor por tipo (o segundo igual não soma).
                    if (!_modifiers.TryGetValue(effect.Type, out var current) || effect.Magnitude > current)
                    {
                        _modifiers[effect.Type] = effect.Magnitude;
                    }
                }
            }
        }

        /// <summary>Magnitude agregada (não-stack, maior) do efeito; 0 se nenhum acessório o fornece.</summary>
        public float GetModifier(AccessoryEffectType type)
        {
            return _modifiers.TryGetValue(type, out var value) ? value : 0f;
        }

        /// <summary>O efeito está ativo (magnitude &gt; 0) em algum slot?</summary>
        public bool HasEffect(AccessoryEffectType type)
        {
            return GetModifier(type) > 0f;
        }

        public void Clear()
        {
            _modifiers.Clear();
            _relicCount = 0;
        }

        // ─── Validação de equip (tipo×slot + 1 relíquia + per-god) ───────────────────────────────

        /// <summary>
        /// fable_23 — pode equipar <paramref name="itemInstanceId"/> no <paramref name="targetSlot"/>?
        /// Valida: (1) o slot é tipo-acessório; (2) o item é acessório/relíquia conhecido; (3) o tipo
        /// do item é compatível com o slot (Ring→Ring1/Ring2; Amulet/Charm→Accessory); (4) no máximo
        /// 1 relíquia equipada (e per-god: uma 2ª relíquia do mesmo deus também é recusada).
        /// </summary>
        public static bool CanEquip(
            string itemInstanceId,
            EquipmentSlot targetSlot,
            IEnumerable<(EquipmentSlot Slot, string ItemInstanceId)> currentAccessorySlots,
            out string rejectionReason)
        {
            rejectionReason = string.Empty;

            if (!AccessoryCatalog.IsAccessorySlot(targetSlot))
            {
                rejectionReason = $"Slot {targetSlot} não é um slot de acessório.";
                return false;
            }

            var def = AccessoryCatalog.ResolveFromInstanceId(itemInstanceId);
            if (def == null)
            {
                rejectionReason = $"Item '{itemInstanceId}' não é um acessório ou relíquia conhecido.";
                return false;
            }

            if (!AccessoryCatalog.IsSlotCompatible(def.SlotKind, targetSlot))
            {
                rejectionReason = $"{def.SlotKind} não pode ser equipado no slot {targetSlot}.";
                return false;
            }

            if (def.IsRelic)
            {
                if (currentAccessorySlots != null)
                {
                    foreach (var entry in currentAccessorySlots)
                    {
                        // Ignora o próprio slot-alvo (re-equipar a mesma relíquia é permitido).
                        if (entry.Slot == targetSlot) continue;
                        var other = AccessoryCatalog.ResolveFromInstanceId(entry.ItemInstanceId);
                        if (other != null && other.IsRelic)
                        {
                            rejectionReason = "Apenas 1 relíquia divina pode ser equipada por vez.";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // ─── Hooks pontuais nomeados (consultados num único ponto por cada sistema-alvo) ──────────

        /// <summary>
        /// CA-2 — fonte única do modificador de ouro em vendas (Anel de Finan). Consumida no ponto
        /// único de venda da economia. Default 0 (sem acessório) => multiplicador neutro 1.0.
        /// </summary>
        public static System.Func<float> GoldGainModifierSource =
            () => Active?.GetModifier(AccessoryEffectType.GoldGainPercent) ?? 0f;

        /// <summary>
        /// CA-2 helper — aplica o bônus de ouro a um valor base de venda (ponto único, sem if espalhado).
        /// Ex.: 100 ouro com Finan (+5%) => 105.
        /// </summary>
        public static int ApplyGoldGain(int baseGold)
        {
            if (baseGold <= 0) return baseGold;
            var pct = GoldGainModifierSource?.Invoke() ?? 0f;
            if (pct <= 0f) return baseGold;
            return UnityEngine.Mathf.RoundToInt(baseGold * (1f + pct));
        }

        /// <summary>
        /// CA-2 — fonte única da CHANCE (0..1) de 1 roll extra de loot raro (Anel de Alihana +0.10).
        /// Consumida no ponto único do <c>EnemyLootResolver</c>. Default 0 => sem roll extra.
        /// </summary>
        public static System.Func<float> ExtraLootRollChanceSource =
            () => Active?.GetModifier(AccessoryEffectType.ExtraLootRollChance) ?? 0f;

        /// <summary>
        /// Fonte única do bônus de durabilidade de ferramentas (Anel de Thoren +0.15). Consumida no
        /// ponto único de aplicação de durabilidade. Default 0 => sem bônus (consumo normal de 1).
        /// </summary>
        public static System.Func<float> ToolDurabilityModifierSource =
            () => Active?.GetModifier(AccessoryEffectType.ToolDurabilityPercent) ?? 0f;

        /// <summary>
        /// Fonte única da redução de fadiga noturna (Amuleto de Nyx +0.30, valor positivo = redução).
        /// Consumida no ponto único de ganho de fadiga noturna (F16). Default 0 => sem redução.
        /// </summary>
        public static System.Func<float> NightFatigueReductionSource =
            () => Active?.GetModifier(AccessoryEffectType.NightFatigueReductionPercent) ?? 0f;

        /// <summary>
        /// Fonte única do bônus de efeito de comida (Charm de Thandra +0.15). Consumida no ponto único
        /// do <c>FoodConsumer</c>. Default 0 => efeito base.
        /// </summary>
        public static System.Func<float> FoodEffectModifierSource =
            () => Active?.GetModifier(AccessoryEffectType.FoodEffectPercent) ?? 0f;

        /// <summary>
        /// Fonte única da resistência a knockback (Charm de Stoneheart +0.50, valor positivo = redução
        /// da força). Consumida no ponto único do <c>KnockbackController</c>. Default 0 => sem redução;
        /// clamp em 1.0 para nunca inverter a direção.
        /// </summary>
        public static System.Func<float> KnockbackResistSource =
            () => Active?.GetModifier(AccessoryEffectType.KnockbackResistPercent) ?? 0f;

        /// <summary>
        /// Helper de durabilidade (ponto único): quanto de durabilidade efetivamente consumir para um
        /// uso base de <paramref name="baseUse"/> (normalmente 1). Thoren (+15%) reduz o consumo médio;
        /// retorna o consumo efetivo arredondado, com piso 0 (nunca negativo). Determinístico/testável.
        /// </summary>
        public static int ApplyToolDurabilityUse(int baseUse)
        {
            if (baseUse <= 0) return baseUse;
            var pct = ToolDurabilityModifierSource?.Invoke() ?? 0f;
            if (pct <= 0f) return baseUse;
            var effective = baseUse * (1f - UnityEngine.Mathf.Clamp01(pct));
            return System.Math.Max(0, UnityEngine.Mathf.RoundToInt(effective));
        }

        /// <summary>
        /// Helper de fadiga noturna (ponto único): aplica a redução do Amuleto de Nyx ao ganho base de
        /// fadiga noturna. Ex.: 10 de fadiga com Nyx (-30%) => 7. Piso 0; sem acessório => valor base.
        /// </summary>
        public static float ApplyNightFatigueReduction(float baseFatigue)
        {
            if (baseFatigue <= 0f) return baseFatigue;
            var pct = NightFatigueReductionSource?.Invoke() ?? 0f;
            if (pct <= 0f) return baseFatigue;
            return UnityEngine.Mathf.Max(0f, baseFatigue * (1f - UnityEngine.Mathf.Clamp01(pct)));
        }

        /// <summary>
        /// Helper de efeito de comida (ponto único): escala um valor base de restauração (fome/stamina)
        /// pelo bônus do Charm de Thandra (+15%). Ex.: 40 com Thandra => 46. Sem acessório => valor base.
        /// </summary>
        public static int ApplyFoodEffect(int baseAmount)
        {
            if (baseAmount <= 0) return baseAmount;
            var pct = FoodEffectModifierSource?.Invoke() ?? 0f;
            if (pct <= 0f) return baseAmount;
            return UnityEngine.Mathf.RoundToInt(baseAmount * (1f + pct));
        }

        /// <summary>
        /// Helper de knockback (ponto único): reduz a força recebida pela resistência do Charm de
        /// Stoneheart (+50%). Ex.: força 8 com Stoneheart => 4. Clamp01 na redução => nunca inverte a
        /// direção; piso 0. Sem acessório => força base.
        /// </summary>
        public static float ApplyKnockbackResist(float baseForce)
        {
            if (baseForce <= 0f) return baseForce;
            var pct = KnockbackResistSource?.Invoke() ?? 0f;
            if (pct <= 0f) return baseForce;
            return UnityEngine.Mathf.Max(0f, baseForce * (1f - UnityEngine.Mathf.Clamp01(pct)));
        }

        /// <summary>
        /// CA-2 helper — decide se UM roll extra de loot raro ocorre, dado um valor sorteado
        /// <paramref name="roll01"/> em [0,1) (do RNG determinístico do resolver). Ponto único de
        /// consulta da chance do Anel de Alihana — sem acessório => false.
        /// </summary>
        public static bool ShouldRollExtraLoot(double roll01)
        {
            var chance = ExtraLootRollChanceSource?.Invoke() ?? 0f;
            return chance > 0f && roll01 < chance;
        }
    }
}
