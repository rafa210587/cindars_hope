# refinamento_init_hunger_stamina_status_balance

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_hunger_stamina_status_balance.md`  
> **Objetivo:** evoluir fome MVP para sistema integrado de stamina, status, buffs/debuffs e balanceamento.

---

## 1. Estado atual

`HungerManager` reduz fome por movimento/dia, restaura fome por comida e aplica dano quando fome chega a zero.

Evidência:

```text
Assets/_Game/Scripts/Player/HungerManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md
```

---

## 2. Gaps

- Não há stamina real.
- Fome não afeta velocidade, stamina regen, dano ou defesa.
- Comidas não aplicam buffs/debuffs além de restaurar fome.
- Não há status de fome crítica com efeitos persistentes.
- Balanceamento de perda por ação ainda é simples.
- Não há UI final de hunger/stamina/status.

---

## 3. Escopo esperado

### Stamina

Criar:

```text
StaminaManager
StaminaChangedEvent
StaminaEmptyEvent
```

Ações devem consumir stamina:

- attack;
- dodge/roll;
- tool use;
- sprint futuro;
- heavy actions.

### Hunger integration

Fome deve influenciar:

- stamina max/regeneration;
- move speed quando crítica;
- eficácia de ações;
- dano por fome vazia com tick controlado.

### Food buffs

Expandir item/consumable data:

```text
HungerRestore
StaminaRestore
BuffDuration
StatusEffectId
```

### UI

- barra de hunger;
- barra de stamina;
- ícones/status simples;
- feedback de ação sem stamina.

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/Player/HungerManager.cs
Assets/_Game/Scripts/Player/StaminaManager.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Combat/StatusEffect/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 5. Definition of Done

- [ ] Stamina existe, salva/carrega e publica eventos.
- [ ] Tool use/attack consomem stamina.
- [ ] Fome crítica afeta pelo menos uma variável de gameplay.
- [ ] Comidas podem restaurar fome/stamina.
- [ ] UI debug ou final mostra fome/stamina.
- [ ] Balance é configurável via data.

---

## 6. Validação

1. Usar tool até stamina baixar.
2. Tentar ação sem stamina e validar bloqueio/feedback.
3. Ficar com fome crítica e validar efeito.
4. Comer item e validar restore/buff.
5. Salvar/carregar fome e stamina.
