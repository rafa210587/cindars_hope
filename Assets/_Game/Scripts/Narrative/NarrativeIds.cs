namespace CindarsHope.Narrative
{
    /// <summary>
    /// fable_63 — IDs estaveis da intro/ponte narrativa. NUNCA renomear (persistidos como
    /// flags na familia de hints do QuestStateSection e/ou consumidos por save legado).
    /// </summary>
    public static class NarrativeIds
    {
        // ─── Quest-ponte ────────────────────────────────────────────────────────────
        /// <summary>Quest-ponte auto-ofertada no 1o DayStarted: "Procure Corvus na cidade".</summary>
        public const string MainQuestHookId = "mq_act1_00";

        /// <summary>NPC alvo da quest-ponte (mesmo id usado pela cadeia E40).</summary>
        public const string CorvusNpcId = "npc_corvus";

        /// <summary>Objective id (TalkToNpc Corvus) da quest-ponte.</summary>
        public const string HookTalkObjectiveId = "obj_mq_act1_00_talk_corvus";

        /// <summary>Flag de conclusao da quest-ponte (recompensa = flag; sem ouro/XP significativo).</summary>
        public const string FlagHookComplete = "flag_mq_act1_00_complete";

        // ─── Flags persistidas (familia GlobalKnownHints do QuestStateSection) ──────────
        /// <summary>Marca que a sequencia de intro ja foi exibida neste save (1x/save).</summary>
        public const string FlagIntroSeen = "narrative_intro_seen";

        /// <summary>Marca que a quest-ponte ja foi auto-ofertada (idempotencia da oferta).</summary>
        public const string FlagHookOffered = "narrative_mq_act1_00_offered";

        /// <summary>Marca que a carta na cama ja foi lida (estado da carta persistido).</summary>
        public const string FlagLetterRead = "narrative_letter_read";
    }
}
