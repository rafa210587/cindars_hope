# Cindar's Hope — Questionário de Decisões para Fechar TODOS os Refinamentos (v1.0)

> ✅ **RESPONDIDO em 2026-06-12.** Registro vinculante das respostas: docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md. Este arquivo é histórico.


> **Status:** aguardando respostas humanas. Cada resposta vira refinamento/spec fable.
> **Data:** 2026-06-12
> **Como responder:** marque a opção (A/B/C...) ou escreva valor. `[default]` = o que assumo
> se você não responder. Perguntas com ⚠️ travam refinamentos inteiros.
> **Referências de Vaalara lidas (clima, não cópia):** Deuses (12 maiores + menores),
> Cosmogonia (Veyraath, dragões, Cataclismo, Maltheris/Necrodraco), Raças (10 povos),
> Dornécia ("Coração Fragmentado", poços astrais), Reinos/Cidades (Cindar's Hope canônica
> em Dornécia, perto de Cindabar/Ochranas Deep).

---

# SEÇÃO 1 — Lore e clima (amarra Vaalara → jogo)

**Q1.1 ⚠️ Raças visíveis na vila.** O roster de NPCs tem nomes que sugerem raças
(Brumdar Ferro-Quieto=anão?, Nimble Galhobaixo=gnomo/halfling?, Zrix=?, Ozzra=?).
Quais povos de Vaalara existem em Cindar's Hope como NPCs?
- A) Só humanos (vila humana; raças aparecem como viajantes raros)
- B) [default] Humanos + anões + gnomos/halflings (vila rural mista de Dornécia; elfos/draconatos só como visitantes de evento)
- C) Mista completa (incl. tiefling/draconato residentes)
- Resposta: ______ | Se B/C: defina a raça de cada um dos 23 NPCs? (posso propor tabela: S/N)

**Q1.2 Templos/capelas na cidade.** Canon do jogo: templo de Kanthor + capela triste de
Anya + culto rural de Thandra. Confirma esses 3 e nenhum outro? (Mesa tem Merithus, Finan
etc.) — [default: sim, 3; Finan/Merithus só como amuletos/ditos populares]
- Resposta: ______

**Q1.3 Dragões/Cosmogonia.** A mesa tem dragões centrais (cromáticos, Maltheris). No jogo:
- A) [default] Só ecos: Draconic Wyrmling/Guardian/Elder da caverna são "crias de sangue da
  Ruptura", sem dragão verdadeiro nomeado
- B) Um dragão ancestral adormecido é o segredo abaixo do nível 101 (muda o endgame!)
- C) Zero menção dracônica
- Resposta: ______

**Q1.4 Pedra Negra cultista — origem (pendência do canon).**
- A) [default] Meteoro de Elyndor corrompido por Veyraath/Contra-Criação (eco da cosmogonia)
- B) Artefato de Nyx
- C) Fragmento planar sem explicação no jogo
- Resposta: ______

**Q1.5 Nymirianos remanescentes.** Existe 1 NPC nymiriano vivo/oculto no jogo (revelação
de Ato 3)? [default: não — só ruínas/ecos; Cindar é memória]
- Resposta: ______

**Q1.6 A Fonte é** (pendência do canon): A) artefato nymiriano B) milagre de Anya
C) [default] híbrido (obra nymiriana SOBRE manifestação de Anya) D) decidir no Ato 4
- Resposta: ______

**Q1.7 Os 3 chefes pós-nível 100 (Elyndor).** Proponho: Guardião do Arco (construct),
Eco de Cindar corrompido (espelho do jogador), Arquivista do Silêncio (final). Aprova
nomes/temas? — Resposta: ______

---

# SEÇÃO 2 — Monstros (lista atual + renomeação + expansão)

## 2.1 Lista ATUAL do roster no código (44)

```text
Banda 1-10:  Verdant Mite, Spore Crawler, Goblin Scrounger, Kobold Sentry, Rot Beetle,
             Pale Grub, Mushroom Puffball
Banda 11-25: Goblin Shaman, Kobold Trapmaster, Orc Grunt, Cave Leaper, Duergar Crossbowman,
             Burrowing Maggot, Fungal Spreader
Banda 26-40: Drow Skirmisher, Orc Berserker, Duergar Warder, Undead Shambler, Cultist Zealot,
             Gnome Tinkerer, Phase Stalker
Banda 41-55: Minor Earth Elemental, Cave Burrower (Elite), Drow Witch, Undead Knight,
             Construct Sentry, Abyssal Hound, Corrupted Vine Horror
Banda 56-70: Ninrorin Phantom, Gnome Wargolem, Draconic Wyrmling, Abyssal Lurker,
             Corrupted Orc Champion, Greater Earth Elemental, Lich Acolyte
Banda 71-85: Draconic Guardian, Goblin Warchief, Orc Warlord, Abyssal Gatekeeper
Bosses/86+:  Cave Mite Queen, Fungal Patriarch, Duergar Artificer Lord, Void Herald,
             Draconic Elder
```

## 2.2 ⚠️ Renomeação (Duergar/Drow/Lich = identidade D&D-WotC; demais são genéricos mas pediu-se desvio leve)

Proposta de renomeação (mantém leitura, evita cópia):

| Atual | Proposta | Atual | Proposta |
|---|---|---|---|
| Goblin * | **Grimlin** * | Duergar * | **Fundeiro** * (anões caídos do subsolo) |
| Kobold * | **Korbel** * | Drow * | **Velkari** * (elfos do Véu corrompido) |
| Orc * | **Orneg** * | Lich Acolyte | **Acólito do Ossário** |
| Abyssal * | **do Abismo de Bromécia** | Draconic * | **Sangue-da-Ruptura** * |
| Phase Stalker | Espreitador do Véu | Void Herald | Arauto de Veyraath (liga Q1.4) |
| Ninrorin Phantom | mantém (original) | Cultist Zealot | Zelote da Pedra Negra |

- Q2.2a Aprova a tabela? (edite à vontade) — Resposta: ______
- Q2.2b Nomes exibidos em PT-BR (Sentinela Korbel) ou EN estilizado? [default PT-BR] — ______

**Q2.3 Tamanho-alvo do bestiário.** 44 hoje. Alvo: A) manter 44 B) [default] 60
(+16: 2-3 por banda p/ variedade, incl. 1 criatura aquática de lago subterrâneo, 1 de gelo,
2 noturnas de Nyx) C) 80+ — Resposta: ______

**Q2.4 Minibosses por gate.** Gates a cada 10 níveis (10 minibosses) + 3 pós-100. Confirma
1 miniboss único POR gate (sem repetição)? [default sim] — Resposta: ______

---

# SEÇÃO 3 — Catálogo de itens (estado atual + alvos)

## 3.1 Estado atual no código (~45 itens)

```text
Sementes(6): wheat, carrot, moonbean, sunpepper, crystal_berry, starroot
Crops(6): idem | Comidas(7): bread, carrot_stew, moonbean_soup, spicy_sunpepper,
crystal_jam, starroot_pie, potion_hp_small | Materiais(4): wood, stone, copper_ore, iron_ore
Kits reparo(3) | Ferramentas: hoe, watering_can, pickaxe, fishing_rod, axe | Peixe(1)
Armas/magia: sword_iron, bow?, fireball_test | Especiais: fruto_mana, agua_viva,
pedra_negra_estabilizada, blackstone_corrupted_shard
```

**Q3.1 Alvos por categoria** (preencha número OU aceite default):
sementes/crops 6→[12] · comidas/receitas 7→[20] · poções 1→[8: HP S/M, MP S/M, antídoto,
resist fire/ice, stamina] · óleos de arma 0→[4: fire/frost/shock/poison — canônicos] ·
flechas 1→[6 das 10 canônicas] · materiais 4→[10: +prata, mithril bruto, cristal arcano,
liga bromeciana, couro, essências×6 (Seção da Têmpera)] · armas 2→[16: 8 tipos × 2 tiers] ·
armaduras 0→[6: light/medium/heavy × 2 tiers] · escudos 0→[3] · wands 0→[3] · scrolls 0→[4]
· acessórios 0→[12] — Respostas: ______

**Q3.2 ⚠️ Qualidade de crop** vira: A) [default] item separado por qualidade? NÃO — campo
no ItemStack (precisa estender inventário) B) multiplicador de preço só C) itens _prata/_ouro
separados (simples, incha catálogo) — Resposta: ______

**Q3.3 Nomes das 6 sementes novas.** Proponho (clima Vaalara/luas): Lágrima-de-Alihana
(noturna), Pimenta-de-Senya, Raiz-de-Sombra (Nyx), Trigo-Dourado-de-Thandra, Abóbora-do-Vale,
Uva-de-Brigandini. Aprova/edita? — ______

---

# SEÇÃO 4 — Acessórios

**Q4.1** Confirma 3 slots (Ring/Amulet/Charm — tipos canônicos) e o catálogo de 12 da
Parte C2 do SYSTEMS_DEEPENING v1.1? Ajustes? — Resposta: ______
**Q4.2** Relíquias divinas (4º tipo, raras, 1 por deus maior, drop de boss/quest): entram
no v1 ou ficam pós-MVP+? [default: pós] — ______

---

# SEÇÃO 5 — Eventos e festivais

**Q5.1 ⚠️ Calendário concreto** (4 estações × 28 dias). Proposta: Primavera d7 Festival do
Plantio (Thandra); d21 Feira de Caravana (Merithus/Finan, mercador raro). Verão d14 Festival
das Luas (3 luas, música da Liora); d28 Torneio do Portão (Alaric/Kaand, arena leve). Outono
d7 Colheita (concurso de crop); d21 Noite dos Véus (Nyx/Alihana, mercado noturno expandido).
Inverno d14 Vigília de Anya (na Fonte, lore); d28 Ano-Novo (fogos, presentes). Aprova/edita
datas e temas? — Resposta: ______
**Q5.2** Eventos lunares mecânicos: cada lua tem 1 noite de pico por estação (efeitos do
canon §27-29). Frequência ok? [default sim] — ______
**Q5.3** Eventos aleatórios de fazenda (corvos, chuva de meteoro=cristal arcano, visita):
quantos no v1? [default 4] — ______

---

# SEÇÃO 6 — Sistema de quests AMPLIADO (maior gap de definição)

Estado: QuestRegistry com 3 quests + Ato 1 especificado (fable_10). Sem XP de quest, sem
quadro, sem dailies.

**Q6.1 ⚠️ Onde o jogador PEGA quests?** Proposta de 4 fontes:
1. Main: NPC da história (Corvus/Thalindra/Maelor) com "!" dourado
2. Side fixas: NPCs com "!" prateado (1-3 por NPC, ligadas ao serviço dele)
3. **Quadro de Avisos** (praça, gerido pelo Hund): 3 contratos rotativos/dia
   (caça/coleta/entrega) — aceita no quadro, entrega no quadro
4. Cave contracts: quadro do Zrix na entrada (objetivos de profundidade/boss)
Aprova as 4? Limite de quests ativas simultâneas? [default: sem limite, 1 tracked]
- Resposta: ______

**Q6.2 ⚠️ XP e recompensas por tier.** PlayerProgression tem XP/level. Proposta:

| Tier | XP | Ouro | Extra |
|---|---|---|---|
| Daily (quadro) | 15-30 | 30-80g | — |
| Side curta | 40-60 | 50-150g | item útil |
| Side longa/cadeia | 80-150 | 150-400g | item único/receita/acessório |
| Main (por quest do ato) | 150-250 | 100-300g | fragmento/desbloqueio + skill point? |
| Cave contract | 60-120 | 100-250g | essência/material raro |

Q6.2a Aprova faixas? — ______ | Q6.2b Main quest dá +1 skill point por ATO? [default sim] — ______

**Q6.3 ⚠️ Estrutura de cadeias side.** Proposta: 1 cadeia de 3 quests POR NPC (23 cadeias,
69 quests side) que destrava o serviço único do NPC no final (liga Parte C5) + amizade
futura. Volume ok ou reduzir p/ 12 NPCs prioritários? [default: 12 no v1, 23 no v2] — ______

**Q6.4 Conexão entre quests.** Regras propostas: side de NPC referencia main no diálogo
quando o ato avança; cadeia do Brumdar (têmpera) exige Ato 1 completo; cadeia da Ozzra
destrava scrolls (F07); daily nunca conflita com main (sem itens de quest compartilhados).
Aprova? — ______

**Q6.5 UI de quests.** Log atual (J) lista tudo. Adicionar: abas Main/Side/Contratos,
tracker HUD (1 quest, objetivos resumidos), "!" sobre NPCs, marcador no quadro. Confirma?
[default sim] — ______

---

# SEÇÃO 7 — Movimentação e ataque de SKILLS (sem arte ainda)

**Q7.1** Toda skill ativa declara: telegraph (cor/flash 0.1-0.3s), deslocamento (lunge
tiles), shape (arc/circle/line/projétil), recovery. Proposta: tabela por skill canônica
derivada dos shapes já definidos (Corte Amplo=arco 140° 1.2 tiles; Golpe de Ruptura=lunge
1.5 + círculo 1.0; Flecha Perfurante=linha 6 tiles pierce; Disparo de Interrupção=projétil
rápido 8 tiles...). Eu gero a tabela completa das ~20 ativas canônicas para sua revisão?
[default sim] — Resposta: ______
**Q7.2** Skills com deslocamento respeitam colisão e NÃO atravessam inimigos (regra dash
do canon)? [default sim] — ______

---

# SEÇÃO 8 — Movimentação e ataque de CRIATURAS

**Q8.1** Confirma implementar os 12 Moves oficiais faltantes (PackFlanker, PackLeader,
RetreatAndCall, FloatingSlow, FloatingOrbit, CircleStrafe, ChargeLine, TreasureIdleAmbush,
HazardLure, ProtectAnchor, BossArenaControl, BossPhaseShift) com as faixas de velocidade do
COMBAT_CORE §16? [default sim — vira emenda F04/F05] — ______
**Q8.2** Windup/recovery padrão por papel (proposta): comum 0.5s/0.5s; elite 0.7s/0.6s
(mais dano); boss 0.9-1.2s/0.8s (telegraph maior). Aprova baseline? — ______
**Q8.3** Projétil inimigo: velocidade 5-7 tiles/s (esquivável andando) ok? [default sim] — ______

---

# SEÇÃO 9 — Balanceamento (curvas concretas)

**Q9.1 ⚠️ Level cap do player e curva de XP.** Proposta: cap 50; XP p/ level N =
80×N^1.6 (lv2=242, lv10=3.2k, lv25=14k, lv50=42k acumulado ~470k); fontes: kills (5-40 por
inimigo conforme banda), quests (Q6.2), descobertas (bestiário/nível novo +10).
Aprova cap e curva? — ______
**Q9.2 HP/dano de inimigos por banda** (multiplicador sobre o roster base): banda 1-10 ×1;
11-25 ×1.6; 26-40 ×2.4; 41-55 ×3.5; 56-70 ×5; 71-85 ×7; 86-101 ×10. TTK alvo (player no
nível adequado da banda): comum 3-6s, elite 12-20s, miniboss 45-90s, boss 2-4min. Aprova? — ______
**Q9.3 Dano do player esperado por banda** — eu derivo tabela de validação (arma do tier ×
atributos esperados) p/ sua revisão? [default sim] — ______

---

# SEÇÃO 10 — Classes INFERIDAS do jogador (conceito novo seu)

**Q10.1 ⚠️ Como inferir.** Proposta: título dinâmico pelo MAIOR investimento (pontos de
skill por árvore + uso de arma dos últimos 3 dias): Lâmina (Melee), Caçador (Ranged),
Arcanista (Magic), Andarilho (Survival), Artesão (Crafting) + híbridos (2 árvores ≥40%:
"Lâmina Arcana" etc.). Recalcula ao dormir. Aprova modelo? — ______
**Q10.2 ⚠️ Bônus da classe inferida.** A) [default] Só título+diálogo (NPCs comentam;
zero poder — evita double-dip com as árvores) B) bônus pequeno (+3% na família dominante)
C) desbloqueia 1 diálogo/serviço especial por classe (ex.: Artesão tem desconto do Brumdar)
- Resposta: ______ (B/C precisam de regra anti-respec-abuse)

---

# SEÇÃO 11 — HUD e menus

**Q11.1 Layout do HUD** (canon define O QUE, falta ONDE). Proposta 1280×720:
HP/MP/Stamina barras horizontais canto sup-esq (240×20px cada, empilhadas); fome/fadiga
ícones+anel ao lado; relógio/data/clima/lua canto sup-dir (F20); hotbar 10 slots centro-inf
(slots 48px); 4 active skills à direita da hotbar (56px, tecla visível); tracker de quest
faixa dir (300px); toasts acima da hotbar; boss bar topo-centro quando ativa. Aprova/edita? — ______
**Q11.2 Menus** (telas F14): A) [default] tela cheia com abas laterais (Inventário/
Equipamento/Skills/Quests/Bestiário/Calendário — Tab cicla) B) janelas separadas como hoje
- Resposta: ______
**Q11.3** Minimapa da caverna (fog of war, células visitadas) entra no v1? [default: v2] — ______

---

# SEÇÃO 12 — Cenas: tamanhos e posições

**Q12.1 Tamanhos-alvo** (tiles; hoje Town=36×30, Farm≈26×20, Cave=por nível ~40×40):
A) [default] manter Town 36×30 e Farm 26×20 (densificar, não crescer) B) Farm 40×30
(expansões futuras de lote precisam — alvará do Tovin) C) outro: ______
**Q12.2 Farm — posições** (proposta de mapa lógico): casa+cama SW(-5,-7); Fonte ao lado
da casa (-3.5,-7) ⚠️(F17); campos centro (24 canteiros); celeiro/coop N (Zone_Construction);
lago SE; árvores E; rochas NW; entrada caverna W(-5.5,0); saída cidade SW; shipping N.
Aprova? — ______
**Q12.3 Tamanho dos elementos** (player 32×48px ≈ 1×1.5 tiles): casa 4×3 tiles, celeiro
3×3, Fonte 2×2, estátua 2×3, barraca 2×1.5, árvore 1×2, canteiro 1×1, NPC 1×1.5, boss ×2.5
visual. Aprova baseline? — ______
**Q12.4** Interiores (F11): mesma cena deslocada (atual proposta) ou cenas separadas?
[default: mesma cena] — ______

---

# SEÇÃO 13 — Futuros (fechar escopo agora, implementar depois)

**Q13.1** Ordem dos futuros pós-MVP+: A) [default] Bestiário UI → Companions → Social/
amizade → Festivais → Mana/endgame → Pets → Automação B) outra: ______
**Q13.2** Pets: mantém HOLD até arte? [default sim] — ______
**Q13.3** Romance: quais dos candidatos do roster v1.1 entram na 1ª leva (sugiro 6:
Sylveth, Mirela, Liora, Zrix, Savra, Orlan)? — ______
**Q13.4** Multiplayer/co-op: descartado de vez? [default: fora de escopo permanente] — ______

---

# Próximo passo

Com as respostas (mesmo parciais — defaults cobrem o resto), eu gero:
1. Refinamentos por seção (catálogos completos nomeados: monstros 60, itens ~100, quests
   ~80 com XP/recompensa/conexões, tabela de skills com movimentação, balance tables).
2. Emendas às fable existentes + novas fable_21-31.
3. Atualização do SPEC_SOURCE_MAP com os novos docs canonizados.
```

**Responda neste próprio arquivo (edite) ou em mensagem — eu consolido.**
