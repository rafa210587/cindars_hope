# SPEC — Quests: 2ª Leva de Cadeias de Side Quest por NPC (11 NPCs restantes)

> **Spec ID:** `fable_70_spec_npc_side_quest_chains_wave2`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P3
> **Type:** Content / Integration
> **Domain:** Quests / NPC
> **Parallelizable:** NO (QuestRegistry lock — roda DEPOIS de F35, na janela P4)
> **Parallel group:** N/A (lock da cadeia quest)
> **Can run with:** N/A
> **Must not run with:** F34, F35, F10, F36, F51, F52, F53 (cadeia QuestRegistry/geradores de quest)
> **Repo lock scope:** geradores de quest, QuestRegistry data, TownNpcDialogueLibrary (ofertas)
> **Depends on:**
> - F35 (1ª leva validada — gerador, gramática de gates/recompensas e ofertas no diálogo prontos)
> - F34 (fonte Npc), F26 (gates de amizade), F32 (itens de recompensa)
> **Blocks:** N/A (flags sq_* desta leva são lidas só por diálogo F28 — nenhuma spec depende delas)
> **Scope:** as 11 cadeias da 2ª leva do QUEST_CATALOG (3 quests cada, ~33 quests) como dados+flags.
> **Out of scope:** cutscenes, romance, quests que exijam sistemas inexistentes (adaptar OU dormante documentado), revelação do culto de Nyx (Ato 3 — F36 é dona).

required_adrs: []
required_game_rules: []

> Nota: `quest_rules.md`/`npc_rules.md` ainda não existem em `docs/game_rules/` (conferido por
> Glob nesta geração). Se forem criados antes da execução (cadeia de governança F57/F67),
> esta spec herda exatamente os mesmos required_game_rules da F35.

---

# /speckit.specify

## Contexto

A decisão 8.3 do Refinamento v2 (`FABLE_DECISOES_RESPOSTAS_v2.0.md`: **"Já gere"**) aprovou
gerar JÁ a 2ª leva de cadeias side declarada como pendência no QUEST_CATALOG (Parte I):
os 11 NPCs do roster v1.1 que ficaram fora da 1ª leva (F35), ~33 quests (3 por NPC), com
os MESMOS moldes (catálogo §8). O roster tem 23 NPCs; a F35 cobriu Brumdar, Ozzra,
Thalindra, Sylveth, Eiran, Gruta, Dagna, Zrix, Hund, Mirela, Tovin e Corvus. Restam:
**Mara, Nimble, Gurd, Yael, Pip, Alaric, Renko, Liora, Orlan, Savra e Maelor**.

A regra das cadeias é a mesma da F35 (catálogo §8): q1 doméstica (apresenta o NPC) → q2
que toca o mundo (caverna/cidade/lua) → q3 com marco (miniboss/banda/ato) e recompensa
permanente — a q3 seta a flag de serviço do NPC. Duas decisões NOVAS do v2 cruzam esta
leva (3.6): **Vaelrion chega no Ato 2, após o primeiro boss** (nenhuma quest pode citá-lo
antes) e **Sethra↔Yael são RIVAIS** — a cadeia da Yael DEVE expressar a rivalidade
comercial com a dona d'A Vela Sem Chama, com escolhas que o jogador pode mediar, SEM
revelar o culto (revelação é do Ato 3, F36).

## Problema

Sem a 2ª leva, metade do roster (11 de 23 NPCs) não tem propósito jogável: amizade sem
payoff, serviços únicos futuros sem flag de destrave e o catálogo declara a pendência em
aberto. A F35 deixa gerador, gramática de gates e fluxo de oferta prontos — adiar a 2ª
leva é desperdiçar a janela em que o padrão está quente e auditável. Se os objetivos do
roster forem implementados sem a matriz de viabilidade, o risco é o mesmo da F35: corte
silencioso de quests ou criação de tipos de objetivo paralelos.

## Objetivo

Ao final desta spec, o projeto deve ter as 11 cadeias da 2ª leva geradas como
QuestDefinitions encadeadas por flag (IDs `sq_<npc>_<n>`), ofertadas no diálogo do NPC
quando elegíveis (flag anterior + amizade mínima + ato quando citado), com recompensas
(gold/item/amizade +8/flag de serviço) e textos na voz do NPC (derivada do roster v1.1),
refletindo as decisões 3.6 do v2 (Vaelrion gated por Ato 2; rivalidade Sethra↔Yael
mediável), sem criar tipos de objetivo novos nem segundo fluxo de oferta.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md (§8 moldes + Parte I pendência)
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md (propósito/voz/quests dos 11)
docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md (8.3 "Já gere"; 3.6 Vaelrion/Sethra↔Yael)
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md (§14-15 — Sethra/Yael, só para coerência)
.specs/a_implementar/fable/fable_35_spec_npc_side_quest_chains.md (gramática-irmã)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar (tudo entregue/validado pela cadeia F26/F28/F34/F35):
- gerador GenerateNpcQuestChains (F35) — esta spec ESTENDE a tabela dele, não cria gerador novo;
- F34 (fonte Npc/gates de oferta) — toda oferta de side passa por ela;
- tipos de objetivo de quest (kill/collect/deliver/talk/flag — WAVE 09/26);
- FriendshipService (F26) — fonte única de amizade/gates sociais;
- QuestFlagService — fonte única de flags;
- TownNpcDialogueLibrary + DialogueCondition (F28: RequiredFlag/MinFriendship);
- 12 cadeias da 1ª leva (sq_brumdar_* ... sq_corvus_*) — intocáveis nesta spec.
Não existe:
- as 11 cadeias da 2ª leva (nenhuma QuestDefinition sq_mara_*, sq_nimble_*, ...);
- ofertas "[!] Quest" no diálogo dos 11 NPCs restantes;
- flags de serviço da 2ª leva (sq_<npc>_3_done).
Auditar Fase 0:
- matriz de viabilidade: tipos de objetivo existentes × objetivos do roster v1.1
  (cada objetivo mapeado a um tipo existente, adaptado ou dormante);
- serviços F25 existentes × flags de q3: dos 11, só a Yael tem serviço F25 declarado
  (Encomenda de Livro) — a flag sq_yael_3_done vira o questFlag desse serviço; as demais
  flags ficam RESERVADAS (dormentes documentadas) para a leva v2 de serviços únicos;
- IDs reais de itens de recompensa (F32) citados pelos esboços abaixo;
- flags de ato disponíveis (act_1_done/act_2_done) — gates de ato só LEEM essas flags.
```

## As 11 cadeias da 2ª leva (derivadas do roster v1.1)

| NPC | Tema/propósito da cadeia | Quests | Gate base |
|---|---|---|---|
| npc_mara | cartório: civilização é o que se assina — licenças, registros roubados e um caso moral | 3 | amizade 1/2/3 |
| npc_nimble | carpintaria: construir, medir e MOVER — culmina na madeira bromeciana | 3 | amizade 1/2/3 |
| npc_gurd | obra pesada: força sem fúria — obstáculos, a parede antiga e a briga contida | 3 | amizade 1/2/3; q3 ato 1 |
| npc_yael | loja noturna: rivalidade comercial com Sethra (A Vela Sem Chama) que o jogador pode mediar | 3 | amizade 1/2/3; q3 ato 2 |
| npc_pip | entregas: o mensageiro que vê tudo — recados, o fantasma do Jardim e a carta errada | 3 | amizade 1/2/2 |
| npc_alaric | guarda: patrulha e responsabilidade — estrada baixa, relatório moral e o marco de caça | 3 | amizade 1/2/3; q3 ato 1 |
| npc_renko | loja geral: barganha com três versões — mercadoria sem dono e a mentira verdadeira | 3 | amizade 1/2/4 |
| npc_liora | música/sonhos: a melodia que ninguém ensinou — estátuas, eco da caverna e Água Viva | 3 | amizade 1/2/3; q3 ato 1 |
| npc_orlan | hospedaria: quem chegou, quem partiu, quem mentiu — o hóspede sem sombra | 3 | amizade 1/2/3 |
| npc_savra | ervas/antídotos: veneno é planta mal compreendida — pragas noturnas e o fungo de baixo | 3 | amizade 1/2/3 |
| npc_maelor | Nyx/memória: a cidade esqueceu de propósito — cadeia tardia ligada ao arco da memória | 3 | amizade 2/3/4; q2+ ato 2 |

## Esboço por cadeia (mesma gramática da F35: ID Nome (QuestLevel): objetivo → recompensa)

```text
Mara (cartório) — "civilização é aquilo que pode ser assinado e cobrado"
sq_mara_1 Licença de Primeira Obra (6): deliver — registrar a 1ª construção da fazenda
  (formulário ao cartório) → +8 amizade, gold escalado, construção formal liberada (flag).
sq_mara_2 As Páginas Arrancadas (18): collect — recuperar 3 registros removidos do cartório
  (ruína urbana rasa) → +8 amizade, gold, flag de segredo urbano (pista, NÃO revela culto).
sq_mara_3 Lei ou Compaixão (35): talk com ESCOLHA — o caso da família sem licença
  (multar/perdoar; flag social que retorna em diálogo) → +8 amizade, item com o cheiro
  dela (selo de Merithus), sq_mara_3_done (flag de serviço reservada: registro preferencial).

Nimble (carpintaria) — "medir duas vezes, mover uma"
sq_nimble_1 Madeira, Pedra e Assinatura (5): collect — 10 wood + 5 stone para a oficina
  → +8 amizade, gold escalado.
sq_nimble_2 A Tábua que Cura (20): collect — 1 amostra de madeira auto-reparável
  (caverna, banda 1) → +8 amizade, gold, pista tecnológica (Bromécia).
sq_nimble_3 Mover é Mais Difícil que Erguer (32): talk/flag — concluir com ele a 1ª
  movimentação de estrutura → +8 amizade, item (nível de bolso do Nimble),
  sq_nimble_3_done (reservada: mover construção com desconto).

Gurd (obra pesada) — "carregar, quebrar ou encarar — nessa ordem"
sq_gurd_1 Pedra Grande, Martelo Maior (7): collect — 8 stone dos obstáculos da fazenda
  → +8 amizade, gold escalado.
sq_gurd_2 A Parede que Não Quebrou (22): talk/flag — investigar a parede antiga que
  resistiu à britadeira dele (pista de ruína) → +8 amizade, gold.
sq_gurd_3 Força sem Fúria (35) [gate: ato 1]: talk com ESCOLHA — conter a briga na
  taverna sem machucar ninguém (intimidar/apartar/deixar) → +8 amizade, item (luva de
  obra do clã), sq_gurd_3_done (reservada: mutirão de limpeza pesada).

Yael (loja noturna) — "o que não existe não deixa recibo" [DECISÃO 3.6: rival de Sethra]
sq_yael_1 Aberto Depois da Meia-Noite (8): talk noturno — encontrar a loja da Yael
  após meia-noite (flag noturna) → +8 amizade, gold escalado, acesso ao estoque base.
sq_yael_2 Comprador de Fragmentos (26): collect/flag — levantar 3 pistas sobre quem
  compra pedras escuras na cidade; a trilha cruza A Vela Sem Chama e expõe a RIVALIDADE
  comercial Sethra↔Yael (duas lojas noturnas, dois códigos) → +8 amizade, gold, flag
  sq_yael_rival_known (pista de culto NÃO revelada — alimenta o Ato 3 sem ser obrigatória).
sq_yael_3 Preço do Silêncio (45) [gate: ato 2]: talk com ESCOLHA espelhada — mediar a
  rivalidade (favor Yael / favor Sethra / mediar — flag sq_yael_rival_choice) sem expor o
  segredo de nenhuma → +8 amizade, item raro noturno, sq_yael_3_done → questFlag do
  serviço F25 "Encomenda de Livro".

Pip (entregas) — "atalho é estrada que ainda não cresceu"
sq_pip_1 Primeira Entrega (4): deliver — levar o pacote do Pip à loja de sementes
  → +8 amizade, gold escalado (tutorial social).
sq_pip_2 Vi o Fantasma (14): talk noturno/flag — conferir com ele a figura no Jardim
  das Estátuas à noite → +8 amizade, gold, pista noturna (Anya, leve).
sq_pip_3 A Carta Errada (25): deliver com ESCOLHA — a carta entregue errada revela uma
  tensão entre dois NPCs; devolver lacrada ou entregar aberta (flag social) → +8 amizade,
  item (mochila remendada do Pip), sq_pip_3_done (reservada: recados expressos).

Alaric (guarda) — "patrulha, lâmina e responsabilidade"
sq_alaric_1 Patrulha da Estrada Baixa (8): kill — 6 criaturas perto da entrada da
  caverna → +8 amizade, gold escalado, reputação com a guarda.
sq_alaric_2 Relatório Incompleto (24): talk com ESCOLHA — encobrir ou reportar o
  incidente para evitar pânico (flag moral que retorna) → +8 amizade, gold.
sq_alaric_3 A Justiça Não Basta (40) [gate: ato 1]: kill/talk — abater o miniboss da
  banda 2 apontado pela guarda E decidir o destino do suspeito inocente → +8 amizade,
  item (escudo de patrulha), sq_alaric_3_done (reservada: patrulha estendida na fazenda).

Renko (loja geral) — "preço fixo é uma ofensa criativa"
sq_renko_1 Preço de Amigo (6): deliver — entregar 3 encomendas atrasadas dele pela
  cidade → +8 amizade, gold escalado, desconto pequeno.
sq_renko_2 Mercadoria Sem Dono (20): flag/talk com ESCOLHA — decidir o destino do item
  perigoso que chegou sem remetente (vender/entregar à guarda/devolver) → +8 amizade,
  gold, pista de Pedra Negra (NÃO revela culto).
sq_renko_3 Três Sorrisos, Uma Mentira (38): talk com ESCOLHA — descobrir qual das três
  versões do Renko é real (amizade 4 exigida) → +8 amizade, item raro do estoque,
  sq_renko_3_done (reservada: estoque raro rotativo).

Liora (música/sonhos) — "algumas canções lembram por nós"
sq_liora_1 Canção sem Autor (7): talk — perguntar a 3 NPCs sobre a melodia que ela
  canta sem ter aprendido → +8 amizade, gold escalado.
sq_liora_2 A Estátua que Escutou (25): flag — acompanhá-la ao Jardim das Estátuas à
  noite e presenciar a reação da estátua → +8 amizade, gold, pista de Anya.
sq_liora_3 Sonho de Água Clara (42) [gate: ato 1]: flag — descer com o mapa do sonho
  dela até o eco na banda 2 da caverna (marco de profundidade) → +8 amizade, item
  (partitura de Alihana), pista de Água Viva, sq_liora_3_done (reservada: canção de
  descanso na taverna). Coerência: NÃO duplica mq_act2_03 (escolta da main é da F36);
  esta q3 é só a pista pessoal dela.

Orlan (hospedaria) — "toda chave conta de onde veio"
sq_orlan_1 Quarto de Viajante (5): collect — 5 suprimentos para preparar a hospedagem
  da caravana → +8 amizade, gold escalado, notícia de estrada.
sq_orlan_2 Hóspede Sem Sombra (16): talk/flag — investigar o viajante que sumiu sem
  pagar nem deixar pegadas → +8 amizade, gold, mapa incompleto (pista).
sq_orlan_3 Conta Aberta (30): talk com ESCOLHA — cobrar a dívida antiga sem virar
  conflito (perdoar/parcelar/cobrar) → +8 amizade, item (chave de quarto reservado),
  sq_orlan_3_done (reservada: quarto reservado/descanso na cidade).

Savra (ervas/antídotos) — "veneno é só uma planta mal compreendida"
sq_savra_1 Antídoto Amargo (6): collect — 6 ervas da trilha para o antídoto base
  → +8 amizade, gold escalado, receita de antídoto.
sq_savra_2 Praga que Anda (22): kill/collect — eliminar 5 criaturas-praga noturnas e
  trazer 3 amostras → +8 amizade, gold, defesa contra pragas (flag).
sq_savra_3 O Fungo de Baixo (38): collect — 3 fungos da caverna (banda 2 — marco de
  banda) → +8 amizade, item (frasco de escama-verde), pista de bioma subterrâneo,
  sq_savra_3_done (reservada: bancada de antídotos/cura de status).

Maelor (Nyx/memória) — "a cidade esqueceu de propósito" [cadeia tardia]
sq_maelor_1 Passos Onde Não Há Luz (20) [gate: amizade 2]: talk noturno/flag —
  encontrá-lo à noite sem cruzar com a ronda da guarda → +8 amizade, gold, rumor de Nyx.
sq_maelor_2 Memória Que Escolheu Sumir (35) [gate: ato 2]: collect/talk — recuperar o
  registro apagado que prova que a cidade esqueceu algo de propósito → +8 amizade, gold,
  pista Anya/Cindar. Diferencia fé em Nyx de culto (coerência §15 da lore — sem revelar Sethra).
sq_maelor_3 O Silêncio Também Protege (55) [gate: ato 2 + amizade 4]: talk com ESCOLHA —
  revelar ou preservar o segredo (flag sq_maelor_secret que retorna em diálogo nos atos
  finais) → +8 amizade, item (lente de Luandil), sq_maelor_3_done (reservada: guia noturno).
```

## Regras das decisões novas (vinculantes — Refinamento v2 §3.6)

```text
1. VAELRION: chega no Ato 2, após o primeiro boss. Nenhum dos 11 NPCs é Vaelrion (ele não
   está no roster v1.1), logo nenhuma cadeia o tem como giver; PROIBIDO citá-lo em texto de
   quest/diálogo desta leva sem DialogueCondition de flag de ato 2 (act_2 iniciado).
2. SETHRA↔YAEL: rivais. A cadeia da Yael é a expressão jogável da rivalidade — q2 a expõe
   (comercial), q3 oferece escolhas espelhadas/conflitantes que o jogador pode mediar
   (favor Yael / favor Sethra / mediar), com flag sq_yael_rival_choice lida só por diálogo
   (F28). A cadeia NUNCA revela o culto de Nyx nem a identidade ritual de Sethra — a
   revelação é do Ato 3 (F36, dona da main). Sethra NÃO ganha cadeia própria nesta spec
   (não é NPC do roster v1.1; é figura da main lore).
3. CORVUS quest giver recorrente: já coberto pela cadeia dele na F35 — esta leva não
   adiciona quests ao Corvus (anti-duplicação).
```

## Engineering stories

```text
Como jogador, quero que TODOS os 23 NPCs do roster tenham cadeia com payoff permanente,
  para que nenhuma amizade seja beco sem saída.
Como Yael, quero que minha rivalidade com Sethra apareça nas minhas quests sem entregar
  o segredo dela, para que o Ato 3 ainda surpreenda.
Como QuestRegistry, quero as ~33 quests novas geradas pela MESMA tabela data-driven da
  F35 (IDs estáveis sq_<npc>_<n>), para não divergir de gramática nem duplicar gerador.
Como agente executor, quero a matriz de viabilidade objetivo×tipo antes de gerar, para
  nunca cortar objetivo do roster silenciosamente.
```

## Escopo

```text
Inclui:
- extensão da tabela do gerador GenerateNpcQuestChains (F35): +11 cadeias / ~33
  QuestDefinitions (IDs sq_<npc>_<n> conforme esboços acima) com objetivos mapeados aos
  tipos existentes, recompensas (gold/item/amizade +8/flag de serviço), gates (flag
  anterior + amizade mínima + ato quando citado);
- oferta no diálogo: opção "[!] Quest" no Conversar dos 11 NPCs quando elegível
  (TownNpcDialogueLibrary + DialogueCondition F28 RequiredFlag/MinFriendship);
- objetivos sem tipo viável: ADAPTAR para tipo existente mantendo a narrativa OU
  dormante (flag NotYetOfferable + nota) — mesma regra da F35;
- textos: título/descrição/diálogo de oferta/conclusão na VOZ do NPC (derivada do
  roster v1.1; epígrafes desta spec como base obrigatória);
- flags de serviço: q3 seta sq_<npc>_3_done; Yael conecta ao serviço F25 existente
  (Encomenda de Livro); demais flags documentadas como RESERVADAS (leva v2 de serviços);
- gating das decisões novas: Vaelrion (regra 1) e rivalidade Sethra↔Yael (regra 2);
- closeout: F30 valida refs de recompensa; matriz cadeia×status no report;
- EditMode tests: encadeamento por flag, gates compostos (flag+amizade+ato), recompensa
  de amizade, flags de rivalidade da Yael, amostra de 3 cadeias completas (fluxo
  sintético aceitar→completar→próxima).
```

## Fora de escopo

```text
Não inclui:
- cutscenes ou scripts de cena;
- cadeia para Sethra ou Vaelrion (não são NPCs do roster v1.1 — main lore/F36);
- revelação do culto de Nyx ou do nome ritual de Sethra (Ato 3 — F36);
- romance (quests de vínculo do roster são outra direction/spec);
- serviços únicos consumidores das flags reservadas (leva v2 de serviços — fora F25);
- main quest (F36), dailies do quadro, contratos do Zrix (F51) e secretas (F52);
- balance final de recompensas (fórmula do BALANCE_CURVES §7 já é a régua);
- retocar as 12 cadeias da 1ª leva (F35 — intocáveis).
```

## Regras de não duplicação

```text
Não criar gerador novo — ESTENDER a tabela do GenerateNpcQuestChains (F35).
Não criar tipos de objetivo novos (adaptar OU dormante).
Não criar segundo fluxo de oferta — única via diálogo F28/F34.
Não criar segundo serviço de amizade — FriendshipService (F26) é o dono.
Não criar segundo registro de flags — QuestFlagService é o dono.
Não duplicar quests da main (F36), do roster-romance, dailies, contratos ou secretas —
  escopo é exclusivamente sq_* dos 11 NPCs listados.
Não adicionar quests ao Corvus ou a qualquer NPC da 1ª leva.
```

## Critérios de aceite

### CA-1 — 11 cadeias geradas com matriz de viabilidade

- As 11 cadeias acima existem como QuestDefinitions encadeadas; nenhum objetivo derivado
  do roster foi silenciosamente cortado (todo objetivo está mapeado, adaptado ou dormante).
- Evidência: matriz cadeia×status + matriz objetivo×tipo no execution report; log do gerador
  com contagem ~33 (12+11 cadeias totais no registry após a spec).

### CA-2 — Encadeamento e gates compostos

- sq_X_2 permanece invisível/inofertável até sq_X_1_done + amizade mínima; gates de ato
  (Gurd/Alaric/Liora q3 → ato 1; Yael q3 e Maelor q2/q3 → ato 2) são respeitados.
- Evidência: EditMode tests de gate composto (flag+amizade+ato), incluindo o gate tardio do Maelor.

### CA-3 — Recompensas, flags de serviço e decisões novas

- Conclusão de quest dá +8 amizade (F26) e seta a flag correspondente; q3 seta
  sq_<npc>_3_done; sq_yael_3_done conecta ao serviço F25 Encomenda de Livro; flags
  reservadas dos demais documentadas no report; nenhuma quest cita Vaelrion sem gate de
  ato 2; a escolha de mediação da Yael grava sq_yael_rival_choice sem revelar o culto.
- Evidência: EditMode tests de recompensa/flag/rivalidade; validação F30 das refs de item.

### CA-4 — Fluxo ponta a ponta

- 3 cadeias percorridas ponta a ponta em teste sintético (aceitar→completar→próxima→flag
  de serviço), incluindo OBRIGATORIAMENTE a da Yael (rivalidade) e a do Maelor (gates tardios).
- Evidência: NpcChainsWave2Tests com fluxo e2e sintético das 3 cadeias.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Quests/
  GenerateNpcQuestChains.cs        (EXISTENTE F35 — tabela estendida com as 11 cadeias)
Assets/_Game/Scripts/NPC/
  TownNpcDialogueLibrary.cs        (ofertas "[!] Quest" dos 11 NPCs + condições F28)
Assets/_Game/Tests/EditMode/Quests/
  NpcChainsWave2Tests.cs           (NOVO)
docs/validation/
  fable_70_spec_npc_side_quest_chains_wave2_execution_report.md
```

## Contratos

### Data contracts
QuestDefinitions com IDs estáveis `sq_<npc>_<n>` (esboços desta spec); recompensas
referenciam IDs de item do F32 e fórmula escalada do BALANCE_CURVES §7 (QuestLevel =
nível de referência da cadeia, valores dos esboços); flags `sq_<npc>_<n>_done`, flags
narrativas `sq_yael_rival_known`/`sq_yael_rival_choice`/`sq_maelor_secret` e flags de
serviço (Yael → questFlag do serviço F25; demais reservadas).

### Runtime contracts
Gates avaliados pelos serviços existentes: QuestFlagService (flag anterior/ato) +
FriendshipService (amizade mínima). Nenhuma API nova de gate. Gates de ato só LEEM
flags de ato existentes (act_1_done/act_2 iniciado) — nunca as setam.

### Event contracts
N/A — não cria eventos novos; usa o fluxo de aceitação/conclusão de quest existente.

### Save contracts
N/A novo — quests/flags persistem pelas seções existentes (quests normais). Nenhum campo
de save novo.

### UI contracts
Marcador "[!] Quest" como opção no diálogo Conversar (TownNpcDialogueLibrary), visível
apenas quando a DialogueCondition (RequiredFlag/MinFriendship) é satisfeita — mesmo
padrão visual e de fluxo da F35.

## Sistemas afetados

```text
Quest registry/data (geração aditiva — tabela do gerador F35)
Diálogo de NPC (ofertas condicionais dos 11 restantes)
Amizade (consumo de gates + recompensa +8)
Serviço F25 da Yael (questFlag de unlock)
Validação de refs (F30)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Quests/GenerateNpcQuestChains.cs (tabela — aditivo)
Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs (ofertas — aditivo)
Assets/_Game/Tests/EditMode/Quests/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (geração só via gerador/AssetDatabase)
Packages/**
ProjectSettings/**
QuestFlagService / FriendshipService (consumir, não alterar)
main quest data (F36), dailies, contratos do Zrix (F51), secretas (F52), festivais (F53)
dados das 12 cadeias da 1ª leva (F35 — nenhuma linha alterada)
SaveManager / seções de save
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Confirmar F35 entregue (gerador + 12 cadeias no registry); matriz objetivos do roster ×
tipos existentes (viável/adaptar/dormante); IDs reais de itens (F32); flag do serviço F25
da Yael; flags de ato disponíveis; confirmar que nenhum sq_<npc>_* da 2ª leva já existe.
### Fase 1 — Tabela data-driven
Estender a tabela do gerador com as 11 cadeias (IDs, gates, recompensas, textos na voz
do NPC com as epígrafes desta spec; regras Vaelrion/rivalidade aplicadas).
### Fase 2 — Geração
GenerateNpcQuestChains re-roda idempotente: 12 cadeias antigas intactas + 11 novas;
log com contagens por cadeia (evidência de assets gerados).
### Fase 3 — Ofertas no diálogo
Opção "[!] Quest" + DialogueConditions para os 11 NPCs; flags narrativas e de serviço
no turn-in da q3 (Yael conectada ao serviço F25).
### Fase 4 — Testes e closeout
EditMode tests (gates compostos incl. ato 2 do Maelor, encadeamento, +8 amizade, flags
de rivalidade, 3 cadeias e2e); F30; matriz no report; run_strict_validation; report.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: N/A
- Must not run with: F34, F35, F10, F36, F51, F52, F53 (mesma cadeia de QuestRegistry/
  geradores de quest/TownNpcDialogueLibrary)
- Shared files/systems that require lock: geradores de quest, QuestRegistry data,
  TownNpcDialogueLibrary
- Reason: estende o gerador e as ofertas compartilhadas da F35; deve rodar DEPOIS de F35
  (1ª leva validada) na janela P4, e nunca em paralelo com specs de quest.

## Impacto em save/load

```text
Does this change save schema? NO (quests/flags usam seções existentes)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO (conteúdo data-driven; sem listeners novos)
```

## Impacto em UI/Unity

```text
Changes UI: marcador [!] no diálogo dos 11 NPCs (opção condicional — sem tela nova)
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador (evidência obrigatória)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: re-rodar o gerador alterar/duplicar as 12 cadeias da 1ª leva.
Mitigação: geração idempotente por ID; teste de regressão conta 12+11 cadeias e compara
  IDs antigos byte a byte (nenhuma definição da 1ª leva alterada).
Risco: cadeia da Yael vazar a revelação do culto (spoiler do Ato 3).
Mitigação: regra 2 vinculante — textos da q2/q3 só falam de rivalidade COMERCIAL; review
  de texto na matriz do report; F36 é dona da revelação.
Risco: citar Vaelrion antes do Ato 2 em texto de oferta/conclusão.
Mitigação: regra 1 — varredura textual "Vaelrion" nas 33 quests no closeout (proibido
  fora de condição de ato 2; nesta leva o esperado é ZERO menções).
Risco: flags de serviço reservadas ficarem órfãs (serviço v2 nunca criado).
Mitigação: tabela de flags reservadas no report + pendência explícita registrada para a
  leva v2 de serviços; Yael prova o caminho com o serviço F25 real.
Risco: objetivo do roster sem tipo viável ser cortado silenciosamente.
Mitigação: matriz de viabilidade explícita no report; adaptar OU dormante documentado.
```

## Rollback

```text
Gerador re-roda sem as 11 cadeias novas: registry volta às 12 da F35.
Ofertas no diálogo são aditivas — remover as entradas restaura o Conversar atual.
Nenhuma flag/seção de save nova para limpar (flags só existem após jogar as quests).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: F35 confirmada; matriz objetivos roster × tipos existentes; IDs de
      item/flag; serviço F25 da Yael; flags de ato.
- [ ] T002 — Tabela das 11 cadeias (textos na voz + regras Vaelrion/rivalidade) na tabela
      do gerador F35; geração idempotente com log.
- [ ] T003 — Ofertas no diálogo dos 11 NPCs + gates compostos + flags narrativas e de
      serviço (q3; Yael → serviço F25).
- [ ] T004 — Testes (3 cadeias e2e sintético incl. Yael e Maelor; regressão das 12 da
      F35) + F30; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gates compostos, encadeamento por flag, flags de rivalidade)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (12 cadeias da F35 e ofertas existentes intactas)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano completando 1 cadeia
  inteira da 2ª leva (preferência: Yael, por cruzar decisão nova)

## Definition of Done

```text
11 cadeias ofertáveis e completáveis; gates corretos (incl. ato 2 Yael/Maelor); voz preservada.
Regras das decisões novas aplicadas: zero menções a Vaelrion; rivalidade Sethra↔Yael
  mediável sem spoiler do culto.
Matriz de viabilidade no report (nenhum objetivo cortado silenciosamente); tabela de
  flags de serviço reservadas registrada.
12 cadeias da 1ª leva byte a byte intactas; nenhum arquivo proibido alterado; testes
  EditMode em Assets/_Game/Tests/EditMode/Quests/.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Diálogos/ofertas existentes dos 23 NPCs continuam funcionando (opções [!] novas são aditivas).
12 cadeias da F35, main quest (Ato 1), dailies, contratos e secretas intactas — nenhum ID
  existente alterado.
FriendshipService/QuestFlagService sem mudança de contrato.
Serviço F25 da Yael (Encomenda de Livro) continua funcional — só ganha o questFlag de unlock.
Nenhuma referência Unity em dados de quest; nenhum GameObject.Find em runtime.
```
