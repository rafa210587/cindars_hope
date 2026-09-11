---
name: validate-unity
description: "Obtém os níveis Unity aplicáveis e registra a evidência real."
---

# /validate-unity

Obtém os níveis Unity aplicáveis e registra a evidência real.

**Arguments:** `$ARGUMENTS` — spec/escopo e, se necessário, filtro de testes.

## Procedimento

1. Consultar `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` e a skill `unity-validation`.
2. Inspecionar resultados vigentes antes de executar; coordenar um runner por projeto.
3. Se Test Runner comprovar compile e testes sobre os inputs atuais, não lançar
   compile-only adicional. Para compile isolado, usar `RunUnityCompileValidation.ps1`.
4. Os runners incluem scanner; não repetir scan do mesmo log.
5. Executar validators/PlayMode/inspeção humana conforme o risco de assets e integração.
6. Docs entram somente quando os inputs documentais mudarem.

## Saída esperada

Comando, configuração/versão, inputs, exit code, log/XML, contagens e níveis separados.
NOT RUN inclui motivo e risco. Nenhum resultado global é inferido de um gate isolado.

## Não fazer automaticamente

Não abrir Unity concorrente; não matar o editor do humano; não repetir gates no closeout;
não converter cenário escrito em aceitação humana.
