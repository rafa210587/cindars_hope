namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Building construction job state.
    /// </summary>
    public enum ConstructionState
    {
        /// <summary>
        /// Not yet started; waiting for player confirmation/cost payment.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Under construction; waiting for build time to complete.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Construction complete; building is functional.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Cancelled or demolished; construction reverted.
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Building is paused (deferred for future gameplay).
        /// </summary>
        Paused = 4
    }
}
