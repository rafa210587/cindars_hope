---
name: chatgpt-web-sprite-gen
description: Gera sprites de MUNDO (peças modulares de construção, props, tiles, vegetação, landmarks) pela interface web do ChatGPT via Chrome MCP, no projeto "Sprites - Fazendeiro", no estilo pixel Stardew+Children of Morta. Usar quando o pipeline local (Aziib/SDXL) não dá conta — peças modulares isoladas, texturas tileáveis — ou quando o usuário pedir geração via ChatGPT. Inclui a regra de ÂNGULO por componente e o tratamento de rate limit (pausa 10 min + retry).
---

# Skill: Geração de Sprites de Mundo via ChatGPT Web

O pipeline local (ComfyUI) **não** produz peça modular limpa: o Aziib vira mini-objeto isométrico ou mete personagem anime, e o SDXL 1024 trava o PC. O **ChatGPT web (gpt-image-1)** acerta de primeira peças isoladas e texturas tileáveis, no estilo do projeto. Esta skill codifica o método completo (Chrome MCP → projeto → prompt → download → staging) e, principalmente, a **regra de ângulo** — o erro mais comum.

## Quando usar

- Gerar peças modulares de construção (parede, porta, janela, telhado), props, tiles de chão, vegetação ou construções-herói para a cidade/fazenda/mundo.
- O pipeline local falhou em isolar/compor a peça, ou o usuário pediu explicitamente via ChatGPT.

## Quando NÃO usar

- Sprites de PERSONAGEM (player/NPC/monstro) — têm pipeline próprio (ver [[pixel-art-prompt-authoring]], retratos em `gen_portraits_openai.py`).
- Quando o local resolve bem e barato (ex.: árvore/arbusto inteiros no Aziib já saem ótimos — ver `reference-world-sprite-recipe` na memória).

## Regra de ÂNGULO (a mais importante — o jogo é 3/4 top-down estilo Stardew/Zelda)

Confirmado pela arte do projeto: player com **8 direções** de caminhada = câmera 3/4 top-down inclinada. Cada componente cai em **um de dois planos**:

| Plano | Ângulo | Componentes |
|---|---|---|
| **Chão** | top-down / bird's eye, achatado | tiles de terreno (grama, paralelepípedo, terra, deck, água); props deitados (tapete, solo arado, marcadores) |
| **Em pé** | elevação **frontal** (câmera mostra só um fiapo do topo em objetos gordos) | paredes, **porta, janela** (ficam na face vertical → frontal, NÃO bird's eye), árvores, arbustos, props (barril, poço, fonte, banco, lampião, cerca, estátua), construções inteiras |
| **Telhado** | inclinação **frontal vista de cima** (3/4): telhas recuam pra cima até a cumeeira, formato trapezoidal | telhados |

Regras de ouro do modular: **ortográfico** (sem fuga de perspectiva), **mesma elevação frontal** em todas as peças (senão parede/porta/telhado não encaixam), **SEM isometria** (parede lateral visível briga com o personagem frontal), uma direção de luz (topo suave).

Erros clássicos de ângulo: porta/janela geradas top-down (erradas — são frontais); telhado gerado como grade chapada reta (errado — precisa da inclinação 3/4); parede gerada top-down virando "chão" (errada — é frontal).

## Prefixo de estilo (travado no projeto — repetir SEMPRE no começo)

> `Authentic 2D PIXEL ART, Stardew Valley style: visible pixel clusters, warm saturated earthy palette, simple flat shading, dense detailed pixel work, NO painterly rendering, NO smooth gradients, NO blur, NO 3D, NO photorealism.`

## Estrutura do prompt

`[prefixo de estilo] + Subject: [componente com ÂNGULO explícito] + [isolamento] + Square 1 to 1.`

- **Objeto isolado** (porta, janela, prop, árvore): `single X as an isolated modular piece, front view ... ALONE, centered on a plain flat neutral grey background. NO wall, NO building, NO ground, NO people, NO text.`
- **Textura tileável** (parede, chão): `seamless X material texture, flat straight-on front view (chão: top-down seen from directly above), fills the ENTIRE square edge to edge and tiles seamlessly. NO whole house, NO perspective, NO people, NO text.`
- **Telhado**: `front-facing PITCHED roof piece, 3/4 top-down Stardew view seen slightly from above, tiles receding upward toward a ridge, wooden eave at the front lower edge, slanted trapezoid shape, NOT a flat straight-on grid.`

## Procedimento (Chrome MCP)

1. `list_connected_browsers` → `select_browser`. `tabs_context_mcp{createIfEmpty:true}`.
2. `navigate` para o projeto: `https://chatgpt.com/g/g-p-6a41832fe7108191b307301200fb288f/project` (id do projeto "Sprites - Fazendeiro"). Se o id mudar, achar via `document.querySelectorAll('a[href*="/g/g-p-"]')`.
3. `find` o textbox do compositor → clicar → `type` o prompt (SEM em-dash "—"; usar ":" ou "-", que o em-dash trava a digitação). Confirmar com JS que o texto entrou (`div[contenteditable].innerText.length`).
4. `find` o botão "Enviar prompt" → clicar (NÃO confiar só no Enter).
5. Aguardar ~60-90s. Verificar por JS:
   `{generating:!!document.querySelector('[data-testid="stop-button"]'), alts:[...document.querySelectorAll('main img')].map(i=>i.alt).filter(Boolean)}`
   Sucesso = `generating:false` e um novo alt "Imagem gerada: ...".
6. **Download** (sem depender do botão da UI): JS in-page
   ```js
   var img=[...document.querySelectorAll('main img')].filter(i=>i.alt).slice(-1)[0];
   var b=await (await fetch(img.src,{credentials:'include'})).blob();
   var a=document.createElement('a'); a.href=URL.createObjectURL(b); a.download='gpt_<id>.png';
   document.body.appendChild(a); a.click(); a.remove();
   ```
   Cai em `~/Downloads`. Mover para staging `art/world_gpt/raw/` (Move-Item).
   > NÃO retornar `img.src` como valor de tool — o harness bloqueia URLs com query/cookie ("BLOCKED: Cookie/query string data"). Só usar `src` dentro do fetch.
7. **Pós** (`postprocess.py`): objeto (porta/janela/prop/árvore) → rembg + trim + downscale (transparente). Textura (parede/telhado/chão) → NÃO remover fundo; só crop/downscale pra tile.

## Tratamento de erro — rate limit (SEMPRE pausar 10 min e retry)

Se a geração falhar por **limite/estouro de uso**, pausar **10 minutos** e **retentar** (repetir até passar). Detectar checando o texto da página por (case-insensitive):
`limite de gera|image generation limit|reached your limit|too many requests|tente novamente mais tarde|rate limit|try again later`.

Para pausar de verdade (o loop dorme e volta sozinho): usar `mcp__...__send_later` ou `ScheduleWakeup` com ~600s, retomando a mesma peça. Não abandonar a fila: registrar quais peças faltam e continuar após o cooldown. Distinguir de falha real (prompt recusado, erro de rede) — essa não é rate limit; nesse caso ajustar e reenviar, não só esperar.

> Cuidado com falso-positivo de detecção: um regex amplo por "rate" casa dentro de "generate/accurate". Ancorar nos termos completos acima.

## Verificação (não confiar na narração)

- Conferir no disco que o PNG existe em `art/world_gpt/raw/` com tamanho > 0 (ver [[subagent-results-not-evidence]] / regra de evidência).
- Julgar o sprite pelo **ângulo** (tabela acima) e pelo estilo (pixel cravado, sem anime, isolado), não pelo alt-text.

## Relacionados

- `pixel-art-prompt-authoring` — autoria de prompt (âncora de estilo/cor); esta skill é o caminho ChatGPT-web.
- `reference-world-sprite-recipe` (memória) — roteamento local vs ChatGPT por categoria.
- `sprite-generation-pipeline` — pipeline local ComfyUI (alternativa para o que o local faz bem).
