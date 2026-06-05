# Cindar's Hope — UI/UX Menu Screen Flows Direction

> **Status:** direction canônico detalhado de telas, menus, submenus e fluxos derivados de UI/UX  
> **Local:** `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_CENTRALIZATION_SOURCE_REFS.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`  
> **Função:** detalhar todos os menus principais, submenus, painéis laterais, tooltips, estados vazios, confirmações, navegação e fluxos derivados que o direction central de UI/UX ainda não detalhava.  
> **Não é spec implementável.** Specs futuras devem quebrar este direction em telas/flows menores.

---

## 0. Referências de design usadas

Este documento usa referências de gênero sem copiar layout, arte, texto ou assets.

Referências conceituais:

```text
Stardew Valley:
  inventário em grid, hotbar persistente, abas, calendário, skills, social, crafting e loja simples.

Rune Factory / farm RPGs:
  combinação de fazenda, combate, equipamento, skills, magia, crafting e relacionamento.

Terraria / Core Keeper / ARPGs leves:
  leitura rápida de item, tooltip, comparação, crafting, raridade e inventário funcional.

RPGs com skill trees:
  painel de node, pré-requisito, custo, preview de rank, confirmação e equipar active skill.
```

Princípios extraídos:

```text
grid é bom para inventário;
abertura por abas é boa para menus grandes;
item selecionado deve abrir detalhe claro;
comparação deve ser acessível sem equipar acidentalmente;
shop deve separar compra e venda;
crafting precisa mostrar material faltante;
skill tree precisa explicar requisito e ganho antes de gastar ponto;
calendário precisa separar informação conhecida de segredo;
menus não devem deixar input de gameplay vazar.
```

---

## 1. Relação com o UI/UX central

`UI_UX_FULL_GAMEPLAY_DIRECTION.md` continua sendo a fonte central para:

```text
input routing;
foco;
modal;
bloqueio de WASD/gameplay input;
HUD principal;
HUD de combate;
tooltips;
notificações;
layout geral;
debug HUD;
acessibilidade básica.
```

Este documento detalha:

```text
telas concretas;
submenus;
painéis laterais;
drawers;
abas;
fluxo de confirmação;
ordem de foco por tela;
empty states;
comparações;
previews;
ações permitidas;
ações proibidas;
interação entre menus.
```

Regra:

```text
UI_UX_FULL_GAMEPLAY_DIRECTION.md define princípios.
UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md define fluxos e conteúdo de tela.
Specs implementáveis definem componentes, prefabs, bindings e runtime.
```

---

## 2. Taxonomia de telas

### 2.1 Tela modal completa

Usar para sistemas que exigem foco total.

Exemplos:

```text
Inventory
Equipment
Skill Tree
Quest Log
Social Log
Calendar
Map futuro
Pause/System
```

Regras:

```text
tempo pausa por padrão;
input de gameplay bloqueado;
ESC/B fecha ou volta camada;
WASD/arrow navega UI;
Confirm atua no elemento focado;
Cancel volta sem executar ação de mundo.
```

### 2.2 Tela modal dividida

Usar quando há duas fontes de dados.

Exemplos:

```text
Shop Buy/Sell
Crafting Station
Storage/Chest
Repair/Upgrade
```

Regras:

```text
a fonte esquerda e direita devem ser claras;
foco atual deve ser visível;
transferência de item deve ter feedback;
empty state deve ser explícito;
quantidade e preço total devem ser visíveis quando aplicável.
```

### 2.3 Drawer / painel lateral

Usar para detalhes de elemento selecionado.

Exemplos:

```text
item detail drawer;
weapon detail drawer;
skill node detail drawer;
spell detail drawer;
quest detail drawer;
NPC detail drawer;
calendar day detail drawer.
```

Regras:

```text
não deve cobrir a lista principal sem necessidade;
deve atualizar ao mover seleção;
deve ter ação principal clara;
deve ter ações secundárias separadas;
texto longo deve ter scroll próprio.
```

### 2.4 Tooltip flutuante

Usar para informação curta.

Exemplos:

```text
item rápido;
status effect;
buff;
slot de active skill;
ícone de clima/lua;
moeda;
material faltante.
```

Regras:

```text
não usar tooltip flutuante para texto longo;
não esconder informação crítica apenas em hover;
controle por teclado/gamepad precisa conseguir abrir tooltip/detalhe.
```

### 2.5 Confirmação

Usar para ações irreversíveis ou custosas.

Exemplos:

```text
gastar SkillPoint;
respec;
vender item raro/favorito;
descartar item;
comprar item caro;
upgrade caro;
escolha final de quest;
romance/casamento futuro;
```

Regras:

```text
texto curto;
mostrar custo/consequência;
foco inicial deve ser Cancel/No em ações destrutivas;
foco inicial pode ser Confirm em ação reversível simples;
nunca confirmar ação irreversível com mesmo botão acidental sem pausa visual.
```

---

## 3. Convenções globais de navegação

### 3.1 Teclado/mouse baseline

```text
I = Inventory baseline.
ESC = fechar/voltar/pause conforme foco.
WASD/Arrow = navegação em grid/lista quando modal aberto.
Enter/E/Confirm = confirmar item focado.
Right click / Secondary = ação secundária ou detalhe, se definido.
Tab/QE = trocar aba, se definido.
Mouse hover = tooltip curto.
Mouse click = selecionar.
Double click = não deve executar ação destrutiva por padrão.
```

### 3.2 Gamepad futuro

```text
D-pad/Left stick = mover foco.
A/Cross = confirmar.
B/Circle = voltar/cancelar.
LB/RB = trocar aba.
LT/RT = trocar categoria/página quando necessário.
Y/Triangle = ação secundária/detalhe.
X/Square = quick action quando seguro.
```

### 3.3 Foco visual

Todo foco deve mostrar:

```text
contorno;
brilho sutil;
realce de slot;
drawer atualizado;
ações disponíveis.
```

Não depender apenas de cor.

### 3.4 Foco inicial por tela

```text
Inventory:
  primeiro slot ocupado da hotbar/inventory.

Equipment:
  arma equipada ou slot principal.

Skill Tree:
  última árvore aberta e node selecionado; fallback no primeiro node desbloqueado.

Shop:
  primeira aba disponível, Buy por padrão se loja tem estoque.

Crafting:
  primeira receita craftável; fallback primeira receita conhecida.

Quest Log:
  main quest ativa se houver; fallback primeira quest ativa.

Calendar:
  dia atual.

Social Log:
  último NPC visto; fallback primeiro NPC conhecido.

Fonte Menu:
  primeira função desbloqueada.
```

---

## 4. Inventory Screen

### 4.1 Objetivo

Inventário permite:

```text
ver itens;
organizar itens;
usar itens;
equipar item quando aplicável;
comparar equipamento;
separar stack;
mover para storage;
vender quando em shop;
travar/favoritar item futuro;
ver detalhe de item;
```

### 4.2 Layout recomendado

```text
Topo:
  abas/categorias opcionais.

Centro:
  grid de inventory.

Base:
  hotbar/current quick slots.

Lateral direita:
  item detail drawer.

Rodapé:
  ações disponíveis conforme foco.
```

### 4.3 Categorias iniciais

```text
All
Tools
Weapons
Armor
Consumables
Materials
Crops/Food
Quest/Key
Magic
Pet/Companion
```

Categorias podem ser filtros, não abas físicas obrigatórias.

### 4.4 Slot states

```text
Empty
Occupied
Selected
Equipped
Favorited/Locked future
Quest/Key protected
Stackable
Not usable here
Not sellable
New item
```

### 4.5 Item detail drawer

Deve mostrar:

```text
nome;
ícone;
categoria;
quantidade;
quality;
rarity;
valor base/preço contextual;
descrição curta;
uso principal;
tags principais;
ações disponíveis.
```

Se equipável, adicionar:

```text
comparação com equipado;
slot de equipamento;
material/tier;
durabilidade;
scaling;
resistências ou dano;
interações de vulnerabilidade conhecidas.
```

### 4.6 Ações possíveis

```text
Use
Equip
Compare
Move
Split Stack
Drop
Sell, apenas em contexto de venda
Lock/Favorite future
Inspect
```

### 4.7 Regras anti-erro

```text
Quest/Key item não pode ser vendido ou descartado por padrão.
Item equipado não pode ser vendido sem desequipar ou confirmar.
Item favorito/travado não pode ser vendido sem destravar.
Drop/descarte precisa confirmar se item raro/único/favorito/quest.
Split Stack precisa mostrar quantidade resultante.
```

---

## 5. Storage / Chest Screen

### 5.1 Objetivo

Storage permite mover itens entre inventário do jogador e container.

### 5.2 Layout

```text
Topo ou esquerda:
  container inventory.

Base ou direita:
  player inventory/hotbar.

Lateral:
  item detail drawer.

Rodapé:
  actions: move one, move stack, split, sort, close.
```

### 5.3 Regras

```text
Deve ficar claro qual grid está focado.
Transferência deve ter feedback imediato.
Container vazio deve mostrar empty state.
Player inventory cheio deve impedir transferência com mensagem clara.
Sort não deve misturar itens protegidos se regra futura existir.
```

---

## 6. Equipment Screen

### 6.1 Objetivo

Equipment Screen permite entender build, equipamento atual e impacto de troca.

### 6.2 Layout recomendado

```text
Lado esquerdo:
  personagem/silhueta e slots equipados.

Centro:
  lista/grid de itens equipáveis filtrados por slot.

Lado direito:
  comparison drawer.

Rodapé:
  equip, unequip, compare, repair/upgrade shortcut quando disponível.
```

### 6.3 Slots previstos

```text
Weapon / Tool active context
Armor
Shield
Accessory 1
Accessory 2
Staff/Focus/Wand quando aplicável
Charm/Relic future
```

Não criar slots antes do sistema existir.

### 6.4 Comparison drawer

Mostrar:

```text
item atual;
item selecionado;
diferença de dano/armor/resistência;
diferença de scaling;
durabilidade atual e máxima;
peso/ASPD/StaminaCost se aplicável;
material/tier/quality/rarity;
efeitos especiais;
known vulnerabilities relevantes;
```

Regras:

```text
ver comparação não equipa;
confirmar equip é ação separada;
se troca remover spell fornecida por item, avisar;
se item exige stat/requisito não cumprido, bloquear e explicar;
```

---

## 7. Weapon / Armor Detail Submenu

### 7.1 Objetivo

Permitir leitura profunda sem poluir tooltip básico.

### 7.2 Abas do detalhe

```text
Overview
Stats
Effects
Material
Durability
Upgrade/Repair, se disponível
Known Enemy Interactions
Source/History future
```

### 7.3 Overview

```text
nome;
ícone;
tipo;
raridade;
qualidade;
resumo de função;
ação principal;
```

### 7.4 Stats

```text
dano/armor;
ASPD;
range;
StaminaCost;
MP interaction se houver;
scaling;
resistências;
block/guard quando escudo;
```

### 7.5 Effects

```text
efeito charged;
status aplicado;
condição de proc;
limitações;
cooldown se houver;
```

### 7.6 Material

```text
material principal;
tier;
interações conhecidas com inimigos;
interações desconhecidas ocultas até descoberta;
```

### 7.7 Durability

```text
durabilidade atual/máxima;
estado visual;
risco de quebrar ou perder eficiência se aplicável;
custo de reparo conhecido;
```

### 7.8 Known Enemy Interactions

Mostrar apenas conhecimento descoberto.

Exemplos:

```text
Eficaz contra família X, descoberto.
Pouco eficaz contra família Y, descoberto.
Interação desconhecida, não revelar.
```

Regra:

```text
Bestiary/Knowledge Discovery futuro vence para regras de descoberta.
UI só apresenta o que já é conhecido.
```

---

## 8. Repair / Upgrade Screen

### 8.1 Objetivo

Permitir reparar e melhorar equipamento com preview claro.

### 8.2 Layout

```text
Item selecionado
Estado atual
Resultado previsto
Materiais necessários
Gold/custo
Station/NPC requerido
Chance/risco, se algum sistema futuro usar
Confirm/Cancel
```

### 8.3 Regras

```text
Reparo não deve ser confundido com upgrade.
Upgrade deve mostrar antes/depois.
Material faltante deve ser destacado.
Item equipado pode ser reparado se sistema permitir.
Item em uso por companion future deve indicar restrição.
Ação custosa exige confirmação.
```

---

## 9. Skill Tree Screen

### 9.1 Objetivo

Permitir entender progressão, gastar pontos, equipar active skills e consultar detalhes.

### 9.2 Layout recomendado

```text
Topo:
  árvores/tabs.

Centro:
  grafo de nodes.

Lado direito:
  node detail drawer.

Base:
  SkillPoints, active slots, ações e legenda.
```

### 9.3 Árvores iniciais

Usar a definição de `PLAYER_SKILL_TREES_DIRECTION.md`.

A UI deve suportar:

```text
5 árvores iniciais;
passivas;
ativas;
ranks;
pré-requisitos;
capstones;
respec pela Fonte;
4 active slots.
```

### 9.4 Node states

```text
LockedUnknown, se oculto por progressão
LockedVisible
Available
Purchased
MaxRank
EquippedActive
BlockedByCapstone
BlockedByRequirement
RefundableWhenRespec
```

### 9.5 Node detail drawer

Deve mostrar:

```text
nome;
ícone;
tipo: passive/active/capstone;
rank atual / rank máximo;
custo em SkillPoints;
pré-requisitos;
efeito do rank atual;
efeito do próximo rank;
condição de uso, se active;
MP/Stamina/cooldown/cast time, se aplicável;
slot ativo, se equipada;
ações disponíveis.
```

### 9.6 Preview antes de comprar

Antes de gastar ponto, mostrar:

```text
custo;
efeito ganho;
novo rank;
se desbloqueia active skill;
se exige equipar em slot;
se afeta stat derivado;
se aproxima de capstone;
```

Ação:

```text
Confirmar compra.
Cancelar sem efeito.
```

### 9.7 Active slot assignment submenu

Quando uma skill active é adquirida:

```text
mostrar slots ativos 1-4;
indicar slots vazios;
indicar skill equipada;
permitir substituir;
confirmar substituição se slot ocupado;
mostrar cooldown/custo resumido;
```

Regra:

```text
Comprar active skill não equipa automaticamente se slots cheios.
Se houver slot vazio, pode sugerir equipar, mas confirmar.
```

### 9.8 Capstones

Capstone detail deve mostrar:

```text
requisito;
exclusividade;
efeito;
consequência de escolha;
confirmação reforçada se irreversível ou custosa.
```

### 9.9 Respec via Fonte

Respec UI deve mostrar:

```text
Fonte stage requerido;
custo;
quantos pontos serão devolvidos;
o que será removido de active slots;
confirmação;
```

Regra:

```text
Respec só aparece quando Fonte desbloquear Fragmento da Memória.
```

---

## 10. Spell / Magic Detail Submenu

### 10.1 Objetivo

Explicar spell, fonte de aprendizado e uso em combate/utility.

### 10.2 Detail content

```text
nome;
ícone;
categoria;
damage/healing/support;
elemento/damage type;
shape/range;
MP cost;
Stamina cost se houver;
cast time;
cooldown;
status aplicado;
scaling;
fonte de unlock;
condição de uso;
variante conhecida;
```

### 10.3 Spell source states

```text
KnownPermanent
KnownVariant
ProvidedByEquippedItem
ConsumableScroll
LearnableScroll
LockedByQuest
LockedByFonte
LockedBySkillTree
Unknown
```

### 10.4 Regras

```text
LearnableScroll ensina permanentemente se requisitos forem cumpridos.
CastScroll casta e consome, mas não ensina.
Wand/Staff/Focus pode fornecer magia temporária enquanto equipado.
Spell fornecida por item deve desaparecer da lista ativa ao desequipar item, salvo se aprendida.
```

### 10.5 Erros claros

```text
MP insuficiente.
Stamina insuficiente.
Alvo inválido.
Requer foco/staff/wand.
Requer Fonte/fragmento.
Requer skill/passive.
Não pode usar nesta área.
```

---

## 11. Shop Buy/Sell Screen

### 11.1 Objetivo

Corrigir o problema de vendedor vazio e venda sem inventário do jogador.

### 11.2 Layout obrigatório

```text
Header:
  nome da loja/NPC;
  gold do jogador;
  modo Buy/Sell.

Buy tab:
  lista/grid de estoque da loja;
  detail drawer do item da loja;
  quantidade;
  preço unitário;
  total;
  stock disponível;
  restock/limited/unique.

Sell tab:
  inventário vendável do jogador;
  detail drawer do item do jogador;
  preço unitário de venda;
  quantidade;
  total;
  motivo se item não vendável.
```

### 11.3 Empty states

```text
Loja sem estoque:
  Esta loja não tem itens disponíveis hoje.

Vendedor não compra categoria:
  Este vendedor não compra este tipo de item.

Player sem item vendável:
  Você não possui itens vendáveis.

Stock esgotado:
  Estoque esgotado até o próximo restock.
```

### 11.4 Regras de compra

```text
não permitir comprar sem gold suficiente;
mostrar quantidade máxima comprável;
mostrar estoque restante;
confirmar compra cara/rara/única;
UniqueStock vendido não reaparece;
LimitedStock respeita counter;
```

### 11.5 Regras de venda

```text
Sell usa inventário do jogador, não estoque da loja.
Item quest/key não aparece ou aparece bloqueado.
Item equipado aparece bloqueado ou exige confirmação de desequipar.
Item favorito/travado future bloqueia venda.
Preço de venda vem da economia, não do preço final persistido.
```

---

## 12. Crafting Screen

### 12.1 Objetivo

Mostrar receitas, requisitos, resultado e estado de craft.

### 12.2 Layout

```text
Lista de receitas/categorias;
recipe detail drawer;
inputs necessários;
quantidade disponível;
resultado;
station;
tempo de craft/processamento;
ações: craft one, craft many, pin recipe future.
```

### 12.3 Recipe states

```text
KnownCraftable
KnownMissingMaterials
KnownMissingStation
KnownLockedBySkill
KnownLockedByQuest
UnknownHidden
ProcessingActive
ReadyToCollect
```

### 12.4 Recipe detail

```text
nome;
ícone;
categoria;
resultado;
quantidade produzida;
material A/B/C;
quantidade possuída vs necessária;
station;
tempo;
qualidade prevista quando aplicável;
uso principal;
```

### 12.5 Regras

```text
Material faltante deve ser explícito.
Craft em massa deve recalcular total.
Recipe locked deve explicar requisito se conhecido.
UnknownHidden não revela spoiler.
Processing timer deve aparecer em station ou log quando aplicável.
```

---

## 13. Quest Log Screen

### 13.1 Objetivo

Organizar objetivos sem revelar spoilers.

### 13.2 Categorias

```text
Main Quest
Side Quest
Social Quest
Farm Order / Encomenda
Companion Quest
Pet/Farm Hint
Completed
Failed/Expired
```

### 13.3 Quest detail drawer

```text
título;
categoria;
resumo curto;
objetivo atual;
local/pista;
NPC relacionado;
item requerido;
estado;
recompensa conhecida/desconhecida;
prazo se houver;
condição temporal se descoberta;
marcador opcional;
histórico breve de etapas concluídas;
```

### 13.4 Main quest rules

Main quest deve mostrar:

```text
Ato atual;
fragmento atual, se descoberto;
estado da Fonte, se conhecido;
objetivo cidade/fazenda/caverna;
condição de lua/clima apenas se descoberta;
sem spoiler de nível 101 cedo;
sem revelar Anya completa porque ela não será restaurada.
```

### 13.5 Quest condition states

```text
Available
Active
WaitingForTime
WaitingForWeather
WaitingForLunarEvent
WaitingForNPC
WaitingForItem
WaitingForCaveDepth
Completed
Failed/Expired
Hidden
```

---

## 14. Social / NPC Detail Screen Future

### 14.1 Objetivo

Preparar social/romance futuro sem implementar runtime agora.

### 14.2 NPC card

```text
nome;
retrato;
ocupação/serviço;
local conhecido;
horário geral conhecido;
estado social geral;
preferências descobertas;
presente dado hoje/semana;
quest pessoal;
romance eligibility quando descoberto;
partner status futuro;
companion eligibility quando descoberto;
```

### 14.3 Regras

```text
Não mostrar NPC como planilha completa.
Preferências são descobertas gradualmente.
Romance bloqueado por idade/narrativa não deve exibir ícone romântico.
Poliamor futuro deve ser estado claro e consentido, sem path obrigatório de poder.
Partner helper futuro deve mostrar orçamento/limite, não produção linear infinita.
```

---

## 15. Calendar Day Detail Screen

### 15.1 Objetivo

Mostrar tempo, estação, clima, eventos e condições conhecidas.

### 15.2 Calendar overview

```text
dia atual;
estação;
ano;
semana;
clima atual;
previsão conhecida;
evento lunar conhecido;
festivais;
orders/encomendas com prazo;
aniversários futuros se existirem;
eventos de quest conhecidos;
lojas fechadas/abertas por evento conhecido.
```

### 15.3 Day detail drawer

```text
data;
estação;
clima previsto;
evento lunar conhecido;
festival;
NPC/evento associado;
orders vencendo;
quest esperando condição temporal;
observações descobertas.
```

### 15.4 Spoiler control

```text
Não revelar evento secreto.
Não revelar Nyx oculto antes da descoberta.
Não revelar condição exata de Mana cedo.
Não revelar evento do nível 101 cedo.
```

---

## 16. Fonte Menu

### 16.1 Objetivo

Fonte Menu mostra funções desbloqueadas por fragmentos.

### 16.2 Estados

```text
Adormecida / Respawn
Fragmento da Água / Água Viva
Fragmento da Memória / Respec
Fragmento da Vida / Purificação e cura avançada
Fragmento da Esperança / decisão final
```

### 16.3 Layout

```text
estado visual da Fonte;
funções desbloqueadas;
funções bloqueadas ocultas ou mostradas como ??? apenas se design pedir;
recurso/custo;
texto curto sem spoiler;
confirmação para ações custosas;
```

### 16.4 Regras

```text
Não mostrar respec antes do Fragmento da Memória.
Não mostrar purificação antes do Fragmento da Vida.
Não mostrar decisão final antes do Fragmento da Esperança.
Não explicar Anya completamente cedo.
Não revelar nível 101 cedo.
```

---

## 17. Bestiary / Knowledge UI Future Hook

Este documento não define o bestiário, mas reserva integração.

Futuro bestiário deve mostrar apenas conhecimento descoberto:

```text
nome conhecido/desconhecido;
família;
habitat;
drops descobertos;
vulnerabilidades descobertas;
resistências descobertas;
comportamentos observados;
notas de NPC/lore;
```

Equipment UI e Weapon Detail só devem mostrar interações com inimigos quando o conhecimento estiver desbloqueado.

Bestiary/Knowledge Discovery futuro vence para regras de descoberta.

---

## 18. Error / Empty / Blocked States

Toda tela precisa tratar erro como estado previsto.

Estados comuns:

```text
NoItems
NoSellableItems
NoShopStock
NoRecipes
NoCraftableRecipes
NoSkillPoints
NoActiveSlots
MissingMaterials
MissingGold
MissingRequirement
ItemLocked
QuestItemProtected
InventoryFull
StorageFull
InvalidTarget
WrongContext
FeatureLockedByStory
FeatureFutureNotImplemented
```

Regra:

```text
Estado vazio nunca deve parecer bug.
Bloqueio precisa explicar o próximo passo quando possível.
Feature futura não deve aparecer em runtime final como promessa quebrada; só em debug/dev.
```

---

## 19. Confirmation Patterns

### 19.1 Confirmação leve

Usar para:

```text
comprar item comum em quantidade alta;
vender stack comum;
craft em massa;
dormir cedo;
```

### 19.2 Confirmação forte

Usar para:

```text
vender item raro;
descartar item;
gastar SkillPoint em capstone;
respec;
upgrade caro;
escolha de quest irreversível;
decisão final;
```

### 19.3 Texto da confirmação

Deve mostrar:

```text
ação;
alvo;
custo;
consequência;
se é reversível;
botões Confirm/Cancel.
```

---

## 20. Focus Order por tela

### 20.1 Inventory

```text
Grid -> Detail actions -> Category tabs -> Hotbar -> Close
```

### 20.2 Equipment

```text
Equipment slots -> Candidate list -> Comparison drawer actions -> Close
```

### 20.3 Skill Tree

```text
Tree tabs -> Node graph -> Node detail actions -> Active slots -> Respec -> Close
```

### 20.4 Shop

```text
Buy/Sell tabs -> Item list -> Quantity selector -> Confirm -> Detail drawer -> Close
```

### 20.5 Crafting

```text
Category tabs -> Recipe list -> Recipe detail -> Quantity selector -> Craft -> Close
```

### 20.6 Quest Log

```text
Category tabs -> Quest list -> Quest detail -> Track/untrack -> Close
```

### 20.7 Calendar

```text
Month/season grid -> Day detail -> Event list -> Close
```

### 20.8 Fonte

```text
Function list -> Function detail -> Confirm/Cancel -> Close
```

---

## 21. Runtime flags conceituais

Possíveis flags/states para specs futuras:

```text
CurrentUIScreen
CurrentUIFocus
PreviousUIFocus
IsModalOpen
IsGameplayInputBlocked
IsTimePausedByUI
SelectedInventorySlot
SelectedItemId
SelectedEquipmentSlot
SelectedSkillTreeId
SelectedSkillNodeId
SelectedQuestId
SelectedNpcId
SelectedCalendarDay
SelectedShopMode
SelectedShopItemId
SelectedCraftRecipeId
SelectedFonteFunction
PendingConfirmationAction
LastOpenedTabByScreen
```

Não são nomes obrigatórios de classes; são contratos conceituais.

---

## 22. Data contracts futuros

Possíveis assets/configs:

```text
UIScreenConfigSO
UIFocusProfileSO
UITabConfigSO
UIDrawerConfigSO
UITooltipProfileSO
UIConfirmationProfileSO
UIEmptyStateProfileSO
InventoryScreenConfigSO
EquipmentScreenConfigSO
SkillTreeScreenConfigSO
ShopScreenConfigSO
CraftingScreenConfigSO
QuestLogScreenConfigSO
CalendarScreenConfigSO
FonteScreenConfigSO
```

Possíveis dados por tela:

```text
ScreenId
DefaultFocusTarget
PausesTime
BlocksGameplayInput
AllowedActions
TabIds
DrawerType
TooltipProfile
EmptyStateIds
ConfirmationProfileIds
CloseBehavior
BackBehavior
```

---

## 23. Regras anti-regressão

```text
Todo menu modal bloqueia WASD e input de gameplay.
Diálogo não deixa o personagem andar.
Shop Sell sempre mostra inventário vendável do jogador ou empty state explícito.
Buy e Sell nunca usam a mesma lista de dados.
Skill node sempre tem detail drawer antes de gastar ponto.
Gastar SkillPoint exige confirmação visual clara.
Active skill comprada não equipa automaticamente em slot cheio.
Equipment compare nunca equipa por hover ou foco.
Repair e Upgrade são ações diferentes.
Quest/Key item não pode ser vendido/descartado por padrão.
Tooltip curto não substitui detail drawer para informação complexa.
Fonte Menu não mostra função ainda não desbloqueada por fragmento.
Calendar não revela segredo antes da descoberta.
Social future não transforma NPC em planilha.
HUD final não mostra debug.
```

---

## 24. Specs futuras recomendadas

```text
spec_ui_menu_focus_stack_runtime.md
spec_ui_inventory_screen_flow_runtime.md
spec_ui_storage_chest_screen_flow_runtime.md
spec_ui_equipment_screen_compare_runtime.md
spec_ui_weapon_armor_detail_drawer_runtime.md
spec_ui_repair_upgrade_screen_flow_runtime.md
spec_ui_skill_tree_node_detail_runtime.md
spec_ui_active_slot_assignment_runtime.md
spec_ui_spell_magic_detail_runtime.md
spec_ui_shop_buy_sell_screen_runtime.md
spec_ui_crafting_recipe_screen_runtime.md
spec_ui_quest_log_detail_runtime.md
spec_ui_calendar_day_detail_runtime.md
spec_ui_fonte_menu_flow_runtime.md
spec_ui_empty_error_confirmation_patterns_runtime.md
spec_ui_menu_gamepad_navigation_future.md
```

---

## 25. Decisões fechadas

```text
O UI/UX central estava correto, mas não detalhava todos os fluxos derivados.
Este documento passa a ser a fonte canônica de menus, submenus, drawers e screen flows.
Skill tree precisa de node detail drawer.
Equipamento precisa de comparison drawer e detail submenu.
Weapon/armor detail precisa separar overview, stats, effects, material, durability e known enemy interactions.
Shop Buy/Sell precisa separar dados de loja e dados de inventário do jogador.
Crafting precisa mostrar materiais faltantes e recipe states.
Quest Log precisa mostrar condição temporal/lunar apenas quando descoberta.
Calendar precisa ter day detail e spoiler control.
Fonte Menu precisa evoluir por fragmento e ocultar funções futuras.
Bestiary/Knowledge Discovery futuro deve controlar quais vulnerabilidades aparecem em tooltips/equipment UI.
```

---

## 26. Pendências abertas

```text
Definir mock visual de cada tela.
Definir resolução base e grid size final.
Definir iconografia.
Definir tipografia final.
Definir quantidade de linhas/colunas por inventory/storage.
Definir se o jogo terá busca/filtro textual ou apenas categorias.
Definir o nível de detalhe mostrado no modo avançado.
Definir regras finais de gamepad.
Definir se Calendar, Social e Quest Log ficam em um Journal unificado ou menus separados.
Definir integração final com Bestiary/Knowledge Discovery.
```
