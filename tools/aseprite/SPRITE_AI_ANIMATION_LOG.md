# Log de Experimentos — Geração e Animação de Sprites por IA Local

> Registro honesto do que foi testado para gerar e animar sprites do jogo com IA
> **local** (ComfyUI + SDXL na GPU AMD/DirectML). Documenta o que **funciona**, o que
> **não funciona** e **por quê**, para não repetirmos becos sem saída.
> Atualizado em 2026-06-28.

---

## 1. Objetivo

Gerar sprites do jogo (player, NPCs, monstros, props) no estilo **"Children of Morta leve"**
(pixel art detalhado mas jogável, proporção levemente estilizada) e **animá-los**
(idle, walk, run, attack, defend) nas 4/8 direções — tudo local e grátis.

## 2. O que FUNCIONA (provado)

| Item | Método | Status |
|---|---|---|
| **Sprites-base** (1 frame por sujeito) | SDXL + LoRA pixel-art-xl, prompt por sujeito | ✅ ótimo |
| **Estilo travado** | "pixel art game sprite, clean silhouette, soft neutral lighting, cel shading" (NÃO "dramatic lighting/volumetric" — isso vira ilustração) | ✅ |
| **Proporção compacta** | esqueleto encurtado (`compact_pose`, leg=0.60, head=1.22) — proporção vem do esqueleto, não do prompt | ✅ |
| **Downscale de qualidade** | LANCZOS + quantize de paleta (NÃO nearest — desperdiça a alta-res) | ✅ |
| **Poses de AÇÃO** (attack/defend/cast/gestos — poses de braço/corpo) | **IP-Adapter** (trava identidade do frame-âncora) + **ControlNet** (pose do esqueleto) + txt2img | ✅ funciona |
| **Batch estático** | 39 NPCs (idle frontal) + 65 props gerados | ✅ feito (estilo antigo — regerar) |

## 3. O que NÃO funciona (e por quê) — o problema é o WALK

O movimento de pernas com **lift** (pé saindo do chão) foi testado por **9 caminhos** e
nenhum fechou com qualidade consistente:

| # | Abordagem | Resultado |
|---|---|---|
| 1 | txt2img puro (prompt de ação) | frontal/parado; modelo ignora a pose pedida no texto |
| 2 | img2img de **foto realista** | pose boa mas vira "foto pixelada" (realista demais) |
| 3 | **ControlNet** esqueleto (pose ditada) | pose pega, mas o **pé não levanta** no walk (viés do modelo) |
| 4 | **cn_i2i** (img2img encadeado do frame anterior) | identidade propaga, mas cabo-de-guerra: denoise baixo mantém id e ignora pose nova; alto muda pose e perde id |
| 5 | **IP-Adapter + ControlNet** | **ações OK** (attack ✅); walk ainda **sem lift** |
| 6 | **LPC → DWPose** (extrair esqueleto do LPC) | DWPose **não detecta** sprite pixel art (treinado em fotos de pessoas; detector yolox não vê "pessoa" no boneco) — testado com/sem detector e suavizado: vazio |
| 7 | **LPC como base de img2img** | **movimento das pernas TRANSFERE** (andam!) — mas identidade **instável** entre frames (sem trava) |
| 8 | LPC img2img + **IP-Adapter forte** | conflito de **orientação**: IP-Adapter (ref frontal) impõe frontalidade sobre a pose lateral do LPC → poses quebradas |
| 9 | LPC img2img + IP-Adapter leve / prompt-forte + seed fixo | ainda instável (frames frontais, cortados, ou identidade variando) |

**Causa-raiz:** o SDXL é um modelo de **geração de imagem**, não de **animação**. Ele
**resiste a tirar o pé do chão** independente do método. Não há modelo local/open-source
maduro que anime pixel art (os que fazem — PixelLab, Pixel Engine — são cloud, fechados).

## 4. Avanço importante (a peça que destravou as AÇÕES)

O **IP-Adapter** (`ComfyUI_IPAdapter_plus`) resolveu a **consistência de identidade**:
ele trava a aparência do personagem-âncora independente da pose. Com ele,
ControlNet (pose) + IP-Adapter (identidade) gera o **mesmo personagem em poses distintas**
— provado num **attack de 3 frames** (windup→swing→impacto). É o caminho das **ações**.

O **LPC** (Universal LPC Spritesheet) provou que, como **base de img2img**, o
**movimento de pernas transfere** (o pé no ar do LPC persiste na imagem) — o que o
ControlNet-esqueleto não conseguia. Falta só estabilizar identidade+orientação.

## 5. Ferramentas instaladas (ComfyUI, local)

- **ControlNet OpenPose SDXL (xinsir)** — `models/controlnet/controlnet-openpose-sdxl-xinsir.safetensors`
- **IP-Adapter** — `ComfyUI_IPAdapter_plus` + `ip-adapter-plus_sdxl_vit-h` + `CLIP-ViT-H-14`
- **DWPose / controlnet_aux** — instalado, mas **não serve** para sprite pixel art (ver #6 acima). Rodar ComfyUI com `HF_HUB_OFFLINE=1`. NÃO instalar `onnxruntime-gpu` (é CUDA; usar onnxruntime CPU). NÃO deixar reinstalar torch (quebra DirectML).
- Scripts: `tools/aseprite/` — `build_prompts.py`, `postprocess.py`; e (scratchpad da sessão) `cn_pilot.py` (modos: cn, img2img, cn_i2i, ip_cn, ip_i2i, pose), `pose_skel.py` (esqueletos OpenPose por PIL).

## 6. Estratégia atual (decidida com o humano, 2026-06-28)

Produção **por partes**, garantindo **unicidade do personagem primeiro**:

- **FASE 1 — Poses-base (idle) por direção**, todas o MESMO personagem:
  ↓ frontal (mestre, já temos) · → direita (perfil) · ↑ costas · ← = flip da direita · diagonais depois.
  Método: ControlNet (esqueleto da direção) + IP-Adapter (identidade do mestre).
- **FASE 2 — De cada pose-base aprovada, animar** (walk e depois ações).
  Walk: abordagem aberta (LPC img2img é a mais promissora; precisa estabilizar id+orientação).

**Regra:** não animar antes da pose-base estar aprovada. Não gerar tudo de uma vez.

## 7. Decisões de produto (HISTÓRICO — superado pela seção 8)

- **Local e grátis** era a meta; **walk** ficou em aberto no SDXL local (9 tentativas, sem lift).

## 8. RESOLUÇÃO (2026-06-28) — pipeline híbrido que FECHA o walk

O SDXL local não anima pixel art de forma consistente (ver seção 3). A solução que
**funcionou de ponta a ponta** divide o trabalho entre **geração cloud** e **pós local**:

```
GPT (gpt-image-1, master sheet) -> rembg (tira fundo) -> slicer (fatia frames por banda/gap)
  -> downscale LANCZOS 64px -> frames PNG transparentes -> GIF/Unity
```

**Por que o GPT e não o SDXL local:** o ChatGPT (operado via Chrome MCP na conta do humano,
descrição conversacional "fazendeiro estilo Stardew + Children of Morta, anda em todas as
direções") gera **a mesma identidade em ciclo completo de walk nas 3 direções, com perfil
lateral verdadeiro e pé levantando** — o exato deliverable que o SDXL recusou. Não há prompt
mágico: consistência do elenco vem de **anexar uma sheet aprovada como referência**.

**Provado em conteúdo real (2026-06-28):**
- Walk sheet (down 8f / perfil 8f / costas 7f) e combat sheet (attack/defend/run up + laterais)
  fatiadas, fundo removido, downscaladas a 64px — identidade consistente, qualidade jogável.
- O **fundo do GPT é não-uniforme** (gradiente/glow) → keying por branco falha; **rembg (u2net)**
  resolve. GPT costuma ignorar "fundo transparente"; sempre passar por rembg.

**Papéis:**
- **Cloud (GPT)** = gerar masters (player, NPCs, monstros) onde precisa walk/perfil/consistência.
- **Local (grátis)** = todo o pós: rembg + slicer + downscale + wiring Unity + variações via
  IP-Adapter (âncora = master do GPT). O pipeline SDXL/IP-Adapter continua útil pra variação/ação.

**Scripts (scratchpad da sessão, a promover p/ `tools/aseprite/`):** `slice_sheet.py`
(remove_bg flood-fill OU `--skip_bg` p/ entrada já keyada; detecta bandas/linhas, fatia por
gap, normaliza célula, downscale, GIF por linha), `run_rembg.py`.

**Tweaks pendentes do slicer:** filtrar rótulo laranja (texto) das bandas; separar frames quando
um rastro de espada conecta caracteres (usar projeção de cor de pele em vez de alpha bruto).

**Próximo:** fechar o loop no Unity (importar uma anim, Animator, ver in-engine); depois batch do
elenco (GPT gera com referência -> pipeline processa em lote).
