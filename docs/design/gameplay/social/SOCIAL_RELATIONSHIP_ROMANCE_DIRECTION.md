# Cindar's Hope — Social Relationship & Romance Direction

> **Status:** documento canônico de direção futura de relacionamento, amizade, romance e casamento  
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
> **Função:** preparar a arquitetura de design para vínculos sociais, romance, casamento e visitas sem forçar implementação imediata.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados/runtime/UI/Unity.

---

## 0. Regra anti-duplicação

Este documento define a direção do **sistema social** do jogo.

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
  vence para companion eligibility, active companion, companion jobs, cave follow/leash, downed/recovery e companion bond funcional.

PETS_DIRECTION.md
  vence para pet bond, pet mood/energy, pet home area, pet food, pet toys, pet follow/leash e pet hints.

ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
  vence para BaseValue, preço, estoque, restock, loja, SellPoint e anti-arbitragem.
```

Este documento vence apenas quando a decisão for sobre:

```text
amizade;
vínculo social;
presentes sociais;
diálogo por relação;
romance;
casamento;
spouse helper;
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
preparar as próximas features para não contradizerem relacionamento/romance/casamento;
garantir que cidade, NPCs, companions, pets, fazenda, calendário, UI e save deixem hooks corretos;
evitar que specs atuais fechem portas técnicas ou narrativas;
definir o que deve ficar reservado para implementação futura.
```

Durante a próxima leva de specs executáveis, esta direção deve ser usada apenas para:

```text
não bloquear NPC relationship state futuro;
não usar campos incompatíveis;
não transformar romance/casamento em caminho obrigatório de poder;
não acoplar spouse helper em farm automation atual;
não misturar companion bond com romance sem regra explícita;
não misturar pet bond com social romance;
não persistir estado social de forma incompleta que vire dívida técnica.
```

Não implementar agora:

```text
RelationshipSystem runtime;
Friendship UI;
gift runtime completo;
romance cutscenes;
marriage ceremony;
spouse moving to farm;
spouse helper automation;
jealousy runtime;
social questline runtime;
festivals sociais completos;
dating state;
divorce/separation;
children/family system.
```

---

## 2. Objetivo de produto

O sistema social deve fazer a cidade parecer habitada, reativa e emocionalmente consistente sem transformar vínculos sociais em obrigação mecânica.

Pilares:

```text
1. Relações dão contexto, histórias, desbloqueios e conveniência, não poder obrigatório.
2. Romance é opcional.
3. Casamento é opcional.
4. Amizade deve ser útil mesmo sem romance.
5. NPCs devem manter identidade própria, rotina, crenças, limites e preferências.
6. O jogador não deve otimizar relações como uma planilha sem feedback diegético.
7. Presentes e escolhas devem respeitar religião, personalidade, trabalho e história do NPC.
8. Spouse helper ajuda, mas não joga pelo jogador.
9. O sistema deve preservar coerência com companions e pets.
10. Save/load social deve ser explícito e estável.
```

---

## 3. Conceitos centrais

### 3.1 Relationship

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

### 3.2 Friendship

Friendship é o vínculo social base.

Serve para:

```text
destravar diálogos;
destravar pequenas cenas;
liberar pedidos pessoais;
melhorar resposta a presentes;
permitir visitas;
criar pequenas conveniências;
abrir possibilidade de romance para NPC elegível.
```

Friendship não deve:

```text
substituir reputação;
dar dano direto;
dar stats permanentes fortes;
ser obrigatória para completar o jogo;
ser farmável infinitamente no mesmo dia.
```

### 3.3 Trust

Trust representa confiança prática.

Exemplos:

```text
NPC aceita falar sobre segredo;
NPC permite acesso a área pessoal;
NPC confia uma entrega;
NPC recomenda o jogador a outro NPC;
NPC aceita visitar a fazenda;
NPC aceita trabalhar como helper eventual.
```

Trust pode subir por:

```text
quest pessoal;
ajuda em evento;
presente coerente;
diálogo alinhado com valores;
serviço prestado;
respeito a tabu religioso ou pessoal.
```

Trust pode cair por:

```text
quebrar promessa;
entregar item odiado repetidamente;
escolha contra dogma central do NPC;
ignorar evento pessoal importante;
agir contra alguém amado pelo NPC.
```

### 3.4 Affection

Affection representa afeição romântica ou proximidade emocional especial.

Só deve existir para NPCs elegíveis a romance ou romance tardio.

Não usar Affection para:

```text
crianças;
NPCs casados;
NPCs indisponíveis;
NPCs bloqueados por narrativa;
pets;
companions sem romance eligibility.
```

### 3.5 Social Memory

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
NPC visitou a fazenda;
NPC conheceu pet ativo;
NPC reagiu à Fonte de Anya;
NPC reagiu a altar/deus relevante;
NPC participou de festival com o jogador.
```

Social Memory deve ser usada para evitar diálogos repetitivos e permitir reatividade.

---

## 4. Estados sociais

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
Estranged
Blocked
```

### 4.1 Unknown

NPC existe no mundo, mas o jogador ainda não interagiu.

### 4.2 Known

O jogador conhece o NPC.

Uso:

```text
aparece no social log;
nome e função ficam visíveis;
presentes ainda têm resposta simples.
```

### 4.3 Acquaintance

Relação inicial regular.

Uso:

```text
diálogos básicos;
serviços normais;
presentes aceitos com efeito limitado.
```

### 4.4 Friendly

NPC reconhece o jogador positivamente.

Uso:

```text
diálogos pessoais leves;
pequenos pedidos;
comentários sobre cidade/fazenda;
primeiros hints de preferência.
```

### 4.5 CloseFriend

NPC compartilha temas pessoais.

Uso:

```text
quests pessoais;
visitas ocasionais;
convites para eventos;
comentários sobre outros NPCs;
reação mais forte a presentes relevantes.
```

### 4.6 Trusted

NPC confia no jogador.

Uso:

```text
serviços especiais;
rotas sociais;
segredos;
ajuda em situações futuras;
companion eligibility para NPCs aplicáveis;
romance eligibility para NPCs aplicáveis.
```

### 4.7 RomanceAvailable

NPC é elegível a romance e a relação atingiu pré-requisitos.

Pré-requisitos típicos:

```text
romance eligibility no roster;
Friendship mínima;
Trust mínima;
quest pessoal resolvida;
sem bloqueio narrativo ativo;
sem casamento fixo;
sem TooYoungOrNarrativelyBlocked.
```

### 4.8 Dating

Jogador iniciou relacionamento romântico.

Uso:

```text
diálogos românticos;
convites especiais;
eventos pessoais;
limites de presentes/atenção;
possível ciúme leve se o jogo decidir usar esse sistema futuramente.
```

### 4.9 Committed

Relação romântica avançada, pré-casamento.

Uso:

```text
pedido de casamento futuro;
quest de compromisso;
preparação de cerimônia;
resolução de conflito pessoal.
```

### 4.10 Married

Jogador casou com o NPC.

Uso:

```text
spouse rotina especial;
visitas ou residência na fazenda;
spouse helper futuro;
diálogos de vida conjunta;
reações a pets, companions e Fonte de Anya;
pequenos benefícios de conveniência.
```

### 4.11 Estranged

Relação foi prejudicada.

Uso:

```text
diálogos frios;
serviços sociais bloqueados;
romance pausado;
quest de reconciliação futura.
```

### 4.12 Blocked

Relação não pode avançar por regra sistêmica ou narrativa.

Exemplos:

```text
NPC casado;
NPC jovem demais;
NPC indisponível por dogma/narrativa;
NPC antagonista;
NPC morto/desaparecido;
NPC bloqueado por capítulo futuro.
```

---

## 5. Elegibilidade de romance

A elegibilidade individual é declarada no roster de NPCs.

Estados esperados:

```text
RomanceEligibleAnyPlayerGender
UnavailableForRomance
MarriedToNpc
TooYoungOrNarrativelyBlocked
LateRomanceEligible
```

### 5.1 RomanceEligibleAnyPlayerGender

NPC pode se relacionar com o jogador independentemente do gênero escolhido.

Regra:

```text
O gênero do player não bloqueia romance com NPC marcado assim.
```

### 5.2 UnavailableForRomance

NPC não é candidato romântico.

Ainda pode ter:

```text
amizade;
trust;
quest pessoal;
serviço especial;
visita à fazenda;
comentários reativos;
vínculo de respeito.
```

### 5.3 MarriedToNpc

NPC está em casal fixo.

Não pode ser romance do jogador.

Pode ter:

```text
amizade alta;
quest de casal;
serviços familiares;
visitas em dupla;
reações a presentes para o cônjuge.
```

### 5.4 TooYoungOrNarrativelyBlocked

NPC está bloqueado por idade ou narrativa.

Regra:

```text
Não criar rotas ambíguas.
Não criar flerte.
Não criar presente romântico.
Não criar evento de dating.
```

### 5.5 LateRomanceEligible

NPC pode virar romance apenas após evento futuro.

Uso:

```text
arco narrativo;
resolução de trauma;
revelação de memória;
capítulo de história;
mudança de cidade;
liberação por quest pessoal.
```

Não implementar como romance disponível no início.

---

## 6. Candidatos atuais registrados no roster

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

Qualquer alteração futura deve ocorrer no roster canônico e este documento deve apenas refletir regras sistêmicas.

---

## 7. Presentes

### 7.1 Papel de presentes

Presentes são uma forma de comunicar atenção, não o único caminho de relacionamento.

Presentes devem:

```text
revelar preferências;
reforçar personalidade;
conectar cidade, fazenda, caverna e crafting;
ser limitados para evitar grind diário;
gerar feedback claro;
ter memória social.
```

Presentes não devem:

```text
substituir quests pessoais;
permitir casamento sem relação narrativa;
ser a forma dominante de progressão;
dar ganhos infinitos por spam;
criar economia quebrada.
```

### 7.2 Categorias de preferência

Cada NPC pode ter:

```text
LovedGiftTags
LikedGiftTags
NeutralGiftTags
DislikedGiftTags
HatedGiftTags
ForbiddenGiftTags
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

### 7.3 Presentes proibidos

ForbiddenGiftTags não são apenas presentes ruins; são presentes que ferem limite pessoal, dogma ou narrativa.

Exemplos:

```text
item de Nyx para NPC devoto de Kanthor, se o contexto for ofensivo;
item profano para curandeiro de Anya;
arma de guerra para NPC pacifista;
item roubado para NPC legalista;
produto animal para NPC com tabu específico;
artefato de caverna corrompido para NPC sensível a corrupção.
```

ForbiddenGift pode:

```text
bloquear ganho social;
reduzir trust;
disparar diálogo único;
registrar SocialMemory;
abrir quest de reparação;
impedir presente adicional por período.
```

### 7.4 Qualidade do item

Quality pode modificar efeito social, mas não deve quebrar limites.

Regra:

```text
Item odiado de alta qualidade continua odiado.
Item proibido de alta qualidade continua proibido.
Item amado de alta qualidade pode dar bônus controlado.
```

### 7.5 Limites de presente

Baseline futuro recomendado:

```text
1 presente social relevante por NPC por dia.
2 presentes sociais relevantes por NPC por semana.
Presentes adicionais no mesmo período geram diálogo, mas não ganho pleno.
Eventos especiais podem abrir exceção.
```

Os números finais são de balance futuro.

---

## 8. Diálogo social

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
romance/dating/marriage;
pet ativo;
companion ativo;
progresso na caverna;
eventos de Anya/Nyx/Senya;
reputação local.
```

### 8.1 Camadas de diálogo

```text
Greeting
DailySmallTalk
ScheduleContext
ServiceContext
GiftReaction
RelationshipMilestone
PersonalQuest
RomanceContext
MarriageContext
FestivalContext
FarmVisitContext
PetReaction
CompanionReaction
DeityReaction
StoryProgressionReaction
```

### 8.2 Regra de repetição

O sistema deve evitar repetir falas especiais.

Falas únicas devem gravar SocialMemory.

Exemplo:

```text
npc_sylveth_first_farm_visit_seen
npc_yael_first_nyx_moon_comment_seen
npc_corvus_first_anya_fountain_reaction_seen
npc_dagna_first_mana_ore_warning_seen
```

---

## 9. Quests pessoais

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
quest de casamento;
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

## 10. Romance

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
spouse helper limitado futuro;
pequenos buffs situacionais;
atalhos sociais;
final/epílogo personalizado;
lore pessoal.
```

### 10.1 Início de romance

Pré-requisitos futuros típicos:

```text
NPC elegível;
Friendship alta;
Trust mínimo;
quest pessoal inicial concluída;
presente ou item de intenção romântica;
evento de conversa aceito;
sem estado Estranged;
sem bloqueio narrativo.
```

### 10.2 Dating

Durante Dating:

```text
NPC mantém rotina própria;
NPC não abandona profissão;
NPC pode visitar a fazenda ocasionalmente;
diálogos especiais aparecem;
eventos de romance podem ser agendados;
presentes românticos ganham contexto;
outros NPCs podem comentar, mas sem punição sistêmica pesada no baseline.
```

### 10.3 Romance múltiplo

Decisão base:

```text
Não definir romance múltiplo como sistema agora.
```

Para specs futuras:

```text
não assumir monogamia sistêmica rígida no core data;
não implementar ciúme punitivo agora;
reservar campo para relationship exclusivity rule;
deixar decisão de design para spec futura específica.
```

### 10.4 Ciúme e conflito

Ciúme não deve ser sistema punitivo amplo no baseline.

Se existir futuramente, deve ser:

```text
leve;
narrativo;
limitado;
transparente;
reparável;
não abusivo;
não necessário para o core loop.
```

---

## 11. Casamento

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
aceite explícito do NPC.
```

### 11.1 Casamento não é domínio do jogador

NPC casado com o jogador mantém:

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

### 11.2 Residência

Possibilidades futuras:

```text
NPC se muda para a fazenda;
NPC alterna cidade/fazenda;
NPC mantém casa na cidade e visita a fazenda;
NPC só visita em dias definidos por narrativa.
```

A decisão pode variar por NPC.

Não assumir que todos os cônjuges seguem o mesmo padrão.

### 11.3 Spouse helper

Spouse helper é ajuda limitada e contextual.

Pode ajudar em:

```text
regar poucas crops;
alimentar animais;
coletar produto simples;
preparar comida ocasional;
entregar item social;
dar hint de cidade;
dar hint de pet;
ajudar em craft leve se fizer sentido para o NPC;
acompanhar evento de fazenda.
```

Não pode:

```text
automatizar a fazenda inteira;
substituir companions;
substituir pets;
substituir skill tree;
substituir crafting progression;
resolver economia;
executar caverna sozinho;
ser fonte superior de ouro;
quebrar stamina/fome/cansaço do jogador.
```

### 11.4 Casamento e companions

Um NPC pode ser:

```text
apenas spouse;
apenas companion;
spouse e companion, se o roster e companions direction permitirem;
spouse com helper farm, mas sem cave companion;
companion sem romance.
```

Regra:

```text
romance/casamento não deve transformar automaticamente um NPC em companion de caverna.
companion unlock não deve obrigar romance.
```

---

## 12. Visitas à fazenda

Visitas sociais à fazenda são hooks importantes para preparar specs futuras.

Tipos:

```text
visita casual;
visita por amizade;
visita por quest;
visita por romance;
visita por casamento;
visita de serviço;
visita de festival;
visita por pet;
visita por companion;
visita por evento de Anya/Fonte;
visita por clima/lua/season.
```

### 12.1 Regras de visita

Visita deve respeitar:

```text
schedule do NPC;
weather;
season;
fase do relacionamento;
quest state;
local seguro na fazenda;
spawn point válido;
pathing possível;
horário de retorno;
colisão;
prioridade de evento.
```

### 12.2 Anchors na fazenda

Specs futuras de farm layout devem reservar anchors conceituais para:

```text
visitor spawn;
visitor idle point;
visitor social talk point;
spouse idle point;
spouse helper start point;
pet interaction point;
companion interaction point;
festival temporary point;
Fonte de Anya reaction point.
```

Não implementar o runtime social agora, mas não bloquear esses anchors.

---

## 13. Relação com cidade

Cidade deve suportar social future hooks.

Specs atuais de cidade devem evitar:

```text
NPC sem ID estável;
NPC sem residência ou schedule mínimo;
NPC sem local de serviço;
NPC sem estado social futuro;
NPC sem religião/preferências básicas;
NPC sem anchor de diálogo;
NPC sem tag de romance eligibility;
NPC sem possibilidade de social memory futura.
```

Specs de cidade executáveis agora podem preparar:

```text
NPC IDs;
residências;
beds;
schedule markers;
shop/service markers;
talk interactable;
quest board hooks;
festival anchors;
visitor route anchors.
```

Mas não devem implementar:

```text
Friendship runtime;
gift runtime;
romance runtime;
marriage runtime;
spouse helper;
cutscenes sociais.
```

---

## 14. Relação com fazenda

Fazenda deve suportar social future hooks.

Specs executáveis atuais de fazenda devem evitar:

```text
layout que impeça visitor spawn;
spouse helper acoplado à automação básica;
pet home area incompatível com visitas;
Fonte de Anya sem espaço para reação de NPC;
shipping/processing que assuma nenhum NPC interage socialmente;
farmhouse sem expansão social futura.
```

Podem preparar:

```text
social anchor vazio;
visitor spawn placeholder;
farmhouse upgrade hook;
pet interaction area;
Fonte de Anya reaction point;
board/mailbox hook para convites.
```

Não implementar runtime social agora.

---

## 15. Relação com companions

Companion bond e social relationship são sistemas relacionados, mas não iguais.

```text
Relationship = vínculo social geral com NPC.
CompanionBond = vínculo funcional com companion ativo/potencial.
Romance = rota opcional para NPC elegível.
Marriage = estado social avançado opcional.
```

Um companion pode exigir:

```text
Friendship mínima;
Trust mínimo;
quest pessoal;
reputação;
serviço contratado;
história principal;
resgate;
evento de cidade/caverna.
```

Mas companion não deve exigir romance por padrão.

---

## 16. Relação com pets

Pets têm bond próprio.

Pet bond não é Relationship.

Interações sociais possíveis:

```text
NPC gosta do pet ativo;
NPC teme o pet ativo;
NPC dá comida/brinquedo ao pet;
NPC vende item de pet;
NPC comenta pet na fazenda;
spouse interage com pet;
pet reage a visitante.
```

Não implementar romance/pet em qualquer forma.

---

## 17. Religião, deuses e preferências

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
```

Exemplos de uso:

```text
NPC devoto de Kanthor reage bem a justiça, contratos honrados e atos de proteção.
NPC ligado a Finan valoriza sorte, estrada, humor e oportunidade.
NPC ligado a Thoren valoriza trabalho, forja, pedra, compromisso e ferramentas.
NPC ligado a Nyx pode reagir de forma complexa a segredo, noite, morte, silêncio e memória.
NPC ligado a Anya ou à Fonte deve ter reação especial a cura, esperança e Água Viva.
```

---

## 18. Data assets esperados

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
MarriageProfileSO
SpouseHelperProfileSO
FarmVisitProfileSO
SocialQuestProfileSO
FestivalSocialProfileSO
SocialBalanceProfileSO
SocialHUDProfileSO
```

### 18.1 RelationshipProfileSO

Campos conceituais:

```text
NpcId
InitialState
RomanceEligibility
FriendshipThresholds
TrustThresholds
AffectionThresholds
GiftPreferenceProfileId
RomanceRouteProfileId
MarriageProfileId
FarmVisitProfileId
PersonalQuestIds
DialogueConditionSetIds
ForbiddenSocialActions
```

### 18.2 NpcRelationshipState

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
IsEstranged
LastGiftDay
GiftsGivenThisWeek
DiscoveredLovedGiftTags
DiscoveredHatedGiftTags
SocialMemoryFlags
CompletedSocialQuestIds
ActiveSocialQuestIds
LastTalkDay
LastVisitDay
```

### 18.3 GiftPreferenceProfileSO

Campos conceituais:

```text
LovedGiftTags
LikedGiftTags
NeutralGiftTags
DislikedGiftTags
HatedGiftTags
ForbiddenGiftTags
UniqueLovedItemIds
UniqueHatedItemIds
QualityMultiplierRules
DeityTagRules
SeasonalGiftRules
FestivalGiftRules
```

### 18.4 SpouseHelperProfileSO

Campos conceituais:

```text
NpcId
AllowedHelperActions
BlockedHelperActions
WeeklyHelperFrequency
HelperActionBudget
WeatherRules
SeasonRules
RelationshipMoodRules
RequiredFarmAnchors
DialogueBeforeAction
DialogueAfterAction
```

---

## 19. Save/load conceitual

Social save deve persistir valores simples e IDs estáveis.

Persistir:

```text
NpcId;
RelationshipState;
FriendshipValue;
TrustValue;
AffectionValue;
booleans de dating/committed/married/estranged;
SocialMemoryFlags;
Gift discovery;
quest social state;
last gift/talk/visit day;
spouse state;
farm visit state.
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
4. aplicar relationship state;
5. aplicar SocialMemoryFlags;
6. resolver schedule/scene atual;
7. instanciar NPCs conforme cena;
8. aplicar diálogo/ícones/availability;
9. aplicar spouse/farm visit state apenas se cena permitir.
```

---

## 20. HUD e feedback social

Feedback deve ser claro, mas não excessivamente numérico por padrão.

Possíveis superfícies:

```text
ícone de relação no social log;
texto de reação ao presente;
ícone de diálogo novo;
marcador de quest pessoal;
convite recebido;
calendário de aniversário/evento;
notificação de visita;
feedback de spouse helper;
registro de preferências descobertas.
```

Evitar:

```text
barra gigante sempre visível;
spam de números;
feedback que transforme NPC em planilha;
feedback ambíguo quando presente é ofensivo;
ícone romântico em NPC bloqueado por idade/narrativa.
```

---

## 21. Balance e anti-exploit

Riscos:

```text
farm infinito de presentes;
comprar item barato e converter em relação alta;
presentear item produzido em massa sem limite;
casamento usado como automação superior;
spouse helper superando companions;
romance virando requisito de poder;
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
spouse helper com orçamento limitado;
romance/casamento sem bônus permanente forte;
presentes comprados em loja não devem quebrar progressão;
qualidade não anula preferência negativa;
serviços essenciais não devem ficar indisponíveis sem fallback;
eventos sociais devem respeitar schedule e prioridade.
```

---

## 22. Preparação para specs atuais sem implementação social

Specs executáveis próximas devem considerar este documento apenas como restrição de compatibilidade futura.

### 22.1 Cidade

Ao criar specs atuais de cidade, preparar:

```text
NpcId estável;
residência;
schedule marker;
service marker;
talk anchor;
festival anchor;
farm visit eligibility hook;
romance eligibility vindo do roster;
religion fields;
sem runtime social completo.
```

### 22.2 Fazenda

Ao criar specs atuais de fazenda, preparar:

```text
visitor spawn anchor;
visitor idle anchor;
Fonte de Anya reaction anchor;
farmhouse expansion hook;
mailbox/board hook;
pet/social interaction area;
sem spouse helper runtime.
```

### 22.3 Companions

Ao criar specs atuais de companions, preparar:

```text
separação entre Relationship e CompanionBond;
companion unlock por Trust/Friendship futuro sem romance obrigatório;
active companion independente de spouse;
sem romance runtime.
```

### 22.4 Pets

Ao criar specs atuais de pets, preparar:

```text
pet bond separado;
NPC reaction hooks;
spouse-pet interaction hook futuro;
sem social romance runtime.
```

### 22.5 Save/load

Ao criar specs atuais de save, não implementar social state incompleto.

Se necessário, reservar versão/schema futuro:

```text
SocialSaveData: deferred
```

Não criar campos parciais sem uso.

---

## 23. Roadmap de specs futuras

Specs futuras sugeridas, não atuais:

```text
spec_social_relationship_profile_contract_future.md
spec_social_friendship_trust_runtime_future.md
spec_social_gift_preferences_reactions_future.md
spec_social_dialogue_conditions_memory_future.md
spec_social_personal_quests_future.md
spec_social_romance_route_runtime_future.md
spec_social_marriage_ceremony_state_future.md
spec_social_spouse_helper_farm_runtime_future.md
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

## 24. Decisões fechadas

```text
Relacionamento/romance/casamento é feature futura, não entra na execução atual.
O documento existe para preparar dependências e evitar bloqueios.
Romance é opcional.
Casamento é opcional.
Romance/casamento não são caminho obrigatório de poder.
Amizade deve ter valor mesmo sem romance.
NPCs casados, jovens demais ou bloqueados narrativamente não têm rota romântica.
Spouse helper é futuro, limitado e não substitui companions, pets, skill tree, ferramentas ou execução do jogador.
CompanionBond não é igual a Relationship.
PetBond não é Relationship.
NPC roster vence para elegibilidade individual.
Cidade/fazenda devem reservar hooks e anchors, mas não implementar social runtime agora.
Save/load social deve usar IDs e valores simples, nunca referências Unity.
```

---

## 25. Pendências abertas / validações futuras

```text
Definir valores finais de Friendship/Trust/Affection.
Definir thresholds por estado social.
Definir se haverá aniversários.
Definir se haverá bouquet/item de namoro.
Definir se haverá item/ritual de casamento.
Definir se NPC casado muda para fazenda ou mantém rotina híbrida.
Definir se romance múltiplo existe ou não.
Definir se ciúme existe e com qual severidade.
Definir lista de loved/liked/disliked/hated gifts por NPC.
Definir social questline de cada candidato.
Definir spouse helper por NPC.
Definir reação de cada NPC à Fonte de Anya.
Definir reação de cada NPC às luas Alihana/Senya/Nyx.
Definir como festivais sociais interagem com relação.
Definir se amizade afeta preços apenas via reputação/economia ou por regra social separada.
Atualizar SPEC_SOURCE_MAP.md para incluir este documento como fonte canônica futura quando for seguro preservar o arquivo inteiro.
```
