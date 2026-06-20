namespace CindarsHope.Localization
{
    /// <summary>
    /// Lightweight, pure id -> string resolution for quest/dialogue text authored from phase P4
    /// onward (ADR-0012 — Localization via String Table from Phase P4).
    ///
    /// This formalizes the ad-hoc <c>def.DisplayNameKey ?? def.QuestId</c> fallback already practiced
    /// in <see cref="CindarsHope.Quests.Log.QuestLogProjectionService"/> into a single tested service:
    /// when a key is missing/null/absent, the id itself is returned (deterministic, visible fallback —
    /// never an exception, never a surprise empty string).
    ///
    /// Constraints (ADR-0012): no localization framework, no runtime locale switch, no .po/.resx,
    /// no save persistence. The service is static and pure — no MonoBehaviour, no Unity reference,
    /// no GameObject.Find/FindObjectOfType, no GameEventBus channel (a lookup is a synchronous data
    /// read, not gameplay communication). Resolution is deterministic, idempotent and reentrant.
    /// </summary>
    public static class LocalizationService
    {
        /// <summary>
        /// Resolves <paramref name="id"/> to its PT-BR string.
        /// - id present in the table  => the stored value.
        /// - id absent (non-empty)    => the id itself (deterministic, visible fallback).
        /// - id null or empty         => the argument is returned unchanged (null -> null, "" -> "").
        /// Never throws; never returns a surprise empty string for a present key.
        /// </summary>
        public static string Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Edge: preserve the argument exactly (deterministic, documented — CA-3).
                return id;
            }

            if (LocalizationStringTable.TryGetValue(id, out var value) && value != null)
            {
                return value;
            }

            // Missing key => fall back to the id (parity with the legacy "?? QuestId" — CA-2).
            return id;
        }

        /// <summary>
        /// Tries to resolve <paramref name="id"/>.
        /// Returns true and the stored value when the key exists; otherwise returns false and sets
        /// <paramref name="value"/> to the resolved fallback from <see cref="Get"/> (the id, or the
        /// null/empty argument) so callers can use <paramref name="value"/> directly either way.
        /// </summary>
        public static bool TryGet(string id, out string value)
        {
            if (!string.IsNullOrEmpty(id)
                && LocalizationStringTable.TryGetValue(id, out var stored)
                && stored != null)
            {
                value = stored;
                return true;
            }

            value = Get(id);
            return false;
        }

        /// <summary>True when the table has a resolvable entry for <paramref name="id"/>.</summary>
        public static bool Has(string id)
        {
            return LocalizationStringTable.Contains(id);
        }
    }
}
