# SPEC — Ownership Residual de Save: `Farm|Save` e `Quests|Save`

> **Spec ID:** `spec_arch_save_ownership_residual_v1`
> **Status:** A implementar
> **Wave:** WAVE ARCH — Modularização Residual (continuação do plano `CLAUDE_MODULARIZATION_REMAINING_PLAN.md`, Fase A — Save Boundary)
> **Priority:** P1
> **Type:** Save
> **Domain:** Save / Farm / Quests
> **Parallelizable:** CONDITIONAL
> **Parallel group:** arch_residual_core_and_save
> **Can run with:** specs que não toquem `Assets/_Game/Scripts/Save/SaveManager.cs`, `Assets/_Game/Scripts/Farm/FarmTileGrid.cs`, `Assets/_Game/Scripts/Quests/Save/QuestSectionProvider.cs`
> **Must not run with:** `spec_arch_core_boundary_residual_v1` (ambas podem tocar `SaveManager.cs`/`GameBootstrap.cs` na mesma janela — rodar em série)
> **Repo lock scope:** `Assets/_Game/Scripts/Save/SaveManager.cs`, `Assets/_Game/Scripts/Save/Providers/**`, `Assets/_Game/Scripts/Farm/FarmTileGrid.cs`, `Assets/_Game/Scripts/Quests/Save/QuestSectionProvider.cs`
> **Depends on (Depende de):**
> - Nenhuma spec pendente. Depende da infraestrutura de providers já existente (`ISaveSectionProvider` + `SaveProviderRegistry`, ~31 `*SectionProvider` já implementados).
> **Blocks (Bloqueia):**
> - Nenhuma spec futura declarada nomeadamente; reduz o débito medido por `MutualModulePairs`.
> **Scope:** Fechar `Farm|Save` sob a métrica completa do snapshot movendo a posse de `FarmTileGrid`/
> `FarmNonArableZones` do `SaveManager` para o domínio Farm (mantendo `SaveManager` como consumidor via
> provider, não dono); revalidar/fechar `Quests|Save` — que já foi "fechado" por
> `spec_arch_quests_save_cycle_reduction_v24` sob a métrica antiga, mas reaparece sob a métrica completa
> por causa de uma referência fully-qualified residual em `SaveManager.cs`.
> **Out of scope:** Qualquer outro par `*|Save` (todos os demais já ausentes do snapshot atual —
> `Economy|Save`, `Equipment|Save`, `Inventory|Save`, `NPC|Save`, `Player|Save`, `Save|UI`, `Save|World`,
> `Cave|Save` — não re-tocar); `Core|Save` (fica para `spec_arch_core_boundary_residual_v1`); saves reais
> do usuário em disco; balanceamento; UI; regen de cena.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Fase A ("Save Boundary") do plano `docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md`. A maior
parte dos pares `*|Save` já foi fechada por uma série de specs em `.specs/implementados/`
(`spec_arch_player_save_cycle_reduction_v20` até `spec_arch_equipment_save_cycle_reduction_v29`, mais
`spec_arch_cave_save_cycle_reduction_v28`). O snapshot atual confirma que esses cortes se mantiveram —
nenhum desses pares aparece na lista atual de `MutualModulePairs`.

Restam dois: `Farm|Save` (nunca foi fechado — o próprio `MODULARIZATION_PAIR_BREAK_MAP.md` não tem
"FEITO" na linha `Farm|Save`, diferente de todos os outros `*|Save`) e `Quests|Save` (foi declarado
"FEITO" por `spec_arch_quests_save_cycle_reduction_v24`, 2026-07-08, mas reaparece no snapshot atual —
mesma causa raiz documentada em `spec_arch_core_boundary_residual_v1`: a spec v24 usou uma referência
fully-qualified sem `using` como técnica de corte, e o commit `e9b8c5bc` (2026-07-10) tornou a métrica
do snapshot sensível a qualquer token fully-qualified, não só a `using`).

`Farm|Save` é qualitativamente diferente dos demais pares `*|Save` já fechados: não é um type-move de
DTO puro. `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` (Tier 3) descreve a estratégia como
*"Relocar posse de `FarmTileGrid`/`FarmNonArableZones` do SaveManager p/ serviço Farm no composition
root + mover `FarmSaveData`/`FarmPlotSaveData` p/ Farm"* — ou seja, `SaveManager` hoje **possui e
instancia** um objeto de domínio Farm diretamente, em vez de só persistir/restaurar um DTO através de um
provider, que é o padrão canônico do resto do save system.

## 6. Problema

`SaveManager.cs` instancia e possui `FarmTileGrid` diretamente (`Assets/_Game/Scripts/Save/SaveManager.cs:129`:
`private readonly Farm.FarmTileGrid _farmTileGrid = new Farm.FarmTileGrid(new Farm.FarmNonArableZones());`),
expondo-o via `public Farm.FarmTileGrid FarmTileGrid => _farmTileGrid;` (linha 142). Isso quebra o
princípio "cada seção de save tem um owner único" que o resto do sistema de providers já respeita: em
vez de o domínio Farm possuir seu próprio estado de tiles e o `SaveManager` só persistir/restaurar via
um `ISaveSectionProvider`, é o `SaveManager` quem possui o objeto de runtime, e 3 arquivos de Farm
(`FarmPlotRegistry.cs`, `Runtime/FarmSceneRuntimeBootstrap.cs`, `Runtime/FarmTillingInputController.cs`)
importam `CindarsHope.Save` só para alcançar essa instância via `[SerializeField] SaveManager
_saveManager` e `_saveManager.FarmTileGrid`. Isso é o oposto do layering pretendido (Save deveria
depender de Farm para persistir o estado de Farm, não Farm depender de Save para acessar seu próprio
estado de runtime).

`Quests|Save` tem um problema menor, mas real: `SaveManager.cs:193` constrói o provider via
`new CindarsHope.Quests.Save.QuestSectionProvider();` — fully-qualified, sem `using` — o que ainda
conta como aresta `Save -> Quests` sob a métrica completa, mesmo já não contando sob `UsingOnly*`.

## 7. Objetivo

Ao final desta spec, o domínio Farm possui seu próprio grid de tiles araveis (owner real é um serviço
Farm, não `SaveManager`), e `SaveManager`/`FarmTilesSectionProvider` (que já existe e já reusa
`FarmTileGrid`/`FarmPlotLogic` para captura/restore — ver `Assets/_Game/Scripts/Save/Providers/
FarmTilesSectionProvider.cs`) consomem esse grid como um provider comum consome estado de domínio,
sem que `SaveManager` seja a fonte de instanciação. `Farm|Save` sai de `MutualModulePairs`. `Quests|Save`
é revalidado e, se a causa raiz for confirmada como a referência fully-qualified isolada, corrigida com
o mínimo de mudança necessária para sair de `MutualModulePairs` sob a métrica completa, sem reabrir o
trabalho já feito por `spec_arch_quests_save_cycle_reduction_v24`.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md (seção 6 "Fase A — Save Boundary", "Atualização 2026-07-10 — métrica de snapshot corrigida", "Atualizacao 2026-07-12")
docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md (linha Farm|Save — SEM "FEITO"; linha Quests|Save — "FEITO" mas ver spec_arch_core_boundary_residual_v1 para o padrão de causa raiz)
.specs/implementados/spec_arch_quests_save_cycle_reduction_v24.md
.specs/implementados/spec_arch_inventory_save_cycle_reduction_v25.md (precedente de type-move de DTO puro para o domínio dono)
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/save-section-provider/SKILL.md
.claude/rules/unity-architecture.md (regra 3 — save DTOs simple types + IDs)
.claude/rules/id-stability.md
.claude/rules/code-minimalism-ladder.md
.claude/rules/testing-quality-gate.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão, 2026-07-13/14)

Snapshot real medido nesta sessão:

```text
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
RuntimeModuleEdges=241
MutualModulePairs=25
```

`Farm|Save` e `Quests|Save` estão ambos em `MutualModulePairs`. `Quests|Save` está **ausente** de
`UsingOnlyMutualModulePairs` (confirma a causa fully-qualified); `Farm|Save` está **presente** também em
`UsingOnlyMutualModulePairs` (confirma que aqui há `using` real dos dois lados, não só token
fully-qualified — corte mais substancial que Quests).

**Evidência `Save -> Farm` (real, `using` de topo):**

```text
Assets/_Game/Scripts/Save/SaveManager.cs:13   using CindarsHope.Farm;
Assets/_Game/Scripts/Save/SaveManager.cs:129  private readonly Farm.FarmTileGrid _farmTileGrid =
                                                   new Farm.FarmTileGrid(new Farm.FarmNonArableZones());
Assets/_Game/Scripts/Save/SaveManager.cs:142  public Farm.FarmTileGrid FarmTileGrid => _farmTileGrid;
```

`SaveManager.cs` também referencia `Farm.Runtime.FarmDailyGoalsSaveData`, `Farm.Lots.FarmLotsSaveData`,
`Farm.Animals.FarmAnimalsSaveData` (linhas ~311-314, ~622-624) — esses três são DTOs de save já
possuídos pelo domínio Farm e consumidos por provider (`_dailyGoalsProvider`, `_farmLotsProvider`,
`_farmAnimalsProvider`), então **não** são o problema — são o padrão correto já em uso (provider
consome DTO do domínio dono). O problema é especificamente `_farmTileGrid`: não é um DTO de save, é o
objeto de runtime mutável em si, instanciado e possuído por `SaveManager`.

**Evidência `Farm -> Save` (real, `using` de topo, 3 arquivos):**

```text
Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs:2                    using CindarsHope.Save;
Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs:1   using CindarsHope.Save;
Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs:7  using CindarsHope.Save;
```

`FarmSceneRuntimeBootstrap.cs` (comentário de topo do arquivo, linha ~10): *"Chama SetBounds no
FarmTileGrid do SaveManager (bounds v7: 64x44, origem centrada)"*; linha ~25:
`[SerializeField] private SaveManager _saveManager;`; método público `EditorWire(SaveManager
saveManager)` (linha ~163). `FarmTillingInputController.cs` linha ~41: `[SerializeField] private
SaveManager _saveManager;`; comentário linha ~47: *"Serviço de solo arável (obtido do SaveManager na
inicialização)"*; `EditorWire(SaveManager saveManager, Transform playerTransform, EquipmentManager
equipmentManager = null, ...)` (linha ~222). Ambos precisam do `SaveManager` **apenas** para chegar em
`saveManager.FarmTileGrid` — nenhuma outra API de `SaveManager` é consumida por esses dois arquivos
(confirmar na Fase 0 de execução, releitura completa dos 2 arquivos).

`FarmPlotRegistry.cs` deve ser lido por completo na Fase 0 de execução para confirmar o uso exato de
`CindarsHope.Save` (não citado em detalhe nesta auditoria — Grep confirmou só o `using`, sem
ocorrências adicionais de `Save.` no corpo do arquivo listadas nesta sessão).

O provider `FarmTilesSectionProvider` (`Assets/_Game/Scripts/Save/Providers/FarmTilesSectionProvider.cs`)
já existe e já é o padrão correto: recebe `FarmTileGrid` via construtor (`public
FarmTilesSectionProvider(FarmTileGrid grid)`), captura/restaura via `_grid.AllTilledTiles`/
`_grid.Clear()`/`_grid.GetOrCreateTileLogicForRestore(...)`. **Não precisa ser recriado** — só precisa
parar de receber o grid de dentro do próprio `SaveManager` (que o instancia) e passar a recebê-lo de
onde ele for possuído depois desta spec.

**Evidência `Quests|Save` (fully-qualified residual):**

```text
Assets/_Game/Scripts/Save/SaveManager.cs:193  _questProvider = new CindarsHope.Quests.Save.QuestSectionProvider();
```

Nenhum outro token `CindarsHope.Quests` encontrado em `Save/**` nesta auditoria (Grep restrito a
`SaveManager.cs`; a Fase 0 de execução deve revarrer `Save/**` inteiro, não só esse arquivo, para
confirmar que não há uma segunda ocorrência esquecida).

**O que esta spec NÃO deve recriar:** `ISaveSectionProvider`, `SaveProviderRegistry`,
`FarmTilesSectionProvider`, `QuestSectionProvider` já existem e funcionam — reusar. Não inventar um
mecanismo de auto-registro de provider a partir do módulo de domínio (o precedente de
`spec_arch_quests_save_cycle_reduction_v24` já investigou isso e confirmou que **não existe** esse
padrão vivo no repo — `SaveManager.Initialize()`/`RegisterProviderDescriptors()` constrói todos os ~30
providers diretamente; é um composition-root paralelo ao `GameBootstrap`, débito conhecido e
**fora de escopo** desta spec).

## 10. User stories / engineering stories

```text
Como sistema de farm, quero possuir meu próprio grid de tiles araveis, sem precisar importar Save só
para alcançar meu próprio estado de runtime.
Como SaveManager, quero apenas persistir/restaurar o estado de Farm via um provider — não instanciá-lo.
Como mantenedor, quero que Quests|Save realmente saia do snapshot sob a métrica vigente, corrigindo o
último ponto fully-qualified residual sem redesenhar o corte já feito pela v24.
```

## 11. Escopo

Inclui:
- Mover a posse de `FarmTileGrid`/`FarmNonArableZones` do `SaveManager` para um ponto do domínio Farm —
  candidatos a avaliar na Fase 0 de execução: um serviço/manager Farm já existente (auditar
  `Assets/_Game/Scripts/Farm/**` e `Assets/_Game/Scripts/Farm/Runtime/**` por um candidato natural antes
  de criar algo novo — rule `system-reuse-audit`/`code-minimalism-ladder`) ou, se nenhum existir, um
  novo tipo mínimo `FarmTileGridOwner`/equivalente no domínio Farm, registrado no `GameRuntimeCompositionRoot`
  ou resolvido via `DomainManagerRegistry` (mesma infraestrutura de `spec_arch_core_boundary_residual_v1`).
- `SaveManager`/`FarmTilesSectionProvider` passam a **consumir** o grid a partir do novo owner (via
  referência injetada ou `DomainManagerRegistry.Get<...>()`), não a instanciá-lo.
- `FarmPlotRegistry.cs`, `FarmSceneRuntimeBootstrap.cs`, `FarmTillingInputController.cs` param de
  importar `CindarsHope.Save` para alcançar o grid — passam a referenciá-lo diretamente pelo novo owner
  Farm (sem necessidade de atravessar `SaveManager`).
- `Quests|Save`: substituir a construção fully-qualified do `QuestSectionProvider` em `SaveManager.cs:193`
  por uma técnica que não deixe o token `CindarsHope.Quests` no arquivo — avaliar na Fase 0 se um
  `using CindarsHope.Quests.Save;` local e restrito (já que só `Quests.Save.QuestSectionProvider` é
  necessário, não o domínio Quests inteiro) reintroduz o par sob a métrica completa (reintroduz, o
  regex não distingue sub-namespace) — portanto a solução real precisa remover a instanciação direta do
  `SaveManager.cs` ou aceitar que este sublote fica documentado como não resolvível sem uma mudança maior
  (ver critério de aceite 14.3 para o caminho de aceitar isso como risco residual).
- EditMode round-trip de save para `FarmTilesSectionProvider` (capturar → limpar grid → restaurar →
  comparar) se não existir cobertura equivalente hoje.

Fora:
- Mover `FarmSaveData`/`FarmPlotSaveData` (DTOs) se eles já morarem no namespace correto — confirmar na
  Fase 0 antes de assumir que precisam mover; se já estiverem em `CindarsHope.Farm`/sub-namespace, não
  tocar.
- Qualquer outro par `*|Save`.
- Migração de saves existentes (ver seção 23 — `JsonUtility` serializa por nome de campo, mover
  tipo/mover posse do objeto de runtime não migra nada quando o DTO capturado continua igual).

## 12. Fora de escopo

```text
Não inclui: Core|Save; qualquer outro par *|Save já fechado; balanceamento; UI; regen de cena; cenas
antigas sem o novo owner Farm wireado (documentar fallback, não regenerar cena nesta spec a menos que
seja estritamente necessário e documentado).
```

## 13. Regras de não duplicação

```text
Não recriar ISaveSectionProvider/SaveProviderRegistry — já existem.
Não recriar FarmTilesSectionProvider — já existe e já funciona por injeção de FarmTileGrid.
Não criar um segundo grid de tiles paralelo — mover a posse do único grid existente.
Não criar um novo mecanismo de auto-registro de provider — SaveManager continua construindo providers
diretamente (débito conhecido, fora de escopo).
```

## 14. Critérios de aceite

### 14.1 `Farm|Save` fechado sob a métrica completa

- `Get-ModularizationDependencySnapshot.ps1` não lista mais `Farm|Save` em `MutualModulePairs` nem em
  `UsingOnlyMutualModulePairs`. Nenhum par novo aparece.
- `SaveManager.cs` não instancia mais `FarmTileGrid`/`FarmNonArableZones` diretamente — só consome via
  referência injetada/resolvida.
- `FarmPlotRegistry.cs`, `FarmSceneRuntimeBootstrap.cs`, `FarmTillingInputController.cs` não têm mais
  `using CindarsHope.Save` (confirmar por Grep pós-mudança) — a menos que a Fase 0 encontre outro motivo
  legítimo para o import que não seja alcançar `FarmTileGrid` (documentar se for o caso).

### 14.2 Round-trip de save preservado

- `FarmTilesSectionProvider.Capture`/`Restore` continuam produzindo o mesmo `FarmTilesSaveData` para o
  mesmo estado de grid, antes e depois da mudança de posse (mesmos campos, mesma serialização —
  `JsonUtility` por nome de campo, não por namespace/owner).
- EditMode test de round-trip (capturar → limpar → restaurar → comparar) PASS.

### 14.3 `Quests|Save` — fechar ou documentar risco residual explícito

- Se a Fase 0 confirmar que a causa raiz é só a linha `SaveManager.cs:193`, corrigir e confirmar que o
  par sai de `MutualModulePairs`.
- Se a correção mínima não for suficiente (ex.: outra ocorrência não mapeada nesta auditoria, ou a
  mudança necessária exigir tocar o padrão de composição de providers, fora de escopo), documentar como
  `BLOCKED_BY_SCOPE` com o motivo exato e a ocorrência remanescente citada por arquivo:linha — não é
  bloqueador do DoD desta spec (o alvo principal é `Farm|Save`).

### 14.4 Build e testes

- `Invoke-UnityGeneratedProjectsBuild.ps1` exit 0, 7/7, 0W/0E.
- `RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"` exit 0 — suíte completa,
  crítica para round-trip.
- `RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"` exit 0.
- Suíte de Farm (se existir namespace/assembly de teste dedicado — confirmar na Fase 0; se não existir,
  a suíte Save já cobre `FarmTilesSectionProvider`, mesma situação documentada em
  `spec_arch_core_inventory_cycle_reduction_v36`).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/
  FarmTileGrid.cs                  (inalterado na lógica; só muda quem instancia)
  FarmTileGridOwner.cs             (novo, SE nenhum owner Farm existente servir — decisão de Fase 0;
                                     nome final a confirmar; candidato: expor via DomainManagerRegistry
                                     ou via GameRuntimeCompositionRoot/instalador Farm já existente)

Assets/_Game/Scripts/Save/
  SaveManager.cs                   (para de instanciar FarmTileGrid; passa a resolver/injetar)
  Providers/FarmTilesSectionProvider.cs (inalterado — já recebe o grid por construtor)

Assets/_Game/Scripts/Quests/Save/
  QuestSectionProvider.cs          (inalterado — só o ponto de construção em SaveManager.cs muda)

Assets/_Game/Tests/EditMode/Save/
  FarmTilesSectionProviderRoundTripTests.cs (novo, se não existir equivalente)

docs/validation/
  spec_arch_save_ownership_residual_v1_execution_report.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
`FarmTilesSaveData`/`FarmTileEntry` inalterados (mesmo schema). Nenhum DTO novo obrigatório — só se a
Fase 0 decidir que o novo owner precisa de um pequeno contrato de acesso (ex.: getter público do grid).

### 16.2 Runtime contracts
Novo ponto de posse do `FarmTileGrid` no domínio Farm (interface opcional, ex. `IFarmTileGridOwner`, se
a Fase 0 decidir que resolver via `DomainManagerRegistry` é o caminho mais consistente com o padrão já
usado por `spec_arch_core_boundary_residual_v1`) — decisão final na Fase 0 de execução, não pré-travada
aqui (evitar over-design antes de auditar candidatos existentes).

### 16.3 Event contracts
N/A — nenhum evento novo ou alterado.

### 16.4 Save contracts
Nenhuma mudança de schema. `FarmTilesSectionProvider` continua com a mesma assinatura pública
(`ProviderId => "farm_tiles"`, `Capture`/`Restore`). Apenas a fonte do `FarmTileGrid` injetado no
construtor muda de "instanciado dentro de `SaveManager`" para "resolvido/injetado do domínio Farm".

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Save (SaveManager para de possuir estado de Farm)
Farm (passa a possuir seu próprio grid de tiles)
Quests (só o ponto de construção do provider em SaveManager muda, se viável)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/Providers/FarmTilesSectionProvider.cs
Assets/_Game/Scripts/Farm/FarmTileGrid.cs
Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs
Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs
Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs
Assets/_Game/Scripts/Farm/** (só se a Fase 0 identificar um owner novo necessário)
Assets/_Game/Scripts/Foundation/** (só se a Fase 0 decidir por uma porta DomainManagerRegistry)
Assets/_Game/Scripts/Quests/Save/QuestSectionProvider.cs (leitura; edição só se necessário)
Assets/_Game/Tests/EditMode/Save/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md (só atualização de status)
docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md (só atualização das linhas Farm|Save/Quests|Save)
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Save/SaveData.cs salvo mudança de campo aditiva documentada (não esperado nesta spec)
Assets/**/*.unity, *.prefab, *.asset salvo autorização explícita
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs (fora de escopo — spec irmã spec_arch_core_boundary_residual_v1)
docs_old/**, docs/archive/**
Packages/**, ProjectSettings/**
```

## 20. Estratégia de implementação

```md
### Fase 0 — Auditoria de ownership
  - Ler FarmTileGrid.cs, FarmPlotRegistry.cs, FarmSceneRuntimeBootstrap.cs, FarmTillingInputController.cs
    por inteiro.
  - Procurar um serviço/manager Farm já existente que seja candidato natural a possuir o grid (rule
    system-reuse-audit) antes de criar tipo novo.
  - Confirmar se GameRuntimeCompositionRoot/algum *RuntimeBootstrap de Farm já é o lugar certo de
    instanciar o grid.
  - Revarrer Save/** inteiro por CindarsHope.Quests para confirmar a única ocorrência (linha 193).
### Fase 1 — Mover posse do FarmTileGrid
  - Instanciar o grid no owner Farm decidido na Fase 0.
  - SaveManager para de instanciar; passa a resolver a referência (injeção via construtor/composition
    root, ou DomainManagerRegistry.Get<...>()).
  - FarmTilesSectionProvider continua recebendo o grid por construtor — só a fonte muda.
### Fase 2 — Farm para de importar Save
  - FarmPlotRegistry.cs/FarmSceneRuntimeBootstrap.cs/FarmTillingInputController.cs passam a referenciar
    o grid via o novo owner Farm, sem precisar de SaveManager.
  - Confirmar que nenhum outro membro de SaveManager era consumido por esses 3 arquivos (se for,
    documentar e resolver caso a caso, sem expandir escopo).
### Fase 3 — Quests|Save
  - Aplicar a correção mínima identificada na Fase 0 (ou documentar BLOCKED conforme 14.3).
### Fase 4 — Testes de round-trip
  - Criar/confirmar EditMode test de round-trip para FarmTilesSectionProvider.
### Fase 5 — Gates e relatório
```

## 21. Ordem de execucao (ordem segura)

```text
1. Preflight: git status, branch dev, snapshot atual.
2. Fase 0 completa antes de qualquer edição.
3. Mover posse do FarmTileGrid (Fase 1-2), gates completos, commit.
4. Quests|Save (Fase 3), gates completos, commit separado.
5. Relatório final consolidado.
```

## Paralelização

```md
- Parallelizable: CONDITIONAL
- Parallel group: arch_residual_core_and_save
- Can run with: specs que não toquem SaveManager.cs/FarmTileGrid.cs/QuestSectionProvider.cs
- Must not run with: spec_arch_core_boundary_residual_v1 (mesma janela de SaveManager.cs/GameBootstrap.cs — rodar em série)
- Shared files/systems that require lock: Save/SaveManager.cs, Farm/FarmTileGrid.cs
- Reason: SaveManager.cs é composition-root paralelo tocado por múltiplas specs de arquitetura; edição
  concorrente arrisca merge conflict e edges não detectados até snapshot final.
```

## 22. Impacto em save/load

```text
Does this change save schema? NO (mesmos campos em FarmTilesSaveData/FarmTileEntry)
Does this add a save section? NO
Does this require migration? NO — JsonUtility serializa por nome de campo, não por namespace/owner;
mover QUEM instancia o grid não muda o que é serializado.
Does this persist Unity references? NO — inalterado, já não persistia.
```

Se a Fase 0 revelar que o schema precisa mudar por algum motivo não previsto aqui, parar e reportar
(stop condition — spec e estado real do repo conflitam).

## 23. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 24. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO (a menos que a Fase 0 prove necessidade de novo wiring de owner Farm nas 3 cenas MVP
— nesse caso, documentar e avaliar se cabe nesta spec ou exige spec própria de regen de cena)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (fluxo de plantio/tilling depende do grid; confirmar que
FarmScene carrega e til/plant/harvest continuam funcionando via smoke)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 25. Riscos técnicos

```text
Risco: FarmSceneRuntimeBootstrap/FarmTillingInputController podem consumir mais de SaveManager do que
só FarmTileGrid (não confirmado nesta auditoria — só o using foi grepado, não o corpo inteiro dos 2
arquivos).
Mitigação: Fase 0 de execução lê os arquivos por inteiro antes de remover o using; se houver outro
consumo real, resolver caso a caso ou documentar como razão para manter using CindarsHope.Save nesse
arquivo específico (nesse caso o par pode não fechar 100% — documentar).

Risco: nenhum owner Farm existente serve, exigindo tipo novo — risco de over-design se a interface for
maior que o necessário.
Mitigação: expor só um getter do grid (mesmo shape que SaveManager.FarmTileGrid hoje); rule
code-minimalism-ladder.

Risco: ordem de inicialização entre o novo owner Farm e o SaveManager/providers pode não estar
garantida (mesmo tipo de risco já documentado nas specs Core|* irmãs).
Mitigação: documentar como risco residual aceito, validar via PlayMode que a FarmScene carrega sem erro
e que tilling/plant/harvest funcionam no smoke.

Risco: Quests|Save pode não ter uma correção mínima limpa dentro do padrão atual de composição de
SaveManager (que constrói ~30 providers diretamente, "por design").
Mitigação: aceitar BLOCKED_BY_SCOPE documentado (14.3) em vez de forçar uma mudança maior de arquitetura
de providers, que está fora de escopo desta spec.
```

## 26. Rollback

```text
Reverter a mudança de posse do FarmTileGrid (SaveManager volta a instanciar).
Reverter a correção de Quests|Save independentemente (commits separados).
Não afeta saves reais em disco — nenhuma migration, nenhum schema alterado.
```

---

# /speckit.tasks

## 27. Tasks

```md
- [ ] T001 — Fase 0: ler FarmTileGrid.cs, FarmPlotRegistry.cs, FarmSceneRuntimeBootstrap.cs, FarmTillingInputController.cs por inteiro; decidir owner Farm.
- [ ] T002 — Mover posse do FarmTileGrid/FarmNonArableZones para o owner Farm decidido.
- [ ] T003 — Atualizar SaveManager.cs/FarmTilesSectionProvider para consumir via referência injetada/resolvida.
- [ ] T004 — Remover using CindarsHope.Save dos 3 arquivos Farm afetados (ou documentar exceção).
- [ ] T005 — Gates do sublote Farm|Save (snapshot, build, EditMode Save/Architecture, PlayMode FarmScene). Commit.
- [ ] T006 — Fase 0 de Quests|Save: revarrer Save/** por CindarsHope.Quests; aplicar correção mínima ou declarar BLOCKED.
- [ ] T007 — Gates do sublote Quests|Save. Commit se aplicável.
- [ ] T008 — EditMode round-trip test de FarmTilesSectionProvider (criar se não existir).
- [ ] T009 — Consolidar relatório final; atualizar MODULARIZATION_PAIR_BREAK_MAP.md e CLAUDE_MODULARIZATION_REMAINING_PLAN.md.
```

## 28. Validações obrigatórias

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save" -ResultsPath "TestResults\save-ownership-save.xml" -LogFile "Logs\save-ownership-save.log"
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture" -ResultsPath "TestResults\save-ownership-arch.xml" -LogFile "Logs\save-ownership-arch.log"
```

PlayMode de composição (`GameRuntimeCompositionRootPlayModeTests`) e, se praticável, um smoke dedicado
de FarmScene (till → plant → harvest) via Unity Test Runner ou execução manual documentada. Se não
puder rodar, `NOT RUN` com motivo e risco residual — nunca inferir PASS de output filtrado.

## 29. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (round-trip de captura/restauração de FarmTilesSaveData; ponto de
  construção do QuestSectionProvider)
- Requires EditMode tests: YES (round-trip de FarmTilesSectionProvider; suíte Save completa como
  regressão)
- Requires PlayMode automated or final human scenario: YES (fluxo de tilling/plant/harvest na FarmScene
  depende do FarmTileGrid; confirmar que continua funcionando após a mudança de posse)
- Requires regression test: YES (suíte EditMode.Save completa é a regressão mínima; nenhum campo de
  FarmTilesSaveData pode mudar de valor para o mesmo estado de grid)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: snapshot confirma Farm|Save fora de MutualModulePairs; build
  7/7 PASS; EditMode Save+Architecture PASS incluindo o round-trip novo/existente; PlayMode de
  composição PASS ou log revisado sem erro novo relacionado a Farm/tilling.
```

## 30. Definition of Done

```text
Farm|Save sai de MutualModulePairs (métrica completa) — critério principal desta spec.
SaveManager.cs não instancia mais FarmTileGrid/FarmNonArableZones diretamente.
Round-trip de save de farm_tiles preservado (mesmo schema, mesmos valores para o mesmo estado).
Quests|Save fechado OU documentado como BLOCKED_BY_SCOPE com motivo específico (não bloqueia DoD).
Build 7/7 e EditMode Save completo verdes.
Relatório de execução documenta snapshot antes/depois e o estado final de cada um dos 2 pares.
Sem claim de PASS de Play Mode humano sem evidência real.
```

## 31. Anti-regressão

```text
Não alterar schema/campo de FarmTilesSaveData/FarmTileEntry sem migration.
Não alterar comportamento de tilling/plant/harvest observável.
Não renomear providerId "farm_tiles"/"quests".
Não usar GameObject.Find/FindObjectOfType em runtime para resolver o novo owner Farm.
Não expandir o escopo do corte de Quests|Save para redesenhar o composition-root paralelo de
SaveManager (débito conhecido, fora de escopo).
```

## 32. Notas para execução posterior

```text
Se um owner Farm dedicado for criado nesta spec, ele é candidato natural a também possuir outros
estados de Farm hoje soltos (fora de escopo aqui, mas registrar a oportunidade no relatório).
Se Quests|Save ficar BLOCKED, documentar como candidato a uma spec futura que ataque o composition-root
paralelo do SaveManager (fora do escopo de qualquer spec residual pontual).
```
