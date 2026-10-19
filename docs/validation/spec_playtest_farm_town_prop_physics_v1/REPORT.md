# Física de props Farm/Town — execução parcial

Status: **PARTIAL**. EditMode 23/23 e PlayMode focal 3/3 PASS. Cena salva não foi gravada nem regenerada; caminhada e composição visual não foram aceitas.

## Auditoria da cena materializada antes da correção

- FarmScene: `Visual_SouthYardWestPost`, `Visual_SouthYardEastPost`, `Visual_HomesteadHay`, `Visual_HouseCrates` e `Visual_ShedCrate` aparecem só com Transform/SpriteRenderer, sem filhos físicos homônimos. `FarmSettlementVisualComposer.Prop` já cria `Solid_*` para várias caixas, fardos e cochos recentes; nenhuma duplicata foi acrescentada a essas instâncias.
- TownScene: `TownWell` e `GardenFence_00` aparecem sem sólido local. O último censo Town registrado tem hash `98CAD...`, enquanto a cena vigente tinha `8A6E...` na auditoria; seu PASS de 963 suportes medidos e 2.661 exceções não é evidência física vigente para poço/cercas/pontes. As pontes mantêm decks e intervalos de água autorados; não foi adicionado collider sobre os decks.
- Matinhos, arbustos, flores, grama, tapetes e água visual permanecem sem novos colliders.

## Mudanças de código

- `FarmPropPhysicsInstaller.Ensure(Scene)` instala uma base curta derivada dos pixels inferiores dos assets conhecidos (`fence` 79×64, `hay_bale` 58×64, `crate` 61×64) em cinco props específicos. Usa suporte já presente se três pontos inferiores estiverem cobertos por um mesmo sólido; é idempotente. `FarmSceneRuntimeBootstrap.Start` executa quando a cena está carregada e registra bases novas na máscara de solo não arável.
- `TownPropPhysicsInstaller.Ensure(Scene)` instala apenas bases de `TownWell` e `GardenFence_*` visíveis, salvo se já houver sólido cobrindo três pontos inferiores. `TownSceneRuntimeReferenceInstaller.Start` executa no carregamento da TownScene. Não altera NPC, portas, decks, água ou vegetação. A tentativa inicial no `Awake` falhou no PlayMode de cena salva; movida para `Start` para observar roots completos.
- `PlaytestPropPhysicsInstallerTests` cobre idempotência e ausência de collider em bush/bridge de teste: 2/2 PASS dentro da rodada combinada EditMode 23/23 (`TestResults/playtest-world-physics-editmode.xml`). `PlaytestPropPhysicsScenePlayModeTests` carregou as cenas salvas e passou **3/3** (`TestResults/playtest-prop-physics-playmode.xml`): três pontos das bases, 32 arbustos baixos da Farm sem Collider2D, vegetação Town sem Collider2D, casts do corpo real nos dois sentidos de duas pontes e corpo real livre no deck do cais Farm com três amostras de água adjacente bloqueadas.

## Gates

| Critério | Estado | Evidência/limite |
|---|---|---|
| AC01 Farm props e tint da cena | PARTIAL | Cinco bases efetivas PASS na FarmScene carregada; YAML não foi salvo. Hash SHA256 `9665E4E315EF520EA70C770A9D5FC8756C4A2930D695A485F07F71B97895286D` igual ao início da fatia, sem diff Git de cena; caminhada humana pendente. |
| AC02 poço/cercas Town | PARTIAL | TownScene carregada passou: poço e cercas com base efetiva; hash SHA256 `8A6E486BE6B13D27C653F51DE391EE4E272CDAD5A25BE40A3DB7FC4A8307A370` preservado, sem diff Git. Censo completo e caminhada humana pendentes. |
| AC03 pontes/cais | PARTIAL | Cast com corpo real passou nas duas pontes Town nos dois sentidos; no cais Farm, corpo real coube no deck e três amostras de água adjacente bloquearam. Cais/rails Town ainda não medidos. |
| AC04 vegetação baixa sem sólido | SCOPED PASS | 32 arbustos baixos da Farm e plantas Town examinados na cena real, zero Collider2D próprio/descendente; não é censo de toda vegetação do mapa. |
| AC05 rotas/interações | PARTIAL | `TestResults/playtest-npc-navigation-playmode.xml` 11/11 PASS cobre rotas de NPC; rotas gerais do player, interações e caminhada humana não foram reexecutadas. |

Unity EditMode: PASS, 23/23 na rodada combinada; testes próprios 2/2. Unity PlayMode focal: PASS 3/3 (`TestResults/playtest-prop-physics-playmode.xml`), exit 0, log scan PASS. N1 NPC PlayMode: PASS 11/11 (`TestResults/playtest-npc-navigation-playmode.xml`). O coordenador executou os runners de cena real. Residual risk: rotas gerais do player, cais/rails Town, interações e caminhada jogada não validados.

PlayMode anterior: uma rodada 0/2 identificou timeout `DeathSystemBootstrap` no `Temp/__Backupscenes/0.backup` antes de carregar a Farm e TownWell sem base, porque o installer estava no `Awake`. Os XMLs de mesmo nome foram substituídos pelo rerun final; não são citados como artefatos dessas falhas. Após mover installers para `Start`, um teste Town isolado novamente não alcançou asserções pelo log do backup. O teste agora declara `LogAssert.Expect` **apenas para essa mensagem exata no InitTestScene/backup**; todos os demais erros continuam visíveis e capazes de falhar o runner.

Rerun intermediário: 1/2, Town PASS, Farm FAIL por fixture que esperava três `Visual_SouthCourtBush_*` quando a cena salva tem zero. O seletor foi corrigido para as famílias existentes `Visual_MeadowLowBush_*` (27) e `Visual_PathGardenBush_*` (5); rerun final 2/2 PASS. A primeira falha permanece registrada como erro de fixture, sem ser apresentada como aprovação do mapa.

Próxima validação: probe de rotas gerais do player e interações, cais/rails Town e comparação de tint/alpha de Tilemap antes/depois; depois caminhada e revisão visual quando autorizadas. Os hashes das duas cenas permaneceram iguais nesta fatia. Se algum trajeto falhar, manter FAIL e ajustar a base, sem retirar árvores/props para fabricar PASS.
