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

        public static void Install(Transform owner)
        {
            if (_instance != null) return;

            var go = new GameObject("PlayerMovementActionRuntimeBootstrap");
            go.transform.SetParent(owner);
            if (owner == null) DontDestroyOnLoad(go);
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
                var playerController = ResolvePlayerController();
                if (playerController != null)
                {
                    AttachControllers(playerController);
                    yield break;
                }

                yield return null;
            }

            Debug.LogWarning("[PlayerMovementActionRuntimeBootstrap] Could not resolve PlayerController after max attempts. Dash/Dodge/Block will not be active.");
        }

        // Resolves the real PlayerController GameObject (not PlayerManager, which is data-only).
        // Priority: GameBootstrap.PlayerManager children → scene search fallback.
        private static PlayerController ResolvePlayerController()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null)
            {
                var playerManager = bootstrap.PlayerManager;
                if (playerManager != null)
                {
                    // PlayerController may be on the same GO or a child
                    var pc = playerManager.GetComponent<PlayerController>();
                    if (pc != null) return pc;

                    pc = playerManager.GetComponentInChildren<PlayerController>(true);
                    if (pc != null) return pc;

                    pc = playerManager.GetComponentInParent<PlayerController>();
                    if (pc != null) return pc;
                }
            }

            // Fallback: find in scene (one-shot; only reached if bootstrap hierarchy has no PlayerController)
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<PlayerController>();
#else
            return Object.FindObjectOfType<PlayerController>();
#endif
        }

        private void AttachControllers(PlayerController playerController)
        {
            var playerObject = playerController.gameObject;
            var rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"[PlayerMovementActionRuntimeBootstrap] PlayerController '{playerObject.name}' has no Rigidbody2D. Movement controllers will not be attached.", playerObject);
                return;
            }

            EnsureComponent<PlayerMovementDisplacementResolver>(playerObject);
            EnsureComponent<PlayerDashController>(playerObject);
            EnsureComponent<DirectionalDoubleTapDetector>(playerObject);
            EnsureComponent<PlayerDodgeController>(playerObject);
            EnsureComponent<PlayerBlockController>(playerObject);

            if (!_loggedAttached)
            {
                Debug.Log($"[PlayerMovementActionRuntimeBootstrap] Resolved PlayerController '{playerObject.name}' and attached movement action controllers.", playerObject);
                _loggedAttached = true;
            }
        }

        private static void EnsureComponent<T>(GameObject playerObject) where T : Component
        {
            if (playerObject.GetComponent<T>() == null)
                playerObject.AddComponent<T>();
        }
    }
}
