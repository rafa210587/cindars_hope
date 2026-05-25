using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public class CorpseSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _corpsePrefab;
        [SerializeField] private Transform _corpseParent;

        private CorpseRecoveryManager _recoveryManager;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CorpseCreatedEvent>(OnCorpseCreated);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CorpseCreatedEvent>(OnCorpseCreated);
        }

        public void Initialize(CorpseRecoveryManager recoveryManager)
        {
            _recoveryManager = recoveryManager;
        }

        private void OnCorpseCreated(CorpseCreatedEvent evt)
        {
            if (_corpsePrefab == null)
            {
                Debug.LogWarning("[CorpseSpawner] Corpse prefab not assigned, using default sphere");
                SpawnDefaultCorpse(evt);
                return;
            }

            SpawnCorpseFromPrefab(evt);
        }

        private void SpawnCorpseFromPrefab(CorpseCreatedEvent evt)
        {
            var corpseGO = Instantiate(_corpsePrefab, _corpseParent);
            corpseGO.name = $"Corpse_{evt.CorpseId}";

            var interactable = corpseGO.GetComponent<CorpseInteractable>();
            if (interactable != null && _recoveryManager != null && _recoveryManager.ActiveCorpse != null)
            {
                interactable.Initialize(_recoveryManager.ActiveCorpse, _recoveryManager);
                Debug.Log($"[CorpseSpawner] Corpse {evt.CorpseId} spawned with interactable");
            }
            else
            {
                Debug.LogWarning($"[CorpseSpawner] Could not initialize corpse interactable for {evt.CorpseId}");
            }
        }

        private void SpawnDefaultCorpse(CorpseCreatedEvent evt)
        {
            var corpseGO = new GameObject($"Corpse_{evt.CorpseId}");
            corpseGO.transform.parent = _corpseParent;
            corpseGO.transform.position = Vector3.zero;

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = corpseGO.transform;
            sphere.transform.localPosition = Vector3.zero;
            sphere.GetComponent<Collider>().enabled = true;

            var interactable = corpseGO.AddComponent<CorpseInteractable>();
            if (_recoveryManager != null && _recoveryManager.ActiveCorpse != null)
            {
                interactable.Initialize(_recoveryManager.ActiveCorpse, _recoveryManager);
            }

            Debug.Log($"[CorpseSpawner] Default corpse spawned for {evt.CorpseId}");
        }
    }
}
