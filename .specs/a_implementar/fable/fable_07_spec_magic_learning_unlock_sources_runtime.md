# SPEC — Magia: Aprendizado e Fontes de Desbloqueio (knownSpellIds, Scrolls, Tomes, Wands, Focuses)

> **Spec ID:** `fable_07_spec_magic_learning_unlock_sources_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco C (magia)
> **Priority:** P2
> **Type:** Runtime / Data / Save
> **Domain:** Combat / Player / Inventory
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_C
> **Can run with:** fable_04, fable_09, fable_11, fable_12
> **Must not run with:** fable_08 (consome), fable_13 (save schema lock)
> **Repo lock scope:** `SpellDataSO`, `SpellCastService`, `GameSaveData` (seção nova), `FoodConsumer`/item-use
> **Depends on:**
> - WAVE 06 (SpellDatabase), slice 2026-06-12 (SpellCastService)
> **Blocks:**
> - `fable_08`
> **Scope:** estado de magias conhecidas com fontes de aprendizado canônicas e persistência.
> **Out of scope:** novas magias em massa, spell shapes (F08), ensino por NPC (hook apenas), capstones Anya/Senya.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

`MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md` é canônica: magia não vem de level/skill point;
skill tree libera domínio, **fonte libera spell**. Fontes: LearnableScroll (ensina permanente,
consome, exige pré-requisito), CastScroll (casta e consome, não ensina), Tome (estudo),
Wand/Staff/arma/focus (magia temporária enquanto equipado, sem adicionar knownSpellIds),
NPC teaching e Fonte de Anya (story). No repo, magia = item de categoria Magic equipado na mão
(`ItemDataSO.SpellId` → `SpellCastService`). Não existe knownSpellIds, nem distinção
scroll/tome/wand, nem persistência de conhecimento arcano.

## Problema

Sem estado de conhecimento: toda magia é "item na mão", o que colide com a direction
(equipar tomo ≠ aprender), impede progressão arcana persistente, bloqueia F08 (shapes por
spell conhecida), bloqueia o capstone narrativo da Fonte (WAVE 10 tem os hooks de fragmento
sem nada para desbloquear) e a loja da Ozzra/Yael não pode vender scrolls com semântica.

## Objetivo

Ao final desta spec, deve existir `PlayerSpellbook` (knownSpellIds + fontes temporárias de
item equipado) com seção de save própria, consumo de LearnableScroll/CastScroll/Tome via uso
de item, e o `SpellCastService` deve validar castabilidade por `CanCast(spellId)` =
conhecida OU provida por item equipado — preservando 100% o fluxo atual de item Magic equipado.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
.claude/rules/save-dto-simple-types-only.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- SpellDataSO/SpellDatabaseSO; SpellCastService; ItemDataSO.SpellId/Category.Magic
- EquipmentManager (slots de mão); FoodConsumer (padrão de uso de consumível)
- SaveManager + GameSaveData (SchemaVersion 5; padrão de seção WI-18: DTOs [Serializable],
  Capture/Restore + pending data estático)
- ItemDataInitializer/ItemDataGenerator (padrão de geração de itens)
- WAVE 10: FonteAnya progression state (fragmentos) — hooks para story unlock
Não existe:
- PlayerSpellbook; categorias de item scroll/tome/wand; uso de item que ensina
Auditar Fase 0:
- como FoodConsumer intercepta uso de item (replicar padrão para scrolls/tomes)
- enum ItemCategory atual (adicionar valores é aditivo e seguro?)
```

## Engineering stories

```text
Como jogador, quero aprender uma magia lendo um LearnableScroll comprado da Ozzra e mantê-la após reload.
Como jogador, quero castar um CastScroll de emergência sem aprender nada.
Como portador de wand, quero a spell da wand disponível só enquanto ela está equipada.
Como Fonte de Anya, quero conceder spell por fragmento via API simples (story unlock).
```

## Escopo

```text
Inclui:
- PlayerSpellbook (NOVO, DontDestroyOnLoad via bootstrap pattern WI-15): KnownSpellIds
  (HashSet<string>), TryLearn(spellId, source), CanCast(spellId), GetGrantedByEquipment();
- SpellSourceType enum: LearnableScroll, CastScroll, Tome, EquippedItem, NpcTeaching, FonteStory;
- campos aditivos em ItemDataSO: SpellSourceType + TaughtSpellId (defaults neutros);
- SpellItemUseHandler (NOVO): uso de LearnableScroll (aprende + consome, valida pré-requisito
  de domínio na skill tree via SkillTreeManager), CastScroll (casta via SpellCastService + consome),
  Tome (aprende após N usos — contador no spellbook);
- SpellCastService.CanCast integrado (item Magic equipado continua válido SEM conhecimento —
  comportamento atual preservado como EquippedItem source);
- save: SpellbookSaveData {List<string> KnownSpellIds, List<TomeProgressEntry>} — seção nova
  no GameSaveData, padrão WI-18 (sem migration: default vazio em saves antigos);
- gerador editor: 4 itens exemplares (scroll_learn_fire_spark, scroll_cast_heal_minor,
  tome_ice_studies, wand_spark) + entradas de loja na Ozzra (Shop_Ozzra via gerador de shop);
- API FonteStoryUnlock(spellId) publicada para WAVE 10 hooks (sem implementar a quest);
- eventos: SpellLearnedEvent(spellId, source);
- EditMode tests: aprender/idempotência, CanCast por fonte, tome progress, save round-trip.
```

## Fora de escopo

```text
Não inclui: spell shapes/targeting (F08); ensino por diálogo de NPC (hook só); capstones
Anya/Senya; rebalance de mana; UI de grimório (F14 lista known spells no detail).
```

## Regras de não duplicação

```text
Não criar segundo caminho de cast — SpellCastService permanece único.
Não criar segundo uso-de-item — seguir padrão FoodConsumer (auditar e estender ponto único).
Não criar storage paralelo de spells — SpellDatabase continua catálogo; spellbook é estado.
```

## Critérios de aceite

### CA-1 Aprendizado persistente
- TryLearn idempotente; LearnableScroll consome e adiciona; save/load preserva KnownSpellIds.
- Evidência: testes de round-trip da seção (padrão WI-18) + uso de scroll.

### CA-2 Fontes distintas
- CastScroll casta sem aprender e consome; Tome aprende no 3º uso; wand concede apenas equipada.
- Evidência: testes por fonte.

### CA-3 Zero regressão no fluxo atual
- Item Magic equipado casta exatamente como hoje sem entrada no spellbook.
- Evidência: teste de caracterização do SpellCastService.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Magic/
  PlayerSpellbook.cs          (NOVO)
  SpellSourceType.cs          (NOVO)
  SpellItemUseHandler.cs      (NOVO)
  SpellbookSaveData.cs        (NOVO — DTOs simples)
Assets/_Game/Scripts/Combat/SpellCastService.cs (CanCast)
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs (campos aditivos)
Assets/_Game/Scripts/Save/SaveManager.cs (Capture/Restore da seção — padrão WI-18)
Assets/_Game/Scripts/Editor/Magic/GenerateSpellLearningItems.cs (NOVO)
Assets/_Game/Tests/EditMode/Player/SpellbookTests.cs
```

## Contratos

### Data contracts
`ItemDataSO`: + `SpellSourceType` (default None) + `TaughtSpellId` (default vazio).
### Runtime contracts
`PlayerSpellbook`: API acima; singleton bootstrap RuntimeInitializeOnLoadMethod.
### Event contracts
`SpellLearnedEvent(spellId, source)` — NOVO.
### Save contracts
Seção nova `Spellbook` em GameSaveData; owner PlayerSpellbook; restore order: após Inventory;
default: vazio (saves antigos OK, sem migration); IDs simples apenas.
### UI contracts
N/A (F14 exibirá).

## Sistemas afetados

```text
Magic/Combat cast, Inventory item use, Save/load (seção nova), SkillTree (pré-requisito), Economy (itens de loja), Event bus
```

## Arquivos permitidos

```text
Arquivos da arquitetura + Assets/_Game/Scripts/Core/Events/MagicEvents.cs
Assets/_Game/Scripts/Player/FoodConsumer.cs (somente se o ponto único de uso exigir extensão)
Assets/_Game/Tests/EditMode/Player/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab; *.asset manual (geradores apenas); Packages/ProjectSettings
SpellDataSO.cs (catálogo não muda nesta spec)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar uso de item (FoodConsumer), ItemCategory, padrão de seção WI-18.
### Fase 1 — Spellbook + SaveData + Capture/Restore + testes round-trip.
### Fase 2 — Fontes (scroll/tome/wand) + handler de uso + consumo de inventário.
### Fase 3 — CanCast no SpellCastService (caracterização antes) + FonteStoryUnlock API.
### Fase 4 — Gerador de itens/loja + validação estrita + report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Must not run with: fable_08, fable_13
- Reason: adiciona seção de save (lock de schema) e contratos consumidos por F08.

## Impacto em save/load

```text
Does this change save schema? YES (seção aditiva, sem migration — default vazio)
Does this add a save section? YES (Spellbook; owner PlayerSpellbook; após Inventory)
Does this require migration? NO
Does this persist Unity references? NO (IDs string apenas)
```

## Impacto em eventos

```text
Adds events: YES (SpellLearnedEvent) | Changes existing: NO | Unsubscribe: YES
```

## Impacto em UI/Unity

```text
Changes assets: YES (gerador de itens/loja) | Scenes/prefabs: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: dupla via de cast criar inconsistência (equipado vs conhecido).
Mitigação: CanCast única; teste de matriz fonte×estado.
Risco: seção nova quebrar saves antigos. Mitigação: default vazio + teste de load legado (padrão WI-18).
```

## Rollback

```text
Remover arquivos novos; campos aditivos com default neutro; seção ausente no save é ignorada no load.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar uso de item / ItemCategory / padrão WI-18.
- [ ] T002 — PlayerSpellbook + DTOs + Capture/Restore + round-trip tests.
- [ ] T003 — Campos aditivos no ItemDataSO + SpellSourceType.
- [ ] T004 — SpellItemUseHandler (scroll/castscroll/tome) + consumo.
- [ ] T005 — CanCast no SpellCastService + caracterização do fluxo atual.
- [ ] T006 — FonteStoryUnlock API + SpellLearnedEvent.
- [ ] T007 — GenerateSpellLearningItems + entradas de loja.
- [ ] T008 — csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES (incl. save round-trip e load legado)
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (cast por item equipado intacto)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano aprender→reload→castar

## Definition of Done

```text
Spellbook persistente com 4 fontes funcionais; zero regressão no cast atual; saves antigos
carregam; builds 0E; report criado.
```

## Anti-regressão

```text
Sem refs Unity no save (rule). Saves SchemaVersion 5 antigos carregam com spellbook vazio.
Cast por item Magic equipado inalterado. Skill tree não concede spell (canon).
```

## Notas para execução posterior

```text
F08 usa KnownSpellIds para shapes/slots de spell.
NPC teaching: adicionar DialogueActionType.TeachSpell em spec futura de social/diálogo.
Capstones Anya/Senya entram com promoção da WAVE 19/20.
```
