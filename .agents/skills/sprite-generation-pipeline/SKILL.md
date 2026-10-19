---
name: sprite-generation-pipeline
description: Operar o pipeline de geração de sprites por IA (ComfyUI/SDXL local na AMD) — build de prompts, geração em lote resumível, pós-processamento (remoção de fundo + downscale) e wiring no Unity. Use para gerar/regenerar arte de inimigos, NPCs, props e player.
---

# Skill: Operar o Pipeline de Geração de Sprites (IA local)

> **Complementa** [pixel-art-prompt-authoring](../pixel-art-prompt-authoring/SKILL.md) (que ensina a ESCREVER o texto do prompt). Esta skill é o **manual de operação** do sistema: build → gerar → pós → wire. Específica do projeto Cindar's Hope (Windows/PowerShell, AMD RX 9070 XT / DirectML).

**Invariante:** a prova de um sprite gerado é o **arquivo no disco + inspeção visual** (não o exit code, não a narração). Cada etapa verifica artefato antes de seguir. Geração é **resumível** (pula `<id>_raw.png` existente) — nunca apague o lote inteiro pra "recomeçar" sem motivo.

---

## 1. Arquitetura (4 estágios)

```
manifests + monster_overrides.json
        │  (build_prompts.py — determinístico)
        ▼
   prompts.json  (168 itens: 60 monster + 39 npc + 65 world + 4 player)
        │  (ComfyUI API: SDXL base + LoRA pixel-art-xl)
        ▼
   <id>_raw.png  (1024px, fundo sujo)
        │  (postprocess.py — rembg u2net + trim + downscale nearest)
        ▼
   <id>_cut.png / <id>_64.png / <id>_PREVIEW.png
        │  (copiar p/ Resources + reimport Unity)
        ▼
   sprite no jogo  (CaveEnemyMaterializer autoload por id)
```

Tudo em `tools/aseprite/`. Saída de geração no **scratchpad** da sessão (`...\scratchpad\ai_stab\...`), NÃO no repo até virar `<id>_64.png` aprovado.

## 2. Modelo e ambiente (fixos)

- **ComfyUI:** `D:\AI\ComfyUI` (venv em `venv\Scripts\python.exe`). Server `http://127.0.0.1:8188`.
- **Checkpoint:** `sd_xl_base_1.0.safetensors` (SDXL base — menos anime que aziib). LoRA `pixel-art-xl.safetensors` força **1.1**.
- **Steps:** **12** (decidido após comparar 12 vs 25). CFG 7, sampler euler/normal, 1024px.
- **Flags AMD obrigatórias:** `--directml --cpu-vae --disable-smart-memory`. Sem `--disable-smart-memory` o DirectML crasha (access-violation) na rotatividade de memória.
- **SSL/AV:** downloads de modelo via `curl --ssl-no-revoke`; pip com `--trusted-host pypi.org --trusted-host files.pythonhosted.org`.

## 3. Comandos (exatos)

```powershell
# (A) BUILD — sempre que mexer em manifests/overrides/build_prompts.py
cd D:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite
python build_prompts.py          # -> prompts.json (imprime "escrito prompts.json: 168 itens")

# (B) TESTE de 1-N ids (valida pose/arma antes do lote) — usa it.positive AS-IS
.\comfy_gen.ps1 -Ckpt "sd_xl_base_1.0.safetensors" -Out "<scratch>\ai_stab\raw" -Id goblin_grashnaar_scavenger -Steps 12
#   (repita -Id por criatura; ou -Cat monster -Limit 4)

# (C) LOTE completo, resumível + auto-restart do server em crash
.\supervisor.ps1 -Ckpt "sd_xl_base_1.0.safetensors" -Steps 12 -Total 168 -Out "<scratch>\ai_stab\raw"

# (D) PÓS — remove fundo + downscale (gera _cut/_64/_48/_PREVIEW)
python postprocess.py "<scratch>\ai_stab\raw" "<scratch>\ai_stab\processed" 64

# (E) WIRE inimigos — copiar _64 para Resources e reimportar no Unity
#   copy <id>_64.png -> Assets\_Game\Resources\EnemySprites\<id>.png
#   Unity: menu CindarsHope/Dev/Fix Enemy Sprite Size (reimport)  -> PPU 128
#   Play CaveScene: CaveEnemyMaterializer faz Resources.Load<Sprite>("EnemySprites/<slug>") por id.
```

> **NÃO use `comfy_sdxl_test.ps1`** (deprecated): ele re-injeta `"standing, centered"` e reintroduz a pose de mostruário. Use `comfy_gen.ps1`.

## 4. Direção de prompt (já codificada em build_prompts.py)

- **Pose = idle agressivo pronto-pra-combate** (1 frame serve idle/walk/attack; o juice anima). Mid-swing congelado é proibido (quebra parado).
- **Papel por criatura** (`role()`): `melee` / `ranged` / `caster` / `natural`. Define pose **e** negatives anti-ação-errada (melee não brilha magia; fera não segura arma). Criaturas curadas (OVR) usam `COMBAT_OVR` (não nomeia arco/cajado, pra não brigar com a arma da descrição).
- **Família coesa:** descrições com abertura compartilhada por família em `monster_overrides.json` (orcs, drow, duergar gray-skin, gnomorin maligno-cinza, void aberrations, dragões...).
- **Fundo:** prompt pede branco puro; o rembg limpa de verdade (alpha binarizado p/ borda crisp).

## 5. Wiring no Unity (estado atual)

- **Inimigos: WIRED.** `CaveEnemyMaterializer` autoload por id (`Resources/EnemySprites/<slug>.png`, slug = `enemyId` sem `enemy_`); cor branca quando sprite real; `SpriteJuice` (squash+flash, `SetBobPosition(false)`). Importer `GeneratedSpriteImporter` (PPU 128, point, bottom-center) cobre `Art/Generated/` e `Resources/EnemySprites/`. Menu `Fix Enemy Sprite Size` força PPU+reimport (determinístico).
- **NPCs / world props / player: NÃO wired.** `NpcDataSO` **não tem** campo de sprite; props entram por scene-creators; player tem ponto de injeção próprio (a achar). É o próximo trabalho de wiring.
- **Escala:** PPU 128 ⇒ inimigo médio ~1 unidade (~2 tiles, ~2× o player). 1 tile do cave = 1 unidade (confirmado por CavePlayerPathConfinement). Câmera cave ortho 7.

## 6. Validação (disciplina obrigatória)

1. **Geração:** confirme `<id>_raw.png` existe (resumível confia nisso).
2. **Pós:** confirme `<id>_64.png` + abra `<id>_PREVIEW.png` (checker) — fundo 100% limpo?
3. **Por-sprite × prompt:** cada sprite bate com a descrição (cor/arma/anatomia/papel)? O usuário exige essa checagem como passo padrão.
4. **Unicidade:** hash MD5 dos `_64.png` — 0 duplicatas (mesmo id spawnado várias vezes no jogo é esperado, não bug).
5. **No jogo:** Play na cena; o usuário roda Play Mode (o agente não roda). Reporte honesto: compile PASS ≠ visual PASS.

## 7. Pegadinhas conhecidas

- `Resources` em código de runtime resolve pro namespace `CindarsHope.Cave.Resources` → use **`UnityEngine.Resources.Load`** qualificado.
- Mudar PPU no `GeneratedSpriteImporter` **não reimporta** assets já importados → use o menu `Fix Enemy Sprite Size`.
- `dotnet build --no-restore` falha sem `project.assets.json` → rode com restore.
- Aseprite GUI não bloqueia o PowerShell → `Start-Process -Wait -PassThru` (caminho legado de placeholder; hoje a arte vem da IA).
- `supervisor.ps1` tem default `AziibPixelMix`/steps 20 — **sempre** passe `-Ckpt sd_xl_base_1.0.safetensors -Steps 12`.

## Enforcement

Revisional. Esta skill + `pixel-art-prompt-authoring` cobrem o ciclo. O `non-regression-review`/closeout checa o wiring no Unity (forbidden search, namespace). A prova é sempre o arquivo no disco + Play Mode humano.
