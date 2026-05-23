# refinamento_init_cave_runtime_generation_checkpoints_boss_gates

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_cave_runtime_generation_checkpoints_boss_gates.md`  
> **Objetivo:** completar a caverna procedural runtime, checkpoints, boss gates, confinement, snapshot replay e validação Unity.

---

## 1. Estado atual

A caverna possui implementação em código para run state, seed, níveis, snapshots, materialização runtime e checkpoints/boss gates em estado parcial.

Evidências principais:

```text
Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs
Assets/_Game/Scripts/Cave/**
docs/specs/implementados/spec_cave_002_procedural_resources_parcial.md
docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md
docs/specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md
docs/specs/implementados/spec_cave_005_visual_runtime_materializer_camera_enemy_visuals_parcial.md
docs/specs/implementados/spec_cave_006_spawn_anchor_safe_positioning_parcial.md
docs/specs/implementados/spec_cave_007_snapshot_replay_full_layout_parcial.md
docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_parcial.md
```

---

## 2. Gaps

- Unity Play Mode ainda precisa validar materialização real da cena.
- Checkpoints/boss gates existem em código, mas precisam de validação com assets/registries reais.
- Boss fights reais ainda não existem.
- Spawn pools por cave band/faction/bioma ainda não estão completos.
- Resource renewal por dia e nível precisa validação end-to-end.
- Snapshot replay/backtracking precisa teste comparando layout antes/depois.
- Debug skip deve respeitar confinement e não corromper run state.
- `CaveBossGateRegistrySO` ou referência equivalente precisa estar garantida em Resources/Inspector/installer.

---

## 3. Escopo esperado

### Runtime

Completar e validar:

```text
CaveRunManager
CaveRuntimeMaterializer
CaveLevelSnapshot
CaveCheckpointService
CaveBossGateService
CaveSpawnAnchorService
CaveDebugSkipService
```

### Regras

- Save possui seed estável da run/caverna.
- Entrar no mesmo nível com snapshot existente deve reproduzir layout anterior.
- Avançar sem derrotar boss gate deve bloquear.
- Derrotar boss deve liberar checkpoint/band conforme regra.
- Player nunca spawna dentro de parede.
- Debug skip não pode ignorar travas permanentes sem flag explícita.

### Dados

Criar/consolidar:

```text
CaveBiomeDataSO
CaveBossGateDataSO
CaveResourceSpawnProfileSO
CaveEnemySpawnProfileSO
```

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs
Assets/_Game/Scripts/Cave/Generation/**
Assets/_Game/Scripts/Cave/Data/**
Assets/_Game/Scripts/SceneManagement/CaveSceneRuntimeReferenceInstaller.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/SaveManager.cs
```

---

## 5. Fora de escopo

- Criar todos os 100 níveis finais com arte definitiva.
- Boss AI final.
- UI final da caverna.
- Biomas finais completos.

---

## 6. Definition of Done

- [ ] CaveScene abre sem warnings bloqueantes de referências ausentes.
- [ ] Entrar na caverna cria run state válido.
- [ ] Voltar para nível já visitado usa snapshot, não reroll.
- [ ] Checkpoint é salvo/carregado.
- [ ] Boss gate bloqueia avanço enquanto locked.
- [ ] Boss gate libera avanço após estado derrotado/sinalizado.
- [ ] Player spawn é seguro em todos os níveis testados.
- [ ] Unity batchmode e Play Mode manual passam.

---

## 7. Validação

1. Abrir FarmScene e entrar CaveScene via portal.
2. Validar CaveRunManager inicializado com seed.
3. Avançar nível, voltar, comparar layout/snapshot.
4. Forçar boss gate locked e validar bloqueio.
5. Marcar boss derrotado e validar liberação.
6. Salvar/carregar dentro e fora da cave.
7. Rodar batchmode e procurar `MissingReference`, `Missing Script`, `not found` e `NullReferenceException`.
