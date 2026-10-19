# ComfyUI na AMD 9070 XTX (RDNA4) — setup p/ geração automatizada

> Objetivo: ter o ComfyUI rodando com **API local** (`http://127.0.0.1:8188`) pra eu disparar as 164 gerações por script, sem clique.
> ⚠️ **RDNA4 é muito nova.** Espere algum atrito. Se travar, o plano B é gerar no Amuse manualmente e me dar os PNGs (eu pós-processo igual).

## Passo 1 — Instalar o ComfyUI (escolha UMA rota)

**Rota fácil (recomendada): Stability Matrix**
- Baixe o **Stability Matrix** (gerenciador GUI de ComfyUI). Ele cuida do ambiente Python e do backend AMD.
- Em "Add Package" → **ComfyUI**. Quando perguntar o backend/torch, escolha **ZLUDA** (mais rápido) ou **DirectML** (mais compatível — use se ZLUDA falhar na RDNA4).
- Isso evita mexer com Python/venv na mão.

**Rota manual (power-user): ComfyUI-Zluda**
- `git clone https://github.com/patientx/ComfyUI-Zluda` → rodar `install.bat`.
- Requer **HIP SDK (ROCm) 6.2+** instalado. Em RDNA4 confira se a versão do HIP/ZLUDA já lista o gfx do 9070 XTX; se não, use a rota DirectML.
- Fallback DirectML: ComfyUI oficial + `pip install torch-directml` e iniciar com `--directml`.

## Passo 2 — Baixar o modelo
- Pegue o **aziib pixel mix** em `.safetensors` (Civitai) — a versão checkpoint SD1.5/SDXL.
- Coloque em `ComfyUI/models/checkpoints/`.
- Anote o **nome exato do arquivo** (ex.: `aziibPixelMix_v10.safetensors`) — vou precisar dele no script.

## Passo 3 — Iniciar com a API acessível
- Inicie o ComfyUI normalmente (Stability Matrix tem botão "Launch"; manual: `run.bat` ou `python main.py`).
- A API já sobe em `http://127.0.0.1:8188`. Se precisar, adicione `--listen 127.0.0.1`.
- **Teste:** abra `http://127.0.0.1:8188` no navegador e veja a interface. Se abrir, a API está no ar.

## Passo 4 — Me avise com 2 infos
1. ComfyUI rodando e acessível em `127.0.0.1:8188` (sim/não).
2. O **nome do arquivo do checkpoint** em `models/checkpoints/`.

Aí eu: construo o **workflow de geração** (API), rodo **1 sprite de teste**, te mostro, ajusto prompt/parâmetros, e disparo o **lote dos 164**.

## O que EU faço depois (pós-processamento — igual pra qualquer fonte)
1. Gerar 512px em fundo chapado a partir do `prompts.json`.
2. **Remover fundo** (key da cor de borda) + recortar (trim).
3. **Downscale nearest** pra grade real (alvo do manifesto: 32×48 etc.).
4. **Quantizar** pra paleta (opcional — casa com nossos hex).
5. **Import Unity** (PNG + metadata).

## Limite honesto da IA: animação
A IA entrega bem o **sprite estático (idle)**. **Frames de animação consistentes** (walk/attack/hit) frame-a-frame são difíceis de manter coerentes via SD puro. Plano: IA gera o **idle de qualidade**; a animação vem depois (hand no Aseprite, ou movimento programático leve sobre o idle da IA). Decidimos isso quando a arte estática estiver boa.
