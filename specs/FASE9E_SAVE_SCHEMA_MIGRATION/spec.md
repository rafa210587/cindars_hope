# SpecKit â€” FASE9E Save Schema, Migration e PersistÃªncia

> **Feature:** FASE9E_SAVE_SCHEMA_MIGRATION  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero que meu save continue carregando mesmo apÃ³s novas versÃµes do jogo. Como dev, quero evoluir o schema de save sem quebrar dados antigos nem salvar referÃªncias Unity invÃ¡lidas.

---

## 2. Objetivos funcionais

### O1 â€” SchemaVersion

Todo save novo deve ter versÃ£o explÃ­cita.

### O2 â€” Backwards compatibility

Saves antigos sem campos novos devem carregar com defaults seguros.

### O3 â€” Migration pipeline

MudanÃ§as de schema devem migrar versÃ£o por versÃ£o.

### O4 â€” IDs estÃ¡veis

Save deve persistir IDs e tipos simples, nunca referÃªncias Unity.

### O5 â€” Equipment/Hotbar

Persistir equipment, hotbar, mÃ£os, active seed, ammo e consumÃ­vel selecionado.

### O6 â€” Progression

Persistir Level, CurrentXp, XpToNextLevel, UnspentAttributePoints e UnspentSkillPoints.

### O7 â€” Status

Persistir status ativos do player e de entidades persistentes futuras.

### O8 â€” Pickups/Farm/Cave

Persistir pickups, drops do jogador, farm state e cave state MVP.

---

## 3. Non-goals

Fora de escopo:

- cloud save;
- criptografia;
- mÃºltiplos slots avanÃ§ados;
- save binÃ¡rio;
- save incremental;
- persistÃªncia total de todo objeto dinÃ¢mico;
- procedural cave completo.

---

## 4. Regras de negÃ³cio

### R1 â€” NÃ£o salvar Unity refs

NÃ£o salvar GameObject, Transform, MonoBehaviour, ScriptableObject, Sprite, Collider ou Rigidbody.

### R2 â€” Load tolerante

Campo ausente usa default seguro.

### R3 â€” ID invÃ¡lido

ID invÃ¡lido limpa slot/entrada ou ignora item com warning. Load nÃ£o quebra inteiro.

### R4 â€” Ammo vazio

Ammo slot Ã© limpo se item de ammo nÃ£o existe ou quantidade chega a 0.

### R5 â€” Bow vence Magic

Se Bow e Magic forem salvos ao mesmo tempo, Bow vence e Magic Ã© limpo.

### R6 â€” Cave mobs

Inimigos comuns da cave respawnam ao entrar novamente ou no novo dia. HP/status deles nÃ£o salva no MVP.

### R7 â€” Player status

Status do player salva com duraÃ§Ã£o restante.

### R8 â€” Progression default

Level = 1, CurrentXp = 0, XpToNextLevel = 100, pontos nÃ£o gastos = 0.

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

## 6. CritÃ©rios de aceite

### CA1 â€” Versionamento

Todo save novo tem SchemaVersion atual.

### CA2 â€” Save antigo

Save antigo sem Equipment/Hotbar/Status/Progression carrega.

### CA3 â€” Migration

Migration roda versÃ£o por versÃ£o.

### CA4 â€” IDs invÃ¡lidos

IDs invÃ¡lidos geram warning e nÃ£o quebram load.

### CA5 â€” Equipment/Hotbar

Equipment, hotbar 6 slots, LeftHand, RightHand e ActiveSeed persistem.

### CA6 â€” Progression

Level, XP e pontos persistem com defaults seguros.

### CA7 â€” Status

Player status salva/restaura duraÃ§Ã£o restante.

### CA8 â€” Pickups

Pickups e drops do jogador persistem.

### CA9 â€” Farm

Plots, crops, watered/tilled state e trees persistem.

### CA10 â€” Cave

Cave state MVP tem regra clara de respawn e listas persistentes futuras.

---

## 7. DependÃªncias

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
- `docs_old/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md` estiver lida.
- Estado real em `dev` tiver sido validado.

