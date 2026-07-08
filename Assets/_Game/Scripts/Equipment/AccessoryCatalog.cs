using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Equipment
{
    /// <summary>
    /// fable_23 — qual TIPO de slot tipo-acessório um item ocupa. Mapeia a taxonomia do catálogo
    /// (Ring/Amulet/Charm do ITEM_CATALOG §16-17) para os 3 slots reais do <see cref="EquipmentSlot"/>:
    /// Ring → Ring1/Ring2 (qualquer um dos dois), Amulet/Charm → Accessory.
    /// </summary>
    public enum AccessorySlotKind
    {
        Ring = 0,
        Amulet = 1,
        Charm = 2
    }

    /// <summary>
    /// fable_23 — efeito tipado + magnitude de um acessório (uma linha do ITEM_CATALOG §16-17).
    /// Imutável. Magnitude sempre POSITIVA (a semântica de "redução" já está no nome do tipo).
    /// </summary>
    public readonly struct AccessoryEffect
    {
        public readonly AccessoryEffectType Type;
        public readonly float Magnitude;

        public AccessoryEffect(AccessoryEffectType type, float magnitude)
        {
            Type = type;
            Magnitude = magnitude;
        }
    }

    /// <summary>
    /// fable_23 — definição canônica de um acessório/relíquia: o slot que ocupa, seus efeitos tipados
    /// e (se relíquia) o deus associado. Imutável; descreve SÓ a camada de gameplay de efeito —
    /// NÃO duplica o item-catalog (BV/nome/loja/drop vivem no CanonicalItemCatalog editor / ItemDataSO).
    /// </summary>
    public sealed class AccessoryDefinition
    {
        public string Id { get; }
        public AccessorySlotKind SlotKind { get; }
        public bool IsRelic { get; }
        public RelicGod God { get; }
        public IReadOnlyList<AccessoryEffect> Effects { get; }

        public AccessoryDefinition(string id, AccessorySlotKind slotKind, AccessoryEffect[] effects,
            bool isRelic = false, RelicGod god = RelicGod.None)
        {
            Id = id;
            SlotKind = slotKind;
            Effects = effects ?? System.Array.Empty<AccessoryEffect>();
            IsRelic = isRelic;
            God = god;
        }
    }

    /// <summary>
    /// fable_23 — catálogo PURO (sem Unity, testável) dos 12 acessórios + 4 relíquias canônicos do
    /// ITEM_CATALOG §16-17. Fonte ÚNICA do mapeamento item→efeito em runtime: o gerador de assets
    /// (CanonicalItemCatalog, editor-only) não é visível em runtime, então este catálogo carrega só a
    /// camada de EFEITO de gameplay (slot/tipo/magnitude/relíquia). Resolução por CONTÊM o id canônico
    /// no itemInstanceId (mesmo idioma de EquipmentManager.InferToolTypeFromId) — instâncias derivam do
    /// id de definição.
    /// </summary>
    public static class AccessoryCatalog
    {
        // IDs canônicos (ITEM_CATALOG §16-17). Públicos para uso por testes e hooks.
        public const string RingThoren = "item_acc_ring_thoren";
        public const string RingFinan = "item_acc_ring_finan";
        public const string RingAlihana = "item_acc_ring_alihana";
        public const string RingSwiftcurrent = "item_acc_ring_swiftcurrent";
        public const string RingRootguard = "item_acc_ring_rootguard";
        public const string RingEmberward = "item_acc_ring_emberward";
        public const string AmuletAnya = "item_acc_amulet_anya";
        public const string AmuletKanthor = "item_acc_amulet_kanthor";
        public const string AmuletSenya = "item_acc_amulet_senya";
        public const string AmuletNyx = "item_acc_amulet_nyx";
        public const string CharmThandra = "item_acc_charm_thandra";
        public const string CharmStoneheart = "item_acc_charm_stoneheart";

        public const string RelicKanthor = "item_relic_kanthor";
        public const string RelicKaand = "item_relic_kaand";
        public const string RelicAnya = "item_relic_anya";
        public const string RelicAlihana = "item_relic_alihana";

        private static readonly List<AccessoryDefinition> _all = BuildAll();
        private static readonly Dictionary<string, AccessoryDefinition> _byId = BuildIndex(_all);

        /// <summary>Todas as definições canônicas (12 acessórios + 4 relíquias).</summary>
        public static IReadOnlyList<AccessoryDefinition> All => _all;

        /// <summary>Definição EXATA por id canônico, ou null.</summary>
        public static AccessoryDefinition GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        /// <summary>
        /// Resolve a definição a partir de um itemInstanceId que CONTÉM um id canônico de acessório
        /// (instância derivada do id de definição). Retorna null se não for um acessório/relíquia
        /// conhecido. Match determinístico pelo id mais longo contido (evita prefixo ambíguo).
        /// </summary>
        public static AccessoryDefinition ResolveFromInstanceId(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return null;
            if (_byId.TryGetValue(itemInstanceId, out var exact)) return exact;

            AccessoryDefinition best = null;
            foreach (var def in _all)
            {
                if (itemInstanceId.Contains(def.Id) &&
                    (best == null || def.Id.Length > best.Id.Length))
                {
                    best = def;
                }
            }
            return best;
        }

        public static bool IsAccessoryOrRelic(string itemInstanceId)
        {
            return ResolveFromInstanceId(itemInstanceId) != null;
        }

        public static bool IsRelic(string itemInstanceId)
        {
            var def = ResolveFromInstanceId(itemInstanceId);
            return def != null && def.IsRelic;
        }

        /// <summary>O slot de definição (Ring/Amulet/Charm) é compatível com o EquipmentSlot real?</summary>
        public static bool IsSlotCompatible(AccessorySlotKind kind, EquipmentSlot slot)
        {
            switch (kind)
            {
                case AccessorySlotKind.Ring:
                    return slot == EquipmentSlot.Ring1 || slot == EquipmentSlot.Ring2;
                case AccessorySlotKind.Amulet:
                case AccessorySlotKind.Charm:
                    return slot == EquipmentSlot.Accessory;
                default:
                    return false;
            }
        }

        /// <summary>Os 3 slots tipo-acessório canônicos (inventory_equipment_rules: Ring1, Ring2, Accessory).</summary>
        public static bool IsAccessorySlot(EquipmentSlot slot)
        {
            return slot == EquipmentSlot.Ring1 || slot == EquipmentSlot.Ring2 || slot == EquipmentSlot.Accessory;
        }

        private static Dictionary<string, AccessoryDefinition> BuildIndex(List<AccessoryDefinition> all)
        {
            var map = new Dictionary<string, AccessoryDefinition>(all.Count);
            foreach (var def in all) map[def.Id] = def;
            return map;
        }

        private static List<AccessoryDefinition> BuildAll()
        {
            AccessoryEffect E(AccessoryEffectType t, float m) => new AccessoryEffect(t, m);

            return new List<AccessoryDefinition>
            {
                // ── §16 — 12 acessórios ───────────────────────────────────────────────────────────
                new AccessoryDefinition(RingThoren, AccessorySlotKind.Ring, new[]
                    { E(AccessoryEffectType.ToolDurabilityPercent, 0.15f) }),
                new AccessoryDefinition(RingFinan, AccessorySlotKind.Ring, new[]
                    { E(AccessoryEffectType.GoldGainPercent, 0.05f) }),
                new AccessoryDefinition(RingAlihana, AccessorySlotKind.Ring, new[]
                    { E(AccessoryEffectType.ExtraLootRollChance, 0.10f) }),
                new AccessoryDefinition(RingSwiftcurrent, AccessorySlotKind.Ring, new[]
                    { E(AccessoryEffectType.DashDodgeStaminaReductionPercent, 0.10f) }),
                new AccessoryDefinition(RingRootguard, AccessorySlotKind.Ring, new[]
                    { E(AccessoryEffectType.RootImmuneChillResist, 1f) }),
                new AccessoryDefinition(RingEmberward, AccessorySlotKind.Ring, new[]
                    { E(AccessoryEffectType.FireHeatResistFlat, 10f) }),

                new AccessoryDefinition(AmuletAnya, AccessorySlotKind.Amulet, new[]
                    { E(AccessoryEffectType.HealingReceivedPercent, 0.25f) }),
                new AccessoryDefinition(AmuletKanthor, AccessorySlotKind.Amulet, new[]
                    {
                        E(AccessoryEffectType.BlockStabilityPercent, 0.10f),
                        E(AccessoryEffectType.PostureTakenReductionPercent, 0.20f)
                    }),
                new AccessoryDefinition(AmuletSenya, AccessorySlotKind.Amulet, new[]
                    {
                        E(AccessoryEffectType.MagicDamagePercent, 0.10f),
                        E(AccessoryEffectType.MpCostIncreasePercent, 0.10f)
                    }),
                new AccessoryDefinition(AmuletNyx, AccessorySlotKind.Amulet, new[]
                    { E(AccessoryEffectType.NightFatigueReductionPercent, 0.30f) }),

                new AccessoryDefinition(CharmThandra, AccessorySlotKind.Charm, new[]
                    {
                        E(AccessoryEffectType.FoodEffectPercent, 0.15f),
                        E(AccessoryEffectType.AnimalProductPercent, 0.10f)
                    }),
                new AccessoryDefinition(CharmStoneheart, AccessorySlotKind.Charm, new[]
                    {
                        E(AccessoryEffectType.KnockbackResistPercent, 0.50f),
                        E(AccessoryEffectType.MoveSpeedReductionPercent, 0.05f)
                    }),

                // ── §17 — 4 relíquias divinas (ocupam Amulet/Charm; só 1 relíquia equipada) ───────
                new AccessoryDefinition(RelicKanthor, AccessorySlotKind.Amulet, new[]
                    { E(AccessoryEffectType.RelicPerfectBlockHealPercent, 0.02f) }, isRelic: true, god: RelicGod.Kanthor),
                new AccessoryDefinition(RelicKaand, AccessorySlotKind.Amulet, new[]
                    { E(AccessoryEffectType.RelicCritWindowExtendSeconds, 0.5f) }, isRelic: true, god: RelicGod.Kaand),
                new AccessoryDefinition(RelicAnya, AccessorySlotKind.Amulet, new[]
                    { E(AccessoryEffectType.RelicLivingWaterPerDay, 1f) }, isRelic: true, god: RelicGod.Anya),
                new AccessoryDefinition(RelicAlihana, AccessorySlotKind.Charm, new[]
                    { E(AccessoryEffectType.RelicMonthlyCalendarReveal, 1f) }, isRelic: true, god: RelicGod.Alihana),
            };
        }
    }
}
