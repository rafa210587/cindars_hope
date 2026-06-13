# SPEC CAVE-005 — Visual runtime, camera e enemy visuals

> Status: Implementado em código — validação Unity pendente
> Camada: Cave
> Fonte histórica: `docs_old/audits/FIX_CAVE_PROCEDURAL_VISUAL_HANDOFF.md`
> Refinamento relacionado: `docs/refinements/implementados/ref_fix_cave_procedural_visual_handoff.md`
> Evidência principal: `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs`

---

## 1. /speckit.specify

### O que existe
Cave runtime materializa visuais fallback, inimigos visíveis, câmera follow e elementos visuais de cave procedural.

### Por que existe
Torna a cave procedural inspecionável e jogável no MVP sem arte final.

### Fora de escopo
Não representa arte final, tileset final, VFX ou polish visual.

---

## 2. /speckit.plan

### Arquitetura real
CaveRuntimeMaterializer, CameraFollow2D, scripts de Combat e DebugHud.

### Fluxo
Generator/runtime fornece layout; materializer cria chão/parede/nodes/inimigos e câmera acompanha o player.

### Persistência
Visual runtime não é save por si; estado persistido fica em CaveSaveData/snapshots.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Validar visualização e ausência de erro vermelho no Unity.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` | Evidência principal. |
| Histórico | `docs_old/audits/FIX_CAVE_PROCEDURAL_VISUAL_HANDOFF.md` | Fonte histórica preservada. |
| Refinamento | `docs/refinements/implementados/ref_fix_cave_procedural_visual_handoff.md` | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Validar visualização e ausência de erro vermelho no Unity.




