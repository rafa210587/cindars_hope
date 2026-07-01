---
name: pixel-art-prompt-authoring
description: Autora prompts em INGLÊS para geração de sprites pixel art (NPCs, monstros, props/mundo) via Stable Diffusion/aziib pixel mix, ancorados na lore de Vaalara e no D&D oficial. Use ao criar ou corrigir prompts em tools/aseprite/prompts.json, ao preparar um lote de geração, ou quando um sprite gerado sair com cor de pele, equipamento ou anatomia errados.
---

# Skill: Autoria de Prompts para Pixel Art (Vaalara + D&D)

A geração de arte do projeto roda local (ComfyUI + DirectML na GPU AMD, modelo **aziib pixel mix**, base SD 1.5) dirigida por `tools/aseprite/comfy_gen.ps1`, que lê os prompts de `tools/aseprite/prompts.json`. O modelo é treinado em **inglês com tags estilo booru** e tem viés **anime-chibi** — então o prompt manda na qualidade. Precedente real: o `orc_kaand_berserker` saiu com **corpo de pele humana rosa** (só a cabeça verde), **sem machado e sem armadura**, porque o prompt era vago e em português. Com um prompt em inglês explicitando **pele verde + machado + armadura bárbara + "dungeons and dragons"**, virou um orc correto. Esta skill codifica esse método.

## Quando usar

- Criar/regerar `tools/aseprite/prompts.json` (campos `positive`/`negative` por entidade).
- Preparar um lote de geração de sprites (monstros do bestiário, NPCs, props/mundo).
- Um sprite saiu errado (cor de pele/escama, arma ausente, anatomia, fundo bagunçado, HUD) e o prompt precisa de correção.
- Traduzir prompts em PT (vindos dos manifestos) para inglês detalhado.

## Quando NÃO usar

- Geração programática de placeholder (silhueta via Aseprite/`gen_creature.lua`) — não usa prompt de IA.
- Pós-processamento (fundo/downscale/paleta) — ver `postprocess.py`.
- Texto voltado ao player (diálogo/UI) — isso é (skill: localization-authoring).

## Por que existe

Sem método, cada prompt vira tentativa e erro caro (cada geração custa tempo de GPU). Um prompt **completo, explícito e ancorado** (cor + equipamento + âncora D&D + tema Vaalara + estilo) acerta na primeira e mantém **consistência** entre os 164 sprites. Documentar evita repetir a lição do orc em cada criatura.

## Leitura mínima

Para autorar com fidelidade, puxe os detalhes destas fontes (não invente cor/equipamento):

- `docs/design/art/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md` — silhueta, **cores**, comportamento por criatura.
- `docs/design/art/ART_DIRECTION_ILLUSTRATOR_GUIDE.md` — NPCs nomeados (têm **hex** e descrição), raças, props, fazenda, cidade.
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md` + `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` — raça/subraça, clã, deus, papel (tema temático).
- O `.asset` do bestiário (`Assets/_Game/Data/Bestiary/bestiary_*.asset`) — `FactionText`, `BehaviorHint`, família de loot.

## Procedimento

### 1. Identifique a âncora D&D oficial
O modelo entende vocabulário de **D&D oficial**. Mapeie a criatura/raça de Vaalara para o análogo D&D e use esse termo no prompt:

| Vaalara | Âncora D&D no prompt |
|---|---|
| Orc / Meio-orc (Clã da Fúria/Chama) | `orc barbarian`, `half-orc warrior` |
| Goblin Zhak'thul / Kobold | `goblin`, `kobold` |
| Anão Khaz Baruk / Duergar | `dwarf`, `gray dwarf duergar` |
| Drow / Elfo da Noite (Luandil) | `drow dark elf`, `shadow elf` |
| Ninrorin (elfo cinzento, vazio/Nyx) | `gray elf`, `void-touched elf, githyanki-like` |
| Draconato / Draconic / Wyvern | `dragonborn`, `young dragon`, `wyvern` |
| Abyssal / Void (aberração) | `aberration`, `demon`, `void horror` |
| Lich shard / Bone knight / Cracked bone | `lich`, `skeleton knight`, `undead` |
| Beholder kin / Eye tyrant | `beholder`, `eye tyrant` |
| Mimic / Owlbear / Bulette / Basilisk | `mimic`, `owlbear`, `bulette`, `basilisk` (são monstros D&D diretos) |
| Slime / Cube / Jelly / Ooze | `gelatinous cube`, `ooze`, `slime` |

### 2. Monte o prompt na ordem de peso (mais importante primeiro)
SD dá mais peso ao início. Ordem canônica:

1. **Subject + âncora D&D** — `orc barbarian berserker`.
2. **Cores EXPLÍCITAS** (puxe do guia/bestiário; nunca implícita) — `fully green skin, green-skinned body and face, red war paint`.
3. **Equipamento / arma / armadura** (do papel + lore) — `wielding a large two-handed battle axe, barbarian fur armor, bone and iron pauldrons, leather straps`.
4. **Tema/facção Vaalara** — Clã da Chama Viva → `ash and ember motifs`; Nyx → `lunar, dark shadow, glowing purple runes`; Pedra Negra → `black stone corruption, purple veins`.
5. **Pose/framing** — `single full-body character, facing forward, battle stance`.
6. **Style tokens (FIXOS, sempre)** — `pixel art, pixel world, clean pixel sprite, crisp pixels, no antialiasing`. (`pixel art`/`pixel world` são os trigger words do aziib.)
7. **Isolamento (FIXO, sempre)** — `isolated on plain solid white background, simple flat background`.

### 3. Escreva o negative prompt
- **Fixo (sempre):** `multiple characters, two characters, multiple subjects, text, letters, watermark, signature, scenery, landscape, room, floor tiles, wall, brick wall, frame, border, user interface, hud, health bar, game screenshot, 3d render, photorealistic, blurry, antialiasing, gradient background, drop shadow, extra limbs, deformed`.
- **Condicional (a lição do orc):** quando a cor de pele/corpo NÃO for humana, adicione ao negative: `human skin, pink skin, pale skin, light skin`. Quando a criatura deve ter arma, adicione `unarmed, no weapon`.

### 4. Ajuste por categoria
- **NPC:** sempre declare **gênero** (`male`/`female` — corrige o viés andrógino do modelo) e idade quando relevante (`elderly` para Velorin, `young child` para Pip/criança). Inclua raça (`tiefling`, `halfling`, `gnome`), profissão (`blacksmith`, `priest`, `merchant`), e roupa/cor do guia.
- **Monster:** âncora D&D + cor + arma/equipamento + tema do bioma/facção. Declare `one enemy only`.
- **World/props:** `single object only`, sem personagem; descreva material/cor (`wooden barrel with iron hoops`, `oak tree with layered green canopy`). Construções: `building exterior, fantasy medieval`.

### 5. Valide contra o checklist (§ Regressões comuns) antes de gerar o lote.

## Saída esperada

Entrada no `prompts.json` (consumida por `comfy_gen.ps1`):

```json
{
  "id": "orc_kaand_berserker", "cat": "monster", "w": 36, "h": 52,
  "body": "6A8A54", "accent": "8A2010",
  "positive": "orc barbarian berserker, fully green skin, green-skinned muscular body and face, fierce red war paint, wielding a large two-handed battle axe, barbarian fur armor with bone and iron shoulder pauldrons, tribal leather straps and belt, prominent tusks, ash and ember war motifs, angry roaring expression, dungeons and dragons fantasy, single full-body character, facing forward, battle stance, pixel art, pixel world, clean pixel sprite, no antialiasing, isolated on plain solid white background",
  "negative": "human skin, pink skin, pale skin, unarmed, no weapon, multiple characters, text, watermark, scenery, wall, floor, hud, ui, blurry, antialiasing, extra limbs, deformed"
}
```

## Regras

- **Sempre em inglês.** O modelo não entende bem português; nunca deixe prosa PT no `positive`/`negative`.
- **Cor sempre explícita.** Se o guia dá hex/nome, traduza para nome de cor em inglês (`#6A8A54` → `green`, `olive green`). Pele/escama não-humana SEMPRE declarada + reforçada no negative.
- **Ancore em D&D oficial** (vocabulário que o modelo conhece) **e** na lore de Vaalara (tema/facção/clã/deus). As duas juntas.
- **Não invente** cor/arma/lore — puxe das fontes (§ Leitura mínima). Se faltar dado, marque e pergunte.
- **Mantenha os tokens fixos** (style + isolamento + negative fixo) idênticos entre todas as entradas — consistência do lote.
- **1 sujeito por sprite.** Sempre `single ... character`/`single object only` + negative anti-múltiplos.

## Regressões comuns

| Sintoma no sprite | Causa | Correção no prompt |
|---|---|---|
| Pele humana/rosa em criatura verde/escamada | cor implícita | cor explícita no positive + `human skin, pink skin` no negative |
| Faltou arma/escudo/cajado | equipamento não citado | citar a arma + `unarmed, no weapon` no negative |
| Fundo de cenário (tijolo, sala, chão) | sem isolamento | `isolated on plain solid white background` + negative `scenery, wall, floor` |
| Barra de vida/HUD/UI desenhada | "game sprite/screenshot" mal interpretado | negative `hud, ui, health bar, game screenshot`; evite "game screenshot" no positive |
| Dois personagens / membros extras | sem trava de unicidade | `single ... character` + negative `multiple characters, extra limbs` |
| NPC homem saiu andrógino/feminino | viés anime do modelo | declarar `male` (e reforçar `masculine` se persistir) |
| Estilo muito anime/fofo demais | trigger fraco | reforçar `pixel art, retro game sprite`; reduzir tokens "cute" |

## Onde se aplica

Específico do pipeline de arte de Cindar's Hope em `tools/aseprite/` (`prompts.json`, `comfy_gen.ps1`, `postprocess.py`). Os prompts gerados servem tanto pro ComfyUI (lote automatizado) quanto pra uso manual no Amuse.

## Relacionados

- `tools/aseprite/PIPELINE.md` — pipeline completo (placeholder + IA + import).
- `tools/aseprite/COMFYUI_SETUP_AMD.md` — setup do ComfyUI/DirectML.
- (skill: data-catalog-authoring) — quando os ids/manifestos vêm de catálogos de dados.
- (skill: localization-authoring) — texto voltado ao player (não confundir com prompt de arte).
