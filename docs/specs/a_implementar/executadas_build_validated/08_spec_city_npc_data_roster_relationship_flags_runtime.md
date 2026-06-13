# SPEC — City NPC Data Roster Relationship Flags Runtime

> **Spec ID:** `08_spec_city_npc_data_roster_relationship_flags_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 08 — City / NPC / Dialogue / Services  
> **Priority:** P0  
> **Type:** Runtime / Data Contract / City NPC / Roster / Relationship Flags  
> **Domain:** City / NPCData / Functional Classes / Religion / Relationship Flags / Stats  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_08_CITY_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere NPC schedule runtime, companion eligibility, social/romance, city services, dialogue sets, save schema, pets ou scene/prefab de NPC.  
> **Repo lock scope:** `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/NPC/**`, `Assets/_Game/Scripts/Social/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/City/**`, `docs/validation/08_spec_city_npc_data_roster_relationship_flags_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - city schedule runtime;
  - dialogue/rpg interactions;
  - service providers;
  - farm visits;
  - companion eligibility.
  - relationship/social progression.
> **Scope:** definir/endurecer NPCDataSO/NpcData runtime contract com roster, classes funcionais, religião, romance flags, stats e validações canônicas.  
> **Out of scope:** romance/casamento profundo, full social progression, companion AI, pet runtime, visual NPC prefabs/portraits, final dialogue writing.

---

# /speckit.specify

## 1. Contexto

A cidade precisa parecer viva e ser hub econômico, social, narrativo, de progressão e cultura local. O roster define NPCs com classes funcionais do jogo, não D&D, e atributos próprios do projeto. O direction lista campos recomendados para `NPCDataSO`.

Esta spec cria o contrato de dados/validação do NPC. Não cria romance profundo, prefabs, diálogos finais nem pet systems.

---

## 2. Problema

Sem contrato de NPCData:

```text
NPC pode usar classe de D&D como classe mecânica;
Fôlego/Breath/BR pode reaparecer como stat;
relacionamento, romance e casamento podem ser misturados;
NPC casado pode virar candidato por erro;
serviço pode depender de string solta;
religião pessoal pode faltar e quebrar diálogo/festival/reputação;
companion eligibility pode ser hardcoded pelo nome;
pet data pode se misturar com NPC data.
```

---

## 3. Objetivo

Criar/endurecer:

```text
NPCDataSO/NpcDefinition;
FunctionalGameplayClass;
NpcRoleTags;
NpcServiceTags;
ReligionProfile;
RelationshipStatus;
RomanceEligibility;
MarriageEligibility;
NpcStats;
ScheduleId/Home/Work refs;
QuestlineId;
DialogueSetId;
FarmVisitRules refs;
validators.
```

---

## 4. Regras de design

```text
NPCs usam classes funcionais do próprio jogo.
NPCs usam atributos/status do próprio jogo.
Não usar D&D como classe mecânica.
Não existe Fôlego/Breath/BR para NPC.
Anya não tem culto público ativo conhecido.
Templo público principal é Kanthor.
Romance é foco em vínculo/confiança/quests/rotina; sem conteúdo explícito.
Pet é sistema separado e deferido.
```

---

## 5. User stories / engineering stories

```text
Como designer, quero declarar cada NPC com classe funcional, tags, religião, serviço e relacionamento.
Como runtime, quero consultar NPC por serviço, agenda, diálogo, farm visit ou companion eligibility.
Como validator, quero impedir classe D&D, Fôlego/Breath/BR e romance inválido.
Como save/load, quero persistir estado mutável separado de definição estática.
```

---

## 6. Escopo

Inclui:

```text
NpcDefinition/NPCDataSO contract;
functional class enum;
relationship/romance flags;
religion profile;
stats without Breath;
home/work/schedule/dialogue refs;
service tags;
farm visit rule refs;
validators/tests.
```

Não inclui:

```text
full social relationship runtime;
romance/casamento implementation;
dialogue text content;
NPC prefabs/portraits;
pet runtime;
companion AI/jobs;
city scene placement.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Cidade é hub econômico, social, narrativo, de progressão e cultural.
- NPCs usam classes funcionais do jogo, não classes de D&D.
- NPCs seguem a decisão canônica de que Fôlego/Breath/BR não é atributo, recurso, barra, custo, campo de save/load ou stat.
- Cada NPC deve declarar classe primária, classe secundária e tags de serviço.
- Cada NPC deve declarar deus cultuado/principal, simpatias e deuses de que desconfia/não gosta.
- Campos recomendados incluem NpcId, DisplayName, Gender, AgeBand, RaceId, SubraceId, GameplayClassPrimary, GameplayClassSecondary, RoleTags, ServiceTags, DeityWorshipped, relationship flags, CanVisitFarm, stats, schedule/home/work, questline, gifts, dialogue and portrait.

### Deferred / future from directions

- Romance/casamento profundo.
- NPC prefabs/portraits.
- Dialogue writing final.
- Social point progression.
- Companion behavior.
- Pets.

### Explicitly not redefined here

- Schedule runtime.
- Dialogue runtime.
- Service runtime.
- Farm visit runtime.
- Quest runtime.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 FunctionalGameplayClass

```text
Plantador
Colhedor
Pescador
Lenhador
Minerador
Artesao
Explorador
Construtor
Tratador
Comerciante
Escriba
Curandeiro
Guardiao
Combatente
Pesquisador
Musico
Alquimista
```

Proibido como classe mecânica:

```text
Cleric
Paladin
Fighter
Wizard
Rogue
Bard
Druid
Ranger
Warlock
Sorcerer
Monk
Barbarian
```

### 7.2 RelationshipStatus

```text
MarriedToNpc
RomanceEligibleAnyPlayerGender
UnavailableForRomance
TooYoungOrNarrativelyBlocked
LateRomanceEligible
```

### 7.3 NpcDefinition

```text
NpcId
DisplayName
Gender
AgeBand
RaceId
SubraceId
GameplayClassPrimary
GameplayClassSecondary optional
RoleTags[]
ServiceTags[]
DeityWorshipped
DeitySympathy[]
DeityDisliked[]
RelationshipStatus
SpouseNpcId optional
RomanceEligibility
MarriageEligibility
CanVisitFarm
CanMoveToFarmAfterMarriage
FarmVisitRules[]
BaseHP
BaseMP
BaseStamina
Attributes
StatusResistances[]
StatusWeaknesses[]
CombatProfileId optional
ScheduleId
HomeLocationId
WorkLocationId
QuestlineId optional
GiftPreferences
DialogueSetId
PortraitSpriteId
```

### 7.4 NpcStats

```text
HP
MP
Stamina
Forca
Constituicao
Destreza
Inteligencia
Vontade
Carisma
```

Explicitly forbidden:

```text
Folego
Breath
BR
```

---

## 8. Roster validation rules

```text
NpcId must be stable and unique.
Every NPC must have DisplayName, class primary, role/service tags, religion profile, schedule/home/work references when applicable.
MarriedToNpc requires SpouseNpcId.
NPCs in fixed couples must not be RomanceEligibleAnyPlayerGender.
TooYoungOrNarrativelyBlocked must not be MarriageEligible.
RomanceEligibleAnyPlayerGender does not implement romance by itself.
Anya may appear as sympathy/lore, but no active public temple/cult service is created here.
```

---

## 9. Canon roster anchors

Validators should accept at least the active roster IDs:

```text
npc_corvus
npc_mara
npc_sylveth
npc_brumdar
npc_nimble
npc_gurd
npc_hund
npc_ozzra
npc_gruta
npc_zrix
npc_yael
npc_thalindra
npc_dagna
npc_pip
npc_alaric
npc_mirela
npc_renko
npc_eiran
npc_liora
npc_orlan
npc_savra
npc_tovin
npc_maelor
```

The spec does not require creating all data assets now; it requires contract and validator readiness.

---

## 10. Save/load

Definition data is static. Save/load should persist only mutable state elsewhere:

```text
relationship points/state;
questline progress;
availability flags;
temporary location override;
farm visit event state;
service unlock flags;
companion unlock state;
```

Do not persist:

```text
NPC prefab reference;
portrait object reference;
static NpcDefinition copy;
D&D class name;
pet state.
```

---

## 11. Criteria

```text
NPCData contract exists or is hardened.
Functional class taxonomy exists.
D&D class names are rejected as mechanical classes.
Folego/Breath/BR rejected.
Relationship flags are explicit.
Fixed couples cannot become romance candidates.
Religion profile supports dialogue/reputation/festivals.
Tests cover valid roster, invalid D&D class, invalid Breath stat, fixed couples and Anya/Kanthor guardrails.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/NpcDefinition.cs
Assets/_Game/Scripts/NPC/FunctionalGameplayClass.cs
Assets/_Game/Scripts/NPC/NpcRelationshipStatus.cs
Assets/_Game/Scripts/NPC/NpcReligionProfile.cs
Assets/_Game/Scripts/NPC/NpcStats.cs
Assets/_Game/Scripts/NPC/NpcDefinitionValidator.cs
Assets/_Game/Tests/EditMode/City/NpcDefinitionValidationTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/City/**
docs/validation/08_spec_city_npc_data_roster_relationship_flags_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar NPCData/NpcDefinition/Relationship systems.
2. Consolidar functional class enum and NPC definition contract.
3. Implementar validators for classes, stats, relationship and religion.
4. Garantir no pets and no deep romance implementation.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - schedule runtime;
  - dialogue runtime;
  - companion eligibility;
  - social/romance;
  - services.
- Reason: NPCData é fundação consumida por todos os sistemas de cidade.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless existing save stores NPC static data incorrectly; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default; this is data/validator.
```

---

## 20. Riscos

```text
Risco: validator conflicts with existing placeholder NPC data.
Mitigação: report violations before mass data rewrite.

Risco: D&D class names already exist as flavor.
Mitigação: allow narrative tags only if not mechanical class.

Risco: pets mixed with Tratador class.
Mitigação: pets remain deferred.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar NPC data systems.
- [ ] T003 — Consolidar functional class taxonomy.
- [ ] T004 — Consolidar NpcDefinition contract.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a NPC data/roster/relationship flags foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de NPC data/roster/relationship flags? | Arquivos alterados e justificativa. | PARTIAL |
| Canon de cidade | Kanthor/Anya/classes funcionais/sem D&D/sem Fôlego foram preservados? | Checklist no report. | PARTIAL |
| Pets | A execução não criou runtime/UI/save/data asset de pets? | Checklist explícito no report. | BLOCKED se violar |
| Romance | A execução não criou romance/casamento profundo fora do escopo? | Checklist explícito no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/08_spec_city_npc_data_roster_relationship_flags_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "NpcDefinition|NPCData|FunctionalGameplayClass|RelationshipStatus|RomanceEligible|MarriedToNpc|DeityWorshipped|Folego|Breath|BR|D&D|Cleric|Paladin" Assets/_Game/Scripts docs/design docs/specs
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a NPC data/roster/relationship flags existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, agenda, serviços, diálogo e UI projection permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Canon protection

```text
Given a spec toca NPC, serviço, religião, cidade, diálogo ou agenda
When validar os dados
Then templo público principal permanece Kanthor
And Anya não ganha templo/altar ativo independente
And NPCs usam classes funcionais do jogo, não classes de D&D
And Fôlego/Breath/BR não é criado como stat de NPC
And pets permanecem deferidos.
```

### Scenario 4 — Invalid state

```text
Given NPC, agenda, serviço, diálogo, farm visit, porta, cama, licença ou contrato inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não corrompe save, não libera serviço indevido e não bloqueia progresso sem fallback.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- NPC with D&D class as mechanical class.
- NPC with Folego/Breath/BR stat.
- Fixed married NPC marked romance eligible.
- TooYoung/NarrativelyBlocked marked marriage eligible.
- Anya active public temple/service created.
- NpcId not stable.
- Pet state mixed with NPC definition.
- Static NPC definition persisted as mutable save state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — City NPC Data Roster Relationship Flags Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Canon compliance
- Kanthor public temple preserved:
- Anya altar/temple not created:
- Functional classes used:
- Folego/Breath/BR not created:
- Pets not implemented:
- Deep romance/marriage not implemented:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Canon protection:
- Invalid state:
- Save/load safety:
- Visual/final scenario:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever City/NPC/Dialogue/Shop/Quest system canônico existente.
5. A implementação criar templo/altar ativo independente de Anya.
6. A implementação usar classe de D&D como classe mecânica de NPC.
7. A implementação recriar Fôlego/Breath/BR como stat/recurso/campo de NPC.
8. A implementação criar pets runtime/UI/save/data assets.
9. A implementação criar romance/casamento profundo ou conteúdo explícito.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Canon Guardrail Matrix

| Rule | Validation |
|---|---|
| Kanthor is public temple | city service/building specs must preserve |
| Anya no active public temple | block Anya temple/altar service |
| Functional classes only | reject D&D mechanical class |
| No Breath stat | reject Folego/Breath/BR |
| Fixed couples protected | married NPC not romance candidate |
| Pets deferred | no pet runtime/data save |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design docs/specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, NPC data validation and relationship guardrail logic is deterministic.
- Requires EditMode tests: YES for valid/invalid class/stat/relationship/canon tests.
- Requires PlayMode automated or final human scenario: NO by default; data/validator only.
- Requires regression test: YES if fixing existing NPC data validation bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver CityScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no D&D class; no Breath stat; no Anya active temple.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/08_spec_city_npc_data_roster_relationship_flags_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não criar templo/altar ativo independente de Anya.
Não usar classes de D&D como classes mecânicas.
Não recriar Fôlego/Breath/BR em NPC.
Não implementar pets.
Não implementar romance/casamento profundo.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
