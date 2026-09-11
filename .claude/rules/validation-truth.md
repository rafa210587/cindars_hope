# Rule: Verdade na Validação

**Invariante: cada claim exige evidência vigente e resultado explícito; seleção de gates segue
a matriz canônica em `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.**

## Onde se aplica

- Exit 0 é necessário para PASS; falha/exception/resultado desconhecido nunca vira sucesso.
- Não infira resultado de logs filtrados. Preserve comando, exit code, artefatos e inputs.
- `run_strict_validation.ps1` sem `-Gates` é GLOBAL; com gates explícitos é SCOPED.
  `SCOPED_PASS` não significa `GLOBAL_PASS`. A lista selecionada deve cobrir a matriz/spec.
- Falha global conhecida permanece FAIL e visível; não aumentar baseline para esconder regressão.
- Docs-only/harness não exige build Unity. Um gate já executado sobre inputs equivalentes
  pode ser reutilizado após revisão. Não repetir builds por agente, fase ou closeout.
- Nova edição invalida apenas evidência cujos inputs relevantes mudaram; dependências,
  defines, settings, assets e tooling também são inputs, não apenas código/HEAD.
- Falha em gate obrigatório impede o claim correspondente e ações dependentes.
  Corrigir/diagnosticar dentro do escopo; trabalho independente autorizado pode continuar.

## Claims distintos

| Claim | Evidência que comprova |
|---|---|
| .NET PASS | Compile do grafo/projetos gerados; não prova importação/lifecycle Unity |
| Unity compile PASS | Processo Unity + diagnóstico de compile; pode vir do Test Runner |
| EditMode/PlayMode PASS | XML recente válido, casos executados e escopo coberto |
| Asset validation PASS | Resultado explícito do validator aplicável |
| Human validation PASS | Cenário realmente executado, com resultado |
| BUILD_VALIDATED/ACCEPTED | Somente requisitos satisfeitos conforme matriz e spec |

## Report

Registrar modo GLOBAL/SCOPED, gates selecionados e razão, comando/versões/configuração,
exit code por execução, arquivos de log/XML realmente produzidos, inputs validados,
falhas e NOT RUN/NOT APPLICABLE. Não citar artefato que o runner não produz.
Cenário humano escrito é plano; deferimento não é ACCEPTED.

## Enforcement

`run_strict_validation.ps1`, contratos de tooling e revisão de evidência.
`pre-bash-guard.ps1` proíbe padrões que filtram o resultado do build.
