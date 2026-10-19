# Cindar's Hope — Quest Catalog Direction v1.1

> **Status:** documento canônico de catálogo nominal de quests
> **Local:** `docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md`
> **Complementa:** `QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md` (modelo de sistema vence)
> **Depende de:**
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md`
> - `QUESTS_MAIN_LORE_DIRECTION.md` + `QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
> - `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` (propósito/lore por NPC vence)
> - `BALANCE_CURVES_DIRECTION_v1.0.md` §7 (fórmula de XP/ouro escalados)
> **Função:** listar nominalmente as ~86 quests do v1, suas fontes, recompensas e conexões.
> **Não é spec implementável.** O QuestRegistry deriva daqui.

---

## 0. Regra anti-duplicação

```text
Modelo (QuestDefinition/State/Objective/Trigger/Reward/Flag): QUEST_OBJECTIVE_EVENT vence.
Lore da main quest: QUESTS_MAIN_LORE/PROGRESSION vencem.
Identidade/serviço de NPC: CITY_NPC_ROSTER v1.1 vence — toda side DEVE nascer do propósito do NPC.
Fórmula de recompensa: BALANCE_CURVES §7 vence (valores escalam com QuestLevel — nada estático).
Em conflito de LISTA (quais quests existem), ESTE catálogo vence.
```

---

# PARTE A — Como as quests chegam ao jogador

## 1. As cinco fontes (decisão Q6.1)

A vila fala com o jogador por cinco bocas, cada uma com sua linguagem visual:

```text
1. MAIN — "!" DOURADO sobre NPCs da história (Corvus, Thalindra, Maelor, Vaelrion...).
   Nunca expira. Conduz os 4 atos.
2. SIDE — "!" PRATEADO sobre NPCs com cadeia pessoal disponível (pré-requisitos atendidos).
   Cadeias de 3, nascidas do propósito do NPC; a 3ª destrava o serviço único dele.
3. QUADRO DE AVISOS — pergaminho na praça, gerido pelo Hund: 3 contratos rotativos/dia
   (templates procedurais). Aceita-se e entrega-se NO QUADRO.
4. CONTRATOS DE CAVERNA — quadro do Zrix na entrada da caverna: marcos de profundidade
   e desafios de run.
5. SECRETAS DA CAVERNA — sem marcador. Entregues por criaturas não-agressivas (Old
   Scrounger King, Goblin Warchief, Silence Warden) e pelos MERCADORES ERRANTES. O jogador
   descobre conversando, oferecendo, hesitando antes de atacar.
```

Regras de fila: sem limite de quests ativas; 1 quest "tracked" no HUD; abas no log
(Main/Side/Contratos/Secretas — esta última só lista as já descobertas).

## 2. Recompensas (decisão Q6.2 — tudo escala)

```text
XP e ouro usam a fórmula do BALANCE_CURVES §7: Base(tier) × (1 + 0.06 × QuestLevel).
QuestLevel: daily = nível do player no aceite; side = nível de referência da cadeia;
main = fixo por ato (10/35/65/90).
Main quest: +1 SKILL POINT no turn-in final de cada ato (4 no total).
Recompensas de item NUNCA são genéricas: cada side entrega algo com o cheiro do NPC.
```

---

# PARTE B — Main Quest: os Quatro Atos (20 quests)

## 3. A espinha do jogo

A main quest é a história da Fonte reaprendendo a falar — fragmento a fragmento — e do
jogador descobrindo que o nome da vila é um aviso. Cada ato termina num gate da caverna e
numa função nova da Fonte. *Nenhuma quest de ato expira; nenhuma depende de evento raro
sem pista (regra canônica).*

## 4. Ato 1 — A Fonte Adormecida (QuestLevel 10)

As 5 quests do Ato 1 estão integralmente especificadas na `fable_10_spec_main_quest_act1`:
`mq_act1_01_fonte_adormecida` → `02_registros_perdidos` → `03_eco_da_agua` →
`04_guardiao_da_agua` (gate 10) → `05_fragmento_da_agua` (**Fragmento da ÁGUA → Água Viva**).

## 5. Ato 2 — O Arco da Memória (QuestLevel 35)

| ID | Giver | Resumo | Entrega |
|---|---|---|---|
| mq_act2_01_records_of_silver | Thalindra | restaurar 3 memory_shard (Ninrorin de ruínas rasas) para reabrir a ala selada do arquivo | acesso + lore Nymiriana |
| mq_act2_02_the_veiled_visitor | **Vaelrion** (chega à cidade) | receber e escoltar o estudioso do Véu até a Fonte; ele reconhece as marcações tribais | apresenta o Arco |
| mq_act2_03_song_below | Liora | descer ao nível 25 com Liora (escolta script) e ouvir o eco que ela sonhou | a melodia vira pista |
| mq_act2_04_gate_of_frost | — | derrotar **Rimelock Colossus** (gate 30) | caminho às ruínas |
| mq_act2_05_fragment_of_memory | Fonte | ritual com Vaelrion na Fonte | **Fragmento da MEMÓRIA → respec**; Cindrathel "acorda" (foreshadow do 101) |

## 6. Ato 3 — A Pedra que Sussurra (QuestLevel 65)

| ID | Resumo |
|---|---|
| mq_act3_01_blackstone_ledger | Mara descobre compras anômalas de pedra negra nos registros — investigação NA CIDADE (diálogos, horários, o quadro mente) que revela um infiltrado do culto |
| mq_act3_02_thrall_mercy | purificar 3 Blackstone Thralls (Purify, F08) — **eles voltam como aldeões resgatados** (aparecem na cidade; a vila muda) |
| mq_act3_03_the_quiet_priest | confronto com o infiltrado — ESCOLHA: prender ou exilar (flag que retorna no final) |
| mq_act3_04_warden_of_silence | o **NPC Nymiriano** (decisão Q1.5) chega: ensina a acalmar o Silence Warden com Água Viva; revela-se o último do povo de Cindar |
| mq_act3_05_fragment_of_life | gate 70 (Draconic Guardian) + ritual | **Fragmento da VIDA → purificação** |

## 7. Ato 4 — A Esperança Enterrada (QuestLevel 90)

| ID | Resumo |
|---|---|
| mq_act4_01_litany_complete | reunir as 3 partes da Litania do Primeiro Retorno (arquivo + templo + o nymiriano) |
| mq_act4_02_the_jailer | derrotar o **Draconic Elder** (gate 100) — que passa a luta inteira AVISANDO, não impedindo |
| mq_act4_03_vel_karaum | nível 101: **Vel-Karaúm, Warden of the Arch** |
| mq_act4_04_broken_remembrance | **Cindrathel** — o espelho da própria build |
| mq_act4_05_final_choice | **Arquivista do Silêncio** → escolha Proteger/Selar/Usar; **Ithryndor** responde conforme (aliado/sono/última luta) |

---

# PARTE C — Side Chains por NPC (12 cadeias × 3 = 36 quests no v1)

## 8. Regra das cadeias

```text
Toda cadeia nasce do PROPÓSITO do NPC (roster v1.1) e termina destravando o serviço único
dele (SYSTEMS_DEEPENING C5). Estrutura: q1 doméstica (apresenta o NPC) → q2 que toca o
mundo (caverna/cidade/lua) → q3 com marco (miniboss/banda/ato) e recompensa permanente.
A 2ª leva (11 NPCs restantes, 33 quests) segue no v2 com os mesmos moldes.
```

## 9. As doze cadeias

**Brumdar (forja)** — *"o ferro lembra"*
sq_brumdar_1 **Cold Iron** (8): 10 iron_ore — ele testa sua palavra, não seu braço.
sq_brumdar_2 **The Old Anvil** (20): recuperar a bigorna do avô numa ruína rasa.
sq_brumdar_3 **Tempered Soul** (45): trazer o vask_hammer_head do Forge-Tyrant →
**destrava a Têmpera de Essência**.

**Ozzra (alquimia)** — *"tudo borbulha por um motivo"*
sq_ozzra_1 **Bubbling Trouble** (6): 5 glowcap. sq_ozzra_2 **Essence of the Matter** (28):
2 essence_ice ("frio engarrafado!"). sq_ozzra_3 **Forbidden Page** (50): o veil_grimoire da
Veilkin Witch → **destrava LearnableScrolls**.

**Thalindra (arquivo)** — *"a poeira guarda"*
sq_thalindra_1 **Dust and Order** (5): entregar 3 livros pela cidade. sq_thalindra_2
**Field Notes** (15): 3 amostras de criaturas (bestiário!). sq_thalindra_3 **The First
Archivist** (40): um nymirian_engraving → **análise de amostra 2×/dia**.

**Sylveth (sementes)** — *"a terra responde a quem pergunta"*
sq_sylveth_1 **Seeds of Trust** (4): colher 10 crops. sq_sylveth_2 **Night Bloom** (18):
cultivar 1 alihana_tear até a colheita. sq_sylveth_3 **The Stubborn Soil** (30): plantar
shadowroot NA CAVERNA → **troca 2:1 fora de estação + sementes de lua na loja**.

**Eiran (animais)** — *"bicho sente antes da gente"*
sq_eiran_1 **Lost Hen** (5): achar a galinha fujona (está perto da entrada da caverna —
presságio). sq_eiran_2 **Wolf at the Fence** (16): 5 Grimfangs. sq_eiran_3 **Prize Goat**
(28): 5 rações premium → **pensão de animais + filhote raro à venda**.

**Gruta (taverna)** — *"barriga cheia, língua solta"*
sq_gruta_1 **Empty Barrels** (7): 5 grape. sq_gruta_2 **Recipe Hunt** (20): 1 mirrorfin
("dizem que faz o copo brilhar"). sq_gruta_3 **Feast for All** (35): banquete do festival
→ **2 pratos de buff/dia**.

**Dagna (pedreira)** — *"toda pedra tem veio; é só ouvir"*
sq_dagna_1 **Pretty Rocks** (6): 8 stone. sq_dagna_2 **The Singing Vein** (22): minerar o
veio que "canta" no nível 15. sq_dagna_3 **Cold Vein** (38): 1 glacier_hide → **mapa do
veio 2×/run**.

**Zrix (estrada da caverna)** — *"desça devagar, suba inteiro"*
sq_zrix_1 **Trail Markers** (8): marcar 3 níveis. sq_zrix_2 **Lost Patrol** (25): achar o
kit do batedor desaparecido (e o que sobrou dele). sq_zrix_3 **Seven Back** (45): resgatar
um NPC vivo no nível 40 → **resgate com desconto permanente**.

**Hund (guarda)** — *"ordem é rotina bem feita"*
sq_hund_1 **Night Rounds** (6): acompanhar a ronda das 22h. sq_hund_2 **The Footprints**
(18): investigar as pegadas que somem (gancho do Maelor). sq_hund_3 **Warbanner** (50):
o orc_warbanner do Packlord Ruvash → **contratos do quadro pagam +20%**.

**Mirela (atelier)** — *"a costura conta a história do rasgo"*
sq_mirela_1 **Torn Cloak** (5): 5 fiber. sq_mirela_2 **Silk of the Deep** (24): 3
veil_cloth. sq_mirela_3 **The Midnight Dress** (40): a encomenda misteriosa da Liora →
**2º upgrade de mochila**.

**Tovin (permissões)** — *"o carimbo protege quem carimba"*
sq_tovin_1 **Stamped Twice** (6): entregar formulários. sq_tovin_2 **The Missing Permit**
(20): rastrear um alvará sumido (leva à Yael — e fica por isso mesmo, por enquanto).
sq_tovin_3 **Charter of Hope** (35): colher 5 assinaturas → **alvarás de lote da fazenda**.

**Corvus (templo)** — *"a Fonte não esqueceu; nós esquecemos dela"*
sq_corvus_1 **Candles for the Quiet** (5): 5 velas (craft). sq_corvus_2 **The Doubting
Father** (25): um diálogo longo com ESCOLHA sobre fé (flag social). sq_corvus_3 **Litany
Fragment** (45): achar a parte do templo da Litania → **bênção com 2 opções/dia** (e liga o Ato 4).

---

# PARTE D — Quadro de Avisos (dailies procedurais)

## 10. Os seis moldes

3 contratos/dia, montados destes templates com alvo/quantidade pelo nível do jogador:

```text
bd_cull_<family>    — "Caçada: abater 8-15 criaturas da família X na banda adequada."
bd_gather_<mat>     — "Coleta: 10-20 do material X."
bd_delivery         — "Entrega: levar pacote ao NPC Y antes das 20h."
bd_escort_supply    — "Suprimento: levar o caixote ao posto do Zrix."
bd_harvest          — "Encomenda: vender N crops no shipping hoje."
bd_repair           — "Mutirão: doar N materiais à obra do Gurd."
Regras: nunca pedem item de quest; nunca apontam criaturas não-agressivas; recompensa
Daily escalada; 1 contrato concluído/dia conta para a amizade futura.
```

---

# PARTE E — Contratos de Caverna (8) e Secretas (8)

## 11. Quadro do Zrix

```text
cc_depth_5 / _15 / _30 / _50 / _70 / _90 — "First to depth N" (marcos 1×; XP alto + mapa do trecho).
cc_boss_rematch (semanal) — re-derrotar um boss de gate: essência garantida.
cc_no_hit_floor (semanal) — limpar 1 nível sem tomar dano: título + charm.
```

## 12. As secretas (decisão Q6.1 — a caverna também pede)

```text
scq_scrounger_bargain — Old Scrounger King: 3 itens Rare+ → anel + ele vira VENDEDOR fixo
  do nível. "Tudo tem preço. Eu sou a prova."
scq_merchant_list_1/2/3 — o mercador errante pede 5 glowcap / 3 frost_core / 1 wyrmling_scale
  → desconto permanente de 10% + item raro (1× por run cada, estável por seed).
scq_warden_offering — oferecer Água Viva ao Silence Warden → pacífico para sempre +
  nymirian_engraving (alimenta o Ato 3 sem ser obrigatória).
scq_goblin_truce — aceitar o Duelo ritual do Warchief e VENCER sem que o pack morra →
  banda neutra por 1 run + warchief_crest (o goblin da fazenda menciona seu nome).
scq_thrall_name — um Thrall purificado sussurra um nome → levar à Mara (lore do Ato 3).
scq_dragon_egg — o ovo frio no ninho da Ashwing → chocar na incubadora da Nimble
  (cosmético no v1; gancho do sistema de pets futuro).
```

---

# PARTE F — Quests de Festival (8 — uma por festival)

```text
fq_plantio (Primavera d7) — concurso de plantio de Thandra.
fq_caravana (P d21) — escoltar a carga de Finan/Merithus pela estrada (combate leve).
fq_luas (Verão d14) — coletar 3 ecos, um sob cada lua, na mesma noite.
fq_torneio (V d28) — 3 duelos na arena do portão (Alaric arbitra; Kaand aprova).
fq_colheita (Outono d7) — o maior crop do vale (qualidade conta!).
fq_veus (O d21) — no mercado noturno expandido, achar o item "que não existe".
fq_vigilia (Inverno d14) — a vigília de Anya na Fonte (sem combate; lore pesada).
fq_anonovo (I d28) — entregar presentes a 5 NPCs antes da meia-noite.
```

---

# PARTE G — Conexões e UI

## 13. Regras de conexão (decisão Q6.4)

```text
1. A q3 de toda cadeia exige um MARCO (miniboss/banda/ato) — a side empurra para a caverna.
2. Gating cruzado: Têmpera ← Brumdar-3 + Ato 1 · Scrolls ← Ozzra-3 · Lotes ← Tovin-3.
3. Toda side ganha 1 linha de diálogo que referencia o ato corrente (flags por ato).
4. As secretas ALIMENTAM a main (engraving, o nome do thrall) sem nunca serem obrigatórias.
5. Dailies/contratos jamais usam itens de quest ou alvos não-agressivos.
6. Total v1: 20 main + 36 side + 6 moldes daily + 8 contratos + 8 secretas + 8 festivais ≈ 86.
```

## 14. UI (decisão Q6.5)

```text
Log (J): abas Main / Side / Contratos / Secretas (só descobertas) — padrão F14.
Tracker no HUD: 1 quest, 2 linhas, colapsável.
Marcadores: "!" dourado (main), prateado (side), pergaminho (quadro).
TODAS as caixas de diálogo e painéis: ancoragem responsiva em % da tela (emenda F14).
```

---

# PARTE H — Decisões fechadas

```text
5 fontes de quest, incluindo secretas entregues por criaturas e mercadores errantes.
XP/ouro escalam por QuestLevel (fórmula no BALANCE_CURVES §7); nada é estático.
Main: +1 skill point por ato; nunca expira; sem evento raro sem pista.
12 cadeias de NPC no v1 (36 quests), cada uma destravando o serviço único do NPC.
Escolhas com flag que retornam: o infiltrado do Ato 3; a fé do Corvus.
Thralls purificados voltam como aldeões — o mundo reage.
```

# PARTE I — Pendências

```text
2ª leva de cadeias (11 NPCs, 33 quests) — v2, mesmos moldes.
Textos finais de diálogo de cada quest (gerados na execução das specs, PT-BR).
Recompensas únicas item-a-item das q3 (validar contra o catálogo de itens).
Daily de pesca e daily de cozinha — avaliar adição após telemetria do quadro.
```
