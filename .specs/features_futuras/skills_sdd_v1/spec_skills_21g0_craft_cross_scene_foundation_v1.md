# Skills 21G0 — fundação Craft persistente entre Farm e Town

> **Spec ID:** `spec_skills_21g0_craft_cross_scene_foundation_v1`
> **Status:** DRAFT — blueprint técnico para revisão; implementação NÃO autorizada; fora de `.specs/a_implementar/`
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+UI+Save integration / Craft / P1
> **Parallelizable / repo lock:** NÃO; geradores/cenas, bootstrap e SaveManager em sequência; preserve edits concorrentes
> **Depende de:** 21F T1–T6, runtime/save v6 e testes focais registrados no [relatório](../../../docs/validation/skills_sdd_v1/execution/PHASE_21F_PURCHASED_ORIGIN_EXECUTION_REPORT.md)
> **Bloqueia:** `spec_skills_21g_recipe_activation_v1` e teste físico das cadeias Cooking/Sewing da 21F
> **Ordem de execucao:** composição persistente → UI local → save provider → geração de cenas → PlayMode bidirecional → revisão/closeout
> **Scope:** Craft runtime/modal/pontos existentes, FarmScene e TownScene, save provider existente e seus testes
> **Out of scope:** registro das três receitas dormentes, preços, drops, itens, armadura, novos managers, schema de save e reinserção de `recipe_processed_wood`
> **Validation level alvo:** Unity compile + EditMode de composição + PlayMode real Farm/Town e save/load; nada aqui foi executado por este draft

Este slice é independente das decisões de balanceamento [R21G](../../../docs/refinements/a_implementar/ref_skills_21g_recipe_utility_balance_v1.md). Não altera o conteúdo disponível ao jogador por si.

# /speckit.specify

## Spec

**Estado atual (inspeção estática, 2026-09-13).** `CreateMvpFarmScene.CreateBootstrap()` adiciona `CraftingRuntime`, configura-o com `InventoryManager` e passa-o a `SaveManager`; `CreateCraftingUi()` cria `CraftingModal`. A Town cria `CraftingManager` e `CraftingPoint` em modo evento, mas nenhum `CraftingRuntime`/`CraftingModal`. `CraftingPoint.Interact()` publica `OpenCraftingStationRequestedEvent`; somente `CraftingModal` o consome com runtime. `GameBootstrap.Awake()` conserva o primeiro `_Bootstrap` via `DontDestroyOnLoad` e destrói o bootstrap de cena seguinte. `SaveManager.Initialize()` cria `CraftingSectionProvider` capturando seu `_craftingRuntime` atual. As refs do runtime/modal/provider precisam sobreviver a ambas as ordens de entrada.

- **AC01 — uma autoridade Craft:** início direto em Farm ou Town e sequências Farm→Town→Farm e Town→Farm→Town terminam com exatamente um `CraftingRuntime` ativo e inicializado no `_Bootstrap` sobrevivente. Jobs/capacidade/estações não são reinicializados ao trocar de cena. Um `CraftingManager` legado não executa receitas de estação por engano.
- **AC02 — ponto e UI por cena:** `CraftingPoint.Interact(player)` de `farm_cooking_01` e `station_house_residential_3` abre o modal da cena ativa associado ao runtime persistente e ao `ModalManager` correto; fecha/abre sem assinatura duplicada. Receitas inexistentes ou dormentes não surgem como fabricáveis. A estação errada mostra falha, sem consumo. O teste da costura pode usar fixture temporária sem editar `RecipeDatabase` de produção.
- **AC03 — save sem perda:** `SaveManager`/`CraftingSectionProvider` capturam/restauram o runtime sobrevivente em ambos os inícios e ambas as transições; save/load v6 conserva jobs, IDs de estação, quantidades e origem dos lotes existentes. Schema/IDs DTO não mudam. Bootstrap da segunda cena destruído não pode fornecer o provider ativo.
- **AC04 — geração determinística:** geradores Farm/Town materializam o mesmo contrato após duas execuções, sem duplicar runtime/modal/pontos e sem tocar receitas dormentes. Validator de cena verifica Town runtime/modal e os dois pontos físicos; não apresenta PASS para cena ausente.
- **AC05 — validadores coerentes:** `CindarsHopeDataValidator` e `ValidateCraftingSystem` concordam que `recipe_processed_wood` legado permanece ausente. A reconciliação não habilita as três receitas dormentes nem altera IDs de itens. Ambos passam na geração idempotente do catálogo atual.

## Limites e falhas

Bootstrap inicial Farm, inicial Town, transição em ambas direções, save antes/depois, modal destruído ao descarregar cena, evento recebido duas vezes, runtime de segunda cena destruído, componente ausente e inventário vazio são casos obrigatórios. Em falha de referência, apresentar erro diagnóstico e bloquear craft, preservando inventário/job; nunca criar runtime paralelo silencioso. Nenhuma mudança em `CraftingSectionProvider` pode descartar estado já carregado.

# /speckit.plan

## Plan — contrato proposto, sujeito à revisão de código antes da fila

1. **Reuso e lifetime (AC01):** `GameBootstrap.Instance` (`Core/Bootstrap/GameBootstrap.cs`) é a autoridade persistente. Reusar `CraftingRuntime.ActiveInstances`, `Initialize()`, `RebindInventoryManager(InventoryManager)`, `RebindStaminaManager(MonoBehaviour)` e `GetOrCreateStation(string, WorkshopType)` (`Craft/CraftingRuntime.cs`); nenhum novo serviço. `GameBootstrap.Awake()` chama `InitializeManagers()` no primeiro bootstrap, antes dos `Start()` dos installers. Ao iniciar Town, `CreateMvpTownScene.ConfigureBootstrap()` deve serializar runtime e sua ref no SaveManager **antes** do PlayMode, pois `SaveManager.Initialize()` constrói `CraftingSectionProvider` com a ref atual. Nas transições, os installers recuperam o runtime via `GameBootstrap.Instance.GetComponent<CraftingRuntime>()` e reconfiguram só o modal da cena; não criam substituto. Não adicionar dependência Core→Craft.
2. **UI e pontos (AC02):** reutilizar `CraftingModal.Configure(CraftingRuntime, ModalManager)` e seu subscribe em `OnEnable`/unsubscribe em `OnDisable` (`UI/Crafting/CraftingModal.cs`). `CraftingPoint.Configure(string, WorkshopType)` mantém modo evento, evitando refs à cena anterior. `CreateMvpFarmScene.CreateCraftingStation` hoje usa overload direto do ponto; migrar ao mesmo contrato de evento com cuidado para não alterar posição/arte. `CreateMvpTownScene` cria modal local próximo ao canvas/ModalManager existente. `FarmSceneRuntimeReferenceInstaller.RebindAllReferences()` e `TownSceneRuntimeReferenceInstaller.Start()` ligam modal local ao runtime sobrevivente após scene load. Não duplicar assinaturas.
3. **Save (AC03):** `SaveManager.Initialize()` cria `CraftingSectionProvider(_craftingRuntime)` e registra o provider; captura/restauração também usam `_craftingProvider` diretamente. Como o primeiro bootstrap e runtime sobrevivem às trocas de cena, **não é esperado alterar SaveManager/provider**: ligar a ref correta na geração de Farm e Town antes de `Awake()` e provar que é o mesmo instance ID após transições. `RebindRuntimeManagers`/`RebindOptionalRuntimeManagers` não rebinda Craft. Somente se teste mostrar provider obsoleto em cena legada, retornar à spec para definir API idempotente que atualize tanto `_craftingProvider` quanto seu descritor em `SaveProviderRegistry` sem apagar jobs; não implementar um rebind parcial. Não serializar Unity refs.
4. **Geradores/validador (AC04):** editar `CreateMvpTownScene.CreateBootstrap()`/`ConfigureBootstrap()`/`CreateCraftingStation()` e `CreateMvpFarmScene.CreateBootstrap()`/`ConfigureBootstrap()`/`CreateCraftingUi()`/`CreateCraftingStation()` apenas no wiring Craft. Reusar o pipeline Editor existente para gerar cenas/asset, não editar YAML manualmente. Estender `MvpSceneValidator` para inspecionar Town Craft e autoridade única. Segunda geração não cria refs ou objetos extras. Os paths de cenas/geradores entram sob lock serial.
5. **Catálogo sem ativação (AC05):** em `CindarsHopeDataValidator.cs`, remover a exigência contraditória de `recipe_processed_wood` legado; preservar `ValidateCraftingSystem.cs`, que exige sua ausência. Adicionar teste de regressão que roda ambos sobre o registry existente e prova que as três receitas 21G seguem dormentes. Não editar RecipeDatabase nem o asset legado.

**Ownership:** mudanças previstas em `Assets/_Game/Scripts/SceneManagement/{Farm,Town}SceneRuntimeReferenceInstaller.cs`, `Assets/_Game/Scripts/Editor/SceneCreation/{CreateMvpFarmScene,CreateMvpTownScene}.cs`, `Assets/_Game/Scripts/Editor/Validation/{MvpSceneValidator,ValidateCraftingSystem}.cs`, `Assets/_Game/Scripts/Editor/DataValidation/CindarsHopeDataValidator.cs` e cenas geradas, além de testes novos. `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`, `Assets/_Game/Scripts/Craft/CraftingRuntime.cs`, `Assets/_Game/Scripts/Save/{SaveManager,SaveProviderRegistry}.cs` e `Providers/CraftingSectionProvider.cs` são **somente leitura**, a menos que um teste focal prove necessidade e a spec seja emendada. Reusar `runtime-bootstrap-pattern`, `bootstrap-wiring`, `ui-modal-stack`, `save-load-pattern` e `unity-validation`.

## Estratégia de teste

Nomear `CraftRuntimeDirectFarmStartBindsModalAndSave`, `CraftRuntimeDirectTownStartBindsModalAndSave`, `CraftRuntimeFarmTownFarmPreservesJobAndSingleAuthority`, `CraftRuntimeTownFarmTownPreservesJobAndSingleAuthority` e `CraftPointInteractUsesActiveSceneModalOnly` em suite PlayMode nova; cada um verifica instance ID de runtime, contador de modals inscritos e estado de job antes/depois. EditMode valida geração/serialização de referência de scene e ausência de mudanças em RecipeDatabase. PlayMode provoca estação errada, save/load v6 e fechamento/reabertura de modal. Evidência: XML de Test Runner `failed=0`, log Unity sem Error inesperado e inspeção visual do modal nas duas cenas. O número de testes é alvo, não resultado executado.

# /speckit.tasks

## Tasks — todas pendentes

| ID | Dependência | Owner/ação | Critério e evidência |
|---|---|---|---|
| T0 [ ] | 21F T1–T6 | Confirmar no código a ordem `GameBootstrap.Awake`→`SaveManager.Initialize`→installers `Start` e que o provider retém o runtime persistente; preservar leitura dos arquivos Core/Save | AC01–AC05; contrato sem nova API de rebind salvo falha focal demonstrada |
| T1 [ ] | T0 | Implementar runtime único no bootstrap inicial Town/Farm e rebind nos dois installers | AC01; testes de inícios e transições com um instance ID |
| T2 [ ] | T1 | Gerar modal Town local, migrar ponto Farm para evento e ligar modal por cena | AC02; `Interact` físico abre somente modal da cena ativa |
| T3 [ ] | T1–T2 | Testar provider Craft do SaveManager ligado ao runtime sobrevivente; regressão de job/save/load, sem editar SaveManager se passar | AC03; snapshot v6 idêntico antes/depois, sem Unity refs |
| T4 [ ] | T3 | Reconciliar `CindarsHopeDataValidator` com `ValidateCraftingSystem`, testar ausência do legado e três receitas dormentes | AC05; ambos os validadores passam sem adicionar GUID ao registry |
| T5 [ ] | T4 | Gerar/validar cenas duas vezes e executar EditMode/PlayMode nomeados | AC01–AC05; segunda geração 0 duplicatas, XML `failed=0`, log sem Error inesperado |
| T6 [ ] | T5 | Revisão independente, inspecionar UI real, relatório e `/finish-spec` somente se todos os critérios passarem | AC01–AC05; PASS/FAIL/NOT RUN explícito por gate |

**Readiness:** análise estática indica uma solução sem alterar Core/Save, mas a ordem de inicialização e referência do provider precisam de prova PlayMode T0/T3. A spec continua DRAFT fora da fila; nenhuma alteração de runtime/asset foi feita por este draft, e nenhum teste Unity desta spec foi executado.
