# SPEC — mundo: reveal da estufa e investigação visual do playtest

> Spec ID: spec_playtest_world_readability_v1
> Status: PARTIAL em 2026-09-27; reveal testado, Cave/circulação/escala/combate visual pendentes. Sem promoção.
> Type: Wiring/Validation; Domain: World/Farm/Cave; Priority: P1/P2; Decisões: D15–D19, D22.
> Ownership: CreateMvpFarmScene, reparo Editor focal e testes próprios. Unity e gravação de cena somente pelo coordenador.
> Depende de: specs canônicas de keyart e stable run; não as substitui. Bloqueia: aceite visual D21.
> Ordem de execucao: antes da cobertura integrada.
required_adrs: []
required_game_rules: []

# /speckit.specify
## Objetivo e Phase 0
CreateMvpFarmScene.CreateGreenhouse cria GreenhouseFloor com arte exterior opaca e quatro canteiros, mas não conecta RoofRevealController, já usado na casa. A exceção arável/colisões usa FarmSettlementPhysicsContract.GreenhouseInteriorBounds. Conectar o mesmo reveal nesse footprint sem recriar cena. D17 não possui medida comparável; D18 não comprovou caminhada bloqueada; D19 não mediu timing contínuo. Não alterar escala/colisores/hitboxes por hipótese. D15 retângulo claro ainda precisa identificação do renderer e captura equivalente. Documento amendment FASE9F referenciado em AGENTS está ausente; histórico absorvido em ref_cave_stable_run_replay_pr170_192.md não autoriza mudança de geração.

# /speckit.plan
## Escopo
Modificar CreateMvpFarmScene.cs para chamar GreenhouseRevealWiring.Configure(Transform, SpriteRenderer), em Assets/_Game/Scripts/Editor/SceneCreation/GreenhouseRevealWiring.cs. Reusar o mesmo método em runtime via `Assets/_Game/Scripts/Farm/Watering/GreenhouseRevealRuntime.cs` e `GreenhouseRuntimeHost.Awake()` para cobrir a cena já existente. API idempotente usa trigger na região útil em world space, RoofRevealController.Configure/ConfigureFeetOccupancy; mantém exterior enquanto fora e oculta ao entrar. Teste em Assets/_Game/Tests/EditMode/World/GreenhouseRevealWiringTests.cs valida feet, sensor, restauração, host da cena e ausência de duplicata. Não editar YAML, arte, footprint, câmera, geração, inimigos, loot ou balanceamento.

# /speckit.tasks
- [x] Reveal idempotente e pés dentro/fora sem sensor antecipar: GreenhouseRevealWiringTests (EditMode 29/29 da rodada final).
- [x] Host runtime confirmado na FarmScene existente por PlayMode headless 2/2; cena não salva. Tentativa de SaveScene em 27/09 foi revertida integralmente após auditoria detectar perda de tint em 12.541 células. `RepairActiveScene()` permanece para cenas de autoria futuras, sem invocação automática neste pacote.
- [ ] D16 Play: caminhar entrada/saída, arar/plantar/regar/colher; comparar leitura sem alterar produção.
- [ ] D15: mesma seed/área/câmera; identificar renderer do retângulo e textura; corrigir apenas causa comprovada em amendment desta spec; reentrada/hash igual. Enquanto não identificado: NÃO VALIDADO.
- [ ] D17: registrar ortho/PPU/altura ator em Farm e Town e pares de captura; seguir keyart aprovada, sem redimensionar por impressão.
- [ ] D18: FarmPhysicalRouteProbe + caminhada ponte nos dois sentidos; mapear graph_missing por NPC/cena, diferenciar falta de grafo de rota obstruída antes de editar.
- [ ] D19: 1×1/grupo com arma fixa; registrar antecipação/alcance/impacto. Não modificar tempo ou dano.
- [x] D22: checks executados; Farm 64/64 PASS, picaretas Tool sem erro WeaponId; validador geral FAIL por 3 erros de transição estática da Cave, registrados sem mascarar. Circulação jogada pendente.
## Evidência e falhas
Relatório integrado até 60 linhas, requisito→cenário→PASS/FAIL/NOT RUN, sem equivalência entre compile e aceite. Testes RoofReveal existentes preservados. Ausência de evidência visual mantém a spec em a_implementar. Esta spec permite executar investigação, mas não transformar hipótese em correção não fundamentada.
