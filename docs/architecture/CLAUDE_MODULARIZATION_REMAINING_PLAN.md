# Plano restante de modularização e eficiência — instruções para Claude

> Data de referência: 2026-07-07  
> Branch esperada: `dev`  
> Objetivo: continuar a modularização/eficiência sem quebrar o jogo, sem regressão de saves, cenas, prefabs, IDs, balanceamento ou fluxos de gameplay.

## Estado final (2026-07-16) — MutualModulePairs=0

`tools/architecture/Get-ModularizationDependencySnapshot.ps1` confirma `MutualModulePairs=0` no
HEAD `a4203461` (branch `dev`) — os 25 pares mútuos restantes deste plano foram fechados pelas 7
specs do lote ARCH_RESIDUAL (`.specs/implementados/spec_arch_core_boundary_residual_v1.md` e
irmãs). Build 7/7 exit 0; EditMode 2845/2845 exit 0. O critério deste documento ("Não declarar
modularização ampla concluída enquanto existirem pares mútuos") está agora satisfeito sob a métrica
completa — **com a ressalva de que a validação humana de Play Mode segue pendente**
(`spec_validation_human_playmode_smoke_v1`, ainda em `.specs/a_implementar/`). Ver header de
evidência de cada spec em `.specs/implementados/` para os commits reais e eventuais desvios
(`spec_arch_npc_quest_boundary_residual_v1` cortou o par só pela direção Quests→NPC e teve os
critérios 14.1/14.2 fechados em seguida, na mesma sessão — ver seu header). O restante deste
documento é histórico/planejamento e não reflete mais o estado atual.

> Nota de escopo: `MutualModulePairs=0` significa ZERO CICLOS entre módulos. Arestas
> unidirecionais (ex.: `NPC → Quests`) continuam existindo e são acoplamento em camadas normal —
> não são débito por si só.

## Atualizacao 2026-07-12 - estado real antes de continuar

Verificacao atual no disco/git:

```text
Branch=dev
origin/dev...dev=0 2
UnityGeneratedProjectsBuild=PASS, 7/7, 0W/0E
EditMode antes da correcao=FAILED, 2836/2837 PASS
EditMode apos a correcao=PASS, 2837/2837 PASS
Snapshot:
  UsingOnlyModuleEdges=213
  UsingOnlyMutualModulePairs=18
  RuntimeModuleEdges=246
  MutualModulePairs=30
Auto-bootstraps reais=3
```

Os 3 `[RuntimeInitializeOnLoadMethod]` reais continuam aceitos por design:

- `GameRuntimeCompositionRoot`
- `SceneTransitionRouter`
- `CollisionDebugOverlayBootstrap`

Bloqueador imediato encontrado nesta rodada: `ArchitectureRatchetTests` detectou aumento
de `Resources.Load` em `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` apos a rodada
concorrente de Cave (`ResourcesLoad` 5 vs baseline 4). A correcao arquitetural escolhida e:

1. `CreateMvpCaveScene` deve serializar `CaveEcosystemBalanceSO` no campo `_ecosystemBalance`.
2. `CaveRuntimeMaterializer` nao deve adicionar novo `Resources.Load`; cenas antigas sem wiring usam
   uma instancia default em memoria com warning.
3. `GenerateCaveEcosystemBalance` deve manter o asset canonico em `Assets/_Game/Data/Cave`, sem
   depender de copia em `Resources` para runtime.

Gate fechado nesta rodada: `ResourcesLoad` voltou ao baseline 4 e EditMode completo passou 2837/2837.
Em seguida, o corte `Core|Economy` foi aplicado via `IGameBootstrapRuntimeService` e `IShopStockRuntime`;
snapshot caiu para `RuntimeModuleEdges=247` e `MutualModulePairs=31`, com build 7/7 e EditMode 2837/2837.
O corte `Core|Equipment` tambem foi aplicado via `IEquipmentRuntime`; snapshot caiu para
`RuntimeModuleEdges=246` e `MutualModulePairs=30`, com build 7/7 e EditMode 2837/2837.
O corte `NPC|World` tambem foi aplicado via `INpcScheduleAvailabilityRuntime` e `INpcDoorTraveler`;
snapshot caiu para `RuntimeModuleEdges=246` e `MutualModulePairs=29`, com build 7/7 e EditMode 2837/2837.
O corte `Farm|UI` tambem foi aplicado movendo `MenuGuiStyle` de UI para Core; snapshot caiu para
`RuntimeModuleEdges=244` e `MutualModulePairs=28`, com build 7/7 e EditMode 2837/2837.
O corte `Craft|UI` tambem foi aplicado via `ICraftingStationModal` no modulo Craft; snapshot caiu para
`RuntimeModuleEdges=243` e `MutualModulePairs=27`, com build 7/7 e EditMode 2837/2837.
O corte `Save|UI` tambem foi aplicado via `IOnboardingHintsRuntime` em Foundation e provider canonico
em `Save.Providers`; snapshot caiu para `RuntimeModuleEdges=242` e `MutualModulePairs=26`, com build
7/7 e EditMode 2837/2837.
O corte `Economy|Equipment` tambem foi aplicado via `IGoldGainModifierRuntime` em Foundation; snapshot
caiu para `RuntimeModuleEdges=241` e `MutualModulePairs=25`, com build 7/7 e EditMode 2837/2837.

Observacao critica: existem specs antigas em `.specs/implementados/` que declaram alguns cortes
`Core|*` como fechados (`Core|Skills`, `Core|Inventory`, `Core|Player`, `Core|UI`, `Core|Save`),
mas o snapshot real abaixo ainda lista esses pares. Ate que o codigo/snapshot confirmem o contrario,
trate essas specs como documentacao historica/adiantada, nao como prova de conclusao.

Pares reais restantes nesta data:

```text
Cave|Combat
Cave|Core
Cave|Enemy
Cave|SceneManagement
Cave|UI
Combat|Core
Combat|Enemy
Combat|Inventory
Combat|Player
Combat|Skills
Core|Inventory
Core|Player
Core|Save
Core|Skills
Core|UI
Equipment|Player
Farm|Save
Inventory|Player
NPC|Quests
NPC|UI
Player|Skills
Player|UI
Player|World
Quests|Save
UI|World
```

Prioridade daqui para frente:

1. Consolidar/commitar/preservar mudancas concorrentes de Cave/Enemy antes de novos cortes grandes.
2. Atacar `Core|*` restantes com ports/registries pequenos, um par por vez, somente quando a troca
   nao exigir alterar contratos de save/gameplay.
3. Atacar `Save|*` restantes com spec de boundary e roundtrip tests.
4. Deixar `Cave|*`, `Combat|*`, `Player|*`, `NPC|*` para specs com PlayMode/smoke humano, pois sao
   fronteiras de gameplay e nao apenas type-move.

## 0. Regra principal

Não declarar “modularização ampla concluída” enquanto existirem pares mútuos no snapshot arquitetural.

O estado atual do código é a fonte canônica. Specs antigas podem estar defasadas; antes de alterar qualquer coisa, valide no disco/git.

### Atualização 2026-07-10 — métrica de snapshot corrigida

O script `tools/architecture/Get-ModularizationDependencySnapshot.ps1` agora conta dependências por:

- `using CindarsHope.X`;
- referências fully-qualified `CindarsHope.X` em código compilado;
- comentários e strings são removidos antes da contagem.

Com isso, a métrica antiga permanece disponível como histórico:

```text
UsingOnlyModuleEdges=211
UsingOnlyMutualModulePairs=18
```

Mas a métrica principal corrigida passa a ser:

```text
RuntimeModuleEdges=248
MutualModulePairs=32
```

Interpretação: `18` era o número de pares usando a regra antiga baseada só em `using`. O debt
semântico real ainda inclui acoplamentos por fully-qualified names, especialmente no `GameBootstrap`.
Daqui em diante, novos cortes devem usar `RuntimeModuleEdges/MutualModulePairs` corrigidos como
fonte de verdade. `UsingOnly*` serve apenas para comparar com o histórico dos commits anteriores.

Nota: a versão final do script ignora declarações `namespace CindarsHope.X` para não contar namespace
como dependência. Após os cortes `Core|Craft` e `Inventory|Magic`, o snapshot atual é:

```text
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=18
RuntimeModuleEdges=248
MutualModulePairs=32
```

Pares reais removidos em 2026-07-10:

- `Core|Craft`: `GameBootstrap` usa `ICraftingRuntimeManager` via `DomainManagerRegistry`.
- `Inventory|Magic`: `SpellSourceType` virou enum puro em `Foundation`.

## 1. Preflight obrigatório

Rode antes de tocar código:

```powershell
git fetch origin dev
git branch --show-current
git status --short
git log --oneline -10
git rev-list --left-right --count origin/dev...dev
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
```

Pare e reporte se:

- a branch não for `dev`;
- houver mudanças inesperadas além das mudanças concorrentes conhecidas;
- o snapshot atual divergir muito sem explicação;
- houver commits ausentes em relação ao histórico abaixo.

## 2. Estado atual verificado em 2026-07-07

Últimos commits relevantes do rework:

```text
8c4bbb93 refactor(arquitetura): mover eventos de npc para dominio
98bacbb6 refactor(arquitetura): mover evento de clima para world
cc28348b refactor(arquitetura): mover catalogo de sementes para farm
2758f6c7 refactor(arquitetura): desacoplar cenas do mundo
ca47de42 refactor(arquitetura): remover ciclo craft save
ecdfa39d refactor(arquitetura): desacoplar respawn da fonte
28dd62aa refactor(arquitetura): desacoplar fonte de progressao final
```

Snapshot atual após esses commits:

```text
RuntimeModuleEdges=227
MutualModulePairs=38
```

### ATUALIZAÇÃO 2026-07-08 — 12 cortes de type-move (38 → 27), teto headless atingido

Claude executou (delegado + verificado, cada gate = snapshot cair): `Player|Save`, `Save|World`,
`Economy|Save`, `NPC|Save`, `Quests|Save`, `Inventory|Save`, `Save|UI`, `Core|Locations`, `Cave|Save`,
`Equipment|Save` (+ derrubou `Crafting|Save` de brinde). **MutualModulePairs = 27.** Specs v20–v29 em
`implementados/`. Técnica: relocar tipo puro (DTO/enum) p/ Foundation ou domínio (JsonUtility é
namespace-agnóstico → sem migration; `git mv` preserva GUID). Ver `MODULARIZATION_PAIR_BREAK_MAP.md`.

**Os 27 restantes NÃO são type-move limpo** — precisam de regen de cena, ports+wiring, ou decisão de
design. Categorias (teto do que é seguro fazer headless = 27):

- **GameBootstrap serializa `[SerializeField] Manager` (regen de cena + port p/ remover o campo):**
  `Core|Craft`, `Core|Economy`, `Core|Enemy`, `Core|Equipment`, `Core|Inventory`, `Core|Player`,
  `Core|Skills`, `Core|UI` (ModalManager), `Core|Save` (SaveManager). Mover só o tipo NÃO derruba —
  o `using`/campo do manager no GameBootstrap mantém a aresta. Exige `PlayerRuntimeReferences`/port +
  regen de cena p/ tirar o `[SerializeField]`.
- **Serialized-field bloqueado (lista 5.2, spec própria):**
  `Player|UI` (ManaManager→ModalManager), `UI|World` (CorpseInteractable→CorpseRecoveryUIController),
  `Player|Skills` (SkillPassiveModifier serializado em SkillNodeDataSO), `NPC|UI`.
- **Gameplay entrelaçado (decisão de design + PlayMode):** `Combat|Core`, `Combat|Enemy`, `Combat|Player`,
  `Combat|Skills`, `Cave|Combat` (EnemyHealth scaling), `Cave|Core`, `Cave|Enemy`, `Player|World`,
  `Inventory|Player`, `NPC|Quests`, `NPC|World`.
- **Outros:** `Farm|Save` (mover posse de FarmTileGrid do SaveManager — médio/grande), `Cave|SceneManagement`
  (corte rejeitado antes, ver 5.1).

Para ir abaixo de 27: abrir o Unity (regen de cena após remover `[SerializeField]` dos managers do
GameBootstrap) e/ou specs de design por par. Não é seguro autonomamente/headless.

Gates recentes passaram:

- `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
- EditMode dos lotes v9/v10/v11/v12: exit 0, 2747/2747 PASS.

## 3. Mudanças concorrentes a preservar

Não inclua nos commits de modularização sem autorização explícita:

```text
Assets/_Game/Resources/EnemySprites/gnome_tinkerer.png
Assets/_Game/Resources/EnemySprites/goblin_shaman.png
Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs
Assets/_Game/Scripts/Enemy/EnemyAnimator.cs
ProjectSettings/ProjectSettings.asset
ProjectSettings/UnityConnectSettings.asset
cindars_hope.slnx
tools/enemy_anim/normalize_enemy_sheets.py
tools/aseprite/_preview_farm_props.py
tools/aseprite/slice_farm_props.py
```

Use sempre `git add` com paths explícitos. Nunca use `git add -A` ou `git add .` neste trabalho.

## 4. Pares mútuos restantes

Revalidar com o snapshot antes de executar. No último estado medido restavam:

```text
Cave|Combat
Cave|Core
Cave|Enemy
Cave|Save
Cave|SceneManagement
Combat|Core
Combat|Enemy
Combat|Player
Combat|Skills
Core|Craft
Core|Economy
Core|Enemy
Core|Equipment
Core|Inventory
Core|Locations
Core|Player
Core|Save
Core|Skills
Core|UI
Craft|UI
Economy|Save
Equipment|Inventory
Equipment|Save
Farm|Save
Inventory|Player
Inventory|Save
NPC|Quests
NPC|Save
NPC|UI
NPC|World
Player|Save
Player|Skills
Player|UI
Player|World
Quests|Save
Save|UI
Save|World
UI|World
```

## 5. O que já foi tentado e deve ser respeitado

### 5.1. Tentativa rejeitada: `Cave|SceneManagement`

Foi testado mover `CaveSceneRuntimeReferenceInstaller` de `SceneManagement` para `Cave.Runtime`.

Resultado:

- removia `Cave|SceneManagement`;
- criava `Cave|UI`;
- quebrou build por resolução de `Resources.Load` dentro do namespace `CindarsHope.Cave.Runtime`.

Decisão: não repetir esse corte como micro-refactor. Se for atacar, criar spec própria para installers de Cave e separar também dependências de UI/combat/save.

### 5.2. Pares avaliados como não seguros para microcorte

Não mexer sem spec específica:

- `Craft|UI`: `CraftingPoint` tem referência serializada opcional para `CraftingModal`.
- `Equipment|Inventory`: `RepairKitManager` usa `InventoryManager`, `ItemDatabaseSO`, `ConsumableSubtype.RepairKit`; `Inventory` usa `EquipmentSlot`.
- `Player|UI`: `ManaManager` tem campo serializado `ModalManager`.
- `UI|World`: `CorpseInteractable` aponta para `CorpseRecoveryUIController`.
- `Core|Locations`: `GameBootstrap` tem campo serializado `AnyaFountain` e consumidores reais usam `bootstrap.AnyaFountain`.
- `Cave|Combat`: `EnemyHealth` usa scaling/vulnerabilidade de Cave em lógica real.
- `Player|Skills` (investigado 2026-07-07, NÃO é microcorte): a direção leve Player→Skills vem de `PlayerVitalsApplier`/`DerivedStatsCalculator` consumirem `SkillTreeManager.GetAllActivePassiveModifiers()`, cujo retorno é `List<SkillPassiveModifier>`. `SkillPassiveModifier` é tipo do módulo Skills, também consumido por Combat (`PlayerDamageReceiver`, `PlayerCombatStatsProvider`, `PlayerAttackController`) E **serializado** em `SkillNodeDataSO.PassiveModifiers`. Quebrar exige mover `SkillPassiveModifier` (+ `SkillModifierType`) para local compartilhado (Core/Foundation) COM migração de serialização dos assets `SkillNodeDataSO`. Spec própria; não tentar como microcorte.
- pares `*|Save`: envolvem DTOs/schema/providers reais; não mover sem plano de compatibilidade.

## 6. Ordem recomendada do que falta fazer

### Fase A — Save Boundary

Prioridade alta. Resolve muitos pares e melhora manutenção.

Pares-alvo:

```text
Economy|Save
Equipment|Save
Farm|Save
Inventory|Save
NPC|Save
Player|Save
Quests|Save
Save|UI
Save|World
Cave|Save
Core|Save
```

Como fazer:

1. Criar spec: `spec_arch_save_boundary_v13.md`.
2. Mapear todos os DTOs usados por domínio:
   - `InventorySaveData`
   - `PlayerSaveData`
   - `NpcManagerSaveData`
   - `QuestStateSectionSaveData`
   - `EquipmentSaveData`
   - `EquipmentDurabilitySaveData`
   - `ShopStockSaveData`
   - `WeaponInfusionSaveData`
   - `CaveSaveData`
   - `WorldTimeSaveData`
3. Não renomear campo serializado sem migration.
4. Separar responsabilidades:
   - schema/DTO simples em namespace canônico de save, ou no domínio dono com provider adaptando;
   - providers por domínio;
   - `SaveManager` só orquestra providers.
5. Manter DTOs simples:
   - sem `MonoBehaviour`;
   - sem `ScriptableObject`;
   - sem `UnityEngine.Object`;
   - evitar refs de cena.
6. Criar/atualizar testes de roundtrip antes de mover DTOs.

Gates mínimos:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -ResultsPath "TestResults\save-boundary-editmode.xml" -LogFile "Logs\save-boundary-editmode.log"
```

Se tocar migration/save load real, rodar também testes focados de save fixtures existentes.

### Fase B — GameBootstrap / Composition Root

Prioridade alta. Ataca os pares `Core|*` que sobraram.

Pares-alvo:

```text
Core|Craft
Core|Economy
Core|Enemy
Core|Equipment
Core|Inventory
Core|Locations
Core|Player
Core|Skills
Core|UI
```

Problema atual:

- `GameBootstrap` ainda concentra fields serializados e properties de vários domínios.
- Muitos sistemas buscam dependências diretamente em `GameBootstrap.Instance`.
- Isso mantém `Core` acoplado a quase todos os domínios.

Como fazer:

1. Criar spec: `spec_arch_gamebootstrap_domain_installers_v14.md`.
2. Para cada domínio, criar installer/registry próprio:
   - `PlayerRuntimeReferences`
   - `InventoryRuntimeReferences`
   - `CraftRuntimeReferences`
   - `EconomyRuntimeReferences`
   - `CombatRuntimeReferences`
   - `WorldRuntimeReferences`
3. `GameBootstrap` deve ficar como composição mínima, não como API pública de todos os managers.
4. Migrar consumidores de `GameBootstrap.Instance.X` para ports específicos.
5. Não remover fields serializados em massa. Fazer por lote e preservar fallback.
6. Um par por commit quando possível.

Não fazer:

- Não trocar field serializado por interface diretamente; Unity não serializa interface por padrão.
- Não renomear campos sem migration ou `[FormerlySerializedAs]`.
- Não remover fallback antes de PlayMode/smoke.

### Fase C — UI Presenter Ports

Prioridade média/alta. Ataca acoplamentos UI e reduz risco futuro de cena.

Pares-alvo:

```text
Craft|UI
NPC|UI
Player|UI
Save|UI
UI|World
Core|UI
```

Casos conhecidos:

- `CraftingPoint -> CraftingModal`
- `ManaManager -> ModalManager`
- `CorpseInteractable -> CorpseRecoveryUIController`
- `NpcController/NpcShopController -> UI/dialogue/modal`
- `Save/UI` por title/system tab/debug HUD/hotbar providers

Como fazer:

1. Criar spec: `spec_arch_ui_presenter_ports_v15.md`.
2. Introduzir ports pequenos:
   - `IModalStateReader`
   - `ICraftingStationPresenter`
   - `ICorpseRecoveryPresenter`
   - `INpcDialoguePresenter`
3. Implementações concretas ficam no módulo UI.
4. Gameplay consome evento/port, não controller visual.
5. Para fields serializados, usar adapter MonoBehaviour concreto quando necessário:
   - o field continua serializável;
   - o domínio consome a interface exposta pelo adapter.
6. Fazer tela por tela, com smoke visual quando possível.

Gates:

- build;
- EditMode;
- PlayMode de composição se mexer em wiring;
- smoke manual/visual se mexer em modal/canvas.

### Fase D — NpcShopController

Prioridade média. Mantém manutenção ruim mesmo depois da modularização parcial.

Objetivo:

Reduzir `NpcShopController` por extração de serviços/collaborators sem mudar comportamento.

Recorte já executado:

- [x] `NpcDebugExpressionChoicePolicy`: IDs, labels e parse de debug expression compartilhados entre `NpcController` e `NpcShopController`.
  - Spec: `spec_arch_npc_debug_expression_policy_v18.md`.
  - Gates usados: build 7/7, EditMode 2747/2747.
  - Snapshot após o recorte: `RuntimeModuleEdges=227`, `MutualModulePairs=38`, sem novo ciclo.
- [x] `NpcCityServiceChoicePolicy`: regra de escolha/compra de serviço cívico saiu do controller.
  - Spec: `spec_arch_npcshop_city_service_policy_v17.md`.
  - Gates usados: build 7/7, EditMode 2747/2747.
  - Snapshot após o recorte: `RuntimeModuleEdges=227`, `MutualModulePairs=38`, sem novo ciclo.
- [x] `NpcSpecialIdentityPolicy`: identificação de Thalindra/Brumdar saiu do controller.
  - Spec: `spec_arch_npcshop_special_identity_policy_v16.md`.
  - Gates usados: build 7/7, EditMode 2747/2747.
- [x] `NpcShopChoiceUiAdapter`: conversão de escolhas de domínio para `DialogueChoice` saiu do controller.
  - Spec: `spec_arch_npcshop_choice_ui_adapter_v15.md`.
  - Gates usados: build 7/7, EditMode 2747/2747.
- [x] `NpcShopServiceChoiceBuilder`: montagem de opções de serviços únicos saiu do controller.
  - Spec: `spec_arch_npcshop_service_choice_builder_v14.md`.
  - Gates usados: build 7/7, EditMode 2747/2747.
- [x] `NpcShopInitializationGuard`: validação de referências obrigatórias saiu do controller.
  - Spec: `spec_arch_npcshop_initialization_guard_v13.md`.
  - Gates usados: build 7/7, EditMode 2747/2747.

Extrair em ordem:

1. `NpcDialogueFlowController`
2. `NpcShopAvailabilityPolicy`
3. `NpcGiftInteractionService`
4. `NpcQuestInteractionBridge`
5. `NpcShopTransactionFacade`
6. `NpcSchedulePresentationAdapter`

Regras:

- `NpcShopController` vira orquestrador fino.
- Não mudar textos/IDs/quest flow.
- Não mudar preços/quantidades.
- Não mudar horários.
- Não mudar schedule anchors.

Gates:

- testes focados de NPC/shop/gifting/quests;
- build;
- EditMode completo.

### Fase E — EnemyBrain

Prioridade média. Melhor ganho de manutenção/performance de gameplay.

Objetivo:

Separar IA em state/strategy/config sem mudar comportamento.

Extrair em ordem:

1. `EnemyTargetingPolicy`
2. `EnemyMovementStateMachine`
3. `EnemyActionSelectionStrategy`
4. `EnemyCombatContext`
5. `EnemyBrainConfigurationResolver`
6. [x] `EnemyDebugTelemetry` — logs one-shot/avisos de diagnóstico (`EnemyThreatExpired`,
   `BossSwapActionSetMissing`, `EliteWardedResistedStatus`, `EliteVolatileExploding`,
   `EnemyPackLeashReset`) saíram do `EnemyBrain` sem mudar texto, guard one-shot ou ponto de disparo.
   Spec: `spec_arch_enemybrain_debug_telemetry_v19.md`. `AnnouncePackEngagementOnce` e os guards de
   gameplay (`_wardedStatusConsumed`, `_volatileExploded`, `_packEngagedAnnounced`) permaneceram no
   brain por serem lógica de gameplay, não telemetria. Snapshot após o recorte:
   `RuntimeModuleEdges=227`, `MutualModulePairs=38`, sem novo ciclo.

Padrões recomendados:

- State Machine explícita;
- Strategy para seleção de ação;
- Blackboard pequeno e imutável por tick quando possível;
- config/tuning fora do `Update`.

Regras:

- Não mexer em dano, cooldown, range, stun, posture, loot ou cave scaling no mesmo lote.
- Não trocar ordem de seleção sem teste/paridade.
- Não usar `FindObjectOfType` runtime.

Gates:

- build;
- EditMode;
- testes focados de enemy/action kits se existirem;
- PlayMode/smoke cave se mexer em movement/targeting.

### Fase F — Gameplay domain boundaries

Prioridade média/baixa, fazer após Save/UI/GameBootstrap.

Pares-alvo:

```text
Combat|Enemy
Combat|Player
Combat|Skills
Cave|Combat
Cave|Enemy
Player|Skills
Player|World
NPC|Quests
NPC|World
Inventory|Player
Equipment|Inventory
```

Como fazer:

- Criar uma spec por par ou grupo pequeno.
- Usar ports e eventos, não mover lógica para “esconder” ciclo.
- Se o par envolve balanceamento ou AI, exigir PlayMode/smoke.

Exemplos:

- `Player|Skills`: mover inferência de classe para um serviço de progressão/skills ou criar port de pontos por árvore.
- `Equipment|Inventory`: extrair contrato de repair-kit/consumable lookup sem quebrar fields serializados.
- `NPC|World`: separar calendário/weather/schedule em value objects/queries comuns.
- `NPC|Quests`: formalizar bridge de quest giver como adapter.

## 7. Melhorias de eficiência ainda pendentes

Além da modularização, avaliar:

1. `Update()` vazio ou polling sem necessidade.
2. `FindObjectsByType`/`Resources.Load` em runtime.
3. Subscriptions duplicadas no `GameEventBus`.
4. Allocations em loops de AI/combat/cave.
5. Rebuilds de UI desnecessários.
6. LINQ em paths quentes.
7. Logs em frequência alta.
8. Caches por cena para referências runtime.
9. Profiler markers em:
   - enemy AI;
   - cave materialization;
   - save/load;
   - inventory/shop transactions;
   - UI refresh;
   - event dispatch.

Não otimizar por suposição. Medir antes/depois quando envolver performance.

## 8. Regras de commit

- Um assunto por commit.
- Mensagem em português.
- Um par removido por commit quando possível.
- Atualizar:
  - spec implementada;
  - `.specs/SPEC_REGISTRY_IMPLEMENTED.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Não fazer push sem autorização humana explícita.

## 9. Gates obrigatórios por lote

Para microcortes seguros:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -ResultsPath "TestResults\<nome-do-lote>-editmode.xml" -LogFile "Logs\<nome-do-lote>.log"
```

Critérios de aceite:

- build exit 0;
- EditMode exit 0;
- `MutualModulePairs` diminui;
- nenhum par novo aparece;
- nenhuma mudança concorrente entra no commit;
- nenhum save/schema/cena/prefab/balanceamento muda sem spec explícita.

Para mudanças em composition/runtime wiring:

- rodar também PlayMode de composição, se disponível.

Para mudanças visuais/UI:

- exigir smoke visual/manual ou PlayMode específico.

## 10. Critério de parada

Pare e reporte em vez de forçar se:

- o corte cria outro par mútuo;
- exige renomear field serializado;
- altera DTO/save schema sem migration;
- mexe em cena/prefab/asset sem escopo;
- muda gameplay/balanceamento;
- depende de decisão de design;
- build/test começa a falhar por motivo não trivial.

## 11. Próximo passo recomendado (ATUALIZADO 2026-07-07 — análise por workflow)

**Ver `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`** — mapa completo dos 20 pares de maior valor
(`*|Save` + `Core|*`) com a direção leve a cortar, estratégia concreta (arquivo:linha), risco e esforço,
+ os findings de eficiência.

**Correção importante:** a premissa antiga de que Save Boundary era "área vermelha do save" foi **refutada**.
JsonUtility serializa por nome de campo (não por namespace) e mover `.cs`+`.meta` preserva GUID — então
relocar DTOs/enums entre namespaces é transparente a saves/assets, **sem migration**. A infra de providers
por domínio (`ISaveSectionProvider` + `SaveProviderRegistry` + ~31 `*SectionProvider`) **já existe**; não é
preciso "criar arquitetura de providers".

**Fazer nesta ordem** (um par por commit; gates: snapshot cai 1, build 7/7, EditMode 2747, round-trip de save
ao mover DTO):

1. **Tier 1 (microcut/low risk, ~7 pares → 38 pode cair p/ ~31):** `Player|Save`, `Core|Skills`, `Core|Player`,
   `Save|World`, `NPC|Save`, `Economy|Save`, `Quests|Save`. Detalhe no mapa.
2. **Tier 2 (small-spec/medium):** `Inventory|Save`, `Save|UI`, `Core|Locations`, `Core|UI`, `Core|Enemy`
   (este exige regen de cena).
3. **Tier 3 (large-spec):** `Equipment|Save`, `Farm|Save`, `Cave|Save`, `Core|Save`, `Core|Craft`,
   `Core|Economy`, `Core|Equipment`, `Core|Inventory` — spec própria por par/grupo. Sinergia: mover o enum
   `EquipmentSlot` p/ Foundation ajuda 3 pares de uma vez.

Não é mais preciso um único `spec_arch_save_boundary_v13.md` monolítico: os cortes são majoritariamente
microcuts independentes. Agrupar por spec só os do Tier 3.
