# WAVE_INTEGRATION_11 — Action Skill Balance Patch

> **Projeto:** Cindar's Hope  
> **Aplica sobre:** `WAVE_INTEGRATION_11_skill_effects_gameplay_bridge_REVISED.md`  
> **Tipo:** Patch obrigatório de catálogo, balanceamento de action skills e execução de efeitos  
> **Motivo:** A WAVE_INTEGRATION_10 já foi implementada. Não reexecutar a WAVE10. Corrigir a WAVE11 para garantir que todas as skill trees tenham catálogo completo, action skills equipáveis suficientes e tratamento separado para Dash/Dodge/Block.  
> **Regra:** só adicionar. Não remover skills existentes, não renomear skills existentes, não quebrar saves/IDs já criados.

---

## 0. Decisão

A WAVE_INTEGRATION_11 precisa ser ajustada antes da execução.

A WAVE_INTEGRATION_10 já foi implementada, então esta correção **não deve voltar para refazer a skill tree UI**. Ela deve:

```text
1. Ler o runtime/data já criado pela WAVE_INTEGRATION_10.
2. Criar um catálogo canônico de skills/effects.
3. Adicionar action skills faltantes como extensão data-driven.
4. Não remover nenhum node já existente.
5. Não trocar IDs existentes.
6. Mapear cada skill para active/passive/modifier/capstone/non-slot.
7. Garantir que active slot skills tenham EffectId.
8. Garantir que Dash/Dodge/Block não ocupem active slot.
9. Implementar pelo menos o primeiro vertical slice de execução de efeito.
```

---

## 1. Regra crítica: WAVE10 já executada

Não executar novamente:

```text
WAVE_INTEGRATION_10_skill_tree_ui_active_skill_equip
```

Não destruir/recriar:

```text
SkillTree UI
SkillTreeRuntimeBinder
SkillNodeRuntimeView
SkillNodeDetailRuntimeView
ActiveSkillSlotEquipController
ActiveSkillSlotsRuntimeView
SkillTreeInputController
Skill authoring model já existente
Skill definitions já criadas
Skill node IDs já criados
```

A WAVE11 deve tratar os dados da WAVE10 como baseline.

Se faltar algo na WAVE10 para suportar novas action skills:

```text
criar migration/addendum data-driven;
não refazer UI;
não criar runtime paralelo.
```

---

## 2. Fonte canônica obrigatória

A WAVE11 deve usar como fonte primária:

```text
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
```

E deve cruzar com:

```text
docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_REPORT.md
docs/validation/WAVE_INTEGRATION_10_SKILL_AUTHORING_MODEL.md
docs/project/CURRENT_STATE.md
Assets/_Game/Scripts/**/Skill*.cs
Assets/_Game/**/Skill*
```

Se `PLAYER_SKILL_TREES_DIRECTION.md` existir, ele é a fonte de verdade para:

```text
- árvores canônicas;
- skill point economy;
- active slots;
- Dash/Dodge/Block;
- skills existentes;
- passivas;
- modifiers;
- capstones.
```

---

## 3. Árvores canônicas

A WAVE11 deve tratar estas árvores como canônicas:

```text
1. Melee / Guerreiro
2. Ranged / Caçador
3. Magic / Arcano
4. Survival / Sobrevivente
5. Crafting / Produção
```

Sistemas sociais, romance, companions e pets continuam transversais/futuros.

---

## 4. Active slot e non-slot actions

Regra final:

```text
4 active slots são para habilidades equipáveis.
Dash não ocupa active slot.
Dodge não ocupa active slot.
Block não ocupa active slot.
Dash = Space + direção.
Dodge = double tap direcional.
Block = Left Shift.
```

A WAVE11 deve refletir isso em:

```text
WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
WAVE_INTEGRATION_11_SKILL_ACTION_SLOT_MAPPING.md
WAVE_INTEGRATION_11_DASH_DODGE_MOVEMENT_CONTRACT.md
WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md
```

---

## 5. Documentos obrigatórios novos/atualizados

Além dos documentos já exigidos pela WAVE11, criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_BALANCE_ADDENDUM.md
docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_SLOT_MAPPING.md
docs/validation/WAVE_INTEGRATION_11_SKILL_PASSIVE_MODIFIER_MAPPING.md
docs/validation/WAVE_INTEGRATION_11_SKILL_MOVEMENT_ACTIONS_MAPPING.md
```

Atualizar obrigatoriamente:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_DECISION.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md
docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_AUTHORING_MODEL.md
docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md
docs/project/CURRENT_STATE.md
```

---

## 6. Objetivo de balanceamento de action skills

Objetivo desta correção:

```text
Cada árvore deve ter quantidade minimamente equilibrada de action skills equipáveis.
```

Base pretendida para o catálogo:

```text
Melee: 5 active slot skills
Ranged: 5 active slot skills
Magic: 5 active slot skills
Survival: 5 active slot utility skills
Crafting: 5 active slot utility skills
```

Regra:

```text
Só adicionar.
Não remover.
Não renomear.
Não reduzir rank.
Não alterar capstones existentes.
```

Se o runtime atual não aceitar 25 active slot skills totais ainda:

```text
1. registrar todos no catálogo;
2. implementar só subset vertical slice;
3. marcar os demais como DEFERRED_RUNTIME_EFFECT;
4. manter authoring pronto.
```

---

# 7. Action Skill Balance Addendum

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_BALANCE_ADDENDUM.md
```

Conteúdo obrigatório abaixo.

---

## 7.1 Melee / Guerreiro

### Skills existentes que continuam

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId sugerido | Observação |
|---|---:|---:|---|---:|---|---|
| Corte Amplo | 2 | 1-3 | Active Skill | YES | `melee.corte_amplo` | ataque em arco curto |
| Golpe de Ruptura | 4 | 1-3 | Active Skill | YES | `melee.golpe_ruptura` | golpe forte com posture damage e knockback |

### Novas action skills adicionadas

Adicionar sem remover nada:

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---:|---|---|---|
| Avanço de Aço | 2 | 1-3 | Active Skill | YES | `melee.avanco_aco` | Avanço curto ofensivo até inimigo/posição à frente, seguido de golpe melee. Não é Dash. Respeita colisão e bounds. Rank aumenta alcance curto/dano leve/posture. | rastro metálico curto + impacto |
| Grito de Desafio | 3 | 1-3 | Active Utility | YES | `melee.grito_desafio` | Provoca inimigos próximos por curta duração, aumenta estabilidade contra stagger por poucos segundos. Em bosses, aplica apenas postura/atenção reduzida, sem taunt total. | pulso circular + ícone de ameaça |
| Investida Quebra-Guarda | 4 | 1-3 | Active Skill | YES | `melee.investida_quebra_guarda` | Ombro/escudo/arma avança curto e causa alto posture damage frontal. Se alvo estiver bloqueando/canalizando, aplica stagger maior. Não atravessa inimigos sólidos. | impacto pesado + quebra visual |

### Total esperado

```text
Melee active slot skills = 5
```

---

## 7.2 Ranged / Caçador

### Skills existentes que continuam

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId sugerido | Observação |
|---|---:|---:|---|---:|---|---|
| Marcador de Presa | 1 | 1-3 | Active Skill | YES | `ranged.marcador_presa` | marca 1 alvo |
| Disparo Carregado | 1 | 1-5 | Active Skill | YES | `ranged.disparo_carregado` | tiro com windup |
| Disparo de Interrupção | 3 | 1-3 | Active Skill | YES | `ranged.disparo_interrupcao` | interrompe casts/canalizações |
| Flecha Perfurante | 3 | 1-3 | Active Skill | YES | `ranged.flecha_perfurante` | atravessa alvos |
| Armadilha de Caçador | 3 | 1-3 | Active Utility | YES | `ranged.armadilha_cacador` | trap de slow/root curto |

### Novas action skills adicionadas

Nenhuma obrigatória nesta correção.

Ranged já possui 5 active slot skills.

### Total esperado

```text
Ranged active slot skills = 5
```

---

## 7.3 Magic / Arcano

### Skills existentes que continuam

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId sugerido | Observação |
|---|---:|---:|---|---:|---|---|
| Projétil Arcano | 1 | 1-5 | Active Skill | YES | `magic.projetil_arcano` | dano arcano básico |
| Selo de Proteção | 2 | 1-3 | Active Skill | YES | `magic.selo_protecao` | barreira curta |
| Toque Restaurador | 3 | 1-5 | Active Skill | YES | `magic.toque_restaurador` | cura com custo alto/cooldown alto |

### Novas action skills adicionadas

Adicionar sem remover nada:

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---:|---|---|---|
| Chama Breve | 2 | 1-3 | Active Skill | YES | `magic.chama_breve` | Pequeno cone ou projétil curto de fogo. Causa dano moderado e chance leve de Burn. Escala com Afinidade Elemental: Fogo. | chama curta + ícone burn |
| Rajada Gélida | 3 | 1-3 | Active Skill | YES | `magic.rajada_gelida` | Rajada curta de gelo/natureza fria. Aplica Chill/slow leve por curta duração, sem stun abusivo. Escala com Afinidade Elemental: Gelo. | sopro frio + ícone slow |

### Total esperado

```text
Magic active slot skills = 5
```

---

## 7.4 Survival / Sobrevivente

Survival já melhora Dash/Dodge e sobrevivência, mas quase não possui action skills equipáveis. Esta correção adiciona utilities equipáveis sem transformar Survival em árvore de dano principal.

### Skills existentes que continuam

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId sugerido | Observação |
|---|---:|---:|---|---:|---|---|
| Descanso Curto | 3 | 1-5 | Utility/Passive | NO por padrão | `survival.descanso_curto_passive` | fora de combate recupera fração de Stamina/cansaço; não precisa ocupar slot salvo decisão futura |

### Novas action skills adicionadas

Adicionar sem remover nada:

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---:|---|---|---|
| Sinal de Retirada | 2 | 1-3 | Active Utility | YES | `survival.sinal_retirada` | Buff curto de evasão: reduz custo de Stamina de movimento/Dodge por poucos segundos e reduz cansaço gerado. Não é Dash. | pegadas claras + ícone retirada |
| Isca Improvisada | 2 | 1-3 | Active Utility | YES | `survival.isca_improvisada` | Lança isca curta que distrai criaturas orgânicas simples por poucos segundos. Em boss, não distrai; pode gerar pequena janela de atenção. Consome item simples se balance exigir. | isca no chão + alerta inimigo |
| Kit de Emergência | 3 | 1-3 | Active Utility | YES | `survival.kit_emergencia` | Usa suprimento/kit para recuperar pequena Stamina ou reduzir cansaço/efeito leve fora de combate. Não substitui cura mágica. | ícone kit + recuperação pequena |
| Instinto de Sobrevivência | 3 | 1-3 | Active Utility | YES | `survival.instinto_sobrevivencia` | Revela brevemente recursos, perigos leves, entradas ou objetos interagíveis próximos. Não revela segredos narrativos críticos. | pulso sensorial no chão |
| Campo Seguro | 4 | 1-3 | Active Utility | YES | `survival.campo_seguro` | Cria zona curta fora de combate que reduz ganho de cansaço/fome por poucos segundos e melhora recovery leve. Não funciona em combate direto. | círculo discreto + brisa |

### Total esperado

```text
Survival active slot utility skills = 5
```

---

## 7.5 Crafting / Produção

Crafting precisa ter utilities equipáveis para o jogador sentir a árvore no gameplay, sem virar árvore de combate principal.

### Skills existentes que continuam

As skills existentes de Crafting continuam como passive/unlock/modifier/crafting.

### Novas action skills adicionadas

Adicionar sem remover nada:

| Skill | Tier | Ranks | Tipo | Active slot? | EffectId | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---:|---|---|---|
| Reparo Rápido | 2 | 1-3 | Active Utility | YES | `crafting.reparo_rapido` | Repara pequena durabilidade de ferramenta/arma equipada ou reduz desgaste temporariamente. Consome material simples ou carga, se balance exigir. Não substitui bancada final. | faíscas leves + ícone reparo |
| Irrigador Portátil | 3 | 1-3 | Active Utility | YES | `crafting.irrigador_portatil` | Rega pequeno grupo de crop plots próximos ou alvo selecionado. Consome Stamina/água/carga conforme runtime disponível. | gotas em área pequena |
| Bomba Improvisada | 3 | 1-3 | Active Utility | YES | `crafting.bomba_improvisada` | Arremessa bomba leve craftada. Dano baixo/moderado, mais útil para stagger, rochas frágeis ou swarms pequenos. Consome item/carga. | explosão pequena |
| Mecanismo de Campo | 4 | 1-3 | Active Utility | YES | `crafting.mecanismo_campo` | Coloca dispositivo temporário simples: puxa/agrega item próximo, ativa mecanismo leve ou ajuda em interação técnica. Efeito exato depende do target. | engrenagem no chão |
| Marca de Eficiência | 4 | 1-3 | Active Utility | YES | `crafting.marca_eficiencia` | Buff curto em área de trabalho: reduz custo de Stamina de ações agrícolas/crafting próximas por poucos segundos. Não acumula consigo mesmo. | runa técnica/engrenagem |

### Total esperado

```text
Crafting active slot utility skills = 5
```

---

# 8. Mapeamento obrigatório de slots ativos

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_SLOT_MAPPING.md
```

Template:

```md
# WAVE INTEGRATION 11 — Skill Action Slot Mapping

## Status
COMPLETE / PARTIAL / BLOCKED

## Rules

- Active slots count: 4
- Dash uses: Space + direction
- Dodge uses: double tap directional
- Block uses: Left Shift
- Dash/Dodge/Block do not occupy active slots

## Active slot eligible skills

| Tree | SkillId | Skill name | EffectId | Slot eligible | TargetType | Cost | Cooldown | ImplementNow | DeferredReason |
|---|---|---|---|---:|---|---|---|---:|---|

## Non-slot actions

| Action | Input | Source | Uses active slot? | Runtime system | Notes |
|---|---|---|---:|---|---|
| Dash | Space + direction | canonical direction | NO | movement ability | costs Stamina |
| Dodge | double tap directional | canonical direction | NO | movement ability | costs Stamina |
| Block | Left Shift | canonical direction | NO | defensive action | drains Stamina |
```

---

# 9. Passive/modifier mapping obrigatório

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_PASSIVE_MODIFIER_MAPPING.md
```

Template:

```md
# WAVE INTEGRATION 11 — Skill Passive/Modifier Mapping

## Passive and modifier skills

| Tree | SkillId | Skill name | Type | EffectId | Runtime hook target | ImplementNow | DeferredReason |
|---|---|---|---|---|---|---:|---|
```

Deve incluir:

```text
Passive
Modifier
Unlock
Crafting
Passive/Lore
Unlock/Modifier
Exclusive Capstone
Capstone
```

---

# 10. Movement action mapping obrigatório

Criar:

```text
docs/validation/WAVE_INTEGRATION_11_SKILL_MOVEMENT_ACTIONS_MAPPING.md
```

Template:

```md
# WAVE INTEGRATION 11 — Skill Movement Actions Mapping

## Non-slot movement/defensive actions

| Action | Base availability | Input | Skill tree interactions | Cost | Cooldown | Distance/rule | ImplementNow | Notes |
|---|---|---|---|---|---|---|---:|---|
| Dash | tutorial/progressão inicial | Space + direction | Survival/Passo de Impulso melhora | Stamina | TBD/refinement | base + cap global ~8 tiles |  |  |
| Dodge | desde o começo | double tap directional | Survival/Reflexo de Esquiva melhora | Stamina | TBD/refinement | X tiles from refinement |  |  |
| Block | desbloqueado/melhorado por Melee | Left Shift | Melee/Block e Guarda Firme melhoram | Stamina drain | n/a or TBD | frontal damage reduction |  |  |
```

Se custos finais estiverem pendentes, não inventar:

```text
COST_FINAL_PENDING
COOLDOWN_FINAL_PENDING
```

---

# 11. Ajuste obrigatório no Skill Effect Catalog

Atualizar `WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md` para incluir:

```text
1. todas as skills originais;
2. todas as action skills adicionadas neste patch;
3. Dash/Dodge/Block como non-slot movement/defensive actions;
4. status implement now/deferred;
5. EffectId para cada active slot skill;
6. EFFECT_UNSPECIFIED apenas se realmente faltar efeito no design;
7. SkillId estável para cada skill nova.
```

Formato de `SkillId` recomendado:

```text
melee.treinamento_marcial
melee.corte_amplo
melee.avanco_aco
ranged.marcador_presa
magic.projetil_arcano
survival.sinal_retirada
crafting.reparo_rapido
```

Regra:

```text
Não usar acento em ID.
Não usar display name como ID persistente.
Não usar índice.
Não trocar IDs depois.
```

---

# 12. Implementação mínima obrigatória da WAVE11 após este patch

A WAVE11 deve implementar no mínimo:

```text
1. catálogo completo;
2. mapping de active slot;
3. mapping passivo/modifier;
4. mapping Dash/Dodge/Block;
5. pipeline EffectId → executor;
6. pelo menos 1 active slot skill real executável;
7. pelo menos Dash ou Dodge funcional;
8. Block auditado e, se runtime permitir, funcional;
9. HUD/feedback mínimo.
```

Vertical slice recomendado:

```text
Active slot skill:
- `magic.projetil_arcano` ou `melee.corte_amplo` se combate target existir;
- se combate não existir, usar `crafting.irrigador_portatil` ou `survival.instinto_sobrevivencia`.

Movement:
- implementar Dash primeiro se input/movement permitir;
- implementar Dodge se double tap detector estiver seguro;
- se ambos forem bloqueados por distância/collision rule ausente, parar com blocker.
```

---

# 13. Dash/Dodge/Block: regras de integração

## 13.1 Dash

```text
Input: Space + direction.
Active slot: NO.
Cost: Stamina.
Distance: usar refinement; cap global de Dash longo até ~8 tiles.
Direction: facing/movement direction.
Collision: não atravessa sólidos.
Bounds: não sai da cena.
Survival/Passo de Impulso: melhora custo/cooldown/recovery/distância conforme rank.
```

Se distância base não estiver especificada:

```text
DASH_BASE_DISTANCE_PENDING
```

Mas não inventar valor final.

## 13.2 Dodge

```text
Input: double tap directional.
Active slot: NO.
Cost: Stamina.
Direction:
- double tap back → dodge para trás relativo ao facing;
- double tap left/right → dodge lateral;
- double tap forward só se design permitir.
Collision: não atravessa sólidos.
Bounds: não sai da cena.
Survival/Reflexo de Esquiva: melhora i-frame/recovery em valores pequenos.
```

Se janela de double tap não estiver especificada:

```text
DOUBLE_TAP_WINDOW_PENDING
```

Pode usar config temporária apenas com:

```text
TODO_INTEGRATION_NOT_FINAL
```

## 13.3 Block

```text
Input: Left Shift.
Active slot: NO.
Cost: Stamina drain.
Unlock: Melee/Block rank 1.
Effect:
- reduz dano frontal por rank;
- drena Stamina ao segurar e ao receber impacto;
- melhora com Guarda Firme.
```

Se combate ainda não existe:

```text
BLOCK_RUNTIME_DEFERRED_WITH_REASON
```

Mas o input/state/HUD pode ser preparado.

---

# 14. Acceptance Criteria adicionais

Adicionar estes ACs ao report da WAVE11:

| ID | Critério | Obrigatório |
|---|---|---|
| AC-ACTION-01 | Catálogo inclui todas as skills originais das 5 árvores | Sim |
| AC-ACTION-02 | Catálogo inclui novas action skills adicionadas neste patch | Sim |
| AC-ACTION-03 | Cada árvore tem meta de 5 active slot skills ou debt explícito | Sim |
| AC-ACTION-04 | Nenhuma skill existente foi removida | Sim |
| AC-ACTION-05 | Nenhum ID existente foi trocado | Sim |
| AC-ACTION-06 | Active slot skills têm EffectId | Sim |
| AC-ACTION-07 | Passives/modifiers/unlocks/capstones não entram indevidamente em active slot | Sim |
| AC-ACTION-08 | Dash/Dodge/Block não ocupam active slot | Sim |
| AC-ACTION-09 | Dash = Space + direção | Sim |
| AC-ACTION-10 | Dodge = double tap direcional | Sim |
| AC-ACTION-11 | Block = Left Shift | Sim |
| AC-ACTION-12 | Pelo menos 1 active skill executa efeito real ou bridge controlado | Sim |
| AC-ACTION-13 | Pelo menos Dash ou Dodge funciona no Play Mode ou blocker honesto existe | Sim |

---

# 15. Checklist humano adicional

Adicionar ao checklist da WAVE11:

```md
## Action Skill Balance Checklist

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open skill tree | All 5 trees visible or documented |  |  |
| Open Melee | 5 active slot skills are listed or deferred |  |  |
| Open Ranged | 5 active slot skills are listed or deferred |  |  |
| Open Magic | 5 active slot skills are listed or deferred |  |  |
| Open Survival | 5 active utility skills are listed or deferred |  |  |
| Open Crafting | 5 active utility skills are listed or deferred |  |  |
| Equip active skill | Skill appears in one of 4 slots |  |  |
| Try passive skill equip | Passive cannot be equipped |  |  |
| Use active skill | Effect fires or clear blocked reason appears |  |  |
| Press Space + direction | Dash attempts movement |  |  |
| Double tap direction | Dodge attempts movement |  |  |
| Hold Left Shift | Block state/input appears or combat debt is explicit |  |  |
| Verify Dash/Dodge/Block slots | None occupies active slot |  |  |
```

---

# 16. Prompt de execução recomendado

```text
Estamos no repo rafa210587/cindars_hope, branch dev.

A WAVE_INTEGRATION_10 já foi implementada. Não reexecutar a WAVE10.

Objetivo:
Aplicar o patch obrigatório da WAVE_INTEGRATION_11 para corrigir catálogo de skills/effects, balancear action skills entre árvores e garantir Dash/Dodge/Block como non-slot actions.

Leia primeiro:
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
- docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_REPORT.md
- docs/validation/WAVE_INTEGRATION_10_SKILL_AUTHORING_MODEL.md
- docs/project/CURRENT_STATE.md
- docs/specs/a_implementar/WAVE_INTEGRATION_11_skill_effects_gameplay_bridge.md
- este patch: WAVE_INTEGRATION_11 — Action Skill Balance Patch

Regras:
- Só adicionar; não remover skill existente.
- Não renomear skill existente.
- Não trocar IDs existentes.
- Não criar uma segunda skill tree.
- Não criar active slot runtime paralelo.
- Não refazer UI da WAVE10.
- Não colocar Dash/Dodge/Block em active slot.
- Não inventar custo/cooldown final quando estiver pendente.
- Não alterar Packages/ProjectSettings.
- Não marcar ACCEPTED.

Criar/atualizar:
- docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_BALANCE_ADDENDUM.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_SLOT_MAPPING.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_PASSIVE_MODIFIER_MAPPING.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_MOVEMENT_ACTIONS_MAPPING.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
- docs/validation/WAVE_INTEGRATION_11_DASH_DODGE_MOVEMENT_CONTRACT.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_DECISION.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md
- docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_AUTHORING_MODEL.md
- docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md
- docs/project/CURRENT_STATE.md

Adicionar action skills novas conforme patch:
Melee:
- Avanço de Aço
- Grito de Desafio
- Investida Quebra-Guarda

Magic:
- Chama Breve
- Rajada Gélida

Survival:
- Sinal de Retirada
- Isca Improvisada
- Kit de Emergência
- Instinto de Sobrevivência
- Campo Seguro

Crafting:
- Reparo Rápido
- Irrigador Portátil
- Bomba Improvisada
- Mecanismo de Campo
- Marca de Eficiência

Ranged:
- não adicionar obrigatoriamente; já possui 5 active slot skills.

Implementação mínima:
- catálogo completo das 5 árvores;
- active slot mapping;
- passive/modifier mapping;
- movement action mapping;
- EffectId para active skills;
- pipeline EffectId → executor;
- pelo menos 1 active skill executável;
- pelo menos Dash ou Dodge funcional se refinement suficiente existir;
- Block auditado e preparado/deferred se combate ainda não existir.

Validação:
- dotnet restore .\Assembly-CSharp.csproj
- dotnet build .\Assembly-CSharp.csproj --no-restore
- dotnet restore .\Assembly-CSharp-Editor.csproj
- dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
- .\tools\docs\validate_docs.ps1
- .\tools\docs\check_spec_quality.ps1, se existir

Commit:
git add apenas arquivos alterados explicitamente.
git commit -m "feat: rebalance wave integration 11 action skills and movement abilities"
git push origin dev

Resposta final obrigatória:
Status:
WAVE10 preserved:
Skills removed:
IDs changed:
Action balance addendum:
Melee active count:
Ranged active count:
Magic active count:
Survival active count:
Crafting active count:
Dash slot status:
Dodge slot status:
Block slot status:
Active skill implemented:
Dash implemented:
Dodge implemented:
Block implemented/deferred:
Assembly-CSharp:
Assembly-CSharp-Editor:
Docs validation:
Quality check:
Commit:
Pushed:
Can continue to WAVE12:
Human Play Mode validation needed:
```

---

## 17. Status esperado

Se tudo for aplicado corretamente:

```text
Status: BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
```

Aceitável porque a WAVE11 não precisa implementar todos os 25 efeitos finais agora.

Mas não aceitar:

```text
BUILD_VALIDATED_SCENE_WIRED
```

a menos que:

```text
1. catálogo esteja completo;
2. pelo menos 1 active skill funcione;
3. Dash ou Dodge funcione;
4. HUD/feedback funcione;
5. Play Mode checklist esteja executável.
```
