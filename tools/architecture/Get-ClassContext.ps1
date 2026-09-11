#requires -Version 7.0
<#
.SYNOPSIS
Consulta contexto de tipos C# próprios usando o parser Roslyn incluído no PowerShell 7.
.DESCRIPTION
Sem cache persistente: lê o checkout atual. Sem filtros, retorna apenas resumo.
OutputPath exporta registros JSONL determinísticos; Type/Module limitam o contexto exibido.
Analisa sintaxe com defines dos csproj Unity gerados; não resolve símbolos ou chamadas.
Declarações inativas em #if não fazem parte da configuração consultada.
#>
[CmdletBinding()]
param(
    [string]$ProjectRoot = (Join-Path $PSScriptRoot '../..'),
    [string]$Type,
    [string]$Module,
    [switch]$IncludeTests,
    [switch]$Members,
    [switch]$Summary,
    [ValidateRange(1, 1000)][int]$MaxResults = 10,
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
if (-not ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
    throw 'Roslyn indisponível. Execute com pwsh (PowerShell 7 com Microsoft.CodeAnalysis.CSharp).'
}

$assemblyCache = @{}
$parseOptionsCache = @{}
function Get-AssemblyContext([string]$Directory) {
    if ($assemblyCache.ContainsKey($Directory)) { return $assemblyCache[$Directory] }
    $cursor = $Directory
    $assemblyName = 'predefined'
    while ($cursor -and $cursor.StartsWith($ProjectRoot, [StringComparison]::OrdinalIgnoreCase)) {
        $definitions = @(Get-ChildItem -LiteralPath $cursor -Filter '*.asmdef' -File)
        if ($definitions.Count -gt 1) { throw "Mais de uma asmdef em $cursor" }
        if ($definitions.Count -eq 1) {
            $assemblyName = (Get-Content -LiteralPath $definitions[0].FullName -Raw | ConvertFrom-Json).name
            break
        }
        $cursor = [IO.Path]::GetDirectoryName($cursor)
    }
    $assemblyCache[$Directory] = $assemblyName
    return $assemblyName
}

function Get-ParseContext([string]$AssemblyName) {
    if ($parseOptionsCache.ContainsKey($AssemblyName)) { return $parseOptionsCache[$AssemblyName] }
    $project = Join-Path $ProjectRoot "$AssemblyName.csproj"
    $symbols = @()
    $codeText = 'no-generated-project: no-preprocessor-symbols'
    if (Test-Path -LiteralPath $project) {
        [xml]$projectXml = Get-Content -LiteralPath $project -Raw
        $symbols = @($projectXml.SelectNodes('//*[local-name()="DefineConstants"]') |
            ForEach-Object { $_.InnerText -split ';' } |
            Where-Object { $_ -match '^[A-Za-z_][A-Za-z0-9_]*$' } | Sort-Object -Unique)
        $codeText = "$AssemblyName.csproj"
    }
    $options = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithPreprocessorSymbols([string[]]$symbols)
    $result = @{ Options = $options; Source = $codeText }
    $parseOptionsCache[$AssemblyName] = $result
    return $result
}

function Get-TypeSegment($Node) {
    $name = $Node.Identifier.ValueText
    if ($Node -is [Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax] -and $Node.TypeParameterList) {
        $name += '`' + $Node.TypeParameterList.Parameters.Count
    }
    return $name
}

function Get-Documentation($Node, [string]$Tag) {
    $trivia = $Node.GetLeadingTrivia().ToFullString()
    $xml = (($trivia -split '\r?\n' | Where-Object { $_ -match '^\s*///' }) -replace '^\s*///\s?', '') -join "`n"
    $match = [regex]::Match($xml, "(?s)<$Tag\b[^>]*>(.*?)</$Tag>")
    if (-not $match.Success) { return $null }
    $value = $match.Groups[1].Value -replace '<(?:see|paramref|typeparamref)\s+(?:cref|name)="([^"]+)"\s*/>', '$1'
    return [Net.WebUtility]::HtmlDecode(($value -replace '<[^>]+>', '' -replace '\s+', ' ').Trim())
}

$roots = @(Join-Path $ProjectRoot 'Assets/_Game/Scripts')
if ($IncludeTests) { $roots += Join-Path $ProjectRoot 'Assets/_Game/Tests' }
$files = @($roots | ForEach-Object {
    if (Test-Path -LiteralPath $_) { Get-ChildItem -LiteralPath $_ -Recurse -File -Filter '*.cs' }
} | Sort-Object FullName)
$records = [Collections.Generic.List[object]]::new()
$parseErrors = [Collections.Generic.List[object]]::new()
$scannedFiles = 0
foreach ($file in $files) {
    $relativePath = [IO.Path]::GetRelativePath($ProjectRoot, $file.FullName).Replace('\', '/')
    $parts = $relativePath.Split('/')
    $moduleName = if ($parts[2] -eq 'Tests') { 'Tests/' + $parts[3] } else { $parts[3] }
    if ($Module -and $moduleName -notlike $Module) { continue }
    $scannedFiles++
    $assembly = Get-AssemblyContext $file.DirectoryName
    $parseContext = Get-ParseContext $assembly
    $codeText = [IO.File]::ReadAllText($file.FullName)
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($codeText, $parseContext.Options)
    $root = $tree.GetRoot()
    $errors = @($tree.GetDiagnostics() | Where-Object { $_.Severity.ToString() -eq 'Error' })
    if ($errors.Count) {
        $parseErrors.Add([ordered]@{ path = $relativePath; errors = @($errors | ForEach-Object { $_.ToString() }) })
    }
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($codeText))).ToLowerInvariant()
    foreach ($node in $root.DescendantNodes()) {
        if ($node -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax]) { continue }
        $typeParts = [Collections.Generic.List[string]]::new()
        $namespaceParts = [Collections.Generic.List[string]]::new()
        $typeParts.Add((Get-TypeSegment $node))
        foreach ($ancestor in $node.Ancestors()) {
            if ($ancestor -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax]) {
                $typeParts.Insert(0, (Get-TypeSegment $ancestor))
            }
            elseif ($ancestor -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax]) {
                $namespaceParts.Insert(0, $ancestor.Name.ToString())
            }
        }
        $namespace = $namespaceParts -join '.'
        $symbol = ($typeParts -join '+')
        if ($namespace) { $symbol = $namespace + '.' + $symbol }
        if ($Type -and $node.Identifier.ValueText -notlike $Type -and $symbol -notlike $Type) { continue }
        $span = $node.GetLocation().GetLineSpan()
        $record = [ordered]@{
            symbol = $symbol
            kind = $node.GetType().Name.Replace('DeclarationSyntax', '').ToLowerInvariant()
            module = $moduleName
            assembly = $assembly
            path = $relativePath
            line = $span.StartLinePosition.Line + 1
            endLine = $span.EndLinePosition.Line + 1
            partial = [bool](@($node.Modifiers | Where-Object { $_.Text -eq 'partial' }).Count)
            bases = @(if ($node.BaseList) { $node.BaseList.Types | ForEach-Object { $_.ToString() } })
            summary = Get-Documentation $node 'summary'
            remarks = Get-Documentation $node 'remarks'
            sourceHash = $hash
            preprocessorSource = $parseContext.Source
        }
        if ($Members -and $node -is [Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax]) {
            $record.members = @($node.Members | Where-Object {
                $_ -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax] -and
                (@($_.Modifiers | Where-Object { $_.Text -in @('public', 'protected') }).Count -or
                    ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax] -and
                        -not @($_.Modifiers | Where-Object { $_.Text -in @('private', 'internal') }).Count))
            } | ForEach-Object {
                # Assinatura sintática: não exportar bodies, initializers ou comentários.
                if ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax]) {
                    $prefix = ($_.Modifiers | ForEach-Object Text) -join ' '
                    if ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax]) { $prefix += ' event' }
                    ($prefix + ' ' + $_.Declaration.Type + ' ' + (($_.Declaration.Variables | ForEach-Object { $_.Identifier.ValueText }) -join ', ')).Trim()
                }
                else {
                    $end = $_.Span.End
                    if ($_.Body) { $end = $_.Body.SpanStart }
                    elseif ($_.ExpressionBody) { $end = $_.ExpressionBody.SpanStart }
                    elseif ($_.AccessorList) { $end = $_.AccessorList.SpanStart }
                    ($codeText.Substring($_.SpanStart, $end - $_.SpanStart) -replace '\s+', ' ').Trim().TrimEnd(';')
                }
            })
        }
        $records.Add([pscustomobject]$record)
    }
}

if ($parseErrors.Count) {
    $parseErrors | ConvertTo-Json -Depth 6 | Write-Warning
    throw "CLASS_CONTEXT_PARSE_FAILURE: $($parseErrors.Count) arquivo(s); índice incompleto não será exportado."
}

if ($OutputPath) {
    $outputAbsolute = if ([IO.Path]::IsPathRooted($OutputPath)) {
        [IO.Path]::GetFullPath($OutputPath)
    } else {
        [IO.Path]::GetFullPath((Join-Path $ProjectRoot $OutputPath))
    }
    if ([IO.Path]::GetExtension($outputAbsolute) -ne '.jsonl') { throw 'OutputPath deve terminar em .jsonl.' }
    $assetsRoot = [IO.Path]::GetFullPath((Join-Path $ProjectRoot 'Assets')) + [IO.Path]::DirectorySeparatorChar
    if ($outputAbsolute.StartsWith($assetsRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Exporte o índice fora de Assets para evitar imports e alterações de conteúdo.'
    }
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($outputAbsolute)) | Out-Null
    $lines = @($records | ForEach-Object { $_ | ConvertTo-Json -Depth 6 -Compress })
    [IO.File]::WriteAllLines($outputAbsolute, [string[]]$lines, [Text.UTF8Encoding]::new($false))
}

if ($Summary -or (-not $Type -and -not $Module) -or $OutputPath) {
    [ordered]@{
        scannedFiles = $scannedFiles
        declarations = $records.Count
        uniqueTypes = @($records | ForEach-Object { $_.assembly + '|' + $_.symbol } | Sort-Object -Unique).Count
        documentedDeclarations = @($records | Where-Object summary).Count
        modules = @($records | Group-Object module | Sort-Object Name | ForEach-Object {
            [ordered]@{ module = $_.Name; declarations = $_.Count }
        })
        analysis = 'Roslyn syntax, current generated-project defines; partial declarations retain separate source locations. No semantic dependency analysis.'
        outputPath = $OutputPath
    } | ConvertTo-Json -Depth 5
}
else {
    $records | Select-Object -First $MaxResults | ForEach-Object { $_ | ConvertTo-Json -Depth 6 -Compress }
    if ($records.Count -gt $MaxResults) {
        Write-Warning "$($records.Count) declarações encontradas; mostrando $MaxResults. Refine Type/Module ou use OutputPath."
    }
    if (-not $records.Count) { Write-Warning 'Nenhum tipo corresponde aos filtros.' }
}
