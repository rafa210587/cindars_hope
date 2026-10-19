using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.UI.Modal
{
    public abstract class ModalBase : MonoBehaviour
    {
        private ModalManager _modalManager;

        public abstract ModalType ModalType { get; }

        public virtual void InitializeModal(ModalManager modalManager)
        {
            _modalManager = modalManager;
        }

        public virtual void ShowModal()
        {
            gameObject.SetActive(true);
        }

        public virtual void CloseModal()
        {
            if (_modalManager != null)
            {
                _modalManager.TryPopModal(ModalType, out _);
            }

            Destroy(gameObject);
        }
    }
}
