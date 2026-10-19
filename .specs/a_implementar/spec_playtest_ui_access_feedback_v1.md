# spec_playtest_ui_access_feedback_v1

Status: UNITY_VALIDATED em 2026-09-27 (EditMode 166/166 e 29/29); foco/layout jogados pendentes. Sem promoção.
Tipo: manutenção UI/runtime; prioridade P1/P2; domínio D04–D14 do refinement de playtest.
Ownership: UI (exceto Death), Core/MenuGuiStyle, Fonte/FonteInteractable, World/FarmHouseChestInteractable, Localization, Magic/SpellItemUseController e linha de instalação no composition root. Save/schema/crafting domain/scene YAML proibidos. Root executa validação Unity integrada; não executar concorrente.
Ordem de execucao: antes da cobertura integrada.
Depende de: owners existentes de modal, UI e localização.
Bloqueia: aceite visual e de navegação do pacote integrado.
required_adrs: []
required_game_rules: []

# /speckit.specify

## Phase 0 real
Leitura em 27/09: GameplayInputRouter publica PauseOpenedEvent sem subscriber; ModalManager é exclusão de um modal, não stack aninhável. FonteInteractable abre IMGUI sem push/pop; ModalPauseGate documenta pausa em qualquer menu. BuyPanel.RefreshItemStock não atualiza detalhe selecionado. CraftingModal ignora retorno de sucesso e apresenta IDs. DebugHud lê V durante campo de loja. SpellItemUseController usa J, reservado ao QuestLogRuntimeBinder, e cast callback está null. Chest é stub.

## Contratos e implementação
# /speckit.plan
- Criar PauseMenuController Canvas, instalado idempotentemente no root; assinar PauseOpenedEvent/ModalCloseRequestedEvent e liberar modal/token no disable. Open somente se ModalManager aceitar Pause. Close só libera sua posse. Usar GameTimeScaleCoordinator para congelar simulação. Router em ordem antecipada evita Escape fechar painel legado e abrir pausa no mesmo frame. Skill ui-modal-stack; nenhuma busca global.
- FonteInteractable entra/sai no ModalType.AnyaFountain; fechamento pelo evento central quando router ativo e fallback Escape sem router. Não revelar funções seladas nem enum técnico. Close idempotente inclusive disable.
- DebugHud bloqueia atalhos quando modal ativo ou campo de texto focado. Atalho legado J de scroll deixa de consumir itens; uso pela UI requer contrato funcional, sem inventar novo binding. Preservar APIs públicas de uso e falhas sem consumo.
- BuyPanel mantém seleção autoritativa e reprojeta detalhe após transação; layout reserva feedback com wrap e evita lista cobrir detalhes. Não mudar transações.
- CraftingModal mostra sucesso imediato com output/quantidade, início temporizado e cancelamento/coleta; resolve nomes via catálogo existente e texto via localização. Botões mouse e teclado chamam mesmo owner. Não alterar custos, job, tempo ou save.
- MenuGuiStyle usa fundo opaco e wrap; equipamento resolve nomes pelo catálogo e remove stats fictícios. Baú anuncia indisponibilidade antes de interação. Diálogo fecha via Escape consistentemente. Preservar narrativa.

## Aceites binários e evidência
# /speckit.tasks
AC1 Escape mundo abre Canvas Pause; retomar/Escape restaura timeScale; modal existente fecha sem abrir pausa. Teste PauseMenuControllerTests + PlayMode integrado.
AC2 Fonte push/pop e disable restauram HasActiveModal; relógio pausado por gate canônico; sem enum na UI.
AC3 Ctrl+V em quantidade não abre DebugHud; J abre quests sem consumir scroll. Inspeção e PlayMode.
AC4 Compra atualiza estoque na lista e detalhe da mesma seleção; feedback em área com wrap. Teste/PlayMode última unidade e saldo insuficiente.
AC5 Craft imediato mostra quantidade produzida; temporizado mostra trabalho iniciado; falha preserva motivo. Crafting/Shop regressões existentes e PlayMode.
AC6 Textos novos em LocalizationStringTable com fallback existente preservado; teste PlaytestUiLocalizationTests.
AC7 Comparação visual em 1280×720 e 1920×1080: contraste, áreas de texto, navegação e ausência de IDs nos campos alterados. Aceite visual não presumido por compile.

## Limites e continuidade D08/D11–D14
Este pacote corrige superfícies existentes; não cria armazenamento, título completo, bindings de magia novos, ícones/arte, filtros ou redesign integral. Magia com hit/obstáculos, transações avançadas de inventário, revisão completa de skills/HUD e apresentação de diálogos ficam critérios de cobertura integrados, honestamente NOT RUN até exercício. D08 cast scroll runtime permanece indisponível quando callback ausente; não apresentar sucesso falso.

## Falhas/lifecycle
Sem bootstrap/modal owner: não abrir pausa/fonte. Repetição de open não duplica tokens. Close/disable libera somente o próprio modal. OnDisable remove listeners. Ingredientes/estoques/nome ausentes usam falha visível, nunca inventam resultado. Nada promove status antes dos gates.

## Implementação entregue e roteiro de integração
- D04: Canvas de pausa instalado; três volumes e alternância de tela cheia reutilizam SystemTabController/GameAudioSettings. Título/novo/continuar não são criados incidentalmente; boot existente continua por cena. Tecla Escape roteada antes dos painéis legados.
- D05–D08: debug ignora modal/InputField; J reservado a quests; fonte toma/libera modalidade; baú anuncia indisponibilidade; inventário só oferece Equipar quando existe slot compatível. CastScroll sem callback continua indisponível, preservando itens.
- D09–D10: detalhe da compra reapresenta estoque após sucesso; loja reserva feedback e detalhes fora da lista; crafting exibe resultado real, separa andamento/pronto/ocioso e aceita clique nas mesmas ações de teclado.
- D11–D14: base IMGUI opaca e wrap; equipamento usa nome de catálogo e ausência explícita de resumo; skills mostram nomes de requisitos/ações; inventário mostra rótulos de mãos/ações e motivo de uso indisponível; HUD projeta ambas as mãos; quests vazias orientam conversa; diálogo permite Escape com escolhas e wrap/altura mínima dos botões.
- Correção associada D22: EquipmentManager.InferToolTypeFromId testa pickaxe antes de axe; não muda IDs.

Testes adicionados: PauseMenuControllerTests.OpenRepeated_CloseEvent_ReleasesExactlyOnePause, Disable_ReleasesPauseWithoutReleasingOtherOwner, MissingModal_DoesNotFreezeSimulation; PlaytestUiLocalizationTests (chaves, parâmetros, fallback, material ausente); EquipmentToolInferenceTests.Pickaxe_IsNotInferredAsAxe.

Política de tempo/foco: Pause congela simulação por token e relógio por ModalManager; Fonte/inventário/atributos/equipamento/skills/quests/loja/craft usam modal para relógio/foco; prompts de proximidade não pausam. Pesca/crafting fora de menu preservam comportamento de domínio. ModalManager admite somente um modal: transições pai/filho existentes continuam liberar→abrir, sem introduzir stack incompatível.

Mockup implementado: pausa central Canvas 520×420 em referência 1280×720, título no topo, hint, três linhas de volume com +/- e ações tela cheia/retomar; loja reserva 300px abaixo/acima da lista para detalhe/feedback/rodapé; crafting central 620×430 com lista scroll135px, materiais/estado/ações/feedback; demais painéis preservam geometrias e usam fundo opaco. Captura/playback em resoluções alvo ainda é gate de integração.

Implementação concluída; compile/EditMode integrados passaram. Navegação e inspeção visual seguem pendentes. Nenhum YAML, save/schema, arte ou balanceamento foi alterado por esta fatia.

### Follow-up de revisão
ModalCloseRequestedEvent agora carrega TargetModal capturado pelo router; consumidores ignoram alvo diferente, preservando construtor vazio para chamadas legadas. Isso impede fechar pai reaberto pela seleção de equipamento no mesmo broadcast. PauseMenuController resolve EventSystem.current e materializa fallback filho do Canvas somente se ausente; retomar recebe seleção inicial e close limpa foco próprio. Canvas fechado desativa seu fallback, sem busca global nem alterar EventSystem da cena. Teste CloseRequestForInventory_DoesNotCloseNewPauseOwner cobre isolamento; fixtures EditMode acionam OnEnable/OnDisable explicitamente porque callbacks não rodam automaticamente fora de Play Mode.

### Evidência parcial verificada
Unity compile PASS na rodada integrada. XML `TestResults/playtest-corrections-editmode.xml` de 27/09: PlaytestUiLocalizationTests 9/9 PASS e EquipmentToolInferenceTests 1/1 PASS; fixture PauseMenuControllerTests corrigida para InvokeLifecycle por reflection após falha de execução de callbacks em EditMode (rerun pendente). Testes não certificam screenshots, mouse/teclado real ou aceitação humana; status de integração continua pendente.

### Resultado integrado e revisão D14
Rodada integrada `TestResults/playtest-corrections-editmode.xml`: 166/166 PASS, incluindo os 14 testes novos de pausa, localização e inferência de ferramenta. Após essa rodada, D14 recebeu viewport com máscara/scroll independente para fala e escolhas: rolagem de fala por roda/PageUp/PageDown; navegação das escolhas reposiciona scroll. Isso preserva as áreas originais do painel e impede texto longo invadir escolhas. `DialogueOverflowTests.LongSpeech_UsesMaskedScrollableContentWithoutMovingChoicesArea` verifica máscara, área de60px e idempotência; novo teste/compile e PlayMode ainda pendentes dessa revisão. Não reexecutar 166 sem mudança que justifique.
