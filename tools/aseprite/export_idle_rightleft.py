"""Exporta idle RIGHT (todos os frames da sheet) e espelha para LEFT (flipX).
Saida em Resources/PlayerSprites/idle/{right,left}/ + preview gif no work dir.
Le os frames fatiados do scratchpad (idle_right_sl)."""
import os, glob
from PIL import Image

SL    = r'C:\Users\Rafa\AppData\Local\Temp\claude\F--Projetos-Jogos-Cindars-hope-cindars-hope\8bcd3b92-eb8e-4129-b317-e83069a2fe97\scratchpad\idle_right_sl'
FWORK = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\work\idle'
RES   = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\PlayerSprites\idle'
GIF_MS = 360

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

# carrega frames do right na ordem
src = sorted(glob.glob(os.path.join(SL, 'row00_f*.png')))
frames = [Image.open(f).convert('RGBA') for f in src]
cw = max(f.width for f in frames); ch = max(f.height for f in frames)

def emit(dir_name, mirror):
    out = os.path.join(RES, dir_name)
    os.makedirs(out, exist_ok=True)
    for old in glob.glob(os.path.join(out, '*.png')) + glob.glob(os.path.join(out, '*.gif')):
        os.remove(old)
    cells = []
    for k, fr in enumerate(frames, 1):
        f = fr.transpose(Image.FLIP_LEFT_RIGHT) if mirror else fr
        cell = Image.new('RGBA', (cw, ch), (0,0,0,0))
        cell.alpha_composite(f, ((cw-f.width)//2, ch-f.height))  # bottom-center
        cell.save(os.path.join(out, f'idle_{dir_name}_{k:02d}.png'))
        cells.append(cell)
    gi = []
    for c in cells:
        cv = checker(cw, ch).copy(); cv.alpha_composite(c)
        gi.append(cv.convert('P', palette=Image.ADAPTIVE))
    gp = os.path.join(FWORK, f'idle_{dir_name}_preview.gif')
    gi[0].save(gp, save_all=True, append_images=gi[1:], duration=GIF_MS, loop=0, disposal=2)
    print(f'idle_{dir_name}: {len(cells)} frames, cell {cw}x{ch} -> {out}')

emit('right', mirror=False)
emit('left',  mirror=True)
