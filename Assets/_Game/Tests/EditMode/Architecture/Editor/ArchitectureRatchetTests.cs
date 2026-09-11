using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Architecture
{
    /// <summary>Checks compiled assembly boundaries; source/GUID scans belong to Test-ArchitectureRatchet.ps1.</summary>
    public class ArchitectureRatchetTests
    {
        [Test]
        public void FoundationAssembly_GroupsContractsWithoutEngineReferences()
        {
            var assembly = typeof(CindarsHope.Core.Data.IIdentifiedData).Assembly;
            Assert.That(assembly, Is.SameAs(typeof(CindarsHope.Core.Data.IDataRegistry<>).Assembly));
            Assert.That(assembly.GetName().Name, Is.EqualTo("CindarsHope.Foundation"));
            AssertNoEngineReference(assembly);
        }

        [Test]
        public void GameplayAssembly_GroupsDecisionsWithoutEngineReferences()
        {
            var assembly = typeof(CindarsHope.UI.Routing.GameplayShortcutDecision).Assembly;
            Assert.That(assembly, Is.SameAs(typeof(CindarsHope.NPC.NpcShopInteractionSession).Assembly));
            Assert.That(assembly, Is.SameAs(typeof(CindarsHope.Gameplay.Narrative.NarrativeIds).Assembly));
            Assert.That(assembly.GetName().Name, Is.EqualTo("CindarsHope.Gameplay"));
            AssertNoEngineReference(assembly);
        }

        private static void AssertNoEngineReference(Assembly assembly)
        {
            Assert.That(assembly.GetReferencedAssemblies()
                .Any(reference => reference.Name != null
                    && reference.Name.StartsWith("UnityEngine", StringComparison.Ordinal)),
                Is.False, "Pure contracts and decisions must remain independent from UnityEngine assemblies.");
        }
    }
}
