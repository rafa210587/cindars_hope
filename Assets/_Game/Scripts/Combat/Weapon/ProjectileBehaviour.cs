using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat.Weapon
{
    [DisallowMultipleComponent]
    public class ProjectileBehaviour : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _range = 6f;
        [SerializeField] private int _baseDamage = 5;
        [SerializeField] private DamageType _damageType = DamageType.Physical;
        [SerializeField] private float _knockbackForce = 2f;
        [SerializeField] private Rigidbody2D _rigidbody;
        // Keep this serialized field as Collider2D so projectile prefabs can use either
        // CircleCollider2D or BoxCollider2D without Unity YAML type mismatch errors.
        [SerializeField] private Collider2D _collider;
        [SerializeField] private CindarsHope.Combat.StatusEffect.StatusEffectSO _statusEffect;
        [SerializeField] private float _statusApplyChance = 0f;

        [SerializeField] private int _maxHits = 1;

        // fable_48 (aditivo): tags de material/elemento da muniÃ§Ã£o (ex.: "Silver"/"Fire"). Anexadas
        // ao DamageRequest no impacto para o matching de vulnerabilidade F06. Null => sem tags.
        private string[] _appliedTags;

        private Vector2 _spawnPosition;
        private int _hitCount;
        private IProjectileHitPolicy _hitPolicy;
        private float _postureDamageMultiplier;
        private int _sourceCasterRuntimeId;
        private bool _stopOnSolidObstacle;
        private System.Func<bool, ProjectileImpactDamage> _impactDamageResolver;
        private System.Func<ProjectileImpactContext, ProjectileImpactDamage>
            _targetedImpactDamageResolver;
        private DamageSourceKind _sourceKind;
        private SpellDiscipline _spellDiscipline;
        private string _sourceInstanceId = string.Empty;
        private string _actionToken = string.Empty;
        private bool _canTriggerCapstones;
        private bool _canTriggerStatusEffects = true;
        private bool _canTriggerReactions = true;

        // Game-feel de desaceleracao: a flecha comeca rapida e perde velocidade conforme avanca,
        // chegando a (_speedDecayToFraction * velocidade inicial) no alcance maximo. 1 = constante.
        private Vector2 _direction = Vector2.right;
        private float _initialSpeed;
        private float _speedDecayToFraction = 1f;

        // Encolhimento VISUAL no fim do voo (so quando ha decay): a flecha diminui de tamanho perto do
        // alcance maximo. O COLIDER e compensado para a colisao em world-space ficar CONSTANTE (acerto
        // fisico nao muda). Escala visual minima atingida no alcance maximo.
        private const float MinVisualScaleFraction = 0.45f;
        private Vector3 _baseScale = Vector3.one;
        private bool _hasCircleCollider;
        private float _baseCircleRadius;

        // Homing: persegue o inimigo mais proximo dentro de _homingRange, curvando a velocidade a uma
        // taxa fixa. 0 = sem perseguicao. Usa OverlapCircleNonAlloc (sem busca global, sem alocacao).
        private const float HomingTurnRateDegPerSec = 360f;
        private float _homingRange;
        private static readonly Collider2D[] s_homingBuffer = new Collider2D[16];
        // NoFilter() = todos os layers + triggers (mesma abrangencia do antigo OverlapCircleNonAlloc, que
        // usava o queriesHitTriggers global). A filtragem real por inimigo vivo e feita via EnemyHealth abaixo.
        private static readonly ContactFilter2D s_homingFilter = ContactFilter2D.noFilter;

        private void Start()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            _spawnPosition = transform.position;
        }

        private void Update()
        {
            float distanceTraveled = Vector2.Distance(transform.position, _spawnPosition);
            if (distanceTraveled > _range)
            {
                Destroy(gameObject);
            }
        }

        // Desaceleracao + encolhimento visual por distancia (so quando _speedDecayToFraction < 1).
        // Curva eased (t^2): fica cheia/rapida na maior parte do voo e cai PERTO DO FINAL. O collider
        // e compensado para o acerto fisico permanecer constante mesmo com o sprite menor.
        private void FixedUpdate()
        {
            if (_rigidbody == null)
            {
                return;
            }

            // Homing: curva a velocidade em direcao ao inimigo mais proximo dentro do alcance.
            if (_homingRange > 0f)
            {
                ApplyHoming();
            }

            if (_initialSpeed <= 0f || _speedDecayToFraction >= 1f)
            {
                return;
            }

            float traveled = Vector2.Distance(_rigidbody.position, _spawnPosition);
            float t = _range > 0f ? Mathf.Clamp01(traveled / _range) : 0f;
            float endWeighted = t * t; // concentra o efeito no final do voo

            float currentSpeed = Mathf.Lerp(_initialSpeed, _initialSpeed * _speedDecayToFraction, endWeighted);
            _rigidbody.linearVelocity = _direction * currentSpeed;

            // Encolhe o VISUAL; compensa o collider para a colisao em world-space ficar constante.
            float visualFactor = Mathf.Lerp(1f, MinVisualScaleFraction, endWeighted);
            transform.localScale = _baseScale * visualFactor;
            if (_hasCircleCollider && visualFactor > 0.001f && _collider is CircleCollider2D circle)
            {
                circle.radius = _baseCircleRadius / visualFactor;
            }
        }

        // Curva a velocidade em direcao ao inimigo mais proximo dentro de _homingRange, preservando a
        // velocidade atual. Sem alvo no alcance, segue reto. Tambem gira o sprite para a direcao.
        private void ApplyHoming()
        {
            var target = FindNearestEnemy(_rigidbody.position, _homingRange);
            if (target == null)
            {
                return;
            }

            Vector2 toTarget = ((Vector2)target.position - _rigidbody.position);
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float maxRadians = HomingTurnRateDegPerSec * Mathf.Deg2Rad * Time.fixedDeltaTime;
            Vector3 newDir = Vector3.RotateTowards((Vector3)_direction, (Vector3)toTarget.normalized, maxRadians, 0f);
            _direction = ((Vector2)newDir).normalized;

            float speed = _rigidbody.linearVelocity.magnitude;
            if (speed < 0.01f)
            {
                speed = _initialSpeed;
            }
            _rigidbody.linearVelocity = _direction * speed;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Inimigo vivo mais proximo do centro dentro do raio. OverlapCircleNonAlloc + buffer estatico
        // (sem busca global, sem alocacao por frame). Sobe ate o EnemyHealth (collider pode ser filho).
        private static Transform FindNearestEnemy(Vector2 center, float radius)
        {
            int count = Physics2D.OverlapCircle(center, radius, s_homingFilter, s_homingBuffer);
            Transform best = null;
            float bestSq = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var col = s_homingBuffer[i];
                if (col == null)
                {
                    continue;
                }

                var enemy = col.GetComponentInParent<EnemyHealth>() ?? col.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float sq = ((Vector2)enemy.transform.position - center).sqrMagnitude;
                if (sq < bestSq)
                {
                    bestSq = sq;
                    best = enemy.transform;
                }
            }

            return best;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hitCount >= _maxHits)
                return;

            var enemyHealth = collision.GetComponentInParent<EnemyHealth>() ?? collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                float hitMultiplier = 1f;
                if (_hitPolicy != null && !_hitPolicy.TryResolveHit(enemyHealth.GetEntityId().GetHashCode(), out hitMultiplier))
                    return;
                _hitCount++;
                HitEnemy(enemyHealth, hitMultiplier);
                if (_hitCount >= _maxHits)
                {
                    Destroy(gameObject);
                }
            }
            else if (_stopOnSolidObstacle && !collision.isTrigger)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Allows piercing projectiles (line piercer skill). 1 = despawn on first hit.</summary>
        public void SetMaxHits(int maxHits)
        {
            _maxHits = Mathf.Max(1, maxHits);
        }

        public void SetSkillHitPayload(IProjectileHitPolicy hitPolicy,
            float postureDamageMultiplier, int sourceCasterRuntimeId, bool stopOnSolidObstacle)
        {
            _hitPolicy = hitPolicy;
            _postureDamageMultiplier = Mathf.Max(0f, postureDamageMultiplier);
            _sourceCasterRuntimeId = sourceCasterRuntimeId;
            _stopOnSolidObstacle = stopOnSolidObstacle;
        }

        public void SetCombatContext(System.Func<bool, ProjectileImpactDamage> impactDamageResolver,
            DamageSourceKind sourceKind, SpellDiscipline spellDiscipline,
            string sourceInstanceId, string actionToken,
            bool canTriggerCapstones, bool canTriggerStatusEffects, bool canTriggerReactions)
        {
            _impactDamageResolver = impactDamageResolver;
            _sourceKind = sourceKind;
            _spellDiscipline = spellDiscipline;
            _sourceInstanceId = sourceInstanceId ?? string.Empty;
            _actionToken = actionToken ?? string.Empty;
            _canTriggerCapstones = canTriggerCapstones;
            _canTriggerStatusEffects = canTriggerStatusEffects;
            _canTriggerReactions = canTriggerReactions;
        }

        public void SetTargetedImpactDamageResolver(
            System.Func<ProjectileImpactContext, ProjectileImpactDamage> resolver)
        {
            _targetedImpactDamageResolver = resolver;
        }

        /// <summary>
        /// fable_48 â€” anexa tags de material/elemento da muniÃ§Ã£o (ex.: flecha "Silver"/"Fire"),
        /// propagadas ao DamageRequest no impacto para o matching de vulnerabilidade F06. Aditivo;
        /// chamado pelo ProjectileSpawnService. Null/vazio mantÃ©m o comportamento sem tags.
        /// </summary>
        public void SetAppliedTags(string[] appliedTags)
        {
            _appliedTags = appliedTags;
        }

        private void HitEnemy(EnemyHealth enemyHealth, float hitMultiplier)
        {
            float markedMultiplier = 1f;
            var marked = enemyHealth.GetComponent<IRangedDamageModifierRuntime>();
            if (marked != null)
                markedMultiplier = marked.ResolveRangedDamageMultiplier(_sourceCasterRuntimeId);
            var vulnerability = enemyHealth.GetComponent<IEnemyVulnerabilityWindow>();
            bool guaranteedCritical = vulnerability != null && vulnerability.IsVulnerable;
            var impactContext = new ProjectileImpactContext(guaranteedCritical,
                enemyHealth.EnemyInstanceId, enemyHealth.transform.position.x,
                enemyHealth.transform.position.y);
            var impactDamage = _targetedImpactDamageResolver != null
                ? _targetedImpactDamageResolver(impactContext)
                : _impactDamageResolver != null
                    ? _impactDamageResolver(guaranteedCritical)
                    : new ProjectileImpactDamage(_baseDamage, false);
            int resolvedDamage = Mathf.Max(1,
                Mathf.RoundToInt(impactDamage.Damage * hitMultiplier * markedMultiplier));
            var damageRequest = new DamageRequest(enemyHealth.EnemyId, resolvedDamage)
            {
                DamageType = _damageType,
                SourcePosition = transform.position,
                KnockbackForce = _knockbackForce,
                // fable_48: tags da flecha viajam atÃ© o matching F06 (bÃ´nus sÃ³ com vulnerabilidade
                // declarada). Null/vazio para magias/projÃ©teis sem tags (comportamento inalterado).
                WeaponMaterialTags = _appliedTags,
                SourceKind = _sourceKind,
                SpellDiscipline = _spellDiscipline,
                SourceInstanceId = _sourceInstanceId,
                TargetInstanceId = enemyHealth.EnemyInstanceId,
                ActionToken = _actionToken,
                IsCritical = impactDamage.IsCritical,
                IsPrimaryDamage = true,
                CanTriggerCapstones = _canTriggerCapstones,
                CanTriggerStatusEffects = _canTriggerStatusEffects,
                CanTriggerReactions = _canTriggerReactions
            };

            enemyHealth.TakeDamage(damageRequest);

            if (_postureDamageMultiplier > 0f)
            {
                var posture = enemyHealth.GetComponent<EnemyPostureState>();
                posture?.ApplyPostureDamage(resolvedDamage * _postureDamageMultiplier);
            }

            if (_canTriggerStatusEffects && !enemyHealth.IsDead && _statusEffect != null &&
                _statusApplyChance > 0f && Random.value <= _statusApplyChance)
            {
                bool refreshed = enemyHealth.StatusEffects.HasStatusEffect(_statusEffect.Id);
                if (refreshed) enemyHealth.StatusEffects.RemoveStatusEffect(_statusEffect.Id);
                enemyHealth.ApplyStatusEffect(_statusEffect);
                if (refreshed)
                    GameEventBus.Publish(new StatusRefreshedEvent(enemyHealth.EnemyId, _statusEffect.Id, _statusEffect.DurationTurns));
                else
                    GameEventBus.Publish(new StatusAppliedEvent(enemyHealth.EnemyId, _statusEffect.Id, string.Empty, _statusEffect.DurationTurns));
            }
        }

        public void Initialize(Vector2 direction, float speed, float range, int baseDamage, DamageType damageType, float knockbackForce, float speedDecayToFraction = 1f, float homingRange = 0f)
        {
            // Start() sÃ³ roda no prÃ³ximo frame; caminhos procedurais chamam Initialize no mesmo frame
            // do AddComponent â€” cacheia aqui para garantir que velocity e collider estejam disponÃ­veis.
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            _speed = speed;
            _range = range;
            _baseDamage = baseDamage;
            _damageType = damageType;
            _knockbackForce = knockbackForce;
            _spawnPosition = transform.position;

            _direction = direction.normalized;
            _initialSpeed = speed;
            _speedDecayToFraction = speedDecayToFraction <= 0f ? 1f : Mathf.Clamp01(speedDecayToFraction);
            _homingRange = Mathf.Max(0f, homingRange);

            // Base do encolhimento visual: escala atual (definida pela factory/EnsureVisibleSprite) e o
            // raio do collider, para compensar a colisao quando a escala visual diminuir.
            _baseScale = transform.localScale;
            if (_collider is CircleCollider2D circle)
            {
                _hasCircleCollider = true;
                _baseCircleRadius = circle.radius;
            }

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = _direction * _speed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void InitializeWithStatus(Vector2 direction, float speed, float range, int baseDamage, DamageType damageType, float knockbackForce, CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect, float statusApplyChance, float speedDecayToFraction = 1f, float homingRange = 0f)
        {
            _statusEffect = statusEffect;
            _statusApplyChance = statusApplyChance;
            Initialize(direction, speed, range, baseDamage, damageType, knockbackForce, speedDecayToFraction, homingRange);
        }
    }
}
