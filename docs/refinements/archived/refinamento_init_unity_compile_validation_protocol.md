# refinamento_init_unity_compile_validation_protocol

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `.specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md`
> Objetivo: validacao minima de compilacao Unity em batchmode.

---

## 1. Estado atual

Projeto sem validacao automatizada de compilacao Unity antes de avancar para specs runtime.

---

## 2. Gaps

- Nao ha validacao PowerShell de compilacao Unity.
- Nao ha scanner de log critico de compilacao.
- Agentes podem alterar runtime/C# sem detectar erros de compilacao.

---

## 3. Decisoes aprovadas

- Tooling em PowerShell minimo.
- Validacao em batchmode sem Play Mode.
- Scanner detecta erros criticos, warnings nao sao bloqueantes.
- Registro obrigatorio quando Unity nao puder rodar.
