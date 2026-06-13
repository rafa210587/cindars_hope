# SPEC - Hunger, stamina, status, tempo e balance

> Spec ID: spec_hunger_stamina_status_balance
> Status: Implementado completo
> Ordem de execucao: 09
> Data de fechamento: 2026-05-24

## Entrega

- `HungerManager` publica estados de fome e aplica dano de HP controlado por `GameTimeTickEvent` quando a fome chega a zero, com respawn seguro.
- `StaminaManager` inicia em `100`, regenera apos delay, respeita tiers de fome e mantem regeneracao fixa em fome zero sem dano incorreto de stamina.
- `PlayerNeedsBalanceSO` e `GameTimeBalanceSO` mantem os valores de balanceamento em assets configuraveis.
- `PlayerController` aplica reducao de movimento no tier critico de fome.
- Custos de stamina existentes em farm, pesca, arvores, craft e ataque sao religados ao manager persistente.
- `GameTimeManager`, `StaminaManager` e `StatusEffectManager` sao materializados e ligados em Farm, Town e Cave.
- Comidas aplicam `HungerRestore`, `StaminaRestore` e status configurados em `ItemDataSO`.
- Status temporarios publicam apply/refresh/remove/expire, persistem por ID/duracao e aparecem no HUD minimo com duracao.
- `SaveManager` captura/restaura hunger, stamina, game time e status apenas com DTOs simples.

## Reclassificacao

- Canvas final, icones finais e acabamento visual do HUD permanecem na SPEC 17. A SPEC 09 entrega a visualizacao funcional minima no `DebugHud`/`PlayerNeedsHUD`.
- Validacao humana de input/UX em Play Mode permanece no checklist final do projeto; nao bloqueia esta promocao por regra do prompt.

## Evidencia

- Runtime: `Assets/_Game/Scripts/Player/`, `Assets/_Game/Scripts/Core/GameTimeManager.cs`, `Assets/_Game/Scripts/Save/SaveManager.cs`.
- Wiring/assets: `Assets/_Game/Scripts/Editor/SceneCreation/`, `Assets/_Game/Scenes/`, `Assets/_Game/Data/Config/PlayerNeedsBalance.asset`, `Assets/_Game/Data/Config/GameTimeBalance.asset`.
- Validacao: `docs/validation/SPEC_09_HUNGER_STAMINA_STATUS_TIME_VALIDATION_20260524.md`.
