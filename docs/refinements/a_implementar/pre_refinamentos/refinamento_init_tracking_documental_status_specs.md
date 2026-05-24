# refinamento_init_tracking_documental_status_specs

> **Status:** Refinamento inicial a implementar
> **Origem:** validaÃ§Ã£o pÃ³s-estabilizaÃ§Ã£o `review/stabilize-overnight-specs`
> **Objetivo:** corrigir divergÃªncias de tracking entre cÃ³digo, specs, registries e reports.
> **Spec futura sugerida:** `spec_tracking_documental_status_specs.md`

---

## 1. Contexto

ApÃ³s a estabilizaÃ§Ã£o overnight, o cÃ³digo corrigiu parte do que a documentaÃ§Ã£o ainda trata como pendente. TambÃ©m hÃ¡ uma spec de estabilizaÃ§Ã£o ainda registrada como `A implementar`, embora a run jÃ¡ tenha sido executada e documentada.

Este refinamento existe para impedir que o time planeje prÃ³ximas waves com base em status falso.

---

## 2. Estado atual observado

### JÃ¡ feito

- `docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md` foi criado.
- `docs/IMPLEMENTATION_DELIVERY_20260523.md` foi rebaixado de `COMPLETE` para `PARTIAL`.
- `ItemCategory`, SkillPoints, EnemyDataSO e RecipeDataSO foram estabilizados.

### DivergÃªncias

- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` ainda lista `spec_stabilization_overnight_fase9c_to_fase9l_v1.md` como `A implementar`.
- `spec_progression_001_xp_level_atributos_parcial.md` ainda diz que SkillPoint a cada 2 nÃ­veis estÃ¡ pendente, mas isso jÃ¡ foi corrigido no cÃ³digo.
- `spec_inventory_001_inventario_itens_gold_e_stacks.md` estÃ¡ como `Implementado`, mas o sistema real Ã© inventory por ID com uma stack agregada por item, nÃ£o inventory final por slots.

---

## 3. MudanÃ§as necessÃ¡rias

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
Implementado em cÃ³digo/documentaÃ§Ã£o â€” validaÃ§Ã£o Unity pendente
```

2. Atualizar progression:

```text
SkillPoint a cada 2 nÃ­veis: implementado.
Pendentes: skill trees completas, active slots, capstones, respec Fonte de Anya.
```

3. Rebaixar inventory para:

```text
Implementado parcial â€” inventory por ID e stack Ãºnica por item.
Pendentes: slots reais, mÃºltiplas stacks, capacidade, UI final.
```

---

## 4. Definition of Done

- [ ] Registry futuro nÃ£o lista specs jÃ¡ executadas como `A implementar`.
- [ ] Registry implementado descreve corretamente implementado, parcial e validaÃ§Ã£o Unity pendente.
- [ ] Progression doc nÃ£o diz que SkillPoint a cada 2 nÃ­veis estÃ¡ pendente.
- [ ] Inventory doc deixa claro que o MVP Ã© por ID/stack agregada, nÃ£o slots finais.
- [ ] `IMPLEMENTATION_STATUS.md` e `PROJECT_LOG.md` refletem a realidade.
- [ ] NÃ£o hÃ¡ contradiÃ§Ã£o entre report, registry e specs.

---

## 5. ValidaÃ§Ã£o

```powershell
.\tools\docs\validate_docs.ps1
```

TambÃ©m revisar manualmente:

```text
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/IMPLEMENTATION_STATUS.md
docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md
```
