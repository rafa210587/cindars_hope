using System.Collections.Generic;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — modelo PURO do estado de posse dos lotes (sem Unity, testável em EditMode).
    ///
    /// Responsável por: estado por lote {Locked, Owned}, destravamento idempotente, e
    /// captura/restauração para save. Toda a lógica de cena (cerca/conteúdo/placa) vive no
    /// FarmLotService (MonoBehaviour) e apenas consome o resultado de Unlock().
    ///
    /// Invariantes:
    ///  - lotes desconhecidos nascem e permanecem Locked (default);
    ///  - Unlock retorna true só na PRIMEIRA vez (transição Locked→Owned); 2ª chamada = false (no-op);
    ///  - Restore aceita lista possivelmente vazia/legada → todos os ausentes ficam Locked.
    /// </summary>
    public sealed class FarmLotProgression
    {
        private readonly Dictionary<string, FarmLotState> _states = new Dictionary<string, FarmLotState>();

        public FarmLotProgression()
        {
            foreach (var lotId in FarmLotId.All)
            {
                _states[lotId] = FarmLotState.Locked;
            }
        }

        public FarmLotState GetState(string lotId)
        {
            if (FarmLotId.IsKnown(lotId) && _states.TryGetValue(lotId, out var state))
            {
                return state;
            }

            return FarmLotState.Locked;
        }

        public bool IsOwned(string lotId) => GetState(lotId) == FarmLotState.Owned;

        public bool IsLocked(string lotId) => GetState(lotId) == FarmLotState.Locked;

        /// <summary>
        /// Destrava um lote. Idempotente: retorna true apenas na transição Locked→Owned;
        /// chamadas subsequentes (re-uso da escritura, reload) retornam false sem duplicar efeito.
        /// Lote desconhecido nunca destrava (retorna false).
        /// </summary>
        public bool Unlock(string lotId)
        {
            if (!FarmLotId.IsKnown(lotId))
            {
                return false;
            }

            if (_states.TryGetValue(lotId, out var current) && current == FarmLotState.Owned)
            {
                return false;
            }

            _states[lotId] = FarmLotState.Owned;
            return true;
        }

        /// <summary>Lista (cópia) dos lotes Owned, para save.</summary>
        public List<string> GetOwnedLotIds()
        {
            var owned = new List<string>();
            foreach (var lotId in FarmLotId.All)
            {
                if (_states.TryGetValue(lotId, out var state) && state == FarmLotState.Owned)
                {
                    owned.Add(lotId);
                }
            }

            return owned;
        }

        public FarmLotsSaveData Capture()
        {
            return new FarmLotsSaveData { OwnedLots = GetOwnedLotIds() };
        }

        /// <summary>
        /// Restaura a posse a partir do save. Reset para Locked e re-aplica apenas os IDs salvos
        /// que são conhecidos. save == null ou lista ausente (legado) ⇒ tudo Locked (CA-4).
        /// Idempotente: restaurar a mesma lista 2× mantém o mesmo conjunto Owned.
        /// </summary>
        public void Restore(FarmLotsSaveData save)
        {
            foreach (var lotId in FarmLotId.All)
            {
                _states[lotId] = FarmLotState.Locked;
            }

            if (save?.OwnedLots == null)
            {
                return;
            }

            foreach (var lotId in save.OwnedLots)
            {
                if (FarmLotId.IsKnown(lotId))
                {
                    _states[lotId] = FarmLotState.Owned;
                }
            }
        }
    }
}
