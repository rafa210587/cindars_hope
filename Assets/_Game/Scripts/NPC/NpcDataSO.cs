using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.NPC
{
    public enum NpcMovementMode
    {
        Static,
        RandomWander
    }

    [CreateAssetMenu(fileName = "NpcData", menuName = "CindarsHope/NPC/NPC Data")]
    public class NpcDataSO : ScriptableObject, IIdentifiedData
    {
        public string NpcId;
        public string DisplayName;
        public string OpeningLine;
        public string ClosingLine;
        public DialogueTreeSO DialogueTree;
        public string ShopId;
        public string DefaultSceneId = "TownScene";
        public string DefaultPositionId;
        public Vector2 DefaultPosition;
        public NpcMovementMode MovementMode = NpcMovementMode.Static;
        public NpcWanderData WanderData;
        public Sprite BodySprite;

        [Header("Retratos (busto) para UI de conversa / status de companion")]
        public Sprite PortraitNeutral;
        public Sprite PortraitHappiness;
        public Sprite PortraitLove;
        public Sprite PortraitDisdain;
        public Sprite PortraitHatred;

        string IIdentifiedData.Id => NpcId;

        /// <summary>
        /// Retrato (busto) para a feicao dada. Cai no retrato neutro se a expressao especifica
        /// nao tiver sido atribuida; pode retornar null se nem o neutro existir (HUD trata como
        /// fallback para o placeholder tintado).
        /// </summary>
        public Sprite GetPortrait(NpcExpression expression)
        {
            Sprite s;
            switch (expression)
            {
                case NpcExpression.Hatred: s = PortraitHatred; break;
                case NpcExpression.Disdain: s = PortraitDisdain; break;
                case NpcExpression.Happiness: s = PortraitHappiness; break;
                case NpcExpression.Love: s = PortraitLove; break;
                default: s = PortraitNeutral; break;
            }
            return s != null ? s : PortraitNeutral;
        }
    }

    [System.Serializable]
    public class NpcWanderData
    {
        public float WanderSpeed = 1.5f;
        public float WanderRadius = 10f;
        public float PauseMinDuration = 2f;
        public float PauseMaxDuration = 5f;
    }
}
