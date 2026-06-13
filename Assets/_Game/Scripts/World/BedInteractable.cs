using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Player.Conditions;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Cama da fazenda (F16). Primeira interação pede confirmação; segunda (em até 4s)
    /// dorme: recuperação via SleepRecoveryCalculator + day transition (mesmo caminho do debug).
    /// </summary>
    [DisallowMultipleComponent]
    public class BedInteractable : MonoBehaviour, IInteractable
    {
        private const float ConfirmWindowSeconds = 4f;

        private float _confirmUntil;

        public string InteractionPrompt => "Dormir";

        public bool CanInteract(GameObject interactor)
        {
            return PlayerConditionService.Instance != null;
        }

        public void Interact(GameObject interactor)
        {
            var service = PlayerConditionService.Instance;
            if (service == null)
            {
                Debug.LogWarning("BedInteractable: PlayerConditionService ausente. " +
                    "Cena: FarmScene | GameObject: Bed | componente: BedInteractable | campo: service runtime.", this);
                return;
            }

            if (Time.unscaledTime > _confirmUntil)
            {
                _confirmUntil = Time.unscaledTime + ConfirmWindowSeconds;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dormir ate amanha? Interaja novamente para confirmar."));
                return;
            }

            _confirmUntil = 0f;
            service.SleepInBed();
        }
    }
}
