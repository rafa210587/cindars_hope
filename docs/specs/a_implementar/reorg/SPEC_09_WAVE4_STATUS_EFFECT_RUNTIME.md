# SPEC 09 - Wave 4 - Status Effect Runtime Unification

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_09_wave4_status_effect_runtime
> Ordem de execucao: 09
> Tipo: Runtime/Combat Status Effects
> Depende de: SPEC 08
> Bloqueia: Specs posteriores conforme README
> Escopo: Unificar status effect runtime para burn/DOT sem depender de Resources.Load e sem semantica ambigua.

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

Análise pode rodar em paralelo com análise da SPEC_10, mas implementação deve ser sequencial após SPEC_08.

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

Burn DOT foi implementado como MVP usando `DurationTurns` como ticks de 1 segundo e `Resources.Load` por StatusEffectId. Isso funciona como ponte, mas cria acoplamento fragil e semantica confusa.

## Objetivos

- Criar ou consolidar StatusEffectDatabaseSO.
- Remover dependencia de Resources.Load para fireball burn.
- Padronizar tick de DOT em tempo real ou documentar conversao.
- Preservar burn atual.
- Nao quebrar status effects do player.
- Criar validators para status effects.

## Nao objetivos

- Nao criar sistema complexo de buffs/debuffs completo.
- Nao alterar balanceamento salvo campos necessarios.
- Nao alterar enemy AI.
- Nao alterar save schema salvo se indispensavel e aprovado.
- Nao mudar fireball visual/projetil.

## User stories

### US-001 - Burn confiavel

Como tester, quero que fireball aplique burn sem depender de Resources folder.
### US-002 - Semantica clara

Como desenvolvedor, quero saber se duracao e segundos ou turnos.
### US-003 - Status extensivel

Como designer, quero adicionar poison/bleed depois usando mesmo contrato.

## Requisitos funcionais

### FR-001 - Criar StatusEffectDatabaseSO

Registry por ID para StatusEffectSO se ainda nao existir.
### FR-002 - Wiring no bootstrap/contexto

Disponibilizar database para SpellCastService/ProjectileSpawnService sem Resources.Load.
### FR-003 - Resolver status por registry

Fireball deve resolver StatusEffectId via database.
### FR-004 - Padronizar DOT

Se combate e tempo real, adicionar campo DurationSeconds ou tratar DurationTurns com conversao documentada.
### FR-005 - Preservar burn

status_burn_test continua com DamagePerTurn e duracao equivalente.
### FR-006 - Ticker seguro

DOT nao aplica em inimigo morto e para ao morrer.
### FR-007 - Validator

Detectar SpellDataSO.StatusEffectId sem StatusEffectSO no registry.

## Requisitos nao funcionais

### NFR-001 - Sem duplicar manager

Evitar mais um StatusEffectManager paralelo sem necessidade.
### NFR-002 - Performance simples

Ticker 1s MVP aceitavel.
### NFR-003 - Compativel

Player status effects existentes nao quebram.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Combat/StatusEffect/**
- Assets/_Game/Scripts/Combat/Magic/**
- Assets/_Game/Scripts/Combat/Weapon/**
- Assets/_Game/Scripts/Core/Data/**
- Assets/_Game/Scripts/Core/Bootstrap/** se wiring necessario
- Assets/_Game/Scripts/Editor/Validation/**
- Assets/_Game/Data/Combat/**
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Player movement
- Enemy AI rewrite
- Save schema sem aprovacao
- UI
- Cave procedural




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Criar registry de status.
2. Wire registry onde necessario.
3. Trocar Resources.Load por registry.
4. Padronizar duracao.
5. Validar fireball burn.
6. Atualizar validators.

# /speckit.tasks

## T-001 - Criar registry

StatusEffectDatabaseSO.

## T-002 - Wire database

Bootstrap/registry/contexto.

## T-003 - Atualizar spell projectile status

Remover Resources.Load.

## T-004 - Padronizar tick

Documentar ou criar DurationSeconds.

## T-005 - Validator

Atualizar CombatDatabaseValidator.

## T-006 - Validar.

## Acceptance criteria

- Fireball ainda aplica burn.
- Burn DOT ainda causa dano.
- Nao existe Resources.Load para status burn no fluxo principal.
- Validator detecta StatusEffectId quebrado.
- Player status effects continuam compilando.
- Sem regressao em projectile.

## Rollback

Rollback: voltar resolucao por Resources.Load e remover registry/wiring se necessario.

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
