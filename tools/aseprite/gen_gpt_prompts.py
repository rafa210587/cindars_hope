# Gera, por NPC, uma SUBPASTA no archive contendo o PROMPT (.txt) + a SPRITE (.png) juntos,
# para colar no ChatGPT. O prompt tem hardening pra SEGUIR O ESTILO/LOOK DA SPRITE anexada.
# Saida: _art_archive/gpt_prompts/<id>/<id>.txt + <id>/<id>.png   (+ _INDEX.md).
# Reusa roster + blocos de raca + color-lock de gen_all_npcs e as 5 expressoes de gen_npc_portraits.
#
# Fluxo de uso (manual, no ChatGPT): abra a subpasta do NPC -> arraste o .png na caixa -> cole o .txt -> gere.
# Sai 1 imagem = folha com as 5 expressoes (NEUTRO/FELICIDADE/AMOR/DESDEM/ODIO).
import os, sys, shutil
TOOLS = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite'
OUT = r'F:\Projetos\Jogos\Cindars_hope\_art_archive\gpt_prompts'
SPRITE_SRC = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\NpcSprites'

sys.path.insert(0, TOOLS)
from gen_all_npcs import NPCS, RACE_BLOCK, COLORLOCK  # noqa: E402
from gen_npc_portraits import EXPRESSIONS              # noqa: E402

# Rotulos PT-BR (batem com NpcExpression / a HUD) na ordem fixa do sheet.
ORDER = [("neutral", "NEUTRO"), ("happiness", "FELICIDADE"), ("love", "AMOR"),
         ("disdain", "DESDEM"), ("hatred", "ODIO")]

# HARDENING: amarra identidade E estilo de renderizacao a sprite anexada.
HARD = (
    "Use the ATTACHED pixel sprite as the HARD, binding reference for this character. The five portraits "
    "MUST be unmistakably the SAME character as the sprite. Copy faithfully FROM THE SPRITE: the exact skin "
    "tone, the hair color and hairstyle, the ear and race features, the outfit's colors and design, the "
    "accent color, and the overall color palette. ALSO match the sprite's ART STYLE: the same flat, "
    "cel-shaded PIXEL-ART rendering and the same chunky stocky proportions as the sprite. Do NOT smooth it "
    "out, do NOT add painterly/airbrush shading, do NOT turn it into anime/manga, and do NOT redesign the "
    "outfit or recolor anything that the sprite already defines."
)

STYLE = (
    "Art style: a 2D action-RPG character portrait that matches the attached sprite - 'Children of Morta' "
    "PIXEL-ART look, chunky proportions, clean cel-shading with a few tones per color, a firm selective dark "
    "outline, soft top lighting. This is PIXEL ART, NOT anime and NOT manga: no glossy anime shading, no "
    "smooth airbrush gradients, no big sparkly anime eyes, grounded adult face proportions. No full body, "
    "no legs, no frame, no UI, and no text other than the five labels."
)

# Para um busto, descartamos clausulas de corpo inteiro / objeto-na-mao / escala de pernas/pes.
_DROP = ("legs", "leg ", "feet", "foot", "barefoot", "heads tall", "head tall", "holding",
         "knee-high", "knee high", "at his feet", "at her feet", "sitting at", "waist down",
         "short stub", "long legs")


def bust_clean(text):
    kept = []
    for clause in text.split(", "):
        low = clause.lower()
        if any(tok in low for tok in _DROP):
            continue
        kept.append(clause)
    return ", ".join(kept)


def build(entry):
    nid, race, porte, desc, neg_extra = entry
    character = bust_clean(RACE_BLOCK[race]) + ", " + bust_clean(desc)
    expr_lines = []
    for i, (k, label) in enumerate(ORDER, 1):
        expr_lines.append("%d) %s - %s" % (i, label, EXPRESSIONS[k]))
    return (
        HARD + "\n\n"
        "Generate ONE image: a horizontal sheet of 5 head-and-shoulders BUST portraits (head, shoulders and "
        "upper chest only) of the SAME character, evenly spaced left to right on a plain solid WHITE "
        "background, with a small text label under each. Keep the SAME character, outfit and colors in all "
        "five, but make each one DYNAMIC and expressive: the FACE AND the BODY LANGUAGE change with the "
        "emotion - vary the head angle, the shoulders, the posture (leaning in/back) and the hands/gesture "
        "for each expression. Do not repeat the exact same pose five times.\n\n"
        "Character (must stay consistent with the sprite): " + character + ". " + COLORLOCK + ".\n\n"
        + STYLE + "\n\n"
        "The 5 expressions in THIS order, each with its label under the bust:\n"
        + "\n".join(expr_lines) + "\n"
    )


def main():
    os.makedirs(OUT, exist_ok=True)
    index = ["# Prompts GPT por personagem (busto + 5 expressoes)\n",
             "Cada subpasta tem o prompt (.txt) + a sprite (.png). No ChatGPT: arraste o .png, cole o .txt, gere.\n"]
    made, no_sprite = 0, []
    for entry in NPCS:
        nid = entry[0]
        d = os.path.join(OUT, nid)
        os.makedirs(d, exist_ok=True)
        with open(os.path.join(d, nid + ".txt"), "w", encoding="utf-8") as f:
            f.write(build(entry))
        src = os.path.join(SPRITE_SRC, nid + ".png")
        if os.path.exists(src):
            shutil.copyfile(src, os.path.join(d, nid + ".png"))
        else:
            no_sprite.append(nid)
        index.append("- **%s** -> `gpt_prompts/%s/` (%s.txt + %s.png)" % (nid, nid, nid, nid))
        made += 1
    with open(os.path.join(OUT, "_INDEX.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(index) + "\n")
    print("Subpastas geradas: %d em %s" % (made, OUT))
    if no_sprite:
        print("SEM sprite (.txt criado, faltou copiar .png): " + ", ".join(no_sprite))


if __name__ == "__main__":
    main()
