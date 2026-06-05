# Cindar's Hope — Combat Core Direction

> **Status:** documento canônico de direção ampla do combate  
> **Local:** `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md`  
> **Função:** consolidar como o combate deve funcionar: ritmo, input, movimentação, stamina, ataque, defesa, magia, janelas, inimigos, companions, pets, HUD e validação.  
> **Não é spec implementável.** Este documento orienta design. Specs futuras devem transformar isto em código/data assets.

---

## 0. Regra anti-duplicação

Este documento não deve ser a fonte primária de fórmulas numéricas de atributos derivados.

Fontes canônicas:

```text
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  fórmulas de HP, MP, Stamina, Stamina Regen, BlockImpact, Defense, AttackDamage etc.

docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
  exemplo/teste de mesa de personagem contra monstros.

docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
  roster, atributos, packs, bosses, traits, moves, behavior e dados de monstros.

docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades, janelas, TTK, active combat budget e telemetria de caverna.
```

Regra:

```text
Se houver divergência de fórmula, PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md vence.
Se houver divergência de roster/monstro, CAVE_MONSTER_ROSTER_DIRECTION.md vence.
Se houver divergência de vulnerabilidade/janela da caverna, CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md vence.
Combat Core pode repetir decisões fechadas apenas em resumo, não como fonte duplicada de cálculo.
```

---

## 1. Referência externa de design — Children of Morta

`Children of Morta` é referência de feeling e arquitetura de combate, não de cópia.

Elementos usados como inspiração:

```text
action RPG / hack'n'slash com ritmo claro
incursões em dungeons com risco e progressão persistente
kits legíveis
combate baseado em leitura, posicionamento, ataques, evasão, defesa, habilidades e passivas
sensação de run perigosa, com base/casa/narrativa sustentando progressão
```

Adaptação para Cindar's Hope:

```text
Cindar's Hope usa personagem livre, não personagens fixos.
A build nasce de atributos, skill trees, armas, magia, equipamentos, companions e pets.
A caverna deve ter combate action semelhante no ritmo: inimigos legíveis, packs densos, janelas de punição, evasão cara, defesa cara e progressão persistente.
A identidade vem de Vaalara, fazenda, Fonte de Anya, cidade, companions, pets e caverna.
```

Regra:

```text
Não copiar nomes, personagens, habilidades, textos, valores, mapas, bosses ou sistemas proprietários.
```

---

# PARTE A — Visão geral do combate

## 2. Fantasia de combate

O combate deve ser:

```text
tático
rápido o suficiente para parecer action RPG
legível o suficiente para depender de leitura, não reflexo impossível
punitivo quando o jogador erra repetidamente
possível quando o jogador entende inimigo, Stamina, janelas e preparação
fortemente conectado a build, gear, companions, pets e consumíveis
```

Não deve ser:

```text
spam de ataque sem custo
spam de dodge/dash sem custo
HP sponge sem janelas
stunlock permanente
controle injusto do jogador
dano inevitável sem telegraph
arena lotada sem counterplay
```

## 3. Pilares

```text
Stamina é decisão.
Movimento normal é a primeira defesa.
Dash e Dodge são fortes, caros e intencionais.
Block é forte, caro e escala com gear/skills.
HP é margem de erro, não tanque universal.
MP abre magia, suporte e controle, mas com custo e build.
Vulnerabilidades e janelas recompensam leitura.
Companions/pets ajudam, mas não jogam pelo jogador.
Bosses são fases, telegraphs e escolhas, não só vida alta.
A caverna continua densa e desafiadora; progressão mitiga pressão naturalmente.
```

## 4. Loop de combate desejado

```text
entrar na sala
ler composição do pack
identificar ameaça principal
usar posicionamento normal antes de gastar Dash/Dodge
atacar com ritmo
bloquear quando fizer sentido
usar Dodge para evitar ataque crítico
usar Dash para reposicionar, sair de zona ou criar espaço
explorar MinorOpening/CriticalWindow/CoreExposed
usar companion/pet/consumível quando necessário
coletar recompensa ou decidir recuar
```

---

# PARTE B — Inputs centrais

## 5. Inputs de combate

```text
Movimento: WASD ou direcional equivalente
Ataque leve: botão primário
Ataque pesado/charged: segurar ou botão secundário conforme spec final
Dash: Space + direção
Dodge: double tap direcional
Block: Left Shift
Active skills: 4 slots equipáveis
Interação/loot: botão dedicado
Troca de arma/ferramenta: hotbar/atalhos
```

Regra:

```text
Dash, Dodge e Block não ocupam active slot.
Active slots são para skills equipáveis: ataques especiais, magias, suporte, utilidade ou técnicas.
```

## 6. Responsividade e input buffer

Regras:

```text
Input buffer curto para ataque, dodge, dash, block e skill.
Cancelamento limitado, nunca irrestrito.
Prioridade de Dodge/Dash deve ser clara.
Block deve levantar rápido, mas não instantâneo se o jogador estiver em recovery pesado.
Ataques pesados devem comprometer o jogador.
```

Alvos iniciais de sensação:

```text
Input buffer comum: 0.10s a 0.18s.
Buffer de Dodge/Dash: 0.08s a 0.14s.
Buffer de ataque leve em combo simples: 0.12s a 0.20s.
Buffer de ataque pesado: menor ou inexistente se for charged.
```

Regra:

```text
O jogador deve sentir que perdeu por decisão ruim, posicionamento ruim ou leitura ruim, não por input engolido.
```

---

# PARTE C — Recursos em combate

## 7. HP

HP é margem de erro.

Direção:

```text
HP do jogador escala devagar.
Constituição não transforma o jogador em elite tank sozinha.
Sobrevivência real vem da soma de HP, armor, block, dodge, comida, skills, companion/pet e execução.
```

Fórmula canônica:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

## 8. MP

MP é reserva mágica.

Direção:

```text
MP permite magia, suporte, dano e controle.
MP Regen natural é lenta.
Magia forte precisa de custo, cooldown, cast time, risco ou build.
```

Fórmula canônica:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

## 9. Stamina

Stamina é recurso físico imediato.

Direção:

```text
Stamina deve ser restritiva no começo.
Com tempo, skills, gear, comida, companions e domínio reduzem a fricção.
Mesmo no late game, Stamina não deve virar irrelevante.
```

Custos de referência já fechados:

```text
Light melee com Espada de Aço: 25 Stamina
Heavy melee com Espada de Aço: 40 Stamina
Dash: 40 Stamina
Dodge: 40 Stamina
Block hold: 18 Stamina/s
```

Fórmulas canônicas:

```text
Stamina Max: PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
Stamina Regen: PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
Block Impact: PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
Teste de mesa: PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
```

## 10. Fome e Cansaço no combate

Direção:

```text
Fome baixa reduz Stamina Regen e eficiência.
Cansaço alto reduz recuperação, segurança e ritmo.
Caverna longa deve pressionar planejamento, comida e decisão de retorno.
```

Fonte canônica:

```text
PLAYER_CORE_SYSTEMS_DIRECTION.md
FARM_DESIGN_DIRECTION_v1.3.md quando a spec tocar sono/fazenda/comida
```

---

# PARTE D — Movimento em combate

## 11. Unidade de movimento

Usar `tile/s` e `tiles` como unidades de design.

Regra:

```text
1 tile = unidade lógica de tilemap definida pela spec de cena.
Sprites podem ter 32x48 px, mas velocidade deve ser calibrada por tile/s e footbox, não por tamanho visual inteiro.
```

Direção visual:

```text
Player sprite: aproximadamente 32x48 px.
Player footbox: menor que o sprite completo, calibrada para colisão justa.
Ataques e hazards devem respeitar footbox/hurtbox legíveis.
```

## 12. Velocidade do jogador

Valores iniciais de design:

| Estado do jogador | Velocidade alvo | Observação |
|---|---:|---|
| caminhada/exploração normal | 3.8-4.2 tiles/s | velocidade base fora de combate |
| combate com arma pronta | 3.4-3.8 tiles/s | sensação mais tática |
| com armadura média | -4% a -8% | penalidade leve/moderada |
| com armadura pesada | -8% a -15% | em troca de armor/block |
| Stamina baixa crítica | -5% a -12% | feedback de exaustão, se usado |
| cansaço alto | -5% a -15% | aplicado fora/long run conforme sistema |
| fome baixa | sem slow direto no início | preferir afetar regen/eficiência antes de speed |
| slow status | -20% a -45% | duração curta, telegraph claro |
| root | movimento 0 ou quase 0 | raro, curto, com counterplay |

Regra:

```text
Movimento normal deve ser suficiente para evitar parte dos ataques simples.
Nem todo ataque deve exigir Dodge ou Dash.
```

## 13. Dash

```text
Input: Space + direção
Custo base: 40 Stamina
Função: reposicionamento forte
Não ocupa active slot
```

Valores iniciais:

| Propriedade | Alvo inicial |
|---|---:|
| distância base | 3.2-4.0 tiles |
| distância com upgrades/skills fortes | até 8.0 tiles, com cap |
| duração ativa base | 0.18s-0.30s |
| duração ativa com upgrades longos | pode subir, mas deve manter leitura visual |
| recovery base | 0.25s-0.40s |
| recovery em Dash muito longo | pode ser maior se necessário |
| cooldown mínimo base | 0.75s-1.20s |
| cooldown/custo em Dash muito longo | deve ser balanceado por skill/gear/cap |
| i-frame | nenhum ou muito baixo |

Influenciado por:

```text
Destreza
Survival / Passo de Impulso
Ritmo Controlado
equipamento leve/pesado
cansaço/status
gear leve/endgame
buffs/consumíveis específicos
```

Regras:

```text
Dash serve para reposicionar, sair de zona, cruzar espaço perigoso ou criar distância.
Dash deve ser perceptivelmente maior que Dodge.
Dash base deve parecer forte, mas não resolver tudo.
Dash com upgrades/skills pode virar mobilidade longa de build, chegando até ~8 tiles com cap.
Dash não deve ser esquiva universal.
Dash não deve substituir Dodge.
Dash caro exige que hazards/telegraphs deem espaço para decisão.
Dash longo não deve ter i-frame relevante por padrão.
Dash longo pode atravessar espaço, mas atravessar inimigos/corpos exige skill ou regra explícita.
O cap alto existe para late game/builds de mobilidade, não para início do jogo.
```

## 14. Dodge

```text
Input: double tap direcional
Custo base: 40 Stamina
Função: evasão de timing
Não ocupa active slot
```

Valores iniciais:

| Propriedade | Alvo inicial |
|---|---:|
| distância | 1.2-1.8 tiles |
| duração total | 0.28s-0.45s |
| i-frame base | 0.16s-0.24s |
| recovery | 0.18s-0.35s |
| cooldown mínimo | 0.45s-0.90s |

Influenciado por:

```text
Destreza
Survival / Reflexo de Esquiva
Ritmo Controlado
cansaço/status
```

Regras:

```text
Dodge deve ser defesa de timing.
Dodge não deve substituir movimentação normal.
Dodge caro exige telegraph justo dos inimigos.
Skills podem melhorar janela/custo/recovery, mas com cap.
```

## 15. Movimento durante ações

| Ação | Movimento permitido |
|---|---|
| ataque leve | 25%-45% da velocidade base durante execução |
| cadeia de ataques leves | micro avanço opcional, sem magnetismo exagerado |
| ataque pesado/charged | 0%-25% durante charge/execução |
| bow aiming | 45%-65% da velocidade base |
| cast mágico leve | 40%-70%, dependendo da magia |
| cast mágico pesado | 0%-30%, com telegraph claro |
| block segurado | 35%-55% da velocidade base |
| hit reaction leve | breve redução, sem travar demais |
| knockback/stagger | controle parcial ou nenhum, curto e legível |

Regra:

```text
Ação forte deve comprometer movimento.
Ação leve pode manter fluidez.
O jogador não deve deslizar sem controle nem ficar travado por longos períodos sem feedback.
```

## 16. Velocidade dos inimigos

Os nomes oficiais de `Move` pertencem ao roster da caverna.

Fonte canônica:

```text
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Este documento só registra faixas de velocidade sugeridas para esses `Move` oficiais.

| Move oficial do roster | Velocidade alvo | Uso |
|---|---:|---|
| TankSlowPush | 1.5-2.4 tiles/s | tanques, constructos, baluartes |
| GroundPatrol | 2.0-3.0 tiles/s | patrulha, guarda leve, deslocamento não agressivo |
| GuardStationary | 0.0-1.5 tiles/s | guardião fixo, anchor, recurso/baú/porta |
| GroundChase | 2.7-3.5 tiles/s | comuns corpo a corpo |
| PackFlanker | 3.2-4.2 tiles/s | flanqueadores, humanoides rápidos, pack hunters |
| PackLeader | 2.8-3.8 tiles/s | líder que comanda e pressiona sem ser sempre o mais rápido |
| RetreatAndCall | 2.8-4.0 tiles/s | recua, chama pack, reposiciona |
| SwarmErratic | 3.4-5.2 tiles/s | enxames, morcegos, pequenos rápidos |
| KiteRanged | 2.4-3.4 tiles/s | arqueiros/cuspidor, recua e atira |
| CasterKeepAway | 2.0-3.0 tiles/s | caster que reposiciona pouco |
| FloatingSlow | 1.6-2.6 tiles/s | wisp lento, eco, ameaça espacial |
| FloatingOrbit | 2.0-3.2 tiles/s | orbit, olho, wisp, caster flutuante |
| CircleStrafe | 3.0-4.0 tiles/s | duelistas, arqueiros móveis, predadores táticos |
| Leaper | baixa base + salto | usa burst, não speed contínua alta |
| ChargeLine | windup + burst | investida em linha, sempre com telegraph/recovery |
| BurrowAmbush | deslocamento subterrâneo por telegraph | não deve ser hitscan |
| PhaseShortBlink | blink curto com tell/recovery | não deve ser fuga infinita sem counterplay |
| TreasureIdleAmbush | 0 até ativar | emboscada de tesouro, ativa por interação/proximidade |
| HazardLure | 2.2-3.4 tiles/s | tenta puxar o jogador para hazard |
| ProtectAnchor | 0.8-2.2 tiles/s | protege núcleo, ritual, node ou boss mechanic |
| BossArenaControl | custom | por fase/arena |
| BossPhaseShift | custom | transição e reposicionamento por fase |

Regra:

```text
Inimigo mais rápido que o jogador precisa ter vida menor, telegraph claro, ou janelas de punição frequentes.
Inimigo tank lento pode ter HP/armor maior.
Burst de movimento deve ter windup/recovery.
Dash longo do jogador não obriga reduzir velocidade/densidade dos inimigos; ele é recurso caro e/ou de build.
Não criar novos nomes de MovementProfile em specs se já existir Move oficial equivalente no roster.
```

## 17. Collision e body blocking

Direção:

```text
Player e inimigos têm footbox clara.
Inimigos pequenos podem cercar, mas não travar completamente o jogador sem rota.
Inimigos grandes podem bloquear passagem como papel tático.
Bosses usam colliders customizados.
```

Regras:

```text
Swarm pode pressionar, mas precisa de saída.
Body block deve ser desafio, não bug de pathing.
Dash pode atravessar alguns inimigos pequenos apenas se skill/spec permitir.
Dodge não deve atravessar todos os corpos por padrão.
Dash longo sem skill de atravessar corpo ainda deve respeitar colisão.
```

---

# PARTE E — Ataques do jogador

## 18. Ataque leve

Função:

```text
dano principal de baixo compromisso
cadência curta
baixo/moderado posture damage
custo relevante de Stamina
```

Regras:

```text
Ataque leve não é gratuito.
Ataque leve deve ter recovery suficiente para punir spam em inimigos perigosos.
Armas leves podem atacar mais rápido, mas ainda gastam Stamina.
```

Valores de sensação:

| Tipo | Duração total alvo | Uso |
|---|---:|---|
| dagger/light | 0.22s-0.38s | baixo dano, alta cadência |
| sword/spear | 0.35s-0.55s | baseline |
| axe/hammer leve | 0.50s-0.75s | mais dano/posture, mais risco |

## 19. Ataque pesado / charged attack

Função:

```text
alto dano
alto posture damage
maior risco
maior custo de Stamina
boa ferramenta contra armor, constructos, elites e janelas
```

Valores de sensação:

| Tipo | Charge/execução alvo | Uso |
|---|---:|---|
| sword heavy | 0.55s-0.85s | finisher/abertura |
| axe/hammer heavy | 0.75s-1.20s | posture/armor |
| spear thrust charged | 0.55s-0.95s | alcance/pierce |
| bow charged | 0.65s-1.10s | precisão/dano |
| staff charged | 0.70s-1.30s | magia/risco |

Regras:

```text
Ataque pesado deve comprometer movimento/recovery.
Ataque pesado deve ser recompensador em CriticalWindow/CoreExposed.
Ataque pesado não deve ser sempre melhor que ataque leve.
```

## 20. Combos

Direção inicial:

```text
Combos devem ser simples.
Não criar lista longa de inputs.
Progressão vem mais de skills, armas, janelas e timing do que de combo complexo.
```

Possibilidades futuras:

```text
terceiro hit com stagger maior
ataque leve -> pesado como finisher
ataque pós-perfect block
ataque pós-dodge bem sucedido
ataque pós-companion setup
```

---

# PARTE F — Block

## 21. Block base

```text
Input: Left Shift
Custo: hold + impacto
Não ocupa active slot
Desbloqueio/melhoria: Melee / Guerreiro
```

Regras:

```text
Block é forte, mas caro.
Block reduz dano, mas drena Stamina.
Sem Stamina, Block quebra ou perde eficiência.
Block não deve resolver todo tipo de ataque.
```

## 22. Block Power e BlockImpact

Fonte canônica:

```text
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

Decisões fechadas:

```text
Block hold custa 18 Stamina/s como referência.
Block impact usa dano pós-armadura/mitigação contra HP máximo do jogador.
Armor reduz dano recebido e reduz dreno de Stamina do impacto.
Block Stability reduz dreno de Stamina, não deve eliminar custo.
```

## 23. Perfect Block / Counter futuro

Direção:

```text
Perfect Block pode existir como skill/upgrade.
Deve exigir timing curto.
Pode reduzir Stamina drain, abrir MinorOpening/CriticalWindow ou ativar Contra-Ataque.
Não deve ser obrigatório no early game.
```

---

# PARTE G — Dano, armor e resistências

## 24. Dano físico

Fonte canônica de fórmula:

```text
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

Direção:

```text
Força domina dano bruto.
Destreza ajuda armas leves, cadência, recovery e crítico condicional.
Arma/material/skill devem importar mais que atributo isolado.
```

## 25. Armor e mitigação

Fonte canônica de fórmula:

```text
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

Direção:

```text
Armor é a principal mitigação flat.
Constituição não substitui equipamento.
Defense/Armor não deve zerar dano de packs inteiros sem regra de dano mínimo.
Armor também reduz o dreno de Stamina em BlockImpact.
```

## 26. Resistências

Tipos principais:

```text
Physical
Fire
Ice
Lightning
Water/Nature
Poison
Bleed
Shadow/Nyx
Arcane
Blackstone/Corruption
Fear/Mental
Heat
Cold
```

Regras:

```text
Resistência reduz dano/efeito.
Imunidade deve ser rara e justificada por boss/lore.
Vulnerabilidade deve abrir counterplay, não ser requisito único.
```

---

# PARTE H — Crit, janelas e vulnerabilidades

## 27. Tipos de abertura

Fonte canônica de categorias:

```text
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Resumo:

```text
MinorOpening: abertura comum/curta, sem crítico automático.
CriticalWindow: janela clara, mais rara/arriscada, pode garantir crítico.
CoreExposed/BossMechanicWindow/StaggeredWindow: janela especial, pode garantir crítico + bônus moderado.
```

## 28. Critical Hit

Direção:

```text
Crítico normal depende de chance.
CriticalWindow pode garantir crítico.
CoreExposed pode garantir crítico + bônus moderado.
Crítico nunca deve virar dano infinito.
```

## 29. Vulnerabilidades

Cada inimigo deve declarar pelo menos um caminho de counterplay:

```text
ElementVulnerability
StatusVulnerability
AttackTypeVulnerability
WeaponVulnerability
BehavioralVulnerabilityWindow
```

Fonte canônica:

```text
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

## 30. Telegraphs

Todo ataque relevante deve ter:

```text
windup
sinal visual/sonoro
execução
recovery
janela de vulnerabilidade quando aplicável
categoria da janela
```

Bosses precisam de telegraph mais claro que mobs comuns.

---

# PARTE I — Status e controle

## 31. Status negativos permitidos

```text
Burn
Poison
Bleed
Slow
Stun
Chill
Root
Fear
ConfusionLite
DurabilityStress
Hunger
Fatigue
HeatStress
ColdStress
Corruption
```

Regras:

```text
ConfusionLite não remove controle total do jogador.
Controle permanente não existe.
Stun forte exige telegraph/cooldown.
Status de boss deve ter counterplay.
DurabilityStress não destrói item permanentemente sem spec própria.
```

## 32. Posture / stagger

Fonte canônica futura:

```text
spec_combat_posture_stagger_guardbreak.md
```

Direção:

```text
Ataques pesados causam mais posture damage.
Hammer/Axe/Pickaxe podem ser melhores contra armor/constructos.
Posture break pode abrir CriticalWindow ou StaggeredWindow.
Bosses podem ter posture por fase, não barra única simples.
```

## 33. Guard Break

GuardBreak é ferramenta contra Block/Shield.

Usos:

```text
inimigos anti-block
elites tank
bosses
skills do jogador
```

Regras:

```text
GuardBreak precisa de telegraph claro.
GuardBreak não deve ignorar toda defesa sem aviso.
```

---

# PARTE J — Armas e estilos

## 34. Tipos iniciais

```text
Sword
Axe
Hammer
Spear
Bow
Dagger
Staff
ToolAttack
```

## 35. Papéis

```text
Sword: equilíbrio, bom contra aberturas médias.
Axe: dano forte, bom contra madeira/raízes/armadura média.
Hammer: posture, constructos, armor, stagger.
Spear: alcance, pierce, asas/pontos expostos.
Bow: distância, flying/floating, marcação.
Dagger: velocidade, crítico condicional, risco alto.
Staff: magia, suporte, dano arcano, defesa mágica.
ToolAttack: utilitário, emergencial, não substitui arma dedicada.
```

## 36. Materiais e tiers

Direção:

```text
Tiers maiores aumentam dano/eficiência/utilidade, mas não eliminam custo.
Prata deve ser boa contra mortos-vivos/sombras/maldições.
Mithril tende a ser leve/durável/eficiente.
Materiais raros podem alterar stamina cost, damage type ou vulnerability interaction.
```

---

# PARTE K — Magia

## 37. Magia em combate

Magia usa MP e deve ter função clara.

Tipos:

```text
projétil ofensivo
zona elemental
barreira/selo
cura limitada
purificação
controle leve
interação ambiental
buff/debuff
```

Regras:

```text
Magia não substitui todos os estilos físicos.
Magia forte exige MP, cooldown, cast time ou risco.
Cura mágica deve ser limitada, cara e com cooldown.
Barreiras não bloqueiam tudo.
```

## 38. Anya e Senya

```text
Semente Arcana de Anya: suporte, cura, purificação, barreira, eco de recuperação.
Semente Arcana de Senya: dano mágico, efeitos elementais, overload/crit mágico controlado.
```

Regra:

```text
Capstones são escolhas fortes e exclusivas.
Não devem invalidar comida, poções, companions ou gear.
```

---

# PARTE L — Inimigos, packs e bosses

## 39. Monstros comuns

Função:

```text
ensinar padrões
pressionar posicionamento
formar packs
criar atrito de HP/Stamina
proteger recursos/tesouros
```

Regras:

```text
Comum isolado não deve ser trivial sempre.
Comum em pack deve mudar o problema tático.
Comum não deve exigir execução perfeita.
```

## 40. Elites

Função:

```text
testar uma mecânica
forçar leitura
punir spam
recompensar vulnerabilidade/janela
criar mini-clímax de sala
```

Regras:

```text
Elite deve ter pelo menos 2 ações relevantes.
Elite deve ter janela clara.
Elite pode exigir uso de skill, comida, companion/pet ou recuo.
Elite não deve ser apenas comum com HP alto.
```

## 41. Packs

Fonte canônica de composição e roster:

```text
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Direção:

```text
Packs devem ser densos e desafiadores.
O active budget não deve ser reduzido só porque Stamina é cara.
Skills, companions, pets, consumíveis e domínio do jogador mitigam naturalmente a pressão.
```

## 42. Bosses

Bosses devem ter:

```text
2-3 fases
ataques bem telegráficos
mudança de movimento ou arena
janelas claras
vulnerabilidades por fase
pressão de Stamina/HP/MP
momentos de recuperação controlados
```

Bosses não devem depender só de:

```text
HP alto
dano inevitável
adds infinitos
controle permanente
```

---

# PARTE M — Companions e pets

## 43. Companions

Papéis possíveis:

```text
Tank
Ranged
Healer/Support
Controller
Miner/Hybrid
```

Regras:

```text
Companion ajuda, mas não joga sozinho.
Companion pode abrir MinorOpening/CriticalWindow em casos específicos.
Companion pode segurar pressão, mas não substituir decisão do jogador.
```

## 44. Pets

Pets são sistema próprio.

Cachorro:

```text
apoio contra swarm
interrupção leve
marcação
atração curta de pequenos inimigos
detecção de perigo/treasure trap
```

Gato:

```text
sorte/achados
detecção de anomalia
bônus indiretos
vínculo social/fazenda
```

Regras:

```text
Cachorro não ocupa slot de companion.
Pet não deve tankar boss.
Pet não deve ser obrigatório no início.
```

---

# PARTE N — Consumíveis e preparo

## 45. Consumíveis

Tipos:

```text
comida de Stamina
comida de HP
poção de cura
poção de MP
antídoto
resistência elemental
óleo de arma
bombas leves futuras
Fruto de Mana raro
```

Regras:

```text
Consumível deve ser parte do preparo de run.
Consumível não deve trivializar boss.
Comida deve conversar com fazenda/cozinha.
Fruto de Mana é raro e especial, não item comum de spam.
```

## 46. Loadout de entrada

Antes da caverna, o jogador deve considerar:

```text
arma
ferramenta
comida
poções
resistência elemental
companion
pet
skills ativas
equipamento
objetivo da run
```

---

# PARTE O — HUD e feedback

## 47. Feedback de combate que precisa ser comunicado

O jogador precisa ler:

```text
hit recebido
hit causado
block bem sucedido
block quase quebrando
Stamina insuficiente
MP insuficiente
MinorOpening
CriticalWindow
CoreExposed
vulnerabilidade hit
status aplicado
boss phase transition
companion setup
pet interrupt
```

Apresentação, HUD layout, prioridade visual, feedback visual/sonoro, notificações, cave HUD e debug HUD são definidos em:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

---

# PARTE P — Integração com caverna

## 50. Procedural e combate

A geração procedural deve respeitar:

```text
active enemy budget
spawn anchors seguros
rotas de fuga
salas de tesouro com risco
packs com lógica
hazards por bioma
boss gates
checkpoints
```

## 51. Biomas

Cada bioma deve influenciar combate:

```text
frio: Chill, ColdStress, menor regen, resistência necessária
fogo: Burn, HeatStress, zona de dano
ruínas: constructs, traps, overcharge, puzzles
abismo: Fear, Shadow/Nyx, controllers
núcleo: corrupção, Pedra Negra, bosses/endgame
```

## 52. Nível 101

```text
conteúdo endgame
bosses em sequência
informação final de Anya
libertação parcial do poder de Anya
recuperação controlada entre encontros
sem farm trivial
```

---

# PARTE Q — Telemetria e validação

## 53. Métricas de Play Mode

Registrar em playtest:

```text
TTK por inimigo/papel
Stamina gasta por categoria
Stamina recuperada
HP perdido
MP gasto
número de Dodges/Dashes
distância média do Dash usado
tempo segurando Block
Block impacts
hits em MinorOpening/CriticalWindow/CoreExposed
consumíveis usados
companion/pet triggers
deaths/retreats
velocidade média do jogador em combate
vezes em que o jogador ficou body blocked
vezes em que hitbox/telegraph pareceram injustos
```

Regra:

```text
Não transformar consumo de Stamina em percentual fixo de spec.
Usar telemetria para detectar extremos: trivial, injusto, esponja, sem counterplay, velocidade ruim, dash longo quebrando salas ou stamina irrelevante.
```

## 54. Validação humana

A validação humana deve observar:

```text
se o combate parece justo
se o custo de Stamina é sentido
se Dodge/Dash são escolhas fortes
se Dash longo cria posicionamento divertido sem trivializar salas
se Block é útil, mas não dominante
se movimento normal resolve parte dos ataques simples
se telegraphs são legíveis
se packs são desafiadores sem avalanche injusta
se companions/pets ajudam sem resolver tudo
se bosses têm fases claras
se velocidade do jogador e inimigos combina com tela pequena e pixel art
```

---

# PARTE R — Decisões fechadas

```text
Combat Core segue referência de feeling similar a Children of Morta: action RPG/hack'n'slash roguelite, runs, kits claros, dungeons perigosas e progressão persistente.
Combat Core não deve duplicar fórmulas canônicas de atributos derivados.
Cindar's Hope usa personagem livre com build, não personagens fixos.
Breath/Fôlego não existe como atributo/recurso.
Breath pode existir em nome de ataque de sopro de criatura.
HP do jogador escala devagar.
Constituição não é atributo defensivo universal.
Stamina usa Level + CON + FOR + DES conforme documento de atributos derivados.
Light melee com Espada de Aço custa 25 Stamina.
Heavy melee com Espada de Aço custa 40 Stamina.
Dash custa 40 Stamina.
Dodge custa 40 Stamina.
Block hold custa 18 Stamina/s.
Block impact usa dano pós-armadura/mitigação contra HP máximo.
Dash, Dodge e Block não ocupam active slot.
4 active slots são para skills equipáveis.
Movimento normal é a primeira defesa.
Velocidade base do jogador em exploração deve ficar em 3.8-4.2 tiles/s.
Velocidade em combate com arma pronta deve ficar em 3.4-3.8 tiles/s.
Dash base deve cobrir 3.2-4.0 tiles e ser reposicionamento forte, não dodge universal.
Dash com upgrades/skills fortes pode chegar até ~8.0 tiles, com cap e validação.
Dash longo não deve ter i-frame relevante por padrão.
Dash longo não atravessa corpos por padrão; atravessar inimigos exige skill/regra explícita.
Dodge deve cobrir 1.2-1.8 tiles com i-frame curto e recovery.
Moves oficiais de inimigos vêm do CAVE_MONSTER_ROSTER_DIRECTION.md.
Combat Core só sugere faixas de velocidade para esses Moves oficiais.
Nem toda abertura dá crítico automático.
MinorOpening, CriticalWindow e CoreExposed são categorias diferentes.
Active combat budget da caverna não foi reduzido por causa da Stamina cara.
Companions/pets/gear/skills/consumíveis mitigam naturalmente a dificuldade com progressão.
```

---

# PARTE S — Pendências para specs futuras

```text
Definir CombatController input contract.
Definir PlayerMovementController e unidade tile/s em Unity.
Definir PlayerAttackController.
Definir WeaponActionDataSO.
Definir StaminaCostProfileSO.
Definir BlockController e BlockImpact formula final.
Definir Dash/Dodge timing, i-frames, cooldown e recovery.
Definir Dash long-upgrade cap, validação de sala e interação com colisão.
Definir MovementProfileSO para jogador e inimigos usando nomes oficiais de Move do CAVE_MONSTER_ROSTER_DIRECTION.md.
Definir DamageType final.
Definir Armor/Defense final.
Definir CriticalHit contract.
Definir MinorOpening/CriticalWindow/CoreExposed data contract.
Definir EnemyAction telegraph/recovery/window contract.
Definir UI feedback de Stamina baixa, block, critical window e vulnerability.
Definir integração com companions/pets.
Definir Play Mode telemetry para TTK/Stamina/HP/MP/movement/dash distance.
Definir validações Unity por spec.
```