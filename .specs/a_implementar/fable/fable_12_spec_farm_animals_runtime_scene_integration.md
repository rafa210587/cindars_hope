# SPEC — Fazenda: Animais Runtime + Integração de Cena (Galinheiro/Estábulo, Cuidado, Produtos)

> **Spec ID:** `fable_12_spec_farm_animals_runtime_scene_integration`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco D (mundo)
> **Priority:** P2
> **Type:** Runtime / Data / Save / Integration
> **Domain:** Farm
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_D
> **Can run with:** fable_01, fable_04, fable_09, fable_11
> **Must not run with:** fable_13 (save schema lock), fable_07 (save schema lock)
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/Animals/**`, `CreateMvpFarmScene.cs`, `GameSaveData` (seção nova)
> **Depends on:**
> - WAVE 05 (specs de animals executadas em nível de contrato; AnimalHousingCapacityState)
> - WAVE 02 (DayStartedEvent), Eiran shop (WI-12C — venda de filhotes)
> **Blocks:** N/A
> **Scope:** animais de fazenda funcionais ponta a ponta: aquisição, abrigo, alimentação, produto, save.
> **Out of scope:** pastagem livre com cerca dinâmica, qualidade de produto por afeto avançado, animais na caverna, pets (HOLD).

required_adrs: []
required_game_rules: [farm_rules.md]

---

# /speckit.specify

## Contexto

`FARM_DESIGN_DIRECTION_v1.3.md` define animais (galinha/cabra/vaca) com abrigo, alimentação
diária, vínculo simples e produtos coletáveis (ovo/leite). A WAVE 05 executou as specs de
animals em nível de contrato: no repo só existe `Farm/Animals/AnimalHousingCapacityState.cs`.
Não há FarmAnimal runtime, nem abrigo em cena, nem produto. O NPC Eiran (curral, WI-12C)
vende sem nada para comprar de verdade, e o quintal de animais da cidade tem cerca decorativa.

## Problema

Metade do fantasma "Harvest Moon" do pitch não existe: o loop diário da fazenda termina em
plantar/colher. Sem animais, ração não tem uso, o Eiran não tem função econômica e o
DayStartedEvent não tem consumidor pecuário.

## Objetivo

Ao final desta spec, o jogador deve poder comprar um filhote do Eiran (item), soltá-lo no
galinheiro/estábulo da fazenda (gerados pelo CreateMvpFarmScene), alimentá-lo diariamente
(ração craftável/comprável), coletar produto por interação quando alimentado no dia anterior,
e ter tudo persistido em seção própria de save — reusando AnimalHousingCapacityState.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/save-dto-simple-types-only.md
.claude/rules/testing-quality-gate.md
.claude/skills/scene-interactable-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- Farm/Animals/AnimalHousingCapacityState.cs (capacidade por abrigo — auditar API)
- DayStartedEvent (WAVE 02); InventoryManager; ItemDatabase (gerar itens novos via initializer)
- Shop_Eiran.asset (WI-12C — entradas via gerador de shop)
- CreateMvpFarmScene v2 (zonas; Zone_Construction livre para abrigos)
- SaveManager padrão WI-18; FarmDailyGoalService (padrão de serviço farm com bootstrap)
- NpcWanderer (padrão de movimento em bounds — reutilizável para animal)
Não existe:
- FarmAnimal runtime, AnimalDataSO, abrigos em cena, ração, produtos, save de animais
```

## Engineering stories

```text
Como jogador, quero comprar item_animal_chicken_chick do Eiran e soltá-lo no galinheiro.
Como criador, quero alimentar com ração; animal alimentado ontem produz hoje.
Como coletor, quero interagir com o animal/ninho para receber ovo/leite.
Como save, quero animais persistidos por ID/tipo/abrigo/fed-state sem refs Unity.
```

## Escopo

```text
Inclui:
- AnimalDataSO (NOVO): AnimalId, DisplayName, HousingType {Coop, Barn}, FeedItemId,
  ProductItemId, ProductIntervalDays (1), PurchaseItemId, cor placeholder;
- 3 animais: animal_chicken (coop, ovo), animal_goat (barn, leite de cabra),
  animal_cow (barn, leite) + itens via ItemDataInitializer (filhote/ração/produtos);
- FarmAnimalRuntime (NOVO MonoBehaviour): wander em bounds do abrigo (reuso do padrão
  NpcWanderer), estado FedToday/ProducedToday/DaysOwned; IInteractable: alimentar (consome
  ração do inventário) ou coletar produto (AddItem);
- FarmAnimalRegistry (NOVO service, bootstrap pattern): spawn/track por abrigo, consome
  DayStartedEvent (reset fed → produced se fed ontem), usa AnimalHousingCapacityState;
- AnimalReleaseHandler: usar item filhote perto do abrigo → registra animal (consome item);
- CreateMvpFarmScene: Coop_01 (capacidade 4) e Barn_01 (capacidade 4) na Zone_Construction
  com visual composto (corpo+telhado+porta) e área cercada (bounds de wander);
- save: FarmAnimalsSaveData {List<AnimalRecord {AnimalInstanceId, AnimalId, HousingId,
  FedToday, ProducedToday, DaysOwned}}} — seção nova padrão WI-18, default vazio;
- entradas no Shop_Eiran via gerador (filhotes + ração);
- EditMode tests: ciclo fed→produce, capacidade, save round-trip, IDs estáveis (animal_<housing>_<index>).
```

## Fora de escopo

```text
Não inclui: felicidade/afeto avançado e qualidade de produto; incubadora; animais soltos no
mapa todo; processamento (queijo — recipes futuras); pets (HOLD WAVE 23); arte.
```

## Regras de não duplicação

```text
Reusar AnimalHousingCapacityState — não criar segundo controle de capacidade.
Reusar padrão wander/bootstrap/seção de save existentes.
Itens via ItemDataInitializer — não criar gerador de itens paralelo.
```

## Critérios de aceite

### CA-1 Loop pecuário completo
- Comprar → soltar → alimentar → (dia vira) → coletar produto; sem ração não produz.
- Evidência: testes do ciclo no registry (DayStartedEvent sintético) + cenário humano.

### CA-2 Capacidade e IDs estáveis
- 5º animal no coop de 4 é recusado com feedback; AnimalInstanceId determinístico e único.
- Evidência: testes de capacidade/IDs.

### CA-3 Persistência
- Save/load preserva animais/estados; save antigo carrega com zero animais.
- Evidência: round-trip + load legado (padrão WI-18).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Animals/
  AnimalDataSO.cs / FarmAnimalRuntime.cs / FarmAnimalRegistry.cs /
  AnimalReleaseHandler.cs / FarmAnimalsSaveData.cs            (NOVOS)
Assets/_Game/Scripts/Save/SaveManager.cs                      (Capture/Restore seção)
Assets/_Game/Scripts/Editor/Farm/GenerateFarmAnimalAssets.cs  (NOVO — SOs+itens+shop)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (abrigos)
Assets/_Game/Tests/EditMode/Farm/FarmAnimalsTests.cs
```

## Contratos

### Data contracts — AnimalDataSO (IIdentifiedData) + itens novos.
### Runtime contracts — registry API: TryRelease, Feed, Collect, GetByHousing.
### Event contracts — `AnimalProductCollectedEvent(animalId, itemId)` (NOVO; daily goals futuros).
### Save contracts — seção FarmAnimals; owner FarmAnimalRegistry; restore após Inventory;
default vazio; sem migration; IDs simples.
### UI contracts — prompts via IInteractable existente.

## Sistemas afetados

```text
Farm, Inventory, Economy (shop Eiran), Save (seção nova), Time (DayStarted), Scene generator
```

## Arquivos permitidos

```text
Arquivos da arquitetura + Assets/_Game/Scripts/Core/Events/FarmEvents aditivos
Assets/_Game/Tests/EditMode/Farm/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab/*.asset manual; Packages/ProjectSettings; FarmPlot/colheita (não tocar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar AnimalHousingCapacityState, ItemDataInitializer, padrão de seção.
### Fase 1 — SOs/itens/gerador + registry puro com ciclo diário + testes.
### Fase 2 — Runtime (wander/interação) + release handler.
### Fase 3 — Save section + round-trip/legado.
### Fase 4 — Abrigos no gerador de cena + shop Eiran + validação estrita + report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Must not run with: fable_07/fable_13 (save schema), fable_09 (nada compartilhado — OK)
- Reason: adiciona seção de save.

## Impacto em save/load

```text
Does this change save schema? YES (seção aditiva FarmAnimals, default vazio)
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES (AnimalProductCollectedEvent) | Changes existing: NO | Unsubscribe: YES (registry)
```

## Impacto em UI/Unity

```text
Changes scenes: via gerador | Assets: via gerador | Prefabs: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: animal wander escapar do cercado. Mitigação: bounds estritos + clamp por frame.
Risco: dupla coleta no mesmo dia. Mitigação: ProducedToday flag + teste de idempotência.
```

## Rollback

```text
Remover arquivos/seção (default vazio em load); abrigos saem do gerador; itens permanecem inertes.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar capacidade/itens/seções existentes.
- [ ] T002 — AnimalDataSO + itens + GenerateFarmAnimalAssets.
- [ ] T003 — FarmAnimalRegistry (ciclo diário) + testes.
- [ ] T004 — FarmAnimalRuntime (wander/feed/collect) + ReleaseHandler.
- [ ] T005 — Seção de save + round-trip/legado.
- [ ] T006 — Abrigos no CreateMvpFarmScene + Shop_Eiran.
- [ ] T007 — csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES (ciclo, capacidade, round-trip)
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (farm loop atual intacto)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano comprar→alimentar→coletar→reload

## Definition of Done

```text
Loop pecuário completo e persistido; abrigos gerados; Eiran funcional; builds 0E; report.
```

## Anti-regressão

```text
FarmPlot/colheita/shipping intactos. Saves antigos carregam. Sem refs Unity em save.
```

## Notas para execução posterior

```text
Qualidade por afeto + processamento (queijo): specs futuras de farm.
Pets continuam HOLD (WAVE 23) — animal de fazenda não é pet.
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. PRODUTOS com variantes de qualidade (decisão Q3.2): egg/goat_milk/cow_milk/wool +
   _silver/_gold como itens separados; qualidade por dias de FedToday consecutivos
   (>=3 dias = silver 30%, >=7 = gold 15%) — números do ITEM_CATALOG §18.
2. IDs/BVs dos itens: ITEM_CATALOG vence.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. MORTE PERMANENTE (5.1-A): animal negligenciado por N dias consecutivos (propor N=7 na
   Fase 0) MORRE permanentemente — com avisos progressivos (doente → crítico → morte).
2. Gato dá bônus lunar (5.5): SIM — anotar hook p/ quando pets/gato existirem (HOLD).
3. Companion pode gastar fertilizante raro automaticamente (5.5): SIM — anotar p/ wave companions.
```
