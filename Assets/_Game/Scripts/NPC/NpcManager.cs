using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.NPC.Schedule;
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
                    data.Npcs.Add(CaptureNpc(npc.NpcData, npc.transform, npc.HasMet));
                }
            }

            foreach (var npc in _shopNpcs)
            {
                if (npc != null && npc.NpcData != null)
                {
                    data.Npcs.Add(CaptureNpc(npc.NpcData, npc.transform, npc.HasMet));
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
                        RestoreLoadedPosition(npc.transform, savedNpc.Position);
                        RestoreInterior(npc.transform, savedNpc);
                        npc.RestoreState(savedNpc.HasMet);
                        break;
                    }
                }

                foreach (var npc in _shopNpcs)
                {
                    if (npc != null && npc.NpcData != null && npc.NpcData.NpcId == savedNpc.NpcId)
                    {
                        RestoreLoadedPosition(npc.transform, savedNpc.Position);
                        RestoreInterior(npc.transform, savedNpc);
                        npc.RestoreState(savedNpc.HasMet);
                        break;
                    }
                }
            }
        }

        private static void RestoreLoadedPosition(Transform npc, Vector2 savedPosition)
        {
            var wanderer = npc != null ? npc.GetComponent<NpcWanderer>() : null;
            var schedule = Schedule.NpcScheduleService.Instance;
            if (wanderer != null && schedule != null && schedule.TryRestoreLoadedPosition(wanderer, savedPosition))
                return;
            if (npc != null) npc.position = savedPosition;
        }

        private static void RestoreInterior(Transform npc, NpcSaveData saved)
        {
            var adapter = npc.GetComponent<NpcInteriorPresentationAdapter>();
            if (adapter == null) return;
            if (string.IsNullOrEmpty(saved.InteriorHouseId))
                adapter.RestoreExterior(npc.position);
            else
                adapter.RestoreInterior(saved.InteriorHouseId, saved.InteriorAnchorId,
                    new Vector2(saved.InteriorLocalX, saved.InteriorLocalY), npc.position);
        }

        private static NpcSaveData CaptureNpc(NpcDataSO data, Transform npc, bool hasMet)
        {
            var adapter = npc.GetComponent<NpcInteriorPresentationAdapter>();
            var save = new NpcSaveData
            {
                NpcId = data.NpcId,
                SceneId = data.DefaultSceneId,
                Position = adapter != null && adapter.IsInterior ? adapter.ExteriorSavePosition : npc.position,
                HasMet = hasMet
            };
            if (adapter != null && adapter.IsInterior)
            {
                save.InteriorHouseId = adapter.InteriorHouseId;
                save.InteriorAnchorId = adapter.AnchorId;
                save.InteriorLocalX = adapter.InteriorLocalPosition.x;
                save.InteriorLocalY = adapter.InteriorLocalPosition.y;
            }
            return save;
        }
    }
}
