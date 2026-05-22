# GDD v2.7 — Delta FASE 9C Tools/Farm/Combat

> Complementa `docs/design/GDD_v2.6.md`.
> Este delta registra a decisão de design para ferramentas, ações da fazenda e combate.

## 1. Nova prioridade de gameplay

A próxima fase do jogo deve transformar ações hoje funcionais, mas simplificadas, em sistemas com progressão.

Prioridade:

1. ferramentas com níveis;
2. ações de fazenda dependentes de ferramenta;
3. fallback limitado sem ferramenta correta;
4. armas equipáveis;
5. esquiva;
6. ataques corpo a corpo, distância física e magia à distância.

## 2. Filosofia de ferramentas

Ferramentas são progressão, não apenas chaves de interação.

Elas devem influenciar:

- acesso à ação;
- velocidade;
- yield;
- quantidade de hits;
- custo de fome/stamina;
- área de efeito;
- recursos desbloqueados.

## 3. Tiers de ferramenta

| Tier | Nome | Papel no design |
|---:|---|---|
| 0 | Improvised | fallback fraco, não substitui ferramenta real |
| 1 | Basic | libera ação mínima |
| 2 | Copper | melhora eficiência/yield |
| 3 | Iron | reduz hits/tempo e desbloqueia recursos médios |
| 4 | Steel | amplia área e eficiência |
| 5 | Arcane | interage com magia, luas e recursos especiais |

## 4. Ações da fazenda

### Plantio

O plantio deve deixar de ser automático por seed fixa.

Novo design:

- jogador escolhe seed explicitamente;
- Hoe melhora ou libera plantio;
- sem Hoe, apenas seed básica ou penalidade controlada;
- seeds avançadas exigem ferramenta apropriada.

### Colheita

A colheita deve diferenciar:

- mão;
- Sickle Basic;
- Sickle de tiers superiores.

Sem Sickle, crops simples podem ser colhidos com yield mínimo. Crops raros podem exigir ferramenta.

### Árvores

Cortar árvore deve exigir Axe para progresso real.

Sem Axe:

- pode haver fallback pequeno;
- o fallback não incrementa hits;
- a árvore não é derrubada.

Com Axe:

- aplica hits;
- tier aumenta ação/yield;
- tiers futuros podem cortar área.

### Pesca e mineração

Pesca deve usar `FishingRod` via sistema genérico de ferramenta.

Mineração futura deve usar `Pickaxe` com a mesma regra.

## 5. Combate

O combate atual com soco é placeholder funcional. A próxima evolução deve introduzir equipamento ofensivo.

Tipos obrigatórios:

| Tipo | Descrição |
|---|---|
| Corpo a corpo | espada, adaga, martelo, machado de combate |
| Distância física | arco, besta, sling |
| Magia à distância | cajado, varinha, foco arcano |

## 6. Esquiva

Adicionar esquiva para deixar combate menos estático.

Regras:

- Space executa dodge;
- A/D + Space esquiva lateral;
- S + Space esquiva para trás;
- invulnerabilidade curta;
- cooldown;
- sem ataque/interação durante dodge.

## 7. Progressão futura

Ferramentas e armas criam base para:

- economia de upgrades;
- crafting de ferramentas;
- mineração;
- recursos raros;
- biomas da cave;
- inimigos resistentes a tipos de dano;
- habilidades de personagem;
- quests que exigem ferramentas específicas.

## 8. Fora de escopo imediato

- durabilidade de ferramenta;
- árvore de habilidades;
- combos avançados;
- procedural cave;
- arte final;
- UI final completa;
- balanceamento final.

## 9. Referências

- `docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`
- `docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md`
- `docs/architecture/ARCH_fase4_v2.3_FASE9C_DELTA.md`
- `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md`


