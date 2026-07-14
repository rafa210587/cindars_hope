using CindarsHope.Player.Death;

namespace CindarsHope.World
{
    /// <summary>
    /// Porta local do modulo World para abrir o modal de recovery de corpse em uma UI concreta
    /// sem depender do tipo concreto do modulo de UI de morte.
    /// </summary>
    public interface ICorpseRecoveryPresenter
    {
        void OpenRecoveryModal(Corpse corpse);
    }
}
