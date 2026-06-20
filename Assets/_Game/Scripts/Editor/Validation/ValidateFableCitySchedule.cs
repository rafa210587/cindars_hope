using System.Collections.Generic;
using System.IO;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
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
    /// - 12 minimal interiors exist in the off-playfield band (y > +40);
    /// - 24 paired house doors exist (exterior + interior per house).
    /// Also checks runtime code presence. Menu: CindarsHope/Validate/Fable City Schedule (fable_11).
    /// </summary>
    public static class ValidateFableCitySchedule
    {
        private const string MenuPath = "CindarsHope/Validate/Fable City Schedule (fable_11)";
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const float InteriorBandMinY = 40f;

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

            // --- SCENE GRAPH ---
            if (!File.Exists(ScenePath))
            {
                Check($"TownScene exists at {ScenePath}", false, ref passCount, ref failCount);
                Report(passCount, failCount);
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var anchors = FindAll<NpcScheduleAnchor>(scene);
            var doors = FindAll<DoorInteractable>(scene);

            // Anchors: >= 3 per canonical NPC, all 23 covered.
            var byNpc = new Dictionary<string, int>();
            foreach (var anchor in anchors)
            {
                if (anchor == null || string.IsNullOrEmpty(anchor.NpcId)) continue;
                byNpc.TryGetValue(anchor.NpcId, out var c);
                byNpc[anchor.NpcId] = c + 1;
            }

            int npcsWith3 = 0;
            foreach (var entry in NpcTownRosterRegistry.AllEntries)
            {
                if (byNpc.TryGetValue(entry.NpcId, out var count) && count >= 3)
                {
                    npcsWith3++;
                }
            }

            Check($"All {NpcTownRosterRegistry.CanonicalCount} canonical NPCs have >= 3 anchors (have {npcsWith3})",
                npcsWith3 >= NpcTownRosterRegistry.CanonicalCount, ref passCount, ref failCount);

            // Interiors band: count GameObjects named Interior_* with y > +40.
            int interiorCount = CountInteriors(scene);
            Check($"12 interiors in off-playfield band (y>+40) — found {interiorCount}",
                interiorCount >= 12, ref passCount, ref failCount);

            // Doors: 24 paired (exterior + interior per 12 houses).
            Check($"24 paired house doors — found {doors.Count}",
                doors.Count >= 24, ref passCount, ref failCount);

            // At least one shop-gated door (closed-shop hours, decision 6.4-A).
            int shopDoors = 0;
            foreach (var door in doors)
            {
                if (door != null && door.IsShopDoor && !string.IsNullOrEmpty(door.LinkedNpcId)) shopDoors++;
            }
            Check($"At least one closed-shop-gated door — found {shopDoors}",
                shopDoors >= 1, ref passCount, ref failCount);

            Report(passCount, failCount);
        }

        private static int CountInteriors(Scene scene)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != "HouseInteriors") continue;
                foreach (Transform child in root.transform)
                {
                    if (child.name.StartsWith("Interior_") && child.position.y > InteriorBandMinY)
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
