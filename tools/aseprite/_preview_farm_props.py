import os, glob
from PIL import Image, ImageDraw
D = os.path.join(os.path.dirname(__file__), "..", "..", "art", "world_gpt", "ready", "farm_props")
files = sorted(f for f in glob.glob(os.path.join(D, "*.png")) if not os.path.basename(f).startswith("_"))

def checker(w, h, sq=16, c1=(90, 90, 98), c2=(60, 60, 68)):
    bg = Image.new("RGBA", (w, h), c1 + (255,)); d = ImageDraw.Draw(bg)
    for y in range(0, h, sq):
        for x in range(0, w, sq):
            if ((x // sq) + (y // sq)) % 2 == 0:
                d.rectangle([x, y, x + sq - 1, y + sq - 1], fill=c2 + (255,))
    return bg

cell = 340; cols = 4; rows = (len(files) + cols - 1) // cols
sheet = Image.new("RGBA", (cols * cell, rows * cell), (25, 25, 30, 255)); d = ImageDraw.Draw(sheet)
for i, f in enumerate(files):
    im = Image.open(f).convert("RGBA")
    s = min((cell - 30) / im.width, (cell - 30) / im.height)
    im = im.resize((max(1, int(im.width * s)), max(1, int(im.height * s))), Image.NEAREST)
    cx = (i % cols) * cell; cy = (i // cols) * cell
    bg = checker(cell, cell); bg.alpha_composite(im, ((cell - im.width) // 2, (cell - im.height) // 2 - 8))
    sheet.alpha_composite(bg, (cx, cy))
    d.text((cx + 6, cy + cell - 16), os.path.basename(f)[:-4], fill=(240, 240, 120, 255))
out = os.path.join(D, "_PREVIEW.png"); sheet.convert("RGB").save(out)
print("saved", os.path.abspath(out), sheet.size)
