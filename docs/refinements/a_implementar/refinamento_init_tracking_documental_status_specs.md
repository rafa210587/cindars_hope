# refinamento_init_tracking_documental_status_specs

> **Status:** Refinamento inicial a implementar  
> **Origem:** validação pós-estabilização `review/stabilize-overnight-specs`  
> **Objetivo:** corrigir divergências de tracking entre código, specs, registries e reports.  
> **Spec futura sugerida:** `spec_tracking_documental_status_specs.md`

---

## 1. Contexto

Após a estabilização overnight, o código corrigiu parte do que a documentação ainda trata como pendente. Também há uma spec de estabilização ainda registrada como `A implementar`, embora a run já tenha sido executada e documentada.

Este refinamento existe para impedir que o time planeje próximas waves com base em status falso.

---

## 2. Estado atual observado

### Já feito

- `docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md` foi criado.
- `docs/IMPLEMENTATION_DELIVERY_20260523.md` foi rebaixado de `COMPLETE` para `PARTIAL`.
- `ItemCategory`, SkillPoints, EnemyDataSO e RecipeDataSO foram estabilizados.

### Divergências

- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` ainda lista `spec_stabilization_overnight_fase9c_to_fase9l_v1.md` como `A implementar`.
- `spec_progression_001_xp_level_atributos_parcial.md` ainda diz que SkillPoint a cada 2 níveis está pendente, mas isso já foi corrigido no código.
- `spec_inventory_001_inventario_itens_gold_e_stacks.md` está como `Implementado`, mas o sistema real é inventory por ID com uma stack agregada por item, não inventory final por slots.

---

## 3. Mudanças necessárias

### Arquivos a revisar

```text
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/IMPLEMENTATION_STATUS.md
docs/IMPLEMENTATION_DELIVERY_20260523.md
docs/specs/implementados/spec_progression_001_xp_level_atributos_parcial.md
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
docs/refinements/a_implementar/ref_futuro_map.md
docs/refinements/implementados/ref_implementados_map.md
PROJECT_LOG.md
```

### Ajustes esperados

1. Mover/reclassificar `spec_stabilization_overnight_fase9c_to_fase9l_v1.md` para implementada com status:

```text
Implementado em código/documentação — validação Unity pendente
```

2. Atualizar progression:

```text
SkillPoint a cada 2 níveis: implementado.
Pendentes: skill trees completas, active slots, capstones, respec Fonte de Anya.
```

3. Rebaixar inventory para:

```text
Implementado parcial — inventory por ID e stack única por item.
Pendentes: slots reais, múltiplas stacks, capacidade, UI final.
```

---

## 4. Definition of Done

- [ ] Registry futuro não lista specs já executadas como `A implementar`.
- [ ] Registry implementado descreve corretamente implementado, parcial e validação Unity pendente.
- [ ] Progression doc não diz que SkillPoint a cada 2 níveis está pendente.
- [ ] Inventory doc deixa claro que o MVP é por ID/stack agregada, não slots finais.
- [ ] `IMPLEMENTATION_STATUS.md` e `PROJECT_LOG.md` refletem a realidade.
- [ ] Não há contradição entre report, registry e specs.

---

## 5. Validação

```powershell
.\tools\docs\validate_docs.ps1
```

Também revisar manualmente:

```text
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/IMPLEMENTATION_STATUS.md
docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md
```
