# SPEC 14A-FIX3 — Spawn Ecology, Combat Feedback, Menu Consolidation

**Data:** 2026-05-29  
**Branch:** `dev`  
**Sessão:** 29f  
**Status:** IMPLEMENTADO EM CÓDIGO — Unity batchmode pendente

---

## 1. Causa raiz dos bugs observados em Play Mode

### Bug A: Level 1 resolveu apenas 3 inimigos (alvo: 14)

- `pack_stone_fauna_basic` tinha `MaxTotalEnemies = 4` → resolver retornava até ~5 por chamada.
- `CaveEnemySpawnPlanner.CreatePlan` chamava o resolver **uma única vez** → resolvedCount ≤ 5.
- `EnemySpawnPoints` explícitos: 10 pontos < `MinEnemiesPerLevel = 14` → planner limitava os slots.

### Bug B: Level 15 resolveu 0 inimigos ("no spawn pack or individual candidate matched")

- `BiomeId = "biome_cave_earth"` → `NormalizeBiomeTag` retornava `"stone"`.
- Todos os perfis/packs de nível 11-25 tinham bioma `"fungal"`.
- `TagsMatch(["stone"], ["fungal"]) = false` → **todos rejeitados**.
- `BuildBiomeTags` retornava apenas a tag normalizada do `BiomeId`, ignorando a faixa de nível.

---

## 2. Arquivos alterados

| Arquivo | Mudança |
|---------|---------|
| `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` | `BuildBiomeTags` retorna `[levelTag, normalizedTag]`; `ResolveSpawnPoints` complementa com walkable tiles; loop multi-pass (4x) com seed `levelSeed + pass * 13337` |
| `Assets/_Game/Scripts/Enemy/EnemySpawnResolver.cs` | `BuildDiagnosticSummary` logado quando nenhum candidato passa pelos filtros |
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateEnemySpawnEcologyData.cs` | Packs rebalanceados: MaxTotalEnemies 4-5 → 10-14; novos `pack_low_undead` (1-10) e `pack_beast_mid` (11-25); `lock_default_low_tier` inclui `orc_nyx` e novos packs |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AFix2CombatFeedback.cs` | MenuItem renomeado para FIX3; novos métodos `ValidateSpawnEcologyData`, `ValidateMenuConsolidation`, `ValidateBiomeFix` |

### 8 arquivos com consolidação de menus (`"Cindar's Hope/"` → `"CindarsHope/"`)

- `MissingScriptScanner.cs`
- `ValidateItemAndShopData.cs`
- `Spec17CSceneWiringInitializer.cs`
- `CreateCaveBossAssets.cs`
- `ValidateShopModalFlow.cs`
- `CreateDefaultScaleAssets.cs`
- `ValidateTownShopWiring.cs`
- `ValidateSpec17AScaleConfig.cs`

---

## 3. Detalhes das correções

### Fix A — BuildBiomeTags multi-tag

**Antes:**  
`BuildBiomeTags` → apenas `NormalizeBiomeTag(level.BiomeId)` = `"stone"` para `biome_cave_earth`.

**Depois:**  
```
level 15 (biome_cave_earth) → levelTag = "fungal" (faixa 11-25), normalizedTag = "stone"
→ BiomeTags = ["fungal", "stone"]
→ TagsMatch(["fungal","stone"], ["fungal"]) = true → packs fungal passam
```

### Fix B — ResolveSpawnPoints com fallback walkable tiles

**Antes:** apenas `EnemySpawnPoints` explícitos (típico: 10 pontos por nível).  
**Depois:** se `explicit.Count < MinEnemiesPerLevel`, complementa com `WalkableTiles` válidas.

### Fix C — Multi-pass resolver (4 passes)

**Antes:** 1 chamada ao resolver por nível → resolvedCount ≤ MaxTotalEnemies do pack.  
**Depois:** loop de 4 passes, cada um com `seed = levelSeed + pass * 13337`; interrompe quando `resolvedCount >= requestMaxEnemies`.

### Fix D — Pack MaxTotalEnemies rebalanceados

| Pack | Antes | Depois |
|------|-------|--------|
| `pack_stone_fauna_basic` (1-10) | MaxTotal=4 | MaxTotal=14 |
| `pack_grashnaar_kobold_scouts` (1-10) | MaxTotal=5 | MaxTotal=14 |
| `pack_blackroot_growth` (1-10→1-25) | MaxTotal=3 | MaxTotal=10 |
| `pack_fungal_colony` (11-25) | MaxTotal=5 | MaxTotal=14 |
| `pack_urudakh_trappers` (26-40) | MaxTotal=4 | MaxTotal=12 |
| `pack_nyx_ambush` (41-55) | MaxTotal=3 | MaxTotal=10 |
| Novos: `pack_low_undead`, `pack_beast_mid` | — | MaxTotal=8/10 |

### Fix E — Diagnóstico do resolver

`EnemySpawnResolver` agora registra `BuildDiagnosticSummary` quando nenhum candidato passa:
```
EnemySpawnResolver diagnostic: CaveLevel=15, BiomeTags=[fungal,stone],
EnvTags=[...], RoomSize=Medium, ProfilesTotal=40, ProfilesAfterLevel=N, ProfilesAfterBiome=M, ...
```

---

## 4. Antes/Depois por nível

| Nível | Antes | Depois (esperado) |
|-------|-------|-------------------|
| 1 | 3 inimigos resolvidos | 14-24 (alvo cumprido com multi-pass) |
| 15 | 0 inimigos (bioma mismatch) | 14-24 (fungal tags passam) |
| 30 | Não testado | Funcional (ice band) |
| 45 | Não testado | Funcional (fire band) |

---

## 5. Validações executadas

| Validação | Resultado |
|-----------|-----------|
| `dotnet build Assembly-CSharp.csproj` | PENDENTE |
| `dotnet build Assembly-CSharp-Editor.csproj` | PENDENTE |
| `tools/docs/validate_docs.ps1` | PENDENTE |
| Unity batchmode compile | BLOQUEADO (Unity Editor aberto) |
| Play Mode level 1 densidade | PENDENTE (Unity Editor pendente) |
| Play Mode level 15 inimigos | PENDENTE (Unity Editor pendente) |
| `CindarsHope/Validation/Validate SPEC 14A-FIX3` | PENDENTE (Unity Editor) |

---

## 6. Ações pendentes pelo usuário em Unity Editor

1. Rodar `CindarsHope/Generate/Enemy/Generate And Wire SPEC 13G Assets` para regenerar assets de spawn ecology com os packs/pesos atualizados.
2. Rodar `CindarsHope/Validation/Validate SPEC 14A-FIX3 - Spawn Ecology and Combat Feedback`.
3. Validar Play Mode em Cave Level 1 e Level 15: esperar ≥ 14 inimigos resolvidos.
4. Verificar floating numbers vermelhos ao levar dano de contato.
5. Verificar que menus Unity exibem apenas raiz `CindarsHope/`, sem `Cindar's Hope/`.

---

## 7. Risco residual

- Assets YAML em `Assets/_Game/Data/EnemySpawn/` foram gerados na sessão 29d; o rebalanceamento de packs (FIX3) está **no código do editor**, mas não se propaga automaticamente aos assets existentes — exige regeneração via menu Unity.
- Se os assets existentes não forem regenerados, o comportamento de spawn em Play Mode permanece com os pesos antigos.
- FASE9F stable-run: multi-pass usa seed determinístico `levelSeed + pass * 13337` — compatível com o contrato de replay.
