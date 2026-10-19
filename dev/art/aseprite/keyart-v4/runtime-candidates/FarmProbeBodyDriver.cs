using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Temporary Editor probe: controlled Rigidbody motion, never an input-system test.</summary>
    [AddComponentMenu("")]
    public sealed class FarmProbeBodyDriver : MonoBehaviour
    {
        internal Rigidbody2D Body;
        internal Vector2 Target;
        internal bool Driving;
        internal int FixedSteps;
        private void FixedUpdate()
        {
            if (Body == null) return;
            FixedSteps++;
            Body.linearVelocity = Vector2.zero;
            if (Driving) Body.MovePosition(Vector2.MoveTowards(Body.position, Target, 3f * Time.fixedDeltaTime));
        }
    }
}
