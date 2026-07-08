# slice_farm_props.py — fatia as folhas de props da fazenda (geradas no ChatGPT, fundo cinza
# liso, grade) em sprites individuais transparentes. Só PIL+numpy (sem scipy/rembg).
#
# Método: recorta cada célula da grade -> remove o fundo CONECTADO ÀS BORDAS por flood-fill
# (BFS) dentro de uma tolerância da cor de fundo -> apara no bbox -> salva RGBA.
# O cinza INTERNO (pedra/forja/anvil) é preservado porque não toca a borda da célula.
#
# Uso: python slice_farm_props.py
import os
import numpy as np
from collections import deque
from PIL import Image

RAW = os.path.join(os.path.dirname(__file__), "..", "..", "art", "world_gpt", "raw")
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "art", "world_gpt", "ready", "farm_props")

# folha -> (rows, cols, [nomes em ordem row-major])
SHEETS = {
    "gpt_props_sheet1_craft.png": (2, 3, [
        "workbench", "forge", "cooking_hearth",
        "shipping_bin", "sell_stall", "notice_board",
    ]),
    "gpt_props_sheet2_buildings.png": (2, 2, [
        "coop", "barn",
        "cheese_hut", "wine_shed",
    ]),
    "gpt_props_sheet3_special.png": (2, 2, [
        "greenhouse", "cave_entrance",
        "bridge", "anya_fountain",
    ]),
}

TOL = 30       # tolerância de cor p/ considerar "fundo"
INSET = 6      # px removidos da borda de cada célula (evita bleed do vizinho)


def remove_border_bg(cell):
    """cell: PIL RGB. Remove o fundo conectado às bordas (BFS) dentro de TOL da cor mediana
    das bordas. Retorna RGBA aparada no bbox do conteúdo (ou None se vazio)."""
    rgb = np.array(cell.convert("RGB")).astype(np.int16)
    h, w, _ = rgb.shape
    # cor de fundo = mediana dos pixels de borda
    border = np.concatenate([rgb[0, :], rgb[-1, :], rgb[:, 0], rgb[:, -1]], axis=0)
    bg = np.median(border, axis=0)
    dist = np.sqrt(((rgb - bg) ** 2).sum(axis=2))
    bgish = dist < TOL  # candidatos a fundo (inclui cinza interno parecido)

    # BFS a partir de todos os pixels de borda que são bgish -> só remove o fundo CONECTADO
    visited = np.zeros((h, w), dtype=bool)
    dq = deque()
    for x in range(w):
        for y in (0, h - 1):
            if bgish[y, x] and not visited[y, x]:
                visited[y, x] = True; dq.append((y, x))
    for y in range(h):
        for x in (0, w - 1):
            if bgish[y, x] and not visited[y, x]:
                visited[y, x] = True; dq.append((y, x))
    while dq:
        y, x = dq.popleft()
        for ny, nx in ((y-1, x), (y+1, x), (y, x-1), (y, x+1)):
            if 0 <= ny < h and 0 <= nx < w and not visited[ny, nx] and bgish[ny, nx]:
                visited[ny, nx] = True; dq.append((ny, nx))

    alpha = np.where(visited, 0, 255).astype(np.uint8)
    out = np.dstack([rgb.astype(np.uint8), alpha])
    img = Image.fromarray(out, "RGBA")
    bbox = img.getbbox()
    return img.crop(bbox) if bbox else None


def main():
    os.makedirs(OUT, exist_ok=True)
    total = 0
    for fname, (rows, cols, names) in SHEETS.items():
        path = os.path.join(RAW, fname)
        if not os.path.exists(path):
            print("FOLHA AUSENTE:", fname); continue
        sheet = Image.open(path).convert("RGB")
        W, H = sheet.size
        cw, ch = W / cols, H / rows
        idx = 0
        for r in range(rows):
            for c in range(cols):
                if idx >= len(names):
                    continue
                name = names[idx]; idx += 1
                box = (int(c*cw)+INSET, int(r*ch)+INSET,
                       int((c+1)*cw)-INSET, int((r+1)*ch)-INSET)
                cell = sheet.crop(box)
                iso = remove_border_bg(cell)
                if iso is None:
                    print("  vazio:", name); continue
                outp = os.path.join(OUT, name + ".png")
                iso.save(outp)
                print("OK %-16s %s -> %dx%d" % (name, fname, iso.width, iso.height))
                total += 1
    print("=== %d props salvos em %s ===" % (total, os.path.abspath(OUT)))


if __name__ == "__main__":
    main()
