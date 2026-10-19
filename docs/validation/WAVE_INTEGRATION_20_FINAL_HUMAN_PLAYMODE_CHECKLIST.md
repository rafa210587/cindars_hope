# WAVE_INTEGRATION_20 — Final Human Play Mode Checklist

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_EXECUTION

---

## Pré-condições

1. Unity Editor aberto, sem erros vermelhos no Console.
2. Branch `dev` atualizado: `git pull origin dev`.
3. FarmScene como cena inicial.
4. Build Settings: FarmScene indexada como Scene 0.
5. Criar objetos de cena se necessário: `CindarsHope/Integration/Create MVP Farm Scene`.

---

## Pré-wiring Recomendado (antes de Play)

Para testar o slice completo, o humano deve executar as seguintes ações de wiring no Unity Editor antes de Play (não são code bugs — são HUMAN_WIRING_REQUIRED):

| Ação | WAVE | Instruções |
|---|---|---|
| Farm→Town SceneTransitionGate | 13 | `docs/validation/WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md` |
| Farm Build Settings | 13 | `docs/validation/WAVE_INTEGRATION_13_HUMAN_UNITY_BUILD_SETTINGS_INSTRUCTIONS.md` |
| CraftingStation em FarmScene | 14 | `docs/validation/WAVE_INTEGRATION_14_HUMAN_UNITY_CRAFTING_WIRING_INSTRUCTIONS.md` |
| CaveEntranceInteractable em FarmScene | 16 | `docs/validation/WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md` |
| CaveSmokeTestSpawnerBridge em CaveScene | 17 | `docs/validation/WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md` |

Estes pré-wirings habilitam os steps 10-22. Os steps 1-9 e 23-26 funcionam sem eles.

---

## Checklist

| Step | Ação | Resultado Esperado | Pass/Fail | Bug ID | Notas |
|---|---|---|---|---|---|
| **BOOT E FARM** |  |  |  |  |  |
| 1 | Abrir Unity Editor | Console sem erro vermelho persistente | | | P0 se houver erro |
| 2 | Abrir FarmScene | Cena abre sem erros | | | |
| 3 | Press Play | Player spawna no FarmScene | | | |
| 4 | Andar com WASD/Arrows | Player se move, camera segue | | | |
| 5 | DebugHud visível | Gold/HP/Stamina/Hotbar mostrados | | | |
| **FARM INTERACTIONS** |  |  |  |  |  |
| 6 | Aproximar de FarmPlot | Prompt aparece: "Interacao: Plantar/Regar/Colher" | | | Requer FarmPlot wired |
| 7 | Plantar semente no plot | Plot muda para estado planted | | | Requer watering can no inventory |
| 8 | Regar plot plantado | Estado muda para watered | | | |
| 9 | Avançar dia (se disponível) / esperar | Cultura cresce (CropGrowthProcessor) | | | |
| 10 | Colher cultura pronta | Item entra no inventory | | | |
| 11 | Interagir com recurso (árvore/rocha/forragem) | Item entra no inventory | | | Requer resource interactable wired |
| **INVENTORY E ECONOMY** |  |  |  |  |  |
| 12 | Pressionar I | Inventory abre com itens | | | RuntimeInitializeOnLoad — sem wiring |
| 13 | Tentar dash (Space) com inventory aberto | Dash NÃO executa | | | |
| 14 | Pressionar Escape | Inventory fecha | | | |
| 15 | Aproximar de SellPoint (shipping bin) | Prompt de venda aparece | | | Requer SellPoint wired |
| 16 | Vender item | Gold aumenta, item sai do inventory | | | |
| 17 | Feedback HUD: "Vendido! +X ouro." | Mensagem visível no DebugHud | | | |
| **TOWN E NPC** |  |  |  |  |  |
| 18 | Ir para saída da farm (zone_exit_town) | Transição para TownScene | | | Requer WAVE13 wiring |
| 19 | TownScene carrega sem erro | Cena carrega, player spawna | | | |
| 20 | Falar com Thalindra | Diálogo abre com: "! Qual é a tarefa?", "Comprar", "Vender", "Adeus" | | | |
| 21 | Selecionar "Comprar" | Shop buy panel abre | | | |
| 22 | Comprar item | Inventory recebe item, gold reduz | | | |
| 23 | Tentar dash durante shop aberto | Dash NÃO executa | | | |
| 24 | Pressionar Escape | Shop fecha | | | |
| **QUEST FLOW** |  |  |  |  |  |
| 25 | Falar com Thalindra → "! Qual é a tarefa?" | QuestOffer panel abre com detalhes | | | |
| 26 | Aceitar quest | Feedback HUD: "Quest aceita: {questId}" | | | |
| 27 | Pressionar J (QuestLog) | Quest log abre mostrando quest ativa | | | |
| 28 | Tentar dash com quest log aberto | Dash NÃO executa (ModalType.QuestLog) | | | |
| 29 | Pressionar Escape | Quest log fecha | | | |
| 30 | Completar objetivos da quest (coletar itens) | Feedback HUD: "Pronto para entregar: {questId}" | | | |
| 31 | Voltar à Thalindra e entregar quest | Feedback HUD: "Quest concluída: {questId}" + "Recompensa: Ng" | | | |
| 32 | Entregar de novo (idempotência) | Reward NÃO aplica segunda vez | | | |
| **SKILL TREE** |  |  |  |  |  |
| 33 | Pressionar U | Skill tree abre com nodes visíveis | | | RuntimeInitializeOnLoad — sem wiring |
| 34 | Comprar node de skill | Node marcado como comprado | | | |
| 35 | Equipar active skill em slot R/T/Y/G | HUD mostra skill no slot | | | |
| 36 | Usar skill equipada (ex: Dash = Space) | Efeito real ocorre (deslocamento) | | | |
| **CAVE (SE WIRED)** |  |  |  |  |  |
| 37 | Ir para CaveEntrance em FarmScene | Prompt aparece: "Entrar na Caverna" | | | Requer WAVE16 wiring |
| 38 | Interagir e confirmar | Feedback HUD: "Entrando na caverna (nível 1)" | | | |
| 39 | CaveScene carrega | Player spawna na cave | | | |
| 40 | Enemy aparece (slime básico) | Enemy visível e perseguindo | | | Requer WAVE17 wiring |
| 41 | Atacar enemy (Q/E keys) | Enemy toma dano | | | |
| 42 | Enemy derrotado | Feedback HUD: "Inimigo derrotado: ... \| Loot: ..." | | | |
| 43 | Loot entra no inventory | item_material_stone visível no inventory | | | |
| 44 | Interagir com saída da cave | Feedback HUD: "Retornando à superfície → FarmScene" | | | |
| 45 | FarmScene carrega | Player volta ao ponto de spawn da farm | | | |
| **SAVE/LOAD** |  |  |  |  |  |
| 46 | Pressionar F5 (Save) | Feedback HUD: "Jogo salvo." | | | |
| 47 | Pressionar F9 (Load) | Feedback HUD: "Jogo carregado." | | | |
| 48 | Gold/inventory mantidos após load | Estado restaurado corretamente | | | |
| 49 | Quest ativa mantida após load | Quest ainda aparece no QuestLog (J) | | | |
| 50 | Skill equipada mantida após load | Slot de skill mantido | | | |
| **MODAL GUARDS** |  |  |  |  |  |
| 51 | Abrir inventory (I) + tentar Dodge (DoubleTap) | Dodge NÃO executa | | | |
| 52 | Abrir inventory (I) + tentar Block (Shift) | Block NÃO ativa | | | |
| 53 | Fechar inventory (Esc) + tentar Dash | Dash EXECUTA normalmente | | | |
| **BUG VERIFICATION** |  |  |  |  |  |
| 54 | Verificar Console | Sem NullReferenceException recorrente | | | P0 se houver |
| 55 | Verificar slot_1.json | SchemaVersion=5; Quests.QuestStates presente | | | |
| 56 | Confirmar P0/P1 | Zero P0; P1 apenas HUMAN_WIRING_REQUIRED (não code bugs) | | | |

---

## Steps Mínimos para Sub-Slice (sem wiring manual)

Steps **1-5, 12-14, 33-36, 46-53** são testáveis imediatamente:

```
Boot → inventory → skill tree → dash/dodge/block → save/load → modal guards
```

Este sub-slice valida o core engine sem dependência de scene wiring.

---

## Blocking Issues

| Issue | Severidade | Resolução |
|---|---|---|
| Console com erro vermelho ao abrir Unity | P0 | Investigar + fix imediato |
| Player não aparece em Play Mode | P0 | Verificar GameBootstrap + player prefab |
| Inventory não abre com I | P1 | Verificar InventoryPanelController.RuntimeInitializeOnLoadMethod |
| Dash executa durante modal | P1 | Verificar HasActiveModal em PlayerDashController |
| Save não cria slot_1.json | P1 | Verificar SaveManager.SaveGame() + caminho |
| Quest não aparece após aceitar | P1 | Verificar QuestRuntimeBootstrap.Instance |
| Farm→Town não transiciona | P1 | HUMAN_WIRING_REQUIRED: WAVE13 wiring |
| Cave não entra | P1 | HUMAN_WIRING_REQUIRED: WAVE16 wiring |

---

## Resultado Esperado ao Completar

Preencher após execução:

```
Data de execução:
Testador:
Unity version:
Branch + commit:
Steps 1-56: __/56 passaram
P0 bugs encontrados: 0 ou listar
P1 bugs encontrados: 0 ou listar
P2/P3 bugs novos: listar se houver
Decisão: ACCEPTED / ACCEPTED_WITH_DEBT / REJECTED_BLOCKED
```

Atualizar após execução:
- `docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md`
- `docs/validation/WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md`
- `docs/project/CURRENT_STATE.md`
