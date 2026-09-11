using System;

namespace CindarsHope.Foundation
{
    public interface ICombatCapstoneModifierSource
    {
        float DirectMeleeDamageMultiplier { get; }
        float MeleeCriticalDamageBonus { get; }
    }

    public static class CombatCapstoneModifierProvider
    {
        public static ICombatCapstoneModifierSource Source { get; set; }

        public static float ResolveDirectMeleeDamageMultiplier()
            => Math.Max(0f, Source?.DirectMeleeDamageMultiplier ?? 1f);

        public static float ResolveMeleeCriticalDamageBonus()
            => Math.Max(0f, Source?.MeleeCriticalDamageBonus ?? 0f);
    }

    public enum DamageSourceKind
    {
        None = 0,
        PlayerMelee = 1,
        PlayerRanged = 2,
        PlayerMagic = 3,
        Enemy = 4,
        Environment = 5,
        DamageOverTime = 6,
        CapstoneSecondary = 7
    }

    public enum SpellDiscipline
    {
        None = 0,
        Spiritual = 1,
        Offensive = 2
    }

    public interface IPlayerControlResistanceSource
    {
        float KnockbackReductionFraction { get; }
        float StunDurationReductionFraction { get; }
    }

    public static class PlayerControlResistanceProvider
    {
        public static IPlayerControlResistanceSource Source { get; set; }

        public static float ResolveKnockbackForce(float force)
        {
            float reduction = Source != null ? Source.KnockbackReductionFraction : 0f;
            return Math.Max(0f, force) * (1f - Clamp01(reduction));
        }

        public static float ResolveStunDuration(float seconds)
        {
            float reduction = Source != null ? Source.StunDurationReductionFraction : 0f;
            return Math.Max(0f, seconds) * (1f - Clamp01(reduction));
        }

        private static float Clamp01(float value) => Math.Max(0f, Math.Min(1f, value));
    }

    public readonly struct RangedLunarLaunchModifier
    {
        public float RangeMultiplier { get; }
        public float ProjectileSpeedMultiplier { get; }

        public RangedLunarLaunchModifier(float rangeMultiplier, float projectileSpeedMultiplier)
        {
            RangeMultiplier = Math.Max(0f, rangeMultiplier);
            ProjectileSpeedMultiplier = Math.Max(0f, projectileSpeedMultiplier);
        }

        public static RangedLunarLaunchModifier Neutral =>
            new RangedLunarLaunchModifier(1f, 1f);
    }

    public readonly struct RangedLunarImpactModifier
    {
        public float CriticalChanceBonus { get; }
        public float CriticalDamageBonus { get; }

        public RangedLunarImpactModifier(float criticalChanceBonus, float criticalDamageBonus)
        {
            CriticalChanceBonus = Math.Max(0f, criticalChanceBonus);
            CriticalDamageBonus = Math.Max(0f, criticalDamageBonus);
        }

        public static RangedLunarImpactModifier Neutral =>
            new RangedLunarImpactModifier(0f, 0f);
    }

    public interface IRangedLunarModifierSource
    {
        bool TryActivateFirstTarget(string targetInstanceId, string actionToken);
        RangedLunarLaunchModifier ResolveLaunchModifier();
        RangedLunarImpactModifier ResolveImpactModifier(
            string targetInstanceId, float targetPositionX, float targetPositionY);
    }

    public static class RangedLunarModifierProvider
    {
        public static IRangedLunarModifierSource Source { get; set; }

        public static bool TryActivateFirstTarget(string targetInstanceId, string actionToken)
            => Source?.TryActivateFirstTarget(targetInstanceId, actionToken) ?? false;

        public static RangedLunarLaunchModifier ResolveLaunchModifier()
            => Source?.ResolveLaunchModifier() ?? RangedLunarLaunchModifier.Neutral;

        public static RangedLunarImpactModifier ResolveImpactModifier(
            string targetInstanceId, float targetPositionX, float targetPositionY)
            => Source?.ResolveImpactModifier(targetInstanceId, targetPositionX, targetPositionY)
               ?? RangedLunarImpactModifier.Neutral;
    }

    public enum SpellCastTransactionState
    {
        Prepared = 0,
        Reserved = 1,
        Committed = 2,
        Cancelled = 3
    }

    public readonly struct SpellCastPreparationRequest
    {
        public string ActionId { get; }
        public string ActionToken { get; }
        public SpellDiscipline Discipline { get; }
        public int BaseManaCost { get; }
        public bool IsActiveSkill { get; }

        public SpellCastPreparationRequest(string actionId, string actionToken,
            SpellDiscipline discipline, int baseManaCost, bool isActiveSkill = false)
        {
            ActionId = actionId ?? string.Empty;
            ActionToken = actionToken ?? string.Empty;
            Discipline = discipline;
            BaseManaCost = Math.Max(0, baseManaCost);
            IsActiveSkill = isActiveSkill;
        }
    }

    public readonly struct SpellCastPreparation
    {
        public SpellCastPreparationRequest Request { get; }
        public int ManaCost { get; }
        public float DirectDamageMultiplier { get; }
        public float CriticalChanceBonus { get; }
        public float SpiritualOutputMultiplier { get; }
        public int StaminaEchoAmount { get; }
        public float StaminaEchoDurationSeconds { get; }
        public bool ReservesConfluence { get; }

        public SpellCastPreparation(SpellCastPreparationRequest request, int manaCost)
            : this(request, manaCost, 1f, 0f, 1f, 0, 0f, false)
        {
        }

        public SpellCastPreparation(SpellCastPreparationRequest request, int manaCost,
            float directDamageMultiplier, float criticalChanceBonus,
            float spiritualOutputMultiplier, int staminaEchoAmount,
            float staminaEchoDurationSeconds, bool reservesConfluence)
        {
            Request = request;
            ManaCost = Math.Max(0, manaCost);
            DirectDamageMultiplier = Math.Max(0f, directDamageMultiplier);
            CriticalChanceBonus = Math.Max(0f, criticalChanceBonus);
            SpiritualOutputMultiplier = Math.Max(0f, spiritualOutputMultiplier);
            StaminaEchoAmount = Math.Max(0, staminaEchoAmount);
            StaminaEchoDurationSeconds = Math.Max(0f, staminaEchoDurationSeconds);
            ReservesConfluence = reservesConfluence;
        }
    }

    public interface ISpellCastPreparationPolicy
    {
        SpellCastPreparation Prepare(SpellCastPreparationRequest request);
    }

    /// <summary>
    /// Optional lifecycle extension for policies whose state changes only after a reserved cast
    /// resolves. Keeps interrupted/refunded casts out of deterministic spend ledgers.
    /// </summary>
    public interface ISpellCastLifecyclePolicy : ISpellCastPreparationPolicy
    {
        void Commit(SpellCastPreparation preparation, int maxManaAtCommit);
        void Cancel(SpellCastPreparation preparation);
    }

    public static class SpellCastPreparationProvider
    {
        public static ISpellCastPreparationPolicy Source { get; set; }

        public static SpellCastPreparation Prepare(SpellCastPreparationRequest request)
            => Source != null
                ? Source.Prepare(request)
                : new SpellCastPreparation(request, request.BaseManaCost);

        public static void Commit(SpellCastPreparation preparation, int maxManaAtCommit)
        {
            if (Source is ISpellCastLifecyclePolicy lifecycle)
                lifecycle.Commit(preparation, Math.Max(0, maxManaAtCommit));
        }

        public static void Cancel(SpellCastPreparation preparation)
        {
            if (Source is ISpellCastLifecyclePolicy lifecycle)
                lifecycle.Cancel(preparation);
        }
    }

    public sealed class SpellCastTransaction
    {
        public SpellCastPreparation Preparation { get; }
        public SpellCastTransactionState State { get; private set; }

        public SpellCastTransaction(SpellCastPreparation preparation)
        {
            Preparation = preparation;
            State = SpellCastTransactionState.Prepared;
        }

        public bool TryReserve(Func<int, bool> tryDebit)
        {
            if (State != SpellCastTransactionState.Prepared || tryDebit == null ||
                !tryDebit(Preparation.ManaCost))
                return false;

            State = SpellCastTransactionState.Reserved;
            return true;
        }

        public bool Commit()
        {
            if (State != SpellCastTransactionState.Reserved)
                return false;

            State = SpellCastTransactionState.Committed;
            return true;
        }

        public bool Cancel(Action<int> refundReservedMana = null)
        {
            if (State == SpellCastTransactionState.Committed ||
                State == SpellCastTransactionState.Cancelled)
                return false;

            if (State == SpellCastTransactionState.Reserved)
                refundReservedMana?.Invoke(Preparation.ManaCost);

            State = SpellCastTransactionState.Cancelled;
            return true;
        }
    }
}
