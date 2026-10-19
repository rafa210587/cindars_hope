# Human Play Mode Scenario — fable_66 (Limpeza de Débitos)

> Spec: `.specs/a_implementar/fable/fable_66_spec_code_debt_cleanup_slice_mode.md`
> Status: DEFERRED_TO_FINAL_VALIDATION (executar no lote final de validação humana)

Pré-requisito: build do projeto no Unity Editor sem erros de compile.

## Fluxo 1 — Loop de crop preservado (slice mode intacto)

1. Abrir `FarmScene` em Play Mode.
2. Interagir com um `FarmPlot` (slice mode ON): Arar → Molhar → Plantar → Simular crescimento → Colher.
3. **Esperado:** loop completo funciona como antes (comportamento NÃO mudou — slice mode re-registrado,
   não removido). Item colhido entra no inventário.

## Fluxo 2 — Block drena stamina + erro de wiring

1. Em Play Mode com player wired ao `StaminaManager`, segurar Left Shift (block).
2. **Esperado:** stamina drena ~18/s; ao zerar, block solta com feedback "Stamina insuficiente para block."
3. (Diagnóstico) Se um `PlayerBlockController` ficar sem `_staminaManager` (cena mal wired):
   **Esperado:** UM `Debug.LogError` com `[PlayerBlockController] WIRING ERROR: StaminaManager ausente.
   Scene=..., GameObject=..., campo='_staminaManager'` (uma vez, sem spam por frame). Nunca silencioso.

## Fluxo 3 — Morte na caverna grava dados reais + substitui corpo

1. Avançar o dia algumas vezes (ex.: dormir) para `TimeManager.CurrentDay > 1`.
2. Entrar na caverna, morrer (HP 0) — corpse criado.
3. **Esperado:** corpse registra o dia REAL (não 1 fixo) e um `SnapshotLayoutHash` não vazio
   (determinístico para a run/nível).
4. Morrer de novo na caverna (segundo corpse, id diferente).
5. **Esperado:** `CorpseReplacedEvent` publicado (log `[DeathSystemBootstrap] Corpse X replaced by Y`);
   o `CorpseInteractable` reflete o corpse mais recente. Sem corpse antigo órfão pelo caminho do resolver.

## Fluxo 4 — Dano ao player sem PlayerHitEvent

1. Levar dano de inimigo melee, contato e projétil na caverna.
2. **Esperado:** HP reduz por todos os caminhos (log `CombatLog: PlayerDamageReceived ...`), provando que
   o dano flui por `PlayerDamageReceiver.ApplyDamage` direto — o `PlayerHitEvent` aposentado não é necessário.
