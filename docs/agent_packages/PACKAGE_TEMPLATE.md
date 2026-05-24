# PACKAGE TEMPLATE

> Package ID: PXX
> Status: Ready | Blocked | In Progress | Done | Partial
> Spec alvo: SPEC XX - nome
> Depende de: PXX
> Bloqueia: PXX
> Tipo: docs | runtime | validation | closure

## Objetivo

Descrever em uma frase o resultado verificável deste pacote.

## Escopo

Este pacote deve fazer:

- item 1
- item 2
- item 3

## Fora de escopo

Este pacote não deve fazer:

- item 1
- item 2
- item 3

## Arquivos obrigatórios para ler

```text
AGENTS.md
CLAUDE.md
memory/MEMORY.md
memory/project_skills_available.md
docs/specs/SPEC_EXECUTION_ORDER.md
```

## Arquivos permitidos para alterar

```text
listar caminhos permitidos
```

## Arquivos proibidos

```text
docs_old/**
specs/**
spec/**
```

## Passos obrigatórios

1. Ler arquivos obrigatórios.
2. Confirmar branch e status local.
3. Implementar somente este pacote.
4. Rodar validações.
5. Atualizar documentação permitida.
6. Criar commit em português.
7. Parar.

## Critérios de aceite

- critério 1
- critério 2
- critério 3

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Se Unity não rodar:

```text
Unity validation: NOT RUN
Reason: <motivo real>
Command attempted: <comando exato>
Residual risk: Unity compile not validated locally
```

## Checks finais

```powershell
# listar buscas específicas do pacote
```

## Entrega esperada

Responder com:

- branch usada;
- commit base;
- arquivos alterados;
- validações executadas;
- resultado dos checks;
- pendências;
- commit criado;
- se o próximo pacote está liberado ou bloqueado.
