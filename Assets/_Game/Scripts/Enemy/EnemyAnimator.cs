using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Anima o SpriteRenderer de um inimigo da cave: caminhada em 8 direcoes + folhas de ataque,
    /// a partir dos sprites fatiados por <see cref="CindarsHope.Editor.Enemy.GenerateEnemyWalkAnimations"/>
    /// (grade N linhas x 5 colunas; N por folha = 5 bipede/quadrupede, 3 inseto/voador, 1 efeito radial).
    ///
    /// Diferencas em relacao ao NpcWalkAnimator (Assets/_Game/Scripts/NPC/NpcWalkAnimator.cs):
    ///  - Inimigos se movem por transform.position (a IA nao seta Rigidbody2D.velocity), entao a
    ///    velocidade e derivada do DELTA de posicao entre frames, nao lida do Rigidbody.
    ///  - Numero de linhas VARIA por folha (5/3/1); o mapa direcao->linha e escolhido por rowCount.
    ///  - Ataque: le EnemyBrain.CurrentState (componente irmao); em AttackWindup/CastPrepare/
    ///    AttackRecover troca para a folha de ataque (especial se EnemyBrain.CurrentActionId termina
    ///    em "_special" e a folha existir; senao a normal). Sem tocar em nada do combate.
    ///  - Escala: no Configure, redimensiona o transform para casar a ALTURA-ALVO de mundo que o
    ///    CaveEnemyMaterializer resolveu para o sprite de skin, agora medida pelo frame idle da walk.
    ///    Assim o inimigo animado tem exatamente o tamanho pretendido, sem "pular" ao trocar do skin
    ///    estatico para a folha animada (a folha e normalizada para frames internamente consistentes).
    ///
    /// So os inimigos do batch 1 (ver <see cref="EnemyIdToSlug"/>) tem folhas; os demais nao recebem
    /// este componente (o materializer so o adiciona quando ha slug mapeado) — mantem o skin estatico.
    /// Leitura de componente irmao (GetComponent) e permitida pela rule unity-architecture.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyAnimator : MonoBehaviour
    {
        private const int Columns = 5;

        // 8 buckets de direcao (mesmo setor de 45 graus do PlayerWalkAnimator/NpcWalkAnimator).
        private const int DirRight = 0;
        private const int DirUpRight = 1;
        private const int DirUp = 2;
        private const int DirUpLeft = 3;
        private const int DirLeft = 4;
        private const int DirDownLeft = 5;
        private const int DirDown = 6;
        private const int DirDownRight = 7;

        // --- Mapa direcao->linha para folhas de 5 linhas ---
        // ATENCAO: as folhas de INIMIGO foram geradas com diagonais da DIREITA (linha1 baixo-direita,
        // linha4 cima-direita) — diferente dos NPCs (que usaram diagonais da esquerda). Logo o flip
        // e o OPOSTO do NpcWalkAnimator: right/down-right/up-right SEM flip; left e as diagonais da
        // ESQUERDA espelham a linha correspondente da direita. down/up simetricas (sem flip).
        // Linhas: 0=down, 1=down-right, 2=right, 3=up, 4=up-right. Index = bucket de 8 direcoes.
        private static readonly int[] Row5 =    { 2, 4, 3, 4, 2, 1, 0, 1 };
        private static readonly bool[] Flip5 =  { false, false, false, true, true, true, false, false };

        // --- Mapa para folhas de 3 linhas (down=0, right=1, up=2) ---
        // Puro cima->linha2, puro baixo->linha0, tudo com componente horizontal->perfil (linha1),
        // espelhado para a esquerda. Le bem para insetos/voadores/quadrupedes de 3 linhas.
        private static readonly int[] Row3 =    { 1, 1, 2, 1, 1, 1, 0, 1 };
        private static readonly bool[] Flip3 =  { false, false, false, true, true, true, false, false };

        // --- Folhas de 1 linha (efeito radial/pilha): sempre linha 0, espelha por sinal de X ---

        // enemyId (roster legado do batch 1) -> slug de animacao (nome das folhas geradas).
        // As 2 trocas (mossling/spore_imp) usam gen_* porque as skins primarias do JSON (puffball/
        // rotcap) nao tinham membros para animar — ver memoria project-enemy-anim-batch1.
        private static readonly Dictionary<string, string> EnemyIdToSlug = new()
        {
            { "enemy_cave_mite",                  "verdant_mite" },
            { "enemy_stone_rat",                  "gen_dire_rat" },
            { "enemy_cave_bat",                   "roost_cave_bat" },
            { "enemy_goblin_grashnaar_scavenger", "goblin_scrounger" },
            { "enemy_kobold_scout",               "kobold_sentry" },
            { "enemy_cracked_bone",               "undead_shambler" },
            { "enemy_mossling",                   "gen_mossling" },
            { "enemy_blackroot_sprout",           "fungal_spreader" },
            { "enemy_spore_imp",                  "gen_spore_imp" },
            { "enemy_thorn_archer",               "gen_thorn_archer" },
            // --- Batch 2 (fungal 11-25 + gelo 26-40) ---
            { "enemy_rootsnare",                  "gen_root_snare" },
            { "enemy_goblin_urudakh_trapper",     "goblin_shredder" },
            { "enemy_orc_nyx_stalker",            "orc_grunt" },
            { "enemy_nyx_moth",                   "gloom_moth" },
            { "enemy_hollow_stagling",            "gen_hollow_stag" },
            { "enemy_frost_gnawer",               "gen_frost_rodent" },
            { "enemy_glassbone",                  "gen_cracked_skeleton" },
            { "enemy_icebound_sentinel",          "gen_sentinel" },
            { "enemy_cold_cult_acolyte",          "coldcult_preacher" },
            { "enemy_duergar_frostdelver",        "gen_duergar" },
            // --- Batch 2b (enemyId homonimo; cada slug e o primario da skin binding -> anim casa
            //     com o skin estatico). gen_moth/gen_vine_lasher NAO wirados: sao skins de variancia
            //     sem enemyId proprio (ver memoria project-enemy-anim-batch1). ---
            { "enemy_construct_sentry",           "construct_sentry" },
            { "enemy_corrupted_orc_champion",     "corrupted_orc_champion" },
            { "enemy_corrupted_vine_horror",      "corrupted_vine_horror" },
            { "enemy_cultist_zealot",             "cultist_zealot" },
            { "enemy_frost_wisp",                 "frost_wisp" },
            { "enemy_frostbound_revenant",        "frostbound_revenant" },
            { "enemy_gnome_tinkerer",             "gnome_tinkerer" },
            { "enemy_goblin_shaman",              "goblin_shaman" },
            { "enemy_rimelock_colossus",          "rimelock_colossus" },
        };

        // Slugs cujo corpo NAO tem pose parada (voadores/flutuadores): o ciclo de "walk" e na verdade
        // batida de asa / ondulacao e deve rodar SEMPRE, inclusive parado — senao congela num frame
        // (morcego com a asa travada no ar fica estranho). Ground creatures ficam no frame 0 (idle real).
        private static readonly HashSet<string> AlwaysAnimateSlugs = new()
        {
            "roost_cave_bat",   // morcego: bate asa mesmo pairando
            "fungal_spreader",  // agua-viva/blackroot: ondula flutuando no lugar
            "gloom_moth",       // mariposa (batch2): bate asa pairando
            "gen_root_snare",   // planta ancorada (batch2): ondula no lugar
            "frost_wisp",       // wisp de gelo (batch2b): flutua/pulsa sempre
            "corrupted_vine_horror", // horror de vinha ancorado (batch2b): ondula no lugar
        };

        // slug -> (clip de ataque NORMAL, clip de ataque ESPECIAL). Reuso quando especial = overlay
        // no golpe base (ver catalogo/README do batch): mite/rat/blackroot/spore_imp/thorn reusam.
        private static readonly Dictionary<string, (string normal, string special)> SlugAttackClips = new()
        {
            { "verdant_mite",     ("atk_bite",  "atk_bite") },
            { "gen_dire_rat",     ("atk_bite",  "atk_bite") },
            { "roost_cave_bat",   ("atk_bite",  "atk_scream") },
            { "goblin_scrounger", ("atk_slash", "atk_leap") },
            { "kobold_sentry",    ("atk_throw", "atk_scream") },
            { "undead_shambler",  ("atk_claw",  "atk_rise") },
            { "gen_mossling",     ("atk_claw",  "atk_nova") },
            { "fungal_spreader",  ("atk_whip",  "atk_whip") },
            { "gen_spore_imp",    ("atk_throw", "atk_throw") },
            { "gen_thorn_archer", ("atk_bow",   "atk_bow") },
            // --- Batch 2 ---
            { "gen_root_snare",       ("atk_whip",   "atk_whip") },   // especial (Root) = overlay
            { "goblin_shredder",      ("atk_slash",  "atk_throw") },  // lamina dupla / dardo-armadilha
            { "orc_grunt",            ("atk_cleave", "atk_blink") },  // machado / Manto de Nyx
            { "gloom_moth",           ("atk_claw",   "atk_nova") },   // rasante / Po Lunar (nuvem)
            { "gen_hollow_stag",      ("atk_slam",   "atk_charge") }, // chifrada / Investida Oca
            { "gen_frost_rodent",     ("atk_bite",   "atk_bite") },   // especial (Chill) = overlay
            { "gen_cracked_skeleton", ("atk_claw",   "atk_nova") },   // garra / Estilhacar
            { "gen_sentinel",         ("atk_thrust", "atk_thrust") }, // estocada / Prisao de Gelo (Root)
            { "coldcult_preacher",    ("atk_cast",   "atk_buff") },   // raio de frio / Prece do Frio
            { "gen_duergar",          ("atk_cleave", "atk_buff") },   // martelo / Crescer da Pedra
            // --- Batch 2b (kit = arquivo raw real das folhas geradas 2026-07-07) ---
            { "construct_sentry",       ("atk_slam",   "atk_cast") },
            { "corrupted_orc_champion", ("atk_cleave", "atk_slam") },
            { "corrupted_vine_horror",  ("atk_whip",   "atk_whip") },  // ancorada, 1 ataque
            { "cultist_zealot",         ("atk_cast",   "atk_buff") },
            { "frost_wisp",             ("atk_cast",   "atk_nova") },
            { "frostbound_revenant",    ("atk_cleave", "atk_rise") },
            { "gnome_tinkerer",         ("atk_throw",  "atk_summon") },
            { "goblin_shaman",          ("atk_cast",   "atk_buff") },
            { "rimelock_colossus",      ("atk_slam",   "atk_summon") },
        };

        // Fracao da ALTURA da celula que o personagem realmente ocupa. As celulas normalizadas
        // (tools/enemy_anim/normalize_enemy_sheets.py) tem CELL_H=360px mas escalam a mediana do
        // personagem para TARGET_CHAR_H=168px, deixando margem para bracos erguidos/asas/aneis. Como
        // o sprite fatiado tem bounds = celula INTEIRA, escalar pela altura de bounds deixaria o bicho
        // ~2x pequeno (so ~47% da celula e corpo). Compensamos por esta fracao (168/360) para o
        // personagem — nao a celula — casar a altura-alvo de mundo. ACOPLADO ao normalizador: se mudar
        // TARGET_CHAR_H/CELL_H la, atualize aqui.
        private const float CreatureCellHeightFraction = 168f / 360f;

        [SerializeField] private float _walkFps = 7f;
        [SerializeField] private float _idleFps = 4f;
        [SerializeField] private float _attackFps = 10f;
        [SerializeField] private float _moveThreshold = 0.03f;

        private SpriteRenderer _spriteRenderer;
        private EnemyBrain _brain;
        private Transform _target; // player: para o ataque encarar o alvo, nao o ultimo movimento

        private string _slug;
        private string _normalClip;
        private string _specialClip;

        // clip -> (rows, cols) frames; _frames[clip][row][col].
        private readonly Dictionary<string, Sprite[][]> _clips = new();
        private readonly Dictionary<string, int> _clipRows = new();

        // Log de diagnostico one-shot por slug (inimigos spawnam muitos; evita spam no Console).
        private static readonly HashSet<string> _loggedSlugs = new();

        private bool _hasWalk;
        private bool _alwaysAnimateIdle;
        private int _currentDir = DirDown;   // direcao do movimento (walk)
        private int _attackDir = DirDown;    // direcao travada do ataque atual (encara o alvo)
        private float _walkTimer;
        private float _idleTimer;
        private float _attackTimer;
        private Vector3 _lastPos;
        private Vector2 _smoothVel;          // velocidade suavizada (reduz jitter de direcao)
        private bool _wasAttacking;

        /// <summary>
        /// Chamado pelo CaveEnemyMaterializer apos setar sprite/escala do skin. Resolve o slug pelo
        /// enemyId; se o inimigo nao tem folhas (fora do batch), desabilita e mantem o skin estatico.
        /// targetWorldHeight = altura de mundo que o materializer resolveu (appliedScale * bounds.y do
        /// skin); usada para redimensionar o transform pela altura do frame idle animado.
        /// </summary>
        public void Configure(string enemyId, float targetWorldHeight, Transform target = null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _brain = GetComponent<EnemyBrain>();
            _target = target;

            if (string.IsNullOrEmpty(enemyId) || !EnemyIdToSlug.TryGetValue(enemyId, out _slug))
            {
                enabled = false;
                return;
            }

            if (SlugAttackClips.TryGetValue(_slug, out var pair))
            {
                _normalClip = pair.normal;
                _specialClip = pair.special;
            }
            _alwaysAnimateIdle = AlwaysAnimateSlugs.Contains(_slug);

            LoadClips();

            if (!_hasWalk)
            {
                enabled = false;
                return;
            }

            // Idle = frame 0 da linha "down" da walk; casa a altura de mundo alvo.
            Sprite idle = _clips["walk"][RowForDirection("walk", DirDown, out _)][0];
            if (idle != null)
            {
                _spriteRenderer.sprite = idle;
                // Altura REAL do personagem na celula = bounds da celula * fracao ocupada.
                float creatureH = idle.bounds.size.y * CreatureCellHeightFraction;
                if (creatureH > 0.001f && targetWorldHeight > 0.001f)
                {
                    float s = targetWorldHeight / creatureH;
                    transform.localScale = new Vector3(s, s, 1f);
                    // O SpriteJuice ja cacheou a escala do skin no Awake e a reimporia todo frame no
                    // squash; reapontar a base para a nova escala animada (senao o inimigo "pularia").
                    var juice = GetComponent<CindarsHope.Visual.SpriteJuice>();
                    if (juice != null) juice.SetBaseScale(transform.localScale);
                }
            }

            _lastPos = transform.position;
        }

        private void LoadClips()
        {
            // Carrega walk + idle + todos os clips de ataque conhecidos do slug (os que existirem).
            var wanted = new List<string> { "walk", "idle" };
            if (!string.IsNullOrEmpty(_normalClip) && !wanted.Contains(_normalClip)) wanted.Add(_normalClip);
            if (!string.IsNullOrEmpty(_specialClip) && !wanted.Contains(_specialClip)) wanted.Add(_specialClip);

            foreach (var clip in wanted)
            {
                var loaded = Resources.LoadAll<Sprite>($"EnemyAnimSprites/{_slug}_{clip}");
                if (loaded == null || loaded.Length == 0) continue;

                int maxRow = 0;
                foreach (var sp in loaded)
                {
                    if (TryParseRowCol(sp.name, out int r, out _) && r > maxRow) maxRow = r;
                }
                int rows = maxRow + 1;

                var frames = new Sprite[rows][];
                for (int r = 0; r < rows; r++) frames[r] = new Sprite[Columns];
                foreach (var sp in loaded)
                {
                    if (TryParseRowCol(sp.name, out int r, out int c) && r >= 0 && r < rows && c >= 0 && c < Columns)
                        frames[r][c] = sp;
                }

                _clips[clip] = frames;
                _clipRows[clip] = rows;
                if (clip == "walk") _hasWalk = true;
            }

            if (!_hasWalk)
            {
                if (_loggedSlugs.Add(_slug))
                    Debug.LogWarning($"[EnemyAnimator] Sem folha de walk em Resources/EnemyAnimSprites/{_slug}_walk " +
                                     $"(slug '{_slug}'). Verifique se 'CindarsHope/Inicializar Projeto' fatiou os sprites " +
                                     $"(o PNG deve importar como Sprite/Multiple, nao Textura Default). Anim desabilitada; mantem skin estatico.");
            }
            else if (_loggedSlugs.Add(_slug))
            {
                int walkRows = _clipRows.TryGetValue("walk", out int wr) ? wr : 0;
                Debug.Log($"[EnemyAnimator] slug '{_slug}': walk carregado (rows={walkRows}), " +
                          $"{_clips.Count} clip(s): {string.Join(", ", _clips.Keys)}.");
            }
        }

        // Espera "<slug>_<clip>_r{row}_c{col}".
        private static bool TryParseRowCol(string spriteName, out int row, out int col)
        {
            row = -1; col = -1;
            int rIdx = spriteName.LastIndexOf("_r", System.StringComparison.Ordinal);
            int cIdx = spriteName.LastIndexOf("_c", System.StringComparison.Ordinal);
            if (rIdx < 0 || cIdx < 0 || cIdx <= rIdx) return false;
            string rowStr = spriteName.Substring(rIdx + 2, cIdx - (rIdx + 2));
            string colStr = spriteName.Substring(cIdx + 2);
            return int.TryParse(rowStr, out row) && int.TryParse(colStr, out col);
        }

        private void Update()
        {
            if (!_hasWalk || _spriteRenderer == null) return;

            float dt = Time.deltaTime;
            Vector3 pos = transform.position;
            Vector2 instVel = dt > 0.0001f ? (Vector2)((pos - _lastPos) / dt) : Vector2.zero;
            _lastPos = pos;
            // Suaviza a velocidade (EMA) para o facing nao "tremer" com o micro-jitter do pathing.
            _smoothVel = Vector2.Lerp(_smoothVel, instVel, 1f - Mathf.Exp(-10f * dt));
            bool isMoving = _smoothVel.sqrMagnitude > _moveThreshold * _moveThreshold;
            if (isMoving) _currentDir = GetDirectionBucket(_smoothVel);

            // Estado de ataque via componente irmao (nao muda o combate).
            bool attacking = false;
            string attackClip = null;
            if (_brain != null)
            {
                var st = _brain.CurrentState;
                attacking = st == EnemyBrainState.AttackWindup
                            || st == EnemyBrainState.CastPrepare
                            || st == EnemyBrainState.AttackRecover;
                if (attacking) attackClip = ResolveAttackClip();
            }

            // Ao INICIAR o ataque, trava a direcao encarando o ALVO (player) — nao o ultimo movimento
            // (senao o inimigo ataca "de costas"). Mantem travada durante todo o golpe.
            if (attacking && !_wasAttacking)
            {
                if (_target != null)
                {
                    Vector2 toTarget = (Vector2)(_target.position - transform.position);
                    if (toTarget.sqrMagnitude > 0.0001f) _attackDir = GetDirectionBucket(toTarget);
                    else _attackDir = _currentDir;
                }
                else
                {
                    _attackDir = _currentDir;
                }
            }

            if (attacking && attackClip != null && _clips.ContainsKey(attackClip))
            {
                PlayAttack(attackClip);
            }
            else if (isMoving)
            {
                PlayWalk();
            }
            else
            {
                PlayIdle();
            }
            _wasAttacking = attacking;
        }

        private string ResolveAttackClip()
        {
            string id = _brain != null ? _brain.CurrentActionId : null;
            bool special = !string.IsNullOrEmpty(id)
                           && id.EndsWith("_special", System.StringComparison.Ordinal)
                           && !string.IsNullOrEmpty(_specialClip)
                           && _clips.ContainsKey(_specialClip);
            string clip = special ? _specialClip : _normalClip;
            if (!string.IsNullOrEmpty(clip) && _clips.ContainsKey(clip)) return clip;
            // fallback: qualquer clip de ataque carregado
            if (!string.IsNullOrEmpty(_normalClip) && _clips.ContainsKey(_normalClip)) return _normalClip;
            return null;
        }

        // Em movimento: loop do ciclo de caminhada na direcao atual.
        private void PlayWalk()
        {
            int row = RowForDirection("walk", _currentDir, out bool flip);
            var frames = _clips["walk"];
            if (row < 0 || row >= frames.Length || frames[row] == null) return;
            _spriteRenderer.flipX = flip;
            _walkTimer += Time.deltaTime;
            int idx = (int)(_walkTimer * _walkFps) % Columns;
            if (frames[row][idx] != null) _spriteRenderer.sprite = frames[row][idx];
        }

        // Parado: toca a folha de IDLE (micro-movimento: respiro, balanco de cauda/arma) em loop
        // lento. Sem folha de idle: voadores/flutuadores seguem o loop de walk (nao ha pose parada);
        // terrestres congelam no frame 0 da walk (idle estatico, comportamento antigo).
        private void PlayIdle()
        {
            _walkTimer = 0f;
            var frames = _clips.TryGetValue("idle", out var idleFrames) ? idleFrames : null;
            if (frames != null)
            {
                int row = RowForDirection("idle", _currentDir, out bool flip);
                if (row < 0 || row >= frames.Length || frames[row] == null) return;
                _spriteRenderer.flipX = flip;
                _idleTimer += Time.deltaTime;
                int idx = (int)(_idleTimer * _idleFps) % Columns;
                if (frames[row][idx] != null) _spriteRenderer.sprite = frames[row][idx];
                return;
            }

            // Sem folha de idle: fallback ao comportamento antigo.
            var walk = _clips["walk"];
            int wrow = RowForDirection("walk", _currentDir, out bool wflip);
            if (wrow < 0 || wrow >= walk.Length || walk[wrow] == null) return;
            _spriteRenderer.flipX = wflip;
            if (_alwaysAnimateIdle)
            {
                _walkTimer += Time.deltaTime;
                int idx = (int)(_walkTimer * _walkFps) % Columns;
                if (walk[wrow][idx] != null) _spriteRenderer.sprite = walk[wrow][idx];
            }
            else if (walk[wrow][0] != null)
            {
                _spriteRenderer.sprite = walk[wrow][0];
            }
        }

        private void PlayAttack(string clip)
        {
            if (!_wasAttacking) _attackTimer = 0f;
            _attackTimer += Time.deltaTime;
            int row = RowForDirection(clip, _attackDir, out bool flip);
            var frames = _clips[clip];
            if (row < 0 || row >= frames.Length || frames[row] == null) return;
            _spriteRenderer.flipX = flip;
            int idx = (int)(_attackTimer * _attackFps) % Columns;
            if (frames[row][idx] != null) _spriteRenderer.sprite = frames[row][idx];
        }

        /// <summary>Mapeia o bucket de 8 direcoes para a linha da folha, conforme rowCount do clip.</summary>
        private int RowForDirection(string clip, int dir, out bool flip)
        {
            int rows = _clipRows.TryGetValue(clip, out int r) ? r : 5;
            switch (rows)
            {
                case 5:
                    flip = Flip5[dir];
                    return Row5[dir];
                case 3:
                    flip = Flip3[dir];
                    return Row3[dir];
                default: // 1 (ou qualquer outro): linha 0, espelha por facing horizontal
                    flip = dir == DirLeft || dir == DirUpLeft || dir == DirDownLeft;
                    return 0;
            }
        }

        private static int GetDirectionBucket(Vector2 v)
        {
            if (v == Vector2.zero) return DirDown;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return (int)((angle + 22.5f) / 45f) % 8;
        }
    }
}
