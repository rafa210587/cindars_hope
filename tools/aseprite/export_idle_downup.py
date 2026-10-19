"""Exporta idle DOWN e UP: TODOS os 10 frames (1->10), celula uniforme bottom-center,
gif lento. Saida final em Resources/PlayerSprites/idle/<dir>/ + preview no work dir."""
import os, glob
from PIL import Image

FWORK = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\work\idle'
RES   = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\PlayerSprites\idle'
GIF_MS = 360  # mais lento entre frames (walk idle ref era 260)

def load_all(slug):
    d = os.path.join(FWORK, slug + '_sl')
    frames = []
    for ri in (0, 1):
        for fi in range(5):
            p = os.path.join(d, f'row{ri:02d}_f{fi:02d}.png')
            if os.path.exists(p):
                frames.append(Image.open(p).convert('RGBA'))
    return frames

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

def build(slug, dir_name):
    frames = load_all(slug)
    cw = max(f.width for f in frames); ch = max(f.height for f in frames)
    out = os.path.join(RES, dir_name)
    os.makedirs(out, exist_ok=True)
    for old in glob.glob(os.path.join(out, '*.png')) + glob.glob(os.path.join(out, '*.gif')):
        os.remove(old)
    cells = []
    for k, fr in enumerate(frames, 1):
        cell = Image.new('RGBA', (cw, ch), (0,0,0,0))
        cell.alpha_composite(fr, ((cw-fr.width)//2, ch-fr.height))  # bottom-center
        cell.save(os.path.join(out, f'idle_{dir_name}_{k:02d}.png'))
        cells.append(cell)
    gi = []
    for c in cells:
        cv = checker(cw, ch).copy(); cv.alpha_composite(c)
        gi.append(cv.convert('P', palette=Image.ADAPTIVE))
    gif_path = os.path.join(FWORK, f'idle_{dir_name}_preview.gif')
    gi[0].save(gif_path, save_all=True, append_images=gi[1:], duration=GIF_MS, loop=0, disposal=2)
    # also drop a copy of the gif next to the frames in Resources work? keep only in FWORK preview
    print(f'idle_{dir_name}: {len(cells)} frames, cell {cw}x{ch}, {GIF_MS}ms -> {out}')
    print(f'  preview gif: {gif_path}')

build('idle_down', 'down')
build('idle_up', 'up')
