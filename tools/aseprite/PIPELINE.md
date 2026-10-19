# Pipeline de Sprites — Aseprite (placeholders) + IA (arte final)

> **Local:** `tools/aseprite/`
> **Objetivo:** gerar sprites de todos os componentes do jogo (NPCs, monstros, props, mundo) com tamanho/paleta/animação corretos. Placeholders programáticos destravam o Unity já; a arte final entra por cima via IA de imagem.
> **Fonte das descrições/cores:** `docs/design/art/` (guia do ilustrador + direção visual de monstros).

## Camadas (quem faz o quê)

| Camada | Ferramenta | Resultado | Qualidade |
|---|---|---|---|
| Descrição forte por entidade | Opus / subagents Sonnet | Texto + **manifesto** (id, tamanho, bodytype, paleta hex, prompt) | — |
| Placeholder em lote | `gen_sprite.lua` + loop PowerShell sobre o **Aseprite headless** | PNG sheet + JSON atlas (com tags) | Stand-in geométrico, cor/tamanho/animação finais |
| Arte final | **IA de imagem** (Amuse local / cloud de pixel art) consumindo os prompts | Pixel art de verdade | Alta |
| Pós + import | Scripts (quantizar p/ paleta, fatiar) + importer Unity | Sprites + AnimationClips no jogo | — |

> **Importante:** subagents Haiku/Sonnet **não desenham** — são texto/código. Servem pra escrever descrições/manifesto e código Lua, não pra "gerar arte". Arte de verdade só vem da IA de imagem. As descrições ricas valem nos dois caminhos: viram placeholder **e** prompt da IA.

## Componentes

- **`gen_sprite.lua`** — gerador refinado. Cria sprite no tamanho dado, carrega paleta dos hex, desenha silhueta por **tipo de corpo**, monta 6 frames + tags (`idle/walk/attack/hit/death`), exporta PNG (`ROWS`) + JSON (`JSON_HASH`, `listTags`).
  - Params: `id, w, h, body, accent, eye, outline, bodytype, outdir`.
  - `bodytype`: `humanoid | flying | blob | quadruped` (`insect`→blob, `dragon`→quadruped por enquanto).
- **`gen_placeholder.lua`** — v1 (só humanoide). Mantido como referência; use o `gen_sprite.lua`.
- **`manifest.example.json`** — formato do manifesto que dirige o lote.

## Como rodar (headless)

O Aseprite no Windows é app GUI → o PowerShell precisa **esperar** com `Start-Process -Wait`:

```powershell
$ase = "C:\Program Files (x86)\Steam\steamapps\common\Aseprite\Aseprite.exe"
$a = @('-b',
  '--script-param','id=cave_bat','--script-param','w=32','--script-param','h=32',
  '--script-param','bodytype=flying','--script-param','body=5A4A6A',
  '--script-param','accent=2A2A40','--script-param','eye=C0C0E0','--script-param','outline=1A1620',
  '--script-param','outdir=C:/saida','--script','tools/aseprite/gen_sprite.lua')
Start-Process -FilePath $ase -ArgumentList $a -Wait -NoNewWindow -PassThru
```

## Loop de feedback (sem MCP)

1. escrever/ajustar o Lua → 2. rodar via PowerShell → 3. **abrir o PNG exportado e VER o resultado** → 4. corrigir.
O stdout do Aseprite GUI é mudo no Windows; por isso o script escreve `<id>_log.txt` e, em falha, `_ERROR.txt`. Sempre **verifique os arquivos no disco** (a existência do PNG/JSON é a prova, não o exit code).

## Convenção de animação

Tags fixas por entidade: `idle, walk, attack, hit, death`. O JSON atlas traz `frameTags` com `from/to`, prontos pra virar `AnimationClip` no import do Unity.

## Pendências (roadmap do pipeline)

- [ ] Importer Unity que lê PNG+JSON e cria sprites fatiados + clips.
- [ ] Polir templates (asas do `flying`, mais tipos: `swarm/tiny`, `construct`, `boss`).
- [ ] Pós-processamento da arte da IA (quantizar p/ paleta, downscale grade real, recorte, fatiar frames).
- [ ] Guia de setup do Amuse/ComfyUI (AMD 9070 XTX) para a arte final.
- [ ] Saída final dos PNG vai para `Assets/_Game/Art/...` (Unity importa de lá; `docs/design/art/` é só descrição).
