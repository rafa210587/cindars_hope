# Retro-Spec 03 — Mercador Errante da Caverna (encontro de loja determinístico)

> **Spec ID:** `spec_retro_03_cave_wandering_merchant`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** Cave runtime / Economia
> **Código que documenta:**
> - `Assets/_Game/Scripts/Cave/Runtime/CaveWanderingMerchant.cs`
> **Evidência de execução:** `docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md` (testes EditMode `CaveWanderingMerchantTests`)
> **Supersedida/complementada por:** — (visual placeholder será substituído por arte futura; fluxo de trade pode ganhar UI modal própria em wave futura)

---

# /speckit.specify

## Contexto

Encontro roguelike de loja dentro da caverna: em alguns andares aparece um mercador errante com duas ofertas de compra e um ponto de venda. O requisito central é o contrato de stable run (ADR-0005/FASE9F): aparição, posição e estoque são funções puras de `CaveWorldSeed + CaveRunSeed + CaveLevel + salt` — revisitar o mesmo nível na mesma run sempre produz o mesmo mercador (ou a mesma ausência dele). O trade reusa o caminho de economia por eventos (`BuyItemPoint`/`SellAllPoint` → `ItemPurchaseRequestedEvent`/`SellAllRequestedEvent` → `EconomyManager`), dispensando UI modal de loja dentro da CaveScene.

## Comportamento implementado

### 1. Aparição (`ShouldAppear` — função pura, testada em EditMode)

```text
hash = CaveEnemySpawnPlanner.StableHash($"{worldSeed}|{runSeed}|{caveLevel}|wandering_merchant")
aparece ⇔ abs(hash) % 100 < 22
```

- `AppearanceChancePercent = 22` (público, const). Salt: `"wandering_merchant"`.
- Sem GUID, timestamp ou `UnityEngine.Random`.

### 2. Ciclo de vida

- Classe estática com bootstrap `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`: re-subscreve (`Unsubscribe`+`Subscribe`) `CaveRuntimeMaterializationCompleteEvent` no GameEventBus.
- A cada nível materializado: `DespawnCurrent()` (destrói o root anterior), lê seeds do `CaveRunManager.Instance`, decide `ShouldAppear`, resolve tile e spawna.

### 3. Posição (`ResolveSpawnTile`)

- Candidatos: `WalkableTiles` com `Manhattan(tile, Entrance) >= 6` (`MinDistanceFromEntrance`), `Manhattan(tile, Exit) >= 3` (`MinDistanceFromExit`) e fora de `WallTiles`.
- Ordenação determinística: `StableHash($"{seed}|{x}|{y}")` (seed = StableHash de `worldSeed|runSeed|level|wandering_merchant_tile`), desempate por x, depois y; primeiro candidato vence.
- Sem tile seguro → warning `[CaveWanderingMerchant] No safe tile...` e mercador pulado (sem fallback inseguro).
- Conversão grid→mundo: `(tile.x - Width*0.5, tile.y - Height*0.5, 0)`.

### 4. Catálogo e seleção de ofertas (`OfferCatalog` + `ResolveOfferIndices`)

Catálogo fixo de 6 ofertas `(itemId, amount, totalCost, prompt)`:

| # | ItemId | Qtde | Custo total |
|---|---|---|---|
| 0 | `item_consumable_potion_hp_small` | 2 | 60 |
| 1 | `item_consumable_food_bread` | 3 | 30 |
| 2 | `item_consumable_repair_kit_basic` | 1 | 45 |
| 3 | `item_consumable_food_carrot_stew` | 2 | 40 |
| 4 | `item_material_wood` | 5 | 25 |
| 5 | `item_seed_carrot` | 4 | 20 |

Seleção de par distinto por nível (função pura, testada):

```text
hash   = abs(StableHash($"{worldSeed}|{runSeed}|{caveLevel}|wandering_merchant_stock"))
first  = hash % 6
second = (first + 1 + (hash / 7) % 5) % 6     // sempre != first
```

### 5. Composição do spawn (`SpawnMerchant`)

- Root `WanderingMerchant_L{nivel}`: SpriteRenderer com sprite sólido 8×8 em cache (`HideAndDontSave`, funciona em build), cor `(0.85, 0.7, 0.3)`, sortingOrder 3, escala `(0.9, 1.3, 1)`.
- Dois pontos de compra filhos em offsets locais `(-1, 0)` e `(+1, 0)`: escala `(0.7, 0.55, 1)`, cor `(0.5, 0.36, 0.2)`, `BoxCollider2D` trigger 1×1, componente `BuyItemPoint.ConfigureOffer(shopId, itemId, amount, totalCost, prompt)`.
- Um ponto de venda filho em `(0, -1.1)`: cor `(0.32, 0.45, 0.3)`, `SellAllPoint.ConfigureSource(shopId, "Vender itens ao mercador errante")`.
- ShopId estável por andar: `shop_cave_wandering_merchant_l{caveLevel}`.
- Feedback ao player via `GameEventBus.Publish(new PlayerActionFeedbackEvent("Um mercador errante montou banca neste andar..."))` + log com nível, tile e ofertas.

## Critérios de aceite (verificáveis no código atual)

1. `ShouldAppear` é determinística: mesma tupla (worldSeed, runSeed, level) → mesmo resultado; taxa nominal 22% por nível.
2. `ResolveOfferIndices` retorna sempre dois índices distintos do catálogo de 6, deterministicamente.
3. Tile de spawn respeita distâncias Manhattan >= 6 da entrada e >= 3 da saída e nunca é parede; sem tile válido o mercador é omitido com warning.
4. Trade usa exclusivamente `BuyItemPoint`/`SellAllPoint` (eventos de economia existentes) — nenhuma chamada direta ao `EconomyManager` e nenhuma UI modal nova.
5. Apenas um mercador vivo por vez (despawn no evento de materialização seguinte).
6. Nenhum `UnityEngine.Random`/GUID/timestamp em decisões persistentes.

---

# /speckit.plan

## Arquitetura real

| Elemento | Responsabilidade |
|---|---|
| `CaveWanderingMerchant` (static) | Decisão de aparição, posição, estoque, montagem/teardown do GameObject |
| `CaveEnemySpawnPlanner.StableHash` (reuso) | Função de hash determinística compartilhada (FNV-1a) |
| `BuyItemPoint` / `SellAllPoint` (reuso) | Interactables de economia por evento |
| `CaveRunManager` (reuso) | Fonte de `CaveWorldSeed` / `CaveRunSeed` |
| `CaveRuntimeMaterializationCompleteEvent` (reuso) | Gatilho de spawn por nível |

## Contratos

- `CaveWanderingMerchant.ShouldAppear(worldSeed, runSeed, caveLevel) : bool` — pura, exercitada por testes EditMode.
- `CaveWanderingMerchant.ResolveOfferIndices(worldSeed, runSeed, caveLevel) : (int, int)` — pura, testada.
- `CaveWanderingMerchant.OfferCatalog : MerchantOffer[]` — público/readonly (struct `MerchantOffer { ItemId, Amount, TotalCost, Prompt }`).
- `AppearanceChancePercent = 22` — const pública (ponto único de tuning).
- Eventos consumidos: `CaveRuntimeMaterializationCompleteEvent`. Eventos publicados: `PlayerActionFeedbackEvent`.

## Decisões e invariantes

- **ADR-0005 (cave stable run)**: toda decisão deriva de `StableHash(worldSeed|runSeed|level|salt)` com salts distintos por preocupação (`wandering_merchant`, `_stock`, `_tile`) — aparição, estoque e posição variam independentemente mas são estáveis por run.
- **Sem sistema paralelo de loja**: reuso integral do caminho evento-driven (`ItemPurchaseRequestedEvent`/`SellAllRequestedEvent` tratados pelo `EconomyManager`).
- **Fail-safe visível**: ausência de tile seguro degrada para "sem mercador" + warning, nunca para posição inválida.
- **Estática + evento**: sem MonoBehaviour persistente; estado limitado ao root atual (`s_merchantRoot`) e sprite em cache.

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Criar classe estática `CaveWanderingMerchant` em `CindarsHope.Cave.Runtime` com bootstrap `RuntimeInitializeOnLoadMethod` subscrevendo `CaveRuntimeMaterializationCompleteEvent`.
2. Implementar `ShouldAppear`/`ResolveOfferIndices` como funções puras com os hashes/salts exatos da seção de comportamento (chance 22/100; par distinto via `(first + 1 + (hash/7) % 5) % 6`).
3. Definir `OfferCatalog` com as 6 ofertas e preços da tabela.
4. Implementar `ResolveSpawnTile` com filtros Manhattan (6/3), exclusão de paredes e ordenação por StableHash.
5. Montar o GameObject composto (root + 2 BuyItemPoint + 1 SellAllPoint) com shopId `shop_cave_wandering_merchant_l{level}` e visual placeholder em cache.
6. Garantir despawn no início de cada materialização e feedback via `PlayerActionFeedbackEvent`.
7. Cobrir `ShouldAppear`/`ResolveOfferIndices` com testes EditMode (estabilidade por seed e distinção de ofertas).

## Débitos conhecidos

- Estoque não persiste compras (recomprável ao revisitar o nível dentro da mesma run) — aceito enquanto não há snapshot de estado de loja por nível.
- Visual é placeholder sólido 8×8; arte e idle animation pendentes.
- Sem diálogo/flavor próprio — possível integração futura com sistema de diálogo (fable_28).
- Preços fixos no código; quando o catálogo de economia central (skill `economy-balance-tuning`) cobrir vendas na caverna, migrar para data assets.
