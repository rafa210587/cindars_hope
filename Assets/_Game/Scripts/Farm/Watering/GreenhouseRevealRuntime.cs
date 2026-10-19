using CindarsHope.Farm.Scene;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Farm.Watering
{
    /// <summary>Connects the authored greenhouse shell to the existing roof reveal behavior.</summary>
    public static class GreenhouseRevealRuntime
    {
        public static RoofRevealController Configure(Transform greenhouse, SpriteRenderer exterior)
        {
            if (greenhouse == null || exterior == null)
                throw new System.ArgumentNullException("Greenhouse and exterior are required.");

            var revealTransform = greenhouse.Find("RoofReveal");
            if (revealTransform == null)
            {
                var created = new GameObject("RoofReveal");
                created.transform.SetParent(greenhouse, false);
                revealTransform = created.transform;
            }

            var bounds = FarmSettlementPhysicsContract.GreenhouseInteriorBounds;
            revealTransform.position = new Vector3(bounds.center.x, bounds.center.y, greenhouse.position.z);
            var trigger = revealTransform.GetComponent<BoxCollider2D>();
            if (trigger == null) trigger = revealTransform.gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.offset = Vector2.zero;
            trigger.size = new Vector2(bounds.width / Mathf.Abs(revealTransform.lossyScale.x),
                bounds.height / Mathf.Abs(revealTransform.lossyScale.y));

            var reveal = revealTransform.GetComponent<RoofRevealController>();
            if (reveal == null) reveal = revealTransform.gameObject.AddComponent<RoofRevealController>();
            reveal.Configure(new[] { exterior });
            reveal.ConfigureFeetOccupancy(trigger);
            return reveal;
        }
    }
}
