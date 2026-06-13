# FABLE 00C — Plano Mestre de Execução (Revisão + Ordem + Paralelismo)

> **Tipo:** plano de execução do lote FABLE — NÃO é spec executável.
> **Data:** 2026-06-12
> **Insumos:** catálogos canonizados (BESTIARY/ITEM/QUEST/BALANCE/SKILL_MOVEMENT/HUD_SCENES),
> FABLE_DECISOES_RESPOSTAS, SYSTEMS_DEEPENING v1.1 (Parte D: correções), auditoria 00B.
> **Função:** (A) veredicto de revisão por spec existente; (B) fila global ordenada F01-F42;
> (C) grupos de paralelismo e locks; (D) protocolo de execução.

---

# PARTE A — Revisão das specs existentes (F01-F20)

Critério: a spec ainda faz sentido frente aos catálogos canonizados? Veredictos:

| Spec | Veredicto | Emenda necessária |
|---|---|---|
| F01 status effects | ✅ VÁLIDA | EMENDA: conjunto canônico tem 15 status (Slow≠Chill; +HeatStress/ColdStress; Hunger/Fatigue são sistemas, não ticker) |
| F02 weapon actions+derived | ✅ VÁLIDA | EMENDA: crit normal por CHANCE existe (canon) + janelas garantem; multiplicadores canônicos (heavy ×1.45/posture ×1.60; charged ×1.65-1.90/×1.80-2.20); custos por arma da matriz canônica (sword light 25 STA) |
| F03 equipment baselines | ✅ VÁLIDA | EMENDA: usar os 20 campos canônicos de WeaponDataSO + matriz/materiais/charged effects de EQUIPMENT_MECHANICAL_BASELINES (não inventar tabela própria) |
| F04 threat/pack | ✅ VÁLIDA como está | — (Moves novos vão para F24) |
| F05 boss phases | ✅ VÁLIDA | EMENDA: fases concretas POR BOSS vêm do BESTIARY_CATALOG (Mite Queen→Draconic Elder) |
| F06 loot+vulnerability | ✅ VÁLIDA | EMENDA: famílias = as 9 do bestiário (Insect/Plant/Humanoid/Beast/Undead/Construct/Elemental/Dragon/Aberration); itens de drop da lista do ITEM_CATALOG §11; crit por chance alinhado |
| F07 magic learning | ✅ VÁLIDA | — |
| F08 spell shapes | ✅ VÁLIDA | — |
| F09 biome layout | ✅ VÁLIDA | EMENDA: tamanhos 42×42/55×55/65×65 determinísticos + densidade re-escalada por área + 1 lago/5 níveis nas bandas aquáticas + solo plantável (shadowroot) |
| F10 main quest Ato 1 | ✅ VÁLIDA | — (já alinhada ao QUEST_CATALOG) |
| F11 interiores/schedule | ✅ VÁLIDA | — (executar APÓS F19) |
| F12 farm animals | ✅ VÁLIDA | EMENDA: produtos com variantes de qualidade (_silver/_gold) do ITEM_CATALOG |
| F13 save debts | ✅ VÁLIDA | — |
| F14 UI canvas | ✅ VÁLIDA | EMENDA: painel ÚNICO de 8 abas (Tab cicla) + âncoras % responsivas + abas no quest log (decisões Q11.2/Q6.5) |
| F15 órfãos farm/clima | ✅ VÁLIDA (P0) | — |
| F16 fadiga/sono | ✅ VÁLIDA (P0) | — |
| F17 Fonte física | ✅ VÁLIDA (P0) | — |
| F18 vitals derivados | ✅ VÁLIDA | — |
| F19 schedule dedup+serviços | ✅ VÁLIDA | — |
| F20 calendar UI | ✅ VÁLIDA | EMENDA: widget mostra janela de visibilidade da lua (8-10 dias/estação, decisão Q5.1) |

**Conclusão da revisão:** nenhuma spec é descartada; 9 recebem emenda curta (seção
"EMENDA 2026-06-12" aplicada diretamente no arquivo).

---

# PARTE B — Specs NOVAS (F21-F42): os itens faltantes

Derivadas dos catálogos + decisões. Formato compacto (catálogos são o conteúdo; as specs
são o contrato de execução).

| # | Spec | Cobre | Depende de |
|---|---|---|---|
| F21 | bestiary_knowledge_runtime | tiers K0-K4 + contadores + save + fontes externas (Thalindra/livros) | F06 |
| F22 | essence_tempering_forge | Têmpera de Essência no Brumdar (2 tiers, 1 elemento/arma, ItemInstance.infusion) | F03, F06 |
| F23 | accessories_relics_runtime | 3 slots (Ring/Amulet/Charm) + 12 acessórios + 4 relíquias + gerador | F18 |
| F24 | enemy_moves_elite_affixes | 12 Moves oficiais faltantes no enum + 6 afixos determinísticos de elite | F04 |
| F25 | npc_unique_services | serviços únicos numéricos dos 23 NPCs (bênção, seguro, treino, refeição...) | F19 |
| F26 | friendship_state_contract | FriendshipState CONTRACT_ONLY (save section, sem UI/recompensa) | — |
| F27 | perfect_block_runtime | Perfect Block (janela 0.15s) + integração Contra-Ataque/relic Kanthor | F02 |
| F28 | dialogue_conditions_pools | DialogueNode.Requirement (flag/hora/lua/quest) + pools contextuais + rumor útil | — |
| F29 | canonical_skill_catalog_migration | substituir DefaultSkillCatalog pelos ~70 nodes canônicos + crosswalk + refund | F02, F08 |
| F30 | catalog_validator | validator de IDs/categorias/BV/órfãos (editor) | — |
| F31 | magic_items_runtime | wands (cargas) + cast/learn scrolls + tomes + focuses | F07, F08 |
| F32 | item_catalog_data_expansion | gerar ~118 itens + variantes de qualidade nos initializers/lojas | F30 |
| F33 | bestiary_data_expansion | roster 44→60 no gerador + renames Veilkin/Gravedelver + stats do catálogo | F24, F06 |
| F34 | quest_sources_infrastructure | Quadro de Avisos (3 dailies) + contratos Zrix + givers secretos da caverna | — |
| F35 | npc_side_quest_chains | 12 cadeias × 3 (36 side quests) com gates de serviço | F34, F25 |
| F36 | main_quest_acts_2_4 | 15 quests dos Atos 2-4 + NPCs Vaelrion/nymiriano + bridges de fragmento | F10, F33 |
| F37 | festivals_lunar_events | 8 festivais com datas + luas 8-10 dias/estação com pico + eventos aleatórios | F15 |
| F38 | minimap_v1 | minimapa 180px (fog of war caverna, planta town/farm) | F14 |
| F39 | inferred_player_class | título dinâmico + diálogos + bônus +3% + serviços por classe (3 recompensas) | F29 |
| F40 | town_48x42_relayout | TownScene 48×42 (anel viário, praça 12×12, distritos) | F11 |
| F41 | farm_lot_expansions | 3 lotes compráveis (alvará Tovin) com cercas que abrem | F25 |
| F42 | progression_cap100_xp | cap 100 + curva 60×N^1.5 + XP por kill/quest/descoberta + pontos bônus | — |

---

# PARTE C — Fila global ordenada e paralelismo

## Batches (ordem de execução; ◭ = pode rodar em paralelo com o batch anterior se agentes múltiplos)

```text
BATCH 0 — CORRETIVAS P0 (sequencial; destravam tudo):
  F15 → F16 → F17
BATCH 1 — FUNDAÇÃO SOLO (cada uma SEM paralelo algum):
  F13 (save debts) → F42 (cap 100/XP)
BATCH 2 — COMBATE NÚCLEO (sequencial; lock: caminho de ataque):
  F01 → F02 → F03 → F18 → F27
BATCH 3 — INIMIGOS (sequencial; lock: EnemyBrain/materializer) ◭ com BATCH 2 até F02:
  F04 → F24 → F05
BATCH 4 — MAGIA (sequencial; lock: SpellCastService) ◭ com BATCH 3:
  F07 → F08 → F31
BATCH 5 — DADOS/ECONOMIA (lock: geradores/ItemDatabase):
  F30 → F32 → F06 → F33 → F22 → F23
BATCH 6 — MUNDO (lock: geradores de cena; F19 antes de F11 é obrigatório):
  F19 → F11 → F09 → F40 → F41 → F12 → F37   ◭ F37 com F12
BATCH 7 — PROGRESSÃO/CONTEÚDO (lock: QuestRegistry/SkillCatalog):
  F29 → F39 → F25 → F28 → F26 → F34 → F35 → F10 → F36
BATCH 8 — UI (lock: Canvas/builder):
  F14 → F20 → F38
```

## Matriz de paralelismo entre batches (para execução multi-agente futura)

```text
PODEM rodar simultaneamente (locks disjuntos):
  BATCH 2 (Combat/*) ∥ BATCH 6 (geradores de cena) ∥ BATCH 8 (UI/*)
  BATCH 3 (Enemy/Cave runtime) ∥ BATCH 4 (Magic) ∥ BATCH 8
  BATCH 5 ∥ BATCH 8 (desde que F23 espere F18)
NUNCA em paralelo com nada:
  F13, F42 (save/progressão tocam schema e managers transversais)
  F29 (troca o catálogo de skills inteiro)
REGRAS DE TRAVA (sempre):
  EnemyBrain: F02→F04→F24→F05 em ordem | SpellCastService: F07→F08→F31
  Geradores de cena: F11→F09→F40→F41 em ordem | QuestRegistry: F34→F35→F10→F36
  Save schema: F13, F07, F12, F21, F26, F42 — UM por vez, nunca simultâneos.
EXECUÇÃO SOLO (este plano, um agente): seguir a ordem dos batches 0→8 linearmente,
intercalando ◭ apenas se conveniente; a ordem linear completa é:
  F15 F16 F17 | F13 F42 | F01 F02 F03 F18 F27 | F04 F24 F05 | F07 F08 F31 |
  F30 F32 F06 F33 F22 F23 | F19 F11 F09 F40 F41 F12 F37 |
  F29 F39 F25 F28 F26 F34 F35 F10 F36 | F14 F20 F38
  (42 specs)
```

---

# PARTE D — Protocolo de execução (por spec)

```text
1. Ler a spec + emendas + catálogo(s) citados.
2. Fase 0 de auditoria DE INSTANCIAÇÃO (lição da 00B — quem cria? quem chama?).
3. Implementar dentro dos arquivos permitidos; testes EditMode onde exigido.
4. Adicionar novos .cs aos csproj (Compile includes).
5. run_strict_validation.ps1 — exit 0 obrigatório (docs zero erros mantido).
6. Execution report individual em docs/validation/ (template strict, com
   validated_adrs/validated_game_rules e Testing Quality Gate).
7. Atualizar CURRENT_STATE (1 linha por batch concluído, não por spec).
8. Status máximo: BUILD_VALIDATED (Play Mode humano deferido ao final do lote).
9. Commit por spec (mensagem em português) — SOMENTE se autorizado pelo humano.
10. Parar e reportar se: spec conflitar com catálogo, arquivo proibido for necessário,
    ou validação falhar sem caminho documentado.
```

## Marcos de checagem humana (sugeridos, não bloqueantes)

```text
M1 após BATCH 2: combate novo testável em Play Mode (pesos, stagger, vitals).
M2 após BATCH 5: economia/têmpera/acessórios + bestiário 60 no jogo.
M3 após BATCH 7: questline completa dos 4 atos jogável.
M4 após BATCH 8: UI final + minimapa → rodada de Play Mode geral do lote.
```


---

# PARTE E — ENUMERAÇÃO DE EXECUÇÃO E01-E47 (REVISÃO 2 — VINCULANTE, supersede a ordem da PARTE C)

> Atualização 2026-06-12: (a) F21 entrou na ordem (faltava na linear da PARTE C);
> (b) F26 movida para ANTES de F25/F28 (dependência de amizade); (c) F31 movida para
> DEPOIS de F06 (loot resolver); (d) 5 specs novas F43-F47 inseridas; (e) E01-E10 já
> executadas (BUILD_VALIDATED, reports em docs/validation/fable_XX_*_execution_report.md).

## E.1 Ordem linear enumerada

| # | Spec | Status | Depende de | Lock principal |
|---|------|--------|-----------|----------------|
| E01 | fable_15 clima/farm wiring | ✅ BUILD_VALIDATED | — | FarmPlot/FarmScene/DayStarted |
| E02 | fable_16 fadiga/sono/colapso | ✅ BUILD_VALIDATED | — | Player/Conditions, FarmScene |
| E03 | fable_17 Fonte física | ✅ BUILD_VALIDATED | — | Fonte/**, FarmScene |
| E04 | fable_13 save debt closure | ✅ BUILD_VALIDATED | — | SAVE SCHEMA (solo) |
| E05 | fable_42 progressão cap 100 | ✅ BUILD_VALIDATED | E04 | XP/level (solo) |
| E06 | fable_01 status canônicos | ✅ BUILD_VALIDATED | — | StatusEffect/** |
| E07 | fable_02 combat actions | ✅ BUILD_VALIDATED | E06 | PlayerAttackController |
| E08 | fable_03 equipment baselines | ✅ BUILD_VALIDATED | E07 | WeaponDataSO/dano recebido |
| E09 | fable_18 vitals derivados | ✅ BUILD_VALIDATED | E07 | managers de vitals |
| E10 | fable_27 perfect block | ✅ BUILD_VALIDATED | E07 | block/receiver |
| E11 | fable_47 derived stats follow-ups | A implementar | E06,E09,E02 | PlayerController/movement |
| E12 | fable_04 enemy actions/telegraphs | A implementar | E07 | EnemyBrain (cadeia) |
| E13 | fable_24 12 Moves + elites | A implementar | E12 | EnemyBrain (cadeia) |
| E14 | fable_05 boss phase AI | A implementar | E13 | EnemyBrain (cadeia) |
| E15 | fable_07 mana/spell save | A implementar | E07 | SAVE SCHEMA (solo) |
| E16 | fable_08 spell shapes | A implementar | E15 | SpellCastService |
| E17 | fable_30 catalog validator | A implementar | — | Editor/Validation (editor-only) |
| E18 | fable_32 item catalog (~118) | A implementar | E17,E16 | geradores de item |
| E19 | fable_06 loot/vulnerability | A implementar | E06,E18 | EnemyDataSO/loot |
| E20 | fable_31 itens mágicos | A implementar | E16,E19 | inventário/baús |
| E21 | fable_33 bestiário 64 fichas | A implementar | E13,E19,E18,E17 | roster/spawn tables |
| E22 | fable_22 têmpera de essência | A implementar | E08,E19,E18 | equipment save (aditivo) |
| E23 | fable_23 acessórios/relíquias | A implementar | E09,E18 | EquipmentSlot/Manager |
| E24 | fable_21 bestiary knowledge | A implementar | E19,E21 | SAVE SCHEMA (solo) |
| E25 | fable_19 city services/schedule | A implementar | — | City/Schedule |
| E26 | fable_11 town interiors | A implementar | — | gerador Town (cadeia cena) |
| E27 | fable_09 cave biome/layout 42/55/65 | A implementar | — | gerador Cave (cadeia cena) |
| E28 | fable_44 cave save multi-nível | A implementar | E04,E27 | SAVE SCHEMA (solo) |
| E29 | fable_40 town 48×42 | A implementar | E26 | gerador Town (cadeia cena) |
| E30 | fable_41 lotes da fazenda | A implementar | E29,E18 | gerador Farm (cadeia cena) |
| E31 | fable_12 animais da fazenda | A implementar | E30 | SAVE SCHEMA (solo) |
| E32 | fable_37 festivais/picos lunares | A implementar | E01,E18,E29 | WorldEventService/praça |
| E33 | fable_29 catálogo de skills (~70) | A implementar | E07,E10,E05,E06 | SOLO TOTAL (skills+save) |
| E34 | fable_39 classe inferida | A implementar | E33 | leitura SkillTree |
| E35 | fable_26 amizade | A implementar | E04 | SAVE SCHEMA (solo) |
| E36 | fable_25 serviços únicos NPC | A implementar | E24,E35,E02 | NpcShopController/diálogo |
| E37 | fable_28 diálogo condicional | A implementar | E35,E01 | TownNpcDialogueLibrary |
| E38 | fable_34 fontes de quest | A implementar | E35,E18 | QuestRegistry (cadeia) |
| E39 | fable_35 12 cadeias side quest | A implementar | E38,E35,E18 | QuestRegistry (cadeia) |
| E40 | fable_10 quest fragmento/objetivos | A implementar | E39 | QuestRegistry (cadeia) |
| E41 | fable_36 main quest atos 2-4 | A implementar | E38,E39,E40,E21 | QuestRegistry (cadeia) |
| E42 | fable_46 romance foundation | A implementar | E35,E39,E37,E41 | NPC/diálogo/save amizade |
| E43 | fable_43 endgame Ato 5 + finais | A implementar | E41,E14,E21,E03,E38 | quest+cave endgame |
| E44 | fable_14 UI canvas 4 telas | A implementar | — | UI canvas (cadeia UI) |
| E45 | fable_20 relógio/calendário HUD | A implementar | E44,E01 | UI canvas (cadeia UI) |
| E46 | fable_38 minimapa v1 | A implementar | E44,E45,E27 | UI canvas (cadeia UI) |
| E47 | fable_45 bestiário codex UI | A implementar | E44,E24,E21 | UI canvas (cadeia UI) |

## E.2 Grupos paralelos (tracks que podem rodar SIMULTANEAMENTE)

Regras: dentro de cada track a ordem é sequencial; tracks distintos podem rodar em
paralelo (agentes separados) porque não compartilham locks. Specs marcadas SAVE SCHEMA
nunca rodam simultâneas entre si (E04,E15,E24,E28,E31,E35 — uma por vez no projeto).
E33 (skills) é SOLO TOTAL: nada roda junto.

```text
JANELA P1 (após E10):
  Track INIMIGOS:  E12 → E13 → E14
  Track MAGIA:     E15 → E16            (E15 é save-lock: iniciar quando nenhum outro save spec ativo)
  Track TOOLING:   E17                   (editor-only — paralelo com qualquer)
  Track PLAYER:    E11                   (movement locks — paralelo com INIMIGOS/MAGIA/TOOLING)

JANELA P2 (após E16+E17):
  Track DADOS:     E18 → E19 → E20 → E21 → E22 → E23 → E24
  Track CIDADE:    E25 → E26 → E27 → E28 → E29 → E30 → E31 → E32
  (DADOS ∥ CIDADE: locks distintos; exceção: E24/E28/E31 são save-lock —
   coordenar para nunca 2 save specs ao mesmo tempo; E30 precisa de E18)

JANELA P3 (após P2):
  E33 (SOLO TOTAL — nada em paralelo)

JANELA P4 (após E33):
  Track SOCIAL:    E35 → E36 → E37      (E34 ∥ SOCIAL após E33)
  Track QUESTS:    E38 → E39 → E40 → E41 → E42 → E43
  (SOCIAL precede parcialmente QUESTS: E38 precisa de E35 — iniciar QUESTS após E35)

JANELA P5 (após E43):
  Track UI:        E44 → E45 → E46 → E47 (sequencial — canvas lock)
```

## E.3 Como executar (quando autorizado)

```text
Para cada Exx: usar /execute-spec-strict com a spec correspondente.
Gate por spec: run_strict_validation.ps1 exit 0 + report individual (modelo: reports E01-E10).
Batch máximo: 10 specs por loop (regra spec_quality_gate). Save specs: 1 por vez.
Checkpoints humanos: M1 já atingido (pós-E10 — pendências Unity listadas no CURRENT_STATE);
M2 = pós-E24 (dados+combate completos); M3 = pós-E43 (conteúdo completo); M4 = pós-E47 (UI).
```

---

# PARTE F — EXTENSÃO E48-E60 + RETRO-SPECS (auditoria de completude/reconstrutibilidade 2026-06-12)

Três auditorias (código↔specs; design↔specs combate/núcleo; design↔specs mundo/social/UI)
encontraram gaps. Resultado: 13 specs funcionais novas (F48-F60), 9 retro-specs
(spec_retro_01..09 em docs/specs/implementados/ — documentam código que existia SEM spec:
slice 2026-06-12 e WAVE_INTEGRATION 17-26), 2 emendas (F21: +skill points por marco de
bestiário; F34: +QuestSource.CaveContract) e a migração da fundação (~66 specs de
implementados/) para .specs/00_executadas/fundacao/.

## F.1 Novas specs enumeradas (encaixe nas janelas da PARTE E)

| # | Spec | Janela | Depende de | Lock principal |
|---|------|--------|-----------|----------------|
| E48 | fable_48 munição de arco + flechas elementais | P2 (DADOS, após E19) | E08✅,E18,E19 | BowArrowAttackService/ammo |
| E49 | fable_49 craft tier alto + upgrades | P2 (DADOS, após E22) | E18,E08✅,E21,E22 | crafting/ItemInstance |
| E50 | fable_50 pesca v2 (lagos+tabelas) | P2-fim (após E21 e E27) | E27,E21,E18,E01✅ | FishingSpot/geração |
| E51 | fable_51 contratos do Zrix (cc_*) | P4 (QUESTS, após E38) | E38,E27,E14 | QuestRegistry (cadeia) |
| E52 | fable_52 secretas scq_* + goblin visitante | P4 (após E41) | E38,E21,E41,E32 | QuestRegistry (cadeia) |
| E53 | fable_53 quests de festival (fq_*) | P4 (após E38 e E32) | E32,E38,E35,E18 | QuestRegistry (cadeia) |
| E54 | fable_54 forrageio + shipping noturno | P2 (CIDADE-fim) | E01✅,E04✅,E18 | FarmScene/save aditivo |
| E55 | fable_55 processamento + estufa | P2 (CIDADE-fim, após E31) | E31,E18,E30 | FarmScene/crafting |
| E56 | fable_56 aba Sistema + título/new game | P5 (após E44) | E44 | UI canvas (cadeia UI) |
| E57 | fable_57 cidade viva (aniversários/estalagem/ADR reputação) | P4 (SOCIAL-fim) | E35,E32,E02✅,E26 | NPC/calendário |
| E58 | fable_58 áudio/SFX hooks | LIVRE (qualquer janela) | E07✅,E10✅ | Audio/** (lock próprio) |
| E59 | fable_59 telemetria de combate | LIVRE (após E13,E21) | E07✅,E13,E21 | Telemetry/** (leitura) |
| E60 | fable_60 armadilhas por bioma/tier | P2 (CIDADE, após E27) | E27,E21,E06✅ | gerador Cave (cadeia cena) |

## F.2 Retro-specs (já documentadas — não entram na fila de execução)

R01..R09 = spec_retro_01..09 em docs/specs/implementados/ (espelho em
.specs/00_executadas/retro/). Cobrem: projéteis procedurais, EnemyBrain behaviors +
densidade, mercador errante, catálogo real de skill effects, cenas v2 + diálogo 23×13,
WI-23 HUD canvas, WI-24 daily goals, WI-25 schedules, WI-17..22+26 hardening/questlines.
Débito remanescente delas: testes de caracterização (listados em cada retro-spec).

## F.3 Placar oficial

```text
Specs funcionais:      60 (F01-F60) — 10 executadas (E01-E10) + 50 a executar (E11-E60)
Retro-specs:           9 (documentam código já implementado)
Fundação migrada:      66 specs em .specs/00_executadas/fundacao/
Legado waves:          94 specs em .specs/00_executadas/legado/
```

---

# PARTE G — EXTENSÃO E61-E67 + REFINAMENTO V2 (auditorias de MVP/shipping 2026-06-12)

Três auditorias finais (pendências declaradas; jornada do jogador ponta-a-ponta;
não-funcionais de shipping) acharam: build standalone inexistente (ZERO cenas no
EditorBuildSettings), onboarding ausente, intro do New Game ausente, e ~30 decisões
humanas em aberto → `docs/design/FABLE_REFINAMENTO_V2_PERGUNTAS.md` (aguardando respostas;
blocos 1-4 e 7 travam F36/F43/F46/F56/F63 e o corte do MVP).

## G.1 Novas specs enumeradas

| # | Spec | Janela | Depende de | Observação |
|---|------|--------|-----------|------------|
| E61 | fable_61 build standalone Windows | P5-fim (pode antecipar c/ FarmScene como entry) | E56; Refin. 7.1-7.3 | BLOQUEADOR de shipping |
| E62 | fable_62 onboarding/control hints | P5 (após E56) | E56, E67 | BLOQUEIA MVP jogável |
| E63 | fable_63 intro New Game + carta→Corvus | P5 (após E56) | E40, E38, E56; Refin. Bloco 3 (texto) | PLACEHOLDER_LORE até refinamento |
| E64 | fable_64 tela de morte Canvas + corpse msg | P5 (após E44) | E44 | |
| E65 | fable_65 daily goals reward + HUD | LIVRE (após E18) | WI-24, E05✅, E18 | fecha DAILY_GOAL_*_DEFERRED |
| E66 | fable_66 limpeza de débitos de código | P2-fim | E18; Refin. Bloco 2 (kit inicial) | slice mode só sai com kit canônico |
| E67 | fable_67 governança (input map + ADR densidade + re-mapa refs) | LIVRE (PODE RODAR JÁ) | — | docs-only |

## G.2 Emendas desta rodada

- F34 (EMENDA-C): fechar PREREQUISITE_UI_DEBT (WI-26) como critério de aceite.
- F20 (EMENDA-C): widget Nível/XP + toast de level up (PlayerLevelChangedEvent sem consumidor de UI real).

## G.3 Placar oficial

```text
Specs funcionais:  67 (F01-F67) — 10 executadas (E01-E10) + 57 a executar (E11-E67)
Retro-specs:       9 | Fundação: 66 | Legado: 94
Refinamento v2:    ~30 perguntas AGUARDANDO (docs/design/FABLE_REFINAMENTO_V2_PERGUNTAS.md)
Caminho crítico MVP (proposta 7.6-A do refinamento): E01-E10✅ → P1 (E11-E17,E58) →
  E44 (telas) → E56 (título/save) → E61 (build) → E62 (onboarding) → Play Mode humano
```

---

# PARTE H — Rodada Refinamento v2 RESPONDIDO (2026-06-12): E68-E70 + decisões vinculantes

> Respostas integrais do dono em `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md`
> (+ APÊNDICE LORE A.1-A.5). 18 emendas EMENDA-D aplicadas (F08/F11/F12/F14/F20/F21/F37/
> F41/F43/F45/F46/F49/F50/F56/F61/F63/F66 + apêndice). 3 specs novas:

## H.1 Novas specs enumeradas

| Exx | Spec | Janela | Depende de | Observação |
|---|------|--------|-----------|------------|
| E68 | fable_68 Marcas dos Deuses (11 altares com bônus diário) | P2-fim ou P4 (após E22 fable_22 + E23 fable_23) | E22, E23, E06✅, E25, E32 | catálogo vinculante no apêndice A.5; cave-stable-run |
| E69 | fable_69 sprint em combate (3.8-4.2 a 8 STA/s) | P1+ (logo após E11 fable_47) | E11 (composer), E02✅ | decisão 6.5-A; micro-spec |
| E70 | fable_70 2ª leva side quests (11 NPCs, ~33 quests) | P4 (após E39 fable_35) | E39, E38, E35, E18 | decisão 8.3 "Já gere"; Vaelrion gateado Ato 2 |

## H.2 Decisões v2 que alteram o plano

- **7.6 "Faça tudo": MVP = LOTE INTEIRO (E01-E70).** O caminho crítico da PARTE G vira
  apenas ordem interna — não há corte curto; V1_SCOPE = esta enumeração.
- 7.4 A+B: 3 slots manuais + backup rolling a cada save (E56 Fase 0 RESOLVIDA).
- 5.2-B: Fonte começa Dormant (ajuste sobre E03 executada — beat na E63).
- 2.1/2.2: kit canônico + baú narrativo (E66 destravada; E63 emendada).
- 4.5-B: nível 101 = CaveScene especial (E43 emendada).
- Lore consolidada (apêndice A.1-A.5) destrava os textos de E41/E43/E63 (sai PLACEHOLDER_LORE).

## H.3 Placar oficial (substitui G.3)

```text
Specs funcionais:  70 (F01-F70) — 10 executadas (E01-E10) + 60 a executar (E11-E70)
Retro-specs:       9 | Fundação: 66 | Legado: 94
Refinamento v2:    RESPONDIDO E REGISTRADO (FABLE_DECISOES_RESPOSTAS_v2.0.md, vinculante)
MVP = lote inteiro (decisão 7.6). Execução aguarda autorização humana; checkpoint M1
(Unity: FarmScene, status effects, baselines, Test Runner) segue PENDENTE.
```

---

# PARTE I — Refinamento v3 RESPONDIDO (2026-06-13): fable_71 + emendas + ADRs + re-auditoria de código

> Respostas integrais do dono em `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (blocos 1-4:
> skills, itens, companions, boas práticas). Re-auditoria de código (`reaudit-code-v3`)
> confirmou os achados contra o working tree melhorado. Geração: 10 artefatos, validate_docs exit 0.

## I.1 Nova spec enumerada

| Exx | Spec | Janela | Depende de | Observação |
|---|------|--------|-----------|------------|
| E71 | fable_71 combat feel pass (hit-stop + screen shake + HudSuppressionChangedEvent) | LIVRE (como F58) | eventos de dano (existentes) | decisão 4.2; números de dano JÁ existem (não recriar) — só hit-stop/shake/supressão |

## I.2 Reescritas canônicas e ADRs (governança)

- **ADR-0010** reconcilia `skill_tree_rules.md` e `inventory_equipment_rules.md` com o código FABLE
  (cap 100, 4 slots teclas 1-4, cooldown tempo real, respec grátis+250g via Fonte, 5 árvores,
  30 slots de inventário, 3 slots tipo-acessório). Era a stop condition latente das specs de skill/inventário.
- **ADR-0011** arte 32 px/tile · **ADR-0012** localização id→string desde a P4 · **ADR-0013**
  input teclado/mouse only no v1 · **ADR-0014** dificuldade única no v1.
- **Adendo numérico de skills**: `docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md`
  (dano/cooldown/custo por rank + cadeias de pré-requisito dos 70 nós + triggers de capstone) — contrato da F29.
- **Matriz de presentes**: `docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md` (gostos loved/liked/neutral/disliked/hated por NPC).
- **Catálogo de papéis de companion**: `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` (bônus + ações por papel).

## I.3 Emendas EMENDA-2026-06-13-V3

F29 (rank cap dinâmico, hooks nomeados, prereqs, respec punitivo+revalidação, aposentar slots R/T/Y/G,
decisão dos 5 modificadores mortos) · F42 (50 base + 1/ato ~55; tiers rebalanceados) · F32/F03 (Gold ×2.0,
6 essências, enums save-safe Hammer/Wand/Tool + Armor/Shield/Accessory/Relic/Essence/AnimalProduct) ·
ITEM_CATALOG (receitas de poção/óleo, mágicos F31, gear tier alto, drops órfãos, água-como-item, ~10 peixes,
durabilidade/upgrade) · F26 (gosto por NPC, não +3 fixo) · COMPANIONS_DIRECTION (stances 12/4 tiles, comandos,
equip arma+acessório base, revive 30%, permadeath narrativo + Fonte) + 4 specs 14_* spec-ready ·
F14 (relógio pausa em modal) · F56 (save corrompido→backup) · F58 (MusicState) · F43 (números já existem; supressão via fable_71).

## I.4 Placar oficial (substitui H.3)

```text
Specs funcionais:  71 (F01-F71) — 10 executadas (E01-E10) + 61 a executar (E11-E71)
Retro-specs:       9 | Fundação: 66 | Legado: 94 | ADRs: 14 | game_rules reconciliados: 2
Refinamentos:      v1/v2/v3 RESPONDIDOS e vinculantes. MVP = LOTE INTEIRO (decisão 7.6).
Execução aguarda autorização humana; checkpoint M1 (Unity) segue PENDENTE.
```

---

# PARTE J — Cobertura de design v3 (2026-06-13): fable_72/73 + companions densas

> Pergunta do dono: "o design novo virou direction como os outros? então gere as specs SpecKit
> completas pra executar depois." Conferido: as 8 game_rules NÃO viram spec (são regras consumidas
> pelas specs; domínios já cobertos). O design de FEATURE sem spec densa virou spec aqui.

## J.1 Novas specs no lote v1

| Exx | Spec | Janela | Depende de | Observação |
|---|------|--------|-----------|------------|
| E72 | fable_72 sistema de presentes por NPC (gostos loved/liked/neutral/disliked/hated) | P4 (junto/após E35 fable_26) | F26 (amizade), NPC_GIFT_TASTE_MATRIX, ITEM_CATALOG gift_* | reusa estado de amizade da F26; estende NpcGiftPreferences |
| E73 | fable_73 localização tabela id→string | P4 (infra, no início) | ADR-0012 | só texto NOVO desde a P4; não retrofita hardcoded anterior |

## J.2 Companions — 4 specs elevadas a SpecKit DENSO (seguem WAVE 14, gated — decisão 3.12)

Em `docs/specs/a_implementar/features_futuras/` (NÃO entram na enumeração Exx do lote v1):
- `14_spec_companion_cave_assist_brain_balance_future_runtime.md` (747 linhas) — brain de combate, stances/comandos, DPS 25/40%, downed/revive 30%, permadeath narrativo.
- `14_spec_companion_eligibility_recruitment_state_save_future_runtime.md` (594) — recrutamento, bond, equip arma+acessório, **CompanionSaveSectionProvider** (round-trip que faltava), Romance/Spouse como flags.
- `14_spec_companion_farm_jobs_board_automation_future_runtime.md` (554) — 9 jobs, board, toggle de fertilizante (3.8).
- `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime.md` (596) — HUD de companion, comandos/stances (tap/hold radial), exibição de equip.

Insumo de design: `COMPANION_ROLES_CATALOG_v1.0.md` (bônus + ações por papel) + EMENDA-V3 na COMPANIONS_DIRECTION.

## J.3 Placar oficial (substitui I.4)

```text
Specs funcionais (lote v1): 73 (F01-F73) — 10 executadas (E01-E10) + 63 a executar (E11-E73)
Companions WAVE 14 (gated, fora do lote v1): 4 specs densas spec-ready
Retro-specs: 9 | Fundação: 66 | Legado: 94 | ADRs: 14 | game_rules: 20
Refinamentos v1/v2/v3 RESPONDIDOS. MVP = LOTE INTEIRO (decisão 7.6).
Execução aguarda autorização; checkpoint M1 (Unity) PENDENTE.
```