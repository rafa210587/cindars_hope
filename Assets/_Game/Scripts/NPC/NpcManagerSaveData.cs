using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.NPC
{
    // arch: movido de CindarsHope.Save (SaveData.cs) para co-localizar com o owner NpcManager,
    // quebrando o ciclo mutuo NPC|Save (precedente: FriendshipSaveData/NpcServicesSaveData).
    // JsonUtility serializa por nome de campo, nao por namespace/type-name — sem migration.
    [Serializable]
    public class NpcManagerSaveData
    {
        public List<NpcSaveData> Npcs = new List<NpcSaveData>();
    }

    [Serializable]
    public class NpcSaveData
    {
        public string NpcId;
        public string SceneId;
        public Vector2 Position;
        public bool HasMet;
    }
}
