# SPEC 00 - Strategy and Subagents for Architecture Reorganization

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_00_strategy_subagents
> Ordem de execucao: 00
> Tipo: Operacional / Arquitetura
> Depende de: AGENTS.md, PROJECT_LOG.md, docs/IMPLEMENTATION_STATUS.md
> Bloqueia: Todas as specs deste pacote
> Escopo: Definir como Claude Code/Codex/subagents devem executar as specs de reorganizacao sem degradar o jogo.

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

Não paralelizar. Executar primeiro. Define estratégia e gates.

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

O projeto Cindar's Hope usa Unity, C#, ScriptableObjects, registries por ID, `GameBootstrap`, `SaveManager`, `GameEventBus`, geradores Editor e fluxo SpecKit.

A auditoria arquitetural identificou que o projeto possui bons contratos de MVP, mas varios arquivos estao acumulando responsabilidades. A reorganizacao deve ser incremental, segura e validavel.

## Problema

Execucoes anteriores foram eficazes para entregar features, mas algumas geraram:

- acumulacao de logica em controllers/managers;
- prefabs criados ou editados por YAML com risco de import;
- fluxos legados coexistindo com fluxos novos;
- dependencias implicitas em `GameBootstrap.Instance`;
- validacoes Unity pendentes;
- specs parciais sendo tratadas como proximas de completas.

## Objetivo

Definir um modo padrao de execucao para este pacote:

- specs pequenas quando houver isolamento;
- execucao sequencial quando houver risco de regressao;
- subagents somente em areas independentes;
- validacao objetiva por etapa;
- documentacao clara do que foi mudado;
- nenhuma remocao sem evidencia;
- nenhum gameplay funcional degradado.

## Nao objetivos

- Nao implementar features novas de gameplay.
- Nao reorganizar pastas fisicas em massa.
- Nao renomear namespaces em massa.
- Nao remover legado automaticamente.
- Nao alterar save schema.
- Nao substituir Unity UI, cave, shop, farm ou enemy AI.

## Personas / subagents

### Architecture reviewer

Responsavel por:

- verificar acoplamento;
- revisar se a spec nao cria sistema paralelo;
- validar se a extracao preserva comportamento;
- apontar risco de regressao.

### Spec implementer

Responsavel por:

- implementar somente arquivos permitidos;
- manter escopo;
- atualizar logs;
- rodar build.

### Unity validator

Responsavel por:

- rodar compile/import;
- rodar menus de validate/repair;
- verificar scenes/prefabs quando possivel.

### Non-regression auditor

Responsavel por:

- comparar comportamento esperado antes/depois;
- validar Q/E, hotbar, combat, save, inventory, projectile, UI blockers;
- impedir degradacao silenciosa.

### Docs curator

Responsavel por:

- atualizar `PROJECT_LOG.md`;
- atualizar status se necessario;
- manter spec em `docs/specs/`.

## User stories

### US-001 - Execucao segura

Como mantenedor do projeto, quero executar reorganizacoes arquiteturais em ondas pequenas para evitar quebrar sistemas ja funcionais.

### US-002 - Subagents controlados

Como operador do Claude Code/Codex, quero saber quais specs podem rodar em paralelo para nao criar conflitos de merge ou arquitetura.

### US-003 - Rastreabilidade

Como mantenedor, quero que cada spec registre objetivo, diagnostico, arquivos, validacoes e riscos para auditar depois.

## Requisitos funcionais

### FR-001 - Ordem de execucao

O pacote deve ser executado na ordem definida no README.

### FR-002 - Paralelizacao limitada

Somente specs de validators da Onda 0 podem rodar em paralelo.

### FR-003 - Escopo por spec

Cada spec deve declarar:

- arquivos permitidos;
- arquivos proibidos;
- requisitos;
- criterios de aceite;
- validacoes.

### FR-004 - Handoff

Cada spec deve poder ser copiada para Claude Code/Codex como unidade independente.

### FR-005 - Registro

Cada execucao deve atualizar `PROJECT_LOG.md`.

### FR-006 - Nao degradacao

Toda spec deve conter criterio explicito de nao regressao.

## Requisitos nao funcionais

### NFR-001 - Baixo risco

Mudancas estruturais devem ser incrementais.

### NFR-002 - Reversibilidade

Toda spec deve permitir rollback por commit.

### NFR-003 - Diagnostico antes de alteracao

Quando a spec tocar comportamento existente, o agente deve diagnosticar o estado real antes de modificar.

### NFR-004 - Sem dependencias ocultas

Novos services devem receber dependencias por parametro, rebind ou installer, nao via busca global indiscriminada.


## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Criar branch por onda.
2. Rodar leitura minima.
3. Executar spec.
4. Rodar build/validadores.
5. Atualizar log.
6. Fazer smoke manual/Unity quando aplicavel.
7. So entao seguir para a proxima spec.

## Dependencias

```text
Wave 0 validators -> Wave 1 legacy quarantine -> Wave 2 combat extraction -> Wave 3 data contracts -> Wave 4 status runtime -> Wave 5 save providers -> Wave 6 bootstrap installers -> Wave 7 closeout
```

## Contratos de execucao

Cada spec deve usar esta estrutura:

```text
/speckit.specify
/speckit.plan
/speckit.tasks
Acceptance Criteria
Validation
Rollback
```

# /speckit.tasks

## T-001 - Preparar branch

Criar ou usar branch apropriada.

## T-002 - Ler contexto minimo

Ler:

- `AGENTS.md`
- `PROJECT_LOG.md` topo
- `docs/IMPLEMENTATION_STATUS.md`
- spec atual

## T-003 - Executar sem extrapolar

Alterar somente arquivos permitidos pela spec.

## T-004 - Validar

Executar comandos obrigatorios.

## T-005 - Registrar

Atualizar `PROJECT_LOG.md`.

## Acceptance criteria

- Ordem de execucao clara.
- Specs independentes onde possivel.
- Subagents definidos.
- Stop rules definidos.
- Validacao padrao definida.

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
