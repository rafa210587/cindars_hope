using UnityEditor;
using System.Xml.Linq;
using System.IO;

namespace CindarsHope.Editor.AssetPostprocessing
{
    /// <summary>
    /// SPEC 01 BLOCKER FIX: Ensures Assembly-CSharp-Editor.csproj has a reference to Assembly-CSharp.dll.
    /// Without this reference, editor scripts cannot use runtime types (CindarsHope.Core, etc.).
    /// </summary>
    public class CSharpProjectPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Called after Unity generates the C# project file.
        /// Adds missing Assembly-CSharp reference to Assembly-CSharp-Editor.csproj.
        /// </summary>
        public static void OnGeneratedCSProject(string path, string content)
        {
            if (!path.EndsWith("Assembly-CSharp-Editor.csproj"))
                return;

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

            // Check if Assembly-CSharp reference already exists
            var existing = refItemGroup.Elements($"{ns}Reference")
                .FirstOrDefault(r => r.Attribute("Include")?.Value == "Assembly-CSharp");

            if (existing != null)
                return; // Already has the reference

            // Add Assembly-CSharp reference
            var newRef = new XElement($"{ns}Reference",
                new XAttribute("Include", "Assembly-CSharp"),
                new XElement($"{ns}HintPath", $"Library{Path.DirectorySeparatorChar}ScriptAssemblies{Path.DirectorySeparatorChar}Assembly-CSharp.dll"),
                new XElement($"{ns}Private", "False")
            );

            refItemGroup.Add(newRef);

            // Write back the modified content
            var modifiedContent = doc.Declaration + "\n" + doc.ToString();
            File.WriteAllText(path, modifiedContent);
        }
    }
}
