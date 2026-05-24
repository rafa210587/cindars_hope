using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Inventory;
using CindarsHope.Player;
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
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private List<NpcController> _npcs = new();

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
    }
}
