using UnityEngine;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Player
{
    /// <summary>
    /// Anima o SpriteRenderer do player: caminhada + idle + ataque, em 8 direcoes.
    /// Carrega frames via Resources.LoadAll em Awake (sem alocacao em Update).
    /// Depende de PlayerController no mesmo GameObject via GetComponent (permitido pelas regras).
    /// O ataque melee e disparado por PlayerMeleeSwingEvent (com arquetipo de arma) e tem prioridade sobre walk/idle;
    /// a duracao do swing vem do evento (= cooldown efetivo = attack speed do player).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerWalkAnimator : MonoBehaviour
    {
        // Indices canonicos das 8 direcoes — mapeados ao bucket de angulo em Update.
        private const int DirRight     = 0;
        private const int DirUpRight   = 1;
        private const int DirUp        = 2;
        private const int DirUpLeft    = 3;
        private const int DirLeft      = 4;
        private const int DirDownLeft  = 5;
        private const int DirDown      = 6;
        private const int DirDownRight = 7;
        private const int DirCount     = 8;

        // Chaves de pasta na mesma ordem dos indices acima.
        private static readonly string[] DirKeys = new string[DirCount]
        {
            "right", "upright", "up", "upleft", "left", "downleft", "down", "downright"
        };

        // Nomes de pasta por arquetipo, indexados pelo valor int do enum PlayerAttackAnimArchetype.
        // Array estatico evita depender de enum.ToString() (§26 da spec — risco de divergencia).
        // Ordem: Sword=0, Bow=1, Heavy=2, Thrust=3, Dagger=4, Cast=5.
        private static readonly string[] ArchetypeFolders = new string[]
        {
            "attack_sword",   // 0 = Sword
            "attack_bow",     // 1 = Bow
            "attack_heavy",   // 2 = Heavy
            "attack_thrust",  // 3 = Thrust
            "attack_dagger",  // 4 = Dagger
            "attack_cast",    // 5 = Cast
        };

        // Idle: pasta de idle por direcao. Diagonais ainda nao tem idle dedicado,
        // entao caem na lateral (right/left) para manter o facing horizontal continuo.
        // Trocar aqui se quiser diagonal->front/back, ou quando houver idle diagonal proprio.
        private static readonly string[] IdleDirKeys = new string[DirCount]
        {
            "right", "right", "up", "left", "left", "left", "down", "right"
        };

        [SerializeField] private float _framesPerSecond = 10f;
        [SerializeField] private float _idleFramesPerSecond = 2.0f; // ~500ms/frame (idle lento)
        [SerializeField] private float _moveThreshold = 0.1f;

        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;
        private Rigidbody2D _rigidbody;
        private Vector2 _lastPhysicalFacing;
        private bool _usingPhysicalMotion;
        private Vector2 _previousPhysicalPosition;
        private Vector2 _physicalVelocity;
        private bool _hasPhysicalSample;

        // Frames cacheados por direcao — carregados uma vez em Awake.
        private readonly Sprite[][] _frames = new Sprite[DirCount][];
        private readonly Sprite[][] _idleFrames = new Sprite[DirCount][];
        // Frames de ataque por arquetipo × direcao. Dimensoes: [arquetipo][direcao].
        // Indexado pelo valor int de PlayerAttackAnimArchetype (Sword=0 .. Cast=5).
        private readonly Sprite[][][] _attackFramesByArchetype = new Sprite[6][][];
        private readonly Sprite[][] _bowFrames = new Sprite[DirCount][];    // arco

        private float _timer;
        private float _idleTimer;
        private int _currentDir = DirDown;
        private Sprite[] _currentFrames;
        private bool _missingController;

        // Estado de one-shot (ataque melee por arquetipo OU tiro de arco): prioridade sobre walk/idle.
        // A sequencia da direcao e capturada no disparo; toca uma vez ao longo de _attackDuration.
        private bool _attacking;
        private Sprite[] _oneShotFrames;
        private float _attackTimer;
        private float _attackDuration;
        private bool _rootMovement; // trava o movimento durante este one-shot (ex.: tiro de arco)

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerController = GetComponent<PlayerController>();
            _rigidbody = GetComponent<Rigidbody2D>();

            if (_playerController == null)
            {
                Debug.LogError($"[PlayerWalkAnimator] PlayerController nao encontrado em '{name}'. " +
                               "Adicione PlayerController ao mesmo GameObject. Componente desabilitado.");
                _missingController = true;
                enabled = false;
                return;
            }

            LoadAllFrames();

            _currentDir = DirDown;
            _currentFrames = _frames[_currentDir];
        }

        private void LoadAllFrames()
        {
            for (int i = 0; i < DirCount; i++)
            {
                _frames[i]     = LoadFolderSorted("PlayerSprites/walk/" + DirKeys[i], required: true);
                // Idle: opcional. Diagonais reusam a lateral via IdleDirKeys. Ausente => walk[0] estatico.
                _idleFrames[i] = LoadFolderSorted("PlayerSprites/idle/" + IdleDirKeys[i], required: false);
                // Arco: opcional. As 8 direcoes existem (diagonais por espelho); ausente => sem anim.
                _bowFrames[i]  = LoadFolderSorted("PlayerSprites/bow/" + DirKeys[i], required: false);
            }

            // Carrega frames de ataque por arquetipo usando pastas attack_{arquetipo}/{dir}/.
            // ArchetypeFolders e indexado pelo valor int do enum (Sword=0 .. Cast=5).
            for (int a = 0; a < ArchetypeFolders.Length; a++)
            {
                _attackFramesByArchetype[a] = new Sprite[DirCount][];
                for (int i = 0; i < DirCount; i++)
                {
                    _attackFramesByArchetype[a][i] = LoadFolderSorted(
                        "PlayerSprites/" + ArchetypeFolders[a] + "/" + DirKeys[i],
                        required: false);
                }
            }
        }

        private void OnEnable()
        {
            ResetPhysicalMotionSampling();
            GameEventBus.Subscribe<PlayerMeleeSwingEvent>(OnMeleeSwing);
            GameEventBus.Subscribe<PlayerBowShootEvent>(OnBowShoot);
        }

        private void OnDisable()
        {
            ResetPhysicalMotionSampling();
            GameEventBus.Unsubscribe<PlayerMeleeSwingEvent>(OnMeleeSwing);
            GameEventBus.Unsubscribe<PlayerBowShootEvent>(OnBowShoot);
            // Seguranca: nao deixar o player travado se desabilitar no meio do tiro.
            _attacking = false;
            EndOneShotRoot();
        }

        // Destrava o movimento do player ao fim de um one-shot que travava (arco).
        private void EndOneShotRoot()
        {
            if (_rootMovement && _playerController != null)
            {
                _playerController.MovementLocked = false;
            }
            _rootMovement = false;
        }

        // Melee: dispara a anim de ataque pelo arquetipo da arma.
        // Fallback para Sword se o set do arquetipo estiver vazio (arte ainda nao entregue).
        // Trava o movimento durante a animacao (attack commitment): golpear andando ficava
        // estranho com a anim de swing por arquetipo, entao o player para enquanto ataca.
        private void OnMeleeSwing(PlayerMeleeSwingEvent evt)
        {
            int archetypeIdx = (int)evt.Archetype;
            Sprite[][] set = (archetypeIdx >= 0 && archetypeIdx < _attackFramesByArchetype.Length)
                ? _attackFramesByArchetype[archetypeIdx]
                : null;

            // Fallback para Sword se o arquetipo nao tem arte carregada.
            if (set == null || IsSetEmpty(set))
                set = _attackFramesByArchetype[(int)PlayerAttackAnimArchetype.Sword];

            BeginOneShot(set, evt.Direction, evt.Duration, rootMovement: true);
        }

        private static bool IsSetEmpty(Sprite[][] set)
        {
            if (set == null) return true;
            for (int i = 0; i < set.Length; i++)
                if (set[i] != null && set[i].Length > 0) return false;
            return true;
        }

        // Arco: dispara a anim de tiro quando uma flecha e disparada. Trava o movimento
        // durante o saque/release (o player para para atirar).
        private void OnBowShoot(PlayerBowShootEvent evt)
        {
            BeginOneShot(_bowFrames, evt.Direction, evt.Duration, rootMovement: true);
        }

        // Inicia uma animacao one-shot direcional. Duracao atrela a velocidade ao cooldown
        // efetivo do ataque (= attack speed, escala com progressao). Sem arte p/ a direcao: ignora.
        // rootMovement trava o PlayerController (mesmo GameObject) enquanto o one-shot toca.
        private void BeginOneShot(Sprite[][] set, Vector2 direction, float duration, bool rootMovement)
        {
            if (_missingController) return;

            int dir = GetDirectionBucket(direction);
            Sprite[] frames = set[dir];
            if (frames == null || frames.Length == 0) return;

            _attacking = true;
            _oneShotFrames = frames;
            _attackTimer = 0f;
            _attackDuration = Mathf.Max(duration, 0.05f); // piso minimo p/ ser visivel
            _rootMovement = rootMovement;
            _playerController.MovementLocked = rootMovement; // trava melee e arco durante o one-shot
        }

        private static Sprite[] LoadFolderSorted(string folder, bool required)
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>(folder);

            if (loaded == null || loaded.Length == 0)
            {
                if (required)
                {
                    Debug.LogError($"[PlayerWalkAnimator] Nenhum sprite encontrado em Resources/{folder}. " +
                                   "Verifique se os PNGs foram importados corretamente.");
                }
                return new Sprite[0];
            }

            // Ordena por nome (ordinal) para garantir ordem frame 01, 02, ...
            System.Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
            return loaded;
        }

        private void ResetPhysicalMotionSampling()
        {
            _hasPhysicalSample = false;
            _physicalVelocity = Vector2.zero;
            _usingPhysicalMotion = false;
        }

        private void FixedUpdate()
        {
            if (_playerController == null || _playerController.enabled || _rigidbody == null)
            {
                ResetPhysicalMotionSampling();
                return;
            }

            // MovePosition may report zero linearVelocity after simulation. Sample the actual
            // body's displacement; the first sample only establishes a framing/reset baseline.
            var position = _rigidbody.position;
            _physicalVelocity = _hasPhysicalSample && Time.fixedDeltaTime > 0f
                ? (position - _previousPhysicalPosition) / Time.fixedDeltaTime
                : Vector2.zero;
            _previousPhysicalPosition = position;
            _hasPhysicalSample = true;
        }

        private void Update()
        {
            if (_missingController) return;

            // One-shot (melee/arco) tem prioridade: ignora walk/idle e toca a sequencia uma vez.
            if (_attacking)
            {
                Sprite[] oneShot = _oneShotFrames;
                if (oneShot != null && oneShot.Length > 0)
                {
                    _attackTimer += Time.deltaTime;
                    int n = oneShot.Length;
                    int idx = (int)(_attackTimer / _attackDuration * n);
                    if (idx < n)
                    {
                        _spriteRenderer.sprite = oneShot[idx];
                        return;
                    }
                }
                // Terminou (ou sem frames): destrava movimento e volta ao walk/idle neste mesmo frame.
                _attacking = false;
                EndOneShotRoot();
            }

            bool physicalMotion = !_playerController.enabled;
            if (!physicalMotion) ResetPhysicalMotionSampling();
            if (physicalMotion && !_usingPhysicalMotion)
                _lastPhysicalFacing = _playerController.LastFacingDirection;
            _usingPhysicalMotion = physicalMotion;
            Vector2 moveInput = physicalMotion
                ? _physicalVelocity
                : _playerController.MoveInput;
            bool isMoving = moveInput.sqrMagnitude > _moveThreshold * _moveThreshold;
            if (physicalMotion && isMoving) _lastPhysicalFacing = moveInput;
            Vector2 idleFacing = physicalMotion ? _lastPhysicalFacing : _playerController.LastFacingDirection;
            Vector2 dirVec = isMoving ? moveInput : idleFacing;
            int newDir = GetDirectionBucket(dirVec);

            if (newDir != _currentDir)
            {
                _currentDir = newDir;
                _currentFrames = _frames[_currentDir];
                // Nao reseta o timer ao trocar direcao enquanto anda — transicao mais suave.
            }

            if (_currentFrames == null || _currentFrames.Length == 0) return;

            if (isMoving)
            {
                _idleTimer = 0f;
                _timer += Time.deltaTime;
                int frameCount = _currentFrames.Length;
                // Sem modulo flutuante para evitar divisao: usa int cast + modulo inteiro.
                int idx = (int)(_timer * _framesPerSecond) % frameCount;
                _spriteRenderer.sprite = _currentFrames[idx];
            }
            else
            {
                _timer = 0f;
                Sprite[] idle = _idleFrames[_currentDir];
                if (idle != null && idle.Length > 0)
                {
                    _idleTimer += Time.deltaTime;
                    int idx = (int)(_idleTimer * _idleFramesPerSecond) % idle.Length;
                    _spriteRenderer.sprite = idle[idx];
                }
                else
                {
                    // Sem idle dedicado: mantem o primeiro frame de walk (facing correto, estatico).
                    _spriteRenderer.sprite = _currentFrames[0];
                }
            }
        }

        /// <summary>
        /// Mapeia um Vector2 ao bucket de 8 direcoes mais proximo.
        /// Usa Mathf.Atan2 (y, x) — em Unity 2D, y+ e para cima.
        /// Setores de 45 graus, centrados em cada direcao cardinal/diagonal.
        /// </summary>
        private static int GetDirectionBucket(Vector2 v)
        {
            if (v == Vector2.zero) return DirDown;

            // graus em [0, 360)
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;

            // Bucket de 45 graus:
            //   right     = 337.5 .. 22.5   (0 deg)
            //   upright   = 22.5  .. 67.5   (45 deg)
            //   up        = 67.5  .. 112.5  (90 deg)
            //   upleft    = 112.5 .. 157.5  (135 deg)
            //   left      = 157.5 .. 202.5  (180 deg)
            //   downleft  = 202.5 .. 247.5  (225 deg)
            //   down      = 247.5 .. 292.5  (270 deg)
            //   downright = 292.5 .. 337.5  (315 deg)
            int sector = (int)((angle + 22.5f) / 45f) % 8;

            // sector 0 = right, 1 = upright, 2 = up, 3 = upleft,
            // sector 4 = left, 5 = downleft, 6 = down, 7 = downright
            // Corresponde exatamente aos indices DirRight..DirDownRight definidos acima.
            return sector;
        }
    }
}
