# SPEC 08 - Wave 3 - Item and Equipment Contracts

> Status: A implementar
> Fonte oficial: `docs/specs/`
> Projeto: Cindar's Hope
> Branch alvo: `dev`
> Modo de execucao: SpecKit (`/speckit.specify` -> `/speckit.plan` -> `/speckit.tasks` -> implementacao)
> Regra principal: evoluir arquitetura sem degradar comportamento ja funcional.
> Validacao humana: somente no final do pacote, salvo quando a spec pedir Play Mode pontual.

> Spec ID: spec_arch_reorg_08_wave3_item_equipment_contracts
> Ordem de execucao: 08
> Tipo: Data/Runtime Contracts
> Depende de: SPEC 07
> Bloqueia: Specs posteriores conforme README
> Escopo: Formalizar contratos de uso/equipamento de item sem quebrar assets existentes.

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

Não paralelizar. Contratos de item/equipment afetam múltiplos domínios.

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

`ItemDataSO` esta acumulando campos genericos: category, consumable subtype, equippable, WeaponId, SpellId, status effects. Equipamento usa ItemId/InstanceId de forma ambigua. Para escalar armas, ammo, spells, tools e consumables, o contrato precisa ficar explicito.

## Objetivos

- Adicionar contratos leves para tipo de uso de item.
- Declarar slots permitidos.
- Declarar relacionamento ammo/bow e spell/item.
- Manter compatibilidade com assets existentes.
- Nao migrar tudo de uma vez.
- Criar validators para novos contratos.

## Nao objetivos

- Nao reescrever InventoryManager.
- Nao implementar item instances completos.
- Nao alterar save schema nesta spec.
- Nao quebrar hotbar/inventory UI.
- Nao remover Category existente.

## User stories

### US-001 - Contrato claro

Como designer, quero configurar item sem depender de string implicita.
### US-002 - Compatibilidade

Como mantenedor, quero que assets antigos continuem funcionando.
### US-003 - Validacao

Como tester, quero saber quando item equippable nao tem slot/uso coerente.

## Requisitos funcionais

### FR-001 - Adicionar ItemUseKind

Criar enum com None, EquipWeapon, EquipAmmo, EquipSpell, ConsumeFood, ConsumePotion, UseTool, Quest.
### FR-002 - Adicionar AllowedEquipmentSlots

ItemDataSO deve poder declarar slots permitidos sem quebrar assets existentes.
### FR-003 - Adicionar AmmoType opcional

Ammo pode declarar tipo, ex: Arrow.
### FR-004 - Adicionar RequiredPairedUseKind opcional

Arrow pode declarar que exige EquipWeapon/Bow na outra mao.
### FR-005 - Manter Category

Category continua existindo e funcionando.
### FR-006 - Fallback automatico

Se ItemUseKind estiver None, inferir comportamento atual por Category/WeaponId/SpellId.
### FR-007 - Atualizar validators

CombatDatabaseValidator deve validar novos campos se existirem.
### FR-008 - Nao mudar save

Novos campos em SO nao alteram DTOs de save.

## Requisitos nao funcionais

### NFR-001 - Backward compatible

Assets antigos sem novos campos continuam validos.
### NFR-002 - Data-driven

Novas regras devem depender de SO/registry, nao hardcode no controller.
### NFR-003 - Sem schema migration

Nao alterar GameSaveData.

## Arquivos e areas permitidas

- Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
- Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs se necessario
- Assets/_Game/Scripts/Equipment/** se necessario
- Assets/_Game/Scripts/Editor/Validation/**
- Assets/_Game/Data/**/*.asset somente se atualizar assets de teste com cuidado
- PROJECT_LOG.md

## Arquivos e areas proibidas

- InventoryManager rewrite
- SaveManager schema changes
- Scene/prefab changes
- Combat balance changes




## Política de execução por subagent

- Subagent deve atuar como executor de uma única spec.
- Subagent não deve antecipar tarefas de specs posteriores.
- Subagent não deve corrigir problemas fora do escopo, exceto se forem bloqueadores diretos da compilação causada pela própria spec.
- Subagent deve registrar qualquer achado fora do escopo como `OutOfScopeFinding` no relatório.

# /speckit.plan

## Plano tecnico

1. Adicionar campos compatíveis no ItemDataSO.
2. Atualizar services para usar fallback compatível.
3. Atualizar validators.
4. Atualizar assets de bow/arrow/fireball se seguro.
5. Build e Unity import se possivel.

# /speckit.tasks

## T-001 - Definir enums/campos

Adicionar ItemUseKind e slots permitidos.

## T-002 - Implementar inferencia

Criar helper que infere uso quando ItemUseKind None.

## T-003 - Atualizar services

Usar helper sem quebrar Category.

## T-004 - Atualizar validators

Checar contratos novos.

## T-005 - Atualizar assets de teste

Aplicar em bow/arrow/fireball se Unity seguro.

## T-006 - Validar.

## Acceptance criteria

- Assets antigos continuam funcionando.
- Bow/arrow/fireball continuam funcionando.
- Validator detecta item equippable sem uso/slot.
- Nenhuma mudanca de save schema.
- Hotbar/inventory nao regride.

## Rollback

Rollback: remover campos/enums novos e voltar services para Category/WeaponId/SpellId. Se assets foram alterados, reverter commit.

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
