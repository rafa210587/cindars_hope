using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.World.Calendar;
using CindarsHope.World.Lunar;
using UnityEngine;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_20 — adapter fino (MonoBehaviour) do widget de relógio/calendário no
    /// GameplayHudCanvas. Toda a derivação de texto/estado vive na projeção pura
    /// <see cref="ClockCalendarHudWidgetModel"/>; esta View só:
    ///  - assina eventos do GameEventBus (fase, clima, nível) — comunicação de gameplay via bus,
    ///    nunca chamada direta MonoBehaviour→MonoBehaviour (Rule 2 unity-architecture);
    ///  - reflete data/lua a cada minuto in-game (não por frame) lendo os VALORES já expostos
    ///    pelos serviços via referências serializadas injetadas no setup (sem GameObject.Find).
    ///
    /// O binding visual de Text/Image (sprites, ícones) é feito no Editor/scene e fica DEFERIDO
    /// para a validação final (DEFERRED_UI_VISUAL) — esta View expõe o Model para esse binding.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClockCalendarHudWidgetView : MonoBehaviour
    {
        [SerializeField] private GameCalendarService _calendarService;
        [SerializeField] private LunarCycleService _lunarService;

        private readonly ClockCalendarHudWidgetModel _model = new ClockCalendarHudWidgetModel();
        private int _lastRenderedDay = -1;

        /// <summary>Projeção lida pelo binding visual (e por testes de adapter, se necessário).</summary>
        public ClockCalendarHudWidgetModel Model => _model;

        /// <summary>Injeção explícita de dependências (chamada pelo bootstrap do HUD, sem Find).</summary>
        public void Configure(GameCalendarService calendarService, LunarCycleService lunarService)
        {
            _calendarService = calendarService;
            _lunarService = lunarService;
            RefreshDateAndLunar(force: true);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<GamePhaseChangedEvent>(OnPhaseChanged);
            GameEventBus.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            GameEventBus.Subscribe<PlayerLevelChangedEvent>(OnLevelChanged);
            RefreshDateAndLunar(force: true);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GamePhaseChangedEvent>(OnPhaseChanged);
            GameEventBus.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
            GameEventBus.Unsubscribe<PlayerLevelChangedEvent>(OnLevelChanged);
        }

        private void Update()
        {
            // Atualiza data/lua só quando o dia muda (Rule "tick por frame custoso" da spec:
            // atualizar por evento/dia, não a cada frame). A fase/clima/nível já vêm por evento.
            RefreshDateAndLunar(force: false);
        }

        private void RefreshDateAndLunar(bool force)
        {
            if (_calendarService != null && _calendarService.IsInitialized)
            {
                GameDate date = _calendarService.CurrentDate;
                if (force || date.AbsoluteDay != _lastRenderedDay)
                {
                    _model.SetDate(date);
                    _lastRenderedDay = date.AbsoluteDay;
                }
            }

            if (_lunarService != null && _lunarService.IsInitialized)
            {
                _model.SetLunar(_lunarService.CurrentCycle);
            }
        }

        private void OnPhaseChanged(GamePhaseChangedEvent evt)
        {
            _model.SetPhase(evt.NewPhase == GamePhaseChangedEvent.GamePhase.Night);
        }

        private void OnWeatherChanged(WeatherChangedEvent evt)
        {
            _model.SetWeather(evt.Weather);
        }

        private void OnLevelChanged(PlayerLevelChangedEvent evt)
        {
            // XP corrente/threshold são donos do sistema de progressão; aqui só o nível é certo
            // pelo evento. O binding pode injetar XP via SetLevel quando o provider de XP existir.
            _model.SetLevel(evt.NewLevel, _model.CurrentXp, _model.XpForNextLevel);
        }
    }
}
