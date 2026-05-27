using UnityEngine;

namespace CindarsHope.Core.Data
{
    [CreateAssetMenu(fileName = "GameScaleConfig", menuName = "CindarsHope/Config/Game Scale Config")]
    public sealed class GameScaleConfigSO : ScriptableObject
    {
        [Header("Reference")]
        public float PlayerReferenceScale = 1f;

        [Header("World Objects")]
        public float TreeScale = 3f;
        public float LakeScale = 6f;

        [Header("Boss")]
        public float BossScale = 2.5f;
        public float BossMinScale = 2f;
        public float BossMaxScale = 3f;

        [Header("Enemies")]
        public float NormalEnemySmallScale = 1.15f;
        public float NormalEnemyMediumScale = 1.35f;
        public float NormalEnemyLargeScale = 1.65f;

        [Header("Cave Multipliers")]
        public int CaveWidthMultiplier = 2;
        public int CaveHeightMultiplier = 2;
        public int CaveRoomSizeMultiplier = 2;
        public int CaveCorridorWidthMultiplier = 2;

        private void OnValidate()
        {
            PlayerReferenceScale = Mathf.Max(0.1f, PlayerReferenceScale);
            TreeScale = Mathf.Max(0.1f, TreeScale);
            LakeScale = Mathf.Max(0.1f, LakeScale);
            BossMinScale = Mathf.Max(0.1f, BossMinScale);
            BossMaxScale = Mathf.Max(BossMinScale, BossMaxScale);
            BossScale = Mathf.Clamp(BossScale, BossMinScale, BossMaxScale);
            NormalEnemySmallScale = Mathf.Max(0.1f, NormalEnemySmallScale);
            NormalEnemyMediumScale = Mathf.Max(0.1f, NormalEnemyMediumScale);
            NormalEnemyLargeScale = Mathf.Max(0.1f, NormalEnemyLargeScale);
            CaveWidthMultiplier = Mathf.Max(1, CaveWidthMultiplier);
            CaveHeightMultiplier = Mathf.Max(1, CaveHeightMultiplier);
            CaveRoomSizeMultiplier = Mathf.Max(1, CaveRoomSizeMultiplier);
            CaveCorridorWidthMultiplier = Mathf.Max(1, CaveCorridorWidthMultiplier);
        }
    }
}
