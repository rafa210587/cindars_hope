using CindarsHope.Farm;
using CindarsHope.Farm.Runtime;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Validador de presença dos elementos da FarmScene v7 (spec_farm_scene_relayout_v4 §15.5 v7).
    /// Executado via CindarsHope/Validar Projeto (read-only; nao gera nem repara assets).
    /// v5 (2026-06-26): adicionados checks de FarmHouse (walk-in) e FarmHouseChest.
    /// v6 (2026-06-26): adicionados checks de NPC_Zrix_Farm e Exp_North/West/South (Fases 5-6).
    /// v7 (2026-06-27): composicao por borda — rio borda leste, homestead leste, bosque oeste denso,
    ///   animais borda sul, Exp_North/NE/West nas bordas. Exp_South renomeada para Exp_NE.
    ///   Miolo x[-18,16] y[-15,16] totalmente aravel.
    /// </summary>
    public static class ValidateFarmSceneLayoutV4
    {
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";

        public static void Validate()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(FarmScenePath);
            if (sceneAsset == null)
            {
                Debug.LogWarning("[ValidateFarmSceneLayoutV4] FarmScene.unity nao encontrada em " + FarmScenePath +
                                 ". Rode CindarsHope/Inicializar Projeto primeiro.");
                return;
            }

            // Abre a cena em modo additive para inspecionar objetos sem destruir a cena atual.
            var scene = EditorSceneManager.OpenScene(FarmScenePath, OpenSceneMode.Additive);

            int passed = 0;
            int failed = 0;

            void Check(bool condition, string label)
            {
                if (condition)
                {
                    passed++;
                }
                else
                {
                    failed++;
                    Debug.LogError($"[ValidateFarmSceneLayoutV4] FALHA: {label}");
                }
            }

            // ── Spawn points ───────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "Spawn_farm_default") != null,    "Spawn_farm_default presente");
            Check(FindByName(scene, "Spawn_farm_from_town") != null,  "Spawn_farm_from_town presente");
            Check(FindByName(scene, "Spawn_farm_from_cave") != null,  "Spawn_farm_from_cave presente");

            // ── Portal ─────────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "Portal_Farm_To_Town") != null,   "Portal_Farm_To_Town presente");

            // ── Casa WALK-IN v5 ────────────────────────────────────────────────────────────────
            // v5: casa fisica percorrivel com Bed + BedLetter + FarmHouseChest dentro.
            Check(FindByName(scene, "FarmHouse") != null,             "FarmHouse (walk-in) presente");
            Check(FindByName(scene, "Bed") != null,                   "Bed (dentro da FarmHouse) presente");
            Check(FindByName(scene, "BedLetter") != null,             "BedLetter (dentro da FarmHouse) presente");
            Check(FindByName(scene, "FarmHouseChest") != null,        "FarmHouseChest (dentro da FarmHouse) presente");

            // ── Quadro de Evolucoes (substitui lotes fable_41) ─────────────────────────────────
            Check(FindComponentOfType<FarmEvolutionBoardInteractable>(scene) != null,
                  "FarmEvolutionBoardInteractable presente");

            // ── Caverna ────────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "CaveEntrance") != null,          "CaveEntrance presente");
            Check(FindByName(scene, "Board_Zrix") != null,            "Board_Zrix presente");

            // ── Zrix NPC (Fase 5) ──────────────────────────────────────────────────────────────
            Check(FindByName(scene, "NPC_Zrix_Farm") != null,         "NPC_Zrix_Farm (perambulador) presente");

            // ── Areas de expansao v7 (nas bordas — Exp_North/NE/West) ─────────────────────────
            Check(FindByName(scene, "Exp_North") != null,             "Exp_North presente (borda norte)");
            Check(FindByName(scene, "Exp_NE") != null,                "Exp_NE presente (borda norte-leste)");
            Check(FindByName(scene, "Exp_West") != null,              "Exp_West presente (borda oeste-sul)");

            // ── Montanha ───────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "MountainBarrier") != null,       "MountainBarrier presente");

            // ── Rio e ponte ────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "RiverAndBridge") != null,        "RiverAndBridge presente");
            Check(FindByName(scene, "Bridge_01") != null,             "Bridge_01 presente");

            // ── Veios de minerio ───────────────────────────────────────────────────────────────
            var oreNodes = FindComponentsOfType<LockedOreNodeInteractable>(scene);
            Check(oreNodes != null && oreNodes.Length >= 4,           "4 LockedOreNodeInteractable presentes");

            // ── FarmSceneRuntimeBootstrap e FarmTillingInputController ─────────────────────────
            Check(FindComponentOfType<FarmSceneRuntimeBootstrap>(scene) != null,
                  "FarmSceneRuntimeBootstrap presente");
            Check(FindComponentOfType<FarmTillingInputController>(scene) != null,
                  "FarmTillingInputController presente");

            // ── Construcoes ────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "Coop_01") != null,               "Coop_01 presente");
            Check(FindByName(scene, "Barn_01") != null,               "Barn_01 presente");
            Check(FindByName(scene, "Station_CheesePress") != null,   "Station_CheesePress presente");
            Check(FindByName(scene, "Station_WineBarrel") != null,    "Station_WineBarrel presente");
            Check(FindByName(scene, "Greenhouse") != null,            "Greenhouse presente");

            // ── Crafting ───────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "CraftingStation_Workbench") != null,    "CraftingStation_Workbench presente");
            Check(FindByName(scene, "CraftingStation_Forge") != null,        "CraftingStation_Forge presente");
            Check(FindByName(scene, "CraftingStation_CookingStation") != null, "CraftingStation_CookingStation presente");

            // ── Economy ────────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "ShippingBin_farm_shipping_bin_01") != null, "ShippingBin presente");
            Check(FindByName(scene, "SellPoint") != null,             "SellPoint presente");

            // ── Recursos ───────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "FishingSpot") != null,           "FishingSpot presente");
            Check(FindByName(scene, "RockResource_01") != null,       "RockResource_01 presente");
            Check(FindByName(scene, "FonteAnya") != null,             "FonteAnya presente");

            // ── Forage ─────────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "FarmForagePoints") != null,      "FarmForagePoints (parent) presente");

            // ── Bounds v6 (64x44) ──────────────────────────────────────────────────────────────
            Check(FindByName(scene, "Bounds") != null,                "Bounds presente");

            // ── Arvores ────────────────────────────────────────────────────────────────────────
            Check(FindByName(scene, "Trees") != null,                 "Trees (parent) presente");

            EditorSceneManager.CloseScene(scene, true);

            if (failed == 0)
            {
                Debug.Log($"[ValidateFarmSceneLayoutV4] PASS — {passed}/{passed + failed} checks. FarmScene v7 OK (miolo aberto, borda leste homestead+rio, bosque oeste, animais sul, Exp_North/NE/West).");
            }
            else
            {
                Debug.LogError($"[ValidateFarmSceneLayoutV4] FAIL — {failed} falha(s) de {passed + failed} checks.");
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────────────────────

        private static GameObject FindByName(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindByNameRecursive(root.transform, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindByNameRecursive(Transform t, string name)
        {
            if (t.name == name) return t.gameObject;
            foreach (Transform child in t)
            {
                var found = FindByNameRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static T FindComponentOfType<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var comp = root.GetComponentInChildren<T>(true);
                if (comp != null) return comp;
            }
            return null;
        }

        private static T[] FindComponentsOfType<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            var list = new System.Collections.Generic.List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                list.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return list.ToArray();
        }
    }
}
