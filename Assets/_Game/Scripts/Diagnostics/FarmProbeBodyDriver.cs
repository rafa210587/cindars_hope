#if UNITY_EDITOR
using UnityEngine;

namespace CindarsHope.Diagnostics
{
    /// <summary>Temporary Editor probe: controlled physical motion; the runtime animator observes actual velocity.</summary>
    [AddComponentMenu("")]
    public sealed class FarmProbeBodyDriver : MonoBehaviour
    {
        public Rigidbody2D Body;
        public Vector2 Target;
        public bool Driving;
        public int FixedSteps;
        private void FixedUpdate()
        {
            if (Body == null) return;
            FixedSteps++;
            Body.linearVelocity = Vector2.zero;
            if (Driving) Body.MovePosition(Vector2.MoveTowards(Body.position, Target, 3f * Time.fixedDeltaTime));
        }
    }
}
#endif
