using System.Collections.Generic;
using CindarsHope.Farm.Crops;

namespace CindarsHope.Farm.Processing
{
    public enum ProcessingJobState
    {
        Queued = 0,
        Processing = 1,
        ReadyToCollect = 2,
        Collected = 3,
        Cancelled = 4
    }

    public class FarmProcessingJob
    {
        public string JobId { get; set; }
        public string StationId { get; set; }
        public string InputItemId { get; set; }
        public int InputQuantity { get; set; }
        public string OutputItemId { get; set; }
        public int OutputQuantity { get; set; }
        public CropQualityTier InputQuality { get; set; }
        public CropQualityTier OutputQuality { get; set; }
        public int StartDay { get; set; }
        public int FinishDay { get; set; }
        public ProcessingJobState State { get; set; } = ProcessingJobState.Queued;
        public bool OutputCollected { get; set; }

        public bool IsReadyOnDay(int currentDay)
        {
            return State == ProcessingJobState.Processing && currentDay >= FinishDay;
        }

        public void AdvanceToReady()
        {
            if (State == ProcessingJobState.Processing)
                State = ProcessingJobState.ReadyToCollect;
        }

        public bool Collect()
        {
            if (State != ProcessingJobState.ReadyToCollect || OutputCollected)
                return false;
            State = ProcessingJobState.Collected;
            OutputCollected = true;
            return true;
        }
    }
}
