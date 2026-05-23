# SPEC COMBAT-002 — Enemy data-driven stats

> Status: Implementado parcial
> Camada: Combat
> Fonte histórica: `docs_old/FASE9B3_ENEMY_DATA_DRIVEN_STATS_v1.0.md`
> Refinamento relacionado: `docs/refinements/implementados/ref_pr130_reconciliacao_handoff.md`
> Evidência principal: `Assets/_Game/Scripts/Combat/EnemyDataSO.cs`

---

## 1. /speckit.specify

### O que existe
Slime e inimigos base usam dados para HP, damage, XP/drop e parâmetros de contato/chase conforme suporte atual.

### Por que existe
Tira balanceamento de MonoBehaviours e prepara a arquitetura de múltiplos monstros.

### Fora de escopo
IA completa, 40 monstros finais e roles avançadas ainda são futuros.

---

## 2. /speckit.plan

### Arquitetura real
EnemyDataSO, EnemyDatabaseSO quando presente, assets em Assets/_Game/Data/Combat e scripts EnemyHealth/EnemyContactDamage/EnemyChaseController.

### Fluxo
Dados alimentam componentes de combate; morte/drops/XP usam os campos disponíveis.

### Persistência
Stats de inimigo são dados; estado runtime individual ainda depende do combate/cave.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Completar AI/actions/status por specs FASE9D/FASE9G futuras.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` | Evidência principal. |
| Histórico | `docs_old/FASE9B3_ENEMY_DATA_DRIVEN_STATS_v1.0.md` | Fonte histórica preservada. |
| Refinamento | `docs/refinements/implementados/ref_pr130_reconciliacao_handoff.md` | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Completar AI/actions/status por specs FASE9D/FASE9G futuras.




