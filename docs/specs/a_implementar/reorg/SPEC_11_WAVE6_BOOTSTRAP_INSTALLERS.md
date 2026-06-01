# SPEC 11 - Wave 6 - Bootstrap Installers Incremental Refactor

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_11_wave6_bootstrap_installers
> Ordem de execucao: 11
> Tipo: Runtime/Bootstrap Architecture
> Depende de: SPEC 10
> Bloqueia: Specs posteriores conforme README
> Escopo: Dividir GameBootstrap em installers/coordinators incrementais sem mudar lifecycle do jogo.

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

Não paralelizar. GameBootstrap é central.

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

GameBootstrap concentra muitas referencias e inicializacoes. Isso e aceitavel no MVP, mas dificulta escala. A refatoracao deve manter GameBootstrap como composition root, mas delegar inicializacao por dominio.

## Objetivos

- Criar installer/coordinator para um dominio piloto.
- Manter GameBootstrap.Instance.
- Manter DontDestroyOnLoad.
- Nao mudar ordem funcional de inicializacao.
- Reduzir crescimento futuro do GameBootstrap.

## Nao objetivos

- Nao remover GameBootstrap.
- Nao criar DI framework.
- Nao mover todos os managers de uma vez.
- Nao alterar cenas manualmente em massa.
- Nao mudar save/load.

## User stories

### US-001 - Bootstrap menor

Como desenvolvedor, quero GameBootstrap delegando inicializacao por dominio.
### US-002 - Lifecycle preservado

Como tester, quero que novo jogo, save/load e managers inicializem igual.
### US-003 - Sem framework

Como mantenedor, quero solucao Unity simples sem container externo.

## Requisitos funcionais

### FR-001 - Criar installer piloto

Criar por exemplo `CombatRuntimeInstaller` ou `PlayerRuntimeInstaller`.
### FR-002 - Installer serializavel

Pode ser MonoBehaviour ou classe serializavel simples referenciada pelo GameBootstrap.
### FR-003 - Ordem explicita

GameBootstrap chama installers em ordem documentada.
### FR-004 - Sem fallback silencioso novo

Installer deve logar erro claro se wiring obrigatorio ausente.
### FR-005 - Preservar getters

APIs publicas de GameBootstrap permanecem por compatibilidade.
### FR-006 - Atualizar geradores

Scene generators devem adicionar/wire installer se necessario.
### FR-007 - Validator

SceneRuntimeReference validator deve verificar installer/wiring.

## Requisitos nao funcionais

### NFR-001 - Baixo risco

Extrair somente um dominio piloto.
### NFR-002 - Sem dependencia externa

Nao adicionar packages.
### NFR-003 - Rollback simples

GameBootstrap pode voltar a inicializar diretamente.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Core/Bootstrap/**
- Assets/_Game/Scripts/Editor/SceneCreation/** se necessario
- Assets/_Game/Scripts/Editor/Validation/**
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Save schema
- Gameplay balance
- Cave procedural
- UI rewrite
- Mass namespace moves




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Escolher dominio piloto.
2. Criar installer.
3. Delegar trecho correspondente.
4. Manter getters e comportamento.
5. Atualizar geradores/validators.
6. Build/Unity validation.

# /speckit.tasks

## T-001 - Escolher dominio piloto

Preferir CombatRuntimeInstaller se Wave 2 ja estabilizou.

## T-002 - Criar installer

Criar classe simples.

## T-003 - Delegar GameBootstrap

Mover inicializacao correspondente.

## T-004 - Atualizar geradores

Garantir scene creation/wiring.

## T-005 - Validator

Verificar wiring obrigatorio.

## T-006 - Validar.

## Acceptance criteria

- GameBootstrap.Instance continua funcionando.
- Managers continuam inicializando.
- Domino piloto inicializa via installer.
- Getters publicos continuam.
- Nenhum AddComponent fallback novo silencioso.
- Build passa.

## Rollback

Rollback: mover inicializacao de volta para GameBootstrap e remover installer.

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
