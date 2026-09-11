using CindarsHope.Farm.Scene;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Materializes solid fence runs while preserving the authored orchard and pasture gates.</summary>
    public static class FarmSettlementPhysicsComposer
    {
        public static void Create()
        {
            var root = new GameObject("FarmSettlementPhysics");
            root.transform.position = Vector3.zero;
            foreach (var segment in FarmSettlementPhysicsContract.AllFenceSegments)
            {
                var item = new GameObject("SolidFence_" + segment.Id);
                item.transform.SetParent(root.transform, false);
                item.transform.position = segment.Center;
                CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                    item, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.WorldSolid);
                var collider = item.AddComponent<BoxCollider2D>();
                collider.isTrigger = false;
                collider.size = segment.Size;
            }
        }
    }
}
