# SPEC — Corte Residual dos Pares Mútuos `Core|*` (Inventory/Player/Save/Skills/UI)

> **Spec ID:** `spec_arch_core_boundary_residual_v1`
> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-16
> **Wave:** WAVE ARCH — Modularização Residual (continuação do plano `CLAUDE_MODULARIZATION_REMAINING_PLAN.md`)
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Core
> **Parallelizable:** CONDITIONAL
> **Parallel group:** arch_residual_core_and_save
> **Can run with:** specs que não toquem `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`, `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs`, ou os arquivos-alvo listados na seção "Repo lock scope"
> **Must not run with:** `spec_arch_save_ownership_residual_v1` (ambas tocam `SaveManager.cs` e/ou `GameBootstrap.cs` na mesma janela — rodar em série, não em paralelo, mesmo repositório)
> **Repo lock scope:** `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`, `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs`, `Assets/_Game/Scripts/Foundation/**`
> **Depends on (Depende de):**
> - Nenhuma spec pendente. Depende apenas da infraestrutura já existente em `CindarsHope.Foundation` (`DomainManagerRegistry`, `IEquipmentRuntime`, `IModalStateProvider`) e do script `tools/architecture/Get-ModularizationDependencySnapshot.ps1` (versão pós-commit `e9b8c5bc`).
> **Blocks (Bloqueia):**
> - Nenhuma spec futura declarada nomeadamente; reduz o débito medido por `MutualModulePairs` que bloqueia a declaração de "modularização ampla concluída" (rule `code-minimalism-ladder` não se aplica ao gate; o gate relevante é o critério da seção 0 de `CLAUDE_MODULARIZATION_REMAINING_PLAN.md`: "Não declarar modularização ampla concluída enquanto existirem pares mútuos").
> **Scope:** Reduzir, um par por commit, os pares mútuos `Core|Inventory`, `Core|Player`, `Core|Save`, `Core|Skills`, `Core|UI` sob a métrica **corrigida** (`MutualModulePairs`/`RuntimeModuleEdges`, que conta referências fully-qualified além de `using`), eliminando toda ocorrência literal do token `CindarsHope.<Módulo>` dentro de `Core/**` para o módulo-alvo de cada corte — não apenas o `using` de topo de arquivo.
> **Out of scope:** `Core|Enemy`, `Core|Craft`, `Core|Economy`, `Core|Equipment`, `Core|Locations` (já removidos e ausentes do snapshot atual — não re-tocar); qualquer par não-`Core|*`; balanceamento; saves; cenas/prefabs; IDs; UI visual; PlayMode de gameplay além do smoke de composição já existente.

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (2026-07-16)

Os 5 sublotes desta spec foram fechados sob a métrica completa (`MutualModulePairs`), todos via
porta mínima em `CindarsHope.Foundation` + `DomainManagerRegistry.Get<TInterface>()`, exatamente a
técnica prescrita nesta spec (molde `IEquipmentRuntime`/`IModalStateProvider`). Nenhum desvio de
técnica identificado.

```text
Commits reais (branch dev, HEAD a4203461):
ee45510e refactor(arquitetura): cortar par mutuo Core|Skills via porta
         -> ISkillTreeRuntime (Foundation); MutualModulePairs 25 -> 24
a208bba7 refactor(arquitetura): cortar par mutuo Core|UI via ModalType->Foundation + porta
         -> ModalType movido a Foundation, IModalRuntime; MutualModulePairs 12 -> 11
9dc84efa refactor(arquitetura): cortar par mutuo Core|Save via self-resolve + porta
         -> ISaveRuntime (Foundation); MutualModulePairs 10 -> 9
ca4ec23e refactor(arquitetura): cortar par mutuo Core|Inventory via portas + self-resolve
         -> IInventoryRuntime (Foundation); MutualModulePairs 7 -> 6
4435b0f0 refactor(arquitetura): cortar par mutuo Core|Player via portas + self-resolve
         -> IPlayerRuntime/IHungerRuntime/IStaminaRuntime/IManaRuntime/IStatusEffectRuntime/
            IPlayerProgressionRuntime (Foundation); MutualModulePairs 6 -> 5

Validation method: Invoke-UnityGeneratedProjectsBuild.ps1 + RunUnityEditModeTests.ps1 +
Get-ModularizationDependencySnapshot.ps1
Build (7 projects): PASS (exit 0)
EditMode: PASS 2837/2837 (exit 0)
Snapshot: MutualModulePairs=0 (Core|Inventory, Core|Player, Core|Save, Core|Skills, Core|UI ausentes)
Play Mode / validacao humana: NOT RUN - pendente; coberto por spec_validation_human_playmode_smoke_v1
(segue em a_implementar/)
```

**Desvios de técnica:** nenhum. `Core|Save` foi resolvido com uma porta (`ISaveRuntime`), não com a
alternativa mais simples sugerida na seção 9 ("reavaliar Core|Save primeiro sob essa lente antes de
criar uma porta grande") — mas a porta ficou mínima (3 membros: SaveGame/LoadGame/HotbarState),
dentro do espírito de minimalismo pedido; não é um desvio material.

**Residual risk:** rewiring de init do `GameBootstrap` (ordem de `Awake`/registro no
`DomainManagerRegistry` entre managers e o bootstrap) não é garantido pelo Unity sem Script
Execution Order explícito — mesmo risco já documentado nas specs-irmãs v34/v36-v39. Validado apenas
por EditMode; comportamento runtime completo (as 3 cenas MVP carregando sem
missing-script/NullReferenceException, save/load real) depende do playtest humano
(`spec_validation_human_playmode_smoke_v1`).

---

# /speckit.specify

## 5. Contexto

Este é o Tier 3 do plano `docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md` e do mapa
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`. Cinco specs anteriores
(`spec_arch_core_skills_cycle_reduction_v34`, `spec_arch_core_inventory_cycle_reduction_v36`,
`spec_arch_core_player_cycle_reduction_v37`, `spec_arch_core_ui_cycle_reduction_v38`,
`spec_arch_core_save_cycle_reduction_v39`, todas em `.specs/implementados/`, todas com header
`Status: Implementado e BUILD_VALIDATED`) já declararam esses 5 pares como fechados, medindo queda de
`MutualModulePairs` no momento da execução (2026-07-08/09).

**O que mudou desde então:** o commit `e9b8c5bc` ("chore(arquitetura): corrigir snapshot e hardening
de registries", 2026-07-10) reescreveu `Get-ModularizationDependencySnapshot.ps1` para contar não só
`using` de topo de arquivo (`UsingOnlyModuleEdges`/`UsingOnlyMutualModulePairs`), mas **qualquer
ocorrência do token `CindarsHope.<Módulo>`** no corpo do arquivo, incluindo referências fully-qualified
inline (`RuntimeModuleEdges`/`MutualModulePairs`). Essa é hoje a métrica canônica, conforme
`docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md` ("Atualização 2026-07-10 — métrica de
snapshot corrigida"): *"Daqui em diante, novos cortes devem usar `RuntimeModuleEdges/MutualModulePairs`
corrigidos como fonte de verdade. `UsingOnly*` serve apenas para comparar com o histórico"*.

A estratégia usada pelas 5 specs-irmãs (v34/v36-v39) para não reintroduzir o `using` era manter
referências **fully-qualified sem `using`** (ex.: `CindarsHope.Skills.SkillTreeManager.Instance`,
`CindarsHope.Inventory.InventoryManager`). Essa técnica derrubava a métrica antiga (`UsingOnly*`), mas
o token `CindarsHope.Skills`/`CindarsHope.Inventory`/etc. continua presente no arquivo — a métrica
corrigida volta a contar essas 5 arestas como presentes. O próprio
`CLAUDE_MODULARIZATION_REMAINING_PLAN.md` (atualização 2026-07-12) já registra isso explicitamente:

```text
Observacao critica: existem specs antigas em .specs/implementados/ que declaram alguns cortes Core|*
como fechados (Core|Skills, Core|Inventory, Core|Player, Core|UI, Core|Save), mas o snapshot real
abaixo ainda lista esses pares. Ate que o codigo/snapshot confirmem o contrario, trate essas specs
como documentacao historica/adiantada, nao como prova de conclusao.
```

Esta spec é a correção residual: os 5 pares precisam ser fechados de verdade, sob a métrica atual, com
uma técnica que **não deixe nenhum token `CindarsHope.<Módulo>` literal** dentro de `Core/**` para os
módulos Inventory/Player/Save/Skills/UI — só o nome do tipo Foundation (interface/porta) pode aparecer.

## 6. Problema

Sem este corte, o snapshot de arquitetura continua reportando `Core|Inventory`, `Core|Player`,
`Core|Save`, `Core|Skills`, `Core|UI` como pares mútuos ativos — o débito real não caiu, apesar de 5
specs "implementadas" alegarem o contrário. Isso corrói a confiabilidade do próprio processo de
specs/evidência (rule `subagent-results-not-evidence`/`validation-truth`): se uma spec futura reler
essas 5 specs-irmãs como prova de que o corte já foi feito, vai pular trabalho necessário e a métrica
vai continuar estagnada.

## 7. Objetivo

Ao final desta spec, `Get-ModularizationDependencySnapshot.ps1` não lista mais `Core|Inventory`,
`Core|Player`, `Core|Save`, `Core|Skills` ou `Core|UI` na seção `MutualModulePairs` (métrica completa,
não `UsingOnly*`) — para os pares em que isso for possível dentro do escopo headless desta spec (ver
critério de aceite 14.2 para o caso de abortar um sublote específico). Nenhum par novo aparece.
`GameBootstrap.cs` e `GameRuntimeCompositionRoot.cs`/installers não contêm nenhuma ocorrência literal
de `CindarsHope.Inventory`, `CindarsHope.Player`, `CindarsHope.Save`, `CindarsHope.Skills` ou
`CindarsHope.UI` (com a exceção documentada do `[SerializeField]` concreto quando Unity não serializa
interface — ver seção 15).

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md (seção "Atualizacao 2026-07-12" e "Atualização 2026-07-10 — métrica de snapshot corrigida")
docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md (linhas dos 5 pares Core|* já marcados "FEITO", tratar como histórico não confiável)
.specs/implementados/spec_arch_core_skills_cycle_reduction_v34.md
.specs/implementados/spec_arch_core_inventory_cycle_reduction_v36.md
.specs/implementados/spec_arch_core_player_cycle_reduction_v37.md
.specs/implementados/spec_arch_core_ui_cycle_reduction_v38.md
.specs/implementados/spec_arch_core_save_cycle_reduction_v39.md
.specs/implementados/spec_arch_core_equipment_cycle_reduction_v35.md (precedente que FUNCIONA sob a métrica corrigida — ver seção 9)
.claude/rules/unity-architecture.md
.claude/rules/id-stability.md
.claude/rules/code-minimalism-ladder.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
.claude/skills/bootstrap-wiring/SKILL.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão, 2026-07-13/14)

Snapshot real medido nesta sessão (branch `dev`, rodando
`tools/architecture/Get-ModularizationDependencySnapshot.ps1` sem edições pendentes):

```text
CSharpFiles=1632
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
RuntimeModuleEdges=241
MutualModulePairs=25
```

Os 5 pares-alvo desta spec estão presentes em `MutualModulePairs` (métrica completa), mas **ausentes**
de `UsingOnlyMutualModulePairs` — confirmando exatamente o diagnóstico da seção 6: o `using` foi
removido (specs v34/v36-v39 fizeram esse trabalho), mas o token fully-qualified continua no arquivo.

**Evidência ponto-a-ponto, `Core/Bootstrap/GameBootstrap.cs` (grep desta sessão, linhas reais):**

```text
L30  [SerializeField] private CindarsHope.UI.Modal.ModalManager _modalManager;
L35  [SerializeField] private CindarsHope.Save.SaveManager _saveManager;
L36  [SerializeField] private CindarsHope.Player.HungerManager _hungerManager;
L37  [SerializeField] private CindarsHope.Player.StaminaManager _staminaManager;
L38  [SerializeField] private CindarsHope.Player.Data.PlayerDataSO _playerData;
L42  [SerializeField] private CindarsHope.Inventory.Data.ItemDatabaseSO _itemDatabase;
L46  [SerializeField] private CindarsHope.Player.ManaManager _manaManager;
L51  private CindarsHope.Player.Death.CorpseRecoveryManager _corpseRecoveryManager;
L55  public CindarsHope.Player.PlayerManager PlayerManager =>
L56      CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();
L63  public CindarsHope.Inventory.InventoryManager InventoryManager =>
L64      CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
L68  public CindarsHope.UI.Modal.ModalManager ModalManager => _modalManager;
L69  public CindarsHope.Save.SaveManager SaveManager => _saveManager;
L77  public CindarsHope.Player.Progression.PlayerProgressionManager PlayerProgressionManager =>
L78      CindarsHope.Player.Progression.PlayerProgressionManager.Instance;
L80  public CindarsHope.Player.StatusEffectManager StatusEffectManager =>
L81      CindarsHope.Player.StatusEffectManager.Instance;
L143 var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
L148 var playerManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();
L273 var progressionManager = CindarsHope.Player.Progression.PlayerProgressionManager.Instance;
L274 var statusEffectManager = CindarsHope.Player.StatusEffectManager.Instance;
L283 var skillTreeManager = CindarsHope.Skills.SkillTreeManager.Instance;
L317 var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
L384 _manaManager = GetComponent<CindarsHope.Player.ManaManager>();
L390 _manaManager = gameObject.AddComponent<CindarsHope.Player.ManaManager>();
L398 var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
L401 var playerManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();
L404 _corpseRecoveryManager = new CindarsHope.Player.Death.CorpseRecoveryManager(playerManager, inventoryManager, equipmentRuntime);
L423 var statusEffectManager = CindarsHope.Player.StatusEffectManager.Instance;
L446 var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
L454 var playerManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();
```

(Linhas exatas devem ser reconfirmadas na Fase 0 de execução — o arquivo pode ter mudado desde esta
auditoria; usar como mapa aproximado, não como verdade imutável.)

**Precedente que FUNCIONA sob a métrica corrigida:** `Core|Equipment`
(`spec_arch_core_equipment_cycle_reduction_v35`, já ausente de `MutualModulePairs` no snapshot atual —
confirmado nesta sessão) usa um padrão diferente: `GameBootstrap.cs` não contém **nenhum** token
`CindarsHope.Equipment` — só `IEquipmentRuntime` (`CindarsHope.Foundation`, arquivo
`Assets/_Game/Scripts/Foundation/IEquipmentRuntime.cs`, 2 membros: `GetEquippedItem(EquipmentSlot)`,
`EquipItem(EquipmentSlot, string)`) e `EquipmentSlot` (também Foundation). O código real:

```csharp
// GameBootstrap.cs — trecho real, funciona sob a métrica corrigida
var equipmentRuntime = DomainManagerRegistry.Get<IEquipmentRuntime>();
```

Isso é o oposto do padrão usado por v34/v36-v39: em vez de `Get<CindarsHope.Inventory.InventoryManager>()`
(token do módulo concreto), usa `Get<IEquipmentRuntime>()` (token só de Foundation). O mesmo padrão já
existe para `Core|Economy`/`Core|Craft` (`IGameBootstrapRuntimeService`,
`Assets/_Game/Scripts/Core/Bootstrap/IGameBootstrapRuntimeService.cs`) e para `Core|UI`/`GameTimeManager`
(`IModalStateProvider`, `Assets/_Game/Scripts/Foundation/IModalStateProvider.cs`, já usado por
`Core/GameTimeManager.cs`, mas **não** pelo `GameBootstrap.cs` — o campo `_modalManager`/property
`ModalManager` do `GameBootstrap` continua com o tipo concreto `CindarsHope.UI.Modal.ModalManager`).

**Conclusão de Fase 0:** a técnica correta para fechar os 5 pares desta vez é a mesma já comprovada em
`IEquipmentRuntime`/`IGameBootstrapRuntimeService`/`IModalStateProvider` — introduzir uma porta mínima
em `CindarsHope.Foundation` por manager consumido, com só os membros que `GameBootstrap`/
`GameRuntimeCompositionRoot` realmente chamam, e trocar toda referência de tipo concreto por essa porta.
Não é preciso reinventar infraestrutura: `DomainManagerRegistry` já existe e já suporta
`Register<TInterface>(this)`/`Get<TInterface>()` (usado por `IEquipmentRuntime`, `IModalStateProvider`).

**Regra de ratchet a respeitar** (`tools/architecture/architecture-ratchet-rules.tsv`,
citada nas specs-irmãs): `GlobalInventoryAccess` proíbe `InventoryManager.Instance/Active/ActiveInstance`;
`GlobalGoldAccess` proíbe o mesmo para `PlayerManager`/`GoldManager`/`EconomyManager`. Isso não afeta a
estratégia de porta (a porta é resolvida via `DomainManagerRegistry.Get<TInterface>()`, não via
`.Instance` no tipo concreto) — mas deve ser revalidado com `Test-ArchitectureRatchet.ps1` na Fase 0 de
cada sublote.

**O que esta spec NÃO deve recriar:** `DomainManagerRegistry`, `IEquipmentRuntime`, `IModalStateProvider`,
`IGameBootstrapRuntimeService` já existem — reusar. Não criar um segundo registry genérico. Não criar
`ISaveService`/porta de Save nova se um caso mais simples (ex.: mover a última chamada primitiva, como já
fez `Core|Save`/v39 com `GameTimeManager.RestorePhaseState`) resolver com menos código — reavaliar
`Core|Save` primeiro sob essa lente antes de criar uma porta grande de `SaveManager`.

## 10. User stories / engineering stories

```text
Como mantenedor da arquitetura, quero que Core|Inventory/Player/Save/Skills/UI saiam de
MutualModulePairs (métrica completa) de verdade, não só de UsingOnlyMutualModulePairs, para o
snapshot parar de mentir sobre o estado real de acoplamento.
Como agente executor futuro, quero specs implementadas confiáveis — se uma spec disser "par fechado",
o snapshot real deve confirmar, sob a métrica vigente no momento da leitura.
```

## 11. Escopo

Inclui:
- Introduzir portas mínimas novas em `CindarsHope.Foundation` (só as que ainda não existem) para os
  managers concretos hoje referenciados fully-qualified por `GameBootstrap.cs`/installers:
  `IInventoryRuntime` (Inventory), `IPlayerRuntime` (Player — mínimo necessário; `HazardType`/
  `PlayerAttributeType` já são Foundation desde v37, não recriar), `ISkillTreeRuntime` (Skills).
- Reusar `IModalStateProvider` (já existe) para fechar a última referência concreta de `ModalManager`
  em `GameBootstrap.cs` (campo/property), não só em `GameTimeManager` (que já usa a porta desde v38).
- Para `Core|Save`: reavaliar se a única aresta residual (campo/property `SaveManager` concreto em
  `GameBootstrap.cs`) pode ser resolvida com uma porta mínima (`ISaveRuntime`) ou se o esforço é maior
  que os outros 4 pares — se maior, documentar e possivelmente abortar esse sublote isoladamente (ver
  14.2), sem bloquear os outros 4.
- Cada porta expõe **apenas** os membros que `GameBootstrap.cs`/`GameRuntimeCompositionRoot.cs`/
  installers realmente chamam (auditar por Fase 0 de cada sublote — não copiar a API pública inteira do
  manager).
- Um sublote (= um par) por commit, gates completos por sublote.
- EditMode golden/smoke conforme já existe (suíte `Architecture`, `Save`, e a suíte do domínio do par).

Fora:
- Tocar `[SerializeField]` que o Unity não pode substituir por interface (Unity não serializa interface
  diretamente) — nesse caso, o campo serializado mantém o tipo concreto (precedente `_modalManager`,
  `_saveManager`, `_itemDatabase`, `_staminaManager`/`_manaManager`/`_hungerManager`/`_playerData`
  já documentado como aceito nas specs-irmãs); o que muda é a **property pública**/variável local que
  hoje devolve o tipo concreto — essa deve devolver a porta Foundation onde o consumidor só precisa da
  porta. Onde o `[SerializeField]` é inevitável, documentar como risco residual aceito (mesmo padrão
  das specs-irmãs) — não é uma "meia-vitória": o campo serializado sozinho, sem property pública/uso
  fully-qualified em outro ponto do arquivo, **não** dispara o token-scan do snapshot (o regex casa
  `CindarsHope\.` em qualquer lugar do arquivo, então mesmo um campo puro `[SerializeField]` com tipo
  concreto AINDA conta como aresta — ver seção 15 para a exceção sancionada e como ela é medida).

## 12. Fora de escopo

```text
Não inclui: Core|Enemy, Core|Craft, Core|Economy, Core|Equipment, Core|Locations (já fechados, não
re-tocar); qualquer par não-Core|*; mudança de gameplay/balance/IDs/saves/cenas/prefabs; validação
humana imediata (fica DEFERRED_TO_FINAL_VALIDATION); regen de cena (a menos que um sublote prove que é
estritamente necessário e a Fase 0 desse sublote documente o porquê antes de propor).
```

## 13. Regras de não duplicação

```text
Não recriar DomainManagerRegistry (Foundation) — já existe e é genérico.
Não recriar IEquipmentRuntime/IModalStateProvider/IGameBootstrapRuntimeService — reusar como estão.
Não criar um segundo mecanismo de "porta"/"service locator" — usar sempre DomainManagerRegistry.Get<T>().
Não mover lógica de gameplay dos managers para Core — só extrair a assinatura mínima de porta.
Não duplicar HazardType/PlayerAttributeType/EquipmentSlot/QuestSource (já em Foundation).
```

## 14. Critérios de aceite

### 14.1 Pares fechados sob a métrica completa

- Para cada sublote concluído (Inventory, Player, Skills, UI, e Save se viável): rodar
  `Get-ModularizationDependencySnapshot.ps1` antes e depois; o par correspondente sai de
  `MutualModulePairs` (não só de `UsingOnlyMutualModulePairs`, que já estava fora desde as specs-irmãs).
  Nenhum par novo aparece em nenhum dos dois conjuntos.
- Evidência esperada: output bruto do script, colado no relatório de execução, antes/depois de cada
  sublote.

### 14.2 Sublote pode ser abortado individualmente sem bloquear os demais

- Se um sublote (tipicamente `Core|Save`, o de maior risco) exigir regen de cena, mudança de contrato
  de save, ou qualquer coisa fora do escopo headless desta spec, esse sublote específico é documentado
  como `NOT ATTEMPTED` ou `BLOCKED` com motivo, e os demais sublotes seguem normalmente. A spec não
  falha inteira por causa de um sublote — critério de aceite geral é "pelo menos os sublotes viáveis
  headless fecham; os inviáveis ficam documentados para spec futura".

### 14.3 Zero token literal do módulo removido em `Core/**`

- Grep de `CindarsHope\.Inventory` (ou Player/Skills/UI/Save, conforme sublote) em
  `Assets/_Game/Scripts/Core/**` retorna zero ocorrências em código de produção, exceto:
  - o `[SerializeField]` documentado como exceção sancionada (seção 15), e
  - comentários explicando a exceção (não contam para o snapshot, que remove trivia antes de contar —
    confirmado por leitura do próprio script, `Remove-CSharpTrivia`).

### 14.4 Build e testes

- `Invoke-UnityGeneratedProjectsBuild.ps1` exit 0, 7/7 projetos, 0 warnings, 0 errors, por sublote.
- `RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"` exit 0, sem
  regressão de contagem.
- `RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"` exit 0 (crítico — nenhum
  sublote deve tocar schema de save).
- Suíte EditMode do domínio do sublote (ex. `CindarsHope.Tests.EditMode.Skills` para o sublote Skills)
  exit 0.
- `Test-ArchitectureRatchet.ps1` exit 0 — nenhuma violação de `GlobalInventoryAccess`/`GlobalGoldAccess`
  nova.

## 15. Exceção sancionada — `[SerializeField]` concreto

Unity não serializa campos de tipo interface. Onde o campo `[SerializeField]` precisa continuar apontando
para o tipo concreto (ex.: `_saveManager`, `_itemDatabase`, `_staminaManager`), essa **única** linha do
campo serializado é a exceção documentada — mas ela sozinha **ainda conta como aresta** no snapshot
atual (o regex não distingue `[SerializeField]` de outro uso). Ou seja: para sublotes onde a única forma
de zerar o token é através de um campo serializado que não pode virar interface, o par **não pode ser
zerado por esta spec** sob a métrica atual — isso deve ser identificado já na Fase 0 do sublote e, se for
o caso, o sublote é declarado `BLOCKED_BY_UNITY_SERIALIZATION_LIMIT` (conforme 14.2), não forçado.
Para os sublotes Inventory/Player/Skills, a Fase 0 desta sessão não encontrou nenhum campo
`[SerializeField]` do tipo concreto do módulo removido (`InventoryManager`/`PlayerManager`/
`SkillTreeManager` não são hoje campos serializados em `GameBootstrap.cs` — só resolvidos via
`DomainManagerRegistry`/`.Instance`), então esses 3 sublotes não deveriam esbarrar nesta exceção. Os
sublotes UI (`_modalManager`) e Save (`_saveManager`) **têm** campo serializado do tipo concreto — a
Fase 0 de cada um deve reavaliar se isso bloqueia o corte completo ou se a porta ainda reduz o número de
ocorrências o suficiente para a aresta desaparecer (não deveria — uma ocorrência já basta) e, se
bloquear, aplicar 14.2.

---

# /speckit.plan

## 16. Arquitetura alvo

```text
Assets/_Game/Scripts/Foundation/
  IInventoryRuntime.cs      (novo — membros mínimos usados por GameBootstrap)
  IPlayerRuntime.cs         (novo — membros mínimos usados por GameBootstrap para PlayerManager)
  ISkillTreeRuntime.cs      (novo — membro mínimo: o que GameBootstrap.SkillTreeManager.Instance chama)
  IModalStateProvider.cs    (existente — reusar para a property pública ModalManager do GameBootstrap)

Assets/_Game/Scripts/Inventory/InventoryManager.cs      (implementa IInventoryRuntime, registra no DomainManagerRegistry)
Assets/_Game/Scripts/Player/PlayerManager.cs             (implementa IPlayerRuntime, idem)
Assets/_Game/Scripts/Skills/SkillTreeManager.cs          (implementa ISkillTreeRuntime, idem — mantém static Instance existente)
Assets/_Game/Scripts/UI/Modal/ModalManager.cs            (já implementa IModalStateProvider — sem mudança)

Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs     (properties/variáveis locais trocam de tipo concreto para porta)

docs/validation/
  spec_arch_core_boundary_residual_v1_execution_report.md
```

## 17. Contratos, dados e eventos

### 17.1 Data contracts
Nenhum novo DTO. Interfaces de runtime puras (Foundation), sem estado.

### 17.2 Runtime contracts
`IInventoryRuntime`/`IPlayerRuntime`/`ISkillTreeRuntime` — assinatura definida na Fase 0 de cada
sublote, a partir da leitura real de quais métodos `GameBootstrap`/`GameRuntimeCompositionRoot` chamam
hoje (não copiar API pública inteira do manager — porta mínima, rule `code-minimalism-ladder`).

### 17.3 Event contracts
N/A — nenhum evento novo ou alterado.

### 17.4 Save contracts
N/A — nenhum DTO de save tocado. `Core|Save`, se atacado, muda só a forma como `GameBootstrap` resolve
a referência ao `SaveManager`, não o schema.

### 17.5 UI contracts
N/A — `ModalManager` já implementa `IModalStateProvider`; nenhuma mudança de comportamento visual.

## 18. Sistemas afetados

```text
Core (GameBootstrap, composition root/installers)
Foundation (novas portas)
Inventory, Player, Skills (managers passam a implementar a porta correspondente)
UI (ModalManager — sem mudança de comportamento, só consumo pelo GameBootstrap)
Save (só se o sublote Core|Save for viável — ver 14.2)
```

## 19. Arquivos permitidos

```text
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Core/Bootstrap/Installers/**
Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs
Assets/_Game/Scripts/Foundation/**
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Skills/SkillTreeManager.cs
Assets/_Game/Scripts/UI/Modal/ModalManager.cs (só se necessário; auditar antes)
Assets/_Game/Tests/EditMode/Core/**
Assets/_Game/Tests/EditMode/Architecture/**
tools/architecture/architecture-ratchet-baseline.tsv (só se um novo `static Instance`/`SingletonDeclaration` for adicionado)
docs/validation/**
docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md (só para registrar os pares fechados/tentados — não reescrever o documento inteiro)
docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md (só para corrigir a coluna dos 5 pares desta spec)
```

## 20. Arquivos proibidos

```text
Assets/**/*.unity, *.prefab, *.asset salvo autorização explícita
Assets/_Game/Scripts/Save/SaveData.cs (schema de save — não tocar)
Assets/_Game/Scripts/Save/Providers/** (fora de escopo — spec irmã spec_arch_save_ownership_residual_v1 cuida de Save)
docs_old/**, docs/archive/**
Packages/**, ProjectSettings/**
```

## 21. Estratégia de implementação

```md
### Fase 0 — Auditoria por sublote (repetir para cada um dos 5 pares)
  - Revalidar no disco que o par ainda está em MutualModulePairs.
  - Grep de CindarsHope.<Módulo> em Core/** para listar cada ocorrência real (linha atual, pode ter
    mudado desde a auditoria desta spec).
  - Para cada ocorrência, decidir: (a) vira porta Foundation nova; (b) já existe porta reusável; (c)
    é [SerializeField] que não pode virar interface — documentar como exceção/bloqueio.
### Fase 1 — Sublote Inventory
  - Criar IInventoryRuntime (Foundation) com os membros mínimos.
  - InventoryManager implementa a interface; Register<IInventoryRuntime>(this)/Unregister<IInventoryRuntime>().
  - GameBootstrap.cs: propriedade InventoryManager e todas as variáveis locais trocam de
    CindarsHope.Inventory.InventoryManager para IInventoryRuntime.
  - Gates completos (seção 29). Commit.
### Fase 2 — Sublote Player
  - Mesma técnica para PlayerManager -> IPlayerRuntime.
  - Gates completos. Commit.
### Fase 3 — Sublote Skills
  - Mesma técnica para SkillTreeManager -> ISkillTreeRuntime (mantém static Instance existente,
    internamente; GameBootstrap para de referenciar o tipo concreto).
  - Gates completos. Commit.
### Fase 4 — Sublote UI (ModalManager em GameBootstrap)
  - Reusa IModalStateProvider já existente; GameBootstrap.ModalManager (property pública) e o campo
    interno trocam para a porta onde os consumidores só precisam de HasActiveModal, OU documentar por
    que os ~40 consumidores de bootstrap.ModalManager precisam de mais API do que a porta oferece (nesse
    caso, ampliar IModalStateProvider com os membros mínimos adicionais realmente usados, sem copiar a
    API inteira do ModalManager).
  - Gates completos. Commit.
### Fase 5 — Sublote Save (tentativa condicional)
  - Auditar se GameBootstrap.SaveManager (property + os 9 consumidores externos) pode ser reduzido a uma
    porta ISaveRuntime mínima, ou se o [SerializeField] concreto por si só já bloqueia (seção 15).
  - Se viável: aplicar a mesma técnica. Se não: declarar BLOCKED_BY_UNITY_SERIALIZATION_LIMIT ou
    BLOCKED_BY_SCOPE, documentar e não forçar.
  - Gates completos (mesmo se BLOCKED — rodar o snapshot final confirmando o estado).
### Fase 6 — Relatório final
  - Consolidar snapshot final (todos os sublotes), atualizar MODULARIZATION_PAIR_BREAK_MAP.md e
    CLAUDE_MODULARIZATION_REMAINING_PLAN.md com o estado real (sublotes fechados vs. bloqueados).
```

## 22. Ordem de execucao (ordem segura)

```text
1. Preflight: git status, branch dev, snapshot atual.
2. Fase 0 de cada sublote antes de escrever qualquer código desse sublote.
3. Um sublote por commit, gates completos antes de passar para o próximo.
4. Se um sublote falhar ou bloquear, documentar e seguir para o próximo (não travar a spec inteira).
5. Relatório final consolidado.
```

## Paralelização

```md
- Parallelizable: CONDITIONAL
- Parallel group: arch_residual_core_and_save
- Can run with: specs que não toquem GameBootstrap.cs/GameRuntimeCompositionRoot.cs/Foundation/**
- Must not run with: spec_arch_save_ownership_residual_v1 (mesmo arquivo GameBootstrap.cs/SaveManager.cs
  potencialmente tocado por ambas — rodar em série)
- Shared files/systems that require lock: GameBootstrap.cs, GameRuntimeCompositionRoot.cs, Foundation/**
- Reason: ambas as specs residuais desta wave tocam pontos centrais de composição; edição concorrente
  arrisca conflito de merge e edges novos não detectados até o snapshot final.
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A — nenhum DTO novo
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (comportamento visual idêntico; só a origem da referência ao ModalManager muda no sublote 4)
Changes scenes: NO (a menos que a Fase 0 de algum sublote prove necessidade — não esperado)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (composição do bootstrap é gameplay-crítica — confirmar que as
3 cenas MVP carregam sem missing-script/NullReferenceException após cada sublote)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: ordem de Awake entre o manager concreto (que registra no DomainManagerRegistry) e
GameBootstrap.InitializeManagers() (que resolve a porta) não é garantida pelo Unity sem Script
Execution Order explícito — mesmo risco documentado e aceito nas specs-irmãs v36-v38.
Mitigação: mesmo padrão de warning existente quando a resolução retorna null; validar via PlayMode que
as 3 cenas MVP carregam sem erro (mesma evidência empírica usada nas specs-irmãs).

Risco: ampliar a porta além do mínimo necessário reintroduz acoplamento amplo "pela porta dos fundos".
Mitigação: cada porta só ganha os membros que a Fase 0 do sublote confirmar como efetivamente chamados
por Core; qualquer membro extra exige justificativa explícita no relatório.

Risco: sublote Save pode não ser viável headless (campo serializado concreto inevitável).
Mitigação: critério de aceite 14.2 permite abortar esse sublote isoladamente sem bloquear os outros 4.
```

## 27. Rollback

```text
Reverter cada sublote independentemente (um commit por sublote facilita o rollback seletivo).
Remover as interfaces novas se o corte for revertido.
Não afeta dados de save nem estado em disco do usuário.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Fase 0: revalidar snapshot atual e mapear ocorrências reais de CindarsHope.Inventory em Core/**.
- [ ] T002 — Sublote Inventory: criar IInventoryRuntime, atualizar InventoryManager e GameBootstrap.cs.
- [ ] T003 — Gates do sublote Inventory (snapshot, build, EditMode Architecture/Save/Inventory, PlayMode). Commit.
- [ ] T004 — Fase 0 + sublote Player: criar IPlayerRuntime, atualizar PlayerManager e GameBootstrap.cs.
- [ ] T005 — Gates do sublote Player. Commit.
- [ ] T006 — Fase 0 + sublote Skills: criar ISkillTreeRuntime, atualizar SkillTreeManager e GameBootstrap.cs.
- [ ] T007 — Gates do sublote Skills. Commit.
- [ ] T008 — Fase 0 + sublote UI: reusar IModalStateProvider em GameBootstrap.cs (ou ampliar minimamente).
- [ ] T009 — Gates do sublote UI. Commit.
- [ ] T010 — Fase 0 + tentativa do sublote Save: aplicar porta ou declarar BLOCKED com motivo.
- [ ] T011 — Gates do sublote Save (mesmo se BLOCKED, rodar snapshot final). Commit se aplicável.
- [ ] T012 — Consolidar relatório final; atualizar MODULARIZATION_PAIR_BREAK_MAP.md e CLAUDE_MODULARIZATION_REMAINING_PLAN.md.
```

## 29. Validações obrigatórias (por sublote)

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\architecture\Test-ArchitectureRatchet.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture" -ResultsPath "TestResults\core-boundary-<sublote>-arch.xml" -LogFile "Logs\core-boundary-<sublote>-arch.log"
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save" -ResultsPath "TestResults\core-boundary-<sublote>-save.xml" -LogFile "Logs\core-boundary-<sublote>-save.log"
```

Suíte EditMode do domínio do sublote (ex. `CindarsHope.Tests.EditMode.Skills`, `...Player`,
`...Inventory`, `...UI`) quando existir. PlayMode de composição
(`GameRuntimeCompositionRootPlayModeTests`) via Unity Test Runner ou `Unity.exe -batchmode -nographics
-runTests -testPlatform PlayMode`, se praticável; senão `NOT RUN` com motivo.

Se qualquer comando não puder rodar, registrar `NOT RUN` com motivo e risco residual — nunca inferir
PASS de output filtrado (rule `validation-truth`).

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (resolução de referência de manager no composition root)
- Requires EditMode tests: YES (suíte Architecture já cobre a contagem de padrões proibidos/singletons;
  reusar, sem exigir teste novo dedicado a menos que a Fase 0 de um sublote identifique gap)
- Requires PlayMode automated or final human scenario: YES (carregamento das 3 cenas MVP sem
  missing-script/NullReferenceException é o smoke mínimo; PlayMode automatizado já existe
  — GameRuntimeCompositionRootPlayModeTests — e deve ser reexecutado por sublote)
- Requires regression test: YES (nenhum comportamento de gameplay pode mudar; suíte EditMode completa do
  domínio tocado é a regressão)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED por sublote: snapshot antes/depois (par sai de
  MutualModulePairs), build 7/7 PASS, EditMode Architecture+Save+domínio PASS, PlayMode de composição
  PASS ou log revisado sem erro novo.
```

## 31. Definition of Done

```text
Pelo menos os sublotes viáveis headless (Inventory, Player, Skills — Fase 0 desta sessão não encontrou
bloqueio de [SerializeField] para esses 3) fecham de verdade sob MutualModulePairs (métrica completa).
Sublote UI fecha ou é documentado como bloqueado com motivo específico.
Sublote Save fecha ou é documentado como BLOCKED com motivo específico — não é obrigatório para DoD.
Nenhum par novo aparece em nenhum momento.
Build 7/7 e EditMode completo verdes na versão final.
Relatório de execução documenta, por sublote: estado final, snapshot antes/depois, gates.
Sem claim de "modularização Core|* concluída" além do que o snapshot final confirmar.
```

## 32. Anti-regressão

```text
Não alterar schema/campo de save.
Não alterar cena/prefab sem autorização.
Não alterar comportamento de gameplay observável (loadout inicial, modal behavior, skill tree,
inventory, player stats) — só a origem da referência de tipo.
Não usar GameObject.Find/FindObjectOfType em runtime.
Não reintroduzir static Instance em tipos cobertos por GlobalInventoryAccess/GlobalGoldAccess.
Não criar uma segunda spec-irmã que reafirme "par fechado" sem rodar o snapshot real primeiro.
```

## 33. Notas para execução posterior

```text
As specs v34/v36-v39 permanecem em .specs/implementados/ como estavam (não movidas, não deletadas —
rule docs-governance) mas esta spec deve deixar claro nos documentos vivos (MODULARIZATION_PAIR_BREAK_MAP.md,
CLAUDE_MODULARIZATION_REMAINING_PLAN.md) que a métrica correta de "par fechado" é a completa
(MutualModulePairs), com data de reconfirmação.
Se o sublote Save ficar bloqueado, ele é candidato a uma spec própria futura (ex.:
spec_arch_core_save_boundary_v2), fora desta.
```
