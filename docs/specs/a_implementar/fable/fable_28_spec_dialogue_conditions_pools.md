# SPEC — Diálogo: Pools Condicionais (estação/clima/amizade/quest/hora)

> **Spec ID:** `fable_28_spec_dialogue_conditions_pools`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P2
> **Type:** Runtime / Data
> **Domain:** NPC / Dialogue
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_7
> **Can run with:** specs que não tocam TownNpcDialogueLibrary, NpcDialogueSetRegistry, FriendshipService ou o fluxo de diálogo
> **Must not run with:** F25, F26 (diálogo/amizade locks)
> **Repo lock scope:** TownNpcDialogueLibrary, NpcDialogueSetRegistry
> **Depends on:**
> - F26 (nível de amizade)
> - F15 (clima real)
> - WAVE 02 (estação/hora)
> **Blocks:**
> - F35 (cadeias usam condições)
> **Scope:** seleção de falas por condições de mundo + variação por amizade.
> **Out of scope:** localização, voice, diálogo ramificado profundo (árvore atual basta), lip de romance.

required_adrs: []
required_game_rules: [npc_rules.md]

---

# /speckit.specify

## Contexto

A `TownNpcDialogueLibrary` (criada na slice de NPCs) centraliza a autoria de diálogo dos
23 NPCs da cidade com 13 nós por NPC, materializados via `RebuildTownNpcDialogues` e
registrados no `NpcDialogueSetRegistry`. A seleção atual de fala é estática/aleatória: o
NPC fala a mesma coisa num dia de chuva de inverno e numa manhã de festival de verão.

A direction de NPCs (`CITY_NPC_ROSTER_DIRECTION` v1.1) define voz, propósito e bordões por
NPC (§7-29 — um bloco detalhado por personagem) e pede falas vivas: o NPC deve reagir a
estação, chuva, festival, nível de amizade, progresso da main quest e hora do dia. O
`QUEST_CATALOG_DIRECTION_v1.0` reforça com epígrafes/vozes por NPC ("o ferro lembra",
"tudo borbulha por um motivo") que devem contaminar as falas novas.

Os serviços de mundo necessários já existem ou estão na fila imediatamente anterior:
`GameCalendarService` (estação/dia/hora — WAVE 02), `WorldWeatherService` (clima real —
F15), `FriendshipService` (nível 0-5 — F26) e `QuestFlagService` (flags de marco). Esta
spec é a peça que liga esses serviços à seleção de fala — pequena porque NÃO cria árvore
de diálogo nova, apenas condiciona a escolha do nó dentro da library existente.

O que fica para specs futuras: cadeias de side quest com diálogo próprio (F35), romance,
localização e voice.

## Problema

Sem seleção condicional, o investimento das WAVEs de calendário/clima/amizade não aparece
na superfície mais visível do jogo — a conversa. O jogador rega amizade até o nível 4 e o
NPC continua com a fala de estranho; chove e ninguém comenta; a main quest avança e a vila
não reage. Além disso, F35 (cadeias de side quest) depende destas mesmas condições: sem
F28, F35 teria que criar um mecanismo paralelo de gating de fala, violando a regra de
não duplicação.

## Objetivo

Ao final desta spec, o projeto deve ter `DialogueCondition` + `DialogueConditionContext` +
`DialogueSelector` determinístico por dia, com a `TownNpcDialogueLibrary` expandida com
pools condicionais por NPC (estação/chuva/festival/amizade/marcos de main quest) na voz do
roster v1.1, permitindo que F35 reuse as MESMAS condições, sem alterar o fluxo de UI da
caixa de diálogo atual nem criar segunda library/registry.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/npcs_city/CITY_NPC_ROSTER_DIRECTION (v1.1 — vozes por NPC)
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md (epígrafes/vozes)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§6 diálogos)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/npc-dialogue-authoring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- TownNpcDialogueLibrary (23 NPCs × 13 nós + BuildNodes) — fonte única de autoria;
- NpcDialogueSetRegistry — registry canônico de sets de diálogo;
- RebuildTownNpcDialogues (editor) — gerador que materializa a library;
- GameCalendarService (estação/dia/hora — WAVE 02);
- WorldWeatherService (clima real — F15);
- FriendshipService (nível de amizade — F26);
- QuestFlagService (flags persistidas de quest).
Não existe:
- condições por nó de diálogo;
- contexto de condições montado dos serviços de mundo;
- pools condicionais de fala (estação/clima/festival/amizade/marcos);
- seletor por prioridade com desempate determinístico.
Auditar Fase 0:
- shape dos nós da library e do registry (os campos novos são aditivos?);
- como o fluxo atual escolhe o nó (ponto único de seleção a interceptar);
- se F15/F26 já aterrissaram no branch (gate de dependência).
```

## Engineering stories

```text
Como jogador, quero que o NPC comente a chuva, a estação e o festival, para a vila parecer viva.
Como jogador com amizade 4 com um NPC, quero falas mais calorosas do que as de um estranho.
Como sistema de quests (F35), quero reutilizar as mesmas condições de diálogo para gating de cadeias.
Como autor de conteúdo, quero toda a autoria centralizada na TownNpcDialogueLibrary, sem segunda fonte.
```

## Escopo

```text
Inclui:
- DialogueCondition {Season?, Weather?, MinFriendship?, RequiredFlag?, ForbiddenFlag?,
  TimeBand? (manhã/tarde/noite), FestivalDay?} — todos os campos opcionais (null = sempre
  elegível); estrutura serializável e aditiva ao shape atual do nó;
- DialogueConditionContext montado dos serviços reais (GameCalendarService,
  WorldWeatherService, FriendshipService, QuestFlagService) em um ponto único;
- seleção: filtrar nós elegíveis → prioridade = nº de condições atendidas (o nó mais
  específico vence) → desempate determinístico por dia via StableHash(npcId|dia) — a mesma
  fala vale o dia todo e muda no dia seguinte;
- autoria: TownNpcDialogueLibrary ganha por NPC: 2 falas por estação, 1 de chuva,
  1 de festival genérica, 2 por banda de amizade (0-1/2-3/4-5), mantendo as 13 atuais
  como fallback (≈10 novas/NPC, geradas na voz do roster v1.1 — §por-NPC com bordões,
  maneirismos e propósito de cada personagem);
- linha de quest: nó com RequiredFlag para reagir a marcos de main quest (3 marcos:
  chegada, pós-Ato-1, pós-Ato-3) — texto por NPC, na voz do roster;
- rebuild generator atualizado (RebuildTownNpcDialogues materializa os campos novos);
- EditMode tests: prioridade (mais específico vence), desempate determinístico por dia,
  fallback sem condição nunca vazio, contexto sintético por estação/clima/amizade/flag.
```

## Fora de escopo

```text
Não inclui:
- localização (PT-BR direto na library, como hoje);
- voice/áudio;
- diálogo ramificado profundo (a árvore atual basta);
- lip/sistema de romance (specs futuras);
- conteúdo autoral das cadeias de side quest (F35);
- UI nova de diálogo (caixa atual preservada).
```

## Regras de não duplicação

```text
Não criar segunda library nem segundo registry — os campos de condição são aditivos.
Não criar segundo serviço de calendário/clima/amizade — o contexto LÊ os serviços existentes.
Cadeias de side quest (F35) usam as MESMAS condições — não criar gating paralelo lá.
Sem árvore ramificada nova — apenas seleção condicionada do nó dentro da estrutura atual.
```

## Critérios de aceite

### CA-1 Especificidade vence

- Com contexto sintético "chuva + inverno + amizade 4", o seletor escolhe a fala que
  atende mais condições simultaneamente (e não uma genérica de estação).
- Evidência: EditMode test de prioridade com nós sintéticos de 0, 1, 2 e 3 condições.

### CA-2 Determinismo por dia

- O mesmo NPC repete a mesma fala durante o dia inteiro (StableHash(npcId|dia)) e
  apresenta fala potencialmente diferente no dia seguinte.
- Evidência: EditMode test fixando npcId/dia e variando hora; e variando o dia.

### CA-3 Pools completas na voz do roster

- Os 23 NPCs têm pools novas (estação/chuva/festival/amizade) na voz do roster v1.1;
  o fallback (13 nós atuais) nunca retorna vazio.
- Evidência: validação de contagem por NPC no rebuild + teste de fallback com contexto
  que não satisfaz nenhuma condição.

### CA-4 Reação a marcos de main quest

- Setar a flag sintética de um marco (chegada / pós-Ato-1 / pós-Ato-3) muda a fala
  selecionada dos NPCs que possuem nó com aquele RequiredFlag.
- Evidência: EditMode test com QuestFlagService sintético antes/depois da flag.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/
  DialogueCondition.cs          (NOVO — struct/classe serializável de condições opcionais)
  DialogueConditionContext.cs   (NOVO — snapshot do mundo: estação/clima/hora/amizade/flags)
  DialogueSelector.cs           (NOVO — filtro de elegibilidade + prioridade + desempate)
  TownNpcDialogueLibrary.cs     (autoria — pools novas por NPC, campos aditivos nos nós)
Assets/_Game/Scripts/Editor/
  RebuildTownNpcDialogues.cs    (atualizado — materializa condições/pools)
Assets/_Game/Tests/EditMode/City/
  DialogueConditionsTests.cs    (NOVO)
docs/validation/
  fable_28_spec_dialogue_conditions_pools_execution_report.md
```

## Contratos

### Data contracts

- `DialogueCondition`: campos opcionais `Season?`, `Weather?`, `MinFriendship?`,
  `RequiredFlag?`, `ForbiddenFlag?`, `TimeBand?` (manhã/tarde/noite), `FestivalDay?`.
  Campo null = condição ausente = nó sempre elegível por aquele eixo.
- Nós da library: campos aditivos — nenhum campo existente é renomeado/removido.

### Runtime contracts

- `DialogueConditionContext` é montado em ponto único a partir dos serviços reais
  (GameCalendarService, WorldWeatherService, FriendshipService, QuestFlagService);
  injetável sintético em testes.
- `DialogueSelector.Select(nodes, context, npcId, dia)`: puro e determinístico —
  elegíveis → maior nº de condições atendidas → StableHash(npcId|dia) como desempate.

### Event contracts

- N/A — não publica nem altera eventos; apenas LÊ serviços existentes.

### Save contracts

- N/A — nenhum estado novo persiste (estação/clima/amizade/flags já têm owners de save).

### UI contracts

- N/A — caixa de diálogo atual intacta; muda apenas QUAL texto chega a ela.

## Sistemas afetados

```text
NPC dialogue (library/registry/rebuild)
Calendar/Weather/Friendship/QuestFlag (somente leitura)
Testes EditMode (City)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/** (DialogueCondition/Context/Selector + library)
Assets/_Game/Scripts/Editor/RebuildTownNpcDialogues.cs (ou caminho real auditado na Fase 0)
Assets/_Game/Tests/EditMode/City/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/**
ProjectSettings/**
GameCalendarService / WorldWeatherService / FriendshipService / QuestFlagService
  (consumir APIs existentes, não alterar)
UI da caixa de diálogo (fluxo atual preservado)
SaveManager / seções de save
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar shape dos nós da library e do registry (confirmar que campos são aditivos);
localizar o ponto único de seleção de fala; confirmar presença de F15/F26 no branch.

### Fase 1 — Contratos e seletor
DialogueCondition + DialogueConditionContext + DialogueSelector determinístico,
com testes de prioridade/desempate/fallback ANTES da autoria.

### Fase 2 — Autoria das pools
Pools por NPC (estação/chuva/festival/amizade/marcos) na voz do roster v1.1,
mantendo os 13 nós atuais como fallback.

### Fase 3 — Rebuild e integração
RebuildTownNpcDialogues materializa os campos novos; o fluxo de diálogo passa a
chamar o seletor com o contexto real.

### Fase 4 — Validação e report
EditMode tests completos; csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: fable_batch_7
- Can run with: specs que não tocam diálogo/amizade nem os arquivos do lock scope.
- Must not run with: F25, F26 (locks de diálogo/amizade).
- Shared files/systems that require lock: TownNpcDialogueLibrary, NpcDialogueSetRegistry.
- Reason: a library é arquivo único de autoria; edição concorrente causa conflito direto.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
(Estação/clima/amizade/flags já persistem em seções próprias com owners existentes.)
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO (seletor puro; contexto montado sob demanda)
```

## Impacto em UI/Unity

```text
Changes UI: NO (caixa de diálogo atual)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: somente via RebuildTownNpcDialogues (gerador editor)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: volume de autoria (≈10 falas novas × 23 NPCs) sair da voz dos personagens.
Mitigação: gerar na voz do roster v1.1 seção por NPC (bordões/propósito); revisão humana
no lote final de validação.

Risco: campos de condição quebrarem o shape serializado dos nós existentes.
Mitigação: Fase 0 confirma aditividade; rebuild regenera tudo de uma fonte única.

Risco: seleção não determinística (Random) vazar para a escolha diária.
Mitigação: desempate exclusivamente por StableHash(npcId|dia), coberto por teste.

Risco: dependências F15/F26 não aterrissadas no momento da execução.
Mitigação: gate na Fase 0 — sem os serviços, marcar BLOCKED_BY_DEPENDENCY_PENDING.
```

## Rollback

```text
Flag de seletor: ignorar condições e cair no fallback (13 nós atuais) restaura o
comportamento anterior sem remover dados. Reverter os arquivos novos (Condition/Context/
Selector + testes) desfaz a spec por completo; a library mantém os nós antigos intactos.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar nós/registry; confirmar campos aditivos e ponto único de seleção.
- [ ] T002 — DialogueCondition + DialogueConditionContext + DialogueSelector determinístico + testes.
- [ ] T003 — Autoria: pools por NPC (estação/clima/amizade/festival/marcos) na voz do roster.
- [ ] T004 — Rebuild generator atualizado; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (seleção por prioridade + desempate por dia)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (diálogo atual permanece como fallback funcional)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano com falas
  mudando por clima/amizade

## Definition of Done

```text
Falas condicionais determinísticas por dia; library única com pools novas nos 23 NPCs;
fallback garantido (nunca vazio); seletor coberto por testes; rebuild materializa tudo;
nenhum arquivo proibido alterado; builds 0E; execution report criado.
```

## Anti-regressão

```text
Diálogo atual (13 nós por NPC) permanece como fallback — nunca remover.
Fallback nunca retorna vazio para nenhum NPC/contexto.
Nenhum GameObject.Find em runtime; nenhuma referência Unity em dados de condição.
Registry/library únicos — nenhuma segunda fonte de autoria criada.
Mesma fala o dia inteiro (determinismo) — sem Random na seleção diária.
```
