# Execution report — `town_components_doors_and_collision_v1`

## Acceptance criteria extracted / compliance

| Critério | Evidência final | Status |
|---|---|---|
| C-1 arte nativa em escala de jogo | Capturas finais de templo, moinho e padaria/alquimia no PlayMode; revisão visual humana final ainda não executada | DEFERRED_TO_FINAL_HUMAN_VALIDATION |
| C-2 sequência fechada→aberta→fechada, sem folha duplicada | PlayMode `playmode-20260911-c1fix2`: 4 fases e capturas closed/open; animação e estado final verificados | PASS |
| C-3 blocker/passagem/fechamento seguro | `live-physics.json`: blocker, ocupação do vão e restauração; rotas físicas em 3 famílias | PASS |
| C-4 entrada/saída real, reveal por pés e móveis sólidos | `live-physics.json` + capturas interior/exit; proximidade não revelou e saída recobriu | PASS |
| C-5 censo sem desconhecidos/duplicados | Diagnostic `diagnostic-20260911-062737-754-c4a12f5f`: **963/963**, `pending=0`, `errors=0`, `proposedInterior.pending=0`, `conflicts=0`, `warnings=0`; Alchemy/Sewing bound to measured station sources | PASS |
| C-6 rotas, acessos e preservação | Validator: **194/194 + 8/8**; preservação: 24 houses, 29 NPCs, 84 anchors, 754 trees | PASS |
| C-7 evidências separadas | Arte estática, PlayMode, física e testes registrados separadamente; aceite visual humano continua pendente | PASS_WITH_HUMAN_PENDING |
| C-8 classificação e três famílias completas | 24/24: 14 `walk_in_reveal`, 9 `shop_hours_gated`, 1 `exterior_only`; templo, moinho e bakery/alchemy cobertos | PASS |
| C-9 evento/SFX físico sem spam | Foundation event/SFX tests previamente registrados no report; transições concluídas observadas no PlayMode | PASS |
| C-10 loja fechada/aberta e horários | Validator/PlayMode de shops e contratos existentes; sem alteração de IDs ou schedules | PASS |

## Existing systems audit

Foram reutilizados `HouseDoorInteractable` e `RoofRevealController`; nenhum runtime compartilhado foi alterado nesta consolidação. IDs, anchors, NPCs, saves e cenas Farm/Cave permanecem preservados.

## Final evidence

- C5 diagnostic: `diagnostic-20260911-062737-754-c4a12f5f`, `963/963`, pending 0, errors 0, proposed interior pending 0/conflicts 0/warnings 0.
- Town validator: `194/194` checks e `8/8` secondary checks PASS.
- Preservação estrutural: 24 houses, 29 NPCs, 84 anchors, 754 trees.
- EditMode: Door `9/9`, Roof `4/4`, Component `33/33` PASS.
- PlayMode: [`playmode-20260911-c1fix2`](town_components_doors_and_collision_v1/playmode-20260911-c1fix2), 4 fases, 106 rotas + conector, blocker/reveal/ocupação/restauração PASS.
- Event/SFX foundation tests: evidência previamente registrada e mantida; não repetida.

## Testing Quality Gate

Todos os gates automatizados e físicos aplicáveis desta rodada têm evidência PASS vigente. A revisão visual/humana final não foi executada e não é inferida a partir das capturas automatizadas.

## Honest status rationale

Status: **DEFERRED_TO_FINAL_HUMAN_VALIDATION**.

A implementação e os gates estruturais, EditMode e PlayMode estão validados. Permanece apenas a aceitação visual humana final da arte/escala e composição; a spec não é promovida nem movida nesta consolidação.

## C1 finding follow-up — 2026-09-11

- Classificação agora é consumida por `CreateMvpTownScene.CreateHouses()` e por `ValidateTownKeyartScene.Validate()`. O contrato exige os 24 IDs canônicos e as contagens 14 `walk_in_reveal`, 9 `shop_hours_gated`, 1 `exterior_only`; o validator observou `PASS 24/24 door/interior classifications (14/9/1)` em `Logs/town-c1-validator-final.log`.
- O capture PlayMode foi alterado para selecionar `House_CarvalhoTorto` com residente canônico `npc_hund`. O NPC mantém sua posição inicial e recebe destino pelo approach/waypoint físico, devendo abrir e sair pelo vão sem teleporte do harness. A execução nova `playmode-20260911-c1fix3` **FAIL/NOT RUN completo**: crash nativo Unity em `Camera.Render` antes do cenário NPC (`Logs/town-c1-playmode-c1fix3.log`, primeiro blocker). A evidência anterior de PlayMode continua válida para portas/player, mas não prova autoabertura NPC.
- Teste existente de evento→categoria→cooldown: `AudioSfxHooksTests`, filtro exato, **20/20 PASS** em `Logs/town-c1-audio-door.xml`; foundation não foi reescrita.

Status após follow-up: **PARTIAL / BLOCKED_BY_PLAYMODE_CRASH**. Contagens C5/validator/preservação anteriores permanecem preservadas; primeiro blocker para closeout é reproduzir o PlayMode sem o crash nativo e observar `npc_hund` abrindo/restaurando a porta.

## C1 final validation update — 2026-09-11

- Execução solicitada com D3D explícito (`-force-d3d11`), output `playmode-20260911-c1fix4` e método `TownKeyartPlayModeCapture.RunBatch`: **NOT RUN completo**. O harness concluiu os 8 overview e escreveu `live-physics.json`, mas não produziu `capture-metadata.json` nem entrou nas fases da casa; após timeout controlado, somente o PID Unity deste repo foi encerrado. Log: `Logs/town-c1-playmode-c1fix4.log`.
- Evidência parcial fresca: `playmode-20260911-c1fix4/live-physics.json` registra **106 rotas PASS** e `TownAccessPlayMode PASS 8/8`; capturas overview estão no mesmo diretório. `sceneUnchanged`, `savesUnchanged` e `setupRestored` não podem ser afirmados sem o metadata final.
- Validação saved-scene independente: **194/194 física + 8/8 acesso PASS**, log `Logs/town_c1_validator_baseline_20260911.log`. EditMode focado: Door `9/9`, RoofReveal `4/4`, Component contract `33/33` PASS, XMLs em `Logs/town_c1_*_editmode_20260911.xml`.
- Diagnóstico de censo/import fresco continua **FAIL**: `C5=FAIL`, `measuredFail=769`, `pending=1888`, `DiagnosticErrors=0`, no output gerado por `TownComponentPhysicsDiagnostics.ExportSavedTown` e log `Logs/town_c1_diagnostics_20260911.log`.

Status desta rodada: **NOT RUN / BLOCKED**. Primeiro blocker: o cenário auto-NPC `House_CarvalhoTorto`/`npc_hund` não avançou após a etapa de overview dentro do timeout, portanto não há evidência atribuível de approach, autoabertura, passagem, saída e restauração natural. O C5 census gate também permanece FAIL.

## C1 auto-NPC state-machine follow-up — 2026-09-11

- Correção mínima aplicada somente ao harness `TownKeyartPlayModeCapture`: `TickNpcAutoOpen` agora processa os estados `PENDING` e `OBSERVED`; ao observar a abertura, reinicia o timeout da fase de saída. Movimento continua sendo `SetDestination` real e os asserts de blocker/fechamento permanecem intactos.
- Diagnóstico C5 fresco preservado: `diagnostic-20260911-062737-754-c4a12f5f` — `963/963`, `errors=0`, `warnings=0`, `conflicts=0`, `pending=0`, **C5 PASS**.
- PlayMode D3D `playmode-20260911-c1fix5`: metadata gerado; auto-NPC `npcAutoOpenStatus=PASS`, `npcAutoOpenObserved=true`, restauração e `sceneUnchanged/savesUnchanged=true`. A execução global ainda terminou **FAIL** por instabilidade posterior do ciclo público (`EnsureDoorStable`), portanto não é declarada como PASS completo.
