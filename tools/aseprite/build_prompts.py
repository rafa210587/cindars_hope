# build_prompts.py — gera tools/aseprite/prompts.json (EN, detalhado) a partir dos manifestos,
# codificando a skill pixel-art-prompt-authoring (ancora D&D + cor explicita + equipamento + tema Vaalara).
# Uso: python build_prompts.py
import json, os

BASE = os.path.dirname(os.path.abspath(__file__))
def load(n):
    with open(os.path.join(BASE, n), encoding="utf-8-sig") as f: return json.load(f)
mon = load("manifest.monsters.json")["sprites"]
npc = load("manifest.npcs.json")["sprites"]
wld = load("manifest.world.json")["props"]
OVR = load("monster_overrides.json")["overrides"]

# Estilo-alvo: "Children of Morta leve" (detalhado, volume, proporcao levemente estilizada,
# outline sutil) — NAO o flat cel-shaded jrpg antigo. Travado em 2026-06-28 apos matriz de estilo.
# LoRA pixel-art-xl NAO entra aqui (e parametro de geracao): humanoides=1.0, monstros=0.8.
STYLE = ("detailed pixel art, 2d action rpg game sprite, Children of Morta art style, "
         "clean defined cel-shading with few tones per color, soft neutral top lighting, "
         "firm selective dark outline, cohesive readable silhouette, hand-crafted spritework, "
         "full body, centered, single subject only, one character only, "
         "on a plain pure white background, no shadow, no ground, no floor, no antialiasing")
NEG_FIX = ("inconsistent style, photorealistic, realistic photograph, 3d render, extra figures, duplicate character, small extra characters, "
           "character reference sheet, model sheet, multiple views, character turnaround, multiple poses, "
           "cropped, cut off, bust, portrait, upper body only, half body, headshot, close-up, legs cut off, "
           "cast shadow, ground shadow, drop shadow under feet, floor, gray floor, pedestal, "
           "floating weapons, extra weapons, multiple weapons, extra objects, item icons, "
           "multiple characters, two characters, multiple subjects, text, letters, watermark, signature, "
           "scenery, landscape, room, floor tiles, wall, brick wall, frame, border, user interface, hud, "
           "health bar, game screenshot, blurry, antialiasing, gradient background, "
           "flat plain shading, flat colors, thick heavy black outline, super deformed cartoon, "
           "drop shadow, extra limbs, deformed, mutated")

# Ancora de PROPORCAO do heroi (fazendeiro): cabeca levemente grande, pernas curtas, atarracado ~4 cabecas.
# Aplicada a HUMANOIDES (NPCs, player). O ControlNet (esqueleto compacto) e o lock real; isto so reforca.
PROPORTION = ("compact heroic action-rpg proportions, slightly large head, short sturdy legs, "
              "stocky grounded build, roughly 4 heads tall stylized, same body proportions as the village hero")
NEG_PROP = ("tall lanky body, long legs, realistic seven heads tall proportions, elongated figure, "
            "small head, supermodel proportions, chibi, super deformed baby proportions")

NAMED = [
    ("black",(20,20,24)),("dark gray",(60,60,68)),("gray",(120,120,128)),("light gray",(200,200,205)),
    ("white",(240,240,240)),("dark red",(110,30,20)),("red",(190,40,30)),("crimson",(150,20,40)),
    ("orange",(220,110,40)),("brown",(120,80,45)),("dark brown",(70,45,25)),("tan",(190,150,100)),
    ("beige",(220,205,170)),("gold",(200,160,50)),("yellow",(220,200,60)),("olive",(110,120,60)),
    ("dark green",(40,90,45)),("green",(90,150,80)),("lime green",(120,190,80)),("teal",(50,130,120)),
    ("blue",(60,110,200)),("dark blue",(40,60,110)),("navy",(30,40,70)),("light blue",(140,190,225)),
    ("purple",(110,60,160)),("violet",(150,100,200)),("magenta",(190,60,150)),("pink",(220,150,170)),
    ("green-gray",(110,130,110)),("ash gray",(150,140,135)),
]
def cname(hx):
    hx=hx.lstrip("#"); r,g,b=int(hx[0:2],16),int(hx[2:4],16),int(hx[4:6],16)
    return min(NAMED,key=lambda c:(r-c[1][0])**2+(g-c[1][1])**2+(b-c[1][2])**2)[0]

# ---- monstros: ancora D&D por keyword no id (ordem importa) ----
ANCHOR = [
    ("draconic_elder","ancient dragon elder, wise draconic patriarch","scales"),
    ("draconic","corrupted dragon, draconic spawn","scales"),
    ("wyvern","wyvern, winged dragon","scales"),
    ("ninrorin","void-touched gray elf, githyanki-like ascetic","skin"),
    ("lich","lich, ancient undead sorcerer","robe"),
    ("bone","skeleton knight, armored undead","bone"),
    ("cracked","broken skeleton, undead","bone"),
    ("abyssal","abyssal aberration, eldritch void horror","body"),
    ("void_reaver","void reaver aberration, eldritch horror","body"),
    ("void","void aberration, eldritch horror","body"),
    ("shadow_sentinel","shadow sentinel, animated darkness guardian","body"),
    ("beholder","beholder, floating eye tyrant","body"),
    ("eye_tyrant","beholder, floating eye tyrant","body"),
    ("mimic","mimic, monstrous chest with teeth and tongue","body"),
    ("owlbear","owlbear, bear with owl head","fur"),
    ("bulette","bulette, armored burrowing monster","plates"),
    ("basilisk","basilisk, reptilian monster","scales"),
    ("orc","orc barbarian warrior","skin"),
    ("goblin","goblin scavenger","skin"),
    ("kobold","kobold, small reptilian","scales"),
    ("drow","drow dark elf","skin"),
    ("duergar","gray dwarf duergar","skin"),
    ("gnom","gnome tinkerer","skin"),
    ("clockwork","clockwork construct, mechanical golem","metal"),
    ("puzzle","puzzle golem, magical construct","stone"),
    ("golem","stone golem construct","stone"),
    ("sentinel","animated sentinel construct","metal"),
    ("knight","armored knight","armor"),
    ("sealed","sealed empty armor, animated knight","armor"),
    ("warden","armored warden construct","armor"),
    ("furnace","furnace construct, forge golem","metal"),
    ("cultist","robed cultist","robe"),
    ("acolyte","robed acolyte cultist","robe"),
    ("slime","gelatinous slime ooze","body"),
    ("sludge","translucent gelatinous cube ooze","body"),
    ("cube","gelatinous cube ooze","body"),
    ("jelly","brain jelly ooze","body"),
    ("devourer_slime","devouring ooze slime","body"),
    ("bat","giant cave bat","fur"),
    ("rat","dire rat","fur"),
    ("mite","tiny mite vermin, small insect","carapace"),
    ("tick","engorged tick, small insect","carapace"),
    ("beetle","giant beetle insect","carapace"),
    ("moth","giant moth","wings"),
    ("wisp","floating magical wisp, glowing mote","body"),
    ("rune_shard","floating rune shard, magical fragment","body"),
    ("shard","floating magical shard","body"),
    ("spore","spore imp, fungal creature","body"),
    ("myco","fungal bulwark, mushroom creature","body"),
    ("mossling","mossy fungal creature","body"),
    ("blackroot","animated black root, plant monster","bark"),
    ("rootsnare","animated root claw, plant monster","bark"),
    ("thorn","thorn archer, plant creature with bow","bark"),
    ("stagling","hollow stag, undead deer","fur"),
    ("gnawer","feral frost rodent beast","fur"),
    ("leaper","crystalline leaping creature","crystal"),
    ("crawler","crawling beast","hide"),
    ("panther","distorted panther, shadow beast","fur"),
    ("hound","feral hound, dark dog","fur"),
    ("stalker","stalking predator beast","hide"),
    ("wailer","wailing wraith, undead spirit","robe"),
    ("oathless","oathless shade, ghostly figure","robe"),
    ("oath_eater","oath eater aberration","body"),
    ("hulk","umber hulk, massive burrowing monster","plates"),
    ("corrupt_hulk","corrupted flesh hulk, mutated giant","body"),
    ("mirror","mirror adept, reflective humanoid","body"),
    ("mana_warped","mana-warped beast, mutated creature","hide"),
    ("anya","corrupted luminous echo spirit, sorrowful","body"),
]
def anchor(idd):
    for kw,a,part in ANCHOR:
        if kw in idd: return a,part
    return "fantasy monster","body"

FORM = {"humanoid":"single full-body creature","flying":"single winged flying creature",
        "blob":"single amorphous blob creature","quadruped":"single four-legged beast",
        "insect":"single small insect creature","dragon":"single winged dragon"}

def theme(idd):
    t=lambda *k: any(x in idd for x in k)
    if t("kaand","ember","lava","cinder","scorched","furnace","ash","fire"): return "fire and ember motifs, glowing hot cracks"
    if t("frost","ice","duergar","cold","glass","crystal","gnawer","wailer"): return "icy frost, pale blue glow, frozen"
    if t("nyx","moonless","drow","moon","night"): return "dark lunar shadow, glowing purple runes"
    if t("blackstone","corrupt","void","abyssal","ninrorin","lich","core","oath","shadow"): return "black stone corruption, purple veins, eldritch aura"
    if t("rune","clockwork","gnom","sealed","mirror","puzzle","golem","clock"): return "ancient bromecian runes, weathered metal plates"
    if t("spore","myco","moss","root","thorn","blackroot","stagling"): return "underground forest, moss roots and fungus"
    return ""

# ---- papel de combate por criatura: define POSE (pronto-pra-combate) + arma correta + ----
# ---- negatives anti-acao-errada (melee nao brilha, fera nao segura arma, etc). Ordem importa. ----
def role(idd):
    # caster primeiro (robe/magia); senao 'tinker'/'rune' cairiam em melee
    if any(x in idd for x in ("cultist","acolyte","caster","ashcaller","oracle","adept","madcap",
                              "tinker","lich","wisp","rune","shard","sorcer","mage","warlock","mirror")):
        return "caster"
    if any(x in idd for x in ("archer","thorn","scout","spitter","web","sling")):
        return "ranged"
    if any(x in idd for x in ("orc","knight","bone","cracked","berserker","kaand","goblin","drow",
                              "duergar","gnom","sentinel","warden","guard","reaver","brute","warrior",
                              "sealed","skeleton","hulk","clockwork","furnace","golem")):
        return "melee"
    return "natural"  # feras, oozes, insetos, plantas, espiritos sem arma

COMBAT = {
    "melee":   "in an aggressive combat-ready stance, crouched and leaning forward, gripping a melee weapon in one hand, ready to strike",
    "ranged":  "in a combat-ready stance, drawing a bow with an arrow nocked, taking aim at the enemy",
    "caster":  "in a spellcasting combat stance, channeling glowing magical energy in one raised hand, arcane aura",
    "natural": "in an aggressive predatory stance, body low and tense, snarling, ready to lunge and attack",
}
# variante p/ criaturas CURADAS (OVR ja descreve a arma): nao nomeia arco/cajado pra
# nao contradizer a descricao (ex.: kobold com lanca nao deve "drawing a bow").
COMBAT_OVR = {
    "melee":   "in an aggressive combat-ready stance, crouched and leaning forward, weapon gripped, ready to strike",
    "ranged":  "in an alert combat-ready stance, weapon readied, taking aim at the enemy",
    "caster":  "in a spellcasting combat stance, channeling glowing magical energy in one raised hand, arcane aura",
    "natural": "in an aggressive predatory stance, body low and tense, snarling, ready to lunge and attack",
}
# o que CADA papel NAO pode mostrar (evita o goblin de espada parecendo lancar magia, etc)
ROLE_NEG = {
    "melee":   "casting spell, magic glow, glowing hands, spell effect, energy ball, magic aura, holding a bow, ",
    "ranged":  "casting spell, magic glow, glowing hands, spell effect, energy ball, magic aura, ",
    "caster":  "",
    "natural": "weapon, sword, axe, mace, bow, staff, casting spell, magic glow, glowing hands, holding an object, ",
}

items=[]
for m in mon:
    form = FORM.get(m["bodytype"],"single full-body creature")
    r = role(m["id"])
    if m["id"] in OVR:
        # descricao curada (D&D-accurate) tem prioridade sobre a logica automatica
        pos = ", ".join([OVR[m["id"]], COMBAT_OVR[r], "facing forward, single subject, dungeons and dragons fantasy", STYLE])
        neg = ROLE_NEG[r] + "human skin, pale human skin, " + NEG_FIX
    else:
        anc,part = anchor(m["id"])
        col = cname(m["body"]); acc = cname(m["accent"])
        th = theme(m["id"])
        parts = [anc, f"{col} {part}", (f"{acc} accents" if acc!=col else "")]
        parts.append(COMBAT[r])
        if th: parts.append(th)
        parts += [form, "facing forward, one enemy only, dungeons and dragons fantasy monster", STYLE]
        pos = ", ".join([p for p in parts if p])
        neg = ROLE_NEG[r] + NEG_FIX
        if part=="skin": neg = "human skin, pale skin, " + neg
    items.append({"id":m["id"],"cat":"monster","w":m["w"],"h":m["h"],"body":m["body"],"accent":m["accent"],"positive":pos,"negative":neg})

# ---- NPCs ----
GENDER={"npc_corvus":"male","npc_mara":"female","npc_sylveth":"female","npc_brumdar":"male","npc_nimble":"male",
"npc_gurd":"male","npc_hund":"male","npc_ozzra":"female","npc_gruta":"female","npc_zrix":"male","npc_yael":"female",
"npc_thalindra":"female","npc_dagna":"female","npc_pip":"male","npc_alaric":"male","npc_mirela":"female",
"npc_renko":"male","npc_eiran":"male","npc_liora":"female","npc_orlan":"male","npc_savra":"female","npc_tovin":"male",
"npc_maelor":"male","npc_velorin":"male","npc_sael":"male","npc_mella":"female","npc_hess":"male","npc_tibbet":"male"}
RACE=[("tiefling","tiefling with small horns"),("halfling","halfling"),("gnome","gnome"),("dwarf","dwarf"),
("dwarven","dwarf"),("orc","half-orc"),("draconato","dragonborn"),("draconata","dragonborn"),("elf","elf"),
("goblin","goblin"),("human","human")]
ROLE={"warrior":"armored town guard warrior with sword","mage":"robed cleric mage holding a staff",
"rogue":"hooded rogue merchant with dagger","brute":"large muscular brute laborer",
"commoner":"villager in simple work clothes","elder":"elderly village elder leaning on a wooden staff",
"child":"young child, big head small body","beast":"small animal"}
# overrides de NPC (descricao curada tem prioridade)
NPC_DESC={
 "npc_zrix":"male dragonborn, copper-gray scaled reptilian dragon head with short horns and a snout, scaled muscular body, traveler hooded cloak and explorer gear, dungeons and dragons dragonborn",
 "race_human":"young human male villager, light brown hair, plain linen tunic, brown trousers, leather belt and boots, friendly fantasy peasant face",
}
def npc_race(idd,prompt):
    for kw,r in RACE:
        if kw in idd or kw in prompt.lower(): return r
    return ""
for n in npc:
    g = GENDER.get(n["id"], "")
    arche = n.get("archetype","commoner")
    role = ROLE.get(arche,"villager")
    skin = cname(n["body"]); clo = cname(n["accent"]); hair = cname(n.get("hair","3A2A1A"))
    if n["id"] in NPC_DESC: pos=", ".join([NPC_DESC[n["id"]], "single full-body character, facing forward, one person only, fantasy rpg npc", STYLE])
    elif n["id"]=="pet_cat": pos=f"cute pixel cat, {skin} colored fur, single animal, sitting, {STYLE}"
    elif n["id"]=="pet_dog": pos=f"cute pixel dog, {skin} colored fur, single animal, {STYLE}"
    elif n["id"].startswith("race_"):
        race = n["id"].replace("race_","")
        pos=f"fantasy {race} villager, {skin} skin, {clo} colored clothes, single full-body character, facing forward, rpg npc, {STYLE}"
    else:
        race = npc_race(n["id"], n.get("prompt",""))
        desc = (n.get("prompt","") or "").split(".")[0]
        # usa a descricao original (EN nos npcs) + reforco de genero/raca/cor
        pos=", ".join([p for p in [f"{g} {race} {role}".strip(), f"{skin} skin", f"{clo} colored outfit",
            f"{hair} hair", "single full-body character, facing forward, one person only, fantasy rpg npc", STYLE] if p])
    is_pet = n["id"] in ("pet_cat","pet_dog")
    fpos = pos if is_pet else PROPORTION + ", " + pos
    fneg = NEG_FIX if is_pet else NEG_PROP + ", " + NEG_FIX
    items.append({"id":n["id"],"cat":"npc","w":n["w"],"h":n["h"],"body":n["body"],"accent":n["accent"],"positive":fpos,"negative":fneg})

# ---- World ----
def wdesc(x):
    k=x["kind"]; c1=cname(x["c1"]); c2=cname(x["c2"]); idw=x["id"].replace("_"," ")
    if k=="tree": return f"single fantasy tree, {c1} leafy canopy, {c2} wooden trunk with roots"
    if k=="barrel": return f"single wooden barrel, {c1} wood with {c2} iron hoops"
    if k=="crate": return f"single wooden crate, {c1} planks"
    if k=="ore_node": return f"single rock with embedded {c2} ore gems, gray stone"
    if k=="crystal": return f"single {c1} crystal cluster, glowing"
    if k=="rock": return f"single gray boulder rock"
    if k=="fence": return f"single wooden fence segment, {c1} wood"
    if k=="sign": return f"single wooden sign post, {c1} wood"
    if k=="building": return f"{idw} building exterior, fantasy medieval stone and wood architecture, {c1} walls"
    if k=="crop": return f"single {idw} crop plant, {c1} green leaves"
    if k=="animal": return f"single cute farm {idw}, pixel animal"
    if k=="tile": return f"{idw} ground texture, {c1}, top-down tileable terrain"
    return f"single {idw} object, {c1}"
for x in wld:
    is_tile = x["kind"] == "tile"
    extra = "" if is_tile else ", isolated floating object, no ground, no grass, no base, no pedestal, on pure white background"
    neg = NEG_FIX if is_tile else NEG_FIX + ", grass, green grass, ground, dirt, soil, mound, platform, pedestal, base, terrain patch, round platform, magic circle, halo, glowing circle, shadow disc, floor disc, podium"
    pos=f"{wdesc(x)}, single object only, no character, fantasy rpg game asset{extra}, {STYLE}"
    items.append({"id":x["id"],"cat":"world","w":x["w"],"h":x["h"],"body":x["c1"],"accent":x["c2"],"positive":pos,"negative":neg})

# ---- Player (raca anao/humano x genero M/F; neutro depois) ----
_pbase = "young adult {0} farmer hero, common villager appearance, no armor, beige linen shirt, brown reinforced work pants, amber leather boots, determined tired expression, single full-body character, facing forward, fantasy rpg player character"
PLAYER=[
 ("player_human_male",   _pbase.format("human male") + ", short brown hair"),
 ("player_human_female", _pbase.format("human female") + ", brown hair tied back in a ponytail"),
 ("player_dwarf_male",   _pbase.format("dwarf male") + ", VERY SHORT and stocky dwarf, big head and short stubby legs, broad shoulders, a thick dense full bushy brown beard covering the chin, gruff rugged dwarf face"),
 ("player_dwarf_female", _pbase.format("dwarf female") + ", VERY SHORT and stocky dwarf, big head and short stubby legs, broad build, brown hair in two thick braids, no beard, round dwarf face"),
]
for pid,ppos in PLAYER:
    items.append({"id":pid,"cat":"player","w":32,"h":48,"body":"","accent":"",
        "positive":ppos + ", " + STYLE, "negative":"human skin discoloration, " + NEG_FIX})

out={"count":len(items),"style":STYLE,"negative":NEG_FIX,"items":items}
with open(os.path.join(BASE,"prompts.json"),"w",encoding="utf-8") as f:
    json.dump(out,f,ensure_ascii=False,indent=1)
print(f"escrito prompts.json: {len(items)} itens")
for ex in ("orc_kaand_berserker","ninrorin_void_sentinel","draconic_elder_kin","npc_corvus","tree_adult"):
    it=next((i for i in items if i["id"]==ex),None)
    if it: print(f"\n[{ex}]\n{it['positive']}")
