using System.Collections;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — self-wiring do sistema de lotes. Segue o idioma *RuntimeBootstrap do projeto
    /// (RuntimeInitializeOnLoadMethod + DontDestroyOnLoad + bind-retry), resolvendo dependências
    /// por singletons existentes — SEM GameObject.Find/FindObjectOfType de gameplay.
    ///
    /// Responsabilidades:
    ///   1. Garantir um <see cref="FarmLotService"/> (se não houver um já presente na cena/binding).
    ///   2. Registrar o <see cref="DeedUseHandler"/> para cada escritura no <see cref="ItemUseManager"/>
    ///      existente (mesmo pipeline F08; sem fluxo de consumo paralelo).
    /// </summary>
    public sealed class FarmLotRuntimeBootstrap : MonoBehaviour
    {
        private const string GameObjectName = "FarmLotRuntimeBootstrap";
        private const int MaxBindAttempts = 120;

        private static FarmLotRuntimeBootstrap _instance;

        private DeedUseHandler _deedHandler;
        private bool _handlersRegistered;

        public static FarmLotRuntimeBootstrap Instance => _instance;
        public bool IsFullyBound => _handlersRegistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject(GameObjectName);
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<FarmLotRuntimeBootstrap>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureService();
            StartCoroutine(BindWhenReady());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // Cria um FarmLotService apenas se ainda não existe (o gerador de cena pode tê-lo criado
        // já com o FarmLotSceneBinding wired). Não duplica.
        private void EnsureService()
        {
            if (FarmLotService.Instance != null)
            {
                return;
            }

            var go = new GameObject("FarmLotService");
            DontDestroyOnLoad(go);
            go.AddComponent<FarmLotService>();
        }

        private IEnumerator BindWhenReady()
        {
            var attempts = 0;
            while (attempts < MaxBindAttempts && !_handlersRegistered)
            {
                TryRegisterHandlers();
                if (_handlersRegistered)
                {
                    Debug.Log("[FarmLotRuntimeBootstrap] Escrituras de lote registradas no ItemUseManager.", this);
                    yield break;
                }

                attempts++;
                yield return null;
            }

            if (!_handlersRegistered)
            {
                Debug.LogWarning(
                    $"[FarmLotRuntimeBootstrap] Não registrou as escrituras após {MaxBindAttempts} tentativas. " +
                    "ItemUseManager indisponível; escrituras podem ficar inertes até ele existir.", this);
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

            if (_deedHandler == null)
            {
                _deedHandler = ScriptableObject.CreateInstance<DeedUseHandler>();
            }

            foreach (var def in FarmLotCatalog.All)
            {
                useManager.RegisterHandler(def.DeedItemId, _deedHandler);
            }

            _handlersRegistered = true;
        }
    }
}
