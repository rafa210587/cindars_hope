from PIL import Image
import glob, os

SP = r'C:\Users\Rafa\AppData\Local\Temp\claude\D--Projetos-Jogos-Cindars-hope-cindars-hope\ddc4989e-2c06-4628-b2e5-787bf8ed8ac3\scratchpad'
OUT = r'C:\Users\Rafa\Downloads\Sprites\Fazendeiro\idleright'

src = sorted(glob.glob(os.path.join(SP, 'idle_proc', 'sl', 'row00_f*.png')))
frames = [Image.open(f).convert('RGBA') for f in src]
cw = max(f.width for f in frames); ch = max(f.height for f in frames)

os.makedirs(OUT, exist_ok=True)
for old in glob.glob(os.path.join(OUT, '*')): os.remove(old)

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

cells = []
for k, fr in enumerate(frames, 1):
    cell = Image.new('RGBA', (cw, ch), (0,0,0,0))
    cell.alpha_composite(fr, ((cw-fr.width)//2, ch-fr.height))
    cell.save(os.path.join(OUT, 'idle_right_%02d.png' % k))
    cells.append(cell)

gi = []
for c in cells:
    cv = checker(cw, ch).copy(); cv.alpha_composite(c)
    gi.append(cv.convert('P', palette=Image.ADAPTIVE))
gi[0].save(os.path.join(OUT, 'idle_right.gif'), save_all=True, append_images=gi[1:], duration=260, loop=0, disposal=2)
print('idleright:', len(cells), 'frames, cell', cw, ch, '->', OUT)
