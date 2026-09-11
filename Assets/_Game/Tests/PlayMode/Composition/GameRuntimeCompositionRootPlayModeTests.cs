using System.Collections;
using System.Text.RegularExpressions;
using CindarsHope.Combat;
using CindarsHope.Composition;
using CindarsHope.City.Services;
using CindarsHope.NPC.Friendship;
using CindarsHope.NPC.Gifting;
using CindarsHope.NPC.Services;
using CindarsHope.NPC.Schedule;
using CindarsHope.Skills.Runtime.Effects;
using CindarsHope.Skills.Runtime;
using CindarsHope.UI.Routing;
using CindarsHope.World.Events;
using CindarsHope.World.Weather;
using CindarsHope.World;
using CindarsHope.Farm.Animals;
using CindarsHope.Farm.Forage;
using CindarsHope.Farm.Lots;
using CindarsHope.Farm.Resources;
using CindarsHope.Farm.Runtime;
using CindarsHope.Farm.Shipping;
using CindarsHope.Craft;
using CindarsHope.Inventory;
using CindarsHope.Items.Runtime;
using CindarsHope.Magic;
using CindarsHope.Fonte;
using CindarsHope.Player.Conditions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public class GameRuntimeCompositionRootPlayModeTests
    {
        private static readonly string[] GameplayScenes =
        {
            "FarmScene",
            "TownScene",
            "CaveScene"
        };

        [UnityTest]
        public IEnumerator RootAndTracker_RemainSingleAcrossPlayModeFrames()
        {
            GameRuntimeCompositionRoot root = GameRuntimeCompositionRoot.EnsureExists();
            CombatStateTracker tracker = CombatStateTracker.ActiveInstance;
            GameplayInputRouter inputRouter = GameplayInputRouter.Instance;

            yield return null;

            Assert.That(GameRuntimeCompositionRoot.EnsureExists(), Is.SameAs(root));
            Assert.That(CombatStateTrackerBootstrap.Install(), Is.SameAs(tracker));
            Assert.That(GameRuntimeCompositionRoot.IsReady, Is.True);
            Assert.That(ActiveSkillExecutionController.Instance, Is.Not.Null);
            AssertOwnedByRoot<SurvivalSkillRuntimeCoordinator>(root);
            AssertOwnedByRoot<EfficiencyMarkRuntimeCoordinator>(root);
            Assert.That(inputRouter, Is.Not.Null);
            Assert.That(inputRouter.gameObject, Is.SameAs(root.gameObject));
            AssertOwnedByRoot<FriendshipService>(root);
            AssertOwnedByRoot<GiftGivingService>(root);
            AssertOwnedByRoot<NpcServiceRuntime>(root);
            AssertOwnedByRoot<CityServiceRuntimeBootstrap>(root);
            AssertOwnedByRoot<WorldWeatherService>(root);
            AssertOwnedByRoot<WorldEventService>(root);
            AssertOwnedByRoot<NpcScheduleService>(root);
            AssertOwnedByRoot<ItemDropSpawner>(root);
            AssertOwnedByRoot<FarmDailyGoalService>(root);
            AssertOwnedByRoot<FarmResourceRefreshRuntime>(root);
            AssertOwnedByRoot<FarmAnimalRegistry>(root);
            AssertOwnedByRoot<FarmForageRuntimeService>(root);
            AssertOwnedByRoot<ShippingBinRuntimeService>(root);
            AssertOwnedByRoot<FarmLotRuntimeBootstrap>(root);
            AssertOwnedByRoot<ItemUseManager>(root);
            AssertOwnedByRoot<ConsumableItemRuntimeBootstrap>(root);
            AssertOwnedByRoot<MagicItemRuntimeBootstrap>(root);
            AssertOwnedByRoot<PlayerSpellbook>(root);
            AssertOwnedByRoot<CraftingStationRuntimeBootstrap>(root);
            AssertOwnedByRoot<FonteRuntimeService>(root);
            AssertOwnedByRoot<PlayerConditionService>(root);

            yield return null;

            Assert.That(GameRuntimeCompositionRoot.Instance, Is.SameAs(root));
            Assert.That(CombatStateTracker.ActiveInstance, Is.SameAs(tracker));
            Assert.That(GameplayInputRouter.Instance, Is.SameAs(inputRouter));
        }

        private static void AssertOwnedByRoot<T>(GameRuntimeCompositionRoot root) where T : Component
        {
            var service = Object.FindAnyObjectByType<T>();
            Assert.That(service, Is.Not.Null, $"{typeof(T).Name} was not installed.");
            Assert.That(service.transform.parent, Is.EqualTo(root.transform));
        }

        [UnityTest]
        public IEnumerator GameplayScenes_LoadWithoutMissingScripts()
        {
            // Ao finalizar, o Test Framework restaura sua cena vazia. O DeathSystemBootstrap e
            // DontDestroyOnLoad e esgota o bind nessa cena sem GameBootstrap; isso nao representa
            // uma transicao real do jogo. Consumimos somente essa mensagem exata, mantendo qualquer
            // outro erro como falha do smoke.
            LogAssert.Expect(
                LogType.Error,
                new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));

            foreach (string sceneName in GameplayScenes)
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                Assert.That(load, Is.Not.Null, $"Scene '{sceneName}' did not start loading.");

                yield return load;
                yield return null;

                Scene activeScene = SceneManager.GetActiveScene();
                Assert.That(activeScene.name, Is.EqualTo(sceneName));
                Assert.That(activeScene.isLoaded, Is.True);

                foreach (GameObject root in activeScene.GetRootGameObjects())
                {
                    AssertNoMissingScripts(root, sceneName);
                }
            }
        }

        private static void AssertNoMissingScripts(GameObject gameObject, string sceneName)
        {
            Component[] components = gameObject.GetComponents<Component>();
            for (var index = 0; index < components.Length; index++)
            {
                Assert.That(
                    components[index],
                    Is.Not.Null,
                    $"Scene '{sceneName}' has a missing script at '{GetHierarchyPath(gameObject)}' component {index}.");
            }

            foreach (Transform child in gameObject.transform)
            {
                AssertNoMissingScripts(child.gameObject, sceneName);
            }
        }

        private static string GetHierarchyPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform current = gameObject.transform.parent;
            while (current != null)
            {
                path = $"{current.name}/{path}";
                current = current.parent;
            }

            return path;
        }
    }
}
