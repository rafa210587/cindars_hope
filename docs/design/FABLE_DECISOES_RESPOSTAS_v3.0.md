# FABLE — Decisões e Respostas v3.0 (Skills, Itens, Companions, Boas Práticas)

> **Status:** VINCULANTE — respostas do dono ao `FABLE_REFINAMENTO_V3_PERGUNTAS.md` (2026-06-13).
> Complementa v1.0/v2.0; em conflito, este vence (mais novo).
> **Nota do dono:** *"revejam o código — em outra sessão melhoramos ele."* → re-auditoria de
> código em curso (workflow `reaudit-code-v3`). O **PLANO DE EMENDAS** no fim deste doc será
> ajustado ao estado real do working tree antes de qualquer aplicação. Achados das auditorias
> de design ficam em `FABLE_REFINAMENTO_V3_PERGUNTAS.md` (diagnóstico) — aqui só as decisões.
>
> Legenda: ★ = aceitou o default · **(OVERRIDE)** = escolheu o não-default · **(CUSTOM)** =
> resposta livre que vai além das opções.

---

## BLOCO 1 — SKILLS (trava F29/F33; calibra F02/F27)

| # | Decisão |
|---|---------|
| 1.1 | **B ★** — Rank cap **dinâmico** por profundidade da árvore (cap de TODAS sobe com o tier desbloqueado: T1 aberto→cap 2 … T4/5→cap 5). |
| 1.2 | **A ★** — 1 ponto de skill por rank, inclusive os 3 ranks de capstone. |
| 1.3 | **B (OVERRIDE)** — Pré-requisitos por nó **SIM**, além do tier gating. → autorar cadeias de pré-requisito para os 70 nós (coluna nova na `PLAYER_SKILL_TREES_DIRECTION`). |
| 1.4 | **A ★** — Dano base / cooldown / custo das ativas em **adendo numérico único** (tabela skill × rank) anexado à `SKILL_ACTION_MOVEMENT_TABLE` ANTES da F29 — vira o contrato dos executores. |
| 1.5 | **A ★** — Lista fechada de dormentes (`NotYetExecutable`): executáveis = Corte Amplo, Golpe de Ruptura, Projétil Arcano, Flecha Perfurante, Toque Restaurador; dormentes = Marcador de Presa, Armadilha, Selo de Proteção, Disparo de Interrupção, Disparo Carregado, Descanso Curto. |
| 1.6 | **A ★** — F29 publica hooks **NOMEADOS** (IGoldDropModifier, IHarvestYieldModifier, ICraftCostModifier…) e cada spec consumidora (F06/F17/F31/F48/F49/F55) ganha 1 linha de escopo "consome hook X". Até lá, tooltip declara "efeito pendente". |
| 1.7 | **A ★** — Triggers de capstone fechados agora: sequência de MP = janela 6s; Kanthor cura 3% HP se HP<100% senão +10 Stamina; Telisandra = HP<25% OU Stamina<15% OU "Exausto". |
| 1.8 | **A ★** — Reescrever `skill_tree_rules.md` (cap 100, 1pt/2 níveis, 4 slots, respec via Fonte) **ANTES** da F29 — senão toda spec de skill bate na stop condition de conflito código×game_rule. |
| 1.9 | **A (OVERRIDE)** — **Mantém +1 skill point por ATO de main quest** (decisão v1). Teto real vira ~55 → **F42 será emendada**: invariante deixa de ser "exatamente 50"; passa a "50 base + 1/ato", com os tiers (5/11/18/26) rebalanceados para o novo teto. |
| 1.10 | **A ★** — Marcador de Presa: texto canônico vira "dano do jogador e de aliados invocados, quando existirem" — companions herdam o hook depois, sem retrabalho. |
| 1.11 | **B (OVERRIDE)** — Respec **PUNITIVO**: invalida também o uso de itens de tiers que deixaram de estar desbloqueados. → exige **sistema de revalidação de equipamento** no respec (emenda à F29). Itens permanecem no inventário, mas ficam não-equipáveis/não-usáveis até o tier ser redesbloqueado. |

## BLOCO 2 — ITENS (trava F32 e a cadeia de dados)

| # | Decisão |
|---|---------|
| 2.1 | **A ★** — Catálogo vence: 3 níveis de qualidade, Silver ×1.5 / **Gold ×2.0**; corrigir F32 (×2.2); tabela Q0-Q4 da economia fica para sistemas futuros, não para crops. |
| 2.2 | **A ★** — Emendar `ITEM_CATALOG` com receitas canônicas de poção/óleo (ingredientes de drops/crops existentes), validadas pela fórmula inputs ×1.5-3.0. |
| 2.3 | **A ★** — Itens mágicos da F31 (8 + `unidentified_trinket_N` + `scroll_identify`) entram no catálogo com seção própria (BV/raridade); corrigir a citação "§19" da F31. |
| 2.4 | **A + (CUSTOM)** — `gift_*` definidos no catálogo, **MAS cada NPC reage de forma diferente**: tabela de **GOSTOS por NPC** (loved / liked / neutral / disliked / hated). Loved dá **mais** afinidade; hated **REDUZ** afinidade — específico por NPC. → autorar **matriz NPC × gift**; **F26 deixa de ser "+3 fixo"** e passa a ler o gosto do NPC. |
| 2.5 | **A ★** — 6 essências (catálogo + decisão v1); corrigir o "8" da F32. |
| 2.6 | **A ★** — Lista nominal craftável de gear tier alto (Mithril/Bromeciana/Pedra Negra/Meteórica, ~14 itens, BV pela fórmula da economia). |
| 2.7 | **A ★** — Adicionar enums save-safe: `Hammer`/`Wand`/`Tool` em `WeaponType`; `Armor`/`Shield`/`Accessory`/`Relic`/`Essence`/`AnimalProduct` em `ItemCategory` (valores altos explícitos). *(Verificar na re-auditoria se o código já não os tem — nota do dono.)* |
| 2.8 | **A ★** — ~20 drops órfãos do bestiário entram no catálogo §11 (BV por banda, EN-only, resolvendo "stabilized blackstone" × "pedra_negra_estabilizada") com uso declarado. |
| 2.9 | **B (OVERRIDE)** — Água **vira item** (balde/poço — recurso coletável, não infinito); carne = `grub_meat` (drop existente); `festival_cake` exige **cow_milk** especificamente (não "qualquer leite"). |
| 2.10 | **A ★** — Expandir roster de peixes para ~10 (1-2 por estação no açude + 2-3 de caverna por banda + 1 do Moonless Pool), com BVs fixados. |
| 2.11 | **A ★** — Tabela derivada: DurabilityMax por classe×material (bases 80 arma / 150 armadura × modifiers §16/§26) e custo de upgrade +N = (2N × material da banda) + (BV × 0.5N) ouro. |
| 2.12 | **A ★** — Reescrever `inventory_equipment_rules.md` (slots de equipamento atualizados com Ring/Amulet/Charm; nota do pouch +6) **ANTES** da execução, via `decision-rule-extraction`. |

## BLOCO 3 — COMPANIONS (não trava o lote; destrava as specs 14_*)

> Canônico (COMPANIONS_DIRECTION + código WAVE 05): companion = NPC da cidade com vínculo;
> 1 ativo na caverna; DPS 15-35% (35-50% especialista); jobs de fazenda (9) já implementados.

| # | Decisão |
|---|---------|
| 3.1 | **(CUSTOM)** — Há **mais papéis** do que os 3 propostos. → consolidar TODOS os papéis dos refinamentos/`COMPANIONS_DIRECTION` num **Catálogo de Papéis de Companion**, e para CADA papel definir explicitamente: **bônus** (combate + fora de combate) e **conjunto de ações**. O número de companions iniciais será fixado depois do catálogo. |
| 3.2 | **(CUSTOM)** — **Comandos:** Ficar/Esperar · Seguir · Atacar alvo marcado. **Stances:** Agressivo (ataca tudo num raio de até **12 tiles** do jogador) · Passivo (não ataca ninguém) · **Defensivo (default)** (só ataca quem chega a **≤4 tiles** do jogador ou que ataca o jogador) · Suporter (cura/buff — para papéis de suporte). |
| 3.3 | **B ★ (reconciliado com 3.2)** — 3 stances de combate (Agressivo/Defensivo/Passivo) + **Suporter** como modo dos papéis de suporte. |
| 3.4 | **C (OVERRIDE — conflita com COMPANIONS_DIRECTION §45-46)** — Companion equipa **arma + acessório** no **sistema base**. → **EMENDAR a direction** (equipment deixa de ser "futuro"). Companions seguem na WAVE 14. |
| 3.5 | **A ★** — Derrota do companion: Downed → janela de resgate → senão Retreat automático → `Injured` 1-2 dias. |
| 3.6 | **(CUSTOM)** — Quando o **player** é derrotado com companion ativo: o companion **tenta reviver o player (30% de chance)**. Se conseguir, player volta; se falhar, companion **escapa e volta `Injured` 1 dia**. |
| 3.7 | **B (OVERRIDE)** — Companions **PODEM morrer** em **eventos narrativos específicos** (roteirizados); a Fonte de Anya **ressuscita com custo progressivo** ("Ressurreição Dolorosa"). **Reconciliação com 3.5:** combate base = SEM permadeath (só `Injured`); permadeath existe **apenas** em eventos narrativos marcados. |
| 3.8 | **A ★** — Fertilizante raro automático: **toggle por job de Planter, default OFF**, consome do storage autorizado (executa o SIM da 5.5). |
| 3.9 | **B ★** — Sem captura no base; 1 pet mágico narrativo de late-game ligado à Fonte (caminho da PETS_DIRECTION). |
| 3.10 | **A ★** — DPS alvo: 25% comum / 40% especialista, em `CompanionBalanceProfileSO`. |
| 3.11 | **A ★** — Bond 0-5; 1 perk passivo nos níveis 2/4; JobRank/CaveRank separados (estrutura já existe no código). |
| 3.12 | **A ★** — Manter WAVE 14; este refinamento converte as specs `14_spec_companion_*` de "future mapped" para spec-ready (implementação segue gated). |

## BLOCO 4 — BOAS PRÁTICAS / APRESENTAÇÃO (trava F14/F20/F56/F58 e o checkpoint M1)

| # | Decisão |
|---|---------|
| 4.1 | **A ★** — Relógio do dia **PAUSA** em qualquer modal/painel/diálogo/loja (padrão Stardew). `GameTimeManager` ganha gate único; resolve a duplicidade MenuManager × PauseMenuController. |
| 4.2 | **A ★** — Criar **fable_71 "combat feel pass"**: números de dano flutuantes (fecha o fantasma da F43), hit-stop curto 40-60ms em heavy/posture break, screen shake leve em boss/charged. Só consome eventos existentes; janela LIVRE como F58. |
| 4.3 | **A ★** — Escala de arte canônica: **32 px/tile** (Medium 1×1 = 32px, Huge 3×3 = 96px). → ADR antes de qualquer sprite final. |
| 4.4 | **A ★** — Aba Sistema (v1): volumes Master/SFX/Música persistidos + toggle Fullscreen/Windowed + dropdown de resolução. Nada mais. |
| 4.5 | **A ★** — v1 é **teclado/mouse ONLY**; gamepad vira spec pós-v1. → registrar no `input_map` (F67). |
| 4.6 | **A ★** — Dificuldade **única** no v1, tuning por telemetria F59; sem assist mode. → registrar decisão. |
| 4.7 | **B (OVERRIDE)** — Localização: a partir da **P4** (F35/F36/F70 + diálogos), textos de quest/diálogo entram via **tabela id→string** (disciplina leve, sem framework). → ADR + convenção de tabela antes da onda P4. |
| 4.8 | **A ★** — Save corrompido: load falhou → oferecer automaticamente o backup rolling ("Save danificado — restaurar backup de <data>?"). Emenda F56. |
| 4.9 | **A ★** — F58 inclui `MusicState` (Calmo/Combate/Boss/Festival) + eventos de transição com crossfade placeholder; faixas reais na fase de áudio. |

---

## Conflitos a reconciliar (geram emenda/ADR explícita)

1. **3.4 (C) × COMPANIONS_DIRECTION §45-46** (equipment era "futuro") → **emendar a direction**: companion equipment (arma+acessório) passa a ser sistema base.
2. **3.7 (B) × 3.5 (A)** → resolvido neste doc: permadeath **só** em eventos narrativos roteirizados; combate base permanece `Injured`. A "Ressurreição Dolorosa" da Fonte cobre o permadeath narrativo.
3. **1.9 (A) × invariante "exatamente 50" da F42** → emendar F42 para "50 base + 1/ato (~55)" e rebalancear tiers.
4. **1.11 (B)** → novo **sistema de revalidação de equipamento** ao respecar (emenda F29) — adiciona complexidade que não existia no design anterior.

## Ambiguidades interpretadas (para veto rápido)

1. **3.3 vs 3.2** — "Suporter" foi tratado como **modo de papel** (não 4ª stance universal): só papéis de suporte expõem o modo Suporter; as 3 stances universais são Agressivo/Defensivo/Passivo. Se você quis 4 stances para todos, avise.
2. **3.1** — interpretei como "encha o catálogo de papéis com bônus+ações antes de fixar o nº inicial de companions", não como escolha A/B/C. O "C" anotado fica como piso (pelo menos o Explorador como primeiro desbloqueio guiado).
3. **2.9 (B)** — "água vira item" implica um ponto de coleta (balde no poço/lago). Vou propor um `tool_bucket` + recurso `item_material_water` na emenda do catálogo; confirme se prefere água como recurso de estação de cozinha (sem balde).
4. **4.7 (B)** — a tabela id→string vale para **texto novo a partir da P4**; o texto já hardcoded (diálogos das waves anteriores) NÃO será retrofitado agora — fica como dívida registrada no ADR.

---

## Plano de emendas / artefatos decorrentes (PRELIMINAR — confirmar contra a re-auditoria de código)

| Artefato | Origem | Tipo |
|---|---|---|
| Adendo numérico de skills (dano/cooldown/custo + cadeias de pré-requisito dos 70 nós) | 1.3/1.4/1.7 | anexo à `SKILL_ACTION_MOVEMENT_TABLE` |
| Reescrita de `skill_tree_rules.md` | 1.8 | game_rule (decision-rule-extraction) |
| Reescrita de `inventory_equipment_rules.md` | 2.12 | game_rule |
| Hooks nomeados na F29 + 1 linha de escopo em F06/F17/F31/F48/F49/F55 | 1.6 | emendas de spec |
| Sistema de revalidação de equipamento no respec | 1.11 | emenda F29 |
| Emenda F42 (50 + 1/ato; rebalancear tiers) | 1.9 | emenda spec |
| ITEM_CATALOG: receitas poção/óleo, mágicos F31, gear tier alto, drops órfãos, água/carne/milk, ~10 peixes | 2.2/2.3/2.6/2.8/2.9/2.10 | emendas catálogo |
| **Matriz de gostos de presente NPC × gift** + itens `gift_*` | 2.4 (CUSTOM) | novo doc + emenda F26 |
| Tabela derivada durabilidade/upgrade | 2.11 | emenda baseline |
| Enums save-safe (se a re-auditoria confirmar que faltam) | 2.7 | emenda F32/F03 |
| **Catálogo de Papéis de Companion** (bônus + ações por papel) | 3.1 (CUSTOM) | novo doc + emenda COMPANIONS_DIRECTION |
| Emenda COMPANIONS_DIRECTION: stances/comandos/equip(arma+acessório)/revive 30%/permadeath narrativo | 3.2/3.4/3.6/3.7 | emenda direction + specs 14_* |
| **fable_71 — combat feel pass** | 4.2 | nova spec |
| Emenda F43 (números de dano vêm da fable_71) | 4.2 | emenda spec |
| Emenda F14 (pausa do relógio com modal) + F56 (save corrompido) + F58 (MusicState) | 4.1/4.8/4.9 | emendas spec |
| ADRs: 32px/tile · localização id→string desde P4 · gamepad pós-v1 · dificuldade única | 4.3/4.5/4.6/4.7 | ADRs via F67 |

---

## Re-auditoria de código (2026-06-13) — ajustes confirmados ao plano

> Workflow `reaudit-code-v3` (6 dimensões) verificou os achados contra o working tree melhorado.
> Resultado: 3 premissas caíram (evita retrabalho), 5 acréscimos novos. Resumo:

**JÁ RESOLVIDO NO CÓDIGO (sai do plano ou vira mínimo):**
- **Números de dano flutuantes JÁ EXISTEM e estão wired** (`FloatingDamageNumberDisplayer` + `DamagePopupAnchor`, séries 14A-FIX). → **fable_71 NÃO cria números**; escopo vira só **hit-stop + screen shake + `HudSuppressionChangedEvent`** (este último confirmado inexistente).
- **Inventário já é 30 slots** (`InventoryManager` DefaultCapacity=MaxCapacity=30), não 20. → **nenhuma emenda de código**; o game_rule (que ainda diz 20) segue para reescrita.
- **21 executores de skill reais já existem** + catálogo cresceu para **69 nós** (patch WI-11). → a parte "criar execução de skill" some; resta a fração menor abaixo.

**AINDA VERDADEIRO (emenda integral):**
- `WeaponType` sem **Hammer/Wand/Tool**; `ItemCategory` sem **Armor/Shield/Accessory/Relic/Essence/AnimalProduct** (nota: `Tool` JÁ existe em `ItemCategory`; "armadura" já existe via `EquipmentSlot` — a emenda 2.7 fala de **categoria de item**, não de "sistema de armadura").
- **7 game_rules obsoletos** confirmados (skill_tree + inventory). Como ambos são `status: accepted`, a reescrita exige **ADR superseding** (governança), não só edição.
- **Zero código de companion em combate/caverna** (brain/assist/downed/HUD/stances/comandos/equip/revive) — emenda integral.
- **5 modificadores de skill mortos** (BowProjectileSpeedFlat, DualWieldAttackSpeedBonus, TwoHandedDamageBonus, DodgeCostReduction, StatusDurationReduction): declarados e atribuídos a nós, sem consumidor → implementar consumo OU aposentar.

**ACRÉSCIMOS NOVOS (não estavam no plano v3):**
1. **Dois sistemas de slot de skill paralelos**: `ActiveSkillExecutionController` (teclas **1-4**, caminho real WI-11) vs `ActiveSkillSlots.cs` legado (teclas **R/T/Y/G**). → desambiguar/aposentar um (emenda F29).
2. **`NpcGiftPreferences` já existe** (Liked/Loved/Disliked + DailyGiftLimit) mas é **código morto** → a emenda 2.4 fica menor: adicionar `neutral`/`hated`, cabear leitura/escrita/geração/validação, reutilizar a tag `ItemTag.Giftable` existente.
3. **`CompanionManagerSaveData` é só DTO, sem `SaveSectionProvider`** (sem round-trip) → incluir o provider no escopo da spec de companion.
4. **`Romance`/`Spouse` existem em `CompanionEligibilityFlags` (bools) mas NÃO no enum `CompanionRole`** → decidir unificação no catálogo de papéis.
5. **Skill point: código dá 1 a cada 2 níveis** (`SkillPointIntervalLevels=2` → 50 base no cap 100). Com a decisão 1.9 (+1/ato) o teto vira ~55 — o game_rule reescrito deve refletir "1/2 níveis + 1/ato".
6. **Dois modelos de item paralelos** (`ItemDataSO` SO vs `ItemDefinition` POCO com Rarity/Quality/EconomicFlags) — risco de drift; decidir unificação (relevante à 2.7).

*Gerado em 2026-06-13 a partir das respostas do dono ao Refinamento v3, com plano confirmado
contra a re-auditoria de código (`reaudit-code-v3`, 7 agentes, evidência por arquivo:linha).*
