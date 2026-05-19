# Plan — FASE9C Tools, Farm Actions e Combat Refinement

> **Feature:** FASE9C_TOOLS_FARM_COMBAT_REFINEMENT  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar em blocos pequenos:

1. contratos de ferramentas;
2. assets de ferramentas;
3. EquipmentManager mínimo;
4. integração com ações de farm/world;
5. contratos de armas;
6. PlayerCombatController;
7. dodge;
8. projéteis físico/mágico;
9. validação/handoff.

---

## 2. Arquitetura proposta

### Novas pastas

```text
Assets/_Game/Scripts/Tools/
Assets/_Game/Scripts/Tools/Data/
Assets/_Game/Scripts/Equipment/
Assets/_Game/Scripts/Combat/Weapons/
Assets/_Game/Scripts/Combat/Projectiles/
Assets/_Game/Data/Tools/
Assets/_Game/Data/Weapons/
```

### Componentes centrais

- `ToolActionResolver` puro.
- `EquipmentManager` persistente.
- `PlayerCombatController` data-driven.
- `PlayerDodgeController` isolado.
- `ProjectileController` reutilizável.

---

## 3. Integrações

### GameBootstrap

Adicionar `EquipmentManager` como manager persistente.

### SaveManager

Adicionar `EquipmentSaveData` opcional e backwards compatible.

### FarmPlot

Migrar plantio/colheita para ferramenta/seed explícita.

### TreeNode

Migrar corte para Axe/tier/fallback.

### FishingSpot

Migrar required tool ID para ToolRequirement.

### Combat

PlayerAttackController deve ser preservado como fallback ou substituído por PlayerCombatController com Unarmed equivalente.

---

## 4. Riscos

| Risco | Mitigação |
|---|---|
| Tool System grande demais | dividir PRs por contrato/assets/manager/integração |
| Quebrar farm loop atual | Hoe/Sickle/Axe Basic devem preservar comportamento atual |
| Save antigo quebrar | EquipmentSaveData opcional e tolerante a null |
| Dodge atravessar colisores | usar Rigidbody2D.MovePosition e teste manual |
| Projétil sem facing robusto | usar última direção de movimento no MVP |

---

## 5. Testes manuais mínimos

- Equipar ferramenta.
- Plantar com Hoe.
- Tentar plantar sem Hoe.
- Colher com Sickle.
- Cortar árvore com Axe.
- Tentar cortar sem Axe.
- Pescar com FishingRod.
- Atacar desarmado.
- Atacar com arma melee.
- Usar dodge.
- Disparar projétil físico.
- Disparar projétil mágico.
- Save/load com equipamento.

---

## 6. Fora de escopo técnico

- UI final.
- Arte final.
- Durabilidade.
- Skill tree.
- Balanceamento final.
- Animações finais.
