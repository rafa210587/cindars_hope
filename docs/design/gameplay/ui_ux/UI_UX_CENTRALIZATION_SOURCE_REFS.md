# Cindar's Hope — UI/UX Centralization Source References

> **Status:** mapa auxiliar de centralização de UI/UX  
> **Local:** `docs/design/gameplay/ui_ux/UI_UX_CENTRALIZATION_SOURCE_REFS.md`  
> **Fonte central:** `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> **Função:** registrar onde existiam decisões locais de UI/HUD/feedback e como elas devem ser tratadas depois da centralização.  
> **Não é spec implementável.**

---

## 0. Regra principal

A partir da criação de `UI_UX_FULL_GAMEPLAY_DIRECTION.md`, todo documento de sistema deve seguir a separação:

```text
Documento de sistema:
  define o que precisa ser comunicado.

UI_UX_FULL_GAMEPLAY_DIRECTION.md:
  define como comunicar, onde comunicar, prioridade visual, layout, input, foco, navegação e feedback.
```

Se houver conflito:

```text
O documento de sistema vence para significado mecânico.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para apresentação, layout, modal, foco, input routing e feedback.
```

---

## 1. Fonte central obrigatória

Toda spec que envolva UI, HUD, menus, input, foco, feedback, tooltip, navegação, modal, diálogo, inventário, shop, crafting, skill tree, quest log, social log, calendário, cave HUD, combat feedback, Fonte UI, pet/companion UI ou debug HUD deve ler:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

---

## 2. Conteúdo centralizado a partir de Player Core

Arquivo de origem:

```text
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
```

Conteúdo local identificado:

```text
HUD base:
  HP
  MP quando relevante
  Stamina
  Fome
  Cansaço
  hotbar
  4 active slots
  indicador de Dash/Dodge/Block
  status negativos
  buffs
  arma/ferramenta ativa
  pet/companion status quando relevante

Não mostrar:
  Breath
  Fôlego
  BR
```

Tratamento após centralização:

```text
Player Core mantém o que precisa existir no HUD do personagem.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para layout, prioridade visual, foco, input, modal, tamanho, navegação e feedback.
```

Referência canônica no UI/UX central:

```text
Seções 3, 4, 5, 17, 18, 21 e 24 de UI_UX_FULL_GAMEPLAY_DIRECTION.md.
```

---

## 3. Conteúdo centralizado a partir de Combat Core

Arquivo de origem:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
```

Conteúdo local identificado:

```text
HUD de combate:
  HP
  MP quando relevante
  Stamina
  Fome/Cansaço compacto
  active slots
  hotbar
  Dash/Dodge feedback
  Block state
  status negativos
  buffs
  arma/ferramenta ativa
  companion/pet state quando relevante

Não mostrar:
  Breath/Fôlego como atributo/recurso
  BR
  números internos demais na HUD principal

Feedback visual:
  hit recebido
  block bem sucedido
  block quase quebrando
  Stamina insuficiente
  MinorOpening
  CriticalWindow
  CoreExposed
  vulnerabilidade elemental
  status aplicado
  boss phase transition
  companion setup
  pet interrupt

Feedback sonoro:
  windup de elite/boss
  block impact
  Stamina baixa
  critical hit
  vulnerability hit
  cast perigoso
  pet/companion trigger
```

Tratamento após centralização:

```text
Combat Core mantém quais sinais de combate são necessários.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para apresentação, prioridade, legibilidade, HUD layout, áudio/visual feedback, modal e debug separation.
```

Referência canônica no UI/UX central:

```text
Seções 17, 18, 19, 20, 21, 22 e 24 de UI_UX_FULL_GAMEPLAY_DIRECTION.md.
```

---

## 4. Conteúdo centralizado a partir de Social Relationship Romance

Arquivo de origem:

```text
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

Conteúdo local identificado:

```text
HUD e feedback social:
  ícone de relação no social log
  texto de reação ao presente
  ícone de diálogo novo
  marcador de quest pessoal
  convite recebido
  calendário de aniversário/evento
  notificação de visita
  feedback de partner helper
  registro de preferências descobertas
  status de parceiro/companions desbloqueado
  status de parceiro na polycule

Evitar:
  barra gigante sempre visível
  spam de números
  feedback que transforme NPC em planilha
  feedback ambíguo quando presente é ofensivo
  ícone romântico em NPC bloqueado por idade/narrativa
```

Tratamento após centralização:

```text
Social Relationship mantém quais feedbacks sociais existem.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para social log, notificações, ícones, prioridade visual, modal, tooltip, calendário e UX de apresentação.
```

Referência canônica no UI/UX central:

```text
Seções 13, 14, 15, 19, 20, 22 e 24 de UI_UX_FULL_GAMEPLAY_DIRECTION.md.
```

---

## 5. Conteúdo centralizado a partir de Main Quest Progression

Arquivo de origem:

```text
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Conteúdo local identificado:

```text
Cada fragmento deve ter:
  efeito visual na Fonte
  mudança perceptível no mundo

Fonte por estágio:
  respawn
  Água Viva
  respec
  purificação/cura avançada
  decisão final

Eventos/feedbacks:
  Fonte reage
  água volta a circular
  símbolos nymirianos aparecem
  reflexos mostram cenas antigas
  NPCs esquecem detalhes
  sonhos sob Alihana
  loja noturna muda
  gato reage a Nyx
  Fragmento da Esperança libera decisão final
```

Tratamento após centralização:

```text
Quest Progression mantém significado narrativo e progressão dos fragmentos.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para Fonte UI, quest log, notificações, feedback de evento, modal de decisão final e spoiler control.
```

Referência canônica no UI/UX central:

```text
Seções 13, 16, 19, 20 e 24 de UI_UX_FULL_GAMEPLAY_DIRECTION.md.
```

---

## 6. Conteúdo centralizado a partir de Farm Design

Arquivo de origem:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
```

Conteúdo local identificado:

```text
UI agrícola mais clara.
Feedback visual de solo seco/molhado/plantado/pronto.
SellPoint/caixa de venda.
Pedidos/encomendas simples.
Baús/storage.
Cansaço, fome, Stamina e rotina.
Fonte de Anya visível.
```

Tratamento após centralização:

```text
Farm Design mantém quais estados agrícolas precisam existir.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para prompts, feedback visual, HUD agrícola, inventário, shipping/sell feedback, storage UI, Fonte UI e notificações.
```

Referência canônica no UI/UX central:

```text
Seções 4, 6, 10, 11, 16, 19, 20 e 24 de UI_UX_FULL_GAMEPLAY_DIRECTION.md.
```

---

## 7. Conteúdo centralizado a partir de City Design

Arquivo de origem:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
```

Conteúdo local identificado:

```text
NPCs disponíveis ou indisponíveis para romance de forma clara.
Serviços úteis.
Rumores.
Festivais.
Visitas à fazenda.
Respostas ao progresso do jogador.
Templo, lojas, altares e cidade como hub social/econômico/narrativo.
```

Tratamento após centralização:

```text
City Design mantém o que a cidade precisa comunicar.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para diálogo, prompts, shop UI, social log, quest log, calendar, rumors UI, service UI e markers.
```

Referência canônica no UI/UX central:

```text
Seções 9, 10, 13, 14, 15, 19, 20 e 24 de UI_UX_FULL_GAMEPLAY_DIRECTION.md.
```

---

## 8. Conteúdo centralizado por domínio adicional

### Equipment

```text
Equipment documents definem dados de arma/armadura/material/durabilidade.
UI_UX_FULL_GAMEPLAY_DIRECTION.md define equipment UI, tooltip, comparação e apresentação de durabilidade.
```

Referência:

```text
Seções 7 e 8.
```

### Magic

```text
Magic documents definem spell, source, unlock, MP, shape, cooldown, cast time.
UI_UX_FULL_GAMEPLAY_DIRECTION.md define tooltip de magia, slot feedback, MP insuficiente, cooldown e cast feedback.
```

Referência:

```text
Seções 5, 8, 18 e 19.
```

### Companions

```text
Companions documents definem companion state, active companion, bond, cave/farm behavior.
UI_UX_FULL_GAMEPLAY_DIRECTION.md define companion HUD, notification, prompt e state presentation.
```

Referência:

```text
Seções 4, 17, 18 e 19.
```

### Pets

```text
Pets documents definem pet bond, mood, energy, hints e cave/farm behavior.
UI_UX_FULL_GAMEPLAY_DIRECTION.md define pet HUD, hints, notifications e cave feedback.
```

Referência:

```text
Seções 4, 17, 18 e 19.
```

### Loot / Crafting / Economy

```text
Loot/Economy documents definem ItemStack, ItemInstance, price, shop, stock, restock, sell point, recipes.
UI_UX_FULL_GAMEPLAY_DIRECTION.md define inventory, tooltip, shop Buy/Sell, crafting UI, empty states e notifications.
```

Referência:

```text
Seções 6, 8, 10, 11 e 19.
```

---

## 9. Refactor posterior recomendado

Quando houver edição segura com patch parcial ou reescrita controlada dos documentos grandes, reduzir seções locais de UI para referências curtas:

```text
Para apresentação, layout, input, foco, modal, feedback visual/sonoro, tooltips, HUD e navegação, ler:
  docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

Prioridade de refactor posterior:

```text
1. PLAYER_CORE_SYSTEMS_DIRECTION.md
2. COMBAT_CORE_DIRECTION.md
3. SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
4. QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
5. FARM_DESIGN_DIRECTION_v1.3.md
6. CITY_DESIGN_DIRECTION_v1.2.md
```

Motivo para não remover diretamente agora:

```text
Esses arquivos são grandes.
A edição via API disponível exige substituir o arquivo completo.
Remover seções internas sem patch parcial seguro aumenta risco de perda de conteúdo canônico.
```

---

## 10. Decisão fechada

```text
UI/UX_FULL_GAMEPLAY_DIRECTION.md é a fonte central de UI/UX.
Este arquivo registra de onde a centralização veio.
Documentos antigos ainda podem conter resumos locais, mas não vencem o UI/UX central em apresentação, input, foco, layout e feedback.
```
