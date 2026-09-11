using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Foundation.Time;
using CindarsHope.NPC.Runtime;
using UnityEngine;
using Unity.Profiling;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Runtime MonoBehaviour that resolves each town NPC's Work/Social/Home/Night block from the
    /// current hour and moves the NPC toward the matching anchor.
    ///
    /// fable_11 closes the two WAVE25 debts:
    /// - TIME_BLOCK_DEBT: blocks are now resolved per hour (via <see cref="NpcScheduleBlockResolver"/>),
    ///   not once per day. The service subscribes to <see cref="GameTimeTickEvent"/> and re-resolves
    ///   on every tick, publishing <see cref="NpcScheduleBlockChangedEvent"/> on block transitions.
    /// - SCENE_WIRING_DEBT: the generator now emits 3 anchors per NPC (work/social/home), registered
    ///   here at bootstrap.
    ///
    /// Movement uses <c>NpcWanderer.SetDestination</c>; an NPC blocked en route teleports to the anchor
    /// after a fallback timeout. NPCs mid-interaction keep their schedule paused (handled by the
    /// controllers + wanderer's existing SetInteractionPaused).
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcScheduleService : MonoBehaviour, INpcScheduleAvailabilityRuntime
    {
        // Schedule anchors are collision-audited pockets. Finishing broadly around them can leave
        // an actor beside a wall/table and make the next departure segment invalid.
        private const float ArriveRadius = 0.08f;

        // Faixa vertical onde vivem os interiores das casas (off-playfield, y>+40 — city_rules Rule 3).
        // Mudar para/desta faixa (NPC indo dormir em casa ou saindo de manhã) é um "salto" entre o
        // exterior e o interior: teleporta na hora em vez de deslizar pelo mapa inteiro. Como a faixa
        // é fora da câmera do jogador, o pop é invisível.
        private static readonly ProfilerMarker ResolveSchedulesMarker =
            new ProfilerMarker("CindarsHope.NpcSchedule.ResolveAll");

        private static NpcScheduleService s_instance;

        private readonly Dictionary<string, NpcScheduleAnchor> _anchors =
            new Dictionary<string, NpcScheduleAnchor>();

        private readonly Dictionary<string, NpcScheduleRuntimeState> _runtimeStates =
            new Dictionary<string, NpcScheduleRuntimeState>();

        private readonly Dictionary<string, NpcScheduleProfile> _profiles =
            new Dictionary<string, NpcScheduleProfile>();
        private NpcTownRouteGraph _routeGraph;

        // Registered NPC controllers (both dialogue and shop)
        private readonly List<NpcController> _npcControllers = new List<NpcController>();
        private readonly List<NpcShopController> _shopControllers = new List<NpcShopController>();

        // Tracks per-NPC movement (target + stuck timer) keyed by npcId.
        private readonly Dictionary<string, MoveOrder> _moveOrders = new Dictionary<string, MoveOrder>();

        private IGameClock _clock;
        private int _lastResolvedHour = -1;
        private bool _diagnosticMoveIsolation;

        public static NpcScheduleService Instance => s_instance;
        public int RegisteredProfileCount => _profiles.Count;
        public IReadOnlyCollection<string> RegisteredNpcIds => _profiles.Keys;

        /// <summary>Diagnostic seam used by the dedicated N1 physics harness; it still runs through
        /// the production progress/replan loop in FixedUpdate and never moves the body directly.</summary>
        public bool BeginTrackedMoveForDiagnostics(string npcId, NpcWanderer wanderer, Vector3 target)
        {
            if (string.IsNullOrEmpty(npcId) || wanderer == null) return false;
            _moveOrders[npcId] = new MoveOrder
            {
                Npc = wanderer.transform,
                Wanderer = wanderer,
                Target = target,
                Approach = target,
                HasApproach = false,
                ProgressElapsed = 0f,
                ProgressDistance = 0f,
                ProgressOrigin = wanderer.transform.position,
                Replans = 0
            };
            SetRouteToTarget(wanderer, wanderer.transform.position, target);
            return true;
        }

        public void ClearTrackedMovesForDiagnostics()
        {
            _moveOrders.Clear();
            _diagnosticMoveIsolation = true;
        }

        public void ResetDiagnosticMoveIsolation() => _diagnosticMoveIsolation = false;

        private struct MoveOrder
        {
            public Transform Npc;
            public NpcWanderer Wanderer;
            public Vector3 Target;
            public Vector3 Approach;
            public bool HasApproach;
            public float ProgressElapsed;
            public float ProgressDistance;
            public Vector3 ProgressOrigin;
            public int Replans;
        }

        public readonly struct MoveDiagnostic
        {
            public readonly string NpcId;
            public readonly Vector3 Target;
            public readonly float ProgressElapsed;
            public readonly float ProgressDistance;
            public readonly int Replans;

            public MoveDiagnostic(string npcId, Vector3 target, float progressElapsed,
                float progressDistance, int replans)
            {
                NpcId = npcId;
                Target = target;
                ProgressElapsed = progressElapsed;
                ProgressDistance = progressDistance;
                Replans = replans;
            }
        }

        public bool TryGetMoveDiagnostic(string npcId, out MoveDiagnostic diagnostic)
        {
            if (!string.IsNullOrEmpty(npcId) && _moveOrders.TryGetValue(npcId, out var order))
            {
                diagnostic = new MoveDiagnostic(npcId, order.Target, order.ProgressElapsed,
                    order.ProgressDistance, order.Replans);
                return true;
            }

            diagnostic = default;
            return false;
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DomainManagerRegistry.Register<INpcScheduleAvailabilityRuntime>(this);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Subscribe<GameTimeTickEvent>(HandleTimeTick);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Unsubscribe<GameTimeTickEvent>(HandleTimeTick);
        }

        private void OnDestroy()
        {
            if (s_instance == this)
            {
                DomainManagerRegistry.Unregister<INpcScheduleAvailabilityRuntime>(this);
                s_instance = null;
            }
        }

        /// <summary>Inject the time source (bootstrap). Optional — the service degrades to day-start only.</summary>
        public void SetTimeManager(GameTimeManager timeManager)
        {
            SetClock(timeManager);
        }

        public void SetClock(IGameClock clock)
        {
            _clock = clock;
            // A newly bound clock establishes a new time source. Force the next tick to resolve
            // even when its hour happens to equal the hour previously supplied by another clock.
            _lastResolvedHour = -1;
        }

        /// <summary>Register a named anchor for schedule position resolution.</summary>
        public void RegisterAnchor(NpcScheduleAnchor anchor)
        {
            if (anchor == null || string.IsNullOrEmpty(anchor.AnchorId))
            {
                return;
            }

            _anchors[anchor.AnchorId] = anchor;
        }

        public void RegisterRouteGraph(NpcTownRouteGraph graph) => _routeGraph = graph;

        /// <summary>Register an NpcController for schedule management and seed its profile.</summary>
        public void RegisterNpcController(NpcController controller)
        {
            if (controller != null && !_npcControllers.Contains(controller))
            {
                _npcControllers.Add(controller);
                EnsureProfile(controller.NpcData, hasShop: false);
            }
        }

        /// <summary>Register an NpcShopController for schedule management and seed its profile.</summary>
        public void RegisterShopController(NpcShopController controller)
        {
            if (controller != null && !_shopControllers.Contains(controller))
            {
                _shopControllers.Add(controller);
                EnsureProfile(controller.NpcData, hasShop: true);
            }
        }

        /// <summary>
        /// Register (or replace) the schedule profile for an NPC. Used by the generator/bootstrap to
        /// assign the canonical archetype (e.g. Yael/Maelor = Night). Idempotent.
        /// </summary>
        public void RegisterProfile(NpcScheduleProfile profile)
        {
            if (profile == null || string.IsNullOrEmpty(profile.NpcId))
            {
                return;
            }

            _profiles[profile.NpcId] = profile;
        }

        private void EnsureProfile(NpcDataSO npcData, bool hasShop)
        {
            if (npcData == null || string.IsNullOrEmpty(npcData.NpcId))
            {
                return;
            }

            if (_profiles.ContainsKey(npcData.NpcId))
            {
                return;
            }

            // Default archetype inferred from roster movement profile when available, else shop/wander.
            var archetype = NpcScheduleArchetype.Shopkeeper;
            if (!hasShop)
            {
                archetype = NpcScheduleArchetype.Wanderer;
            }

            _profiles[npcData.NpcId] = NpcScheduleProfile.CreateForArchetype(npcData.NpcId, archetype);
        }

        /// <summary>Try to resolve a named anchor position.</summary>
        public bool TryResolveAnchor(string anchorId, out Vector3 position)
        {
            position = Vector3.zero;
            if (string.IsNullOrEmpty(anchorId))
            {
                return false;
            }

            if (_anchors.TryGetValue(anchorId, out var anchor) && anchor != null)
            {
                position = anchor.GetPosition();
                return true;
            }

            return false;
        }

        /// <summary>Resolve the full anchor (position + optional door approach point).</summary>
        private bool TryGetAnchor(string anchorId, out NpcScheduleAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrEmpty(anchorId))
            {
                return false;
            }

            return _anchors.TryGetValue(anchorId, out anchor) && anchor != null;
        }

        /// <summary>
        /// fable_11 (CA-1) — current Work/Social/Home/Night block for an NPC at the given hour.
        /// Pure passthrough to the resolver using the NPC's registered archetype.
        /// </summary>
        public NpcRuntimeBlock GetCurrentBlock(string npcId, int hour)
        {
            var archetype = ArchetypeFor(npcId);
            return NpcScheduleBlockResolver.ResolveBlock(archetype, hour);
        }

        /// <summary>Current block using the live time source (falls back to hour 12 if no time manager).</summary>
        public NpcRuntimeBlock GetCurrentBlock(string npcId)
        {
            return GetCurrentBlock(npcId, CurrentHour());
        }

        /// <summary>
        /// fable_11 (CA-4) — is the NPC available for shop/dialogue right now? An NPC at Home/Night is
        /// unavailable; night-types invert (available only at night). Uses the live time source.
        /// </summary>
        public bool IsAvailable(string npcId)
        {
            return IsAvailable(npcId, CurrentHour());
        }

        /// <summary>Availability at an explicit hour (testable).</summary>
        public bool IsAvailable(string npcId, int hour)
        {
            var archetype = ArchetypeFor(npcId);
            return NpcScheduleBlockResolver.IsAvailable(archetype, hour);
        }

        private NpcScheduleArchetype ArchetypeFor(string npcId)
        {
            if (!string.IsNullOrEmpty(npcId) && _profiles.TryGetValue(npcId, out var profile) && profile != null)
            {
                return profile.Archetype;
            }

            return NpcScheduleArchetype.Shopkeeper;
        }

        private int CurrentHour()
        {
            return _clock != null ? _clock.CurrentHourOfDay : 12;
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            if (_diagnosticMoveIsolation) return;
            // Force a re-resolution at the start of each day.
            _lastResolvedHour = -1;
            ResolveAllNpcs(CurrentHour());
        }

        private void HandleTimeTick(GameTimeTickEvent evt)
        {
            if (_diagnosticMoveIsolation) return;
            var hour = CurrentHour();
            if (hour == _lastResolvedHour)
            {
                return;
            }

            ResolveAllNpcs(hour);
        }

        private void ResolveAllNpcs(int hour)
        {
            using var profilerScope = ResolveSchedulesMarker.Auto();
            _lastResolvedHour = hour;

            foreach (var controller in _npcControllers)
            {
                if (controller == null)
                {
                    continue;
                }

                ResolveNpc(controller.gameObject, controller.NpcData, hour,
                    controller.GetComponent<NpcWanderer>());
            }

            foreach (var controller in _shopControllers)
            {
                if (controller == null)
                {
                    continue;
                }

                ResolveNpc(controller.gameObject, controller.NpcData, hour,
                    controller.GetComponent<NpcWanderer>());
            }
        }

        private void ResolveNpc(GameObject npcGo, NpcDataSO npcData, int hour, NpcWanderer wanderer)
        {
            if (npcGo == null || npcData == null || string.IsNullOrEmpty(npcData.NpcId))
            {
                return;
            }

            var npcId = npcData.NpcId;
            var archetype = ArchetypeFor(npcId);
            var block = NpcScheduleBlockResolver.ResolveBlock(archetype, hour);
            var available = block == NpcRuntimeBlock.Work || block == NpcRuntimeBlock.Social;
            var suffix = NpcScheduleBlockResolver.AnchorSuffixForBlock(block);
            // NpcId ja e canonico (por exemplo, "npc_yael"). Prefixar novamente gerava
            // "npc_npc_yael_work" e quebrava o contrato usado pelos perfis e testes.
            var anchorId = $"{npcId}_{suffix}";

            // Resolve target position: named anchor first, else NpcDataSO.DefaultPosition.
            TryGetAnchor(anchorId, out var targetAnchor);
            // Compatibilidade temporaria com TownScene gerada antes da correcao do builder.
            // O estado publicado continua canonico; somente a leitura aceita o alias legado.
            if (targetAnchor == null)
            {
                TryGetAnchor($"npc_{anchorId}", out targetAnchor);
            }
            Vector3 targetPosition = targetAnchor != null ? targetAnchor.GetPosition() : npcData.DefaultPosition;

            // Update runtime state and publish the block transition (only on change).
            var changed = !_runtimeStates.TryGetValue(npcId, out var prev)
                          || prev == null
                          || prev.CurrentScheduleBlock != block.ToString();

            _runtimeStates[npcId] = new NpcScheduleRuntimeState
            {
                NpcId = npcId,
                CurrentScheduleId = $"schedule_{npcId}",
                CurrentScheduleBlock = block.ToString(),
                CurrentAnchorId = anchorId,
                IsAvailable = available
            };

            if (changed)
            {
                GameEventBus.Publish(new NpcScheduleBlockChangedEvent(npcId, block.ToString(), available));
            }

            // Order physical movement toward the target. A route graph is preferred; direct motion is
            // retained as a compatibility fallback for scenes generated before N1 nodes existed.
            _moveOrders[npcId] = new MoveOrder
            {
                Npc = npcGo.transform,
                Wanderer = wanderer,
                Target = targetPosition,
                Approach = targetAnchor != null && targetAnchor.HasApproach ? targetAnchor.ApproachPoint : targetPosition,
                HasApproach = targetAnchor != null && targetAnchor.HasApproach,
                ProgressElapsed = 0f,
                ProgressDistance = 0f,
                ProgressOrigin = npcGo.transform.position,
                Replans = 0
            };

            if (wanderer != null)
            {
                // Se o anchor tem ponto de aproximação (a porta), o NPC passa por ele antes de entrar —
                // usa a porta em vez de cruzar a parede.
                if (targetAnchor != null && targetAnchor.HasApproach)
                {
                    SetRouteOrDirect(wanderer, npcGo.transform.position, targetAnchor.ApproachPoint, targetPosition);
                }
                else
                {
                    // A target without door approach still needs the full Town graph. Routing
                    // from->from and appending target produced one giant unchecked final segment.
                    SetRouteToTarget(wanderer, npcGo.transform.position, targetPosition);
                }
            }
        }

        private void SetRouteOrDirect(NpcWanderer wanderer, Vector3 from, Vector3 approach, Vector3 target)
        {
            var points = new List<Vector3>();
            if (_routeGraph != null && _routeGraph.TryBuildRoute(from, approach, points,
                    GetMovementCollider(wanderer)))
            {
                if (Vector2.Distance(approach, target) > ArriveRadius) points.Add(target);
                wanderer.SetRoute(points, ArriveRadius);
                return;
            }

            if (Vector2.Distance(approach, target) > ArriveRadius)
                wanderer.SetDestination(target, approach, ArriveRadius);
            else
                wanderer.SetDestination(target, ArriveRadius);
            Debug.LogWarning($"[NpcScheduleService] Graph route unavailable reason={_routeGraph?.LastFailureDiagnostic ?? "graph_missing"} from={from} via={approach} to={target}");
        }

        private void SetRouteToTarget(NpcWanderer wanderer, Vector3 from, Vector3 target)
        {
            var points = new List<Vector3>();
            if (_routeGraph != null && _routeGraph.TryBuildRoute(from, target, points,
                    GetMovementCollider(wanderer)))
            {
                wanderer.SetRoute(points, ArriveRadius);
                return;
            }

            wanderer.SetDestination(target, ArriveRadius);
            Debug.LogWarning($"[NpcScheduleService] Graph route unavailable reason={_routeGraph?.LastFailureDiagnostic ?? "graph_missing"} from={from} to={target}");
        }

        private static Collider2D GetMovementCollider(NpcWanderer wanderer)
        {
            if (wanderer == null) return null;
            var colliders = wanderer.GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < colliders.Length; i++)
                if (colliders[i] != null && !colliders[i].isTrigger) return colliders[i];
            return colliders.Length > 0 ? colliders[0] : null;
        }

        private void FixedUpdate()
        {
            if (_moveOrders.Count == 0)
            {
                return;
            }

            // Iterate over a snapshot of keys so we can mutate the dictionary.
            _stuckScratch.Clear();
            foreach (var kvp in _moveOrders)
            {
                _stuckScratch.Add(kvp.Key);
            }

            for (var i = 0; i < _stuckScratch.Count; i++)
            {
                var npcId = _stuckScratch[i];
                if (!_moveOrders.TryGetValue(npcId, out var order) || order.Npc == null)
                {
                    _moveOrders.Remove(npcId);
                    continue;
                }

                var distance = Vector2.Distance(order.Npc.position, order.Target);
                if (distance <= ArriveRadius)
                {
                    _moveOrders.Remove(npcId);
                    continue;
                }

                order.ProgressElapsed += Time.fixedDeltaTime;
                // Progress means distance actually closed toward the requested target. Counting raw
                // displacement lets a body slide sideways along a wall forever without triggering
                // the bounded replan contract.
                order.ProgressDistance = NpcRouteRecoveryPolicy.ProgressTowardTarget(
                    order.ProgressOrigin, order.Npc.position, order.Target);
                if (order.ProgressElapsed >= NpcRouteRecoveryPolicy.ProgressWindowSeconds)
                {
                    // Deterministic right-of-way is expected waiting, not a blocked route. Restart
                    // the progress window while the lower-ordinal body owns the convergent lane.
                    if (order.Wanderer != null && !string.IsNullOrEmpty(order.Wanderer.YieldingToNpcId))
                    {
                        order.ProgressElapsed = 0f;
                        order.ProgressOrigin = order.Npc.position;
                        order.ProgressDistance = 0f;
                        _moveOrders[npcId] = order;
                        continue;
                    }
                    var progress = order.ProgressDistance;
                    order.ProgressElapsed = 0f;
                    order.ProgressOrigin = order.Npc.position;
                    order.ProgressDistance = 0f;
                    if (progress < NpcRouteRecoveryPolicy.MinimumProgress &&
                        NpcRouteRecoveryPolicy.CanReplan(order.Replans) && order.Wanderer != null)
                    {
                        order.Replans++;
                        if (order.HasApproach)
                            SetRouteOrDirect(order.Wanderer, order.Npc.position, order.Approach, order.Target);
                        else
                            SetRouteToTarget(order.Wanderer, order.Npc.position, order.Target);
                        Debug.LogWarning($"[NpcScheduleService] Replan {order.Replans}/2 npc={npcId} reason=insufficient_progress from={order.Npc.position} to={order.Target}");
                    }
                    else if (progress < NpcRouteRecoveryPolicy.MinimumProgress && order.Wanderer != null)
                    {
                        // Route failure is not an interaction lock. Keep the actor safely stopped,
                        // but allow a later schedule block/load restore to issue a fresh route.
                        order.Wanderer.ClearDestination();
                        Debug.LogWarning($"[NpcScheduleService] Waiting at safe position npc={npcId} reason=route_blocked from={order.Npc.position} to={order.Target}");
                    }
                }

                _moveOrders[npcId] = order;
            }
        }

        private readonly List<string> _stuckScratch = new List<string>();

        /// <summary>Get the current runtime state of an NPC.</summary>
        public bool TryGetRuntimeState(string npcId, out NpcScheduleRuntimeState state)
        {
            return _runtimeStates.TryGetValue(npcId, out state);
        }

        public bool TryGetTrackedAvailability(string npcId, out bool isAvailable)
        {
            isAvailable = false;
            if (!TryGetRuntimeState(npcId, out var state) || state == null)
            {
                return false;
            }

            isAvailable = state.IsAvailable;
            return true;
        }

        /// <summary>Load/offscreen-only recovery; circulation callers must pass false and remain physical.</summary>
        public bool TrySnapOffscreen(NpcWanderer wanderer, Vector3 safePosition, bool offscreen)
            => TrySnapOffscreen(wanderer, safePosition, offscreen, "offscreen_load_recovery");

        public bool TryRestoreLoadedPosition(NpcWanderer wanderer, Vector3 requestedPosition)
        {
            if (wanderer == null) return false;
            // Save restore can run during scene bootstrap before the first physics step. The query
            // must observe the final authored collider transforms, otherwise an occupied save point
            // can be misclassified as clear and accepted verbatim.
            Physics2D.SyncTransforms();
            var self = GetMovementCollider(wanderer);
            if (NpcRouteRecoveryPolicy.IsSafePoint(requestedPosition, 0.3f, self))
                return TrySnapOffscreen(wanderer, requestedPosition, true, "load_restore_exact");
            if (_routeGraph != null && _routeGraph.TryFindNearestSafeUnoccupied(
                    requestedPosition, 0.3f, self, out var safeNode))
                return TrySnapOffscreen(wanderer, safeNode, true, "load_restore_safe_node");
            return false;
        }

        private bool TrySnapOffscreen(NpcWanderer wanderer, Vector3 safePosition, bool offscreen, string reason)
        {
            if (wanderer == null || !NpcRouteRecoveryPolicy.CanSnap(offscreen) ||
                !NpcRouteRecoveryPolicy.IsSafePoint(safePosition, 0.3f, GetMovementCollider(wanderer)))
                return false;

            var from = wanderer.transform.position;
            wanderer.transform.position = safePosition;
            Debug.Log($"[NpcScheduleService] Offscreen safe snap npc={wanderer.StableNpcId} " +
                      $"reason={reason} from={from} to={safePosition}");
            return true;
        }
    }
}
