# Cindar's Hope — Save/Load Full State Direction

> **Status:** direction canônico de persistência full-state, capture/restore, migration, providers e contratos de save/load  
> **Local:** `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`  
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> - `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md`  
> - `docs/specs/implementados/spec_save_002_schema_migration_v2.md`  
> **Função:** definir o estado alvo de save/load full-state do jogo, a partir do save/load já existente no código, sem reimplementar do zero e sem travar números de versões futuras.  
> **Não é spec implementável.** Specs futuras devem quebrar esta direção em specs de runtime, migrations e validações.

---

## 0. Decisão de escopo

Este documento não cria uma nova implementação de save/load do zero.

O repo já possui:

```text
SaveManager;
GameSaveData;
SchemaVersion;
Migration registry;
Safe write com .tmp;
Save/load cross-scene;
DTOs simples;
seções de save já existentes para vários sistemas.
```

Este documento define o estado final desejado, as regras de ownership, capture/restore, preservação por cena, anti-regressões e lacunas frente aos refinamentos atuais.

Fora de escopo deste direction:

```text
alterar código C#;
subir SchemaVersion;
criar migrations reais;
criar specs implementáveis;
modificar SaveManager agora;
implementar providers agora;
implementar UI de save/load;
implementar política final de save em caverna.
```

Decisões fechadas:

```text
Não vamos trabalhar aqui com números de versões futuras.
Este direction fala em objetivos finais e depois cada mudança vira spec própria.
Save em caverna pode continuar permitido por enquanto.
A política final de restrição de save em caverna será refinada depois.
SaveManager continua como orquestrador atual.
O alvo futuro é migrar gradualmente para providers por seção.
UI state não deve ser salvo como fonte de verdade, salvo preferências futuras.
Quest/Main/Fonte devem ser seções separadas.
```

---

## 1. Estado atual real do repo

O save/load atual já é amplo e deve ser tratado como base existente.

Estado observado:

```text
CurrentSchemaVersion atual do código: 5.
Save usa JSON.
Save usa Application.persistentDataPath/saves/slot_1.json.
SaveManager registra migrations sequenciais já existentes.
SaveManager usa escrita segura com .tmp.
GameSaveData é DTO raiz versionado.
SaveManager captura e restaura múltiplos sistemas.
Bestiary já aparece como seção de GameSaveData.
SkillTree e ActiveSkillSlots já aparecem como seções de GameSaveData.
GameTime já aparece como seção de GameSaveData.
```

Seções existentes no `GameSaveData` atual:

```text
SchemaVersion;
CurrentDay;
CurrentSceneName;
CurrentScenePath;
Player;
Inventory;
Equipment;
Hotbar;
Progression;
Farm;
World;
Cave;
Death;
Economy;
Crafting;
Stamina;
EquipmentDurability;
Npcs;
GameTime;
PlayerStatusEffects;
ActiveSkillSlots;
SkillTree;
Bestiary.
```

Leitura correta:

```text
O save/load atual já passou do MVP mínimo.
O próximo trabalho não é criar save/load.
O próximo trabalho é consolidar full-state, reduzir acoplamento, fechar lacunas e garantir que novos refinamentos entrem por contrato.
```

---

## 2. Problema atual

O save/load já existe, mas o jogo está crescendo em vários eixos:

```text
tempo/calendário/clima/luas;
Fonte de Anya;
fragmentos de Anya;
main quest;
quests gerais;
Mana;
pets;
companions;
social/romance futuro;
partner helper futuro;
inventory/equipment mais avançado;
item instances;
crafting/processing;
shops/restock/economia;
cave run/snapshots;
death/corpse recovery;
bestiary/knowledge discovery;
UI menus e screen flows;
NPC schedules por clima/lua/festival.
```

Sem um direction transversal, cada spec futura pode:

```text
adicionar campos de save em lugar errado;
salvar estado derivado;
serializar referência Unity por acidente;
esquecer migration;
quebrar load de saves antigos;
restaurar sistemas fora de ordem;
sobrescrever seção ausente por default;
criar duplicidade entre managers;
salvar UI state como se fosse gameplay state;
expor spoiler por save/load;
perder dados ao salvar fora da cena onde o sistema vive.
```

Este documento resolve isso definindo:

```text
quem é dono de cada seção;
quando capturar;
quando preservar;
como restaurar;
o que nunca salvar;
o que salvar só por ID;
o que é global;
o que é scene-bound;
o que é derivado;
como novos refinamentos devem pedir save/load.
```

---

## 3. Princípios canônicos

```text
Save/load persiste estado necessário, não objetos Unity.
DTOs de save usam tipos simples e IDs estáveis.
ScriptableObjects, GameObjects, Transforms, MonoBehaviours, Sprites, Colliders e Rigidbodies nunca entram em DTO.
Estado derivado deve ser recalculado no load.
Estado visual de UI não é fonte de verdade.
O SaveManager atual é o orquestrador, mas o alvo é provider architecture por seção.
Salvar fora da cena de um sistema deve preservar a seção existente se o sistema não está carregado.
Migration é obrigatória quando uma spec muda shape de dados persistidos.
Toda nova seção de save deve ter owner, capture policy, restore policy, defaults e validação.
```

Regra central:

```text
Nenhuma spec futura pode adicionar estado persistido sem declarar:
  seção de save;
  dono;
  IDs usados;
  quando captura;
  quando preserva;
  restore order;
  migration necessária ou justificativa de não precisar;
  validação mínima.
```

---

## 4. Arquitetura alvo

## 4.1 Curto prazo

```text
SaveManager continua agregador central.
Novas specs respeitam o GameSaveData atual.
Novas specs não refatoram tudo para providers de uma vez.
Save em caverna continua permitido por enquanto.
```

## 4.2 Médio prazo

```text
Sistemas começam a expor capture/restore por provider.
SaveManager passa a delegar seções.
Cada provider declara seção, owner e prioridade de restore.
Preservação cross-scene fica padronizada.
```

## 4.3 Longo prazo

```text
SaveManager vira orquestrador de providers.
GameSaveData continua DTO raiz.
Providers cuidam de captura/restore/normalização de suas seções.
Migrations continuam globais e sequenciais.
Validações de save por seção ficam padronizadas.
```

---

## 5. Provider architecture alvo

Possível contrato futuro:

```text
ISaveSectionProvider
  SectionId
  CapturePolicy
  RestoreOrder
  Capture(GameSaveData target, SaveCaptureContext context)
  Restore(GameSaveData source, SaveRestoreContext context)
  Validate(GameSaveData source, SaveValidationContext context)
  Normalize(GameSaveData source)
```

O provider deve saber:

```text
se o sistema está carregado;
se a cena atual contém runtime da seção;
se deve capturar runtime atual;
se deve preservar seção existente;
se deve aplicar defaults;
se deve bloquear load por dado inválido;
```

Regra:

```text
Provider não substitui migration.
Provider captura/restaura seção atual.
Migration transforma shape de save entre schemas.
```

---

## 6. Capture policy por cena

Toda seção deve declarar uma política de captura.

Políticas:

```text
AlwaysCapture:
  sistema global em memória; capturar sempre.

CaptureIfSceneLoaded:
  capturar se runtime da cena está carregado.

PreserveIfSceneMissing:
  se runtime não existe na cena atual, preservar seção anterior do save.

DeriveOnLoad:
  não salvar; recalcular no load.

FutureOnly:
  contrato reservado, sem runtime atual.
```

Regra atual recomendada:

```text
Sistemas globais capturam sempre.
Sistemas scene-bound preservam seção anterior quando a cena não está carregada.
Salvar fora da Farm não pode apagar Farm.
Salvar fora da Town não pode apagar NPCs de Town.
Salvar fora da Cave não pode apagar Cave state se ele for necessário.
```

---

## 7. Restore order canônico

A ordem de restore deve ser explícita para evitar dependência quebrada.

Ordem alvo:

```text
1. Ler JSON bruto.
2. Detectar schema e aplicar migrations necessárias.
3. Validar e normalizar GameSaveData.
4. Carregar cena correta ou decidir fallback seguro.
5. Inicializar bootstrap e registries globais.
6. Restaurar tempo/mundo global.
7. Restaurar player core.
8. Restaurar inventory.
9. Restaurar equipment.
10. Restaurar hotbar.
11. Restaurar progression/skill tree/active slots.
12. Restaurar farm/world scene-bound state.
13. Restaurar economy/shop/crafting/processing.
14. Restaurar NPCs e schedules.
15. Restaurar pets/companions futuros.
16. Restaurar quests/main progression.
17. Restaurar Fonte/Anya/fragments.
18. Restaurar cave/death/corpse recovery.
19. Restaurar bestiary/knowledge discovery.
20. Recalcular HUD/UI derivada.
21. Publicar eventos de runtime carregado, se necessário.
```

Regras:

```text
Hotbar depende de inventory/equipment.
Active slots dependem de skill tree/skill action registry.
Equipment depende de inventory/item instances.
NPC schedules dependem de time/calendar/weather/lunar state.
Fonte/Anya pode depender de main progression, mas deve ter seção própria.
Bestiary knowledge não deve desbloquear spoiler acima do progresso salvo.
UI nunca deve ser fonte primária do restore.
```

---

## 8. Section ownership alvo

### 8.1 PlayerSection

Dono:

```text
PlayerManager / Player systems
```

Persistir:

```text
HP atual/máximo;
Mana atual/máximo;
Gold;
Hunger;
posição;
cena atual;
```

Não persistir:

```text
animação atual;
facing transitório se não houver regra;
input atual;
VFX/SFX;
UI aberta.
```

### 8.2 InventorySection

Persistir:

```text
Inventory slots;
ItemId;
ItemInstanceId futuro;
Amount;
SlotIndex;
locked/favorite futuro;
quest/key protection derivada por item definition, não hardcoded no save.
```

Regra:

```text
Item definition não é salva; salva-se ID.
ItemInstance salva atributos mutáveis, se existirem.
```

### 8.3 EquipmentSection

Persistir:

```text
slots equipados;
ItemInstanceId;
durabilidade;
upgrades futuros;
bindings necessários;
```

Regra:

```text
Equipment UI não é salva.
Comparison drawer não é salvo.
```

### 8.4 HotbarSection

Persistir:

```text
slots da hotbar;
bindings por ItemId/ItemInstanceId/SkillActionId conforme tipo;
```

Regra:

```text
Hotbar deve validar se item/skill ainda existe após migration/load.
Binding inválido deve ser limpo com log claro, não quebrar load.
```

### 8.5 SkillTreeSection

Persistir:

```text
skill points;
nodes comprados;
ranks;
capstones escolhidos;
respec state se necessário;
```

Não persistir:

```text
layout visual da árvore;
node hover;
drawer aberto;
preview de compra.
```

### 8.6 ActiveSkillSlotsSection

Persistir:

```text
4 active slots;
SkillActionId por slot;
```

Regra:

```text
Active slot inválido após migration deve ser removido ou marcado inválido de forma segura.
```

### 8.7 FarmSection

Persistir:

```text
plots;
crops;
watered state;
growth state;
fertilizer;
quality inputs;
trees;
buildings futuros;
animals futuros;
shipping/sell pending se pertencer à fazenda;
Fonte local visual derivado de FonteAnyaSection, não duplicado.
```

Regra:

```text
Salvar fora da Farm preserva FarmSection anterior se runtime da Farm não está carregado.
```

### 8.8 WorldSection

Persistir:

```text
pickups persistentes;
árvores do mundo;
coletáveis persistentes;
estado de objetos únicos;
```

Regra:

```text
Pickup coletado deve permanecer coletado após load.
Pickup não deve ser destruído sem persistir IsCollected.
```

### 8.9 TimeCalendarWeatherSection

Persistir alvo:

```text
CurrentTime;
CurrentDay;
CurrentSeason;
CurrentYear;
CurrentWeather;
TomorrowWeather;
ActiveLunarEvent;
KnownLunarEvents;
FestivalState;
WeatherSeed;
CalendarEventStates;
PendingDayTransitionState;
LastProcessedDay;
```

Observação:

```text
GameTime já existe no GameSaveData atual, mas ainda precisa evoluir para cobrir calendário, clima, estação e lua quando esses sistemas forem implementados.
```

### 8.10 EconomySection

Persistir:

```text
shop stock state;
limited/unique counters;
last restock day;
pending payments;
orders/encomendas futuras;
```

Não persistir:

```text
preço final calculado como fonte primária;
texto renderizado de preço;
UI de loja.
```

### 8.11 CraftingProcessingSection

Persistir:

```text
crafting jobs em andamento;
station id;
recipe id;
start time/day;
remaining time ou completion target;
output pending;
```

Regra:

```text
Processing deve sobreviver a troca de cena e passagem de dia.
```

### 8.12 NpcScheduleSection

Persistir atual/futuro:

```text
NpcId;
SceneId;
posição se necessário;
HasMet;
estados persistentes de NPC;
flags de evento;
```

Não persistir:

```text
pathfinding atual;
waypoint transitório;
animação;
rotina derivada de horário/clima/lua;
```

Regra:

```text
Schedule deve ser recalculado a partir de time/calendar/weather/lunar state e city schedule rules.
```

### 8.13 QuestStateSection

Persistir:

```text
QuestId;
state;
current step;
completed steps;
failed/expired flags;
tracked quest;
known objectives;
temporal conditions discovered;
items delivered;
NPC states vinculados à quest;
```

Regra:

```text
Quest state não deve ficar escondido dentro de NPC save.
NPC pode ter flags, mas quest log precisa de seção própria.
```

### 8.14 MainProgressionSection

Persistir:

```text
ato atual;
fragmentos conhecidos;
fragmentos protegidos;
fragmentos usados/selados;
Arco da Memória state;
Cindar knowledge state;
Vaelrion/Sethra/Yael critical flags;
nível 100/101 flags;
final choice state;
```

Regra:

```text
Main progression é separado de QuestState genérico.
```

### 8.15 FonteAnyaSection

Persistir:

```text
FonteStage;
respawn unlocked;
Água Viva unlocked;
respec unlocked;
purification unlocked;
final decision unlocked;
charges/cooldowns se existirem;
corruption state;
visual state derivável;
```

Regra:

```text
Fonte não deve ser persistida apenas como objeto da Farm.
Fonte é sistema próprio ligado a Anya e main progression.
```

### 8.16 CaveSection

Persistir atual/futuro:

```text
active run;
run seed;
floor/depth;
checkpoint;
boss gates;
snapshots;
resources/mining state;
loot state;
spawn state quando necessário;
```

Decisão atual:

```text
Save em caverna pode continuar permitido por enquanto.
Política final de restrição/checkpoint será refinada depois.
```

Regra atual:

```text
Enquanto save em caverna é permitido, load precisa restaurar run state de forma consistente ou aplicar fallback explícito.
```

### 8.17 DeathCorpseRecoverySection

Persistir:

```text
player death state;
corpse position;
corpse scene;
items/gold lost if applicable;
recovery state;
checkpoint/fonte return state;
```

Regra:

```text
Death/corpse não deve depender apenas da cena atual.
```

### 8.18 BestiaryKnowledgeSection

Persistir atual/futuro:

```text
EnemyKnowledgeStates;
KnownEnemyIds;
KnownFamilyIds;
KnownDropIds;
KnownVulnerabilityTagsByEnemy;
KnownResistanceTagsByEnemy;
KnownImmunityTagsByEnemy;
KnownBehaviorWindowsByEnemy;
KnownLoreNoteIds;
KnowledgeRumors;
KnowledgeConfirmedFlags;
BestiaryVersion/shape marker se necessário;
```

Regra:

```text
Bestiary UI não entra na primeira entrega, mas conhecimento descoberto futuro precisa ser persistível.
```

### 8.19 PetSectionFuture

Persistir futuro:

```text
PetId;
active pet;
bond;
mood;
energy;
food state;
home area;
bed/bowl/toy state;
routine state relevante;
```

Regra:

```text
Pet não conta como companion.
Pet state não deve ficar misturado com companion state.
```

### 8.20 CompanionSectionFuture

Persistir futuro:

```text
unlocked companions;
active companion;
bond;
injury/downed/recovery state;
farm job state;
cave availability;
schedule overrides;
```

Regra:

```text
Baseline de caverna continua 1 companion ativo.
Partner companion futuro deve usar os mesmos contratos de companion, sem duplicar.
```

### 8.21 SocialRelationshipSectionFuture

Persistir futuro:

```text
friendship;
trust;
gift history;
known preferences;
personal quest states;
romance state;
marriage/partner state;
polycule consent state;
partner helper assignments;
farm visit states;
```

Regra:

```text
Social/romance/poliamor é futuro e não entra na primeira entrega atual, mas deve ter seção própria quando entrar.
```

---

## 9. UI state e save/load

UI state não é fonte de verdade.

Não salvar como gameplay state:

```text
menu aberto;
modal stack;
tooltip aberto;
hover atual;
item selecionado;
scroll position;
filtro de inventory;
aba de skill tree aberta;
drawer de item aberto;
quest selecionada;
calendário aberto;
shop tab aberta;
crafting recipe selecionada;
Bestiary page aberta.
```

Pode salvar futuramente como preferência, fora do gameplay save:

```text
configurações de acessibilidade;
volume;
idioma;
escala de UI;
keybindings;
opções visuais;
preferência de sort se for UX preference, não estado de gameplay.
```

Regra:

```text
HUD e menus devem ser reconstruídos a partir do runtime restaurado.
```

---

## 10. IDs estáveis obrigatórios

Todo estado persistido deve usar IDs estáveis.

IDs esperados:

```text
ItemId;
ItemInstanceId;
SkillId;
SkillActionId;
SpellId;
QuestId;
NpcId;
PetId;
CompanionId;
EnemyId;
EnemyFamilyId;
BestiaryEntryId;
SceneId;
BuildingId;
CropId;
RecipeId;
ShopId;
StationId;
WeatherId;
LunarEventId;
FestivalId;
FonteStageId;
FragmentId;
PickupId;
TreeId;
CaveRunId;
BossGateId;
CheckpointId;
```

Regra:

```text
Se um sistema não tem ID estável, ele não está pronto para persistência robusta.
```

---

## 11. O que nunca salvar

Nunca persistir em DTO:

```text
ScriptableObject;
GameObject;
Transform;
MonoBehaviour;
Sprite;
Collider;
Rigidbody;
Animator;
AudioSource;
ParticleSystem;
Tilemap reference;
Scene object reference;
Delegate/event subscription;
Coroutine;
Task runtime;
Pathfinding current path;
VFX runtime;
SFX runtime;
computed tooltip;
rendered text;
final price calculado;
shop UI list;
modal stack;
mouse hover state;
```

Persistir por ID e estado simples.

---

## 12. Defaults, normalização e seção ausente

Toda seção nova precisa declarar default seguro.

Regras:

```text
Se seção antiga não existe em save legado, migration ou normalização deve criar default seguro.
Se seção existe mas tem dados inválidos, validar e corrigir quando possível.
Se dado inválido compromete integridade, rejeitar load com erro claro.
Defaults não podem apagar progresso existente.
Defaults não podem revelar conteúdo futuro.
```

Exemplo:

```text
Bestiary ausente:
  criar estado vazio sem revelar conhecimento.

FonteAnya ausente:
  criar estágio inicial compatível com progresso antigo, ou migration específica se houver main quest.

TimeCalendarWeather ausente:
  usar CurrentDay existente e defaults de clima/estação seguros.
```

---

## 13. Migrations por objetivo, não por número neste direction

Este documento não trava números futuros de schema.

Cada spec que alterar payload persistido deve decidir:

```text
precisa migration?
não precisa migration?
por quê?
qual seção muda?
qual default para saves antigos?
qual validação negativa?
qual backup esperado?
```

Objetivos futuros que provavelmente exigem migration:

```text
Time/Calendar/Weather/Lunar full state;
FonteAnyaSection;
QuestState/MainProgression sections;
full ItemInstance/equipment upgrades;
PetSection;
CompanionSection;
SocialRelationshipSection;
Bestiary knowledge confidence/spoiler tiers;
Farm buildings/animals expansion;
Cave save policy final;
```

Regra:

```text
Não subir schema apenas por desejo de organização.
Subir schema quando shape de save muda ou quando default/migration precisa ser aplicado.
```

---

## 14. Save em caverna

Decisão atual:

```text
Save em caverna continua permitido por enquanto.
```

Motivo:

```text
Não vamos bloquear fluxo atual antes de refinar política final de caverna.
```

Regras enquanto permitido:

```text
Save deve preservar run seed.
Save deve preservar floor/depth/checkpoint relevante.
Save deve preservar snapshot se sistema exigir.
Load deve restaurar estado consistente ou aplicar fallback explícito.
Falha de restore de caverna não deve corromper save inteiro sem mensagem clara.
```

Políticas futuras a discutir depois:

```text
save livre em qualquer ponto;
save apenas em checkpoint/fonte/camp;
save em qualquer ponto, mas load retorna ao checkpoint;
save fora de combate apenas;
save desabilitado em boss/floor especial.
```

Nada disso é fechado aqui.

---

## 15. Validação futura obrigatória

Matriz mínima de validação Play Mode futura:

```text
Salvar na Farm e carregar na Farm.
Salvar na Town e carregar na Town.
Salvar na Cave e carregar na Cave.
Salvar na Cave durante run e restaurar run state.
Salvar depois de morrer e validar corpse recovery.
Salvar com inventory slots preenchidos.
Salvar com equipment equipado e durabilidade alterada.
Salvar com hotbar bindings.
Salvar com skill tree comprada.
Salvar com active skill slots.
Salvar com shop stock alterado.
Salvar com crafting/processing em andamento.
Salvar com NPC conhecido e schedule recalculado após load.
Salvar com day/time avançado.
Salvar com weather/lunar/festival futuro.
Salvar com Fonte stage avançado futuro.
Salvar com quest state futuro.
Salvar com bestiary entry parcial futuro.
Salvar em cena que não contém Farm e validar preservação de FarmSection.
Salvar em cena que não contém Town/NPCs e validar preservação de NpcSection.
```

Validações negativas:

```text
schema futuro rejeitado claramente;
JSON corrompido rejeitado claramente;
ID inexistente tratado com fallback seguro ou erro claro;
ItemInstance ausente não quebra load inteiro sem log;
SkillActionId removido limpa slot ativo com log;
SceneId inválido usa fallback seguro;
migration falha sem sobrescrever save original;
```

---

## 16. Relação com primeira entrega

Na primeira entrega executável atual:

```text
não implementar social/romance save;
não implementar pet/companion full save se feature não entra;
não implementar bestiary UI save;
não implementar Fonte/main quest full state se feature não entra;
não restringir save em caverna ainda;
não refatorar todo SaveManager para providers de uma vez.
```

Permitido preparar quando barato:

```text
IDs estáveis;
hooks vazios;
seções vazias com defaults seguros;
interfaces futuras;
logs;
validação documental;
normalização conservadora;
campos que não expõem UI/promessas ao jogador.
```

---

## 17. Specs futuras recomendadas

```text
spec_save_provider_architecture_runtime.md
spec_save_restore_order_contract_runtime.md
spec_save_section_ownership_registry.md
spec_save_time_calendar_weather_lunar_state_future.md
spec_save_quest_main_fonte_state_future.md
spec_save_item_instance_equipment_upgrade_state_future.md
spec_save_pet_companion_social_state_future.md
spec_save_bestiary_knowledge_state_future.md
spec_save_cave_policy_checkpoint_future.md
spec_save_playmode_validation_matrix.md
spec_save_invalid_id_fallback_rules.md
```

---

## 18. Anti-regressão

```text
Save/load não deve ser refeito do zero.
SaveManager é o orquestrador atual.
Provider architecture é alvo gradual, não refactor massivo imediato.
Não trabalhar com números futuros de schema neste direction.
Cada spec futura define migration se alterar payload persistido.
Save em caverna continua permitido por enquanto.
Política final de save em caverna será refinada depois.
DTOs de save usam IDs e tipos simples.
Não serializar referências Unity.
UI state não é gameplay state e não deve ser salvo no save principal.
HUD deriva do runtime restaurado.
Salvar fora da cena de um sistema não pode apagar a seção desse sistema.
Seção ausente em save legado precisa de default seguro ou migration.
QuestState, MainProgression e FonteAnya devem ser seções separadas.
Pet, Companion e Social futuro devem ser seções separadas.
Bestiary knowledge futuro deve persistir conhecimento descoberto sem revelar spoiler.
Fonte não deve ficar persistida apenas dentro de FarmSection.
Schedules de NPC devem ser recalculados por tempo/clima/lua, não salvos como pathfinding transitório.
Preço final calculado não é fonte primária de save.
Migration falha não pode corromper save original.
```

---

## 19. Decisões fechadas

```text
O direction parte do estado real atual: SaveManager + GameSaveData + migrations já existem.
O objetivo é consolidar full-state, não substituir o sistema.
Não haverá planejamento por número de versão neste documento.
Objetivos futuros de persistência serão transformados em specs específicas.
Save em caverna permanece permitido por enquanto.
SaveManager segue como agregador atual.
Alvo futuro: providers por seção.
UI state não será salvo como gameplay state.
Quest/Main/Fonte serão separadas.
Social/romance/poliamor/pets/companions ficam futuros se as features não entrarem na primeira entrega.
```

---

## 20. Pendências abertas

```text
Definir quando migrar SaveManager para providers.
Definir spec de provider architecture.
Definir política final de save em caverna.
Definir seção final de QuestState.
Definir seção final de MainProgression.
Definir seção final de FonteAnya.
Definir seção final de TimeCalendarWeatherLunar.
Definir seção final de Pet/Companion/Social.
Definir se haverá save slots múltiplos.
Definir se haverá autosave.
Definir se haverá backup visível ao jogador.
Definir UI futura de save/load.
Definir matriz Play Mode final obrigatória antes de promover save/load para final.
```
