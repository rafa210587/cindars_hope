# FASE9G Amendment v1.1 â€” Enemy Combat Roles, AI, Status & Movesets

> **Status:** amendment aprovado para orientar prÃ³ximas waves.
> **Tipo:** complemento da `FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0`.
> **NÃ£o substitui:** FASE9G v1.0.
> **Objetivo:** detalhar como criaturas, elites, minibosses e bosses da cave lutam, se movem, aplicam status, usam armas elementais, escalam por nÃ­vel e respeitam legibilidade/balance do combate action.

---

## 1. AvaliaÃ§Ã£o de qualidade e hardening aplicado

O rascunho base estava bom em cobertura de criaturas e intenÃ§Ã£o de combate, mas precisava de hardening em quatro pontos:

1. **Legibilidade do combate:** muitos inimigos com status/mecÃ¢nicas poderiam virar caos visual se nÃ£o houver telegraph, cooldown e limite de simultaneidade.
2. **Status stacking:** permitir mÃºltiplos status Ã© bom, mas precisa de regras de orÃ§amento por tipo de inimigo.
3. **Aerial/flying behavior:** drakes, bats e wyverns precisam de fallback terrestre e limites de colisÃ£o/pathing.
4. **Boss/miniboss:** trÃªs fases e duas mecÃ¢nicas sÃ£o bons, mas precisam de regra mÃ­nima de fase, telegraph e janela de resposta do jogador.

Este amendment adiciona:

- contratos de `MovementPatternId`, `CombatBehaviorId`, `StatusId`, `DamageType` e `EnemyCombatProfile`;
- orÃ§amento de status por tier;
- regras de telegraph/cooldown;
- regras de spawn composition por sala;
- regras para armas elementais e drops;
- matriz de movimento/comportamento por famÃ­lia de criatura;
- movesets de minibosses e bosses;
- critÃ©rios de aceite para implementaÃ§Ã£o futura.

---

## 2. DecisÃµes fechadas

```text
D1: Combate da cave deve ser action RPG simples, com padrÃµes legÃ­veis, movimentaÃ§Ã£o ativa e fases.
D2: Inimigos podem ter mais de 1 status principal.
D3: Bosses devem ter 3 fases na spec.
D4: Minibosses devem ter 2 mecÃ¢nicas especiais.
D5: Fear, Curse, Blind, Chill e outros status entram jÃ¡ como IDs de contrato.
D6: Humanoides podem usar armas elementais jÃ¡ no MVP/near-MVP.
D7: Armas elementais tÃªm chance baixa de drop.
D8: Wyverns/Drakes variam por tipo: alguns aÃ©reos, alguns terrestres/dash, alguns hÃ­bridos.
D9: Toda criatura precisa ter MovementPattern + CombatBehavior + StatusProfile.
D10: Toda mecÃ¢nica forte precisa de telegraph ou janela de resposta.
```

---

## 3. Hardening rules obrigatÃ³rias

### H1 â€” Telegraph obrigatÃ³rio

Todo ataque especial deve ter aviso visual/temporal antes de aplicar dano/status.

Exemplos:

```text
charge attack -> windup curto antes da investida
ground pool -> marca no chÃ£o antes de ativar
beam attack -> linha/olho carregando antes do raio
boss slam -> animaÃ§Ã£o/flash antes do impacto
```

### H2 â€” Cooldown mÃ­nimo por comportamento especial

Ataques especiais nÃ£o podem ser spammados.

SugestÃ£o inicial:

| Behavior | Cooldown mÃ­nimo |
|---|---:|
| charge/leap | 2.0s |
| web/root/trap | 3.0s |
| ground pool | 4.0s |
| summon adds | 8.0s |
| beam/rotating beam | 5.0s |
| boss phase skill | 6.0s |

### H3 â€” Status budget por tier

| Tier | Status ativos permitidos no design |
|---|---:|
| Common | 0â€“2 |
| Strong | 1â€“2 |
| Elite | 1â€“3 |
| Miniboss | 2â€“4 |
| Boss | 3+ por fases |

Regra: nÃ£o significa que todos aplicam tudo ao mesmo tempo. Cada status deve ter chance/cooldown/duraÃ§Ã£o.

### H4 â€” Spawn composition budget

Para evitar salas injustas, cada encounter deve respeitar composiÃ§Ã£o mÃ¡xima.

SugestÃ£o por sala comum:

```text
atÃ© 1 Elite
atÃ© 1 Controller pesado
atÃ© 1 Caster forte
atÃ© 2 Ranged
atÃ© 4â€“8 Grunts/Swarm, conforme nÃ­vel
```

Miniboss room:

```text
1 Miniboss
0â€“2 elites menores
adds controlados por fase ou cooldown
```

Boss arena:

```text
1 Boss
adds apenas em fases especÃ­ficas
limite de adds simultÃ¢neos
```

### H5 â€” No unavoidable chain control

Status de controle forte nÃ£o podem encadear sem janela de recuperaÃ§Ã£o.

```text
root + stun + freeze + fear nÃ£o devem manter jogador sem aÃ§Ã£o continuamente.
```

### H6 â€” Aerial fallback

Toda criatura aÃ©rea deve ter fallback se o path/arena nÃ£o suportar voo real.

```text
Flying real disponÃ­vel -> usar move_flying_swoop/move_flying_circle.
Sem suporte -> usar dash terrestre, leap ou projectile placeholder.
```

### H7 â€” DamageCalculator Ãºnico

Todo dano deve passar pelo `DamageCalculator` definido na FASE9E. NÃ£o criar dano paralelo.

### H8 â€” Sem hardcode de status solto

Status devem ser referenciados por `StatusId`, resolvidos por data/SO/registry quando implementado.

---

## 4. StatusIds oficiais da cave

| StatusId | Tipo | Efeito conceitual |
|---|---|---|
| `status_burn` | DoT | dano Fire por tempo |
| `status_poison` | DoT | dano Poison por tempo |
| `status_bleed` | DoT fÃ­sico | dano fÃ­sico por tempo, futuro |
| `status_chill` | slow | reduz movimento/aÃ§Ã£o |
| `status_freeze` | hard CC | imobiliza curto, futuro |
| `status_fear` | controle | forÃ§a recuo/desorganizaÃ§Ã£o, futuro |
| `status_blind` | debuff | reduz precisÃ£o/percepÃ§Ã£o, futuro |
| `status_curse` | debuff | reduz atributo/resistÃªncia |
| `status_shadow_mark` | debuff | aumenta dano Shadow recebido |
| `status_arcane_mark` | debuff | aumenta dano Arcane recebido |
| `status_corruption` | DoT/debuff | dano e reduÃ§Ã£o futura de cura |
| `status_stun` | hard CC | interrupÃ§Ã£o curta |
| `status_knockback` | displacement | empurra |
| `status_root` | controle | prende no lugar |
| `status_webbed` | controle | slow/root por teia |
| `status_armor_up` | buff | reduz dano recebido |
| `status_enrage` | buff | aumenta dano/velocidade |
| `status_lifesteal` | buff | cura pequena ao causar dano |
| `status_silence` | debuff | impede magia, futuro |
| `status_confusion` | controle | movimento/aÃ§Ã£o errÃ¡tica, futuro |

---

## 5. DamageTypes

| DamageType | Uso |
|---|---|
| `Physical` | mordida, arma, garra, impacto |
| `Fire` | fogo, magma, brasa |
| `Ice` | gelo, geada, cristal frio |
| `Poison` | veneno, fungo, aranha |
| `Shadow` | Nyx, mortos, drows, escuridÃ£o |
| `Arcane` | Elyndor, portais, magia pura |
| `Lightning` | draconatos, runas, storm |
| `Acid` | ruÃ­na, draconatos, oozes |
| `Corruption` | Pedra Negra, Veyraath |
| `Thunder` | martelo rÃºnico, julgamento, choque |

MVP pode implementar subset, mas os IDs devem estar reservados.

---

## 6. MovementPatternIds

| MovementPatternId | DescriÃ§Ã£o |
|---|---|
| `move_idle_guard` | guarda posiÃ§Ã£o atÃ© aggro |
| `move_direct_chase` | persegue em linha direta |
| `move_slow_heavy_chase` | persegue lentamente com massa alta |
| `move_fast_chase` | perseguiÃ§Ã£o rÃ¡pida |
| `move_hop_chase` | avanÃ§a em pulos |
| `move_erratic_hop` | pulo irregular/imprevisÃ­vel |
| `move_pack_circle` | tenta circular o jogador em grupo |
| `move_hit_and_run` | aproxima, ataca, recua |
| `move_ranged_keep_distance` | mantÃ©m distÃ¢ncia ideal |
| `move_backline_caster` | recua e conjura |
| `move_patrol_route` | patrulha pontos |
| `move_ambush_stationary` | fica oculto atÃ© trigger |
| `move_burrow_ambush` | emerge do chÃ£o |
| `move_wall_ceiling_drop` | cai de parede/teto |
| `move_flying_swoop` | voo com mergulho |
| `move_flying_circle` | orbita e ataca de longe |
| `move_short_blink` | teleporte curto |
| `move_phase_shift` | atravessa/evade por fase |
| `move_guard_node` | defende node/baÃº/portal |
| `move_guard_position` | defende regiÃ£o ou aliado |
| `move_boss_arena` | movimento por fases em arena |
| `move_dash_line` | investida reta |
| `move_dash_arc` | dash em arco |
| `move_summoner_keepaway` | foge e invoca |
| `move_stationary_turret` | quase parado, ataca Ã  distÃ¢ncia |

---

## 7. CombatBehaviorIds

| CombatBehaviorId | DescriÃ§Ã£o |
|---|---|
| `ai_contact_damage` | dano por contato |
| `ai_basic_melee` | ataque melee simples |
| `ai_combo_melee` | combo curto |
| `ai_charge_attack` | investida telegrafada |
| `ai_leap_attack` | salto/impacto |
| `ai_bite_and_retreat` | morde e recua |
| `ai_ranged_projectile` | projÃ©til simples |
| `ai_spread_projectile` | vÃ¡rios projÃ©teis |
| `ai_arc_projectile` | projÃ©til em arco |
| `ai_ground_pool` | cria poÃ§a/Ã¡rea |
| `ai_trap_place` | coloca armadilha |
| `ai_web_shot` | teia/slow/root |
| `ai_root` | prende por raiz |
| `ai_buff_allies` | buffa aliados |
| `ai_debuff_player` | aplica debuff |
| `ai_summon_adds` | invoca adds |
| `ai_heal_or_lifesteal` | cura ou rouba vida |
| `ai_shield_guard` | bloqueia/reduz dano |
| `ai_breath_cone` | cone elemental |
| `ai_tail_sting` | ferrÃ£o/cauda |
| `ai_flying_dive` | mergulho aÃ©reo |
| `ai_blink_strike` | teleporta e ataca |
| `ai_beam_attack` | raio/olhar |
| `ai_rotating_beams` | beams em rotaÃ§Ã£o |
| `ai_area_slam` | pancada AoE |
| `ai_shockwave` | onda de choque |
| `ai_boss_phase_switch` | troca fase por HP |
| `ai_boss_minion_wave` | fase de invocaÃ§Ã£o |
| `ai_boss_enrage` | fase final agressiva |

---

## 8. EnemyCombatProfile target

```csharp
public class EnemyCombatProfileSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string EnemyId;
    public string RoleId;
    public string MovementPatternId;
    public string[] CombatBehaviorIds;
    public string[] DamageTypeIds;
    public string[] StatusIds;
    public string[] ResistanceDamageTypeIds;
    public string[] VulnerabilityDamageTypeIds;
    public int EnemyLevelOffset;
    public float HpMultiplier;
    public float DamageMultiplier;
    public float SpecialCooldownSeconds;
    public bool RequiresTelegraph;
}
```

---

## 9. Scaling por CaveLevel

```text
EffectiveEnemyLevel = CaveLevel + EnemyLevelOffset
```

| Tipo | Offset |
|---|---:|
| Common | 0 |
| Strong | +1 |
| Special | +2 |
| Elite | +3 |
| MiniBoss | +4 |
| Boss | +5 |

FÃ³rmulas alvo:

```text
HP = BaseHP + EffectiveEnemyLevel Ã— HPGrowth Ã— RoleHpMultiplier
Damage = BaseDamage + EffectiveEnemyLevel Ã— DamageGrowth Ã— RoleDamageMultiplier
Defense = BaseDefense + floor(EffectiveEnemyLevel / 5)
XP = EffectiveEnemyLevel Ã— DifficultyMultiplier
```

Multiplicadores de HP:

| Role | HP Multiplier |
|---|---:|
| Swarm | 0.5 |
| Grunt | 1.0 |
| Skirmisher | 0.85 |
| Archer | 0.75 |
| Caster | 0.75 |
| Brute | 1.8 |
| Tank | 2.2 |
| Elite | 2.5 |
| MiniBoss | 5.0 |
| Boss | 10.0 |

Multiplicadores de dano:

| Role | Damage Multiplier |
|---|---:|
| Swarm | 0.5 |
| Grunt | 1.0 |
| Skirmisher | 1.1 |
| Archer | 1.0 |
| Caster | 1.2 |
| Brute | 1.4 |
| Tank | 0.9 |
| Assassin | 1.8 |
| Elite | 1.6 |
| MiniBoss | 2.2 |
| Boss | 3.0 |

---

## 10. ResistÃªncias e vulnerabilidades por famÃ­lia

| FamÃ­lia | ResistÃªncia | Vulnerabilidade |
|---|---|---|
| Oozes | Poison | Fire ou Ice por subtipo |
| Fire creatures | Fire | Ice |
| Ice creatures | Ice | Fire |
| Undead | Shadow/Poison | Fire/Arcane |
| Constructs | Physical parcial | Lightning/Arcane |
| Drow/Nyx | Shadow | Light futuro / Arcane situacional |
| Veyraathi | Fire/Corruption | Ice/Arcane |
| Draconic Fire | Fire | Ice |
| Draconic Acid | Acid | Lightning futuro |
| Blackstone | Shadow/Corruption | Arcane/Light futuro |
| Beasts | nenhuma padrÃ£o | depende do subtipo |

---

# 11. Combat matrix por faixa

## 11.1 NÃ­veis 1â€“10 â€” Local Caves

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Slime | Grunt | `move_hop_chase` | `ai_contact_damage` | Physical |
| Stone Slime | Tank Grunt | `move_slow_heavy_chase` | `ai_contact_damage`, `ai_area_slam` leve | Physical, `status_knockback` |
| Cave Bat | Flyer | `move_flying_swoop` | `ai_flying_dive` | Physical |
| Giant Bat | Flyer Elite | `move_flying_circle` | `ai_flying_dive`, `ai_debuff_player` | Physical, `status_blind` futuro |
| Cave Rat | Swarm | `move_fast_chase` | `ai_basic_melee` | Physical |
| Dire Rat | Swarm Strong | `move_pack_circle` | `ai_bite_and_retreat` | Physical, `status_poison` leve |
| Wolf | Skirmisher | `move_pack_circle` | `ai_bite_and_retreat` | Physical |
| Cave Wolf | Skirmisher | `move_fast_chase` | `ai_charge_attack` | Physical, `status_bleed` futuro |
| Spider | Controller | `move_hit_and_run` | `ai_basic_melee` | Poison |
| Venom Spider | Controller | `move_wall_ceiling_drop` | `ai_web_shot`, `ai_basic_melee` | Poison, `status_webbed` |
| Cave Beetle | Tank Grunt | `move_direct_chase` | `ai_basic_melee`, `ai_shield_guard` leve | Physical |
| Tunnel Worm | Ambusher | `move_burrow_ambush` | `ai_leap_attack` | Physical |
| Small Ooze | Grunt | `move_erratic_hop` | `ai_contact_damage`, `ai_ground_pool` pequeno | Poison/Physical |
| Mud Ooze | Controller | `move_slow_heavy_chase` | `ai_ground_pool` | slow via `status_chill`, Physical |
| Rock Crab | Tank | `move_guard_node` | `ai_shield_guard`, `ai_basic_melee` | Physical |
| Mole Beast | Burrower | `move_burrow_ambush` | `ai_charge_attack` | Physical, `status_knockback` |
| Brood Spider | Miniboss Candidate | `move_wall_ceiling_drop` | `ai_web_shot`, `ai_summon_adds` | Poison, `status_webbed` |
| Alpha Cave Wolf | Miniboss Candidate | `move_pack_circle` | `ai_charge_attack`, `ai_buff_allies` | Physical, `status_bleed` futuro |
| Giant Bat Broodmother | Miniboss Candidate | `move_flying_circle` | `ai_flying_dive`, `ai_summon_adds` | Physical, `status_blind` futuro |
| Armored Beetle | Tank Elite | `move_slow_heavy_chase` | `ai_shield_guard`, `ai_area_slam` | Physical |
| Burrower Larva | Ambusher | `move_burrow_ambush` | `ai_leap_attack` | Poison |
| Crystal Tick | Swarm | `move_fast_chase` | `ai_contact_damage` | Arcane leve |
| Blind Cave Hound | Skirmisher | `move_fast_chase` | `ai_bite_and_retreat` | Physical |
| Spore Toad | Controller | `move_idle_guard` | `ai_ground_pool`, `ai_debuff_player` | Poison |
| Razor Mole | Burrower | `move_burrow_ambush` | `ai_charge_attack` | Physical, `status_bleed` futuro |

## 11.2 NÃ­veis 11â€“15 â€” Meteor-Touched Threshold

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Meteor-Touched Slime | Special Grunt | `move_erratic_hop` | `ai_contact_damage`, `ai_arc_projectile` | Arcane, `status_arcane_mark` |
| Redstone Bat | Flyer | `move_flying_swoop` | `ai_flying_dive`, blink futuro | Arcane/Physical |
| Giant Spider | Controller | `move_wall_ceiling_drop` | `ai_web_shot`, `ai_basic_melee` | Poison, `status_webbed` |
| Skeleton Miner | Undead Grunt | `move_direct_chase` | `ai_basic_melee` | Physical |
| Restless Miner | Undead Grunt | `move_patrol_route` | `ai_basic_melee`, `ai_debuff_player` leve | Shadow |
| Goblin Bloodfang Stabber | Assassin | `move_hit_and_run` | `ai_bite_and_retreat`, `ai_combo_melee` | Physical, `status_bleed` |
| Goblin Swarm Runner | Swarm | `move_pack_circle` | `ai_basic_melee` | Physical |
| Uru'dakh Swarm Goblin | Swarm/Support | `move_pack_circle` | `ai_buff_allies`, `ai_basic_melee` | Physical |
| Kobold Tunnel Scout | Skirmisher | `move_hit_and_run` | `ai_basic_melee`, `ai_trap_place` | Physical |
| Kobold Pebble-Slinger | Ranged | `move_ranged_keep_distance` | `ai_ranged_projectile` | Physical |
| Kobold Trap-Keeper | Trapper | `move_guard_node` | `ai_trap_place`, `ai_ranged_projectile` | Physical, `status_stun` futuro |
| Meteor Ooze | Elite Ooze | `move_erratic_hop` | `ai_ground_pool`, `ai_arc_projectile` | Arcane, `status_arcane_mark` |
| Redstone Tick | Swarm | `move_fast_chase` | `ai_contact_damage` | Arcane |
| Cave Centipede | Skirmisher | `move_hit_and_run` | `ai_bite_and_retreat` | Poison |
| Bone Spider | Ambusher | `move_wall_ceiling_drop` | `ai_web_shot`, `ai_basic_melee` | Poison/Shadow |
| Stoneback Boar | Brute | `move_dash_line` | `ai_charge_attack` | Physical, `status_knockback` |
| Rift Rat | Swarm Special | `move_short_blink` | `ai_basic_melee` | Arcane |
| Shard Beetle | Tank | `move_direct_chase` | `ai_shield_guard`, `ai_basic_melee` | Physical/Arcane |

### Boss 15 candidates

| Boss | Movimento | Fase 1 | Fase 2 | Fase 3 | Status |
|---|---|---|---|---|---|
| Meteor Ooze King | `move_boss_arena`, `move_erratic_hop` | contact + jump | invoca slimes | projÃ©teis meteÃ³ricos + pools | `status_arcane_mark`, slow, `status_knockback` |
| Goblin Bloodfang Butcher | `move_dash_arc` | melee combos | chama goblins | enrage + bleed strikes | `status_bleed`, `status_enrage` |
| Kobold Tunnel Tyrant | `move_guard_node` | traps + sling | chama kobolds | cave-in/shockwave | `status_stun`, `status_knockback` |

## 11.3 NÃ­veis 16â€“30 â€” Underground Forest

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Goblin Trapper | Trapper | `move_hit_and_run` | `ai_trap_place`, `ai_ranged_projectile` | Physical, `status_root` leve |
| Bone-Scrap Bomber | Ranged AoE | `move_ranged_keep_distance` | `ai_arc_projectile`, `ai_ground_pool` | Fire/Physical, `status_burn` |
| Zhak'thul Free-Scream Runner | Disruptor | `move_fast_chase` | `ai_debuff_player`, `ai_basic_melee` | Physical, `status_blind` futuro |
| Zhak'thul Noise-Shaman | Support | `move_backline_caster` | `ai_buff_allies`, `ai_debuff_player` | Shadow/Physical, `status_fear` futuro |
| Goblin Net-Thrower | Controller | `move_ranged_keep_distance` | `ai_web_shot` | `status_root`, Physical |
| Goblin Mushroom Thief | Skirmisher | `move_hit_and_run` | `ai_bite_and_retreat` | Poison |
| Worg | Pack Brute | `move_pack_circle` | `ai_charge_attack`, `ai_buff_allies` | Physical, `status_fear` futuro |
| Orc Fury Grunt | Brute | `move_direct_chase` | `ai_combo_melee` | Physical |
| Blood-Tusk Charger | Brute | `move_dash_line` | `ai_charge_attack` | Physical, `status_knockback` |
| Orc Hunter | Archer | `move_ranged_keep_distance` | `ai_ranged_projectile` | Physical |
| Orc Bone-Axe Guard | Brute/Tank | `move_slow_heavy_chase` | `ai_combo_melee`, `ai_shield_guard` | Physical |
| Orc Cave Howler | Support | `move_guard_position` | `ai_buff_allies`, `ai_debuff_player` | `status_fear` futuro |
| Owlbear | Brute | `move_slow_heavy_chase` | `ai_leap_attack`, `ai_combo_melee` | Physical, `status_knockback` |
| Dire Wolf | Skirmisher | `move_pack_circle` | `ai_bite_and_retreat`, `ai_charge_attack` | Physical, `status_bleed` futuro |
| Rootbound Hunter | Archer | `move_ranged_keep_distance` | `ai_ranged_projectile`, `ai_root` | Physical, `status_root` |
| Thornblade Scout | Skirmisher | `move_hit_and_run` | `ai_combo_melee` | Poison/Physical |
| Corrupted Grove Warden | Elite Controller | `move_guard_node` | `ai_ground_pool`, `ai_root` | Poison, `status_root`, `status_corruption` |
| Animated Vine | Controller | `move_ambush_stationary` | `ai_web_shot`, `ai_root` | `status_root` |
| Briar Sprite | Flyer/Caster | `move_flying_circle` | `ai_ranged_projectile`, `ai_debuff_player` | Poison/Arcane |
| Mycelium Walker | Grunt | `move_slow_heavy_chase` | `ai_ground_pool` | Poison |
| Fungal Crawler | Skirmisher | `move_hit_and_run` | `ai_basic_melee`, spore burst | Poison |
| Spore Bat | Flyer | `move_flying_swoop` | `ai_ground_pool` ao passar | Poison |
| Root Horror | Brute Controller | `move_slow_heavy_chase` | `ai_area_slam`, `ai_root` | Physical, `status_root` |
| Poison Slime | Ooze | `move_erratic_hop` | `ai_contact_damage`, `ai_ground_pool` | Poison |
| Mushroom Brute | Brute | `move_slow_heavy_chase` | `ai_area_slam` | Poison, `status_knockback` |
| Spore Cloudling | Caster | `move_backline_caster` | `ai_debuff_player`, `ai_ground_pool` | Poison, `status_blind` |
| Cave Bear | Brute | `move_slow_heavy_chase` | `ai_combo_melee`, `ai_charge_attack` | Physical |
| Giant Centipede | Skirmisher | `move_hit_and_run` | `ai_bite_and_retreat` | Poison |
| Horned Cave Boar | Charger | `move_dash_line` | `ai_charge_attack` | Physical, `status_knockback` |
| Mossback Brute | Tank Beast | `move_slow_heavy_chase` | `ai_area_slam`, `ai_shield_guard` | Physical |

### Minibosses 20/25

| Miniboss | MecÃ¢nica 1 | MecÃ¢nica 2 |
|---|---|---|
| Owlbear Matriarch | leap slam | rage roar / adds |
| Goblin Trap-King | coloca traps | chama goblins ranged |
| Root-Tethered Worg | pack howl | root bite |
| Orc Blood-Tusk Captain | charge chain | enrage aura |
| Sporeheart Brute | poison cloud | slam AoE |
| Thornblade Grove Warden | root zones | ranged thorn volley |

### Boss 30 candidates

| Boss | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Rootbound Guardian | vine attacks | root adds + zones | arena roots + slam |
| Fungal Brood Sovereign | poison pools | spore summons | poison nova |
| Orc Root-Reaver Warchief | melee combos | warcry buffs | enrage + charge |

## 11.4 NÃ­veis 31â€“45 â€” Frost / Deep Subterranean

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Sunless Dwarf Miner | Tank Grunt | `move_direct_chase` | `ai_basic_melee` | Physical |
| Deep Hammer Guard | Tank | `move_slow_heavy_chase` | `ai_shield_guard`, `ai_area_slam` | Physical, `status_stun` |
| Black-Anvil Smith | Elite Support | `move_guard_node` | `ai_buff_allies`, `ai_area_slam` | Fire/Physical, `status_armor_up` |
| Runemarked Shieldbearer | Tank Support | `move_guard_position` | `ai_shield_guard`, aura | Physical, `status_armor_up` |
| Forge-Bellower Brute | Brute | `move_slow_heavy_chase` | `ai_area_slam`, `ai_shockwave` | Physical/Fire |
| Coal-Eyed Axeguard | Brute | `move_direct_chase` | `ai_combo_melee` | Physical |
| Ice Spider | Controller | `move_wall_ceiling_drop` | `ai_web_shot` | Ice/Poison, `status_chill`, `status_webbed` |
| Drow Scout | Skirmisher | `move_hit_and_run` | `ai_basic_melee`, evade | Physical/Shadow |
| Drow Hand-Crossbow Hunter | Archer | `move_ranged_keep_distance` | `ai_ranged_projectile` | Poison |
| Drow Spellblade | Hybrid Elite | `move_hit_and_run` | `ai_combo_melee`, `ai_ranged_projectile` | Shadow/Arcane |
| Drow Frostblade | Elite | `move_dash_arc` | `ai_combo_melee` | Ice/Shadow, `status_chill` |
| Drow Webcaller | Controller | `move_backline_caster` | `ai_web_shot`, `ai_summon_adds` | Webbed/Poison |
| Drow Shadow Duelist | Assassin | `move_short_blink` | `ai_blink_strike` | Shadow, `status_blind` |
| Frozen Skeleton | Grunt | `move_direct_chase` | `ai_basic_melee` | Ice/Physical, `status_chill` |
| Frost Wight | Elite | `move_phase_shift` | `ai_debuff_player`, `ai_basic_melee` | Shadow/Ice, `status_curse`, `status_chill` |
| Zombie Miner | Grunt | `move_slow_heavy_chase` | `ai_basic_melee` | Physical |
| Skeleton Knight | Tank | `move_guard_position` | `ai_shield_guard`, `ai_combo_melee` | Physical |
| Ice Ooze | Controller | `move_erratic_hop` | `ai_ground_pool` | Ice, `status_chill` |
| Frostbound Ghoul | Skirmisher | `move_fast_chase` | `ai_bite_and_retreat` | Ice/Poison |
| Cold Lantern Wraith | Flyer/Caster | `move_phase_shift` | `ai_ranged_projectile`, `ai_debuff_player` | Shadow/Ice, `status_fear` |
| Crystal Crawler | Skirmisher | `move_hit_and_run` | `ai_leap_attack` | Physical/Arcane |
| Frost Wolf | Skirmisher | `move_pack_circle` | `ai_charge_attack` | Ice/Physical, `status_chill` |
| Crystal Bat | Flyer | `move_flying_swoop` | `ai_flying_dive`, shard burst | Arcane |
| Shardback Lizard | Brute | `move_direct_chase` | `ai_area_slam` | Physical/Arcane |
| Glasshorn Beetle | Tank | `move_slow_heavy_chase` | `ai_charge_attack` | Physical, `status_knockback` |
| Snowblind Cave Bear | Brute | `move_slow_heavy_chase` | `ai_combo_melee`, roar | Ice/Physical, `status_blind` |

### Minibosses 35/40

| Miniboss | MecÃ¢nica 1 | MecÃ¢nica 2 |
|---|---|---|
| Black-Anvil Smith | armor aura | forge slam |
| Drow Frostblade Captain | blink slash | chill cone |
| Ice Spider Queen | web zones | spider adds |
| Deep Hammer Overseer | shockwave | guard stance |
| Frost Wight Commander | curse aura | summon skeletons |
| Crystal Crawler Prime | shard dash | crystal spikes |

### Boss 45 candidates

| Boss | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Gatebreaker of the Deep Forge | hammer slams | shield + shockwave | forge rage + falling rocks |
| Drow Frostblade Matriarch | frost blade combos | shadow blink adds | arena darkness + frost lines |
| Frost Wight Commander | undead melee | skeleton waves | curse field + chill nova |

## 11.5 NÃ­veis 46â€“60 â€” Fire / Warbands / Draconic

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Flame-Dance Raider | Skirmisher | `move_dash_arc` | `ai_combo_melee` | Fire/Physical, `status_burn` |
| Ember Axe Orc | Brute | `move_direct_chase` | `ai_area_slam`, `ai_combo_melee` | Fire/Physical |
| Senya War-Dancer | Support/Elite | `move_hit_and_run` | `ai_buff_allies`, `ai_combo_melee` | Fire, `status_enrage` |
| Kaand-Marked Berserker | Elite Brute | `move_fast_chase` | `ai_combo_melee`, `ai_boss_enrage` lite | Physical, `status_knockback` |
| Ash Hound | Skirmisher | `move_dash_line` | `ai_charge_attack` | Fire, `status_burn` |
| Orc Fire-Drummer | Support | `move_guard_position` | `ai_buff_allies` | `status_enrage` |
| Gnoll Bonechewer | Brute | `move_pack_circle` | `ai_basic_melee` | Physical, `status_bleed` |
| Gnoll Pack Hunter | Skirmisher | `move_pack_circle` | `ai_bite_and_retreat` | Physical |
| Gnoll Bloodhowler | Support | `move_guard_position` | `ai_debuff_player`, `ai_buff_allies` | `status_fear` futuro |
| Ruinblood Acolyte | Caster | `move_backline_caster` | `ai_ranged_projectile`, `ai_debuff_player` | Acid/Corruption |
| Acid-Breath Marauder | Brute | `move_direct_chase` | `ai_breath_cone`, melee | Acid |
| Kobold Dragon-Acolyte | Caster | `move_backline_caster` | `ai_ranged_projectile`, `ai_buff_allies` | Fire/Arcane |
| Drake Whelp | Skirmisher | `move_dash_line` | `ai_bite_and_retreat`, spit | Fire/Physical |
| Fire Drake | Elite | `move_dash_arc` ou `move_flying_swoop` por variaÃ§Ã£o | `ai_breath_cone`, `ai_charge_attack` | Fire, `status_burn` |
| Magma Ooze | Controller | `move_slow_heavy_chase` | `ai_ground_pool` | Fire, `status_burn` |
| Ember Bat | Flyer | `move_flying_swoop` | `ai_flying_dive` | Fire, `status_burn` |
| Living Ember | Swarm | `move_erratic_hop` | `ai_contact_damage` | Fire |
| Veyraathi Cultist | Caster | `move_backline_caster` | `ai_ranged_projectile`, `ai_debuff_player` | Fire/Corruption |
| Infernal Knife | Assassin | `move_hit_and_run` | `ai_combo_melee` | Fire/Physical |
| Abyssal Flamecaller | Elite Caster | `move_backline_caster` | `ai_ground_pool`, `ai_spread_projectile` | Fire, `status_burn`, `status_curse` |
| Diabrete de Brasa | Flyer/Caster | `move_flying_circle` | `ai_ranged_projectile` | Fire |
| Cinder Hexer | Debuffer | `move_backline_caster` | `ai_debuff_player` | Curse/Fire |

### Minibosses 50/55

| Miniboss | MecÃ¢nica 1 | MecÃ¢nica 2 |
|---|---|---|
| Gnoll Packlord | pack buff | bleed charge |
| Orc Flamecaller | fire zones | ally enrage |
| Ruinblood Scale-Priest | acid cone | cultist shield |
| Fire Drake | breath cone | dash/fly variant |
| Veyraathi Red Cinder | curse projectile | fire pool |
| Kaand-Marked Berserker Chief | enrage | shockwave leap |

### Boss 60 candidates

| Boss | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Ember Maw Wyvern | ground dash + tail | aerial dives | burn arena + poison tail |
| Gnoll Ash-Pack Prophet | pack waves | fire/fear howl | enrage pack + ash zones |
| Ruinblood Tyrant | acid/fire breath | summons acolytes | corruption aura + melee rage |

## 11.6 NÃ­veis 61â€“75 â€” Elyndor Ruins

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Runic Sentinel | Tank | `move_guard_position` | `ai_shield_guard`, `ai_basic_melee` | Physical/Arcane |
| Animated Armor | Grunt/Tank | `move_direct_chase` | `ai_combo_melee` | Physical |
| Relic Drone | Ranged | `move_stationary_turret` | `ai_ranged_projectile` | Arcane |
| Stone Golem-like Guardian | Brute/Tank | `move_slow_heavy_chase` | `ai_area_slam`, `ai_shockwave` | Physical |
| Elyndor Gatekeeper | Elite | `move_guard_node` | `ai_beam_attack`, `ai_shield_guard` | Arcane |
| Broken Rune Turret | Turret | `move_stationary_turret` | `ai_beam_attack` | Arcane |
| Archive Shield-Form | Support/Tank | `move_guard_position` | `ai_buff_allies`, shield | Arcane, `status_armor_up` |
| Ninrorin Gate-Touched Mage | Caster | `move_backline_caster` | `ai_ranged_projectile`, `move_short_blink` | Arcane |
| Planar Scholar | Caster | `move_backline_caster` | `ai_debuff_player`, projectile | Arcane, `status_arcane_mark` |
| Arcane Duelist | Hybrid | `move_hit_and_run` | `ai_combo_melee`, arcane shot | Arcane/Physical |
| Broken Planewalker | Elite | `move_short_blink` | `ai_blink_strike`, projectile | Arcane |
| Arcane Wisp | Flyer | `move_flying_circle` | `ai_ranged_projectile` | Arcane |
| Portal Scribe | Summoner | `move_summoner_keepaway` | `ai_summon_adds` | Arcane |
| Gem Hermit | Caster | `move_backline_caster` | shard projectile | Arcane |
| Crystal Channeler | Caster | `move_stationary_turret` | `ai_beam_attack` | Arcane |
| Radiant Shardguard | Tank | `move_guard_position` | shield + melee | Physical/Arcane |
| Gem-Crazed Prospector | Elite | `move_direct_chase` | pickaxe combo + shard burst | Physical/Arcane |
| Clockwork Handler | Summoner | `move_summoner_keepaway` | summons clockwork adds | Physical/Arcane |
| Drow Portalist | Caster | `move_short_blink` | portals + projectile | Arcane/Shadow |
| Drow House Assassin | Assassin | `move_short_blink` | `ai_blink_strike` | Poison/Shadow |
| Drow Spellblade | Hybrid | `move_hit_and_run` | melee + spell | Shadow/Arcane |
| Portal Spider | Controller | `move_short_blink` | web + blink | Poison/Arcane |
| Floating Eye | Ranged | `move_flying_circle` | `ai_beam_attack` | Arcane |
| Eye of the Broken Gate | Miniboss | `move_flying_circle` | beams + summons | Arcane/Shadow |
| Portal Maw | Controller | `move_stationary_turret` | pull + summon | Arcane |
| Tyrant Eye Fragment | Elite | `move_flying_circle` | rotating beams lite | Arcane |
| Lesser Observador de Elyndor | Elite | `move_flying_circle` | beams/debuff | Arcane, `status_blind` |
| Mind-Glare Orb | Caster | `move_flying_circle` | debuff beam | `status_confusion`, Arcane |

### Minibosses 65/70

| Miniboss | MecÃ¢nica 1 | MecÃ¢nica 2 |
|---|---|---|
| Eye of the Broken Gate | beam rotation | portal adds |
| Ninrorin Broken Planewalker | blink strike | arcane mark |
| Gem-Crazed Prospector | shard traps | pickaxe burst |
| Runic Golem | shockwave | armor up |
| Drow Portalist Captain | portal blink | shadow bolts |
| Prism Shardguard Prime | reflective shield | prism beam |

### Boss 75 candidates

| Boss | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Relic Sentinel of Elyndor | melee/beam | summons sentinels | arena rune beams |
| Observador do Arco Partido | eye beams | portal pulls | rotating beams + blind |
| Ninrorin Gate-Sealer | arcane duel | seal zones | portal storm |

## 11.7 NÃ­veis 76â€“90 â€” Shadow Abyss

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Skeleton Knight | Tank | `move_guard_position` | shield + melee | Physical |
| Zombie Miner | Grunt | `move_slow_heavy_chase` | melee | Physical |
| Ghoul | Skirmisher | `move_fast_chase` | bite + retreat | Poison/Shadow |
| Wight | Elite | `move_phase_shift` | debuff + melee | Shadow, `status_curse` |
| Wraith-like Shadow | Assassin/Flyer | `move_phase_shift` | phase strike | Shadow, `status_fear` |
| Lich Fragment | Caster | `move_backline_caster` | projectile + summon | Arcane/Shadow, `status_curse` |
| Vampire Spawn | Elite | `move_fast_chase` | melee + lifesteal | Physical/Shadow, `status_lifesteal` |
| Bloodbound Noble | Elite | `move_hit_and_run` | rapier combo + charm/fear future | Shadow |
| Blood Rapier Duelist | Assassin | `move_dash_arc` | combo burst | Physical, `status_bleed` |
| Bat Swarm | Swarm/Flyer | `move_flying_swoop` | contact swarm | Physical, `status_blind` |
| Night-Tusk Scout | Assassin | `move_hit_and_run` | ambush strike | Shadow/Physical |
| Shadow Shaman | Caster | `move_backline_caster` | curse + shadow bolt | Shadow, `status_curse` |
| Moonless Spear | Elite | `move_dash_line` | spear charge | Shadow/Physical |
| Nyx-Bound Cave Prophet | Miniboss/Caster | `move_summoner_keepaway` | summons + fear | Shadow, `status_fear`, `status_curse` |
| Veyraathi Cultist | Caster | `move_backline_caster` | fire/shadow projectile | Fire/Corruption |
| Abyssal Flamecaller | Elite Caster | `move_backline_caster` | fire pool + curse | Fire, `status_burn`, `status_curse` |
| Veyraath's Red Herald | Miniboss/Elite | `move_boss_arena` | fire/corruption patterns | Fire/Corruption |
| Infernal Knife | Assassin | `move_hit_and_run` | melee burst | Fire/Physical |
| Blackstone Horror | Elite Aberration | `move_erratic_hop` | corruption pools | Corruption/Shadow |
| Void Ooze | Ooze | `move_erratic_hop` | ground pool | Shadow/Corruption |
| Portal-Torn Beast | Brute | `move_short_blink` | leap + blink | Physical/Arcane |
| Shadow Slime | Ooze | `move_erratic_hop` | contact + pool | Shadow, `status_shadow_mark` |
| Broken Eye Fragment | Ranged | `move_flying_circle` | beam attack | Arcane/Shadow |
| No-Light Maw | Brute | `move_slow_heavy_chase` | pull + bite | Shadow |
| Crawling Rift Flesh | Swarm/Brute | `move_erratic_hop` | contact + corruption | Corruption |

### Minibosses 80/85

| Miniboss | MecÃ¢nica 1 | MecÃ¢nica 2 |
|---|---|---|
| Vampire Spawn Lord | lifesteal combo | bat swarm |
| Nyx-Bound Cave Prophet | fear field | shadow summons |
| Blackstone Horror Prime | corruption pools | blink slam |
| Wight Commander | curse aura | undead wave |
| Veyraath's Red Herald | fire/corruption bolt | curse zone |
| Moonless Spear Champion | spear dash | shadow mark |

### Boss 90 candidates

| Boss | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Hollow Lich of the Black Stone | shadow bolts | summons undead | blackstone curse + teleport |
| Bloodbound Court Patriarch/Matriarch | melee lifesteal | vampire adds | blood/shadow arena |
| Prophet of the Moonless Gate | fear/shadow | orc/shadow adds | moonless curse storm |

## 11.8 NÃ­veis 91â€“99 â€” Corrupted Core

| Criatura | Role | Movimento | Comportamento | Status/Dano |
|---|---|---|---|---|
| Blackstone Drake | Elite | `move_dash_line` ou `move_flying_swoop` | breath + claw | Corruption/Physical |
| Crystal Drake | Elite | `move_dash_arc` | shard breath + dash | Arcane |
| Drake Whelp | Skirmisher | `move_dash_line` | bite + spit | Physical/Elemental |
| Young Dragon-like Boss | Boss | `move_boss_arena`, aerial/ground mix | breath/wing/tail | Elemental |
| Meteor Elemental | Elite | `move_slow_heavy_chase` | slam + projectile | Arcane/Fire |
| Obsidian Wing | Flyer | `move_flying_swoop` | dive + shard | Physical/Shadow |
| Corefire Drake | Elite | `move_dash_arc` | fire breath + dash | Fire/Corruption |
| Fallen Judge of Kanthor | Miniboss/Elite | `move_guard_position` | thunder strike + judgment | Thunder/Physical |
| Ashen Judge | Tank | `move_slow_heavy_chase` | shield + melee | Physical/Thunder |
| Thunder-Sentence Guard | Tank | `move_guard_node` | shockwave | Thunder |
| Grey Scale Inquisitor | Caster/Tank | `move_backline_caster` | judgment beam | Thunder/Arcane |
| Broken Elyndor Golem | Miniboss | `move_slow_heavy_chase` | slam + beam | Physical/Arcane |
| Lich Fragment | Caster | `move_backline_caster` | projectile + summon | Shadow/Arcane |
| Cavaleiro do PortÃ£o Morto | Tank Elite | `move_guard_position` | melee + curse | Physical/Shadow |
| Wight Commander | Elite | `move_phase_shift` | undead command | Shadow/Curse |
| Spectral Scholar | Caster | `move_phase_shift` | arcane projectile | Arcane/Shadow |
| Blackstone Horror | Elite | `move_erratic_hop` | pools + slam | Corruption |
| Broken Eye Tyrant | Miniboss | `move_flying_circle` | rotating beams | Arcane/Shadow |
| Portal Maw | Controller | `move_stationary_turret` | pull + summon | Arcane |
| Mind Horror | Caster | `move_phase_shift` | confusion/fear beams | Arcane/Shadow |
| Observador de Elyndor | Elite/Boss | `move_flying_circle` | gaze beams | Arcane |
| Veyraath's Red Herald | Elite/Miniboss | `move_boss_arena` | corruption/fire patterns | Fire/Corruption |
| Ruinblood Tyrant | Boss/Elite | `move_slow_heavy_chase` | acid breath + melee | Acid/Corruption |
| Veyraath-Touched Scale | Elite | `move_direct_chase` | breath + claw | Acid/Corruption |
| Core Ooze | Ooze Elite | `move_erratic_hop` | corruption pools | Corruption |

### Minibosses 94/97

| Miniboss | MecÃ¢nica 1 | MecÃ¢nica 2 |
|---|---|---|
| Blackstone Drake | corruption breath | ground/aerial dash by variant |
| Fallen Judge of Kanthor | thunder judgment | shield phase |
| Lich Fragment Prime | summon undead | curse projectile |
| Broken Eye Tyrant | rotating beams | blind gaze |
| Veyraath's Red Herald | corruption pool | fire curse |
| Broken Elyndor Golem | beam sweep | shockwave slam |

### Boss 99 candidates

| Boss | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Three Gatebound Elites | elite 1 | elite 2 joins | all surviving elites enrage |
| Blackstone Drake Sovereign | ground breath | aerial dive | corruption storm |
| Council of Broken Arches | construct/caster phase | eye/portal phase | mixed arcane collapse |

## 11.9 Level 100 â€” Arco Central de Elyndor

| Final Boss | Movimento | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|---|
| The Portal-Bound Ancient | `move_boss_arena`, `move_short_blink` | physical/arcane melee | opens portals and summons prior families | Red/Black meteor phase, arena collapse |
| Blackstone Dragon of the Central Arch | aerial/ground hybrid | ground claw/breath | aerial dive + corruption pools | blackstone storm + wing shockwaves |
| Meteor Lich of Elyndor | caster/summoner | arcane bolts + curse | summons undead/constructs | meteor portal storm + phylactery shield |

Status:

```text
The Portal-Bound Ancient: status_arcane_mark, status_corruption, status_knockback
Blackstone Dragon: status_burn, status_corruption, status_fear
Meteor Lich: status_curse, status_shadow_mark, status_arcane_mark
```

---

# 12. Armas elementais humanoides

Humanoides podem usar armas elementais com chance baixa.

## 12.1 Weapon Affix IDs

| AffixId | Efeito |
|---|---|
| `affix_fire_touched` | adiciona Fire e chance de Burn |
| `affix_frost_touched` | adiciona Ice e chance de Chill |
| `affix_shadow_touched` | adiciona Shadow e chance de Shadow Mark |
| `affix_arcane_touched` | adiciona Arcane e chance de Arcane Mark |
| `affix_poisoned` | adiciona Poison |
| `affix_blackstone` | adiciona Corruption |
| `affix_thunder_rune` | adiciona Thunder/Stun futuro |
| `affix_bloodbound` | adiciona Lifesteal/Bleed futuro |

## 12.2 Drop chance inicial

| Inimigo | Chance de arma elemental |
|---|---:|
| Common humanoid | 1% |
| Strong humanoid | 2% |
| Elite humanoid | 5% |
| Miniboss humanoid | 15% |
| Boss humanoid | 25% |

Hardening:

```text
Drop inicial pode ser versÃ£o danificada/quebrada.
Arma pode exigir reparo/refino/crafting para uso pleno.
Chance final deve passar por balance posterior.
```

---

# 13. CritÃ©rios de aceite

## CA1 â€” Todo inimigo tem perfil de combate

Cada inimigo da FASE9G deve possuir ou herdar:

```text
RoleId
MovementPatternId
CombatBehaviorIds
DamageTypeIds
StatusIds
Resistance/Vulnerability profile
```

## CA2 â€” Todo ataque especial tem telegraph

Ataques especiais precisam de telegraph ou janela clara de resposta.

## CA3 â€” Status nÃ£o encadeiam controle injusto

ImplementaÃ§Ã£o deve evitar chain control sem recovery window.

## CA4 â€” Boss tem 3 fases na spec

Cada boss candidate precisa ter 3 fases definidas.

## CA5 â€” Miniboss tem 2 mecÃ¢nicas

Cada miniboss precisa ter 2 mecÃ¢nicas especiais definidas.

## CA6 â€” Aerial fallback existe

Criaturas aÃ©reas precisam de fallback terrestre/placeholder quando a arena/pathing nÃ£o suportar voo real.

## CA7 â€” Armas elementais sÃ£o data-driven

Affixes e drops elementais nÃ£o devem ser hardcoded em inimigos individuais.

## CA8 â€” DamageCalculator Ãºnico

Todo dano/status usa o fluxo do DamageCalculator/Status system definido nas specs anteriores.

## CA9 â€” Debug futuro

Quando implementado, DebugHud deve conseguir mostrar:

```text
EnemyRole
MovementPattern
CombatBehavior
StatusIds
DamageTypes
```

---

# 14. Non-goals

Fora deste amendment:

```text
IA final de produÃ§Ã£o
animaÃ§Ãµes finais
balance final de dano/HP
arte/sprites finais
sistema completo de dodge do player
sistema completo de hitbox/hurtbox avanÃ§ado
bosses finais totalmente implementados
VFX final de status
som final de telegraphs
```

---

# 15. Impacto em specs anteriores

Este amendment complementa:

- `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`

NÃ£o altera destrutivamente a FASE9G v1.0.

ImplementaÃ§Ã£o futura deve tratar este amendment como fonte para:

- EnemyCombatProfile data;
- AI behavior selection;
- status mapping;
- boss/miniboss movesets;
- weapon affix drops;
- debug de combate.


