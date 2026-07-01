---
name: run-editmode-tests
description: "Comando de workflow do projeto (equivalente ao /run-editmode-tests do Claude Code). Executa os testes EditMode via Unity batchmode e reporta o resultado com honestidade (PASS / FAIL / NOT RUN com motivo). Não implementa correções."
---

# /run-editmode-tests

Executa os testes EditMode via Unity batchmode e reporta o resultado com honestidade (PASS / FAIL / NOT RUN com motivo). Não implementa correções.

**Arguments:** `$ARGUMENTS` — opcional: filtro de categoria/nome de teste

---

## Pré-condições (verificar antes de rodar)

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
Get-Process -Name "Unity" -ErrorAction SilentlyContinue
```

- Se houver processo Unity rodando: **NÃO rodar** (rule: unity-assets / no-parallel-unity-batchmode). Reportar `NOT RUN — Unity lock` e parar.
- Compile fallback primeiro: `dotnet build .\Assembly-CSharp.csproj --no-restore` com `$LASTEXITCODE -eq 0`. Se falhar, os testes nem compilam — reportar FAIL de compile e parar.

## Procedimento

```powershell
.\tools\unity\RunUnityEditModeTests.ps1
if ($LASTEXITCODE -ne 0) { Write-Host "TESTS FAILED (exit $LASTEXITCODE)" }
```

Aguardar o término (batchmode é sequencial, um log por execução). Ler o log/result file que o script produzir.

## Saída esperada

```text
EditMode Tests
──────────────
Compile fallback (dotnet): PASS | FAIL
Unity batchmode tests: PASS | FAIL | NOT RUN
Reason if NOT RUN: <Unity lock | license | timeout | approval>
Total / Passed / Failed: <n/n/n ou UNKNOWN>
Failed tests: <lista ou none>
Log: <caminho>
```

## Regras

- Nunca converter NOT RUN em PASS (rule: validation-truth).
- Nunca filtrar a saída do build para inferir sucesso — exit code apenas.
- Falha de teste = parar e reportar; correção é tarefa separada (/bugfix ou spec ativa).
- Nunca alegar "tests passed" sem o resultado do runner no report.
