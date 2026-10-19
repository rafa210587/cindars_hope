# contact_sheet.py — monta contact sheets por categoria a partir dos _64.png processados
import json, os, sys
from PIL import Image, ImageDraw
proc = sys.argv[1]                      # pasta processed
pj   = sys.argv[2]                       # prompts.json
items = json.load(open(pj, encoding="utf-8-sig"))["items"]
order = {"monster": [], "npc": [], "world": []}
for it in items:
    if os.path.exists(os.path.join(proc, it["id"] + "_64.png")):
        order[it["cat"]].append(it["id"])
SCALE = 2
def build(cat, cols):
    ids = order[cat]
    if not ids: return
    cellw = 64 * SCALE + 10; cellh = 64 * SCALE + 22
    rows = (len(ids) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * cellw, rows * cellh), (34, 34, 40, 255))
    d = ImageDraw.Draw(sheet)
    for i, idd in enumerate(ids):
        im = Image.open(os.path.join(proc, idd + "_64.png")).convert("RGBA")
        im = im.resize((im.width * SCALE, im.height * SCALE), Image.NEAREST)
        cx = (i % cols) * cellw; cy = (i // cols) * cellh
        sheet.alpha_composite(im, (cx + (cellw - im.width) // 2, cy + 2))
        d.text((cx + 2, cy + cellh - 14), idd.replace("npc_", "")[:16], fill=(220, 220, 220, 255))
    sheet.save(os.path.join(proc, f"_SHEET_{cat}.png"))
    print(f"{cat}: {len(ids)} -> _SHEET_{cat}.png")
build("monster", 10); build("npc", 8); build("world", 10)
