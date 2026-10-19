using UnityEngine;

namespace CindarsHope.City.Interiors
{
    [CreateAssetMenu(fileName = "TownInteriorTransitionConfig", menuName = "Cindar's Hope/Town/Interior Transition Config")]
    public sealed class TownInteriorTransitionConfigSO : ScriptableObject
    {
        [Min(0.01f)] public float FadeOutSeconds = 0.20f;
        [Min(0.01f)] public float FadeInSeconds = 0.25f;
        [Min(0f)] public float LoadingHintAfterSeconds = 0.75f;
        [Min(0.01f)] public float TimeoutSeconds = 10f;
        public Vector2 PresentationOrigin = new Vector2(4096f, 4096f);
    }
}
