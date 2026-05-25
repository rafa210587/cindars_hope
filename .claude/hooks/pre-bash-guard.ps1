# Pre-Bash Guard Hook
# Blocks unsafe git and file operations before execution

# This hook is called before bash/powershell command execution
# It inspects the command and blocks dangerous patterns

param(
    [string]$Command
)

# Blocked commands - destructive or unsafe
$BlockedPatterns = @(
    # Git operations
    "git\s+push",
    "git\s+reset\s+--hard",
    "git\s+clean",
    "git\s+stash",
    "git\s+rebase",
    "git\s+reset\s+--(hard|mixed|merge)",

    # File operations
    "rm\s+-rf",
    "rm\s+-r",
    "del\s+/s",
    "del\s+/q",
    "Remove-Item\s+.*-Recurse",
    "Remove-Item\s+.*-Force",

    # Destructive batch operations
    "Clear-Content.*-Force",
    "Set-Content.*\|\s*Select-Object"
)

# Check command against blocked patterns
foreach ($pattern in $BlockedPatterns) {
    if ($Command -match $pattern) {
        Write-Error "🚫 Blocked unsafe command: $Command"
        Write-Error "   Reason: Matches blocked pattern '$pattern'"
        Write-Error ""
        Write-Error "   Allowed alternatives:"
        Write-Error "   - git status / git diff / git log (info only)"
        Write-Error "   - git add / git commit (when intentional)"
        Write-Error "   - Copy-Item / Move-Item (safe file ops)"
        Write-Error ""
        return $false
    }
}

# Command is safe
return $true
