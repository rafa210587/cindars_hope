# SPEC â€” Validator Runner Honesto (NOT_CONFIGURED) + Registro dos Validators Existentes

> **Spec ID:** `spec_codex_01_validator_not_configured`
> **Status:** Implementado e BUILD_VALIDATED (docs-migration 2026-08-12)
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P1
> **Type:** Validation / Tooling
> **Domain:** Editor / Validation
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_02, spec_codex_03, spec_codex_04, spec_codex_05, spec_codex_06, spec_codex_07, spec_codex_08
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/Validation/ArchitectureValidationMenu.cs`, `Assets/_Game/Scripts/Editor/Validation/ProjectValidationRunner.cs`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma. Spec isolada em `Editor/Validation`, sem dependÃªncia de outra spec deste lote.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. NÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** Runner de validaÃ§Ã£o de arquitetura deixa de reportar PASS quando nÃ£o hÃ¡ validators registrados; passa a reportar `NOT_CONFIGURED` (tratado como falha) e registra os 4 validators de arquitetura jÃ¡ existentes no repo.
> **Out of scope:** Criar validators novos; validar catÃ¡logos de conteÃºdo (coberto por outras specs/skills).

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (docs-migration 2026-08-12)

```text
Status: Implementado e BUILD_VALIDATED
Execution report: docs/validation/spec_codex_01_validator_not_configured_execution_report.md (2026-07-03)
Re-verificação nesta sessão (Grep/Read no disco, não apenas o report):
  - ValidationReport.cs: `public bool IsConfigured { get; set; } = true;` confirmado.
  - ProjectValidationRunner.cs: caminho vazio/nulo seta `IsConfigured = false` e loga
    `Debug.LogError("... NOT_CONFIGURED: ...")` — confirmado.
  - ArchitectureValidationMenu.cs: registra os 4 validators reais
    (ValidateFarmLevel1LayoutContract, CombatDatabaseValidator, ValidateFarmScaleContract,
    ProjectilePrefabValidator) — confirmado.
  - Assets/_Game/Tests/EditMode/Editor/ProjectValidationRunnerTests.cs existe no disco.
Build: dotnet build Assembly-CSharp + Assembly-CSharp-Editor PASS (relatado no execution report).
Play Mode: NOT REQUIRED (spec seção 25/30) — evidência automatizada é suficiente para esta spec.
Unity Test Runner (EditMode real): NOT RUN na sessão de implementação original; não bloqueia
  o closeout desta spec (Human validation timing: NOT REQUIRED).
```

---

# /speckit.specify

## 5. Contexto

A auditoria de convergÃªncia confirmou (`docs/validation` / achados verificados nesta sessÃ£o): `ArchitectureValidationMenu.RunArchitectureValidation()` (`Assets/_Game/Scripts/Editor/Validation/ArchitectureValidationMenu.cs` L12-17) chama `ProjectValidationRunner.RunValidators()` **sem argumentos**. `ProjectValidationRunner.RunValidators(params IProjectValidator[] validators)` (`Assets/_Game/Scripts/Editor/Validation/ProjectValidationRunner.cs` L17-23) trata lista vazia/nula assim:

```csharp
if (validators == null || validators.Length == 0)
{
    Debug.Log("[ProjectValidationRunner] No validators to run. Returning PASS.");
    return new ValidationReport();
}
```

Isso viola a rule `validation-truth` (nenhum claim de PASS sem evidÃªncia) â€” um `ValidationReport` vazio (`Issues.Count == 0`, `HasErrors == false`) Ã© indistinguÃ­vel de "tudo passou" quando na verdade "nada rodou". Esta spec fecha esse gap de honestidade e, ao mesmo tempo, resolve a causa raiz parcial: existem 4 validators de arquitetura jÃ¡ escritos no repo que implementam `IProjectValidator` mas **nunca sÃ£o passados** ao runner, entÃ£o nunca rodam mesmo quando alguÃ©m chama o menu.

## 6. Problema

Hoje, rodar `CindarsHope/Validar Projeto` (ou qualquer chamada a `ArchitectureValidationMenu.RunArchitectureValidation()`) sempre reporta "PASS" mesmo que nenhuma checagem real tenha ocorrido, dando falso sinal de saÃºde de arquitetura. Um agente ou humano pode acreditar que a arquitetura foi validada quando na verdade zero regras foram checadas.

## 7. Objetivo

Ao final desta spec, `ProjectValidationRunner.RunValidators()` deve emitir um status explÃ­cito `NOT_CONFIGURED` (tratado como falha, nunca como PASS) quando a lista de validators Ã© vazia/nula, e `ArchitectureValidationMenu` deve registrar os 4 validators de arquitetura jÃ¡ existentes no cÃ³digo (`ValidateFarmLevel1LayoutContract`, `CombatDatabaseValidator`, `ValidateFarmScaleContract`, `ProjectilePrefabValidator`), sem alterar o comportamento de nenhum outro caller do runner.

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/testing-quality-gate.md
.claude/rules/validation-truth.md
.claude/skills/editor-validator-authoring/SKILL.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o)

- `Assets/_Game/Scripts/Editor/Validation/IProjectValidator.cs` define o contrato: `ValidatorId`, `DisplayName`, `ValidationReport Run()`.
- `Assets/_Game/Scripts/Editor/Validation/ValidationReport.cs` jÃ¡ tem `ErrorCount`/`WarningCount`/`InfoCount`/`HasErrors`/`HasWarnings` â€” nenhuma mudanÃ§a necessÃ¡ria aqui.
- Exatamente **4 classes** implementam `IProjectValidator` hoje (Grep confirmado, zero outras):
  - `ValidateFarmLevel1LayoutContract` (`Assets/_Game/Scripts/Editor/Validation/ValidateFarmLevel1LayoutContract.cs`, `ValidatorId => "farm_level1_layout_contract"`)
  - `CombatDatabaseValidator` (`Assets/_Game/Scripts/Editor/Validation/CombatDatabaseValidator.cs`, `ValidatorId => "combat_database_validator"`)
  - `ValidateFarmScaleContract` (`Assets/_Game/Scripts/Editor/Validation/ValidateFarmScaleContract.cs`, `ValidatorId => "farm_scale_contract"`)
  - `ProjectilePrefabValidator` (`Assets/_Game/Scripts/Editor/Validation/ProjectilePrefabValidator.cs`, `ValidatorId => "projectile_prefab_validator"`)
- As ~65 outras classes em `Assets/_Game/Scripts/Editor/Validation/**` (ex.: `ValidateSpec11Damage`, `ValidateWave19HudUxAcceptanceGate`, etc.) sÃ£o `[MenuItem]`/mÃ©todo estÃ¡tico prÃ³prios, **nÃ£o** implementam `IProjectValidator` â€” fora de escopo desta spec (nÃ£o recriar; nÃ£o converter â€” converter os ~65 seria escopo massivo nÃ£o pedido).
- `ProjectValidationRunner` nÃ£o tem noÃ§Ã£o de status alÃ©m de `Issues` vazio/nÃ£o-vazio; nÃ£o existe hoje um enum/flag de "configured".
- Nenhum teste EditMode cobre `ProjectValidationRunner` hoje (Grep em `Assets/_Game/Tests/EditMode/**` por "ProjectValidationRunner": nenhum resultado).

## 10. User stories / engineering stories

```text
Como agente de validaÃ§Ã£o, quero que "zero validators" nunca seja relatado como PASS, para nÃ£o propagar falso sinal de saÃºde de arquitetura.
Como maintainer, quero que os 4 validators de arquitetura jÃ¡ escritos rodem de fato quando eu chamo o comando de validaÃ§Ã£o.
```

## 11. Escopo

Inclui:
- Adicionar um resultado explÃ­cito de "nÃ£o configurado" ao fluxo de `ProjectValidationRunner.RunValidators()` quando `validators` Ã© nulo/vazio â€” via novo campo booleano `IsConfigured` (ou equivalente) em `ValidationReport`, e o runner logando `Debug.LogError` (nÃ£o `Debug.Log`) com a string `NOT_CONFIGURED` quando esse caso ocorrer.
- Alterar `ArchitectureValidationMenu.RunArchitectureValidation()` para instanciar e passar os 4 validators existentes (`new ValidateFarmLevel1LayoutContract()`, `new CombatDatabaseValidator()`, `new ValidateFarmScaleContract()`, `new ProjectilePrefabValidator()`).
- EditMode test cobrindo: (a) `RunValidators()` sem args retorna `IsConfigured == false` e nunca `HasErrors == false` tratado como "tudo ok" silenciosamente (o teste deve assertar o novo campo, nÃ£o inferir de `Issues.Count`); (b) `RunValidators()` com 1 validator fake retorna `IsConfigured == true`.

Fora:
- Converter as ~65 classes de validator/menu avulsas para `IProjectValidator`.
- Criar validators novos de conteÃºdo/catÃ¡logo.
- Mudar o comportamento de qualquer outro caller de `ProjectValidationRunner.RunValidators(...)` que jÃ¡ passa validators explÃ­citos (nenhum encontrado hoje, mas a mudanÃ§a deve ser aditiva/retrocompatÃ­vel).

## 12. Fora de escopo

```text
NÃ£o inclui: criaÃ§Ã£o de validators novos; conversÃ£o em massa dos MenuItems avulsos para IProjectValidator; mudanÃ§a na taxonomia de status das specs (isso Ã© .specs/SPEC_VALIDATION_MATRIX_MASTER.md, nÃ£o cÃ³digo).
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o criar um segundo runner paralelo â€” estender ProjectValidationRunner existente.
NÃ£o criar um novo enum de status Pass/Fail â€” reusar HasErrors/HasWarnings jÃ¡ existentes; adicionar apenas o campo de "configured".
```

## 14. CritÃ©rios de aceite

### 14.1 Runner honesto sobre ausÃªncia de validators

- `ProjectValidationRunner.RunValidators()` (sem args) retorna um `ValidationReport` com um campo `IsConfigured == false`.
- O log emitido nesse caso usa `Debug.LogError` (ou nÃ­vel equivalente de falha), contendo a string literal `NOT_CONFIGURED`, nÃ£o mais "Returning PASS".
- EvidÃªncia esperada: trecho do cÃ³digo alterado + teste EditMode novo passando.

### 14.2 Validators reais registrados

- `ArchitectureValidationMenu.RunArchitectureValidation()` chama `ProjectValidationRunner.RunValidators(new IProjectValidator[] { new ValidateFarmLevel1LayoutContract(), new CombatDatabaseValidator(), new ValidateFarmScaleContract(), new ProjectilePrefabValidator() })` (ou equivalente).
- EvidÃªncia esperada: leitura do arquivo alterado.

### 14.3 Teste automatizado

- Novo teste em `Assets/_Game/Tests/EditMode/Editor/ProjectValidationRunnerTests.cs` (ou pasta anÃ¡loga jÃ¡ usada pelo projeto) cobrindo os dois casos de 14.1.
- EvidÃªncia esperada: `dotnet build` PASS + descriÃ§Ã£o do teste no execution report.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Validation/
  ValidationReport.cs           (+ campo IsConfigured, default true)
  ProjectValidationRunner.cs    (lÃ³gica NOT_CONFIGURED quando vazio/nulo)
  ArchitectureValidationMenu.cs (registra os 4 validators)

Assets/_Game/Tests/EditMode/Editor/
  ProjectValidationRunnerTests.cs (novo)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
`ValidationReport.IsConfigured` (bool, default `true` para nÃ£o quebrar callers existentes que jÃ¡ passam validators; passa a `false` apenas no caminho vazio/nulo).

### 16.2 Runtime contracts
N/A â€” editor-only, sem runtime.

### 16.3 Event contracts
N/A.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Editor Validation (ProjectValidationRunner, ArchitectureValidationMenu)
Testing (EditMode)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Validation/ProjectValidationRunner.cs
Assets/_Game/Scripts/Editor/Validation/ArchitectureValidationMenu.cs
Assets/_Game/Scripts/Editor/Validation/ValidationReport.cs
Assets/_Game/Tests/EditMode/Editor/ProjectValidationRunnerTests.cs
docs/validation/**
```

## 19. Arquivos proibidos

```text
Qualquer outro validator em Assets/_Game/Scripts/Editor/Validation/** (leitura ok, ediÃ§Ã£o nÃ£o)
Assets/**/*.unity, *.prefab, *.asset
Packages/**, ProjectSettings/**
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria (concluÃ­da nesta spec; ver seÃ§Ã£o 9)
### Fase 1 â€” ValidationReport.IsConfigured + lÃ³gica NOT_CONFIGURED no runner
### Fase 2 â€” Registrar os 4 validators em ArchitectureValidationMenu
### Fase 3 â€” EditMode tests
### Fase 4 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Adicionar campo IsConfigured a ValidationReport (default true).
2. Alterar RunValidators() para setar IsConfigured = false e logar NOT_CONFIGURED apenas no caminho vazio/nulo; caminho com validators inalterado.
3. Atualizar ArchitectureValidationMenu para passar os 4 validators.
4. Escrever EditMode tests para os dois caminhos.
5. dotnet build Assembly-CSharp + Assembly-CSharp-Editor.
6. Rodar EditMode tests (Unity Test Runner) se praticÃ¡vel; senÃ£o NOT RUN com motivo.
7. Registrar relatÃ³rio.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: todas as outras 7 specs deste lote (arquivos nÃ£o se sobrepÃµem)
- Must not run with: N/A
- Shared files/systems that require lock: nenhum compartilhado com as outras 7
- Reason: escopo isolado em Editor/Validation, sem tocar save/event/scene
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
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

## 26. Riscos tÃ©cnicos

```text
Risco: os 4 validators recÃ©m-registrados podem ter erros reais pendentes (ex.: CombatDatabaseValidator referenciando asset ausente) e o menu passar a reportar FAIL onde antes reportava PASS falso.
MitigaÃ§Ã£o: isso Ã© o comportamento correto e esperado (rule validation-truth); documentar no execution report quaisquer erros reais encontrados, sem tentar corrigi-los nesta spec (fora de escopo â€” spec de honestidade de validaÃ§Ã£o, nÃ£o de correÃ§Ã£o de conteÃºdo).
```

## 27. Rollback

```text
Reverter ArchitectureValidationMenu.cs para RunValidators() sem args.
Reverter ValidationReport.cs (remover IsConfigured).
Remover o teste novo.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Adicionar campo `IsConfigured` a `ValidationReport` (default true).
- [ ] T002 â€” Alterar `ProjectValidationRunner.RunValidators()` para setar `IsConfigured = false` + log `NOT_CONFIGURED` via `Debug.LogError` no caminho vazio/nulo.
- [ ] T003 â€” Atualizar `ArchitectureValidationMenu.RunArchitectureValidation()` para registrar os 4 validators existentes.
- [ ] T004 â€” Criar `ProjectValidationRunnerTests.cs` cobrindo os dois caminhos (vazio â†’ NOT_CONFIGURED; com validator â†’ configured).
- [ ] T005 â€” Gerar execution report com resultado dos 4 validators recÃ©m-ativados (quantos erros/warnings reais, se houver).
```

## 29. ValidaÃ§Ãµes obrigatÃ³rias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Test Runner â€” EditMode: rodar se praticÃ¡vel; senÃ£o `NOT RUN` com motivo (ex.: Unity Editor ocupado).

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (RunValidators branch logic)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO (nÃ£o Ã© bugfix de gameplay; Ã© hardening de honestidade de validaÃ§Ã£o)
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode test novo PASS + leitura confirmando os 4 validators registrados no menu.
```

## 31. Definition of Done

```text
ValidationReport.IsConfigured existe e Ã© respeitado pelo runner.
ArchitectureValidationMenu registra os 4 validators reais.
EditMode test novo cobre os dois caminhos.
Nenhum outro validator/menu tocado.
Execution report criado com achados reais dos 4 validators (erros/warnings, se houver).
```

## 32. Anti-regressÃ£o

```text
NÃ£o alterar a assinatura pÃºblica de RunValidators(params IProjectValidator[] validators) â€” apenas o corpo do caminho vazio.
NÃ£o remover nenhum validator dos ~65 MenuItem avulsos.
NÃ£o converter status para PASS quando IsConfigured == false.
```

## 33. Notas para execuÃ§Ã£o posterior

```text
Os ~65 validators MenuItem avulsos continuam fora do runner central â€” se uma spec futura quiser puxÃ¡-los para IProjectValidator, isso Ã© trabalho novo, nÃ£o desta spec.
Se CombatDatabaseValidator/ProjectilePrefabValidator/ValidateFarmScaleContract/ValidateFarmLevel1LayoutContract reportarem erros reais ao rodar pela primeira vez via runner, isso deve virar um ledger de dÃ­vida separado â€” nÃ£o bloqueia esta spec.
```
