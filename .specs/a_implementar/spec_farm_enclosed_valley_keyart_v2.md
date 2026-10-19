# spec_farm_enclosed_valley_keyart_v2

> Status: IN_PROGRESS — reaberta por rejeição visual humana; reconstrução v4 autorizada. Promoção NO.
> Evidence: [reconstrução v4 em andamento](../../docs/validation/farm_keyart_v4/REPORT.md); [entrega anterior](../../docs/validation/farm_enclosed_valley_20260909/REPORT.md).
> Type: Runtime scene integration / visual reconstruction
> Domain: FarmScene
> Ownership: terreno/limites — implement_harness_routing; composição interior — implement_visual_skills; integração/evidência — root; auditoria — audit_agent_roles.
> Ordem de execucao: referência e contratos → regiões em paralelo → geração única integrada → testes/capturas → correções → closeout.
> Depende de: estado existente preservado como baseline; imagem aprovada v2.
> Bloqueia: aceite final da fazenda.

required_adrs: []
required_game_rules: []

# /speckit.specify

## Objetivo e autorização
Reproduzir a imagem [farm_enclosed_valley_approved_v2.png](../../docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png) na cena Unity. O pedido humano rejeitou o prado exterior aberto: o contorno deve ser floresta/montanha que bloqueia fisicamente, não paredes invisíveis no gramado. A proposta aprovada mantém marcos, adiciona pomar e pasto, permite ampliação moderada. A v2 substitui a direção visual aberta da spec_farm_outskirts_composition_v1, sem apagar suas evidências históricas.

## Phase 0 confirmado
Baseline tem solo112×80, região jogável64×44, lago e rio poligonais, ponte livre, 71 TreeNodes em SceneRuntimeReferences (68 no TreeRegistry e3periféricos), Zrix, quatro plots legados, instalações e sistemas existentes. A periferia anterior usa decoração sem física e quatro paredes retangulares: insuficiente para o novo requisito. Reusar FarmPerimeterVisualComposer, FarmSettlementVisualComposer, FarmDecorationPlanner, FarmLandscapeVisualComposer e os contratos FarmLevel1LayoutContract/FarmSceneSpatialContract. Não criar sistema paralelo de cena.

## Contratos e limites de comportamento
- Ampliar envelope jogável para72×50, x[-36,36], y[-25,25], cerca28% de área adicional. Sem escalar globalmente personagem, edifícios ou mundo.
- Floresta/rochas formam faixa contínua W/S/E; escarpa contínua N. Colisão é apoiada nas bases visíveis, não em copas soltas nem clareiras. A saída cidade continua uma interação de transição no corredor leste; não abrir fuga para fora do mundo. Caverna exterior preserva ligação de entrada, sem tocar cave procedural.
- Manter água/cais/ponte funcionais e seus IDs. Preservar todos IDs de plots, árvores, NPCs, crafting e save; reposicionamento visual autorizado, schema e regras econômicas não.
- Referência de regiões: casa+estufa NE; fonte oeste; pomar entre fonte e cultivo (centro aproximado -17,0); pasto SW (centro -26,-15); dois blocos agrícolas centrais; fila de quatro construções ao sul; lago SE. Ajustar posições dentro dessas regiões por medição, não inventar outro layout.
- Árvores coletáveis existentes preservam registry/IDs; árvores da fronteira são cenário sólido permanente. Pasto visual não cria animais econômicos gratuitos nem nova simulação. Usar sprites/representações existentes e registrar qualquer diferença da referência; não fingir funcionalidade inexistente.
- Decisão humana durante execução: COMEÇAR VAZIA, cultivos somente após plantar. A ausência de culturas na cena inicial é diferença intencional da imagem. Não pintar lavoura decorativa sobre células utilizáveis nem inserir plantações por captura/save.

# /speckit.plan

## Arquivos e ownership
Owner limites: Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs; Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs (preservar tile→mundo ao ampliar bounds e reconstruir máscaras); Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs; Assets/_Game/Scripts/Editor/Art/FarmPerimeterVisualComposer.cs; Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs; Assets/_Game/Scripts/World/Scale/FarmSceneCompositionContract.cs. Criar helper de geometria apenas se necessário para compartilhar fronteira visual/física sem duplicação, sob Farm/Scene/. Coordenar assinatura antes de uso.

Owner interior: Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs, FarmDecorationPlanner.cs, FarmLandscapeVisualComposer.cs, WorldTilemapGround.cs; novos sprites necessários via ferramentas autorizadas em Assets/_Game/Art/Generated/World/, import por Editor API. Não editar creator do outro owner: pedir wiring por mensagem.

Owner validação/root: Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs e FarmPlayModeCaptureSession.cs; Assets/_Game/Scripts/Editor/Validation/FarmPhysicalRouteProbe.cs, ValidateFarmSceneNavigation.cs e validadores farm pertinentes somente correções de critérios afetados; testes específicos em Assets/_Game/Tests/EditMode/; FarmScene.unity gerada via Unity Editor API. Evidência docs/validation/farm_enclosed_valley_20260909/, esta spec, índices, PROJECT_LOG e IMPLEMENTATION_STATUS.

Proibidos: edição manual de YAML, Town/Cave internals, saves/DTOs, novos menus avulsos, refactors sem relação, git destrutivo/commits/push. Preservar dirty anterior. Um único owner inicia Unity após sincronizar fontes.

## Plano por fatia
1. Limites: contrato72×50, estender escarpa, substituir periferia aberta por floresta profunda com pedras e física contínua derivada da mesma geometria; alinhar saída cidade e terreno. Remover/realocar somente obstáculos afetados que invadam acessos.
2. Interior: reservar clareiras de pomar/pasto no planner, reagrupar bosque, aplicar cercados e vegetação a partir da arte aprovada; refinar caminhos e escala relativa das massas. Comunicar mudanças de ancôras ao owner creator.
3. Integração: creator une componentes preservando funções; atualizar enquadramento geral para mostrar contorno completo; capturar em Unity e comparar referência/regiões.
4. Validação: testes geométricos e consultas reais de física demonstram fechamento de fronteira e acesso aos marcos; revisão independente visual/arquitetural, corrigir findings antes da entrega.

# /speckit.tasks

- [x] T1 Referência preservada e responsabilidades acordadas.
- [x] T2 Contorno natural contínuo com física alinhada, envelope ampliado e saída leste preservada.
- [x] T3 Pomar, pasto, bosque, caminhos e marcos reproduzem regiões da imagem; diferenças de acabamento registradas.
- [x] T4 Unity gera cena sem erros de compilação; captura geral e regionais revisadas.
- [x] T5 480 consultas de bloqueio do perímetro e 19 rotas PASS com collider real, incluindo portal leste; movimento humano não simulado.
- [x] T6 IDs e membros dos registries preservados; Zrix e71TreeNodes mantidos (68noTreeRegistry). Sete seleções reais PASS; diferenças de hooks legados e riscos registrados no relatório.
- [x] T7 Relatório, imagens e cenário humano entregues; sem alegar equivalência pixel-a-pixel ou aceite humano.

## Evidência e riscos
Gates proporcionais pela SPEC_VALIDATION_MATRIX_MASTER: compile pela geração/testes Unity, testes geométricos úteis para fronteira, consultas de physics/interaction e screenshots. Testes antigos com coordenadas64×44 devem ser avaliados quanto a contrato atual; não afrouxar assertions para mascarar passagem. Provar limites e rotas separadamente. Human walk/visual acceptance é pendente até execução real. Cena pode ser gerada com referências não determinísticas de hooks legados sem efeito: registrar diffs concretos. Medir objetos/colliders/performance de fronteira para evitar milhares de novos comportamentos runtime.

Relatório final deve distinguir implementado, divergência visual da keyart, teste executado e NOT RUN. Sem declarar pronto apenas por compile.

## Refinement — building proportion and enclosure language (2026-09-09)

Human review found the south building row too small and questioned the isolated wall-like border. Recalibrate
coop, barn and both processing sheds from visible opaque pixels against the farmer, preserving interaction roots
and IDs. Keep the main house and greenhouse scale unless the same-camera comparison proves drift. Replace the
uniform perimeter stone strip with sparse regional rock/undergrowth accents while retaining the shared physical
boundary. Make orchard fencing read as an intentional, permeable orchard enclosure, distinct from permanent
world bounds. Update the keyart fidelity rule and Farm composition validator so positive Transform scale alone
cannot pass proportion review.

## Refinement — compact southern farm nucleus (2026-09-09)

Human review still perceived the buildings as small after their opaque extents were corrected. Preserve the
approved uniform sprite scale and solve the remaining composition problem: reduce the four-building center span,
replace per-building fence fragments with one shared rural frontage, use non-blocking work props to connect the
silhouettes, and narrow the southern path. Keep doors and interaction roots unobstructed. The composition contract
must reject the former dispersed anchors as well as miniature sprite extents. Regenerate through the Unity scene
creator and re-run physical routes, perimeter blocking, interaction selection and same-camera visual review.

## Refinement — measured keyart delta, paths and physical affordances (2026-09-09)

Human review found the integrated scene still materially different from the approved keyart and the main house too
small. Increase the farmhouse as a single uniform visual and enlarge its walk-in footprint, walls, reveal trigger and
tilling mask from the same contract. Redraw the path centerlines with narrower widths and additional control points so
the network reads as winding farm tracks rather than broad rectilinear roads; retain Point-filtered source pixels and
avoid non-uniform scaling. Audit the visible affordances against actual physics. Orchard and pasture fences must block
along visible runs while keeping their gates open; coop, barn, both processing huts, house and greenhouse must have
solid structural bases with clear frontal approaches; bridge and paths remain traversable; water and natural perimeter
remain blocking. Record those expectations from live player-collider queries instead of relying only on component counts.

## Refinement — Aseprite finishing pilot

User authorized preparing the Aseprite workflow before editing and opening the installed editor. Create a concise
`aseprite-authoring` skill in the canonical harness with conditional CLI/Lua guidance, examples and a work-order
template; generate Codex copies. Prefer the installed CLI over a new MCP unless a demonstrated capability is missing.
Produce editable layered art under `dev/art/aseprite/` and keep original input PNGs intact. Pilot a dirt-track texture
with coherent pixel clusters instead of the tiny tinted sand texture in `CreateMvpFarmScene.PaintPath`; retain the
path geometry, add subtle deterministic variants, and import candidate art through Unity Editor API. Root owns the
road generator and asset import, including `Editor/Art/FarmPathPalette.cs` which maps world cells into the authored
64x64 atlas at 32 pixels/unit and persists its eight-pixel slices as Tile subassets. The fence owner changes only `FarmSettlementVisualComposer` so visible rails follow
`FarmSettlementPhysicsContract` segments and open gates. No new gameplay systems, IDs, save changes or Town/Cave edits.
Validate the actual Aseprite roundtrip and inputs, inspect native/gameplay scale, regenerate FarmScene through Editor
API and run the existing live physical/interaction probes. Do not equate image export or passing physics with visual
acceptance. Record the remaining macro-composition differences separately from this finishing pilot.

## Refinement — delegated fidelity rebuild v4 (2026-09-09)

Human explicitly rejects the current similarity and authorizes rebuilding farm terrain, roads, landmarks and
composition toward a 95% visual target, with delegated implementation, independent audits and staged images.
Keep that target an art acceptance objective, not an unsupported computed percentage. Compare normalized
landmark silhouettes/positions, terrain topology, mass distribution, materials and decorative detail independently.
Exclude intentionally absent initial crops from likeness judgments, while retaining real empty farm functionality.

Implementation owner farm_rebuild_v4 may revise the previously listed farm composition/layout/physics contracts,
generators and associated factual validation coordinates. Root owns source-art extraction through Aseprite,
new PNGs/aseprite sources under dev/art/aseprite/keyart-v4 and Generated/World, capture/probe/test integration,
spec/report and Unity execution. Reviewer remains read-only. Do not bake the whole keyart as a background,
remove interactions, fake motion, relocate live save IDs or hide differences by camera zoom/cropping.
Individual approved reference objects may be extracted into true-alpha layered source assets; validate their
silhouettes, support and collisions in game. Use animation skills only when authored motion is in scope.

Stage1: measured layout/scale changes and northern mass reconstruction, followed by a same-camera Unity capture.
Stage2: material/landmark detail and missed topology corrections from audit, followed by fresh images.
Stage3: physical route/perimeter/interactions, relevant EditMode and final independent visual review. Iterate
on concrete failures, reusing valid evidence only over unchanged inputs. Root shows actual captures as work proceeds.
Outputs under docs/validation/farm_keyart_v4. No global PASS or 95% claim solely from code/tests.

### V4 measured implementation decisions

The authorized rebuild extends only the western envelope to 76×50, x[-40,36], y[-25,25], so the pasture can match
the reference without global zoom. Grid origin tiles become (-8,-3); tile(0,0) remains at the same world position,
and the former eastern edge remains tile67. This supersedes the original 72×50 envelope for this refinement.
The root owns regression tests for existing soil identity, actual collider probes and capture framing.

House/greenhouse exteriors use individually isolated reference images with the existing walk-in/roof reveal and
door anchors. Fountain respawn is a child south of the physical basin; its ID and bootstrap binding remain.
Greenhouse tilling permits only complete cells inside the useful interior between structural bases.

Road geometry uses four-pixel atlas slices at32PPU on0.125u cells; native texture pixel density is preserved while
reducing stair-stepping on curves. Water uses16px atlas slices at16PPU on1u cells, replacing a full high-resolution
texture compressed into each cell. These farm-only tile assets are persisted through Editor API.

Stage4 review stabilized the forest but rejected remaining hard path margins and secondary landmark silhouettes.
Stage5 isolates four southern exteriors and dock/boat from the reference. The existing dock entry and fishing ID
remain fixed; its visible deck is enlarged with the lake collision notch to x[12.06,15.06], y[-10.43,-7.3],
intersecting the sloped west shoreline at(12.06,-8.375) to keep the polygon simple.
The boat remains over blocked water. Tests must prove the newly exposed deck corner is open and the water beyond
the new front remains blocked. Existing processing-root scale is preserved while child exteriors change.
Path finishing runs once over the union, preserving opaque centers and applying short world-aligned clustered
margins. It does not add colliders or change save state. Lake vegetation and courtyard planting remain decorative.

Stage8 refines source-derived undergrowth/flowers and the material visible between southern trees. Rebuild
the local river bend below the unchanged bridge toward x29 at y[-2,-5.5], with its tail inside the existing lake;
visual water and both solid bank polygons must follow the same footprint. Add the source-derived confluence
cascade within that water geometry. Preserve the lake/deck contour, existing IDs, farming and town/cave exits.

Stage10 addresses the stage9 visual rejection through native-scale regional planting along the northern cliff,
well clearing and southern forest pockets, plus source-derived fence rails. Use the fixed camera's18.2857px/u
to calibrate individual source silhouettes; avoid shrinking new motifs to arbitrary legacy asset heights.
Preserve the exact functional enclosure runs/gates, walking approaches and shared physical boundary. Any new
visible blocking fence must have a matching shared physical segment and navigation evidence.

## Refinement — physical affordances and ambient/door animation (2026-09-10)

The user explicitly requests a detailed pass on component collision, animated building doors, the visible
house interior on entry, moving lake water, the Farm-to-Town gate and the cave entrance. Continue visual fidelity
work; this is an extension of the authorized delivery, not a replacement objective.

### Contracts

- Reuse the existing main-house walk-in interior, HouseDoorInteractable and RoofRevealController. Author a
  separate true-alpha door leaf/open threshold from the approved exterior; opening must visibly progress,
  with the physical blocker released at the clear pose. Closing cannot trap an actor in the doorway.
- Reveal the house interior when the player's feet enter its useful interior, ignoring the interaction
  sensor's proximity through walls. Restore the exterior after leaving. Preserve roof/leaf visibility ownership;
  an open leaf cannot reappear just because interior renderers are enabled.
- Bed/chest and other visibly solid furnishing bases block passage while retaining usable approaches.
  Decorative flowers/low growth remain non-blocking; do not add colliders to every decoration.
- Barn/coop presentation should follow their existing interaction/action. Do not invent animal economies,
  interiors or transition destinations. Inspect the existing greenhouse entrance before adding any blocker.
- Lake/rivers use a subtle authored looping pixel animation; shore, lilies, boat and dock stay registered.
  Use fixed canvas/palette/PPU and seamless frame timing. Water animation never changes collision geometry.
- Farm-to-Town and cave entrance must have readable opening/support and a reachable interaction region.
  Preserve destination IDs, cave stable-run behavior and Town ownership. Change only Farm-side presentation,
  collision and wiring; this authorizes no procedural-cave or saved-state modifications.

### Ownership and plan

1. Root records current affordances and owns Farm generator/wiring, gate/cave corrections, capture and evidence.
2. Runtime worker owns World/HouseDoorInteractable.cs, World/RoofRevealController.cs, a small opt-in presentation
   helper only if justified, and focused World EditMode tests. Coordinate shared API with the Town task;
   preserve existing configuration/default behavior for its unmodified wiring.
3. Art worker uses pixel-art-animator + aseprite-authoring for layered door/water candidates under
   dev/art/aseprite/keyart-v4 and new Generated/World assets. Reopen frames/tags/durations; deliver timed preview.
4. Root may author a small data-driven TileBase animation asset type under World/Rendering and its Editor
   authoring helper under Editor/Art, reusing native Tilemap animation instead of per-tile Update scripts.
5. Generate one integrated Farm scene, inspect timed playback and actual interior/door sequence, then verify
   opening/closing occupancy, furniture collision, gate/cave approach and unchanged water/border blocking.

### Tasks and acceptance

- [x] F1 Current house/interior presentation defect mapped: invisible Leaf/Threshold and proximity reveal.
- [x] F2 Door art, motion/blocker timing and safe closing integrated; useful interior revealed on actual entry. Stage12 functional PASS; exterior actor occlusion remains a visual finding to fix.
- [x] F3 Solid furnishing bases and Farm-side gate/cave affordances corrected and physically probed.
- [x] F4 Authored water frames loop in Unity with stable shore alignment; timed evidence displayed.
- [ ] F5 Independent review, focused regressions and actual gameplay sequence recorded. Static screenshots
  alone do not certify motion, transitions or human acceptance.

Stage12 wiring corrections: preserve the existing built-in Player tag contract in CreatePlayer so both occupancy
and feet-reveal identify the actual actor. Explicit serialized collider references on FarmSceneRuntimeBootstrap
register the19 authored prop bases as non-arable using the existing rectangle helper; these are scene references,
never save DTO fields. The optional motion probe's temporary body driver lives in Diagnostics under UNITY_EDITOR
because Unity disallows attaching Editor-assembly behaviours; it is not shipped in a player build.

Human follow-up on the moving preview identifies apparent sinking/sliding. Correct exterior Y sorting at the
door's ground contact without revealing through a proximity sensor. The active project uses legacy input;
do not migrate ProjectSettings/packages solely to run a demo. A bounded PlayerWalkAnimator correction may use
the cached actual Rigidbody2D velocity when its PlayerController is disabled for controlled movement, retaining
that physical facing when stopped. Normal enabled-controller input/attack behavior stays unchanged. Validate
this fallback with focused tests and the actual house traversal; no injected sprite/facing or manual input claim.

## Refinement — bounded visual round 14

The user requests a latest-result HTML first, then the economical iteration strategy. Baseline: stage13
integrated capture. Its motion probe failed the walking-frame criterion; the newer animator fallback is
an unvalidated candidate, not accepted production behavior. Do not expand that candidate in this visual round.

Plan/tasks: (1) publish `progress-review.html` with actual reference/history and evidence limits;
(2) review the three largest differences; (3) revise only existing path ribbons and decorative groupings
through FarmDecorationPlanner/FarmLandscapeVisualComposer, preserving physical trees, IDs, working areas,
water and all collision contracts; (4) generate one integrated capture, inspect before/after and run the
affected decoration/navigation checks; (5) update the same report and latest HTML. Two attempts without
visible improvement trigger a method review, not an unsupported acceptance claim. The lake contour remains
an explicit next-round finding because it couples art to collision. Existing art is reused; position-only
changes do not require manufacturing another image. Root owns integration/Unity, one executor supplies the
bounded candidate, and one reviewer inspects actual results. Apply `.claude/rules/visual-iteration-budget.md`.

## Refinement — ambient details and spacing v15

Human authorizes light non-blocking shrubs, a slightly larger dock, more space between the four
southern buildings, five-frame fountain motion, animated river-to-lake cascade, and a fish jumping
and splashing every20 seconds. Preserve IDs, soil coordinates, crops-empty startup and physical trees.

Plan/tasks:
- K1 Add small source-derived ground-cover accents through existing guarded decoration placement;
  no collider or gameplay component on these shrubs, no cover over doors or working fields.
- K2 Enlarge the dock uniformly10% about its existing land-contact anchor. Expand only its matching
  walkable lake notch; retain fishing ID/anchor, boat over blocked water and contiguous lake polygon.
- K3 Separate the southern building roots at X=-13.1,-7.2,-1.0,4.9 (sameY/IDs). Their existing child
  solids/interactions move with the roots; existing dependent props use the shared layout constants.
- K4 Author preserved-source Aseprite candidates: fountain5frames, confluence5frames, fish jump+fall+
  splash sequence. Show timed art preview before integration; no new approval round is required.
- K5 Use native AnimationClip/AnimatorController for ambient playback, with stable sprite canvas,
  PPU and pivots. Fish is decorative, invisible between jumps, period20s of running game time;
  no fishing reward, AI, physics, save field or new runtime manager. Continuous fountain/cascade loop.
- K6 One integrated capture; inspect proportions and art at game scale. Validate native clip
  timing/frame contracts, affected layout/decoration/navigation tests and live approaches/solids.
  Record animation samples honestly; editable GIF is not Unity playback proof.

Ownership: root edits FarmLevel1LayoutContract, FarmSceneSpatialContract, CreateMvpFarmScene,
FarmSettlementVisualComposer, relevant existing farm tests/probes and docs. Art executor owns
`dev/art/aseprite/keyart-v4/ambient-v15/` artwork. Independent wiring executor owns candidates in
its `wiring/` subdirectory and FarmAmbientAnimationAuthoring Editor helper after root integration.
Root alone runs Unity after shared-source freeze. No Town/Skills/PlayerWalkAnimator changes.
Output: `docs/validation/farm_keyart_v4/stage15/`, existing comparison HTML plus ambient preview.

V15 execution: K1–K5 integrated; K6 scoped automated gates PASS (33/33 EditMode,18/18 PlayMode captures/routes). See docs/validation/farm_keyart_v4/REPORT.md. Continuous fish observation and human visual approval pending; no global promotion.

V15 follow-up authorized: observe fountain/cascade/fish through normal native Animator playback in
Play Mode, preserving gameplay camera zoom. Root may add an optional ambient-only mode to
FarmPlayModeCaptureSession and an Editor-only FarmAmbientPlaybackCapture helper. Reuse existing
scene/save isolation; skip unrelated layout tests. Record real timestamps and camera pixel crops,
two complete fish jumps about20s apart, null-sprite wait and two fountain/cascade cycles. Deliver
HTML playback; make art/timing corrections only for observed defects. No scene regeneration.

Ambient follow-up completed: real41sPlayModePASS; two8-frame jumps20.000061s apart, fountain/cascade5frames,8idle-null samples; scene/savesunchanged. See ambient_runtime15 metadata/HTML. Human visual acceptance/global spec remains pending.
