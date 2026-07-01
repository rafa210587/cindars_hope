# Gerador de RETRATOS (busto) via API de imagens da OpenAI (gpt-image-1), usando o sprite de
# corpo do NPC como REFERENCIA (endpoint images/edits) + o mesmo prompt de estilo/expressao do
# pipeline local. Serve para comparar GPT vs SDXL-local lado a lado.
#
# Requer a variavel de ambiente OPENAI_API_KEY (a chave NUNCA fica no codigo).
# Custo: ~1 chamada por expressao (5 por NPC) na sua conta OpenAI.
#
# Uso:
#   $env:OPENAI_API_KEY = "sk-..."
#   python gen_portraits_openai.py                # so mara_vellum (teste)
#   python gen_portraits_openai.py mara_vellum    # um NPC especifico (art_folder)
#   python gen_portraits_openai.py all            # todos os NPCs do roster
import os, sys, json, base64, uuid, urllib.request

TOOLS = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite'
SPRITE_ROOT = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\Assets\_Game\Resources\NpcSprites'
OUTROOT = r'F:\Projetos\Jogos\Cindars_hope\cindars_hope\art\portraits\_npc_openai'

sys.path.insert(0, TOOLS)
from gen_all_npcs import NPCS, RACE_BLOCK, COLORLOCK  # noqa: E402
from gen_npc_portraits import EXPRESSIONS              # noqa: E402

API = "https://api.openai.com/v1/images/edits"
MODEL = "gpt-image-1"
SIZE = "1024x1536"   # retrato (mais alto que largo) — busto

# Estilo de busto em fraseado que o gpt-image-1 entende (negativos viram instrucao positiva).
GPT_STYLE = (
    "Redraw this character as a head-and-shoulders BUST PORTRAIT: only the head, upper chest and "
    "shoulders, face large and centered, looking toward the viewer, slight three-quarter angle, "
    "like an RPG dialogue portrait. Keep the exact same character identity, skin/scale color, hair, "
    "ears, race features, outfit and color palette as the reference sprite. "
    "Refined HIGH-RESOLUTION PIXEL ART, Children of Morta art style, clean cel-shading with several "
    "tones per color, firm selective dark outline, soft top lighting, on a plain solid white background. "
    "No full body, no legs, no text, no frame, no UI."
)


def build_gpt_prompt(entry, expr_key):
    nid, race, porte, desc, neg_extra = entry
    return (RACE_BLOCK[race] + ". " + desc + ". Facial expression: " + EXPRESSIONS[expr_key]
            + ". " + COLORLOCK + ". " + GPT_STYLE)


def multipart(fields, files):
    boundary = "----openaiportrait" + uuid.uuid4().hex
    body = b""
    for k, v in fields.items():
        body += ("--%s\r\n" % boundary).encode()
        body += ('Content-Disposition: form-data; name="%s"\r\n\r\n' % k).encode()
        body += (str(v) + "\r\n").encode()
    for k, path in files:
        fn = os.path.basename(path)
        with open(path, "rb") as f:
            content = f.read()
        body += ("--%s\r\n" % boundary).encode()
        body += ('Content-Disposition: form-data; name="%s"; filename="%s"\r\n' % (k, fn)).encode()
        body += b"Content-Type: image/png\r\n\r\n" + content + b"\r\n"
    body += ("--%s--\r\n" % boundary).encode()
    return boundary, body


def gen_one(entry, expr_key, sprite_path, out):
    key = os.environ.get("OPENAI_API_KEY")
    if not key:
        raise RuntimeError("OPENAI_API_KEY ausente no ambiente.")
    prompt = build_gpt_prompt(entry, expr_key)
    boundary, body = multipart(
        {"model": MODEL, "prompt": prompt, "size": SIZE, "n": 1},
        [("image", sprite_path)],
    )
    req = urllib.request.Request(API, data=body, method="POST", headers={
        "Authorization": "Bearer " + key,
        "Content-Type": "multipart/form-data; boundary=" + boundary,
    })
    with urllib.request.urlopen(req, timeout=180) as r:
        data = json.load(r)
    b64 = data["data"][0]["b64_json"]
    with open(out, "wb") as f:
        f.write(base64.b64decode(b64))
    return os.path.exists(out) and os.path.getsize(out) > 0


def run_npc(entry):
    nid = entry[0]
    sprite = os.path.join(SPRITE_ROOT, nid + ".png")
    if not os.path.exists(sprite):
        print("SKIP %s (sprite-base ausente: %s)" % (nid, sprite), flush=True)
        return
    d = os.path.join(OUTROOT, nid); os.makedirs(d, exist_ok=True)
    for k in EXPRESSIONS:
        out = os.path.join(d, "%s_%s.png" % (nid, k))
        if os.path.exists(out):
            print("skip %s %s" % (nid, k), flush=True); continue
        try:
            ok = gen_one(entry, k, sprite, out)
        except Exception as e:
            print("ERRO %s %s: %s" % (nid, k, e), flush=True); ok = False
        print(("OK   " if ok else "FAIL ") + "%s %s -> %s" % (nid, k, out), flush=True)


def main():
    arg = sys.argv[1] if len(sys.argv) > 1 else "mara_vellum"
    targets = NPCS if arg == "all" else [e for e in NPCS if e[0] == arg]
    if not targets:
        print("NPC nao encontrado no roster: %s" % arg); sys.exit(2)
    os.makedirs(OUTROOT, exist_ok=True)
    print("=== OpenAI gpt-image-1 | %d npc(s) x %d expr | size %s ===" % (len(targets), len(EXPRESSIONS), SIZE), flush=True)
    for e in targets:
        run_npc(e)
    print("=== DONE ===", flush=True)


if __name__ == "__main__":
    main()
