using System.Collections;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// Bootstrap de self-wiring para itens consumíveis (comida/poção) no pipeline <see cref="ItemUseManager"/>.
    /// Segue o idioma *RuntimeBootstrap do projeto: RuntimeInitializeOnLoadMethod + DontDestroyOnLoad singleton
    /// + coroutine de retry — sem GameObject.Find nem FindObjectOfType.
    ///
    /// Registra um único <see cref="ConsumableFoodUseHandler"/> em todos os itens de Category=Food/Consumable
    /// ou ConsumableSubtype=Food/BuffFood/Potion que existam no <see cref="ItemDatabaseSO"/> do GameBootstrap.
    /// </summary>
    public sealed class ConsumableItemRuntimeBootstrap : MonoBehaviour
    {
        private const string GameObjectName = "ConsumableItemRuntimeBootstrap";
        private const int MaxBindAttempts = 120;

        private static ConsumableItemRuntimeBootstrap _instance;

        private ConsumableFoodUseHandler _handler;
        private bool _handlersRegistered;

        public static ConsumableItemRuntimeBootstrap Instance => _instance;
        public bool IsFullyBound => _handlersRegistered;

        public static ConsumableItemRuntimeBootstrap Install(Transform owner)
        {
            if (_instance != null)
                return _instance;

            var go = new GameObject(GameObjectName);
            if (owner != null) go.transform.SetParent(owner, false);
            else DontDestroyOnLoad(go);
            _instance = go.AddComponent<ConsumableItemRuntimeBootstrap>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            StartCoroutine(BindWhenReady());
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private IEnumerator BindWhenReady()
        {
            var attempts = 0;
            while (attempts < MaxBindAttempts && !IsFullyBound)
            {
                TryRegisterHandlers();

                if (IsFullyBound)
                {
                    Debug.Log("[ConsumableItemRuntimeBootstrap] Consumable food handlers registrados.", this);
                    yield break;
                }

                attempts++;
                yield return null;
            }

            if (!IsFullyBound)
            {
                Debug.LogWarning(
                    $"[ConsumableItemRuntimeBootstrap] Nao foi possivel registrar handlers apos {MaxBindAttempts} tentativas. " +
                    "Itens de comida podem nao restaurar fome ate que GameBootstrap/ItemUseManager estejam disponiveis.", this);
            }
        }

        private void TryRegisterHandlers()
        {
            if (_handlersRegistered)
                return;

            var useManager = ItemUseManager.Instance;
            if (useManager == null)
                return;

            var bootstrap = GameBootstrap.Instance;
            var itemDatabase = bootstrap != null ? bootstrap.ItemDatabase : null;
            if (itemDatabase == null)
                return;

            if (_handler == null)
            {
                _handler = ScriptableObject.CreateInstance<ConsumableFoodUseHandler>();
                _handler.Configure(itemDatabase);
            }

            var registered = 0;
            foreach (var itemData in itemDatabase.All)
            {
                if (itemData == null || string.IsNullOrWhiteSpace(itemData.Id))
                    continue;

                if (IsConsumableItem(itemData))
                {
                    useManager.RegisterHandler(itemData.Id, _handler);
                    registered++;
                }
            }

            if (registered == 0)
            {
                // Database pode ainda nao ter sido populado — retry na proxima frame.
                return;
            }

            _handlersRegistered = true;
            Debug.Log($"[ConsumableItemRuntimeBootstrap] {registered} handlers de consumivel registrados.", this);
        }

        private static bool IsConsumableItem(ItemDataSO item)
        {
            return item.Category == ItemCategory.Food
                || item.Category == ItemCategory.Consumable
                || item.ConsumableSubtype == ConsumableSubtype.Food
                || item.ConsumableSubtype == ConsumableSubtype.BuffFood
                || item.ConsumableSubtype == ConsumableSubtype.Potion;
        }
    }
}
