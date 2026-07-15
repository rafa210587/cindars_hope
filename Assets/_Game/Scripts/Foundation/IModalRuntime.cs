// arch: quebra do par mutuo Core|UI (2026-07-15) — port puro (sem dependencia de engine) que permite
// a GameBootstrap.ModalManager (Core) expor push/pop/clear de modal para os consumidores existentes
// sem nomear CindarsHope.UI.Modal.ModalManager. ModalManager (em CindarsHope.UI.Modal) implementa
// esta interface com as mesmas assinaturas (implementacao implicita, sem mudanca de logica).
namespace CindarsHope.Foundation
{
    public interface IModalRuntime
    {
        bool HasActiveModal { get; }
        ModalType CurrentModal { get; }
        bool PushModal(ModalType modalType);
        bool TryPopModal(ModalType expectedType, out ModalType poppedModal);
        bool TryPopIfCurrent(ModalType expectedType);
        void ClearAllModals();
    }
}
