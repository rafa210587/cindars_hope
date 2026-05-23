# SPEC CAVE-007 — Snapshot replay full layout hardening

> Status: Implementado em código — validação Unity pendente
> Camada: Cave
> Fonte histórica: `docs_old/audits/FIX_CAVE_SNAPSHOT_REPLAY_FULL_LAYOUT.md`
> Refinamento relacionado: `docs/refinements/implementados/ref_fix_cave_snapshot_replay_full_layout.md`
> Evidência principal: `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs`

---

## 1. /speckit.specify

### O que existe
Snapshots guardam layout completo e restore reconstrói o nível visitado para evitar reroll no backtrack.

### Por que existe
Mantém identidade da run procedural e preserva decisões do jogador.

### Fora de escopo
Não garante validação Play Mode completa ainda.

---

## 2. /speckit.plan

### Arquitetura real
VisitedLevelSnapshot, CaveLevelRuntimeController, CaveRuntimeMaterializer e CaveSaveData.

### Fluxo
Ao visitar/materializar, captura layout; ao voltar, restaura snapshot em vez de gerar novo layout.

### Persistência
CaveSaveData persiste snapshots/estado por DTOs simples.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Testar backtrack, save/load e nodes depletados no Unity.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` | Evidência principal. |
| Histórico | `docs_old/audits/FIX_CAVE_SNAPSHOT_REPLAY_FULL_LAYOUT.md` | Fonte histórica preservada. |
| Refinamento | `docs/refinements/implementados/ref_fix_cave_snapshot_replay_full_layout.md` | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Testar backtrack, save/load e nodes depletados no Unity.




