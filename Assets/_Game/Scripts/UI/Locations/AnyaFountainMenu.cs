using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player.Death;
using CindarsHope.Skills;
using CindarsHope.UI.Modal;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Locations
{
    public class AnyaFountainMenu : ModalBase
    {
        [SerializeField] private Button _returnToCaveButton;
        [SerializeField] private Button _respecButton;
        [SerializeField] private Text _respecCostText;
        [SerializeField] private Button _exitButton;

        private AnyaRespawnService _respawnService;

        public override ModalType ModalType => ModalType.AnyaFountain;

        private void OnEnable()
        {
            if (_returnToCaveButton != null)
                _returnToCaveButton.onClick.AddListener(OnReturnToCaveClicked);

            if (_respecButton != null)
                _respecButton.onClick.AddListener(OnRespecClicked);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);

            RefreshRespecButton();
        }

        private void OnDisable()
        {
            if (_returnToCaveButton != null)
                _returnToCaveButton.onClick.RemoveListener(OnReturnToCaveClicked);
            if (_respecButton != null)
                _respecButton.onClick.RemoveListener(OnRespecClicked);
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(OnExitClicked);
        }

        public void Initialize(AnyaRespawnService respawnService)
        {
            _respawnService = respawnService;
            RefreshRespecButton();
        }

        private void RefreshRespecButton()
        {
            var skillMgr = SkillTreeManager.Instance;
            if (_respecButton == null) return;

            if (skillMgr == null)
            {
                _respecButton.interactable = false;
                return;
            }

            _respecButton.interactable = true;
            int cost = skillMgr.GetRespecCost();

            if (_respecCostText != null)
                _respecCostText.text = cost == 0 ? "Respec Gratuito" : $"Respec: {cost}g";
        }

        private void OnReturnToCaveClicked()
        {
            CloseModal();
        }

        private void OnRespecClicked()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null) return;

            var skillMgr = SkillTreeManager.Instance;
            var playerMgr = bootstrap.PlayerManager;
            var progMgr = bootstrap.PlayerProgressionManager;

            if (skillMgr == null || playerMgr == null || progMgr == null)
            {
                Debug.LogWarning("[AnyaFountainMenu] Missing managers for respec.");
                return;
            }

            int gold = playerMgr.CurrentGold;
            int level = progMgr.Level;

            if (skillMgr.TryRespec(ref gold, level))
            {
                playerMgr.SetGold(gold);
                RefreshRespecButton();
            }
        }

        private void OnExitClicked()
        {
            CloseModal();
        }
    }
}
