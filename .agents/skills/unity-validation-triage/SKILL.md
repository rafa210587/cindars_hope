---
name: unity-validation-triage
description: Classifica falhas de validação do Unity, dotnet e log scanning sem esconder erros reais de compile. Use sempre que Unity batchmode, dotnet build ou Unity log scanning falhar.
---

# Skill: Triagem de Validação do Unity

Use esta skill sempre que Unity batchmode, `dotnet build` ou Unity log scanning falhar.

## Objetivo

Separar falhas de código de falhas de ambiente/tooling antes de reportar o status.

## Inputs obrigatórios

- Comando tentado.
- Exit code.
- Path do arquivo de log, quando presente.
- Linhas-chave de output.
- Quais gates eram aplicáveis pela SPEC_VALIDATION_MATRIX_MASTER e qual evidência existe.
- Se `Invoke-UnityGeneratedProjectsBuild.ps1` foi usado como fallback/feedback; não é pré-requisito do Test Runner.

## Regras de classificação

### Falha real de compile

Trate como falha de código se qualquer log atual contém:

```text
error CS
error Unity
error NETSDK
```

Exceção: `NETSDK1004 project.assets.json not found` é um problema de restore/setup se for corrigido por `dotnet restore`.

Ação:

1. Corrija o código.
2. Rode de novo a mesma validação.
3. Não feche a spec enquanto erros reais de compile permanecerem.

### Drift de `.csproj` local

Provável se `dotnet build` reporta tipos ausentes para arquivos recém-adicionados, e o Unity não regenerou os project files.

Ação:

1. Cheque se o novo `.cs` está no projeto gerado correspondente ao asmdef.
2. Regenerar projetos pelo Unity quando necessário; não perpetuar includes manuais em projetos obsoletos.
3. Reexecute somente o gate invalidado; Test Runner válido pode prover compile Unity.
4. Mencione o drift de `.csproj` nas notas de validação se tocado.

### Unity já aberto

Provável se o log/output contém:

```text
another Unity instance is running with this project open
Multiple Unity instances cannot open the same project
```

Ação:

1. Não chame isso de falha de compile.
2. Registre a Unity validation como bloqueada por editor lock.
3. Se autorizado pelo protocolo humano/de projeto, feche o Unity obsoleto e rode de novo.
4. Caso contrário, deixe a validação de Play Mode/Editor pendente.

### Ruído de License / Hub / Package

Não chame a falha de falha de compile de código se o log não tem `error CS` e contém apenas:

```text
license
Package Manager
Assembly-CSharp-firstpass.dll not valid
EditorTests.dll not valid
Package tests assembly not valid
```

Ação:

1. Registre como blocker de ambiente/tooling.
2. Prefira `dotnet build` como fallback de compile.
3. Mantenha a Unity Play Mode validation pendente.

### Sandbox / Permission

Provável se o output contém:

```text
Access to the path is denied
couldn't create signal pipe
cannot write Temp\obj
```

Ação:

1. Rode de novo o mesmo comando necessário com escalação aprovada, se a política permitir.
2. Se ainda bloqueado, registre o path e o comando exatos.

## Template de reporte

```text
Validation triage:
- Command: <command>
- Exit code: <code>
- Classification: <real compile failure | csproj drift | Unity lock | tooling noise | permission>
- Evidence: <short key line>
- Follow-up: <fixed/rerun/pending>
```

## Regras

- Não achate todas as falhas de validação em "build failed".
- Não declare que o Unity compile passou só a partir de `dotnet build`.
- Não ignore `error CS`.
- Não esconda Unity validation bloqueada; documente-a como pendente.
