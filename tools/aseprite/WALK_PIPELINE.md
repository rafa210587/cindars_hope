# WALK_PIPELINE — método que FUNCIONOU para animação de personagem (2026-06-28)

> O SDXL local (ComfyUI) NÃO anima pixel art de forma consistente (ver `SPRITE_AI_ANIMATION_LOG.md` §3).
> O que fechou de ponta a ponta foi este pipeline **híbrido**: master sheet gerado no **GPT** (cloud) +
> **pós-produção 100% local** (rembg + slice + Unity). Provado no player (fazendeiro), 8 direções.

## Visão geral
```
GPT (ChatGPT web, conta do humano) gera master sheet  ──►  download do PNG
   ──►  rembg (tira fundo, mesmo gradiente)  ──►  slice_sheet.py (fatia frames por banda/gap)
   ──►  ordenar frames por direção (manual, com regra de espelho)  ──►  exportar pasta walk<dir>
   ──►  Resources/PlayerSprites/walk/<dir>/ (128 PPU, bottom-center)  ──►  PlayerWalkAnimator
```

## 1. Gerar o master sheet (GPT, via Chrome MCP)
- Operar o ChatGPT **web** pela conta do humano via **Chrome MCP** (`mcp__Claude_in_Chrome__*`).
  Extensão já pareada ("Browser 1"). Navegar p/ chatgpt.com, achar/abrir o chat, baixar a imagem
  pelo **botão de download da UI** (a URL `oaiusercontent` é assinada e bloqueada de leitura direta).
- **Não há prompt mágico:** o humano descreve conversacional ("fazendeiro estilo Stardew + Children of
  Morta, anda em todas as direções") e a ferramenta de imagem do GPT faz. **Consistência do elenco** vem
  de **anexar uma sheet aprovada como referência** ("mesmo personagem/estilo/proporção desta imagem").
- O download vai pra `Downloads`. Copiar pra pasta de trabalho do personagem
  (ex.: `Downloads\Sprites\Fazendeiro\`).

## 2. Tirar o fundo (rembg)
GPT costuma ignorar "fundo transparente" e devolve gradiente/glow → **keying por branco falha**. Usar
**rembg (u2net)** que segmenta por modelo:
```python
from rembg import remove, new_session
from PIL import Image
remove(Image.open('sheet.png').convert('RGBA'), session=new_session('u2net')).save('sheet_nobg.png')
```
**Python a usar (centralizado no F:, disco saudável):**
`F:\Projetos\Jogos\AI\ComfyUI\venv\Scripts\python.exe` — já tem PIL/numpy/**rembg**/onnxruntime
instalados; o modelo u2net está em `%USERPROFILE%\.u2net` (C:). NÃO montar venv novo nem usar o D:.

## 3. Fatiar os frames (`slice_sheet.py`)
```
python slice_sheet.py sheet_nobg.png --out <pasta_saida> --char_h 72 --skip_bg --row_min 50
```
- `--skip_bg`: entrada já tem alpha (rembg). Detecta bandas (linhas) e fatia por gaps; downscale
  LANCZOS p/ `char_h` (72px = ~0.56u a 128 PPU). Saída: `row<NN>_f<NN>.png` por frame + GIF por linha.
- Cuidado: rastro de espada/efeito que **conecta** caracteres impede a separação por gap (ataques).

## 4. Ordenar os frames por direção (manual — onde o humano dirige)
A IA gera os frames fora de ordem. Para CADA direção, montar o loop olhando as pernas:
- **f0 costuma ser o PARADO (idle)** — pés juntos, simétrico. Tirar do walk e usar como idle.
- Ler contato (passada aberta) vs passagem (pés juntos) e alternar as pernas.
- Mostrar GIF + filmstrip rotulado ao humano e iterar a sequência até ele aprovar ("é esse").
- **REGRA DO ESPELHO (flipX):**
  - **down/up (simétricos):** dá pra DOBRAR o ciclo usando frames + espelhados (ex.: a 2ª metade =
    espelho da 1ª, alternando a perna). Ex. aprovado up: `f2·f0·f1'·f4'·f1'·f2'·f0'·f1·f4·f1`.
  - **direita e diagonais (perfil):** espelhar **inverte a direção** → NÃO espelhar dentro do loop;
    o ciclo usa os frames próprios. Ex. aprovado direita: `f0·f2·f3·f4·f0·f1·f0`.
  - **esquerda / diagonais-esquerda = flip horizontal da direita/diagonal-direita** (de graça).

## 5. Exportar pasta padrão por direção
Padrão (ver `export_*.py` de referência): `Downloads\Sprites\Fazendeiro\walk<dir>\`:
- `walk_<dir>_NN.png` — célula uniforme, **bottom-center** (pés ancorados), transparente.
- `walk_<dir>.gif` — preview (~170ms/frame).
- 8 direções: down, up, right, left, downright, downleft, upright, upleft (left/diag-left por flip).
- idle: pastas `idle<dir>` (idle direita = 4 frames de respiração; esquerda = flip).

## 6. Importar no Unity + animar
- Copiar os frames p/ `Assets/_Game/Resources/PlayerSprites/walk/<dir>/`.
- `GeneratedSpriteImporter.cs` já cobre `Resources/PlayerSprites/` → importa a **128 PPU, Point,
  Single, BottomCenter** automaticamente ao abrir o Unity.
- **`PlayerWalkAnimator.cs`** (Assets/_Game/Scripts/Player/): lê `PlayerController.MoveInput` /
  `LastFacingDirection`, escolhe a direção (8 baldes por ângulo), troca `spriteRenderer.sprite`;
  parado = frame[0]. (Script-driven, sem Animator Controller YAML — proibido editar YAML.)
- `CreatePlayer()` nos 3 scene-creators já pluga o animator (sprite down_01 + AddComponent + escala 1.3).
- Pra VER: `CindarsHope → Inicializar Projeto` (regenera cenas com o wiring) → Play.

## Estado atual (2026-06-28)
- **Walk 8 direções: PRONTO** em `Downloads\Sprites\Fazendeiro\walk*` e copiado p/ `Resources/PlayerSprites/walk/`.
- **Idle:** direita + esquerda (4 frames). up/down/diagonais usam idle default por enquanto.
- **Falta:** integrar idle dedicado no animator; rodar `Inicializar Projeto` + Play (validação in-game);
  depois regerar NPCs/monstros no mesmo método.
- **Pendente do animator:** validar compile do Unity (csproj regenera ao abrir) + ajustar escala/fps in-game.

## Scripts neste diretório
- `slice_sheet.py` — fatiador (rembg→slice). `export_*.py` — exemplos de export/ordem/flip por direção.
