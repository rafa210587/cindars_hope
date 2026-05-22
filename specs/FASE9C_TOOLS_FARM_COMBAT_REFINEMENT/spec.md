# SpecKit â€” FASE9C Tools, Farm Actions e Combat Refinement

> **Feature:** FASE9C_TOOLS_FARM_COMBAT_REFINEMENT  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`

---

## 1. User story

Como jogador de Cindar's Hope, quero que ferramentas e armas tenham funÃ§Ã£o real no gameplay para plantar, colher, cortar Ã¡rvores, pescar, lutar, esquivar e evoluir minha eficiÃªncia.

---

## 2. Objetivos funcionais

### O1 â€” Tool System

Criar sistema genÃ©rico de ferramenta com tipo, tier, eficiÃªncia, action power e fallback.

### O2 â€” EquipmentManager

Persistir ferramenta e arma equipadas por IDs estÃ¡veis.

### O3 â€” Farm actions com ferramenta

Plantio, colheita, Ã¡rvores e pesca devem usar ferramenta correta ou fallback limitado.

### O4 â€” Weapon foundation

Criar base para armas melee, distÃ¢ncia fÃ­sica e magia Ã  distÃ¢ncia.

### O5 â€” Dodge

Adicionar esquiva lateral/para trÃ¡s com cooldown e janela curta de invulnerabilidade.

### O6 â€” Ranged e magic

Criar projÃ©til fÃ­sico e projÃ©til mÃ¡gico bÃ¡sico.

---

## 3. Non-goals

Fora de escopo:

- UI final de inventÃ¡rio;
- arte final;
- durabilidade de ferramenta;
- Ã¡rvore de skills;
- combos complexos;
- procedural cave;
- balanceamento final;
- Input System migration completa.

---

## 4. Regras de negÃ³cio

### R1 â€” Ferramenta correta libera aÃ§Ã£o real

AÃ§Ã£o de mundo deve consultar ferramenta equipada antes de progredir.

### R2 â€” Fallback sem ferramenta Ã© limitado

Sem ferramenta correta, fallback pode existir, mas nÃ£o substitui aÃ§Ã£o real.

### R3 â€” Ãrvore exige Axe para corte real

Sem Axe, a Ã¡rvore nÃ£o incrementa progresso de corte.

### R4 â€” Plantio nÃ£o escolhe seed automaticamente

FarmPlot nÃ£o deve escolher seed fixa por ordem hardcoded. Deve usar seed explÃ­cita/ativa.

### R5 â€” Colheita usa Sickle para eficiÃªncia

Sem Sickle, yield mÃ­nimo; com Sickle, yield normal ou bÃ´nus por tier.

### R6 â€” Pesca usa ToolRequirement

FishingSpot nÃ£o deve depender de item ID hardcoded; deve usar regra genÃ©rica.

### R7 â€” Dodge bloqueia ataque/interaÃ§Ã£o

Durante dodge, o jogador nÃ£o deve atacar nem interagir.

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

## 6. CritÃ©rios de aceite

### CA1 â€” Tool contracts

`ToolDataSO` e `ToolRequirement` existem, compilam e podem ser criados via CreateAssetMenu.

### CA2 â€” Tool assets

Hoe, Sickle, Axe, Pickaxe e FishingRod bÃ¡sicos existem como dados.

### CA3 â€” EquipmentManager

Equipamento ativo Ã© salvo/restaurado.

### CA4 â€” Plantio

Com Hoe Basic, plantio funciona. Sem Hoe, sÃ³ fallback permitido.

### CA5 â€” Colheita

Com Sickle, colheita normal. Sem Sickle, yield mÃ­nimo.

### CA6 â€” Ãrvores

Sem Axe, Ã¡rvore nÃ£o progride corte real. Com Axe, comportamento atual Ã© preservado ou melhorado.

### CA7 â€” Pesca

FishingSpot funciona com FishingRod via ToolRequirement.

### CA8 â€” Weapon contracts

WeaponDataSO suporta Melee, RangedPhysical e RangedMagic.

### CA9 â€” PlayerCombatController

Ataque desarmado mantÃ©m comportamento atual; arma equipada muda dano/range/cooldown.

### CA10 â€” Dodge

Space executa dodge com cooldown e invulnerabilidade curta.

### CA11 â€” Ranged physical

Arma fÃ­sica Ã  distÃ¢ncia dispara projÃ©til que causa dano.

### CA12 â€” Ranged magic

Arma mÃ¡gica dispara projÃ©til mÃ¡gico que causa dano.

---

## 7. DependÃªncias

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
- aÃ§Ã£o bloqueada por ferramenta ausente;
- fallback usado;
- dodge iniciado/finalizado;
- ataque acertou/errou.

---

## 9. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md` estiver lido.
- Estado real em `dev` tiver sido validado.

