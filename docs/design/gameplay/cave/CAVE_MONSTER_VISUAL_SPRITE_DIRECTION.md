# Cindar's Hope — Cave Monster Visual & Sprite Direction

> **Status:** documento canônico complementar de descrição visual para sprites dos monstros da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** descrever visualmente cada família/criatura para futura produção de sprites, animações e variações.  
> **Não é spec implementável.** Specs futuras devem converter estas direções em assets, prompts de sprite, atlas, AnimatorControllers e prefabs.

---

## 0. Regra de uso

Toda spec que criar sprites, prefabs visuais, atlas, animações, VFX ou prompt de arte para monstros deve ler este documento.

Regra principal:

```text
Cada monstro precisa ser reconhecível por silhueta, cor, tamanho, comportamento visual e bioma.
Sprites devem comunicar ameaça e função de gameplay antes do jogador ler o bestiário.
```

---

# PARTE A — Escala visual geral

## 1. Tamanhos

| SizeClass | Sprite visual sugerido | Uso |
|---|---:|---|
| Tiny | 16x16 a 24x24 px | ácaros, wisps, enxames |
| Small | 24x32 a 32x40 px | goblins, kobolds, diabretes, besouros |
| Medium | 32x48 px | humanoides, cultistas, drow, mortos |
| Large | 48x56 a 64x64 px | feras, orcs grandes, constructos |
| Huge | 80x80 a 96x96 px | hulks, lodos, titãs, monstros de sala |
| Boss | 96x96 a 192x160 px | boss gates e level 101 |

Regra:

```text
Pivot bottom-center.
Collider/footbox não acompanha o sprite inteiro.
Silhueta deve funcionar em 1x e em zoom de gameplay.
```

## 2. Leitura por função

```text
Swarm: pequeno, rápido, muitos frames simples.
Tank: corpo largo, base pesada, pouca antecipação rápida.
Caster: foco visual em mãos/olhos/símbolos, animação de cast clara.
Ranged: arma/projétil legível.
Burrower: poeira/chão quebrando antes de emergir.
TreasureTrap: deve parecer objeto interativo antes de revelar boca/dentes.
Boss: silhueta única, fases visuais, sinais fortes de ataques.
```

## 3. Variações por variante

| Variante | Marcador visual |
|---|---|
| Veterano | cicatrizes, armadura extra, cor levemente mais escura |
| Elite | brilho/ornamento, tamanho +10-20%, aura curta |
| Corrompido | veios pretos, olhos escuros/roxos, partículas de Pedra Negra |
| Lunar | tons azul-escuros/prateados, poeira lunar, sombras suaves |
| Bromeciano | peças metálicas, runas, engrenagens, placas antigas |
| Ígneo | brasas, rachaduras quentes, fumaça |
| Gélido | cristais, vapor frio, tons azul-pálidos |
| Raiz-Negra | raízes no corpo, seiva escura, folhas mortas |

---

# PARTE B — Direção visual por criatura

## 4. Níveis 1-10 — Caverna de Pedra

```text
enemy_cave_mite — Ácaro da Fenda
Silhueta: inseto minúsculo, corpo oval, 6 patas rápidas.
Cores: cinza pedra, marrom escuro, brilho branco mínimo nos olhos.
Animações: idle tremendo, corrida rápida, mordida curta, morte esmagada.
Leitura: swarm frágil.

 enemy_stone_rat — Rato de Basalto
Silhueta: rato baixo com dorso de pedra quebrada.
Cores: cinza basalto, focinho escuro, olhos âmbar.
Animações: farejar, corrida, dash bite, knockback.
Leitura: perseguidor simples.

 enemy_cave_bat — Morcego de Fenda
Silhueta: asas largas pequenas, corpo escuro, orelhas grandes.
Cores: roxo escuro/cinza, bordas azuladas.
Animações: flutuar baixo, mergulho, pulso de eco circular.
Leitura: evasivo/fraco, incomoda em grupo.

 enemy_goblin_grashnaar_scavenger — Saqueador Grash'naar
Silhueta: pequeno, curvado, mochila de sucata, faca curta.
Cores: pele verde-acinzentada, trapos marrons, cobre oxidado.
Animações: andar furtivo, arremessar pedra, facada, recuar/chamar.
Leitura: saqueador oportunista.

 enemy_kobold_scout — Batedor Kobold
Silhueta: pequeno reptiliano, focinho longo, lança curta.
Cores: escamas cobre/terra, olhos amarelos, faixas de couro.
Animações: apontar alvo, recuar, jab de lança, tiro de pedra.
Leitura: scout tático.

 enemy_mossling — Musguinho Errante
Silhueta: bolota fúngica com pernas curtas e braços musgo.
Cores: verde musgo, marrom úmido, esporos amarelos.
Animações: cambalear, puff de esporo, slam pequeno.
Leitura: resistente e lento.

 enemy_cracked_bone — Osso Rachado
Silhueta: esqueleto incompleto, postura torta, braço maior que outro.
Cores: osso gasto, rachaduras escuras, brilho frio nos olhos.
Animações: arrastar, estocada, swipe, desmontar ao morrer.
Leitura: morto-vivo direto.

 enemy_blackroot_sprout — Broto Raiz-Negra
Silhueta: planta fixa/semifixa, raiz retorcida como mão.
Cores: preto-marrom, seiva roxa, folhas escuras.
Animações: brotar do chão, disparar agulha, agarrar curto.
Leitura: turret/guardião vegetal.

 enemy_translucent_sludge_cube — Cubo de Lodo Translúcido
Silhueta: cubo arredondado/gelatinoso, objetos visíveis dentro.
Cores: verde translúcido, brilho ácido, moedas/ossos internos.
Animações: ondular, avançar lentamente, engolir, borbulhar.
Leitura: bloqueador lento e perigoso.

 enemy_rust_beetle — Besouro Ferrugem
Silhueta: besouro baixo, antenas longas, mandíbulas pequenas.
Cores: laranja ferrugem, carapaça marrom, pó metálico.
Animações: farejar metal, mordida rápida, sacudir antenas.
Leitura: ameaça a metal/equipamento.

 enemy_chest_biter — Baú-Mordente
Silhueta antes: baú velho comum.
Silhueta revelada: tampa como mandíbula, língua curta, pernas pequenas.
Cores: madeira escura, ferragens enferrujadas, interior vermelho escuro.
Animações: idle objeto, revelar dentes, mordida, salto curto.
Leitura: treasure trap.
```

## 5. Níveis 11-25 — Floresta Subterrânea

```text
enemy_spore_imp — Diabrete de Esporo
Silhueta: pequeno corpo magro, chapéu fúngico, braços finos.
Cores: verde, lilás, amarelo-esporo.
Animações: pular, conjurar nuvem, rir/recuar.

 enemy_rootsnare — Garra-Raiz
Silhueta: raiz grossa em forma de garra saindo do chão.
Cores: marrom escuro, seiva preta, pontas verdes.
Animações: chão tremendo, emergir, agarrar, retrair.

 enemy_hollow_stagling — Cervino Oco
Silhueta: cervo magro com cavidade no peito e chifres tortos.
Cores: couro cinza, chifres pálidos, brilho verde interno.
Animações: pisotear, investida, coice, stagger após errar.

 enemy_goblin_urudakh_trapper — Armeiro Uru'dakh
Silhueta: goblin com armadilhas nas costas e máscara parcial.
Cores: verde escuro, couro preto, metais pequenos.
Animações: colocar armadilha, arremessar, recuar.

 enemy_thorn_archer — Espinhador Sombrio
Silhueta: arqueiro magro com arco de espinhos.
Cores: verde escuro, madeira negra, olhos amarelos.
Animações: puxar arco, tiro, tiro preso/slow.

 enemy_orc_nyx_stalker — Espreitador Orc de Nyx
Silhueta: orc médio, ombros largos, capuz/pele escura.
Cores: pele cinza-esverdeada, marcas azul-escuras, couro preto.
Animações: surgir da sombra, dash lateral, corte amplo.

 enemy_mycobulwark — Baluarte Micélio
Silhueta: fungo grande em forma de escudo vivo.
Cores: branco sujo, verde musgo, placas marrons.
Animações: erguer defesa, slam, aura de esporos.

 enemy_nyx_moth — Mariposa de Nyx
Silhueta: mariposa pequena com asas grandes e olho-padrão.
Cores: azul escuro, prata, roxo profundo.
Animações: flutuar, bater asas, soltar pó lunar.

 enemy_root_owlbear — Urso-Coruja Raiz-Oca
Silhueta: corpo de urso, cabeça de coruja, raízes no dorso.
Cores: marrom, penas cinza, raízes pretas.
Animações: rugido, garra pesada, bicada, investida.

 enemy_basilisk_lizard — Lagarto Basilisco de Musgo
Silhueta: lagarto baixo com crista e olhos grandes.
Cores: verde musgo, cinza pedra, olhos dourados opacos.
Animações: olhar carregado, mordida venenosa, rastejar.

 enemy_panther_distorted — Pantera Distorcida
Silhueta: felino médio com cauda duplicada/sombra deslocada.
Cores: preto, roxo, contorno azulado.
Animações: passo baixo, blink curto, pulo, sombra atrasada.
```

## 6. Níveis 26-40 — Caverna de Gelo

```text
enemy_frost_gnawer — Roedor de Geada
Pequeno roedor com pelos congelados e dentes azuis. Deve parecer frágil, rápido e cortante.

 enemy_duergar_frostdelver — Escavador Duergar do Gelo
Anão subterrâneo robusto, barba congelada, picareta pesada e placas frias.

 enemy_duergar_shieldbreaker — Quebra-Escudo Duergar
Duergar grande com escudo rachado e martelo curto. Silhueta larga e defensiva.

 enemy_icebound_sentinel — Sentinela Enregelado
Constructo de gelo e metal antigo. Núcleo azul no peito, movimentos rígidos.

 enemy_glassbone — Osso de Vidro
Esqueleto cristalino translúcido com pontas quebradas. Deve brilhar levemente.

 enemy_cold_cult_acolyte — Acólito do Frio
Humanoide encapuzado, símbolos congelados, mãos azuladas de magia.

 enemy_crystal_leaper — Saltador Cristalino
Criatura cristalina com pernas longas. Silhueta angular e postura de salto.

 enemy_frost_wailer — Lamento Frio
Morto-vivo flutuante ou arrastado com boca aberta, vapor frio constante.

 enemy_hook_horror_ice — Horror-Gancho de Gelo
Monstro alto com braços-ganchos de gelo. Deve ocupar altura e ameaçar arcos amplos.

 enemy_mind_eater_larva — Larva Devora-Mentes
Criatura pequena, corpo mole, cabeça desproporcional, tentáculos curtos. Deve parecer psíquica e frágil, mas perigosa.
```

## 7. Níveis 41-55 — Caverna de Fogo

```text
enemy_ember_tick — Carrapato de Brasa
Inseto pequeno com abdômen incandescente.

 enemy_ash_crawler — Rastejante de Cinza
Quadrúpede coberto de cinza, deixa rastro escuro no chão.

 enemy_orc_kaand_berserker — Berserker Orc de Kaand
Orc grande, marcas vermelhas de guerra, arma pesada, postura agressiva.

 enemy_orc_kaand_ashcaller — Chamador de Cinzas de Kaand
Orc xamânico/caster, ossos queimados, cinzas flutuando nas mãos.

 enemy_lava_bulwark — Baluarte de Lava
Elemental largo, rocha negra com rachaduras laranja.

 enemy_cinder_spitter — Cuspidor de Cinza
Besta média com garganta inflada, placas de cinza e boca brilhante.

 enemy_scorched_cultist — Cultista Chamuscado
Cultista de pele queimada, manto rasgado, símbolo de fogo.

 enemy_furnace_warden — Guardião da Fornalha
Constructo de forja, braços pesados, peito como forno.

 enemy_fire_basilisk — Basilisco de Brasa
Lagarto grande com crista incandescente e olhos de brasa.

 enemy_stone_bulette — Tubarão de Pedra
Predador enorme com placas dorsais, mandíbula larga e postura de escavação.
```

## 8. Níveis 56-70 — Ruínas Antigas

```text
enemy_rune_shard — Lasca Rúnica
Pequeno fragmento flutuante de pedra/runa com brilho arcano.

 enemy_clockwork_guard — Guarda de Corda
Constructo humanoide de engrenagens, passos mecânicos e arma simples.

 enemy_gnome_gem_madcap — Gnomo de Gema Enlouquecido
Gnomo pequeno, olhos ampliados por gema, cabelo bagunçado, ferramentas.

 enemy_gnomorin_rune_tinker — Gnomorin Runa-Torta
Gnomorin técnico com mochila de peças, runas tortas e luvas grandes.

 enemy_sealed_knight — Cavaleiro Selado
Armadura antiga vazia, elmo fechado, selo brilhando no peito.

 enemy_mirror_adept — Adepto do Espelho
Humanoide com fragmentos refletivos, manto claro/escuro, pose elegante.

 enemy_puzzle_golem — Golem de Enigma
Golem grande com placas móveis, núcleo exposto em ciclos.

 enemy_oathless_shade — Sombra Sem-Juramento
Sombra humanoide com restos de armadura/juramento quebrado.

 enemy_beholder_kin_lesser — Observador Menor da Ruína
Corpo ocular flutuante irregular, olho central rachado, olhos menores de cristal.

 enemy_mimic_armory — Arsenal-Mordente
Parece armário/rack de armas. Revelado: dentes, correntes, armas como membros.

 enemy_brain_jelly — Geleia-Memória
Lodo cerebral translúcido com imagens/fragmentos internos.
```

## 9. Níveis 71-85 — Abismo Sombrio

```text
enemy_drow_shadowblade — Lâmina Sombria Drow
Drow ágil com lâmina curva, capa escura e olhos frios.

 enemy_drow_moon_caster — Conjurador Lunar Drow
Drow caster com foco lunar e manto azul-prateado escuro.

 enemy_drow_web_scout — Batedor de Teia Drow
Drow arqueiro/scout com fios escuros e postura baixa.

 enemy_void_caster — Conjurador do Vazio
Humanoide distorcido, rosto quase apagado, energia escura nas mãos.

 enemy_abyss_wisp — Fagulha do Abismo
Pequena chama escura flutuante, centro azul/preto.

 enemy_moonless_hound — Cão Sem-Lua
Cão magro, pele escura, dentes claros e olhos sem brilho.

 enemy_black_lantern_cultist — Cultista da Lanterna Negra
Cultista com lanterna escura, manto preto e luz inversa.

 enemy_oath_eater — Devorador de Juramento
Aberração grande, boca no peito ou barriga, marcas de votos quebrados.

 enemy_mind_eater_adult — Devora-Mentes Abissal
Cabeça alongada, olhos leitosos, tentáculos curtos e manto orgânico.

 enemy_eye_tyrant_blackmoon — Tirano Ocular da Lua Negra
Grande olho flutuante, olhos menores orbitando, aura lunar escura.
```

## 10. Níveis 86-99 — Núcleo Corrompido

```text
enemy_corrupt_hulk — Massa Corrompida
Gigante de carne mineralizada e Pedra Negra, braços desiguais, corpo instável.

 enemy_ninrorin_broken_oracle — Oráculo Ninrorin Quebrado
Figura antiga com olhos cobertos, tecido ritual, fragmentos proféticos flutuando.

 enemy_corrupted_pseudodragon — Pseudodragão Corrompido
Pequeno dragão deformado, asas rasgadas, escamas negras/roxas.

 enemy_blackstone_wyvern — Wyvern de Pedra Negra
Wyvern grande, placas negras, asas feridas e brilho roxo nas fendas.

 enemy_blackstone_cult_paragon — Paragon da Pedra Negra
Cultista avançado, armadura ritual, cristal negro no peito ou cajado.

 enemy_core_mirror — Reflexo do Núcleo
Humanoide espelhado quebrado, rosto sem identidade, movimentos duplicados.

 enemy_mana_warped_beast — Fera Distorcida por Mana
Fera grande com músculos assimétricos e veios azul/roxo.

 enemy_anya_silent_echo — Eco Silencioso de Anya
Figura luminosa contida/corrompida, mais triste do que agressiva. Não deve parecer monstro comum.

 enemy_beholder_blackstone — Observador Tirano da Pedra Negra
Grande observador ocular com olho central de Pedra Negra, veios escuros e placas cristalinas.

 enemy_umber_hulk_corebreaker — Titã Escavador Quebra-Núcleo
Monstro enorme escavador, mandíbulas largas, braços de escavação e placas escuras.

 enemy_core_devourer_slime — Lodo Devorador de Núcleo
Lodo colossal escuro, objetos e ossos dentro, brilho de núcleo corrompido.
```

---

# PARTE C — Bosses

## 11. Boss gates

```text
boss_blackroot_matriarch
Grande massa de raiz/fungo em forma de rainha fixa. Deve ter núcleo visível que abre nas janelas de vulnerabilidade.

 boss_duergar_frost_captain
Duergar robusto, armadura gelada, escudo/martelo. Fase final com escudo quebrado visível.

 boss_kaand_ember_champion
Orc enorme de Kaand, brasas nas marcas de guerra. Fase final mais queimado e agressivo.

 boss_bromecian_puzzle_colossus
Constructo gigante com placas móveis e núcleo lógico. Visual deve mostrar quando o núcleo está exposto.

 boss_moonless_drow_hierophant
Drow ritualista alto, manto lunar escuro, símbolos orbitando. Fases mudam brilho/posição das sombras.

 boss_blackstone_wyvern
Wyvern com asas e placas negras. Uma asa deve parecer danificada na fase final.

 boss_core_oathbreaker
Humanoide/constructo corrompido gigante, armadura de juramento quebrado e Pedra Negra no núcleo.
```

## 12. Bosses do nível 101

```text
boss_blackstone_warden_prime
Guardião massivo de Pedra Negra, formato de sentinela primária, visual muito estável e opressivo.

 boss_elyndor_oath_construct
Constructo antigo de Elyndor, runas limpas sob corrupção, animações mecânicas e solenes.

 boss_nyx_broken_herald
Arauto de Nyx quebrado, forma humanoide alongada, sombra viva, fragmentos lunares.

 boss_draconic_core_remnant
Remanescente dracônico enorme, menos animal e mais núcleo vivo, placas e energia interna.

 boss_anya_bound_echo
Figura acorrentada/luminosa/corrompida. Não deve parecer inimigo comum. Precisa transmitir proteção, dor e poder contido.
```

---

# PARTE D — Animações mínimas

## 13. Animações mínimas por tipo

| Tipo | Animações mínimas |
|---|---|
| Comum melee | idle, walk, attack, hit, death |
| Ranged | idle, walk, aim/cast, projectile, hit, death |
| Caster | idle, cast windup, cast release, recover, hit, death |
| Burrower | idle, burrow, underground tell, emerge, attack, death |
| TreasureTrap | object idle, reveal, attack, recover, death |
| Elite | idle, walk, attack A, attack B, vulnerable, hit, death |
| Boss | idle, movement, phase transition, attack set, vulnerable/core exposed, hit, death/defeat |

## 14. Telegraph visual

Todo sprite que tiver golpe forte precisa de:

```text
windup frame
sinal de direção ou brilho
execução
recovery/vulnerable frame
```

Isso é obrigatório porque ataques durante janelas de vulnerabilidade causam crítico automático.

---

# PARTE E — Specs futuras derivadas

```text
spec_cave_monster_sprite_prompts_by_family.md
spec_cave_monster_sprite_atlas_requirements.md
spec_cave_monster_animation_sets.md
spec_cave_boss_phase_visuals.md
spec_cave_vulnerability_telegraph_vfx.md
spec_cave_monster_variant_visuals.md
```

---

# PARTE F — Decisões fechadas

```text
Cada monstro precisa ter descrição visual suficiente para produção de sprite.
Sprites devem comunicar função de gameplay.
Bosses precisam ter fases visuais.
Variantes precisam ter marcadores visuais claros.
Treasure traps precisam parecer objetos antes de revelar ameaça.
Eco de Anya e boss final de Anya não devem parecer monstros comuns.
```

---

# PARTE G — Pendências

```text
Transformar cada bloco visual em prompts específicos para geração/produção de sprites.
Definir paleta final por bioma.
Definir número de frames por animação.
Definir atlas por bioma ou por família.
Validar legibilidade em 32x32/32x48/64x64.
Criar spritesheet conventions.
