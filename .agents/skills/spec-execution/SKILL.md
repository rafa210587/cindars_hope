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
- [ ] Changed runtime boundaries have explicit state owner, dependency direction, commit point,
      lifecycle and failure contract in the existing spec/plan; see `solid-and-ai-context`.
- [ ] Gates/testes escolhidos pela SPEC_VALIDATION_MATRIX_MASTER e critérios da spec.
- [ ] Responsável pela rodada integrada Unity/build definido quando aplicável.

## Leitura mínima

Read the applicable scope/risk rows of `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` for checks
and status. Read `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` only for dependency resolution,
batch coordination or promotion. Do not load unrelated rows, history, PROJECT_LOG, ROADMAP
or reports by default; do not skip a risk introduced by the actual diff.

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
6. **Evidence:** use the risk table below; create a report only for integration/keyart.
   Keep relevant integration scenarios in that report or reference an existing wave scenario.
7. **Closeout:** /finish-spec decide promoção por evidência da matriz/protocolo.
   Não mover spec nem criar commits/push automaticamente contra instruções da sessão.

## Proportional evidence

| Change | Required evidence |
|---|---|
| Docs / harness | Diff and applicable checks; no folder or report in docs/validation |
| Pure logic / bugfix | EditMode test XML; command, inputs, result and exit code in the commit message |
| Wiring / integration | Applicable test plus one paragraph in the spec report |
| Scene with approved keyart | Applicable checks plus one comparative capture; only this case permits an evidence image |

Use one stable folder `docs/validation/<spec_id>/` only when required; reuse an existing
spec report in place. Reruns overwrite outputs, never create timestamp/run subfolders.
Keep raw logs/XML in ignored `Logs/<spec_id>/` or `TestResults/`, accessible for verification.
A spec report is at most 60 lines: criteria, reused systems, requirement-to-test mapping,
commands/inputs/exit codes, real artifacts and honest status; integration needs one paragraph.
Do not copy .unity/.prefab/.asset as snapshots/backups for evidence; cite the commit hash.
No generated index.html unless the human explicitly requests it. These are storage rules,
not permission to weaken save/migration, IDs, secrets or data-integrity checks.
Status comes from the canonical matrix. A written scenario is not executed human acceptance.

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

One spec per iteration with evidence from the risk table; keep criteria traceable.
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
