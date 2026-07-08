# Mapa de quebra dos pares mútuos — análise 2026-07-07 (workflow paralelo, read-only)

Gerado por 20 agents de análise (1 por par mútuo de maior valor: `*|Save` + `Core|*`) + 6 de auditoria de
eficiência. **É análise, não execução** — antes de executar cada corte, revalide no disco (a estratégia
cita arquivo:linha; confira). Complementa `CLAUDE_MODULARIZATION_REMAINING_PLAN.md`.

## Insight-chave (muda a estratégia)

A maioria dos 20 pares quebra pela MESMA técnica: **relocar um tipo puro** (DTO `[Serializable]` ou enum)
para `CindarsHope.Foundation` (leaf sem Unity) OU para o namespace do domínio dono. É **seguro** por dois
motivos comprovados na base:

1. **JsonUtility serializa por NOME DE CAMPO, não por namespace/type-name** → mover um DTO de namespace é
   transparente aos saves existentes no disco. **Sem migration.** (Ainda assim: rodar round-trip de save.)
2. **Mover um `.cs` junto do `.cs.meta` preserva o GUID** → refs serializadas de asset/cena (ScriptableObject,
   MonoBehaviour) sobrevivem sem editar YAML.

Regra geral: **corte sempre a direção LEVE** (menos arquivos). A direção pesada (provider/composition-root →
domínio) é o layering correto e **permanece**. Refs `[SerializeField]` de manager concreto no `GameBootstrap`
são exceção sancionada do composition root (unity-architecture §5) — podem ficar; para cortar o `using`
associado, migra-se o acesso para port/registro tipado.

## Ordem de execução recomendada (risco crescente)

### Tier 1 — microcut / low risk (fazer primeiro; 1 commit cada; ~7 pares → 38 pode cair p/ ~31)

| Par | Direção leve a cortar | Estratégia (resumo) | Risco/esforço |
|---|---|---|---|
| `Player\|Save` (**FEITO** — `.specs/implementados/spec_arch_player_save_cycle_reduction_v20.md`) | PlayerManager→Save (1 arquivo, tipo `PlayerSaveData`) | Inverter: `PlayerSectionProvider` (já importa Player) constrói/restaura o DTO via getters/setters públicos de `PlayerManager`; deletar `using CindarsHope.Save` de `PlayerManager.cs:5`. **Não move schema.** | low / microcut |
| `Core\|Skills` | Core→Skills (`SkillActionDatabaseSO`) | Mover `Core/Data/SkillActionDatabaseSO.cs` (+`.meta`) p/ `Skills/`, namespace `CindarsHope.Skills`. GUID preservado → `.asset` e ref serializada do GameBootstrap sobrevivem. | low / microcut |
| `Core\|Player` | Core.Events→Player (2 enums) | Mover `HazardType` + `PlayerAttributeType` p/ Foundation; reapontar ~8 usings. GameBootstrap/installer mantêm import Player (composition root, ok). | low / small |
| `Save\|UI` (**FEITO** — `.specs/implementados/spec_arch_save_ui_cycle_reduction_v26.md`) | Save→UI (2 sub-arestas: tipos de Hotbar + provider de Onboarding) | `HotbarState`/`HotbarSaveData` (puros) movidos p/ `CindarsHope.Foundation`; `OnboardingHintsSectionProvider` relocado p/ `CindarsHope.UI.Onboarding.Save`, `SaveManager` constroi por nome totalmente qualificado (mesma tecnica de `Quests\|Save`). | low / small |
| `Save\|World` (**FEITO** — `.specs/implementados/spec_arch_save_world_cycle_reduction_v21.md`) | World→Save (2 métodos) | Estreitar assinaturas: `TreeRegistry.RestoreFromSaveData(IReadOnlyList<TreeSaveData>)` e `GameCalendarService.RestoreFromAbsoluteDay(int)`; callers (`WorldSectionProvider`, `FarmSceneRuntimeStateCache`) passam os dados. **Não moveu nenhum DTO.** | low / small |
| `NPC\|Save` (**FEITO** — `.specs/implementados/spec_arch_npc_save_cycle_reduction_v23.md`) | NpcManager→Save (2 DTOs) | Mover `NpcManagerSaveData`/`NpcSaveData` p/ `CindarsHope.NPC` (precedente vivo: `FriendshipSaveData`). Qualificar campo em `SaveData.cs:40`. | low / small |
| `Economy\|Save` (**FEITO** — `.specs/implementados/spec_arch_economy_save_cycle_reduction_v22.md`) | Economy→Save (3 DTOs) | Mover `WeaponInfusionSaveData`/`ShopStockSaveData`/`ShopItemStockEntry` p/ Foundation; trocar `using` em WeaponInfusionRegistry/ShopManager. Deletar `using Economy` morto em `SaveManager.Migration.cs:11`. | low / small |
| `Quests\|Save` (**FEITO** — `.specs/implementados/spec_arch_quests_save_cycle_reduction_v24.md`) | Save→Quests (enum + provider) | Enum `QuestSource` movido p/ Foundation; `QuestSectionProvider` relocado p/ `CindarsHope.Quests.Save`. Nota: não existia o "padrão vivo" de auto-registro no `SaveProviderRegistry` citado aqui — `SaveManager.cs` continua construindo o provider, mas por nome totalmente qualificado (sem novo `using`), mesmo técnica de `NPC\|Save`/`Economy\|Save`. | low / small |

### Tier 2 — small-spec / medium risk

| Par | Direção leve | Estratégia (resumo) |
|---|---|---|
| `Inventory\|Save` (**FEITO** — `.specs/implementados/spec_arch_inventory_save_cycle_reduction_v25.md`) | InventoryManager→Save | Mover `InventorySaveData`/`InventorySlotSaveData`/`InventoryItemSaveData` p/ `CindarsHope.Inventory` (ou Foundation). |
| `Core\|Locations` (**FEITO** — `.specs/implementados/spec_arch_core_locations_cycle_reduction_v27.md`) | Core→Locations (`AnyaFountain`) | Mover a MonoBehaviour trivial `AnyaFountain` (+`.meta`, GUID) p/ `Core.Respawn` junto da interface que já implementa. |
| `Core\|UI` | Core→UI (`ModalManager`, const) | (A) mover `HotbarState.SlotCount` p/ const em Foundation; (B) port `IModalStateProvider` em Foundation p/ `GameTimeManager` — ou gatear o tick por token de pausa (`GameTimeScaleCoordinator`) e largar o tipo UI. |
| `Core\|Enemy` | Core→Enemy (`BestiaryManager`) | Port `IBestiaryKnowledgeProvider` + `BestiaryRuntimeBootstrap` auto-registrando; **exige regen de cena** p/ remover o ref serializado. |

### Tier 3 — large-spec / medium risk (spec própria por par ou grupo)

| Par | Estratégia (resumo) |
|---|---|
| `Equipment\|Save` (**FEITO** — `.specs/implementados/spec_arch_equipment_save_cycle_reduction_v29.md`) | Enum `EquipmentSlot` + DTOs de save de equipment (`EquipmentSaveData`/`EquipmentSlotSaveData`/`EquipmentUpgradeSaveData`/`EquipmentDurabilitySaveData`/`DurabilityEntryData`) movidos para `CindarsHope.Foundation`. 30 arquivos consumidores atualizados. Efeito colateral: `Crafting\|Save` também some do snapshot (não medido isoladamente antes). |
| `Farm\|Save` | Relocar posse de `FarmTileGrid`/`FarmNonArableZones` do SaveManager p/ serviço Farm no composition root + mover `FarmSaveData`/`FarmPlotSaveData` p/ Farm. |
| `Cave\|Save` (**FEITO** — `.specs/implementados/spec_arch_cave_save_cycle_reduction_v28.md`) | Mover `CaveSaveData` p/ `Cave.Runtime` (precedente: `CaveRunSaveData`). |
| `Core\|Save` | `ISaveService` em Core/Foundation p/ GameBootstrap; `GameTimeManager` expõe setters primitivos e o provider lê o DTO. |
| `Core\|Craft` | Mover enum `WorkshopType` p/ Foundation; port/registro p/ `CraftingManager`. |
| `Core\|Economy` | Ports `IEconomyService`/`IShopService`; mover fiação concreta p/ composition root; matar hop reverso `SellableItemPolicy→GameBootstrap.Instance`. |
| `Core\|Equipment` | Mover enum `EquipmentSlot` p/ Foundation (compartilhado com `Core\|Save`/`Equipment\|Save`). |
| `Core\|Inventory` | Mover o wrapper `ItemDatabaseSO` (1 linha) p/ `CindarsHope.Inventory.Data` (GUID preservado). NÃO mover `ItemDataSO` (puxaria Equipment/Magic p/ Core). |

> Nota de sinergia: mover o enum `EquipmentSlot` p/ Foundation ajuda **3 pares** de uma vez
> (`Core\|Equipment`, `Equipment\|Save`, e o field serializado do DTO). Idem `EquipmentSlot`/DTOs de save
> em Foundation viram base para o "Save schema em Foundation".

## Eficiência (Seção 7 — findings REAIS verificados por leitura)

- **[MÉDIO]** `Player/Death/AnyaFountainRespawnFlow.cs:152` — `FindObjectsByType<MonoBehaviour>()` **sem filtro
  de tipo** (varre todos os MonoBehaviours da cena) p/ achar `IAnyaFountainRespawnPoint`. Viola
  unity-architecture #1. Não é per-frame (só no respawn). Fix: registro estático auto-registrado (padrão
  `EnemyHealth.ActiveInstances`); a `AnyaFountain` se registra em OnEnable/OnDisable.
- **[BAIXO]** `Cave/CaveLevelRuntimeController.cs:618` — `FindObjectsByType<ResourceNode>()` a cada
  `DayStartedEvent`. Fix: registro estático de `ResourceNode`.
- **[BAIXO]** `Cave/CaveLevelRuntimeController.cs:187` — `Update()` lê `SceneManager.GetActiveScene().name`
  por frame (aloca string por frame) só p/ um poll de debug (LeftShift+R). Fix: cachear via
  `activeSceneChanged`, ou gatear o poll atrás de `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
- **Sem findings** (bom): Update/FixedUpdate/LateUpdate vazios; subscribe sem Unsubscribe pareado / duplicado;
  LINQ em hot path; Debug.Log em alta frequência sem guard.

## Como usar este mapa

1. Executar Tier 1 na ordem, **um par por commit**, cada um com os gates (`snapshot` deve cair 1; build 7/7;
   EditMode 2747; round-trip de save quando mover DTO).
2. Revalidar a estratégia no disco antes de cada corte (a base pode ter mudado).
3. Marcar o par como feito aqui e em `CLAUDE_MODULARIZATION_REMAINING_PLAN.md`.
4. `git add` com paths explícitos; nunca incluir Cave-decor/arte concorrente.
