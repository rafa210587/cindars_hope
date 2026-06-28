using CindarsHope.Core;
using CindarsHope.Core.Events;
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

        private const float WaypointArriveRadius = 0.45f;

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
            _hasScheduleDestination = false;
            _hasWaypoint = false;
            StopMotion();
            StartPause();
        }

        private void MoveTowardScheduleDestination()
        {
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
            if (distance <= _scheduleArriveRadius)
            {
                _hasScheduleDestination = false;
                StopMotion();
                // Re-ancora o vaivém no ponto de chegada (mila em torno do destino atual).
                _homeSpot = transform.position;
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

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction * _moveSpeed;
            }
            else
            {
                transform.position += (Vector3)direction * _moveSpeed * Time.fixedDeltaTime;
            }
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
