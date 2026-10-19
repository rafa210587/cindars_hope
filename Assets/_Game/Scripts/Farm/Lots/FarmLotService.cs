using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — dono em runtime do estado dos lotes de expansão {Locked, Owned}.
    ///
    /// - <see cref="UseDeed"/> destrava um lote (idempotente; refs serializadas; sem Find), publica
    ///   <see cref="FarmLotUnlockedEvent"/> e aplica o estado visual via <see cref="FarmLotSceneBinding"/>.
    /// - Capture/Restore persistem APENAS IDs (campo aditivo na seção farm do save; ADR-0006).
    /// - Comunicação por <see cref="GameEventBus"/> (sem chamadas diretas entre sistemas de gameplay).
    ///
    /// Decisão 5.3: a posse do lote é gated APENAS por dinheiro/recursos (compra da escritura) —
    /// nenhum gate de caverna. O serviço não conhece a caverna.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FarmLotService : MonoBehaviour
    {
        [SerializeField] private FarmLotSceneBinding _sceneBinding;

        private readonly FarmLotProgression _progression = new FarmLotProgression();

        private static FarmLotService _instance;
        public static FarmLotService Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void Start()
        {
            // Reaplica o estado inicial (tudo Locked por default) à cena, caso o binding exista.
            ApplyAllVisualStates();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Liga a ponte de cena (chamado pelo gerador/installer). Sem Find em runtime.</summary>
        public void BindScene(FarmLotSceneBinding sceneBinding)
        {
            _sceneBinding = sceneBinding;
            ApplyAllVisualStates();
        }

        public FarmLotState GetState(string lotId) => _progression.GetState(lotId);

        public bool IsOwned(string lotId) => _progression.IsOwned(lotId);

        /// <summary>
        /// Destrava o lote referente à escritura/lote informado. Idempotente: a 2ª chamada (re-uso
        /// da escritura ou reload) retorna false e NÃO republica o evento nem reativa conteúdo.
        /// Retorna true só na transição Locked→Owned.
        /// </summary>
        public bool UseDeed(string lotId)
        {
            if (!_progression.Unlock(lotId))
            {
                return false;
            }

            ApplyVisualState(lotId);

            var displayName = FarmLotCatalog.TryGetByLotId(lotId, out var def) ? def.DisplayName : lotId;
            GameEventBus.Publish(new FarmLotUnlockedEvent(lotId, displayName));
            Debug.Log($"[FarmLotService] Lote '{lotId}' destravado.", this);
            return true;
        }

        /// <summary>Destrava pelo id do item de escritura (usado pelo handler de use-item).</summary>
        public bool UseDeedByItemId(string deedItemId)
        {
            var lotId = FarmLotCatalog.ResolveLotIdForDeed(deedItemId);
            return lotId != null && UseDeed(lotId);
        }

        public FarmLotsSaveData CaptureSaveData() => _progression.Capture();

        public void RestoreFromSaveData(FarmLotsSaveData saveData)
        {
            _progression.Restore(saveData);
            ApplyAllVisualStates();
            var count = saveData?.OwnedLots != null ? saveData.OwnedLots.Count : 0;
            Debug.Log($"[FarmLotService] Restaurados {count} lote(s) do save.", this);
        }

        private void ApplyAllVisualStates()
        {
            if (_sceneBinding == null)
            {
                return;
            }

            foreach (var lotId in FarmLotId.All)
            {
                _sceneBinding.ApplyVisualState(lotId, _progression.GetState(lotId));
            }
        }

        private void ApplyVisualState(string lotId)
        {
            if (_sceneBinding == null)
            {
                return;
            }

            _sceneBinding.ApplyVisualState(lotId, _progression.GetState(lotId));
        }
    }
}
