---
name: run-editmode-tests
description: "Executa EditMode com evidência fresca ou reutiliza resultado pertinente verificável."
---

# /run-editmode-tests

Executa EditMode com evidência fresca ou reutiliza resultado pertinente verificável.

**Arguments:** `$ARGUMENTS` — filtro regex Unity opcional; sem filtro executa a suíte completa.

## Procedimento

1. Consultar matriz de validação e resultados anteriores. Um novo input pertinente
   invalida a evidência; delegação por si só não invalida.
2. Rodar `tools/unity/RunUnityEditModeTests.ps1` com `-TestFilter` quando pertinente,
   paths de XML/log próprios da execução e timeout adequado.
3. Não exigir build .NET antes: Unity compila no Test Runner. .NET é feedback/fallback
   opcional conforme matriz; drift de csproj não prova que testes Unity não compilam.
4. Conferir XML novo, total positivo, resultado agregado, casos e filtro. Runner também
   faz scan de compile/crash; exceptions esperadas ficam sob o Test Framework.
5. Registrar resultado, comando/versão/configuração, inputs, exit, contagens e artefatos.

## Saída esperada

EditMode PASS / FAIL / NOT RUN; compile e comportamento identificados separadamente.
Lista de falhas e pendências, sem confundir zero testes com sucesso.

## Não fazer automaticamente

Não iniciar projeto aberto nem encerrar editor preexistente. Não modificar testes ou
expected apenas para obter PASS. Não repetir suíte idêntica no retorno do subagent.
