# refinamento_init_hunger_stamina_status_balance

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_hunger_stamina_status_balance.md`
> **Objetivo:** evoluir fome MVP para sistema integrado de stamina, status, buffs/debuffs e balanceamento.

---

## 1. Estado atual

`HungerManager` reduz fome por movimento/dia, restaura fome por comida e aplica dano quando fome chega a zero.

EvidÃªncia:

```text
Assets/_Game/Scripts/Player/HungerManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md
```

---

## 2. Gaps

- NÃ£o hÃ¡ stamina real.
- Fome nÃ£o afeta velocidade, stamina regen, dano ou defesa.
- Comidas nÃ£o aplicam buffs/debuffs alÃ©m de restaurar fome.
- NÃ£o hÃ¡ status de fome crÃ­tica com efeitos persistentes.
- Balanceamento de perda por aÃ§Ã£o ainda Ã© simples.
- NÃ£o hÃ¡ UI final de hunger/stamina/status.

---

## 3. Escopo esperado

### Stamina

Criar:

```text
StaminaManager
StaminaChangedEvent
StaminaEmptyEvent
```

AÃ§Ãµes devem consumir stamina:

- attack;
- dodge/roll;
- tool use;
- sprint futuro;
- heavy actions.

### Hunger integration

Fome deve influenciar:

- stamina max/regeneration;
- move speed quando crÃ­tica;
- eficÃ¡cia de aÃ§Ãµes;
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
- Ã­cones/status simples;
- feedback de aÃ§Ã£o sem stamina.

---

## 4. Arquivos provÃ¡veis

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
- [ ] Fome crÃ­tica afeta pelo menos uma variÃ¡vel de gameplay.
- [ ] Comidas podem restaurar fome/stamina.
- [ ] UI debug ou final mostra fome/stamina.
- [ ] Balance Ã© configurÃ¡vel via data.

---

## 6. ValidaÃ§Ã£o

1. Usar tool atÃ© stamina baixar.
2. Tentar aÃ§Ã£o sem stamina e validar bloqueio/feedback.
3. Ficar com fome crÃ­tica e validar efeito.
4. Comer item e validar restore/buff.
5. Salvar/carregar fome e stamina.
