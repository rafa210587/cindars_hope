---
name: implementation-closeout
description: Fecha uma entrega com revisão do diff, evidência vigente, documentação e status conforme a matriz canônica.
---

# Skill: Fechamento de Implementação

O fechamento reúne resultados já obtidos e identifica o que ainda falta.

**Regra central: conferir evidência e escopo, sem executar novamente todos os comandos
quando os inputs pertinentes não mudaram.**

## Quando usar

Ao finalizar spec, refactor, tooling, bugfix ou alteração documental relevante.

## Checklist

- [ ] Diff e arquivos entregues conferidos; dirty de terceiros preservado.
- [ ] Critérios e integração exigida mapeados à evidência.
- [ ] Matriz canônica aplicada; resultados GLOBAL/SCOPED diferenciados.
- [ ] Comandos, inputs/configuração, exit, XML/log e contagens verificáveis.
- [ ] Pendências de humano/Unity/Player registradas com motivo e risco.
- [ ] Promoção/commit compatíveis com instruções da sessão.

## Procedimento

1. Inspecionar diff e critérios. Non-regression review verifica comportamento/arquitetura
   que compile não prova; não precisa repetir testes confiáveis do executor.
2. Conferir evidência vigente. Executar somente gates faltantes/invalidados ou necessários
   para uma preocupação nova. Consultar SPEC_VALIDATION_MATRIX_MASTER.
3. Atualizar report e índices/status dentro do escopo autorizado; não mover specs
   automaticamente. /finish-spec determina elegibilidade.
4. Após editar/mover docs, obter docs validation sobre esse estado. Isso não invalida
   compile/testes C# cujos inputs não mudaram.
5. Se encerrando integração ampla, obter uma rodada global pertinente e testes integrados.
   Não transformar FAIL global antigo em PASS pelo fato de o scoped passar.
6. Registrar resultado final e omissões. Cenário de wave pode cobrir várias specs;
   referência documentada não significa execução humana.

## Saída esperada

Arquivos, comportamento/contratos entregues, evidência e limites.
Report de spec contém Acceptance criteria extracted, Existing systems audit,
Spec Compliance Matrix, Validation e Honest status rationale.

## Quando NÃO usar

Leitura simples sem entregável nem alteração persistente.

## Quando parar e reportar

Gate obrigatório sem evidência ou critérios não atendidos: manter status correspondente;
corrigir dentro do escopo ou deixar pendência explícita. Não alegar ACCEPTED antecipadamente.

## Relacionados

- `(skill: spec-execution)`; `(skill: unity-validation)`; `(skill: docs-migration)`.
- `(rule: validation-truth)`; `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
