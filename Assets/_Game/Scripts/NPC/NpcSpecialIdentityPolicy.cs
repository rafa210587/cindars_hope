using System;

namespace CindarsHope.NPC
{
    public static class NpcSpecialIdentityPolicy
    {
        public static bool IsThalindra(NpcDataSO npcData)
        {
            return Matches(npcData, "npc_thalindra", "Thalindra");
        }

        public static bool IsBrumdar(NpcDataSO npcData)
        {
            return Matches(npcData, "npc_brumdar", "Brumdar");
        }

        private static bool Matches(NpcDataSO npcData, string expectedNpcId, string displayNameToken)
        {
            if (npcData == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(npcData.NpcId)
                && npcData.NpcId.Equals(expectedNpcId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrEmpty(npcData.DisplayName)
                && npcData.DisplayName.IndexOf(displayNameToken, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
