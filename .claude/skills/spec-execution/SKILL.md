---
name: spec-execution
description: Executa uma spec com ownership, critérios verificáveis, seleção canônica de validação e closeout com evidência.
---

# Skill: Execução de Spec

Reusa sistemas do projeto e mantém escopo/estado explícitos.

**Regra central: implementar critérios autorizados e comprovar comportamento;
a matriz canônica define validações e status, sem repetir gates por cerimônia.**

## Quando usar

Implementar/avançar spec em `.specs/a_implementar/`, incluindo manutenção autorizada.

## Checklist antes de editar

- [ ] AGENTS/CLAUDE, CURRENT_STATE, spec e arquivos pertinentes lidos.
- [ ] Ownership, dirty anterior e arquivos permitidos/proibidos registrados.
- [ ] Critérios centrais e anti-regressão extraídos; sistemas existentes auditados.
- [ ] Dependências resolvidas; nenhuma implementação paralela desnecessária.
- [ ] Gates/testes escolhidos pela SPEC_VALIDATION_MATRIX_MASTER e critérios da spec.
- [ ] Responsável pela rodada integrada Unity/build definido quando aplicável.

## Leitura mínima

Além do checklist: `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` para promoção/dependências e
`.specs/SPEC_VALIDATION_MATRIX_MASTER.md` para seleção/status. Não carregar histórico,
PROJECT_LOG, ROADMAP ou relatórios não relacionados por padrão.

## Procedimento

1. **Phase 0:** confrontar spec/estado, dependências e implementação existente.
   Conflito relevante é reportado; não inventar outra feature.
2. **Implementação:** editar a menor fatia coesa, preservar APIs/IDs/GUIDs/schema salvo
   mudança autorizada. Runtime MonoBehaviour fino, bus para gameplay cross-system.
3. **Comportamento:** executar testes existentes pertinentes e novos casos quando faltam
   invariantes relevantes. Teste que espelha setter/List não substitui comportamento.
   EditMode, PlayMode e contratos de tooling têm diretórios/assemblies próprios.
4. **Validação:** seguir /validate-spec. Obter gates aplicáveis uma vez por estado dos
   inputs; pode reutilizar evidência de subagent verificada. Não exigir builds por fase.
5. **Revisão:** conferir diff, critérios, consumidores, falhas, wiring e save afetados.
   Usar non-regression-review; refactor relevante usa solid-refactoring.
6. **Report:** criar arquivo individual em docs/validation, com seções abaixo.
   Documentar cenários de integração pertinentes ou referenciar o cenário da wave.
7. **Closeout:** /finish-spec decide promoção por evidência da matriz/protocolo.
   Não mover spec nem criar commits/push automaticamente contra instruções da sessão.

## Report obrigatório

- Acceptance criteria extracted: critério binário e evidência esperada.
- Existing systems audit: encontrado/reutilizado/criado e motivo.
- Spec Compliance Matrix: requirement -> implementação/teste -> OK/FAIL/DEFERRED/N/A.
- Validation: gates e modo GLOBAL/SCOPED, comandos, versões/configuração, inputs,
  exit codes, artefatos reais, contagens e resultados; NOT RUN com motivo/risco.
- Honest status rationale: o que foi comprovado, pendências e trabalho restante.

Status/taxonomia ficam somente na matriz. BUILD_VALIDATED exige critérios centrais
implementados, compile/gates pertinentes PASS e report; não equivale a global PASS,
PlayMode nem aceitação humana. Cenário escrito não é cenário executado.

## Testes e proteção comportamental

A matriz nomeia os contratos por domínio. Manter round-trip/legado/recovery de save,
seed/snapshot de cave, idempotência, transações/quantidades e lifecycle/eventos pertinentes.
Bugfix exige regressão ou justificativa concreta e risco. Refactor relevante pode exigir
caracterização antes/depois. Reusar cobertura existente não é testing deferred.
Mudança reversível de baixo impacto não exige teste novo que apenas repete implementação.
UI/cena/input/física/wiring requer evidência de integração; humano pode ser agrupado por wave.
Code-only sem esse risco não exige PlayMode indiscriminado.

## Dependências: resolver e voltar

1. Identificar Depends on/Required systems/Blocks e dependências reais dos critérios.
2. Para dependência same-wave autorizada, empilhar a original e resolver depth-first.
3. Executar a raiz e voltar à spec ORIGINAL antes de outra tarefa.
4. BLOCKED_BY_DEPENDENCY_PENDING é temporário; registrar plano/estado de batch quando útil.
5. Bloquear apenas dependência fora do escopo autorizado, HOLD/future, decisão indispensável
   ou requisito sem solução. Scene/asset/Package/PlayMode não é bloqueio automático
   quando a spec e a sessão já autorizam esse trabalho.
6. Contrato fundacional sem integração necessária impede avançar consumidores.

## Batch

Uma spec por iteração e report individual; critérios continuam rastreáveis.
Gates/testes pertinentes por fatia, integração global no checkpoint; não repetir a mesma
evidência por spec. Falha obrigatória impede ações dependentes; diagnóstico/correção
autorizados e trabalho independente podem continuar. Batch não prova aceitação da wave.

## Quando NÃO usar

Auditoria read-only ou pergunta sem implementação; bug pontual segue bugfix.

## Quando parar e reportar

Conflito spec/CURRENT_STATE, escopo proibido, dependência impeditiva ou resultado
obrigatório ausente. Explicar a causa, preservar os resultados reais e o dirty de terceiros.

## Relacionados

- `(skill: system-reuse-audit)`; `(skill: solid-refactoring)`; `(skill: unity-validation)`.
- `(skill: non-regression-review)`; `(skill: implementation-closeout)`.
- `(rule: validation-truth)`; `(rule: testing-quality-gate)`.
