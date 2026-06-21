using System.Collections.Generic;
using CindarsHope.UI.HUD;

namespace CindarsHope.UI.Onboarding
{
    /// <summary>
    /// fable_62 — tabela estatica e imutavel dos hints de onboarding de primeira ocorrencia.
    ///
    /// Fonte unica de verdade dos ids/textos/prioridades dos hints. Os textos de tecla espelham
    /// docs/game_rules/input_map.md (F67); divergencia = bug de doc (regra: mudou tecla -> atualiza
    /// input_map -> atualiza este catalogo). Classe pura (sem UnityEngine), testavel em EditMode.
    /// NAO e um segundo canal de toast nem duplica o ContextHintController — apenas declara o
    /// conteudo que o <see cref="OnboardingHintService"/> publica no canal WI-23 existente.
    /// </summary>
    public static class OnboardingHintCatalog
    {
        // Ids estaveis (persistidos em save como List<string>). NUNCA renomear sem migration.
        public const string HintMove = "hint_move";
        public const string HintInteract = "hint_interact";
        public const string HintAttack = "hint_attack";
        public const string HintCaveDanger = "hint_cave_danger";
        public const string HintStatus = "hint_status";

        public readonly struct HintEntry
        {
            public readonly string HintId;
            public readonly string Text;
            public readonly FeedbackMessagePriority Priority;
            public readonly float DurationSeconds;

            public HintEntry(string hintId, string text, FeedbackMessagePriority priority, float durationSeconds)
            {
                HintId = hintId;
                Text = text;
                Priority = priority;
                DurationSeconds = durationSeconds;
            }
        }

        private static readonly HintEntry[] Entries =
        {
            new HintEntry(HintMove,
                "WASD ou setas para mover.",
                FeedbackMessagePriority.Normal, 4f),
            new HintEntry(HintInteract,
                "E interage / coleta / usa.",
                FeedbackMessagePriority.Normal, 4f),
            new HintEntry(HintAttack,
                "E ataca (segure para carregar); Q usa a mao esquerda.",
                FeedbackMessagePriority.Normal, 5f),
            new HintEntry(HintCaveDanger,
                "Perigo: morrer aqui derruba seus itens. Space desvia; segure Shift para bloquear.",
                FeedbackMessagePriority.Important, 6f),
            new HintEntry(HintStatus,
                "Status ativos (veneno, sangramento...) aparecem no HUD.",
                FeedbackMessagePriority.Normal, 5f),
        };

        /// <summary>Todas as entradas em ordem estavel (read-only).</summary>
        public static IReadOnlyList<HintEntry> All => Entries;

        /// <summary>Busca uma entrada por id estavel. Retorna false se inexistente.</summary>
        public static bool TryGet(string hintId, out HintEntry entry)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].HintId == hintId)
                {
                    entry = Entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
