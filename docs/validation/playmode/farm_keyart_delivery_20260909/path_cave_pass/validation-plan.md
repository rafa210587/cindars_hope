# H — caminhos, entrada exterior da caverna e vegetação baixa

Status: **PREPARED**. Unity NOT RUN nesta rodada, aguardando freeze conjunto e release do root. Validator exclusivo; nenhuma edição de C#, PNG ou YAML nesta preparação.

## Baseline imutável

- before_diagnostic e before_gameplay copiam proportion_pass/regen_02 e gameplay_02, incluindo snapshots, metadados, logs e comandos.
- Cena SHA256 `EB848C096E5E13CD015E61516EE0E7E11374708CBB22B37CF22CB9515F723D16`.
- Backup `art/farm-pixelart-review/backups/20260909_114614-path-cave-pass`: 370 arquivos, 150 PNGs, zero divergências de hash. Manifesto em baseline-inputs.json.
- Baseline G: nove testes focais PASS, três TreeNode reutilizados por equivalência, oito vistas, 16 rotas, quatro seleções e CustomAxis/Y. Evidência histórica permanece em proportion_pass.

## Auditoria da mudança de posição da caverna

| Ponto | Fonte atual | Implicação para a revisão |
|---|---|---|
| Entrada funcional | CreateMvpFarmScene.CreateCaveEntrance, posição literal (-28;18,5) | Mover o root com trigger e visual; trocar só o sprite não muda a interação |
| Âncora e footprint | FarmLevel1LayoutContract: CaveEntranceX/Y, CaveMouthMinX/Y/Width/Height | Atualizar de forma coerente com a entrada materializada; FarmSceneSpatialContract e Zone_CaveEntrance consomem esse contrato |
| Retorno da cave | CreateSceneSpawnInstaller: farm_from_cave literal (-23;15); SpawnFromCaveX/Y no contrato | Preservar o ID, garantir ponto físico seguro e coincidência entre contrato e SceneSpawnPoint |
| Entrada na outra cena | CaveEntranceInteractable._targetSpawnId = cave_from_farm | Preservar ID/destino; esta rodada trata o exterior da Farm, não procedural ou stable run |
| Mural vizinho | Board_Zrix literal (-22;16) | Owner decide reposicionar com a entrada ou preservar; evitar abandono visual/obstrução do acesso |
| Rota existente | FarmPhysicalRouteProbe: cave_entrance_approach em (CaveEntranceX;CaveMouthMinY-0,75) | Atualiza pelo contrato, mas comprova aproximação com tolerância 1,25u; não comprova seleção ou transição |
| Foto de gameplay | FarmPlayModeCaptureSession, view mountain usa SpawnFromCaveX/Y | Atualiza pelo contrato; conferir se a câmera mostra a entrada nova e o player em chão livre |
| Diagnóstico regional | FarmSceneCapture: mountain centrado em (0;MountainBaseY-2), ortho14 | Recorte amplo; root confere visibilidade da entrada no novo enquadramento |
| Composição/collision | FarmLandscapeVisualComposer.CreateNorthCliff e polígonos Mountain do FarmSceneSpatialContract | Manter boca visível, sem face rochosa cobrindo a arte nem sólido invisível no ponto de aproximação |
| Testes | FarmLevel1LayoutContractTests.CaveEntranceIsAtNorthwest exige X<0 e Y>5 | Reconciliar com a âncora aprovada se deixar de ser NW; atualizar intenção verificável, sem remover proteção por conveniência |

Extensão H03 autorizada explicitamente pelo root e documentada na spec antes do código: quinto alvo CaveEntranceInteractable, preservando os quatro anteriores e o mesmo probe, alcance e budget. Usar route cave_entrance_approach, callbacks reais e GetCurrentInteractable; sem Interact ou transição. Novo resultado esperado: cinco seleções PASS. Entrada aprovada (-19,5;17), retorno (-19,5;14,5), mouthMin (-21,5;15,25), tamanho 4×3,5 e Board_Zrix (-16,5;15,5).

## Gates mínimos após freeze

1. Conferir emenda H e inputs finais. Exclusividade Unity, snapshot e hashes antes/depois; geração via Editor API, dez diagnósticos e scan único. Compile reportado pela execução, sem compile-only redundante.
2. Inspeção visual do root: caminhos orgânicos e junções, alinhamento da boca/rocha, retorno em chão livre, vegetação baixa sem cobrir rotas/portas. Não converter PASS técnico em aceite artístico.
3. Focais dependem do diff efetivo: Layout/Spatial/Navigation quando suas âncoras ou polígonos mudarem; Planner quando planejar caminhos/clareiras; Composition somente se inputs pertinentes mudarem. Reusar G e TreeNode quando equivalência for demonstrada, sem repetir suíte global.
4. Oito vistas vigentes, 16 rotas com collider real, cinco seleções reais (casa, craft3 e cave exterior) e CustomAxis/Y. Fotos e seleção não demonstram transição Farm↔Cave nem stable run.
5. Auditoria serializada compara entrada, trigger, zone, spawn farm_from_cave e IDs/destino; nenhuma mudança de save/schema. Mesmo conjunto de 71 TreeIDs; posições alteradas pelo novo corredor devem ser comparadas e documentadas, conforme autorização H01; vegetação pequena nova sem Collider/TreeNode.
6. Preservar dimensões e posição da ponte/porta aprovadas em G, limites e saída Town. Comparar 150 PNGs/metas por hash; novos imports somente se houver novo asset autorizado.
7. Scene hash atual == metadata antes/depois == snapshot; fonte congelada durante Unity, zero erros runtime e saves intactos. Docs consolidados no closeout quando os inputs estabilizarem.

Residual risk atual: cena H ainda não materializada/validada. Humano, input, animação em movimento e transição completa não executados. Nenhum claim de revisão do interior procedural da caverna; se o escopo passar a alcançá-lo, aplicar FASE9F antes de qualquer edição e selecionar gates correspondentes.

