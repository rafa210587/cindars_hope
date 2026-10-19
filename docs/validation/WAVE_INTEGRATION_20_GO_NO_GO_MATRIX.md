# WAVE_INTEGRATION_20 — Go/No-Go Matrix

**Date:** 2026-06-10
**Status:** `BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE`

---

## Gate Results (Build Evidence)

| Gate | Required | Code Evidence | Build Result | Play Mode | Go? |
|---|---|---|---|---|---|
| Build runtime | Sim | Assembly-CSharp.csproj | PASS (0E/0W) | N/A | GO |
| Build editor | Sim | Assembly-CSharp-Editor.csproj | PASS (0E/3W pre-existing) | N/A | GO |
| Unity opens | Sim | N/A | NOT RUN | PENDING | PENDING |
| Player movement | Sim | PlayerController.cs | CODE_READY | PENDING | PENDING |
| Farm loop | Sim | FarmPlot.cs + CropGrowthProcessor + FarmWateringService | CODE_READY | PENDING | PENDING |
| Inventory | Sim | InventoryPanelController (RuntimeInitializeOnLoad) | BUILD_VALIDATED | PENDING | PENDING |
| Economy | Sim | SellPoint + EconomyTransactionCompletedEvent | BUILD_VALIDATED | PENDING | PENDING |
| Quest | Sim | QuestService + QuestRuntimeBootstrap + QuestLogPanelController | BUILD_VALIDATED | PENDING | PENDING |
| Skill | Sim | SkillTreeGameplayPanelController + PlayerMovementActionRuntimeBootstrap | BUILD_VALIDATED | PENDING | PENDING |
| Cave | Sim | CaveEntranceInteractable + CaveRuntimeBridge + CaveSmokeTestSpawnerBridge | CODE_READY_HUMAN_WIRING | PENDING | PENDING |
| Save/load | Sim | SaveManager + QuestStateSectionSaveData + GameLoadedEvent | BUILD_VALIDATED | PENDING | PENDING |
| Modal guards | Sim | HasActiveModal em Dash+Dodge+Block+DoubleTap | BUILD_VALIDATED | PENDING | PENDING |
| P0 bugs | zero | Nenhum P0 encontrado em auditoria de código | ZERO (code audit) | PENDING | PENDING |
| P1 bugs | zero | 5 P1 identificados — TODOS HUMAN_WIRING_REQUIRED (não code bugs) | NOT_ZERO_WIRING | PENDING | PENDING |
| P2 accepted debt | permitido | 8 P2 documentados | DOCUMENTED | N/A | GO (documentado) |

---

## Interpretação dos Gates PENDING

Os gates com `PENDING` requerem execução humana do checklist em Play Mode.

### O que muda o status de PENDING para GO/NO-GO:

| Gate | Condição GO | Condição NO-GO |
|---|---|---|
| Unity opens | Console sem erro vermelho | Erro vermelho persistente → P0 |
| Player movement | Player anda + camera segue | Player não aparece → P0 |
| Farm loop | Plot/resource/harvest funciona (com wiring) | Crash ao interagir → P0/P1 |
| Inventory | I abre inventory, Esc fecha | Inventory não abre → P1 |
| Economy | SellPoint vende, gold muda | Gold não muda → P1 |
| Quest | Quest aceita, log mostra, entrega funciona | Quest não aparece → P1 |
| Skill | SkillTree abre, dash/dodge/block executam | Skill não equipa → P1 |
| Cave | Cave entra, enemy spawna, loot cai | Cave não carrega (com wiring) → P1 |
| Save/load | F5/F9 funcionam, quest persiste | Quest perde após load → P1 |
| Modal guards | Dash bloqueado durante modal | Dash fura modal → P1 |
| P0 bugs | 0 P0 encontrados | Qualquer P0 → REJECTED |
| P1 bugs | 0 P1 código encontrados | P1 código → REJECTED |

---

## Resultado Final Esperado

**Sem Play Mode (status atual):**
```
GO: Build runtime, Build editor, P2 debt
PENDING: Todos os outros (requerem Play Mode humano)
RESULTADO: BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE
```

**Após Play Mode (sub-slice sem wiring manual):**
```
Esperado: steps 1-5, 12-14, 33-36, 46-53 passam → ACCEPTED_WITH_DEBT
Condição: P0=0, P1=0 em código (P1 HUMAN_WIRING é debt aceito para sub-slice)
```

**Após Play Mode completo (com wiring):**
```
Esperado: steps 1-56 passam → ACCEPTED_WITH_DEBT
Condição: P0=0, P1=0 código, P1 wiring resolvido pelo humano
```

---

## Preencher Após Play Mode

```
Data:
Testador:
Resultado global: GO / NO-GO
Decisão: ACCEPTED / ACCEPTED_WITH_DEBT / REJECTED_BLOCKED
Notas:
```
