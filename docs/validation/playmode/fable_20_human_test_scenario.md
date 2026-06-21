# fable_20 — Human Play Mode Test Scenario

> **Spec:** `fable_20_spec_calendar_clock_hud_day_detail_ui`
> **Status:** DEFERIDO para validação final (dono autorizou pular Play Mode nesta sessão).
> Cobre o que os EditMode tests não conseguem: cena, canvas, modal, input, foco visível.

## Pré-condições
- Novo jogo ou save válido carregado; GameplayHudCanvas presente.
- `ClockCalendarHudWidgetView` configurado com `GameCalendarService` e `LunarCycleService` (injeção via bootstrap, sem GameObject.Find).
- Binding visual do widget e da aba Calendário aplicado no prefab/scene.

## Widget de relógio/calendário (CA-1)
1. [ ] Widget aparece no canto **superior-direito** (sob o espaço do minimapa F38), legível a 1280×720 e 1920×1080.
2. [ ] Mostra fase (Dia/Noite), data "Semeio D3 · Dia 3/28 · Ano 1", clima de hoje, fase lunar.
3. [ ] Avançar um dia (dormir/virada Night→Day): data, clima e lua atualizam; estação rola de Semeio→Brasa no dia 29.
4. [ ] Trocar de fase (Dia↔Noite): rótulo muda sem recarregar a cena.
5. [ ] Subir de nível: barra fina de XP/“Nv N” junto ao relógio atualiza (EMENDA-C); toast “Nível N!” aparece.
6. [ ] Widget não fica atualizando texto a cada frame (custo): só muda em evento/virada de dia.

## Detalhe do dia / aba Calendário (CA-2, CA-3)
7. [ ] Abrir a aba Calendário pelo painel único F14 (NÃO há tecla `C` global — `C` é Craft de bolso).
8. [ ] Modal abre: HUD some (`HudVisibilityChangedEvent`), **relógio pausa** (gate único de time_rules Rule 2).
9. [ ] **Sem movimento do player** enquanto o modal está aberto (anti-regressão).
10. [ ] Detalhe mostra: data de hoje, clima de hoje, previsão de amanhã (se conhecida).
11. [ ] Pico lunar **conhecido** (descoberto) aparece como "Pico de Alihana/Senya/Nyx"; pico **não descoberto** NÃO aparece.
12. [ ] Aniversário de NPC do dia aparece (informação pública).
13. [ ] Foco navegável por teclado (setas/Tab): outline visível, ordem termina no botão Fechar, wrap linear.
14. [ ] **Esc** fecha o modal; HUD volta; relógio retoma do mesmo ponto.

## Anti-regressão
15. [ ] DebugHud IMGUI continua funcionando (debug), não duplicado pelo widget.
16. [ ] Nenhum serviço de tempo/calendário/lua/clima alterado em comportamento.
17. [ ] Abrir outro modal com a aba Calendário aberta é rejeitado (ModalManager single-stack).

## Resultado
- [ ] PASS  / [ ] FAIL — observações:
