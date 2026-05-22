# SPEC CAVE-005 — Visual runtime, camera e enemy visuals

> Status: Implementado em código — validação Unity pendente
> Camada: Cave
> Fonte histórica: $source
> Refinamento relacionado: $ref
> Evidência principal: $evidence

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
| Código | $evidence | Evidência principal. |
| Histórico | $source | Fonte histórica preservada. |
| Refinamento | $ref | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Validar visualização e ausência de erro vermelho no Unity.
