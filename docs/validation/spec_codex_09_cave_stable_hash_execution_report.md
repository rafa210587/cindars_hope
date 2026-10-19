# Execution Report — spec_codex_09_cave_stable_hash

> **Spec:** `.specs/a_implementar/spec_codex_09_cave_stable_hash.md`
> **Status:** BUILD_VALIDATED (Phase 2-3 DEFERRED_TO_FINAL_HUMAN_VALIDATION)
> **Type:** Runtime / Determinism fix
> **Date:** 2026-07-03

---

## Acceptance criteria extracted

Da seção "14. Critérios de aceite" da spec:

| # | Critério | Evidência |
|---|---|---|
| 14.1 | Hash estável nos 3 arquivos — `CaveEnemySpawner.cs`, `CaveResourceNodeMaterializer.cs` (2 pontos) e `EnemyActionExecution.cs` (2 chamadas em `DeriveSummonSeed`) não usam mais `string.GetHashCode()`; todos usam a mesma função de hash determinística | Grep pós-mudança confirma zero ocorrências de `.GetHashCode()` como chamada de código nos 3 arquivos (só permanecem em comentários explicativos). Todos os 5 pontos agora chamam `CaveLayoutStableHash.Compute(...)` |
| 14.2 | Seed strings preservadas — composição/ordem de concatenação de cada seed string idêntica ao original | Confirmado por diff: nenhuma seed string foi alterada (`"{world}_{run}_{level}_enemies"`, `"{world}_{run}_{level}_resource_spawn"`, `"{world}_{run}_{level}_resources_{spawnIndex}_{x}_{y}"`, e a fórmula de combinação `h * 31 + x` em `DeriveSummonSeed`); só a chamada de hash trocou |
| 14.3 | Determinismo verificável — EditMode test comprova mesmo input → mesmo hash em execuções repetidas, com golden value fixo; `dotnet build` PASS | `Assets/_Game/Tests/EditMode/Cave/CaveStableHashUsageTests.cs` (9 testes) com golden values literais fixos (não recomputados dinamicamente) + testes de determinismo cross-call. `dotnet build` Assembly-CSharp e Assembly-CSharp-Editor: PASS, exit 0 |

---

## Existing systems audit (Phase 0)

- `CaveLayoutStableHash.Compute(string value)` (`Assets/_Game/Scripts/Cave/Generation/CaveLayoutStableHash.cs`) já existia, `public static int`, FNV-1a 32-bit (offset `2166136261`, prime `16777619`), com guard `hash == int.MinValue ? 0 : hash`. Namespace `CindarsHope.Cave.Generation`. Algoritmo **não foi alterado** nesta spec.
- **Cross-namespace já existente, sem risco novo:** `CaveEnemySpawner.cs` (namespace `CindarsHope.Cave.Runtime`) já importava `using CindarsHope.Cave.Generation;` (linha 5, pré-existente) — usado para `EnemyScaleResolver`/outros. `CaveResourceNodeMaterializer.cs` (mesmo namespace `Cave.Runtime`) também já importava `Cave.Generation` (linha 3, pré-existente). Portanto, reusar `CaveLayoutStableHash.Compute` nesses dois arquivos não introduziu nenhum acoplamento novo — a referência cruzada Runtime→Generation já era um padrão estabelecido no projeto.
- `EnemyActionExecution.cs` (namespace `CindarsHope.Enemy`) **não** importava `Cave.Generation` antes desta spec, mas o namespace `Enemy` já depende de `Cave` em outros arquivos do mesmo diretório (`EnemyActionRunner.cs`, `EliteAffix.cs`, `EnemyPackCoordinator.cs` já importam `Cave.*`), e não há import reverso de `Enemy` a partir de `Cave.Generation` (grep confirmado: zero arquivos em `Cave/Generation` importam `CindarsHope.Enemy`). Um `using CindarsHope.Cave.Generation;` foi adicionado a `EnemyActionExecution.cs` sem risco de ciclo — ambos compilam na mesma assembly (`Assembly-CSharp`), então não há sequenciamento de build a proteger, só uma dependência de namespace, já precedente no projeto.
- **Decisão de Fase 0:** reuso direto de `CaveLayoutStableHash.Compute` nos 3 arquivos, sem extração de um `StableHash` comum em `Core` — confirmado desnecessário (escada de minimalismo: YAGNI), já que o acesso cross-namespace já é viável e precedente.
- **Achado adicional relevante (não estava explícito na seção 9 da spec):** existe um **quarto local** com uma implementação FNV-1a **duplicada** e não tocada por esta spec: `CaveEnemySpawnPlanner.StableHash` (`Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs:244`), usado pelo caminho de produção mais recente (`CaveEnemyMaterializer`/`CaveEnemySpawnPlanner`/`CaveEnemySpawnPlanService`, que já usa hash estável — não `GetHashCode()`). O `CaveEnemySpawner.cs` (arquivo alvo desta spec) é um spawner **legado**, ainda ativamente referenciado (serialized field em `CaveLevelRuntimeController.cs:17` e usado por `CaveSmokeTestSpawnerBridge.cs`/`CreateMvpCaveScene.cs`), mas não é o único ponto de spawn de inimigos do projeto. Esta spec corrigiu exatamente os 3 arquivos no scope declarado; a duplicação de FNV-1a em `CaveEnemySpawnPlanner.StableHash` está **fora de escopo** (a spec não pediu consolidar essa quarta implementação, e ela já não usa `GetHashCode()` — não é o bug que esta spec resolve). Não modificado.
- **Auditoria de persistência de save da cave (seção 15 da spec — obrigatória confirmar antes de assumir "nenhuma migration necessária"):** a suposição da spec ("o save só persiste o `CaveRunSeed` numérico") **está incorreta** para o estado atual do código. `CaveRunSaveData` (`Assets/_Game/Scripts/Cave/Runtime/CaveRunSaveData.cs`) persiste, via `VisitedLevelSnapshots` (fable_44, cap LRU de 8 níveis), o **conteúdo materializado completo** de cada nível visitado: `EnemySpawnPlan`/`EnemySpawns`, `ResourceNodes`/`ResourceNodeStates`, `LayoutHash`, posições de entrada/saída, tiles walkable/wall, estado de baús/armadilhas/fog-of-war/conflito, etc. (`VisitedLevelSnapshot.cs`, ~60 campos). Isso muda a análise de risco da spec:
  - Para níveis **com snapshot já persistido** no save (dentro do cap LRU de 8), a materialização em revisita usa o **snapshot restaurado diretamente** (`MaterializeResourceNodesFromSnapshot`, confirmado em `CaveResourceNodeMaterializer.cs:69-73` — o branch de `snapshotResourceNodeStates` roda **antes** do branch que usa `System.Random(...GetHashCode()/Compute(...))`), então a troca de hash **não afeta** o conteúdo desses níveis ao recarregar — o snapshot é a fonte de verdade, não o RNG re-derivado.
  - O risco real e residual desta mudança fica limitado a: (a) níveis que **nunca tiveram snapshot salvo** (fora do cap LRU, ou visitados e nunca persistidos antes do save mais recente) — nesses, a re-entrada pós-deploy vai re-rolar com o hash novo, produzindo composição diferente da vez anterior (quebra pontual, documentada); (b) o `EnemyActionExecution.DeriveSummonSeed`, cujo resultado (posições de summon de adds) **não é persistido em snapshot** — é recalculado toda vez que um summon dispara, então o efeito da troca de hash aqui é: adds invocados **após o deploy** terão posições diferentes das que teriam com o hash antigo, mas isso não quebra nenhum invariante de revisita (a fórmula ainda é determinística por `CaveRunSeed + caveLevel + summonerId`, só o valor numérico final mudou uma vez).
  - Conclusão: **nenhuma migration de save é necessária** (confirmando a spec), mas pelo motivo mais preciso de "o snapshot já persistido blinda o conteúdo materializado", não porque "o save só guarda o seed numérico" — essa segunda afirmação da spec está desatualizada frente ao fable_44/multi-level snapshot já implementado. Registrado aqui como correção de entendimento, não como bloqueio.

Nenhum sistema paralelo de hash foi criado — todos os 3 pontos reusam `CaveLayoutStableHash.Compute` já existente.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|---|---|---|
| `CaveEnemySpawner.cs:66` — trocar `seedString.GetHashCode()` (11. Escopo) | `new System.Random(CaveLayoutStableHash.Compute(seedString))` | OK |
| `CaveResourceNodeMaterializer.cs:78` — trocar `spawnSeedString.GetHashCode()` (11. Escopo) | `new System.Random(CaveLayoutStableHash.Compute(spawnSeedString))` | OK |
| `CaveResourceNodeMaterializer.cs:347` — trocar `seedString.GetHashCode()` (11. Escopo) | `new System.Random(CaveLayoutStableHash.Compute(seedString))` | OK |
| `EnemyActionExecution.cs:134,136` — trocar as 2 chamadas `.GetHashCode()` em `DeriveSummonSeed`, preservando `h * 31 + x` (11. Escopo) | `h = h * 31 + CaveLayoutStableHash.Compute(caveRunSeed ?? string.Empty);` / `h = h * 31 + CaveLayoutStableHash.Compute(summonerId ?? string.Empty);` — fórmula de combinação intacta | OK |
| Comentário citando a rule `cave-stable-run` no ponto de substituição (11. Escopo) | Comentários adicionados nos 4 pontos citando `spec_codex_09` / `cave-stable-run` | OK |
| EditMode tests com golden values fixos para cada ponto/função reusada (11. Escopo, 14.3) | `CaveStableHashUsageTests.cs` — 9 testes: 4 golden values literais (`CaveLayoutStableHash.Compute` para as 3 seed strings + string vazia), 2 testes de determinismo cross-call, 1 teste de golden value combinado para `DeriveSummonSeed` (fórmula `h*31+x` recomputada manualmente no teste), 1 teste de diferenciação por nível, 1 teste de diferenciação por summoner, 1 teste de null-safety | OK |
| Não mudar composição das seed strings (13. Regras de não duplicação / 32. Anti-regressão) | Confirmado por diff — só a chamada de hash mudou em cada linha | PRESERVED |
| Não mudar `CaveLayoutStableHash.Compute` em si (32. Anti-regressão) | Arquivo não tocado (só leitura) | PRESERVED |
| Não introduzir GUID/timestamp (32. Anti-regressão) | Nenhum `Guid`/`DateTime` introduzido | PRESERVED |
| Documentar quebra de seed intencional e única (15., 18. Notas) | Ver seção "Quebra de seed — nota obrigatória" abaixo | OK |
| Auditar persistência de save de cave antes de assumir "nenhuma migration necessária" (Fase 0, seção 15) | Ver "Existing systems audit" acima — achado corrige a premissa da spec (snapshot já persiste conteúdo materializado, não só o seed numérico), mas confirma a mesma conclusão (nenhuma migration necessária) por um motivo mais preciso | OK |

---

## Files changed

- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawner.cs` — trocada a chamada de hash em `SpawnEnemiesForLevel` (linha ~66); comentário adicionado citando `spec_codex_09`/`cave-stable-run`.
- `Assets/_Game/Scripts/Cave/Runtime/CaveResourceNodeMaterializer.cs` — trocadas as 2 chamadas de hash (`MaterializeResourceNodes` linha ~78 e `SelectResourceNodeData` linha ~347); comentários adicionados.
- `Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs` — adicionado `using CindarsHope.Cave.Generation;`; trocadas as 2 chamadas de hash em `DeriveSummonSeed` (linhas ~134, ~136), preservando a fórmula `h * 31 + x`; comentário adicionado.
- `Assets/_Game/Tests/EditMode/Cave/CaveStableHashUsageTests.cs` (novo) — 9 testes EditMode com golden values fixos para os pontos de hash tocados e para `DeriveSummonSeed`.
- `Assembly-CSharp.csproj` (modificado) — adicionada a linha `<Compile Include="Assets\_Game\Tests\EditMode\Cave\CaveStableHashUsageTests.cs" />` (necessária: o `.csproj` do projeto usa includes explícitos por arquivo, não glob; sem essa linha o novo teste não compila nem é descoberto pelo Test Runner, mesmo com `dotnet build` retornando sucesso silenciosamente ao ignorar o arquivo). Confirmado antes/depois: build sem a linha compilava 0 erros mas **não incluía** o arquivo novo; com a linha, compila 0 erros e o arquivo é parte da assembly.
- `docs/validation/spec_codex_09_cave_stable_hash_execution_report.md` (este arquivo, novo).

---

## Quebra de seed — nota obrigatória (seção 15/18 da spec)

Trocar `string.GetHashCode()` por `CaveLayoutStableHash.Compute()` muda o valor numérico do seed derivado para **qualquer seed string já usada antes deste deploy**. Isto é uma **quebra de compatibilidade intencional e única**, não um reroll silencioso recorrente:

- Acontece exatamente uma vez — no primeiro load/materialização pós-deploy desta mudança.
- Depois disso, o hash é estável de verdade (FNV-1a determinístico entre processos/versões de runtime), então o invariante `cave-stable-run` passa a valer de fato, não só nominalmente.
- **Escopo real do impacto** (corrigindo a premissa original da seção 15 da spec — ver "Existing systems audit" acima): níveis com snapshot já persistido (`VisitedLevelSnapshots`, cap LRU de 8) **não são afetados** — a materialização usa o snapshot, não o RNG re-derivado. O impacto real fica restrito a (a) níveis sem snapshot ainda salvo e (b) posições de summon de adds (`DeriveSummonSeed`, nunca persistidas em snapshot), que terão composição/posições diferentes da vez anterior na primeira re-entrada/summon pós-deploy.
- Nenhuma migration de save foi implementada — confirmado desnecessário, pois o conteúdo materializado que precisa permanecer estável (para saves com snapshot) já está protegido pelo mecanismo de snapshot existente, independente da função de hash usada para gerar conteúdo *novo*.

---

## Validation

```text
Validation method: run_strict_validation.ps1 (2 execuções — antes e depois da correção do .csproj)
Exit code: 1 (QUALITY_CHECK_FAILURE — ver detalhe abaixo)
Assembly-CSharp: PASS (0 erros, warnings pré-existentes não relacionados)
Assembly-CSharp-Editor: PASS (0 erros, warnings pré-existentes não relacionados)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY
Quality check: FAIL — mas 100% dos erros reportados pertencem ao conjunto legado pré-declarado
  (spec_npc_physics_cat_companion.md sem markers speckit; placeholders em tools/codex/Generate-CodexHarness.ps1;
  Enemies/Roster + 3 cenas pré-sessão; ~50+ execution reports antigos sem as 5 seções obrigatórias, de
  waves anteriores a esta spec). Confirmado via grep no log completo: ZERO menções a CaveEnemySpawner.cs,
  CaveResourceNodeMaterializer.cs, EnemyActionExecution.cs ou CaveStableHashUsageTests.cs em qualquer erro.
Result artifact: N/A (script não gerou LAST_STRICT_VALIDATION_RESULT.json nesta execução — só grep manual do
  log, capturado em C:\Users\Rafa\...\Temp\strict_validation_out2.txt)
```

**dotnet build isolado (evidência adicional, pós-fix do `.csproj`):**

```text
dotnet build .\Assembly-CSharp.csproj --no-restore       → exit 0, "Compilação com êxito.", 0 Erro(s)
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore → exit 0, "Compilação com êxito.", 0 Erro(s)
```

Unity Test Runner (EditMode, execução real dentro do Editor): **NOT RUN** — Unity Editor não foi aberto nesta sessão (ambiente sandbox sem GUI do Editor). Os 9 testes novos foram verificados por: (a) compilação limpa via `dotnet build` incluindo o arquivo no `.csproj`; (b) os golden values foram calculados de forma independente via um pequeno programa C# console (`unchecked` FNV-1a idêntico ao algoritmo de `CaveLayoutStableHash.Compute`, rodado fora do projeto Unity) e conferidos manualmente contra os literais usados nos testes — não são "chutados", são o resultado real do algoritmo para os inputs escolhidos.

## Testing Quality Gate

```text
Changed runtime code:           YES
Changed deterministic logic:    YES (função de hash usada para seeds de RNG determinístico)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — Assets/_Game/Tests/EditMode/Cave/CaveStableHashUsageTests.cs (9 testes)
Automated tests command:        dotnet build .\Assembly-CSharp.csproj --no-restore (compila os testes;
                                 execução real via Unity Test Runner NOT RUN nesta sessão — ver acima)
Manual Play Mode scenario:      NOT RUN nesta sessão — exigido pela spec (seção 26, 31) antes de ACCEPTED.
                                 Cenário: entrar em um nível da cave, sair, reentrar na mesma run
                                 (mesmo CaveRunSeed) e confirmar que layout/inimigos/recursos permanecem
                                 idênticos (nenhum reroll visível). Path sugerido para a próxima etapa:
                                 docs/validation/playmode/spec_codex_09_cave_stable_hash_human_test_scenario.md
                                 (NÃO criado nesta execução — Phase 2-3 deferida, ver Honest status rationale)
Justification if no tests:      N/A (testes foram criados)
Residual risk:                  Ver seção "Quebra de seed — nota obrigatória" acima. Adicionalmente: os golden
                                 values do teste são literais fixos calculados fora do Unity Editor via um
                                 programa C# console equivalente ao algoritmo real; se o algoritmo de
                                 CaveLayoutStableHash.Compute for alterado no futuro (fora do escopo desta
                                 spec — proibido pela seção 32), estes testes vão falhar corretamente (não são
                                 tautológicos, pois os valores são fixos, não recomputados a partir da própria
                                 função sob teste).
```

---

## Honest status rationale

- **BUILD_VALIDATED** é o status correto para o núcleo desta spec: os 4 pontos de troca de hash estão implementados, preservando seed strings e a fórmula `h*31+x`; `dotnet build` PASS (0 erros) nas duas assemblies; EditMode tests com golden values fixos criados e compilando corretamente (incluindo a correção do `.csproj`, sem a qual o teste teria sido silenciosamente ignorado); nenhum arquivo fora do scope permitido foi tocado; nenhum sistema paralelo de hash foi criado.
- **NÃO** reivindico `ACCEPTED`/`PLAYMODE_VALIDATED`: a spec (seções 26, 31) exige explicitamente um cenário de Play Mode (humano ou automatizado) confirmando que revisitar um nível na mesma run não causa reroll — isso não foi executado nesta sessão (sem acesso ao Unity Editor rodando). Também não executei o Unity Test Runner real (só compilação via `dotnet build`), então os 9 testes novos estão **compilando corretamente e com valores golden verificados externamente**, mas não têm uma execução `PASS` do NUnit real registrada nesta sessão.
- A falha de `run_strict_validation.ps1` (exit 1, `QUALITY_CHECK_FAILURE`) é 100% atribuível ao conjunto legado pré-declarado no prompt do orquestrador (specs/relatórios antigos sem seções obrigatórias, `spec_npc_physics_cat_companion.md`, placeholders em `tools/codex/`, Enemies/Roster + 3 cenas pré-sessão) — confirmado por grep explícito no log completo não retornando nenhuma menção aos 4 arquivos desta spec.
- Correção de entendimento registrada (não bloqueia esta spec, mas é relevante para specs futuras de save/cave): a premissa da seção 15 da spec de que "o save só persiste o CaveRunSeed numérico" está desatualizada — o save já persiste snapshots completos de nível (fable_44). Isso não muda a conclusão da spec (nenhuma migration necessária), mas muda o motivo, e reduz o escopo real do risco residual (só afeta níveis sem snapshot e summons futuros, não o conteúdo já persistido).

---

## Remaining work

1. **Phase 2-3 (Play Mode):** executar o cenário humano/automatizado descrito na seção 26/31 da spec (entrar → sair → reentrar no mesmo nível/run, confirmar ausência de reroll). Path sugerido: `docs/validation/playmode/spec_codex_09_cave_stable_hash_human_test_scenario.md` — não criado nesta execução; a spec já documenta o cenário esperado nas próprias seções 26 e 31, então a criação do arquivo formal de checklist é o único passo que falta antes de rodar o teste humano.
2. **Unity Test Runner real:** rodar `CaveStableHashUsageTests` dentro do Editor (EditMode) para ter uma execução `PASS` registrada do NUnit real, não só compilação via `dotnet build`.
3. Nenhum trabalho de migration de save é necessário (confirmado, ver seção "Existing systems audit").
4. Fora de escopo desta spec, achado incidental para consideração futura (não uma ação obrigatória): `CaveEnemySpawnPlanner.StableHash` é uma quarta implementação duplicada de FNV-1a — já não usa `GetHashCode()`, então não é um bug, mas poderia ser consolidada com `CaveLayoutStableHash.Compute` numa spec futura de redução de duplicação, se o projeto decidir que vale o refactor.

---

## Dependency Chain

```text
Original target: spec_codex_09_cave_stable_hash
Dependency chain: nenhuma dependência de outra spec do lote (spec depende apenas de CaveLayoutStableHash.Compute, já existente)
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: N/A (esta já É a spec original; nenhuma cadeia foi aberta)
```
