# ENEMY ATTACK CATALOG — DIRECTION v1.1

> **Status:** DIRECTION (design canônico de ataques — ainda não implementado/wired)
> **Data:** 2026-07-03 (v1.1 no mesmo dia: cobertura estendida do Roster 60 para o universo completo)
> **Escopo:** o **universo completo de 178 IDs de inimigo**, organizado assim (terminologia oficial — NÃO usar mais "legadas vs. canônicas"):
> - **O CATÁLOGO: 117 fichas** do `CanonicalBestiaryCatalog` (`Assets/_Game/Scripts/Combat/Bestiary/`): 113 criaturas nas 7 bands (Stone/Fungal/Ice/Fire/Ruins/Deep/Void) + 4 chefes finais (`boss_*`, nível 101) — assets em `Assets/_Game/Data/Enemies/Canonical/`. **Fonte de verdade.**
> - **60 IDs do Roster em jogo** (`Assets/_Game/Data/Enemies/Roster/enemy_*.asset`) — o que a cave spawna HOJE. Reclassificados pelo crosswalk da §4A: **53 são VARIÂNCIAS** de criaturas do catálogo (reskin/renomeação — herdam o kit de ataques da criatura-mãe) e **7 são NOVAS** (kit próprio; candidatas a ganhar ficha no catálogo).
> - **1 boss extra**: `enemy_meteor_ooze_king` (Stone, lvl 15, fora do catálogo; kit próprio na §4B.9).
> **Fontes:** `docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md`, `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`, `.specs/implementados/spec_fable_80_bestiary_expansion_40_creatures_vaalara.md`, `tools/aseprite/monster_overrides.json`, `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`, D&D 5e (analogias de stat block por criatura).
> **Sistemas alvo (já existem, data-driven):** `EnemyActionSO` (13 `EnemyActionType`), `EnemyActionSetSO`, `EnemyActionRunner`, `EnemyTelegraphController`, `EnemyProjectileBehaviour`, `StatusEffectDatabaseSO`, `EnemyMovementType` (22 tipos).

---

## 1. Regras de design

1. **Toda criatura comum/elite tem 2 OU 3 ataques.** Kit de 2 = **Melee Normal + Especial** (o padrão). Kit de 3 = **Melee (fallback) + Ranged Normal + Especial** — para toda criatura com ataque à distância justificado. Nas tabelas deste doc, criaturas com Dist **R** têm kit de 3: o ataque listado como "Normal" é o ranged, e o melee de fallback segue a regra 1b abaixo. **Minibosses têm +1 especial (S1/S2); gate bosses têm 1 especial por fase** (`BossPhaseShift` já troca ActionSet por fase).
   - **1b. Melee de fallback por plano de corpo** (para os kits de 3, usado quando o player cola — `MinRange` no ranged força a troca via `SelectBestAction`): humanoide caster/atirador → `atk_slash` fraco (adaga/cajado/coronhada); besta/dracônico → `atk_bite`; construct → `atk_slam`; planta/fungo → `atk_whip`; espectro/elemental flutuante → `atk_claw` (toque). Dano ~60% do ranged, sem status.
2. **Ranged só onde a lore justifica.** No universo completo, ~25% têm projétil verdadeiro e ~6% cone curto (sopro/jato); o resto é melee/mobilidade — pressão corpo a corpo é o padrão da caverna.
3. **Mecânica compartilhada, arte compartilhada.** Ataques do mesmo **arquétipo** usam o mesmo `EnemyActionType` + parâmetros e a **mesma folha-modelo de animação** (re-skinada por criatura). Um golpe de espada é UM arquétipo — o Cracked Bone, o Sealed Knight e o Clockwork Guard reutilizam o template `atk_slash`, cada um com seu corpo.
4. **Especiais únicos existem, mas poucos.** Só quando o arquétipo não expressa a criatura (ex.: Rearticular do esqueleto, Imagem Espelhada do Mirror Adept). Cada especial único lista sua exigência mecânica na §5.
5. **IDs estáveis:** todo arquétipo tem slug `atk_*` (const no futuro catálogo C#, rule `id-stability`). Ações concretas seguem `action_{enemyId sem prefixo}_{normal|special}`.
6. **Telegraph obrigatório no especial:** todo especial usa `WindupSeconds >= 0.6` + `TelegraphProfileId` (piscar de cor já implementado) — e, quando a arte de animação existir, os frames de windup do template.

---

## 2. Biblioteca de arquétipos mecânicos (compartilhados)

| Slug | Nome | `EnemyActionType` | Perfil típico | Quem usa (exemplos) |
|---|---|---|---|---|
| `atk_slash` | Golpe de Lâmina | MeleeAttack | dano médio, windup 0.3–0.5s, range 1.2 | espadas/adagas/lâminas curvas (goblin, drow, knights, clockwork) |
| `atk_cleave` | Talho Pesado | MeleeAttack | dano alto, windup 0.7–0.9s, posture +, range 1.4 | machado/mace/picareta/montante (orc, warden, duergar, void knight) |
| `atk_thrust` | Estocada | MeleeAttack | dano médio, range 1.8 (alcance estendido) | lanças (ninrorin void sentinel) |
| `atk_bite` | Mordida | MeleeAttack | dano baixo-médio, windup 0.2–0.3s, range 0.9 | bestas, vermes, wyrmlings, ticks |
| `atk_claw` | Garra/Toque | MeleeAttack | dano baixo, rápido, range 1.0 | felinos, dracônicos, sombras espectrais |
| `atk_slam` | Pancada de Massa | MeleeAttack | dano alto, windup 1.0s, posture ++, knockback | golems, bulwarks, tanks |
| `atk_bash` | Encontrão | MeleeAttack + knockback forte | dano baixo, empurrão 2–3 tiles, Stun 0.5s opcional | escudeiros/guardas (shadow warden, void sentinel) |
| `atk_charge` | Investida | MultiHitCharge | telegraph 0.8s + linha reta, atinge trajetória | ash crawler, hollow stagling, void reaver |
| `atk_leap` | Bote | LeapStrike | salto no player + impacto | leapers, scavenger, wyvern (rasante) |
| `atk_whip` | Chicote de Raiz/Vinha | MeleeAttack | range 1.6–2.0, arco frontal | plantas/fungos (blackroot, rootsnare, mycobulwark) |
| `atk_bow` | Tiro de Arco/Besta | RangedProjectile | projétil dodgável, range 6–8 | thorn archer (e gravedelver crossbowman fora do roster) |
| `atk_throw` | Arremesso | RangedProjectile | projétil lento, range 4–6 | funda do kobold, dardo do trapper, engenhoca do tinker |
| `atk_spit` | Cuspe | RangedProjectile | projétil orgânico, range 4–5, status comum | spitters (cinder, void, ashspitter), spore imp |
| `atk_cast` | Projétil Mágico | CastProjectile (estado CastPrepare) | windup 0.8s+, projétil elemental | casters (adept, acolytes, lich shard, ashcaller) |
| `atk_breath` | Sopro em Cone | TelegraphedAoE frontal | cone curto range 2.5–3.5, elemental | dracônicos, furnace warden |
| `atk_nova` | Pulso em Área | AreaPulse / TelegraphedAoE (self) | raio 2–2.5 ao redor de si | glassbone, scorched cultist, rune shard, sealed knight |
| `atk_scream` | Grito/Uivo/Hino | DebuffStrike em área (dano 0–baixo) | aplica status (Fear/ConfusionLite/Chill), telegraph sonoro | bat, wailer, echo shade, kobold (alerta) |
| `atk_burrow` | Erupção Subterrânea | BurrowStrike | emerge sob o player; erro = vulnerável 2s | rootsnare (e maggots/matron fora do roster) |
| `atk_blink` | Golpe-Fase | BlinkStrike | teleporte flanqueando + golpe | phasewalker, riftstalker, nyx stalker |
| `atk_summon` | Invocação | SummonAdds | 1–3 adds, posições determinísticas (seed) | lich shard, mirror adept |
| `atk_buff` | Autobuff/Cura | SelfBuff | buff próprio ou heal em aliado | mycobulwark, cold cult acolyte, duergar frostdelver |
| `atk_flurry` | Sequência de Golpes | ComboStrike | 2–4 hits, intervalo 0.15s | shadowblade, void knight, clockwork guard, berserker |

**Regra de reuso de arte:** cada arquétipo define UMA coreografia (poses de windup → strike → recover). A skill de geração de sprites vai gerar a folha por criatura seguindo a coreografia do arquétipo — mesma silhueta de movimento, corpo diferente.

---

## 3. Convenção de sprites de animação (para a futura skill)

- **Movimento:** folha 5×5 seguindo o precedente do `NpcWalkAnimator` (pipeline `npc-walk-animation`), adaptada por plano de corpo: bípede, quadrúpede, voador, serpentino/flutuante, amorfo/enxame, estacionário-ancorado.
- **Ataque:** 1 folha por ataque (normal e especial separados), 5 frames por direção: `windup_a`, `windup_b`, `strike`, `follow`, `recover`. Direções mínimas: down/up/side (side espelhado via flipX — flip automático ainda não existe no código, ver §6).
- **Slug de asset:** `{skin_slug}_atk_{arquétipo}` (ex.: `goblin_atk_slash`, `roost_cave_bat_atk_scream`). Especial único usa o slug próprio da §5 (ex.: `cracked_bone_atk_rearticulate`).
- **Telegraph:** frames de windup devem ler como "vai atacar" mesmo sem cor (silhueta recuada/inflada); o piscar de cor do `EnemyTelegraphController` continua por cima.

---

## 4A. Crosswalk — os 60 IDs do Roster: variância ou nova

**Regra de implementação:** VARIÂNCIA **herda o kit de ataques (2–3 ações) e as folhas de animação da criatura-mãe do catálogo** — o `actionset_` da variância referencia as mesmas `EnemyActionSO` da mãe (autoria e arte 1×, sem duplicação). Os kits escritos na PARTE A (§4) valem como *flavor* PT e podem sobrescrever pontualmente o Especial da mãe quando divergirem — na dúvida, a mãe vence. As **7 NOVAS** mantêm o kit próprio da PARTE A e são candidatas a ganhar ficha no `CanonicalBestiaryCatalog` (follow-up).

**53 VARIÂNCIAS** (`id do Roster` → criatura-mãe; evidência: skin binding / nome / família):

| Roster | Mãe no catálogo | Roster | Mãe no catálogo |
|---|---|---|---|
| abyssal_riftstalker | abyssal_lurker | mossling | mushroom_puffball |
| abyssal_void_reaver | abyssal_gatekeeper | mycobulwark | mycelial_warden |
| blackstone_wyvern | cindershard_wyrm | ninrorin_echo_shade | ninrorin_phantom |
| cave_bat | roost_cave_bat | ninrorin_phasewalker | ninrorin_phantom |
| cave_mite | verdant_mite | ninrorin_void_acolyte | nyx_shade_elemental |
| cinder_spitter | cinder_shade | ninrorin_void_knight | veilkin_voidknight |
| clockwork_guard | construct_sentry | ninrorin_void_sentinel | sealed_observer |
| cold_cult_acolyte | coldcult_preacher | nyx_moth | gloom_moth |
| corrupted_draconic_spawn | draconic_wyrmling | oathless_shade | night_haunt |
| corrupted_lich_shard | undead_lich_acolyte | orc_kaand_ashcaller | orc_drummer |
| cracked_bone | undead_shambler | orc_kaand_berserker | orc_berserker † |
| crystal_leaper | crystal_hound | orc_nyx_stalker | orc_grunt |
| draconic_ashspitter | sulfur_wyrmling | puzzle_golem | gnome_wargolem |
| draconic_elder_kin | draconic_elder | rootsnare | corrupted_vine_horror |
| draconic_void_wyrm | corrupt_pseudowyrm | rune_shard | rune_sentry_mk2 |
| drow_arcane_adept | veilkin_witch | scorched_cultist | cultist_zealot |
| drow_shadow_warden | veilkin_voidknight | sealed_knight | ruin_warden |
| drow_shadowblade | veilkin_skirmisher | shadow_sentinel | silence_warden |
| duergar_frostdelver | gravedelver_warder † | spore_imp | mushroom_puffball |
| duergar_shieldbreaker | gravedelver_warder † | void_spitter | void_tendril_watcher |
| ember_tick | ember_scorpion | void_tick | void_brood_larva |
| frost_wailer | frost_wisp † | glassbone | frostbound_revenant |
| furnace_warden | forge_tyrant_vask | goblin_grashnaar_scavenger | goblin_scrounger |
| gnome_gem_madcap | gnome_tinkerer | goblin_urudakh_trapper | goblin_shredder |
| gnomorin_rune_tinker | gnome_tinkerer | icebound_sentinel | rimelock_colossus |
| kobold_scout | kobold_sentry | lava_bulwark | forge_tyrant_vask |
| mirror_adept | mirror_golem | | |

† **Revisão humana recomendada:** `duergar_*` — o skin binding aponta arte de gnome (`gnome_tinkerer`/`gnome_wargolem`), mas nome+facção são Duergar→Gravedelver; mapeei pela família correta e o binding de arte deve ser corrigido. `orc_kaand_berserker` — skin lista só `orc_grunt`, mas nome e kit (flurry) são de `orc_berserker`. `frost_wailer` — wraith undead mapeado num wisp elemental por falta de ficha de wraith de gelo; alternativa: `frostshard_wisp`.

**7 NOVAS** (kit próprio na §4; candidatas a ficha no catálogo): `ash_crawler` (rastejador de cinzas), `blackroot_sprout` (broto de raiz negra), `corrupted_bone_knight` (cavaleiro ósseo do Void), `hollow_stagling` (cervo oco espectral), `stone_rat` (roedor de pedra), `frost_gnawer` (roedor de gelo — sem ficha de roedor no catálogo), `thorn_archer` (único arqueiro de arco do jogo — sem ficha de archer no catálogo).

---

## 4. Catálogo por criatura — PARTE A: os 60 IDs do Roster (kits *flavor*; variâncias herdam da mãe — §4A)

Legenda **Dist** (vale para todo o documento): M = melee, R = ranged (projétil), C = cone curto, M+ = melee com mobilidade (leap/blink/charge).

### 4.1 Goblinoides & Kobolds & Orcs

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_goblin_grashnaar_scavenger` | `atk_slash` (adaga suja) | **Furto Covarde** — `atk_leap` + após acertar entra em Retreat 2s (foge com o "loot") | M+ | Grashnaar: ladrões, não lutadores; fogem <30% HP. D&D: goblin Nimble Escape |
| `enemy_goblin_urudakh_trapper` | `atk_throw` (dardo) | **Armadilha de Raiz** — DebuffStrike ranged, aplica Root 1.5s | R | Urudakh: especialistas em armadilha, máscaras de osso |
| `enemy_kobold_scout` | `atk_throw` (funda) | **Ganido de Alerta** — `atk_scream` sem dano; alerta o pack inteiro (usa pack alert existente) e ganha +speed 3s | R | Lore: "gritar vale mais que lutar". D&D: kobold sling + Pack Tactics |
| `enemy_orc_kaand_berserker` | `atk_cleave` (machadão) | **Turbilhão de Kaand** — `atk_flurry` 3 machadadas com posture + | M | Orcs de Kaand: frenesi ao tambor. D&D: orc Aggressive + greataxe |
| `enemy_orc_kaand_ashcaller` | `atk_cast` (brasa, fire) | **Chuva de Cinzas** — TelegraphedAoE fire no player, Burn 25% | R | Caster de fogo do clã; eco do Cindershard |
| `enemy_orc_nyx_stalker` | `atk_slash` (cutelo serrilhado) | **Manto de Nyx** — `atk_blink` (some na sombra, reaparece flanqueando) | M+ | Caçador sob a lua oculta de Nyx; D&D: shadow monk/gloom stalker |

### 4.2 Veilkin (Drow) & Gravedelver (Duergar) & Gnomorin

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_drow_shadowblade` | `atk_slash` (lâmina curva) | **Dança das Sombras** — `atk_flurry` 3 hits + Bleed no último | M | Assassino do Véu, CircleStrafe. D&D: drow elite warrior |
| `enemy_drow_arcane_adept` | `atk_cast` (projétil lunar, windup lento e forte) | **Salva Lunar** — 3 projéteis em leque (ComboHits ranged) | R | Caster lunar que teleporta ao ser acertado (PhaseShortBlink já cobre). D&D: drow mage |
| `enemy_drow_shadow_warden` | `atk_cleave` (mace espiculada) | **Encontrão do Véu** — `atk_bash` knockback 3 tiles + Stun 0.5s | M | Guarda pesado do Véu. D&D: war priest / shield bash |
| `enemy_duergar_frostdelver` | `atk_cleave` (picareta pesada) | **Crescer da Pedra** — `atk_buff`: +25% escala visual e +50% dano por 6s | M | D&D: duergar **Enlarge** (icônico!); `VisualScale` já existe no código |
| `enemy_duergar_shieldbreaker` | `atk_slam` (marreta) | **Quebra-Guarda** — DebuffStrike: dano de postura dobrado, quebra Block do player (interage com F27) | M | Anti-escudo; pune quem só segura Block. D&D: duergar hammerer |
| `enemy_gnome_gem_madcap` | `atk_claw` (caco de gema) | **Estilhaço de Gema** — `atk_throw` que fragmenta: AreaPulse 1.5 no impacto, Bleed 20% | R | Lunático das gemas; gnomo maligno de Vaalara |
| `enemy_gnomorin_rune_tinker` | `atk_throw` (engenhoca) | **Runa Detonante** — TelegraphedAoE: planta runa sob o player, detona após 0.8s | R | Tinker cruel de runas corruptas. D&D: rogue tinker traps |
| `enemy_mirror_adept` | `atk_cast` (estilhaço de espelho, arcane) | **Imagem Espelhada** — `atk_summon`: 2 cópias de 1 HP que imitam movimento | R | Salões espelhados do Arquivo. D&D: **Mirror Image** |

### 4.3 Ninrorin (elfos do Vazio)

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_ninrorin_void_sentinel` | `atk_thrust` (lança rift) | **Muralha do Vazio** — `atk_bash` com escudo rift-stone + 2s GuardHold | M | Sentinela de lança e escudo; protege âncoras. D&D: githyanki warrior |
| `enemy_ninrorin_void_knight` | `atk_cleave` (montante-vazio serrilhado) | **Combo do Montante** — `atk_flurry` 2 hits + Corruption garantido | M | Elite duelista, evolução final dos corrompidos |
| `enemy_ninrorin_void_acolyte` | `atk_cast` (bolt do vazio) | **Litania do Vazio** — TelegraphedAoE arcane + Weakness 4s | R | Acólito que canta a litania de Veyraath |
| `enemy_ninrorin_phasewalker` | `atk_slash` (adaga) | **Passo-Fase** — `atk_blink` backstab (dano +50% se atingir pelas costas) | M+ | Phasewalker translúcido mid-blink; BlinkStrike pronto no código |
| `enemy_ninrorin_echo_shade` | `atk_claw` (toque espectral, chill) | **Lamento do Eco** — `atk_scream` Fear 2s em raio 2.5 | M | Eco-memória wraith. D&D: specter Life Drain + wail |

### 4.4 Mortos-vivos & Cultistas

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_cracked_bone` | `atk_slash` (espada enferrujada) | **Rearticular** — 1× por vida: ao "morrer", colapsa e se reergue com 25% HP após 2s (janela para o player destruir os ossos) | M | D&D: skeleton; mecânica de reerguer citada na lore do Frostbound Revenant (§5 gap) |
| `enemy_glassbone` | `atk_claw` (cacos) | **Estilhaçar** — `atk_nova` de cristal, raio 2.0, Bleed 30% | M | Undead de cristal translúcido; frágil e cortante |
| `enemy_frost_wailer` | `atk_claw` (toque gélido, chill leve) | **Grito Gélido** — `atk_scream` Chill/Slow 50% por 3s em raio 3.0 | M | Wraith de boca aberta gritando; lore: slow em área |
| `enemy_hollow_stagling` | `atk_slam` (chifrada) | **Investida Oca** — `atk_charge` em linha + Fear 1.5s (o brilho da cavidade) | M+ | Cervo undead de peito vazio; D&D: charge de cervídeo |
| `enemy_oathless_shade` | `atk_claw` (garra de fumaça) | **Dreno de Juramento** — DebuffStrike + cura o shade em 50% do dano causado | M | Espírito sem juramento drena vida; reusa a matemática Vampiric do elite affix |
| `enemy_corrupted_bone_knight` | `atk_cleave` (espadão negro) | **Talho Corrompido** — TelegraphedAoE em arco frontal amplo + Corruption | M | Cavaleiro undead com veias de Pedra Negra. D&D: death knight lite |
| `enemy_corrupted_lich_shard` | `atk_cast` (bolt sombrio) | **Chamado dos Ossos** — `atk_summon`: 2× `enemy_cracked_bone` | R | D&D: lich **Animate Dead**; SummonAdds pronto (seed determinística) |
| `enemy_sealed_knight` | `atk_slash` (espada cerimonial) | **Julgamento Selado** — `atk_nova` radiante raio 2.2 + Stun 0.8s | M | Guardião nymiriano de Anya; pulso de julgamento (lore do Sealed Observer) |
| `enemy_cold_cult_acolyte` | `atk_cast` (raio de frio) | **Prece do Frio** — `atk_buff`: cura 25% HP do aliado mais ferido em 4 tiles | R | Cultista do frio; D&D: ray of frost + cure wounds. Requer heal-aliado (§5) |
| `enemy_scorched_cultist` | `atk_cast` (firebolt) | **Auto-Imolação** — `atk_nova` fire raio 2.0 + Burn, custa 10% do próprio HP | R | Fanático queimado; a devoção literalmente o consome |
| `enemy_icebound_sentinel` | `atk_slam` (punhos de gelo) | **Prisão de Gelo** — DebuffStrike Root 1.5s (congela o player no chão) | M | Guardião congelado do culto; D&D: ice mephit/water elemental freeze |

### 4.5 Constructos

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_clockwork_guard` | `atk_slash` (lâmina embutida) | **Rotina de Supressão** — `atk_flurry` 3 golpes em ritmo mecânico exato | M | Sentinela bromeciana; a precisão é a assinatura |
| `enemy_puzzle_golem` | `atk_slam` | **Rearranjo** — `atk_buff`: +50% defense 5s e troca a janela de vulnerabilidade | M | Golem-enigma: "resolver" = atacar na janela certa |
| `enemy_rune_shard` | `atk_cast` (feixe rúnico) | **Sobrecarga Rúnica** — `atk_nova` lightning raio 2.5 | R | Fragmento de sentinela de Elyndor; varredura de feixe (lore) |
| `enemy_furnace_warden` | `atk_slam` (braços de ferro, arco amplo) | **Jato de Brasa** — `atk_breath` fire cone 3.0 + Burn | C | Constructo-forja de peito incandescente (lore: expele jato de brasa) |
| `enemy_shadow_sentinel` | `atk_claw` (sombra) | **Feixe-Olho** — `atk_cast` perfurante de longo alcance (range 7) | R | Fragmento do olhar de Veyraath (lore do Void Tendril Watcher) |

### 4.6 Abissais / Vazio

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_abyssal_riftstalker` | `atk_claw` | **Bote do Rift** — `atk_blink` + Bleed | M+ | Predador que caça entre fendas; emboscador |
| `enemy_abyssal_void_reaver` | `atk_slash` (garra-foice) | **Rasgo de Fase** — `atk_charge` (MultiHitCharge) + ConfusionLite 2s | M+ | Lore do Reality Render: bote de rasgo-de-fase que "silencia" |
| `enemy_void_spitter` | `atk_spit` (glóbulo do vazio) | **Salva Corrosiva** — 3 cuspes em leque + DurabilityStress | R | O nome é o contrato; prole de Veyraath desgasta equipamento |
| `enemy_void_tick` | `atk_bite` (mordida corrosiva) | **Sanguessuga do Vazio** — DebuffStrike DurabilityStress + rouba 10 stamina | M | Enxame que rói equipamento (lore Void Brood Larva) |

### 4.7 Dracônicos

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_corrupted_draconic_spawn` | `atk_bite` | **Mini-Sopro Corrompido** — `atk_breath` cone 2.0 toxic | C | Wyrmling quadrúpede de escamas negras; sopro fraco de ninhada |
| `enemy_draconic_ashspitter` | `atk_spit` (brasa) | **Sopro de Cinza** — TelegraphedAoE fire no player + Burn 40% | R | O nome é o contrato; ninhada do Cindershard |
| `enemy_draconic_elder_kin` | `atk_claw` (garra ancestral, forte) | **Sopro Ancestral** — `atk_breath` cone 3.5 elemental, dano alto | C | Parente ancião; D&D: dragonborn elder Breath Weapon |
| `enemy_draconic_void_wyrm` | `atk_bite` | **Feixe de Sopro-Vazio** — `atk_cast` beam range 6 + Corruption | R | Lore do Corrupt Pseudo-Wyrm: feixe de sopro-vazio orbitando |
| `enemy_blackstone_wyvern` | `atk_claw` + mordida | **Ferrão Farpado** — `atk_leap` (rasante) + Poison garantido | M+ | D&D: **wyvern stinger venenoso** — assinatura obrigatória |

### 4.8 Bestas & Vermes

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_cave_bat` | `atk_bite` (rasante) | **Guincho Desorientante** — `atk_scream` ConfusionLite 2s raio 2.0 | M | Colônia que defende o teto; guincho + rasante (lore) |
| `enemy_cave_mite` | `atk_bite` (veneno leve) | **Mordida Séptica** — DebuffStrike Poison garantido 3s | M | Praga de celeiro com irmãos; imune a Bleed |
| `enemy_stone_rat` | `atk_bite` | **Roída Mineral** — DebuffStrike DurabilityStress (rói o equipamento) | M | Roedor que mastiga até pedra |
| `enemy_ash_crawler` | `atk_claw` (pinça) | **Investida em Chamas** — `atk_charge` + Burn 25% | M+ | Skin ember_scorpion; territorial de veio de magma (ChargeLine, lore) |
| `enemy_frost_gnawer` | `atk_bite` (gelo) | **Mastigada Congelante** — DebuffStrike Chill 50% 3s | M | Roedor de pele congelada espinhuda |
| `enemy_crystal_leaper` | `atk_claw` | **Bote Presa-de-Gelo** — `atk_leap` + Chill 40% | M+ | Fera de cristal ágil; bote de gelo (lore) |
| `enemy_ember_tick` | `atk_bite` | **Abdômen Fervente** — `atk_nova` fire raio 1.2 + Burn (pequena, punindo melee colado) | M | Carrapato de abdômen incandescente |
| `enemy_nyx_moth` | `atk_claw` (rasante de asa) | **Pó Lunar** — TelegraphedAoE nuvem 2.0, Slow 40% + ConfusionLite | M | Mariposa de Nyx; pó de asa desorienta |

### 4.9 Plantas & Fungos

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_blackroot_sprout` | `atk_whip` (raiz) | **Raiz Prendedora** — DebuffStrike Root 1.5s | M | Broto de raiz negra; D&D: twig blight + entangle |
| `enemy_rootsnare` | `atk_whip` | **Emboscada de Raiz** — `atk_burrow`: raízes irrompem sob o player + Root | M | O nome é o contrato; BurrowStrike pronto no código |
| `enemy_mossling` | `atk_claw` (tapa de musgo) | **Baforada de Esporos** — `atk_nova` toxic raio 1.2, Poison 25% | M | Criaturinha goblin-like de musgo (criatura viva, não planta) |
| `enemy_spore_imp` | `atk_throw` (esporo) | **Estouro Fúngico** — TelegraphedAoE toxic + Poison 35% | R | Imp fúngico arremessador; jardins azedos de Thandra |
| `enemy_mycobulwark` | `atk_whip` (chicote de raiz) | **Regeneração Micelial** — `atk_buff`: regen 5% HP/s por 4s (lore: perto de esporos) | M | Escudo-cogumelo vivo; tank com regen (lore) |
| `enemy_thorn_archer` | `atk_bow` (flecha de espinho) | **Salva de Espinhos** — 3 flechas rápidas (ComboHits ranged) + Bleed 25% | R | Arqueiro de espinhos; mecanicamente = arco clássico |

### 4.10 Elementais

| enemyId | Normal | Especial | Dist | Lore / D&D |
|---|---|---|---|---|
| `enemy_cinder_spitter` | `atk_spit` (fagulha) | **Cusparada Tripla** — 3 projéteis em leque + Burn 25% | R | Cuspidor de brasas; o nome é o contrato |
| `enemy_lava_bulwark` | `atk_slam` (braços fundidos) | **Trilha de Magma** — TelegraphedAoE que deixa poça de lava 4s no chão (hazard persistente — §5) | M | Grande elemental de rocha negra; trilha de magma e bloqueio de caminho (lore) |

---

## 4B. Catálogo por criatura — PARTE B: Bestiário canônico (117 fichas, fonte de verdade)

Fichas extraídas do `CanonicalBestiaryCatalog` (Role/Move/Notes táticas orientaram cada atribuição — a maioria dos especiais abaixo JÁ tem nome canônico na ficha). Minibosses: Normal + S1/S2. Gate bosses: Normal + especial por fase (F1/F2/F3).

### 4B.1 Band STONE (lvl 1–10)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_verdant_mite` | `atk_bite` | **Frenesi do Enxame** — `atk_buff` +speed quando 3+ vivos | M |
| `enemy_pale_grub` | `atk_bite` (Nip, só se agredida) | **Chiado de Isca** — `atk_scream` sem dano que atrai 1 Cave Leaper (`atk_summon` 1) | M |
| `enemy_spore_crawler` | `atk_slam` | **Estouro Póstumo** — explosão toxic ao morrer (reusa runner Volatile), Poison 10% | M |
| `enemy_goblin_scrounger` | `atk_slash` (adaga) | **Areia no Olho** (Pocket Sand) — DebuffStrike ConfusionLite 1s | M |
| `enemy_kobold_sentry` | `atk_throw` (funda) | **Ganido de Alerta** (Yelp) — `atk_scream` alerta o pack 1×/combate | R |
| `enemy_mushroom_puffball` | `atk_nova` toxic raio 1.5 (Spore Cloud, único golpe) | **Estouro Final** — morte volátil toxic | M |
| `enemy_rot_beetle` | `atk_bite` | **Esmagar de Mandíbula** — DebuffStrike posture +50% | M |
| `enemy_grimfang_packleader` | `atk_bite` (Rend) | **Uivo de Matilha** (Howl) — `atk_buff` aliados +15% vel | M |
| `enemy_lake_lurker` | `atk_bite` (Drag Bite) | **Arrasto Submerso** — DebuffStrike Root 0.5s + puxa 1 tile p/ água | M |
| `enemy_glimmer_centipede` | `atk_bite` | **Pulso Bioluminescente** — `atk_scream` ConfusionLite 0.5s | M |
| `enemy_stone_burrower` | `atk_bite` | **Investida Vertical** — `atk_burrow` tell 0.5s | M+ |
| `enemy_roost_cave_bat` | `atk_bite` (rasante) | **Guincho da Colônia** — `atk_scream` ConfusionLite 0.5s | M |
| `enemy_bandit_scavenger` | `atk_slash` (arma improvisada) | **Chamar Capangas** — `atk_scream` de reforço (RetreatAndCall como ação) | M |
| `enemy_cracked_golem_shard` | `atk_slam` (pancada lenta) | **Pulso de Escombros** — `atk_nova` physical raio 1.5 | M |
| `enemy_burrow_matron` (miniboss) | `atk_slam` | S1 **Erupção** — `atk_burrow` tell 1.0s, vulnerável 2s se erra · S2 **Chamado da Ninhada** — `atk_summon` 3 Pale Grubs a 50% HP | M+ |
| `enemy_scrounger_king` (miniboss) | `atk_slash` | S1 **Rajada de Facas** — `atk_flurry` 3 hits · S2 **Bomba de Fumaça** — TelegraphedAoE ConfusionLite + reposiciona (blink) | M+ |
| `enemy_cave_mite_queen` (boss g10) | `atk_bite` | F1 **Ninhada** `atk_summon` mites + investidas · F2 (66%) **Jato Ácido** `atk_breath` toxic · F3 (33%) **Frenesi** `atk_buff` +30% vel | M/C |

### 4B.2 Band FUNGAL (lvl 11–25)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_burrowing_maggot` | `atk_bite` | **Solapar** (Undermine) — `atk_burrow`; errar = 2s vulnerável | M+ |
| `enemy_goblin_shaman` | `atk_cast` (Fungal Spark) | **Remendar** (Mend) — `atk_buff` cura aliado 15% (delta heal-aliado) | R |
| `enemy_kobold_trapmaster` | `atk_throw` (dardo) | **Armadilha de Laço** — DebuffStrike ranged Root 1.5s (max 2 ativas) | R |
| `enemy_orc_grunt` | `atk_cleave` | **Ceifada em Arco** — TelegraphedAoE frontal 120° | M |
| `enemy_cave_leaper` | `atk_claw` | **Bote** (Pounce) — `atk_leap` 4 tiles; errar = 1.5s vulnerável | M+ |
| `enemy_gravedelver_crossbowman` | `atk_bow` (besta) | **Virote Pesado** — projétil forte, posture +30%, recarga atrás de cobertura | R |
| `enemy_fungal_spreader` | `atk_cast` (esporo) | **Campo Micelial** — zona de slow -25%/4s no chão (delta hazard-zone) | R |
| `enemy_gloom_moth` | `atk_claw` (rasante) | **Asa de Pó** — `atk_scream` ConfusionLite 0.8s | M |
| `enemy_rotcap_cluster` | `atk_nova` toxic raio 1.5 | **Estouro Final** — morte volátil toxic | M |
| `enemy_mycelial_warden` | `atk_whip` (raiz, 2 tiles) | **Regeneração Micelial** — `atk_buff` regen perto de esporos | M |
| `enemy_goblin_shredder` | `atk_slash` | **Investida de Lâmina Dupla** — `atk_flurry` 2 hits | M |
| `enemy_orc_drummer` | `atk_slam` (baqueta) | **Tambor de Guerra** — `atk_buff` aliados +15% dano (delta buff-aliado) | M |
| `enemy_cave_stalker_cat` | `atk_claw` | **Bote da Escuridão** — `atk_leap` + stagger | M+ |
| `enemy_spore_amalgam` | `atk_slam` (engolfar) | **Engolfo** — DebuffStrike Slow | M |
| `enemy_packlord_ruvash` (miniboss) | `atk_cleave` | S1 **Tambor de Ruvash** — `atk_buff` até 4 grunts · S2 **Recuar ao Portão** — retreat + chama reforço | M |
| `enemy_fungal_tyrant_sprout` (miniboss) | `atk_whip` | S1 **Onda de Raízes** — TelegraphedAoE Root 1s (janela 3s depois) · S2 **Florescer de Esporos** — `atk_nova` toxic raio 2 | M |
| `enemy_fungal_patriarch` (boss g20) | `atk_whip` | F1 **Brotos** `atk_summon` · F2 (66%) **Labirinto Micelial** — zonas de slow no chão · F3 (33%) **Florescer Explosivo** — `atk_nova` grande, núcleo exposto 4s/ciclo | M |

### 4B.3 Band ICE (lvl 26–40)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_undead_shambler` | `atk_claw` | **Reerguer** — levanta 1× a 25% HP salvo queimado (delta rise-once) | M |
| `enemy_frost_wisp` | `atk_cast` (Toque Gélido, Chill 1.5s) | **Véu de Frio** — `atk_nova` chill pequena | R |
| `enemy_veilkin_skirmisher` | `atk_slash` | **Passo do Véu** — `atk_blink` 2 tiles ao ser focado | M+ |
| `enemy_mirrorfin_shoal` | `atk_bite` (Frenzy Nibble, Bleed leve) | **Frenesi por Sangue** — `atk_buff` +speed se o alvo sangra | M |
| `enemy_orc_berserker` | `atk_cleave` | **Corrente de Fúria** — `atk_flurry` 3 golpes; <30% HP ignora posture | M |
| `enemy_gravedelver_warder` | `atk_slam` | **Segurar a Linha** — `atk_buff` bloqueio frontal 70% (GuardHold) | M |
| `enemy_cultist_zealot` | `atk_cast` (Blackstone Bolt, Corruption 10%) | **Guarda Sombria** — `atk_buff` escudo 20 HP num aliado (delta buff-aliado) | R |
| `enemy_rime_stalker` | `atk_claw` | **Investida Frostbite** — `atk_leap` só pelo flanco/costas + Chill 25% | M+ |
| `enemy_frostshard_wisp` | `atk_cast` (raio de frio) | **Dreno Congelante** — DebuffStrike Chill + dreno de stamina | R |
| `enemy_crystal_hound` | `atk_bite` (presa-de-gelo, slow 1s) | **Uivo Cristalino** — `atk_buff` do par (PackLeader) | M |
| `enemy_veilkin_iceblade` | `atk_slash` (rapieira) | **Riposte de Geada** — contra-ataque na janela pós-bloqueio (delta riposte) | M |
| `enemy_coldcult_preacher` | `atk_cast` (frio) | **Canto de Husinord** — TelegraphedAoE Chill + chama reforço | R |
| `enemy_frostbound_revenant` | `atk_cleave` (talho congelado) | **Reerguer Gélido** — levanta 1× a 50% HP salvo fire/radiant (delta rise-once) | M |
| `enemy_glacier_tick` | `atk_bite` (agarra) | **Dreno de Frostbite** — DebuffStrike Chill + rouba stamina | M |
| `enemy_glacier_maw` (miniboss) | `atk_bite` | S1 **Investida Avalanche** — `atk_charge` tell 1.2s, derruba pilares-hazard; bater na parede = janela 3s · S2 **Abocanhar** — `atk_bite` reforçada | M+ |
| `enemy_veilkin_witch` (miniboss) | `atk_cast` | S1 **Véu Espelhado** — `atk_summon` 2 cópias 1 HP (delta decoy) · S2 **Hex** — DebuffStrike Weakness -15% dano | R |
| `enemy_rimelock_colossus` (boss g30) | `atk_slam` (punhos) | F1 **Pilares de Gelo** · F2 (66%) **Piso Escorregadio** global + 2 Frost Wisps (`atk_summon`) · F3 (33%) núcleo exposto ao quebrar pilar | M |

### 4B.4 Band FIRE (lvl 41–55)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_earth_elemental_minor` | `atk_slam` (Punho-Rocha, posture ×2) | **Tremor** — `atk_nova` physical raio 1.5 | M |
| `enemy_ember_hound` | `atk_bite` (Burning Bite, Burn 20%) | **Inchar e Estourar** — morte volátil fire | M |
| `enemy_cave_burrower_elite` | `atk_bite` | **Erupção Magmática** — `atk_burrow` + rastro de lava 2s (delta hazard-zone) | M+ |
| `enemy_cinder_shade` | `atk_claw` (cinza) | **Agarrar de Cinzas** — DebuffStrike que puxa 1 tile p/ hazard (delta pull) | M |
| `enemy_veilkin_pyromancer` | `atk_cast` (Lança de Chama, linha) | **Muralha de Fogo** — hazard em linha 3 tiles/4s (delta hazard-zone) | R |
| `enemy_corrupted_vine_horror` | `atk_whip` (Lash 3 tiles) | **Constrição** — DebuffStrike Root 1.5s | M |
| `enemy_abyssal_hound` | `atk_bite` (Bocarra Sombria) | **Bote Sombrio** — `atk_blink` curto quando perde o alvo | M+ |
| `enemy_magma_slug` | `atk_slam` (corpo) | **Trilha de Magma** — hazard tile 8s por onde passa (delta hazard-zone) | M |
| `enemy_ember_scorpion` | `atk_claw` (pinça) | **Investida em Chamas** — `atk_charge` + ferrão Burn 3s | M+ |
| `enemy_sulfur_wyrmling` | `atk_spit` | **Cone Sulfúrico** — `atk_breath` Poison + Burn | R/C |
| `enemy_emberroot_horror` | `atk_whip` (flamejante, 3 tiles) | **Vinha Ardente** — DebuffStrike Root + Burn | M |
| `enemy_veilkin_pyrecaller` | `atk_cast` (bola de fogo) | **Chuva de Brasas** — TelegraphedAoE fire cobrindo a retirada | R |
| `enemy_steam_golem_proto` | `atk_slam` | **Jato de Vapor** — `atk_breath` heat cone | M/C |
| `enemy_ashwing_matriarch` (miniboss) | `atk_claw` | S1 **Garra em Mergulho** — `atk_leap` aéreo telegrafado (janela 3s pós-mergulho) · S2 **Tempestade de Cinza** — TelegraphedAoE cegueira 1s | M+ |
| `enemy_forge_tyrant_vask` (miniboss) | `atk_slam` (Forge Slam) | S1 **Martelo Fundido** — TelegraphedAoE raio 2; errar aplica ArmorCracked NELE · S2 **Investida de Forja** — `atk_charge` | M+ |
| `enemy_cindershard_wyrm` (boss g50) | `atk_claw` + cauda | F1 garras/cauda · F2 (66%) **Sopro de Brasa** `atk_breath` cone + voo curto · F3 (33%) pousa exausta, CoreExposed 4s | M/C |

### 4B.5 Band RUINS (lvl 56–70)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_gnome_tinkerer` | `atk_throw` (engenhoca) | **Montar Torreta** — `atk_summon` torreta estática 30 HP (max 1) | R |
| `enemy_ninrorin_phantom` | `atk_claw` (espectral) | **Lamento** (Wail) — `atk_breath` cone Fear 1s | M/C |
| `enemy_hoardmaw` | `atk_bite` (Snap Bite) | **Agarrão do Baú** — DebuffStrike grab 0.8s (só emboscando quem abre) | M |
| `enemy_construct_sentry` | `atk_slam` | **Feixe Sobrecarregado** — `atk_cast` beam em linha, tell 1.2s | M/R |
| `enemy_night_haunt` | `atk_claw` (Toque de Pavor, Fear 1.2s) | **Apagar as Luzes** — `atk_scream` Fear + extingue tochas próximas (delta ambiente) | M |
| `enemy_ruin_warden` | `atk_slam` (Varredura 180°) | **Pulso da Âncora** — puxa o player 2 tiles (delta pull) | M |
| `enemy_corrupted_orc_champion` | `atk_cleave` (Lâmina de Pedra Negra, Corruption 15%) | **Slam Enraivecido** — TelegraphedAoE; a pedra no peito brilha antes (tell) | M |
| `enemy_rune_sentry_mk2` | `atk_cast` (burst pós-scan 3s) | **Varredura Rúnica** — `atk_cast` beam em linha | R |
| `enemy_mirror_golem` | `atk_slam` | **Reflexo Perfeito** — reflete projéteis + empurrão (delta reflect) | M |
| `enemy_gravedelver_runepriest` | `atk_cast` (raio arcano) | **Barreira Rúnica** — `atk_buff` absorve 1 hit | R |
| `enemy_ninrorin_echo_warrior` | `atk_slash` (lâmina-fantasma) | **Flurry Espectral** — `atk_flurry` que ignora 1 ponto de defesa | M |
| `enemy_chromatic_hoardling` | `atk_bite` | **Bote-Mímico** — `atk_leap` do idle + stagger | M+ |
| `enemy_runic_warbeast` | `atk_slam` (chifres) | **Investida Rúnica** — `atk_charge` 3 tiles + knockback forte | M+ |
| `enemy_gnome_wargolem` (miniboss) | `atk_slam` | S1 **Investida a Vapor** — `atk_charge` · S2 **Salva de Canhão** — 3 projéteis em arco; superaquece a cada 25s = ShockOverloaded 4s | M+/R |
| `enemy_undead_lich_acolyte` (miniboss) | `atk_cast` | S1 **Erguer Ossos** — `atk_summon` 3 Shamblers · S2 **Guarda da Morte** — `atk_buff` nega 1 golpe fatal 1× | R |
| `enemy_gravedelver_artificer_lord` (boss g60) | `atk_slam` (martelo) | F1 **Torretas** `atk_summon` ×2 · F2 (66%) **Exo-Armadura a Vapor** `atk_buff` · F3 (33%) **Sobrecarga** — arena eletrificada por zonas | M |
| `enemy_draconic_guardian` (boss g70) | `atk_thrust` (lança) | F1 lança+escudo (pede GuardBreak) · F2 **Troca de Postura** `atk_buff` · F3 **Chamar Ninhada** — `atk_summon` 2 Wyrmlings | M |

### 4B.6 Band DEEP (lvl 71–85)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_blackstone_thrall` | `atk_claw` (Claw Frenzy) | **Estouro de Corrupção** — morte volátil Corruption perto de outros | M |
| `enemy_draconic_wyrmling` | `atk_bite` (Tail Snap) | **Sopro de Faísca** — `atk_breath` cone curto (janela 2s depois) | M/C |
| `enemy_abyssal_lurker` | `atk_bite` (Maw Crush) | **Agarrar de Tentáculo** — DebuffStrike Root 1s + puxa 2 tiles (delta pull) | M |
| `enemy_deep_angler` | `atk_bite` (devastador) | **Isca Luminosa** — a "luz" imita loot; quem chega leva o bote (`atk_leap` da emboscada; lure via HazardLure) | M+ |
| `enemy_earth_elemental_greater` | `atk_slam` | S1 **Terremoto** (Quake) — TelegraphedAoE arena, tell 1.4s · S2 **Arremesso de Rocha** — `atk_throw` pesado | M/R |
| `enemy_whisper_of_veyraath` | `atk_cast` (Raio do Desfazer, DurabilityStress 20%) | **Litania do Silêncio** — `atk_scream` silencia skills 2s (delta silence) | R |
| `enemy_void_brood_larva` | `atk_bite` (corrosiva, DurabilityStress) | **Frenesi da Prole** — `atk_buff` de enxame | M |
| `enemy_mindbound_thrall` | `atk_slash` (golpe-fantoche) | **Convulsão do Sussurro** — `atk_flurry` 2 hits erráticos | M |
| `enemy_veilkin_voidassassin` | `atk_slash` | **Backstab do Silêncio** — `atk_blink` + Silence 2s | M+ |
| `enemy_gloomspine_lurker` | `atk_whip` (tentáculo do teto) | **Descida e Arrasto** — `atk_burrow` (variação teto) + grab | M+ |
| `enemy_corrupt_pseudowyrm` | `atk_cast` (feixe-vazio, Blackstone Stain) | **Sopro-Vazio Contínuo** — `atk_breath` beam | R |
| `enemy_nyx_shade_elemental` | `atk_claw` (sombra) | **Agarrar de Sombra** — DebuffStrike cegueira 2s (ConfusionLite forte) | M |
| `enemy_orc_warlord` (miniboss) | `atk_cleave` | S1 **Investida do Senhor da Guerra** — `atk_charge` que atravessa colunas (errar = janela 3.5s) · S2 **Executar** — dano dobrado se player <30% HP (delta execute) | M+ |
| `enemy_goblin_warchief` (miniboss) | `atk_slash` | S1 **Salva Comandada** — pack atira em volley · S2 **Desafio de Duelo** — ritual 1v1 (liga à quest `scq_goblin_truce`) | M |
| `enemy_abyssal_gatekeeper` (boss g80) | `atk_whip` (tentáculos) | F1 tentáculos por zona · F2 (66%) **Ciclo de Portais** · F3 (33%) **Devorar a Arena** pelas bordas | M |

### 4B.7 Band VOID (lvl 86–101)

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_void_husk` | `atk_claw` | **Pulso de Corrupção** — `atk_nova` Corruption (a aura passiva 10%/s adjacente é delta à parte) | M |
| `enemy_veilkin_blademaster` | `atk_slash` | S1 **Flurry** — `atk_flurry` 4 golpes · S2 **Aparar Espelhado** — pune o mesmo golpe 3× seguidas (delta riposte) | M |
| `enemy_starfall_remnant` | `atk_slam` (gravitacional) | S1 **Chuva de Estilhaços Meteóricos** — TelegraphedAoE múltiplas áreas · S2 **Poço Gravitacional** — pull contínuo 1.5s (delta pull) | M/R |
| `enemy_silence_warden` | `atk_slam` (Varredura do Julgamento) | **Pulso Selante** — `atk_nova` que silencia 2.5s (delta silence); para de atacar se o player carrega Água Viva | M |
| `enemy_dread_choir` | `atk_cast` | **Verso da Perdição** — TelegraphedAoE Fear + Corruption, cast 2.0s interrompível; trio: membro morto = -33% dano | R |
| `enemy_void_tendril_watcher` | `atk_whip` (tentáculo) | **Feixe-Olho** — `atk_cast` beam do olho central | M/R |
| `enemy_reality_render` | `atk_claw` | **Rasgo de Fase** — `atk_charge` + Silence instantâneo | M+ |
| `enemy_veilkin_voidknight` | `atk_cleave` (montante-vazio) | **Combo do Montante** — `atk_flurry` + Blackstone Stain | M |
| `enemy_sealed_observer` | `atk_bash` (empurrão de aviso; NÃO-agressivo por padrão) | **Pulso de Julgamento** — `atk_nova` condicional (só se agredido/item corrupto) | M |
| `enemy_dread_chorister` | `atk_cast` | **Hino do Medo** — `atk_scream` Fear + oscilação de HUD | R |
| `enemy_heralds_hand` (miniboss) | `atk_slam` | S1 **Agarrão do Arauto** — `atk_charge` + grab que arrasta p/ fora da zona segura (delta pull/drag) · S2 **Investida** — `atk_charge` | M+ |
| `enemy_gravelborn_twins` (miniboss duplo) | `atk_slam` (tank) / `atk_throw` (ranged) | **Vínculo Gêmeo** — regen 1%/s enquanto ambos vivem; matar 1 e demorar >10s = enrage +40% (delta twin-link) | M+R |
| `enemy_void_herald` (boss g90) | `atk_thrust` (lanças do vazio) | F1 lanças + **Husks** `atk_summon` · F2 (66%) **Inversão da Arena** — ConfusionLite zonal · F3 (33%) **Fendas** — hazards de vazio | M |
| `enemy_draconic_elder` (boss g100) | `atk_claw`/cauda | F1 terra (garra/cauda/`atk_breath`) · F2 **Voo** — rasantes `atk_leap` + sopros · F3 pousa exausto (CoreExposed 5s) + **Chamar Ninhada** `atk_summon` | M/C |

### 4B.8 The Four (nível 101 — DORMANTE)

`boss_vel_karaum`, `boss_cindrathel`, `boss_archivist_of_silence`, `boss_ithryndor`: as fichas existem, mas as mecânicas de encontro (feixes/plataformas; espelhar a build do jogador; luta de informação no HUD; luta condicional à escolha final) estão **DORMANTES por design** — fora dos arquétipos deste catálogo, adiadas para a spec de endgame. Este catálogo cobre apenas: eles REUSAM os arquétipos visuais (`atk_slam`, `atk_cast`, `atk_breath`, `atk_summon`) como base de animação.

### 4B.9 Boss legado solto

| enemyId | Normal | Especial | Dist |
|---|---|---|---|
| `enemy_meteor_ooze_king` (boss legado Stone lvl 15) | `atk_slam` (gosma) | S1 **Fissão** — `atk_summon` oozes menores · S2 **Queda Meteórica** — TelegraphedAoE grande | M |

---

## 5. Especiais que exigem mecânica nova (delta sobre o que existe)

O sistema atual cobre ~85% do universo via configuração de `EnemyActionSO`. As primitivas novas, consolidadas das PARTES A e B (cada uma serve várias criaturas):

| Primitiva nova | Serve a (exemplos) | Delta mecânico | Prioridade |
|---|---|---|---|
| **Rise-once** (reerguer 1×) | cracked_bone, undead_shambler, frostbound_revenant | "morrer → colapsar → reerguer 1× com X% HP após 2s, salvo dano do elemento que impede" — flag em `EnemyHealth`/`EnemyBrain` | P2 |
| **Heal/Buff de aliado** | cold_cult_acolyte, goblin_shaman (Mend), grimfang/crystal_hound (uivo), orc_drummer (tambor), cultist_zealot (escudo), lich_acolyte (ward) | `SelfBuff` é parcial e não tem alvo-aliado; adicionar `AllyHeal`/`AllyBuff` com raio | P2 |
| **Hazard-zone persistente** | lava_bulwark, magma_slug, cave_burrower_elite, fungal_spreader (slow), veilkin_pyromancer (muralha), fungal_patriarch F2 | AoE que deixa zona de dano/slow por Xs — `HazardZone` genérico (dano/status + duração) | P2 |
| **Pull/arrasto** | cinder_shade, ruin_warden, abyssal_lurker, starfall_remnant, heralds_hand, lake_lurker | DebuffStrike que desloca o player N tiles na direção do atacante/hazard | P2 |
| **Morte volátil como especial de espécie** | spore_crawler, puffball, rotcap, ember_hound, blackstone_thrall, glassbone (legado) | Reusar `EnemyVolatileExplosionRunner` do elite affix como propriedade da espécie (elemento configurável) | P3 |
| **Decoy summon** | mirror_adept (legado), veilkin_witch | SummonAdds com flag `SummonAsDecoy` (1 HP, skin do dono) | P3 |
| **Alerta de pack como ação** | kobold_scout (legado), kobold_sentry, bandit_scavenger | Ação que dispara o pack alert do `EnemyPackCoordinator` | P3 |
| **Quebra-Guarda** | duergar_shieldbreaker (legado), draconic_guardian F1 | DebuffStrike com multiplicador de posture contra player em Block (F27) | P3 |
| **Silence** | whisper_of_veyraath, silence_warden, voidassassin, reality_render | Status que bloqueia skills ativas do player por Xs (novo StatusEffect type ou proxy) | P3 |
| **Riposte/counter** | veilkin_iceblade, veilkin_blademaster | Janela de contra-ataque disparada por bloqueio/padrão repetido do player | P3 |
| **Reflect de projétil** | mirror_golem | Estado que devolve `EnemyProjectileBehaviour`/projétil do player | P3 |
| **Execute condicional** | orc_warlord | Multiplicador de dano se HP do player < threshold | P3 |
| **Twin-link** | gravelborn_twins | Regen enquanto par vive + enrage por timer após 1ª morte | P4 |
| **Ambiente (apagar tochas / isca de loot)** | night_haunt, deep_angler | Interações com luz/loot falso — adiado; usar proxy (Fear/lure via movement) até spec própria | P4 |

Tudo o mais: **zero código novo** — só autoria de `EnemyActionSO`/`EnemyActionSetSO` via gerador (rule `editor-generation-orchestration`: registrar como `RunStep` em `Inicializar Projeto`, nunca `[MenuItem]` avulso).

---

## 6. Gaps de apresentação (o que a skill de sprites precisa resolver)

1. **Não existe sistema de animação de inimigo** — sprites estáticos + piscar de telegraph. Será necessário um `EnemyAttackAnimator` análogo ao `NpcWalkAnimator` (mesma filosofia: folha fatiada + troca de frame por estado do `EnemyBrain`: Chase → walk sheet; AttackWindup/Resolve → attack sheet).
2. **Não existe flip automático por direção** — decidir na skill: side espelhado via `flipX` (barato) vs. frames dedicados.
3. **Folhas por arquétipo, não por criatura**: a coreografia (§2) é do arquétipo; a skill gera a folha re-skinada por `skin_slug` do `enemy_skin_bindings.json` (variantes de skin compartilham a MESMA folha de ataque re-skinada por variante).
4. **Ordem de produção sugerida** (no universo completo de 178, cobre o máximo de criaturas com o mínimo de arquétipos): `atk_bite` (~32 criaturas) → `atk_cast` (~25) → `atk_claw` (~20) → `atk_slam` (~20) → `atk_slash` (~18) → `atk_cleave` (~12) → `atk_whip` (~9) → demais. Contagem detalhada por arquétipo no doc de implementação (`ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0.md`).

---

## 7. Próximos passos

1. **Doc de implementação** (par deste): `ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0.md` — cada arquétipo com mecânica + descrição visual + tabela ataque→criaturas.
2. **Skill nova** `.claude/skills/enemy-attack-animation/` (via `harness-authoring`): gerar/auditar/wire folhas de movimento + ataque dos monstros seguindo §2–§3 e estes dois docs como fonte de verdade.
3. **Spec de conteúdo** (via `/start-spec`): autoria dos `EnemyActionSO`/`EnemyActionSetSO` do universo (normal+especial; bosses por fase) + as primitivas novas da §5 em ordem de prioridade, com generator registrado em `Inicializar Projeto` e validator em `Validar Projeto`.
4. ~~Crosswalk~~ **FEITO na §4A** (53 variâncias + 7 novas). Follow-ups do crosswalk: corrigir os bindings de arte marcados com †, e criar fichas no `CanonicalBestiaryCatalog` para as 7 novas.
5. **Spec de endgame**: mecânicas DORMANTES dos The Four (§4B.8).

---

*Criado: 2026-07-03 (v1.0 Roster 60; v1.1 mesmo dia: universo completo — 117 canônicas + 60 legadas + 1 boss legado = 178 IDs). Fonte de verdade de design para ataques de inimigos. Mudanças mecânicas canônicas devem virar ADR/game_rule via `decision-rule-extraction` quando implementadas.*
