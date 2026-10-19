using System;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — upgrade focado {level, focus} gravado por instância de equipamento. Tipo PURO (sem
    /// refs Unity) para caber no DTO de equipment aditivo e ser 100% testável em EditMode. Level 0 = sem
    /// upgrade; 1..3 = +1/+2/+3 (§35: UM foco por nível, teto +3).
    /// </summary>
    [Serializable]
    public readonly struct EquipmentUpgrade
    {
        public readonly int Level;
        public readonly UpgradeFocus Focus;

        public EquipmentUpgrade(int level, UpgradeFocus focus)
        {
            Level = level;
            Focus = focus;
        }

        public bool IsActive => Level >= 1 && Focus != UpgradeFocus.None;

        public static readonly EquipmentUpgrade None = new EquipmentUpgrade(0, UpgradeFocus.None);
    }

    /// <summary>
    /// fable_49 — parse/serialização estável do foco de upgrade (chave de save). Mesma disciplina do
    /// TemperingCanon: strings estáveis pinadas, desconhecido => None (ignorado no load).
    /// </summary>
    public static class HighTierGearCanonFocus
    {
        public static UpgradeFocus ParseFocus(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return UpgradeFocus.None;
            switch (raw.Trim().ToLowerInvariant())
            {
                case "damage": return UpgradeFocus.Damage;
                case "durability": return UpgradeFocus.Durability;
                case "weight": return UpgradeFocus.Weight;
                case "stamina": return UpgradeFocus.Stamina;
                case "block": return UpgradeFocus.Block;
                default: return UpgradeFocus.None;
            }
        }

        public static string ToStableString(UpgradeFocus focus)
        {
            switch (focus)
            {
                case UpgradeFocus.Damage: return "damage";
                case UpgradeFocus.Durability: return "durability";
                case UpgradeFocus.Weight: return "weight";
                case UpgradeFocus.Stamina: return "stamina";
                case UpgradeFocus.Block: return "block";
                default: return string.Empty;
            }
        }
    }
}
