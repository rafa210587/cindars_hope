# Fatia as folhas de expressao geradas no GPT (5 colunas = neutral/happiness/love/disdain/hatred;
# 1 ou 2 linhas = 1 ou 2 personagens) em retratos individuais, removendo rotulo e fundo branco.
# Saida: Assets/_Game/Resources/NpcPortraits/<id>_<expr>.png  (fundo transparente).
#
# CONFIG: por folha (path relativo ao archive), uma lista de char_id por LINHA (None = pular linha).
import os
import numpy as np
from scipy import ndimage
from PIL import Image

ARCHIVE = r'F:\Projetos\Jogos\Cindars_hope\_art_archive\gpt_prompts'
OUT = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\NpcPortraits'
EXPR = ["neutral", "happiness", "love", "disdain", "hatred"]  # ordem das 5 colunas

# nrows = len(lista); cada item = char_id daquela linha (None pula).
SHEETS = {
    # SINGLES (1 linha)
    r'anciao_velorin\5d41d7e2-0083-462e-89a7-997cadeda8a5.png':       ["anciao_velorin"],
    r'brumdar_ferro_quieto\833e3f73-c2f4-4e76-b081-a033feeadc5e.png': ["brumdar_ferro_quieto"],
    r'dagna_rocha_morna\4c392214-48af-4390-9bf9-456701af8776.png':    ["dagna_rocha_morna"],
    r'eiran_valeclaro\0b38bc60-9cdc-48d4-b0af-ef9c9d8e030b.png':      ["eiran_valeclaro"],
    r'liora_canta_rio\e353f3f8-c2e7-4c0c-b066-be8453fc948b.png':      ["liora_canta_rio"],
    r'maelor_cinza\ce6fdd8c-76ee-4aef-aec4-449264506d8d.png':         ["maelor_cinza"],
    r'mirela_dos_lacos\5e1f7253-dff2-4dbf-b016-593ffdcabe7d.png':     ["mirela_dos_lacos"],
    r'nimble_galhobaixo\847716af-64c8-4e6b-bf1c-64891021cc4e.png':    ["nimble_galhobaixo"],
    r'zrix_das_estradas\282d4df9-2f11-4ada-8612-072432ece309.png':    ["zrix_das_estradas"],
    # DUPLAS (2 linhas: [cima, baixo]) — pasta 1a em ordem alfabetica = cima
    r'gruta_panela_funda\01dadd88-a6fe-4616-876d-2460f55e5742.png':   ["gruta_panela_funda", "gurd_carvalho_torto"],
    r'hess_couro_fundo\be46df18-2843-4fa8-94ed-0487707b9ec5.png':     ["hess_couro_fundo", "hund_carvalho_torto"],
    r'mara_vellum\bedf49c6-838f-4311-8167-d3e3277dbd2c.png':          ["mara_vellum", "mella_forno_quente"],
    r'orlan_pouso_curto\1c8980ce-9c2d-40ab-b725-145127a63e01.png':    ["orlan_pouso_curto", "ozzra_fumacazul"],
    r'pip_semente_solta\2c700647-9b07-4b44-84f8-08d4ffd823a2.png':    ["pip_semente_solta", "renko_tres_sorrisos"],
    r'sael_mare_quieta\fe3c6920-f978-4065-958d-ee2ec6b40c17.png':     ["sael_mare_quieta", "savra_escama_verde"],
    r'ser_alaric_veyr\c4424d0f-d219-4307-b4e5-40525d1a2e1c.png':      ["ser_alaric_veyr", "sylveth"],
    r'thalindra_veu_de_lua\883320fc-5381-4404-9c07-0b4fa216d595.png': ["thalindra_veu_de_lua", "yael_noite_mansa"],
    r'tibbet_vela_torta\c4a029d1-da0d-4e08-a085-88a25b8b8603.png':    ["tibbet_vela_torta", "tovin_maos_de_selo"],
}

WHITE = 244  # canal >= WHITE em RGB => branco de fundo


def isolate(cell):
    """cell: PIL RGB do recorte de uma celula. Remove fundo branco (conectado a borda) e o rotulo
    (mantem so o maior componente opaco = o busto). Retorna RGBA recortado no bbox do busto."""
    rgb = np.array(cell.convert("RGB"))
    h, w, _ = rgb.shape
    white = np.all(rgb >= WHITE, axis=2)
    # fundo = branco conectado a alguma borda
    lab, n = ndimage.label(white)
    border = set(np.unique(np.concatenate([lab[0, :], lab[-1, :], lab[:, 0], lab[:, -1]])))
    border.discard(0)
    bg = np.isin(lab, list(border))
    opaque = ~bg
    # maior componente opaco = busto (descarta texto do rotulo, specks)
    lab2, n2 = ndimage.label(opaque)
    if n2 == 0:
        return None
    sizes = ndimage.sum(np.ones_like(lab2), lab2, index=range(1, n2 + 1))
    biggest = int(np.argmax(sizes)) + 1
    mask = lab2 == biggest
    alpha = np.where(mask, 255, 0).astype(np.uint8)
    out = np.dstack([rgb, alpha])
    img = Image.fromarray(out, "RGBA")
    bbox = img.getbbox()
    return img.crop(bbox) if bbox else None


def main():
    os.makedirs(OUT, exist_ok=True)
    total = 0
    for rel, rows in SHEETS.items():
        path = os.path.join(ARCHIVE, rel)
        if not os.path.exists(path):
            print("FOLHA AUSENTE:", rel); continue
        sheet = Image.open(path).convert("RGB")
        W, H = sheet.size
        nrows = len(rows)
        rowh = H / nrows
        colw = W / 5
        for r, char in enumerate(rows):
            if not char:
                continue
            for c, expr in enumerate(EXPR):
                cell = sheet.crop((int(c * colw), int(r * rowh), int((c + 1) * colw), int((r + 1) * rowh)))
                iso = isolate(cell)
                if iso is None:
                    print("  vazio:", char, expr); continue
                outp = os.path.join(OUT, "%s_%s.png" % (char, expr))
                iso.save(outp)
                total += 1
            print("OK", char, "(linha %d de %s)" % (r, rel))
    print("=== %d retratos salvos em %s ===" % (total, OUT))


if __name__ == "__main__":
    main()
