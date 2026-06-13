# SPEC — Governança Canônica: Input Map + ADR de Densidade da Caverna + Re-mapa de Refinamentos

> **Spec ID:** `fable_67_spec_canonical_governance_input_map_adr`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P2
> **Type:** Docs / Governance
> **Domain:** Docs / Decisions / Game Rules
> **Parallelizable:** YES (docs-only; zero código; pode rodar JÁ)
> **Parallel group:** fable_batch11_shipping
> **Can run with:** F61, F62, F63, F64, F65, F66 (nenhum arquivo de código compartilhado)
> **Must not run with:** N/A (apenas coordenar com F62, consumidora do input_map.md)
> **Repo lock scope:** docs/game_rules/**, docs/decisions/**, docs/refinements/a_implementar/**, docs/project/DOCUMENT_DELETE_CANDIDATES.md
> **Depends on:** nenhuma (pode rodar imediatamente)
> **Blocks:**
> - F62 (E67 — input_map.md é a fonte canônica dos textos de hint/tela Controles)
> - qualquer spec futura de UI/ability que adicione tecla
> **Scope:** (a) game_rule nova input_map.md; (b) ADR superseding de densidade de inimigos + atualização da cave_rules.md; (c) correção do ref_futuro_map.md e arquivamento dos pre_refinamentos v1.0 superados (via delete candidates).
> **Out of scope:** mudar QUALQUER código ou tecla, mudar a densidade real do código (o ADR documenta a realidade), rebind, deletar documentos diretamente, reescrever refinamentos.

required_adrs: [ADR-0005-cave-stable-run-and-replay.md, ADR-0001-canonical-documentation-structure.md]
required_game_rules: [cave_rules.md, documentation_rules.md]

---

# /speckit.specify

## Contexto

Três rachaduras de governança verificadas no repo:

1. **Input sem mapa canônico.** ~138 ocorrências de `KeyCode.` em 34 arquivos de
   `Assets/_Game/Scripts/**` (GameplayInputRouter: I/K/U/Esc; PlayerMovementActionInput:
   WASD/setas/Space/LeftShift; PlayerAttackController: E/Q com hold; painéis: L/J/C/M/
   Tab/1-4/R/T/Y/G; e dezenas de teclas de debug). Não existe documento que liste o
   input real — cada spec de UI/ability escolhe tecla no escuro, com risco real de
   colisão (a tecla só "existe" no .cs que a consome).

2. **Densidade de inimigos: docs mentem.** ADR-0005 (§Ranges) e
   docs/game_rules/cave_rules.md (Rule: Enemy Count Range) dizem "12-20 inimigos por
   nível". O código real (CaveEnemySpawnPlanner.cs) implementa: MinEnemiesPerLevel=16,
   MaxEnemiesPerLevel=32, DepthScalingHardCap=44, com escala por profundidade
   (ResolveTargetEnemyCount: minBound = min(44, 16 + depth/12); maxBound = min(44,
   max(32, configurado) + depth/8)). Regra do projeto (docs-governance #3): conflito
   código × ADR/game_rule → um dos dois está stale; mudança de comportamento já
   aceita → ADR superseding (skill decision-rule-extraction).

3. **Mapa de refinamentos quebrado.** docs/refinements/a_implementar/ref_futuro_map.md
   aponta 8 specs em `docs/specs/a_implementar/spec_*.md` que NÃO existem mais (a pasta
   raiz só tem README.md — o planejamento migrou para as specs fable_*). E em
   pre_refinamentos/ há 2 documentos v1.0 superados pelas v1.1 correspondentes
   (PRE_REFINAMENTO_FARM_ESTRUTURA_ATIVIDADES_RECURSOS e PRE_REFINAMENTO_VISAO_GERAL_JOGO)
   convivendo como se ambos fossem vivos. Arquivamento segue o processo de governança:
   delete candidates / mover para arquivo — NUNCA deletar direto.

## Problema

Sem input map, F62 (hints/tela Controles) não tem fonte canônica e cada spec nova pode
colidir tecla com sistema existente. Com ADR/game_rule de densidade desatualizados,
qualquer agente que "corrija o código para obedecer o ADR" introduziria uma regressão de
banda contra o comportamento aceito (16-32/cap 44). Com o ref_futuro_map quebrado, o
processo de dependência de refinamentos aponta para o vazio e os v1.0 superados podem
ser lidos como direção vigente.

## Objetivo

Ao final desta spec (docs-only): (1) `docs/game_rules/input_map.md` existe como fonte
canônica do input real — tabela completa derivada de varredura `KeyCode.` no código
(gameplay × painéis × debug separados) + a regra "specs de UI/ability CONSULTAM o input
map antes de adicionar tecla e o ATUALIZAM no mesmo diff"; (2) ADR-0010 (superseding
parcial do ADR-0005 no range de inimigos) documenta a densidade real
(min 16 / max 32 / hard cap 44 / escala por profundidade) com o porquê, e cave_rules.md
é atualizada para o valor real com referência ao ADR novo; (3) ref_futuro_map.md tem os
8 links re-mapeados para as specs fable equivalentes (existentes e verificadas) e os 2
pre_refinamentos v1.0 superados são arquivados pelo processo (delete candidates +
status), nunca deletados direto.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/** (varredura KeyCode. — fonte da verdade do input; arquivos-chave:
  UI/Input/GameplayInputRouter.cs, Player/Movement/PlayerMovementActionInput.cs,
  Combat/PlayerAttackController.cs, UI/** painéis, Skills/ActiveSkillSlots.cs,
  UI/Hotbar/**, Cave/Runtime/** debug)
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs (densidade real:
  ResolveTargetEnemyCount, constantes 16/32/44)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md (§Ranges — 12-20 stale)
docs/game_rules/cave_rules.md (Rule: Enemy Count Range — 12-20 stale)
docs/game_rules/GAME_RULES_INDEX.md (registro da game_rule nova)
docs/refinements/a_implementar/ref_futuro_map.md (8 links quebrados)
docs/refinements/a_implementar/pre_refinamentos/*.md (v1.0 × v1.1)
docs/project/DOCUMENT_DELETE_CANDIDATES.md (processo de arquivamento)
docs/specs/a_implementar/fable/fable_00_index_gap_analysis.md (mapa de domínio →
  fable specs, p/ o re-mapa)
.claude/rules/docs-governance.md (delete candidates; ADRs canônicos)
.claude/skills/decision-rule-extraction/SKILL.md (formato ADR superseding)
.claude/skills/docs-governance/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- 9 ADRs (ADR-0001..0009) — numeração do novo: ADR-0010;
- 12 game_rules + GAME_RULES_INDEX.md (input_map.md entra no índice);
- processo de delete candidates (docs/project/DOCUMENT_DELETE_CANDIDATES.md);
- fable_00_index_gap_analysis.md (mapa p/ os equivalentes fable).
Não existe:
- input_map.md; ADR de densidade real; links corretos no ref_futuro_map.
Auditar Fase 0 (tudo docs):
- varredura completa KeyCode. (Select-String) — tabela bruta tecla → arquivo → função;
- classificar: gameplay canônico × painel/UI × debug-only (debug listado em seção
  separada, marcado não-canônico);
- COLISÕES reais existentes (mesma tecla, contextos sobrepostos?) — listar como achados,
  sem resolver (resolução = spec de código futura);
- mapa dos 8 refinamentos → fable specs equivalentes VERIFICADAS (proposta inicial:
  equipment_durability/loot → fable_03+fable_06; damage_status_elements → fable_01+
  fable_02; player_combat_weapons_spells → fable_02+fable_07+fable_08; enemy_ai_roster_
  bestiary → fable_04+fable_33; cave_generation_checkpoints_boss → fable_05+fable_09;
  cave_entry_death_anya_corpse → fable_17+fable_44; skill_trees_slots_respec →
  fable_29; ui_ux_full_gameplay → fable_14 — CONFIRMAR cada uma lendo o escopo);
- linhas exatas a mudar em ADR-0005 (nota de superseding parcial) e cave_rules.md.
```

## Engineering stories

```text
Como F62, quero o input map canônico, para hints e tela Controles citarem teclas reais.
Como spec futura de ability, quero consultar um mapa antes de escolher tecla, para não
  colidir com as 138 bindings existentes.
Como agente lendo ADR-0005, quero a densidade documentada igual ao código aceito, para
  nunca "corrigir" o código na direção errada.
Como processo de refinamento, quero o ref_futuro_map apontando para specs que existem,
  para a cadeia de dependência documental voltar a fechar.
```

## Escopo

```text
Inclui (docs-only):
- docs/game_rules/input_map.md (game_rule NOVA):
  - tabela canônica de gameplay: movimento (WASD/setas), interação/ataque (E + hold,
    Q), defesa (Space dodge, LeftShift block), painéis (I/K/L/U/J/C/M/Tab/Esc),
    hotbar (1-4), skill slots (R/T/Y/G) — cada linha: tecla → ação → arquivo
    consumidor;
  - seção separada DEBUG (não-canônica): teclas de debug/dev (DebugHud, CaveDebug*,
    HotbarDebugInput, SaveInput etc.) com aviso de não-contrato;
  - seção de achados: colisões/ambiguidades detectadas na varredura (sem resolver);
  - REGRA CANÔNICA: "toda spec de UI/ability que adiciona/muda tecla DEVE consultar
    este mapa e atualizá-lo no mesmo diff" + nota de que F62 consome este doc;
  - registro no GAME_RULES_INDEX.md;
- docs/decisions/ADR-0010-cave-enemy-density-depth-scaling.md (superseding PARCIAL do
  ADR-0005 — apenas o range de inimigos):
  - contexto (12-20 planejado → slice implementou/aceitou 16/32/44 com escala por
    profundidade), decisão (valores reais + fórmula ResolveTargetEnemyCount), status
    do ADR-0005 anotado (nota "superseded em §Ranges/inimigos por ADR-0010" — sem
    reescrever o resto, que permanece canônico);
  - formato da skill decision-rule-extraction;
- docs/game_rules/cave_rules.md atualizada: Rule "Enemy Count Range" passa a 16-32
  (hard cap 44, escala por profundidade) citando ADR-0010; demais regras intactas;
- docs/refinements/a_implementar/ref_futuro_map.md corrigido:
  - 8 linhas re-mapeadas p/ as fable specs equivalentes verificadas (com os caminhos
    reais docs/specs/a_implementar/fable/fable_XX_*.md); observação por linha mantendo
    a história ("spec original substituída pelo planejamento fable");
  - links de pre_refinamentos conferidos (apontar só p/ arquivos que existem);
- arquivamento dos v1.0 superados (FARM_ESTRUTURA v1.0, VISAO_GERAL v1.0):
  - adicionar a docs/project/DOCUMENT_DELETE_CANDIDATES.md + marcar
    status: delete_candidate/superseded-by-v1.1 no cabeçalho, OU mover p/ pasta de
    arquivo git-tracked (caminho permitido sem pré-aprovação pela docs-governance);
  - NUNCA deletar direto (regra docs-governance #1).
```

## Fora de escopo

```text
Não inclui:
- QUALQUER mudança de código (nem tecla, nem densidade — o ADR documenta a realidade);
- resolver colisões de tecla encontradas (achados listados; resolução em spec futura);
- rebind/acessibilidade de input;
- deletar qualquer documento (só candidatar/arquivar via processo);
- reescrever ADR-0005 inteiro (nota de superseding parcial apenas);
- reconciliar outras seções do ADR-0005 (resource nodes 4-10 etc. — só inimigos; se a
  varredura achar outros ranges stale, LISTAR como achado, não corrigir).
```

## Regras de não duplicação

```text
Uma única fonte canônica de input: input_map.md (specs citam o doc, não re-listam
teclas por conta própria).
ADR-0010 não duplica o ADR-0005 — supersede APENAS o range de inimigos; cave_rules.md
cita o ADR, não repete a justificativa.
Re-mapa aponta p/ specs fable EXISTENTES (verificadas por Glob) — nunca criar specs
novas para fechar link.
Arquivamento segue o processo existente — nenhum mecanismo novo de arquivo.
```

## Critérios de aceite

### CA-1 Input map canônico e completo

- input_map.md lista TODAS as bindings reais da varredura (gameplay/painéis/debug em
  seções separadas), com arquivo consumidor por linha, regra de consulta/atualização, e
  está registrado no GAME_RULES_INDEX.md.
- Evidência: doc criado + contagem da varredura citada no doc (≈138 ocorrências/34
  arquivos na data) + índice atualizado.

### CA-2 ADR-0010 + cave_rules reconciliados

- ADR-0010 documenta min 16/max 32/cap 44/escala por profundidade com a fórmula real;
  ADR-0005 ganha nota de superseding parcial; cave_rules.md reflete os valores reais
  citando ADR-0010; zero mudança de código.
- Evidência: 3 diffs de docs + citação das constantes/linhas do CaveEnemySpawnPlanner.

### CA-3 ref_futuro_map fecha

- As 8 linhas apontam para arquivos fable existentes (verificados); nenhuma referência
  a docs/specs/a_implementar/spec_*.md inexistente sobra no doc.
- Evidência: diff + verificação de existência de cada alvo (Test-Path por link).

### CA-4 v1.0 arquivados pelo processo

- Os 2 pre_refinamentos v1.0 superados estão candidatados/arquivados (delete candidates
  + status OU pasta de arquivo), com as v1.1 declaradas como vigentes; nenhum delete
  direto no diff.
- Evidência: diff do DOCUMENT_DELETE_CANDIDATES.md/headers + git status sem deleções.

### CA-5 Docs validation verde

- `.\tools\docs\validate_docs.ps1` PASS após todas as mudanças (links novos válidos).
- Evidência: saída exit 0 no report.

---

# /speckit.plan

## Arquitetura alvo

```text
docs/game_rules/input_map.md                                   (NOVO)
docs/game_rules/GAME_RULES_INDEX.md                            (linha aditiva)
docs/decisions/ADR-0010-cave-enemy-density-depth-scaling.md    (NOVO)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md          (nota de superseding parcial)
docs/game_rules/cave_rules.md                                  (range de inimigos atualizado)
docs/refinements/a_implementar/ref_futuro_map.md               (8 links re-mapeados)
docs/refinements/a_implementar/pre_refinamentos/*_v1.0.md      (status/candidatura — 2 docs)
docs/project/DOCUMENT_DELETE_CANDIDATES.md                     (2 entradas)
docs/validation/fable_67_spec_canonical_governance_input_map_adr_execution_report.md
```

## Contratos

### Data contracts

- N/A (docs-only). Formato do input_map: tabela markdown
  `| Tecla | Ação | Contexto | Arquivo consumidor |`.

### Runtime contracts

- N/A — ZERO mudança de código. A varredura é leitura.

### Event contracts

- N/A.

### Save contracts

- N/A.

### UI contracts

- input_map.md vira contrato documental consumido por F62 (hints/tela Controles) e por
  toda spec futura de tecla.

## Sistemas afetados

```text
Governança de decisões (ADR novo + nota no ADR-0005)
game_rules (input_map nova; cave_rules corrigida; índice)
Refinements (mapa corrigido; 2 arquivamentos por processo)
F62 (consumidora downstream do input_map)
```

## Arquivos permitidos

```text
SOMENTE os listados na Arquitetura alvo + docs/validation/** (report).
```

## Arquivos proibidos

```text
Assets/** (qualquer código — leitura apenas)
Packages/** ; ProjectSettings/**
docs_old/ e caminhos legados (docs-governance)
Deleção de QUALQUER arquivo (arquivamento via processo, nunca delete)
ADR-0005 além da nota de superseding parcial
Outras game_rules além de cave_rules.md e do índice
```

## Estratégia de implementação

```md
### Fase 0 — Varredura e verificação
Varredura KeyCode. completa (tabela bruta); classificação gameplay/painéis/debug;
colisões anotadas; mapa dos 8 equivalentes fable VERIFICADO por leitura de escopo;
linhas exatas do ADR-0005/cave_rules.

### Fase 1 — Input map
input_map.md (tabelas + regra canônica + seção debug + achados) + GAME_RULES_INDEX.

### Fase 2 — ADR de densidade
ADR-0010 (formato decision-rule-extraction) + nota no ADR-0005 + cave_rules.md.

### Fase 3 — Refinamentos e fechamento
ref_futuro_map re-mapeado (8 links verificados) + candidatura/arquivamento dos 2 v1.0;
validate_docs PASS; execution report.
```

## Paralelização

- Parallelizable: YES (docs-only).
- Parallel group: fable_batch11_shipping.
- Can run with: todas as demais do batch.
- Must not run with: N/A.
- Shared files/systems that require lock: nenhum código; apenas avisar F62 (consome
  input_map) — F67 idealmente roda ANTES.
- Reason: zero superfície de código; só artefatos documentais novos/corrigidos.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: N/A
```

## Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO (docs-only — Phase 2-3 NOT IN SCOPE, promoção
  após BUILD_VALIDATED conforme spec_quality_gate)
Human validation timing: NOT REQUIRED (docs-only)
```

## Riscos técnicos

```text
Risco: input map nascer incompleto (tecla esquecida = doc que mente).
Mitigação: varredura mecânica (Select-String KeyCode.) é a fonte; contagem de
ocorrências citada no doc; revisão arquivo-a-arquivo dos 34.

Risco: ADR-0010 "legalizar" um valor que o humano considerava bug.
Mitigação: o ADR registra explicitamente a origem (slice implementado/aceito em
validações de wave) e marca a decisão como reversível por ADR futuro; report destaca
o delta 12-20 → 16-32/44 p/ visto humano.

Risco: re-mapa apontar p/ fable spec errada (escopo divergente).
Mitigação: cada equivalência verificada LENDO o escopo da fable spec alvo na Fase 0;
mapeamentos 1→N permitidos (linha cita as 2 specs).

Risco: arquivar v1.0 que ainda tem conteúdo não absorvido pela v1.1.
Mitigação: diff v1.0 × v1.1 na Fase 0; se houver conteúdo único, anotar na candidatura
(arquiva mesmo assim, com nota) — decisão final de deleção continua humana.

Risco: validate_docs quebrar por link novo.
Mitigação: CA-5 obriga PASS; links verificados por Test-Path antes do commit.
```

## Rollback

```text
git revert dos diffs de docs (nenhum arquivo deletado, nenhum código tocado).
Candidaturas no DOCUMENT_DELETE_CANDIDATES são reversíveis por natureza.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: varredura KeyCode. completa + classificação + colisões; verificação
        dos 8 equivalentes fable; diff v1.0 × v1.1; linhas do ADR-0005/cave_rules.
- [ ] T002 — input_map.md + GAME_RULES_INDEX.md.
- [ ] T003 — ADR-0010 + nota de superseding no ADR-0005 + cave_rules.md.
- [ ] T004 — ref_futuro_map re-mapeado + candidatura/arquivamento dos 2 v1.0;
        validate_docs PASS; execution report (delta de densidade destacado p/ humano).
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
.\tools\docs\run_strict_validation.ps1
# docs-only: builds dotnet devem permanecer 0E por ausência de diff de código
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

## Testing Quality Gate

- Changed runtime code: NO (docs-only)
- Changed deterministic logic: NO
- Requires EditMode tests: NO — justificativa: mudança puramente documental
  (testing-quality-gate: "purely documentation-only change")
- Requires PlayMode automated or final human scenario: NO (Phase 2-3 NOT IN SCOPE)
- Requires regression test: validate_docs PASS + zero diff em Assets/**
- Human validation timing: NOT REQUIRED; delta de densidade destacado p/ visto humano
- Minimum validation evidence for ACCEPTED: validate_docs exit 0 + CA-1..5 evidenciados
  no report

## Definition of Done

```text
input_map.md canônico (varredura completa, regra de consulta/atualização, debug
separado, registrado no índice); ADR-0010 + nota no ADR-0005 + cave_rules.md
reconciliados com o código real (16/32/44, zero mudança de código); ref_futuro_map com
8 links válidos verificados; 2 v1.0 candidatados/arquivados pelo processo sem delete
direto; validate_docs exit 0; execution report criado.
```

## Anti-regressão

```text
Zero diff em Assets/** (docs-only — git status prova).
Nenhum documento deletado (somente candidatura/arquivo git-tracked).
ADR-0005 permanece canônico em tudo exceto o range de inimigos (nota cirúrgica).
Nenhum caminho legado (docs_old/, specs/ raiz) criado ou citado.
Links novos todos existentes (Test-Path); validate_docs PASS.
```
