# SPEC — Fronteira de Integração Cave: Resíduo de Acoplamento Modular (Combat/Core/Enemy/SceneManagement/UI)

> **Spec ID:** `spec_arch_cave_integration_boundary_residual_v1`
> **Status:** A implementar
> **Wave:** WAVE ARCH — Redução de Acoplamento Modular Residual (pós CV04)
> **Priority:** P3
> **Type:** Runtime / Integration
> **Domain:** Cave
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** `spec_cave_visual_polish_runtime`, `fable_78`, qualquer spec tocando `Cave/Ecosystem`, `Cave/Runtime` materializers, `CaveBiomeArtProfileSO`, ou `Assets/_Game/Scripts/Cave/**` em geral
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Combat/EnemyHealth.cs`, `Assets/_Game/Scripts/Enemy/**`, `Assets/_Game/Scripts/SceneManagement/CaveSceneRuntimeReferenceInstaller.cs`
> **Depends on:**
> - `docs/project/CURRENT_STATE.md` (estado ativo do projeto)
> - `.claude/rules/cave-stable-run.md`
> - `.claude/rules/unity-architecture.md`
> - `spec_cave_visual_polish_runtime` (NÃO pode ser desfeita ou revertida por esta spec)
> **Blocks:**
> - Nenhuma spec declarada nesta wave até o momento.
> **Scope:** Auditar e, onde seguro, reduzir o acoplamento modular residual entre `Cave` e os módulos `Combat`, `Core`, `Enemy`, `SceneManagement`, `UI` — sem desfazer o polimento visual recente da cave, sem adicionar `Resources.Load`, sem quebrar o stable-run contract, e sem tentar de novo o microcorte já rejeitado (mover `CaveSceneRuntimeReferenceInstaller` de `SceneManagement` para `Cave.Runtime`).
> **Out of scope:** arte/visual da cave; saves; IDs de domínio; balanceamento; IA de inimigos; diálogos; declarar "modularização concluída" (o baseline do projeto tem `MutualModulePairs > 0`, portanto esse claim nunca é verdadeiro nesta wave).

required_adrs: []
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## 5. Contexto

Esta spec existe para tratar o débito de acoplamento modular que ficou como resíduo depois da wave de
redução de acoplamento (`1ea7bf88 refactor(arquitetura): reduzir acoplamento modular residual`) e do
polimento visual runtime da cave (`spec_cave_visual_polish_runtime` / CV04, ainda em `.specs/a_implementar/`
como não promovida a `implementados/`, conforme `docs/project/CURRENT_STATE.md`). O roadmap de arquitetura
do projeto rastreia acoplamento entre módulos via `tools/architecture/Get-ModularizationDependencySnapshot.ps1`,
que reporta `RuntimeModuleEdges`, `MutualModulePairs`, `UsingOnlyModuleEdges` e `UsingOnlyMutualModulePairs`.
Nenhuma spec pode declarar a modularização "concluída" enquanto `MutualModulePairs > 0` — este é o estado
real hoje (ver seção 9), então esta spec é explicitamente um passo de redução, não de fechamento.

Este é um domínio sensível: qualquer edit em `Assets/_Game/Scripts/Cave/**` cai sob a rule
`cave-stable-run` (preservar layout/enemies/resources idênticos ao revisitar um `CaveLevel` dentro do
mesmo `CaveRunSeed`) e sob o `ArchitectureRatchetTests` (que trava a contagem de `Resources.Load` por
arquivo — não pode subir). A spec de polimento visual (`spec_cave_visual_polish_runtime`, CV04) tocou
`CaveTileMaterializer`, `CaveEnvironmentElementPlanner/Materializer`, `CaveBiomeArtProfileSO` e criou
`CaveVignetteController` — todos dentro do mesmo lock scope desta spec. Por isso esta spec é
`Parallelizable: NO` e nunca deve rodar simultaneamente com ela.

## 6. Problema

`Cave` tem hoje pelo menos 5 pares de módulo com acoplamento mútuo ou dependência forte confirmados
nesta sessão via leitura direta do código (ver seção 9): `Cave|Combat`, `Cave|Enemy` (via
`CaveBandScaling`/`CaveEcosystemBalanceSO` referenciados por `EnemyHealth`), `Cave|SceneManagement`
(via `CaveSceneRuntimeReferenceInstaller`), e possivelmente `Cave|UI`/`Cave|Core` dependendo do que a
Fase 0 confirmar. Uma tentativa anterior de corte (mover `CaveSceneRuntimeReferenceInstaller` de
`SceneManagement` para `Cave.Runtime`) foi **rejeitada**: removia a aresta `Cave|SceneManagement` mas
criava uma aresta nova `Cave|UI` e quebrava o build por causa de um `Resources.Load` dentro de
`CindarsHope.Cave.Runtime` que dependia do posicionamento original do installer. Sem uma spec dedicada
que documente esse histórico e trate o corte com cuidado (ou decida deliberadamente não cortar), o
próximo agente que tocar nesse arquivo corre o risco de repetir a mesma tentativa rejeitada.

## 7. Objetivo

Ao final desta spec, o projeto deve ter uma auditoria Phase 0 completa e atualizada dos pares
`Cave|Combat`, `Cave|Core`, `Cave|Enemy`, `Cave|SceneManagement`, `Cave|UI` (edges reais, direção,
motivo de cada dependência), e — apenas onde a auditoria confirmar que um corte é seguro sem
regredir `ArchitectureRatchetTests`, sem quebrar o stable-run contract, sem alterar visualmente a cave
gerada, e sem repetir o microcorte já rejeitado do `CaveSceneRuntimeReferenceInstaller` — reduzir o
número de `MutualModulePairs` e/ou `RuntimeModuleEdges` do snapshot de modularização. Onde um corte
não for seguro, esta spec documenta explicitamente o motivo e deixa a aresta como débito conhecido,
em vez de forçar uma refatoração arriscada. Isto não implementa "modularização concluída" — apenas
reduz o resíduo com evidência.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/RULES.md
.claude/rules/cave-stable-run.md
.claude/rules/unity-architecture.md
.claude/rules/id-stability.md
.claude/rules/code-minimalism-ladder.md
.claude/rules/testing-quality-gate.md
.claude/skills/cave-stable-run-guard/SKILL.md
.claude/skills/system-reuse-audit/SKILL.md
.specs/a_implementar/spec_cave_visual_polish_runtime.md (NÃO desfazer; ler para não conflitar em escopo/arquivos)
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão, 2026-07-13/14)

Snapshot de modularização confirmado (`tools/architecture/Get-ModularizationDependencySnapshot.ps1`,
baseline citado no cabeçalho da tarefa que gerou esta spec):

```text
Branch=dev
RuntimeModuleEdges=241
MutualModulePairs=25
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
```

`MutualModulePairs > 0`, portanto nenhuma spec desta wave pode declarar "modularização concluída".
A Fase 0 de execução desta spec DEVE re-rodar o script (o número pode ter mudado desde este snapshot)
e usar o valor fresco como baseline de comparação pós-corte.

Achados confirmados por Grep/Read nesta sessão:

```text
- Assets/_Game/Scripts/Combat/EnemyHealth.cs (módulo "Combat") importa e usa:
    using CindarsHope.Cave.Data;
    using CindarsHope.Cave.Ecosystem;
    using CindarsHope.Cave.Runtime;
  Uso real de gameplay, não cosmético: CaveBandScaling.ScaleHp/BandForLevel/BandMinLevel para scaling
  de HP por nível de cave, CaveEcosystemBalanceSO como parâmetro de TakeDamageFromEnemy, e
  CaveBossDeathReporter/CaveRuntimeMaterializer citados em comentário como caller do fluxo pós-Configure.
  Isto é acoplamento de DOMÍNIO (vulnerabilidade/scaling de inimigo depende de contexto de cave), não
  um artefato acidental — qualquer corte aqui precisa preservar o comportamento de scaling, não só mover
  código.

- Assets/_Game/Scripts/Cave/Runtime/CaveEnemyMaterializer.cs importa:
    using CindarsHope.Cave.Data;
    using CindarsHope.Cave.Generation;
  (dependência intra-Cave normal, não cross-module — citado para contexto de onde o materializer se
  conecta ao restante do domínio Cave).

- Assets/_Game/Scripts/SceneManagement/CaveSceneRuntimeReferenceInstaller.cs existe e é o ponto de
  wiring de referências runtime da CaveScene. HISTÓRICO CONFIRMADO: uma tentativa anterior de mover
  esta classe para Cave.Runtime (visando remover a aresta Cave|SceneManagement) foi rejeitada porque
  criava uma aresta nova Cave|UI e quebrava o build via um Resources.Load dentro de
  CindarsHope.Cave.Runtime que dependia do posicionamento original do installer em SceneManagement.
  A Fase 0 de execução DEVE reconfirmar este histórico lendo o arquivo atual e, se ainda aplicável,
  NÃO repetir esse microcorte específico sem uma spec própria dedicada a installers de Cave que trate
  o problema do Resources.Load em separado.

- ArchitectureRatchetTests (Assets/_Game/Tests/EditMode/Architecture/Editor/ArchitectureRatchetTests.cs)
  lê o baseline em tools/architecture/architecture-ratchet-baseline.tsv e falha se a contagem de uma
  regra (ex.: ResourcesLoad) para um arquivo exceder o valor do baseline. Baseline confirmado nesta
  sessão:
    ResourcesLoad  Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs  4
  Isto é a contagem ATUAL permitida — nenhum corte desta spec pode elevar esse número sem atualizar o
  baseline TSV, e elevar o baseline só é aceitável com justificativa explícita no execution report
  (a regra do ratchet é não regredir silenciosamente).

- Cave|Core, Cave|UI: NÃO confirmados nesta sessão via Grep direcionado (span de tempo limitado da
  auditoria que gerou esta spec). A Fase 0 de execução DEVE rodar Grep/análise fresca para `using
  CindarsHope.Cave` em Assets/_Game/Scripts/Core/** e Assets/_Game/Scripts/UI/** (e o inverso, `using
  CindarsHope.Core`/`using CindarsHope.UI` dentro de Assets/_Game/Scripts/Cave/**) antes de assumir a
  existência ou ausência desses pares. Não recriar a spec assumindo que esses pares existem — confirmar
  primeiro.

- spec_cave_visual_polish_runtime (CV04) declara lock scope em Assets/_Game/Scripts/Cave/**,
  Assets/_Game/Scripts/Editor/Cave/**, Assets/_Game/Data/Cave/Biomes/** e toca CaveTileMaterializer,
  CaveEnvironmentElementPlanner/Materializer, CaveBiomeArtProfileSO, e cria CaveVignetteController
  (novo). Ela está em .specs/a_implementar/ (não promovida a implementados/ conforme leitura desta
  sessão) — esta spec de arquitetura NÃO pode reverter nenhum desses sistemas nem seus pools de arte,
  e NÃO deve rodar em paralelo com ela (mesmo lock scope de arquivos).
```

O que esta spec NÃO deve recriar:

```text
Não recriar ArchitectureRatchetTests nem o script Get-ModularizationDependencySnapshot.ps1 — já existem.
Não recriar CaveSceneRuntimeReferenceInstaller — auditar o existente.
Não recriar CaveSceneRuntimeReferenceInstaller como novo installer em outro módulo sem antes resolver
  o motivo raiz da rejeição anterior (Resources.Load cross-module).
```

## 10. User stories / engineering stories

```text
Como mantenedor de arquitetura, quero um relatório atualizado dos pares de módulo Cave-relacionados
para saber quais são débito aceito e quais são cortáveis com segurança.
Como agente executor futuro, quero que o histórico do microcorte rejeitado (CaveSceneRuntimeReferenceInstaller)
esteja documentado nesta spec para não repetir a mesma tentativa e quebrar o build de novo.
Como sistema de cave procedural, quero que qualquer refactor de fronteira preserve o stable-run contract
e a arte materializada pela CV04 sem alteração visual.
```

## 11. Escopo

Inclui:
- Fase 0: re-rodar `Get-ModularizationDependencySnapshot.ps1` e obter o snapshot fresco no momento da execução.
- Fase 0: confirmar via Grep/Read a existência real e a direção de cada um dos pares `Cave|Combat`,
  `Cave|Core`, `Cave|Enemy`, `Cave|SceneManagement`, `Cave|UI` (edges reais, não presumidos).
- Fase 0: reconfirmar o histórico do microcorte rejeitado de `CaveSceneRuntimeReferenceInstaller` lendo
  o arquivo atual e documentar se a condição que causou a rejeição (Resources.Load cross-module) ainda existe.
- Para cada par confirmado, classificar como: (a) cortável com segurança nesta spec (mudança pequena,
  isolada, sem risco de stable-run/ratchet/visual), (b) débito aceito documentado (motivo concreto para
  não cortar agora), ou (c) fora de escopo (precisa de spec própria maior, ex.: installers de Cave).
- Implementar apenas os cortes classificados como (a), um de cada vez, com validação entre cada corte.
- Atualizar `tools/architecture/architecture-ratchet-baseline.tsv` apenas se um corte legitimamente
  reduzir uma contagem existente (nunca para permitir aumento).
- EditMode/build validation após cada corte aplicado.
- Documentar em `docs/validation/` o relatório final com snapshot antes/depois e a lista completa de
  pares tratados (cortados, aceitos como débito, ou deferidos).

Fora:
- Cortar `Cave|Combat`/`Cave|Enemy` movendo `CaveBandScaling`/`CaveEcosystemBalanceSO` para um módulo
  neutro (é uma refatoração maior de domínio de scaling — spec própria se decidido no futuro).
- Repetir o microcorte de mover `CaveSceneRuntimeReferenceInstaller` para `Cave.Runtime` sem resolver
  primeiro o Resources.Load cross-module que causou a rejeição anterior — se a Fase 0 confirmar que a
  condição ainda existe, este corte específico fica classificado como (c) fora de escopo, não (a).
- Qualquer mudança em `spec_cave_visual_polish_runtime` (CV04) ou em seus arquivos-alvo além do que for
  estritamente necessário para o corte aprovado (e mesmo assim, apenas se CV04 já estiver com status
  `implementados` — se ainda estiver em `a_implementar`, esta spec não toca nenhum arquivo do lock scope
  dela, ponto final).

## 12. Fora de escopo

```text
Não inclui: arte/visual da cave; saves; IDs de domínio; balanceamento; IA de inimigos; diálogos;
declarar modularização concluída; mudar o algoritmo de scaling de HP; adicionar Resources.Load novo em
qualquer arquivo; reverter ou modificar CV04.
```

## 13. Regras de não duplicação

```text
Não recriar Get-ModularizationDependencySnapshot.ps1, ArchitectureRatchetTests, ou
CaveSceneRuntimeReferenceInstaller — auditar e, se aplicável, ajustar o existente.
Não criar um segundo installer paralelo de Cave scene wiring.
Não criar um novo mecanismo de tracking de acoplamento — o snapshot script já é a fonte de verdade.
```

## 14. Critérios de aceite

### 14.1 Auditoria Phase 0 completa e honesta

- Cada um dos 5 pares (`Cave|Combat`, `Cave|Core`, `Cave|Enemy`, `Cave|SceneManagement`, `Cave|UI`) tem
  status confirmado: edge real existente (com arquivo/linha) ou edge não confirmado (ausente).
- O histórico do microcorte rejeitado está reconfirmado e citado no execution report.
- Evidência esperada: seção de auditoria no execution report com Grep/Read citados.

### 14.2 Nenhum asset de Cave removido sem substituição

- Nenhum arquivo de arte, `.asset`, prefab ou pool de sprite usado pela CV04 (`CaveBiomeArtProfileSO`,
  `CaveVignetteController`, `CaveTileMaterializer`, `CaveEnvironmentElementPlanner/Materializer`) é
  removido, renomeado ou tem comportamento alterado por esta spec.
- Evidência esperada: diff não toca nenhum arquivo do lock scope de `spec_cave_visual_polish_runtime`.

### 14.3 CaveScene e CreateMvpCaveScene permanecem coerentes

- `Assets/_Game/Scenes/CaveScene.unity` e `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs`
  continuam consistentes entre si (nenhuma referência quebrada introduzida por um corte de módulo).
- Evidência esperada: `MvpSceneValidator` (ou equivalente rodado pelo comando canônico `CindarsHope/Validar Projeto`) PASS.

### 14.4 ArchitectureRatchetTests não regride

- A contagem de `Resources.Load` (e qualquer outra regra do ratchet) para arquivos de Cave não aumenta
  em relação ao baseline vigente no momento da execução.
- Evidência esperada: `ArchitectureRatchetTests` PASS no EditMode run.

### 14.5 Build/EditMode passam

- `dotnet build .\Assembly-CSharp.csproj --no-restore` e `.\Assembly-CSharp-Editor.csproj` retornam exit code 0.
- EditMode tests relevantes (`CindarsHope.Tests.EditMode.Cave`, `ArchitectureRatchetTests`) PASS.
- Evidência esperada: log de build + resultado do Test Runner citados no execution report.

### 14.6 Smoke visual humano documentado

- Um cenário de smoke visual humano (entrar na `CaveScene`, confirmar que a arte/terreno/vinheta/feixe
  da CV04 estão visualmente idênticos ao estado pré-spec) é documentado, mesmo que a execução real do
  humano fique `DEFERRED_TO_FINAL_VALIDATION`.
- Evidência esperada: passos do cenário no execution report ou anexado ao smoke report canônico
  (`spec_validation_human_playmode_smoke_v1`, se já existir).

## 15. Riscos de repetir o microcorte rejeitado (não implementar, apenas documentar)

```text
Esta spec não tenta de novo mover CaveSceneRuntimeReferenceInstaller para Cave.Runtime a menos que a
Fase 0 confirme que a causa raiz da rejeição anterior (Resources.Load cross-module criando a aresta
Cave|UI) foi eliminada por outra mudança desde então. Se a condição ainda existir, este corte específico
fica classificado como (c) — fora de escopo, precisa de spec própria dedicada a installers de Cave que
resolva o Resources.Load primeiro.
```

---

# /speckit.plan

## 16. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/**                              (auditoria; cortes pontuais se aprovados na Fase 0)
Assets/_Game/Scripts/Combat/EnemyHealth.cs                 (auditoria; corte só se classificado (a))
Assets/_Game/Scripts/Enemy/**                               (auditoria; corte só se classificado (a))
Assets/_Game/Scripts/SceneManagement/CaveSceneRuntimeReferenceInstaller.cs  (auditoria; provável débito aceito)
tools/architecture/architecture-ratchet-baseline.tsv        (atualizar só se contagem cair)
docs/validation/spec_arch_cave_integration_boundary_residual_v1_execution_report.md (novo)
```

## 17. Contratos, dados e eventos

### 17.1 Data contracts
N/A — nenhum novo ScriptableObject ou data asset.

### 17.2 Runtime contracts
Se um corte for aplicado, assinaturas públicas de classes tocadas (`EnemyHealth.TakeDamageFromEnemy`,
etc.) permanecem inalteradas; só a localização/namespace de dependências internas pode mudar, nunca o
contrato externo consumido por outros sistemas.

### 17.3 Event contracts
N/A — nenhum evento novo ou alterado esperado. Se um corte exigir um evento novo para desacoplar (ex.:
substituir referência direta por `GameEventBus`), documentar explicitamente e seguir a skill
`event-bus-pattern` e `event-catalog-and-tracing`.

### 17.4 Save contracts
N/A — nenhuma mudança de save DTO.

### 17.5 UI contracts
N/A — nenhuma mudança de UI, exceto se a Fase 0 confirmar a aresta `Cave|UI` e um corte for aprovado;
mesmo assim, sem mudança de comportamento visível ao jogador.

## 18. Sistemas afetados

```text
Cave (auditoria de fronteira modular)
Combat (EnemyHealth — auditoria, corte condicional)
Enemy (auditoria, corte condicional)
SceneManagement (CaveSceneRuntimeReferenceInstaller — auditoria, corte provavelmente NÃO aplicado)
Architecture tooling (snapshot + ratchet baseline)
```

## 19. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Combat/EnemyHealth.cs
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/SceneManagement/CaveSceneRuntimeReferenceInstaller.cs
Assets/_Game/Tests/EditMode/Cave/**
Assets/_Game/Tests/EditMode/Architecture/**
tools/architecture/architecture-ratchet-baseline.tsv
docs/validation/**
```

## 20. Arquivos proibidos

```text
Assets/_Game/Scripts/Cave/** (arquivos do lock scope de spec_cave_visual_polish_runtime — CaveTileMaterializer,
  CaveEnvironmentElementPlanner/Materializer, CaveBiomeArtProfileSO, CaveVignetteController — a menos que
  CV04 já esteja em implementados/ E o corte aprovado exija tocar um desses; documentar explicitamente antes)
Assets/_Game/Data/Cave/Biomes/**
Assets/**/*.unity, *.prefab, *.asset salvo autorização explícita
Packages/**
ProjectSettings/**
docs_old/**
```

## 21. Estratégia de implementação

```md
### Fase 0 — Auditoria: re-rodar snapshot, confirmar edges reais dos 5 pares, reconfirmar histórico do microcorte rejeitado, classificar cada par (a)/(b)/(c)
### Fase 1 — Aplicar apenas cortes classificados (a), um por vez, com build+EditMode entre cada
### Fase 2 — Atualizar architecture-ratchet-baseline.tsv apenas se contagem cair
### Fase 3 — Validação final: build, EditMode, ArchitectureRatchetTests, MvpSceneValidator/comando canônico de validação
### Fase 4 — Smoke visual humano documentado (cenário, não execução)
### Fase 5 — Execution report com snapshot antes/depois e classificação final de cada par
```

## 22. Ordem de execucao (ordem segura)

```text
1. Ler as fontes obrigatórias e a rule cave-stable-run.
2. Rodar Get-ModularizationDependencySnapshot.ps1 e registrar o snapshot fresco.
3. Auditar cada um dos 5 pares via Grep/Read; reconfirmar histórico do microcorte rejeitado.
4. Classificar cada par (a)/(b)/(c) com justificativa escrita.
5. Para cada par (a): aplicar o corte isoladamente, rodar dotnet build + EditMode tests, confirmar
   ArchitectureRatchetTests PASS antes de seguir para o próximo.
6. Rodar CindarsHope/Validar Projeto (via Unity batchmode, se disponível) para confirmar MvpSceneValidator PASS.
7. Rodar o snapshot novamente e comparar antes/depois.
8. Documentar cenário de smoke visual humano (DEFERRED_TO_FINAL_VALIDATION).
9. Escrever execution report com evidência completa.
```

## 23. Paralelização

```md
## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: nenhuma spec que toque Assets/_Game/Scripts/Cave/**, Combat/EnemyHealth.cs, Enemy/**,
  ou SceneManagement/CaveSceneRuntimeReferenceInstaller.cs
- Must not run with: spec_cave_visual_polish_runtime, fable_78, qualquer spec de Cave/Ecosystem ou
  Cave/Runtime materializers, CaveBiomeArtProfileSO
- Shared files/systems that require lock: Assets/_Game/Scripts/Cave/** (lock total durante a execução)
- Reason: mudanças de fronteira modular em Cave têm alto risco de conflito silencioso com specs de
  conteúdo/visual rodando no mesmo domínio; o histórico do microcorte rejeitado mostra que mesmo um
  corte pequeno pode quebrar build por acoplamento não-óbvio (Resources.Load cross-module).
```

## 24. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A
```

## 25. Impacto em eventos

```text
Adds events: CONDITIONAL — só se um corte (a) exigir desacoplar via GameEventBus; documentar se ocorrer.
Changes existing events: NO
Requires unsubscribe pattern: CONDITIONAL — mesmo caso acima.
```

## 26. Impacto em UI/Unity

```text
Changes UI: NO (salvo corte condicional em Cave|UI, se confirmado e aprovado)
Changes scenes: NO (CaveScene.unity não deve precisar de edit manual; se precisar, seguir unity-assets.md)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 27. Riscos técnicos

```text
Risco: repetir o microcorte já rejeitado (CaveSceneRuntimeReferenceInstaller → Cave.Runtime) e quebrar
  o build de novo via Resources.Load cross-module.
Mitigação: Fase 0 reconfirma explicitamente a causa raiz antes de qualquer tentativa; se a causa ainda
  existir, classificar como (c) fora de escopo, não tentar.

Risco: um corte em Cave|Combat/Cave|Enemy altera o comportamento de scaling de HP por CaveLevel,
  quebrando balance ou o stable-run contract indiretamente (RNG/seed usado em scaling).
Mitigação: qualquer corte nesse par deve preservar CaveBandScaling.ScaleHp/BandForLevel/BandMinLevel
  byte-a-byte no comportamento; se não for possível sem reescrever a lógica, classificar como (b) débito
  aceito em vez de arriscar regressão de gameplay.

Risco: elevar Resources.Load em CaveRuntimeMaterializer.cs acima do baseline atual (4) e regredir
  ArchitectureRatchetTests.
Mitigação: nenhum corte desta spec deve adicionar Resources.Load novo; preferir referência serializada
  ou installer explícito conforme a skill bootstrap-wiring.

Risco: alterar visualmente a cave gerada pela CV04 como efeito colateral de um refactor de namespace/módulo.
Mitigação: nenhum corte toca lógica de placement/materialização; smoke visual humano documentado
  confirma paridade antes/depois.
```

## 28. Rollback

```text
Reverter os cortes aplicados na Fase 1 (mudanças isoladas por design, cada uma revertível independentemente).
Restaurar tools/architecture/architecture-ratchet-baseline.tsv ao valor anterior se um corte for revertido.
Não apagar snapshot ou execution report — manter como registro histórico mesmo se revertido.
```

---

# /speckit.tasks

## 29. Tasks

```md
- [ ] T001 — Rodar Get-ModularizationDependencySnapshot.ps1 e registrar snapshot fresco (Fase 0).
- [ ] T002 — Auditar via Grep/Read os 5 pares (Cave|Combat, Cave|Core, Cave|Enemy, Cave|SceneManagement, Cave|UI); reconfirmar histórico do microcorte rejeitado.
- [ ] T003 — Classificar cada par (a)/(b)/(c) com justificativa escrita no execution report.
- [ ] T004 — Aplicar cortes classificados (a), um por vez, com build+EditMode entre cada.
- [ ] T005 — Atualizar architecture-ratchet-baseline.tsv apenas se contagem cair (nunca subir).
- [ ] T006 — Rodar CindarsHope/Validar Projeto (ou equivalente) para MvpSceneValidator PASS.
- [ ] T007 — Rodar snapshot novamente; comparar antes/depois no execution report.
- [ ] T008 — Documentar cenário de smoke visual humano (DEFERRED_TO_FINAL_VALIDATION).
- [ ] T009 — Escrever execution report com evidência completa (build exit codes, EditMode results, classificação final).
```

## 30. Validações obrigatórias

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Cave"
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "ArchitectureRatchetTests"
& .\tools\docs\validate_docs.ps1
```

Se algum comando não puder rodar (Unity locked, sandbox, etc.), registrar `NOT RUN` com motivo e risco
residual, conforme `.claude/rules/validation-truth.md`.

## 31. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: CONDITIONAL (só se um corte (a) tocar CaveBandScaling/scaling de HP —
  nesse caso YES)
- Requires EditMode tests: YES (ArchitectureRatchetTests + testes de Cave existentes devem continuar
  passando; novos testes só se um corte introduzir lógica nova, o que não é esperado)
- Requires PlayMode automated or final human scenario: YES (paridade visual da cave pós-corte)
- Requires regression test: YES (stable-run replay + ArchitectureRatchetTests não regredir)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS (ambos csproj) + Unity compile PASS +
  ArchitectureRatchetTests PASS + EditMode Cave PASS + snapshot antes/depois documentado + cenário de
  smoke visual humano documentado (execução real deferida).
```

## 32. Definition of Done

```text
Auditoria Phase 0 completa e honesta dos 5 pares, com histórico do microcorte rejeitado reconfirmado.
Apenas cortes classificados (a) implementados; (b) e (c) documentados como débito/fora de escopo.
Nenhum arquivo do lock scope de spec_cave_visual_polish_runtime tocado.
CaveScene e CreateMvpCaveScene coerentes.
ArchitectureRatchetTests não regride (Resources.Load e demais regras).
Build e EditMode PASS ou NOT RUN com motivo documentado.
Smoke visual humano documentado (não necessariamente executado).
Execution report criado em docs/validation/.
Nenhum claim de "modularização concluída".
```

## 33. Anti-regressão

```text
Não desfazer nenhum sistema/arte da spec_cave_visual_polish_runtime (CV04).
Não adicionar Resources.Load novo em nenhum arquivo de Cave.
Não repetir o microcorte rejeitado de CaveSceneRuntimeReferenceInstaller sem resolver a causa raiz.
Não alterar seed strings, composição de layout, ou qualquer conteúdo que viole a rule cave-stable-run.
Não usar GameObject.Find/FindObjectOfType em código novo.
Não editar YAML de .unity/.prefab/.asset manualmente.
Não declarar modularização concluída enquanto MutualModulePairs > 0.
```

## 34. Notas para execução posterior

```text
Se a Fase 0 confirmar que Cave|Core e/ou Cave|UI não existem como edges reais, remover essa afirmação
de specs futuras que os citem — não presumir a existência de um edge sem confirmação fresca.
Um corte maior em Cave|Combat/Cave|Enemy (mover CaveBandScaling para um módulo neutro) fica como debt
para uma spec futura dedicada, se o time decidir que vale o risco de tocar lógica de scaling de gameplay.
Resolver o Resources.Load cross-module que bloqueia o corte de CaveSceneRuntimeReferenceInstaller é
pré-requisito de qualquer spec futura de "installers de Cave" — citar esta spec como origem do achado.
```
