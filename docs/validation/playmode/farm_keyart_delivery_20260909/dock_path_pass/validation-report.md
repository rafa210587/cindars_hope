# I — cais, pesca e acabamento dos caminhos

Resultado técnico: **SCOPED_PASS**. Cena final SHA256 `751A9263324F2A322135E9900E969FC5B22F73955DB31D629A592296FFACF803`, materializada em regen_01 e capturada em gameplay_01. Root confirmou a revisão do overview, water_bridge e das oito vistas reais. Aceite humano específico desta versão permanece separado.

## Acceptance criteria extracted

Encaixar cais e barco no lago, permitir acesso real ao deck, preservar bloqueio da água adjacente, selecionar pesca pelo sistema real e melhorar o acabamento das trilhas. Preservar IDs, cenas/saves, player/câmera, 71 árvores e sistemas não afetados. Nenhum Interact, pescaria ou input artificial integra o smoke.

## Existing systems audit

Reusados PNG original do cais/barco, gerador Farm, contratos de layout/colisão/navegação, Planner, composers e probes existentes. A imagem tem 1245×1263 pixels; nenhum raster novo foi produzido. O Lake visual permanece, enquanto LakeCollisionPath recorta deck e gangway e fornece o mesmo contorno ao NavigationRaster.

FishingSpot usava ID vazio com fallback pela posição arredondada. A revisão moveu o root para (10;-10) e serializou o ID legado `farm_spot_7_-10`, preservando a identidade. O profile e os mecanismos de trigger/edge continuam; seleção real comprova seu alcance no novo local.

Baseline H preservada em before_diagnostic/before_gameplay, backup de 372 arquivos e 151 PNGs. Todos os hashes de origem/cópia foram conferidos em baseline-inputs.json.

## Spec Compliance Matrix

| Critério | Resultado | Evidência |
|---|---|---|
| Compile Editor e geração via API | PASS | regen_01/process.json, unity.log, runner-result.json; processo e scan exit 0; dez diagnósticos |
| Testes pertinentes | PASS 32/32 | focal-tests_01.xml/log: Planner 5, Layout 17, Spatial 4, Navigation 6; runner/process/scan 0 |
| Câmera real e sorting | PASS | gameplay_01: oito vistas, todas CustomAxis/Y |
| Rotas | PASS 16/16 | 6465 nós visitados, paths não vazios; fishing_approach solicitado e alcançado exatamente em (10;-10), tolerância 0,1u |
| Água adjacente | PASS 3/3 | Collider real bloqueado em (8,5;-10), (11,8;-10,7) e (10;-11,5), separados da evidência de alcance |
| Seleção de pesca | PASS | FishingSpot selecionado em (10;-9,5), quinta tentativa, por callbacks/CanInteract/GetCurrentInteractable; cinco alvos anteriores preservados |
| Pés no deck, barco na água | Revisão visual do root | lake.png mostra player (10;-10) apoiado; root inspecionou a imagem final |
| Recursos | PASS de integridade | scene-identity-review.json: mesmos 71 IDs, zero mudanças de posição/sprite/escala/dados; 15 jardins sem Collider/TreeNode |
| Arte e imports | Reuso verificado | reused-art-inputs.json: 151 PNGs e 151 metas anteriores iguais; nenhum PNG novo |
| Cena, fonte e saves | PASS | final-audit.json: hash atual==metadata antes/depois==snapshot, zero source/test drift, saves iguais e zero runtimeErrors |

## Validation

Existing tests executed: YES
Test command: powershell.exe -File tools/unity/RunUnityEditModeTests.ps1 -ProjectPath . -TestFilter CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests;CindarsHope.Tests.EditMode.Farm.FarmLevel1LayoutContractTests;CindarsHope.Tests.EditMode.Farm.FarmSceneSpatialContractTests;CindarsHope.Tests.EditMode.Farm.FarmSceneNavigationContractTests -ResultsPath docs/validation/playmode/farm_keyart_delivery_20260909/dock_path_pass/focal-tests_01.xml -LogFile docs/validation/playmode/farm_keyart_delivery_20260909/dock_path_pass/focal-tests_01.log
Test evidence: focal-tests_01.xml/log e focal-inputs.json
Test result: PASS 32/32

Unity 6000.5.7f1. Invoke-FarmValidation.ps1 registra comando, timestamps, sourceInputs e snapshots e executa um único scan por log. O Test Runner comprova compile e comportamento numa execução. Não houve falha técnica nesta rodada nem repetição de .NET, compile-only ou suíte global. Nenhum Unity permaneceu aberto.

Medição read-only comparável em dock-baseline-water-review.json e dock-after-water-review.json: a ROI aproximada do casco passou de 46,64% para 100% de seus pixels opacos sobre o polígono visual Lake; o conjunto cais+barco passou de 8,26% para 89,86%. A ROI não é segmentação exata nem gate artístico. O conteúdo opaco mudou de aproximadamente 5,011u para 4,209u de altura (4,22 para 3,54 alturas do player). A geometria final e sua inspeção contextual prevalecem sobre esses indicadores.

A validação documental usa tools/docs/run_strict_validation.ps1 -Gates diff,quality -ScopePath docs/validation/playmode/farm_keyart_delivery_20260909/dock_path_pass/docs-scope.json. Resultado documental separado em docs-scoped.log/result; não inclui auditoria global de dívida não afetada.

## Honest status rationale

A revisão I tem evidência vigente de geração, colisão, seleção, escala e inspeção visual. A cena H e seu histórico permanecem imutáveis nos backups/evidências; seus PASSs não foram reutilizados como imagens da nova cena.

As seis seleções não executam ações. Não houve pescaria, consumo, minigame, captura de peixe ou save/load; portanto esses fluxos continuam NOT RUN. O ID legado foi preservado na cena, sem alegação de round-trip de persistência. BFS e fotos não substituem input humano nem animação em movimento. Build Player/IL2CPP e aceitação humana não fazem parte dos gates executados. Não declarar GLOBAL_PASS ou promover a spec com esses critérios pendentes.
