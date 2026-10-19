"""Finaliza o arco: espelha right/downright/upright -> left/downleft/upleft (flipX)
e materializa as 8 direcoes em:
  - Assets/_Game/Resources/PlayerSprites/bow/<dir>/   (runtime, 128 PPU pelo importer)
  - art/animations/Fazendeiro/bow/<dir>/              (organizacao + gif)
Le os frames de tools/aseprite/work/bow/<dir>/frame_*.png (nframes varia por dir)."""
import os, glob
import numpy as np
from PIL import Image

WORK = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\work\bow'
RES  = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\PlayerSprites\bow'
ART  = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\animations\Fazendeiro\bow'
GIF_MS = 110
# Casa o tom do arco com o do IDLE (a aparencia parada que o jogador ve). Ganho por
# canal medido por mediana (idle/bow_work). Ver compute_gains/analyze_tone.
COLOR_GAIN = (1.315, 1.224, 1.0)

def apply_gain(im, g):
    a = np.array(im).astype(float)
    for c in range(3):
        a[:, :, c] = np.clip(a[:, :, c] * g[c], 0, 255)
    return Image.fromarray(a.astype('uint8'), 'RGBA')

PLAN = {
    'down':      ('down', False),
    'up':        ('up', False),
    'right':     ('right', False),
    'downright': ('downright', False),
    'upright':   ('upright', False),
    'left':      ('right', True),
    'downleft':  ('downright', True),
    'upleft':    ('upright', True),
}

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

def clean(d):
    os.makedirs(d, exist_ok=True)
    for old in glob.glob(os.path.join(d, '*')):
        if os.path.basename(old) == '_source.png':  # preserva a master sheet arquivada
            continue
        os.remove(old)

for dst_dir, (src_dir, mirror) in PLAN.items():
    src = sorted(glob.glob(os.path.join(WORK, src_dir, 'frame_*.png')))
    frames = [Image.open(f).convert('RGBA') for f in src]
    if mirror:
        frames = [f.transpose(Image.FLIP_LEFT_RIGHT) for f in frames]
    frames = [apply_gain(f, COLOR_GAIN) for f in frames]  # match tom do walk
    res_out = os.path.join(RES, dst_dir); clean(res_out)
    art_out = os.path.join(ART, dst_dir); clean(art_out)
    for i, fr in enumerate(frames, 1):
        fr.save(os.path.join(res_out, f'bow_{dst_dir}_{i:02d}.png'))
        fr.save(os.path.join(art_out, f'bow_{dst_dir}_{i:02d}.png'))
    gi = [Image.alpha_composite(checker(*f.size), f).convert('P', palette=Image.ADAPTIVE) for f in frames]
    gi[0].save(os.path.join(art_out, f'bow_{dst_dir}.gif'), save_all=True,
               append_images=gi[1:], duration=GIF_MS, loop=0, disposal=2)
    print(f'{dst_dir:10s} <- {src_dir}{" (flipX)" if mirror else ""}: {len(frames)} frames')

print('OK 8 direcoes de arco finalizadas')
