from PIL import Image, ImageDraw, ImageFont
import glob, os

SP = r'C:\Users\Rafa\AppData\Local\Temp\claude\D--Projetos-Jogos-Cindars-hope-cindars-hope\ddc4989e-2c06-4628-b2e5-787bf8ed8ac3\scratchpad'
OUT = r'C:\Users\Rafa\Downloads\Sprites\Fazendeiro\walkup'
REV = r'C:\Users\Rafa\Desktop\cindars_review\fazendeiro_dirs'

def font(sz):
    try: return ImageFont.truetype(r'C:\Windows\Fonts\arialbd.ttf', sz)
    except: return ImageFont.load_default()

F = [Image.open(f).convert('RGBA') for f in sorted(glob.glob(os.path.join(SP, 'faz_proc', 'Diagonal_cima', 'sl', 'f*.png')))]
seq = [(2,False),(0,False),(1,True),(4,True),(1,True),(2,True),(0,True),(1,False),(4,False),(1,False)]
labels = ['f2','f0',"f1'","f4'","f1'","f2'","f0'",'f1','f4','f1']

def get(i, m):
    im = F[i]
    return im.transpose(Image.FLIP_LEFT_RIGHT) if m else im

frames = [get(i, m) for (i, m) in seq]
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
    cell.save(os.path.join(OUT, 'walk_up_%02d.png' % k))
    cells.append(cell)

gi = []
for c in cells:
    cv = checker(cw, ch).copy(); cv.alpha_composite(c)
    gi.append(cv.convert('P', palette=Image.ADAPTIVE))
gi[0].save(os.path.join(OUT, 'walk_up.gif'), save_all=True, append_images=gi[1:], duration=170, loop=0, disposal=2)

scale = 5; pad = 10; cellw = cw*scale + pad*2
out2 = Image.new('RGB', (cellw*len(frames), ch*scale + 72), (28,28,34)); d = ImageDraw.Draw(out2)
d.text((12, 6), 'walkup FINAL - 10 frames', fill=(255,230,80), font=font(22))
for i, (c, lab) in enumerate(zip(cells, labels)):
    big = c.resize((c.width*scale, c.height*scale), Image.NEAREST); x0 = i*cellw
    out2.paste(big, (x0+pad, 34), big)
    mir = seq[i][1]
    col = (120,200,255) if mir else (120,255,140)
    d.text((x0+pad, 34 + ch*scale + 6), '%d:%s' % (i+1, lab), fill=col, font=font(18))
out2.save(os.path.join(REV, '01_walkup_FINAL.png'))
print('cell', cw, ch, '| 10 frames + walk_up.gif ->', OUT)
