using System.Collections.Generic;

namespace CindarsHope.UI.HUD
{
    // Debug HUD projection — explicitly marked, never shown in final HUD
    // Contains data that is helpful for development but must never leak to production gameplay
    public class DebugHudProjection
    {
        public bool IsVisible { get; set; } = false;

        // Internal IDs shown in debug
        public Dictionary<string, string> StateIds { get; set; } = new Dictionary<string, string>();

        // Scene coordinates
        public float PlayerX { get; set; }
        public float PlayerY { get; set; }
        public string ActiveScene { get; set; }

        // State machine display
        public string PlayerStateMachine { get; set; }
        public string InputFocusState { get; set; }

        // Quest/Fonte debug
        public List<string> ActiveQuestFlags { get; set; } = new List<string>();
        public string FonteState { get; set; }

        // Economy debug
        public string SaveLoadState { get; set; }
        public int EnemyBudget { get; set; }

        // Debug metadata
        public string DebugBuildVersion { get; set; }
        public bool IsDebugBuild { get; set; } = true;

        // Guard: this must NEVER be used for final gameplay communication
        public const string DebugOnlyDisclaimer = "DEBUG_HUD_NOT_FOR_FINAL_BUILD";

        public bool IsAllowedInFinalBuild => false;
    }
}
