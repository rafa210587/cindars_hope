using CindarsHope.Combat;
using CindarsHope.Core.Events;
using CindarsHope.Core;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Executa o movimento físico do inimigo, delegado pelo EnemyBrain.
    /// Recebe referências via Init(); não é MonoBehaviour.
    /// Contém todo o estado de movimento e os métodos Move* / Try* extraídos do EnemyBrain.
    /// </summary>
    internal sealed class EnemyMovementExecutor
    {
        // ─── Referências injetadas ────────────────────────────────────────────

        private Rigidbody2D _rb;
        private Transform _transform;
        private EnemyDataSO _enemyData;
        private EnemyMovementProfileSO _movementProfile;
        private EnemyTelegraphController _telegraph;

        // Tuning SerializeFields do EnemyBrain (não podem sair do MonoBehaviour)
        private float _leapCooldownSeconds = 3.5f;
        private float _leapSpeedMultiplier = 3.2f;
        private float _blinkCooldownSeconds = 5f;
        private float _burrowSpeedMultiplier = 1.7f;

        // Callbacks para acessar estado do EnemyBrain
        private System.Func<GameObject> _getPlayerTarget;
        private System.Func<float> _getMoveSpeed;
        private System.Func<float> _getDistanceToPlayer;
        private System.Func<Vector2> _getDirectionToPlayer;
        private System.Action<bool> _setSubmergedVisual;
        private System.Func<EnemyPackCoordinator> _getPackCoordinator;
        private System.Func<string> _getPackId;
        private System.Func<bool> _hasActiveThreat;
        private System.Func<Vector2> _getLastKnownPosition;

        // ─── Estado de movimento (antes em EnemyBrain) ───────────────────────

        /// <summary>Timer de alternância de direção em patrulha e erratic swarm.</summary>
        internal float PatrolDirectionTimer;
        /// <summary>Ancora de spawn — ponto de referência para patrulha e guard.</summary>
        internal Vector2 SpawnAnchor;
        /// <summary>Timestamp de fim do leap atual.</summary>
        internal float LeapEndTime;
        /// <summary>True enquanto o inimigo está em flight de leap.</summary>
        internal bool IsLeaping;
        /// <summary>Timestamp de cooldown do próximo leap.</summary>
        internal float NextLeapTime;
        /// <summary>Timestamp de cooldown do próximo blink de movimento.</summary>
        internal float NextBlinkTime;
        /// <summary>Lado de flanco do blink (alternado por instância, sem UnityEngine.Random).</summary>
        internal float BlinkFlankSide;
        /// <summary>fable_24: sinal de rotação de órbita.</summary>
        internal float OrbitSign = 1f;
        /// <summary>fable_24: telegraph de charge ativo.</summary>
        internal bool ChargeTelegraphActive;
        /// <summary>fable_24: timestamp de início do telegraph de charge.</summary>
        internal float ChargeTelegraphStart;
        /// <summary>fable_24: direção travada de charge (sem homing).</summary>
        internal Vector2 ChargeLockedDirection;
        /// <summary>fable_24: true enquanto o charge está em flight.</summary>
        internal bool IsCharging;
        /// <summary>fable_24: timestamp de fim do charge.</summary>
        internal float ChargeEndTime;
        /// <summary>fable_24: timestamp de cooldown do próximo charge.</summary>
        internal float NextChargeTime;
        /// <summary>fable_24: mimic ficou disfarçado até ativar.</summary>
        internal bool MimicActivated;
        /// <summary>fable_24: timestamp em que o flanker viu o líder pela última vez.</summary>
        internal float FlankerNoLeaderSince = -1f;
        /// <summary>fable_24: throttle de call-for-help.</summary>
        internal float NextCallForHelpTime;
        /// <summary>fable_24: âncora para ProtectAnchor / BossArenaControl.</summary>
        internal Vector2 AnchorPoint;
        /// <summary>fable_24: raio de leash da âncora.</summary>
        internal float AnchorLeashTiles = EnemyMoveLogic.DefaultAnchorLeashTiles;
        /// <summary>fable_83: evitando obstáculo via whisker-raycast.</summary>
        internal bool IsAvoidingObstacle;
        /// <summary>fable_82: sidestep reativo em curso.</summary>
        internal bool IsEvasiveSidestepping;
        /// <summary>fable_82: timestamp de fim do sidestep.</summary>
        internal float EvadeEndTime;
        /// <summary>fable_82: velocidade do sidestep em curso.</summary>
        internal Vector2 EvadeVelocity;
        /// <summary>fable_82: dash de reposicionamento em curso.</summary>
        internal bool IsRepositionDashing;
        /// <summary>fable_82: timestamp de fim do dash.</summary>
        internal float DashEndTime;
        /// <summary>fable_82: velocidade do dash em curso.</summary>
        internal Vector2 DashVelocity;
        /// <summary>fable_82: flanco alternado de evasão (sem UnityEngine.Random).</summary>
        internal float EvadeFlankSign = 1f;
        /// <summary>fable_82: windup do player detectado na última janela de decisão.</summary>
        internal bool PlayerWindupDetected;
        /// <summary>fable_82: timestamp até quando o windup é válido.</summary>
        internal float PlayerWindupClearTime;
        /// <summary>fable_82: cooldown de evasão.</summary>
        internal float NextEvadeTime;
        /// <summary>fable_82: cooldown de dash de reposicionamento.</summary>
        internal float NextDashTime;
        /// <summary>RNG seeded por hash de posição de spawn (determinístico; sem GUID/timestamp).</summary>
        internal System.Random EvasionRng;
        /// <summary>LayerMask de obstáculos para pathing leve (injeta do SerializeField do EnemyBrain).</summary>
        internal LayerMask ObstacleLayerMask;

        // ─── Init ────────────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa o executor com as referências necessárias do EnemyBrain.
        /// Deve ser chamado em Awake (após GetComponent) e re-chamado em ConfigureRuntime.
        /// </summary>
        internal void Init(
            Rigidbody2D rb,
            Transform transform,
            EnemyDataSO enemyData,
            EnemyMovementProfileSO movementProfile,
            EnemyTelegraphController telegraph,
            System.Func<GameObject> getPlayerTarget,
            System.Func<float> getMoveSpeed,
            System.Func<float> getDistanceToPlayer,
            System.Func<Vector2> getDirectionToPlayer,
            System.Action<bool> setSubmergedVisual,
            System.Func<EnemyPackCoordinator> getPackCoordinator,
            System.Func<string> getPackId,
            System.Func<bool> hasActiveThreat = null,
            System.Func<Vector2> getLastKnownPosition = null,
            float leapCooldownSeconds = 3.5f,
            float leapSpeedMultiplier = 3.2f,
            float blinkCooldownSeconds = 5f,
            float burrowSpeedMultiplier = 1.7f)
        {
            _rb = rb;
            _transform = transform;
            _enemyData = enemyData;
            _movementProfile = movementProfile;
            _telegraph = telegraph;
            _getPlayerTarget = getPlayerTarget;
            _getMoveSpeed = getMoveSpeed;
            _getDistanceToPlayer = getDistanceToPlayer;
            _getDirectionToPlayer = getDirectionToPlayer;
            _setSubmergedVisual = setSubmergedVisual;
            _getPackCoordinator = getPackCoordinator;
            _getPackId = getPackId;
            _hasActiveThreat = hasActiveThreat;
            _getLastKnownPosition = getLastKnownPosition;
            _leapCooldownSeconds = leapCooldownSeconds;
            _leapSpeedMultiplier = leapSpeedMultiplier;
            _blinkCooldownSeconds = blinkCooldownSeconds;
            _burrowSpeedMultiplier = burrowSpeedMultiplier;
        }

        /// <summary>Atualiza referências de dados após ConfigureRuntime do EnemyBrain.</summary>
        internal void UpdateRefs(EnemyDataSO enemyData, EnemyMovementProfileSO movementProfile, EnemyTelegraphController telegraph)
        {
            _enemyData = enemyData;
            _movementProfile = movementProfile;
            _telegraph = telegraph;
        }

        /// <summary>
        /// Atualiza os valores de tuning SerializeField do EnemyBrain (mantidos no MonoBehaviour).
        /// Chamado quando EnemyBrain.ConfigureRuntime ou OnEnable for invocado.
        /// </summary>
        internal void UpdateTuning(float leapCooldown, float leapSpeed, float blinkCooldown, float burrowSpeed)
        {
            _leapCooldownSeconds = leapCooldown;
            _leapSpeedMultiplier = leapSpeed;
            _blinkCooldownSeconds = blinkCooldown;
            _burrowSpeedMultiplier = burrowSpeed;
        }

        // ─── ExecuteMovement (ponto de entrada principal) ────────────────────

        /// <summary>
        /// Ponto de entrada chamado pelo EnemyBrain.Update a cada frame.
        /// Coordena inércia de evasão, charge, leap e despacha para o Move* correto.
        /// Retorna a velocidade linear resultante já aplicada ao Rigidbody2D.
        /// </summary>
        internal void ExecuteMovement(EnemyBrainState currentState, EnemyMovementType effectiveMove)
        {
            if (_rb == null) return;

            // fable_82: limpar flag de windup ao expirar a janela.
            if (PlayerWindupDetected && Time.time >= PlayerWindupClearTime)
                PlayerWindupDetected = false;

            // fable_82: sidestep reativo em curso — manter velocidade do hop até o fim.
            if (IsEvasiveSidestepping)
            {
                if (Time.time < EvadeEndTime)
                {
                    _rb.linearVelocity = EvadeVelocity;
                    return;
                }
                IsEvasiveSidestepping = false;
            }

            // fable_82: dash de reposicionamento em curso.
            if (IsRepositionDashing)
            {
                if (Time.time < DashEndTime)
                {
                    _rb.linearVelocity = DashVelocity;
                    return;
                }
                IsRepositionDashing = false;
            }

            if (IsLeaping)
            {
                if (Time.time >= LeapEndTime)
                    IsLeaping = false;
                else
                    return; // leap velocity is in flight; do not override it
            }

            // fable_24: ChargeLine investida overrides normal movement while in flight (straight line).
            if (IsCharging)
            {
                if (Time.time >= ChargeEndTime)
                {
                    IsCharging = false;
                }
                else
                {
                    _rb.linearVelocity = EnemyMoveLogic.ResolveChargeVelocity(ChargeLockedDirection, _getMoveSpeed() * 3.2f);
                    return;
                }
            }

            // fable_82: tentar evasão reativa e dash quando em estados engajados.
            if (currentState == EnemyBrainState.Chase ||
                currentState == EnemyBrainState.Kite ||
                currentState == EnemyBrainState.Alert)
            {
                if (TryReactiveSidestep()) return;
                if (TryRepositionDash()) return;
            }

            switch (currentState)
            {
                case EnemyBrainState.Patrol:
                    MovePatrol();
                    break;
                case EnemyBrainState.Chase:
                    MoveChase(effectiveMove);
                    break;
                case EnemyBrainState.Kite:
                    MoveKite(effectiveMove);
                    break;
                case EnemyBrainState.Retreat:
                    MoveRetreat();
                    break;
                case EnemyBrainState.Burrow:
                    MoveBurrow();
                    break;
                case EnemyBrainState.AttackWindup:
                case EnemyBrainState.AttackRecover:
                    StopMovement();
                    break;
                case EnemyBrainState.GuardHold:
                    MoveGuardHold();
                    break;
            }
        }

        // ─── Move* ───────────────────────────────────────────────────────────

        /// <summary>Chase: despacha para TryMoveNewBehaviour ou move padrão com avoidance.</summary>
        private void MoveChase(EnemyMovementType effectiveMove)
        {
            // fable_24: dispatch the new moves first; they fully own the chase velocity for their tick.
            if (TryMoveNewBehaviour(effectiveMove))
            {
                return;
            }

            var moveType = _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;
            float speed = _getMoveSpeed();

            // fable_04: if the player slipped out of detection range but threat memory is still
            // valid, pursue the last known position instead of stopping. Special movement (leap,
            // blink, swarm jitter) only triggers when the player is actually visible/in range.
            var playerTarget = _getPlayerTarget();
            bool playerVisible = playerTarget != null && _getDistanceToPlayer() <= (_movementProfile?.DetectionRange ?? _enemyData?.detectionRadius ?? 10f);
            if (!playerVisible)
            {
                bool activeThreat = _hasActiveThreat != null && _hasActiveThreat();
                if (activeThreat && _getLastKnownPosition != null)
                {
                    MoveTowardLastKnownPosition(speed, _getLastKnownPosition());
                }
                else if (playerTarget == null)
                {
                    StopMovement();
                }
                else
                {
                    _rb.linearVelocity = _getDirectionToPlayer() * speed;
                }
                return;
            }

            switch (moveType)
            {
                case EnemyMovementType.SwarmErratic:
                    MoveSwarmErratic(speed);
                    return;
                case EnemyMovementType.TankSlowPush:
                    speed = Mathf.Min(speed, 1.5f);
                    break;
                case EnemyMovementType.Leaper:
                    if (TryLeap(speed))
                        return;
                    break;
                case EnemyMovementType.PhaseShortBlink:
                    if (TryBlink())
                        return;
                    break;
                default:
                    // fable_82: gap-closer leap para roles não-Leaper que tenham GapCloserLeap=true.
                    if (_movementProfile != null && _movementProfile.GapCloserLeap && TryGapCloserLeap(speed))
                        return;
                    break;
            }

            // fable_83: pathing leve — desvio local de obstáculo via whiskers.
            // Só roda se a layer de obstáculo foi configurada no prefab (layer 0 = skip).
            if (ObstacleLayerMask.value != 0)
            {
                var avoidance = EnemyLocalAvoidance.SteerWithRaycast(
                    _transform.position,
                    _getDirectionToPlayer(),
                    ObstacleLayerMask,
                    IsAvoidingObstacle);
                IsAvoidingObstacle = avoidance.IsAvoiding;
                _rb.linearVelocity = avoidance.Direction * (speed * (avoidance.IsAvoiding ? EnemyLocalAvoidance.AvoidanceSpeedMultiplier : 1f));
            }
            else
            {
                IsAvoidingObstacle = false;
                _rb.linearVelocity = _getDirectionToPlayer() * speed;
            }
        }

        // ─── fable_24: the 12 new moves ──────────────────────────────────────

        /// <summary>
        /// Tenta despachar para um dos 12 novos moves de fable_24.
        /// Retorna true quando um move assumiu a velocidade deste tick.
        /// </summary>
        private bool TryMoveNewBehaviour(EnemyMovementType effectiveMove)
        {
            float speed = _getMoveSpeed();
            switch (effectiveMove)
            {
                case EnemyMovementType.CircleStrafe:
                case EnemyMovementType.FloatingOrbit:
                    MoveOrbit(speed);
                    return true;

                case EnemyMovementType.FloatingSlow:
                    MoveFloatingSlow(speed);
                    return true;

                case EnemyMovementType.ChargeLine:
                    MoveChargeLine(speed);
                    return true;

                case EnemyMovementType.RetreatAndCall:
                    MoveRetreatAndCall(speed);
                    return true;

                case EnemyMovementType.HazardLure:
                    // Lure the player by backing away (toward a hazard the level designer placed);
                    // straight retreat reuses the existing retreat vector (no pathfinding).
                    MoveRetreat();
                    return true;

                case EnemyMovementType.TreasureIdleAmbush:
                    MoveMimicAmbush(speed);
                    return true;

                case EnemyMovementType.ProtectAnchor:
                case EnemyMovementType.BossArenaControl:
                    MoveAnchoredChase(speed);
                    return true;

                case EnemyMovementType.PackFlanker:
                    MovePackFlanker(speed);
                    return true;

                case EnemyMovementType.PackLeader:
                    // Leader chases normally; its death (handled via pack alert) turns flankers to
                    // RetreatAndCall. No special steering here — fall through to default chase.
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>CircleStrafe / FloatingOrbit: hold firing distance and orbit the player.</summary>
        private void MoveOrbit(float speed)
        {
            if (_getPlayerTarget() == null)
            {
                StopMovement();
                return;
            }

            float preferred = Mathf.Max(_movementProfile?.PreferredDistance ?? 4f, 1f);
            float dist = _getDistanceToPlayer();
            Vector2 toPlayer = _getDirectionToPlayer();
            Vector2 tangent = Vector2.Perpendicular(toPlayer) * OrbitSign;

            // Blend a radial correction (keep ~preferred distance) with the tangential orbit.
            float radialError = dist - preferred;
            Vector2 radial = toPlayer * Mathf.Clamp(radialError, -1f, 1f);
            _rb.linearVelocity = (tangent + radial).normalized * speed;
        }

        /// <summary>FloatingSlow: hover toward the player at reduced speed, ignoring floor obstacles (no burrow).</summary>
        private void MoveFloatingSlow(float speed)
        {
            if (_getPlayerTarget() == null)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = _getDirectionToPlayer() * (speed * 0.6f);
        }

        /// <summary>ChargeLine: telegraph a straight line, lock the aim, then charge straight (dodgeable).</summary>
        private void MoveChargeLine(float speed)
        {
            if (_getPlayerTarget() == null)
            {
                StopMovement();
                return;
            }

            bool offCooldown = Time.time >= NextChargeTime;

            if (!ChargeTelegraphActive && offCooldown && !IsCharging)
            {
                // Begin telegraph: lock direction now so the player can sidestep the committed line.
                ChargeTelegraphActive = true;
                ChargeTelegraphStart = Time.time;
                ChargeLockedDirection = _getDirectionToPlayer();
                _telegraph?.StartTelegraph(Color.red);
                GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, _transform.position));
                StopMovement();
                return;
            }

            if (ChargeTelegraphActive)
            {
                float elapsed = Time.time - ChargeTelegraphStart;
                float telegraphDuration = EnemyMoveLogic.CommonWindupSeconds;
                StopMovement(); // hold still during the windup
                if (EnemyMoveLogic.ShouldChargeLineFire(true, elapsed, telegraphDuration, offCooldown))
                {
                    ChargeTelegraphActive = false;
                    IsCharging = true;
                    ChargeEndTime = Time.time + 0.4f;
                    NextChargeTime = Time.time + 3f;
                    _telegraph?.EndTelegraph();
                }
                return;
            }

            // Between charges: close in at normal speed.
            _rb.linearVelocity = _getDirectionToPlayer() * speed;
        }

        /// <summary>RetreatAndCall: flee and periodically emit EnemyCallForHelpEvent so allies regroup.</summary>
        private void MoveRetreatAndCall(float speed)
        {
            if (_getPlayerTarget() == null)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = -_getDirectionToPlayer() * (speed * 1.15f);
            EmitCallForHelp();
        }

        /// <summary>TreasureIdleAmbush (mimic): stay disguised and immobile until the player is &lt; 2 tiles.</summary>
        private void MoveMimicAmbush(float speed)
        {
            if (!MimicActivated)
            {
                if (EnemyMoveLogic.ShouldMimicActivate(_getDistanceToPlayer()))
                {
                    MimicActivated = true;
                    GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, _transform.position));
                }
                else
                {
                    StopMovement();
                    return;
                }
            }

            // Once sprung, behave like a chaser.
            if (_getPlayerTarget() != null)
            {
                _rb.linearVelocity = _getDirectionToPlayer() * speed;
            }
        }

        /// <summary>ProtectAnchor / BossArenaControl: chase, but never leave the leash radius of the anchor.</summary>
        private void MoveAnchoredChase(float speed)
        {
            if (_getPlayerTarget() == null)
            {
                ReturnToAnchor(speed);
                return;
            }

            Vector2 desired = (Vector2)_transform.position + _getDirectionToPlayer();
            Vector2 clamped = EnemyMoveLogic.ClampToAnchor(desired, AnchorPoint, AnchorLeashTiles);

            if (EnemyMoveLogic.IsBeyondAnchorLeash((Vector2)_transform.position, AnchorPoint, AnchorLeashTiles))
            {
                ReturnToAnchor(speed);
                return;
            }

            Vector2 step = clamped - (Vector2)_transform.position;
            _rb.linearVelocity = step.sqrMagnitude > 0.0001f ? step.normalized * speed : Vector2.zero;
        }

        private void ReturnToAnchor(float speed)
        {
            Vector2 toAnchor = AnchorPoint - (Vector2)_transform.position;
            _rb.linearVelocity = toAnchor.sqrMagnitude > 0.09f ? toAnchor.normalized * speed : Vector2.zero;
        }

        /// <summary>
        /// PackFlanker: only engage while a living leader is in range; otherwise hold, then after a
        /// timeout fall back to a plain chase (no deadlock). Leader presence comes from the pack
        /// coordinator (no scene search).
        /// </summary>
        private void MovePackFlanker(float speed)
        {
            bool leaderAlive = PackLeaderInRange(out float distToLeader, out float awareness);
            if (EnemyMoveLogic.ShouldFlankerEngage(leaderAlive, distToLeader, awareness))
            {
                FlankerNoLeaderSince = -1f;
                if (_getPlayerTarget() != null)
                {
                    // Flank: approach offset to the side of the player instead of head-on.
                    Vector2 toPlayer = _getDirectionToPlayer();
                    Vector2 flank = Vector2.Perpendicular(toPlayer) * OrbitSign;
                    _rb.linearVelocity = (toPlayer + flank * 0.5f).normalized * speed;
                }
                return;
            }

            // No leader in range — start/continue the fallback timer.
            if (FlankerNoLeaderSince < 0f)
            {
                FlankerNoLeaderSince = Time.time;
            }

            if (EnemyMoveLogic.ShouldFlankerFallbackToChase(Time.time - FlankerNoLeaderSince) && _getPlayerTarget() != null)
            {
                _rb.linearVelocity = _getDirectionToPlayer() * speed; // act as GroundChase
            }
            else
            {
                StopMovement(); // hold, waiting for a leader
            }
        }

        /// <summary>
        /// Pack leader lookup via the coordinator anchor as a stand-in for leader position (no scene
        /// search). A pack with at least one living member is treated as having a leader in range when
        /// the anchor is within awareness radius. Solo enemies (no pack) report no leader.
        /// </summary>
        private bool PackLeaderInRange(out float distanceToLeader, out float awarenessRadius)
        {
            awarenessRadius = Mathf.Max(_movementProfile?.DetectionRange ?? 10f, 1f);
            distanceToLeader = float.MaxValue;

            var packCoordinator = _getPackCoordinator();
            var packId = _getPackId();
            if (packCoordinator == null || string.IsNullOrWhiteSpace(packId))
            {
                return false;
            }

            // A living pack still has members registered; use the deterministic anchor as the leader
            // reference point (centroid of the pack's spawn — stable run / ADR-0005).
            if (packCoordinator.MemberCount(packId) <= 1)
            {
                return false;
            }

            Vector2 anchor = packCoordinator.GetAnchor(packId);
            distanceToLeader = Vector2.Distance(_transform.position, anchor);
            return true;
        }

        /// <summary>Emite EnemyCallForHelpEvent com throttle de 2 segundos.</summary>
        internal void EmitCallForHelp()
        {
            if (Time.time < NextCallForHelpTime)
            {
                return;
            }

            NextCallForHelpTime = Time.time + 2f;
            float radius = Mathf.Max(_movementProfile?.DetectionRange ?? 8f, 4f);
            GameEventBus.Publish(new EnemyCallForHelpEvent(_enemyData?.enemyId, _transform.position, radius));

            // If part of a pack, also wake siblings directly (same path as fable_04 pack alert).
            var packCoordinator = _getPackCoordinator();
            var packId = _getPackId();
            if (packCoordinator != null && !string.IsNullOrWhiteSpace(packId))
            {
                packCoordinator.Alert(packId, _transform.position);
            }
        }

        /// <summary>
        /// fable_04: walk toward the remembered position; once reached, drop velocity so the next
        /// decision tick can resolve back to Patrol when the memory finally expires (legible reset).
        /// </summary>
        internal void MoveTowardLastKnownPosition(float speed, Vector2 lastKnownPosition)
        {
            Vector2 toTarget = lastKnownPosition - (Vector2)_transform.position;
            if (toTarget.sqrMagnitude <= 0.09f)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = toTarget.normalized * speed;
        }

        /// <summary>Leapers lunge in a fast burst when the player is in the mid-range band.</summary>
        private bool TryLeap(float baseSpeed)
        {
            if (Time.time < NextLeapTime)
                return false;

            float dist = _getDistanceToPlayer();
            float attackRange = _movementProfile?.AttackRange ?? 1.5f;
            if (dist < attackRange || dist > attackRange * 3.5f)
                return false;

            IsLeaping = true;
            LeapEndTime = Time.time + 0.35f;
            NextLeapTime = Time.time + _leapCooldownSeconds;
            _rb.linearVelocity = _getDirectionToPlayer() * baseSpeed * _leapSpeedMultiplier;
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, _transform.position));
            return true;
        }

        /// <summary>Phase enemies blink to the player's flank instead of walking the gap.</summary>
        private bool TryBlink()
        {
            if (Time.time < NextBlinkTime || _getPlayerTarget() == null)
                return false;

            float dist = _getDistanceToPlayer();
            float preferred = Mathf.Max(_movementProfile?.PreferredDistance ?? 1f, 0.9f);
            if (dist < preferred * 2.5f)
                return false;

            NextBlinkTime = Time.time + _blinkCooldownSeconds;
            Vector2 toPlayer = _getDirectionToPlayer();
            Vector2 flank = Vector2.Perpendicular(toPlayer) * BlinkFlankSide;
            Vector2 destination = (Vector2)_getPlayerTarget().transform.position - toPlayer * preferred + flank * 0.5f;
            _rb.position = destination;
            _rb.linearVelocity = Vector2.zero;
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, destination));
            return true;
        }

        // ─── fable_82: Evasão Reativa ─────────────────────────────────────────

        /// <summary>
        /// fable_82 — Sidestep reativo ao windup do player (CA-1).
        /// Executa um hop lateral curto e legível quando as condições de evasão são atendidas.
        /// </summary>
        internal bool TryReactiveSidestep()
        {
            if (_movementProfile == null) return false;
            if (IsEvasiveSidestepping) return false;

            var role = _enemyData?.PrimaryRole ?? EnemyRole.Chaser;
            bool cooldownReady = Time.time >= NextEvadeTime;
            float chanceRoll = (float)EvasionRng.NextDouble();

            bool should = EnemyEvasionDecision.ShouldEvade(
                PlayerWindupDetected,
                _getDistanceToPlayer(),
                _movementProfile.EvadeReactionRadius,
                cooldownReady,
                role,
                chanceRoll,
                _movementProfile.EvadeChance,
                _movementProfile.CanReactiveEvade);

            if (!should) return false;

            // Iniciar hop lateral: telegraph via cor de blink padrão (curto; legível).
            _telegraph?.StartTelegraph(new Color(0.3f, 0.8f, 1f));
            Vector2 toPlayer = _getDirectionToPlayer();
            Vector2 sideDir = EnemyEvasionDecision.SidestepDirection(toPlayer, EvadeFlankSign);
            EvadeFlankSign = -EvadeFlankSign; // alterna flanco na próxima evasão

            float speed = _movementProfile.EvadeSidestepDistance / Mathf.Max(_movementProfile.EvadeSidestepDuration, 0.05f);
            EvadeVelocity = sideDir * speed;
            IsEvasiveSidestepping = true;
            EvadeEndTime = Time.time + _movementProfile.EvadeSidestepDuration;
            NextEvadeTime = Time.time + _movementProfile.EvadeCooldown;
            PlayerWindupDetected = false; // consome o windup

            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, _transform.position));
            CindarsHope.Combat.CombatLog.Log($"CombatLog: EnemyReactiveSidestep. EnemyId={_enemyData?.enemyId}, FlankSign={EvadeFlankSign:F0}, Dist={_getDistanceToPlayer():F2}");
            return true;
        }

        /// <summary>
        /// fable_82 — Dash de reposicionamento (CA-2): ranged/caster recuam para a distância preferida.
        /// </summary>
        internal bool TryRepositionDash()
        {
            if (_movementProfile == null) return false;
            if (IsRepositionDashing) return false;
            if (!_movementProfile.RepositionDashEnabled) return false;

            var role = _enemyData?.PrimaryRole ?? EnemyRole.Chaser;
            bool cooldownReady = Time.time >= NextDashTime;
            float preferred = Mathf.Max(_movementProfile.PreferredDistance, 0.5f);

            bool should = EnemyEvasionDecision.ShouldRepositionDash(
                _movementProfile.RepositionDashEnabled,
                cooldownReady,
                role,
                _getDistanceToPlayer(),
                preferred);

            if (!should) return false;

            // Telegraph mínimo (cor azul-esverdeada) antes do dash.
            _telegraph?.StartTelegraph(new Color(0.2f, 0.9f, 0.5f));

            Vector2 toPlayer = _getDirectionToPlayer();
            Vector2 dashDir = EnemyEvasionDecision.RepositionDashDirection(toPlayer, EvadeFlankSign);

            float speed = _movementProfile.DashDistance / Mathf.Max(_movementProfile.DashDuration, 0.05f);
            DashVelocity = dashDir * speed;
            IsRepositionDashing = true;
            DashEndTime = Time.time + _movementProfile.DashDuration;
            NextDashTime = Time.time + _movementProfile.DashCooldown;

            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, _transform.position));
            CindarsHope.Combat.CombatLog.Log($"CombatLog: EnemyRepositionDash. EnemyId={_enemyData?.enemyId}, Role={role}, Dist={_getDistanceToPlayer():F2}");
            return true;
        }

        /// <summary>
        /// fable_82 — Leap como gap-closer agressivo (CA-3): reutiliza TryLeap existente.
        /// Chamado quando GapCloserLeap=true E inimigo está muito longe do player.
        /// Não duplica lógica — usa o Leap já implementado.
        /// </summary>
        internal bool TryGapCloserLeap(float baseSpeed)
        {
            if (_movementProfile == null || !_movementProfile.GapCloserLeap) return false;
            return TryLeap(baseSpeed);
        }

        /// <summary>Burrowers movem-se em direção ao player submersos.</summary>
        private void MoveBurrow()
        {
            if (_getPlayerTarget() == null) return;
            _rb.linearVelocity = _getDirectionToPlayer() * (_getMoveSpeed() * _burrowSpeedMultiplier);
        }

        /// <summary>Retreat: recua na direção oposta ao player.</summary>
        internal void MoveRetreat()
        {
            if (_getPlayerTarget() == null)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = -_getDirectionToPlayer() * (_getMoveSpeed() * 1.25f);
        }

        /// <summary>Guards hold their post: drift back to the spawn anchor when displaced.</summary>
        private void MoveGuardHold()
        {
            Vector2 toAnchor = SpawnAnchor - (Vector2)_transform.position;
            if (toAnchor.sqrMagnitude > 0.36f)
                _rb.linearVelocity = toAnchor.normalized * (_getMoveSpeed() * 0.6f);
            else
                StopMovement();
        }

        /// <summary>Kite: mantém distância preferida; delega para TryMoveNewBehaviour se aplicável.</summary>
        private void MoveKite(EnemyMovementType effectiveMove)
        {
            // fable_24: orbit/strafe and other new moves own their steering even in the Kite state.
            if (TryMoveNewBehaviour(effectiveMove))
            {
                return;
            }

            if (_getPlayerTarget() == null) return;

            float speed = _getMoveSpeed();
            float preferred = _movementProfile?.PreferredDistance ?? 5f;
            float dist = _getDistanceToPlayer();
            Vector2 toPlayer = _getDirectionToPlayer();

            if (dist < preferred * 0.8f)
                _rb.linearVelocity = -toPlayer * speed;
            else if (dist > preferred * 1.2f)
                _rb.linearVelocity = toPlayer * (speed * 0.5f);
            else
                StopMovement();
        }

        /// <summary>Patrol: movimenta-se aleatoriamente ancorado ao spawn.</summary>
        private void MovePatrol()
        {
            PatrolDirectionTimer -= Time.deltaTime;
            if (PatrolDirectionTimer > 0f) return;

            PatrolDirectionTimer = Random.Range(1.5f, 3.5f);

            // Anchor patrol to the spawn point so idle enemies stay in their room
            // instead of drifting across the level over time.
            float wanderRadius = Mathf.Max(_movementProfile?.WanderRadius ?? 5f, 1f);
            Vector2 fromAnchor = (Vector2)_transform.position - SpawnAnchor;
            Vector2 direction;
            if (fromAnchor.sqrMagnitude > wanderRadius * wanderRadius)
            {
                direction = (-fromAnchor).normalized;
            }
            else
            {
                direction = Random.insideUnitCircle.normalized;
            }

            _rb.linearVelocity = direction * (_getMoveSpeed() * 0.4f);
        }

        private void MoveSwarmErratic(float speed)
        {
            PatrolDirectionTimer -= Time.deltaTime;
            if (PatrolDirectionTimer > 0f) return;

            PatrolDirectionTimer = Random.Range(0.15f, 0.5f);
            Vector2 toPlayer = _getDirectionToPlayer();
            Vector2 erratic = (toPlayer * 0.6f + (Vector2)Random.insideUnitCircle * 0.8f).normalized;
            _rb.linearVelocity = erratic * speed;
        }

        /// <summary>Para o movimento imediatamente (velocidade zero).</summary>
        internal void StopMovement()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>Ajusta a transparência do sprite para o estado submerso (Burrow).</summary>
        internal void SetSubmergedVisual(bool submerged, SpriteRenderer spriteRenderer, float spriteBaseAlpha)
        {
            if (spriteRenderer == null) return;
            var color = spriteRenderer.color;
            color.a = submerged ? spriteBaseAlpha * 0.25f : spriteBaseAlpha;
            spriteRenderer.color = color;
        }
    }
}
