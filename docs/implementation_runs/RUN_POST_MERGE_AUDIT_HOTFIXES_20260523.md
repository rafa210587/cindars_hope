# RUN — Post-Merge Audit & Hotfixes 2026-05-23

> **Branch:** `dev`  
> **Contexto:** validação pós-merge da estabilização overnight + correções surgidas em Play Mode.  
> **Status:** tracking atualizado; validação Unity batchmode local ainda obrigatória.

---

## 1. Objetivo

Registrar o estado real após:

- merge da branch `review/stabilize-overnight-specs` em `dev`;
- correções de compile/runtime reportadas durante validação manual;
- ajustes de debug para cave, boss gates, combat logs e XP debug;
- revisão dos arquivos de acompanhamento/documentação.

---

## 2. Correções aplicadas após o merge

### 2.1 Compile / Create Scene

- `LootTableSO`: `Random` ambíguo corrigido para `UnityEngine.Random.value`.
- `SceneSpawnPoint`: warning transitório de `spawnId` vazio removido do `OnValidate`.
- `CaveDebugLevelSkipController`: compatibilidade restaurada com campos serializados antigos usados pelo generator de CaveScene.

### 2.2 Cave boss gates / checkpoints

- `P/F2` passa a pular para o próximo boss gate, não mais para `level + 1`.
- `CaveBossGateRegistrySO` agora possui fallback runtime para gates padrão:

```text
15, 30, 45, 60, 75, 90
```

- `CaveRunManager` teve o bloqueio generalizado:

```text
level N com boss gate bloqueia avanço N -> N+1 enquanto boss_gate_level_N não estiver derrotado.
```

- Regra atual:

```text
Checkpoints oficiais: 1, 15, 30, 45, 60, 75, 90
Boss gates: 15, 30, 45, 60, 75, 90
Derrotar boss em N libera checkpoint N.
```

### 2.3 Boss runtime

- Removido uso da tag Unity `BossEnemy`, que não existia e gerava erro.
- `CaveBossDeathReporter` passou a ser chamado pelo próprio `EnemyHealth` do GameObject do boss.
- A morte do boss não depende mais de distância até o spawn, evitando falha quando knockback desloca o boss.
- Boss logs expõem:

```text
IsBoss=true
BossGateId
CaveLevel
CheckpointUnlock
```

### 2.4 Debug HUD / XP / logs

- `DebugHud` agora possui tecla `O` para conceder `+99 XP`.
- Checagem de `CanAdvancePastCurrentBossGate` no HUD é silenciosa para evitar log infinito.
- `EnemyHealth` agora loga:

```text
Name
EnemyId
IsBoss
Damage
HP antes/depois/total
Drop
XP
```

---

## 3. Estado real pós-auditoria

| Área | Estado |
|---|---|
| Overnight specs | Parcial; backend/data skeleton estabilizado. |
| Merge em `dev` | Feito. |
| Compile Unity | Ainda precisa batchmode local final. |
| Play Mode manual | Parcial; logs do usuário confirmaram fluxo do boss gate 15 após correções. |
| Boss gates | Parcial estabilizado; fallback runtime para 15/30/45/60/75/90. |
| CaveBossGateRegistry.asset persistido | Pendente; fallback runtime ainda em uso se asset vazio. |
| SkillPoint | Corrigido: +1 em níveis pares. |
| AttributePoint | +1 por level up; gasto final parcial. |
| Skill tree final | Parcial; active slots/capstones/respec/Fonte de Anya pendentes. |
| Debug UX | Parcial; útil para validação, não UI final. |

---

## 4. Arquivos principais alterados nesta etapa

```text
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/SceneManagement/SceneSpawnPoint.cs
Assets/_Game/Scripts/Cave/Runtime/CaveDebugLevelSkipController.cs
Assets/_Game/Scripts/Cave/Data/CaveBossGateRegistrySO.cs
Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs
Assets/_Game/Scripts/Cave/Runtime/CaveBossSpawner.cs
Assets/_Game/Scripts/Cave/Runtime/CaveBossDeathReporter.cs
Assets/_Game/Scripts/Combat/EnemyHealth.cs
Assets/_Game/Scripts/UI/DebugHud.cs
docs/IMPLEMENTATION_DELIVERY_20260523.md
```

---

## 5. Validações recomendadas

### 5.1 Batchmode

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-dev-post-merge.log"

Select-String -Path "Logs\unity-compile-dev-post-merge.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|Failed|Tag: BossEnemy|CaveBossGateRegistry not found"
```

### 5.2 Play Mode mínimo

```text
1. Create MVP FarmScene.
2. Create MVP TownScene.
3. Create MVP CaveScene.
4. Play CaveScene.
5. Pressionar P: deve ir para level 15.
6. Antes de matar boss: gate 15 deve bloquear 15 -> 16.
7. Matar boss: log deve mostrar IsBoss=true e CaveBossDeathReporter.
8. Checkpoint 15 deve ser desbloqueado.
9. Avanço 15 -> 16 deve funcionar.
10. P novamente deve ir para 30.
11. Gate 30 deve bloquear 30 -> 31 até boss morrer.
12. Pressionar O deve conceder +99 XP.
```

---

## 6. Pendências preservadas

- Persistir `CaveBossGateRegistry.asset` com gates reais para remover fallback runtime.
- Validar gates 30/45/60/75/90 em Play Mode.
- Consolidar roster real de bosses por gate.
- Finalizar save schema migration.
- Finalizar skill tree runtime: compra de nodes, active slots, capstones, Fonte de Anya e save/load.
- Finalizar weapons/spells/skill actions runtime.
- Finalizar equipment/durability/environment runtime.
- Finalizar bestiary/faction locks/AI runtime.
- Finalizar UI/UX gameplay.

---

## 7. Conclusão

A documentação central foi atualizada para refletir que a `dev` recebeu a estabilização e hotfixes de validação, mas o estado geral continua parcial.

Não tratar FASE9H/I/J/K/L como completas.

Próximo trabalho recomendado: refinar colaborativamente os 18 `refinamento_init_*.md` antes de gerar specs runtime executáveis.
