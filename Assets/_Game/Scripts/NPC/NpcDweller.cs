using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Marcador leve: identifica um GameObject como um NPC morador da cidade. Usado por gatilhos de
    /// cena (ex.: <c>HouseDoorInteractable</c>) para distinguir um NPC do jogador sem busca global —
    /// a porta abre sozinha quando um morador se aproxima, mas o jogador continua abrindo com E.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcDweller : MonoBehaviour, INpcDoorTraveler
    {
        [SerializeField] private string _npcId;

        public string NpcId => _npcId;

        public void Configure(string npcId) => _npcId = npcId;
    }
}
