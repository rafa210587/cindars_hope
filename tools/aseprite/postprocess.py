# postprocess.py — transforma a saida crua da IA em sprite pronto:
#   remove fundo (flood fill das bordas) -> recorta (trim) -> downscale nearest -> preview
# Uso: python postprocess.py <in_dir> <out_dir> [alturas separadas por virgula, ex 64,48]
import sys, os, glob
import numpy as np
from scipy import ndimage
from PIL import Image, ImageDraw

# rembg (remocao de fundo por IA) — robusto p/ fundo dithered/texturizado; fallback chroma-key
try:
    from rembg import remove as _rembg_remove, new_session as _rembg_session
    _SESSION = _rembg_session("u2net")
    _HAS_REMBG = True
except Exception as _e:
    _HAS_REMBG = False
    print("rembg indisponivel, usando chroma-key:", _e)

in_dir  = sys.argv[1]
out_dir = sys.argv[2]
heights = [int(x) for x in (sys.argv[3].split(",") if len(sys.argv) > 3 else ["64","48"])]
os.makedirs(out_dir, exist_ok=True)

def remove_bg(img, tol=42):
    if _HAS_REMBG:
        out = _rembg_remove(img.convert("RGBA"), session=_SESSION).convert("RGBA")
        r, g, b, a = out.split()
        a = a.point(lambda v: 255 if v > 120 else 0)  # binariza alpha p/ borda crisp de pixel art
        return Image.merge("RGBA", (r, g, b, a))
    # ---- fallback: chroma-key por cor de borda ----
    rgb = np.array(img.convert("RGB")).astype(np.int16)
    h, w, _ = rgb.shape
    border_px = np.concatenate([rgb[0, :, :], rgb[-1, :, :], rgb[:, 0, :], rgb[:, -1, :]], axis=0)
    bgcol = np.median(border_px, axis=0)
    dist = np.sqrt(((rgb - bgcol) ** 2).sum(axis=2))
    mask = dist < tol
    labels, _ = ndimage.label(mask)
    border = set(np.unique(np.concatenate([labels[0, :], labels[-1, :], labels[:, 0], labels[:, -1]])))
    border.discard(0)
    bgmask = np.isin(labels, list(border))
    alpha = np.where(bgmask, 0, 255).astype(np.uint8)
    return Image.fromarray(np.dstack([rgb.astype(np.uint8), alpha]), "RGBA")

def checker(w, h, c1=(70,70,78), c2=(50,50,56), sq=8):
    bg = Image.new("RGBA", (w, h), c1 + (255,))
    d = ImageDraw.Draw(bg)
    for y in range(0, h, sq):
        for x in range(0, w, sq):
            if ((x // sq) + (y // sq)) % 2 == 0:
                d.rectangle([x, y, x+sq-1, y+sq-1], fill=c2 + (255,))
    return bg

count = 0
previews = []
for f in sorted(glob.glob(os.path.join(in_dir, "*_raw.png"))):
    name = os.path.basename(f).replace("_raw.png", "")
    img = Image.open(f)
    cut = remove_bg(img)
    bbox = cut.getbbox()
    if bbox: cut = cut.crop(bbox)
    cut.save(os.path.join(out_dir, name + "_cut.png"))
    for th in heights:
        nw = max(1, round(cut.width * th / cut.height))
        small = cut.resize((nw, th), Image.NEAREST)
        small.save(os.path.join(out_dir, f"{name}_{th}.png"))
        if th == heights[0]:
            up = small.resize((nw*4, th*4), Image.NEAREST)
            bg = checker(nw*4, th*4)
            bg.alpha_composite(up)
            previews.append((name, bg))
    count += 1

# contact sheet dos previews (4x ampliado, sobre xadrez)
if previews:
    cols = min(6, len(previews))
    cellw = max(p[1].width for p in previews) + 10
    cellh = max(p[1].height for p in previews) + 22
    rows = (len(previews) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols*cellw, rows*cellh), (30,30,36,255))
    d = ImageDraw.Draw(sheet)
    for i,(nm,im) in enumerate(previews):
        cx = (i % cols)*cellw; cy = (i // cols)*cellh
        sheet.alpha_composite(im, (cx + (cellw-im.width)//2, cy + 4))
        d.text((cx+4, cy+cellh-16), nm[:18], fill=(230,230,230,255))
    sheet.save(os.path.join(out_dir, "_PREVIEW.png"))

print(f"processados: {count}")
print(f"saida: {out_dir}")
