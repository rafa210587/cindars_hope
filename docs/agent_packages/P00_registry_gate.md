# P00 - Registry Gate

> Package ID: P00
> Status: Ready
> Spec alvo: governança documental antes da SPEC 12
> Depende de: Nenhuma
> Bloqueia: P12A
> Tipo: docs/validation

## Objetivo

Corrigir a contradição documental atual antes de continuar a SPEC 12.

O repo não pode dizer ao mesmo tempo que:

- `SPEC_EXECUTION_ORDER.md` marca 12-17 como `A implementar`;
- `SPEC_REGISTRY_TO_IMPLEMENT.md` lista apenas a SPEC 17;
- `PROJECT_LOG.md` diz que a SPEC 12 está em progresso.

## Escopo

Este pacote deve:

- alinhar `SPEC_REGISTRY_TO_IMPLEMENT.md` com a realidade;
- manter 12, 13, 14, 15, 16 e 17 como pendentes/parciais bloqueadoras;
- registrar que 06-11 têm implementação parcial e não devem ser tratadas como 100%;
- atualizar audit/status/log para não contradizer os registries;
- manter memory/skills existentes;
- não alterar runtime C#.

## Fora de escopo

Este pacote não deve:

- implementar gameplay;
- alterar scripts C#;
- mover specs parciais para completo;
- iniciar SPEC 12;
- iniciar SPEC 13+;
- mexer em `docs_old/**`.

## Arquivos obrigatórios para ler

```text
AGENTS.md
CLAUDE.md
memory/MEMORY.md
memory/project_skills_available.md
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/audits/SPECS_01_16_COMPLETENESS_AUDIT_20260524.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/agent_packages/PACKAGE_EXECUTION_ORDER.md
```

## Arquivos permitidos para alterar

```text
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/audits/SPECS_01_16_COMPLETENESS_AUDIT_20260524.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
docs/agent_packages/PACKAGE_EXECUTION_ORDER.md
docs/agent_packages/P00_registry_gate.md
```

## Passos obrigatórios

1. Confirmar branch `dev` e `git status`.
2. Ler todos os arquivos obrigatórios.
3. Corrigir `SPEC_REGISTRY_TO_IMPLEMENT.md` para listar novamente:
   - SPEC 12;
   - SPEC 13;
   - SPEC 14;
   - SPEC 15;
   - SPEC 16;
   - SPEC 17.
4. Marcar cada uma como `A implementar` ou `Parcial bloqueadora`, conforme o estado real.
5. Garantir que `SPEC_EXECUTION_ORDER.md` e `SPEC_REGISTRY_TO_IMPLEMENT.md` concordem.
6. Atualizar audit/status/log para deixar claro:
   - SPEC 12 não está concluída;
   - SPEC 13 não deve iniciar antes da SPEC 12;
   - SPEC 17 está bloqueada.
7. Atualizar este pacote para `Done` somente se validações passarem.
8. Atualizar `PACKAGE_EXECUTION_ORDER.md`: P00 `Done`, P12A `Ready`.
9. Rodar validação documental.
10. Commitar em português.
11. Parar.

## Critérios de aceite

- `SPEC_REGISTRY_TO_IMPLEMENT.md` lista 12-17.
- `SPEC_EXECUTION_ORDER.md` não contradiz o registry.
- `PROJECT_LOG.md` não diz que 12 está completa.
- Audit/status não dizem que 12-16 estão completas.
- P12A fica `Ready` apenas se P00 ficar `Done`.
- Nenhum arquivo C# alterado.

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
```

Unity compile não é obrigatória neste pacote porque não há runtime.

## Checks finais

```powershell
Select-String -Path "docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md" -Pattern "spec_player_combat|spec_enemy_ai|spec_cave_runtime|spec_cave_entry|spec_skill_trees|spec_ui_ux"
Select-String -Path "docs/specs/SPEC_EXECUTION_ORDER.md" -Pattern "12 |13 |14 |15 |16 |17 "
```

## Entrega esperada

Responder com:

- branch usada;
- arquivos alterados;
- resultado do docs validator;
- resultado dos checks;
- commit criado;
- confirmação de que P12A está liberado ou bloqueado.
