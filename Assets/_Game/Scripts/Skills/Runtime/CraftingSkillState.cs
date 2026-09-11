using System;
using System.Collections.Generic;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>
    /// Shared, saveable owner of Living Forge daily charges. A reservation blocks another station
    /// for the same day, while consumption happens only after its enhanced output enters inventory.
    /// </summary>
    public sealed class CraftingSkillState
    {
        private readonly HashSet<int> _consumedDayIndices = new HashSet<int>();
        private readonly Dictionary<int, string> _pendingReservations =
            new Dictionary<int, string>();

        public int CurrentDayIndex { get; private set; } = 1;

        public void ObserveDay(int dayIndex)
        {
            if (dayIndex > 0)
                CurrentDayIndex = dayIndex;
        }

        public bool IsChargeConsumed(int dayIndex) =>
            dayIndex > 0 && _consumedDayIndices.Contains(dayIndex);

        public bool HasPendingReservation(int dayIndex) =>
            dayIndex > 0 && _pendingReservations.ContainsKey(dayIndex);

        public bool IsChargeAvailable(int dayIndex) =>
            dayIndex > 0 && !IsChargeConsumed(dayIndex) && !HasPendingReservation(dayIndex);

        public bool TryReserve(int dayIndex, string reservationToken)
        {
            if (!IsChargeAvailable(dayIndex) || string.IsNullOrWhiteSpace(reservationToken))
                return false;

            CurrentDayIndex = dayIndex;
            _pendingReservations.Add(dayIndex, reservationToken);
            return true;
        }

        public bool Commit(int dayIndex, string reservationToken)
        {
            if (dayIndex <= 0 || string.IsNullOrWhiteSpace(reservationToken) ||
                !_pendingReservations.TryGetValue(dayIndex, out var pendingToken) ||
                !string.Equals(pendingToken, reservationToken, StringComparison.Ordinal) ||
                _consumedDayIndices.Contains(dayIndex))
            {
                return false;
            }

            _pendingReservations.Remove(dayIndex);
            _consumedDayIndices.Add(dayIndex);
            return true;
        }

        public bool Release(int dayIndex, string reservationToken)
        {
            if (dayIndex <= 0 || string.IsNullOrWhiteSpace(reservationToken) ||
                !_pendingReservations.TryGetValue(dayIndex, out var pendingToken) ||
                !string.Equals(pendingToken, reservationToken, StringComparison.Ordinal))
            {
                return false;
            }

            return _pendingReservations.Remove(dayIndex);
        }

        public CraftingSkillSaveData CaptureSaveData()
        {
            var data = new CraftingSkillSaveData { CurrentDayIndex = CurrentDayIndex };
            data.ConsumedDayIndices.AddRange(_consumedDayIndices);
            data.ConsumedDayIndices.Sort();

            var days = new List<int>(_pendingReservations.Keys);
            days.Sort();
            foreach (var day in days)
            {
                data.PendingReservations.Add(new CraftingSkillReservationSaveData
                {
                    DayIndex = day,
                    ReservationToken = _pendingReservations[day]
                });
            }

            return data;
        }

        public void RestoreFromSaveData(CraftingSkillSaveData data)
        {
            // Consumption is monotonic for the live save slot: loading an older snapshot in the
            // same session must not rearm a charge that already produced an output.
            _pendingReservations.Clear();
            CurrentDayIndex = Math.Max(1, data?.CurrentDayIndex ?? 1);
            if (data == null)
                return;

            if (data.ConsumedDayIndices != null)
            {
                foreach (var day in data.ConsumedDayIndices)
                {
                    if (day > 0)
                        _consumedDayIndices.Add(day);
                }
            }

            if (data.PendingReservations == null)
                return;

            foreach (var reservation in data.PendingReservations)
            {
                if (reservation == null || reservation.DayIndex <= 0 ||
                    string.IsNullOrWhiteSpace(reservation.ReservationToken) ||
                    _consumedDayIndices.Contains(reservation.DayIndex) ||
                    _pendingReservations.ContainsKey(reservation.DayIndex))
                {
                    continue;
                }

                _pendingReservations.Add(reservation.DayIndex,
                    reservation.ReservationToken);
            }
        }
    }
}
