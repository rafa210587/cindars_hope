using UnityEngine;

namespace CindarsHope.Core.Events
{
    public readonly struct SceneTransitionCompletedEvent
    {
        public readonly string SceneName;
        public readonly string SpawnId;
        public readonly Vector2 PlayerPosition;

        public SceneTransitionCompletedEvent(string sceneName, string spawnId, Vector2 playerPosition)
        {
            SceneName = sceneName;
            SpawnId = spawnId;
            PlayerPosition = playerPosition;
        }
    }
}
