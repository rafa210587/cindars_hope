# refinamento_init_skill_trees_active_slots_respec_anya

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_skill_trees_active_slots_respec_anya_runtime.md`  
> **Objetivo:** completar skill trees, purchase, active slots, capstones, save/load e respec na Fonte de Anya.

---

## 1. Estado atual

A regra de SkillPoint foi estabilizada:

```text
+1 SkillPoint em níveis pares, começando no nível 2.
```

Existem dados/managers iniciais de skill tree, mas o runtime completo ainda não está concluído.

---

## 2. Gaps

- Active slots não estão plenamente conectados a `SkillActionSO`/`SpellDataSO`.
- Purchase de nodes ainda precisa validação completa de prerequisites/custo.
- Capstones não estão completos.
- Save/load de purchased nodes e active slots não está final.
- Respec na Fonte de Anya não está integrado.
- Não há UI de skill tree final.
- Não há validação de limite de 4 active slots.

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

- 1 SkillPoint a cada 2 níveis.
- 4 active slots.
- Active slots equipam actions/spells desbloqueados, não apenas nodeId.
- Capstone exige pré-requisitos finais da árvore.
- Respec só acontece na Fonte de Anya.
- Respec tem custo configurável ou política explícita.

### Save/load

Salvar:

```text
PurchasedNodeIds
EquippedActiveSkillIds por slot
SpentSkillPoints
LastRespecCost opcional
```

---

## 4. Arquivos prováveis

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

- [ ] SkillPoint rule permanece a cada 2 níveis.
- [ ] Purchase valida custo e prerequisites.
- [ ] 4 active slots existem e salvam/carregam.
- [ ] Active slot executa SkillActionSO/SpellDataSO desbloqueado.
- [ ] Capstones existem e bloqueiam corretamente.
- [ ] Respec só funciona na Fonte de Anya.
- [ ] UI mostra árvore, pontos, prerequisites e slots.

---

## 6. Validação

1. Subir level e ganhar SkillPoint em níveis pares.
2. Comprar node com/sem prerequisite.
3. Equipar skill em active slot.
4. Salvar/carregar purchased nodes e slots.
5. Fazer respec na Fonte de Anya.
6. Validar capstone bloqueado/desbloqueado.
