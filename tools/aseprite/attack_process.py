"""Processa sheets de ATAQUE (fundo cinza, arma cinza/marrom + rastro/flecha):
rembg (preserva arma por forma) -> N colunas IGUAIS -> trim vertical GLOBAL
(baseline/altura consistente, sem recentralizar por frame => preserva lunge/arma)
-> downscale uniforme (corpo em pe ~char_h) -> frames + contact sheet + gif.
Saida de trabalho em tools/aseprite/work/<kind>/<dir>/.

Uso:
  python attack_process.py [sword|bow] [dir1 dir2 ...]   (default: sword, todos)
"""
import os, sys, glob
os.environ.setdefault('OMP_NUM_THREADS', '1')  # estabiliza onnxruntime (evita crash 0xC0000005)
import numpy as np
from PIL import Image, ImageDraw
from rembg import remove, new_session
from scipy import ndimage

ART = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\animations\Fazendeiro'
ROOT_WORK = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\work'
CHAR_H = 72       # altura do corpo em pe (casa com walk/idle)
WIN_FACTOR = 1.3  # alarga a janela do frame (so quando strip): cabe o swoosh inteiro sem clipar

# sheets: dir -> (arquivo, nframes). nframes varia por sheet (a IA nem sempre gera 4/5 fixo).
KINDS = {
    'sword': {
        'work': 'attack', 'gif_ms': 110, 'char_h': 69,  # bem pouquinho menor (era 72)
        'sheets': {
            'down':      ('ataque baixo.png', 4),
            'up':        ('ataque cima.png', 4),
            'right':     ('ataque direita.png', 4),
            'downright': ('ataque diagnal baixo direita.png', 4),
            'upright':   ('ataque diagnal cima direita.png', 4),
        },
        # sword guardou _source.png nas pastas; cai pra art\attack\<dir>\_source.png se a sheet sumiu do topo
        'fallback_source': lambda d: os.path.join(ART, 'attack', d, '_source.png'),
        'strip_detached': True,   # remove blobs soltos (vazamento de coluna) — so na espada
    },
    'bow': {
        'work': 'bow', 'gif_ms': 110, 'char_h': 67,  # arco ~7% menor (ele fica maior usando arco)
        'strip_detached': False,  # arco: a flecha voadora e solta de proposito (manter)
        'sheets': {
            'down':      ('Ataque baixo flecha.png', 5),
            'up':        ('Ataque cima flecha.png', 5),
            'right':     ('Ataque direita flecha.png', 5),
            'downright': ('Ataque diagonal baixodireita flecha.png', 5),
            'upright':   ('Ataque diagonal cima direita flecha.png', 4),
        },
        'fallback_source': lambda d: os.path.join(ART, 'bow', d, '_source.png'),
    },
}

def content_rows(a):
    return (a[:, :, 3] > 12).any(axis=1)

def keep_main_component(im):
    """Mantem so o maior componente conexo (corpo+arma+trail conectado), zerando blobs
    soltos — ex.: cauda do swoosh da coluna vizinha que vazou na fatia. Usado so na espada;
    no arco a flecha voadora e solta de proposito e NAO deve ser removida."""
    a = np.array(im)
    mask = a[:, :, 3] > 30
    lbl, n = ndimage.label(mask)
    if n <= 1:
        return im
    sizes = ndimage.sum(np.ones_like(lbl), lbl, range(1, n + 1))
    main = int(np.argmax(sizes)) + 1
    a[(lbl != main) & (lbl != 0), 3] = 0
    return Image.fromarray(a, 'RGBA')

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

def process(dir_name, sheet_path, sess, nframes, fwork, gif_ms, strip_detached=False, char_h=CHAR_H):
    img = Image.open(sheet_path).convert('RGBA')
    cut = remove(img, session=sess)
    a = np.array(cut)
    H, W = a.shape[:2]
    colw = W // nframes
    # Janela do frame: alargada quando vamos limpar blobs soltos (espada) — assim o swoosh
    # inteiro cabe sem clipar, e o vazamento do vizinho e removido depois pelo keep_main_component.
    winw = round(colw * WIN_FACTOR) if strip_detached else colw

    rows = content_rows(a)
    ys = np.where(rows)[0]
    y0, y1 = int(ys.min()), int(ys.max()) + 1

    # escala: altura do CORPO no frame 0, ignorando a arma (pixels finos).
    f0 = a[y0:y1, 0:colw, :]
    rowcount = (f0[:, :, 3] > 12).sum(axis=1)
    thr = max(8, 0.15 * colw)
    body_rows = np.where(rowcount > thr)[0]
    body_h = int(np.ptp(body_rows)) + 1 if body_rows.size else (y1 - y0)
    scale = char_h / body_h

    out = os.path.join(fwork, dir_name)
    os.makedirs(out, exist_ok=True)
    for old in glob.glob(os.path.join(out, '*')): os.remove(old)

    cellw = max(1, round(winw * scale)); cellh = max(1, round((y1 - y0) * scale))
    cells = []
    for i in range(nframes):
        cx = i * colw + colw // 2                 # centro do frame na grade
        x0 = cx - winw // 2                        # janela centrada (pode sair da borda: PIL preenche transparente)
        sub = cut.crop((x0, y0, x0 + winw, y1)).resize((cellw, cellh), Image.LANCZOS)
        if strip_detached:
            sub = keep_main_component(sub)
        sub.save(os.path.join(out, f'frame_{i+1:02d}.png'))
        cells.append(sub)

    SC = 2
    sheet_img = Image.new('RGBA', ((cellw*SC)*nframes, cellh*SC + 16), (30,30,38,255))
    dr = ImageDraw.Draw(sheet_img)
    for i, c in enumerate(cells):
        big = c.resize((cellw*SC, cellh*SC), Image.NEAREST)
        cb = checker(cellw*SC, cellh*SC); cb.alpha_composite(big)
        sheet_img.alpha_composite(cb, (i*cellw*SC, 0))
        dr.text((i*cellw*SC + 4, cellh*SC + 2), str(i+1), fill=(255,255,80,255))
    sheet_img.convert('RGB').save(os.path.join(fwork, f'contact_{dir_name}.png'))

    gi = [Image.alpha_composite(checker(cellw, cellh), c).convert('P', palette=Image.ADAPTIVE) for c in cells]
    gi[0].save(os.path.join(fwork, f'anim_{dir_name}.gif'), save_all=True,
               append_images=gi[1:], duration=gif_ms, loop=0, disposal=2)
    print(f'{dir_name}: {nframes}f cell {cellw}x{cellh}, body_h={body_h}px scale={scale:.3f} -> {out}', flush=True)

def main():
    kind = sys.argv[1] if len(sys.argv) > 1 and sys.argv[1] in KINDS else 'sword'
    cfg = KINDS[kind]
    fwork = os.path.join(ROOT_WORK, cfg['work'])
    os.makedirs(fwork, exist_ok=True)
    rest = [x for x in sys.argv[2:] if x in cfg['sheets']] or list(cfg['sheets'].keys())
    sess = new_session('u2net')
    for d in rest:
        fname, nframes = cfg['sheets'][d]
        sheet_path = os.path.join(ART, fname)
        if not os.path.exists(sheet_path) and cfg['fallback_source']:
            sheet_path = cfg['fallback_source'](d)
        process(d, sheet_path, sess, nframes, fwork, cfg['gif_ms'],
                cfg.get('strip_detached', False), cfg.get('char_h', CHAR_H))

if __name__ == '__main__':
    main()
