# Execution Report — Cadeias couro/tecido/lã (slice village_economy 2)

> **Direção:** `docs/design/gameplay/city/CITY_SELF_SUFFICIENCY_DIRECTION_v1.0.md` §2
> **Date:** 2026-06-23 · **Status:** BUILD_VALIDATED (materialização de assets + Play Mode DEFERRED)
> **validated_adrs:** [] · **validated_game_rules:** []

## 1. O que entrou (aditivo, sem mudar receitas existentes)

| Item/Cadeia | Implementação |
|---|---|
| Itens novos | `item_material_hide` (Drop), `item_material_cloth` (Craft) em `CanonicalItemCatalog.AddMaterials` |
| Receitas (Sewing) | `recipe_leather_from_hide`, `recipe_cloth_from_fiber`, `recipe_cloth_from_wool`, `recipe_leather_armor`, `recipe_arcane_robe` — `RecipeRows()` com `.Station = WorkshopType.Sewing` |
| Pipeline de estação | `CatalogRecipeRow.Station` (novo, default None) + `GenerateCanonicalItemCatalog.ApplyRecipe/ApplyRecipeIfChanged` agora setam `RequiredStationType` (idempotente; receitas antigas seguem None) |
| Lã / ovelha | `FarmAnimalCatalog`: `AnimalSheep` (`animal_sheep`) → `item_animal_wool` (já existia); filhote `item_animal_sheep_lamb` criado pelo `GenerateFarmAnimalAssets` |

## 2. Cadeias fechadas

```
Ovelha (NOVA) → lã → [Tear/Sewing] → tecido (item_material_cloth) → [Sewing] → Manto de Pano (item_armor_robe_arcane)
Fibra (existe) → [Sewing] → tecido → ...
Hide → [Curtir/Sewing] → couro (item_material_leather) → [Sewing] → Armadura de Couro (item_armor_light_leather)
```

A estação **Sewing** (tear da Mirela, slice 1) agora tem 5 receitas → deixa de abrir vazia.

## 3. Validation

```text
Assembly-CSharp: PASS (exit 0)
Assembly-CSharp-Editor: PASS (exit 0)
Materialização (.asset de itens/receitas/ovelha): ocorre no menu CindarsHope/Inicializar Projeto
  (GenerateCanonicalItemCatalog.Run + GenerateFarmAnimalAssets.Generate) — DEFERRED ao Unity.
ValidateCatalogConsistency: roda no Inicializar/Validar Projeto; ingredientes/saídas das receitas
  resolvem (hide/cloth/leather/fiber/wool/armor_light_leather/robe_arcane todos existem). DEFERRED.
```

## 4. Honest status rationale

**BUILD_VALIDATED.** Edições são C# puro de catálogo (compilam exit 0). Os `.asset` (itens, receitas, ovelha)
materializam quando o humano roda `Inicializar Projeto` — só lá o `ValidateCatalogConsistency` confirma os ids.

## 5. Decisão de design registrada (sua confirmação no futuro)

- **Receitas existentes NÃO foram re-tagueadas por estação** (cozinha/poções seguem `None`=craft de bolso).
  Consequência: as estações **CookingStation/Alchemy/Carpentry** (slice 1) abrem **com poucas/zero** receitas
  por enquanto; só **Forge** (do HighTierGearRecipeGenerator) e **Sewing** (esta slice) têm receitas próprias.
  Mover cozinha→fogão, alquimia→alambique, etc. é uma decisão de balance (gating de craft por estação) que
  deixei para você bater o martelo — não retaguei unilateralmente para não tirar o craft de bolso de comida.

## 6. Remaining (hide sourcing)

- `item_material_hide` é fonte Drop; **não está em loot table ainda** (a cadeia de couro só fica utilizável
  quando hide cair de feras ou for vendido pelo curtidor Hess — slice 3). Item + receita já existem.
