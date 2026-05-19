# Tasks — FASE9C Tools, Farm Actions e Combat Refinement

> **Feature:** FASE9C_TOOLS_FARM_COMBAT_REFINEMENT  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-100 — Tool contracts

### Escopo

Criar contratos de ferramenta sem alterar gameplay.

### Arquivos esperados

- `ToolType.cs`
- `ToolTier.cs`
- `ToolDataSO.cs`
- `ToolDatabaseSO.cs`
- `ToolRequirement.cs`
- `ToolActionResolver.cs`
- `ToolEquippedEvent.cs`
- `ToolActionResolvedEvent.cs`

### Critérios

- [ ] Compila.
- [ ] CreateAssetMenu funciona.
- [ ] Nenhuma cena alterada.
- [ ] Nenhum gameplay alterado.

---

## PR-101 — Tool assets MVP

### Escopo

Criar ferramentas básicas.

### Assets esperados

- Hoe Basic
- Sickle Basic
- Axe Basic
- Pickaxe Basic
- FishingRod Basic
- ToolDatabase

### Critérios

- [ ] ItemDatabase inclui itens de ferramenta.
- [ ] ToolDatabase inclui tools.
- [ ] Validator cobre ToolDatabase.

---

## PR-102 — EquipmentManager mínimo

### Escopo

Criar manager persistente para ferramenta/arma.

### Critérios

- [ ] Equipamento ativo aparece no HUD debug.
- [ ] Save/load preserva equipamento.
- [ ] Saves antigos carregam.

---

## PR-103 — Plantio com ferramenta

### Escopo

Migrar plantio para seed explícita e Hoe/fallback.

### Critérios

- [ ] FarmPlot não escolhe seed por ordem hardcoded.
- [ ] Com Hoe Basic planta normalmente.
- [ ] Sem Hoe aplica fallback definido.
- [ ] Evento de tool action publicado.

---

## PR-104 — Colheita com ferramenta

### Escopo

Aplicar Sickle e yield por tier.

### Critérios

- [ ] Sem Sickle: yield mínimo.
- [ ] Sickle Basic: comportamento atual.
- [ ] Tier maior: bônus configurável.

---

## PR-105 — Árvores com Axe/tier

### Escopo

TreeNode exige Axe para corte real.

### Critérios

- [ ] Sem Axe não incrementa HitsTaken.
- [ ] Sem Axe pode dar fallback mínimo se configurado.
- [ ] Axe Basic preserva comportamento atual.
- [ ] Tier maior altera ActionPower/yield.

---

## PR-106 — FishingSpot com ToolRequirement

### Escopo

Migrar pesca para ToolRequirement.

### Critérios

- [ ] FishingRod Basic pesca.
- [ ] Sem FishingRod não pesca.
- [ ] Sem item ID hardcoded no componente.

---

## PR-107 — Weapon contracts

### Escopo

Criar contratos de armas.

### Critérios

- [ ] WeaponType existe.
- [ ] WeaponDataSO existe.
- [ ] WeaponDatabaseSO existe.
- [ ] Eventos de weapon/attack existem.

---

## PR-108 — Weapon slot no EquipmentManager

### Escopo

Adicionar arma equipada.

### Critérios

- [ ] Save/load preserva arma.
- [ ] HUD debug mostra arma.
- [ ] Sem arma usa Unarmed.

---

## PR-109 — PlayerCombatController melee

### Escopo

Migrar ataque para dados de arma.

### Critérios

- [ ] J continua atacando.
- [ ] Unarmed replica soco atual.
- [ ] Melee usa WeaponDataSO.

---

## PR-110 — PlayerDodgeController

### Escopo

Adicionar esquiva.

### Critérios

- [ ] Space esquiva.
- [ ] A/D + Space esquiva lateral.
- [ ] S + Space esquiva para trás.
- [ ] Cooldown impede spam.
- [ ] Invulnerability curta funciona.

---

## PR-111 — RangedPhysical projectile

### Escopo

Adicionar projétil físico.

### Critérios

- [ ] Arma física à distância dispara projectile.
- [ ] Projectile acerta EnemyHealth.
- [ ] Usa DamageRequest.

---

## PR-112 — RangedMagic projectile

### Escopo

Adicionar projétil mágico.

### Critérios

- [ ] Arma mágica dispara projectile.
- [ ] Dano/range/cooldown vêm de WeaponDataSO.
- [ ] Custo temporário ou MP futuro respeitado.

---

## Smoke test final

- [ ] Plantar/colher/cortar/pescar com ferramentas.
- [ ] Testar fallback sem ferramenta.
- [ ] Equipar arma.
- [ ] Atacar melee.
- [ ] Usar dodge.
- [ ] Disparar ranged physical.
- [ ] Disparar ranged magic.
- [ ] Salvar/carregar.
- [ ] Console sem erro vermelho.
