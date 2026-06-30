# Orquestrador noturno v3: gera TODOS os NPCs (3 seeds) com BLOCO DE RACA (anti-drift) +
# descricao especifica (vestes/deus) + color-lock + negativos por raca e por personagem.
# Skeleton por PORTE (med/elf/dwarf/small/big). Auto-restart do ComfyUI (DirectML OOM), resumivel.
import os, sys, time, json, subprocess, re, urllib.request

TOOLS  = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite'
PY     = r'F:\Projetos\Jogos\AI\ComfyUI\venv\Scripts\python.exe'
COMFY  = r'F:\Projetos\Jogos\AI\ComfyUI'
OUTDIR = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\animations\_npc_tests\batch_v3'
LOG    = os.path.join(OUTDIR, '_progress.log')
SEEDS  = [7, 23, 42]
RESTART_EVERY = 2

STYLE = ("single full-body character, facing forward, one person only, fantasy rpg npc, detailed pixel art, "
         "2d action rpg game sprite, Children of Morta art style, clean defined cel-shading with few tones per color, "
         "soft neutral top lighting, firm selective dark outline, cohesive readable silhouette, full body, centered, "
         "on a plain pure white background, no shadow, no antialiasing")
NEG   = ("realistic seven heads tall proportions, photorealistic, 3d render, character reference sheet, multiple views, "
         "two characters, extra figures, cropped, headshot, text, watermark, scenery, floor, ground, drop shadow, "
         "gradient background, deformed face, distorted face, melted face, deformed, mutated, "
         "feathered wings, fur wings, fur mantle, fur pauldrons, large fur shoulder pads, fur ruff behind the back, "
         "naked, nude, topless woman, bare breasts, exposed breasts, cleavage, revealing clothes, underwear, lingerie, bikini, "
         "item icons, equipment icons, inventory grid, sprite sheet, gear grid, side panel of objects, tilesheet, icon set, "
         "floating items around the character, ui panel")
COLORLOCK = ("keep every described hair, skin, scale and eye color exactly as written, the unique accent color stays "
             "only on the clothing and gear and never tints the hair, skin or scales")

# ---- BLOCOS DE RACA (vem PRIMEIRO no prompt; tamanho relativo ao humano + features em MAIUSCULA) ----
RACE_BLOCK = {
 'human':    "a human with the exact CHUNKY STOCKY proportions of the village farmer hero, BIG ROUND HEAD, SHORT STUBBY LEGS, wide sturdy stocky body about 3.5 heads tall, not tall and not slim",
 'dwarf':    "a DWARF, about 20 percent SHORTER and 20 percent BROADER than a human, very robust and stocky, THICK STRONG WIDE muscular arms and big hands, broad barrel chest, short stout legs, low and grounded, weathered normal-tone skin",
 'elf':      "an ELF, the same height as a human but SLENDER SLIM and graceful, thin elegant build, LARGE clearly POINTED elf ears, bright vibrant glowing eyes",
 'half_elf': "a HALF-ELF, human height and a normal slim human build, small subtly POINTED ears (less than a full elf)",
 'orc':      "an ORC, about 20 percent BIGGER taller and bulkier than a human, heavy muscular build, very wide shoulders, clearly NON-HUMAN colored orcish skin, LARGE prominent TUSKS clearly jutting up from the lower jaw",
 'half_orc': "a HALF-ORC, a bit bigger and bulkier than a human, clearly NON-HUMAN colored orcish skin, LARGE visible TUSKS jutting up from the lower jaw, heavy muscular build, wide shoulders",
 'goblin':   "a GOBLIN, about 30 percent SHORTER than a human, BIG-HEADED and SKINNY, an ugly characterful face with a BIG CURVED HOOKED NOSE, LONG curved POINTED ears with little tufts of hair on them, beardless, sharp small teeth",
 'gnome':    "a GNOME, much SHORTER than a human (about 30 percent shorter), SHORT and PLUMP and CHUBBY with a round soft belly, NOT muscular and not tall, a LONG THIN POINTED NOSE, LARGE pointed leaf-shaped ears, normal human skin tone",
 'halfling': "a HALFLING, about 30 percent SHORTER than a human, slender and CHILD-LIKE in proportion (reads like a child), slightly oversized cheerful eyes, LARGE BARE HAIRY FEET with no shoes, a happy friendly face, normal human skin tone",
 'nymirian': "a NYMIRIAN, human height, POINTED ears, BLUE-TINTED skin, VIOLET eyes, flowing arabic-style robes and wraps covering most of the skin",
 'tiefling': "a TIEFLING, human height and build, small HORNS curving back from the forehead, a thin pointed TAIL visible behind the body, solid-colored eyes with NO whites, unusual skin tone",
 'dragonborn':"a DRAGONBORN, about 20 percent BIGGER and bulkier than a human, upright and imposing, reptilian SCALES covering the body, a short blunt SNOUT muzzle, HORNS swept back, vertical-pupil reptilian eyes, a visible thick TAIL, NO human ears",
}
RACE_NEG = {
 'human':    "",
 'dwarf':    "tall, slim, thin, lanky, long legs, smooth chin on a man",
 'elf':      "round ears, small ears, human ears, stocky, fat, chubby, dull eyes",
 'half_elf': "huge elf ears, round ears, blue skin",
 'orc':      "small, short, slim, thin, no tusks, tiny tusks, human teeth, fully covered chest, human skin, tan skin, bronze skin, beige skin, normal flesh skin, caucasian skin",
 'half_orc': "small, slim, no tusks, tiny tusks, human skin, tan skin, bronze skin, beige skin, normal flesh skin, caucasian skin",
 'goblin':   "tall, cute, pretty, beautiful, human nose, small round ears, beard, handsome, human skin, tan skin, beige skin, caucasian skin",
 'gnome':    "tall, muscular, athletic, human-size nose, short stubby nose, blue skin, green skin",
 'halfling': "tall, adult proportions, muscular, shoes, boots, grumpy, blue skin",
 'nymirian': "round ears, fully bare skin, revealing clothes",
 'tiefling': "no horns, no tail, white eyes, sclera, plain human skin",
 'dragonborn':"human face, human ears, smooth skin, no tail, no snout, small, short",
}

SKELS = {
 'med':   os.path.join(TOOLS, 'skeletons', 'npc_stand_player.png'),
 'elf':   os.path.join(TOOLS, 'skeletons', 'npc_elf.png'),
 'dwarf': os.path.join(TOOLS, 'skeletons', 'npc_dwarf.png'),
 'small': os.path.join(TOOLS, 'skeletons', 'npc_small.png'),
 'gnome': os.path.join(TOOLS, 'skeletons', 'npc_gnome.png'),
 'big':   os.path.join(TOOLS, 'skeletons', 'npc_big.png'),
}

# (id, race, porte, descricao especifica [papel+vestes+deus, cores reafirmadas], neg_extra [cores/erros a proibir])
NPCS = [
 ("mara_vellum", "human", "med",
  "a clearly FEMININE adult human WOMAN clerk and record-keeper with a soft womanly face, OLIVE skin, BLACK hair in a tidy bun, dark analytical eyes, a buttoned-up slate-blue coat with a row of bronze buttons, holding a thick brown ledger book in front of her waist, a small brass quill-and-ledger civic insignia on the chest, slate-blue outfit with one bronze accent on the clothing",
  "red hair, blonde hair, loose flowing hair, boy, man, male, masculine face, child"),
 ("sylveth", "elf", "elf",
  "a young wood elf WOMAN seed-seller and herbalist, warm BRONZE-COPPER sun-kissed tanned skin, MOSS-BROWN hair braided with a small green leaf, vivid GREEN-AMBER eyes, a feminine light green linen blouse over an earth-tone shirt, a leather herb belt with many little herb pouches and a round green apple-like Mana fruit of Thandra hanging from the belt, earthy green outfit",
  "pink hair, blue hair, pale white skin, round ears"),
 ("brumdar_ferro_quieto", "dwarf", "dwarf",
  "a very short and squat low dwarf blacksmith MAN with a thick GRAY BEARD in three braids with iron rings, weathered skin, thick muscular forearms, burn-scarred palms, wearing a heavy BROWN leather blacksmith apron (brown leather, not blue and not green) with a round Thoren badge showing a hammer crossed over an anvil on the chest, metal bracers, no helmet, glowing forge-orange accents",
  "beardless, tall, slim, young, green apron, green clothes, blue apron, blue vest, hook, sickle"),
 ("nimble_galhobaixo", "halfling", "small",
  "a very tiny SHORT halfling MAN builder, knee-high and child-sized, his big BARE HAIRY HOBBIT FEET with visible round toes are completely BAREFOOT (definitely no shoes and no boots), light-brown curly hair, holding a carpenter's hammer with a rolled blueprint in his tool-belt, a vest with tool pockets, a tiny brass road-coin charm of Merithus, warm brown outfit",
  "shoes, boots, footwear, sandals, socks, covered feet, tall, adult man height, long legs, big body"),
 ("gurd_carvalho_torto", "half_orc", "big",
  "a half-orc MAN of the Fury clan, his SKIN IS clearly GREEN-GRAY orcish skin (classic green-gray orc skin, definitely NOT human pink or tan), a brutish orc face with two LARGE white TUSKS clearly jutting up from the lower jaw, a short black mohawk, huge muscular shoulders, a heavy-labor demolition worker at the construction yard wearing a thick leather work harness and heavy work gloves, bare muscular chest under the harness, rugged work trousers, blue-black clan tattoos on the arms, fang Kaand war markings, combat scars",
  "human skin, tan skin, pink skin, bronze skin, beige skin, no tusks, small tusks, full shirt, slim, elf face"),
 ("hund_carvalho_torto", "half_orc", "big",
  "a half-orc MAN of the Night clan, his SKIN IS a DARK GREEN-GRAY orcish skin (the same green-gray orc skin as his brother Gurd but a darker shade, definitely NOT blue and not human), a heavy brutish ORC FACE with two LARGE white TUSKS jutting up and a broad heavy jaw, NO horns on his head, dark watchful eyes, shaved sides with a slicked top, a town GUARD and road escort on patrol wearing dark studded-leather guard armor and NO helmet, a guard's short spear, a patrol sash, a hidden black crescent-moon mark of Nyx, dark shadowy palette",
  "blue skin, blue face, red skin, human skin, no tusks, small tusks, horns, antlers, helmet, bare chest, pretty face, elf face"),
 ("ozzra_fumacazul", "goblin", "small",
  "a goblin alchemist MAN, GRAY-ORANGE skin, big hooked nose, long pointed hairy ears one pierced with a glass-tube earring, VIVID BLUE spiky hair, bright YELLOW glowing goblin eyes, a leather apron stained with multicolor splotches, many bubbling colorful potion vials hung from the belt, a small arcane sigil charm of magic",
  "human nose, cute, pretty, bearded, small round ears, tall, big, broad, red eyes"),
 ("gruta_panela_funda", "orc", "big",
  "an orc WOMAN tavern keeper, her SKIN IS clearly DARK-GREEN orcish green skin and definitely NOT human flesh tone, two big white TUSKS clearly sticking up from her lower jaw, intense RED hair in braids, FULLY CLOTHED wearing a thick long-sleeved shirt and a heavy RED tavern apron (red apron, not green) that completely cover her whole torso and chest, only her forearms bare, holding a wooden tankard, a glowing orange ember-and-flame emblem of the Living Flame clan on the apron",
  "blue skin, gray skin, red skin, no tusks, slim, bare chest, bare torso, topless, shirtless, naked torso, exposed chest, bare belly"),
 ("zrix_das_estradas", "dragonborn", "big",
  "a dragonborn cartographer traveler MAN, clearly REPTILIAN, a scaled COPPER reptile face with a prominent SNOUT muzzle and AMBER vertical-pupil eyes poking out from under a raised brown traveling HOOD, copper SCALES, scaled CLAWED hands, a thick visible TAIL, wearing a brown traveling cloak with the HOOD UP over his head, simple traveler clothes, a rolled map and a leather map-case at the hip, a small silver scales-and-raven charm",
  "human face, human nose, human hands, no tail, no scales, no snout, smooth skin, heavy plate armor, knight armor, full metal helmet"),
 ("yael_noite_mansa", "elf", "elf",
  "a Luandil night elf WOMAN, a DROW dark elf with DEEP BLUE skin, her face hands and skin are clearly dark blue blue-skinned drow-like and never pale or human-colored, long straight WHITE silver platinum hair, glowing bright VIOLET PURPLE eyes, faint luminescent constellation tattoos, FULLY DRESSED in a modest long-sleeved brown and beige tunic and trousers covering her whole body, a tiny white crescent charm of Alihana",
  "pale skin, white skin, light skin, peach skin, human skin tone, human face color, pale face, round ears, purple hair, dark hair, black hair, blue hair, teal hair, orange eyes, yellow eyes, amber eyes, topless, shirtless, bare chest, bare torso"),
 ("thalindra_veu_de_lua", "elf", "elf",
  "a Ninrorin gray elf WOMAN scholar, pale near-colorless GRAY skin, straight SILVER hair to the shoulders, ICE-BLUE eyes, a DEEP COBALT-BLUE dark blue scholar tunic with silver thread embroidered in geometric patterns at the borders, holding a rolled parchment scroll in one hand, a silver crescent-and-three-stars emblem of Alihana on the chest",
  "human skin tone, tan skin, round ears, dark hair, white robe, teal robe, light robe"),
 ("dagna_rocha_morna", "dwarf", "dwarf",
  "a very SHORT plump CHUBBY stout round female DWARF MAIDEN miner-warrior, a soft pretty FEMININE woman face with smooth clean-shaven cheeks and rosy lips and absolutely NO beard and NO facial hair, sun-bronzed skin, long BLACK hair with white clan streaks worn in MANY thick clan braids with beads, big determined dark-brown eyes, a reinforced leather cuirass with metal bracers, a pickaxe on the back, a small Thoren hammer token, copper accent on the gear",
  "beard, mustache, facial hair, bearded, male face, man, masculine, tall, slim, blonde hair"),
 ("pip_semente_solta", "halfling", "small",
  "a very SHORT little halfling CHILD merchant BOY, tiny and very child-like, messy RED hair, freckles, golden BARE HAIRY HOBBIT FEET completely barefoot no shoes, simple patched child clothes with a little vest, a big oversized backpack on his back too big for him, a pair of lucky dice and a coin charm of Finan at his belt, cheerful",
  "shoes, boots, adult, tall, beard"),
 ("ser_alaric_veyr", "human", "med",
  "a human city guard MAN, weathered-bronze skin, short BROWN hair, a neat symmetric trimmed beard, light-GREEN eyes, wearing an open-faced steel guard HELMET on his head and SLATE-BLUE steel half-armor (blue-gray, not green) with a clear golden SHIELD-WITH-SCALES emblem of Kanthor on the right shoulder, dutiful military posture",
  "no armor, robe, green armor, green tabard"),
 ("mirela_dos_lacos", "human", "med",
  "a human tailor WOMAN, BROWN skin, dark curly hair woven through with MANY colorful fabric ribbons (red blue yellow green), expressive brown eyes, a yellow measuring tape draped around her neck, a finely-tailored vibrant warm multicolor outfit that is her own showcase work, a small broken-mask brooch of Selen",
  "pale skin"),
 ("renko_tres_sorrisos", "goblin", "small",
  "a goblin merchant MAN, GRAY-ORANGE skin, thin slicked DARK hair under a small tilted cap, big hooked nose, long pointed hairy ears, half-lidded calculating YELLOW eyes, a sly TOO-WIDE GRIN showing many small teeth, a small tilted merchant cap on the head, a merchant vest with shiny brass buttons over a quality shirt, holding a gold trade-coin, a Merithus coin charm, dark-brown outfit with gold button accents",
  "human nose, cute, pretty, bearded, small round ears, tall"),
 ("eiran_valeclaro", "half_elf", "med",
  "a half-elf MAN animal tender, sun-bronzed skin, small subtly pointed ears, long BROWN hair tied with a green cord, patient AMBER eyes, simple practical keeper clothes stained with grass, a feed pouch on his belt, a small leaf token of Thandra, a small cat sitting at his feet beside him",
  "huge elf ears, round ears, blue skin"),
 ("liora_canta_rio", "nymirian", "med",
  "a half-nymirian WOMAN musician, golden skin with a FAINT BLUE tint, pointed ears, dark-brown wavy hair, VIOLET-blue eyes, flowing colorful light arabic-style fabrics and a veil, holding a small golden lyre harp in her arms, a broken-mask pendant of Selen at her neck",
  "round ears, fully blue skin, stocky, extra arm, third arm, extra limb, deformed hands, three arms"),
 ("orlan_pouso_curto", "human", "med",
  "a short broad human tavernkeeper MAN, pale skin, thin BROWN hair combed without success, a short symmetric moustache and mostly clean-shaven, kind harmless eyes, wearing a dirty-BEIGE work apron clearly over a white shirt and dark vest, a ring of jingling iron keys hanging from his belt, humble low-saturation pale and beige palette",
  "full beard, green apron, dark apron"),
 ("savra_escama_verde", "dragonborn", "big",
  "a dragonborn WOMAN herbalist, DARK-GREEN scales with irregular olive blotches, GOLD-yellow vertical-pupil eyes, short soft horns swept back, a long thin expressive tail, an herbalist apron with herb pockets, light bracers, a small Thandra leaf token",
  "human face, human ears, no tail, no scales, gray scales"),
 ("tovin_maos_de_selo", "gnome", "gnome",
  "a very SHORT and very PLUMP fat round-bellied gnome artificer office-clerk MAN, normal human skin, a LONG THIN POINTED NOSE, large pointed ears, a long PURPLE beard, round gold-frame glasses (an office clerk, NO wizard hat), a vest full of stamps each in its pocket, a wax-seal emblem of contracts and order, cobalt vest",
  "wizard hat, pointed hat, tall, muscular, human nose, blue skin"),
 ("maelor_cinza", "elf", "elf",
  "a Luandil night elf MAN, a DROW dark elf with deep ONYX BLACK skin, his face hands and whole body skin are jet BLACK obsidian black (true black drow skin, never brown gray tan pale or human), dark blue almost-black hair, SILVER reflective eyes, discreet luminescent neck tattoos, a closed unreadable expression, dark explorer clothing, a faint silver crescent of the hidden moon",
  "brown skin, charcoal gray skin, tan skin, gray skin, pale skin, white skin, light skin, peach skin, human skin tone, pale face, round ears, light hair, blonde hair"),
 ("sael_mare_quieta", "tiefling", "med",
  "a tiefling MAN fisherman with a serene presence, his SKIN IS clearly solid BLUE blue-skinned (a blue tiefling, definitely blue not gray not human), two small CURVED HORNS clearly on his forehead, a long thin pointed devil TAIL curving out behind his body, straight wet BLACK hair tied back, solid AMBER eyes with no whites, green-gray waxed fisher clothes, tall dark leather boots, a folded net over the shoulder, a wave-and-hook sea charm",
  "no horns, no tail, white eyes, plain human skin, gray skin, pale skin, human skin tone, brown skin"),
 ("mella_forno_quente", "human", "med",
  "a robust warm human baker WOMAN, warm BROWN skin with rosy cheeks, BROWN hair tied in a terra-red headscarf with flour-dusted strands, a flour-dusted WHITE baker apron (white apron, not brown or leather) over a warm-brown dress, white flour dust on her hands and apron, a dish towel on the shoulder, a small wheat-sheaf token, an easy warm smile",
  "pale skin, brown apron, leather apron, dark apron"),
 ("hess_couro_fundo", "dragonborn", "big",
  "the oldest quietest dragonborn tanner MAN, clearly REPTILIAN with a prominent long SNOUT muzzle, big curved HORNS swept back, thick DULL EARTH-BROWN matte brown scales (brown scales, definitely NOT green), a heavy visible TAIL, pale deep-set AMBER eyes, an old tanner who works animal hides, a stiff darkened stained leather tanner apron, holding a curved hide-scraping tool, a stretched animal hide hanging at his side, huge calloused clawed hands",
  "green scales, shiny glossy scales, human face, no tail, no snout, young, no horns"),
 ("tibbet_vela_torta", "gnome", "gnome",
  "a tiny very SHORT and plump pale gnome gravedigger-altarboy MAN with strong gnome features, a comically LONG THIN POINTED NOSE, LARGE pointed leaf-shaped ears, a short scruffy gray-brown gnome beard, enormous dark sleepless eyes with deep eye-bags, an oversized OFF-WHITE altarboy tunic robe (white robe, not green) with a grave-dirt-stained hem, holding a thick lit crooked candle with dripped wax, a small silver black-crescent-moon pendant of Nyx at the collar",
  "wizard hat, pointed hat, tall, muscular, blue skin, green coat, green robe"),
 ("anciao_velorin", "elf", "elf",
  "a VERY OLD elderly Ninrorin gray elf ELDER MAN, ancient and wrinkled wise face with age lines, long ASH-WHITE hair and a LONG FLOWING ASH-WHITE BEARD down his chest, thick white eyebrows, deep tired AMBER eyes, a dignified slightly forward-bent old posture, a long civic GRAY-BLUE cloak fastened by a silver Kanthor scale fibula, leaning on a tall knotted wooden oak staff",
  "human skin tone, round ears, young, middle-aged, beardless, clean-shaven, dark hair, muscular, green cloak"),
]

def up():
    try:
        urllib.request.urlopen('http://127.0.0.1:8188/system_stats', timeout=4); return True
    except Exception:
        return False

def listen_pids():
    try:
        out = subprocess.check_output(['powershell', '-NoProfile', '-Command',
            "(Get-NetTCPConnection -LocalPort 8188 -State Listen -ErrorAction SilentlyContinue).OwningProcess"],
            text=True, stderr=subprocess.DEVNULL)
        return [p.strip() for p in out.split() if p.strip()]
    except Exception:
        return []

def kill_comfy():
    for p in listen_pids():
        subprocess.run(['taskkill', '/F', '/PID', p], capture_output=True)
    for _ in range(20):
        if not listen_pids():
            break
        time.sleep(1)
    time.sleep(2)

def start_comfy():
    subprocess.Popen([PY, 'main.py', '--directml', '--cpu-vae', '--disable-smart-memory'],
                     cwd=COMFY, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    for _ in range(55):
        time.sleep(4)
        if up():
            time.sleep(3)
            return True
    return False

def fresh_comfy():
    kill_comfy()
    if not start_comfy():
        raise RuntimeError('ComfyUI nao subiu apos restart')

def fetch(pmid, out):
    for _ in range(30):
        try:
            h = json.load(urllib.request.urlopen('http://127.0.0.1:8188/history/%s' % pmid, timeout=5))
        except Exception:
            h = {}
        if pmid in h:
            for v in h[pmid].get('outputs', {}).values():
                if v.get('images'):
                    im = v['images'][0]
                    u = ('http://127.0.0.1:8188/view?filename=%s&subfolder=%s&type=%s'
                         % (im['filename'], im.get('subfolder', ''), im['type']))
                    open(out, 'wb').write(urllib.request.urlopen(u, timeout=20).read()); return True
        time.sleep(4)
    return False

def build_prompt(entry):
    nid, race, porte, desc, neg_extra = entry
    pos = RACE_BLOCK[race] + ", " + desc + ", " + COLORLOCK + ", " + STYLE
    neg = NEG
    if RACE_NEG.get(race):
        neg += ", " + RACE_NEG[race]
    if neg_extra:
        neg += ", " + neg_extra
    return pos, neg, SKELS[porte]

def gen(entry, seed, out):
    pos, neg, sk = build_prompt(entry)
    r = subprocess.run([PY, os.path.join(TOOLS, 'cn_pilot.py'), 'cn', '--control', sk,
                        '--pos', pos, '--neg', neg, '--lora', '1.0', '--cn_str', '1.0', '--cn_end', '1.0',
                        '--cfg', '6.5', '--steps', '26', '--seed', str(seed), '--w', '704', '--h', '1024',
                        '--out', out], capture_output=True, text=True)
    if os.path.exists(out):
        return True
    m = re.search(r'prompt_id:\s*([0-9a-f-]+)', (r.stdout or '') + (r.stderr or ''))
    if m and fetch(m.group(1), out):
        return True
    return False

def log(s):
    open(LOG, 'a', encoding='utf-8').write(s + "\n")
    print(s, flush=True)

def contact(nid, d):
    try:
        from PIL import Image, ImageDraw, ImageFont
        imgs = [os.path.join(d, '%s_s%d.png' % (nid, s)) for s in SEEDS]
        imgs = [p for p in imgs if os.path.exists(p)]
        if not imgs:
            return
        TH = 360
        try:
            font = ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf', 18)
        except Exception:
            font = ImageFont.load_default()
        th = []
        for p in imgs:
            im = Image.open(p).convert('RGBA'); w = int(im.width * TH / im.height)
            th.append((im.resize((w, TH), Image.NEAREST), os.path.basename(p)))
        cw = max(t.width for t, _ in th) + 12; ch = TH + 24
        sheet = Image.new('RGBA', (cw * len(th), ch), (245, 245, 248, 255)); dr = ImageDraw.Draw(sheet)
        for i, (im, lab) in enumerate(th):
            x = i * cw; dr.rectangle([x, 0, x + cw - 1, ch - 1], outline=(200, 200, 205, 255))
            dr.text((x + 6, 4), lab, fill=(20, 20, 30, 255), font=font)
            sheet.alpha_composite(im, (x + (cw - im.width) // 2, 22))
        sheet.convert('RGB').save(os.path.join(OUTDIR, '_%s_ALL.png' % nid))
    except Exception as e:
        log("  contact err %s: %s" % (nid, e))

def main():
    os.makedirs(OUTDIR, exist_ok=True)
    log("=== START v3 %d npcs x %d seeds (race-block + anti-drift) ===" % (len(NPCS), len(SEEDS)))
    since = 0
    try:
        fresh_comfy(); since = 0
    except Exception as e:
        log("  start inicial falhou: %s" % e)
    for entry in NPCS:
        nid = entry[0]
        d = os.path.join(OUTDIR, nid); os.makedirs(d, exist_ok=True)
        for seed in SEEDS:
            out = os.path.join(d, '%s_s%d.png' % (nid, seed))
            if os.path.exists(out):
                log("skip %s s%d" % (nid, seed)); continue
            ok = False
            for attempt in (1, 2, 3):
                try:
                    if (not up()) or since >= RESTART_EVERY:
                        fresh_comfy(); since = 0
                    ok = gen(entry, seed, out)
                except Exception as e:
                    log("  err %s s%d att%d: %s" % (nid, seed, attempt, e)); ok = False
                if ok:
                    since += 1
                    break
                log("  retry %s s%d (att%d falhou) -> restart limpo" % (nid, seed, attempt))
                try:
                    fresh_comfy(); since = 0
                except Exception as e:
                    log("  restart falhou: %s" % e); time.sleep(8)
            log(("OK   " if ok else "FAIL ") + "%s s%d" % (nid, seed))
        contact(nid, d)
        log("-- contact sheet: _%s_ALL.png" % nid)
    log("=== DONE ===")

if __name__ == '__main__':
    main()
