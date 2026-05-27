# Cindar's Hope - Implementation Status

> Status: tracking reconciliado por validacao estatica de codigo em 2026-05-26.
> Fonte oficial de specs: `docs/specs/`.
> A pasta raiz `specs/` foi removida e nao deve ser recriada.

## 1. Resumo executivo

A validacao de codigo ate a SPEC 16 confirma que o projeto possui baseline suficiente para seguir para a proxima etapa de UI/closeout, desde que as proximas execucoes nao tratem specs parciais como completas.

Nao ha erro CS0023 ativo em `ValidateItemAndShopData.cs`: o validador usa `entry.Item == null` para `StartingItem` struct.

Principais conclusoes:

- SPECS 00-09: base documental/tooling/runtime MVP majoritariamente implementada, com algumas areas historicas ainda parciais.
- SPECS 10-14: implementacao parcial/residual ativa; nao tratar como completas.
- SPEC 15: implementacao parcial em codigo. Existem death DTOs, `PlayerDeathController`, `Corpse`, `CorpseRecoveryManager` e captura de `DeathSaveData`, mas a orquestracao completa de cave death/Anya/corpse spawn/restore nao esta fechada no codigo validado.
- SPEC 16: implementada em codigo. Skill trees, 5 arvores/55 nodes, compra, slots ativos, respec service, save v5 e manager existem; Unity compile e Play Mode humano seguem pendentes.
- SPEC 17+: ja recebeu incrementos, mas permanece etapa de UI/closeout e validacao final.

## 2. Resumo por area

| Area | Status real | Evidencia / observacao |
|---|---|---|
| Claude Code project structure `.claude/` | Implementado completo | `.claude/settings.json`, commands, skills, agents e hooks. |
| Governanca documental / fonte unica | Implementado documental parcial | Fonte oficial em `docs/specs/`. Nao recriar `specs/` ou `spec/` na raiz. |
| Unity compile validation protocol | Implementado completo como tooling minimo | `tools/unity/RunUnityCompileValidation.ps1`, `tools/unity/ScanUnityLogs.ps1`, `tools/docs/validate_docs.ps1`. |
| Core/event bus/bootstrap | Implementado parcial | Base operacional presente; ainda existem gaps de wiring em features parciais. |
| Data/IDs/registries | Implementado | ScriptableObjects e registries ativos. |
| Save/load JSON cross-scene | Implementado parcial | SaveManager v5 existe; algumas features ainda nao restauram todo estado funcional. |
| Save schema migration v2+ | Implementado em codigo | Registry de migrations inclui v1->v2, v2->v3, v3->v4 e v4->v5. |
| Inventory slots/capacidade/UI minima | Implementado em codigo / parcial funcional | Slots reais e compat agregado; pendem Use especifico, drag/drop, sort e UI final. |
| Farm irrigacao/solo/planting UI | Implementado parcial | Menu contextual/plantio/irrigacao existem; Play Mode final e polimento seguem pendentes. |
| World activities/fishing/trees/loot | Implementado parcial | Loot/fishing/tree HP/regrowth existem parcialmente; spawners/cave variants ainda residuais. |
| Economy/shop/stock/pricing/UI | Implementado em codigo | ShopManager, sessoes, buy/sell, stock e save existem; 17D/17E ainda pedem Unity/Play Mode final. |
| Crafting queue/workstations/recipes/UI | Implementado em codigo | CraftingManager e runtime existem; Play Mode final segue pendente. |
| Town NPC/dialogue/schedule/quests | Implementado em codigo | NPC/shop/dialogue wiring existe; Play Mode final segue pendente. |
| Hunger/stamina/status/time | Implementado completo MVP | GameTime, hunger, stamina, status, save e HUD minimo estao documentados como fechados; Canvas final fica na SPEC 17. |
| Equipment/durability/environment/loot | Implementado parcial | Mantido como parcial. |
| Damage/status/elements/resistances | Implementado parcial | Mantido como parcial. |
| Player combat/weapons/spells/skill actions | Implementado parcial | Mantido como parcial; UI final em SPEC 17. |
| Enemy AI/roster/bestiary/faction locks | Implementado parcial | Residual ativo. |
| Cave runtime/procedural/checkpoints/boss gates | Implementado parcial / validacao Unity pendente | Residual ativo. |
| Cave entry/death/Anya/corpse recovery — SPEC 15 | Implementado parcial em codigo | Ver secao 5. |
| Skill trees/active slots/respec Anya — SPEC 16 | Implementado em codigo; Unity/Play Mode pendentes | Ver secao 6. |
| UI Gameplay MVP / closeout — SPEC 17+ | Implementado parcialmente | SPEC 17 ampla permanece aberta; 17C/17D/17E possuem pendencias de Unity/Play Mode. |

## 3. Status oficial por SPEC/prompt ate 16

| Spec | Status real apos validacao de codigo | Observacao |
|---|---|---|
| 00 | Implementado documental parcial | Nao reexecutar spec antiga. |
| 01 | Implementado completo | Tooling minimo existe. |
| 02 | Implementado parcial | Infra de migration existe; manter parcial conforme ordem oficial. |
| 03 | Implementado parcial | Inventory tem slots e capacidade, mas ainda ha pendencias funcionais/UI. |
| 04 | Implementado parcial | Farm planting/irrigacao existem, mas Play Mode/polimento pendem. |
| 05 | Implementado parcial | World activities existem parcialmente; residual ativo. |
| 06 | Implementado completo em codigo | Shop/economy implementado; Play Mode final ainda depende das specs de closeout. |
| 07 | Implementado completo em codigo | Crafting/workstations/recipes em codigo; Play Mode final pendente. |
| 08 | Implementado completo em codigo | Town/NPC/dialogue em codigo; Play Mode final pendente. |
| 09 | Implementado completo MVP | Hunger/stamina/status/time fechados como MVP. |
| 10 | Implementado parcial | Nao tratar como completo. |
| 11 | Implementado parcial | Nao tratar como completo. |
| 12 | Implementado parcial | Nao tratar como completo. |
| 13 | Implementado parcial - residual ativo | Ainda listado como residual ativo. |
| 14 | Implementado parcial - residual ativo | Ainda listado como residual ativo. |
| 15 | Implementado parcial em codigo | Death/corpse base existe; Anya/orquestracao/spawn/restore incompletos no codigo validado. |
| 16 | Implementado em codigo | Skill tree stack existe; Unity compile/Play Mode humano pendentes. |

## 4. Evidencias de codigo validadas

### 4.1 Erro CS0023 ja corrigido

`Assets/_Game/Scripts/Editor/Validation/ValidateItemAndShopData.cs` usa:

```csharp
var entry = playerData.StartingItems[i];
if (entry.Item == null)
```

Como `StartingItem` e struct, isso esta correto. Nao ha mais `entry?.Item` nesse trecho.

### 4.2 SaveManager v5 / migrations

`SaveManager` esta em schema v5 e registra:

- `InventorySlotsV1ToV2Migration`
- `SaveV2ToV3Migration`
- `SaveV3ToV4Migration`
- `SaveV4ToV5Migration`

Tambem captura `SkillTree`, `ActiveSkillSlots`, `Death`, `Economy`, `Crafting`, `Stamina`, `GameTime`, `StatusEffects`, `EquipmentDurability` e `Npcs`.

## 5. SPEC 15 — Cave entry, death, Anya e corpse recovery

Status real: **implementado parcial em codigo**.

### 5.1 Confirmado em codigo

Arquivos/elementos encontrados e coerentes:

- `PlayerDeathController`: escuta `HPChangedEvent` e publica `PlayerDiedEvent` quando HP chega a zero.
- `Corpse`: modelo runtime com id, status, run/cave data, posicao, gold e listas de itens/equipment.
- `CorpseRecoveryManager`: controla active corpse, recuperacao de gold, itens e equipment, e publica eventos de recovery/parcial/replaced.
- `CorpseSaveData`, `CorpseItemSaveData`, `DeathStatsSaveData`, `DeathSaveData`: DTOs serializaveis.
- `SaveManager.CaptureDeathSaveData`: captura/preserva `DeathStats` e `ActiveCorpse` existente.

### 5.2 Nao encontrado / nao fechado no codigo validado

Nao foram encontrados no `dev` durante esta validacao:

- `CaveDeathResolver`
- `DeathSystemBootstrap`
- `CorpseInteractable`
- `CorpseSpawner`
- `AnyaFountain`
- `AnyaRespawnService`
- `AnyaFountainInteractable`

Alem disso, `SaveManager.RestoreDeathSaveData` ainda contem TODO e nao restaura active corpse para o runtime.

Conclusao: SPEC 15 nao deve ser tratada como completa. Ela e suficiente como fundacao parcial, mas nao como fluxo funcional fechado de morte -> respawn Anya -> corpse persistente -> recover.

## 6. SPEC 16 — Skill trees, active slots e respec Anya

Status real: **implementado em codigo; Unity compile/Play Mode humano pendentes**.

Confirmado em codigo/documentacao:

- `SkillTreeManager` como `MonoBehaviour`.
- Fallback `DefaultSkillCatalog` com 5 arvores / 55 nodes.
- `SkillPurchaseService` para custo, prerequisites, level minimo e capstone rules.
- `SkillRespecService` com primeiro respec gratuito e custo padrao posterior.
- `SkillPassiveApplicator` e eventos de derived stats.
- `SkillTreeSaveData` com `PurchasedNodeIds`, `ActiveSkillSlots` e `RespecCount`.
- `SaveV4ToV5Migration` inicializa `SkillTreeSaveData`.
- `SaveManager` captura e restaura `SkillTree`.
- `SkillTreeInputHandler` abre skill tree em `U`.

Pendencias:

- Rodar Unity compile real.
- Play Mode humano: subir level par, comprar node, equipar slot, respec, salvar/carregar.
- Validar a integracao real com Fonte de Anya em cena, porque SPEC 15/Anya ainda esta parcial no codigo validado.

## 7. Pendencias que nao devem bloquear a proxima etapa de UI/closeout

As pendencias abaixo nao impedem iniciar a proxima etapa, desde que fiquem declaradas como riscos/residuais:

- SPEC 10-14 continuam parciais/residuais.
- SPEC 15 nao fecha orquestracao/Anya/corpse restore.
- SPEC 16 precisa Unity/Play Mode humano.
- SPEC 17 ampla ainda precisa Canvas final, pause/options, fluxos cave/corpse/toasts e validacao final.

## 8. Proximo passo recomendado

Seguir para SPEC 17/UI/UX/closeout, com guardrails:

- Nao marcar 10-15 como completas.
- Nao depender de Anya/corpse restore como pronto.
- Validar UI em cima do que existe em codigo.
- Deixar claro que a validacao humana sera feita no final do pacote.
- Antes de fechamento final, executar Unity compile, scanners e Play Mode.
