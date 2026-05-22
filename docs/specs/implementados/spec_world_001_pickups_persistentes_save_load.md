# SPEC WORLD-001 — Pickups persistentes e save/load

> Status: Implementado parcial
> Camada: World
> Fonte histórica: $source
> Refinamento relacionado: $ref
> Evidência principal: $evidence

---

## 1. /speckit.specify

### O que existe
Pickups persistentes mantêm GameObject no mundo, usam estado coletado e salvam PickupIndex, ItemId, Amount, Position e IsCollected.

### Por que existe
Evita duplicação de coleta e preserva estado do mundo entre save/load.

### Fora de escopo
Não cobre loot table final, respawn avançado ou arte final de pickups.

---

## 2. /speckit.plan

### Arquitetura real
ItemPickup, ItemPickupRegistry, ItemPickupSaveData, WorldSaveData e SaveManager.

### Fluxo
Ao coletar, o pickup marca estado coletado, atualiza inventário e o save captura o estado; no load, o estado coletado é restaurado sem recriar indevidamente o item.

### Persistência
Persiste por DTOs simples em WorldSaveData/ItemPickupSaveData.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Validar F5/F9 e transições em Play Mode.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | $evidence | Evidência principal. |
| Histórico | $source | Fonte histórica preservada. |
| Refinamento | $ref | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Validar F5/F9 e transições em Play Mode.
