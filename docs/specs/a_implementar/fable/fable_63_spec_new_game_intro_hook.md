# SPEC — Narrativa: Intro do New Game + Ponte para a Main Quest

> **Spec ID:** `fable_63_spec_new_game_intro_hook`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P2
> **Type:** Runtime / Narrative / Integration
> **Domain:** Quests / UI / Narrative
> **Parallelizable:** CONDITIONAL (lock da cadeia quest e do fluxo New Game F56)
> **Parallel group:** fable_batch11_shipping
> **Can run with:** F61, F64, F65, F67
> **Must not run with:** F10, F34, F35, F36 (cadeia quest — mq_act1_00 precede a cadeia E40), F56 (fluxo New Game/título — mesma superfície), F62 (coordenar ordem de mensagens no 1º dia)
> **Repo lock scope:** fluxo New Game (F56), QuestRegistry/QuestManager (quest nova mq_act1_00), gerador da FarmScene (carta na cama), DayStarted handlers
> **Depends on:**
> - F10 (E40 — cadeia mq_act1_01..05 com oferta do Corvus na cidade; CONDICIONAL: a carta
>   funciona apontando para o Corvus mesmo antes da cadeia existir)
> - F34 (E38 — infraestrutura de fontes de quest; mq_act1_00 usa source Main)
> - F56 (E56 — New Game/título; a intro pluga no New Game; CONDICIONAL: sem E56, a intro
>   roda no primeiro boot de save novo)
> - REFINAMENTO DE LORE PENDENTE (Nymirianos/Cindar) — textos finais; usar
>   PLACEHOLDER_LORE provisório (gate explícito, ver Riscos)
> **Blocks:** aceitação da jornada "New Game → main quest" no MVP
> **Scope:** sequência de abertura programática (3-5 telas de texto, skip com Esc) + carta na cama no 1º dia + quest mq_act1_00 auto-ofertada no primeiro DayStarted (idempotente).
> **Out of scope:** cutscene animada/arte, voice, lore final (refinamento pendente), conteúdo da cadeia mq_act1_01+ (F10), qualquer ato 2+.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [ui_modal_rules.md, save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

Auditoria da jornada: o New Game é um cold open na fazenda — o jogador aparece num campo
sem uma linha de contexto sobre quem é, onde está ou o que fazer. A main quest existe
como plano (F10: cadeia mq_act1_01..05, ofertada por Corvus NA CIDADE via nó OfferQuest
no TownNpcDialogueLibrary), mas nada leva o jogador até o Corvus: quem nunca for à
cidade nunca encontra a main quest. Não há nenhum gancho narrativo entre "acordei na
fazenda" e "procure o templo".

O repo tem as peças: QuestRegistry/QuestManager (aceite/progresso/recompensa/save),
QuestFlagService (flags persistidas), DayStartedEvent (gatilho de 1º dia), gerador
CreateMvpFarmScene (interactables na fazenda via editor script — padrão para a carta na
cama), e F34 (E38) define QuestSource.Main. A F56 define o fluxo New Game — o ponto
natural de disparo da intro.

Restrição vinculante: o TEXTO FINAL da abertura depende do refinamento de lore pendente
(Nymirianos/Cindar). Esta spec implementa a ESTRUTURA com textos provisórios marcados
`PLACEHOLDER_LORE` e lista a dependência — trocar texto depois é diff trivial; a
estrutura não pode esperar a lore.

## Problema

Sem intro, o jogo não diz ao jogador o mínimo ("você chegou a Cindar's Hope"); sem a
carta/quest-ponte, a main quest é inalcançável por design para quem não explorar a
cidade por conta própria — um softlock de motivação no primeiro minuto de jogo. E
qualquer solução ingênua (ofertar quest a cada DayStarted) duplicaria a oferta em
reload: a idempotência por flag salva é requisito, não detalhe.

## Objetivo

Ao final desta spec: (1) New Game dispara uma sequência de abertura programática — 3 a 5
telas de texto (chegada a Cindar's Hope), navegação por tecla, skip com Esc, exibida
exatamente 1× por save; (2) no primeiro DayStarted do save, a quest `mq_act1_00`
("Procure Corvus na cidade", source Main) é auto-ofertada/aceita exatamente 1×
(idempotente por flag persistida) e uma carta interactable aparece na cama da fazenda
reforçando o chamado; (3) concluir mq_act1_00 (falar com Corvus) conecta na cadeia E40
quando ela existir (prerequisite), com textos PLACEHOLDER_LORE documentados.

## Fontes obrigatórias lidas

```text
docs/specs/a_implementar/fable/fable_10_spec_main_quest_act1_playable_runtime.md
  (IDs mq_act1_01..05, Corvus/Thalindra/Maelor, PrerequisiteQuestIds, OfferQuest)
docs/specs/a_implementar/fable/fable_34_spec_quest_sources_infrastructure.md
  (QuestSource.Main; fluxo único de aceite)
docs/specs/a_implementar/fable/fable_56_spec_system_tab_title_flow.md (New Game/reset)
Assets/_Game/Scripts/Quests/** (QuestRegistry/QuestManager/QuestFlagService — shape real)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (padrão de interactable)
.claude/rules/testing-quality-gate.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/ui-modal-stack/SKILL.md
.claude/skills/spec-execution/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- QuestRegistry/QuestManager (fluxo único de quest fixa — mq_act1_00 entra nele);
- QuestFlagService (flag de idempotência da oferta);
- DayStartedEvent (gatilho do 1º dia);
- CreateMvpFarmScene (gerador — a carta entra por ele, nunca YAML manual);
- ModalManager/ModalBase + DialogueModal (padrões de tela com input bloqueado);
- IInteractable/InteractionSystem (carta = interactable padrão).
Não existe:
- qualquer intro/abertura; carta na cama; mq_act1_00; gancho New Game → narrativa.
Auditar Fase 0:
- ponto de disparo da intro: New Game da F56 (E56) OU detecção de save novo no boot
  (fallback sem E56) — decidir pelo que existe na execução;
- shape de QuestDefinition real (campos p/ quest TalkToNpc + auto-offer);
- a cama existe como objeto na FarmScene gerada? (âncora da carta — senão posição fixa
  do gerador);
- ordem 1º dia: intro → toast de hints (F62) → carta — documentar a sequência p/ não
  empilhar telas.
```

## Engineering stories

```text
Como jogador novo, quero 30 segundos de contexto ao iniciar (quem sou, onde cheguei),
  para o mundo não começar mudo.
Como jogador, quero uma carta na minha cama e uma quest clara me mandando à cidade,
  para saber qual é o primeiro passo.
Como jogador apressado/veterano, quero pular a intro com Esc, para New Game não ter
  fricção em runs repetidas.
Como cadeia da main quest (F10), quero mq_act1_00 como prerequisite limpo, para a
  oferta do Corvus assumir do ponto certo.
```

## Escopo

```text
Inclui:
- IntroSequenceController (programático, padrão de construção F14/F56 — sem YAML):
  - 3-5 telas de texto PT-BR (PLACEHOLDER_LORE marcado em cada string provisória);
  - avanço por tecla (E/Enter), skip total com Esc; input de gameplay bloqueado durante
    (padrão modal); ao terminar/skipar, gameplay libera;
  - exibida exatamente 1× por save (flag IntroSeen persistida — mesma família de flags
    do QuestFlagService ou seção aditiva simples; auditar Fase 0);
  - disparo: fluxo New Game (E56) OU primeiro boot de save novo (fallback);
- quest mq_act1_00 "Cartas de Cindar's Hope" (ID estável; source Main quando E38
  existir; senão quest fixa com tag documentada):
  - objetivo TalkToNpc Corvus; recompensa mínima (flag de conclusão — sem ouro/XP
    significativo: é quest-ponte);
  - auto-ofertada/aceita no PRIMEIRO DayStartedEvent do save, exatamente 1×
    (flag persistida mq_act1_00_offered checada antes — idempotente em reload);
  - registrada como PrerequisiteQuestId da mq_act1_01 (diff aditivo na cadeia E40
    quando existir; senão documentado p/ F10 consumir);
- carta na cama: interactable LetterInteractable na FarmScene VIA CreateMvpFarmScene
  (gerador — nunca YAML manual): interagir mostra o texto da carta ("Procure Corvus na
  cidade" — PLACEHOLDER_LORE) e marca a quest como vista no log; a carta some (ou fica
  lida) após a 1ª leitura — estado persistido simples;
- EditMode tests: idempotência da oferta (2º DayStarted/reload não duplica), flag
  IntroSeen (round-trip; save legado = intro NÃO re-exibida em save em progresso —
  default seguro auditado), skip state machine (Esc de qualquer tela encerra), textos
  PLACEHOLDER_LORE presentes e ids estáveis.
```

## Fora de escopo

```text
Não inclui:
- texto final de lore (refinamento Nymirianos/Cindar pendente — gate documentado);
- arte/cutscene/música da intro (telas de texto programáticas);
- conteúdo da cadeia mq_act1_01..05 (F10/E40);
- diálogo completo do Corvus (F10 — aqui só o alvo TalkToNpc);
- intro re-assistível por menu (decisão futura junto com extras).
```

## Regras de não duplicação

```text
Não criar segundo fluxo de quest — mq_act1_00 entra no QuestRegistry/QuestManager
existente (aceite/progresso/save iguais a qualquer quest).
Não criar segundo sistema de flags — QuestFlagService/seção existente (Fase 0 decide).
Não criar segundo padrão de modal/tela — padrão F14/ModalManager existente.
Carta = IInteractable padrão via gerador de cena (nunca YAML manual, nunca Find).
Não duplicar o reset de New Game (F56 é dona do reset; a intro só ESCUTA o fluxo).
```

## Critérios de aceite

### CA-1 Intro 1× com skip

- New Game (ou 1º boot de save novo) mostra a sequência de 3-5 telas; Esc pula tudo;
  termina/skipa → gameplay libera; nunca re-exibe no mesmo save (inclusive após reload).
- Evidência: EditMode tests (state machine + flag round-trip) + cenário humano.

### CA-2 Oferta idempotente da mq_act1_00

- O primeiro DayStarted do save oferta/aceita mq_act1_00 exatamente 1×; reload + novo
  DayStarted não duplica; a quest aparece no log apontando Corvus.
- Evidência: EditMode test de idempotência com flag persistida + save/load.

### CA-3 Carta na cama funcional

- A carta existe na FarmScene (via gerador), interagir mostra o texto e o estado lido
  persiste; nenhuma edição manual de YAML no diff.
- Evidência: diff do gerador + evidência de regeneração da cena + cenário humano.

### CA-4 Ponte para a cadeia E40

- Concluir mq_act1_00 (TalkToNpc Corvus) seta a flag/prerequisite que a mq_act1_01
  consome (ou documenta a integração p/ F10 se E40 pendente).
- Evidência: EditMode test da conclusão/flag + nota de integração no report.

### CA-5 PLACEHOLDER_LORE rastreável

- Todo texto provisório está marcado PLACEHOLDER_LORE e o report lista a dependência do
  refinamento de lore com os pontos de troca.
- Evidência: grep PLACEHOLDER_LORE no diff + seção do report.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Narrative/
  IntroSequenceController.cs    (NOVO — telas programáticas, skip Esc, 1×/save)
  IntroSequenceModel.cs         (NOVO — state machine pura testável + textos PLACEHOLDER_LORE)
Assets/_Game/Scripts/Quests/
  MainQuestHookService.cs       (NOVO — auto-oferta mq_act1_00 no 1º DayStarted, idempotente)
Assets/_Game/Scripts/World/
  LetterInteractable.cs         (NOVO — carta na cama; IInteractable padrão)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (diff aditivo — carta)
registro da quest mq_act1_00   (no registry de quests existente — diff aditivo)
flags persistidas              (IntroSeen, mq_act1_00_offered, letter_read — família existente)
Assets/_Game/Tests/EditMode/Narrative/IntroHookTests.cs (NOVO)
docs/validation/fable_63_spec_new_game_intro_hook_execution_report.md
```

## Contratos

### Data contracts

- `mq_act1_00` (ID estável, nunca renomear): TalkToNpc Corvus; source Main (E38) ou
  tag equivalente; recompensa = flag.
- Textos da intro: array imutável de strings PT-BR marcadas PLACEHOLDER_LORE.

### Runtime contracts

- `IntroSequenceModel`: state machine pura {tela atual, Advance(), Skip(), Finished} —
  testável sem Unity.
- `MainQuestHookService`: em DayStartedEvent → se flag ausente → oferta/aceita
  mq_act1_00 + seta flag; ponto único; unsubscribe pareado.
- `LetterInteractable`: IInteractable padrão; estado lido persistido como flag simples.

### Event contracts

- Consome: DayStartedEvent; eventos de quest existentes (QuestAcceptedEvent etc. já
  publicados pelo fluxo). Nenhum evento novo obrigatório (se necessário,
  IntroSequenceFinishedEvent documentado). Tudo via GameEventBus.

### Save contracts

- Flags simples persistidas (IntroSeen, mq_act1_00_offered, letter_read) na família de
  flags existente — sem seção nova, sem migração, sem referências Unity.
- Default seguro p/ save legado em progresso: intro tratada como vista (não interromper
  um save no meio com tela de abertura) — regra explícita testada.

### UI contracts

- Intro bloqueia input de gameplay enquanto ativa (padrão modal do projeto); Esc pula;
  nenhuma tela além das 3-5 da sequência; carta usa o prompt padrão "[E]".

## Sistemas afetados

```text
Quest (registro de mq_act1_00 + hook de oferta)
Fluxo New Game/boot (disparo da intro — superfície F56)
FarmScene via gerador (carta)
Flags persistidas (3 novas)
UI modal/input routing (intro bloqueia gameplay)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Narrative/** (novos)
Assets/_Game/Scripts/Quests/MainQuestHookService.cs (novo) + registro da quest (aditivo)
Assets/_Game/Scripts/World/LetterInteractable.cs (novo)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (diff aditivo da carta)
ponto de disparo New Game/boot (diff mínimo — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/Narrative/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por YAML manual (cena SÓ via gerador + regeneração)
Packages/** ; ProjectSettings/**
Conteúdo da cadeia mq_act1_01..05 (F10)
TownNpcDialogueLibrary além do mínimo TalkToNpc alvo (diálogo rico é F10)
SaveManager core (apenas flags na família existente)
Reset de New Game (F56 é dona — a intro só escuta)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Ponto de disparo (E56 × boot de save novo); shape de QuestDefinition; âncora da cama no
gerador; família de flags; ordem de mensagens do 1º dia com F62.

### Fase 1 — Intro
IntroSequenceModel (pura) + IntroSequenceController + flag IntroSeen + default seguro
p/ save legado + testes (state machine/skip/round-trip).

### Fase 2 — Quest-ponte
mq_act1_00 registrada + MainQuestHookService (1º DayStarted, idempotente) + flag +
testes de idempotência com reload.

### Fase 3 — Carta e fechamento
LetterInteractable + diff do gerador + regeneração com evidência + prerequisite p/ E40
documentado; csproj; run_strict_validation; execution report (PLACEHOLDER_LORE listado).
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch11_shipping.
- Can run with: F61, F64, F65, F67.
- Must not run with: F10/F34/F35/F36 (cadeia quest), F56 (New Game), F62 (coordenar
  1º dia).
- Shared files/systems that require lock: QuestRegistry/QuestManager, fluxo New Game,
  CreateMvpFarmScene, DayStarted handlers.
- Reason: toca a cadeia de quest mais sensível e o fluxo de boot — ambas superfícies
  com donos em specs vizinhas.

## Impacto em save/load

```text
Does this change save schema? NO (flags na família existente)
Does this add a save section? NO
Does this require migration? NO (defaults seguros; legado = intro vista)
Does this persist Unity references? NO (flags string/bool)
```

## Impacto em eventos

```text
Adds events: NO (preferência; IntroSequenceFinishedEvent só se necessário, documentado)
Changes existing events: NO
Requires unsubscribe pattern: YES (hook assina DayStartedEvent; OnDisable pareado)
```

## Impacto em UI/Unity

```text
Changes UI: YES — sequência de intro programática (padrão F14/modal)
Changes scenes: via gerador (carta na FarmScene) — regeneração com evidência
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: oferta da quest duplicar em reload (pior bug de quest possível).
Mitigação: flag persistida checada ANTES da oferta + teste de idempotência com
save/load; reuso do fluxo único de aceite.

Risco: intro re-exibir em save legado em progresso.
Mitigação: default seguro (legado = vista) + teste explícito.

Risco: lore pendente travar a spec.
Mitigação: gate explícito — estrutura com PLACEHOLDER_LORE; troca de texto é diff
trivial pós-refinamento; report lista cada ponto de troca.

Risco: empilhamento de telas/toasts no 1º dia (intro + hints F62 + carta).
Mitigação: ordem documentada (intro fecha → hints → carta é pull, não push); cenário
humano valida a sequência.

Risco: E56/E38/E40 pendentes na execução.
Mitigação: dependências CONDICIONAIS com fallbacks definidos (boot de save novo; quest
fixa com tag; prerequisite documentado p/ F10) — nada bloqueia o núcleo.

Risco: carta órfã se a cena for regenerada sem o diff do gerador.
Mitigação: carta SÓ existe via gerador (nunca à mão); evidência de regeneração.
```

## Rollback

```text
Remover controller/serviço/carta + registro da quest; flags persistidas órfãs são
inofensivas. Regenerar FarmScene sem o diff da carta. Cadeia E40 e New Game E56
intactos. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: disparo (E56 × boot), shape de quest, âncora da cama, família de
        flags, ordem do 1º dia com F62.
- [ ] T002 — IntroSequenceModel/Controller + IntroSeen + default legado + testes.
- [ ] T003 — mq_act1_00 + MainQuestHookService idempotente + testes com reload.
- [ ] T004 — LetterInteractable + gerador + regeneração com evidência + prerequisite
        E40 documentado; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (idempotência da oferta, state machine da intro,
  flags persistidas, default legado)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — New Game →
  intro → skip/replay → carta → quest no log → Corvus)
- Requires regression test: YES (quests existentes e DayStarted handlers intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano da jornada completa
  New Game → mq_act1_00 → conclusão no Corvus

## Definition of Done

```text
Intro programática 3-5 telas (skip Esc, 1×/save, default legado seguro); mq_act1_00
auto-ofertada idempotente no 1º DayStarted; carta na cama via gerador com estado
persistido; prerequisite p/ cadeia E40 definido/documentado; PLACEHOLDER_LORE marcado e
listado no report; zero YAML manual; builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Fluxo de quest existente intacto (mq_act1_00 é quest comum no fluxo único).
DayStarted handlers existentes não afetados (assinatura aditiva).
New Game/reset (F56) sem caminho duplicado — intro apenas escuta.
FarmScene regenerada sem perder interactables existentes (diff aditivo do gerador).
Flags só tipos simples; saves antigos carregam com defaults seguros.
Eventos só via GameEventBus; unsubscribe pareado; zero GameObject.Find em runtime.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. KIT INICIAL CANÔNICO (2.1 APROVADO): enxada+regador+machado+picareta (tier básico),
   12 sementes de cenoura, 3 pães, 1 poção de vida pequena, 150g. SUBSTITUI o debug loadout.
2. PRIMEIRA ARMA (2.2-B): espada velha vem de BAÚ NARRATIVO na fazenda ("do antigo dono"),
   gerado pelo CreateMvpFarmScene — não no inventário inicial.
3. DESPERTAR DA FONTE (5.2-B): a Fonte começa DORMENTE (FonteState.Dormant); desperta na
   PRIMEIRA MORTE orgânica do jogador (3.5-A) — beat narrativo: acordar na Fonte recém-
   desperta. Ajustar o estado inicial configurado pela F17 (executada) nesta spec.
```
