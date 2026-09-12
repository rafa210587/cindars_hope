# town_npc_visuals_navigation_v1 — execution report

Status: REVIEW_PENDING / PLAYMODE_VALIDATED. Não promover antes da revisão independente. A aceitação visual humana permanece separada.

## Acceptance criteria

| Critério | Evidência vigente | Status |
|---|---|---|
| N-1 — 29/29 visuais finais | `docs/validation/town_n1_npc_visuals_20260911.md`: 29/29; Corvus e Peregrino em folhas 5x5/25 frames, Point, PPU234, alpha e pivot corretos | PASS técnico; humano pendente |
| N-2 — 84 anchors alcançáveis | `Logs/n1-validator-acceptance-real.log`: 28 perfis, 84 anchors, grafo único 2.445/2.445 conectado e todo anchor representado | PASS no que mede — **ver ressalva N-2 abaixo** |
| N-3 — agenda acelerada com corpos reais | `Logs/n1-play-acceptance-real.xml`: 28/28 corpos canônicos percorrem fisicamente o trecho final de suas rotas no checkpoint acelerado 09h, sem recovery nem save write | PARTIAL: não reproduz as 24h inteiras |
| N-4 — porta real | Mesmo XML: `npc_alaric` usa `House_GateKeeper`, autoabre por proximidade sem `Interact`, entra/sai e não cruza o blocker enquanto fechado; blocker restaura | PASS |
| N-5 — corredor convergente | Mesmo XML: Alaric/Hund reais disputam a rota serializada para GateKeeper; penetração >0,02u não persiste >0,5s, Hund cede por ordem de `NpcId`, ambos retomam <=5s após abertura | PASS |
| N-6 — recovery limitado/observável | Mesmo XML/log: Pip real, obstáculo físico induzido, exatamente 2 replans pelo grafo, `reason/from/to`, max-step sem teleport, sem overlap final e hashes de save iguais | PASS |
| N-7 — validator e testes | Validator 24 PASS/0 FAIL; EditMode `Logs/n1-edit-acceptance-real.xml` 6/6; PlayMode `Logs/n1-play-acceptance-real.xml` 6/6 | PASS |
| N-8 — contratos preservados | Regeneração: 29 NPCs, 24 casas, 754 árvores, 23 stalls, 84 anchors, 2 spawns; PlayMode compara persistent files antes/depois | PASS no escopo |
| N-9 — load/offscreen separado | PlayMode usa serviço bootstrapado + `npc_zrix` real, rejeita onscreen, escolhe node seguro, exige `reason=offscreen_load_recovery/from/to` e save unchanged | NOT PROVEN para fluxo real de load: não existe caller de produção do seam |

## Mudanças relevantes

- Replan reconstrói caminho pelo `NpcTownRouteGraph` até o alvo; não repete movimento direto.
- Progresso é deslocamento físico em janela de 2s; máximo de dois replans permanece.
- `NpcWanderer` aplica direito de passagem determinístico: `NpcId` ordinal menor avança e o maior recua/cede.
- Bootstrap exclui o NPC `LegacyRetained` da agenda por regra de roster; continuam 29 visuais e exatamente 28 perfis agendados.
- Log offscreen contém NPC, motivo, origem e destino; não escreve save.
- Anchor `npc_velorin_social`: `(-6,4)` -> `(-8,4)`, node de rua adjacente transitável. TownScene regenerada pelo método canônico.

## Evidência fresca

- Unity 6000.5.7f1 compile: PASS, exit 0, `Logs/unity-compile-validation.log`.
- EditMode: `NpcTownRouteGraphTests`, 6/6 PASS, `Logs/n1-edit-acceptance-real.xml` e `.log`.
- PlayMode D3D: 6/6 PASS em 17,535s, `Logs/n1-play-acceptance-real.xml` e `.log`.
- Schedule/graph: 24 PASS/0 FAIL, `Logs/n1-validator-acceptance-real.log`.
- Regeneração: `Logs/n1-town-regenerate.log`; censo 29/24/754/23/84/2 preservado.
- TownScene SHA-256: antes `358CCE140E1CF99EC0DE1572BE53208A8C8D877077C2F687FFAC12D65752C0CF`; depois intencional `CAB401BE6DB841E406013C720719E0EA18B3A5B0D62E591C350ADC11C98FF8D1`.
- Inputs finais: `NpcWanderer.cs` `AE7E7F7D...E9DC`; `NpcScheduleService.cs` `367C349F...E0F8`; `NpcScheduleRuntimeBootstrap.cs` `BE52B307...0686`; harness PlayMode `1621589D...A15D`.

## Ressalva N-2 — alcançável não significa próximo (registrada em 2026-09-11)

O critério N-2 foi declarado PASS por conectividade: o grafo é um único componente e todo anchor
está representado. Isso é verdadeiro e insuficiente. Conectividade não mede **distância**, e a
medição posterior mostrou que o grafo serializado é praticamente uma **árvore** — 2.576 nós contra
2.577 arestas, quando uma árvore geradora precisa de 2.575. Sem ciclos existe exatamente um caminho
entre dois pontos, e âncoras entravam na malha por aresta única de 57–73u tendo nó de estrada a
0,00–2,58u de distância.

Consequência medida por `ValidateTownRouteDetourRatio`: **85 de 162** transições de agenda
percorriam caminho acima de 3× a distância em linha reta, pior caso 111,9× —
`npc_maelor_work → npc_maelor_home`, 2,58u reais custando 288u. Ou seja, NPCs atravessavam a cidade
para ir da oficina à própria casa a poucos metros, e todos os gates existentes ficavam verdes porque
nenhum comparava distância no grafo com a euclidiana.

Isso não invalida os testes de N-2; invalida a **leitura** de que N-2 provava circulação sã. A prova
de 24h e o checkpoint acelerado passavam porque exigiam chegada, não trajeto plausível.

Estado do conserto: `AnchorReentryRadius` em `CreateMvpTownScene` reduz para **10 de 162** em ensaio
sobre o grafo materializado (`Logs/n1-junction-g5.xml`), com pior razão 9,5×. Ainda **não validado na
cena** — exige regeneração da TownScene. O residual está delimitado ao distrito do curral
(`npc_hund`, `npc_gurd`), onde nenhuma curva viária autorada passa.

## Limites honestos

Os seis PlayMode cobrem rota longa, porta real, corredor real, recovery, checkpoint acelerado dos 28 perfis e seam offscreen com atores/colliders da TownScene. Ainda não há prova automatizada das 24 horas completas nem caller de produção de load/offscreen; por isso N-3 permanece parcial e N-9 `NOT PROVEN`, até decisão/revisão independente. Não é aceitação visual humana nem PASS global da TownScene.
