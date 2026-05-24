# SPEC - Cave entry, death, Anya e corpse recovery

> Spec ID: spec_cave_entry_death_anya_corpse_recovery
> Status: A implementar
> Ordem de execucao: 15
> Depende de: 00-14
> Bloqueia: 16, 17
> Tipo: Runtime/UI minima
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar fluxo de morte na cave, respawn na Fonte de Anya, corpse recovery do ultimo corpo, perda/recuperacao de itens/equipment/gold, perda de XP para inicio do nivel atual, integracao com redistribuicao de inimigos da cave e save/load.
> Fora de escopo: criar novo menu de entrada da cave, substituir checkpoint portal da spec 14, respec real, skill trees, UI final, balance final de penalidades, quest items complexos, multiplayer, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_cave_entry_death_anya_corpse_recovery.md

---

# /speckit.specify

## Contexto

A entrada/teleporte/checkpoint da cave ja foi definida na spec 14:

```text
- Portal de checkpoint ao lado da entrada da caverna na fazenda.
- Portal de checkpoint nos niveis 15/30/45/60/75/90 ao lado do portal da cave.
- Portal abre menu lateral para escolher checkpoint desbloqueado.
```

Portanto, esta spec 15 nao cria um novo menu de entrada/loadout antes da cave. O foco aqui e morte, respawn, Fonte de Anya, corpse recovery e penalidades.

Evidencias/parciais atuais:

```text
Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs
Assets/_Game/Scripts/Player/Death/**
docs/specs/implementados/spec_fase9j_cave_entry_loadout_death_anya_corpse.md
```

Specs anteriores relevantes:

```text
02 - save schema migration
03 - inventory slots/capacity/UI
09 - hunger/stamina/status/time
10 - equipment/durability/environment/loot
11 - damage/status/elements/resistances
12 - player combat/weapons/spells
13 - enemy AI/roster/bestiary/faction locks
14 - cave runtime/checkpoints/boss gates/snapshots/respawn
```

## Pre-condicoes

Implementar runtime somente depois de specs 02-14 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
09 - hunger/stamina/status/time
10 - equipment/durability/environment/loot
11 - damage/status/elements/resistances
12 - player combat/weapons/spells/skill actions
13 - enemy AI/roster/bestiary/faction locks
14 - cave runtime generation/checkpoints/boss gates
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Player/Death/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/Death/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Death/**
Assets/_Game/Data/Cave/**
```

Se progression/XP ainda nao existir como sistema formal, criar contrato minimo seguro ou registrar bloqueio sem criar progressao paralela incompatível.

## Problema

Gaps atuais:

- morte na cave ainda nao tem regra consolidada de perda;
- corpse recovery nao esta garantido como unico ultimo corpo recuperavel;
- segunda morte antes de recuperar ainda nao tem politica final;
- Fonte de Anya precisa ser ponto real de respawn;
- inventario/equipamento/gold precisam ser removidos e restaurados de forma transacional;
- XP precisa voltar para inicio do nivel atual;
- morte precisa disparar redistribuicao de inimigos da spec 14;
- save/load precisa preservar corpse ativo, conteudo, posicao, cave run e estado recuperado;
- morte fora da cave precisa ter comportamento diferente e seguro.

## Objetivo

Implementar death/corpse MVP com:

- morte na cave criando corpse recuperavel;
- perda de todos os itens carregados, equipamentos equipados e gold para o corpse;
- XP voltando para o inicio do nivel atual;
- apenas o ultimo corpse ativo;
- morrer novamente antes de recuperar substitui o corpse anterior e o conteudo anterior desaparece;
- recuperar corpse devolve todos os itens/equipamentos/gold possiveis, menos XP;
- corpse persiste por save/load;
- respawn na Fonte de Anya;
- HP/Stamina/Mana restaurados no respawn;
- Hunger nao restaurada automaticamente;
- inimigos da cave redistribuidos por evento da spec 14;
- morte fora da cave sem corpse/perda no MVP.

## Decisoes aprovadas

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

## Cave entry e checkpoints

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

## Death policy na cave

Criar/usar:

```text
PlayerDeathController
CaveDeathPolicy
CaveDeathResolver
CorpseRecoveryManager
CorpseInteractable
AnyaRespawnService
```

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

## XP penalty

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

## Corpse ativo unico

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

## Conteudo do corpse

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

## Local do corpse

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

## Corpse recovery

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

## Fonte de Anya

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

## Respawn state

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

## Morte fora da cave

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

## Integracao com cave runtime da spec 14

Ao resolver morte na cave:

- publicar evento para redistribuir inimigos;
- manter RunId e snapshots;
- nao apagar checkpoints desbloqueados;
- nao reabrir boss gates completed;
- nao resetar boss unique rewards claimed;
- corpse deve ser salvo no contexto da run/snapshot.

Eventos esperados:

```text
CaveEnemiesRedistributionRequestedEvent
CavePlayerDeathResolvedEvent
```

A redistribuicao real dos inimigos e responsabilidade da spec 14.

## Save/load

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

Regras:

- Nunca serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.
- Se corpse salvo referencia level/snapshot inexistente, logar erro e nao crashar.
- Se item salvo no corpse tem ItemId desconhecido, manter dado cru se possivel ou registrar warning sem sumir silenciosamente.
- Save migration da spec 02 deve incluir novos campos.

## Eventos

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

Nao duplicar eventos equivalentes se ja existirem.

## UI minima

- Corpse recovery mini modal entra agora.
- AnyaFountainMenu minimo entra agora.
- UI final fica spec 17.
- Nao criar cave loadout UI.
- Nao duplicar checkpoint side menu da spec 14.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- checkpoint portal/menu lateral da spec 14;
- cave snapshots/run/gates da spec 14;
- enemy respawn/redistribution da spec 14;
- inventory slots/capacity da spec 03;
- equipment bindings/item instances/durability da spec 10;
- health/damage/status da spec 11;
- player combat/mana da spec 12;
- enemy/bestiary/XP hooks da spec 13;
- save migration e DTOs simples da spec 02;
- modal stack;
- GameEventBus;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se uma integracao ainda nao existir, registrar pendencia clara em vez de criar sistema paralelo incompatível.

## Criterios de aceite

- Nao existe novo menu de entrada/loadout obrigatorio da cave nesta spec.
- Morte na cave cria corpse recuperavel no local/safe anchor correto.
- Ao morrer na cave, inventory, equipment, hotbar/equipment bindings e gold sao movidos para o corpse.
- XP volta para inicio do nivel atual e nao e recuperavel pelo corpse.
- Gold do player vai para 0 apos morte na cave.
- Equipment slots ficam vazios apos morte na cave.
- Apenas o ultimo corpse fica ativo.
- Morrer de novo antes de recuperar substitui corpse anterior e perde conteudo anterior.
- Corpse recovery devolve gold e itens recuperaveis.
- Recovery parcial funciona quando inventory esta cheio.
- Corpse so desaparece quando completamente recuperado ou substituido.
- Respawn acontece na Fonte de Anya.
- HP/Stamina/Mana restauram no respawn; Hunger nao restaura automaticamente.
- Morte fora da cave nao cria corpse e nao remove itens/gold/XP no MVP.
- Morte na cave dispara redistribuicao de inimigos da spec 14.
- Save/load preserva corpse ativo, conteudo, status, posicao, run, cave level e death stats.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Player/Death/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/Death/**
Assets/_Game/Scripts/UI/Anya/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Death/**
Assets/_Game/Data/Cave/**
```

Managers/bridges Unity devem ser finos. Death resolution, corpse content transfer, XP reset e recovery devem ficar em classes testaveis quando possivel.

## Ordem segura de implementacao

1. Revalidar sistemas atuais de death, cave context, inventory, equipment, XP/progression e save.
2. Confirmar specs 02-14 implementadas antes de runtime.
3. Criar/ajustar DeathContext e CaveDeathPolicy.
4. Criar transfer transacional inventory/equipment/gold -> corpse.
5. Criar XP reset para inicio do nivel atual.
6. Criar corpse unico ativo e politica de substituicao.
7. Implementar CorpseInteractable + recovery parcial/completa.
8. Implementar Fonte de Anya como respawn point e menu minimo.
9. Integrar respawn state HP/Stamina/Mana/Hunger.
10. Integrar evento de redistribuicao de inimigos da cave.
11. Implementar save/load/migration de corpse/death stats.
12. Validar anti-regressao e atualizar tracking.

## Fluxos

### Morte na cave

```text
PlayerDiedEvent
Resolver DeathContext=Cave
Se corpse ativo existe, marcar Replaced e perder conteudo antigo
Criar novo CorpseId
Mover inventory/equipment/gold para corpse
Limpar inventory/equipment bindings/gold do player
Resetar XP para inicio do nivel atual
Registrar DeathStats
Publicar CaveEnemiesRedistributionRequestedEvent
Respawnar na Fonte de Anya
Salvar estado
```

### Recuperar corpse

```text
Player interage com corpse usando E
Abrir mini modal
Selecionar Recuperar
Adicionar gold ao player
Adicionar itens enquanto houver espaco
Remover do corpse apenas itens adicionados
Se sobrou item, marcar PartiallyRecovered
Se nao sobrou nada, marcar Recovered e remover corpse do mundo
```

### Segunda morte antes de recuperar

```text
Player morre novamente na cave
Corpse antigo Active/PartiallyRecovered -> Replaced
Conteudo antigo perdido
Novo corpse criado com novo conteudo perdido
```

### Morte fora da cave

```text
PlayerDiedEvent
Resolver DeathContext=NonCave
Nao criar corpse
Nao remover inventory/gold/XP
Respawnar em safe point configurado
```

## Riscos de regressao

- Duplicar itens ao morrer ou recuperar.
- Perder itens sem criar corpse por falha intermediaria.
- Equipment binding ficar apontando para item removido.
- Segunda morte deixar dois corpses ativos.
- XP reset afetar level errado.
- Recovery cheia sumir com itens.
- Respawn manter status morto/stunado.
- Morte fora da cave aplicar penalidade errada.

## Mitigacao

- Transfer transacional.
- Corpse unico com status claro.
- Limpeza de equipment bindings apos morte.
- Recovery remove item do corpse somente apos add bem-sucedido.
- Teste de inventory cheio.
- DeathContext explicito.
- Save DTOs simples.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar PlayerDeathController, CorpseRecoveryManager, CaveEntryDataSO, CaveRunManager, Inventory, Equipment, Progression/XP e Save atuais.
- [ ] Confirmar specs 02-14 implementadas antes de runtime.
- [ ] Criar/ajustar `DeathContext` com Cave/NonCave.
- [ ] Criar/ajustar `CaveDeathPolicy`.
- [ ] Implementar transferencia transacional de inventory/equipment/gold para corpse.
- [ ] Implementar reset de XP para inicio do nivel atual.
- [ ] Implementar corpse unico ativo.
- [ ] Implementar substituicao de corpse em segunda morte.
- [ ] Implementar `CorpseInteractable` e recovery mini modal.
- [ ] Implementar recovery parcial quando inventory estiver cheio.
- [ ] Implementar Fonte de Anya como respawn point.
- [ ] Implementar AnyaFountainMenu minimo com respec futuro bloqueado.
- [ ] Implementar respawn state HP/Stamina/Mana restaurados e Hunger preservada.
- [ ] Implementar NonCaveDeathPolicy sem corpse/perda no MVP.
- [ ] Publicar evento de redistribuicao de inimigos da cave.
- [ ] Implementar save/load/migration de corpse e death stats.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Player/Death/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/Death/**
Assets/_Game/Scripts/UI/Anya/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Death/**
Assets/_Game/Data/Cave/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Cave death/corpse/Anya runtime funcional.
- Todos os itens/equipamentos/gold vao para o ultimo corpse.
- XP volta para inicio do nivel atual.
- Segundo death substitui corpse anterior.
- Recovery parcial/completa funciona.
- Morte fora da cave nao penaliza no MVP.
- Save/load preserva corpse/death state.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

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
