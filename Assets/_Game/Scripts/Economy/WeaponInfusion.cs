using System;

namespace CindarsHope.Economy
{
    /// <summary>
    /// fable_22 — elemento canônico de têmpera de essência. Mapeia 1:1 com as 6 essências do
    /// ITEM_CATALOG §10 (fire/ice/toxic/lightning/arcane/void) e com a StatusTag de gume aplicada
    /// à arma (FireEdge/FrostEdge/PoisonEdge/ShockEdge/ArcaneEdge/VoidEdge). Valores estáveis
    /// pinados para nunca driftar em saves serializados por inteiro.
    /// </summary>
    public enum TemperingElement
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Toxic = 3,
        Lightning = 4,
        Arcane = 5,
        Void = 6
    }

    /// <summary>
    /// fable_22 — infusão permanente {element, tier} gravada por instância de arma. Tipo PURO
    /// (sem refs Unity) para caber no DTO de equipment aditivo e ser 100% testável em EditMode.
    /// Tier 0 = sem infusão; 1 e 2 = têmpera C3.
    /// </summary>
    [Serializable]
    public readonly struct WeaponInfusion
    {
        public readonly TemperingElement Element;
        public readonly int Tier;

        public WeaponInfusion(TemperingElement element, int tier)
        {
            Element = element;
            Tier = tier;
        }

        public bool IsActive => Element != TemperingElement.None && Tier >= 1;

        public static readonly WeaponInfusion None = new WeaponInfusion(TemperingElement.None, 0);
    }

    /// <summary>
    /// fable_22 — tabela canônica única (decisões §C3 + ITEM_CATALOG §10). Centraliza o vocabulário
    /// para que a tag de gume (que entra no matching F06 como qualquer tag), a essência consumida e
    /// a chance de status por tier tenham UMA fonte de verdade. Não duplica o matching nem cria
    /// multiplicador próprio: só fornece a STRING de tag canônica que o ponto único de combate
    /// adiciona a DamageRequest.WeaponMaterialTags.
    /// </summary>
    public static class TemperingCanon
    {
        /// <summary>StatusTag de gume canônica do elemento (string que casa no matching F06).</summary>
        public static string EdgeTag(TemperingElement element)
        {
            switch (element)
            {
                case TemperingElement.Fire: return "FireEdge";
                case TemperingElement.Ice: return "FrostEdge";
                case TemperingElement.Toxic: return "PoisonEdge";
                case TemperingElement.Lightning: return "ShockEdge";
                case TemperingElement.Arcane: return "ArcaneEdge";
                case TemperingElement.Void: return "VoidEdge";
                default: return null;
            }
        }

        /// <summary>Item de essência consumido para o elemento (ITEM_CATALOG §10).</summary>
        public static string EssenceItemId(TemperingElement element)
        {
            switch (element)
            {
                case TemperingElement.Fire: return "item_essence_fire";
                case TemperingElement.Ice: return "item_essence_ice";
                case TemperingElement.Toxic: return "item_essence_toxic";
                case TemperingElement.Lightning: return "item_essence_lightning";
                case TemperingElement.Arcane: return "item_essence_arcane";
                case TemperingElement.Void: return "item_essence_void";
                default: return null;
            }
        }

        /// <summary>Item extra exigido SÓ no T2 (1 essência void), por C3. Vazio se não exigido.</summary>
        public const string VoidCatalystItemId = "item_essence_void";

        /// <summary>Chance (0..1) de aplicar o status do elemento no hit, por tier (10% T1, 20% T2).</summary>
        public static float StatusChanceForTier(int tier)
        {
            switch (tier)
            {
                case 1: return 0.10f;
                case 2: return 0.20f;
                default: return 0f;
            }
        }

        /// <summary>Custo em ouro por tier (C3: 150g T1, 600g T2).</summary>
        public static int GoldCostForTier(int tier)
        {
            switch (tier)
            {
                case 1: return 150;
                case 2: return 600;
                default: return 0;
            }
        }

        /// <summary>Quantidade da essência do elemento consumida por tier (C3: 3 T1, 6 T2).</summary>
        public static int ElementEssenceCostForTier(int tier)
        {
            switch (tier)
            {
                case 1: return 3;
                case 2: return 6;
                default: return 0;
            }
        }

        /// <summary>Quantidade de essência void EXTRA exigida por tier (C3: 0 T1, 1 T2).</summary>
        public static int VoidCatalystCostForTier(int tier)
        {
            return tier == 2 ? 1 : 0;
        }

        public static bool IsValidTier(int tier) => tier == 1 || tier == 2;

        /// <summary>Parse estável de string (save) → elemento. Desconhecido => None.</summary>
        public static TemperingElement ParseElement(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return TemperingElement.None;
            switch (raw.Trim().ToLowerInvariant())
            {
                case "fire": return TemperingElement.Fire;
                case "ice": return TemperingElement.Ice;
                case "toxic": return TemperingElement.Toxic;
                case "lightning": return TemperingElement.Lightning;
                case "arcane": return TemperingElement.Arcane;
                case "void": return TemperingElement.Void;
                default: return TemperingElement.None;
            }
        }

        /// <summary>Serialização estável elemento → string (save).</summary>
        public static string ToStableString(TemperingElement element)
        {
            switch (element)
            {
                case TemperingElement.Fire: return "fire";
                case TemperingElement.Ice: return "ice";
                case TemperingElement.Toxic: return "toxic";
                case TemperingElement.Lightning: return "lightning";
                case TemperingElement.Arcane: return "arcane";
                case TemperingElement.Void: return "void";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// fable_22 CA-5 — regra "óleo sobrepõe têmpera temporariamente". Decide a ÚNICA tag de gume
        /// ativa no hit: se há um óleo aplicado (coatingTag não-vazio e não-expirado), ele SUPRIME a
        /// têmpera pela duração; senão vale a tag da infusão. Garante uma tag ativa por vez (sem somar
        /// bônus de óleo + têmpera). Função pura e determinística.
        /// </summary>
        public static string ResolveActiveEdgeTag(string infusionTag, string coatingTag)
        {
            return string.IsNullOrEmpty(coatingTag) ? infusionTag : coatingTag;
        }
    }
}
