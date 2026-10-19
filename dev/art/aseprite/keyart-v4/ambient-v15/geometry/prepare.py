"""Prepare the authorized v15 geometry delta without touching live Unity inputs."""
from pathlib import Path
import hashlib, json
root = Path.cwd()
out = Path(__file__).resolve().parent
changes = {
'Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs': [
 ('CoopX = -11.9f', 'CoopX = -13.1f'), ('BarnX = -7f', 'BarnX = -7.2f'),
 ('ProcessingAX = -1.7f', 'ProcessingAX = -1.0f'), ('ProcessingBX = 3.3f', 'ProcessingBX = 4.9f')],
'Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs': [
 ('new Vector2(12.06f, -8.375f), new Vector2(12.06f, -10.43f)',
  'new Vector2(11.91f, -8.4875f), new Vector2(11.91f, -10.798f)'),
 ('new Vector2(15.06f, -10.43f), new Vector2(15.06f, -7.3f)',
  'new Vector2(15.21f, -10.798f), new Vector2(15.21f, -7.355f)'),
 ('new Vector2(14.01f, -7.3f), new Vector2(14.01f, -6.9125f)',
  'new Vector2(14.055f, -7.355f), new Vector2(14.055f, -6.87875f)')],
'Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs': [
 ('ApplyKnownOpaqueHeightScale(dock, dockSprite, 117f / 19f, 117f / 137f);',
  'ApplyKnownOpaqueHeightScale(dock, dockSprite, 1.10f * 117f / 19f, 117f / 137f);')],
'Assets/_Game/Tests/EditMode/Farm/FarmSceneNavigationContractTests.cs': [
 ('public void Dock_OpensGangwayAndDeck_WithoutOpeningBoatOrSurroundingLake()\n        {',
  'public void Dock_OpensGangwayAndDeck_WithoutOpeningBoatOrSurroundingLake()\n        {\n'
  '            Assert.That(FarmSceneNavigationRaster.IsBlocked(new Vector2(15.1f, -10.55f)), Is.False, "v15 enlarged deck");\n'
  '            Assert.That(FarmSceneNavigationRaster.IsBlocked(new Vector2(15.45f, -10.55f)), Is.True, "water beyond v15 deck");')],
'Assets/_Game/Scripts/Editor/Validation/FarmPhysicalRouteProbe.cs': [
 ('Check("dock_expanded_front", new Vector2(14.5f, -10.05f), false);',
  'Check("dock_expanded_front", new Vector2(14.5f, -10.05f), false);\n'
  '            Check("dock_v15_front", new Vector2(14.75f, -10.4f), false);')],
}
plants = '''        private static void CreateLowGroundCover(Transform parent)
        {
            // Low visual-only shrubs; existing footprint guards keep fields and approaches readable.
            var sprite = WorldSpriteLibrary.Foliage("undergrowth_keyart_v4");
            var feet = new[] { new Vector2(-32.1f, 5.9f), new Vector2(-24.4f, -0.4f),
                new Vector2(-25.2f, -1.5f), new Vector2(-13.4f, 16.2f),
                new Vector2(-5.6f, 15.3f), new Vector2(-3.2f, 13.1f),
                new Vector2(4.4f, 13.5f), new Vector2(6.3f, 15.1f),
                new Vector2(10.4f, -3.7f), new Vector2(18.8f, -2.5f),
                new Vector2(6.1f, -16.8f), new Vector2(-14.5f, -16.2f) };
            for (var i = 0; i < feet.Length; i++)
                TryRegionalPlant(parent, "LowGroundCover_" + i, sprite, feet[i], 0.65f + i % 3 * 0.1f);
        }

'''
changes['Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs'] = [
 ('CreateFountainGarden(group.transform);', 'CreateFountainGarden(group.transform);\n            CreateLowGroundCover(group.transform);'),
 ('        private static bool TryRegionalPlant(', plants + '        private static bool TryRegionalPlant(')]
manifest = []
for relative, replacements in changes.items():
    source = root / relative
    original = source.read_bytes()
    text = original.decode('utf-8-sig').replace('\r\n', '\n')
    for old, new in replacements:
        assert text.count(old) == 1, (relative, old, text.count(old))
        text = text.replace(old, new)
    candidate = source.name + '.txt'
    (out / candidate).write_text(text, encoding='utf-8')
    (out / (candidate + '.baseline')).write_bytes(original)
    manifest.append(dict(path=relative, originalSha256=hashlib.sha256(original).hexdigest(), candidate=candidate))
(out / 'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
print(f'{len(manifest)} bounded candidates prepared; live Assets unchanged')
