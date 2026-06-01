# SPEC 04 - Wave 1 - Legacy Combat Quarantine

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_04_wave1_legacy_combat_quarantine
> Ordem de execucao: 04
> Tipo: Runtime/Architecture
> Depende de: SPEC 01, SPEC 02, SPEC 03
> Bloqueia: Specs posteriores conforme README
> Escopo: Identificar e colocar em quarentena segura fluxos legados de combate/spell sem remover comportamento funcional.

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

Não paralelizar. Depende dos relatórios SPEC_02 e SPEC_03.

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

O log indica coexistencia de fluxos antigos e novos: `FireballItemBridge`, `FireballUseHandler`, `PlayerSpellCaster` ainda existem, mas o fluxo novo usa `PlayerAttackController`. Tambem existe `PlayerCombatController` reduzido a dano recebido. Coexistencia sem contrato claro aumenta risco de chamada duplicada, manutenção errada e bugs intermitentes.

## Objetivos

- Auditar scripts legados de combate/spell.
- Identificar referencias em cenas, prefabs, geradores e assets.
- Criar quarentena segura: marcar legacy, desativar auto-wiring ou documentar uso residual.
- Nao deletar nada nesta spec, salvo arquivo comprovadamente morto e autorizado no log.
- Garantir que PlayerAttackController continue sendo caminho autoritativo de ataque.

## Nao objetivos

- Nao extrair services ainda.
- Nao remover arquivos por impulso.
- Nao mudar bow/arrow/fireball behavior.
- Nao mudar input Q/E/Space.
- Nao alterar save schema.
- Nao mexer em UI alem de referencias se necessario.

## User stories

### US-001 - Saber caminho oficial

Como desenvolvedor, quero saber qual classe e autoritativa para ataque.
### US-002 - Evitar sistema paralelo

Como mantenedor, quero impedir que fireball antiga e fireball nova disparem por caminhos diferentes.
### US-003 - Manter fallback seguro

Como tester, quero que o jogo continue funcionando se algum legado ainda estiver referenciado.

## Requisitos funcionais

### FR-001 - Inventariar legados

Buscar e listar `PlayerSpellCaster`, `FireballItemBridge`, `FireballUseHandler`, `PlayerCombatController` e outros scripts duplicados de spell/combat.
### FR-002 - Verificar referencias

Checar cenas, prefabs, geradores e assets que adicionam ou referenciam esses scripts.
### FR-003 - Definir autoridade

Documentar que `PlayerAttackController` e o caminho autoritativo de ataque Q/E/Space.
### FR-004 - Marcar legacy

Adicionar comentario ou `[Obsolete]` onde seguro, sem quebrar compilacao Unity.
### FR-005 - Bloquear auto-wire legado

Se algum gerador adiciona bridge/caster legado sem uso, remover apenas essa adicao do gerador, preservando arquivo.
### FR-006 - Adicionar validator

Criar validator que reporta se cenas/prefabs ainda possuem componentes legados ativos, classificando Warning ou Error conforme risco.
### FR-007 - Nao deletar

Arquivos legados permanecem ate closeout posterior.

## Requisitos nao funcionais

### NFR-001 - Sem regressao

Q/E/Space continuam funcionando pelo PlayerAttackController.
### NFR-002 - Rastreavel

Cada legado tem status: active, quarantined, referenced, unused, unknown.
### NFR-003 - Sem ambiguidade

Logs/documentacao indicam caminho oficial.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Combat/**
- Assets/_Game/Scripts/Player/** scripts legados identificados
- Assets/_Game/Scripts/Editor/Validation/**
- Assets/_Game/Scripts/Editor/SceneCreation/** somente se geradores adicionarem legado
- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md se status mudar

## Arquivos e areas proibidas

- Alterar regras de PlayerAttackController
- Deletar scripts
- Alterar ItemDatabase/WeaponDatabase/SpellDatabase
- Alterar prefabs/cenas manualmente sem validator/diagnostico
- Alterar save schema




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Rodar busca textual por scripts legados.
2. Mapear referencias.
3. Classificar.
4. Marcar legacy sem remover.
5. Remover adicao automatica de geradores somente se houver novo caminho equivalente ja funcional.
6. Criar validator de legacy references.
7. Validar builds.

# /speckit.tasks

## T-001 - Buscar legados

Procurar classes e referencias.

## T-002 - Criar matriz de status

Documentar cada script legado no PROJECT_LOG.

## T-003 - Marcar legacy

Adicionar comentarios/Obsolete onde seguro.

## T-004 - Ajustar geradores

Se gerador adiciona componente legado inutil, parar de adicionar.

## T-005 - Criar validator

Reportar componentes legados ativos em cenas/prefabs.

## T-006 - Validar

Build e, se possivel, Unity validation.

## Acceptance criteria

- PlayerAttackController permanece autoritativo.
- Nenhum script e deletado sem autorizacao.
- Validator reporta legados ativos.
- Geradores nao adicionam bridge/caster legado se nao usado.
- Melee/unarmed/bow/arrow/fireball continuam compilando.
- PROJECT_LOG.md possui matriz de legados.

## Rollback

Rollback: reverter alteracoes em comentarios/Obsolete/geradores/validator. Como arquivos nao sao deletados, rollback deve preservar comportamento anterior.

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
