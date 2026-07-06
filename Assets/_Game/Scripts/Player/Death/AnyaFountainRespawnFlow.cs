using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Locations;
using CindarsHope.SceneManagement;
using CindarsHope.World.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Player.Death
{
    /// <summary>
    /// Fluxo de respawn na Fonte da Anya, com suporte CROSS-CENA.
    ///
    /// - Se a AnyaFountain existe na cena ATIVA: restaura vitais + teleporta para o RespawnPoint
    ///   imediatamente (caso da FarmScene).
    /// - Se NAO existe (ex.: morte na caverna): carrega a cena da Fonte (FarmScene) via
    ///   SceneTransitionRouter e, quando a cena terminar de carregar, re-resolve a Fonte e completa
    ///   o respawn ali. Anti-softlock: se mesmo apos o load nao houver Fonte, restaura o HP cheio
    ///   no lugar para o jogador nunca ficar preso morto.
    ///
    /// Nasce sozinho via self-bootstrap estatico (idiom *RuntimeBootstrap do projeto): GameObject
    /// DontDestroyOnLoad + singleton guard. FindAnyObjectByType<AnyaFountain> aqui e wiring de SETUP
    /// do fluxo de respawn (re-resolve um objeto POR CENA apos load), NAO comunicacao de gameplay —
    /// e o mesmo idiom ja usado pelo DeathSystemBootstrap original.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnyaFountainRespawnFlow : MonoBehaviour
    {
        private static AnyaFountainRespawnFlow _instance;

        // Cena que contem a Fonte da Anya (a Fonte vive na FarmScene; ver CreateMvpFarmScene).
        private const string FountainSceneName = SceneNames.Farm;
        private const string FountainSpawnAnchorId = SceneId.SpawnFarmDefault;
        private const string RespawnGateId = "gate_death_respawn_anya_fountain";

        // True enquanto esperamos a cena da Fonte carregar para completar um respawn cross-cena.
        private bool _awaitingFountainScene;

        public static AnyaFountainRespawnFlow Instance => _instance;

        public static void Install(Transform owner)
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("AnyaFountainRespawnFlow");
            go.transform.SetParent(owner);
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AnyaFountainRespawnFlow>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Inicia o respawn na Fonte da Anya. Retorna imediatamente; o respawn cross-cena completa
        /// de forma assincrona quando a cena da Fonte termina de carregar.
        /// </summary>
        public void Respawn()
        {
            var fountain = Object.FindAnyObjectByType<AnyaFountain>();
            if (fountain != null && fountain.RespawnPoint != null)
            {
                CompleteRespawnAtFountain(fountain);
                return;
            }

            // Sem Fonte na cena atual: carrega a cena da Fonte e completa apos o load.
            _awaitingFountainScene = true;
            Debug.Log($"[AnyaFountainRespawnFlow] Sem Fonte na cena atual; carregando '{FountainSceneName}' para respawn cross-cena.");

            var request = new SceneTransitionRequest(
                SceneManager.GetActiveScene().name,
                FountainSceneName,
                FountainSpawnAnchorId,
                RespawnGateId);

            var result = SceneTransitionRouter.Execute(request);
            if (!result.Succeeded)
            {
                // O router recusou (ex.: outra transicao em andamento). Anti-softlock: revive no lugar.
                _awaitingFountainScene = false;
                Debug.LogWarning($"[AnyaFountainRespawnFlow] Falha ao carregar cena da Fonte: {result.Reason}. Revivendo no lugar (anti-softlock).");
                ReviveInPlaceFallback();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_awaitingFountainScene)
            {
                return;
            }

            _awaitingFountainScene = false;

            var fountain = Object.FindAnyObjectByType<AnyaFountain>();
            if (fountain != null && fountain.RespawnPoint != null)
            {
                CompleteRespawnAtFountain(fountain);
                return;
            }

            Debug.LogWarning(
                $"[AnyaFountainRespawnFlow] Cena '{scene.name}' carregou mas nenhuma AnyaFountain foi encontrada. " +
                "Revivendo no lugar (anti-softlock).");
            ReviveInPlaceFallback();
        }

        private void CompleteRespawnAtFountain(AnyaFountain fountain)
        {
            var bootstrap = GameBootstrap.Instance;
            var playerManager = bootstrap != null ? bootstrap.PlayerManager : null;
            if (playerManager == null)
            {
                Debug.LogError("[AnyaFountainRespawnFlow] PlayerManager indisponivel; respawn na Fonte abortado.");
                return;
            }

            var respawnService = new AnyaRespawnService(
                playerManager,
                bootstrap.StaminaManager,
                bootstrap.ManaManager,
                fountain.RespawnPoint);

            respawnService.RespawnAtAnyaFountain();
            Debug.Log("[AnyaFountainRespawnFlow] Jogador respawnado na Fonte da Anya.");
        }

        private static void ReviveInPlaceFallback()
        {
            var bootstrap = GameBootstrap.Instance;
            var playerManager = bootstrap != null ? bootstrap.PlayerManager : null;
            if (playerManager != null)
            {
                playerManager.SetHP(playerManager.MaxHP);
                GameEventBus.Publish(new AnyaRespawnCompletedEvent());
            }
        }
    }
}
