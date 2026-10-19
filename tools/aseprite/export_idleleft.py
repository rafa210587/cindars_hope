from PIL import Image
import glob, os

RIGHT = r'C:\Users\Rafa\Downloads\Sprites\Fazendeiro\idleright'
OUT = r'C:\Users\Rafa\Downloads\Sprites\Fazendeiro\idleleft'

src = sorted(glob.glob(os.path.join(RIGHT, 'idle_right_*.png')))
frames = [Image.open(f).convert('RGBA').transpose(Image.FLIP_LEFT_RIGHT) for f in src]
cw, ch = frames[0].size

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
    fr.save(os.path.join(OUT, 'idle_left_%02d.png' % k))
    cells.append(fr)

gi = []
for c in cells:
    cv = checker(cw, ch).copy(); cv.alpha_composite(c)
    gi.append(cv.convert('P', palette=Image.ADAPTIVE))
gi[0].save(os.path.join(OUT, 'idle_left.gif'), save_all=True, append_images=gi[1:], duration=260, loop=0, disposal=2)
print('idleleft:', len(frames), 'frames (flip de idleright) ->', OUT)
