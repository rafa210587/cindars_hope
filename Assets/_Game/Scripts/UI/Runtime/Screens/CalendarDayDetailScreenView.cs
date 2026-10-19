using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.UI.Calendar;
using CindarsHope.UI.Modal;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;
using UnityEngine;

namespace CindarsHope.UI.Runtime.Screens
{
    /// <summary>
    /// fable_20 — adapter fino da tela/aba "Detalhe do Dia" do Calendário. É a aba Calendário do
    /// painel único F14 (EMENDA 2026-06-12 ponto 3): abre via ModalManager (bloqueia gameplay,
    /// pausa o relógio pelo gate único de time_rules Rule 2), navega por teclado com o
    /// <see cref="UiFocusController"/> (F14) e fecha no Esc/back.
    ///
    /// Toda a montagem do conteúdo é a projeção pura <see cref="CalendarDayDetailProjectionBuilder"/>
    /// (spoiler-gated); esta View só orquestra modal/foco e expõe o modelo ao binding visual, que
    /// fica DEFERIDO para a validação final (DEFERRED_UI_VISUAL).
    ///
    /// Decisão de input (auditoria do router): a tecla "C" já está ocupada pelo Craft de bolso
    /// (CraftingModal._pocketCraftKey). Para não conflitar, o Calendário NÃO usa um global key novo —
    /// é alcançado como aba do painel único F14 (GameplayScreenTab.Calendar). Ver report.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CalendarDayDetailScreenView : MonoBehaviour
    {
        // Ids de foco navegável da tela (CA-3 "foco navegável"); estáveis para o binding visual.
        public const string FocusToday = "calendar.today";
        public const string FocusForecast = "calendar.forecast";
        public const string FocusLunar = "calendar.lunar";
        public const string FocusBirthdays = "calendar.birthdays";
        public const string FocusClose = "calendar.close";

        [SerializeField] private ModalManager _modalManager;

        private readonly UiFocusController _focus = new UiFocusController();
        private CalendarDayDetailModel _model;
        private bool _isOpen;

        public bool IsOpen => _isOpen;
        public CalendarDayDetailModel Model => _model;
        public UiFocusController Focus => _focus;

        public void Configure(ModalManager modalManager)
        {
            _modalManager = modalManager;
        }

        /// <summary>
        /// Abre a tela com o modelo já projetado. Empurra o modal (reusa ModalType.QuestLog-style?
        /// não: usa o ModalType do painel via tab) — aqui usa <see cref="ModalType.Inventory"/> como
        /// o painel único faz; o caller normalmente passa pela aba. Retorna false se outro modal
        /// estiver ativo (ModalManager rejeita).
        /// </summary>
        public bool Open(CalendarDayDetailModel model, ModalType panelModalType)
        {
            if (_isOpen) return false;
            if (_modalManager != null && !_modalManager.PushModal(panelModalType))
            {
                return false;
            }

            _model = model;
            _focus.SetElements(BuildFocusOrder(model));
            _isOpen = true;
            return true;
        }

        /// <summary>Esc/back: fecha a tela e solta o modal (gameplay/relógio voltam).</summary>
        public bool Close(ModalType panelModalType)
        {
            if (!_isOpen) return false;
            _modalManager?.TryPopIfCurrent(panelModalType);
            _focus.Clear();
            _isOpen = false;
            return true;
        }

        /// <summary>Roteia uma entrada de navegação para o foco; Cancel fecha a tela.</summary>
        public UiFocusController.FocusResult HandleInput(
            UiFocusController.FocusInput input, ModalType panelModalType)
        {
            var result = _focus.Apply(input);
            if (result == UiFocusController.FocusResult.Cancelled)
            {
                Close(panelModalType);
            }
            return result;
        }

        /// <summary>
        /// Ordem de foco determinística (CA-3). Inclui só as seções com conteúdo + o botão fechar;
        /// reflete o spoiler gate (sem evento lunar conhecido → sem foco lunar).
        /// </summary>
        public static IReadOnlyList<string> BuildFocusOrder(CalendarDayDetailModel model)
        {
            var order = new List<string> { FocusToday };
            if (model != null && model.HasForecast) order.Add(FocusForecast);
            if (model != null && model.HasLunarEvent) order.Add(FocusLunar);
            if (model != null && model.Birthdays != null && model.Birthdays.Count > 0)
            {
                order.Add(FocusBirthdays);
            }
            order.Add(FocusClose);
            return order;
        }
    }
}
