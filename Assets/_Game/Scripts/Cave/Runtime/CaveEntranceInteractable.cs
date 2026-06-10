using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.World.Scenes;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// WAVE_INTEGRATION_16 — Cave Entrance Interactable.
    ///
    /// Place this on a GameObject in FarmScene (or TownScene) to give the player
    /// an entry point into CaveScene. Delegates the actual scene transition to
    /// SceneTransitionRouter (WAVE_INTEGRATION_13) so the transition guard and
    /// spawn resolution flow work correctly.
    ///
    /// Wiring requirements (human Unity Editor action required):
    ///   - Place on Zone_CaveEntrance (FarmScene) or equivalent object in TownScene
    ///   - Set _targetCaveScene = "CaveScene"
    ///   - Set _targetSpawnId = "spawn_cave_from_farm" (or "spawn_cave_from_town")
    ///   - Set _transitionGateId = "gate_farm_cave_entrance" (stable ID from SceneId)
    ///   - Set _fromSceneId = "FarmScene" (or "TownScene")
    ///
    /// Does NOT start a new CaveRunSeed — CaveRunManager handles initialization
    /// in CaveScene's Awake (restores from GameBootstrap cache or creates new).
    ///
    /// Does NOT touch combat, loot, or enemy AI (WAVE_INTEGRATION_17 scope).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CaveEntranceInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _transitionGateId = "gate_farm_cave_entrance";
        [SerializeField] private string _fromSceneId = "FarmScene";
        [SerializeField] private string _targetCaveScene = "CaveScene";
        [SerializeField] private string _targetSpawnId = "spawn_cave_from_farm";
        [SerializeField] private string _interactionPrompt = "Entrar na caverna";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            if (string.IsNullOrWhiteSpace(_targetCaveScene))
            {
                Debug.LogWarning(
                    $"[CaveEntranceInteractable] '{name}': targetCaveScene is not set. Cannot enter cave.",
                    this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_targetSpawnId))
            {
                Debug.LogWarning(
                    $"[CaveEntranceInteractable] '{name}': targetSpawnId is not set. Cannot enter cave.",
                    this);
                return false;
            }

            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            Debug.Log(
                $"[CaveEntranceInteractable] Player entering cave. " +
                $"Gate: {_transitionGateId} | {_fromSceneId} → {_targetCaveScene} @{_targetSpawnId}",
                this);

            // Notify interested systems that a cave run is about to start
            // (CaveRunManager in CaveScene will initialize/restore its own state).
            GameEventBus.Publish(new CaveRunStartedEvent(
                caveRunSeed: string.Empty,          // Seed not known until CaveScene loads
                enteredFromScene: _fromSceneId,
                targetSpawnId: _targetSpawnId));

            var request = new SceneTransitionRequest(
                sourceSceneId: _fromSceneId,
                destinationSceneId: _targetCaveScene,
                targetSpawnAnchorId: _targetSpawnId,
                initiatingGateId: _transitionGateId);

            SceneTransitionRouter.Execute(request);
        }

        private void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(_transitionGateId))
                _transitionGateId = _transitionGateId.Trim();
            if (!string.IsNullOrWhiteSpace(_fromSceneId))
                _fromSceneId = _fromSceneId.Trim();
            if (!string.IsNullOrWhiteSpace(_targetCaveScene))
                _targetCaveScene = _targetCaveScene.Trim();
            if (!string.IsNullOrWhiteSpace(_targetSpawnId))
                _targetSpawnId = _targetSpawnId.Trim();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.3f, 0.1f, 0.8f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.8f,
                $"Cave Entrance\n{_transitionGateId}");
        }
#endif
    }
}
