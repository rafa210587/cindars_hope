# REF — PR116 item ID audit

> Origem histórica: `docs_old/audits/PR116_ITEM_ID_AUDIT.md`
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# PR-116 - Auditoria de IDs de itens MVP

Data: 2026-05-20  
Branch: `feature/pr-101-reconciliar-branches-pr099`

## IDs verificados

| ID | Estado no repo | Evidencia |
|---|---|---|
| `item_wood` | Existe | `Assets/_Game/Data/Items/Item_Wood.asset` |
| `item_material_processed_wood` | Existe | `Assets/_Game/Data/Items/Item_Processed_Wood.asset` |
| `item_fish_common` | Existe | `Assets/_Game/Data/Items/Item_Fish_Common.asset` |
| `seed_wheat` | Existe | `Assets/_Game/Data/Items/Item_Semente_Trigo.asset` |
| `seed_carrot` | Existe | `Assets/_Game/Data/Items/Item_Semente_Cenoura.asset` |
| `item_tool_fishing_rod_basic` | Existe | `Assets/_Game/Data/Items/Item_Cana_Basica.asset` |
| `item_material_stone` | Ausente | Necessario para ResourceNode fallback futuro. |
| `ore_copper` | Ausente | Necessario para cave/resources futuro. |

## Decisao

Como este pacote consolida hardening antes da cave procedural, nao foram criados assets novos de stone/copper. O drop atual do Slime em `Enemy_Slime.asset` usa `item_wood`, que existe no `ItemDatabase`.

## Pendencia

Criar e registrar `item_material_stone` e `ore_copper` quando ResourceNode/cave resources entrar no fluxo PR-131+.



