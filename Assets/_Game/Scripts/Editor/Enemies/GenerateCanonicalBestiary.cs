using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Enemies
{
    /// <summary>
    /// fable_33 — idempotent generator that materializes the 77 canonical bestiary fichas
    /// (CanonicalBestiaryCatalog) as EnemyDataSO assets, one per enemyId, under
    /// Assets/_Game/Data/Enemies/Canonical. Re-running updates fields in place (no duplicates, no
    /// new GUIDs) so the catalog stays the single source of truth.
    ///
    /// Assets store the BAND-BASE stats (catalog values at the band minimum level); per-level
    /// scaling (+12% HP / +8% DMG, DEF fixed) is applied at spawn by CaveBandScaling, never baked.
    /// VisualScale: size class base × (1.5 miniboss / 2.5 boss) per catalog §2.1 (colliders still
    /// follow the SPEC 13 SizeProfileId, not this visual hint).
    ///
    /// Generation is DEFERRED for fable_33 (per owner authorization, Unity is not run in this
    /// session). Batch entry point: GenerateCanonicalBestiary.RunBatch via
    ///   Unity -batchmode -quit -projectPath . -executeMethod CindarsHope.Editor.Enemies.GenerateCanonicalBestiary.RunBatch
    /// </summary>
    public static class GenerateCanonicalBestiary
    {
        private const string CanonicalFolder = "Assets/_Game/Data/Enemies/Canonical";

        public static void GenerateMenu()
        {
            var (created, updated) = Generate();
            Debug.Log($"[fable_33] Canonical bestiary generation complete. " +
                      $"Created: {created}, Updated: {updated}, Total fichas: {CanonicalBestiaryCatalog.All.Count}.");
        }

        /// <summary>Batchmode entry point (writes the same log; safe to run headless).</summary>
        public static void RunBatch()
        {
            var (created, updated) = Generate();
            Debug.Log($"[fable_33][batch] Canonical bestiary generation complete. " +
                      $"Created: {created}, Updated: {updated}, " +
                      $"Total fichas: {CanonicalBestiaryCatalog.All.Count} " +
                      $"(expected {CanonicalBestiaryCatalogCounts.TotalDistinct}).");
        }

        public static (int created, int updated) Generate()
        {
            EnsureFolder(CanonicalFolder);

            int created = 0;
            int updated = 0;

            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                string assetPath = $"{CanonicalFolder}/{def.EnemyId}.asset";
                var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(assetPath);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<EnemyDataSO>();
                    AssetDatabase.CreateAsset(so, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }

                ApplyFicha(so, def);
                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return (created, updated);
        }

        private static void ApplyFicha(EnemyDataSO so, BestiaryCreatureDef def)
        {
            so.enemyId = def.EnemyId;
            so.DisplayName = def.DisplayName;
            so.BestiaryEntryId = def.EnemyId; // catalog §2.5: BestiaryEntryId == enemy_id
            so.CaveBand = def.Band;
            so.enemyLevel = def.MinLevel;

            // Base stats at the band minimum (catalog). Scaling applied later by CaveBandScaling.
            so.maxHp = Mathf.Max(1, def.Hp);
            so.contactDamage = Mathf.Max(0, def.Damage);
            so.defense = Mathf.Max(0, def.Defense);
            so.xpReward = Mathf.Max(0, def.Xp);

            so.PrimaryRole = def.Role;
            so.MovementProfileId = MovementProfileIdFor(def.MovePrimary);
            so.MoveSecondary = def.MoveSecondary;
            so.SizeProfileId = SizeProfileIdFor(def.Size);

            so.PrimaryDamageTypeId = def.PrimaryDamageTypeId ?? string.Empty;
            so.VulnerabilityMatrixProfileId = def.VulnerabilityMatrixProfileId ?? string.Empty;

            so.dropItemId = def.PrimaryDropItemId ?? string.Empty;
            so.dropAmount = 1;

            so.SpoilerTier = Mathf.Clamp(def.SpoilerTier, 0, 4);
            so.BestiarySize = def.Size;
            so.IsMiniBoss = def.IsMiniBoss;
            so.IsBoss = def.IsBoss;
            so.baseDifficulty = ResolveDifficulty(def);

            so.VisualScale = ResolveVisualScale(def);
            so.Notes = def.Notes ?? string.Empty;
            so.LoreTagline = BuildTagline(def);
            so.FactionId = $"faction_{def.Family.ToLowerInvariant().Replace("-", "_")}";
        }

        // Visual scale: size-class footprint base, then miniboss ×1.5 / boss ×2.5 (catalog §2.1).
        private static float ResolveVisualScale(BestiaryCreatureDef def)
        {
            float baseScale;
            switch (def.Size)
            {
                case BestiarySizeClass.Tiny: baseScale = 0.5f; break;
                case BestiarySizeClass.Small: baseScale = 0.75f; break;
                case BestiarySizeClass.Medium: baseScale = 1f; break;
                case BestiarySizeClass.Large: baseScale = 2f; break;
                case BestiarySizeClass.Huge: baseScale = 3f; break;
                case BestiarySizeClass.Gargantuan: baseScale = 4f; break;
                default: baseScale = 1f; break;
            }

            if (def.IsBoss) return baseScale * 2.5f;
            if (def.IsMiniBoss) return baseScale * 1.5f;
            return baseScale;
        }

        private static EnemyDifficulty ResolveDifficulty(BestiaryCreatureDef def)
        {
            if (def.IsBoss) return EnemyDifficulty.Boss;
            if (def.IsMiniBoss) return EnemyDifficulty.MiniBoss;
            if (def.Role == EnemyRole.Elite) return EnemyDifficulty.Elite;
            if (def.Band >= 6) return EnemyDifficulty.Hard;
            if (def.Band >= 3) return EnemyDifficulty.Normal;
            return EnemyDifficulty.Easy;
        }

        private static string BuildTagline(BestiaryCreatureDef def)
        {
            var tags = new List<string>();
            if (def.IsNocturnal) tags.Add("[NOTURNA]");
            if (def.IsAquatic) tags.Add("[AQUATICA]");
            if (def.IsNonAggressive) tags.Add("[NAO-AGRESSIVA]");
            var prefix = tags.Count > 0 ? string.Join(" ", tags) + " " : string.Empty;
            return $"{prefix}{def.Family} | banda {def.Band} | lvl {def.MinLevel}-{def.MaxLevel}";
        }

        // Map the canonical Move enum to the SPEC 13 movement profile id convention used by the roster.
        private static string MovementProfileIdFor(EnemyMovementType move)
        {
            return $"movement_{ToSnake(move.ToString())}";
        }

        private static string SizeProfileIdFor(BestiarySizeClass size)
        {
            // SPEC 13 size profiles top out at Huge/Boss; map Gargantuan to the boss footprint.
            switch (size)
            {
                case BestiarySizeClass.Gargantuan: return "size_boss";
                default: return $"size_{size.ToString().ToLowerInvariant()}";
            }
        }

        private static string ToSnake(string pascal)
        {
            if (string.IsNullOrEmpty(pascal)) return pascal;
            var chars = new List<char>(pascal.Length + 4);
            for (int i = 0; i < pascal.Length; i++)
            {
                var c = pascal[i];
                if (char.IsUpper(c) && i > 0)
                {
                    chars.Add('_');
                }
                chars.Add(char.ToLowerInvariant(c));
            }
            return new string(chars.ToArray());
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
