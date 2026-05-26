# refinamento_init_cave_entry_death_anya_corpse_recovery

> Status: **IMPLEMENTADO** — 2026-05-25
> ⚠️ **MOVIDO** — Versão ativa em: `docs/refinements/implementados/ref_cave_entry_death_anya_corpse_recovery.md`
> Spec relacionada: `docs/specs/implementados/spec_cave_entry_death_anya_corpse_recovery.md`
> Objetivo: completar fluxo de morte na cave, respawn na Fonte de Anya, corpse recovery do ultimo corpo, perda/recuperacao de itens/equipment/gold, perda de XP para inicio do nivel atual, integracao com redistribuicao de inimigos da cave e save/load.

---

## 1. Estado atual

A execucao overnight criou dados/configuracoes iniciais para cave entry, death handler e corpse recovery, mas o fluxo runtime completo ainda nao esta validado.

Evidencias principais:

```text
Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs
Assets/_Game/Scripts/Player/Death/**
docs/specs/implementados/spec_fase9j_cave_entry_loadout_death_anya_corpse.md
```

---

## 2. Decisoes aprovadas

- Nao criar novo menu de entrada/loadout da cave nesta spec.
- Entrada/checkpoints usam o portal e menu lateral da spec 14.
- Ao morrer na cave, o jogador perde:

```text
Todos os itens carregados no inventory
Todos os equipamentos equipados
Todos os itens em hotbar/maos/equipment bindings
Todo o gold carregado
XP progress do nivel atual
```

- Itens/equipamentos/gold vao para o corpse e podem ser recuperados.
- XP perdido nao e recuperado pelo corpse.
- XP volta para o inicio do nivel atual.
- Apenas o ultimo corpse ativo existe.
- Se o jogador morrer de novo antes de recuperar o corpse anterior:

```text
Corpse anterior desaparece.
Conteudo anterior e perdido.
Novo corpse e criado com o novo conteudo perdido.
```

- Corpse desaparece somente quando todo conteudo recuperavel for recuperado ou quando for substituido por nova morte.
- Se inventory estiver cheio ao recuperar, recuperacao pode ser parcial e o restante permanece no corpse.
- Respawn ocorre na Fonte de Anya.
- Fonte de Anya fica na fazenda/proxima da entrada da cave.
- Fonte de Anya nao implementa respec real nesta spec; respec fica para spec 16.
- Morte fora da cave nao cria corpse e nao aplica perda no MVP.
- Morte na cave dispara redistribuicao de inimigos da spec 14 sem trocar EnemyIds.
- Nao aplicar dano extra de durabilidade na morte.

---

## 3. Gaps

- Morte na cave ainda nao possui regra runtime consolidada de perda total.
- Corpse recovery nao esta garantido como unico ultimo corpo recuperavel.
- Segunda morte antes de recuperar precisa substituir o corpo anterior e perder conteudo antigo.
- Fonte de Anya precisa ser ponto real de respawn.
- Inventario/equipamento/gold precisam ser removidos e restaurados de forma transacional.
- XP precisa voltar para inicio do nivel atual.
- Morte precisa disparar redistribuicao de inimigos da spec 14.
- Save/load precisa preservar corpse ativo, conteudo, posicao, cave run e estado recuperado.
- Morte fora da cave precisa ter comportamento diferente e seguro.

---

## 4. Cave entry e checkpoints

Esta spec nao cria UI propria de entrada/loadout.

Regras:

- Entrada normal da cave continua sendo responsabilidade do runtime/portal ja existente.
- Checkpoint portal e menu lateral continuam responsabilidade da spec 14.
- Esta spec pode reagir a eventos de entrada/saida somente para registrar contexto de morte/corpse.
- Nao duplicar `CaveCheckpointSideMenu`.
- Nao criar modal de loadout obrigatorio.

Eventos de contexto que podem ser consumidos:

```text
CaveRunStartedEvent
CaveRunLoadedEvent
CaveCheckpointTeleportCompletedEvent
CaveLevelMaterializedEvent
```

---

## 5. Death policy na cave

Fluxo:

```text
Player HP chega a 0 dentro da cave
Publicar PlayerDiedEvent
CaveDeathResolver identifica contexto Cave
Criar/substituir corpse ativo
Mover inventory + equipment + gold para corpse
Reduzir XP para inicio do nivel atual
Marcar run death state
Solicitar redistribuicao de inimigos da cave
Respawnar player na Fonte de Anya
Salvar estado
```

Regras:

- Operacao deve ser transacional: nao perder item sem registrar no corpse, salvo substituicao explicita por nova morte.
- Equipamentos equipados devem ser removidos dos slots de equipment e movidos para o corpse.
- Inventory deve ficar vazio apos morte na cave, exceto se houver itens tecnicos nao removiveis exigidos pelo runtime; se houver, registrar excecao explicitamente.
- Equipment bindings devem ser limpos.
- Gold do player deve ir para 0.
- XP progress do nivel atual deve ir para 0 ou valor inicial daquele nivel.
- Player level nao reduz no MVP, salvo se o sistema de progressao ja exigir outra regra explicita.
- XP perdido nao fica no corpse e nao volta na recovery.

---

## 6. XP penalty

Regra:

```text
Ao morrer na cave, CurrentLevelXpProgress = 0.
Level atual e mantido.
Total XP deve ser ajustado para o valor minimo do nivel atual, se o sistema usar TotalXP.
```

Campos/conceitos esperados:

```text
CurrentPlayerLevel
CurrentLevelXpProgress
TotalXP
LevelStartTotalXP
```

Se o sistema de XP ainda nao existir:

- criar DTO/contrato minimo para futura integracao;
- nao marcar XP penalty como completo;
- registrar pendencia clara.

---

## 7. Corpse ativo unico

Regra central:

```text
Apenas 1 corpse ativo por jogador/save.
```

Se novo corpse for criado enquanto ja existe corpse ativo:

```text
Corpse antigo recebe estado Replaced.
Conteudo antigo e perdido.
Corpse antigo desaparece do mundo.
Novo corpse vira Active.
Publicar CorpseReplacedEvent.
```

Estados de corpse:

```text
None
Active
PartiallyRecovered
Recovered
Replaced
ExpiredDebugOnly opcional
```

Dados minimos:

```text
CorpseId
CorpseStatus
RunId
CaveSeed
CaveLevel
SnapshotLayoutHash
SceneName
Position
SafeAnchorId opcional
GoldAmount
InventoryItems[]
EquipmentItems[]
CreatedAtGameDay
CreatedAtGameTime opcional
RecoveredAtGameDay opcional
ReplacedByCorpseId opcional
```

---

## 8. Conteudo do corpse

Corpse deve guardar:

```text
GoldAmount
InventoryItems[]
EquipmentItems[]
HotbarBoundItems[] opcional se necessario
ItemInstanceData[] para equipaveis/duraveis
```

Regras:

- Stackables preservam `ItemId + Amount`.
- Equipaveis/duraveis preservam `ItemInstanceId`, `ItemId`, durability, broken state e modifiers se existirem.
- Equipment slot original pode ser registrado apenas para restaurar bindings se desejado, mas recovery minima pode devolver ao inventory.
- Se o inventory nao tiver espaco para tudo ao recuperar, devolver o que couber e manter o restante no corpse.
- Corpse so marca `Recovered` quando gold e todos os itens recuperaveis forem devolvidos.
- XP nao entra no corpse.

---

## 9. Local do corpse

Corpse deve aparecer no local da morte ou em safe anchor proximo.

Dados:

```text
CaveLevel
Position
SafeAnchorId opcional
RunId
CaveSeed
SnapshotLayoutHash
```

Regras:

- Se posicao de morte for invalida, usar nearest safe anchor da spec 14.
- Corpse deve ser materializado ao revisitar o nivel/snapshot correto.
- Corpse nao deve spawnar dentro de parede ou fora da area navegavel.
- Corpse deve respeitar cave snapshot/replay.
- Corpse no nivel 23 exige voltar ao nivel 23 para recuperar, mesmo que o jogador use checkpoint 15/30 etc.

---

## 10. Corpse recovery

Interacao:

```text
Player pressiona E no CorpseInteractable
Abrir mini modal de recovery
Opcoes: Recuperar, Sair
```

Regras:

- Recuperar tenta devolver gold e itens.
- Gold deve ser devolvido mesmo se inventory estiver cheio.
- Itens so sao removidos do corpse quando adicionados com sucesso ao inventory/equipment storage.
- Se inventory estiver cheio, recovery e parcial.
- Recovery parcial publica `CorpsePartiallyRecoveredEvent`.
- Recovery completa publica `CorpseRecoveredEvent` e remove corpse do mundo.
- Recuperar corpse nao restaura XP perdido.
- Recuperar corpse nao desfaz redistribuicao de inimigos.

---

## 11. Fonte de Anya

Criar/usar:

```text
AnyaFountain
AnyaRespawnPoint
AnyaRespawnService
AnyaFountainInteractable
AnyaFountainMenu
```

Local:

```text
Fazenda / area externa da cave / proxima da entrada da caverna
```

Funcoes MVP:

```text
Respawn point apos morte na cave
Menu/interacao minima
Acesso futuro a respec da spec 16 bloqueado/desabilitado
Opcao de voltar/entrar na cave se integracao existir
```

Menu minimo:

```text
Retomar cave / ir para checkpoint portal se aplicavel
Respec futuro bloqueado
Sair
```

Regras:

- Fonte de Anya deve ter ID estavel.
- Respawn usa posicao/anchor configurado, nao `GameObject.Find()`.
- Interacao usa modal stack.
- Fonte nao duplica checkpoint menu da spec 14; pode encaminhar/abrir fluxo existente se disponivel.

---

## 12. Respawn state

Ao respawnar na Fonte de Anya apos morte na cave:

```text
HP = MaxHP
Stamina = MaxStamina
Mana = MaxMana, se ManaManager existir
Hunger nao restaura automaticamente
Status temporarios negativos e positivos sao limpos, salvo regra futura explicita
```

Regras:

- Respawn nao deve deixar player morto, stunado ou sem controle.
- Respawn nao deve recriar itens perdidos fora do corpse.
- Equipment fica vazio ate recuperar/reequipar itens.
- Se MaxMana/ManaManager nao existir ainda, ignorar com seguranca.

---

## 13. Morte fora da cave

MVP:

```text
Morte fora da cave nao cria corpse.
Morte fora da cave nao remove itens/gold/XP.
Respawn em safe spawn point da cena ou Fonte de Anya se configurado.
```

Regras:

- Death resolver deve diferenciar `DeathContext = Cave` vs `NonCave`.
- NonCaveDeathPolicy deve ser simples e segura.
- Futuras penalidades fora da cave ficam fora desta spec.

---

## 14. Save/load

Persistir usando DTOs simples:

```text
DeathSaveData
- ActiveCorpse
- DeathStats

CorpseSaveData
- CorpseId
- CorpseStatus
- RunId
- CaveSeed
- CaveLevel
- SnapshotLayoutHash
- SceneName
- Position
- SafeAnchorId
- GoldAmount
- InventoryItems[]
- EquipmentItems[]
- CreatedAtGameDay
- CreatedAtGameTime
- RecoveredAtGameDay
- ReplacedByCorpseId

CorpseItemSaveData
- ItemId
- Amount
- ItemInstanceId opcional
- DurabilityCurrent opcional
- DurabilityMax opcional
- IsBroken opcional
- SourceSlotType opcional
- SourceSlotIndex opcional

DeathStatsSaveData
- TotalCaveDeaths
- LastDeathAtGameDay
- LastDeathAtCaveLevel
- LastCorpseId
```

Nunca serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

---

## 15. Eventos

Usar/criar eventos oficiais:

```text
PlayerDiedEvent
PlayerRespawnRequestedEvent
PlayerRespawnCompletedEvent
CavePlayerDeathStartedEvent
CavePlayerDeathResolvedEvent
CorpseCreatedEvent
CorpseReplacedEvent
CorpseRecoveryStartedEvent
CorpsePartiallyRecoveredEvent
CorpseRecoveredEvent
CorpseRecoveryFailedEvent
AnyaRespawnRequestedEvent
AnyaRespawnCompletedEvent
AnyaFountainOpenedEvent
CaveEnemiesRedistributionRequestedEvent
XpResetToLevelStartEvent
```

---

## 16. Definition of Done

- [ ] Nao existe novo menu de entrada/loadout obrigatorio da cave nesta spec.
- [ ] Morte na cave cria corpse recuperavel no local/safe anchor correto.
- [ ] Ao morrer na cave, inventory, equipment, hotbar/equipment bindings e gold sao movidos para o corpse.
- [ ] XP volta para inicio do nivel atual e nao e recuperavel pelo corpse.
- [ ] Gold do player vai para 0 apos morte na cave.
- [ ] Equipment slots ficam vazios apos morte na cave.
- [ ] Apenas o ultimo corpse fica ativo.
- [ ] Morrer de novo antes de recuperar substitui corpse anterior e perde conteudo anterior.
- [ ] Corpse recovery devolve gold e itens recuperaveis.
- [ ] Recovery parcial funciona quando inventory esta cheio.
- [ ] Corpse so desaparece quando completamente recuperado ou substituido.
- [ ] Respawn acontece na Fonte de Anya.
- [ ] HP/Stamina/Mana restauram no respawn; Hunger nao restaura automaticamente.
- [ ] Morte fora da cave nao cria corpse e nao remove itens/gold/XP no MVP.
- [ ] Morte na cave dispara redistribuicao de inimigos da spec 14.
- [ ] Save/load preserva corpse ativo, conteudo, status, posicao, run, cave level e death stats.
- [ ] Invariantes anti-regressao preservadas.

---

## 17. Validacao

1. Entrar na cave com inventory, equipamentos equipados, gold e XP parcial no nivel atual.
2. Forcar morte na cave.
3. Validar respawn na Fonte de Anya.
4. Validar HP/Stamina/Mana restaurados e Hunger preservada.
5. Validar inventory vazio, equipment vazio e gold 0.
6. Validar XP no inicio do nivel atual.
7. Validar corpse criado no cave level/posicao/safe anchor correto.
8. Salvar/carregar antes de recuperar e validar corpse persistente.
9. Recuperar corpse com inventory vazio e validar retorno de itens/equipment/gold, sem XP.
10. Forcar morte e depois morrer novamente antes de recuperar.
11. Validar que corpse antigo desapareceu e conteudo antigo foi perdido.
12. Validar que apenas o novo corpse esta ativo.
13. Tentar recuperar corpse com inventory cheio e validar recovery parcial.
14. Esvaziar espaco e completar recovery, validando remocao do corpse.
15. Validar que morte na cave publica evento de redistribuicao de inimigos.
16. Forcar morte fora da cave e validar sem corpse/perda no MVP.
17. Validar que nenhum DTO serializa referencia Unity.
18. Validar Unity compile validation e docs validation.
