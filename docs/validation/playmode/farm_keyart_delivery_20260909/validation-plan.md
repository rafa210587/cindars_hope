# Plano de validação — entrega FarmScene pela keyart

Escopo SCOPED selecionado por `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`: gerador C#, contratos de layout, cena regenerada e apresentação real. Não executar .NET/compile-only redundante: Test Runner e geração provam compile Editor.

- Backup prévio: PASS 191 arquivos; 140 PNGs World com hashes. Baselines da rodada anterior preservados em before_gameplay/ e before_diagnostic/.
- Exclusividade: nenhum Unity ativo no preflight; somente este owner executa Unity após liberação do executor/root.
- EditMode: filtro `CindarsHope.Tests.EditMode.Farm;CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests;CindarsHope.Tests.EditMode.Editor.FarmTerrainMaskTests`. Runner canônico, XML e log novos, scan incorporado sem repetição.
- Regeneração: `FarmSceneCapture.RegenAndCapture`, batch gráfico + quit; capturas diagnósticas preservadas por rodada, log e exit explícitos. PNGs World preservados; delta de cena/meta/tileassets comparado ao backup.
- Cena: validators pertinentes de composição, navegação, spatial/terrain/decoração conforme capacidade do entry point. Resultado do menu exige mensagem/contagens explícitas, exit zero sozinho não comprova PASS.
- PlayMode: `FarmSceneCapture.CaptureFarmGameplayBatch`, sem quit/nographics; captura spawn/bridge/cultivation, hashes cena/saves, zero runtimeErrors. Capturas adicionais regionais serão registradas se implementadas no helper existente.
- Movimento/física: flood-fill atual prova contrato, não toda física materializada. Ponte verifica OverlapPoint; captura com teleporte não prova deslocamento. Cenário físico deve produzir evidência própria ou permanecer NOT RUN/residual.
- Docs: docs/gates sob responsabilidade do orquestrador; nenhuma alegação GLOBAL_PASS. Falhas históricas de CURRENT_STATE permanecem explícitas, não reinterpretadas como sucesso.

Command attempted: nenhum Unity iniciado nesta preparação; aguardando liberação para evitar compilar inputs concorrentes.
