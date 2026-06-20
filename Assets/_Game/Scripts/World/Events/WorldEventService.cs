using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.World.Events
{
    /// <summary>
    /// fable_37 — autoridade runtime dos EVENTOS DE MUNDO. A cada DayStartedEvent resolve, de forma
    /// 100% determinística (calendário + StableHash(worldSeed|dia)), {festival hoje?, pico lunar hoje?,
    /// evento aleatório?}, publica a resolução em WorldEventHooks.Active (ponto único lido pelos sistemas
    /// alvo), dispara FestivalStartedEvent/WorldEventStartedEvent, monta/desmonta as barracas da praça,
    /// aplica o pico de Lua Verde (crops +1 estágio) e anuncia por toast. NÃO persiste nada (tudo deriva
    /// de calendário + worldSeed — ver WorldEventResolver / time_rules.md Rule 8/9).
    ///
    /// Não duplica calendário/lua (WAVE 02) nem clima (F15): só DERIVA deles. O acessor estático Instance
    /// segue o idioma do WorldWeatherService/GreenhouseRuntimeHost (sem busca global de cena no gameplay).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldEventService : MonoBehaviour
    {
        public const string DefaultWorldSeed = "default";

        private static WorldEventService _instance;
        public static WorldEventService Instance => _instance;

        [SerializeField] private string _worldSeed = DefaultWorldSeed;

        private readonly List<GameObject> _festivalStalls = new List<GameObject>();
        private Transform _plazaAnchor;

        public int CurrentDay { get; private set; } = 1;
        public WorldDayResolution CurrentResolution { get; private set; }

        public string ActiveFestivalId => CurrentResolution?.FestivalId ?? string.Empty;
        public string ActiveLunarPeakId => CurrentResolution?.LunarPeakId ?? string.Empty;
        public string ActiveWorldEventId => CurrentResolution?.WorldEventId ?? string.Empty;

        public bool IsFestivalActive(string festivalId)
        {
            return !string.IsNullOrEmpty(festivalId)
                && CurrentResolution != null
                && CurrentResolution.FestivalId == festivalId;
        }

        /// <summary>Próximos festivais conhecidos (mural F34 / spoiler gate F20). knownFestivalIds nulo =
        /// todos públicos (festivais são públicos por padrão).</summary>
        public List<WorldEventDefinitions.FestivalDefinition> NextKnownFestivals(int count, ISet<string> knownFestivalIds = null)
        {
            return WorldEventResolver.NextKnownFestivals(CurrentDay + 1, count, knownFestivalIds);
        }

        /// <summary>Define a semente de mundo usada no sorteio diário (chamado pelo bootstrap/save de tempo).</summary>
        public void SetWorldSeed(string worldSeed)
        {
            if (!string.IsNullOrEmpty(worldSeed)) _worldSeed = worldSeed;
        }

        /// <summary>Define o ponto da praça onde as barracas nascem (injetado pelo gerador da cidade).</summary>
        public void SetPlazaAnchor(Transform anchor)
        {
            _plazaAnchor = anchor;
        }

        /// <summary>Resolve o dia <paramref name="dayNumber"/> e aplica todos os efeitos. Idempotente por dia.</summary>
        public void ResolveForDay(int dayNumber)
        {
            var day = Mathf.Max(1, dayNumber);
            CurrentDay = day;

            var resolution = WorldEventResolver.ResolveDay(_worldSeed, day);
            CurrentResolution = resolution;
            WorldEventHooks.Active = resolution; // ponto único lido pelos sistemas alvo

            ApplyFestival(resolution);
            ApplyGreenMoonGrowth(resolution);
            Announce(resolution);
            PublishEvents(resolution);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                if (WorldEventHooks.Active == CurrentResolution)
                {
                    WorldEventHooks.Active = null;
                }
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            ResolveForDay(evt.DayNumber);
        }

        // ── Festival: flag (via evento) + barracas spawn/despawn pelo serviço ─────────────────────
        private void ApplyFestival(WorldDayResolution resolution)
        {
            // Sempre desmonta as barracas do dia anterior antes de (re)montar — garante limpeza (CA-1).
            DespawnFestivalStalls();

            if (resolution == null || !resolution.HasFestival)
            {
                return;
            }

            var festival = WorldEventDefinitions.FindFestival(resolution.FestivalId);
            if (festival == null)
            {
                return;
            }

            SpawnFestivalStalls(festival);
        }

        private void SpawnFestivalStalls(WorldEventDefinitions.FestivalDefinition festival)
        {
            // 3 barracas: comida grátis, jogo de pesca (item aleatório), brinde. Placeholders runtime
            // (interactables temporários) ancorados na praça; desmontados no dia seguinte. Sem prefab/
            // asset: criados por código (a spec proíbe edição manual de .unity/.prefab/.asset).
            var basePosition = _plazaAnchor != null ? _plazaAnchor.position : Vector3.zero;
            var offsets = new[]
            {
                new Vector3(-2.5f, 1.5f, 0f),
                new Vector3(0f, 2.0f, 0f),
                new Vector3(2.5f, 1.5f, 0f)
            };
            var kinds = new[]
            {
                FestivalStallInteractable.StallKind.FreeFood,
                FestivalStallInteractable.StallKind.FishingGame,
                FestivalStallInteractable.StallKind.Gift
            };

            for (var i = 0; i < kinds.Length; i++)
            {
                var go = new GameObject($"FestivalStall_{festival.FestivalId}_{kinds[i]}");
                go.transform.position = basePosition + offsets[i];
                if (_plazaAnchor != null)
                {
                    go.transform.SetParent(_plazaAnchor, true);
                }

                var stall = go.AddComponent<FestivalStallInteractable>();
                stall.Configure(kinds[i], festival.FestivalId, festival.GiftItemId);
                _festivalStalls.Add(go);
            }
        }

        private void DespawnFestivalStalls()
        {
            for (var i = 0; i < _festivalStalls.Count; i++)
            {
                if (_festivalStalls[i] != null)
                {
                    Destroy(_festivalStalls[i]);
                }
            }

            _festivalStalls.Clear();
        }

        // ── Pico de Lua Verde: crops +1 estágio (hook único no farm) ──────────────────────────────
        private void ApplyGreenMoonGrowth(WorldDayResolution resolution)
        {
            if (resolution == null || resolution.LunarEffect != WorldEventDefinitions.LunarPeakEffect.GreenCropGrowth)
            {
                return;
            }

            // O serviço DIRIGE o hook nomeado do farm (o farm não conhece o WorldEventService): o hook
            // localiza o registro de canteiros e avança +N estágios. Sem busca global de gameplay aqui —
            // a localização do registro fica encapsulada no hook (mesma fronteira do RainIrrigation).
            var stages = WorldEventHooks.GetGreenMoonExtraStages();
            if (stages > 0)
            {
                Farm.WorldEventFarmHook.AdvanceAllCropsStages(stages);
            }
        }

        // ── Anúncio (fluxo de toast existente — não cria segundo fluxo) ───────────────────────────
        private void Announce(WorldDayResolution resolution)
        {
            if (resolution == null) return;

            if (resolution.HasFestival)
            {
                var festival = WorldEventDefinitions.FindFestival(resolution.FestivalId);
                if (festival != null)
                {
                    GameEventBus.Publish(new NotificationToastRequestedEvent($"Hoje é {festival.DisplayName}!"));
                }
            }

            if (resolution.HasLunarPeak)
            {
                var peak = WorldEventDefinitions.FindPeak(resolution.LunarPeakId);
                if (peak != null)
                {
                    GameEventBus.Publish(new NotificationToastRequestedEvent($"Pico lunar: {peak.DisplayName}."));
                }
            }

            if (resolution.HasWorldEvent)
            {
                var worldEvent = WorldEventDefinitions.FindRandomEvent(resolution.WorldEventId);
                if (worldEvent != null && !string.IsNullOrEmpty(worldEvent.DisplayName))
                {
                    GameEventBus.Publish(new NotificationToastRequestedEvent(worldEvent.DisplayName));
                }
            }
        }

        private void PublishEvents(WorldDayResolution resolution)
        {
            if (resolution == null) return;

            if (resolution.HasFestival)
            {
                var festival = WorldEventDefinitions.FindFestival(resolution.FestivalId);
                GameEventBus.Publish(new FestivalStartedEvent(
                    resolution.FestivalId,
                    festival != null ? festival.DisplayName : resolution.FestivalId,
                    CurrentDay));
            }

            if (resolution.HasLunarPeak || resolution.HasWorldEvent)
            {
                var message = BuildAnnouncementMessage(resolution);
                GameEventBus.Publish(new WorldEventStartedEvent(
                    resolution.LunarPeakId,
                    resolution.WorldEventId,
                    message,
                    CurrentDay));
            }
        }

        private static string BuildAnnouncementMessage(WorldDayResolution resolution)
        {
            if (resolution.HasWorldEvent)
            {
                var worldEvent = WorldEventDefinitions.FindRandomEvent(resolution.WorldEventId);
                if (worldEvent != null) return worldEvent.DisplayName;
            }

            if (resolution.HasLunarPeak)
            {
                var peak = WorldEventDefinitions.FindPeak(resolution.LunarPeakId);
                if (peak != null) return $"Pico lunar: {peak.DisplayName}.";
            }

            return string.Empty;
        }
    }
}
