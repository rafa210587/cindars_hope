# SPEC — Hash Estável FNV-1a para Seeds Determinísticos da Cave

> **Spec ID:** `spec_codex_09_cave_stable_hash`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE — Honestidade de Validação (Lote 2)
> **Priority:** P0
> **Type:** Runtime
> **Domain:** Cave / Enemy / Determinism
> **Parallelizable:** YES
> **Parallel group:** codex_convergence_lote2
> **Can run with:** spec_codex_10, spec_codex_11, spec_codex_12, spec_codex_13
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawner.cs`, `Assets/_Game/Scripts/Cave/Runtime/CaveResourceNodeMaterializer.cs`, `Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs`
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende apenas do algoritmo FNV-1a já existente em `CaveLayoutStableHash.Compute` (`Assets/_Game/Scripts/Cave/Generation/CaveLayoutStableHash.cs`).
> **Blocks (Bloqueia):**
> - Nenhuma outra spec do lote `codex_convergence_lote2`.
> **Scope:** Substituir `string.GetHashCode()` (não estável entre versões/processos do .NET/Mono — pode variar por randomização de hash de string em runtime) pelo hash FNV-1a 32-bit determinístico já existente no projeto, nos 3 pontos identificados que hoje semeiam `System.Random` de conteúdo estável da cave.
> **Out of scope:** Mudar o algoritmo FNV-1a em si; mudar a composição das seed strings (`"{world}_{run}_{level}_enemies"` etc. permanecem idênticas); resolver persistência de snapshot completo (fora de escopo, já documentado como limitação conhecida da FASE9F).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Achado verificado nesta sessão (Grep + leitura direta):

- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawner.cs:66`: `var deterministicRandom = new System.Random(seedString.GetHashCode());`
- `Assets/_Game/Scripts/Cave/Runtime/CaveResourceNodeMaterializer.cs:78`: `var spawnRandom = new System.Random(spawnSeedString.GetHashCode());`
- `Assets/_Game/Scripts/Cave/Runtime/CaveResourceNodeMaterializer.cs:347`: `var deterministicRandom = new System.Random(seedString.GetHashCode());`
- `Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs:134-136` (`DeriveSummonSeed`): `h = h * 31 + (caveRunSeed ?? string.Empty).GetHashCode(); ... h = h * 31 + (summonerId ?? string.Empty).GetHashCode();`

`string.GetHashCode()` em .NET/Mono **não tem garantia de estabilidade** entre processos (pode variar com randomização de hash por segurança) nem entre versões do runtime — isto contradiz o próprio propósito dessas 4 chamadas, que é gerar seeds **determinísticos** para stable-run.

O projeto já tem o algoritmo correto implementado e em uso em 3 lugares análogos: `CaveLayoutStableHash.Compute` (`Assets/_Game/Scripts/Cave/Generation/CaveLayoutStableHash.cs`), `QuestStableHash` (`Assets/_Game/Scripts/Quests/QuestStableHash.cs`), `ForageStableHash` (`Assets/_Game/Scripts/Farm/Forage/ForageStableHash.cs`) — todos FNV-1a 32-bit com offset `2166136261` / prime `16777619`, mesmo padrão.

## 6. Problema

`CaveEnemySpawner`, `CaveResourceNodeMaterializer` e `EnemyActionExecution.DeriveSummonSeed` usam `string.GetHashCode()` para semear RNG de conteúdo que a rule `cave-stable-run` exige ser **idêntico** ao revisitar o mesmo nível dentro do mesmo `CaveRunSeed`. Se o hash de string variar entre execuções (processo novo, versão de runtime diferente, ou simplesmente não documentado como estável pela Microsoft), o layout de inimigos/recursos/summons pode divergir silenciosamente — quebrando o invariante central da FASE9F sem nenhum erro visível.

## 7. Objetivo

Ao final desta spec, os 3 arquivos usam um hash FNV-1a estável (reusando `CaveLayoutStableHash.Compute` ou extraindo um `StableHash` comum, conforme decisão de Fase 0) em vez de `string.GetHashCode()`, preservando a composição exata das seed strings e a ordem das chamadas de RNG subsequentes.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/rules/cave-stable-run.md
.claude/skills/cave-stable-run-guard/SKILL.md
docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md (archived/historical — contexto de origem do invariante, não fonte canônica; a rule cave-stable-run.md é o canônico atual)
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão)

- `CaveLayoutStableHash.Compute(string value)` (Cave/Generation) já existe, é `public static int`, FNV-1a 32-bit, com guard `hash == int.MinValue ? 0 : hash` (evita `System.Random` receber `int.MinValue`, que causa comportamento degenerado em alguns overloads). Está em `CindarsHope.Cave.Generation`.
- `CaveEnemySpawner` está em `CindarsHope.Cave.Runtime` (ou namespace equivalente sob `Cave/Runtime`) — Fase 0 de execução deve confirmar se referenciar `Cave.Generation` a partir de `Cave.Runtime` é aceitável (checar se já há outras refs cruzadas Runtime→Generation no projeto; se sim, reusar `CaveLayoutStableHash.Compute` diretamente é o caminho de menor código, conforme a escada de minimalismo). Se houver ciclo de dependência de assembly ou motivo de isolamento, avaliar extrair um `StableHash` comum em `Core` (ex.: `CindarsHope.Core.StableHash`) que `CaveLayoutStableHash`, `QuestStableHash` e `ForageStableHash` podem futuramente delegar — mas **não é obrigatório fazer essa extração nesta spec** (YAGNI: só extrair se o wiring direto não funcionar).
- `EnemyActionExecution.DeriveSummonSeed` (Enemy namespace) não usa `Cave.Generation` hoje — mesma decisão de referenciar `CaveLayoutStableHash` ou usar hash local se cross-namespace for indesejado.
- As 3 seed strings devem permanecer **byte-a-byte idênticas** (mesma interpolação, mesma ordem de concatenação) — só a função de hash muda.
- **Quebra de compatibilidade intencional e documentada:** trocar a função de hash muda o valor do seed derivado para QUALQUER seed string já usada em uma run ativa. Um save com uma cave run em andamento (`CaveRunSeed` fixado) que seja recarregado após esta mudança fará `CaveEnemySpawner`/`CaveResourceNodeMaterializer` produzirem um `System.Random` com seed numérico diferente do que gerou o layout original — ou seja, um nível já materializado e revisitado terá enemy/resource composition diferente da vez anterior, mesmo dentro do mesmo `CaveRunSeed`. Isto é uma violação pontual e única do invariante da rule `cave-stable-run`, não um reroll silencioso recorrente em runtime: acontece exatamente uma vez, no primeiro load pós-deploy desta mudança, e nunca mais depois disso (a partir daí o hash é estável de verdade). Documentar esta troca explicitamente no execution report e, se o projeto tiver um changelog de saves/compat, registrar lá também.

## 10. User stories / engineering stories

```text
Como sistema de cave procedural, quero que o mesmo CaveRunSeed + nível sempre produza o mesmo hash de seed, para o stable-run contract ser real e não apenas nominal.
Como desenvolvedor, quero reusar o FNV-1a já testado em vez de duplicar o algoritmo pela quarta vez.
```

## 11. Escopo

Inclui:
- Substituir `seedString.GetHashCode()` em `CaveEnemySpawner.cs:66` por `CaveLayoutStableHash.Compute(seedString)` (ou hash local equivalente, decisão de Fase 0).
- Substituir `spawnSeedString.GetHashCode()` em `CaveResourceNodeMaterializer.cs:78` pelo mesmo padrão.
- Substituir `seedString.GetHashCode()` em `CaveResourceNodeMaterializer.cs:347` pelo mesmo padrão.
- Substituir as duas chamadas `.GetHashCode()` em `EnemyActionExecution.DeriveSummonSeed` (L134, L136) pelo mesmo padrão — atenção: aqui o hash é combinado manualmente (`h = h * 31 + ...`), então a troca é apenas do hash de cada string individual, mantendo a fórmula de combinação `h * 31 + x` intacta.
- Adicionar comentário citando a rule `cave-stable-run` no ponto de substituição (mesmo padrão de comentário já usado em `CaveResourceNodeMaterializer.cs:75-76`).
- EditMode tests: para cada ponto substituído (ou para a função de hash reusada, se centralizada), golden values fixos — mesmo input produz mesmo hash em execuções repetidas do teste.

Fora:
- Extrair um `StableHash` comum em Core (só se necessário por conflito de assembly/namespace — documentar a decisão de qualquer forma).
- Mudar a composição das seed strings.
- Persistência de snapshot completo (limitação conhecida, fora de escopo — próxima spec de SPEC 14 se necessário).

## 12. Fora de escopo

```text
Não inclui: mudança de algoritmo FNV-1a; refactor de CaveLayoutStableHash; snapshot completo de stable-run; UI nova.
```

## 13. Regras de não duplicação

```text
Não escrever uma quarta implementação de FNV-1a — reusar CaveLayoutStableHash.Compute (ou, apenas se inevitável por motivo de assembly/namespace documentado, replicar via chamada compartilhada, nunca copiar o corpo do algoritmo).
```

## 14. Critérios de aceite

### 14.1 Hash estável nos 3 arquivos

- `CaveEnemySpawner.cs`, `CaveResourceNodeMaterializer.cs` (2 pontos) e `EnemyActionExecution.cs` (2 chamadas em `DeriveSummonSeed`) não usam mais `string.GetHashCode()`.
- Todos usam a mesma função de hash determinística (FNV-1a via `CaveLayoutStableHash.Compute` ou equivalente).
- Evidência esperada: leitura do código (zero ocorrências de `.GetHashCode()` nesses 3 arquivos, exceto se algum outro uso não relacionado a seed existir — Grep confirma).

### 14.2 Seed strings preservadas

- A composição/ordem de concatenação de cada seed string permanece idêntica ao código original (diff mostra só a chamada de hash trocada, não a string).

### 14.3 Determinismo verificável

- EditMode test comprova: mesmo input de string → mesmo valor de hash em execuções repetidas (golden value fixo no teste, não recomputado dinamicamente).
- `dotnet build` PASS.

## 15. Notas sobre a quebra de seed (documentar, não implementar migration)

```text
Esta spec não implementa migration de saves — apenas documenta no execution report que a troca do algoritmo de hash é uma quebra intencional e única de compatibilidade de seed para runs de cave já em andamento no momento do deploy. Não é responsabilidade desta spec adicionar um SaveMigrationService para isso, dado que o próprio conteúdo materializado da run (GameObjects em cena) não é persistido hoje pelo save — apenas o CaveRunSeed numérico é. Confirmar essa afirmação na Fase 0 de execução (auditar o que o save de cave realmente persiste) antes de assumir que nenhuma migration é necessária; se o save persistir posições/IDs já materializados com a seed antiga, reportar como risco residual, não silenciar.
```

---

# /speckit.plan

## 16. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Runtime/
  CaveEnemySpawner.cs              (troca de hash)
  CaveResourceNodeMaterializer.cs  (troca de hash, 2 pontos)

Assets/_Game/Scripts/Enemy/
  EnemyActionExecution.cs          (troca de hash em DeriveSummonSeed)

Assets/_Game/Tests/EditMode/Cave/
  CaveStableHashUsageTests.cs (novo, ou extensão de teste existente de CaveLayoutStableHash)
```

## 17. Contratos, dados e eventos

### 17.1 Data contracts
Nenhum novo. Reusa `CaveLayoutStableHash` existente.

### 17.2 Runtime contracts
Assinaturas públicas de `CaveEnemySpawner`, `CaveResourceNodeMaterializer` e `EnemyActionExecution.DeriveSummonSeed` inalteradas — só a implementação interna do hash muda.

### 17.3 Event contracts
N/A — nenhum evento novo ou alterado.

### 17.4 Save contracts
N/A — nenhum DTO novo. Ver seção 15 sobre a quebra de seed (documentar, não migrar).

### 17.5 UI contracts
N/A.

## 18. Sistemas afetados

```text
Cave (enemy spawn, resource node materialization)
Enemy (summon seed derivation)
```

## 19. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawner.cs
Assets/_Game/Scripts/Cave/Runtime/CaveResourceNodeMaterializer.cs
Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs
Assets/_Game/Tests/EditMode/Cave/**
Assets/_Game/Tests/EditMode/Enemy/**
docs/validation/**
```

## 20. Arquivos proibidos

```text
Assets/_Game/Scripts/Cave/Generation/CaveLayoutStableHash.cs (leitura ok, edição não — a menos que a Fase 0 confirme necessidade de extração comum, e mesmo assim documentar antes de tocar)
Assets/**/*.unity, *.prefab, *.asset
Assets/_Game/Scripts/Save/**
```

## 21. Estratégia de implementação

```md
### Fase 0 — Confirmar acesso cross-namespace a CaveLayoutStableHash.Compute a partir de Cave/Runtime e Enemy; auditar o que o save de cave persiste (ver seção 15)
### Fase 1 — Substituir hash em CaveEnemySpawner.cs
### Fase 2 — Substituir hash em CaveResourceNodeMaterializer.cs (2 pontos)
### Fase 3 — Substituir hash em EnemyActionExecution.DeriveSummonSeed (2 chamadas)
### Fase 4 — EditMode tests (golden values)
### Fase 5 — dotnet build + relatório (citando a quebra de seed documentada)
```

## 22. Ordem de execucao (ordem segura)

```text
1. Ler .claude/rules/cave-stable-run.md e confirmar entendimento do invariante.
2. Auditar acesso cross-namespace e persistência de save de cave run (Fase 0).
3. Trocar hash nos 3 arquivos, preservando seed strings e ordem de RNG.
4. Escrever EditMode tests com golden values.
5. dotnet build + EditMode tests.
6. Registrar relatório citando a quebra de seed intencional e única.
```

## 23. Paralelização

```md
- Parallelizable: YES
- Parallel group: codex_convergence_lote2
- Can run with: demais specs deste lote (arquivos não se sobrepõem)
- Must not run with: N/A
- Shared files/systems that require lock: Cave/Runtime/CaveEnemySpawner.cs, Cave/Runtime/CaveResourceNodeMaterializer.cs, Enemy/EnemyActionExecution.cs (lock local)
- Reason: escopo isolado a 3 arquivos; único ponto de contato externo é uma função pura já estável (CaveLayoutStableHash.Compute)
```

## 24. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO (mas ver seção 15 — risco residual de conteúdo materializado divergente em runs ativas, documentar)
Does this persist Unity references? N/A
```

## 25. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 26. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (stable-run é um invariante gameplay-crítico; cenário humano deve confirmar revisita de nível sem reroll)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 27. Riscos técnicos

```text
Risco: quebra de compatibilidade de seed para runs de cave já em andamento no momento do deploy (ver seção 15).
Mitigação: documentar explicitamente no execution report; confirmar via Fase 0 que o save não persiste conteúdo materializado (só o CaveRunSeed numérico), reduzindo o impacto a "layout pode diferir uma vez, sem corromper o save em si".

Risco: acesso cross-namespace de Cave/Runtime e Enemy a Cave/Generation pode não existir hoje e introduzir acoplamento novo.
Mitigação: se indesejado, replicar a função de hash como método privado idêntico em cada arquivo (não é DRY perfeito, mas evita acoplamento de assembly não intencional) — documentar a escolha.
```

## 28. Rollback

```text
Reverter as 3 substituições para string.GetHashCode() (não recomendado, mas mecanicamente reversível).
Remover testes novos.
```

---

# /speckit.tasks

## 29. Tasks

```md
- [ ] T001 — Auditar acesso cross-namespace e persistência de save de cave (Fase 0).
- [ ] T002 — Trocar hash em CaveEnemySpawner.cs:66.
- [ ] T003 — Trocar hash em CaveResourceNodeMaterializer.cs:78 e :347.
- [ ] T004 — Trocar hash em EnemyActionExecution.cs:134,136 (DeriveSummonSeed).
- [ ] T005 — Escrever EditMode tests com golden values para cada ponto (ou para a função de hash reusada).
- [ ] T006 — dotnet build + EditMode tests; gerar execution report citando a quebra de seed intencional e única.
```

## 30. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Test Runner — EditMode: rodar se praticável; senão `NOT RUN` com motivo.

## 31. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (função de hash usada para seeds)
- Requires EditMode tests: YES (golden values para cada ponto de substituição)
- Requires PlayMode automated or final human scenario: YES (revisitar nível na mesma run e confirmar ausência de reroll)
- Requires regression test: YES (garantir que layout/enemy/resource não mudam ao revisitar, dentro da MESMA execução pós-deploy)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode tests com golden values PASS + cenário de Play Mode documentado (entrar em um nível, sair, reentrar, confirmar composição idêntica).
```

## 32. Definition of Done

```text
Os 3 arquivos usam hash FNV-1a estável em vez de string.GetHashCode().
Seed strings e ordem de RNG preservadas byte-a-byte.
EditMode tests com golden values criados e passando.
Execution report documenta a quebra de seed intencional e única para runs ativas no momento do deploy.
```

## 33. Anti-regressão

```text
Não alterar a composição das seed strings (mesma interpolação, mesma ordem).
Não alterar CaveLayoutStableHash.Compute em si (algoritmo intocado).
Não introduzir GUID/timestamp em nenhum ponto (violação direta da rule cave-stable-run).
```

## 34. Notas para execução posterior

```text
Se a Fase 0 revelar que o save de cave persiste mais do que o CaveRunSeed numérico (ex.: posições/IDs já materializados), registrar isso como gap para uma spec futura de save migration — não implementar aqui (fora de escopo).
```
