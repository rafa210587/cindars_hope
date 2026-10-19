# Cindar's Hope — Pets Direction

> **Status:** documento canônico de direção futura do sistema de pets  
> **Local:** `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Função:** registrar direção futura para cachorro, gato, pets futuros, vínculo, alimentação, rotina, área/cama, funções na fazenda, funções na caverna, HUD, save/load, data assets e roadmap de specs futuras.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados, runtime, UI e validações Unity.

---

## Nota de escopo atual

Pets estão **deferidos**.

Este documento existe apenas como referência de direção futura. Pets não entram no escopo atual de implementação, specs, código, UI, save/load ou validação Unity.

Até nova decisão explícita:

```text
não criar specs de pets;
não implementar runtime de pets;
não criar HUD de pets;
não criar data assets de pets;
não integrar pets à fazenda;
não integrar pets à caverna;
não integrar pets a companion jobs;
não tratar pet como requisito de progressão.
```

Quando o sistema de pets voltar ao escopo, ele deve ser reavaliado a partir deste documento, do `SPEC_SOURCE_MAP.md` e do estado real do repo na branch `dev`.

---

## 0. Regra anti-duplicação

Este documento define **pets** como direção futura.

Ele não redefine:

```text
companions;
roster completo de NPCs;
romance/casamento;
IA de inimigos;
combate core;
loot tables finais;
preços finais;
layout final da fazenda;
layout final da caverna;
fórmulas finais de atributos derivados.
```

Fontes canônicas relacionadas:

```text
COMPANIONS_DIRECTION.md
  define companions e estabelece que pet é sistema separado.

FARM_DESIGN_DIRECTION_v1.3.md
  define o papel da fazenda, rotina, área produtiva e vida rural.

FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
  define escala, área, construções, anchors e expansão da fazenda.

COMBAT_CORE_DIRECTION.md
  define Stamina, combate, input, dano, Block, Dash, Dodge e HUD de combate.

ENEMY_BEHAVIORS_DIRECTION.md
  define reação de inimigos, leash, target priority e comportamento modular.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  define active combat budget, TTK, limites de companions/pets e balance da caverna.

LOOT_CRAFTING_ECONOMY_DIRECTION.md
  define itens, loot, storage, crafting, economia e recompensas.

ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
  define BaseValue, preços, estoque, restock, SellPoint, refresh e anti-arbitrage.

PLAYER_CORE_SYSTEMS_DIRECTION.md / PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  definem HP, MP, Stamina, fome, cansaço e remoção de Breath/Fôlego.
```

Regra:

```text
Pet é sistema próprio.
Pet não é companion.
Pet não ocupa o slot de companion ativo.
Pet não substitui companion.
Pet não substitui build do jogador.
Pet não joga pelo jogador.
```

---

# PARTE A — Identidade do sistema

## 1. O que é pet

Pet é um animal/mascote ligado ao jogador por vínculo, rotina e convivência.

No escopo futuro, um pet pode:

```text
morar na fazenda;
seguir o jogador em áreas permitidas;
interagir com cama, tigela, brinquedo e área própria;
gerar alertas leves;
ajudar em pequenas funções rurais;
ajudar em exploração de caverna com limites claros;
dar pistas de tesouro, perigo ou armadilha;
oferecer feedback visual/emocional ao jogador;
ter vínculo, humor, energia e rotina;
ser salvo/carregado como parte do estado do jogador/fazenda.
```

Pet não deve:

```text
ser combat companion completo;
tankar boss;
curar o jogador;
substituir skill tree;
substituir companion;
gerar loot separado por padrão;
abrir caminho de progressão obrigatório;
puxar packs novos;
resolver puzzle/quest sozinho;
ser obrigatório para terminar o jogo;
produzir dinheiro infinito;
ter Breath/Fôlego.
```

## 2. Diferença entre pet e companion

| Conceito | Definição |
|---|---|
| NPC comum | personagem com rotina, diálogo, serviço ou quest |
| Companion | NPC que pode ajudar em jobs, quests, caverna ou combate com vínculo funcional |
| Pet | animal/mascote do jogador; sistema instintivo, afetivo e de suporte leve |
| Farm animal | animal produtivo, como vaca/galinha/futuro, ligado a produtos e manejo |
| Mount | montaria futura, se existir; não é pet base |

Regra futura:

```text
Pet pode coexistir com companion ativo, mas não aumenta o active combat budget livremente.
Se um pet estiver ativo na caverna junto de companion, suas ações devem ser leves, raras e de baixo impacto.
```

## 3. Tom de design

Pets reforçam o lado pastoral/confortável da superfície.

Na caverna, pets reforçam tensão por reação instintiva:

```text
cachorro late;
gato observa o escuro;
pet hesita diante de corrupção;
pet aponta ruído, cheiro, armadilha ou presença.
```

Pets devem criar vínculo e legibilidade, não complexidade tática pesada.

---

# PARTE B — Pets iniciais e futuros

## 4. Pets iniciais futuros

Quando o sistema voltar ao escopo, o baseline sugerido deve suportar:

```text
cachorro;
gato.
```

A escolha inicial futura pode ser:

```text
um pet inicial escolhido pelo jogador;
ou pet desbloqueado cedo por evento simples da fazenda/cidade.
```

Regra:

```text
O jogador não deve precisar escolher cachorro ou gato por poder ótimo.
A escolha deve ser majoritariamente afetiva/estética, com diferenças funcionais leves.
```

## 5. Cachorro

Identidade:

```text
protetor;
alerta;
social;
ligado a som, cheiro e presença física.
```

Funções futuras principais:

```text
alerta de perigo;
latido de interrupção leve;
rastreamento de pequenos achados;
reação a inimigo escondido;
companhia visual na fazenda;
pequena ajuda com animais/pasto no futuro.
```

Na fazenda:

```text
pode circular perto da casa, canteiros, pasto e entrada da caverna;
pode reagir a visitantes;
pode latir quando há item perdido/forage próximo;
pode indicar animal fora da área correta no futuro;
não planta, colhe, minera ou processa sozinho.
```

Na caverna:

```text
pode alertar antes de emboscada;
pode latir para interromper ação simples de inimigo comum;
pode farejar tesouro simples ou sala suspeita;
pode hesitar perto de corrupção forte;
não deve atacar continuamente;
não deve segurar aggro de boss;
não deve revelar todos os segredos do andar.
```

## 6. Gato

Identidade:

```text
silencioso;
observador;
curioso;
ligado a detalhes, passagens, brilho e comportamento estranho.
```

Funções futuras principais:

```text
pista de tesouro escondido;
pista de armadilha;
reação a magia/corrupção leve;
passiva social/afetiva na fazenda;
pequena interrupção por distração, não por força.
```

Na fazenda:

```text
pode circular na casa, celeiro futuro, muros, caixas e jardins;
pode indicar item pequeno perdido;
pode deitar perto da Fonte de Anya e reagir a eventos raros;
pode trazer achados simples em dias específicos;
não deve gerar item valioso de forma recorrente.
```

Na caverna:

```text
pode parar e olhar para parede/sombra suspeita;
pode miar ou arquear o corpo perto de trap;
pode indicar tesouro menor ou passagem visualmente suspeita;
pode distrair inimigo comum por janela curta;
não deve revelar rota ótima;
não deve trivializar traps;
não deve ativar mecanismo sozinho.
```

## 7. Pets futuros possíveis

Pets futuros só devem entrar após cachorro/gato estarem sólidos.

Possíveis famílias futuras:

```text
corvo/corva — sinais, brilho, ruínas, presságios;
furão/doninha — pequenos buracos, achados, objetos perdidos;
coruja — alerta noturno, visão, magia/lua;
lagarto/mini-draco não-combatente — calor, rochas, reação a dragões;
coelho/cabra pequena — vida rural, vínculo, eventos leves de fazenda;
pet mágico ligado à Fonte de Anya — late/endgame, raro e narrativo.
```

Regra:

```text
Pet mágico não deve virar summon de combate permanente.
Pet raro não deve ser power creep direto de cachorro/gato.
```

---

# PARTE C — Vínculo, humor, alimentação e rotina

## 8. Bond / vínculo

Todo pet deve ter vínculo com o jogador quando o sistema futuro existir.

Estados sugeridos:

```text
PetBondLevel
PetMood
PetEnergy
PetTrustFlags
PetKnownPlaces
```

Ações que aumentam vínculo:

```text
alimentar;
fazer carinho;
brincar;
levar para passeio;
voltar vivo da caverna;
dar cama/área adequada;
interagir em eventos específicos;
responder a susto/medo do pet em situações de caverna.
```

Ações que reduzem humor/energia, mas não devem destruir vínculo permanentemente no baseline:

```text
ignorar por muitos dias;
não alimentar;
levar a caverna longa demais;
forçar presença em área perigosa;
ficar exausto/faminto sem cuidar da rotina.
```

Regra:

```text
Pet não deve morrer por falta de alimentação no sistema base.
Falta de cuidado reduz humor, energia, disponibilidade e qualidade das reações.
```

## 9. Alimentação

Pets devem ter alimentação simples, sem virar microgerenciamento pesado.

Fontes possíveis:

```text
PetFood comum;
comida simples permitida;
petiscos;
itens especiais raros de vínculo;
comida favorita por espécie.
```

Regras:

```text
Alimentar recupera humor/energia.
Petisco pode dar bônus temporário leve.
Comida favorita aumenta vínculo um pouco mais.
Comida não deve criar buff forte de combate recorrente.
Não usar itens raros/lore como ração comum.
```

Economia:

```text
PetFood tem BaseValue.
PetFood pode ser vendido em loja geral, tratador/pecuária futura ou crafting simples.
PetFood não deve ser canal de arbitragem.
Petiscos raros devem ter restock limitado ou condição clara.
```

## 10. Rotina

Pets devem ter rotina diária simples:

```text
acordar perto da cama/área;
circular por zona da fazenda;
seguir o jogador se chamado;
descansar quando energia baixa;
voltar para cama/área à noite;
reagir a chuva/estação/eventos futuros.
```

Estados de presença:

```text
AtHome
FollowingPlayer
Resting
Sleeping
ExploringFarm
CaveActive
UnavailableByMood
UnavailableByStory
```

Regra:

```text
Rotina de pet deve ser previsível o suficiente para o jogador encontrar o pet.
Pet não deve sumir aleatoriamente sem feedback.
```

## 11. Cama, tigela e área do pet

A fazenda deve suportar área do pet quando o sistema futuro existir.

Objetos base futuros:

```text
PetBed
PetBowl
PetToy
PetAreaAnchor
```

Regras:

```text
PetBed define ponto de descanso/sono.
PetBowl define ponto de alimentação.
PetToy pode gerar interação diária simples.
PetAreaAnchor ajuda a limitar circulação sem hardcode por posição.
```

Progressão possível:

```text
cama simples;
cama confortável;
área externa cercada/decorativa;
brinquedos;
variações cosméticas.
```

Regra econômica:

```text
Upgrades de pet devem ser conforto/vínculo/rotina, não multiplicadores econômicos fortes.
```

---

# PARTE D — Funções na fazenda

## 12. Funções permitidas no escopo futuro

Pets podem ajudar na fazenda de forma leve quando o sistema for retomado.

Funções possíveis:

```text
alerta de visitante;
indicação de item perdido;
indicação de forage próximo;
reação a evento da Fonte;
reação a animal/pasto futuro;
pequeno achado ocasional;
companhia visual/emocional;
redução leve de stress narrativo por rotina;
entrada em eventos sociais/festivais futuros.
```

Funções proibidas no baseline:

```text
plantar;
colher;
regar;
minerar;
cortar árvores;
processar item;
vender item;
comprar item;
abrir shipping;
coletar produção inteira de animal;
gerar dinheiro recorrente relevante;
substituir companion job.
```

## 13. Achados na fazenda

Pets podem encontrar itens simples quando o sistema existir.

Categorias aceitáveis:

```text
forage comum;
semente comum ocasional;
petisco perdido;
moeda baixa;
item decorativo simples;
pista de evento;
fragmento narrativo raro com gatilho específico.
```

Regras:

```text
Achado de pet deve ter cooldown.
Achado valioso deve exigir condição, evento ou progresso.
Achado não deve bypassar cave/farm/crafting progression.
Achado não deve entrar automaticamente no inventário sem feedback.
```

## 14. Relação com Fonte de Anya e Mana

Pets podem reagir a elementos de lore, mas não explicam tudo.

Reações possíveis:

```text
gato dorme perto da Fonte;
cachorro evita água corrompida;
pet fica inquieto em noite específica;
pet percebe mudança antes de NPCs;
pet reage a Raiz Dormente de Mana sem revelar solução.
```

Regras:

```text
Pet pode sinalizar mistério.
Pet não deve entregar resposta de puzzle/lore.
Pet não deve produzir Água Viva.
Pet não deve cultivar Fruto de Mana.
Pet não deve purificar Pedra Negra sozinho.
```

---

# PARTE E — Funções na caverna

## 15. Entrada na caverna

Baseline futuro:

```text
1 pet ativo opcional pode acompanhar o jogador na caverna, se desbloqueado e com energia/humor suficientes.
```

Regras:

```text
Pet ativo não conta como companion.
Pet ativo ainda respeita active combat budget.
Pet não deve puxar pack novo.
Pet não deve sair do leash do jogador.
Pet deve usar safe spawn/follow position.
Pet deve respeitar boss gates, checkpoints e restrições de run.
```

## 16. Alerta de perigo

Pet pode alertar perigo por feedback simples quando o sistema existir.

Tipos de alerta:

```text
EnemyNearbyAlert;
AmbushHint;
TrapHint;
TreasureHint;
CorruptionReaction;
BossFearReaction;
LowPlayerStateReaction;
```

Regras:

```text
Alerta deve ser probabilístico ou condicionado, não radar perfeito.
Alerta deve ter cooldown.
Alerta deve ser legível por ícone, animação, som ou texto curto.
Alerta não deve revelar posição exata sempre.
```

## 17. Interrupção leve

Pet pode interromper levemente inimigo comum quando o sistema existir.

Exemplos:

```text
cachorro late e cancela windup simples;
gato distrai e atrasa reação curta;
pet faz inimigo comum olhar para outro lado por instante;
pet aplica mini-stagger não-danoso em criatura pequena.
```

Limites:

```text
não funciona em boss por padrão;
não funciona em inimigo imune a distração;
não interrompe cast/ação pesada sem regra específica;
não vira stun recorrente;
não substitui Block/Dodge/Dash do jogador;
usa cooldown e energia.
```

## 18. Tesouros e traps

Pets podem ajudar a perceber tesouros/traps sem invalidar exploração.

TreasureHint:

```text
indica sala suspeita;
indica tile/prop estranho;
indica baú escondido simples;
indica brilho/cheiro próximo.
```

TrapHint:

```text
pet para antes de área perigosa;
pet mia/late;
pet evita pisar;
ícone de cautela aparece.
```

Regras:

```text
Pet não desarma trap.
Pet não abre baú.
Pet não marca caminho ótimo inteiro.
Pet não revela loot table.
Pet não substitui percepção/atenção do jogador.
```

## 19. Limites contra bosses

Contra bosses, pet deve ser suporte emocional/legibilidade, não ferramenta de vitória.

Permitido:

```text
reação de medo;
alerta de fase;
latido/miado cosmético;
indicação leve de área perigosa já telegráfica;
retirada para posição segura;
pequeno bônus passivo de vínculo, se existir e for balanceado.
```

Proibido:

```text
tankar boss;
segurar aggro;
interromper fase principal;
dar dano relevante;
curar jogador;
reviver jogador;
ignorar arena/boss gate;
gerar loot extra;
resolver mecânica central.
```

Regra:

```text
Se o pet estiver presente em boss fight, ele deve ser protegido por comportamento de recuo/safe state, não por tanking.
```

---

# PARTE F — HUD, feedback e legibilidade

## 20. HUD de pet

HUD de pet é futuro e deve ser leve.

Informações possíveis:

```text
ícone do pet ativo;
humor;
energia;
fome/alimentação como estado simples;
alerta contextual;
cooldown de ação especial, se existir;
estado: seguindo, descansando, na fazenda, indisponível.
```

Regra:

```text
HUD de pet não deve competir com HP/MP/Stamina do jogador.
Não criar barra complexa se ícone/estado resolver.
```

## 21. Ícones e feedback

Ícones sugeridos:

```text
PetHappy;
PetHungry;
PetTired;
PetAlert;
PetTreasureHint;
PetTrapHint;
PetFear;
PetResting;
PetFollowing;
```

Feedback visual/sonoro:

```text
balão pequeno;
ícone acima do pet;
latido/miado;
animação de atenção;
pausa e olhar para direção;
volta para o jogador;
recuo em perigo forte.
```

Regra:

```text
Feedback de pet deve ser compreensível sem texto longo.
```

---

# PARTE G — Dados, contratos e save/load futuros

## 22. Data assets esperados

Specs futuras podem derivar os seguintes assets:

```text
PetDataSO
PetBondProfileSO
PetMoodProfileSO
PetFoodDataSO
PetRoutineProfileSO
PetFarmBehaviorProfileSO
PetCaveBehaviorProfileSO
PetAlertProfileSO
PetTreasureHintProfileSO
PetTrapHintProfileSO
PetHUDProfileSO
PetCosmeticProfileSO
PetAreaProfileSO
PetSaveProfileSO
```

Campos conceituais de `PetDataSO`:

```text
PetId
DisplayName
Species
VisualVariantIds
DefaultBondProfileId
DefaultRoutineProfileId
AllowedFoodTags
FavoriteFoodIds
CanEnterCave
CanUseFarmHints
CanUseCaveHints
CanUseLightInterrupt
BaseEnergy
BaseMood
IconId
SpriteSetId
```

## 23. Estado salvo

Save/load de pet é futuro.

Quando existir, deve preservar estado por IDs e valores simples.

Estado esperado:

```text
ownedPetIds;
activePetId;
petNameById;
petVariantById;
bondLevelByPetId;
bondXpByPetId;
moodByPetId;
energyByPetId;
lastFedDayByPetId;
lastInteractionDayByPetId;
lastGiftedItemIdByPetId;
petHomeAnchorIdByPetId;
petBedInstanceIdByPetId;
petRoutineStateByPetId;
petUnlockedFlags;
petCosmeticState;
```

Regras:

```text
Não serializar GameObject.
Não serializar Transform.
Não serializar ScriptableObject direto.
Não serializar referência Unity.
Salvar IDs estáveis e valores simples.
Pet não deve ter Breath/Fôlego no save.
```

## 24. Compatibilidade com economia e inventário

Itens de pet devem seguir economia geral quando entrarem no escopo.

Categorias prováveis:

```text
PetFood;
PetTreat;
PetToy;
PetBed;
PetBowl;
PetCosmetic;
PetQuestItem;
```

Regras:

```text
Item vendável de pet precisa BaseValue.
PetQuestItem deve ter proteção contra venda/descarte acidental.
PetFood pode ser stack.
PetBed/PetBowl/PetToy podem ser ItemInstance se tiverem estado/posição/variante.
Cosmético não deve alterar poder de combate de forma relevante.
```

---

# PARTE H — Balance e restrições

## 25. Princípios de balance

```text
Pet adiciona companhia, leitura e pequenos apoios.
Pet não adiciona segundo personagem jogável.
Pet não aumenta loot por padrão.
Pet não cria economia paralela.
Pet não transforma caverna em modo automático.
Pet não remove risco de trap, emboscada ou boss.
```

## 26. Energia e cooldown

Ações úteis de pet devem consumir energia ou cooldown.

Exemplos:

```text
alerta comum — cooldown curto/médio;
treasure hint — cooldown por andar ou por dia;
trap hint — chance/cooldown por sala/área;
interrupção leve — cooldown alto e limitado a inimigo comum;
achado de fazenda — cooldown diário ou por evento.
```

Regra:

```text
Pet com energia baixa deve reduzir ou suspender ações úteis.
```

## 27. Relação com player stats

Pets não usam a ficha completa do player/companion.

Eles podem ter stats próprios simples:

```text
Energy;
Mood;
Bond;
AlertChance;
HintChance;
InterruptCooldown;
FollowDistance;
CaveStress;
```

Regras:

```text
Pet não tem HP/MP/Stamina completos no baseline.
Pet não tem Breath/Fôlego.
Pet não deve exigir build de atributos do jogador para funcionar.
Bonificações de player podem afetar vínculo/social futuramente, mas não devem ser obrigatórias.
```

---

# PARTE I — Roadmap conceitual futuro

## 28. Roadmap 0 — Reavaliar entrada no escopo

Objetivo futuro:

```text
confirmar quando pets voltam ao roadmap;
confirmar que pet é separado de companion;
confirmar que pet não usa Breath/Fôlego;
confirmar que pets não entram como farm invasion/town hostile/world enemy events;
confirmar que SPEC_SOURCE_MAP aponta para PETS_DIRECTION.md;
definir se cachorro/gato entram no primeiro corte ou se todo o sistema continua deferido.
```

Specs futuras sugeridas, somente após nova aprovação:

```text
spec_pet_data_contract.md
spec_pet_save_load_contract.md
```

## 29. Roadmap 1 — Base doméstica/fazenda futura

Objetivo:

```text
criar posse de pet;
criar cama/tigela/área;
criar rotina simples;
criar alimentar, carinho e seguir/parar;
criar HUD mínimo.
```

Specs futuras sugeridas:

```text
spec_pet_home_area_bed_bowl_runtime.md
spec_pet_bond_mood_energy_runtime.md
spec_pet_follow_home_routine_runtime.md
spec_pet_hud_icons_feedback.md
```

## 30. Roadmap 2 — Funções leves de fazenda futuras

Objetivo:

```text
alerta de visitante;
achados simples;
reação à Fonte;
forage hint leve;
integração com rotina da fazenda.
```

Specs futuras sugeridas:

```text
spec_pet_farm_alerts_and_foraging_hints.md
spec_pet_farm_daily_find_cooldowns.md
spec_pet_anya_fountain_reactions.md
```

## 31. Roadmap 3 — Caverna e exploração futuras

Objetivo:

```text
pet ativo opcional na caverna;
follow/leash/safe spawn;
alerta de perigo;
treasure hint;
trap hint;
interrupção leve.
```

Specs futuras sugeridas:

```text
spec_pet_cave_follow_leash_safe_spawn.md
spec_pet_cave_alerts_treasure_trap_hints.md
spec_pet_light_interrupt_runtime.md
spec_pet_cave_stress_energy_limits.md
```

## 32. Roadmap 4 — Integrações sociais/econômicas futuras

Objetivo:

```text
pet em festivais;
pet em diálogos de NPC;
loja de pet food/toys/cosmetics;
reputação social leve;
interações com spouse/companions.
```

Specs futuras sugeridas:

```text
spec_pet_shop_food_toys_cosmetics.md
spec_pet_city_festival_reactions.md
spec_pet_companion_spouse_interactions.md
```

## 33. Roadmap 5 — Conteúdo futuro e pets especiais

Objetivo:

```text
novas espécies;
pets raros;
pets ligados a lore;
eventos de Fonte/Anya/Mana;
cosméticos avançados.
```

Specs futuras sugeridas:

```text
spec_pet_future_species_unlocks.md
spec_pet_lore_reactive_special_pet_future.md
spec_pet_cosmetic_variants_runtime.md
```

## 34. Fora de roadmap atual

Não implementar agora:

```text
qualquer sistema de pet;
pet combat build;
pet skill tree grande;
pet equipment completo;
pet permadeath;
pet breeding;
pet marketplace;
pet como mount;
pet como summon mágico permanente;
farm invasion pet defense;
town hostile event pet behavior;
world enemy event pet behavior;
multiplayer pet sync.
```

---

# PARTE J — Regras finais

## 35. Regras canônicas de pets

```text
Pet é sistema separado de companion.
Pet não entra no escopo atual.
Pet não conta como companion ativo.
Pet pode coexistir com companion no futuro, respeitando active combat budget.
Pet ajuda por alerta, pista, vínculo e interrupção leve apenas quando o sistema for retomado.
Pet não joga pelo jogador.
Pet não cura, tanka ou causa dano relevante.
Pet não gera loot extra por padrão.
Pet não puxa packs novos.
Pet não resolve boss.
Pet não tem Breath/Fôlego.
Pet não substitui farm jobs de companion.
Cachorro e gato são baseline futuro sugerido.
Pets futuros devem ser expansão, não power creep obrigatório.
```

## 36. Resultado esperado

Depois deste documento, specs futuras de pets devem conseguir nascer sem depender de conversa solta.

Elas devem apontar para:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

E devem respeitar:

```text
pets como sistema futuro/deferido;
pets como suporte leve;
separação clara de companions;
economia sem abuso;
caverna sem trivialização;
save/load por IDs;
HUD simples;
validação Unity futura por fatias pequenas.
```
