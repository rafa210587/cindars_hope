# Cindar's Hope — Player Skill Trees Direction

> **Status:** documento canônico de direção detalhada das skill trees do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> **Complementa:** `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> **Não é spec implementável.** Este documento define direção de design, mecânica e balanceamento das skills. Specs futuras devem quebrar isso em implementação quando for a hora.

---

## 0. Objetivo

Este documento refina somente as **skills** e **skill trees** do jogador.

Ele substitui a lista preliminar de skills do documento de core sempre que houver conflito.

Direção central:

```text
O jogador terá 5 skill trees.
O jogador terá no máximo 50 SkillPoints no endgame.
Cada árvore tem 5 tiers.
Tier 5 é o capstone.
O jogador deve conseguir masterizar uma árvore principal e investir parcialmente em outras.
O jogador não deve conseguir masterizar duas árvores completas.
Skills devem ter efeito mecânico real no jogo, não só flavour.
Skills de precisão pura devem ser evitadas porque chance de miss será superficial.
```

---

# PARTE A — Regras globais das skill trees

## 1. Árvores canônicas

As 5 árvores iniciais são:

```text
1. Melee / Guerreiro
2. Ranged / Caçador
3. Magic / Arcano
4. Survival / Sobrevivente
5. Crafting / Produção
```

Sistemas sociais, romance, companions e pets são transversais neste momento. Eles podem receber bônus por atributos, arquétipos, quests ou itens, mas não são uma sexta árvore agora.

## 2. Economia de SkillPoints

```text
SkillPoints máximos no endgame: 50.
SkillPoint: 1 ponto a cada 2 níveis, salvo ajuste futuro de curva.
Masterização funcional de uma árvore: ~28-32 pontos.
Completar tudo de uma árvore: ~38-45 pontos.
Com 50 pontos: 1 árvore principal masterizada + investimento parcial em outras.
Com 50 pontos: não deve ser possível masterizar duas árvores completas.
```

## 3. Tiers

| Tier | Requisito de pontos gastos na árvore | Papel |
|---:|---:|---|
| Tier 1 | 0 | Fundamentos, ações básicas, pequenas passivas |
| Tier 2 | 4 | Eficiência, custo, primeira especialização leve |
| Tier 3 | 9 | Técnicas relevantes, alteração de gameplay |
| Tier 4 | 16 | Especialização forte, sinergias de build |
| Tier 5 | 24 | Capstone, identidade final da árvore |

## 4. Tipos de node

| Tipo | Ocupa active slot? | Direção |
|---|---:|---|
| Passive | Não | Bônus permanente ou condicional |
| Active Skill | Sim | Habilidade equipada em um dos 4 active slots |
| Movement Action | Não | Dash/Dodge ficam fora dos active slots |
| Defensive Action | A decidir | Block pode usar input próprio ou active slot, decisão futura |
| Unlock | Não | Libera sistema, receita, tier, material, item ou interação |
| Modifier | Não | Altera ação existente, como ataque, dodge, mineração, magia |
| Capstone | Normalmente não | Pode ser passivo, modificador ou active especial |

## 5. Regras de miss, precisão e crítico

Chance de miss existe, mas deve ser superficial.

```text
Miss não deve dominar o combate.
Skills não devem gastar muitos pontos só para “acertar mais”.
Precisão pode existir como flavour, mas mecanicamente deve virar bônus mais útil.
```

Preferir efeitos como:

```text
+chance de crítico
+dano crítico
+janela de crítico em inimigo vulnerável
+stagger/posture damage
+redução de custo
+redução de cooldown
+melhor resposta após dodge/block
+melhor loot/yield
+maior controle/status
```

## 6. Regra sobre partes do corpo / pontos fracos

O jogo ainda não terá sistema de ataque em local específico do corpo.

Portanto:

```text
Não usar skill que exige mirar em olho, asa, cabeça, perna ou core específico.
“Ponto fraco” pode existir como flavour textual.
Mecanicamente, deve ser tratado como abertura, vulnerabilidade, critical window, alvo marcado ou estado exposto.
```

Exemplo correto:

```text
Leitura de Abertura:
  aumenta chance de crítico contra inimigos marcados, stunned, slowed, em recovery ou durante critical window.
```

Exemplo incorreto:

```text
Tiro no Olho:
  exige acertar especificamente o olho do monstro.
```

## 7. Active slots e input separado

```text
4 active slots são para habilidades equipáveis.
Dash não ocupa active slot.
Dodge não ocupa active slot.
Dash = Space + direção, após desbloqueio.
Dodge = double tap direcional, ação base.
Block é skill, mas precisa de decisão final: input defensivo próprio ou active slot.
```

---

# PARTE B — Melee / Guerreiro

## 8. Identidade da árvore

Melee/Guerreiro é a árvore de combate físico de curta distância.

Foco:

```text
ataques corpo a corpo
block
postura/stagger
heavy attacks
controle de swarms próximos
crítico por abertura
sobrevivência em proximidade
```

Atributos de sinergia:

```text
Força: dano pesado, stagger, armas grandes.
Constituição: HP, resistência a dano/status/stagger.
Destreza: timing, dodge follow-up, crítico por abertura.
Breath: ritmo de combate e sustentação de block/ataques pesados.
```

## 9. Skills Melee/Guerreiro

| Skill | Tier | Custo/Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Treinamento Marcial | 1 | 1-5 | Passive | +2/4/6/8/10% dano com armas melee. Não afeta ferramentas usadas para coleta. | bônus passivo no painel da arma |
| Ataque Pesado | 1 | 1-5 | Modifier | Charged attack melee causa +10/18/26/34/42% posture damage e +5/10/15/20/25% dano, com custo maior de Stamina. | brilho/charge na arma |
| Block / Bloqueio | 1 | 1-5 | Defensive Action | Habilita block. Reduz dano frontal em 35/42/49/56/63%. Consome Stamina/Breath ao segurar. | ícone de escudo + drain de recurso |
| Guarda Firme | 2 | 1-5 | Passive | Reduz custo de block em 6/12/18/24/30% e reduz chance de stagger recebido enquanto bloqueia. | escudo reforçado no HUD |
| Corte Amplo | 2 | 1-3 | Active Skill | Ataque em arco curto. Atinge até 2/3/4 inimigos próximos com dano reduzido em alvos secundários. Bom contra packs. | active slot + cooldown |
| Contra-Ataque | 2 | 1-3 | Modifier/Reaction | Após block perfeito, próximo ataque melee em até 1.5s ganha +20/35/50% crit chance. | flash de contra-ataque |
| Quebra-Postura | 3 | 1-5 | Passive | +8/16/24/32/40% dano de postura contra inimigos em telegraph, recovery ou usando ataque pesado. | barra/ícone de posture do inimigo |
| Lâmina de Abertura | 3 | 1-5 | Passive | Ataques melee contra inimigos vulneráveis, stunned, slowed ou em critical window ganham +5/10/15/20/25% crit chance. | feedback de abertura |
| Especialização: Arma Pesada | 3 | 1-5 | Passive | Hammer/Axe/Great Sword: +6/12/18/24/30% dano de postura; +3/6/9/12/15% custo de Stamina. | marcador de especialização |
| Especialização: Arma Leve | 3 | 1-5 | Passive | Dagger/Sword/Spear curta: -4/8/12/16/20% recovery entre ataques; +2/4/6/8/10% crit chance contra alvo marcado/vulnerável. | marcador de especialização |
| Pele de Batalha | 4 | 1-5 | Passive | Enquanto HP > 60%, reduz dano físico recebido em 3/6/9/12/15%. Não regenera HP. | buff passivo no status |
| Golpe de Ruptura | 4 | 1-3 | Active Skill | Golpe forte que aplica grande posture damage e pequeno knockback. Custo alto, cooldown médio. | active slot + impacto |
| Capstone: Guerreiro das Profundezas | 5 | 3 | Capstone | Após quebrar postura de inimigo, por 5s: +20% dano melee, +20% resistência a stagger, e ataques em critical window causam +25% dano crítico. | aura curta + ícone de capstone |

## 10. Observações de balanceamento Melee

```text
Melee deve ser forte em risco/recompensa.
Block dá segurança, mas custa recurso.
Ataque pesado deve ser perigoso se usado no timing errado.
Crítico vem de abertura/vulnerabilidade, não de mirar em parte específica.
```

---

# PARTE C — Ranged / Caçador

## 11. Identidade da árvore

Ranged/Caçador é a árvore de distância, controle, marcação e exploração ofensiva segura.

Foco:

```text
arco/projéteis
controle de distância
marcação de alvo
crítico por abertura
interrupção
loot de caça
combate contra inimigos voadores/flutuantes sem mirar em parte específica
```

Atributos de sinergia:

```text
Destreza: velocidade, crit chance condicional, mobilidade.
Inteligência: leitura de padrões, marcação, armadilhas simples.
Vontade: foco sob medo/pressão e resistência mental.
```

## 12. Skills Ranged/Caçador

| Skill | Tier | Custo/Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Treino de Arco | 1 | 1-5 | Passive | +2/4/6/8/10% dano com armas ranged. Não reduz miss de forma relevante. | bônus na arma |
| Marcador de Presa | 1 | 1-3 | Active Skill | Marca 1 alvo por 8/10/12s. Você e pet/companion causam +5/8/12% dano nele. | ícone acima do alvo |
| Disparo Carregado | 1 | 1-5 | Active Skill | Tiro com windup. +15/25/35/45/55% dano e +10/15/20/25/30% chance de stagger leve/interrupção em inimigos menores. | barra de charge |
| Passo do Caçador | 2 | 1-5 | Passive | Após ataque ranged, próximo movimento nos 0.8s seguintes recebe +4/8/12/16/20% velocidade curta. Não é Dash. | rastro breve nos pés |
| Leitura de Abertura | 2 | 1-5 | Passive | Contra alvo marcado, slowed, stunned ou em recovery: +4/8/12/16/20% crit chance. Substitui “tiro em ponto fraco”. | brilho no retículo/alvo |
| Disparo de Interrupção | 2 | 1-3 | Active Skill | Interrompe casts fracos ou ataques canalizados de inimigos não-boss. Em boss, reduz barra de posture em 6/9/12%. | active slot + efeito de impacto |
| Flecha Perfurante | 3 | 1-3 | Active Skill | Projétil atravessa até 2/3/4 alvos em linha curta. Dano cai 20% por alvo atravessado. | linha de projétil |
| Armadilha de Caçador | 3 | 1-3 | Active Utility | Coloca trap simples de slow/root curto por 1.0/1.5/2.0s. Não substitui traps do mundo. | armadilha no chão + cooldown |
| Caçador de Voadores | 3 | 1-5 | Passive | Contra inimigos flying/floating: +4/8/12/16/20% dano e +5/10/15/20/25% posture damage. | ícone de asa no alvo |
| Flecha Preparada | 4 | 1-5 | Modifier/Unlock | Permite aplicar consumíveis simples à flecha: fogo/gelo/veneno leve. Consome item ou carga. Efeitos fortes exigem crafting/magia. | ícone de munição preparada |
| Retirada Tática | 4 | 1-3 | Passive | Ao acertar inimigo marcado que se aproxima, chance de 20/35/50% de aplicar slow curto. Cooldown interno. | efeito de slow |
| Olho do Caçador | 4 | 1-5 | Passive | +3/6/9/12/15% chance de loot de caça adicional em criaturas orgânicas, couro, presa, pena, carne, componente. | feedback de loot extra |
| Capstone: Predador de Aberturas | 5 | 3 | Capstone | Contra alvo marcado e vulnerável: primeiro crítico ranged causa +40% dano crítico e renova 50% da duração da marca. Cooldown por alvo. | marca evoluída + hit flash |

## 13. Observações de balanceamento Ranged

```text
Ranged deve ser seguro, mas depender de posicionamento e recurso.
Não deve virar magia gratuita.
Não deve depender de mirar em partes específicas.
“Ponto fraco” vira abertura, marcação, vulnerabilidade ou critical window.
Precisão pura é baixa prioridade porque miss será superficial.
```

---

# PARTE D — Magic / Arcano

## 14. Identidade da árvore

Magic/Arcano é a árvore de MP, magia elemental, magia arcana, magia espiritual/divina limitada, uso de itens mágicos e interação com Fonte/Mana.

Foco:

```text
custo de MP
regen lenta de MP
magias elementais
cura limitada
barreiras/selos
purificação/corrupção
uso de itens mágicos
interação com Fruto de Mana e Fonte de Anya
```

Atributos de sinergia:

```text
Vontade: MP, regen lenta, resistência espiritual.
Inteligência: controle, eficiência, potência técnica.
Carisma: suporte/liderança espiritual, quando aplicável.
```

## 15. Skills Magic/Arcano

| Skill | Tier | Custo/Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Canalização Serena | 1 | 1-5 | Passive | Reduz custo de MP de magias básicas em 4/8/12/16/20%. | custo de MP menor no tooltip |
| Foco Arcano | 1 | 1-5 | Passive | +3/6/9/12/15% dano/efeito de magias ofensivas e de controle leve. | bônus no painel mágico |
| Projétil Arcano | 1 | 1-5 | Active Skill | Magia básica de dano arcano. Custo baixo, dano médio. Ranks aumentam dano e reduzem levemente cooldown. | active slot + custo MP |
| Fluxo Lento | 2 | 1-5 | Passive | Regen natural de MP aumenta em +5/10/15/20/25% sobre a base de Vontade. Continua lenta. | indicador discreto de MP regen |
| Selo de Proteção | 2 | 1-3 | Active Skill | Gera barreira curta que absorve dano por 2/3/4s. Não bloqueia tudo. Custo médio/alto. | ward visual + escudo temporário |
| Afinidade Elemental: Fogo | 2 | 1-5 | Modifier/Unlock | Magias de fogo: +4/8/12/16/20% dano de Burn e duração controlada. | ícone fogo |
| Afinidade Elemental: Gelo | 2 | 1-5 | Modifier/Unlock | Magias de gelo: +4/8/12/16/20% potência de Chill/slow, sem stun abusivo. | ícone gelo |
| Afinidade Elemental: Raio | 3 | 1-5 | Modifier/Unlock | Magias de raio: +4/8/12/16/20% chance de interrupção leve; bônus contra constructos/máquinas. | ícone raio |
| Afinidade Natural/Água | 3 | 1-5 | Modifier/Unlock | Magias de água/natureza: melhora purificação leve, suporte e interação ambiental. | ícone água/folha |
| Toque Restaurador | 3 | 1-5 | Active Skill | Cura limitada. Custo alto. Cura 8/12/16/20/24% do HP máximo, cooldown alto. Não substitui comida/poção. | active slot + cura |
| Uso de Item Mágico | 3 | 1-5 | Unlock/Passive | Permite usar varinhas, selos, scrolls, amuletos e cargas mágicas de tiers maiores. Reduz chance de falha/efeito fraco de item mágico. | item mágico desbloqueado |
| Resistir Corrupção | 4 | 1-5 | Passive | Reduz efeito/duração de Corruption, Shadow/Nyx, Void e Blackstone em 5/10/15/20/25%. | ícone de resistência espiritual |
| Eco da Fonte | 4 | 1-5 | Passive/Lore | Melhora efeitos da Fonte de Anya, Água Viva, respec e cura especial após marcos narrativos. Sem efeito total no início. | HUD contextual na Fonte |
| Capstone: Fragmento Desperto | 5 | 3 | Capstone | Após gastar 40% do MP máximo em combate/exploração, próxima magia espiritual/arcana custa -50% MP e tem +25% efeito. Cooldown longo. | aura de Fonte/Mana |

## 16. Observações de balanceamento Magic

```text
MP regenera lentamente e não deve ser trivializado.
Cura existe, mas é cara e limitada.
Magia elemental deve gerar escolhas de build.
Uso de item mágico é progressão real, não só flavour.
Magia sombria/corrompida deve depender de lore, risco e progressão; não é magia inicial livre.
```

---

# PARTE E — Survival / Sobrevivente

## 17. Identidade da árvore

Survival/Sobrevivente é a árvore de runs longas, resistência, fome, cansaço, exploração da caverna, loot contextual, tesouros, ouro e mitigação de ambiente.

Foco:

```text
cansar menos
sentir menos fome
suportar runs mais longas
resistir a frio/calor/gás/medo/corrupção leve
melhorar loot e ouro em contexto de caverna
achar itens melhores em baús/tesouros já existentes
extrair melhor de nodes de minério durante run
Dash
Dodge melhorado
HP regen fora de combate
```

Não cobre neste momento:

```text
detecção de secret rooms
atalhos ocultos
map reveal avançado
sistema de stealth profundo
```

Atributos de sinergia:

```text
Constituição: tolerância, HP, resistência física.
Destreza: dodge, dash, reação.
Vontade: medo, corrupção, pressão espiritual.
Inteligência: leitura de perigo e aproveitamento de recursos.
```

## 18. Skills Survival/Sobrevivente

| Skill | Tier | Custo/Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Estômago Forte | 1 | 1-5 | Passive | Reduz perda de fome em runs/caverna em 4/8/12/16/20%. Em combate, reduz parte do multiplicador de fome. | barra de fome com ícone |
| Ritmo de Jornada | 1 | 1-5 | Passive | Reduz ganho de cansaço por movimento/exploração em 4/8/12/16/20%. | indicador de cansaço reduzido |
| Reflexo de Esquiva | 1 | 1-5 | Passive | Dodge continua base. Aumenta janela de invulnerabilidade/reduz recovery em valores pequenos por rank. | feedback de dodge perfeito |
| Dash | 2 | 1-5 | Movement Action | Habilita Dash com Space + direção. Ranks reduzem custo/cooldown/recovery. Não ocupa active slot. | indicador de Dash separado |
| Respiração Controlada | 2 | 1-5 | Passive | Reduz custo de Breath em Dash, corrida, dodge e pressão ambiental em 5/10/15/20/25%. | Breath com ícone de fôlego |
| Saqueador Cuidadoso | 2 | 1-5 | Passive | +3/6/9/12/15% chance de ouro adicional em drops de criatura e tesouros. Não cria loot infinito. | pop-up de ouro extra |
| Garimpo de Run | 2 | 1-5 | Passive | Nodes de minério na caverna têm +3/6/9/12/15% chance de minério extra. Funciona em caverna; produção/fazenda segue Crafting. | feedback de minério extra |
| Descanso Curto | 3 | 1-5 | Utility/Passive | Fora de combate por X segundos, recupera pequena fração de Stamina/Breath e reduz cansaço levemente. | ícone de descanso válido |
| Regeneração Natural | 3 | 1-5 | Passive | HP regen lenta e limitada fora de combate: 0.2/0.4/0.6/0.8/1.0% HP máx por segundo ou intervalo balanceado. Pausa ao tomar dano. Itens amplificam. | ícone de regen ativa |
| Resistência Ambiental | 3 | 1-5 | Passive | Reduz penalidades de frio, calor, gás, gelo ambiental e outros hazards em 5/10/15/20/25%. | ícone de proteção ambiental |
| Faro de Tesouro | 4 | 1-5 | Passive | Baús/tesouros já gerados têm +2/4/6/8/10% chance de rolar item de raridade superior. Não cria sala secreta. | brilho no baú após abrir |
| Mente Inabalável | 4 | 1-5 | Passive | Reduz duração/efeito de Fear, ConfusionLite, Nyx/Void leve em 5/10/15/20/25%. | ícone mental/espiritual |
| Resistência de Run Profunda | 4 | 1-3 | Passive | Em run de caverna, após 10/20/30 min in-game de exploração contínua, reduz penalidades de fome/cansaço em valor moderado. | buff de run longa |
| Capstone: Sobrevivente das Profundezas | 5 | 3 | Capstone | Uma vez por run, ao chegar em cansaço alto ou HP baixo, ativa por 8s: -40% gasto de Breath, +25% resistência ambiental, HP regen fora de combate fica dobrada após sair do perigo. | alerta de sobrevivência |

## 19. Observações de balanceamento Survival

```text
Survival deve tornar runs longas mais viáveis, não mais fáceis sem limite.
Loot melhor deve ser probabilístico e moderado.
Ouro extra deve ser pequeno para não quebrar economia.
Garimpo de Run existe porque mineração da caverna é loop central.
Secret rooms/atalhos ficam fora até o sistema existir.
```

---

# PARTE F — Crafting / Produção

## 20. Identidade da árvore

Crafting/Produção é a árvore de ferramentas, fazenda, construção, oficinas, materiais, qualidade, tiers e eficiência econômica/produtiva.

Foco:

```text
reduzir custo de ações agrícolas no early/mid game
reduzir cansaço indireto por menor gasto de stamina
melhorar qualidade/yield produtivo
reduzir custo de construção/crafting
liberar tiers de material
liberar armas/ferramentas melhores
melhorar cozinha, buffs e preparo
criar máquinas/processadores/oficinas
```

Atributos de sinergia:

```text
Inteligência: crafting, receita, máquinas, qualidade.
Força: derrubar/quebrar obstáculos, mas não yield direto.
Constituição: aguentar rotina produtiva.
Destreza: timing de pesca/ferramenta.
Carisma: comércio/animais em efeitos futuros.
Vontade: crops raras, Mana e Fonte em efeitos específicos.
```

## 21. Skills Crafting/Produção

| Skill | Tier | Custo/Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Mãos de Lavrador | 1 | 1-5 | Passive | Reduz custo de Stamina ao arar/regar/plantar em 5/10/15/20/25%. Importante early/mid; reduz cansaço indiretamente. | custo menor no tooltip/ação |
| Coleta Eficiente | 1 | 1-5 | Passive | +3/6/9/12/15% chance de recurso extra em colheita, madeira comum e coleta simples. Substitui bônus bruto de Força. | pop-up de recurso extra |
| Lenhador Prático | 1 | 1-5 | Passive | Árvores/troncos exigem menos golpes por rank em limites moderados e podem gerar +chance pequena de madeira extra. | contador/golpes reduzidos |
| Prospector de Superfície | 1 | 1-5 | Passive | Mineração comum/baixa: +3/6/9/12/15% chance de minério extra. Minérios raros profundos dependem de Survival/Caverna e tiers. | minério extra |
| Cozinha Sustentadora | 2 | 1-5 | Crafting/Passive | Comidas recuperam +5/10/15/20/25% fome/stamina ou reduzem cansaço em valor pequeno, conforme receita. | tooltip de comida melhorado |
| Oficina Organizada | 2 | 1-5 | Passive | Reduz custo de construção/crafting estrutural em 3/6/9/12/15%, limitado por categoria e sem zerar materiais raros. | custo reduzido no menu |
| Ferramentas de Ferro | 2 | 1 | Unlock | Libera upgrade/craft de ferramentas de Ferro. | tier liberado |
| Ferramentas de Aço | 3 | 1 | Unlock | Libera ferramentas de Aço e upgrades equivalentes. | tier liberado |
| Forja de Prata | 3 | 1-3 | Unlock/Modifier | Libera armas de Prata. Ranks melhoram dano contra mortos-vivos/sombras/maldições em +5/10/15%. | ícone prata + bônus situacional |
| Trabalho em Mithril | 4 | 1-3 | Unlock/Modifier | Libera armas/armaduras/ferramentas de Mithril. Equipamentos tendem a ser leves, duráveis e com menor custo de stamina. | tier mithril |
| Engenharia Bromeciana | 4 | 1-5 | Unlock/Crafting | Libera máquinas, processadores, irrigação avançada, mecanismos e itens técnicos por rank. | UI de máquina/processador |
| Alquimia Prática | 3 | 1-5 | Crafting | Melhora poções, antídotos, bombas leves, óleos de arma e consumíveis de run. | tooltip de consumível |
| Cultivo de Mana | 4 | 1-5 | Lore/Crafting | Aumenta chance de cultivar/estabilizar crops raras ligadas a Mana, sem garantir Fruto de Mana. Depende de Fonte/lore/fazenda. | ícone de Mana/crop rara |
| Capstone: Mestre da Produção | 5 | 3 | Capstone | Uma vez por dia, ao craftar/construir/colher lote relevante: chance de recuperar parte de material comum, melhorar qualidade ou gerar output extra moderado. Não afeta materiais únicos/endgame. | feedback dourado de produção |

## 22. Por que reduzir stamina agrícola ainda importa

```text
No early game, reduz custo direto de ações básicas.
No mid game, reduz cansaço acumulado porque Stamina gasta acelera cansaço.
No late game, continua útil em dias de grande produção, mas perde peso para automação, irrigação, companions, máquinas e upgrades.
```

Direção:

```text
Mãos de Lavrador não deve ser capstone.
É skill de conforto e eficiência inicial.
No late game, deve ser complementada por máquinas, ferramentas melhores, companions e construções.
```

## 23. Substituição de “Estabilização Rara”

A skill antiga “Estabilização Rara” fica removida como nome genérico.

Substituições melhores:

```text
Cultivo de Mana
  para crops raras, Fruto de Mana, Fonte e fazenda.

Engenharia Bromeciana
  para tecnologia, máquinas e mecanismos.

Alquimia Prática
  para consumíveis, poções, óleos e preparo de run.
```

---

# PARTE G — HUD e UI das skills

## 24. HUD base relacionado a skills

HUD deve comunicar:

```text
4 active slots
cooldown de active skills
custo de MP/Stamina/Breath
recurso insuficiente
Dash separado dos active slots
Dodge por feedback de movimento, não hotbar
Block disponível/ativo, conforme decisão de input
buffs/debuffs de skills
HP regen ativa/inativa
loot/yield extra quando ocorrer
critical window e hit crítico
marcação de alvo
resistência ambiental ativa
```

## 25. Menu de skill tree

O menu deve mostrar:

```text
50 SkillPoints máximos
pontos gastos totais
pontos restantes
pontos gastos por árvore
tier desbloqueado por árvore
pré-requisitos por node
rank atual de cada skill
se a skill ocupa active slot ou não
se a skill é movement action, passive, unlock, modifier ou capstone
preview mecânico do próximo rank
respec disponível na Fonte de Anya
```

## 26. Tooltip de skill

Cada tooltip deve ter formato mínimo:

```text
Nome
Árvore
Tier
Tipo
Rank atual / rank máximo
Custo em SkillPoints
Pré-requisitos
Efeito mecânico atual
Efeito do próximo rank
Custo de recurso se active
Cooldown se active
Ocupa active slot? Sim/Não
Interações com atributos/equipamentos
```

---

# PARTE H — Decisões fechadas neste refinamento

```text
Tiro em ponto fraco não será skill literal de mirar em parte específica.
Ranged terá Leitura de Abertura, Marcador de Presa e crítico condicional.
Chance de miss será superficial; skills de precisão pura serão evitadas.
Secret rooms/atalhos não entram em Survival por enquanto.
Survival foca runs longas, fome/cansaço, loot, ouro, tesouros existentes, mineração de caverna, resistência ambiental, Dash, Dodge melhorado e HP regen.
Crafting/Produção mantém redução de stamina agrícola, mas como skill early/mid que reduz cansaço indiretamente.
Oficina Organizada reduz custo de construir/craftar estruturas, não apenas flavour.
Estabilização Rara foi removida como skill genérica.
Crafting/Produção terá Cultivo de Mana, Engenharia Bromeciana e Alquimia Prática como alternativas mais claras.
Dash não ocupa active slot.
Dodge não ocupa active slot.
Block é skill, mas input final ainda precisa ser definido.
HP regen é skill de Survival e pode ser amplificada por itens.
```

---

# PARTE I — Pendências

```text
Definir custo final de cada node.
Definir se todos os ranks custam 1 ponto ou se ranks altos custam mais.
Definir input final do Block.
Definir valores finais de cooldown, custo e duração em gameplay real.
Validar se o capstone de cada árvore fica passivo ou active.
Validar quantos active skills por árvore entram na primeira implementação.
Validar se Ranged terá munição consumível ou munição abstrata.
Validar como loot extra interage com economia da cidade.
Validar quais materiais entram em cada tier de crafting.
Validar se Cultivo de Mana depende de nível da fazenda, Fonte, estação, lua ou quest.
Validar se Engenharia Bromeciana depende de ruínas/caverna/cidade.
Validar integração com HUD final.
