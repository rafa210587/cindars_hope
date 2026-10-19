# FABLE — Decisões e Respostas v2.0 (Refinamento v2)

> **Status:** VINCULANTE — respostas do dono do projeto ao questionário
> `FABLE_REFINAMENTO_V2_PERGUNTAS.md` (2026-06-12). Complementa (não substitui) o
> `FABLE_DECISOES_RESPOSTAS_v1.0.md`. Em conflito, este documento vence por ser mais novo.
> Seção de LORE (3.1/3.3/4.1/3.4-detalhe) será anexada após consolidação em andamento.

---

## BLOCO 1 — Identidade do jogador

| # | Decisão |
|---|---------|
| 1.1 | **A** — Jogador escolhe NOME (campo de texto no New Game). |
| 1.2 | **B** — Escolha de apresentação COM opção neutra (M/F/Neutro). Regra do NPC nymiriano para jogador neutro: *proposta registrada* — o nymiriano se manifesta com apresentação ambígua/etérea (coerente com a natureza da raça); confirmar na Fase 0 da fable_46. |
| 1.3 | **B** — Tints placeholder simples (cor de cabelo/pele) na criação, pré-arte. |

**Efeito:** emenda fable_56 (criação de personagem: nome + apresentação + tints) e fable_46 (regra do nymiriano).

## BLOCO 2 — Kit inicial do New Game

| # | Decisão |
|---|---------|
| 2.1 | **APROVADO** o kit canônico: enxada + regador + machado + picareta (tier básico, sem foice); espada velha (tier 0 gasta, BV baixo); 12 sementes de cenoura, 3 pães, 1 poção de vida pequena; **150g**. |
| 2.2 | **B** — A primeira ARMA vem de um **baú narrativo na fazenda** ("do antigo dono"), não no inventário. |

**Efeito:** emendas fable_63 (baú narrativo na intro) e fable_66 (kit canônico substitui o debug loadout; destrava remoção do slice mode).

## BLOCO 3 — Lore

| # | Decisão |
|---|---------|
| 3.1 | Nymirianos: **buscar/consolidar da lore existente** + proposta (consolidação em andamento — será anexada como APÊNDICE LORE). |
| 3.2 | **Cindar FUNDOU a cidade** (confirmado pelo dono; citações dos refinamentos no apêndice). |
| 3.3 | Raiz/Árvore de Mana: dono pediu **proposta inspirada na lore** (apêndice). |
| 3.4 | **Sem outros templos** na cidade além de Kanthor (templo) e Thandra (culto rural). **NOVO CONTEÚDO APROVADO:** altares dos deuses espalhados — ex.: **altar de Finan nas cavernas com bônus** — e "elementos dos deuses" presentes na cidade, fazenda e caverna, coerentes com o panteão. → **vira spec fable_68 (Marcas dos Deuses)**. |
| 3.5 | **A** — Primeira morte é **orgânica** (sem evento scriptado). |
| 3.6 | **Vaelrion chega no Ato 2, após o primeiro boss.** Sethra↔Yael são **rivais**. Corvus **pode ser quest giver recorrente** além do Ato 1. |

**Efeito:** insumos para fable_36/43/63 (textos) e fable_68 (nova).

## BLOCO 4 — Os 3 finais

| # | Decisão |
|---|---------|
| 4.1 | USAR a Fonte: **consolidar o mapeamento existente** ("depende do que já liberou de Anya — cura, respec, buffs, purificações"; apêndice lore). |
| 4.2 | SELAR a Fonte: **epílogo com eventos de ataque** — criaturas poderosas atacam a fazenda e ameaçam destruir a cidade (raids pós-final). |
| 4.3 | PROTEGER a Fonte: **a cidade prospera visivelmente** (final de prosperidade). |
| 4.4 | **APROVADA** a tabela de recompensa única por gate: 15 Cobre Temperado · 30 planta de baú de corpse melhorado · 45 Aço Profundo · 60 Mithril Work · 75 Bromeciana · 90 Pedra Negra · 100 acesso ao 101 + Meteórica. |
| 4.5 | **B** — Nível 101 é uma **CaveScene especial** (não cena/arena dedicada). |

**Efeito:** emendas fable_43 (epílogos Selar/Proteger, 101 como CaveScene especial, tabela de gates) e fable_49 (receitas por gate).

## BLOCO 5 — Fazenda

| # | Decisão |
|---|---------|
| 5.1 | **A** — Animais têm **morte permanente** por negligência prolongada. |
| 5.2 | **B** — A Fonte **começa DORMENTE** e desperta na 1ª morte/quest. (F17 executada com Fonte ativa → ajuste registrado: estado inicial Dormant; despertar = beat da fable_63/primeira morte.) |
| 5.3 | Fazenda nível máximo: **apenas dinheiro e recursos** (SEM gate de caverna). |
| 5.4 | **B** — Fruto Mana é **vendável a preço ALTÍSSIMO**, dificílimo de produzir. |
| 5.5 | **S/S/S** — Gato dá bônus lunar: SIM · companion pode gastar fertilizante raro automaticamente: SIM · chuva pode falhar em evento de seca: SIM. |
| 5.6 | **A** — Mirrorfin/Lake Lurker no lago da FAZENDA: raríssimo (0,5%, só à noite). |

**Efeito:** emendas fable_12 (morte permanente; gato lunar; companion+fertilizante anotado p/ wave companions), fable_41 (nível máx sem gate), fable_50 (peixes raros na fazenda), fable_37 (evento de seca), fable_55/farm (Fruto Mana preço altíssimo — entra no catálogo F32).

## BLOCO 6 — Mundo/UI

| # | Decisão |
|---|---------|
| 6.1 | **A** — Nomes próprios de Vaalara: estações **Semeio / Brasa / Véu / Gelo**; dias D1-D7 sem nome. |
| 6.2 | **B** — Loja noturna só em **pico de Nyx + quest**. |
| 6.3 | **A** — Calendário mostra as 3 luas **desde o início**. |
| 6.4 | **A** — Lojas fechadas: **porta bloqueada** com aviso de horário. |
| 6.5 | **A** — **Sprint em combate APROVADO** (manter 3.8-4.2 tiles/s em combate a 8 stamina/s) → **vira spec fable_69**. |
| 6.6 | **A/A** — Tier 5 do v1: **Eco de Anya + Ruptura de Senya**; Projétil Arcano **auto-target**. |
| 6.7 | **B/A** — Recompensa por entrada FullyDocumented do bestiário: **MECÂNICA (+stats)**; bosses revelam a ficha **após derrota**. |

**Efeito:** emendas fable_20 (nomes de estações), fable_37 (loja noturna por Nyx), fable_11 (portas bloqueadas), fable_08 (Tier 5/auto-target), fable_21+45 (recompensa mecânica). Nova: fable_69 (sprint).

## BLOCO 7 — Não-funcionais / shipping

| # | Decisão |
|---|---------|
| 7.1 | Identidade do produto: *sem resposta* — defaults provisórios: productName **"Cindar's Hope"**, versão **0.1.0**, companyName a confirmar (Fase 0 da fable_61). |
| 7.2 | **A** — 1920×1080/16:9, fullscreen com windowed opcional. |
| 7.3 | **A** — Build standalone a cada checkpoint M1-M4; edição de EditorBuildSettings/ProjectSettings via editor script autorizada (confirmação pontual via permissions.ask a cada execução). |
| 7.4 | **A + B** — Backup rolling do último save bom a CADA gravação: SIM · Slots: **3 slots manuais** (não slot único). Fecha a pendência SAVE_LOAD §20. |
| 7.5 | **A** — Object pooling aguarda telemetria (F59). |
| 7.6 | **"Faça tudo"** — **MVP = LOTE INTEIRO (E01-E67+)**. Não há corte curto; o jogo v1 completo é o alvo. V1_SCOPE = a própria enumeração do 00C. |
| 7.7 | **A com abas / A** — Journal unificado SIM, **em forma de abas** (painel único F14 ganha aba **Social**) · Inventário **com busca textual**. |

**Efeito:** emendas fable_56 (3 slots + backup + identidade), fable_61 (identidade/política), fable_14 (aba Social = 9 abas + busca no inventário).

## BLOCO 8 — Adiáveis

| # | Decisão |
|---|---------|
| 8.1/8.2 | Sem resposta — permanecem futuros (companions/social v2). |
| 8.3 | **"Já gere"** — gerar JÁ a 2ª leva de cadeias side (11 NPCs restantes, ~33 quests) → **vira spec fable_70**. |

---

## Novas specs decorrentes deste documento

| Spec | Origem | Conteúdo |
|---|---|---|
| fable_68 | 3.4 | Marcas dos Deuses: altares com bônus (Finan na caverna etc.) + elementos do panteão em cidade/fazenda/caverna |
| fable_69 | 6.5 | Sprint em combate (3.8-4.2 tiles/s, 8 stamina/s) |
| fable_70 | 8.3 | 2ª leva de cadeias side: 11 NPCs restantes, ~33 quests (moldes da F35) |

## Ambiguidades interpretadas (para veto rápido se discordar)

1. **4.5 = B** (CaveScene especial) — o "SIM" da resposta veio colado à opção B, seguindo o padrão das demais respostas. Se a intenção era A (arena dedicada), avisar.
2. **1.2 neutro × nymiriano** — proposta: nymiriano de apresentação ambígua/etérea para jogador neutro (a confirmar na fable_46 Fase 0).
3. **7.7 "A mas com abas"** — interpretado como: o Journal unificado É o painel de abas (F14), ganhando a aba **Social**; não é um menu separado.
4. **4.4** — sem marcação explícita; registrado como APROVADO (tabela ★ sem objeção).

*Registrado em 2026-06-12. Apêndice de lore (3.1/3.2/3.3/4.1/3.4-catálogo) anexado após consolidação.*


---

# APÊNDICE LORE (consolidação 2026-06-12 — canon encontrado + propostas PENDENTES DE APROVAÇÃO)

> Itens 3.1/3.2/3.3/4.1/3.4 consolidados de: VAALARA_GAME_CANON, QUESTS_MAIN_LORE,
> QUESTS_MAIN_PROGRESSION, QUESTS_LORE_WEAVING_BRIEF, QUEST_CATALOG, CAVE_DESIGN,
> CAVE_BESTIARY_CATALOG, FARM_DESIGN Parte L, CITY_DESIGN §5-8 e código
> (FonteFunctionUnlockService/FinalChoiceService/EndgameContracts).
> [CANON] = já estava escrito. [PROPOSTA] = aguarda seu OK (vale como default até veto).

## A.1 Nymirianos (3.1)

[CANON] Povo antigo de Anya (águas/cura/esperança/memória); Cindar era a última sacerdotisa
nymiriana local; a Fonte tem artefato nymiriano com marcações tribais; 1 remanescente vivo =
o NPC do Ato 3 (romanceável); Liora tem sangue nymiriano distante; Silence Warden (90-100) é
construct nymiriano com runas tribais; banda 86-101 tem corredores nymirianos; nível 101 =
santuário nymiriano sob o Arco da Memória.

[PROPOSTA] (a) Aparência — "povo das marcas d'água": pele com iridescência perolada, cabelos
azul-acinzentado a branco-espuma, olhos claros; MARCAS TRIBAIS FLUIDAS na pele (as mesmas do
artefato da Fonte e do Warden) que brilham perto de Água Viva/Fonte/lagos. (b) Remanescentes:
APENAS o NPC do Ato 3 vivo puro (povo minguou pós-Cataclismo); linhagens diluídas sem marcas
(precedente: Liora); NUNCA vila escondida. (c) Ruínas visitáveis SIM em 3 camadas: santuários
d'água como special rooms raras (bandas 26-40 congelado; 71-85 profanado), corredores
nymirianos formalizados na 86-101 com o Warden, e o nível 101 = santuário íntegro
(progressão visual ruína→corrupção→origem).

## A.2 Cindar fundou a cidade (3.2 — CONFIRMADO)

[CANON] QUESTS_MAIN_LORE §3: "fundadora mítica de Cindar's Hope"; sobreviventes se reuniram
ao redor da Fonte que ela protegia. Verdades em camadas mantidas: fundadora ✔, última
nymiriana ✔, guardiã dos fragmentos ✔, e a mulher que SELOU algo perigoso sob a cidade.
Eco jogável: boss_cindrathel (Eco de Cindar corrompido, espelha a build do jogador).
Formulação final p/ textos (F36/43/63): "Hope" é o nome da promessa que ela deixou.

## A.3 Raiz/Árvore de Mana (3.3 — PROPOSTA)

[CANON] Fruto Mana raro/sagrado/politicamente perigoso; Árvore consciente (escolhe onde
crescer); Raiz Dormente na fase 5 da fazenda (área arcana da Fonte; ManaRootState já
previsto); nível 101 contém "raiz/estrutura de Mana"; ManaBloomPolicy por final já codificada.

[PROPOSTA] A estrutura do 101 é a RAIZ PRIMEVA: raiz-mãe de uma Árvore ancestral que cresceu
sobre o santuário nymiriano, entrelaçada ao Arco e parcialmente enjaulada pela máquina
bromeciana (a prova física do erro de Bromécia); Ithryndor dorme sob ela. Raízes secundárias
atravessam as bandas como "veios que cantam" (a sq_dagna_2 já é raiz mineralizada). O rebento
único na superfície é a Raiz Dormente da fazenda — a árvore escolheu o lugar por causa da Fonte.
DESPERTAR: Fragmento da VIDA integrado (Fonte → LifeBlooming) faz a Raiz Dormente GERMINAR;
a FLORAÇÃO (1º fruto) só após ver a Raiz Primeva no 101 + escolha final; o ritmo pós-final
segue a ManaBloomPolicy do ending. ECONOMIA (5.4-B): 1 fruto/floração, máx 1 floração/estação,
BaseValue ~5.000g, venda SÓ por canal especial (caravana de Finan/mercador errante — nunca
shipping comum; vender é um EVENTO com reação de NPCs). Drift documental a normalizar na F36:
QUEST_CATALOG cita "gate 70" → lista canônica é 75.

## A.4 USAR a Fonte (4.1 — mapeamento consolidado + regra proposta)

[CANON codificado] Liberação de Anya por fragmento: Despertar→ReturnPoint · Água→Água Viva
(3 cargas)+Purificação menor · Memória→Respec (cooldown) · Vida→Purificação avançada ·
Esperança→FinalChoicePreparation. Final USAR: FonteState=FinalizedUsed, Mana MorePredictable,
cidade MaterialProgress, tech Expanded, corrupção RiskOfRepeat; Anya NUNCA é restaurada;
Ithryndor responde com a última luta.

[PROPOSTA — regra que fecha "depende do que ele já liberou"] USAR não concede nada novo:
AMPLIFICA somente o que já foi liberado (o que não foi, perde-se para sempre):
ReturnPoint→retorno sem perda de corpse 1×/dia · Água Viva→6 cargas, recarga previsível ·
Respec→sem cooldown/custo · Purif. menor→Pedra Negra Estabilizada p/ crafting (gates 90/100) ·
Purif. avançada→"Bênção do Arco" (1 buff ≤5% por dia à escolha). Custo narrativo: a Fonte
vira ferramenta — Corvus se afasta, o nymiriano entristece (gate de romance), eventos raros
de corrupção residual pós-game (RiskOfRepeat materializado).

## A.5 Marcas dos Deuses (3.4 — catálogo p/ fable_68)

[CANON] Templo só de Kanthor + culto rural de Thandra; ANYA NUNCA TEM ALTAR (só a Fonte —
regra inviolável); caverna já reserva "altares quebrados/marcas de deuses" como special
rooms; relíquias F23 existem (kanthor/kaand/anya/alihana).

[PROPOSTA — 11 Marcas; orar = 1/dia, efeito ≤5% até dormir, NÃO stacka com relíquia do mesmo deus]
CAVERNA: 1. Moeda Enterrada (Finan, qualquer banda raro): +5% gold find ·
2. Bigorna Fria (Thoren, 41-55): −5% perda de durabilidade · 3. Pedra do Desafio (Kaand,
41-55/71-85): +3% dano causado E recebido · 4. Poço Sem Lua (Nyx, 71-85, só à noite):
+5% chance de itens secretos · 5. Raiz Trançada (Tandra, 11-25): +5% drops de Beast.
CIDADE/FAZENDA: 6. Nicho da Colheita (Thandra, fazenda): oferenda de crop → +2% Silver
amanhã · 7. Pedra de Juramento (Kanthor, adro do templo): +2% block stability ·
8. Selo de Merithus (cartório/caixa de envio): +2% valor do shipping do dia ·
9. Espelho de Alihana (lago da fazenda, noite de Alihana): sonho-pista + +5% semente rara ·
10. Mastro de Senya (praça, só festival/pico): +5% XP de magia.
EXCEÇÃO: 11. As Três Águas (Anya — Jardim das Estátuas + Fonte + salas Anya Echo): SEM bônus
de oração; só lore + a cura limitada já canônica. "Anya não tem fiéis: tem guardiões."