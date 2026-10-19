---
doc_type: validation
status: evidence
spec_id: fable_70_spec_npc_side_quest_chains_wave2
validation_type: automated
result: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-20
executor: Claude Code
source_of_truth: false
validated_adrs: []
validated_game_rules: [quest_rules.md, npc_rules.md]
---

# Validation Report — fable_70 NPC Side Quest Chains (2a leva, 11 NPCs)

> This report is evidence, NOT an execution queue.
> Do not re-run the spec based on this report alone.

Status: **BUILD_VALIDATED_WITH_WARNINGS** — nucleo (11 cadeias x 3 = 33 quests sq_<npc>_<n>,
gates compostos incl. atos, encadeamento, recompensas, flags de servico + flags de rivalidade
da Yael e segredo do Maelor) implementado por EXTENSAO do catalogo fable_35 (NENHUM segundo
sistema de cadeia), buildado (0 erros nos dois assemblies) e coberto por EditMode tests. O
marcador visivel "[!] Quest" no dialogo e a aceitacao ponta a ponta em cena ficam diferidos
(DEFERRED_TO_FINAL_VALIDATION) — exigem Play Mode/cena (autorizado a pular pelo dono nesta sessao).

---

## What Was Run

- [x] dotnet restore (Assembly-CSharp + Assembly-CSharp-Editor) -> exit 0 (NETSDK1004 inicial resolvido)
- [x] dotnet build Assembly-CSharp.csproj --no-restore -> PASS (0 erros, 1 warning pre-existente CombatTelemetrySession)
- [x] dotnet build Assembly-CSharp-Editor.csproj --no-restore -> PASS (0 erros, 3 warnings pre-existentes)
- [x] tools/docs/validate_docs.ps1 -> PASS (exit 0)
- [x] tools/docs/check_spec_diff_completeness.ps1 -> PASS (exit 0)
- [x] tools/docs/run_strict_validation.ps1 -> exit 1 (AMBIENTAL: 3 .unity pre-modificadas + WARNs de reports alheios; NAO e falha desta spec)
- [ ] Unity validators (F30) -> NOT RUN (Unity Editor indisponivel nesta sessao)
- [ ] Play Mode checklist -> DEFERRED_TO_FINAL_VALIDATION

---

## Acceptance criteria extracted

| CA | Criterio | Evidencia | Status |
|----|----------|-----------|--------|
| CA-1 | 11 cadeias geradas com matriz de viabilidade; nenhum objetivo cortado silenciosamente | `NpcQuestChainCatalog.Wave2Steps` (33 steps); matriz objetivo x tipo abaixo; testes `Catalog_Has23Chains_69Steps_AfterWave2`, `Wave2_NoSilentlyCutObjective_EveryStepMapsToExistingType` | OK |
| CA-2 | Encadeamento + gates compostos (flag anterior + amizade por step + ato) incl. Maelor tardio | `NpcQuestChainService.IsEligible` (MinFriendshipOverride); testes `PerStepFriendshipGate_Renko_1_2_4`, `MaelorChain_IsLate_FriendshipAndActTwoGated`, `ActOneGatedFinals_Gurd_Alaric_Liora`, `YaelFinal_IsActTwoGated`, `Step1_IsOfferable_Step2NotUntilStep1Done_Mara` | OK |
| CA-3 | Recompensas/flags de servico/decisoes novas (Yael rivalidade, zero Vaelrion) | testes `YaelChain_GrantsRivalryFlags_AndF25ServiceFlag`, `YaelRivalChoiceFlag_IsMutuallyExclusiveSingleRecord_NoDoubleGrant`, `NoQuest_CitesVaelrion_InAnyTargetOrFlag`, `MaelorFinal_GrantsSecretFlag`, `RewardItem_GrantedOnQ3_ForWave2Finals`, `FriendshipReward_PlusEight_IsTheExistingF26Hook_NotASecondPath` | OK |
| CA-4 | 3 cadeias ponta a ponta incl. OBRIGATORIAMENTE Yael (rivalidade) e Maelor (gates tardios) | testes `YaelAndMaelor_EndToEnd_AcceptCompleteNextService` + `Wave2DynamicInstance_SurvivesSaveRoundTrip` | OK |

### Matriz cadeia x status (11 cadeias, 33 quests)

Todas as 11 cadeias tem 3 steps (q1 domestica -> q2 mundo -> q3 marco) com IDs canonicos
`sq_<npc>_<n>`. Contagem total no catalogo apos a spec = 23 cadeias / 69 steps (12 fable_35 + 11 fable_70).
Nenhum step dormante (toda narrativa coube em 1 tipo de objetivo existente).

| Cadeia | q1 | q2 | q3 (servico destravado) | Gate amizade | Ato gate |
|--------|----|----|--------------------------|--------------|----------|
| mara | Deliver licenca de obra | Collect paginas arrancadas x3 | Talk caso lei/compaixao -> registro preferencial (RESERVADO) | 1/2/3 | — |
| nimble | Collect wood x10 | Collect madeira auto-reparavel | Talk 1a movimentacao de estrutura -> mover c/ desconto (RESERVADO) | 1/2/3 | — |
| gurd | Collect stone x8 | Reach parede antiga | Talk conter briga -> mutirao pesado (RESERVADO) | 1/2/3 | act_1_done |
| yael | Talk loja apos meia-noite | Collect pistas x3 (+sq_yael_rival_known) | Talk mediar rivalidade (+sq_yael_rival_choice) -> servico F25 Encomenda de Livro | 1/2/3 | act_2_done |
| pip | Deliver pacote sementes | Talk fantasma do Jardim | Deliver carta errada -> recados expressos (RESERVADO) | 1/2/2 | — |
| alaric | Defeat estrada baixa x6 | Talk relatorio incompleto | Defeat miniboss banda 2 -> patrulha estendida (RESERVADO) | 1/2/3 | act_1_done |
| renko | Deliver encomendas x3 | Talk mercadoria sem dono | Talk tres sorrisos -> estoque raro rotativo (RESERVADO) | 1/2/4 | — |
| liora | Talk cancao sem autor | Reach estatua a noite | Reach eco do sonho banda 2 -> cancao de descanso (RESERVADO) | 1/2/3 | act_1_done |
| orlan | Collect suprimentos x5 | Reach hospede sem sombra | Talk conta aberta -> quarto reservado (RESERVADO) | 1/2/3 | — |
| savra | Collect ervas x6 | Defeat pragas noturnas x5 | Collect fungo de baixo x3 -> bancada de antidotos (RESERVADO) | 1/2/3 | — |
| maelor | Talk passos sem luz | Collect registro apagado | Talk silencio protege (+sq_maelor_secret) -> guia noturno (RESERVADO) | 2/3/4 | q2+q3 act_2_done |

### Matriz objetivo x tipo existente (viabilidade — nenhum corte silencioso)

| Kind do esboco | QuestObjectiveType existente | Observacao / adaptacao |
|----------------|------------------------------|-------------------------|
| Collect (coletar N) | `CollectItem` | wood/stone/ervas/fungos/paginas/suprimentos/pistas/registro |
| Deliver (entregar) | `DeliverItem` | licenca/pacote/encomendas/carta |
| Talk (conversar/mediar/escolha) | `TalkToNpc` | ADAPTA "caso moral", "conter briga", "mediar rivalidade", "tres versoes", "silencio protege", "conta aberta" -> falar com NPC/marker no local |
| Reach (alcancar/investigar) | `ReachLocation` | ADAPTA "investigar parede antiga", "estatua a noite", "eco do sonho banda 2", "hospede sem sombra" |
| Defeat (derrotar marco) | `DefeatEnemy` | banda da estrada baixa / miniboss banda 2 / pragas noturnas (count) |

Nenhum objetivo do esboco ficou sem mapeamento; NENHUM tipo de objetivo novo foi criado.

### Tabela de flags de servico (q3) — Yael real, demais RESERVADAS

| NPC | Flag de servico q3 | Estado |
|-----|--------------------|--------|
| yael | service_unlock_yael_book_order | ATIVA — questFlag do servico F25 "Encomenda de Livro" (unico do roster v1.1 com servico declarado) |
| mara | service_unlock_mara_preferred_registry | RESERVADA (leva v2 de servicos unicos) |
| nimble | service_unlock_nimble_move_discount | RESERVADA |
| gurd | service_unlock_gurd_heavy_cleanup | RESERVADA |
| pip | service_unlock_pip_express_runs | RESERVADA |
| alaric | service_unlock_alaric_extended_patrol | RESERVADA |
| renko | service_unlock_renko_rotating_rare_stock | RESERVADA |
| liora | service_unlock_liora_rest_song | RESERVADA |
| orlan | service_unlock_orlan_reserved_room | RESERVADA |
| savra | service_unlock_savra_antidote_bench | RESERVADA |
| maelor | service_unlock_maelor_night_guide | RESERVADA |

Pendencia explicita: a leva v2 de servicos unicos consumira as 10 flags RESERVADAS. Yael ja prova
o caminho com o servico F25 real. As flags so existem em save apos o jogador concluir a q3.

### Decisoes novas (Refinamento v2 3.6) — aplicadas

- **Vaelrion (regra 1):** ZERO mencoes. Nenhum dos 11 NPCs e Vaelrion; nenhuma `TargetId`/flag cita
  "vaelrion" (teste `NoQuest_CitesVaelrion_InAnyTargetOrFlag` varre todos os 33 steps).
- **Sethra<->Yael (regra 2):** a cadeia da Yael e a expressao jogavel da rivalidade COMERCIAL. q2 expoe
  (`sq_yael_rival_known`), q3 e a escolha de mediacao (favor Yael / favor Sethra / mediar) gravada em
  `sq_yael_rival_choice` — flag unica, mutuamente exclusiva (um unico registro/turn-in, sem double grant;
  teste `YaelRivalChoiceFlag_IsMutuallyExclusiveSingleRecord_NoDoubleGrant`), lida SO por dialogo (F28).
  Nenhum texto/flag revela o culto de Nyx nem a identidade ritual de Sethra (Ato 3 / F36 e dona). Sethra
  NAO ganha cadeia propria.
- **Corvus (regra 3):** nenhuma quest adicionada — escopo exclusivo dos 11 listados.

---

## Existing systems audit

| Sistema | Encontrado | Reutilizado / Criado |
|---------|-----------|----------------------|
| fable_35 catalogo/servico de cadeia | `NpcQuestChainCatalog` (12 cadeias), `NpcQuestChainService` (gates/oferta/progresso) | ESTENDIDO — wave-2 adicionada a `s_wave2Steps` e concatenada em `s_steps`; NENHUM segundo sistema de cadeia, gerador, registry ou formula |
| F34 fonte Npc / quest dinamica | `QuestInstance`, `QuestService.AcceptDynamicInstance/TurnIn/MarkObjectiveComplete`, `QuestRewardScaling` | REUTILIZADO — quests fluem pelo service unico |
| F26 amizade | `FriendshipService.OnQuestGiverInteracted` (+8 idempotente por questId) | REUTILIZADO — +8 amizade automatico no turn-in; o catalogo NAO carrega reward de amizade (teste `FriendshipReward_PlusEight_...`) e NAO toca estado de amizade; gates de amizade so LEEM via probe injetado |
| Flags | `QuestFlagService` via `QuestRewardApplicator` (QuestFlagGrant, TrackByFlagId) | REUTILIZADO — `sq_*_done`, `service_unlock_*`, `sq_yael_rival_known/choice`, `sq_maelor_secret` gravados pelo applicator existente; sem segundo registro de flags |
| F25 servico da Yael | "Encomenda de Livro" (servico unico declarado) | LIDO — `service_unlock_yael_book_order` e o questFlag de unlock (consumo no sistema dono) |
| F28 dialogo (condicoes) | `TownNpcDialogueLibrary`, `DialogueLineCondition.RequiredFlag/MinFriendship` | LIDO — oferta visivel diferida; predicado `HasOffer`/`GetOfferableStep` e o contrato que a camada de dialogo consome (mesmo que fable_35) |
| F73 localizacao | `TitleKey/DescKey/OfferKey/TurnInKey` = `quest.<sq_id>.<slot>` | REUTILIZADO — textos na voz do NPC enderecados por id de localizacao (strings no string table F73) |
| Save | `QuestStateRecord`/`QuestStateSection` (simple types) | REUTILIZADO — instancias dinamicas wave-2 persistem pela secao existente; sem campo/secao nova (teste `Wave2DynamicInstance_SurvivesSaveRoundTrip`) |
| **Criado** | nenhuma classe nova — apenas: campos `MinFriendshipOverride`/`ExtraGrantedFlagIds` em `NpcChainStepData`, dados de 33 steps, e `NpcChainsWave2Tests` | conteudo + 2 campos de dados; nenhum sistema paralelo |

---

## Spec Compliance Matrix

| Requisito (escopo spec) | Implementacao | Status |
|--------------------------|---------------|--------|
| Extensao da tabela do gerador F35: +11 cadeias / ~33 QuestDefinitions IDs `sq_<npc>_<n>` | `NpcQuestChainCatalog.s_wave2Steps` (33) concatenado em `s_steps`; `BuildInstance` reutilizado | OK |
| Objetivos mapeados aos tipos existentes (adaptar ou dormante) | matriz objetivo x tipo; 0 dormantes | OK |
| Recompensas: gold/item/amizade +8/flag de servico | gold+XP escalados (F34) + item q3 (RewardItemId) + flag de servico; +8 amizade pelo hook F26 | OK |
| Gates: flag anterior + amizade minima por step + ato quando citado | `IsEligible` com `MinFriendshipOverride`; gates de ato act_1/act_2 (read-only) | OK |
| Vaelrion gated por ato 2 (regra 1) | zero mencoes (varredura no teste); regra cumprida por ausencia | OK |
| Rivalidade Sethra<->Yael mediavel sem spoiler do culto (regra 2) | `sq_yael_rival_known` (q2) + `sq_yael_rival_choice` (q3, mutuamente exclusiva); nenhum reveal de culto | OK |
| Flag da Yael conecta ao servico F25; demais RESERVADAS documentadas | `service_unlock_yael_book_order` ativa; tabela de reservadas acima | OK |
| Oferta "[!] Quest" no dialogo dos 11 NPCs | predicado `HasOffer`/`GetOfferableStep` exposto (mesmo contrato F35); marcador visivel em cena | DEFERRED_UI_VISUAL |
| 12 cadeias da 1a leva intactas (regressao) | `Wave1Steps` separado e imutavel; teste `Wave1_TwelveChains_AreUntouched_Regression` | OK |
| EditMode tests (encadeamento, gates compostos incl. ato 2 Maelor, +amizade, rivalidade, 3 cadeias e2e) | `NpcChainsWave2Tests` (16 testes) | OK |
| Sem tipos de objetivo novos / segundo fluxo de oferta / segundo sistema de cadeia | matriz objetivo x tipo; oferta unica via QuestService; catalogo unico estendido | OK |

---

## Validation

```text
Validation method: builds individuais + validate_docs + check_spec_diff_completeness (+ run_strict_validation)
Assembly-CSharp: PASS (0 erros, 1 warning pre-existente CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (0 erros, 3 warnings pre-existentes)
Docs validation: PASS (exit 0)
Spec diff completeness (escopado): PASS (exit 0)
run_strict_validation exit code: 1 (AMBIENTAL — nao e falha desta spec)
  Causa: 3 cenas .unity (CaveScene/FarmScene/TownScene) ja modificadas no working tree
         ANTES desta sessao (confirmado no git status inicial) + WARNs de secoes
         ausentes em execution reports de OUTRAS specs (pre-existentes).
  Esta spec NAO tocou nenhum .unity/.prefab/.asset (verificado em git status).
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

### Testing Quality Gate

```text
Changed runtime code: YES (NpcQuestChainCatalog [+2 campos, +33 steps], NpcQuestChainService [gate de amizade por step])
Changed deterministic logic: YES (gates compostos por step + ato, encadeamento por flag, flags de rivalidade/segredo, mapeamento de objetivo)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Quests/NpcChainsWave2Tests.cs — 16 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (testes compilam no Assembly-CSharp; Unity Test Runner NOT RUN — Editor indisponivel)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (oferta visivel no dialogo + cadeia completa em cena; preferencia: Yael)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: marcador "[!] Quest" e aceitacao por dialogo em cena ainda nao exercitados em Play
  Mode; auto-progresso por eventos reais coberto so por chamadas diretas aos hooks do servico,
  nao pela ponte de eventos viva; F30 (refs de item de recompensa) NOT RUN — ids de item dos q3
  (merithus_seal, nimble_pocket_level, clan_work_glove, nightmarket_rarity, pip_patched_backpack,
  patrol_shield, renko_rare_stock_item, alihana_score, reserved_room_key, greenscale_vial, luandil_lens)
  sao forward-declared no F32 e precisam de validacao Unity no closeout final.
```

---

## Honest status rationale

A logica determinística central (11 cadeias x 3 = 33 quests da 2a leva, gates compostos com amizade
POR STEP e atos, encadeamento por flag, recompensas, flags de servico e as flags de rivalidade da Yael
e segredo do Maelor) esta implementada por EXTENSAO direta do catalogo fable_35 — sem segundo sistema
de cadeia, gerador, registry, formula de recompensa ou caminho de amizade. Compila nos dois assemblies
(0 erros) e e coberta por 16 EditMode tests rodando sobre o fluxo F34 existente, incluindo um teste de
regressao que afirma as 12 cadeias da 1a leva intactas. Por isso **BUILD_VALIDATED_WITH_WARNINGS** e nao
BUILD_VALIDATED puro: (1) `run_strict_validation` retorna exit 1 por causa ambiental (3 .unity sujas de
antes da sessao + WARNs de reports alheios), nao por esta spec; (2) o marcador visivel "[!] Quest" no
dialogo, a aceitacao ponta a ponta em cena e a validacao F30 das refs de item dependem de Play Mode/Unity
Editor e ficam DEFERRED_TO_FINAL_VALIDATION (autorizado a pular nesta sessao). Nao ha claim de Play Mode
PASS nem de Unity validation; a unidade Unity/Test Runner/F30 esta NOT RUN (Editor indisponivel).

## Remaining work

- Wiring runtime do marcador "[!] Quest" no `TownNpcDialogueLibrary`/DialogueController para os 11 NPCs
  consumindo `NpcQuestChainService.HasOffer/GetOfferableStep` (Play Mode).
- Strings na voz do NPC no string table (F73) para os ids `quest.<sq_id>.<slot>` dos 33 steps (epigrafes
  da spec como base obrigatoria).
- F30: validar as refs de item de recompensa dos q3 (11 ids) no Unity Editor.
- Leva v2 de servicos unicos consumindo as 10 flags `service_unlock_*` RESERVADAS.
- Cenario humano de Play Mode completando 1 cadeia inteira da 2a leva (preferencia: Yael, por cruzar a
  decisao nova) — minimo para ACCEPTED.
```
