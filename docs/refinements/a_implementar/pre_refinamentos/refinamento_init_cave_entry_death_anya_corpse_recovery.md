# refinamento_init_cave_entry_death_anya_corpse_recovery

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_cave_entry_death_anya_corpse_recovery.md`
> **Objetivo:** completar fluxo de entrada da cave, loadout, morte, Fonte de Anya, corpse recovery e penalidades.

---

## 1. Estado atual

A execuÃ§Ã£o overnight criou dados/configuraÃ§Ãµes iniciais para cave entry, death handler e corpse recovery, mas o fluxo runtime completo ainda nÃ£o estÃ¡ validado.

EvidÃªncias principais:

```text
Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs
Assets/_Game/Scripts/Player/Death/**
docs/specs/implementados/spec_fase9j_cave_entry_loadout_death_anya_corpse.md
```

---

## 2. Gaps

- Entrada da cave ainda nÃ£o possui loadout UI/flow final.
- NÃ£o hÃ¡ garantia de que morte em cave cria corpse Ãºnico recuperÃ¡vel.
- Fonte de Anya ainda nÃ£o estÃ¡ integrada como ponto real de respawn/respec/recovery.
- Penalidade de morte precisa ser aplicada exatamente conforme regra configurada.
- Save/load do corpse precisa preservar posiÃ§Ã£o, conteÃºdo, run seed e estado de recuperaÃ§Ã£o.
- Falta validar morte em Farm/Town vs morte na Cave com regras diferentes.

---

## 3. Escopo esperado

### Cave entry

- Abrir fluxo de entrada antes de CaveScene.
- Permitir confirmar loadout/equipamento levado.
- Validar itens bloqueados ou protegidos.
- Registrar cave run context.

### Death flow

Ao morrer na cave:

1. Publicar `PlayerDiedEvent`.
2. Calcular penalidade.
3. Criar corpse recuperÃ¡vel com itens/gold definidos.
4. Respawnar na Fonte de Anya ou ponto configurado.
5. Preservar XP/checkpoints conforme regra.
6. Salvar estado.

### Corpse recovery

- Apenas 1 corpse ativo por jogador/run, salvo regra contrÃ¡ria.
- Interagir com corpse devolve itens/gold guardados.
- Corpse recuperado desaparece e salva estado.
- Se morrer novamente antes de recuperar, polÃ­tica deve ser explÃ­cita: substituir, mesclar ou perder antigo.

### Fonte de Anya

- Respawn point.
- Recovery/respec point futuro.
- UI/interaÃ§Ã£o mÃ­nima.
- Deve ter ID estÃ¡vel e referÃªncia por cena/installer.

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Player/Death/PlayerDeathController.cs
Assets/_Game/Scripts/Player/Death/CorpseRecoveryManager.cs
Assets/_Game/Scripts/Player/Death/CorpseInteractable.cs
Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs
Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/UI/CaveEntry/**
Assets/_Game/Scripts/UI/Death/**
```

---

## 5. Regras de save

Salvar apenas DTOs simples:

```text
CorpseId
SceneName
CaveLevel
Position
ItemId/Amount[]
GoldAmount
CreatedAtDay
Recovered
RunSeed
```

Nunca salvar:

```text
GameObject
Transform
MonoBehaviour
ScriptableObject
```

---

## 6. Definition of Done

- [ ] Morte na cave cria corpse recuperÃ¡vel.
- [ ] Respawn acontece na Fonte de Anya/ponto configurado.
- [ ] Penalidade de morte aplica itens/gold conforme regra.
- [ ] Corpse persiste em save/load.
- [ ] Recuperar corpse devolve conteÃºdo e marca como recovered.
- [ ] Morrer de novo antes de recuperar segue polÃ­tica explÃ­cita.
- [ ] Fluxo nÃ£o quebra se morte ocorrer fora da cave.

---

## 7. ValidaÃ§Ã£o

1. Entrar na cave com itens/gold.
2. ForÃ§ar morte.
3. Validar respawn na Fonte de Anya.
4. Validar corpse criado no nÃ­vel/posiÃ§Ã£o esperados.
5. Salvar/carregar antes de recuperar.
6. Recuperar corpse e validar retorno de itens/gold.
7. Morrer novamente antes de recuperar e validar polÃ­tica configurada.
