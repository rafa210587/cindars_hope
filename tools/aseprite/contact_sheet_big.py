# contact_sheet_big.py — paginas ampliadas por categoria p/ revisao de aderencia
# Uso: python contact_sheet_big.py <proc> <prompts.json> <cat> <cols> <rows> <scale>
import json, os, sys
from PIL import Image, ImageDraw
proc, pj, cat = sys.argv[1], sys.argv[2], sys.argv[3]
cols  = int(sys.argv[4]) if len(sys.argv) > 4 else 5
rows  = int(sys.argv[5]) if len(sys.argv) > 5 else 4
scale = int(sys.argv[6]) if len(sys.argv) > 6 else 4
items = json.load(open(pj, encoding="utf-8-sig"))["items"]
ids = [it["id"] for it in items if it["cat"] == cat and os.path.exists(os.path.join(proc, it["id"] + "_64.png"))]
per = cols * rows
cellw = 64 * scale + 14; cellh = 64 * scale + 26
def cell_img(idd):
    im = Image.open(os.path.join(proc, idd + "_64.png")).convert("RGBA")
    return im.resize((im.width * scale, im.height * scale), Image.NEAREST)
page = 0
for start in range(0, len(ids), per):
    page += 1
    chunk = ids[start:start+per]
    r = (len(chunk) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * cellw, r * cellh), (40, 40, 48, 255))
    d = ImageDraw.Draw(sheet)
    for i, idd in enumerate(chunk):
        im = cell_img(idd)
        cx = (i % cols) * cellw; cy = (i // cols) * cellh
        sheet.alpha_composite(im, (cx + (cellw - im.width) // 2, cy + 4))
        d.text((cx + 3, cy + cellh - 18), idd.replace("npc_", ""), fill=(225, 225, 225, 255))
    out = os.path.join(proc, f"_BIG_{cat}_{page}.png")
    sheet.save(out)
    print(out)
