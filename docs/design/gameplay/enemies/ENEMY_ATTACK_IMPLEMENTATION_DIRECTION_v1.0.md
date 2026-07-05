# ENEMY ATTACK IMPLEMENTATION — DIRECTION v1.0

> **Status:** DIRECTION (guia de implementação — mecânica + visual de TODOS os ataques de inimigo)
> **Data:** 2026-07-03
> **Par de:** `ENEMY_ATTACK_CATALOG_DIRECTION_v1.0.md` (v1.1 — atribuição normal/especial por criatura, universo de 178 IDs). Este doc é **ataque-cêntrico**: cada ataque possível com (a) como implementar na mecânica existente, (b) **descrição visual** de como aparece na tela (base para as folhas de sprite), (c) **quais criaturas o usam**.
> **Sistemas alvo:** `EnemyActionSO`/`EnemyActionSetSO`/`EnemyActionRunner` (13 `EnemyActionType`), `EnemyTelegraphController`, `EnemyProjectileBehaviour` + `RuntimeProjectileFactory`, `StatusEffectDatabaseSO`, `EnemyVolatileExplosionRunner`.

---

## 1. Pipeline de implementação de um ataque

1. **Autoria data-driven:** cada ataque = 1 `EnemyActionSO` (`action_{enemyId sem prefixo}_{melee|ranged|special[_n]}`) com `ActionType`, dano, range, `WindupSeconds`/`RecoverSeconds`, `CooldownSeconds`, `StatusApplicationIds`, `TelegraphProfileId`. Cada criatura = 1 `EnemyActionSetSO` (`actionset_{enemyId}`) com o **kit de 2 ou 3 ações**: Melee + Especial (padrão), ou Melee (fallback) + Ranged + Especial (criaturas com distância justificada). O `SelectBestAction` já escolhe por distância: configure `MinRange` no ranged (ex.: 2.5) para a criatura trocar para o melee de fallback quando o player cola. Bosses: 1 action set por fase (`BossPhaseShift` troca).
   - **Melee de fallback (kits de 3):** por plano de corpo — humanoide caster/atirador → `atk_slash` fraco; besta/dracônico → `atk_bite`; construct → `atk_slam`; planta/fungo → `atk_whip`; espectro/elemental → `atk_claw`. Dano ~60% do ranged, sem status. Reusa a folha de animação do arquétipo — custo de arte zero além das folhas-modelo.
2. **Geração:** generator idempotente registrado como `RunStep` em `CindarsHope/Inicializar Projeto` (rule `editor-generation-orchestration`); validator em `Validar Projeto` (checa: todo inimigo tem action set com ≥1 Normal + ≥1 Especial; ação ranged só em criatura marcada ranged; `StatusApplicationIds` existem no `StatusEffectDatabaseSO`).
3. **Visual (fase 2):** `EnemyAttackAnimator` (novo, análogo ao `NpcWalkAnimator`) troca frames pela state machine do `EnemyBrain`: `Chase/Patrol` → folha de movimento; `AttackWindup` → frames de windup em loop; resolve → `strike/follow`; `AttackRecover` → `recover`. O piscar de cor do `EnemyTelegraphController` continua por cima como reforço de leitura.
4. **Folha de ataque:** 5 frames por direção (`windup_a`, `windup_b`, `strike`, `follow`, `recover`), direções down/up/side (side espelhado via `flipX`). Slug: `{skin_slug}_atk_{arquétipo}`. Variantes de skin re-skinam a MESMA coreografia.

## 2. Convenção de cor/VFX por elemento (telegraph + impacto)

| DamageType/Status | Cor de telegraph | VFX de impacto |
|---|---|---|
| physical | branco | risco/arco branco + poeira |
| fire/Burn | laranja | labareda curta + fagulhas |
| ice/Chill | azul-claro | cristais + névoa |
| toxic/Poison | verde | bolhas/nuvem verde |
| arcane/void/Corruption | roxo | fissura roxa + partículas pretas |
| radiant | dourado | pulso dourado |
| lightning/Shock | amarelo | zigue-zague |
| Fear/ConfusionLite | roxo-escuro pulsante | espiral/olhos |
| Root | verde-musgo | raízes/correntes no chão |
| DurabilityStress | cinza-ferrugem | lascas de metal |

---

## 3. Os ataques compartilhados (22 arquétipos): mecânica, visual, quem usa

Formato de "Usam": ids sem o prefixo `enemy_`; `*` = usa como **Especial**; `(F)` = move de fase de boss. Lista = universo completo (60 legados + 117 canônicos + 1 boss legado).

### `atk_slash` — Golpe de Lâmina
- **Mecânica:** MeleeAttack; windup 0.3–0.5s; range 1.2; dano médio.
- **Visual:** windup — braço da arma recua atrás do ombro, torso gira levemente; strike — arco de lâmina (VFX crescente branco) varre 90° à frente, 1 frame de blur; follow — arma termina do lado oposto; recover — volta à pose neutra. Curto e seco, sem deslocamento.
- **Usam (N):** goblin_grashnaar_scavenger, orc_nyx_stalker, drow_shadowblade, ninrorin_phasewalker, sealed_knight, clockwork_guard, cracked_bone, abyssal_void_reaver, goblin_scrounger, bandit_scavenger, goblin_shredder, veilkin_skirmisher, veilkin_iceblade, ninrorin_echo_warrior, mindbound_thrall, veilkin_voidassassin, goblin_warchief, scrounger_king, veilkin_blademaster.

### `atk_cleave` — Talho Pesado (machado/mace/martelo/montante/picareta)
- **Mecânica:** MeleeAttack; windup 0.7–0.9s; range 1.4; dano alto; posture bonus.
- **Visual:** windup — arma erguida acima da cabeça com os 2 braços, corpo inclina para trás (2 frames, pose "vou rachar"); strike — descida vertical com VFX de arco pesado + poeira no ponto de impacto; follow — arma cravada embaixo 1 frame; recover — puxa a arma de volta. Peso > velocidade.
- **Usam (N):** orc_kaand_berserker, drow_shadow_warden, duergar_frostdelver, corrupted_bone_knight, ninrorin_void_knight, orc_grunt, orc_berserker, frostbound_revenant, corrupted_orc_champion, orc_warlord, veilkin_voidknight, packlord_ruvash.

### `atk_thrust` — Estocada (lança)
- **Mecânica:** MeleeAttack; range 1.8 (alcance estendido); windup 0.5s.
- **Visual:** windup — lança recolhida junto ao quadril, ponta brilha; strike — extensão reta com VFX de linha perfurante; recover — recolhe. A silhueta "alonga" na direção do alvo.
- **Usam (N):** ninrorin_void_sentinel, draconic_guardian, void_herald.

### `atk_bite` — Mordida
- **Mecânica:** MeleeAttack; windup 0.2–0.3s; range 0.9; dano baixo-médio.
- **Visual:** windup — cabeça/corpo recua e abre mandíbula; strike — avanço curto de cabeça (a criatura inteira dá um passo/impulso), fauces fecham com VFX de risco duplo; recover — recuo mastigando. Em enxame, os frames podem ser só 3 (windup/strike/recover).
- **Usam (N):** cave_bat, cave_mite, stone_rat, frost_gnawer, ember_tick, void_tick, corrupted_draconic_spawn, draconic_void_wyrm, verdant_mite, pale_grub, rot_beetle, grimfang_packleader, lake_lurker, glimmer_centipede, stone_burrower, roost_cave_bat, burrowing_maggot, mirrorfin_shoal, crystal_hound, glacier_tick, ember_hound, cave_burrower_elite, abyssal_hound, hoardmaw, chromatic_hoardling, draconic_wyrmling, abyssal_lurker, deep_angler, void_brood_larva, cave_mite_queen, glacier_maw.

### `atk_claw` — Garra/Toque
- **Mecânica:** MeleeAttack; rápido (windup 0.25s); range 1.0; dano baixo.
- **Visual:** windup — pata/mão ergue com dedos abertos; strike — rasgo diagonal com VFX de 2–3 riscos paralelos; recover — pata baixa. Em espectros, os riscos são do elemento (chill/void) e a "pata" é fumaça.
- **Usam (N):** mossling, blackstone_wyvern, ninrorin_echo_shade, oathless_shade, shadow_sentinel, abyssal_riftstalker, gnome_gem_madcap, crystal_leaper, ash_crawler, nyx_moth, draconic_elder_kin, cave_leaper, cave_stalker_cat, rime_stalker, cinder_shade, ninrorin_phantom, night_haunt, blackstone_thrall, nyx_shade_elemental, void_husk, reality_render, undead_shambler, ashwing_matriarch, draconic_elder.

### `atk_slam` — Pancada de Massa
- **Mecânica:** MeleeAttack; windup 1.0s; range 1.3; dano alto; posture ++; knockback opcional.
- **Visual:** windup — os 2 braços/corpo inteiro erguem, sombra do golpe aparece no chão (2 frames); strike — impacto com VFX radial de poeira + rachadura no chão 1 frame; follow — braços plantados; recover — reergue devagar. Para amorfos (oozes/amálgamas), o corpo inteiro infla e desaba.
- **Usam (N):** duergar_shieldbreaker, icebound_sentinel, puzzle_golem, furnace_warden, lava_bulwark, hollow_stagling, spore_crawler, cracked_golem_shard, burrow_matron, orc_drummer, spore_amalgam, gravedelver_warder, earth_elemental_minor, magma_slug, steam_golem_proto, forge_tyrant_vask, construct_sentry, ruin_warden, gnome_wargolem, runic_warbeast, earth_elemental_greater, starfall_remnant, silence_warden, heralds_hand, gravelborn_twins, gravedelver_artificer_lord, rimelock_colossus, meteor_ooze_king.

### `atk_bash` — Encontrão / Escudo
- **Mecânica:** MeleeAttack + knockback forte (2–3 tiles); Stun 0.5s opcional; windup 0.5s.
- **Visual:** windup — ombro/escudo vira para o alvo, corpo abaixa como mola; strike — avanço explosivo de meio tile com VFX de onda de choque frontal; recover — freia com poeira nos pés. A silhueta "quadrada" na frente é a leitura.
- **Usam (S):** drow_shadow_warden*, ninrorin_void_sentinel*; **(N):** sealed_observer.

### `atk_charge` — Investida
- **Mecânica:** MultiHitCharge (atinge tudo na trajetória); telegraph 0.8–1.2s; distância 3–6 tiles.
- **Visual:** windup — criatura "trava" mirando, riscos de poeira atrás das patas, linha de telegraph no chão até o destino; strike — deslocamento em 2 frames com motion blur + rastro; impacto final com poeira. Errar/bater em parede = frame de atordoado (estrelas/cabeça baixa).
- **Usam (S):** hollow_stagling*, abyssal_void_reaver*, ash_crawler*, ember_scorpion*, glacier_maw*(S1), forge_tyrant_vask*(S2), gnome_wargolem*(S1), runic_warbeast*, orc_warlord*(S1), heralds_hand*, reality_render*.

### `atk_leap` — Bote
- **Mecânica:** LeapStrike; salto até 4 tiles + impacto; errar = 1.5–2s vulnerável.
- **Visual:** windup — agachamento profundo, cauda/traseiro treme (2 frames); strike — arco no ar com sombra crescendo no ponto de queda; impacto com poeira radial. O frame aéreo é a pose-assinatura (garras à frente).
- **Usam (S):** goblin_grashnaar_scavenger*, crystal_leaper*, blackstone_wyvern*, cave_leaper*, rime_stalker*, cave_stalker_cat*, ashwing_matriarch*(S1, mergulho aéreo), chromatic_hoardling*, deep_angler*, draconic_elder (F2 rasantes).

### `atk_whip` — Chicote de Raiz/Vinha/Tentáculo
- **Mecânica:** MeleeAttack; range 1.6–2.0; arco frontal; windup 0.5s.
- **Visual:** windup — o apêndice enrola para trás em espiral; strike — chicotada em S com VFX de rastro fino (cor do elemento: musgo/fogo/void); recover — apêndice assenta. O corpo-base quase não se move — só o membro.
- **Usam (N):** blackroot_sprout, rootsnare, mycobulwark, mycelial_warden, fungal_tyrant_sprout, fungal_patriarch, corrupted_vine_horror, emberroot_horror, gloomspine_lurker, void_tendril_watcher, abyssal_gatekeeper.

### `atk_bow` — Arco/Besta
- **Mecânica:** RangedProjectile; range 6–8; projétil dodgável; windup 0.6s (mira).
- **Visual:** windup — arma ergue e puxa (corda/manivela visível 2 frames), laser de mira sutil opcional; strike — disparo com recuo de 1 frame + projétil (flecha/virote com rastro); recover — recarrega (besta: frame extra de manivela).
- **Usam (N):** thorn_archer, gravedelver_crossbowman.

### `atk_throw` — Arremesso (funda/dardo/bomba/pedra)
- **Mecânica:** RangedProjectile; range 4–6; projétil lento e visível.
- **Visual:** windup — braço gira/funda roda acima da cabeça (2 frames circulares); strike — soltura com corpo torcendo; projétil em arco balístico com sombra no chão. Engenhocas piscam vermelho antes de estourar.
- **Usam (N):** goblin_urudakh_trapper, kobold_scout, gnomorin_rune_tinker, spore_imp, kobold_sentry, kobold_trapmaster, gnome_tinkerer, gravelborn_twins (metade ranged); **(S):** gnome_gem_madcap*, earth_elemental_greater*(S2 pedra), gnome_wargolem*(S2 salva de canhão ×3).

### `atk_spit` — Cuspe
- **Mecânica:** RangedProjectile orgânico; range 4–5; status quase sempre (Burn/Poison/Corruption).
- **Visual:** windup — pescoço/tórax infla, brilho do elemento sobe pela garganta (2 frames); strike — cabeça chicoteia à frente, glóbulo com rastro pingando; impacto = respingo do elemento no chão.
- **Usam (N):** void_spitter, cinder_spitter, draconic_ashspitter, sulfur_wyrmling; **(S):** void_spitter* e cinder_spitter* (salvas em leque ×3).

### `atk_cast` — Projétil/Feixe Mágico
- **Mecânica:** CastProjectile (estado CastPrepare); windup 0.8s+; projétil elemental OU beam em linha (tell 1.2s).
- **Visual:** windup — mãos/foco se juntam, orbe do elemento cresce entre elas com partículas sugadas para dentro (2 frames — ESTE é o telegraph de leitura); strike — braços projetam, orbe/feixe sai com rastro; recover — vestes/braços assentam. Beam: linha fina de aviso → feixe grosso 0.5s.
- **Usam (N):** drow_arcane_adept, ninrorin_void_acolyte, corrupted_lich_shard, cold_cult_acolyte, scorched_cultist, rune_shard, orc_kaand_ashcaller, mirror_adept, goblin_shaman, fungal_spreader, frost_wisp, frostshard_wisp, cultist_zealot, coldcult_preacher, veilkin_pyromancer, veilkin_pyrecaller, rune_sentry_mk2, gravedelver_runepriest, undead_lich_acolyte, whisper_of_veyraath, corrupt_pseudowyrm, dread_choir, dread_chorister, veilkin_witch; **(S/beam):** shadow_sentinel*, draconic_void_wyrm*, construct_sentry*, void_tendril_watcher*, rune_sentry_mk2*.

### `atk_breath` — Sopro em Cone
- **Mecânica:** TelegraphedAoE frontal; cone 2.0–3.5; windup 0.8–1.0s; elemental.
- **Visual:** windup — inspiração profunda (tórax infla 2 frames), brasas/gelo/vapor vazando pelas narinas/grelha; strike — cone contínuo 2 frames alternados (textura do elemento), chão do cone marcado; recover — cabeça baixa exausta (é a janela de punição — deixe LEGÍVEL).
- **Usam (S):** furnace_warden*, corrupted_draconic_spawn*, draconic_elder_kin*, sulfur_wyrmling*, steam_golem_proto*, ninrorin_phantom* (Wail: cone de Fear), draconic_wyrmling*, corrupt_pseudowyrm* (beam contínuo), cave_mite_queen (F2 jato ácido), cindershard_wyrm (F2), draconic_elder (F1).

### `atk_nova` — Pulso em Área (ao redor de si)
- **Mecânica:** AreaPulse/TelegraphedAoE centrado no inimigo; raio 1.2–2.5; windup 0.8s.
- **Visual:** windup — corpo comprime e brilha por dentro (glow do elemento crescendo, 2 frames); strike — anel de choque expande do corpo (VFX circular do elemento) + 1 frame de corpo "estourado"; recover — desinfla. Anel no chão durante o windup marca o raio.
- **Usam (N):** mushroom_puffball, rotcap_cluster; **(S):** glassbone*, sealed_knight*, scorched_cultist*, rune_shard*, ember_tick*, mossling*, cracked_golem_shard*, frost_wisp*, earth_elemental_minor*, fungal_tyrant_sprout*(S2), void_husk*, silence_warden*, sealed_observer*, fungal_patriarch (F3).

### `atk_scream` — Grito/Uivo/Hino (debuff em área)
- **Mecânica:** DebuffStrike em área (dano 0–baixo); raio 2–3; aplica Fear/ConfusionLite/Chill; alguns disparam pack alert.
- **Visual:** windup — cabeça inclina para trás, boca/mandíbula abre além do natural (2 frames); strike — ondas concêntricas visíveis (anéis finos) saindo da cabeça + tremor de tela leve; recover — cabeça volta. A cor dos anéis segue o status (§2).
- **Usam (S):** cave_bat*, frost_wailer*, ninrorin_echo_shade*, kobold_scout*, pale_grub*, kobold_sentry*, glimmer_centipede*, roost_cave_bat*, bandit_scavenger*, gloom_moth*, night_haunt*, whisper_of_veyraath* (Litania: silence), dread_chorister*.

### `atk_burrow` — Erupção Subterrânea
- **Mecânica:** BurrowStrike (estado Burrow); emerge sob o player; errar = 2s vulnerável; tell no chão 0.5–1.0s.
- **Visual:** submerso — só um montinho de terra/rachadura deslizando (2 frames alternados); windup — o chão sob o alvo racha e treme (ESTE é o telegraph, sem a criatura); strike — erupção vertical com terra voando, criatura em pose estendida; recover — meio corpo fora, sacudindo (janela de punição). Variante teto (gloomspine): pedrisco caindo → descida.
- **Usam (S):** rootsnare*, stone_burrower*, burrow_matron*(S1), burrowing_maggot*, cave_burrower_elite*, gloomspine_lurker*.

### `atk_blink` — Golpe-Fase
- **Mecânica:** BlinkStrike; teleporte flanqueando + golpe; cooldown 5s; alguns aplicam Silence/bônus backstab.
- **Visual:** windup — corpo desfoca e "dobra" para dentro (2 frames com afterimage); strike — some (partículas de void no lugar) e reaparece atrás do alvo já no frame de golpe; recover — pose de finalização. O afterimage no ponto de origem persiste 2 frames para leitura.
- **Usam (S):** orc_nyx_stalker*, ninrorin_phasewalker*, abyssal_riftstalker*, veilkin_skirmisher*, abyssal_hound*, veilkin_voidassassin*, scrounger_king (S2 fumaça+blink).

### `atk_summon` — Invocação
- **Mecânica:** SummonAdds; 1–3 adds; posições determinísticas (seed run+level+summoner); respeita cap da sala.
- **Visual:** windup — braços erguidos/corpo pulsa, círculos de invocação (cor do elemento) desenham no chão nos pontos de spawn (2 frames); strike — flash nos círculos, adds emergem (de baixo p/ cima em 2 frames); recover — invocador curvado ofegante. Torreta (tinker): martela o chão e a engenhoca desdobra.
- **Usam (S):** corrupted_lich_shard*, mirror_adept*, pale_grub* (atrai 1 leaper), burrow_matron*(S2), gnome_tinkerer*, undead_lich_acolyte*(S1), veilkin_witch*(S1 decoys), cave_mite_queen (F1), fungal_patriarch (F1), rimelock_colossus (F2), gravedelver_artificer_lord (F1), draconic_guardian (F3), void_herald (F1), draconic_elder (F3), meteor_ooze_king*(S1 fissão).

### `atk_buff` — Autobuff / Buff-Cura de Aliado
- **Mecânica:** SelfBuff (delta: variação `AllyHeal`/`AllyBuff` com raio — ver §5 do catálogo).
- **Visual:** windup — pose ritual (tambor/cajado/uivo/corpo brilha); strike — aura sobe do chão no ALVO (espiral de partículas na cor do efeito: verde=cura, vermelho=frenesi, azul=escudo, cinza=defesa) 2 frames; recover — assenta. Buff visível persiste como outline sutil no beneficiado.
- **Usam (S):** mycobulwark*, duergar_frostdelver* (cresce +25%), puzzle_golem*, cold_cult_acolyte* (heal aliado), verdant_mite*, grimfang_packleader* (uivo), goblin_shaman* (Mend), mycelial_warden*, orc_drummer* (tambor), packlord_ruvash*(S1), mirrorfin_shoal*, gravedelver_warder* (GuardHold), cultist_zealot* (escudo), crystal_hound*, void_brood_larva*, gravedelver_runepriest* (barreira), undead_lich_acolyte*(S2 Death Ward), gravedelver_artificer_lord (F2 exo-armadura), draconic_guardian (F2 postura), cave_mite_queen (F3 frenesi).

### `atk_flurry` — Sequência de Golpes
- **Mecânica:** ComboStrike; 2–4 hits com intervalo 0.15s; dano dividido.
- **Visual:** windup — pose de lâmina cruzada (1 frame — mais curto que o slash); strike — 2–3 frames de golpes alternados esquerda/direita com múltiplos arcos de VFX sobrepostos e afterimages dos braços; recover — 1 frame ofegante. É o ataque mais "frenético" da folha — os frames podem sacrificar clareza por velocidade.
- **Usam (S):** drow_shadowblade*, ninrorin_void_knight*, clockwork_guard*, orc_kaand_berserker*, thorn_archer* (salva ranged), drow_arcane_adept* (salva ranged), scrounger_king*(S1), goblin_shredder*, orc_berserker*, ninrorin_echo_warrior*, mindbound_thrall*, veilkin_blademaster*(S1 ×4), veilkin_voidknight*.

### Pseudo-arquétipo `atk_debuff` — Golpe com Status Garantido
- **Mecânica:** DebuffStrike (dano do golpe base + `DebuffStatusId` garantido). NÃO tem folha própria: **reusa a folha do golpe base** da criatura (bite/claw/slash/slam/whip) com overlay de VFX do status (§2) no frame de strike.
- **Usam (S):** goblin_urudakh_trapper* (Root), cave_mite* (Poison), stone_rat* (DurabilityStress), frost_gnawer* (Chill), icebound_sentinel* (Root), duergar_shieldbreaker* (quebra-guarda), oathless_shade* (dreno), void_tick*, goblin_scrounger* (areia), rot_beetle* (posture), lake_lurker* (root+pull), spore_amalgam* (slow), kobold_trapmaster* (Root ranged), frostshard_wisp*, glacier_tick*, emberroot_horror* (Root+Burn), corrupted_vine_horror* (Root), cinder_shade* (pull), hoardmaw* (grab), ruin_warden* (pull), nyx_shade_elemental* (cegueira), abyssal_lurker* (root+pull), veilkin_witch*(S2 Hex), whisper (DurabilityStress no N), mirrorfin (Bleed no N).

---

## 4. Especiais únicos e primitivas novas — visual

| Primitiva | Visual na tela |
|---|---|
| **Rise-once** (cracked_bone, undead_shambler, frostbound_revenant) | "Morte": colapsa em pilha de ossos/corpo caído (2 frames) MAS um brilho frio pulsa na pilha (telegraph de que não acabou); remontagem — peças flutuam de volta em 3 frames, reergue com HP parcial. Se morto pelo elemento que impede (fire/radiant), a pilha queima e não pulsa. |
| **Hazard-zone** (magma/mycel/fire wall) | Poça/faixa no chão com borda animada em 2 frames alternados (lava borbulha, esporos flutuam, chamas lambem); entra com "splash" e some com fade nos últimos 0.5s (aviso de expiração). |
| **Pull/arrasto** (ash grasp, anchor pulse, gravity well, tentacle grab) | Linha visível conecta atacante→player 2 frames (corrente de cinza / feixe / tentáculo); player desliza com poeira nos pés contra a direção do movimento; tremor de tela curto. |
| **Morte volátil** (spore_crawler, puffball, rotcap, ember_hound, blackstone_thrall, glassbone, ember_tick) | Reusa o runner Volatile: corpo INFLA 2 frames + piscar acelerado na cor do elemento → explosão radial (anel + partículas) do raio exato. |
| **Decoy summon** (mirror_adept, veilkin_witch) | Cópias surgem num flash de espelho (frame de vidro quebrando ao contrário); decoys têm outline levemente translúcido (tell justo para observadores atentos) e estilhaçam em cacos ao levar 1 hit. |
| **Silence** (whisper, silence_warden, voidassassin, reality_render) | Glifo roxo de "boca selada" sobe do player + as skill slots da HUD ficam acinzentadas com corrente; anéis do grito (atk_scream) em roxo-profundo. |
| **Riposte/counter** (veilkin_iceblade, veilkin_blademaster) | Pose de guarda com a lâmina na vertical brilhando (1 frame parado — o tell é a IMOBILIDADE); no parry, flash branco em X + contra-golpe usando a folha de atk_slash em velocidade 2×. |
| **Reflect** (mirror_golem) | Superfície espelhada do torso flasheia; projétil refletido troca de cor para a do golem e volta com rastro invertido. |
| **Execute** (orc_warlord) | Se o player está <30% HP, o windup do cleave ganha VFX vermelho-sangue e o strike tem frame de impacto duplo. |
| **Twin-link** (gravelborn_twins) | Fio de luz contínuo conecta os dois enquanto vivos (pulsa a cada regen tick); ao morrer um, o fio chicoteia e o sobrevivente ganha outline vermelho crescente (enrage timer visível). |
| **Alerta de pack** (kobolds, bandit) | Ganido com anéis AMARELOS (diferente dos debuffs) + ícones "!" sobre os aliados alertados. |
| **Aura de Corruption** (void_husk) | Névoa roxa rente ao chão num raio de 1 tile ao redor do husk, constante — o player DENTRO ganha partículas subindo. |
| **Isca luminosa** (deep_angler) | Um "brilho de loot" falso (mesmo sprite de drop, levemente errado — pisca fora de ritmo); ao se aproximar, o brilho apaga e a silhueta do angler acende os dentes primeiro. |

---

## 5. Resumo — contagem por arquétipo (ordem de produção de folhas)

| Arquétipo | Criaturas (N+S) | Arquétipo | Criaturas (N+S) |
|---|---|---|---|
| `atk_bite` | ~31 | `atk_scream` | ~13 |
| `atk_slam` | ~28 | `atk_flurry` | ~13 |
| `atk_cast` | ~28 | `atk_charge` | ~11 |
| `atk_claw` | ~24 | `atk_breath` | ~11 |
| `atk_buff` | ~20 | `atk_whip` | ~11 |
| `atk_slash` | ~19 | `atk_leap` | ~10 |
| `atk_debuff` (overlay, sem folha própria) | ~25 | `atk_throw` | ~10 |
| `atk_nova` | ~16 | `atk_blink` | ~7 |
| `atk_summon` | ~15 | `atk_burrow` | ~6 |
| `atk_cleave` | ~12 | `atk_bash` | ~3 |
| `atk_spit` | ~6 | `atk_thrust` | ~3 |
| `atk_bow` | ~2 | | |

**Leitura de produção:** 6 folhas-modelo (`atk_bite`, `atk_slam`, `atk_cast`, `atk_claw`, `atk_slash`, `atk_buff`) cobrem ~150 usos; `atk_debuff` é overlay grátis. O rabo longo (bow/thrust/bash) fica por último.

---

## 6. Validação (quando implementar)

- Validator em `Validar Projeto`: todo `enemyId` do universo tem `actionset_*` com kit completo — ≥1 Melee + ≥1 Especial, e se tem ação Ranged então tem TAMBÉM o melee de fallback (kit de 3) com `MinRange` configurado no ranged; bosses têm set por fase; `StatusApplicationIds` resolvem; ações ranged só em fichas com Role Ranged/Caster ou justificativa deste doc; `TelegraphProfileId` existe.
- EditMode tests (skill `editmode-test-authoring`): seleção de ação por distância (SelectBestAction) para 2–3 fichas representativas por arquétipo; determinismo de summon positions.
- Testing Quality Gate: mudanças das primitivas novas (§5 do catálogo) exigem testes ou Play Mode scenario documentado.

---

*Criado: 2026-07-03. Par ataque-cêntrico do ENEMY_ATTACK_CATALOG (v1.1). Fonte de verdade para a skill `enemy-attack-animation` (folhas de sprite) e para a spec de autoria dos action sets.*
