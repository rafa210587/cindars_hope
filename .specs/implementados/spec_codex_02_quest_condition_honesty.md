# SPEC â€” Honestidade de Quest Conditions (CombatCondition real + IsFutureCondition auditÃ¡vel)

> **Spec ID:** `spec_codex_02_quest_condition_honesty`
> **Status:** Implementado e BUILD_VALIDATED (docs-migration 2026-08-12)
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P1
> **Type:** Runtime / Quest
> **Domain:** Quests
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_03, spec_codex_04, spec_codex_05, spec_codex_06, spec_codex_07, spec_codex_08
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/Conditions/QuestConditionResolver.cs`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende apenas de sistemas existentes: `QuestConditionResolver`/`QuestConditionContext`/`QuestConditionService` (Quests/Conditions), e leitura read-only do manager real de HP do player (ex.: `StaminaManager`/`PlayerHealthManager`, a confirmar em Fase 0).
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. NÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** `CombatCondition` deixa de retornar `true` hardcoded e passa a avaliar HP real do player quando aplicÃ¡vel, ou retornar `NotEvaluable` explÃ­cito com log one-shot; `IsFutureCondition()` ganha log one-shot dev + doc de dÃ©bito listando os 3 tipos deferidos.
> **Out of scope:** Implementar um `CombatSnapshot`/sistema de replay de combate completo; resolver os 3 tipos `*ConditionFuture` (Social/Pet/Companion) â€” esses ficam documentados como dÃ©bito, nÃ£o implementados.

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (docs-migration 2026-08-12)

```text
Status: Implementado e BUILD_VALIDATED
Execution report: docs/validation/spec_codex_02_quest_condition_honesty_execution_report.md (2026-07-03)
Re-verificação nesta sessão (Grep/Read no disco):
  - QuestConditionContext.cs: `public int? PlayerCurrentHp { get; set; }` / `PlayerMaxHp` confirmados.
  - QuestConditionResolver.cs: `EvaluateCombat()` real (compara HP% contra ExpectedValue/Operator,
    falha explícita quando snapshot ausente) confirmado; `CombatCondition => true` hardcoded removido.
  - `s_loggedFutureConditionTypes` (HashSet, log one-shot por ConditionType) confirmado em Evaluate().
  - Assets/_Game/Tests/EditMode/Quests/QuestConditionResolverCombatTests.cs existe no disco.
  - docs/game_rules/quest_rules.md contém "Rule 4.1: CombatCondition Is Real, Not Future...".
Build: dotnet build PASS (ambos assemblies, relatado no execution report).
Play Mode: NOT REQUIRED (spec seção 25) — lógica pura C#, evidência automatizada suficiente.
```

---

# /speckit.specify

## 5. Contexto

Achado verificado: `QuestConditionResolver.cs` L51 tem `QuestConditionType.CombatCondition => true, // deferred, no combat snapshot yet` â€” toda quest condition de combate sempre passa, mascarando o fato de que nenhuma checagem real ocorre. L35-36 tem `if (cond.IsFutureCondition()) return ConditionEvaluationResult.Pass();` â€” comportamento correto (nÃ£o travar quest em sistema nÃ£o implementado) mas silencioso, sem log nem doc de rastreamento.

## 6. Problema

Uma quest com condiÃ§Ã£o `CombatCondition` (ex.: "player abaixo de X% de HP", "matou N inimigos nesta sessÃ£o") sempre Ã© avaliada como satisfeita, mesmo que o estado real nÃ£o corresponda. Isso pode permitir progresso de quest indevido sem que ninguÃ©m perceba, porque nÃ£o hÃ¡ log nem teste que torne esse comportamento visÃ­vel. Da mesma forma, `IsFutureCondition()` sempre passa silenciosamente para 3 tipos de condiÃ§Ã£o (`SocialConditionFuture`, `PetConditionFuture`, `CompanionConditionFuture`) sem nenhum rastro â€” se uma quest real usar esses tipos, ninguÃ©m vai saber que a condiÃ§Ã£o nunca foi de fato avaliada.

## 7. Objetivo

Ao final desta spec: (a) `CombatCondition` avalia HP real do player via `QuestConditionContext` quando o dado existir no contexto, ou retorna falha explÃ­cita `NotEvaluable`/log one-shot quando nÃ£o hÃ¡ dado suficiente â€” nunca mais `true` incondicional; (b) `IsFutureCondition()` continua passando (comportamento correto para nÃ£o bloquear quest em sistema futuro) mas emite log one-shot dev by tipo e hÃ¡ um doc de dÃ©bito citando os 3 tipos deferidos e o critÃ©rio de fechamento.

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/rules/id-stability.md
.claude/skills/quest-authoring/SKILL.md
.claude/skills/observability-and-logging/SKILL.md
.claude/skills/editmode-test-authoring/SKILL.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o)

- `Assets/_Game/Scripts/Quests/Conditions/QuestConditionResolver.cs` (124 linhas) â€” pure C#, sem MonoBehaviour. MÃ©todos privados `EvaluateX(QuestConditionDefinition, QuestConditionContext)` para cada tipo de condiÃ§Ã£o jÃ¡ seguem um padrÃ£o consistente.
- `QuestConditionContext` (`Assets/_Game/Scripts/Quests/Conditions/QuestConditionContext.cs`) hoje expÃµe (conforme uso em `EvaluateX`): `ActiveFlags`, `PlayerReputation`, `ItemCounts`, `VisitedLocations`, `CurrentDay`, `CurrentWeather`, `CurrentLunarPhase`, `MetNpcIds`, `KnownDialogueIds`, `FarmLevel`, `CaveProgress`, `BestiaryKnownIds`. **NÃ£o hÃ¡ campo de HP/combate hoje** â€” precisa ser adicionado (ex.: `PlayerCurrentHpPercent` ou `int? PlayerCurrentHp` / `int? PlayerMaxHp`), a menos que a auditoria de implementaÃ§Ã£o encontre um campo jÃ¡ existente com nome diferente (checar antes de adicionar duplicado).
- `QuestConditionType` enum (`Assets/_Game/Scripts/Quests/Conditions/QuestConditionType.cs`) L1-23: 14 tipos reais (0-13) + 3 tipos `*ConditionFuture` (90-92: `SocialConditionFuture`, `PetConditionFuture`, `CompanionConditionFuture`).
- `QuestConditionDefinition.IsFutureCondition()` (`Assets/_Game/Scripts/Quests/Conditions/QuestConditionDefinition.cs` L20-22) checa exatamente esses 3 tipos futuros â€” `CombatCondition` (valor 11) **nÃ£o** Ã© um future type, Ã© um tipo real (0-13) que sÃ³ nÃ£o tem avaliaÃ§Ã£o real implementada. Ou seja, o problema do `CombatCondition => true` Ã© distinto do problema de `IsFutureCondition()`: o primeiro Ã© uma condiÃ§Ã£o "real" mal implementada; o segundo Ã© o mecanismo correto de skip para condiÃ§Ãµes genuinamente futuras.
- `ConditionEvaluationResult` (Grep necessÃ¡rio na implementaÃ§Ã£o para confirmar shape exato: `Success`, `FailedConditionIds`, `KnownFailureReasons`, `HiddenFailureReasons`, `CanShowInQuestLog`, `CanRetry`, `SuggestedFallbackId`) â€” nÃ£o tem hoje um estado "NotEvaluable" distinto de "Fail". Auditar se adicionar esse estado Ã© necessÃ¡rio ou se "Fail" com uma `KnownFailureReasons` explÃ­cita (`"Combat snapshot indisponÃ­vel"`) Ã© suficiente e mais minimalista (ver rule `code-minimalism-ladder`) â€” preferir a segunda opÃ§Ã£o se o contrato de `ConditionEvaluationResult` permitir sem quebrar consumidores.
- Player HP real existe em runtime (ex.: `PlayerHealthManager` ou equivalente â€” auditar nome exato em `Assets/_Game/Scripts/Player/**` antes de implementar) â€” quem monta o `QuestConditionContext` (procurar `QuestConditionService.cs`, que jÃ¡ apareceu no Grep de `CurrentDay`) precisa ser estendido para popular o novo campo a partir desse manager real.

## 10. User stories / engineering stories

```text
Como sistema de quest, quero que CombatCondition reflita o estado real de combate/HP do player quando o dado existir, para nÃ£o conceder progresso indevido.
Como agente de debug, quero um log one-shot quando uma condiÃ§Ã£o de combate nÃ£o pode ser avaliada, para nÃ£o silenciar o gap.
Como maintainer, quero saber exatamente quais 3 tipos de condiÃ§Ã£o sÃ£o "futuros" e por quÃª, documentado em um lugar canÃ´nico.
```

## 11. Escopo

Inclui:
- Adicionar o(s) campo(s) de HP real ao `QuestConditionContext` (ou reusar campo existente, se a auditoria de implementaÃ§Ã£o achar um).
- Estender `QuestConditionService` (ou o construtor equivalente do contexto) para popular esse campo a partir do manager de HP real do player.
- Reescrever o case `CombatCondition` em `QuestConditionResolver.Evaluate()` para: se o campo de HP estiver disponÃ­vel no contexto, avaliar de fato (ex.: comparar `ExpectedValue`/`Operator` contra HP percentual, seguindo o mesmo padrÃ£o de `EvaluateTime`/`EvaluateFarm`); se o dado nÃ£o estiver disponÃ­vel (contexto nÃ£o populado), falhar explicitamente com `KnownFailureReasons` citando "Combat snapshot indisponÃ­vel" e logar one-shot dev (nÃ£o crashar, nÃ£o silenciar).
- Adicionar log one-shot dev em `IsFutureCondition()` (ou no ponto de chamada em `Evaluate()`) citando qual tipo futuro foi encontrado, guardado para nÃ£o repetir por frame/chamada (seguir padrÃ£o de `(skill: observability-and-logging)`).
- Criar/estender doc de dÃ©bito canÃ´nico (`docs/game_rules/` ou `docs/backlog/`, conforme convenÃ§Ã£o existente) listando os 3 tipos `*ConditionFuture` com critÃ©rio de fechamento (quando Social/Pet/Companion tiverem sistema real).
- EditMode tests: (a) `CombatCondition` com contexto populado avalia corretamente (caso satisfeito e nÃ£o satisfeito); (b) `CombatCondition` sem dado no contexto retorna falha explÃ­cita, nÃ£o `true`; (c) `IsFutureCondition()` para os 3 tipos futuros continua retornando Pass (nÃ£o regredir), com verificaÃ§Ã£o de que o log one-shot nÃ£o dispara mais de uma vez por tipo por sessÃ£o (se praticÃ¡vel testar isso; senÃ£o documentar como residual).

Fora:
- Implementar um `CombatSnapshot`/replay de combate completo.
- Resolver os 3 tipos `*ConditionFuture` de verdade (isso Ã© trabalho de specs futuras de Social/Pet/Companion).
- Alterar qualquer outro `EvaluateX` jÃ¡ funcional.

## 12. Fora de escopo

```text
NÃ£o inclui: sistema de combate novo; UI de quest log; migraÃ§Ã£o de save (nenhum save schema muda aqui).
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o criar um segundo QuestConditionContext paralelo â€” estender o existente.
NÃ£o criar um novo enum de resultado â€” reusar ConditionEvaluationResult, adicionando apenas o necessÃ¡rio (uma KnownFailureReason nomeada Ã© preferÃ­vel a um novo estado, se o contrato permitir).
```

## 14. CritÃ©rios de aceite

### 14.1 CombatCondition avalia estado real quando disponÃ­vel

- Com `QuestConditionContext` populado com HP real do player, uma `CombatCondition` com operador/valor esperado compatÃ­vel com o HP atual passa; incompatÃ­vel falha.
- EvidÃªncia esperada: EditMode test cobrindo os dois casos.

### 14.2 CombatCondition falha explicitamente quando nÃ£o hÃ¡ dado

- Sem o campo populado no contexto, `Evaluate()` retorna `Success == false` com uma razÃ£o de falha citando indisponibilidade do snapshot de combate â€” nunca `true`.
- EvidÃªncia esperada: EditMode test + trecho do cÃ³digo.

### 14.3 IsFutureCondition() auditÃ¡vel

- Log one-shot dev disparado ao encontrar um tipo futuro, citando o `ConditionType` especÃ­fico.
- Doc de dÃ©bito lista os 3 tipos com critÃ©rio de fechamento.
- EvidÃªncia esperada: leitura do doc + do log one-shot + teste confirmando que Pass ainda ocorre para os 3 tipos (anti-regressÃ£o).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Conditions/
  QuestConditionContext.cs       (+ campo de HP real, se ausente)
  QuestConditionResolver.cs      (case CombatCondition real + log one-shot em IsFutureCondition)
  QuestConditionService.cs       (popular o novo campo do contexto a partir do manager real de HP)

docs/game_rules/ ou docs/backlog/
  <doc de dÃ©bito de quest conditions futuras> (novo ou seÃ§Ã£o adicionada a doc existente)

Assets/_Game/Tests/EditMode/Quests/
  QuestConditionResolverCombatTests.cs (novo, ou extensÃ£o de arquivo de teste existente do domÃ­nio)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
`QuestConditionContext` ganha 1-2 campos novos (nullable/opcional) de estado de combate â€” sem Unity refs, apenas `int`/`float`/`bool?`.

### 16.2 Runtime contracts
`QuestConditionResolver.Evaluate()` â€” mesma assinatura pÃºblica, lÃ³gica interna do case `CombatCondition` muda.

### 16.3 Event contracts
N/A â€” a menos que a auditoria de implementaÃ§Ã£o confirme que popular o contexto exige assinar um evento existente (ex.: `PlayerHealthChangedEvent`) em vez de ler um manager direto; se assim for, documentar aqui antes de implementar.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Quests (condition evaluation)
Player (fonte de leitura do HP real â€” read-only, sem nova escrita)
Observability/logging (log one-shot)
Docs (dÃ©bito canÃ´nico)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/Conditions/QuestConditionResolver.cs
Assets/_Game/Scripts/Quests/Conditions/QuestConditionContext.cs
Assets/_Game/Scripts/Quests/QuestConditionService.cs
Assets/_Game/Tests/EditMode/Quests/**
docs/game_rules/**
docs/backlog/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Qualquer arquivo de Player/Combat alÃ©m de leitura (nenhuma escrita nova no manager de HP)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria (concluÃ­da; ver seÃ§Ã£o 9) + confirmar nome exato do manager de HP real e se QuestConditionContext jÃ¡ tem campo equivalente
### Fase 1 â€” Estender QuestConditionContext + QuestConditionService
### Fase 2 â€” Reescrever case CombatCondition no Resolver
### Fase 3 â€” Log one-shot em IsFutureCondition + doc de dÃ©bito
### Fase 4 â€” Testes/validaÃ§Ã£o
### Fase 5 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Confirmar nome do manager de HP real (Grep antes de codar).
2. Adicionar campo(s) ao QuestConditionContext.
3. Popular o campo em QuestConditionService a partir do manager real.
4. Reescrever CombatCondition => avaliaÃ§Ã£o real / falha explÃ­cita.
5. Adicionar log one-shot em IsFutureCondition.
6. Criar/estender doc de dÃ©bito.
7. Escrever EditMode tests.
8. dotnet build + EditMode tests.
9. Registrar relatÃ³rio.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: demais specs do lote (sem overlap de arquivo)
- Must not run with: N/A
- Shared files/systems that require lock: Quests/Conditions/** (lock local Ã  spec)
- Reason: escopo isolado em Quests, nÃ£o toca Farm/World/Skills das outras specs do lote
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A
```

## 24. Impacto em eventos

```text
Adds events: NO (a menos que a Fase 0 de implementaÃ§Ã£o decida assinar evento existente â€” documentar se ocorrer)
Changes existing events: NO
Requires unsubscribe pattern: CONDITIONAL (sÃ³ se optar por assinar evento em vez de leitura direta)
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

## 26. Riscos tÃ©cnicos

```text
Risco: nenhuma quest real no repo hoje usa CombatCondition, tornando a mudanÃ§a "silenciosa" em termos de gameplay observÃ¡vel.
MitigaÃ§Ã£o: EditMode tests cobrem a lÃ³gica isoladamente; documentar no execution report se nenhuma quest data usa esse tipo hoje (Grep em Assets/_Game/Data/Quests/** por ConditionType == CombatCondition antes de fechar).

Risco: popular QuestConditionContext com HP real pode exigir acesso a um manager que nÃ£o estÃ¡ sempre disponÃ­vel (ex.: fora de Play Mode real, ou cena sem player).
MitigaÃ§Ã£o: tratar ausÃªncia do manager como "dado indisponÃ­vel" (case 14.2), nÃ£o como exceÃ§Ã£o.
```

## 27. Rollback

```text
Reverter QuestConditionResolver.cs para o case antigo (`=> true`).
Remover campo(s) novos de QuestConditionContext e a populaÃ§Ã£o em QuestConditionService.
Remover doc de dÃ©bito e testes novos.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Grep/confirmar nome do manager de HP real do player e se QuestConditionContext jÃ¡ tem campo equivalente.
- [ ] T002 â€” Adicionar campo(s) de estado de combate a QuestConditionContext.
- [ ] T003 â€” Popular o(s) campo(s) em QuestConditionService a partir do manager real.
- [ ] T004 â€” Reescrever o case CombatCondition em QuestConditionResolver (avaliaÃ§Ã£o real + falha explÃ­cita com log one-shot).
- [ ] T005 â€” Adicionar log one-shot dev em IsFutureCondition (por ConditionType).
- [ ] T006 â€” Criar/estender doc de dÃ©bito canÃ´nico com os 3 tipos futuros + critÃ©rio de fechamento.
- [ ] T007 â€” Escrever EditMode tests (satisfeito / nÃ£o satisfeito / dado ausente / future types ainda passam).
- [ ] T008 â€” Gerar execution report.
```

## 29. ValidaÃ§Ãµes obrigatÃ³rias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Test Runner â€” EditMode: rodar se praticÃ¡vel; senÃ£o `NOT RUN` com motivo.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (pure C# resolver)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: YES (proteger que os 3 future types continuam passando)
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode tests novos PASS cobrindo os 4 cenÃ¡rios de 14.1-14.3.
```

## 31. Definition of Done

```text
CombatCondition nunca mais retorna true incondicional.
IsFutureCondition() continua passando os 3 tipos futuros, agora com log one-shot.
Doc de dÃ©bito criado/atualizado.
EditMode tests novos passando.
Execution report criado.
```

## 32. Anti-regressÃ£o

```text
NÃ£o quebrar nenhuma quest existente que dependa de IsFutureCondition() sempre passar para os 3 tipos futuros.
NÃ£o alterar o comportamento de nenhum outro EvaluateX.
NÃ£o introduzir exceÃ§Ã£o nÃ£o tratada quando o manager de HP estiver ausente.
```

## 33. Notas para execuÃ§Ã£o posterior

```text
Se a auditoria de implementaÃ§Ã£o (T001) encontrar que nenhuma quest data hoje usa CombatCondition, documentar isso explicitamente â€” a spec ainda vale como hardening preventivo.
Resolver os 3 tipos *ConditionFuture de verdade fica para quando Social/Pet/Companion tiverem sistema real (fora de escopo aqui).
```
