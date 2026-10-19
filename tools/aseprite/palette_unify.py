# palette_unify.py — deriva uma paleta-mestra compartilhada de TODOS os sprites e requantiza
# cada um pra ela => coesao de cor entre todos os sprites (mantendo alpha).
# Uso: python palette_unify.py <proc_dir> [ncolors] [sample_ids_csv]
import sys, os, glob, math
import numpy as np
from PIL import Image, ImageDraw

proc    = sys.argv[1]
ncol    = int(sys.argv[2]) if len(sys.argv) > 2 else 48
samples = (sys.argv[3].split(",") if len(sys.argv) > 3 else [])
unidir  = os.path.join(proc, "unified")
os.makedirs(unidir, exist_ok=True)

files = sorted(glob.glob(os.path.join(proc, "*_cut.png")))

# 1) coleta pixels opacos (subamostrados) de todos os sprites p/ a paleta-mestra
samp = []
for f in files:
    a = np.array(Image.open(f).convert("RGBA"))
    mask = a[:, :, 3] > 20
    rgb = a[:, :, :3][mask]
    if len(rgb) > 800:
        idx = np.linspace(0, len(rgb) - 1, 800).astype(int)
        rgb = rgb[idx]
    samp.append(rgb)
allpx = np.concatenate(samp, axis=0).astype(np.uint8)
side = int(math.sqrt(len(allpx))) + 1
comb = np.zeros((side * side, 3), np.uint8)
comb[:len(allpx)] = allpx
comb_img = Image.fromarray(comb.reshape(side, side, 3), "RGB")
pal_img = comb_img.quantize(colors=ncol, method=Image.MEDIANCUT)
print(f"paleta-mestra: {ncol} cores de {len(files)} sprites")

# 2) requantiza cada sprite pra paleta-mestra (preserva alpha)
def unify(path):
    im = Image.open(path).convert("RGBA")
    alpha = im.getchannel("A")
    q = im.convert("RGB").quantize(palette=pal_img, dither=Image.NONE).convert("RGB")
    q.putalpha(alpha)
    return q

for f in files:
    unify(f).save(os.path.join(unidir, os.path.basename(f).replace("_cut", "_uni")))

# 3) demo antes/depois (sobre xadrez, 4x)
def checker(w, h, s=8):
    bg = Image.new("RGBA", (w, h), (60, 60, 68, 255)); d = ImageDraw.Draw(bg)
    for y in range(0, h, s):
        for x in range(0, w, s):
            if ((x // s) + (y // s)) % 2 == 0: d.rectangle([x, y, x+s-1, y+s-1], fill=(44, 44, 50, 255))
    return bg
if samples:
    SC = 4; pad = 8
    cells = []
    for sid in samples:
        cp = os.path.join(proc, sid + "_cut.png")
        if not os.path.exists(cp): continue
        orig = Image.open(cp).convert("RGBA"); uni = unify(cp)
        h = max(orig.height, uni.height)
        def up(im):
            r = im.resize((im.width*SC, im.height*SC), Image.NEAREST); bg = checker(r.width, r.height); bg.alpha_composite(r); return bg
        cells.append((sid, up(orig), up(uni)))
    if cells:
        colw = max(c[1].width for c in cells) + max(c[2].width for c in cells) + pad*3
        roww = max(max(c[1].height, c[2].height) for c in cells) + 24
        sheet = Image.new("RGBA", (colw, roww*len(cells)), (30, 30, 36, 255)); d = ImageDraw.Draw(sheet)
        for i, (sid, o, u) in enumerate(cells):
            y = i*roww
            sheet.alpha_composite(o, (pad, y+18)); sheet.alpha_composite(u, (pad*2 + o.width, y+18))
            d.text((pad, y+4), f"{sid}    [ORIGINAL]              [PALETA UNIFICADA]", fill=(230,230,230,255))
        sheet.save(os.path.join(proc, "_PALETTE_DEMO.png"))
        print("demo salvo: _PALETTE_DEMO.png")
