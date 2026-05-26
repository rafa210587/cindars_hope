using CindarsHope.Core.Bootstrap;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Skills;
using UnityEngine;

namespace CindarsHope.UI.Skills
{
    // Handles K key to open/close SkillTreePanel modal.
    [DisallowMultipleComponent]
    public class SkillTreeInputHandler : MonoBehaviour
    {
        [SerializeField] private SkillTreePanel _skillTreePanelPrefab;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.K)) return;

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null) return;

            var modal = bootstrap.ModalManager;
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
