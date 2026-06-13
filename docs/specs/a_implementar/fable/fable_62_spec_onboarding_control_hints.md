# SPEC — Onboarding: Hints Contextuais de Controles + Tutorial de Combate + Aviso da Caverna

> **Spec ID:** `fable_62_spec_onboarding_control_hints`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P1 (bloqueia MVP — jogador novo não descobre os controles sozinho)
> **Type:** Runtime / UI / Integration
> **Domain:** UI / Onboarding
> **Parallelizable:** CONDITIONAL (lock do HUD canvas WI-23 e da aba Sistema F56)
> **Parallel group:** fable_batch11_shipping
> **Can run with:** F61, F63 (coordenar: intro também usa 1º DayStarted), F64, F66, F67
> **Must not run with:** F65 (ambas adicionam widgets no GameplayHudCanvas — mesma superfície), F56 (tela Controles entra na aba Sistema — coordenar ordem)
> **Repo lock scope:** GameplayHudCanvas/feedback views (WI-23), aba Sistema (F56), seção de save aditiva HintsSeen
> **Depends on:**
> - WI-23 (existente — GameplayHudCanvasController/GameplayFeedbackService/FeedbackToastHudView)
> - F56 (E56 — aba Sistema p/ a tela "Controles"; CONDICIONAL: hints rodam sem ela; a tela
>   estática entra quando a aba existir)
> - F67 (E67 — docs/game_rules/input_map.md como fonte canônica dos textos de tecla)
> **Blocks:** aceitação do MVP por jogador novo (jornada do zero)
> **Scope:** OnboardingHintService (hints de primeira ocorrência por contexto, flags persistidas) + tela estática "Controles" na aba Sistema.
> **Out of scope:** tutorial modal/bloqueante, quest tutorial, rebind de teclas, localização, vídeo/imagens, hints de sistemas ainda não shipados.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md, ADR-0006-save-data-contracts-simple-dtos.md]
required_game_rules: [ui_modal_rules.md, save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

Auditoria da jornada do jogador: NENHUMA das 60 specs ensina os controles. O input real
do jogo (verificado no código) é denso: WASD/setas movem (PlayerMovementActionInput);
E interage OU ataca com a mão direita — com hold para heavy/charged
(PlayerAttackController decide por InteractionCandidate presente); Q usa a mão esquerda;
Space é dodge e LeftShift é block (PlayerMovementActionInput); I inventário, K
equipamento, U skill tree, Esc pause/fecha modal (GameplayInputRouter); L equipment, J
quests, C calendário (F20), M minimapa (F38), Tab painel único (F14), 1-4 hotbar,
R/T/Y/G slots de skill ativa. ~138 usos de KeyCode em 34 arquivos — e zero superfície
no jogo que conte isso ao jogador.

A infraestrutura de feedback JÁ existe (WI-23): GameplayFeedbackService enfileira
mensagens com prioridade e publica HudFeedbackUpdatedEvent consumido por
FeedbackToastHudView no GameplayHudCanvas; ContextHintController já renderiza
"[E] {prompt}" via InteractionPromptChangedEvent. O padrão de persistência aditiva
existe (ISaveSectionProvider, precedente HotbarSectionProvider).

Decisão de design vinculante da direção de UX do projeto: onboarding NUNCA bloqueia
gameplay — nada de tutorial modal; hints são toasts/painel pequeno, na primeira
ocorrência de cada contexto, uma vez por save.

## Problema

Um jogador novo abre o jogo e não sabe andar, interagir, atacar, desviar nem abrir o
inventário. A caverna — conteúdo de risco com perda de itens na morte — é alcançável sem
nenhum aviso de perigo nem instrução de dodge/block. Sem flags persistidas, qualquer
hint reaparece a cada sessão (ruído). Sem uma tela de referência, quem esquecer uma
tecla não tem onde consultar (o input map F67 é doc de governança, não superfície de
jogo).

## Objetivo

Ao final desta spec: (1) OnboardingHintService (wired via bootstrap) dispara hints de
primeira ocorrência — mover (1º frame de gameplay), interagir (1º InteractionCandidate),
atacar/hold (1ª arma equipada), aviso de perigo + dodge/block (1ª entrada na caverna),
status sofrido (1º status no player) — pelos toasts/painel do HUD existente, cada um
exatamente 1× por save; (2) flags HintsSeen persistidas em seção aditiva simples;
(3) tela estática "Controles" na aba Sistema (F56) listando o input map canônico (F67);
(4) zero bloqueio de gameplay em qualquer ponto.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs (fila/prioridade de toasts)
Assets/_Game/Scripts/UI/HUD/Views/FeedbackToastHudView.cs (render no canvas WI-23)
Assets/_Game/Scripts/UI/Notification/ContextHintController.cs ("[E] {prompt}" existente)
Assets/_Game/Scripts/UI/Input/GameplayInputRouter.cs (I/K/U/Esc canônicos)
Assets/_Game/Scripts/Player/Movement/PlayerMovementActionInput.cs (WASD/Space/Shift)
Assets/_Game/Scripts/Combat/PlayerAttackController.cs (E ataque × interação; hold)
Assets/_Game/Scripts/Save/ISaveSectionProvider.cs + Providers/HotbarSectionProvider.cs
docs/game_rules/input_map.md (E67 — fonte canônica das teclas; se ainda não existir,
  validar teclas direto no código e registrar a dependência)
.claude/rules/testing-quality-gate.md
.claude/skills/event-bus-pattern/SKILL.md
.claude/skills/save-section-provider/SKILL.md
.claude/skills/bootstrap-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- GameplayFeedbackService + FeedbackToastHudView (WI-23) — canal único de toast;
- ContextHintController ("[E] {prompt}" contextual — NÃO é hint de onboarding; coexiste);
- InteractionPromptChangedEvent (hasCandidate, prompt);
- CaveLevelEnteredEvent (gatilho do aviso da caverna);
- eventos de equipamento (1ª arma) e de status no player — auditar nomes exatos Fase 0;
- ISaveSectionProvider/HotbarSectionProvider (precedente de seção aditiva);
- GameEventBus (única via de comunicação).
Não existe:
- qualquer hint de onboarding; flags HintsSeen; tela Controles.
Auditar Fase 0:
- nomes exatos dos eventos de "arma equipada" e "status aplicado no player"
  (EquipmentChanged?/StatusEffectApplied? — mapear no código);
- "1º frame de gameplay": melhor gatilho disponível (evento de cena pronta/bootstrap
  completo — escolher o existente, não criar evento novo sem necessidade);
- API da aba Sistema (F56) p/ registrar a tela Controles (se F56 pendente, a tela fica
  gated e documentada como entrega parcial até E56);
- onde a seção HintsSeen pluga (SaveManager seções v5 / provider pattern).
```

## Engineering stories

```text
Como jogador novo, quero um toast me dizendo como andar e interagir nos primeiros
  segundos, para não abandonar o jogo sem descobrir o básico.
Como jogador entrando na caverna pela 1ª vez, quero um aviso de perigo com dodge/block,
  para saber que ali se morre e como me defender ANTES de apanhar.
Como jogador veterano, quero que cada hint apareça uma única vez por save, para o HUD
  não virar ruído.
Como jogador que esqueceu uma tecla, quero a tela Controles na aba Sistema, para
  consultar sem sair do jogo.
```

## Escopo

```text
Inclui:
- OnboardingHintService (MonoBehaviour wired via bootstrap — padrão do projeto):
  - tabela estática de hints {hintId, gatilho, texto PT-BR, prioridade}:
    hint_move        — 1º frame de gameplay → "WASD/setas para mover";
    hint_interact    — 1º InteractionPromptChangedEvent com hasCandidate → "E interage";
    hint_attack      — 1ª arma equipada → "E ataca (segure p/ heavy); Q usa a mão esquerda";
    hint_cave_danger — 1ª CaveLevelEnteredEvent → aviso de perigo (morte derruba itens)
                       + "Space desvia · Shift bloqueia" (prioridade Important);
    hint_status      — 1º status sofrido pelo player → "Status ativos aparecem no HUD";
  - dispara cada hint EXATAMENTE 1× por save (checa flag antes; marca depois);
  - entrega via GameEventBus → canal de toast existente (NotificationToastRequestedEvent/
    PlayerActionFeedbackEvent — escolher o canal auditado na Fase 0; um só);
  - nunca bloqueia gameplay; nunca abre modal;
- persistência: OnboardingHintsSaveData (List<string> de hintIds vistos — só tipos
  simples) em seção aditiva (provider pattern — precedente HotbarSectionProvider);
  saves antigos carregam com lista vazia (todos os hints elegíveis de novo — seguro);
- tela "Controles" (estática, read-only) na aba Sistema (F56): lista do input map
  canônico (movimento/combate/defesa/painéis/hotbar/slots) — conteúdo espelhando
  docs/game_rules/input_map.md (F67); navegável por teclado, Esc fecha (padrão F14);
- EditMode tests: primeira-ocorrência (2º gatilho não re-dispara), round-trip da seção
  HintsSeen, save legado (lista vazia → elegível), tabela de hints íntegra (ids únicos,
  textos não-vazios), gatilho da caverna marcado Important.
```

## Fora de escopo

```text
Não inclui:
- tutorial modal/bloqueante de qualquer tipo (decisão vinculante);
- quest tutorial guiada (F63 cobre a motivação narrativa);
- rebind de teclas / detecção de gamepad;
- localização (PT-BR only, como o resto do projeto);
- hints para sistemas não shipados (festivais, romance, etc.);
- mudar o ContextHintController existente (coexiste; superfícies distintas).
```

## Regras de não duplicação

```text
Não criar segundo canal de toast — reusar a fila do GameplayFeedbackService (WI-23).
Não duplicar o ContextHintController — hint de onboarding (1×/save) ≠ prompt contextual
(sempre que há candidato); superfícies separadas e documentadas.
Não criar segunda fonte de verdade de teclas — textos espelham input_map.md (F67);
divergência = bug de doc, reportar.
Não criar segundo padrão de persistência — provider/section aditiva existente.
```

## Critérios de aceite

### CA-1 Hints de primeira ocorrência

- Cada um dos 5 hints dispara na PRIMEIRA ocorrência do seu contexto e nunca mais
  (mesmo save), via toast do HUD existente, sem bloquear gameplay.
- Evidência: EditMode tests de primeira-ocorrência por hint + cenário humano do lote.

### CA-2 Aviso da caverna com defesa

- A 1ª entrada na caverna mostra aviso de perigo (perda de itens na morte) + instrução
  de dodge (Space) e block (Shift), com prioridade Important (fura a fila de toasts).
- Evidência: EditMode test do gatilho/prioridade + cenário humano.

### CA-3 Flags persistidas

- HintsSeen sobrevive a save/load (hint visto não reaparece após reload); save legado
  sem a seção carrega com defaults seguros (lista vazia).
- Evidência: EditMode tests de round-trip + seção ausente.

### CA-4 Tela Controles na aba Sistema

- A aba Sistema (F56) ganha a entrada "Controles" com o input map canônico, read-only,
  navegável por teclado; conteúdo espelha input_map.md (F67).
- Evidência: cenário humano do lote; SE F56/F67 pendentes, gated + débito documentado
  no report (hints CA-1..3 não dependem disso).

### CA-5 Zero bloqueio

- Nenhum hint pausa, abre modal ou rouba input; movimento contínuo durante toasts.
- Evidência: diff sem ModalManager nos hints + cenário humano.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Onboarding/
  OnboardingHintService.cs       (NOVO — gatilhos, tabela de hints, 1×/save)
  OnboardingHintCatalog.cs       (NOVO — tabela estática {id, texto, prioridade})
Assets/_Game/Scripts/Save/Providers/
  OnboardingHintsSectionProvider.cs (NOVO — seção aditiva HintsSeen)
Assets/_Game/Scripts/UI/System/
  ControlsReferenceScreen.cs     (NOVO — tela estática na aba Sistema F56; gated se E56 pendente)
GameSaveData                     (campo aditivo OnboardingHints — default seguro)
Assets/_Game/Tests/EditMode/UI/OnboardingHintTests.cs (NOVO)
docs/validation/fable_62_spec_onboarding_control_hints_execution_report.md
```

## Contratos

### Data contracts

- `OnboardingHintsSaveData`: `List<string> SeenHintIds` — somente tipos simples.
- `OnboardingHintCatalog`: entradas imutáveis {hintId (string estável), texto PT-BR,
  FeedbackMessagePriority}.

### Runtime contracts

- `OnboardingHintService.TryShowHint(hintId)`: ponto único — checa SeenHintIds, publica
  no canal de toast, marca e persiste; idempotente.
- Gatilhos por assinatura de eventos existentes (InteractionPromptChangedEvent,
  CaveLevelEnteredEvent, evento de equip, evento de status — nomes auditados Fase 0);
  unsubscribe em OnDisable.
- Lógica de primeira-ocorrência extraída em classe pura testável (sem MonoBehaviour).

### Event contracts

- Consome eventos existentes; publica APENAS no canal de toast existente. Nenhum evento
  novo (se a Fase 0 provar necessidade, OnboardingHintShownEvent documentado).

### Save contracts

- Seção aditiva OnboardingHints (provider pattern); sem migração (default = lista
  vazia); zero referências Unity; saves antigos intactos.

### UI contracts

- Toasts pela fila WI-23 (prioridade Normal; caverna Important).
- Tela Controles: read-only, padrão de navegação F14/F56 (teclado, Esc fecha), sem
  estado próprio além de scroll/foco.

## Sistemas afetados

```text
HUD canvas WI-23 (consumo do canal de toast — sem mudança estrutural)
Save (seção aditiva nova)
Aba Sistema F56 (entrada Controles — aditiva, gated)
Event bus (assinaturas novas; zero eventos novos)
Bootstrap (wiring do serviço)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Onboarding/** (novos)
Assets/_Game/Scripts/Save/Providers/OnboardingHintsSectionProvider.cs (novo)
GameSaveData (campo aditivo — arquivo auditado na Fase 0)
Assets/_Game/Scripts/UI/System/ControlsReferenceScreen.cs (novo; registro na aba F56 —
  diff aditivo no arquivo de registro auditado)
ponto de bootstrap (diff aditivo de wiring — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/UI/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (YAML manual)
Packages/** ; ProjectSettings/**
GameplayFeedbackService/FeedbackToastHudView internals (consumir, não reescrever)
ContextHintController (coexiste intacto)
SaveManager core (apenas seção aditiva via provider)
GameplayInputRouter/PlayerMovementActionInput/PlayerAttackController (ler, não mudar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Nomes exatos dos eventos de equip/status; melhor gatilho de "1º frame de gameplay";
canal único de toast; API da aba F56; ponto da seção de save; existência do
input_map.md (F67).

### Fase 1 — Serviço e persistência
OnboardingHintCatalog + OnboardingHintService (lógica pura extraída) + seção aditiva
HintsSeen + testes (primeira-ocorrência, round-trip, legado).

### Fase 2 — Gatilhos
Assinaturas dos 5 gatilhos + prioridade Important da caverna + testes de gatilho.

### Fase 3 — Tela Controles e fechamento
ControlsReferenceScreen na aba Sistema (gated se E56 pendente, com débito documentado);
wiring bootstrap; csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch11_shipping.
- Can run with: F61, F63 (coordenar 1º DayStarted/boot), F64, F66, F67.
- Must not run with: F65 (mesma superfície GameplayHudCanvas), F56 (aba Sistema).
- Shared files/systems that require lock: GameplayHudCanvas/feedback (WI-23), aba
  Sistema (F56), GameSaveData (campo aditivo).
- Reason: adiciona consumidores e uma entrada de aba em superfícies compartilhadas de
  UI; colisão de merge com F65/F56 é provável se simultâneo.

## Impacto em save/load

```text
Does this change save schema? YES (campo aditivo OnboardingHints)
Does this add a save section? YES (aditiva, provider pattern, default lista vazia)
Does this require migration? NO (default seguro; legado = todos os hints elegíveis)
Does this persist Unity references? NO (List<string> apenas)
```

## Impacto em eventos

```text
Adds events: NO (preferência forte; consumo apenas)
Changes existing events: NO
Requires unsubscribe pattern: YES (serviço assina 4-5 eventos; OnDisable obrigatório)
```

## Impacto em UI/Unity

```text
Changes UI: YES — toasts (canal existente) + tela Controles na aba Sistema
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final — jornada do jogador novo)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: hint virar ruído (reaparecer todo load / spam de fila).
Mitigação: flag persistida checada ANTES de publicar + teste de primeira-ocorrência.

Risco: aviso da caverna se perder atrás de outros toasts.
Mitigação: prioridade Important (fura a fila — comportamento já existente do serviço
WI-23) + teste.

Risco: gatilho de "1º frame" disparar em tela de título/loading.
Mitigação: Fase 0 escolhe gatilho pós-bootstrap de gameplay; cenário humano confere.

Risco: textos de tecla divergirem do código (tecla muda, hint mente).
Mitigação: fonte canônica input_map.md (F67) + teste de integridade da tabela; regra:
mudou tecla → atualiza input_map → atualiza catálogo.

Risco: F56/F67 pendentes travarem a spec inteira.
Mitigação: dependência CONDICIONAL — hints (núcleo P1) rodam sozinhos; tela Controles
gated com débito documentado.

Risco: colisão com F63 (intro também age no 1º dia).
Mitigação: hints não dependem de DayStarted (gatilhos próprios); coordenar ordem de
toasts no cenário humano.
```

## Rollback

```text
Remover o serviço + provider + tela (gated). Campo aditivo de save é inofensivo vazio.
Canal de toast e ContextHintController intactos. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar eventos de equip/status, gatilho de 1º frame, canal de
        toast, API da aba F56, ponto da seção de save.
- [ ] T002 — Catálogo + serviço (lógica pura testável) + seção aditiva HintsSeen +
        testes (primeira-ocorrência/round-trip/legado).
- [ ] T003 — 5 gatilhos assinados + prioridade Important da caverna + testes.
- [ ] T004 — Tela Controles na aba Sistema (gated se E56 pendente) + wiring bootstrap;
        csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (primeira-ocorrência, persistência de flags,
  integridade do catálogo)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — jornada do
  jogador novo: boot → mover → interagir → equipar → caverna, cada hint 1×)
- Requires regression test: YES (toasts existentes e ContextHintController intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com os 5 hints
  aparecendo 1× e não reaparecendo após reload

## Definition of Done

```text
5 hints de primeira ocorrência funcionais via canal de toast WI-23 (caverna com
Important), flags HintsSeen persistidas em seção aditiva com defaults seguros, tela
Controles na aba Sistema (ou gated com débito documentado), zero bloqueio de gameplay,
zero evento novo, builds 0E, run_strict_validation exit 0, execution report criado.
```

## Anti-regressão

```text
Fila de toasts WI-23 sem mudança estrutural (consumidor aditivo apenas).
ContextHintController intacto ("[E] {prompt}" continua igual).
Saves antigos carregam com defaults seguros (seção ausente = lista vazia).
Nenhum modal/bloqueio adicionado ao fluxo de gameplay.
Eventos só via GameEventBus; unsubscribe pareado; zero GameObject.Find em runtime.
Input real (GameplayInputRouter/PlayerMovementActionInput/PlayerAttackController)
inalterado — esta spec só DESCREVE as teclas, não as muda.
```
