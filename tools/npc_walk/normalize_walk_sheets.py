"""Normaliza as folhas de caminhada de NPC (5x5) geradas pelo ChatGPT (gpt-image-1).

Problema que resolve
--------------------
As folhas cruas em art/npc_anim_gpt/raw/gpt_<nome>_walk.png vem com DOIS defeitos que
estragam o slice 5x5 uniforme do editor:
  1. Fundo OPACO de xadrez claro (sem canal alpha) — nao e transparencia real.
  2. Os 25 personagens NAO ficam numa grade uniforme: cada um flutua fora de centro e as
     vezes a cabeca/apendices cruzam para a celula vizinha. Cortar numa grade 5x5 uniforme
     gera frames descentralizados, cortados e "tremidos" na animacao.

O que faz (por folha)
---------------------
  1. Remove o fundo: mascara pixels claros dessaturados (min>=215 e (max-min)<=16) e faz
     flood-fill 4-conexo a partir das bordas -> alpha=0. Preserva roupa branca INTERNA
     (cercada por outline). [mesmo algoritmo do slicer C# WriteWithTransparentBackground]
  2. Detecta as 5 faixas de linha (projecao de ocupacao) e, em cada faixa, as 5 colunas.
  3. Extrai o bbox apertado de cada um dos 25 personagens.
  4. Re-compoe numa grade 5x5 de celulas IGUAIS (CELL px), cada personagem centrado pelo
     ANCORA DOS PES (centroide da faixa inferior do corpo) e apoiado numa baseline fixa.
     => todos os frames alinhados: caminhada "no lugar", pivot BottomCenter correto.

Saida: art/npc_anim_gpt/normalized/gpt_<nome>_walk.png (RGBA, 5*CELL x 5*CELL).
O slicer do editor (GenerateNpcWalkAnimations) prefere essa pasta.

Requer: Python 3 + Pillow + numpy. Uso:
    py tools/npc_walk/normalize_walk_sheets.py            # todas as folhas de raw/
    py tools/npc_walk/normalize_walk_sheets.py dagna sael # so essas
"""
import os, sys
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
RAW  = os.path.join(REPO, 'art', 'npc_anim_gpt', 'raw')
OUT  = os.path.join(REPO, 'art', 'npc_anim_gpt', 'normalized')
NP   = os.path.join(REPO, 'Assets', '_Game', 'Resources', 'NpcSprites')

# ESCALA: o sprite de walk usa o MESMO PPU (234) do sprite base (BodySprite). Se o personagem
# na folha de walk for maior em pixels que no base, o NPC "incha" ao andar. Por isso escalamos
# cada personagem para a MESMA altura (px) do seu sprite base -> mesmo tamanho no jogo, igual ao
# player. Sem isso os NPCs saem ~1.8-2x maiores andando. Fallback DEFAULT_BASE_H se base ausente.
DEFAULT_BASE_H = 128
CELL = 150          # celula de saida (>= maior personagem ja ESCALADO ~140 + margem)
FOOT_MARGIN = 6     # px da baseline dos pes ao fundo da celula
FEET_FRAC = 0.22    # fracao inferior do corpo usada como ancora horizontal (pes/pernas)
MIN_LIGHT = 215
MAX_SAT = 16

# nome-curto -> arquivo do sprite base (para casar a altura). Cruzado com AssignNpcBodySprites.
BASEFILE = {
 'alaric':'ser_alaric_veyr.png','brumdar':'brumdar_ferro_quieto.png','dagna':'dagna_rocha_morna.png',
 'eiran':'eiran_valeclaro.png','gruta':'gruta_panela_funda.png','gurd':'gurd_carvalho_torto.png',
 'hess':'hess_couro_fundo.png','hund':'hund_carvalho_torto.png','liora':'liora_canta_rio.png',
 'maelor':'maelor_cinza.png','mara':'mara_vellum.png','mella':'mella_forno_quente.png',
 'mirela':'mirela_dos_lacos.png','nimble':'nimble_galhobaixo.png','orlan':'orlan_pouso_curto.png',
 'ozzra':'ozzra_fumacazul.png','pip':'pip_semente_solta.png','renko':'renko_tres_sorrisos.png',
 'sael':'sael_mare_quieta.png','savra':'savra_escama_verde.png','sylveth':'sylveth.png',
 'thalindra':'thalindra_veu_de_lua.png','tibbet':'tibbet_vela_torta.png','tovin':'tovin_maos_de_selo.png',
 'velorin':'anciao_velorin.png','yael':'yael_noite_mansa.png','zrix_goblin':'zrix_das_estradas.png',
}

def base_char_height(name):
    bf = BASEFILE.get(name)
    if not bf: return DEFAULT_BASE_H
    p = os.path.join(NP, bf)
    if not os.path.exists(p): return DEFAULT_BASE_H
    a = np.array(Image.open(p).convert('RGBA'))[...,3] > 40
    ys = np.where(a.any(1))[0]
    return int(ys.max()-ys.min()+1) if len(ys) else DEFAULT_BASE_H

def strip_bg(rgb):
    """RGB HxWx3 -> alpha HxW (0 no fundo xadrez conectado a borda)."""
    mn = rgb.min(2).astype(np.int16); mx = rgb.max(2).astype(np.int16)
    light = (mn >= MIN_LIGHT) & ((mx - mn) <= MAX_SAT)
    H, W = light.shape
    m = light.ravel(); vis = np.zeros(H*W, bool); st = []
    for idx in np.concatenate([np.arange(0,W), np.arange((H-1)*W,H*W),
                               np.arange(0,H*W,W), np.arange(W-1,H*W,W)]):
        if m[idx] and not vis[idx]:
            vis[idx] = True; st.append(int(idx))
    while st:
        i = st.pop(); y, x = divmod(i, W)
        for j in ((i-1) if x>0 else -1, (i+1) if x<W-1 else -1,
                  (i-W) if y>0 else -1, (i+W) if y<H-1 else -1):
            if j >= 0 and m[j] and not vis[j]:
                vis[j] = True; st.append(j)
    return np.where(vis.reshape(H, W), 0, 255).astype(np.uint8)

def runs(mask, min_len):
    out=[]; s=None
    for i,v in enumerate(mask):
        if v and s is None: s=i
        elif not v and s is not None:
            if i-s>=min_len: out.append((s,i-1))
            s=None
    if s is not None and len(mask)-s>=min_len: out.append((s,len(mask)-1))
    return out

def detect(a):
    H,W = a.shape
    row_bands = runs(a.sum(1) > W*0.01, H//40)
    if len(row_bands)!=5: return None, f'{len(row_bands)} faixas de linha'
    grid=[]
    for (y0,y1) in row_bands:
        band = a[y0:y1+1,:]
        cols = runs(band.sum(0) > (y1-y0)*0.02, W//40)
        if len(cols)!=5: return None, f'linha y[{y0},{y1}] tem {len(cols)} colunas'
        cells=[]
        for (x0,x1) in cols:
            ys,xs = np.where(a[y0:y1+1, x0:x1+1])
            cells.append((x0+xs.min(), x0+xs.max(), y0+ys.min(), y0+ys.max()))
        grid.append(cells)
    return grid, 'ok'

def anchor_x(a, bb):
    x0,x1,y0,y1 = bb
    fh = max(1, int((y1-y0)*FEET_FRAC))
    ys,xs = np.where(a[y1-fh:y1+1, x0:x1+1])
    return (x0 + xs.mean()) if len(xs) else (x0+x1)/2

def normalize_one(name):
    src = os.path.join(RAW, f'gpt_{name}_walk.png')
    if not os.path.exists(src):
        print(f'{name}: MISSING'); return False
    rgb = np.array(Image.open(src).convert('RGB'))
    alpha = strip_bg(rgb)
    im = Image.fromarray(np.dstack([rgb, alpha]), 'RGBA')
    a = alpha > 40
    grid, msg = detect(a)
    if grid is None:
        print(f'{name}: FALHA na deteccao ({msg}) — NAO normalizado'); return False
    # fator de escala p/ casar a altura do personagem com o sprite BASE (mesmo PPU 234 -> mesmo
    # tamanho no jogo). Referencia = MEDIANA das 25 alturas de bbox (robusta a frames de braco alto).
    heights = [grid[r][c][3]-grid[r][c][2]+1 for r in range(5) for c in range(5)]
    ref_h = float(sorted(heights)[len(heights)//2])
    base_h = base_char_height(name)
    scale = base_h / ref_h if ref_h > 0 else 1.0
    out = Image.new('RGBA', (CELL*5, CELL*5), (0,0,0,0))
    warn=''
    for r in range(5):
        for c in range(5):
            x0,x1,y0,y1 = grid[r][c]
            char = im.crop((x0,y0,x1+1,y1+1))
            sw = max(1, int(round(char.width*scale)))
            sh = max(1, int(round(char.height*scale)))
            char = char.resize((sw, sh), Image.LANCZOS)
            if sw>CELL or sh>CELL: warn=f' WARN char escalado {sw}x{sh}>CELL {CELL}'
            ax = (anchor_x(a, grid[r][c]) - x0) * scale
            px = int(round(c*CELL + CELL//2 - ax))
            py = int(round(r*CELL + (CELL - FOOT_MARGIN) - sh))
            out.alpha_composite(char, (px, py))
    os.makedirs(OUT, exist_ok=True)
    out.save(os.path.join(OUT, f'gpt_{name}_walk.png'))
    print(f'{name}: ok  base_h={base_h} ref_h={int(ref_h)} scale={scale:.2f}{warn}')
    return True

def main():
    names = sys.argv[1:]
    if not names:
        names = sorted(f[4:-9] for f in os.listdir(RAW)
                       if f.startswith('gpt_') and f.endswith('_walk.png')
                       and not any(f.endswith(s) for s in ('_bad.png','_backup.png','_base.png','_old.png')))
    ok=0
    for n in names:
        if normalize_one(n): ok+=1
    print(f'\n{ok}/{len(names)} normalizadas -> {OUT}')

if __name__ == '__main__':
    main()
