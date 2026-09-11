# Validation Report — FarmScene reconstruída pela keyart

**Resultado técnico: SCOPED_PASS.** Unity6000.5.7f1. Evidência final: `regen_06` + `gameplay_05`. Aceitação estética/humana da fazenda pertence ao orquestrador/usuário e não é inferida destes gates.

## Acceptance criteria extracted

Regenerar FarmScene por Unity API; obter visão geral, seis regiões e câmera real; preservar PNGs anteriores, GUIDs e saves; importar três sprites GPT rastreáveis com Point/None/noMip; testar contratos e RoofReveal alterados; provar conectividade das aproximações usando o collider real. Seleção pela `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`: cena/geração/import + runtime focal + física. Não executar .NET, compile-only ou suite global redundantes.

## Existing systems audit

Reutilizados CreateMvpFarmScene, FarmSceneCapture, FarmPlayModeCaptureSession e RunUnityEditModeTests.ps1. Owner exclusivo Unity; nenhuma edição de runtime/gerador pelo validador, nenhum YAML manual. O executor corrigiu falhas reportadas. Baseline e saves foram preservados; Unity está fechado após a execução.

Backup anterior à reconstrução: `art/farm-pixelart-review/backups/20260908_232245-keyart-delivery/backup-manifest.json`, **191 arquivos verificados** e hashes de **140 PNGs World**. Cópia do manifest: [baseline-inputs.json](baseline-inputs.json). Capturas anteriores permanecem em before_diagnostic/ e before_gameplay/.

## Spec Compliance Matrix

| Gate | Resultado | Evidência vigente |
|---|---|---|
| Backup original | PASS, 191 cópias SHA256 iguais | [manifest](baseline-inputs.json) |
| Regeneração final | PASS, exit0 e nove PNGs | [processo](regen_06/process.json), [geral](regen_06/farm_capture.png) |
| Compile Editor | PASS por Unity final e Test Runner | [log regen](regen_06/unity.log), [log testes](focal-tests_02.log) |
| EditMode farm | PASS 329/329 | [XML](editmode_01.xml), [runner](editmode_01-runner.txt) |
| RoofReveal + Composition após alterações | PASS 6/6 (2+4) | [XML](focal-tests_02.xml), [runner](focal-tests_02-runner.txt) |
| Importação três PNGs | PASS raw=asset, RGBA, Point/None/noMip,128PPU | [hashes/settings/proveniência](import-validation.json) |
| 140 PNGs anteriores | PASS, zero alterações | [delta](generated-delta.json) |
| Câmera real | PASS 8/8,640x480,ortho8.5,exit0 | [metadata](gameplay_05/capture-metadata.json) |
| Rotas físicas | PASS 16/16,6833 nós | metadata acima; [resultado resumido](gameplay_05/validation-result.json) |
| Cena e saves durante captura | PASS, cena idêntica e saves0→0 | resultado acima |
| Erros runtime da sessão | PASS, zero | metadata acima |
| Scans finais | PASS, scan0 | [regen](regen_06/log-scan.txt), [gameplay](gameplay_05/log-scan.txt) |
| Fonte vigente | PASS, zero drift da captura final | [equivalência](gameplay_05/source-equivalence.json) |
| Inspeção artística regional | Orquestrador; capturas disponibilizadas | nove PNGs regen_06 e oito gameplay_05 |
| Docs consolidado/global | NOT RUN por este owner; orquestrador | não inferir GLOBAL_PASS |
| Input humano, ações, animação e aceite | NOT RUN | riscos abaixo |

Os seis casos focais incluem quatro Composition já contados nos329 anteriores; não somar335 casos distintos. Demais inputs farm pertinentes permanecem equivalentes. [Mudanças desde teste farm](changed-inputs-since-farm-tests.json) mostram gerador/captura/Composition; Composition foi reexecutado, gerador final e helpers foram exercitados no Unity. Correções posteriores de cliff/sorting alteraram somente composição visual; nova geração e captura foram feitas, sem repetir329 testes puros.

## Validation

### Execução e integridade

Comandos reais completos, horários, versão, hashes de fontes e exit codes ficam nos process.json de cada rodada. [Índice de rodadas](rounds-summary.json). Runner local verificável: [Invoke-FarmValidation.ps1](Invoke-FarmValidation.ps1).

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File docs/validation/playmode/farm_keyart_delivery_20260909/Invoke-FarmValidation.ps1 -Label regen_06 -Method CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture
powershell.exe -NoProfile -ExecutionPolicy Bypass -File docs/validation/playmode/farm_keyart_delivery_20260909/Invoke-FarmValidation.ps1 -Label gameplay_05 -Method CindarsHope.Editor.Dev.FarmSceneCapture.CaptureFarmGameplayBatch -AsyncPlayMode -TimeoutSeconds 300
```

Regeneração usa batch gráfico com quit; captura PlayMode usa batch gráfico sem quit. Cada log próprio recebeu exatamente um scan; Test Runner canônico incluiu seu scan, sem repetição. Warnings obsoletos CS0618 continuam visíveis nos logs, sem critical errors finais. Scan histórico gameplay02 falhou pela exception de landmark e está preservado.

Existing tests executed: YES
Test command: `tools/unity/RunUnityEditModeTests.ps1 -ProjectPath . -TestFilter 'CindarsHope.Tests.EditMode.Farm;CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests;CindarsHope.Tests.EditMode.Editor.FarmTerrainMaskTests' -ResultsPath docs/validation/playmode/farm_keyart_delivery_20260909/editmode_01.xml -LogFile docs/validation/playmode/farm_keyart_delivery_20260909/editmode_01.log -TimeoutSeconds 600`
Test evidence: `editmode_01.xml`, `editmode_01-runner.txt`
Test result: PASS329/329.

Focal command: mesmo runner com filtro `CindarsHope.Tests.EditMode.World.RoofRevealControllerTests;CindarsHope.Tests.EditMode.Farm.FarmSceneCompositionContractTests`, outputs `focal-tests_02.xml/log`, timeout600. PASS6/6.

Cena final SHA256 **C9796A69045C3C27CAFA23128DB14FED74B01701A6EC184B66C09256BCE1DF6E**, idêntico antes/depois da captura e ao arquivo entregue. Scene mudou somente por regen autorizado. Dentro dos191 inputs originais, oito mudaram (cena + sete fontes previstas); nenhum meta/tileasset original foi alterado. Três novos PNGs e respectivos metas foram importados; nenhum dos140 PNGs originais mudou.

Casa:1374x1145 RGBA; píer:1245x1263 RGBA; cachoeira:887x1774 RGBA. Raw e asset são byte-idênticos, com alpha0..255. DefaultTexturePlatform compression0; Standalone/WebGL override0 seguem Default. Manifest inclui prompts existentes e SHA256, sem alegar grid nativo64 nos PNGs de geração.

### Física realmente exercitada

Probe executado após bootstrap e primeira captura: BoxCollider2D real do player, dimensões0.640625x1.125u, offset(0,0.28125), escala2, máscara real-1. BFS cardinal em0.5u com OverlapCollider e Collider2D.Cast, tolerância de aproximação1.25u;6833 nós visitados. Dezesseis rotas possuem PASS e sequência de pontos na metadata:

casa, cultivo, poço visual, fonte, entrada cave, estufa, pesca, bancada, forja, cozinha, ponte, saída leste, animais oeste/leste, aproximação porta galinheiro e celeiro.

O teste prova conectividade física das aproximações solicitadas. Ele reposiciona o player em memória e consulta sweeps; não aplica input nem simula andar, interagir, animação, troca de cena ou gatilhos por locomoção. Porta/ponte têm aproximações físicas comprovadas, não um walkthrough humano.

Captura usa Main Camera real e CameraFollow2D em640x480/ortho8.5. SaveInput desativado somente em memória antes do PlayMode; nenhum save/load chamado; hashes da pasta gameplay saves0→0. Camera.Render não inclui UI Screen Space Overlay. Fonte final permaneceu idêntica ao início da execução.

### Ponte: alinhamento visual inspecionado

O frame gameplay04/bridge.png revelou player sobre água abaixo do deck, embora as16rotas físicas estivessem válidas. O executor preservou root/footprint e alinhou o filho Visual da ponte ao apoio do personagem, com sorting Ground. Frame final [bridge.png](gameplay_05/bridge.png) inspecionado diretamente pelo validador e confirmado pelo orquestrador: pés sobre madeira, personagem legível e ponte cobrindo o vão do rio. Player(27,2.5), câmera(26.53125,2.84375), ortho8.5: base projetada aproximada(333.24,249.71)px no render640x480. Esta inspeção de um frame é separada da conectividade física e não prova todas as poses da travessia.

### Falhas e iterações preservadas

- Regen01/02/03/04 tiveram exit0 técnico, mas o orquestrador identificou defeitos visuais (interior exposto, sorting/dock, cliff com grid errado e folhagem). Não recebem aceitação artística retroativa. Regen05 corrigiu sorting da folhagem; regen06 corrigiu alinhamento visual da ponte. Cada rodada mantém suas próprias imagens.
- Gameplay01:8/8 imagens,0runtimeErrors,saves intactos,exit1 porque JsonUtility reidratou Evidence null em objeto default e o guard impediu chamar o probe. Corrigido por flag persistida physicalRoutesEvaluated.
- Gameplay02:8/8 imagens,setup collider válido,exit1/scan1 porque target Workbench não correspondia ao nome real CraftingStation_Workbench. Três nomes corrigidos e todos os oito lookups nominais auditados antes do rerun.
- Gameplay03/04: PASS16/16 e8/8 sobre seus inputs; preservados. Gameplay04 revelou desalinhamento visual da ponte apesar do gate físico positivo; regen06 e gameplay05 materializam a correção e nova inspeção.
- Roof-tests01:1/2 FAIL pelo Assert ShouldRunBehaviour do Unity ao usar GameObject.SendMessage em EditMode. Fixture passou a invocar handlers por reflection mantendo transições/asserts; rerun6/6 PASS. Runner saiu antes do scan histórico por process exit2; scan dessa tentativa não foi executado.
- Regen01 teve drift de FarmPhysicalRouteProbe por targets adicionais escritos durante inicialização: [registro](regen_01/source-drift.json). Gerador/cena não mudaram naquele intervalo; execuções finais com fontes congeladas comprovam estado atual.

## Honest status rationale

**Validação técnica SCOPED_PASS**: compile, testes pertinentes, geração/import, oito vistas reais, dezesseis rotas físicas e integridade de saves/cena têm evidência vigente. Isso não é GLOBAL_PASS, build Player/IL2CPP, aceite humano ou demonstração de naturalidade das novas animações. As quatro falhas farm históricas foram superadas no filtro pertinente329/329; demais falhas globais/wiring antigas não foram reexecutadas nem declaradas corrigidas.

Residual risk: leitura em outras resoluções, desempenho jogável, interação completa com casa/estações/transições, input, animações e aceitação artística exigem avaliação adicional. Nenhuma integração das12poses do laboratório ou geração final de todos os tiles foi inferida destas capturas.


