# SPEC — Editor: Validador de Consistência dos Catálogos (itens/bestiário/quests/skills)

> **Spec ID:** `fable_30_spec_catalog_consistency_validator`
> **Status:** A implementar
> **Wave:** FABLE Batch 5
> **Priority:** P1
> **Type:** Editor / Tooling
> **Domain:** Tooling
> **Parallelizable:** YES (editor-only, sem locks de runtime)
> **Parallel group:** fable_batch_5
> **Can run with:** qualquer spec que não edite `Editor/Validation/**`
> **Must not run with:** N/A (sem locks de runtime)
> **Repo lock scope:** `Editor/Validation/**` apenas
> **Depends on:**
> - nenhuma (valida o que existir; roda incremental)
> **Blocks:**
> - nada (mas F32/F33/F34/F29 DEVEM rodá-lo no closeout)
> **Scope:** menu editor que cruza assets gerados × catálogos canônicos e reporta divergências.
> **Out of scope:** auto-fix (só relatório), validação de cenas, lint de texto.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## Contexto

Os 6 catálogos canônicos gerados a partir das decisões FABLE (itens ~118, bestiário 60+4,
quests ~86, skills ~70, balance, HUD) serão materializados como assets por geradores em
F29/F32/F33/F34. Cada catálogo referencia os outros: drops do bestiário apontam para itens,
recompensas de quest apontam para itens/skills, receitas apontam para ingredientes, lojas
apontam para itens, skills ativas apontam para executores.

O repo já tem um padrão de validadores editor (`Editor/Validation/Validate*.cs` com menu +
log + exit code para batchmode) que esta spec reusa. O que não existe é a validação
CRUZADA: nada hoje detecta um drop de inimigo apontando para item inexistente ou uma quest
premiando um item fantasma. Como os geradores rodam em specs separadas (e em ordens
possivelmente diferentes), o validador precisa ser INCREMENTAL: categoria cujo gerador
ainda não rodou é SKIPPED, não ERROR.

Esta spec é deliberadamente pequena e independente (editor-only, sem locks de runtime)
para poder rodar ANTES dos geradores e servir de gate de closeout para eles.

## Problema

Sem um validador cruzado, divergências entre catálogos e assets (ID canônico faltante,
drop referenciando item inexistente, quest premiando item fantasma, contagem parcial de
geração) só aparecem em runtime — no pior momento e no pior lugar para diagnosticar.
Os geradores F29/F32/F33/F34 materializarão centenas de assets; sem contagens esperadas ×
reais auditáveis, a regra de evidência de geração (`generated-asset-evidence`) não tem
ferramenta para ser cumprida.

## Objetivo

Ao final desta spec, deve existir o menu `CindarsHope/Validate/Catalog Consistency`
(e o método batchmode equivalente via `-executeMethod`) que produz um relatório único com
contagens esperadas × reais por categoria e todas as referências cruzadas quebradas,
agrupadas por severidade, com exit code ≠ 0 na presença de ERROR — permitindo que
F29/F32/F33/F34 o citem como evidência de closeout, sem alterar nenhum sistema de runtime.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md
docs/design/gameplay/player_character/PLAYER_SKILL_TREES_DIRECTION.md
.claude/rules/generated-asset-evidence.md
.claude/skills/unity-validation/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- padrão de validadores editor (Editor/Validation/Validate*.cs com menu + log +
  exit code p/ batchmode) — reusar o harness;
- ItemDatabase / EnemyDataSO / QuestRegistry / SkillDataSO — fontes de leitura dos assets;
- regra generated-asset-evidence (evidência de geração obrigatória nos geradores).
Não existe:
- validação cruzada de catálogo (catálogo×assets e catálogo×catálogo);
- tabela de expectativas (contagens/IDs canônicos) versionada em código;
- modo incremental com SKIPPED.
Auditar Fase 0:
- assinatura dos validadores existentes (menu, formato de log, convenção de exit code)
  para reusar o harness sem fork.
```

## Engineering stories

```text
Como gerador de catálogo (F29/F32/F33/F34), quero um validador único para anexar no closeout como evidência.
Como maintainer, quero saber em 1 relatório quais IDs canônicos faltam e quais assets são extras não documentados.
Como pipeline batchmode, quero exit code ≠ 0 com ERROR para falhar o gate automaticamente.
Como executor incremental, quero SKIPPED (não ERROR) para categorias cujo gerador ainda não rodou.
```

## Escopo

```text
Inclui:
- CatalogExpectations (tabela estática versionada em código): contagens por categoria
  (itens por grupo, criaturas por banda, quests por fonte, skills por árvore) + listas de
  IDs canônicos, com comentário de fonte por linha (doc + seção que justifica o número);
- checagens cruzadas (8):
  (a) ID canônico sem asset;
  (b) asset sem ID canônico (extra não documentado);
  (c) drop de inimigo → item inexistente;
  (d) recompensa de quest → item/skill inexistente;
  (e) receita → ingrediente inexistente;
  (f) shop entry → item inexistente;
  (g) skill ativa → executor/flag dormante válido;
  (h) BestiaryEntryId duplicado;
- saída: log agrupado por severidade (ERROR = referência quebrada, WARN = contagem
  divergente, INFO = dormante declarado) + exit code ≠ 0 quando houver ERROR
  (batchmode-friendly, padrão dos validadores existentes);
- modo incremental: categorias cujo gerador ainda não rodou reportam SKIPPED (não ERROR);
  categoria gerada parcial reporta WARN com contagem X/Y;
- motor de cross-ref PURO (sem dependência de AssetDatabase no núcleo) para ser testável;
- EditMode tests: motor de cross-ref com dados sintéticos (asset fake + expectativa fake)
  cobrindo cada severidade e o modo SKIPPED.
```

## Fora de escopo

```text
Não inclui:
- auto-fix (só relatório — nenhuma escrita em assets);
- validação de cenas (validadores existentes cobrem);
- lint de texto/ortografia;
- validação de balance numérico (TTK/curvas — telemetria de playtest);
- execução dos geradores (cada spec geradora roda o seu).
```

## Regras de não duplicação

```text
Não duplicar validadores existentes (cena/registro) — esta spec valida só catálogo×catálogo
e catálogo×assets.
Não criar segundo harness de menu/log/exit code — reusar a assinatura auditada na Fase 0.
Sem auto-fix — geradores corrigem; validador só reporta.
```

## Critérios de aceite

### CA-1 Referência quebrada é ERROR identificável

- Drop apontando para item inexistente gera ERROR com enemyId + itemId no log.
- Evidência: EditMode test do motor com drop sintético quebrado + log real de execução.

### CA-2 Incremental honesto

- Categoria não gerada = SKIPPED; categoria gerada parcial = WARN com contagem X/Y.
- Evidência: EditMode tests dos dois caminhos com expectativas sintéticas.

### CA-3 Gate batchmode

- Exit code 0 só sem ERRORs; com ERROR, exit code ≠ 0; o método roda via -executeMethod.
- Evidência: execução batchmode registrada (comando + exit code) no execution report.

### CA-4 Motor testado

- O motor de cross-ref puro é coberto por testes sintéticos (asset fake + expectativa
  fake) para as 8 checagens.
- Evidência: CatalogValidatorTests verdes no EditMode.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Validation/
  ValidateCatalogConsistency.cs  (NOVO — menu CindarsHope/Validate/Catalog Consistency +
                                  método batchmode; coleta assets e delega ao motor)
  CatalogExpectations.cs         (NOVO — tabela estática versionada: contagens + IDs
                                  canônicos, comentário de fonte por linha)
Assets/_Game/Tests/EditMode/Tooling/
  CatalogValidatorTests.cs       (NOVO — motor puro extraído, testado com dados sintéticos)
docs/validation/
  fable_30_spec_catalog_consistency_validator_execution_report.md
```

## Contratos

### Data contracts

- `CatalogExpectations`: por categoria — contagem esperada + lista de IDs canônicos +
  fonte (doc/seção). Tabela estática em código, versionada com o repo.
- Resultado de validação: lista de findings {severidade ERROR/WARN/INFO/SKIPPED,
  categoria, ids envolvidos, mensagem}.

### Runtime contracts

- N/A — editor-only. O motor de cross-ref é classe pura (entrada: snapshots de
  expectativas + assets; saída: findings) para permitir teste sem AssetDatabase.

### Event contracts

- N/A — nenhuma comunicação de gameplay.

### Save contracts

- N/A — nenhum dado persiste.

### UI contracts

- N/A — saída é log de editor/console, padrão dos validadores existentes.

## Sistemas afetados

```text
Tooling editor (Validation)
Leitura read-only de: ItemDatabase, EnemyDataSO/roster, QuestRegistry, SkillDataSO
Testes EditMode (Tooling)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Validation/** (ValidateCatalogConsistency, CatalogExpectations)
Assets/_Game/Tests/EditMode/Tooling/**
docs/validation/**
csproj includes (Assembly-CSharp-Editor / testes)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual (e por escrita do validador — read-only)
Packages/**
ProjectSettings/**
Qualquer script de runtime (Assets/_Game/Scripts/** fora de Editor/)
Databases/registries (somente leitura — sem auto-fix)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar a assinatura dos validadores existentes (menu, log, exit code) para reusar o
harness; mapear como ler ItemDatabase/roster/QuestRegistry/SkillDataSO em editor.

### Fase 1 — Expectativas e motor
CatalogExpectations a partir das 4 fontes (com comentário de fonte por linha) + motor
de cross-ref puro + testes sintéticos.

### Fase 2 — Checagens e severidades
As 8 checagens (a)-(h) + severidades ERROR/WARN/INFO + modo incremental SKIPPED.

### Fase 3 — Menu e batchmode
Menu CindarsHope/Validate/Catalog Consistency + método -executeMethod com exit code;
log agrupado por severidade.

### Fase 4 — Fechamento
csproj editor; run_strict_validation; execução real registrada; execution report.
```

## Paralelização

- Parallelizable: YES.
- Parallel group: fable_batch_5.
- Can run with: qualquer spec que não edite `Editor/Validation/**`.
- Must not run with: N/A.
- Shared files/systems that require lock: `Editor/Validation/**` apenas.
- Reason: editor-only, leitura read-only de assets, zero contratos de runtime tocados.

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
Requires unsubscribe pattern: NO
```

## Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (leitura apenas)
Requires Play Mode final validation: NO (editor tooling)
Human validation timing: NOT REQUIRED
```

## Riscos técnicos

```text
Risco: expectativas desatualizarem quando um catálogo canônico mudar.
Mitigação: tabela única com comentário de fonte por linha (doc + seção); divergência de
contagem é WARN visível, não silêncio.

Risco: validador acoplar-se a AssetDatabase e ficar intestável.
Mitigação: motor de cross-ref puro (snapshots in-memory); a casca editor só coleta.

Risco: falso ERROR antes dos geradores rodarem travar o pipeline.
Mitigação: modo incremental — categoria sem geração reporta SKIPPED.

Risco: rodar em paralelo com Unity batchmode de outra tarefa.
Mitigação: regra no-parallel-unity-batchmode — execuções sequenciais, log próprio.
```

## Rollback

```text
Remover o menu/método e os arquivos novos (ValidateCatalogConsistency,
CatalogExpectations, testes). Nenhum asset ou sistema de runtime foi alterado;
não há estado a desfazer.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar harness de validadores existentes (menu/log/exit code).
- [ ] T002 — CatalogExpectations (das 4 fontes, com comentário de fonte) + motor cross-ref puro + testes.
- [ ] T003 — 8 checagens (a)-(h) + severidades ERROR/WARN/INFO + SKIPPED incremental.
- [ ] T004 — Menu + batchmode (-executeMethod, exit code); csproj editor; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (motor de cross-ref)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: NOT REQUIRED (editor-only)
- Requires regression test: N/A (aditivo editor — nenhum comportamento existente alterado)
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: testes do motor + log de uma execução real

## Definition of Done

```text
Validador funcional e incremental (8 checagens, severidades, SKIPPED), motor puro
testado, menu + batchmode com exit code correto, execução real registrada; geradores
F29/F32/F33/F34 passam a citá-lo no closeout; nenhum arquivo proibido alterado;
builds 0E; execution report criado.
```

## Anti-regressão

```text
Validadores existentes (cena/registro) intactos — nenhum fork do harness.
Nenhuma escrita em assets (read-only absoluto — sem auto-fix).
Nenhum código de runtime adicionado/alterado.
Exit code de batchmode nunca converte ERROR em pass (regra de honestidade de validação).
```
