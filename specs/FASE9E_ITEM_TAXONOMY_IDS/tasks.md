# Tasks — FASE9E Item Taxonomy, IDs e Regras de Item

> **Feature:** FASE9E_ITEM_TAXONOMY_IDS  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-148 — Item taxonomy contracts

### Escopo

Criar/expandir categorias, subtipo de consumível e flags de item.

### Critérios

- [ ] ItemCategory contém categorias oficiais.
- [ ] ConsumableSubtype existe.
- [ ] ItemDataSO suporta flags novas.
- [ ] Defaults não quebram itens existentes.

---

## PR-148.1 — Future ideas TODO

### Escopo

Registrar ideias futuras fora do MVP.

### Critérios

- [ ] ItemRarity registrado em `docs_old/FUTURE_IDEAS_TODO_v1.0.md`.
- [ ] Weapon advanced properties registradas como futuro.
- [ ] Food buffs avançados registrados como futuro.

---

## PR-149 — ID prefix validation

### Escopo

Adicionar validação de prefixo por categoria.

### Critérios

- [ ] Prefixo Seed validado.
- [ ] Prefixo Crop validado.
- [ ] Prefixo Consumable validado.
- [ ] Prefixo Tool/Weapon/Magic/Ammo validado.
- [ ] Prefixo Quest/Key validado.
- [ ] ID duplicado detectado.

---

## PR-150 — Stack rules validation

### Escopo

Validar stack/maxStack e preparar regras de split/drop.

### Critérios

- [ ] MaxStack <= 0 é erro.
- [ ] IsStackable false com MaxStack > 1 é warning/erro.
- [ ] IsStackable true com MaxStack == 1 é warning.
- [ ] Drop com quantidade escolhida está previsto.
- [ ] Split par/ímpar documentado no handoff.

---

## PR-151 — Shop/crafting item rules

### Escopo

Validar regras de venda, compra e crafting.

### Critérios

- [ ] Sellable sem preço detectado.
- [ ] Buyable sem preço detectado.
- [ ] Quest/Key sellable sem override detectado.
- [ ] DebugOnly em shop/crafting/loot normal detectado.
- [ ] Receita com item inexistente detectada.

---

## PR-152 — Equipment/hotbar category rules

### Escopo

Validar categorias permitidas por slot.

### Critérios

- [ ] Seed pode ir para hotbar.
- [ ] Consumable pode ir para hotbar.
- [ ] Quest usável pode ir para hotbar.
- [ ] RightHand pode receber Weapon quando permitido.
- [ ] Ammo só vai para slot adequado.

---

## PR-153 — Item examples migration

### Escopo

Criar/migrar exemplos iniciais.

### Critérios

- [ ] 6 seeds definidas.
- [ ] 6 crops definidos.
- [ ] foods correspondentes definidos.
- [ ] tools por material documentadas/criadas conforme escopo.
- [ ] weapons por material documentadas/criadas conforme escopo.
- [ ] ammo elemental documentada/criada conforme escopo.
- [ ] magic items documentados/criados conforme escopo.

---

## PR-154 — Item taxonomy handoff

### Escopo

Documentar checklist para criação de novo item.

### Critérios

- [ ] Handoff explica categoria.
- [ ] Handoff explica prefixo.
- [ ] Handoff explica flags.
- [ ] Handoff explica stack/drop/split.
- [ ] Handoff explica shop/crafting/equipment.

---

## Smoke test final

- [ ] Criar item de cada categoria.
- [ ] Validar prefixos.
- [ ] Stackar seed/ammo/consumable/fish até 99.
- [ ] Splitar stack par e ímpar.
- [ ] Dropar quantidade escolhida.
- [ ] Tentar vender Quest/Key item.
- [ ] Equipar seed.
- [ ] Equipar food/consumable.
- [ ] Equipar weapon no RightHand quando permitido.
- [ ] Console sem erro vermelho.


