# SPEC 17D - Closeout Validation - 2026-05-26

## Status

Implementado em codigo. Unity batchmode e Play Mode final permanecem pendentes porque o projeto estava aberto em outra instancia Unity durante a validacao.

## Correcoes implementadas

- `NpcShopController` inicializa a sessao antes dos paines e verifica, antes de abrir buy/sell, que a sessao existe e que o painel esta ligado ao mesmo `ShopManager`, player, inventory, database e modal context.
- `BuyPanel` e `SellPanel` expõem verificacao explicita de contexto inicializado; falhas continuam logadas com cena, GameObject, componente e `shopId`.
- `ValidateSpec17DShopUiWiring` valida `TownScene`: missing scripts, instancia unica de manager/painel, referencias dos NPCs, campos UI e itens precificados nos shops.
- `L` agora mostra `Chest`, `RightHand`, `LeftHand` e `Accessory` com acoes `Equipar/Trocar` e `Desequipar`.
- `InventoryPanelController` possui modo de selecao por `EquipmentSlot`; itens incompativeis ficam desabilitados, a selecao equipa e retorna ao painel `L`, e `Esc` cancela retornando ao painel.
- `InventoryManager` registra novos bindings por slot (`equipment-slot:<slot>`) e limpa apenas a stack afetada; bindings antigos por `itemId` possuem fallback limitado a uma stack.

## Gates executados

| Gate | Resultado | Evidencia |
|---|---|---|
| Runtime compile fallback | PASS | `dotnet build .\Assembly-CSharp.csproj`: 0 errors; 7 warnings legados. |
| Editor compile fallback | PASS | `dotnet build .\Assembly-CSharp-Editor.csproj` incluindo localmente `ValidateSpec17DShopUiWiring.cs`: 0 errors. |
| Guardrails novos runtime | PASS | Nenhum novo `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` nos arquivos runtime alterados. |
| Unity compile/batchmode | BLOCKED | `Logs/spec17d-unity-compile-validation.log`: outra instancia Unity esta com o projeto aberto; encerrou antes de compilar. |

## Auditoria do baseline e wiring

- `TownScene.unity` possui um `ShopManager` e os dois `NpcShopController` serializados apontam para o mesmo manager, `BuyPanel` e `SellPanel`.
- A 17C ja registrou `ValidateShopSystem` com 24/0, integration shop flow PASS e missing scripts zero; a 17D exige rerun no Unity apos a instancia aberta ser liberada por tocar runtime e validator novo.
- Prompts 15/16 permanecem fora de `docs/agent_prompts/a_executar/`; o prompt ativo novo e `SPEC_17D_shop-injection-equipment-slot-picker_PROMPT.md`.
- A Actions HUD ja refletia `I`, `K`, `L`, `U` e `Esc` desde a 17C; nenhuma mudanca adicional foi necessaria.

## Pendencias obrigatorias no Unity

- Executar `Validate SPEC 17D Shop UI Wiring`.
- Reexecutar scanner de missing scripts nas tres cenas gameplay e em prefabs `Assets/_Game`.
- Play Mode em `TownScene`: comprar/vender em `shop_seeds_tools` e `shop_weapons_armor`.
- Play Mode: `K` atributos, `L` slot picker com espada/ferramenta/armadura/acessorio, `Esc` cancelar e `I` inventario normal.
- Save/load com item comprado e equipado.

## Comandos atuais

- `I`: inventory normal
- `K`: attributes/progression
- `L`: equipment slot picker
- `U`: skill trees
- `Esc`: close/back/cancel selection
