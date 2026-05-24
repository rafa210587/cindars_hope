# refinamento_init_player_combat_weapons_spells_skill_actions

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_player_combat_weapons_spells_skill_actions_runtime.md`
> **Objetivo:** substituir o punch MVP por combate data-driven com armas, magia, skill actions, stamina/mana e active slots.

---

## 1. Estado atual

O combate atual do jogador Ã© MVP/debug:

```text
PlayerAttackController
- tecla J
- dano fixo
- range fixo
- cooldown fixo
- overlap circle
```

TambÃ©m existem schemas iniciais:

```text
WeaponDataSO
SpellDataSO
SkillActionSO
```

mas eles ainda nÃ£o dirigem o runtime principal.

---

## 2. Gaps

- Armas nÃ£o sÃ£o equipadas/consumidas pelo attack controller.
- WeaponDataSO nÃ£o define o ataque real.
- NÃ£o hÃ¡ mana.
- NÃ£o hÃ¡ spell caster.
- NÃ£o hÃ¡ skill action executor.
- NÃ£o hÃ¡ active slots reais conectados a SkillActionSO/SpellDataSO.
- NÃ£o hÃ¡ stamina cost/cooldown por aÃ§Ã£o.
- NÃ£o hÃ¡ bow/ammo, dual wield, heavy/light attack, block/parry/dodge.

---

## 3. Escopo esperado

### Runtime

Criar ou evoluir:

```text
PlayerCombatController
PlayerWeaponController
PlayerSpellCaster
SkillActionExecutor
CombatResourceManager opcional
```

### Dados

Usar oficialmente:

```text
WeaponDataSO
SpellDataSO
SkillActionSO
DamageType
StatusEffectSO
```

### AÃ§Ãµes mÃ­nimas

- light melee attack;
- heavy melee attack futuro;
- ranged/bow placeholder;
- spell cast bÃ¡sico;
- dodge/roll com stamina;
- block/parry futuro;
- active skill action por slot.

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Combat/PlayerAttackController.cs
Assets/_Game/Scripts/Combat/PlayerCombatController.cs
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs
Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs
Assets/_Game/Scripts/Skills/SkillTreeManager.cs
Assets/_Game/Scripts/UI/Hotbar/**
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
```

---

## 5. Definition of Done

- [ ] Ataque usa arma equipada ou fallback explÃ­cito.
- [ ] WeaponDataSO define dano/range/cooldown/stamina/damage type.
- [ ] SpellDataSO pode ser executado por caster.
- [ ] SkillActionSO pode ser equipado em active slot.
- [ ] Cooldown/stamina/mana impedem spam.
- [ ] Eventos de hit/cast/action sÃ£o publicados.
- [ ] Punch debug fica removido ou claramente isolado.

---

## 6. ValidaÃ§Ã£o

1. Equipar arma e atacar inimigo.
2. Trocar arma e validar diferenÃ§a de dano/range/cooldown.
3. Usar skill action em slot ativo.
4. Castar spell com recurso suficiente e insuficiente.
5. Validar cooldown e stamina.
6. Salvar/carregar active slots/equipamento se aplicÃ¡vel.
