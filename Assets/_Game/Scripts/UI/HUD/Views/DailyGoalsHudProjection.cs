using System.Collections.Generic;
using CindarsHope.Farm.Runtime;

namespace CindarsHope.UI.HUD.Views
{
    /// <summary>
    /// fable_65 — projeção read-only das metas diárias para o widget de HUD.
    /// C# puro (sem UnityEngine) → testável em EditMode. Não é fonte de verdade: deriva do
    /// snapshot de FarmDailyGoalService.GetCurrentGoals(). Sem estado persistido próprio.
    /// </summary>
    public sealed class DailyGoalsHudProjection
    {
        /// <summary>Uma linha do widget: rótulo, progresso n/m e flags de check.</summary>
        public readonly struct Row
        {
            public readonly string GoalId;
            public readonly string DisplayName;
            public readonly int Current;
            public readonly int Required;
            public readonly bool Completed;
            public readonly bool Claimed;

            public Row(string goalId, string displayName, int current, int required, bool completed, bool claimed)
            {
                GoalId = goalId;
                DisplayName = displayName;
                Current = current;
                Required = required;
                Completed = completed;
                Claimed = claimed;
            }

            /// <summary>Texto de progresso "n/m" quando RequiredProgress > 1; vazio quando 1.</summary>
            public string ProgressText => Required > 1 ? Current + "/" + Required : string.Empty;

            /// <summary>Marcador de check para metas concluídas.</summary>
            public string CheckMark => Completed ? "[x]" : "[ ]";
        }

        public IReadOnlyList<Row> Rows { get; private set; } = new List<Row>();

        /// <summary>True se o dia não oferece nenhuma meta (empty state — não deve ocorrer, mas não quebra).</summary>
        public bool IsEmpty => Rows.Count == 0;

        /// <summary>Quantas metas estão concluídas (para um cabeçalho compacto "k/total").</summary>
        public int CompletedCount { get; private set; }

        /// <summary>Total de metas oferecidas no dia.</summary>
        public int TotalCount => Rows.Count;

        /// <summary>Reconstrói as linhas a partir do snapshot de metas. Ordem estável (entrada).</summary>
        public void Rebuild(IReadOnlyList<FarmDailyGoalState> goals)
        {
            var rows = new List<Row>();
            int completed = 0;

            if (goals != null)
            {
                foreach (var g in goals)
                {
                    if (g == null)
                    {
                        continue;
                    }

                    var displayName = FarmDailyGoalCatalog.GetDisplayName(g.GoalId);
                    rows.Add(new Row(g.GoalId, displayName, g.CurrentProgress, g.RequiredProgress, g.Completed, g.Claimed));
                    if (g.Completed)
                    {
                        completed++;
                    }
                }
            }

            Rows = rows;
            CompletedCount = completed;
        }
    }
}
