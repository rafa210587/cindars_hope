# Plan — FASE9E Save Schema, Migration e Persistência

> **Feature:** FASE9E_SAVE_SCHEMA_MIGRATION  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar versionamento e migração antes de expandir gameplay dependente de save.

Ordem:

1. SaveSchema e SchemaVersion;
2. migration pipeline base;
3. Equipment/Hotbar migration;
4. Attributes/Progression/Status migration;
5. Farm/world state migration;
6. Cave state migration;
7. load validation hardening;
8. smoke tests/handoff.

---

## 2. Arquitetura proposta

### Novas classes/DTOs

- `SaveSchema`
- `SaveMigrationPipeline`
- `PlayerProgressionSaveData`
- `EquipmentSaveData`
- `HotbarSaveData`
- `ActiveStatusSaveData`
- `FarmSaveData`
- `FarmPlotSaveData`
- `TreeStateSaveData`
- `ResourceNodeSaveData`
- `CaveSaveData`

### SaveData vNext

`SaveData` deve ter SchemaVersion e campos opcionais tolerantes a null.

---

## 3. Migrações

### v1 -> v2

Adicionar Equipment e Hotbar com defaults.

### v2 -> v3

Adicionar attributes, Level/XP/pontos e ActiveStatuses.

### v3 -> v4

Adicionar Farm/world state.

### v4 -> v5

Adicionar Cave state MVP.

---

## 4. Validação pós-load

Validar:

- item IDs;
- equipment IDs;
- hotbar slots;
- hand slots;
- active seed;
- selected consumable;
- pickups;
- status;
- progression fields.

Erros devem gerar warning e limpar/ignorar entrada, não quebrar load.

---

## 5. Riscos

| Risco | Mitigação |
|---|---|
| Save antigo quebrar | defaults e migration incremental |
| IDs inválidos travarem load | validator pós-load |
| Campo novo null | DTOs opcionais e defaults |
| Progression ainda sem sistema | salvar defaults até spec de level up |
| Split real exigir stacks | manter agregado até feature de split parcial |

---

## 6. Testes manuais mínimos

- Carregar save v1 sem Equipment.
- Migrar para versão atual.
- Salvar e recarregar Equipment/Hotbar.
- Salvar e recarregar Level/XP defaults.
- Salvar e recarregar status do player.
- Coletar pickup e recarregar.
- Dropar item e recarregar.
- Alterar farm plot e recarregar.
- Entrar na cave e confirmar regra de respawn.

---

## 7. Fora de escopo técnico

- cloud save;
- criptografia;
- múltiplos slots avançados;
- save binário;
- save incremental;
- progression formula completa;
- UI de level up.

