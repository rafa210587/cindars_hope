# Tasks â€” FASE9E Item Taxonomy, IDs e Regras de Item

> **Feature:** FASE9E_ITEM_TAXONOMY_IDS  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-148 â€” Item taxonomy contracts

### Escopo

Criar/expandir categorias, subtipo de consumÃ­vel e flags de item.

### CritÃ©rios

- [ ] ItemCategory contÃ©m categorias oficiais.
- [ ] ConsumableSubtype existe.
- [ ] ItemDataSO suporta flags novas.
- [ ] Defaults nÃ£o quebram itens existentes.

---

## PR-148.1 â€” Future ideas TODO

### Escopo

Registrar ideias futuras fora do MVP.

### CritÃ©rios

- [ ] ItemRarity registrado em `docs_old/FUTURE_IDEAS_TODO_v1.0.md`.
- [ ] Weapon advanced properties registradas como futuro.
- [ ] Food buffs avanÃ§ados registrados como futuro.

---

## PR-149 â€” ID prefix validation

### Escopo

Adicionar validaÃ§Ã£o de prefixo por categoria.

### CritÃ©rios

- [ ] Prefixo Seed validado.
- [ ] Prefixo Crop validado.
- [ ] Prefixo Consumable validado.
- [ ] Prefixo Tool/Weapon/Magic/Ammo validado.
- [ ] Prefixo Quest/Key validado.
- [ ] ID duplicado detectado.

---

## PR-150 â€” Stack rules validation

### Escopo

Validar stack/maxStack e preparar regras de split/drop.

### CritÃ©rios

- [ ] MaxStack <= 0 Ã© erro.
- [ ] IsStackable false com MaxStack > 1 Ã© warning/erro.
- [ ] IsStackable true com MaxStack == 1 Ã© warning.
- [ ] Drop com quantidade escolhida estÃ¡ previsto.
- [ ] Split par/Ã­mpar documentado no handoff.

---

## PR-151 â€” Shop/crafting item rules

### Escopo

Validar regras de venda, compra e crafting.

### CritÃ©rios

- [ ] Sellable sem preÃ§o detectado.
- [ ] Buyable sem preÃ§o detectado.
- [ ] Quest/Key sellable sem override detectado.
- [ ] DebugOnly em shop/crafting/loot normal detectado.
- [ ] Receita com item inexistente detectada.

---

## PR-152 â€” Equipment/hotbar category rules

### Escopo

Validar categorias permitidas por slot.

### CritÃ©rios

- [ ] Seed pode ir para hotbar.
- [ ] Consumable pode ir para hotbar.
- [ ] Quest usÃ¡vel pode ir para hotbar.
- [ ] RightHand pode receber Weapon quando permitido.
- [ ] Ammo sÃ³ vai para slot adequado.

---

## PR-153 â€” Item examples migration

### Escopo

Criar/migrar exemplos iniciais.

### CritÃ©rios

- [ ] 6 seeds definidas.
- [ ] 6 crops definidos.
- [ ] foods correspondentes definidos.
- [ ] tools por material documentadas/criadas conforme escopo.
- [ ] weapons por material documentadas/criadas conforme escopo.
- [ ] ammo elemental documentada/criada conforme escopo.
- [ ] magic items documentados/criados conforme escopo.

---

## PR-154 â€” Item taxonomy handoff

### Escopo

Documentar checklist para criaÃ§Ã£o de novo item.

### CritÃ©rios

- [ ] Handoff explica categoria.
- [ ] Handoff explica prefixo.
- [ ] Handoff explica flags.
- [ ] Handoff explica stack/drop/split.
- [ ] Handoff explica shop/crafting/equipment.

---

## Smoke test final

- [ ] Criar item de cada categoria.
- [ ] Validar prefixos.
- [ ] Stackar seed/ammo/consumable/fish atÃ© 99.
- [ ] Splitar stack par e Ã­mpar.
- [ ] Dropar quantidade escolhida.
- [ ] Tentar vender Quest/Key item.
- [ ] Equipar seed.
- [ ] Equipar food/consumable.
- [ ] Equipar weapon no RightHand quando permitido.
- [ ] Console sem erro vermelho.

