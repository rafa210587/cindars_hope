# SPEC — Redução Residual de Acoplamento NPC|Quests

> **Spec ID:** `spec_arch_npc_quest_boundary_residual_v1`
> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-16
> **Wave:** WAVE ARCH — Redução Residual de Acoplamento Modular
> **Priority:** P2
> **Type:** Runtime
> **Domain:** NPC
> **Parallelizable:** CONDITIONAL
> **Parallel group:** arch_boundary_residual
> **Can run with:** specs que não tocam `NPC/NpcController.cs`, `NPC/NpcShopController.cs`, `Quests/Runtime/QuestGiverInteractable.cs`, `Quests/FestivalQuests/FestivalQuestService.cs`, `Quests/Runtime/QuestRuntimeBootstrap.cs`
> **Must not run with:** `spec_arch_ui_boundary_residual_v1` (lock em `NPC/NpcController.cs`)
> **Repo lock scope:** `Assets/_Game/Scripts/NPC/NpcController.cs`, `Assets/_Game/Scripts/NPC/NpcShopController.cs`, `Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs`, `Assets/_Game/Scripts/Quests/FestivalQuests/FestivalQuestService.cs`
> **Depends on (Depende de):**
> - `.claude/rules/RULES.md` (invariante de comunicação de gameplay só via GameEventBus; ID stability — não renomear `quest_*`/`npc_*` sem migration)
> - `.claude/skills/npc-dialogue-authoring/SKILL.md`
> - `.claude/skills/quest-authoring/SKILL.md`
> **Blocks (Bloqueia):**
> - Nenhuma outra spec depende diretamente desta.
> **Scope:** Caracterizar o comportamento atual de NPCs com quest (papel, rotina, diálogo, oferta/turn-in) via testes de caracterização, e então extrair um contrato de quest-interaction (adapter/bridge) para o único ponto de acoplamento direto confirmado: `NpcController.ResolveQuestInteractionMode` chamando `QuestRuntimeBootstrap.QuestService` diretamente, e `QuestGiverInteractable` chamando `GetComponent<NpcController>()`. Preservar 100% do comportamento observável de diálogo, quest offer/turn-in, papel e rotina.
> **Out of scope:** Texto/IDs de quest; conteúdo de diálogo; balance de recompensa; save schema; horários/rotina em si (só não podem regredir); UI de quest log/offer panel.

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (2026-07-16)

O par `NPC|Quests` saiu de `MutualModulePairs`, mas **apenas via um lado** do corte prescrito —
verificado por leitura direta do código nesta sessão de closeout, não só pelo texto do commit.

```text
Commit real (branch dev, HEAD a4203461):
1b92c6eb refactor(arquitetura): cortar par mutuo NPC|Quests via porta + evento
         -> Quests->NPC: QuestGiverInteractable.ResolveNpcId() troca GetComponent<NpcController>()
            por INpcIdentity (Foundation); FestivalQuestService troca CindarsHope.NPC.Events/
            Friendship por um evento irmao minimo NpcGiftAcceptedEvent (Core.Events).
         -> MutualModulePairs 18 -> 17 (par sai da lista porque so uma direcao restava mutua)

Validation method: Invoke-UnityGeneratedProjectsBuild.ps1 + RunUnityEditModeTests.ps1 +
Get-ModularizationDependencySnapshot.ps1
Build (7 projects): PASS (exit 0)
EditMode: PASS 2837/2837 (exit 0)
Snapshot: MutualModulePairs=0 (NPC|Quests ausente)
Play Mode / validacao humana: NOT RUN - pendente; coberto por spec_validation_human_playmode_smoke_v1
(segue em a_implementar/)
```

**Histórico de execução em duas etapas (registrado para honestidade):**

O commit `1b92c6eb` cortou o par mútuo mas cumpriu apenas o lado `Quests -> NPC`; os critérios
nomeados 14.1 e 14.2 ficaram em aberto. Isso foi detectado no closeout por LEITURA DO CÓDIGO — não
teria sido visível pelo snapshot nem pela mensagem do commit — e fechado em seguida:

```text
Commit de fechamento de 14.1/14.2 (mesma sessao, 2026-07-16):
  -> 14.2 CUMPRIDO: contrato minimo IQuestInteractionQuery (CanTurnIn/HasQuestState) em
     CindarsHope.Quests.Runtime; QuestService o implementa (HasQuestState => GetQuestState(id) != null).
     A decisao foi extraida para a policy pura NpcQuestInteractionPolicy.ResolveMode(questId, query)
     — mesmo precedente das outras policies puras de NPC ja testadas em EditMode.
     NpcController.ResolveQuestInteractionMode delega a policy, preservando assinatura, visibilidade
     e a MESMA fonte de resolucao (QuestRuntimeBootstrap.QuestService): sem bootstrap novo, sem
     mudanca de timing.
  -> 14.1 CUMPRIDO: NpcQuestInteractionPolicyTests (8 testes de caracterizacao) cobrindo questId
     vazio/null/whitespace -> NoQuest; query ausente -> Offer; CanTurnIn -> TurnIn; sem estado ->
     Offer; com estado -> NoQuest; e a precedencia CanTurnIn > HasQuestState.
     EditMode 2837 -> 2845 (Passed 2845, Failed 0, exit 0). Build 7/7 exit 0.
     Snapshot permanece MutualModulePairs=0 (nenhum par novo).
```

Equivalência verificada linha a linha: `service == null -> Offer` ≡ `query == null -> Offer`;
`GetQuestState(id) == null ? Offer : NoQuest` ≡ `!HasQuestState(id) ? Offer : NoQuest`.

**Desvio remanescente (honesto, fora do escopo desta spec):** o lado `NPC -> Quests` da ARESTA (não
do par mútuo, que está cortado) persiste — `NpcController` ainda importa `CindarsHope.Quests.Runtime`
para o tipo do contrato, e `NpcShopController` (2 sites) e `Friendship/RomanceService` também nomeiam
`CindarsHope.Quests`. Isso NÃO é um ciclo e não afeta `MutualModulePairs=0`: uma dependência
unidirecional NPC -> Quests é acoplamento em camadas normal. Fechá-la por completo seria trabalho
novo, não previsto por esta spec.

**Resultado líquido:** critério central (par fora de `MutualModulePairs`) satisfeito; 14.1 e 14.2
cumpridos; build/EditMode verdes.

**Residual risk:** validação humana de Play Mode não rodada (coberta por
`spec_validation_human_playmode_smoke_v1`, que segue em `a_implementar/`) — o fluxo de oferta/turn-in
de quest via diálogo de NPC deve ser exercitado no playtest.

---

# /speckit.specify

## 5. Contexto

Esta spec reduz drift acumulado de acoplamento modular na fronteira `NPC|Quests`, medido pelo snapshot de dependências do repo (ver baseline abaixo). Esta fronteira é mútua (ambas as direções existem hoje), o que a torna mais arriscada que um corte unidirecional: qualquer extração precisa preservar tanto o caminho `NPC → Quests` quanto `Quests → NPC` sem quebrar o fluxo real de conversa/quest do jogo.

Baseline de dependência modular (Branch=dev, Snapshot 2026-07-13, gerado por `tools/architecture/Get-ModularizationDependencySnapshot.ps1`):

```text
RuntimeModuleEdges=241
MutualModulePairs=25
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
```

Nenhuma spec desta wave declara "modularização concluída" enquanto `MutualModulePairs > 0`. Esta spec reduz o número, não o zera.

## 6. Problema

`NpcController` e `QuestGiverInteractable` (em `Quests/Runtime`) referenciam-se mutuamente sem contrato explícito: `NpcController.ResolveQuestInteractionMode` (static, linhas 457-478) chama `CindarsHope.Quests.Runtime.QuestRuntimeBootstrap.QuestService.CanTurnIn(questId)` e `.GetQuestState(questId)` diretamente; `QuestGiverInteractable.ResolveNpcId()` (linha 74) faz `GetComponent<NpcController>()` para ler `NpcData.NpcId` quando `_npcId` não está setado no Inspector. Isso significa que uma mudança na assinatura pública de `QuestService` ou em `NpcController` propaga silenciosamente para o outro módulo sem um contrato nomeado, dificultando revisão de impacto e mantendo `NPC|Quests` como par mútuo no snapshot de arquitetura.

## 7. Objetivo

Ao final desta spec: (a) existe suíte de testes de caracterização cobrindo o comportamento atual de oferta/turn-in de quest via NPC antes de qualquer refactor; (b) `NpcController.ResolveQuestInteractionMode` consome quest state via um contrato explícito (interface ou wrapper nomeado) em vez de chamar `QuestRuntimeBootstrap.QuestService` diretamente inline; (c) `QuestGiverInteractable.ResolveNpcId()` obtém o `npcId` via um contrato mínimo (interface implementada por `NpcController`, ex. `INpcIdentity`) em vez de acoplar-se ao tipo concreto `NpcController` via `GetComponent`; (d) nenhum ID de quest/NPC muda; nenhuma regressão de diálogo, oferta, turn-in, papel ou rotina.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/RULES.md
.claude/rules/unity-architecture.md
.claude/rules/id-stability.md
.claude/skills/npc-dialogue-authoring/SKILL.md
.claude/skills/quest-authoring/SKILL.md
.claude/skills/editmode-test-authoring/SKILL.md
```

Para runtime/code specs, incluir também:

```text
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão via Grep/Read direto)

Confirmado nesta sessão:

- `Assets/_Game/Scripts/NPC/NpcController.cs`: `using CindarsHope.Quests.Runtime;` (linha 7). Método estático privado `ResolveQuestInteractionMode(string questId)` (linhas 457-478) chama `QuestRuntimeBootstrap.QuestService` diretamente: `service.CanTurnIn(questId)`, `service.GetQuestState(questId)`. Isto é acoplamento direto ao runtime de Quests, não apenas via evento. `NpcController` também publica `QuestGiverInteractedEvent` via `GameEventBus` (linha ~381) quando o choice é `DialogueActionType.OfferQuest` — este caminho já é desacoplado (evento), o problema está isolado no método `ResolveQuestInteractionMode`.
- `Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs`: `using CindarsHope.NPC;` (linha 4). Comentário no próprio arquivo (linhas 9-21) já documenta a intenção de desacoplamento: "Publishes QuestGiverInteractedEvent; does NOT accept quests directly. QuestOfferPanelController (UI side) listens and calls QuestService.AcceptQuest. Does NOT use GameObject.Find/FindObjectOfType." Porém `ResolveNpcId()` (linha 74) ainda faz `GetComponent<NpcController>()` e lê `npcController.NpcData.NpcId` diretamente quando o campo `_npcId` não está preenchido no Inspector — este é o ponto de acoplamento reverso (`Quests → NPC`). `GetComponent` no mesmo GameObject é uma exceção explicitamente permitida pela rule `unity-architecture.md` item 2 (não é scene search), então este ponto pode ser preservado como está OU substituído por uma interface (`INpcIdentity`) se a Fase 0 concluir que reduz o par mútuo sem custo de comportamento — decisão de Fase 0, documentar a escolha.
- `Assets/_Game/Scripts/Quests/FestivalQuests/FestivalQuestService.cs`: `using CindarsHope.NPC.Events;` e `using CindarsHope.NPC.Friendship;` (linhas 5-6) — acoplamento adicional de `Quests → NPC` fora do escopo desta spec (Friendship/Events, não diálogo/oferta); Fase 0 deve confirmar se este uso é via evento/tipo de dados simples (provavelmente aceitável) ou chamada direta de método — se for direta, registrar como candidato de spec futura, não expandir escopo aqui.
- `Assets/_Game/Scripts/Quests/NpcChains/NpcQuestChainService.cs`: confirmado NÃO referenciar `CindarsHope.NPC` (grep sem match) — já desacoplado, não tocar.
- `NpcShopController.cs` não foi confirmado nesta sessão como acoplado a Quests diretamente (grep de "Quest" em `NPC/` não listou `NpcShopController.cs` entre os arquivos com match) — Fase 0 de execução deve reconfirmar antes de assumir que está fora do escopo real; se `NpcShopController` não tiver acoplamento a Quests, não tocar nele além do necessário para o Repo lock scope declarado (mantido por precaução de overlap com a outra spec).
- O que esta spec não deve recriar: `QuestService`, `QuestRegistry`, `GameEventBus`, `QuestGiverInteractedEvent`.

Este estado real precisa ser reconfirmado na Fase 0 de execução (a auditoria acima é a base, não substitui a leitura direta do código no momento de implementar).

## 10. User stories / engineering stories

```text
Como agente executor, quero uma suíte de caracterização do comportamento atual de NPC+Quest antes de tocar em qualquer contrato, para detectar regressão imediatamente.
Como NpcController, quero resolver o modo de interação de quest (Offer/TurnIn/NoQuest) via um contrato nomeado, para não depender do tipo concreto QuestRuntimeBootstrap.QuestService inline.
Como QuestGiverInteractable, quero obter o npcId do NPC anexado via um contrato mínimo, para reduzir o acoplamento a NpcController concreto sem perder a leitura de fallback existente.
```

## 11. Escopo

Inclui:
- Testes de caracterização (EditMode, se praticável sem exigir cena completa; caso contrário, documentar como PlayMode scenario) cobrindo: `ResolveQuestInteractionMode` para os 3 casos (`NoQuest` quando `questId` vazio; `Offer` quando quest não iniciada e prerequisitos completos; `TurnIn` quando `CanTurnIn` é true), e `QuestGiverInteractable.DetermineMode`/`FindRelevantQuestId` para os mesmos 3 casos.
- Extrair um contrato mínimo (ex. `IQuestInteractionQuery` em `Foundation` ou `Quests`, decisão de Fase 0 seguindo a convenção de ports existente) cobrindo apenas `CanTurnIn(string questId)`, `GetQuestState(string questId)` (ou equivalente), `ArePrerequisitesComplete(string questId)` — o subset real usado por `NpcController`/`QuestGiverInteractable`.
- `NpcController.ResolveQuestInteractionMode` passa a consumir o contrato em vez de `QuestRuntimeBootstrap.QuestService` diretamente (resolução do contrato via o mesmo `QuestRuntimeBootstrap` ou `DomainManagerRegistry`, decisão de Fase 0 — não criar novo bootstrap).
- Avaliar (Fase 0) se `QuestGiverInteractable.ResolveNpcId()` deve trocar `GetComponent<NpcController>()` por `GetComponent<INpcIdentity>()` (interface mínima implementada por `NpcController`) — implementar se reduzir o par mútuo sem custo de comportamento; documentar se decidir manter como está (por já ser `GetComponent` no mesmo GameObject, exceção permitida pela rule).

Fora:
- Texto/IDs de quest, diálogo, papéis, rotina em si (não podem regredir, mas não são o objeto da mudança).
- `FestivalQuestService` → `NPC.Events`/`NPC.Friendship` (fora de escopo, candidato a spec futura se Fase 0 confirmar acoplamento direto problemático).
- `NpcQuestChainService` (já desacoplado, confirmado nesta sessão).
- UI de quest log/offer panel.

## 12. Fora de escopo

```text
Não inclui: mudança de texto/IDs de quest; mudança de diálogo; mudança de rotina/horário de NPC; mudança de FestivalQuestService; mudança de save schema; UI nova.
```

## 13. Regras de não duplicação

```text
Não criar um segundo QuestService ou QuestRegistry.
Não criar um novo *RuntimeBootstrap — reusar QuestRuntimeBootstrap existente para resolver o contrato.
Não duplicar a lógica de ArePrerequisitesComplete/CanTurnIn — o contrato deve delegar para QuestService, não reimplementar a regra.
```

## 14. Critérios de aceite

### 14.1 Caracterização antes do refactor

- Testes de caracterização criados e passando ANTES de qualquer mudança de contrato (commit/checkpoint intermediário se praticável).
- Evidência esperada: arquivo de teste + resultado de execução ANTES do refactor (capturado no relatório).

### 14.2 NpcController usa contrato, não tipo concreto inline

- `ResolveQuestInteractionMode` não chama `QuestRuntimeBootstrap.QuestService` diretamente dentro do corpo do método (a resolução do contrato pode acontecer via bootstrap, mas o método de decisão em si opera sobre a interface).
- Os 3 modos (`NoQuest`/`Offer`/`TurnIn`) continuam corretos para os mesmos inputs cobertos pela caracterização.
- Evidência esperada: diff + testes de caracterização ainda passando.

### 14.3 QuestGiverInteractable — decisão documentada

- Se a Fase 0 decidir manter `GetComponent<NpcController>()`: documentar por quê (exceção permitida pela rule) no relatório.
- Se a Fase 0 decidir trocar por `GetComponent<INpcIdentity>()`: `NpcController` implementa a interface; comportamento de `ResolveNpcId()` idêntico para os casos testados.

### 14.4 Nenhum ID muda; nenhuma regressão de fluxo

- Nenhum `quest_*`/`npc_*` ID é renomeado ou removido.
- NPCs continuam com papel e rotina esperados; quest offer/turn-in continuam funcionando; smoke humano de conversa/quest documentado e executado.
- Build/EditMode passam.

## 15. Notas

```text
Esta spec trata a fronteira NPC|Quests como mútua e de maior risco que um corte unidirecional simples — por isso exige caracterização ANTES do refactor, diferente de specs de corte unidirecional (ex. Craft|UI).
```

---

# /speckit.plan

## 16. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Runtime/  (ou Foundation/, decisão de Fase 0)
  IQuestInteractionQuery.cs        (novo — contrato mínimo: CanTurnIn, GetQuestState, ArePrerequisitesComplete)

Assets/_Game/Scripts/NPC/
  NpcController.cs                  (ResolveQuestInteractionMode consome o contrato)
  INpcIdentity.cs                   (novo, SE Fase 0 decidir pelo corte reverso também)

Assets/_Game/Scripts/Quests/Runtime/
  QuestGiverInteractable.cs         (ResolveNpcId via INpcIdentity, SE decidido em Fase 0)
  QuestService.cs                   (implementa IQuestInteractionQuery — sem mudança de comportamento público)

Assets/_Game/Tests/EditMode/NPC/
  NpcQuestInteractionCharacterizationTests.cs  (novo)

Assets/_Game/Tests/EditMode/Quests/
  QuestGiverInteractableCharacterizationTests.cs  (novo, se praticável sem cena completa)

docs/validation/
  spec_arch_npc_quest_boundary_residual_v1_execution_report.md
```

## 17. Contratos, dados e eventos

### 17.1 Data contracts
Nenhum novo DTO de dados. Contrato de interface (`IQuestInteractionQuery`, e opcionalmente `INpcIdentity`) em C# puro/Unity-agnostic quando possível.

### 17.2 Runtime contracts
`IQuestInteractionQuery` cobre exatamente `CanTurnIn(string questId): bool`, `GetQuestState(string questId): <tipo atual de retorno>`, `ArePrerequisitesComplete(string questId): bool` — assinaturas idênticas às já expostas por `QuestService`, só extraídas para interface.

### 17.3 Event contracts
N/A — nenhum evento novo ou alterado. `QuestGiverInteractedEvent` permanece inalterado.

### 17.4 Save contracts
N/A — nenhum DTO novo, nenhuma mudança de save schema, nenhum ID alterado.

### 17.5 UI contracts
N/A — nenhuma UI tocada por esta spec.

## 18. Sistemas afetados

```text
NPC (NpcController, possivelmente NpcShopController se Fase 0 confirmar acoplamento)
Quests (QuestGiverInteractable, QuestService via implementação de interface)
```

## 19. Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/NpcController.cs
Assets/_Game/Scripts/NPC/NpcShopController.cs
Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs
Assets/_Game/Scripts/Quests/Runtime/QuestService.cs
Assets/_Game/Scripts/Quests/Runtime/IQuestInteractionQuery.cs
Assets/_Game/Scripts/Foundation/**  (apenas se o contrato for posicionado em Foundation por decisão de Fase 0)
Assets/_Game/Tests/EditMode/NPC/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/**
```

## 20. Arquivos proibidos

```text
Assets/_Game/Scripts/Quests/FestivalQuests/**
Assets/_Game/Scripts/Quests/NpcChains/**
Assets/_Game/Scripts/UI/Quests/**
Assets/**/*.unity, *.prefab, *.asset salvo autorização explícita
Assets/_Game/Scripts/Save/**
docs_old/**
docs/archive/**
Packages/**
ProjectSettings/**
```

## 21. Estratégia de implementação

```md
### Fase 0 — Auditoria: reler NpcController.cs (ResolveQuestInteractionMode), QuestGiverInteractable.cs, QuestService.cs (assinatura real); reconfirmar se NpcShopController tem acoplamento a Quests; decidir posicionamento do contrato (Quests/Runtime vs Foundation); decidir se INpcIdentity entra no escopo
### Fase 1 — Escrever testes de caracterização (ANTES de qualquer mudança de contrato) e capturar resultado baseline
### Fase 2 — Extrair IQuestInteractionQuery; QuestService implementa
### Fase 3 — NpcController.ResolveQuestInteractionMode consome o contrato
### Fase 4 — (Condicional) INpcIdentity + QuestGiverInteractable.ResolveNpcId via interface
### Fase 5 — Rodar testes de caracterização novamente; confirmar nenhuma regressão
### Fase 6 — Build + EditMode + smoke humano; relatório
```

## 22. Ordem de execucao (ordem segura)

```text
1. Ler .claude/rules/id-stability.md e confirmar nenhum ID será tocado.
2. Auditar shape real de QuestService, NpcController, QuestGiverInteractable, NpcShopController.
3. Escrever testes de caracterização e rodar contra o código atual (baseline verde).
4. Extrair IQuestInteractionQuery; QuestService implementa sem mudar assinatura pública existente.
5. NpcController.ResolveQuestInteractionMode passa a usar o contrato.
6. (Condicional) INpcIdentity + QuestGiverInteractable.
7. Rodar testes de caracterização de novo — devem passar sem edição.
8. dotnet build (Assembly-CSharp e Assembly-CSharp-Editor).
9. EditMode tests completos do domínio NPC/Quests.
10. Smoke humano documentado (ver Testing Quality Gate).
11. Rodar snapshot de dependência modular e registrar delta no relatório.
12. Registrar relatório.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: arch_boundary_residual
- Can run with:
  - specs que não tocam os arquivos do repo lock scope
- Must not run with:
  - `spec_arch_ui_boundary_residual_v1` (lock em `NPC/NpcController.cs`)
- Shared files/systems that require lock:
  - `Assets/_Game/Scripts/NPC/NpcController.cs`
  - `Assets/_Game/Scripts/NPC/NpcShopController.cs`
  - `Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs`
  - `Assets/_Game/Scripts/Quests/Runtime/QuestService.cs`
- Reason:
  - Overlap direto de arquivo com `spec_arch_ui_boundary_residual_v1` em `NpcController.cs`.

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A — sem save DTO novo
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: extrair contrato sem caracterização prévia pode mascarar regressão sutil em ArePrerequisitesComplete/CanTurnIn (lógica de gate de quest documentada como single source em QuestService per fable_34/EMENDA 2026-06-12-C).
Mitigação: Fase 1 (caracterização) é obrigatória e roda ANTES de qualquer mudança de contrato — bloqueante para as fases seguintes.

Risco: mudar ResolveNpcId() de GetComponent<NpcController>() para GetComponent<INpcIdentity>() pode não ser necessário (GetComponent no mesmo GameObject já é exceção permitida) e adicionar complexidade sem reduzir risco real.
Mitigação: Fase 0 decide explicitamente se vale a pena; se não, documentar a decisão de manter como está e não forçar a mudança (escada de minimalismo — não implementar se YAGNI).

Risco: FestivalQuestService também acopla a NPC.Events/NPC.Friendship — ficar fora do escopo pode deixar o par mútuo não totalmente resolvido.
Mitigação: aceitável — esta spec reduz, não zera, o par mútuo; registrar o remanescente como candidato de spec futura no relatório.
```

## 27. Rollback

```text
Reverter NpcController.ResolveQuestInteractionMode para chamada direta a QuestRuntimeBootstrap.QuestService.
Remover IQuestInteractionQuery (e INpcIdentity, se criado) se não usado em outro lugar.
Reverter QuestGiverInteractable.ResolveNpcId se alterado.
Manter os testes de caracterização mesmo em rollback (não removem valor).
Nenhum dado de save é afetado.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Auditar shape real de QuestService, NpcController, QuestGiverInteractable, NpcShopController (Fase 0).
- [ ] T002 — Escrever testes de caracterização e capturar baseline verde ANTES do refactor.
- [ ] T003 — Extrair IQuestInteractionQuery; QuestService implementa.
- [ ] T004 — NpcController.ResolveQuestInteractionMode consome o contrato.
- [ ] T005 — (Condicional, decisão de Fase 0) INpcIdentity + QuestGiverInteractable.ResolveNpcId via interface.
- [ ] T006 — Rodar testes de caracterização novamente; confirmar zero regressão.
- [ ] T007 — dotnet build + EditMode tests completos (NPC + Quests).
- [ ] T008 — Executar e documentar smoke humano.
- [ ] T009 — Rodar snapshot de dependência modular; registrar delta no relatório.
- [ ] T010 — Gerar execution report.
```

## 29. Validações obrigatórias

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.NPC" -ResultsPath "TestResults\spec-npc-quest-boundary-npc.xml" -LogFile "Logs\spec-npc-quest-boundary-npc.log"
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Quests" -ResultsPath "TestResults\spec-npc-quest-boundary-quests.xml" -LogFile "Logs\spec-npc-quest-boundary-quests.log"
.\tools\docs\validate_docs.ps1
```

Se algum comando não puder rodar, o report deve registrar `NOT RUN` com motivo e risco residual.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (resolução de modo de interação de quest — Offer/TurnIn/NoQuest)
- Requires EditMode tests: YES (caracterização ANTES do refactor + testes do contrato novo)
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (a própria suíte de caracterização é o regression test)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + testes de caracterização PASS antes e depois do refactor + smoke humano documentado com os passos abaixo executados e resultado registrado.
```

Smoke humano obrigatório (executar e documentar resultado por passo):

```text
1. Conversar com npc_thalindra (ou outro NPC quest giver disponível) sem quest ativa — confirmar prompt/oferta aparece quando prerequisitos estão completos.
2. Aceitar a quest ofertada — confirmar QuestGiverInteractedEvent dispara e o fluxo de aceitação (QuestOfferPanelController) funciona sem regressão.
3. Completar os objetivos da quest e voltar ao NPC — confirmar modo muda para TurnIn e a entrega funciona.
4. Conversar com um NPC sem quest disponível — confirmar prompt "não há nada para você agora" (ou equivalente) continua correto.
5. Confirmar papel/rotina do NPC (horário, posição, disponibilidade) não regrediu.
```

## 31. Definition of Done

```text
Spec implementada dentro dos arquivos permitidos.
Nenhum arquivo proibido alterado.
Nenhum ID de quest/NPC renomeado ou removido.
Testes de caracterização criados ANTES do refactor e passando depois, sem edição de expectativa.
Docs validation executada ou NOT RUN com motivo.
dotnet build + Unity compile validation executados ou NOT RUN com motivo.
Smoke humano executado e documentado.
Snapshot de dependência modular re-rodado e delta de MutualModulePairs/UsingOnlyMutualModulePairs registrado no relatório.
Execution report criado.
Sem claim de ACCEPTED sem evidência.
```

## 32. Anti-regressão

```text
NPCs continuam com papel e rotina esperados.
Quest offer/turn-in continuam funcionando exatamente como hoje para os casos cobertos pela caracterização.
Nenhum ID de quest ou NPC muda.
Não alterar evento público sem atualizar consumidores.
Não serializar referência Unity em save.
Não usar GameObject.Find/FindObjectOfType em runtime (GetComponent no mesmo GameObject permanece exceção permitida).
Não declarar "modularização concluída" — apenas registrar o delta de MutualModulePairs.
```

## 33. Notas para execução posterior

```text
Esta spec não resolve o acoplamento remanescente FestivalQuestService → NPC.Events/NPC.Friendship — registrar como candidato de spec futura no relatório se confirmado como chamada direta (não apenas tipo de dado/evento).
Esta spec não executa validação humana imediata fora do smoke que é seu próprio critério de aceite — não substitui uma validação humana de lote maior se o usuário pedir uma no final da wave.
```

## 34. Checklist final da spec pronta

```text
[x] Tem cabeçalho completo.
[x] Declara Parallelizable / Parallel group / locks.
[x] Declara fontes obrigatórias lidas.
[x] Declara estado atual do repo.
[x] Tem escopo pequeno.
[x] Tem fora de escopo explícito.
[x] Tem regras de não duplicação.
[x] Tem arquivos permitidos e proibidos.
[x] Tem contratos/dados/eventos/save/UI quando aplicável.
[x] Tem critérios de aceite verificáveis.
[x] Tem validações obrigatórias.
[x] Tem Testing Quality Gate.
[x] Marca validação humana como DEFERRED_TO_FINAL_VALIDATION quando aplicável.
[x] Tem riscos e rollback.
[x] Não pede execução humana intermediária.
[x] Não altera SPEC_EXECUTION_ORDER.md como se a spec já estivesse implementada.
```
