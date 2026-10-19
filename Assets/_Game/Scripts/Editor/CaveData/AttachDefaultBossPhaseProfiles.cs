using System.Collections.Generic;
using CindarsHope.Cave.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.CaveData
{
    /// <summary>
    /// fable_05 — editor generator that authors default <see cref="BossPhaseProfileSO"/> assets for the
    /// bosses produced by <see cref="CreateCaveBossAssets"/> and wires them into a
    /// <see cref="BossPhaseProfileRegistrySO"/>. The CaveBossSpawner reads the registry at runtime:
    /// a boss WITH a profile gets the EnemyBrain + BossBrainController (3 phases, telegraphed
    /// transitions, vulnerability windows, idempotent adds); a boss WITHOUT one keeps the simple
    /// chase behaviour (anti-regression).
    ///
    /// EMENDA 2026-06-12: concrete per-boss phases for the catalog ★ bosses (Mite Queen, Fungal
    /// Patriarch, Cindershard Wyrm, Draconic Elder, ...) belong to the bestiary catalog content
    /// (fable_33) and are authored here as those enemy/action-set assets land. Cindershard Wyrm and
    /// Draconic Elder use the flight phase: set the phase's MovementProfileId to a FloatingOrbit
    /// movement profile and BossBrainController swaps the Move for that phase. The default profile
    /// below targets the currently-generated boss (enemy_meteor_ooze_king) and documents the pattern.
    /// </summary>
    public static class AttachDefaultBossPhaseProfiles
    {
        private const string ProfilesPath = "Assets/_Game/Data/Cave/BossPhases";
        private const string RegistryPath = "Assets/_Game/Data/Cave/BossPhaseProfileRegistry.asset";

        // The boss CreateCaveBossAssets currently generates. Additional catalog bosses are added to this
        // table as their EnemyDataSO + action-set assets exist (fable_33).
        private const string MeteorOozeKingId = "enemy_meteor_ooze_king";

        public static void AttachAll()
        {
            EnsureDirectory(ProfilesPath);

            var profiles = new List<BossPhaseProfileSO>
            {
                EnsureMeteorOozeKingProfile()
            };

            WireRegistry(profiles);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AttachDefaultBossPhaseProfiles] {profiles.Count} boss phase profile(s) authored and wired. Regenerate CaveScene / re-run boss spawn to activate.");
        }

        private static BossPhaseProfileSO EnsureMeteorOozeKingProfile()
        {
            var profileId = BossPhaseProfileRegistrySO.BuildProfileId(MeteorOozeKingId);
            var path = $"{ProfilesPath}/{profileId}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BossPhaseProfileSO>(path);

            BossPhaseProfileSO asset;
            if (existing != null)
            {
                asset = existing;
            }
            else
            {
                asset = ScriptableObject.CreateInstance<BossPhaseProfileSO>();
                AssetDatabase.CreateAsset(asset, path);
                Debug.Log($"[AttachDefaultBossPhaseProfiles] Created {path}.");
            }

            asset.ProfileId = profileId;
            asset.BossEnemyId = MeteorOozeKingId;
            asset.Notes = "fable_05 default 3-phase profile for the generated cave boss. Phase 3 summons adds. " +
                          "Catalog ★ bosses get bespoke profiles (EMENDA); flight-phase bosses set a FloatingOrbit MovementProfileId.";

            // 3 phases at 100% / 66% / 33% HP. ActionSetIds reference existing action sets (the brain
            // no-ops gracefully if an id is ever missing). The final phase opens a longer window and
            // summons 2 cave mite adds ONCE (idempotent + deterministic positions, CA-3).
            asset.Phases = new[]
            {
                new BossPhase
                {
                    HpThresholdPercent = 100f,
                    ActionSetId = "actionset_enemy_stone_rat",
                    MovementProfileId = string.Empty, // keep base movement
                    MoveSpeedMultiplier = 1f,
                    DamageMultiplier = 1f,
                    AddsEnemyId = string.Empty,
                    AddsCount = 0,
                    VulnerabilityWindowSeconds = 0f // no window on the opening phase
                },
                new BossPhase
                {
                    HpThresholdPercent = 66f,
                    ActionSetId = "actionset_enemy_cracked_bone",
                    MovementProfileId = string.Empty,
                    MoveSpeedMultiplier = 1.15f,
                    DamageMultiplier = 1.15f,
                    AddsEnemyId = string.Empty,
                    AddsCount = 0,
                    VulnerabilityWindowSeconds = 1.5f
                },
                new BossPhase
                {
                    HpThresholdPercent = 33f,
                    ActionSetId = "actionset_enemy_furnace_warden",
                    // EMENDA pattern: a flight-phase boss would set this to a FloatingOrbit profile id.
                    // The ground ooze king stays grounded, so this is left empty by design.
                    MovementProfileId = string.Empty,
                    MoveSpeedMultiplier = 1.3f,
                    DamageMultiplier = 1.3f,
                    AddsEnemyId = "enemy_cave_mite",
                    AddsCount = 2,
                    VulnerabilityWindowSeconds = 2f
                }
            };

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void WireRegistry(List<BossPhaseProfileSO> profiles)
        {
            var registry = AssetDatabase.LoadAssetAtPath<BossPhaseProfileRegistrySO>(RegistryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<BossPhaseProfileRegistrySO>();
                AssetDatabase.CreateAsset(registry, RegistryPath);
                Debug.Log($"[AttachDefaultBossPhaseProfiles] Created registry {RegistryPath}.");
            }

            var serializedObject = new SerializedObject(registry);
            var itemsProp = serializedObject.FindProperty("_items");
            itemsProp.ClearArray();

            for (var i = 0; i < profiles.Count; i++)
            {
                itemsProp.InsertArrayElementAtIndex(i);
                itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = profiles[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
            Debug.Log($"[AttachDefaultBossPhaseProfiles] Registry wired with {profiles.Count} profile(s).");
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
