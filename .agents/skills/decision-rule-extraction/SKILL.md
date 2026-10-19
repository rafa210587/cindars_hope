---
name: decision-rule-extraction
description: Extrai decisões duráveis e regras atuais de specs, amendments, refinements e validation evidence para ADRs e game_rules. Use ao criar ou mudar decisões de arquitetura/gameplay, ao migrar amendments, quando uma spec introduz uma nova regra canônica, ou quando uma regra muda e precisa de um ADR que a substitua (superseding).
---

# Skill: Extração de Decisão e Regra

ADRs (`docs/decisions/`) registram o *porquê* da decisão; game_rules (`docs/game_rules/`) registram o *o quê* do comportamento atual. Amendments são apenas registro histórico — nunca cite como canônico.

## Quando usar

- Um documento contém uma decisão durável de design ou arquitetura.
- Uma regra de gameplay/system precisa virar canônica.
- Uma spec muda comportamento existente de um jeito que afeta specs futuras.
- Um amendment/spec/refinement está sendo aposentado.
- Uma regra muda e exige um ADR que a substitua (superseding).

## Leitura mínima

- `docs/project/CURRENT_STATE.md`
- `docs/project/DECISION_LOG.md`
- `docs/game_rules/GAME_RULES_INDEX.md`
- apenas os ADRs relevantes (citados pelo documento de origem)
- apenas os game_rules relevantes (citados pelo documento de origem)
- documento de origem sendo migrado

## Não ler por padrão

- todos os ADRs
- todos os game_rules
- PROJECT_LOG.md
- todos os validation reports
- todas as specs (exceto o documento de origem)

## Procedimento

1. **Identifique o tipo de conteúdo:**
   - decision rationale → criar/atualizar ADR
   - regra operacional/de gameplay atual → criar/atualizar game_rules
   - apenas evidence → permanece no validation report
   - obsoleto → marcar como delete candidate / archive

2. **Crie ou atualize o ADR:**
   - Siga `docs/decisions/_templates/ADR_TEMPLATE.md`
   - Documente o rationale da decisão e as consequências
   - Referencie o documento de origem
   - Use numeração: ADR-0001, ADR-0002, etc.

3. **Crie ou atualize o game_rule:**
   - Siga `docs/game_rules/_templates/GAME_RULE_TEMPLATE.md`
   - Documente o comportamento atual (não o estado futuro desejado)
   - Referencie os ADRs e documentos de origem
   - Use naming snake_case (ex.: `cave_rules.md`)

4. **Atualize os índices:**
   - Atualize `docs/project/DECISION_LOG.md` se houver novo ADR
   - Atualize `docs/game_rules/GAME_RULES_INDEX.md` se houver novo game_rule

5. **Atualize as referências:**
   - Atualize quaisquer specs ou docs que antes citavam a origem antiga
   - Aponte-os para o novo ADR/game_rule
   - Marque amendments como "archived" no README se o conteúdo foi migrado

6. **Se a origem era um amendment:**
   - Verifique que TODO o conteúdo foi migrado para ADR/game_rules
   - Archive ou delete o amendment conforme a governance
   - Mantenha nota de migração em `docs/amendments/README.md`
   - Atualize `docs/project/DOCUMENT_INDEX.md` se o amendment for referenciado

7. **Valide:**
   - Rode `tools/docs/validate_docs.ps1`
   - Verifique que não há referências quebradas
   - Confirme que ADR/game_rule estão devidamente indexados

## Quando parar e reportar

Pare e reporte ao humano se:

- A origem contém uma regra de Batch 2 ainda não validada em Phase 2-3 (reservar para Batch 2)
- Não existe target canônico (precisa de specification primeiro)
- A migração perderia uma decisão única (documentar à parte)
- Um ADR existente conflita com a nova regra (reconciliar primeiro)
- Um game_rule existente conflita com o código atual e não há reconciliação documentada (reportar o conflito)

## Saída esperada

Ao concluir, forneça:

1. **ADRs criados/atualizados:**
   - Paths de arquivo
   - ADR IDs
   - Resumos da decisão

2. **game_rules criados/atualizados:**
   - Paths de arquivo
   - Domain
   - Contagem de regras

3. **Documentos de origem resolvidos:**
   - Status (migrated, archived, deleted)
   - Completude da migração (%)

4. **Resultado da validação:**
   - tools/docs/validate_docs.ps1 PASS/FAIL
   - Quaisquer warnings ou errors

---

*Skill created: 2026-06-01 (SPEC_DOCS_39)*
