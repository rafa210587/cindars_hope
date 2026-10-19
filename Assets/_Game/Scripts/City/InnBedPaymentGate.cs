using CindarsHope.City.Services;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Player.Conditions;
using UnityEngine;

namespace CindarsHope.City
{
    /// <summary>
    /// fable_57 — cama de HÓSPEDE da estalagem (Taverna Panela-Funda). É um gate de pagamento na
    /// frente do MESMO fluxo de dormir da fazenda (F16 — PlayerConditionService.SleepInBed): primeira
    /// interação pede confirmação ("Dormir — 50g"); a segunda (em até 4s) cobra a diária e, SÓ no
    /// débito bem-sucedido, executa o day transition + recuperação. Sem ouro ⇒ recusa com feedback e
    /// sem débito (CA-3, risco "cobrança sem dormir").
    ///
    /// NÃO cria segunda cama/fluxo de dormir: reusa PlayerConditionService.SleepInBed (F16). NÃO usa
    /// busca global de cena: o ouro vem dos acessores estáticos JÁ ligados ao PlayerManager por
    /// CityServiceRuntimeBootstrap (CityServiceAccess.CurrentGoldFunc/SpendGoldFunc), no mesmo idioma
    /// de fachada estática do projeto. Acessores são injetáveis para EditMode (TryPayNightlyRate puro).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InnBedPaymentGate : MonoBehaviour, IInteractable
    {
        private const float ConfirmWindowSeconds = 4f;

        [SerializeField] private int _nightlyRate = InnLodgingPaymentResolver.DefaultNightlyRate;

        private float _confirmUntil;

        // Acessores de ouro. Default: fachada de cidade (ligada ao PlayerManager pelo runtime bridge).
        // Injetáveis (setter) para teste sem cena.
        private System.Func<int> _currentGold;
        private System.Func<int, bool> _trySpend;

        public string InteractionPrompt => $"Dormir — {_nightlyRate}g";

        private System.Func<int> CurrentGold => _currentGold ?? CityServiceAccess.CurrentGoldFunc;
        private System.Func<int, bool> TrySpend => _trySpend ?? CityServiceAccess.SpendGoldFunc;

        /// <summary>Configuração do gerador da cena (diária). Mantém refs serializadas, sem YAML manual.</summary>
        public void Configure(int nightlyRate)
        {
            _nightlyRate = nightlyRate > 0 ? nightlyRate : InnLodgingPaymentResolver.DefaultNightlyRate;
        }

        /// <summary>Injeção dos acessores de ouro para EditMode (sem PlayerManager/cena).</summary>
        public void ConfigureGoldAccessForTests(System.Func<int> currentGold, System.Func<int, bool> trySpend)
        {
            _currentGold = currentGold;
            _trySpend = trySpend;
        }

        public bool CanInteract(GameObject interactor)
        {
            return PlayerConditionService.Instance != null;
        }

        public void Interact(GameObject interactor)
        {
            if (PlayerConditionService.Instance == null)
            {
                Debug.LogWarning("InnBedPaymentGate: PlayerConditionService ausente. " +
                    "Cena: TownScene | GameObject: Interior_Inn/GuestBed | componente: InnBedPaymentGate | " +
                    "campo: PlayerConditionService.Instance runtime.", this);
                return;
            }

            // Passo 1 — confirmação (mesma janela do BedInteractable da F16).
            if (Time.unscaledTime > _confirmUntil)
            {
                _confirmUntil = Time.unscaledTime + ConfirmWindowSeconds;
                GameEventBus.Publish(new PlayerActionFeedbackEvent(
                    $"Dormir na estalagem por {_nightlyRate}g? Interaja novamente para confirmar."));
                return;
            }

            _confirmUntil = 0f;

            // Passo 2 — cobra a diária ANTES de dormir; só o caminho Paid debita e dorme.
            var result = InnLodgingPaymentResolver.TryPayNightlyRate(_nightlyRate, CurrentGold, TrySpend);
            switch (result.Outcome)
            {
                case InnLodgingPaymentResolver.Outcome.Paid:
                    PlayerConditionService.Instance.SleepInBed();
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        $"Você pagou {result.Cost}g e descansou na estalagem.", 3f));
                    break;

                case InnLodgingPaymentResolver.Outcome.NotEnoughGold:
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        $"Ouro insuficiente para a diária ({result.Cost}g)."));
                    break;

                default:
                    Debug.LogWarning("InnBedPaymentGate: acessores de ouro nao ligados (CityServiceRuntimeBootstrap " +
                        "esperado). Cena: TownScene | GameObject: Interior_Inn/GuestBed.", this);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("A estalagem nao pode cobrar agora."));
                    break;
            }
        }
    }
}
