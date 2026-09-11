# H — caminhos e acesso exterior da caverna

Resultado técnico: **SCOPED_PASS** sobre regen_04 e gameplay_02. Cena final SHA256 `1C6311A582901D2F69A60A3D3E90E43940E95C3D18E8D1485CA3C7944F764844`. Root confirmou a inspeção do overview e das oito vistas finais. Aceite humano específico de H permanece pendente.

## Acceptance criteria extracted

Caminhos orgânicos, entrada exterior e retorno coerentes com a keyart, vegetação pequena sem física; preservar recursos, IDs, ponte e porta aprovadas. Oito vistas MainCamera, 16 rotas e cinco seleções reais, incluindo CaveEntrance, com CustomAxis/Y, cena e saves preservados. Revisão visual do root e aceite humano são resultados separados.

## Existing systems audit

Reuso do gerador Farm, contratos Layout/Spatial/Navigation, Planner, composers, captura e probes existentes. H acrescenta CaveEntrance à lista de seleção real; não executa Interact ou transição, nem altera procedural/stable run. Baseline G preservada em before_diagnostic/before_gameplay e backup de 370 arquivos/150 PNGs, com hashes verificados.

## Spec Compliance Matrix

| Critério | Evidência atual | Estado |
|---|---|---|
| Entrada, zona e retorno | regen_04 snapshot: entrada/zone (-19,5;17), retorno (-19,5;14,5), mural (-16,5;15,5); IDs/destino preservados | Materializado |
| Planejador e contratos | 26 casos válidos de focal-tests_01.xml + Planner 5/5 em planner-tests_03.xml; equivalência dos inputs não afetados | PASS 31 distintos |
| Oito vistas, 16 rotas, cinco seleções | gameplay_02: PASS, 6455 nós; cave selecionada em (-19;15,5), duas tentativas; oito CustomAxis/Y | PASS vigente |
| Recursos e vegetação | regen_04: mesmos 71 IDs, 29 posições diferentes da baseline G; 15 PathGarden, sem Collider/TreeNode | Auditoria estrutural PASS |
| Caminhos e arte final | Root solicitou melhoria do raster diagonal após regen_02; regen_04 usa grade visual 0,25u e mural GPT | Inspeção final do root separada |

## Validation

Unity 6000.5.7f1, owner exclusivo. Comandos, timestamps, hashes e snapshots nas pastas das execuções. Scan único por log de geração/Play. Compile-only, .NET e suíte global não repetidos.

Existing tests executed: YES
Test command: powershell.exe -File tools/unity/RunUnityEditModeTests.ps1 -TestFilter CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests -ResultsPath docs/validation/playmode/farm_keyart_delivery_20260909/path_cave_pass/planner-tests_03.xml -LogFile docs/validation/playmode/farm_keyart_delivery_20260909/path_cave_pass/planner-tests_03.log
Test evidence: planner-tests_03.xml/log (5/5), focal-tests_01.xml/log (26 outros casos válidos), focal-unaffected-reuse.json
Test result: PASS, 31 casos distintos pertinentes por execução e equivalência verificável

Histórico preservado:

1. regen_01 FAIL: InvalidOperationException, sem posição válida para TreeID 43. Nenhum PNG novo, saved scene inalterada, Unity/scan exit 1. Implementação acrescentou busca determinística nas outras bolsas existentes, mantendo afastamentos e limites.
2. regen_02 PASS de geração; focal-tests_01 FAIL 30/31 por Meadow count 11. Runner encerrou após Unity test exit 2, portanto scan desse log NOT RUN. Root também rejeitou os degraus grandes dos caminhos diagonais.
3. gameplay_01 PASS 8/16/5, sem ação ou transição da caverna. Evidência independente útil, preservada como intermediária.
4. regen_03 PASS de geração após Meadow/clareira e grade visual 0,25u; Planner 5/5 PASS, processo/scan 0. Os outros 26 casos são reutilizados por sete inputs iguais. Nenhuma falha anterior foi apagada.

5. regen_04 e gameplay_02 PASS após integração do mural e sua reserva. Planner foi executado novamente em planner-tests_03.xml: 5/5, processo e scan 0. O conjunto final contém 31 casos distintos válidos, sem somar reruns como testes novos.

## Honest status rationale

A cena final tem geração, captura e critérios técnicos comprovados. final-audit.json confirma hash de cena atual igual ao metadata antes/depois e snapshot, zero source/test drift, oito vistas CustomAxis/Y, 16 paths não vazios, cinco seleções, zero runtimeErrors e saves inalterados. Nenhum Unity permaneceu aberto. Play por teleporte/BFS, fotos e seleção não comprovam caminhada por input, animação, receitas ou transição completa Farm↔Cave. O interior da cave, saves e stable run não foram exercitados nesta rodada. Resolução runtime da textura da ponte permanece NOT QUERIED, conforme escopo G aceito pelo root; não é mudança H.

Import novo: notice-board-import-review.json comprova mural RGBA 1254×1254, alpha 0..255, raw==asset byte-exato, Point/None/noMip, 128 PPU e DefaultTexturePlatform max2048. reused-art-inputs.json preserva 150 PNGs e 150 metas anteriores. H não alterou o arquivo antigo do mural/placeholder nem sprites anteriores. Nenhum GLOBAL_PASS ou aceite humano inferido. Validação documental selecionada: tools/docs/run_strict_validation.ps1 -Gates diff,quality -ScopePath docs/validation/playmode/farm_keyart_delivery_20260909/path_cave_pass/docs-scope.json. Resultado separado em docs-scoped.log e docs-scoped-result.json; este gate não inclui a auditoria documental global.


