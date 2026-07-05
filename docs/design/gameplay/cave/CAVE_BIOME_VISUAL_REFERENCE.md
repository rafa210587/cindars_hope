# CAVE_BIOME_VISUAL_REFERENCE — Direção Visual dos Biomas da Caverna

> **Status:** ATIVO — validado com o humano em 2026-07-03.
> **Propósito:** fonte canônica da identidade VISUAL de cada bioma da caverna — terreno, paredes,
> elementos, armadilhas, baús, luz e paleta — e guia para gerar as imagens de referência.
> **Não cobre:** mecânica de geração (ver `CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`),
> roster de inimigos (ver `CAVE_MONSTER_ROSTER_DIRECTION.md`).
>
> **Fontes de código (o que já está implementado e este doc respeita):**
> - Biomas: `Assets/_Game/Scripts/Cave/Data/CaveBiomeRegistrySO.cs` (8 biomas canônicos)
> - Layout por banda: `Assets/_Game/Scripts/Cave/Generation/CaveBiomeLayoutProfile.cs`
> - Armadilhas por bioma: `Assets/_Game/Scripts/Cave/Traps/TrapDefinition.cs` (10 traps v1)
> - Hazards de tile: `Assets/_Game/Scripts/Cave/Generation/CaveHazardKind.cs` (ToxicPool, IceSlick, FallingRock)
> - Baús: `Assets/_Game/Scripts/Cave/Runtime/TreasureChestInteractable.cs` + `Assets/_Game/Scripts/Cave/Traps/FalseChestTrap.cs`

---

## 1.b Direção elevada (2026-07-04): as keyarts são o ALVO, não só mood

Decisão do humano: a cave em jogo deve **refletir as keyarts** de `art/world_gpt/raw/cave_guides/`
o mais fielmente possível — chão, parede, elementos e escuridão. As keyarts deixam de ser só
referência de paleta e passam a ser o alvo visual. Ressalva técnica registrada: um Tilemap que
repete tiles não iguala uma cena-herói autoral pixel a pixel (iluminação única, props espalhados,
vinheta), então "refletir" = mesma linguagem (lajota de pedra no chão, pedras empilhadas na parede,
mesma paleta) + as camadas 2 (props) e escuridão progressiva, não um recorte literal. Minérios já
estão prontos (`Art/Generated/World/props/rock_ore_*`) — não regerar. Gerar o que faltar via GPT
(chão/parede em estilo lajota, props do bioma, bordas de Rule Tile).

## 1. Decisões de direção validadas (2026-07-03)

1. **Os 8 biomas do registro são canônicos** — nenhuma mudança de lista/ordem.
2. **Bioma 2 (níveis 11–25) é FÚNGICO com vegetação** — cogumelos gigantes bioluminescentes +
   raízes e musgo, não floresta clássica. O display name `Floresta Subterrânea` do registro fica,
   mas a arte segue a direção fúngica (as traps do bioma — esporos e laço de raiz — já apontam para isso).
3. **Biomas finais: escuridão → corrupção void.** Abismo = pedra negra quase sem luz;
   Núcleo Corrompido = cristais roxos e corrupção orgânica, ligando com os inimigos
   `ninrorin_void_*` e `corrupted_*` do roster.

---

## 2. Estilo global (vale para TODOS os biomas)

- **Estilo:** pixel art no padrão do projeto — Stardew Valley + Children of Morta (paleta rica,
  outline suave, shading em 3–4 tons por material).
- **Ângulo:** 3/4 top-down do PLAYER (near-frontal). Paredes de caverna mostram a FACE frontal
  como faixa vertical + a "borda de topo" — nunca vista aérea pura. Mesma regra dos sprites de
  mundo (ver skill `tilemap-world-rendering` e memória de ângulo de sprite).
- **Chão:** tiles sem seam, prontos para Rule Tiles (variações de tile cheio + bordas). Cada bioma
  precisa de: tile base (2–3 variações), tile de detalhe raro (pedrisco, rachadura, flor), e a
  transição chão→parede.
- **Legibilidade acima de tudo:** hazard e trap SEMPRE mais saturados/contrastados que o chão.
  Regra: se o player morrer por não ver, a arte falhou. Decor nunca pode parecer hazard e vice-versa.
- **Escuridão progressiva:** a luz ambiente cai conforme as bandas sobem. Bandas 1–2 legíveis sem
  fonte de luz; 3–5 com pontos de luz do próprio bioma (cristal, lava, braseiro); 6–7 escuras, com
  a luz vindo quase só de elementos emissivos.

### 2.1 Elementos COMPARTILHADOS entre biomas (gerar uma vez, tintar por bioma quando couber)

| Elemento | Estado no código | Direção visual |
|---|---|---|
| **Baú de tesouro** | Implementado (placeholder cor dourada) | Baú de madeira escura com ferragens douradas; versão fechada (brilho sutil) e aberta/vazia (apagada). 1 sprite base global; biomas 5–7 podem ter reskin (pedra rúnica / void). |
| **Baú falso** (`trap_false_chest`) | Implementado (placeholder laranja) | IDÊNTICO ao baú real à distância; tells sutis de perto: dentes na fresta da tampa, "respiração" (1–2 frames), musgo/gosma na base. Estado detectado: olhos visíveis na fresta. |
| **Escada/saída (ForwardExit / BackExit)** | Existe em runtime | Buraco com escada de corda (descer) e escada escavada na rocha (voltar). Silhueta idêntica em todos os biomas — só o material ao redor muda. |
| **Nós mineráveis** (MineableNode) | Reusa ResourceNode | Veios na rocha com cristal/minério exposto na cor do minério (cobre 1–10+, ferro 41+…). O corpo da rocha usa a pedra DO bioma; só o veio muda de cor. |
| **Poça tóxica** (ToxicPool) | Implementado | Líquido verde-doentio com bolhas; borda mais clara para ler o limite do tile. Reskin por bioma permitido (fúngico = gosma; núcleo = ichor roxo). |
| **Gelo escorregadio** (IceSlick) | Implementado | Placa azul-clara translúcida com brilho especular e rachaduras finas. |
| **Queda de rochas** (FallingRock) | Implementado | Telegraph: sombra + pedrisco caindo; depois pilha de pedras no chão. Estalactites no "teto" (faixa superior da parede) sinalizam a zona. |
| **Checkpoint / boss gate** | Existem em runtime | Portal/arco de pedra com runa acesa; a cor da runa pode seguir o bioma. |

---

## 3. Os 8 biomas

Formato de cada seção: **Mood** (1 frase que a imagem de guia precisa transmitir) · **Paleta** ·
**Terreno** · **Paredes** · **Elementos/decor** · **Armadilhas** (as do pool real do código) ·
**Luz** · **Âncoras do roster** (inimigos que a arte precisa acomodar).

---

### 3.1 Caverna de Pedra — `biome_stone_cavern` (níveis 1–10, banda 1)

- **Mood:** a caverna "arquetípica" e acolhedora — o tutorial do subterrâneo; perigo baixo, leitura fácil.
- **Paleta:** cinzas quentes e marrons (pedra calcária), toques de cobre esverdeado nos veios.
- **Terreno:** chão de pedra batida com pedriscos; manchas de terra seca; poças d'água rasas inofensivas.
- **Paredes:** rocha estratificada em blocos irregulares, face frontal bem definida; estalactites curtas no topo.
- **Elementos:** pilhas de pedras, cogumelos marrons pequenos (comuns, não bioluminescentes),
  restos de mineração antiga (picareta quebrada, carrinho velho, tábuas), veios de cobre.
- **Armadilhas do pool:** `trap_spike_floor` (espinhos de pedra saindo de fresta no chão),
  `trap_clockwork_dart` (bocal de dardo embutido na parede), `trap_loose_rocks` (teto instável).
- **Luz:** ambiente clara (é a banda de entrada); feixes de luz de fendas no teto perto da entrada.
- **Âncoras do roster:** kobold_scout, cave_bat, cave_mite, cracked_bone.

### 3.2 Caverna Fúngica — `biome_forest` (níveis 11–25, banda 2)

> Display name no registro: "Floresta Subterrânea". Direção validada: **fúngico com vegetação**.

- **Mood:** labirinto vivo e úmido — bonito e venenoso ao mesmo tempo; claustrofóbico (salas pequenas e numerosas).
- **Paleta:** verdes musgo + azul-turquesa e lilás bioluminescente sobre pedra marrom-escura.
- **Terreno:** pedra coberta de musgo em placas; tapetes de micélio azulado; raízes grossas cruzando o chão.
- **Paredes:** rocha úmida tomada por prateleiras de fungo (orelhas-de-pau) e raízes descendo do topo;
  cogumelos-gigantes fazem papel de "árvores" encostados na parede.
- **Elementos:** cogumelos gigantes (2–3 tiles de altura, chapéu bioluminescente), moitas de cogumelos
  pequenos coloridos, esporos flutuando (partícula), casulos/bolsas de esporo, troncos pálidos caídos, gosma.
- **Armadilhas do pool:** `trap_spore_pod` (cápsula inchada que estoura em nuvem verde — poison+slow),
  `trap_root_snare` (raiz enrolada camuflada no musgo — root), poças tóxicas frequentes.
- **Luz:** média-baixa; a bioluminescência dos chapéus e do micélio é a fonte de luz principal — halos turquesa.
- **Âncoras do roster:** mossling, mycobulwark, goblin_grashnaar_scavenger, ember_tick.

### 3.3 Caverna de Gelo — `biome_ice` (níveis 26–40, banda 3)

- **Mood:** catedral congelada — salas ENORMES e vazias onde o eco importa; beleza hostil.
- **Paleta:** azuis frios e branco, sombras azul-marinho profundas; brilhos cianos no gelo.
- **Terreno:** pedra congelada + placas de gelo translúcido (coisas presas dentro: bolhas, galhos, um esqueleto);
  neve acumulada nos cantos das salas.
- **Paredes:** gelo maciço facetado sobre rocha escura; cascatas congeladas; estalactites de gelo longas.
- **Elementos:** pilares e "agulhas" de gelo, cristais de gelo azul (mineráveis), lagos congelados,
  corpos/relíquias presos no gelo como storytelling ambiental.
- **Armadilhas do pool:** `trap_ice_plate` (placa polida quase invisível — chill),
  `trap_frost_burst_rune` (runa azul gravada no chão que estoura — dano+chill), `trap_loose_rocks` (estalactites).
- **Luz:** fria e difusa; o gelo reflete e espalha — brilho especular exagerado nas superfícies IceSlick.
- **Âncoras do roster:** frost_gnawer, icebound_sentinel, goblin_urudakh_trapper.

### 3.4 Caverna de Fogo — `biome_fire` (níveis 41–55, banda 4)

- **Mood:** forja natural — corredores apertados e quentes entre salas médias; pressão constante.
- **Paleta:** basalto cinza-escuro/preto + laranjas e vermelhos de lava; céu de sala avermelhado.
- **Terreno:** basalto rachado com brasas nas fissuras (rachaduras emissivas); cinza acumulada;
  trilhas de obsidiana lisa e escura.
- **Paredes:** rocha vulcânica angulosa; veios de lava escorrendo na face frontal; colunas de basalto hexagonal.
- **Elementos:** rios/poças de lava (bloqueio visual, não tile pisável), gêiseres de vapor, cristais de
  enxofre amarelo, restos de forja anã antiga (bigorna, corrente, portão de ferro derretido), veios de ferro.
- **Armadilhas do pool:** `trap_ember_vent` (grelha/respiradouro que cospe labareda — burn),
  `trap_loose_rocks`, poças "tóxicas" reskinadas como piche/alcatrão borbulhante.
- **Luz:** quente e pulsante — a lava é a luz; sombras longas e alaranjadas; partículas de fagulha subindo.
- **Âncoras do roster:** cinder_spitter, furnace_warden, lava_bulwark, draconic_ashspitter.

### 3.5 Ruínas Antigas — `biome_ruins` (níveis 56–70, banda 5)

- **Mood:** civilização morta engolida pela caverna — galerias GRANDES com arquitetura; quem morou aqui?
- **Paleta:** pedra trabalhada bege/cinza-esverdeada, ouro envelhecido, azul-petróleo nas sombras;
  runas em ciano apagado.
- **Terreno:** lajota de pedra trabalhada (padrão geométrico) quebrada, com o chão de caverna natural
  aparecendo nos buracos; tapetes podres; mosaicos parciais.
- **Paredes:** alvenaria de blocos grandes com baixo-relevo; colunas (inteiras e tombadas); arcos e
  portais; estátuas erodidas de figuras dracônicas/humanoides.
- **Elementos:** estátuas (inteiras/quebradas), braseiros apagados e alguns ainda acesos (verde-fantasma),
  urnas e vasos quebráveis, estantes podres, sarcófagos, escombros de teto, runas gravadas que brilham fraco.
- **Armadilhas do pool:** `trap_clockwork_dart` (bocal ornamentado na parede), `trap_rune_lock_pulse`
  (círculo de runas no chão — stun+slow), `trap_spike_floor`, `trap_false_chest` (PRIMEIRA aparição — o
  tesouro aqui é abundante e mentiroso), `trap_loose_rocks`.
- **Luz:** pontual e teatral — braseiros e runas; feixes de poeira dourada caindo de rachaduras altas.
- **Âncoras do roster:** corrupted_bone_knight, corrupted_lich_shard, gnomorin_rune_tinker, drow_arcane_adept.

### 3.6 Abismo Sombrio — `biome_abyss` (níveis 71–85, banda 6)

- **Mood:** a escuridão como inimiga — denso, claustrofóbico, a lanterna do player importa; olhos no escuro.
- **Paleta:** preto + cinza-carvão; azul-aço quase dessaturado; raros pontos de luz branca fria
  (fungos pálidos) e o roxo distante da corrupção chegando.
- **Terreno:** rocha negra lisa; fendas sem fundo (borda de penhasco como decor bloqueante);
  cascalho escuro que "some" na sombra.
- **Paredes:** basalto negro sem detalhe interno visível — silhueta e outline sutil; formações de
  obsidiana; teias densas nos cantos.
- **Elementos:** pontes naturais de pedra sobre o vazio, fungos-lanterna pálidos (única vida),
  ossadas grandes, teias, cristais negros que ABSORVEM luz (outline roxo), pares de olhos no fundo (decor).
- **Armadilhas do pool:** `trap_spore_pod`, `trap_root_snare`, `trap_ice_plate`, `trap_spike_floor`,
  `trap_ember_vent`, `trap_false_chest` — o Abismo mistura quase tudo, e a escuridão as esconde.
- **Luz:** MÍNIMA — vinheta forte; o que emite luz é raro e pálido; hazards precisam de brilho próprio
  para continuarem legíveis (regra da seção 2 vale dobrado aqui).
- **Âncoras do roster:** drow_shadow_warden, ninrorin_echo_shade, ninrorin_void_sentinel, blackstone_wyvern.

### 3.7 Núcleo Corrompido — `biome_core` (níveis 86–99, banda 7)

- **Mood:** a fonte da doença de Vaalara — a caverna deixou de ser pedra e virou outra coisa; errado em todos os detalhes.
- **Paleta:** roxos e magentas void sobre preto; veias rosa-carne; cristais violeta emissivos; acentos ciano doentio.
- **Terreno:** rocha "infectada" — placas de pedra com veias pulsantes de corrupção entre elas;
  trechos de carne/membrana; cristal void crescendo do chão como erva daninha.
- **Paredes:** massa de cristal void + rocha derretida em formas orgânicas; geometria levemente
  impossível (ângulos que não fecham, blocos flutuando a centímetros da parede).
- **Elementos:** cristais void gigantes (luz principal), olhos e bocas na parede (decor animado 2 frames),
  "ovos"/casulos de corrupção, fragmentos de ruína dracônica flutuando, ichor roxo escorrendo,
  membranas que pulsam.
- **Armadilhas do pool:** `trap_rune_lock_pulse` e `trap_frost_burst_rune` (runas corrompidas roxas),
  `trap_clockwork_dart`, `trap_false_chest`, `trap_loose_rocks` — reskin void de tudo: dardos viram
  espinhos de cristal, runas viram glifos de corrupção.
- **Luz:** a corrupção É a luz — tudo emissivo em roxo/magenta; pulso lento global (respiração do núcleo).
- **Âncoras do roster:** ninrorin_void_knight, corrupted_draconic_spawn, draconic_void_wyrm, draconic_elder_kin.

### 3.8 Boss Final — `biome_final` (nível 100)

- **Mood:** arena — um único espaço memorável; o coração exposto da corrupção.
- **Direção:** evolução extrema do Núcleo Corrompido: plataforma circular de ruína dracônica suspensa
  no void, cristais colossais ao redor, o "coração" pulsando ao fundo. Sem decor de preenchimento —
  tudo na sala serve à luta (legibilidade máxima de telegraphs).
- **Nota:** conteúdo/armadilhas dos níveis 100/101 estão fora de escopo v1 (spec_fable_60);
  a arte da arena pode ser a última do lote.

---

## 3.9 Arquitetura de renderização do terreno procedural (3 camadas) — decisão 2026-07-03

A cave é procedural (grid de células gerado por seed), então o terreno se constrói em **3 camadas**:

1. **Chão/parede = Tilemap + Rule Tiles.** O gerador já produz grid; Rule Tiles resolvem
   automaticamente bordas/quinas/face frontal/topo de parede a partir de ~15–20 peças por bioma —
   qualquer forma gerada sai orgânica, sem parecer grade. Variação de tile por **hash
   determinístico da célula** (`worldSeed|runSeed|level|x|y`) — nunca Random (stable-run).
2. **Elementos grandes = sprites livres (props), NÃO tiles.** Cogumelo gigante, pilar de gelo,
   estátua, cristal colossal: 1 sprite com Y-sort e collider próprio, posicionado pelo planner em
   células válidas (DecorBlocking/NonBlocking do fable_78). É esta camada que quebra a sensação de grade.
3. **Set-pieces = stamps (moldes), só como exceção.** Sala de tesouro, arena do boss (nível 100) e
   entrada podem ser áreas autoradas à mão "carimbadas" no lugar da sala gerada (room template).
   O terreno geral NUNCA é por molde — moldes como base exigiriam dezenas por bioma e refariam a geração.

Consequência para a produção de arte (seção 4): o guia de terreno/parede de cada bioma deve ser
produzido como **peças de tileset para Rule Tile** (tile base ×3, detalhe, bordas, quinas, face
frontal e topo de parede) + **props isolados** — nunca como cena pronta. As keyarts são referência
de paleta/mood, não fonte de recorte.

Implementação em código: spec `spec_cave_biome_art_profiles_runtime` (CV01). Stamps de set-piece
ficam para spec futura.

**Decisão 2026-07-04 (humano): abordagem MISTA confirmada.** O terreno geral fica procedural por
tile (camadas 1–2); as **salas-herói** (tesouro, boss, entrada) viram **stamps** autorados à mão —
templates pintados que o gerador carimba. Cadência: um bioma por vez no GPT, terminando a Caverna de
Pedra como "molde de qualidade" antes de replicar. Riqueza visual sem quebrar o procedural vem de:
(a) **tile set rico** — variações de chão/parede + **bordas/quinas de Rule Tile** que arredondam a
parede em qualquer formato (maior impacto; o corte reto atual é o principal gap vs. keyart);
(b) **feature chunks** — formações multi-tile (entulho 3×3, aglomerado de cristal/minério, cogumelo
gigante, estalactites de teto) que o decor planner (CV02) carimba nas salas. Câmaras-imagem inteiras
foram DESCARTADAS: não encaixam no gerador (salas de tamanho/posição variáveis por seed) e exigiriam
reescrever a geração, perdendo a variedade. Limitação de pipeline registrada: autotile 47-peças
pixel-perfeito não é confiável via GPT-image; bordas saem como "rim" coeso fatiado + complemento de
sombra chão↔parede no código.

**STATUS (2026-07-03):** CV01 `BUILD_VALIDATED` (Play Mode/EditMode Unity real deferidos —
ver `docs/validation/spec_cave_biome_art_profiles_execution_report.md`). Contrato de dados
(`CaveBiomeArtProfileSO`), resolver puro banda→arte (`CaveBiomeArtResolver`), evento
`CaveBiomeChangedEvent`, integração fallback-first em trap/chest/hazard/exit, Tilemap floor/wall
condicional, gerador/validador nos 3 comandos canônicos e toggle dev de banda fixa (T011) foram
implementados. `biome_stone_cavern` (banda 1) já tem terreno de teste staged
(`Art/Generated/World/cave/biome_stone_cavern/{floor_a,floor_b,wall_face,wall_top}.png`); os
outros 7 biomas seguem sem arte (profile vazio = fallback aos placeholders atuais). Lote 2 de arte
(props/hazard/trap/chest/exit sprites, demais biomas) permanece como entrega futura.

---

## 4. Plano de imagens de guia e referência

Objetivo: para cada bioma, um pacote de imagens que sirva de contrato visual ANTES de produzir
tiles finais. Pipeline conforme memória do projeto: peças modulares/tiles pela web do ChatGPT
(projeto "Sprites - Fazendeiro", skill `chatgpt-web-sprite-gen`, ângulo 3/4 por componente);
props isolados podem ir pelo pipeline local (skill `sprite-generation-pipeline`).

### 4.1 Pacote por bioma (7 biomas + arena = 8 pacotes)

| # | Imagem | Conteúdo | Uso |
|---|---|---|---|
| 1 | **Mood/keyart** (1 por bioma) | Cena ampla do bioma no ângulo do jogo, com chão+parede+2–3 elementos+1 fonte de luz | Alinhar paleta e clima; NÃO vira asset |
| 2 | **Guia de terreno** | Tile base ×3 variações + tile de detalhe + transição chão→parede | Base do tileset/Rule Tiles |
| 3 | **Guia de parede** | Face frontal + topo + quina interna/externa + 1 variação (veio/raiz/gelo) | Tileset de parede |
| 4 | **Sheet de elementos** | 6–10 props do bioma (seção 3.x) em grade, mesmo ângulo/escala | Props/decor blocking e non-blocking |
| 5 | **Sheet de armadilhas** | Cada trap do pool do bioma em 3 estados: armada (sutil) / telegraph / disparada | Sprites de trap legíveis |

### 4.2 Pacote global (1x, compartilhado)

- Baú de tesouro: fechado / abrindo / aberto-vazio + baú falso (idêntico + tells + detectado).
- Hazards de tile: ToxicPool / IceSlick / FallingRock (base + reskins citados na seção 2.1).
- Saídas: escada de descida + escada de retorno.
- Nó minerável: rocha base + 3 cores de veio (cobre/ferro/cristal).
- Checkpoint/boss gate: arco com runa (a runa tinta por bioma).

### 4.2.1 STATUS — lote 1 gerado em 2026-07-03 (ChatGPT web / gpt-image-1)

Staging: `art/world_gpt/raw/cave_guides/` (11 PNGs, auditados visualmente — ângulo e estilo OK):

| Arquivo | Conteúdo |
|---|---|
| `gpt_cave_guide_chests.png` | Baú real (fechado/abrindo/aberto) + mimic (idêntico/tells/revelado) |
| `gpt_cave_guide_hazards.png` | ToxicPool + reskins (ichor roxo, gosma fúngica), IceSlick, FallingRock (telegraph + pilha) |
| `gpt_cave_guide_fixtures.png` | Saída descida/subida, nó minerável (cobre/ferro/cristal roxo), arco de checkpoint c/ runa |
| `gpt_cave_keyart_stone_cavern.png` … `gpt_cave_keyart_final_arena.png` | 8 keyarts de mood, uma por bioma |

Pendentes do plano (lote 2+): guias de terreno (#2), guias de parede (#3), sheets de elementos (#4)
e sheets de armadilhas em 3 estados (#5) por bioma.

### 4.3 Ordem de produção sugerida

1. Pacote global (destrava legibilidade de gameplay em qualquer bioma).
2. Caverna de Pedra (banda 1 — é onde o player passa as primeiras horas).
3. Fúngica → Gelo → Fogo → Ruínas (ordem de progressão).
4. Abismo → Núcleo → Arena (podem iterar depois; dependem das decisões de corrupção visual).

---

## 5. Identidade sonora por bioma (música + ambiência)

> Decisão validada 2026-07-03: **cada bioma tem música própria** que reflete seu clima, além da
> camada de ambiência. Implementação futura via GameEventBus (skill `audio-event-wiring` /
> sistema de audio fable_58): trocar a track ao entrar num nível cuja banda difere da anterior,
> com crossfade — nunca AudioSource direto em código de gameplay.

| Bioma | Música (direção) | Ambiência (loop de fundo) |
|---|---|---|
| **Caverna de Pedra** | Calma e exploratória — violão/harpa esparsa, percussão leve de "pedrinhas"; tom de descoberta | Gotejar de água, eco distante, morcegos |
| **Caverna Fúngica** | Orgânica e curiosa — marimba/kalimba, pads úmidos, sinos detune; levemente psicodélica | Esporos "chiando", gosma, criaturas pequenas se movendo |
| **Caverna de Gelo** | Espaçosa e frágil — piano esparso com muito reverb, glass harmonics, cordas frias; andamento lento | Vento gelado, gelo estalando/rangendo, eco longo |
| **Caverna de Fogo** | Tensa e rítmica — percussão tribal/metálica (bigorna), baixos graves pulsando; andamento mais rápido | Lava borbulhando, vapor, rocha se partindo |
| **Ruínas Antigas** | Solene e misteriosa — coral fantasma, órgão baixo, motivos "antigos" (modo frígio/dórico) | Poeira caindo, mecanismos distantes, sussurros |
| **Abismo Sombrio** | Quase não-música — drones graves, silêncio como instrumento, stingers raros; tensão de horror contido | Respiração de algo grande, gotas em vazio profundo, nada |
| **Núcleo Corrompido** | Distorcida e errada — os motivos dos biomas anteriores retornam corrompidos (detune, reverso, bit-crush), pulso cardíaco constante | Pulsação de carne, cristal ressoando, vozes void |
| **Boss Final** | Épica — o tema da corrupção em força total + o motivo principal do jogo em contraponto; camadas por fase do boss | (a música domina; ambiência mínima) |

Regras de coesão: um **motivo melódico do jogo** deve aparecer em todas as tracks (variado por bioma)
para dar unidade; a transição de banda usa crossfade (~2s); combate pode adicionar camada de
intensidade (layer) em vez de trocar de música.

---

## 6. Lacunas conhecidas (fora do escopo deste doc, registrar onde couber)

- Tilesets finais e wiring no Unity (Rule Tiles, CompositeCollider2D) — skill `tilemap-world-rendering`.
- Guardião de sala de tesouro anotado em spec_fable_09 mas não confirmado no materializer.
- Integração do baú com LootTableSO (hoje fallback hardcoded).
- Consumidor da detecção de trap (F23) dormante.
- Conteúdo dos níveis 100/101.
- Produção/aquisição das tracks de música por bioma e wiring no sistema de audio (seção 5) — exige spec própria.

---

*Criado: 2026-07-03 — direção validada com o humano (fúngico p/ banda 2; escuridão→void p/ bandas 6–7).*
