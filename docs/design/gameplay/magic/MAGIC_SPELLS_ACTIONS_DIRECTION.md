# Cindar's Hope — Magic, Spells & Spell Actions Direction

> **Status:** documento canônico de direção de magia, spells, formas de alvo, MP, scaling e pré-requisitos  
> **Local:** `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> **Função:** definir um conjunto enxuto de magias jogáveis, levemente inspirado em D&D, adaptado ao combate action de Cindar's Hope, com pré-requisitos, escala, formas de alvo, custos, cooldowns, status e integração com equipment/enemies.  
> **Não é spec implementável.** Specs futuras devem converter isto em data assets, runtime e UI.

---

## 0. Regras anti-duplicação

Este documento **não** redefine:

```text
fórmulas de MP máximo, MP Regen ou MagicDamage derivado;
significado de Burn, Chill, Poison, Stun, Slow, Root, Fear, ConfusionLite, DurabilityStress ou Corruption;
stats de staff, wand, scroll, tome ou focus;
vulnerabilidades concretas de inimigos;
active slots gerais;
capstones da árvore Magic.
```

Fontes canônicas:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  fórmulas de MP, MP Regen, MagicDamage e derivados.

PLAYER_SKILL_TREES_DIRECTION.md
  árvore Magic/Arcano, tiers, skills, capstones Semente Arcana de Anya e Semente Arcana de Senya.

COMBAT_CORE_DIRECTION.md
  input, active slots, cast interruptível, ritmo de combate, janelas e HUD base.

STATUS_EFFECTS_DIRECTION.md
  significado mecânico de status.

EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
  staff charged, wands, scrolls, tomes, focuses, tags e custos base de itens mágicos.

EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
  matching entre tags de magia/equipamento e vulnerabilidades dos inimigos.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades por família, limites de multiplicador e TTK.
```

Regra:

```text
Este documento define quais magias existem, como são usadas e como escalam.
Fórmulas finais e runtime ficam em specs futuras.
```

---

# PARTE A — Filosofia da magia

## 1. Magia deve ser limitada e legível

O jogo não deve ter uma lista enorme de magias.

Direção:

```text
poucas magias;
cada magia com função clara;
formas de alvo variadas;
pré-requisitos claros;
escala por INT/VON/skill/equipment;
custo de MP relevante;
cast time/cooldown quando efeito for forte;
risco de interrupção;
boa leitura visual;
boa integração com vulnerabilidades/status;
sem escola divina rígida por deus.
```

## 2. Inspiração em D&D, adaptação para action RPG

A inspiração de D&D entra como arquétipo:

```text
Magic Missile-like: projétil arcano guiado/autotarget fraco.
Burning Hands-like: cone de fogo curto.
Ray of Frost-like: linha/raio de gelo com Chill.
Lightning Bolt-like: linha de raio.
Cure Wounds-like: cura direta curta.
Shield-like: selo/barreira curta.
Bless-like: buff curto de foco/defesa.
Faerie Fire/Hunter's Mark-like: revelar/marcar alvo.
Lesser Restoration-like: purificação de status/corrupção leve.
```

Regra:

```text
Não copiar nomes, texto, progressão, valores ou funcionamento exato de D&D.
Usar apenas a fantasia/arquetipo como referência.
```

## 3. Deus no nome não cria tipo de magia

Magias podem ter nome ligado a deuses de Vaalara, mas isso não cria uma escola mecânica separada.

Exemplos:

```text
Selo de Kanthor é uma magia de barreira/buff espiritual.
Brasa de Senya é uma magia elemental/ofensiva.
Toque de Anya é uma cura/purificação espiritual.
Brisa de Telisandra é um buff de mobilidade/resistência.
Martelo de Thoren é uma linha/impacto arcano-físico.
```

Regra:

```text
O tipo mecânico continua sendo DamageType, SpellShape, StatusTags, Tags e Scaling.
Nome de deus é identidade/lore, não escola exclusiva.
```

---

# PARTE B — Modelo mecânico de spell

## 4. SpellActionDataSO — campos mínimos

```text
SpellId
DisplayName
Description
Tier
SpellCategory
SpellShape
DamageTypes
StatusTags
UtilityTags
PrimaryAttribute
SecondaryAttribute
ScalingWeights
BasePower
MPCost
Cooldown
CastTime
ChannelTime optional
RecoveryTime
RangeTiles
RadiusTiles optional
ConeAngle optional
LineWidth optional
MaxTargets optional
CanAutoTarget
RequiresAim
CanAffectSelf
CanAffectCompanion
CanAffectPet
CanAffectEnemy
CanAffectBoss
Interruptible
MovementLock
RequiredMagicTreePoints
RequiredSkillNode
RequiredSkillRank
RequiredTomeOrRecipe optional
RequiredEquipment optional
RequiredStoryFlag optional
AllowedByCapstone optional
ForbiddenByCapstone optional
TagsApplied
VulnerabilityTagsChecked
HUDIcon
VFXProfileId
SFXProfileId
DebugTags
```

## 5. SpellCategory

Categorias mecânicas permitidas:

```text
Damage
Healing
Buff
Barrier
Utility
Purification
ControlLight
RiskCorruption
```

Regras:

```text
Damage é a maior parte do kit.
Healing deve existir, mas ser limitado por MP/cooldown/cast.
Buffs devem ser poucos e curtos.
Utilidade deve ser pequena e clara.
Controle forte não deve existir sem telegraph/cooldown e resistência.
RiskCorruption é late/endgame e não deve ser usado cedo.
```

## 6. SpellShape

Formas de alvo permitidas:

```text
Self
AllyTarget
AutoTargetEnemy
AimProjectile
Cone
Line
PlacedAoE
Chain
AuraShort
GroundSeal
```

Regras:

```text
AutoTarget deve ter dano menor, custo baixo/médio e limite de alvos.
AimProjectile deve recompensar mira e posicionamento.
Cone é forte contra swarm, mas exige proximidade.
Line é forte em corredor, mas exige alinhamento.
PlacedAoE é forte contra packs, mas exige cast time/telegraph.
Chain é forte contra grupos, mas precisa limite de saltos.
Self/AllyTarget são suporte/cura/buff.
GroundSeal/AuraShort devem ter duração curta e área clara.
```

## 7. Atributos e scaling

Scaling padrão:

```text
Inteligência
  dano elemental, arcano, raio, fire, gelo, AoE, wands ofensivas.

Vontade
  MP, MP Regen, cura, barreira, purificação, resistência espiritual, estabilidade de cast.

Destreza
  não escala magia diretamente por padrão, mas pode afetar conforto de mira/cast em specs futuras se necessário.

Força/Constituição
  não escalam magia ofensiva por padrão.
```

Regras:

```text
Magia ofensiva comum: INT 70% / VON 30%.
Magia espiritual/suporte: VON 70% / INT 30%.
Magia híbrida/arcana: INT 55% / VON 45%.
Magia de risco/corrupção: INT/VON custom e story gating.
```

## 8. MP, cast e interrupção

Regras:

```text
Toda spell gasta MP, salvo scroll consumível que pode reduzir ou substituir parte do custo.
MP Regen natural continua lenta.
Spell forte precisa de cast time, cooldown, recovery, custo alto, posição arriscada ou pré-requisito.
Spell pode ser interrompida por dano, stagger, silence futuro ou ação inimiga específica.
Cast interrompido pode consumir parte do MP conforme spec futura.
Staff/focus melhora magia, mas não remove custo/risco.
```

Baseline de custo:

```text
Tier 1: 6-12 MP
Tier 2: 12-20 MP
Tier 3: 18-32 MP
Tier 4: 28-45 MP
Tier 5/endgame: 40+ MP ou custo especial
```

## 9. Relação com active slots

```text
Spells equipáveis ocupam active slots.
O jogador tem 4 active slots.
Dash, Dodge e Block não ocupam active slot.
StaffBasicBolt pode existir como ação da arma/focus, não necessariamente como active slot.
Scrolls e wands podem ser usados por hotbar/consumível/equipamento conforme spec futura.
```

Regra:

```text
A escolha de spells nos 4 active slots deve ser estratégica.
Não permitir carregar cura, barreira, AoE, burst, utility e controle tudo ao mesmo tempo sem tradeoff.
```

---

# PARTE C — Progressão e pré-requisitos

## 10. Tiers de acesso

Usar tiers da árvore Magic/Arcano:

```text
Tier 1 — 0 pontos na árvore Magic
Tier 2 — 5 pontos na árvore Magic
Tier 3 — 11 pontos na árvore Magic
Tier 4 — 18 pontos na árvore Magic
Tier 5 — 26 pontos na árvore Magic / capstone / story flags
```

Regra:

```text
Nem toda spell Tier 1 precisa ser gratuita; algumas podem exigir tutorial, tome, NPC ou item.
Ranks de skill melhoram custo, dano, duração, efeito ou acesso a variações.
```

## 11. Pré-requisitos por skill node

Nós de Magic já definidos:

```text
Canalização Serena
Foco Arcano
Projétil Arcano
Fluxo Lento
Selo de Proteção
Afinidade Elemental: Fogo
Afinidade Elemental: Gelo
Afinidade Elemental: Raio
Afinidade Natural/Água
Toque Restaurador
Uso de Item Mágico
Resistir Corrupção
Eco da Fonte
Capstone: Semente Arcana de Anya
Capstone: Semente Arcana de Senya
```

Regras:

```text
Spells elementais usam afinidades correspondentes como melhoria/pré-requisito de ranks altos.
Spells de cura/purificação usam Toque Restaurador, Eco da Fonte ou Vontade alta.
Wands/scrolls/tomes/focuses usam Uso de Item Mágico para tiers altos.
Capstones não são obrigatórios para jogar magia, mas liberam/alteram magias icônicas de late game.
```

---

# PARTE D — Lista enxuta de spells

## 12. Lista canônica inicial

Total inicial: 18 spells.

Distribuição:

```text
Dano: 10
Cura/purificação: 3
Buff/barreira: 3
Utilidade: 2
```

Regra:

```text
Não criar novas spells antes de esgotar variações, upgrades, tomes, focus modifiers e charged staff effects.
```

---

# PARTE E — Tier 1 spells

## 13. Projétil Arcano

```text
SpellId: arcane_projectile
Categoria: Damage
Shape: AutoTargetEnemy ou AimProjectile curto, conforme spec final
DamageType: Arcane
Scaling: INT 55% / VON 45%
MPCost: 8
Cooldown: 0.8s
CastTime: 0.15s-0.25s
Range: 5.0 tiles
Pré-requisito: Magic Tier 1 / skill Projétil Arcano
Tags: Arcane, MagicProjectile
```

Função:

```text
Magia básica confiável.
Dano médio/baixo.
Boa para build arcana inicial.
Não deve superar arma física sem investimento.
```

## 14. Fagulha de Senya

```text
SpellId: senya_spark
Categoria: Damage
Shape: AimProjectile
DamageType: Fire
StatusTag: Burn baixa chance
Scaling: INT 70% / VON 30%
MPCost: 10
Cooldown: 1.5s
CastTime: 0.25s
Range: 4.5 tiles
Pré-requisito: Magic Tier 1; melhora com Afinidade Elemental: Fogo
Tags: Fire, BurnSource, MagicProjectile
```

Função:

```text
Dano de fogo simples.
Útil contra inimigos vulneráveis a Fire/Burn.
Nome de Senya é flavor/lore, não escola separada.
```

## 15. Frio Lunar

```text
SpellId: lunar_chill
Categoria: Damage / ControlLight
Shape: AimProjectile
DamageType: Ice
StatusTag: Chill baixa/moderada chance
Scaling: INT 60% / VON 40%
MPCost: 9
Cooldown: 1.6s
CastTime: 0.25s
Range: 5.0 tiles
Pré-requisito: Magic Tier 1; melhora com Afinidade Elemental: Gelo
Tags: Ice, ChillSource, MagicProjectile
```

Função:

```text
Dano menor que fogo, mas aplica Chill leve.
Não deve virar stun ou slow permanente.
```

## 16. Luz Menor de Anya

```text
SpellId: anya_minor_light
Categoria: Healing
Shape: Self
DamageType: nenhum
Scaling: VON 75% / INT 25%
MPCost: 12
Cooldown: 12s
CastTime: 0.45s
Pré-requisito: Magic Tier 1; melhora com Toque Restaurador
Tags: Healing, Light/Radiant, AnyaFlavor
```

Função:

```text
Cura pequena inicial.
Não substitui comida/poção.
Não deve ser spammável em combate.
```

---

# PARTE F — Tier 2 spells

## 17. Cone de Brasa

```text
SpellId: ember_cone
Categoria: Damage
Shape: Cone
DamageType: Fire
StatusTag: Burn chance moderada em vulneráveis
Scaling: INT 75% / VON 25%
MPCost: 18
Cooldown: 5s
CastTime: 0.35s
Range: 2.2 tiles
ConeAngle: 55°-70°
Pré-requisito: Magic Tier 2 + Afinidade Elemental: Fogo rank 1
Tags: Fire, BurnSource, Cone, AreaOfEffect
```

Função:

```text
Resposta contra swarms próximos.
Exige proximidade, então é arriscada.
```

## 18. Linha de Gelo

```text
SpellId: ice_line
Categoria: Damage / ControlLight
Shape: Line
DamageType: Ice
StatusTag: Chill
Scaling: INT 65% / VON 35%
MPCost: 17
Cooldown: 5s
CastTime: 0.35s
Range: 4.5 tiles
LineWidth: estreita
Pré-requisito: Magic Tier 2 + Afinidade Elemental: Gelo rank 1
Tags: Ice, ChillSource, Line
```

Função:

```text
Boa em corredor/alinhamento.
Dano moderado e Chill leve.
Não deve rootar por padrão.
```

## 19. Selo de Kanthor

```text
SpellId: kanthor_ward
Categoria: Barrier / Buff
Shape: Self ou AllyTarget curto
DamageType: nenhum
Scaling: VON 70% / INT 30%
MPCost: 18
Cooldown: 14s
CastTime: 0.35s
Duration: 3s-5s
Pré-requisito: Magic Tier 2 + Selo de Proteção rank 1
Tags: Barrier, Spiritual, KanthorFlavor
```

Função:

```text
Barreira curta.
Reduz dano recebido ou aumenta resistência a stagger por pouco tempo.
Não bloqueia tudo.
Não substitui Block/escudo.
```

## 20. Marca da Lua Oculta

```text
SpellId: hidden_moon_mark
Categoria: Utility / Buff ofensivo
Shape: AutoTargetEnemy ou AimProjectile
DamageType: nenhum ou Arcane mínimo
Scaling: INT 50% / VON 50%
MPCost: 14
Cooldown: 10s
CastTime: 0.30s
Duration: 6s-10s
Pré-requisito: Magic Tier 2 + Foco Arcano rank 2
Tags: Mark, Reveal, Arcane, MoonFlavor
```

Função:

```text
Marca/revela um inimigo.
Melhora levemente crit chance ou dano mágico/ranged contra o alvo.
Não acumula livremente com Marcador de Presa sem cap.
```

---

# PARTE G — Tier 3 spells

## 21. Raio de Thoren

```text
SpellId: thoren_sparkline
Categoria: Damage / ControlLight
Shape: Line
DamageType: Lightning
StatusTag: Stun leve/interrupção se vulnerável
Scaling: INT 75% / VON 25%
MPCost: 24
Cooldown: 7s
CastTime: 0.45s
Range: 5.0 tiles
LineWidth: estreita/média
Pré-requisito: Magic Tier 3 + Afinidade Elemental: Raio rank 1
Tags: Lightning, InterruptSource, Line, ThorenFlavor
```

Função:

```text
Linha de raio para interrupção leve e dano contra alvos alinhados.
Forte contra constructs/máquinas se o inimigo declarar vulnerabilidade.
Não deve stunnar boss sem janela específica.
```

## 22. Corrente Arcana

```text
SpellId: arcane_chain
Categoria: Damage
Shape: Chain
DamageType: Arcane
Scaling: INT 65% / VON 35%
MPCost: 26
Cooldown: 8s
CastTime: 0.45s
MaxTargets: 3-4
Range: alvo inicial 4.5 tiles; saltos curtos
Pré-requisito: Magic Tier 3 + Foco Arcano rank 3
Tags: Arcane, Chain, MultiTarget
```

Função:

```text
Dano multi-target controlado.
Dano cai por salto.
Boa contra packs médios, ruim contra single boss se comparada a spell focada.
```

## 23. Toque Restaurador

```text
SpellId: restoring_touch
Categoria: Healing
Shape: Self ou AllyTarget curto
DamageType: nenhum
Scaling: VON 80% / INT 20%
MPCost: 28
Cooldown: 25s
CastTime: 0.65s
Range: 1.2 tiles se ally/companion
Pré-requisito: Magic Tier 3 + Toque Restaurador rank 1
Tags: Healing, Spiritual, AnyaFlavor
```

Função:

```text
Cura principal.
Pode curar jogador ou companion, se sistema de companion permitir.
Custo alto e cooldown alto.
Não trivializa combate prolongado.
```

## 24. Purificar Mácula

```text
SpellId: purify_taint
Categoria: Purification / Utility
Shape: Self ou AllyTarget curto; variação PlacedAoE pequena em ranks altos
DamageType: Light/Radiant mínimo contra inimigo corrompido, se permitido
Scaling: VON 75% / INT 25%
MPCost: 24
Cooldown: 22s
CastTime: 0.60s
Pré-requisito: Magic Tier 3 + Afinidade Natural/Água rank 1 ou Toque Restaurador rank 2
Tags: Purify, Light/Radiant, CorruptionCounter, AnyaFlavor
```

Função:

```text
Remove/reduz Poison, Corruption leve ou Shadow/Nyx leve conforme status data.
Contra inimigo corrompido pode causar efeito menor se vulnerável.
Não remove boss corruption sem mecânica/lore específica.
```

---

# PARTE H — Tier 4 spells

## 25. Chuva de Faíscas

```text
SpellId: spark_rain
Categoria: Damage
Shape: PlacedAoE
DamageType: Lightning / Arcane
StatusTag: interrupção leve se vulnerável
Scaling: INT 80% / VON 20%
MPCost: 34
Cooldown: 14s
CastTime: 0.75s
Radius: 2.0-2.5 tiles
Duration: 2s-3s
Pré-requisito: Magic Tier 4 + Afinidade Elemental: Raio rank 3
Tags: Lightning, Arcane, PlacedAoE, InterruptSource
```

Função:

```text
AoE de controle leve e dano moderado.
Boa contra packs parados ou canalizadores.
Exige previsão e posicionamento.
```

## 26. Raízes de Água Viva

```text
SpellId: living_water_roots
Categoria: ControlLight / Purification
Shape: PlacedAoE
DamageType: Water/Nature ou Light/Radiant leve
StatusTag: Slow; Root curto apenas se inimigo permitir
Scaling: VON 60% / INT 40%
MPCost: 32
Cooldown: 18s
CastTime: 0.70s
Radius: 2.0 tiles
Duration: 2s-4s
Pré-requisito: Magic Tier 4 + Afinidade Natural/Água rank 3
Tags: Water/Nature, Purify, SlowSource, RootSource, FonteFlavor
```

Função:

```text
Controle de área leve.
Pode Slow e, contra inimigos vulneráveis, Root curto.
Não deve prender boss ou elite em loop.
Não consome Água Viva por padrão; variantes raras podem exigir recurso.
```

## 27. Brisa de Telisandra

```text
SpellId: telisandra_breeze
Categoria: Buff
Shape: Self / AuraShort
DamageType: nenhum
Scaling: VON 60% / INT 40%
MPCost: 30
Cooldown: 25s
CastTime: 0.40s
Duration: 6s-10s
Pré-requisito: Magic Tier 4 + Fluxo Lento rank 3 ou Resistir Corrupção rank 2
Tags: Buff, Movement, EnvironmentalResistance, TelisandraFlavor
```

Função:

```text
Buff curto de mobilidade/resistência ambiental/mental.
Não substitui Survival.
Pode reduzir levemente custo de Dash/Dodge ou melhorar resistência a Chill/Fear por curta duração.
```

## 28. Selo de Silêncio Rúnico

```text
SpellId: runic_silence_seal
Categoria: Utility / ControlLight
Shape: GroundSeal
DamageType: nenhum ou Arcane mínimo
Scaling: INT 60% / VON 40%
MPCost: 36
Cooldown: 28s
CastTime: 0.75s
Radius: 1.5-2.0 tiles
Duration: 3s-5s
Pré-requisito: Magic Tier 4 + Uso de Item Mágico rank 3 + tome/blueprint
Tags: Arcane, Seal, InterruptSource, AntiCaster
```

Função:

```text
Zona curta que dificulta casts fracos ou canalizações de não-boss.
Em bosses, só reduz posture/cast stability se houver janela específica.
Não é silence permanente.
```

---

# PARTE I — Tier 5 / late game spells

## 29. Eco de Anya

```text
SpellId: anya_echo
Categoria: Healing / Barrier / Purification
Shape: AuraShort ou Self + AllyTarget curto
DamageType: Light/Radiant mínimo contra corrupção, se vulnerável
Scaling: VON 85% / INT 15%
MPCost: 44
Cooldown: 45s
CastTime: 0.90s
Duration: 5s-8s
Pré-requisito: Magic Tier 5 + Eco da Fonte rank 3 + story flag da Fonte/Anya
AllowedByCapstone: Semente Arcana de Anya melhora fortemente, mas não é obrigatório se story permitir versão fraca
ForbiddenByCapstone: nenhum por padrão
Tags: Healing, Barrier, Purify, Light/Radiant, AnyaFlavor
```

Função:

```text
Magia late de suporte.
Cura moderada, barreira curta e purificação leve em área curta.
Não ressuscita em campo.
Não substitui a Fonte de Anya.
Não liberta Anya sozinha.
```

## 30. Ruptura de Senya

```text
SpellId: senya_rupture
Categoria: Damage
Shape: PlacedAoE ou Line curta explosiva
DamageType: Fire/Arcane, variação elemental conforme focus
StatusTag: Burn ou efeito elemental conforme focus
Scaling: INT 85% / VON 15%
MPCost: 46
Cooldown: 35s
CastTime: 1.0s
Radius: 2.0 tiles ou linha curta
Pré-requisito: Magic Tier 5 + Foco Arcano rank 5 + uma Afinidade Elemental rank 3
AllowedByCapstone: Semente Arcana de Senya melhora dano/efeito/custo de risco
Tags: Fire, Arcane, Burst, SenyaFlavor
```

Função:

```text
Magia late ofensiva forte.
Alto custo, cast arriscado e cooldown alto.
Não deve ser botão universal de limpar sala.
```

## 31. Véu de Nyx

```text
SpellId: nyx_veil
Categoria: RiskCorruption / Utility / Damage opcional
Shape: Self ou AuraShort
DamageType: Shadow/Nyx ou Corruption apenas em variações específicas
Scaling: INT 50% / VON 50%
MPCost: 38 + risco/custo especial
Cooldown: 40s
CastTime: 0.75s
Duration: 4s-7s
Pré-requisito: Magic Tier 5 + Resistir Corrupção rank 4 + story flag específica
Tags: Shadow/Nyx, CorruptionRisk, Risk, NyxFlavor
```

Função:

```text
Magia de risco, não core inicial.
Pode reduzir detecção, mitigar Fear/ConfusionLite ou gerar burst sombrio pequeno conforme focus.
Deve ter risco de Corruption, custo alto ou consequência.
Não deve ser necessária para terminar o jogo.
```

## 32. Martelo de Thoren

```text
SpellId: thoren_hammer
Categoria: Damage / ControlLight
Shape: Line curta ou PlacedAoE pequeno
DamageType: Lightning + Blunt/Arcane tag, conforme spec
StatusTag: Stun leve/posture se vulnerável
Scaling: INT 65% / VON 35%
MPCost: 42
Cooldown: 30s
CastTime: 0.90s
Pré-requisito: Magic Tier 5 + Uso de Item Mágico rank 4 + tome/blueprint bromeciano ou Thoren-related
Tags: Lightning, Arcane, BluntMagic, Posture, ThorenFlavor
```

Função:

```text
Magia late anti-construct/armor/posture.
Não substitui Hammer físico.
Boa quando inimigo tem vulnerabilidade a Lightning/Posture/Construct.
```

---

# PARTE J — Resumo das 18 spells

| Tier | Spell | Categoria | Shape | Papel |
|---:|---|---|---|---|
| 1 | Projétil Arcano | Damage | Auto/AimProjectile | básico arcano |
| 1 | Fagulha de Senya | Damage | AimProjectile | fogo inicial |
| 1 | Frio Lunar | Damage/ControlLight | AimProjectile | gelo + Chill leve |
| 1 | Luz Menor de Anya | Healing | Self | cura pequena |
| 2 | Cone de Brasa | Damage | Cone | anti-swarm próximo |
| 2 | Linha de Gelo | Damage/ControlLight | Line | corredor + Chill |
| 2 | Selo de Kanthor | Barrier/Buff | Self/Ally | proteção curta |
| 2 | Marca da Lua Oculta | Utility/Buff | Auto/Aim | marca/revela |
| 3 | Raio de Thoren | Damage/ControlLight | Line | raio/interrupção |
| 3 | Corrente Arcana | Damage | Chain | multi-target |
| 3 | Toque Restaurador | Healing | Self/Ally | cura principal |
| 3 | Purificar Mácula | Purification | Self/Ally/AoE pequena | remove corrupção/status leve |
| 4 | Chuva de Faíscas | Damage | PlacedAoE | AoE raio/arcano |
| 4 | Raízes de Água Viva | Control/Purify | PlacedAoE | slow/root curto se vulnerável |
| 4 | Brisa de Telisandra | Buff | Self/Aura | mobilidade/resistência curta |
| 4 | Selo de Silêncio Rúnico | Utility/Control | GroundSeal | anti-caster leve |
| 5 | Eco de Anya | Healing/Barrier/Purify | Aura/Self/Ally | suporte late |
| 5 | Ruptura de Senya | Damage | AoE/Line | burst late |
| 5 | Véu de Nyx | Risk/Utility | Self/Aura | risco/corrupção opcional |
| 5 | Martelo de Thoren | Damage/Control | Line/AoE pequeno | anti-construct/posture |

Observação:

```text
A lista acima tem 20 entradas se contar todas as opções Tier 5.
Para o MVP/baseline inicial, escolher 18 ativas: manter Eco de Anya e Ruptura de Senya como late principais, deixar Véu de Nyx e Martelo de Thoren como unlocks opcionais/expansão late.
```

Regra:

```text
Specs iniciais não devem implementar todas as Tier 5 ao mesmo tempo.
Tier 5 deve ser liberado por roadmap e playtest.
```

---

# PARTE K — Relação com equipment mágico

## 33. Staff

```text
Staff é o canal principal de build mágica.
Staff charged usa Arcane Channel.
Staff pode modificar spell shape, custo, cast time ou tags.
Staff não deve remover custo de MP.
```

## 34. Wands

Fonte canônica de baseline:

```text
EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
```

Regras:

```text
Wand oferece magia por carga/MP sem exigir build completa.
Wand não deve superar staff + skill tree.
Wand pode replicar versões fracas de Projétil Arcano, Fagulha, Frio, Raio ou Purificar.
Uso de Item Mágico libera wands de tiers maiores.
```

## 35. Scrolls / Pergaminhos

```text
Scroll é consumível.
Scroll pode permitir uso pontual de magia sem pré-requisito total.
Scroll forte precisa ser raro/caro/limitado.
Scroll não deve trivializar boss gate.
```

## 36. Tomes / Grimórios

```text
Tome desbloqueia spell, variação ou upgrade.
Tome é persistente e raro.
Tome pode ser recompensa de caverna, NPC, loja avançada, boss ou pesquisa.
```

## 37. Focus / Relic

```text
Focus altera identidade da spell.
ArcaneFocus melhora dano arcano.
HealingFocus melhora cura/barreira.
ElementalFocus melhora um elemento e piora versatilidade.
WardFocus melhora selo/barreira.
BlackstoneFocus é risco/endgame.
```

---

# PARTE L — Scaling e upgrades

## 38. Como spells escalam

Spells podem escalar por:

```text
INT/VON;
rank da skill relevante;
Magic tree points;
staff/focus/wand/tome;
material do equipamento;
capstone Anya/Senya;
vulnerabilidade do inimigo;
CriticalWindow/CoreExposed se spell permitir;
status ativo no alvo;
food/potion/buff temporário.
```

Regra:

```text
Escala não deve ser multiplicativa sem cap.
Vulnerabilidades e critical windows seguem limites de Cave Combat Balance.
```

## 39. Upgrade de spell

Não criar 5 versões de cada magia como spell separada.

Preferir:

```text
rank melhora custo/dano/duração;
tome desbloqueia variante;
focus muda elemento/forma;
capstone amplifica uma magia compatível;
gear reduz cast time ou MP em pequena escala;
```

Exemplos:

```text
Projétil Arcano pode virar AutoTarget mais confiável com rank, não virar nova spell.
Cone de Brasa pode ganhar ângulo maior com tome/focus, não virar segunda magia.
Toque Restaurador pode ganhar variação AllyTarget, não nova magia.
```

---

# PARTE M — Status e vulnerabilidades

## 40. Status aplicados por spells

Spells podem aplicar:

```text
Burn
Chill
Stun leve/interrupção
Slow
Root curto se vulnerável
Corruption apenas em spells de risco
Purify como efeito de remoção/redução
Marked como window/buff, não status DoT
```

Fonte canônica:

```text
STATUS_EFFECTS_DIRECTION.md
```

Regra:

```text
Spell não redefine status.
Spell aplica StatusTag.
EnemyData declara vulnerabilidade/resistência.
```

## 41. Vulnerabilidade

```text
Fire spell só recebe bônus se inimigo tiver vulnerabilidade Fire/Burn compatível.
Ice spell só recebe bônus/Chill forte se inimigo permitir.
Lightning spell só interrompe forte se inimigo permitir.
Light/Radiant/Purify só afeta corrupção/undead/shadow conforme vulnerabilidade.
Corruption/Shadow/Nyx deve respeitar ResistanceTags e risco narrativo/mecânico.
```

Fonte canônica de matching:

```text
EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
```

---

# PARTE N — HUD e feedback

## 42. HUD de spell

Mostrar:

```text
spell equipada nos active slots;
custo de MP;
cooldown;
cast bar para spells com cast time;
interrupção de cast;
range/shape preview quando aplicável;
AoE telegraph;
status aplicado;
MP insuficiente;
spell bloqueada por pré-requisito;
wand charges/scroll count quando aplicável.
```

Regras:

```text
Tela pequena não deve ficar poluída.
Shape preview deve ser claro, curto e só enquanto mira/casta.
AoE inimigo e AoE do jogador devem ter leitura visual distinta.
```

## 43. Feedback por shape

```text
AutoTargetEnemy
  highlight no alvo escolhido.

AimProjectile
  linha curta de mira ou retículo.

Cone
  preview de cone.

Line
  faixa/linha telegráfica.

PlacedAoE
  círculo no chão com tempo de cast.

Chain
  alvo inicial + indicação discreta de saltos.

Self/Aura
  ícone no player e borda/buff timer.

AllyTarget
  highlight em companion/pet permitido.
```

---

# PARTE O — Save/load

## 44. Salvar estado mágico

Persistir:

```text
knownSpellIds;
unlockedSpellVariants;
equippedSpellSlots;
knownTomes;
activeFocusItemInstance;
wand item instances com charges;
scroll stacks;
spell cooldowns relevantes se salvar em run/combat;
active buffs/debuffs com duração se necessário;
magic-related story flags;
capstone choice Anya/Senya se escolhido.
```

Não persistir como fonte primária:

```text
MagicDamage final recalculável;
MP cost final recalculável;
tooltip final;
spell power final;
```

Regra:

```text
Derivados devem ser recalculados no load com base em atributos, skills, equipment e buffs.
```

---

# PARTE P — Data assets esperados

## 45. Assets

```text
SpellActionDataSO
SpellShapeProfileSO
SpellScalingProfileSO
SpellUnlockRuleSO
SpellUpgradeRuleSO
SpellStatusApplicationSO
SpellVFXProfileSO
SpellSFXProfileSO
SpellHUDProfileSO
TomeSpellUnlockSO
FocusSpellModifierSO
WandSpellProfileSO
ScrollSpellProfileSO
MagicBalanceProfileSO
```

## 46. MagicBalanceProfileSO mínimo

```text
ProfileId
MPCostByTier
CooldownByCategory
CastTimeByShape
DamageMultiplierByShape
HealingMultiplierRules
StatusChanceCaps
BossEffectMultipliers
CapstoneAnyaModifiers
CapstoneSenyaModifiers
WandPowerModifiers
ScrollPowerModifiers
DebugTags
```

---

# PARTE Q — Balance contra inimigos

## 47. Regras gerais

```text
Magia deve abrir counterplay, não ignorar inimigos.
Fire é bom contra vulneráveis a fogo/Burn, mas fraco contra fire/lava.
Ice/Chill ajuda controle leve, mas não deve travar packs indefinidamente.
Lightning pode interromper/counter constructs se vulneráveis, mas não stunna boss sem janela.
Arcane é versátil, mas menos eficiente que counter específico.
Light/Radiant/Purify é forte contra corrupção/undead/shadow se vulnerável, mas não universal.
Corruption/Nyx é risco, não caminho obrigatório.
```

## 48. Validação de playtest

Validar:

```text
spell DPS vs arma física equivalente;
MP sustain em run curta/média/longa;
tempo para matar packs com AoE;
valor real de cura vs poção/comida;
barreira sem substituir Block;
controle sem stunlock;
cast interruptível por enemies;
spells Tier 1 úteis mas não dominantes;
spells Tier 4/Tier 5 fortes mas caras;
capstone Anya forte em suporte, não dano universal;
capstone Senya forte em dano, mas com custo/risco;
wands/scrolls úteis sem invalidar build mágica.
```

---

# PARTE R — Roadmap de specs futuras

```text
spec_magic_spell_action_data_contract.md
spec_magic_spell_shapes_targeting_runtime.md
spec_magic_mp_cost_cast_cooldown_runtime.md
spec_magic_spell_unlocks_tomes_focus_modifiers.md
spec_magic_status_application_and_vulnerability_matching.md
spec_magic_staff_wand_scroll_integration.md
spec_magic_healing_barrier_purification_runtime.md
spec_magic_elemental_damage_runtime.md
spec_magic_hud_cast_preview_feedback.md
spec_magic_save_load_known_spells_slots_cooldowns.md
spec_magic_balance_playtest_profile.md
```

---

# PARTE S — Decisões fechadas

```text
Magia terá lista enxuta, não biblioteca enorme.
Magias são levemente inspiradas em arquétipos de D&D, sem copiar nomes/textos/sistemas.
Deuses podem nomear spells, mas não criam escolas mecânicas próprias.
Tipos mecânicos são DamageType, SpellCategory, SpellShape, Tags e Scaling.
Dano será maioria do kit; cura/buff/utilidade serão poucos e com cooldown/custo.
Spells equipáveis ocupam active slots.
Staff charged canaliza spell/focus e gasta MP.
Wands usam cargas e/ou MP, mas não superam staff + skill tree.
Scrolls são consumíveis e não trivializam boss gates.
Tomes desbloqueiam spells/variantes/modificadores.
Focus altera identidade da spell com tradeoff.
MP Regen natural continua lenta.
Spells fortes precisam de custo, cast time, cooldown, risco ou gating.
Controle forte não existe sem counterplay e resistência.
Corruption/Nyx é risco late/endgame, não core obrigatório.
Eco de Anya não ressuscita em campo e não substitui a Fonte de Anya.
Ruptura de Senya é burst caro, não botão universal de limpar sala.
Vulnerabilidade vem do inimigo; spell só aplica tags.
```

---

# PARTE T — Pendências abertas

```text
Definir valores finais de MagicDamage no Player Derived Attributes.
Definir SpellActionDataSO runtime.
Definir shape preview visual por spell.
Definir se Projétil Arcano é auto-target ou aim projectile no MVP.
Definir quais Tier 5 entram no MVP: recomendação inicial é Eco de Anya + Ruptura de Senya; Véu de Nyx e Martelo de Thoren podem ficar para roadmap late.
Definir tomes/blueprints que desbloqueiam Tier 4/Tier 5.
Definir bestiary hints de vulnerabilidade para spells.
Validar spell list contra enemy roster real e active combat budget.
```
