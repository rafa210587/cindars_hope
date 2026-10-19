namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Active construction job tracking state and progress.
    /// </summary>
    public class ConstructionJob
    {
        /// <summary>
        /// Unique job ID (e.g., building instance ID).
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// Building being constructed.
        /// </summary>
        public FarmBuildingDefinition Building { get; set; }

        /// <summary>
        /// Current construction state.
        /// </summary>
        public ConstructionState State { get; set; } = ConstructionState.Pending;

        /// <summary>
        /// Building position X (may differ from placement grid if deferred).
        /// </summary>
        public float PositionX { get; set; }

        /// <summary>
        /// Building position Y.
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        /// Building rotation (0-3).
        /// </summary>
        public int Rotation { get; set; }

        /// <summary>
        /// Day construction started (or -1 if not started).
        /// </summary>
        public int StartDay { get; set; } = -1;

        /// <summary>
        /// Day construction will complete (or -1 if not computed).
        /// </summary>
        public int CompletionDay { get; set; } = -1;

        /// <summary>
        /// Days remaining until completion (computed from game time).
        /// </summary>
        public int DaysRemaining
        {
            get
            {
                if (CompletionDay < 0) return Building.BuildTimeDays;
                // Actual remaining days would be computed from current game day
                // This is a placeholder; real implementation uses GameCalendarService
                return Building.BuildTimeDays;
            }
        }

        public ConstructionJob(string jobId, FarmBuildingDefinition building, float posX, float posY, int rotation = 0)
        {
            JobId = jobId;
            Building = building;
            PositionX = posX;
            PositionY = posY;
            Rotation = rotation % 4;
        }

        /// <summary>
        /// Mark construction as started.
        /// </summary>
        public void StartConstruction(int currentDay)
        {
            if (State != ConstructionState.Pending) return;
            StartDay = currentDay;
            CompletionDay = currentDay + Building.BuildTimeDays;
            State = ConstructionState.InProgress;
        }

        /// <summary>
        /// Mark construction as completed.
        /// </summary>
        public void CompleteConstruction()
        {
            if (State != ConstructionState.InProgress) return;
            State = ConstructionState.Completed;
        }

        /// <summary>
        /// Cancel or demolish construction.
        /// </summary>
        public void Cancel()
        {
            State = ConstructionState.Cancelled;
        }
    }
}
