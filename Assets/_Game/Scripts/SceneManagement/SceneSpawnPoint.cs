using UnityEngine;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId;

        public string SpawnId => _spawnId;
        public Vector2 Position => transform.position;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_spawnId))
            {
                Debug.LogWarning($"{nameof(SceneSpawnPoint)} on '{name}' has no spawn id assigned.", this);
            }
        }
    }
}
