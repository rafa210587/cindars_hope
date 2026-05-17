using UnityEngine;

namespace CindarsHope.Save
{
    [DisallowMultipleComponent]
    public class SaveInput : MonoBehaviour
    {
        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private KeyCode _saveKey = KeyCode.F5;
        [SerializeField] private KeyCode _loadKey = KeyCode.F9;

        private bool _missingSaveManagerWarningLogged;

        private void Awake()
        {
            EnsureSaveManager();
        }

        private void Reset()
        {
            _saveManager = GetComponent<SaveManager>();
        }

        private void OnValidate()
        {
            if (_saveManager == null)
            {
                _saveManager = GetComponent<SaveManager>();
            }
        }

        private void Update()
        {
            if (!EnsureSaveManager())
            {
                return;
            }

            if (Input.GetKeyDown(_saveKey))
            {
                _saveManager.SaveGame();
            }

            if (Input.GetKeyDown(_loadKey))
            {
                _saveManager.LoadGame();
            }
        }

        private bool EnsureSaveManager()
        {
            if (_saveManager == null)
            {
                _saveManager = GetComponent<SaveManager>();
            }

            if (_saveManager != null)
            {
                return true;
            }

            if (!_missingSaveManagerWarningLogged)
            {
                Debug.LogWarning($"{nameof(SaveInput)} on '{name}' has no SaveManager assigned.", this);
                _missingSaveManagerWarningLogged = true;
            }

            return false;
        }
    }
}
