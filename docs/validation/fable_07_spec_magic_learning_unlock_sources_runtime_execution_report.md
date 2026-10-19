# Execution Report — fable_07 Magic Learning / Unlock Sources (Runtime)

> Spec: `.specs/a_implementar/fable/fable_07_spec_magic_learning_unlock_sources_runtime.md`
> Date: 2026-06-19
> Branch: `dev`
> Status: **BUILD_VALIDATED_WITH_WARNINGS**
> Type: Runtime / Data / Save (save-lock spec)

---

## Honest Status Rationale

Status is **BUILD_VALIDATED_WITH_WARNINGS** (not ACCEPTED):

- Núcleo (PlayerSpellbook + estado de conhecimento + 4 fontes de magia + seção de save +
  CanCast no SpellCastService + API FonteStoryUnlock + evento SpellLearnedEvent + gerador de
  itens) implementado e atende todos os critérios centrais; Spec Compliance Matrix toda OK.
- `dotnet build` Assembly-CSharp PASS (0E/0W) e Assembly-CSharp-Editor PASS (0E/3W pré-existentes);
  docs validation, diff completeness e `run_strict_validation.ps1` exit 0.
- EditMode tests escritos (25 testes) cobrindo save round-trip, seção ausente, ID inválido,
  idempotência, CanCast por fonte, tome progress e matriz de uso por fonte. **Compilam dentro do
  Assembly-CSharp** (sem asmdef de teso separado neste projeto); **execução no Unity Test Runner
  é DIFERIDA** (Unity Editor/Play Mode não executado por ordem do dono — DEFERRED_TO_FINAL_VALIDATION).
- Geração de assets (4 itens + registro no ItemDatabase) é **editor-only e NÃO executada** (sem
  Unity Editor nesta sessão). Não há claim de que os assets existem — ver "Asset generation".
- CastScroll em runtime depende do contexto de cast do player (Play Mode); a lógica determinística
  (consumir + delegar cast + não aprender) está testada, mas o wiring runtime do callback é
  diferido. Documentado como residual risk.

Não é ACCEPTED nem PLAYMODE_VALIDATED: nenhum Play Mode foi executado e a geração de assets/itens
de loja não rodou no Editor.

---

## Phase Status

| Phase | Status | Notes |
|-------|--------|-------|
| Phase 0 Audit | DONE | Reuso de SpellDatabase, SpellCastService, ISaveSectionProvider/HotbarSectionProvider, FoodConsumer, FarmDailyGoalService/FonteRuntimeService (singleton + Capture/Restore), SkillTreeManager.IsNodeUnlocked |
| Phase 1 Build (C#) | BUILD_VALIDATED | Assembly-CSharp 0E/0W; Assembly-CSharp-Editor 0E/3W pré-existentes |
| Phase 2 Unity batchmode | NOT RUN | Unity Editor não executado (DEFERRED por ordem do dono) |
| Phase 3 Play Mode / human | NOT RUN | DEFERRED_TO_FINAL_VALIDATION |

---

## Files Changed

### New (runtime — Assembly-CSharp)
- `Assets/_Game/Scripts/Magic/SpellSourceType.cs` — enum de fontes (None/LearnableScroll/CastScroll/Tome/EquippedItem/NpcTeaching/FonteStory).
- `Assets/_Game/Scripts/Magic/SpellbookSaveData.cs` — DTOs simples (`SpellbookSaveData`, `TomeProgressEntry`).
- `Assets/_Game/Scripts/Magic/SpellbookState.cs` — lógica determinística (C# puro, testável).
- `Assets/_Game/Scripts/Magic/PlayerSpellbook.cs` — MonoBehaviour singleton + API pública + eventos + Capture/Restore.
- `Assets/_Game/Scripts/Magic/PlayerSpellbookRuntimeBootstrap.cs` — RuntimeInitializeOnLoadMethod (sem busca global de cena).
- `Assets/_Game/Scripts/Magic/SpellItemUseHandler.cs` — ponto único de uso scroll/tome (serviço puro, fontes + consumo + pré-requisito).
- `Assets/_Game/Scripts/Magic/SpellItemUseController.cs` — MonoBehaviour fina de input (resolve via GameBootstrap; respeita ModalManager).
- `Assets/_Game/Scripts/Core/Events/MagicEvents.cs` — `SpellLearnedEvent(spellId, source)`.
- `Assets/_Game/Scripts/Save/Providers/SpellbookSectionProvider.cs` — provider de save (padrão HotbarSectionProvider).

### New (editor — Assembly-CSharp-Editor)
- `Assets/_Game/Scripts/Editor/Magic/GenerateSpellLearningItems.cs` — gerador dos 4 itens exemplares + registro no ItemDatabase (idempotente).

### New (tests — Assembly-CSharp)
- `Assets/_Game/Tests/EditMode/Player/SpellbookTests.cs` — 25 testes EditMode.

### Modified
- `Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs` — campos aditivos `SpellSource`, `TaughtSpellId`, `TomeUsesRequired`, `RequiredSkillNodeId` (defaults neutros).
- `Assets/_Game/Scripts/Combat/SpellCastService.cs` — `Spellbook` property (null-safe) + `CanCast(spellId)` + registro de EquippedItem grant no `TryCast` (fluxo equipado inalterado).
- `Assets/_Game/Scripts/Save/SaveData.cs` — campo aditivo `GameSaveData.Spellbook`.
- `Assets/_Game/Scripts/Save/SaveManager.cs` — provider do grimório (Initialize/Capture/Restore após Inventory).
- `Assembly-CSharp.csproj` / `Assembly-CSharp-Editor.csproj` — Compile Includes dos novos arquivos.

---

## Acceptance Criteria Extracted (da spec, com evidência)

| CA | Critério | Implementação | Evidência | Status |
|----|----------|---------------|-----------|--------|
| CA-1 | Aprendizado persistente: TryLearn idempotente; LearnableScroll consome+adiciona; save/load preserva KnownSpellIds | `SpellbookState.TryLearn` (HashSet, idempotente); `SpellItemUseHandler.UseLearnableScroll`; `SpellbookSectionProvider` + SaveManager | Testes `TryLearn_*`, `Handler_LearnableScroll_LearnsAndConsumes`, `SaveRoundTrip_PreservesKnownSpellsAndTomeProgress` | OK |
| CA-2 | Fontes distintas: CastScroll casta sem aprender e consome; Tome aprende no 3º uso; wand só equipada | `UseCastScroll` (delega cast, não aprende); `RegisterTomeUse` (N usos); `AddEquipmentGrant`/`CanCast` | `Handler_CastScroll_CastsAndConsumes_WithoutLearning`, `Handler_Tome_LearnsOnThirdUse`, `CanCast_EquipmentGranted_True_WithoutBecomingKnown` | OK |
| CA-3 | Zero regressão: item Magic equipado casta como hoje sem entrada no spellbook | `SpellCastService.TryCast` core inalterado; EquippedItem grant é transitório e NÃO entra em knownSpellIds; `CanCast` retorna true sem spellbook (legado) | `CanCast_EquipmentGranted_True_WithoutBecomingKnown`, `Capture_DoesNotPersistEquipmentGrants`; caracterização: TryCast preservado (só adiciona grant null-safe) | OK |

---

## Existing Systems Audit (reuse, sem sistema paralelo)

| Necessidade | Sistema existente | Decisão |
|-------------|-------------------|---------|
| Catálogo de magias | `SpellDatabaseSO`/`SpellDataSO` | REUSE — não alterado (proibido pela spec) |
| Caminho único de cast | `SpellCastService` + `EquippedItemResolver` | REUSE — `TryCast` preservado; só `CanCast` advisory + grant adicionados |
| Provider de seção de save | `ISaveSectionProvider` + `HotbarSectionProvider` | REUSE — `SpellbookSectionProvider` segue o precedente |
| Capture/Restore stateful | `HotbarState`, `FarmDailyGoalService`, `FonteRuntimeService` | REUSE — padrão singleton + Capture/Restore (F13/F16/F17) |
| Uso de consumível | `FoodConsumer` | REUSE do padrão — `SpellItemUseController` replica (HasItem/RemoveItem/TryGetItemData + GameBootstrap + ModalManager) |
| Pré-requisito de domínio | `SkillTreeManager.IsNodeUnlocked` | REUSE — injetado no handler |
| Registro de item | `ItemDatabaseSO` (`DataRegistrySO._items` via SerializedObject) | REUSE — gerador registra via SerializedObject (mesmo idioma dos repair helpers) |

Sistemas criados (NOVOS, sem equivalente): `PlayerSpellbook`/`SpellbookState` (estado de conhecimento — não existia), `SpellSourceType`, `SpellItemUseHandler`/`SpellItemUseController`, `SpellbookSaveData`, `SpellLearnedEvent`, `GenerateSpellLearningItems`.

---

## Spec Compliance Matrix

| Requisito (spec) | Implementação | Status |
|------------------|---------------|--------|
| PlayerSpellbook: KnownSpellIds (HashSet), TryLearn, CanCast, GetGrantedByEquipment | `PlayerSpellbook` + `SpellbookState` | OK |
| SpellSourceType enum (6 valores) | `SpellSourceType.cs` | OK |
| Campos aditivos em ItemDataSO (SpellSourceType + TaughtSpellId, defaults neutros) | `ItemDataSO` (+ TomeUsesRequired, RequiredSkillNodeId) | OK |
| SpellItemUseHandler: LearnableScroll (aprende+consome+pré-requisito), CastScroll (casta+consome), Tome (aprende após N) | `SpellItemUseHandler` | OK |
| SpellCastService.CanCast integrado (equipado continua válido SEM conhecimento) | `SpellCastService.CanCast` + EquippedItem grant | OK |
| Save: SpellbookSaveData {List<string> KnownSpellIds, List<TomeProgressEntry>} seção nova, sem migration, default vazio | `SpellbookSaveData` + `GameSaveData.Spellbook` + provider | OK |
| Restore order: após Inventory | Restore chamado após bloco de inventário em `ApplySaveData` | OK |
| API FonteStoryUnlock(spellId) para WAVE 10 | `PlayerSpellbook.FonteStoryUnlock` | OK |
| Evento SpellLearnedEvent(spellId, source) | `MagicEvents.cs` | OK |
| Gerador editor: 4 itens (scroll_learn_fire_spark, scroll_cast_heal_minor, tome_ice_studies, wand_spark) | `GenerateSpellLearningItems` | OK (código; execução diferida) |
| Entradas de loja Ozzra (Shop_Ozzra) | DEFERRED — ver Remaining work | DEFERRED |
| EditMode tests (aprender/idempotência, CanCast por fonte, tome, round-trip) | `SpellbookTests` (25) | OK (escritos; execução diferida) |
| Sem refs Unity no save | DTOs só string/int | OK |
| Sem busca global de cena / GameEventBus only / sem editar .unity/.prefab/.asset | bootstrap por singleton; eventos via GameEventBus; nenhum YAML editado | OK |

---

## Save Expectations (save-lock spec)

| Expectativa | Comportamento implementado | Teste |
|-------------|----------------------------|-------|
| Default values | `SpellbookSaveData` novo = listas vazias; `CaptureSaveData` default = vazio | `CaptureSaveData_Default_IsEmpty` |
| Legacy / missing section | `GameSaveData.Spellbook` ausente em save antigo → null → Restore com null = grimório vazio (sem exceção, sem migration) | `RestoreFromSaveData_Null_LeavesEmpty` |
| Invalid ID fallback | IDs vazios/nulos ignorados no restore; TomeProgress com Uses<=0 ignorado | `RestoreFromSaveData_InvalidIds_AreIgnored` |
| Round-trip capture/restore | KnownSpellIds + TomeProgress preservados | `SaveRoundTrip_PreservesKnownSpellsAndTomeProgress` |
| Idempotency após reload | HashSet não duplica em re-restore; re-aprender é no-op | `Restore_IsIdempotent_NoDuplication`, `TryLearn_SameSpellTwice_IsIdempotent` |
| No Unity references in DTOs | `SpellbookSaveData`/`TomeProgressEntry` só `string`/`int`/`List<>` | revisão de código + rule save-dto-simple-types-only |
| SchemaVersion | Inalterado (5); seção aditiva sem migration | `SaveManager` CurrentSchemaVersion=5 (não tocado) |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (0E/0W)
Assembly-CSharp-Editor: PASS (0E/3W pré-existentes)
Quality check: PASS
Docs validation: PASS (validate_docs.ps1 exit 0)
Diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Comandos executados:
- `dotnet build .\Assembly-CSharp.csproj --no-restore` → exit 0
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` → exit 0
- `.\tools\docs\validate_docs.ps1` → exit 0
- `.\tools\docs\check_spec_diff_completeness.ps1` → exit 0
- `.\tools\docs\run_strict_validation.ps1` → exit 0

### Asset generation
```text
Asset generation: NOT RUN (BLOCKED — Unity Editor não executado nesta sessão)
Command attempted (futuro humano): menu "CindarsHope/Magic/Generate Spell Learning Items"
Residual risk: os 4 itens (scroll_learn_fire_spark, scroll_cast_heal_minor, tome_ice_studies,
  wand_spark) e seu registro no ItemDatabase só existirão após o humano rodar o gerador no Editor.
  As SpellDataSO referenciadas (spell_fire_spark/spell_heal_minor/spell_ice_shard/spell_spark) devem
  existir/ser geradas no catálogo; senão CanCast por item resolverá vazio (esperado até geração).
```

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (spellbook learn/canCast/tome, save capture/restore, item-use handler)
Changed Unity scene/prefab/asset wiring: NO (gerador editor criado mas NÃO executado; nenhum YAML editado)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Player/SpellbookTests.cs, 25 testes)
Automated tests command: NOT RUN no Unity Test Runner (Unity Editor diferido). Compilam via dotnet build Assembly-CSharp (0E).
Manual Play Mode scenario: NOT RUN (DEFERRED_TO_FINAL_VALIDATION) — cenário humano aprender→reload→castar a executar no checklist final
Justification if no automated tests: N/A (testes adicionados)
Residual risk:
  - Execução dos EditMode tests no Unity Test Runner não realizada nesta sessão (diferida).
  - CastScroll runtime: callback de cast (castSpell) não wired ao SpellCastService no controller
    (depende do contexto de cast do player em Play Mode); lógica determinística testada, wiring diferido.
  - Geração de assets/itens e entradas de loja Ozzra não executadas (Editor diferido).
```

---

## Dependency Chain

```text
Original target: fable_07
Dependency chain: nenhuma dependência same-wave pendente (WAVE 06 SpellDatabase + slice SpellCastService já existem e foram reusados)
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: YES
```

---

## Validated ADRs / Game Rules

```text
validated_adrs: []   (required_adrs vazio na spec)
validated_game_rules:
  - docs/game_rules/combat_rules.md — LIDO; sem conflito. A spec adiciona ESTADO DE CONHECIMENTO
    arcano (knownSpellIds + fontes), não altera fórmulas de dano/status/turnos do combat_rules.
    Skill tree não concede spell (consistente com o canon "fonte libera spell").
```

---

## Remaining Work (para fases futuras / humano)

- **Unity (humano):** rodar `CindarsHope/Magic/Generate Spell Learning Items` (cria 4 itens + registra no ItemDatabase); garantir que as 4 SpellDataSO referenciadas existem no catálogo.
- **Entradas de loja Ozzra:** adicionar os scrolls/tome ao `Shop_Ozzra` via gerador de shop existente (DEFERRED_SHOP_WIRING — fora do ponto único de itens deste gerador).
- **CastScroll runtime:** wire do callback `castSpell` do `SpellItemUseController` ao `SpellCastService` com contexto de cast do player (Play Mode).
- **Unity Test Runner:** executar `SpellbookTests` (EditMode) — 25 testes.
- **Play Mode (humano):** cenário aprender→reload→castar; CastScroll consome sem aprender; wand só equipada.
- **F08:** consumirá `KnownSpellIds` para shapes/slots de spell.
- **F14:** UI de grimório listará known spells.
```

---

*BUILD_VALIDATED_WITH_WARNINGS — Phase 2-3 NOT RUN (DEFERRED_TO_FINAL_VALIDATION). Não promovido a implementados/.*
