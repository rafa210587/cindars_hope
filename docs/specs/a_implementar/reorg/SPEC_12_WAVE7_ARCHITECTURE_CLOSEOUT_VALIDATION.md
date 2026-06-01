# SPEC 12 - Wave 7 - Architecture Closeout Validation

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_12_wave7_architecture_closeout_validation
> Ordem de execucao: 12
> Tipo: Validation/Documentation
> Depende de: SPEC 01-11
> Bloqueia: Specs posteriores conforme README
> Escopo: Fechar pacote com validacao integrada, documentacao e plano residual.

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

Não paralelizar. Executar por último como closeout.

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

A reorganizacao so e util se terminar com estado documentado, validators rodando e smoke tests claros. Historicamente, algumas areas ficaram `implementado em codigo` mas com Unity/Play Mode pendente. Esta spec fecha o pacote sem promover indevidamente specs parciais.

## Objetivos

- Rodar suite de validators criada.
- Rodar builds e Unity validation se possivel.
- Executar checklist Play Mode humano.
- Atualizar status documental real.
- Listar debitos residuais sem mascarar.
- Definir proximas ondas de refatoracao se necessario.

## Nao objetivos

- Nao implementar nova feature.
- Nao remover legado.
- Nao mudar arquitetura durante closeout salvo fix pequeno de validator.
- Nao marcar specs parciais como completas sem validacao.

## User stories

### US-001 - Fechamento confiavel

Como mantenedor, quero saber o que realmente ficou validado.
### US-002 - Sem falsa conclusao

Como operador, quero que pendencias Unity/Play Mode fiquem explicitas.
### US-003 - Plano residual

Como planejador, quero saber proximas melhorias sem bloquear MVP.

## Requisitos funcionais

### FR-001 - Rodar validators

Executar validators de architecture, projectile, combat database, legacy, scene/runtime se existirem.
### FR-002 - Rodar builds

Executar build runtime/editor.
### FR-003 - Rodar Unity validation

Tentar Unity compile/log scan.
### FR-004 - Play Mode checklist

Validar movimento, interacao, hotbar, inventory, equip, Q/E, bow/arrow, fireball, burn, save/load, scene transition basica.
### FR-005 - Atualizar PROJECT_LOG

Registrar resultado consolidado.
### FR-006 - Atualizar IMPLEMENTATION_STATUS

Somente se status real mudou.
### FR-007 - Criar residual backlog

Adicionar lista de debitos remanescentes em docs/backlog ou PROJECT_LOG.

## Requisitos nao funcionais

### NFR-001 - Factualidade

Nao declarar PASS para validacao nao executada.
### NFR-002 - Reprodutibilidade

Checklist deve ter comandos e passos.
### NFR-003 - Nao invasivo

Closeout nao deve alterar runtime salvo fix pequeno e documentado.

## Arquivos e areas permitidas

- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md
- docs/validation/**
- docs/backlog/** se necessario
- Validators se houver fix pequeno

## Arquivos e areas proibidas

- Runtime gameplay changes
- Save schema
- Scene/prefab changes salvo fix bloqueador aprovado
- Large refactors




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Rodar validacoes.
2. Executar checklist.
3. Documentar resultados.
4. Registrar riscos.
5. Fechar pacote.

# /speckit.tasks

## T-001 - Rodar validators

Executar todos menus/suites disponiveis.

## T-002 - Rodar build

Runtime/editor/docs.

## T-003 - Rodar Unity validation

Compile/log scan.

## T-004 - Play Mode humano

Executar checklist.

## T-005 - Atualizar docs

PROJECT_LOG, IMPLEMENTATION_STATUS, docs/validation.

## T-006 - Backlog residual

Listar pendencias.

## Acceptance criteria

- Todos validators executados ou NOT RUN registrado.
- Build runtime/editor passa.
- Docs validation passa.
- Unity validation executada ou NOT RUN registrado.
- Play Mode checklist preenchido.
- Status documental nao exagera completude.
- Debitos residuais listados.

## Rollback

Rollback: closeout e documental; se alguma atualizacao documental estiver incorreta, corrigir por commit posterior.

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
