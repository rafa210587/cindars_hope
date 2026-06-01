# SPEC 10 - Wave 5 - Save Providers Incremental Refactor

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_10_wave5_save_providers
> Ordem de execucao: 10
> Tipo: Runtime/Save Architecture
> Depende de: SPEC 09
> Bloqueia: Specs posteriores conforme README
> Escopo: Reduzir acoplamento do SaveManager criando providers/adapters sem alterar schema inicial.

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

Análise pode rodar em paralelo com análise da SPEC_09, mas implementação deve ser sequencial. SaveManager é central.

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

SaveManager conhece diretamente muitos dominios. Isso torna qualquer feature nova dependente de editar o mesmo arquivo e aumenta risco de regressao. A primeira etapa deve criar adapters sem mudar GameSaveData nem schema.

## Objetivos

- Criar contratos de save provider/adapters.
- Extrair captura/restauracao de 1 ou 2 dominios de baixo risco como prova.
- Manter GameSaveData e schema v5.
- Manter SaveManager como orquestrador de arquivo/migration.
- Nao mudar comportamento de save/load.

## Nao objetivos

- Nao refatorar todos os dominios de uma vez.
- Nao alterar schema version.
- Nao mudar path do save.
- Nao implementar multiplos slots.
- Nao mudar migrations.

## User stories

### US-001 - SaveManager menor

Como desenvolvedor, quero reduzir responsabilidade do SaveManager sem mudar save file.
### US-002 - Provider incremental

Como mantenedor, quero mover dominio por dominio com rollback facil.
### US-003 - Save compativel

Como jogador/tester, quero carregar saves existentes.

## Requisitos funcionais

### FR-001 - Criar interface

Criar contrato para Capture/Restore com GameSaveData.
### FR-002 - Criar provider simples

Extrair dominio de baixo risco, recomendado Hotbar ou Bestiary, se estiver bem isolado.
### FR-003 - Orquestrar providers

SaveManager chama providers sem perder captura atual.
### FR-004 - Schema igual

CurrentSchemaVersion permanece 5.
### FR-005 - Compatibilidade

Load de save antigo continua.
### FR-006 - Fallback

Se provider ausente, SaveManager preserva comportamento anterior.
### FR-007 - Logs

Save log indica providers executados.

## Requisitos nao funcionais

### NFR-001 - Baixo risco

Mover poucos dominios por spec.
### NFR-002 - Sem quebrar JsonUtility

DTOs permanecem serializaveis.
### NFR-003 - Sem Unity refs

Providers nao serializam Unity refs.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Save/**
- Assets/_Game/Scripts/Enemy/** se Bestiary provider escolhido
- Assets/_Game/Scripts/UI/Hotbar/** se Hotbar provider escolhido
- PROJECT_LOG.md

## Arquivos e areas proibidas

- GameSaveData schema changes
- Migration changes
- Save file path changes
- Cave save extraction nesta primeira spec
- Death/corpse save extraction nesta primeira spec




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Criar interface e base.
2. Escolher dominio piloto.
3. Extrair captura/restauracao.
4. Garantir save JSON igual ou equivalente.
5. Validar load/save em build.

# /speckit.tasks

## T-001 - Mapear SaveManager

Listar secoes capturadas/restauradas.

## T-002 - Criar contrato

ISaveSectionProvider ou adapter equivalente.

## T-003 - Extrair dominio piloto

Hotbar ou Bestiary.

## T-004 - Integrar SaveManager

Chamar provider preservando fallback.

## T-005 - Validar

Build e, se possivel, save/load Play Mode.

## Acceptance criteria

- CurrentSchemaVersion continua 5.
- Save file path igual.
- Hotbar/Bestiary selecionado salva/carrega como antes.
- SaveManager reduziu acoplamento para dominio piloto.
- Nenhuma migration nova.
- Build passa.

## Rollback

Rollback: devolver codigo extraido ao SaveManager e remover provider.

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
