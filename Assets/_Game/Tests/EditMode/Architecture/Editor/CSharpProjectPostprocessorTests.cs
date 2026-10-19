using System.Xml.Linq;
using System.Linq;
using CindarsHope.Editor.AssetPostprocessing;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Architecture
{
    public class CSharpProjectPostprocessorTests
    {
        private const string ProjectXml =
            "<Project><ItemGroup><Reference Include=\"System\" /></ItemGroup></Project>";
        private const string RuntimeProjectName = "GameRuntime";
        private const string EditorProjectName = RuntimeProjectName + "-Editor";

        [Test]
        public void NonEditorProject_ReturnsOriginalContent()
        {
            string result = CSharpProjectPostprocessor.OnGeneratedCSProject(
                RuntimeProjectName + ".csproj",
                ProjectXml);

            Assert.That(result, Is.EqualTo(ProjectXml));
        }

        [Test]
        public void EditorProject_AddsRuntimeAssemblyReference()
        {
            string result = CSharpProjectPostprocessor.OnGeneratedCSProject(
                EditorProjectName + ".csproj",
                ProjectXml);
            XDocument document = XDocument.Parse(result);

            XElement reference = document.Root?
                .Element("ItemGroup")?
                .Elements("Reference")
                .FirstOrDefault(element => (string)element.Attribute("Include") == RuntimeProjectName);

            Assert.That(reference, Is.Not.Null);
            Assert.That((string)reference.Element("Private"), Is.EqualTo("False"));
            StringAssert.EndsWith(RuntimeProjectName + ".dll", (string)reference.Element("HintPath"));
        }

        [Test]
        public void EditorProject_TransformationIsIdempotent()
        {
            string first = CSharpProjectPostprocessor.OnGeneratedCSProject(
                EditorProjectName + ".csproj",
                ProjectXml);
            string second = CSharpProjectPostprocessor.OnGeneratedCSProject(
                EditorProjectName + ".csproj",
                first);
            XDocument document = XDocument.Parse(second);

            int runtimeReferences = document.Descendants("Reference")
                .Count(element => (string)element.Attribute("Include") == RuntimeProjectName);

            Assert.That(runtimeReferences, Is.EqualTo(1));
        }
    }
}
