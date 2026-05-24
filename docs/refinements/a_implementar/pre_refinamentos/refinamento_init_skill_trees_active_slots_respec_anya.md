# refinamento_init_skill_trees_active_slots_respec_anya

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_skill_trees_active_slots_respec_anya_runtime.md`
> **Objetivo:** completar skill trees, purchase, active slots, capstones, save/load e respec na Fonte de Anya.

---

## 1. Estado atual

A regra de SkillPoint foi estabilizada:

```text
+1 SkillPoint em nÃ­veis pares, comeÃ§ando no nÃ­vel 2.
```

Existem dados/managers iniciais de skill tree, mas o runtime completo ainda nÃ£o estÃ¡ concluÃ­do.

---

## 2. Gaps

- Active slots nÃ£o estÃ£o plenamente conectados a `SkillActionSO`/`SpellDataSO`.
- Purchase de nodes ainda precisa validaÃ§Ã£o completa de prerequisites/custo.
- Capstones nÃ£o estÃ£o completos.
- Save/load de purchased nodes e active slots nÃ£o estÃ¡ final.
- Respec na Fonte de Anya nÃ£o estÃ¡ integrado.
- NÃ£o hÃ¡ UI de skill tree final.
- NÃ£o hÃ¡ validaÃ§Ã£o de limite de 4 active slots.

---

## 3. Escopo esperado

### Dados

```text
SkillTreeDataSO
SkillNodeDataSO
SkillNodeType
SkillActionSO
CapstoneNode
```

### Runtime

```text
SkillTreeManager
ActiveSkillSlotManager
SkillPurchaseService
SkillRespecService
```

Regras:

- 1 SkillPoint a cada 2 nÃ­veis.
- 4 active slots.
- Active slots equipam actions/spells desbloqueados, nÃ£o apenas nodeId.
- Capstone exige prÃ©-requisitos finais da Ã¡rvore.
- Respec sÃ³ acontece na Fonte de Anya.
- Respec tem custo configurÃ¡vel ou polÃ­tica explÃ­cita.

### Save/load

Salvar:

```text
PurchasedNodeIds
EquippedActiveSkillIds por slot
SpentSkillPoints
LastRespecCost opcional
```

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Skills/SkillTreeManager.cs
Assets/_Game/Scripts/Skills/SkillTreeDataSO.cs
Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs
Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/UI/Skills/**
Assets/_Game/Scripts/Cave/Anya/** ou SceneManagement/FonteAnya
```

---

## 5. Definition of Done

- [ ] SkillPoint rule permanece a cada 2 nÃ­veis.
- [ ] Purchase valida custo e prerequisites.
- [ ] 4 active slots existem e salvam/carregam.
- [ ] Active slot executa SkillActionSO/SpellDataSO desbloqueado.
- [ ] Capstones existem e bloqueiam corretamente.
- [ ] Respec sÃ³ funciona na Fonte de Anya.
- [ ] UI mostra Ã¡rvore, pontos, prerequisites e slots.

---

## 6. ValidaÃ§Ã£o

1. Subir level e ganhar SkillPoint em nÃ­veis pares.
2. Comprar node com/sem prerequisite.
3. Equipar skill em active slot.
4. Salvar/carregar purchased nodes e slots.
5. Fazer respec na Fonte de Anya.
6. Validar capstone bloqueado/desbloqueado.
