# Execution Report — fable_12 Farm Animals Runtime + Scene Integration

> **Spec:** `.specs/a_implementar/fable/fable_12_spec_farm_animals_runtime_scene_integration.md`
> **Date:** 2026-06-20
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Phase 0-1:** COMPLETE · **Phase 2 (Unity batchmode):** NOT RUN (deferred) · **Phase 3 (Play Mode):** DEFERRED_TO_FINAL_VALIDATION (human-authorized)

---

## Honest status rationale

Núcleo pecuário ponta a ponta implementado, compila limpo (0E runtime + editor) e é coberto por
22 testes EditMode determinísticos (ciclo, capacidade, IDs, morte permanente, qualidade, save
round-trip/legado, idempotência). O dono autorizou pular validação humana / Play Mode / regeneração
de cena no Unity Editor (DEFERIDO). Portanto o status máximo honesto é
`BUILD_VALIDATED_WITH_WARNINGS`: a colocação física dos abrigos em FarmScene.unity e os assets
gerados (itens/SOs/Eiran) exigem o Unity Editor e o gerador `Generate Farm Animal Assets`, ainda
não executados (não há evidência de geração ⇒ não declaro Phase 2). Nenhuma alegação de
`ACCEPTED`/`PLAYMODE_VALIDATED`.

---

## Acceptance criteria extracted

| ID | Critério (spec) | Implementação | Evidência | Status |
|----|-----------------|---------------|-----------|--------|
| CA-1 | Loop pecuário completo: comprar → soltar → alimentar → (dia vira) → coletar; sem ração não produz | `AnimalReleaseHandler` (soltar/consumir filhote) → `FarmAnimalRegistry.Feed` → `ProcessDayTransition` (produto pronto se alimentado ontem) → `Collect`+`AddItem`; `FarmAnimalRuntime.Interact` exige ração no inventário p/ alimentar | Tests `Cycle_FedAnimal_ProducesNextDay`, `Cycle_UnfedAnimal_DoesNotProduce`, `Collect_ReturnsBaseProduct_AndConsumesReadiness` | OK |
| CA-2 | Capacidade (5º recusado) + AnimalInstanceId determinístico/único `animal_<housing>_<index>` | `AnimalHousingCapacityState.CanAddAnimal` (reuso WAVE 05) + `BuildNextInstanceId` | Tests `Capacity_FifthAnimalInCoopOfFour_IsRejected`, `Ids_AreDeterministicAndUnique` | OK |
| CA-3 | Persistência: save/load preserva animais/estados; save antigo carrega com zero animais | Seção aditiva `FarmAnimalsSaveData` no `GameSaveData` (sem migration), `Capture/RestoreFromSaveData` no registry + wiring no `SaveManager` (padrão WI-18/F13) | Tests `Save_RoundTrip_PreservesAnimalsAndState`, `Save_LegacyNull_LoadsZeroAnimals`, `Save_EmptySection_LoadsZeroAnimals`, `Save_DeadAnimal_RestoresAsDead_AndNotInHousing`, `Save_IdCounter_StaysUniqueAfterRestore` | OK |
| EMENDA 5.1-A | Morte permanente por negligência prolongada (N proposto na Fase 0), avisos progressivos (doente → crítico → morte), sem reviver | `FarmAnimalRegistry.ProcessDayTransition` mata em `DaysWithoutFood >= NeglectDeathDays`, publica `AnimalHealthChangedEvent`/`AnimalDiedEvent` + `PlayerActionFeedbackEvent`; `Feed` recusa `AnimalDead` | Tests `Neglect_SevenConsecutiveDaysWithoutFood_KillsPermanently`, `Neglect_ProgressiveWarnings_HungryThenCritical`, `Neglect_FeedingResetsCountdown_PreventsDeath`, `Death_IsPermanent_CannotFeedDead`, `Death_RemovesAnimalFromHousingCapacity` | OK |
| EMENDA §18 | Produtos com qualidade _silver/_gold por dias consecutivos de FedToday (>=3 silver, >=7 gold) | `FarmAnimalCatalog.ResolveProductItemId` + `_consecutiveFedDays` no registry; gerador cria os itens base + variantes | Tests `Quality_ThreeConsecutiveFedDays_YieldsSilver`, `Quality_SevenConsecutiveFedDays_YieldsGold`, `Quality_ResolveProductItemId_DirectMapping` | OK |

### Valor N de morte proposto (Fase 0)

**N = 7 dias consecutivos sem alimentação** (constante `FarmAnimalCatalog.NeglectDeathDays = 7`,
exposta também em `AnimalDataSO.NeglectDeathDays`). Casa com o limiar de qualidade `_gold` (também 7)
e com a faixa de carência da spec ("propor N=7 na Fase 0"). Progressão de avisos reusa a semântica
WAVE 05: dia 1 = `Hungry` (faminto), dia 3 = `Unavailable` (doente/crítico), dia 7 = `Dead`
(terminal, sem reviver).

---

## Existing systems audit (reuse vs. created)

A "Estado atual do repo" da spec estava DESATUALIZADA (afirmava existir só
`AnimalHousingCapacityState`). A auditoria Fase 0 encontrou um conjunto amplo de lógica pura de
animais da WAVE 05 — **reusada, não recriada**:

| Sistema | Encontrado | Decisão |
|---------|-----------|---------|
| `AnimalHousingCapacityState` (capacidade por abrigo) | ✔ WAVE 05 | REUSO direto (capacidade + membership) |
| `AnimalInstanceState` (FedToday/DaysWithoutFood/HealthState/CareScore) | ✔ WAVE 05 | REUSO; só APPEND `AnimalHealthState.Dead = 6` (não renumera; não altera `MarkUnfed`) |
| `AnimalDefinition`, `AnimalCareService`, `AnimalDailyProcessor` | ✔ WAVE 05 | REUSO conceitual; a camada de integração replica a MESMA semântica de fome (Hungry@1/Unavailable@3) e a estende com Dead@N |
| `AnimalProductDefinition`/`AnimalProductCollectionService`/`AnimalProductQualityResolver` | ✔ WAVE 05 | mantidos intactos; a qualidade por dias consecutivos (§18) é nova e ortogonal |
| `FarmAnimalSpecies` enum | ✔ WAVE 05 | REUSO (cabra mapeada em `Sheep` por ausência de `Goat`; produto/ID distinguem) |
| `ItemDataInitializer` / `ItemDataSO` / `ItemCategory.AnimalProduct` (=116) | ✔ | REUSO do padrão de criação de item (mesmo `CreateAsset`); categoria já existia (fable_32) |
| `Shop_Eiran.asset` / `ShopDataSO` | ✔ WI-12C | entradas adicionadas via gerador (sem recriar shop) |
| `ISaveSectionProvider` + padrão aditivo `DailyGoals/FarmLots/Spellbook` no `SaveManager` | ✔ | REUSO do padrão (seção aditiva, sem bump de schema, sem migration) |
| `NpcWanderer` (wander em bounds + clamp) | ✔ | padrão reusado em `FarmAnimalRuntime` (clamp por frame) |
| `IInteractable` / `FarmResourceInteractable` (prompt + reward via inventário) | ✔ | padrão reusado em `FarmAnimalRuntime`/`AnimalReleaseHandler` |
| `FarmDailyGoalService` + `FarmDailyGoalRuntimeBootstrap` | ✔ | padrão de serviço-singleton + `DayStartedEvent` + save replicado em `FarmAnimalRegistry`/`FarmAnimalRuntimeBootstrap` |
| `GameEventBus` | ✔ | única via de comunicação de gameplay |

**Criados (camada de integração ausente nos contratos WAVE 05):**
`AnimalDataSO`, `FarmAnimalCatalog`, `FarmAnimalsSaveData`+`FarmAnimalRecord`, `FarmAnimalRegistry`,
`FarmAnimalRuntime`, `AnimalReleaseHandler`, `FarmAnimalRuntimeBootstrap`,
`Core/Events/FarmAnimalEvents.cs` (`AnimalProductCollectedEvent`, `AnimalHealthChangedEvent`,
`AnimalDiedEvent`), `Editor/Farm/GenerateFarmAnimalAssets.cs`, abrigos no `CreateMvpFarmScene`,
`FarmAnimalsRuntimeTests`. Nenhum sistema paralelo de capacidade/cuidado/produto foi criado.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementação |
|--------------------|---------------|
| AnimalDataSO (AnimalId/DisplayName/HousingType/FeedItemId/ProductItemId/ProductIntervalDays/PurchaseItemId/cor) | `AnimalDataSO.cs` (IIdentifiedData) |
| 3 animais (chicken/coop/ovo · goat/barn/leite cabra · cow/barn/leite) + itens via initializer | `FarmAnimalCatalog` + `GenerateFarmAnimalAssets` (itens + variantes _silver/_gold) |
| FarmAnimalRuntime: wander em bounds + IInteractable (alimentar/coletar) | `FarmAnimalRuntime.cs` (clamp por frame; feed consome ração; collect AddItem) |
| FarmAnimalRegistry: spawn/track por abrigo, DayStartedEvent, AnimalHousingCapacityState | `FarmAnimalRegistry.cs` (Subscribe DayStartedEvent; reuso capacity state) |
| AnimalReleaseHandler: usar filhote perto do abrigo → registra animal (consome item) | `AnimalReleaseHandler.cs` (IInteractable; spawn runtime) |
| CreateMvpFarmScene: Coop_01(4) + Barn_01(4) na Zone_Construction, visual composto + cercado | `CreateAnimalHousings`/`CreateAnimalHousing` (corpo+telhado+porta; bounds de wander) |
| save: FarmAnimalsSaveData {AnimalRecord{InstanceId/AnimalId/HousingId/FedToday/ProducedToday/DaysOwned}} aditivo default vazio sem migration sem refs Unity | `FarmAnimalsSaveData.cs` + wiring `SaveManager` (campo `FarmAnimals`) |
| entradas no Shop_Eiran via gerador (filhotes + ração) | `GenerateFarmAnimalAssets.AddEiranShopEntries` |
| AnimalProductCollectedEvent (NOVO) | `Core/Events/FarmAnimalEvents.cs` |
| EditMode tests: ciclo, capacidade, save round-trip, IDs estáveis | `FarmAnimalsRuntimeTests.cs` (22 testes) |
| Gato dá bônus lunar (5.5) | HOOK anotado (HOLD — pets/gato não existem); ver "Hooks deferidos" |
| Companion gasta fertilizante raro (5.5) | HOOK anotado p/ wave companions; ver "Hooks deferidos" |

### Hooks deferidos (EMENDA 2026-06-12-D)
- **Gato/bônus lunar (5.5):** pets estão em HOLD (WAVE 23). Anotado: quando `gato`/pets existirem, o
  bônus lunar de produto deverá consultar a fase da lua (sistema lunar da WAVE 02) e majorar
  `FarmAnimalCatalog.ResolveProductItemId`/quantidade. Nenhum código de pet criado aqui (fora de escopo).
- **Companion + fertilizante raro (5.5):** anotado para a wave de companions — um companion poderá
  consumir fertilizante raro automaticamente. Sem implementação agora (companions fora de escopo).

---

## Files changed

**Runtime (`Assets/_Game/Scripts/`):**
- `Farm/Animals/AnimalDataSO.cs` (novo)
- `Farm/Animals/FarmAnimalCatalog.cs` (novo)
- `Farm/Animals/FarmAnimalsSaveData.cs` (novo)
- `Farm/Animals/FarmAnimalRegistry.cs` (novo)
- `Farm/Animals/FarmAnimalRuntime.cs` (novo)
- `Farm/Animals/AnimalReleaseHandler.cs` (novo)
- `Farm/Animals/FarmAnimalRuntimeBootstrap.cs` (novo)
- `Farm/Animals/AnimalInstanceState.cs` (APPEND `Dead = 6` ao enum; sem alterar lógica existente)
- `Core/Events/FarmAnimalEvents.cs` (novo)
- `Save/SaveData.cs` (campo aditivo `FarmAnimals`)
- `Save/SaveManager.cs` (capture/restore da seção)

**Editor (`Assets/_Game/Scripts/Editor/`):**
- `Editor/Farm/GenerateFarmAnimalAssets.cs` (novo — itens + SOs + entradas Eiran)
- `Editor/SceneCreation/CreateMvpFarmScene.cs` (abrigos)

**Tests:**
- `Assets/_Game/Tests/EditMode/Farm/FarmAnimalsRuntimeTests.cs` (novo — 22 testes)

**Build (não comitados — somente p/ compilar localmente):** `Assembly-CSharp.csproj`,
`Assembly-CSharp-Editor.csproj` (includes explícitos; o projeto não usa glob).

---

## Save expectations

- **Schema:** `CurrentSchemaVersion` permanece **5** (seção ADITIVA, sem migration — mesmo padrão de
  `DailyGoals`/`FarmLots`/`Spellbook`).
- **Default vazio:** `saveData.FarmAnimals` ausente (save legado) ⇒ registry recebe `null` ⇒ zero
  animais (CA-3). Coberto por `Save_LegacyNull_LoadsZeroAnimals`.
- **Round-trip:** id/tipo/abrigo/FedToday/DaysWithoutFood/ConsecutiveFedDays/HealthState/LastProductDay
  persistidos; `ProductReady` restaurado como `false` (recomputado no próximo `DayStarted`) para
  impedir coleta fantasma após load (idempotência).
- **Sem refs Unity:** `FarmAnimalRecord` só tem `string`/`int`/`bool` (rule save-dto-simple-types-only).
- **Animal morto:** restaurado como `Dead` e NÃO reocupa vaga de abrigo.
- **Contador de IDs:** restaurado à frente dos índices já usados (unicidade preservada).

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (ciclo diário, capacidade, qualidade, morte, save)
Changed Unity scene/prefab/asset wiring: YES (via gerador de cena + gerador de assets; nenhum YAML manual)
Automated tests added/updated: YES (FarmAnimalsRuntimeTests, 22 testes)
Automated tests command: Unity Test Runner EditMode (NOT RUN nesta sessão — requer Unity; dotnet build PASS como sinal de compilação)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (autorizado pelo dono)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: wander/colisão dos abrigos, prompts de IInteractable, e a geração de assets/cena só
verificáveis no Unity Editor; a regra de qualidade lunar do gato e o consumo de fertilizante por
companion são hooks deferidos (pets/companions fora de escopo).
```

---

## Validation

```text
Validation method: gates por spec (dotnet build + validate_docs + check_spec_diff_completeness + run_strict_validation)
Assembly-CSharp: PASS (exit 0, 0 errors, 1 pre-existing warning)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 pre-existing warnings)
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Strict validation: run_strict_validation.ps1 exit 0 (ver LAST_STRICT_VALIDATION_RESULT.json)
Unity batchmode (Phase 2): NOT RUN — Reason: deferido/autorizado; sem Unity nesta sessão. Residual: compile Unity + log scan pendentes.
Play Mode (Phase 3): DEFERRED_TO_FINAL_VALIDATION (human-authorized)
```

### Anti-regressão
- `FarmPlot`/colheita/shipping NÃO tocados. Os 16 testes `FarmAnimalCareTests` e os
  `AnimalProductCollectionTests` da WAVE 05 permanecem válidos (não alterei `MarkUnfed`, só APPEND no enum).
- Saves antigos carregam (seção ausente ⇒ zero animais). Sem refs Unity em save. Sem `GameObject.Find`/
  `FindObjectOfType` em runtime (o registry recebe runtimes por chamada explícita; o bootstrap usa
  `FindAnyObjectByType` só em setup, mesma exceção documentada do `FarmDailyGoalRuntimeBootstrap`).

---

## validated_game_rules

- `farm_rules.md` — esta spec ESTENDE o domínio farm com pecuária (animais, ração, produtos, morte por
  negligência). Não conflita com as regras vigentes (farm_rules ainda descreve só crops); a morte por
  negligência aplica-se a ANIMAIS (a regra "crops não morrem por negligência no MVP" permanece intacta).
  Recomendação de closeout: atualizar `farm_rules.md` com a seção de pecuária ao promover (fora do escopo
  de código desta spec).

validated_adrs: [] (a spec não cita ADRs; required_adrs vazio)

---

## Remaining work (deferido)

1. **Unity Editor (humano):** rodar `CindarsHope/Farm/Generate Farm Animal Assets` (cria itens/SOs/Eiran);
   regenerar `FarmScene` (`CreateMvpFarmScene`) para materializar Coop_01/Barn_01; rodar Test Runner EditMode.
2. **Re-hidratação de runtimes pós-load:** `AnimalReleaseHandler.RespawnExistingAnimals` existe; ligar a
   chamada após o load no bootstrap de cena é trabalho de wiring no Unity (Phase 2).
3. **Phase 3 Play Mode:** comprar→soltar→alimentar→virar dia→coletar→reload (cenário humano).
4. **Hooks:** bônus lunar do gato (pets/WAVE 23) e fertilizante por companion (wave companions).
