# SPEC 01 - Wave 0A - Architecture Validator Foundation

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_01_wave0a_architecture_validator_foundation
> Ordem de execucao: 01
> Tipo: Editor/Validation
> Depende de: SPEC 00
> Bloqueia: Specs posteriores conforme README
> Escopo: Criar fundacao comum para validators Editor sem mudar gameplay.

---


## Execution Contract v3 — anti-ambiguidade

### Fontes obrigatórias antes de alterar

Ler antes de qualquer mudança:

```text
AGENTS.md
PROJECT_LOG.md somente topo/entradas recentes
docs/IMPLEMENTATION_STATUS.md
docs/operations/AGENT_EXECUTION_PROTOCOL.md
docs/operations/READING_MATRIX.md
```

Ler também todos os arquivos citados nesta spec antes de editar qualquer código.

### Stop conditions

Parar e gerar relatório de bloqueio se qualquer condição ocorrer:

1. O arquivo real no repo diverge da premissa da spec.
2. A mudança exige arquivo fora do escopo permitido.
3. A mudança exige remover código/asset que a spec não autorizou remover.
4. A mudança exige alterar save schema sem migration explicitamente definida.
5. A mudança exige editar cena/prefab YAML manualmente sem validator ou sem Unity para confirmar.
6. O build C# falha por erro não relacionado ao escopo.
7. O agente não consegue localizar asset/ID citado pela spec.
8. Há conflito entre dois fluxos runtime e não está claro qual é autoritativo.

### Regra de menor mudança segura

Quando houver múltiplas soluções, aplicar a menor mudança que satisfaça os critérios de aceite e preserve comportamento atual. Não fazer limpeza estética, rename amplo, reorganização de pastas ou remoção de legado fora da spec.

### Política de paralelização desta spec

Não paralelizar. Fundação obrigatória para SPEC_02 e SPEC_03.

### Arquivos compartilhados sensíveis

Considerar estes arquivos como sensíveis. Não alterar em execução paralela sem branch própria e merge sequencial:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
Assembly-CSharp.csproj
Assembly-CSharp-Editor.csproj
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Combat/PlayerAttackController.cs
Assets/_Game/Scripts/Combat/Weapon/ProjectileBehaviour.cs
Assets/_Game/Data/** registry assets
Assets/_Game/Scenes/**
```

# /speckit.specify

## Contexto

Esta spec faz parte do pacote de auditoria e reorganizacao arquitetural v2.0. Ela deve melhorar a capacidade do projeto de escalar, modificar e evoluir sem degradar o que ja funciona.

## Problema

O projeto possui varios validators e repair tools, mas ainda nao ha um contrato padrao para resultados de validacao arquitetural. Isso leva cada validator a logar de forma propria e dificulta rodar uma suite consistente antes de cada onda.

## Objetivos

- Criar um modelo simples e reutilizavel de resultado de validacao Editor.
- Criar helpers para registrar erro, warning, info e summary.
- Criar um menu raiz para rodar validadores de arquitetura sem alterar assets.
- Nao mudar runtime gameplay.
- Preparar base para validators de projectile, combat database, scene wiring e legacy paths.

## Nao objetivos

- Nao implementar validators especificos de projectile ou combat database nesta spec.
- Nao criar repair automatico.
- Nao alterar cenas, prefabs ou ScriptableObjects de dados.
- Nao alterar GameBootstrap, SaveManager ou PlayerAttackController.

## User stories

### US-001 - Rodar validacao padronizada

Como mantenedor, quero rodar uma suite de validadores e receber resultado consistente por erro/warning/info.
### US-002 - Extensao por dominio

Como implementador, quero adicionar validators futuros sem reescrever formato de report.
### US-003 - Sem alteracao de asset

Como operador, quero que o validator foundation nao modifique projeto, apenas reporte.

## Requisitos funcionais

### FR-001 - Criar ValidationSeverity

Adicionar enum editor-only com valores Info, Warning, Error.
### FR-002 - Criar ValidationIssue

Adicionar classe/struct editor-only com Area, Code, Severity, Message, AssetPath, ObjectName e SuggestedFix.
### FR-003 - Criar ValidationReport

Adicionar report com lista de issues, contadores e metodo para summary textual.
### FR-004 - Criar interface IProjectValidator

Contrato deve expor ValidatorId, DisplayName e Run().
### FR-005 - Criar runner

Criar ProjectValidationRunner que recebe validators e imprime summary no Console.
### FR-006 - Criar menu

Criar menu `CindarsHope/Validate/Architecture/Run Architecture Validators` que por enquanto roda suite vazia ou validators registrados manualmente.
### FR-007 - Falhar com clareza

Se houver errors, logar summary com quantidade e codigos.

## Requisitos nao funcionais

### NFR-001 - Editor-only

Todo codigo novo deve ficar em pasta Editor ou protegido por UNITY_EDITOR.
### NFR-002 - Sem runtime overhead

Nenhuma classe runtime deve depender desta fundacao.
### NFR-003 - Sem mutacao

Esta spec nao deve chamar SetDirty, SaveAssets ou salvar cenas.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Editor/Validation/**
- Assembly-CSharp-Editor.csproj
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Assets/_Game/Scripts/Combat/**
- Assets/_Game/Scripts/Player/**
- Assets/_Game/Scripts/Core/Bootstrap/**
- Assets/_Game/Scripts/Save/**
- Assets/_Game/Data/**
- Assets/_Game/Scenes/**




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Verificar validators existentes em `Assets/_Game/Scripts/Editor/Validation`.
2. Criar modelos comuns sem quebrar validators existentes.
3. Criar runner e menu.
4. Rodar build editor.
5. Registrar no log.

# /speckit.tasks

## T-001 - Auditar validators existentes

Listar validators existentes e seus menus.

## T-002 - Criar modelos

Criar `ValidationSeverity`, `ValidationIssue`, `ValidationReport`.

## T-003 - Criar contrato

Criar `IProjectValidator`.

## T-004 - Criar runner

Criar `ProjectValidationRunner`.

## T-005 - Criar menu raiz

Adicionar menu de arquitetura sem repair.

## T-006 - Validar build

Rodar build editor.

## Acceptance criteria

- Menu novo aparece no Unity Editor.
- Runner consegue executar sem validators e retorna PASS vazio.
- Nenhum asset/cena/prefab e modificado.
- Build runtime/editor passa.
- PROJECT_LOG.md atualizado.

## Rollback

Reverter os arquivos criados nesta spec. Como nao ha alteracao de runtime/assets, rollback deve ser direto por commit.

## Guardrails globais

- Nao recriar pastas raiz `specs/` ou `spec/`.
- Nao editar `docs_old/**`.
- Nao remover feature, script, asset, prefab, cena ou fluxo legado nesta spec, salvo se a propria spec disser explicitamente e depois de validacao objetiva.
- Nao alterar regras de gameplay fora do escopo.
- Nao mascarar erro de wiring criando fallback silencioso novo.
- Nao usar `GameObject.Find()` ou `FindObjectOfType()`.
- Nao criar comunicacao direta entre sistemas de gameplay quando houver evento/contrato existente.
- Nao serializar referencias Unity em DTOs de save.
- Nao alterar save schema sem necessidade comprovada.
- Nao afirmar Unity/Play Mode validado se nao executou Unity.
- Sempre atualizar `PROJECT_LOG.md` no final.
- Atualizar `docs/IMPLEMENTATION_STATUS.md` apenas se o status real de area/spec mudar.


## Validacao obrigatoria

Executar, nesta ordem:

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
tools/docs/validate_docs.ps1
```

Se a task alterar runtime Unity, prefabs, cenas, ScriptableObjects, geradores ou validators Unity, tentar tambem:

```powershell
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Se Unity nao rodar, registrar no `PROJECT_LOG.md`:

```text
Unity validation: NOT RUN
Reason: <motivo>
Command attempted: <comando>
Residual risk: Unity compile/import/play mode not validated locally
```


## Atualizacao obrigatoria de documentacao

Adicionar entrada no topo do `PROJECT_LOG.md` com:

- spec executada;
- objetivo;
- diagnostico real encontrado;
- arquivos alterados;
- contratos criados/alterados;
- comportamento preservado;
- validacoes executadas;
- validacoes nao executadas com motivo;
- riscos residuais;
- proximas specs desbloqueadas.



## Checklist final v3

Antes de encerrar esta spec, confirmar explicitamente no relatório:

- [ ] Arquivos obrigatórios foram lidos.
- [ ] Escopo permitido foi respeitado.
- [ ] Nenhum arquivo proibido foi alterado.
- [ ] Nenhum sistema paralelo foi criado sem necessidade.
- [ ] Nenhum código/asset legado foi removido fora do escopo.
- [ ] Build runtime foi executado ou marcado como NOT RUN com motivo.
- [ ] Build editor foi executado ou marcado como NOT RUN com motivo.
- [ ] `tools/docs/validate_docs.ps1` foi executado ou marcado como NOT RUN com motivo.
- [ ] Unity validation foi executada ou marcada como NOT RUN com motivo.
- [ ] Relatório `docs/validation/<SPEC_ID>_execution_report.md` foi criado/atualizado.
- [ ] Em modo paralelo, `PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md` não foram editados pelo subagent.
- [ ] Em modo sequencial, `PROJECT_LOG.md` foi atualizado quando houve mudança relevante.
