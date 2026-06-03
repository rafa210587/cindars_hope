# Cindar's Hope — City Design Direction v1.2

> **Status:** direção ativa de gameplay/sistema  
> **Local:** `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> **Substitui como direção ativa:** `CITY_DESIGN_DIRECTION_v1.1.md`  
> **Base de canon obrigatória:** `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Roster ativo:** `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> **Função:** grande descrição de como a cidade de Cindar's Hope deve funcionar no jogo.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Alterações da v1.2

Esta versão consolida quatro decisões novas:

1. **Anya não pode receber estátua ou altar construível na fazenda.**
2. A única representação física de Anya ligada à fazenda é a **Fonte de Ressurreição / Fonte de Anya**.
3. A cidade passa a suportar NPCs casados, NPCs indisponíveis para romance e NPCs disponíveis para relacionamento/casamento com o jogador.
4. O roster de NPCs passa a exigir subraça/origem, classe clara, aparência, HP, atributos, resistências/status, relações e pelo menos 3 quests por NPC.

---

# PARTE A — Regra de Anya

## 1. Anya na fazenda

Regra fechada:

```text
O jogador não pode construir estátua de Anya na fazenda.
O jogador não pode construir altar independente de Anya na fazenda.
A única representação física de Anya na fazenda é a Fonte de Ressurreição / Fonte de Anya.
```

A Fonte pode evoluir, receber upgrades, reagir a Água Viva, liberar respec, ressurreição e eventos narrativos, mas ela não deve virar uma construção decorativa opcional.

## 2. Por que essa regra existe

Anya deve permanecer como ausência, mistério e memória fragmentada.

Se o jogador puder construir uma estátua comum de Anya cedo, o mistério perde força.

Direção narrativa:

```text
Kanthor é público.
Thandra é rural.
Anya é descoberta.
```

## 3. Anya na cidade

Na cidade, Anya não tem templo ativo.

Ela aparece apenas em:

- estátuas antigas gastas;
- Jardim das Estátuas Antigas;
- inscrições apagadas;
- símbolos de água/renascimento;
- falas confusas de alguns NPCs;
- registros incompletos;
- sonhos de Liora;
- pesquisas de Thalindra;
- silêncio desconfortável de Corvus;
- eventos raros de Alihana.

Regra:

```text
As estátuas antigas de Anya existem na cidade, não como culto ativo, mas como vestígio histórico.
Na fazenda, apenas a Fonte representa Anya.
```

---

# PARTE B — Templo, altares e religião pública

## 4. Templo principal: Kanthor

O templo público principal de Cindar's Hope é de **Kanthor**.

Funções:

- justiça;
- ordem;
- juramentos;
- proteção civil;
- resolução de disputas;
- legitimidade da guarda;
- cerimônias públicas;
- contraste com segredos subterrâneos.

## 5. Thandra

Thandra aparece na cultura rural:

- festival de colheita;
- bênçãos de animais;
- pequenos ritos de estação;
- loja de sementes;
- concursos agrícolas;
- altares rurais simples.

## 6. Altares construíveis pelo jogador

O jogador pode erguer altares a deuses relevantes, exceto Anya como construção independente.

Tabela base:

| Deus | Pode ter altar construível? | Local possível | Bônus sugerido |
|---|---:|---|---|
| Kanthor | Sim | cidade/fazenda | defesa, ordem, reputação com guarda |
| Thandra | Sim | fazenda/cidade rural | crops, animais, qualidade agrícola |
| Finan | Sim | fazenda/cidade | sorte, achados, mercadores raros |
| Merithus/Meritos | Sim | cidade/fazenda | contratos, taxas, licenças, caixa de envio |
| Thoren | Sim | fazenda/ofícios | ferramentas, forja, durabilidade |
| Alihana | Sim | fazenda/cidade | sonhos, sementes raras, pistas |
| Senya | Sim | cidade/fazenda | magia, festivais, mutações controladas |
| Nyx | Sim, com restrição | área oculta/noite | segredos, loja noturna, stealth |
| Tandra/Telisandra | Sim | borda/floresta | madeira, caça, resistência natural |
| Kaand | Sim, com risco | área de treino/caverna | força, dano, risco/recompensa |
| Anya | **Não como altar/estátua livre** | **somente Fonte de Ressurreição** | cura, respec, ressurreição, Água Viva |

Regra:

```text
Anya não compete com outros altares.
Anya é sistema narrativo central da Fonte.
```

---

# PARTE C — Relacionamentos e casamento

## 7. Tipos de relacionamento de NPC

Cada NPC deve ter um estado social claro:

```text
MarriedToNpc
RomanceEligibleAnyPlayerGender
UnavailableForRomance
TooYoungOrNarrativelyBlocked
WidowedOrPastRelationship
```

## 8. Casamento com o jogador

Quando um NPC for marcado como `RomanceEligibleAnyPlayerGender`, isso significa:

```text
O NPC pode se relacionar e casar com o jogador independentemente do gênero escolhido pelo jogador.
```

Regras:

- sem conteúdo explícito;
- foco em vínculo, confiança, quests e rotina;
- casamento deve exigir relacionamento alto, questline pessoal completa e evento próprio;
- NPC casado com outro NPC não deve virar candidato de romance;
- NPC muito jovem ou narrativamente bloqueado não deve ser candidato;
- casamento não deve remover automaticamente a função social do NPC;
- alguns cônjuges podem visitar a fazenda, morar parcialmente na fazenda ou manter trabalho na cidade.

## 9. Casais já existentes na cidade

Casais recomendados:

| Casal | Função narrativa |
|---|---|
| Nimble Galhobaixo + Mirela dos Laços | casal de ofícios; construção + costura; tensão entre praticidade e estética |
| Gruta Panela-Funda + Orlan Pouso-Curto | casal da taverna/estalagem; comida, rumores e viajantes |
| Mara Vellum + Tovin Mãos-de-Selo | casal burocrático; cartório, contratos, licenças e humor seco |

Esses casais não são candidatos de romance do jogador.

## 10. Candidatos a relacionamento/casamento

Candidatos recomendados:

| NPC | Gênero | Tipo |
|---|---|---|
| Sylveth | mulher | RomanceEligibleAnyPlayerGender |
| Ozzra | mulher | RomanceEligibleAnyPlayerGender |
| Zrix | homem | RomanceEligibleAnyPlayerGender |
| Yael | mulher | RomanceEligibleAnyPlayerGender |
| Thalindra | mulher | RomanceEligibleAnyPlayerGender |
| Dagna | mulher | RomanceEligibleAnyPlayerGender |
| Ser Alaric Veyr | homem | RomanceEligibleAnyPlayerGender |
| Eiran Valeclaro | homem | RomanceEligibleAnyPlayerGender |
| Liora Canta-Rio | mulher | RomanceEligibleAnyPlayerGender |
| Savra Escama-Verde | mulher | RomanceEligibleAnyPlayerGender |
| Maelor Cinza | homem | RomanceEligibleAnyPlayerGender, tardio |

Observação: Maelor deve ser candidato tardio, condicionado a confiança, Nyx e trama de memória.

## 11. NPCs indisponíveis

| NPC | Motivo |
|---|---|
| Padre Corvus | voto religioso e papel institucional |
| Nimble | casado com Mirela |
| Mirela | casada com Nimble |
| Gruta | casada com Orlan |
| Orlan | casado com Gruta |
| Mara | casada com Tovin |
| Tovin | casado com Mara |
| Hund | bloqueado por dever familiar/defesa |
| Gurd | bloqueado inicialmente; pode virar amizade/companion, não romance nesta versão |
| Renko | foco em comércio; romance não prioritário |
| Pip | jovem/tutoriais; não romance |

---

# PARTE D — Visitas à fazenda

## 12. Regra geral

Cidadãos podem visitar a fazenda conforme:

- reputação geral da cidade;
- relacionamento individual;
- quest ativa;
- serviço contratado;
- casamento;
- festival;
- horário;
- estação;
- lua ativa;
- progresso na caverna;
- evento de companion;
- nível da fazenda.

## 13. Visitas por relacionamento

```text
NpcRelationship >= 25:
  carta, comentário, presente simples ou visita curta.

NpcRelationship >= 50:
  visita social, serviço especial, pedido pessoal ou evento de amizade.

NpcRelationship >= 75:
  ajuda na fazenda, evento de confiança, visita de alerta ou quest avançada.

NpcRelationship >= 100:
  vínculo máximo, casamento se elegível, habilidade única ou rotina especial.
```

## 14. Visitas por casamento

Se o NPC casar com o jogador:

- pode visitar ou morar parcialmente na fazenda;
- pode manter trabalho na cidade;
- pode ter agenda híbrida;
- pode dar buff leve por rotina, não por bônus quebrado;
- não deve invalidar companions, pets ou automação;
- não deve abandonar serviços essenciais da cidade sem substituto.

---

# PARTE E — Exigências do roster

## 15. Cada NPC deve ter

- ID estável;
- nome;
- gênero;
- idade narrativa aproximada;
- raça;
- subraça/origem;
- classe/arquétipo clara;
- função na cidade;
- serviço mecânico;
- estado de relacionamento;
- disponibilidade para casamento;
- aparência;
- personalidade;
- background;
- relações com pelo menos 2 NPCs;
- se visita a fazenda;
- se pode virar companion/serviço;
- HP;
- MP, se aplicável;
- atributos: FOR, CON, DES, INT, VON, CAR;
- resistências/status;
- pelo menos 3 quests detalhadas;
- segredo/tensão.

## 16. Fonte ativa do roster

```text
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
```

Esse documento deve ser usado antes de qualquer spec de:

- NPCs;
- lojas;
- agenda;
- romance/casamento;
- reputação;
- companions;
- visitas à fazenda;
- quests pessoais.

---

# PARTE F — Dados futuros

## 17. Campos recomendados para NPCDataSO

```text
NpcId
DisplayName
Gender
AgeBand
RaceId
SubraceId
ClassId
RoleTags[]
ServiceTags[]
RelationshipStatus
SpouseNpcId
RomanceEligibility
MarriageEligibility
CanVisitFarm
CanMoveToFarmAfterMarriage
FarmVisitRules[]
BaseHP
BaseMP
Stats
StatusResistances[]
StatusWeaknesses[]
CombatProfileId
ScheduleId
HomeLocationId
WorkLocationId
QuestlineId
GiftPreferences
DialogueSetId
PortraitSpriteId
```

## 18. Campos recomendados para RomanceData

```text
NpcId
RomanceEligibility
RequiredRelationship
RequiredQuestIds[]
MarriageEventId
PostMarriageScheduleId
FarmVisitScheduleId
BlockedReason
```

## 19. Campos recomendados para AltarData

```text
AltarId
DeityId
CanBuildOnFarm
CanBuildInCity
RequiresLicense
RequiresLoreDiscovery
BonusType
BonusDuration
CooldownDays
ReputationImpact[]
```

Para Anya:

```text
DeityId = anya
CanBuildOnFarm = false
CanBuildInCity = false
UsesFountainSystem = true
```

---

# PARTE G — Decisões fechadas

```text
Templo público principal da cidade é de Kanthor.
Anya não tem templo ativo conhecido na cidade.
Anya aparece por estátuas antigas na cidade e pela Fonte na fazenda.
Jogador não pode construir estátua/altar de Anya na fazenda.
A única representação física de Anya na fazenda é a Fonte de Ressurreição.
Thandra mantém festival e ritos rurais.
Altares de outros deuses continuam possíveis.
NPCs agora exigem subraça/origem, classe clara, aparência, HP, atributos e status.
Existem NPCs casados entre si.
Existem NPCs disponíveis para relacionamento/casamento com jogador de qualquer gênero.
Cidadãos podem visitar a fazenda por reputação, relacionamento, serviço, quest ou casamento.
```

---

# PARTE H — Próximas specs derivadas

```text
spec_city_npc_data_roster_stats.md
spec_city_relationship_romance_marriage.md
spec_city_farm_visits_schedule.md
spec_city_kanthor_temple_and_altars.md
spec_farm_fountain_anya_no_buildable_statue.md
```
