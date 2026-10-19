# Cindar's Hope — Social Relationship & Romance Direction

> **Status:** documento canônico de direção futura de relacionamento, amizade, romance, casamento, poliamor e parceiros-companions  
> **Local:** `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> **Função:** preparar a arquitetura de design para vínculos sociais, romance, casamento, poliamor consentido, visitas, parceiro-companion e ajuda na fazenda sem forçar implementação imediata.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados/runtime/UI/Unity.

---

## 0. Regra anti-duplicação

Este documento define a direção futura do **sistema social**.

Ele **não redefine**:

```text
NPC roster, raças, serviços e romance eligibility individual;
layout da cidade;
horários e prédios da cidade;
companion AI, companion jobs e companion combat;
pets, pet bond e pet routine;
economia, preços, loja, estoque e recompensas;
lore canônica de Vaalara, deuses, Anya, Nyx, Senya ou Dornécia.
```

Fontes específicas que continuam vencendo:

```text
CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
  vence para elenco concreto, NPC IDs, romance eligibility, casais fixos, classe funcional, religião pessoal e serviços.

CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
  vence para prédios, residências, camas, rotas, horários, locais de visita e anchors físicos da cidade.

COMPANIONS_DIRECTION.md
  vence para companion runtime, active companion, cave follow/leash, downed/recovery, companion jobs e companion bond funcional.

PETS_DIRECTION.md
  vence para pet bond, pet mood/energy, pet home area, pet food, pet toys, pet follow/leash e pet hints.

ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
  vence para BaseValue, preço, estoque, restock, loja, SellPoint e anti-arbitragem.
```

Este documento vence quando a decisão for sobre:

```text
amizade;
vínculo social;
presentes sociais;
diálogo por relação;
romance;
romance homoafetivo;
casamento;
casamento poliamoroso consentido;
limite de parceiros românticos/cônjuges;
parceiro virando companion;
parceiro ajudando na fazenda;
spouse/partner helper;
visitas sociais à fazenda;
convites sociais;
ciúme/conflito social leve;
progressão social não econômica;
HUD/feedback social;
save/load social;
anti-exploit social.
```

---

## 1. Escopo temporal

Esta feature **não entra nas specs executáveis atuais**.

Objetivo desta direção:

```text
preparar próximas features para não contradizerem relacionamento/romance/casamento;
garantir que cidade, NPCs, companions, pets, fazenda, calendário, UI e save deixem hooks corretos;
evitar que specs atuais fechem portas técnicas ou narrativas;
definir o que deve ficar reservado para implementação futura.
```

Durante a próxima leva de specs executáveis, esta direção deve ser usada apenas para:

```text
não bloquear NPC relationship state futuro;
não usar campos incompatíveis;
não transformar romance/casamento em caminho obrigatório de poder;
não acoplar partner helper em farm automation atual;
não misturar companion bond com romance sem regra explícita;
não misturar pet bond com social romance;
não persistir estado social de forma incompleta que vire dívida técnica;
reservar anchors de visita/partner sem implementar runtime social.
```

Não implementar agora:

```text
RelationshipSystem runtime;
Friendship UI;
gift runtime completo;
romance cutscenes;
marriage ceremony;
poly marriage ceremony;
partner moving to farm;
partner helper automation;
romantic partner companion unlock runtime;
jealousy runtime;
social questline runtime;
festivals sociais completos;
dating state;
divorce/separation;
children/family system.
```

---

## 2. Inspiração e limite de referência

A direção segue padrões comuns de **farm sim/social sim**:

```text
conversar com NPCs;
dar presentes;
descobrir preferências;
ganhar amizade ao longo do tempo;
liberar eventos pessoais;
resolver quests pessoais;
iniciar romance;
casar;
receber ajuda doméstica/fazenda;
ver NPCs reagindo ao calendário, clima, festivais e progresso do jogador.
```

Referências de gênero incluem Harvest Moon e Stardew Valley apenas como inspiração de estrutura.

Regra:

```text
Não copiar nomes, textos, UI, balance, eventos, personagens, falas, paleta, assets ou tabelas específicas.
Usar apenas padrões de gênero e adaptar à identidade de Vaalara, Cindar's Hope, Anya, Nyx, Senya, cidade, fazenda e caverna.
```

---

## 3. Objetivo de produto

O sistema social deve fazer a cidade parecer habitada, reativa e emocionalmente consistente sem transformar vínculos sociais em obrigação mecânica.

Pilares:

```text
1. Relações dão contexto, histórias, desbloqueios e conveniência, não poder obrigatório.
2. Romance é opcional.
3. Casamento é opcional.
4. Romance homoafetivo é permitido por padrão para candidatos marcados como RomanceEligibleAnyPlayerGender.
5. Casamento poliamoroso consentido é permitido até 3 parceiros totais.
6. Amizade deve ser útil mesmo sem romance.
7. NPCs mantêm identidade própria, rotina, crenças, limites e preferências.
8. Presentes e escolhas respeitam religião, personalidade, trabalho e história do NPC.
9. Parceiros românticos/cônjuges podem virar companions futuros.
10. Parceiros românticos/cônjuges podem ajudar na fazenda futuramente.
11. Ajuda de parceiros não deve substituir o jogador, companions, pets, skill tree, ferramentas ou economia.
12. Save/load social deve ser explícito e estável.
```

---

## 4. Conceitos centrais

### 4.1 Relationship

Relationship é o estado social do jogador com um NPC.

Inclui:

```text
amizade;
confiança;
afeto;
respeito;
conflito;
romance;
casamento;
polycule membership;
bloqueios narrativos;
memória de eventos sociais importantes.
```

Não inclui:

```text
reputação econômica global;
faction standing global;
companion combat bond;
pet bond;
shop price isolado;
quest flags genéricas sem relação social.
```

### 4.2 Friendship

Friendship é o vínculo social base.

Serve para:

```text
destravar diálogos;
destravar pequenas cenas;
liberar pedidos pessoais;
melhorar resposta a presentes;
permitir visitas;
abrir possibilidade de romance para NPC elegível;
abrir possibilidade de companion unlock para parceiro/companions elegíveis.
```

Friendship não deve:

```text
substituir reputação;
dar dano direto;
dar stats permanentes fortes;
ser obrigatória para completar o jogo;
ser farmável infinitamente no mesmo dia.
```

### 4.3 Trust

Trust representa confiança prática.

Exemplos:

```text
NPC aceita falar sobre segredo;
NPC permite acesso a área pessoal;
NPC confia uma entrega;
NPC recomenda o jogador a outro NPC;
NPC aceita visitar a fazenda;
NPC aceita trabalhar como helper eventual;
NPC aceita se tornar companion futuro se elegível;
NPC aceita entrar em relação poliamorosa se sua rota permitir.
```

### 4.4 Affection

Affection representa afeição romântica ou proximidade emocional especial.

Só existe para NPCs elegíveis a romance ou romance tardio.

Não usar Affection para:

```text
crianças;
NPCs casados em casal fixo indisponível;
NPCs indisponíveis;
NPCs bloqueados por narrativa;
pets;
companions sem romance eligibility.
```

### 4.5 Polycule

Polycule é o conjunto de parceiros românticos/cônjuges do jogador quando houver relação poliamorosa.

Regra fechada:

```text
O jogador pode ter até 3 parceiros românticos/cônjuges simultâneos no máximo.
```

Regras de segurança de design:

```text
todos os envolvidos devem consentir pela rota narrativa;
NPCs podem recusar relação poliamorosa por personalidade, fé, história ou preferência;
casais fixos não são automaticamente disponíveis;
poliamor não é exploit de helper, ouro ou combat power;
limite de 3 é global, não por cidade/fazenda;
romance múltiplo não deve exigir mentira como mecânica base;
ciúme, se existir, deve ser narrativo, leve, claro e reparável.
```

### 4.6 Social Memory

Social Memory guarda fatos sociais relevantes.

Exemplos:

```text
primeiro presente dado;
presente favorito descoberto;
presente odiado descoberto;
primeiro convite aceito;
quest pessoal concluída;
pedido recusado;
romance iniciado;
romance encerrado;
casamento realizado;
polycule formado;
novo parceiro aceito pela relação atual;
parceiro recusou poliamor;
NPC visitou a fazenda;
NPC virou companion;
NPC ajudou na fazenda;
NPC conheceu pet ativo;
NPC reagiu à Fonte de Anya;
NPC reagiu a altar/deus relevante;
NPC participou de festival com o jogador.
```

---

## 5. Estados sociais

Estados base:

```text
Unknown
Known
Acquaintance
Friendly
CloseFriend
Trusted
RomanceAvailable
Dating
Committed
Married
PolyculePartner
Estranged
Blocked
```

### 5.1 Unknown

NPC existe no mundo, mas o jogador ainda não interagiu.

### 5.2 Known

O jogador conhece o NPC.

### 5.3 Acquaintance

Relação inicial regular.

### 5.4 Friendly

NPC reconhece o jogador positivamente.

### 5.5 CloseFriend

NPC compartilha temas pessoais.

### 5.6 Trusted

NPC confia no jogador.

Uso:

```text
serviços especiais;
rotas sociais;
segredos;
ajuda em situações futuras;
companion eligibility para NPCs aplicáveis;
romance eligibility para NPCs aplicáveis;
partner helper eligibility futura.
```

### 5.7 RomanceAvailable

NPC é elegível a romance e a relação atingiu pré-requisitos.

Pré-requisitos típicos:

```text
romance eligibility no roster;
Friendship mínima;
Trust mínima;
quest pessoal resolvida;
sem bloqueio narrativo ativo;
sem casamento fixo indisponível;
sem TooYoungOrNarrativelyBlocked;
limite de parceiros não excedido;
compatibilidade com monogamia/poliamor do NPC.
```

### 5.8 Dating

Jogador iniciou relacionamento romântico.

### 5.9 Committed

Relação romântica avançada, pré-casamento.

### 5.10 Married

Jogador casou com o NPC.

### 5.11 PolyculePartner

NPC participa de relação poliamorosa consentida com o jogador.

Pode coexistir com:

```text
Dating;
Committed;
Married.
```

### 5.12 Estranged

Relação foi prejudicada.

### 5.13 Blocked

Relação não pode avançar por regra sistêmica ou narrativa.

---

## 6. Elegibilidade de romance

A elegibilidade individual é declarada no roster de NPCs.

Estados esperados:

```text
RomanceEligibleAnyPlayerGender
UnavailableForRomance
MarriedToNpc
TooYoungOrNarrativelyBlocked
LateRomanceEligible
PolyCompatible
PolyBlocked
PartnerCompanionEligible
PartnerFarmHelperEligible
```

### 6.1 RomanceEligibleAnyPlayerGender

NPC pode se relacionar com o jogador independentemente do gênero escolhido pelo jogador.

Regra:

```text
Romance homoafetivo é permitido por padrão para NPC marcado assim.
O gênero do player não bloqueia romance.
```

### 6.2 UnavailableForRomance

NPC não é candidato romântico.

Ainda pode ter amizade, trust, quest pessoal, serviço especial, visita à fazenda e vínculo de respeito.

### 6.3 MarriedToNpc

NPC está em casal fixo.

Não pode ser romance do jogador, salvo se uma decisão futura alterar explicitamente o roster e a rota narrativa.

### 6.4 TooYoungOrNarrativelyBlocked

NPC está bloqueado por idade ou narrativa.

Regra:

```text
Não criar rota ambígua.
Não criar flerte.
Não criar presente romântico.
Não criar evento de dating.
```

### 6.5 LateRomanceEligible

NPC pode virar romance apenas após evento futuro.

### 6.6 PolyCompatible

NPC aceita relação poliamorosa se os pré-requisitos narrativos forem cumpridos.

### 6.7 PolyBlocked

NPC não aceita relação poliamorosa.

Não é falha do jogador; é identidade/preferência/limite do NPC.

### 6.8 PartnerCompanionEligible

NPC, quando parceiro romântico/cônjuge, pode ser desbloqueado como companion futuro.

### 6.9 PartnerFarmHelperEligible

NPC, quando parceiro romântico/cônjuge, pode ajudar na fazenda futuramente.

---

## 7. Candidatos atuais registrados no roster

O roster canônico atual declara como candidatos:

```text
Sylveth
Ozzra
Zrix
Yael
Thalindra
Dagna
Ser Alaric Veyr
Eiran Valeclaro
Liora Canta-Rio
Savra Escama-Verde
Maelor Cinza (romance tardio)
```

Casais fixos atuais:

```text
Nimble Galhobaixo + Mirela dos Laços
Gruta Panela-Funda + Orlan Pouso-Curto
Mara Vellum + Tovin Mãos-de-Selo
```

Este documento não altera essa lista.

Alterações individuais devem ocorrer no roster canônico.

---

## 8. Presentes e progressão por relacionamento

O sistema pode usar pontos/valores internos, mas o feedback ao jogador não deve ser uma planilha dominante.

Ganho de relação pode vir de:

```text
conversa diária;
presente relevante;
presente em aniversário/festival;
quest pessoal;
ajuda em trabalho do NPC;
escolha de diálogo;
escolha religiosa/cultural respeitosa;
evento de fazenda;
evento de caverna;
proteção/apoio em evento;
convite aceito;
participação em festival;
spouse/partner event;
companion event.
```

### 8.1 Categorias de preferência

Cada NPC pode ter:

```text
LovedGiftTags
LikedGiftTags
NeutralGiftTags
DislikedGiftTags
HatedGiftTags
ForbiddenGiftTags
RomanticGiftTags
PolyCommitmentGiftTags
MarriageGiftTags
```

Tags podem vir de:

```text
item category;
item material;
item origin;
item deity association;
item rarity;
item quality;
crafted/foraged/farmed/fished/mined/cave;
food;
flower;
book;
reagent;
tool;
weapon;
religious object;
Nyx/Anya/Senya/Kanthor/Thoren/Finan/etc.
```

### 8.2 Presentes proibidos

ForbiddenGiftTags ferem limite pessoal, dogma ou narrativa.

Quality não anula rejeição:

```text
Item odiado de alta qualidade continua odiado.
Item proibido de alta qualidade continua proibido.
Item amado de alta qualidade pode dar bônus controlado.
```

### 8.3 Limites de presente

Baseline futuro recomendado:

```text
1 presente social relevante por NPC por dia.
2 presentes sociais relevantes por NPC por semana.
Eventos especiais podem abrir exceção.
```

Os números finais são de balance futuro.

---

## 9. Diálogo social

O diálogo deve variar por:

```text
estado social;
horário;
season;
lua atual;
clima;
local;
quest pessoal;
favorito/ódio descoberto;
religião do NPC;
romance/dating/marriage/polycule;
pet ativo;
companion ativo;
progresso na caverna;
eventos de Anya/Nyx/Senya;
reputação local.
```

Camadas de diálogo:

```text
Greeting
DailySmallTalk
ScheduleContext
ServiceContext
GiftReaction
RelationshipMilestone
PersonalQuest
RomanceContext
PolyRelationshipContext
MarriageContext
FestivalContext
FarmVisitContext
PetReaction
CompanionReaction
DeityReaction
StoryProgressionReaction
```

---

## 10. Quests pessoais

Relacionamento alto deve desbloquear quests pessoais.

Tipos:

```text
pedido simples;
entrega;
coleta;
pesquisa;
craft específico;
visita a local;
evento de cidade;
evento de fazenda;
evento de caverna;
conciliação entre NPCs;
quest de romance;
quest de poliamor/aceite entre parceiros;
quest de casamento;
quest de partner companion unlock;
quest de partner helper unlock;
quest de reconciliação.
```

Regras:

```text
Quest pessoal não deve ser só fetch genérico.
Quest pessoal deve expressar o NPC.
Quest pessoal pode envolver profissão, fé, medo, sonho ou relação com outro NPC.
Quest pessoal não deve bloquear o core loop principal.
Quest pessoal pode dar conveniência, lore, item cosmético, serviço ou abertura social.
```

---

## 11. Romance

Romance deve ser uma rota opcional de aprofundamento narrativo.

Não deve ser:

```text
build obrigatória;
fonte superior de ouro;
fonte superior de stamina/HP/MP;
atalho obrigatório para companion forte;
obrigatório para final bom;
obrigatório para acessar cidade/caverna/fazenda.
```

Pode oferecer:

```text
diálogos;
eventos;
visitas;
presente especial;
cosmético;
partner helper limitado futuro;
companion unlock futuro;
pequenos buffs situacionais;
atalhos sociais;
final/epílogo personalizado;
lore pessoal.
```

### 11.1 Início de romance

Pré-requisitos futuros típicos:

```text
NPC elegível;
Friendship alta;
Trust mínimo;
quest pessoal inicial concluída;
presente ou item de intenção romântica;
evento de conversa aceito;
sem estado Estranged;
sem bloqueio narrativo;
limite global de parceiros respeitado.
```

### 11.2 Romance homoafetivo

Decisão fechada:

```text
Romance homoafetivo é permitido.
NPCs marcados como RomanceEligibleAnyPlayerGender podem se relacionar com o jogador independentemente do gênero escolhido.
```

### 11.3 Romance múltiplo / poliamor

Decisão fechada:

```text
O jogo permite relação poliamorosa consentida com até 3 parceiros românticos/cônjuges totais.
```

Regras:

```text
limite máximo global: 3 parceiros;
cada NPC pode aceitar ou recusar poliamor conforme perfil;
poliamor exige consentimento narrativo dos envolvidos;
poliamor não deve ser exploit de farm helper, companion power ou economia;
novos parceiros devem passar por evento/quest de aceite quando já houver parceiro ativo;
NPC PolyBlocked não entra em polycule;
NPC PolyCompatible pode entrar se pré-requisitos forem cumpridos.
```

### 11.4 Ciúme e conflito

Ciúme não deve ser sistema punitivo amplo no baseline.

Se existir futuramente, deve ser:

```text
leve;
narrativo;
limitado;
transparente;
reparável;
não obrigatório;
não abusivo;
não necessário para o core loop.
```

---

## 12. Casamento

Casamento é estado social avançado e opcional.

Pré-requisitos futuros típicos:

```text
Dating ou Committed;
Friendship muito alta;
Trust alto;
quest pessoal principal concluída;
item/ritual de compromisso;
casa/fazenda com condição mínima;
cerimônia ou evento social;
aceite explícito do NPC;
compatibilidade com relação atual do jogador.
```

### 12.1 Casamento poliamoroso

Decisão fechada:

```text
Casamento poliamoroso consentido é permitido até o limite de 3 parceiros totais.
```

Regras:

```text
cada parceiro precisa aceitar a estrutura;
cada parceiro mantém identidade, rotina, limites e preferências;
a cerimônia pode ser individual ou coletiva, conforme rota futura;
nenhum parceiro deve ser tratado como subordinado;
benefícios de helper devem usar orçamento global para evitar exploit.
```

### 12.2 Parceiros mantêm agência

NPC parceiro/cônjuge mantém:

```text
nome;
personalidade;
fé;
gostos;
limites;
rotina;
trabalho;
relações com outros NPCs;
preferências de local;
reações próprias.
```

---

## 13. Parceiros como companions

Decisão fechada:

```text
Parceiros românticos/cônjuges podem se tornar companions futuros quando forem elegíveis.
```

Regras:

```text
romance/casamento pode desbloquear rota de companion para NPC elegível;
companion unlock ainda exige perfil e quest apropriada;
ser parceiro não deve ignorar balance de companion;
ser parceiro não remove companion injury/recovery, leash, cave risk ou limites de combate;
NPC não combatente pode ser farm helper sem virar cave companion;
NPC combatente pode virar cave companion se tiver PartnerCompanionEligible.
```

Conflito resolvido com caverna:

```text
Mesmo com até 3 parceiros, o baseline atual da caverna continua 1 companion ativo por run.
Os 3 parceiros podem estar desbloqueados como companions, mas apenas 1 acompanha a caverna por vez até uma decisão futura mudar o party size.
```

---

## 14. Parceiros ajudando na fazenda

Decisão fechada:

```text
Parceiros românticos/cônjuges podem ajudar na fazenda futuramente.
```

Ajuda possível:

```text
regar poucas crops;
alimentar animais;
coletar produto simples;
preparar comida ocasional;
entregar item social;
dar hint de cidade;
dar hint de pet;
ajudar em craft leve se fizer sentido para o NPC;
acompanhar evento de fazenda;
executar pequena rotina baseada na classe funcional do NPC.
```

Limites:

```text
não automatizar a fazenda inteira;
não substituir companions;
não substituir pets;
não substituir skill tree;
não substituir crafting progression;
não resolver economia;
não executar caverna sozinho;
não ser fonte superior de ouro;
não quebrar stamina/fome/cansaço do jogador.
```

### 14.1 Orçamento global de ajuda

Para evitar exploit com até 3 parceiros:

```text
partner helper deve ter orçamento global por dia/semana;
mais parceiros aumentam variedade e cobertura narrativa, não multiplicam linearmente produção;
helper actions devem ser limitadas por perfil, clima, humor, schedule e anchors;
se 3 parceiros ajudarem, cada um executa contribuição pequena ou alternada.
```

---

## 15. Visitas à fazenda

Tipos:

```text
visita casual;
visita por amizade;
visita por quest;
visita por romance;
visita por casamento;
visita por polycule;
visita de serviço;
visita de festival;
visita por pet;
visita por companion;
visita por evento de Anya/Fonte;
visita por clima/lua/season.
```

Specs futuras de farm layout devem reservar anchors conceituais para:

```text
visitor spawn;
visitor idle point;
visitor social talk point;
partner idle point;
partner helper start point;
polycule shared idle point;
pet interaction point;
companion interaction point;
festival temporary point;
Fonte de Anya reaction point.
```

---

## 16. Relação com cidade

Specs atuais de cidade devem preparar, sem implementar runtime social:

```text
NPC IDs estáveis;
residências;
beds;
schedule markers;
service markers;
talk interactable;
quest board hooks;
festival anchors;
visitor route anchors;
romance eligibility vindo do roster;
poly compatibility hook futuro;
partner companion eligibility hook futuro;
partner farm helper eligibility hook futuro;
religion fields;
sem Friendship/Gift/Romance runtime completo.
```

---

## 17. Relação com fazenda

Specs atuais de fazenda devem preparar, sem implementar runtime social:

```text
visitor spawn anchor;
visitor idle anchor;
partner helper anchor;
polycule shared anchor;
Fonte de Anya reaction anchor;
farmhouse expansion hook;
mailbox/board hook;
pet/social interaction area;
sem partner helper runtime.
```

---

## 18. Relação com companions

Relationship e CompanionBond são relacionados, mas não iguais.

```text
Relationship = vínculo social geral com NPC.
CompanionBond = vínculo funcional com companion ativo/potencial.
Romance = rota opcional para NPC elegível.
Marriage = estado social avançado opcional.
Polycule = conjunto de até 3 parceiros românticos/cônjuges consentidos.
```

Regra:

```text
romance/casamento pode abrir rota de companion para parceiros elegíveis;
companion unlock não deve obrigar romance por padrão;
active cave companion baseline continua 1.
```

---

## 19. Relação com pets

Pets têm bond próprio.

Pet bond não é Relationship.

Interações sociais possíveis:

```text
NPC gosta do pet ativo;
NPC teme o pet ativo;
NPC dá comida/brinquedo ao pet;
NPC vende item de pet;
NPC comenta pet na fazenda;
partner interage com pet;
pet reage a visitante;
pet reage a múltiplos parceiros na fazenda.
```

Não implementar romance/pet em qualquer forma.

---

## 20. Religião, deuses e preferências

Religião pessoal do NPC deve afetar relação.

Campos conceituais:

```text
PrimaryDeity
LikedDeities
DistrustedDeities
ForbiddenDeityTags
SacredGiftTags
OffensiveGiftTags
FestivalPreferenceTags
AltarReactionRules
FonteAnyaReactionRule
NyxMoonReactionRule
SenyaMoonReactionRule
AlihanaMoonReactionRule
PolyRelationshipRule
MarriageRitePreference
```

---

## 21. Data assets esperados

Specs futuras podem criar assets como:

```text
RelationshipProfileSO
NpcRelationshipState
GiftPreferenceProfileSO
GiftReactionRuleSO
SocialMemoryFlagSO
DialogueConditionSO
DialogueLineSO
RomanceRouteProfileSO
PolyRelationshipProfileSO
MarriageProfileSO
PartnerCompanionProfileSO
PartnerHelperProfileSO
FarmVisitProfileSO
SocialQuestProfileSO
FestivalSocialProfileSO
SocialBalanceProfileSO
SocialHUDProfileSO
```

### 21.1 RelationshipProfileSO

Campos conceituais:

```text
NpcId
InitialState
RomanceEligibility
PolyCompatibility
PartnerCompanionEligibility
PartnerFarmHelperEligibility
FriendshipThresholds
TrustThresholds
AffectionThresholds
GiftPreferenceProfileId
RomanceRouteProfileId
PolyRelationshipProfileId
MarriageProfileId
PartnerCompanionProfileId
PartnerHelperProfileId
FarmVisitProfileId
PersonalQuestIds
DialogueConditionSetIds
ForbiddenSocialActions
```

### 21.2 NpcRelationshipState

Campos conceituais de save:

```text
NpcId
RelationshipState
FriendshipValue
TrustValue
AffectionValue
IsKnown
IsDating
IsCommitted
IsMarried
IsPolyculePartner
IsEstranged
PartnerSlotIndex
PolyculeId
LastGiftDay
GiftsGivenThisWeek
DiscoveredLovedGiftTags
DiscoveredHatedGiftTags
SocialMemoryFlags
CompletedSocialQuestIds
ActiveSocialQuestIds
LastTalkDay
LastVisitDay
IsPartnerCompanionUnlocked
IsPartnerHelperUnlocked
```

### 21.3 PolyRelationshipProfileSO

Campos conceituais:

```text
MaxPartners = 3
RequiresConsentEvent
RequiresExistingPartnerApproval
AllowedNpcIds
BlockedNpcIds
PolyCompatibilityTags
JealousyRules
ConflictResolutionQuestIds
SharedCeremonyRules
SharedFarmVisitRules
SharedHelperBudgetRules
```

### 21.4 PartnerHelperProfileSO

Campos conceituais:

```text
NpcId
AllowedHelperActions
BlockedHelperActions
WeeklyHelperFrequency
HelperActionBudget
SharedPolyculeHelperBudget
WeatherRules
SeasonRules
RelationshipMoodRules
RequiredFarmAnchors
DialogueBeforeAction
DialogueAfterAction
```

---

## 22. Save/load conceitual

Social save deve persistir valores simples e IDs estáveis.

Persistir:

```text
NpcId;
RelationshipState;
FriendshipValue;
TrustValue;
AffectionValue;
booleans de dating/committed/married/polycule/estranged;
PolyculeId;
PartnerSlotIndex;
SocialMemoryFlags;
Gift discovery;
quest social state;
partner companion unlock;
partner helper unlock;
last gift/talk/visit day;
partner visit state.
```

Não persistir:

```text
referências Unity;
GameObject;
Transform;
ScriptableObject direto;
preço social calculado;
texto de diálogo resolvido;
NPC runtime object;
pathfinding state transitório.
```

Ordem conceitual de load futuro:

```text
1. carregar registries de NPCs e relationship profiles;
2. carregar save social bruto;
3. validar NpcIds;
4. validar limite de 3 parceiros;
5. aplicar relationship state;
6. aplicar SocialMemoryFlags;
7. resolver partner companion/helper unlock;
8. resolver schedule/scene atual;
9. instanciar NPCs conforme cena;
10. aplicar diálogo/ícones/availability;
11. aplicar farm visit/partner helper state apenas se cena permitir.
```

---

## 23. HUD e feedback social

O sistema social precisa comunicar:

```text
ícone de relação no social log
reação a presente
diálogo novo
quest pessoal
convite
evento de calendário
visita
partner helper
preferências descobertas
status de parceiro
status de polycule
```

Social log, ícones, notificações, layout, feedback social, calendário e apresentação de estado social são definidos em:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

---

## 24. Balance e anti-exploit

Riscos:

```text
farm infinito de presentes;
comprar item barato e converter em relação alta;
presentear item produzido em massa sem limite;
casamento usado como automação superior;
3 parceiros multiplicarem produção linearmente;
partner helper superando companions;
romance virando requisito de poder;
parceiro-companion superando companions normais;
relação social quebrando economia de loja;
reset de save para rerollar reação;
visitas bloqueando pathing/trabalho;
NPC abandonando serviço essencial por evento social.
```

Regras anti-exploit:

```text
limite diário/semanal de presente relevante;
diminishing returns para spam;
memória de presente odiado/proibido;
partner helper com orçamento global;
3 parceiros aumentam variedade, não produção linear;
romance/casamento sem bônus permanente forte;
presentes comprados em loja não devem quebrar progressão;
qualidade não anula preferência negativa;
serviços essenciais não devem ficar indisponíveis sem fallback;
eventos sociais devem respeitar schedule e prioridade;
active cave companion baseline continua 1.
```

---

## 25. Preparação para specs atuais sem implementação social

Specs executáveis próximas devem considerar este documento apenas como restrição de compatibilidade futura.

### 25.1 Cidade

Preparar:

```text
NpcId estável;
residência;
schedule marker;
service marker;
talk anchor;
festival anchor;
farm visit eligibility hook;
romance eligibility vindo do roster;
poly compatibility hook futuro;
partner companion/helper hooks futuros;
religion fields;
sem runtime social completo.
```

### 25.2 Fazenda

Preparar:

```text
visitor spawn anchor;
visitor idle anchor;
partner helper anchor;
polycule shared anchor;
Fonte de Anya reaction anchor;
farmhouse expansion hook;
mailbox/board hook;
pet/social interaction area;
sem partner helper runtime.
```

### 25.3 Companions

Preparar:

```text
separação entre Relationship e CompanionBond;
companion unlock por Trust/Friendship futuro sem romance obrigatório;
partner companion eligibility hook;
active companion independente de spouse/partner count;
sem romance runtime.
```

### 25.4 Pets

Preparar:

```text
pet bond separado;
NPC reaction hooks;
partner-pet interaction hook futuro;
sem social romance runtime.
```

### 25.5 Save/load

Não implementar social state incompleto.

Se necessário, reservar versão/schema futuro:

```text
SocialSaveData: deferred
```

Não criar campos parciais sem uso.

---

## 26. Roadmap de specs futuras

Specs futuras sugeridas, não atuais:

```text
spec_social_relationship_profile_contract_future.md
spec_social_friendship_trust_runtime_future.md
spec_social_gift_preferences_reactions_future.md
spec_social_dialogue_conditions_memory_future.md
spec_social_personal_quests_future.md
spec_social_romance_route_runtime_future.md
spec_social_poly_relationship_runtime_future.md
spec_social_marriage_ceremony_state_future.md
spec_social_partner_companion_unlock_future.md
spec_social_partner_helper_farm_runtime_future.md
spec_social_farm_visits_runtime_future.md
spec_social_festivals_dates_birthdays_future.md
spec_social_hud_log_feedback_future.md
spec_social_save_load_state_future.md
spec_social_balance_anti_exploit_future.md
```

Specs atuais que devem ficar preparadas, mas sem implementar social runtime:

```text
spec_city_scene_tilemap_collision_spawns.md
spec_city_buildings_exteriors_and_doors.md
spec_city_core_interiors_shops_services.md
spec_city_npc_residences_beds_schedule_markers.md
spec_city_npc_data_roster_stats.md
spec_city_npc_pathfinding_waypoints.md
spec_city_props_interactables_calendar_boards.md
spec_city_relationship_romance_marriage.md  # mover para futuro se a leva atual for execução imediata
spec_city_farm_visits_schedule_hooks.md     # mover para futuro se a leva atual for execução imediata
spec_farm_layout_expansion_zones_free_build.md
spec_farm_pets_dog_cat_bond_buffs.md
spec_farm_companion_jobs_automation.md
spec_companion_unlock_availability_runtime.md
spec_companion_bond_progression_runtime.md
spec_pet_bond_mood_energy_runtime.md
spec_pet_follow_home_routine_runtime.md
```

---

## 27. Decisões fechadas

```text
Relacionamento/romance/casamento é feature futura, não entra na execução atual.
O documento existe para preparar dependências e evitar bloqueios.
Romance é opcional.
Casamento é opcional.
Romance homoafetivo é permitido.
NPCs RomanceEligibleAnyPlayerGender aceitam jogador de qualquer gênero.
Casamento poliamoroso consentido é permitido até 3 parceiros totais.
Cada NPC pode aceitar ou recusar poliamor conforme perfil.
Todos os envolvidos em poliamor precisam consentir por rota narrativa.
Amizade deve ter valor mesmo sem romance.
NPCs casados fixos, jovens demais ou bloqueados narrativamente não têm rota romântica por padrão.
Parceiros românticos/cônjuges podem virar companions futuros se elegíveis.
Parceiros românticos/cônjuges podem ajudar na fazenda futuramente.
Mesmo com 3 parceiros, baseline de caverna continua 1 companion ativo por run.
Partner helper é futuro, limitado e não substitui companions, pets, skill tree, ferramentas ou execução do jogador.
Partner helper usa orçamento global para evitar multiplicação linear de produção.
CompanionBond não é igual a Relationship.
PetBond não é Relationship.
NPC roster vence para elegibilidade individual.
Cidade/fazenda devem reservar hooks e anchors, mas não implementar social runtime agora.
Save/load social deve usar IDs e valores simples, nunca referências Unity.
```

---

## 28. Pendências abertas / validações futuras

```text
Definir valores finais de Friendship/Trust/Affection.
Definir thresholds por estado social.
Definir se haverá aniversários.
Definir se haverá bouquet/item de namoro.
Definir item/ritual de casamento.
Definir cerimônia monogâmica e poliamorosa.
Definir quais NPCs são PolyCompatible e PolyBlocked.
Definir quais NPCs são PartnerCompanionEligible.
Definir quais NPCs são PartnerFarmHelperEligible.
Definir se NPC casado muda para fazenda ou mantém rotina híbrida.
Definir lista de loved/liked/disliked/hated gifts por NPC.
Definir social questline de cada candidato.
Definir partner helper por NPC.
Definir orçamento global de helper para 1, 2 e 3 parceiros.
Definir reação de cada NPC à Fonte de Anya.
Definir reação de cada NPC às luas Alihana/Senya/Nyx.
Definir como festivais sociais interagem com relação.
Definir se amizade afeta preços apenas via reputação/economia ou por regra social separada.
Atualizar SPEC_SOURCE_MAP.md para incluir este documento como fonte canônica futura quando for seguro preservar o arquivo inteiro.
```
