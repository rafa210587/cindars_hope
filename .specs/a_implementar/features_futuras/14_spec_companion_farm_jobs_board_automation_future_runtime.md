# SPEC — Companion Farm Jobs: Board, Automação Diária e Toggle de Fertilizante (WAVE 14, runtime gated)

> **Spec ID:** `14_spec_companion_farm_jobs_board_automation_future_runtime`
> **Status:** A implementar / SPEC-READY (contrato destravado; execução de runtime GATED na WAVE 14 — decisão 3.12 do Refinamento v3)
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks
> **Priority:** P1
> **Type:** Runtime / Farm Automation / Companion Jobs
> **Domain:** Companion / Farm Jobs / Job Board / Automation Limits / Output Rules
> **Parallelizable:** NO
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE
> **Can run with:** N/A (cruza Farm, Inventory, Economy, Save, Time — lock amplo)
> **Must not run with:** qualquer spec que reescreva o backend de crop/animal/process da fazenda, o backend de inventory/storage, a city schedule, uma save migration, pet jobs, o layout/prefab da UI do board ou o balanceamento final de economia.
> **Repo lock scope:**
> - `Assets/_Game/Scripts/Companions/**` (jobs/board — REUSE/HARDEN, ver Estado atual do repo)
> - `Assets/_Game/Scripts/Companions/FarmJobs/**` (subpasta nova, se a Fase 0 confirmar que consolidar lá é mais limpo que manter na raiz Companions/)
> - `Assets/_Game/Scripts/Save/Providers/**` (CompanionSaveSectionProvider — precedente HotbarSectionProvider)
> - `Assets/_Game/Tests/EditMode/Companions/**`
> - `docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md`
> **Depends on:**
> - `14_spec_companion_eligibility_recruitment_state_save_future_runtime` (DONA do round-trip de save / `CompanionSaveSectionProvider`; esta spec PRECISA do provider para persistir `JobBoardState`/assignments — se a sibling ainda não o criou, esta spec pode criá-lo seguindo o precedente `HotbarSectionProvider`, em coordenação, sem duplicar)
> - `CompanionFarmJobType` / `CompanionFarmJobDefinition` / `CompanionFarmJobAssignment` / `JobBoardState` / `CompanionJobBoardService` (EXISTENTES — REUSE/HARDEN, não recriar; ver Estado atual do repo)
> - `CompanionAvailabilityResolver` + `CompanionBondState` (EXISTENTES — fonte de disponibilidade, fadiga, injury, JobRank)
> - GameEventBus (existente — canal único de comunicação de gameplay; ADR-0007)
> - Backend de Farm/Inventory/Economy (EXISTENTE — esta spec lê estado real, não reescreve)
> **Blocks:**
> - job board UI (layout/prefab — `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime`)
> - validações finais de farm automation
> - companion relationship / job rank progression (consumidores de `RelationshipGain`/`JobRank`)
> - economy anti-exploit final
> - day-transition jobs (gancho de automação diária no avanço de dia)
> **Scope:** endurecer (HARDEN) o runtime EXISTENTE dos 9 jobs de fazenda por companion — board + atribuição (assignment) + automação diária + execução com validação dry-run + output/rendimento derivado de estado real + fadiga por trabalho + múltiplos companions na fazenda — e ADICIONAR o **toggle de auto-fertilização por job de Planter (default OFF)** que consome fertilizante do storage AUTORIZADO (decisão 3.8/5.5), tudo com round-trip de save via `CompanionSaveSectionProvider`.
> **Out of scope:** reescrita do backend de crop/animal/process; reescrita do backend de inventory/storage; pet jobs; sistema de relacionamento/romance/spouse completo; tuning final de economia; layout/prefab visual do board; save migration de schema.

required_adrs: [ADR-0006, ADR-0007]
required_game_rules: [farm_rules.md, save_rules.md, event_rules.md]

---

# EMENDA 2026-06-13-V3 (Refinamento v3) — vinculante

> **Fonte vinculante:** `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, BLOCO 3 (decisões 3.8, 3.11, 3.12) e BLOCO 5 (decisão 5.5).
> **Direction emendada:** `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md` → seções **V3.7** (fertilizante, decisão 3.8, linhas 1577-1588) e **V3.8** (Bond/JobRank, decisão 3.11, linhas 1590-1600).
> **Catálogo de papéis (artefato A7):** `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` → §3.8 FarmWorker/Plantador/Colhedor (linhas 374-402), §3.10 Miner (linhas 437-464), §3.11 AnimalCaretaker, §3.12 Crafter/Builder.

Esta spec deixou de ser "future mapped" e passou a **SPEC-READY** (decisão 3.12): contrato destravado; execução de runtime gated na **WAVE 14**. Decisões v3 que esta spec DEVE refletir:

```text
3.8 — Fertilizante raro automático: TOGGLE por job de Planter, DEFAULT OFF (executa o SIM da 5.5).
        Adicionar ao CompanionFarmJobDefinition do job de Planter (CompanionFarmJobType.Planter)
        um toggle de auto-fertilização (default OFF). Quando LIGADO, consome fertilizante do
        STORAGE AUTORIZADO do job (§9 storage access policy) — NUNCA cria do nada (§8 output
        rules), NUNCA acessa inventário do jogador sem comando. Sem fertilizante no storage
        autorizado, o job planta SEM fertilizar (NÃO falha por isso). Cobrir por EditMode test.

3.11 — Bond 0-5 e JobRank separados: qualidade/output do job pode escalar por Bond e por JobRank
        (trilhas DISTINTAS; o código já tem CompanionBondState.BondLevel e .JobRank, e
        CompanionSaveEntry.BondLevel/.JobRank/.CaveRank). Os caps de automação (capacidade do
        board / stamina / tempo / DailyLimit) continuam valendo INDEPENDENTE de bond/rank.

3.12 — Manter WAVE 14; execução gated. A spec fica DENSA e pronta, mas permanece em
        features_futuras/ até a WAVE 14 ou decisão humana explícita de antecipar.
```

Notas de reconciliação de código (re-auditoria 2026-06-13, COMPANIONS_DIRECTION V3.9):

```text
- Os 9 jobs de fazenda JÁ ESTÃO implementados como contratos C# puros (CompanionFarmJobType
  com 9 valores; CompanionFarmJobDefinition; CompanionFarmJobAssignment; JobBoardState;
  CompanionJobBoardService com ValidateJobExecution/RecordJobExecution). O executor deve
  REUSE/HARDEN — NÃO recriar enum, definition, assignment, board nem service.
- O round-trip de save (CompanionSaveSectionProvider, precedente HotbarSectionProvider) é
  dívida compartilhada com a sibling de eligibility/state/save (acréscimo #3 / V3.9.1). Esta
  spec persiste o JobBoardState/assignments por esse provider; se a sibling ainda não o criou,
  criar aqui SEM duplicar (um único provider de companions).
- Romance/Spouse continuam FLAGS de elegibilidade (CompanionEligibilityFlags.CanBeRomanceCompanion/
  CanBeSpouseCompanion) e NÃO papéis no enum CompanionRole (acréscimo #4). Esta spec NÃO toca esse enum.
```

---

# /speckit.specify

## Contexto

Decisão 3.12 (`docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, BLOCO 3): as quatro specs
`14_spec_companion_*` deixam de ser "future mapped" e viram **spec-ready**, com execução de
runtime gated na WAVE 14. Companion, neste canon, é um **NPC da cidade com vínculo** (não
animal, não summon): 1 ativo na caverna, mas **múltiplos** podem trabalhar em jobs de fazenda
simultaneamente. Cada papel (`COMPANION_ROLES_CATALOG_v1.0.md`) entrega bônus de combate +
bônus fora de combate + um conjunto de ações; os papéis de fazenda relevantes a esta spec são
FarmWorker/Plantador/Colhedor (§3.8), Miner (§3.10), AnimalCaretaker (§3.11) e Crafter/Builder
(§3.12).

A re-auditoria de 2026-06-13 confirmou que o **núcleo de jobs já existe como código C# puro e
testável** (ver Estado atual do repo). Portanto esta spec NÃO cria o sistema do zero: ela
**endurece** (HARDEN) o que existe, fecha lacunas confirmadas (round-trip de save; output
derivado de estado real; automação diária no avanço de dia; múltiplos companions; toggle de
fertilizante da decisão 3.8) e ancora tudo nas invariantes do projeto (sem `GameObject.Find`/
`FindObjectOfType`, comunicação por `GameEventBus`, save DTOs com tipos simples + IDs).

## Problema

Companions podem ajudar em jobs da fazenda, mas devem **reduzir repetição sem remover
planejamento**. Hoje os contratos C# existem mas há lacunas e riscos não fechados:

```text
- o JobBoardState e os assignments NÃO têm round-trip de save (nenhum CompanionSaveSectionProvider);
  ao recarregar, a programação diária some;
- a automação diária (gancho no avanço de dia) não está ligada ao reset/execução de jobs;
- output/rendimento precisa ser DERIVADO do estado real do mundo (seed real, crop maduro real,
  nó de recurso real) — sem isso, o companion poderia "criar item do nada";
- o toggle de fertilizante raro automático (decisão 3.8) NÃO existe no CompanionFarmJobDefinition;
- múltiplos companions na fazenda precisam de caps de board (capacidade/stamina/tempo) que não
  estourem economia;
- duplicação de output no day transition / após reload (idempotência) não está testada.
```

Sem fechar isso: companion planta/colhe tudo sozinho no early game; job cria item sem world
state real; storage é acessado sem permissão; job ignora clima/season/horário/ferramenta;
companion trabalha com stamina infinita; output gera economia infinita; e o board não persiste.

## Objetivo

Ao final desta spec o runtime EXISTENTE de jobs de fazenda deve, dentro do escopo gated da WAVE 14:

```text
1. REUSE/HARDEN os contratos existentes (CompanionFarmJobType[9], CompanionFarmJobDefinition,
   CompanionFarmJobAssignment, JobBoardState, CompanionJobBoardService) — sem recriar.
2. Atribuir job com área marcada, ferramenta/estação, horário, StaminaBudget e DailyLimit, com
   resultado esperado previsível; suportar MÚLTIPLOS companions na fazenda.
3. Executar via VALIDAÇÃO dry-run primeiro (ValidateJobExecution já existe) e só então aplicar
   o efeito, derivando OUTPUT do estado real do mundo (seed/crop/nó reais; nunca "do nada").
4. Aplicar fadiga por trabalho (FatigueGain) em CompanionBondState.Fatigue; respeitar o gate de
   fadiga já existente (CompanionAvailabilityResolver: Fatigue > 80 = indisponível).
5. ADICIONAR o toggle de auto-fertilização por job de Planter (default OFF) que consome
   fertilizante do storage AUTORIZADO; sem fertilizante = planta sem fertilizar (não falha).
6. Resetar a programação diária no avanço de dia (RepeatPolicy) com IDEMPOTÊNCIA (sem duplicar
   output após reload/transição de dia).
7. Persistir JobBoardState/assignments por CompanionSaveSectionProvider (precedente
   HotbarSectionProvider) — DTOs simples + IDs, sem refs Unity (ADR-0006).
8. Comunicar marcos (job atribuído/concluído) APENAS via GameEventBus (ADR-0007), com unsubscribe.
```

Tudo gated: a spec fica densa e pronta, mas permanece em `features_futuras/` (decisão 3.12).

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (BLOCO 3: 3.8/3.11/3.12; BLOCO 5: 5.5)
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md (V3.7 linhas 1577-1588; V3.8 linhas 1590-1600; V3.9.1 round-trip de save)
docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md (§3.8 FarmWorker linhas 374-402; §3.10 Miner linhas 437-464; §3.11/§3.12)
docs/design/SPEC_SOURCE_MAP.md (mapeamento de domínio companion → COMPANIONS_DIRECTION)
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md (estado real de crop; ciclo diário)
docs/game_rules/farm_rules.md (layout de plots; ciclo de crescimento; reset diário; storage da fazenda separado da inventory; persistência)
docs/game_rules/save_rules.md (DTOs simples + IDs; sem refs Unity; resolução no load)
docs/game_rules/event_rules.md (comunicação só via GameEventBus; unsubscribe)
docs/decisions/ADR-0006-save-data-contracts-simple-dtos.md
docs/decisions/ADR-0007-event-bus-gameplay-communication.md
.claude/skills/save-section-provider/SKILL.md (padrão HotbarSectionProvider)
.claude/skills/economy-balance-tuning/SKILL.md (anti-exploit de output)
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
```

## Estado atual do repo

```text
EXISTE e NÃO RECRIAR (confirmado pela re-auditoria 2026-06-13 — REUSE/HARDEN):
- Assets/_Game/Scripts/Companions/CompanionFarmJobType.cs:3-14 — enum com OS 9 JOBS:
  Planter(0), Waterer(1), Harvester(2), Lumberjack(3), Fisherman(4), AnimalCaretaker(5),
  Crafter(6), Organizer(7), Miner(8). NÃO alterar a ordem nem os valores (estável p/ save);
  adições só no FIM, se a Fase 0 justificar.
- Assets/_Game/Scripts/Companions/CompanionFarmJobDefinition.cs — já carrega:
  - AllowedArea (AreaId, GridX/Y, SizeX/Y) — linhas 5-12;
  - AllowedResources (AllowedItemIds, ForbiddenItemIds, MaxDailyConsumption) — linhas 14-19;
  - OutputRules (DestinationStorageId, AllowedOutputItemIds, AutoSell=false) — linhas 21-26;
  - CompanionFarmJobDefinition (JobId, JobType, RequiredToolOrStation, StartTimeHour, EndTimeHour,
    StaminaBudgetPerDay, RelationshipGainPerJob, FatigueGainPerJob, Area, Resources, OutputRules,
    DailyLimit=3, Priority=1) — linhas 28-49;
  - helpers CanExecuteAtTime (51-54), IsItemAllowed (56-65), CanOutputItem (67-73).
  O toggle de fertilizante (3.8) é um ACRÉSCIMO de campo aqui (default OFF) — não recriar a classe.
- Assets/_Game/Scripts/Companions/CompanionFarmJobAssignment.cs — JobAssignmentStatus
  (Assigned/InProgress/Completed/Failed/Paused/Cancelled, linhas 3-11) e CompanionFarmJobAssignment
  (AssignmentId, CompanionId, JobId, IsActive, ExecutionCount, FailureCount, Status,
  StaminaUsedToday, CurrentDay; ResetDaily(); CanExecuteMoreToday(dailyLimit);
  HasStaminaForJob(budget, stamina)) — linhas 13-54. REUSE.
- Assets/_Game/Scripts/Companions/JobBoardState.cs — AvailableJobs, CompanionAssignments
  (Dictionary companionId -> List<assignment> — SUPORTA múltiplos companions), CurrentDay;
  AddJob/AssignJobToCompanion/GetCompanionAssignments/GetJobDefinition/ResetDailyState. REUSE.
- Assets/_Game/Scripts/Companions/CompanionJobBoardService.cs — JobExecutionResult enum
  (Success/InsufficientStamina/AreaRestrictionViolation/TimeWindowClosed/DailyLimitExceeded/
  CompanionUnavailable/JobNotFound/ResourceRestrictionViolation, linhas 6-16) e o serviço com
  ValidateJobExecution (33-61, DRY-RUN), RecordJobExecution (63-84), GetActiveJobsForCompanion
  (86-90), DeactivateJobAssignment (92-101), RegisterCompanionStamina (28-30). REUSE/HARDEN.
- Assets/_Game/Scripts/Companions/CompanionAvailabilityResolver.cs — ResolveAvailability já gate
  por injury (Incapacitated) e FADIGA (Fatigue > 80, linha 31). REUSE para "companion disponível".
- Assets/_Game/Scripts/Companions/CompanionBondState.cs — BondLevel, Fatigue, InjuryState,
  JobRank, CaveRank (campos já distintos; decisão 3.11). REUSE como fonte de bond/rank/fatigue.
- Assets/_Game/Scripts/Save/SaveData.cs:225-245 — CompanionManagerSaveData { List<CompanionSaveEntry> }
  e CompanionSaveEntry (CompanionId, NpcId, UnlockState, UnlockedRoles, UnlockedByQuestIds,
  BondLevel, TrustPoints, Fatigue, InjuryState, LastInteractionDay, JobRank, CaveRank). DTO simples.
- Assets/_Game/Scripts/Save/ISaveSectionProvider.cs + Save/Providers/HotbarSectionProvider.cs —
  PRECEDENTE EXATO do provider a criar (Capture(GameSaveData)/Restore(object), ProviderId).
- GameEventBus (Publish/Subscribe) — canal único de gameplay (ADR-0007). Backend de Farm/
  Inventory/Economy — fonte do ESTADO REAL (crops, seeds, nós de recurso, storage).

NÃO EXISTE (lacunas confirmadas — escopo de HARDEN/ADD desta spec):
- CompanionSaveSectionProvider para companions (só HotbarSectionProvider existe). O JobBoardState/
  assignments NÃO têm round-trip de save hoje.
- Campo/toggle de auto-fertilização no CompanionFarmJobDefinition (decisão 3.8) — ausente.
- Gancho de automação diária: ResetDailyState existe em JobBoardState, mas não há serviço que o
  acione no avanço de dia nem que execute os jobs com IDEMPOTÊNCIA após reload.
- Derivação de OUTPUT a partir do estado real do mundo (a CanOutputItem só valida lista de IDs;
  falta o passo que consome seed real / exige crop maduro real / exige nó real).
- Eventos de marco (CompanionJobAssignedEvent / CompanionJobCompletedEvent) no GameEventBus.

AUDITAR Fase 0 (registrar achados no execution report, classificando EXISTING_CANONICAL/
EXISTING_PARTIAL/MISSING_SAFE_TO_CREATE/MISSING_BUT_DEFER/CONFLICT):
- como obter o ESTADO REAL de crop/seed/nó de recurso e do storage da fazenda SEM GameObject.Find/
  FindObjectOfType (via interface/adapter injetado, GameBootstrap ou serialized ref);
- onde fica o "fertilizante raro" como item/ID e qual storage é o "autorizado" do job de Planter;
- de qual sinal vem o avanço de dia (evento de day transition já publicado?) para acionar o reset/
  execução diária — consumir o existente, NÃO inventar relógio;
- se a sibling 14_spec_companion_eligibility_recruitment_state_save_* já criou o
  CompanionSaveSectionProvider; se sim, ESTENDER (não duplicar); se não, criar aqui.
```

## Engineering stories

```text
Como jogador, quero atribuir a um companion um job com área marcada, horário, ferramenta/estação,
  StaminaBudget e DailyLimit, e ver o resultado esperado (tempo/custo/output) antes de confirmar.
Como jogador, quero ligar/desligar a auto-fertilização no job de Planter (default OFF), sabendo
  que ela só consome fertilizante do storage autorizado e nunca do meu inventário.
Como jogador, quero atribuir jobs a VÁRIOS companions na fazenda ao mesmo tempo, limitado pela
  capacidade do board, sem que isso quebre a economia.
Como sistema de fazenda, quero validar (dry-run) se crop/seed/estação/nó existem no estado REAL
  antes de produzir qualquer output.
Como companion, quero consumir stamina/fadiga e respeitar vínculo (Bond) / JobRank, ficando
  indisponível quando minha fadiga passa do limite existente (> 80).
Como economia, quero impedir output infinito: caps de board, DailyLimit, StaminaBudget, sem
  auto-sell e sem shipping por padrão.
Como save/load, quero preservar a programação do dia (board + assignments) por um SaveSectionProvider
  e NÃO duplicar output após reload nem na transição de dia (idempotência).
Como arquitetura, quero comunicação só por GameEventBus e DTOs de save com tipos simples + IDs.
```

## Escopo

```text
Inclui (REUSE/HARDEN + ADD — sempre o menor conjunto seguro de arquivos):
- HARDEN do CompanionJobBoardService: pipeline "ValidateJobExecution (dry-run) -> aplicar efeito
  derivado do estado real -> RecordJobExecution", com idempotência por (CompanionId, JobId, dia,
  ExecutionCount). Não duplicar a lógica de validação já existente; estendê-la.
- DERIVAÇÃO de output do estado real do mundo via um adapter/porta injetada (interface) que
  expõe: existe seed real? crop maduro na área? nó de recurso real/não-depletado? capacidade do
  storage autorizado? — implementação concreta liga ao backend de Farm/Inventory (sem busca global).
- TOGGLE de auto-fertilização (decisão 3.8): campo bool no CompanionFarmJobDefinition (DEFAULT
  OFF) específico do job de Planter (CompanionFarmJobType.Planter); quando ON, consome fertilizante
  do storage AUTORIZADO; sem fertilizante = planta sem fertilizar (NÃO falha). EditMode test
  cobrindo ON-com-fertilizante / ON-sem-fertilizante / OFF.
- AUTOMAÇÃO diária: serviço que consome o sinal de avanço de dia EXISTENTE (Fase 0) e chama
  JobBoardState.ResetDailyState + executa os assignments com RepeatPolicy aplicável, idempotente.
- MÚLTIPLOS companions: o board já usa Dictionary companionId -> assignments; HARDEN os caps
  (capacidade do board / soma de stamina / janela de tempo) para que N companions não estourem
  economia. Caps valem INDEPENDENTE de Bond/JobRank (decisão 3.11); Bond/JobRank só modulam
  QUALIDADE/output, nunca removem os caps.
- FADIGA por trabalho: aplicar FatigueGainPerJob em CompanionBondState.Fatigue no sucesso; o gate
  de indisponibilidade por fadiga (> 80) já existe no resolver — respeitar, não duplicar.
- SAVE: CompanionSaveSectionProvider (precedente HotbarSectionProvider) com DTO simples para
  JobBoardState/assignments (IDs + tipos simples; sem refs Unity). Round-trip capture/restore.
- EVENTOS: CompanionJobAssignedEvent / CompanionJobCompletedEvent no GameEventBus (Publish no
  serviço; consumidores externos fazem Subscribe/unsubscribe).
- EditMode tests: assignment válido; missing tool/station; missing input/seed; área/recurso
  inválido; janela de horário fechada; DailyLimit/capacidade do board; StaminaBudget; storage
  policy; output derivado de estado real; idempotência após reload/day transition; sem auto-sell;
  toggle de fertilizante (3 casos); múltiplos companions dentro do cap; round-trip do provider.
```

## Fora de escopo

```text
- Reescrita do backend de crop/animal/process da fazenda (esta spec LÊ o estado real via adapter);
- Reescrita do backend de inventory/storage (LÊ/grava só via storage autorizado do job);
- Pet jobs (Pets é sistema separado/deferido — NÃO criar pet runtime/save/HUD/assets);
- Sistema de relacionamento/romance/spouse completo (Romance/Spouse são FLAGS de elegibilidade,
  NÃO papéis — não tocar CompanionRole nem CompanionEligibilityFlags semanticamente);
- Tuning final de economia (preços/curvas finais ficam para spec de economy balance);
- AnimalCaretaker (job 5): requer sistema de animais futuro — BLOQUEADO agora salvo se o backend
  estiver disponível na Fase 0; caso ausente, retorna SkippedUnavailable (não falha o board);
- Layout/prefab visual do job board e qualquer .unity/.prefab/.asset;
- Save migration de schema (se um novo estado persistido EXIGIR migration, PARAR e reportar);
- Mudança de relógio/day-cycle (apenas CONSUMIR o sinal de avanço de dia existente).
```

## Regras de não duplicação

```text
- NÃO recriar CompanionFarmJobType / CompanionFarmJobDefinition / CompanionFarmJobAssignment /
  JobBoardState / CompanionJobBoardService — todos existem; HARDEN/estender no lugar.
- NÃO criar um segundo enum de resultado de job (JobExecutionResult já existe) nem um segundo
  board/serviço de jobs.
- NÃO criar um segundo SaveSectionProvider de companions — um único CompanionSaveSectionProvider
  (estender o da sibling se já existir).
- NÃO criar canal de comunicação fora do GameEventBus (ADR-0007 / event_rules.md).
- NÃO recriar o gate de fadiga (CompanionAvailabilityResolver já o tem); reutilizar.
- NÃO inventar evento de day-cycle/relógio; consumir o existente (Fase 0).
- NÃO adicionar Romance/Spouse ao enum CompanionRole (permanecem flags).
```

## Critérios de aceite

### CA-1 — Atribuição validada e múltiplos companions sob cap
- Um job só é atribuído/executado se passar a validação dry-run existente (`ValidateJobExecution`):
  companion disponível, job existente, dentro da janela de horário, abaixo do DailyLimit, com
  StaminaBudget suficiente, dentro da área/recurso permitidos. Múltiplos companions podem ter
  assignments simultâneos, mas a soma respeita a capacidade do board / stamina / tempo.
- Evidência: EditMode tests (válido; missing tool; missing input; área/recurso inválido; janela
  fechada; DailyLimit; capacidade do board com N companions). Spec Compliance Matrix.

### CA-2 — Output derivado de estado real (sem "item do nada")
- Plantar consome SEED real; colher exige crop MADURO na área permitida; minerar/cortar exige nó/
  árvore real e não-depletado; processar exige input real + estação construída. Sem o pré-requisito
  real, o resultado é Skipped* (não produz). Nenhum job cria item do nada, faz auto-sell ou abre
  shipping por padrão (`OutputRules.AutoSell == false`).
- Evidência: EditMode tests de derivação (seed ausente -> SkippedMissingInput; crop imaturo ->
  SkippedUnavailable; nó depletado -> Skipped; AutoSell sempre false).

### CA-3 — Toggle de auto-fertilização (decisão 3.8)
- O job de Planter tem um toggle de auto-fertilização com DEFAULT OFF. Quando ON, consome
  fertilizante do storage AUTORIZADO do job; sem fertilizante no storage autorizado, o job planta
  SEM fertilizar e NÃO falha. Nunca acessa o inventário do jogador sem comando; nunca cria
  fertilizante do nada.
- Evidência: EditMode tests (OFF = não consome; ON + fertilizante = consome e fertiliza; ON sem
  fertilizante = planta sem fertilizar, status não-falho). Cross-ref a V3.7 (linhas 1582-1588).

### CA-4 — Fadiga, Bond e JobRank
- Sucesso de job aplica FatigueGainPerJob em CompanionBondState.Fatigue; com Fatigue > 80 o
  companion fica indisponível (gate existente do resolver). Bond/JobRank modulam QUALIDADE/output
  mas NÃO removem caps de automação (decisão 3.11).
- Evidência: EditMode tests (fadiga acumula e bloqueia disponibilidade; caps independem de Bond/
  JobRank).

### CA-5 — Automação diária idempotente + round-trip de save
- No avanço de dia, o board reseta o estado diário (ResetDailyState) e executa os assignments
  conforme RepeatPolicy, SEM duplicar output após reload ou transição de dia. O JobBoardState/
  assignments persistem por CompanionSaveSectionProvider (DTO simples + IDs; sem refs Unity), com
  round-trip capture/restore.
- Evidência: EditMode tests (idempotência no day transition; idempotência após reload; round-trip
  do provider) + ADR-0006 (DTO simples).

### CA-6 — Comunicação só por GameEventBus
- Marcos (job atribuído / concluído) publicam `CompanionJobAssignedEvent` /
  `CompanionJobCompletedEvent` no GameEventBus; nenhuma chamada direta MonoBehaviour->MonoBehaviour
  de gameplay; subscribers fazem unsubscribe no OnDisable.
- Evidência: EditMode test do publish/contrato dos eventos + nota de unsubscribe (ADR-0007 /
  event_rules.md).

---

# /speckit.plan

## Arquitetura alvo

```text
EXISTENTES — HARDEN/estender (NÃO recriar):
Assets/_Game/Scripts/Companions/CompanionFarmJobType.cs            (enum 9 jobs — estável)
Assets/_Game/Scripts/Companions/CompanionFarmJobDefinition.cs      (+ toggle auto-fertilização Planter, default OFF)
Assets/_Game/Scripts/Companions/CompanionFarmJobAssignment.cs      (reuse; idempotência por ExecutionCount/dia)
Assets/_Game/Scripts/Companions/JobBoardState.cs                   (reuse; caps de capacidade)
Assets/_Game/Scripts/Companions/CompanionJobBoardService.cs        (HARDEN: pipeline validate->aplicar->record; eventos)
Assets/_Game/Scripts/Companions/CompanionAvailabilityResolver.cs   (reuse; gate de fadiga > 80)
Assets/_Game/Scripts/Companions/CompanionBondState.cs              (reuse; Fatigue/Bond/JobRank)

NOVOS (apenas se a Fase 0 confirmar ausência e que consolidar é mais limpo):
Assets/_Game/Scripts/Companions/FarmJobs/IFarmWorldStateAdapter.cs           (porta p/ estado real — sem busca global)
Assets/_Game/Scripts/Companions/FarmJobs/CompanionFarmJobAutomationService.cs(automação diária idempotente; consome day transition)
Assets/_Game/Scripts/Companions/FarmJobs/CompanionFarmJobEvents.cs           (CompanionJobAssignedEvent/CompanionJobCompletedEvent)
Assets/_Game/Scripts/Save/Providers/CompanionSaveSectionProvider.cs          (round-trip; precedente HotbarSectionProvider; estender se a sibling já criou)
Assets/_Game/Tests/EditMode/Companions/CompanionFarmJobsTests.cs             (testes)

DTO de save (em SaveData.cs, tipos simples + IDs — sem refs Unity):
  estado do board/assignments persistível (estender CompanionManagerSaveData OU DTO dedicado,
  decidido na Fase 0 sem quebrar o schema existente; se exigir migration, PARAR e reportar).

docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md
```

## Contratos

### Data contracts (ADR-0006 / save_rules.md)
- DTO de save do board: SOMENTE tipos simples + IDs (string JobId/CompanionId/AssignmentId/
  AreaId/StorageId; int dia/contadores/stamina; bool toggle de fertilizante; enums como int;
  List de DTOs simples). PROIBIDO qualquer ref Unity (GameObject/Transform/MonoBehaviour/SO/Sprite).
- Toggle de auto-fertilização: bool no CompanionFarmJobDefinition, DEFAULT OFF, semântica restrita
  ao job de Planter; persistido como bool.
- `OutputRules.AutoSell` permanece `false` por padrão (linha 25) e esta spec não o liga.

### Runtime contracts
- `CompanionJobBoardService`: manter `ValidateJobExecution` como DRY-RUN puro; o efeito (consumo de
  seed/input, produção de output, fadiga) só após validação OK; `RecordJobExecution` registra
  contadores. Idempotência: não reaplicar efeito para o mesmo (CompanionId, JobId, dia) além do
  ExecutionCount permitido.
- `IFarmWorldStateAdapter` (porta): consultas read-only ao estado real (existe seed X? crop maduro
  na área Y? nó não-depletado? capacidade do storage autorizado?) e comandos mínimos autorizados
  (consumir seed/input do storage autorizado; depositar output no storage autorizado). Implementação
  concreta injetada via GameBootstrap/serialized ref — NUNCA GameObject.Find/FindObjectOfType.
- `CompanionFarmJobAutomationService`: assina o evento de avanço de dia existente; chama
  ResetDailyState + executa assignments idempotentemente; unsubscribe no descarte.

### Event contracts (ADR-0007 / event_rules.md)
- Adiciona: `CompanionJobAssignedEvent`, `CompanionJobCompletedEvent` (structs simples — IDs +
  resultado + outputs como IDs/contagens). Consome (sem alterar): o evento de day transition
  existente. Requer unsubscribe nos OnDisable dos subscribers.

### Save/UI contracts
- Save: round-trip via CompanionSaveSectionProvider (ProviderId "companions" — coordenar com a
  sibling para um único provider). UI: esta spec NÃO cria o board visual (é da sibling de UI);
  apenas expõe o estado consumível.

## Sistemas afetados

```text
Companion jobs/board (HARDEN), Save providers (provider novo), Event bus (2 eventos novos),
Farm/Inventory (LEITURA de estado real via adapter), Economy (caps anti-exploit). NÃO tocados:
backend de crop/inventory/storage, pet, romance/spouse, relógio/day-cycle, CompanionRole enum.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**            (HARDEN dos contratos existentes)
Assets/_Game/Scripts/Companions/FarmJobs/**   (adapter/automation/eventos novos, se Fase 0 confirmar)
Assets/_Game/Scripts/Save/Providers/**        (CompanionSaveSectionProvider)
Assets/_Game/Scripts/Save/SaveData.cs         (DTO simples do board — SEM migration; se exigir, PARAR)
Assets/_Game/Tests/EditMode/Companions/**     (testes)
docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
Packages/** ; ProjectSettings/**
Assets/**/*.unity ; Assets/**/*.prefab ; Assets/**/*.asset (manuais)
Assets/_Game/Scripts/Pets/** (pet é sistema separado/deferido)
CompanionRole.cs (não adicionar Romance/Spouse ao enum — permanecem flags)
Backend de crop/animal/process e de inventory/storage (LER via adapter, não reescrever)
.specs/SPEC_EXECUTION_ORDER.md ; .specs/implementados/** ; docs/refinements/implementados/**
docs/project/CURRENT_STATE.md ; PROJECT_LOG.md ; índices compartilhados (SPEC_REGISTRY, fable_00C, GAME_RULES_INDEX, DECISION_LOG, .specs)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar (registrar no report, classificar achados): contratos existentes (confirmados);
   sinal de avanço de dia existente; fonte do estado real de crop/seed/nó e do storage autorizado;
   ID do fertilizante raro; se a sibling já criou CompanionSaveSectionProvider; AnimalCaretaker
   (backend de animais existe? se não, job 5 = SkippedUnavailable). Decidir REUSE/HARDEN/CREATE/DEFER.
### Fase 1 — Toggle de auto-fertilização no CompanionFarmJobDefinition (bool, default OFF, só Planter) + testes (3 casos).
### Fase 2 — IFarmWorldStateAdapter (porta read-only + comandos mínimos autorizados) + derivação de output do estado real; HARDEN do pipeline no CompanionJobBoardService (validate dry-run -> aplicar -> record; idempotência) + testes.
### Fase 3 — CompanionFarmJobAutomationService (consome day transition; ResetDailyState + execução idempotente; múltiplos companions sob cap; fadiga) + testes.
### Fase 4 — Eventos (CompanionJobAssignedEvent/CompanionJobCompletedEvent) no GameEventBus + teste de contrato/unsubscribe.
### Fase 5 — CompanionSaveSectionProvider + DTO simples do board (sem migration) + round-trip tests.
### Fase 6 — csproj; validações; execution report (Spec Compliance Matrix; Testing Quality Gate; cenário final deferido para FINAL_HUMAN_VALIDATION_BY_WAVE).
```

## Paralelização

- Parallelizable: NO — jobs cruzam Farm, Inventory, Economy, Time e Save (lock amplo).
- Parallel group: WAVE_14_COMPANIONS_FUTURE.
- Must not run with: reescrita de crop/farm runtime; reescrita de inventory/storage; save migration;
  fundação de companion state; pet runtime; UI/prefab do board; economy balance final.
- Shared files/systems que exigem lock: `Companions/**`, `Save/Providers/**`, `SaveData.cs`,
  GameEventBus (2 eventos novos).
- Reason: HARDEN cruza múltiplos domínios e o save; precisa de janela exclusiva na WAVE 14.

## Impacto em save/load

```text
Does this change save schema? PREFERENCIALMENTE NO — persistir o board reusando/estendendo
  CompanionManagerSaveData com tipos simples + IDs. Se um novo estado EXIGIR uma seção/migration
  nova, PARAR e reportar (stop condition) — não inventar migration sem spec de migration.
Does this add a save section? NO por padrão (usa CompanionSaveSectionProvider sobre DTO existente);
  só com migration aprovada.
Does this require migration? NO; se exigir, STOP.
Does this persist Unity references? NO (ADR-0006 / save_rules.md — só IDs e tipos simples).
```

## Impacto em eventos

```text
Adds events: YES — CompanionJobAssignedEvent, CompanionJobCompletedEvent (structs simples).
Changes existing events: NO. Consome (sem alterar) o evento de avanço de dia existente.
Requires unsubscribe pattern: YES (automation service + subscribers em OnDisable).
```

## Impacto em UI/Unity

```text
Changes UI: NO final — apenas expõe estado para o board (UI é da sibling de UI/HUD).
Changes scenes: NO. Changes prefabs: NO. Changes ScriptableObjects/assets: NO.
Requires Play Mode final validation: YES, DEFERIDO — fluxo visual do board / atribuição /
  automação ao dormir; registrar em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION (não pedir validação humana por spec).
```

## Riscos técnicos

```text
Risco: output duplicado no day transition / após reload.
  Mitigação: idempotência por (CompanionId, JobId, dia, ExecutionCount); ResetDailyState no
  avanço de dia; EditMode tests de idempotência (day transition + reload).
Risco: companion "cria item do nada".
  Mitigação: output SEMPRE derivado do estado real via IFarmWorldStateAdapter; sem pré-requisito
  real -> Skipped*; testes de seed/crop/nó ausentes.
Risco: acesso indevido ao inventário do jogador (fertilizante).
  Mitigação: consumo só do storage AUTORIZADO; sem fertilizante = planta sem fertilizar; nunca
  inventário do jogador sem comando (decisão 3.8 / V3.7); teste dos 3 casos.
Risco: economia infinita com N companions.
  Mitigação: caps de capacidade do board / StaminaBudget / DailyLimit / janela de horário,
  INDEPENDENTES de Bond/JobRank; AutoSell sempre false; teste de cap com múltiplos companions.
Risco: tentação de reescrever backend de farm/inventory ou recriar os contratos existentes.
  Mitigação: adapter read-only + Estado atual do repo fixa REUSE/HARDEN; anti-regressão proíbe.
Risco: busca global para achar farm/storage/câmera de estado.
  Mitigação: adapter injetado via GameBootstrap/serialized ref; sem GameObject.Find/FindObjectOfType.
Risco: migration de save acidental.
  Mitigação: DTO simples sobre seção existente; se exigir migration -> STOP/BLOCKED.
Risco: AnimalCaretaker sem backend de animais.
  Mitigação: job 5 retorna SkippedUnavailable (não falha o board); DEFER documentado.
```

## Rollback

```text
Remover o adapter, o automation service, os 2 eventos, o provider e o DTO do board, e reverter o
campo de toggle no CompanionFarmJobDefinition desliga a camada de automação/save. Os contratos
preexistentes (enum/definition/assignment/board/service) permanecem intactos. Nenhuma migration
foi feita, então nenhum save legado é invalidado. As specs siblings perdem o gancho de automação/
save mas seguem com seus próprios escopos.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar contratos existentes (confirmados); sinal de avanço de dia; fonte do
       estado real (crop/seed/nó/storage); ID do fertilizante raro; provider da sibling; backend de
       animais (AnimalCaretaker). Classificar EXISTING_CANONICAL/PARTIAL/MISSING_SAFE/DEFER/CONFLICT.
- [ ] T002 — Toggle de auto-fertilização no CompanionFarmJobDefinition (bool, default OFF, só Planter) + EditMode tests (OFF / ON+fert / ON sem fert).
- [ ] T003 — IFarmWorldStateAdapter + derivação de output de estado real; HARDEN do pipeline no CompanionJobBoardService (validate dry-run -> aplicar -> record; idempotência) + tests.
- [ ] T004 — CompanionFarmJobAutomationService (consome day transition; ResetDailyState + execução idempotente; múltiplos companions sob cap; fadiga) + tests.
- [ ] T005 — Eventos CompanionJobAssignedEvent/CompanionJobCompletedEvent no GameEventBus + teste de contrato/unsubscribe.
- [ ] T006 — CompanionSaveSectionProvider + DTO simples do board (sem migration; se exigir, STOP) + round-trip tests.
- [ ] T007 — csproj; run_strict_validation; execution report (Spec Compliance Matrix, Testing Quality Gate, cenário final deferido a FINAL_HUMAN_VALIDATION_BY_WAVE).
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "Assembly-CSharp build FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "Assembly-CSharp-Editor build FAILED"; exit 1 }
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { Write-Host "Strict validation FAILED"; exit 1 }
```

Unity compile (quando houver alteração Unity C#) — sequencial, nunca paralelo:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Busca local mínima de não-duplicação (registrar saída no report):

```powershell
Select-String -Path "Assets\_Game\Scripts\*","docs\design\*",".specs\*" -Pattern "CompanionFarmJob|JobBoard|AllowedArea|StaminaBudget|OutputRules|StorageAccess|FatigueGain|AnimalCaretaker|ISaveSectionProvider|fertiliz" -Recurse
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (validação/execução/output/cap/idempotência/toggle de fertilizante — tudo C# determinístico)
Changed Unity scene/prefab/asset wiring: NO (adapter injetado via bootstrap é wiring humano de cena, não edição YAML por agente)
Automated tests added/updated: YES (EditMode — assignment válido; missing tool/input; área/recurso; janela; DailyLimit/cap; StaminaBudget; storage policy; output de estado real; idempotência day-transition+reload; sem auto-sell; toggle de fertilizante 3 casos; múltiplos companions sob cap; round-trip do provider)
Automated tests command: Unity Test Runner — EditMode (ou comando local equivalente do repo)
Manual Play Mode scenario: DEFERRED — fluxo visual do board/atribuição/automação ao dormir → docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md (não pedir validação humana por spec)
Justification if no automated tests: N/A (lógica determinística tem EditMode tests)
Residual risk: derivação de output depende do backend real de farm/inventory (adapter); até a WAVE 14 ligar o adapter concreto, a parte de estado-real fica coberta por testes com fake adapter, e o fluxo integrado é DEFERIDO ao Play Mode final. AnimalCaretaker DEFERIDO até existir backend de animais.
```

## Definition of Done

```text
Contratos existentes REUSADOS/ENDURECIDOS (não recriados); toggle de auto-fertilização do Planter
(default OFF, consome só storage autorizado, sem fertilizante = planta sem fertilizar) implementado;
output sempre derivado de estado real (sem item do nada, sem auto-sell); caps de automação
independentes de Bond/JobRank; fadiga aplicada e gate de fadiga > 80 respeitado; automação diária
idempotente (day transition + reload); round-trip de save via CompanionSaveSectionProvider (DTO
simples + IDs, sem refs Unity, sem migration); comunicação só por GameEventBus com unsubscribe;
builds Assembly-CSharp e Assembly-CSharp-Editor exit 0; run_strict_validation exit 0; execution
report com Spec Compliance Matrix + Testing Quality Gate + cenário final deferido; sem claim
ACCEPTED/PLAYMODE_VALIDATED (máximo BUILD_VALIDATED nesta fase gated); arquivos proibidos intactos;
spec PERMANECE em features_futuras/ (decisão 3.12 — execução gated).
```

## Anti-regressão

```text
- Não recriar CompanionFarmJobType/Definition/Assignment/JobBoardState/CompanionJobBoardService
  (existem — HARDEN/estender). Não criar segundo board/serviço/enum de resultado.
- Não reescrever backend de crop/animal/process nem de inventory/storage (ler via adapter).
- Não criar pet runtime/save/HUD/assets. Não criar romance/casamento profundo/spouse.
- Não adicionar Romance/Spouse ao enum CompanionRole (permanecem flags de elegibilidade).
- Output sempre de estado real; nunca criar item do nada; AutoSell permanece false; sem shipping
  por padrão; sem economia/loot infinito; caps independem de Bond/JobRank.
- Companion ajuda, não joga pelo jogador, e não é obrigatório para terminar o jogo; respeita active
  combat budget na fazenda (papéis de fazenda têm bônus de combate mínimo — ROLES_CATALOG §3.8).
- Sem GameObject.Find/FindObjectOfType em runtime (adapter injetado). Comunicação só por GameEventBus.
- Save com DTOs simples + IDs, sem refs Unity, sem migration acidental (se exigir migration, STOP).
- Não pedir human test por spec; cenário final em FINAL_HUMAN_VALIDATION_BY_WAVE.md.
- Não alterar SPEC_EXECUTION_ORDER.md, CURRENT_STATE.md, PROJECT_LOG nem índices compartilhados.
- Cross-ref vivo com as 3 siblings: eligibility/state/save (round-trip/provider compartilhado),
  cave assist/brain/balance (1 companion ativo na caverna; combat budget), e UI/HUD/invite/visit/
  dialogue (board visual). Não invadir o escopo delas.
```

## Stop Conditions (PARAR e registrar BLOCKED)

```text
1. Exigir alterar Packages/ ou ProjectSettings/.
2. Exigir scene/prefab/asset wiring fora do escopo (edição YAML por agente).
3. Exigir save migration sem spec de migration dedicada.
4. Criar pet runtime/save/HUD/assets.
5. Criar romance/casamento profundo ou spouse system, ou mover Romance/Spouse para CompanionRole.
6. Tornar companion obrigatório para terminar a main quest.
7. Permitir companion tankar boss, curar infinito, matar boss sozinho ou farmar sem jogador ativo.
8. Permitir companion gerar loot/recurso/economia infinita ou item do nada.
9. Adicionar Breath/Fôlego como recurso de companion.
10. Tratar classes de D&D como papéis mecânicos de companion.
11. Não conseguir decidir se um sistema existente é canônico ou obsoleto (CONFLICT).
```
