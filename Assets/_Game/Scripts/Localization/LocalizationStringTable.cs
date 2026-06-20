using System.Collections.Generic;

namespace CindarsHope.Localization
{
    /// <summary>
    /// Lightweight id -> string table for quest/dialogue text authored from phase P4 onward
    /// (ADR-0012 — Localization via String Table from Phase P4). PT-BR is the single authoring
    /// language populated in v1; a second language can later be added by populating this table,
    /// not by rewriting call sites.
    ///
    /// Pure data, no framework, no runtime locale switch, no .po/.resx, no Unity references and
    /// no save persistence (the table is a data resource, not game state). The static/lazy/TryGet
    /// shape intentionally mirrors <see cref="CindarsHope.NPC.NpcDialogueSetRegistry"/> /
    /// TownNpcDialogueLibrary instead of introducing a new catalogue pattern.
    ///
    /// IMPORTANT (ADR-0012 "Registered Debt"): pre-P4 hardcoded text (TownNpcDialogueLibrary,
    /// DialogueNode.Text, DialogueTreeSO Nodes[].Text) is NOT migrated here. Only NEW quest/dialogue
    /// text from P4 onward declares keys in this table.
    ///
    /// Id convention (PROPOSAL to calibrate with F67 — fable_67 governance; lookup is agnostic to
    /// the id format, so this convention does not gate resolution):
    ///   {domain}.{spec_or_npc}.{slot}    e.g. "quest.first_supplies.title",
    ///                                          "quest.first_supplies.desc",
    ///                                          "dialogue.pip.greeting"
    ///   - domain prefix: "quest." or "dialogue." (lowercase)
    ///   - dot separator, snake_case segments, no spaces.
    /// A future text spec (F35/F36/F70) DECLARES its keys by adding entries to <see cref="SeedEntries"/>
    /// (or a partial of this class) using this convention.
    /// </summary>
    public static class LocalizationStringTable
    {
        /// <summary>
        /// Stable id -> PT-BR string entry. Simple types only (no Unity references), so the entry
        /// can implement <see cref="CindarsHope.Core.Data.IIdentifiedData"/> for id-keyed reuse.
        /// </summary>
        public sealed class LocalizationEntry : CindarsHope.Core.Data.IIdentifiedData
        {
            private readonly string _id;
            private readonly string _value;

            public LocalizationEntry(string id, string value)
            {
                _id = id;
                _value = value;
            }

            public string Id => _id;
            public string Value => _value;
        }

        // Default authoring language. Single locale in v1 (ADR-0012). No runtime locale switch.
        public const string DefaultLanguage = "pt-BR";

        private static Dictionary<string, string> s_entries;

        private static Dictionary<string, string> Entries
        {
            get
            {
                if (s_entries == null)
                {
                    s_entries = new Dictionary<string, string>();
                    foreach (var entry in SeedEntries())
                    {
                        if (entry == null || string.IsNullOrEmpty(entry.Id))
                        {
                            continue;
                        }

                        // Last writer wins; deterministic given a fixed seed order.
                        s_entries[entry.Id] = entry.Value;
                    }
                }

                return s_entries;
            }
        }

        /// <summary>
        /// Seed entries for the convention. v1 ships only a tiny set of demonstration keys so the
        /// CONTRACT and discipline are exercised; production text is populated by the consumer specs
        /// (F35/F36/F70). These demo keys document the id convention in code.
        /// </summary>
        private static IEnumerable<LocalizationEntry> SeedEntries()
        {
            // Convention demonstration keys (NOT production quest content — that comes from F35/F36/F70).
            yield return new LocalizationEntry("quest.sample.title", "Missao de Exemplo");
            yield return new LocalizationEntry("quest.sample.desc", "Descricao de exemplo da missao.");
            yield return new LocalizationEntry("dialogue.sample.greeting", "Ola, viajante.");
            // fable_26: rotulo da linha de cabecalho do Conversar ("Amizade: nivel N"). Texto novo
            // voltado ao jogador declarado aqui via LocalizationService (ADR-0012 / decisao 4.7).
            yield return new LocalizationEntry("ui.friendship.level_label", "Amizade: nivel");
            // fable_57: textos novos voltados ao jogador (toast de aniversario + label da diaria da
            // estalagem) declarados aqui via LocalizationService (ADR-0012). {0} = nome do NPC.
            yield return new LocalizationEntry("ui.birthday.gift_toast", "Hoje e aniversario de {0}!");
            yield return new LocalizationEntry("ui.inn.sleep_prompt", "Dormir na estalagem");
        }

        /// <summary>Number of resolvable keys currently in the table.</summary>
        public static int Count => Entries.Count;

        /// <summary>True when the table has an entry for <paramref name="id"/>.</summary>
        public static bool Contains(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            return Entries.ContainsKey(id);
        }

        /// <summary>
        /// Looks up the PT-BR string for <paramref name="id"/>. Returns true and the stored value
        /// when present; false otherwise (value is left as null for the caller to apply its fallback).
        /// </summary>
        public static bool TryGetValue(string id, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            return Entries.TryGetValue(id, out value);
        }

        /// <summary>
        /// EditMode-only helper: replace the active table with an explicit map (no scene, no asset).
        /// Pass null to reset back to the seeded table on next access. Kept minimal and side-effect
        /// free for deterministic tests (mirrors the lazy-init reset idiom).
        /// </summary>
        public static void OverrideForTests(IDictionary<string, string> entries)
        {
            if (entries == null)
            {
                s_entries = null;
                return;
            }

            var replacement = new Dictionary<string, string>();
            foreach (var pair in entries)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    continue;
                }

                replacement[pair.Key] = pair.Value;
            }

            s_entries = replacement;
        }
    }
}
