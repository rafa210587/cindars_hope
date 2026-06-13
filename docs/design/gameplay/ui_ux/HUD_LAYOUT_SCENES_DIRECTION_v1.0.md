# Cindar's Hope — HUD Layout & Scenes Direction v1.1

> **Status:** documento canônico de layout de HUD, menus e dimensões de cena
> **Local:** `docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md`
> **Complementa:** `UI_UX_FULL_GAMEPLAY_DIRECTION.md` e `UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> (princípios, input routing, fluxos — VENCEM em regra de UX)
> **Depende de:**
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md` (§11-12: minimapa no v1, abas, tamanhos)
> - `COMBAT_CORE_DIRECTION.md` (o que o HUD precisa comunicar)
> **Função:** fixar ONDE e COM QUE TAMANHO cada elemento vive — HUD, minimapa, menus em
> abas, cenas (Town/Farm/Cave), lotes compráveis e a escala física dos objetos do mundo.
> **Não é spec implementável.**

---

## 0. Regra-mãe: responsividade

```text
A referência de design é 1280×720, mas NENHUMA posição é fixada em pixels absolutos:
todo elemento ancora em % da tela via CanvasScaler (ScaleWithScreenSize).
Caixas de diálogo: largura 60-80% da tela, ancoradas embaixo, fonte escalável.
Quem escrever spec de UI copiando pixels sem âncora está violando esta direction.
```

---

# PARTE A — O HUD de gameplay

## 1. O mapa da tela

A tela tem quatro cantos e um palco. O canto superior esquerdo é o CORPO (vitais); o
superior direito é o MUNDO (tempo e lugar); a base é a MÃO (ações); a lateral direita é a
MEMÓRIA (objetivo atual). O centro fica vazio — o palco pertence ao jogo.

```text
┌────────────────────────────────────────────────────────┐
│ HP/STA/MP        (vazio)              relógio/clima/lua │
│ fome·fadiga                            MINIMAPA 180px   │
│ status icons                           título de classe │
│                                        tracker de quest │
│                                                         │
│              [boss bar quando ativa]                    │
│                 toasts (máx 3)                          │
│        hotbar 10×48px   skills 4×56px                   │
└────────────────────────────────────────────────────────┘
```

## 2. Especificação por elemento (referência 720p)

| Elemento | Âncora | Tamanho ref. | Conteúdo/regra |
|---|---|---|---|
| Barras vitais | sup-esq (2%,2%) | HP 240×20 · STA 240×16 · MP 240×16, gap 4px | MP só aparece com magia conhecida/equipada |
| Fome/fadiga | sob as barras | ícones 24px + anel de preenchimento | piscam no threshold crítico |
| Status | sob fome/fadiga | fila de ícones 24px | tooltip no hover |
| Relógio/data/clima/lua | sup-dir (98%,2%) | 220×64 | widget da F20 |
| **Minimapa** | sup-dir, sob o relógio (98%,12%) | **180×180** | decisão Q11.3: NO V1 — caverna com fog of war por célula visitada (baú descoberto = ponto dourado; saídas = setas); town/farm = planta fixa com o player |
| Título de classe inferida | sob o minimapa | texto pequeno | ex.: "Lâmina Arcana" — atualiza ao dormir |
| Tracker de quest | dir (98%,38%) | 300px largura, 2 linhas | colapsável; 1 quest tracked |
| Hotbar | centro-inf (50%,96%) | 10 slots de 48px | números 1-0 visíveis |
| Active skills | à direita da hotbar | 4 slots de 56px | tecla + cooldown radial |
| Toasts | acima da hotbar | fila de máx 3 | prioridade do GameplayFeedbackService |
| Boss bar | topo-centro (50%,6%) | 480×24 | nome + fase; só durante boss |

## 3. O minimapa em detalhe (entra no v1)

```text
Caverna: grid de células (1 célula = 2×2 tiles); revela por presença; persiste por nível
DENTRO da run (stable run: revisitar mostra o que já viu). Ícones: player (seta), saídas,
baú visto, mercador errante encontrado, hazard visto. NÃO mostra inimigos (leitura é do palco).
Town/Farm: planta fixa simplificada (prédios como blocos), player e NPCs com "!" apenas.
Tecla M: abre o mapa grande na aba Mapa do menu.
```

---

# PARTE B — Menus

## 4. Um painel, oito abas (decisão Q11.2)

```text
Tab (ou I/K/U/J/C/M direto) abre o PAINEL ÚNICO: 90% × 85% da tela, centralizado.
Abas: Inventário · Equipamento · Skills · Quests · Bestiário · Calendário · Mapa · Sistema.
Tab/Shift+Tab ciclam abas; Esc fecha. Atalhos diretos abrem já na aba certa.
Construção: padrão F14 (RuntimeUiBuilder + UiFocusController) — navegável 100% por teclado.
Regras canônicas preservadas: detail antes de gastar ponto; compare nunca equipa por hover;
empty states explícitos; spoiler rules no Bestiário/Calendário.
```

---

# PARTE C — As cenas e suas medidas

## 5. Tamanhos de cena (decisão Q12.1)

| Cena | Antes | AGORA | Notas |
|---|---|---|---|
| Town | 36×30 | **48×42** | re-layout: anel viário interno, praça central 12×12, distritos com respiro |
| Farm | 26×20 | 26×20 **+ lotes** | lote_norte +10×20 · lote_leste +12×14 · lote_sul +10×20 — comprados via alvará do Tovin (`item_key_lot_deed_*`); cercas geradas fechadas que ABREM na compra |
| Cave (base) | ~40×40 | **55×55** | nível padrão |
| Cave (pequeno) | — | **42×42** | ~20% dos níveis (oscilação determinística por seed) |
| Cave (grande) | — | **65×65** | ~20% dos níveis |
| Interiores | — | 8×6 a 12×9 | cenas deslocadas na MESMA cena Unity (offset y>+40) |

```text
Regra de densidade: a contagem de inimigos por nível re-escala pela área:
count_final = count_base × (área_do_nível / 3025). Emenda obrigatória da F09.
```

## 6. O chão da fazenda (decisão Q12.2 — posições aprovadas)

```text
Casa + cama: SW (-5,-7)        Fonte de Anya: ao lado da casa (-3.5,-7) ⚠ F17
Campos: centro (24 canteiros)  Celeiro/Coop: N (Zone_Construction)
Lago: SE                       Árvores: E       Rochas: NW
Entrada da caverna: W (-5.5,0) Saída p/ cidade: SW        Shipping: N
Lotes compráveis: bordas N / E / S (cercas fechadas até a compra).
```

## 7. A escala do mundo (decisão Q12.3)

O jogador mede 1×1.5 tiles (sprite ~32×48px). Tudo se mede contra ele:

| Objeto | Tiles | Objeto | Tiles |
|---|---|---|---|
| NPC | 1×1.5 | casa | 4×3 |
| estalagem/casa grande | 6×4 | celeiro | 3×3 |
| coop | 2×2 | Fonte | 2×2 |
| estátua do guerreiro | 2×3 | barraca de mercado | 2×1.5 |
| árvore | 1×2 | canteiro | 1×1 |
| baú | 1×1 | porta | 1×1.5 |

Criaturas usam tamanhos naturais de fantasia (decisão Q12.3):

```text
Tiny 0.5×0.5 · Small 0.75×0.75 · Medium 1×1 · Large 2×2 · Huge 3×3 · Gargantuan 4×4
Miniboss: tamanho natural ×1.5 VISUAL. Boss: ×2.5 VISUAL.
Colliders SEMPRE pela size class (SPEC 13) — nunca pelo visual inflado.
```

---

# PARTE D — Decisões fechadas

```text
Tudo ancorado em %; diálogos responsivos 60-80% da largura.
Minimapa entra no v1 (180px, fog of war na caverna, sem inimigos no mapa).
Menus = painel único de 8 abas (Tab cicla), padrão F14.
Town 48×42 · Cave 55×55 com oscilação 42/65 determinística · Farm 26×20 + 3 lotes compráveis.
Interiores como cenas deslocadas (y>+40) na mesma cena Unity.
Tamanhos D&D naturais para criaturas; boss infla só o visual.
```

# PARTE E — Pendências

```text
Re-layout do gerador da Town para 48×42 (emenda do CreateMvpTownScene — posições novas).
Arte do minimapa (fase de arte; v1 usa blocos de cor).
Resoluções ultrawide: validar âncoras a 21:9 no playtest de UI.
Mapa-múndi (região de Dornécia) na aba Mapa — pós-v1.
```
