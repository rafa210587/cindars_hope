# Execution Report — fable_43 (Endgame: Ato 5, os Quatro do 101 e a Escolha Final)

> Spec: `.specs/a_implementar/fable/fable_43_spec_endgame_act5_final_bosses_choice.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (lógica determinística + contratos + save + quest pronta; scene wiring / Play Mode da sequência DEFERRED_TO_FINAL_VALIDATION)
> Branch: `dev` · Data: 2026-06-21
> validated_game_rules: `fonte_rules.md`, `save_rules.md`, `event_rules.md`, `cave_rules.md` (stable-run não tocada)

---

## Acceptance criteria extracted

| CA | Critério | Evidência | Status |
|----|----------|-----------|--------|
| CA-1 | Gate 100→101: Elder derrotado + Fragmento da Vida → 101 destravado; sem isso, bloqueio com aviso | `EndgameGate.TryUnlockLevel101OnElderDefeated` consome `FinalChoiceService.CanUnlockLevel101` (EXISTENTE); `Level101BlockedMessage`; testes `Gate_*` (5) | OK (lógica) |
| CA-2 | Sequência dos 4 em ordem canônica; recuperação controlada entre eles; re-entrável após derrota, sem softlock | `EndgameSequenceController` (máquina NotStarted→InEncounter→Recovery→Complete); `RetryCurrent`; testes `Sequence_*` (3) | OK (lógica) |
| CA-3 | Cindrathel reflete build real (mapper determinístico + fallback); Archivist oculta widgets por fase e SEMPRE restaura | `MirrorBuildMapper` + `MirrorBuildMapperTests` (6); `HudSuppressionChangedEvent` + `HudSuppressionConsumer` + restauração centralizada; testes `HudSuppression_*` (4) + `HudConsumer_*` (2) | OK (lógica) |
| CA-4 | Escolha final só com confirmação forte; aplicada 1× (AlreadyApplied); Ithryndor responde conforme; PostGameWorldState + FonteState persistem e sobrevivem save/load | `FinalChoiceRuntimeAdapter` (token + idempotência + evento 1×); `EndgameGate.BranchFor*`; save aditivo `MainProgressionSaveData`; testes `Apply_*`, `EndgameSaveFields_RoundTrip`, `LegacySave_*` | OK (lógica) |

Validação humana / Play Mode da sequência completa (scene 101 + spawn dos 4): **DEFERRED_TO_FINAL_VALIDATION** (autorizado pelo dono; sem Unity Editor nesta sessão).

---

## Existing systems audit

Fase 0 — auditoria de reuso (system-reuse-audit), nada recriado:

| Sistema existente | Reuso nesta spec |
|---|---|
| `FinalChoiceService` (EvaluateFinalChoice, CanUnlockLevel101, token `FINAL_CHOICE_CONFIRMED`, idempotência) | CONSUMIDO pelo `EndgameGate` e `FinalChoiceRuntimeAdapter`. Regras NÃO reescritas. |
| `EndgameContracts` (FinalChoiceType, EndingEffectProfile Protect/Seal/Use, ArchivistRevealState) | CONSUMIDOS (perfis canônicos, sem campos novos). |
| `MainProgressionSection` (gate 100/101, FinalChoiceStatus, PostGameWorldState, MainAct.PostGame, FragmentStateRecord) | CONSUMIDA; host vivo = `FonteRuntimeService.Progression`. |
| `FonteRuntimeService` (IntegrateFragment, RestoreFromSave, Progression) | ESTENDIDO com `RestoreEndgameState(...)` (5 campos endgame). Integração do Hope continua via `IntegrateFragment`. |
| `MainProgressionQuestBridge` + `QuestMainActsIds.Finales` | ESTENDIDO: +1 entrada `ActFinale` (Act 5) → integra Hope + concede 1 skill point do ato (idempotente via `RewardedMainActIds`). |
| `QuestRegistry` (partial, padrão F36 in-memory; `mq_act4_03/04` já existiam) | ESTENDIDO com `QuestRegistry.Endgame.cs` → `mq_act4_05_final_choice` encadeada de `mq_act4_04`. ID canônico do catálogo preservado. |
| Boss phase AI (F05) + assets F33 dos 4 finais | Consumidos por id estável no `EndgameEncounterCatalog`; sem segundo boss AI. |
| `GameEventBus` | Único canal de comunicação dos eventos endgame. |

**Fase 0 — achado de persistência:** `MainProgressionSection` NÃO era persistida (só `FonteSaveData` existia). Conforme a spec (CONDITIONAL), adicionada seção aditiva `MainProgressionSaveData` (padrão WI-18), tipos simples apenas.

**Decisão de escopo (documentada):** o gerador foi modelado como registro in-memory `QuestRegistry.Endgame.cs` (padrão vivo de F10/F36), não como Editor SO generator — a spec exige "não criar segundo fluxo de main quest; o gerador entra na fonte Main" e o projeto não possui pipeline SO de quest. O `HudSuppressionChangedEvent` é definido aqui (EMENDA V3: fable_71 ainda não executada na Batch 11; F43 é dona temporária do contrato e consumidora — quando F71 landar, esta definição é o contrato de referência).

---

## Spec Compliance Matrix

| Requisito da spec | Implementação |
|---|---|
| Gate 100→101 consumindo `CanUnlockLevel101` + bloqueio com mensagem | `EndgameGate.cs` |
| `EndgameSequenceController` (Begin/AdvanceEncounter/CurrentEncounter, re-entrável, recuperação) | `EndgameSequenceController.cs` |
| Cindrathel espelho de build (puro, determinístico, fallback guerreiro básico) | `MirrorBuildMapper.cs` |
| Archivist supressão de HUD por fase + restauração garantida | `EndgameSequenceController.PublishHudSuppressionPhase/RestoreHud`, `HudSuppressionConsumer.cs`, `EndgameEvents.cs` |
| Escolha final: confirmação forte + token + `EvaluateFinalChoice` | `FinalChoiceRuntimeAdapter.cs` |
| Ithryndor condicional (Protect=aliado/Seal=parcial/Use=4 fases) | `EndgameGate.BranchFor/BranchForEnding` + `IthryndorBranch` |
| Epílogos como estado de mundo (PostGameWorldState + FonteState finalizado) | aplicado por `FinalChoiceService` (existente); persistido por `MainProgressionSaveData` |
| Quests endgame encadeadas (`mq_act4_05_final_choice`, ID canônico) + XP/skill point | `QuestRegistry.Endgame.cs` + `Finales` Act 5 (Hope + skill point) |
| Eventos: `HudSuppressionChangedEvent`, `FinalChoiceResolvedEvent`, `EndgameEncounterChangedEvent` | `EndgameEvents.cs` |
| Save aditivo simples (sem refs Unity); load legado com defaults | `MainProgressionSaveData` + `SaveManager` capture/restore + `FonteRuntimeService.RestoreEndgameState` |
| Anti-softlock quest final | `mq_act4_05` Category.Main (não expira) + objetivos re-entráveis |
| Stable-run da caverna (ADR-0005) intocada | nenhum gerador procedural tocado; 101 fora do escopo de seed nesta fatia |

---

## Validation

```
Validation method: dotnet build (--no-restore) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
Assembly-CSharp:        PASS (exit 0; 1 warning pré-existente CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (exit 0; 3 warnings pré-existentes)
Docs validation:        ver bloco abaixo
Diff completeness:      ver bloco abaixo
run_strict_validation:  ver bloco abaixo (exit 1 ambiental por 3 .unity já modificadas no working tree — NÃO desta spec)
```

(Valores preenchidos no commit; ver SPEC_RESULT no encerramento.)

Testes EditMode adicionados (3 arquivos, ~22 testes): `EndgameSequenceTests`, `MirrorBuildMapperTests`, `FinalChoiceRuntimeTests`. Tipos referenciados são runtime puros (sem `CindarsHope.Editor.*`) → pasta `Tests/EditMode/MainProgression/`. Execução do Test Runner: DEFERRED (sem Unity Editor nesta sessão; build compila os testes 0E).

---

## Honest status rationale

Status **BUILD_VALIDATED_WITH_WARNINGS**: o núcleo determinístico (gate, máquina de estados dos 4, mapper de espelho, supressão de HUD, adapter da escolha, save aditivo, quest do endgame) está implementado, compila 0E em ambos os assemblies e é coberto por EditMode tests. A materialização de cena do nível-101 (CaveScene especial), o spawn físico dos 4 bosses, o binder MonoBehaviour do HUD e o gatilho da escolha na Fonte física são trabalho de Play Mode/cena, explicitamente DEFERRED_TO_FINAL_VALIDATION pela própria spec e pelo dono. Nenhum claim ACCEPTED / PLAYMODE_VALIDATED é feito.

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (gate, escolha, mapper, máquina de estados, save copy)
Changed Unity scene/prefab/asset wiring: NO (nenhum .unity/.prefab/.asset editado)
Automated tests added/updated: YES (~22 EditMode tests em Tests/EditMode/MainProgression)
Automated tests command: Unity Test Runner EditMode — NOT RUN nesta sessão (sem Editor); compilam 0E via dotnet build
Manual Play Mode scenario: REQUIRED — sequência 101 + escolha + 1 epílogo + save/load → DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A (testes adicionados)
Residual risk:
 - sequência dos 4 / spawn / arena 101 não wired em cena (Play Mode pendente);
 - HudSuppressionConsumer precisa de binder MonoBehaviour + ligação às views reais do HUD (Play Mode);
 - escolha final precisa do gatilho na Fonte física (modal de confirmação forte F14) — pendente cena;
 - MirrorBuildMapper recebe o snapshot do loadout via adapter de cena (a montar em Play Mode);
 - HudSuppressionChangedEvent é provido por F43 hoje; quando fable_71 (Batch 11) landar, conferir que não há duplicação do contrato.
```

## Remaining work (Play Mode / cena — DEFERRED)

1. CaveScene especial do nível 101 + hook de entrada gated por `EndgameGate.CanEnterLevel101`.
2. Spawner sequencial dos 4 (consome F05 + assets F33) acionado por `EndgameSequenceController`.
3. Binder MonoBehaviour do HUD assinando `HudSuppressionChangedEvent` via `HudSuppressionConsumer` e ocultando/restaurando as views reais.
4. Gatilho da escolha na `FonteInteractable` (modal de confirmação forte F14 → `FinalChoiceRuntimeAdapter.Apply`).
5. Adapter de cena que lê o loadout real (classe inferida F39 + arma + skills ativas) → `MirrorBuildInput` no início do encontro de Cindrathel.
6. CavePostGamePolicy "NewAreas" (epílogo Use) — flag para spec futura.
7. Rodar Test Runner EditMode no Unity (validação Phase 2).
