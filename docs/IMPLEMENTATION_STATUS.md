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
| Equipment/Hotbar | Implementado parcial | Contratos, manager/debug e save parcial vindos do consolidado PR-101 a PR-130; UI final pendente. |
| Progression/LevelUp | Implementado parcial | Contratos, XP/level MVP e save parcial vindos do consolidado PR-101 a PR-130; balanceamento final pendente. |
| Damage Formula MVP | Implementado parcial | `DamageCalculator`, `DamageResult` e integracao melee MVP; status/elementos completos pendentes. |
| Cave Procedural/Resources | Pendente | FASE9F pronta; proximo bloco recomendado PR-132 a PR-145. |
| Cave Bestiary/Faction Locks | Especificado | FASE9G pronta; depende da base FASE9F para implementação real. |
| Reconciliação pós PR-099 | Implementado parcial | PR-100 auditado; PR-101 a PR-130 consolidados na `dev`; Unity ainda pendente. |

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
| Fase 9E Specs | Docs/specs + MVP parcial | Implementado parcial | UI/Hotbar, Damage/Status, Item Taxonomy, Save Migration, Level Up | Contratos/MVP parcial implementados; UI final, status completos e migration robusta pendentes. |
| Fase 9F Specs | Docs/specs | Pendente | Cave procedural/resources/encounters spec | Próximo bloco PR-132 a PR-145. |
| Fase 9G Specs | Docs/specs | Especificado | Cave bestiary/faction locks/portal ecology spec | Depende de FASE9F procedural foundation. |
| PR-100 pós PR-099 | PR-100 | Implementado | `docs/audits/PR100_POST_PR099_REPO_AUDIT.md` | Reconciliação seguinte foi consolidada em PR-101 a PR-130. |
| PR-101 a PR-130 pós PR-099 | Consolidado | Implementado parcial | `docs/audits/PR101_PR099_BRANCH_RECONCILIATION.md`, `docs/audits/PR130_RECONCILIACAO_HANDOFF.md`, Tools/Equipment/Hotbar/Progression/Damage MVP | Unity e cenas ainda precisam validação. |

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

### 4.2 FASE9E parcial / pendente

- UI/Hotbar/Inventory/Equipment: implementado parcial; UI final e integração completa pendentes.
- Damage/Elementos/Status/Fórmula única: fórmula MVP implementada; elementos/status completos pendentes.
- Item Taxonomy/IDs.
- Item examples/variations.
- Save Schema/Migration: save parcial implementado; migration robusta pendente.
- Player Level Up/Progression: implementado parcial; balanceamento/UX completos pendentes.

### 4.3 Reconciliação pós PR-099 implementada parcial

- PR-101 a PR-130 foram consolidados na `dev`.
- Evidência: auditorias PR-101/PR-116/PR-130, smoke test documentado, hardening combat, Tools/Equipment/Hotbar/Progression/Damage MVP e save parcial.
- Pendência: validar Unity, regenerar cenas quando necessário e executar smoke test Farm/Town/Cave.

### 4.4 FASE9F pendente

- PR-132 — Pre-flight Unity hardening antes da Cave Procedural.
- PR-133 — Cave procedural contracts.
- PR-134 — Cave procedural generator puro.
- PR-135 — Cave generated level model/debug.
- PR-136 — CaveLevelRuntimeController MVP.
- PR-137 — CaveScene generator/wiring procedural.
- PR-138 — CaveRunManager seeds.
- PR-139 — Regeneração da run após KO/derrota.
- PR-140 — Cave checkpoints service.
- PR-141 — Entrada por checkpoint debug/MVP.
- PR-142 — ResourceNode contracts.
- PR-143 — ResourceNode rules/fallback.
- PR-144 — ResourceNode runtime MVP.
- PR-145 — Cave save/load + validator + handoff.

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
FASE9F-A - Cave Procedural Foundation
```

Escopo recomendado do próximo bloco:

| PR | Nome | Objetivo |
|---:|---|---|
| PR-132 | Pre-flight Unity hardening | Corrigir bloqueadores simples antes de cave procedural. |
| PR-133 | Cave procedural contracts | Criar contratos base de cave procedural. |
| PR-134 | Cave generator puro | Criar gerador determinístico por seeds e level. |
| PR-135 | Cave generated level debug | Adicionar debug textual/ASCII do layout. |
| PR-136 | CaveLevelRuntimeController MVP | Gerar CaveLevel 1 no runtime. |

PR-170+ foi reclassificado como sequencia futura PR-131+ apos handoff PR-130, mas a numeração operacional vigente agora é PR-132 a PR-145. A branch local `feature/pr-170-cave-procedural-contracts`, se existir, deve ser tratada como codigo adiantado/candidato e reaproveitada depois sem rollback destrutivo.

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
- Branch: `feature/pr-131-sync-status-pos-reconciliacao`
- Tipo: sync documental PR-131 pos reconciliacao PR-101 a PR-130

---

## 8. Atualizacao 2026-05-20 - PR-101 a PR-130 consolidado

Status: Implementado parcial consolidado na `dev`.

Evidencia no repo:
- `docs/audits/PR101_PR099_BRANCH_RECONCILIATION.md`
- `docs/audits/PR116_ITEM_ID_AUDIT.md`
- `docs/audits/PR130_RECONCILIACAO_HANDOFF.md`
- `docs/validation/SMOKE_TEST_FARM_TOWN_CAVE_MVP.md`
- hardening em `Assets/_Game/Scripts/Combat/**`
- contratos em `Assets/_Game/Scripts/Tools/**`, `Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/UI/Hotbar/**`
- progresso MVP em `Assets/_Game/Scripts/Player/Progression/**`
- save parcial em `Assets/_Game/Scripts/Save/SaveData.cs` e `Assets/_Game/Scripts/Save/SaveManager.cs`

Pendencias reais:
- Unity ainda precisa compilar/validar.
- Cenas nao foram regeneradas nesta sessao.
- `item_material_stone` e `ore_copper` seguem pendentes para Cave Resources.
- Cave Procedural deve voltar como PR-131+ apos validacao.

Proximo bloco pendente: validacao Unity do consolidado e depois Cave Procedural Foundation PR-131+.

---
