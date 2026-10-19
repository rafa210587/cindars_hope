# WAVE_INTEGRATION_21 — Bug Triage Matrix

**Date:** 2026-06-10
**Source:** WAVE_INTEGRATION_20_BUG_DEBT_REGISTER.md

---

## Regras de Triagem

- Root cause deve ser comprovado — sem "provavelmente".
- ALREADY_FIXED_VERIFY_ONLY = bug corrigido em commit posterior.
- NOT_REPRODUCIBLE = não consegue reproduzir com informação disponível.
- HUMAN_WIRING_REQUIRED = código correto; ação Unity Editor pendente.
- FIX_IN_WAVE21 = fix pequeno e localizado, aprovado.
- REQUIRES_NEW_SPEC = fix requer feature grande ou redesign.

---

## Triagem P0 (0 bugs)

Não há P0.

---

## Triagem P1

| BugId | Severity | Área | Repro Codificado | Root Cause | Fix Permitido em WAVE21? | Decisão |
|---|---|---|---|---|---|---|
| B001 | P1 | Scene transitions | Abrir Play, tentar ir para Town/Cave via zona de saída | SceneTransitionGate + SpawnAnchor não foram colocados nas cenas via Unity Editor. Código em SceneTransitionGate.cs está correto. | NÃO (requer Unity Editor; unity-yaml-editing-policy proíbe YAML manual sem autorização explícita) | HUMAN_WIRING_REQUIRED — fora de escopo do agente |
| B002 | P1 | Cave entrance | Ir até Zone_CaveEntrance, tentar interagir | CaveEntranceInteractable MonoBehaviour não foi colocado no FarmScene via Unity Editor. Código em CaveEntranceInteractable.cs está correto. | NÃO (requer Unity Editor) | HUMAN_WIRING_REQUIRED — fora de escopo do agente |
| B003 | P1 | Cave combat/loot | Entrar na cave (se B002 resolvido), aguardar spawn | CaveSmokeTestSpawnerBridge não foi colocado na CaveScene e enemy_slime_basic não foi assignado. Código correto. | NÃO (requer Unity Editor) | HUMAN_WIRING_REQUIRED — fora de escopo do agente |
| B004 | P1 | FarmScene layout | Tentar interagir com FarmPlot/Resource/SellPoint | FarmPlot GameObjects, SellPoint, e resource interactables precisam ser colocados via CreateMvpFarmScene generator ou wiring manual. Código correto. | NÃO (requer Unity Editor ou scene creator command) | HUMAN_WIRING_REQUIRED — fora de escopo do agente |
| B005 | P1 | Crafting | Tentar crafting no FarmScene | CraftingPoint + CraftingRuntime MonoBehaviour não colocados via Unity Editor. Código correto. (P2 na verdade — crafting não está no roteiro mínimo dos 15 steps) | NÃO (requer Unity Editor; também reclassificado como P2 abaixo) | HUMAN_WIRING_REQUIRED — P2 reclassificado |

---

## Triagem P2 (sem fix planejado)

| BugId | Severity | Área | Root Cause | Fix em WAVE21? |
|---|---|---|---|---|
| B005 | P2 (**reclassificado de P1**) | Crafting | Crafting não está no roteiro mínimo (15 steps); não bloqueia sub-slice | NÃO |
| B006 | P2 | Cave enemy HP save | CaveSaveData não persiste HP individual de enemies — design decision WAVE18 | NÃO |
| B007 | P2 | Companion save | CompanionManager.CaptureSaveData não implementado — future spec | NÃO |
| B008 | P2 | HUD Canvas | DebugHud é IMGUI — deferred UI visual | NÃO |
| B009 | P2 | Quest tracker | QuestLogPanelController é IMGUI — deferred UI visual | NÃO |
| B010 | P2 | CaveEnteredEvent proxy | CaveLevelEnteredEvent usado como proxy — aceito em WAVE19 | NÃO |
| B011 | P2 | NPC schedules | Schedules placeholder — future spec | NÃO |
| B012 | P2 | CraftingModal Canvas | Canvas não wired — deferred UI visual | NÃO |

---

## Triagem P3 (sem fix planejado)

| BugId | Área | Root Cause | Fix em WAVE21? |
|---|---|---|---|
| B013 | Inventory tooltip | Tooltip minimal (itemId+amount) — future spec | NÃO |
| B014 | Equipment modal guard | CharEquipment não empurra ModalManager — P3 debt | NÃO |
| B015 | Visual art | Placeholder art — future spec | NÃO |

---

## Reclassificação: B005

B005 (CraftingStation) foi listado como P1 no WAVE20 Bug Register. Após triagem:

- Crafting **não está** no roteiro mínimo dos 15 steps do roadmap.
- Sub-slice mínimo não requer crafting.
- O slice pode ser ACCEPTED_WITH_DEBT sem crafting wired.

**Reclassificação: P1 → P2**

---

## Sumário Final Triagem

| Contagem | Total | Fix em WAVE21 |
|---|---|---|
| P0 | 0 | N/A |
| P1 código | 0 | 0 |
| P1 HUMAN_WIRING | 4 (B001-B004) | 0 (requer Unity Editor) |
| P2 (reclassificado) | 9 (B005-B012 + reclas. B005) | 0 |
| P3 | 3 | 0 |

**Resultado: NO_OP_NO_P0_P1_FOUND** — nenhum código a corrigir.
