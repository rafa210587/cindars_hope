# SPEC CAVE-008 — Debug skip, confinement e wall distance hardening

> Status: Implementado em código — validação Unity pendente
> Camada: Cave
> Fonte histórica: `docs_old/audits/FIX_CAVE_DEBUG_SKIP_AND_CONFINEMENT_TOLERANCE.md`
> Refinamento relacionado: `docs/refinements/implementados/ref_fix_cave_debug_skip_and_confinement_tolerance.md`
> Evidência principal: `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs`

---

## 1. /speckit.specify

### O que existe
Hardening inclui debug skip, tolerância de confinamento, reset/injeção de registry, wall distance e HUD gate debug.

### Por que existe
Ajuda QA de níveis profundos e evita que player saia do caminho procedural.

### Fora de escopo
Não é UI final de debug nem substitui progressão normal da cave.

---

## 2. /speckit.plan

### Arquitetura real
CaveDebugLevelSkipController quando presente, CavePlayerPathConfinement, CaveBossSpawner, CaveBossDefeatMonitor e DebugHud.

### Fluxo
Comando debug avança nível quando habilitado; confinement limita player a tiles válidos e sistemas de boss/gate respeitam distâncias/estado.

### Persistência
Persistência se limita ao estado de cave/boss/checkpoints quando aplicável.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Validar debug skip, confinement e boss gates em Play Mode.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs` | Evidência principal. |
| Histórico | `docs_old/audits/FIX_CAVE_DEBUG_SKIP_AND_CONFINEMENT_TOLERANCE.md` | Fonte histórica preservada. |
| Refinamento | `docs/refinements/implementados/ref_fix_cave_debug_skip_and_confinement_tolerance.md` | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Validar debug skip, confinement e boss gates em Play Mode.




