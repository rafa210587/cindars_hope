# Rule: Windows / PowerShell Only

This project is executed on Windows with PowerShell. No Unix/Bash commands.

## Shell obrigatório

Toda execução de agente deve usar somente sintaxe PowerShell (`pwsh` ou `powershell`).

## Comandos Unix/Bash proibidos

Do NOT use in any agent task:

```text
head
tail
ls
find
grep
cat
pwd
cd d:/...
bash pipes assuming Unix tools
```

Estes falham no Windows ou produzem comportamento incorreto.

## Equivalentes PowerShell

Use estes no lugar:

| Need | Unix/Bash | PowerShell |
|---|---|---|
| go to repo | `cd d:/path` | `Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'` |
| first N lines | `head -50 file` | `Get-Content file \| Select-Object -First 50` |
| last N lines | `tail -50 file` | `Get-Content file \| Select-Object -Last 50` |
| list files | `ls -la /path` | `Get-ChildItem /path -Recurse` |
| find files | `find . -name '*.cs'` | `Get-ChildItem -Recurse -Filter *.cs` |
| grep text | `grep -r "pattern" .` | `Select-String -Path . -Pattern "pattern" -Recurse` |
| first lines of git | `git status \| head -20` | `git status --short \| Select-Object -First 20` |
| all specs in wave | `ls .specs/a_implementar/fable/fable_*.md` | `Get-ChildItem .\.specs\a_implementar\fable\fable_*.md \| Sort-Object Name` |
| check if file exists | `test -f path` | `Test-Path path` |
| file content | `cat file.md` | `Get-Content file.md` |

## Política de falha de ambiente

Se um comando falhar por shell mismatch (sintaxe Unix no Windows):

1. **Não marque a spec como BLOCKED imediatamente.**
2. Marque o passo como `ENV_COMMAND_RETRY_REQUIRED`.
3. Tente novamente uma vez usando o equivalente PowerShell.
4. Só se o retry PowerShell **também falhar**, classifique como `ENV_COMMAND_FAILURE`.
5. `ENV_COMMAND_FAILURE` **não** é uma falha de spec a menos que fundamental, ex.: arquivo ausente ou erro de lógica.

## Preflight obrigatório (toda spec)

Antes de executar qualquer spec, rode:

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
git status --short | Select-Object -First 50
git branch --show-current
```

Não prossiga sem confirmar:
- ✓ Diretório correto
- ✓ Branch correto (`dev`)
- ✓ Estado uncommitted esperado

## Regra de Exit Code do PowerShell

**Ao usar PowerShell, sempre cheque `$LASTEXITCODE` depois de comandos externos.**

Nunca confie em output filtrado para inferir sucesso:

```powershell
# ❌ FORBIDDEN
dotnet build ... | Select-String "error"
# Exit code is lost; success/failure unknown

# ✓ REQUIRED
dotnet build ...
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed"
    exit 1
}
```

Isto se aplica a:
- `dotnet build`
- `dotnet test`
- comandos `git`
- scripts PowerShell
- Qualquer executável externo

---

## Regra de Fallback

Se um agente usar sintaxe Unix no Windows, o humano executando a tarefa pode ver falhas. O execution report deve documentar:

```text
Command retry:
  Original: head -50 file.md
  Failed: [error message]
  Retry: Get-Content file.md | Select-Object -First 50
  Result: [success/failure]
```

Se o agente usou bash e não tentou novamente em PowerShell, isto **não** é um erro do agente — mas a tarefa pode não ter rodado.

---

*Criado: 2026-06-08 (Windows Harness)*  
*Aplica-se a toda execução de spec e validação rodada por agente.*
