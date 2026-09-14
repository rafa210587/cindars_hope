namespace CindarsHope.Foundation
{
    /// <summary>
    /// Opt-in access decision for a physical walk-in house door. The door owns animation,
    /// occupancy safety and feedback; a policy only answers whether opening is allowed now.
    /// </summary>
    public interface IHouseDoorAccessPolicy
    {
        bool CanOpen(out string unavailableReason);
    }
}
