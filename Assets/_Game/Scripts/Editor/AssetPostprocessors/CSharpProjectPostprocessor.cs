using UnityEditor;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System;

namespace CindarsHope.Editor.AssetPostprocessing
{
    /// <summary>
    /// Ensures a generated editor project can reference its runtime counterpart without coupling the
    /// callback to Unity's predefined assembly names.
    /// </summary>
    public class CSharpProjectPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Called after Unity generates the C# project file.
        /// Adds the missing runtime counterpart reference to generated *-Editor projects.
        /// </summary>
        public static string OnGeneratedCSProject(string path, string content)
        {
            string editorProjectName = Path.GetFileNameWithoutExtension(path);
            const string editorSuffix = "-Editor";
            if (string.IsNullOrEmpty(path)
                || string.IsNullOrEmpty(content)
                || string.IsNullOrEmpty(editorProjectName)
                || !editorProjectName.EndsWith(editorSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return content;
            }

            string runtimeAssemblyName = editorProjectName.Substring(
                0,
                editorProjectName.Length - editorSuffix.Length);
            if (string.IsNullOrEmpty(runtimeAssemblyName))
            {
                return content;
            }

            // Parse the project XML
            var doc = XDocument.Parse(content);
            var ns = XNamespace.Get(""); // Default namespace

            // Find ItemGroup with References
            var itemGroups = doc.Descendants($"{ns}ItemGroup");
            XElement refItemGroup = null;

            foreach (var group in itemGroups)
            {
                if (group.Elements($"{ns}Reference").Any())
                {
                    refItemGroup = group;
                    break;
                }
            }

            // If no ItemGroup with References found, create one
            if (refItemGroup == null)
            {
                refItemGroup = new XElement($"{ns}ItemGroup");
                doc.Root.Add(refItemGroup);
            }

            var existing = refItemGroup.Elements($"{ns}Reference")
                .FirstOrDefault(r => r.Attribute("Include")?.Value == runtimeAssemblyName);

            if (existing != null)
            {
                return content;
            }

            var newRef = new XElement($"{ns}Reference",
                new XAttribute("Include", runtimeAssemblyName),
                new XElement($"{ns}HintPath", $"Library{Path.DirectorySeparatorChar}ScriptAssemblies{Path.DirectorySeparatorChar}{runtimeAssemblyName}.dll"),
                new XElement($"{ns}Private", "False")
            );

            refItemGroup.Add(newRef);

            return doc.Declaration != null
                ? doc.Declaration + "\n" + doc
                : doc.ToString();
        }
    }
}
