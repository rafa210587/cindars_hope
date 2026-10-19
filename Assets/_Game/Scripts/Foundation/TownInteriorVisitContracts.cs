using System;

namespace CindarsHope.Foundation
{
    public readonly struct TownInteriorRecoveryRequiredEvent
    {
        public readonly string OperationId;
        public TownInteriorRecoveryRequiredEvent(string operationId) => OperationId = operationId;
    }

    public readonly struct TownInteriorRecoveryRetryRequestedEvent
    {
        public readonly string OperationId;
        public TownInteriorRecoveryRetryRequestedEvent(string operationId) => OperationId = operationId;
    }

    public interface IPlayerFacingSource
    {
        float FacingX { get; }
        float FacingY { get; }
    }

    public readonly struct TownInteriorRestoreStateRequestedEvent
    {
        public readonly string OperationId;
        public TownInteriorRestoreStateRequestedEvent(string operationId) => OperationId = operationId;
    }

    public readonly struct TownInteriorRestoreStateReadyEvent
    {
        public readonly string OperationId;
        public readonly bool Success;
        public TownInteriorRestoreStateReadyEvent(string operationId, bool success)
        { OperationId = operationId; Success = success; }
    }

    public interface ITownInteriorVisitAdmission
    {
        bool CanBegin(TownInteriorVisitRequest request, out string reason);
    }

    public readonly struct TownInteriorBlackoutRequestedEvent
    {
        public readonly string OperationId;
        public readonly float FadeOutSeconds, FadeInSeconds, LoadingHintAfterSeconds;
        public TownInteriorBlackoutRequestedEvent(string operationId, float fadeOutSeconds,
            float fadeInSeconds, float loadingHintAfterSeconds)
        { OperationId = operationId; FadeOutSeconds = fadeOutSeconds;
          FadeInSeconds = fadeInSeconds; LoadingHintAfterSeconds = loadingHintAfterSeconds; }
    }

    // Cancel invalidates forward callbacks but keeps blackout until rollback placement is ready.
    public readonly struct TownInteriorVisitCancelledEvent
    {
        public readonly string OperationId;
        public TownInteriorVisitCancelledEvent(string operationId) => OperationId = operationId;
    }

    public readonly struct TownInteriorCameraBoundsChangedEvent
    {
        public readonly string OperationId;
        public readonly bool HasBounds;
        public readonly float MinX, MinY, MaxX, MaxY;
        public TownInteriorCameraBoundsChangedEvent(string operationId, bool hasBounds,
            float minX, float minY, float maxX, float maxY)
        { OperationId = operationId; HasBounds = hasBounds;
          MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY; }
    }

    public enum TownVisitPhase { Idle, Opening, Armed, FadingOut, Loading, Placing, FadingIn, RollingBack }

    public readonly struct TownInteriorVisitRequest
    {
        public readonly string OperationId, HouseId, EntryAnchorId, ReturnAnchorId;
        public readonly bool Exiting;

        public TownInteriorVisitRequest(string operationId, string houseId, string entryAnchorId,
            string returnAnchorId, bool exiting)
        {
            OperationId = Required(operationId, nameof(operationId));
            HouseId = Required(houseId, nameof(houseId));
            EntryAnchorId = Required(entryAnchorId, nameof(entryAnchorId));
            ReturnAnchorId = Required(returnAnchorId, nameof(returnAnchorId));
            Exiting = exiting;
        }

        private static string Required(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A stable ID is required.", name);
            return value.Trim();
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(OperationId) && !string.IsNullOrWhiteSpace(HouseId) &&
            !string.IsNullOrWhiteSpace(EntryAnchorId) && !string.IsNullOrWhiteSpace(ReturnAnchorId);
    }

    public readonly struct TownInteriorLocationSnapshot
    {
        public readonly string HouseId, AnchorId, ReturnAnchorId;
        public readonly float LocalX, LocalY, FacingX, FacingY, SafeTownX, SafeTownY;
        public bool IsInside => !string.IsNullOrEmpty(HouseId);

        public TownInteriorLocationSnapshot(string houseId, string anchorId, string returnAnchorId,
            float localX, float localY, float facingX, float facingY, float safeTownX, float safeTownY)
        {
            HouseId = houseId?.Trim() ?? string.Empty;
            AnchorId = anchorId?.Trim() ?? string.Empty;
            ReturnAnchorId = returnAnchorId?.Trim() ?? string.Empty;
            LocalX = localX; LocalY = localY; FacingX = facingX; FacingY = facingY;
            SafeTownX = safeTownX; SafeTownY = safeTownY;
        }

        public bool TryValidate(out string reason)
        {
            if (!Finite(LocalX) || !Finite(LocalY) || !Finite(FacingX) || !Finite(FacingY) ||
                !Finite(SafeTownX) || !Finite(SafeTownY))
            {
                reason = "Interior location contains a non-finite position or facing.";
                return false;
            }
            if (IsInside && (string.IsNullOrWhiteSpace(HouseId) || string.IsNullOrWhiteSpace(AnchorId) ||
                string.IsNullOrWhiteSpace(ReturnAnchorId)))
            {
                reason = "Interior location requires house, entry and return anchor IDs.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public readonly struct TownInteriorVisitRequestedEvent
    {
        public readonly TownInteriorVisitRequest Request;
        public TownInteriorVisitRequestedEvent(TownInteriorVisitRequest request) => Request = request;
    }
    public readonly struct TownInteriorBlackoutReadyEvent
    {
        public readonly string OperationId;
        public TownInteriorBlackoutReadyEvent(string operationId) => OperationId = operationId;
    }
    public readonly struct TownInteriorPlacementReadyEvent
    {
        public readonly string OperationId, HouseId;
        public TownInteriorPlacementReadyEvent(string operationId, string houseId)
        { OperationId = operationId; HouseId = houseId; }
    }
    public readonly struct TownInteriorVisitFinishedEvent
    {
        public readonly string OperationId, HouseId, Reason;
        public readonly bool Success;
        public TownInteriorVisitFinishedEvent(string operationId, string houseId, bool success, string reason)
        { OperationId = operationId; HouseId = houseId; Success = success; Reason = reason ?? string.Empty; }
    }
    public readonly struct TownInteriorPlayerLockEvent
    {
        public readonly string OperationId;
        public readonly bool Locked;
        public TownInteriorPlayerLockEvent(string operationId, bool locked)
        { OperationId = operationId; Locked = locked; }
    }
    public readonly struct TownInteriorContextChangedEvent
    {
        public readonly string OperationId, HouseId;
        public readonly bool IsInside;
        public TownInteriorContextChangedEvent(string operationId, string houseId, bool isInside)
        { OperationId = operationId; HouseId = houseId ?? string.Empty; IsInside = isInside; }
    }
    public readonly struct TownInteriorPlayerPlacementRequestedEvent
    {
        public readonly string OperationId;
        public readonly float WorldX, WorldY, FacingX, FacingY;
        public TownInteriorPlayerPlacementRequestedEvent(string operationId, float worldX, float worldY,
            float facingX, float facingY)
        { OperationId = operationId; WorldX = worldX; WorldY = worldY; FacingX = facingX; FacingY = facingY; }
    }
    public readonly struct TownInteriorPlayerPlacementCompletedEvent
    {
        public readonly string OperationId;
        public readonly bool Success;
        public TownInteriorPlayerPlacementCompletedEvent(string operationId, bool success)
        { OperationId = operationId; Success = success; }
    }
    public readonly struct TownInteriorFadeFinishedEvent
    {
        public readonly string OperationId;
        public TownInteriorFadeFinishedEvent(string operationId) => OperationId = operationId;
    }
}
