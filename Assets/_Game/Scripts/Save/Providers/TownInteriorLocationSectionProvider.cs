using System;
using CindarsHope.Foundation;

namespace CindarsHope.Save.Providers
{
    /// <summary>Persists one committed interior location without querying runtime more than once.</summary>
    public sealed class TownInteriorLocationSectionProvider : ISaveSectionProvider
    {
        private readonly Func<TownInteriorLocationSnapshot?> _captureCommitted;
        private readonly Action<TownInteriorLocationSnapshot> _restoreCommitted;

        public TownInteriorLocationSectionProvider(
            Func<TownInteriorLocationSnapshot?> captureCommitted,
            Action<TownInteriorLocationSnapshot> restoreCommitted)
        {
            _captureCommitted = captureCommitted;
            _restoreCommitted = restoreCommitted;
        }

        public string ProviderId => "town_interior_location";

        public object Capture(GameSaveData existingSaveData)
        {
            var snapshot = _captureCommitted?.Invoke();
            if (!snapshot.HasValue || !snapshot.Value.IsInside) return null;
            if (!snapshot.Value.TryValidate(out var reason))
                throw new InvalidOperationException($"Cannot capture town interior location: {reason}");

            var value = snapshot.Value;
            return new TownInteriorLocationSaveData
            {
                HouseId = value.HouseId,
                AnchorId = value.AnchorId,
                ReturnAnchorId = value.ReturnAnchorId,
                LocalX = value.LocalX,
                LocalY = value.LocalY,
                FacingX = value.FacingX,
                FacingY = value.FacingY,
                SafeTownX = value.SafeTownX,
                SafeTownY = value.SafeTownY
            };
        }

        public void Restore(object sectionData)
        {
            var data = sectionData as TownInteriorLocationSaveData;
            if (data == null) return;
            if (!TownInteriorLocationSaveData.TryDecode(data, out var snapshot, out var reason))
                throw new ArgumentException($"Cannot restore town interior location: {reason}", nameof(sectionData));
            _restoreCommitted?.Invoke(snapshot);
        }
    }
}
