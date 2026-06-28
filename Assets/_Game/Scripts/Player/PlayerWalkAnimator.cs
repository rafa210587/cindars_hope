using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// Anima o SpriteRenderer do player com sprites de caminhada de 8 direcoes.
    /// Carrega frames via Resources.LoadAll em Awake (sem alocacao em Update).
    /// Depende de PlayerController no mesmo GameObject via GetComponent (permitido pelas regras).
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

        [SerializeField] private float _framesPerSecond = 10f;
        [SerializeField] private float _moveThreshold = 0.1f;

        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;

        // Frames cacheados por direcao — carregados uma vez em Awake.
        private readonly Sprite[][] _frames = new Sprite[DirCount][];

        private float _timer;
        private int _currentDir = DirDown;
        private Sprite[] _currentFrames;
        private bool _missingController;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerController = GetComponent<PlayerController>();

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
                string folder = "PlayerSprites/walk/" + DirKeys[i];
                Sprite[] loaded = Resources.LoadAll<Sprite>(folder);

                if (loaded == null || loaded.Length == 0)
                {
                    Debug.LogError($"[PlayerWalkAnimator] Nenhum sprite encontrado em Resources/{folder}. " +
                                   "Verifique se os PNGs foram importados corretamente.");
                    _frames[i] = new Sprite[0];
                    continue;
                }

                // Ordena por nome (ordinal) para garantir ordem frame 01, 02, ...
                System.Array.Sort(loaded, (a, b) =>
                    string.CompareOrdinal(a.name, b.name));
                _frames[i] = loaded;
            }
        }

        private void Update()
        {
            if (_missingController) return;

            Vector2 moveInput = _playerController.MoveInput;
            bool isMoving = moveInput.sqrMagnitude > _moveThreshold * _moveThreshold;

            Vector2 dirVec = isMoving ? moveInput : _playerController.LastFacingDirection;
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
                _timer += Time.deltaTime;
                int frameCount = _currentFrames.Length;
                // Sem modulo flutuante para evitar divisao: usa int cast + modulo inteiro.
                int idx = (int)(_timer * _framesPerSecond) % frameCount;
                _spriteRenderer.sprite = _currentFrames[idx];
            }
            else
            {
                _timer = 0f;
                _spriteRenderer.sprite = _currentFrames[0];
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
