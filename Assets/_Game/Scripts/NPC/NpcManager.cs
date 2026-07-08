using System.Collections.Generic;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public class NpcManager : MonoBehaviour
    {
        [SerializeField] private DialogueModal _dialogueModal;
        [SerializeField] private ModalManager _modalManager;
        [SerializeField] private List<NpcController> _npcs = new();
        [SerializeField] private List<NpcShopController> _shopNpcs = new();

        private void Start()
        {
            if (_dialogueModal != null && _modalManager != null)
            {
                _dialogueModal.Initialize(_modalManager);
            }
        }

        public void RegisterNpc(NpcController npc)
        {
            if (npc != null && !_npcs.Contains(npc))
            {
                _npcs.Add(npc);
            }
        }

        public void UnregisterNpc(NpcController npc)
        {
            if (npc != null)
            {
                _npcs.Remove(npc);
            }
        }

        public NpcManagerSaveData CaptureSaveData()
        {
            var data = new NpcManagerSaveData();
            foreach (var npc in _npcs)
            {
                if (npc != null && npc.NpcData != null)
                {
                    data.Npcs.Add(CaptureNpc(npc.NpcData, npc.transform.position, npc.HasMet));
                }
            }

            foreach (var npc in _shopNpcs)
            {
                if (npc != null && npc.NpcData != null)
                {
                    data.Npcs.Add(CaptureNpc(npc.NpcData, npc.transform.position, npc.HasMet));
                }
            }

            return data;
        }

        public void RestoreFromSaveData(NpcManagerSaveData data)
        {
            if (data?.Npcs == null)
            {
                return;
            }

            foreach (var savedNpc in data.Npcs)
            {
                foreach (var npc in _npcs)
                {
                    if (npc != null && npc.NpcData != null && npc.NpcData.NpcId == savedNpc.NpcId)
                    {
                        npc.transform.position = savedNpc.Position;
                        npc.RestoreState(savedNpc.HasMet);
                        break;
                    }
                }

                foreach (var npc in _shopNpcs)
                {
                    if (npc != null && npc.NpcData != null && npc.NpcData.NpcId == savedNpc.NpcId)
                    {
                        npc.transform.position = savedNpc.Position;
                        npc.RestoreState(savedNpc.HasMet);
                        break;
                    }
                }
            }
        }

        private static NpcSaveData CaptureNpc(NpcDataSO data, Vector3 position, bool hasMet)
        {
            return new NpcSaveData
            {
                NpcId = data.NpcId,
                SceneId = data.DefaultSceneId,
                Position = position,
                HasMet = hasMet
            };
        }
    }
}
