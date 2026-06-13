# SPEC — Companion: Brain de Combate na Caverna, Stances/Comandos, Balance de DPS e Downed/Resgate (future runtime)

> **Spec ID:** `14_spec_companion_cave_assist_brain_balance_future_runtime`
> **Status:** A implementar / SPEC-READY (contrato destravado; execução de runtime **gated na WAVE 14** — decisão 3.12; ver EMENDA 2026-06-13-V3 integrada abaixo)
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks
> **Priority:** P1
> **Type:** Runtime / Future / Cave Assist / Companion AI / Balance
> **Domain:** Companion / Cave Entry / Brain / Assist / Combat Budget / Downed State / Revive
> **Parallelizable:** NO
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE
> **Can run with:** N/A — toca Companions/**, Cave/** e Combat/** ao mesmo tempo; precisa de lock exclusivo na pasta de companion.
> **Must not run with:** qualquer spec que altere enemy AI core (`EnemyBrain.cs`), combat formulas, cave procedural snapshot (`CaveSnapshotService`/`VisitedLevelSnapshot`), boss fights, pet cave runtime, save migration do `SaveManager`, companion equipment data ou UI/HUD final de companion (essas são as outras 3 specs companion da WAVE 14 — cross-ref na seção 0.4).
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/Cave/**` (novo), `Assets/_Game/Scripts/Core/Events/**` (eventos novos de companion), `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md`. **Leitura-apenas** (audit) de `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Enemy/**`, `Assets/_Game/Scripts/Combat/**`.
> **Depends on:**
> - `14_spec_companion_eligibility_recruitment_state_save_future_runtime` (provê elegibilidade/unlock/bond/CaveRank/InjuryState + o `CompanionSaveSectionProvider` de round-trip; esta spec **consome** esse estado — não recria; ver cross-ref 0.4)
> - Cave runtime estável (FASE9F / ADR-0005): `CaveRunManager`, `CaveRunSeed`, `VisitedLevelSnapshot`, `CaveEnemySpawnPlan` (existentes — esta spec lê o estado da run, não o regenera)
> - GameEventBus + eventos de combate existentes (`DamageAppliedEvent`, `PlayerDamagedEvent`, `EnemyKilledEvent`, `CavePlayerDefeatedEvent`, `PlayerDiedEvent`) — consumir; não criar canal paralelo
> - `CompanionBalanceProfileSO` (asset de balance — a SER criado nesta spec como contrato de dados; valores de tuning 25%/40% vêm da decisão 3.10)
> **Blocks:**
> - cave entry loadout UI / companion HUD (spec `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime`)
> - combat balance validation de caverna com companion ativo
> - enemy target priority adapters (alvo do companion vs. budget de combate)
> - knowledge/research hints em caverna (ação ResearchHint do papel Researcher)
> **Scope:** definir e endurecer o **companion de caverna** em combate: seleção/entrada (1 ativo, opcional), follow/leash/safe-spawn, brain states + assist actions, comandos (Ficar/Seguir/Atacar-alvo-marcado) e stances (Agressivo/Defensivo-default/Passivo + modo Suporter), balance de DPS (25%/40% via `CompanionBalanceProfileSO`), downed → resgate → retreat → Injured, revive do player (30%), regra de não-puxar-packs, combat budget adapter e persistência do estado do companion **dentro da run** (cave-stable-run).
> **Out of scope:** enemy AI rewrite, boss AI, combat formula tuning, geração/snapshot procedural da caverna, animações de arte, pet cave runtime, romance/casamento, companion equipment data assets (slots arma+acessório são **lidos** aqui, mas a criação dos data assets/UI de equipar é de outra spec), UI/HUD visual final, party multi-companion na caverna.

required_adrs: [ADR-0005, ADR-0007]
required_game_rules: [combat_rules.md, cave_rules.md, fonte_rules.md, event_rules.md]

---

## 0. Integração da EMENDA 2026-06-13-V3 (decisões vinculantes do Refinamento v3)

> **Fontes vinculantes (canônicas):**
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, BLOCO 3 (decisões 3.2–3.12).
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md` → seção "EMENDA 2026-06-13-V3 (Refinamento v3)" (linha ~1450) + override de equipment §45-46→3.4 (linha ~1088) + stances (linhas ~1479-1500).
> - `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` — bônus + ações + stance default + exposição de Suporter por papel (artefato A7 da decisão 3.1).
> - Game rules: `docs/game_rules/combat_rules.md`, `docs/game_rules/cave_rules.md`, `docs/game_rules/fonte_rules.md` (Rule 8 — revive do companion), `docs/game_rules/event_rules.md`.
>
> Por docs-governance, `docs/amendments/**` é registro histórico e **não** é citado como fonte canônica. As decisões abaixo já estão refletidas no corpo denso desta spec; esta seção é o resumo de reconciliação para o executor.

Esta spec deixou de ser "future mapped" e é **SPEC-READY** (decisão 3.12): o contrato está destravado, a execução de runtime segue **gated na WAVE 14**. O documento permanece em `docs/specs/a_implementar/features_futuras/`. Decisões v3 que o runtime DEVE refletir quando executado:

### 0.1 Comandos + Stances (decisões 3.2 / 3.3)

```text
COMANDOS (universais, todos os papéis — catálogo §1.1):
  Ficar/Esperar       — fica na posição atual; não segue, não puxa pack.
  Seguir              — acompanha o jogador respeitando FollowDistance/leash.
  Atacar alvo marcado — foca o inimigo marcado pelo jogador (override TEMPORÁRIO da stance).
  Input (3.2): 1 tecla — tap = alterna Seguir/Esperar; hold = menu radial com stance + recuar.

STANCES (3 universais — decisão 3.3 / catálogo §1.2):
  Agressivo  — ataca qualquer inimigo num raio <= 12 tiles do jogador.
  Defensivo  — DEFAULT — só ataca quem chega a <= 4 tiles do jogador OU quem ataca o jogador.
  Passivo    — não ataca por iniciativa própria (só obedece "Atacar alvo marcado").

SUPORTER (decisão 3.3 — catálogo §1.3):
  NÃO é 4ª stance universal. É um MODO de papel, exposto SOMENTE por papéis de suporte
  (Healer, Alchemist, MusicianSupport — ExposesSupporterMode = true no catálogo §6).
  Em Suporter o papel prioriza cura/buff/cleanse sobre dano, dentro de MP/cooldown/limites.
  Papéis sem capacidade de suporte NÃO expõem o modo Suporter.
```

`CompanionBrainState`, `CompanionAssistAction` (§7.2/§7.4 do modelo de domínio) e a AI priority (§9) devem honrar **comando + stance** antes de qualquer iniciativa.

### 0.2 Equipment base (decisão 3.4 — OVERRIDE)

```text
3.4 substitui §45-46 da COMPANIONS_DIRECTION (eram "futuro"): o sistema base equipa
ARMA + ACESSÓRIO por companion (2 slots). O cálculo de contribuição/DPS/budget
(CompanionCombatBudgetAdapter, §10) DEVE considerar o gear equipado SEM estourar os limites
de DPS/budget. ESTA spec LÊ os ids de gear (string) do estado do companion para escalar o
DPS-alvo; a criação dos data assets de equipment e a UI de equipar são de OUTRA spec
(out of scope). O gear é persistido como IDs (string) no save da spec de eligibility/state.
```

### 0.3 Downed / Resgate / Injured + Revive do player + Permadeath (decisões 3.5 / 3.6 / 3.7)

```text
3.5 — Derrota do companion: Downed -> janela de resgate (player pode reerguer)
      -> senão Retreat automático -> Injured 1-2 dias. Detalha §11 e CompanionDownedInjuredState.
      NÃO há permadeath em combate base.
3.6 (CUSTOM) — Player derrotado COM companion ativo: companion TENTA REVIVER o player
      (30% de chance). Se falhar, escapa e volta Injured 1 dia. Adicionar regra/ação de revive
      ao brain/assist; SEM garantir 100% no sistema base. Cross-ref death/fonte (§11.4).
3.7 (OVERRIDE) — Permadeath SOMENTE em eventos narrativos roteirizados; combate procedural de
      caverna permanece SEM permadeath (apenas Injured). Fonte de Anya ressuscita o companion
      em eventos narrativos com custo progressivo ("Ressurreição Dolorosa") — fonte_rules.md
      Rule 8. Boss/guardrails (§10) e edge cases (§23D) assumem que nenhum combate procedural
      mata o companion permanentemente.
```

### 0.4 Balance, Bond e governança (decisões 3.10 / 3.11 / 3.12) + cross-ref das specs irmãs

```text
3.10 — DPS alvo: 25% comum / 40% especialista do DPS esperado do jogador no mesmo estágio,
       fixados em CompanionBalanceProfileSO.DpsContributionTargets. As faixas amplas
       15-35% / 35-50% (COMPANIONS_DIRECTION §29) continuam como LIMITE; o ALVO de tuning é
       25% / 40% (suporte/fazenda abaixo da banda comum).
3.11 — Bond 0-5; perk passivo nos níveis 2 e 4; CaveRank é trilha SEPARADA de bond e de JobRank.
       O brain/balance leem bond e CaveRank como trilhas distintas (campos já em
       CompanionBondState.cs: BondLevel/JobRank/CaveRank).
3.12 — Execução gated na WAVE 14: a spec fica DENSA e pronta, mas só é executada quando City/NPC,
       Farm, Cave, Combat, Save, UI e Quest estiverem estáveis ou houver decisão humana explícita.

CROSS-REF (as 4 specs companion da WAVE 14 — caminhos):
  - eligibility/recruitment/state/save:
    docs/specs/a_implementar/features_futuras/14_spec_companion_eligibility_recruitment_state_save_future_runtime.md
    (provê CompanionSaveSectionProvider + bond/CaveRank/InjuryState/gear ids — esta spec consome)
  - farm jobs board / automation:
    docs/specs/a_implementar/features_futuras/14_spec_companion_farm_jobs_board_automation_future_runtime.md
    (jobs de fazenda — multiplos companions; fertilizante raro toggle default OFF — decisão 3.8)
  - UI / HUD / invite / visit / dialogue hooks:
    docs/specs/a_implementar/features_futuras/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime.md
    (consome o estado de brain/stance/HUD que ESTA spec expõe via eventos)
```

### 0.5 Notas de reconciliação de código (re-auditoria 2026-06-13)

```text
- ZERO código de companion em combate/caverna hoje: nenhum brain/assist/downed/HUD/stances/
  comandos/equip/revive de companion existe em runtime. Esta spec implementa o contrato do zero
  quando executada (CREATE_MINIMAL esperado). Confirmado por busca (seção 23B).
- CompanionBondState.cs já tem BondLevel/JobRank/CaveRank/InjuryState; CompanionSaveEntry
  (SaveData.cs:231-245) já tem os mesmos campos. REUTILIZAR, não duplicar.
- O round-trip de save (CompanionSaveSectionProvider, precedente HotbarSectionProvider) é
  responsabilidade da spec de eligibility/state/save (acréscimo re-audit #3). ESTA spec apenas
  CONSOME o estado já persistido por aquela spec e persiste o CompanionCaveRunState DENTRO da run.
- Romance/Spouse = flags de elegibilidade (CompanionEligibilityFlags.cs:13-14), NÃO papéis no
  enum CompanionRole (acréscimo re-audit #4). NÃO adicionar Romance/Spouse ao enum de papel.
```

---

# /speckit.specify

## 1. Contexto

A caverna de Cindar's Hope é o loop de risco do jogo: procedural por run e estável dentro da run (ADR-0005; `cave_rules.md`). O companion é uma **opção/vantagem, não requisito** — o jogador deve poder descer sozinho. A caverna assume no máximo **1 companion ativo** (na fazenda podem existir múltiplos, em jobs — outra spec). O companion precisa funcionar com snapshot/replay da run, level procedural, checkpoints (`CaveCheckpointService`), boss gates (`CaveBossGateService`), safe spawn (`CaveSpawnAnchor`), confinement walls (`CavePlayerPathConfinement`), leash e fog/reveal se existir — sem nunca reescrever esses sistemas.

Esta spec cria os contratos de **cave assist / brain / balance**: a inteligência de combate do companion na caverna (states, ações, prioridades), os controles do jogador (comandos + stances + modo Suporter), o teto de poder (DPS 25%/40%, healer não-infinito, não-tankar-boss, não-puxar-pack), o ciclo de derrota (Downed → resgate → retreat → Injured, sem permadeath), o revive do player (30%) e a persistência do estado do companion dentro da run estável.

Hoje **não há nenhum código** de companion em combate/caverna (re-auditoria 0.5). Os contratos de elegibilidade/estado/save já existem parcialmente em `Assets/_Game/Scripts/Companions/**` (enums, bond state, availability resolver, job board) e devem ser **reutilizados**, não recriados.

## 2. Problema

Sem cave assist guardrails, o companion quebra o jogo de várias formas:

```text
companion vira party completa e o jogador deixa de jogar;
companion puxa sala/pack novo e estoura o budget de combate da run;
companion tanka o boss e ignora as mecânicas de boss gate;
healer cura infinito (imortalidade);
DPS do companion supera o do player;
companion morre/perde estado no reload (quebra cave-stable-run);
companion trava no mapa procedural (sem leash/safe-spawn fallback);
enemy AI ignora o active combat budget e o número de inimigos vira instável;
pet entra como combat companion junto (sistema separado, deferido);
ao perder o player com companion ativo, não há regra de revive (3.6) nem de retreat (3.5).
```

A decisão v3 (3.2–3.12) está aprovada e sem spec densa de runtime. Esta spec fecha a lacuna mantendo as invariantes do projeto (cave-stable-run, event bus, save DTOs simples, sem busca global).

## 3. Objetivo

Ao executar esta spec (WAVE 14), o jogo deve ter, **dentro do escopo de runtime de combate de caverna**:

```text
1. Entrada de caverna que permite SEM companion e com no máx. 1 ativo (CompanionCaveEntryState).
2. Brain de combate por papel (CompanionBrainProfile + CompanionBrainState) que honra
   comando (Ficar/Seguir/Atacar-marcado) + stance (Agressivo/Defensivo-default/Passivo) +
   modo Suporter (só papéis de suporte), com follow/leash/safe-spawn determinístico.
3. Ações de assistência (CompanionAssistAction) com custo de stamina/MP/cooldown e
   ActiveCombatBudgetCost, respeitando AllowedEnemyTypes/ForbiddenEnemyTypes/BossPolicy.
4. Balance: DPS-alvo 25% comum / 40% especialista (CompanionBalanceProfileSO), healer/buff
   com MP+cooldown+limite por combate, escalado pelo gear equipado (arma+acessório) SEM
   estourar o teto.
5. Regra de não-puxar-pack e de não-tankar/não-matar-boss (CompanionCombatBudgetAdapter):
   o companion CONTA no budget de combate da run e NÃO dispara spawn de pack novo.
6. Ciclo de derrota: Downed -> janela de resgate -> Retreat automático -> Injured 1-2 dias,
   sem permadeath em combate base (CompanionDownedInjuredState).
7. Revive do player: com companion ativo, ao player ser derrotado, 30% de tentar reviver;
   se falhar, companion escapa Injured 1 dia (sem garantir 100%).
8. Persistência do CompanionCaveRunState DENTRO da run estável (cave-stable-run): o estado do
   companion na run não rerrola layout/inimigos/recursos; só é resetado nos mesmos gatilhos que
   o CaveRunSeed (new game / KO-defeat / debug regen).
9. EditMode tests cobrindo entrada, cap de 1 ativo, follow/leash, não-puxar-pack, healer cooldown,
   DPS/budget cap, downed/injured, boss guardrail, revive 30% (probabilidade determinística com seed).
```

Tudo via `GameEventBus` (ADR-0007), com save DTO de tipos simples (ADR-0006), sem `GameObject.Find`/`FindObjectOfType` em runtime, e sem reescrever enemy AI, boss AI, combat formulas ou o snapshot procedural.

## 4. Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (BLOCO 3 — 3.2/3.3 stances+comandos+Suporter;
  3.4 equipment base OVERRIDE; 3.5 downed/injured; 3.6 revive 30%; 3.7 permadeath narrativo +
  Ressurreição Dolorosa; 3.8 fertilizante toggle; 3.10 DPS 25%/40%; 3.11 bond/CaveRank; 3.12 gated)
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md (§9-11 papéis; §24 não-trap/baú;
  §29-34 combate/balance/threat; §30 healer não-imortal; §31 guardian; §42-44 abilities;
  §56-59 data assets; EMENDA 2026-06-13-V3; override §45-46→3.4 equipment)
docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md (§1 stances/comandos/Suporter;
  PARTE A papéis de combate; §4-6 CompanionRoleProfileSO + bandas DPS + stance/Suporter por papel;
  PARTE D reconciliação contexto vs. papel funcional; PARTE F pendências WAVE 14)
docs/game_rules/combat_rules.md (modelo de dano; "Enemy AI Behavior" determinístico por CaveRunSeed)
docs/game_rules/cave_rules.md (cave-stable-run; regeneração só em new game / KO-defeat / debug)
docs/game_rules/fonte_rules.md (Rule 8 — companion sem permadeath base; Ressurreição Dolorosa)
docs/game_rules/event_rules.md (GameEventBus; DTO de evento simples; unsubscribe em OnDestroy)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
docs/decisions/ADR-0007-event-bus-gameplay-communication.md
.claude/skills/cave-stable-run-guard/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md (revive 30% e qualquer RNG do brain = seeded por run)
.claude/skills/system-reuse-audit/SKILL.md
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
.claude/rules/cave-stable-run.md
```

## 5. Estado atual do repo (grounding — file:line)

```text
EXISTE e REUTILIZAR (não recriar):
- Assets/_Game/Scripts/Companions/CompanionRole.cs
    enum [Flags] CompanionRole { None, FarmCompanion=1, CaveCompanion=2, QuestCompanion=4,
    SocialCompanion=8 } -> é CONTEXTO de elegibilidade ("onde atua"), NÃO papel funcional
    (catálogo PARTE D). enum InjuryState { Healthy, Injured, Incapacitated, Recovering } (linha 27-33).
    enum UnlockState (linha 15-25).
- Assets/_Game/Scripts/Companions/CompanionBondState.cs:5-17
    [Serializable] com CompanionId, BondLevel, TrustPoints, Fatigue, InjuryState,
    UnlockedRoleFlags, JobRank, CaveRank, LastInteractionDay -> LER bond/CaveRank/InjuryState daqui.
- Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs:6-19
    CanBeCaveCompanion (linha 11) -> gate de entrada na caverna; CanBeRomance/SpouseCompanion
    (13-14) = elegibilidade social, NÃO papel funcional (não tocar no enum por causa de romance).
- Assets/_Game/Scripts/Companions/CompanionFarmJobType.cs (9 jobs) -> fazenda, fora do escopo aqui.
- Assets/_Game/Scripts/Companions/CompanionJobBoardService.cs:18-103
    padrão de validação por stamina/horário/limite diário (JobExecutionResult) -> ESPELHAR o
    estilo de "validador com enum de resultado" para o CompanionCombatBudgetAdapter.
- Assets/_Game/Scripts/Companions/CompanionAvailabilityResolver.cs:5-84
    ResolveAvailability (gate por injury/fatigue/story/unlock) -> REUSAR como pré-condição de entrada.
- Assets/_Game/Scripts/Save/SaveData.cs:225-245
    CompanionManagerSaveData + CompanionSaveEntry (CompanionId, NpcId, UnlockState, UnlockedRoles,
    BondLevel, TrustPoints, Fatigue, InjuryState, LastInteractionDay, JobRank, CaveRank)
    -> estado base de companion já persiste; o provider de round-trip é da spec de eligibility.

Cave runtime (LER, NÃO regenerar — cave-stable-run / ADR-0005):
- Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs:259-267
    HandlePlayerDefeated() -> GenerateNewRunSeed("PlayerDefeated") + limpa snapshots e publica
    CavePlayerDefeatedEvent. CONFIRMA: defeat reseta a run -> o CompanionCaveRunState deve ser
    resetado nos MESMOS gatilhos (new game / KO-defeat / debug regen), nunca em Forward/BackExit.
- Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlan.cs:8-36
    CaveEnemySpawnPlan (CaveRunSeed, LevelSeed, LayoutHash, Entries[]) -> plano determinístico de
    inimigos. O companion NÃO altera esse plano nem dispara spawn novo (não-puxar-pack).
- Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs / CaveSnapshotService.cs
    -> snapshot estável da run; o companion NÃO escreve nele.
- Assets/_Game/Scripts/Cave/Runtime/CaveCheckpointService.cs, CaveBossGateService.cs,
    CaveSpawnAnchor.cs, CavePlayerPathConfinement.cs -> follow/leash/safe-spawn do companion
    consomem esses limites (audit Fase 0 para os pontos de extensão), nunca os reescrevem.

Enemy AI (LER, NÃO reescrever):
- Assets/_Game/Scripts/Enemy/EnemyBrain.cs:11 (class), 242/253/280/299/769
    já tem leash/retreat (LeashRange, ShouldRetreatAtLowHealth, inLeash). ESPELHAR o ESTILO de
    state machine + leash para o CompanionBrain; NÃO modificar o EnemyBrain.

Eventos existentes (CONSUMIR via GameEventBus — ADR-0007):
- Assets/_Game/Scripts/Core/Events/StatusAndDamageEvents.cs
    DamageAppliedEvent (DamageResult+TargetPosition), PlayerDamagedEvent (DamageAmount+pos+source),
    VulnerabilityWindowStarted/EndedEvent.
- Assets/_Game/Scripts/Core/Events/EnemyKilledEvent.cs (EnemyId, DropItemId, pos, XpReward).
- Assets/_Game/Scripts/Core/Events/CavePlayerDefeatedEvent.cs (player derrotado na caverna).
- Assets/_Game/Scripts/Player/Death/PlayerDeathController.cs:39 publica PlayerDiedEvent
    -> gatilho do revive 30% (3.6): companion ativo assina e tenta reviver ANTES da resolução final.
- Assets/_Game/Scripts/Core/Events/CaveSpec14Events.cs (boss gate / checkpoint / respawn events).

NÃO EXISTE (escopo CREATE_MINIMAL desta spec):
- CompanionCaveEntryState, CompanionBrainProfile, CompanionBrainState, CompanionAssistAction,
  CompanionCaveRunState, CompanionDownedInjuredState, CompanionCaveAssistService,
  CompanionCombatBudgetAdapter, CompanionBalanceProfileSO, e eventos de companion
  (CompanionDownedEvent, CompanionRetreatedEvent, CompanionAssistUsedEvent,
   CompanionRevivePlayerAttemptedEvent, CompanionInjuredEvent).

AUDITAR Fase 0 (registrar no report; nunca inventar sistema novo):
- ponto de extensão para "alvo marcado" pelo jogador (input/ID do inimigo focado) sem busca global;
- como o companion lê DPS esperado do jogador no estágio (de PLAYER_DERIVED_ATTRIBUTES / progressão)
  para calcular a banda 25%/40% — se não houver fonte determinística, marcar DpsExpectedSource como
  (proposta a calibrar) e usar um placeholder claro, nunca um número mágico;
- ids de gear (arma+acessório) no estado do companion (decisão 3.4) — origem na spec de eligibility;
- evento/flag de "boss presente" para BossPolicy (CaveBossGateService / boss spawner) — se não houver
  gatilho confiável, a parte boss-specific fica DEFERRED e documentada.
```

## 6. Engineering stories

```text
Como jogador, quero escolher 1 companion antes de descer OU descer sozinho, vendo papel/HP/MP/
  stamina/risco/restrições, para decidir conscientemente.
Como jogador, quero dar comandos simples (Ficar/Seguir/Atacar-marcado) e trocar stance
  (Agressivo/Defensivo/Passivo) com 1 tecla (tap/hold radial), para controlar o companion sem menus.
Como jogador de papel de suporte, quero ativar o modo Suporter para priorizar cura/buff dentro de
  MP/cooldown, sem virar 4ª stance universal.
Como companion, quero seguir/leash/retreat sem travar no mapa procedural (safe-spawn fallback),
  e nunca puxar pack novo nem abrir baú/trap por iniciativa.
Como sistema de combate, quero o companion útil mas NÃO dominante: DPS 25%/40%, healer não-infinito,
  sem tankar/matar boss, contando no active combat budget.
Como ciclo de derrota, quero Downed -> janela de resgate -> Retreat -> Injured 1-2 dias, sem
  permadeath em combate base.
Como jogador derrotado com companion ativo, quero 30% de chance de ser revivido pelo companion;
  se falhar, ele escapa Injured 1 dia.
Como save/load (cave-stable-run), quero o estado do companion preservado DENTRO da run e resetado
  só nos gatilhos canônicos (new game / KO-defeat / debug regen).
Como enemy AI, quero target priority compatível com o active combat budget (companion não cria
  inimigos extras nem altera o plano de spawn determinístico).
```

## 7. Escopo

```text
Inclui:
- CompanionCaveEntryState (state de entrada; cap 1; opcional; pré-condições via AvailabilityResolver);
- CompanionBrainProfile (config por papel: distances, retreat threshold, allowed/forbidden actions,
  cooldown/resource/hazard/leash rules, target priority) — alimentado por CompanionRoleProfileSO/catálogo;
- CompanionBrainState (Idle/FollowPlayer/ExploreFollow/CombatAssist/DefensiveAssist/HealingAssist/
  Retreat/Downed/InjuredUnavailable/QuestScripted) honrando comando+stance+Suporter;
- comandos (Ficar/Esperar, Seguir, Atacar alvo marcado) + stances (Agressivo<=12 / Defensivo<=4 DEFAULT /
  Passivo) + modo Suporter (só papéis de suporte);
- follow/leash/safe-spawn (warp curto SÓ como fallback técnico, lendo limites da cave runtime);
- CompanionAssistAction (LightAttack/Guard/Intercept/Heal/Buff/Debuff/Alert/LoreComment/ResearchHint)
  com StaminaCost/MpCost/Cooldown/TargetPolicy/AllowedEnemyTypes/ForbiddenEnemyTypes/BossPolicy/
  ActiveCombatBudgetCost;
- CompanionBalanceProfileSO (DpsContributionTargets 25%/40% + suporte; HealingCooldownRules;
  escala por gear arma+acessório SEM estourar teto);
- CompanionCombatBudgetAdapter (companion conta no budget; NÃO dispara spawn de pack; não tanka/mata boss);
- CompanionDownedInjuredState + ciclo Downed->resgate->Retreat->Injured 1-2 dias (sem permadeath base);
- revive do player (30%, RNG seeded por run) + fallback escape Injured 1 dia;
- CompanionCaveRunState (estado por run, persistido DENTRO da run estável; reset só nos gatilhos canônicos);
- eventos de companion via GameEventBus (CompanionDownedEvent, CompanionRetreatedEvent,
  CompanionAssistUsedEvent, CompanionRevivePlayerAttemptedEvent, CompanionInjuredEvent);
- EditMode tests determinísticos (seção 26).
```

## 8. Fora de escopo

```text
- enemy AI core rewrite (EnemyBrain.cs) e boss AI;
- combat formula tuning (dano/HP/MP/Stamina finais — PLAYER_DERIVED_ATTRIBUTES);
- geração/snapshot procedural da caverna (CaveSnapshotService/VisitedLevelSnapshot/CaveEnemySpawnPlan);
- pet cave runtime (sistema separado, deferido — STOP se aparecer);
- romance/casamento/spouse (flags de elegibilidade já existem; NÃO virar papel funcional);
- companion equipment DATA ASSETS e UI de equipar (slots arma+acessório são LIDOS aqui; criação é de outra spec);
- UI/HUD visual final de companion (consumidor é a spec de UI/HUD — esta spec só EXPÕE estado via eventos);
- party multi-companion na caverna (>1 ativo) e comandos táticos avançados / AI squad;
- o CompanionSaveSectionProvider de round-trip do estado base de companion (é da spec de eligibility/state/save);
- farm jobs / job board / fertilizante toggle (decisão 3.8 — spec de farm jobs);
- animações de arte, SFX, partículas.
```

## 9. Regras de não duplicação

```text
- REUTILIZAR CompanionBondState/CompanionSaveEntry para bond/CaveRank/InjuryState/gear ids;
  NÃO criar segundo estado de bond/injury.
- NÃO criar segundo enum de papel funcional confundido com o CompanionRole atual (que é CONTEXTO).
  Papel funcional vem de CompanionRoleProfileSO/catálogo (PARTE D); contexto continua no enum atual.
- NÃO recriar leash/retreat copiando o EnemyBrain por valor; ESPELHAR o ESTILO de state machine,
  mas o CompanionBrain é classe própria em Companions/Cave/**.
- NÃO criar canal de comunicação fora do GameEventBus (ADR-0007).
- NÃO escrever no snapshot/plano da run (cave-stable-run): o companion LÊ a run, não a regenera.
- NÃO duplicar o validador de stamina/horário do JobBoardService; o budget adapter é de combate,
  separado, mas segue o MESMO padrão (enum de resultado).
- Revive 30% e qualquer RNG do brain = RNG SEEDED por run (cave-stable-run / rng-and-determinism),
  nunca System.Random/timestamp/GUID.
```

## 10. Critérios de aceite

### CA-1 — Entrada: opcional e cap 1
- A caverna permite entrar SEM companion; com companion, no máximo 1 ativo; pré-condições
  (CanBeCaveCompanion, não Injured/Incapacitated/Exhausted, unlock/story) avaliadas via
  `CompanionAvailabilityResolver` antes de Ready.
- **Evidência:** EditMode tests (entrada sem companion; segundo companion rejeitado; bloqueio por
  injured/fatigue/story) + cenário humano de cave entry (deferido).

### CA-2 — Comandos + stances + Suporter
- Comandos Ficar/Seguir/Atacar-marcado funcionam; stances Agressivo (<=12 tiles), Defensivo
  (DEFAULT, <=4 tiles ou quem ataca o player), Passivo (não ataca) respeitadas pelo brain;
  Suporter exposto **só** por Healer/Alchemist/MusicianSupport (catálogo §6); "Atacar alvo
  marcado" é override temporário da stance.
- **Evidência:** EditMode tests (cada stance escolhe/recusa alvo conforme raio; Passivo não ataca;
  Suporter indisponível em papel não-suporte; comando override) + cenário humano.

### CA-3 — Follow / leash / safe-spawn sem travar e sem puxar pack
- Companion segue respeitando FollowDistance/leash; usa warp curto só como fallback técnico ao
  exceder leash/ficar preso; NÃO dispara spawn de pack novo nem altera o `CaveEnemySpawnPlan`;
  não abre baú/porta nem aciona trap por iniciativa.
- **Evidência:** EditMode tests (leash exceeded -> retreat/warp fallback; ação de ataque NÃO
  incrementa contagem de inimigos/pack) + checklist + cenário humano.

### CA-4 — Balance: DPS 25%/40%, healer não-infinito, gear escala sem estourar
- Contribuição de DPS do companion fica na banda-alvo 25% (comum) / 40% (especialista) de
  `CompanionBalanceProfileSO.DpsContributionTargets`, com suporte/fazenda abaixo; cura/buff têm
  MP+cooldown+limite por combate (sem imortalidade); gear (arma+acessório) escala o DPS-alvo
  SEM ultrapassar o limite amplo (15-35% / 35-50%).
- **Evidência:** EditMode tests (DPS dentro da banda; healer bloqueado sem MP/cooldown; gear sobe
  DPS mas clamp no teto) + matriz de balance.

### CA-5 — Boss guardrails + combat budget
- O companion NÃO tanka boss (sem aggro permanente), NÃO mata boss sozinho, NÃO ignora mecânicas
  de boss gate; conta no active combat budget via `CompanionCombatBudgetAdapter`; sem boss trigger
  confiável (Fase 0), a parte boss-specific fica DEFERRED e documentada.
- **Evidência:** EditMode tests (BossPolicy nega tank/solo; budget cost contabilizado) + checklist.

### CA-6 — Downed / resgate / Injured (sem permadeath base)
- Derrota do companion: para de assistir -> entra Downed -> abre janela de resgate (player pode
  reerguer) -> senão Retreat automático -> ao fim da run vira InjuredUnavailable por 1-2 dias;
  NUNCA permadeath em combate procedural (3.5/3.7); não faz o player perder progressão principal.
- **Evidência:** EditMode tests (transições Downed->resgate->Retreat->Injured; Injured 1-2 dias;
  sem permadeath) + cross-ref fonte_rules Rule 8.

### CA-7 — Revive do player (30%) com fallback
- Com companion ativo, ao `PlayerDiedEvent`/derrota, o companion tenta reviver o player com **30%**
  de chance (RNG seeded por run); se falhar, o companion escapa e volta Injured 1 dia; nunca garante
  100% no sistema base.
- **Evidência:** EditMode tests (probabilidade determinística com seed fixo: sucesso e falha;
  falha -> Injured 1 dia; evento CompanionRevivePlayerAttemptedEvent publicado).

### CA-8 — Persistência dentro da run estável (cave-stable-run)
- `CompanionCaveRunState` persiste DENTRO da run (HP/MP/stamina/fatigue/downed/injured/leash/
  cooldowns/assist history) sem rerrolar layout/inimigos/recursos; é resetado só nos gatilhos
  canônicos (new game / KO-defeat via `CaveRunManager.HandlePlayerDefeated` / debug regen) e nunca
  em Forward/BackExit; DTO de tipos simples + IDs (ADR-0006), sem refs Unity. Se o round-trip exigir
  alterar o schema do `SaveManager`/migration, **STOP** (é da spec de eligibility/state/save).
- **Evidência:** EditMode tests (estado estável entre revisitas; reset em defeat; round-trip do DTO
  sem refs Unity) + declaração de save (§18).

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/Cave/CompanionCaveEntryState.cs          (NOVO — enum/contrato de entrada)
Assets/_Game/Scripts/Companions/Cave/CompanionBrainState.cs              (NOVO — enum de brain states)
Assets/_Game/Scripts/Companions/Cave/CompanionStanceAndCommand.cs        (NOVO — enums Stance/Command + SupporterMode)
Assets/_Game/Scripts/Companions/Cave/CompanionBrainProfile.cs            (NOVO — config por papel; pure C#)
Assets/_Game/Scripts/Companions/Cave/CompanionAssistAction.cs            (NOVO — ação + custos + policies)
Assets/_Game/Scripts/Companions/Cave/CompanionCaveRunState.cs            (NOVO — estado por run; [Serializable] simples)
Assets/_Game/Scripts/Companions/Cave/CompanionDownedInjuredState.cs      (NOVO — ciclo downed/injured)
Assets/_Game/Scripts/Companions/Cave/CompanionBalanceProfileSO.cs        (NOVO — ScriptableObject de balance; DpsContributionTargets)
Assets/_Game/Scripts/Companions/Cave/CompanionCaveAssistService.cs       (NOVO — brain runtime; assina/decide; pure-ish + MonoBehaviour bridge)
Assets/_Game/Scripts/Companions/Cave/CompanionCombatBudgetAdapter.cs     (NOVO — validador de budget; enum de resultado, estilo JobBoardService)
Assets/_Game/Scripts/Companions/Cave/CompanionRevivePlayerPolicy.cs      (NOVO — regra 30% seeded; pure C#)
Assets/_Game/Scripts/Core/Events/CompanionCaveEvents.cs                  (NOVO — eventos de companion via GameEventBus)
Assets/_Game/Scripts/Editor/Validation/ValidateCompanionCaveAssistBalance.cs (NOVO — validador editor de bandas/policies)
Assets/_Game/Tests/EditMode/Companions/CompanionCaveAssistBrainBalanceTests.cs (NOVO — testes determinísticos)
docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md (report)
```

> Consolidar com qualquer arquivo existente que a Fase 0 revele equivalente (improvável — re-auditoria diz zero código). Lógica de decisão deve ficar em **pure C#** sempre que possível (testável em EditMode); a casca MonoBehaviour só assina eventos, lê refs serializadas/bootstrap e delega.

## 12. Contratos

### 12.1 Data contracts (DTO/SO — tipos simples + IDs, ADR-0006)

```text
CompanionCaveEntryState (enum): NoCompanion, SelectedAvailable, SelectedUnavailable,
  SelectedInjured, SelectedExhausted, SelectedStoryBlocked, Ready.

CompanionBrainState (enum): Idle, FollowPlayer, ExploreFollow, CombatAssist, DefensiveAssist,
  HealingAssist, Retreat, Downed, InjuredUnavailable, QuestScripted.

CompanionStance (enum): Aggressive, Defensive (DEFAULT), Passive.
CompanionCommand (enum): StayWait, Follow, AttackMarkedTarget.
SupporterMode: bool (válido só se o papel ExposesSupporterMode).

CompanionBrainProfile (pure C# / serializável): CompanionBrainProfileId, PrimaryRole (string id),
  SecondaryRole (string id), FollowDistance (float tiles), CombatDistance (float tiles),
  RetreatThreshold (float 0-1 HP), DefaultStance (CompanionStance), ExposesSupporterMode (bool),
  AssistPriority (int[] ordenado), TargetPriority (string[] enemy type ids),
  AllowedActions (string[] ActionId), ForbiddenActions (string[]),
  CooldownRules / ResourceRules / HazardAvoidanceRules (listas de structs simples),
  LeashRules (float leashRangeTiles + warp fallback flag), ReviveOrRetreatRules
  (revivePlayerChance float = 0.30; injuredDaysOnFail int = 1; injuredDaysOnDowned int = 1-2).

CompanionAssistAction (pure C# / serializável): ActionId (string), ActionType (enum: LightAttack,
  Guard, Intercept, Heal, Buff, Debuff, Alert, LoreComment, ResearchHint), RequiredRole (string id),
  StaminaCost (int), MpCost (int), Cooldown (float s), TargetPolicy (enum),
  AllowedEnemyTypes (string[]), ForbiddenEnemyTypes (string[]), BossPolicy (enum: Forbidden,
  LimitedAssist, AllowedNonTank), ActiveCombatBudgetCost (int).

CompanionCaveRunState ([Serializable], tipos simples + IDs): RunId (string = CaveRunSeed-derived),
  CompanionId (string), CurrentBrainState (int enum), CurrentStance (int enum),
  CurrentCommand (int enum), SupporterMode (bool), Hp/Mp/Stamina/Fatigue (int),
  Downed (bool), Injured (bool), InjuredDaysRemaining (int), LastSafePosition (Vector2 ou x/y floats),
  LeashState (int enum), CooldownStates (List<float> ou List<CooldownEntry>),
  AssistHistory (List<string> actionIds). SEM ScriptableObject/GameObject/MonoBehaviour/Sprite.

CompanionBalanceProfileSO (ScriptableObject): DpsContributionTargets { commonPct=0.25,
  specialistPct=0.40, supportPctMax<commonPct } ; DpsHardLimits { commonMin=0.15, commonMax=0.35,
  specialistMin=0.35, specialistMax=0.50 } ; HealingCooldownRules { mpCost, cooldownS,
  maxUsesPerCombat, maxUsesPerRun, rangeTiles, castTimeS, interruptible } ;
  GearScalingRules (como arma+acessório escalam o DPS-alvo, com clamp no hard limit).
  TODOS os números são (proposta a calibrar) salvo os ancorados em decisão 3.10 (25%/40%) e
  3.6 (revive 30%).
```

### 12.2 Runtime contracts

```text
CompanionCaveAssistService: assina eventos de combate/cave (DamageAppliedEvent, PlayerDamagedEvent,
  EnemyKilledEvent, CavePlayerDefeatedEvent, PlayerDiedEvent) e decide brain state/ação respeitando
  comando+stance+Suporter+leash+budget; publica eventos de companion. SEM busca global (refs por
  bootstrap/serializadas). Unsubscribe em OnDestroy/OnDisable.
CompanionCombatBudgetAdapter: ValidateAssist(action, context) -> enum result (Allowed,
  BudgetExceeded, BossForbidden, EnemyTypeForbidden, OnCooldown, NoResource, WouldPullPack);
  ContributesToBudget(companion) -> int. Estilo do CompanionJobBoardService.ValidateJobExecution.
CompanionRevivePlayerPolicy: TryRevive(seed, chance=0.30) -> bool determinístico (seed da run);
  on fail -> companion Injured 1 dia.
CompanionDownedInjuredState (lógica): Downed -> RescueWindow(open/close) -> Retreat -> Injured(1-2d).
```

### 12.3 Event contracts (GameEventBus — ADR-0007 / event_rules.md)

```text
ADICIONA (DTOs simples, sem refs Unity; sufixo *Event; unsubscribe obrigatório):
  CompanionDownedEvent { CompanionId, CaveLevel, RescueWindowSeconds }
  CompanionRetreatedEvent { CompanionId, Reason }
  CompanionAssistUsedEvent { CompanionId, ActionId, TargetEnemyId, BudgetCost }
  CompanionRevivePlayerAttemptedEvent { CompanionId, Success, Chance }
  CompanionInjuredEvent { CompanionId, InjuredDays }
CONSOME (sem alterar): DamageAppliedEvent, PlayerDamagedEvent, EnemyKilledEvent,
  CavePlayerDefeatedEvent, PlayerDiedEvent, e (se Fase 0 confirmar) o gatilho de boss presente.
NÃO altera eventos existentes.
```

### 12.4 Save / UI contracts

```text
Save: CompanionCaveRunState é DTO simples (ADR-0006), persistido DENTRO da run estável; reset só
  nos gatilhos canônicos (cave-stable-run). O round-trip do estado BASE de companion (provider) é
  da spec de eligibility/state/save — se este runtime exigir migration do SaveManager, STOP.
UI: esta spec NÃO desenha HUD; apenas EXPÕE estado via eventos para a spec de UI/HUD consumir.
```

## 13. Sistemas afetados

```text
Companion (nova camada Cave/** — brain/assist/balance/budget/downed/revive);
Cave runtime (LEITURA: run seed, snapshot, plano de spawn, checkpoints/boss gates/safe spawn — não modificados);
Combat (CONSOME eventos de dano/kill; não altera formulas nem enemy AI);
Event bus (eventos novos de companion);
Save (DTO de run-state do companion — sem migration do SaveManager nesta spec).
NÃO tocados: EnemyBrain, boss AI, combat formulas, CaveSnapshotService/VisitedLevelSnapshot/CaveEnemySpawnPlan,
  Pet, romance/spouse, companion equipment data assets, companion HUD.
```

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/Cave/**            (novos contratos/runtime)
Assets/_Game/Scripts/Core/Events/CompanionCaveEvents.cs (eventos novos)
Assets/_Game/Scripts/Editor/Validation/ValidateCompanionCaveAssistBalance.cs (validador editor)
Assets/_Game/Tests/EditMode/Companions/**          (testes)
docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor) se necessário
```

## 15. Arquivos proibidos

```text
Packages/** ; ProjectSettings/**
Assets/**/*.unity ; Assets/**/*.prefab ; Assets/**/*.asset (YAML manual)
Assets/_Game/Scripts/Pets/**                       (pet runtime — STOP se necessário)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs e enemy AI core (apenas LER)
Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs / VisitedLevelSnapshot.cs /
  CaveEnemySpawnPlan*.cs (snapshot/plano — apenas LER; NÃO regenerar)
Assets/_Game/Scripts/Save/SaveManager.cs / SaveData.cs (migration/provider — é de outra spec; STOP se preciso)
Assets/_Game/Scripts/Companions/CompanionRole.cs (NÃO adicionar Romance/Spouse/papel funcional ao enum)
docs/specs/SPEC_EXECUTION_ORDER.md ; docs/specs/implementados/** ; docs/refinements/implementados/**
docs/project/CURRENT_STATE.md ; PROJECT_LOG.md ; índices compartilhados (SPEC_REGISTRY, fable_00C,
  GAME_RULES_INDEX, DECISION_LOG, .specs)
Companion HUD / consumo dos eventos de companion (é da spec de UI/HUD)
```

## 16. Estratégia de implementação por fases

```md
### Fase 0 — Audit (sem editar runtime)
- Rodar a busca da seção 23B; classificar achados (EXISTING_CANONICAL / EXISTING_PARTIAL /
  MISSING_SAFE_TO_CREATE / MISSING_BUT_DEFER / CONFLICT) no report.
- Confirmar: zero código de companion combate/caverna; reuso de CompanionBondState/CompanionSaveEntry/
  AvailabilityResolver; estilo de EnemyBrain (leash/retreat) e JobBoardService (validador enum).
- Resolver os 4 pontos de Fase 0 da §5 (alvo marcado; fonte de DPS esperado; ids de gear; trigger de boss).
- Decidir REUSE/HARDEN/CREATE_MINIMAL/DEFER e justificar.
### Fase 1 — Contratos de dados (pure C#/SO): enums (entry/brain/stance/command), CompanionBrainProfile,
  CompanionAssistAction, CompanionCaveRunState, CompanionDownedInjuredState, CompanionBalanceProfileSO.
  Testes de shape/defaults (Defensivo default; Suporter só papéis de suporte; 25%/40%; revive 30%).
### Fase 2 — Eventos de companion (CompanionCaveEvents.cs) + testes de shape/DTO simples.
### Fase 3 — Entrada (CompanionCaveEntryState + AvailabilityResolver reuse): cap 1, opcional, pré-condições. Testes.
### Fase 4 — Brain runtime (CompanionCaveAssistService): comando+stance+Suporter; follow/leash/safe-spawn;
  assina/consome eventos; sem busca global; unsubscribe. Testes de seleção de alvo por stance/raio.
### Fase 5 — Balance + budget (CompanionBalanceProfileSO + CompanionCombatBudgetAdapter): DPS 25%/40%,
  healer cooldown, gear scaling com clamp, não-puxar-pack, boss guardrails. Testes.
### Fase 6 — Downed/Injured + revive 30% (CompanionDownedInjuredState + CompanionRevivePlayerPolicy):
  ciclo completo; RNG seeded por run; sem permadeath. Testes determinísticos (sucesso/falha).
### Fase 7 — CompanionCaveRunState persistência dentro da run (reset só nos gatilhos canônicos);
  validador editor de bandas/policies; csproj; run_strict_validation; execution report.
```

## 17. Paralelização

```text
- Parallelizable: NO.
- Parallel group: WAVE_14_COMPANIONS_FUTURE.
- Must not run with: combat formula changes; enemy AI rewrite; cave procedural snapshot changes;
  companion eligibility/state/save foundation (dependência — deve vir antes); companion equipment
  data/UI; companion HUD/UI; pet runtime.
- Shared systems requiring lock: GameEventBus (eventos novos), pasta Companions/Cave/** exclusiva.
- Reason: cave companion toca combate/cave balance e o estado por run; precisa de lock exclusivo e
  da fundação de eligibility/state/save resolvida primeiro.
```

## 18. Impacto em save/load

```text
Does this change save schema (SaveManager/SaveData.cs)? SHOULD BE NO. O estado BASE de companion já
  existe (CompanionSaveEntry, SaveData.cs:225-245) e o provider de round-trip é da spec de
  eligibility/state/save. Se ESTE runtime exigir alterar SaveData/SaveManager ou migration -> STOP.
Does this add a save section? NO (o CompanionCaveRunState vive DENTRO do estado da run estável;
  se exigir nova seção persistida no SaveManager -> STOP até a spec de save).
Does this require migration? NO (STOP se sim).
Does this persist Unity references? NO — DTO de tipos simples + IDs (ADR-0006).
Cave-stable-run: o CompanionCaveRunState NÃO rerrola layout/inimigos/recursos; reset só em
  new game / KO-defeat (CaveRunManager.HandlePlayerDefeated) / debug regen; nunca em Forward/BackExit.
```

## 19. Impacto em eventos

```text
Adds events: YES — CompanionDownedEvent, CompanionRetreatedEvent, CompanionAssistUsedEvent,
  CompanionRevivePlayerAttemptedEvent, CompanionInjuredEvent (DTOs simples, GameEventBus).
Changes existing events: NO.
Requires unsubscribe pattern: YES — CompanionCaveAssistService e qualquer MonoBehaviour subscriber
  desinscreve em OnDestroy/OnDisable (event_rules.md / ADR-0007).
```

## 20. Impacto em UI/Unity

```text
Changes UI: NO final (esta spec só EXPÕE estado via eventos; HUD é da spec de UI/HUD).
Changes scenes: NO. Changes prefabs: NO.
Changes ScriptableObjects/assets: cria a CLASSE CompanionBalanceProfileSO (código .cs); a CRIAÇÃO do
  asset .asset em si é wiring humano/gerador autorizado — NÃO editar YAML manualmente (ADR-0008).
Requires Play Mode final validation: YES — fluxo de cave companion (entrada, follow, assist, downed,
  revive, retreat) — DEFERRED_TO_FINAL_VALIDATION (FINAL_HUMAN_VALIDATION_BY_WAVE.md).
```

## 21. Riscos técnicos

```text
Risco: companion domina o combate (DPS > player / healer infinito).
  Mitigação: DpsContributionTargets 25%/40% + hard limits; HealingCooldownRules (MP+cooldown+max
  por combate/run); EditMode tests de banda e cooldown.
Risco: companion puxa pack / altera o plano de spawn determinístico (quebra cave-stable-run).
  Mitigação: CompanionCombatBudgetAdapter nega WouldPullPack; companion NUNCA escreve no
  CaveEnemySpawnPlan/snapshot; teste de "ataque não incrementa contagem de inimigos".
Risco: companion trava no mapa procedural.
  Mitigação: LeashRules + safe-spawn warp curto SÓ como fallback técnico (lendo CaveSpawnAnchor/
  confinement), nunca teleporte livre; teste de leash-exceeded.
Risco: revive 30% não-determinístico quebra replay da run.
  Mitigação: RNG SEEDED por run (rng-and-determinism); teste com seed fixo (sucesso e falha).
Risco: permadeath acidental em combate base.
  Mitigação: CompanionDownedInjuredState nunca aplica morte permanente; permadeath só em evento
  narrativo (3.7); cross-ref fonte_rules Rule 8; teste de "downed -> injured, nunca dead".
Risco: estado do companion perdido no reload da run.
  Mitigação: CompanionCaveRunState DTO simples; reset só nos gatilhos canônicos; teste de revisita.
Risco: tentação de reescrever EnemyBrain/snapshot para "facilitar" o brain do companion.
  Mitigação: arquivos proibidos (§15); anti-regressão (§28); apenas LER esses sistemas.
Risco: trigger de boss inexistente -> inventar evento de boss.
  Mitigação: parte boss-specific DEFERRED e documentada (CA-5); nunca inventar evento de boss.
```

## 22. Rollback

```text
Remover a pasta Companions/Cave/**, o CompanionCaveEvents.cs e o validador editor desliga toda a
camada de cave assist; nenhum sistema preexistente é afetado (estado base de companion, cave runtime,
enemy AI, combat e save permanecem intactos). A spec de eligibility/state/save e as specs de farm
jobs / UI-HUD continuam válidas; a UI/HUD apenas deixa de receber os eventos de companion (volta a
pendente até a 14 cave assist entrar). Nenhum save existente é migrado por esta spec, então o
rollback não exige downgrade de schema.
```

---

# /speckit.tasks

## 23. Tasks

```md
- [ ] T001 — Fase 0: audit (busca §23B), classificar achados, resolver os 4 pontos de Fase 0 (§5),
        decidir REUSE/HARDEN/CREATE_MINIMAL/DEFER, registrar no report.
- [ ] T002 — Contratos de dados: enums (entry/brain/stance/command), CompanionBrainProfile,
        CompanionAssistAction, CompanionCaveRunState, CompanionDownedInjuredState,
        CompanionBalanceProfileSO + testes de shape/defaults (Defensivo default; Suporter só suporte;
        25%/40%; revive 30%).
- [ ] T003 — Eventos de companion (CompanionCaveEvents.cs) + testes de DTO simples/shape.
- [ ] T004 — Entrada (CompanionCaveEntryState + reuse AvailabilityResolver): cap 1, opcional,
        pré-condições + testes (sem companion; 2º rejeitado; injured/fatigue/story bloqueiam).
- [ ] T005 — Brain runtime (CompanionCaveAssistService): comando+stance+Suporter; follow/leash/
        safe-spawn; assina/consome eventos; sem busca global; unsubscribe + testes de alvo por stance/raio.
- [ ] T006 — Balance + budget (CompanionBalanceProfileSO + CompanionCombatBudgetAdapter): DPS 25%/40%,
        healer cooldown, gear scaling com clamp, não-puxar-pack, boss guardrails + testes.
- [ ] T007 — Downed/Injured + revive 30% (CompanionDownedInjuredState + CompanionRevivePlayerPolicy):
        ciclo completo, RNG seeded, sem permadeath + testes determinísticos (sucesso/falha).
- [ ] T008 — CompanionCaveRunState persistência dentro da run (reset nos gatilhos canônicos);
        validador editor; csproj; run_strict_validation; execution report.
```

## 24. Validações obrigatórias (PowerShell)

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "Assembly-CSharp FAIL"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "Assembly-CSharp-Editor FAIL"; exit 1 }
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { Write-Host "strict validation FAIL"; exit 1 }
```

Busca local mínima (audit Fase 0 — adaptar para PowerShell `Select-String -Recurse` se `rg` indisponível):

```powershell
# (Windows: usar rg se presente; senão Get-ChildItem ... | Select-String)
rg -n "CompanionBrain|CaveCompanion|CompanionAssist|FollowPlayer|CombatAssist|HealingAssist|Leash|Downed|ActiveCombatBudget|Boss|CompanionBalanceProfile|CompanionCaveRunState" Assets/_Game/Scripts docs/design docs/specs
rg -n "Companion|CompanionRole|CompanionBondState|CompanionSaveEntry|InjuryState|CaveRank|Romance|Spouse|Pet" Assets/_Game/Scripts
```

Unity compile (quando houver alteração Unity C# — sequencial, um log; NÃO paralelo):

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

## 25. Testing Quality Gate

```text
Changed runtime code: YES (quando executada na WAVE 14).
Changed deterministic logic: YES — entry eligibility, stance/raio target selection, DPS band,
  healer cooldown, budget adapter, downed/injured transitions, revive 30% (seeded), run-state round-trip.
Changed Unity scene/prefab/asset wiring: NO (a criação do .asset de CompanionBalanceProfileSO é
  wiring humano/gerador autorizado; a CLASSE é código).
Automated tests added/updated: YES (EditMode — seção 26).
Automated tests command: Unity Test Runner EditMode (ou equivalente local do repo); arquivos só em
  Assets/_Game/Tests/EditMode/Companions/**.
Manual Play Mode scenario: YES, DEFERRED_TO_FINAL_VALIDATION — fluxo de cave companion
  (entrada/follow/assist/downed/revive/retreat); registrar em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
Justification if no automated tests: N/A (lógica determinística testável).
Residual risk: feel de combate/assist e tuning numérico (25%/40%, cooldowns, leash, janela de resgate)
  só validáveis em Play Mode; parte boss-specific DEFERRED se Fase 0 não achar trigger de boss confiável.
```

## 26. EditMode tests obrigatórios (determinísticos)

```text
- Entry: entrada sem companion = NoCompanion->Ready; 2º companion ativo rejeitado (cap 1);
  injured/incapacitated/fatigue>80/story bloqueiam (via CompanionAvailabilityResolver).
- Stances: Agressivo ataca alvo a <=12 tiles e recusa a >12; Defensivo (default) só ataca a <=4 tiles
  ou quem atacou o player; Passivo não ataca; "Atacar alvo marcado" sobrepõe a stance temporariamente.
- Suporter: exposto só para Healer/Alchemist/MusicianSupport; recusado para Fighter/Guardian/Scout/etc.
- Follow/leash: leash excedido -> Retreat/warp fallback; ação de ataque NÃO incrementa contagem de
  inimigos/pack (não-puxar-pack); não abre baú/trap por iniciativa.
- Balance: DPS na banda 25% (comum) / 40% (especialista); suporte abaixo; gear sobe DPS mas clamp no
  hard limit (15-35% / 35-50%); healer bloqueado sem MP/cooldown e ao exceder maxUsesPerCombat.
- Budget/boss: CompanionCombatBudgetAdapter contabiliza budget; BossPolicy nega tank/solo de boss;
  EnemyTypeForbidden/OnCooldown/NoResource retornam o enum correto.
- Downed/Injured: Downed para de assistir -> janela de resgate -> Retreat -> Injured 1-2 dias;
  nunca dead/permadeath em combate base.
- Revive 30%: seed fixo produz sucesso/falha determinísticos; falha -> Injured 1 dia; evento
  CompanionRevivePlayerAttemptedEvent publicado com Success/Chance corretos.
- Run-state: CompanionCaveRunState estável entre revisitas (mesmo CaveRunSeed); resetado em defeat;
  DTO round-trip sem refs Unity (só simples + ids).
- Eventos: shape/defaults dos 5 eventos novos; sem refs Unity; sufixo *Event.
```

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos (§15).
Contratos de dados, eventos, brain, balance, budget, downed/injured, revive e run-state implementados
  apenas dentro do escopo (Companions/Cave/** + Core/Events/CompanionCaveEvents.cs + validador editor + testes).
Decisões v3 refletidas: comandos+stances+Suporter (3.2/3.3), equipment escala o DPS (3.4),
  downed/resgate/Injured (3.5), revive 30% (3.6), sem permadeath base (3.7), DPS 25%/40% (3.10),
  bond/CaveRank trilhas separadas (3.11), gated WAVE 14 (3.12).
Invariantes mantidas: GameEventBus (ADR-0007), save DTO simples (ADR-0006), cave-stable-run (ADR-0005),
  sem GameObject.Find/FindObjectOfType em runtime.
EditMode tests (§26) PASS; builds Assembly-CSharp e Assembly-CSharp-Editor exit 0; docs validation PASS;
  run_strict_validation exit 0.
Execution report criado em docs/validation/14_..._execution_report.md com Spec Compliance Matrix,
  existing-systems audit, decisão REUSE/CREATE_MINIMAL e Testing Quality Gate.
Sem claim ACCEPTED / PLAYMODE_VALIDATED por compile; máximo BUILD_VALIDATED com Play Mode DEFERRED.
```

## 28. Anti-regressão

```text
- NÃO reescrever EnemyBrain.cs / enemy AI core / boss AI / combat formulas.
- NÃO regenerar ou escrever no snapshot/plano da run (CaveSnapshotService / VisitedLevelSnapshot /
  CaveEnemySpawnPlan): cave-stable-run preservado; companion só LÊ a run.
- NÃO alterar SaveData.cs/SaveManager.cs nem fazer migration (é da spec de eligibility/state/save).
- NÃO adicionar Romance/Spouse nem papel funcional ao enum CompanionRole (contexto != papel; PARTE D).
- NÃO implementar Pet runtime / pet save / pet HUD / pet data (STOP).
- NÃO criar canal de comunicação fora do GameEventBus; sempre unsubscribe.
- NÃO tornar o companion obrigatório para terminar a main quest; caverna deve permitir entrar sem companion.
- NÃO deixar o companion jogar pelo jogador: DPS 25%/40%, healer não-infinito, sem tankar/matar boss,
  sem puxar pack, sem gerar loot/recurso extra.
- NÃO permadeath em combate base (só evento narrativo — 3.7); downed -> Injured.
- Revive e qualquer RNG do brain = seeded por run; nunca System.Random/timestamp/GUID.
- NÃO usar GameObject.Find/FindObjectOfType em runtime (refs por bootstrap/serializadas).
- NÃO editar índices compartilhados (SPEC_EXECUTION_ORDER, SPEC_REGISTRY, CURRENT_STATE, fable_00C,
  GAME_RULES_INDEX, DECISION_LOG, .specs) — o orquestrador cuida.
- NÃO executar runtime em massa antes da liberação da WAVE 14 ou exceção humana explícita (3.12).
```

## 29. Execution Report mínimo (template)

```md
# Execution Report — 14_spec_companion_cave_assist_brain_balance_future_runtime
## Summary: Spec / Branch / Executor / Date / Final status
## Sources read: (lista da §4)
## Local audit: comandos (§23B) + achados classificados (EXISTING_CANONICAL/PARTIAL/MISSING_SAFE_TO_CREATE/MISSING_BUT_DEFER/CONFLICT)
## Implementation decision: REUSE / HARDEN / CREATE_MINIMAL / DEFER + justificativa
## Spec Compliance Matrix: CA-1..CA-8 -> arquivo:linha + teste
## Companion compliance: ajuda mas não joga pelo player; sem pet; sem romance/spouse; sem Breath/Fôlego;
   active combat budget respeitado; boss guardrails; sem permadeath base; revive 30%; save/cave-stable safe
## Files changed
## Validation: docs / Assembly-CSharp / Assembly-CSharp-Editor / Unity compile / EditMode / Play Mode (deferred)
## Testing Quality Gate (bloco da §25)
## Residual risks
## Next specs impacted: UI/HUD companion; farm jobs; combat balance validation
```

## 30. Stop Conditions

Parar e registrar `BLOCKED` (ou o status preciso do `spec_quality_gate`) se:

```text
1. Exigir alterar Packages/ ou ProjectSettings/.
2. Exigir scene/prefab/asset YAML manual fora do escopo (ADR-0008).
3. Exigir save migration ou alterar SaveData/SaveManager (é da spec de eligibility/state/save).
4. Exigir criar pet runtime/save/HUD/data.
5. Exigir criar romance/casamento/spouse ou adicionar isso ao enum de papel.
6. Tornar o companion obrigatório para terminar a main quest.
7. Permitir companion tankar boss / curar infinito / matar boss sozinho / farmar inimigos sem player ativo.
8. Permitir companion gerar loot/recurso/economia infinita.
9. Adicionar Breath/Fôlego como recurso de companion.
10. Tratar classes de D&D como papéis mecânicos de companion.
11. Exigir reescrever EnemyBrain / combat formulas / snapshot procedural para o brain funcionar.
12. A spec de eligibility/recruitment/state/save (dependência) ainda não estar resolvida
    (resolver a dependência same-wave antes — spec_dependency_resolution).
13. Não for possível decidir se um sistema existente é canônico ou obsoleto.
```

---

## 31. Notas para execução posterior

Esta spec é SPEC-READY mas **gated na WAVE 14** (decisão 3.12). Deve ser executada apenas quando City/NPC, Farm, Cave, Combat, Save, UI e Quest estiverem estáveis, depois da spec de eligibility/recruitment/state/save (dependência), ou quando houver decisão humana explícita de antecipar Companions. Mantém o status future/SPEC-READY e permanece em `docs/specs/a_implementar/features_futuras/` até a liberação.

---

*Reescrita densa (SpecKit) em 2026-06-13 — integra a EMENDA 2026-06-13-V3 (decisões 3.2–3.12) e o COMPANION_ROLES_CATALOG_v1.0 no corpo da spec. Fontes canônicas: FABLE_DECISOES_RESPOSTAS_v3.0 (BLOCO 3), COMPANIONS_DIRECTION (EMENDA V3), COMPANION_ROLES_CATALOG, game_rules (combat/cave/fonte/event), ADR-0005/ADR-0007, e código vivo em Assets/_Game/Scripts/{Companions,Cave,Enemy,Combat,Core/Events,Save}/**. Números sem fonte = (proposta a calibrar). Docs-only: nenhum código de runtime alterado.*
