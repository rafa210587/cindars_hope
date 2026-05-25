using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Player.Death
{
    [DisallowMultipleComponent]
    public class PlayerDeathController : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;

        private bool _isDead;
        private string _currentSceneName;

        private void Start()
        {
            _currentSceneName = SceneManager.GetActiveScene().name;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<HPChangedEvent>(OnHPChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHPChanged);
        }

        private void OnHPChanged(HPChangedEvent evt)
        {
            if (_isDead || _playerManager.CurrentHP > 0)
            {
                return;
            }

            _isDead = true;
            GameEventBus.Publish(new PlayerDiedEvent { SceneName = _currentSceneName });
        }
    }
}
