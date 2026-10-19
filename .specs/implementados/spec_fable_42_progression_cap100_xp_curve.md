# SPEC — Progressão: Cap 100 + Curva de XP Canônica + Pontos de Skill por Nível

> **Spec ID:** `fable_42_spec_progression_cap100_xp_curve`
> **Status:** BUILD_VALIDATED (executada — E05; evidência: docs/validation/fable_42_spec_progression_cap100_xp_curve_execution_report.md) | **Wave:** FABLE Batch 1 | **Priority:** P0
> **Type:** Runtime / Save | **Domain:** Player / Progression
> **Parallelizable:** NO — SOLO (XP/level tocam tudo; save de player)
> **Must not run with:** QUALQUER outra spec
> **Repo lock scope:** PlayerLevelService/XP, save de player, fontes de XP
> **Depends on:** F13 (save schema estável)
> **Blocks:** F29 (50 pontos), F39 (investimento), F34 (XP de quest), F33 (XP por criatura)
> **Scope:** cap 100, XPnext = 60×N^1.5, 1 ponto/2 níveis (=50), fontes de XP normalizadas.
> **Out of scope:** prestige/pós-100, rebalance fino por playtest (curva entra como autorada).

required_adrs: []
required_game_rules: [player_rules.md]

## Contexto / Problema / Objetivo

Decisão Q8.1 (cap 100, "me traga uma proposta melhor" → resolvida): cap 100 preserva o
canônico 1 ponto de skill a cada 2 níveis = exatamente 50 pontos (orçamento das árvores).
BALANCE_CURVES v1.0: XPnext = 60 × N^1.5; XP por criatura = ficha do bestiário (banda);
XP de quest escala (F34). O level service atual tem cap/curva antigos (auditar). Objetivo:
fonte única de verdade da progressão + migração de save (XP total recomputa nível).

## Fontes obrigatórias

```text
docs/design/gameplay/balance/BALANCE_CURVES_DIRECTION_v1.0.md (INTEIRO — §curva/§fontes)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§8)
docs/design/gameplay/player_character/PLAYER_SKILL_TREES_DIRECTION.md (1pt/2níveis)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe: PlayerLevelService (auditar: cap/curva/eventos), XP por kill (EnemyDataSO),
SkillPoint grant, save de player (auditar campos: xp total? nível?).
Não existe: cap 100, curva canônica, normalização de fontes.
Auditar Fase 0: TODAS as fontes de XP atuais (lista no report) + shape do save.
```

## Escopo

```text
- ProgressionCurve (puro, estático): XpForNext(n) = round(60 × n^1.5); TotalXpForLevel(n);
  LevelForTotalXp(xp); cap 100 (XP além acumula mas não sobe);
- skill points: a cada nível PAR (+1) → 50 no cap; grant idempotente por nível
  (não re-concede no load);
- fontes normalizadas: kill (ficha F33 quando rodar; até lá valores atuais), quest (F34
  fórmula), descoberta de nível novo da caverna (+15×banda — curva doc), primeira colheita
  de cultivo novo (+10) — cada fonte com hook nomeado;
- save: armazenar XP TOTAL (fonte de verdade); migração: save antigo {nível, xpParcial} →
  totalXp = TotalXpForLevel(nível)+xpParcial; nível sempre derivado no load;
- eventos: PlayerLeveledUpEvent existente mantido (auditar payload);
- HUD: barra de XP existente lê curva nova (auditar binding);
- EditMode tests: curva (valores de referência da tabela do doc: N=1→60, N=10→1897,
  N=50→21213...), cap, pontos por nível par (50 no total), migração, idempotência de grant.
```

## Fora de escopo / Não duplicação

```text
Não criar segundo level service — refatorar o existente p/ consumir ProgressionCurve.
Rebalance de XP por criatura = F33 (fichas).
```

## Critérios de aceite

```text
CA-1: curva bate com a tabela de referência do BALANCE_CURVES (testes com 6 pontos).
CA-2: nível 100 = cap; 99→100 concede o 50º ponto; nenhum ponto duplicado em load.
CA-3: migração: save antigo carrega com nível equivalente (XP preservado).
CA-4: 4 fontes de XP funcionais com hooks nomeados.
```

# /speckit.plan

```text
Arquitetura: Player/ProgressionCurve.cs (puro) + PlayerLevelService (refactor consumo) +
migração no save de player + hooks de fonte + Tests/EditMode/Player/ProgressionTests.cs.
Save: migração (version bump na seção player). Eventos: existentes.
UI: barra lê curva. Play Mode final: YES (lote). SOLO: toca save+XP global.
Risco: nível derivado mudar builds salvos → migração preserva XP total (nível pode
ajustar ±1 — documentado como esperado).
Rollback: curva antiga por flag até validação.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar level service/fontes/save (lista completa).
- [ ] T002 — ProgressionCurve + testes de referência.
- [ ] T003 — Refactor service (cap/pontos pares/idempotência) + testes.
- [ ] T004 — Migração de save + testes legado.
- [ ] T005 — Fontes de XP (4 hooks); HUD binding; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (núcleo) | EditMode: YES | PlayMode/human: YES (lote) | Regression: YES (migração crítica)
- Human timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum evidence: testes (curva/migração/idempotência) + cenário humano subindo 1 nível

## DoD / Anti-regressão

```text
Progressão canônica (cap 100, 60×N^1.5, 50 pontos) como fonte única; migração segura;
fontes nomeadas. Builds 0E; report.
```

> **Nota:** o invariante "50 pontos" acima é **superado** pela EMENDA 2026-06-13-V3 (decisão 1.9):
> passa a "50 base + 1 ponto por ato de main quest (~55)". Ler a emenda antes de tocar a economia
> de pontos.

---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md, decisão 1.9)

> Esta emenda preserva TODO o conteúdo original acima. Em conflito, esta seção vence (mais nova).
> Decisão aplicada: 1.9 (OVERRIDE, "A") — confirmada pela re-auditoria de código 2026-06-13
> (acréscimo #5: o código já concede 1 ponto a cada 2 níveis via `SkillPointIntervalLevels=2`).

```text
1. INVARIANTE DE PONTOS REDEFINIDO (decisão 1.9 — OVERRIDE):
   - O invariante deixa de ser "exatamente 50 pontos".
   - Passa a ser:
       50 base   = 1 ponto de skill a cada 2 níveis no cap 100 (estrutura canônica preservada);
       + 1 ponto por ATO de main quest concluído (decisão v1 mantida);
       teto real ~= 55 pontos (assumindo ~5 atos de main quest; o número exato segue a
       contagem de atos da QUESTS_MAIN_*; o invariante é a REGRA, não o número fixo).
   - Toda referência no corpo original a "exatamente 50", "= 50 pontos", "orçamento das árvores
     = 50" deve ser lida como "50 base + 1/ato (~55)".
   - O grant por NÍVEL PAR permanece idempotente (não re-concede no load). O grant por ATO de
     main quest também é idempotente por ato (não re-concede no load nem ao reentrar no ato).

2. RECOMPENSA DE ATO = SKILL POINT (decisão 1.9):
   - A recompensa de concluir um ato de main quest passa a ser 1 SKILL POINT — NÃO uma moeda
     alternativa / recurso paralelo. O ato publica no MESMO pool de pontos de skill consumido
     pela skill tree (F29). Não criar segundo pool nem "moeda de progressão" separada.
   - Hook nomeado de fonte de XP/progressão adicionado à lista de fontes (item de Escopo
     "fontes normalizadas"): além de kill/quest/descoberta de caverna/colheita, a CONCLUSÃO DE
     ATO concede +1 skill point (hook nomeado, idempotente por ato). Este é um grant de PONTO,
     não de XP — documentar a distinção no report (XP -> nível -> pontos de nível; ato -> ponto
     direto).

3. REBALANCEAR OS LIMIARES DE TIER PARA O NOVO TETO (decisão 1.9):
   - Os limiares de tier por árvore da F29 eram 5/11/18/26 pontos GASTOS NA ÁRVORE, dimensionados
     para o teto de 50. Com o teto subindo para ~55, os limiares 5/11/18/26 devem ser
     reavaliados para que: (a) o teto total permita abrir os tiers altos de pelo menos uma árvore
     de especialização sem inviabilizar as demais; (b) a soma dos limiares de Tier 4/5 das 5
     árvores continue exigindo escolha (não dá para maximizar tudo).
   - Esta spec (F42) é a FONTE do teto; o REBALANCE dos limiares 5/11/18/26 é APLICADO na F29
     (que possui os limiares como gating). Esta emenda DECLARA o novo teto e a obrigação de
     rebalance; a F29 (emenda 2026-06-13-V3, itens 1 e 6) consome esse teto ao recalibrar os
     limiares. Coordenação: F42 define teto -> F29 ajusta gating. Não duplicar os limiares aqui.
   - O número final dos limiares rebalanceados deve constar do execution report da F29 com a
     justificativa de escolha (matriz teto x limiares). F42 documenta apenas o teto (~55) e a
     contagem de atos usada.
```

### Critérios de aceite atualizados por esta emenda

```text
CA-2 (atualizado): nível 100 = cap; 99->100 concede o 50º ponto POR NÍVEL; o pool total chega a
       ~55 com os atos de main quest; nenhum ponto duplicado em load (níveis E atos idempotentes).
CA-5 (novo): concluir um ato de main quest concede +1 skill point ao MESMO pool (não moeda
       alternativa); reentrar no ato ou recarregar o save não re-concede (idempotência por ato).
       EditMode test obrigatório (grant por ato idempotente).
CA-6 (novo): o invariante testado é a REGRA "50 base + 1/ato", não o literal 50; o teste de
       pontos verifica 1 ponto a cada 2 níveis (=50 no cap) E 1 ponto por ato simulado.
```

### Impacto em save/load (atualização)

```text
A seção player passa a persistir (ou derivar de forma idempotente) os atos de main quest já
recompensados com ponto, para garantir não-reconcessão no load. Preferir DERIVAR do estado de
quest já persistido (idempotência por ato) a duplicar um contador — se um contador for
necessário, é int simples (regra save-dto-simple-types-only respeitada; sem refs Unity).
Migração: saves antigos sem o conceito "ponto por ato" não perdem nada — o nível continua
derivado de XP total; os pontos por ato são (re)concedidos uma vez de forma idempotente conforme
o estado de atos já concluídos.
```

### Dependência / coordenação adicionada

```text
F42 (esta spec) DEFINE o teto (~55) e a regra "50 base + 1/ato".
F29 CONSUME o teto para rebalancear os limiares de tier 5/11/18/26 (emenda F29 2026-06-13-V3).
Ordem: F42 antes de F29 (F42 já é "Blocks: F29" no cabeçalho original — coerente).
```
