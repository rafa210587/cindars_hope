"""Offline preparation only: reads Assets, writes candidates in this directory."""
from pathlib import Path
import hashlib,json
ROOT=Path.cwd(); OUT=Path(__file__).resolve().parent
manifest=[]
def sha(b): return hashlib.sha256(b).hexdigest()
def load(rel): return (ROOT/rel).read_text(encoding='utf-8-sig')
def replace(s,a,b):
    if s.count(a)!=1: raise RuntimeError('Expected one anchor: '+a[:100])
    return s.replace(a,b,1)
def save(rel,s):
    p=ROOT/rel; candidate=OUT/(p.name+'.txt')
    original=p.read_bytes() if p.exists() else None
    candidate.write_text(s,encoding='utf-8',newline='\n')
    manifest.append(dict(path=rel,candidate=candidate.name,before=sha(original) if original else None,after=sha(candidate.read_bytes())))

rel='Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs'
s=load(rel)
s=replace(s,'CreateFishingSpot(inventoryManager);','CreateFishingSpot(inventoryManager, playerTransform);')
s=replace(s,'private static void CreateFishingSpot(InventoryManager inventoryManager)','private static void CreateFishingSpot(InventoryManager inventoryManager, Transform playerTransform)')
a=s.index('            var blockingCollider = fishingObject.AddComponent<BoxCollider2D>();',s.index('private static void CreateFishingSpot'))
b=s.index('            var fishingSpot = fishingObject.AddComponent<FishingSpot>();',a)
s=s[:a]+s[b:]
a=s.index('            serializedFishing.FindProperty("_edgeInteractionOuterHalfExtents")',a)
b=s.index('            // The north gangway',a)
s=s[:a]+'''            serializedFishing.ApplyModifiedPropertiesWithoutUndo();
            var body = playerTransform.GetComponent<BoxCollider2D>();
            var priorScale = playerTransform.localScale;
            var priorSize = body.size;
            Bounds runtimeBody;
            try
            {
                // Awake applies this profile in Play, after the creator's temporary visual scale.
                var scale = playerTransform.GetComponent<VisualScaleApplicator>();
                if (scale != null) scale.Apply();
                Physics2D.SyncTransforms();
                runtimeBody = body.bounds;
            }
            finally
            {
                playerTransform.localScale = priorScale;
                body.size = priorSize;
                Physics2D.SyncTransforms();
            }
            const float clearance = 0.12f;
            const float stanceHeight = 0.4f;
            var footBottomOffset = runtimeBody.min.y - playerTransform.position.y;
            var stanceBottom = FarmSceneSpatialContract.DockDeckSouthY + clearance - footBottomOffset;
            var availableHalfWidth = (FarmSceneSpatialContract.DockDeckEastX - FarmSceneSpatialContract.DockDeckWestX) * 0.5f
                - runtimeBody.extents.x - clearance;
            if (availableHalfWidth <= 0f) throw new System.InvalidOperationException("Player body cannot fit dock tip.");
            var stance = new GameObject("FishingStanceZone");
            stance.transform.SetParent(fishingObject.transform, false);
            stance.transform.position = new Vector3(FarmLevel1LayoutContract.FishingSpotX, stanceBottom + stanceHeight * 0.5f, 0f);
            var stanceCollider = stance.AddComponent<BoxCollider2D>();
            stanceCollider.isTrigger = true;
            stanceCollider.size = new Vector2(Mathf.Min(0.55f, availableHalfWidth) * 2f / fishingObject.transform.lossyScale.x,
                stanceHeight / fishingObject.transform.lossyScale.y);
            fishingSpot.ConfigureStanceZone(stanceCollider);
            EditorUtility.SetDirty(fishingSpot);

''' +s[b:]
s=replace(s,'            // spec_farm_scene_keyart_visual_corrections_v1: anel de flores brancas em volta da', '''            var depthGroup = new GameObject("FountainDepthGroup");
            depthGroup.transform.SetParent(fonteRoot.transform, false);
            var sortingGroup = depthGroup.AddComponent<UnityEngine.Rendering.SortingGroup>();
            sortingGroup.sortingLayerName = "World";
            sortingGroup.sortingOrder = 0;
            fonteVisual.transform.SetParent(depthGroup.transform, true);

            // spec_farm_scene_keyart_visual_corrections_v1: anel de flores brancas em volta da''')
s=replace(s,'            CreateContractSolidBase(fonteRoot.transform, "SolidBasin", FarmSettlementPhysicsContract.FountainBasin);', '''            CreateContractSolidBase(fonteRoot.transform, "SolidBasin", FarmSettlementPhysicsContract.FountainBasin);
            foreach (var foot in FarmSettlementPhysicsContract.FountainColumnFeet)
                CreateContractSolidBase(fonteRoot.transform, foot.Id, foot);''')
save(rel,s)

rel='Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs';s=load(rel)
s=replace(s,'        // The narrow notch opens only the deck', '''        public const float DockDeckWestX = 11.91f;
        public const float DockDeckEastX = 15.21f;
        public const float DockDeckSouthY = -10.798f;

        // The narrow notch opens only the deck''')
s=replace(s,'new Vector2(11.91f, -10.798f)', 'new Vector2(DockDeckWestX, DockDeckSouthY)')
s=replace(s,'new Vector2(15.21f, -10.798f)', 'new Vector2(DockDeckEastX, DockDeckSouthY)')
save(rel,s)

rel='Assets/_Game/Scripts/Farm/Scene/FarmSettlementPhysicsContract.cs';s=load(rel)
s=replace(s,'        public static readonly FarmSolidRect CentralWell', '''        // Native source pixel-edge rectangles around the bottom stone feet, not column height.
        // Measured in fountain_keyart_v3.png; contact-v16/column-contact-measurements.json.
        public static IReadOnlyList<FarmSolidRect> FountainColumnFeet { get; } = new[]
        {
            FountainPixelFoot("FountainColumnNW", 19f, 108f, 16f, 8f),
            FountainPixelFoot("FountainColumnNE", 126f, 107f, 18f, 9f),
            FountainPixelFoot("FountainColumnSW", 19f, 47f, 18f, 8f),
            FountainPixelFoot("FountainColumnSE", 126f, 47f, 18f, 8f)
        };
        private static FarmSolidRect FountainPixelFoot(string id, float x, float y, float width, float height)
        {
            const float unitsPerPixel = 7f / 137f;
            var center = new Vector2(FarmLevel1LayoutContract.FonteAnchorX, FarmLevel1LayoutContract.FonteAnchorY)
                + (new Vector2(x + width * 0.5f, y + height * 0.5f) - new Vector2(85f, 43f)) * unitsPerPixel;
            return new FarmSolidRect(id, center, new Vector2(width, height) * unitsPerPixel);
        }
        public static readonly FarmSolidRect CentralWell''')
save(rel,s)
rel='Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs';s=load(rel)
s=replace(s,'            RegisterBlockedSolidRect(zones, grid, FarmSettlementPhysicsContract.FountainBasin);','''            RegisterBlockedSolidRect(zones, grid, FarmSettlementPhysicsContract.FountainBasin);
            foreach (var foot in FarmSettlementPhysicsContract.FountainColumnFeet) RegisterBlockedSolidRect(zones, grid, foot);''')
save(rel,s)
for rel in ['Assets/_Game/Scripts/Editor/Art/FarmAmbientAnimationAuthoring.cs','Assets/_Game/Scripts/Editor/Dev/FarmAmbientPlaybackCapture.cs']:
    s=load(rel)
    s=replace(s,'.transform.Find("Visual")','.transform.Find("FountainDepthGroup/Visual")')
    save(rel,s)

rel='Assets/_Game/Scripts/Editor/Dev/FarmPlayModeCaptureSession.cs';s=load(rel)
s=replace(s,'        private static FarmAmbientPlaybackCapture ambientProbe;', '        private static FarmAmbientPlaybackCapture ambientProbe;\n        private static FarmContactCapture contactProbe;')
s=replace(s,'            public FarmAmbientPlaybackCapture.Evidence ambient;', '            public FarmAmbientPlaybackCapture.Evidence ambient;\n            public bool contactOnlyRequested, contactStarted, contactEvaluated;\n            public FarmContactCapture.Evidence contact;')
s=replace(s,'                ambientOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_AMBIENT") == "1"', '                ambientOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_AMBIENT") == "1",\n                contactOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_CONTACT") == "1"')
s=replace(s,'            Persist();\n            Attach();\n            try', '''            if (session.contactOnlyRequested)
            {
                session.motionRequested = false;
                session.ambientOnlyRequested = false;
                session.limitations = "Controlled-body contact and live fishing selection only. No successful cast, input, complete circulation or human visual acceptance claimed; inspect contact.method.";
            }
            Persist();
            Attach();
            try''')
s=replace(s,'session.stage = session.ambientOnlyRequested ? "ambient" : "capturing";', 'session.stage = session.contactOnlyRequested ? "contact" : session.ambientOnlyRequested ? "ambient" : "capturing";')
s=replace(s,'session.stage != "motion" && session.stage != "ambient"','session.stage != "motion" && session.stage != "ambient" && session.stage != "contact"')
s=s.replace('session.motionRequested || session.ambientOnlyRequested','session.motionRequested || session.ambientOnlyRequested || session.contactOnlyRequested')
# Reuse the existing lifecycle, hash isolation and optional-mode completion structure exactly.
a=s.index('                if (session.stage == "ambient")');b=s.index('                if (session.stage == "motion")',a)
branch=s[a:b].replace('ambient','contact').replace('Ambient','Contact').replace('FarmContactPlaybackCapture(scene, camera, OutputDirectory)','FarmContactCapture(player, camera, OutputDirectory)')
s=s[:a]+branch+s[a:]
a=s.index('        private static void DisposeAmbient()');b=s.index('        private static void Fail(',a)
method=s[a:b].replace('ambient','contact').replace('Ambient','Contact')
s=s[:a]+method+s[a:]
s=s.replace('                DisposeAmbient();\n                Persist();', '                DisposeContact();\n                DisposeAmbient();\n                Persist();')
a=s.index('            if (session.ambientOnlyRequested && session.ambient != null)',s.index('private static void Fail'))
b=s.index('            if (session.motionRequested)',a)
s=s[:a]+s[a:b].replace('ambient','contact').replace('Ambient','Contact')+s[a:]
a=s.index('                if (session.ambientOnlyRequested)',s.index('private static void Complete'))
b=s.index('                bool sortingValid',a)
s=s[:a]+s[a:b].replace('ambient','contact').replace('Ambient','Contact')+s[a:]
s=replace(s,'if (pass) Debug.Log(session.ambientOnlyRequested ?', 'if (pass) Debug.Log(session.contactOnlyRequested ? "Farm contact capture: PASS" : session.ambientOnlyRequested ?')
save(rel,s)

for rel in ['Assets/_Game/Scripts/World/FishingSpot.cs','Assets/_Game/Tests/EditMode/World/FishingStanceZoneTests.cs','Assets/_Game/Scripts/Editor/Dev/FarmContactCapture.cs']:
    p=OUT/(Path(rel).name+'.txt')
    if p.exists(): save(rel,p.read_text(encoding='utf-8'))
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Offline candidates prepared; live Assets untouched.')
