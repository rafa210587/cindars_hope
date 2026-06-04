# Cindar's Hope — Equipment, Weapons, Armor & Materials Direction

> **Status:** documento canônico de direção de equipamentos, armas, armaduras e materiais  
> **Local:** `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> **Função:** definir como equipamentos, armas, armaduras, escudos, acessórios, materiais, tiers, resistência, durabilidade, crafting e loot devem funcionar sem quebrar o balanceamento de player, inimigos e caverna.  
> **Não é spec implementável.** Este documento define direção de design. Specs futuras devem converter isto em dados, ScriptableObjects e sistemas.

---

## 0. Regra anti-duplicação

Este documento não é a fonte primária de fórmulas gerais de atributos derivados.

Fontes canônicas:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  fórmulas de HP, MP, Stamina, Stamina Regen, AttackDamage, Defense, BlockImpact etc.

COMBAT_CORE_DIRECTION.md
  feeling de combate, inputs, Dash, Dodge, Block, movimento, janelas, HUD e telemetria.

ENEMY_BEHAVIORS_DIRECTION.md
  taxonomia de inimigos, EnemyAction, BehaviorProfiles, Traits, Stamina/MP inimiga e reações.

CAVE_MONSTER_ROSTER_DIRECTION.md
  monstros concretos, stats, drops, packs, bosses e scaling da caverna.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades, critical windows, active combat budget, TTK e telemetria de combate na caverna.
```

Regra:

```text
Este documento define o que equipamento deve afetar e como deve se relacionar com balance.
Fórmulas finais continuam em Player Derived Attributes ou specs futuras específicas.
Se houver conflito de cálculo, o documento de atributos derivados vence.
Se houver conflito de vulnerabilidade/inimigos, documentos de caverna/enemies vencem.
```

---

# PARTE A — Visão geral

## 1. Filosofia de equipamento

Equipamento não deve ser apenas `+dano` ou `+armor`.

Ele deve alterar:

```text
estilo de combate
custo de Stamina
risco/recompensa
Block
Dodge/Dash indiretamente por peso
resistências
vulnerabilidades exploráveis
qualidade da run
interação com inimigos
interação com crafting/mineração/fazenda
```

Equipamento bom deve criar decisão:

```text
mais dano, mas mais peso/custo de Stamina
mais armor, mas menos mobilidade/regeneração
mais Block, mas menor velocidade/recovery
mais magia, mas menor defesa física
mais loot/utility, mas menor poder direto
mais poder corrompido, mas custo/risco espiritual
```

Equipamento ruim seria:

```text
sempre equipar o maior tier sem decisão
armadura pesada sem penalidade
arma rápida com dano alto e custo baixo
escudo que trivializa BlockImpact
material raro que ignora todos os inimigos
resistência ampla demais que anula biomas/bosses
```

## 2. Relação com balance de inimigos

Todo item deve ser pensado contra inimigos reais.

Cada família de inimigo deve ter pelo menos uma resposta de equipamento plausível, mas nenhuma resposta deve trivializar a família inteira.

Exemplos:

```text
Undead/Sombras
  Prata, Light/Radiant, blunt em ossos, itens de purificação.

Constructs/Bromecianos
  Hammer, Pickaxe, Lightning, Water em alguns casos, heavy attacks, DurabilityStress reverso futuro.

Fungal/Raiz-Negra
  Axe, Sword, Fire, Light/Radiant, resistência a poison/root.

Beasts/Predadores
  Spear, Bow, bleed, armor leve/média para mobilidade.

Orcs de Kaand
  armor física, anti-stagger, Ice/Water, armas de posture.

Duergar/Gelo
  Fire, Lightning, armor contra frio/chill, blunt/heavy.

Aberrants/Observadores
  Bow/Staff, Light/Radiant, mental resistance, ranged precision por janela.

Dracônicos/Pedra Negra
  Spear/Bow contra asa/ponto exposto, Light/Radiant, armor elemental, resistance a corruption.
```

Regra:

```text
Equipamento deve ampliar counterplay.
Equipamento não deve substituir leitura de telegraph, posicionamento, Stamina e janelas.
```

---

# PARTE B — Slots e categorias

## 3. Slots do personagem

Slots direcionais:

```text
MainHand
OffHand
ArmorBody
Head futuro/opcional
Accessory1
Accessory2
ToolSlot
HotbarConsumables
ActiveSkillSlots separados
```

Regra:

```text
Armas, ferramentas e escudos competem por mãos/slots.
Active skills não são equipamento.
Dash, Dodge e Block não ocupam active slot.
```

## 4. Categorias de equipamento

```text
Weapons
  Sword, Axe, Hammer, Spear, Dagger, Bow, Staff.

OffHand
  Shield, Focus, Dagger offhand futuro, utility item futuro.

Armor
  LightArmor, MediumArmor, HeavyArmor, Robe/ArcaneArmor.

Accessories
  Ring, Amulet, Charm, Relic, Belt futuro.

Tools
  Hoe, AxeTool, Pickaxe, WateringCan, FishingRod, HammerTool futuro.

Consumables
  Food, Potion, Oil, Antidote, Bomb leve futura, Fruto de Mana raro.
```

---

# PARTE C — Armas

## 5. Papéis das armas

| Arma | Papel | Forte contra | Fraca/risco |
|---|---|---|---|
| Sword | equilíbrio | humanoides, aberturas médias | não domina armor/posture |
| Axe | dano forte e corte | raízes, fungais, madeira, armor média | recovery maior |
| Hammer | posture/armor/construct | constructs, shields, ossos, tanks | lento e caro |
| Spear | alcance/pierce | beasts, dracônicos, asas, linhas | ruim cercado |
| Dagger | velocidade/crítico | alvos marcados/vulneráveis | baixo alcance e risco alto |
| Bow | distância/flying | voadores, casters, olhos, kiting controlado | pressão melee/swarms |
| Staff | magia/suporte | elementais, corruptos, suporte/cura | depende de MP/cast |
| ToolAttack | emergência/utilidade | nodes, constructs leves, flavor | não substitui arma dedicada |

## 6. Sword

Direção:

```text
Arma baseline.
Boa para jogador generalista.
Custo de Stamina médio.
Boa em MinorOpening e CriticalWindow.
Não deve superar hammer em posture nem dagger em crit.
```

Uso contra inimigos:

```text
bom contra goblins, kobolds, humanoides médios, beasts comuns.
aceitável contra fungais se não houver axe.
limitado contra constructos/tanks sem skill ou material adequado.
```

## 7. Axe

Direção:

```text
Mais dano por golpe que sword.
Mais recovery e custo.
Melhor contra madeira, raízes, fungais, shields leves e armor média.
Conversa com farm/crafting por lenhador/ferramentas, mas arma e ferramenta devem ser itens diferentes ou modos distintos.
```

Uso contra inimigos:

```text
forte contra Raiz-Negra, rootsnare, fungal bulwark, estruturas orgânicas.
menos eficiente contra constructs duros do que hammer/pickaxe.
```

## 8. Hammer

Direção:

```text
Baixa velocidade.
Alto posture damage.
Alto custo de Stamina.
Forte contra shields, armor, constructs, ossos, guard break.
Deve recompensar timing e CriticalWindow.
```

Uso contra inimigos:

```text
forte contra constructs, duergar blindado, mortos-vivos ósseos, tanks.
ruim contra swarms rápidos se o jogador não tiver controle/companion/pet.
```

## 9. Spear

Direção:

```text
Alcance e controle de linha.
Bom para manter distância curta.
Pode ter thrust/charged thrust.
Pode explorar wings/exposed points sem sistema de mira por parte corporal rígida.
```

Uso contra inimigos:

```text
bom contra beasts, chargers, dracônicos, inimigos em linha, voadores baixos.
fraco se cercado por swarms/flankers.
```

## 10. Dagger

Direção:

```text
Alta cadência.
Baixo alcance.
Baixo dano base.
Alta sinergia com crit, marked target, vulnerability, dodge follow-up e backstab futuro.
Custo por ataque pode ser menor que sword, mas spam ainda deve drenar Stamina.
```

Uso contra inimigos:

```text
bom contra casters frágeis, alvos marcados, humanoides isolados.
ruim contra armor/tank/construct sem material/skill.
```

## 11. Bow

Direção:

```text
Controle de distância.
Charged shot para dano/interrupt.
Requer linha de visão.
Não deve permitir kite infinito.
Custo de Stamina ou ammo/carga deve existir conforme spec final.
```

Uso contra inimigos:

```text
forte contra floating/flying, casters, olhos, inimigos com CoreExposed à distância.
pressionado por PackFlanker, SwarmErratic, Leaper e Line of Sight.
```

## 12. Staff / foco mágico

Direção:

```text
Amplifica magia, MP, cast, cura, barreira ou elementos.
Pode ter ataque básico fraco ou projétil arcano.
Não deve substituir todas as armas físicas.
```

Uso contra inimigos:

```text
forte contra elementais, corrupção, sombras e enemies vulneráveis a magia.
limitado se MP baixo, cast interrompido ou inimigo resistente.
```

---

# PARTE D — Offhand, escudos e Block

## 13. Escudos

Escudos são equipamento de Block.

Devem afetar:

```text
BlockPower
BlockStability
BlockImpactStaminaCost indiretamente
GuardBreakResistance
PostureResistance
peso
movimento
StaminaRegen
```

Regra:

```text
Escudo forte não deve zerar custo de Block.
Escudo pesado protege mais, mas deve reduzir mobilidade ou regen.
Escudo leve permite reação, mas aguenta menos impacto.
```

## 14. Tipos de escudo

| Tipo | Papel | Tradeoff |
|---|---|---|
| Buckler | perfect block/counter | baixa proteção bruta |
| Round Shield | equilíbrio | médio em tudo |
| Tower Shield | proteção pesada | peso, mobilidade, regen menor |
| Arcane Ward Focus | block mágico/barreira | depende de MP/Vontade |
| Thorn/Spiked Shield futuro | dano refletido leve | menor estabilidade |

## 15. Relação com inimigos

```text
GuardBreak enemy actions devem punir Block previsível.
Swarm não deve quebrar escudo sozinho, mas pode cercar.
Casters podem usar zona/ângulo para contornar shield.
Tanks/bosses drenam Stamina por impacto pesado.
Armor reduz dano pós-armadura e reduz dreno por BlockImpact.
```

Fonte de fórmula de BlockImpact:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

---

# PARTE E — Armaduras

## 16. Tipos de armor

| Tipo | Protege | Penaliza | Build típica |
|---|---|---|---|
| LightArmor | mobilidade, Dodge/Dash, menor peso | baixa armor | ranged, dagger, survival |
| MediumArmor | equilíbrio | penalidade leve | sword/spear/generalista |
| HeavyArmor | armor, block, posture | movimento, StaminaRegen, Dash/Dodge comfort | hammer, shield, tank |
| Robe/ArcaneArmor | MP, magia, resistências espirituais | baixa defesa física | staff, support, magic |

## 17. Armor e inimigos

Armadura deve responder a famílias inimigas:

```text
Physical armor
  útil contra humanoides, beasts, orcs, duergar e ataques comuns.

Elemental armor
  útil por bioma: frio, fogo, poison, lightning, acid.

Spiritual/mental armor
  útil contra sombras, Nyx, aberrants, fear/confusion/corruption.

Blackstone resistance
  rara, perigosa ou late game; não deve trivializar Pedra Negra.
```

Regra:

```text
Nenhuma armor deve resolver todos os biomas e famílias ao mesmo tempo.
Resistências fortes devem ter escopo claro.
```

## 18. Penalidades de peso

Peso deve afetar:

```text
MovementSpeed
Dash comfort
Dodge recovery
StaminaRegen
Block stability positiva se armor/escudo pesado
Stamina cost de algumas ações
```

Regra:

```text
Armadura pesada é boa para tomar/blockar impacto.
Armadura leve é boa para não estar onde o impacto acontece.
```

---

# PARTE F — Acessórios e relíquias

## 19. Acessórios

Acessórios devem dar especialização, não substituir arma/armor.

Tipos:

```text
Ring
Amulet
Charm
Relic
Belt futuro
Totem futuro
```

Efeitos possíveis:

```text
+MP
+Stamina Max pequeno
+resistência específica
+crit chance condicional
+loot chance específico
+menor cansaço em run
+melhor cura recebida
+melhor cooking/food effect
+afinidade elemental
+detecção de treasure trap futura
```

Regra:

```text
Acessório não deve dar bônus amplo demais.
Preferir bônus condicional ou especializado.
```

## 20. Relíquias divinas

Relíquias podem ter ligação com deuses, sem transformar o jogador automaticamente em devoto.

Exemplos direcionais:

```text
Kanthor
  block, equilíbrio, justiça, anti-stagger, proteção.

Kaand
  dano, crit, postura, risco, perda defensiva.

Anya
  cura, purificação, Fonte, proteção espiritual, recuperação.

Senya
  dano mágico, caos controlado, elemental, overload.

Telisandra
  sobrevivência, vento, último fôlego, resistência ambiental.

Thoren
  forja, durabilidade, crafting, armor/weapon quality.

Finan
  sorte, loot, gold, chance pequena de evento positivo.

Nyx
  sombra, risco, corrupção, mental/void, alto custo narrativo/mecânico.
```

Regra:

```text
Relíquia divina forte deve ter tradeoff, condição, cooldown ou escopo.
```

---

# PARTE G — Materiais e tiers

## 21. Tiers de material

Tiers direcionais:

```text
Tier 0 — Improvisado / Madeira / Pedra
Tier 1 — Cobre / Osso comum / Couro simples
Tier 2 — Ferro
Tier 3 — Aço
Tier 4 — Prata / Aço refinado / Couro reforçado
Tier 5 — Mithril / Cristal arcano / Liga bromeciana
Tier 6 — Meteórico / Mana / Pedra Negra estabilizada / Relíquias divinas
```

Regra:

```text
Tier maior não deve ser sempre melhor em tudo.
Tier maior deve abrir identidade, eficiência, resistência ou especialização.
```

## 22. Materiais comuns

| Material | Identidade | Uso |
|---|---|---|
| Madeira | inicial, leve, frágil | bow, staff simples, tools |
| Pedra | improvisado, pesado | hammer/tool inicial |
| Cobre | early craft | ferramentas/armas iniciais |
| Ferro | confiável | baseline mid early |
| Aço | forte e comum avançado | armas/armor principais |
| Couro | leve/mobilidade | light armor |
| Osso | tribal/necromântico leve | weapons, charms, risco/lore |

## 23. Prata

Prata deve ser material anti-espiritual.

Forte contra:

```text
undead
sombras
maldições
algumas criaturas de Nyx
corrupção leve
certos aberrants espirituais
```

Tradeoffs:

```text
menor dano bruto que aço refinado contra alvos comuns
custo maior
menor durabilidade que mithril/aço pesado, se fizer sentido
especialização forte, não material universal
```

## 24. Mithril

Mithril deve ser leve, raro e eficiente.

Efeitos possíveis:

```text
menor peso
menor penalidade de Stamina
melhor recovery
boa durabilidade
boa sinergia com Dash/Dodge e armas rápidas
boa armor leve/média avançada
```

Tradeoffs:

```text
não deve ter sempre o maior dano bruto
não deve ter sempre a maior armor flat
raro e caro
exige cave/mineração avançada
```

## 25. Liga bromeciana

Material técnico/ruína.

Uso:

```text
construct gear
máquinas
armas técnicas
escudos especiais
armaduras com resistência elétrica/física
crafting avançado
```

Forte contra:

```text
constructs
ruínas bromecianas
hazards técnicos
```

Riscos/tradeoffs:

```text
peso médio/alto
requer Engenharia Bromeciana
pode ter manutenção/custo alto
```

## 26. Cristal arcano / Mana

Material mágico.

Uso:

```text
staffs
focos
acessórios
barreiras
MP
cura/suporte
magias elementais
```

Relação com Fruto de Mana:

```text
Fruto de Mana é raro e não deve virar insumo comum de craft em massa.
Itens ligados a Mana devem ser especiais, caros e limitados.
```

## 27. Pedra Negra estabilizada

Material perigoso e late game.

Uso possível:

```text
armas anti-corruptas ou corrompidas
resistência parcial a Blackstone
alto dano mágico/físico
interação com nível 100/101
crafts narrativos
```

Regras:

```text
Pedra Negra bruta não deve ser equipamento seguro.
Pedra Negra estabilizada deve exigir processo, lore, risco e gating.
Não deve trivializar corrupção, bosses ou Anya.
```

---

# PARTE H — Dano, armor, resistências e inimigos

## 28. DamageTypes de equipamento

Tipos direcionais:

```text
Slash
Pierce
Blunt
PhysicalGeneric
Fire
Ice
Lightning
Water/Nature
Poison
Bleed
Arcane
Light/Radiant
Shadow/Nyx
Blackstone/Corruption
```

Regra:

```text
Arma pode ter dano primário e dano secundário.
Dano secundário forte deve ter custo, material, enchant, oil, buff ou condição.
```

## 29. Resistências de equipamento

Resistências devem ser específicas.

```text
PhysicalResistance
FireResistance
IceResistance
LightningResistance
PoisonResistance
BleedResistance
Fear/MentalResistance
CorruptionResistance
HeatResistance
ColdResistance
```

Regra:

```text
Resistência ampla demais quebra bioma e boss.
Preferir peças com 1 resistência forte ou 2 moderadas, não tudo.
```

## 30. Vulnerabilidade explorada por equipamento

Equipamento pode explorar vulnerabilidades de inimigos por:

```text
material
DamageType
AttackType
WeaponType
status aplicado
janela comportamental
consumível aplicado na arma
skill tree
```

Exemplo:

```text
Hammer de aço pesado
  bom contra construct e armor por Blunt/Posture.

Sword de prata
  bom contra undead/shadow por material.

Bow com flecha de raio
  bom contra flying/construct se houver line of sight e custo.

Staff de Anya
  bom para purificação/cura/barreira, não dano bruto universal.
```

Regra:

```text
Counter de equipamento deve ajudar, não vencer sozinho.
O jogador ainda precisa de Stamina, timing e leitura.
```

---

# PARTE I — Durabilidade e DurabilityStress

## 31. Durabilidade

Durabilidade pode existir, mas não deve ser punição irritante.

Direção:

```text
Equipamentos têm durabilidade ou condição.
Durabilidade cai mais em uso intenso, block pesado, hazards, corrosão ou DurabilityStress.
Item não deve quebrar permanentemente sem sistema claro de reparo.
Preferir estado Damaged/LowDurability a destruição irreversível.
```

## 32. DurabilityStress

Status/efeito aplicado por alguns inimigos/hazards.

Pode afetar:

```text
durabilidade temporária
custo de reparo
eficiência do item até manutenção
chance de jam em equipamento técnico futuro
```

Regras:

```text
DurabilityStress precisa de feedback visual/HUD.
Não deve destruir item sem aviso.
Não deve aparecer cedo demais.
Constructs, acid bodies, corrosive slimes e hazards técnicos são bons candidatos.
```

## 33. Reparo

Reparo deve se conectar com:

```text
Crafting / Produção
Forja Viva de Thoren
oficina
materiais
cidade/ferreiro futuro
loot de cave
```

Regra:

```text
Reparo deve custar recurso/tempo, mas não travar o jogo.
```

---

# PARTE J — Crafting, upgrades e loot

## 34. Crafting de equipamento

Crafting deve depender de:

```text
material base
tier desbloqueado
skill Crafting/Produção
oficina/forja
componentes de caverna
ouro
receita/planta
```

Regra:

```text
Crafting não deve ignorar a caverna.
A maioria da mineração relevante vem da caverna.
Pedreira da fazenda é recurso final/late farm, não fonte principal de mineração.
```

## 35. Upgrades

Upgrades podem melhorar:

```text
dano
posture
peso
custo de Stamina
durabilidade
resistência
BlockPower
BlockStability
MP/cast
crit condicional
```

Regra:

```text
Upgrade não deve melhorar tudo ao mesmo tempo.
Cada upgrade deve ter foco.
```

## 36. Loot da caverna

Drops de inimigos devem alimentar equipamento.

Exemplos:

```text
fungal spores -> poison/root resistance, alchemy, oils
construct cores -> bromecian gear, lightning interactions
undead bone/essence -> silver/holy gear, charms
blackstone shards -> late corruption/anti-corruption crafts
beast hide/claw -> leather, spear/bow, crit/bleed
orc metal scraps -> heavy armor, brutal weapons
aberrant eye/ichor -> arcane/mental resistance, risky gear
```

Regra:

```text
Drops raros devem estar ligados a inimigos que fazem sentido.
Loot não deve ser só genérico.
```

## 37. Equipamentos únicos

Únicos devem ter:

```text
nome próprio
lore curta
efeito específico
tradeoff ou condição
origem clara
não serem craft genérico repetível
```

Exemplos direcionais:

```text
Escudo de Kanthor
  Block forte contra stagger, efeito em perfect block, peso relevante.

Lâmina de Kaand
  dano/crit/posture alto, defesa menor ou custo maior.

Cajado de Anya
  cura/purificação/barreira, dano baixo/moderado.

Foco de Senya
  magia ofensiva forte, custo/risco maior.

Martelo de Thoren
  crafting/reparo/posture, pesado.
```

---

# PARTE K — Progressão e balance por fase

## 38. Early game

Objetivo:

```text
itens simples
decisões claras
poucas resistências
baixo número de efeitos especiais
stamina apertada
inimigos ensinam arma/counterplay
```

Materiais:

```text
madeira
pedra
cobre
couro simples
ferro inicial
```

## 39. Mid game

Objetivo:

```text
especialização por build
armaduras diferenciadas
primeiras resistências importantes
prata contra sombras/undead
armas de aço
início de gear mágico
```

Materiais:

```text
ferro
aço
prata
couro reforçado
cristais simples
componentes de cave
```

## 40. Late game

Objetivo:

```text
counterplay por bioma/boss
builds fortes
mithril
liga bromeciana
arcane gear
resistências específicas
escudos avançados
```

Materiais:

```text
mithril
liga bromeciana
cristal arcano
componentes raros de bosses
mana limitado
```

## 41. Endgame / nível 100-101

Objetivo:

```text
itens únicos
materiais estabilizados
Pedra Negra estabilizada
relíquias divinas
resistências e counters específicos
sem trivializar boss final ou Anya
```

Regra:

```text
Endgame gear pode ser forte, mas bosses finais devem exigir execução, janelas e preparo.
```

---

# PARTE L — HUD, inventory e tooltips

## 42. Tooltip de equipamento

Tooltip deve mostrar de forma clara:

```text
nome
categoria
tier
material
slots
DamageType
AttackSpeed/Recovery se aplicável
StaminaCost modifier
Defense/Armor
BlockPower/BlockStability se aplicável
resistências
peso
durabilidade
requisitos de skill/tier
bônus condicionais
tradeoffs
```

## 43. Comparação de item

Comparação deve destacar:

```text
o que aumenta
o que diminui
mudança de peso
mudança de Stamina
mudança de resistência
mudança de Block/Dodge/Dash comfort
```

Regra:

```text
Não mostrar só setas verdes/vermelhas de dano/armor.
Equipamento é decisão multidimensional.
```

## 44. HUD em combate

HUD pode mostrar:

```text
durabilidade baixa
arma ativa
escudo/block state
status de oil/buff na arma
resistência temporária ativa
Stamina insuficiente por peso/custo
```

Regra:

```text
Não poluir tela pequena.
Alertas devem aparecer só quando relevantes.
```

---

# PARTE M — Save/load

## 45. Salvar equipamento

Salvar:

```text
item instance id
base item id
material/tier
upgrade level
durability/current condition
affixes/modifiers se existirem
slot equipado
hotbar position
custom name futuro, se existir
```

Recalcular ao carregar:

```text
derived stats
AttackDamage
Defense
BlockPower
BlockStability
resistências
peso
Stamina modifiers
MP/cast modifiers
```

Regra:

```text
Save não deve persistir valor derivado que pode ser recalculado, salvo cache/debug explicitamente necessário.
```

---

# PARTE N — Data assets futuros

## 46. Assets esperados

```text
EquipmentItemSO
WeaponDataSO
ArmorDataSO
ShieldDataSO
AccessoryDataSO
ToolDataSO
MaterialDataSO
EquipmentTierSO
UpgradeRecipeSO
RepairRecipeSO
LootTableSO
EquipmentSetSO futuro
UniqueEquipmentSO
EquipmentAffixSO futuro
```

## 47. EquipmentItemSO conceitual

```text
ItemId
DisplayName
Description
Category
Slot
Tier
MaterialId
Rarity
WeightClass
DurabilityMax
AllowedUpgrades
BaseStats
Modifiers
Resistances
DamageTypes
SkillRequirements
CraftingRequirements
RepairProfile
Tags
```

## 48. WeaponDataSO conceitual

```text
WeaponType
BaseDamage
DamageTypePrimary
DamageTypeSecondary
AttackSpeedProfile
LightAttackCostModifier
HeavyAttackCostModifier
PostureDamage
CritChanceModifier
CritDamageModifier
RangeProfile
StaminaWeightClass
AllowedMaterials
AllowedWeaponActions
```

## 49. ArmorDataSO conceitual

```text
ArmorType
ArmorFlat
PhysicalResistance
ElementalResistances
Mental/SpiritualResistances
WeightClass
MovementPenalty
StaminaRegenPenalty
DodgeRecoveryModifier
DashComfortModifier
BlockStabilityModifier
DurabilityProfile
```

## 50. MaterialDataSO conceitual

```text
MaterialId
Tier
MaterialFamily
DamageModifier
ArmorModifier
WeightModifier
DurabilityModifier
StaminaCostModifier
ResistanceModifiers
EnemyFamilyBonus
EnemyFamilyPenalty
CraftingRarity
SourceTags
LoreTags
```

---

# PARTE O — Decisões fechadas

```text
Equipamento não é só +dano/+armor.
Todo equipamento relevante deve ter tradeoff ou especialização.
Balance de equipamento deve considerar famílias reais de inimigos e vulnerabilidades.
Armadura pesada protege mais, mas penaliza movimento/StaminaRegen/Dash-Dodge comfort.
Armadura leve protege menos, mas favorece mobilidade.
Escudo forte melhora Block, mas não remove BlockImpact.
Dash longo continua build de mobilidade; equipamento pesado deve prejudicar essa fantasia.
Prata é anti-undead/sombra/corrupção leve, não material universal.
Mithril é leve/eficiente, não necessariamente maior dano/armor bruto.
Pedra Negra estabilizada é late/endgame e perigosa; não deve trivializar corrupção.
Fruto/Mana não vira insumo comum de craft em massa.
Durabilidade pode existir, mas item não deve quebrar permanentemente sem sistema claro de reparo.
Ferramenta pode atacar emergencialmente, mas não substitui arma dedicada.
A maioria da mineração relevante vem da caverna; pedreira da fazenda é late/final farm.
```

---

# PARTE P — Pendências para specs futuras

```text
Definir EquipmentItemSO contract.
Definir WeaponDataSO contract.
Definir ArmorDataSO contract.
Definir ShieldDataSO contract.
Definir MaterialDataSO contract.
Definir DamageType final em contrato de combate.
Definir WeaponActionDataSO integrado com Combat Core.
Definir stamina cost modifiers por weapon/armor/material.
Definir armor weight penalties e caps.
Definir shield BlockPower/BlockStability e interação com BlockImpact.
Definir durability e repair system.
Definir upgrade/crafting recipes.
Definir loot tables de componentes por enemy family.
Definir unique equipment rules.
Definir tooltip/inventory comparison UI.
Definir save/load de equipment instances.
Validar balance contra enemy families, packs e bosses em Play Mode.
```
