# Cindar's Hope — Addendum Macro pós FASE 9B-1 v1.0

> **Data:** 2026-05-18  
> **Base:** `dev` pós Farm/Town/Cave/Combat MVP  
> **Objetivo:** registrar de forma consolidada o estado real do projeto sem reescrever documentos longos legados ainda úteis como histórico.

---

## 1. Por que este addendum existe

Após a FASE 9B-1, parte dos documentos macro originais ainda descreve o projeto como se estivesse no planejamento da Fase 6/7/8. O `README.md`, os handoffs da FASE 9A/9B e o roadmap foram atualizados, mas documentos longos como `GDD_v2.6.md`, `ARCH_fase4_v2.2.md`, `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`, `FASE6_INDEX_global_v1.2.md`, `FASE7_SPEC_MVP_FARM_v2.2.md` e `FASE8_EXECUTION_PLAN_CODEX_v1.0.md` devem ser lidos com esta correção contextual.

Regra de leitura a partir desta data:

1. `README.md` indica o estado atual executável.
2. `PROJECT_LOG.md` indica o histórico operacional e o estado mais recente.
3. `docs/FASE9A_HANDOFF_FARM_TOWN_SAVE_v1.0.md` indica o fechamento de Farm/Town/Save.
4. `docs/FASE9B_CAVE_COMBAT_MVP_v1.0.md` indica o fechamento de Cave/Combat MVP.
5. `docs/NEXT_WAVES_ROADMAP_v1.0.md` indica a sequência recomendada.
6. Documentos antigos continuam válidos como arquitetura/base de design, mas podem conter status defasado.

---

## 2. Estado real dos épicos após FASE 9B-1

| Épico | Estado real | Observação |
|---|---|---|
| FARM | ✅ MVP funcional | Plantio, crescimento, colheita, árvores, pesca, venda, pickups e save/load. |
| SAVE | ✅ MVP funcional | JSON em `persistentDataPath`, cena atual, inventário, player, farm/world state e cross-scene rebind. |
| CITY | ✅ MVP funcional | TownScene, Pip, compra de sementes, venda SellBox, portal Farm↔Town. |
| CRAFT | ✅ MVP funcional | CraftingPoint e receita básica, ainda sem UI visual final. |
| CAVE | ✅ MVP funcional | CaveScene, portal Farm↔Cave, Slime, retorno para Farm. Sem procedural ainda. |
| COMBAT | ✅ MVP funcional | Soco melee com J, EnemyHealth, dano por contato, chase simples do Slime. |
| UI | 🔄 Parcial | DebugHud persistente; UI real de inventário/shop/crafting pendente. |
| CHAR | 🔄 Parcial | HP e fome básicos; XP, atributos e skill tree pendentes. |
| COMP | ⏳ Pendente | Companions ainda não implementados. |
| QUEST | ⏳ Pendente | Dialogue/Quest system ainda não implementados. |
| ART | ⏳ Pendente | Placeholders; visual slice deve vir antes de arte final. |

---

## 3. Contratos atuais que substituem premissas antigas

### 3.1 Cenas e runtime references

- `GameBootstrap` persiste entre cenas.
- `DebugHud` persiste entre cenas.
- Managers persistentes não devem ser resetados por troca de cena.
- Cada cena deve ter installer próprio para rebind de referências:
  - `FarmSceneRuntimeReferenceInstaller`
  - `TownSceneRuntimeReferenceInstaller`
  - `CaveSceneRuntimeReferenceInstaller`
- Save/load usa `CurrentSceneName` e `CurrentScenePath` para restaurar a cena correta.

### 3.2 Geradores editoriais

Geradores atuais:

- `CindarsHope/Scenes/Create MVP FarmScene`
- `CindarsHope/Scenes/Create MVP TownScene`
- `CindarsHope/Scenes/Create MVP CaveScene`

Regras:

- Geradores devem ser reproduzíveis.
- Geradores não devem alterar `ProjectSettings` sem aprovação explícita.
- Geradores não devem depender de tags/layers inexistentes.
- Avisos de Sorting Layer são dívida técnica aceita no MVP, mas devem ser limpos em PR dedicado.
- Não usar `SetReference` em arrays; arrays devem ser preenchidos via `SerializedProperty.arraySize` e índice.

### 3.3 Combate MVP

- `PlayerAttackController` fica no Player.
- Ataque atual é soco melee curto com tecla `J`.
- Dano atual do soco é baixo e exige proximidade.
- `EnemyHealth` fica no inimigo.
- `EnemyContactDamage` fica no trigger filho do inimigo.
- `EnemyChaseController` fica apenas no Slime da Cave por enquanto.
- Gameplay runtime deve preferir detecção por componentes (`GetComponentInParent`) em vez de tags Unity.
- Não adicionar tags em `ProjectSettings` como contrato de gameplay sem aprovação.

### 3.4 Save e IDs

- Save serializa dados simples e IDs estáveis.
- Não serializar `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject` no JSON.
- Pickups, plots, trees e resource nodes devem ter IDs estáveis quando forem persistidos.
- Cave save ainda é MVP; hardening de drops/resource nodes continua recomendado.

---

## 4. Leitura atual dos documentos legados

### `docs/GDD_v2.6.md`

Continua como fonte de visão de jogo/lore/sistemas completos. Status de implementação deve ser corrigido pelo `README.md` e pelos handoffs 9A/9B.

### `docs/ARCH_fase4_v2.2.md`

Continua válido para arquitetura geral, EventBus, SOs, cenas e pipeline. Deve ser lido com os contratos atualizados deste addendum para runtime installers, save cross-scene e combat MVP.

### `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`

Continua válido para eventos, IDs e save. Deve considerar que `CurrentSceneName` e `CurrentScenePath` já existem no save real e que Cave/Combat MVP já entrou.

### `docs/FASE6_INDEX_global_v1.2.md`

Status dos épicos está defasado. Usar a tabela de estado real deste addendum como override até uma reescrita futura do índice.

### `docs/FASE7_SPEC_MVP_FARM_v2.2.md`

Continua válido para specs Farm MVP, mas Farm/Town/Cave já avançaram além do escopo original.

### `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md`

Continua válido como histórico e estilo operacional de execução por PRs pequenos, mas a fase executada já avançou para 9A/9B.

---

## 5. Próxima sequência recomendada

A próxima fase técnica não deve ser arte final ainda.

Sequência recomendada:

1. **PR-092 — Limpar warnings de geradores**
   - Sorting Layers.
   - Spawn IDs.
   - avisos de Input Manager deprecated registrados.
2. **PR-093 — Combat feel MVP**
   - knockback simples;
   - invulnerability frames curtos;
   - flash visual placeholder;
   - feedback mínimo de dano/morte.
3. **PR-094 — Cave hardening**
   - drops;
   - resource nodes;
   - save/load de Cave mais robusto;
   - 2–3 inimigos simples.
4. **PR-095 a PR-097 — UI real MVP**
   - inventário;
   - loja;
   - crafting.
5. **PR-098/099 — Dialogue + primeira quest**
6. **Visual Slice**
7. **Arte final/polish**

---

## 6. Critério para considerar docs sincronizadas

Este addendum, junto com:

- `README.md`
- `PROJECT_LOG.md`
- `FASE9A_HANDOFF_FARM_TOWN_SAVE_v1.0.md`
- `FASE9B_CAVE_COMBAT_MVP_v1.0.md`
- `NEXT_WAVES_ROADMAP_v1.0.md`

fecha a sincronização documental mínima pós FASE 9B-1.

A reescrita completa dos documentos longos pode ser feita depois, mas não deve bloquear PR-092.
