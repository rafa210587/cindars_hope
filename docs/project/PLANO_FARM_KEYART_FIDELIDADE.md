# Plano DENSO — Fidelidade da FarmScene à Keyart

> Continuação do `PLANO_FARM_KEYART_ALINHAMENTO.md`. Alvo: `docs/art_catalog/_reference_keyart_GPT/
> farm_keyart_layout_aprovado_v1.png`. Objetivo: fechar o **gap de ESTILO** (não só de layout).
> Criado 2026-08-14. Atualizar o log de progresso ao fim de cada fase.

## Diagnóstico central — por que ainda "não parece a keyart"
O layout/posições já batem. O que falta é o que dá a **cara de pintura**:

1. **BORDAS ORGÂNICAS.** Na keyart, grama↔terra↔água têm transições suaves e irregulares (a terra
   "mordida" na grama, a água com margem de pedra recortada). No jogo são **retângulos de tile com
   borda dura** → parece grade/blocos. **Este é o maior lever técnico** (precisa de autotile/rule-tiles
   com peças de canto/lado).
2. **DENSIDADE de decoração.** A keyart é **densa** de mini-detalhes: flores, trevos, cogumelos
   vermelhos, tocos, pedrinhas, tufos de grama, juncos, vitórias-régias. O jogo tem quase nada. **Maior
   lever visual** (camada de scatter denso).
3. **CONTEÚDO faltante.** Campo de cultivo central arado com fileiras; borda de PEDRA no lago (não
   areia) + juncos/lírios/píer/barco/cascata; muralha de rocha com VOLUME; poço; banca com toldo;
   cercados de animais (galinheiro com cerca+galinhas); floresta densa (tocos/cogumelos/berry/macieira).

## Tabela de gaps (keyart → atual → correção)
| Elemento | Keyart | Atual | Correção (fase) |
|---|---|---|---|
| Caminhos | orgânicos, curvos, borda suave "mordida" | retângulos retos, borda dura | autotile grama↔terra (R1) |
| Chão grama | rico, blendado, flores/trevo espalhados | mottled mas quadriculado, pouca decoração | scatter denso + tune peso (R2) |
| Lago | orgânico, **borda de pedra**, juncos, lírios, píer+barco | blob retangular, margem de areia | footprint orgânico + rock border + props (R3) |
| Rio | margem de pedra + **cascata** na nascente | faixa azul + margem areia | rock border + waterfall (R3) |
| Cultivo | bloco central arado c/ **fileiras** de culturas | plots pouco visíveis | campo arado central + crops (R4) |
| Montanha | muralha alta de rocha c/ volume + props na base | faixa rochosa chapada | muralha com sombra + hay/ore/boulders (R5) |
| Floresta O | densa: pinheiros+macieira+tocos+cogumelos+berry | árvores esparsas | adensar em camadas (R6) |
| Fonte Anya | fonte ornada, anel de pedra, clareira, flores | sprite pequeno | clareira + anel + flores (R6) |
| Homestead | poço, banca c/ **toldo**, cercado ao lado da casa | falta poço/toldo/cercado | props extras (R7) |
| Animais | galinheiro c/ **cerca+galinhas**, feno | prédios sem cercado | pens + galinhas + feno (R7) |
| Bordas do mapa | naturais (floresta/rocha/água) | treeline ok, mas leste tem trilho residual | revisar (R7) |

## Os 2 grandes levers (fazer primeiro)
- **LEVER A — Autotile de bordas (R1):** peças de transição (9-slice/blob: 4 lados + 4 cantos + centro,
  interno e externo) para grama↔terra e para a margem de água (pedra). Implementar suporte a
  RuleTile/pintura-por-vizinhança no `WorldTilemapGround` (checar vizinhos e pintar a peça de borda
  correta). É o que troca "blocos" por "formas orgânicas".
- **LEVER B — Decoração densa (R2):** camada `Decoration` com **scatter determinístico DENSO** de
  sprites pequenos (flores, tufos, trevo, cogumelo, pedrinha) sobre grama/floresta, + juncos/lírios na
  água. Blenda o quadriculado e enche o vazio.

## Fases (ordenadas por impacto de estilo)

### R2 — Decoração densa  *(começar por aqui: maior salto, mais rápido)*
- Gerar sheet(s) de decoração (props isolados, fundo transparente): flores (moitas de flor branca/
  amarela), tufo de grama alto, trevo, cogumelo vermelho, pedrinha, galho/folha caída.
- `WorldTilemapGround`/novo helper `ScatterDecoration(layer, sprites[], weights[], density, region)` —
  camada de sprites (não tilemap) espalhados por hash determinístico, densidade alta na grama, evitando
  pisar em prédios/água/caminho (respeitar máscara de ocupação simples por retângulos).
- Reduzir peso de `ground_grass_a` (sombreada) no `PaintGrass` (menos manchado).

### R1 — Bordas orgânicas (autotile)
- Gerar tiles de transição: `ground_dirt_edge_*` (grama↔terra: lados N/S/L/O + 4 cantos externos +
  4 internos), e `ground_water_rock_edge_*` (margem de pedra da água).
- `WorldTilemapGround`: suporte a autotile — ao pintar terra/água, pintar a peça de borda certa pela
  vizinhança (grama em volta). Repintar caminhos e água com bordas suaves.

### R3 — Água orgânica
- Footprint do lago como **blob orgânico** (vários círculos/óvalos sobrepostos, não retângulos).
- **Borda de pedra** (boulders) ao redor via autotile (R1) ou props de rocha na margem.
- Props na/na beira da água: **juncos/cattails**, **vitórias-régias (lily pads)**, **píer** (deck) +
  **barco a remo**; **cascata** na nascente do rio (topo).

### R4 — Campo de cultivo central
- Bloco central de **solo arado** (`ground_soil` tiled) com **fileiras de culturas** visíveis (reusar
  `crops/crop_*`/`seed_*` ou plantar sprites de cultura em grade). Cerca de madeira em volta.
- Posicionar no centro (~(1,-2) já é onde os FarmPlots ficam) e deixá-lo PROEMINENTE.

### R5 — Muralha de rocha com volume
- Regenerar/ajustar a muralha pra ter **profundidade/sombra** (não faixa chapada); base com **fardos de
  feno**, **ore chunks** (minério brilhante), **boulders**. Cave entrance + contract board (já existem).

### R6 — Floresta densa (oeste) + Fonte
- Adensar o bosque O: mais **pinheiros** + **macieiras** (com fruta) + **tocos** + **cogumelos** +
  **arbustos de berry**, em camadas com Y-sort. Fonte da Anya numa **clareira** com anel de pedra + flores.

### R7 — Detalhes do homestead + animais + bordas
- **Poço** (well) decorativo central. **Banca de venda com toldo** (awning). **Cercado** (fence pen)
  ao lado da casa. Cercados de **galinheiro** (cerca + galinhas + feno). Rever borda leste (trilho
  residual). Barris/detalhes ao redor dos prédios.

### R8 — Rede de caminhos orgânica
- Repintar a rede de caminhos com as bordas orgânicas (R1), curvando entre os marcos como na keyart
  (poço↔casa↔crafts↔cultivo↔animais↔caverna↔lago).

## Arte a gerar (consolidado)
1. Sheet **decoração** (R2): flores, tufo, trevo, cogumelo, pedrinha, galho.
2. Sheet **bordas terra** (R1): 9-slice grama↔terra (lados+cantos ext/int).
3. Sheet **bordas água/pedra** (R1/R3): margem de pedra 9-slice + boulders.
4. Sheet **água props** (R3): juncos, cattails, lily pads, cascata (frames), (píer/barco já existem? gerar se não).
5. Sheet **cultivo** (R4): se `crops/*` não bastarem, gerar fileiras de culturas (cabbage/wheat/etc.).
6. Sheet **floresta props** (R6): tocos variados, cogumelos, arbusto berry, macieira c/ fruta (se faltar).
7. Sheet **homestead props** (R7): poço, banca c/ toldo, cerca (peças), fardo de feno, galinha.

## Como executar (cada fase)
Gerar arte (ChatGPT, Chrome logado) → fatiar/reseam/downscale/importar → wirar (subagent Sonnet) →
**regen batchmode + captura (full+homestead+animais)** → **comparar com a keyart** → iterar → commitar.
Regra: validar SEMPRE contra a keyart; nunca dar por bom sem captura comparada.

## Ordem recomendada
**R2 → R1 → R3 → R4 → R6 → R5 → R7 → R8.** (Decoração + bordas orgânicas primeiro = maior salto de
estilo; depois água, cultivo, floresta, montanha, detalhes, caminhos.) É um esforço de **várias sessões**
(≈7 sheets de arte + vários passes de wiring). Cada fase entrega um salto visível.

## PROGRESSO
- **2026-08-14:** plano criado. Base atual: grama rica, treeline, montanha faixa, água c/ margem de areia,
  caminhos retos, crafts em pátio, rio movido a oeste (casa com respiro). **PRÓXIMO:** R2 (decoração densa).
