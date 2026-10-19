"""Baka a SEQUENCIA de idle na pasta de Resources: o nucleo calmo (respiracao)
repete N vezes entre cada 'beat' de fidget (virar cabeca / piscar / mao na cabeca).
Le os 10 frames canonicos de art/animations/Fazendeiro/idle{down,up} e escreve
seq_####.png (ordem = nome) em Resources/PlayerSprites/idle/{down,up}.
Tunar: edite CALM / REPS / BEATS abaixo e rode de novo."""
import os, glob
from PIL import Image

ART = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\animations\Fazendeiro'
RES = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\PlayerSprites\idle'
GIF_MS = 500  # casa com _idleFramesPerSecond = 2.0 no animator

# --- AJUSTE AQUI (numeros de frame 1-based, conforme contact sheet) ---
CONFIG = {
    'down': {
        'folder': 'idledown', 'prefix': 'idle_down',
        'calm':  [1, 3, 6, 7, 9, 10],
        'reps':  4,
        'beats': [[2, 4], [5], [8]],   # vira-cabeca, piscada, mao-na-cabeca (alternam)
    },
    'up': {
        'folder': 'idleup', 'prefix': 'idle_up',
        'calm':  [1, 2, 3, 4, 5, 7, 8, 9, 10],
        'reps':  4,
        'beats': [[6]],                # mao na cabeca
    },
}

def checker(w, h, s=8):
    bg = Image.new('RGBA', (w, h), (200,200,200,255)); px = bg.load()
    for y in range(h):
        for x in range(w):
            if (x//s + y//s) % 2 == 0: px[x, y] = (170,170,170,255)
    return bg

def build_sequence(cfg):
    seq = []
    for beat in cfg['beats']:
        seq += cfg['calm'] * cfg['reps']
        seq += beat
    return seq

def emit(dir_name, cfg):
    folder = os.path.join(ART, cfg['folder'])
    frames = {}
    for n in range(1, 11):
        p = os.path.join(folder, f"{cfg['prefix']}_{n:02d}.png")
        if os.path.exists(p):
            frames[n] = Image.open(p).convert('RGBA')
    if not frames:
        print('SEM FRAMES em', folder); return
    cw = max(f.width for f in frames.values()); ch = max(f.height for f in frames.values())

    seq = build_sequence(cfg)
    out = os.path.join(RES, dir_name)
    os.makedirs(out, exist_ok=True)
    for old in glob.glob(os.path.join(out, '*.png')) + glob.glob(os.path.join(out, '*.gif')):
        os.remove(old)

    cells = []
    for i, n in enumerate(seq, 1):
        fr = frames[n]
        cell = Image.new('RGBA', (cw, ch), (0,0,0,0))
        cell.alpha_composite(fr, ((cw-fr.width)//2, ch-fr.height))
        cell.save(os.path.join(out, f'seq_{i:04d}.png'))
        cells.append(cell)

    gi = [Image.alpha_composite(checker(cw, ch), c).convert('P', palette=Image.ADAPTIVE) for c in cells]
    gp = os.path.join(ART, cfg['folder'], f"{cfg['prefix']}_loop.gif")
    gi[0].save(gp, save_all=True, append_images=gi[1:], duration=GIF_MS, loop=0, disposal=2)
    secs = len(seq) * GIF_MS / 1000.0
    print(f'{dir_name}: {len(seq)} frames na sequencia (~{secs:.1f}s/ciclo @ {GIF_MS}ms) -> {out}')
    print(f'  preview: {gp}')

for d, cfg in CONFIG.items():
    emit(d, cfg)
