using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SemanticPilot {
public sealed class Result { public string Json { get; set; } public int ExitCode { get; set; } }
public static class Query {
    sealed class Match { public string path {get;set;} public int line {get;set;} public int column {get;set;} public string kind {get;set;} public string boundSymbol {get;set;} }
    sealed class Context {
        public readonly Dictionary<string,CSharpCompilation> Built = new Dictionary<string,CSharpCompilation>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> Visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> Errors = new List<string>();
        public readonly SortedDictionary<string,string> Hashes = new SortedDictionary<string,string>(StringComparer.Ordinal);
        public readonly Dictionary<string,string[]> Defines = new Dictionary<string,string[]>();
    }
    static string Hash(byte[] b) { using (var sha=SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(b)).Replace("-", "").ToLowerInvariant(); }
    static void AddHash(Context c, string p) { p=Path.GetFullPath(p); if (!File.Exists(p)) { c.Errors.Add("MISSING_INPUT: " + p); return; } c.Hashes[p]=Hash(File.ReadAllBytes(p)); }
    static string[] Values(XElement x, string name, string attr) => x.Descendants().Where(e=>e.Name.LocalName==name).Select(e=>(string)e.Attribute(attr)).Where(v=>!String.IsNullOrWhiteSpace(v)).ToArray();
    static CSharpCompilation Build(string project, Context c) {
        project=Path.GetFullPath(project);
        if(c.Built.TryGetValue(project,out var old)) return old;
        if(!c.Visiting.Add(project)) { c.Errors.Add("PROJECT_CYCLE: " + project); return null; }
        if(!File.Exists(project)) { c.Errors.Add("MISSING_PROJECT: " + project); c.Visiting.Remove(project); return null; }
        AddHash(c,project);
        XElement xml;
        try { xml=XElement.Load(project); } catch(Exception ex) { c.Errors.Add("PROJECT_XML: " + ex.Message); c.Visiting.Remove(project); return null; }
        if(xml.Name.LocalName!="Project" || xml.Attribute("Sdk")!=null) c.Errors.Add("UNSUPPORTED_PROJECT_ROOT_OR_SDK: " + project);
        var allowedTop=new HashSet<string>(new[]{"PropertyGroup","ItemGroup"},StringComparer.Ordinal);
        foreach(var child in xml.Elements().Where(e=>!allowedTop.Contains(e.Name.LocalName))) c.Errors.Add("UNSUPPORTED_PROJECT_ELEMENT: " + child.Name.LocalName + " " + project);
        var allowedItems=new HashSet<string>(new[]{"Compile","Reference","ProjectReference","Analyzer","ProjectCapability"},StringComparer.Ordinal);
        foreach(var item in xml.Elements().Where(e=>e.Name.LocalName=="ItemGroup").SelectMany(e=>e.Elements()).Where(e=>!allowedItems.Contains(e.Name.LocalName))) c.Errors.Add("UNSUPPORTED_ITEM: " + item.Name.LocalName + " " + project);
        foreach(var item in xml.Descendants().Where(e=>e.Attribute("Update")!=null || e.Attribute("Exclude")!=null || e.Attribute("Remove")!=null)) c.Errors.Add("UNSUPPORTED_ITEM_MUTATION: " + item.Name.LocalName + " " + project);
        var dir=Path.GetDirectoryName(project);
        var props=xml.Descendants().Where(e=>e.Name.LocalName=="PropertyGroup").SelectMany(e=>e.Elements()).ToArray();
        string Prop(string key) => props.Where(e=>e.Name.LocalName==key).Select(e=>e.Value.Trim()).LastOrDefault();
        var name=Prop("AssemblyName") ?? Path.GetFileNameWithoutExtension(project);
        var defines=(Prop("DefineConstants") ?? "").Split(';').Where(s=>!String.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s=>s,StringComparer.Ordinal).ToArray();
        c.Defines[project]=defines;
        if(xml.Descendants().Any(e=>e.Attribute("Condition")!=null)) c.Errors.Add("UNSUPPORTED_CONDITION: " + project);
        if(xml.Descendants().Any(e=>e.Name.LocalName=="Compile" && e.Attribute("Remove")!=null)) c.Errors.Add("UNSUPPORTED_COMPILE_REMOVE: " + project);
        if(xml.Descendants().Any(e=>e.Name.LocalName=="Import" && e.Attribute("Sdk")==null)) c.Errors.Add("UNSUPPORTED_IMPORT: " + project);
        if(xml.Descendants().Any(e=>e.Name.LocalName=="Analyzer")) c.Errors.Add("UNSUPPORTED_ANALYZER_OR_GENERATOR: " + project);
        var sources=Values(xml,"Compile","Include");
        if(sources.Length==0) c.Errors.Add("NO_EXPLICIT_SOURCES: " + project);
        var trees=new List<SyntaxTree>();
        var parse=CSharpParseOptions.Default.WithPreprocessorSymbols(defines);
        foreach(var s in sources) {
            if(s.Contains("$") || s.IndexOfAny(new[]{'*','?'})>=0) { c.Errors.Add("UNSUPPORTED_SOURCE: " + s); continue; }
            var p=Path.GetFullPath(Path.Combine(dir,s)); AddHash(c,p);
            if(File.Exists(p)) trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(p),parse,p));
        }
        var refs=new List<MetadataReference>();
        foreach(var r in xml.Descendants().Where(e=>e.Name.LocalName=="Reference")) {
            var hint=r.Elements().FirstOrDefault(e=>e.Name.LocalName=="HintPath")?.Value;
            if(String.IsNullOrWhiteSpace(hint) || hint.Contains("$")) { c.Errors.Add("UNSUPPORTED_REFERENCE: " + (string)r.Attribute("Include")); continue; }
            var p=Path.GetFullPath(Path.Combine(dir,hint)); AddHash(c,p);
            if(File.Exists(p)) try { refs.Add(MetadataReference.CreateFromFile(p)); } catch(Exception ex) { c.Errors.Add("BAD_METADATA: " + p + " " + ex.Message); }
        }
        foreach(var r in Values(xml,"ProjectReference","Include")) {
            if(r.Contains("$")) { c.Errors.Add("UNSUPPORTED_PROJECT_REFERENCE: " + r); continue; }
            var dependency=Build(Path.Combine(dir,r),c);
            if(dependency!=null) refs.Add(dependency.ToMetadataReference());
        }
        var language=Prop("LangVersion");
        var opts=new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe:String.Equals(Prop("AllowUnsafeBlocks"),"true",StringComparison.OrdinalIgnoreCase));
        if(!String.IsNullOrWhiteSpace(language)) {
            if(!LanguageVersionFacts.TryParse(language,out var version)) c.Errors.Add("UNSUPPORTED_LANG_VERSION: " + language);
            else parse=parse.WithLanguageVersion(version);
            trees=trees.Select(t=>CSharpSyntaxTree.ParseText(t.GetText(),parse,t.FilePath)).Cast<SyntaxTree>().ToList();
        }
        var compilation=CSharpCompilation.Create(name,trees,refs,opts);
        c.Built[project]=compilation; c.Visiting.Remove(project);
        return compilation;
    }
    static IEnumerable<CSharpCompilation> All(CSharpCompilation root, Context c) => c.Built.Values.Distinct();
    static bool Equivalent(ISymbol a, ISymbol b) {
        if(a==null || b==null) return false;
        if(a is IAliasSymbol alias) a=alias.Target;
        if(a is IMethodSymbol method && method.ReducedFrom!=null) a=method.ReducedFrom;
        a=a.OriginalDefinition; b=b.OriginalDefinition;
        return SymbolEqualityComparer.Default.Equals(a,b) || (a.ContainingAssembly?.Identity.ToString()==b.ContainingAssembly?.Identity.ToString() && DocumentationCommentId.CreateDeclarationId(a)==DocumentationCommentId.CreateDeclarationId(b));
    }
    public static Result Run(string project,string symbol,int maxResults,string helperPath,string entryPath) {
        var c=new Context(); AddHash(c,helperPath); AddHash(c,entryPath); AddHash(c,typeof(CSharpCompilation).Assembly.Location); AddHash(c,typeof(Compilation).Assembly.Location); var root=Build(project,c);
        var matches=new List<Match>(); var diagnostics=new List<string>(c.Errors);
        ISymbol target=null;
        if(root!=null) {
            foreach(var comp in All(root,c)) {
                var found=DocumentationCommentId.GetFirstSymbolForDeclarationId(symbol,comp);
                if(found!=null && found.Locations.Any(l=>l.IsInSource)) { if(target!=null && !Equivalent(found,target)) diagnostics.Add("AMBIGUOUS_SYMBOL: " + symbol); else target=found; }
            }
            if(target==null) diagnostics.Add("UNRESOLVED_SYMBOL: " + symbol);
            if(target is IMethodSymbol tm && tm.MethodKind!=MethodKind.Ordinary && tm.MethodKind!=MethodKind.ReducedExtension) diagnostics.Add("UNSUPPORTED_SYMBOL_KIND: " + tm.MethodKind);
            if(target is IPropertySymbol tp && tp.IsIndexer) diagnostics.Add("UNSUPPORTED_SYMBOL_KIND: Indexer");
            foreach(var comp in All(root,c)) {
                foreach(var d in comp.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error)) diagnostics.Add("COMPILATION_ERROR: " + d.ToString());
                foreach(var tree in comp.SyntaxTrees) {
                    var model=comp.GetSemanticModel(tree);
                    foreach(var node in tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>()) {
                        if(node.Ancestors().Any(a=>a is UsingDirectiveSyntax)) continue;
                        var info=model.GetSymbolInfo(node);
                        var resolved=info.Symbol ?? (info.CandidateSymbols.Length==1 ? info.CandidateSymbols[0] : null);
                        if(info.CandidateSymbols.Length>1) { diagnostics.Add("AMBIGUOUS_BINDING: " + tree.FilePath + ":" + node.GetLocation().GetLineSpan().StartLinePosition.Line); continue; }
                        if(!Equivalent(resolved,target)) continue;
                        var span=node.GetLocation().GetLineSpan();
                        matches.Add(new Match { path=Path.GetRelativePath(Path.GetDirectoryName(project),tree.FilePath).Replace('\\','/'), line=span.StartLinePosition.Line+1, column=span.StartLinePosition.Character+1, kind=resolved is INamedTypeSymbol ? "type" : (node.Parent is InvocationExpressionSyntax || node.Parent is MemberAccessExpressionSyntax access && access.Parent is InvocationExpressionSyntax ? "invocation" : "member"), boundSymbol=DocumentationCommentId.CreateDeclarationId(resolved is IMethodSymbol m && m.ReducedFrom!=null ? m.ReducedFrom : resolved) });
                    }
                }
            }
        }
        var ordered=matches.OrderBy(m=>m.path,StringComparer.Ordinal).ThenBy(m=>m.line).ThenBy(m=>m.column).ToArray();
        var omitted=Math.Max(0,ordered.Length-maxResults);
        var complete=diagnostics.Count==0 && omitted==0;
        var hashMaterial=String.Join("\n",c.Hashes.Select(kv=>kv.Key+"="+kv.Value))+"\n"+String.Join(";",c.Defines.OrderBy(k=>k.Key).Select(k=>k.Key+"="+String.Join(",",k.Value)));
        var unique=diagnostics.Distinct().ToArray();
        var payload=new { status=complete?"COMPLETE":"INCOMPLETE", complete, assembly=root?.AssemblyName, symbol, configuration=c.Defines.OrderBy(k=>k.Key).Select(k=>new { project=k.Key, defineCount=k.Value.Length }).ToArray(), inputHash=Hash(Encoding.UTF8.GetBytes(hashMaterial)), matches=ordered.Take(maxResults).ToArray(), totalMatches=ordered.Length, omittedMatches=omitted, diagnostics=unique.Take(50).ToArray(), totalDiagnostics=unique.Length, omittedDiagnostics=Math.Max(0,unique.Length-50), limitations=new[]{"Complete covers explicit bound SimpleName references in the selected project graph and defines only; implicit/pattern/dynamic references and using directives are excluded.","Constructor/operator/conversion/indexer targets are unsupported and fail closed.","Reflection, serialized IDs and GameEventBus are excluded.","Explicit csproj items only; no MSBuild evaluation or source generators."} };
        return new Result { Json=JsonSerializer.Serialize(payload), ExitCode=complete?0:2 };
    }
}
}
