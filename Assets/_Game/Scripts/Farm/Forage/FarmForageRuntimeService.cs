using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Forage
{
    /// <summary>
    /// fable_54 — host runtime (padrao F15 / runtime-bootstrap) que da VIDA ao modulo orfao
    /// FarmForageSpawnService. No DayStartedEvent: resolve a ForageTable da estacao atual
    /// (ForageSeasonTable, dados em codigo v1), seleciona deterministicamente ate
    /// ForageSeasonTable.DailyActiveCount pontos (ForageStableHash sobre worldSeed+day — sem
    /// Random/GUID/timestamp) e (re)ativa os ForageSpawnState dos pontos da cena via TryRespawn/ativacao.
    ///
    /// Os pontos fisicos da cena se registram via RegisterScenePoint (refs serializadas pelo gerador;
    /// SEM GameObject.Find). Collect(spawnId) delega ao servico orfao (idempotente, zonas proibidas,
    /// estacao). NUNCA reescreve FarmForageSpawnService — hospeda e liga.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FarmForageRuntimeService : MonoBehaviour
    {
        private static FarmForageRuntimeService _instance;
        public static FarmForageRuntimeService Instance => _instance;

        // spawnId estavel (farm_forage_01..06) -> estado
        private readonly Dictionary<string, ForageSpawnState> _spawns = new Dictionary<string, ForageSpawnState>();
        // spawnId -> callbacks de visual da cena (ativar/depletar). Refs diretas do gerador.
        private readonly Dictionary<string, Action> _onActivated = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action> _onDepleted = new Dictionary<string, Action>();
        // ordem estavel de registro dos pontos da cena (para selecao por indice)
        private readonly List<string> _registeredOrder = new List<string>();

        private FarmForageSpawnService _service;
        private int _currentDay = 1;
        private string _worldSeed = "default";

        public int CurrentDay => _currentDay;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            BuildService();
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
            }
        }

        private void BuildService()
        {
            _service = new FarmForageSpawnService(ForageSeasonTable.AllDefinitionsById());
        }

        /// <summary>
        /// Ponto fisico da cena se registra (chamado pelo interactable em Start). spawnId estavel,
        /// zona (para regra de zona proibida do servico), callbacks de visual. Sem Find.
        /// </summary>
        public void RegisterScenePoint(string spawnId, string zoneId, int tileX, int tileY, Action onActivated, Action onDepleted)
        {
            if (string.IsNullOrWhiteSpace(spawnId)) return;

            if (!_spawns.TryGetValue(spawnId, out var state))
            {
                state = new ForageSpawnState
                {
                    ForageInstanceId = spawnId,
                    ForageId = string.Empty,
                    ZoneId = zoneId,
                    TileX = tileX,
                    TileY = tileY,
                    CurrentState = ForageNodeState.Hidden
                };
                _spawns[spawnId] = state;
                _registeredOrder.Add(spawnId);
            }
            else
            {
                state.ZoneId = zoneId;
                state.TileX = tileX;
                state.TileY = tileY;
            }

            _onActivated[spawnId] = onActivated;
            _onDepleted[spawnId] = onDepleted;

            // Aplica visual atual ja no registro (caso o load tenha ocorrido antes do ponto existir).
            ApplyVisual(spawnId, state);
        }

        public void SetDayContext(int dayNumber, string worldSeed)
        {
            _currentDay = Mathf.Max(1, dayNumber);
            if (!string.IsNullOrEmpty(worldSeed)) _worldSeed = worldSeed;
        }

        private string ResolveCurrentSeason()
        {
            // Estacao via WorldWeatherService (CurrentDay) -> GameDate -> Season. Fallback: dia atual.
            var day = _currentDay;
            var season = World.Calendar.GameDate.FromAbsoluteDay(day).CurrentSeason;
            return season.ToString(); // Primavera/Verao/Outono/Inverno
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = Mathf.Max(1, evt.DayNumber);
            RegenerateDailySpawns(_currentDay, ResolveCurrentSeason());
        }

        /// <summary>
        /// (Re)gera a selecao do dia. Determinista por (worldSeed, day): mesma entrada => mesma
        /// selecao. Pontos selecionados recebem um forageId da tabela da estacao e ficam Available;
        /// os nao selecionados ficam Hidden. Respawn por politica para pontos ja coletados.
        /// </summary>
        public void RegenerateDailySpawns(int dayNumber, string seasonId)
        {
            _currentDay = Mathf.Max(1, dayNumber);
            if (_service == null) BuildService();

            var table = ForageSeasonTable.ForSeason(seasonId);
            if (table.Count == 0 || _registeredOrder.Count == 0)
            {
                return;
            }

            var pointCount = _registeredOrder.Count;
            var activeIndices = ForageStableHash.SelectDailyIndices(
                _worldSeed, _currentDay, pointCount, Math.Min(ForageSeasonTable.DailyActiveCount, pointCount));
            var activeSet = new HashSet<int>(activeIndices);

            for (var i = 0; i < pointCount; i++)
            {
                var spawnId = _registeredOrder[i];
                if (!_spawns.TryGetValue(spawnId, out var state)) continue;

                if (activeSet.Contains(i))
                {
                    // Escolhe um item da tabela da estacao deterministicamente por (seed, day, spawnId).
                    var forageId = PickForageId(table, spawnId);
                    state.ForageId = forageId;

                    if (state.CurrentState == ForageNodeState.Collected)
                    {
                        // Tenta respawn pela politica do servico orfao; se nao elegivel, mantem coletado.
                        if (_service.TryRespawn(state, _currentDay, seasonId))
                        {
                            // respawn ok
                        }
                    }
                    else
                    {
                        state.CurrentState = ForageNodeState.Available;
                        state.SpawnedDay = _currentDay;
                        state.NextEligibleSpawnDay = -1;
                    }
                }
                else
                {
                    state.CurrentState = ForageNodeState.Hidden;
                }

                ApplyVisual(spawnId, state);
            }
        }

        private string PickForageId(IReadOnlyList<ForageDefinition> table, string spawnId)
        {
            var key = $"{ForageStableHash.Salt}|item|{_worldSeed}|{_currentDay}|{spawnId}";
            var h = (uint)ForageStableHash.Compute(key);
            var idx = (int)(h % (uint)table.Count);
            return table[idx].ForageId;
        }

        private void ApplyVisual(string spawnId, ForageSpawnState state)
        {
            if (state.IsAvailable)
            {
                if (_onActivated.TryGetValue(spawnId, out var act)) act?.Invoke();
            }
            else
            {
                if (_onDepleted.TryGetValue(spawnId, out var dep)) dep?.Invoke();
            }
        }

        /// <summary>
        /// Coleta um ponto. Delega ao servico orfao (idempotente: 2a coleta => ForageNotAvailable;
        /// zona proibida => ForbiddenZone; estacao errada => WrongSeason). Em sucesso, deple o visual.
        /// O chamador adiciona o item ao inventario.
        /// </summary>
        public ForageCollectResult Collect(string spawnId)
        {
            if (_service == null) BuildService();
            if (string.IsNullOrWhiteSpace(spawnId) || !_spawns.TryGetValue(spawnId, out var state))
            {
                return ForageCollectResult.Fail("SpawnNotFound");
            }

            var result = _service.Collect(state, _currentDay, ResolveCurrentSeason());
            if (result.Success)
            {
                ApplyVisual(spawnId, state);
            }
            return result;
        }

        public bool TryGetSpawnItem(string spawnId, out string forageId, out string itemId)
        {
            forageId = null;
            itemId = null;
            if (string.IsNullOrWhiteSpace(spawnId) || !_spawns.TryGetValue(spawnId, out var state))
            {
                return false;
            }

            forageId = state.ForageId;
            var defs = ForageSeasonTable.AllDefinitionsById();
            if (!string.IsNullOrEmpty(forageId) && defs.TryGetValue(forageId, out var def))
            {
                itemId = def.ItemId;
                return true;
            }
            return false;
        }

        // ─── Save (campo aditivo na secao farm; dono deste estado) ───

        public ForageSpawnsSaveData CaptureSaveData()
        {
            var data = new ForageSpawnsSaveData { Day = _currentDay };
            foreach (var spawnId in _registeredOrder)
            {
                if (!_spawns.TryGetValue(spawnId, out var state)) continue;
                data.Spawns.Add(new ForageSpawnSaveData
                {
                    SpawnId = spawnId,
                    ForageId = state.ForageId,
                    State = (int)state.CurrentState,
                    SpawnedDay = state.SpawnedDay,
                    CollectedDay = state.CollectedDay,
                    NextEligibleSpawnDay = state.NextEligibleSpawnDay
                });
            }
            return data;
        }

        /// <summary>
        /// Restaura o estado dos pontos. saveData nulo/ausente (legado) => nada a restaurar; os
        /// spawns serao regerados no proximo DayStarted. Idempotente.
        /// </summary>
        public void RestoreFromSaveData(ForageSpawnsSaveData saveData)
        {
            if (saveData == null || saveData.Spawns == null)
            {
                return;
            }

            if (saveData.Day > 0) _currentDay = saveData.Day;

            foreach (var saved in saveData.Spawns)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.SpawnId)) continue;

                if (!_spawns.TryGetValue(saved.SpawnId, out var state))
                {
                    state = new ForageSpawnState { ForageInstanceId = saved.SpawnId };
                    _spawns[saved.SpawnId] = state;
                    if (!_registeredOrder.Contains(saved.SpawnId)) _registeredOrder.Add(saved.SpawnId);
                }

                state.ForageId = saved.ForageId;
                state.CurrentState = (ForageNodeState)saved.State;
                state.SpawnedDay = saved.SpawnedDay;
                state.CollectedDay = saved.CollectedDay;
                state.NextEligibleSpawnDay = saved.NextEligibleSpawnDay;

                ApplyVisual(saved.SpawnId, state);
            }

            Debug.Log($"[FarmForageRuntimeService] Restaurado {saveData.Spawns.Count} pontos de forrageio.", this);
        }
    }

    /// <summary>
    /// Garante FarmForageRuntimeService em runtime (mesmo padrao dos outros bootstraps de farm).
    /// FindAnyObjectByType permitido: wiring de bootstrap, nao gameplay.
    /// </summary>
    public static class FarmForageRuntimeBootstrap
    {
        public static FarmForageRuntimeService Install(Transform owner)
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<FarmForageRuntimeService>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("FarmForageRuntimeService");
            if (owner != null) go.transform.SetParent(owner, false);
            else UnityEngine.Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<FarmForageRuntimeService>();
            Debug.Log("[FarmForageRuntimeBootstrap] FarmForageRuntimeService instanciado via bootstrap.");
            return service;
        }
    }
}
