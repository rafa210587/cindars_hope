using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Faz um companheiro (ex.: gato do Eiran) seguir um alvo (ex.: o NPC hospedeiro)
    /// mantendo uma distancia-alvo curta, com leve atraso e parada quando ja esta perto.
    ///
    /// Logica pura em <see cref="CompanionFollowCore"/> (C# sem heranca de MonoBehaviour);
    /// este MonoBehaviour e o adapter que alimenta o core com o estado do mundo e aplica
    /// o resultado via Rigidbody2D.MovePosition (respeita fisica sem conflitar com o NpcWanderer
    /// do hospedeiro, que roda em FixedUpdate separado).
    ///
    /// Sem GameObject.Find / FindObjectOfType: o alvo e injetado pelo gerador de cena via
    /// <see cref="SetTarget"/> ou serializado via Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CompanionFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Rigidbody2D _rigidbody;

        [Header("Parametros de follow")]
        [Tooltip("Distancia abaixo da qual o companheiro para (ja esta perto o suficiente).")]
        [SerializeField] private float _stopRadius = 0.6f;

        [Tooltip("Distancia acima da qual o companheiro comeca a se mover em direcao ao alvo.")]
        [SerializeField] private float _startRadius = 1.0f;

        [Tooltip("Velocidade maxima de perseguicao (unidades/s).")]
        [SerializeField] private float _moveSpeed = 1.2f;

        [Tooltip("Leve atraso no movimento: fracao da posicao interpolada por FixedUpdate (0=sem atraso, 1=nunca chega).")]
        [Range(0f, 0.95f)]
        [SerializeField] private float _lag = 0.12f;

        private CompanionFollowCore _core;

        private void Awake()
        {
            _core = new CompanionFollowCore(_stopRadius, _startRadius, _moveSpeed, _lag);
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        /// <summary>
        /// Injeta o Transform-alvo em runtime (chamado pelo gerador ou por outro sistema).
        /// Nao usa Find: o caller passa a referencia diretamente.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void FixedUpdate()
        {
            if (_target == null || _core == null)
            {
                return;
            }

            var selfPos = (Vector2)transform.position;
            var targetPos = (Vector2)_target.position;
            var result = _core.Tick(selfPos, targetPos, Time.fixedDeltaTime);

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(result.NextPosition);
            }
            else
            {
                transform.position = result.NextPosition;
            }
        }
    }

    /// <summary>
    /// Core puro (sem MonoBehaviour, testavel em EditMode). Calcula a proxima posicao do
    /// companheiro dado self/target, sem conhecer o motor de fisica.
    /// </summary>
    public sealed class CompanionFollowCore
    {
        private readonly float _stopRadius;
        private readonly float _startRadius;
        private readonly float _moveSpeed;
        private readonly float _lag;

        public CompanionFollowCore(float stopRadius, float startRadius, float moveSpeed, float lag)
        {
            _stopRadius = Mathf.Max(0f, stopRadius);
            _startRadius = Mathf.Max(_stopRadius + 0.01f, startRadius);
            _moveSpeed = Mathf.Max(0.01f, moveSpeed);
            _lag = Mathf.Clamp01(lag);
        }

        public struct TickResult
        {
            public Vector2 NextPosition;
            public bool IsMoving;
        }

        /// <summary>
        /// Calcula a proxima posicao do companheiro.
        /// </summary>
        /// <param name="selfPos">Posicao atual do companheiro.</param>
        /// <param name="targetPos">Posicao atual do alvo.</param>
        /// <param name="dt">Delta time (FixedDeltaTime).</param>
        public TickResult Tick(Vector2 selfPos, Vector2 targetPos, float dt)
        {
            float distance = Vector2.Distance(selfPos, targetPos);

            if (distance <= _stopRadius)
            {
                // Ja esta perto — apenas aplica o lag (fica ligeiramente atras do alvo).
                var lerped = Vector2.Lerp(selfPos, targetPos, 1f - _lag);
                return new TickResult { NextPosition = lerped, IsMoving = false };
            }

            if (distance <= _startRadius)
            {
                // Zona de amortecimento: velocidade reduzida proporcional a distancia.
                float t = (distance - _stopRadius) / (_startRadius - _stopRadius);
                var direction = (targetPos - selfPos).normalized;
                var step = direction * (_moveSpeed * t * dt);
                return new TickResult { NextPosition = selfPos + step, IsMoving = true };
            }

            // Fora do raio de inicio: move em velocidade maxima em direcao ao alvo.
            {
                var direction = (targetPos - selfPos).normalized;
                var step = direction * (_moveSpeed * dt);
                return new TickResult { NextPosition = selfPos + step, IsMoving = true };
            }
        }
    }
}
