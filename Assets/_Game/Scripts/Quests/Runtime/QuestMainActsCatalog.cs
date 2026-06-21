using System.Collections.Generic;
using CindarsHope.MainProgression;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_36 — stable IDs, milestone flags, fragment mapping and lore records for the main-quest
    /// Acts 2-4 ("O Arco da Memoria", "A Pedra que Sussurra", "A Esperanca Enterrada").
    ///
    /// These materialize QUEST_CATALOG §main (Acts 2-4) as flag-chained quests that REUSE the
    /// fable_10 Act 1 pattern (QuestRegistry definitions + MainProgressionQuestBridge -> Fonte
    /// fragment integration). No parallel main-quest flow is created (quest_rules Rule 1/9).
    ///
    /// Boundary (Out of scope, deferred to fable_43): Act 5 / final choice / Ithryndor /
    /// mq_act4_05_final_choice are NOT authored here. Act 4 ends at "prep da Esperanca": the Hope
    /// fragment is HINTED (lore record), not integrated — integration + the irreversible final
    /// decision belong to the endgame spec.
    ///
    /// Cave-gate adaptation (Phase 0 audit): the canonical boss gates created by
    /// CreateCaveBossAssets exist only at levels 15/30/45/60/75/90 with boss id
    /// "enemy_meteor_ooze_king" (there is no Rimelock/Draconic boss asset, no gate at 70 or 100).
    /// Per the fable_10 precedent and the spec risk-mitigation, act boss objectives use the
    /// supported ReachCaveDepth + DefeatEnemy "any" runtime handlers against the act's design depth,
    /// clamped to the deepest real content (90). The mapping is documented in the execution report.
    /// </summary>
    public static class QuestMainActsIds
    {
        // ─── Act 2 — "O Arco da Memoria" (QuestLevel 35; gate 30; Fragmento da MEMORIA) ──────────
        public const string Act2Quest01RecordsOfSilver = "mq_act2_01_records_of_silver";
        public const string Act2Quest02TheVeiledVisitor = "mq_act2_02_the_veiled_visitor"; // Vaelrion chega
        public const string Act2Quest03SongBelow = "mq_act2_03_song_below";                // Liora
        public const string Act2Quest04GateOfFrost = "mq_act2_04_gate_of_frost";           // gate 30
        public const string Act2Quest05FragmentOfMemory = "mq_act2_05_fragment_of_memory"; // -> respec

        // ─── Act 3 — "A Pedra que Sussurra" (QuestLevel 65; gate 70; Fragmento da VIDA) ──────────
        public const string Act3Quest01BlackstoneLedger = "mq_act3_01_blackstone_ledger";
        public const string Act3Quest02ThrallMercy = "mq_act3_02_thrall_mercy";
        public const string Act3Quest03TheQuietPriest = "mq_act3_03_the_quiet_priest";
        public const string Act3Quest04WardenOfSilence = "mq_act3_04_warden_of_silence";   // Nymirian chega
        public const string Act3Quest05FragmentOfLife = "mq_act3_05_fragment_of_life";     // gate 70 -> purificacao

        // ─── Act 4 — "A Esperanca Enterrada" (QuestLevel 90; gate 100; prep da ESPERANCA) ───────
        public const string Act4Quest01LitanyComplete = "mq_act4_01_litany_complete";
        public const string Act4Quest02TheJailer = "mq_act4_02_the_jailer";                // gate 100 (clamped 90)
        public const string Act4Quest03VelKaraum = "mq_act4_03_vel_karaum";
        public const string Act4Quest04BrokenRemembrance = "mq_act4_04_broken_remembrance";
        // fable_43 — Act 5 endgame finale (canonical catalog id). Authored in QuestRegistry.Endgame.cs.
        public const string Act4Quest05FinalChoice = "mq_act4_05_final_choice";

        // ─── NPC givers / actors ────────────────────────────────────────────────────────────────
        public const string CorvusId = QuestMainAct1Ids.CorvusId;     // npc_corvus
        public const string ThalindraId = QuestRuntimeIds.ThalindraId; // npc_thalindra
        public const string MaelorId = QuestRuntimeIds.MaelorId;       // npc_maelor
        public const string LioraId = "npc_liora";
        public const string VaelrionId = "npc_vaelrion";   // chega no Ato 2 apos o 1o boss
        public const string NymirianId = "npc_nymirian";   // Warden of Silence, conversavel no Ato 3

        // ─── Milestone flags (act-done; read by side chains F35/F70 and dialogue F28/services F25) ─
        // act_N_done: read by NpcQuestChainCatalog (ActOneDoneFlag/ActTwoDoneFlag/ActThreeDoneFlag).
        public const string FlagAct1Done = "act_1_done";
        public const string FlagAct2Done = "act_2_done";
        public const string FlagAct3Done = "act_3_done";
        public const string FlagAct4Done = "act_4_done";
        public const string FlagAct5Done = "act_5_done"; // fable_43 — endgame done milestone

        // flag_mq_actN_complete: internal act-complete milestone (chains the next act's offer).
        public const string FlagAct2Complete = "flag_mq_act2_complete";
        public const string FlagAct3Complete = "flag_mq_act3_complete";
        public const string FlagAct4Complete = "flag_mq_act4_complete";
        public const string FlagAct5Complete = "flag_mq_act5_complete"; // fable_43 — endgame chain-complete

        // flag_main_post_actN: read by TownNpcDialogueLibrary (F28) for post-act greeting lines.
        // (post_act1 already granted by Act 1; post_act3 is consumed by the dialogue library today.)
        public const string FlagMainPostAct2 = "flag_main_post_act2";
        public const string FlagMainPostAct3 = "flag_main_post_act3";
        public const string FlagMainPostAct4 = "flag_main_post_act4";

        // Nymirian becomes conversable from Act 3: gated on this flag (set on act3_04 turn-in).
        public const string FlagNymirianAvailable = "flag_nymirian_available";

        // ─── Lore records (gradual reveal of the Black Stone; ActCompletedEvent carries the id) ───
        // Derived from flags (not persisted as text) — the quest-detail UI shows the record text.
        public const string LoreRecordAct2 = "lore_main_act2_memory";
        public const string LoreRecordAct3 = "lore_main_act3_life";
        public const string LoreRecordAct4 = "lore_main_act4_hope";
        public const string LoreRecordAct5 = "lore_main_act5_final_choice"; // fable_43 — epilogue lore

        /// <summary>
        /// The act-final quest of each act -> (act number, fragment, act-done flag, lore record).
        /// Act 1's final quest (mq_act1_05) maps to Water, reusing the same milestone shape.
        /// Act 4 maps to Hope but is HINTED only (the bridge does not integrate Hope — fable_43).
        /// </summary>
        public sealed class ActFinale
        {
            public int ActNumber;
            public string FinalQuestId;
            public MainFragmentType Fragment;
            public bool IntegratesFragment; // false for Act 4 (Hope prep -> fable_43)
            public string ActDoneFlagId;
            public string LoreRecordId;
            public string ActIdForSkillPoint; // stable id recorded in RewardedMainActIds
        }

        public static IReadOnlyList<ActFinale> Finales { get; } = new List<ActFinale>
        {
            new ActFinale
            {
                ActNumber = 1,
                FinalQuestId = QuestMainAct1Ids.Quest05FragmentoDaAgua,
                Fragment = MainFragmentType.Water,
                IntegratesFragment = true,
                ActDoneFlagId = FlagAct1Done,
                LoreRecordId = "lore_main_act1_water",
                ActIdForSkillPoint = "act_1_water"
            },
            new ActFinale
            {
                ActNumber = 2,
                FinalQuestId = Act2Quest05FragmentOfMemory,
                Fragment = MainFragmentType.Memory,
                IntegratesFragment = true,
                ActDoneFlagId = FlagAct2Done,
                LoreRecordId = LoreRecordAct2,
                ActIdForSkillPoint = "act_2_memory"
            },
            new ActFinale
            {
                ActNumber = 3,
                FinalQuestId = Act3Quest05FragmentOfLife,
                Fragment = MainFragmentType.Life,
                IntegratesFragment = true,
                ActDoneFlagId = FlagAct3Done,
                LoreRecordId = LoreRecordAct3,
                ActIdForSkillPoint = "act_3_life"
            },
            new ActFinale
            {
                ActNumber = 4,
                FinalQuestId = Act4Quest04BrokenRemembrance,
                Fragment = MainFragmentType.Hope,
                IntegratesFragment = false, // Hope is hinted; integration + final choice -> fable_43
                ActDoneFlagId = FlagAct4Done,
                LoreRecordId = LoreRecordAct4,
                ActIdForSkillPoint = "act_4_hope"
            },
            // fable_43 — Act 5 (endgame): completing mq_act4_05_final_choice INTEGRATES the Hope
            // fragment into the Fonte (prerequisite of the final choice, fonte_rules Rule 7) and grants
            // the final act's +1 skill point exactly once. The irreversible Protect/Seal/Use decision
            // itself is executed at the Fonte by FinalChoiceRuntimeAdapter, AFTER this integration.
            new ActFinale
            {
                ActNumber = 5,
                FinalQuestId = Act4Quest05FinalChoice,
                Fragment = MainFragmentType.Hope,
                IntegratesFragment = true,
                ActDoneFlagId = FlagAct5Done,
                LoreRecordId = LoreRecordAct5,
                ActIdForSkillPoint = "act_5_hope"
            }
        };

        /// <summary>Returns the ActFinale whose FinalQuestId matches, or null.</summary>
        public static ActFinale FinaleForQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return null;
            foreach (var f in Finales)
                if (f.FinalQuestId == questId) return f;
            return null;
        }
    }
}
