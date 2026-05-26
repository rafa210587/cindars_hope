# SPEC 16 Validation Report — Skill Trees, Active Slots e Respec Anya
**Data:** 2026-05-26
**Branch:** `dev`
**Objetivo:** Implementar skill trees completas (55 nodes / 5 arvores), respec na Fonte de Anya e save/load

---

## Status Final

| Validação | Status | Observação |
|-----------|--------|------------|
| Código implementado | ✅ COMPLETO | Todos os sistemas criados/expandidos |
| Unity compile validation | ⏳ PENDENTE | Unity Editor aberto - executar ao fechar |
| ScanUnityLogs.ps1 | ⏳ PENDENTE | Aguarda compile pass |
| validate_docs.ps1 | ⚠️ FAIL PRÉ-EXISTENTE | Erros em specs 10/11/12 fora do escopo |
| Play Mode humano | ⏳ PENDENTE | Requer execução manual |

**SPEC 16: Implementado em código — compile validation pendente**

---

## Arquivos Criados

| Arquivo | Descrição |
|---------|-----------|
| `Assets/_Game/Scripts/Skills/SkillEnums.cs` | SkillNodeType, SkillCategory, SkillModifierType, SkillTreeId |
| `Assets/_Game/Scripts/Skills/SkillPassiveModifier.cs` | Modificador serializable (tipo + valor) |
| `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs` | REESCRITO: NodeType, SkillCategory, IsCapstone, Prerequisites, PassiveModifiers |
| `Assets/_Game/Scripts/Skills/SkillTreeDataSO.cs` | REESCRITO: CapstoneNodeId, Nodes list |
| `Assets/_Game/Scripts/Skills/SkillActionSO.cs` | EXPANDIDO: ChargeTime, LinePierce, Dash, Leap, Projectile fields |
| `Assets/_Game/Scripts/Skills/SkillTreeRegistrySO.cs` | Registry de SkillTreeDataSO |
| `Assets/_Game/Scripts/Skills/SkillNodeDatabaseSO.cs` | Registry de SkillNodeDataSO |
| `Assets/_Game/Scripts/Skills/SkillTreeSaveData.cs` | DTO: PurchasedNodeIds, ActiveSkillSlots, RespecCount |
| `Assets/_Game/Scripts/Core/Events/SkillTreeEvents.cs` | 14 novos eventos de skill tree |
| `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` | 55 nodes / 5 arvores gerados por código |
| `Assets/_Game/Scripts/Skills/SkillTreeState.cs` | Estado runtime: pontos, nodes, slots, respec |
| `Assets/_Game/Scripts/Skills/SkillPurchaseService.cs` | Validação e execução de compra de node |
| `Assets/_Game/Scripts/Skills/SkillRespecService.cs` | Full respec na Anya (1º gratuito, 250g padrão) |
| `Assets/_Game/Scripts/Skills/SkillTreeManager.cs` | REESCRITO: MonoBehaviour, orquestra serviços |
| `Assets/_Game/Scripts/Skills/SkillPassiveApplicator.cs` | Aplica/remove modificadores passivos |
| `Assets/_Game/Scripts/UI/Skills/SkillTreePanel.cs` | Modal com tecla K, 5 abas, purchase, equip |
| `Assets/_Game/Scripts/UI/Skills/SkillTreeInputHandler.cs` | Handler dedicado para tecla K |
| `Assets/_Game/Scripts/Save/Migrations/SaveV4ToV5Migration.cs` | Inicializa SkillTreeSaveData |

## Arquivos Modificados

| Arquivo | Modificação |
|---------|-------------|
| `Assets/_Game/Scripts/Player/DerivedStatsCalculator.cs` | Aceita IList<SkillPassiveModifier> |
| `Assets/_Game/Scripts/Skills/ActiveSkillSlots.cs` | Subscribers de eventos, validação EquippableSkill |
| `Assets/_Game/Scripts/UI/Locations/AnyaFountainMenu.cs` | Respec button habilitado, integrado SkillRespecService |
| `Assets/_Game/Scripts/Save/SaveData.cs` | Adicionado SkillTreeSaveData |
| `Assets/_Game/Scripts/Save/SaveManager.cs` | v5, CaptureSkillTreeSaveData, RestoreFromSaveData, SaveV4ToV5Migration |
| `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` | Expõe SkillTreeManager |
| `Assets/_Game/Scripts/UI/SkillTree/SkillTreePanel.cs` | Esvaziado (substituído por UI/Skills/SkillTreePanel.cs) |

---

## Catálogo de Skill Trees

### Melee (11 nodes — 10 comuns + 1 capstone)
- Passivas: iron_grip, guarded_stance, dual_wield_flow, two_handed_momentum, dodge_training
- Equipáveis: offhand_cut, guarded_block, battle_dash, leap_attack, whirl_cut
- Capstone: battle_rhythm

### Ranged (11 nodes — 10 comuns + 1 capstone)
- Passivas: steady_hand, long_sight, quick_nock, kiting_steps, projectile_tuning
- Equipáveis: charged_shot, line_piercer, multishot_fan, bleeding_arrow, marked_prey
- Capstone: eagle_focus

### Magic (11 nodes — 10 comuns + 1 capstone)
- Passivas: mana_well, quick_channel, arcane_edge, arcane_bolt_mastery
- Equipáveis: fire_spark, ice_bind, toxic_cloud, lightning_chain, elemental_ward, slowing_sigils
- Capstone: elemental_confluence

### Survival (11 nodes — 10 comuns + 1 capstone)
- Passivas: cave_lungs, hard_skin, low_rations, toxic_sense, cold_habit, heat_temper, status_recovery, safe_step
- Equipáveis: emergency_roll, last_breath
- Capstone: caveborn

### Crafting (11 nodes — 10 comuns + 1 capstone)
- Passivas: fast_hands, repair_care, material_eye, station_focus, pack_order, salvage_method, durable_finish, shop_sense
- Equipáveis: field_patch, quick_repair
- Capstone: master_artisan

**Total: 55 nodes confirmados**

---

## Regras Implementadas

| Regra | Status |
|-------|--------|
| SkillPoint a cada level par (começando no 2) | ✅ PlayerProgressionRules (pré-existente) |
| Compra válida: custo + prerequisites + level mínimo | ✅ SkillPurchaseService |
| Capstone exige 8 nodes na arvore + prerequisites | ✅ RequiredPurchasedNodesInTree=8 |
| PassiveSkill aplica ao comprar (sem equipar) | ✅ SkillPassiveApplicator |
| EquippableSkill atribui a slot R/T/Y/G | ✅ SkillTreeManager.TryAssignActiveSlot |
| Tecla K abre/fecha SkillTreePanel | ✅ SkillTreeInputHandler |
| Modal K respeita modal stack | ✅ ModalManager.PushModal |
| Respec apenas na Fonte de Anya | ✅ AnyaFountainMenu.OnRespecClicked |
| 1º respec gratuito; seguintes 250g | ✅ SkillRespecService |
| Respec não altera level/XP/inventory/equipment/cave | ✅ SkillTreeState.FullRespec (isolado) |
| Save/load PurchasedNodeIds, ActiveSlots, RespecCount | ✅ SkillTreeSaveData + SaveV4ToV5Migration |
| Slot inválido no load → warning + clear | ✅ SkillTreeManager.ValidateActiveSlots |
| Derived stats recalculados em purchase/respec/load | ✅ SkillDerivedStatsChangedEvent |

---

## Invariantes Anti-Regressão

| Invariante | Status |
|-----------|--------|
| Active slots R/T/Y/G da spec 12 | ✅ Preservados |
| Q/E fora de modal = mãos do player (spec 12) | ✅ Modal captura Q/E somente dentro dele |
| Fonte de Anya da spec 15 | ✅ Respec habilitado sem alterar respawn |
| Death/corpse da spec 15 | ✅ Não alterados |
| Modal stack | ✅ SkillTreePanel usa PushModal/PopModal |
| GameEventBus | ✅ Todos os eventos via GameEventBus.Publish |
| Nenhum GameObject.Find() / FindObjectOfType() | ✅ GameBootstrap.Instance usado |
| Save DTOs simples (sem Unity refs) | ✅ SkillTreeSaveData usa strings e ints |

---

## Próximos Passos

1. **Fechar Unity Editor**
2. Executar: `.\tools\unity\RunUnityCompileValidation.ps1`
3. Se PASS: executar `.\tools\unity\ScanUnityLogs.ps1`
4. Play Mode humano:
   - Subir para level 2 → validar +1 SkillPoint
   - Abrir K → navegar aba Melee → comprar melee_iron_grip
   - Comprar melee_offhand_cut → equipar em slot R
   - Respec gratuito na Fonte de Anya
   - Salvar/carregar → validar estado preservado
5. Iniciar SPEC 17 após compile PASS confirmado
