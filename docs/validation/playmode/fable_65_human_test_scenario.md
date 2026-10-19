# fable_65 — Cenário Humano Play Mode (DEFERRED_TO_FINAL_VALIDATION)

> Metas diárias: recompensa idempotente + widget no HUD + catálogo 4-6 metas.
> Pré-requisito humano: ligar os labels visuais do `DailyGoalsHudView` no GameplayHudCanvas
> (headless, igual às demais views WI-23). Pré-validação automatizada: 18 testes EditMode
> (FarmDailyGoalServiceTests.cs). Builds 0E.

## Dia completo

1. Iniciar novo jogo. Ao acordar (DayStartedEvent), abrir o HUD.
   - ESPERADO: widget de metas diárias visível, listando 6 metas com `[ ]` e progresso `n/m`
     nas que exigem >1 (Colher 3, Plantar 3).
2. Plantar 3 sementes.
   - ESPERADO: meta "Plantar 3 sementes" avança 1/3 → 2/3 → 3/3, conclui, toast
     "Meta concluída: +10 ouro, +8 XP"; ouro e XP do player sobem 1x.
3. Colher 3 cultivos.
   - ESPERADO: "Primeira colheita do dia" conclui na 1ª colheita (+15/+10); "Colher 3 cultivos"
     conclui na 3ª (+20/+12). Cada toast 1x; widget marca `[x]`.
4. Cortar 1 árvore.
   - ESPERADO: "Coletar 1 recurso da fazenda" conclui (+15/+8).
5. Falar com 1 morador (TownScene).
   - ESPERADO: "Falar com 1 morador" conclui (+10/+5).
6. Vender 1 item colhido.
   - ESPERADO: "Vender primeiro item colhido" conclui (+20/+10).
7. Conferir ouro/XP total ganho no dia.
   - ESPERADO: no máximo +90 ouro e +53 XP (soma da tabela). Nenhuma meta paga 2x.

## Idempotência por reload

8. Após concluir 2-3 metas, salvar e carregar o jogo.
   - ESPERADO: metas concluídas continuam `[x]`/Claimed; NENHUMA recompensa é re-paga;
     ouro/XP inalterados pelo load.

## Reset diário

9. Dormir (avança o dia).
   - ESPERADO: widget reseta todas as metas para `[ ]` e 0/m; ao re-concluir, paga de novo.

## Visibilidade do HUD

10. Abrir um modal (inventário/skill tree).
    - ESPERADO: widget de metas some junto com o resto do HUD (HudVisibilityController) e
      retorna ao fechar o modal. Widget é read-only (sem input próprio).
