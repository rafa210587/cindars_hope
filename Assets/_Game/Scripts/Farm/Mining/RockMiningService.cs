using System.Collections.Generic;

namespace CindarsHope.Farm.Mining
{
    public class RockMineResult
    {
        public bool Success { get; set; }
        public bool RockDepleted { get; set; }
        public string[] DroppedItemIds { get; set; } = System.Array.Empty<string>();
        public int[] DroppedQuantities { get; set; } = System.Array.Empty<int>();
        public string FailureReason { get; set; }

        public static RockMineResult Fail(string reason) => new RockMineResult { Success = false, FailureReason = reason };
        public static RockMineResult Hit() => new RockMineResult { Success = true };
        public static RockMineResult Depleted(string[] items, int[] qtys) =>
            new RockMineResult { Success = true, RockDepleted = true, DroppedItemIds = items, DroppedQuantities = qtys };
    }

    public class RockMiningService
    {
        private readonly Dictionary<string, RockDefinition> _definitions;

        public RockMiningService(Dictionary<string, RockDefinition> definitions)
        {
            _definitions = definitions ?? new Dictionary<string, RockDefinition>();
        }

        public RockMineResult Mine(RockInstanceState rock, string pickaxeTier, int currentDay)
        {
            if (rock == null)
                return RockMineResult.Fail("RockNotFound");

            if (!_definitions.TryGetValue(rock.RockId, out var def))
                return RockMineResult.Fail("DefinitionNotFound");

            if (def.IsQuarryReserved)
                return RockMineResult.Fail("QuarryReservedFuture");

            if (rock.IsDepleted)
                return RockMineResult.Fail("AlreadyDepleted");

            rock.RemainingHits--;

            if (rock.RemainingHits <= 0)
            {
                rock.IsDepleted = true;
                rock.DepletedOnDay = currentDay;
                if (def.CanRefresh)
                    rock.NextRefreshEligibleDay = currentDay + def.RefreshAfterDays;

                var drops = ResolveDrops(def.DropTableId);
                return RockMineResult.Depleted(drops.Item1, drops.Item2);
            }

            return RockMineResult.Hit();
        }

        public void RefreshIfEligible(RockInstanceState rock, int currentDay)
        {
            if (rock == null || !rock.IsDepleted) return;
            if (!_definitions.TryGetValue(rock.RockId, out var def) || !def.CanRefresh) return;
            if (currentDay < rock.NextRefreshEligibleDay) return;

            rock.IsDepleted = false;
            rock.RemainingHits = def.MaxHits;
        }

        private (string[], int[]) ResolveDrops(string dropTableId)
        {
            return (new[] { "item_stone" }, new[] { 2 });
        }
    }
}
