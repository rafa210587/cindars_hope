# SPEC 17C - Closeout Validation - 2026-05-26

## Status

**Implementado em codigo; gates automaticos executados; Play Mode humano final pendente.**

SPEC 17C nao esta fechada porque os fluxos interativos finais exigidos devem ser confirmados no Unity pelo humano.

## Correcoes aplicadas

- `SkillTreeManager` agora e ligado ao `GameBootstrap` e ao `SaveManager` nas tres cenas gameplay.
- Geradores de `FarmScene`, `TownScene` e `CaveScene` preservam esse wiring em recriacoes futuras.
- `K` abre atributos/progressao; `L` abre equipamento no controller compacto; `U` permanece Skill Trees.
- `BuyPanel`/`SellPanel` e `NpcShopController` falham com diagnostico explicito quando sessao/referencia esta ausente.
- `BuyPanelItem` e `SellPanelItem` foram separados em scripts attachable proprios; os dois componentes orfaos de `TownScene` foram restaurados.
- `MissingScriptScanner` valida as tres cenas gameplay e prefabs, falhando com caminho e indice exatos.
- Actions HUD lista os atalhos vigentes, incluindo `I`, `K`, `L`, `U` e `Esc`; `J` foi ligado ao ataque principal.
- Stubs de prompts SPEC 15/16 foram removidos de `docs/agent_prompts/a_executar/`.
- Stubs de specs ja promovidas (10-12, 15 e 16) foram removidos de `docs/specs/a_implementar/`; os residuais de 13/14 foram alinhados no registry e na ordem.

## Gates automaticos

| Gate | Comando / evidencia | Resultado |
| --- | --- | --- |
| Unity compile interno | `Logs/spec17c-unity-compile-validation.log` contem `Tundra build success (0.08 seconds)` e nenhum `error CS`. | PASS tecnico |
| Compile runtime fallback | `dotnet build .\Assembly-CSharp.csproj` | PASS, 0 erros; 7 warnings legados |
| Compile Editor fallback | `dotnet build .\Assembly-CSharp-Editor.csproj` | PASS, 0 erros |
| Missing scripts - gameplay | `MissingScriptScanner.ScanGameplayScenes`, log `Logs/spec17c-missing-gameplay-scenes-final.log` | PASS, zero missing scripts |
| Missing scripts - prefabs | `MissingScriptScanner.ScanPrefabs`, log `Logs/spec17c-missing-prefabs-final.log` | PASS, zero missing scripts |
| Shop data/managers | `ValidateShopSystem.ValidateShops`, log `Logs/spec17c-validate-shop.log` | PASS, 24 passed / 0 failed |
| Shop integration | `IntegrationTest_ShopFlow.RunShopFlowTest`, log `Logs/spec17c-integration-shop-flow.log` | PASS, all tests passed |
| Docs / registries | `tools/docs/validate_docs.ps1` | PASS apos reconciliar stubs promovidos |
| Diff whitespace | `git diff --check` | PASS |
| Runtime search guardrail | Busca por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `StreamingAssets` | Nenhum uso novo; warnings de `FindObjectOfType` no bootstrap sao preexistentes |

## Tooling residual

- `tools/unity/RunUnityCompileValidation.ps1` retornou falha por exit code externo `1`, embora seu log contenha `Tundra build success`, nenhum `error CS` e shutdown Unity com return code interno `0`.
- `tools/unity/ScanUnityLogs.ps1` marca mensagens legadas de `Assembly-CSharp-firstpass.dll not valid` como criticas; nao representam erro C# desta entrega.
- O primeiro `tools/docs/validate_docs.ps1` apontou stubs preexistentes de specs ja movidas; os stubs de 10, 11, 12, 15 e 16 foram removidos da fonte ativa e o gate foi repetido.

## Gates humanos pendentes

- `FarmScene`: abrir `U`, ver cinco arvores, comprar node, confirmar reducao de skill points e fechar com `Esc`.
- Pressionar `K`, gastar attribute point; confirmar que nao exibe equipamento.
- Equipar via `I`, abrir `L`, desequipar e confirmar inventario.
- `TownScene`: comprar e vender com vendedor de armas; confirmar gold/inventory e console limpo.
- Comprar/equipar/comprar skill, salvar com `F5`, carregar com `F9` e confirmar persistencia.

## Atalhos

| Tecla | Funcao |
| --- | --- |
| `I` | Inventory |
| `K` | Attributes / progression |
| `L` | Equipment |
| `U` | Skill Trees |
| `Esc` | Close modal / back |
