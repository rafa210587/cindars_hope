namespace CindarsHope.Foundation
{
    /// <summary>Committed logical location only; no Unity or save DTO references.</summary>
    public interface ITownInteriorLocationRuntime
    {
        bool IsTransitioning { get; }
        TownInteriorLocationSnapshot CaptureCommitted();
        bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, out string reason);
        // The save caller owns the token so asynchronous completion cannot match another visit.
        bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, string operationId, out string reason);
    }
}
