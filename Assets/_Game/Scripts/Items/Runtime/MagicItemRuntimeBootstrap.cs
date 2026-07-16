using System.Collections;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// fable_31 — Self-wiring runtime bootstrap for the magic-item systems. Mirrors the project's established
    /// *RuntimeBootstrap idiom (RuntimeInitializeOnLoadMethod + DontDestroyOnLoad singleton + bind-retry
    /// coroutine), and resolves all dependencies via existing singletons — NO GameObject.Find / FindObjectOfType.
    ///
    /// Responsibilities:
    ///   1. Register <see cref="MagicItemUseHandler"/> for the four magic "use" items on the existing
    ///      <see cref="ItemUseManager"/> (F08 pipeline — no parallel consumption flow).
    ///   2. Host an <see cref="ItemPassiveTracker"/> and bind it to the live InventoryManager so the continuous
    ///      flags (pendant/lantern/pouch/candle) and pouch slot bonus stay current.
    ///
    /// Does NOT create a parallel InventoryManager or ItemUseManager; does NOT touch cave generation.
    /// </summary>
    public sealed class MagicItemRuntimeBootstrap : MonoBehaviour
    {
        private const string GameObjectName = "MagicItemRuntimeBootstrap";
        private const int MaxBindAttempts = 120;

        private static MagicItemRuntimeBootstrap _instance;

        private ItemPassiveTracker _passiveTracker;
        private MagicItemUseHandler _useHandler;
        private ScrollIdentifyUseHandler _scrollHandler;
        private bool _handlersRegistered;
        private bool _scrollRegistered;
        private bool _trackerBound;

        public static MagicItemRuntimeBootstrap Instance => _instance;
        public bool IsFullyBound => _handlersRegistered && _scrollRegistered && _trackerBound;

        public static MagicItemRuntimeBootstrap Install(Transform owner)
        {
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject(GameObjectName);
            if (owner != null) go.transform.SetParent(owner, false);
            else DontDestroyOnLoad(go);
            _instance = go.AddComponent<MagicItemRuntimeBootstrap>();
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

            _passiveTracker = gameObject.AddComponent<ItemPassiveTracker>();
            StartCoroutine(BindWhenReady());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private IEnumerator BindWhenReady()
        {
            var attempts = 0;
            while (attempts < MaxBindAttempts && !IsFullyBound)
            {
                TryRegisterHandlers();
                TryBindTracker();

                if (IsFullyBound)
                {
                    Debug.Log("[MagicItemRuntimeBootstrap] Magic items wired (use handler + passive tracker).", this);
                    yield break;
                }

                attempts++;
                yield return null;
            }

            if (!IsFullyBound)
            {
                Debug.LogWarning(
                    $"[MagicItemRuntimeBootstrap] Could not fully bind after {MaxBindAttempts} attempts. " +
                    $"HandlersRegistered={_handlersRegistered}, TrackerBound={_trackerBound}. " +
                    "Magic item effects may be inert until InventoryManager/ItemUseManager are available.", this);
            }
        }

        private void TryRegisterHandlers()
        {
            if (_handlersRegistered)
            {
                return;
            }

            var useManager = ItemUseManager.Instance;
            if (useManager == null)
            {
                return;
            }

            if (_useHandler == null)
            {
                _useHandler = ScriptableObject.CreateInstance<MagicItemUseHandler>();
                _useHandler.Configure(new MagicItemUseService(), new RuntimeMagicTimeGateway());
            }

            foreach (var itemId in MagicItemCatalog.MagicItemIds)
            {
                if (MagicItemCatalog.IsUseItem(itemId))
                {
                    useManager.RegisterHandler(itemId, _useHandler);
                }
            }

            _handlersRegistered = true;
        }

        private void TryBindTracker()
        {
            if (_trackerBound)
            {
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — cast local para o tipo concreto
            // (bootstrap.InventoryManager agora retorna a porta IInventoryRuntime).
            var inventory = bootstrap != null ? bootstrap.InventoryManager as InventoryManager : null;
            if (inventory == null || !inventory.IsInitialized)
            {
                return;
            }

            if (_passiveTracker == null)
            {
                _passiveTracker = gameObject.AddComponent<ItemPassiveTracker>();
            }

            _passiveTracker.Bind(inventory);
            _trackerBound = true;

            RegisterScrollHandler(inventory);
        }

        private void RegisterScrollHandler(InventoryManager inventory)
        {
            if (_scrollRegistered)
            {
                return;
            }

            var useManager = ItemUseManager.Instance;
            if (useManager == null || inventory == null)
            {
                return;
            }

            if (_scrollHandler == null)
            {
                _scrollHandler = ScriptableObject.CreateInstance<ScrollIdentifyUseHandler>();
                _scrollHandler.Configure(inventory);
            }

            useManager.RegisterHandler(MagicItemCatalog.ScrollIdentifyId, _scrollHandler);
            _scrollRegistered = true;
        }
    }
}
