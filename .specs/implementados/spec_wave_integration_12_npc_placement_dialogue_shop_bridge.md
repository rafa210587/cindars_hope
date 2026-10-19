# spec_wave_integration_12_npc_placement_dialogue_shop_bridge.md

required_adrs: []
required_game_rules: []

Ordem de execucao: WAVE_INTEGRATION_12
Depende de: WAVE_INTEGRATION_11_skill_effects_gameplay_bridge
Bloqueia: next integration wave ate BUILD_VALIDATED_SCENE_WIRED_PENDING_HUMAN_PLAYMODE

# /speckit.specify
# /speckit.plan
# /speckit.tasks

> **Projeto:** Cindar's Hope  
> **Wave:** WAVE_INTEGRATION — Scene Wiring e Playable Runtime Bridge  
> **Spec:** 12 — NPC Placement, Dialogue e Shop Bridge  
> **Tipo:** Integração Unity / NPC Runtime / Dialogue UI / Shop Bridge / Scene Wiring  
> **Status inicial:** A implementar  
> **Prioridade:** P0 — cria o primeiro loop social/comercial visível do playable slice  
> **Referência do arquivo mestre:** `WAVE_07_PLAYABLE_SCENE_INTEGRATION_ROADMAP_MACRO.md` → `SPEC 12 — NPC Placement, Dialogue e Shop Bridge`  
> **Dependência direta:** `WAVE_INTEGRATION_11_skill_effects_gameplay_bridge`  
> **Resultado esperado:** pelo menos um NPC aparece em cena com diálogo funcional e pelo menos um NPC/objeto comercial abre fluxo de shop buy/sell usando inventory/economy real ou debt explícito, sem quebrar player, HUD, inventory, input/focus/modal ou scenes.

---

## 0. Intenção desta spec

Esta spec cria o primeiro loop social/comercial visível.

Até aqui, a WAVE_INTEGRATION deve ter criado ou preparado:

```text
player visível
FarmScene organizada
crop/resource interactables
inventory/economy/shipping bridge
HUD mínimo
inventory UI
skill tree/effects bridge
```

Agora o jogo precisa começar a parecer habitado:

```text
NPC visível → player aproxima → prompt → diálogo abre → escolha/opção → resposta/fechamento
NPC/merchant visível → player interage → shop abre → buy/sell mínimo → inventory/economy atualiza
```

Esta spec não deve criar narrativa final, árvores completas de relacionamento, calendário social complexo, quests finais, reputação completa ou cidade final. Ela deve conectar NPC, diálogo e shop em um vertical slice jogável e extensível.

---

## 1. Nome e localização

Arquivo desta spec:

```text
.specs/a_implementar/WAVE_INTEGRATION_12_npc_placement_dialogue_shop_bridge.md
```

Padrão oficial:

```text
WAVE_INTEGRATION_{NUMBER_SPEC}_{slug}.md
```

---

## 2. Objetivo

Conectar NPC placement, dialogue runtime/UI e shop buy/sell runtime/UI a uma scene real.

## 2.1 Escopo revisado: roster canônico de NPCs

Esta spec não deve criar apenas 1 NPC de teste se já existir uma lista canônica de NPCs nos documentos de design/direction.

A execução deve primeiro descobrir o roster canônico do MVP:

```text
1. NPCs da FarmScene;
2. NPCs da TownScene/cidade inicial;
3. NPCs comerciantes;
4. NPCs de tutorial;
5. NPCs de diálogo/lore;
6. NPCs de serviço/profissão;
7. NPCs explicitamente marcados como MVP/playable slice;
8. NPCs futuros, se houver, devem ficar em future scope.
```

O agente deve criar:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
```

Esse documento deve conter todos os NPCs encontrados nos design/directions aplicáveis.

Se nenhum roster canônico existir, o agente deve criar:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_ROSTER_GAP_REPORT.md
```

e marcar:

```text
Status: BLOCKED_BY_MISSING_CANONICAL_NPC_ROSTER
```

ou seguir apenas com `TEMPORARY_TEST_NPCS`, sem declarar que “todos os NPCs” foram criados.

---

## 2.2 Regra importante: não criar uma classe por NPC

Não é desejável criar uma classe C# para cada NPC como padrão.

Padrão correto:

```text
NpcDefinition data-driven por NPC;
NpcScenePlacementMarker por instância em scene;
NpcSceneInteractable genérico;
NpcDialogueBridge genérico;
NpcShopBridge genérico;
NpcMovementSchedule genérico;
NpcDailyRoutineController genérico;
NpcRole/Responsibility data-driven.
```

Criar classe específica por NPC somente se houver comportamento realmente único e justificado:

```text
Exemplo aceitável:
BlacksmithNpcBehavior
HealerNpcBehavior
QuestCriticalNpcBehavior
```

Exemplo proibido como padrão:

```text
BobNpc.cs
MariaNpc.cs
Npc001.cs
Merchant01Npc.cs
```

A spec deve registrar:

```text
NPC implementation model: DATA_DRIVEN_GENERIC_CONTROLLERS
```

Se o agente criar uma classe por NPC sem justificativa, marcar:

```text
NEEDS_REWORK_NPC_CLASS_PER_CHARACTER
```

---

## 2.3 Campos obrigatórios por NPC

Cada NPC do roster canônico deve ter pelo menos:

```text
NpcId estável;
DisplayName;
Role;
Purpose;
Responsibilities;
PrimaryScene;
SpawnPosition/PlacementMarker;
MovementProfile;
DialogueSetId;
MinimumDialogueOptions;
ShopId, se merchant;
ScheduleId, se tiver rotina;
Relationship/Reputation hooks, mesmo que deferred;
Quest hooks, mesmo que deferred;
FutureScope flag, se não for MVP.
```

Template obrigatório no roster:

```md
| NpcId | DisplayName | Role | Purpose | Responsibilities | PrimaryScene | Placement | MovementProfile | DialogueSetId | DialogueOptions | ShopId | MVP? | Status |
|---|---|---|---|---|---|---|---|---|---:|---|---:|---|
```

Status possíveis:

```text
PLACED_AND_WIRED
PLACED_DIALOGUE_ONLY
PLACED_SHOP_ONLY
AUTHORING_READY_HUMAN_WIRING_REQUIRED
FUTURE_SCOPE
BLOCKED_BY_MISSING_DESIGN
```

---

## 2.4 Diálogo obrigatório: pelo menos 10 opções/entradas por NPC MVP

Para cada NPC MVP, a spec deve construir ou exigir um `DialogueSet` com pelo menos 10 entradas/opções de diálogo.

Não é necessário escrever narrativa final/lore final se não existir design suficiente. Mas é obrigatório criar uma estrutura expansível.

Cada NPC MVP deve ter no mínimo estes tipos de entrada:

```text
1. Greeting padrão;
2. Quem é você / função;
3. Sobre o local/cidade/farm;
4. Dica de gameplay ou sistema;
5. Rumor leve/lore não-spoiler;
6. Estado contextual por horário/clima/dia, se runtime existir;
7. Opção relacionada ao shop/serviço, se aplicável;
8. Opção relacionada a quest futura, marcada deferred;
9. Resposta de fallback/repeat;
10. Goodbye/close.
```

Se o design/direction trouxer falas canônicas, usar essas falas.

Se não trouxer falas canônicas, criar placeholders técnicos não-finais:

```text
TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER
TODO_NARRATIVE_FINALIZATION
```

e registrar que não é narrativa final.

Arquivo obrigatório:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md
```

Template:

```md
## NPC: <NpcId>

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
```

Regra:

```text
Cada NPC MVP precisa ter >= 10 entries.
NPCs future scope não precisam.
```

Se algum NPC MVP tiver menos de 10 entradas:

```text
Status: NEEDS_REWORK_DIALOGUE_COVERAGE
```

---

## 2.5 Movimentação e rotina pela cidade

Cada NPC MVP precisa ter um plano de movimentação.

A spec deve criar ou atualizar:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md
```

Cada NPC deve ter:

```text
MovementProfile:
- Stationary;
- Patrol;
- DailySchedule;
- ShopKeeperFixed;
- WanderWithinZone;
- FutureScheduleOnly.
```

Para NPCs de cidade, quando TownScene existir, definir:

```text
home/spawn;
work location;
shop/service point;
social point;
route waypoints;
return point;
active hours;
idle behavior.
```

Se TownScene ainda não existir ou não estiver jogável:

```text
TEMPORARY_FARM_SCENE_PLACEMENT
TOWNSCENE_MOVEMENT_DEFERRED
```

Mas ainda assim deve haver um plano de rotina documentado.

Template:

```md
| NpcId | MovementProfile | Scene | Waypoints/Anchors | ActiveHours | Purpose | ImplementedNow | DeferredReason |
|---|---|---|---|---|---|---:|---|
```

Se o NPC for merchant, `ShopKeeperFixed` é aceitável no MVP, desde que documentado.

---

## 2.6 Propósitos e responsabilidades

Cada NPC MVP precisa ter propósito e responsabilidade clara.

Criar ou preencher no roster:

```text
Purpose:
- por que esse NPC existe no MVP;
- qual sistema ele ensina/suporta;
- qual função ele exerce no gameplay.

Responsibilities:
- vende sementes;
- compra itens;
- explica ferramentas;
- introduz cave;
- explica skills;
- dá rumor/lore;
- serve como placeholder social;
- aponta para quest futura;
- fornece serviço de upgrade/repair;
- fornece diálogo contextual.
```

Se um NPC não tem propósito claro:

```text
Status: NPC_PURPOSE_UNDEFINED
```

e ele não deve ser considerado MVP pronto.

---

## 2.7 Shop e serviço por NPC

Se um NPC tiver papel comercial, ele deve ter:

```text
ShopId estável;
shop role;
stock inicial;
buy/sell mode;
pricing source;
inventory/economy API;
debt se preço/stock for temporário.
```

Não colocar stock hardcoded na UI.

Arquivo obrigatório para shops de NPC:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md
```

Template:

```md
| NpcId | ShopId | ServiceType | StockSource | Buy? | Sell? | PricingSource | InventoryIntegration | Temporary? | Notes |
|---|---|---|---|---:|---:|---|---|---:|---|
```

---

## 2.8 Authoring e expansão futura

A spec deve garantir que adicionar novo NPC depois seja simples.

O authoring model deve responder:

```text
1. Como adicionar um NPC novo?
2. Onde definir NpcId/displayName/role?
3. Onde posicionar na scene?
4. Onde definir rotas/movimentação?
5. Onde criar 10+ entradas de diálogo?
6. Onde associar DialogueSetId?
7. Onde associar ShopId?
8. Onde definir stock/preço?
9. Onde conectar quest/reputation/schedule no futuro?
10. Como validar que NPC não ficou sem diálogo/movimento/propósito?
```

Se isso não estiver documentado:

```text
NPC_AUTHORING_MODEL_INCOMPLETE
```

---


A spec deve:

1. Confirmar que WAVE_INTEGRATION_11 não quebrou input/focus/HUD/player.
2. Auditar scenes disponíveis: FarmScene, TownScene, CaveScene e qualquer scene alvo social/comercial.
3. Definir onde o primeiro NPC e o primeiro merchant/shop devem aparecer.
4. Auditar NPC runtime existente.
5. Auditar dialogue runtime e UI/projection.
6. Auditar shop/economy/inventory buy/sell runtime.
7. Criar ou reutilizar NPC placement markers.
8. Criar ou reutilizar NPC controller/interactable.
9. Conectar diálogo básico com escolha/resposta/fechamento.
10. Conectar shop mínimo com compra e venda real ou debt explícito.
11. Preservar InputFocusRouter/ModalManager.
12. Criar checklist humano Play Mode.
13. Revalidar contra design/direction no final.

---

## 2.1 Objetivo arquitetural obrigatório

A implementação precisa preparar NPCs, dialogue e shop para expansão.

Não pode virar:

```text
um NPC hardcoded na UI;
um diálogo hardcoded dentro do interactable;
uma loja hardcoded dentro de um botão;
um inventário paralelo para compra/venda;
um sistema social paralelo.
```

O resultado precisa facilitar:

```text
1. adicionar novos NPCs;
2. mover NPCs entre scenes;
3. trocar linhas de diálogo;
4. adicionar opções de diálogo;
5. associar NPC a shop;
6. adicionar estoque por NPC/shop;
7. aplicar preço/economia existente;
8. integrar quests/reputation depois;
9. integrar schedule/calendar depois;
10. salvar flags de conversa/shop se necessário.
```

Se a implementação só funcionar para um NPC hardcoded sem modelo de expansão:

```text
Status: NEEDS_REWORK_NPC_DIALOGUE_SHOP_HARDCODED
```

---

## 2.2 Separação obrigatória de camadas

Manter estas camadas separadas:

```text
NPC Definition Layer
- NPC id;
- display name;
- role;
- scene placement;
- dialogue id;
- shop id, se tiver.

NPC Runtime/Scene Layer
- posição;
- interactable;
- facing/visual;
- prompt;
- enabled/disabled.

Dialogue Definition Layer
- dialogue id;
- lines;
- choices;
- conditions;
- next dialogue ids.

Dialogue Runtime/UI Layer
- abre diálogo;
- exibe texto/opções;
- envia escolha;
- fecha diálogo;
- não decide inventory/economy.

Shop Definition Layer
- shop id;
- stock items;
- buy prices/sell prices ou referência ao pricing service;
- restrictions.

Shop Runtime/UI Layer
- abre shop;
- mostra itens;
- chama inventory/economy;
- não cria wallet/inventory paralelo.
```

Se UI executar regra de shop/dialogue diretamente sem passar por runtime/service quando existir service oficial:

```text
Status: NEEDS_REWORK_LAYERING_VIOLATION
```

---

## 2.3 Princípio data-driven obrigatório

NPCs, dialogue e shop devem ser data-driven ou preparados para ser data-driven.

Permitido:

```text
NpcDefinition
DialogueDefinition
DialogueNode
DialogueChoice
ShopDefinition
ShopStockEntry
ScriptableObject registry existente
JSON/config registry existente
registry centralizado já usado no projeto
scene marker com stableId
```

Proibido como fonte final:

```text
NPC id hardcoded em UI;
linhas de diálogo hardcoded em botão;
shop stock hardcoded em shop panel;
preço hardcoded em UI;
gold/inventory paralelo;
dialogue state salvo por nome de GameObject.
```

Se for necessário usar dados temporários para smoke test:

```text
TEMPORARY_NPC_DIALOGUE_SHOP_TEST_DATA
TODO_INTEGRATION_NOT_FINAL
Blocks final acceptance: YES
```

---

## 3. Design/Direction References

O agente deve consultar estas referências antes de implementar.

## 3.1 Referências primárias de design/direction

Consultar, se existirem:

```text
docs/GDD_v2.6.md
docs/FASE7_SPEC_MVP_FARM_v2.2.md
docs/FASE6_FARM_backlog_v1.2.md
docs/FASE6_INDEX_global_v1.2.md
docs/CINDARS_HOPE_PROJECT_REFINEMENT_SKILL.md
docs/project/CURRENT_STATE.md
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/validation/WAVE_07_PLAYABLE_SCENE_INTEGRATION_ROADMAP_MACRO.md
docs/validation/PLAYABLE_SLICE_INTEGRATION_ROADMAP_MACRO.md
```

## 3.2 Referências específicas de NPC/dialogue/shop/UI

Consultar, se existirem:

```text
docs/validation/03_spec_dialogue_choice_runtime_execution_report.md
docs/validation/03_spec_quest_dialogue_bridge_runtime_execution_report.md
docs/validation/04_spec_ui_dialogue_choice_runtime_execution_report.md
docs/validation/04_spec_ui_shop_buy_sell_runtime_execution_report.md
docs/validation/04_spec_ui_input_focus_modal_routing_runtime_execution_report.md
docs/validation/06_spec_economy_shop_pricing_runtime_execution_report.md
docs/validation/06_spec_shop_buy_sell_runtime_execution_report.md
docs/validation/11_spec_ui_dialogue_projection_runtime_execution_report.md
docs/validation/11_spec_ui_shop_projection_runtime_execution_report.md
docs/validation/WAVE_04_CODE_QUALITY_REVIEW_REPORT.md
docs/validation/WAVE_11_CLOSEOUT_REPORT.md
docs/validation/WAVE_00_12_SPEC_CODE_AUDIT_REPORT.md
```

Também buscar em `.specs/implementados/`, `.specs/a_implementar/` e `docs/validation/` por:

```text
NPC
dialogue
dialog
choice
shop
merchant
buy
sell
pricing
reputation
relationship
town
vendor
```

## 3.3 Referências específicas das waves de integração anteriores

Consultar, se existirem:

```text
docs/validation/WAVE_INTEGRATION_07_SHIPPING_ECONOMY_REPORT.md
docs/validation/WAVE_INTEGRATION_08_HUD_RUNTIME_BINDING_REPORT.md
docs/validation/WAVE_INTEGRATION_09_INVENTORY_TOOLTIP_EQUIPMENT_REPORT.md
docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_REPORT.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md
```

## 3.4 Referências de scene/playable slice

Consultar, se existirem:

```text
docs/validation/WAVE_07_SCENE_INVENTORY.md
docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE.md
docs/validation/WAVE_INTEGRATION_03_PLAYER_CAMERA_MOVEMENT_REPORT.md
docs/validation/WAVE_INTEGRATION_04_FARMSCENE_REBUILD_REPORT.md
docs/validation/WAVE_INTEGRATION_04_05_SPATIAL_RECONCILIATION_REPORT.md
```

## 3.5 Referências de lore/narrativa/canon

Consultar apenas para evitar nomes/conteúdo conflitante; não criar lore final se não houver orientação:

```text
docs/DORNECIA_Guia_Completo.md
docs/Deuses_de_Vaalara.md
docs/Cronologia_de_Vaalara_curada.md
docs/COSMOGONIA_VAALARA.md
docs/RACAS_DE_VAALARA_Guia_Completo.md
```

## 3.6 Regra se referência não existir

Se algum arquivo listado não existir, registrar no report:

```text
Reference not found: <path>
```

Não inventar conteúdo ausente. Usar os arquivos encontrados.

---

## 4. Matriz obrigatória de aderência a design/direction

O agente deve criar e preencher uma matriz explícita no decision report e no execution report:

```md
## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|
```

Regras:

```text
1. Não basta dizer que os arquivos foram lidos.
2. Cada regra relevante precisa virar uma linha da matriz.
3. Se nenhum arquivo de design/direction existir, registrar:
   NO_DESIGN_DIRECTION_SOURCE_FOUND
4. Se arquivos existirem mas não trouxerem regra aplicável a NPC/dialogue/shop, registrar:
   NO_APPLICABLE_NPC_DIALOGUE_SHOP_DIRECTION_FOUND
5. Se uma regra de design conflitar com a spec, parar com:
   BLOCKED_BY_DESIGN_DIRECTION_CONFLICT
```

Exemplos de regras a extrair, se existirem:

```text
- quais NPCs fazem parte do MVP;
- se o primeiro shop deve vender sementes, ferramentas ou itens básicos;
- se shop fica na FarmScene ou TownScene;
- se há NPC específico de cidade/farm;
- como diálogo deve abrir/fechar;
- se diálogo pausa gameplay;
- se shop usa gold real;
- se buy/sell deve usar pricing service;
- se NPC tem schedule;
- se relationship/reputation fica fora do MVP;
- se escolhas de diálogo devem afetar quest/reputation;
- padrões de anti-spoiler/canon;
- padrões de stable IDs;
- padrões de save/load.
```

Esta matriz é obrigatória mesmo que resulte em ausência documentada.

---

## 5. Dependências

## 5.1 Dependência obrigatória

Esta spec depende de:

```text
WAVE_INTEGRATION_11_skill_effects_gameplay_bridge
```

Critérios mínimos:

```text
1. WAVE11 não está BLOCKED.
2. Player, HUD, input/focus continuam estáveis.
3. Target FarmScene ou scene social/comercial alvo está definida.
4. Inventory/economy da WAVE07/09 não está quebrado.
5. Builds runtime/editor passando.
```

Se WAVE11 não existir ou estiver `BLOCKED`:

```text
Status: BLOCKED_BY_MISSING_SKILL_EFFECTS_BASELINE
```

Parar.

Se WAVE11 ainda não foi executada, esta spec pode ser criada/documentada, mas **não deve ser executada**.

## 5.2 Dependências runtime/UI

Auditar antes de criar código:

```text
NpcController / NpcDefinition / NpcId
DialogueService / DialogueRuntime / DialogueDefinition
DialogueChoice / DialogueNode / dialogue UI
ShopService / ShopRuntime / ShopDefinition
ShopBuySell UI/projection
EconomyManager / Pricing service / Gold
InventoryManager / ItemStack / ItemDefinition
InputFocusRouter
ModalManager
Interaction system
HUD feedback
Scene transition / TownScene, se aplicável
SaveManager, se dialogue/shop flags persistirem
```

Regra:

```text
Reutilizar se existir.
Criar bridge pequeno se necessário.
Não criar sistema paralelo.
```

## 5.3 Regra adapter-first obrigatória

Se existir runtime real para NPC/dialogue/shop, os componentes Unity criados nesta spec devem atuar como **adapters/binders/interactables**, não como fontes paralelas.

Exemplos:

```text
NpcSceneInteractable pode:
- receber interação;
- abrir DialogueService;
- abrir ShopService;
- emitir prompt;
- passar NpcId/DialogueId/ShopId.

NpcSceneInteractable não pode:
- armazenar diálogo final em string local como fonte permanente;
- calcular preço sozinho se PricingService existe;
- criar inventory/gold paralelo;
- substituir DialogueService;
- substituir ShopService;
- criar quest/reputation state paralelo.
```

Se runtime real existir mas não for integrável nesta spec:

```text
Status: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
```

ou:

```text
Status: BLOCKED_BY_NPC_DIALOGUE_SHOP_RUNTIME_INTEGRATION_GAP
```

---

## 6. Escopo permitido

Esta spec pode:

```text
1. Criar NPC placement marker, se não existir.
2. Criar NpcSceneInteractable, se não existir.
3. Criar NPC dialogue bridge, se não existir.
4. Criar NPC shop bridge, se não existir.
5. Criar UI bridge para dialogue/shop, se não houver equivalente.
6. Criar dados temporários de NPC/dialogue/shop para smoke test, com TODO.
7. Posicionar 1 dialogue NPC e 1 merchant/shop NPC.
8. Reusar o mesmo NPC para diálogo + shop, se design permitir.
9. Alterar FarmScene ou TownScene alvo, se autorizado.
10. Criar checklist humano.
11. Atualizar CURRENT_STATE.md.
```

---

## 7. Escopo proibido

Esta spec não pode:

```text
1. Criar sistema social completo.
2. Criar relationship/reputation final.
3. Criar schedule/calendar NPC completo.
4. Criar quests finais.
5. Criar narrative/canon final sem design.
6. Criar shop completo com balance final.
7. Criar inventory/economy paralelo.
8. Criar pricing paralelo permanente.
9. Criar item database novo sem necessidade.
10. Criar combat/cave NPCs.
11. Criar cutscenes.
12. Executar WAVE 13.
13. Executar future/mapped.
14. Executar pets.
15. Marcar ACCEPTED.
16. Alterar Packages/ProjectSettings sem autorização.
```

---

## 8. Política sobre scene/prefab/assets

Esta spec pode alterar a scene alvo para posicionar NPCs e UI, se autorizado.

## 8.1 Permitido

```text
FarmScene .unity ou TownScene .unity alvo, apenas uma scene por execução
Scripts C# novos/alterados
Docs de validação
```

## 8.2 Evitar

```text
Prefabs novos
ScriptableObject assets novos
Sprites novos
Fonts novas
Animator/Animation
```

Se precisar de visual, usar:

```text
GameObjects simples
SpriteRenderer com sprite existente
colored placeholder
Canvas simples já existente
debug labels temporários
```

## 8.3 Proibido sem autorização explícita

```text
Packages/**
ProjectSettings/**
Assets/**/*.png
Assets/**/*.aseprite
Assets/**/*.anim
Assets/**/*.controller
Assets/**/*.ttf
Assets/**/*.otf
```

## 8.4 Se não puder alterar a scene automaticamente

Terminar como:

```text
CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
```

e criar instruções humanas detalhadas.

---

## 9. Scene target policy

A spec deve escolher uma única scene alvo.

Ordem preferencial:

```text
1. TownScene, se já existir e estiver jogável com player/camera.
2. FarmScene, se TownScene ainda não está pronta.
3. Cena social/comercial definida pela scene architecture.
```

Se usar FarmScene como temporário:

```text
TEMPORARY_FARM_SCENE_NPC_SHOP_PLACEMENT
TODO_INTEGRATION_NOT_FINAL
```

Se nenhuma scene suportar player/camera:

```text
Status: BLOCKED_BY_MISSING_SOCIAL_SCENE_TARGET
```

---

## 10. Modelo mínimo de NPC/dialogue

O mínimo aceitável:

```text
NPC visível
NpcId estável
Interaction prompt
DialogueId
1 linha de diálogo
1 opção de escolha ou botão continue/close
Close devolve controle ao player
Focus/modal respeitado
```

Preferência:

```text
Dialogue choice runtime existente.
```

Se usar diálogo temporário:

```text
TEMPORARY_DIALOGUE_TEST_DATA
TODO_INTEGRATION_NOT_FINAL
```

---

## 11. Modelo mínimo de shop

O mínimo aceitável:

```text
Shop visível ou NPC merchant visível
ShopId estável
Interaction prompt
Shop UI abre
Mostra pelo menos 1 item comprável ou vendável
Compra usa gold real se disponível
Venda usa inventory real se disponível
Inventory/gold atualiza ou debt explícito
Close devolve controle ao player
```

Se gold/economy/inventory não permite buy/sell real:

```text
Status: BUILD_VALIDATED_WITH_SHOP_ECONOMY_DEBT
```

ou:

```text
Status: BLOCKED_BY_SHOP_INVENTORY_ECONOMY_GAP
```

Não vender/comprar item inexistente.

---

## 12. Stable IDs obrigatórios

NPC/dialogue/shop devem ter IDs estáveis.

Exemplos:

```text
npc_test_villager_01
npc_test_merchant_01
dialogue_test_villager_intro
shop_test_seed_vendor
```

Se forem temporários, ainda assim devem ser estáveis para não quebrar future save.

Proibido:

```text
usar GameObject.name como única fonte de ID;
usar índice de lista como ID persistente;
usar texto de diálogo como ID.
```

---

## 13. Arquivos esperados

## 13.1 Documentação obrigatória

```text
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_DECISION.md
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md
docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md
docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md
docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_AUTHORING_MODEL.md
docs/validation/WAVE_INTEGRATION_12_HUMAN_PLAYMODE_CHECKLIST.md
```

## 13.2 Documentação opcional

```text
docs/validation/WAVE_INTEGRATION_12_HUMAN_UNITY_NPC_DIALOGUE_SHOP_WIRING_INSTRUCTIONS.md
```

## 13.3 Código opcional

Criar somente se não existir equivalente:

```text
Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs
Assets/_Game/Scripts/NPC/Runtime/NpcSceneInteractable.cs
Assets/_Game/Scripts/NPC/Runtime/NpcDialogueBridge.cs
Assets/_Game/Scripts/NPC/Runtime/NpcShopBridge.cs
Assets/_Game/Scripts/UI/Dialogue/Runtime/DialogueRuntimePanelController.cs
Assets/_Game/Scripts/UI/Shop/Runtime/ShopRuntimePanelController.cs
Assets/_Game/Scripts/Editor/Validation/ValidateNpcDialogueShopBridge.cs
```

## 13.4 Scene alvo

Apenas uma scene pode ser alterada:

```text
<path-da-scene-alvo>.unity
```

---

# 14. Execução passo a passo

## STEP 01 — Preflight Git

### Ação

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'

git fetch origin dev
git status --short | Select-Object -First 100
git log --oneline -8
git branch --show-current
```

### Critério

```text
Branch = dev.
Working tree limpa.
```

### Se falhar

Parar:

```text
Status: BLOCKED_BY_GIT_PREFLIGHT
```

---

## STEP 02 — Confirmar WAVE_INTEGRATION_11

### Ação

```powershell
Get-ChildItem .\docs\validation -File |
  Where-Object {
    $_.Name -match 'WAVE_INTEGRATION_11|SKILL_EFFECTS'
  } |
  Sort-Object Name |
  Select-Object Name, FullName
```

Abrir documentos encontrados.

### Verificar

```text
Status
Target FarmScene
Input/focus status
HUD feedback status
Can start WAVE_INTEGRATION_12
```

### Se falhar

Parar:

```text
Status: BLOCKED_BY_MISSING_WAVE_INTEGRATION_11
```

---

## STEP 03 — Build antes de alterações

### Ação

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Assembly-CSharp build FAILED"
    exit 1
}

dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Assembly-CSharp-Editor build FAILED"
    exit 1
}
```

### Critério

Ambos passam.

---

## STEP 04 — Auditar design/direction

### Ação

Buscar arquivos de direction/design:

```powershell
Get-ChildItem .\docs -Recurse -File |
  Where-Object {
    $_.FullName -match 'design|direction|directions|roadmap|validation|project|GDD|FASE|NPC|dialogue|shop|economy'
  } |
  Sort-Object FullName |
  Select-Object FullName
```

Ler arquivos aplicáveis a NPC, dialogue, shop, economy e scene placement.

### Resultado obrigatório

Preencher `Design/Direction Compliance Matrix`.

### Se nenhum arquivo aplicável existir

Registrar:

```text
NO_APPLICABLE_NPC_DIALOGUE_SHOP_DIRECTION_FOUND
```

---

## STEP 04.1 — Extrair roster canônico de NPCs

### Ação

Buscar NPCs nos documentos de design/direction:

```powershell
Get-ChildItem .\docs -Recurse -File |
  Select-String -Pattern "NPC|Npc|personagem|villager|merchant|vendor|shopkeeper|ferreiro|vendedor|cidade|town|dialogue|diálogo|loja|shop"
```

Buscar NPCs no código/assets:

```powershell
Get-ChildItem .\Assets -Recurse -File |
  Where-Object {
    $_.Name -match 'NPC|Npc|Villager|Merchant|Vendor|Shopkeeper|Dialogue|Shop'
  } |
  Sort-Object FullName |
  Select-Object FullName
```

### Criar

```text
docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
```

### Conteúdo obrigatório

```md
# WAVE INTEGRATION 12 — NPC Canonical Roster

## Status
COMPLETE / PARTIAL / BLOCKED_BY_MISSING_CANONICAL_NPC_ROSTER

## Sources scanned

| Source | Found | Notes |
|---|---:|---|

## Canonical NPC roster

| NpcId | DisplayName | Role | Purpose | Responsibilities | PrimaryScene | Placement | MovementProfile | DialogueSetId | DialogueOptions | ShopId | MVP? | Status |
|---|---|---|---|---|---|---|---|---|---:|---|---:|---|

## Future-scope NPCs

| NpcId | Reason | Required before |
|---|---|---|

## Gaps

| Gap | Impact | Decision |
|---|---|---|
```

### Regra

Se não houver roster canônico suficiente, não declarar que todos os NPCs foram criados.

Status permitido:

```text
TEMPORARY_TEST_NPCS_ONLY
```

ou:

```text
BLOCKED_BY_MISSING_CANONICAL_NPC_ROSTER
```

---

## STEP 04.2 — Criar dialogue sets com 10 entradas por NPC MVP

### Criar

```text
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md
```

### Requisitos

Para cada NPC MVP:

```text
mínimo 10 entries/options.
```

Tipos mínimos:

```text
Greeting
Role
Location
GameplayTip
RumorNonSpoiler
Contextual
ShopOrService
QuestFutureHook
RepeatFallback
Goodbye
```

### Template

```md
## NPC: <NpcId>

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
```

### Regra

Se as falas forem placeholder:

```text
TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER
TODO_NARRATIVE_FINALIZATION
```

Se algum NPC MVP tiver menos de 10:

```text
Status: NEEDS_REWORK_DIALOGUE_COVERAGE
```

---

## STEP 04.3 — Criar movement schedules

### Criar

```text
docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md
```

### Template

```md
# WAVE INTEGRATION 12 — NPC Movement Schedules

| NpcId | MovementProfile | Scene | Waypoints/Anchors | ActiveHours | Purpose | ImplementedNow | DeferredReason |
|---|---|---|---|---|---|---:|---|
```

### MovementProfile permitido

```text
Stationary
Patrol
DailySchedule
ShopKeeperFixed
WanderWithinZone
FutureScheduleOnly
```

### Regra

Se TownScene ainda não existir:

```text
TOWNSCENE_MOVEMENT_DEFERRED
```

mas o plano de movimentação ainda deve ser documentado.

---

## STEP 04.4 — Criar shop/service mapping por NPC

### Criar

```text
docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md
```

### Template

```md
# WAVE INTEGRATION 12 — NPC Shop Services

| NpcId | ShopId | ServiceType | StockSource | Buy? | Sell? | PricingSource | InventoryIntegration | Temporary? | Notes |
|---|---|---|---|---:|---:|---|---|---:|---|
```

### Regra

Se merchant não tiver stock/preço/inventory/economy:

```text
SHOP_SERVICE_DEBT
```

Não criar stock hardcoded na UI como fonte final.

---

## STEP 05 — Auditar scenes alvo

### Ação

```powershell
Get-ChildItem .\Assets -Recurse -Filter *.unity |
  Sort-Object FullName |
  Select-Object FullName
```

Verificar:

```text
FarmScene
TownScene
ou scene social/comercial equivalente
```

### Decisão

Escolher:

```text
USE_TOWNSCENE
USE_FARMSCENE_TEMPORARY
BLOCKED_BY_MISSING_SOCIAL_SCENE_TARGET
```

Registrar no decision report.

---

## STEP 06 — Auditar NPC runtime

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "Npc|NPC|Villager|Merchant|Vendor|DialogueId|ShopId|NpcId|Schedule|Relationship|Reputation"
```

### Resultado

Criar tabela:

```md
## NPC runtime audit

| System/File | Found | Role | Decision |
|---|---:|---|---|
```

Identificar:

```text
NpcId
NpcController
NpcDefinition
dialogue linkage
shop linkage
placement/schedule support
```

---

## STEP 07 — Auditar dialogue runtime/UI

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "Dialogue|Dialog|Choice|DialogueNode|DialogueChoice|Conversation|Line|Speaker|DialoguePanel|DialogueView"
```

### Resultado

Criar tabela:

```md
## Dialogue runtime/UI audit

| System/File | Found | Role | Decision |
|---|---:|---|---|
```

Identificar:

```text
DialogueId
DialogueLine
DialogueChoice
open dialogue API
close dialogue API
focus/modal behavior
choice selection behavior
```

---

## STEP 08 — Auditar shop/economy/inventory runtime

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "Shop|Buy|Sell|Merchant|Vendor|Economy|Price|Pricing|Gold|Currency|InventoryManager|ItemStack|ItemDefinition"
```

### Resultado

Criar tabela:

```md
## Shop/economy/inventory audit

| System/File | Found | Role | Decision |
|---|---:|---|---|
```

Identificar:

```text
ShopId
stock item model
buy item API
sell item API
pricing API
gold API
inventory add/remove API
```

Se não houver buy/sell real:

```text
BUILD_VALIDATED_WITH_SHOP_ECONOMY_DEBT
```

ou blocker, dependendo da lacuna.

---

## STEP 09 — Auditar input/focus/modal/interaction

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "IInteractable|Interactable|Interaction|InputFocusRouter|ModalManager|DialogueFocus|ShopFocus|GameplayFocus|Back|Cancel|Confirm"
```

### Decisão

Escolher:

```text
USE_EXISTING_INTERACTION_SYSTEM
USE_INPUT_FOCUS_ROUTER
USE_MODAL_MANAGER
TEMPORARY_INTERACTION_BRIDGE
BLOCKED_BY_INPUT_FOCUS_GAP
```

---

## STEP 10 — Criar decisão de NPC/dialogue/shop bridge

Criar:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_DECISION.md
```

Conteúdo:

```md
# WAVE INTEGRATION 12 — NPC Placement, Dialogue e Shop Bridge Decision

## Status
DRAFT / VALIDATED / BLOCKED

## Scene target strategy

## NPC strategy

## Dialogue strategy

## Shop strategy

## Economy strategy

## Inventory strategy

## Input/focus strategy

## UI technology strategy

## NPC implementation model

DATA_DRIVEN_GENERIC_CONTROLLERS / NEEDS_REWORK_NPC_CLASS_PER_CHARACTER / TEMPORARY_TEST_NPCS_ONLY

## Canonical roster strategy

COMPLETE_CANONICAL_ROSTER / PARTIAL_ROSTER / TEMPORARY_TEST_NPCS_ONLY / BLOCKED_BY_MISSING_CANONICAL_NPC_ROSTER

## Dialogue coverage strategy

10_ENTRIES_PER_MVP_NPC / TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER / NEEDS_REWORK_DIALOGUE_COVERAGE

## Movement/schedule strategy

STATIONARY / PATROL / DAILY_SCHEDULE / SHOPKEEPER_FIXED / TOWNSCENE_MOVEMENT_DEFERRED

## Stable IDs

| Entity | ID | Temporary? | Notes |
|---|---|---:|---|

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|

## Temporary/debt policy

- TODO_INTEGRATION_NOT_FINAL required:
- Temporary NPC data:
- Temporary dialogue data:
- Temporary shop stock:
- Shop economy debt:
- Relationship/reputation deferred:
- Schedule deferred:
- Quest bridge deferred:

## Scene modification policy

## Human Unity actions required
```

---

## STEP 11 — Criar NPC scene marker/interactable, se necessário

Criar apenas se não existir equivalente.

Arquivos possíveis:

```text
Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs
Assets/_Game/Scripts/NPC/Runtime/NpcSceneInteractable.cs
```

Requisitos:

```text
1. NpcId serializado.
2. DialogueId serializado.
3. ShopId opcional serializado.
4. InteractionPrompt.
5. Implementa IInteractable, se existir.
6. Não contém diálogo final hardcoded como fonte permanente.
7. Não contém shop stock final hardcoded como fonte permanente.
8. Não usa GameObject.Find/FindObjectOfType.
```

---

## STEP 12 — Criar NpcDialogueBridge, se necessário

Criar:

```text
Assets/_Game/Scripts/NPC/Runtime/NpcDialogueBridge.cs
```

Requisitos:

```text
1. Recebe NpcId/DialogueId.
2. Chama DialogueService/UI existente se houver.
3. Abre dialogue panel.
4. Aplica DialogueFocus/modal se existir.
5. Fecha e retorna GameplayFocus.
6. Não cria quest/reputation final.
7. Se usar dados temporários, marcar TODO.
```

---

## STEP 13 — Criar NpcShopBridge, se necessário

Criar:

```text
Assets/_Game/Scripts/NPC/Runtime/NpcShopBridge.cs
```

Requisitos:

```text
1. Recebe NpcId/ShopId.
2. Chama ShopService/UI existente se houver.
3. Usa InventoryManager/EconomyManager reais se buy/sell funcionar.
4. Aplica ShopFocus/modal se existir.
5. Fecha e retorna GameplayFocus.
6. Não cria inventory/gold paralelo.
7. Se usar dados temporários, marcar TODO.
```

---

## STEP 14 — Criar/ajustar DialogueRuntimePanelController, se necessário

Criar:

```text
Assets/_Game/Scripts/UI/Dialogue/Runtime/DialogueRuntimePanelController.cs
```

Requisitos:

```text
1. Abre/fecha painel.
2. Mostra speaker/name se disponível.
3. Mostra linha de diálogo.
4. Mostra choices se existirem.
5. Permite close/continue.
6. Não decide regra de quest/reputation.
7. Não hardcodar diálogo final.
```

Se já existir UI, reusar.

---

## STEP 15 — Criar/ajustar ShopRuntimePanelController, se necessário

Criar:

```text
Assets/_Game/Scripts/UI/Shop/Runtime/ShopRuntimePanelController.cs
```

Requisitos:

```text
1. Abre/fecha shop.
2. Mostra buy list.
3. Mostra sell list ou selected item sell.
4. Chama buy/sell APIs reais se existirem.
5. Mostra blocked reason se gold/inventory insuficiente.
6. Não calcula preço se PricingService existe.
7. Não cria gold/inventory paralelo.
```

Se já existir UI, reusar.

---

## STEP 16 — Criar NPC/dialogue/shop authoring model

Criar:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_AUTHORING_MODEL.md
```

Template:

```md
# WAVE INTEGRATION 12 — NPC, Dialogue e Shop Authoring Model

## Status
COMPLETE / PARTIAL / BLOCKED

## Purpose

This document explains how to add, modify and connect NPCs, dialogues and shops after WAVE_INTEGRATION_12.

## NPC definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|

## Dialogue definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|

## Shop definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|

## Stable IDs

| ID type | Format/rule | Used by save? | Notes |
|---|---|---:|---|
| NpcId |  |  |  |
| DialogueId |  |  |  |
| ShopId |  |  |  |

## How to add a new NPC

## How to avoid creating one class per NPC

## How to define NPC purpose and responsibilities

## How to place an NPC in a scene

## How to define NPC movement/schedule

## How to attach dialogue to an NPC

## Minimum 10 dialogue entries/options per MVP NPC

## How to add a dialogue choice

## How to attach shop to an NPC

## How to add a shop item

## How pricing is resolved

## How inventory/gold is updated

## How focus/modal should behave

## How relationship/reputation/quest/schedule should connect later

## Validation checklist for new NPC authoring

- NpcId stable:
- Role:
- Purpose:
- Responsibilities:
- Placement:
- MovementProfile:
- DialogueSet with >=10 entries:
- ShopId/service if applicable:
- Future hooks:

## Known debts

| Debt | Impact | Required before |
|---|---|---|
```

If the system does not yet allow real authoring:

```text
NPC_DIALOGUE_SHOP_AUTHORING_MODEL_INCOMPLETE
```

---

## STEP 17 — Alterar scene com NPCs, se autorizado

### Condição

Só executar se:

```text
1. Scene alvo existe.
2. Scene changes estão autorizadas.
3. O agente consegue editar a scene com segurança.
```

### Ações esperadas

Adicionar ou configurar:

```text
NPC_Dialogue_01
NPC_Merchant_01
NpcScenePlacementMarker
NpcSceneInteractable
NpcDialogueBridge
NpcShopBridge, se merchant
collider/trigger se interação exigir
visual placeholder
```

Se usar FarmScene temporária:

```text
TEMPORARY_FARM_SCENE_NPC_SHOP_PLACEMENT
TODO_INTEGRATION_NOT_FINAL
```

Se não puder alterar scene:

```text
CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
```

e criar instruções humanas.

---

## STEP 18 — Criar instruções humanas, se necessário

Criar:

```text
docs/validation/WAVE_INTEGRATION_12_HUMAN_UNITY_NPC_DIALOGUE_SHOP_WIRING_INSTRUCTIONS.md
```

Conteúdo:

```md
# WAVE INTEGRATION 12 — Human Unity NPC/Dialogue/Shop Wiring Instructions

## Target scene

## Required hierarchy

| Object | Component | References |
|---|---|---|

## Step-by-step

1. Open target scene.
2. Create or locate NPC root.
3. Add NPC_Dialogue_01.
4. Add visual placeholder.
5. Add collider/trigger if interaction requires it.
6. Add NpcSceneInteractable.
7. Assign NpcId and DialogueId.
8. Add/assign NpcDialogueBridge.
9. Add NPC_Merchant_01 or configure same NPC as merchant.
10. Assign ShopId.
11. Add/assign NpcShopBridge.
12. Configure dialogue/shop UI references.
13. Enter Play Mode.
14. Approach dialogue NPC.
15. Interact and validate dialogue.
16. Approach merchant.
17. Interact and validate shop.
18. Buy or sell item.
19. Confirm inventory/gold updates or debt is explicit.
20. Close UI and confirm movement returns.

## Validation checklist
```

---

## STEP 19 — Criar editor validator opcional

Se útil:

```text
Assets/_Game/Scripts/Editor/Validation/ValidateNpcDialogueShopBridge.cs
```

Requisitos:

```text
1. Editor-only.
2. Não usar IProjectValidator/ValidationReport.
3. Validar scripts existem.
4. Validar NPC IDs não vazios.
5. Validar DialogueId/ShopId não vazios quando exigidos.
6. Validar collider/interactable.
7. Não abrir scene automaticamente se instável.
8. Não ser critério único de sucesso.
```

---

## STEP 20 — Atualizar CURRENT_STATE

Atualizar:

```text
docs/project/CURRENT_STATE.md
```

Com:

```md
## WAVE_INTEGRATION — Scene Wiring e Playable Runtime Bridge

### WAVE_INTEGRATION_12 — NPC Placement, Dialogue e Shop Bridge

Status: BUILD_VALIDATED_SCENE_WIRED / BUILD_VALIDATED_WITH_SHOP_ECONOMY_DEBT / CODE_READY_HUMAN_UNITY_ACTION_REQUIRED / BLOCKED

Validation:
- Assembly-CSharp:
- Assembly-CSharp-Editor:
- Docs validation:
- Target scene:
- NPCs placed:
- Dialogue:
- Shop:
- Inventory/economy:
- Input/focus:
- Authoring model:
- Can start next wave:
```

---

## STEP 21 — Build depois

Rodar:

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Assembly-CSharp build FAILED"
    exit 1
}

dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Assembly-CSharp-Editor build FAILED"
    exit 1
}

.\tools\docs\validate_docs.ps1
$docsCode = $LASTEXITCODE
if ($docsCode -ne 0) {
    Write-Host "Docs validation returned non-zero. Classify as EXPECTED_FAIL_LEGACY_ONLY only if no new errors were introduced."
}
```

---

## STEP 22 — Criar execution report

Criar:

```text
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md
```

Template:

```md
# WAVE INTEGRATION 12 — NPC Placement, Dialogue e Shop Bridge — Execution Report

## Status
BUILD_VALIDATED_SCENE_WIRED / BUILD_VALIDATED_WITH_SHOP_ECONOMY_DEBT / CODE_READY_HUMAN_UNITY_ACTION_REQUIRED / BLOCKED

## Summary

## Source documents read

| Document | Found | Notes |
|---|---|---|

## Design/Direction references checked

| Reference | Found | Used? | Notes |
|---|---:|---:|---|

## Preflight

| Check | Result |
|---|---|

## Baseline gates from WAVE 11

| Gate | Result | Evidence |
|---|---|---|

## Build validation

| Target | Before | After | Result |
|---|---|---|---|

## Target scene

| Field | Value |
|---|---|

## Scene target decision

| Strategy | Result | Reason |
|---|---|---|

## NPC runtime audit

| System/File | Found | Evidence | Decision |
|---|---:|---|---|

## Dialogue runtime/UI audit

| System/File | Found | Evidence | Decision |
|---|---:|---|---|

## Shop/economy/inventory audit

| System/File | Found | Evidence | Decision |
|---|---:|---|---|

## Input/focus/modal audit

| System/File | Found | Evidence | Decision |
|---|---:|---|---|

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|

## Integration strategy

| Area | Strategy | Notes |
|---|---|---|
| NPC |  |  |
| Dialogue |  |  |
| Shop |  |  |
| Economy |  |  |
| Inventory |  |  |
| Input/focus |  |  |
| UI |  |  |
| Scene placement |  |  |

## Stable IDs

| Entity | ID | Temporary? | Notes |
|---|---|---:|---|

## Canonical NPC roster

Link:
- `docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md`

| Metric | Value |
|---|---|
| MVP NPCs found |  |
| MVP NPCs placed |  |
| Future-scope NPCs |  |
| NPCs missing purpose |  |
| NPCs missing movement |  |
| NPCs missing dialogue coverage |  |

## Dialogue set coverage

Link:
- `docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md`

| NpcId | Entries/options | >=10? | Temporary? | Notes |
|---|---:|---:|---:|---|

## Movement schedule coverage

Link:
- `docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md`

| NpcId | MovementProfile | ImplementedNow | DeferredReason |
|---|---|---:|---|

## Shop/service coverage

Link:
- `docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md`

| NpcId | ShopId | ServiceType | Buy | Sell | Debt |
|---|---|---|---:|---:|---|

## Temporary/debt declaration

| Item | Value |
|---|---|
| TODO_INTEGRATION_NOT_FINAL required |  |
| Temporary NPC data |  |
| Temporary dialogue data |  |
| Temporary shop stock |  |
| Shop economy debt |  |
| Relationship/reputation deferred |  |
| Schedule deferred |  |
| Quest bridge deferred |  |
| Risk |  |

## NPC/dialogue/shop authoring model

| Item | Result | Evidence |
|---|---|---|
| Authoring model created |  |  |
| NPC authoring documented |  |  |
| Dialogue authoring documented |  |  |
| Shop authoring documented |  |  |
| Pricing/inventory documented |  |  |
| Future relationship/quest/schedule bridge documented |  |  |

## Code created

| File | Reason |
|---|---|

## Scene changes

| File/Object | Change | Reason |
|---|---|---|

## Human Unity actions required

| Action | Required | Reason |
|---|---:|---|

## Acceptance criteria matrix

| AC | Result | Evidence |
|---|---|---|

## Final Design/Direction Revalidation

| Check | Result | Evidence |
|---|---|---|
| All listed design/direction references checked |  |  |
| Applicable NPC/dialogue/shop rules extracted |  |  |
| NPC placement rules applied |  |  |
| Dialogue rules applied |  |  |
| Shop/economy rules applied or deferred |  |  |
| Input/focus/modal rules applied |  |  |
| Stable ID rules applied |  |  |
| No design conflict remains |  |  |

## Decision

- Can start next wave:
- Blocking issues:
- Human Play Mode validation required:
```

---

## STEP 23 — Criar checklist humano

Criar:

```text
docs/validation/WAVE_INTEGRATION_12_HUMAN_PLAYMODE_CHECKLIST.md
```

Conteúdo:

```md
# WAVE INTEGRATION 12 — Human Play Mode Checklist

## Status
PENDING / PASS / FAIL

## Preconditions

- Unity opens without red console errors.
- WAVE_INTEGRATION_11 did not break input/HUD/player.
- Target scene contains NPC wiring or human wiring was applied.
- Inventory/economy is available if testing shop buy/sell.

## Checklist

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open target scene | Scene opens |  |  |
| Press Play | Game starts |  |  |
| Player movement | Player moves |  |  |
| Dialogue NPC visible | NPC appears |  |  |
| Approach dialogue NPC | Interaction prompt appears |  |  |
| Interact dialogue NPC | Dialogue panel opens |  |  |
| Dialogue content | Speaker/line/choice appears |  |  |
| Close dialogue | Panel closes |  |  |
| Movement restored | Player moves again |  |  |
| Merchant/shop NPC visible | Merchant appears |  |  |
| Approach merchant | Interaction prompt appears |  |  |
| Open shop | Shop panel opens |  |  |
| Buy item | Gold decreases/item added or blocked reason appears |  |  |
| Sell item | Item removed/gold increases or debt is explicit |  |  |
| Close shop | Shop closes |  |  |
| Focus/modal | No stuck UI state |  |  |
| Stop Play | Scene is not corrupted |  |  |

## Result

## Bugs found

## Can start next wave
YES/NO
```

---

# 15. Acceptance Criteria

| ID | Critério | Obrigatório |
|---|---|---|
| AC-01 | WAVE_INTEGRATION_11 existe e não está BLOCKED | Sim |
| AC-02 | Build runtime antes/depois passa | Sim |
| AC-03 | Build editor antes/depois passa | Sim |
| AC-04 | Design/Direction Compliance Matrix foi preenchida ou ausência foi registrada | Sim |
| AC-05 | Scene alvo foi definida | Sim |
| AC-06 | NPC runtime foi auditado | Sim |
| AC-07 | Dialogue runtime/UI foi auditado | Sim |
| AC-08 | Shop/economy/inventory runtime foi auditado | Sim |
| AC-09 | Input/focus/modal foi auditado | Sim |
| AC-10 | NPC visível ou human wiring claro existe | Sim |
| AC-11 | Dialogue abre/fecha ou human wiring claro existe | Sim |
| AC-12 | Shop abre/fecha ou debt/blocker explícito existe | Sim |
| AC-13 | Buy/sell usa inventory/economy real ou debt explícito | Sim |
| AC-14 | Stable IDs foram definidos | Sim |
| AC-15 | Authoring model foi criado | Sim |
| AC-16 | Scene changes foram documentadas, se ocorreram | Sim |
| AC-17 | Checklist humano foi criado | Sim |
| AC-18 | Final Design/Direction Revalidation existe no report | Sim |
| AC-19 | Roster canônico de NPCs foi extraído ou gap/blocker declarado | Sim |
| AC-20 | NPCs MVP têm propósito e responsabilidades | Sim |
| AC-21 | NPCs MVP têm plano de movimentação/schedule | Sim |
| AC-22 | NPCs MVP têm pelo menos 10 entradas/opções de diálogo ou debt explícito | Sim |
| AC-23 | NPCs comerciais têm ShopId/service mapping ou debt explícito | Sim |
| AC-24 | Implementação usa controllers genéricos/data-driven, não uma classe por NPC | Sim |

---

# 16. Definition of Done

A spec termina como `BUILD_VALIDATED_SCENE_WIRED` se:

```text
1. Pelo menos um NPC está visível na scene alvo.
2. Player consegue interagir.
3. Dialogue abre/fecha.
4. Focus/modal funciona.
5. Shop abre/fecha.
6. Buy ou sell mínimo funciona com inventory/economy real.
7. Stable IDs existem.
8. Authoring model foi criado.
9. Builds passam.
10. Checklist humano pode ser executado.
11. Final Design/Direction Revalidation foi preenchida.
12. Roster canônico foi criado.
13. NPCs MVP têm purpose/responsibilities.
14. NPCs MVP têm movement/schedule documentado.
15. NPCs MVP têm >=10 dialogue entries ou debt explícito.
16. Merchant/shop NPCs têm service mapping.
17. Implementação não cria uma classe por NPC como padrão.
```

A spec termina como `BUILD_VALIDATED_WITH_SHOP_ECONOMY_DEBT` se:

```text
1. NPC e dialogue funcionam.
2. Shop abre.
3. Buy/sell ainda depende de debt de economy/inventory/pricing.
4. Debt foi marcado como não-final.
5. Builds passam.
```

A spec termina como `CODE_READY_HUMAN_UNITY_ACTION_REQUIRED` se:

```text
1. Código está pronto.
2. Builds passam.
3. Instruções humanas de wiring existem.
4. Scene/UI precisa ser ajustada manualmente no Unity.
```

A spec termina como `BLOCKED` se:

```text
1. WAVE11 está ausente ou bloqueada.
2. Nenhuma scene alvo existe.
3. Dialogue runtime não pode abrir painel.
4. Shop/economy/inventory gap impede fluxo mínimo e não há debt aceitável.
5. Builds falham.
6. Design/direction conflita.
7. Alteração proibida seria necessária.
```

---

# 17. Commit

Se tudo passar:

```powershell
git add `
  docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_DECISION.md `
  docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md `
  docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md `
  docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md `
  docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md `
  docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md `
  docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_AUTHORING_MODEL.md `
  docs/validation/WAVE_INTEGRATION_12_HUMAN_PLAYMODE_CHECKLIST.md `
  docs/project/CURRENT_STATE.md
```

Adicionar código se criado:

```powershell
git add Assets/_Game/Scripts/NPC/Runtime
git add Assets/_Game/Scripts/UI/Dialogue/Runtime
git add Assets/_Game/Scripts/UI/Shop/Runtime
git add Assets/_Game/Scripts/Editor/Validation/ValidateNpcDialogueShopBridge.cs
```

Adicionar instruções humanas se criadas:

```powershell
git add docs/validation/WAVE_INTEGRATION_12_HUMAN_UNITY_NPC_DIALOGUE_SHOP_WIRING_INSTRUCTIONS.md
```

Adicionar scene somente se alterada explicitamente:

```powershell
git add <path-da-scene-alvo>.unity
```

Nunca usar:

```powershell
git add Assets
```

Commit:

```powershell
git commit -m "feat: execute wave integration 12 npc dialogue shop bridge"
git push origin dev
git fetch origin dev
git log --oneline -5 origin/dev
git status --short
```

---

# 18. Resposta final obrigatória

Responder:

```text
Status:
Branch:
Working tree preflight:
WAVE_INTEGRATION_11 baseline:
Target scene:
Design/direction references checked:
Design/direction compliance:
Scene target strategy:
NPC strategy:
NPC implementation model:
Canonical roster:
NPC purposes/responsibilities:
NPC dialogue coverage:
NPC movement/schedules:
NPC shop/services:
Dialogue strategy:
Shop strategy:
Economy strategy:
Inventory strategy:
Input/focus strategy:
UI technology:
Stable IDs:
Temporary/debt mode:
Authoring model:
Scene changes:
Code created:
Human wiring instructions:
Assembly-CSharp before:
Assembly-CSharp-Editor before:
Assembly-CSharp after:
Assembly-CSharp-Editor after:
Docs validation:
Execution report:
Final design/direction revalidation:
Human checklist:
Commit:
Pushed:
Remote HEAD:
Can start next wave:
Human Play Mode validation needed:
```

---

# 19. Critérios de bloqueio

Parar imediatamente se:

```text
1. Branch não for dev.
2. Working tree estiver suja.
3. WAVE_INTEGRATION_11 não existir ou estiver BLOCKED.
4. Nenhuma scene alvo existir.
5. Build runtime falhar.
6. Build editor falhar.
7. Já existir NPC/dialogue/shop bridge e o agente tentar criar outro paralelo.
8. Dialogue runtime não puder ser aberto.
9. Shop/economy/inventory gap impedir fluxo e não houver debt aceitável.
10. Alteração de scene for necessária sem autorização.
11. A spec tentar implementar relationship/reputation/schedule/quest final fora do escopo.
12. Design/direction conflict for encontrado.
```

---

## 20. Revalidação final contra design/direction

Antes de concluir, o agente deve executar uma última revisão e registrar no execution report:

```md
## Final Design/Direction Revalidation

| Check | Result | Evidence |
|---|---|---|
| All listed design/direction references checked |  |  |
| Applicable NPC/dialogue/shop rules extracted |  |  |
| NPC placement rules applied |  |  |
| Dialogue rules applied |  |  |
| Shop/economy rules applied or deferred |  |  |
| Inventory rules applied or deferred |  |  |
| Input/focus/modal rules applied |  |  |
| Stable ID rules applied |  |  |
| No design conflict remains |  |  |
```

Se esta seção não existir no execution report, a spec deve ser considerada incompleta.

---

## 21. Observação final

Esta spec não cria sistema social final.

Ela cria o primeiro fluxo habitado/comercial:

```text
NPC visível → diálogo → shop → buy/sell mínimo → inventory/economy/feedback
```

Se o usuário abrir Unity depois desta spec e não conseguir interagir com NPC, abrir diálogo e testar shop/debt documentado, a spec não cumpriu o objetivo.

A próxima etapa após essa spec deve ser definida pelo arquivo mestre atualizado: transições de cena, cave entrance, town/cave playable path, save/load hardening ou final playable slice polish, conforme status real do repo.
