using System.Collections.Generic;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.NPC
{
    // arch: quebra do par mutuo NPC|UI (2026-07-15) — campos viram MonoBehaviour + cast para as
    // portas INpcDialoguePresenter/IModalRuntime (precedente Craft/ICraftingStationModal),
    // preservando a ref de cena sem regen.
    [DisallowMultipleComponent]
    public class NpcManager : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _dialogueModal;
        [SerializeField] private MonoBehaviour _modalManager;
        [SerializeField] private List<NpcController> _npcs = new();
        [SerializeField] private List<NpcShopController> _shopNpcs = new();

        private void Start()
        {
            var presenter = _dialogueModal as INpcDialoguePresenter;
            var modalRuntime = _modalManager as IModalRuntime;
            if (presenter != null && modalRuntime != null)
            {
                presenter.Initialize(modalRuntime);
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
