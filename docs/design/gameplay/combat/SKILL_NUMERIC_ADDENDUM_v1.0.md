# Cindar's Hope — Skill Numeric Addendum v1.0

> **Status:** documento canônico — CONTRATO NUMÉRICO das skills (dano base / cooldown / custo),
> cadeias de pré-requisito dos 70 nós e triggers de capstone fechados.
> **Local:** `docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md`
> **Origem:** decisões VINCULANTES `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (1.1, 1.2, 1.3, 1.4, 1.5, 1.7).
> **Anexa-se a:** `SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md` (corpo físico das ativas — telegraph/deslocamento/shape/recovery).
> **Depende de (vence em conteúdo):**
> - `PLAYER_SKILL_TREES_DIRECTION.md` — lista dos 70 nós, tiers, ranks e mecânica.
> - `SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md` — execução física e faixas de cooldown.
> - `FABLE_DECISOES_RESPOSTAS_v3.0.md` — decisões fechadas (vence em conflito; mais novo).
> **Não é spec implementável.** É o contrato que a **F29** (catálogo de skills) consome para
> trancar `DefaultSkillCatalog` e os executores de `ActiveSkillExecutionController`.

---

## 0. Como ler este documento

Decisão **1.4 (A)**: dano base, cooldown e custo das skills **ativas** ficam num adendo numérico
único (tabela skill × rank) anexado à `SKILL_ACTION_MOVEMENT_TABLE` **antes da F29** — vira o
contrato dos executores. Este é esse adendo.

```text
ESCOPO numérico (Parte 1): SOMENTE skills ATIVAS executáveis (decisão 1.5).
  Passivas e modificadores NÃO recebem linha de dano/cooldown/custo: seus valores por rank já
  vivem na coluna "Mecânica in-game" da PLAYER_SKILL_TREES_DIRECTION e não são executores.
ESCOPO de pré-requisito (Parte 2): TODOS os 70 nós (decisão 1.3=B), ativos e passivos.
ESCOPO de capstone (Parte 3): triggers fechados das 5 capstones com gatilho dinâmico (decisão 1.7).
```

Fonte numérica autoritativa das ativas executáveis = os valores **WI-11** já vivos em
`Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
(`RegisterCombatExecutors`) + as faixas da `SKILL_ACTION_MOVEMENT_TABLE` §0.5. Onde a direction
e o código divergem em nome, esta tabela reconcilia (ver §1.0 nota de nomenclatura).

---

## 1. Tabela numérica das skills ATIVAS por rank (dano / cooldown / custo)

### 1.0 Nota de nomenclatura e reconciliação código × direction

Decisão **1.5** fechou a lista de executáveis pelos **nomes de design**: *Corte Amplo, Golpe de
Ruptura, Projétil Arcano, Flecha Perfurante, Toque Restaurador*. O código WI-11 vivo expõe
executores reais cujos **IDs/nomes** nem sempre batem 1:1 com esses nomes de design (o catálogo
cresceu para 69 nós e ganhou ativas extras nos patches WI-11). Esta tabela:

- usa os **valores numéricos reais** já registrados em `RegisterCombatExecutors` (autoridade);
- mapeia cada nome de design da decisão 1.5 para o **executor que cumpre o papel**;
- inclui também as ativas extras WI-11 que já têm executor real (são executáveis de fato).

```text
RECONCILIAÇÃO 1.5 (papel de design  →  executor real / effectId WI-11):
  Corte Amplo (Melee T2, arco)        →  combat.melee.offhand_cut / combat.melee.whirl_cut (arco/círculo)
  Golpe de Ruptura (Melee T4, posture)→  melee.investida_quebra_guarda (alto posture, postureMult 3x)
  Projétil Arcano (Magic T1, bolt)    →  combat.magic.fire_spark / magic.chama_breve (bolt elemental básico)
  Flecha Perfurante (Ranged T3, pierce)→ combat.ranged.line_piercer (pierce em linha)
  Toque Restaurador (Magic T3, cura)  →  survival.kit_emergencia / survival.campo_seguro (restore real)
                                          [a cura % de HP da magia segue §1.5; o restore flat já existe]
DORMENTES (NotYetExecutable, decisão 1.5 — feedback-only, sem dano/CD operacional ainda):
  Marcador de Presa · Armadilha · Selo de Proteção · Disparo de Interrupção ·
  Disparo Carregado · Descanso Curto.
```

### 1.1 Convenção das colunas

```text
Dano base   = dano do projétil/golpe no RANK BASE (sem mods passivos de árvore). Inteiro, igual ao executor.
Escala/rank = como o número sobe por rank (interpolar nos executores; ver §1.6).
Cooldown    = segundos, RANK BASE. Ranks altos reduzem recovery, NÃO o cooldown base (§1.6).
Custo       = STA (stamina) para ações físicas; MP (mana) para magia; "—" para restores/utilidades sem custo.
Cap de rank = DINÂMICO por profundidade da árvore (decisão 1.1; ver §3 nota).
Custo de ponto = 1 ponto por rank, sempre, inclusive ranks de capstone (decisão 1.2).
```

### 1.2 MELEE — ativas executáveis

| Skill (effectId / papel 1.5) | Tier | Ranks | Dano base (R1) | Escala/rank | Cooldown (s) | Custo | Shape |
|---|---:|---:|---:|---|---:|---:|---|
| Corte da Mão Secundária `combat.melee.offhand_cut` | 1 | 1-3 | 8 | +2/rank | 2.5 | 10 STA | arco 140° × 1.2 t |
| Corte Giratório `combat.melee.whirl_cut` (= Corte Amplo, arco/círculo) | 2 | 1-3 | 10 | +2/rank | 6.0 | 22 STA | círculo 360° × 1.7 t |
| Avanço de Aço `melee.avanco_aco` | 2 | 1-3 | 12 | +3/rank | 6.0 | 22 STA | lunge 2.5 t + arco 110° × 1.3 t |
| Avanço de Batalha `combat.melee.battle_dash` | 3 | 1-3 | 8 | +2/rank | 5.0 | 20 STA | lunge 3.0 t + arco 100° × 1.2 t |
| Ataque Saltante `combat.melee.leap_attack` | 3 | 1-3 | 14 | +3/rank | 7.0 | 25 STA | lunge 2.2 t + arco 120° × 1.4 t |
| Grito de Desafio `melee.grito_desafio` | 3 | 1-3 | 6 | +1/rank | 8.0 | 18 STA | taunt 360° × 2.2 t (knockback 6) |
| Investida Quebra-Guarda `melee.investida_quebra_guarda` (= Golpe de Ruptura) | 4 | 1-3 | 16 | +4/rank | 9.0 | 26 STA | lunge 2.0 t + arco 90° × 1.3 t · **posture ×3** |

### 1.3 RANGED — ativas executáveis e dormentes

| Skill (effectId / papel 1.5) | Tier | Ranks | Dano base (R1) | Escala/rank | Cooldown (s) | Custo | Projétil |
|---|---:|---:|---:|---|---:|---:|---|
| Linha Perfurante `combat.ranged.line_piercer` (= Flecha Perfurante) | 3 | 1-3 | 12 | +3/rank | 7.0 | 18 STA | pierce até 5 alvos · 14 t/s · 10 t · -20% dano/alvo |
| Leque de Flechas `combat.ranged.multishot_fan` | 3 | 1-5 | 8 | +2/rank | 8.0 | 24 STA | 3 projéteis · spread 28° · 11 t/s · 7 t |
| Flecha Lacerante `combat.ranged.bleeding_arrow` | 3 | 1-3 | 14 | +3/rank | 6.0 | 16 STA | 12 t/s · 8 t · aplica `status_bleed` |
| **Disparo Carregado** (DORMENTE) | 1 | 1-5 | charge 0.65-1.10s | — | 6.0 (alvo) | 38 STA | feedback-only — sem executor de dano (decisão 1.5) |
| **Disparo de Interrupção** (DORMENTE) | 3 | 1-3 | — | — | — | 25 STA | feedback-only — projétil 16 t/s reservado (1.5) |
| **Marcador de Presa** (DORMENTE) | 1 | 1-3 | — | — | — | 10 STA | feedback-only — marca pendente (1.5/1.10) |
| **Armadilha de Caçador** (DORMENTE) | 3 | 1-3 | — | — | — | 20 STA | feedback-only — trap pendente (1.5) |

### 1.4 MAGIC — ativas executáveis e dormentes

| Skill (effectId / papel 1.5) | Tier | Ranks | Dano base (R1) | Escala/rank | Cooldown (s) | Custo | Projétil/efeito |
|---|---:|---:|---:|---|---:|---:|---|
| Faísca de Fogo `combat.magic.fire_spark` (= Projétil Arcano, fogo) | 1 | 1-5 | 12 | +2/rank | 3.0 | 10 MP | bolt 10 t/s · 7 t |
| Chama Breve `magic.chama_breve` (Projétil Arcano básico, MP baixo) | 2 | 1-3 | 8 | +2/rank | 2.5 | 8 MP | bolt 10 t/s · 6 t · chance Burn |
| Prisão de Gelo `combat.magic.ice_bind` | 2 | 1-3 | 10 | +2/rank | 6.0 | 14 MP | 9 t/s · 7 t · aplica `status_chill` |
| Rajada Gélida `magic.rajada_gelida` | 3 | 1-3 | 6 | +1/rank | 6.0 | 16 MP | 3 projéteis · spread 30° · 9 t/s · 6 t · Chill |
| Nuvem Tóxica `combat.magic.toxic_cloud` | 3 | 1-5 | 8 | +2/rank | 8.0 | 18 MP | 3 projéteis · spread 40° · 7 t/s · `status_poison` |
| Corrente Elétrica `combat.magic.lightning_chain` | 3 | 1-5 | 12 | +2/rank | 8.0 | 20 MP | salta até 4 alvos · 16 t/s · 9 t |
| **Selo de Proteção** (DORMENTE) | 2 | 1-3 | — | — | — | 18 MP | feedback-only — barreira 2/3/4s pendente (1.5) |
| **Toque Restaurador** (papel 1.5 = restore) | 3 | 1-5 | cura 8/12/16/20/24% HP máx | +4 pts %/rank | 30-60 | 30 MP | ver §1.5 (cura %); restore flat já existe via Survival |

### 1.5 SURVIVAL — restores executáveis e dormentes

| Skill (effectId / papel 1.5) | Tier | Ranks | Restaura (R1) | Escala/rank | Cooldown (s) | Custo | Nota |
|---|---:|---:|---|---|---:|---:|---|
| Kit de Emergência `survival.kit_emergencia` (= Toque Restaurador, restore flat) | 3 | 1-5 | +30 HP | +6 HP/rank | 45 | — | fora de combate |
| Instinto de Sobrevivência `survival.instinto_sobrevivencia` | 3 | 1-5 | +50 STA | +8 STA/rank | 30 | — | reveal + restore |
| Campo Seguro `survival.campo_seguro` | 4 | 1-5 | +15 HP / +25 STA / +15 MP | +3/+5/+3 por rank | 60 | — | zona de descanso |
| **Descanso Curto** (DORMENTE) | 3 | 1-5 | — | — | — | — | feedback-only — canal 2.0s fora de combate (1.5) |

> A "cura % de HP máximo" do **Toque Restaurador** (Magic T3: 8/12/16/20/24% por rank, custo 30 MP,
> cooldown 30-60s) é o **papel canônico de cura** da decisão 1.5. O executor real de restore que já
> existe é flat (HP/STA/MP fixos via Survival). A F29 deve, ao consumir este contrato, decidir se
> converte o Toque Restaurador em restore percentual de MP no Magic ou mantém o restore flat de
> Survival como cumprindo o papel. Ambos contam como "Toque Restaurador" executável para 1.5.

### 1.6 CRAFTING — ativa ofensiva executável

| Skill (effectId) | Tier | Ranks | Dano base (R1) | Escala/rank | Cooldown (s) | Custo | Projétil |
|---|---:|---:|---:|---|---:|---:|---|
| Bomba Improvisada `crafting.bomba_improvisada` | 3 | 1-3 | 18 | +3/rank | 12.0 | 20 STA (carga) | salta até 3 alvos · 8 t/s · 5 t · Toxic |

### 1.7 Regras de escala e recovery por rank

```text
1. Cooldown BASE não muda por rank. O que cai por rank é o RECOVERY físico (PARTE D da
   SKILL_ACTION_MOVEMENT_TABLE: "ranks altos reduzem recovery — interpolar nos executores").
   Sugestão de interpolação: -8% recovery por rank acima de R1, piso de 0.10s.
2. Dano base sobe pela coluna "Escala/rank" (+N por rank acima de R1). Ex.: Faísca de Fogo
   R1=12, R2=14, R3=16, R4=18, R5=20. Inteiros, batendo com o int baseDamage dos executores.
3. Custo (STA/MP) NÃO sobe por rank: é fixo no valor do executor. Passivas de árvore
   (Canalização Serena, Ritmo Controlado) reduzem custo via modificador, não esta tabela.
4. Faixas de cooldown (contrato com a SKILL_ACTION_MOVEMENT_TABLE §0.5):
     golpes melee rápidos   2.5-3.0 s
     golpes melee pesados   6.0-9.0 s
     projéteis de técnica   6.0-8.0 s
     utilidades/gadgets     8.0-12.0 s
     restores (survival)    30-60 s
5. Mods passivos de árvore aplicam-se DEPOIS do dano base desta tabela (multiplicativo do
   executor); este contrato fixa o piso, não o teto buffado.
```

---

## 2. Cadeias de PRÉ-REQUISITO dos 70 nós (decisão 1.3 = B)

Decisão **1.3 (OVERRIDE, B)**: além do tier gating (pontos gastos na árvore — `PLAYER_SKILL_TREES_DIRECTION` §3),
**cada nó lista 0-2 pré-requisitos dentro da MESMA árvore**. A coluna nova abaixo é o contrato; a
F29 grava em `SkillNodeDataSO.PrerequisiteNodeIds` (lista) + mantém `RequiredPurchasedNodesInTree`
para o tier gating. Os pré-reqs já existentes no `DefaultSkillCatalog` foram preservados; onde a
direction tem nó sem analog de código ainda, o pré-req segue a topologia de tier.

```text
LEITURA:
  - "raiz" = nó de entrada do tier (0 pré-req); só o tier gating (pontos na árvore) o trava.
  - Pré-reqs sempre na MESMA árvore (regra 1.3). Capstone exige o nó imediatamente anterior + tier 5.
  - Onde o nó tem effectId/ID de código vivo, o ID está entre crases para a F29 cabear direto.
```

### 2.1 MELEE (14 nós)

| Nó (direction) | Tier | Pré-requisito(s) (mesma árvore) |
|---|---:|---|
| Treinamento Marcial / `melee_iron_grip` | 1 | — (raiz) |
| Ataque Pesado | 1 | Treinamento Marcial |
| Block / Bloqueio / `melee_guarded_block` | 1 | `melee_guarded_stance` (Postura Guardada) |
| Postura Guardada / `melee_guarded_stance` | 1 | Treinamento Marcial |
| Guarda Firme | 2 | Block / Bloqueio |
| Corte Amplo / `melee_whirl_cut` (`melee_offhand_cut`) | 2 | Treinamento Marcial; (offhand: `melee_dual_wield_flow`) |
| Avanço de Aço / `melee.avanco_aco` | 2 | Treinamento Marcial |
| Contra-Ataque | 3 | Block / Bloqueio; Corte Amplo |
| Quebra-Postura | 3 | Ataque Pesado |
| Lâmina de Abertura | 3 | Contra-Ataque |
| Especialização: Arma Pesada | 3 | Quebra-Postura |
| Especialização: Arma Leve | 3 | Lâmina de Abertura |
| Pele de Batalha | 4 | Guarda Firme |
| Golpe de Ruptura / `melee.investida_quebra_guarda` | 4 | Quebra-Postura; Avanço de Aço |
| Capstone Kanthor/Kaand / `melee_capstone_battle_rhythm` | 5 | Golpe de Ruptura; Pele de Batalha (+ tier 5) |

### 2.2 RANGED (13 nós)

| Nó (direction) | Tier | Pré-requisito(s) (mesma árvore) |
|---|---:|---|
| Treino de Arco / `ranged_steady_hand` | 1 | — (raiz) |
| Marcador de Presa / `ranged_marked_prey` | 1 | Treino de Arco |
| Disparo Carregado / `ranged_charged_shot` | 1 | Mira Longa (`ranged_long_sight`) |
| Passo do Caçador / `ranged_kiting_steps` | 2 | Treino de Arco |
| Leitura de Abertura | 2 | Marcador de Presa |
| Disparo de Interrupção | 3 | Encaixe Rápido (`ranged_quick_nock`) |
| Flecha Perfurante / `ranged_line_piercer` | 3 | Encaixe Rápido |
| Armadilha de Caçador | 3 | Leitura de Abertura |
| Caçador de Voadores | 3 | Disparo Carregado |
| Flecha Preparada / `ranged_bleeding_arrow` | 4 | Flecha Perfurante |
| Retirada Tática | 4 | Passo do Caçador; Armadilha de Caçador |
| Olho do Caçador | 4 | Caçador de Voadores |
| Capstone Três Luas / `ranged_capstone_eagle_focus` | 5 | Flecha Preparada; Olho do Caçador (+ tier 5) |

### 2.3 MAGIC (15 nós)

| Nó (direction) | Tier | Pré-requisito(s) (mesma árvore) |
|---|---:|---|
| Canalização Serena / `magic_quick_channel` | 1 | Poço de Mana (`magic_mana_well`) |
| Foco Arcano / `magic_arcane_edge` | 1 | Poço de Mana |
| Projétil Arcano / `magic_fire_spark` (`magic.chama_breve`) | 1 | Foco Arcano |
| Fluxo Lento | 2 | Canalização Serena |
| Selo de Proteção | 2 | Projétil Arcano |
| Afinidade Elemental: Fogo | 2 | Projétil Arcano |
| Afinidade Elemental: Gelo / `magic_ice_bind` | 2 | Canalização Serena |
| Afinidade Elemental: Raio / `magic_lightning_chain` | 3 | Afinidade Gelo |
| Afinidade Natural/Água | 3 | Fluxo Lento |
| Toque Restaurador | 3 | Selo de Proteção |
| Uso de Item Mágico | 3 | Foco Arcano |
| Resistir Corrupção | 4 | Afinidade Natural/Água |
| Eco da Fonte | 4 | Toque Restaurador |
| (Rajada Gélida / `magic.rajada_gelida`) | 3 | Afinidade Elemental: Gelo |
| Capstone Anya/Senya / `magic_capstone_elemental_confluence` | 5 | Resistir Corrupção; Eco da Fonte (+ tier 5) |

### 2.4 SURVIVAL (14 nós)

| Nó (direction) | Tier | Pré-requisito(s) (mesma árvore) |
|---|---:|---|
| Estômago Forte / `survival_low_rations` | 1 | Pulmões da Caverna (`survival_cave_lungs`) |
| Ritmo de Jornada | 1 | Pulmões da Caverna |
| Reflexo de Esquiva | 1 | Pele Dura (`survival_hard_skin`) |
| Passo de Impulso / `survival_safe_step` | 2 | Estômago Forte |
| Ritmo Controlado | 2 | Reflexo de Esquiva |
| Saqueador Cuidadoso | 2 | Ritmo de Jornada |
| Garimpo de Run | 2 | Saqueador Cuidadoso |
| Descanso Curto / `survival_status_recovery` | 3 | Ritmo Controlado |
| Regeneração Natural | 3 | Reflexo de Esquiva; Descanso Curto |
| Resistência Ambiental | 3 | Passo de Impulso |
| Kit de Emergência / `survival.kit_emergencia` | 3 | Descanso Curto |
| Faro de Tesouro | 4 | Garimpo de Run |
| Mente Inabalável | 4 | Resistência Ambiental |
| Resistência de Run Profunda / Campo Seguro `survival.campo_seguro` | 4 | Resistência Ambiental; Kit de Emergência |
| Capstone Telisandra / `survival_capstone_caveborn` | 5 | Faro de Tesouro; Mente Inabalável (+ tier 5) |

> Survival tem 14 nós de direction + nós extras de execução WI-11 (`survival.sinal_retirada`,
> `survival.isca_improvisada`, `survival.instinto_sobrevivencia`); a F29 consolida-os sob os nós
> de direction acima sem ultrapassar o teto de 0-2 pré-reqs/nó.

### 2.5 CRAFTING (14 nós)

| Nó (direction) | Tier | Pré-requisito(s) (mesma árvore) |
|---|---:|---|
| Mãos de Lavrador / `crafting_fast_hands` | 1 | — (raiz) |
| Coleta Eficiente / `crafting_material_eye` | 1 | Mãos de Lavrador |
| Lenhador Prático | 1 | Mãos de Lavrador |
| Prospector de Superfície | 1 | Coleta Eficiente |
| Cozinha Sustentadora | 2 | Lenhador Prático |
| Oficina Organizada / `crafting_station_focus` | 2 | Coleta Eficiente |
| Ferramentas de Ferro | 2 | Oficina Organizada |
| Ferramentas de Aço | 3 | Ferramentas de Ferro |
| Forja de Prata | 3 | Ferramentas de Aço |
| Alquimia Prática / `crafting.bomba_improvisada` | 3 | Cozinha Sustentadora |
| Trabalho em Mithril | 4 | Forja de Prata |
| Engenharia Bromeciana / `crafting.mecanismo_campo` | 4 | Trabalho em Mithril |
| Cultivo de Mana | 4 | Alquimia Prática |
| Capstone Thoren / `crafting_capstone_master_artisan` | 5 | Trabalho em Mithril; Cultivo de Mana (+ tier 5) |

> **Invariantes das cadeias (contrato F29):**
> ```text
> - Nenhum nó referencia pré-req fora da própria árvore (regra 1.3).
> - Máximo 2 pré-reqs por nó; raízes têm 0.
> - Sem ciclos: o grafo de pré-reqs é um DAG (todo pré-req está em tier ≤ tier do nó).
> - Tier gating (pontos na árvore) e pré-req de nó são CUMULATIVOS: compra exige os dois.
> - Capstone (tier 5) exige os 2 nós listados + cap de pontos do tier 5 (26 na árvore).
> ```

---

## 3. Triggers de CAPSTONE fechados (decisão 1.7) + nota de rank cap dinâmico e custo de ponto

### 3.1 Sequência de MP — janela única de 6s (capstones Anya/Senya)

Decisão **1.7**: o gatilho "gastar 35% do MP máximo em sequência" das duas capstones de Magic
(Semente de Anya, Semente de Senya) tem **janela fechada de 6 segundos**.

```text
GATILHO (Anya e Senya): acumular gasto ≥ 35% do MP MÁXIMO dentro de uma JANELA DESLIZANTE de 6s.
  - O acumulador soma MP gasto por casts; reseta se 6s passarem sem novo gasto.
  - Ao cruzar 35% do MP máx dentro da janela, dispara a Semente correspondente à capstone equipada.
  - Anya: Semente por 10s (suporte: próxima magia espiritual -50% MP, +35% cura/barreira/purificação,
    eco de regen leve 4s). Bloqueia Senya.
  - Senya: Semente por 8s (ofensiva: próxima magia +35% Magic Damage, elemental amplificado,
    +15% crit mágico/overload, custo +10% MP). Bloqueia Anya.
  - Capstones são EXCLUSIVAS entre si (PLAYER_SKILL_TREES_DIRECTION §6): respec na Fonte de Anya troca.
```

### 3.2 Kanthor — cura condicional ao HP (capstone Melee)

Decisão **1.7**: o "próximo hit em critical window" do Voto do Aço Profundo de Kanthor resolve assim:

```text
GATILHO Kanthor: bloquear perfeitamente OU quebrar postura → "Julgamento de Aço" 8s
  (+15% dano melee, +25% Block Stability, +20% resistência a stagger).
EFEITO do hit em critical window durante o buff:
  SE HP < 100% do HP máximo  → cura 3% do HP máximo;
  SENÃO (HP == 100%)         → recupera +10 Stamina.
  (Bloqueia Kaand. Kaand permanece sem cura — só ofensivo: +30% dano, +25% crit dmg, etc.)
```

### 3.3 Telisandra — gatilho de emergência (capstone Survival)

Decisão **1.7**: "entrar em HP baixo, Stamina crítica ou cansaço alto" do Último Fôlego de
Telisandra fecha nos seguintes limiares (OU lógico, 1×/run):

```text
GATILHO Telisandra (1× por run de caverna), dispara se QUALQUER condição:
  HP      < 25% do HP máximo
  OU Stamina < 15% da Stamina máxima
  OU estado "Exausto" ativo (cansaço alto).
EFEITO "Fôlego de Telisandra" por 10s:
  -45% gasto de Stamina em Dash/Dodge/corrida; +30% resistência ambiental/mental;
  Dodge ganha pequena janela extra; HP Regen fora de combate DOBRA após sair do perigo.
```

> As outras duas capstones (Ranged "Marca da Caça das Três Luas"; Crafting "Forja Viva de Thoren")
> não têm gatilho dinâmico de recurso e ficam como na `PLAYER_SKILL_TREES_DIRECTION` §11/§17
> (Três Luas = primeiro alvo marcado no encontro; Thoren = 1×/dia ao craftar/colher lote).

### 3.4 Nota: rank cap DINÂMICO (decisão 1.1)

Decisão **1.1 (B)**: o rank cap é **dinâmico por profundidade da árvore** — o cap de TODAS as
skills da árvore sobe conforme o tier mais alto desbloqueado naquela árvore.

```text
Tier 1 aberto  → cap de rank 2 (todas as skills da árvore)
Tier 2 aberto  → cap de rank 3
Tier 3 aberto  → cap de rank 4
Tier 4 ou 5    → cap de rank 5
```

Implicação para esta tabela: as colunas "Escala/rank" só são alcançáveis até o cap dinâmico
vigente. Uma ativa de Tier 1 (ex.: Faísca de Fogo) começa limitada a rank 2 e só chega a rank 5
quando a árvore Magic atinge Tier 4/5. A direction §4 tinha cap "recomendado" por tier; a decisão
1.1 torna esse cap a **regra dura** que a F29 aplica no `SkillPurchaseService`.

### 3.5 Nota: custo de 1 ponto por rank (decisão 1.2)

Decisão **1.2 (A)**: cada rank custa **exatamente 1 ponto de skill**, inclusive os 3 ranks de
capstone. `SkillNodeDataSO.SkillPointCost` permanece 1; comprar rank N consome 1 ponto e exige
ranks 1..N-1 já comprados (e o cap dinâmico de §3.4). O game_rule `skill_tree_rules` (reescrito
sob decisão 1.8) é a fonte canônica da economia de pontos; este addendum só fixa o custo por rank.

---

## 4. Contrato para a F29 (o que esta tabela tranca)

```text
1. Toda ativa executável da Parte 1 vira/permanece executor real em ActiveSkillExecutionController
   com EXATAMENTE estes baseDamage/cost/cooldown no rank base (já batem com WI-11 hoje).
2. Toda dormente da decisão 1.5 permanece feedback-only (FeedbackOnlySkillEffectExecutor) com
   tooltip "efeito pendente" até a spec consumidora existir (decisão 1.6, hooks nomeados).
3. As cadeias de pré-req da Parte 2 viram PrerequisiteNodeIds (0-2/nó, mesma árvore) + o tier
   gating segue em RequiredPurchasedNodesInTree. Grafo DAG, sem cross-tree.
4. Os triggers da Parte 3 são os valores que o runtime de capstone deve checar (janela 6s;
   Kanthor 3% HP / +10 STA; Telisandra HP<25% OU STA<15% OU Exausto).
5. Rank cap dinâmico (§3.4) e 1 ponto/rank (§3.5) são travas do SkillPurchaseService.
```

> **Divergências conhecidas a resolver na F29 (não bloqueiam este contrato):**
> ```text
> - Nomes de design 1.5 × IDs de código WI-11: a F29 deve padronizar DisplayName por árvore
>   (ver §1.0). Os números são os do executor; os nomes de design são o rótulo canônico.
> - Toque Restaurador: decidir cura % de MP (Magic) vs restore flat (Survival) como cumprindo
>   o papel (§1.5).
> - 5 modificadores mortos (BowProjectileSpeedFlat, DualWieldAttackSpeedBonus, TwoHandedDamageBonus,
>   DodgeCostReduction, StatusDurationReduction): implementar consumo OU aposentar (fora do escopo
>   numérico; citado aqui só para a F29 não os tratar como dano/cooldown).
> - Dois caminhos de slot (teclas 1-4 real WI-11 vs ActiveSkillSlots R/T/Y/G legado): desambiguar
>   na F29; este contrato assume o caminho 1-4 (ActiveSkillExecutionController).
> ```

---

*Gerado em 2026-06-13 a partir das decisões VINCULANTES `FABLE_DECISOES_RESPOSTAS_v3.0.md`*
*(1.1/1.2/1.3/1.4/1.5/1.7), da `PLAYER_SKILL_TREES_DIRECTION`, da `SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0`*
*e dos valores WI-11 vivos em `ActiveSkillExecutionController.RegisterCombatExecutors`. Contrato numérico para a F29.*
