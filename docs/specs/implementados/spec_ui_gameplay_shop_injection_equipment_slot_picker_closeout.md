# SPEC 17D - Fix Shop Injection + Equipment Slot Picker UX

**Status:** Implementado em codigo; dotnet compile PASS; Unity/Play Mode pendentes
**Data de implementacao:** 2026-05-26
**Ordem de execucao:** 17D
**Depende de:** SPEC 17C implementada em codigo
**Bloqueia:** fechamento humano final da UI gameplay

## /speckit.specify

Corrigir a inicializacao verificavel de `ShopManager`, `BuyPanel` e `SellPanel` em `TownScene` e transformar o painel de equipamento aberto por `L` em um seletor por slot que usa o inventario para escolher somente itens compativeis.

## /speckit.plan

- Endurecer o contrato runtime de shop sem busca global ou fallback silencioso.
- Adicionar validator Editor-time para wiring e dados dos shops em `TownScene`.
- Manter `K` para atributos, `I` para inventario normal, `U` para skill trees e `L` para equipamento.
- Permitir `Equipar`/`Trocar`/`Desequipar` por `Chest`, `RightHand`, `LeftHand` e `Accessory`.
- Abrir inventario em modo selecao para um slot alvo e retornar ao painel `L` ao selecionar ou cancelar.
- Identificar binding de inventario por slot de equipamento, evitando limpar multiplas copias por `itemId`.

## /speckit.tasks

- [x] Registrar evidencia anterior em `docs/validation/SPEC17D_REPRO_BEFORE_20260526.md`.
- [x] Validar e endurecer injecao shop buy/sell.
- [x] Implementar selector de equipamento por slot e filtro de compatibilidade.
- [ ] Validar missing scripts, compile Unity e shops em batchmode quando a instancia Unity liberar o projeto.
- [x] Registrar evidencias automaticas e pendencias de Play Mode.

## Evidence / Closeout

**Implementado em codigo em:** 2026-05-26 (commit `fffeaa9`)

**Evidencia de codigo:**

- `Assets/_Game/Scripts/NPC/NpcShopController.cs` — inicializa sessao antes dos paineis; valida sessao/contexto exato antes de abrir transacao
- `Assets/_Game/Scripts/UI/Shop/BuyPanel.cs`, `SellPanel.cs` — verificacao explicita de contexto inicializado; falhas logadas com cena/componente/shopId
- `Assets/_Game/Scripts/Editor/Validation/ValidateTownShopWiring.cs` — valida missing scripts, singleton de manager, refs dos NPCs/paineis, dados/itens precificados
- `Assets/_Game/Scripts/UI/Character/CharacterEquipmentPanelController.cs` — slots `Chest`, `RightHand`, `LeftHand`, `Accessory` com acoes `Equipar/Trocar` e `Desequipar`
- `Assets/_Game/Scripts/UI/InventoryPanelController.cs` — modo selecao por `EquipmentSlot`; itens incompativeis desabilitados; selecao equipa e retorna ao painel `L`

**Nota sobre nomenclatura:** `ValidateSpec17DShopUiWiring` foi renomeado para `ValidateTownShopWiring` (commit `d19347d` durante SPEC 17E) para manter nome semantico permanente.

**Gates automaticos executados:**

| Gate | Resultado |
|---|---|
| dotnet build runtime | PASS (0 erros, 7 warnings legados) |
| dotnet build editor | PASS (0 erros, incluindo ValidateTownShopWiring) |
| Guardrails runtime (Find, FindObjectOfType) | PASS (nenhum novo) |
| Unity compile/batchmode | BLOCKED (outra instancia Unity aberta) |

**Validacoes pendentes (gates Unity/Play Mode):**

- Executar `Cindar's Hope/Validation/Validate Town Shop Wiring` na TownScene
- Reexecutar missing scripts nas tres cenas e prefabs
- Play Mode: buy/sell em `shop_seeds_tools` e `shop_weapons_armor`
- Play Mode: `L` slot picker com espada/ferramenta/armadura/acessorio; `Esc` cancela
- Play Mode: save/load com item comprado e equipado

**Evidencia documental:** `docs/validation/SPEC17D_CLOSEOUT_VALIDATION_20260526.md`
