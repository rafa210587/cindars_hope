# SPEC — Dados: Expansão do Bestiário para 60 Criaturas + 4 Bosses Finais (catálogo canônico)

> **Spec ID:** `fable_33_spec_bestiary_data_expansion_60_creatures`
> **Status:** A implementar
> **Wave:** FABLE Batch 5
> **Priority:** P1
> **Type:** Data / Editor
> **Domain:** Cave / Enemy
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_5
> **Can run with:** specs que não tocam EnemyDataSO/roster/spawn planner
> **Must not run with:** F05, F06, F24 (consumidores de EnemyDataSO)
> **Repo lock scope:** geradores de EnemyDataSO, EnemyDatabase/roster, CaveEnemySpawnPlanner tabelas por banda
> **Depends on:**
> - F24 (22 Moves no enum)
> - F06 (loot/vulnerability fields)
> - F32 (itens de drop existem)
> - F30 (validador)
> **Blocks:**
> - F21 (knowledge cobre os 60)
> - F05 (bosses finais)
> **Scope:** materializar as 64 fichas do CAVE_BESTIARY_CATALOG como EnemyDataSO + tabelas de spawn por banda.
> **Out of scope:** comportamentos NOVOS além dos 22 Moves (dormante documentado), arte (placeholder shapes/cores por família), Ithryndor scripted finale (asset entra; orquestração = spec futura de endgame).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

O `CAVE_BESTIARY_CATALOG` v1.0 tem ficha completa de 60 criaturas de banda + 4 chefes
finais (64 fichas): Move oficial, HP, DMG, DEF, XP, ataques com windup, defesas com
counterplay declarado, comportamento observável, drops ligados ao catálogo de itens,
narração no tom de Vaalara, SpoilerTier (comuns 0-1, minibosses 2, bosses de gate 3, os
Quatro do 101 = 4), banda de níveis e tamanho D&D natural (Tiny 0.5×0.5 a Gargantuan 4×4
em tiles). As 7 bandas temáticas (Stone 1-10, Fungal 11-25, Ice 26-40, Fire 41-55,
Ruins 56-70, Deep 71-85, Void 86-101) têm tabela-resumo própria, cada uma com 2
minibosses errantes e 1 boss de gate; marcações transversais incluem [NOTURNA] (20h-06h
ou pico de Nyx), [AQUÁTICA] (só níveis com lago) e [NÃO-AGRESSIVA] (quests secretas).

O roster runtime atual tem ~20 inimigos. Os campos de suporte já chegaram pelas
dependências: 22 Moves no enum do EnemyBrain (F24), loot/vulnerability (F06) e os itens
de drop (F32). Os valores das fichas valem para o nível mínimo da faixa; a escala dentro
da banda é +12% HP / +8% DMG por nível (BALANCE_CURVES §5 — DEF fixa por criatura), e os
multiplicadores por TIPO já estão APLICADOS nos números das fichas.

Esta spec é regida pelo stable-run (ADR-0005 / cave_rules): as tabelas de spawn por banda
têm de ser determinísticas, e o closeout exige replay validator PASS.

## Problema

Com ~20 inimigos, a caverna repete as mesmas criaturas por 100 níveis — a variedade
prometida pelas 7 bandas não existe em jogo. F21 (bestiary knowledge) não tem o que
cobrir e F05 (bosses) não tem os assets finais. Pior: o roster atual carrega nomes
trademarked (Drow/Duergar) que a decisão humana mandou renomear (Veilkin/Gravedelver) —
risco de produto, não só de conteúdo. E qualquer tabela de spawn não-determinística
violaria o contrato stable-run da FASE9F.

## Objetivo

Ao final desta spec, o projeto deve ter as 64 fichas materializadas como EnemyDataSO por
gerador idempotente (por enemyId), com todos os campos da ficha, fórmula de escala
(+12%/+8%) aplicada no planner (não nos assets), tabelas de spawn determinísticas por
banda no CaveEnemySpawnPlanner, renames aplicados, validação F30 (64/64) e replay
validator PASS — permitindo F21/F05, sem comportamento novo além dos 22 Moves e sem
quebrar o stable-run.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (INTEIRO — fonte das fichas)
docs/design/gameplay/balance/BALANCE_CURVES_DIRECTION_v1.0.md (multiplicadores por TYPE)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§2)
.claude/rules/cave-stable-run.md
.claude/rules/generated-asset-evidence.md
.claude/skills/cave-stable-run-guard/SKILL.md
.claude/skills/unity-asset-generation/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemyDataSO (auditar campos vs. ficha do catálogo);
- geradores de enemy assets (padrão a estender);
- roster/registry de inimigos;
- CaveEnemySpawnPlanner (bandas — recebe as tabelas novas);
- EnemyBrain com 22 Moves (F24);
- loot/vulnerability fields (F06).
Não existe:
- ~44 criaturas do catálogo;
- bosses finais (os Quatro do 101);
- tamanhos D&D nos assets;
- tabela de spawn completa por banda.
Auditar Fase 0:
- diff roster existente × catálogo (mapear renames Veilkin/Gravedelver — quais assets
  existentes mudam de nome/ID e quem os referencia);
- campos do EnemyDataSO faltantes p/ a ficha (size/secondary move/spoiler);
- formato atual das tabelas do planner (como injetar pesos por banda).
```

## Engineering stories

```text
Como jogador, quero criaturas diferentes a cada banda para a descida ter identidade (Stone ≠ Fungal ≠ Void).
Como CaveEnemySpawnPlanner, quero tabelas determinísticas por banda para o stable-run permanecer intacto.
Como F21 (bestiário), quero BestiaryEntryId e SpoilerTier em todas as 64 fichas.
Como produto, quero zero nomes trademarked no roster (Veilkin/Gravedelver aplicados).
```

## Escopo

```text
Inclui:
- gerador GenerateCanonicalBestiary (idempotente por enemyId): 64 fichas com TODOS os
  campos (stats base da banda, Move primário/secundário, família F06, vulnerabilidades/
  resistências, drops com pesos, SpoilerTier, BestiaryEntryId, tamanho {Small/Medium/
  Large/Huge/Gargantuan → escala do transform placeholder}, cor/forma por família);
- escala por nível DENTRO da banda: +12% HP / +8% DMG por nível (fórmula no planner, não
  nos assets — assets guardam a base da banda; DEF fixa por criatura);
- minibosses (≥2 por gate — decisão Q12.3) e bosses de gate marcados (isMiniboss/isBoss,
  tamanho +1 categoria);
- 4 finais: Vel-Karaúm, Cindrathel, Archivist of Silence, Ithryndor (assets completos;
  mecânicas especiais ficam dormantes com flag até F05/endgame spec);
- tabelas de spawn por banda no CaveEnemySpawnPlanner (composição determinística por
  nível — pesos do catálogo; aquáticos só em níveis com lago F09; noturnas conforme
  marcação do catálogo — decisão de spawn na geração do nível, stable-run vence o relógio);
- renames aplicados (Drow→Veilkin, Duergar→Gravedelver) em assets e referências;
- closeout: rodar F30 (categoria bestiário 64/64) + replay validator (stable-run);
- EditMode tests: fórmula de escala, contagem por banda, renames aplicados, pesos de
  spawn somam corretamente, spawn determinístico com a tabela nova (mesmo seed = mesma
  composição).
```

## Fora de escopo

```text
Não inclui:
- comportamentos NOVOS além dos 22 Moves (comportamento especial não coberto = dormante
  documentado na ficha, campo notes);
- arte final (placeholder shapes/cores por família);
- orquestração do Ithryndor scripted finale (asset entra; orquestração = spec futura de
  endgame);
- mecânicas de boss de F05 (fases/janelas — F05 consome os assets daqui);
- loot tables finais por família (autoria F06 usando os IDs daqui).
```

## Regras de não duplicação

```text
Não criar segundo roster/registry — estender os geradores existentes.
Comportamento especial não coberto pelos 22 Moves = dormante documentado na ficha
(campo notes), nunca Move novo nem brain paralelo.
Não duplicar fórmula de escala — ponto único no planner (assets guardam só a base).
Não recriar tabelas de loot — pesos referenciam o sistema F06.
```

## Critérios de aceite

### CA-1 Catálogo completo e íntegro

- F30 reporta bestiário 64/64; zero referências de drop quebradas (checagem (c)).
- Evidência: log do ValidateCatalogConsistency anexado ao execution report.

### CA-2 Fórmula de escala

- Criatura da banda 11-25 spawnada no nível 20 tem HP = base × 1.12^(20-11)
  (e DMG = base × 1.08^(20-11)); DEF inalterada.
- Evidência: EditMode test da fórmula no planner com casos por banda.

### CA-3 Spawn determinístico (stable-run)

- Mesmo seed = mesma composição de inimigos por nível com as tabelas novas;
  revisitas não rerolam (contrato FASE9F).
- Evidência: EditMode test de determinismo + replay validator PASS no closeout.

### CA-4 Renames e política de nomes

- Veilkin/Gravedelver aplicados em assets e referências; zero nomes trademarked no
  roster; nomes de exibição em inglês.
- Evidência: EditMode test de varredura de nomes + diff da Fase 0 resolvido.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Enemies/
  GenerateCanonicalBestiary.cs        (NOVO — gerador idempotente por enemyId)
  GenerateCanonicalBestiary.Band*.cs  (tabela data-driven das 64 fichas — 1 arquivo por
                                       banda, partial class, na ordem do catálogo)
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs
  (campos aditivos: size, secondary move, SpoilerTier, isMiniboss/isBoss, notes/dormant)
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs
  (tabelas de spawn por banda + fórmula de escala +12%/+8% por nível)
Assets/_Game/Tests/EditMode/Cave/BestiaryDataTests.cs (NOVO)
docs/validation/fable_33_spec_bestiary_data_expansion_60_creatures_execution_report.md
```

## Contratos

### Data contracts

- `EnemyDataSO` (aditivo): stats BASE da banda (nível mínimo da faixa), Move primário/
  secundário (enum F24), família (F06), vulnerabilidades/resistências, drops com pesos
  (IDs F32), `BestiaryEntryId` (= enemy_id, único), `SpoilerTier` (0-4), size class
  (Tiny..Gargantuan → escala do transform placeholder), `isMiniboss`/`isBoss`
  (tamanho +1 categoria), `notes` (dormantes documentados).
- Tabela de spawn por banda: {enemyId, peso, faixa de nível, flags aquática/noturna/
  não-agressiva}.

### Runtime contracts

- `CaveEnemySpawnPlanner`: composição por nível derivada da tabela da banda com seed
  determinística (CaveWorldSeed + CaveRunSeed + CaveLevel + salt estável — ADR-0005);
  fórmula de escala aplicada no spawn: HP × 1.12^(lvl−min), DMG × 1.08^(lvl−min).
- Aquáticas só em níveis com lago (F09); noturnas decididas NA GERAÇÃO do nível
  (stable-run vence o relógio).

### Event contracts

- N/A — nenhum evento novo; spawn flui pelo pipeline existente.

### Save contracts

- N/A — nenhum schema novo; snapshots de nível visitado continuam persistindo IDs
  estáveis existentes (renames auditados contra snapshots na Fase 0).

### UI contracts

- N/A — placeholder shapes/cores por família via campos do asset; sem UI nova
  (bestiário UI = F20/F21).

## Sistemas afetados

```text
Enemy data (assets/roster/geradores)
CaveEnemySpawnPlanner (tabelas por banda + escala)
Stable-run da caverna (determinismo — ADR-0005)
Drops (IDs F32 referenciados; pesos p/ F06)
Bestiary knowledge (F21 consome BestiaryEntryId/SpoilerTier)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Enemies/** (gerador + tabelas partial por banda)
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs (campos aditivos)
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs (tabelas + fórmula)
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (assets SÓ via gerador editor)
Packages/**
ProjectSettings/**
EnemyBrain/Moves (consumir o enum F24, não alterar)
Geração procedural de layout da caverna (somente tabelas de spawn do planner)
SaveManager / snapshots (somente leitura na auditoria de renames)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Diff roster existente × catálogo (incluindo renames Veilkin/Gravedelver e quem referencia
os IDs antigos); auditoria de campos do EnemyDataSO; formato das tabelas do planner.

### Fase 1 — Campos e fórmula
Campos aditivos no EnemyDataSO + fórmula de escala (+12%/+8%) no planner + testes.

### Fase 2 — Tabela das 64
Tabela data-driven das 64 fichas (partial class por banda, ordem do catálogo) + gerador
idempotente por enemyId + testes de contagem/renames.

### Fase 3 — Spawn por banda
Tabelas de spawn determinísticas (pesos do catálogo; aquáticas/noturnas/minibosses) no
planner + testes de determinismo + replay validator.

### Fase 4 — Geração, validação e fechamento
Rodar gerador (evidência obrigatória) + F30 (64/64) + replay validator PASS; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch_5.
- Can run with: specs que não tocam EnemyDataSO/roster/planner.
- Must not run with: F05, F06, F24 (consumidores de EnemyDataSO).
- Shared files/systems that require lock: geradores de EnemyDataSO, EnemyDatabase/roster,
  tabelas por banda do CaveEnemySpawnPlanner.
- Reason: os consumidores leem/estendem os mesmos assets e tabelas; edição concorrente
  quebra o diff de renames e o determinismo auditado.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO (renames auditados contra snapshots na Fase 0; se um ID
salvo mudar, documentar fallback de ID inválido antes do closeout)
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
Changes ScriptableObjects/assets: YES — somente via gerador editor
Geração de assets: EVIDÊNCIA obrigatória (menu/método, log, exit code, contagens 64/64)
Requires Play Mode final validation: YES (lote — variedade percebida por banda)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: tabela gigante (64 fichas) inauditável num arquivo só.
Mitigação: 1 arquivo por banda (partial class), na ordem do catálogo, revisável lado a
lado com as tabelas-resumo.

Risco: quebrar o stable-run (FASE9F) com tabelas/pesos não determinísticos.
Mitigação: seeds CaveWorldSeed+CaveRunSeed+CaveLevel+salt (ADR-0005); teste de
determinismo; replay validator obrigatório no closeout.

Risco: renames quebrarem referências/snapshots com IDs antigos.
Mitigação: diff completo na Fase 0 + teste de varredura + fallback documentado.

Risco: ficha exigir comportamento que os 22 Moves não cobrem.
Mitigação: dormante documentado no campo notes (INFO no F30) — nunca Move novo aqui.
```

## Rollback

```text
Planner usa a tabela antiga por flag (rollback imediato do spawn). Assets novos ficam
inertes sem a tabela nova; reverter gerador/campos/testes desfaz a spec. Renames são a
única parte com alcance — reverter exige o diff da Fase 0 (documentado no report).
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Diff roster × catálogo (renames/referências) + auditoria de campos.
- [ ] T002 — Campos aditivos + fórmula de escala (+12%/+8%) no planner + testes.
- [ ] T003 — Tabela das 64 fichas (partial por banda) + gerador idempotente + testes.
- [ ] T004 — Tabelas de spawn por banda (pesos/aquáticos/noturnas/minibosses) + replay validator.
- [ ] T005 — Rodar gerador + F30 (64/64) + evidência; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (escala/spawn por seed)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote — variedade percebida)
- Requires regression test: YES (replay validator — stable-run intacto)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + log do gerador + log F30 (64/64) +
  replay PASS

## Definition of Done

```text
64 fichas materializadas com todos os campos; escala +12%/+8% no planner (DEF fixa);
spawn por banda determinístico (replay PASS); renames Veilkin/Gravedelver aplicados;
zero trademark; minibosses ≥2 por gate e bosses marcados; 4 finais como assets com
dormantes documentados; evidência de geração + log F30 anexados; builds 0E; report.
```

## Anti-regressão

```text
Stable-run intacto: mesma CaveRunSeed = mesma composição em revisita (ADR-0005);
ForwardExit/BackExit nunca mudam seed.
Nenhum GUID aleatório/timestamp em IDs estáveis de conteúdo runtime.
Roster existente não duplicado; brains/Moves F24 inalterados.
Drops apontam só para IDs existentes (F32) — zero refs quebradas no F30.
Zero nomes trademarked em assets e exibição.
```
