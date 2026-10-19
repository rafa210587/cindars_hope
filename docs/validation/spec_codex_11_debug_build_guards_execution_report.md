# Execution Report — spec_codex_11_debug_build_guards

> **Spec:** `.specs/a_implementar/spec_codex_11_debug_build_guards.md`
> **Status:** BUILD_VALIDATED (Phase 2-3 DEFERRED_TO_FINAL_HUMAN_VALIDATION)
> **Type:** Runtime / Build guard fix
> **Date:** 2026-07-03

---

## Acceptance criteria extracted

Da secao "14. Criterios de aceite" da spec:

| # | Criterio | Evidencia |
|---|---|---|
| 14.1 | `CollisionDebugOverlayBootstrap` guardado — fora de `UNITY_EDITOR`/`DEVELOPMENT_BUILD`, `Init()` e no-op; dentro de Editor/Dev Build, comportamento inalterado | `Init()` inteiro (corpo do metodo) envolto em `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD ... #endif`; leitura de codigo confirma que fora dessas condicoes o metodo nao executa nenhuma instrucao |
| 14.2 | `CaveDebugLevelSkipController` seguro por padrao e por build — `_enableDebugLevelSkip` default `false`; logica de skip (tecla + execucao + botao) guardada pela mesma diretiva | Default trocado de `true` para `false` (linha do campo serializado); corpo de `Update()` e `OnGUI()` envolto em `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD ... #endif` |
| 14.3 | Sem regressao em Editor/Development Build — `dotnet build` PASS | `dotnet build .\Assembly-CSharp.csproj --no-restore` e `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`: ambos exit 0, 0 Erro(s), 0 Aviso(s) |

---

## Existing systems audit

- **Phase 0 — Grep de `RuntimeInitializeOnLoadMethod` sem guard:**
  - Em `Assets/_Game/Scripts/DebugTools/`: apenas `CollisionDebugOverlayBootstrap.cs` usa o atributo (1 arquivo, o proprio alvo desta spec). Nenhum outro arquivo de debug tooling sem guard encontrado nesta pasta.
  - Em `Assets/_Game/Scripts/Cave/`: 4 arquivos usam `RuntimeInitializeOnLoadMethod` — `CaveWanderingMerchant.cs`, `CaveConflictFeedbackBridge.cs`, `Death/DeathSystemBootstrap.cs`, `Cave/Runtime/CaveRuntimeBridge.cs`. Nenhum destes e "debug tooling" no sentido da spec (sao sistemas de producao: mercador errante, feedback de conflito, bootstrap de morte, bridge de runtime da cave) — fora do repo lock scope desta spec, nao tocados. Nenhum achado adicional de debug tooling sem guard fora dos 2 arquivos ja nomeados no escopo.
- **Confirmacao de consumidores de `IsDebugSkipEnabled` (Phase 0, antes de mudar o default):** unico consumidor encontrado e `Assets/_Game/Scripts/UI/DebugHud.cs:633`, que so le a propriedade para **exibir texto de status** ("enabled"/"disabled") no HUD IMGUI — nao usa o valor para gatear nenhuma logica de gameplay. Portanto, mudar o default de `true` para `false` e seguro: nenhum consumidor assume implicitamente `true` para funcionar.
- **Confirmacao do padrao local de guard `#if UNITY_EDITOR || DEVELOPMENT_BUILD`:** grep por esse padrao exato em `Assets/_Game/Scripts/**` nao encontrou nenhum precedente identico ja em uso (`Combat/Weapon/ProjectileSpawnService.cs` aparece no grep amplo mas por outro motivo/comentario, nao pelo padrao de guard exato). A spec autoriza usar a diretiva de compilacao padrao do Unity mesmo sem precedente exato local, por ser a convencao nativa da engine para este proposito (nao e criacao de um sistema de guard novo — e o mecanismo built-in do Unity/C# preprocessor). Aplicado nos 2 arquivos do scope.
- **Campos legados `_nextLevelKey`/`_alternateNextLevelKey` (`[HideInInspector]`):** confirmados ainda lidos por `SyncLegacySerializedFields()` (chamada em `OnValidate`, `Start` e `Update`) para manter compatibilidade com scene generators antigos. Nao removidos — fora do escopo minimo desta spec (guard + default), conforme a nota da propria spec (secao 33). Nao alterei essa logica de sincronizacao.
- **Excecao `FindAnyObjectByType` em `CollisionDebugOverlayBootstrap.cs`:** confirmado como uso legitimo sob `DebugTools/` (exceval documentada na rule `unity-architecture` para ferramentas de debug/editor, nao gameplay code). Documentado explicitamente com comentario no proprio arquivo (ver "Files changed" abaixo) — nao troquei por serialized ref, pois o padrao de auto-bootstrap dormente depende de detectar se uma instancia ja existe antes de criar outra.
- **`DebugHud` (IMGUI):** confirmado como HUD real do slice atual (gameplay-facing), nao ferramenta de debug descartavel — **nao tocado** nesta spec, conforme decisao explicita da propria spec (secoes 11, 19, 32). Unica interacao com `DebugHud.cs` nesta execucao foi leitura (grep de `IsDebugSkipEnabled`) para confirmar consumidores antes de mudar o default; nenhuma linha do arquivo foi editada.

Nenhum sistema novo de guard de build foi criado — usadas apenas as diretivas de compilacao padrao do Unity/C# (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`), conforme exigido pela secao 13 da spec.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|---|---|---|
| Guardar `CollisionDebugOverlayBootstrap.Init()` com `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD` (11. Escopo) | Corpo inteiro do metodo envolto na diretiva; fora de Editor/Dev Build o metodo executa zero instrucoes | OK |
| Trocar default de `_enableDebugLevelSkip` para `false` (11. Escopo) | `[SerializeField] private bool _enableDebugLevelSkip = false;` | OK |
| Guardar logica de skip (tecla + execucao + botao de debug) com a mesma diretiva (11. Escopo) | Corpo de `Update()` e `OnGUI()` envoltos em `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD ... #endif` | OK |
| Dupla protecao — default seguro + guard de compilacao independentes (14.2, 26. Riscos) | Confirmado: mesmo se `_enableDebugLevelSkip` for setado `true` manualmente em asset/prefab de producao, o guard de compilacao ainda impede a execucao fora de Editor/Dev Build | OK |
| Documentar excecao `FindAnyObjectByType` em `DebugTools/` (11. Escopo) | Comentario adicionado no corpo do metodo `Init()` explicando a excecao | OK |
| Varredura Grep por outros `*Debug*` sem guard, documentada sem expandir escopo (11. Escopo, 20. Fase 0) | Ver "Existing systems audit" acima — 4 arquivos com `RuntimeInitializeOnLoadMethod` em `Cave/` sao sistemas de producao, nao debug tooling; nenhum achado adicional que exigisse expandir o repo lock scope | OK |
| Nao tocar `DebugHud` (11. Fora, 19, 32. Anti-regressao) | Nenhuma linha de `DebugHud.cs` editada; apenas lido para auditoria de consumidor | PRESERVED |
| Nao remover `CollisionDebugOverlay` ou `CaveDebugLevelSkipController` (32. Anti-regressao) | Ambas as classes preservadas; apenas guard + default adicionados | PRESERVED |
| Nao criar novo sistema de guard (13. Regras de nao duplicacao) | Usada apenas a diretiva de compilacao nativa do Unity/C# preprocessor | PRESERVED |
| `dotnet build` PASS nas duas assemblies (14.3, 29. Validacoes obrigatorias) | Ver secao "Validation" abaixo | OK |

---

## Files changed

- `Assets/_Game/Scripts/DebugTools/CollisionDebugOverlayBootstrap.cs` — corpo de `Init()` envolto em `#if UNITY_EDITOR || DEVELOPMENT_BUILD ... #endif`; comentario adicionado documentando o build guard e a excecao `FindAnyObjectByType` (DebugTools, nao gameplay).
- `Assets/_Game/Scripts/Cave/Runtime/CaveDebugLevelSkipController.cs` — `_enableDebugLevelSkip` default trocado de `true` para `false`; corpo de `Update()` e `OnGUI()` envolto em `#if UNITY_EDITOR || DEVELOPMENT_BUILD ... #endif`; comentario adicionado explicando a dupla protecao (default + guard).
- `docs/validation/spec_codex_11_debug_build_guards_execution_report.md` (este arquivo, novo).

Nenhum outro arquivo foi tocado. `DebugHud.cs` e `CollisionDebugOverlay.cs` nao foram editados (fora de escopo/proibidos pela spec).

---

## Validation

```text
Validation method: run_strict_validation.ps1 (2 execucoes nesta sessao)
Exit code: 1 (QUALITY_CHECK_FAILURE) — ver detalhe abaixo
Assembly-CSharp: PASS (0 Erro(s), 0 Aviso(s))
Assembly-CSharp-Editor: PASS (0 Erro(s), 0 Aviso(s))
Docs validation: EXPECTED_FAIL_LEGACY_ONLY
Quality check: FAIL — mas 100% dos erros/warnings pertencem ao conjunto legado pre-declarado pelo
  orquestrador (spec_npc_physics_cat_companion.md sem markers speckit; placeholders em
  tools/codex/Generate-CodexHarness.ps1; Enemies/Roster + 3 cenas .asset/.unity ja modificadas antes
  desta sessao; ~50+ execution reports antigos de waves anteriores sem as 5 secoes obrigatorias).
  Confirmado via busca textual no log completo de "Forbidden files altered": a lista de arquivos citada e
  inteiramente Assets/_Game/Data/Enemies/Roster/*.asset (pre-existente, nao alterado por esta spec).
  Grep adicional por "CollisionDebugOverlayBootstrap" e "CaveDebugLevelSkipController" no log completo:
  ZERO ocorrencias — nenhum erro/warning cita os 2 arquivos alterados por esta spec.
Result artifact: N/A (log completo capturado manualmente em
  C:\Users\Rafa\AppData\Local\Temp\claude\...\scratchpad\strict_out2.log)
```

**dotnet build isolado (evidencia adicional):**

```text
dotnet build .\Assembly-CSharp.csproj --no-restore       -> exit 0, "Compilacao com exito.", 0 Erro(s), 0 Aviso(s)
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -> exit 0, "Compilacao com exito.", 0 Erro(s), 0 Aviso(s)
```

Unity batchmode / Play Mode: **NOT RUN** — Unity Editor nao foi aberto nesta sessao (ambiente sem GUI do Editor disponivel). Leitura de codigo confirma os guards corretos (ver Spec Compliance Matrix); comportamento real em build de producao vs. Development Build fica pendente do cenario humano documentado no Testing Quality Gate abaixo.

## Testing Quality Gate

```text
Changed runtime code:           YES (2 arquivos)
Changed deterministic logic:    NO — mudanca e diretiva de compilacao + default de campo serializado,
                                 nao logica testavel em EditMode da forma usual (conforme a propria
                                 spec, secao 15, ja antecipa)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  NO — residual risk documentado (ver abaixo), conforme Testing Quality
                                 Gate da propria spec (secao 30): "Requires EditMode tests: NO"
Manual Play Mode scenario:      REQUIRED, NOT RUN nesta sessao. Cenario (conforme secao 30/22 da spec):
                                 (a) gerar um build Standalone Release sem DEVELOPMENT_BUILD e confirmar
                                 ausencia de CollisionDebugOverlay na cena e do botao/skip de nivel da
                                 cave (tecla P/F2 sem efeito); (b) gerar/rodar em Development Build ou
                                 Play Mode do Editor e confirmar que ambas as ferramentas continuam
                                 funcionando identicamente a antes da mudanca.
Justification if no tests:      A garantia real desta spec vem de revisao de codigo (guards de
                                 compilacao corretos, confirmados por leitura) + o cenario de build de
                                 producao real, que e o unico jeito de provar que o `#if` realmente
                                 exclui o codigo do binario final — um EditMode test roda sempre dentro
                                 do contexto Editor (onde UNITY_EDITOR esta sempre definido), entao nao
                                 consegue exercitar o branch "fora de Editor/Dev Build" de forma
                                 significativa.
Residual risk:                  Ate a execucao do cenario de Play Mode/build real: risco residual de que
                                 o guard tenha sido colocado de forma sintaticamente correta mas nao
                                 cubra 100% do codigo executavel (ex.: um caminho de codigo remanescente
                                 fora do bloco #if). Mitigado por leitura cuidadosa do diff nesta sessao
                                 (o corpo inteiro de Init(), Update() e OnGUI() foi confirmado dentro do
                                 bloco #if em cada um dos 2 arquivos).
```

---

## Honest status rationale

- **BUILD_VALIDATED** e o status correto: os dois arquivos do repo lock scope foram guardados exatamente conforme os criterios de aceite (14.1, 14.2, 14.3); `dotnet build` PASS (0 erros, 0 avisos) nas duas assemblies; nenhum arquivo fora do scope permitido foi tocado; `DebugHud` explicitamente nao alterado; nenhum sistema paralelo de guard foi criado.
- **NAO** reivindico `ACCEPTED`/`PLAYMODE_VALIDATED`: a spec (secao 25, "Requires Play Mode final validation: YES") exige confirmar em build real que o overlay e o skip de nivel ficam ausentes fora de Editor/Development Build, e presentes dentro — isso nao foi executado nesta sessao (sem acesso a build pipeline/Unity Editor rodando).
- A falha de `run_strict_validation.ps1` (exit 1, `QUALITY_CHECK_FAILURE`) e inteiramente atribuivel ao conjunto legado pre-declarado no prompt do orquestrador (reports antigos sem secoes obrigatorias, spec de companion felino de fisica NPC sem markers speckit, placeholders em `tools/codex/`, `Enemies/Roster` + 3 cenas pre-sessao) — confirmado por busca textual explicita no log completo, que nao retorna nenhuma mencao aos 2 arquivos desta spec como causa da falha.
- Achado adicional documentado (nao bloqueia esta spec, apenas registrado conforme a secao 33 da propria spec): os campos legados `_nextLevelKey`/`_alternateNextLevelKey` em `CaveDebugLevelSkipController.cs` permanecem ativos via `SyncLegacySerializedFields()` — vestigiais para compatibilidade com scene generators antigos, candidatos a uma spec de limpeza futura, fora do escopo minimo desta spec (guard + default).

---

## Remaining work

1. **Phase 2-3 (Play Mode / build real):** gerar um build Standalone Release (sem `DEVELOPMENT_BUILD`) e confirmar ausencia do `CollisionDebugOverlay` na cena e do skip de nivel da cave (tecla P/F2 sem efeito, botao de debug ausente); em seguida confirmar que ambos continuam funcionando normalmente em Development Build/Play Mode do Editor. Nao executado nesta sessao (Phase 2-3 deferida).
2. Nenhum trabalho de migration de save e necessario (spec nao toca save schema — confirmado, secao 23).
3. Fora de escopo desta spec, achado incidental para consideracao futura: os campos legados `_nextLevelKey`/`_alternateNextLevelKey` (`[HideInInspector]`) em `CaveDebugLevelSkipController.cs` poderiam ser removidos numa spec de limpeza futura, apos confirmar que nenhum scene generator ativo ainda depende deles.

---

## Dependency Chain

```text
Original target: spec_codex_11_debug_build_guards
Dependency chain: nenhuma dependencia de outra spec do lote codex_convergence_lote2
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: N/A (esta ja e a spec original; nenhuma cadeia foi aberta)
```
