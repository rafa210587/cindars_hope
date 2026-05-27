# SPEC 17E - ShopSession Lifecycle Fix Validation - 2026-05-26

## Status

Implementado em codigo. Unity compile, scanners e Play Mode permanecem pendentes de evidencia final.

## Causa raiz

- `TownScene` possuia exatamente um `ShopManager` serializado e os NPCs referenciavam o mesmo manager/painel.
- `GameBootstrap` e persistente por `DontDestroyOnLoad`; ao transicionar para a Town, o `_Bootstrap` duplicado da cena era destruido.
- O `ShopManager` da Town ficava associado ao bootstrap destruido, enquanto os NPCs ainda precisavam registrar/consultar `shop_weapons_armor` e `shop_seeds_tools`.

## Correcoes implementadas

- `GameBootstrap` agora expoe um `ShopManager` persistente e cria um no bootstrap de cenas antigas quando a referencia ainda nao esta serializada.
- `NpcShopController.TryEnsureShopInitialized(reason)` valida campos separadamente, adota managers persistentes, inicializa sessao idempotentemente e impede interacao sem readiness.
- `ShopManager` agora fornece `RegisteredShopIds`, `HasSession` e `GetDiagnosticSummary`; a inicializacao valida IDs, itens no database e preco valido.
- `BuyPanel` e `SellPanel` apenas diagnosticam manager/sessao ausente e listam sessoes conhecidas; a responsabilidade de criar sessao permanece no NPC/manager.
- `ValidateTownShopWiring` verifica missing scripts, um unico manager, wiring dos dois NPCs/paines, dados/itens precificados e as duas sessoes exigidas.
- `TownScene` e os geradores de Farm/Town/Cave passaram a ligar o `ShopManager` ao bootstrap/save persistentes.

## Evidencia estatica

- `TownScene` tem um unico `ShopManager` no `_Bootstrap` e os dois NPCs apontam para ele.
- `Shop_Weapons_Armor.asset`: id `shop_weapons_armor`, itens nao vazios.
- `Shop_Seeds_Tools.asset`: id `shop_seeds_tools`, itens nao vazios.
- Itens das duas lojas inspecionados possuem `BaseValue > 0`; o validator reforca a verificacao contra `ItemDatabaseSO`.

## Gates automaticos executados

| Gate | Resultado | Evidencia |
|---|---|---|
| Runtime compile fallback | PASS | `dotnet build .\Assembly-CSharp.csproj --no-restore`: 0 erros; 7 warnings preexistentes. |
| Editor compile fallback | PASS | `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`: 0 erros. |
| New forbidden runtime searches | PASS | Nenhum uso novo de busca global foi introduzido; usos localizados em `GameBootstrap.InitializeUIControllers` ja existiam fora do diff desta spec. |
| Docs validation | PASS | `.\tools\docs\validate_docs.ps1`. |
| Diff whitespace validation | PASS | `git diff --check`. |

## Gates Unity pendentes

| Gate | Status |
|---|---|
| Unity compile batchmode | NOT RUN: tentativa abortada porque outra instancia Unity mantem o projeto aberto. |
| `Cindar's Hope/Validation/Validate Town Shop Wiring` | Pendente no Unity. |
| Missing scripts Farm/Town/Cave e prefabs | Pendente no Unity. |
| Sessions `shop_seeds_tools` e `shop_weapons_armor` em runtime | Pendente no Play Mode. |
| Buy/Sell dos dois NPCs | Pendente no Play Mode. |
| Save/load apos compra | Pendente no Play Mode. |

Comando Unity tentado:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -ProjectPath '.' -LogFile '.\Logs\spec17e-unity-compile-validation.log' -TimeoutSeconds 180
```

Resultado: `Multiple Unity instances cannot open the same project.` O bloqueio ocorreu antes de compile/scanner/Play Mode.

## Criterio de fechamento

A SPEC 17E nao esta fechada. Promover somente apos a execucao Unity confirmar as duas sessoes, buy/sell, ausencia de missing scripts e persistencia via save/load.
