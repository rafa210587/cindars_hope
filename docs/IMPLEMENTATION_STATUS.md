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
| UI | Debug parcial | `DebugHud`/OnGUI existe; PR-132-FIX adicionou split em paineis e feedback temporario; UI final pendente. |
| Equipment/Hotbar | Implementado parcial | Tool/hotbar existem e persistem parcialmente; PR-132-FIX aplica gating em arvore/pesca e plantio por hotbar; integracoes finais pendentes. |
| Progression/LevelUp | Implementado parcial | XP/level/pontos existem; distribuição debug de atributos ainda pendente. |
| Damage Formula MVP | Implementado parcial | `DamageCalculator`, `DamageResult` e integracao melee MVP; status/elementos completos pendentes. |
| Cave Procedural/Resources | Implementado parcial | PR-140 criou contratos base; generator/runtime/resources/save integrados ainda pendentes. |
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
| PR-131 validação HUD/tools/progressão | PR-131 | Implementado | `docs/audits/PR131_VALIDACAO_HUD_TOOLS_PROGRESSION.md` | Seguir PR-132 a PR-139 antes da Cave Procedural. |
| PR-140 Cave procedural contracts | PR-140 | Implementado parcial | `Assets/_Game/Scripts/Cave/Data/**`, `CaveRuntimeState`, `CaveSaveData`, eventos de cave | Generator, runtime manager, save integration e resources ainda pendentes. |

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
- HUD debug: PR-132-FIX implementou split em painel de acoes e painel de informacoes via `DebugHud`; polimento final ainda pendente.
- Tool gating: PR-132-FIX exige ferramenta equipada em `TreeNode` e `FishingSpot`.
- Hotbar seed gating: PR-132-FIX faz `FarmPlot` usar o item selecionado na hotbar para plantio.
- Attribute allocation: pontos existem, mas ainda não há input debug para gastar pontos.
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

- Cave procedural tem contratos base criados no PR-140.
- Cave atual ainda e fixed MVP: sem generator, seeds runtime, HUD procedural, ResourceNode runtime ou save/load integrado.
- Proximo passo FASE9F: PR-141 Cave procedural generator puro.

### 4.5 FASE9E-D HUD/tools/progression debug pendente

- PR-132-FIX — HUD split, feedback de acao, tool gating e hotbar seed gating implementados parcialmente.
- PR-137 — Attribute allocation debug MVP.
- PR-138 — DebugHud progression/tool/hotbar polish.
- PR-139 — Handoff para Cave Procedural.

### 4.6 FASE9G pendente

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
FASE9E-D - HUD Debug v2 + Tool Gating + Attribute Allocation
```

Escopo recomendado do próximo bloco:

| PR | Nome | Objetivo |
|---:|---|---|
| PR-132 | DebugHud layout v2 | Separar ações/contexto de informações/status. |
| PR-133 | Action feedback event | Criar feedback temporário para falhas de ação. |
| PR-134 | Tool gating contracts | Permitir sistemas consultarem ferramenta equipada. |
| PR-135 | Tool gating world actions | Exigir Axe para árvore e FishingRod para pesca. |
| PR-136 | Hotbar seed gating | Usar seed selecionada na hotbar para plantio. |

Cave Procedural deve ser retomada somente depois do handoff PR-139 deste pacote.

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
- Branch: `feature/pr-140-cave-procedural-contracts`
- Tipo: PR-140 Cave procedural contracts

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

Proximo bloco pendente: FASE9E-D HUD/tools/progressao debug PR-132 a PR-139; depois Cave Procedural.

---

## 9. Atualizacao 2026-05-20 - PR-132-FIX HUD/tools/hotbar

Status: Implementado parcial.

Evidencia no repo:
- `Assets/_Game/Scripts/UI/DebugHud.cs` com `DrawActionsPanel`, `DrawInfoPanel`, feedback temporario e cave fixed MVP.
- `Assets/_Game/Scripts/Core/Events/PlayerActionFeedbackEvent.cs`.
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs` com `HasTool` e mensagem de ferramenta ausente.
- `Assets/_Game/Scripts/World/TreeNode.cs` exige Axe Basic.
- `Assets/_Game/Scripts/World/FishingSpot.cs` exige FishingRod Basic.
- `Assets/_Game/Scripts/Farm/FarmPlot.cs` planta pelo item selecionado na hotbar.

Pendencias reais:
- Unity ainda precisa compilar/validar.
- Layout do HUD precisa verificacao visual em 1280x720.
- Attribute allocation debug segue pendente.
- Cave Procedural segue pendente.

Proximo bloco pendente: validar PR-132-FIX no Unity; depois seguir para attribute allocation debug/handoff FASE9E-D.

---

## 10. Atualizacao 2026-05-20 - PR-140 Cave procedural contracts

Status: Implementado parcial.

Evidencia no repo:
- `Assets/_Game/Scripts/Cave/Data/CaveGenerationConfigSO.cs`
- `Assets/_Game/Scripts/Cave/Data/CaveLevelConfigSO.cs`
- `Assets/_Game/Scripts/Cave/Data/CaveBiomeDataSO.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeState.cs`
- `Assets/_Game/Scripts/Save/CaveSaveData.cs`
- `Assets/_Game/Scripts/Core/Events/CaveLevelEnteredEvent.cs`
- `Assets/_Game/Scripts/Core/Events/CaveRunRegeneratedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/CaveCheckpointUnlockedEvent.cs`

Pendencias reais:
- Unity ainda precisa compilar/validar.
- `CaveSaveData` ainda nao esta integrado ao `GameSaveData`/`SaveManager`.
- Generator procedural, `CaveRunManager`, checkpoints service e ResourceNode runtime seguem pendentes.

Proximo bloco pendente: PR-141 Cave procedural generator puro.

---

## 11. Atualizacao 2026-05-20 - Pacote PR-141 a PR-153 Cave Procedural Runtime

Status: Em implementacao nesta branch.

Evidencia incremental:
- PR-141: `Assets/_Game/Scripts/Cave/Generation/**` com generator puro e debug ASCII.
- PR-142: `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` com seeds e captura/restauracao de `CaveSaveData`.
- PR-143: `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` gera cave no runtime e publica `CaveLevelEnteredEvent`.
- PR-144: `CreateMvpCaveScene` cria `CaveRuntime` e configura `CaveGenerationConfig_Default` ao regenerar a cena no Unity.
- PR-145: `DebugHud` exibe status procedural quando o installer rebinda `CaveRunManager` e `CaveLevelRuntimeController`.
- PR-146: `CaveLevelRuntimeController` regenera a run com `Shift+R` na CaveScene.
- PR-147: `CaveCheckpointService` implementa checkpoints oficiais e usa `CaveRuntimeState`.
- PR-148: `ResourceNodeDataSO`, `ResourceNodeDatabaseSO` e `ResourceNodeDepletedEvent` criados.
- PR-149: `ResourceNodeRules` e resultados puros de interacao/tool check criados.
- PR-150: `ResourceNode` runtime MVP interagivel com ferramenta/tier/fallback e deplecao.

Pendencias reais:
- Unity ainda precisa compilar/validar.
- Runtime controller, seeds, HUD, resources, save/load e validator seguem nos proximos commits desta branch.
