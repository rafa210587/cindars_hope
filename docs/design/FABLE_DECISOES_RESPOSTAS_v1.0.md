# Cindar's Hope — Registro de Decisões do Questionário FABLE (v1.0)

> **Status:** DECISÕES HUMANAS FECHADAS (2026-06-12) — respostas ao FABLE_QUESTIONARIO_DECISOES_v1.0.
> **Função:** fonte vinculante para os catálogos/refinamentos gerados a partir daqui.
> Onde o humano deu latitude ("como achar melhor"), a escolha resolvida está marcada [RESOLVIDO].

## 1. Lore e clima

- **Raças**: vila MISTA; raças malvistas (goblins/orcs etc.) não residem na cidade. Goblin
  visitante ocasional na FAZENDA para diálogo + quest em algum ato. [RESOLVIDO: tabela de
  raça por NPC será proposta no catálogo de NPCs — humanos maioria, Brumdar/Dagna anões,
  Nimble gnomo, Pip halfling, Zrix draconato civilizado, Ozzra tiefling, Sylveth/Liora elfas.]
- **Religião**: politeísta — Kanthor (templo), Thandra (culto rural), elementos de Merithus,
  Finan e outros nos diálogos/serviços, expandindo o conhecimento de Vaalara.
- **Dragões**: SIM — dragão ancestral adormecido é o segredo do nível 101; wyrmlings na
  caverna; 1 boss de gate intermediário é um dragão pequeno.
- **Pedra Negra cultista**: meteoro de Elyndor corrompido por Veyraath (Contra-Criação),
  explicado AOS POUCOS (lore por fragmentos).
- **NPC Nymiriano**: existe, do sexo OPOSTO ao do jogador (romanceável), aparece TARDE
  (Ato 3+).
- **Fonte**: fragmento de Anya + artefato nymiriano (com marcações tribais nymirianas).
- **Chefes pós-100 (4)**: Guardião do Arco (construct) → Eco de Cindar corrompido →
  Arquivista do Silêncio → **Dragão Ancião** (4º/final verdadeiro). Nomes derivados das
  línguas de Vaalara (arquivos da mesa) — propostos no bestiário.

## 2. Monstros

- Renomear APENAS trademarks (Drow→Veilkin, Duergar→Gravedelver); goblin/kobold/orc/lich/
  elemental etc. são livres e PERMANECEM. Descrições completas estilo D&D permitidas (sem
  texto copiado). **Nomes em INGLÊS.**
- Catálogo: expandir 44 → **60** (avaliação aprovada), com stat block completo por criatura
  (características, comportamento, ataques, defesas, HP, variação de nível, movimentação).
- Gates: podem ter 2 bosses OU múltiplos minibosses; **mínimo 2 minibosses na caverna por
  gate-tier**; **níveis da caverna AUMENTAM de tamanho** (ver §12).

## 3. Itens

- Alvos aprovados: sementes 12, comidas/receitas 20, poções 8, óleos 4, flechas 6,
  materiais 10+essências 6, armas 16, armaduras 6, escudos 3, wands 3, scrolls 4, acessórios 12.
- **Qualidade**: [RESOLVIDO] crops/produtos animais têm variantes por qualidade como itens
  separados (sufixos _silver/_gold; 3 níveis: Normal/Silver/Gold) — simples, save-compatível.
- Nomes das 6 sementes novas aprovados (Lágrima-de-Alihana, Pimenta-de-Senya, Raiz-de-Sombra,
  Trigo-Dourado-de-Thandra, Abóbora-do-Vale, Uva-de-Brigandini → IDs em inglês).

## 4. Acessórios

- 3 slots (Ring/Amulet/Charm) + catálogo de 12 CONFIRMADO — **criar** (spec fable_23).
- Relíquias divinas: SIM (4º tipo, raras, 1 por deus maior, drop boss/quest) — fable futura.

## 5. Eventos

- Calendário de 8 festivais aprovado (datas da proposta).
- **Luas**: cada lua fica visível 8-10 dias por estação, com 1 noite de PICO (efeitos canon).
- Eventos aleatórios: VÁRIOS (corvos, meteoro→cristal arcano, visita, caravana, etc. — lista no catálogo de eventos).

## 6. Quests

- 4 fontes aprovadas (main "!", side por NPC, Quadro de Avisos do Hund 3/dia, contratos do
  Zrix) **+ 5ª fonte: quests SECRETAS na caverna** — entregues por monstro não-agressivo e
  pelos mercadores errantes (coleta de itens etc.).
- **XP/recompensas ESCALAM com o nível da quest** (não estáticas) — fórmula no doc de balance.
  Main quest: +1 skill point por ATO.
- Cadeias de 3 side-quests por NPC (12 NPCs no v1) — DEVEM nascer do propósito/lore do NPC
  (roster v1.1 vence).
- Conexões aprovadas (side referencia main; têmpera atrás do Ato 1; scrolls atrás da cadeia
  da Ozzra; dailies sem itens de quest compartilhados).
- UI: abas Main/Side/Contratos + tracker HUD + "!" + **organizar TODAS as HUDs em abas** e
  **caixas de diálogo responsivas ao tamanho da tela** (vai para spec F14/F20 emendas).

## 7-8. Movimentação

- Tabela de movimentação por skill ativa: GERAR (telegraph/lunge/shape/recovery).
- Skills com deslocamento NÃO atravessam inimigos; **apenas Dodge atravessa**.
- 12 Moves faltantes: implementar com faixas do COMBAT_CORE §16, atentos a balanceamento e
  ativação. Windup/recovery por papel aprovados (0.5/0.5 comum; 0.7/0.6 elite; 0.9-1.2/0.8 boss).
- Projétil inimigo 5-7 tiles/s aprovado.

## 9. Balanceamento

- **Level cap 100** (mantém canon: 1 skill point/2 níveis → 50 pontos) — curva de XP nova
  proposta no doc de balance (sincronizada com as 7 bandas da caverna: nível do player ≈
  nível da caverna como régua).
- Multiplicadores de HP/dano **POR TIPO de monstro** (não banda chapada) — tabela por
  família×papel no doc de balance.
- Tabela de validação de dano do player por banda: gerar.

## 10. Classes inferidas

- Modelo aprovado (maior investimento + uso de arma; recalcula ao dormir; híbridos).
- **As 3 recompensas juntas**: diálogos diferentes + bônus pequeno (+3% família dominante)
  + serviços especiais por classe. Anti-abuse: bônus só muda ao dormir.

## 11. HUD/Menus

- Layout 1280×720 aprovado + **MINIMAPA ENTRA NO V1** (canto sup-dir, abaixo do relógio;
  fog of war por células visitadas na caverna; planta fixa em town/farm).
- Menus em tela cheia com abas (Tab cicla).

## 12. Cenas

- **Town 48×42** (re-layout do gerador). **Cave base 55×55** com oscilação: níveis pequenos
  42×42, grandes 65×65 (emenda F09 + densidade recalibrada). **Farm mantém 26×20 com
  EXPANSÕES por compra de lotes** (alvará do Tovin — fases de lote: +N, +S, +E).
- Posições da farm aprovadas; boa folga de espaço.
- Tamanhos: baseline aprovado + **monstros usam tamanhos naturais D&D** (Tiny 0.5×0.5,
  Small 0.75, Medium 1×1, Large 2×2, Huge 3×3 em tiles) e miniboss/boss ganham acréscimo
  visual (×1.5 / ×2.5).
- Interiores: **cenas deslocadas na mesma cena Unity** (offset y>+40), conforme F11.

## 13. Futuros

- Ordem padrão mantida. Pets HOLD. Multiplayer FORA (reavaliar no futuro).
- **Romance**: usar os candidatos JÁ definidos no CITY_NPC_ROSTER v1.1 (§6) + opções
  bissexuais + **poliamor com 2 parceiros simultâneos** (ajuste ao canon que dizia até 3:
  DECISÃO NOVA = 2) + NPC nymiriano tardio romanceável.

## Documentos a gerar a partir destas decisões

```text
1. CAVE_BESTIARY_CATALOG_DIRECTION_v1.0 (60 criaturas + 4 chefes finais nomeados)
2. ITEM_CATALOG_DIRECTION_v1.0 (~110 itens nomeados com BaseValue)
3. QUEST_CATALOG_DIRECTION_v1.0 (~80 quests: fontes, XP escalado, recompensas, conexões)
4. SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0 (~20 ativas canônicas)
5. BALANCE_CURVES_DIRECTION_v1.0 (XP 1-100, HP/dano por tipo, TTK, dano esperado)
6. HUD_LAYOUT_SCENES_DIRECTION_v1.0 (layout px, minimapa, abas, cenas novas, lotes)
7. Atualizar SPEC_SOURCE_MAP + emendas fable (F09 tamanhos, F14 abas/responsivo, F02/F05) 
   + novas fable_21-31.
```
