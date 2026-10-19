#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$ProjectRoot = (Join-Path $PSScriptRoot '../..'),
    [ValidateRange(1,1000)][int]$MaxFindings = 20,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
$findings = [Collections.Generic.List[object]]::new()
$counts = [ordered]@{ skills=0; commands=0; agents=0; rules=0; hookScripts=0; markdownFiles=0; localLinks=0; externalLinks=0; typedReferences=0; settingsHooks=0 }
$root = [IO.Path]::GetFullPath($ProjectRoot).TrimEnd('/','\')
function Add-Finding([string]$Code,[string]$Path,[string]$Message) {
    $findings.Add([ordered]@{code=$Code;path=$Path;message=$Message})
}
function Relative([string]$Path) { [IO.Path]::GetRelativePath($root,$Path).Replace('\','/') }
function Assert-NoReparse([string]$Path) {
    $current = [IO.Path]::GetFullPath($Path)
    while ($current) {
        if ((Test-Path -LiteralPath $current) -and ((Get-Item -Force -LiteralPath $current).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Reparse point is not an auditable input: $(Relative $current)" }
        $parent = [IO.Path]::GetDirectoryName($current)
        if ($parent -eq $current) { break }; $current = $parent
    }
}
function Read-Input([string]$Path) {
    Assert-NoReparse $Path
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Required input missing: $(Relative $Path)" }
    $text = [IO.File]::ReadAllText($Path,[Text.UTF8Encoding]::new($false,$true))
    if ([string]::IsNullOrWhiteSpace($text) -or $text.Contains([char]0)) { throw "Empty or binary input: $(Relative $Path)" }
    return $text
}
function Remove-Fences([string]$Text) {
    $lines = [Collections.Generic.List[string]]::new(); $marker=''; $length=0
    foreach ($line in ($Text -split '\r?\n')) {
        if (-not $marker -and $line -match '^\s{0,3}(`{3,}|~{3,})') { $marker=$Matches[1].Substring(0,1); $length=$Matches[1].Length; continue }
        if ($marker) {
            if ($line -match ('^\s{0,3}'+[regex]::Escape($marker)+'{'+$length+',}\s*$')) { $marker='' }
            continue
        }
        $lines.Add($line)
    }
    return $lines -join "`n"
}
function Check-Target([string]$Source,[string]$Target,[string]$Code='BROKEN_LINK',[string]$BaseDirectory='') {
    if ($Target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:' -or $Target.StartsWith('//')) {
        if ($Code -eq 'BROKEN_HOOK_SCRIPT') { Add-Finding $Code $Source ('Hook script must be local: '+$Target) } else { $counts.externalLinks++ }
        return
    }
    if ($Target -match '{{.+?}}') {
        if ($Source -match '/(?:templates|examples)/') { return }
        Add-Finding 'PLACEHOLDER_REFERENCE' $Source $Target; return
    }
    $targetPath = [uri]::UnescapeDataString(($Target -split '[#?]',2)[0])
    if (-not $targetPath) { return }
    $counts.localLinks++
    try {
        if (-not $BaseDirectory) { $BaseDirectory=Split-Path (Join-Path $root $Source) -Parent }
        $full = [IO.Path]::GetFullPath((Join-Path $BaseDirectory $targetPath))
        if (-not $full.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Target escapes project root.' }
        Assert-NoReparse $full
        if (-not (Test-Path -LiteralPath $full)) { throw 'Target does not exist.' }
        if ($Code -eq 'BROKEN_HOOK_SCRIPT' -and ([IO.Path]::GetExtension($full) -ne '.ps1' -or -not (Test-Path -LiteralPath $full -PathType Leaf))) { throw 'Hook target must be a PowerShell script file.' }
    } catch { Add-Finding $Code $Source ($Target+': '+$_.Exception.Message) }
}
$inventory = @{}; $documents = @{}; $identity = @{}
try {
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { throw 'ProjectRoot does not exist.' }
    Assert-NoReparse $root
    foreach ($kind in @('skills','commands','agents','rules','hooks')) {
        $folder = Join-Path $root ('.claude/'+$kind)
        Assert-NoReparse $folder
        if (-not (Test-Path -LiteralPath $folder -PathType Container)) { throw "Required input directory missing: .claude/$kind" }
        $entries = @(Get-ChildItem -LiteralPath $folder -Recurse -Force)
        foreach ($entry in $entries) { Assert-NoReparse $entry.FullName }
        $files = @($entries | Where-Object { -not $_.PSIsContainer })
        if ($kind -eq 'skills') {
            foreach ($directory in $entries | Where-Object { $_.PSIsContainer -and $_.Parent.FullName -eq $folder }) {
                if (-not (Test-Path -LiteralPath (Join-Path $directory.FullName 'SKILL.md') -PathType Leaf)) { Add-Finding 'MISSING_SKILL_ENTRY' (Relative $directory.FullName) 'Required SKILL.md missing.' }
            }
        }
        $canonical = switch ($kind) {
            skills { @($files | Where-Object { $_.Name -eq 'SKILL.md' -and $_.Directory.Parent.FullName -eq $folder }) }
            hooks { @($files | Where-Object { $_.Extension -eq '.ps1' }) }
            default { @($files | Where-Object { $_.Extension -eq '.md' -and $_.DirectoryName -eq $folder }) }
        }
        if ($canonical.Count -eq 0) { throw "Empty canonical inventory: .claude/$kind" }
        $counts[$(if ($kind -eq 'hooks') { 'hookScripts' } else { $kind })] = $canonical.Count
        $inventory[$kind] = @{}
        foreach ($file in $canonical) {
            $id = if ($kind -eq 'skills') { $file.Directory.Name } else { $file.BaseName }
            $path = Relative $file.FullName
            if ($id -cnotmatch '^[a-z][a-z0-9_-]*$' -and -not ($kind -eq 'rules' -and $id -ceq 'RULES')) { Add-Finding 'INVALID_ID' $path $id }
            if ($inventory[$kind].ContainsKey($id)) { Add-Finding 'DUPLICATE_ID' $path $id }
            $inventory[$kind][$id] = $path
            $content = Read-Input $file.FullName
            if ($kind -in @('skills','agents') -or $content -match '\A---\r?\n') {
                $front = [regex]::Match($content,'\A---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)')
                if (-not $front.Success) { Add-Finding 'INVALID_FRONTMATTER' $path 'Missing or unterminated frontmatter.'; continue }
                $fields=@{}
                foreach ($line in ($front.Groups[1].Value -split '\r?\n')) {
                    if ($line -match '^([A-Za-z][A-Za-z0-9_-]*):\s*(.*)$') {
                        $fieldName=$Matches[1]; $value=$Matches[2].Trim()
                        if ($fields.ContainsKey($fieldName)) { Add-Finding 'INVALID_FRONTMATTER' $path ('Duplicate field: '+$fieldName) }
                        if (($value.StartsWith('"') -and -not $value.EndsWith('"')) -or ($value.StartsWith("'") -and -not $value.EndsWith("'")) -or ($value.StartsWith('[') -and -not $value.EndsWith(']')) -or ($value.StartsWith('{') -and -not $value.EndsWith('}'))) { Add-Finding 'INVALID_FRONTMATTER' $path ('Unterminated value: '+$fieldName) }
                        $fields[$fieldName]=$value.Trim('"',"'")
                    } elseif ($line -and $line -notmatch '^\s|^#') { Add-Finding 'INVALID_FRONTMATTER' $path ('Invalid field: '+$line) }
                }
                if ($kind -in @('skills','agents')) {
                    foreach ($key in @('name','description')) { if (-not $fields[$key]) { Add-Finding 'INVALID_FRONTMATTER' $path ('Required field: '+$key) } }
                    if ($fields.name -cne $id) { Add-Finding 'IDENTITY_MISMATCH' $path ('name must equal '+$id) }
                    $key=$kind+':'+$fields.name
                    if ($identity.ContainsKey($key)) { Add-Finding 'DUPLICATE_ID' $path ('Duplicate frontmatter name: '+$fields.name) }
                    $identity[$key]=$path
                }
            }
        }
        foreach ($file in $files | Where-Object Extension -eq '.md') { $documents[(Relative $file.FullName)] = Read-Input $file.FullName }
    }
    foreach ($path in @('CLAUDE.md','.claude/HARNESS_INDEX.md','.claude/HARNESS_AUTHORING_STANDARD.md')) { $documents[$path] = Read-Input (Join-Path $root $path) }
    $counts.markdownFiles = $documents.Count
    $catalog = Remove-Fences $documents['.claude/HARNESS_INDEX.md']
    $listed=@{skills=@{};commands=@{};agents=@{}}; $section=''
    foreach ($line in ($catalog -split '\r?\n')) {
        if ($line -match '^##\s+(\w+)') { $section=$Matches[1].ToLowerInvariant() }
        if ($section -in @('skills','commands','agents') -and $line -match '^\|\s*`/?([^`]+)`\s*\|') {
            $id=$Matches[1]
            if ($listed[$section].ContainsKey($id)) { Add-Finding 'CATALOG_DUPLICATE' '.claude/HARNESS_INDEX.md' ($section+':'+$id) }
            $listed[$section][$id]=$true
            if (-not $inventory[$section].ContainsKey($id)) { Add-Finding 'CATALOG_ORPHAN' '.claude/HARNESS_INDEX.md' ($section+':'+$id) }
        }
    }
    foreach ($kind in @('skills','commands','agents')) {
        foreach ($id in $inventory[$kind].Keys) { if (-not $listed[$kind].ContainsKey($id)) { Add-Finding 'UNCATALOGUED' $inventory[$kind][$id] $id } }
    }
    # The rules index is the catalog's existing second level; no competing registry.
    $cataloguedRules=@{}
    foreach ($catalogPath in @('.claude/HARNESS_INDEX.md','.claude/rules/RULES.md')) {
        foreach ($link in [regex]::Matches((Remove-Fences $documents[$catalogPath]),'\]\(([^\s)#]+)(?:#[^)]*)?\)')) {
            $target=$link.Groups[1].Value
            if ($target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') { continue }
            $full=[IO.Path]::GetFullPath((Join-Path (Split-Path (Join-Path $root $catalogPath) -Parent) $target))
            $cataloguedRules[(Relative $full)]=$true
        }
    }
    foreach ($id in $inventory.rules.Keys) {
        if (-not $cataloguedRules.ContainsKey($inventory.rules[$id])) { Add-Finding 'UNCATALOGUED' $inventory.rules[$id] $id }
    }
    foreach ($path in $documents.Keys) {
        $text = Remove-Fences $documents[$path]
        foreach ($link in [regex]::Matches($text,'!?\[[^\]\r\n]*\]\(\s*(?:<(?<target>[^>]+)>|(?<target>[^\s)]+))(?:\s+"[^"]*")?\s*\)')) { Check-Target $path $link.Groups['target'].Value }
        foreach ($link in [regex]::Matches($text,'(?m)^\s{0,3}\[[^\]\r\n]+\]:\s*(?:<(?<target>[^>]+)>|(?<target>\S+))')) { Check-Target $path $link.Groups['target'].Value }
        foreach ($parenthesis in [regex]::Matches($text,'\([^()]*\)')) {
          foreach ($reference in [regex]::Matches($parenthesis.Value,'\b(skill|rule|agent):\s*`?(\{\{[^}]+\}\}|[^\s`,;/)]+)?')) {
            $counts.typedReferences++; $type=$reference.Groups[1].Value; $id=$reference.Groups[2].Value
            if ($id -match '{{.+?}}' -and $path -match '/(?:templates|examples)/') { continue }
            $kind = @{skill='skills';rule='rules';agent='agents'}[$type]
            if (-not $inventory[$kind].ContainsKey($id) -and -not ($type -eq 'skill' -and $inventory.commands.ContainsKey($id))) { Add-Finding 'BROKEN_TYPED_REFERENCE' $path ($type+':'+$id) }
          }
        }
    }
    $settings = (Read-Input (Join-Path $root '.claude/settings.json')) | ConvertFrom-Json -AsHashtable
    if ($settings -isnot [Collections.IDictionary] -or $settings.hooks -isnot [Collections.IDictionary] -or $settings.hooks.Count -eq 0) { throw 'Invalid or empty settings hooks object.' }
    foreach ($event in $settings.hooks.Keys) {
        if ($settings.hooks[$event] -isnot [array] -or $settings.hooks[$event].Count -eq 0) { throw "Invalid hook event array: $event" }
        foreach ($group in $settings.hooks[$event]) {
            if ($group.hooks -isnot [array] -or $group.hooks.Count -eq 0) { throw "Invalid hooks array: $event" }
            foreach ($hook in $group.hooks) {
                $counts.settingsHooks++
                if ($hook -isnot [Collections.IDictionary] -or $hook.type -isnot [string] -or $hook.type -ne 'command' -or $hook.command -isnot [string] -or [string]::IsNullOrWhiteSpace($hook.command)) { throw "Invalid command hook: $event" }
                $scriptTargets=[regex]::Matches($hook.command,'(?i)(?:-File\s+)(?:"([^"]+)"|''([^'']+)''|(\S+))')
                if (-not $scriptTargets.Count) { Add-Finding 'UNSUPPORTED_HOOK_COMMAND' '.claude/settings.json' $hook.command; continue }
                foreach ($match in $scriptTargets) {
                    $target=($match.Groups[1..3].Value | Where-Object { $_ } | Select-Object -First 1)
                    Check-Target '.claude/settings.json' $target 'BROKEN_HOOK_SCRIPT' $root
                }
            }
        }
    }
} catch { Add-Finding 'INVALID_INPUT' '.' $_.Exception.Message }

if ($OutputPath) {
    try {
        $output = [IO.Path]::GetFullPath($OutputPath)
        Assert-NoReparse $output
        $relative = Relative $output
        if ($relative -match '^(?:Assets|\.claude|\.agents|\.codex)(?:/|$)' -or $output -notmatch '\.json$' -or (Test-Path -LiteralPath $output)) { throw 'Output must be a new JSON outside canonical/generated inputs and Assets.' }
        if (-not (Test-Path -LiteralPath (Split-Path $output -Parent) -PathType Container)) { throw 'Output parent directory does not exist.' }
    } catch { Add-Finding 'INVALID_OUTPUT' '.' $_.Exception.Message; $output=$null }
}
$ordered = @($findings | Sort-Object { $_.path },{ $_.code },{ $_.message })
$result = [ordered]@{schemaVersion=1;counts=$counts;findings=@($ordered | Select-Object -First $MaxFindings);totalFindings=$ordered.Count;omittedFindings=[Math]::Max(0,$ordered.Count-$MaxFindings);result=$(if ($ordered.Count) {'FAIL'} else {'PASS'})}
$json = $result | ConvertTo-Json -Depth 8
if ($output) {
    try {
        $stream=[IO.File]::Open($output,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try { $bytes=[Text.UTF8Encoding]::new($false).GetBytes($json+"`n"); $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
    } catch { Write-Error $_; exit 1 }
}
Write-Output $json
if ($ordered.Count) { exit 1 }; exit 0
