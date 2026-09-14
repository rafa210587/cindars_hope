using System;
using CindarsHope.Foundation;

namespace CindarsHope.Save
{
    [Serializable]
    public sealed class TownInteriorLocationSaveData
    {
        public int Version = 1;
        public string HouseId, AnchorId, ReturnAnchorId;
        public float LocalX, LocalY, FacingX, FacingY, SafeTownX, SafeTownY;

        public static bool TryDecode(TownInteriorLocationSaveData dto,
            out TownInteriorLocationSnapshot snapshot, out string reason)
        {
            snapshot = default;
            if (dto == null)
            {
                reason = "Town interior location data is null.";
                return false;
            }
            if (dto.Version != 1)
            {
                reason = $"Unsupported town interior location version '{dto.Version}'.";
                return false;
            }
            if (!ValidIdShape(dto.HouseId) || !ValidIdShape(dto.AnchorId) || !ValidIdShape(dto.ReturnAnchorId))
            {
                reason = "Town interior location contains a whitespace-only ID.";
                return false;
            }

            var candidate = new TownInteriorLocationSnapshot(dto.HouseId, dto.AnchorId, dto.ReturnAnchorId,
                dto.LocalX, dto.LocalY, dto.FacingX, dto.FacingY, dto.SafeTownX, dto.SafeTownY);
            if (!candidate.TryValidate(out reason)) return false;
            snapshot = candidate;
            reason = string.Empty;
            return true;
        }

        private static bool ValidIdShape(string value) =>
            string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value);
    }
}
