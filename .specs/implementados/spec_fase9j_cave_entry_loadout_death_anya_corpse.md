# SPEC FUTURA — FASE9J Cave Entry, Loadout, Death, Anya e Corpse

> Origem histórica: `docs_old/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_SPEC_v1.0.md`
> Status: A implementar
> Observação: conteúdo refinado preservado da documentação antiga.

---

# FASE 9J — Cave Run Entry, Loadout, HUD & Failure Flow Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** `FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW`  
> **Base:** FASE9F Cave Procedural/Resources + FASE9G Bestiary/Faction Locks + FASE9H Equipment/Loot + FASE9I Player Combat.  
> **Objetivo:** definir o fluxo jogável completo da run da cave: entrada, checkpoint, loadout, HUD, saída, item de retorno, morte, Fonte de Anya, corpse recovery, XP loss e persistência de run.

---

## 1. Problema

As specs anteriores definem sistemas importantes, mas ainda falta conectar tudo no loop real da cave:

- como o jogador entra;
- como escolhe checkpoint;
- como prepara loadout;
- como recebe avisos de risco;
- como vê HUD da cave;
- como sai vivo;
- o que acontece ao morrer;
- como recuperar corpo e itens perdidos;
- o que reseta e o que persiste.

Sem essa spec, cave, combat, equipment, save e progression ficam desconectados no gameplay.

---

## 2. User story

Como jogador, quero entrar na cave por checkpoints seguros, preparar meu loadout, entender os riscos, explorar, sair vivo com loot ou recuperar meu corpo após morrer, para que a run da cave tenha tensão, consequência e progressão justa.

---

## 3. Decisões fechadas

```text
D1: Ao morrer na cave, jogador retorna para a Farm na Fonte de Anya.
D2: Ao morrer, jogador perde itens carregados e gold.
D3: O corpo do jogador fica no ponto onde morreu.
D4: O jogador pode resgatar o corpo para recuperar o que perdeu.
D5: Só existe um corpo recuperável: o da última morte.
D6: Se morrer de novo antes de recuperar o corpo anterior, o corpo anterior é perdido/substituído.
D7: Ao morrer, jogador perde XP acumulado no nível atual.
D8: XP volta para o início do nível atual, sem perder level.
D9: Jogador só pode sair da cave por pontos de saída ou item de retorno.
D10: Existe item de retorno para voltar para a entrada/Farm de forma segura.
D11: Entrar por checkpoint começa no level exato do checkpoint.
D12: Todo checkpoint fica numa sala segura.
D13: Jogador pode trocar equipamento dentro da cave livremente.
D14: Hotbar pode ser editada dentro da cave.
D15: Durabilidade quebrada impede uso do item.
D16: Ao sair vivo, a CaveRunSeed continua.
D17: CaveRunSeed só muda em novo jogo ou morte/KO.
D18: Tipos de criaturas, faction locks e ecology gerados continuam os mesmos dentro da mesma run.
D19: Boss derrotado libera checkpoint/avanço imediatamente.
D20: Cave HUD será um OnGUI MVP próprio, separado conceitualmente do DebugHud.
```

---

## 4. Fonte de Anya

A Farm precisa ter um ponto fixo:

```text
Anya Fountain
Fonte de Anya
```

Função:

```text
respawn após morte na cave
ponto narrativo de retorno
eventual cura/restauração futura
```

Fluxo:

```text
Jogador morre na cave
→ feedback de morte
→ itens/gold/equipment/ammo carregados transferidos para corpse recovery
→ XP atual do nível é zerado
→ jogador respawna na Fonte de Anya na Farm
→ CaveRunSeed é regenerada
→ checkpoints permanecem
→ deepest level permanece
→ último corpo recuperável fica registrado
```

---

## 5. Morte e corpse recovery

### 5.1 O que o jogador perde

Ao morrer na cave:

```text
gold carregado
inventário carregado
equipamentos carregados/equipados
ammo
consumíveis
materiais coletados
armas/ferramentas/armaduras/acessórios
```

Tudo isso vai para o corpo recuperável.

### 5.2 O que não deve ser revertido

Para evitar softlock injusto, proteger:

```text
key progression flags
boss defeated flags
unlocked checkpoints
deepest level reached
quest flags
recipes unlocked
schema/system data
attribute points/skill points já ganhos ou gastos
skills já aprendidas
level atual
```

Exemplo:

```text
Derrotou boss e liberou checkpoint 30.
Morreu depois.
Checkpoint 30 continua liberado.
```

### 5.3 XP perdido

Regra:

```text
Ao morrer, XP volta para o início do level atual.
O jogador não perde level.
```

Exemplo:

```text
Level 12
XP atual no level: 340 / 900
Morreu
XP atual no level: 0 / 900
Continua Level 12
```

---

## 6. Corpo recuperável

Criar conceito:

```text
PlayerCorpseRecovery
```

Dados persistidos:

```text
CorpseId
CaveRunSeedAtDeath
CaveLevel
Position ou CorpseRecoveryRoom anchor
LostGold
LostInventoryItems
LostEquipmentItems
LostAmmo
CreatedAtDay
IsRecovered
```

Regra:

```text
Só existe um corpo recuperável.
Nova morte substitui o corpo anterior.
```

Se morrer antes de recuperar:

```text
corpo anterior desaparece
itens/gold do corpo anterior são perdidos
novo corpo é criado no novo ponto de morte
```

### 6.1 Recuperação

Para recuperar:

```text
jogador precisa chegar ao CaveLevel onde morreu
encontrar o corpo
interagir com E
recuperar itens/gold/equipment/ammo
corpo marca IsRecovered = true
```

Hardening:

```text
Se inventário estiver cheio, recuperar parcialmente ou abrir tela de escolha.
MVP pode permitir recuperar tudo ignorando limite temporariamente, mas isso deve ser marcado como débito técnico.
```

---

## 7. CaveRunSeed e corpse após morte

Regra fechada:

```text
CaveRunSeed continua ao sair vivo.
CaveRunSeed muda apenas em novo jogo ou morte/KO.
```

Ponto crítico:

```text
Ao morrer, CaveRunSeed muda.
Mas o corpo precisa continuar recuperável.
```

### 7.1 Opções avaliadas

#### Opção A — manter snapshot do level antigo

```text
Salva o layout antigo do level onde morreu até recuperar o corpo.
```

Mais fiel, mais complexo.

#### Opção B — corpse anchor por level

```text
Após morte, nova CaveRunSeed é criada.
O corpo aparece em uma CorpseRecoveryRoom garantida no mesmo CaveLevel.
```

Mais simples e robusto.

### 7.2 Decisão MVP

```text
MVP usa Opção B: CorpseRecoveryRoom garantida no mesmo CaveLevel.
```

Regra:

```text
Ao gerar o CaveLevel onde há corpse pendente,
o generator força uma sala de corpo acessível.
```

---

## 8. Entrada da cave

Ao interagir com entrada da cave:

```text
abrir CaveEntryMenu
mostrar checkpoints liberados
mostrar loadout atual
mostrar avisos de risco
permitir escolher checkpoint
entrar na sala segura do checkpoint
```

### 8.1 Checkpoints

Checkpoints:

```text
1
15
30
45
60
75
90
```

Regra:

```text
checkpoint começa exatamente no CaveLevel do checkpoint
todo checkpoint tem sala segura
```

### 8.2 Sala segura

Safe room deve ter:

```text
sem inimigos
sem dano ambiental imediato
spawn point fixo
ponto de saída/retorno se aplicável
feedback de checkpoint
```

Pode ter no futuro:

```text
mini shrine
repair point
campfire
merchant raro
storage limitado
```

---

## 9. Loadout antes da run

CaveEntryMenu deve mostrar:

```text
RightHand
LeftHand
Armor
Accessory
Ammo
Magic item
Hotbar
Consumables
Tools
Food
Return item
```

Validações:

```text
sem arma equipada
sem ferramenta recomendada
sem flechas
sem magic focus, se build mágica
durabilidade baixa
item quebrado
HeatResistance insuficiente
ColdResistance insuficiente
Pickaxe tier baixo
Axe tier baixo
inventário cheio
sem item de retorno
```

Regra:

```text
Avisos não bloqueiam entrada, exceto requisitos absolutos de checkpoint/gate.
```

Exemplo:

```text
Você pode entrar sem ColdResistance suficiente,
mas o menu avisa que haverá risco de dano ambiental.
```

---

## 10. Troca de equipamento e hotbar dentro da cave

Regra fechada:

```text
Jogador pode trocar equipamento dentro da cave livremente.
Jogador pode editar hotbar dentro da cave.
```

Hardening:

```text
Não permitir trocar equipamento durante animação de ataque/cast.
Não permitir trocar equipamento durante dano/stun/root.
Troca pode ser livre fora de combate.
MVP pode permitir sempre, mas isso deve ser marcado como simplificação.
```

Decisão MVP:

```text
MVP permite troca livre.
Futuro restringe durante combate/ação.
```

---

## 11. Durabilidade quebrada

Regra fechada:

```text
Item quebrado impede uso.
```

Aplicação:

| Tipo | Durability 0 |
|---|---|
| Weapon | não ataca |
| Tool | não coleta node/não quebra gate |
| Bow | não dispara |
| MagicFocus/Wand/Staff | não conjura |
| Armor | não concede defesa/resistência |
| Shield | não bloqueia |
| Accessory | perde efeito, se aplicável |

HUD deve avisar:

```text
Weapon Broken
Pickaxe Broken
Armor Broken
Shield Broken
Magic Focus Broken
```

---

## 12. Saída da cave

O jogador só pode sair por:

```text
pontos de saída
item de retorno
```

### 12.1 Pontos de saída

Podem existir em:

```text
entrada do level 1
safe rooms de checkpoint
boss rooms após vitória
salas especiais raras
```

### 12.2 Item de retorno

Item sugerido:

```text
item_consumable_anya_return_stone
Pedra de Retorno de Anya
```

Função:

```text
teleporta para entrada da cave ou Farm/Fonte de Anya
consome item
não conta como morte
preserva loot
não troca CaveRunSeed
```

Decisão MVP:

```text
MVP retorna para entrada da cave/Farm safely.
Destino exato pode ser configurável em dados.
```

---

## 13. Boss defeat

Regra fechada:

```text
Derrotar boss libera checkpoint/avanço imediatamente.
```

Fluxo:

```text
boss defeated
→ registrar DefeatedBossId
→ liberar próximo range/bioma
→ liberar checkpoint se aplicável
→ notificação no HUD
→ persistir no save
```

Hardening:

```text
Se jogador morrer depois de derrotar boss, boss permanece derrotado.
Checkpoint permanece liberado.
ProgressionDrop físico pode estar no corpo se morreu carregando, mas unlock sistêmico permanece.
```

---

## 14. CaveRunHud

Decisão:

```text
Cave HUD será OnGUI MVP próprio, separado conceitualmente do DebugHud.
```

Nome sugerido:

```text
CaveRunHud
```

Motivo:

```text
DebugHud é ferramenta de desenvolvimento.
CaveRunHud é HUD jogável mínimo.
```

### 14.1 Elementos do CaveRunHud

```text
HP
Mana
Stamina
Hunger
CaveLevel
Checkpoint
Biome
Current Weapon/Tool
Durability warning
Ammo count
Active Status
Heat/Cold warning
Return item count
Gold carried
Corpse recovery indicator
Boss HP quando em boss fight
Checkpoint unlocked notification
```

### 14.2 Debug opcional

Em modo debug, mostrar também:

```text
CaveRunSeed
CaveWorldSeed
FactionLockId
EncounterEcologyId
EnemyFamilyIds
BossCandidateId
CorpseRecoveryId
```

---

## 15. Save schema alvo

### 15.1 CaveRunFailureSaveData

```csharp
[Serializable]
public class CaveRunFailureSaveData
{
    public bool HasRecoverableCorpse;
    public PlayerCorpseRecoverySaveData RecoverableCorpse;
}
```

### 15.2 PlayerCorpseRecoverySaveData

```csharp
[Serializable]
public class PlayerCorpseRecoverySaveData
{
    public string CorpseId;
    public int CaveLevel;
    public SerializableVector3 LastKnownPosition;
    public string CaveRunSeedAtDeath;
    public int LostGold;
    public List<ItemStackSaveData> LostInventoryItems;
    public List<EquipmentInstanceSaveData> LostEquipmentItems;
    public bool IsRecovered;
    public int CreatedAtDay;
}
```

### 15.3 Player progression requirement

Adicionar ao player/progression:

```text
XpInCurrentLevel
```

Ou equivalente para permitir zerar XP do nível atual sem mexer no level.

---

## 16. Failure flow completo

```text
1. Player HP chega a 0 na cave.
2. Captura inventory/equipment/gold/ammo carregados.
3. Cria PlayerCorpseRecoverySaveData.
4. Remove esses itens/gold do jogador.
5. Zera XP acumulado do level atual.
6. Mantém level, skill points gastos, attribute points gastos, checkpoints, bosses derrotados.
7. Regenera CaveRunSeed.
8. Reposiciona player na Fonte de Anya na Farm.
9. Marca corpse pendente.
10. Ao reentrar na cave e chegar ao CaveLevel do corpse, generator cria CorpseRecoveryRoom acessível.
11. Jogador interage com corpo.
12. Recupera itens/gold/equipment/ammo.
13. Marca corpse como recovered.
```

---

## 17. Hardening

```text
H1: Morte não remove level.
H2: Morte não remove checkpoints.
H3: Morte não remove boss defeated flags.
H4: Só existe um corpo recuperável.
H5: Nova morte substitui corpo anterior.
H6: Corpo precisa ser acessível mesmo com nova CaveRunSeed.
H7: Item quebrado impede uso.
H8: Sair vivo não muda CaveRunSeed.
H9: Boss derrotado libera checkpoint imediatamente.
H10: CaveRunHud separado do DebugHud.
H11: Entrada por checkpoint sempre em safe room.
H12: Tool/environment warnings devem aparecer antes de entrada.
H13: Return item não conta como morte.
H14: Key progression unlocks sobrevivem à morte.
H15: Corpo recuperável não deve depender de Unity refs no save.
```

---

## 18. Critérios de aceite

```text
CA1: CaveEntryMenu mostra checkpoints liberados.
CA2: Checkpoint inicia no CaveLevel exato em sala segura.
CA3: Loadout mostra arma, shield/offhand, armor, accessory, ammo, magic item, tools e consumables.
CA4: CaveEntryMenu mostra warnings de resistência, tool tier, durability, ammo e retorno.
CA5: CaveRunHud mostra HP, Mana, Stamina, Hunger, CaveLevel, Ammo, Durability e Status.
CA6: Ao sair vivo, CaveRunSeed permanece.
CA7: Ao morrer, player respawna na Fonte de Anya na Farm.
CA8: Ao morrer, gold/inventory/equipment/ammo carregados vão para corpo recuperável.
CA9: Ao morrer, XP do nível atual zera.
CA10: Ao morrer, CaveRunSeed muda.
CA11: Checkpoints e boss defeated persistem após morte.
CA12: Só o último corpo pode ser recuperado.
CA13: Corpo é acessível no CaveLevel da morte mesmo após regenerar run.
CA14: Item de retorno permite sair sem morte.
CA15: Durabilidade 0 impede uso.
CA16: Boss derrotado libera checkpoint/avanço imediatamente.
```

---

## 19. Non-goals

Fora desta spec:

```text
UI final polida
animação final de morte
cutscene da Fonte de Anya
sistema completo de storage na safe room
merchant da cave
campfire/rest completo
perda parcial configurável por dificuldade
multiplayer/co-op corpse recovery
snapshot completo de layout antigo para corpse
```

---

## 20. MVP recomendado

```text
1. CaveEntryMenu OnGUI simples com checkpoints e warnings.
2. CaveRunHud OnGUI separado do DebugHud.
3. Fonte de Anya como respawn point na Farm.
4. Failure flow: morte → corpse save → XP current level zerado → Farm/Fonte de Anya.
5. CorpseRecoveryRoom no mesmo CaveLevel após nova run seed.
6. Return item simples.
7. Durability 0 bloqueia uso.
8. Boss defeated libera checkpoint imediatamente.
```

Não implementar UI final/polida no primeiro pacote.

---

## 21. Impacto em specs anteriores

Complementa:

- `docs_old/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- `docs_old/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `docs_old/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_SPEC_v1.0.md`
- `docs_old/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md`

Não altera specs antigas destrutivamente.





