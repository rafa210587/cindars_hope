# recolor_liora_walk.py — corrige a paleta ERRADA da folha de caminhada 5x5 da Liora
# (manto/veu saiu teal/verde-petroleo) remapeando por HUE para o roxo/lavanda correto,
# usando a folha antiga (gpt_liora_walk_v1_purplestatic_old.png) como referencia de
# identidade (roxo com borda dourada, robe creme, vestido azul claro).
#
# v3 (passe de refinamento): a v2 (faixa 150-210, sat>=0.12, delta fixo +102.5) deixou
# residuos verdes — mancha "camuflagem" no capuz (frame r3c1) e mesclas verdes nas dobras
# de costas (fileiras 4-5). Amostragem mostrou que esses residuos vivem em hue 60-150
# com sat ~0.2-0.3, mais um resto de 150-210 com sat 0.06-0.12 que escapou pelo piso.
# Um delta fixo nao serve para hue ~90 (cairia em azul), entao o v3 usa remap POR PARTES:
#   - banda 150-210 (teal principal): delta fixo +102.5 -> hue 252.5-312.5 (igual v2,
#     preserva o visual ja auditado do manto);
#   - banda 60-150 (verdes/oliva dessaturados residuais): map linear 150->252.5 ate
#     60->279.5 (continuo na juncao, tudo cai na familia do roxo);
#   - piso de saturacao reduzido para 0.06; piso de valor 0.08 (poupa contorno preto).
# Protegidos por construcao: hue 10-60 (pele, cabelo castanho, dourado da harpa/bordas),
# hue < 60 em geral, pixels quase-cinza (sat < 0.06) e transparencia (alpha intacto).
# S e V originais sao preservados pixel a pixel — sombras/highlights da animacao intactos.
#
# Fonte: gpt_liora_walk_teal_backup.png (folha errada original, limpa).
# Saida: gpt_liora_walk_v3_purple.png — NUNCA sobrescreve gpt_liora_walk.png.
#
# Uso: python recolor_liora_walk.py

import numpy as np
from PIL import Image, ImageDraw
import os

BASE = r"F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\npc_anim_gpt\raw"
WRONG_PATH = os.path.join(BASE, "gpt_liora_walk_teal_backup.png")   # fonte teal limpa
REF_PATH = os.path.join(BASE, "gpt_liora_walk_v1_purplestatic_old.png")
OUT_PATH = os.path.join(BASE, "gpt_liora_walk_v3_purple.png")
AUDIT_PATH = os.path.join(BASE, "audit_liora_recolor.png")

# Faixas medidas por amostragem:
#   teal principal do manto: pico 160-200, media ~176.6 | roxo de referencia: media ~279.1
#   residuos verdes (capuz r3c1, dobras r4-r5): hue 60-150, sat ~0.2-0.3
TEAL_LO, TEAL_HI = 150.0, 210.0
GREEN_LO = 60.0                      # inicio da banda residual verde/oliva
TEAL_DELTA = 279.1 - 176.6           # +102.5 (igual v2)
GREEN_OUT_AT_LO = 279.5              # hue 60  -> 279.5 (roxo)
GREEN_OUT_AT_HI = TEAL_LO + TEAL_DELTA  # hue 150 -> 252.5 (continuo com a banda teal)

MIN_SAT = 0.06
MIN_VAL = 0.08


def rgb_to_hsv(rgb01):
    r, g, b = rgb01[..., 0], rgb01[..., 1], rgb01[..., 2]
    maxc = np.maximum(np.maximum(r, g), b)
    minc = np.minimum(np.minimum(r, g), b)
    v = maxc
    delta = maxc - minc
    s = np.where(maxc <= 1e-6, 0.0, delta / np.where(maxc <= 1e-6, 1.0, maxc))
    h = np.zeros_like(r)
    nz = delta > 1e-6
    rc = np.zeros_like(r)
    gc = np.zeros_like(r)
    bc = np.zeros_like(r)
    rc[nz] = (maxc[nz] - r[nz]) / delta[nz]
    gc[nz] = (maxc[nz] - g[nz]) / delta[nz]
    bc[nz] = (maxc[nz] - b[nz]) / delta[nz]
    is_r = (maxc == r) & nz
    is_g = (maxc == g) & nz & ~is_r
    is_b = (maxc == b) & nz & ~is_r & ~is_g
    h[is_r] = (bc[is_r] - gc[is_r])
    h[is_g] = 2.0 + (rc[is_g] - bc[is_g])
    h[is_b] = 4.0 + (gc[is_b] - rc[is_b])
    h = (h / 6.0) % 1.0
    return h, s, v


def hsv_to_rgb(h, s, v):
    i = np.floor(h * 6.0).astype(np.int32)
    f = h * 6.0 - i
    p = v * (1.0 - s)
    q = v * (1.0 - f * s)
    t = v * (1.0 - (1.0 - f) * s)
    i = i % 6
    r = np.select([i == 0, i == 1, i == 2, i == 3, i == 4, i == 5], [v, q, p, p, t, v])
    g = np.select([i == 0, i == 1, i == 2, i == 3, i == 4, i == 5], [t, v, v, q, p, p])
    b = np.select([i == 0, i == 1, i == 2, i == 3, i == 4, i == 5], [p, p, t, v, v, q])
    return np.stack([r, g, b], axis=-1)


def frame_box(w, h, row, col):
    fw, fh = w // 5, h // 5
    return col * fw, row * fh, (col + 1) * fw, (row + 1) * fh


def sample_patch(hue_deg, sat, val, alpha, w, h, row, col, label):
    """Reporta os pixels verdes residuais (hue 60-150) de um frame — evidencia antes/depois."""
    x0, y0, x1, y1 = frame_box(w, h, row, col)
    fh_ = hue_deg[y0:y1, x0:x1]
    fs_ = sat[y0:y1, x0:x1]
    fv_ = val[y0:y1, x0:x1]
    fa_ = alpha[y0:y1, x0:x1]
    sel = (fa_ > 10) & (fh_ >= GREEN_LO) & (fh_ < TEAL_LO) & (fs_ >= MIN_SAT) & (fv_ >= MIN_VAL)
    if sel.sum():
        print(f"  [{label}] r{row+1}c{col+1}: {int(sel.sum())} px verdes residuais (hue 60-150), "
              f"hue medio {fh_[sel].mean():.1f}, sat {fs_[sel].mean():.2f}, val {fv_[sel].mean():.2f}")
    else:
        print(f"  [{label}] r{row+1}c{col+1}: 0 px na banda verde 60-150")
    return int(sel.sum())


def main():
    wrong_im = Image.open(WRONG_PATH).convert("RGBA")
    wrong = np.array(wrong_im).astype(np.float32)
    h, w = wrong.shape[0], wrong.shape[1]

    rgb01 = wrong[:, :, :3] / 255.0
    alpha = wrong[:, :, 3]
    hue, sat, val = rgb_to_hsv(rgb01)
    hue_deg = hue * 360.0

    base = (sat >= MIN_SAT) & (val >= MIN_VAL) & (alpha > 10)
    teal_mask = base & (hue_deg >= TEAL_LO) & (hue_deg <= TEAL_HI)
    green_mask = base & (hue_deg >= GREEN_LO) & (hue_deg < TEAL_LO)
    full_mask = teal_mask | green_mask

    print("Amostra ANTES (fonte teal backup) — pixels da mancha do capuz e dobras:")
    sample_patch(hue_deg, sat, val, alpha, w, h, 2, 0, "antes")
    sample_patch(hue_deg, sat, val, alpha, w, h, 3, 2, "antes")
    sample_patch(hue_deg, sat, val, alpha, w, h, 4, 2, "antes")

    new_hue_deg = hue_deg.copy()
    # banda teal principal: delta fixo (identico ao v2)
    new_hue_deg[teal_mask] = (hue_deg[teal_mask] + TEAL_DELTA) % 360.0
    # banda verde residual: map linear 60->279.5 ... 150->252.5 (continuo na juncao)
    gh = hue_deg[green_mask]
    t = (gh - GREEN_LO) / (TEAL_LO - GREEN_LO)
    new_hue_deg[green_mask] = GREEN_OUT_AT_LO + t * (GREEN_OUT_AT_HI - GREEN_OUT_AT_LO)

    new_rgb01 = hsv_to_rgb((new_hue_deg / 360.0) % 1.0, sat, val)
    out_rgb01 = np.where(full_mask[..., None], new_rgb01, rgb01)
    out_rgb = np.clip(out_rgb01 * 255.0, 0, 255).astype(np.uint8)
    out = np.dstack([out_rgb, alpha.astype(np.uint8)])
    out_im = Image.fromarray(out, "RGBA")
    out_im.save(OUT_PATH)

    n_opaque = int((alpha > 10).sum())
    n_teal = int(teal_mask.sum())
    n_green = int(green_mask.sum())
    n_total = int(full_mask.sum())

    # evidencia DEPOIS: reamostra o resultado salvo nos mesmos frames
    out_arr = np.array(out_im).astype(np.float32)
    oh, os_, ov = rgb_to_hsv(out_arr[:, :, :3] / 255.0)
    oh_deg = oh * 360.0
    print("Amostra DEPOIS (v3 salvo) — mesma banda verde 60-150 deve estar ~zerada:")
    sample_patch(oh_deg, os_, ov, out_arr[:, :, 3], w, h, 2, 0, "depois")
    sample_patch(oh_deg, os_, ov, out_arr[:, :, 3], w, h, 3, 2, "depois")
    sample_patch(oh_deg, os_, ov, out_arr[:, :, 3], w, h, 4, 2, "depois")

    print(f"\nEntrada (teal backup): {WRONG_PATH}  {wrong_im.size}")
    print(f"Saida:                 {OUT_PATH}  {out_im.size}")
    print(f"Alpha preservado: {np.array_equal(np.array(out_im)[:, :, 3], np.array(wrong_im)[:, :, 3])}")
    print(f"Bandas: teal [{TEAL_LO}-{TEAL_HI}] delta {TEAL_DELTA:+.1f} | verde residual [{GREEN_LO}-{TEAL_LO}) -> [{GREEN_OUT_AT_HI:.1f}-{GREEN_OUT_AT_LO:.1f}] linear")
    print(f"Pisos: sat >= {MIN_SAT}, val >= {MIN_VAL} | protegidos: hue < {GREEN_LO} (pele/cabelo/dourado), sat < {MIN_SAT}")
    print(f"Pixels opacos: {n_opaque}")
    print(f"Remapeados: teal={n_teal}  verde-residual={n_green}  total={n_total} ({100.0*n_total/n_opaque:.1f}%)")

    # ---- auditoria: 3 frames (f1, r3c1, r4c3) x 3 folhas (errado, v3, referencia) ----
    ref_im = Image.open(REF_PATH).convert("RGBA")
    frames = [(0, 0, "frame 1 (r1c1)"), (2, 0, "capuz (r3c1)"), (3, 2, "costas (r4c3)")]
    SC = 2
    pad = 12
    label_h = 18
    fw, fh = w // 5, h // 5

    def crop_up(im, row, col):
        iw, ih = im.size
        cfw, cfh = iw // 5, ih // 5
        c = im.crop((col * cfw, row * cfh, (col + 1) * cfw, (row + 1) * cfh))
        return c.resize((c.width * SC, c.height * SC), Image.NEAREST)

    cell_w = fw * SC + pad
    cell_h = fh * SC + pad + label_h
    sheet = Image.new("RGBA", (cell_w * 3 + pad, cell_h * len(frames) + pad + label_h), (40, 40, 46, 255))
    d = ImageDraw.Draw(sheet)
    col_labels = ["ERRADO (teal)", "RECOLORIDO (v3)", "REFERENCIA (roxo)"]
    for ci, cl in enumerate(col_labels):
        d.text((pad + ci * cell_w, pad), cl, fill=(230, 230, 230, 255))
    for fi, (row, col, flabel) in enumerate(frames):
        y = pad + label_h + fi * cell_h
        d.text((pad, y), flabel, fill=(180, 200, 255, 255))
        for ci, im in enumerate([wrong_im, out_im, ref_im]):
            sheet.alpha_composite(crop_up(im, row, col), (pad + ci * cell_w, y + label_h))
    sheet.save(AUDIT_PATH)
    print(f"Auditoria salva: {AUDIT_PATH}  {sheet.size}")


if __name__ == "__main__":
    main()
