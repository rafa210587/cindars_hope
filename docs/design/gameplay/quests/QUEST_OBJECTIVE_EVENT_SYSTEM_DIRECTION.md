# Cindar's Hope — Quest / Objective / Event System Direction

> **Status:** direction canônico de sistema genérico de quests, objetivos, condições, triggers, rewards, flags e eventos de gameplay  
> **Local:** `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`  
> - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`  
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> **Função:** definir o sistema transversal de quests/objectives/events que será usado por main quest, side quests, farm orders, festival quests, social quests futuras, cave contracts, tutorial e eventos ocultos.  
> **Não é spec implementável.** Specs futuras devem quebrar este direction em contracts, data assets, runtime, UI, save/load e validações.

---

## 0. Regra de uso

Este documento define o sistema genérico de quest/objective/event.

Ele não substitui:

```text
QUESTS_MAIN_LORE_DIRECTION.md
  define a lore principal da main quest.

QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
  define atos, fragmentos, Fonte, gates, eventos principais e progressão jogável da main quest.

UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
  define como Quest Log, Quest Detail e UI de objetivos aparecem.

SAVE_LOAD_FULL_STATE_DIRECTION.md
  define como quest state, main progression e Fonte são persistidos.
```

Este documento define:

```text
QuestId;
QuestDefinition;
QuestState;
QuestStep;
Objective;
Condition;
Trigger;
Reward;
QuestFlag;
QuestEvent;
Branching;
Expiry/failure;
Quest Log contract;
Save/load contract;
anti-spoiler;
anti-softlock;
integrações com NPC, diálogo, cidade, fazenda, caverna, Fonte, tempo, lua, bestiary, pets e companions.
```

Regra central:

```text
Main quest, side quests, farm orders, social quests futuras, companion quests, festival quests, cave contracts e tutorial devem usar o mesmo sistema base, com categorias diferentes e adapters quando necessário.
```

---

## 1. Problema que este sistema resolve

Sem um sistema genérico de quests, cada feature tende a criar seu próprio fluxo:

```text
main quest com flags próprias;
farm orders com contadores próprios;
social quests com estados próprios;
festival quests com datas próprias;
cave contracts com triggers próprios;
tutorial com hacks próprios;
NPC dialogue com flags próprias;
Fonte com desbloqueios próprios;
```

Isso cria risco de:

```text
duplicidade de estado;
softlock;
quest log inconsistente;
recompensa aplicada duas vezes;
trigger perdido;
quest avançando sem condição;
quest bloqueada por NPC fora de agenda;
quest quebrada por save/load;
spoiler no log;
main quest impossível por item vendido/perdido;
```

Solução:

```text
Todas as quests usam um modelo comum de Definition -> State -> Steps -> Objectives -> Conditions -> Triggers -> Rewards -> Flags.
Cada domínio pode ter objective/reward/condition types próprios, mas o contrato base é o mesmo.
```

---

## 2. Filosofia de design

Quests devem criar intenção clara sem transformar o jogo em checklist mecânico excessivo.

Princípios:

```text
A quest deve dizer o próximo objetivo conhecido.
A quest não deve revelar objetivo secreto cedo.
A quest deve aceitar múltiplas formas de resolver quando fizer sentido.
A quest crítica deve ser recuperável.
A quest deve registrar progresso real, não apenas diálogo.
A quest deve sobreviver a save/load, troca de cena e mudança de horário.
A quest deve usar calendário/clima/lua quando isso enriquecer, não para frustrar.
A quest deve separar estado formal de flags narrativas.
```

Regra de tom:

```text
Cindar's Hope pode ter quests de rotina rural simples e quests de mistério profundo.
O mesmo sistema precisa suportar os dois extremos.
```

---

## 3. Categorias de quest

Categorias canônicas:

```text
Main
Side
FarmOrder
SocialFuture
CompanionFuture
PetFuture
Festival
CaveContract
Tutorial
Hidden
System
```

### 3.1 Main

Usada para:

```text
fragmentos de Anya;
Fonte;
Cindar;
Arco da Memória;
Pedra Negra;
nível 100/101;
Arquivista do Silêncio;
finais Proteger/Selar/Usar.
```

Regras:

```text
Main quest não falha por tempo.
Main quest pode ter escolhas irreversíveis apenas em pontos claramente sinalizados.
Main quest precisa de anti-softlock reforçado.
Main quest não revela boss/final cedo no Quest Log.
```

### 3.2 Side

Usada para:

```text
pedidos narrativos de NPCs;
histórias locais;
pequenas investigações;
lojas;
cidade;
fazenda;
caverna;
```

Regras:

```text
Side quest pode ser opcional.
Side quest pode ter prazo se claramente comunicada.
Side quest importante não deve falhar permanentemente sem aviso forte.
```

### 3.3 FarmOrder

Usada para:

```text
encomendas de crops;
itens processados;
entregas por prazo;
orders sazonais;
pedidos de loja;
shipping especial;
```

Regras:

```text
Pode ter prazo.
Pode expirar.
Pode repetir por tabela.
Não deve bloquear main quest.
Não deve quebrar economia com recompensa infinita.
```

### 3.4 Festival

Usada para:

```text
eventos de calendário;
competição de crop;
minigames;
lojas temporárias;
social boost futuro;
quest hooks de festival;
```

Regras:

```text
Pode expirar quando o festival acaba.
Deve informar data/horário quando conhecido.
Não deve impedir rotina agrícola essencial sem aviso.
```

### 3.5 SocialFuture

Usada para:

```text
friendship;
trust;
personal quests;
romance routes;
casamento;
casamento poliamoroso consentido futuro;
partner helper futuro;
```

Regras:

```text
Feature futura.
Não entra na primeira entrega atual.
Não deve ser caminho obrigatório de poder.
Deve respeitar consentimento e elegibilidade narrativa.
```

### 3.6 CompanionFuture

Usada para:

```text
companion unlock;
companion bond;
companion injury/recovery;
companion farm jobs;
companion cave trust;
```

Regras:

```text
Feature pode depender de companions.
Baseline de caverna continua 1 companion ativo.
```

### 3.7 PetFuture

Usada para:

```text
pet bond;
pet alerts;
pet home upgrades;
pet hints;
```

Regras:

```text
Pet ajuda, mas não resolve quest sozinho.
Pet não substitui companion.
```

### 3.8 CaveContract

Usada para:

```text
caçar inimigo;
coletar recurso;
atingir profundidade;
derrotar elite;
descobrir fraqueza;
mapear área;
```

Regras:

```text
Pode ter risco maior.
Pode expirar por calendário se for contrato.
Não deve conflitar com state da cave run.
```

### 3.9 Tutorial

Usada para:

```text
primeiro plantio;
regar;
colher;
vender;
entrar na cidade;
entrar na caverna;
usar ferramenta;
abrir inventário;
```

Regras:

```text
Tutorial deve ser leve.
Tutorial não deve bloquear jogador experiente sem necessidade.
Tutorial pode ser omitível ou silencioso quando feature já foi usada.
```

### 3.10 Hidden

Usada para:

```text
segredos;
rumores;
Nyx;
Pedra Negra;
eventos de memória;
inscrições;
condições secretas;
```

Regras:

```text
Hidden quest não aparece no log até ser descoberta.
Hidden quest não pode ser necessária para main quest sem pista razoável.
```

---

## 4. Modelo conceitual

### 4.1 QuestDefinition

Representa o contrato autorado da quest.

Campos conceituais:

```text
QuestId
Category
DisplayName
HiddenName
Description
SpoilerTier
StartConditions
AutoStartTriggers
Steps
FailureRules
ExpiryRules
Rewards
QuestFlagsGranted
PrerequisiteQuestIds
BlockedByQuestIds
RepeatPolicy
Trackable
JournalVisibilityPolicy
DebugTags
```

Regra:

```text
QuestDefinition é dado autorado.
QuestDefinition não é estado runtime.
```

### 4.2 QuestState

Representa o estado salvo da quest.

Campos conceituais:

```text
QuestId
State
CurrentStepId
CompletedStepIds
FailedStepIds
ObjectiveStates
KnownObjectiveIds
KnownHints
StartedAtDay
StartedAtTime
CompletedAtDay
ExpiresAtDay
Tracked
Discovered
FailureReason
ChoiceHistory
GrantedRewardIds
GrantedFlagIds
```

Regra:

```text
QuestState é persistido.
QuestDefinition é referenciada por QuestId.
```

### 4.3 QuestStep

Representa uma etapa da quest.

Campos conceituais:

```text
StepId
DisplayName
DescriptionKnown
DescriptionHidden
Objectives
CompletionMode
StartEvents
CompleteEvents
Rewards
NextStepRules
BranchRules
VisibilityPolicy
```

### 4.4 Objective

Representa condição acionável de progresso.

Campos conceituais:

```text
ObjectiveId
ObjectiveType
TargetId
RequiredAmount
CurrentAmount
Conditions
Triggers
VisibilityPolicy
Optional
FailurePolicy
HintText
MapMarkerPolicy
```

---

## 5. QuestState enum

Estados canônicos:

```text
Unknown
Discovered
Available
Active
Waiting
ReadyToComplete
Completed
Failed
Expired
HiddenCompleted
Blocked
```

Significado:

```text
Unknown:
  jogador não sabe que a quest existe.

Discovered:
  jogador descobriu pista, mas quest ainda não está ativa.

Available:
  quest pode ser aceita/iniciada.

Active:
  quest em andamento.

Waiting:
  quest espera tempo, clima, lua, NPC, festival, crafting, processamento ou evento externo.

ReadyToComplete:
  objetivos completos, falta entregar/conversar/confirmar.

Completed:
  quest concluída e recompensas aplicadas.

Failed:
  falhou por condição clara.

Expired:
  prazo terminou.

HiddenCompleted:
  sistema registrou conclusão oculta sem mostrar ao jogador.

Blocked:
  pré-requisito ainda não cumprido ou conflito ativo.
```

---

## 6. CompletionMode

Modos de conclusão de step:

```text
AllObjectivesRequired
AnyObjectiveRequired
ChoiceBranch
Timed
ManualEvent
ScriptedSequence
HiddenCondition
```

Regras:

```text
AllObjectivesRequired exige todos os objectives obrigatórios.
AnyObjectiveRequired permite alternativa.
ChoiceBranch avança conforme escolha.
Timed depende de prazo/tempo.
ManualEvent depende de evento autorado.
ScriptedSequence depende de cena/cutscene/timeline futura.
HiddenCondition só deve ser usada com cuidado e pista suficiente se crítica.
```

---

## 7. Objective types

Objective types iniciais e futuros:

```text
TalkToNpc
ReachLocation
InteractWithObject
CollectItem
DeliverItem
UseItem
PlantCrop
WaterCrop
HarvestCrop
CraftItem
ProcessItem
BuyItem
SellItem
ShipItem
EarnGold
BuildOrUpgrade
FeedAnimalFuture
PetInteractionFuture
CompanionAssignedFuture
DefeatEnemy
DefeatEnemyFamily
SurviveCombat
DiscoverBestiaryKnowledge
DiscoverWeakness
ReachCaveDepth
CompleteCaveRun
RecoverCorpse
UnlockFonteFunction
UpgradeFonte
ProtectFragment
SealFragment
UseFragment
WaitForTime
WaitForDay
WaitForSeason
WaitForWeather
WaitForLunarEvent
AttendFestival
WinFestivalActivityFuture
ReadDocument
LearnSpell
EquipItem
RepairItem
UpgradeItem
MakeDialogueChoice
MakeFinalChoice
```

Regra:

```text
ObjectiveType não deve codificar lógica única demais.
Casos específicos usam TargetId, Conditions e QuestFlags.
```

---

## 8. Conditions

Condition é estado que precisa ser verdadeiro.

Categorias:

```text
QuestCondition
PlayerCondition
InventoryCondition
WorldCondition
TimeCondition
WeatherCondition
LunarCondition
NpcCondition
DialogueCondition
FarmCondition
CaveCondition
CombatCondition
FonteCondition
BestiaryCondition
SocialConditionFuture
PetConditionFuture
CompanionConditionFuture
```

Exemplos:

```text
QuestStateIs(QuestId, Completed)
HasItem(ItemId, Amount)
HasGold(Amount)
CurrentSeasonIs(Winter)
CurrentWeatherIs(Fog)
ActiveLunarEventIs(Nyx)
NpcKnown(NpcId)
NpcAvailable(NpcId)
FonteStageAtLeast(Memory)
CaveDepthReached(Depth)
KnowledgeDiscovered(EnemyId, VulnerabilityTag)
CropHarvested(CropId, Amount)
```

Regras:

```text
Condition não avança quest sozinha.
Condition habilita ou bloqueia objective/step/quest.
Trigger/event avança quando condição permite.
```

---

## 9. Triggers e QuestEvents

Trigger é evento que informa o sistema de quest.

Eventos canônicos:

```text
OnQuestAccepted
OnQuestStarted
OnQuestStepStarted
OnQuestStepCompleted
OnQuestCompleted
OnQuestFailed
OnQuestExpired
OnNpcDialogueStarted
OnNpcDialogueEnded
OnDialogueChoiceSelected
OnItemCollected
OnItemDelivered
OnItemUsed
OnItemCrafted
OnItemProcessed
OnItemSold
OnItemShipped
OnCropPlanted
OnCropWatered
OnCropHarvested
OnBuildingUpgraded
OnLocationReached
OnObjectInteracted
OnEnemyDefeated
OnEnemyFamilyDefeated
OnBossPhaseReached
OnCaveDepthReached
OnCaveRunCompleted
OnPlayerDeath
OnCorpseRecovered
OnFonteFunctionUnlocked
OnFonteUsed
OnFragmentProtected
OnFragmentSealed
OnFragmentUsed
OnBestiaryKnowledgeUnlocked
OnWeaknessDiscovered
OnSpellLearned
OnItemEquipped
OnDayStarted
OnTimeReached
OnSeasonStarted
OnWeatherChanged
OnLunarEventStarted
OnFestivalStarted
OnFestivalEnded
OnShopStockChanged
OnSocialStateChangedFuture
OnPetHintFuture
OnCompanionHintFuture
```

Regra:

```text
Quest system consome eventos de gameplay.
Quest system não deve ser a fonte primária de sistemas como combat, farm, inventory ou economy.
```

---

## 10. Rewards

Reward é efeito declarado aplicado ao completar objective/step/quest.

Reward types:

```text
Gold
Item
Recipe
ToolUnlock
EquipmentUnlock
SpellUnlock
SkillPoint
SkillTreeUnlock
RelationshipFuture
KnowledgeUnlock
BestiaryEntryUnlock
FonteUpgrade
LivingWaterCharge
QuestFlagGrant
QuestFlagClear
AreaUnlock
CaveDepthUnlock
ShopUnlock
ShopStockUnlock
DialogueUnlock
NpcScheduleUnlock
FestivalUnlock
CompanionUnlockFuture
PetUnlockFuture
SocialUnlockFuture
```

Regras:

```text
Reward precisa ser idempotente.
Reward não deve aplicar duas vezes após save/load.
Reward deve registrar GrantedRewardIds quando necessário.
Reward não deve burlar sistemas de economia sem regra explícita.
Reward de conhecimento deve respeitar Bestiary spoiler control.
Reward de Fonte deve respeitar FonteAnyaSection/MainProgression.
```

---

## 11. QuestFlags

QuestFlag é fato persistente consultável por sistemas.

Exemplos:

```text
FonteRespawnUnlocked
FonteLivingWaterUnlocked
FonteRespecUnlocked
FragmentWaterProtected
FragmentMemoryProtected
FragmentLifeProtected
FragmentHopeProtected
CindarDiaryRead
VaelrionIntroduced
SethraKnown
NyxCultRumorKnown
BlackStoneObserved
TownKnowsAnyaRumor
ShopNightUnlocked
CaveGateMemoryOpened
ArchivistNameKnown
```

Regra:

```text
QuestState registra estado formal de uma quest.
QuestFlag registra fatos que outras quests/sistemas podem consultar.
```

Anti-padrão:

```text
Não usar QuestFlag como substituto para todos os estados de quest.
Não esconder progressão complexa em flags soltas quando uma QuestState deveria existir.
```

---

## 12. Branching e escolhas

Branching pode existir em quests.

Tipos:

```text
DialogueChoiceBranch
ObjectiveAlternativeBranch
RewardChoiceBranch
MoralChoiceBranch
FinalChoiceBranch
HiddenConsequenceBranch
```

Regras:

```text
Escolha irreversível deve ser sinalizada.
Escolha cosmética pode ser leve.
Escolha final da main quest precisa de confirmação forte.
Branching não deve quebrar save/load.
Branching deve persistir ChoiceHistory quando consequência futura depender disso.
```

Main quest final choices:

```text
Proteger
Selar
Usar
```

Regra:

```text
Essas escolhas pertencem à main quest e Fonte/Anya, mas usam o sistema genérico de branching e confirmation.
```

---

## 13. Failure e Expiry

Falha/expiração deve ser usada com moderação.

### 13.1 Main quest

```text
Main quest não falha por tempo.
Main quest não expira por calendário.
Main quest não pode ficar impossível por item vendido, NPC indisponível, morte do jogador, lua perdida ou inventário cheio.
```

### 13.2 Farm orders

```text
Podem expirar.
Devem mostrar prazo.
Podem repetir se design permitir.
Não bloqueiam main quest.
```

### 13.3 Festival quests

```text
Podem expirar no fim do festival.
Devem mostrar data/horário quando conhecidas.
```

### 13.4 Social/companion future

```text
Falha permanente deve ser rara.
Consequências devem ser claras.
Recuperação deve ser possível quando narrativamente razoável.
```

---

## 14. Anti-softlock

Toda quest crítica precisa de plano anti-softlock.

Riscos cobertos:

```text
item vendido;
item descartado;
inventário cheio;
NPC fora de schedule;
NPC em evento diferente;
clima raro perdido;
lua rara perdida;
caverna resetada;
run seed alterada;
player morreu;
corpse recovery pendente;
quest reward já aplicado;
step completado antes da quest iniciar;
save/load no meio do evento;
```

Soluções permitidas:

```text
reentrega;
item de quest protegido;
item reaparece em local seguro;
NPC alternativo;
diálogo alternativo;
marcador alternativo;
fallback de calendário;
condição repetível;
trigger retroativo;
normalização no load;
admin/debug repair future;
```

Regra:

```text
Main quest deve ser recuperável sem intervenção externa do jogador fora do jogo.
```

---

## 15. Anti-spoiler

Quest system deve respeitar spoiler control.

Não revelar cedo:

```text
Arquivista do Silêncio;
natureza completa da Pedra Negra;
finais da main quest;
nível 101;
condições exatas de Mana;
identidade completa de eventos ocultos;
fraquezas de bosses não descobertas;
Nyx/culto antes de pista adequada;
```

Quest Log pode mostrar:

```text
objetivo atual conhecido;
pista atual;
NPC relacionado conhecido;
local conhecido;
condição temporal descoberta;
recompensa conhecida;
prazo conhecido;
```

Quest Log não deve mostrar:

```text
steps futuros ocultos;
trigger secreto;
recompensa secreta;
boss oculto;
branch invisível;
condição lunar ainda não descoberta;
```

Regra:

```text
VisibilityPolicy deve existir para quest, step, objective, reward e hint.
```

---

## 16. Integração com UI/Quest Log

Quest Log é apresentação, não fonte de verdade.

UI deve mostrar:

```text
categoria;
título conhecido;
resumo conhecido;
objetivo atual;
progresso numérico quando aplicável;
pista;
NPC/local relacionado;
prazo;
condição temporal descoberta;
recompensa conhecida;
estado: active, waiting, ready, completed, failed, expired;
track/untrack;
```

UI não deve mostrar:

```text
QuestState interno cru;
flags ocultas;
spoiler tier acima do permitido;
objetivos ocultos;
recompensa secreta;
```

Regras:

```text
Quest Log segue UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md.
Quest detail drawer precisa respeitar VisibilityPolicy.
Notificação de quest atualizada deve ser curta e não bloquear gameplay.
```

---

## 17. Integração com Save/Load

Save/load deve persistir:

```text
QuestState;
CurrentStepId;
ObjectiveStates;
KnownObjectiveIds;
KnownHints;
StartedAtDay;
CompletedAtDay;
ExpiresAtDay;
Tracked;
Discovered;
FailureReason;
ChoiceHistory;
GrantedRewardIds;
GrantedFlagIds;
QuestFlags;
```

Separações obrigatórias:

```text
QuestStateSection:
  quests genéricas, side, orders, festival, tutorial, hidden.

MainProgressionSection:
  atos, fragmentos, Cindar, Arco da Memória, nível 100/101, finais.

FonteAnyaSection:
  funções da Fonte, Água Viva, respec, purificação, decisão final, corrupção/estado.
```

Regra:

```text
QuestState, MainProgression e FonteAnya são seções separadas conforme SAVE_LOAD_FULL_STATE_DIRECTION.md.
```

---

## 18. Integração com tempo, clima, luas e festivais

Quest system deve suportar:

```text
WaitForTime;
WaitForDay;
WaitForSeason;
WaitForWeather;
WaitForLunarEvent;
AttendFestival;
FestivalStarted;
FestivalEnded;
```

Regras:

```text
Quest obrigatória com tempo/lua precisa dar pista e controle razoável.
Evento raro não pode ser requisito opaco de main quest.
Festival quest precisa informar data/horário quando descoberta.
Condition temporal descoberta pode aparecer no Quest Log.
Condition temporal secreta não aparece antes de descoberta.
```

---

## 19. Integração com NPCs e diálogo

NPC/dialogue hooks:

```text
OnNpcDialogueStarted;
OnNpcDialogueEnded;
OnDialogueChoiceSelected;
NpcAvailable;
NpcHasMet;
NpcTrustFuture;
NpcServiceUnlocked;
```

Regras:

```text
Quest não deve depender de NPC em horário impossível.
Se NPC está fora do schedule, Quest Log pode mostrar pista de disponibilidade se conhecida.
Diálogo pode iniciar quest, avançar step, entregar reward, revelar condition ou setar QuestFlag.
Diálogo não deve aplicar reward duplicado após reload.
```

---

## 20. Integração com fazenda

Farm objectives:

```text
PlantCrop;
WaterCrop;
HarvestCrop;
DeliverCrop;
ProcessItem;
ShipItem;
BuildOrUpgrade;
UseFonte;
ProtectFragment;
```

Regras:

```text
FarmOrder usa o mesmo sistema base.
Crop quest deve aceitar itens de qualidade quando permitido.
Quest item agrícola não deve ser perdido por shipping acidental se protegido.
Shipping quest processa no day transition se definido.
```

---

## 21. Integração com cidade

City objectives:

```text
TalkToNpc;
VisitBuilding;
BuyItem;
SellItem;
AttendFestival;
ReadNoticeBoard;
AcceptOrder;
UnlockShop;
```

Regras:

```text
Quadro público pode iniciar FarmOrder, CaveContract, Festival quest ou Side quest.
Lojas podem desbloquear stock via quest reward.
NPC schedules são consultados, não duplicados no quest system.
```

---

## 22. Integração com caverna e combate

Cave/combat objectives:

```text
ReachCaveDepth;
DefeatEnemy;
DefeatEnemyFamily;
DefeatBoss;
CollectCaveResource;
DiscoverWeakness;
CompleteCaveRun;
RecoverCorpse;
InteractWithCaveObject;
```

Regras:

```text
CaveContract pode usar enemy family e depth range.
Main quest pode usar boss gates e depth gates.
Defeat objective deve usar EnemyId/FamilyId/BossId estável.
Quest não deve depender de spawn raro sem fallback.
```

---

## 23. Integração com Bestiary/Knowledge

Knowledge objectives:

```text
DiscoverBestiaryKnowledge;
DiscoverWeakness;
ConfirmRumor;
DocumentBehavior;
CollectSample;
ReadLoreNote;
```

Regras:

```text
Bestiary controla conhecimento descoberto.
Quest pode pedir descoberta, mas não deve revelar resposta cedo.
Quest reward pode conceder KnowledgeUnlock.
Equipment UI e Spell UI continuam sem revelar informação desconhecida.
```

---

## 24. Integração com Fonte, Anya e main progression

Fonte/Main objectives:

```text
UpgradeFonte;
UnlockFonteFunction;
UseLivingWater;
ProtectFragment;
SealFragment;
UseFragment;
MakeFinalChoice;
RecoverMemory;
EnterMemoryEvent;
```

Regras:

```text
FonteAnyaSection persiste estado da Fonte.
MainProgressionSection persiste atos e fragmentos.
Quest system pode acionar e observar eventos, mas não deve esconder todo estado da Fonte dentro da quest.
```

---

## 25. Debug/dev validation

Quest system futuro deve permitir validação dev:

```text
listar quests ativas;
listar objectives ativos;
forçar trigger em dev;
ver conditions falhando;
ver rewards já aplicados;
ver flags setadas;
validar referências quebradas;
validar objective sem target;
validar reward sem target;
validar quest sem anti-softlock quando crítica;
validar spoiler tier;
```

Não é UI final.

---

## 26. Data assets futuros

Possíveis assets:

```text
QuestDefinitionSO
QuestStepDataSO
QuestObjectiveDataSO
QuestConditionDataSO
QuestTriggerDataSO
QuestRewardDataSO
QuestFlagDefinitionSO
QuestCategorySO
QuestVisibilityPolicySO
QuestSoftlockPolicySO
QuestDebugValidationProfileSO
```

### 26.1 QuestDefinitionSO

Campos conceituais:

```text
QuestId
Category
DisplayNameKey
HiddenDisplayNameKey
DescriptionKey
SpoilerTier
StartConditionIds
AutoStartTriggerIds
StepIds
FailureRuleIds
ExpiryRuleIds
RewardIds
QuestFlagGrantIds
PrerequisiteQuestIds
BlockedByQuestIds
RepeatPolicy
Trackable
JournalVisibilityPolicyId
```

### 26.2 QuestState save

Campos conceituais:

```text
QuestId
State
CurrentStepId
CompletedStepIds
FailedStepIds
ObjectiveStates
KnownObjectiveIds
KnownHints
StartedAtDay
StartedAtTime
CompletedAtDay
ExpiresAtDay
Tracked
Discovered
FailureReason
ChoiceHistory
GrantedRewardIds
GrantedFlagIds
```

---

## 27. Specs futuras recomendadas

```text
spec_quest_objective_event_contract_runtime.md
spec_quest_state_save_load_runtime.md
spec_quest_condition_trigger_runtime.md
spec_quest_reward_application_idempotency_runtime.md
spec_quest_log_visibility_spoiler_runtime.md
spec_quest_flags_registry_runtime.md
spec_quest_farm_orders_adapter_runtime.md
spec_quest_festival_expiry_runtime_future.md
spec_quest_bestiary_discovery_objectives_future.md
spec_quest_fonte_main_progression_hooks_future.md
spec_quest_debug_validation_tools_future.md
spec_quest_anti_softlock_validation_future.md
```

---

## 28. Anti-regressão

```text
QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md é fonte canônica de sistema genérico de quests, objectives, conditions, triggers, rewards, quest flags e quest events.
Main quest lore/progression continuam em documentos próprios.
Main quest, side quests, farm orders, social future, companion future, pet future, festival, cave contracts e tutorial usam o mesmo sistema base.
QuestState, MainProgression e FonteAnya são seções separadas de save/load.
QuestFlag não substitui QuestState.
Condition não avança quest sozinha; Trigger/event avança quando condições permitem.
Reward precisa ser idempotente.
Quest Log não revela objetivo, reward, boss, condição ou final oculto cedo.
Main quest não falha por tempo.
Main quest não pode ficar impossível por item vendido, NPC fora de schedule, clima/lua perdido, morte do jogador ou inventário cheio.
FarmOrder e Festival quests podem expirar se prazo for claro.
Hidden quest não aparece no log até ser descoberta.
Quest crítica precisa de anti-softlock.
Quest system consome eventos de gameplay; não substitui farm, combat, inventory, economy, bestiary ou Fonte.
```

---

## 29. Decisões fechadas

```text
Todas as categorias de quest usam o mesmo sistema base.
Quest é composta por steps e objectives.
Condition e Trigger são separados.
QuestFlag e QuestState são separados.
MainProgression e FonteAnya ficam fora do QuestState genérico.
Quest Log mostra apenas conhecimento autorizado.
Main quest não expira por tempo.
Farm orders e festival quests podem expirar.
Quest crítica precisa de fallback anti-softlock.
```

---

## 30. Pendências abertas

```text
Definir se QuestDefinitionSO será único asset por quest ou asset composto por steps/objectives separados.
Definir naming final de QuestId.
Definir sistema de localização/text keys.
Definir quais quest categories entram na primeira entrega.
Definir se FarmOrders entram como QuestCategory ou sistema de board com adapter.
Definir se Hidden quests aparecem após conclusão como HiddenCompleted.
Definir se haverá tracked quest marker no HUD inicial.
Definir se QuestFlags terão registry próprio.
Definir ferramenta dev de validação.
Definir política final de repeatable quests.
```
