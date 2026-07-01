using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Registry estatico runtime de NpcDataSO por NpcId, alimentado pelo NpcController
    /// (Register no OnEnable, Unregister no OnDisable). Permite a UI (ex.: NpcInteractionPortraitHud)
    /// resolver o NpcDataSO de um NPC ativo SEM GameObject.Find / FindObjectOfType. Mesmo padrao
    /// do NpcTownRosterRegistry, porem mapeando para o ScriptableObject (retratos, sprites).
    /// </summary>
    public static class NpcVisualRegistry
    {
        private static readonly Dictionary<string, NpcDataSO> ById = new Dictionary<string, NpcDataSO>();

        public static void Register(NpcDataSO data)
        {
            if (data != null && !string.IsNullOrEmpty(data.NpcId))
            {
                ById[data.NpcId] = data;
            }
        }

        public static void Unregister(string npcId)
        {
            if (!string.IsNullOrEmpty(npcId))
            {
                ById.Remove(npcId);
            }
        }

        public static bool TryGet(string npcId, out NpcDataSO data)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                data = null;
                return false;
            }
            return ById.TryGetValue(npcId, out data);
        }
    }
}
