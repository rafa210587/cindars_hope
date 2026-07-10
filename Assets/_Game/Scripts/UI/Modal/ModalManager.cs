using System.Collections.Generic;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.UI.Modal
{
    public enum ModalType
    {
        None,
        Dialogue,
        QuestOffer,
        ShopMenu,
        Buy,
        Sell,
        Crafting,
        Inventory,
        CorpseRecovery,
        AnyaFountain,
        SkillTree,
        CharacterEquipment,
        Pause,
        Death,
        CaveCheckpoint,
        QuestLog,
        // fable_56: confirmacoes da aba Sistema / titulo (Carregar, Sair, recuperacao de backup).
        SystemConfirm
    }

    [DisallowMultipleComponent]
    public sealed class ModalManager : MonoBehaviour, IModalStateProvider
    {
        private Stack<ModalType> _modalStack = new Stack<ModalType>();

        public bool IsInitialized { get; private set; }
        public bool HasActiveModal => _modalStack.Count > 0;
        public ModalType CurrentModal => HasActiveModal ? _modalStack.Peek() : ModalType.None;

        // arch: quebra do ciclo Core|UI (spec_arch_core_ui_cycle_reduction_v38) — self-registro no
        // DomainManagerRegistry como IModalStateProvider (molde Core|Inventory/Core|Player) para que
        // Core.GameTimeManager consulte HasActiveModal sem referenciar CindarsHope.UI.Modal.
        private void Awake()
        {
            DomainManagerRegistry.Register<IModalStateProvider>(this);
        }

        private void OnDestroy()
        {
            DomainManagerRegistry.Unregister<IModalStateProvider>(this);
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            _modalStack.Clear();
            IsInitialized = false;
        }

        public bool PushModal(ModalType modalType)
        {
            if (modalType == ModalType.None)
            {
                Debug.LogWarning("Cannot push ModalType.None");
                return false;
            }

            if (HasActiveModal)
            {
                if (CurrentModal == modalType)
                {
                    return true;
                }

                Debug.LogWarning($"Cannot show modal {modalType} while {CurrentModal} is active.");
                return false;
            }

            _modalStack.Push(modalType);
            Debug.Log($"Modal pushed: {modalType}. Stack size: {_modalStack.Count}");
            return true;
        }

        public bool TryPopModal(ModalType expectedType, out ModalType poppedModal)
        {
            poppedModal = ModalType.None;

            if (_modalStack.Count == 0)
            {
                return false;
            }

            var top = _modalStack.Peek();
            if (top != expectedType)
            {
                Debug.LogWarning($"Modal type mismatch: expected {expectedType}, but top is {top}");
                return false;
            }

            _modalStack.Pop();
            poppedModal = top;
            Debug.Log($"Modal popped: {top}. Stack size: {_modalStack.Count}");
            return true;
        }

        public bool TryPopIfCurrent(ModalType expectedType)
        {
            return CurrentModal == expectedType && TryPopModal(expectedType, out _);
        }

        public void ClearAllModals()
        {
            _modalStack.Clear();
            Debug.Log("All modals cleared");
        }

        public T OpenModal<T>(T prefab) where T : ModalBase
        {
            if (prefab == null)
            {
                Debug.LogWarning("Cannot open modal: prefab is null");
                return null;
            }

            if (!PushModal(prefab.ModalType))
            {
                Debug.LogWarning($"Cannot open modal {prefab.ModalType}: another modal is active or stack rejected the push");
                return null;
            }

            var instance = Instantiate(prefab, transform);
            instance.InitializeModal(this);
            instance.ShowModal();

            return instance;
        }
    }
}
