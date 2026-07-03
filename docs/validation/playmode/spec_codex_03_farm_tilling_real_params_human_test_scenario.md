# Human Test Scenario — spec_codex_03_farm_tilling_real_params

## Feature Summary

Testar que arar/regar tiles de fazenda agora usa stamina real (`StaminaManager`) e dia real
(`TimeManager.CurrentDay`) em vez dos literais fixos `staminaOk: true` / `currentDay: 1`. Sem
stamina suficiente, a ação deve ser recusada com uma mensagem específica de stamina (não a
mensagem genérica de "tile inválido").

## Pré-requisito humano antes de testar

Regenerar a FarmScene via `CindarsHope/Inicializar Projeto` para que o `FarmTillingInputController`
existente na cena receba as novas referências de `StaminaManager`/`TimeManager` (o `EditorWire`
mudou de assinatura). Sem essa regeneração, o controller usa o fallback permissivo com warning no
console (comportamento antigo).

## Scenes

- FarmScene (spawn padrão do player)

## Required Initial State

- Save novo ou save existente com o player tendo enxada (Hoe) e regador (WateringCan) equipados/no
  inventário (via `EquipmentManager`).
- Stamina inicial no máximo (100 por padrão).

## Scenario 1 — Happy Path (stamina suficiente)

1. Inicie o jogo na FarmScene com stamina cheia.
2. Ande até um tile de solo arável ainda não arado.
3. Pressione `[F]`.
4. Verifique: o tile fica arado (feedback "Solo arado!" no toast/HUD).
5. Abra o HUD de stamina (ou o debug overlay) e confirme que a stamina caiu (custo esperado: 10).
6. Pressione `[F]` novamente sobre o mesmo tile (agora arado) para regar.
7. Verifique: o tile fica molhado (estado `TilledWet`); stamina cai novamente (custo esperado: 8).

Expected logs: nenhum warning de "StaminaManager nao wired" ou "TimeManager nao wired" (confirma
que o wiring real está ativo).

## Scenario 2 — Negative Path (stamina insuficiente)

1. Reduza a stamina do player para abaixo de 10 (ex.: correndo/atacando repetidamente, ou via
   debug tool se disponível).
2. Ande até um tile arável não arado e pressione `[F]`.
3. Verifique: a ação é recusada e aparece a mensagem específica "Stamina insuficiente para usar a
   ferramenta." (não a mensagem genérica "Nao e possivel arar aqui").
4. Confirme que a stamina NÃO foi descontada (a ação falhou antes do gasto).
5. Repita com stamina entre 8 e 9 sobre um tile já arado (tentando regar): deve recusar regar com
   a mesma mensagem específica de stamina.

Expected logs: nenhum erro; nenhum crash.

## Scenario 3 — Dia real na rega

1. Avance o dia do jogo (dormir ou mecanismo de avanço de dia existente) até `CurrentDay` > 1.
2. Are e regue um tile.
3. Confirme (via debug log ou inspeção do estado do tile) que o dia registrado na rega corresponde
   ao dia atual real do jogo, não sempre "1".

Expected logs: nenhum.

## Scenario 4 — Fallback permissivo (regressão, opcional)

Só se a FarmScene NÃO tiver sido regenerada com o wiring novo:

1. Pressione `[F]` sobre um tile arável.
2. Verifique que aparece o warning `[FarmTillingInputController] StaminaManager nao wired —
   permitindo acao sem verificacao de stamina (residual).` no console, e que a ação ainda funciona
   (fallback permissivo, sem crash) — confirma que a spec não quebrou o comportamento antigo quando
   o wiring não está presente.

## Expected Results

- Arar/regar com stamina suficiente funciona e desconta a stamina correta.
- Arar/regar sem stamina suficiente é recusado com feedback específico de stamina.
- Rega usa o dia real do jogo.
- Sem crashes; sem exceptions no console.

## Console Expectations

- Errors: NENHUM
- Warnings esperados: NENHUM (após regeneração da cena) — ou os warnings de fallback residual
  (Scenario 4), aceitáveis apenas se a cena ainda não foi regenerada.
- Proibido: qualquer `Exception` ou `ERROR` no console.

## Pass/Fail Checklist

- [ ] Arar com stamina suficiente funciona e desconta stamina (custo 10)
- [ ] Regar com stamina suficiente funciona e desconta stamina (custo 8)
- [ ] Arar sem stamina suficiente é recusado com mensagem específica de stamina
- [ ] Regar sem stamina suficiente é recusado com mensagem específica de stamina
- [ ] Stamina não é descontada quando a ação falha
- [ ] Dia real é usado na rega (não fixo em 1)
- [ ] Sem crashes / sem exceptions
- [ ] EditorWire retrocompatível (cena antiga sem regenerar ainda funciona via fallback, com warning)

**Overall:** NOT RUN (Play Mode deferred — Unity Editor não executável nesta sessão)

## Notes

- Tested on: N/A (pending human execution)
- Tested by: N/A
- Date: N/A
- Custo de stamina (10 para arar, 8 para regar) é placeholder de balance; sujeito a tuning em spec
  futura de economy-balance-tuning.
