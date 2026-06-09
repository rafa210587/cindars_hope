using System.Collections;
using CindarsHope.Core.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Player.Movement
{
    public sealed class PlayerMovementActionRuntimeBootstrap : MonoBehaviour
    {
        private const int MaxBindAttempts = 120;
        private static PlayerMovementActionRuntimeBootstrap _instance;
        private bool _loggedAttached;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null) return;

            var go = new GameObject("PlayerMovementActionRuntimeBootstrap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayerMovementActionRuntimeBootstrap>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            StartCoroutine(BindWhenReady());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _loggedAttached = false;
            StartCoroutine(BindWhenReady());
        }

        private IEnumerator BindWhenReady()
        {
            for (var attempt = 0; attempt < MaxBindAttempts; attempt++)
            {
                var player = GameBootstrap.Instance?.PlayerManager;
                if (player != null)
                {
                    AttachControllers(player.gameObject);
                    yield break;
                }

                yield return null;
            }
        }

        private void AttachControllers(GameObject playerObject)
        {
            if (playerObject == null) return;

            EnsureComponent<PlayerMovementDisplacementResolver>(playerObject);
            EnsureComponent<PlayerDashController>(playerObject);
            EnsureComponent<DirectionalDoubleTapDetector>(playerObject);
            EnsureComponent<PlayerDodgeController>(playerObject);
            EnsureComponent<PlayerBlockController>(playerObject);

            if (!_loggedAttached)
            {
                Debug.Log("[PlayerMovementActionRuntimeBootstrap] Dash/Dodge/Block controllers attached.", playerObject);
                _loggedAttached = true;
            }
        }

        private static void EnsureComponent<T>(GameObject playerObject) where T : Component
        {
            if (playerObject.GetComponent<T>() == null)
            {
                playerObject.AddComponent<T>();
            }
        }
    }
}
