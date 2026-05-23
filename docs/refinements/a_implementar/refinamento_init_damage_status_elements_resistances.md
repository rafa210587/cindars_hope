# refinamento_init_damage_status_elements_resistances

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_damage_status_elements_resistances_runtime.md`  
> **Objetivo:** evoluir fórmula MVP de dano para pipeline completo com tipos, resistências, fraquezas, status e efeitos temporais.

---

## 1. Estado atual

`DamageCalculator` calcula dano direto simples com:

```text
baseDamage
attributeBonus
typeMultiplier
```

Existem dados iniciais de status effect, mas ainda sem integração completa com o pipeline de hit/damage.

---

## 2. Gaps

- Não há `DamageType` oficial completo.
- Defesa do inimigo ainda não entra na fórmula principal.
- Resistência/fraqueza elemental não está integrada.
- Status effect não é aplicado por weapon/spell/enemy hit.
- Não há tick de dano ao longo do tempo integrado ao combate.
- Não há imunidade, stack rule, refresh rule ou duração clara.
- Não há eventos de status aplicado/removido.

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

### Fórmula mínima

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
- Stun: bloqueia ação por duração curta.
- Bleed: dano físico temporal.

---

## 4. Arquivos prováveis

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

- [ ] DamageType é usado por weapon/spell/enemy attack.
- [ ] Defesa e resistência alteram dano final.
- [ ] Status pode ser aplicado por hit.
- [ ] Status tem duração/tick/stack rule.
- [ ] Eventos são publicados para UI/VFX.
- [ ] Save/load trata status persistente ou define que status é runtime-only.

---

## 6. Validação

1. Atacar inimigo com defesa 0 e defesa alta.
2. Usar dano elemental contra resistência/fraqueza.
3. Aplicar poison/burn e validar ticks.
4. Validar expiração do status.
5. Validar que status não quebra inimigo morto/desativado.
