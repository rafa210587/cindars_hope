# Tasks — FASE9E Save Schema, Migration e Persistência

> **Feature:** FASE9E_SAVE_SCHEMA_MIGRATION  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-155 — Save schema version contracts

### Escopo

Criar contratos de versionamento.

### Critérios

- [ ] SaveSchema existe.
- [ ] CurrentVersion existe.
- [ ] SaveData tem SchemaVersion.
- [ ] SaveData tem SavedAtUtc.
- [ ] Save novo grava versão atual.

---

## PR-156 — Save migration v1-v2 equipment/hotbar

### Escopo

Adicionar EquipmentSaveData e HotbarSaveData.

### Critérios

- [ ] EquipmentSaveData existe.
- [ ] HotbarSaveData existe.
- [ ] Hotbar tem 6 slots.
- [ ] LeftHand/RightHand persistem.
- [ ] ActiveSeed persiste.
- [ ] Save antigo recebe defaults.

---

## PR-157 — Save migration v2-v3 attributes/progression/status

### Escopo

Adicionar atributos, progressão e status ativos.

### Critérios

- [ ] Strength/Dexterity/Intelligence default 1.
- [ ] Level default 1.
- [ ] CurrentXp default 0.
- [ ] XpToNextLevel default 100.
- [ ] UnspentAttributePoints default 0.
- [ ] UnspentSkillPoints default 0.
- [ ] ActiveStatusSaveData existe.

---

## PR-158 — Save migration v3-v4 farm/world state

### Escopo

Adicionar FarmSaveData.

### Critérios

- [ ] FarmSaveData existe.
- [ ] FarmPlotSaveData existe.
- [ ] TreeStateSaveData existe.
- [ ] ResourceNodeSaveData existe.
- [ ] Baseline pode ser criado a partir da cena.

---

## PR-159 — Save migration v4-v5 cave state

### Escopo

Adicionar CaveSaveData MVP.

### Critérios

- [ ] CaveSaveData existe.
- [ ] CurrentLayer default 1.
- [ ] OpenedChestIds existe.
- [ ] DepletedNodeIds existe.
- [ ] DefeatedPersistentEnemyIds existe.
- [ ] Regra de respawn de inimigos comuns documentada.

---

## PR-160 — Load validation hardening

### Escopo

Validar save após migration.

### Critérios

- [ ] IDs inválidos limpam equipment slots.
- [ ] Hotbar inválida limpa slot.
- [ ] Ammo slot sem item no inventory é limpo.
- [ ] Progression inválida recebe default seguro.
- [ ] Pickup com item inválido é ignorado com warning.
- [ ] Load não quebra por item inválido isolado.

---

## PR-161 — Save/load smoke tests handoff

### Escopo

Documentar smoke tests e handoff.

### Critérios

- [ ] Handoff cobre save antigo.
- [ ] Handoff cobre equipment/hotbar.
- [ ] Handoff cobre progression defaults.
- [ ] Handoff cobre status.
- [ ] Handoff cobre pickups/drop.
- [ ] Handoff cobre farm.
- [ ] Handoff cobre cave respawn rule.

---

## Smoke test final

- [ ] Criar save v1 antigo/fake.
- [ ] Carregar e migrar para versão atual.
- [ ] Confirmar defaults de Equipment/Hotbar.
- [ ] Confirmar defaults de Level/XP/pontos.
- [ ] Salvar e carregar hotbar/mãos.
- [ ] Salvar e carregar status do player.
- [ ] Dropar item e recarregar.
- [ ] Coletar pickup e recarregar.
- [ ] Alterar farm plot e recarregar.
- [ ] Entrar na cave e validar respawn de inimigos comuns.
- [ ] Console sem erro vermelho.

