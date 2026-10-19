# Gerador de RETRATOS (busto: rosto + parte do dorso) de cada NPC, para a UI:
#   - painel de conversa (NpcInteractionPortraitHud)
#   - tela de status de companion (aba Social — a construir)
# Pixel-art de alta-res (mais caprichado que o sprite de mundo), SEM esqueleto de corpo inteiro
# (txt2img + pixel LoRA via cn_pilot.py mode "txtpix"). Resumivel, auto-restart do ComfyUI.
#
# Reaproveita TODO o roster + blocos de raca + color-lock de gen_all_npcs.py (nao duplica descricao).
#
# FASE 1 (escolha de seed): NPCs SEM seed fixo em CHOSEN_SEED geram so o NEUTRO nos 3 seeds
#   candidatos -> abra a contact sheet _<id>_FACE.png e escolha o melhor seed.
# FASE 2 (expressoes): preencha CHOSEN_SEED[<id>] = <seed> e rode de novo; NPCs com seed fixo
#   geram as 5 expressoes NAQUELE seed (rosto consistente entre expressoes).
import os, sys, time, json, subprocess, re, urllib.request

TOOLS  = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite'
PY     = r'F:\Projetos\Jogos\AI\ComfyUI\venv\Scripts\python.exe'
COMFY  = r'F:\Projetos\Jogos\AI\ComfyUI'
OUTDIR = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\portraits\_npc\batch_v1'
LOG    = os.path.join(OUTDIR, '_progress.log')
SEEDS  = [7, 23, 42]          # seeds candidatos da FASE 1 (escolha de rosto)
RESTART_EVERY = 2

# Reusa o roster e os blocos de raca ja autorados (degrau 2 da escada de minimalismo: reuse).
sys.path.insert(0, TOOLS)
from gen_all_npcs import NPCS, RACE_BLOCK, RACE_NEG, COLORLOCK  # noqa: E402

# ---- Framing de BUSTO + estilo pixel-art de alta-res (substitui o STYLE full-body) ----
PORTRAIT_STYLE = (
    "head and shoulders bust portrait, only the head and upper chest and shoulders visible, "
    "face large centered and clearly readable, looking toward the viewer, slight three-quarter angle, "
    "shoulders-up framing like an rpg dialogue portrait, one person only, "
    "refined high-resolution detailed pixel art, 2d action rpg game character portrait, "
    "Children of Morta art style, clean defined cel-shading with several tones per color for soft volume, "
    "soft neutral top lighting, firm selective dark outline, cohesive readable silhouette, "
    "on a plain pure white background, no shadow, no antialiasing"
)
# Negativo do BUSTO: invertido em relacao ao full-body — aqui PROIBIMOS corpo inteiro e pernas.
PORTRAIT_NEG = (
    "full body, full length, whole body, legs, knees, feet, boots shown, hips, waist down, standing pose, "
    "wide shot, distant camera, tiny head, small face, far away, "
    "photorealistic, 3d render, character reference sheet, multiple views, turnaround, "
    "two characters, extra figures, extra person in background, cropped face, forehead cut off, chin cut off, "
    "text, letters, watermark, signature, scenery, room, floor, ground, gradient background, "
    "frame, border, ui panel, hud, health bar, "
    "deformed face, distorted face, melted face, asymmetric eyes, extra eyes, deformed, mutated, extra fingers, "
    "feathered wings, fur mantle, large fur shoulder pads, fur ruff behind the back, "
    "naked, nude, topless, bare breasts, exposed breasts, cleavage, revealing clothes, underwear, lingerie, "
    "item icons, equipment icons, inventory grid, sprite sheet, icon set, floating items around the character"
)

# ---- Expressoes mapeadas aos estados de NpcOpinion da HUD ----
# (chave = sufixo do arquivo; usada depois no wiring por enum de emocao)
EXPRESSIONS = {
    # Cada feicao traz ROSTO + LINGUAGEM CORPORAL (postura, angulo de cabeca, ombros, maos/gesto)
    # para dar dinamismo — a pose muda com a emocao, nao so o rosto.
    "neutral":   "a calm neutral composed expression, relaxed and still, upright posture with level shoulders, looking straight at the viewer",
    "happiness": "a warm friendly genuine smile with bright kind eyes, open lively posture, shoulders back and head lifted a little, a light welcoming hand gesture",
    "love":      "a soft loving affectionate expression, gentle tender smile and adoring half-lidded eyes with a faint blush, leaning slightly toward the viewer with the head tilted and one hand raised near the chest or cheek",
    "disdain":   "a cold disdainful unimpressed sneer with one eyebrow raised and the chin tilted up, leaning back and turning a shoulder away, arms crossed or a dismissive flick of the hand",
    "hatred":    "a furious hateful glare with a deep scowl and bared gritted teeth, leaning aggressively forward with raised tense shoulders and a clenched fist or a sharp threatening gesture",
}
NEUTRAL = "neutral"

# Seeds escolhidos por NPC (FASE 2). Vazio = ainda escolhendo rosto -> roda so o NEUTRO em N seeds.
# Ex.: CHOSEN_SEED = {"mara_vellum": 23, "sylveth": 7}
CHOSEN_SEED = {}

# Dimensoes do busto (retrato, alta-res p/ UI). Downscale fica no postprocess (ex. height 256).
W, H = 768, 960
CFG, STEPS, LORA = 6.0, 28, 0.9


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


def build_prompt(entry, expr_key):
    """Reusa raca + descricao + color-lock; troca o framing full-body por busto + expressao."""
    nid, race, porte, desc, neg_extra = entry
    expr = EXPRESSIONS[expr_key]
    pos = RACE_BLOCK[race] + ", " + desc + ", " + expr + ", " + COLORLOCK + ", " + PORTRAIT_STYLE
    neg = PORTRAIT_NEG
    if RACE_NEG.get(race):
        neg += ", " + RACE_NEG[race]
    if neg_extra:
        neg += ", " + neg_extra
    return pos, neg

def gen(entry, expr_key, seed, out):
    pos, neg = build_prompt(entry, expr_key)
    r = subprocess.run([PY, os.path.join(TOOLS, 'cn_pilot.py'), 'txtpix',
                        '--pos', pos, '--neg', neg, '--lora', str(LORA),
                        '--cfg', str(CFG), '--steps', str(STEPS), '--seed', str(seed),
                        '--w', str(W), '--h', str(H), '--out', out], capture_output=True, text=True)
    if os.path.exists(out):
        return True
    m = re.search(r'prompt_id:\s*([0-9a-f-]+)', (r.stdout or '') + (r.stderr or ''))
    if m and fetch(m.group(1), out):
        return True
    return False

def log(s):
    open(LOG, 'a', encoding='utf-8').write(s + "\n")
    print(s, flush=True)

def contact(nid, d, items, tag):
    """items = lista de (label, path). Monta uma folha lado a lado para escolha visual."""
    try:
        from PIL import Image, ImageDraw, ImageFont
        items = [(lab, p) for (lab, p) in items if os.path.exists(p)]
        if not items:
            return
        TH = 420
        try:
            font = ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf', 18)
        except Exception:
            font = ImageFont.load_default()
        th = []
        for lab, p in items:
            im = Image.open(p).convert('RGBA'); w = int(im.width * TH / im.height)
            th.append((im.resize((w, TH), Image.NEAREST), lab))
        cw = max(t.width for t, _ in th) + 12; ch = TH + 24
        sheet = Image.new('RGBA', (cw * len(th), ch), (245, 245, 248, 255)); dr = ImageDraw.Draw(sheet)
        for i, (im, lab) in enumerate(th):
            x = i * cw; dr.rectangle([x, 0, x + cw - 1, ch - 1], outline=(200, 200, 205, 255))
            dr.text((x + 6, 4), lab, fill=(20, 20, 30, 255), font=font)
            sheet.alpha_composite(im, (x + (cw - im.width) // 2, 22))
        sheet.convert('RGB').save(os.path.join(OUTDIR, '_%s_%s.png' % (nid, tag)))
    except Exception as e:
        log("  contact err %s: %s" % (nid, e))


def main():
    os.makedirs(OUTDIR, exist_ok=True)
    phase2 = [e for e in NPCS if e[0] in CHOSEN_SEED]
    log("=== START portraits: %d npcs | FASE2(seed fixo)=%d | FASE1(escolha)=%d ==="
        % (len(NPCS), len(phase2), len(NPCS) - len(phase2)))
    since = 0
    try:
        fresh_comfy(); since = 0
    except Exception as e:
        log("  start inicial falhou: %s" % e)

    for entry in NPCS:
        nid = entry[0]
        d = os.path.join(OUTDIR, nid); os.makedirs(d, exist_ok=True)
        chosen = CHOSEN_SEED.get(nid)

        if chosen is None:
            # FASE 1 — so o neutro, varios seeds, para escolher o rosto
            jobs = [(NEUTRAL, s) for s in SEEDS]
            tag = "FACE"
        else:
            # FASE 2 — todas as expressoes no seed escolhido
            jobs = [(k, chosen) for k in EXPRESSIONS.keys()]
            tag = "EXPR"

        for expr_key, seed in jobs:
            out = os.path.join(d, '%s_%s_s%d.png' % (nid, expr_key, seed))
            if os.path.exists(out):
                log("skip %s %s s%d" % (nid, expr_key, seed)); continue
            ok = False
            for attempt in (1, 2, 3):
                try:
                    if (not up()) or since >= RESTART_EVERY:
                        fresh_comfy(); since = 0
                    ok = gen(entry, expr_key, seed, out)
                except Exception as e:
                    log("  err %s %s s%d att%d: %s" % (nid, expr_key, seed, attempt, e)); ok = False
                if ok:
                    since += 1
                    break
                log("  retry %s %s s%d (att%d) -> restart limpo" % (nid, expr_key, seed, attempt))
                try:
                    fresh_comfy(); since = 0
                except Exception as e:
                    log("  restart falhou: %s" % e); time.sleep(8)
            log(("OK   " if ok else "FAIL ") + "%s %s s%d" % (nid, expr_key, seed))

        if chosen is None:
            contact(nid, d, [("s%d" % s, os.path.join(d, '%s_%s_s%d.png' % (nid, NEUTRAL, s))) for s in SEEDS], "FACE")
            log("-- contact (escolha de rosto): _%s_FACE.png" % nid)
        else:
            contact(nid, d, [(k, os.path.join(d, '%s_%s_s%d.png' % (nid, k, chosen))) for k in EXPRESSIONS.keys()], "EXPR")
            log("-- contact (expressoes): _%s_EXPR.png" % nid)
    log("=== DONE ===")

if __name__ == '__main__':
    main()
