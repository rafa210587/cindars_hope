# fable_38 — Cenário humano (Play Mode) — Minimapa v1

> Diferido para a validação final do lote (DEFERRED_TO_FINAL_VALIDATION). Cobre o que os
> EditMode tests não cobrem: scene/canvas, input, anchoring %, binding visual da Texture2D.
> Pré-requisito: binding visual do `MinimapWidget` (RawImage + Texture2D) feito no Editor e o
> widget construído sob o relógio (F20) no GameplayHudCanvas.

## CA-3 — Town/Farm estático + toggle M

1. Carregue a TownScene. Confirme o minimapa no canto superior-direito, ABAIXO do widget de
   relógio (F20), ancorado em % (redimensione a janela: o widget acompanha, sem pixels fixos).
2. Confirme planta estática completa, SEM fog. Pontos: player (branco), NPCs (amarelo),
   board/mural (azul), entrada da caverna (vermelho). Sem inimigos no mapa.
3. Pressione **M**: o widget some. Pressione **M** de novo: volta. (Sem painel/modal aberto.)
4. Repita na FarmScene.

## CA-1 — Fog intra-run na caverna + revisita

5. Entre na caverna (nível 1). Confirme que o mapa nasce ESCURO (só o entorno imediato do
   player visível). Player sempre centrado; ao andar, o mapa rola e novas células são reveladas
   num raio ~6.
2. Ande até a escada de descida; confirme o ícone de escada aparecendo quando a célula é
   revelada.
6. Desça um nível e VOLTE (BackExit) ao nível 1: confirme que o já-revelado permanece (não
   re-escurece). Salve e recarregue na mesma run: o revelado persiste (estado de nível do
   stable-run).

## CA-2 — Reset por nova run

7. Force KO/death (ou new game). Reentre na caverna: o fog do nível 1 deve estar ZERADO
   (caverna escura de novo) — nova `CaveRunSeed`.

## lantern_of_true_sight (F31 — hook)

8. Com a `item_magic_lantern_of_true_sight` equipada, confirme que o raio de revelação cresce
   (6 → 9): ao parar numa célula, uma área visivelmente maior é revelada.

## Aba Mapa (F14)

9. Abra o painel único e vá à aba **Mapa**: confirme a versão expandida (2×) com legenda das
   cores/ícones. Esc/back fecha o painel; com o painel aberto, **M** não alterna o overlay
   (guard de modal). O player não se move com o painel aberto.

## CA-4 — Throttle (observacional)

10. Com o profiler/console, confirme que o minimapa atualiza no máximo ~4×/s (não por frame) e
    sem alloc perceptível por update (buffer/Texture2D reutilizados).
