# Cindar's Hope — Implementation Status

> Fonte curta e verificável de capacidades implementadas, specs concluídas e próximos blocos pendentes.  
> Atualizar ao final de cada PR relevante.  
> `PROJECT_LOG.md` continua sendo o log operacional/histórico.

---

## 1. Resumo executivo

| Área | Status | Observação |
|---|---|---|
| Core/EventBus | Implementado | `GameEventBus` e eventos base criados. |
| Data/Registries | Implementado | Contratos identificáveis e registries por ID. |
| Farm Loop | Implementado MVP | Plantar, avançar dia, crescer e colher. |
| Economy | Implementado parcial | Compra/venda básica por pontos/eventos; UI final pendente. |
| Hunger | Implementado parcial | Fome MVP/debug, perda por passos/dia e consumo de comida. |
| Save/Load | Implementado parcial | JSON local, cena atual, rebind cross-scene e pickups persistentes. |
| World Activities | Implementado parcial | Árvores, pesca básica e pickups persistentes. |
| Crafting | Implementado MVP | Contratos, receita de madeira processada, `CraftingManager` e `CraftingPoint`. |
| Town | Implementado MVP | Portal Farm/Town, TownScene gerável, NPC Pip placeholder e comércio básico. |
| Cave | Implementado MVP básico | CaveScene/portal/combat básico conforme log; procedural progressivo pendente. |
| Combat | Implementado MVP básico | Slime, melee/contact damage, drops e feedback básico conforme log. |
| UI | Debug apenas | `DebugHud`/OnGUI para validação; UI final pendente. |
| Equipment/Hotbar | Especificado | FASE9E pendente de implementação. |
| Progression/LevelUp | Especificado | FASE9E pendente de implementação. |
| Cave Procedural/Resources | Especificado | FASE9F pronta; próximo bloco recomendado PR-170+. |
| Cave Bestiary/Faction Locks | Especificado | FASE9G pronta; depende da base FASE9F para implementação real. |
| Reconciliação pós PR-099 | Implementado parcial | PR-100 auditou estado real; PR-101+ pendentes. |

---

## 2. Specs/PRs implementados

| Bloco | PRs/Wave | Status | Evidência principal | Pendências |
|---|---:|---|---|---|
| Fase 8 Core Foundation | PR-001 a PR-012 | Implementado | Core events, managers, interaction base | Validar Unity completo. |
| Fase 8 Farm Loop | PR-013 a PR-017 | Implementado MVP | `FarmPlot`, `TimeManager`, growth/harvest | UI final de plantio pendente. |
| Fase 8 Economy/Hunger/HUD | PR-018 a PR-024 | Implementado parcial | `DebugHud`, `SellPoint`, `HungerManager`, `FoodConsumer` | UI final pendente. |
| Fase 8 Save/Load | PR-025 a PR-030 | Implementado parcial | `SaveManager`, `SaveData`, registries de cena | Validar Play Mode completo. |
| Fase 8 World/Shop/Hardening | PR-031 a PR-045 | Implementado parcial | `TreeNode`, `FishingSpot`, `SeedShopPoint`, `ItemPickup` | Validar assets/cena no Unity. |
| Fix pickups persistentes | Pós PR-045 | Implementado | `ItemPickupRegistry`, `ItemPickupSaveData` | Validar F5/F9 no Unity. |
| Fase 9A Crafting | PR-046 a PR-052 | Implementado MVP | `CraftingManager`, `CraftingPoint`, receita madeira processada | UI/fila/receitas completas pendentes. |
| Fase 9A Town | PR-053 a PR-063 | Implementado MVP | Scene portals, TownScene generator, Pip, shop points | UI final de loja/diálogo pendente. |
| Fase 9A Save Cross-Scene Hardening | PR-065+ | Implementado parcial | rebind/cache/save scene | Smoke test cross-scene pendente. |
| Fase 9B Cave/Combat MVP | Log atual | Implementado MVP básico | CaveScene, portal, Slime/combat básico conforme `PROJECT_LOG.md` | Procedural/resources/checkpoints pendentes. |
| Fase 9E Specs | Docs/specs | Especificado | UI/Hotbar, Damage/Status, Item Taxonomy, Save Migration, Level Up | Implementação pendente. |
| Fase 9F Specs | Docs/specs | Especificado | Cave procedural/resources/encounters spec | Próximo bloco PR-170+. |
| Fase 9G Specs | Docs/specs | Especificado | Cave bestiary/faction locks/portal ecology spec | Depende de FASE9F procedural foundation. |
| PR-100 pós PR-099 | PR-100 | Implementado | `docs/audits/PR100_POST_PR099_REPO_AUDIT.md` | PR-101 deve reconciliar branches/fixes sem merge automático. |

---

## 3. Capacidades jogáveis hoje

- Iniciar na FarmScene.
- Mover jogador.
- Interagir com objetos por tecla E.
- Plantar sementes em plots.
- Avançar dia.
- Crescer crops por dia.
- Colher crops.
- Ver HUD debug com estado básico.
- Vender itens vendáveis.
- Comprar sementes básicas.
- Perder fome por movimento/dia.
- Consumir comida para restaurar fome.
- Salvar/carregar JSON local.
- Persistir plots, árvores, pickups, inventário, ouro, fome, HP, dia e posição conforme MVP.
- Transitar FarmScene ↔ TownScene.
- Interagir com NPC Pip placeholder.
- Comprar/vender em pontos de comércio na cidade.
- Craftar madeira processada no ponto de carpintaria.
- Cortar árvore para obter madeira.
- Pescar peixe comum.
- Coletar pickup persistente.
- Entrar/sair da CaveScene conforme MVP básico.
- Enfrentar Slime básico conforme MVP básico.
- Receber drops básicos conforme MVP básico.

Observação: várias capacidades ainda são MVP/debug, não versão final de UX, arte, balanceamento ou UI.

---

## 4. Capacidades pendentes

### 4.1 Validação/hardening pendente

- Rodar `CindarsHope/Validate/Validate MVP Data`.
- Rodar `CindarsHope/Scenes/Create MVP FarmScene`.
- Rodar `CindarsHope/Scenes/Create MVP TownScene`.
- Rodar `CindarsHope/Scenes/Create MVP CaveScene`.
- Testar Play Mode Farm → Town → Cave → Farm.
- Testar save/load em FarmScene.
- Testar save/load em TownScene.
- Testar save/load em CaveScene.
- Confirmar Console sem erro vermelho.
- Confirmar HUD não duplica após transições.
- Confirmar referências runtime são rebindadas após troca de cena.

### 4.2 FASE9E pendente

- UI/Hotbar/Inventory/Equipment.
- Damage/Elementos/Status/Fórmula única.
- Item Taxonomy/IDs.
- Item examples/variations.
- Save Schema/Migration.
- Player Level Up/Progression.

### 4.3 Reconciliação pós PR-099 pendente

- PR-101 — Reconciliar branches/fixes PR-099.
- PR-102 — Corrigir compile/hardening do EnemyHealth.
- PR-103 — Corrigir PlayerAttackController data-driven.
- PR-104 — Corrigir EnemyContactDamage data-driven.
- PR-105 — Corrigir EnemyChaseController data-driven.
- PR-106 — Corrigir HitFlash e Knockback.
- PR-107 — Corrigir EnemyDropSpawner.
- PR-108 — Validar EnemyDataSO e asset do Slime.
- PR-109 — Corrigir CreateMvpCaveScene combat wiring.
- PR-110 — Corrigir CaveSceneRuntimeReferenceInstaller.
- PR-111 — Corrigir DebugHud singleton cross-scene.
- PR-112 — Corrigir SaveManager para CaveScene.
- PR-113 — Consolidar validator Farm/Town/Cave.
- PR-114 — Regenerar cenas Farm/Town/Cave.
- PR-115 — Smoke test documentado Farm/Town/Cave.
- PR-116 — Inventory/Item taxonomy audit.
- PR-117 — Padronizar IDs cave/combat temporários.
- PR-118 — Preparar Tool contracts mínimos.
- PR-119 — EquipmentManager mínimo.
- PR-120 — Seed/Tool selection debug mínimo.
- PR-121 — Hotbar contracts sem UI final.
- PR-122 — Inventory UI debug melhorado.
- PR-123 — Damage formula MVP.
- PR-124 — Integrar DamageCalculator ao melee.
- PR-125 — Player progression contracts.
- PR-126 — XP reward contracts para inimigos.
- PR-127 — PlayerProgressionManager MVP.
- PR-128 — Integrar XP por EnemyKilledEvent.
- PR-129 — Save/load progression/equipment/hotbar.
- PR-130 — Handoff pós reconciliação PR-099 → PR-129.

### 4.4 FASE9F pendente

- PR-170 — Cave procedural contracts.
- PR-171 — Cave procedural generator MVP.
- PR-172 — Cave run regeneration on player defeat/KO.
- PR-173 — Cave checkpoints a cada 15 níveis.
- PR-174 — ResourceNode contracts com ferramenta, tier, stamina e fallback.
- PR-175 — ResourceNode runtime MVP: Stone, CopperOre, CaveRootTree.
- PR-176 — Cave save/load: seeds, current layer, deepest layer, checkpoints, depleted nodes.
- PR-177 — Enemy spawn by CaveLevel com Slime especial colorido.
- PR-178 — Loot tables para inimigos e nodes.
- PR-179 — XP integration com EnemyLevel × DifficultyMultiplier.
- PR-180 — Daily cave refresh apenas para `RespawnsDaily = true`.
- PR-181 — Biome boss MVP no CaveLevel 15.
- PR-182 — Cave validator.

### 4.5 FASE9G pendente

- Cave encounter ecology contracts.
- Faction locks por subfaixa de 3–5 níveis.
- Enemy families por bioma/facção.
- Incompatibility matrix.
- Boss/miniboss candidates com 3 opções por marco.
- Persistência de boss checkpoint por save.
- Persistência de miniboss por run.
- Debug de `EncounterEcologyId`, `FactionLockId`, `EnemyFamilyIds` e `BossCandidateId`.

---

## 5. Próximo bloco recomendado

```text
FASE9B-4 / FASE9C-0 - Reconciliação pós PR-099
```

Escopo recomendado do próximo bloco:

| PR | Nome | Objetivo |
|---:|---|---|
| PR-101 | Reconciliar branches/fixes PR-099 | Comparar branches citadas e listar ja esta na dev, falta, conflita ou deve virar PR proprio. |
| PR-102 | Corrigir compile/hardening do EnemyHealth | Validar `EnemyHealth`, `DamageRequest` e `KnockbackRequest`. |
| PR-103 | Corrigir PlayerAttackController data-driven | Consolidar ataque melee por componente e `DamageRequest`. |
| PR-104 | Corrigir EnemyContactDamage data-driven | Consolidar dano por contato usando `EnemyDataSO`. |
| PR-105 | Corrigir EnemyChaseController data-driven | Garantir chase configuravel por `EnemyDataSO`. |

PR-170+ foi reclassificado como sequencia futura PR-131+ apos handoff PR-130. A branch local `feature/pr-170-cave-procedural-contracts` existe como codigo adiantado/candidato e deve ser reaproveitada depois, sem rollback destrutivo.

FASE9G deve ser usada quando a implementação chegar em enemy ecology, faction locks, boss/miniboss candidates e bestiário procedural.

---

## 6. Regra de manutenção

Todo PR futuro deve atualizar:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
```

`PROJECT_LOG.md` deve registrar execução, branch, arquivos alterados, testes e pendências.

`docs/IMPLEMENTATION_STATUS.md` deve registrar apenas status curto, verificável e fácil de comparar.

Regras para agentes:

- Ao implementar uma spec, mover o item correspondente de pendente para implementado/parcial.
- Ao criar nova spec aprovada, adicionar o item em pendente.
- Não marcar como implementado sem evidência no repo.
- Não apagar histórico de status sem substituição clara.
- Se houver dúvida, marcar como `Parcial` e registrar pendência.

---

## 7. Última atualização

- Data: 2026-05-20
- Responsável: Codex/ChatGPT
- Branch: `feature/pr-100-audit-pos-pr099`
- Tipo: auditoria documental/estatica PR-100
