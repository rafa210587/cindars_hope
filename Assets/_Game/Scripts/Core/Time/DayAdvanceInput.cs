using UnityEngine;
using CindarsHope.Core.Bootstrap;

namespace CindarsHope.Core.Time
{
    [DisallowMultipleComponent]
    public class DayAdvanceInput : MonoBehaviour
    {
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private KeyCode _advanceDayKey = KeyCode.Tab;

        private bool _missingTimeManagerWarningLogged;

        private void Awake()
        {
            EnsureTimeManager();
        }

        private void Reset()
        {
            _timeManager = GetComponent<TimeManager>();
        }

        private void OnValidate()
        {
            if (_timeManager == null)
            {
                _timeManager = GetComponent<TimeManager>();
            }
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                return;
            }

            if (!Input.GetKeyDown(_advanceDayKey))
            {
                return;
            }

            if (!EnsureTimeManager())
            {
                return;
            }

            _timeManager.AdvanceDay();
        }

        private bool EnsureTimeManager()
        {
            if (_timeManager == null)
            {
                _timeManager = GetComponent<TimeManager>();
            }

            if (_timeManager != null)
            {
                return true;
            }

            if (!_missingTimeManagerWarningLogged)
            {
                Debug.LogWarning($"{nameof(DayAdvanceInput)} on '{name}' has no TimeManager assigned.", this);
                _missingTimeManagerWarningLogged = true;
            }

            return false;
        }
    }
}
