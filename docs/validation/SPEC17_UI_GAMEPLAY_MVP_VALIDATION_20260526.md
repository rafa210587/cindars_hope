# SPEC 17 - UI Gameplay MVP Validation - 2026-05-26

## Escopo entregue

- Estoques MVP de compra nas lojas existentes e novos assets `shop_general_store`, `shop_blacksmith` e `shop_cave_supplies`.
- Valores de venda para bread, small potion, wood e iron ore; starter loadout com `item_shop_weapon_sword_iron`.
- Inventory com botoes de selecao e equipar/desequipar ligado ao `EquipmentManager`.
- Painel compacto de personagem/equipamento em `K`, incluindo `TrySpendAttributePoint`.
- Painel compacto de skill trees em `U`, compra com `Skill Points` e autoalocacao nos slots `R`, `T`, `Y`, `G`.
- Bloqueio de input gameplay durante modal em movimento existente, ataques, dodge, hotbar, consumo e avancar dia.

## Validacao automatica

| Check | Resultado | Evidencia |
|---|---|---|
| Geracao/preenchimento de shops por Editor utility | PASS | `Logs/spec17-create-shop-assets-pass2.log` encerra com return code 0 |
| Validator de shops Editor | PASS | `Logs/spec17-validate-shops-final.log`: 24 passed, 0 failed |
| Integration test shop flow | PASS | `Logs/spec17-shop-flow-test-final.log`: ALL TESTS PASSED; teste corrigido para isolar `ShopManager` por caso |
| Unity batchmode compile | PASS para C# | `Logs/spec17-ui-compile-final.log` sem `error CS` e com return code interno 0 |
| `dotnet build .\Assembly-CSharp.csproj` | PASS | 0 erros; 7 warnings anteriores |
| `git diff --check` | PASS | Sem whitespace errors |
| Busca de APIs proibidas novas | PASS no escopo novo | Nao foi introduzida busca global em runtime; usos remanescentes em `GameBootstrap` e `CaveLevelRuntimeController` sao anteriores |

## Observacoes de tooling

- `RunUnityCompileValidation.ps1` reportou exit code 1 embora o log do Unity finalize com `return code 0`.
- `ScanUnityLogs.ps1` marca assemblies `Assembly-CSharp-firstpass.dll` antigos como critico; nao ha `error CS` no log.
- A compilacao C# foi confirmada pelo fallback `dotnet build`.
- `tools/docs/validate_docs.ps1` falha em specs futuras preexistentes (`spec_damage_status_elements_resistances_runtime.md`, `spec_equipment_durability_environment_loot_runtime.md` e `spec_player_combat_weapons_spells_skill_actions_runtime.md`) sem marcadores/cabecalhos SpecKit; estes arquivos nao foram alterados neste incremento.

## Validacao humana final pendente

- Interagir com os dois vendedores da Town e confirmar compra/venda, estoque e feedback.
- Confirmar `I` inventario, equipar/desequipar a espada inicial e persistencia em save/load.
- Confirmar `K` personagem/equipamento e gasto de `Attribute Points`.
- Confirmar `U` skill tree, compra, prerequisitos e autoalocacao `R/T/Y/G`.
- Confirmar que WASD, ataque, dodge, hotbar e interacao nao executam sob cada modal aberto.

## Fechamento documental

Este documento comprova o incremento MVP solicitado. A SPEC 17B ampla nao foi promovida para implementados porque ainda inclui UI Canvas final, pause/options e fluxos cave/corpse/toasts fora deste recorte.
