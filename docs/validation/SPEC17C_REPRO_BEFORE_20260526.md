# SPEC 17C - Evidencia de reproducao antes das correcoes - 2026-05-26

## Contexto

Reproducao automatizavel executada antes de alterar a implementacao da SPEC 17C.
A validacao humana Play Mode permanece para o fechamento final.

## Evidencias confirmadas

| Sintoma | Evidencia pre-correcao | Resultado |
| --- | --- | --- |
| Skill Trees sem manager | `FarmScene.unity`, `TownScene.unity` e `CaveScene.unity` serializam `_skillTreeManager: {fileID: 0}` no bootstrap/save; geradores nao adicionam nem atribuem `SkillTreeManager`. | Reproduzido estaticamente. |
| `K` abre superficie mista | `CharacterEquipmentPanelController` trata apenas `KeyCode.K` e desenha atributos e slots de equipamento no mesmo painel. | Reproduzido estaticamente. |
| Shop buy/sell sem diagnostico suficiente | `BuyPanel.Show` retorna com `BuyPanel cannot show shop`; `SellPanel.Show` retorna com `ShopManager not initialized`; `NpcShopController` nao verifica sucesso de `InitializeShop`. | Reproduzido em codigo; fluxo Play Mode deve ser retestado apos correcao. |
| Prompts 15/16 continuam ativos | Existem stubs `docs/agent_prompts/a_executar/SPEC_15_*` e `SPEC_16_*`, apesar das versoes em `implementados/`. | Reproduzido. |
| Actions HUD obsoleta | `DebugHud.DrawCommands` nao lista `I`, `K`, `L`, `U` ou `Esc` e mantem comandos antigos. | Reproduzido. |

## Missing scripts

- `MissingScriptScanner.ScanPrefabs` executado em batchmode informou zero missing scripts em prefabs `Assets/_Game`.
- Scan batchmode da cena `TownScene` informou zero missing scripts na cena aberta.
- Os comandos equivalentes para `FarmScene` e `CaveScene` nao produziram linha conclusiva do scanner; o scanner existente nao possui gate unico para abrir e validar as tres cenas.
- O relato manual de warnings em Play Mode deve ser revalidado depois que o scanner for fortalecido.

## Acao requerida

Implementar wiring persistente de skill tree/save, separar teclas `K`/`L`, endurecer inicializacao do shop, automatizar scans das tres cenas e reconciliar documentos/fila de prompts.
