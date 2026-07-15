using CindarsHope.Core.Bootstrap;
using CindarsHope.Foundation;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Skills;
using UnityEngine;

namespace CindarsHope.UI.Skills
{
    // U is reserved for skill trees; K opens character/equipment in the gameplay MVP.
    [DisallowMultipleComponent]
    public class SkillTreeInputHandler : MonoBehaviour
    {
        [SerializeField] private SkillTreePanel _skillTreePanelPrefab;

        private void Update()
        {
            if (!global::UnityEngine.Input.GetKeyDown(KeyCode.U)) return;

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null) return;

            // arch: Core|UI (2026-07-15) — GameBootstrap.ModalManager agora e IModalRuntime (porta);
            // cast para o tipo concreto porque este handler usa OpenModal<T>, nao portavel.
            // SkillTreeInputHandler ja e do modulo UI, entao isso nao afeta o par Core|UI.
            var modal = bootstrap.ModalManager as ModalManager;
            if (modal == null) return;

            // If skill tree is already open, it will handle its own close via its Update.
            // If another modal is active (not skill tree), block.
            if (modal.HasActiveModal && modal.CurrentModal != ModalType.SkillTree) return;
            if (modal.HasActiveModal && modal.CurrentModal == ModalType.SkillTree) return;

            if (_skillTreePanelPrefab == null)
            {
                Debug.LogWarning("SkillTreeInputHandler: SkillTreePanel prefab not assigned.", this);
                return;
            }

            modal.OpenModal(_skillTreePanelPrefab);
        }
    }
}
