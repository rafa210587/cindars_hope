# Cindar's Hope — Cave Bestiary Catalog Direction v1.1

> **Status:** documento canônico de catálogo nominal de criaturas da caverna
> **Local:** `docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md`
> **Complementa:** `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
> **Depende de:**
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md` (decisões humanas vinculantes)
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md` (Moves oficiais)
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md` (janelas/vulnerabilidades)
> - `docs/design/gameplay/combat/BALANCE_CURVES_DIRECTION_v1.0.md` (multiplicadores por tipo, XP)
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md` (tom: pastoral acima, horror abaixo)
> **Função:** ser a lista nominal completa e canônica das criaturas do jogo — 60 criaturas de
> banda + 4 chefes finais — com ficha, comportamento, narrativa e drops por criatura.
> **Não é spec implementável.** Specs de dados (roster→EnemyDataSO) devem derivar deste catálogo.

---

## 0. Regra anti-duplicação

Este documento é a fonte da **lista** (quem existe, com que números, onde).
Ele não redefine **modelo**:

```text
Modelo de Move/velocidade: ENEMY_BEHAVIORS_DIRECTION.md + COMBAT_CORE_DIRECTION.md §16.
Modelo de vulnerabilidade/janela: CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md.
Multiplicadores por tipo e fórmula de XP: BALANCE_CURVES_DIRECTION_v1.0.md.
Se houver divergência de MODELO, a direction-mãe vence.
Se houver divergência de LISTA/NÚMERO de criatura, ESTE catálogo vence.
```

---

# PARTE A — Como ler este catálogo

## 1. A ficha de criatura

Cada criatura tem uma ficha neste formato:

```text
Nome — enemy_id
Tabela: Lvl (faixa de spawn) | Família | Size | Role | Move oficial + velocidade | HP | DMG | DEF | XP
Narração: quem é essa criatura no mundo (2-3 linhas, tom Vaalara).
Ataques: ações com windup e efeito.
Defesas: fraquezas, resistências, imunidades (sempre com pelo menos 1 counterplay declarado).
Comportamento: 2-3 regras de IA observáveis pelo jogador.
Drops: itens-chave e ligações com o catálogo de itens.
```

Os valores listados valem para o **nível mínimo da faixa**. Escala dentro da faixa:

```text
HP: +12% por nível acima do mínimo.
DMG: +8% por nível acima do mínimo.
Os multiplicadores por tipo (tank, swarm, caster, elite, miniboss, boss) do
BALANCE_CURVES já estão APLICADOS nos números das fichas.
```

## 2. Regras transversais do bestiário

```text
1. Tamanhos naturais: as criaturas usam tamanhos naturais de fantasia clássica —
   Tiny 0.5×0.5, Small 0.75×0.75, Medium 1×1, Large 2×2, Huge 3×3, Gargantuan 4×4 (em tiles).
   Miniboss recebe ×1.5 de escala VISUAL; boss recebe ×2.5. Colliders seguem a size class
   do SPEC 13, nunca o visual.
2. Noturnas: criaturas marcadas [NOTURNA] só spawnam entre 20h-06h, OU a qualquer hora
   durante o pico da lua de Nyx. Revisitar o nível na mesma run preserva o que foi gerado
   (stable run vence o relógio: o spawn é decidido na geração do nível).
3. Aquáticas: criaturas marcadas [AQUÁTICA] só existem em níveis com lago subterrâneo.
   A geração deve garantir ao menos 1 lago a cada 5 níveis nas bandas que as contêm.
4. Não-agressivas: criaturas marcadas [NÃO-AGRESSIVA] não atacam até serem atacadas e
   podem oferecer quests secretas (QUEST_CATALOG §5). NUNCA contam para contratos de caça.
5. Bestiário/spoiler: toda criatura tem BestiaryEntryId = enemy_id. SpoilerTier: comuns 0-1,
   minibosses 2, bosses de gate 3, os Quatro do 101 = 4 (regras do BESTIARY_KNOWLEDGE).
6. Packs: composição de pack continua regida pelo CAVE_MONSTER_ROSTER e pelos
   EnemySpawnPackSO; este catálogo indica afinidades de pack na ficha quando relevante.
7. Cada gate de 10 níveis tem 1 boss dedicado; cada banda tem 2 minibosses errantes.
   Gates podem ter encontros duplos (decisão humana: ex. Gravelborn Twins).
```

## 3. Política de nomes

Decisão humana aplicada:

```text
Nomes de exibição em INGLÊS (UI pode localizar depois).
Renomeados APENAS os nomes que são identidade de produto de terceiros:
  Drow      → Veilkin     (elfos do Véu — ressoa com os véus de Luandil)
  Duergar   → Gravedelver (anões-fundos exilados de Khaz Baruk)
Goblin, kobold, orc, lich, elemental, construct etc. são patrimônio comum da fantasia e
PERMANECEM. As descrições são originais; nenhum texto de terceiros é copiado.
```

---

# PARTE B — Banda 1-10 · STONE (Pedra)

## 4. O bioma

Os primeiros dez níveis são a caverna que os fazendeiros conhecem de ouvir falar: pedra
parda, raízes que descem do teto, o eco do gotejar. É aqui que a vila "perde ferramentas e
acha coragem". O horror ainda não mostrou o rosto — mas as larvas pálidas engordam no
escuro, e quem alimenta uma larva está alimentando outra coisa mais abaixo. O perigo desta
banda ensina o vocabulário do jogo: ler telegraph, respeitar pack, voltar para casa antes
do colapso.

## 5. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Verdant Mite | enemy_verdant_mite | 1-4 | Insect | Tiny | Swarm | 8/1/0 | 4 |
| Pale Grub | enemy_pale_grub | 1-4 | Insect | Small | Fodder | 10/1/0 | 3 |
| Spore Crawler | enemy_spore_crawler | 1-5 | Plant | Small | Chaser | 14/2/0 | 6 |
| Goblin Scrounger | enemy_goblin_scrounger | 2-6 | Humanoid | Small | Skirmisher | 16/3/1 | 8 |
| Kobold Sentry | enemy_kobold_sentry | 2-6 | Humanoid | Small | Ranged | 12/3/0 | 8 |
| Mushroom Puffball | enemy_mushroom_puffball | 2-6 | Plant | Small | Hazard | 12/0/0 | 6 |
| Rot Beetle | enemy_rot_beetle | 3-7 | Insect | Small | Tank-lite | 24/2/2 | 9 |
| Grimfang Packleader | enemy_grimfang_packleader | 4-8 | Beast | Medium | PackLeader | 30/4/1 | 18 |
| Lake Lurker | enemy_lake_lurker | 5-9 | Beast | Medium | Ambusher | 26/5/1 | 16 |
| Burrow Matron ◆ | enemy_burrow_matron | 8-10 | Insect | Large | Miniboss | 90/6/2 | 60 |
| Old Scrounger King ◆ | enemy_scrounger_king | 8-10 | Humanoid | Small | Miniboss | 70/5/2 | 55 |
| Cave Mite Queen ★ | enemy_cave_mite_queen | 10 | Insect | Huge | Boss | 220/7/2 | 150 |

◆ = miniboss · ★ = boss de gate

## 6. Fichas

### Verdant Mite — `enemy_verdant_mite`
**Move:** SwarmErratic 3.6 t/s
**Narração:** um ácaro do tamanho de uma mão, de carapaça coberta de musgo — a mesma praga
que rói os celeiros da vila, só que aqui ela tem irmãos. Os fazendeiros juram que eles
escutam o farfalhar das sacas de trigo.
**Ataques:** Bite (windup 0.3s, dano de contato).
**Defesas:** fraco contra Fire e ataques de área; imune a Bleed (não tem sangue que preste).
**Comportamento:** enxames de 4-8; recua quando isolado; ignora o jogador parado por mais de 3s.
**Drops:** chitin, fiber.

### Pale Grub — `enemy_pale_grub`
**Move:** GroundPatrol 1.6 t/s
**Narração:** larva pálida e cega que engorda no escuro. Inofensiva — e é exatamente isso
que deveria preocupar: algo lá embaixo planta iscas vivas.
**Ataques:** Nip (0.3s) — só se pisada ou atacada.
**Defesas:** fraca contra tudo.
**Comportamento:** quase cega; 3 ou mais juntas atraem Cave Leapers (isca natural).
**Drops:** grub meat (futura comida de pet), slime.

### Spore Crawler — `enemy_spore_crawler`
**Move:** GroundChase 2.8 t/s
**Narração:** um broto que escapou dos jardins de Thandra e azedou longe do sol. Anda como
planta não deveria andar; morre como praga deveria morrer — espalhando-se.
**Ataques:** Slam (0.5s); Spore Puff ao morrer (telegraph verde; Poison 10%, raio 1 tile).
**Defesas:** fraco contra Fire e Axe; imune a Poison.
**Comportamento:** persegue em linha reta; a explosão póstuma pune o melee guloso.
**Drops:** spores, fiber.

### Goblin Scrounger — `enemy_goblin_scrounger`
**Move:** PackFlanker 3.6 t/s
**Narração:** catador grimento das bordas de Dornécia, mais ladrão que lutador. Os goblins
não são todos inimigos — este só ainda não aprendeu boas maneiras (a vila conhecerá um que
aprendeu).
**Ataques:** Shiv (0.4s); Pocket Sand (0.6s, ConfusionLite 1s, cooldown 8s).
**Defesas:** levemente fraco contra tudo — covarde por design.
**Comportamento:** flanqueia em dupla; foge com <30% HP CARREGANDO loot do chão (dropa se
morto na fuga); rouba 1 item caído se ninguém olhar.
**Drops:** gold pouch, copper ore.

### Kobold Sentry — `enemy_kobold_sentry`
**Move:** KiteRanged 2.8 t/s
**Narração:** vigia escamoso que aprendeu cedo que gritar vale mais que lutar. O primeiro
grito dele já custou muita run descuidada.
**Ataques:** Sling Stone (0.6s, projétil 5.5 t/s); Yelp (alerta o pack inteiro, 1× por combate).
**Defesas:** fraco contra burst melee (morre se alcançado).
**Comportamento:** mantém 4-5 tiles de distância; SEMPRE alerta o pack ao avistar; mira o alvo mais próximo.
**Drops:** stones, sling fiber.

### Mushroom Puffball — `enemy_mushroom_puffball`
**Move:** GuardStationary 0 t/s
**Narração:** um cogumelo-bexiga plantado convenientemente perto de algo que você quer.
A caverna tem senso de humor; ele é a piada de entrada.
**Ataques:** Spore Cloud (telegraph de 1.0s — ele "respira" fundo; raio 1.5, Poison 25% por 3s, cd 6s).
**Defesas:** fraco contra Fire (explode sem soltar a nuvem); qualquer ranged o trivializa.
**Comportamento:** estacionário; o posicionamento É o monstro.
**Drops:** spores, glowcap.

### Rot Beetle — `enemy_rot_beetle`
**Move:** TankSlowPush 1.8 t/s
**Narração:** besouro-carniceiro de carapaça dura como telha de Brigandini. O primeiro
"muro com pernas" que o jogador encontra — e a primeira aula de flanqueio.
**Ataques:** Mandible Crush (0.7s, posture +50%).
**Defesas:** resiste Physical 25%; fraco contra Hammer e Toxic.
**Comportamento:** avança em linha; ignora knockback leve; vira-se devagar (flanqueável).
**Drops:** chitin plate, rot gland.

### Grimfang Packleader — `enemy_grimfang_packleader` *(novo)*
**Move:** PackLeader 3.2 t/s
**Narração:** lobo-cinza das trilhas de Cindabar que desceu atrás de presa fácil e virou
chefe do que encontrou. Onde ele uiva, o resto corre junto.
**Ataques:** Rend (0.5s); Howl (0.9s — buffa o pack com +15% de velocidade por 5s, cd 12s).
**Defesas:** fraco contra Fire.
**Comportamento:** lidera 3-5 bestas; 50% do pack debanda se ele morre primeiro; prioriza
quem feriu um aliado dele.
**Drops:** fang, hide, tabela loot_beast.

### Lake Lurker — `enemy_lake_lurker` *(novo)* [AQUÁTICA]
**Move:** TreasureIdleAmbush (submerso) → GroundChase 3.0 t/s
**Narração:** a razão de os mineiros experientes não encherem o cantil nos lagos baixos.
A superfície ondula um segundo antes — quem conhece, conhece.
**Ataques:** Drag Bite (0.6s; +Root 0.5s se o jogador está na margem).
**Defesas:** fraco contra Fire fora d'água; na água resiste Physical 25%.
**Comportamento:** invisível no lago até a aproximação (tell: ondulação); não persegue além
de 4 tiles da água.
**Drops:** fish_pale, slick hide.

### ◆ Burrow Matron — `enemy_burrow_matron` *(novo, miniboss)*
**Move:** BurrowAmbush 2.2 t/s
**Narração:** a matriarca das larvas. O chão ao redor dela é mentira: cede, racha e pare
filhotes. Matá-la é a primeira vez que a caverna "responde".
**Ataques:** Eruption (telegraph de 1.0s no chão); Brood Call (invoca 3 Pale Grubs, 1× ao chegar a 50% HP).
**Defesas:** resiste Physical 25%; fraca contra Toxic e charged attacks.
**Comportamento:** alterna submersa↔superfície; fica vulnerável 2s após cada erupção.
**Drops:** roll extra na tabela + chitin plate ×3.

### ◆ Old Scrounger King — `enemy_scrounger_king` *(novo, miniboss)* [NÃO-AGRESSIVA]
**Move:** CircleStrafe 3.8 t/s
**Narração:** o goblin mais velho da caverna — esperto demais para morrer, ganancioso
demais para subir. Senta num trono de tralha e negocia. Traí-lo é escolha; sobreviver à
escolha é outra conversa.
**Ataques (se traído):** Knife Flurry (3 hits); Smoke Bomb (blink de 3 tiles).
**Defesas:** ágil; fraco contra AoE.
**Comportamento:** oferece a quest secreta `scq_scrounger_bargain` (QUEST_CATALOG §5);
vira miniboss apenas se atacado.
**Drops:** gold ×3, 1 anel aleatório do catálogo de acessórios.

### ★ Cave Mite Queen — `enemy_cave_mite_queen` (boss do gate 10)
**Move:** BossArenaControl
**Narração:** quando os ácaros somem dos níveis de cima, é porque a Rainha está com fome.
O primeiro boss é uma mãe — o jogo inteiro vai rimar com isso mais tarde.
**Fases (modelo F05):** F1 spawn de mites + investidas lentas; F2 (66%) Acid Spray em cone;
F3 (33%) frenesi (+30% velocidade).
**Defesas:** fraca contra Fire; imune a ConfusionLite. **Janela:** 2.5s após cada Acid Spray.
**Drops (first-kill):** receita de cozinha + essência da banda garantida.

---

# PARTE C — Banda 11-25 · FUNGAL

## 7. O bioma

A pedra cede lugar ao macio. Cogumelos do tamanho de carroças filtram a luz dos veios em
verde-doente, e o ar tem gosto de pão esquecido. Aqui a caverna começa a se organizar:
goblins têm xamãs, kobolds têm armadilhas, orcs têm tambores. É a banda onde o jogador
descobre que packs têm *intenção* — e onde a primeira semente de Shadowroot pode ser
plantada no solo úmido.

## 8. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Burrowing Maggot | enemy_burrowing_maggot | 11-16 | Insect | Medium | Burrower | 36/6/1 | 18 |
| Goblin Shaman | enemy_goblin_shaman | 11-16 | Humanoid | Small | Caster | 28/6/1 | 20 |
| Kobold Trapmaster | enemy_kobold_trapmaster | 11-17 | Humanoid | Small | Trapper | 24/5/1 | 20 |
| Orc Grunt | enemy_orc_grunt | 12-18 | Humanoid | Medium | Bruiser | 44/8/2 | 24 |
| Cave Leaper | enemy_cave_leaper | 12-18 | Beast | Medium | Leaper | 30/7/1 | 22 |
| Gravedelver Crossbowman | enemy_gravedelver_crossbowman | 13-19 | Humanoid | Medium | Ranged | 34/9/3 | 26 |
| Fungal Spreader | enemy_fungal_spreader | 14-20 | Plant | Medium | Controller | 40/5/1 | 24 |
| Gloom Moth [NOTURNA] | enemy_gloom_moth | 15-22 | Insect | Small | Swarm | 18/4/0 | 14 |
| Packlord Ruvash ◆ | enemy_packlord_ruvash | 20-25 | Humanoid | Medium | Miniboss | 130/10/3 | 85 |
| Fungal Tyrant Sprout ◆ | enemy_fungal_tyrant_sprout | 22-25 | Plant | Large | Miniboss | 160/9/2 | 90 |
| Fungal Patriarch ★ | enemy_fungal_patriarch | 20 | Plant | Huge | Boss | 420/11/3 | 280 |

## 9. Fichas

### Burrowing Maggot — `enemy_burrowing_maggot`
**Move:** BurrowAmbush 2.0 t/s
**Narração:** prima crescida da larva pálida, com mandíbulas que já entenderam para que servem.
**Ataques:** Undermine (erupção sob o alvo, telegraph 0.9s).
**Defesas:** fraca enquanto emersa.
**Comportamento:** só emerge sob o jogador; erra = 2s de vulnerabilidade.
**Drops:** slime, grub meat.

### Goblin Shaman — `enemy_goblin_shaman`
**Move:** CasterKeepAway 2.4 t/s
**Narração:** o primeiro sinal de que os goblins da caverna têm cultura própria: bastões de
glowcap, máscaras de casca, e uma fé barulhenta em algo que mora mais fundo.
**Ataques:** Fungal Spark (cast 0.8s, projétil 5 t/s); Mend (cura aliado em 15%, cd 10s).
**Defesas:** frágil; fraco contra interrupt e dash.
**Comportamento:** fica atrás do pack; cura o mais ferido; foge se for o último de pé.
**Drops:** spark dust, glowcap.

### Kobold Trapmaster — `enemy_kobold_trapmaster`
**Move:** RetreatAndCall 3.4 t/s
**Narração:** kobold que trocou a funda por engenhoca. Cada recuo dele é um convite com
dentes de ferro escondidos no caminho.
**Ataques:** Dart (projétil); Snare Trap (planta armadilha de Root 1.5s; máx 2 ativas; tell de plantio).
**Defesas:** frágil em combate direto.
**Comportamento:** recua plantando; chama reforços 1×.
**Drops:** trap parts, darts.

### Orc Grunt — `enemy_orc_grunt`
**Move:** GroundChase 3.0 t/s
**Narração:** os tambores de guerra de Vaalara ecoam até aqui embaixo. O grunt é a batida
mais simples: avançar, golpear, repetir até alguém cair.
**Ataques:** Cleave (0.6s, arco 120°); Shove (knockback 1 tile, cd 8s).
**Defesas:** fraco contra Ice e slow.
**Comportamento:** agressão direta; +15% de dano por 5s quando um aliado morre (fúria).
**Drops:** orc tusk, iron scrap.

### Cave Leaper — `enemy_cave_leaper`
**Move:** Leaper (burst)
**Narração:** músculo e fome em forma de gato sem pelo. Caça do teto, das paredes e da sua
distração.
**Ataques:** Pounce (windup 0.7s, salto de 4 tiles; errar = 1.5s de recovery vulnerável).
**Defesas:** fraco contra ranged enquanto salta.
**Comportamento:** salta de superfícies; SEMPRE reposiciona após cada bote.
**Drops:** sinew, hide.

### Gravedelver Crossbowman — `enemy_gravedelver_crossbowman` *(renomeado)*
**Move:** KiteRanged 2.6 t/s
**Narração:** anões-fundos exilados de Khaz Baruk que venderam a mira a quem pagasse — hoje,
ao culto. A disciplina deles sobreviveu à honra.
**Ataques:** Heavy Bolt (recarga 1.0s, projétil 7 t/s, posture +30%).
**Defesas:** bem blindado (DEF 3); fraco contra Lightning e flanqueio.
**Comportamento:** linha de tiro disciplinada; recarrega atrás de cobertura.
**Drops:** bolts, iron ore, ale.

### Fungal Spreader — `enemy_fungal_spreader`
**Move:** FloatingSlow 2.0 t/s
**Narração:** um jardineiro do avesso: onde flutua, o chão deixa de ser seu aliado.
**Ataques:** Spore Lash (0.6s); Mycel Field (zona de slow -25% por 4s, cd 9s, telegraph circular).
**Defesas:** fraco contra Fire; imune a Poison.
**Comportamento:** zoneia rotas de fuga; evita fogo ativamente.
**Drops:** spores ×2, mycel thread.

### Gloom Moth — `enemy_gloom_moth` *(novo)* [NOTURNA]
**Move:** SwarmErratic 4.4 t/s
**Narração:** mariposas da Lua Oculta. Dizem que o pó das asas apaga lembranças curtas —
os mineiros usam isso de desculpa para muita coisa.
**Ataques:** Dust Wing (contato; ConfusionLite 0.8s, 15%).
**Defesas:** fracas contra Fire e Light.
**Comportamento:** orbitam tochas e a luz do jogador; enxame de 5-9.
**Drops:** moth dust (reagente da Ozzra).

### ◆ Packlord Ruvash — `enemy_packlord_ruvash` *(novo, miniboss)*
**Move:** PackLeader 3.4 t/s
**Narração:** o orc que unificou três bandos com um tambor e duas execuções. Ruvash não
luta batalhas — administra cercos.
**Ataques:** Twin Cleave; War Drum (buffa até 4 grunts).
**Defesas:** sólido; covarde estratégico.
**Comportamento:** nunca luta sozinho; se o pack cai, recua para o gate.
**Drops:** orc warbanner (item da quest do Hund), tabela de elite.

### ◆ Fungal Tyrant Sprout — `enemy_fungal_tyrant_sprout` *(novo, miniboss)*
**Move:** ProtectAnchor
**Narração:** filho menor do Patriarca, ensaiando o trono num anel de cogumelos. A ambição
é hereditária até entre fungos.
**Ataques:** Root Wave (linha, Root 1s); Spore Bloom (raio 2, Poison).
**Defesas:** fraco contra Fire. **Janela:** 3s após cada Root Wave.
**Drops:** mycel heart, essence_toxic.

### ★ Fungal Patriarch — `enemy_fungal_patriarch` (boss do gate 20)
**Move:** BossArenaControl + ProtectAnchor
**Narração:** o coração velho da banda. O Patriarca não odeia o jogador — apenas o digere
com paciência, como digeriu todos os outros.
**Fases:** F1 chicotadas + spawns; F2 (66%) Mycel Maze (corredores de slow na arena);
F3 (33%) Burst Bloom contínuo, com núcleo exposto 4s por ciclo.
**Defesas:** fraco contra Fire (queimar o anel abre CoreExposed).
**Drops (first-kill):** receita rara + essence_toxic ×3.

---

# PARTE D — Banda 26-40 · ICE (Gelo)

## 10. O bioma

O frio aqui não é clima: é vontade. Paredes de gelo azul guardam coisas paradas no meio de
um gesto, e a respiração do jogador vira neblina antes de virar medo. Husinord conhece esse
frio lá em cima; cá embaixo, ele tem dentes. É a banda do primeiro inverno mecânico —
Chill, stamina punida, e inimigos que usam o terreno melhor que você.

## 11. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Undead Shambler | enemy_undead_shambler | 26-33 | Undead | Medium | Fodder-tank | 60/10/2 | 30 |
| Frost Wisp | enemy_frost_wisp | 26-33 | Elemental | Small | FloatingSlow | 30/9/0 | 28 |
| Veilkin Skirmisher | enemy_veilkin_skirmisher | 26-32 | Humanoid | Medium | Skirmisher | 52/12/3 | 36 |
| Mirrorfin Shoal [AQUÁTICA] | enemy_mirrorfin_shoal | 27-34 | Beast | Tiny ×6 | Swarm | 12/3/0 (cada) | 4/peixe |
| Orc Berserker | enemy_orc_berserker | 27-34 | Humanoid | Medium | Berserker | 70/15/1 | 40 |
| Gravedelver Warder | enemy_gravedelver_warder | 28-35 | Humanoid | Medium | Guard | 80/11/5 | 42 |
| Cultist Zealot | enemy_cultist_zealot | 29-36 | Humanoid | Medium | Caster | 56/13/2 | 40 |
| Rime Stalker | enemy_rime_stalker | 30-38 | Beast | Medium | Stalker | 58/14/2 | 44 |
| Glacier Maw ◆ | enemy_glacier_maw | 36-40 | Beast | Large | Miniboss | 280/18/4 | 160 |
| Veilkin Witch ◆ | enemy_veilkin_witch | 38-40 | Humanoid | Medium | Miniboss | 240/16/3 | 150 |
| Rimelock Colossus ★ | enemy_rimelock_colossus | 30 | Elemental | Huge | Boss | 700/16/6 | 420 |

## 12. Fichas

### Undead Shambler — `enemy_undead_shambler`
**Move:** TankSlowPush 1.7 t/s
**Narração:** mineiros que o frio guardou e algo pior devolveu. Andam para frente porque é
a única direção que a morte lhes deixou.
**Ataques:** Grasp (Root 0.5s, 20%).
**Defesas:** fraco contra Fire, Silver e Radiant; **imune a Poison, Bleed e Fear; Ice -50%**.
**Comportamento:** nunca recua, nunca teme; levanta 1× ao chegar a 25% HP — a menos que
tenha sido queimado.
**Drops:** bone, grave dust.

### Frost Wisp — `enemy_frost_wisp` *(novo)*
**Move:** FloatingSlow 2.2 t/s
**Narração:** um suspiro do gelo que ganhou opinião. Onde flutua, a stamina morre primeiro.
**Ataques:** Chill Touch (Chill 1.5s, 30%); aura fria (stamina regen -20% num raio de 2 tiles).
**Defesas:** **fraco contra Fire ×2; imune a Ice e Chill**.
**Comportamento:** atravessa paredes finas; persegue o jogador com MENOS stamina.
**Drops:** frost core, essence_ice.

### Veilkin Skirmisher — `enemy_veilkin_skirmisher` *(renomeado)*
**Move:** CircleStrafe 3.6 t/s
**Narração:** elfos do Véu que seguiram os segredos fundo demais — e agora servem ao que
encontraram. Lutam como quem dança uma lembrança.
**Ataques:** Twin Blades (combo de 2); Veil Step (blink curto de 2 tiles, cd 7s).
**Defesas:** fraco contra Light e Fire; resiste Shadow.
**Comportamento:** orbita e pica; blinka ao ser focado.
**Drops:** veil cloth, silvered dagger part.

### Mirrorfin Shoal — `enemy_mirrorfin_shoal` *(novo)* [AQUÁTICA]
**Move:** SwarmErratic 4.8 t/s (apenas na água)
**Narração:** peixes-espelho dos lagos gelados — bonitos até você sangrar.
**Ataques:** Frenzy Nibble (stack leve de Bleed).
**Defesas:** inofensivos fora d'água.
**Comportamento:** cardume de 6; atraídos por Bleed no jogador.
**Drops:** mirrorfin (peixe raro de menu).

### Orc Berserker — `enemy_orc_berserker`
**Move:** GroundChase 3.4 t/s
**Narração:** o tambor ficou para trás; sobrou só a batida no peito. Quando os olhos dele
acendem, a postura deixa de importar — para ele.
**Ataques:** Frenzy Chain (3 golpes acelerando); abaixo de 30% HP ignora posture (tell: olhos brilham).
**Defesas:** fraco contra Ice e Chill; nunca bloqueia.
**Comportamento:** foca quem o feriu por último.
**Drops:** tusk, rage gland.

### Gravedelver Warder — `enemy_gravedelver_warder` *(renomeado)*
**Move:** GuardStationary → TankSlowPush
**Narração:** onde um Warder planta o escudo, o corredor deixa de existir. Os exilados
guardam o caminho do culto com a mesma teimosia com que um dia guardaram Khaz Baruk.
**Ataques:** Shield Bash (posture); Hold the Line (bloqueio frontal de 70%).
**Defesas:** exige GuardBreak ou flanqueio; as costas são honestas.
**Comportamento:** ancora corredores; avança 1 tile a cada 3s; ranged do pack atira por trás dele.
**Drops:** tower shield part, iron.

### Cultist Zealot — `enemy_cultist_zealot`
**Move:** CasterKeepAway 2.6 t/s
**Narração:** voz mansa, faca suja. O zelote não grita dogma — sussurra promessas, e a
Pedra Negra sussurra de volta.
**Ataques:** Blackstone Bolt (cast 0.9s; Corruption 10%); Dark Ward (escudo de 20 HP num aliado, cd 12s).
**Defesas:** fraco contra interrupt; Radiant ×1.5.
**Comportamento:** sacrifica Shamblers para se curar (ritual com tell de 1.2s).
**Drops:** blackstone shard (corrupted), ritual knife.

### Rime Stalker — `enemy_rime_stalker` *(novo)*
**Move:** CircleStrafe 3.8 t/s
**Narração:** felino do gelo de Husinord que desceu atrás de caça num inverno ruim e achou
um inverno melhor. Nunca ataca o rosto — o rosto olha de volta.
**Ataques:** Frostbite Lunge (0.6s; Chill 25%) — apenas pelo flanco ou costas.
**Defesas:** fraco contra Fire; resiste Ice.
**Comportamento:** caça em par espelhado: um distrai, o outro flanqueia; se encarado, circula.
**Drops:** white pelt, fang.

### ◆ Glacier Maw — `enemy_glacier_maw` *(novo, miniboss)*
**Move:** ChargeLine
**Narração:** quatro toneladas de fome com geleira por pele. A investida dele redecora a sala.
**Ataques:** Avalanche Charge (linha, tell 1.2s — derruba pilares de gelo que viram hazard); Bite.
**Defesas:** blindado. **Janela:** 3s quando a investida acerta parede.
**Drops:** glacier hide, essence_ice ×2.

### ◆ Veilkin Witch — `enemy_veilkin_witch` *(renomeada, miniboss)*
**Move:** CasterKeepAway + PhaseShortBlink
**Narração:** uma estudiosa que parou de fazer perguntas porque começou a receber respostas.
Suas cópias riem meio segundo atrasadas.
**Ataques:** Shadow Volley (3 projéteis); Mirror Veil (2 cópias ilusórias de 1 HP, cd 15s);
Hex (dano do jogador -15% por 4s).
**Defesas:** frágil quando encontrada. **Janela:** 2.5s quando o Mirror Veil quebra.
**Drops:** veil grimoire (destrava scrolls na Ozzra), essence_ice.

### ★ Rimelock Colossus — `enemy_rimelock_colossus` *(novo, boss do gate 30)*
**Move:** BossArenaControl
**Narração:** dizem que é o inverno de um ano que se recusou a acabar, empilhado em forma
de gente. O Colosso não persegue: espera, porque o frio sempre espera.
**Fases:** F1 punhos lentos + pilares de gelo; F2 (66%) chão escorregadio global (IceSlick)
+ 2 Frost Wisps; F3 (33%) cada pilar quebrado expõe o núcleo (Hammer/charged +50%).
**Defesas:** **fraco contra Fire; imune a Ice, Chill e Stun.**
**Drops (first-kill):** receita Frost Oil + Anel de Brasa (o tesouro temático invertido).

---

# PARTE E — Banda 41-55 · FIRE (Fogo)

## 13. O bioma

O calor sobe antes da luz: veios de magma respiram nas paredes e o suor vira parte do
equipamento. É a banda do *trade-off* — todo caminho rápido passa perto da lava, todo
tesouro tem um banho de brasa por perto. E no fim dela, algo pequeno demais para o céu
guarda o portão como se ensaiasse para um trono maior.

## 14. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Minor Earth Elemental | enemy_earth_elemental_minor | 41-47 | Elemental | Medium | Tank | 110/16/7 | 60 |
| Ember Hound | enemy_ember_hound | 41-48 | Beast | Medium | Pack | 64/15/2 | 48 |
| Cave Burrower Elite | enemy_cave_burrower_elite | 42-50 | Insect | Large | Elite | 130/18/4 | 80 |
| Cinder Shade [NOTURNA+] | enemy_cinder_shade | 44-52 | Undead | Medium | HazardLure | 56/14/1 | 52 |
| Veilkin Pyromancer | enemy_veilkin_pyromancer | 45-53 | Humanoid | Medium | Caster | 70/19/2 | 62 |
| Corrupted Vine Horror | enemy_corrupted_vine_horror | 46-54 | Plant | Large | Controller | 140/17/3 | 75 |
| Abyssal Hound | enemy_abyssal_hound | 47-55 | Beast | Medium | Chaser | 86/20/3 | 70 |
| Ashwing Matriarch ◆ | enemy_ashwing_matriarch | 50-55 | Beast | Large | Miniboss | 300/20/3 | 210 |
| Forge-Tyrant Vask ◆ | enemy_forge_tyrant_vask | 52-55 | Humanoid | Large | Miniboss | 380/22/8 | 220 |
| Cindershard Wyrm ★ | enemy_cindershard_wyrm | 50 | Dragon | Large | Boss | 1100/24/8 | 700 |

## 15. Fichas

### Minor Earth Elemental — `enemy_earth_elemental_minor`
**Move:** TankSlowPush 1.6 t/s
**Narração:** pedra que esqueceu de ficar parada. Não tem raiva — tem inércia, o que é pior.
**Ataques:** Boulder Fist (posture ×2); Tremor (raio 1.5, telegraph 1.0s).
**Defesas:** **fraco contra Hammer e Lightning; imune a Poison, Bleed e Burn.**
**Comportamento:** lento e inevitável; quebra paredes finas para encurtar caminho.
**Drops:** stone core, iron ore ×2.

### Ember Hound — `enemy_ember_hound` *(novo)*
**Move:** PackFlanker 4.0 t/s
**Narração:** cães de brasa que correm em três e mordem em quatro — o quarto é a explosão
de despedida.
**Ataques:** Burning Bite (Burn 20%); ao morrer incha e explode (raio 1, tell visível).
**Defesas:** **imunes a Fire/Burn; fracos contra Ice ×1.5 e água.**
**Comportamento:** matilha de 3; cercam antes do bote.
**Drops:** ember fang, essence_fire.

### Cave Burrower Elite — `enemy_cave_burrower_elite`
**Move:** BurrowAmbush 2.6 t/s
**Narração:** a Matron era o ensaio; este é o concerto. Emerge com o magma atrás.
**Ataques:** erupção + Magma Wake (rastro de lava por 2s ao emergir).
**Defesas:** blindado. **Janela:** 2.5s pós-erupção.
**Drops:** magma chitin, essence_fire.

### Cinder Shade — `enemy_cinder_shade` *(novo)* [NOTURNA: +20% no pico de Nyx]
**Move:** HazardLure 3.0 t/s
**Narração:** o que sobra de quem dormiu perto da rocha quente demais. Não quer te matar —
quer te apresentar ao fogo, como apresentaram a ele.
**Ataques:** Ash Grasp (puxa o jogador 1 tile na direção do hazard mais próximo; tell 0.9s).
**Defesas:** fraco contra Radiant e Silver; imune a Burn.
**Comportamento:** recua POR CIMA de hazards (flutua); manobra para pôr lava entre vocês.
**Drops:** shade ash, grave dust.

### Veilkin Pyromancer — `enemy_veilkin_pyromancer` *(renomeado)*
**Move:** CasterKeepAway 2.6 t/s
**Narração:** entre os Veilkin, os que estudaram fogo dizem que apenas "traduzem" a caverna.
A caverna, aqui, fala alto.
**Ataques:** Flame Lance (linha, cast 1.0s); Fire Wall (parede de 3 tiles por 4s, cd 14s).
**Defesas:** fraco contra interrupt e Ice.
**Comportamento:** corta rotas com paredes e recasta por trás delas.
**Drops:** flame focus part, essence_fire.

### Corrupted Vine Horror — `enemy_corrupted_vine_horror`
**Move:** ProtectAnchor 1.4 t/s
**Narração:** uma planta que bebeu da pedra errada. As flores ainda abrem — na direção
errada, para o lado de dentro.
**Ataques:** Lash (alcance 3 tiles); Constrict (Root 1.5s).
**Defesas:** fraco contra Fire e Axe; **Purify abre CriticalWindow** (a corrupção é a armadura).
**Comportamento:** ancorado a um núcleo menor de pedra negra (destrutível).
**Drops:** corrupted vine, blackstone shard.

### Abyssal Hound — `enemy_abyssal_hound`
**Move:** GroundChase 3.8 t/s + PhaseShortBlink
**Narração:** os cães do Abismo de Bromécia não latem. O silêncio deles chega meio segundo
antes dos dentes.
**Ataques:** Shadow Maw; blink curto quando perde o alvo de vista.
**Defesas:** fraco contra Radiant; resiste Shadow.
**Comportamento:** caça em dupla; blinka para fechar distância.
**Drops:** abyssal fang, shade ash.

### ◆ Ashwing Matriarch — `enemy_ashwing_matriarch` *(novo, miniboss)*
**Move:** FloatingOrbit 3.4 t/s
**Narração:** a senhora dos céus baixos da caverna. O ninho dela tem um ovo frio — e isso
é uma história para outra hora (e outra quest).
**Ataques:** Dive Talon (mergulho telegrafado); Ash Storm (cegueira leve 1s em área).
**Defesas:** fraca contra Bow e charged enquanto voa baixo. **Janela:** 3s após o mergulho.
**Drops:** ashwing feather (flecha rara), essence_fire.

### ◆ Forge-Tyrant Vask — `enemy_forge_tyrant_vask` *(novo, miniboss)*
**Move:** TankSlowPush + ChargeLine
**Narração:** um Gravedelver que vendeu a forja ao fogo errado. O martelo dele ainda lembra
de Khaz Baruk; o dono, não.
**Ataques:** Molten Hammer (AoE 2 — errar aplica ArmorCracked NELE = janela); Forge Slam.
**Defesas:** fortemente blindado; fraco contra Lightning e posture.
**Drops:** vask's hammer head (peça de upgrade do Brumdar), essence_fire ×2.

### ★ Cindershard Wyrm — `enemy_cindershard_wyrm` *(novo, boss do gate 50 — o dragão pequeno)*
**Move:** BossArenaControl + BossPhaseShift
**Narração:** cria do Sangue da Ruptura — pequena demais para o céu, grande demais para a
caverna. Por enquanto. O rugido dela é a primeira vez que o jogo diz a palavra "dragão"
em voz alta.
**Fases:** F1 garras e cauda (arcos telegrafados); F2 (66%) Ember Breath em cone + voo
curto (FloatingOrbit); F3 (33%) pousa exausta a cada 20s — CoreExposed por 4s.
**Defesas:** **fraca contra Ice; imune a Fire/Burn;** o primeiro rugido aplica Fear (tell).
**Drops (first-kill):** dragon ember scale (material de têmpera única) + receita do scroll Fire Wall.

---

# PARTE F — Banda 56-70 · RUINS (Ruínas de Bromécia)

## 16. O bioma

Aqui a caverna deixa de ser natural. Colunas com runas que ainda acendem, portas que se
consertam sozinhas, e sentinelas que não sabem que o império acabou. Bromécia não caiu —
foi *arquivada*, e o arquivo tem guardas. É a banda das perguntas: quem construiu, por que
desceu, e por que TUDO aqui aponta para baixo.

## 17. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Gnome Tinkerer | enemy_gnome_tinkerer | 56-62 | Humanoid | Small | Engineer | 76/18/3 | 78 |
| Ninrorin Phantom | enemy_ninrorin_phantom | 56-63 | Undead | Medium | Phase | 90/22/2 | 85 |
| Hoardmaw | enemy_hoardmaw | 56-70 | Aberration | Medium | Mimic | 130/26/4 | 110 |
| Construct Sentry | enemy_construct_sentry | 57-65 | Construct | Medium | Guard | 150/20/9 | 90 |
| Night Haunt [NOTURNA] | enemy_night_haunt | 58-66 | Undead | Medium | Stalker | 84/24/1 | 95 |
| Ruin Warden | enemy_ruin_warden | 60-68 | Construct | Large | ProtectAnchor | 220/23/10 | 120 |
| Corrupted Orc Champion | enemy_corrupted_orc_champion | 62-70 | Humanoid | Large | Elite | 260/28/6 | 140 |
| Gnome Wargolem ◆ | enemy_gnome_wargolem | 66-70 | Construct | Huge | Miniboss | 520/30/12 | 280 |
| Lich Acolyte ◆ | enemy_undead_lich_acolyte | 68-70 | Undead | Medium | Miniboss | 420/26/4 | 260 |
| Gravedelver Artificer Lord ★ | enemy_gravedelver_artificer_lord | 60 | Humanoid | Large | Boss | 1500/28/10 | 900 |
| Draconic Guardian ★ | enemy_draconic_guardian | 70 | Dragon-kin | Large | Boss | 1900/32/9 | 1100 |

## 18. Fichas

### Gnome Tinkerer — `enemy_gnome_tinkerer`
**Move:** RetreatAndCall 3.2 t/s
**Narração:** gnomos renegados convencidos de que Bromécia "só precisava de manutenção".
O problema de consertar o que não se entende é que às vezes funciona.
**Ataques:** Shock Prod; Deploy Turret (torreta de 30 HP, projétil 6 t/s, máx 1 ativa).
**Defesas:** fraco contra rush; a torreta é fraca contra Lightning.
**Comportamento:** recua montando engenhocas; conserta constructs aliados (+10% HP/s).
**Drops:** gears ×2, copper, turret core.

### Ninrorin Phantom — `enemy_ninrorin_phantom`
**Move:** PhaseShortBlink 3.0 t/s
**Narração:** ecos dos que serviram aqui antes do fim. Não assombram por maldade —
continuam *trabalhando*, e o turno deles nunca acaba.
**Ataques:** Wail (cone, Fear 1s, 25%); Phase Strike.
**Defesas:** **Physical -50%; fraco contra Radiant, Arcane e Silver.**
**Comportamento:** atravessa paredes de ruína; evita luz forte por 2s.
**Drops:** phantom essence, memory shard.

### Hoardmaw — `enemy_hoardmaw` *(novo)*
**Move:** TreasureIdleAmbush
**Narração:** a razão de se bater no baú antes de abrir. Ninguém sabe se Bromécia os criou
como armadilha ou se eles simplesmente *aprenderam* a forma do desejo alheio.
**Ataques:** Snap Bite (agarra 0.8s se o jogador "abre o baú"); Tongue Lash.
**Defesas:** fraco contra Fire. **Tell:** o baú respira sutilmente e nunca tem cadeado.
**Comportamento:** no máximo 1 por nível; se ignorado, foge engolindo 1 item do chão.
**Drops:** tudo que engoliu + roll de tesouro real.

### Construct Sentry — `enemy_construct_sentry`
**Move:** GuardStationary → GroundPatrol 2.2 t/s
**Narração:** a sentinela não sabe que o império caiu. O feixe do olho dela varre o corredor
há setecentos anos, e a sua silhueta é apenas a próxima irregularidade no relatório.
**Ataques:** Piston Jab; Overcharge Beam (linha, tell 1.2s).
**Defesas:** **fraco contra Hammer e Lightning (ShockOverloaded = janela 3s); imune a
Poison, Bleed e Fear; posture ×2.**
**Comportamento:** patrulha rota fixa; ativa ao cruzar o feixe.
**Drops:** bromecian alloy, gears.

### Night Haunt — `enemy_night_haunt` *(novo)* [NOTURNA]
**Move:** CircleStrafe 3.4 t/s (parcialmente invisível)
**Narração:** quando as tochas da sala apagam uma a uma, não é o vento. A Lua Oculta tem
filhos que nunca viram o céu.
**Ataques:** Dread Touch (Fear 1.2s, 30%) — fica visível por 1.5s ao atacar (a única janela).
**Defesas:** fraco contra Radiant; luz de tocha o revela.
**Comportamento:** apaga tochas (tell sonoro); caça quem se isola.
**Drops:** night essence, moth dust ×2.

### Ruin Warden — `enemy_ruin_warden` *(novo)*
**Move:** ProtectAnchor 1.8 t/s
**Narração:** maior, mais antigo e mais teimoso que a Sentry. Guarda portas que não levam
mais a lugar nenhum — ou que ainda levam, o que explicaria o zelo.
**Ataques:** Sweep (180°); Anchor Pulse (puxa intrusos 2 tiles na direção do núcleo).
**Defesas:** como a Sentry; o núcleo fica nas COSTAS (flanqueio é a resposta).
**Comportamento:** nunca abandona a âncora; desativa se o núcleo da sala receber Purify.
**Drops:** warden core, alloy ×2.

### Corrupted Orc Champion — `enemy_corrupted_orc_champion`
**Move:** GroundChase 3.0 t/s
**Narração:** um campeão que desceu atrás de glória e achou uma pedra que prometia mais.
O brilho no peito dele pulsa no ritmo de um coração que já não manda em nada.
**Ataques:** Blackstone Blade (Corruption 15%); Enraged Slam.
**Defesas:** posture alta; fraco contra Radiant e Purify.
**Comportamento:** a pedra no peito BRILHA antes de cada slam (tell).
**Drops:** corrupted tusk, blackstone shard ×2.

### ◆ Gnome Wargolem — `enemy_gnome_wargolem` *(miniboss)*
**Move:** TankSlowPush + ChargeLine
**Narração:** a obra-prima dos Tinkerers: um golem de guerra com caldeira bromeciana.
Funciona perfeitamente — esse é o problema.
**Ataques:** Steam Charge; Cannon Volley (3 projéteis em arco).
**Defesas:** blindagem máxima; **superaquece a cada 25s = ShockOverloaded por 4s (janela).**
**Drops:** wargolem boiler (peça única da Nimble), alloy ×3, essence_arcane.

### ◆ Lich Acolyte — `enemy_undead_lich_acolyte` *(miniboss)*
**Move:** CasterKeepAway + invocações
**Narração:** ainda não é um lich — é um estagiário da eternidade. O filactério dele é
pequeno, malfeito e escondido na sala, como tudo que é feito com pressa e medo.
**Ataques:** Soul Bolt; Raise Bones (3 Shamblers, cd 20s); Death Ward (nega 1 golpe fatal, 1×).
**Defesas:** fraco contra Radiant, Silver e Fire; **destruir o filactério menor remove o Ward.**
**Drops:** acolyte phylactery shard, grimoire page.

### ★ Gravedelver Artificer Lord — `enemy_gravedelver_artificer_lord` *(renomeado, boss do gate 60)*
**Move:** BossArenaControl
**Narração:** o senhor dos exilados, vestindo uma exo-forja que Khaz Baruk teria orgulho de
banir. Ele não serve ao culto — *cobra* dele, e o aluguel é o portão.
**Fases:** F1 martelo + 2 torretas; F2 (66%) exo-armadura a vapor (posture ×2, mais lento);
F3 (33%) sobrecarga — a arena se eletrifica em faixas e a exo entra em ShockOverloaded por ciclo (janela).
**Defesas:** fraco contra Lightning.
**Drops (first-kill):** receita Mithril Work.

### ★ Draconic Guardian — `enemy_draconic_guardian` (boss do gate 70)
**Move:** BossPhaseShift
**Narração:** meio-sangue da Ruptura criado para guardar — por quem, ele já não lembra.
Lança, escudo e uma lealdade órfã apontada para baixo.
**Fases:** F1 lança+escudo (GuardBreak necessário); F2 alterna posturas ofensiva/defensiva
(tells por cor); F3 invoca 2 Wyrmlings.
**Defesas:** fraco contra Spear/Impale nas asas (WingExposed).
**Drops:** guardian scale, dragon ember scale.

---

# PARTE G — Banda 71-85 · DEEP (Profundezas)

## 19. O bioma

Abaixo das ruínas, o escuro fica *espesso*. As lanternas iluminam menos do que deviam, os
mapas mentem por omissão, e as coisas daqui não evoluíram para caçar — evoluíram para
*esperar*. É a banda da paciência armada: tudo é emboscada, isca ou pedágio.

## 20. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Blackstone Thrall | enemy_blackstone_thrall | 71-80 | Humanoid | Medium | Fodder-elite | 150/28/4 | 130 |
| Draconic Wyrmling | enemy_draconic_wyrmling | 71-78 | Dragon | Medium | Skirmisher | 180/30/6 | 160 |
| Abyssal Lurker | enemy_abyssal_lurker | 72-80 | Aberration | Large | Ambusher | 240/34/5 | 180 |
| Deep Angler | enemy_deep_angler | 73-82 | Aberration | Large | Lure | 200/36/4 | 175 |
| Greater Earth Elemental | enemy_earth_elemental_greater | 74-83 | Elemental | Huge | Tank | 480/34/14 | 240 |
| Whisper of Veyraath | enemy_whisper_of_veyraath | 76-85 | Aberration | Medium | Caster | 170/32/3 | 200 |
| Orc Warlord ◆ | enemy_orc_warlord | 80-85 | Humanoid | Large | Miniboss | 720/40/9 | 400 |
| Goblin Warchief ◆ | enemy_goblin_warchief | 82-85 | Humanoid | Medium | Miniboss | 600/34/8 | 380 |
| Abyssal Gatekeeper ★ | enemy_abyssal_gatekeeper | 80 | Aberration | Huge | Boss | 3000/38/10 | 1800 |

## 21. Fichas

### Blackstone Thrall — `enemy_blackstone_thrall` *(novo)*
**Move:** GroundChase 2.8 t/s
**Narração:** mineiros que escutaram a pedra por tempo demais. Alguns ainda podem voltar —
e essa possibilidade é a parte mais cruel do encontro.
**Ataques:** Claw Frenzy; ao morrer perto de outros, explode em Corruption (reação em cadeia).
**Defesas:** fraco contra Radiant; **Purify o SALVA em vez de matar (vira aldeão resgatado — quest).**
**Comportamento:** enxames lentos; a cadeia de explosões pune o AoE descuidado.
**Drops:** blackstone shard ×2.

### Draconic Wyrmling — `enemy_draconic_wyrmling`
**Move:** FloatingOrbit 3.2 t/s
**Narração:** filhotes da Ruptura, raivosos como só os pequenos sabem ser. Onde há dois,
há um ninho; onde há um ninho, há uma Matriarca ou coisa pior.
**Ataques:** Spark Breath (cone curto); Tail Snap.
**Defesas:** fraco contra Ice. **Janela:** 2s após o breath.
**Drops:** wyrmling scale, essência variada.

### Abyssal Lurker — `enemy_abyssal_lurker`
**Move:** TreasureIdleAmbush → ChargeLine
**Narração:** uma dobra na parede que tem opinião sobre a sua passagem. O Abismo de Bromécia
cobra pedágio em surpresa.
**Ataques:** Tentacle Grab (Root 1s, puxa 2 tiles); Maw Crush.
**Defesas:** fraco contra Fire e Radiant.
**Comportamento:** finge ser cenário; ataca quando o jogador passa rente.
**Drops:** lurker eye, void ichor.

### Deep Angler — `enemy_deep_angler` *(novo)*
**Move:** HazardLure 2.4 t/s
**Narração:** aquela luzinha dourada ao longe, idêntica a um baú? Pisca num ritmo
ligeiramente errado. Quem percebe vive; quem corre, alimenta.
**Ataques:** Bite devastador sobre quem cai na isca.
**Defesas:** fraco contra AoE (revela a silhueta).
**Comportamento:** a "luz" imita loot a distância; recua mantendo a isca entre vocês.
**Drops:** angler lamp (UPGRADE de lanterna!), void ichor.

### Greater Earth Elemental — `enemy_earth_elemental_greater`
**Move:** TankSlowPush 1.5 t/s
**Narração:** o irmão menor era um aviso. Este é uma cláusula: a profundidade pertence à pedra.
**Ataques:** Quake (a arena treme, tell 1.4s); Boulder Throw.
**Defesas:** **imune a Physical comum até quebrar a posture; fraco contra Hammer charged e Lightning.**
**Drops:** greater core, gems.

### Whisper of Veyraath — `enemy_whisper_of_veyraath` *(novo)*
**Move:** CasterKeepAway 2.8 t/s
**Narração:** não é uma criatura — é um lugar onde o som desiste. O nome verdadeiro disso
aparece em pedaços, run após run, e nenhum pedaço melhora a noite de sono. *(Lore da Pedra
Negra revelada aos poucos — decisão Q1.4.)*
**Ataques:** Unmaking Bolt (DurabilityStress 20%); Litany of Silence (cone, cast 1.3s — silencia skills por 2s).
**Defesas:** fraco contra Radiant e interrupt.
**Comportamento:** **ouvi-lo sussurrar por 10s acumulados aplica Fear automático** — mate
rápido ou afaste-se.
**Drops:** void ichor, essence_void.

### ◆ Orc Warlord — `enemy_orc_warlord` *(miniboss)*
**Move:** ChargeLine + GroundChase
**Narração:** o último tambor. O Warlord não comanda mais ninguém — sobrou só ele e a
matemática simples da machadada.
**Ataques:** Warlord Charge (atravessa colunas); Execute (dano DOBRADO se o jogador está
abaixo de 30% HP — corra).
**Defesas:** maciço. **Janela:** 3.5s quando a investida erra.
**Drops:** warlord cleaver (arma única), essências ×2.

### ◆ Goblin Warchief — `enemy_goblin_warchief` *(miniboss)* [parcialmente NÃO-AGRESSIVA]
**Move:** PackLeader 3.6 t/s
**Narração:** a prova de que os goblins têm civilização: este unificou a banda com tática,
mérito — e um ritual de duelo que ele honra mais que a própria vida. *(Liga com o goblin
visitante da fazenda — decisão Q1.1.)*
**Ataques:** Command Volley (o pack atira em uníssono); Duel Challenge (1v1 ritual — o pack
PARA de atacar se o desafio for aceito).
**Defesas:** elite com pack de 6.
**Comportamento:** aceitar e VENCER o duelo sem que o pack morra rende a quest secreta
`scq_goblin_truce` (banda neutra por 1 run).
**Drops:** warchief crest (quest), gold ×5.

### ★ Abyssal Gatekeeper — `enemy_abyssal_gatekeeper` (boss do gate 80)
**Move:** BossArenaControl
**Narração:** o porteiro não pergunta quem é você. Pergunta — com cada tentáculo — se o
mundo lá de cima sente a sua falta.
**Fases:** F1 tentáculos por zona; F2 (66%) ciclo de portais (teleporta o jogador entre 3
plataformas); F3 (33%) devora a arena pelas bordas (a zona segura encolhe).
**Defesas:** fraco contra Radiant; **os olhos que abrem são CoreExposed.**
**Drops (first-kill):** scroll Purify maior + stabilized blackstone ×3.

---

# PARTE H — Banda 86-101 · VOID (Vazio / Pedra Negra)

## 22. O bioma

Não há mais bioma — há *ausência* organizada. As paredes lembram corredores nymirianos, o
chão lembra céu, e os habitantes lembram pessoas, ideias, hinos. Tudo aqui já foi outra
coisa. No fundo, alguém canta uma Litania ao contrário, e a caverna inteira presta atenção.

## 23. Tabela-resumo da banda

| Criatura | ID | Lvl | Família | Size | Role | HP/DMG/DEF | XP |
|---|---|---|---|---|---|---|---|
| Void Husk | enemy_void_husk | 86-94 | Aberration | Medium | Fodder | 260/40/6 | 260 |
| Veilkin Blademaster | enemy_veilkin_blademaster | 87-96 | Humanoid | Medium | Duelist | 320/46/7 | 300 |
| Starfall Remnant | enemy_starfall_remnant | 88-98 | Elemental | Large | Tank-caster | 420/44/12 | 320 |
| Silence Warden | enemy_silence_warden | 90-100 | Construct | Large | Guard | 500/42/14 | 360 |
| Dread Choir | enemy_dread_choir | 92-101 | Undead | Medium ×3 | Trio-caster | 180×3/38/4 | 150 cada |
| Herald's Hand ◆ | enemy_heralds_hand | 94-100 | Aberration | Huge | Miniboss | 1100/48/10 | 650 |
| Gravelborn Twins ◆◆ | enemy_gravelborn_twins | 96-100 | Elemental | Large ×2 | Miniboss duplo | 900 cada/44/12 | 600 (par) |
| Void Herald ★ | enemy_void_herald | 90 | Aberration | Huge | Boss | 4800/46/12 | 3000 |
| Draconic Elder ★ | enemy_draconic_elder | 100 | Dragon | Huge | Boss | 6500/52/14 | 4500 |

## 24. Fichas

### Void Husk — `enemy_void_husk`
**Move:** GroundChase 3.0 t/s
**Narração:** o estágio final do Thrall: a pessoa acabou, sobrou o gesto. Andam em grupos
porque a Pedra gosta de coros.
**Ataques:** garras; Corruption por proximidade (10%/s adjacente).
**Defesas:** fraco contra Radiant; aqui, Purify já não salva ninguém.
**Drops:** blackstone shards.

### Veilkin Blademaster — `enemy_veilkin_blademaster` *(novo)*
**Move:** CircleStrafe 4.0 t/s + PhaseShortBlink
**Narração:** o que acontece quando um povo de dançarinos dedica trezentos anos à esgrima e
trinta ao desespero. A lâmina dele responde antes da pergunta.
**Ataques:** Flurry (4 golpes); Mirror Parry (reflete 1 projétil, cd 9s).
**Defesas:** fraco contra AoE e Stagger.
**Comportamento:** duelista — castiga padrões repetidos do jogador (mesmo golpe 3× = punição).
**Drops:** master blade part, veil cloth ×2.

### Starfall Remnant — `enemy_starfall_remnant` *(novo)*
**Move:** FloatingSlow 1.8 t/s
**Narração:** um pedaço VIVO do meteoro de Elyndor — de antes de Veyraath tocá-lo. É o que
a Pedra Negra era quando ainda era só estrela. *(A peça central da revelação gradual da Q1.4.)*
**Ataques:** Meteor Shard Rain (área telegrafada); Gravity Well (puxa, 1.5s).
**Defesas:** **combo Purify→ShockOverloaded é a fraqueza desenhada.**
**Drops:** stabilized blackstone, star iron.

### Silence Warden — `enemy_silence_warden` *(novo)* [condicionalmente NÃO-AGRESSIVA]
**Move:** ProtectAnchor 2.0 t/s
**Narração:** um construct NYMIRIANO — não corrompido, apenas fiel. As runas tribais no
peito dele acendem diante de Água Viva, e por um momento a caverna inteira parece... aliviada.
**Ataques:** Judgement Sweep; Sealing Pulse (silencia magia 2.5s).
**Defesas:** fraco contra Lightning.
**Comportamento:** **para de atacar se o jogador carrega Água Viva** (quest secreta
`scq_warden_offering`; liga o Ato 3 e o NPC nymiriano).
**Drops:** nymirian engraving (peça de lore do Ato 3).

### Dread Choir — `enemy_dread_choir` *(novo)*
**Move:** FloatingOrbit em formação
**Narração:** três vozes que sobraram de um hino. Cantam a Litania ao contrário, e a
harmonia deles é uma contagem regressiva.
**Ataques:** em trio, Doom Verse (AoE Fear+Corruption, cast 2.0s); cada membro morto
enfraquece o acorde (-33% de dano do grupo).
**Defesas:** fraco contra Radiant; interrupt quebra o verso.
**Drops:** choir mask, night essence ×2.

### ◆ Herald's Hand — `enemy_heralds_hand` *(miniboss)*
**Move:** ChargeLine + puxões gravitacionais
**Narração:** a mão desgarrada do Arauto, fazendo sozinha o trabalho de um exército:
agarrar, arrastar, apresentar você ao escuro.
**Ataques:** investidas + agarrão que arrasta o jogador para fora da zona segura.
**Defesas:** fraco contra Radiant. **Janela:** após cada agarrão.
**Drops:** tabela de miniboss + essence_void.

### ◆◆ Gravelborn Twins — `enemy_gravelborn_twins` *(miniboss DUPLO — decisão Q2.4)*
**Move:** um TankSlowPush + um KiteRanged (par complementar)
**Narração:** dois irmãos de pedra nascidos do mesmo veio, lapidados pela mesma queda.
Um segura; o outro pergunta de longe se você ainda está aí.
**Mecânica:** enquanto AMBOS vivem, regeneram 1% HP/s; matar um e demorar >10s no outro =
enrage (+40%).
**Drops:** twin cores (par de acessórios únicos).

### ★ Void Herald — `enemy_void_herald` (boss do gate 90)
**Move:** BossArenaControl + BossPhaseShift
**Narração:** o primeiro nome completo que o Vazio pronuncia no jogo. O Arauto não veio
destruir — veio ANUNCIAR, e a mensagem é você quem entrega, sobrevivendo ou não.
**Fases:** F1 lanças do vazio + Husks; F2 (66%) inverte a arena (ConfusionLite zonal com
telegraph claro); F3 (33%) abre fendas (cair = dano + reposição).
**Defesas:** fraco contra Radiant e Purify.
**Drops (first-kill):** scroll Purify maior + stabilized blackstone ×3.

### ★ Draconic Elder — `enemy_draconic_elder` (boss do gate 100)
**Move:** BossPhaseShift completo
**Narração:** ele podia ter ido embora há séculos. Ficou. O Ancião do gate 100 não guarda
o portão CONTRA você — guarda você contra o que tem atrás. Cada fase da luta é um aviso
mais alto. *(Ele não é o segredo: é o carcereiro voluntário do 101.)*
**Fases:** F1 terra (garra/cauda/breath); F2 voo (FloatingOrbit + mergulhos); F3 pousa
exausto em ciclos (CoreExposed 5s) + chama Wyrmlings.
**Defesas:** fraco contra Ice nas asas e Radiant no peito.
**Drops (first-kill):** elder scale set (peça de armadura final).

---

# PARTE I — Os Quatro do Nível 101

## 25. O encontro final

Decisão humana (Q1.7 + Q1.3): o pós-100 tem QUATRO encontros, em sequência com recuperação
controlada (canon do nível 101). Nomes derivados das raízes linguísticas de Vaalara
(Luandil: ithil/ithryn/thel; Andalasia: vel/karn/zhal).

### 1. Vel-Karaúm, Warden of the Arch — `boss_vel_karaum`
Construct de Elyndor | Huge | HP 7000
O guardião do Arco Estelar. Luta de feixes, plataformas e paciência geométrica — a
tecnologia de Elyndor testando se você merece tocar a porta. Fraco contra ShockOverloaded.

### 2. Cindrathel, the Broken Remembrance — `boss_cindrathel`
Espírito | Medium (visual ×2.5) | HP 5500
O Eco de Cindar corrompido. **Espelha a BUILD do jogador** — usa a classe inferida, as
skills e o estilo de quem a enfrenta. Vencer a si mesmo é o pedágio da memória; a
recompensa é um fragmento REAL da memória de Cindar.

### 3. The Archivist of Silence — `boss_archivist_of_silence`
Aberração do Vazio | Huge | HP 9000
O antagonista canônico da main quest. Luta de INFORMAÇÃO: remove elementos do HUD por
fase (a barra de HP some, o minimapa some, os números somem) — o jogador termina lutando
com o que aprendeu, não com o que vê. Janelas abertas ao completar a Litania invertida.

### 4. Ithryndor, the Buried Dawn — `boss_ithryndor`
Dragão Ancestral | **Gargantuan (4×4)** | HP 14000
O segredo final: um dragão ancestral adormecido sob a Fonte desde o Cataclismo —
carcereiro VOLUNTÁRIO do fragmento de Esperança. A luta é condicional à escolha final:
**Proteger** = ele desperta como aliado (não se luta); **Selar** = adormece para sempre
(luta parcial cerimonial); **Usar** = ele se ergue como o último argumento do mundo
contra a sua decisão (luta completa, 4 fases).
*(Eco distante da cosmogonia dracônica de Vaalara — sem citar nomes da mesa.)*

---

# PARTE J — Decisões fechadas

```text
Catálogo nominal: 60 criaturas de banda + 4 chefes finais (64 fichas).
Renomeações limitadas a trademarks: Drow→Veilkin, Duergar→Gravedelver. Nomes em inglês.
Cada banda: 2 minibosses errantes; cada gate de 10 níveis: 1 boss dedicado.
Gates podem ter encontros duplos (Gravelborn Twins).
Dragões: Wyrmlings na caverna; Cindershard Wyrm é o boss do gate 50; Draconic Elder é o
  carcereiro do gate 100; Ithryndor (ancestral, Gargantuan) é o segredo do 101.
Criaturas noturnas existem e respondem à lua de Nyx; aquáticas exigem lago no nível.
Não-agressivas com quest: Old Scrounger King, Goblin Warchief (duelo), Silence Warden
  (Água Viva), mercadores errantes.
Blackstone Thrall purificado é SALVO, não morto (gancho do Ato 3).
Escala dentro da faixa: HP +12%/lvl, DMG +8%/lvl. Multiplicadores por tipo: BALANCE_CURVES.
Tamanhos naturais de fantasia; miniboss ×1.5 visual; boss ×2.5; colliders pela size class.
SpoilerTier: comuns 0-1, minibosses 2, bosses 3, os Quatro = 4.
```

# PARTE K — Pendências

```text
Validar em playtest os HP/DMG das bandas 71+ (TTK alvo do BALANCE_CURVES).
Definir sprites/paletas por família na fase de arte (CAVE_MONSTER_VISUAL_SPRITE).
Definir loot tables completas por família na execução da spec F06 (itens-chave já listados).
Decidir se Mirrorfin Shoal e Lake Lurker aparecem também no lago da fazenda (evento raro).
Nomear os 2 minibosses adicionais se a banda 86-101 for dividida em duas no futuro.
```
