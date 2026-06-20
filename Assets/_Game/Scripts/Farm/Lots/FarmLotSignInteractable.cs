using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — placa informativa de um lote Locked ("Lote à venda — escritura na prefeitura").
    /// Interactable APENAS informativo: ao interagir, publica um toast com o destino de compra e o
    /// preço da escritura. Não compra nem destrava (a compra é no shop; o destravamento é via uso
    /// da escritura no ItemUseManager). Fica desativada junto com a cerca quando o lote vira Owned.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FarmLotSignInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _lotId = FarmLotId.North;

        public string InteractionPrompt => "Ler placa do lote";

        public bool CanInteract(GameObject interactor) => true;

        public void Interact(GameObject interactor)
        {
            var message = BuildMessage();
            GameEventBus.Publish(new PlayerActionFeedbackEvent(message, 3f));
            Debug.Log($"[FarmLotSign] {message}", this);
        }

        private string BuildMessage()
        {
            if (FarmLotCatalog.TryGetByLotId(_lotId, out var def))
            {
                return $"{def.DisplayName}. Escritura: {def.DeedPrice}g (prefeitura).";
            }

            return "Lote a venda - escritura na prefeitura.";
        }

#if UNITY_EDITOR
        public void EditorSetLotId(string lotId)
        {
            _lotId = lotId;
        }
#endif
    }
}
