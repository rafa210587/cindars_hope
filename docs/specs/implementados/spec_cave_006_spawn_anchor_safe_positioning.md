# SPEC CAVE-006 — Spawn anchor e safe positioning

> Status: Implementado em código — validação Unity pendente
> Camada: Cave
> Fonte histórica: $source
> Refinamento relacionado: $ref
> Evidência principal: $evidence

---

## 1. /speckit.specify

### O que existe
Spawn na cave usa lookup de tile seguro e âncoras para evitar nascer fora do walkable ou exatamente no portal.

### Por que existe
Reduz bugs de entrada/saída em níveis procedurais.

### Fora de escopo
Não cobre pathfinding completo nem todos os casos finais de layout.

---

## 2. /speckit.plan

### Arquitetura real
CaveLevelRuntimeController, CaveEntryController e CaveExitPortal.

### Fluxo
Entrada/saída escolhe âncora, resolve grid seguro próximo e posiciona player no walkable.

### Persistência
Persistência depende do estado de cave/save; spawn anchor em si é runtime.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Validar Farm→Cave, Cave→Farm e backtracking no Unity.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | $evidence | Evidência principal. |
| Histórico | $source | Fonte histórica preservada. |
| Refinamento | $ref | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Validar Farm→Cave, Cave→Farm e backtracking no Unity.
