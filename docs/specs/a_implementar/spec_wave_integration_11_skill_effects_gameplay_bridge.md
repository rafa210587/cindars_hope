# spec_wave_integration_11_skill_effects_gameplay_bridge.md

required_adrs: []
required_game_rules: []

Ordem de execucao: WAVE_INTEGRATION_11
Depende de: WAVE_INTEGRATION_10_skill_tree_ui_active_skill_equip
Bloqueia: WAVE12 ate BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT

# /speckit.specify
# /speckit.plan
# /speckit.tasks

> **Projeto:** Cindar's Hope  
> **Wave:** WAVE_INTEGRATION — Scene Wiring e Playable Runtime Bridge  
> **Spec:** 11 — Skill Effects Gameplay Bridge  
> **Tipo:** Integração Unity / Skill Effects / Active Skill Execution / Gameplay Bridge / Extensible Effect System  
> **Status inicial:** A implementar  
> **Prioridade:** P0 — transforma active skills equipadas em efeitos jogáveis reais  
> **Referência do arquivo mestre:** `WAVE_07_PLAYABLE_SCENE_INTEGRATION_ROADMAP_MACRO.md` → `SPEC 11 — Skill Effects Gameplay Bridge`  
> **Dependência direta:** `WAVE_INTEGRATION_10_skill_tree_ui_active_skill_equip`  
> **Resultado esperado:** pelo menos uma skill ativa equipada pode ser acionada em Play Mode, executar um efeito real ou bridge controlado sobre um alvo válido, consumir custo/cooldown se existir runtime, produzir feedback visual/HUD e preservar uma arquitetura extensível para novos efeitos.

---

## 0. Intenção desta spec

Esta spec existe para ligar o que a WAVE_INTEGRATION_10 tornou visível:

```text
skill tree → node comprado → active skill equipada em slot
```

com o gameplay real:

```text
active slot input → valida skill equipada → valida custo/cooldown/alvo → executa efeito → aplica resultado no mundo → feedback/HUD
```

Ela deve criar o primeiro bridge real entre **skills equipadas** e **sistemas de gameplay**.

Esta spec **não** deve criar uma árvore de skills nova, nem refazer o sistema de active slots, nem implementar todos os efeitos finais do jogo. Ela deve criar uma arquitetura extensível e validar pelo menos um efeito vertical slice.

---

## 1. Nome e localização

Arquivo desta spec:

```text
docs/specs/a_implementar/spec_wave_integration_11_skill_effects_gameplay_bridge.md
```

Padrão oficial:

```text
WAVE_INTEGRATION_{NUMBER_SPEC}_{slug}.md
```

---

## 2. Objetivo

Conectar active skills equipadas a um pipeline de execução de efeitos.

A spec deve:

1. Confirmar que a WAVE_INTEGRATION_10 permite skill ativa equipada ou tem active slot debt explicitamente resolvido.
2. Auditar runtime de skills, active slots, cooldowns, costs, player condition, targeting e gameplay systems.
3. Auditar a existência de `EffectId`, `SkillEffectDefinition`, executor/registry ou equivalente.
4. Criar ou consolidar um contrato mínimo para execução de efeitos, se não existir.
5. Criar um bridge `ActiveSkillExecutionController` ou equivalente.
6. Mapear input de active slot para execução de skill.
7. Validar alvo/contexto.
8. Executar pelo menos um efeito real ou bridge controlado.
9. Aplicar feedback visual/HUD.
10. Não criar efeito hardcoded dentro da UI.
11. Criar authoring model para adicionar novos efeitos depois.
12. Criar checklist humano Play Mode.
13. Revalidar contra design/direction no final.

---

## 2.1 Objetivo arquitetural obrigatório

Esta spec deve preparar um sistema de efeitos expansível.

A implementação precisa permitir que futuras skills sejam adicionadas sem refazer o executor principal.

O sistema deve facilitar:

```text
1. adicionar novos tipos de efeito;
2. mapear SkillId/EffectId para executor;
3. alterar custo/cooldown;
4. alterar alvo permitido;
5. adicionar efeitos passivos depois;
6. adicionar efeitos ativos de farm/combat/magic/survival/utility;
7. plugar efeitos em crop, resources, player condition, combat e cave;
8. salvar/carregar estado de cooldown/equipamento se necessário;
9. testar efeitos isoladamente;
10. impedir execução se skill não estiver equipada/comprada/desbloqueada.
```

Se a implementação só funcionar para um botão hardcoded e um efeito fixo sem arquitetura de expansão:

```text
Status: NEEDS_REWORK_SKILL_EFFECTS_HARDCODED
```

---

## 2.2 Princípio data-driven obrigatório

A relação entre skill e efeito deve ser data-driven ou preparada para ser data-driven.

Permitido:

```text
EffectId em SkillDefinition/SkillNodeDefinition
SkillEffectDefinition
SkillEffectRegistry
SkillEffectExecutor registry
EffectCategory
Targeting rule
Cost/cooldown config
ScriptableObject/config existente
Registry centralizado existente
```

Proibido:

```text
if skillId == "skill_x" dentro da UI;
efeito implementado diretamente no botão;
efeito implementado diretamente no HUD slot;
custo/cooldown hardcoded dentro do input controller;
target hardcoded sem passar por contexto;
skill ativa executando sem verificar se está equipada/desbloqueada;
criar um sistema paralelo de skill points ou active slots.
```

Se for necessário usar dados temporários para smoke test:

```text
TEMPORARY_SKILL_EFFECT_TEST_DATA
TODO_INTEGRATION_NOT_FINAL
Blocks final acceptance: YES
```

---

## 2.3 Separação obrigatória de camadas

A implementação precisa manter estas camadas separadas:

```text
Definition Layer
- SkillDefinition / SkillNodeDefinition / EffectId / cost / cooldown / target rules.

Runtime State Layer
- purchased nodes;
- equipped active slots;
- cooldowns;
- resource/cost state;
- current selected target/context.

Service Layer
- valida skill equipada;
- valida unlock;
- valida custo/cooldown;
- resolve executor por EffectId;
- executa efeito;
- publica eventos/feedback.

Effect Layer
- executores concretos;
- aplicam mudanças em crop/resource/player/world;
- não sabem sobre UI.

UI/HUD/Input Layer
- recebe input;
- mostra cooldown/custo/erro/feedback;
- não decide regra final;
- não aplica efeito diretamente.
```

Se a UI aplicar efeito direto sem passar por service/executor:

```text
Status: NEEDS_REWORK_SKILL_EFFECT_LAYERING_VIOLATION
```

---

## 2.4 Contratos mínimos esperados

Antes de criar o bridge, auditar ou criar contratos equivalentes a:

```text
SkillId: string estável.
EffectId: string estável.
ActiveSlotIndex: 0..3.
SkillEffectCategory: Farm / Combat / Magic / Survival / Utility / Debug / Unknown.
SkillEffectTargetType: Self / WorldPoint / CropPlot / ResourceNode / Enemy / Area / None.
SkillEffectContext: caster, slot, target, world position, scene, time.
SkillEffectResult: success, failure reason, cost spent, cooldown started, feedback message.
ISkillEffectExecutor ou equivalente.
SkillEffectRegistry ou resolver equivalente.
Cooldown/cost model, se existir.
```

Não é obrigatório usar exatamente esses nomes, mas o sistema precisa ter equivalentes claros.

Se não houver equivalentes e a spec criar apenas efeito hardcoded:

```text
Status: BLOCKED_BY_MISSING_SKILL_EFFECT_CONTRACT
```

---

## 2.5 Efeito vertical slice obrigatório

Esta spec deve validar pelo menos um efeito ativo.

Ordem preferencial:

```text
1. Usar uma active skill real já definida na WAVE_INTEGRATION_10.
2. Usar EffectId real se existir.
3. Usar uma skill de farm/utility mínima se o design permitir.
4. Usar efeito temporário de debug apenas se marcado como não-final.
```

Tipos de efeito aceitáveis para primeiro slice:

```text
Farm/Crop:
- regar crop plot alvo;
- acelerar um estágio de crop plot;
- preparar solo em plot alvo;
- colher/forçar ready em plot de teste, se marcado debug.

Resource:
- coletar resource node alvo;
- revelar resource/forage próximo;
- resetar resource de teste, se marcado debug.

Player/Utility:
- restaurar pequena stamina, se player condition runtime existir;
- dash curto, se movement runtime suportar;
- ping/feedback visual em área, se não houver target runtime.

Debug fallback:
- efeito visual/feedback sem gameplay final, marcado TODO_INTEGRATION_NOT_FINAL.
```

Preferência para o primeiro slice:

```text
Farm/Crop effect, porque as WAVE_INTEGRATION_05 e 06 já criaram crop/resources na FarmScene.
```

---


## 2.6 Escopo obrigatório de catálogo de efeitos por skill tree

A versão anterior desta spec definia o **pipeline de execução** e um vertical slice, mas não obrigava descrever o efeito de cada skill de cada árvore.

Esta revisão corrige isso.

Antes de criar código de execução, o agente deve criar um catálogo completo dos efeitos previstos para as skill trees disponíveis no design/refinement.

Criar obrigatoriamente:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
```

Esse catálogo deve conter uma linha por skill/node com efeito de gameplay, mesmo que o efeito seja futuro/deferred.

Template obrigatório:

```md
# WAVE INTEGRATION 11 — Skill Effect Catalog

## Status
COMPLETE / PARTIAL / BLOCKED

## Source of truth

| Source | Found | Used | Notes |
|---|---:|---:|---|

## Skill trees discovered

| Skill tree | Source | Included in WAVE11? | Notes |
|---|---|---:|---|

## Effect catalog

| Tree | SkillId | NodeId | Skill name | Type | Active slot? | EffectId | Target type | Input | Cost | Cooldown | Runtime effect | Visual/HUD feedback | Implement now? | Deferred reason |
|---|---|---|---|---|---:|---|---|---|---|---|---|---|---:|---|

## Passive effects

| Tree | SkillId | EffectId | Trigger | Rule | Runtime target | Implement now? | Deferred reason |
|---|---|---|---|---|---|---:|---|

## Active effects

| Tree | SkillId | EffectId | Slot eligible | Input | Target | Execution rule | Implement now? | Deferred reason |
|---|---|---|---:|---|---|---|---:|---|

## Movement/non-slot abilities

| Ability | Occupies active slot? | Input source | Movement rule | Distance source | Collision rule | Cooldown/cost | Implement now? |
|---|---:|---|---|---|---|---|---:|

## Coverage summary

| Category | Total found | Implement now | Deferred | Blocked |
|---|---:|---:|---:|---:|
```

Regras:

```text
1. Não implementar effects antes do catálogo existir.
2. Não inventar árvore/skill que não existe no design/refinement.
3. Se o design cita skill tree sem listar nodes, registrar PARTIAL e bloquear implementação final daquela árvore.
4. Se uma skill não tem efeito descrito, registrar EFFECT_UNSPECIFIED e não implementar como final.
5. Se uma skill é passiva, ela não entra em active slot.
6. Se uma skill é ativa, ela só equipa em active slot se CanEquipToActiveSlot=true.
7. Dash, Dodge e Block não ocupam active slot.
8. Dash/Dodge/Block devem aparecer no catálogo como movement/non-slot abilities.
```

Se o catálogo não cobrir todos os nodes encontrados:

```text
Status: BLOCKED_BY_INCOMPLETE_SKILL_EFFECT_CATALOG
```

Se o catálogo cobrir todos os nodes, mas a spec implementar só 2 efeitos nesta wave:

```text
Status permitido: BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
```

com lista clara de deferred effects.

---

## 2.7 Regras obrigatórias para Dash, Dodge e Block

Dash, Dodge e Block são habilidades de movimento/defesa próprias do player. Elas **não ocupam active skill slot**.

Esta spec deve ajustar e/ou criar contratos para que elas funcionem de forma consistente com os botões e distâncias definidos no refinement.

### Fonte da verdade

O agente deve localizar no repo/refinements os valores exatos de:

```text
Dash input/button
Dodge input/button ou double-tap rule
Block input/button
Dash distance in tiles/squares
Dodge distance in tiles/squares
Dash cooldown/cost
Dodge cooldown/cost
Dodge invulnerability/iframe rule, se existir
Collision/blocking rule
```

Buscar obrigatoriamente:

```powershell
Get-ChildItem .\docs -Recurse -File |
  Select-String -Pattern "Dash|Dodge|Block|double tap|double-tap|quadrado|tile|squares|movement skill|esquiva|bloqueio"

Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "Dash|Dodge|Block|DoubleTap|MovementAbility|PlayerMovement|Input"
```

Se a distância exata `X` não estiver definida no repo/refinement:

```text
Status: BLOCKED_BY_MISSING_DASH_DODGE_REFINEMENT_DISTANCE
```

Não inventar distância.

### Dash — regra obrigatória

Dash deve ser implementado como movimento rápido **para frente**, na direção atual de facing/movement do player.

Regra:

```text
Input indicado no refinement
→ valida se player pode agir
→ calcula direção frontal/facing
→ move X quadrados/tiles para frente
→ respeita colisão/bounds
→ se caminho bloqueado, para no último ponto válido
→ aplica cooldown/cost se existir
→ publica feedback/HUD se existir
```

Critérios:

```text
1. Dash não usa active skill slot.
2. Dash não é skill equipada.
3. Dash não atravessa colisores sólidos.
4. Dash não sai dos bounds da FarmScene/cave scene.
5. Dash tem distância em grid/tiles conforme refinement, não valor aleatório.
6. Dash deve usar duração curta/interpolação ou snap controlado, conforme movimento atual do jogo permitir.
7. Dash deve ser testável com input real no Play Mode.
```

### Dodge — regra obrigatória

Dodge deve ser implementado por **double tap direcional**, conforme refinement.

Regra:

```text
Double tap direção
→ determina direção do dodge
→ se double tap para trás, dodge vai X quadrados para trás em relação ao facing atual
→ se double tap lateral, dodge vai X quadrados para o lado indicado
→ se double tap para frente e o design permitir, tratar como forward dodge; se não permitir, bloquear/ignorar
→ respeita colisão/bounds
→ se caminho bloqueado, para no último ponto válido
→ aplica cooldown/cost se existir
→ publica feedback/HUD se existir
```

Critérios:

```text
1. Dodge não usa active skill slot.
2. Dodge é acionado por double tap, não por slot de skill.
3. Dodge para trás/lados deve respeitar facing/direction rules.
4. Dodge não atravessa colisores sólidos.
5. Dodge não sai dos bounds da FarmScene/cave scene.
6. Dodge usa distância em grid/tiles conforme refinement.
7. Double tap window vem do refinement ou config; se ausente, bloquear ou documentar debt, não inventar final.
8. Se houver i-frames/invulnerabilidade no refinement, criar contrato; se não houver, não inventar.
```

### Block — regra obrigatória

Block deve ser auditado nesta spec, mas só precisa ser implementado se o refinement atual exigir.

Regra mínima:

```text
Block não ocupa active slot.
Block input/button vem do refinement.
Block não deve ser implementado como active skill equipada.
Se não houver combate/cave target ainda, registrar BLOCK_RUNTIME_DEFERRED_WITH_REASON.
```

### Arquivos esperados para movimento

Criar/reutilizar apenas se não houver equivalente:

```text
Assets/_Game/Scripts/Player/Movement/PlayerMovementAbilityController.cs
Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs
Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs
Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs
Assets/_Game/Scripts/Player/Movement/GridMovementDisplacementResolver.cs
Assets/_Game/Scripts/Editor/Validation/ValidateDashDodgeMovement.cs
```

Regras de arquitetura:

```text
1. Input detecta intenção.
2. Movement ability controller valida estado/cooldown/cost.
3. Displacement resolver calcula destino respeitando grid/colliders/bounds.
4. PlayerMovement aplica movimento.
5. UI/HUD só mostra feedback.
```

Proibido:

```text
1. Implementar dash/dodge dentro da SkillTree UI.
2. Implementar dash/dodge dentro de HUD active slot.
3. Fazer dash/dodge ocuparem active slot.
4. Ignorar colisão.
5. Usar distância hardcoded se refinement definiu outra.
6. Inventar X se refinement não foi encontrado.
```

---

## 2.8 Escopo mínimo revisado da WAVE_INTEGRATION_11

Esta spec deve entregar três camadas, não apenas uma:

```text
A. Catálogo completo de efeitos por árvore/skill.
B. Pipeline extensível de execução de skill effects.
C. Implementação/ajuste real de Dash e Dodge conforme refinement.
```

Execução mínima para passar:

```text
1. Skill Effect Catalog criado.
2. Dash/Dodge refinement localizado e aplicado ou blocker declarado.
3. Pelo menos 2 efeitos reais implementados:
   - 1 efeito de farm/resource/utility via skill active ou passive bridge;
   - Dash ou Dodge funcional como movement ability não-slot.
4. Active skill execution pipeline preservado para expansão.
5. UI não executa efeito diretamente.
```

Se Dash/Dodge não forem implementados porque falta refinement de distância/input:

```text
Status: BLOCKED_BY_MISSING_DASH_DODGE_REFINEMENT
```

Se Dash/Dodge forem implementados mas active skill effect ficar só como fallback:

```text
Status: BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
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
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/validation/WAVE_07_PLAYABLE_SCENE_INTEGRATION_ROADMAP_MACRO.md
docs/validation/PLAYABLE_SLICE_INTEGRATION_ROADMAP_MACRO.md
```

## 3.2 Referências específicas de skill tree/active slots/effects

Consultar, se existirem:

```text
docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_DECISION.md
docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_REPORT.md
docs/validation/WAVE_INTEGRATION_10_SKILL_AUTHORING_MODEL.md
docs/validation/WAVE_INTEGRATION_10_HUMAN_PLAYMODE_CHECKLIST.md
docs/validation/04_spec_ui_skill_tree_active_slots_runtime_execution_report.md
docs/validation/11_spec_ui_skill_tree_projection_runtime_execution_report.md
docs/validation/WAVE_10_CLOSEOUT_REPORT.md
docs/validation/WAVE_11_CLOSEOUT_REPORT.md
docs/validation/WAVE_00_12_SPEC_CODE_AUDIT_REPORT.md
```

Também buscar em `docs/specs/implementados/`, `docs/specs/a_implementar/` e `docs/validation/` por:

```text
skill effect
effect id
active skill
cooldown
cost
mana
stamina
targeting
skill action
ability
spell
farm skill
combat skill
magic skill
```

## 3.3 Referências de gameplay alvo

Consultar, se existirem:

```text
docs/validation/WAVE_INTEGRATION_05_CROP_INTERACTABLE_REPORT.md
docs/validation/WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_REPORT.md
docs/validation/WAVE_INTEGRATION_07_SHIPPING_ECONOMY_REPORT.md
docs/validation/WAVE_INTEGRATION_08_HUD_RUNTIME_BINDING_REPORT.md
docs/validation/05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime_execution_report.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/validation/05_spec_farm_crop_growth_soil_quality_runtime_execution_report.md
docs/validation/05_spec_farm_watering_irrigation_greenhouse_runtime_execution_report.md
docs/validation/05_spec_farm_resource_node_refresh_runtime_execution_report.md
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

## 3.5 Regra se referência não existir

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
4. Se arquivos existirem mas não trouxerem regra aplicável a skill effects/gameplay bridge, registrar:
   NO_APPLICABLE_SKILL_EFFECT_DIRECTION_FOUND
5. Se uma regra de design conflitar com a spec, parar com:
   BLOCKED_BY_DESIGN_DIRECTION_CONFLICT
```

Exemplos de regras a extrair, se existirem:

```text
- quais categorias de skill existem;
- se skill usa stamina/mana/cooldown;
- se active slots são 4;
- se skill exige alvo;
- se farm skill pode afetar crop/resource;
- se combat/magic fica fora do MVP;
- como HUD mostra cooldown;
- se skill effects precisam ser salvos;
- se target deve ser mouse, direção do player, objeto próximo ou self;
- se skill pode falhar e como mostrar blocked reason;
- restrições anti-spoiler/canon;
- restrições de debug skill temporária;
- padrões de events/feedback;
- padrões de ID estável.
```

Esta matriz é obrigatória mesmo que resulte em ausência documentada.

---

## 5. Dependências

## 5.1 Dependência obrigatória

Esta spec depende de:

```text
WAVE_INTEGRATION_10_skill_tree_ui_active_skill_equip
```

Critérios mínimos:

```text
1. Target FarmScene definida.
2. Uma skill ativa pode ser equipada, ou active slot debt está explicitamente resolvido/aceito.
3. Active slots 0..3 existem visualmente ou em runtime.
4. Skill authoring model existe.
5. Builds runtime/editor passando.
```

Se WAVE_INTEGRATION_10 não existir ou estiver `BLOCKED`:

```text
Status: BLOCKED_BY_MISSING_SKILL_TREE_EQUIP_BASELINE
```

Parar.

Se a WAVE10 não permite equipar nenhuma active skill:

```text
Status: BLOCKED_BY_MISSING_EQUIPPED_ACTIVE_SKILL
```

Parar, salvo se a tarefa for explicitamente criar apenas contratos e marcar `CODE_READY`.

## 5.2 Dependências runtime/gameplay

Auditar antes de criar código:

```text
ActiveSkillSlotService
SkillTreeService
SkillDefinition / SkillNodeDefinition
EffectId/effect key
Cooldown model
Cost model
Player condition / stamina
Targeting system
Interaction system
CropPlot/FarmCropPlotInteractable
FarmResourceInteractable
InventoryManager
HUD active slots/cooldown view
InputFocusRouter
GameEventBus
SaveManager, se cooldown/equip persistir
```

Regra:

```text
Reutilizar se existir.
Criar bridge pequeno se necessário.
Não criar sistema paralelo.
```

## 5.3 Regra adapter-first obrigatória

Se existir runtime real para skills/effects, os componentes Unity criados nesta spec devem atuar como **adapters/executors plugáveis**, não como um sistema paralelo.

Exemplos:

```text
ActiveSkillExecutionController pode:
- receber input de slot;
- consultar active slot runtime;
- validar custo/cooldown;
- construir SkillEffectContext;
- resolver executor por EffectId;
- executar executor;
- publicar feedback/evento;
- atualizar HUD.

ActiveSkillExecutionController não pode:
- decidir node purchase;
- criar active slot state paralelo;
- criar skill point state paralelo;
- implementar regras de todas as skills diretamente;
- executar efeito hardcoded por botão de UI;
- ignorar custo/cooldown se runtime existir.
```

Se runtime real existir mas não for integrável nesta spec:

```text
Status: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
```

ou:

```text
Status: BLOCKED_BY_SKILL_EFFECT_RUNTIME_INTEGRATION_GAP
```

---

## 6. Escopo permitido

Esta spec pode:

```text
1. Criar contratos mínimos de effect context/result/executor, se não existirem.
2. Criar SkillEffectRegistry/Resolver, se não houver equivalente.
3. Criar ActiveSkillExecutionController, se não houver equivalente.
4. Criar um executor de farm/utility mínimo para smoke validation.
5. Criar target resolver mínimo, se necessário.
6. Criar cooldown/cost bridge apenas se runtime já existir.
7. Integrar HUD feedback/cooldown se WAVE08/10 expôs slots.
8. Alterar FarmScene para adicionar controller ou references, se autorizado.
9. Criar authoring model de efeitos.
10. Criar checklist humano.
11. Atualizar CURRENT_STATE.md.
```

---

## 7. Escopo proibido

Esta spec não pode:

```text
1. Criar nova skill tree.
2. Reescrever active slots.
3. Reescrever skill points.
4. Implementar todos os efeitos finais.
5. Implementar sistema completo de combate.
6. Implementar magia completa.
7. Implementar balanceamento final.
8. Criar cooldown/cost runtime paralelo permanente.
9. Criar save/load novo, salvo bridge para API existente.
10. Criar assets finais de VFX/SFX.
11. Executar WAVE 13.
12. Executar future/mapped.
13. Executar pets.
14. Marcar ACCEPTED.
15. Alterar Packages/ProjectSettings sem autorização.
```

---

## 8. Política sobre scene/prefab/assets

Esta spec pode alterar a FarmScene alvo para adicionar o execution controller, se autorizado.

## 8.1 Permitido

```text
FarmScene .unity alvo
Scripts C# novos/alterados
Docs de validação
```

## 8.2 Evitar

```text
Prefabs novos
ScriptableObject assets novos
Sprites novos
VFX assets novos
Audio assets novos
Animator/Animation
```

Se precisar de feedback, usar:

```text
HUD feedback existente
debug label temporário
SpriteRenderer color/flash se já disponível
log apenas como fallback secundário
```

## 8.3 Proibido sem autorização explícita

```text
Packages/**
ProjectSettings/**
Assets/**/*.png
Assets/**/*.aseprite
Assets/**/*.anim
Assets/**/*.controller
Assets/**/*.wav
Assets/**/*.mp3
```

## 8.4 Se não puder alterar a scene automaticamente

Terminar como:

```text
CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
```

e criar instruções humanas detalhadas.

---

## 9. Pipeline mínimo de execução

O pipeline mínimo deve seguir:

```text
Input slot 1..4
→ resolve equipped ActiveSkill
→ validate skill unlocked/equipped
→ validate cooldown/cost
→ resolve target/context
→ resolve executor by EffectId
→ execute effect
→ produce SkillEffectResult
→ apply cooldown/cost if success
→ publish feedback
→ update HUD slot/cooldown/feedback
```

Se algum passo for temporário, registrar como debt.

---

## 10. Targeting mínimo

A spec deve escolher uma estratégia de targeting:

```text
SELF
NEAREST_INTERACTABLE
CURRENT_INTERACTABLE
WORLD_POINT_IN_FRONT_OF_PLAYER
SELECTED_CROP_PLOT
SELECTED_RESOURCE_NODE
DEBUG_FIXED_TARGET
```

Preferência para farm slice:

```text
CURRENT_INTERACTABLE
ou
NEAREST_INTERACTABLE
```

Se usar `DEBUG_FIXED_TARGET`:

```text
TODO_INTEGRATION_NOT_FINAL
Blocks final acceptance: YES
```

Não usar `FindObjectOfType` ou busca global não controlada em runtime.

---

## 11. Custo e cooldown

Auditar se existe:

```text
stamina
mana
hunger/fatigue
cooldown
skill cost
resource cost
```

Ordem preferencial:

```text
1. Usar runtime real de cost/cooldown.
2. Se não existir, executar sem custo/cooldown mas marcar debt.
3. Se debug cooldown for necessário, marcar TODO_INTEGRATION_NOT_FINAL.
```

Não criar stamina/mana paralelos.

Se a skill tiver custo definido e o bridge ignorar custo:

```text
Status: NEEDS_REWORK_COST_IGNORED
```

---

## 12. Efeito mínimo recomendado

Se o runtime permitir, implementar um destes como primeiro efeito:

## 12.1 Farm crop effect preferencial

```text
EffectId: farm.crop.water_or_advance_test
Target: crop plot/current interactable
Behavior:
- se target for crop plot com API de water, chamar water;
- senão, avançar visual/test state se for bridge temporário;
- publicar feedback: "Skill applied to crop plot";
- marcar TODO se for modo test.
```

## 12.2 Resource effect alternativo

```text
EffectId: farm.resource.collect_test
Target: resource/current interactable
Behavior:
- chama Interact ou API de resource se seguro;
- não duplica reward;
- respeita CanInteract;
- publica feedback.
```

## 12.3 Self/utility fallback

```text
EffectId: player.feedback_pulse_test
Target: self
Behavior:
- mostra feedback visual/HUD;
- não altera gameplay final;
- marcado TODO_INTEGRATION_NOT_FINAL.
```

A escolha deve estar no decision report.

---

## 13. Arquivos esperados

## 13.1 Documentação obrigatória

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_DECISION.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
docs/validation/WAVE_INTEGRATION_11_DASH_DODGE_MOVEMENT_CONTRACT.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_AUTHORING_MODEL.md
docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md
```

## 13.2 Documentação opcional

```text
docs/validation/WAVE_INTEGRATION_11_HUMAN_UNITY_SKILL_EFFECTS_WIRING_INSTRUCTIONS.md
```

## 13.3 Código opcional

Criar somente se não existir equivalente:

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectCategory.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectTargetType.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectContext.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectResult.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/ISkillEffectExecutor.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectRegistry.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillTargetResolver.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/FarmResourceSkillEffectExecutor.cs
Assets/_Game/Scripts/Player/Movement/PlayerMovementAbilityController.cs
Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs
Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs
Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs
Assets/_Game/Scripts/Player/Movement/GridMovementDisplacementResolver.cs
Assets/_Game/Scripts/Editor/Validation/ValidateSkillEffectsGameplayBridge.cs
Assets/_Game/Scripts/Editor/Validation/ValidateDashDodgeMovement.cs
```

## 13.4 Scene alvo

Apenas a FarmScene alvo pode ser alterada:

```text
<path-da-FarmScene>.unity
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

## STEP 02 — Confirmar WAVE_INTEGRATION_10

### Ação

```powershell
Get-ChildItem .\docs\validation -File |
  Where-Object {
    $_.Name -match 'WAVE_INTEGRATION_10|SKILL_TREE|ACTIVE_EQUIP'
  } |
  Sort-Object Name |
  Select-Object Name, FullName
```

Abrir documentos encontrados.

### Verificar

```text
Target FarmScene
Skill runtime strategy
Skill authoring model
Active slot strategy
Equipped active skill availability
Can start WAVE_INTEGRATION_11
```

### Se falhar

Parar:

```text
Status: BLOCKED_BY_MISSING_WAVE_INTEGRATION_10
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
    $_.FullName -match 'design|direction|directions|roadmap|validation|project|GDD|FASE|skill|effect|progression'
  } |
  Sort-Object FullName |
  Select-Object FullName
```

Ler arquivos aplicáveis a skill effects, active skills, cooldowns, costs, targeting e gameplay bridge.

### Resultado obrigatório

Preencher `Design/Direction Compliance Matrix`.

### Se nenhum arquivo aplicável existir

Registrar:

```text
NO_APPLICABLE_SKILL_EFFECT_DIRECTION_FOUND
```

---

## STEP 05 — Auditar runtime active skill/equip

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "ActiveSkill|ActiveSlot|SkillSlot|EquippedSkill|SkillTree|SkillNode|SkillId|EffectId|CanEquip|Equip"
```

### Resultado

Criar tabela:

```md
## Active skill runtime audit

| System/File | Found | Role | Decision |
|---|---:|---|---|
```

### Critério

Identificar:

```text
1. como saber skill equipada no slot;
2. como saber SkillId;
3. como saber EffectId;
4. como validar se skill está desbloqueada;
5. como atualizar HUD depois.
```

Se não houver active skill equipada:

```text
Status: BLOCKED_BY_MISSING_EQUIPPED_ACTIVE_SKILL
```

---

## STEP 06 — Auditar contratos de effects

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "SkillEffect|EffectId|EffectDefinition|EffectExecutor|AbilityEffect|Cooldown|Target|Targeting|Cost|Mana|Stamina"
```

### Resultado

Criar tabela:

```md
## Skill effect catalog coverage

| Tree | Skills found | Effects specified | Implement now | Deferred | Blocked |
|---|---:|---:|---:|---:|---:|

| Catalog artifact | Status | Evidence |
|---|---|---|
| `WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md` |  |  |

## Dash/Dodge movement contract

| Rule | Result | Evidence |
|---|---|---|
| Dash input found |  |  |
| Dash distance found |  |  |
| Dash moves forward X tiles |  |  |
| Dash respects collision/bounds |  |  |
| Dodge double tap detected |  |  |
| Dodge distance found |  |  |
| Dodge backward/lateral rule applied |  |  |
| Dodge respects collision/bounds |  |  |
| Dash/Dodge do not occupy active slot |  |  |

## Skill effect architecture audit

| Contract | Found | Evidence | Decision |
|---|---:|---|---|
| EffectId |  |  |  |
| Effect definition |  |  |  |
| Effect executor |  |  |  |
| Effect context |  |  |  |
| Effect result |  |  |  |
| Targeting model |  |  |  |
| Cost model |  |  |  |
| Cooldown model |  |  |  |
| Feedback/event model |  |  |  |
```

### Critério

A spec só pode prosseguir com execução real se houver ou for criado contrato mínimo de effect context/result/executor.

---

## STEP 07 — Auditar gameplay targets

### Ação

Buscar crop/resource/player condition APIs:

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "FarmCropPlot|CropPlot|Water|Harvest|Soil|FarmResourceInteractable|Resource|PlayerCondition|Stamina|Hunger|Fatigue|IInteractable|CanInteract|Interact"
```

### Decisão

Escolher efeito vertical slice:

```text
FARM_CROP_EFFECT
FARM_RESOURCE_EFFECT
PLAYER_SELF_EFFECT
DEBUG_FEEDBACK_EFFECT
```

Registrar por quê.

---

## STEP 08 — Auditar input/HUD feedback

### Ação

```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "HUD|Feedback|ActiveSkillSlot|Hotbar|Cooldown|Input|KeyCode|InputAction|InputFocus|GameplayFocus"
```

### Decisão

Escolher:

```text
USE_EXISTING_ACTIVE_SLOT_INPUT
CREATE_TEMPORARY_ACTIVE_SLOT_INPUT
USE_HUD_FEEDBACK
USE_DEBUG_FEEDBACK
```

Se criar input temporário, marcar:

```text
TODO_INTEGRATION_NOT_FINAL
```

---


## STEP 08.1 — Criar catálogo completo de efeitos por skill tree

### Ação

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
```

O catálogo deve ser construído a partir dos documentos de design/refinement e do runtime real.

Buscar:

```powershell
Get-ChildItem .\docs -Recurse -File |
  Select-String -Pattern "Skill Tree|SkillTree|SkillPoint|SkillNode|Active Skill|Passive Skill|Capstone|Dash|Dodge|Block|EffectId|cooldown|stamina|mana|rank|prerequisite"

Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "SkillTree|SkillNode|SkillDefinition|SkillEffect|EffectId|ActiveSkill|Passive|Capstone|Dash|Dodge|Block"
```

### Critério

O catálogo precisa listar:

```text
1. todas as skill trees encontradas;
2. todos os nodes/skills encontrados;
3. se cada skill é ativa, passiva ou capstone;
4. se ocupa active slot;
5. EffectId ou EFFECT_UNSPECIFIED;
6. target type;
7. custo/cooldown;
8. efeito de gameplay planejado;
9. se implementa agora ou fica deferred;
10. motivo do deferred.
```

### Bloqueio

Se o agente não conseguir identificar as skill trees e skills existentes:

```text
Status: BLOCKED_BY_MISSING_SKILL_TREE_EFFECT_SOURCE
```

Se identificar skills, mas efeitos não estiverem especificados:

```text
Status: BLOCKED_BY_UNSPECIFIED_SKILL_EFFECTS
```

salvo se o objetivo for criar apenas catálogo parcial e registrar `PARTIAL`.

---

## STEP 08.2 — Auditar e implementar Dash/Dodge conforme refinement

### Ação

Buscar regras exatas de dash/dodge:

```powershell
Get-ChildItem .\docs -Recurse -File |
  Select-String -Pattern "Dash|Dodge|double tap|double-tap|quadrado|quadrados|tile|tiles|squares|esquiva|bloqueio"

Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs |
  Select-String -Pattern "Dash|Dodge|DoubleTap|PlayerMovement|MovementAbility|Block"
```

### Criar ou atualizar documento

```text
docs/validation/WAVE_INTEGRATION_11_DASH_DODGE_MOVEMENT_CONTRACT.md
```

Template:

```md
# WAVE INTEGRATION 11 — Dash/Dodge Movement Contract

## Status
COMPLETE / PARTIAL / BLOCKED

## Source of truth

| Rule | Value | Source | Evidence |
|---|---|---|---|
| Dash input/button |  |  |  |
| Dodge input/double tap |  |  |  |
| Block input/button |  |  |  |
| Dash distance in tiles/squares |  |  |  |
| Dodge distance in tiles/squares |  |  |  |
| Double tap window |  |  |  |
| Dash cooldown/cost |  |  |  |
| Dodge cooldown/cost |  |  |  |
| Collision rule |  |  |  |
| I-frame/invulnerability rule |  |  |  |

## Dash behavior

## Dodge behavior

## Block behavior

## Implementation decision

## Remaining debt
```

### Implementação esperada

Dash:

```text
button/refinement input → move X tiles forward → collision/bounds safe → cooldown/cost → feedback
```

Dodge:

```text
double tap directional → move X tiles backward or sideways depending on double tap/facing → collision/bounds safe → cooldown/cost → feedback
```

### Critérios de bloqueio

Se faltar distância X:

```text
Status: BLOCKED_BY_MISSING_DASH_DODGE_REFINEMENT_DISTANCE
```

Se faltar botão/input definido:

```text
Status: BLOCKED_BY_MISSING_DASH_DODGE_INPUT_MAPPING
```

Se colisão/bounds não puderem ser respeitados:

```text
Status: BLOCKED_BY_UNSAFE_DASH_DODGE_COLLISION
```

---

## STEP 09 — Criar decisão de skill effects bridge

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_DECISION.md
```

Conteúdo:

```md
# WAVE INTEGRATION 11 — Skill Effects Gameplay Bridge Decision

## Status
DRAFT / VALIDATED / BLOCKED

## Target FarmScene

| Field | Value |
|---|---|

## Active skill source strategy

## Effect architecture strategy

## EffectId strategy

## Targeting strategy

## Cost strategy

## Cooldown strategy

## Feedback strategy

## Vertical slice effect strategy

## HUD/update strategy

## Save/load strategy

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|

## Temporary/debt policy

- TODO_INTEGRATION_NOT_FINAL required:
- Debug effect data:
- Temporary input:
- Cost/cooldown debt:
- Targeting debt:
- Save/load debt:

## Scene modification policy

## Human Unity actions required
```

---

## STEP 10 — Criar contratos de effect, se necessário

Criar apenas se não existirem equivalentes.

Arquivos possíveis:

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectCategory.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectTargetType.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectContext.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectResult.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/ISkillEffectExecutor.cs
```

Requisitos:

```text
1. SkillEffectContext deve carregar SkillId, EffectId, slot index, caster, target/context.
2. SkillEffectResult deve carregar success/failure reason/feedback/cost/cooldown flags.
3. Executor não deve depender de UI.
4. Nenhum contrato deve criar estado global paralelo.
```

---

## STEP 11 — Criar SkillEffectRegistry, se necessário

Criar:

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectRegistry.cs
```

Requisitos:

```text
1. Resolver executor por EffectId.
2. Permitir registrar novos executors.
3. Não hardcodar lógica de todos os efeitos em um switch gigante.
4. Se usar fallback dictionary, manter centralizado.
5. Permitir extensão futura.
```

Aceitável no MVP:

```text
um registry simples com lista serializada ou registro explícito centralizado,
desde que não esteja dentro da UI/HUD/input.
```

---

## STEP 12 — Criar SkillTargetResolver, se necessário

Criar:

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/SkillTargetResolver.cs
```

Requisitos:

```text
1. Resolver alvo de forma controlada.
2. Não usar busca global pesada.
3. Preferir current interactable ou referência serializada.
4. Permitir SELF fallback.
5. Retornar erro claro se alvo inválido.
```

---

## STEP 13 — Criar ActiveSkillExecutionController, se necessário

Criar:

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
```

Requisitos:

```text
1. Recebe comando de slot 0..3.
2. Consulta active skill equipada.
3. Valida unlocked/equipped.
4. Valida custo/cooldown se runtime existir.
5. Resolve EffectId.
6. Resolve target/context.
7. Chama SkillEffectRegistry/executor.
8. Aplica cost/cooldown se sucesso e runtime existir.
9. Publica feedback/HUD/event.
10. Não implementa efeito direto no controller.
11. Não usa GameObject.Find/FindObjectOfType.
12. Não cria active slot state paralelo.
```

Se active slot runtime não expõe API suficiente:

```text
Status: BLOCKED_BY_ACTIVE_SLOT_EXECUTION_API_GAP
```

ou documentar manual wiring se for só referência Unity.

---

## STEP 14 — Criar executor vertical slice

Criar apenas o executor escolhido na decisão.

Opções:

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/FarmResourceSkillEffectExecutor.cs
```

### Regras para FarmCropSkillEffectExecutor

```text
1. Aceitar target crop plot ou current interactable.
2. Se API real de water/advance existir, usar API real.
3. Se só existir bridge visual temporário, usar método público seguro e marcar TODO.
4. Não duplicar crop runtime.
5. Retornar SkillEffectResult.
```

### Regras para FarmResourceSkillEffectExecutor

```text
1. Aceitar target resource interactable.
2. Respeitar CanInteract.
3. Não duplicar reward.
4. Chamar API segura.
5. Retornar SkillEffectResult.
```

Se usar effect de feedback apenas:

```text
Status: BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
```

---

## STEP 15 — Conectar input de active slot

Se já houver input/hotbar:

```text
usar input existente.
```

Se não houver:

```text
criar input temporário para slot 1 apenas, com TODO_INTEGRATION_NOT_FINAL.
```

Exemplo de política:

```text
Key 1 → active slot 0
Key 2 → active slot 1
Key 3 → active slot 2
Key 4 → active slot 3
```

Somente se isso não conflitar com hotbar existente.

Não criar input final se o projeto já tem Input System definido.

---

## STEP 16 — Integrar HUD feedback/cooldown

Se HUD existe:

```text
1. mostrar feedback de sucesso/falha;
2. mostrar cooldown/cost se disponível;
3. atualizar active slot visual;
```

Se HUD não suporta cooldown:

```text
HUD_COOLDOWN_DEFERRED_WITH_REASON
```

Não bloquear execução do efeito se feedback básico existir.

---

## STEP 17 — Criar Skill Effect Authoring Model

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_AUTHORING_MODEL.md
```

Template:

```md
# WAVE INTEGRATION 11 — Skill Effect Authoring Model

## Status
COMPLETE / PARTIAL / BLOCKED

## Purpose

This document explains how to add, modify and connect skill gameplay effects after WAVE_INTEGRATION_11.

## Effect definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|

## Stable IDs

| ID type | Format/rule | Used by save? | Notes |
|---|---|---:|---|
| SkillId |  |  |  |
| EffectId |  |  |  |

## How to add a new active skill effect

1.
2.
3.

## How to map SkillId/SkillNode to EffectId

## How to add a new executor

## How to define target type

## How to define cost/cooldown

## How to publish feedback/HUD events

## How to test an effect in isolation

## How save/load should persist cooldown/equipped state

## Known debts

| Debt | Impact | Required before |
|---|---|---|
```

If the system does not yet allow real effect authoring:

```text
SKILL_EFFECT_AUTHORING_MODEL_INCOMPLETE
```

---

## STEP 18 — Alterar FarmScene com execution controller, se autorizado

### Condição

Só executar se:

```text
1. Target FarmScene existe.
2. Scene changes estão autorizadas.
3. Skill execution controller precisa existir em scene.
4. O agente consegue editar a scene com segurança.
```

### Ações possíveis

Adicionar ou configurar:

```text
ActiveSkillExecutionController
SkillEffectRegistry
SkillTargetResolver
references para active slots/HUD/target source
```

Se não puder alterar scene:

```text
CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
```

e criar instruções humanas.

---

## STEP 19 — Criar instruções humanas, se necessário

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_HUMAN_UNITY_SKILL_EFFECTS_WIRING_INSTRUCTIONS.md
```

Conteúdo:

```md
# WAVE INTEGRATION 11 — Human Unity Skill Effects Wiring Instructions

## Target FarmScene

## Required hierarchy

| Object | Component | References |
|---|---|---|

## Step-by-step

1. Open target FarmScene.
2. Locate skill/active slot runtime object.
3. Create or locate SkillEffectsRoot.
4. Add SkillEffectRegistry.
5. Add SkillTargetResolver.
6. Add ActiveSkillExecutionController.
7. Add chosen executor.
8. Assign active slot source.
9. Assign HUD feedback source.
10. Assign target source/current interactable source.
11. Enter Play Mode.
12. Equip active skill from WAVE10.
13. Target a crop/resource/self depending on decision.
14. Press active slot key.
15. Confirm effect result.
16. Confirm HUD/feedback.
17. Confirm cooldown/cost behavior or debt.

## Validation checklist
```

---

## STEP 20 — Criar editor validator opcional

Se útil:

```text
Assets/_Game/Scripts/Editor/Validation/ValidateSkillEffectsGameplayBridge.cs
```

Requisitos:

```text
1. Editor-only.
2. Não usar IProjectValidator/ValidationReport.
3. Validar existência dos scripts.
4. Validar que registry tem executor para EffectId de teste.
5. Validar que controller não está sem referências críticas.
6. Não abrir scene automaticamente se instável.
7. Não ser critério único de sucesso.
```

---

## STEP 21 — Atualizar CURRENT_STATE

Atualizar:

```text
docs/project/CURRENT_STATE.md
```

Com:

```md
## WAVE_INTEGRATION — Scene Wiring e Playable Runtime Bridge

### WAVE_INTEGRATION_11 — Skill Effects Gameplay Bridge

Status: BUILD_VALIDATED_SCENE_WIRED / BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT / CODE_READY_HUMAN_UNITY_ACTION_REQUIRED / BLOCKED

Validation:
- Assembly-CSharp:
- Assembly-CSharp-Editor:
- Docs validation:
- Target FarmScene:
- Active skill source:
- Effect architecture:
- EffectId:
- Targeting:
- Cost/cooldown:
- Vertical slice effect:
- HUD feedback:
- Save/load:
- Can start WAVE_INTEGRATION_12:
```

---

## STEP 22 — Build depois

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

## STEP 23 — Criar execution report

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md
```

Template:

```md
# WAVE INTEGRATION 11 — Skill Effects Gameplay Bridge — Execution Report

## Status
BUILD_VALIDATED_SCENE_WIRED / BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT / CODE_READY_HUMAN_UNITY_ACTION_REQUIRED / BLOCKED

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

## Baseline gates from WAVE 10

| Gate | Result | Evidence |
|---|---|---|

## Build validation

| Target | Before | After | Result |
|---|---|---|---|

## Target FarmScene

| Field | Value |
|---|---|

## Active skill runtime audit

| System/File | Found | Evidence | Decision |
|---|---:|---|---|

## Skill effect catalog coverage

| Tree | Skills found | Effects specified | Implement now | Deferred | Blocked |
|---|---:|---:|---:|---:|---:|

| Catalog artifact | Status | Evidence |
|---|---|---|
| `WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md` |  |  |

## Dash/Dodge movement contract

| Rule | Result | Evidence |
|---|---|---|
| Dash input found |  |  |
| Dash distance found |  |  |
| Dash moves forward X tiles |  |  |
| Dash respects collision/bounds |  |  |
| Dodge double tap detected |  |  |
| Dodge distance found |  |  |
| Dodge backward/lateral rule applied |  |  |
| Dodge respects collision/bounds |  |  |
| Dash/Dodge do not occupy active slot |  |  |

## Skill effect architecture audit

| Contract | Found | Evidence | Decision |
|---|---:|---|---|
| EffectId |  |  |  |
| Effect definition |  |  |  |
| Effect executor |  |  |  |
| Effect context |  |  |  |
| Effect result |  |  |  |
| Targeting model |  |  |  |
| Cost model |  |  |  |
| Cooldown model |  |  |  |
| Feedback/event model |  |  |  |

## Gameplay target audit

| Target system | Found | Evidence | Decision |
|---|---:|---|---|

## Input/HUD feedback audit

| System/File | Found | Evidence | Decision |
|---|---:|---|---|

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|

## Integration strategy

| Area | Strategy | Notes |
|---|---|---|
| Active skill source |  |  |
| Effect architecture |  |  |
| EffectId |  |  |
| Targeting |  |  |
| Cost |  |  |
| Cooldown |  |  |
| Vertical slice effect |  |  |
| Input |  |  |
| HUD feedback |  |  |
| Save/load |  |  |

## Temporary/debt declaration

| Item | Value |
|---|---|
| TODO_INTEGRATION_NOT_FINAL required |  |
| Debug effect data |  |
| Temporary input |  |
| Cost/cooldown debt |  |
| Targeting debt |  |
| Save/load debt |  |
| Risk |  |

## Skill effect authoring model

| Item | Result | Evidence |
|---|---|---|
| Authoring model created |  |  |
| EffectId mapping documented |  |  |
| New executor flow documented |  |  |
| Target type authoring documented |  |  |
| Cost/cooldown authoring documented |  |  |
| Testing guidance documented |  |  |

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
| Applicable skill effect rules extracted |  |  |
| EffectId/data-driven rules applied |  |  |
| Targeting rules applied |  |  |
| Cost/cooldown rules applied or deferred |  |  |
| Active slot execution rules applied |  |  |
| HUD/feedback rules applied |  |  |
| Save/load rules applied or deferred |  |  |
| Skill effect catalog coverage validated |  |  |
| Dash input/distance rule validated |  |  |
| Dodge double tap/distance rule validated |  |  |
| Dash/Dodge non-slot rule validated |  |  |
| No design conflict remains |  |  |

## Decision

- Can start WAVE_INTEGRATION_12:
- Blocking issues:
- Human Play Mode validation required:
```

---

## STEP 24 — Criar checklist humano

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md
```

Conteúdo:

```md
# WAVE INTEGRATION 11 — Human Play Mode Checklist

## Status
PENDING / PASS / FAIL

## Preconditions

- Unity opens without red console errors.
- WAVE_INTEGRATION_10 skill tree/equip baseline is available.
- Player has active skill equipped or debug active skill is explicitly configured.
- Target FarmScene contains skill effect wiring or human wiring was applied.

## Checklist

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open FarmScene | Scene opens |  |  |
| Press Play | Game starts |  |  |
| Player movement | Player moves |  |  |
| Open skill tree | Skill tree opens |  |  |
| Equip active skill | Skill appears in active slot |  |  |
| Close skill tree | Gameplay resumes |  |  |
| Select/approach target | Valid target exists or self effect is ready |  |  |
| Press active slot input | Skill execution triggers |  |  |
| Use Dash input | Player dashes X tiles/squares forward |  |  |
| Dash blocked collision | Dash stops before obstacle/bounds |  |  |
| Double tap back | Player dodges X tiles/squares backward |  |  |
| Double tap lateral | Player dodges X tiles/squares sideways |  |  |
| Dash/Dodge active slots | Dash/Dodge do not occupy active skill slots |  |  |
| Validate cost | Cost is spent or debt is explicit |  |  |
| Validate cooldown | Cooldown starts or debt is explicit |  |  |
| Validate effect result | Crop/resource/self state changes or feedback appears |  |  |
| Validate HUD feedback | Success/failure appears |  |  |
| Invalid target case | Correct blocked reason appears |  |  |
| Repeated use | Cooldown/cost/blocked behavior is correct or debt explicit |  |  |
| Stop Play | Scene is not corrupted |  |  |

## Result

## Bugs found

## Can start WAVE_INTEGRATION_12
YES/NO
```

---

# 15. Acceptance Criteria

| ID | Critério | Obrigatório |
|---|---|---|
| AC-01 | WAVE_INTEGRATION_10 existe e não está BLOCKED | Sim |
| AC-02 | Build runtime antes/depois passa | Sim |
| AC-03 | Build editor antes/depois passa | Sim |
| AC-04 | Design/Direction Compliance Matrix foi preenchida ou ausência foi registrada | Sim |
| AC-05 | Active skill/equip runtime foi auditado | Sim |
| AC-06 | Effect architecture foi auditada | Sim |
| AC-07 | EffectId ou equivalente foi confirmado/criado | Sim |
| AC-08 | Effect context/result/executor existem ou blocker declarado | Sim |
| AC-09 | Targeting strategy foi definida | Sim |
| AC-10 | Cost/cooldown strategy foi definida | Sim |
| AC-11 | Pelo menos um vertical slice effect foi definido | Sim |
| AC-12 | Active slot input executa skill ou instrução humana clara existe | Sim |
| AC-13 | HUD/feedback mostra sucesso/falha ou debt explícito | Sim |
| AC-14 | Skill Effect Authoring Model foi criado | Sim |
| AC-15 | UI não executa efeito diretamente | Sim |
| AC-16 | Scene changes foram documentadas, se ocorreram | Sim |
| AC-17 | Checklist humano foi criado | Sim |
| AC-18 | Final Design/Direction Revalidation existe no report | Sim |
| AC-19 | Skill Effect Catalog cobre todas as árvores/skills encontradas ou blocker declarado | Sim |
| AC-20 | Cada skill/node tem EffectId ou EFFECT_UNSPECIFIED registrado | Sim |
| AC-21 | Dash não ocupa active slot e funciona por input/refinement button | Sim |
| AC-22 | Dash move X tiles/quadrados para frente conforme refinement | Sim |
| AC-23 | Dodge não ocupa active slot e funciona por double tap | Sim |
| AC-24 | Dodge move X tiles/quadrados para trás ou laterais conforme double tap/facing | Sim |
| AC-25 | Dash/Dodge respeitam colisão e bounds | Sim |
| AC-26 | Dash/Dodge Movement Contract foi criado | Sim |

---

# 16. Definition of Done

A spec termina como `BUILD_VALIDATED_SCENE_WIRED` se:

```text
1. Uma active skill equipada pode ser acionada.
2. EffectId resolve para executor.
3. Target/context é criado.
4. Executor aplica efeito real ou bridge aceito.
5. Cost/cooldown são respeitados se runtime existir.
6. Feedback/HUD mostra resultado.
7. UI/input não aplica efeito diretamente.
8. Authoring model de effects foi criado.
9. Builds passam.
10. Skill Effect Catalog foi criado e cobre todas as skills encontradas ou blockers estão explícitos.
11. Dash/Dodge Movement Contract foi criado.
12. Dash funciona para frente por X tiles/quadrados conforme refinement.
13. Dodge funciona por double tap para trás/laterais conforme refinement.
14. Dash/Dodge não ocupam active slot.
15. Checklist humano pode ser executado.
16. Final Design/Direction Revalidation foi preenchida.
```

A spec termina como `BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT` se:

```text
1. Pipeline existe.
2. Efeito executa com fallback temporário.
3. Debt foi marcado como não-final.
4. Builds passam.
5. WAVE12 pode continuar sem depender do efeito final.
```

A spec termina como `CODE_READY_HUMAN_UNITY_ACTION_REQUIRED` se:

```text
1. Código está pronto.
2. Builds passam.
3. Instruções humanas de wiring existem.
4. Scene/controller precisa ser ajustado manualmente no Unity.
```

A spec termina como `BLOCKED` se:

```text
1. WAVE10 não permite active skill equipada.
2. Active slot runtime não expõe skill equipada.
3. Effect contract não existe e não pode ser criado sem quebrar arquitetura.
4. Targeting não pode ser definido.
5. Builds falham.
6. Design/direction conflita.
7. Alteração proibida seria necessária.
```

---

# 17. Commit

Se tudo passar:

```powershell
git add `
  docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_DECISION.md `
  docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md `
  docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md `
  docs/validation/WAVE_INTEGRATION_11_DASH_DODGE_MOVEMENT_CONTRACT.md `
  docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_AUTHORING_MODEL.md `
  docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md `
  docs/project/CURRENT_STATE.md
```

Adicionar código se criado:

```powershell
git add Assets/_Game/Scripts/Skills/Runtime/Effects
git add Assets/_Game/Scripts/Player/Movement
git add Assets/_Game/Scripts/Editor/Validation/ValidateSkillEffectsGameplayBridge.cs
git add Assets/_Game/Scripts/Editor/Validation/ValidateDashDodgeMovement.cs
```

Adicionar instruções humanas se criadas:

```powershell
git add docs/validation/WAVE_INTEGRATION_11_HUMAN_UNITY_SKILL_EFFECTS_WIRING_INSTRUCTIONS.md
```

Adicionar scene somente se alterada explicitamente:

```powershell
git add <path-da-FarmScene>.unity
```

Nunca usar:

```powershell
git add Assets
```

Commit:

```powershell
git commit -m "feat: execute wave integration 11 skill effects gameplay bridge"
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
WAVE_INTEGRATION_10 baseline:
Target FarmScene:
Design/direction references checked:
Design/direction compliance:
Active skill source:
Effect architecture:
EffectId strategy:
Targeting strategy:
Cost strategy:
Cooldown strategy:
Skill effect catalog:
Dash/Dodge movement contract:
Dash strategy:
Dodge strategy:
Block strategy:
Vertical slice effect:
Input strategy:
HUD feedback:
Save/load strategy:
Skill effect authoring model:
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
Can start WAVE_INTEGRATION_12:
Human Play Mode validation needed:
```

---

# 19. Critérios de bloqueio

Parar imediatamente se:

```text
1. Branch não for dev.
2. Working tree estiver suja.
3. WAVE_INTEGRATION_10 não existir ou estiver BLOCKED.
4. Não existir active skill equipada ou caminho claro para equipar.
5. Target FarmScene não existir.
6. Skill effect contract não puder ser definido.
7. EffectId não puder ser resolvido.
8. Targeting não puder ser definido.
9. Build runtime falhar.
10. Build editor falhar.
11. A spec tentar implementar skill tree/active slots de novo.
12. A spec tentar implementar todos os efeitos finais do jogo.
13. Alteração de scene for necessária sem autorização.
14. Design/direction conflict for encontrado.
15. Skill Effect Catalog incompleto sem blocker honesto.
16. Dash/Dodge distance/input não encontrado no refinement.
17. Dash/Dodge implementados como active slot por engano.
18. Dash/Dodge ignoram colisão/bounds.
```

---

## 20. Revalidação final contra design/direction

Antes de concluir, o agente deve executar uma última revisão e registrar no execution report:

```md
## Final Design/Direction Revalidation

| Check | Result | Evidence |
|---|---|---|
| All listed design/direction references checked |  |  |
| Applicable skill effect rules extracted |  |  |
| EffectId/data-driven rules applied |  |  |
| Targeting rules applied |  |  |
| Cost/cooldown rules applied or deferred |  |  |
| Active slot execution rules applied |  |  |
| HUD/feedback rules applied |  |  |
| Save/load rules applied or deferred |  |  |
| Skill effect catalog coverage validated |  |  |
| Dash input/distance rule validated |  |  |
| Dodge double tap/distance rule validated |  |  |
| Dash/Dodge non-slot rule validated |  |  |
| No design conflict remains |  |  |
```

Se esta seção não existir no execution report, a spec deve ser considerada incompleta.

---

## 21. Observação final

Esta spec não cria todas as skills finais.

Ela cria o pipeline que permite:

```text
active skill equipada → input → target/context → executor → efeito → feedback
```

A próxima spec, `WAVE_INTEGRATION_12_npc_placement_dialogue_shop_bridge`, só deve começar se o playable slice básico continuar estável e se os efeitos de skill não deixarem input/focus/HUD quebrados.
