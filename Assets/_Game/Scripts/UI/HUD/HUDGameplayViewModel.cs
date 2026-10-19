using System.Collections.Generic;
using CindarsHope.UI.Notifications;

namespace CindarsHope.UI.HUD
{
    // Main HUD projection — read-only, never source of truth for gameplay state
    public class GameplayHudViewModel
    {
        // Core always-visible stats
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }

        // MP shown only when magic/build relevant
        public bool ShowMp { get; set; }
        public int Mp { get; set; }
        public int MaxMp { get; set; }

        // Compact secondary needs (shown when contextually relevant)
        public float HungerCompact { get; set; }
        public float FatigueCompact { get; set; }

        // World state projection
        public string CurrentSeason { get; set; }
        public int DayOfSeason { get; set; }
        public string CurrentWeather { get; set; }
        public string CurrentLunarPhase { get; set; }

        // Hotbar
        public List<HotbarSlotViewModel> HotbarSlots { get; set; } = new List<HotbarSlotViewModel>();

        // Active skill slots — CAPPED AT 4 (spec rule 23H)
        public List<ActiveSkillSlotViewModel> ActiveSkillSlots { get; set; } = new List<ActiveSkillSlotViewModel>();

        // Active tool or weapon display
        public string ActiveToolOrWeapon { get; set; }

        // Status effects and buffs
        public List<StatusBuffProjection> StatusEffects { get; set; } = new List<StatusBuffProjection>();
        public List<StatusBuffProjection> Buffs { get; set; } = new List<StatusBuffProjection>();

        // Context prompts
        public ContextPromptProjection ContextPrompt { get; set; }
        public string QuestPrompt { get; set; }

        // Future slots — optional, no runtime until companion/pet specs
        public string CompanionProjection { get; set; }
        public string PetProjection { get; set; }

        // Debug: always false in final HUD
        public bool DebugVisible { get; set; } = false;

        // Computed helpers
        public float HpPercent => MaxHp > 0 ? (float)Hp / MaxHp : 1f;
        public float MpPercent => MaxMp > 0 ? (float)Mp / MaxMp : 1f;
        public float StaminaPercent => MaxStamina > 0 ? (float)Stamina / MaxStamina : 1f;
        public bool IsHpLow => HpPercent <= 0.25f;
        public bool IsMpLow => ShowMp && MpPercent <= 0.25f;
        public bool IsStaminaLow => StaminaPercent <= 0.25f;
    }

    // Legacy alias — keeps backward compat for any compiled code referencing old name
    public class HUDGameplayViewModel : GameplayHudViewModel { }
}
