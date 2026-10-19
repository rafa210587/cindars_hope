# WAVE_INTEGRATION_26 — Human Play Mode Checklist

Date: 2026-06-11
Status: PENDING_HUMAN_PLAYMODE

Execute este checklist em Unity Editor Play Mode apos aplicar scene wiring (veja instrucoes abaixo).

---

## Pre-requisitos de Wiring (antes de Play Mode)

**P1 — npc_pip scene wiring:**
1. No TownScene, encontrar o GameObject do NPC "Pip Semente-Solta"
2. Adicionar componente QuestGiverInteractable
3. _npcId = "npc_pip"
4. _offeredQuestIds = ["quest_tools_for_the_town"]
5. Salvar a cena

**P2 — npc_maelor scene wiring:**
1. No TownScene, encontrar o GameObject do NPC "Maelor Cinza"
2. Adicionar componente QuestGiverInteractable
3. _npcId = "npc_maelor"
4. _offeredQuestIds = ["quest_echo_from_the_cave"]
5. Salvar a cena

---

## Checklist de Play Mode

### Setup
- [ ] Abrir TownScene ou FarmScene — sem erros vermelhos no Console
- [ ] Entrar em Play Mode — Player, HUD, QuestLog (J key) funcionam
- [ ] Console mostra `[QuestRuntimeBootstrap] Quest runtime initialized. Registry quests: 3`
- [ ] Console mostra `[QuestProgressEventBridge] Subscribed to gameplay events (WAVE15 + WAVE26 objective variety)`

### Quest 1 — Suprimentos para Cindar (CollectItem)
- [ ] Interagir com Thalindra (npc_thalindra) — opcao de quest aparece
- [ ] Aceitar quest — QuestLog (J) mostra quest como ativa com 2 objetivos
- [ ] Coletar 2 itens wood (item_material_wood) — objetivo progride no QuestLog
- [ ] Coletar 2 itens stone (item_material_stone) — objetivo progride no QuestLog
- [ ] Quest fica ReadyToComplete
- [ ] Interagir com Thalindra novamente — opcao TurnIn aparece
- [ ] Confirmar entrega — recebe Gold 50 (verificar no HUD)
- [ ] QuestLog mostra quest como Completed
- [ ] Interagir com Thalindra novamente — modo NoQuest (nao oferece de novo)

### Quest 2 — Ferramentas para a Cidade (SellItem)
NOTA: Requer npc_pip wiring (P1 acima).
- [ ] Interagir com Pip (npc_pip) — quest offer aparece (prereq Q1 = DONE)
- [ ] Aceitar quest — QuestLog mostra quest 2 como ativa
- [ ] Ir ate o SellPoint (banca de venda na FarmScene) — vender qualquer item
- [ ] Console mostra `[QuestService] Quest completed: quest_tools_for_the_town` (ou similar)
- [ ] QuestLog mostra Quest 2 como ReadyToComplete
- [ ] Interagir com Pip novamente — TurnIn disponivel
- [ ] Confirmar entrega — recebe Gold 30
- [ ] Interagir com Pip novamente — NoQuest

### Quest 3 — Eco das Cavernas (ReachCaveDepth)
NOTA: Requer npc_maelor wiring (P2 acima) E cave entrance wiring (WAVE16 debt).
- [ ] Interagir com Maelor (npc_maelor) — quest offer aparece (prereq Q2 = DONE)
- [ ] Aceitar quest — QuestLog mostra quest 3 como ativa
- [ ] Entrar nas cavernas (cave entrance interactable, WAVE16)
- [ ] Console mostra `[QuestService] Quest completed: quest_echo_from_the_cave`
- [ ] Quest 3 ReadyToComplete
- [ ] Interagir com Maelor — TurnIn disponivel
- [ ] Confirmar entrega — recebe Gold 60
- [ ] Chain completo

### Save/Load
- [ ] Aceitar quest 1 (se nao completada), Save (S key ou Save menu)
- [ ] Sair do Play Mode, entrar novamente — Load
- [ ] QuestLog mostra quest 1 como ativa com progresso preservado
- [ ] Completar objetivo e TurnIn apos load — reward dado uma vez
- [ ] Tentar TurnIn novamente — reward NAO duplica

### Reward Idempotency
- [ ] Completar e entregar Quest 1 — Gold 50 dado
- [ ] Tentar interagir com Thalindra de novo para quest 1 — modo NoQuest (nao TurnIn)
- [ ] Verificar que Gold nao aumentou de novo

### QuestLog UI
- [ ] Pressionar J — QuestLog abre
- [ ] Ativas e Completadas aparecem separadas
- [ ] Progresso de objetivo aparece (ex: "1/2" ou "Completo")
- [ ] Pressionar Escape — QuestLog fecha

---

## Debts Documentados (nao bloqueia checklist basico)

| Debt | Impact no Checklist |
|------|---------------------|
| SCENE_WIRING_DEBT (Pip, Maelor) | Quests 2 e 3 nao podem ser oferecidas sem wiring; checklist Q2/Q3 requer P1/P2 |
| WAVE16 cave entrance debt | Quest 3 objetivo nao pode ser completado sem cave entrance wired |
| PREREQUISITE_UI_DEBT | Player pode aceitar Q2 antes de Q1 — sequencia de quest pode ser fora de ordem |

---

## Status apos Play Mode

Preencher apos execucao:

```
Data de execucao:
Executor:
Q1 (CollectItem + TurnIn): PASS / FAIL
Q2 (SellItem + TurnIn): PASS / FAIL / BLOCKED_BY_SCENE_WIRING
Q3 (CaveDepth + TurnIn): PASS / FAIL / BLOCKED_BY_SCENE_WIRING / BLOCKED_BY_CAVE_WIRING
Save/Load round-trip: PASS / FAIL
Reward idempotency: PASS / FAIL
QuestLog UI: PASS / FAIL
Final status: PLAYMODE_VALIDATED / PARTIAL_PLAYMODE_VALIDATED_WITH_SCENE_WIRING_DEBT
```
