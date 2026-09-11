using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    public readonly struct MagicConfluenceArmedEvent
    {
        public string Variant { get; }
        public int Rank { get; }
        public float DurationSeconds { get; }
        public MagicConfluenceArmedEvent(string variant, int rank, float durationSeconds)
        {
            Variant = variant ?? string.Empty;
            Rank = rank;
            DurationSeconds = durationSeconds;
        }
    }

    public readonly struct MagicConfluenceConsumedEvent
    {
        public string Variant { get; }
        public string ActionId { get; }
        public MagicConfluenceConsumedEvent(string variant, string actionId)
        {
            Variant = variant ?? string.Empty;
            ActionId = actionId ?? string.Empty;
        }
    }

    /// <summary>Runtime bridge for deterministic Confluence state and gradual stamina echo.</summary>
    public sealed class MagicConfluenceRuntime : ISpellCastLifecyclePolicy
    {
        public const string NodeId = "magic_capstone_elemental_confluence";

        private readonly MagicConfluenceState _state;
        private readonly Func<int> _rankSource;
        private readonly Func<string> _variantSource;
        private readonly Action<int> _restoreStamina;

        public MagicConfluenceRuntime(MagicConfluenceState state, Func<int> rankSource,
            Func<string> variantSource, Action<int> restoreStamina)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _rankSource = rankSource ?? (() => 0);
            _variantSource = variantSource ?? (() => string.Empty);
            _restoreStamina = restoreStamina;
        }

        public MagicConfluenceState State => _state;

        public SpellCastPreparation Prepare(SpellCastPreparationRequest request)
            => _state.Prepare(request, _variantSource(), _rankSource());

        public void Commit(SpellCastPreparation preparation, int maxManaAtCommit)
        {
            string consumedVariant = _state.ArmedVariant;
            bool armed = _state.Commit(preparation, maxManaAtCommit, _variantSource(),
                _rankSource(), out bool consumed);
            if (armed)
                GameEventBus.Publish(new MagicConfluenceArmedEvent(_state.ArmedVariant,
                    _state.ArmedRank, _state.ArmedRemainingSeconds));
            if (!consumed) return;

            if (preparation.StaminaEchoAmount > 0 &&
                preparation.StaminaEchoDurationSeconds > 0f)
            {
                _state.StartStaminaEcho(preparation.StaminaEchoAmount,
                    preparation.StaminaEchoDurationSeconds);
            }
            GameEventBus.Publish(new MagicConfluenceConsumedEvent(consumedVariant,
                preparation.Request.ActionId));
        }

        public void Cancel(SpellCastPreparation preparation) => _state.Cancel(preparation);

        public void Tick(float deltaSeconds)
        {
            _state.Advance(deltaSeconds);
            int amount = _state.AdvanceStaminaEcho(deltaSeconds);
            if (amount > 0) _restoreStamina?.Invoke(amount);
        }

        public void OnRespec()
        {
            _state.ClearForRespec();
        }
    }
}
