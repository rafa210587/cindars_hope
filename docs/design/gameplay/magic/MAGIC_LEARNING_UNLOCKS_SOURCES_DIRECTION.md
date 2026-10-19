# Cindar's Hope — Magic Learning, Unlocks & Sources Direction

> **Status:** documento canônico complementar de aprendizado, fontes e desbloqueio de magias  
> **Local:** `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`  
> **Complementa:** `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> **Função:** definir de onde as magias vêm, como são aprendidas, quando ficam disponíveis para active slots, e como pergaminhos, tomes, wands, armas e focuses interagem com o sistema mágico.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados/runtime/UI.

---

## 0. Regra central

Magia **não aparece automaticamente** apenas porque o jogador subiu de nível ou gastou pontos na árvore Magic.

Regra canônica:

```text
Skill tree libera capacidade, eficiência, pré-requisito e domínio.
A magia em si precisa vir de uma fonte: pergaminho, tome/grimório, arma/focus/wand, NPC/quest, Fonte de Anya ou evento narrativo.
```

Consequência:

```text
Ter Magic Tier 2 permite aprender/castar determinadas magias Tier 2.
Mas o jogador ainda precisa encontrar, comprar, receber, estudar ou equipar a fonte da magia.
```

---

# PARTE A — Tipos de fonte de magia

## 1. Fontes permitidas

```text
LearnableScroll
CastScroll
Tome/Grimório
Wand
Staff/Arma imbuída
Focus/Relic
NPC/QuestTeaching
FonteDeAnya/StoryFlag
Boss/LoreUnlock
```

## 2. LearnableScroll

Pergaminho de aprendizado.

```text
Consumido ao estudar.
Se o jogador cumpre pré-requisitos, adiciona SpellId em knownSpellIds.
Depois de aprendido, a magia pode ser equipada em active slot, se cumprir requisitos de MP/skill/equipment.
```

Regras:

```text
Nem todo pergaminho é aprendível.
Pergaminho aprendível deve ter texto/ícone claro: “Ensina magia”.
Se o jogador não cumprir requisito, o pergaminho não deve ser consumido por acidente.
Pode exigir INT/VON mínima, MagicTreePoints, skill node, tome auxiliar ou NPC.
```

## 3. CastScroll

Pergaminho de uso único.

```text
Consumível.
Casta a magia uma vez ou poucas vezes.
Não adiciona a magia em knownSpellIds.
Pode reduzir ou substituir parte do custo de MP, conforme scroll.
```

Regras:

```text
CastScroll permite acesso pontual a magia sem build completa.
CastScroll forte deve ser raro/caro.
CastScroll não deve trivializar boss gate.
CastScroll não deve virar melhor forma permanente de jogar magia.
```

## 4. Tome / Grimório

Fonte persistente de estudo.

```text
Não é consumível comum.
Desbloqueia spell, variante, upgrade ou categoria de spell.
Pode exigir tempo, estação, NPC, INT/VON, MagicTreePoints ou quest.
```

Uso típico:

```text
Tier 3+ spells;
Tier 4 variants;
Tier 5/late spells;
spells de lore;
spells ligadas a Bromecia, Anya, Senya, Nyx, Thoren ou outros deuses.
```

## 5. Wand

Varinha é item de cast, não aprendizado por padrão.

```text
Permite castar spell enquanto equipada/na hotbar.
Usa charges e/ou MP.
Não adiciona spell ao knownSpellIds, salvo exceção explícita.
```

Regras:

```text
Wand é acesso temporário/equipado.
Wand não deve superar staff + spell aprendida + skill tree.
Wand de tier alto pode exigir Uso de Item Mágico.
```

## 6. Staff / arma imbuída

Arma pode carregar uma magia embutida.

```text
A spell fica disponível apenas enquanto a arma/focus está equipado.
Pode aparecer como WeaponSpellSlot, StaffChannelSpell ou ChargedSpell.
Não é aprendida permanentemente por padrão.
```

Exemplos:

```text
Staff com Projétil Arcano básico.
Espada de prata com Luz Menor fraca contra sombra.
Martelo bromeciano com descarga elétrica curta.
Arma de Pedra Negra com efeito de risco/corrupção.
```

Regra:

```text
Arma com spell embutida deve ter custo, cooldown, charges ou tradeoff.
Não deve burlar prerequisitos de spell forte sem gating.
```

## 7. Focus / Relic

Focus modifica magia ou fornece spell condicional.

```text
Pode alterar elemento, custo, cast time, formato ou efeito.
Pode permitir uma spell enquanto equipado.
Pode exigir que a spell já seja conhecida.
```

Regras:

```text
Focus não deve ser fonte universal de todas as magias.
Focus deve ter identidade e tradeoff.
BlackstoneFocus deve carregar risco.
HealingFocus deve favorecer cura/barreira e reduzir dano ofensivo ou ocupar offhand.
```

## 8. NPC/QuestTeaching

NPC ou quest pode ensinar magia.

```text
Adiciona knownSpellIds ou libera LearnableScroll/Tome.
Exige narrativa, reputação, quest ou serviço.
```

Regras:

```text
NPC não deve ensinar todas as magias.
NPCs ensinam temas coerentes com função/lore.
Cidade não deve entregar magia late sem progresso de caverna/lore.
```

## 9. Fonte de Anya / StoryFlag

A Fonte ou eventos de história podem liberar magia.

```text
Podem desbloquear spell, variante ou potência espiritual.
Podem permitir Eco de Anya, purificação, respec ou cura especial.
```

Regra:

```text
Fonte de Anya não vira loja de spells.
A Fonte pode liberar poder por marcos narrativos, não por grind comum.
```

---

# PARTE B — Disponibilidade real da magia

## 10. Estados possíveis de uma spell

```text
Unknown
  jogador sabe que existe ou não; não pode usar.

Discovered
  vista em loja, tooltip, bestiary, NPC ou item; ainda não pode usar.

Learnable
  jogador possui fonte e cumpre pré-requisitos; pode estudar/aprender.

Known
  conhecida permanentemente; pode ser equipada se requisitos atuais forem cumpridos.

ItemProvided
  disponível apenas por item equipado/hotbar: wand, staff, focus, arma imbuída.

ConsumableProvided
  disponível por CastScroll; uso consome item/carga.

LockedByPrerequisite
  fonte existe, mas falta MagicTreePoints, skill node, atributo, story flag, tome ou equipamento.

DisabledByContext
  conhecida/equipada, mas contexto bloqueia: sem MP, cooldown, silence, boss gate rule, item sem carga etc.
```

## 11. Quando pode equipar em active slot

Uma spell pode ocupar active slot se:

```text
SpellState = Known ou ItemProvided;
pré-requisitos atuais estão válidos;
jogador tem MP/capacidade para cast;
a spell não está bloqueada por contexto;
active slot livre existe;
```

CastScroll normalmente:

```text
fica na hotbar/consumível;
não precisa ocupar active spell slot por padrão;
pode ser mapeado para active slot se spec futura permitir.
```

Wand/arma/focus:

```text
pode ocupar slot de equipamento/hotbar;
pode expor uma SpellAction temporária enquanto equipado;
se remover item, a spell sai da lista disponível.
```

---

# PARTE C — Skill tree vs fonte de magia

## 12. O que a árvore Magic faz

A árvore Magic/Arcano deve:

```text
liberar capacidade de aprender tiers maiores;
reduzir custo de MP;
aumentar dano/efeito;
aumentar MP Regen lentamente;
melhorar cura/barreira/purificação;
permitir uso de itens mágicos melhores;
ampliar afinidade elemental;
melhorar resistência à corrupção;
ativar capstones Anya/Senya.
```

A árvore Magic **não deve**:

```text
dar automaticamente todas as spells do tier;
substituir scrolls/tomes/itens;
transformar level up em biblioteca de magia;
remover necessidade de fonte material/narrativa.
```

## 13. Exemplo de acesso correto

```text
Jogador tem 5 pontos em Magic.
Isso abre Tier 2.
Ele encontra LearnableScroll de Cone de Brasa.
Cone de Brasa exige Tier 2 + Afinidade Elemental: Fogo rank 1.
Se cumprir, estuda o pergaminho e aprende a spell.
Depois pode equipar Cone de Brasa em active slot.
```

Exemplo com wand:

```text
Jogador não conhece Raio de Thoren.
Ele encontra ShockWand.
A wand permite castar versão fraca/limitada de raio enquanto tiver charges/MP.
Ao remover a wand, a spell não fica conhecida.
```

Exemplo com tome:

```text
Jogador encontra Grimório de Selos Rúnicos.
Ele não aprende uma spell imediatamente.
O tome desbloqueia a possibilidade de estudar Selo de Silêncio Rúnico se cumprir Magic Tier 4 + Uso de Item Mágico rank 3.
```

---

# PARTE D — Campos de dados necessários

## 14. SpellUnlockSourceSO

```text
SourceId
SourceType
ProvidedSpellIds
LearnedSpellIds
RequiredMagicTreePoints
RequiredSkillNode
RequiredSkillRank
RequiredAttributes
RequiredStoryFlags
RequiredEquipmentTags
ConsumesSourceOnLearn
ConsumesSourceOnCast
Charges
MPCostOverride
CooldownOverride
PowerModifier
CanEquipInActiveSlot
CanUseFromHotbar
CanTeachPermanently
FailureBehavior
DebugTags
```

## 15. ItemDefinitionSO / EquipmentData extras

Itens mágicos devem poder apontar para spells:

```text
ProvidedSpellIds
LearnedSpellIds optional
SpellUnlockSourceId
WandSpellProfileId
ScrollSpellProfileId
TomeSpellUnlockId
FocusSpellModifierId
WeaponSpellSlotIds
Charges
RequiresKnownSpell boolean
```

## 16. Player magic save state

Persistir:

```text
knownSpellIds
knownSpellVariants
discoveredSpellIds optional
equippedSpellSlots
studiedTomeIds
consumedLearnableScrollIds optional
magicStoryFlags
```

Não persistir como fonte primária:

```text
spell power final;
MP cost final;
status chance final;
tooltip final;
```

---

# PARTE E — Decisões fechadas

```text
Magia não é concedida automaticamente só por level ou skill point.
Skill tree libera capacidade/domínio; fonte libera spell.
Pergaminho pode ser de aprendizado ou de cast consumível.
LearnableScroll ensina permanentemente se pré-requisitos forem cumpridos.
CastScroll casta e consome, mas não ensina.
Tome/Grimório desbloqueia spell, variante ou upgrade persistente, normalmente com requisitos.
Wand fornece cast enquanto equipada/na hotbar, por charges e/ou MP; não ensina por padrão.
Staff/arma imbuída pode fornecer spell temporária enquanto equipada.
Focus pode modificar spell conhecida ou fornecer spell condicional com tradeoff.
NPC/quest/Fonte podem ensinar ou liberar spells, mas não viram biblioteca universal.
knownSpellIds é a fonte de spells permanentes aprendidas.
ItemProvided e ConsumableProvided são estados temporários separados de Known.
```

---

# PARTE F — Pendências para specs futuras

```text
Criar SpellUnlockSourceSO.
Adicionar campos de spell source em ItemDefinitionSO, WandDataSO, ScrollDataSO, TomeDataSO, FocusDataSO e WeaponDataSO.
Criar UI de estudar pergaminho sem consumo acidental se faltar pré-requisito.
Criar UI que diferencia “usar pergaminho” de “aprender magia”.
Criar estado Known vs ItemProvided vs ConsumableProvided.
Criar save/load de knownSpellIds, studiedTomeIds e equippedSpellSlots.
Criar validação: nenhuma spell entra em active slot sem Known ou ItemProvided.
Criar validação: scroll consumível não adiciona knownSpellIds.
Criar validação: wand/weapon spell some ao desequipar item.
Atualizar specs de magic runtime, inventory, equipment, loot/crafting e HUD para ler este documento.
```
