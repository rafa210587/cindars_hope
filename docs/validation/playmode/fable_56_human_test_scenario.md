# fable_56 — Cenario humano de Play Mode: Aba Sistema + Fluxo de Titulo

> Spec: `fable_56_spec_system_tab_title_flow`
> Status de validacao humana: DEFERRED_TO_FINAL_VALIDATION (autorizado pelo dono — pular Play Mode nesta sessao).
> Este roteiro cobre o que os EditMode tests NAO podem provar: scene/canvas, foco, Esc, input blocking, empty/disabled state, e ausencia de movimento com modal aberto.

## Pre-condicoes

- Cena de gameplay aberta (FarmScene) com GameBootstrap ativo.
- A aba Sistema (8ª aba) registrada no painel unico F14 e populada por `SystemTabController`.
- Wiring humano pendente: vincular `SystemTabController` ao conteudo da aba Sistema no Canvas e
  `TitleScreenController` a uma entrada de boot (cena/overlay). Ver "Wiring pendente" abaixo.

## Roteiro

### CA-1 — Aba Sistema navegavel por teclado
1. Abrir o painel (tecla do painel) e ciclar com Tab ate a aba Sistema (8ª).
2. Verificar 4+1 entradas: Salvar, Carregar, Volume (3 canais), Video, Sair para o Titulo.
3. Navegar entre as entradas so com teclado (setas/Tab). Esperado: foco visivel muda; nenhuma exige mouse.
4. Com o painel aberto, tentar mover o player (WASD). Esperado: player NAO se move (input de gameplay bloqueado).
5. Pressionar Esc. Esperado: painel fecha; gameplay volta a responder.

### CA-2 — Salvar / Carregar com confirmacao
6. Selecionar Salvar. Esperado: toast de sucesso ("Jogo salvo." via GameSavedEvent). Em erro de IO: toast de falha.
7. Selecionar Carregar com save existente. Esperado: modal de confirmacao "progresso nao salvo sera perdido".
8. Cancelar (Esc) o modal de confirmacao. Esperado: nada carrega; volta a aba; nenhum estado destruido.
9. Salvar, alterar algo (andar/coletar), Carregar e confirmar. Esperado: estado retorna ao ponto salvo.
10. Em um save novo (sem arquivo), confirmar que Carregar aparece DESABILITADO ("Nenhum jogo salvo.").

### CA-6 — Recuperacao de save corrompido (backup rolling)
11. (Pre: corromper manualmente o `slot_1.json` de teste e ter um `slot_1.json.bak` valido.)
12. Carregar. Esperado: load falha; aparece oferta automatica "Save danificado - restaurar o backup de <data>?".
13. Aceitar. Esperado: carrega a partir do backup e entra no jogo; o arquivo corrompido NAO foi apagado.
14. Repetir sem backup. Esperado: mensagem clara "nao pode ser carregado / sem backup"; permanece no titulo/aba; nada destruido.

### CA-3 — New Game limpa estado
15. Sair para o Titulo (confirmar). New Game.
16. Esperado: mundo novo SEM inventario/ouro/tempo/quests/flags da sessao anterior.
17. Confirmar que `slot_1.json` anterior continua intacto em disco ate o primeiro Salvar explicito.

### CA-4 — Continue condicional
18. No titulo, com save existente: Continue aparece/habilitado; aciona o MESMO caminho de Carregar.
19. Apagar o save (ou primeiro boot): Continue desaparece/desabilita.

### Volume / Video (EMENDA v3 4.4)
20. Ajustar Master/SFX/Music (0-100). Esperado: persistem entre sessoes (PlayerPrefs) e afetam o AudioManager por canal.
21. Alternar Fullscreen/Windowed. Esperado: aplica via Screen.* e persiste.

## Wiring pendente (humano, Unity Editor)
- Vincular `SystemTabController` ao conteudo da aba Sistema no painel F14.
- Vincular `TitleScreenController` ao ponto de entrada de boot (decisao cena propria x overlay — ver report).
- Registrar as superficies de reset reais (`InventoryManager`, `EconomyManager`, `TimeManager`, `QuestRuntimeBootstrap`, flags) como `IResettableGameState` no `TitleScreenController` via bootstrap.

## Resultado
- [ ] CA-1 [ ] CA-2 [ ] CA-3 [ ] CA-4 [ ] CA-6 [ ] Volume/Video
