namespace CindarsHope.Foundation
{
    /// <summary>
    /// Runtime port for effects that own precise movement-control timing while retaining a
    /// canonical status token for UI and gameplay queries.
    /// </summary>
    public interface IStatusMovementOverrideRuntime
    {
        bool SuppressesStatusMovement(string statusId, string sourceId);
    }
}
