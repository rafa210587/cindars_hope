# SPEC — pesca: janela, lifecycle e feedback do playtest

> Spec ID: spec_playtest_fishing_lifecycle_v1
> Status: UNITY_VALIDATED em 2026-09-27 (EditMode integrado 166/166 e regressão final 35/35); pesca jogada pendente. Sem promoção.
> Type: Runtime maintenance; Domain: World/Fishing; Priority: P2; Decisão: D20.
> Ownership: World/FishingSpot e testes próprios; sem Unity concorrente, validação integrada pelo coordenador.
> Depende de: nenhuma mudança de balanceamento; Bloqueia: aceite de pesca do roteiro D21.
> Ordem de execucao: antes da cobertura integrada.
required_adrs: []
required_game_rules: []

# /speckit.specify
## Objetivo e Phase 0
FishingSpot já entrega feedback por PlayerActionFeedbackEvent e possui v1/v2. O delay inicial deixa _windowOpenTime da tentativa anterior; TryConfirmFishing não verifica abertura e pode premiar antes da mordida. Rotina continua após confirmação prematura. Não existe reset ao desativar nem cancelamento quando ator sai da zona. Corrigir o contrato existente, mantendo stamina 20 e tempos serializados. Fonte: FishingSpot.cs, leitura 27/09.

# /speckit.plan
## Escopo e contrato
Modificar somente Assets/_Game/Scripts/World/FishingSpot.cs e criar Assets/_Game/Tests/EditMode/World/FishingLifecycleTests.cs. Reusar minigame, resolver, inventário e eventos atuais. Adicionar flag de janela pronta; confirmação antes de pronta retorna sem prêmio. Início limpa flag; rotina revalida sessão após delay; timeout/confirm/reset limpam flag. OnDisable encerra coroutines e estado. Guardar ator de lançamento para cancelar ao sair da área válida. Prompt projeta Pescar/Aguarde/Puxar sem criar painel. Mensagens de feedback em português. Falha de inventário mantém comportamento vigente, sem gerar item duplicado. Não alterar resolver, tabelas, custos, janela, loot ou schema.

# /speckit.tasks
- [x] Confirmação antes da mordida não encerra sessão nem entrega item: EarlyConfirm_DoesNotAwardOrEndCast.
- [x] Desativação limpa tentativa/janela e prompt: Disable_ClearsCastState.
- [x] Zona perdida cancela a tentativa: LeavingStance_CancelsCast; timeout também limpa janela (Timeout_ClearsBiteWindow).
- [x] Testes existentes FishingV2Tests/FishingStanceZoneTests continuam PASS.
- [ ] Play: cast→espera→mordida→captura; timeout; sair durante espera; desativar/reativar; inventário cheio. Mesmos custos/janelas.
## Validação e risco
EditMode + PlayMode integrado; escrita do cenário não conta como execução. Unity executado pelo coordenador, nenhum resultado pré-declarado. Risco: interrupção ao abrir UI usa tempo escalado como antes, sem mudar política global. Status de aceitação permanece pendente até execução integrada.
