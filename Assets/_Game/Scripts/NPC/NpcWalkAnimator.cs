using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Anima o SpriteRenderer de um NPC durante a caminhada, em 8 direcoes, a partir dos 25 sprites
    /// fatiados por <see cref="CindarsHope.Editor.NPC.GenerateNpcWalkAnimations"/> (grid 5 frames x 5
    /// direcoes geradas). Segue o mesmo esquema de bucket de direcao e avanco de frame do
    /// PlayerWalkAnimator (Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs), generalizado para NPC.
    ///
    /// So 5 direcoes tem arte gerada (down, downleft, right, up, upleft); as outras 3 (left,
    /// downright, upright) sao a mesma linha espelhada horizontalmente (flipX) — down e up nao
    /// precisam de espelho (simetricas no eixo X).
    ///
    /// Le o movimento via Rigidbody2D.linearVelocity no mesmo GameObject (GetComponent — permitido
    /// pela rule unity-architecture, leitura de componente irmao). Sem NpcDataSO.WalkAnimResourcesPath
    /// (NPC ainda sem anim de caminhada gerada): fica desabilitado, mostra so o BodySprite estatico.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NpcWalkAnimator : MonoBehaviour
    {
        private const int Columns = 5; // frames de walk por direcao
        private const int Rows = 5;    // direcoes geradas: down, downleft, right, up, upleft

        // Linhas geradas na folha, nessa ordem (topo->baixo na folha original, ja mapeado no slicer).
        private const int RowDown = 0;
        private const int RowDownLeft = 1;
        private const int RowRight = 2;
        private const int RowUp = 3;
        private const int RowUpLeft = 4;

        // 8 buckets de direcao (mesmo algoritmo de setor 45 graus do PlayerWalkAnimator).
        private const int DirRight = 0;
        private const int DirUpRight = 1;
        private const int DirUp = 2;
        private const int DirUpLeft = 3;
        private const int DirLeft = 4;
        private const int DirDownLeft = 5;
        private const int DirDown = 6;
        private const int DirDownRight = 7;
        private const int DirCount = 8;

        // Por bucket de 8 direcoes: de qual linha gerada tirar os frames, e se espelha (flipX).
        // Down/Up nao tem espelho dedicado pois sao simetricas no eixo horizontal.
        private static readonly int[] RowByDirection =
        {
            RowRight,     // DirRight
            RowUpLeft,    // DirUpRight (espelho de upleft)
            RowUp,        // DirUp
            RowUpLeft,    // DirUpLeft
            RowRight,     // DirLeft (espelho de right)
            RowDownLeft,  // DirDownLeft
            RowDown,      // DirDown
            RowDownLeft,  // DirDownRight (espelho de downleft)
        };

        private static readonly bool[] FlipByDirection =
        {
            false, // DirRight
            true,  // DirUpRight
            false, // DirUp
            false, // DirUpLeft
            true,  // DirLeft
            false, // DirDownLeft
            false, // DirDown
            true,  // DirDownRight
        };

        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private float _framesPerSecond = 7f;
        [SerializeField] private float _moveThreshold = 0.05f;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;

        // frames[row][col] — carregado uma vez em Awake a partir dos sprites fatiados.
        private Sprite[][] _frames;
        private bool _hasAnimation;

        private float _timer;
        private int _currentDir = DirDown;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();

            if (_npcData == null || string.IsNullOrEmpty(_npcData.WalkAnimResourcesPath))
            {
                // NPC sem anim de caminhada gerada ainda: mantem o BodySprite estatico, sem erro.
                _hasAnimation = false;
                enabled = false;
                return;
            }

            LoadFrames(_npcData.WalkAnimResourcesPath);

            // Garante um sprite IDLE visivel ja no Awake, sem esperar o primeiro Update. Assim o NPC
            // nunca fica "invisivel ate se mover" caso o BodySprite nao esteja setado na cena (bug
            // observado no Zrix da fazenda). Frame parado = walk_down coluna 0.
            if (_hasAnimation && _frames != null)
            {
                Sprite[] downRow = _frames[RowDown];
                if (downRow != null && downRow[0] != null)
                {
                    _spriteRenderer.sprite = downRow[0];
                }
            }
        }

        private void LoadFrames(string resourcesPath)
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>(resourcesPath);
            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogWarning($"[NpcWalkAnimator] Nenhum sprite encontrado em Resources/{resourcesPath} " +
                                 $"para NPC '{(_npcData != null ? _npcData.NpcId : name)}'. Anim desabilitada.");
                _hasAnimation = false;
                enabled = false;
                return;
            }

            _frames = new Sprite[Rows][];
            for (int r = 0; r < Rows; r++)
            {
                _frames[r] = new Sprite[Columns];
            }

            int parsed = 0;
            foreach (var sprite in loaded)
            {
                if (TryParseRowCol(sprite.name, out int row, out int col) && row >= 0 && row < Rows && col >= 0 && col < Columns)
                {
                    _frames[row][col] = sprite;
                    parsed++;
                }
            }

            // Diagnostico: se os sprites carregam mas os NOMES nao casam com "<id>_walk_r{r}_c{c}",
            // _frames fica vazio e o NPC "anda sem animar" SEM warning. Aqui sinalizamos e, se nenhum
            // frame valido, desabilitamos (mostra o BodySprite estatico em vez de nada).
            string npcId = _npcData != null ? _npcData.NpcId : name;
            if (parsed < Rows * Columns)
            {
                Debug.LogWarning($"[NpcWalkAnimator] NPC '{npcId}': carregou {loaded.Length} sprites de " +
                                 $"Resources/{resourcesPath} mas so {parsed}/{Rows * Columns} frames validos " +
                                 $"(nome esperado '<id>_walk_r{{row}}_c{{col}}'; 1o sprite carregado = " +
                                 $"'{(loaded.Length > 0 ? loaded[0].name : "<nenhum>")}'). Anim pode ficar parada.");
            }

            _hasAnimation = parsed > 0;
            if (!_hasAnimation)
            {
                enabled = false;
            }
        }

        // Espera nomes "<npcId>_walk_r{row}_c{col}" (gerados por GenerateNpcWalkAnimations).
        private static bool TryParseRowCol(string spriteName, out int row, out int col)
        {
            row = -1;
            col = -1;
            int rIdx = spriteName.LastIndexOf("_r", System.StringComparison.Ordinal);
            int cIdx = spriteName.LastIndexOf("_c", System.StringComparison.Ordinal);
            if (rIdx < 0 || cIdx < 0 || cIdx <= rIdx)
            {
                return false;
            }

            string rowStr = spriteName.Substring(rIdx + 2, cIdx - (rIdx + 2));
            string colStr = spriteName.Substring(cIdx + 2);
            return int.TryParse(rowStr, out row) && int.TryParse(colStr, out col);
        }

        private void Update()
        {
            if (!_hasAnimation || _frames == null) return;

            Vector2 velocity = _rigidbody != null ? _rigidbody.linearVelocity : Vector2.zero;
            bool isMoving = velocity.sqrMagnitude > _moveThreshold * _moveThreshold;

            if (isMoving)
            {
                _currentDir = GetDirectionBucket(velocity);
            }

            Sprite[] row = _frames[RowByDirection[_currentDir]];
            if (row == null) return;

            _spriteRenderer.flipX = FlipByDirection[_currentDir];

            if (isMoving)
            {
                _timer += Time.deltaTime;
                int frameCount = row.Length;
                int idx = (int)(_timer * _framesPerSecond) % frameCount;
                if (row[idx] != null)
                {
                    _spriteRenderer.sprite = row[idx];
                }
            }
            else
            {
                _timer = 0f;
                if (row[0] != null)
                {
                    _spriteRenderer.sprite = row[0];
                }
            }
        }

        /// <summary>Mesmo algoritmo de setor de 45 graus do PlayerWalkAnimator.GetDirectionBucket.</summary>
        private static int GetDirectionBucket(Vector2 v)
        {
            if (v == Vector2.zero) return DirDown;

            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;

            int sector = (int)((angle + 22.5f) / 45f) % 8;
            return sector;
        }
    }
}
