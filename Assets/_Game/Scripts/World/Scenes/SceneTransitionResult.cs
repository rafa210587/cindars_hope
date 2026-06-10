namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// Result DTO returned after a scene transition attempt.
    /// All fields are simple types — no Unity references.
    /// </summary>
    public sealed class SceneTransitionResult
    {
        public static readonly SceneTransitionResult Success = new SceneTransitionResult(true, string.Empty);

        public readonly bool Succeeded;
        public readonly string Reason;

        public SceneTransitionResult(bool succeeded, string reason)
        {
            Succeeded = succeeded;
            Reason = reason ?? string.Empty;
        }

        public static SceneTransitionResult Failure(string reason)
        {
            return new SceneTransitionResult(false, reason ?? "Unknown failure");
        }

        public override string ToString()
        {
            return Succeeded
                ? "SceneTransitionResult [SUCCESS]"
                : $"SceneTransitionResult [FAILURE: {Reason}]";
        }
    }
}
