---
type: refinement
spec: spec_hunger_stamina_status_balance.md
status: implementado
phase: design
---

# Refinement: Hunger/Stamina Status Balance (Implementado)

## Resumo

Especifica integração de fome com stamina, ciclo dia/noite com pause-aware, e sistema básico de status effects. SaveData v3 com migration de v2.

## Escopo Implementado

### 1. GameTimeManager
- Ciclo dia/noite configurável (10 min dia, 5 min noite)
- GameTimeTickEvent a cada 1 segundo
- Pause-aware via ModalManager.HasActiveModal
- SaveData persistence com GameTimeSaveData

### 2. Stamina Regeneration com Hunger Tiers
- 4 tiers: Normal (1.0x), Fome (0.6x), Crítica (0.3x), Vazio (0.0x)
- Damaged mode: -2 stamina/s quando fome=0
- PlayerNeedsBalanceSO configurável

### 3. SaveData v3 Migration
- SaveV2ToV3Migration com Dictionary→List conversion
- Compatível com JsonUtility (sem Dictionary)
- Safe initialization para saves v2 legados

### 4. PlayerNeedsHUD Mínima
- Hunger bar, Stamina bar, Status effects text
- Event subscribers para HungerChangedEvent, StaminaChangedEvent
- Update() sincronizado

### 5. StatusEffectManager Básico
- Tracking de efeitos ativos
- Save/load com List<StatusEffectEntryData>
- GameTimeTickEvent para decay

## Pendências (Spec 17)

- Canvas consolidado com styling
- Painel de dificuldade
- HUD com fonts/spacing final

## Validação

- ✅ Compilação C# em batch mode
- ✅ SaveData v3 migration testável
- ✅ No breaking changes em code existente
- ✅ Compatível com systems dependentes (Craft, Combat, etc)

## Implementado por

Claude Code, 2026-05-24

## Próximo

SPEC 10 (Equipment Durability/Loot/Environment)
