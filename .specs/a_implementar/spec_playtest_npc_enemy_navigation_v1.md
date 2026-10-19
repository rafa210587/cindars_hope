# SPEC — navegação física de NPCs e monstros

> Spec ID: spec_playtest_npc_enemy_navigation_v1
> Status: PARTIAL — NPC PlayMode 11/11, Enemy EditMode 23/23 e parede real da Cave 1/1 PASS; movimento em Cave real/aceite humano pendentes.
> Type: Runtime/Validation
> Domain: NPC/Enemy
> Priority: P1/P2
> Ownership: `Assets/_Game/Scripts/NPC/**`, `EnemyMovementExecutor.cs`, `EnemyLocalAvoidance.cs`, testes focais e relatório próprio.
> Depends on: grafo N1 e física WorldSolid existentes. Blocks: aceite de circulação/combate do playtest.
> Depende de: grafo N1 e física WorldSolid existentes.
> Bloqueia: aceite de circulação e combate do playtest.
> Ordem de execucao: após auditoria D25, antes do aceite integrado de navegação/combate.
> Validation: PlayMode headless de física; EditMode para decisões puras; inspeção do diff.
required_adrs: []
required_game_rules: []

# /speckit.specify
## Objetivo e estado inicial
Investigar quatro riscos descritos em `docs/validation/WORLD_PHYSICS_MOVEMENT_AUDIT_2026_09_27.md`: fallback direto sem rota Town, idle wander contra obstáculos, avoidance ausente em movimentos especiais e raycasts sem largura do corpo. `NpcScheduleService.FixedUpdate()` apresenta ainda um defeito verificável de lifecycle: após o limite de replans, limpa o destino mas retém a ordem ativa; janelas seguintes repetem `Waiting at safe position`. A fixture N1 já testa o primeiro bloqueio, não a persistência. Não inferir defeito observado em cena apenas de leitura estática.

## Contrato e não escopo
- `TryGetMoveDiagnostic(npcId, out diagnostic)` continua disponível após um bloqueio definitivo para diagnóstico. A ordem bloqueada não deve publicar novos avisos nem tentar replan até uma nova ordem de agenda sobrescrevê-la.
- Caminho físico, posição e budget `NpcRouteRecoveryPolicy.MaxReplans` permanecem intactos; sem teleporte onscreen, save, tuning de combate ou alteração de cenas/geradores.
- Fallback direto, idle e enemy avoidance só mudam após probe headless provar falha com corpo/collider real. Charge/leap/dash são ações comprometidas e não devem receber steering genérico por hipótese.

# /speckit.plan
## Edições
1. `NpcScheduleService.MoveOrder`: incluir estado terminal de bloqueio. Em `FixedUpdate`, ignorar ordens nesse estado. Ao esgotar os replans, limpar o destino, registrar o único aviso e marcar a ordem terminal. Nova ordem sobrescreve esse estado naturalmente. Medir progresso contra o waypoint físico da rota, não o anchor final, pois um desvio válido pode se afastar dele. Na janela sem avanço de corner, reutilizar `NpcTownRouteGraph.SegmentIsPhysicallyReachable` para rejeitar falso progresso por deslizamento lateral contra parede.
2. `N1TownNavigationPlayModeTests.Recovery_RealObstacle_UsesBoundedReplansAndNoTeleport`: depois do primeiro aviso, esperar mais de uma janela e afirmar um aviso único, mesmo número de replans e corpo do lado seguro da parede.
3. `EnemyLocalAvoidance.SteerWithRaycast(..., Collider2D bodyCollider = null)`: usar cast do collider não trigger no whisker central, mantendo raycast anterior para chamadas sem corpo; sem alocações por frame.
4. `EnemyMovementExecutor.ApplyLocalAvoidance(Vector2)`: reusar a decisão para GroundChase e órbitas CircleStrafe/FloatingOrbit. Charge/leap/dash continuam linha comprometida; tuning e SOs intocados.
5. `NpcWanderer.MoveTowardIdleTarget()`: verificar o caminho com cast do collider sólido. Alvo inacessível retorna à pausa já configurada, sem ajustar velocidade/intervalos.
6. Probes headless para fallback sem grafo, idle behind wall, footprint largo e comportamento especial. Corrigir apenas reprodução positiva, sem ampliar esta spec por conjectura.

# /speckit.tasks
- [x] Identificar owners/cobertura existente e defeito de ordem retida.
- [x] Corrigir repetição de recuperação após bloqueio.
- [x] Ampliar teste PlayMode N1 para verificar ausência de novas tentativas/avisos.
- [x] N1 inicial 9/11; fallback e recovery PASS, idle fixture corrigida; segunda rodada conjunta revelou idle preso real e falso replan em detours Hund/Zrix (XML no relatório).
- [x] Autorados probes de rota sem grafo, footprint e movimento especial (execução pendente).
- [x] Autorado probe de idle wander atrás de obstáculo (execução pendente).
- [x] Idle reproduzido após fixture isolada (assert FAIL real em 100 FixedUpdates); correção por cast físico implementada. Próxima execução espera apenas o diagnóstico exato do bootstrap da cena temporária do runner.
- [x] Idle pós-correção PASS e AcceleratedAgenda PASS em N1 10/11; Recovery restante era fixture com parede anterior à rota, agora corrigida e aguardando nova execução.
- [x] N1 final 11/11 PASS, UnityExit=0, ScanUnityLogs PASS (`TestResults/playtest-npc-navigation-playmode.xml`, 21:35 local).
- [x] Probes Enemy falharam na primeira rodada (`IsAvoidingObstacle=false` em ambos); o XML do mesmo path foi sobrescrito pela validação pós-correção 23/23 PASS.
- [x] Corrigido cast do footprint e desvio de órbita no executor, sem alterar charge/leap/dash.
- [x] Enemy pós-correção EditMode 23/23 PASS, exit 0 (`TestResults/playtest-world-physics-editmode.xml`), incluindo os dois probes antes falhos e regressões existentes.
- [x] Probe Cave real autorado e executado: `CaveEnemyNavigationLayerPlayModeTests.GeneratedWallCollider_IsIncludedInEnemyWorldSolidQuery` compara collider `WallTile_*` gerado com a máscara `WorldSolid` configurada no EnemyBrain.
- [x] Probe Cave real FAIL antes da correção: `WallTile_24_17` em Default não aparece na query WorldSolid do Enemy (`TestResults/playtest-cave-enemy-layer-playmode.xml`, 0/1). Correção da materialização pertence à `spec_playtest_cave_wall_enemy_physics_v1`; esta fatia não altera Cave.
- [x] Probe de query Cave pós-correção 1/1 PASS, exit 0, log scan PASS (`TestResults/playtest-cave-enemy-layer-playmode.xml`).
- [ ] Física Player/Enemy e stable run mais amplos na `spec_playtest_cave_wall_enemy_physics_v1`; movimento natural de monstro ao redor de parede na Cave permanece sem observação.
- [x] Fallback sem grafo PASS com parede física, até o limite de replan; sem mudança runtime nesse caso.
- [x] N1 pós-correção idle/waypoint/parede tardia 11/11 PASS.
- [ ] Revisão e closeout da spec após testes e aceite de circulação/combate.

## Critérios binários
- `Recovery_RealObstacle_UsesBoundedReplansAndNoTeleport` PASS: exatamente um `Waiting at safe position` em três janelas após primeiro bloqueio; `Replans <= 2`; corpo não atravessa nem teleporta; save não muda.
- Diagnóstico permanece consultável após bloqueio; nova agenda substitui a ordem terminal.
- Movimento de rota que alcança waypoint mas se afasta temporariamente do anchor final não gera falso replan; ator sem avanço ao waypoint continua no budget de 2.
- Idle target fisicamente inacessível não mantém pressão contínua contra sólido; corpo permanece do lado seguro.
- Probes adicionais registram PASS/FAIL real, perfis e colliders usados; nenhum comportamento Enemy é alterado sem FAIL reproduzível.
- Status permanece PARTIAL enquanto Unity headless ou interação humana de percepção estiver pendente.
- Parede gerada real da Cave é consultável pela mesma máscara WorldSolid usada por `CaveEnemyMaterializer` para evitar obstáculos; teste headless de cena deve verificar o collider materializado, não um collider sintético.

## Casos de falha
NPC destruído remove a ordem; novo bloco de agenda substitui a antiga; retorno de interação não reativa ordem terminal. Grafo ausente continua comportamento legado até teste com obstáculo. Teste headless não equivale a aceitação visual/humana.
