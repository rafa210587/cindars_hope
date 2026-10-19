# Runtime bootstrap ownership — residual v5

Data da auditoria: 2026-07-06.

## Resultado

- baseline verificado antes deste lote: 49 atributos reais;
- migrados até o lote anterior: 24 (incluindo Fonte, player condition e inferred class);
- migrados no batch 2: 6 — Cave (`DeathSystemBootstrap`, `CaveRuntimeBridge`,
  `CaveWanderingMerchant`), Combat (`CombatTelemetryService.Bootstrap`), Narrative
  (`NarrativeRuntimeBootstrap`) e Quest (`QuestRuntimeBootstrap`);
- migrados no batch 3: 6 — os componentes anexados ao player (`PlayerStatusReceiver`,
  `AnyaFountainRespawnFlow`, `PlayerDeathController`, `PlayerMovementActionRuntimeBootstrap`,
  `PlayerSprintController`, `PlayerVitalsApplier`);
- migrados no batch 4: 7 — todos os serviços de UI/cena
  (`IntroSequenceController`, `CharacterEquipmentPanelController`, `DeathScreenCanvasController`,
  `GameplayHudBootstrap`, `InventoryPanelController`, `SkillTreeGameplayPanelController`,
  `SceneFadeOverlayBootstrap`);
- migrados no batch 5: 2 — Audio (`AudioManager`, `SfxEventBridge`);
- migrado no batch 6: 1 — diagnóstico NPC (`NpcDialogueExpansionBootstrap`);
- estado atual: 3 atributos reais de `[RuntimeInitializeOnLoadMethod]` em
  `Assets/_Game/Scripts/**` (medido, não estimado);
- restam: composition root (`GameRuntimeCompositionRoot.Bootstrap`, `BeforeSceneLoad` — nunca migra),
  reset de subsistema (`SceneTransitionRouter`, `SubsystemRegistration` — permanece por design) e
  debug (`CollisionDebugOverlayBootstrap`) — este último só migra sob spec própria de define/config,
  não pelo fluxo de composition;
- nenhuma migração restante está autorizada sem preservar o momento de instalação, fallback de cena,
  ownership e teardown descritos abaixo.

## Classificação atual

| Classe | Qtde. | Tratamento |
|---|---:|---|
| Composition root | 1 | autoridade central; manter `BeforeSceneLoad` |
| Reset de subsistema | 1 | manter em `SubsystemRegistration`; não é serviço persistente |
| Debug opt-in | 1 | separar por define/config antes de migrar |
| Diagnóstico sem estado | 0 (migrado no batch 6) | — |
| UI/cena | 0 (migrados no batch 4) | — |
| Componentes anexados ao player | 0 (migrados no batch 3) | — |
| Serviços persistentes de domínio | 0 (Audio migrado no batch 5) | — |

### Reset de subsistema

- `World/Scenes/SceneTransitionRouter.cs`.

### Debug

- debug: `DebugTools/CollisionDebugOverlayBootstrap.cs`;

### Diagnóstico sem estado (migrado no batch 6)

- `NPC/NpcDialogueExpansionBootstrap.cs`.

### UI/cena (migrados no batch 4)

- `Narrative/IntroSequenceController.cs`;
- `UI/Character/CharacterEquipmentPanelController.cs`;
- `UI/Death/DeathScreenCanvasController.cs`;
- `UI/HUD/GameplayHudBootstrap.cs`;
- `UI/InventoryPanelController.cs`;
- `UI/Skills/SkillTreeGameplayPanelController.cs`;
- `World/Scenes/SceneFadeOverlayBootstrap.cs`.

### Componentes anexados ao player (migrados no batch 3)

- `Combat/StatusEffect/PlayerStatusReceiver.cs`;
- `Player/Death/AnyaFountainRespawnFlow.cs`;
- `Player/Death/PlayerDeathController.cs`;
- `Player/Movement/PlayerMovementActionRuntimeBootstrap.cs`;
- `Player/Movement/PlayerSprintController.cs`;
- `Player/PlayerVitalsApplier.cs`.

### Serviços persistentes de domínio (todos migrados)

Migrados: Cave (`DeathSystemBootstrap`, `CaveRuntimeBridge`, `CaveWanderingMerchant`), Combat
(`CombatTelemetryService.Bootstrap`), Narrative (`NarrativeRuntimeBootstrap`), Quest
(`QuestRuntimeBootstrap`), Audio (`AudioManager`, `SfxEventBridge`).

## Migração piloto concluída

`CraftingRuntimeInstaller` instala/desinstala o hook de first-kill. `CaveRuntimeInstaller` cria o
feedback bridge como filho do root. `NpcRuntimeInstaller` instala friendship, gifting, NPC services,
city services e schedule. `WorldRuntimeInstaller` instala weather, world events e item drop.
`FarmRuntimeInstaller` instala animal, forage, lots, resource refresh, daily goal e shipping.
`ItemRuntimeInstaller` instala item use, consumables, magic items, spellbook e crafting station.
`PlayerServiceRuntimeInstaller` instala Fonte e player condition sob o root e tenta anexar inferred
class ao `GameBootstrap`, preservando o gate existente. Os serviços dependentes da
cena são instalados no `Start` do root, mantendo o estágio posterior ao carregamento; os hosts novos
viram filhos do root e os hosts já existentes são adotados sem reparenting. Destruir ou invalidar o
root remove a assinatura estática e os filhos; recriar o root reinstala tudo idempotentemente.

## Migração batch 2 concluída

`CaveSceneRuntimeInstaller` instala, no `Start()` do root, os três serviços de cave dependentes de
cena: `DeathSystemBootstrap.Install(owner)` (corpo/respawn), `CaveRuntimeBridge.Install(owner)`
(bridge de transição) e `CaveWanderingMerchant.Install()` (estático, sem owner — o merchant root
nasce/morre por nível, não é filho do composition root). `CombatTelemetryRuntimeInstaller` chama
`CombatTelemetryService.Bootstrap.Install(owner)`, preservando o toggle debug OFF-por-default: a
instância é sempre criada pelo installer, mas as assinaturas ao `GameEventBus` só ocorrem quando
`CombatTelemetryService.DebugEnabled` é ligado — a migração trocou apenas quem invoca
`EnsureInstance`, não a lógica de `ApplyToggle`/`Subscribe`/`Unsubscribe`. `NarrativeRuntimeInstaller`
e `QuestRuntimeInstaller` chamam `Install(owner)` dos respectivos bootstraps, preservando
coroutines de bind-when-ready, guards de singleton e ordem de dependência (Quest antes de Narrative
na lista do `Start()`, já que `NarrativeRuntimeBootstrap` consome `QuestRuntimeBootstrap.QuestService`
via polling próprio — comportamento inalterado).

## Migração batch 3 concluída

`PlayerLifecycleRuntimeInstaller` instala, no `Start()` do root, os 6 serviços de lifecycle do
player. Cada `Install(Transform owner)` preserva o alvo de attach EXATO do `EnsureInstance()`
original: `PlayerStatusReceiver`, `PlayerSprintController` e `PlayerVitalsApplier` fazem
`AddComponent` num GameObject já existente (statusManager, player, GameBootstrap) e por isso
ignoram `owner` — nenhum host novo é criado, nenhum `SetParent` é chamado, exatamente como antes.
`AnyaFountainRespawnFlow`, `PlayerDeathController` e `PlayerMovementActionRuntimeBootstrap` criam
`new GameObject(...)` standalone com `DontDestroyOnLoad`; nestes, `go.transform.SetParent(owner)`
foi adicionado logo após a criação do GameObject, no mesmo padrão do batch 2. A re-vinculação
per-scene de cada serviço (coroutines de bind-when-ready, `SceneManager.sceneLoaded`, guards de
singleton) permanece 100% intacta — só mudou quem chama a criação inicial do host.

## Migração batch 4 concluída

`PresentationRuntimeInstaller` instala, no `Start()` do root (por último, após lifecycle do
player), os 7 serviços de UI/cena. Todos seguem o mesmo padrão: criavam `new GameObject(...)` +
`DontDestroyOnLoad(go)` num método estático `EnsureInstance`/`EnsureRuntimeInstance` marcado com
`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`. O atributo foi removido e o método virou
`public static void Install(Transform owner)`, com `go.transform.SetParent(owner)` adicionado logo
após a criação do GameObject (mesmo padrão dos batches 2/3) — todos são singletons cross-scene
(`DontDestroyOnLoad`), então o reparent para o root não muda o lifecycle. Nenhuma lógica interna
(subscribes de evento, guards de singleton estático, ordem de criação de canvas, coroutines) foi
alterada — a única mudança é quem/quando chama a criação inicial.

Um teste EditMode pré-existente (`SceneFadeOverlayTests.SceneFadeOverlayBootstrap_HasRuntimeInitializeAttribute`)
assertava a presença do atributo removido; foi atualizado para
`SceneFadeOverlayBootstrap_HasInstallMethod`, que verifica a presença do novo contrato
`Install(Transform)` público estático — cobertura equivalente, sem mudança de comportamento
verificado.

## Migração batch 5 concluída

`AudioRuntimeInstaller` instala, por ÚLTIMO no `Start()` do root, `AudioManager.Install(owner)` e
depois `SfxEventBridge.Install(owner)` (AudioManager primeiro porque o bridge chama `PlaySfx`). Cada
serviço trocou `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)] Bootstrap()` por
`public static void Install(Transform owner)` com `go.transform.SetParent(owner)` após o
`new GameObject`. Toda a lógica interna (pool de vozes, crossfade, `EnsureAudioListener`, cooldown,
Subscribe/Unsubscribe simétrico do bridge) permanece intacta. Verificado: build 7/7, EditMode
2747/2747, PlayMode 2/2; Player.log mostra um único AudioListener criado pelo AudioManager quando a
cena não tem nenhum — comportamento idêntico ao auto-bootstrap original.

## Migração batch 6 concluída

`NpcDialogueExpansionBootstrap` deixou de usar `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` e
passou a expor `Install()`, chamado por `NpcRuntimeInstaller` dentro do `Start()` do
`GameRuntimeCompositionRoot`. O bootstrap não cria GameObject, não assina eventos e não depende de
cena; ele apenas registra logs/avisos de cobertura de diálogo e roster canônico. Por isso a mudança é
somente de ownership de chamada, sem alteração de gameplay, dados, assets, save ou cena.

## Próxima ordem segura

1. debug (`CollisionDebugOverlayBootstrap`) por define/config, fora do fluxo principal de composition
   — exige spec própria, não é migração mecânica;
2. root (`GameRuntimeCompositionRoot`) e `SceneTransitionRouter` permanecem por design.

Nota (batch 5): a hipótese anterior de que Audio exigiria um estágio `AfterSceneLoad` NOVO no root
foi refutada na prática — o `Start()` do root já roda pós-carregamento de cena (AfterSceneLoad-
equivalente). AudioManager instalado por último encontra o AudioListener da cena (se houver) já
existente e cria no máximo um, exatamente como no auto-bootstrap original.

Não contar comentários ou validators Editor que apenas mencionam o atributo; a métrica é obtida por
linhas cujo primeiro token é `[RuntimeInitializeOnLoadMethod` em `Assets/_Game/Scripts/**/*.cs`.
