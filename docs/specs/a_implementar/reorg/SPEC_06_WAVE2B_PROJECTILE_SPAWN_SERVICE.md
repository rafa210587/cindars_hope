# SPEC 06 - Wave 2B - Projectile Spawn Service

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_06_wave2b_projectile_spawn_service
> Ordem de execucao: 06
> Tipo: Runtime/Combat Projectiles
> Depende de: SPEC 05
> Bloqueia: Specs posteriores conforme README
> Escopo: Centralizar spawn e inicializacao de projeteis para bow, fireball e futuras skills.

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

Não paralelizar. Depende de SPEC_05 e altera projectile spawn.

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

Hoje PlayerAttackController instancia projectile diretamente para ranged weapon e spell. Isso duplica regras de spawn position, direction, Initialize/InitializeWithStatus, status effect e logs.

## Objetivos

- Criar ProjectileSpawnService.
- Criar ProjectileSpawnRequest.
- Criar ProjectileSpawnResult.
- Usar service para arrow e fireball.
- Preservar `ProjectileBehaviour` atual.
- Nao alterar balanceamento.

## Nao objetivos

- Nao criar pooling ainda.
- Nao alterar Enemy projectiles.
- Nao alterar prefabs.
- Nao alterar status DOT.
- Nao alterar input.

## User stories

### US-001 - Spawn unico

Como desenvolvedor, quero um unico ponto para instanciar projeteis.
### US-002 - Menos duplicacao

Como mantenedor, quero fireball e arrow compartilhando inicializacao comum.
### US-003 - Preparar skills

Como designer, quero que skill actions futuras possam usar o mesmo spawn service.

## Requisitos funcionais

### FR-001 - Criar request

Request deve conter prefab, source transform/position, direction, speed, range, damage, damageType, knockback, optional statusEffect, statusChance.
### FR-002 - Criar result

Result deve indicar Success, Projectile, ErrorCode e Message.
### FR-003 - Criar service

Service instancia prefab, busca ProjectileBehaviour, chama Initialize/InitializeWithStatus.
### FR-004 - Validar input

Service deve falhar com erro claro se prefab null, direction zero ou ProjectileBehaviour ausente.
### FR-005 - Atualizar PlayerAttackController/services

Arrow e fireball devem usar service.
### FR-006 - Preservar spawn offset

Manter offset atual equivalente: direction.normalized * 0.5f.
### FR-007 - Preservar rotation

ProjectileBehaviour continua controlando rotation.

## Requisitos nao funcionais

### NFR-001 - Sem alocacao excessiva

Service simples; pooling fica futuro.
### NFR-002 - Logs claros

Falhas de spawn devem logar CombatLog com codigo.
### NFR-003 - Sem asset mutation

Nao alterar prefabs.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Combat/**
- Assembly-CSharp.csproj
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Assets/_Game/Data/**
- Assets/_Game/Scenes/**
- Input/UI
- Save schema




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Criar request/result/service.
2. Substituir spawn direto em fluxos de ranged/spell.
3. Garantir comportamento igual.
4. Rodar validators de projectile.
5. Build.

# /speckit.tasks

## T-001 - Criar modelos

ProjectileSpawnRequest e ProjectileSpawnResult.

## T-002 - Criar service

Implementar ProjectileSpawnService.

## T-003 - Integrar arrow

Usar service no disparo de bow/arrow.

## T-004 - Integrar fireball

Usar service no disparo de spell.

## T-005 - Validar

Rodar build e validator de projectile.

## Acceptance criteria

- Arrow ainda dispara.
- Fireball ainda dispara.
- Range/speed/damage/status continuam iguais.
- Erro de prefab null e reportado com clareza.
- PlayerAttackController nao instancia projectile diretamente.
- ProjectileBehaviour segue compativel.

## Rollback

Rollback: restaurar instanciacao direta anterior e remover service/request/result.

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
