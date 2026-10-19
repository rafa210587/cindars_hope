using System.Linq;
using CindarsHope.Farm.Animals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>Checks imported presentation contracts, including shared-species identity isolation.</summary>
    public sealed class HerdMotionProfileTests
    {
        [TestCase("animal_cow", "cow")]
        [TestCase("animal_sheep", "sheep")]
        [TestCase("animal_goat", "goat")]
        public void ImportedProfile_HasDistinctCompleteSequencesAtNativeScale(string id, string name)
        {
            string path = $"Assets/_Game/Art/Generated/World/animals/herd_motion_v19/{name}_v19.png";
            var profile = AssetDatabase.LoadAssetAtPath<AnimalMotionProfileSO>(
                $"Assets/_Game/Data/Animals/AnimalMotionProfile_{id}.asset");
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.AnimalDataId, Is.EqualTo(id));
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Multiple));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.mipmapEnabled, Is.False);

            var sequences = new[] {
                profile.GetFrames(AnimalMotionMode.Idle, AnimalFacingDirection.Down),
                profile.GetFrames(AnimalMotionMode.Walking, AnimalFacingDirection.Down),
                profile.GetFrames(AnimalMotionMode.Walking, AnimalFacingDirection.Up),
                profile.GetFrames(AnimalMotionMode.Walking, AnimalFacingDirection.Side),
                profile.GetFrames(AnimalMotionMode.Peck, AnimalFacingDirection.Down),
                profile.GetFrames(AnimalMotionMode.Rest, AnimalFacingDirection.Down)
            };
            Assert.That(sequences.Select(sequence => sequence.Length), Is.EqualTo(new[] { 2, 4, 4, 4, 3, 2 }));
            var frames = sequences.SelectMany(sequence => sequence).ToArray();
            Assert.That(frames.Distinct().Count(), Is.EqualTo(19), "Missing or accidentally shared poses.");
            for (int index = 0; index < frames.Length; index++)
            {
                Assert.That(frames[index], Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(frames[index]), Is.EqualTo(path), "Cross-species sprite wiring.");
                Assert.That(frames[index].name, Does.StartWith($"{name}_v19_{index:00}_"));
                Assert.That(frames[index].rect.size, Is.EqualTo(new Vector2(48, 48)));
                Assert.That(frames[index].pivot, Is.EqualTo(new Vector2(24, 8)));
                Assert.That(frames[index].pixelsPerUnit, Is.EqualTo(32f));
            }
        }
    }
}
