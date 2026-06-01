# SPEC 05 - Wave 2A - Combat Service Extraction

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_05_wave2a_combat_service_extraction
> Ordem de execucao: 05
> Tipo: Runtime/Combat Architecture
> Depende de: SPEC 04
> Bloqueia: Specs posteriores conforme README
> Escopo: Reduzir responsabilidades do PlayerAttackController sem mudar comportamento.

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

Não paralelizar. Toca PlayerAttackController e inicia refactor crítico de combate.

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

`PlayerAttackController` acumula input, resolucao de item/weapon/spell, cooldown, stamina/mana, bow+arrow, melee, projectile spawn, spell cast e dodge. Isso dificulta evolucao segura.

## Objetivos

- Extrair classes de servico pequenas para logica pura ou quase pura.
- Manter PlayerAttackController como ponte Unity/input.
- Preservar comportamento de Q/E/Space.
- Nao alterar balanceamento.
- Nao alterar assets.
- Preparar terreno para ProjectileSpawnService e Bow/Spell services.

## Nao objetivos

- Nao reescrever combate inteiro.
- Nao alterar UI/input routing.
- Nao mudar regra de bow/arrow/fireball.
- Nao alterar prefabs.
- Nao alterar status effect runtime.

## User stories

### US-001 - Controller fino

Como desenvolvedor, quero PlayerAttackController menor e mais legivel.
### US-002 - Servicos testaveis

Como mantenedor, quero regras de ataque isoladas para validar sem mexer em input.
### US-003 - Sem regressao

Como tester, quero que Q/E/Space funcionem igual antes.

## Requisitos funcionais

### FR-001 - Criar EquippedItemResolver

Resolver item equipado para ItemDataSO, WeaponDataSO ou SpellDataSO usando databases existentes.
### FR-002 - Criar CooldownTracker ou helpers

Centralizar cooldown por slot sem alterar tempos.
### FR-003 - Criar CombatActionContext

Objeto simples com slot, item id, direction, managers e databases necessarios.
### FR-004 - Mover logica de resolucao

PlayerAttackController deve delegar resolucao sem duplicar codigo.
### FR-005 - Preservar logs

Logs existentes de CombatLog devem permanecer equivalentes ou mais claros.
### FR-006 - Manter API publica

Metodos chamados por outros scripts devem continuar existindo ou ter wrapper compatível.
### FR-007 - Sem dependencia nova global

Servicos recebem dependencias por parametro/construtor/rebind.

## Requisitos nao funcionais

### NFR-001 - Baixo impacto

Extracao deve ser mecanica e preservar comportamento.
### NFR-002 - Compilacao limpa

Zero erros runtime/editor.
### NFR-003 - Sem asset mutation

Nenhum asset/cena alterado salvo wiring estritamente necessario.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Combat/**
- Assets/_Game/Scripts/Player/PlayerController.cs somente leitura
- Assets/_Game/Scripts/Equipment/** somente leitura
- Assets/_Game/Scripts/Inventory/** somente leitura
- Assembly-CSharp.csproj
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Assets/_Game/Data/**
- Assets/_Game/Scenes/**
- SaveManager.cs
- GameBootstrap.cs salvo se estritamente necessario para rebind
- UI scripts




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Criar classes auxiliares sem alterar comportamento.
2. Migrar resolucao e cooldown aos poucos.
3. Manter PlayerAttackController chamando os novos services.
4. Rodar builds.
5. Registrar antes/depois.

# /speckit.tasks

## T-001 - Snapshot do comportamento

Ler PlayerAttackController e registrar fluxos atuais.

## T-002 - Criar CombatActionContext

Criar contexto simples.

## T-003 - Criar EquippedItemResolver

Extrair resolucao de item/weapon/spell.

## T-004 - Criar cooldown helper

Extrair calculo de cooldown.

## T-005 - Refatorar controller

Substituir blocos internos por chamadas.

## T-006 - Validar logs

Garantir logs de bloqueio e ataque ainda existem.

## T-007 - Build

Rodar validacoes.

## Acceptance criteria

- Q ataca LeftHand como antes.
- E ataca RightHand quando nao ha InteractionCandidate.
- E ainda prioriza interacao.
- Space dodge funciona.
- Unarmed fallback funciona.
- Bow hand pressed continua bloqueado se regra atual diz isso.
- Arrow hand dispara com bow na outra mao.
- Fireball equipada dispara.
- PlayerAttackController reduziu responsabilidades sem perder comportamento.

## Rollback

Rollback: reverter a extracao e voltar ao PlayerAttackController anterior. Como nao ha asset/schema change, rollback por commit deve bastar.

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
