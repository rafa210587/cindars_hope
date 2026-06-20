using System;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — foco ÚNICO de um nível de upgrade (§35 "upgrade melhora UM foco por vez:
    /// dano OU durabilidade OU peso OU stamina OU block"). Valores estáveis pinados para nunca
    /// driftar em saves serializados por inteiro.
    /// </summary>
    public enum UpgradeFocus
    {
        None = 0,
        Damage = 1,
        Durability = 2,
        Weight = 3,
        Stamina = 4,
        Block = 5
    }

    /// <summary>
    /// fable_49 — banda de material canônica do gear tier alto (BALANCE_CURVES §8 + ITEM_CATALOG §12).
    /// Cada banda fixa o minério-âncora cujo BaseValue entra no custo de upgrade e o modificador de
    /// durabilidade por material (EQUIPMENT_MECHANICAL_BASELINES §16/§26). NUNCA em loja: só craft/têmpera.
    /// </summary>
    public enum MaterialBand
    {
        None = 0,
        Mithril = 1,      // banda 56-70
        Bromecian = 2,    // banda 71-85
        Blackstone = 3,   // banda 86-101 (Pedra Negra)
        Meteoric = 4      // banda 86-101 (Meteórica / star_iron)
    }

    /// <summary>
    /// fable_49 — tabela canônica ÚNICA da forja de tier alto. Centraliza o vocabulário (foco/banda),
    /// a fórmula derivada de custo de upgrade (Decision 2.11, idêntica a inventory_equipment_rules +
    /// economy_rules), a durabilidade derivada por classe×material (Decision 2.11) e o mapeamento
    /// gate→receita do bestiário. NÃO duplica stats por material (a matriz F03 é a fonte; receita só
    /// referencia itemId de saída) e NÃO duplica o matching de combate. Função pura e 100% testável.
    /// </summary>
    public static class HighTierGearCanon
    {
        // ─── Bandas de material e minério-âncora ────────────────────────────────────────────────

        /// <summary>BaseValue do minério-âncora da banda — o "material band" da fórmula de custo.
        /// Iguala economy_rules: mithril_ore 80, bromecian_alloy 90, star_iron 120, blackstone 200.</summary>
        public static int MaterialBandValue(MaterialBand band)
        {
            switch (band)
            {
                case MaterialBand.Mithril: return 80;     // item_material_mithril_ore
                case MaterialBand.Bromecian: return 90;   // item_material_bromecian_alloy
                case MaterialBand.Blackstone: return 200; // item_material_stabilized_blackstone
                case MaterialBand.Meteoric: return 120;   // item_material_star_iron
                default: return 0;
            }
        }

        /// <summary>Item-âncora consumido como ingrediente principal do craft/upgrade da banda.</summary>
        public static string MaterialBandItemId(MaterialBand band)
        {
            switch (band)
            {
                case MaterialBand.Mithril: return "item_material_mithril_ore";
                case MaterialBand.Bromecian: return "item_material_bromecian_alloy";
                case MaterialBand.Blackstone: return "item_material_stabilized_blackstone";
                case MaterialBand.Meteoric: return "item_material_star_iron";
                default: return null;
            }
        }

        /// <summary>Modificador de durabilidade por material (EQUIPMENT_MECHANICAL_BASELINES §16/§26):
        /// Mithril +35%, Bromecian +40%, Blackstone +55%, Meteoric +60%. Fração somada a 1.0.</summary>
        public static float MaterialDurabilityModifier(MaterialBand band)
        {
            switch (band)
            {
                case MaterialBand.Mithril: return 0.35f;
                case MaterialBand.Bromecian: return 0.40f;
                case MaterialBand.Blackstone: return 0.55f;
                case MaterialBand.Meteoric: return 0.60f;
                default: return 0f;
            }
        }

        // ─── Durabilidade derivada (Decision 2.11) ──────────────────────────────────────────────

        /// <summary>Durabilidade-base por classe: 80 arma, 150 armadura/escudo (Decision 2.11).</summary>
        public const int WeaponBaseDurability = 80;
        public const int ArmorBaseDurability = 150;

        /// <summary>Durabilidade máxima derivada = base(classe) × (1 + modificador de material). Determinística.</summary>
        public static int DerivedMaxDurability(bool isArmorOrShield, MaterialBand band)
        {
            int baseDurability = isArmorOrShield ? ArmorBaseDurability : WeaponBaseDurability;
            float modifier = 1f + MaterialDurabilityModifier(band);
            return (int)System.Math.Round(baseDurability * modifier, MidpointRounding.AwayFromZero);
        }

        // ─── Custo de upgrade derivado (Decision 2.11) ──────────────────────────────────────────

        /// <summary>Teto canônico de upgrade (§35 — só +1/+2/+3).</summary>
        public const int MaxUpgradeLevel = 3;

        public static bool IsValidUpgradeLevel(int level) => level >= 1 && level <= MaxUpgradeLevel;

        /// <summary>
        /// Custo em ouro do upgrade para o nível +N (Decision 2.11, idêntico a inventory_equipment_rules
        /// e economy_rules): Cost(+N) = (2N × MaterialBandValue) + (BV × 0.5N), arredondado para baixo.
        /// Exemplos conferidos (economy_rules): sword_iron BV120/iron15 +1=90 +2=180; sword_mithril
        /// BV640/mithril80 +1=480.
        /// </summary>
        public static int UpgradeGoldCost(int level, int itemBaseValue, int materialBandValue)
        {
            if (level <= 0) return 0;
            double cost = (2.0 * level * materialBandValue) + (itemBaseValue * 0.5 * level);
            return (int)System.Math.Floor(cost);
        }

        /// <summary>Quantidade do material-âncora consumida por upgrade de nível +N (escala linear: N unidades).</summary>
        public static int UpgradeMaterialCost(int level)
        {
            return level <= 0 ? 0 : level;
        }

        // ─── Mapeamento gate→receita (CAVE_BESTIARY_CATALOG + EMENDA 2026-06-12-D) ───────────────

        /// <summary>Slug estável da receita aprendida via first-kill do boss de gate, ou null se o gate
        /// não ensina receita de arma. Tabela 4.4 (EMENDA D): 15 Cobre Temperado · 45 Aço Profundo ·
        /// 60 Mithril Work · 75 Bromeciana · 90 Pedra Negra · 100 Meteórica (30 = planta de baú, não-arma).</summary>
        public static string RecipeUnlockForGate(int gateLevel)
        {
            switch (gateLevel)
            {
                case 15: return RecipeUnlock.TemperedCopper;
                case 45: return RecipeUnlock.DeepSteel;
                case 60: return RecipeUnlock.MithrilWork;
                case 75: return RecipeUnlock.BromecianWork;
                case 90: return RecipeUnlock.BlackstoneWork;
                case 100: return RecipeUnlock.MeteoricWork;
                default: return null;
            }
        }

        /// <summary>Banda de material desbloqueada por um slug de unlock (para validação de craft gated).</summary>
        public static MaterialBand BandForRecipeUnlock(string recipeUnlockId)
        {
            if (string.IsNullOrWhiteSpace(recipeUnlockId)) return MaterialBand.None;
            switch (recipeUnlockId.Trim().ToLowerInvariant())
            {
                case RecipeUnlock.MithrilWork: return MaterialBand.Mithril;
                case RecipeUnlock.BromecianWork: return MaterialBand.Bromecian;
                case RecipeUnlock.BlackstoneWork: return MaterialBand.Blackstone;
                case RecipeUnlock.MeteoricWork: return MaterialBand.Meteoric;
                default: return MaterialBand.None;
            }
        }
    }

    /// <summary>fable_49 — slugs canônicos de receita aprendida (chaves estáveis de save). Imutáveis.</summary>
    public static class RecipeUnlock
    {
        public const string TemperedCopper = "recipe_unlock_tempered_copper";
        public const string DeepSteel = "recipe_unlock_deep_steel";
        public const string MithrilWork = "recipe_unlock_mithril_work";
        public const string BromecianWork = "recipe_unlock_bromecian_work";
        public const string BlackstoneWork = "recipe_unlock_blackstone_work";
        public const string MeteoricWork = "recipe_unlock_meteoric_work";
    }
}
