using CindarsHope.EditorTools;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Tooling
{
    public class PlayModeStartSceneSetterTests
    {
        [Test]
        public void CommandLineTestRun_IsNotOverriddenByFarmStartScene()
        {
            Assert.That(
                PlayModeStartSceneSetter.IsCommandLineTestRun(
                    new[] { "Unity.exe", "-batchmode", "-runTests", "-testPlatform", "PlayMode" }),
                Is.True);
        }

        [Test]
        public void RegularEditorLaunch_KeepsConfiguredStartSceneBehavior()
        {
            Assert.That(
                PlayModeStartSceneSetter.IsCommandLineTestRun(new[] { "Unity.exe", "-projectPath", "." }),
                Is.False);
            Assert.That(PlayModeStartSceneSetter.IsCommandLineTestRun(null), Is.False);
        }
    }
}
