# SpecKit — FASE9G Cave Bestiary, Faction Locks & Portal Ecology

> **Feature:** `FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY`  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero que cada nível ou trecho da caverna tenha uma identidade coerente de criaturas, facções, bosses e ecologia, para que a exploração procedural pareça parte viva de Vaalara, e não apenas uma lista aleatória de inimigos.

---

## 2. Objetivos funcionais

### O1 — Faction lock procedural

A geração deve escolher um `FactionLockId` principal para cada CaveLevel ou subfaixa de 3–5 níveis.

### O2 — Coerência de inimigos

Inimigos incompatíveis não devem aparecer no mesmo nível como spawn comum.

### O3 — Bestiário de Vaalara

O bestiário deve usar raças, facções, clãs e criaturas de Vaalara, incluindo goblins, orcs, drows, anões profundos, draconatos, tieflings, Ninrorin, gnomos, undeads, drakes, wyverns, observadores de Elyndor e bestas.

### O4 — Raças não são malignas por natureza

Humanoides inimigos são facções, exilados, cultistas, saqueadores, corrompidos, guardiões ou expedições rivais.

### O5 — Bosses e minibosses procedurais

Cada marco de miniboss/boss deve ter 3 candidatos compatíveis com bioma, ecologia e faction lock.

### O6 — Boss persistente por save

Boss checkpoint escolhido deve persistir no save e não mudar apenas porque a CaveRunSeed mudou.

### O7 — Miniboss persistente por run

Miniboss pode mudar quando a CaveRunSeed muda, mas deve ficar estável dentro da mesma run.

### O8 — Debug

DebugHud deve poder exibir `BiomeId`, `EncounterEcologyId`, `FactionLockId`, `EnemyFamilyIds` e `BossCandidateId` quando a implementação da FASE9G entrar.

---

## 3. Regras de negócio

### R1 — Lock por subfaixa

Padrão recomendado:

```text
FactionLock por subfaixa de 3–5 níveis.
Cada CaveLevel herda o lock dominante da subfaixa.
```

### R2 — Exceções controladas

Misturas incompatíveis só podem ocorrer como:

```text
AmbientFauna
RareIntruder
BossOverride
ConflictEncounter
```

`ConflictEncounter` fica fora do MVP.

### R3 — Rare intruder

Rare intruder deve ter chance baixa, ser limitado por bioma e aparecer no debug como rare.

### R4 — Boss de checkpoint

Boss de checkpoint pode variar entre 3 candidatos compatíveis com o arco/ecologia dominante.

### R5 — Final boss

Level 100 tem três possíveis final bosses por save:

```text
The Portal-Bound Ancient
Blackstone Dragon of the Central Arch
Meteor Lich of Elyndor
```

---

## 4. Entidades funcionais

- `CaveEncounterEcologySO`
- `CaveFactionLockSO`
- `EnemyFamilySO`
- `BossCandidateSO`
- `CaveLevelEcologySaveData`
- `CaveBossChoiceSaveData`

---

## 5. Critérios de aceite

### CA1 — CaveLevel com identidade

Cada CaveLevel gerado possui:

```text
BiomeId
EncounterEcologyId
FactionLockId
EnemyFamilyIds
BossCandidateId quando aplicável
```

### CA2 — Sem mistura incoerente

Se `FactionLock = DeepForgeExiles`, o nível não gera `DrowFrostExpedition` como spawn comum.

### CA3 — Exceções explícitas

Mistura incompatível só ocorre se marcada como ambient fauna, rare intruder, boss override ou conflict encounter.

### CA4 — Três candidatos de miniboss

Cada marco de miniboss tem 3 opções compatíveis.

### CA5 — Três candidatos de boss

Cada marco de boss/checkpoint tem 3 opções compatíveis.

### CA6 — Boss persistente

Boss checkpoint escolhido persiste no save.

### CA7 — Miniboss por run

Miniboss permanece estável dentro da mesma run e pode mudar com nova CaveRunSeed.

### CA8 — Raças de Vaalara respeitadas

Humanoides inimigos são facções ou casos narrativos específicos, não raças inteiras como inimigas.

### CA9 — Debug pronto

Quando implementado, DebugHud consegue mostrar ecologia/faction lock/boss candidate.

---

## 6. Non-goals

Fora desta spec:

- statblocks finais;
- sprites finais;
- balance numérico definitivo;
- IA avançada por facção;
- quests completas de facção;
- diálogos completos;
- arte final de bosses;
- sistema diplomático com facções;
- captura/recrutamento de inimigos;
- movesets finais de todos os bosses.

---

## 7. Boss/miniboss candidate table

| Level | Tipo | Candidatos |
|---:|---|---|
| 10 | Miniboss | Brood Spider Matriarch; Alpha Cave Wolf; Giant Bat Broodmother |
| 15 | Boss | Meteor Ooze King; Goblin Bloodfang Butcher; Kobold Tunnel Tyrant |
| 20 | Miniboss | Owlbear Matriarch; Goblin Trap-King; Root-Tethered Worg |
| 25 | Miniboss | Orc Blood-Tusk Captain; Sporeheart Brute; Thornblade Grove Warden |
| 30 | Boss | Rootbound Guardian; Fungal Brood Sovereign; Orc Root-Reaver Warchief |
| 35 | Miniboss | Black-Anvil Smith; Drow Frostblade Captain; Ice Spider Queen |
| 40 | Miniboss | Deep Hammer Overseer; Frost Wight Commander; Crystal Crawler Prime |
| 45 | Boss | Gatebreaker of the Deep Forge; Drow Frostblade Matriarch; Frost Wight Commander |
| 50 | Miniboss | Gnoll Packlord; Orc Flamecaller; Ruinblood Scale-Priest |
| 55 | Miniboss | Fire Drake; Veyraathi Red Cinder; Kaand-Marked Berserker Chief |
| 60 | Boss | Ember Maw Wyvern; Gnoll Ash-Pack Prophet; Ruinblood Tyrant |
| 65 | Miniboss | Eye of the Broken Gate; Ninrorin Broken Planewalker; Gem-Crazed Prospector |
| 70 | Miniboss | Runic Golem; Drow Portalist Captain; Prism Shardguard Prime |
| 75 | Boss | Relic Sentinel of Elyndor; Observador do Arco Partido; Ninrorin Gate-Sealer |
| 80 | Miniboss | Vampire Spawn Lord; Nyx-Bound Cave Prophet; Blackstone Horror Prime |
| 85 | Miniboss | Wight Commander; Veyraath's Red Herald; Moonless Spear Champion |
| 90 | Boss | Hollow Lich of the Black Stone; Bloodbound Court Patriarch/Matriarch; Prophet of the Moonless Gate |
| 94 | Miniboss | Blackstone Drake; Fallen Judge of Kanthor; Lich Fragment Prime |
| 97 | Miniboss | Broken Eye Tyrant; Veyraath's Red Herald; Broken Elyndor Golem |
| 99 | Boss Gate | Three Gatebound Elites; Blackstone Drake Sovereign; Council of Broken Arches |
| 100 | Final Boss | The Portal-Bound Ancient; Blackstone Dragon of the Central Arch; Meteor Lich of Elyndor |

---

## 8. Dependências

- FASE9F Cave/Resources/Encounters.
- Save schema/migration da FASE9E.
- Item taxonomy da FASE9E.
- Damage/status da FASE9E.
- Player progression da FASE9E.
- Guia de Raças de Vaalara.
- GDD v2.6.

---

## 9. Pronto para Plan quando

- FASE9F procedural contracts estiverem planejados ou implementados.
- A implementação puder criar SOs/DTOs de ecology/faction/enemy family.
- O time aceitar que FASE9G é design baseline, não implementação imediata obrigatória completa.


