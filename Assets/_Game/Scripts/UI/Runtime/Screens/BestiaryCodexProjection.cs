using System.Collections.Generic;
using CindarsHope.Bestiary;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Enemy;

namespace CindarsHope.UI.Runtime.Screens
{
    /// <summary>
    /// fable_45 — pure, deterministic projection that turns the canonical creature catalog
    /// (<see cref="CanonicalBestiaryCatalog"/>, F33) + the discovery-knowledge service
    /// (<see cref="EnemyKnowledgeService"/>, F21 — the SINGLE source of visibility) into the
    /// codex list + per-creature ficha model the <see cref="BestiaryScreenView"/> renders.
    ///
    /// No UnityEngine dependency: every grouping/ordering/hiding/completeness/narration rule is
    /// EditMode-testable here, so the view never makes a visibility decision and can never leak a
    /// spoiler (riscos técnicos §1). Regras de não duplicação: this reads F33 data and asks F21
    /// IsVisible — it never re-implements either.
    ///
    /// Spoiler rule (§20): an entry whose SpoilerTier is "locked" (tier 3+ without its quest flag)
    /// is NOT present in the list at all — not even as a silhouette — and is excluded from the
    /// completeness denominator (anti-spoiler: o denominador é o número de entradas visíveis).
    /// EMENDA-D: a boss ficha only becomes visible after defeat (the F21 reveal-on-kill path);
    /// here it surfaces naturally because its categories are unlocked + its tier flag is set.
    /// </summary>
    public static class BestiaryCodexProjection
    {
        /// <summary>The four core categories whose unlock defines a "documented" (K4) ficha (F21 GetLevel).</summary>
        public static readonly BestiaryKnowledgeCategory[] CoreCategories =
        {
            BestiaryKnowledgeCategory.Identity,
            BestiaryKnowledgeCategory.BehaviorSummary,
            BestiaryKnowledgeCategory.ElementVulnerability,
            BestiaryKnowledgeCategory.DropsCommon,
        };

        /// <summary>Stable silhouette label for an undiscovered creature (anti-spoiler).</summary>
        public const string UnknownLabel = "???";

        /// <summary>Canonical empty-state message (§ empty state) when nothing has been observed yet.</summary>
        public const string EmptyStateMessage = "Nenhuma criatura observada ainda.";

        /// <summary>v1 filters (§15.2 subset): All / Seen / Defeated / per-family (the family is carried in FamilyFilter).</summary>
        public enum CodexFilter
        {
            All,
            Seen,
            Defeated,
            Family,
        }

        /// <summary>
        /// Build the full codex model. <paramref name="entries"/> is the catalog (defaults to
        /// <see cref="CanonicalBestiaryCatalog.All"/>); <paramref name="knowledge"/> is the F21
        /// service (a null service yields an all-undiscovered, all-tier-0-visible codex for tests).
        /// <paramref name="familyFilter"/> is honoured only when <paramref name="filter"/> == Family.
        /// </summary>
        public static BestiaryCodexModel Build(
            IReadOnlyList<BestiaryCreatureDef> entries,
            EnemyKnowledgeService knowledge,
            CodexFilter filter = CodexFilter.All,
            string familyFilter = null)
        {
            var model = new BestiaryCodexModel
            {
                Filter = filter,
                FamilyFilter = familyFilter ?? string.Empty,
                IsEmpty = true
            };
            if (entries == null)
            {
                return model;
            }

            // Stable ordering: band asc, then family (ordinal), then min-level, then id — deterministic.
            var ordered = new List<BestiaryCreatureDef>(entries);
            ordered.Sort(CompareStable);

            var seenFamilies = new SortedSet<string>(System.StringComparer.Ordinal);
            int visibleTotal = 0;
            int seenTotal = 0;
            int documentedTotal = 0;

            BestiaryCodexGroup currentGroup = null;

            foreach (var def in ordered)
            {
                if (string.IsNullOrWhiteSpace(def.EnemyId))
                {
                    continue;
                }

                // §20: an entry whose spoiler tier is locked is absent from the list entirely.
                if (IsTierLocked(def, knowledge))
                {
                    continue;
                }

                visibleTotal++;
                if (!string.IsNullOrWhiteSpace(def.Family))
                {
                    seenFamilies.Add(def.Family);
                }

                bool discovered = IsDiscovered(def, knowledge);
                bool documented = IsDocumented(def, knowledge);
                if (discovered) seenTotal++;
                if (documented) documentedTotal++;

                if (!PassesFilter(def, filter, familyFilter, discovered, documented))
                {
                    continue;
                }

                if (currentGroup == null || currentGroup.Band != def.Band || currentGroup.Family != (def.Family ?? string.Empty))
                {
                    currentGroup = new BestiaryCodexGroup
                    {
                        Band = def.Band,
                        BandTheme = BandTheme(def.Band),
                        Family = def.Family ?? string.Empty,
                    };
                    model.Groups.Add(currentGroup);
                }

                currentGroup.Entries.Add(new BestiaryCodexListItem
                {
                    EnemyId = def.EnemyId,
                    DisplayName = discovered ? def.DisplayName : UnknownLabel,
                    IsDiscovered = discovered,
                    IsDocumented = documented,
                    IsSilhouette = !discovered,
                    Band = def.Band,
                    Family = def.Family ?? string.Empty,
                });
            }

            model.SeenCount = seenTotal;
            model.DocumentedCount = documentedTotal;
            model.VisibleTotal = visibleTotal;
            model.Families = new List<string>(seenFamilies);
            model.IsEmpty = seenTotal == 0;
            model.CompletenessLabel = $"Vistos {seenTotal}/{visibleTotal} · Documentados {documentedTotal}/{visibleTotal}";
            return model;
        }

        /// <summary>
        /// Build the per-creature ficha. Every line obeys F21 IsVisible: a locked category renders
        /// the silhouette label "???", never real content. Narration appears only with Identity
        /// discovered. Returns null when the creature is tier-locked (must not even be addressable).
        /// </summary>
        public static BestiaryFichaModel BuildFicha(BestiaryCreatureDef def, EnemyKnowledgeService knowledge)
        {
            if (string.IsNullOrWhiteSpace(def.EnemyId) || IsTierLocked(def, knowledge))
            {
                return null;
            }

            bool identity = IsVisible(def, BestiaryKnowledgeCategory.Identity, knowledge);
            var ficha = new BestiaryFichaModel
            {
                EnemyId = def.EnemyId,
                IsIdentityKnown = identity,
                DisplayName = identity ? def.DisplayName : UnknownLabel,
                BandTheme = BandTheme(def.Band),
                Band = def.Band,
                IsDocumented = IsDocumented(def, knowledge),
            };

            // Identity line (family / habitat / level band) — only with Identity unlocked.
            ficha.IdentityLine = identity
                ? $"{def.Family} · banda {BandTheme(def.Band)} · nivel {def.MinLevel}-{def.MaxLevel}"
                : UnknownLabel;

            ficha.BehaviorLine = IsVisible(def, BestiaryKnowledgeCategory.BehaviorSummary, knowledge)
                ? BehaviorSummary(def)
                : UnknownLabel;

            ficha.VulnerabilityLine = IsVisible(def, BestiaryKnowledgeCategory.ElementVulnerability, knowledge)
                ? $"Vulneravel a {def.PrimaryDamageTypeId}"
                : UnknownLabel;

            ficha.ResistanceLine = IsVisible(def, BestiaryKnowledgeCategory.ResistanceTags, knowledge)
                ? ResistanceSummary(def)
                : UnknownLabel;

            ficha.DropsLine = IsVisible(def, BestiaryKnowledgeCategory.DropsCommon, knowledge)
                ? def.PrimaryDropItemId
                : UnknownLabel;

            // Defeat count comes from the knowledge service (kills), shown once the creature is seen.
            var state = knowledge?.GetState(def.EnemyId);
            ficha.DefeatCount = state?.Kills ?? 0;
            ficha.DefeatCountLine = identity ? $"Derrotas: {ficha.DefeatCount}" : UnknownLabel;

            // Narration only with identity discovered (CA-3).
            if (identity)
            {
                string narration = BestiaryNarrationTable.GetNarration(def.EnemyId);
                ficha.NarrationText = !string.IsNullOrWhiteSpace(narration)
                    ? narration
                    : def.Notes ?? string.Empty;
            }
            else
            {
                ficha.NarrationText = string.Empty;
            }

            // EMENDA-D: documented ficha shows the mechanical bonus.
            ficha.DocumentedBonusLine = ficha.IsDocumented
                ? $"Documentado: +{(int)(EnemyKnowledgeService.FullyDocumentedDamageBonus * 100)}% de dano contra esta criatura."
                : string.Empty;

            return ficha;
        }

        // ── Visibility / discovery helpers (all delegate to F21) ─────────────────────────────────

        private static bool IsVisible(BestiaryCreatureDef def, BestiaryKnowledgeCategory category, EnemyKnowledgeService knowledge)
        {
            // No service (test/default): tier-0 commons are visible once "unlocked"; with no service
            // there is no unlock state, so nothing is visible — callers pass a service for real data.
            return knowledge != null && knowledge.IsVisible(def.EnemyId, category, def.SpoilerTier);
        }

        private static bool IsDiscovered(BestiaryCreatureDef def, EnemyKnowledgeService knowledge)
        {
            // "Discovered" for the list = Identity visible (you have seen + can name the creature).
            return IsVisible(def, BestiaryKnowledgeCategory.Identity, knowledge);
        }

        private static bool IsDocumented(BestiaryCreatureDef def, EnemyKnowledgeService knowledge)
        {
            if (knowledge == null)
            {
                return false;
            }

            foreach (var category in CoreCategories)
            {
                if (!knowledge.IsVisible(def.EnemyId, category, def.SpoilerTier))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True when the creature must be ABSENT from the codex (not even a silhouette) because of
        /// its spoiler tier (§20). Tiers 0-2 are never tier-locked. Tier 3+ is gated behind a quest
        /// flag (F21): the entry only surfaces once the player has both DISCOVERED it (Identity
        /// unlocked) AND the gating flag is set — i.e. once F21 reports the Identity category visible.
        /// Until then it is hidden, so a gate boss / the Four are never teased before the gate.
        /// A null service (test default) conservatively hides every tier 3+ entry (fail-safe).
        /// </summary>
        private static bool IsTierLocked(BestiaryCreatureDef def, EnemyKnowledgeService knowledge)
        {
            if (def.SpoilerTier < 3)
            {
                return false;
            }

            if (knowledge == null)
            {
                return true;
            }

            // Visible Identity ⇒ discovered AND flag set ⇒ allowed in the list. Anything else
            // (undiscovered, or unlocked-but-flag-not-set) stays hidden.
            return !knowledge.IsVisible(def.EnemyId, BestiaryKnowledgeCategory.Identity, def.SpoilerTier);
        }

        // ── Filters (deterministic) ──────────────────────────────────────────────────────────────

        private static bool PassesFilter(
            BestiaryCreatureDef def, CodexFilter filter, string familyFilter, bool discovered, bool documented)
        {
            switch (filter)
            {
                case CodexFilter.Seen:
                    return discovered;
                case CodexFilter.Defeated:
                    return documented;
                case CodexFilter.Family:
                    return !string.IsNullOrWhiteSpace(familyFilter)
                        && string.Equals(def.Family, familyFilter, System.StringComparison.Ordinal);
                case CodexFilter.All:
                default:
                    return true;
            }
        }

        // ── Presentation helpers ─────────────────────────────────────────────────────────────────

        private static int CompareStable(BestiaryCreatureDef a, BestiaryCreatureDef b)
        {
            int byBand = a.Band.CompareTo(b.Band);
            if (byBand != 0) return byBand;

            int byFamily = string.CompareOrdinal(a.Family ?? string.Empty, b.Family ?? string.Empty);
            if (byFamily != 0) return byFamily;

            int byLevel = a.MinLevel.CompareTo(b.MinLevel);
            if (byLevel != 0) return byLevel;

            return string.CompareOrdinal(a.EnemyId ?? string.Empty, b.EnemyId ?? string.Empty);
        }

        private static string BandTheme(int band)
        {
            foreach (var info in CanonicalBestiaryCatalog.Bands)
            {
                if (info.Band == band)
                {
                    return info.Theme;
                }
            }

            return band >= 8 ? "finale" : "?";
        }

        private static string BehaviorSummary(BestiaryCreatureDef def)
        {
            string role = def.Role.ToString();
            return string.IsNullOrWhiteSpace(def.Notes) ? role : $"{role} — {def.Notes}";
        }

        private static string ResistanceSummary(BestiaryCreatureDef def)
        {
            return string.IsNullOrWhiteSpace(def.VulnerabilityMatrixProfileId)
                ? "Resistencias documentadas."
                : $"Perfil: {def.VulnerabilityMatrixProfileId}";
        }
    }

    /// <summary>fable_45 — the codex list model (pure data; the view renders it 1:1).</summary>
    public sealed class BestiaryCodexModel
    {
        public List<BestiaryCodexGroup> Groups { get; } = new List<BestiaryCodexGroup>();
        public List<string> Families { get; set; } = new List<string>();
        public int SeenCount { get; set; }
        public int DocumentedCount { get; set; }
        public int VisibleTotal { get; set; }
        public bool IsEmpty { get; set; }
        public string CompletenessLabel { get; set; } = string.Empty;
        public BestiaryCodexProjection.CodexFilter Filter { get; set; }
        public string FamilyFilter { get; set; } = string.Empty;

        public string EmptyMessage => BestiaryCodexProjection.EmptyStateMessage;
    }

    /// <summary>fable_45 — one band/family group header + its entries.</summary>
    public sealed class BestiaryCodexGroup
    {
        public int Band { get; set; }
        public string BandTheme { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public List<BestiaryCodexListItem> Entries { get; } = new List<BestiaryCodexListItem>();

        public string Header => $"Banda {Band} ({BandTheme}) · {Family}";
    }

    /// <summary>fable_45 — one row in the codex list.</summary>
    public sealed class BestiaryCodexListItem
    {
        public string EnemyId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsDiscovered { get; set; }
        public bool IsDocumented { get; set; }
        public bool IsSilhouette { get; set; }
        public int Band { get; set; }
        public string Family { get; set; } = string.Empty;
    }

    /// <summary>fable_45 — the per-creature ficha model. Locked lines carry "???" (never real data).</summary>
    public sealed class BestiaryFichaModel
    {
        public string EnemyId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsIdentityKnown { get; set; }
        public bool IsDocumented { get; set; }
        public int Band { get; set; }
        public string BandTheme { get; set; } = string.Empty;

        public string IdentityLine { get; set; } = string.Empty;
        public string BehaviorLine { get; set; } = string.Empty;
        public string VulnerabilityLine { get; set; } = string.Empty;
        public string ResistanceLine { get; set; } = string.Empty;
        public string DropsLine { get; set; } = string.Empty;
        public int DefeatCount { get; set; }
        public string DefeatCountLine { get; set; } = string.Empty;
        public string NarrationText { get; set; } = string.Empty;
        public string DocumentedBonusLine { get; set; } = string.Empty;
    }
}
