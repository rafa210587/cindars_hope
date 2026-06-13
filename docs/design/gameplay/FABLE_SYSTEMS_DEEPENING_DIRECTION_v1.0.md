# Cindar's Hope — Systems Deepening Direction v1.1 (FABLE)

> **Status:** PROPOSTA de direção — aguarda canonização humana.
> **Versão:** v1.1 (2026-06-12) — REESCRITA após leitura do corpus completo de `docs/design/**`.
> A v1.0 foi escrita a partir do SPEC_SOURCE_MAP + auditorias e propunha regras que o canon
> JÁ FECHAVA (às vezes em contradição). A v1.1 corrige: cita o que está definido, preenche
> só o que está genuinamente em aberto, e lista as correções explicitamente.
> **Função:** (A) síntese do jogo e do corpus de design; (B) gaps REAIS de definição com
> regras concretas propostas; (C) correções v1.0→v1.1; (D) mapa design vs. implementação.

---

# PARTE A — Resumo do jogo

**Cindar's Hope** = farm sim × action-roguelike (referência de feeling: Children of Morta),
2D pixel art (player ~32×48px, mundo em tiles), em Vaalara → Dornécia → vila rural de
Cindar's Hope. Pilar de tom: **pastoral acima, horror abaixo**. O jogador (personagem livre,
build por atributos+skills+gear) herda uma fazenda ao lado da **Fonte de Anya** — deusa
publicamente morta, na verdade fragmentada. Loop diário 06:00→02:00 (colapso): fazenda
(crops/animais/craft, deusa cotidiana = Thandra) → cidade (23 NPCs nomeados com serviços,
schedules, templo de Kanthor, loja noturna de Nyx) → caverna de 101 níveis (7 bandas de
bioma, estável por seed/run, packs densos, bosses com gates) → dormir. Meta-arco: 4 atos
recuperando fragmentos de Anya (Água/Memória/Vida/Esperança) que evoluem a Fonte (respawn →
Água Viva → respec → purificação) até a escolha final Proteger/Selar/Usar no 100/101
(Arquivista do Silêncio). Três luas mecânicas (Alihana=sonhos/raros, Senya=caos/magia,
Nyx=noite/segredos). Mana = fruto sagrado consciente, evento de endgame, nunca crop.
Camadas profundas: ruínas de Bromécia (tecnologia perigosa), portais de Elyndor
(Arcos Estelares/checkpoints), Pedra Negra estabilizada (Elyndor) ≠ Pedra Negra cultista
(drena almas), cultos infiltrados.

---

# PARTE B — Mapa de síntese: o que o corpus JÁ define (com âncoras numéricas)

| Área | Doc canônico | Já fechado (amostra das regras concretas) |
|---|---|---|
| Combate core | COMBAT_CORE | Stamina: light espada 25 / heavy 40 / dash 40 / dodge 40 / block 18/s; velocidades 3.8-4.2 tiles/s (3.4-3.8 em combate); dash 3.2-4.0 tiles SEM i-frame (cap 8 c/ build); dodge 1.2-1.8 tiles, i-frame 0.16-0.24s; movimento durante ações (tabela %); input buffers 0.08-0.20s; **crit normal por chance + janelas (MinorOpening/CriticalWindow/CoreExposed) podem garantir**; Perfect Block como skill futura |
| Armas/baselines | EQUIPMENT_MECHANICAL_BASELINES | matriz completa de 8 tipos (Sword 12dmg/1.20ASPD/FOR70-DES30 ... Dagger 7/1.75/DES80); AttackActionMultiplier (heavy ×1.45, charged longo ×1.90); **charged effect POR ARMA** (Sword=Aparar 8-12%, Axe=Bleed 18-28%, Hammer=Knockback/ArmorCracked, Spear=Impale, Dagger=FocusedCrit, Bow=Mark 20-35%, Staff=ArcaneChannel); 12 materiais (Mithril -30% peso, Prata anti-espiritual, Pedra Negra estabilizada +18%/risco); 10 flechas (4 elementais via óleo/craft); fórmula de AttackDamage completa |
| Aplicação elemental | idem + EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER | **já existe canonicamente como TAGS**: FireOil/FrostOil/ShockOil/PurifyingOil/PoisonCoating/BleedEdge (óleos = consumíveis temporários), flechas elementais, materiais (Silver/ArcaneCrystal/Blackstone), wands elementais; bônus SÓ se o inimigo declarar vulnerabilidade |
| Itens mágicos | idem | 6 wands (cargas+MP), 7 scrolls (consumíveis), tomes (unlock persistente), 5 focuses offhand |
| Armadura/escudo | idem | 5 armors (Light 6/0% ... Heavy 20/-10% move) com mods de dash/dodge/regen; 4 escudos (Buckler=perfect block ... Tower=tank) |
| Skills | PLAYER_SKILL_TREES | **catálogo COMPLETO**: 5 árvores × ~14 skills nomeadas c/ tiers (0/5/11/18/26 pts), ranks 1-5 c/ cap por tier, valores por rank, 50 pontos máx (1/2 níveis), capstones exclusivos (Kanthor×Kaand, Anya×Senya), Três Luas, Telisandra, Thoren; respec só na Fonte |
| Atributos | PLAYER_DERIVED_ATTRIBUTES (+TABLETOP) | fórmulas canônicas de HP/MP/Stamina/Regen/BlockImpact/Defense/AttackDamage (FOR/DES/CON/INT/VON) — vence qualquer outro doc em fórmula |
| Status | STATUS_EFFECTS + COMBAT_CORE | 15 status permitidos (incl. Slow≠Chill, HeatStress/ColdStress, Corruption); significado mecânico canônico; sem controle permanente |
| Inimigos | ENEMY_BEHAVIORS + CAVE_MONSTER_ROSTER (+ADAPTER) | roster de ~40 criaturas concretas (stats/drops/packs/bosses/traits); **22 Moves oficiais** com faixas de velocidade (PackFlanker, RetreatAndCall, CircleStrafe, ChargeLine, HazardLure, ProtectAnchor, TreasureIdleAmbush, BossArenaControl...); elites = 2+ ações + janela clara, não HP inflado |
| Vulnerabilidades | CAVE_COMBAT_BALANCE | matriz por família, janelas, TTK targets, active enemy budget, multiplicadores |
| Bestiário | BESTIARY_KNOWLEDGE_DISCOVERY | EnemyKnowledgeState por categoria de conhecimento; fontes (combate/NPC/livro/quest/ruína); **SpoilerTier 0-4** (Safe→Endgame) gateando UI; save/load contract; tooltips só mostram conhecido |
| NPCs | CITY_NPC_ROSTER v1.1 | **seção individual por NPC (23)**: classe funcional, tags de serviço, stats, religião pessoal, casais fixos, candidatos a romance; serviços por NPC já direcionados |
| Cidade | CITY_DESIGN v1.2 + CITY_LAYOUT_BUILDINGS_SCHEDULE | prédios/portas/camas/waypoints/schedules por hora; templo Kanthor; loja noturna Nyx |
| Economia | LOOT_CRAFTING_ECONOMY + ECONOMY_PRICING | BaseValue obrigatório, anti-arbitragem (SellToPlayer > BuyFromPlayer), restock policies, SellPoint no day transition, Quality≠Rarity≠Tier, boss first/repeat |
| Mundo/tempo | SEASONS_CALENDAR_WEATHER_LUNAR | 4 estações × 28 dias, semana de 7, 1h=60s, colapso 02:00, luas mecânicas, festivais |
| Save | SAVE_LOAD_FULL_STATE | seções/owners/restore order/migrations/IDs simples |
| Social | SOCIAL_RELATIONSHIP_ROMANCE | friendship/trust/gift/romance/poliamor(≤3) — **FUTURO por decisão de roadmap** |
| Companions/Pets | COMPANIONS + PETS | sistemas completos — futuros (pets HOLD) |
| Magia | MAGIC_SPELLS_ACTIONS + MAGIC_LEARNING | lista de spells, shapes, MP/cast, fontes de aprendizado (LearnableScroll/CastScroll/Tome/Wand/NPC/Fonte) |
| Quests | QUEST_OBJECTIVE_EVENT + QUESTS_MAIN_* | sistema genérico + 4 atos + finais + Litania/Arquivista |

**Conclusão central:** o jogo está *desenhado*; está pouco *ligado* (auditoria fable_00B) e
pouco *autorado em dados concretos finais*. Os gaps de definição reais são os da Parte C.

---

# PARTE C — Gaps REAIS de definição (pendências do próprio canon + buracos) com regras propostas

> Tudo abaixo respeita as fontes acima; nada redefine o que está fechado.

## C1. Bestiário — thresholds numéricos de descoberta (canon define o modelo, não os números)

```text
Por categoria de conhecimento (EnemyKnowledgeState), gatilho de descoberta por combate:
- Identificação (nome/família): 1 avistamento.
- BehaviorObserved: ver a ação 3× OU sofrer a ação 1×.
- ElementVulnerability/StatusVulnerability: acertar o elemento/status efetivo 3× no inimigo
  (feedback "efetivo!") OU fonte externa (livro/NPC/quest).
- Drops comuns: 5 kills; drops raros: revelados só por fonte externa OU ao dropar.
- Resistências/imunidades: sofrer "resistido" 3× OU fonte externa.
Fontes externas SEMPRE existem por entrada (regra anti-grind): análise da Thalindra
(1/dia grátis), livros (Yael/arquivo, 150-400g), quests de pesquisa.
SpoilerTier do canon continua mandando na UI (não revelar acima do tier autorizado).
```

## C2. Acessórios — catálogo concreto (canon define tipos/classes de efeito, não itens)

Tipos canônicos: Ring, Amulet, Charm, Relic (+Belt/Totem futuros). Regra canônica: bônus
condicional/especializado, nunca amplo. Proposta de catálogo inicial (12, dentro das classes
de efeito permitidas):

| Item | Tipo | Efeito (classe canônica) | Fonte |
|---|---|---|---|
| Anel de Thoren | Ring | +15% durabilidade de ferramentas | Brumdar 400g |
| Anel de Finan | Ring | +5% ouro em vendas | Renko 600g |
| Anel de Alihana | Ring | +10% chance de 1 roll extra de loot raro | tesouro 20+ |
| Anel da Corrente | Ring | -10% custo de Stamina de dash/dodge (classe "menor cansaço em run") | Zrix 500g |
| Anel de Raiz | Ring | resiste Root; Chill -50% duração (resistência específica) | Savra 450g |
| Anel de Brasa | Ring | +resistência Fire/Heat específica | tesouro fire band |
| Amuleto de Anya | Amulet | +melhor cura recebida (+25%) | quest Ato 1 |
| Amuleto de Kanthor | Amulet | +Block Stability +10% (relíquia divina canônica) | templo 700g |
| Amuleto de Senya | Amulet | +afinidade elemental (+10% magic dmg / +10% custo MP) | Ozzra 650g |
| Amuleto de Nyx | Amulet | -30% cansaço noturno; detecção de treasure trap futura | mercador errante |
| Charm de Thandra | Charm | +melhor efeito de comida (+15%) | Eiran 600g |
| Charm de Pedra | Charm | -50% knockback recebido; -5% move | Dagna 350g |

Regras: 1 Ring + 1 Amulet + 1 Charm (3 slots, não 2 — alinhado aos tipos canônicos);
efeitos via DerivedStats/provider; mesmo efeito não stacka entre slots.

## C3. Forja elemental — POSIÇÃO CORRIGIDA

O canon JÁ resolve aplicação elemental: **óleos (temporário, consumível), flechas elementais,
materiais e wands**. Proposta v1.1 (aditiva, não substitutiva): **Têmpera de Essência** na
forja do Brumdar = versão PERMANENTE e exclusiva dos óleos, gated:

```text
- 6 essências por banda de bioma (drop 8% comum / 100% miniboss da banda);
- Têmpera aplica a TAG do óleo correspondente permanentemente (FireOil→FireEdge etc.),
  1 elemento por arma, re-temperar substitui;
- custo: 3 essências + 150g (T1) / 6 essências + Eco do Vazio + 600g (T2 = +chance de status);
- continua valendo a regra-mãe do adapter: bônus SÓ contra vulnerabilidade declarada;
- óleo aplicado SOBREPÕE têmpera temporariamente (óleo continua tendo nicho);
- ferramentas: só T1 (efeitos utilitários: machado fire = +carvão).
DECISÃO HUMANA NECESSÁRIA: aprovar têmpera permanente (poder acima do baseline canônico de óleos).
```

## C4. Skills — CORREÇÃO: não remodelar; IMPLEMENTAR o catálogo canônico

A v1.0 propôs "5×3×7=105 nodes" — **cancelado**: PLAYER_SKILL_TREES já define ~70 skills
nomeadas com ranks/tiers/capstones. Gap real = código (DefaultSkillCatalog ad-hoc com 69
nodes que NÃO correspondem ao canônico). Regra proposta para a migração (única coisa em aberto):

```text
- Substituir DefaultSkillCatalog pelo catálogo canônico 1:1 (IDs skill_<arvore>_<slug>);
- crosswalk dos 69 antigos → canônicos (mapear por semântica; sem equivalente = refund);
- respec automático 1× no load (pontos devolvidos), invalid-id fallback WAVE 01;
- pendências numéricas do próprio canon (custos finais de dash/dodge/regen por rank de
  Passo de Impulso/Ritmo Controlado): preencher com interpolação linear entre os baselines
  fechados do COMBAT_CORE (ex.: Ritmo Controlado rank 5 = dash 40→30 stamina).
```

## C5. NPCs — usar o roster v1.1, preencher só o delta mecânico

O roster já define classe funcional/serviços/stats/religião POR NPC (seções 7-29).
Gap real: serviços direcionais sem REGRA numérica. Proposta: autorar a tabela
serviço→{custo, frequência, efeito}, derivada das seções individuais do roster — ex.:
bênção de Corvus (50g, 1/dia, +10% stamina max OU -20% fadiga), análise da Thalindra
(grátis 1/dia, bestiário C1), rumor de Orlan (1 grátis/dia, 50% verificável — regra de
diálogo), seguro de run da Mara (100g, conserva 50% do ouro, 1 run), treino de Alaric
(+5 HP máx por marco 100/300/600 kills, 3×). **Cada número deve ser validado contra a seção
do NPC no roster antes da spec** (o roster vence em identidade/serviço).

## C6. Amizade — CORREÇÃO de escopo

Social/romance é FUTURO por decisão de roadmap (SPEC_SOURCE_MAP). A v1.0 propôs amizade
funcional — reclassificada: **apenas preparar `FriendshipState` (contrato/save, sem UI nem
recompensas)** quando o humano decidir; recompensas/corações implementam-se na promoção da
WAVE 17. Diálogo condicional por flag/hora/lua (C7) NÃO depende de amizade.

## C7. Diálogos — regras aditivas (não cobertas por nenhum doc)

```text
1. DialogueNode.Requirement (string parseável: "flag:<id>", "hour:18-02", "moon:nyx",
   "quest:<id>:completed") — gate avaliado no NpcController/NpcShopController.
2. Pool contextual mínimo: 4 linhas/NPC reagindo a estado do mundo (chuva, festival,
   boss derrotado, fragmento obtido) — via flags/eventos existentes.
3. Rumor 50% útil verificável / 50% cor (nunca 100% cor).
4. Todo ramo de serviço termina em ação mecânica real (regra anti-beco).
5. Memória curta: 1 linha referenciando a última quest concluída para o NPC (flag).
```

## C8. Inimigos — afixos de elite (canon pede "elite ≠ HP inflado"; afixos não autorados)

Mantida da v1.0 (compatível com canon): 1 afixo determinístico por
`StableHash(instanceId|"affix") % 6` — Ardente/Gélido/Férreo/Voraz/Chamador/Lunar (Lunar
usa a lua ativa do calendário). Elite continua exigindo 2+ ações e janela clara (canon).
Gap adicional REAL: **12 dos 22 Moves oficiais não existem no enum do código**
(PackFlanker, PackLeader, RetreatAndCall, FloatingSlow, FloatingOrbit, CircleStrafe,
ChargeLine, TreasureIdleAmbush, HazardLure, ProtectAnchor, BossArenaControl, BossPhaseShift)
— gap de implementação com semântica JÁ definida (faixas de velocidade no COMBAT_CORE §16).

## C9. Movimentos — CORREÇÃO e delta

Valores canônicos vencem minha tabela v1.0 (dash sem i-frame; dodge i-frame 0.16-0.24s;
cooldowns 0.75-1.2s / 0.45-0.9s). "Parry" da v1.0 → **Perfect Block** (canon §23: skill
Contra-Ataque já existe na árvore Melee; Sword charged = Aparar já é o parry de arma).
Sprint: NÃO existe no canon — proposta nova mínima: correr = manter velocidade de exploração
(3.8-4.2) DENTRO de combate ao custo de 8 stamina/s (combate força 3.4-3.8 por padrão) —
decisão humana necessária.

## C10. Catálogos — governança (gap real; nenhum doc fecha naming/validação)

Mantida da v1.0, ajustada: IDs `item_<categoria>_<slug>` (categoria ∈ 12 valores);
BaseValue>0 obrigatório (fórmula de referência: insumos×1.6 + 2×tier de banda);
Tier≠Quality(×1/1.25/1.5/2)≠Rarity(pesos 100/40/12/3); item órfão (sem fonte E sem uso) =
ERRO de validator; todo item novo só via gerador/initializer.

## C11. Pendências do canon que exigem DECISÃO HUMANA (não regra técnica)

```text
Lore: aparência dos Nymirianos; natureza da Fonte (artefato/milagre/híbrido); local da
Raiz de Mana; os 3 chefes pós-100; origem da Pedra Negra cultista; quais deuses têm templo.
Sistema: aprovar Têmpera de Essência (C3); aprovar corrida em combate (C9); 3º slot de
acessório Charm (C2); calendário de festivais concreto (datas por estação).
```

---

# PARTE D — Correções explícitas v1.0 → v1.1

```text
1. Crítico: v1.0 "sem RNG" → ERRADO. Canon: chance normal existe (Destreza/condicional);
   janelas garantem. Specs F02/F06 devem seguir o canon.
2. Matriz de armas v1.0 (dano 6-20/stamina 6-20) → SUBSTITUÍDA pela matriz canônica
   (Sword 12/25STA etc.). F02/F03 devem ser EMENDADAS para usar EQUIPMENT_MECHANICAL_BASELINES.
3. Forja Elemental v1.0 (conversão de dano por essência) → reposicionada como Têmpera
   (tags de óleo permanentes, aditiva ao sistema canônico de óleos/flechas/materiais).
4. Skills remodel 5×3×7 → CANCELADO; implementar catálogo canônico (C4).
5. Tabela de NPCs v1.0 → subordinada ao roster v1.1 (validar número a número).
6. Amizade 5♥ funcional → contrato apenas (social é futuro).
7. Parry novo → Perfect Block/Aparar canônicos. Cooldowns de dash/dodge corrigidos.
8. Acessórios 2 slots → 3 (Ring/Amulet/Charm, tipos canônicos).
9. Famílias de inimigos v1.0 (6) → manter como AGRUPAMENTO de implementação, mas stats/
   vulnerabilidades por família vêm de CAVE_COMBAT_BALANCE + roster (não desta proposta).
10. Status v1.0 (10) → lista canônica tem 15 (inclui Slow≠Chill, Heat/ColdStress) —
    emendar fable_01.
```

---

# PARTE E — Mapa design → implementação (onde cada gap fecha)

| Gap | Tipo | Destino |
|---|---|---|
| C1 thresholds bestiário | definição→spec | emenda às specs WAVE 13 + fable_21 |
| C2 catálogo de acessórios | definição→spec | fable_23 (3 slots + 12 itens + gerador) |
| C3 Têmpera de Essência | DECISÃO + spec | fable_22 (pós-aprovação; depende F03/F06) |
| C4 catálogo canônico de skills | implementação | fable_29 (migração/crosswalk/refund) |
| C5 números de serviços NPC | definição→spec | fable_25 (validada contra roster §7-29) |
| C6 FriendshipState contrato | contrato | fable_26 (CONTRACT_ONLY até WAVE 17) |
| C7 diálogo condicional | definição→spec | fable_28 |
| C8 afixos + 12 Moves faltantes | implementação | fable_24 (afixos) + emenda F04/F05 (Moves) |
| C9 corrida em combate | DECISÃO + spec | fable_27 (pós-aprovação) |
| C10 governança de catálogo | definição→spec | fable_30 (validator) |
| Emendas F01 (15 status), F02/F03 (matriz canônica, crit), F06 (crit/janelas) | correção | editar specs fable existentes antes de executar |
| 20 campos WeaponDataSO, flechas, óleos, wands/scrolls/tomes/focuses, armors/shields | implementação pura (canon completo) | fable_31 (itens mágicos) + emendas F03 |

Ordem pós-corretivas (F15-F17 primeiro, inalterada): emendas F01-F06 → F22-F31 conforme decisões da Parte C11.
