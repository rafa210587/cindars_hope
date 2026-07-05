"""Normaliza as folhas de animacao dos INIMIGOS geradas pelo ChatGPT (gpt-image-1).

Generaliza tools/npc_walk/normalize_walk_sheets.py para o batch de inimigos:
  - grades de N linhas x 5 colunas, com N variavel por folha (5, 3 ou 1) — ver CLIPS.
  - remove o fundo opaco de xadrez claro (flood-fill de borda; preserva claros internos como
    pintas creme dos cogumelos, cercadas por outline).
  - re-compoe numa grade de celulas iguais, cada personagem centrado pelo ANCORA DOS PES e
    apoiado numa baseline fixa -> anim "no lugar", pivot BottomCenter, sem tremor/clip.
  - ESCALA: cada personagem e escalado para uma ALTURA-ALVO fixa (TARGET_CHAR_H) dentro da
    celula. O fator e calculado UMA vez por slug (da folha de walk, referencia = mediana das
    alturas dos 5 frames frontais) e REUSADO em todas as folhas do slug (walk + ataques), para
    o inimigo ter o MESMO tamanho em todas as animacoes. O tamanho ABSOLUTO in-game NAO vem daqui:
    o EnemyAnimator, no spawn, redimensiona o transform para a altura-alvo do CaveEnemyMaterializer
    (mesma formula visualScale*0.5*nudge por altura de bounds) usando o frame idle da walk — entao
    a folha so precisa ser INTERNAMENTE consistente (todos os frames do mesmo tamanho e baseline).

Saida: art/enemy_anim_gpt/normalized/gpt_<slug>_<clip>.png (RGBA, 5*CELL_W x N*CELL_H).
O slicer do editor (GenerateEnemyWalkAnimations) prefere essa pasta.

Requer: Python 3 + Pillow + numpy. Uso:
    py tools/enemy_anim/normalize_enemy_sheets.py                     # todas as folhas de raw/
    py tools/enemy_anim/normalize_enemy_sheets.py gen_dire_rat        # so as folhas desse slug
"""
import os, sys
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
RAW  = os.path.join(REPO, 'art', 'enemy_anim_gpt', 'raw')
OUT  = os.path.join(REPO, 'art', 'enemy_anim_gpt', 'normalized')

# clip -> numero de linhas de direcao geradas na folha (colunas = sempre 5).
#   5 linhas: down, downleft, right, up, upleft (as outras 3 = flipX em runtime)
#   3 linhas: down, right, up (diagonais reusam o perfil; left = flipX)
#   1 linha : efeito radial / pilha no chao / drift simetrico (mesma faixa p/ toda direcao)
CLIP_ROWS = {
    'walk': None,       # variavel por slug (ver SLUG_WALK_ROWS)
    'atk_bite': None,
    'atk_slash': None,
    'atk_claw': None,
    'atk_throw': None,
    'atk_scream': None,
    'atk_leap': None,
    'atk_bow': None,
    'atk_whip': None,
    'atk_nova': 1,
    'atk_rise': 1,
}
# linhas da folha de MOVIMENTO por slug (define o "corpo": bipede=5, inseto/quadrupede/voador=3/5,
# ancorado=1). As folhas de ATAQUE do slug herdam o mesmo N, exceto clips forcados a 1 (nova/rise).
SLUG_WALK_ROWS = {
    'verdant_mite':     3,
    'gen_dire_rat':     5,
    'roost_cave_bat':   3,
    'goblin_scrounger': 5,
    'kobold_sentry':    5,
    'undead_shambler':  5,
    'gen_mossling':     5,
    'fungal_spreader':  1,
    'gen_spore_imp':    5,
    'gen_thorn_archer': 5,
}
# folhas de ATAQUE cujo N de linhas difere do walk do slug (o prompt pediu 3 linhas nesses).
CLIP_ROWS_OVERRIDE = {
    ('verdant_mite', 'atk_bite'): 3,
    ('roost_cave_bat', 'atk_bite'): 3,
    ('roost_cave_bat', 'atk_scream'): 3,
    ('kobold_sentry', 'atk_scream'): 3,
    ('kobold_sentry', 'atk_claw'): 3,
    ('gen_mossling', 'atk_claw'): 3,
    ('fungal_spreader', 'atk_whip'): 3,
    ('gen_spore_imp', 'atk_whip'): 3,
    ('gen_thorn_archer', 'atk_slash'): 3,
}

COLS = 5
# celulas generosas: cabem asas de morcego abertas (~300w), anel de esporos (~313w), coluna
# magica do rise (~341h) e rastros de chicote sem cortar nas bordas.
CELL_W = 320
CELL_H = 360
TARGET_CHAR_H = 168   # altura-alvo (px) da mediana do personagem dentro da celula
FOOT_MARGIN = 14
FEET_FRAC = 0.22
MIN_LIGHT = 215
MAX_SAT = 16


def strip_bg(rgb):
    """RGB HxWx3 -> alpha HxW (0 no fundo xadrez claro conectado a borda)."""
    mn = rgb.min(2).astype(np.int16); mx = rgb.max(2).astype(np.int16)
    light = (mn >= MIN_LIGHT) & ((mx - mn) <= MAX_SAT)
    H, W = light.shape
    m = light.ravel(); vis = np.zeros(H * W, bool); st = []
    for idx in np.concatenate([np.arange(0, W), np.arange((H - 1) * W, H * W),
                               np.arange(0, H * W, W), np.arange(W - 1, H * W, W)]):
        if m[idx] and not vis[idx]:
            vis[idx] = True; st.append(int(idx))
    while st:
        i = st.pop(); y, x = divmod(i, W)
        for j in ((i - 1) if x > 0 else -1, (i + 1) if x < W - 1 else -1,
                  (i - W) if y > 0 else -1, (i + W) if y < H - 1 else -1):
            if j >= 0 and m[j] and not vis[j]:
                vis[j] = True; st.append(j)
    return np.where(vis.reshape(H, W), 0, 255).astype(np.uint8)


def runs(mask, min_len):
    out = []; s = None
    for i, v in enumerate(mask):
        if v and s is None: s = i
        elif not v and s is not None:
            if i - s >= min_len: out.append((s, i - 1))
            s = None
    if s is not None and len(mask) - s >= min_len: out.append((s, len(mask) - 1))
    return out


def _bbox_in(a, ry0, ry1, rx0, rx1):
    """bbox apertado do conteudo dentro do retangulo [ry0..ry1]x[rx0..rx1]; None se vazio."""
    sub = a[ry0:ry1 + 1, rx0:rx1 + 1]
    ys, xs = np.where(sub)
    if len(xs) == 0:
        return None
    return (rx0 + xs.min(), rx0 + xs.max(), ry0 + ys.min(), ry0 + ys.max())


def detect_projection(a, nrows):
    """Content-aware: nrows faixas por projecao + 5 colunas por faixa. Preciso quando as
    celulas estao bem separadas; falha (retorna None) se projetil/rastro cruza a fronteira."""
    H, W = a.shape
    row_bands = runs(a.sum(1) > W * 0.01, H // (nrows * 8 + 4))
    if len(row_bands) != nrows:
        return None, f'{len(row_bands)} faixas de linha (esperado {nrows})'
    grid = []
    for (y0, y1) in row_bands:
        band = a[y0:y1 + 1, :]
        cols = runs(band.sum(0) > (y1 - y0) * 0.02, W // 40)
        if len(cols) != COLS:
            return None, f'linha y[{y0},{y1}] tem {len(cols)} colunas (esperado {COLS})'
        cells = []
        for (x0, x1) in cols:
            bb = _bbox_in(a, y0, y1, x0, x1)
            cells.append(bb if bb else (x0, x1, y0, y1))
        grid.append(cells)
    return grid, 'ok'


def detect_uniform(a, nrows):
    """Fallback robusto a projetil/rastro: divide a imagem em grade UNIFORME nrows x 5 e pega o
    bbox do conteudo DENTRO de cada celula uniforme (recorte limita o bleed do vizinho). Nunca
    falha. Menos preciso se o personagem flutua muito fora do centro da celula."""
    H, W = a.shape
    ch = H / nrows
    cw = W / COLS
    grid = []
    for r in range(nrows):
        ry0, ry1 = int(round(r * ch)), int(round((r + 1) * ch)) - 1
        cells = []
        for c in range(COLS):
            rx0, rx1 = int(round(c * cw)), int(round((c + 1) * cw)) - 1
            bb = _bbox_in(a, ry0, ry1, rx0, rx1)
            cells.append(bb if bb else (rx0, rx1, ry0, ry1))
        grid.append(cells)
    return grid, 'uniforme'


def detect(a, nrows):
    grid, msg = detect_projection(a, nrows)
    if grid is not None:
        return grid, msg
    return detect_uniform(a, nrows)


def anchor_x(a, bb):
    x0, x1, y0, y1 = bb
    fh = max(1, int((y1 - y0) * FEET_FRAC))
    ys, xs = np.where(a[y1 - fh:y1 + 1, x0:x1 + 1])
    return (x0 + xs.mean()) if len(xs) else (x0 + x1) / 2


def rows_for(slug, clip):
    if (slug, clip) in CLIP_ROWS_OVERRIDE:
        return CLIP_ROWS_OVERRIDE[(slug, clip)]
    forced = CLIP_ROWS.get(clip)
    if forced is not None:
        return forced
    return SLUG_WALK_ROWS.get(slug, 5)


def normalize_one(slug, clip, scale_cache):
    src = os.path.join(RAW, f'gpt_{slug}_{clip}.png')
    if not os.path.exists(src):
        return None
    nrows = rows_for(slug, clip)
    rgb = np.array(Image.open(src).convert('RGB'))
    alpha = strip_bg(rgb)
    im = Image.fromarray(np.dstack([rgb, alpha]), 'RGBA')
    a = alpha > 40
    grid, msg = detect(a, nrows)
    if grid is None:
        print(f'{slug}_{clip}: FALHA na deteccao ({msg}) — NAO normalizado')
        return False

    # escala: reusa a da walk do slug (calculada 1x, mediana -> TARGET_CHAR_H); ataques herdam.
    if slug in scale_cache:
        scale = scale_cache[slug]
    else:
        heights = [grid[r][c][3] - grid[r][c][2] + 1 for r in range(nrows) for c in range(COLS)]
        ref_h = float(sorted(heights)[len(heights) // 2])
        scale = TARGET_CHAR_H / ref_h if ref_h > 0 else 1.0
        scale_cache[slug] = scale  # trava (walk vem 1o por clips_for_slug); ataques herdam

    out = Image.new('RGBA', (CELL_W * COLS, CELL_H * nrows), (0, 0, 0, 0))
    warn = ''
    for r in range(nrows):
        for c in range(COLS):
            x0, x1, y0, y1 = grid[r][c]
            char = im.crop((x0, y0, x1 + 1, y1 + 1))
            sw = max(1, int(round(char.width * scale)))
            sh = max(1, int(round(char.height * scale)))
            char = char.resize((sw, sh), Image.LANCZOS)
            if sw > CELL_W or sh > CELL_H:
                warn = f' WARN char {sw}x{sh} > celula {CELL_W}x{CELL_H}'
            ax = (anchor_x(a, grid[r][c]) - x0) * scale
            px = int(round(c * CELL_W + CELL_W // 2 - ax))
            py = int(round(r * CELL_H + (CELL_H - FOOT_MARGIN) - sh))
            out.alpha_composite(char, (px, py))
    os.makedirs(OUT, exist_ok=True)
    out.save(os.path.join(OUT, f'gpt_{slug}_{clip}.png'))
    print(f'{slug}_{clip}: ok  rows={nrows} scale={scale:.2f} det={msg}{warn}')
    return True


def all_slugs_in_raw():
    slugs = set()
    for f in os.listdir(RAW):
        if f.startswith('gpt_') and f.endswith('.png'):
            stem = f[4:-4]  # remove gpt_ e .png
            for clip in CLIP_ROWS:
                if stem.endswith('_' + clip):
                    slugs.add(stem[:-(len(clip) + 1)])
                    break
    return sorted(slugs)


def clips_for_slug(slug):
    found = []
    for clip in CLIP_ROWS:
        if os.path.exists(os.path.join(RAW, f'gpt_{slug}_{clip}.png')):
            found.append(clip)
    # walk PRIMEIRO (trava a escala reutilizada pelos ataques)
    found.sort(key=lambda c: (c != 'walk', c))
    return found


def main():
    targets = sys.argv[1:] or all_slugs_in_raw()
    scale_cache = {}
    ok = 0; total = 0
    for slug in targets:
        for clip in clips_for_slug(slug):
            total += 1
            r = normalize_one(slug, clip, scale_cache)
            if r:
                ok += 1
    print(f'\n{ok}/{total} folhas normalizadas -> {OUT}')


if __name__ == '__main__':
    main()
