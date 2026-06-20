# Human Play Mode Scenario — fable_44 (Cave Save Completion Policy)

> **Spec:** `.specs/a_implementar/fable/fable_44_spec_cave_save_completion_policy.md`
> **Report:** `docs/validation/fable_44_spec_cave_save_completion_policy_execution_report.md`
> **Timing:** DEFERRED_TO_FINAL_VALIDATION (executar no lote final de Play Mode)
> **Pré-requisito:** CaveScene jogável (wiring humano das WAVE16/17 aplicado) + boss gate configurado.

Este cenário valida o que o EditMode não cobre: o fluxo cena↔save end-to-end do snapshot multi-nível
e da recusa de save em boss fight.

---

## A — Reload multi-nível preserva estado mutável (CA-1)

1. Inicie uma run nova de caverna; desça até o **nível 3** (passando por 1 e 2).
2. No **nível 1**: mate pelo menos 1 inimigo e abra 1 baú (se houver). Anote.
3. No **nível 2**: deplete 1 nó de recurso. Anote.
4. No **nível 3** (corrente): salve o jogo (F5). Confirme o toast "Save complete." (ou equivalente).
5. Feche o Unity Editor por completo e reabra; carregue o save (F9).
6. **Volte** ao nível 1 (BackExit/checkpoint). **Esperado:** o inimigo morto continua morto, o baú
   continua aberto.
7. Volte ao nível 2. **Esperado:** o nó depletado continua vazio.

PASS se mortos/baús/depleção dos níveis 1 e 2 sobreviveram ao fechar/reabrir o jogo (não só o nível 3).

---

## B — Save recusado durante boss fight (CA-3)

1. Desça até um nível com **boss gate** (boss ainda não derrotado).
2. Engaje o boss (entre na arena / aproxime-se até o boss spawnar e iniciar a luta).
3. Tente salvar (F5). **Esperado:** o save é **recusado** com a mensagem
   **"Nao e possivel salvar agora."** (no canal de feedback/HUD existente). Nenhum arquivo de save
   novo é escrito durante a luta.
4. **Derrote** o boss. Tente salvar (F5). **Esperado:** o save **funciona** normalmente.
5. (Variação) Reengaje outro boss; em vez de derrotar, **saia do nível** (ou da caverna). Tente salvar.
   **Esperado:** o save **funciona** (o flag foi limpo na saída — nunca fica órfão).
6. (Variação) Reengaje um boss e **morra** (KO). Após o respawn/fluxo de morte, tente salvar.
   **Esperado:** o save **funciona** (flag limpo na derrota do player).

PASS se: save bloqueado SÓ durante a luta ativa; liberado após derrota, saída ou morte.

---

## C — Compatibilidade com save antigo (CA-4)

1. Se houver um save F13 anterior (gerado antes desta spec), carregue-o.
2. **Esperado:** carrega sem erro; a run retoma no nível salvo (comportamento de um nível — o save
   antigo só tinha o snapshot corrente).
3. Salve de novo (fora de boss) e recarregue. **Esperado:** agora multi-nível ativo, sem erro.

PASS se o save legado carrega sem exceção e o novo save grava multi-nível.

---

## D — Stable-run inalterado (ADR-0005, anti-regressão)

1. Em qualquer nível revisitado, observe os logs: `Layout hash validated for level N` (sem
   `Layout hash mismatch`).
2. **Esperado:** layout, entrada/saída, composição de inimigos/recursos idênticos à primeira visita;
   `CaveRunSeed` constante na run.

PASS se nenhum `Layout hash mismatch` aparece e o seed não muda em ForwardExit/BackExit.

---

## Registro

| Bloco | Resultado | Observações |
|---|---|---|
| A — reload multi-nível | ☐ PASS / ☐ FAIL | |
| B — save em boss | ☐ PASS / ☐ FAIL | |
| C — compat save antigo | ☐ PASS / ☐ FAIL | |
| D — stable-run | ☐ PASS / ☐ FAIL | |
