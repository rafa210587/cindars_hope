# SPEC 02 - Wave 0B - Projectile Prefab Validator

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_02_wave0b_projectile_prefab_validator
> Ordem de execucao: 02
> Tipo: Editor/Validation
> Depende de: SPEC 01
> Bloqueia: Specs posteriores conforme README
> Escopo: Validar prefabs de projeteis e evitar erros de import/serialized type mismatch.

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

Pode rodar em paralelo com SPEC_03 somente em branch própria. Não editar PROJECT_LOG.md nem IMPLEMENTATION_STATUS em modo paralelo.

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

O projeto ja teve erro real de prefab: `Type mismatch. Expected type 'BoxCollider2D', but found 'CircleCollider2D'` em `Projectile_Fireball.prefab`. Prefabs de projectile foram criados/alterados em YAML e precisam de validator objetivo para impedir regressao.

## Objetivos

- Criar validator de prefabs de projectile.
- Detectar ProjectileBehaviour sem Rigidbody2D.
- Detectar ProjectileBehaviour sem Collider2D.
- Detectar collider nao trigger em projectile.
- Detectar campo serializado `_collider` incompatível se voltar a tipo concreto.
- Detectar projectile prefab ausente em bow/fireball assets quando possivel.
- Opcionalmente criar repair seguro somente para componentes faltantes, separado do validator.

## Nao objetivos

- Nao alterar comportamento de ataque.
- Nao alterar PlayerAttackController.
- Nao alterar projectile movement.
- Nao editar prefabs manualmente via YAML se Unity Editor puder gerar/repair.
- Nao criar novos projeteis.

## User stories

### US-001 - Prevenir erro de prefab

Como mantenedor, quero saber antes do Play Mode se um projectile prefab tem componentes incompatíveis.
### US-002 - Validar arrows e fireball

Como tester, quero que arrow e fireball tenham Rigidbody2D, Collider2D trigger e ProjectileBehaviour.
### US-003 - Nao modificar automaticamente

Como operador, quero primeiro um validator que reporte, sem mexer nos assets.

## Requisitos funcionais

### FR-001 - Localizar prefabs

Validator deve procurar prefabs em `Assets/_Game/Data/Combat/Prefabs/**` e qualquer prefab referenciado por WeaponDataSO/SpellDataSO.
### FR-002 - Validar ProjectileBehaviour

Todo prefab de projectile deve ter `ProjectileBehaviour`.
### FR-003 - Validar Rigidbody2D

Todo projectile deve ter Rigidbody2D no mesmo GameObject ou filho aceito explicitamente.
### FR-004 - Validar Collider2D

Todo projectile deve ter pelo menos um Collider2D.
### FR-005 - Validar trigger

Collider usado para hit deve ter `isTrigger = true`.
### FR-006 - Validar serializacao generica

Se `ProjectileBehaviour` tiver campo `_collider`, ele deve aceitar `Collider2D`, nao tipo concreto que force mismatch.
### FR-007 - Validar prefab refs

Todo `WeaponDataSO` com `WeaponType.Bow` deve ter `ProjectilePrefab`. Todo `SpellDataSO` com `SpellType.Fireball` deve ter `ProjectilePrefab`.
### FR-008 - Reportar AssetPath

Todo erro deve informar asset path e nome do prefab/asset.
### FR-009 - Integrar runner

Validator deve ser executavel pelo runner da SPEC 01.

## Requisitos nao funcionais

### NFR-001 - Sem Play Mode obrigatorio

Validator deve rodar no Editor sem entrar em Play Mode.
### NFR-002 - Sem mutacao padrao

Menu validate nao deve salvar assets.
### NFR-003 - Deterministico

Rodadas repetidas devem retornar mesmo resultado no mesmo estado do repo.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Editor/Validation/**
- Assets/_Game/Scripts/Combat/Weapon/ProjectileBehaviour.cs somente para leitura/compatibilidade se necessario
- Assembly-CSharp-Editor.csproj
- PROJECT_LOG.md

## Arquivos e areas proibidas

- PlayerAttackController.cs
- SpellDataSO.cs
- WeaponDataSO.cs
- Assets/_Game/Data/**/*.asset
- Assets/_Game/Data/**/*.prefab
- Assets/_Game/Scenes/**




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Revalidar `ProjectileBehaviour` atual.
2. Implementar `ProjectilePrefabValidator`.
3. Integrar ao runner.
4. Criar menu especifico `CindarsHope/Validate/Combat/Validate Projectile Prefabs`.
5. Rodar build.
6. Se validator encontrar erros, registrar; nao reparar nesta spec salvo se explicitamente pedido.

# /speckit.tasks

## T-001 - Inspecionar contrato atual

Ler `ProjectileBehaviour.cs`, `WeaponDataSO.cs`, `SpellDataSO.cs`, `WeaponDatabaseSO.cs`, `SpellDatabaseSO.cs`.

## T-002 - Criar validator

Implementar validator editor-only.

## T-003 - Integrar runner

Adicionar ao runner comum da SPEC 01.

## T-004 - Criar menu

Adicionar menu dedicado.

## T-005 - Executar validator

Rodar no Unity se disponivel. Se Unity nao disponivel, build editor basta e registrar pendencia.

## Acceptance criteria

- Validator compila.
- Validator reporta prefabs sem ProjectileBehaviour.
- Validator reporta projectile sem Rigidbody2D.
- Validator reporta projectile sem Collider2D.
- Validator reporta collider nao trigger.
- Validator reporta bow/fireball sem ProjectilePrefab.
- Nenhum asset e alterado automaticamente.
- PROJECT_LOG.md atualizado.

## Rollback

Rollback: remover validator e menu criados. Nenhum asset deve precisar rollback.

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
