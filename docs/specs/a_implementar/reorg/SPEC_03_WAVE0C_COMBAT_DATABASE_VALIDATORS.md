# SPEC 03 - Wave 0C - Combat Database Validators

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_03_wave0c_combat_database_validators
> Ordem de execucao: 03
> Tipo: Editor/Validation/Data
> Depende de: SPEC 01
> Bloqueia: Specs posteriores conforme README
> Escopo: Validar consistencia cruzada de ItemDatabase, WeaponDatabase, SpellDatabase e StatusEffect data.

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

Pode rodar em paralelo com SPEC_02 somente em branch própria. Não editar PROJECT_LOG.md nem IMPLEMENTATION_STATUS em modo paralelo.

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

O fluxo bow/arrow/fireball depende de IDs cruzados: ItemDataSO.WeaponId, ItemDataSO.SpellId, WeaponDataSO.ProjectilePrefab, SpellDataSO.StatusEffectId e registries por ID. Hoje uma divergencia de asset/ID pode quebrar em runtime ou cair em logs de erro durante ataque.

## Objetivos

- Criar validators para bancos de combate.
- Detectar ItemDataSO equippable sem contrato coerente.
- Detectar WeaponId ausente no WeaponDatabase.
- Detectar SpellId ausente no SpellDatabase.
- Detectar StatusEffectId ausente ou inacessivel.
- Detectar hotbar default apontando para item inexistente.
- Detectar StartingItems fora do ItemDatabase.

## Nao objetivos

- Nao alterar dados automaticamente.
- Nao criar item novo.
- Nao corrigir registries nesta spec.
- Nao mudar regras de bow/arrow/fireball.

## User stories

### US-001 - Validar data-driven combat

Como mantenedor, quero garantir que item, weapon, spell e status estejam conectados por IDs validos.
### US-002 - Evitar erro runtime

Como tester, quero descobrir antes do Play Mode se fireball ou bow vao falhar por asset ausente.
### US-003 - Auditar starter/hotbar

Como operador, quero saber se starter items e hotbar defaults existem no ItemDatabase.

## Requisitos funcionais

### FR-001 - Validar ItemDataSO Weapon

Item com Category Weapon e IsEquippable true deve ter WeaponId valido, salvo excecao documentada.
### FR-002 - Validar ItemDataSO Magic

Item com Category Magic e IsEquippable true deve ter SpellId valido.
### FR-003 - Validar ItemDataSO Ammo

Item com Category Ammo deve ter MaxStack maior que 1 e IsEquippable true se usado em maos.
### FR-004 - Validar Bow

WeaponDataSO Type Bow deve ter Range > 0, ProjectileSpeed > 0, ProjectilePrefab != null.
### FR-005 - Validar Fireball

SpellDataSO Type Fireball deve ter Range > 0, ProjectileSpeed > 0, ProjectilePrefab != null.
### FR-006 - Validar StatusEffect

SpellDataSO com StatusEffectId deve resolver para StatusEffectSO ou StatusEffectDatabaseSO conforme estado atual.
### FR-007 - Validar PlayerData StartingItems

Cada StartingItem deve ter Item != null, Amount > 0 e estar no ItemDatabase.
### FR-008 - Validar SaveManager hotbar default

IDs hardcoded em hotbar default devem existir no ItemDatabase.
### FR-009 - Integrar runner

Validator deve rodar pelo runner da SPEC 01.

## Requisitos nao funcionais

### NFR-001 - Sem runtime dependency

Validator deve ficar editor-only.
### NFR-002 - Baixo falso positivo

Warnings para gaps conhecidos; Errors para IDs quebrados que impedem uso.
### NFR-003 - Relatorio acionavel

Cada issue deve sugerir arquivo/asset provavel para corrigir.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Editor/Validation/**
- Assets/_Game/Scripts/Core/Data/** somente leitura
- Assets/_Game/Scripts/Inventory/Data/** somente leitura
- Assets/_Game/Scripts/Combat/** somente leitura
- Assembly-CSharp-Editor.csproj
- PROJECT_LOG.md

## Arquivos e areas proibidas

- Assets/_Game/Data/** modificacao
- Assets/_Game/Scenes/**
- PlayerAttackController.cs
- SaveManager.cs runtime logic




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Inspecionar registries e assets por AssetDatabase.
2. Criar `CombatDatabaseValidator`.
3. Ler hardcoded hotbar defaults de forma simples ou lista declarativa duplicada no validator com comentario.
4. Rodar build.
5. Registrar issues encontradas.

# /speckit.tasks

## T-001 - Mapear assets

Localizar ItemDatabase, WeaponDatabase, SpellDatabase, PlayerData.

## T-002 - Implementar regras

Criar checks FR-001 a FR-008.

## T-003 - Integrar runner

Adicionar ao runner da SPEC 01.

## T-004 - Criar menu dedicado

`CindarsHope/Validate/Combat/Validate Combat Databases`.

## T-005 - Rodar e registrar

Executar se Unity disponivel; caso contrario registrar pendencia.

## Acceptance criteria

- Validator compila.
- Validator reporta ItemDataSO Weapon sem WeaponId valido.
- Validator reporta Magic sem SpellId valido.
- Validator reporta Bow sem ProjectilePrefab.
- Validator reporta Fireball sem ProjectilePrefab.
- Validator reporta StartingItems fora do ItemDatabase.
- Validator reporta hotbar default inconsistente.
- Nenhum asset e modificado automaticamente.

## Rollback

Rollback: remover validator e menu. Nenhum asset deve ter sido modificado.

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
