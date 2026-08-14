# Plano — Alinhar a FarmScene à Keyart Aprovada

> Documento de trabalho e **handoff** (Claude ↔ Codex). Atualizar a seção **PROGRESSO / ONDE
> PARAMOS** ao fim de cada etapa. Criado 2026-08-14.

## Meta
| | |
|---|---|
| Repo | `D:\Projetos\Cindars_Hope\cindars_hope` · branch `dev` |
| Keyart aprovada | `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png` (1672×941) |
| Mapa keyart→coords | `docs/art_catalog/_reference_keyart_GPT/README.md` |
| Cena alvo | `Assets/_Game/Scenes/FarmScene.unity` (gerada por `CreateMvpFarmScene.cs`) |
| Unity | 6000.5.7f1 — `C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe` |

## Objetivo
A FarmScene **renderizada** bater com a keyart aprovada. **Regra:** validar cada etapa comparando a
captura contra a keyart (memória `feedback-validate-against-farm-keyart`).

**Princípio-chave:** o **layout/posições da scene já batem com a keyart** (a keyart foi gerada a
partir das coordenadas reais do `CreateMvpFarmScene`). O gap é **VISUAL** — falta o sprite de cada
construção + tratamento de chão/água/caminhos. Ordem natural: **arte → wiring → validação**.

---

## DECISÕES QUE TRAVAM O RESTO (pendentes — usuário)
| # | Decisão | Opções | Voto Claude |
|---|---|---|---|
| **D1** | Casa | (a) walk-in (entra, telhado some) com fachada bespoke; (b) casa exterior sólida + interior em cena separada | (a) |
| **D2** | Câmera pixel-perfect | instalar `com.unity.2d.pixel-perfect` + `PixelPerfectCamera` (conserta grade da grama + nitidez global, muda zoom) | sim |
| **D3** | Água/lago | (a) gerar tiles de margem/shoreline (lago orgânico); (b) aceitar tilemap retangular | (a) |

**Respostas do usuário:** 2026-08-14 "pode começar a executar" — **assumindo os votos do Claude**:
D1=**(a) walk-in c/ fachada** · D2=**sim (pixel-perfect)** — mas CONFIRMAR antes de instalar o pacote
(muda look global) · D3=**(a) shoreline**. Ajustar se o usuário divergir.

---

## FASE 0 — Baseline limpo
- [ ] 0.1 Fechar Unity → batchmode `RegenAndCapture` limpo → **provar compile** e ver estado real.
- [ ] 0.2 Decidir o que commitar do WIP (ver "WIP não-commitado" abaixo). Commitar o que ajuda.
- [ ] 0.3 Confirmar o README do mapa keyart→coords como contrato de layout.

## FASE 1 — Decisões (D1/D2/D3 acima) — **bloqueia FASES 2–3**

## FASE 2 — Geração de arte (ChatGPT, com Chrome logado)
Cada peça = sprite **isolado**, fundo transparente, **3/4 top-down**, estilo da keyart, pivô **base**.
Skill: `chatgpt-web-sprite-gen` (ângulo + rate-limit). Pós: rembg → downscale → import
(`GeneratedSpriteImporter`) → categoria em `Assets/_Game/Art/Generated/World/`.

Construções: 🏠 casa · 🪟 estufa · 🐔 galinheiro · 🌾 celeiro · 🧀 queijaria · 🍷 barril · 🔨 bancada ·
⚒️ forja · 🍳 forno · 🕳️ entrada da caverna · 🌉 ponte · ⛲ Fonte da Anya · 📦 caixa de envio · 🛒 banca.
Tiles de ambiente: 💧 água + **margem/shoreline** · 🟫 caminho de terra (+bordas) · 🪨 penhasco/montanha
(parede+topo). Grama já existe (grid resolvido por D2).

- [ ] 2.x Gerar/wire cada item acima (marcar um a um).

## FASE 3 — Ajuste de código (wiring + layout) — execução via Sonnet
Arquivos: `CreateMvpFarmScene.cs`, `WorldTilemapGround.cs`, `WorldSpriteLibrary.cs`, `GeneratedSpriteImporter.cs`.
- [ ] 3.1 Wirar cada sprite gerado (trocar `GetBuiltinSprite()` fallback pelo real).
- [ ] 3.2 Casa conforme D1 (fachada + Y-sort; roof-reveal se walk-in).
- [ ] 3.3 Lago footprint orgânico (não 4 retângulos) via water-tilemap + shoreline; rio com margens + ponte sprite.
- [ ] 3.4 Caminhos de terra pintados (path-tilemap) ligando homestead↔caverna↔lago↔fileira sul.
- [ ] 3.5 Muralha norte (penhasco) + entrada da caverna + veios de minério visíveis.
- [ ] 3.6 Y-sort global (`Transparency Sort Mode = Custom Axis Y`).
- [ ] 3.7 Tune de posição/escala onde a keyart diverge. NÃO mexer em gameplay/colliders/interactables.
- [ ] 3.8 Revalidar regra "nenhum bloqueio físico sem sprite".

## FASE 4 — Validação (loop por elemento)
- [ ] 4.1 Unity fechado → batchmode `RegenAndCapture` (full + close).
- [ ] 4.2 Comparar CADA região vs keyart; iterar o que divergir.
- [ ] 4.3 Rebuild verde (`dotnet` das assemblies + regen sem erro) — regra `validation-truth`.
- [ ] 4.4 Playtest humano (Y-sort, colisão, roof-reveal, Console limpo).

## FASE 5 — Fechamento
- [ ] 5.1 Commit arte+código+keyart (PT, Co-Authored-By). Push só quando o usuário pedir.
- [ ] 5.2 Atualizar `docs/art_catalog` + memórias.

---

## PROGRESSO / ONDE PARAMOS  ← atualizar ao fim de cada etapa
- **2026-08-14 (Claude):** Keyart gerada, aprovada pelo usuário e salva
  (`_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png` + README com mapa keyart→coords). Plano
  criado (este arquivo). **Nada da FASE 2/3 executado ainda.**
- **2026-08-14 (Claude):** usuário mandou executar. Decisões assumidas (D1=a, D2=sim*, D3=a).
  Iniciando **FASE 2 (geração de sprites via ChatGPT)** — Chrome logado. FASE 0 (baseline/regen limpo)
  PENDENTE até o usuário fechar o Unity.
- **2026-08-14 (Claude):** FASE 2 iniciada. **1ª folha gerada** no ChatGPT (fileira sul: galinheiro/
  celeiro/queijaria/barril), estilo batendo com a keyart. Raw versionado em
  `docs/art_catalog/_reference_keyart_GPT/_raw_building_sheets/gpt_farm_buildings_south_sheet.png`.
  Estilo aprovado (usuário pediu casa maior). **Casa principal gerada** (grande, hero, sobrado telha
  vermelha) → `_raw_building_sheets/gpt_farm_house.png`. Gerando as demais folhas.
- **2026-08-14 (Claude):** FASE 2 (geração) **CONCLUÍDA** — 13 sprites em 4 raws staged em
  `_raw_building_sheets/`:
  - `gpt_farm_house.png` (casa hero, sozinha)
  - `gpt_farm_buildings_south_sheet.png` (2×2: galinheiro, celeiro, queijaria, barril)
  - `gpt_farm_greenhouse_crafts_sheet.png` (2×2: estufa, bancada, forja, fogão)
  - `gpt_farm_landmarks_sheet.png` (2×2: caverna/mina, ponte, Fonte-Anya c/ glifo, caixa de envio)
  - FALTA gerar: **banca de venda (sell stall)** — pendente (pode reusar prop ou gerar depois).
- **PRÓXIMO PASSO (FASE 2→3 — pós/wiring):** fatiar cada folha nos 4 quadrantes + **remover fundo cinza**
  (flood-fill a partir da borda p/ preservar cinza interno de pedra/forja) + trim → PNGs individuais em
  `Assets/_Game/Art/Generated/World/{building,props}/` com nomes canônicos → import (`GeneratedSpriteImporter`)
  → **wirar** no `CreateMvpFarmScene` (trocar `GetBuiltinSprite()` pelos sprites). Slicing/bg = Claude
  (precisa checagem visual); wiring (código) = subagent Sonnet. Confirmar D2 antes de pixel-perfect.
- **2026-08-14 (Claude):** **13 sprites fatiados** (slicer flood-fill validado visualmente em magenta) e
  **copiados pro Assets**: `building/{farmhouse,greenhouse,coop,barn,cheese_hut,barrel_shed}` +
  `props/{workbench,forge,cooking_station,cave_entrance,bridge,fonte_anya,shipping_bin}`. Slicer em
  `scratchpad/slice.ps1`; raws em `_raw_building_sheets/`. **Wiring delegado ao Sonnet** (editar só
  `CreateMvpFarmScene.cs`: cada `Create*` usa o sprite bespoke, mantendo colliders/interactables/posições/
  Y-sort; casa=farmhouse como fachada c/ RoofReveal). Depois: Claude verifica build + regen+captura vs keyart.
- **2026-08-14 (Claude):** **Wiring FASE 3 concluído e VERIFICADO no disco** (build exit 0). Os 13 sprites
  bespoke wirados em `CreateMvpFarmScene.cs` (6 literais + 7 via switch/variável); `CreateFontePart`
  removido; objetos compostos c/ collider no root usam child `Visual` pra escala não afetar o collider;
  interactables/colliders todos preservados. **Ainda NÃO validado visualmente** (Unity estava aberto).
- **2026-08-14 (Claude):** **VALIDADO VISUALMENTE** (regen+captura batchmode, exit 0). Os 13 sprites
  bespoke aparecem no lugar certo, batendo com a keyart (casa NE, caverna NO, Fonte esquerda, fileira
  sul, ponte, lago SE, crafts). **Bug corrigido:** escala da casa (era `localScale`=unidades com drawMode
  Simple → gigante; agora escala uniforme = alvo/nativo + base alinhada à parede sul). Qualidade dos
  sprites excelente; grama boa no zoom do jogo.
  - **GAPS visuais restantes (próximas iterações):** (1) **água/lago ainda RETANGULAR** (falta D3 shoreline);
    (2) sem **caminhos de terra** (keyart tem); (3) **penhasco/montanha** no topo pouco visível;
    (4) confirmar visibilidade da **estufa** (pode estar atrás da casa); (5) **banca de venda** não gerada;
    (6) as cercas do perímetro parecem trilho — revisar.
- **2026-08-14 (Claude):** **MILESTONE COMMITADO** (checkpoint local, 48 arquivos: keyart + 13 sprites +
  wiring + cena regenerada + scripts + plano). NÃO pushado (aguarda pedido do usuário).
- **2026-08-14 (Claude):** **Bump de escala das construções** (~1.35x) aplicado — casa w+4 (~11u), celeiro
  5.5, galinheiro 4, estufa 5.5, queijaria/barril 4.5, caverna 4.5, fonte 3.2, crafts 3, envio 2. Captura
  com close-up no HOMESTEAD (novo center (26,6) na FarmSceneCapture) + player sobreposto em escala real
  pra conferir proporção. Prédios agora imponentes (casa ~8x player). Aguardando veredito do usuário sobre
  o tamanho. (Regen exit 0, build green.) NÃO commitado ainda (o bump).
- **2026-08-14 (Claude):** 2ª iteração — fileira sul MAIOR (celeiro 7u, galinheiro 5u, queijaria/barril
  5.5u) + **crafts espaçados** (workbench 23, forge 27, cooking 31; 4u). Close-ups por área (homestead +
  fileira sul, 3 views na FarmSceneCapture). Prédios/crafts/tamanhos **aprovados visualmente**. Cerca do
  perímetro lê como cerca de madeira no close. Regen exit 0. Bump+spacing NÃO commitado ainda.
- **PRÓXIMO PASSO (TERRENO — fecha a keyart; precisa Chrome/geração):** gerar tiles de **água+margem
  (shoreline/bank)**, **caminho de terra**, **penhasco**; **alargar o rio** com margens (conserta a ponte
  que "flutua"), dar **shoreline ao lago**, pintar **caminhos** ligando homestead↔caverna↔lago↔fileira sul,
  reposicionar a ponte no rio alargado. Depois commitar. Pendências menores: estufa (visibilidade), banca
  de venda, D2 pixel-perfect.

### WIP não-commitado (⚠️ verificar antes de confiar)
Feito nesta sessão mas **não validado em regen limpo** (a última regen do usuário rodou com código
velho — cena salva tinha 0 sprites Tiled):
- Wiring de 17 sites de arte real em `CreateMvpFarmScene.cs` (árvores, forrageio, minério, água, fonte, casa).
- `FarmSceneCapture.cs` (Dev tool: `RegenAndCapture` → `docs/validation/playmode/farm_capture{,_closeup}.png`).
- Water-tilemap (`WorldTilemapGround.PaintWater`) + rio/lago via PaintWater; cerca nos bounds; montanha rock_ore; casa/paredes tiladas. **Compila (dotnet exit 0) mas não confirmado em Unity batchmode.**
- Reseam dos tiles de grama (`ground_grass*`, `ground_soil`) — backup em scratchpad. **Efeito visual marginal** (a grade é mais câmera não-pixel-perfect que seam → ver D2).

---

## CONTEXTO TÉCNICO ESSENCIAL (para retomar frio)
- **Regen + captura (batchmode, Unity FECHADO):** PowerShell, `Start-Process -Wait` (o `& Unity.exe`
  NÃO bloqueia):
  ```powershell
  $u="C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe"; $p="D:\Projetos\Cindars_Hope\cindars_hope"
  Start-Process $u -Wait -ArgumentList "-batchmode","-projectPath",$p,"-executeMethod","CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture","-quit","-logFile","$env:TEMP\regen.log"
  ```
  Guard: se `Get-Process Unity` existir, **abortar** (batchmode paralelo é proibido — rule `unity-assets`).
- **Regen "oficial"** de tudo: menu `CindarsHope/Inicializar Projeto` (pesado; recria as 3 cenas).
- **Sistema de coords:** grid 64×44 centrado, x∈[-32,32] leste+, y∈[-22,22] norte+. Landmarks no README da keyart.
- **Arte de mundo:** `Assets/_Game/Art/Generated/World/{tiles,building,building/houses_modular,trees,foliage,props,interior,animals,crops,locations,cave}`. Carregada por `WorldSpriteLibrary`. Import por `GeneratedSpriteImporter` (PPU 128; carve-outs: houses_modular=64, cave tiles=128).
- **ChatGPT (arte):** projeto "Sprites - Fazendeiro", chat "Recuperação de mapa fazenda" — via **Chrome logado do usuário** (Claude não faz login). Skill `chatgpt-web-sprite-gen`.
- **Regras não-negociáveis:** commits em PT; `validation-truth` (exit 0 ou não passou); resultado de
  subagent NÃO é evidência (verificar no disco + rebuild); sem YAML manual de `.unity/.prefab/.asset`;
  não quebrar colliders/interactables/gameplay ao mexer no visual.
- **Roteamento de modelo (Claude):** execução (edições/regen) → subagent **Sonnet**; design/decisão → Opus.
  (No Codex: fazer você mesmo, seguindo `AGENTS.md`.)

---

## PROMPT PARA O CODEX CONTINUAR (colar quando precisar)
```text
Você é o Codex trabalhando no projeto Unity C# "Cindar's Hope" em D:\Projetos\Cindars_Hope\cindars_hope (branch dev).
Primeiro leia: AGENTS.md e .claude/rules/ (regras), e docs/project/PLANO_FARM_KEYART_ALINHAMENTO.md (o plano + a seção "PROGRESSO / ONDE PARAMOS").

Contexto: estamos alinhando a FarmScene à keyart aprovada em docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png (mapa keyart→coordenadas no README da mesma pasta). O layout/posições da scene já batem com a keyart; o gap é VISUAL (sprites das construções + água/caminhos). Cena gerada por Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs; sprites carregados via WorldSpriteLibrary; chão/água via WorldTilemapGround; import via GeneratedSpriteImporter.

Faça: retome pelo "PRÓXIMO PASSO" da seção PROGRESSO. Respeite as decisões D1/D2/D3 (se ainda não respondidas, PARE e pergunte). Não quebre colliders/interactables/gameplay. Toda geração/validação de asset passa pelos 3 comandos canônicos (Inicializar/Validar/Reparar) — sem [MenuItem] avulso.

Validação (Unity FECHADO, sem batchmode paralelo): rode em PowerShell com Start-Process -Wait o -executeMethod CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture (gera docs/validation/playmode/farm_capture{,_closeup}.png) e COMPARE contra a keyart. Só dê etapa por concluída com build verde (exit 0). Geração de sprites via ChatGPT exige o Chrome logado do usuário — se precisar, PARE e peça.

Ao terminar cada etapa: ATUALIZE a seção "PROGRESSO / ONDE PARAMOS" em docs/project/PLANO_FARM_KEYART_ALINHAMENTO.md (o que fez + próximo passo) e faça commit em português (não faça push sem o usuário pedir).
```
