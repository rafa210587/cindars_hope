# Human Test Scenario — fable_62 Onboarding Hints + Controls Screen

> Spec: `fable_62_spec_onboarding_control_hints`
> Timing: DEFERRED_TO_FINAL_VALIDATION (lote final — jornada do jogador novo)
> Pre-req: Play Mode (Unity Editor). Comece SEM save (new game) para os hints serem elegiveis.

## Objetivo

Confirmar que os 5 hints de onboarding aparecem na PRIMEIRA ocorrencia de cada contexto, via
toast do HUD (canal WI-23), exatamente 1x por save, sem bloquear gameplay; e que a tela Controles
lista o input map canonico (read-only, Esc fecha).

## Setup

1. Apague/renomeie o save atual (ou use slot novo) para garantir `OnboardingHints` vazio.
2. Entre em Play Mode a partir da FarmScene (boot normal de gameplay).

## CA-1 / CA-5 — Hints basicos, sem bloqueio

| Passo | Acao | Esperado |
|---|---|---|
| 1 | Boot de gameplay (HUD aparece) | Toast "WASD ou setas para mover." aparece nos primeiros segundos. Movimento NAO trava. |
| 2 | Ande com WASD enquanto o toast esta visivel | O player se move normalmente durante o toast (zero bloqueio, sem modal). |
| 3 | Aproxime-se de um objeto interagivel (crop, recurso, NPC) | Toast "E interage / coleta / usa." aparece na 1a vez que surge o prompt de interacao. |
| 4 | Afaste e reaproxime de outro interagivel | O hint de interacao NAO reaparece (1x/save). |
| 5 | Equipe uma arma (painel K / Equipamento) na mao | Toast "E ataca (segure para carregar); Q usa a mao esquerda." aparece na 1a arma equipada. |
| 6 | Reequipe / troque de arma | O hint de ataque NAO reaparece. |

## CA-2 — Aviso da caverna com defesa

| Passo | Acao | Esperado |
|---|---|---|
| 7 | Entre na caverna pela 1a vez (CaveScene / 1o nivel) | Toast de PERIGO aparece: menciona que morrer derruba itens + "Space desvia; segure Shift para bloquear". |
| 8 | Avance de nivel e volte | O aviso da caverna NAO reaparece (1x/save). |

> Nota de debito (CA-2 prioridade): o aviso usa o canal de toast Normal com duracao estendida (6s).
> A marca "Important" (furar a fila) exigiria editar `GameplayFeedbackService`, arquivo proibido por
> esta spec — ver execution report (debito documentado). Verifique apenas que o aviso APARECE e e
> legivel; nao e exigido que ele interrompa um toast em andamento.

## CA-1 — Status sofrido

| Passo | Acao | Esperado |
|---|---|---|
| 9 | Sofra um status no player (veneno/sangramento de armadilha ou inimigo) | Toast "Status ativos (...) aparecem no HUD." na 1a vez. |
| 10 | Sofra outro status | O hint de status NAO reaparece. |

## CA-3 — Persistencia (save/load)

| Passo | Acao | Esperado |
|---|---|---|
| 11 | Salve o jogo (F5 dev ou aba Sistema) apos ver alguns hints | Save concluido. |
| 12 | Carregue o save (F9 / aba Sistema) | Nenhum hint ja visto reaparece. |
| 13 | (Save legado) Carregue um save anterior a esta spec | Carrega sem erro; hints ficam elegiveis (lista vazia = default seguro). |

## CA-4 — Tela Controles (aba Sistema)

> Entrega parcial: o conteudo/logica estao prontos; a colocacao visual na aba Sistema depende de
> bind de scene/prefab (fora do escopo desta spec). Quando a entrada estiver ligada:

| Passo | Acao | Esperado |
|---|---|---|
| 14 | Abra a aba Sistema (painel unico F14) e a entrada "Controles" | Lista read-only: Movimento, Combate, Paineis, Dentro de menus. Teclas espelham input_map.md. |
| 15 | Confira que NAO ha teclas de debug (F5/F9/Tab-advance/B/O/P) | Apenas bindings canonicos. |
| 16 | Pressione Esc | A tela fecha (volta a aba); player nao se move enquanto aberta. |

## Regressao

- Toasts existentes (save/load, quest, loot, caverna-nivel) continuam funcionando.
- ContextHintController ("[E] {prompt}") inalterado e coexiste com os hints.
- Nenhum modal/bloqueio novo no fluxo de gameplay durante os toasts.

## Resultado

- [ ] Todos os 5 hints aparecem 1x e nao reaparecem (mesmo save / apos reload).
- [ ] Aviso da caverna legivel com dodge/block.
- [ ] Zero bloqueio de gameplay durante hints.
- [ ] Tela Controles read-only com input map canonico (quando ligada).
