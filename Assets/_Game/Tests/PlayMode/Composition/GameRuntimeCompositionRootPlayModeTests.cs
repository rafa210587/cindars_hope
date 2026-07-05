using System.Collections;
using System.Text.RegularExpressions;
using CindarsHope.Combat;
using CindarsHope.Composition;
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

            yield return null;

            Assert.That(GameRuntimeCompositionRoot.EnsureExists(), Is.SameAs(root));
            Assert.That(CombatStateTrackerBootstrap.Install(), Is.SameAs(tracker));
            Assert.That(GameRuntimeCompositionRoot.IsReady, Is.True);

            yield return null;

            Assert.That(GameRuntimeCompositionRoot.Instance, Is.SameAs(root));
            Assert.That(CombatStateTracker.ActiveInstance, Is.SameAs(tracker));
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
