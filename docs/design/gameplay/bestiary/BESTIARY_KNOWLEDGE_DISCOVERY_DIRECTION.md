# Cindar's Hope — Bestiary & Knowledge Discovery Direction

> **Status:** direction canônico de bestiário, descoberta de conhecimento, identificação de inimigos, vulnerabilidades conhecidas e HUD futura de conhecimento  
> **Local:** `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`  
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> **Função:** definir como o jogador descobre, registra e usa conhecimento sobre inimigos, famílias, comportamentos, habitats, drops, vulnerabilidades, resistências, imunidades, janelas comportamentais e lore, sem revelar informações não descobertas.  
> **Não é spec implementável.** Specs futuras devem quebrar esta direção em assets, save/load, UI e runtime.

---

## 0. Decisão de escopo

O sistema de bestiário e descoberta de conhecimento **não entra na primeira entrega executável atual**.

Escopo atual deste documento:

```text
definir direção canônica;
definir estados de conhecimento;
definir como conhecimento é descoberto;
definir o que a UI futura deve mostrar;
definir como tooltips/equipment/spells dependem de conhecimento descoberto;
definir contratos conceituais de save/load;
definir anti-regressões para evitar spoilers.
```

Fora da primeira entrega executável:

```text
Bestiary UI completa;
Bestiary HUD permanente;
Knowledge Log navegável;
EnemyBestiaryEntrySO implementado;
EnemyKnowledgeState runtime completo;
UI de pesquisa/laboratório;
NPC research services;
cards completos por criatura;
tracking visual avançado de fraquezas;
achievements de documentação;
coleções/livros/enciclopédia completos.
```

Permitido preparar agora, se specs futuras precisarem:

```text
IDs estáveis;
tags de conhecimento;
campos vazios em save/load;
hooks de evento;
contratos conceituais;
UI placeholders de debug;
sem HUD final de bestiário.
```

Regra:

```text
A primeira entrega pode coletar dados ou preparar hooks, mas não deve prometer uma tela de bestiário completa ao jogador.
```

---

## 1. Problema que este sistema resolve

A caverna terá muitos inimigos, famílias, packs, bosses, vulnerabilidades, drops, janelas comportamentais e interações com armas, magia, pets e companions.

Se tudo for revelado de início:

```text
vulnerabilidades viram spoiler;
loot raro fica previsível demais;
preparação perde valor;
NPCs especialistas perdem função;
experiência de descoberta fica rasa;
UI de equipamento revela conhecimento que o jogador ainda não conquistou;
bosses e main quest perdem mistério.
```

Se nada for registrado:

```text
jogador esquece descobertas;
preparação fica frustrante;
farming fica opaco;
fraquezas parecem arbitrárias;
magia/equipamento não comunicam valor;
quests de pesquisa ficam frágeis.
```

Solução:

```text
O jogo deve ter um sistema de Knowledge Discovery.
O bestiário é a apresentação futura desse conhecimento.
Tooltips, equipment UI, spell UI e quest UI só podem mostrar conhecimento que o jogador descobriu ou recebeu legitimamente.
```

---

## 2. Regra anti-duplicação

Este documento **não é** a fonte de stats reais de inimigos.

Fontes canônicas:

```text
CAVE_MONSTER_ROSTER_DIRECTION.md
  inimigos concretos, stats, drops, packs, bosses e scaling da caverna.

ENEMY_BEHAVIORS_DIRECTION.md
  taxonomia geral de Move, Behavior, Trait, EnemyAction, EnemyBrain e módulos de IA.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades, janelas, TTK, active combat budget e balance de caverna.

EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
  contrato de matching entre tags de equipamento/magia/ataque e vulnerabilidades/resistências declaradas no inimigo.

UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
  fluxos de telas, drawers, tooltips, Bestiary/Knowledge UI hook e apresentação de conhecimento conhecido.
```

Este documento define:

```text
como o conhecimento é revelado;
quais estados de conhecimento existem;
como evitar spoiler;
quando uma vulnerabilidade pode aparecer em UI;
quando um drop pode aparecer em UI;
como conhecimento deve ser salvo;
como NPCs, pets, companions, livros e quests podem desbloquear conhecimento;
o que uma futura HUD/bestiary UI precisa conter.
```

Regra:

```text
Bestiary não cria vulnerabilidades.
Bestiary revela vulnerabilidades já existentes quando o jogador as descobre.
```

---

## 3. Filosofia do sistema

O bestiário deve ser um sistema de aprendizado, não apenas uma lista de monstros.

Princípios:

```text
Ver uma criatura revela pouco.
Lutar contra uma criatura revela mais.
Derrotar uma criatura revela mais, mas não tudo.
Usar a arma/magia certa pode revelar fraqueza.
Usar a arma/magia errada pode revelar resistência.
Coletar drop revela drop.
Ler livros e ouvir NPCs pode revelar conhecimento sem combate.
Companions e pets podem dar hints, mas não resolver tudo.
Bosses e main quest têm spoiler control mais forte.
```

O jogador deve sentir:

```text
Eu aprendi algo.
Eu posso preparar melhor.
Meu equipamento/magia faz mais sentido agora.
Meu conhecimento melhora minha chance na caverna.
```

O sistema não deve exigir grind excessivo.

---

## 4. Unidade de conhecimento

Conhecimento deve ser registrado por `EnemyKnowledgeKey`.

Chaves possíveis:

```text
EnemyId
EnemyFamilyId
EnemyVariantId
BossId
BiomeId
FactionId
DropId
AttackId
BehaviorId
VulnerabilityTag
ResistanceTag
ImmunityTag
MaterialInteractionTag
StatusInteractionTag
LoreEntryId
QuestKnowledgeId
```

Granularidade recomendada:

```text
EnemyId:
  conhecimento sobre criatura específica.

EnemyFamilyId:
  conhecimento compartilhado por família.

BossId:
  conhecimento próprio de boss, com spoiler control.

VariantId:
  conhecimento de variante especial, corrompida, elite ou lunar.

DropId:
  drop descoberto pela coleta ou informação.

VulnerabilityTag:
  fraqueza descoberta por teste, NPC, livro ou quest.
```

Regra:

```text
Conhecimento de família pode ajudar em várias criaturas.
Conhecimento de variante pode sobrescrever conhecimento de família.
Conhecimento de boss nunca deve ser totalmente inferido só pela família.
```

---

## 5. Estados principais de conhecimento

### 5.1 EnemyKnowledgeState

Estados progressivos:

```text
Unknown
Seen
ScannedFuture
Fought
Defeated
RepeatedDefeated
Studied
FullyDocumented
```

Significado:

```text
Unknown:
  jogador não viu ou não recebeu informação.

Seen:
  criatura foi vista, detectada, alvejada ou registrada visualmente.

ScannedFuture:
  estado futuro para magia, item, companion ou pesquisa que identifica sem combate.

Fought:
  jogador recebeu ataque, causou dano ou entrou em combate real.

Defeated:
  jogador derrotou ao menos uma instância.

RepeatedDefeated:
  jogador derrotou quantidade suficiente para reforçar conhecimento.

Studied:
  jogador obteve conhecimento por livro, NPC, laboratório futuro, quest ou pesquisa.

FullyDocumented:
  jogador descobriu identidade, habitat, comportamento, drops principais, vulnerabilidades relevantes e notas de lore.
```

### 5.2 KnowledgeConfidence

Nem todo conhecimento precisa ser 100% no primeiro contato.

Estados:

```text
Rumor
Partial
Confirmed
Mastered
```

Uso:

```text
Rumor:
  informação recebida por NPC, livro incompleto ou sinal ambiental.

Partial:
  observação inicial, sem confirmação total.

Confirmed:
  testado em combate, drop coletado ou quest validada.

Mastered:
  conhecimento consolidado por repetição, estudo ou boss/quest finalizada.
```

Regra:

```text
UI pode indicar diferença entre rumor e conhecimento confirmado.
Mas a primeira entrega não precisa implementar visual completo disso.
```

---

## 6. Tipos de informação revelável

Cada criatura/família pode ter informações reveláveis separadas.

Categorias:

```text
Identity
Family
Habitat
DepthRange
Biome
Faction
BehaviorSummary
MovePattern
AttackList
StatusApplied
DropsCommon
DropsRare
DropsBossFirstTime
DropsRepeat
ElementVulnerability
StatusVulnerability
AttackTypeVulnerability
WeaponVulnerability
MaterialVulnerability
BehavioralVulnerabilityWindow
ResistanceTags
ImmunityTags
PackBehavior
LoreNote
QuestNote
```

Regra:

```text
Cada categoria pode ter sua própria condição de descoberta.
Derrotar uma criatura não precisa revelar todas as categorias.
```

Exemplo:

```text
Ver criatura:
  revela silhouette, apelido temporário, habitat atual.

Lutar:
  revela ataque observado e comportamento básico.

Derrotar:
  revela nome comum e drop coletado.

Usar fogo e causar dano aumentado:
  revela Fire vulnerability.

Usar gelo e causar dano reduzido:
  revela Ice resistance.

Coletar item raro:
  revela rare drop.

Ler diário nymiriano:
  revela lore note de variante corrompida.
```

---

## 7. Fontes de descoberta

Conhecimento pode ser descoberto por várias fontes.

### 7.1 Observação direta

Eventos:

```text
EnemySeen
EnemyTargeted
EnemyDamagedPlayer
EnemyUsedAction
EnemyEnteredPackBehavior
EnemyUsedStatus
EnemyFled
EnemyGuardedResource
EnemyTriggeredTrap
```

Pode revelar:

```text
Seen;
apelido temporário;
comportamento básico;
ataque observado;
status aplicado;
habitat atual;
pack role parcial.
```

### 7.2 Combate

Eventos:

```text
PlayerHitEnemy
PlayerCritEnemy
PlayerBlockedEnemy
PlayerDodgedEnemyAction
PlayerTriggeredCriticalWindow
EnemyStaggered
EnemyDefeated
BossPhaseReached
```

Pode revelar:

```text
Fought;
BehavioralVulnerabilityWindow;
AttackType effectiveness;
resistência física;
janela crítica;
ataque perigoso;
phase behavior.
```

### 7.3 Teste de dano/tag

Eventos:

```text
DamageTypeEffective
DamageTypeResisted
StatusAppliedSuccessfully
StatusResisted
MaterialEffective
MaterialResisted
WeaponTypeEffective
WeaponTypeResisted
ImmunityTriggered
```

Pode revelar:

```text
ElementVulnerability;
StatusVulnerability;
AttackTypeVulnerability;
WeaponVulnerability;
MaterialVulnerability;
ResistanceTags;
ImmunityTags.
```

Regra:

```text
A UI só deve mostrar uma vulnerabilidade depois que o evento de descoberta ocorrer ou conhecimento for recebido por fonte legítima.
```

### 7.4 Drops e loot

Eventos:

```text
EnemyDropCollected
BossFirstTimeRewardCollected
RareDropCollected
MaterialHarvested
CorpseResourceCollectedFuture
```

Pode revelar:

```text
DropsCommon;
DropsRare;
DropsBossFirstTime;
DropsRepeat;
material source;
crafting relevance.
```

Regra:

```text
Drop não coletado não deve aparecer como nome completo no bestiário, salvo se NPC/livro/quest revelar.
```

### 7.5 NPCs

NPCs podem revelar conhecimento por:

```text
diálogo;
serviço;
quest;
rumor;
aula;
compra de guia;
recompensa de relacionamento futuro;
evento de cidade;
loja especializada;
```

NPCs possíveis:

```text
ferreiro:
  materiais, armor, fraquezas físicas e reparo.

mago/estudioso:
  magia, elementos, Arcane, Senya.

caçador/minerador:
  habitat, drops, comportamento físico.

Padre Corvus:
  mortos-vivos, corrupção, Anya/Kanthor, proteção.

Yael:
  Nyx, segredos, sombra, criaturas noturnas, diferença entre Nyx e culto.

Sethra:
  conhecimento perigoso, rumores de Pedra Negra, informações ambíguas ou manipuladas.

Vaelrion:
  Elyndor, Bromécia, constructs, Arco da Memória, mas com arrogância e viés.

Companion especialista futuro:
  hints por presença, área ou vínculo.
```

Regra:

```text
NPC pode revelar conhecimento parcial, rumor ou confirmado dependendo da fonte.
NPC não deve revelar automaticamente todo o bestiário.
```

### 7.6 Livros, documentos e ruínas

Fontes:

```text
livros da cidade;
diários de Cindar;
inscrições nymirianas;
ruínas de Elyndor;
registros bromecianos;
mapas incompletos;
anotações de Vaelrion;
notas de caçadores;
cartazes de encomenda;
```

Pode revelar:

```text
lore note;
habitat;
família;
faction;
vulnerabilidade parcial;
boss hint;
quest knowledge;
```

Regra:

```text
Documento antigo pode estar incompleto, enviesado ou errado parcialmente, mas não deve frustrar o jogador com mentira sistêmica sem pista.
```

### 7.7 Pets e companions

Pets podem revelar hints leves:

```text
gato reage a Nyx;
cachorro alerta armadilha/criatura próxima;
pet sinaliza cheiro de drop raro;
pet indica perigo, não identifica stat.
```

Companions podem revelar:

```text
comentário de família de inimigo;
alerta de ataque;
hint de fraqueza se especialista;
memória de luta anterior;
habitat provável;
```

Regra:

```text
Pet/companion ajudam a descobrir, mas não jogam pelo jogador e não completam bestiário sozinhos.
```

---

## 8. Regras de revelação por categoria

### 8.1 Identidade

```text
Unknown:
  ??? ou silhouette.

Seen:
  apelido temporário ou nome visual simples.

Defeated/Studied:
  nome comum.

FullyDocumented:
  nome completo, variante e família.
```

### 8.2 Habitat e profundidade

```text
Seen em uma faixa:
  registra local observado.

RepeatedDefeated em faixa:
  registra faixa provável.

Studied:
  pode revelar habitat adicional.
```

### 8.3 Ataques

```text
EnemyUsedAction:
  registra ataque observado.

PlayerBlocked/Dodged/Hit:
  registra leitura de perigo.

Repeated exposure:
  melhora descrição.
```

### 8.4 Vulnerabilidades

```text
Dano aumentado confirmado:
  revela vulnerabilidade correspondente.

Status aplicado com sucesso em inimigo resistente a outros status:
  pode revelar vulnerabilidade de status.

Janela crítica explorada:
  revela BehavioralVulnerabilityWindow.

NPC/livro:
  revela rumor ou partial até ser confirmado.
```

### 8.5 Resistências e imunidades

```text
Dano reduzido observado:
  revela resistência parcial.

ImmunityTriggered:
  revela imunidade específica.

Repeated failed attempts:
  confirma resistência/imunidade.
```

### 8.6 Drops

```text
Coletar drop comum:
  revela drop comum.

Coletar drop raro:
  revela drop raro.

NPC/livro/quest:
  pode revelar possibilidade de drop como rumor.

Nunca coletado e nunca informado:
  não mostrar nome completo.
```

---

## 9. Integração com Equipment UI

Equipment UI pode mostrar interações com inimigos apenas quando conhecidas.

Exemplo de item antes de conhecimento:

```text
Material: Silver
Known enemy interactions:
  Unknown
```

Depois de descoberta:

```text
Material: Silver
Known enemy interactions:
  Eficaz contra mortos-vivos menores.
  Pouco eficaz contra constructos bromecianos, se descoberto.
```

Regras:

```text
Equipment UI não revela vulnerabilidade não descoberta.
Weapon detail drawer só mostra known enemy interactions.
Comparison drawer pode indicar vantagem conhecida, não vantagem secreta.
Repair/Upgrade UI não deve revelar inimigo futuro.
MaterialVulnerability real continua vindo de enemy data/vulnerability docs.
```

---

## 10. Integração com Spell/Magic UI

Spell UI pode mostrar efetividade conhecida.

Exemplo antes de descoberta:

```text
Fireburst
Known effectiveness:
  Unknown
```

Depois de combate/estudo:

```text
Fireburst
Known effectiveness:
  Forte contra fungos úmidos.
  Fraco contra criaturas de cinza, se descoberto.
```

Regras:

```text
Spell UI não revela resistência/imunidade não descoberta.
LearnableScroll não deve listar todos os inimigos afetados por spoiler.
CastScroll pode revelar após uso.
Wand/Focus pode indicar interação conhecida, não futura.
```

---

## 11. Integração com Loot/Crafting/Economy

Bestiário pode ajudar a entender origem de materiais.

Regras:

```text
Drop coletado pode aparecer como fonte conhecida de item.
Item de crafting pode listar fonte conhecida, se descoberta.
Drop raro não descoberto aparece como ??? ou não aparece.
Preço/economia não deve depender de revelar drop secreto.
Encomenda pode revelar que certo monstro carrega material específico, se a quest autorar isso.
```

Exemplo:

```text
Material: Casca Quitinosa
Fonte conhecida:
  Besouro Ferrugem — confirmado.
  ??? — rumor de ferreiro.
```

---

## 12. Integração com Quests

Quests podem usar conhecimento como objetivo ou recompensa.

Tipos de quest:

```text
identificar criatura;
coletar amostra;
descobrir fraqueza;
confirmar rumor;
documentar comportamento;
caçar variante;
mapear habitat;
pesquisar Pedra Negra;
entender criatura corrompida;
derrotar boss com pista prévia;
```

Quest rewards possíveis:

```text
knowledge unlock;
bestiary entry;
vulnerability hint;
drop source hint;
boss warning;
recipe unlock;
equipment recommendation;
spell recommendation;
```

Regras:

```text
Quest não deve exigir descoberta opaca sem pista.
Quest pode aceitar múltiplas formas de descoberta.
Quest de fraqueza deve reconhecer quando o jogador já descobriu a fraqueza antes.
Main quest deve usar spoiler control para Arquivista do Silêncio, Pedra Negra e nível 101.
```

---

## 13. Integração com Main Quest

Main quest tem entidades sensíveis:

```text
Pedra Negra;
Água Viva corrompida;
constructs bromecianos;
criaturas de memória;
cultistas de Nyx;
variantes corrompidas;
Arquivista do Silêncio;
nível 100/101.
```

Regras:

```text
Bestiário não revela Arquivista do Silêncio antes da main quest.
Bestiário não revela natureza completa da Pedra Negra cedo.
Bestiário pode registrar sintomas: memória drenada, Água Viva turva, comportamento estranho.
Bestiário pode usar entries parciais para criaturas corrompidas.
Bosses de main quest podem ter páginas incompletas até serem enfrentados ou estudados.
```

Exemplo de spoiler control:

```text
Antes da revelação:
  Entidade Arquivada — ???

Após confronto parcial:
  Arquivista do Silêncio — comportamento observado.

Após derrota/final:
  Arquivista do Silêncio — entry completa conforme final escolhido.
```

---

## 14. Integração com clima, lua e calendário

Conhecimento pode depender de contexto temporal.

Exemplos:

```text
Nyx:
  revela comportamento de criaturas noturnas, Pedra Negra e sombra.

Alihana:
  revela memórias, inscrições, criaturas ligadas a água/memória.

Senya:
  revela mutações, magia instável, variantes caóticas.

Fog:
  revela criatura rara ou rastro.

Storm:
  altera spawn/loot e pode revelar comportamento de caverna.

Winter:
  altera habitat e comportamento de criaturas frias.
```

Regra:

```text
Bestiário pode registrar que algo foi observado sob determinada lua/clima, mas não deve revelar todas as condições futuras imediatamente.
```

---

## 15. HUD e UI futura do bestiário

## 15.1 Decisão de escopo

A HUD e UI final de bestiário **não entram na primeira entrega executável atual**.

Este documento define o que elas devem conter futuramente.

Não implementar agora:

```text
Bestiary HUD permanente;
Bestiary Menu completo;
Knowledge Log navegável;
cards visuais completos por monstro;
filtros avançados;
search textual;
tracking automático em HUD;
scan mode;
research UI;
collection progress UI.
```

Pode preparar agora:

```text
ícones futuros;
event hooks;
IDs;
knowledge state placeholder;
debug readout;
save/load placeholder;
strings futuras sem expor ao jogador.
```

## 15.2 Bestiary Menu futuro

Tela futura deve conter:

```text
lista de criaturas conhecidas;
filtros por família;
filtros por bioma/faixa de caverna;
filtros por status de descoberta;
search futura;
entry detail drawer;
abertura por categoria;
notas de lore;
drops conhecidos;
vulnerabilidades conhecidas;
resistências conhecidas;
comportamentos observados;
origem do conhecimento;
confiança da informação;
```

Categorias de filtro:

```text
All
Seen
Defeated
FullyDocumented
By Family
By Cave Depth
By Biome
By Faction
By Drops
By Weakness Known
Bosses
Corrupted
Main Quest
```

## 15.3 Entry detail futura

Cada entry deve poder mostrar:

```text
nome conhecido;
nome oculto ou ??? se não descoberto;
retrato/silhouette;
família;
variante;
habitat;
faixa de profundidade observada;
comportamento resumido;
ataques observados;
status aplicados;
drops descobertos;
raridade conhecida;
vulnerabilidades descobertas;
resistências descobertas;
imunidades descobertas;
janelas comportamentais descobertas;
notas de NPC;
notas de lore;
quest links;
última área observada;
quantidade derrotada;
```

## 15.4 Combat HUD futuro

HUD de combate pode mostrar conhecimento conhecido, mas de forma mínima.

Possíveis elementos futuros:

```text
ícone pequeno de enemy identified;
indicador de weakness known;
indicador de resistance triggered;
notificação curta: Fraqueza descoberta;
notificação curta: Resistência descoberta;
notificação curta: Novo drop registrado;
notificação curta: Bestiário atualizado;
```

Não implementar na primeira entrega.

Quando implementar, regras:

```text
HUD não deve virar planilha durante combate.
Não mostrar lista completa de fraquezas em cima do inimigo.
Não revelar informação desconhecida.
Feedback de descoberta deve ser curto e não bloquear input.
Boss HUD deve respeitar spoiler control.
```

## 15.5 Equipment/Spell tooltip integration

Tooltips futuros podem mostrar:

```text
Eficaz contra: X, Y — apenas conhecidos.
Resistido por: X — apenas conhecido.
Interação desconhecida — se nada foi descoberto.
```

Não mostrar:

```text
todas as famílias futuras;
fraquezas não descobertas;
lista completa de boss;
spoiler de monstro ainda não visto;
```

## 15.6 Journal/Knowledge notification

Notificações futuras:

```text
Nova criatura observada.
Comportamento registrado.
Fraqueza descoberta.
Resistência descoberta.
Drop registrado.
Entrada de bestiário atualizada.
Informação confirmada.
Rumor adicionado.
```

Todas devem seguir `UI_UX_FULL_GAMEPLAY_DIRECTION.md` e não bloquear gameplay.

---

## 16. Data assets futuros

Possíveis assets:

```text
EnemyBestiaryEntrySO
EnemyKnowledgeRuleSO
KnowledgeUnlockRuleSO
KnowledgeSourceSO
BestiaryCategorySO
BestiaryLoreNoteSO
BestiaryDropRevealRuleSO
BestiaryVulnerabilityRevealRuleSO
BestiaryUIProfileSO
EnemyObservationEventSO
ResearchServiceProfileSO
```

### 16.1 EnemyBestiaryEntrySO

Campos conceituais:

```text
EnemyId
DisplayNameKnown
DisplayNameUnknown
FamilyId
VariantId
BiomeTags
DepthRangeHint
FactionTags
PortraitId
SilhouetteId
LoreNoteIds
BehaviorSummaryIds
AttackRevealRules
DropRevealRules
VulnerabilityRevealRules
ResistanceRevealRules
ImmunityRevealRules
QuestKnowledgeLinks
SpoilerTier
```

### 16.2 EnemyKnowledgeState save

Campos conceituais:

```text
EnemyId
State
Confidence
SeenCount
FoughtCount
DefeatedCount
LastSeenDay
LastSeenSeason
LastSeenDepth
KnownAttackIds
KnownDropIds
KnownVulnerabilityTags
KnownResistanceTags
KnownImmunityTags
KnownBehaviorWindowIds
KnownLoreNoteIds
KnowledgeSourceIds
IsFullyDocumented
```

### 16.3 KnowledgeUnlockRuleSO

Campos conceituais:

```text
RuleId
TargetKnowledgeKey
UnlockConditionType
RequiredEnemyId
RequiredFamilyId
RequiredEventType
RequiredCount
RequiredDamageTag
RequiredItemId
RequiredQuestState
RequiredNpcId
RequiredBookId
ConfidenceGranted
SpoilerTierRequired
```

### 16.4 KnowledgeSourceSO

Campos conceituais:

```text
SourceId
SourceType
NpcId
BookId
QuestId
ItemId
CompanionId
PetId
CaveDepthRange
LunarEvent
Weather
GrantedKnowledgeKeys
Confidence
```

---

## 17. Runtime events futuros

Eventos possíveis:

```text
EnemySeenEvent
EnemyFoughtEvent
EnemyDefeatedEvent
EnemyActionObservedEvent
EnemyStatusObservedEvent
DamageEffectivenessObservedEvent
MaterialEffectivenessObservedEvent
StatusEffectivenessObservedEvent
ImmunityObservedEvent
EnemyDropCollectedEvent
BossPhaseObservedEvent
KnowledgeUnlockedEvent
KnowledgeConfirmedEvent
BestiaryEntryUpdatedEvent
NpcKnowledgeGrantedEvent
BookKnowledgeReadEvent
QuestKnowledgeRewardedEvent
CompanionHintKnowledgeEvent
PetHintKnowledgeEvent
```

Regra:

```text
Eventos de bestiário devem ser consumidores de eventos de combate/loot/quest, não a fonte primária do combate.
```

---

## 18. Save/load

Persistir:

```text
EnemyKnowledgeStates
KnownEnemyIds
KnownFamilyIds
KnownDropIds
KnownVulnerabilityTagsByEnemy
KnownResistanceTagsByEnemy
KnownImmunityTagsByEnemy
KnownBehaviorWindowsByEnemy
KnownLoreNoteIds
KnowledgeRumors
KnowledgeConfirmedFlags
BestiaryVersion
```

Não persistir como fonte primária:

```text
texto renderizado;
UI scroll position salvo como regra canônica;
tooltip final;
resultado de dano momentâneo;
lista derivada de filtros;
estado visual de notificação;
```

Load deve:

```text
restaurar conhecimento descoberto;
recalcular entradas visíveis;
recalcular tooltips conhecidos;
respeitar spoiler control;
não revelar conhecimento novo por erro de migração.
```

---

## 19. Progressão e balance de descoberta

Princípios:

```text
Não exigir matar 50 vezes para descobrir informação básica.
Não revelar tudo ao derrotar uma vez.
Recompensar uso inteligente de armas, magia e observação.
Dar caminhos alternativos para jogadores menos combatentes: NPCs, livros, quests, companions.
Bosses podem exigir observação por fases.
Inimigos comuns devem ser documentáveis sem grind excessivo.
```

Progressão sugerida:

```text
1 contato visual:
  Seen.

1 combate real:
  Fought + ataques observados.

1 derrota:
  Defeated + nome comum + drops coletados.

3 derrotas ou estudo:
  comportamento mais claro.

uso de tag efetiva/resistida:
  vulnerabilidade/resistência específica.

quest/livro/NPC:
  rumor/partial/confirmed conforme fonte.

requisitos múltiplos:
  FullyDocumented.
```

Valores finais devem ser definidos em spec.

---

## 20. Spoiler tiers

Cada conhecimento pode ter `SpoilerTier`.

Tiers:

```text
Tier 0 — Safe:
  inimigos comuns, drops comuns, habitat básico.

Tier 1 — Cave Progression:
  inimigos de faixas futuras, elites, drops raros.

Tier 2 — Faction/Lore:
  facções, cultistas, Pedra Negra, Elyndor/Bromécia.

Tier 3 — Main Quest:
  fragmentos de Anya, Fonte, Cindar, Arco da Memória.

Tier 4 — Endgame:
  nível 100/101, Arquivista do Silêncio, finais.
```

Regra:

```text
UI não exibe conteúdo acima do SpoilerTier permitido pelo estado de quest/progresso.
```

---

## 21. Relação com primeira entrega

Na primeira entrega executável atual, este sistema deve ficar como direction/backlog.

Não fazer agora:

```text
Bestiary Menu completo;
Bestiary HUD;
scanning;
pesquisa;
NPC services de conhecimento;
companion/pet knowledge hints complexos;
progressão FullyDocumented;
UI completa de filtros;
integração completa com equipment/spell tooltips.
```

Pode fazer agora, se uma spec futura precisar e for barato:

```text
criar IDs estáveis de enemy/family;
garantir que EnemyDataSO futuro tenha BestiaryEntryId;
garantir que drops tenham IDs;
garantir que vulnerabilities usem tags estáveis;
registrar eventos básicos em debug;
não expor ao jogador ainda.
```

Regra final:

```text
Preparar contrato é permitido.
Entregar sistema de bestiário ao jogador fica fora da primeira entrega.
```

---

## 22. Specs futuras recomendadas

```text
spec_bestiary_entry_data_contract_future.md
spec_enemy_knowledge_state_save_load_future.md
spec_bestiary_discovery_events_runtime_future.md
spec_bestiary_ui_menu_future.md
spec_bestiary_hud_notifications_future.md
spec_equipment_tooltip_known_interactions_future.md
spec_spell_tooltip_known_effectiveness_future.md
spec_bestiary_npc_books_quest_unlocks_future.md
spec_bestiary_boss_spoiler_control_future.md
spec_bestiary_pet_companion_hints_future.md
spec_bestiary_research_service_future.md
```

---

## 23. Regras anti-regressão

```text
Bestiary não entra na primeira entrega executável atual.
Bestiary é sistema de descoberta, não lista completa revelada de início.
Bestiary não cria vulnerabilidade; apenas revela conhecimento sobre vulnerabilidades já autoradas.
Equipment UI não revela vulnerabilidade desconhecida.
Spell UI não revela resistência/imunidade desconhecida.
Drop raro não descoberto não aparece com nome completo.
Derrotar uma criatura uma vez não precisa revelar tudo.
Ver uma criatura não revela fraquezas automaticamente.
NPC/livro/quest pode revelar rumor, partial ou confirmed knowledge.
Pet/companion pode dar hint, mas não completa bestiário sozinho.
Bosses e main quest têm spoiler control.
Arquivista do Silêncio não aparece completo antes da revelação apropriada.
Pedra Negra não revela natureza completa cedo.
Conhecimento descoberto deve ser persistido em save/load.
HUD de bestiário futura não deve virar planilha em combate.
Bestiary UI futura deve mostrar apenas conhecimento conhecido ou autorizado pelo SpoilerTier.
```

---

## 24. Decisões fechadas

```text
O bestiário será tratado como Knowledge Discovery.
A primeira entrega atual não implementa Bestiary UI/HUD completa.
O sistema futuro terá estados Unknown, Seen, Fought, Defeated, RepeatedDefeated, Studied e FullyDocumented.
Conhecimento pode ter confiança Rumor, Partial, Confirmed ou Mastered.
Vulnerabilidades, resistências e drops só aparecem na UI se descobertos ou concedidos por fonte legítima.
Equipment/Spell tooltips dependem do conhecimento descoberto.
Bestiary terá spoiler tiers.
Pets e companions podem contribuir com hints futuros.
NPCs, livros, ruínas e quests podem desbloquear conhecimento.
Save/load deve persistir conhecimento por enemy/family/drop/vulnerability/lore note.
```

---

## 25. Pendências abertas

```text
Definir layout visual do Bestiary Menu futuro.
Definir se bestiário fica dentro de Journal, menu separado ou biblioteca da cidade.
Definir quantidade de derrotas para RepeatedDefeated por família.
Definir quais NPCs ensinam quais famílias de conhecimento.
Definir se haverá scan spell/item.
Definir se pets/companions desbloqueiam conhecimento automaticamente ou apenas hints.
Definir se FullyDocumented dá recompensa mecânica, cosmética ou apenas informação.
Definir se conhecimento pode ser compartilhado por família inteira ou por variante.
Definir se bosses permitem entry completa após derrota ou após quest final.
Definir como migrar saves se EnemyId mudar.
```
