namespace CindarsHope.Items
{
    /// <summary>The concrete effect a magic "use" item produces, resolved by <see cref="MagicItemUseService"/>.</summary>
    public enum MagicUseEffect
    {
        None = 0,
        WardEnemies = 1,        // bell_of_warding: enemies in radius flee for a few seconds
        ReturnToEntrance = 2,   // mirror_of_return: teleport to the CURRENT level entrance (no regen)
        AdvanceToDawn = 3,      // hourglass_of_dawn: advance time to 06:00
        FreeRepair = 4          // whetstone_eternal: one free repair, gated once per day
    }

    public readonly struct MagicUseResult
    {
        public bool Consumed { get; }              // whether the item should be consumed (removed) on use
        public MagicUseEffect Effect { get; }
        public string Reason { get; }              // human-readable diagnostic when not consumed

        public MagicUseResult(bool consumed, MagicUseEffect effect, string reason)
        {
            Consumed = consumed;
            Effect = effect;
            Reason = reason;
        }
    }

    /// <summary>
    /// Time surface needed by the hourglass + whetstone-cooldown logic. Implemented by the real TimeManager
    /// adapter and by test fakes. CurrentDay is the absolute day counter; AdvanceToDawn skips to 06:00.
    /// </summary>
    public interface IMagicTimeGateway
    {
        int CurrentDay { get; }
        void AdvanceToDawn();
    }

    /// <summary>
    /// fable_31 — PURE resolution of the four magic "use" effects (bell / mirror / hourglass / whetstone).
    /// Routed at runtime through the existing F08 ItemUseManager pipeline (no parallel consumption flow); the
    /// heavy lifting (teleport / time skip / repair) is delegated to named gateways so this stays testable.
    ///
    /// Whetstone is gated to once per in-game day: the service tracks the last day it fired and refuses a
    /// second use the same day WITHOUT consuming the item (cooldown derives from the calendar — no save field).
    /// </summary>
    public sealed class MagicItemUseService
    {
        private int _whetstoneLastUsedDay = int.MinValue;

        /// <summary>For save-less restore/testing: seed the last day the free repair fired.</summary>
        public void SetWhetstoneLastUsedDay(int day) => _whetstoneLastUsedDay = day;

        public int WhetstoneLastUsedDay => _whetstoneLastUsedDay;

        /// <summary>
        /// Resolves the effect for a magic "use" item. <paramref name="currentDay"/> drives the whetstone
        /// daily gate. Returns Consumed=false (with a Reason) when the use is refused so the caller does not
        /// remove the item from the inventory.
        /// </summary>
        public MagicUseResult Resolve(string itemId, int currentDay)
        {
            switch (itemId)
            {
                case MagicItemCatalog.BellOfWarding:
                    return new MagicUseResult(true, MagicUseEffect.WardEnemies, string.Empty);

                case MagicItemCatalog.MirrorOfReturn:
                    return new MagicUseResult(true, MagicUseEffect.ReturnToEntrance, string.Empty);

                case MagicItemCatalog.HourglassOfDawn:
                    return new MagicUseResult(true, MagicUseEffect.AdvanceToDawn, string.Empty);

                case MagicItemCatalog.WhetstoneEternal:
                    if (currentDay == _whetstoneLastUsedDay)
                    {
                        return new MagicUseResult(false, MagicUseEffect.None, "whetstone_already_used_today");
                    }
                    _whetstoneLastUsedDay = currentDay;
                    return new MagicUseResult(true, MagicUseEffect.FreeRepair, string.Empty);

                default:
                    return new MagicUseResult(false, MagicUseEffect.None, "not_a_magic_use_item");
            }
        }
    }
}
