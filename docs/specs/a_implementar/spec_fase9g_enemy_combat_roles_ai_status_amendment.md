# SPEC FUTURA — FASE9G Enemy Combat Roles AI Status Amendment

> Origem histórica: $Source
> Status: A implementar / restante não implementado
> Observação: conteúdo histórico preservado; não duplicar capacidades já consolidadas em specs implementadas.

---

## Escopo preservado

# FASE9G Amendment v1.1 — Enemy Combat Roles, AI, Status & Movesets

> **Status:** amendment aprovado para orientar próximas waves.  
> **Tipo:** complemento da `FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0`.  
> **Não substitui:** FASE9G v1.0.  
> **Objetivo:** detalhar como criaturas, elites, minibosses e bosses da cave lutam, se movem, aplicam status, usam armas elementais, escalam por nível e respeitam legibilidade/balance do combate action.

---

## 1. Avaliação de qualidade e hardening aplicado

O rascunho base estava bom em cobertura de criaturas e intenção de combate, mas precisava de hardening em quatro pontos:

1. **Legibilidade do combate:** muitos inimigos com status/mecânicas poderiam virar caos visual se não houver telegraph, cooldown e limite de simultaneidade.
2. **Status stacking:** permitir múltiplos status é bom, mas precisa de regras de orçamento por tipo de inimigo.
3. **Aerial/flying behavior:** drakes, bats e wyverns precisam de fallback terrestre e limites de colisão/pathing.
4. **Boss/miniboss:** três fases e duas mecânicas são bons, mas precisam de regra mínima de fase, telegraph e janela de resposta do jogador.

Este amendment adiciona:

- contratos de `MovementPatternId`, `CombatBehaviorId`, `StatusId`, `DamageType` e `EnemyCombatProfile`;
- orçamento de status por tier;
- regras de telegraph/cooldown;
- regras de spawn composition por sala;
- regras para armas elementais e drops;
- matriz de movimento/comportamento por família de criatura;
- movesets de minibosses e bosses;
- critérios de aceite para implementação futura.

---

## 2. Decisões fechadas

```text
D1: Combate da cave deve ser action RPG simples, com padrões legíveis, movimentação ativa e fases.
D2: Inimigos podem ter mais de 1 status principal.
D3: Bosses devem ter 3 fases na spec.
D4: Minibosses devem ter 2 mecânicas especiais.
D5: Fear, Curse, Blind, Chill e outros status entram já como IDs de contrato.
D6: Humanoides podem usar armas elementais já no MVP/near-MVP.
D7: Armas elementais têm chance baixa de drop.
D8: Wyverns/Drakes variam por tipo: alguns aéreos, alguns terrestres/dash, alguns híbridos.
D9: Toda criatura precisa ter MovementPattern + CombatBehavior + StatusProfile.
D10: Toda mecânica forte precisa de telegraph ou janela de resposta.
```

---

## 3. Hardening rules obrigatórias

### H1 — Telegraph obrigatório

Todo ataque especial deve ter aviso visual/temporal antes de aplicar dano/status.

Exemplos:

```text
charge attack -> windup curto antes da investida
ground pool -> marca no chão antes de ativar
beam attack -> linha/olho carregando antes do raio
boss slam -> animação/flash antes do impacto
```

### H2 — Cooldown mínimo por comportamento especial

Ataques especiais não podem ser spammados.

Sugestão inicial:

| Behavior | Cooldown mínimo |
|---|---:|
| charge/leap | 2.0s |
| web/root/trap | 3.0s |
| ground pool | 4.0s |
| summon adds | 8.0s |
| beam/rotating beam | 5.0s |
| boss phase skill | 6.0s |

### H3 — Status budget por tier

| Tier | Status ativos permitidos no design |
|---|---:|
| Common | 0–2 |
| Strong | 1–2 |
| Elite | 1–3 |
| Miniboss | 2–4 |
| Boss | 3+ por fases |

Regra: não significa que todos aplicam tudo ao mesmo tempo. Cada status deve ter chance/cooldown/duração.

### H4 — Spawn composition budget

Para evitar salas injustas, cada encounter deve respeitar composição máxima.

Sugestão por sala comum:

```text
até 1 Elite
até 1 Controller pesado
até 1 Caster forte
até 2 Ranged
até 4–8 Grunts/Swarm, conforme nível
```

Miniboss room:

```text
1 Miniboss
0–2 elites menores
adds controlados por fase ou cooldown
```

Boss arena:

```text
1 Boss
adds apenas em fases específicas
limite de adds simultâneos
```

### H5 — No unavoidable chain control

Status de controle forte não podem encadear sem janela de recuperação.

```text
root + stun + freeze + fear não devem manter jogador sem ação continuamente.
```

### H6 — Aerial fallback

Toda criatura aérea deve ter fallback se o path/arena não suportar voo real.

```text
Flying real disponível -> usar move_flying_swoop/move_flying_circle.
Sem suporte -> usar dash terrestre, leap ou projectile placeholder.
```

### H7 — DamageCalculator único

Todo dano deve passar pelo `DamageCalculator` definido na FASE9E. Não criar dano paralelo.

### H8 — Sem hardcode de status solto

Status devem ser referenciados por `StatusId`, resolvidos por data/SO/registry quando implementado.

---

## 4. StatusIds oficiais da cave

| StatusId | Tipo | Efeito conceitual |
|---|---|---|
| `status_burn` | DoT | dano Fire por tempo |
| `status_poison` | DoT | dano Poison por tempo |
| `status_bleed` | DoT físico | dano físico por tempo, futuro |
| `status_chill` | slow | reduz movimento/ação |
| `status_freeze` | hard CC | imobiliza curto, futuro |
| `status_fear` | controle | força recuo/desorganização, futuro |
| `status_blind` | debuff | reduz precisão/percepção, futuro |
| `status_curse` | debuff | reduz atributo/resistência |
| `status_shadow_mark` | debuff | aumenta dano Shadow recebido |
| `status_arcane_mark` | debuff | aumenta dano Arcane recebido |
| `status_corruption` | DoT/debuff | dano e redução futura de cura |
| `status_stun` | hard CC | interrupção curta |
| `status_knockback` | displacement | empurra |
| `status_root` | controle | prende no lugar |
| `status_webbed` | controle | slow/root por teia |
| `status_armor_up` | buff | reduz dano recebido |
| `status_enrage` | buff | aumenta dano/velocidade |
| `status_lifesteal` | buff | cura pequena ao causar dano |
| `status_silence` | debuff | impede magia, futuro |
| `status_confusion` | controle | movimento/ação errática, futuro |

---

## 5. DamageTypes

| DamageType | Uso |
|---|---|
| `Physical` | mordida, arma, garra, impacto |
| `Fire` | fogo, magma, brasa |
| `Ice` | gelo, geada, cristal frio |
| `Poison` | veneno, fungo, aranha |
| `Shadow` | Nyx, mortos, drows, escuridão |
| `Arcane` | Elyndor, portais, magia pura |
| `Lightning` | draconatos, runas, storm |
| `Acid` | ruína, draconatos, oozes |
| `Corruption` | Pedra Negra, Veyraath |
| `Thunder` | martelo rúnico, julgamento, choque |

MVP pode implementar subset, mas os IDs devem estar reservados.

---

## 6. MovementPatternIds

| MovementPatternId | Descrição |
|---|---|
| `move_idle_guard` | guarda posição até aggro |
| `move_direct_chase` | persegue em linha direta |
| `move_slow_heavy_chase` | persegue lentamente com massa alta |
| `move_fast_chase` | perseguição rápida |
| `move_hop_chase` | avança em pulos |
| `move_erratic_hop` | pulo irregular/imprevisível |
| `move_pack_circle` | tenta circular o jogador em grupo |
| `move_hit_and_run` | aproxima, ataca, recua |
| `move_ranged_keep_distance` | mantém distância ideal |
| `move_backline_caster` | recua e conjura |
| `move_patrol_route` | patrulha pontos |
| `move_ambush_stationary` | fica oculto até trigger |
| `move_burrow_ambush` | emerge do chão |
| `move_wall_ceiling_drop` | cai de parede/teto |
| `move_flying_swoop` | voo com mergulho |
| `move_flying_circle` | orbita e ataca de longe |
| `move_short_blink` | teleporte curto |
| `move_phase_shift` | atravessa/evade por fase |
| `move_guard_node` | defende node/baú/portal |
| `move_guard_position` | defende região ou aliado |
| `move_boss_arena` | movimento por fases em arena |
| `move_dash_line` | investida reta |
| `move_dash_arc` | dash em arco |
| `move_summoner_keepaway` | foge e invoca |
| `move_stationary_turret` | quase parado, ataca Ã  distância |

---

## 7. CombatBehaviorIds

| CombatBehaviorId | Descrição |
|---|---|
| `ai_contact_damage` | dano por contato |
| `ai_basic_melee` | ataque melee simples |
| `ai_combo_melee` | combo curto |
| `ai_charge_attack` | investida telegrafada |
| `ai_leap_attack` | salto/impacto |
| `ai_bite_and_retreat` | morde e recua |
| `ai_ranged_projectile` | projétil simples |
| `ai_spread_projectile` | vários projéteis |
| `ai_arc_projectile` | projétil em arco |
| `ai_ground_pool` | cria poça/área |
| `ai_trap_place` | coloca armadilha |
| `ai_web_shot` | teia/slow/root |
| `ai_root` | prende por raiz |
| `ai_buff_allies` | buffa aliados |
| `ai_debuff_player` | aplica debuff |
| `ai_summon_adds` | invoca adds |
| `ai_heal_or_lifesteal` | cura ou rouba vida |
| `ai_shield_guard` | bloqueia/reduz dano |
| `ai_breath_cone` | cone elemental |
| `ai_tail_sting` | ferrão/cauda |
| `ai_flying_dive` | mergulho aéreo |
| `ai_blink_strike` | teleporta e ataca |
| `ai_beam_attack` | raio/olhar |
| `ai_rotating_beams` | beams em rotação |
| `ai_area_slam` | pancada AoE |
| `ai_shockwave` | onda de choque |
| `ai_boss_phase_switch` | troca fase por HP |
| `ai_boss_minion_wave` | fase de invocação |
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

Fórmulas alvo:

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

## 10. Resistências e vulnerabilidades por família

| Família | Resistência | Vulnerabilidade |
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
| Beasts | nenhuma padrão | depende do subtipo |

---

# 11. Combat matrix por faixa

## 11.1 Níveis 1–10 — Local Caves

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

## 11.2 Níveis 11–15 — Meteor-Touched Threshold

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
| Meteor Ooze King | `move_boss_arena`, `move_erratic_hop` | contact + jump | invoca slimes | projéteis meteóricos + pools | `status_arcane_mark`, slow, `status_knockback` |
| Goblin Bloodfang Butcher | `move_dash_arc` | melee combos | chama goblins | enrage + bleed strikes | `status_bleed`, `status_enrage` |
| Kobold Tunnel Tyrant | `move_guard_node` | traps + sling | chama kobolds | cave-in/shockwave | `status_stun`, `status_knockback` |

## 11.3 Níveis 16–30 — Underground Forest

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

| Miniboss | Mecânica 1 | Mecânica 2 |
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

## 11.4 Níveis 31–45 — Frost / Deep Subterranean

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

| Miniboss | Mecânica 1 | Mecânica 2 |
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

## 11.5 Níveis 46–60 — Fire / Warbands / Draconic

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
| Fire Drake | Elite | `move_dash_arc` ou `move_flying_swoop` por variação | `ai_breath_cone`, `ai_charge_attack` | Fire, `status_burn` |
| Magma Ooze | Controller | `move_slow_heavy_chase` | `ai_ground_pool` | Fire, `status_burn` |
| Ember Bat | Flyer | `move_flying_swoop` | `ai_flying_dive` | Fire, `status_burn` |
| Living Ember | Swarm | `move_erratic_hop` | `ai_contact_damage` | Fire |
| Veyraathi Cultist | Caster | `move_backline_caster` | `ai_ranged_projectile`, `ai_debuff_player` | Fire/Corruption |
| Infernal Knife | Assassin | `move_hit_and_run` | `ai_combo_melee` | Fire/Physical |
| Abyssal Flamecaller | Elite Caster | `move_backline_caster` | `ai_ground_pool`, `ai_spread_projectile` | Fire, `status_burn`, `status_curse` |
| Diabrete de Brasa | Flyer/Caster | `move_flying_circle` | `ai_ranged_projectile` | Fire |
| Cinder Hexer | Debuffer | `move_backline_caster` | `ai_debuff_player` | Curse/Fire |

### Minibosses 50/55

| Miniboss | Mecânica 1 | Mecânica 2 |
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

## 11.6 Níveis 61–75 — Elyndor Ruins

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

| Miniboss | Mecânica 1 | Mecânica 2 |
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

## 11.7 Níveis 76–90 — Shadow Abyss

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

| Miniboss | Mecânica 1 | Mecânica 2 |
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

## 11.8 Níveis 91–99 — Corrupted Core

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
| Cavaleiro do Portão Morto | Tank Elite | `move_guard_position` | melee + curse | Physical/Shadow |
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

| Miniboss | Mecânica 1 | Mecânica 2 |
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

## 11.9 Level 100 — Arco Central de Elyndor

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
Drop inicial pode ser versão danificada/quebrada.
Arma pode exigir reparo/refino/crafting para uso pleno.
Chance final deve passar por balance posterior.
```

---

# 13. Critérios de aceite

## CA1 — Todo inimigo tem perfil de combate

Cada inimigo da FASE9G deve possuir ou herdar:

```text
RoleId
MovementPatternId
CombatBehaviorIds
DamageTypeIds
StatusIds
Resistance/Vulnerability profile
```

## CA2 — Todo ataque especial tem telegraph

Ataques especiais precisam de telegraph ou janela clara de resposta.

## CA3 — Status não encadeiam controle injusto

Implementação deve evitar chain control sem recovery window.

## CA4 — Boss tem 3 fases na spec

Cada boss candidate precisa ter 3 fases definidas.

## CA5 — Miniboss tem 2 mecânicas

Cada miniboss precisa ter 2 mecânicas especiais definidas.

## CA6 — Aerial fallback existe

Criaturas aéreas precisam de fallback terrestre/placeholder quando a arena/pathing não suportar voo real.

## CA7 — Armas elementais são data-driven

Affixes e drops elementais não devem ser hardcoded em inimigos individuais.

## CA8 — DamageCalculator único

Todo dano/status usa o fluxo do DamageCalculator/Status system definido nas specs anteriores.

## CA9 — Debug futuro

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
IA final de produção
animações finais
balance final de dano/HP
arte/sprites finais
sistema completo de dodge do player
sistema completo de hitbox/hurtbox avançado
bosses finais totalmente implementados
VFX final de status
som final de telegraphs
```

---

# 15. Impacto em specs anteriores

Este amendment complementa:

- `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md`

Não altera destrutivamente a FASE9G v1.0.

Implementação futura deve tratar este amendment como fonte para:

- EnemyCombatProfile data;
- AI behavior selection;
- status mapping;
- boss/miniboss movesets;
- weapon affix drops;
- debug de combate.


## Regra de uso

Antes de implementar, reconciliar este material com docs/specs/implementados/, docs/refinements/implementados/ e o estado real do código.


