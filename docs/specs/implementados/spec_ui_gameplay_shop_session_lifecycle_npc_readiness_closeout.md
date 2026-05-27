# SPEC 17E - Fix definitivo ShopSession lifecycle e NpcShopController readiness

**Status:** Implementado em codigo; dotnet compile PASS; Unity/Play Mode pendentes
**Data de implementacao:** 2026-05-26
**Ordem de execucao:** 17E
**Depende de:** SPEC 17D implementada em codigo
**Bloqueia:** fechamento humano final de shops/UI gameplay

## /speckit.specify

Garantir que `shop_weapons_armor` e `shop_seeds_tools` sejam registrados no `ShopManager` persistente usado pelos NPCs e pelos paines de compra/venda, inclusive apos transicao para `TownScene`.

## /speckit.plan

- Promover `ShopManager` ao contexto persistente de `GameBootstrap`, com rebind explicito no save.
- Tornar `NpcShopController` idempotente em `OnEnable`, `Start`, interacao e buy/sell.
- Separar diagnosticos de readiness, refs, asset vazio e sessao ausente.
- Expor sessoes registradas no `ShopManager`.
- Validar scene wiring, assets, sessoes e missing scripts em Editor utility.

## /speckit.tasks

- [x] Registrar causa raiz e evidencia de baseline.
- [x] Implementar lifecycle persistente e readiness idempotente.
- [x] Implementar validator `Validate Town Shop Wiring`.
- [x] Executar gates automaticos disponiveis.
- [x] Registrar pendencias de Play Mode final.

## Evidence / Closeout

**Implementado em codigo em:** 2026-05-26 (commit `d19347d`)

**Causa raiz:** `GameBootstrap` persistente (DontDestroyOnLoad) destruia o bootstrap duplicado da cena ao transicionar para `TownScene`; o `ShopManager` local perdia associacao com os NPCs que precisavam registrar `shop_weapons_armor` e `shop_seeds_tools`.

**Evidencia de codigo:**

- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` — `EnsurePersistentShopManager()` expoe ShopManager persistente; injeta no save/scene installers
- `Assets/_Game/Scripts/NPC/NpcShopController.cs` — rebind para referencias persistentes; `TryEnsureShopInitialized(reason)` com diagnostico separado por campo/causa; readiness idempotente
- `Assets/_Game/Scripts/Economy/ShopManager.cs` — `RegisteredShopIds`, `HasSession(shopId)`, `GetDiagnosticSummary()`; inicializacao valida IDs/itens/preco contra ItemDatabase
- `Assets/_Game/Scripts/UI/Shop/BuyPanel.cs`, `SellPanel.cs` — reportam manager/sessao ausente com sessoes conhecidas; nao criam fallback
- `Assets/_Game/Scripts/Editor/Validation/ValidateTownShopWiring.cs` — singleton manager, paineis/NPCs/assets, missing scripts e sessoes obrigatorias

**Evidencia estatica de assets:**

- `TownScene.unity`: um unico `ShopManager` no `_Bootstrap`; dois NPCs apontam para ele
- `Shop_Weapons_Armor.asset`: id `shop_weapons_armor`, itens nao vazios, `BaseValue > 0`
- `Shop_Seeds_Tools.asset`: id `shop_seeds_tools`, itens nao vazios, `BaseValue > 0`

**Gates automaticos executados:**

| Gate | Resultado |
|---|---|
| dotnet build runtime | PASS (0 erros, 7 warnings legados) |
| dotnet build editor | PASS (0 erros) |
| Guardrails runtime (Find, FindObjectOfType) | PASS (nenhum uso novo) |
| Docs validation | PASS |
| git diff --check | PASS |
| Unity compile/batchmode | NOT RUN (outra instancia Unity aberta) |

**Validacoes pendentes (gates Unity/Play Mode):**

- Executar `Cindar's Hope/Validation/Validate Town Shop Wiring` na TownScene
- Missing scripts nas tres cenas e prefabs
- Play Mode: sessoes `shop_seeds_tools` e `shop_weapons_armor` em runtime
- Play Mode: buy/sell dos dois NPCs
- Play Mode: save/load apos compra

**Evidencia documental:** `docs/validation/SPEC17E_SHOP_SESSION_FIX_VALIDATION_20260526.md`
