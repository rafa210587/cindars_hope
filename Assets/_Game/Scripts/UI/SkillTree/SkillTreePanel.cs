using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.SkillTree
{
    [DisallowMultipleComponent]
    public class SkillTreePanel : MonoBehaviour
    {
        [SerializeField] private Transform _skillNodesContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _titleLabel;

        private void OnEnable()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (_titleLabel != null)
            {
                _titleLabel.text = "Skill Tree";
            }
        }

        private void OnDisable()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }

        public void DisplaySkillTree()
        {
            gameObject.SetActive(true);
        }

        public void HideSkillTree()
        {
            gameObject.SetActive(false);
        }
    }
}
