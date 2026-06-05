# Cindar's Hope — UI/UX Full Gameplay Direction

> **Status:** fonte canônica central de UI/UX, HUD, menus, feedback, input routing e navegação  
> **Local:** `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> **Função:** centralizar padrões de UI/UX para todo o gameplay, evitando duplicação entre documentos de sistemas.  
> **Não é spec implementável.** Specs futuras devem quebrar este direction em specs menores de runtime/UI.

---

## 0. Regra anti-duplicação

Este documento é a fonte canônica para:

```text
HUD;
UI;
UX;
menus;
modais;
overlays;
input routing;
foco de UI;
bloqueio de movimento durante menu;
navegação por teclado/mouse;
navegação por gamepad futuro;
feedback visual;
feedback sonoro;
notificações;
tooltips;
layout de janelas;
prioridade visual;
legibilidade;
acessibilidade básica;
debug HUD vs HUD final.
```

Documentos de sistema continuam podendo declarar **o que precisa ser comunicado**.

Exemplo:

```text
COMBAT_CORE_DIRECTION.md pode dizer:
  Stamina insuficiente precisa ser comunicada.

UI_UX_FULL_GAMEPLAY_DIRECTION.md define:
  como, onde, com que prioridade e sem poluir a tela.
```

Quando houver conflito:

```text
Documento de sistema vence para significado mecânico.
UI_UX_FULL_GAMEPLAY_DIRECTION.md vence para apresentação, layout, foco, input, prioridade visual, navegação e feedback.
```

Seções antigas de HUD/UI em outros documentos passam a ser tratadas como resumo local e devem referenciar este arquivo em specs futuras.

---

## 1. Problemas reais que este documento resolve

Problemas já observados no jogo:

```text
Ao abrir HUD com I, dialogar ou navegar opções, WASD ainda move o personagem.
Isso dificulta usar menus e diálogos.
Algumas HUDs ocupam espaço demais.
Diálogos devem ocupar apenas fala + retrato + opções.
Vendedores não mostram itens para vender.
Ao selecionar vender, inventário do jogador não aparece corretamente.
```

Esses problemas viram regras canônicas:

```text
UI modal bloqueia input de gameplay.
UI precisa ter foco explícito.
UI precisa ter tamanho adequado ao conteúdo.
Buy/Sell precisa deixar claro inventário da loja e inventário do jogador.
Empty state precisa parecer estado válido, não bug.
```

---

## 2. Modelo mental de UI

A UI do jogo deve ser organizada em camadas.

```text
Gameplay HUD
  sempre visível ou contextual durante gameplay.

Contextual Prompt
  prompt curto para interação, loot, porta, NPC, objeto, Fonte, crop, animal, pet.

Light Overlay
  painel pequeno sem pausar completamente o jogo, quando aplicável.

Modal Gameplay Menu
  inventário, equipamentos, skill tree, crafting, shop, social log, quest log, calendar.

Dialogue Modal
  fala de NPC, escolhas, confirmação, entrega de item, quest dialogue.

System Menu
  pause, options, save/load, quit.

Debug HUD
  apenas dev/debug; nunca confundir com HUD final.
```

Regra:

```text
Quanto mais modal a UI, maior deve ser o bloqueio de input de gameplay.
```

---

## 3. Input routing e foco

### 3.1 Regra principal

Quando qualquer UI modal estiver aberta:

```text
WASD não move o personagem.
Dash/Dodge/Block não executam.
Ataque não executa.
Interact não interage com mundo atrás da UI.
Hotbar não troca item/ferramenta salvo se a própria UI permitir.
Input vai para navegação da UI.
```

UIs modais incluem:

```text
Inventory
Equipment
SkillTree
Crafting
Shop
Dialogue
QuestLog
SocialLog
Calendar
Map
Fonte menu
Pet menu
Companion menu
Pause/System menu
```

### 3.2 Estados de foco

Estados mínimos:

```text
GameplayFocus
DialogueFocus
MenuFocus
ShopFocus
InventoryFocus
CraftingFocus
SkillTreeFocus
QuestLogFocus
SocialLogFocus
SystemFocus
DebugFocus
```

Regras:

```text
Só um foco principal ativo por vez.
Submodal pode existir dentro do foco atual.
Input de gameplay só é aceito em GameplayFocus.
ESC/B/back fecha a camada atual ou volta uma camada.
Confirm/Interact confirma apenas elemento focado.
Cancel fecha ou volta; não executa ação de mundo.
```

### 3.3 Prioridade de input

Ordem:

```text
1. System modal
2. Confirmation modal
3. Dialogue choice
4. Shop/crafting/inventory action
5. Menu navigation
6. Gameplay action
```

Se uma UI modal está aberta, gameplay action não deve receber input.

---

## 4. HUD principal

HUD base consolidada:

```text
HP
MP quando relevante
Stamina
Fome
Cansaço
hotbar
4 active slots
arma/ferramenta ativa
status negativos
buffs
companion state quando relevante
pet state quando relevante
quest/context prompt quando relevante
```

Não mostrar na HUD:

```text
Breath
Fôlego
BR
números internos excessivos
cálculos intermediários
debug de runtime em build final
```

### 4.1 Densidade visual

HUD principal deve ser legível e compacta.

Regra:

```text
A HUD não deve competir com leitura da cena.
A HUD não deve cobrir área central de combate/fazenda/cidade.
HUDs temporárias devem expirar ou recolher quando não relevantes.
```

### 4.2 Estados permanentes vs contextuais

Permanentes:

```text
HP
Stamina
hotbar
active slots
arma/ferramenta ativa
```

Contextuais:

```text
MP se build/magia/equipamento tornar relevante
Fome/Cansaço compacto fora de perigo
companion/pet state se ativo/relevante
status/buffs quando ativos
quest/context prompt perto de interação
```

---

## 5. Hotbar e active slots

Regras:

```text
Hotbar = ferramentas, armas, consumíveis e itens rápidos.
Active slots = até 4 habilidades equipáveis.
Dash/Dodge/Block não ocupam active slots.
Active slots podem conter técnicas, magias, suporte e utilidade.
```

Feedback mínimo:

```text
slot vazio;
slot bloqueado;
slot em cooldown;
sem MP/Stamina;
item sem quantidade;
skill não utilizável no contexto;
alvo inválido;
```

---

## 6. Inventário

Inventário deve suportar:

```text
ItemStack;
ItemInstance;
quantidade;
qualidade;
raridade;
tag/categoria;
favorito/bloqueado contra venda futura;
comparação quando item equipável;
uso rápido quando aplicável;
drop/split/move/sell quando permitido;
```

Regras de UX:

```text
Itens vendáveis devem ser claramente distinguíveis de itens não vendáveis.
Itens quest/key devem ter proteção contra venda/descarte.
Stack split precisa ser explícito.
Comparação de equipamento deve ser acessível sem trocar item por acidente.
Ações destrutivas exigem confirmação se irreversíveis.
```

---

## 7. Equipment UI

Equipment UI deve mostrar:

```text
arma equipada;
ferramenta equipada quando relevante;
armadura;
escudo;
acessórios;
staff/focus/wand quando aplicável;
status derivados principais;
resistências relevantes;
comparação antes/depois;
durabilidade;
material/tier/quality/rarity;
```

Não deve mostrar fórmulas internas completas por padrão.

Pode ter modo avançado futuro:

```text
tooltip expandido;
comparação detalhada;
origem de bônus;
resistências completas;
```

---

## 8. Tooltip padrão

Tooltip de item deve ter camadas:

```text
Nome
Categoria
Quantidade / stack
Quality
Rarity
Valor base ou preço contextual quando em shop/sell
Uso principal
Tags relevantes
Efeitos principais
Requisitos quando aplicável
Avisos de quest/key item
```

Tooltip equipável deve adicionar:

```text
dano/armor/resistência principal;
material/tier;
durabilidade;
scaling;
comparação com item equipado;
interações de vulnerabilidade relevantes;
```

Tooltip de magia/skill deve adicionar:

```text
custo de MP/Stamina;
cooldown;
cast time;
shape/range;
efeito principal;
status aplicado;
condição de uso;
fonte de unlock quando relevante;
```

---

## 9. Diálogo e escolhas

### 9.1 Layout

Diálogo deve ocupar apenas o necessário:

```text
retrato ou sprite do NPC, se disponível;
nome do NPC;
texto de fala;
opções de resposta quando houver;
indicadores de quest/social quando relevantes;
```

Regra:

```text
Diálogo não deve ocupar a tela inteira sem necessidade.
Diálogo não deve deixar WASD mover o personagem.
Opções devem ser navegáveis por teclado/mouse e gamepad futuro.
```

### 9.2 Escolhas

Escolhas devem indicar quando causam ação especial:

```text
aceitar quest;
entregar item;
comprar/vender;
iniciar romance;
confirmar presente;
confirmar decisão final;
entrar em caverna;
ativar ritual/Fonte;
```

Escolhas irreversíveis ou grandes devem ter confirmação.

---

## 10. Shop UI

Shop UI deve separar claramente:

```text
Buy
Sell
Shop Inventory
Player Inventory
Gold do jogador
Preço unitário
Quantidade
Total
Estoque disponível
Restock/limited/unique quando relevante
```

Regras:

```text
Buy mostra estoque da loja.
Sell mostra inventário vendável do jogador.
Itens não vendáveis aparecem bloqueados ou ficam ocultos conforme spec final.
Shop vazio mostra empty state explícito.
Inventário do jogador vazio para venda mostra empty state explícito.
LimitedStock e UniqueStock devem ser visualmente claros.
```

Empty states aceitáveis:

```text
Esta loja não tem itens disponíveis hoje.
Você não possui itens vendáveis.
Este vendedor não compra este tipo de item.
Estoque esgotado até o próximo restock.
```

---

## 11. Crafting UI

Crafting UI deve mostrar:

```text
receitas conhecidas;
receitas bloqueadas quando apropriado;
inputs necessários;
quantidade disponível;
station necessária;
tempo de craft/processamento;
resultado;
qualidade prevista quando aplicável;
energia/custo quando aplicável;
```

Regras:

```text
Receita craftável deve ser distinguível de receita bloqueada.
Falta de material deve indicar qual material falta.
Craft em massa deve mostrar total de custo.
Processamento com timer deve mostrar estado e coleta futura.
```

---

## 12. Skill Tree UI

Skill Tree UI deve mostrar:

```text
5 árvores iniciais;
SkillPoints disponíveis;
nodes bloqueados/desbloqueados/adquiridos;
pré-requisitos;
ranks;
capstones;
active vs passive;
active slots disponíveis;
respec pela Fonte quando desbloqueado;
```

Regras:

```text
Não permitir gastar ponto sem confirmação visual clara.
Node bloqueado precisa explicar requisito.
Active skill precisa mostrar se está equipada ou não.
Capstone deve indicar exclusividade quando aplicável.
Respec deve indicar custo/condição/Fonte.
```

---

## 13. Quest Log

Quest log deve separar:

```text
Main Quest
Side Quest
Social Quest
Farm Order / Encomenda
Companion Quest
Pet/Farm Hint
Completed
Failed/Expired quando aplicável
```

Cada quest deve mostrar:

```text
título;
resumo curto;
objetivo atual;
local ou pista;
NPC relacionado;
itens necessários;
recompensa conhecida ou desconhecida;
estado;
marcador opcional;
```

Main quest deve suportar:

```text
Ato atual;
fragmento atual;
estado da Fonte;
objetivo de cidade/fazenda/caverna;
sem revelar spoilers futuros.
```

---

## 14. Social Log

Social log futuro deve mostrar:

```text
NPC conhecido;
estado social geral;
preferências descobertas;
presentes dados recentemente;
quest pessoal;
romance eligibility quando descoberto;
partner/polycule status quando futuro runtime existir;
visitas/eventos relevantes;
```

Regras:

```text
Não transformar NPC em planilha.
Feedback social pode usar ícones, texto curto e descobertas graduais.
Não mostrar ícone romântico em NPC bloqueado por idade/narrativa.
Feedback ofensivo/proibido precisa ser claro, não ambíguo.
```

---

## 15. Calendar / Seasons / Weather UI

Calendar deve suportar:

```text
dia;
estação;
clima atual/previsão quando desbloqueado;
festivais;
aniversários futuros se existirem;
restock relevante quando aplicável;
orders/encomendas;
eventos lunares;
eventos de quest quando conhecidos;
```

Não deve revelar eventos secretos antes da descoberta.

---

## 16. Fonte UI

Fonte UI deve evoluir por estágio.

Estados:

```text
Adormecida / Respawn
Fragmento da Água / Água Viva
Fragmento da Memória / Respec
Fragmento da Vida / Purificação e cura avançada
Fragmento da Esperança / decisão final
```

Fonte UI deve mostrar apenas funções desbloqueadas.

Não mostrar cedo:

```text
respec antes do Fragmento da Memória;
purificação antes do Fragmento da Vida;
decisão final antes do Fragmento da Esperança;
explicação completa de Anya;
spoiler de nível 101.
```

---

## 17. Cave HUD

Durante caverna, HUD deve priorizar:

```text
HP;
Stamina;
MP quando relevante;
active slots;
hotbar rápida;
status negativos;
buffs;
companion/pet state;
floor/depth quando apropriado;
checkpoint/boss gate feedback;
loot/context prompt;
```

Não poluir com:

```text
inventário completo sempre aberto;
logs longos;
quest text permanente ocupando combate;
números internos de spawn/budget;
```

---

## 18. Combat feedback

O jogador precisa ler:

```text
hit recebido;
hit causado;
block bem sucedido;
block quase quebrando;
Stamina insuficiente;
MP insuficiente;
MinorOpening;
CriticalWindow;
CoreExposed;
vulnerabilidade elemental/material;
status aplicado;
boss phase transition;
companion setup;
pet interrupt;
```

Regras:

```text
Feedback de combate deve ser claro por silhueta, animação, VFX, SFX e timing.
Não depender apenas de texto.
Boss precisa de telegraph mais claro que mob comum.
Pixel art deve priorizar leitura de windup, impacto e recovery.
```

---

## 19. Notifications

Tipos:

```text
loot recebido;
item insuficiente;
quest atualizada;
quest concluída;
recipe aprendida;
spell aprendida;
skill point recebido;
relationship change;
pet/companion state;
shop restock;
farm event;
Fonte reagiu;
fragmento recuperado;
```

Regras:

```text
Notificação não deve bloquear input, salvo confirmação importante.
Notificação não deve empilhar a ponto de cobrir gameplay.
Notificação crítica pode pausar em modal se for decisão irreversível.
```

---

## 20. Modal sizing e layout

Regra principal:

```text
Painel deve ocupar o espaço necessário para sua função, não a tela inteira por padrão.
```

Diretrizes:

```text
Dialogue: parte inferior ou lateral compacta, com opções claras.
Inventory: painel médio/grande, mas não confundir com pause total se não necessário.
Shop: painel dividido em Buy/Sell + lista + detalhe.
Skill tree: pode usar tela maior por complexidade.
Quest/Social/Calendar: tela média/grande, navegação por abas.
Confirmation: modal pequeno central.
```

---

## 21. Debug HUD vs HUD final

Debug HUD pode mostrar:

```text
IDs;
coords;
state machines;
quest flags;
enemy budget;
spawn anchors;
input focus;
shop stock state;
save/load state;
```

HUD final não deve mostrar isso.

Regra:

```text
Debug deve ser explicitamente marcado como debug.
Nunca depender de debug HUD para comunicar regra ao jogador.
```

---

## 22. Acessibilidade básica

Regras iniciais:

```text
texto legível em escala alvo;
contraste suficiente;
não depender apenas de cor para estados críticos;
feedback visual + sonoro para eventos críticos;
opção futura de reduzir flashes fortes;
opção futura de ajustar volume de UI/SFX;
confirmar ações destrutivas;
permitir navegação clara por teclado/mouse;
preparar gamepad futuro sem redesenhar toda UI.
```

---

## 23. Fluxos prioritários para specs futuras

Specs futuras recomendadas:

```text
spec_ui_input_focus_modal_routing.md
spec_ui_hud_main_gameplay_runtime.md
spec_ui_dialogue_choice_runtime.md
spec_ui_inventory_items_tooltips_runtime.md
spec_ui_equipment_compare_runtime.md
spec_ui_shop_buy_sell_runtime.md
spec_ui_crafting_recipe_runtime.md
spec_ui_skill_tree_active_slots_respec_runtime.md
spec_ui_quest_log_main_side_orders_runtime.md
spec_ui_social_log_future.md
spec_ui_calendar_weather_lunar_runtime.md
spec_ui_fonte_anya_progression_runtime.md
spec_ui_cave_combat_feedback_runtime.md
spec_ui_notifications_feedback_runtime.md
spec_ui_debug_hud_separation_runtime.md
```

---

## 24. Decisões fechadas

```text
UI/UX fica centralizado neste documento.
Documentos de sistema dizem o que comunicar; este documento diz como comunicar.
UI modal bloqueia WASD e input de gameplay.
Dialogue, shop, inventory, crafting, skill tree, quest log e social log exigem foco explícito.
Diálogos devem ser compactos e não ocupar tela inteira sem necessidade.
Shop Buy/Sell deve separar loja e inventário do jogador.
Sell view deve mostrar itens vendáveis do jogador ou empty state explícito.
HUD principal não mostra Breath/Fôlego/BR.
HUD final não deve depender de debug.
Fonte UI só mostra funções desbloqueadas por fragmento.
Quest/social/romance/poliamor são feedbacks futuros quando runtime existir.
```

---

## 25. Pendências

```text
Definir layout visual final por resolução.
Definir escala base de pixel art UI.
Definir fonte/tipografia final.
Definir controles finais de teclado/mouse.
Definir convenção de ícones.
Definir navegação gamepad futura.
Definir estilos de painel por domínio: fazenda, cidade, caverna, Fonte, sistema.
Definir quando UI pausa o jogo e quando apenas bloqueia input.
Definir lista final de notifications e prioridades.
Definir regras de localização/tradução futura.
```
