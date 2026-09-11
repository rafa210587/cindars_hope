## Ambient follow-up — real Play Mode observation

- `ambient-runtime15.log`: Unity6000.5.7f1, exit0, native playback PASS. Evidence in `ambient_runtime15/capture-metadata.json`; timestamp playback in `ambient_runtime15/index.html`.
- One isolated41-game-second session; no regeneration, Animator seeking, time-scale change or gameplay/art edit. Added optional Editor ambient-only mode, reusing save/scene isolation and skipping previously valid unrelated gates.
- Fountain and cascade each exhibited5distinct frames over >=1.4s. Fish exhibited all8frames in two native cycles, starting at19.11074 and39.11080 game seconds: spacing20.000061s. Eight null-sprite idle samples. Camera640×480, orthographicSize8.5, crops unscaled.
- Scene/save hashes unchanged; no runtime errors. Independent code review found no blocker in observation/restoration. Idle invisibility is sampled at5s intervals, not every rendered frame.
- Visual inspection: source structures remain registered; fountain/cascade visible. Fish is a small subtle ambient accent over actual water. Human aesthetic acceptance remains pending; no claim of global scene completion. Browser reproduces timestamped PNGs, with sparse idle frames held; not continuous video or input/UI evidence.
- This supersedes earlier NOT RUN for the41s native animation observation only. No additional art change was justified in this bounded check. Geometry tests were not rerun because their inputs were unchanged.
## Rodada 15 — resultado integrado

- Cena regenerada em `stage15e.log` (exit0); imagens reais em `stage15/`.
- Sete `Visual_LowGroundCover_*` decorativos sem collider, cais10% maior com recorte físico correspondente, quatro construções sul mais espaçadas.
- Fonte/cascata5frames em0,7s; peixe8frames em ciclo20s. `ambient15-sampling-b.log` e `stage15/ambient-validation.json`: amostragem nativa AnimationMode PASS, espera do peixe invisível; nenhum collider modificado pela aplicação das animações.
- `editmode-stage15.xml`:33/33PASS. `gameplay_stage15/capture-metadata.json`:18/18capturas, rotas físicas e seleção de interação PASS, saves preservados. Inclui `dock_v15_front`.
-71árvores: nomes e transformações ancestrais idênticos antes/depois (SHA487BF17A4A6103C6A0B1180EC03BFE0A0D743ED72B337C53CF6661C244A7F7FD).
- Revisão independente aceita ganho local: silhuetas separadas, cais ligado à margem, barco livre, arbustos discretos. Props ainda ocupam parte dos vãos laterais.
- Limites: não houve observação contínua de20s do peixe em PlayMode; fluidez/contraste final pedem revisão humana. A rota usa collider real por BFS/casts, não input humano. Movimento legado não foi revalidado. Spec permanece IN_PROGRESS, sem95% ou promoção global.
- Falhas anteriores abaixo são histórico de tentativas, superadas pelos resultados específicos acima. Editor e Assets devolvidos às Skills após os gates.
# Farm keyart reconstruction v4 — execution evidence

Status: IN_PROGRESS. Promotion: NO. Visual target is not an automatically measured percentage.

## Rodada 15 — arbustos, cais, espaçamento e animações ambientes

- Pedido humano: arbustos baixos sem bloqueio, cais um pouco maior, mais espaço entre as quatro
  construções do sul, fonte em5quadros, cascata animada e peixe saltando/caindo a cada20s.
- Candidatos de geometria em `dev/art/aseprite/keyart-v4/ambient-v15/geometry/`: cais10% maior
  com notch correspondente, construções X=-13.1/-7.2/-1.0/4.9, pequenos arbustos com máscaras
  existentes. IDs e solo preservados. Alterações aplicadas ao código; aguardam captura integrada.
- Arte Aseprite em `ambient-v15/`: fonte/cascata5×140ms e peixe8×120ms. Fontes preservadas;
  JSON, folhas, GIFs, arquivos em camadas e manifesto entregues. Revisão independente sem
  bloqueantes, com legibilidade do peixe no lago ainda pendente. [Prévia de arte](../../../dev/art/aseprite/keyart-v4/ambient-v15/preview.html).
- Reprodução por AnimationClip/AnimatorController nativos; nenhum novo runtime manager ou
  MonoBehaviour. Peixe invisível entre saltos, sem colisão ou recompensa. Configuração de20s
  depende do comprimento real do clip, não apenas do stopTime serializado.
- Falhas preservadas: stage15.log (GetComponent fake-null com operador??), stage15b.log
  (controlador parcial sem layer), stage15c.log (Unity acrescenta1/frameRate ao último sprite key).
  `ambient15-diagnostic.log` mediu0.71/20.01s contra stopTime0.7/20. Root corrigiu as causas.
- stage15d.log avançou aos testes, mas SampleAnimation sem Animator não aplicou o sprite no
  objeto temporário. A validação foi isolada do rebuild e adaptada para AnimationMode com
  Animator configurado; `ambient15-sampling.log` ainda não executou esse teste, pois encontrou
  CS0103 em Skills/Runtime/MagicConfluenceRuntime.cs:65/77. Owner informado para corrigir/freeze.
- A captura atual da entrega continua stage14 até uma geração15 bem-sucedida. Não há PASS
  integrado desta rodada neste registro; as prévias GIF não representam playback Unity observado.

## Rodada 14 — apresentação primeiro e execução proporcional

- Entrega atual: [comparação interativa](progress-review.html), publicada antes de nova integração.
  Mostra stage13, histórico stage1/6/10, keyart aprovada, escala de jogo e limites. HTML servido
  com HTTP 200, referências locais verificadas e página inspecionada no navegador.
- Baseline stage13: oclusão da escada corrigida e revisada visualmente; `gameplay_stage13` é FAIL
  no critério dos quadros de caminhada. Não confundir esse resultado com aceitação da animação.
  A revisão posterior de PlayerWalkAnimator e seus testes permanece candidata, ainda não aprovada.
- Revisão independente e inspeção do root: principais diferenças são massas de vegetação,
  caminhos geométricos e contorno do rio/lago. Cultivos ausentes são intencionais.
- Escopo desta rodada: caminhos e agrupamentos decorativos existentes. Água/colisão da margem
  fica explicitamente para a próxima rodada. Nenhuma nova textura é necessária para posicionar arte existente.
- Um executor reutilizado prepara até dois arquivos candidatos fora de Assets; root integra,
  um revisor examina a captura. Modelos herdados; não houve troca de modelo nem medição de economia.
- Regra criada: `.claude/rules/visual-iteration-budget.md`, descoberta pela skill existente e índice.
  Cópias Codex geradas com zero erros de frontmatter/cópia; hash da regra fonte/cópia idêntico.
- Critérios: junções centrais mais curvas e agrupamentos menos isolados; preservar acessos,
  árvores físicas/IDs, campos vazios, água e perímetro. Uma geração integrada, comparação visual
  e verificações pertinentes de decoração/navegação. Não executar o probe de movimento como gate
  desta mudança de composição; sua falha anterior continua aberta.
- Candidato aplicado após conferir SHA e patch: três junções centrais e seis agrupamentos de
  plantas com rejeição das áreas protegidas. Fontes/cópias anteriores em
  `dev/art/aseprite/keyart-v4/visual-round14/`; somente dois arquivos Editor alterados nesta fatia.
- Primeira invocação Unity: exit1 em aproximadamente6s, antes de gerar a cena. `stage14.log`
  registra CS0246 em `Skills/DefaultSkillActionCatalog.cs:70` (DamageType), fora desta fatia.
  Responsável por skills avisado; não repetir até corrigir e congelar as entradas compartilhadas.
  Não há captura stage14 nesta tentativa. TownScene preservada em SHA
  `C4659BAF9F3AB65084744735D456769436C0724A78931DCB2FE6535F0CF30AC3` antes da invocação.
- Tokens/custo: indisponíveis; não estimados a partir do tamanho de arquivos ou agentes.
- Após o owner informar correção/freeze, segunda invocação abortou em4s: `stage14b.log`, CS0841
  em `Skills/Runtime/Effects/ActiveSkillExecutionController.cs:220,224`. Unity devolvido ao owner
  de skills para concluir a compilação da fase antes de nova tentativa Farm. Nenhuma captura
  nova produzida por essas duas falhas. Fontes Farm congeladas; não são rejeições visuais.
- Depois do compile PASS da fase de skills, `stage14c.log` gerou a cena e capturas com exit0.
  É a única integração visual executada nesta rodada. `editmode-stage14.xml/.log`:14/14 PASS
  (FarmDecorationPlannerTests e FarmSceneNavigationContractTests), scan de log PASS.
- Comparação read-only do YAML, sem edição manual:71 árvores `TreeNode_*` mantiveram nomes e
  transformações de toda a hierarquia. `trees-before.json` e `trees-after.json` em visual-round14
  têm SHA idêntico `487BF17A4A6103C6A0B1180EC03BFE0A0D743ED72B337C53CF6661C244A7F7FD`.
  TownScene manteve o SHA anterior. Foram materializadas16 peças em4 dos6 grupos propostos;
  quatro grupos existentes não significam quatro melhorias visuais aceitas.
- Revisão independente: ACEITAR PARCIAL. Fonte e borda sul receberam massas floridas legíveis.
  Não se confirmou ganho perceptível em4/6 regiões; as3 junções ainda parecem quase iguais.
  Root concorda: preservar ganho local, não declarar o objetivo de composição resolvido.
- Decisão de fechamento desta rodada: não ampliar outra vez a população de pequenos detalhes.
  A próxima abordagem deve redesenhar o percurso/contorno em escala de composição antes de
  outra integração. Campos vazios, fileiras de árvores físicas, margens da água e interior continuam
  pendentes conforme seus escopos. A spec permanece IN_PROGRESS, Promotion NO.
- Play Mode não repetido nesta fatia: não houve alteração de runtime, colisões, interação ou
  animação; a evidência anterior permanece histórica e limitada, não um PASS novo. A candidata
  PlayerWalkAnimator e a falha de movimento de stage13 continuam explicitamente não encerradas.

## Acceptance criteria extracted

The user authorized rebuilding farm terrain, paths, sprites and composition, with delegated execution and independent validation against the approved keyart. A new game starts without crops; the absence of planted crops is not a fidelity defect. Preserve access, physical borders, existing NPCs, interaction identities, house interior and save coordinates. Show actual Unity captures during iteration.

## Existing systems audit

- Reference: `docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png`, 1536×1024, SHA256 `33DF838F664367A1B5559F39A54B76798F2C3990ADA9D03847145C6B1F2C0B9F`.
- Baseline: `docs/validation/farm_aseprite_pilot/editor_v3/farm_capture_keyart_composition.png` at identical framing. Do not change the comparison camera to conceal geometric errors.
- Reuse the existing farm creator, spatial/physics contracts, tile painter, registries, roof reveal, Aseprite skills and scoped Unity capture/test runners. No whole-reference background, manual Unity YAML, new shop economy or save schema.
- Original diagnosis was corrected by measurement: field outer size was already close. Main gaps were displaced house/pasture, undersized fountain/well, straight cliff, repetitive forest, flat road/water and missing landmark silhouettes.

## Delegation and review

- `farm_rebuild_v4`: layout, landmark wiring and physical/mask integration, with explicit file ownership and freeze points.
- `farm_landscape_assets_v4`: individual reference extractions in Aseprite; no C# or Unity edits.
- `farm_fidelity_audit_v4`: independent visual inspection and normalized landmark estimates; read-only.
- `farm_physics_review_v4`: read-only integration review; identified the greenhouse tile exception crossing a structural wall.
- `farm_finish_composition_v4`: a separate executor for remaining forest, water and pathside composition after stage2 was not visually accepted.
- Root inspected delivered PNGs, performed Unity regeneration, inspected captures and owns the physics probes/tests and acceptance decision. Delegated summaries alone are not acceptance evidence.

## Art authoring

Aseprite 1.3.18.5-x64 at the user-provided Steam installation was used for actual crop, polygon-alpha isolation, editable layers and PNG export. Sources and scripts are under `dev/art/aseprite/keyart-v4`; the original reference remains unchanged. The separate animation skill is not forced onto static scene work.

Individual fountain, well, bridge, waterfall, market stall, tree, boulder, lateral cow/sheep, house and greenhouse images preserve their own editable sources. Reflections are separated from a water sample. The road is authored from the reference palette/sample and retained as a farm-only atlas. See [asset measurements](asset-measurements.json) and [landscape source review](../../../dev/art/aseprite/keyart-v4/landscape/REVIEW.md).

The opaque-bounds helper uses the full rectangle for unreadable textures. This was discovered during review; known alpha extents are therefore used for these assets rather than claiming the helper measured transparent margins. Import remains Point, uncompressed, no mipmaps; global importer settings are not changed for measurements.

## Spec compliance matrix

| Criterion | Evidence / current result |
|---|---|
| Same-frame comparison and progress | [Comparison page](delivery.html), stages1–10 actual Unity PNGs |
| Major landmarks | Source-derived house, greenhouse, fountain, well, bridge, waterfall, animals and four southern exteriors integrated; source-derived dock enlarged |
| Exterior/material fidelity | Stage4 review stabilized forest; stage5 corrected secondary buildings, planting and path edges. Boat layering and dock geometry corrected for stage6 |
| Physical routes / perimeter | Stages3–5 probes PASS: 19 routes, 488 boundary sweeps, 32 landmark checks, 3 dock-water checks. Stages6–7 add two full-collider deck checks (34landmark checks). Stage5 does not certify polygon simplicity; see independent defect below |
| Non-arable structures | Stages3–5: 63 house tiles, 2 fountain/well checks and 2 greenhouse wall/interior checks PASS |
| Respawn | Child respawn retains fountain identity and bootstrap reference; actual player collider checks PASS |
| Registry continuity | 68 trees rebound, 0 initial plots, Zrix active; no replacement of stable IDs |
| Complete PlayMode capture / interactions | Stage12 PASS:18/18 views,7actual selections,37timed motion images,0runtime errors; see motion scope below |
| EditMode / final human appearance | Stage12 scoped81/81 PASS across ten fixtures. High-fidelity appearance and human acceptance remain pending |

## Validation chronology

1. `stage1.log`: Unity regeneration/capture exit0. Root and reviewer rejected mine occlusion, undersized landmarks, bright/coarse road and repetitive exterior.
2. `stage2.log`: exit1, compile blocked by concurrently authored Town dependency `TownKeyartSceneArt`. Farm did not alter/revert Town. Coordinated with the other task.
3. `stage2b.log`: dependency completed; Unity regeneration/capture exit0. Root inspected full capture.
4. `gameplay_stage2.log` and `gameplay_stage2/capture-metadata.json`: overall FAIL, 8/18 views before obsolete `border_north` framing point was found inside the new mountain. Physical query subsection PASS as enumerated above; zero runtime errors, scene hash unchanged. Interaction subsection remained RUNNING, not PASS. Framing correction is not evidence of a rerun.
5. `stage3.log`: exit0. West-only envelope extension to76×50 and exact house/greenhouse integrated. `gameplay_stage3`: PASS18/18; `editmode-stage3.xml`:37/37 PASS. Independent visual review rejected oversized outer canopy masses, rectangular paths and sparse landmark details.
6. `stage4.log`: exit0. Smaller/darker mixed forest, shaded forest floor, curved variable-width paths, actual water atlas. `gameplay_stage4`: PASS18/18. Visual review accepted stabilizing forest but rejected remaining road margins, undersized mine/dock and missing local detail.
7. `stage5.log`: exit0. Exact southern exteriors, enlarged mine/dock, denser regional planting and road union margins. `gameplay_stage5`: PASS18/18. Independent geometry review nevertheless found an enlarged dock-notch self-intersection and blocked upper deck. Root also observed the boat hidden under water because the sorting helper only sets order on fallback. These are real failures despite the sampled gameplay pass.
8. Stage6 source fixes: cut deck notch at the actual sloped shoreline intersection, expose the upper deck, explicitly sort the boat above water, add segment-crossing regression and two full-player-collider deck checks. Independent geometry review passed the corrected simple contour and confirmed both collider poses fit. `stage6.log`: compile blocked by concurrently edited `TownCityLayout` missing `Name`/`DoorTangentOffset`; Town owner notified. Final regeneration and runtime evidence pending.

Unity version: 6000.5.7f1. Regeneration uses `CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture` with `CINDARS_FARM_CAPTURE_OUTPUT`; PlayMode uses `CaptureFarmGameplayBatch` with `CINDARS_FARM_GAMEPLAY_OUTPUT` and without `-quit`. Root coordinates the single Unity window with the city task.

## Later validation and remaining art work

9. `stage6b.log`: Town dependency corrected, regeneration exit0. `gameplay_stage6`: PASS18/18,34landmark checks, both enlarged-deck poses PASS. Expanded `editmode-final.xml`:67/68 PASS, one obsolete NorthCliff bound reachedy27.5 above the y25 envelope. Corrected the navigation metadata to derive from the actual Mountain footprint; no anchor/envelope changes.
10. `stage7.log`: exit0. Road sample no longer doubles source clusters; exact dark separated lilies and yard board/tool rack integrated; southern woodland mixes smaller tree/rock groups. `gameplay_stage7`: PASS18/18; `editmode-stage7.xml`:68/68 PASS across eight relevant fixtures. Independent reviewer marks coarse-road/lily defects resolved; two visual reviewers still reject very-high fidelity due to forest underlay, incompatible old flower/bush motifs and missing confluence detail. This technical pass is not final visual acceptance.
11. `stage8.log`: regeneration exit0. Source-derived pine, low vegetation and flowers replace incompatible motifs; transparent forest shade exposes detailed ground. River bend and source-derived cascade are integrated below the unchanged bridge. `gameplay_stage8`: PASS18/18. Road/shore material improvements remain visible, but inherited resource-tree art still differs from the decorative perimeter.
12. `stage9.log`: regeneration exit0. The existing68 resource trees use source-derived pine/oak sprite geometry with unchanged IDs/root anchors. `gameplay_stage9.log` first attempt exited1 without a diagnostic cause; retry `gameplay_stage9b.log` exit0 and metadata PASS:18views,19routes,488boundary checks,34landmark checks,7interaction selections,zero runtime errors,Zrix active,scene hash unchanged. `editmode-stage9.xml`:70/70 PASS, including river/lake polygon simplicity and river-mouth overlap regressions.
13. Independent stage9 inspection of full composition and actual homestead/south/lake gameplay views accepts the forest-floor and old flower-motif fixes, while rejecting intended high fidelity: regional undergrowth remains too small/sparse, and fence rails differ. Next bounded pass calibrates reference-pixel size (18.2857pixels/world-unit in this fixed camera) and authors asymmetric cliff/well/forest-edge groups; do not globally enlarge landmarks again.
14. `stage10.log`: regeneration exit0. Source-derived upright fence posts/rails, two shared well-clearing segments, native-scale north/well planting and southern forest pockets integrated. `gameplay_stage10/capture-metadata.json`: PASS18views,19routes,488boundary checks,37landmark checks,12fence tilling checks,7live selections;0runtime errors,68trees rebound,0initial plots,Zrix active. FarmScene hash before/after `1a92c81a8e68d76573921389dddccd47c78042aa2e30cafe24968de7d351a061`. This static configuration does not yet wire the new animated assets.
15. Shared opt-in `HouseDoorInteractable`/`RoofRevealController` API integrated with legacy defaults preserved and coordinated with the Town owner. `editmode-door-api.xml`:10/11 PASS; one EditMode lifecycle fixture failed because disabling an ordinary MonoBehaviour in EditMode did not dispatch OnDisable. The fixture now explicitly exercises that handler. `editmode-door-api-rerun.xml`:11/11 PASS, Unity compile/log scan PASS. This is not in-game door/water evidence.

## Door, interior and water continuation

The user extended the authorized scope to physical props, animated doors, a visible usable house interior, moving lake water and Farm-side Town/cave entrances. GPT-generated existing art is the source; Aseprite authored five door poses at80ms and six64×64 water frames at200ms, with editable sources and actual GIF playback in [animation studies](animation-preview.html). Existing animations were not all migrated. Source/frame checks are recorded under `dev/art/aseprite/keyart-v4/animation`; these GIFs are not Unity captures.

The house facade derivative clears only the399 original door-opening pixels; the closed leaf recomposes those399 pixels exactly. Its leaf remains independently visible inside/outside, while the threshold follows exterior reveal. The front physical wall moves from the staircase support to the measured actual leaf foot. Furniture, entrances and animated tiles were integrated during the next coordinated Farm window; the original source images remain intact.

Independent candidate review found that furniture collision stopped the player before the announced frontal interaction: the original1.2u triggers were too shallow for the1.125u player body. Triggers now reach2u. Stage12 confirms clear local body sweeps to bed(12.7,12.8)/chest(18.1,13.85) and actual trigger distance within0.45u; this does not execute sleeping/storage actions.

16. First stage11 compile failed on the Unity-version error for GetInstanceID; diagnostic identity fields now use GetEntityId. `stage11b.log`: generation exit0. `gameplay_stage11`:18views and all physical/selection subsections PASS; overall FAIL because Unity cannot attach an Editor-assembly MonoBehaviour for the optional motion driver. The driver is now in Diagnostics with UNITY_EDITOR, instantiated only by the Editor probe and absent from player builds.
17. `gameplay_stage11b`: opening showed all5poses and retained its blocker until the final pose; overall FAIL at occupied closing. Investigation found the actual Farm player lacked the Player tag consumed by both door occupancy and roof reveal. Corrected the Farm scene creator's identity wiring; shared door/roof APIs did not change.
18. `stage12.log`: generation exit0. `gameplay_stage12/capture-metadata.json`: overall PASS18views,19routes,488boundary sweeps,37landmark checks,24named solid-instance checks,4local approaches,78authored solid/tilling tiles,7live selections and0runtime errors. Scene hash before/after `507dd886ec824a38fcab4657ccd79532f3c3427c7d86cb5320c283d4a77004f7`; persistent save hashes unchanged. `editmode-stage12.xml`:81/81 PASS; compile/log scan PASS.

The stage12 optional motion phase records37actual PNGs with Time.time/fixedTime: all5opening poses, occupied-close refusal, physical doorway traversal, feet-triggered interior, exterior restoration, reverse closing and a second stopped approach. All6water frame indices were observed with identical lake/river collider fingerprints. [Unity sequence](gameplay-motion.html) replays recorded intervals, with a separately disclosed final presentation pause. Movement is a controlled actual Rigidbody traversal; movement input, continuous video fluidity, sleeping/storage actions and scene transitions are not claimed tested.

Independent stage12 review confirms the physical evidence and source-alpha checks, but found a P2 visual defect: the full Roof-layer facade hides the actor on the exterior stairs before useful-interior entry. Fix sorting/pivot without triggering reveal from a proximity sensor. The room is functional with bed/chest, but wall/decorative finishing remains basic. Barn/coop art already has open leaves; action-linked animation is still pending, and their release handler's spawned animal sprite needs a separate factual check before a success demonstration.

`docs-validation-stage8.log`: strict documentation validation FAIL,77 diagnostics across the repository. The selected Farm spec/report/CURRENT_STATE produced no matching diagnostic in that output; this is not GLOBAL_PASS and unrelated documentation is not changed incidentally.

## Honest status rationale

Reconstruction is in progress; no claim of 95% similarity or final acceptance. Physics queries use the actual player collider but do not simulate movement input, animation quality or completed interactions. Live selection evidence is separately recorded. Existing progressed saves are not certified merely because IDs and coordinate identity are preserved; no real save is overwritten.

Remaining work: execute the final composition/geometry, inspect comparison and gameplay views, fix failures, run the relevant EditMode suite, complete independent review and update this report with current artifacts.


