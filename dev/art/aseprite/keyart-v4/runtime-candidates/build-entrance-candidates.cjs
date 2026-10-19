const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const root = path.resolve(__dirname, '../../../../../');
const target = name => path.join(root, name);
function method(text, signature) {
  const start = text.indexOf(signature);
  if (start < 0) throw new Error('Missing method: ' + signature);
  const open = text.indexOf('{', start);
  let depth = 1, end = open + 1;
  while (depth) { if (text[end] === '{') depth++; if (text[end] === '}') depth--; end++; }
  return text.slice(start, end);
}
function replaceOnce(text, before, after) {
  if (text.split(before).length !== 2) throw new Error('Non-unique patch anchor: ' + before);
  return text.replace(before, after);
}
function patch(name, changes) {
  const source = fs.readFileSync(target(name), 'utf8').replace(/\r\n/g, '\n');
  let output = '*** Begin Patch\n*** Update File: ' + name + '\n';
  for (const [before, after] of changes(source)) {
    if (!source.includes(before)) throw new Error('Missing baseline');
    output += '@@\n' + before.split('\n').map(x => '-' + x).join('\n') + '\n'
      + after.split('\n').map(x => '+' + x).join('\n') + '\n';
  }
  output += '*** End Patch\n';
  fs.writeFileSync(path.join(__dirname, path.basename(name) + '.patch.txt'), output);
  return { source: name, normalizedSha256: crypto.createHash('sha256').update(source).digest('hex') };
}
const baselines = [];
baselines.push(patch('Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs', text => {
  const portal = method(text, '        private static void CreateScenePortal(');
  const visualStart = portal.indexOf('            if (!ScaleProfileLibrary.AttachApplicator');
  const visualEnd = portal.indexOf('            var portal = portalObject.AddComponent<ScenePortal>();');
  const portalNew = portal.slice(0, visualStart) + `            // The measured boundary fence is the visible gate. Keep the interaction root unscaled;
            // a generic colored portal rectangle would conceal the physical support and road.
            portalObject.transform.localScale = Vector3.one;
            var region = FarmEntrancePhysicsContract.TownInteractionTrigger;
            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.offset = region.Center - (Vector2)position;
            collider.size = region.Size;

` + portal.slice(visualEnd);
  const cave = method(text, '        private static void CreateCaveEntrance()');
  let caveNew = replaceOnce(cave,
    'ApplyKnownOpaqueHeightScale(caveVisual.transform, caveSprite, 6.75f, 419f / 435f);',
    'ApplyKnownOpaqueHeightScale(caveVisual.transform, caveSprite, FarmEntrancePhysicsContract.CaveOpaqueHeight,\n                    FarmEntrancePhysicsContract.CaveOpaqueHeightPixels / FarmEntrancePhysicsContract.CaveCanvasHeightPixels);');
  caveNew = replaceOnce(caveNew,
    '            trigger.size = new Vector2(FarmLevel1LayoutContract.CaveMouthWidth, FarmLevel1LayoutContract.CaveMouthHeight);',
    `            var region = FarmEntrancePhysicsContract.CaveInteractionTrigger;
            trigger.offset = region.Center - (Vector2)entrance.transform.position;
            trigger.size = region.Size;

            foreach (var measuredBase in FarmEntrancePhysicsContract.CaveSideBases)
            {
                var body = new GameObject("Solid_" + measuredBase.Id);
                body.transform.SetParent(entrance.transform, false);
                body.transform.position = measuredBase.Center;
                CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                    body, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.WorldSolid);
                var bodyCollider = body.AddComponent<BoxCollider2D>();
                bodyCollider.isTrigger = false;
                bodyCollider.size = measuredBase.Size;
            }`);
  return [[portal, portalNew], [cave, caveNew]];
}));
baselines.push(patch('Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs', text => {
  const old = method(text, '        private static void CreateEnclosureFences(Transform parent)');
  const opening = old.indexOf('            var source');
  const loop = old.indexOf('            foreach (var segment in FarmSettlementPhysicsContract.AllFenceSegments)');
  const loopBodyStart = old.indexOf('{', loop) + 2;
  const loopBodyEnd = old.lastIndexOf('            }');
  const body = old.slice(loopBodyStart, loopBodyEnd).split('\n').map(x => x.startsWith('    ') ? x.slice(4) : x).join('\n');
  const replacement = `        private static void CreateEnclosureFences(Transform parent)
        {
            foreach (var segment in FarmSettlementPhysicsContract.AllFenceSegments)
                CreateFenceRun(parent, segment);
        }

        internal static void CreateFenceRun(Transform parent, FarmSolidRect segment)
        {
` + old.slice(opening, loop) + body + '        }';
  return [[old, replacement]];
}));
baselines.push(patch('Assets/_Game/Scripts/Editor/Art/FarmPerimeterVisualComposer.cs', text => {
  const old = method(text, '        private static void CreateOutskirts(');
  let next = replaceOnce(old, '            var fence = WorldSpriteLibrary.Prop("fence");\n', '');
  next = replaceOnce(next, '                collider.SetPath(0, band);', `                collider.SetPath(0, band);
                if (gate)
                {
                    FarmSettlementVisualComposer.CreateFenceRun(parent, FarmEntrancePhysicsContract.TownGateVisualRun);
                    continue;
                }`);
  next = replaceOnce(next, 'var baseSpacing = gate ? 0.65f : northEscarpment ? 1.35f : BoundaryBaseSpacing;', 'var baseSpacing = northEscarpment ? 1.35f : BoundaryBaseSpacing;');
  next = replaceOnce(next, 'if (!northEscarpment || gate)', 'if (!northEscarpment)');
  next = replaceOnce(next, 'var baseSprite = gate ? fence : useRock ? rock : bush;', 'var baseSprite = useRock ? rock : bush;');
  next = replaceOnce(next, 'var baseKind = gate ? "Gate" : useRock ? "Rock" : "Undergrowth";', 'var baseKind = useRock ? "Rock" : "Undergrowth";');
  next = replaceOnce(next, 'var height = gate ? 1.1f : 0.8f + variation * 0.65f;', 'var height = 0.8f + variation * 0.65f;');
  next = replaceOnce(next, 'if (!gate && !useRock && point.y < -18f)', 'if (!useRock && point.y < -18f)');
  next = replaceOnce(next, '                if (gate) continue;\n', '');
  return [[old, next]];
}));
fs.writeFileSync(path.join(__dirname, 'entrance-candidate-baselines.json'), JSON.stringify(baselines, null, 2));
console.log('Created three targeted candidate patches; no Assets files changed.');
