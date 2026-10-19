# Play Mode Scenario — spec_farm_scene_relayout_v4

**Data:** 2026-06-26
**Spec:** `.specs/a_implementar/spec_farm_scene_relayout_v4.md`
**Prerequisito:** Abrir Unity → CindarsHope/Inicializar Projeto → confirmar FarmScene criada → apertar Play.

---

## Pre-requisito: Regerar a FarmScene

1. Abrir o projeto no Unity Editor.
2. Menu: `CindarsHope/Inicializar Projeto` — aguardar terminar (dialogo de resumo).
3. Confirmar no Console: `MVP FarmScene created at Assets/_Game/Scenes/FarmScene.unity`.
4. Abrir a FarmScene e apertar Play.

---

## Cenario 1 — Spawn e navegacao

**Objetivo:** verificar spawn padrao e bounds v4.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Apertar Play | Jogador spawna em (8, 1) — centro-direita do mapa |
| 2 | Andar para a direita | Encontra portal azul em (17.5, 0) |
| 3 | Andar para o norte extremo | Colisao com montanha (faixa solida y≈13-17) |
| 4 | Andar para o sul extremo | Colisao com bound Bottom (y=-17) |
| 5 | Andar para o oeste extremo | Colisao com bound Left (x=-24) |
| 6 | Confirmar bosque NO | Area densa de arvores em x∈[-22,-13], y∈[0,12] |

**PASS se:** sem queda, sem teletransporte fora dos bounds, colisao da montanha solida.

---

## Cenario 2 — Rio e ponte

**Objetivo:** verificar que o rio bloqueia travessia e a ponte e andavel.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Ir para posicao (~1.5, 5) | Colisao com o rio (corpo principal) |
| 2 | Navegar para a ponte (~4, 1) | Atravessa sem colisao |
| 3 | Tentar atravessar em outros pontos do rio | Bloqueado |

**PASS se:** ponte e a unica travessia livre do rio.

---

## Cenario 3 — FarmEvolutionBoard (substitui lotes fable_41)

**Objetivo:** verificar que o Quadro de Evolucoes e interativo.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Andar ate (~14.5, 1) — junto a casa | Ver quadro de madeira |
| 2 | Pressionar [E] / interagir | Painel IMGUI abre com lista de evolucoes |
| 3 | Clicar "Fechar" | Painel fecha |
| 4 | Confirmar ausencia de lotes cercados (fable_41) | Sem cercas/placas "a venda" na cena |

**PASS se:** painel IMGUI abre/fecha; nenhum objeto FarmExpansionLot presente.

---

## Cenario 4 — Veios de minerio bloqueados

**Objetivo:** verificar feedback de recusa nos 4 nos de minerio.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Ir ate a base da montanha (~-11, 13.5) | Ver objeto de pedra escura |
| 2 | Pressionar [E] para interagir | Toast de HUD: "Minerio bloqueado — requer progressao" |
| 3 | Repetir para os outros 3 nos (-4, 4, 11) | Mesmo feedback em todos |

**PASS se:** 4 nos respondem com feedback de recusa.

---

## Cenario 5 — Aragem por tile (FarmTillingInputController)

**Objetivo:** verificar que pressionar [F] no campo aravel cria toast de "Solo arado!".

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Andar para area aberta (ex: (0, 0)) | Tile deve ser aravel (fora de agua/montanha/construcao) |
| 2 | Pressionar [F] | Toast: "Solo arado!" OU "Nao e possivel arar aqui" (se tile invalido) |
| 3 | Pressionar [F] novamente no mesmo tile | Toast sobre regar ou "nao possivel" |
| 4 | Ir para cima da montanha ou do rio | Pressionar [F] — esperado "Nao e possivel arar aqui" |

**PASS se:** [F] responde com feedback correto; tiles de montanha/rio recusados.

---

## Cenario 6 — Casa com cama (sem craft dentro)

**Objetivo:** verificar que a casa so tem cama, BedLetter e (opcional) bau; sem craft stations dentro.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Ir ate a regiao (14-20, 2.5-7.5) | Area da casa (v4 — leste do mapa) |
| 2 | Interagir com a cama (~13.5, 4.2) | Dialogo de dormir |
| 3 | Interagir com a carta (~14, 4.2) | Texto da carta |
| 4 | Confirmar que craft stations estao fora (~10-13, -1) | Workbench/Forge/Cooking fora da casa |

**PASS se:** dormir e carta funcionam; craft stations nao estao dentro da casa.

---

## Cenario 7 — Spawn da caverna

**Objetivo:** verificar que CaveEntrance e Board_Zrix estao na posicao v4.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Andar ate (-15, 10.5) | Entrada de caverna visivel (base montanha, canto NO) |
| 2 | Andar ate (-13, 9.5) | Board_Zrix visivel (quadro roxo) |
| 3 | Interagir com Board_Zrix | Prompt de contratos de caverna |

**PASS se:** caverna e board acessiveis em posicoes NO corretas.

---

## Cenario 8 — Ausencia de regressoes

**Objetivo:** verificar que sistemas existentes nao regrediram.

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Interagir com FonteAnya (~15.5, 2) | Respawn point registrado |
| 2 | Interagir com FishingSpot (~9, -7.5) | Prompt de pesca |
| 3 | Interagir com ShippingBin (~11.5, 1.5) | Prompt de envio |
| 4 | Interagir com SellPoint (~8.5, 2.5) | UI de venda |
| 5 | Interagir com forageio (ex: -14, -2) | Coleta sazonal ou prompt |
| 6 | Interagir com Coop_01 / Barn_01 | Prompt de animais |
| 7 | Abrir um craftingStation (ex: Workbench ~10,-1) | Modal de craft abre |
| 8 | Salvar e carregar (Ctrl+S → recarregar) | Posicao salva no spawn default |

**PASS se:** todos esses sistemas respondem normalmente.

---

## Resultado esperado

Todos os 8 cenarios PASS = spec validada em Play Mode.
Registrar resultado em `docs/validation/spec_farm_scene_relayout_v4_execution_report.md`
(campo "Validation" — atualizar "NOT RUN" para "PASS" ou "FAIL com evidencia").
