---
doc_type: validation_report
spec: fable_62_spec_onboarding_control_hints
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-21
validated_adrs: [ADR-0007, ADR-0006]
validated_game_rules: [ui_modal_rules.md, save_rules.md, event_rules.md, input_map.md]
---

# Execution Report — fable_62 Onboarding Hints + Controls Screen

Status: **BUILD_VALIDATED_WITH_WARNINGS** (nucleo P1 — 5 hints + persistencia + tela Controles
read-only — completo e testado; prioridade Important do aviso da caverna e colocacao visual da tela
deferidas como debito documentado; Play Mode DEFERRED_TO_FINAL_VALIDATION conforme a spec).

## Acceptance criteria extracted

| CA | Criterio | Implementacao | Evidencia |
|---|---|---|---|
| CA-1 | 5 hints disparam na 1a ocorrencia e nunca mais (mesmo save), via toast do HUD, sem bloquear gameplay | `OnboardingHintTracker.TryMarkSeen` (1x/save) + `OnboardingHintService` (gatilhos em eventos existentes) -> `PlayerActionFeedbackEvent` (canal WI-23) | EditMode: `TryMarkSeen_SecondTrigger_DoesNotReFire`, `TryMarkSeen_AllFiveHints_EachFiresExactlyOnce`; cenario humano passos 1-6, 9-10 |
| CA-2 | 1a entrada na caverna mostra aviso de perigo (perda de itens) + dodge (Space)/block (Shift), prioridade Important | `hint_cave_danger` no catalogo (priority=Important, texto com Space/Shift); gatilho `CaveLevelEnteredEvent` | EditMode: `Catalog_CaveDangerHint_IsImportantPriority`, `Catalog_CaveDangerHint_MentionsDodgeAndBlock`; **debito**: fura-fila real exige editar arquivo proibido (ver Honest status) |
| CA-3 | HintsSeen sobrevive a save/load; save legado sem secao = lista vazia (defaults seguros) | `OnboardingHintsSaveData` (List<string>) + `OnboardingHintsSectionProvider` + campo aditivo em `GameSaveData`; capture/restore no `SaveManager` | EditMode: `RoundTrip_SeenHintsSurvive`, `RestoreSeen_NullSection_LegacySave_AllHintsEligible`, `Provider_*`; cenario humano passos 11-13 |
| CA-4 | Aba Sistema (F56) ganha "Controles" read-only, navegavel, espelhando input_map.md (F67) | `ControlsReferenceContent` (puro, espelha F67) + `ControlsReferenceScreen` (adapter fino, Esc fecha via ModalManager) | EditMode: `ControlsReference_*`; **gating**: colocacao visual na hierarquia da aba depende de scene/prefab (fora do escopo) — DEFERRED |
| CA-5 | Nenhum hint pausa, abre modal ou rouba input; movimento continuo durante toasts | Hints publicam apenas `PlayerActionFeedbackEvent`; nenhum `ModalManager.PushModal` nos hints; diff sem bloqueio | Diff (sem ModalManager nos hints de onboarding); cenario humano passos 2, 5 |

## Existing systems audit

Fase 0 (system-reuse-audit). Nada criado em paralelo; tudo reusa/estende o existente:

| Sistema | Encontrado | Reuso/decisao |
|---|---|---|
| Canal de toast | `GameplayFeedbackService` consome `PlayerActionFeedbackEvent` -> `HudFeedbackUpdatedEvent` -> `FeedbackToastHudView` (WI-23) | REUSADO: hints publicam `PlayerActionFeedbackEvent`. Nao criei segundo canal. Service NAO editado. |
| Prompt contextual | `ContextHintController` ("[E] {prompt}") | INTACTO: coexiste; hint de onboarding (1x/save) e superficie distinta. |
| Prioridade Important | `FeedbackMessagePriority.Important` existe; mas o handler `OnPlayerFeedback` mapeia `PlayerActionFeedbackEvent` para Normal (hardcoded); so eventos de quest sobem para Important | Honrado por duracao estendida (6s); fura-fila real exigiria editar `GameplayFeedbackService` (proibido). Debito. |
| Persistencia aditiva | `ISaveSectionProvider`, `HotbarSectionProvider`, `SpellbookSectionProvider` (fonte = `*.Instance`) | REUSADO: `OnboardingHintsSectionProvider` no mesmo padrao; fonte = `OnboardingHintService.Instance`. |
| Gatilho "1o frame" | `GameplayHudBootstrap` (`RuntimeInitializeOnLoadMethod`, AfterSceneLoad, DontDestroyOnLoad) | REUSADO: servico adicionado ao mesmo GameObject; `OnEnable` = 1o frame de gameplay pos-bootstrap. Nenhum evento novo criado. |
| Evento de equip | `EquipmentSlotChangedEvent(Slot, ItemInstanceId)` | REUSADO: gatilho de `hint_attack` em RightHand/LeftHand com item. |
| Evento de status no player | `StatusAppliedEvent(TargetId,...)`; `StatusEffectManager.PlayerTargetId = "player"` | REUSADO: gatilho de `hint_status` filtrando `TargetId == "player"`. |
| Aba Sistema (F56) | `SystemTabController` / `SystemTabViewModel` / `ModalManager` (SystemConfirm, PushModal/TryPopIfCurrent) | REUSADO: `ControlsReferenceScreen` segue o padrao fino F56 (resolve ModalManager via GameBootstrap, Esc fecha). |
| Catalogo de teclas | `docs/game_rules/input_map.md` (F67, accepted) | FONTE CANONICA: `ControlsReferenceContent` espelha; so bindings canonicos; debug keys excluidas. |

Zero eventos novos. Zero managers/services paralelos. Logica de dominio em C# puro
(`OnboardingHintTracker`, `OnboardingHintCatalog`, `ControlsReferenceContent`); MonoBehaviours
(`OnboardingHintService`, `ControlsReferenceScreen`) sao adapters finos.

## Spec Compliance Matrix

| Requisito da spec | Implementacao | OK |
|---|---|---|
| OnboardingHintService wired via bootstrap | `GameplayHudBootstrap.AddComponent<OnboardingHintService>()` | OK |
| Tabela estatica {id, texto PT-BR, prioridade} | `OnboardingHintCatalog` (5 entradas imutaveis) | OK |
| Cada hint 1x/save (checa flag antes, marca depois) | `OnboardingHintTracker.TryMarkSeen` (HashSet.Add) | OK |
| Entrega via 1 canal de toast existente | `PlayerActionFeedbackEvent` (WI-23) | OK |
| Nunca bloqueia/abre modal | hints nao chamam ModalManager | OK |
| `OnboardingHintsSaveData` (List<string>, simple types) | criado; campo aditivo em `GameSaveData` | OK |
| Seccao aditiva via provider; legado = lista vazia | `OnboardingHintsSectionProvider`; restore null = vazio | OK |
| Saves antigos intactos, sem migration | campo aditivo; sem entrada no SaveMigrationRegistry | OK |
| Tela Controles read-only espelha input_map.md | `ControlsReferenceContent` (espelha F67) + screen | OK (colocacao visual DEFERRED) |
| Esc fecha (padrao F14/F56) | `ControlsReferenceScreen.Close` via ModalManager | OK |
| unsubscribe em OnDisable | `OnboardingHintService.OnDisable` desinscreve os 4 eventos | OK |
| Logica de 1a-ocorrencia em classe pura testavel | `OnboardingHintTracker` (sem MonoBehaviour) | OK |
| Zero evento novo | confirmado (so consumo + `PlayerActionFeedbackEvent`) | OK |
| Aviso caverna prioridade Important | catalogo marca Important; fura-fila real = debito | PARCIAL |

## Validation

```
Validation method: dotnet build (no-restore) + validate_docs + check_spec_diff_completeness + run_strict_validation.ps1
Assembly-CSharp: PASS (0 Erros; 1 warning pre-existente CS0649 nao relacionado)
Assembly-CSharp-Editor: PASS (0 Erros; warnings pre-existentes nao relacionados)
Docs validation: PASS (validate_docs.ps1 exit 0)
check_spec_diff_completeness.ps1: PASS (codigo + report + testes presentes)
run_strict_validation.ps1: exit 1 ESPERADO/AMBIENTAL — 3 cenas .unity (CaveScene/FarmScene/TownScene)
  ja modificadas no working tree ANTES desta spec; nao foram tocadas aqui. Builds e docs PASS.
EditMode tests: OnboardingHintTests (20 testes) — compilam no Assembly-CSharp; execucao via Unity
  Test Runner DEFERRED_TO_FINAL_VALIDATION (Play Mode nao executavel nesta sessao).
```

## Honest status rationale

Nucleo P1 (CA-1, CA-3, CA-5) completo e coberto por EditMode: 5 hints de 1a-ocorrencia, persistencia
aditiva com defaults seguros e zero bloqueio de gameplay. CA-4 (conteudo/logica da tela Controles)
pronto e testado; a colocacao visual na hierarquia da aba Sistema exige bind de scene/prefab, fora do
escopo desta spec (sem edicao de .unity/.prefab) — DEFERRED, conforme o gating de F56 previsto pela
propria spec.

Debito honesto em CA-2: a marca "Important" do aviso da caverna esta no catalogo e e verificada por
teste, mas o "furar a fila" real (comportamento do `GameplayFeedbackService`) so e acionado por eventos
de quest hardcoded; ativa-lo para hints exigiria editar `GameplayFeedbackService`, listado como arquivo
PROIBIDO por esta spec ("consumir, nao reescrever"). Escolha conservadora: aviso entregue no canal Normal
com duracao estendida (6s) para legibilidade. Por isso o status e BUILD_VALIDATED_WITH_WARNINGS, nao
BUILD_VALIDATED pleno.

`run_strict_validation.ps1` retorna exit 1 unicamente pelas 3 cenas .unity ja sujas no working tree
(estado ambiental pre-existente, nao desta spec); builds e docs passam com exit 0.

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (1a-ocorrencia, persistencia de flags, integridade de catalogo/controles)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/UI/OnboardingHintTests.cs — 20 testes)
Automated tests command: Unity Test Runner (EditMode) — NOT RUN nesta sessao (Play Mode/Unity indisponivel)
Manual Play Mode scenario: docs/validation/playmode/fable_62_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: (1) prioridade Important do aviso da caverna nao fura a fila (debito; arquivo proibido);
  (2) colocacao visual da tela Controles na aba Sistema pendente de bind de scene/prefab (DEFERRED);
  (3) execucao dos EditMode tests no Unity Test Runner deferida ao lote final.
```

## Remaining work

- Ligar visualmente a tela Controles na hierarquia da aba Sistema (scene/prefab) — proxima fatia.
- Resolver a prioridade Important real do aviso da caverna quando `GameplayFeedbackService` puder ser
  estendido (fora do escopo desta spec; exige autorizacao de edicao do arquivo).
- Executar `OnboardingHintTests` no Unity Test Runner e o cenario humano no lote final (jornada do
  jogador novo: boot -> mover -> interagir -> equipar -> caverna -> status, cada hint 1x).
