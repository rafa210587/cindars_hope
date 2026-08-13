# SPEC — Companion UI: HUD compacto, convite/visita, hooks de diálogo e UI de comandos/stances

> **Spec ID:** `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime`
> **Status:** A implementar / SPEC-READY (contrato destravado; execução de runtime gated na WAVE 14 — EMENDA 2026-06-13-V3 e decisão 3.12)
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks
> **Priority:** P2
> **Type:** Runtime / Future / UI Projection / Dialogue Hooks / Farm Visits
> **Domain:** Companion / UI / HUD / Invite / Visit / Dialogue / Comandos+Stances / Knowledge Hints
> **Parallelizable:** YES
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE (única do grupo marcada Parallelizable: YES — decisão 3.12)
> **Can run with:** specs docs-only de registry/research que NÃO toquem os mesmos arquivos; nenhuma das outras 3 specs companion que escrevam em `Companions/**`.
> **Must not run with:**
> - `14_spec_companion_cave_assist_brain_balance_future_runtime` (Parallelizable: NO — escreve `Companions/**`, `Cave/**`, `Combat/**`; dona da lógica de stance/comando/downed/revive que esta spec apenas PROJETA);
> - `14_spec_companion_eligibility_recruitment_state_save_future_runtime` (dona do save provider e do estado de elegibilidade/bond que esta spec LÊ);
> - `14_spec_companion_farm_jobs_board_automation_future_runtime` (dona do JobBoard/automação que esta spec projeta);
> - qualquer spec que altere UI prefab/layout final, dialogue writing, romance/casamento, pet HUD, relationship full runtime, save migration ou quest content.
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Companions/**` (NOVO), `Assets/_Game/Scripts/Companions/Dialogue/**` (NOVO), `Assets/_Game/Scripts/Companions/Knowledge/**` (NOVO), `Assets/_Game/Scripts/Editor/Validation/CompanionUIValidator.cs` (NOVO), `Assets/_Game/Tests/EditMode/UI/Companions/**` (NOVO), `docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md`
> **Depends on:**
> - `14_spec_companion_eligibility_recruitment_state_save_future_runtime` — PROVÊ o estado real (CompanionEligibilityFlags, CompanionUnlockState, CompanionBondState, AvailabilityReason, save provider). Esta spec é PROJECTION-ONLY: lê o estado, não o muta nem o persiste.
> - `14_spec_companion_cave_assist_brain_balance_future_runtime` — PROVÊ os eventos/estado de combate (comando ativo, stance ativa, Downed→resgate→Retreat→Injured, revive 30%). Esta spec apenas EXIBE esse estado read-only.
> - `14_spec_companion_farm_jobs_board_automation_future_runtime` — PROVÊ JobBoardState/atribuição de jobs (CompanionJobBoardService já existe). Esta spec projeta a elegibilidade de atribuição.
> - GameEventBus (existente — ADR-0007) e o `ui-projection-pattern` (20+ ViewModels, ~96 EditMode tests).
> **Blocks:**
> - companion invite UI (prefab/layout — fora desta spec, consome estas projections);
> - job board UI (consome CompanionInviteViewModel.CanAssignJob);
> - cave loadout UI (consome CompanionHudProjection + CompanionInviteViewModel.CanInviteCave);
> - relationship/social UI future (consome a exibição read-only de flag romance/spouse);
> - bestiary hints from companion (consome CompanionKnowledgeHintProjection).
> **Scope:** definir/endurecer as PROJECTIONS e hooks read-only de UI/diálogo/visita do companion: HUD compacto (bond/role/vitals/status), indicador de COMANDO+STANCE ativos (3.2/3.3), feedback de Downed/Retreat/Injured e da tentativa de revive (3.5/3.6 — só exibição), exibição compacta read-only dos 2 slots de equipment (3.4), fluxo de convite/visita com razão explícita de indisponibilidade, hooks de diálogo gated por elegibilidade/spoiler, e hint projection de knowledge gated. Tudo via projection pura (sem mutar estado) e GameEventBus.
> **Out of scope:** prefab/layout final, dialogue text, romance/spouse runtime, pet HUD, relationship full system, quest content, persistência (save é da spec de eligibility/state/save), a LÓGICA de stance/comando/downed/revive (é da spec de cave assist).

required_adrs: [ADR-0007]
required_game_rules: [ui_modal_rules.md, ui_rules.md, event_rules.md]

---

# EMENDA 2026-06-13-V3 (Refinamento v3)

> **Fonte vinculante:** `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, BLOCO 3 (decisões 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.11, 3.12 + acréscimos #3 e #4 da re-auditoria de código de 2026-06-13).
> **Direction emendada:** `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md` → seção "EMENDA 2026-06-13-V3 (Refinamento v3)", subseções V3.1 (comandos/stances), V3.3 (equipment base), V3.4/V3.5/V3.6 (downed/revive/permadeath), V3.8 (bond), V3.9 (reconciliações de código), V3.10 (status WAVE 14).
> **Catálogo de papéis (artefato A7):** `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` — fonte do RoleIcon/role label (14 papéis funcionais), da stance default por papel e do flag "expõe Suporter".

Esta spec deixa de ser "future mapped" e passa a **SPEC-READY** (decisão 3.12): contrato destravado e validável; execução de runtime gated na **WAVE 14**. Decisões v3 que esta spec reflete na CAMADA DE PROJECTION read-only (sem mutar estado):

```text
3.2 / 3.3 — A UI deve EXPOR (read-only):
            • o COMANDO ativo: Ficar/Esperar | Seguir | Atacar alvo marcado (3 comandos diretos);
            • a STANCE ativa: Agressivo (raio ≤12 tiles) | Defensivo (DEFAULT, ≤4 tiles ou quem ataca) | Passivo;
            • o modo SUPORTER apenas para papéis de suporte (Healer/Alchemist/MusicianSupport — flag do catálogo A7),
              NÃO como 4ª stance universal.
            A TROCA em si é COMANDO de runtime (vive na spec de cave assist). A UI de input proposta é
            "1 tecla: tap = Seguir/Esperar (toggle); hold = menu radial com stance + recuar" (decisão 3.2) —
            esta spec MODELA o estado read-only que o radial exibe; o binding da tecla e a mutação ficam
            na spec de cave assist / input map F67.

3.4 (OVERRIDE) — Companion equipa ARMA + ACESSÓRIO (2 slots) no sistema base. A UI exibe os 2 slots de forma
            COMPACTA e READ-ONLY (apenas IDs/labels resolvidos por ID). NÃO criar inventário completo de
            companion na UI; sem drag/drop; sem equipar pela HUD nesta spec. §45-46 da direction estão SUPERSEDED.

3.5 / 3.6 — Feedback de derrota: Downed → janela de resgate → Retreat automático → Injured 1-2 dias (estado/aviso
            na HUD). Quando o PLAYER é derrotado com companion ativo, a UI reflete a TENTATIVA de revive (30%) e o
            RESULTADO (player volta OU companion escapa Injured). Apenas feedback/estado; a lógica (chance, escape,
            duração de Injured) vive na spec de cave assist / state. Sem permadeath na UI de combate base (3.7).

3.11 — Bond 0-5 com perks em 2/4: a UI exibe o nível de bond e os marcos de perk de forma DISCRETA, sem competir
            com o HUD principal (HP/MP/Stamina). JobRank/CaveRank são trilhas separadas do bond (campos já no código).
            Manter HUD compacto; SEM Breath/Fôlego.

3.12 — Manter WAVE 14; execução gated. Esta spec é a única do grupo marcada Parallelizable: YES.
```

Notas de reconciliação de código (re-auditoria 2026-06-13, ver direction V3.9):

```text
- Acréscimo #4 (V3.9.2): a UI NÃO trata Romance/Spouse como papel de combate. Romance/Spouse são FLAGS de
  elegibilidade (CompanionEligibilityFlags.CanBeRomanceCompanion/CanBeSpouseCompanion); o RoleIcon/role label
  vem do PAPEL FUNCIONAL (PrimaryRole/SecondaryRole do catálogo A7), NÃO do enum [Flags] CompanionRole de
  contexto. Exibir status de romance/spouse, se houver, é apenas exibição de flag — não habilita combate/job.
- Acréscimo #3 (V3.9.1): a persistência de companion (CompanionManagerSaveData → CompanionSaveSectionProvider,
  precedente HotbarSectionProvider) é responsabilidade da spec de eligibility/state/save. Esta spec NÃO persiste.
- Esta spec é PROJECTION-ONLY: lê estado de companion, NÃO muta, NÃO persiste, NÃO publica eventos de gameplay
  (apenas pode publicar um evento de "UI rebuild requested" se necessário — ver Impacto em eventos).
```

---

# /speckit.specify

## Contexto

Companion continua sendo **NPC da cidade com vínculo** (não animal, não summon): tem casa/cama/rotina, pode recusar convite, visitar a fazenda se relação/reputação permitir e participar de quests/diálogos. **1 companion ativo na CAVERNA; múltiplos em jobs de FAZENDA.** O estado já existe parcialmente no código (WAVE 05): `CompanionEligibilityFlags`, `CompanionUnlockState`, `CompanionBondState`, `CompanionAvailabilityResolver`, `CompanionFarmJobType` (9 jobs), `CompanionJobBoardService`, `CompanionManagerSaveData`.

O que falta é a **camada de apresentação read-only**: como o jogador vê (a) se um companion está disponível e POR QUÊ não, (b) o HUD compacto de vitals/role/bond/status sem competir com o HUD do jogador, (c) o COMANDO e a STANCE ativos (decisão v3 3.2/3.3), (d) o feedback de Downed/Retreat/Injured e da tentativa de revive (3.5/3.6), (e) a exibição compacta dos 2 slots de equipment (3.4), (f) o fluxo de convite/visita sem teleport injustificado, (g) os hooks de diálogo gated por elegibilidade/spoiler, e (h) hints de knowledge gated.

Esta spec entrega essa camada como **projections puras** (MVVM-lite do projeto: `ui-projection-pattern`, 20+ ViewModels, ~96 EditMode tests) e **hooks de diálogo**, sem prefab/layout final, sem dialogue text, sem romance/pet, sem mutar estado e sem persistir.

## Problema

Sem projection/hooks read-only:

```text
o jogador não sabe POR QUE um companion recusou convite (parece bug);
o HUD de companion compete com HP/MP/Stamina do jogador (poluição);
o companion social pode virar romance/casamento automaticamente (escopo proibido);
o pet HUD pode entrar junto (sistema separado, proibido aqui);
o visitante teleporta sem justificativa (quebra de imersão);
um diálogo de companion revela spoiler de quest principal;
um research hint completa o bestiário sozinho (deve apenas dar pista);
o estado de comando/stance ativos não aparece (decisão 3.2/3.3 fica invisível ao jogador);
o feedback de Downed/Retreat/Injured/revive 30% não tem contrato de exibição (3.5/3.6);
os 2 slots de equipment (3.4) não têm como ser exibidos read-only;
job/cave state não aparece na UI de convite/loadout.
```

Além disso, o HUD principal já reserva os campos placeholder `CompanionProjection`/`PetProjection` (`GameplayHudViewModel.cs:48-49`) "no runtime until companion/pet specs" — esta spec define a projection que preenche o slot de companion (e mantém o de pet vazio).

## Objetivo

Ao final desta spec, a CAMADA DE DADOS DA UI deve existir como projections puras e hooks, prontos para uma View MonoBehaviour (prefab/layout) consumir numa fase posterior:

```text
1. CompanionInviteViewModel — por companion: identidade, papéis (PrimaryRole/SecondaryRole funcionais),
   estado de unlock, disponibilidade + AvailabilityReasons[] EXPLÍCITAS, e os gates CanInvite{Farm,Cave,Quest}/
   CanAssignJob/CanVisitFarm, resumo de relação, story-lock e aviso de risco.
2. CompanionAvailabilityReason — enum read-only canônico de razões (reusa/estende o AvailabilityReason existente),
   garantindo que "indisponível" SEMPRE carrega uma razão legível (regra do ui-projection-pattern: sem disable silencioso).
3. CompanionHudProjection — HUD compacto: vitals (HP/MP opcional/Stamina), RoleIcon (papel funcional A7), bond 0-5 +
   marcos de perk (2/4), status (InjuryState etc.), modo atual (FarmJob|CaveAssist|Visiting|Quest|Unavailable),
   COMANDO ativo, STANCE ativa, indicador Suporter (só papéis de suporte), feedback de Downed/Retreat/revive,
   2 slots de equipment read-only, cooldown indicators e warnings. SEM Breath/Fôlego. NÃO compete com HP/MP/Stamina do jogador.
4. CompanionCommandStanceProjection — modela o estado read-only do radial de comandos/stances (1 tecla tap/hold):
   comando vigente, stance vigente, conjunto de stances disponíveis para o papel (3 universais + Suporter condicional),
   e a ação "recuar". A MUTAÇÃO é da spec de cave assist; aqui é só o que o radial exibe.
5. CompanionVisitProjection — visita à fazenda: tipo, área-alvo, horário de chegada/saída derivado da agenda,
   razão, se é interagível, dialogue set opcional e a JUSTIFICATIVA de não-teleport.
6. CompanionDialogueHook — hooks de diálogo gated: tipo (InviteFarm|InviteCave|AssignJob|QuestTemporary|AskForHint|
   Dismiss|CheckStatus|SetCommand|SetStance), disponibilidade requerida, command id, se requer confirmação e spoiler tier.
7. CompanionKnowledgeHintProjection — hint de knowledge gated por expertise/confiança/spoiler; NÃO resolve bestiário.
8. CompanionUIValidator (Editor) — valida no Editor: nenhuma projection com Breath/Fôlego; nenhum pet HUD; nenhum
   romance/spouse como papel funcional; toda razão de indisponibilidade tem string; HUD compacto não duplica HP/MP/Stamina.
```

Tudo PROJECTION-ONLY: lê estado, deriva apresentação, NÃO muta nem persiste, e Views (futuras) reconstroem a partir de eventos (nunca polling).

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (BLOCO 3 — 3.2/3.3 comandos+stances+Suporter; 3.4 equipment base;
  3.5/3.6 downed+revive; 3.7 permadeath narrativo; 3.11 bond; 3.12 WAVE 14 gated; "Ambiguidades interpretadas" #1)
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md (PARTE J estados de IA; PARTE K combate/budget;
  PARTE M §36-38 downed/Injured; PARTE N §39-41 bond; EMENDA 2026-06-13-V3 V3.1/V3.3/V3.4/V3.5/V3.6/V3.8/V3.9/V3.10)
docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md (14 papéis; stance default por papel; flag Suporter;
  §6 tabela stances/Suporter; PARTE D reconciliação contexto vs. papel funcional)
docs/game_rules/ui_rules.md (Rule 2 projection/ViewModel; Rule 7 HUD visibility vs. suppression; HUD overlay spec;
  campos placeholder CompanionProjection/PetProjection)
docs/game_rules/ui_modal_rules.md (modal stack/input blocking/Esc — para o radial/painel de comandos, se modal)
docs/game_rules/event_rules.md (DTO de evento, Publish/Subscribe, unsubscribe on destroy; ADR-0007)
docs/decisions/ADR-0007-event-bus-gameplay-communication.md (comunicação via GameEventBus)
.claude/skills/ui-projection-pattern/SKILL.md (convenção: VM pura, enum-de-estado, reason string, View rebuild-from-events)
.claude/skills/ui-modal-stack/SKILL.md (se o radial/painel de comandos for modal)
.claude/rules/testing-quality-gate.md (UI: EditMode para projeções; Play Mode/cenário humano para Views)
.claude/rules/unity-architecture.md (sem GameObject.Find/FindObjectOfType; GameEventBus; save DTOs simples)
Código (REUSAR, não recriar):
  Assets/_Game/Scripts/Companions/CompanionRole.cs (enum [Flags] CompanionRole = CONTEXTO; enum UnlockState; enum InjuryState)
  Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs (flags Farm/Cave/Quest/Social/Romance/Spouse + locks)
  Assets/_Game/Scripts/Companions/CompanionUnlockState.cs (State, UnlockedRoles, UnlockedByQuestIds, ReputationTier)
  Assets/_Game/Scripts/Companions/CompanionBondState.cs (BondLevel, TrustPoints, Fatigue, InjuryState, JobRank, CaveRank)
  Assets/_Game/Scripts/Companions/CompanionAvailabilityState.cs (enum AvailabilityReason; CompanionAvailabilityState)
  Assets/_Game/Scripts/Companions/CompanionAvailabilityResolver.cs (ResolveAvailability/CanInviteCompanion)
  Assets/_Game/Scripts/Companions/CompanionFarmJobType.cs (9 jobs)
  Assets/_Game/Scripts/Companions/CompanionJobBoardService.cs (JobBoardState)
  Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs (precedente de projection; campos placeholder companion/pet)
  Assets/_Game/Scripts/Dialogue/** (DialogueResolver, DialogueChoice, DialogueCondition, DialogueSetDefinition)
```

## Estado atual do repo

```text
EXISTE e NÃO RECRIAR (verificado 2026-06-13):
- Estado de companion (WAVE 05): CompanionEligibilityFlags, CompanionUnlockState, CompanionBondState,
  CompanionAvailabilityState (+ enum AvailabilityReason), CompanionAvailabilityResolver, CompanionFarmJobType (9),
  CompanionFarmJobDefinition/Assignment, JobBoardState, CompanionJobBoardService — em Assets/_Game/Scripts/Companions/.
  Esta spec LÊ esse estado para preencher projections; NÃO o muta nem o duplica.
- Enum [Flags] CompanionRole = CONTEXTO (None/FarmCompanion/CaveCompanion/QuestCompanion/SocialCompanion), NÃO papel
  funcional (catálogo A7, PARTE D). RoleIcon/role label da HUD vem do PAPEL FUNCIONAL (PrimaryRole/SecondaryRole),
  que ainda NÃO é um enum/identificador no código — modelar como string/identificador estável de papel funcional
  alimentado pelo catálogo A7 (sem recriar o enum de contexto, sem adicionar papel funcional ao enum de contexto).
- Romance/Spouse = bools em CompanionEligibilityFlags (CanBeRomanceCompanion/CanBeSpouseCompanion). NÃO há entrada
  Romance/Spouse no enum CompanionRole. A UI exibe romance/spouse SÓ como flag (acréscimo #4 / V3.9.2).
- HUD principal: GameplayHudViewModel (alias HUDGameplayViewModel) com Rule 2 do ui_rules.md; já tem os campos
  placeholder string CompanionProjection/PetProjection (GameplayHudViewModel.cs:48-49) "no runtime until companion/pet
  specs". A projection de companion desta spec é a fonte que preenche o slot companion (pet permanece vazio).
- HudVisibilityController + HudVisibilityChangedEvent (HudEvents.cs): HUD some quando há modal. CompanionHud é uma
  overlay (não-modal) sujeita à MESMA visibility do HUD do jogador — não criar canal de visibility paralelo.
- Floating damage numbers / hit-flash / knockback: existem e estão wired (ui_rules.md Rule 6). Irrelevante aqui;
  não tocar.
- Dialogue: DialogueResolver/DialogueChoice/DialogueCondition/DialogueSetDefinition em Assets/_Game/Scripts/Dialogue/.
  Hooks de companion devem ENGANCHAR nesse sistema (condições/choices), não recriar runtime de diálogo.
- Save provider: apenas HotbarSectionProvider existe (Assets/_Game/Scripts/Save/Providers/). O provider de companion
  é da spec de eligibility/state/save (V3.9.1). Esta spec NÃO cria provider e NÃO persiste.

NÃO EXISTE (escopo desta spec — criar como projection/hook puro):
- Assets/_Game/Scripts/UI/Companions/** (pasta inexistente hoje — Glob 2026-06-13 retornou vazio);
- CompanionInviteViewModel, CompanionHudProjection, CompanionCommandStanceProjection, CompanionVisitProjection;
- CompanionDialogueHook, CompanionKnowledgeHintProjection;
- CompanionUIValidator (Editor).

NÃO EXISTE AINDA (fora desta spec — esta spec só MODELA o estado read-only que exibirá; lógica nas outras 3):
- runtime de comando/stance/downed/retreat/revive (spec de cave assist);
- runtime de save provider e mutação de eligibility/bond (spec de eligibility/state/save);
- runtime de atribuição/automação de jobs (spec de farm jobs board).

AUDITAR Fase 0:
- de qual evento (ou serviço read-only) a View reconstrói cada projection (combate publica comando/stance/downed?
  job board publica atribuição? eligibility publica disponibilidade?). Se a outra spec ainda não publicou o evento,
  MODELAR a projection + documentar o gancho de evento esperado (DEFERRED_INTEGRATION), nunca inventar mutação;
- onde colocar os hooks de diálogo (enganchar no DialogueResolver/DialogueCondition existente vs. struct própria);
- confirmar o identificador de PAPEL FUNCIONAL (string estável do catálogo A7) a usar no RoleIcon, sem mexer no enum
  de contexto;
- confirmar se o radial de comandos/stances é modal (governado por ui_modal_rules) ou overlay HUD (governado por
  ui_rules) — proposta: overlay HUD radial efêmero (não bloqueia movimento), mas o painel "status do companion"
  completo, se existir, é modal.
```

## Engineering stories

```text
Como jogador, quero saber se um companion está disponível e, se não, POR QUÊ (horário, trabalho, sono, ferido,
  exausto, story lock, relação baixa, já há um ativo) — nunca um botão cinza sem motivo.
Como jogador, quero um HUD compacto do companion (vitals/role/bond/status) que NÃO compita com meu HP/MP/Stamina,
  sem Breath/Fôlego.
Como jogador, quero ver o COMANDO ativo (Ficar/Seguir/Atacar marcado) e a STANCE ativa (Agressivo/Defensivo/Passivo),
  e — só para papéis de suporte — o modo Suporter, num radial de 1 tecla (tap = seguir/esperar; hold = radial).
Como jogador derrotado com companion ativo, quero ver a tentativa de revive (30%) e o resultado (volto OU o companion
  escapa Injured), sem que a UI sugira morte permanente em combate base.
Como jogador, quero ver os 2 slots de equipment do companion (arma + acessório) de forma compacta e read-only.
Como diálogo, quero oferecer opções de convite/job/cave/quest/hint SOMENTE quando válidas e sem revelar spoiler.
Como fazenda, quero mostrar a visita do companion com chegada/saída coerentes com a agenda, sem teleport injustificado.
Como knowledge/researcher, quero dar hints leves e gated — nunca resolver o bestiário sozinho.
Como projection, quero NÃO mutar estado nem persistir: leio o estado real e derivo apresentação; Views reconstroem
  de eventos, nunca por polling.
```

## Escopo

```text
Inclui (tudo PROJECTION-ONLY / hook read-only, pasta UI/Companions + Companions/Dialogue + Companions/Knowledge):

- CompanionInviteViewModel (UI/Companions/, NOVO): pura C#; identidade (CompanionId, NpcId, DisplayName),
  PrimaryRole/SecondaryRole (papel funcional do catálogo A7, identificador estável — NÃO o enum de contexto),
  UnlockState (reusa enum existente), Availability + AvailabilityReasons[] (reusa/estende AvailabilityReason),
  gates booleanos derivados CanInviteFarm/CanInviteCave/CanInviteQuest/CanAssignJob/CanVisitFarm,
  RelationshipSummary, StoryLockSummary?, RiskWarning?. Enum-de-estado de elegibilidade com reason string.

- CompanionAvailabilityReason (UI/Companions/, NOVO): enum read-only para a UI, mapeado 1:1 a partir do
  AvailabilityReason de runtime existente (Available/LowRelationship/WrongTime/Working/Sleeping/InQuestEvent/
  Injured/Exhausted/StoryLocked/ReputationLocked/AlreadyActiveCompanion/ContextNotAllowed). NÃO duplicar a lógica
  do resolver — apenas projetar o resultado dele em vocabulário de UI + reason string.

- CompanionHudProjection (UI/Companions/, NOVO): HUD compacto. CompanionId, DisplayName, RoleIcon (papel funcional),
  Hp/MaxHp, Mp?/MaxMp? (ShowMp só quando relevante, espelhando a regra do HUD do jogador), Stamina/MaxStamina,
  Fatigue?, Status (InjuryState etc.), CurrentMode (FarmJob|CaveAssist|Visiting|Quest|Unavailable), BondLevel (0-5),
  PerkMilestones (marcos em 2/4), ActiveCommand, ActiveStance, IsSupporterMode (só papéis de suporte),
  DownedState (None|Downed|RescueWindow|Retreating|Injured), ReviveAttemptState (None|Attempting|Succeeded|Failed)
  — só EXIBIÇÃO, lógica é da cave assist —, EquipmentWeaponId?/EquipmentAccessoryId? (read-only, resolvidos por ID),
  CooldownIndicators[], Warnings[]. SEM Breath/Fôlego. SEM campos de pet.

- CompanionCommandStanceProjection (UI/Companions/, NOVO): modela o radial read-only. ActiveCommand
  (StayWait|Follow|AttackMarked), ActiveStance (Aggressive|Defensive|Passive), AvailableStances[] (sempre as 3
  universais; Supporter aparece como MODO adicional só se o papel expõe Suporter — flag do catálogo A7),
  CanRetreat (ação "recuar" do radial), e a sugestão de input ("tap = Seguir/Esperar; hold = radial") como dado de
  apresentação. A MUTAÇÃO (trocar comando/stance) é COMANDO de runtime na spec de cave assist; esta projection só
  diz o que o radial mostra.

- CompanionVisitProjection (UI/Companions/, NOVO): CompanionId, VisitType, FarmAreaTarget?, ArrivalTime,
  DepartureTime, Reason, CanInteract, DialogueSetId?, NoTeleportJustification (string explicando a rota/agenda —
  imagem de chegada coerente, nunca teleport mágico).

- CompanionDialogueHook (Companions/Dialogue/, NOVO): HookId, NpcId, HookType (InviteFarm|InviteCave|AssignJob|
  QuestTemporary|AskForHint|Dismiss|CheckStatus|SetCommand|SetStance), RequiredAvailability (AvailabilityReason que
  permite o hook), CommandId (id do comando/diálogo a disparar — string, sem ref Unity), RequiresConfirmation,
  SpoilerTier. Enganchar no DialogueResolver/DialogueCondition existente como condição/choice gated — não recriar
  runtime de diálogo. SetCommand/SetStance apenas DISPARAM um command id; a mutação é da cave assist.

- CompanionKnowledgeHintProjection (Companions/Knowledge/, NOVO): CompanionId, ExpertiseTag, HintType, KnowledgeKey?,
  Confidence, CanShowExact, TextKey, Cooldown, SpoilerGate. Hint leve e gated; NÃO resolve bestiário (apenas indica),
  read-only sobre o estado de knowledge.

- CompanionUIValidator (Editor/Validation/, NOVO): MenuItem no Editor (padrão dos ~60 validators do projeto),
  contadores de erro/warning. Valida: nenhuma projection expõe Breath/Fôlego; nenhuma projection de pet HUD;
  Romance/Spouse não aparecem como papel funcional (só flag); toda razão de indisponibilidade tem reason string;
  HUD compacto não redeclara HP/MP/Stamina do jogador como fonte (apenas vitals do companion).

- EditMode tests (Tests/EditMode/UI/Companions/, NOVO): derivação de cada razão de indisponibilidade; projeção de
  cave entry (role/vitals/risk/restrictions); campos do HUD compacto; ausência de pet HUD; ausência de Breath;
  gating de dialogue hook por disponibilidade; gating de spoiler do hint; comando/stance projetados corretamente
  (3 universais + Supporter condicional ao papel); estado de Downed/revive projetado (sem permadeath em combate base);
  2 slots de equipment read-only; reason string presente em todo estado não-acionável (regra ui-projection-pattern).
```

## Fora de escopo

```text
- Prefab/layout/canvas final do HUD/convite/radial (é wiring humano de cena + View MonoBehaviour de fase posterior);
- Dialogue text final (apenas hooks/condições, não conteúdo);
- A LÓGICA de comando/stance/downed/retreat/revive 30%/Injured — vive na spec de cave assist (esta spec só EXIBE);
- Persistência / save provider de companion — vive na spec de eligibility/state/save (V3.9.1). Esta spec não persiste;
- Atribuição/automação real de jobs (incl. toggle de fertilizante raro 3.8) — vive na spec de farm jobs board;
- Romance/casamento profundo, relationship full system, spouse runtime;
- Pet HUD / pet runtime / pet data (sistema separado e deferido);
- Quest content; equipment runtime/equipar pela HUD (drag/drop) — exibição read-only apenas;
- Mutação de qualquer estado de gameplay; tornar companion obrigatório para terminar o jogo;
- Tratar classes de D&D como papéis mecânicos.
```

## Regras de não duplicação

```text
- NÃO recriar o estado de companion: reusar CompanionEligibilityFlags/UnlockState/BondState/AvailabilityState/
  Resolver/FarmJobType/JobBoardService. Projection LÊ; não copia a lógica do resolver.
- NÃO adicionar Romance/Spouse ao enum CompanionRole (acréscimo #4 / V3.9.2): permanecem flags de elegibilidade.
- NÃO fundir CONTEXTO (enum [Flags] CompanionRole) com PAPEL FUNCIONAL (catálogo A7): RoleIcon usa papel funcional;
  os gates de convite usam contexto/elegibilidade (catálogo A7 PARTE D — eixos ortogonais).
- NÃO criar canal de visibility paralelo: a CompanionHud overlay obedece o HudVisibilityChangedEvent existente
  (some com modal). Não duplicar HudVisibilityController.
- NÃO criar 4ª stance universal: Suporter é MODO de papel (3.3); só Healer/Alchemist/MusicianSupport o expõem (A7 §6).
- NÃO recriar runtime de diálogo: enganchar no DialogueResolver/DialogueCondition existentes.
- NÃO criar provider de save nem persistir: V3.9.1 é da spec de eligibility/state/save.
- NÃO duplicar o HUD do jogador: a regra de presentation/visibility do HUD vive em ui_rules.md (cross-ref, não copiar).
- Comunicação via GameEventBus (ADR-0007); sem GameObject.Find/FindObjectOfType.
```

## Critérios de aceite

### CA-1 — Projeção de convite com razão explícita
- `CompanionInviteViewModel` existe (pura C#), deriva os gates CanInvite{Farm,Cave,Quest}/CanAssignJob/CanVisitFarm
  a partir do estado real e SEMPRE expõe uma `AvailabilityReason` legível quando indisponível (sem disable silencioso).
  Mapeia 1:1 do `AvailabilityResolver` existente; não duplica a lógica.
- Evidência: EditMode tests cobrindo cada razão (LowRelationship/WrongTime/Working/Sleeping/InQuestEvent/Injured/
  Exhausted/StoryLocked/ReputationLocked/AlreadyActiveCompanion/ContextNotAllowed) + reason string presente.

### CA-2 — HUD compacto correto, sem competir e sem Breath/pet
- `CompanionHudProjection` expõe vitals do companion, RoleIcon (papel funcional A7), bond 0-5 + marcos 2/4, status,
  CurrentMode, ActiveCommand, ActiveStance, IsSupporterMode (só papéis de suporte), feedback de Downed/revive (só
  exibição) e os 2 slots de equipment read-only. NÃO expõe Breath/Fôlego; NÃO expõe campos de pet; NÃO redeclara
  HP/MP/Stamina do JOGADOR. Obedece a visibility do HUD (some com modal).
- Evidência: EditMode tests (campos presentes; sem Breath; sem pet; Supporter condicional; equipment read-only) +
  `CompanionUIValidator` Editor PASS + cenário humano (deferido).

### CA-3 — Comando + Stance projetados (decisão 3.2/3.3)
- `CompanionCommandStanceProjection` expõe ActiveCommand (StayWait|Follow|AttackMarked), ActiveStance
  (Aggressive|Defensive|Passive) com Defensivo como default conceitual, AvailableStances[] = sempre as 3 universais,
  e Supporter como MODO adicional só quando o papel expõe (A7 §6). CanRetreat presente. A projection NÃO muta estado;
  a troca é command id para a cave assist.
- Evidência: EditMode tests (3 stances sempre; Supporter só para Healer/Alchemist/MusicianSupport; comando default;
  recuar exposto; nenhuma mutação).

### CA-4 — Feedback de Downed/revive sem permadeath em combate base
- `CompanionHudProjection.DownedState` (None|Downed|RescueWindow|Retreating|Injured) e `ReviveAttemptState`
  (None|Attempting|Succeeded|Failed) existem só como EXIBIÇÃO; a UI nunca sugere morte permanente em combate base
  (3.7). A lógica (chance 30%, escape, duração de Injured 1-2 dias) é da cave assist.
- Evidência: EditMode test (estados projetados; nenhum estado "Dead/Permadeath" em combate base) + nota de que a
  lógica vive na cave assist.

### CA-5 — Convite/visita sem teleport injustificado + hooks de diálogo gated
- `CompanionVisitProjection` carrega ArrivalTime/DepartureTime coerentes com agenda e `NoTeleportJustification`.
  `CompanionDialogueHook` só oferece o hook quando `RequiredAvailability` permite e respeita `SpoilerTier`;
  enganchado no DialogueResolver/DialogueCondition existente (não recriado).
- Evidência: EditMode tests (hook gated por disponibilidade; visita com justificativa; SetCommand/SetStance só
  disparam command id, não mutam aqui).

### CA-6 — Hint de knowledge gated, sem resolver bestiário
- `CompanionKnowledgeHintProjection` respeita ExpertiseTag/Confidence/SpoilerGate/Cooldown e NÃO completa o bestiário
  (apenas indica; `CanShowExact` controla o nível). Diálogo não revela spoiler de quest principal.
- Evidência: EditMode tests (spoiler gate bloqueia hint indevido; hint não marca knowledge como resolvido).

### CA-7 — Romance/Spouse só como flag; contexto ≠ papel funcional
- A UI NÃO trata Romance/Spouse como papel funcional (acréscimo #4 / V3.9.2): RoleIcon vem de PrimaryRole/SecondaryRole;
  romance/spouse, se exibidos, são leitura de flag (CompanionEligibilityFlags), sem habilitar combate/job. Nenhuma
  entrada Romance/Spouse adicionada ao enum CompanionRole.
- Evidência: `CompanionUIValidator` Editor PASS + EditMode test (papel funcional ≠ contexto; flag de romance não vira role).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Companions/CompanionAvailabilityReason.cs        (NOVO — enum UI + reason mapping)
Assets/_Game/Scripts/UI/Companions/CompanionInviteViewModel.cs           (NOVO — projection)
Assets/_Game/Scripts/UI/Companions/CompanionHudProjection.cs             (NOVO — projection compacta)
Assets/_Game/Scripts/UI/Companions/CompanionCommandStanceProjection.cs   (NOVO — radial read-only 3.2/3.3)
Assets/_Game/Scripts/UI/Companions/CompanionVisitProjection.cs           (NOVO — projection de visita)
Assets/_Game/Scripts/Companions/Dialogue/CompanionDialogueHook.cs        (NOVO — hook gated; engancha no Dialogue existente)
Assets/_Game/Scripts/Companions/Knowledge/CompanionKnowledgeHintProjection.cs (NOVO — hint gated)
Assets/_Game/Scripts/Editor/Validation/CompanionUIValidator.cs           (NOVO — Editor validator)
Assets/_Game/Tests/EditMode/UI/Companions/CompanionUIInviteHudDialogueTests.cs (NOVO — EditMode)
docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md (report)
docs/validation/playmode/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_human_test_scenario.md (cenário humano, deferido)
```

Consolidar com o existente: se a spec de eligibility/state/save já tiver criado alguma projection de companion, HARDEN
em vez de recriar (decisão de Fase 0). RoleIcon usa o identificador de papel funcional do catálogo A7 (string estável),
nunca o enum de contexto.

## Contratos

### Data contracts (todos os campos = tipos simples + IDs; sem refs Unity; ui-projection-pattern)

`CompanionInviteViewModel`: `string CompanionId, NpcId, DisplayName`; `string PrimaryRole, SecondaryRole`
(papel funcional A7); `UnlockState UnlockState`; `CompanionAvailabilityReason Availability`;
`List<CompanionAvailabilityReason> AvailabilityReasons`; bools `CanInviteFarm/CanInviteCave/CanInviteQuest/
CanAssignJob/CanVisitFarm`; `string RelationshipSummary, StoryLockSummary, RiskWarning`. Computed: `bool IsAvailable
=> Availability == Available`.

`CompanionAvailabilityReason` (enum UI): mapeado do `AvailabilityReason` runtime; valores Available/LowRelationship/
WrongTime/Working/Sleeping/InQuestEvent/Injured/Exhausted/StoryLocked/ReputationLocked/AlreadyActiveCompanion/
ContextNotAllowed. Função pura de mapeamento `FromRuntime(AvailabilityReason) -> CompanionAvailabilityReason` +
`Describe(...) -> string` (reason string legível).

`CompanionHudProjection`: `string CompanionId, DisplayName, RoleIcon`; `int Hp, MaxHp, Stamina, MaxStamina`;
`bool ShowMp; int Mp, MaxMp`; `float? Fatigue`; `InjuryState Status`; enum `CompanionMode CurrentMode {FarmJob,
CaveAssist,Visiting,Quest,Unavailable}`; `int BondLevel (0-5); List<int> PerkMilestones`;
`CompanionCommand ActiveCommand`; `CompanionStance ActiveStance`; `bool IsSupporterMode`;
enum `DownedState {None,Downed,RescueWindow,Retreating,Injured}`; enum `ReviveAttemptState {None,Attempting,
Succeeded,Failed}`; `string EquipmentWeaponId, EquipmentAccessoryId`; `List<string> CooldownIndicators, Warnings`.
Computed: `bool HasCompanion`, `float HpPercent`, etc. PROIBIDO: qualquer campo Breath/Fôlego ou de pet.

`CompanionCommandStanceProjection`: enum `CompanionCommand {StayWait,Follow,AttackMarked}`; enum `CompanionStance
{Aggressive,Defensive,Passive}` (Defensive = default conceitual); `List<CompanionStance> AvailableStances` (sempre as
3); `bool ExposesSupporterMode` (true só p/ papel de suporte A7); `bool CanRetreat`; `string InputHint`
("tap = Seguir/Esperar; hold = radial"). Sem mutação.

`CompanionVisitProjection`: `string CompanionId; enum VisitType; string FarmAreaTarget; string ArrivalTime,
DepartureTime; string Reason; bool CanInteract; string DialogueSetId; string NoTeleportJustification`.

`CompanionDialogueHook`: `string HookId, NpcId; enum HookType {InviteFarm,InviteCave,AssignJob,QuestTemporary,
AskForHint,Dismiss,CheckStatus,SetCommand,SetStance}; CompanionAvailabilityReason RequiredAvailability; string
CommandId; bool RequiresConfirmation; int SpoilerTier`.

`CompanionKnowledgeHintProjection`: `string CompanionId, ExpertiseTag; enum HintType; string KnowledgeKey; float
Confidence; bool CanShowExact; string TextKey; float Cooldown; int SpoilerGate`.

### Runtime contracts
Todas as classes acima são PURE C# projections/hooks (zero `UnityEngine.UI`, zero MonoBehaviour). Uma futura View
(MonoBehaviour, fase posterior) reconstrói a partir de eventos (nunca polling). Pode existir um builder/serviço pura-C#
que preenche as projections a partir do estado de companion (precedente: builders do ui-projection-pattern) — também
testável em EditMode.

### Event contracts
Adds events: por padrão NÃO. Esta camada CONSOME estado (via builder a partir de eventos publicados pelas outras 3
specs: combate publica comando/stance/downed/revive; eligibility publica disponibilidade; job board publica
atribuição). Se uma View reativa for criada nesta spec, ela SUBSCRIBE eventos existentes e usa o padrão unsubscribe
on disable (event_rules.md). NÃO publicar eventos de gameplay. Visibility da overlay = HudVisibilityChangedEvent
existente. Se um evento de "companion UI rebuild requested" for necessário e a outra spec ainda não o publicar:
DEFERRED_INTEGRATION (modelar a projection + documentar o gancho esperado), nunca inventar mutação.

### Save/UI contracts
Save: N/A — esta spec NÃO persiste nada (V3.9.1 é da spec de eligibility/state/save). UI: define apenas a CAMADA DE
DADOS (projections/hooks); prefab/layout/canvas final é fase posterior. A apresentação/visibility do HUD obedece
`ui_rules.md` (cross-ref, não duplicar) e qualquer painel modal de companion obedece `ui_modal_rules.md`.

## Sistemas afetados

```text
UI/HUD (camada de projection — slot companion do GameplayHudViewModel), Companion state (LEITURA apenas),
Dialogue (hooks gated enganchados no resolver existente), Knowledge/Bestiary (hint read-only).
NÃO tocados: combate runtime, cave snapshot, save provider, job automation runtime, pet, romance.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Companions/**          (projections de UI — NOVO)
Assets/_Game/Scripts/Companions/Dialogue/**    (hook gated — NOVO)
Assets/_Game/Scripts/Companions/Knowledge/**   (hint projection — NOVO)
Assets/_Game/Scripts/Editor/Validation/CompanionUIValidator.cs (Editor validator — NOVO)
Assets/_Game/Tests/EditMode/UI/Companions/**   (EditMode tests — NOVO)
docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md
docs/validation/playmode/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_human_test_scenario.md
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
Packages/** ; ProjectSettings/**
Assets/**/*.unity ; Assets/**/*.prefab ; Assets/**/*.asset (manuais; prefab/layout é wiring humano de cena)
Assets/_Game/Scripts/Pets/** (pet HUD/runtime — sistema separado, proibido aqui)
Assets/_Game/Scripts/Companions/CompanionRole.cs (NÃO adicionar Romance/Spouse nem papel funcional ao enum de contexto)
Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs / CompanionBondState.cs / CompanionAvailabilityResolver.cs
  (LEITURA apenas — não mutar/alterar; lógica é das outras specs)
Assets/_Game/Scripts/Save/** (provider de companion é da spec de eligibility/state/save)
Assets/_Game/Scripts/Cave/** ; Assets/_Game/Scripts/Combat/** (lógica de assist/downed/revive é da cave assist)
Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs (o slot CompanionProjection já existe; integração de string é
  acréscimo da View posterior, não reescrita do HUD do jogador)
.specs/SPEC_EXECUTION_ORDER.md ; .specs/implementados/** ; docs/refinements/implementados/**
docs/project/CURRENT_STATE.md ; PROJECT_LOG.md ; índices compartilhados (fable_00C, SPEC_REGISTRY, .specs,
  GAME_RULES_INDEX, DECISION_LOG)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar (system-reuse-audit):
  - confirmar que UI/Companions/** não existe e que a spec de eligibility/state/save não criou projection equivalente
    (se criou: HARDEN, não recriar);
  - mapear de qual evento/serviço cada projection é reconstruída (combate/eligibility/job board); se o evento ainda
    não existe na outra spec, marcar DEFERRED_INTEGRATION e documentar o gancho esperado (nunca inventar mutação);
  - confirmar o identificador de PAPEL FUNCIONAL do catálogo A7 a usar no RoleIcon (string estável; sem mexer no enum
    de contexto);
  - decidir gancho dos hooks de diálogo no DialogueResolver/DialogueCondition existente;
  - decidir radial = overlay HUD (ui_rules) vs. painel modal (ui_modal_rules).
### Fase 1 — Enums + CompanionAvailabilityReason (mapeamento puro do runtime) + testes de razão.
### Fase 2 — CompanionInviteViewModel + CompanionVisitProjection (gates + razão + justificativa de visita) + testes.
### Fase 3 — CompanionHudProjection + CompanionCommandStanceProjection (vitals/bond/role/comando/stance/Supporter/
            downed/revive/equipment read-only) + testes (sem Breath, sem pet, Supporter condicional, sem permadeath).
### Fase 4 — CompanionDialogueHook (gated, enganchado no Dialogue existente) + CompanionKnowledgeHintProjection
            (gated por spoiler) + testes.
### Fase 5 — CompanionUIValidator (Editor): sem Breath, sem pet HUD, romance/spouse só flag, reason string presente,
            HUD não duplica vitals do jogador.
### Fase 6 — csproj includes; run_strict_validation; execution report (Spec Compliance Matrix) + cenário humano deferido.
```

## Paralelização

- Parallelizable: YES — única do grupo WAVE_14_COMPANIONS_FUTURE marcada YES (decisão 3.12). Camada de projection-only
  em pasta NOVA (`UI/Companions/**`, `Companions/Dialogue/**`, `Companions/Knowledge/**`) que NÃO colide com os locks
  das outras 3 specs (que escrevem `Companions/**` raiz, `Cave/**`, `Combat/**`, `Save/**`).
- Can run with: specs docs-only de reconciliação/registry; specs de knowledge UI que não toquem os mesmos arquivos.
- Must not run with: a spec de cave assist (escreve Companions/Cave/Combat e é dona da lógica de stance/comando/
  downed/revive), a de eligibility/state/save (dona do estado/save que esta lê), a de farm jobs (dona do job board);
  qualquer spec de UI prefab/layout final, dialogue writing, romance/spouse, pet HUD, relationship full, save migration
  ou quest content.
- Shared files/systems that require lock: nenhum arquivo compartilhado de escrita (esta spec cria pastas próprias e só
  LÊ o estado de companion). Dependência LÓGICA: os contratos de estado (eligibility/cave assist/job board) devem
  existir ou ter o gancho de evento documentado (DEFERRED_INTEGRATION).
- Reason: projection layer read-only que consome muitos estados, mas não escreve nos arquivos das outras specs.

## Impacto em save/load

```text
Does this change save schema? NO.
Does this add a save section? NO (provider de companion é da spec de eligibility/state/save — V3.9.1).
Does this require migration? NO.
Does this persist Unity references? NO (projections puras, transientes; só IDs/labels).
```

## Impacto em eventos

```text
Adds events: NO por padrão (consome eventos existentes publicados pelas outras 3 specs; visibility = HudVisibilityChangedEvent).
Changes existing events: NO.
Requires unsubscribe pattern: YES, se uma View reativa for criada nesta spec (subscribe on enable / unsubscribe on disable — event_rules.md).
DEFERRED_INTEGRATION: se um evento da outra spec ainda não existir, modelar a projection + documentar o gancho; nunca inventar mutação/evento de gameplay.
```

## Impacto em UI/Unity

```text
Changes UI data/projection: YES (camada de dados read-only de companion; preenche o slot CompanionProjection do HUD).
Changes prefabs/layout: NO (prefab/layout/canvas final é wiring humano de fase posterior).
Changes scenes/prefabs/assets: NO.
Requires Play Mode final validation: YES — DEFERRED para o fluxo visual de convite/HUD/radial/visita/diálogo.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION (registrar em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md; não pedir validação humana por spec).
```

## Riscos técnicos

```text
Risco: UI sugere que companion indisponível é bug (sem motivo). 
  Mitigação: AvailabilityReason explícita SEMPRE + reason string (ui-projection-pattern Rule 3); CA-1; validator.
Risco: HUD de companion compete com HP/MP/Stamina do jogador / vira poluição.
  Mitigação: HUD compacto, discreto (3.11 §51); obedece HudVisibilityChangedEvent; validator checa que não redeclara
  vitals do jogador como fonte; cross-ref ui_rules.md em vez de duplicar regra de HUD.
Risco: Breath/Fôlego ou pet HUD entram por engano.
  Mitigação: campos proibidos no contrato; pasta Pets/** proibida; CompanionUIValidator falha se detectar.
Risco: Romance/Spouse tratados como papel mecânico (confusão contexto × papel funcional).
  Mitigação: RoleIcon usa papel funcional A7; romance/spouse só leitura de flag; enum de contexto não alterado;
  validator + CA-7 (acréscimo #4 / V3.9.2).
Risco: a projection tentar MUTAR estado (trocar stance/comando, equipar) — viola projection-only.
  Mitigação: comando/stance/equip são command id para a cave assist; nenhuma mutação nas classes desta spec; testes
  garantem ausência de setter de gameplay.
Risco: dialogue hint vaza spoiler de quest principal / resolve bestiário sozinho.
  Mitigação: SpoilerTier/SpoilerGate/Confidence/CanShowExact; CA-6; hint apenas indica.
Risco: visita teleporta sem justificativa.
  Mitigação: ArrivalTime/DepartureTime coerentes com agenda + NoTeleportJustification obrigatória; CA-5.
Risco: evento de origem (comando/stance/downed) ainda não publicado pela cave assist.
  Mitigação: DEFERRED_INTEGRATION — modelar a projection + documentar o gancho de evento esperado; nunca inventar.
Risco: criar segundo sistema de estado de companion.
  Mitigação: system-reuse-audit (Fase 0); LER o estado existente; HARDEN se já houver projection parcial.
```

## Rollback

```text
Remover a pasta UI/Companions/**, Companions/Dialogue/CompanionDialogueHook.cs, Companions/Knowledge/
CompanionKnowledgeHintProjection.cs, o CompanionUIValidator e os testes desliga a camada de apresentação de companion.
O estado de companion (eligibility/bond/save), a lógica de cave assist e o job board permanecem intactos (são de
outras specs). O slot CompanionProjection do HUD volta a ficar vazio. Nenhum save afetado (esta spec não persiste).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: system-reuse-audit (UI/Companions inexistente? projection parcial em outra spec?), mapear evento/
        serviço de origem por projection (DEFERRED_INTEGRATION se faltar), confirmar identificador de papel funcional A7,
        decidir gancho de diálogo e radial overlay-vs-modal. Registrar achados no report.
- [ ] T002 — CompanionAvailabilityReason (enum UI + FromRuntime/Describe puros) + testes de cada razão com reason string.
- [ ] T003 — CompanionInviteViewModel + CompanionVisitProjection (gates derivados + razão + NoTeleportJustification) + testes.
- [ ] T004 — CompanionHudProjection + CompanionCommandStanceProjection (vitals/bond/role/comando/stance/Supporter
        condicional/Downed/revive/equipment read-only) + testes (sem Breath, sem pet, sem permadeath, Supporter só p/ suporte).
- [ ] T005 — CompanionDialogueHook (gated, enganchado no Dialogue existente) + CompanionKnowledgeHintProjection (spoiler gate) + testes.
- [ ] T006 — CompanionUIValidator (Editor): sem Breath, sem pet HUD, romance/spouse só flag, reason string presente, HUD não duplica vitals do jogador.
- [ ] T007 — csproj includes; run_strict_validation; execution report (Spec Compliance Matrix + honest status) + cenário humano deferido em docs/validation/playmode/.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

Busca local mínima (Fase 0 — PowerShell; equivalente ao rg, conforme windows_powershell_only):

```powershell
Select-String -Path .\Assets\_Game\Scripts\*,.\docs\design\*,.\.specs\* -Pattern "CompanionInvite|CompanionHud|CompanionVisit|CompanionDialogue|AvailabilityReason|KnowledgeHint|Romance|Spouse|PetHUD|Breath|Folego" -Recurse
```

Unity compile + log scan quando houver alteração C# (sequencial, sem batchmode paralelo):

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests (lógica determinística criada): Unity Test Runner — EditMode, ou comando local equivalente do repo.

PlayMode/validação humana: não pedir por spec; registrar o cenário visual de convite/HUD/radial/cave entry/visita/
diálogo no execution report e vincular a `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`.

## Testing Quality Gate

```text
Changed runtime code: YES (projections puras / hook read-only — lógica determinística de derivação).
Changed deterministic logic: YES (gates de elegibilidade derivados, mapeamento de razão, Supporter condicional ao papel,
  gating de hook/spoiler, projeção de comando/stance/downed). 
Changed Unity scene/prefab/asset wiring: NO (prefab/layout final é fase posterior; ref de View é wiring humano).
Automated tests added/updated: YES (EditMode — cada AvailabilityReason; cave entry projection; campos do HUD;
  ausência de Breath; ausência de pet; Supporter só p/ suporte; comando/stance projetados; Downed/revive sem permadeath;
  2 slots de equipment read-only; dialogue hook gated; spoiler gate do hint; reason string em todo estado não-acionável).
Automated tests command: Unity Test Runner — EditMode (ou equivalente local).
Manual Play Mode scenario: YES — DEFERRED — docs/validation/playmode/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_human_test_scenario.md
Justification if no automated tests: N/A (testes EditMode obrigatórios para as projeções).
Residual risk: feel/layout visual (compactação, anti-poluição, radial 1-tecla) só validável em Play Mode; integração
  com os eventos das outras 3 specs pode ficar DEFERRED_INTEGRATION até elas publicarem os eventos de origem.
```

## Definition of Done

```text
Projections/hooks read-only implementados apenas dentro do escopo (UI/Companions, Companions/Dialogue, Companions/Knowledge,
  Editor/Validation, Tests/EditMode/UI/Companions); nada de prefab/layout, dialogue text, romance/pet, mutação ou persistência.
Toda indisponibilidade tem AvailabilityReason + reason string; HUD compacto sem Breath/Fôlego e sem pet; comando/stance
  (3 universais + Supporter condicional) e feedback de Downed/revive projetados sem permadeath em combate base; 2 slots de
  equipment read-only; visita com justificativa de não-teleport; hooks de diálogo e hint gated por elegibilidade/spoiler;
  Romance/Spouse só como flag (não papel funcional, enum de contexto inalterado).
CompanionUIValidator PASS; EditMode tests PASS; builds Assembly-CSharp e Assembly-CSharp-Editor 0E; run_strict_validation
  exit 0; execution report com Spec Compliance Matrix + honest status + Testing Quality Gate + cenário humano deferido.
Sem promoção a ACCEPTED apenas por compile (no máximo BUILD_VALIDATED nesta fase; runtime gated WAVE 14).
```

## Anti-regressão

```text
NÃO recriar o estado de companion (eligibility/unlock/bond/availability/resolver/job board) — só LER.
NÃO adicionar Romance/Spouse ao enum CompanionRole; NÃO fundir contexto com papel funcional.
NÃO implementar pet HUD / pet runtime; NÃO implementar romance/casamento profundo / spouse runtime.
NÃO adicionar Breath/Fôlego como recurso de companion.
NÃO criar 4ª stance universal (Suporter é modo de papel — só Healer/Alchemist/MusicianSupport).
NÃO mutar estado de gameplay nem persistir (sem save provider aqui).
NÃO recriar runtime de diálogo (enganchar no DialogueResolver/DialogueCondition existente).
NÃO duplicar o HUD do jogador nem o canal de visibility (usar HudVisibilityChangedEvent existente).
NÃO usar GameObject.Find/FindObjectOfType; comunicação via GameEventBus (ADR-0007).
NÃO tornar companion obrigatório para terminar o jogo; companion ajuda, não joga pelo jogador.
NÃO tratar classes de D&D como papéis mecânicos.
NÃO alterar índices compartilhados (SPEC_EXECUTION_ORDER, SPEC_REGISTRY, CURRENT_STATE, .specs, GAME_RULES_INDEX, DECISION_LOG).
NÃO executar runtime em massa antes da WAVE 14 ou de exceção humana explícita (3.12 — execução gated).
```

---

## Notas para execução posterior

Esta spec é SPEC-READY (contrato destravado) mas a EXECUÇÃO de runtime é gated na WAVE 14 (decisão 3.12). Deve ser
executada quando os contratos de estado das outras 3 specs companion (eligibility/state/save, cave assist/brain/balance,
farm jobs board/automation) estiverem disponíveis ou com o gancho de evento documentado, e quando City/NPC, Farm, Cave,
Combat, Save, UI e Quest estiverem estáveis — ou por decisão humana explícita de antecipar Companions.

Cross-references (as 4 specs companion da WAVE 14 são consistentes entre si):
- `14_spec_companion_eligibility_recruitment_state_save_future_runtime` — estado/elegibilidade/bond + CompanionSaveSectionProvider (V3.9.1).
- `14_spec_companion_cave_assist_brain_balance_future_runtime` — lógica de comando/stance/downed/retreat/revive 30%/budget que ESTA spec exibe.
- `14_spec_companion_farm_jobs_board_automation_future_runtime` — JobBoard/atribuição/automação (incl. toggle de fertilizante 3.8) que ESTA spec projeta.
- Catálogo de papéis (A7): `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` — RoleIcon, stance default, flag Suporter.

*Reescrita em SpecKit denso 2026-06-13 — preserva a substância da spec original + EMENDA 2026-06-13-V3 (decisões 3.2/3.3/3.4/3.5/3.6/3.11/3.12, acréscimos #3/#4). Projection-only; runtime gated WAVE 14.*
