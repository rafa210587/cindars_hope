using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Movimento "vivo" de um NPC. TODO NPC tem um idle-wander (vaivém curto e aleatório em torno do
    /// seu ponto), com raio/velocidade por tier (lojista = micro-movimento no posto; andarilho = raio
    /// maior pela cidade). A configuração é própria deste componente (serializada pelo gerador) e não
    /// depende mais de <c>NpcDataSO.MovementMode/WanderData</c>, para que mesmo lojistas estáticos
    /// pareçam vivos.
    ///
    /// O agendamento (NpcScheduleService) pode sobrepor um destino (ir trabalhar/dormir), opcionalmente
    /// passando por um waypoint (o vão da porta) antes do alvo final — assim o NPC entra em casa pela
    /// porta em vez de atravessar a parede. Durante uma conversa o vaivém pausa (via GameEventBus).
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcWanderer : MonoBehaviour
    {
        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private Rigidbody2D _rigidbody;

        [Header("Idle wander (todos os NPCs)")]
        [SerializeField] private float _moveSpeed = 1.3f;
        [SerializeField] private float _idleWanderRadius = 1.2f;   // ~2 tiles
        [SerializeField] private float _pauseMin = 2.5f;
        [SerializeField] private float _pauseMax = 5.5f;
        [Tooltip("Clamp duro para nunca sair do playfield da cidade.")]
        [SerializeField] private Vector2 _townClampMin = new Vector2(-37f, -31f);
        [SerializeField] private Vector2 _townClampMax = new Vector2(37f, 31f);

        // Centro do vaivém: re-ancorado quando o NPC chega a um destino de agenda (trabalho/casa).
        private Vector3 _homeSpot;
        private Vector3 _idleTarget;
        private float _pauseTimer;
        private bool _isPaused = true;
        private bool _isInteractionPaused;

        // Destino de agenda (override): waypoint opcional (porta) → alvo final (interior).
        private bool _hasScheduleDestination;
        private bool _hasWaypoint;
        private Vector3 _waypoint;
        private Vector3 _scheduleDestination;
        private float _scheduleArriveRadius = 0.3f;
        private readonly List<Vector3> _route = new List<Vector3>();
        private int _routeIndex;

        // Route corners must be reached closely. A broad radius makes a fast actor cut the corner
        // and drive its feet collider into a wall even when both serialized graph edges are clear.
        private const float WaypointArriveRadius = 0.01f;
        private const float FinalLandingEpsilon = 0.001f;

        public bool HasActiveRoute => _hasScheduleDestination && _route.Count > 1;
        public int RouteIndex => _routeIndex;
        public int RouteCount => _route.Count;
        public Vector3 CurrentRouteTarget => _routeIndex > 0 && _routeIndex < _route.Count
            ? _route[_routeIndex] : _scheduleDestination;
        public string StableNpcId
        {
            get
            {
                if (_npcData != null && !string.IsNullOrEmpty(_npcData.NpcId)) return _npcData.NpcId;
                var dweller = GetComponent<NpcDweller>();
                return dweller != null && !string.IsNullOrEmpty(dweller.NpcId) ? dweller.NpcId : name;
            }
        }
        public string YieldingToNpcId { get; private set; }
        public string WaitingForDoorName { get; private set; }

        private void Start()
        {
            _homeSpot = transform.position;
            _idleTarget = _homeSpot;
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }

            StartPause();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<NpcInteractionStartedEvent>(OnInteractionStarted);
            GameEventBus.Subscribe<NpcInteractionEndedEvent>(OnInteractionEnded);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<NpcInteractionStartedEvent>(OnInteractionStarted);
            GameEventBus.Unsubscribe<NpcInteractionEndedEvent>(OnInteractionEnded);
        }

        private void OnInteractionStarted(NpcInteractionStartedEvent evt)
        {
            if (_npcData != null && evt.NpcId == _npcData.NpcId)
            {
                SetInteractionPaused(true);
            }
        }

        private void OnInteractionEnded(NpcInteractionEndedEvent evt)
        {
            if (_npcData != null && evt.NpcId == _npcData.NpcId)
            {
                SetInteractionPaused(false);
            }
        }

        private void FixedUpdate()
        {
            if (_isInteractionPaused)
            {
                StopMotion();
                return;
            }

            if (_hasScheduleDestination)
            {
                MoveTowardScheduleDestination();
                return;
            }

            if (_isPaused)
            {
                UpdatePause();
            }
            else
            {
                MoveTowardIdleTarget();
            }
        }

        /// <summary>
        /// Ordena o NPC a caminhar até um destino de agenda. Sobrepõe o vaivém até chegar.
        /// </summary>
        public void SetDestination(Vector2 target, float arriveRadius)
        {
            _route.Clear();
            _routeIndex = 0;
            _hasWaypoint = false;
            _scheduleDestination = target;
            _scheduleArriveRadius = Mathf.Max(0.05f, arriveRadius);
            _hasScheduleDestination = true;
            _isPaused = false;
        }

        /// <summary>
        /// Como <see cref="SetDestination(Vector2, float)"/>, mas passa primeiro por <paramref name="waypoint"/>
        /// (ex.: o vão da porta) antes do alvo final — entra em casa pela porta, sem cruzar a parede.
        /// </summary>
        public void SetDestination(Vector2 target, Vector2 waypoint, float arriveRadius)
        {
            _route.Clear();
            _routeIndex = 0;
            _waypoint = waypoint;
            _hasWaypoint = true;
            _scheduleDestination = target;
            _scheduleArriveRadius = Mathf.Max(0.05f, arriveRadius);
            _hasScheduleDestination = true;
            _isPaused = false;
        }

        /// <summary>Cancela um destino de agenda e volta ao vaivém local.</summary>
        public void ClearDestination()
        {
            _route.Clear();
            _routeIndex = 0;
            _hasScheduleDestination = false;
            _hasWaypoint = false;
            StopMotion();
            StartPause();
        }

        /// <summary>Move through a precomputed deterministic Town route, preserving physical motion.</summary>
        public void SetRoute(IReadOnlyList<Vector3> points, float arriveRadius)
        {
            _route.Clear();
            if (points != null)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    if (_route.Count == 0 || Vector2.Distance(_route[_route.Count - 1], points[i]) > 0.01f)
                        _route.Add(points[i]);
                }
            }
            _routeIndex = _route.Count > 1 ? 1 : 0;
            _hasWaypoint = false;
            _scheduleArriveRadius = Mathf.Max(0.05f, arriveRadius);
            _scheduleDestination = _route.Count > 0 ? _route[_route.Count - 1] : transform.position;
            _hasScheduleDestination = _route.Count > 1;
            if (_hasScheduleDestination)
            {
                _isPaused = false;
            }
            else
            {
                // An hourly schedule tick may legitimately reissue the anchor already occupied.
                // Treat that zero-length route as a completed arrival. Waking idle movement here
                // reused the old pre-schedule idle target and pulled residents back across Town.
                _homeSpot = transform.position;
                _idleTarget = _homeSpot;
                StartPause();
            }
        }

        private void MoveTowardScheduleDestination()
        {
            if (_routeIndex > 0 && _routeIndex < _route.Count)
            {
                var segment = _route[_routeIndex];
                var segmentArriveRadius = _routeIndex < _route.Count - 1
                    ? WaypointArriveRadius
                    : FinalLandingEpsilon;
                if (Vector2.Distance(transform.position, segment) <= segmentArriveRadius)
                {
                    _routeIndex++;
                    if (_routeIndex >= _route.Count)
                    {
                        _hasScheduleDestination = false;
                        StopMotion();
                        _homeSpot = transform.position;
                        _idleTarget = _homeSpot;
                        StartPause();
                        return;
                    }
                    segment = _route[_routeIndex];
                }
                MoveToward(segment);
                return;
            }

            // Primeiro o waypoint (porta), se houver.
            if (_hasWaypoint)
            {
                if (Vector2.Distance(transform.position, _waypoint) <= WaypointArriveRadius)
                {
                    _hasWaypoint = false;
                }
                else
                {
                    MoveToward(_waypoint);
                    return;
                }
            }

            float distance = Vector2.Distance(transform.position, _scheduleDestination);
            if (distance <= FinalLandingEpsilon)
            {
                _hasScheduleDestination = false;
                StopMotion();
                // Re-ancora o vaivém no ponto de chegada (mila em torno do destino atual).
                _homeSpot = transform.position;
                _idleTarget = _homeSpot;
                StartPause();
                return;
            }

            MoveToward(_scheduleDestination);
        }

        private void UpdatePause()
        {
            _pauseTimer -= Time.fixedDeltaTime;
            if (_pauseTimer <= 0f)
            {
                ChooseNewIdleTarget();
            }
        }

        private void MoveTowardIdleTarget()
        {
            if (Vector2.Distance(transform.position, _idleTarget) < 0.1f)
            {
                StartPause();
                return;
            }

            MoveToward(_idleTarget);
        }

        private void ChooseNewIdleTarget()
        {
            _isPaused = false;
            var randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var randomDistance = Random.Range(0f, _idleWanderRadius);
            var offset = new Vector2(Mathf.Cos(randomAngle) * randomDistance, Mathf.Sin(randomAngle) * randomDistance);
            var intended = (Vector2)_homeSpot + offset;
            _idleTarget = new Vector3(
                Mathf.Clamp(intended.x, _townClampMin.x, _townClampMax.x),
                Mathf.Clamp(intended.y, _townClampMin.y, _townClampMax.y),
                0f);
        }

        private void MoveToward(Vector3 worldTarget)
        {
            var direction = (Vector2)((worldTarget - transform.position));
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            // The proximity trigger starts the real door animation before the solid leaf clears.
            // At accelerated speeds, continuously pushing during those frames deflects an actor
            // sideways into the facade. Wait physically in the trigger until the blocker opens.
            if (IsWaitingForDoorToOpen())
            {
                StopMotion();
                return;
            }

            // Deterministic local right-of-way: at a convergent narrow corridor the lexically later
            // NPC yields. This avoids two bodies continuously driving into each other and makes the
            // winner independent of physics callback/order.
            YieldingToNpcId = null;
            var selfCollider = GetComponent<Collider2D>();
            var probeRadius = selfCollider != null ? Mathf.Max(0.24f, selfCollider.bounds.extents.magnitude + 0.08f) : 0.3f;
            var hits = Physics2D.OverlapCircleAll((Vector2)transform.position + direction * probeRadius, probeRadius);
            for (var i = 0; i < hits.Length; i++)
            {
                var other = hits[i] != null ? hits[i].GetComponentInParent<NpcWanderer>() : null;
                if (other == null || other == this || !other.isActiveAndEnabled) continue;
                // A body that already completed its route is not a competing claimant for the
                // corridor. Treating idle residents as perpetual priority holders can strand a
                // later-ID NPC on an otherwise open street.
                if (!other._hasScheduleDestination) continue;
                if (string.CompareOrdinal(StableNpcId, other.StableNpcId) > 0)
                {
                    YieldingToNpcId = other.StableNpcId;
                    if (_rigidbody != null) _rigidbody.linearVelocity = -direction * (_moveSpeed * 0.45f);
                    else transform.position -= (Vector3)direction * (_moveSpeed * 0.45f * Time.fixedDeltaTime);
                    return;
                }
            }

            if (_rigidbody != null)
            {
                // MovePosition stays in the physics step, respects solid contacts and lands on a
                // short final segment exactly; linear damping otherwise makes the last millimetres
                // asymptotic and leaves the next route starting off the audited anchor node.
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.MovePosition(Vector2.MoveTowards(_rigidbody.position, worldTarget,
                    _moveSpeed * Time.fixedDeltaTime));
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, worldTarget,
                    _moveSpeed * Time.fixedDeltaTime);
            }
        }

        private bool IsWaitingForDoorToOpen()
        {
            WaitingForDoorName = null;
            var colliders = GetComponentsInChildren<Collider2D>();
            Collider2D feet = null;
            for (var i = 0; i < colliders.Length; i++)
                if (colliders[i] != null && !colliders[i].isTrigger) { feet = colliders[i]; break; }
            if (feet == null) return false;
            var contacts = Physics2D.OverlapBoxAll(feet.bounds.center,
                (Vector2)feet.bounds.size + Vector2.one * 0.04f, 0f);
            for (var i = 0; i < contacts.Length; i++)
            {
                var contact = contacts[i];
                if (contact == null || !contact.isTrigger) continue;
                var door = contact.GetComponentInParent<HouseDoorInteractable>();
                if (door != null && door.IsAnimating && !door.IsOpen)
                {
                    WaitingForDoorName = door.transform.parent != null ? door.transform.parent.name : door.name;
                    return true;
                }
            }
            return false;
        }

        private void StartPause()
        {
            _isPaused = true;
            _pauseTimer = Random.Range(_pauseMin, _pauseMax);
            StopMotion();
        }

        public void SetInteractionPaused(bool isPaused)
        {
            _isInteractionPaused = isPaused;
            if (isPaused)
            {
                StopMotion();
            }
            else if (!_hasScheduleDestination)
            {
                StartPause();
            }
        }

        private void StopMotion()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>Configuração do tier de movimento pelo gerador (raio/velocidade/pausa e clamp).</summary>
        public void ConfigureMovement(float moveSpeed, float idleWanderRadius, float pauseMin, float pauseMax,
            Vector2 townClampMin, Vector2 townClampMax)
        {
            _moveSpeed = moveSpeed;
            _idleWanderRadius = idleWanderRadius;
            _pauseMin = pauseMin;
            _pauseMax = pauseMax;
            _townClampMin = townClampMin;
            _townClampMax = townClampMax;
        }
    }
}
