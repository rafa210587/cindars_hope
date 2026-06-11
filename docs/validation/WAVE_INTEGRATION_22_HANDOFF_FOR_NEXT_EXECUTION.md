# WAVE_INTEGRATION_22 — Handoff para Próxima Execução

**Date:** 2026-06-10
**Para:** Humano, Claude Code, Codex
**Branch:** dev

---

## Estado Atual

```
Playable slice:   BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE
WAVE13-22:        Todos executados (WAVE13-19: CODE_READY; WAVE20: closeout; WAVE21: NO_OP; WAVE22: DOC)
P0 abertos:       0
P1 código:        0
P1 wiring:        4 (B001-B004, requer Unity Editor)
P2 canônicos:     13
P3 canônicos:     4 (inclui 1 FIX_NOW_TRIVIAL)
Debts aceitos:    1 (DEBT-CAVE-002)
Debts obsoletos:  6 (resolvidos em waves anteriores)
```

---

## O Que NÃO Refazer

```
NÃO recriar sistemas de save/load — SaveManager está funcional (schema v5)
NÃO recriar InventoryManager — funcional e testado
NÃO recriar QuestService — funcional com reward idempotency
NÃO recriar ModalManager — stack completo e funcional
NÃO recriar Dash/Dodge/Block — PlayerMovementActionRuntimeBootstrap funcional
NÃO recriar HUD — DebugHud funcional (IMGUI); substituir, não recriar
NÃO recriar NPC system — 23 NPCs wired em TownScene com NpcDataSO + DialogueTreeSO
NÃO recriar cave runtime — CaveRunManager/CaveLevelRuntimeController funcional
NÃO recriar GameEventBus — funcional; nunca contornar com direct calls
NÃO recriar GameBootstrap — injeção de dependência funcional
NÃO recriar CreateMvpFarmScene — já criado; executar menu no Unity Editor
```

---

## Specs que Já Existem (Executadas)

| Wave | Spec | Status | Tipo |
|---|---|---|---|
| WAVE01 | Hardening | BUILD_VALIDATED | Code |
| WAVE02 | Time/Calendar/Save | BUILD_VALIDATED | Code |
| WAVE03 | Quests/Events | BUILD_VALIDATED | Code |
| WAVE04 | UI Foundation | BUILD_VALIDATED | Code |
| WAVE05 | Farm Gameplay Core | BUILD_VALIDATED | Code |
| WAVE06 | Economy/Loot/Crafting | BUILD_VALIDATED | Code |
| WAVE07 | Scene Integration | BUILD_VALIDATED | Code |
| WAVE08 | City/NPC/Dialogue | BUILD_VALIDATED | Code |
| WAVE09 | Quest System | BUILD_VALIDATED | Code |
| WAVE10 | Progression/Fonte | BUILD_VALIDATED | Code |
| WAVE11 | UI/HUD/Inventory | BUILD_VALIDATED | Code |
| WAVE12 | Final Validation | BUILD_VALIDATED | Docs |
| WAVE_INT_06A-12C | Integração base | BUILD_VALIDATED | Code |
| WAVE_INT_13 | Scene Transitions | CODE_READY_HUMAN_WIRING | Code+Wiring |
| WAVE_INT_14 | Crafting | CODE_READY_HUMAN_WIRING | Code+Wiring |
| WAVE_INT_15 | Quest Giver/Log | CODE_READY_HUMAN_WIRING | Code+Wiring |
| WAVE_INT_16 | Cave Entrance | CODE_READY_HUMAN_WIRING | Code+Wiring |
| WAVE_INT_17 | Cave Combat/Loot | CODE_READY_HUMAN_WIRING | Code+Wiring |
| WAVE_INT_18 | Save/Load Gap | BUILD_VALIDATED | Code |
| WAVE_INT_19 | HUD/UX Gate | BUILD_VALIDATED | Code |
| WAVE_INT_20 | Playable Slice Closeout | BUILD_VALIDATED | Docs |
| WAVE_INT_21 | Post-Acceptance Bugfix | NO_OP | Docs |
| WAVE_INT_22 | Debt Backlog + Roadmap | DOC_ONLY | Docs |

---

## Specs a Executar a Seguir

### Ação Imediata (humano, sem código)

```
1. Abrir Unity Editor
2. Rodar CindarsHope/Integration/Create MVP Farm Scene
3. Seguir WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md
4. Seguir WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md
5. Rodar Play Mode, executar WAVE20 checklist (56 steps)
6. Preencher WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md
```

### Primeira Spec de Código MVP+

```
MVP_PLUS_04_equipment_modal_guard_fix  (trivial, independente)
→ Fix: CharacterEquipmentPanelController.Open() → ModalManager.PushModal(ModalType.CharacterEquipment)
→ Fix: CharacterEquipmentPanelController.Close() → ModalManager.TryPopModal(ModalType.CharacterEquipment)
→ Build + EditMode test
→ Commit
```

---

## Fontes Primárias a Ler

| Quando | Leia |
|---|---|
| Sempre | CLAUDE.md, docs/project/CURRENT_STATE.md |
| Para debug/wiring | docs/validation/WAVE_INTEGRATION_XX_HUMAN_UNITY_*_INSTRUCTIONS.md |
| Para nova spec | docs/design/SPEC_SOURCE_MAP.md, docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md |
| Para debt specific | docs/validation/WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md |
| Para próxima spec | docs/validation/WAVE_INTEGRATION_22_NEXT_SPEC_CANDIDATE_MATRIX.md |
| Para roadmap | docs/validation/WAVE_INTEGRATION_22_NEXT_ROADMAP_PROPOSAL.md |

---

## Como Decidir Próxima Spec

```
1. Verificar CURRENT_STATE.md para estado atual
2. Verificar WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md para debts abertos
3. Verificar WAVE_INTEGRATION_22_NEXT_SPEC_CANDIDATE_MATRIX.md para candidatas
4. Escolher pelo critério:
   a. Se Play Mode não executado → executar sub-slice primeiro (humano)
   b. Se P1 WIRING pendente → executar wiring manual (humano)  
   c. Se Fix trivial disponível → MVP_PLUS_04 (equipment modal guard)
   d. Se wiring completo → MVP_PLUS_01 (HUD Canvas)
5. Criar spec formal SpecKit-style ANTES de implementar
```

---

## Como Usar o Debt Register

```
1. Abrir: docs/validation/WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md
2. Filtrar por "Prioridade: NOW" → ação imediata necessária
3. Filtrar por "Disposition: NEXT_SPEC_REQUIRED" → criar spec nova
4. Filtrar por "Disposition: FIX_NOW_TRIVIAL" → fix direto sem spec completa
5. Filtrar por "Disposition: NEEDS_HUMAN_DECISION" → decisão humana necessária
6. Filtrar por "Disposition: ACCEPTED_DEBT" → não mexer
```

---

## Invariantes do Projeto (NUNCA Violar)

```
- Nenhum GameObject.Find / FindObjectOfType em runtime
- Toda comunicação de gameplay via GameEventBus
- Save DTOs apenas com tipos simples (sem Unity refs)
- Commits em português
- Não commitar sem build PASS (exit code 0 em ambos os assemblies)
- Não marcar ACCEPTED sem Play Mode humano
- Não editar .unity/.prefab/.asset manualmente sem autorização de spec
- Não alterar Packages/ProjectSettings sem autorização
```

---

## Contatos e Referências

```
Projeto:  Cindar's Hope / Vaalara
Branch:   dev
Remote:   origin/dev
Rules:    .claude/rules/RULES.md (16 regras ativas)
Commands: .claude/commands/ (11 comandos)
Skills:   .claude/skills/ (15 skills)
Agents:   .claude/agents/ (7 agents)
```
