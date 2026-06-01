# SPEC 07 - Wave 2C - Bow/Arrow and Spell Services

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_07_wave2c_bow_arrow_spell_services
> Ordem de execucao: 07
> Tipo: Runtime/Combat Services
> Depende de: SPEC 05, SPEC 06
> Bloqueia: Specs posteriores conforme README
> Escopo: Extrair regras de bow+arrow e spell/fireball para services dedicados.

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

Não paralelizar. Depende de SPEC_05/SPEC_06 e toca regras bow/arrow/fireball.

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

A regra de bow+arrow e spell/fireball e especifica, data-driven e vai crescer. Manter tudo no PlayerAttackController cria risco de regressao em Q/E, stamina/mana, ammo, cooldown e status.

## Objetivos

- Criar BowArrowAttackService.
- Criar SpellCastService.
- Manter regras atuais sem mudar balanceamento.
- Preservar logs e bloqueios.
- Preparar testes/validators futuros.

## Nao objetivos

- Nao mudar input.
- Nao mudar item model.
- Nao mudar status runtime.
- Nao adicionar armas/spells novas.
- Nao alterar assets.

## User stories

### US-001 - Bow arrow isolado

Como desenvolvedor, quero regra de flecha+arco fora do controller.
### US-002 - Spell isolado

Como desenvolvedor, quero spell/fireball fora do controller.
### US-003 - Regra preservada

Como tester, quero que arrow continue exigindo bow na outra mao e fireball continue equipavel.

## Requisitos funcionais

### FR-001 - BowArrowAttackService

Service deve validar ammo na mao ativa e bow na mao oposta.
### FR-002 - Consumir ammo

Service deve consumir 1 arrow somente apos validacoes de cooldown/stamina/inventory passarem.
### FR-003 - Bloquear bow hand

Pressionar mao do bow nao deve disparar se regra atual exige uso pela mao da arrow.
### FR-004 - SpellCastService

Service deve resolver spell, validar cooldown/mana e chamar ProjectileSpawnService.
### FR-005 - Manter E interact priority

Controller continua bloqueando E quando ha InteractionCandidate antes de chamar service.
### FR-006 - Retornar resultado

Services devem retornar AttackResult com Success/ErrorCode.
### FR-007 - Logs

Logs CombatLog devem ter Reason, Slot, ItemId, WeaponId/SpellId.

## Requisitos nao funcionais

### NFR-001 - Sem dependencia Unity pesada

Services podem usar MonoBehaviour somente se necessario para Instantiate via ProjectileSpawnService; preferir classes simples.
### NFR-002 - Determinismo de validacao

Falhas devem retornar codigos previsiveis.
### NFR-003 - Baixo acoplamento

Services recebem InventoryManager, EquipmentManager, databases e managers por contexto.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Combat/**
- Assembly-CSharp.csproj
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Assets/_Game/Data/**
- Assets/_Game/Scenes/**
- SaveManager.cs
- GameBootstrap.cs salvo wiring estritamente necessario




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Definir AttackResult.
2. Extrair bow+arrow.
3. Extrair spell.
4. Atualizar PlayerAttackController para delegar.
5. Validar comportamento.

# /speckit.tasks

## T-001 - Criar AttackResult

Modelo com Success, ErrorCode, Message.

## T-002 - Criar BowArrowAttackService

Mover validacoes e execucao.

## T-003 - Criar SpellCastService

Mover spell/fireball.

## T-004 - Atualizar controller

Controller apenas detecta input e delega.

## T-005 - Build e validators

Rodar build e validators.

## Acceptance criteria

- Arrow na LeftHand + Bow na RightHand: Q dispara.
- Bow na LeftHand + Arrow na RightHand: E dispara se nao houver interacao.
- Bow hand pressed bloqueia com log.
- Arrow sem bow bloqueia com log.
- Sem arrows bloqueia com log.
- Fireball equipada em qualquer mao dispara pela tecla correspondente.
- Mana/cooldown mantidos.
- Melee/unarmed/dodge sem regressao.

## Rollback

Rollback: devolver codigo dos services ao controller usando commit anterior.

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
