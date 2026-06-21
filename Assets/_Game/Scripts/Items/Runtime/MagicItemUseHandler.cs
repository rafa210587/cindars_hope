using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// fable_31 — F08 use-pipeline handler for the four magic "use" items (bell / mirror / hourglass /
    /// whetstone). Registered per-item on <see cref="ItemUseManager"/> so it flows through the EXISTING
    /// consumption path (which removes the item and publishes ItemUsedEvent when this returns true). The
    /// effect resolution + daily gating live in the pure <see cref="MagicItemUseService"/>; this handler only
    /// translates the resolved effect into the named runtime hooks (events) and decides consume vs. refuse.
    ///
    /// Returning false from <see cref="TryUseItem"/> means "do not consume" (e.g. whetstone already used today,
    /// or no active cave run for the mirror) — the item stays in the inventory.
    /// </summary>
    public sealed class MagicItemUseHandler : ItemUseHandler
    {
        private MagicItemUseService _service;
        private IMagicTimeGateway _timeGateway;

        public void Configure(MagicItemUseService service, IMagicTimeGateway timeGateway)
        {
            _service = service;
            _timeGateway = timeGateway;
        }

        public override bool CanUseItem(string itemId, int amount)
        {
            return MagicItemCatalog.IsUseItem(itemId) && amount > 0;
        }

        public override bool TryUseItem(string itemId, int amount, GameObject user)
        {
            if (!MagicItemCatalog.IsUseItem(itemId))
            {
                return false;
            }

            var service = _service ?? (_service = new MagicItemUseService());
            var day = _timeGateway != null ? _timeGateway.CurrentDay : 1;
            var result = service.Resolve(itemId, day);
            if (!result.Consumed)
            {
                CindarsHope.Combat.CombatLog.Log($"CombatLog: MagicUseRefused. Item={itemId}, Reason={result.Reason}.", this);
                return false;
            }

            return ApplyEffect(result.Effect, day);
        }

        private bool ApplyEffect(MagicUseEffect effect, int day)
        {
            switch (effect)
            {
                case MagicUseEffect.WardEnemies:
                    GameEventBus.Publish(new EnemyWardRequestedEvent(8f, 5f));
                    return true;

                case MagicUseEffect.ReturnToEntrance:
                    return TryRequestMirrorReturn();

                case MagicUseEffect.AdvanceToDawn:
                    if (_timeGateway != null)
                    {
                        _timeGateway.AdvanceToDawn();
                    }
                    else
                    {
                        GameEventBus.Publish(new AdvanceToDawnRequestedEvent(day));
                    }
                    return true;

                case MagicUseEffect.FreeRepair:
                    GameEventBus.Publish(new FreeRepairRequestedEvent(day));
                    return true;

                default:
                    return false;
            }
        }

        // Mirror of Return: assert the stable-run invariant (same run seed + level) and raise the named hook.
        // Reads the active cave run via the CaveRunManager singleton (no global scene search). Refuses (returns
        // false → item not consumed) when there is no active cave run.
        private bool TryRequestMirrorReturn()
        {
            var runManager = CaveRunManager.Instance;
            if (runManager == null)
            {
                CindarsHope.Combat.CombatLog.Log("CombatLog: MagicUseRefused. Item=mirror_of_return, Reason=no_active_cave_run.", this);
                return false;
            }

            var runSeed = runManager.CaveRunSeed;
            var level = runManager.CurrentCaveLevel;

            // Stable-run guard: reading again must yield the same seed/level — we never trigger regeneration.
            var decision = MirrorOfReturnDecision.Evaluate(
                runSeed, level, runManager.CaveRunSeed, runManager.CurrentCaveLevel);
            if (!decision.CanTeleport)
            {
                Debug.LogWarning($"CombatLog: MagicUseRefused. Item=mirror_of_return, Reason={decision.Reason}.", this);
                return false;
            }

            GameEventBus.Publish(new MirrorReturnRequestedEvent(runSeed, level));
            CindarsHope.Combat.CombatLog.Log($"CombatLog: MirrorReturnRequested. RunSeed={runSeed}, Level={level}.", this);
            return true;
        }
    }
}
