# Retro-Spec 08 — WI-25 NPC Schedule Runtime + Expansão de Diálogo (registries)

> **Spec ID:** `spec_retro_08_wi25_npc_schedule_runtime_dialogue_expansion`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** NPC / Schedules / Dialogue coverage
> **Código que documenta:**
> - `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleService.cs`
> - `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleProfile.cs`
> - `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlock.cs` (+ enum `NpcTimeBlock`)
> - `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleAnchor.cs`
> - `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleRuntimeState.cs`
> - `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleRuntimeBootstrap.cs`
> - `Assets/_Game/Scripts/NPC/NpcDialogueExpansionBootstrap.cs`
> - `Assets/_Game/Scripts/NPC/NpcDialogueSetRegistry.cs`
> - `Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs` (WAVE12C, consumido)
> **Evidência de execução:** `docs/validation/WAVE_INTEGRATION_25_TOWN_NPC_SCHEDULES_DIALOGUE_REPORT.md` (BUILD_VALIDATED_WITH_SCENE_WIRING_DEBT_AND_TIME_BLOCK_DEBT_PENDING_HUMAN_PLAYMODE) + matrizes ROSTER/ANCHOR/DIALOGUE_EXPANSION/SAVE_LOAD; validador `ValidateWave25TownNpcSchedulesDialogue` (42 checks)
> **Supersedida/complementada por:** `fable_19` (reconciliação de serviços/horários da cidade — fecha o TIME_BLOCK_DEBT com transições intra-dia), `fable_28` (condições/pools de diálogo), `fable_11` (interiores + anchors de schedule em cena). O conteúdo textual dos diálogos é da retro-spec 05 (`TownNpcDialogueLibrary`).

---

# /speckit.specify

## Contexto

A WAVE12C posicionou os 23 NPCs canônicos com placement markers; faltava qualquer noção de rotina diária e uma forma auditável de garantir cobertura de diálogo. A WAVE_INTEGRATION_25 criou a infraestrutura mínima de schedule (resolução por dia — intra-dia explicitamente diferido como `TIME_BLOCK_DEBT`) e registries estáticos de roster/diálogo que validam a cobertura em runtime e nunca divergem do conteúdo.

## Comportamento implementado

### 1. Modelo de schedule (dados)

- `NpcTimeBlock`: `Default=0, Morning=1, Midday=2, Evening=3, Night=4` — enum pronto; somente `Default` é usado hoje (TIME_BLOCK_DEBT).
- `NpcScheduleBlock` (`[Serializable]`): `TimeBlock, AnchorId, SceneId, ActivityLabel, CanInteract=true`.
- `NpcScheduleProfile` (`[Serializable]`): `ScheduleId, NpcId, List<NpcScheduleBlock>` — contrato de dados para fable_19.
- `NpcScheduleRuntimeState` (transiente, NÃO persistido — reconstruído a cada `DayStartedEvent`): `NpcId, CurrentScheduleId, CurrentScheduleBlock, CurrentAnchorId, IsAvailable`.

### 2. Serviço runtime (`NpcScheduleService`)

- Singleton MonoBehaviour (`Instance`; duplicata se autodestrói).
- Registros: `RegisterAnchor(NpcScheduleAnchor)` (dicionário por AnchorId), `RegisterNpcController(NpcController)`, `RegisterShopController(NpcShopController)` (listas sem duplicata).
- `TryResolveAnchor(anchorId, out position)` — resolução nomeada de posição.
- **Resolução diária** (`DayStartedEvent`): para cada controller registrado (diálogo e loja), resolve a posição home:
  1. tenta anchor nomeado `npc_{npcId}_home`;
  2. fallback `NpcDataSO.DefaultPosition`;
  3. teleporta o GameObject (`transform.position = target`) e grava `NpcScheduleRuntimeState` com `CurrentScheduleId = "schedule_{npcId}_default"`, bloco `Default`, `IsAvailable = true`.
- `TryGetRuntimeState(npcId, out state)` — leitura para validadores/UI.
- **TIME_BLOCK_DEBT** (declarado no código): avanço apenas em `DayStartedEvent`; transições Morning/Midday/Evening/Night aguardam um `TimeBlockChangedEvent` (fable_19).

### 3. Anchors em cena (`NpcScheduleAnchor`)

- MonoBehaviour com `_anchorId`/`_npcId` serializados; `GetPosition() = transform.position`. Colocado na TownScene por wiring humano/gerador (SCENE_WIRING_DEBT registrado na wave).

### 4. Bootstrap (`NpcScheduleRuntimeBootstrap`)

- `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`: cria o `NpcScheduleService` DontDestroyOnLoad se ausente e registra todos os `NpcScheduleAnchor` da cena (`FindObjectsOfType` — wiring de bootstrap, com pragma; padrão sob decisão pendente da regra unity-architecture).

### 5. Registries de cobertura de diálogo

- `NpcDialogueSetRegistry` (estático, lazy): deriva entradas de `TownNpcDialogueLibrary.AllContent` — `{NpcId, DialogueSetId, NodeCount = NodesPerNpc (13), HasGreeting, HasRole, HasService (= HasShop), HasTownContext, HasGoodbye}`. Superfícies: `AllEntries`, `TryGet(npcId)`, `TotalNpcsWithDialogue`, `NpcsWithAtLeast10Nodes`. Por derivar da biblioteca, registry e conteúdo não podem divergir.
- `NpcDialogueExpansionBootstrap` (`RuntimeInitializeOnLoadMethod`): loga a cobertura no boot (`N NPCs registered, M with >=10 nodes`); se `NpcsWithAtLeast10Nodes < 5` emite warning `WAVE25 dialogue coverage check: FAIL`; loga também o roster (`NpcTownRosterRegistry.CanonicalCount` / `MvpCount`).
- Roster confirmado pela wave: 23 NPCs canônicos (7 MVP: pip, sylveth, brumdar, renko, thalindra, zrix, nimble; 16 extended) + 1 legado (`vaalara_wanderer_01`).
- `NpcScenePlacementMarker` (WAVE12C, consumido): `NpcId, SceneId, PlacementId, MovementProfile, Reachable` — base das matrizes de posicionamento.

## Critérios de aceite (verificáveis no código atual)

1. Em `DayStartedEvent`, todo NPC registrado é teleportado para `npc_{id}_home` (anchor) ou `NpcDataSO.DefaultPosition`, e ganha runtime state `schedule_{id}_default`/Default/IsAvailable.
2. `NpcScheduleRuntimeState` nunca é persistido (reconstrução diária) — matriz SAVE_LOAD da wave confirma que o save de NPC continua sendo o pré-existente (WAVE08/18).
3. Bootstrap cria o serviço uma única vez e registra todos os anchors presentes na cena carregada.
4. `NpcDialogueSetRegistry.TotalNpcsWithDialogue == 23` e `NpcsWithAtLeast10Nodes == 23` (13 nós por NPC); warning de cobertura dispara apenas se < 5.
5. Nenhum sistema NPC pré-existente (NpcManager/NpcController/NpcShopController/NpcWanderer/save) foi recriado — apenas os 9 arquivos novos da wave.

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `NPC/Schedule/NpcScheduleService.cs` | Singleton; registro de anchors/controllers; resolução diária de posição |
| `NPC/Schedule/NpcScheduleProfile.cs` / `NpcScheduleBlock.cs` | Modelo de dados de rotina (pronto para fable_19) |
| `NPC/Schedule/NpcScheduleAnchor.cs` | Marcador de posição nomeada em cena |
| `NPC/Schedule/NpcScheduleRuntimeState.cs` | Estado transiente por NPC |
| `NPC/Schedule/NpcScheduleRuntimeBootstrap.cs` | Auto-criação + registro de anchors |
| `NPC/NpcDialogueSetRegistry.cs` | Cobertura de diálogo derivada da biblioteca |
| `NPC/NpcDialogueExpansionBootstrap.cs` | Log/validação de cobertura no boot |
| `NPC/NpcTownRosterRegistry.cs` (criado na wave) | Roster canônico estático (23 NPCs) |
| `NPC/Runtime/NpcScenePlacementMarker.cs` (WAVE12C) | Placement por NPC (consumido por validação) |

## Contratos

- `NpcScheduleService.Instance`; `RegisterAnchor/RegisterNpcController/RegisterShopController`; `TryResolveAnchor(anchorId, out Vector3)`; `TryGetRuntimeState(npcId, out NpcScheduleRuntimeState)`.
- Convenções de ID: anchor home `npc_{npcId}_home`; schedule default `schedule_{npcId}_default`.
- Evento consumido: `DayStartedEvent` (único gatilho hoje).
- `NpcDialogueSetRegistry.AllEntries / TotalNpcsWithDialogue / NpcsWithAtLeast10Nodes` — superfície para validadores editor (`ValidateWave25TownNpcSchedulesDialogue`).
- `NpcTimeBlock` + `NpcScheduleBlock.CanInteract` — contrato reservado para fable_19/fable_11.

## Decisões e invariantes

- **TIME_BLOCK_DEBT explícito**: granularidade intra-dia diferida; o modelo de dados já suporta (blocos por período) para evitar retrabalho.
- **Estado de schedule não entra no save**: posição diária é função do dia + anchors — re-resolvível, não persistível.
- **Registry deriva do conteúdo**: cobertura de diálogo nunca é declarada à mão (anti-drift).
- **Reuso total do stack NPC** (WAVE08/12/12C/15/18): a wave só adiciona schedule + registries; matrizes da wave documentam o "já existia / não recriar".
- **FindObjectsOfType apenas em bootstrap** (wiring), nunca em comunicação de gameplay.

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Criar o modelo de dados (`NpcTimeBlock`, `NpcScheduleBlock`, `NpcScheduleProfile`, `NpcScheduleRuntimeState`).
2. Criar `NpcScheduleAnchor` e posicionar anchors `npc_{id}_home` na TownScene (gerador ou wiring humano).
3. Criar `NpcScheduleService` com registros, `TryResolveAnchor` e a resolução diária (anchor → fallback DefaultPosition → teleport + runtime state).
4. Criar `NpcScheduleRuntimeBootstrap` (RuntimeInitializeOnLoadMethod, registro de anchors da cena).
5. Criar `NpcTownRosterRegistry` (23 NPCs) e `NpcDialogueSetRegistry` derivado de `TownNpcDialogueLibrary`; criar `NpcDialogueExpansionBootstrap` com os logs/warnings de cobertura.
6. Registrar os controllers no serviço (ponto de integração com NpcManager/cena) e validar com o validador de 42 checks da wave.

## Débitos conhecidos

- **TIME_BLOCK_DEBT**: sem transições Morning/Midday/Evening/Night (depende de `TimeBlockChangedEvent` — fable_19).
- **SCENE_WIRING_DEBT**: anchors e registro de controllers na TownScene exigem regeneração/wiring humano no Editor.
- Teleporte instantâneo (sem pathing/walk) na resolução diária; NPC pode teleportar mesmo durante interação (guard limitado — `IsInteracting` não é público no controller).
- `NpcScheduleProfile`/`Blocks` ainda não são consumidos pelo serviço (modelo à frente da lógica, por design).
- Sem testes EditMode próprios da wave (validação via validador editor + checklist humano).
