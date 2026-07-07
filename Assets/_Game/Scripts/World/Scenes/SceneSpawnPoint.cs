using UnityEngine;

namespace CindarsHope.World.Scenes
{
    [DisallowMultipleComponent]
    public sealed class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId;

        public string SpawnId => _spawnId;
        public Vector2 Position => transform.position;

        private void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(_spawnId))
            {
                _spawnId = _spawnId.Trim();
            }
        }
    }
}
