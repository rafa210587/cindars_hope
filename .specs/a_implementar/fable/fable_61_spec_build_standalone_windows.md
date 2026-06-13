# SPEC — Shipping: Build Standalone Windows + Registro de Cenas

> **Spec ID:** `fable_61_spec_build_standalone_windows`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P1 (BLOQUEADOR de shipping — sem isto não existe jogo distribuível)
> **Type:** Editor / Build / Tooling
> **Domain:** Build / Shipping
> **Parallelizable:** CONDITIONAL (lock de ProjectSettings e tools/unity)
> **Parallel group:** fable_batch11_shipping
> **Can run with:** F62, F63, F64, F65, F67 (locks disjuntos — nenhuma toca ProjectSettings/Editor/Build)
> **Must not run with:** F66 (compila o projeto inteiro — diff de runtime instável invalida smoke), F56 (cena de título entra no registro — coordenar ordem)
> **Repo lock scope:** ProjectSettings/EditorBuildSettings.asset (via API), ProjectSettings/ProjectSettings.asset (via API), Assets/_Game/Scripts/Editor/Build/**, tools/unity/**
> **Depends on:**
> - F56 (E56 — tela de título; CONDICIONAL: pode rodar ANTES com FarmScene como entry scene e re-registrar quando o título existir)
> **Blocks:** qualquer release/playtest distribuído; smoke de build em CI futuro
> **Scope:** registro de cenas em EditorBuildSettings via API, pipeline de build batchmode (BuildPipeline) com script PowerShell, identidade mínima do player (productName/companyName/versão/ícone placeholder) via PlayerSettings API, smoke check pós-build.
> **Out of scope:** instalador/Steam/itch, build Linux/macOS, IL2CPP, assinatura digital, splash customizado pago, CI/CD automatizado, otimização de tamanho de build.

required_adrs: [ADR-0008-unity-yaml-editing-policy.md, ADR-0004-validation-evidence-phase-gates.md]
required_game_rules: [validation_acceptance_rules.md]

---

# /speckit.specify

## Contexto

O projeto tem 60+ specs de gameplay e ZERO capacidade de produzir um executável. O estado
real verificado no repo: `ProjectSettings/EditorBuildSettings.asset` tem `m_Scenes: []`
(lista de cenas VAZIA — nem o Editor sabe que cenas compõem o jogo); não existe nenhum
script de build em `tools/unity/` (só validação/compile/testes:
RunUnityCompileValidation.ps1, ScanUnityLogs.ps1, RunUnityEditModeTests.ps1,
GenerateSpawnEcologyAssets.ps1); e `ProjectSettings/ProjectSettings.asset` ainda diz
`companyName: DefaultCompany`, `productName: cindars_hope`, `bundleVersion: 1.0` — a
identidade default do template Unity.

As 3 cenas reais do jogo existem em `Assets/_Game/Scenes/`: FarmScene.unity,
TownScene.unity, CaveScene.unity. A F56 pode criar uma cena de título; esta spec deve
registrá-la como entry scene SE ela existir no momento da execução, senão FarmScene é o
entry provisório (documentado como débito até E56).

Restrição dura de governança: ProjectSettings/** é superfície protegida
(`permissions.ask` em `.claude/settings.json` exige autorização humana por instância) e
edição manual de YAML é proibida (ADR-0008). TODA mudança em build settings/player
settings desta spec acontece VIA API do Editor (`EditorBuildSettings.scenes`,
`PlayerSettings.*`) executada por script editor com evidência — nunca editando o .asset
na mão. A aprovação do humano no prompt de permissão É a autorização per-instance.

## Problema

Sem cenas registradas, `BuildPipeline.BuildPlayer` não tem o que buildar e qualquer
`SceneManager.LoadScene("TownScene")` quebra em build standalone (cena fora do build).
Sem pipeline, "o jogo roda" só é verdade dentro do Editor — nenhum playtest externo,
nenhuma validação de comportamento em player real (resolução, Player.log, paths de save
em `persistentDataPath`). Sem identidade, o executável se apresenta como
"DefaultCompany/cindars_hope". Este é o único gap que torna TODO o resto não-entregável.

## Objetivo

Ao final desta spec: (1) um script editor registra as cenas do jogo em
`EditorBuildSettings.scenes` via API (idempotente, com log do antes/depois);
(2) `tools/unity/RunStandaloneBuild.ps1` produz um build Windows standalone via
batchmode `-executeMethod`, com exit code honesto, log dedicado e validação do artefato;
(3) productName/companyName/versão/ícone placeholder configurados via PlayerSettings API
— os VALORES finais (nome comercial, empresa, versão) são DECISÃO HUMANA registrada na
Fase 0 (placeholder documentado se não houver resposta); (4) smoke check pós-build:
executável abre, Player.log sem exceptions de boot, processo encerra limpo.

## Fontes obrigatórias lidas

```text
ProjectSettings/EditorBuildSettings.asset (estado real: m_Scenes vazio)
ProjectSettings/ProjectSettings.asset (companyName/productName/bundleVersion atuais)
tools/unity/RunUnityCompileValidation.ps1 (padrão batchmode do projeto: Unity 6000.4.7f1,
  Start-Process, timeout, log dedicado, exit code)
tools/unity/ScanUnityLogs.ps1 (padrão de scan de log)
docs/decisions/ADR-0008-unity-yaml-editing-policy.md
.claude/rules/unity-assets.md (no parallel batchmode; generated asset evidence)
.claude/rules/validation-truth.md (exit code 0 ou não passou)
.claude/skills/unity-validation/SKILL.md
.claude/skills/unity-asset-generation/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- 3 cenas: Assets/_Game/Scenes/{FarmScene,TownScene,CaveScene}.unity;
- padrão batchmode em tools/unity/RunUnityCompileValidation.ps1 (path do Editor
  6000.4.7f1, resolução de paths, timeout, log) — REUSAR o esqueleto;
- ScanUnityLogs.ps1 (scan de erros em log) — reusar/estender p/ Player.log;
- hook pre-bash-guard.ps1 (bloqueia batchmode paralelo — builds são sequenciais).
Não existe:
- registro de cenas (m_Scenes: []);
- qualquer script de build (editor ou PowerShell);
- identidade de player (DefaultCompany/cindars_hope/1.0);
- smoke check pós-build.
Auditar Fase 0:
- PERGUNTA HUMANA OBRIGATÓRIA: productName comercial ("Cindar's Hope"?), companyName,
  versão inicial (0.1.0?), ícone (existe arte? placeholder programático se não) —
  registrar no report; usar placeholders documentados se sem resposta;
- F56 já criou cena de título? (Glob Assets/_Game/Scenes/*.unity) — define entry scene;
- ordem das cenas no build (entry = título OU FarmScene; demais por load por nome);
- algum SceneTransitionRouter/loader usa build index? (se sim, ordem importa — mapear).
```

## Engineering stories

```text
Como humano do projeto, quero rodar UM script PowerShell e receber um .exe jogável, para
  dar build a qualquer momento sem ritual manual no Editor.
Como SceneTransitionRouter, quero todas as cenas registradas no build, para LoadScene
  por nome funcionar em standalone igual funciona no Editor.
Como playtester externo, quero um executável com nome/versão reais, para reportar bugs
  contra uma versão identificável.
Como validação, quero smoke check pós-build com exit code, para "buildou" nunca ser
  confundido com "abre e roda".
```

## Escopo

```text
Inclui:
- Assets/_Game/Scripts/Editor/Build/BuildSceneRegistrar.cs:
  - MenuItem + método estático batchmode-friendly;
  - monta EditorBuildSettings.scenes via API com a lista canônica:
    [título SE existir] → FarmScene → TownScene → CaveScene (paths de
    Assets/_Game/Scenes/);
  - idempotente (re-rodar produz o mesmo resultado); loga antes/depois (count + paths);
  - falha com exit code != 0 se qualquer cena esperada não existir no disco;
- Assets/_Game/Scripts/Editor/Build/StandaloneBuildPipeline.cs:
  - método estático p/ -executeMethod: garante registro de cenas (chama o registrar),
    aplica identidade via PlayerSettings API (productName/companyName/bundleVersion —
    valores da decisão Fase 0 ou placeholder documentado), executa
    BuildPipeline.BuildPlayer (StandaloneWindows64, output Builds/Windows/);
  - usa BuildReport: summary.result != Succeeded → EditorApplication.Exit(1);
  - loga total de erros/warnings do report e tamanho do build;
- tools/unity/RunStandaloneBuild.ps1:
  - esqueleto do RunUnityCompileValidation.ps1 (mesmo Editor path/param/timeout maior);
  - -batchmode -quit -executeMethod <StandaloneBuildPipeline>; log dedicado
    Logs/unity-standalone-build.log; exit code propagado; valida que o .exe existe
    no output ao final (artefato ausente = FAIL mesmo com exit 0);
- smoke check pós-build (no mesmo RunStandaloneBuild.ps1, flag -Smoke):
  - inicia o .exe, aguarda N segundos de boot, encerra o processo;
  - scan do Player.log (%USERPROFILE%\AppData\LocalLow\<company>\<product>\Player.log)
    por Exception/Error de boot (reusar padrão ScanUnityLogs.ps1);
  - resultado SMOKE_PASS/SMOKE_FAIL com evidência no log;
- identidade placeholder: ícone só se houver arte disponível sem criar asset manual
  (senão registrar como débito de arte — NUNCA YAML manual);
- documentação no report: autorização permissions.ask concedida (per-instance) para os
  diffs de ProjectSettings gerados pela API.
```

## Fora de escopo

```text
Não inclui:
- decidir a identidade comercial (nome/empresa/versão) — pergunta humana Fase 0;
- instalador, distribuição (Steam/itch), auto-update;
- builds de outras plataformas; IL2CPP; stripping agressivo;
- CI/CD (o script é manual-first; CI consome depois);
- resolver bugs de gameplay que o smoke revelar (registrar como débito/bugfix — o smoke
  desta spec valida BOOT, não gameplay completo).
```

## Regras de não duplicação

```text
Não criar segundo esqueleto batchmode — derivar de RunUnityCompileValidation.ps1.
Não criar segundo scanner de log — reusar/estender ScanUnityLogs.ps1.
Não editar ProjectSettings/*.asset por YAML — TODA mudança via EditorBuildSettings/
PlayerSettings API (ADR-0008); o diff resultante no .asset é gerado pelo Unity.
Não registrar cenas hardcoded em dois lugares — lista canônica única no registrar.
```

## Critérios de aceite

### CA-1 Cenas registradas via API

- `EditorBuildSettings.asset` passa a listar as cenas do jogo (FarmScene/TownScene/
  CaveScene + título se existir), gerado EXCLUSIVAMENTE pelo script editor; re-rodar o
  registrar é idempotente.
- Evidência: log do registrar (antes/depois) + diff do .asset gerado pelo Unity +
  autorização permissions.ask registrada no report.

### CA-2 Build standalone produzido com exit code honesto

- `.\tools\unity\RunStandaloneBuild.ps1` produz `Builds/Windows/*.exe` com exit 0;
  falha de compilação/cena ausente/BuildReport != Succeeded → exit != 0 e log claro.
- Evidência: log do build (Logs/unity-standalone-build.log), BuildReport summary,
  existência do artefato validada pelo script.

### CA-3 Identidade mínima aplicada

- productName/companyName/bundleVersion deixam de ser default do template (valores da
  decisão humana OU placeholder explícito documentado); aplicados via PlayerSettings API.
- Evidência: log do pipeline (valores aplicados) + pergunta/resposta humana no report.

### CA-4 Smoke check pós-build

- Com -Smoke: o executável abre, sobrevive ao boot por N segundos, Player.log não contém
  Exception/Error de boot novos, e o resultado é SMOKE_PASS com evidência.
- Evidência: trecho do Player.log no report + resultado do scan.

### CA-5 Pergunta humana registrada

- O execution report contém a pergunta de identidade (nome/empresa/versão/ícone) marcada
  como decisão humana, com os placeholders usados na ausência de resposta.
- Evidência: seção do report.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Build/
  BuildSceneRegistrar.cs       (NOVO — registro idempotente via EditorBuildSettings.scenes)
  StandaloneBuildPipeline.cs   (NOVO — -executeMethod: identidade + BuildPlayer + exit code)
tools/unity/
  RunStandaloneBuild.ps1       (NOVO — batchmode + log + artefato + flag -Smoke)
Builds/Windows/                (output — adicionar a .gitignore se ainda não ignorado)
docs/validation/fable_61_spec_build_standalone_windows_execution_report.md
```

## Contratos

### Data contracts

- Lista canônica de cenas: array estático único em BuildSceneRegistrar
  (`Assets/_Game/Scenes/<nome>.unity`); título opcional resolvido por existência no disco.
- Identidade: constantes/params no StandaloneBuildPipeline (valores Fase 0).

### Runtime contracts

- Nenhum código de runtime muda nesta spec (zero diff em Assets/_Game/Scripts fora de
  Editor/).

### Event contracts

- Nenhum evento novo. N/A.

### Save contracts

- Nenhuma mudança de schema. Atenção do smoke: em standalone o save vai para
  persistentDataPath (comportamento do SaveManager auditado, não alterado).

### UI contracts

- N/A (sem UI nova; o entry scene provisório FarmScene é débito documentado até E56).

## Sistemas afetados

```text
EditorBuildSettings (cenas registradas — via API)
PlayerSettings (identidade — via API)
tools/unity (novo script de build, padrão batchmode existente)
SceneTransitionRouter/loaders (consumidores — auditados, não alterados)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Build/** (novos)
tools/unity/RunStandaloneBuild.ps1 (novo)
ProjectSettings/EditorBuildSettings.asset (SOMENTE diff gerado pela API + permissions.ask)
ProjectSettings/ProjectSettings.asset (SOMENTE diff gerado pela API + permissions.ask)
.gitignore (entrada Builds/ se ausente)
Assembly-CSharp-Editor.csproj includes
docs/validation/**
```

## Arquivos proibidos

```text
Qualquer edição MANUAL de YAML em ProjectSettings/** (.asset só via API do Editor)
Packages/**
Assets/_Game/Scripts/** fora de Editor/ (zero mudança de runtime)
*.unity / *.prefab (nenhuma cena criada/alterada aqui — título é da F56)
tools/unity/RunUnityCompileValidation.ps1 (reusar lendo, não modificar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria e pergunta humana
Registrar pergunta de identidade (nome/empresa/versão/ícone); verificar existência de
cena de título (F56); mapear se algum loader usa build index; confirmar autorização
permissions.ask para os diffs de ProjectSettings.

### Fase 1 — Registro de cenas
BuildSceneRegistrar (API, idempotente, valida existência, log antes/depois) + execução
com evidência (diff do .asset gerado pelo Unity).

### Fase 2 — Pipeline de build
StandaloneBuildPipeline (identidade + BuildPlayer + BuildReport + Exit codes) +
RunStandaloneBuild.ps1 (batchmode, log, timeout, validação de artefato).

### Fase 3 — Smoke e fechamento
Flag -Smoke (boot + Player.log scan); rodar build completo com evidência; .gitignore;
csproj editor; run_strict_validation; execution report com pergunta humana e
autorizações registradas.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch11_shipping.
- Can run with: F62, F63, F64, F65, F67.
- Must not run with: F66 (estabilidade do diff de runtime durante smoke), F56
  (cena de título entra no registro — se simultânea, re-registrar depois).
- Shared files/systems that require lock: ProjectSettings/**, tools/unity/**,
  Editor/Build/**, processo Unity batchmode (NUNCA paralelo — pre-bash-guard).
- Reason: build compila e empacota o projeto INTEIRO; qualquer spec mexendo em runtime
  ao mesmo tempo torna o smoke não-reprodutível.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Observação: smoke documenta o persistentDataPath real do standalone (evidência).
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: N/A (editor-only)
```

## Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO (apenas REGISTRA cenas existentes)
Changes prefabs: NO
Changes ScriptableObjects/assets: ProjectSettings via API (autorizado per-instance)
Requires Play Mode final validation: NO (smoke de build standalone substitui;
  gameplay completo é do lote final)
Human validation timing: smoke nesta spec; gameplay DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: editar ProjectSettings por YAML manual (violação ADR-0008 + permissions.ask).
Mitigação: TODA mudança via EditorBuildSettings/PlayerSettings API; diff gerado pelo
Unity; autorização humana registrada no report.

Risco: build "passou" sem artefato (exit 0 enganoso de batchmode).
Mitigação: script valida existência do .exe + BuildReport.result; validation-truth.

Risco: cenas carregadas por build index quebrarem com a ordem nova.
Mitigação: auditoria Fase 0 dos loaders (LoadScene por nome é o padrão do projeto);
qualquer uso de index documentado e corrigido para nome ANTES do registro.

Risco: batchmode paralelo com outra validação Unity.
Mitigação: hook pre-bash-guard bloqueia; builds sequenciais, um log por execução.

Risco: smoke FAIL por bug de gameplay preexistente (não desta spec).
Mitigação: smoke valida BOOT apenas; falha de boot é desta spec, falha de gameplay vira
débito/bugfix registrado — critério explícito no script.

Risco: título inexistente no momento do build (F56 pendente).
Mitigação: entry provisório FarmScene + débito documentado; registrar é idempotente e
re-roda quando E56 entregar.
```

## Rollback

```text
Reverter os 3 arquivos novos + restaurar EditorBuildSettings/ProjectSettings via git
(diffs pequenos e isolados). Builds/ é output ignorado. Nenhum runtime tocado — zero
risco de regressão de gameplay.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: pergunta humana de identidade + auditoria (título F56? loaders por
        index?) + autorização permissions.ask.
- [ ] T002 — BuildSceneRegistrar (API, idempotente, log antes/depois) + execução com
        evidência de diff.
- [ ] T003 — StandaloneBuildPipeline (identidade + BuildPlayer + BuildReport + exit) +
        RunStandaloneBuild.ps1 (batchmode/log/artefato).
- [ ] T004 — Smoke check (-Smoke: boot + Player.log scan) + build completo com
        evidência; .gitignore; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\unity\RunStandaloneBuild.ps1            # exit 0 + artefato
.\tools\unity\RunStandaloneBuild.ps1 -Smoke     # SMOKE_PASS
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: NO (editor/tooling-only; zero runtime)
- Requires EditMode tests: NO — justificativa: pipeline de build não roda em EditMode;
  a validação canônica É a execução real do build + smoke com exit codes (evidência
  superior ao teste sintético)
- Requires PlayMode automated or final human scenario: NO — smoke de boot standalone é
  a validação desta spec; gameplay completo em build fica no checklist humano do lote
- Requires regression test: YES (RunUnityCompileValidation.ps1 continua passando; zero
  diff de runtime no review)
- Human validation timing: smoke nesta spec; jornada completa no lote final
- Minimum validation evidence for ACCEPTED: build exit 0 + artefato + SMOKE_PASS +
  diffs de ProjectSettings gerados por API com autorização registrada

## Definition of Done

```text
EditorBuildSettings.scenes registrado via API (idempotente, com evidência);
RunStandaloneBuild.ps1 produz Builds/Windows/*.exe com exit code honesto e log;
identidade aplicada via PlayerSettings API (decisão humana ou placeholder documentado);
smoke de boot PASS com Player.log limpo; pergunta humana registrada; zero YAML manual;
zero diff de runtime; builds dotnet 0E; run_strict_validation exit 0; report criado.
```

## Anti-regressão

```text
Zero mudança em Assets/_Game/Scripts/** fora de Editor/Build (runtime intacto).
ProjectSettings só com diffs gerados pela API do Unity (nunca YAML manual — ADR-0008).
RunUnityCompileValidation.ps1 e demais tools/unity intactos e funcionais.
Nenhum batchmode paralelo (pre-bash-guard respeitado; builds sequenciais).
Loaders por nome de cena continuam funcionando no Editor exatamente como antes.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. (7.1/7.2/7.3): productName "Cindar's Hope", versão 0.1.0, companyName a confirmar na
   Fase 0; resolução alvo 1920×1080/16:9 fullscreen + windowed opcional; build standalone
   a CADA checkpoint M1-M4; edição de Build/ProjectSettings via editor script AUTORIZADA
   (confirmação pontual do permissions.ask a cada execução).
```
