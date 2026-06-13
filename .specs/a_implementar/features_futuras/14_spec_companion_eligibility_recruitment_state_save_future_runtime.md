# SPEC — Companion: Eligibility, Recruitment, State Model, Equipment & Save Round-Trip (foundation, WAVE 14 gated)

> **Spec ID:** `14_spec_companion_eligibility_recruitment_state_save_future_runtime`
> **Status:** A implementar / SPEC-READY (contrato destravado pela EMENDA 2026-06-13-V3 e decisão 3.12; execução de runtime **gated na WAVE 14**)
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks
> **Priority:** P1
> **Type:** Runtime / Foundation / Companion State / Recruitment / Save
> **Domain:** Companion / Eligibility / Recruitment / Availability / SaveLoad
> **Parallelizable:** NO
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE
> **Can run with:** N/A (spec fundacional do grupo — todas as outras 14_* dependem do estado base que esta spec endurece)
> **Must not run with:** qualquer spec que altere city NPC roster final, romance/casamento profundo, pet runtime, save migration/schema, cave/farm job execution, combat assist da caverna ou UI visual final de companion. Em particular, NÃO rodar simultaneamente com `14_spec_companion_farm_jobs_board_automation_future_runtime`, `14_spec_companion_cave_assist_brain_balance_future_runtime` ou `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime` (todas leem/escrevem o estado base que esta spec é dona).
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/Save/Providers/**` (provider novo), `Assets/_Game/Scripts/Save/SaveData.cs` (DTO `CompanionManagerSaveData`/`CompanionSaveEntry` JÁ EXISTE — endurecer 2 campos de equipment como IDs, sem nova seção), `Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs` (registrar a seção `Companions` já presente no `GameSaveData`), `Assets/_Game/Scripts/Editor/Validation/**` (validator novo), `Assets/_Game/Tests/EditMode/Companions/**`
> **Depends on:**
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md` (com a seção "EMENDA 2026-06-13-V3 (Refinamento v3)")
> - `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` (artefato A7 — papéis, bônus, ações, reconciliação Romance/Spouse, modelo `CompanionRoleDefinition`)
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (BLOCO 3 — decisões vinculantes 3.1–3.12)
> - `docs/game_rules/save_rules.md` (DTOs simples, IDs estáveis, round-trip, ordem de restore)
> - `docs/game_rules/npc_rules.md` (companion = NPC com vínculo; eligibility é dado do NPC, nunca hardcode por nome)
> - `docs/game_rules/player_rules.md` (recursos HP/MP/Stamina; Breath/Fôlego não existe)
> - `docs/game_rules/event_rules.md` (comunicação de gameplay só via GameEventBus; unsubscribe obrigatório)
> - `Assets/_Game/Scripts/Save/ISaveSectionProvider.cs` (contrato do provider — precedente `HotbarSectionProvider`)
> - `Assets/_Game/Scripts/Save/SaveData.cs` (`CompanionManagerSaveData` + `CompanionSaveEntry` já embutidos no `GameSaveData.Companions`)
> **Blocks:**
> - `14_spec_companion_farm_jobs_board_automation_future_runtime` (consome estado base, JobRank, disponibilidade e equipment IDs)
> - `14_spec_companion_cave_assist_brain_balance_future_runtime` (consome InjuryState, CaveRank, 1-ativo-na-caverna, stances, equipment IDs)
> - `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime` (consome resultado de convite, elegibilidade, bond, equipment)
> - relationship/social future (WAVE 17 — lê flags de elegibilidade Romance/Spouse, sem virar papel)
> **Scope:** definir/endurecer elegibilidade, recrutamento, disponibilidade, estado base (bond/trust/fatigue/JobRank/CaveRank/InjuryState), **equipamento do companion (arma + acessório como IDs estáveis no save — decisão 3.4 OVERRIDE)**, reconciliação Romance/Spouse como **flags de elegibilidade** (NÃO no enum `CompanionRole`), e o **round-trip de save via `CompanionSaveSectionProvider`** (a DTO já existe; falta o provider — precedente `HotbarSectionProvider`).
> **Out of scope:** farm job execution, cave AI/combat assist, romance/casamento profundo, pet runtime, NPC roster authoring, save **migration**/bump de schema, UI visual final, balance numérico de DPS/cura (vive em `CompanionBalanceProfileSO`, decisão 3.10 — outra spec).

required_adrs: [ADR-0006-save-data-contracts-simple-dtos, ADR-0007-event-bus-gameplay-communication]
required_game_rules: [save_rules.md, npc_rules.md, player_rules.md, event_rules.md]

---

# EMENDA 2026-06-13-V3 (Refinamento v3) — VINCULANTE

> **Fonte vinculante:** `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, BLOCO 3.
> **Direction emendada:** `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md` → seção "EMENDA 2026-06-13-V3".
> **Catálogo de papéis (artefato A7):** `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` — fonte detalhada de roles, bônus, ações e da reconciliação Romance/Spouse.

Esta spec deixou de ser "future mapped" e passou a **SPEC-READY** (decisão 3.12): contrato destravado; execução de runtime **gated na WAVE 14**. Decisões v3 que esta spec é OBRIGADA a refletir no modelo de estado/save:

```text
3.4 (OVERRIDE — conflita com COMPANIONS_DIRECTION §45-46) — Companion equipa ARMA + ACESSÓRIO
    no SISTEMA BASE (2 slots). §45-46 da direction (equipment "futuro") foram SUPERSEDED.
    CompanionStateRecord/CompanionSaveEntry DEVEM persistir os 2 slots como IDs estáveis
    (string), NUNCA referência Unity (ver §10 + ADR-0006). Resolução do item ocorre no load
    via registries existentes (precedente: hotbar/equipment resolvem item por ID).

3.5 — Derrota do companion em combate base: Downed → janela de resgate → senão Retreat
    automático → InjuryState=Injured por 1-2 dias. SEM permadeath no combate base.

3.6 — Player derrotado com companion ativo: companion TENTA REVIVER o player (30% de chance);
    se conseguir, player volta; se falhar, companion escapa e volta Injured 1 dia.
    (A lógica de combate vive na 14_companion_cave_assist; ESTA spec só garante que
    InjuryState e InjuredUntilDay persistem o resultado.)

3.7 (OVERRIDE) — Permadeath SÓ em eventos narrativos roteirizados; a Fonte de Anya ressuscita
    com custo progressivo ("Ressurreição Dolorosa" — ver docs/game_rules/fonte_rules.md).
    Reconciliação com 3.5: combate base = sem permadeath (só Injured). ESTA spec NÃO implementa
    permadeath nem a ressurreição; apenas reserva InjuryState.Incapacitated como estado terminal
    de evento narrativo e cruza com fonte_rules.md no report.

3.8 — Fertilizante raro automático: toggle por job de Planter, default OFF. (Vive na
    14_companion_farm_jobs; ESTA spec só garante que o toggle, se persistido, é IDs/bool simples.)

3.10 — DPS alvo 25% comum / 40% especialista em CompanionBalanceProfileSO. (Balance numérico
    NÃO é desta spec; apenas garantir que PrimaryRole/SecondaryRole bastam para o balance derivar.)

3.11 — Bond 0-5; 1 perk passivo nos níveis 2 e 4; JobRank e CaveRank SEPARADOS do bond.
    O estado base DEVE manter BondLevel, JobRank e CaveRank como campos DISTINTOS. O código já
    tem CompanionBondState (BondLevel/JobRank/CaveRank/TrustPoints/Fatigue) e CompanionSaveEntry
    com esses campos — REUTILIZAR, NÃO duplicar.

3.12 — Manter WAVE 14; esta spec é o contrato base de estado/save dos companions (spec fundacional
    do grupo WAVE_14_COMPANIONS_FUTURE).
```

Reconciliações de código confirmadas pela re-auditoria 2026-06-13 — **ESTA SPEC É A DONA DESTAS DÍVIDAS**:

```text
ACRÉSCIMO #3 — CompanionManagerSaveData é só DTO, SEM SaveSectionProvider:
  Estado verificado em código:
    - GameSaveData.Companions existe (SaveData.cs:46) e aponta para
      CompanionManagerSaveData (SaveData.cs:224-228) → List<CompanionSaveEntry> (SaveData.cs:230-245);
    - CompanionSaveEntry já tem CompanionId, NpcId, UnlockState, UnlockedRoles[],
      UnlockedByQuestIds[], BondLevel, TrustPoints, Fatigue, InjuryState, LastInteractionDay,
      JobRank, CaveRank (SaveData.cs:233-244) — três campos de rank (Bond/Job/Cave) JÁ presentes;
    - Em Assets/_Game/Scripts/Save/Providers/ existe SOMENTE HotbarSectionProvider.cs;
      NÃO há provider de companion;
    - SaveManager NÃO captura nem restaura GameSaveData.Companions hoje (grep confirma:
      _hotbarProvider é o único provider wired — SaveManager.cs:72,95,140-142,1025-1027);
      logo o estado de companion NÃO faz round-trip.
  Ação desta spec: criar CompanionSaveSectionProvider : ISaveSectionProvider seguindo o
  precedente HotbarSectionProvider (Capture(GameSaveData)/Restore(object)), fechando o round-trip
  capture/restore de GameSaveData.Companions.
  ATENÇÃO: isto NÃO é "adicionar nova seção de save" (a DTO já existe no GameSaveData);
  é fechar o round-trip via provider + registrar a seção no SaveSectionOwnershipRegistry.
  Endurecer os 2 slots de equipment (decisão 3.4) é ACRÉSCIMO DE CAMPO na DTO existente, não
  troca de schema; se a integração com o pipeline de migração EXIGIR bump de CurrentSchemaVersion
  (hoje = 5, SaveManager.cs:39), vale a Stop Condition (STOP — save migration está FORA do escopo).

ACRÉSCIMO #4 — Romance/Spouse existem em CompanionEligibilityFlags mas NÃO no enum CompanionRole:
  Estado verificado em código:
    - CompanionEligibilityFlags tem CanBeRomanceCompanion/CanBeSpouseCompanion como bools
      (CompanionEligibilityFlags.cs:13-14);
    - o enum CompanionRole ([Flags]) tem só None/FarmCompanion/CaveCompanion/QuestCompanion/
      SocialCompanion (CompanionRole.cs:5-13).
  Reconciliação canônica (COMPANION_ROLES_CATALOG_v1.0 §"reconciliação", linhas ~625-651):
  Romance/Spouse permanecem ELEGIBILIDADES (flags), NÃO papéis mecânicos. NÃO adicionar
  Romance/Spouse ao enum CompanionRole. Combate/job derivam de PrimaryRole/SecondaryRole
  (papéis funcionais do catálogo A7); Romance/Spouse são gates social/elegibilidade.
  Isto preserva a regra de npc_rules.md: "nem todo romance vira companion de combate;
  nem todo spouse vira combat companion".
```

> **Nota de enums (decisão de execução requerida):** o enum `CompanionRole` real do código (`None/FarmCompanion/CaveCompanion/QuestCompanion/SocialCompanion` — quatro **categorias de companion**, marcadas `[Flags]`) **difere** dos **papéis funcionais** do catálogo A7 (`Fighter/Guardian/Healer/Alchemist/Scout/Researcher/MusicianSupport` + papéis de fazenda `FarmWorker/Forager/Miner/AnimalCaretaker/Crafter`). São EIXOS DIFERENTES: a categoria (`CompanionRole`) responde "que tipo de companion é", o papel funcional responde "que bônus/ações em combate/fazenda". O catálogo A7 (linhas ~620-651) resolve isso: manter `CompanionRole` como **categoria** e introduzir o papel funcional como **`RoleId` (string estável)** em `CompanionRoleDefinition`/`CompanionStateRecord.PrimaryRole`, NÃO criar um segundo enum paralelo de papel funcional sem justificativa. O executor deve auditar e registrar a decisão REUSE/HARDEN no execution report.

---

# /speckit.specify

## Contexto

Companion = **NPC da cidade com vínculo funcional** suficiente para acompanhar (1 ativo na caverna) ou ajudar (múltiplos em jobs de fazenda). Não é animal, não é summon, não é pet (pet é sistema separado e deferido — WAVE 23). A re-auditoria 2026-06-13 (`COMPANIONS_DIRECTION.md` §V3.9) confirmou que o **modelo de estado de companion já está parcialmente em código** (enum, flags, bond state, availability resolver, job board), mas com **duas dívidas críticas**: (1) o save de companion **não faz round-trip** porque a DTO `GameSaveData.Companions` não tem provider; (2) Romance/Spouse precisam ficar reconciliados como **flags de elegibilidade**, não papéis no enum. Soma-se a decisão 3.4 (OVERRIDE): companion agora **equipa arma + acessório no sistema base**, e esses 2 slots precisam persistir como IDs estáveis. Esta spec é a **fundação** do grupo WAVE_14_COMPANIONS_FUTURE: endurece o estado base, fecha o save round-trip e fixa as reconciliações, sem implementar jobs, combate de caverna, romance ou pet.

## Problema

Sem este contrato base endurecido:

```text
- companion recrutado/desbloqueado NÃO persiste (DTO existe, mas SaveManager não a captura/restaura);
- elegibilidade pode regredir a hardcode por nome de NPC (viola npc_rules.md);
- Romance/Spouse podem virar papel mecânico (NPC romance vira combat companion automaticamente);
- decisão 3.4 (arma + acessório) não tem onde persistir → equipment seria perdido no save ou,
  pior, serializaria referência Unity (viola ADR-0006 / save_rules.md);
- Bond/JobRank/CaveRank poderiam ser fundidos por engano (viola 3.11);
- InjuryState (downed→Injured 1-2 dias; 3.5/3.6) não teria onde gravar InjuredUntilDay;
- save pode serializar GameObject/Transform/SO de NPC;
- Breath/Fôlego (recurso que NÃO existe — player_rules.md) poderia reaparecer em companion;
- mais de 1 companion ativo na caverna (viola o canon "1 ativo na caverna").
```

## Objetivo

Ao final desta spec o repositório deve ter, **sem implementar jobs/combate/romance/pet**:

```text
1. O modelo de estado base de companion AUDITADO e ENDURECIDO reusando os tipos existentes
   (CompanionRole, UnlockState, InjuryState, CompanionEligibilityFlags, CompanionUnlockState,
   CompanionAvailabilityState, CompanionBondState, CompanionAvailabilityResolver), com a
   decisão REUSE/HARDEN registrada — sem criar tipos paralelos.
2. Os 2 slots de equipment (arma + acessório — decisão 3.4) representados como IDs estáveis
   (string) no estado de runtime e na DTO de save (acréscimo de campo, não troca de schema).
3. CompanionSaveSectionProvider : ISaveSectionProvider fechando o round-trip capture/restore
   de GameSaveData.Companions (precedente HotbarSectionProvider), com normalização defensiva
   (máx. 1 companion ativo na caverna; sem refs Unity; sem Breath).
4. A seção "Companions" registrada no SaveSectionOwnershipRegistry (owner, default, ordem).
5. Romance/Spouse reconciliados como FLAGS de elegibilidade (não no enum CompanionRole),
   com guardrail testado de que não auto-concedem papel de combate.
6. PrimaryRole/SecondaryRole expostos no estado base como RoleId (string do catálogo A7),
   sem segundo enum paralelo.
7. EditMode tests cobrindo eligibility, unlock, invite accept/reject, cap de 1 ativo na caverna,
   round-trip de save, ausência de Breath, ausência de ref Unity e o guardrail Romance/Spouse.
8. Editor validator de consistência do estado de companion.
9. Execution report com Spec Compliance Matrix + Testing Quality Gate.
```

## Fontes obrigatórias lidas

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md (+ EMENDA 2026-06-13-V3)
docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md (papéis, bônus, ações, reconciliação Romance/Spouse, modelo CompanionRoleDefinition §~560-651)
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (BLOCO 3 — decisões 3.1–3.12)
docs/game_rules/save_rules.md (DTOs simples; IDs estáveis; round-trip; ordem de restore; defaults explícitos)
docs/game_rules/npc_rules.md (companion = NPC com vínculo; eligibility é dado do NPC; romance≠combate)
docs/game_rules/player_rules.md (HP/MP/Stamina; Breath/Fôlego não existe)
docs/game_rules/event_rules.md (GameEventBus; unsubscribe)
docs/game_rules/fonte_rules.md (cross-ref — "Ressurreição Dolorosa": permadeath narrativo é da Fonte, não desta spec)
docs/decisions/ADR-0006-save-data-contracts-simple-dtos.md (save DTOs simples + IDs)
docs/decisions/ADR-0007-event-bus-gameplay-communication.md (eventos de gameplay via bus)
Assets/_Game/Scripts/Save/ISaveSectionProvider.cs (contrato do provider)
Assets/_Game/Scripts/Save/Providers/HotbarSectionProvider.cs (precedente do provider)
.claude/skills/save-section-provider/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/system-reuse-audit/SKILL.md
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
```

## Estado atual do repo

```text
EXISTE e DEVE SER REUSADO/ENDURECIDO (NÃO recriar — confirmado pela re-auditoria 2026-06-13):

Tipos de domínio (Assets/_Game/Scripts/Companions/):
- CompanionRole.cs:
    - enum CompanionRole [Flags] (linhas 5-13): None=0, FarmCompanion=1, CaveCompanion=2,
      QuestCompanion=4, SocialCompanion=8 → CATEGORIA de companion (não papel funcional);
    - enum UnlockState (linhas 15-25): Locked, Eligible, Temporary, Unlocked, Scheduled,
      StoryOnly, Unavailable, LockedByReputation;
    - enum InjuryState (linhas 27-33): Healthy, Injured, Incapacitated, Recovering.
- CompanionEligibilityFlags.cs (linhas 6-19): NpcId + CanBeFarm/Cave/Quest/Social/Romance/Spouse
  Companion (bools) + CompanionLockedByStory/Reputation/Quest + CompanionUnavailable.
  → Romance/Spouse JÁ existem como bools aqui (acréscimo #4).
- CompanionUnlockState.cs (linhas 6-16): CompanionId, NpcId, State(UnlockState),
  UnlockedRoles[](string), UnlockedByQuestIds[], UnlockedByReputationTier, UnlockedDay.
- CompanionBondState.cs (linhas 5-17): CompanionId, BondLevel, TrustPoints, Fatigue,
  InjuryState, UnlockedRoleFlags, JobRank, CaveRank, LastInteractionDay.
  → BondLevel + JobRank + CaveRank JÁ separados (decisão 3.11 ✔ — reusar, não duplicar).
- CompanionAvailabilityState.cs (linhas 6-17): CompanionId, AvailableNow, UnavailableReason,
  AllowedContexts[], CurrentScheduleBlock, InjuryState, StoryLocked, QuestConflict; +
  enum AvailabilityReason (linhas 19-32).
- CompanionAvailabilityResolver.cs (static, linhas 3-85): ResolveAvailability(...),
  CanInviteCompanion(...), IsRoleUnlocked/UnlockRole, SetInjury, CanRecruitByReputation,
  CanRecruitByQuest. → REUSAR como motor de availability/recruitment; endurecer se faltar regra.
- CompanionFarmJobType.cs (linhas 3-14): enum com 9 jobs (Planter..Miner) → NÃO desta spec
  (job execution é da 14_companion_farm_jobs); só não duplicar.
- CompanionFarmJobDefinition.cs / CompanionFarmJobAssignment.cs / JobBoardState.cs /
  CompanionJobBoardService.cs → da spec de jobs; NÃO tocar lógica de execução aqui.

Save (Assets/_Game/Scripts/Save/):
- SaveData.cs:46 — GameSaveData.Companions : CompanionManagerSaveData (campo JÁ existe no save raiz).
- SaveData.cs:224-228 — CompanionManagerSaveData { List<CompanionSaveEntry> Companions }.
- SaveData.cs:230-245 — CompanionSaveEntry { CompanionId, NpcId, UnlockState(int),
  UnlockedRoles[], UnlockedByQuestIds[], BondLevel, TrustPoints, Fatigue, InjuryState(int),
  LastInteractionDay, JobRank, CaveRank }. → DTO JÁ tem os 3 ranks; FALTAM os 2 slots de equipment.
- ISaveSectionProvider.cs (linhas 8-30): contrato Capture(GameSaveData)/Restore(object) + ProviderId.
- Providers/HotbarSectionProvider.cs (linhas 9-44): ÚNICO provider existente; precedente exato
  (delega a um state stateful; fallback para existingSaveData; null guard no Restore).
- SaveSectionOwnershipRegistry.cs (linhas 36-411): matriz de seções; "Companions" NÃO está listada.
- SaveManager.cs:39 — CurrentSchemaVersion = 5; SaveManager.cs:72,95,140-142,1025-1027 — só
  _hotbarProvider está wired (capture + restore). GameSaveData.Companions NÃO é capturada/restaurada.

NÃO EXISTE (escopo desta spec):
- CompanionSaveSectionProvider (provider de round-trip — criar seguindo HotbarSectionProvider);
- representação dos 2 slots de equipment (arma + acessório) no estado/save (decisão 3.4);
- registro da seção "Companions" no SaveSectionOwnershipRegistry;
- validator de consistência do estado de companion;
- EditMode tests do estado/save/eligibility.

AUDITAR na Fase 0 (registrar no report como EXISTING_CANONICAL / EXISTING_PARTIAL /
MISSING_SAFE_TO_CREATE / MISSING_BUT_DEFER / CONFLICT):
- onde mora o "manager" de companion em runtime (há um CompanionManager? ou só os tipos
  de dados acima?) — o provider precisa de um state stateful ao qual delegar (como HotbarState).
  Se não houver state stateful, decidir: criar um CompanionState mínimo (capture/restore in-memory)
  OU fazer o provider operar sobre a DTO de GameSaveData diretamente (fallback como o hotbar faz);
- se existe registry/resolver de item por ID para resolver arma/acessório no load (não criar
  pipeline de equipment novo aqui — só persistir o ID; resolução é da spec de cave/jobs);
- se o pipeline de save aceita acréscimo de campo na DTO SEM bump de CurrentSchemaVersion
  (JsonUtility tolera campos novos com default; se exigir migração → STOP).
```

## Engineering stories

```text
Como designer, quero declarar (dado do NPC) se um NPC pode ser farm/cave/quest/social companion,
  e se é elegível a romance/spouse — sem hardcode por nome.
Como jogador, quero desbloquear companion por relação/reputação/quest/evento/resgate, e que esse
  desbloqueio PERSISTA entre sessões.
Como jogador, quero que o companion equipe arma + acessório e que esse equipamento persista.
Como runtime de jobs/caverna, quero ler PrimaryRole/SecondaryRole, JobRank, CaveRank, InjuryState
  e disponibilidade SEM precisar recalcular do zero, a partir de um estado base confiável.
Como save/load, quero round-trip determinístico do estado de companion por IDs, com no máximo 1
  companion ativo na caverna e SEM nenhuma referência Unity nem Breath.
Como sistema social (futuro), quero ler CanBeRomanceCompanion/CanBeSpouseCompanion como GATES,
  sem que isso conceda papel de combate.
Como auditoria, quero um validator que falhe se o estado de companion violar os invariantes
  (cap de caverna, ausência de Breath, equipment como ID, enum não-poluído por Romance/Spouse).
```

## Escopo

```text
Inclui:
- Auditoria REUSE/HARDEN dos tipos existentes (enum/flags/state/resolver) + decisão no report.
- Endurecimento do estado base: PrimaryRole/SecondaryRole como RoleId(string do catálogo A7);
  garantir BondLevel/JobRank/CaveRank distintos; InjuredUntilDay para gravar a janela 3.5/3.6.
- Equipment do companion: 2 slots (arma + acessório) como IDs estáveis (string) no estado de
  runtime e na DTO de save (acréscimo de 2 campos string em CompanionSaveEntry; sem ref Unity).
- CompanionSaveSectionProvider : ISaveSectionProvider — Capture(GameSaveData)/Restore(object),
  ProviderId="companions", delegando a um state stateful (ou operando sobre a DTO, decidido na
  Fase 0), com fallback para existingSaveData e null guard no Restore (espelhar HotbarSectionProvider).
- Normalização defensiva no Restore/Capture: máx. 1 ActiveCaveCompanion; dedup de IDs;
  InjuryState/UnlockState fora de range → default seguro; nunca Breath.
- Registro da seção "Companions" em SaveSectionOwnershipRegistry (owner, default explícito,
  DependsOn, RiskLevel).
- Reconciliação Romance/Spouse: documentar + testar que são flags, não papéis; enum CompanionRole
  permanece None/FarmCompanion/CaveCompanion/QuestCompanion/SocialCompanion (NÃO adicionar valores).
- Editor validator (Assets/_Game/Scripts/Editor/Validation/) de consistência do estado.
- EditMode tests (ver Testing Quality Gate).
- Eventos de domínio CONDICIONAIS (CompanionUnlockedEvent / CompanionAvailabilityChangedEvent)
  SOMENTE se a auditoria mostrar consumidor real nesta wave; caso contrário, DEFER e documentar
  (não criar evento órfão; ADR-0007 — comunicação via bus, com unsubscribe).
```

## Fora de escopo

```text
- Farm job execution / job board automation (14_companion_farm_jobs).
- Cave companion AI / combat assist / stances em runtime / revive 30% (14_companion_cave_assist).
- UI de convite / HUD / job board visual / diálogo (14_companion_ui_hud_invite).
- Romance/casamento profundo, gifts/preferences (WAVE 17).
- Pet runtime / pet save / pet HUD (WAVE 23 — pasta Assets/_Game/Scripts/Pets/** PROIBIDA).
- Save MIGRATION ou bump de CurrentSchemaVersion (STOP se necessário).
- NPC roster authoring (quais NPCs concretos são elegíveis).
- Balance numérico (DPS 25/40%, cura, fertilizante) — vive em CompanionBalanceProfileSO / spec de jobs.
- Permadeath narrativo e "Ressurreição Dolorosa" (é da Fonte — fonte_rules.md; aqui só cross-ref).
- Pipeline de equipment do companion (equipar/desequipar/durabilidade) — aqui só PERSISTIR o ID.
```

## Regras de não duplicação

```text
- REUSAR CompanionRole/UnlockState/InjuryState/CompanionEligibilityFlags/CompanionUnlockState/
  CompanionBondState/CompanionAvailabilityState/CompanionAvailabilityResolver. Endurecer in-place;
  NÃO criar tipos paralelos (ex.: NÃO criar um segundo enum de "papel funcional"; usar RoleId string).
- REUSAR a DTO existente GameSaveData.Companions/CompanionManagerSaveData/CompanionSaveEntry —
  acrescentar campos, não substituir; NÃO criar uma segunda DTO de companion.
- Provider segue ISaveSectionProvider e o padrão HotbarSectionProvider — NÃO inventar contrato novo.
- NÃO criar canal de comunicação fora do GameEventBus (ADR-0007); NÃO adicionar Romance/Spouse ao enum.
- NÃO recriar CompanionFarmJobType nem a lógica do JobBoard (existem; pertencem a outra spec).
```

## Critérios de aceite

### CA-1 — Estado base auditado e endurecido (REUSE, não recriação)
- Os tipos existentes são reusados; a decisão REUSE/HARDEN está no report; `CompanionRole`
  permanece com 5 valores (None/Farm/Cave/Quest/Social); `BondLevel`, `JobRank`, `CaveRank` seguem
  campos distintos; `PrimaryRole`/`SecondaryRole` aparecem como `RoleId` (string do catálogo A7).
- Evidência: trecho do report (Existing systems audit) + EditMode test que assere os 5 valores do
  enum e a distinção dos 3 ranks; nenhum segundo enum de papel funcional criado.

### CA-2 — Equipment do companion como IDs estáveis (decisão 3.4)
- `CompanionSaveEntry` (e o estado de runtime) carregam `EquippedWeaponId` e `EquippedAccessoryId`
  como `string` (vazio = sem item); nenhuma referência Unity é persistida.
- Evidência: EditMode round-trip test capturando/restaurando os 2 IDs + assert de que os campos são
  `string` simples (reflexão/serialização) e que `null/""` é tratado como "sem equipamento".

### CA-3 — Round-trip de save via provider (fecha ACRÉSCIMO #3)
- `CompanionSaveSectionProvider` implementa `ISaveSectionProvider` (ProviderId="companions");
  `Capture` produz a DTO de companion; `Restore(null)` é no-op seguro; capture→restore→capture é
  idempotente; máx. 1 companion ativo na caverna após restore; IDs duplicados são deduplicados.
- Evidência: EditMode tests (idempotência, cap de caverna, dedup, null guard) + nota no report de
  que a seção foi registrada no SaveSectionOwnershipRegistry. (Se o wiring no SaveManager exigir
  bump de schema/migration → STOP documentado, status máximo CONTRACT_ONLY_NEEDS_INTEGRATION.)

### CA-4 — Availability/recruitment respeitam gates
- O resolver existente recusa convite quando: unavailable por story; injured (Incapacitated) /
  fatigue alta; quest/story lock; contexto não permitido; slot de caverna já cheio (1). Aceita
  quando todos os gates passam.
- Evidência: EditMode tests de accept/reject cobrindo cada motivo (`AvailabilityReason`).

### CA-5 — Romance/Spouse são flags, não papéis (fecha ACRÉSCIMO #4)
- `CanBeRomanceCompanion`/`CanBeSpouseCompanion` permanecem em `CompanionEligibilityFlags`; o enum
  `CompanionRole` NÃO ganha valores Romance/Spouse; um NPC marcado romance/spouse NÃO recebe papel
  de combate automaticamente (combate deriva de PrimaryRole/SecondaryRole).
- Evidência: EditMode guardrail test (flag romance/spouse setada ⇒ nenhum papel de combate concedido)
  + nota de reconciliação no report citando o catálogo A7.

### CA-6 — Save seguro (ADR-0006 / save_rules.md / player_rules.md)
- Nenhum `GameObject`/`Transform`/`ScriptableObject`/`Sprite`/`MonoBehaviour` é persistido; nenhum
  campo `Breath`/`Fôlego`/`Folego` existe no estado/DTO de companion; valores fora de range
  normalizam para default seguro.
- Evidência: EditMode test "no Unity ref / no Breath" (por reflexão sobre os tipos) + validator.

### CA-7 — Validator de consistência
- Um Editor validator (MenuItem) audita o estado de companion e contabiliza erros/avisos:
  cap de caverna >1, presença de campo Breath, equipment não-string, enum poluído por Romance/Spouse.
- Evidência: arquivo do validator + descrição do MenuItem no report (execução do validator é
  Unity Editor — pode ficar DEFERRED com residual risk se o Editor não rodar no sandbox).

# /speckit.plan

## Arquitetura alvo

```text
# REUSAR / ENDURECER (NÃO recriar):
Assets/_Game/Scripts/Companions/CompanionRole.cs                 (enum categoria + UnlockState + InjuryState — manter)
Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs     (flags — Romance/Spouse permanecem aqui)
Assets/_Game/Scripts/Companions/CompanionUnlockState.cs          (estado de unlock)
Assets/_Game/Scripts/Companions/CompanionBondState.cs            (bond/job/cave rank — manter distintos; +equipment?)
Assets/_Game/Scripts/Companions/CompanionAvailabilityState.cs    (availability + AvailabilityReason)
Assets/_Game/Scripts/Companions/CompanionAvailabilityResolver.cs (motor de availability/recruitment — endurecer)

# ENDURECER (acréscimo de campo, sem troca de schema):
Assets/_Game/Scripts/Save/SaveData.cs                            (CompanionSaveEntry += EquippedWeaponId, EquippedAccessoryId : string)

# NOVO:
Assets/_Game/Scripts/Companions/CompanionStateRecord.cs          (NOVO — record de estado de runtime: NpcId, CompanionId, UnlockState, Category(CompanionRole), PrimaryRole/SecondaryRole(string RoleId), gates, IsActiveFarm/Cave, availability, Hp/Mp/Stamina, Fatigue, InjuredUntilDay, EquippedWeaponId, EquippedAccessoryId, LastInvitedDay, LastJobDay, LastCaveRunId) — ESPELHA a DTO; sem refs Unity
Assets/_Game/Scripts/Companions/CompanionState.cs                (NOVO se Fase 0 confirmar ausência de state stateful — host in-memory ao qual o provider delega; Capture/Restore como HotbarState)
Assets/_Game/Scripts/Save/Providers/CompanionSaveSectionProvider.cs (NOVO — ISaveSectionProvider; ProviderId="companions")
Assets/_Game/Scripts/Editor/Validation/CompanionStateValidator.cs  (NOVO — MenuItem; erros/avisos)
Assets/_Game/Tests/EditMode/Companions/CompanionEligibilityStateSaveTests.cs (NOVO)

# REGISTRAR (linha nova na matriz existente, sem reescrever a matriz):
Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs        (entrada "Companions")

# REPORT:
docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md
```

> Decisão de execução (Fase 0): se já existir um `CompanionManager`/state stateful, o provider delega a ele (como `HotbarSectionProvider` delega a `HotbarState`). Se NÃO existir, escolher o menor caminho seguro: (a) criar `CompanionState` mínimo in-memory, OU (b) provider operando sobre a DTO via `existingSaveData` (fallback do hotbar). Registrar a escolha + justificativa no report.

## Contratos

### Data contracts (save — ADR-0006 / save_rules.md)
`CompanionSaveEntry` (DTO existente, SaveData.cs:230-245) — ENDURECER acrescentando:
```text
public string EquippedWeaponId;     // ID estável; "" = sem arma
public string EquippedAccessoryId;  // ID estável; "" = sem acessório
```
Todos os campos permanecem tipos simples + listas de string. **Proibido** qualquer ref Unity.
`CompanionManagerSaveData` (SaveData.cs:224-228) e `GameSaveData.Companions` (SaveData.cs:46) — reusar como estão. Sem `Version`/`Breath`. A normalização (cap 1 caverna, dedup) ocorre no provider, não na DTO.

### Runtime contracts
`CompanionAvailabilityResolver` (existente) permanece o motor de availability/recruitment; endurecer apenas se a Fase 0 achar regra faltante (ex.: cap de caverna não checado). `CompanionStateRecord` é o espelho in-memory dos campos da DTO (mais HP/MP/Stamina transientes), sem persistir HP/MP/Stamina se não houver decisão de design (registrar). PrimaryRole/SecondaryRole = `RoleId` string do catálogo A7 — nunca um segundo enum.

### Save contract (provider — precedente HotbarSectionProvider.cs)
```text
class CompanionSaveSectionProvider : ISaveSectionProvider
  ProviderId => "companions"
  object Capture(GameSaveData existingSaveData)   // retorna CompanionManagerSaveData; fallback existingSaveData?.Companions
  void Restore(object sectionData)                // null guard; cast para CompanionManagerSaveData; normaliza (cap 1 caverna, dedup, range, sem Breath)
```
Wiring no `SaveManager` (capturar/restaurar `GameSaveData.Companions`) só se NÃO exigir bump de schema; caso contrário **STOP** (Stop Condition de migration) e a spec entrega o provider como `CONTRACT_ONLY_NEEDS_INTEGRATION`.

### Event contracts (ADR-0007)
Sem evento novo por padrão. `CompanionUnlockedEvent`/`CompanionAvailabilityChangedEvent` SOMENTE se a Fase 0 achar consumidor real nesta wave; senão DEFER (não criar evento órfão). Se criados: publicar via `GameEventBus`, com unsubscribe em `OnDisable`/dispose.

### UI contract
N/A — esta spec NÃO altera UI (HUD/convite são da `14_companion_ui_hud_invite`).

## Sistemas afetados

```text
Companion state/domain (endurecimento), Save (provider novo + acréscimo de campo na DTO +
registro de seção). Jobs/Cave AI/UI/Romance/Pet: NÃO tocados (apenas consumirão este estado depois).
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**                       (endurecer existentes + CompanionStateRecord/CompanionState novos)
Assets/_Game/Scripts/Save/Providers/CompanionSaveSectionProvider.cs  (novo)
Assets/_Game/Scripts/Save/SaveData.cs                    (acréscimo de 2 campos string na DTO existente)
Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs (entrada "Companions")
Assets/_Game/Scripts/Save/SaveManager.cs                 (SOMENTE wiring do provider, se SEM bump de schema; senão STOP)
Assets/_Game/Scripts/Editor/Validation/CompanionStateValidator.cs (novo)
Assets/_Game/Tests/EditMode/Companions/**                (testes)
docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor) se necessário
```

## Arquivos proibidos

```text
Packages/** ; ProjectSettings/**
Assets/**/*.unity ; Assets/**/*.prefab ; Assets/**/*.asset (edição YAML manual)
Assets/_Game/Scripts/Pets/**                              (pet é sistema separado/deferido)
Assets/_Game/Scripts/Companions/CompanionFarmJob*.cs / JobBoardState.cs / CompanionJobBoardService.cs (job execution — outra spec; só não duplicar)
CurrentSchemaVersion / qualquer migration em Assets/_Game/Scripts/Save/Migrations/** (save migration FORA do escopo)
.specs/SPEC_EXECUTION_ORDER.md ; docs/project/CURRENT_STATE.md ; PROJECT_LOG.md
.specs/implementados/** ; docs/refinements/implementados/**
índices compartilhados (fable_00C, SPEC_REGISTRY, GAME_RULES_INDEX, DECISION_LOG, .specs)
HUD / UI de companion (14_companion_ui_hud_invite) ; cave AI/stances runtime (14_companion_cave_assist)
```

## Estratégia de implementação (por fases)

```md
### Fase 0 — Auditoria (system-reuse-audit) — OBRIGATÓRIA antes de tocar arquivo
- Rodar a busca local (ver §Validações) e classificar achados (EXISTING_CANONICAL / EXISTING_PARTIAL
  / MISSING_SAFE_TO_CREATE / MISSING_BUT_DEFER / CONFLICT).
- Confirmar: enum CompanionRole (5 valores), flags Romance/Spouse, bond/job/cave ranks, DTO Companions,
  ausência de provider de companion no SaveManager, HotbarSectionProvider como precedente.
- Decidir: existe state stateful (CompanionManager)? Se não, provider delega a CompanionState novo OU
  opera sobre a DTO (fallback do hotbar). Registrar a decisão.
- Confirmar que acréscimo de 2 campos string NÃO exige bump de CurrentSchemaVersion (JsonUtility tolera).
  Se exigir migração → STOP (Stop Condition de migration).
- Mapear como resolver arma/acessório por ID no load (registry de item existente) — só para documentar;
  resolução real é de spec posterior.

### Fase 1 — Endurecer modelo de estado
- Acrescentar EquippedWeaponId/EquippedAccessoryId (string) em CompanionSaveEntry (SaveData.cs).
- Criar CompanionStateRecord (espelho in-memory) com PrimaryRole/SecondaryRole como RoleId(string),
  Category(CompanionRole), gates, IsActiveFarm/Cave, InjuredUntilDay, os 2 EquippedIds, timestamps.
- Garantir distinção BondLevel/JobRank/CaveRank (já existe; não fundir).
- NÃO adicionar Romance/Spouse ao enum.

### Fase 2 — Provider de save (round-trip)
- Criar CompanionSaveSectionProvider : ISaveSectionProvider (ProviderId="companions"), espelhando
  HotbarSectionProvider (Capture com fallback existingSaveData; Restore com null guard + cast).
- Implementar normalização no Restore/Capture: cap 1 ActiveCaveCompanion; dedup de IDs;
  UnlockState/InjuryState fora de range → default; nunca Breath.
- Registrar a seção "Companions" no SaveSectionOwnershipRegistry (Owner="CompanionSaveSectionProvider",
  DefaultBehavior="Sem companions desbloqueados se ausente", DependsOn=["CurrentDay","Npcs"], RiskLevel="MEDIUM").
- Wiring no SaveManager SOMENTE se sem bump de schema; senão entregar provider como
  CONTRACT_ONLY_NEEDS_INTEGRATION e documentar.

### Fase 3 — Validator + Tests
- CompanionStateValidator (Editor, MenuItem) — erros/avisos para cap caverna, Breath, equipment não-ID,
  enum poluído.
- EditMode tests (ver Testing Quality Gate) cobrindo CA-1..CA-6.

### Fase 4 — Validação + Report
- csproj includes; run_strict_validation (exit 0); execution report com Spec Compliance Matrix,
  Existing systems audit, Testing Quality Gate, Dependency note (esta spec destrava as outras 3 da WAVE 14),
  cross-ref fonte_rules.md (permadeath narrativo não é desta spec).
```

## Paralelização

- Parallelizable: NO — estado base compartilhado por todo o grupo WAVE_14_COMPANIONS_FUTURE.
- Parallel group: WAVE_14_COMPANIONS_FUTURE (esta spec é a fundacional; executar PRIMEIRO).
- Shared files/systems que exigem lock: `Assets/_Game/Scripts/Companions/**`, `Save/Providers/**`,
  `Save/SaveData.cs`, `Save/SaveSectionOwnershipRegistry.cs`.
- Reason: jobs/cave/UI leem e escrevem o mesmo estado base e a mesma DTO de save; rodar em paralelo
  causaria conflito de schema/ownership.

## Impacto em save/load

```text
Does this change save schema? NO (acréscimo de 2 campos string com default; JsonUtility tolera —
  confirmar na Fase 0; se exigir bump/migration → STOP, save migration está fora do escopo).
Does this add a save section? NO (GameSaveData.Companions JÁ existe; esta spec fecha o round-trip
  via provider e registra a seção na ownership matrix).
Does this require migration? NO (a menos que a Fase 0 prove o contrário → STOP).
Does this persist Unity references? NO (equipment = IDs string; nenhum GameObject/Transform/SO).
Round-trip: SIM — capture→restore→capture idempotente; cap 1 caverna; dedup; sem Breath.
```

## Impacto em eventos

```text
Adds events: CONDITIONAL — CompanionUnlockedEvent/CompanionAvailabilityChangedEvent só se houver
  consumidor real nesta wave; caso contrário DEFER (sem evento órfão).
Changes existing events: NO.
Requires unsubscribe pattern: YES se algum subscriber for criado (ADR-0007 / event_rules.md).
```

## Impacto em UI/Unity

```text
Changes UI: NO (HUD/convite são da 14_companion_ui_hud_invite).
Changes scenes: NO. Changes prefabs: NO. Changes ScriptableObjects/assets: NO.
Requires Play Mode final validation: NO por padrão (lógica determinística testável em EditMode);
  YES DEFERRED apenas se a integração de invite/HUD for wired por outra spec.
Editor validator: roda no Unity Editor; pode ficar DEFERRED com residual risk se o Editor não rodar no sandbox.
```

## Riscos técnicos

```text
Risco: acréscimo de campo na DTO forçar bump de CurrentSchemaVersion / migration.
  Mitigação: Fase 0 confirma tolerância do JsonUtility a campos novos; se exigir migração → STOP,
  entregando o provider como CONTRACT_ONLY_NEEDS_INTEGRATION (migration é outra spec).

Risco: provider sem state stateful ao qual delegar (diferente do hotbar).
  Mitigação: Fase 0 decide CompanionState mínimo vs. operar sobre a DTO (fallback do hotbar);
  decisão registrada no report.

Risco: equipment serializar referência Unity (viola ADR-0006).
  Mitigação: campos string (ID); teste de reflexão "no Unity ref"; validator.

Risco: Romance/Spouse virar papel de combate (viola npc_rules.md).
  Mitigação: enum CompanionRole não recebe valores; guardrail test + reconciliação documentada (A7).

Risco: >1 companion ativo na caverna após restore.
  Mitigação: normalização no provider (cap 1) + teste dedicado.

Risco: Breath/Fôlego reaparecer (viola player_rules.md).
  Mitigação: teste de reflexão "no Breath" + validator; nenhum campo Breath em nenhum tipo.

Risco: duplicar tipos/DTO existentes (criar segundo enum/segunda DTO).
  Mitigação: Fase 0 system-reuse-audit; anti-regressão proíbe paralelos; report registra REUSE/HARDEN.

Risco: tocar lógica de job board / cave AI / UI (fora do escopo).
  Mitigação: arquivos proibidos; lock scope estrito; só o estado base + save.
```

## Rollback

```text
Remover CompanionSaveSectionProvider, o validator, os 2 campos de equipment e a entrada de
ownership desfaz a integração de save; os tipos existentes (enum/flags/state/resolver/DTO) e o
restante do save permanecem intactos. As 3 specs irmãs voltam a depender do estado base não-endurecido
(continuam gated). Nenhum save existente é corrompido (campos novos default = vazios; sem migration).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: system-reuse-audit (busca local + classificação); confirmar enum/flags/state/DTO,
       ausência de provider de companion, precedente HotbarSectionProvider; decidir state stateful vs.
       DTO-fallback; confirmar que acréscimo de campo NÃO exige bump de schema (senão STOP).
- [ ] T002 — Endurecer modelo: +EquippedWeaponId/EquippedAccessoryId (string) em CompanionSaveEntry;
       criar CompanionStateRecord (RoleId string p/ Primary/Secondary; ranks distintos; InjuredUntilDay;
       2 EquippedIds); NÃO poluir o enum com Romance/Spouse.
- [ ] T003 — CompanionSaveSectionProvider : ISaveSectionProvider (ProviderId="companions"),
       espelhando HotbarSectionProvider; normalização (cap 1 caverna, dedup, range, sem Breath);
       registrar seção "Companions" no SaveSectionOwnershipRegistry; wiring no SaveManager só se sem
       bump de schema (senão CONTRACT_ONLY_NEEDS_INTEGRATION + STOP de migration).
- [ ] T004 — CompanionStateValidator (Editor, MenuItem) com contadores de erro/aviso.
- [ ] T005 — EditMode tests (CA-1..CA-6): enum 5 valores + ranks distintos; round-trip equipment IDs;
       idempotência/cap caverna/dedup/null guard do provider; accept/reject por AvailabilityReason;
       guardrail Romance/Spouse; "no Unity ref / no Breath".
- [ ] T006 — csproj includes; run_strict_validation (exit 0); execution report (Spec Compliance Matrix,
       Existing systems audit, Testing Quality Gate, Dependency note destravando as 3 irmãs, cross-ref fonte_rules.md).
```

## Validações obrigatórias

```powershell
# Auditoria local (Fase 0) — PowerShell (Windows-only):
Select-String -Path .\Assets\_Game\Scripts\**\*.cs,.\docs\design\**\*.md,.\.specs\**\*.md -Pattern "CompanionEligibility|CompanionState|CompanionSave|CanBeFarmCompanion|CanBeCaveCompanion|CanBeRomanceCompanion|Spouse|Breath|Folego" -Recurse
Select-String -Path .\Assets\_Game\Scripts\**\*.cs -Pattern "ISaveSectionProvider|HotbarSectionProvider|GameSaveData.Companions|CompanionManagerSaveData|CompanionSaveEntry" -Recurse

# Docs + build + strict (exit code 0 obrigatório — Validation Truth):
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }

# Unity compile (quando .cs runtime mudar):
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"

# EditMode tests: Unity Test Runner (EditMode) ou /run-editmode-tests do repo.
```

## Testing Quality Gate

```text
Changed runtime code: YES (Companions domain + Save provider + DTO field)
Changed deterministic logic: YES (eligibility/availability/recruitment, save normalization/round-trip)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (EditMode — CA-1..CA-6)
Automated tests command: Unity Test Runner EditMode (ou /run-editmode-tests)
Manual Play Mode scenario: NOT REQUIRED por padrão (lógica determinística); DEFERRED_TO_FINAL_VALIDATION
  se invite/HUD for wired por spec irmã — registrar em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
Justification if no Play Mode: estado/save são puro C# determinístico, testável em EditMode.
Residual risk: (1) wiring do provider no SaveManager pode exigir migration → STOP, deixando
  CONTRACT_ONLY_NEEDS_INTEGRATION; (2) Editor validator pode ficar DEFERRED se o Editor não rodar no sandbox.
```

## Definition of Done

```text
- Tipos existentes REUSADOS/ENDURECIDOS (sem paralelos); enum CompanionRole intacto (5 valores);
  Bond/Job/Cave rank distintos; Romance/Spouse só como flags (não no enum).
- Equipment = EquippedWeaponId/EquippedAccessoryId (string) na DTO e no estado; sem ref Unity.
- CompanionSaveSectionProvider implementado (round-trip idempotente; cap 1 caverna; dedup; sem Breath);
  seção "Companions" registrada na ownership matrix; wiring no SaveManager OU CONTRACT_ONLY_NEEDS_INTEGRATION
  documentado se migration for exigida.
- EditMode tests CA-1..CA-6 presentes; validator presente.
- run_strict_validation exit 0; Assembly-CSharp e Assembly-CSharp-Editor 0E; docs validation PASS.
- Execution report em docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md
  com Spec Compliance Matrix + Existing systems audit + Testing Quality Gate + Dependency note + cross-ref fonte_rules.md.
- Sem promoção a ACCEPTED só por compile; sem alterar arquivos proibidos.
```

## Anti-regressão

```text
- NÃO adicionar Romance/Spouse ao enum CompanionRole (permanecem flags em CompanionEligibilityFlags).
- NÃO criar segundo enum de papel funcional (usar RoleId string do catálogo A7); NÃO criar segunda DTO de companion.
- NÃO persistir GameObject/Transform/ScriptableObject/Sprite/MonoBehaviour (ADR-0006).
- NÃO adicionar Breath/Fôlego como recurso de companion (player_rules.md).
- NÃO permitir >1 companion ativo na caverna após restore.
- NÃO implementar pet runtime / pet save (Assets/_Game/Scripts/Pets/** proibido).
- NÃO implementar romance/casamento profundo nem permadeath/Ressurreição (Fonte — fonte_rules.md).
- NÃO bumpar CurrentSchemaVersion nem criar migration (STOP se exigido).
- NÃO tocar job board / cave AI / UI de companion (specs irmãs).
- NÃO tornar companion obrigatório para terminar o jogo; companion ajuda, não joga pelo jogador.
- NÃO alterar SPEC_EXECUTION_ORDER.md, CURRENT_STATE.md, índices compartilhados.
```

## Stop Conditions

```text
STOP e registrar BLOCKED se:
1. Acréscimo de campo na DTO exigir bump de CurrentSchemaVersion ou nova migration.
2. Wiring exigir scene/prefab/asset YAML manual.
3. Não houver como decidir REUSE vs. recriação de um tipo existente (conflito não resolvível).
4. A integração exigir criar pet runtime/save/HUD/data assets.
5. A integração exigir romance/casamento profundo, spouse system ou permadeath/Ressurreição.
6. A spec exigir alterar Packages/ ou ProjectSettings/.
7. Companion virar obrigatório, tankar boss, curar/farmar infinito ou jogar pelo jogador.
```

## Notas para execução posterior

```text
Esta spec é a FUNDAÇÃO do grupo WAVE_14_COMPANIONS_FUTURE (decisão 3.12): execução gated na WAVE 14.
Ao destravar, executar ANTES das irmãs:
- 14_spec_companion_farm_jobs_board_automation_future_runtime (consome JobRank, disponibilidade, equipment IDs);
- 14_spec_companion_cave_assist_brain_balance_future_runtime (consome InjuryState/InjuredUntilDay, CaveRank, 1-ativo-caverna, equipment IDs, stances/DPS via CompanionBalanceProfileSO);
- 14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime (consome resultado de convite, bond, equipment).
Permadeath narrativo + "Ressurreição Dolorosa" são da Fonte (docs/game_rules/fonte_rules.md) — não desta spec.
Romance/Spouse aprofundados são WAVE 17 (leem as flags daqui, sem virar papel).
```
