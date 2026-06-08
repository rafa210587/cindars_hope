using System.Collections.Generic;
using CindarsHope.Loot;

namespace CindarsHope.Enemy.Drops
{
    public class DropResolveResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public List<string> GrantedLootTableIds { get; set; } = new List<string>();
        public int GoldGranted { get; set; }
        public bool FirstTimeRewardApplied { get; set; }
        public bool RepeatRewardUsed { get; set; }
        public List<string> GrantedStoryFlags { get; set; } = new List<string>();
        public List<string> DebugNotes { get; set; } = new List<string>();

        public static DropResolveResult Fail(string reason) =>
            new DropResolveResult { Success = false, FailureReason = reason };
    }

    public class EnemyDropResolver
    {
        // Resolves which loot tables apply for a common enemy
        public DropResolveResult ResolveCommon(EnemyDropProfile profile, DefeatRewardContext ctx)
        {
            if (profile == null) return DropResolveResult.Fail("profile is null");
            if (profile.NoNormalLoot) return DropResolveResult.Fail("Enemy has NoNormalLoot flag");

            var result = new DropResolveResult { Success = true };

            if (!string.IsNullOrEmpty(profile.CommonMaterialTableId))
                result.GrantedLootTableIds.Add(profile.CommonMaterialTableId);
            if (!string.IsNullOrEmpty(profile.ThematicComponentTableId))
                result.GrantedLootTableIds.Add(profile.ThematicComponentTableId);
            // Rare only at appropriate depth
            if (!string.IsNullOrEmpty(profile.RareComponentTableId) && ctx.CaveLevel >= profile.NativeFloorMin)
                result.GrantedLootTableIds.Add(profile.RareComponentTableId);

            return result;
        }

        // Resolves elite drops — guaranteed thematic component + bonus
        public DropResolveResult ResolveElite(EliteDropProfile profile, DefeatRewardContext ctx)
        {
            if (profile == null) return DropResolveResult.Fail("elite profile is null");

            var result = new DropResolveResult { Success = true };
            if (!string.IsNullOrEmpty(profile.GuaranteedOrNearGuaranteedComponentItemId))
                result.GrantedLootTableIds.Add("elite_thematic_" + profile.BaseEnemyFamilyId);
            if (!string.IsNullOrEmpty(profile.RareDropTableId))
                result.GrantedLootTableIds.Add(profile.RareDropTableId);
            result.DebugNotes.Add($"Elite bonus: gold x{profile.GoldBonusMultiplier}");
            return result;
        }

        // Resolves boss drops — first-time vs repeat, with BossDefeatState idempotency
        public DropResolveResult ResolveBoss(BossRewardProfile profile, BossDefeatState defeatState)
        {
            if (profile == null) return DropResolveResult.Fail("boss profile is null");
            if (defeatState == null) return DropResolveResult.Fail("defeatState is null");

            var result = new DropResolveResult { Success = true };

            if (!defeatState.FirstTimeRewardConsumed)
            {
                // First-time reward
                result.GrantedLootTableIds.Add(profile.FirstTimeRewardTableId);
                result.GrantedStoryFlags.AddRange(profile.GrantedStoryFlags);
                defeatState.FirstTimeRewardConsumed = true;
                defeatState.IsDefeated = true;
                defeatState.DefeatCount++;
                result.FirstTimeRewardApplied = true;
                result.DebugNotes.Add("FirstTimeReward applied and consumed");
            }
            else
            {
                // Repeat reward — smaller, controlled
                if (!string.IsNullOrEmpty(profile.RepeatRewardTableId))
                {
                    result.GrantedLootTableIds.Add(profile.RepeatRewardTableId);
                    result.RepeatRewardUsed = true;
                }
                defeatState.DefeatCount++;
                result.DebugNotes.Add($"RepeatReward used (defeat #{defeatState.DefeatCount})");
            }

            // Lore rewards only on first time
            if (result.FirstTimeRewardApplied)
                result.GrantedLootTableIds.AddRange(profile.LoreRewardIds);

            return result;
        }
    }
}
