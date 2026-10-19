# Validation Report — continuação artística 01 da FarmScene

**Validação técnica: SCOPED_PASS.** Estado artístico: revisão humana pendente; a rejeição humana da entrega anterior permanece histórica. Gates técnicos não demonstram equivalência à keyart nem substituem o aceite do usuário.

## Acceptance criteria extracted

Regenerar e capturar a nova composição de montanha, árvores, grama, água e margens; preservar IDs existentes e arquivos anteriores; importar seis PNGs GPT com proveniência/settings; validar o novo opt-in de tint do TreeNode e o planner; renovar oito vistas da câmera real e dezesseis rotas físicas após reposicionamento das árvores.

Seleção pela `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`: geração/import/cena, runtime focal TreeNode e conectividade física. Não repetir .NET, compile-only ou suíte global quando a execução Unity e os testes pertinentes já fornecem evidência. Inspeção artística conduzida pelo orquestrador antes da liberação de Play Mode.

## Existing systems audit

Reutilizados CreateMvpFarmScene, WorldTilemapGround, FarmSceneCapture, FarmPlayModeCaptureSession, FarmPhysicalRouteProbe e RunUnityEditModeTests.ps1. O executor adicionou o compositor visual Editor; o validador não editou C#, PNG ou YAML. Exclusividade Unity respeitada e editor fechado ao terminar.

Backup pré-regen: `art/farm-pixelart-review/backups/20260909_001359-continuation-art-01/backup-manifest.json`,344arquivos verificados. [Manifest local](baseline-inputs.json). O snapshot contém145PNGs:143 do estado entregue anterior e dois candidatos cliff/reeds já copiados antes do pedido ao validador. Gerador/planner/Composition já estavam em edição nesse momento; este backup NÃO é apresentado como snapshot pré-edição de todos os arquivos. A cena C9796A69... e capturas before_diagnostic/before_gameplay são anteriores à regeneração desta continuação.

## Spec Compliance Matrix

| Gate | Resultado atual | Evidência |
|---|---|---|
| Backup pré-regen | PASS344 arquivos SHA256 verificados | [baseline](baseline-inputs.json) |
| Regeneração final | PASS, exit0;10imagens | [processo](regen_04/process.json), [geral](regen_04/farm_capture.png), [enquadramento fixo](regen_04/farm_capture_keyart_composition.png) |
| Compile Editor | PASS via Unity e Test Runner | [log final](regen_04/unity.log), [runner](planner-tests_02-runner.txt) |
| TreeNodeVisual | PASS3/3, reuso por equivalência | [XML focal](focal-tests_01.xml) |
| Composition | PASS4/4, reuso por equivalência | XML focal acima |
| Planner após correção | PASS4/4 | [XML rerun](planner-tests_02.xml), [runner](planner-tests_02-runner.txt) |
| Total focal distinto válido | PASS11 casos (3+4+4) | não somar15 por sobreposição dos reruns |
| Importação seis novosPNG | PASS técnico raw=asset, Point/None/noMip/128PPU | [manifest de validação](import-validation.json) |
| Reuso dos imports/testes | PASS zero drift pertinente | [equivalência final](final-input-equivalence.json) |
| PNGs anteriores | PASS145/145 idênticos ao snapshot | [delta](generated-delta.json) |
| IDs de árvores existentes | PASS50 índices anteriores e TreeData GUIDs retidos;21 adicionais | [inspeção da cena](scene-identity-review.json) |
| Homestead materializado | PASS19decorações dentro5..48 | inspeção acima |
| Captura gameplay | PASS8/8,640x480,ortho8.5 | [metadata](gameplay_01/capture-metadata.json) |
| Rotas físicas | PASS16/16,6529nós | metadata acima e [resultado](gameplay_01/validation-result.json) |
| Cena/saves e fonte vigente | PASS, zero alteração durante captura e zero source drift | resultado acima |
| Runtime errors | PASS, zero | metadata acima |
| Scans finais | PASS; cada log recebeu seu scan uma vez | [regen](regen_04/log-scan.txt), [Play](gameplay_01/log-scan.txt) |
| Inspeção visual do orquestrador | Executada nos10diagnósticos e8vistas; melhoria material confirmada | imagens citadas; não equivale a aceite humano |
| Docs consolidado | NOT RUN por este owner; responsabilidade do orquestrador | sem GLOBAL_PASS |
| Input/ações/animação e aceite humano | NOT RUN | limites abaixo |

## Validation

### Comandos e inputs

Unity6000.5.7f1, processo gráfico sem nographics para capturas. Método síncrono de regen usa quit; captura Play Mode assíncrona não usa quit. Entradas, comandos completos, horários, hashes e exit ficam em cada process.json. Runner usado: [Invoke-FarmValidation.ps1](Invoke-FarmValidation.ps1).

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File docs/validation/playmode/farm_keyart_delivery_20260909/continuation_art_01/Invoke-FarmValidation.ps1 -Label regen_04 -Method CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture
powershell.exe -NoProfile -ExecutionPolicy Bypass -File docs/validation/playmode/farm_keyart_delivery_20260909/continuation_art_01/Invoke-FarmValidation.ps1 -Label gameplay_01 -Method CindarsHope.Editor.Dev.FarmSceneCapture.CaptureFarmGameplayBatch -AsyncPlayMode -TimeoutSeconds 300
```

Existing tests executed: YES
Test command: `tools/unity/RunUnityEditModeTests.ps1 -ProjectPath . -TestFilter 'CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests;CindarsHope.Tests.EditMode.Farm.FarmSceneCompositionContractTests;CindarsHope.Tests.EditMode.World.TreeNodeVisualTests' -ResultsPath docs/validation/playmode/farm_keyart_delivery_20260909/continuation_art_01/focal-tests_01.xml -LogFile docs/validation/playmode/farm_keyart_delivery_20260909/continuation_art_01/focal-tests_01.log -TimeoutSeconds 600`
Test evidence: `focal-tests_01.xml`, `focal-tests_01-runner.txt`, `focal-tests_01-inputs.json`.
Test result: FAIL10/11 inicial; TreeNode3 e Composition4 válidos e reutilizados por equivalência comprovada.

Rerun somente `CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests` pelo mesmo runner, outputs `planner-tests_02.xml/log`, timeout600: PASS4/4,process0,runner0,scan integrado PASS. Fonte/testes de TreeNode e Composition mantiveram hashes; não repetir todos os onze sem mudança. [Equivalência](final-input-equivalence.json).

### Falhas e iterações preservadas

- Regen01: exit0 técnico, mas o orquestrador reprovou margens em degraus e o topo azul do rio acima da cascata. Nenhum Play Mode/teste foi usado para aprovar a arte dessa rodada.
- Regen02: exit0 técnico, dez imagens com novo enquadramento; orquestrador identificou bordas cruzando junções, cap visual do rio e minérios posicionados na face da montanha. Executor corrigiu as três questões antes do smoke.
- Regen03: exit0 técnico; inspeção confirmou junções, cap e minérios corrigidos. Teste focal falhou Homestead com apenas3decorações na cena, abaixo do mínimo5. O erro, XML e process exit2 permanecem preservados. Runner retornou antes de seu scan por falha do processo; log scan dessa tentativa é NOT RUN, não PASS.
- Executor acrescentou jardins determinísticos respeitando clearance/IsForbiddenCell; limites5..48 ficaram intactos. Regen04 materializou19decorações Homestead; Planner rerun4/4 PASS.
- Gameplay01 atual PASS8/8 e16/16; não há Play Mode intermediário desta continuação apresentado como estado final.

### Imports, cena e enquadramento

Seis PNGs novos: cliff, reeds, lily_pads, ground_water, shore_bank, ground_grass_v2. Raw e asset são byte-idênticos; quatro props RGBA preservam alpha, água/grama RGB são bases opacas. Point0, mip0, DefaultTexturePlatform compression0,128PPU; cliff e shore max4096 local, demais2048. Overrides de plataforma inativos seguem Default. Manifest registra hashes de arquivo/meta e resumos de prompt, identificados honestamente como resumos. Script de leitura reproduzível: [validate_imports.py](validate_imports.py); nenhum raster foi editado por ele.

Nova grama1254/128 cria células visuais9.796875u. Floor/ceil do PaintRect produz48células e bounds visuais78.375x58.78125 centrados(0,0), excedendo a cobertura solicitada72x52. O overview automático passou de ortho28.5 para31.4; câmera de gameplay não mudou. O novo diagnóstico fixo1600x1000/ortho22/centro0 oferece comparação sem esse efeito. [Análise geométrica](ground-bounds-review.json). Aumentar o chão visual não amplia grid de gameplay ou colisão.

Todos os145PNGs do snapshot ficaram intactos;14arquivos World posteriores são listados no delta, incluindo PNGs/metas ainda ausentes e dois Tile assets/metas de água/grama gerados pela API. Seis PNGs são novos em relação à entrega humana anterior, pois dois já estavam no snapshot desta preparação. Não confundir as bases de comparação.

TreeNodes:50→71(68do bosque+3especiais). Todos os50 índices anteriores e suas referências TreeData persistem;21 índices antes ausentes foram materializados. A composição muda posições, mas não renumera índices existentes. HealthyTint branco está serializado nos71 TreeNodes Farm; comportamento legado de outros callers coberto pelo teste focal.

Cena final SHA256 **7E2FFD6919591E634444D943AE8EB26D1EFF4AECA886A8F03F0B8CBE60A2EA2C**. Captura preservou exatamente esse hash e os saves0→0. Zero drift das fontes ao conferir após execução; importação anterior reutilizada com hashes atuais iguais.

### Play Mode e interpretação da prova

Oito vistas: spawn,bridge,cultivation,homestead,forest,mountain,animals,lake. Main Camera e CameraFollow2D reais,640x480,ortho8.5. SaveInput desativado somente em memória antes do Play Mode; nenhum save/load chamado. Camera.Render não inclui UI Screen Space Overlay.

Probe físico executado com BoxCollider2D real do player, step0.5u, Collider2D.Cast/OverlapCollider e máscara de colisão real.6529nós;16aproximações conectadas, incluindo casa,cultivo,poço,fonte,cave,estufa,pesca,estações,ponte/saída leste,animais e portas coop/barn. Paths completos e dimensões do collider na metadata. A nova distribuição de árvores foi exercitada: resultados antigos de navegação não foram reutilizados.

Orquestrador inspecionou todas as vistas atuais e confirmou personagem legível, pés sobre o deck, montanha modular, floresta/grama novas e margens/junções corrigidas. Validador também conferiu [bridge.png](gameplay_01/bridge.png). São observações das imagens entregues; não aprovação humana ou equivalência integral à keyart.

## Honest status rationale

**SCOPED_PASS técnico** com11testes focais distintos válidos, geração/import, dez diagnósticos, oito vistas, dezesseis rotas físicas e integridade verificáveis. Aceite humano da arte permanece pendente; a rejeição da rodada anterior não foi apagada nem convertida retroativamente em PASS. Nenhum GLOBAL_PASS, build Player ou suite global foi alegado.

Residual risk: o BFS reposiciona e consulta sweeps, não simula input, caminhada/animação, interação, triggers durante travessia, pesca/crafting, transições ou uma sessão jogada. A captura é640x480; outras resoluções, desempenho jogável e semelhança artística final exigem avaliação adicional. As doze poses do laboratório continuam fora desta integração.
