using System.Collections.Generic;
using System.IO;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
using CindarsHope.SceneManagement;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// fable_11 editor validator (CA-2 / CA-3). Opens TownScene and verifies:
    /// - every canonical town NPC has >= 3 schedule anchors (work/social/home);
    /// - casas FÍSICAS percorríveis: cada casa tem um RoofRevealController (telhado some ao entrar),
    ///   substituindo a antiga faixa off-field de interiores + portas de teleporte.
    /// Also checks runtime code presence. Menu: CindarsHope/Validate/Fable City Schedule (fable_11).
    /// </summary>
    public static class ValidateFableCitySchedule
    {
        private const string MenuPath = "CindarsHope/Validate/Fable City Schedule (fable_11)";
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            Debug.Log("[ValidateFableCitySchedule] Starting fable_11 city schedule/doors/interiors validation...");
            int passCount = 0;
            int failCount = 0;

            // --- CODE PRESENCE ---
            Check("DoorInteractable.cs exists",
                File.Exists("Assets/_Game/Scripts/World/DoorInteractable.cs"), ref passCount, ref failCount);
            Check("NpcScheduleBlockResolver.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlockResolver.cs"), ref passCount, ref failCount);
            Check("NpcScheduleAvailabilityGate.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleAvailabilityGate.cs"), ref passCount, ref failCount);
            Check("NpcScheduleBlockChangedEvent.cs exists",
                File.Exists("Assets/_Game/Scripts/Core/Events/NpcScheduleBlockChangedEvent.cs"), ref passCount, ref failCount);
            Check("TownCityLayout.cs exists",
                File.Exists("Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs"), ref passCount, ref failCount);

            // --- SCENE GRAPH ---
            if (!File.Exists(ScenePath))
            {
                Check($"TownScene exists at {ScenePath}", false, ref passCount, ref failCount);
                Report(passCount, failCount);
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var anchors = FindAll<NpcScheduleAnchor>(scene);
            var roofReveals = FindAll<RoofRevealController>(scene);
            var doors = FindAll<HouseDoorInteractable>(scene);
            var spawns = FindAll<SceneSpawnPoint>(scene);

            // Anchors: exactly work/social/home per all 28 canonical NPCs.
            var byNpc = new Dictionary<string, int>();
            foreach (var anchor in anchors)
            {
                if (anchor == null || string.IsNullOrEmpty(anchor.NpcId)) continue;
                byNpc.TryGetValue(anchor.NpcId, out var c);
                byNpc[anchor.NpcId] = c + 1;
            }

            int npcsWith3 = 0;
            var anchorIds = new HashSet<string>();
            bool uniqueAnchorIds = true;
            foreach (var anchor in anchors)
            {
                if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId) || !anchorIds.Add(anchor.AnchorId))
                {
                    uniqueAnchorIds = false;
                }
            }
            foreach (var entry in NpcTownRosterRegistry.AllEntries)
            {
                if (byNpc.TryGetValue(entry.NpcId, out var count) && count >= 3)
                {
                    npcsWith3++;
                }
            }

            Check($"All {NpcTownRosterRegistry.CanonicalCount} canonical NPCs have >= 3 anchors (have {npcsWith3})",
                npcsWith3 >= NpcTownRosterRegistry.CanonicalCount, ref passCount, ref failCount);
            Check($"Exactly 84 schedule anchors are materialized (found {anchors.Count})",
                anchors.Count == NpcTownRosterRegistry.CanonicalCount * 3, ref passCount, ref failCount);
            Check("Schedule anchor IDs are non-empty and unique", uniqueAnchorIds, ref passCount, ref failCount);

            // Casas físicas percorríveis: cada casa tem um RoofRevealController (telhado some ao entrar).
            Check($"Casas percorríveis com revelação de telhado — encontradas {roofReveals.Count} (>=24)",
                roofReveals.Count >= TownCityLayout.BaselineHouseCount, ref passCount, ref failCount);
            Check($"Portas físicas funcionais — encontradas {doors.Count} (>=24)",
                doors.Count >= TownCityLayout.BaselineHouseCount, ref passCount, ref failCount);

            // Toda casa percorrível deve ter um chão (objeto Floor) sob o root House_*.
            int walkInFloors = CountWalkInFloors(scene);
            Check($"Casas com chão andável (Floor sob House_*) — encontradas {walkInFloors} (>=24)",
                walkInFloors >= TownCityLayout.BaselineHouseCount, ref passCount, ref failCount);

            int houseCount = CountNamedPrefix(scene, "House_");
            int npcStalls = CountNamedPrefix(scene, "Stall_npc_");
            int marketStalls = CountNamedPrefix(scene, "MarketSquare_Stall_");
            int treeCount = CountNamedPrefix(scene, "TownTree_");
            Check($"House_* preservation floor — found {houseCount} (>=24)",
                houseCount >= TownCityLayout.BaselineHouseCount, ref passCount, ref failCount);
            Check($"NPC stall preservation floor — found {npcStalls} (>=23)",
                npcStalls >= TownCityLayout.BaselineNpcStallCount, ref passCount, ref failCount);
            Check($"Market stall preservation floor — found {marketStalls} (>=6)",
                marketStalls >= TownCityLayout.BaselineMarketStallCount, ref passCount, ref failCount);
            Check($"Dense irregular exterior forest — found {treeCount} (>={TownCityLayout.BaselineTreeCount})",
                treeCount >= TownCityLayout.BaselineTreeCount, ref passCount, ref failCount);

            Check("All stable building roots exist at their canonical lots",
                ValidateStableBuildings(scene), ref passCount, ref failCount);
            Check("town_default and town_from_farm spawn IDs remain present",
                HasSpawn(spawns, "town_default") && HasSpawn(spawns, "town_from_farm"), ref passCount, ref failCount);
            Check("Temple, cemetery, Chamber, TownHall, market and lake landmarks remain present",
                HasNamedObject(scene, "House_Temple") &&
                HasNamedObject(scene, "Cemetery_Ground") &&
                HasNamedObject(scene, "House_Chamber") &&
                HasNamedObject(scene, "TownHallBuilding") &&
                HasNamedObject(scene, "House_MarketHall") &&
                HasNamedObject(scene, "LakeWater"), ref passCount, ref failCount);

            Check("Only the 8 internal trees carry trunk colliders",
                CountTreeColliders(scene) == 8, ref passCount, ref failCount);
            Check("Every v8 building has a replaceable semantic facade placeholder",
                CountNamedPrefix(scene, "SemanticPlaceholder_") >= TownCityLayout.BaselineHouseCount,
                ref passCount, ref failCount);
            Check("All six standard NPC houses contain a kitchen counter",
                CountNamedPrefix(scene, "Furniture_KitchenCounter") >= 6,
                ref passCount, ref failCount);

            Report(passCount, failCount);
        }

        // Conta casas percorríveis: roots cujo nome começa com "House_" e que têm um filho "Floor".
        private static int CountWalkInFloors(Scene scene)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!t.gameObject.name.StartsWith("House_")) continue;
                    foreach (Transform child in t)
                    {
                        if (child.name == "Floor")
                        {
                            count++;
                            break;
                        }
                    }
                }
            }
            return count;
        }

        private static int CountNamedPrefix(Scene scene, string prefix)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name.StartsWith(prefix, System.StringComparison.Ordinal))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static bool ValidateStableBuildings(Scene scene)
        {
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                Transform found = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (transform.name == lot.Name)
                        {
                            found = transform;
                            break;
                        }
                    }

                    if (found != null) break;
                }

                if (found == null || Vector2.Distance(found.position, lot.Center) > 0.01f)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSpawn(List<SceneSpawnPoint> spawns, string spawnId)
        {
            foreach (var spawn in spawns)
            {
                if (spawn != null && spawn.SpawnId == spawnId) return true;
            }

            return false;
        }

        private static bool HasNamedObject(Scene scene, string objectName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == objectName) return true;
                }
            }

            return false;
        }

        private static int CountTreeColliders(Scene scene)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name.StartsWith("TownTree_", System.StringComparison.Ordinal) &&
                        transform.GetComponent<BoxCollider2D>() != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static List<T> FindAll<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return results;
        }

        private static void Report(int passCount, int failCount)
        {
            Debug.Log($"[ValidateFableCitySchedule] Complete. PASS: {passCount} | FAIL: {failCount}");
            if (failCount > 0)
            {
                Debug.LogError($"[ValidateFableCitySchedule] {failCount} check(s) failed. Review above logs.");
            }
        }

        private static void Check(string label, bool condition, ref int passCount, ref int failCount)
        {
            if (condition)
            {
                Debug.Log($"[ValidateFableCitySchedule] PASS: {label}");
                passCount++;
            }
            else
            {
                Debug.LogError($"[ValidateFableCitySchedule] FAIL: {label}");
                failCount++;
            }
        }
    }
}
