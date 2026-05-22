# SpecKit â€” FASE9G Cave Bestiary, Faction Locks & Portal Ecology

> **Feature:** `FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY`  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero que cada nÃ­vel ou trecho da caverna tenha uma identidade coerente de criaturas, facÃ§Ãµes, bosses e ecologia, para que a exploraÃ§Ã£o procedural pareÃ§a parte viva de Vaalara, e nÃ£o apenas uma lista aleatÃ³ria de inimigos.

---

## 2. Objetivos funcionais

### O1 â€” Faction lock procedural

A geraÃ§Ã£o deve escolher um `FactionLockId` principal para cada CaveLevel ou subfaixa de 3â€“5 nÃ­veis.

### O2 â€” CoerÃªncia de inimigos

Inimigos incompatÃ­veis nÃ£o devem aparecer no mesmo nÃ­vel como spawn comum.

### O3 â€” BestiÃ¡rio de Vaalara

O bestiÃ¡rio deve usar raÃ§as, facÃ§Ãµes, clÃ£s e criaturas de Vaalara, incluindo goblins, orcs, drows, anÃµes profundos, draconatos, tieflings, Ninrorin, gnomos, undeads, drakes, wyverns, observadores de Elyndor e bestas.

### O4 â€” RaÃ§as nÃ£o sÃ£o malignas por natureza

Humanoides inimigos sÃ£o facÃ§Ãµes, exilados, cultistas, saqueadores, corrompidos, guardiÃµes ou expediÃ§Ãµes rivais.

### O5 â€” Bosses e minibosses procedurais

Cada marco de miniboss/boss deve ter 3 candidatos compatÃ­veis com bioma, ecologia e faction lock.

### O6 â€” Boss persistente por save

Boss checkpoint escolhido deve persistir no save e nÃ£o mudar apenas porque a CaveRunSeed mudou.

### O7 â€” Miniboss persistente por run

Miniboss pode mudar quando a CaveRunSeed muda, mas deve ficar estÃ¡vel dentro da mesma run.

### O8 â€” Debug

DebugHud deve poder exibir `BiomeId`, `EncounterEcologyId`, `FactionLockId`, `EnemyFamilyIds` e `BossCandidateId` quando a implementaÃ§Ã£o da FASE9G entrar.

---

## 3. Regras de negÃ³cio

### R1 â€” Lock por subfaixa

PadrÃ£o recomendado:

```text
FactionLock por subfaixa de 3â€“5 nÃ­veis.
Cada CaveLevel herda o lock dominante da subfaixa.
```

### R2 â€” ExceÃ§Ãµes controladas

Misturas incompatÃ­veis sÃ³ podem ocorrer como:

```text
AmbientFauna
RareIntruder
BossOverride
ConflictEncounter
```

`ConflictEncounter` fica fora do MVP.

### R3 â€” Rare intruder

Rare intruder deve ter chance baixa, ser limitado por bioma e aparecer no debug como rare.

### R4 â€” Boss de checkpoint

Boss de checkpoint pode variar entre 3 candidatos compatÃ­veis com o arco/ecologia dominante.

### R5 â€” Final boss

Level 100 tem trÃªs possÃ­veis final bosses por save:

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

## 5. CritÃ©rios de aceite

### CA1 â€” CaveLevel com identidade

Cada CaveLevel gerado possui:

```text
BiomeId
EncounterEcologyId
FactionLockId
EnemyFamilyIds
BossCandidateId quando aplicÃ¡vel
```

### CA2 â€” Sem mistura incoerente

Se `FactionLock = DeepForgeExiles`, o nÃ­vel nÃ£o gera `DrowFrostExpedition` como spawn comum.

### CA3 â€” ExceÃ§Ãµes explÃ­citas

Mistura incompatÃ­vel sÃ³ ocorre se marcada como ambient fauna, rare intruder, boss override ou conflict encounter.

### CA4 â€” TrÃªs candidatos de miniboss

Cada marco de miniboss tem 3 opÃ§Ãµes compatÃ­veis.

### CA5 â€” TrÃªs candidatos de boss

Cada marco de boss/checkpoint tem 3 opÃ§Ãµes compatÃ­veis.

### CA6 â€” Boss persistente

Boss checkpoint escolhido persiste no save.

### CA7 â€” Miniboss por run

Miniboss permanece estÃ¡vel dentro da mesma run e pode mudar com nova CaveRunSeed.

### CA8 â€” RaÃ§as de Vaalara respeitadas

Humanoides inimigos sÃ£o facÃ§Ãµes ou casos narrativos especÃ­ficos, nÃ£o raÃ§as inteiras como inimigas.

### CA9 â€” Debug pronto

Quando implementado, DebugHud consegue mostrar ecologia/faction lock/boss candidate.

---

## 6. Non-goals

Fora desta spec:

- statblocks finais;
- sprites finais;
- balance numÃ©rico definitivo;
- IA avanÃ§ada por facÃ§Ã£o;
- quests completas de facÃ§Ã£o;
- diÃ¡logos completos;
- arte final de bosses;
- sistema diplomÃ¡tico com facÃ§Ãµes;
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

## 8. DependÃªncias

- FASE9F Cave/Resources/Encounters.
- Save schema/migration da FASE9E.
- Item taxonomy da FASE9E.
- Damage/status da FASE9E.
- Player progression da FASE9E.
- Guia de RaÃ§as de Vaalara.
- GDD v2.6.

---

## 9. Pronto para Plan quando

- FASE9F procedural contracts estiverem planejados ou implementados.
- A implementaÃ§Ã£o puder criar SOs/DTOs de ecology/faction/enemy family.
- O time aceitar que FASE9G Ã© design baseline, nÃ£o implementaÃ§Ã£o imediata obrigatÃ³ria completa.

