# Cindar's Hope — Consolidação de Lore (9 documentos / 3 relatórios)

> **Como este documento foi produzido:** três subagentes independentes (modelos `zai/glm-5`, `deepseek/deepseek-v4-pro`, `zai/glm-4.6`) leram integralmente três arquivos cada, sem editar nada. Este arquivo é a consolidação dos três relatórios, reorganizada por tema. Os trechos citados vêm dos próprios relatórios; onde um item não foi encontrado no material lido, ele está marcado como **não encontrado**.
>
> **Data:** 2026-09-27 · **Projeto:** Unity/C# — RPG + farm sim · **Mundo:** Vaalara / Cindar's Hope / Dornécia

---

## 1. Arquivos analisados

| Grupo | Arquivo | Linhas | Foco |
|---|---|---|---|
| A | `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md` | 699 | Canon jogável amplo de Vaalara |
| A | `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md` | 1007 | Lore narrativa da main quest |
| A | `docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md` | 1303 | Síntese + perguntas de decisão |
| B | `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md` | 1497 | Relacionamento / romance / poliamor |
| B | `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` | 1373 | Elenco, serviços, stats, quests |
| B | `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md` | 442 | Cidade como hub econômico/social/narrativo |
| C | `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md` | 1480 | Descoberta e registro de criaturas |
| C | `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md` | 1181 | Roster de monstros da caverna |
| C | `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md` | 1886 | Calendário, clima, luas, festivais |

**Hierarquia de conflito declarada pelos próprios documentos:**
- **Canon Direction v1.0** → autoridade máxima para "canon jogável amplo" (`VAALARA_GAME_CANON_DIRECTION_v1.0.md:6`).
- **Main Lore Direction** → vence para "lore específica da main quest" (`QUESTS_MAIN_LORE_DIRECTION.md:38-42`).
- **Lore Weaving Brief** → vence apenas como "registro de perguntas e síntese" (`QUESTS_LORE_WEAVING_BRIEF.md:33`).
- **Cidade** → base do hub; **Roster** → detalha habitantes; **Social** → projeta o futuro sem "redefinir" elenco/lore; cidade e roster "continuam vencendo" para elenco concreto, IDs e serviços (`SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md:42-44`).

Pipeline narrativa: **Canon Direction → Weaving Brief (síntese + perguntas) → Main Lore Direction (respostas consolidadas)**.

---

## 2. Cosmologia e cânone de mundo

- **Recorte [CANON]:** Vaalara → Dornécia → Cindar's Hope, com fazenda / cidade / caverna / ruínas (`VAALARA_GAME_CANON_DIRECTION_v1.0.md:38-46`).
- **Regra de tom:** "pastoral acima, horror abaixo" (`:97-105`; também `CITY_DESIGN_DIRECTION_v1.2.md:76-101`).
- **Luas:** três luas afetam gameplay, não são decoração (`:471-473`).
- **Camadas de revelação:** a lore é entregue progressivamente, nunca de uma vez (`:50-71`).
- **Regra de ouro de lore:** "lore deve aparecer por consequência jogável, não por enciclopédia obrigatória" (`:72`; `QUESTS_LORE_WEAVING_BRIEF.md:54`).

### Deuses e entidades

Tabela canônica de **11 deuses prioritários** (`:551-563`):
Anya, Alihana, Senya, Nyx, Thandra, Tandra/Telisandra, Finan, Merithus/Meritos Tolerus, Thoren, Kanthor, Kaand.

- **Thandra** sustenta a agricultura comum.
- **Anya** sustenta cura / Fonte / Mana — a divisão evita sobrecarregar Anya (`:565-574`).
- Na cidade existe **templo público de Kanthor**; **Anya é memória obscura** (estátuas antigas, Jardim das Estátuas, sonhos de Liora, silêncio de Corvus) e na fazenda existe só como **Fonte de Ressurreição** (`CITY_DESIGN_DIRECTION_v1.2.md:159-165`).
- **Altares construíveis** para todos os deuses, exceto Anya como construção livre (`:195-207`).
- Anya **não** tem templo ativo nem altar/estátua construível na fazenda (`QUESTS_MAIN_LORE_DIRECTION.md:263-265`; `QUESTS_LORE_WEAVING_BRIEF.md:324-329`, `:676-680`).

### Anya

- **Publicamente morta/desaparecida desde o Cataclismo [CANON]** (`:212-223`).
- **Canon Direction v1.0 (aberto):** "morta, fragmentada, aprisionada, silenciada ou transformada" (`:218-221`).
- **Main Lore Direction (fechado):** "Anya está fragmentada; não pode ser restaurada por completo" (`QUESTS_MAIN_LORE_DIRECTION.md:108-114`).
- Manifesta-se por ecos, sonhos, águas luminosas — nunca como NPC direto cedo (`:243-257`).
- Nenhum dos três finais restaura Anya por completo (`QUESTS_MAIN_LORE_DIRECTION.md:153`, `:1006`).

### Cindar

- Nymiriana, sacerdotisa de Anya há mais de 500 anos, próxima do Cataclismo (`:156-161`; `QUESTS_MAIN_LORE_DIRECTION.md:177-197`).
- Não é heroína limpa nem vilã: fez a "escolha impossível" de proteger fragmentos sem restaurar Anya (`QUESTS_MAIN_LORE_DIRECTION.md:199-207`).
- Grafia preferida: **Nymirianos** (sobre "Nymerianos") (`:170-182`).
- **Origem de Cindar's Hope:** cidade renomeada/preservada ao redor da esperança de Cindar (`:143-145`); pendente se ela fundou, renomeou ou apenas inspirou (`:679`).

### Nymirianos

- Povo antigo ligado a Anya, águas, cura e renascimento (`:184-195`).
- Enterrados por vestígios: arquitetura de água, runas fluidas, diários, cantos quebrados, orações preservadas, santuários subterrâneos (`QUESTS_MAIN_LORE_DIRECTION.md:241-255`).
- **Pendências:** aparência final, remanescentes vivos, relação com Aelythar, ruínas na caverna, natureza da Fonte (`:198-204`).

### Fonte de Anya / Água Viva / Mana

- **Fonte da Fazenda:** centro físico, mecânico e narrativo; interface de respec/ressurreição; conexão com caverna nível 100-101 (`:264-275`).
- **Água Viva:** recurso raro, não-spammável (`:280-297`).
- **Mana:** fruto sagrado, raríssimo, não é crop comum nem commodity (`:304-362`).
- **Árvore de Mana:** consciente, escolhe onde crescer (`:324-330`). Quatro opções canônicas aceitáveis para sua presença: Raiz Dormente / Escolha da Árvore / Fragmento vivo / Evento único (`:336-353`).
- **Regra canônica:** "Nenhuma lua sozinha faz Mana florescer" (`SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md:1474`).

### Outros elementos canônicos

- **Bromécia:** camada antiga perigosa sob Dornécia; erro civilizacional; todo poder tem risco/custo (`:432-465`).
- **Elyndor:** Arcos Estelares / portais / checkpoints — não é fast travel livre (`:368-394`).
- **Pedras Negras:** distinção canônica entre **Pedra de Meteoro Negra Estabilizada** (tecnologia de Elyndor, não drena almas) e **Pedra Negra Cultista/Corrompida** (drena alma/memória) (`:396-426`).
- **Decisões fechadas [CANON]:** `:656-669`. **Pendências:** `:676-685`.

---

## 3. Linha narrativa principal

### Estrutura em 4 atos

| Ato | Tema | Fragmento-alvo | Centralidade |
|---|---|---|---|
| **1** | Fonte e Esquecimento | Fragmento da Água | Fonte revive o jogador; Padre Corvus esquece a *Litania do Primeiro Retorno* (oração de Kanthor que preservava um vestígio de Anya) |
| **2** | Cindar e o Arco da Memória | Fragmento da Memória | Professor Vaelrion chega/traduz inscrições; descobre-se Cindar Nymiriana, Anya irrestaurável, e a existência do Arco da Memória |
| **3** | Culto, Pedra Negra e Vida | Fragmento da Vida | Sethra e o culto de Nyx ganham centralidade; Vaelrion cruza limites e usa o culto (e vice-versa) |
| **4** | Nível 100/101 e Esperança | Fragmento da Esperança | Boss gate no nível 100; nível 101 revela a sobreposição final; surge o Arquivista do Silêncio |

O nível 101 sobrepõe: santuário nymiriano + Arco da Memória + máquina bromeciana + portal de Elyndor + culto + Pedra Negra + Mana + fragmentos de Anya (`QUESTS_MAIN_LORE_DIRECTION.md:934-962`).

### Arco do jogador

Revive pela Fonte → percebe a ligação Fonte/memória/Anya/Pedra Negra → descobre Cindar e o Arco → confronta culto e Vaelrion → decide o destino dos fragmentos.

### Antagonistas

- **Professor Vaelrion Aelth-Silberharth** — Ninrorin/Elfo Cinzento, arqueólogo e linguista arcano. "Está parcialmente certo, isso o torna perigoso" (`:644-692`). Erro central: "compreender algo lhe dá direito de controlá-lo" (`:694-697`).
- **Sethra Veyl-Nocthar** — líder do culto de Nyx, dona da loja noturna "A Vela Sem Chama"; quer "libertar a cidade da esperança" via apagamento (`:731-792`).
- **Arquivista do Silêncio** — boss final; fusão instável de Vaelrion + Arco da Memória + máquina bromeciana + Pedra Negra + Água Viva corrompida + memórias drenadas + ritual de Sethra. Frase-tema: **"Nada sofre quando nada lembra"** (`:815-867`).

### Três finais [CANON]

| Final | O que faz | Tom |
|---|---|---|
| **Proteger** | Protege os fragmentos sem transformá-los em ferramenta | Mais alinhado a Cindar |
| **Selar** | Reforça o selo de Cindar; contém o Arco | Mais seguro e melancólico |
| **Usar** | Usa parte do Arco purificado para estabilizar Fonte/Mana/tecnologia | Não é maligno automático, mas mantém o risco de "repetir Bromécia" |

Nenhum final restaura Anya por completo (`:965-999`).

---

## 4. Lore weaving — como a lore entra no jogo

- **Anya:** ecos, sonhos, águas luminosas, eventos da Fonte, símbolos, lagos subterrâneos, cura inexplicável, fragmentos de memória, reações de Mana, diálogos de sacerdotes, ruínas nymirianas (`:243-257`).
- **Cindar:** ruínas, símbolos, diários, inscrições, relatos contraditórios e efeitos da Fonte — nunca explicada cedo (`:165-168`).
- **Litania do Primeiro Retorno:** lore dentro da liturgia pública de Kanthor; a oração preserva um vestígio de Anya sem que a cidade saiba. É o gatilho do Ato 1 (`QUESTS_MAIN_LORE_DIRECTION.md:326-364`).
- **Arco da Memória:** sobreposição de três camadas (santuário nymiriano → arquitetura de Elyndor → intervenção bromeciana), tecendo toda a lore do subsolo (`:367-425`).
- **Caverna:** eixo de horror/descoberta/mineração/combate; nível 100/101 como revelação maior (`:627-639`).
- **Luas como chaves narrativas:** Alihana revela memórias/sonhos, Senya força instabilidade/risco, Nyx revela corrupção/esquecimento (`:586-641`).
- **NPCs-ponte:** Yael Noite-Mansa é ponte com Nyx sem ser cultista (`:795-812`); o Weaving Brief levanta quem sabe mais do que revela, quem protege segredos, quem lucra e quem é vítima (`QUESTS_LORE_WEAVING_BRIEF.md:1212-1234`).
- **Cidade viva:** horários, rotinas, casais fixos (Nimble+Mirela, Gruta+Orlan, Mara+Tovin — `:879-883`), rumores e segredos revelados depois (`:169-182`).

---

## 5. Cidade, NPCs e estrutura social

### Elenco

**24 NPCs canônicos** (23 + Ancião Velorin, extensão pós-WAVE12C; `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md:242`).

Serviços definidos: Padre Corvus (curandeiro/guardião, Kanthor), Mara Vellum (escriba/comerciante, Merithus), Sylveth (plantador/curandeiro, Thandra), Brumdar (artesão/combatente, Thoren), construtores Nimble/Gurd/Hund, Ozzra (alquimista, Senya), Gruta (taverna), Zrix (explorador, Finan), Yael (loja noturna, Nyx), Thalindra (pesquisadora, Alihana), Dagna (mineradora, Thoren), Pip (entrega, Finan), Ser Alaric (guarda, Kanthor), Mirela (costura), Renko (loja geral), Eiran (tratador, Thandra), Liora (música/sonhos, Alihana), Orlan (estalagem), Savra (ervas, Tandra/Telisandra), Tovin (escriba), Maelor (explorador, Nyx), Ancião Velorin (líder da aldeia, sem loja) (`:1298-1330`).

**Provisório/placeholder:** 4 NPCs novos — Sael Maré-Quieta, Mella Forno-Quente, Hess Couro-Fundo, Tibbet Vela-Torta — descritos no update de autossuficiência (24→28), mas **fora da tabela-mestra** e "ainda não implementados" (`:26-38`). Tibbet "adora Nyx em segredo" (`:33`). Falta adicionar a Ovelha ao FarmAnimalCatalog para fechar a cadeia de lã (`:35`).

### Sistema social

- **13 estados de relacionamento:** Unknown → … → Married / PolyculePartner / Estranged / Blocked (`SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md:331-344`).
- Conceitos: Friendship, Trust, Affection, Polycule, Social Memory (`:185-323`).
- **11 candidatos românticos:** Sylveth, Ozzra, Zrix, Yael, Thalindra, Dagna, Ser Alaric, Eiran, Liora, Savra, Maelor (`:517-529`).
- **3 casais fixos:** Nimble+Mirela, Gruta+Orlan, Mara+Tovin (`:533-537`).
- Elegibilidade: `RomanceEligibleAnyPlayerGender`, `MarriedToNpc`, `TooYoungOrNarrativelyBlocked` (`:441-450`).
- Presentes: categorias Loved/Liked/…/Forbidden/Romantic (`:572-582`); limites 1/dia, 2/semana baseline (`:620-624`).
- **Decisões fechadas:** romance homoafetivo permitido; casamento poliamoroso consentido até **3 parceiros**; parceiro pode virar companion/farm helper; baseline da caverna permanece **1 companion ativo** (`:885-886`, `:1449-1471`).
- Anti-exploit de economia: presentes não quebram a loja; helper com orçamento global; qualidade não anula rejeição (`:1291-1322`).
- Quests pessoais desbloqueadas por relação alta, sem bloquear o core loop (`:700-708`).

### A cidade como hub

- **6 hubs:** econômico, social, narrativo, progressão, cultural e contraponto da caverna (`CITY_DESIGN_DIRECTION_v1.2.md:42-47`).
- **Fluxo do jogador:** rotinas/horários, rumores, festivais e visitas à fazenda gatilhadas por NpcRelationship 25/50/75/100 (`:340-352`).
- **Amarrações de lore:** fundações antigas sob o templo de Kanthor (`CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md:274`); Fonte de Anya na fazenda (`:1336-1353`); Velorin "sabe mais sobre a Fonte e Cindar do que deixa transparecer" (`:1324`).

---

## 6. Caverna — biomas, roster e bosses

### Biomas / faixas de nível

| Faixa | Bioma |
|---|---|
| 1-10 | Caverna de Pedra |
| 11-25 | Floresta Subterrânea |
| 26-40 | Caverna de Gelo |
| 41-55 | Caverna de Fogo |
| 56-70 | Ruínas Antigas |
| 71-85 | Abismo |
| 86-99 | Núcleo Corrompido |
| 100 | Gate Final |
| 101 | Endgame |

### Roster

- **~50 inimigos** com stats reais (HP/MP/STA/FOR/CON/DES/INT/VON/CAR/XP), traits, ataques e drops (`CAVE_MONSTER_ROSTER_DIRECTION.md:15-101`).
- Exemplos: Ácaro da Fenda (1-8, Tiny), Besouro Ferrugem (7-20, DurabilityStress), Urso-Coruja Raiz-Oca (11-25, EliteDuelist), Tirano Ocular da Lua Negra (71-85, mini-boss), Wyvern de Pedra Negra (86-99, boss).
- O campo `BR` foi **removido**; o comportamento equivalente vive em Move/Behavior/Traits (`:15-101`).
- **17 facções** (`:401-418`), incluindo `faction_corrupted` e `faction_abyssal` (`:415-416`).

### Bosses

- **7 bosses de gate** (níveis 15/30/45/60/75/90/100), 3 fases cada; mais **5 bosses de nível 101** (`:387-393`, `:853-858`).
- Regra canônica: sem ataque instantâneo sem telegraph; sem prender sem saída; toda fase com janela de vulnerabilidade (`:387-393`).
- Ex.: `boss_core_oathbreaker` (Gate 100, HP 17500) quebra a arena na Fase 2 (`:853-858`).

### Lore de corrupção e Eco de Cindar

- Variante **"Corrompido"** surge no nível 86+ ou por Pedra Negra (`:294`).
- `enemy_anya_silent_echo` é LoreGuardian, não farmável: "repelir, proteger, revelar lore" (`:787-789`).
- `boss_anya_bound_echo` (nível 101): "não é luta comum; deve alternar proteção, teste de memória e escolha/interação" (`:885-888`).
- **Packs** (~25) com justificativa ecológica/faccional; ex.: `pack_corrupted_core` — "culto da Pedra Negra usa massas corrompidas como muralhas vivas" (`:1018-1019`).

---

## 7. Ciclo de mundo — calendário, clima, luas e festivais

- **Calendário:** 4 estações × 28 dias = **112 dias/ano**; 7 dias/semana (`SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md:369-373`).
- **Relógio:** 06:00–02:00 jogável; 02:00 = colapso forçado; 1h in-game = 60s reais (~20 min/dia) (`:156-179`). O tempo pausa em UI modal (`:196-213`).
- **Clima:** Sunny, Cloudy, Rain, Storm, Fog, Wind, Heat, Cold, Snow; baseline inicial Sunny/Cloudy/Rain/Storm/Snow/Fog (`:584-604`).
  - Rain molha crops externas; Snow não rega; Fog pode revelar inscrições/passagens; Storm pode tornar a caverna mais perigosa ou recompensadora (`:650-688`).

### As três luas (nomes próprios de Vaalara)

| Lua | Domínio | Ref. |
|---|---|---|
| **Alihana** | memória, sonhos, Fonte, Cindar, Água Viva, inscrições nymirianas, sementes raras | `:912-978` |
| **Senya** | caos, magia, mutação, instabilidade, festas, preços altos | `:984-1043` |
| **Nyx** | noite, segredos, loja noturna, Pedra Negra, esquecimento, culto, criaturas noturnas | `:1048-1117` |

- **Ciclo lunar:** dia 7 = Alihana menor; dia 14 = Senya menor/média; dia 21 = Nyx menor/média; dia 28 = evento sazonal maior (`:866-869`).
- **Alinhamentos raros** (duas ou três luas) geram eventos de main quest (`:1121-1151`).
- O nível 100/101 pode exigir ou simular alinhamento triplo (`:1149`).
- A Fonte de Anya reage a fragmentos, luas, Pedra Negra e alinhamentos (`:1416-1442`).

### Festivais

Plantio, colheita, comida/taverna, música/Alihana, juramento/Kanthor, rural/Thandra, mercado/Finan, noturno/Nyx, caótico/Senya (`:1549-1558`).
Festivais alteram schedules e preços e podem ter quest hooks, mas "não devem quebrar rotina agrícola essencial sem aviso" (`:1570`).

---

## 8. Bestiário — descoberta e registro

- **Chaves:** `EnemyKnowledgeKey` com `EnemyId`, `EnemyFamilyId`, `BossId`, `FactionId`, `DropId`, `VulnerabilityTag`, `LoreEntryId` etc. (`BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md:202-223`).
- **Estados progressivos:** Unknown → Seen → ScannedFuture → Fought → Defeated → RepeatedDefeated → Studied → FullyDocumented (`:264-272`).
- **Confiança:** Rumor → Partial → Confirmed → Mastered (`:309-313`). NPCs e livros concedem Rumor; combate confirma.
- **Fontes de descoberta:** observação direta, combate, teste de dano/tag (dano aumentado revela vulnerabilidade; reduzido revela resistência), drops, NPCs (ferreiro, Padre Corvus, Yael, Sethra, Vaelrion — cada um com domínio distinto), livros/ruínas, pets e companions (`:407-640`).
- **Tags de lore por criatura:** `LoreNoteId`, `QuestKnowledgeId`, `FactionId`, `BiomeId` (`:221-222`).
  - Ex.: Yael revela "Nyx, segredos, criaturas noturnas" (`:563`); Sethra revela "rumores de Pedra Negra, informações ambíguas ou manipuladas" (`:566`).
- **Facções:** o bestiário usa `FactionId` mas não define facções — remete ao roster da caverna (17 facções).
- **Canônico:** estados de conhecimento, regras anti-spoiler, chaves e integração com UI de equipamento/spell.
- **Proposto (não na primeira entrega):** UI completa de bestiário, HUD permanente, Knowledge Log navegável, scan spell, NPC research services (`:37-65`).

---

## 9. Tensões, ambiguidades e pendências (visão cruzada)

| # | Tensão / pendência | Onde | Impacto |
|---|---|---|---|
| 1 | **Anya:** Canon Direction v1.0 ainda mantém em aberto ("morta, fragmentada, aprisionada, silenciada ou transformada"), enquanto o Main Lore Direction **fecha** como "fragmentada". Canon v1.0 não foi atualizado. | `VAALARA...v1.0.md:218-221` vs `QUESTS_MAIN_LORE_DIRECTION.md:108-114` | Ambiguidade documental real; quem ler só o canon amplo encontra "pendente" |
| 2 | **Fôlego/Breath:** `CITY_DESIGN` lista como atributo; o Roster afirma que "não é atributo, recurso, barra, custo, campo de save/load ou stat" e não existe para NPCs. | `CITY_DESIGN...v1.2.md:267` vs `CITY_NPC_ROSTER...v1.1.md:100-110` | Conflito direto entre docs ativos |
| 3 | **Nomes das estações e dias da semana** são placeholders (Primavera/Verão/Outono/Inverno). | `SEASONS...md:383`, `:393-394`, `:1874-1875` | Bloqueia textos finais (fable_36/43/63) |
| 4 | **Eventos lunares: fixos por calendário ou seeded?** | `SEASONS...md:1877` | Impacta determinismo de spawns da cave e do bestiário |
| 5 | **Aelythar** — entidade citada no Canon Direction e no Weaving Brief, mas **nunca definida** nos três arquivos. | `VAALARA...v1.0.md:202`; `QUESTS_LORE_WEAVING_BRIEF.md:1061` | Referência órfã; lore incompleta |
| 6 | **4 NPCs novos (24→28)** fora da tabela-mestra; Velorin também fora do `ValidateRefinedCanonicalNpcTownPopulation` (cobre 23; registry `CanonicalCount = 24`). | `CITY_NPC_ROSTER...v1.1.md:26-38`, `:242` | Inconsistência de elenco / cobertura de validator |
| 7 | **Bestiário UI toda futura** enquanto o roster já tem 50+ criaturas com stats completos. | `BESTIARY...md:37-77` vs `CAVE...md` | Dados existem sem interface de acesso |
| 8 | **Papel de Cindar** (fundou / renomeou / inspirou) ainda aberto — o Main usa o hedge "fundada, preservada ou renomeada". | `VAALARA...v1.0.md:679`; `QUESTS_MAIN_LORE_DIRECTION.md:185` | Impacta textos de fundação |
| 9 | **Dano/stamina/MP cost por ação inimiga** pendente no roster. | `CAVE...md:1173-1175` | Sem referência para TTK real |
| 10 | **Nomes das estações/dias + limite de colapso** (02:00 baseline vs penalidade às 00:00). | `SEASONS...md:1876` | Regra de tempo ainda não finalizada |
| 11 | **RepeatedDefeated** sem número definido de derrotas. | `BESTIARY...md:1472` | Progressão de bestiário indefinida |
| 12 | **Spoiler tiers** (Safe/Progression/Lore/MainQuest/Endgame) sem atribuição concreta de criaturas nem gatilhos de liberação. | `BESTIARY...md:1343-1362` | Anti-spoiler sem regra executável |
| 13 | **Integração cave-clima-lua** declarada mas não quantificada (spawn modificado, elites, Pedra Negra mais ativa; variantes Lunar/Corrompido sem triggers). | `SEASONS...md:1382-1394`; `CAVE...md:295-296` | Efeitos existem sem tabela concreta |
| 14 | **67 perguntas estruturantes** no Weaving Brief; o Main responde várias (verdade de Anya, antagonista, boss final, nº de atos, finais), mas não todas (destino do portal final, se o mundo piora com demora, alinhamento lunar perto do nível 100). | `QUESTS_LORE_WEAVING_BRIEF.md:1013-1303` | Lacunas narrativas remanescentes |
| 15 | **Nomenclatura do próximo documento:** o Weaving Brief cita `QUESTS_MAIN_PROGRESSION_DIRECTION.md`; o Main cita `QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`. | `QUESTS_LORE_WEAVING_BRIEF.md:1290-1292` vs `QUESTS_MAIN_LORE_DIRECTION.md:40` | Ambiguidade de continuidade documental |
| 16 | **Conteúdo do Weaving Brief ausente no Canon Direction:** visuais das três luas (Alihana: traços élficos, véu; Senya: meio-elfa ruiva; Nyx: elfa da noite, pele branca, olhos púrpura) e a distinção "Fruto Mana" vs "Raiva Primeva / Mana Viva". | `QUESTS_LORE_WEAVING_BRIEF.md:536-610`, `:574-582` | Canon v1.0 defasado em relação à consolidação posterior |
| 17 | **Pendências abertas do social:** valores de Friendship/Trust/Affection, thresholds, aniversários, item de namoro/casamento, quais NPCs são PolyCompatible/Blocked, PartnerCompanion/HelperEligible, gifts por NPC, reação à Fonte e às luas. | `SOCIAL...md:1477-1496` | Sistema social sem números finais |
| 18 | **Grafia da divindade de Savra:** "Tandra/Telisandra". | `CITY_NPC_ROSTER...v1.1.md:237` | Nomenclatura não consolidada |

### Observações importantes

- **Nenhuma contradição direta** foi encontrada entre os três documentos do grupo A: as divergências são de **fechamento vs. abertura** (Anya) e de **escopo/cobertura** (visuais das luas, Arco da Memória) — não de afirmações contraditórias.
- A **evolução narrativa não foi propagada** ao Canon Direction v1.0: o Main introduz elementos (Arco da Memória como conceito, Arquivista do Silêncio, Litania do Primeiro Retorno, Padre Corvus, Yael Noite-Mansa, a loja "A Vela Sem Chama") que o documento-mãe não registra — coerentes com o canon, mas ausentes dele, sugerindo que o Canon v1.0 é anterior à consolidação narrativa.

---

## 10. Itens não encontrados no material lido

- **Definição de Aelythar** — citada, nunca descrita.
- **Conteúdo de** `QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md` — referenciado, não lido.
- **Rotinas/schedules detalhados** — vivem em `CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md` (não lido).
- **Amarração visual/rotina da cidade** — `UI_UX_FULL_GAMEPLAY_DIRECTION.md` (não lido).
- Os documentos de dependência citados (SPEC_SOURCE_MAP, COMPANIONS, PETS, ECONOMY, CITY_SELF_SUFFICIENCY) não fizeram parte da leitura; quando relevantes acima, o conteúdo é apenas o referenciado pelos nove arquivos-alvo.

---

## Anexo — Links de leitura por grupo

**Grupo A — Canon e narrativa**
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
- `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
- `docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md`

**Grupo B — Sociedade, cidade e NPCs**
- `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
- `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
- `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`

**Grupo C — Bestiário, cave e ciclo de mundo**
- `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
- `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
