from PIL import Image
import glob, os

SP = r'C:\Users\Rafa\AppData\Local\Temp\claude\D--Projetos-Jogos-Cindars-hope-cindars-hope\ddc4989e-2c06-4628-b2e5-787bf8ed8ac3\scratchpad'
BASE = r'C:\Users\Rafa\Downloads\Sprites\Fazendeiro'

F = [Image.open(f).convert('RGBA') for f in sorted(glob.glob(os.path.join(SP, 'faz_proc', 'Diagonal_cima_diagonal_direita', 'sl', 'f*.png')))]
seq = [1, 4, 2, 4, 3, 4]   # up-direita, sem espelhar
frames = [F[i] for i in seq]
cw = max(f.width for f in frames); ch = max(f.height for f in frames)

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

def export(folder, prefix, cells):
    od = os.path.join(BASE, folder)
    os.makedirs(od, exist_ok=True)
    for old in glob.glob(os.path.join(od, '*')): os.remove(old)
    for k, c in enumerate(cells, 1):
        c.save(os.path.join(od, '%s_%02d.png' % (prefix, k)))
    gi = []
    for c in cells:
        cv = checker(cw, ch).copy(); cv.alpha_composite(c)
        gi.append(cv.convert('P', palette=Image.ADAPTIVE))
    gi[0].save(os.path.join(od, prefix + '.gif'), save_all=True, append_images=gi[1:], duration=170, loop=0, disposal=2)
    print(folder, '->', len(cells), 'frames')

cells_r = []
for fr in frames:
    cell = Image.new('RGBA', (cw, ch), (0,0,0,0))
    cell.alpha_composite(fr, ((cw-fr.width)//2, ch-fr.height))
    cells_r.append(cell)
export('walkupright', 'walk_upright', cells_r)

cells_l = [c.transpose(Image.FLIP_LEFT_RIGHT) for c in cells_r]
export('walkupleft', 'walk_upleft', cells_l)
print('cell', cw, ch)
