using System;
using System.Collections.Generic;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    /// <summary>Chooses an explicitly authored, currently clear exterior recovery point.</summary>
    internal static class TownInteriorRestoreCoordinator
    {
        internal static bool TryFallback(TownInteriorLocationSnapshot snapshot,
            IReadOnlyList<TownInteriorVisitController.HouseBinding> bindings,
            Transform defaultTownSpawn, Func<Vector3, bool> clear, out Vector3 position,
            out string reason)
        {
            if (bindings != null)
            {
                foreach (var binding in bindings)
                {
                    if (binding == null || binding.ReturnAnchorId != snapshot.ReturnAnchorId ||
                        binding.ReturnPockets == null) continue;
                    foreach (var pocket in binding.ReturnPockets)
                        if (pocket != null && clear(pocket.position))
                        {
                            position = pocket.position;
                            reason = "Restored at the approved return pocket.";
                            return true;
                        }
                }
            }
            if (defaultTownSpawn != null && clear(defaultTownSpawn.position))
            {
                position = defaultTownSpawn.position;
                reason = "Restored at the registered Town default spawn.";
                return true;
            }
            position = default;
            reason = "No approved free Town fallback exists.";
            return false;
        }
    }
}
