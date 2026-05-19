# SpecKit — FASE9C Tools, Farm Actions e Combat Refinement

> **Feature:** FASE9C_TOOLS_FARM_COMBAT_REFINEMENT  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`

---

## 1. User story

Como jogador de Cindar's Hope, quero que ferramentas e armas tenham função real no gameplay para plantar, colher, cortar árvores, pescar, lutar, esquivar e evoluir minha eficiência.

---

## 2. Objetivos funcionais

### O1 — Tool System

Criar sistema genérico de ferramenta com tipo, tier, eficiência, action power e fallback.

### O2 — EquipmentManager

Persistir ferramenta e arma equipadas por IDs estáveis.

### O3 — Farm actions com ferramenta

Plantio, colheita, árvores e pesca devem usar ferramenta correta ou fallback limitado.

### O4 — Weapon foundation

Criar base para armas melee, distância física e magia à distância.

### O5 — Dodge

Adicionar esquiva lateral/para trás com cooldown e janela curta de invulnerabilidade.

### O6 — Ranged e magic

Criar projétil físico e projétil mágico básico.

---

## 3. Non-goals

Fora de escopo:

- UI final de inventário;
- arte final;
- durabilidade de ferramenta;
- árvore de skills;
- combos complexos;
- procedural cave;
- balanceamento final;
- Input System migration completa.

---

## 4. Regras de negócio

### R1 — Ferramenta correta libera ação real

Ação de mundo deve consultar ferramenta equipada antes de progredir.

### R2 — Fallback sem ferramenta é limitado

Sem ferramenta correta, fallback pode existir, mas não substitui ação real.

### R3 — Árvore exige Axe para corte real

Sem Axe, a árvore não incrementa progresso de corte.

### R4 — Plantio não escolhe seed automaticamente

FarmPlot não deve escolher seed fixa por ordem hardcoded. Deve usar seed explícita/ativa.

### R5 — Colheita usa Sickle para eficiência

Sem Sickle, yield mínimo; com Sickle, yield normal ou bônus por tier.

### R6 — Pesca usa ToolRequirement

FishingSpot não deve depender de item ID hardcoded; deve usar regra genérica.

### R7 — Dodge bloqueia ataque/interação

Durante dodge, o jogador não deve atacar nem interagir.

---

## 5. Entidades funcionais

- `ToolType`
- `ToolTier`
- `ToolDataSO`
- `ToolDatabaseSO`
- `ToolRequirement`
- `ToolActionResolver`
- `EquipmentManager`
- `WeaponType`
- `WeaponDataSO`
- `WeaponDatabaseSO`
- `PlayerCombatController`
- `PlayerDodgeController`
- `ProjectileController`

---

## 6. Critérios de aceite

### CA1 — Tool contracts

`ToolDataSO` e `ToolRequirement` existem, compilam e podem ser criados via CreateAssetMenu.

### CA2 — Tool assets

Hoe, Sickle, Axe, Pickaxe e FishingRod básicos existem como dados.

### CA3 — EquipmentManager

Equipamento ativo é salvo/restaurado.

### CA4 — Plantio

Com Hoe Basic, plantio funciona. Sem Hoe, só fallback permitido.

### CA5 — Colheita

Com Sickle, colheita normal. Sem Sickle, yield mínimo.

### CA6 — Árvores

Sem Axe, árvore não progride corte real. Com Axe, comportamento atual é preservado ou melhorado.

### CA7 — Pesca

FishingSpot funciona com FishingRod via ToolRequirement.

### CA8 — Weapon contracts

WeaponDataSO suporta Melee, RangedPhysical e RangedMagic.

### CA9 — PlayerCombatController

Ataque desarmado mantém comportamento atual; arma equipada muda dano/range/cooldown.

### CA10 — Dodge

Space executa dodge com cooldown e invulnerabilidade curta.

### CA11 — Ranged physical

Arma física à distância dispara projétil que causa dano.

### CA12 — Ranged magic

Arma mágica dispara projétil mágico que causa dano.

---

## 7. Dependências

- `InventoryManager`
- `SaveManager`
- `GameBootstrap`
- `DebugHud`
- `FarmPlot`
- `TreeNode`
- `FishingSpot`
- `EnemyHealth`
- `DamageRequest`
- `GameEventBus`

---

## 8. Observabilidade MVP

HUD/logs devem mostrar:

- ferramenta equipada;
- arma equipada;
- ação bloqueada por ferramenta ausente;
- fallback usado;
- dodge iniciado/finalizado;
- ataque acertou/errou.

---

## 9. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md` estiver lido.
- Estado real em `dev` tiver sido validado.
