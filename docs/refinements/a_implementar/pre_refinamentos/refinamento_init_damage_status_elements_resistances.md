# refinamento_init_damage_status_elements_resistances

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_damage_status_elements_resistances_runtime.md`
> **Objetivo:** evoluir fÃ³rmula MVP de dano para pipeline completo com tipos, resistÃªncias, fraquezas, status e efeitos temporais.

---

## 1. Estado atual

`DamageCalculator` calcula dano direto simples com:

```text
baseDamage
attributeBonus
typeMultiplier
```

Existem dados iniciais de status effect, mas ainda sem integraÃ§Ã£o completa com o pipeline de hit/damage.

---

## 2. Gaps

- NÃ£o hÃ¡ `DamageType` oficial completo.
- Defesa do inimigo ainda nÃ£o entra na fÃ³rmula principal.
- ResistÃªncia/fraqueza elemental nÃ£o estÃ¡ integrada.
- Status effect nÃ£o Ã© aplicado por weapon/spell/enemy hit.
- NÃ£o hÃ¡ tick de dano ao longo do tempo integrado ao combate.
- NÃ£o hÃ¡ imunidade, stack rule, refresh rule ou duraÃ§Ã£o clara.
- NÃ£o hÃ¡ eventos de status aplicado/removido.

---

## 3. Escopo esperado

### Dados

Criar/consolidar:

```text
DamageType
DamageProfile
ResistanceProfile
StatusEffectSO
StatusApplicationRule
```

### Runtime

Criar/evoluir:

```text
DamageCalculator
DamageRequest
DamageResult
StatusEffectManager
StatusEffectInstance
```

### FÃ³rmula mÃ­nima

```text
raw = base + attributeBonus + weaponBonus
mitigated = raw - defense
multiplied = mitigated * resistanceMultiplier
final = max(minDamage, rounded)
```

### Status

- Burn: dano ao longo do tempo.
- Poison: dano ao longo do tempo, possivelmente menor e mais longo.
- Freeze/Slow: modifica movimento.
- Stun: bloqueia aÃ§Ã£o por duraÃ§Ã£o curta.
- Bleed: dano fÃ­sico temporal.

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Combat/DamageCalculator.cs
Assets/_Game/Scripts/Combat/DamageRequest.cs
Assets/_Game/Scripts/Combat/DamageResult.cs
Assets/_Game/Scripts/Combat/StatusEffect/**
Assets/_Game/Scripts/Combat/EnemyHealth.cs
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs
```

---

## 5. Definition of Done

- [ ] DamageType Ã© usado por weapon/spell/enemy attack.
- [ ] Defesa e resistÃªncia alteram dano final.
- [ ] Status pode ser aplicado por hit.
- [ ] Status tem duraÃ§Ã£o/tick/stack rule.
- [ ] Eventos sÃ£o publicados para UI/VFX.
- [ ] Save/load trata status persistente ou define que status Ã© runtime-only.

---

## 6. ValidaÃ§Ã£o

1. Atacar inimigo com defesa 0 e defesa alta.
2. Usar dano elemental contra resistÃªncia/fraqueza.
3. Aplicar poison/burn e validar ticks.
4. Validar expiraÃ§Ã£o do status.
5. Validar que status nÃ£o quebra inimigo morto/desativado.
