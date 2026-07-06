# Runtime bootstrap ownership — residual v5

Data da auditoria: 2026-07-05.

## Resultado

- baseline verificado antes deste lote: 49 atributos reais;
- migrados até o lote anterior: 24 (incluindo Fonte, player condition e inferred class);
- migrados neste lote (batch 2): 6 — Cave (`DeathSystemBootstrap`, `CaveRuntimeBridge`,
  `CaveWanderingMerchant`), Combat (`CombatTelemetryService.Bootstrap`), Narrative
  (`NarrativeRuntimeBootstrap`) e Quest (`QuestRuntimeBootstrap`);
- estado atual: 19 atributos reais de `[RuntimeInitializeOnLoadMethod]` em
  `Assets/_Game/Scripts/**` (medido, não estimado);
- restam apenas Audio (`AudioManager`, `SfxEventBridge`) como serviços persistentes de domínio, mais
  os 16 casos classificados como UI/cena, player-attached, debug, diagnóstico e reset de subsistema;
- nenhuma migração restante está autorizada sem preservar o momento de instalação, fallback de cena,
  ownership e teardown descritos abaixo.

## Classificação atual

| Classe | Qtde. | Tratamento |
|---|---:|---|
| Composition root | 1 | autoridade central; manter `BeforeSceneLoad` |
| Reset de subsistema | 1 | manter em `SubsystemRegistration`; não é serviço persistente |
| Debug opt-in | 1 | separar por define/config antes de migrar |
| Diagnóstico sem estado | 1 | remover de runtime ou mover para validator em spec própria |
| UI/cena | 7 | installer de apresentação após cena; exige fallback e smoke visual |
| Componentes anexados ao player | 6 | installer de player lifecycle, não root global direto |
| Serviços persistentes de domínio | 8 | migrar por domínio, com ordem e teardown explícitos |

### Reset de subsistema

- `World/Scenes/SceneTransitionRouter.cs`.

### Debug e diagnóstico

- debug: `DebugTools/CollisionDebugOverlayBootstrap.cs`;
- diagnóstico: `NPC/NpcDialogueExpansionBootstrap.cs`.

### UI/cena

- `Narrative/IntroSequenceController.cs`;
- `UI/Character/CharacterEquipmentPanelController.cs`;
- `UI/Death/DeathScreenCanvasController.cs`;
- `UI/HUD/GameplayHudBootstrap.cs`;
- `UI/InventoryPanelController.cs`;
- `UI/Skills/SkillTreeGameplayPanelController.cs`;
- `World/Scenes/SceneFadeOverlayBootstrap.cs`.

### Componentes anexados ao player

- `Combat/StatusEffect/PlayerStatusReceiver.cs`;
- `Player/Death/AnyaFountainRespawnFlow.cs`;
- `Player/Death/PlayerDeathController.cs`;
- `Player/Movement/PlayerMovementActionRuntimeBootstrap.cs`;
- `Player/Movement/PlayerSprintController.cs`;
- `Player/PlayerVitalsApplier.cs`.

### Serviços persistentes de domínio

- Audio (ainda auto-bootstrap, deferido): `AudioManager`, `SfxEventBridge`.

Migrados: Cave (`DeathSystemBootstrap`, `CaveRuntimeBridge`, `CaveWanderingMerchant`), Combat
(`CombatTelemetryService.Bootstrap`), Narrative (`NarrativeRuntimeBootstrap`), Quest
(`QuestRuntimeBootstrap`).

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

## Próxima ordem segura

1. player lifecycle somente após existir um `PlayerRuntimeInstaller` ligado ao spawn/despawn;
2. apresentação somente com PlayMode e smoke visual por cena;
3. audio por último: instalar antes da cena altera a decisão de criar `AudioListener` e não é seguro
   sem um estágio `AfterSceneLoad` explícito no root — este estágio ainda não existe no
   `GameRuntimeCompositionRoot` e deve ser criado antes de migrar Audio.

Não contar comentários ou validators Editor que apenas mencionam o atributo; a métrica é obtida por
linhas cujo primeiro token é `[RuntimeInitializeOnLoadMethod` em `Assets/_Game/Scripts/**/*.cs`.
