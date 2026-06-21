using System.Collections.Generic;

namespace CindarsHope.UI.Death
{
    /// <summary>
    /// Where the player died. Drives the location label and the empty-state messaging.
    /// </summary>
    public enum DeathCause
    {
        Unknown = 0,
        Overworld = 1,
        Cave = 2
    }

    /// <summary>
    /// One actionable button on the death screen. <see cref="Enabled"/> false renders an
    /// honest disabled slot (future option) instead of a second button that lies.
    /// </summary>
    public sealed class DeathScreenActionViewModel
    {
        public string ActionId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;

        public DeathScreenActionViewModel() { }

        public DeathScreenActionViewModel(string actionId, string label, bool enabled)
        {
            ActionId = actionId ?? string.Empty;
            Label = label ?? string.Empty;
            Enabled = enabled;
        }
    }

    /// <summary>
    /// Pure projection of the death state for the death screen. No UnityEngine dependency,
    /// no Find, no event publishing — built from already-computed data (cause/scene/cave
    /// level, the authoritative corpse snapshot, and reported XP loss). EditMode-testable.
    /// This object only EXHIBITS data; it never changes death/penalty/corpse rules.
    /// </summary>
    public sealed class DeathScreenViewModel
    {
        public const string RespawnActionId = "respawn_anya_fountain";
        public const string FutureActionId = "future_recovery_option";

        public DeathCause Cause { get; }
        public string LocationLabel { get; }

        public bool HasCorpse { get; }
        public int CorpseItemCount { get; }
        public int CorpseGold { get; }
        public int CorpseCaveLevel { get; }

        public bool HasXpLoss { get; }
        public int XpLost { get; }

        public string Headline { get; }
        public string RecoveryInstruction { get; }

        private readonly List<DeathScreenActionViewModel> _actions = new();
        public IReadOnlyList<DeathScreenActionViewModel> Actions => _actions;

        private DeathScreenViewModel(
            DeathCause cause,
            string locationLabel,
            bool hasCorpse,
            int corpseItemCount,
            int corpseGold,
            int corpseCaveLevel,
            int xpLost,
            string headline,
            string recoveryInstruction)
        {
            Cause = cause;
            LocationLabel = locationLabel ?? string.Empty;
            HasCorpse = hasCorpse;
            CorpseItemCount = corpseItemCount < 0 ? 0 : corpseItemCount;
            CorpseGold = corpseGold < 0 ? 0 : corpseGold;
            CorpseCaveLevel = corpseCaveLevel;
            XpLost = xpLost < 0 ? 0 : xpLost;
            HasXpLoss = XpLost > 0;
            Headline = headline ?? string.Empty;
            RecoveryInstruction = recoveryInstruction ?? string.Empty;

            _actions.Add(new DeathScreenActionViewModel(
                RespawnActionId, "Renascer na Fonte de Anya", true));
            _actions.Add(new DeathScreenActionViewModel(
                FutureActionId, "Recuperar no local (em breve)", false));
        }

        /// <summary>
        /// Snapshot of the corpse used to build the cave projection. Plain values only —
        /// derived from CorpseRecoveryManager.ActiveCorpse (the authoritative source), never
        /// re-derived from loose events.
        /// </summary>
        public readonly struct CorpseSnapshot
        {
            public readonly int ItemCount;
            public readonly int Gold;
            public readonly int CaveLevel;

            public CorpseSnapshot(int itemCount, int gold, int caveLevel)
            {
                ItemCount = itemCount;
                Gold = gold;
                CaveLevel = caveLevel;
            }

            public bool HasAnything => ItemCount > 0 || Gold > 0;
        }

        /// <summary>
        /// Death inside the cave. <paramref name="deathCaveLevel"/> is the level where the
        /// player fell; the corpse snapshot carries its own cave level (they normally match,
        /// but the corpse level is what the player must return to).
        /// </summary>
        public static DeathScreenViewModel ForCaveDeath(
            int deathCaveLevel, CorpseSnapshot? corpse, int xpLost)
        {
            string location = deathCaveLevel > 0
                ? $"Caverna — nivel {deathCaveLevel}"
                : "Caverna";

            bool hasCorpse = corpse.HasValue && corpse.Value.HasAnything;
            int itemCount = hasCorpse ? corpse.Value.ItemCount : 0;
            int gold = hasCorpse ? corpse.Value.Gold : 0;
            int corpseLevel = corpse.HasValue ? corpse.Value.CaveLevel : deathCaveLevel;

            string instruction;
            if (hasCorpse)
            {
                int level = corpseLevel > 0 ? corpseLevel : deathCaveLevel;
                instruction = level > 0
                    ? $"Seus itens estao num corpo no nivel {level} — volte para recupera-los."
                    : "Seus itens ficaram num corpo na caverna — volte para recupera-los.";
            }
            else
            {
                instruction = "Voce nao deixou itens nem ouro para tras.";
            }

            return new DeathScreenViewModel(
                cause: DeathCause.Cave,
                locationLabel: location,
                hasCorpse: hasCorpse,
                corpseItemCount: itemCount,
                corpseGold: gold,
                corpseCaveLevel: corpseLevel,
                xpLost: xpLost,
                headline: "Voce caiu na caverna.",
                recoveryInstruction: instruction);
        }

        /// <summary>
        /// Death outside the cave. No corpse is created for overworld deaths, so the corpse
        /// section stays in a coherent empty state.
        /// </summary>
        public static DeathScreenViewModel ForOverworldDeath(string sceneName)
        {
            bool hasScene = !string.IsNullOrWhiteSpace(sceneName);
            string location = hasScene ? sceneName : "Local desconhecido";
            string headline = hasScene ? $"Voce morreu em {sceneName}." : "Voce morreu.";

            return new DeathScreenViewModel(
                cause: DeathCause.Overworld,
                locationLabel: location,
                hasCorpse: false,
                corpseItemCount: 0,
                corpseGold: 0,
                corpseCaveLevel: 0,
                xpLost: 0,
                headline: headline,
                recoveryInstruction: "Nada foi deixado para tras.");
        }
    }
}
