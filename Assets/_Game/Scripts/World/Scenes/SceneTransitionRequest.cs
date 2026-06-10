namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// Request DTO for a scene transition.
    /// All fields are simple strings — no Unity references allowed.
    /// </summary>
    public sealed class SceneTransitionRequest
    {
        /// <summary>Scene ID of the origin scene (from SceneId constants).</summary>
        public readonly string SourceSceneId;

        /// <summary>Scene ID of the destination scene (from SceneId constants).</summary>
        public readonly string DestinationSceneId;

        /// <summary>
        /// Stable spawn anchor ID in the destination scene.
        /// Must match a SceneSpawnPoint._spawnId or SceneSpawnAnchor._spawnAnchorId.
        /// </summary>
        public readonly string TargetSpawnAnchorId;

        /// <summary>Logical gate ID that initiated this transition (for auditing).</summary>
        public readonly string InitiatingGateId;

        public SceneTransitionRequest(
            string sourceSceneId,
            string destinationSceneId,
            string targetSpawnAnchorId,
            string initiatingGateId = "")
        {
            SourceSceneId = sourceSceneId ?? string.Empty;
            DestinationSceneId = destinationSceneId ?? string.Empty;
            TargetSpawnAnchorId = targetSpawnAnchorId ?? string.Empty;
            InitiatingGateId = initiatingGateId ?? string.Empty;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(DestinationSceneId)
                   && !string.IsNullOrWhiteSpace(TargetSpawnAnchorId);
        }

        public override string ToString()
        {
            return $"SceneTransitionRequest [{SourceSceneId} → {DestinationSceneId} @{TargetSpawnAnchorId}]";
        }
    }
}
