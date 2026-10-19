# SPEC 12 - Player Combat, Weapons, Spells, Skill Actions Runtime

**Status:** Implementado parcial (2026-05-25)

**Entrega completa:**
- ✅ Combate player data-driven (Q/E attacks, dodge)
- ✅ Prioridade de interação preservada em E
- ✅ Unarmed fallback com UnarmedAttackDataSO
- ✅ Dodge simples com stamina (Space, sem i-frames)
- ✅ Bow/Projectile system placeholder
- ✅ Spell casting com mana (SpellDatabaseSO)
- ✅ Active skill slots R/T/Y/G
- ✅ Save/load de mana e active slots
- ✅ Integração com DamageCalculator (SPEC 11)
- ✅ Integração com Equipment slots (SPEC 10)
- ✅ Integração com Stamina (SPEC 09)
- ✅ Todos eventos publicados
- ✅ HUD mínima de mana

## Implementação Técnica

### Controllers
- **PlayerAttackController.cs** - Q/E/Space handlers com event publishing
- **PlayerSpellCaster.cs** - Spell casting via SpellDatabaseSO + GameBootstrap
- **SkillActionExecutor.cs** - Executa skills com validação de stamina/mana
- **ActiveSkillSlots.cs** - 4 skill slots com cooldown per slot

### Data-Driven Systems
- **WeaponDataSO** - Armas com projectile support
- **SpellDataSO** - Spells com mana/cooldown/range
- **UnarmedAttackDataSO** - Fallback unarmed
- **SkillActionSO** - Skill actions (DamageSkill, SelfBuffSkill, LinkedSpellSkill)

### New Systems
- **ProjectileBehaviour.cs** - Projectile movement, hit detection, DamageCalculator integration

### Registries (via GameBootstrap)
- **WeaponDatabaseSO** - Registry<WeaponDataSO>
- **SpellDatabaseSO** - Registry<SpellDataSO>
- **SkillActionDatabaseSO** - Registry<SkillActionSO>

### Integration Points
- **ManaManager** - Integrado em SaveManager capture/restore
- **SaveManager** - Mana capture + ActiveSkillSlots restore
- **InteractionSystem** - E key respects HasCandidate flag
- **GameBootstrap** - Registries injetadas

### Events Published
- PlayerDodgeStartedEvent ✅
- PlayerDodgeEndedEvent ✅
- SpellCastStartedEvent ✅
- SpellCastSucceededEvent ✅
- SpellCastFailedEvent ✅
- SkillActionExecutedEvent ✅
- ActiveSkillSlotChangedEvent ✅
- ManaChangedEvent ✅

## Validação de Escopo

✅ Punch hardcoded removido
✅ Unarmed fallback data-driven
✅ Q = LeftHand attack
✅ E = RightHand (com prioridade de interação)
✅ Space = Dodge sem i-frames
✅ Bow sem ammo, range 6.0
✅ Spell basica testavel
✅ Mana inicia 100, regenera 5/s
✅ Active skill slots R/T/Y/G
✅ AttackSpeed reduz cooldown
✅ Dano passa por SPEC 11 pipeline
✅ Save/load completo

## Gaps Deferred

- **Play Mode testing** - User validará todos critérios
- **UI consolidada** - SPEC 17
- **Block/Parry** - Out of scope
- **Heavy attack** - Hook futuro
- **Ammo system** - Hook futuro
- **I-frames** - Out of scope MVP
- **Skill tree** - SPEC 16

## Data da Conclusão

2026-05-25  
Implementador: Claude (Session 14)  
Status Final: Awaiting Play Mode testing
