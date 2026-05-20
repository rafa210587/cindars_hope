# SpecKit — FASE9E Save Schema, Migration e Persistência

> **Feature:** FASE9E_SAVE_SCHEMA_MIGRATION  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero que meu save continue carregando mesmo após novas versões do jogo. Como dev, quero evoluir o schema de save sem quebrar dados antigos nem salvar referências Unity inválidas.

---

## 2. Objetivos funcionais

### O1 — SchemaVersion

Todo save novo deve ter versão explícita.

### O2 — Backwards compatibility

Saves antigos sem campos novos devem carregar com defaults seguros.

### O3 — Migration pipeline

Mudanças de schema devem migrar versão por versão.

### O4 — IDs estáveis

Save deve persistir IDs e tipos simples, nunca referências Unity.

### O5 — Equipment/Hotbar

Persistir equipment, hotbar, mãos, active seed, ammo e consumível selecionado.

### O6 — Progression

Persistir Level, CurrentXp, XpToNextLevel, UnspentAttributePoints e UnspentSkillPoints.

### O7 — Status

Persistir status ativos do player e de entidades persistentes futuras.

### O8 — Pickups/Farm/Cave

Persistir pickups, drops do jogador, farm state e cave state MVP.

---

## 3. Non-goals

Fora de escopo:

- cloud save;
- criptografia;
- múltiplos slots avançados;
- save binário;
- save incremental;
- persistência total de todo objeto dinâmico;
- procedural cave completo.

---

## 4. Regras de negócio

### R1 — Não salvar Unity refs

Não salvar GameObject, Transform, MonoBehaviour, ScriptableObject, Sprite, Collider ou Rigidbody.

### R2 — Load tolerante

Campo ausente usa default seguro.

### R3 — ID inválido

ID inválido limpa slot/entrada ou ignora item com warning. Load não quebra inteiro.

### R4 — Ammo vazio

Ammo slot é limpo se item de ammo não existe ou quantidade chega a 0.

### R5 — Bow vence Magic

Se Bow e Magic forem salvos ao mesmo tempo, Bow vence e Magic é limpo.

### R6 — Cave mobs

Inimigos comuns da cave respawnam ao entrar novamente ou no novo dia. HP/status deles não salva no MVP.

### R7 — Player status

Status do player salva com duração restante.

### R8 — Progression default

Level = 1, CurrentXp = 0, XpToNextLevel = 100, pontos não gastos = 0.

---

## 5. Entidades funcionais

- `SaveSchema`
- `SaveMigrationPipeline`
- `SaveData`
- `PlayerSaveData`
- `PlayerProgressionSaveData`
- `EquipmentSaveData`
- `HotbarSaveData`
- `PickupSaveData`
- `ActiveStatusSaveData`
- `FarmSaveData`
- `CaveSaveData`

---

## 6. Critérios de aceite

### CA1 — Versionamento

Todo save novo tem SchemaVersion atual.

### CA2 — Save antigo

Save antigo sem Equipment/Hotbar/Status/Progression carrega.

### CA3 — Migration

Migration roda versão por versão.

### CA4 — IDs inválidos

IDs inválidos geram warning e não quebram load.

### CA5 — Equipment/Hotbar

Equipment, hotbar 6 slots, LeftHand, RightHand e ActiveSeed persistem.

### CA6 — Progression

Level, XP e pontos persistem com defaults seguros.

### CA7 — Status

Player status salva/restaura duração restante.

### CA8 — Pickups

Pickups e drops do jogador persistem.

### CA9 — Farm

Plots, crops, watered/tilled state e trees persistem.

### CA10 — Cave

Cave state MVP tem regra clara de respawn e listas persistentes futuras.

---

## 7. Dependências

- `SaveManager`
- `SaveData`
- `InventoryManager`
- `EquipmentManager`
- `HotbarSelectionData`
- `ItemPickup`
- `PlayerManager`
- `EnemyStatusReceiver`
- `FarmPlot`
- `TreeNode`
- `ResourceNode` futuro
- `Cave` futuro

---

## 8. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md` estiver lida.
- Estado real em `dev` tiver sido validado.
