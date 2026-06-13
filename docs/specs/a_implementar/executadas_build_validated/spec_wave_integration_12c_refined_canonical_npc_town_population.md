# /speckit.specify
# /speckit.plan
# /speckit.tasks
Ordem de execucao: WAVE_INTEGRATION_12C
Depende de: WAVE_INTEGRATION_12
Bloqueia: WAVE13 until human Play Mode validation
required_adrs: []
required_game_rules: []
# WAVE_INTEGRATION_12C — Refined Canonical NPC Town Population

> **Projeto:** Cindar's Hope
> **Tipo:** Spec implementável / Data + Scene Wiring / NPC roster canônico refinado
> **Aplica sobre:** WAVE_INTEGRATION_12
> **Objetivo:** inserir todos os 23 NPCs refinados/canônicos da cidade na TownScene, com objetivo funcional, localização, movimento, diálogo mínimo e serviço/shop quando aplicável.
> **Regra:** não é “descobrir quem são os NPCs”; os NPCs estão listados explicitamente nesta spec.

## 0. Problema que esta spec corrige

A WAVE12 atual colocou apenas um subconjunto de NPCs na TownScene. Esta spec promove **todos os 23 NPCs canônicos refinados** para presença visível na cidade, ainda que alguns serviços avançados fiquem com debt.

A execução deve criar/povoar a cidade com NPCs reais do design direction, não placeholders genéricos.

## 1. Fontes obrigatórias

Ler e usar:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md
docs/project/CURRENT_STATE.md
```

Se houver conflito entre esta spec e `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`, parar com `BLOCKED_BY_DESIGN_DIRECTION_CONFLICT`.

## 2. Regras de implementação

```text
NÃO criar uma classe por NPC.
NÃO criar sistema social/romance final.
NÃO criar quest final.
NÃO duplicar DialogueService, ShopService, InventoryManager ou EconomyManager.
NÃO deixar placeholder genérico se o NPC canônico está listado aqui.
NÃO declarar sucesso se algum dos 23 NPCs não estiver no roster/scene/wiring ou debt explícito.
NÃO alterar Packages/ProjectSettings.
```

Modelo obrigatório:

```text
NpcDefinition data-driven
NpcScenePlacementMarker
NpcSceneInteractable
NpcDialogueBridge
NpcShopBridge, se aplicável
NpcMovementController genérico
DialogueTreeSO/DialogueSet data-driven
ShopDataSO/ShopDefinition data-driven
```

## 3. Critério real de sucesso

Ao abrir a TownScene no Unity, o jogador deve conseguir:

```text
1. Ver todos os 23 NPCs canônicos refinados.
2. Chegar até cada NPC.
3. Interagir com cada NPC.
4. Abrir diálogo com cada NPC.
5. Ver o nome correto do NPC.
6. Ter pelo menos 10 entries por NPC no DialogueSet.
7. Abrir shop/service para NPCs comerciais quando o serviço já existir.
8. Ver movimento para NPCs Patrol/Wander/NightRoute ou debt explícito.
9. Fechar diálogo/shop sem travar input/modal.
```

## 4. Roster canônico refinado — implementar todos

| NpcId | Nome | Role | Objetivo no gameplay | Zona da cidade | MovementProfile | Serviço/Shop | Implementar agora |
|---|---|---|---|---|---|---|---:|
| `npc_corvus` | Padre Corvus | Curandeiro/Guardião | Templo de Kanthor | Temple | Stationary/TemplePatrol | healing_blessing_temple | YES |
| `npc_mara` | Mara Vellum | Escriba/Comerciante | Cartório e licenças | Registry | Stationary/RegistryDesk | licenses_contracts | YES |
| `npc_sylveth` | Sylveth | Plantador/Curandeiro | Sementes, crops e ritos de Thandra | SeedShop/Garden | ShopKeeperFixed/FarmVisit | seeds_crops_herbs | YES |
| `npc_brumdar` | Brumdar Ferro-Quieto | Artesão/Combatente | Forja, ferramentas, armas e minério | Forge | ShopKeeperFixed | blacksmith_repair_upgrade | YES |
| `npc_nimble` | Nimble Galhobaixo | Construtor/Artesão | Construções, layout e mover estruturas | Carpenter | Patrol/WorkshopDesk | construction_buildings_move | YES |
| `npc_gurd` | Gurd Carvalho-Torto | Construtor/Combatente | Limpeza pesada, expansão e força bruta | ConstructionYard | Patrol/HeavyWorkZone | heavy_clearance | YES |
| `npc_hund` | Hund Carvalho-Torto | Guardião/Construtor | Patrulha, defesa e transporte seguro | GuardRoute | Patrol/TownRoad | guard_transport | YES |
| `npc_ozzra` | Ozzra Fumaçazul | Alquimista/Artesão | Poções, fertilizantes e reagentes | AlchemyLab | WanderWithinZone/Lab | alchemy_potions_fertilizer | YES |
| `npc_gruta` | Gruta Panela-Funda | Comerciante/Músico | Taverna, comida, rumores e buffs sociais | Tavern | ShopKeeperFixed/TavernStage | tavern_food_rumors | YES |
| `npc_zrix` | Zrix das Estradas | Explorador/Comerciante | Mapas, caverna, rotas e contratos | Guild/RoadGate | Patrol/CaveRoad | maps_cave_contracts | YES |
| `npc_yael` | Yael Noite-Mansa | Comerciante/Explorador | Loja noturna, itens raros e segredos | NightMarket | NightOnly/WanderHidden | night_shop_rare_items | YES |
| `npc_thalindra` | Thalindra Véu-de-Lua | Pesquisador/Alquimista | Arquivo, lore, Cindar, Anya, Elyndor/Bromécia | Archive | Stationary/ArchiveDesk | archive_lore_quests_blueprints | YES |
| `npc_dagna` | Dagna Rocha-Morna | Minerador/Combatente | Mineração, pedreira, cave ore | Quarry/MineOffice | Patrol/QuarryRoad | ore_mining_cave | YES |
| `npc_pip` | Pip Semente-Solta | Comerciante/Explorador | Tutorial, recados, entrega e humor | TownEntrance/Market | WanderWithinZone | tutorial_delivery | YES |
| `npc_alaric` | Ser Alaric Veyr | Guardião/Combatente | Guarda, patrulha, combate e segurança | GuardPost | Patrol/TownGate | guard_combat_training | YES |
| `npc_mirela` | Mirela dos Laços | Artesão/Comerciante | Bolsas, roupas, acessórios | Tailor | ShopKeeperFixed | tailor_bags_clothing | YES |
| `npc_renko` | Renko Três-Sorrisos | Comerciante/Artesão | Loja geral, barganha e itens comuns | GeneralStore | ShopKeeperFixed | general_store_bargain | YES |
| `npc_eiran` | Eiran Valeclaro | Tratador/Plantador | Animais, pets e ração | AnimalYard | WanderWithinZone/AnimalArea | animals_pets_feed | YES |
| `npc_liora` | Liora Canta-Rio | Músico/Pesquisador | Música, sonhos, Alihana e pistas de Anya | Tavern/StatueGarden | WanderWithinZone/EveningStage | music_dream_lore | YES |
| `npc_orlan` | Orlan Pouso-Curto | Comerciante/Escriba | Hospedagem, viajantes e notícias | Inn | ShopKeeperFixed | inn_lodging_news | YES |
| `npc_savra` | Savra Escama-Verde | Curandeiro/Explorador | Ervas, antídotos, venenos e floresta | Herbalist/ForestGate | Patrol/HerbRoute | herbs_antidote_forest | YES |
| `npc_tovin` | Tovin Mãos-de-Selo | Escriba/Artesão | Registros, impostos, altares e permissões | Registry | Stationary/PermitDesk | permits_altars_registry | YES |
| `npc_maelor` | Maelor Cinza | Explorador/Pesquisador | Nyx, memória, ruínas e segredos tardios | StatueGarden/NightRoute | NightOnly/WanderHidden | late_lore_memory | YES |

## 5. Zonas de posicionamento recomendadas

| ZoneKey | Uso |
|---|---|
| Temple | templo de Kanthor / lado norte da praça |
| Registry | cartório / prefeitura local / mesa de registros |
| SeedShop/Garden | loja de sementes e jardim rural |
| Forge | forja / oficina metalúrgica |
| Carpenter | carpintaria / pátio de obras |
| ConstructionYard | pátio de construção pesada |
| GuardRoute | rota entre portão, poço e estrada baixa |
| AlchemyLab | laboratório de alquimia |
| Tavern | taverna |
| Guild/RoadGate | guilda/estrada para caverna |
| NightMarket | loja noturna / beco de Nyx |
| Archive | arquivo/biblioteca |
| Quarry/MineOffice | pedreira / escritório de mineração |
| TownEntrance/Market | entrada da cidade / mercado |
| GuardPost | posto da guarda |
| Tailor | alfaiataria / loja de bolsas |
| GeneralStore | loja geral |
| AnimalYard | área de animais |
| Tavern/StatueGarden | taverna ou jardim das estátuas |
| Inn | pousada |
| Herbalist/ForestGate | ervanário / portão da floresta |
| StatueGarden/NightRoute | jardim das estátuas / rota noturna |

## 6. DialogueSets obrigatórios

Cada NPC abaixo deve ter pelo menos 10 entries. As frases são seed text implementável. Elas podem entrar em `DialogueTreeSO`, `DialogueSet`, JSON/config ou data asset equivalente. Se o runtime atual não suporta 10 escolhas simultâneas, usar árvore/continue/branch, mas manter as 10 entries no data.

### 6.1 `npc_corvus` — Padre Corvus

**Objetivo:** Templo de Kanthor.
**Role:** Curandeiro/Guardião.
**Zona:** Temple.
**Movimento:** Stationary/TemplePatrol.
**Serviço/Shop:** `healing_blessing_temple`.
**Quest seeds:** O Sino que Não Toca, Juramento Partido, O Nome Sob a Pedra.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_corvus_dialogue_01` | Greeting | Que Kanthor pese seus passos com justiça. | YES |
| `npc_corvus_dialogue_02` | Role | Sou Corvus, servo do templo e guardião dos juramentos da cidade. | YES |
| `npc_corvus_dialogue_03` | Location | Cindar's Hope parece simples, mas toda cidade guarda pedra sob a terra. | YES |
| `npc_corvus_dialogue_04` | GameplayTip | Procure ajuda antes de descer à caverna; coragem sem preparo vira luto. | YES |
| `npc_corvus_dialogue_05` | RumorNonSpoiler | Há símbolos antigos sob o templo que poucos gostam de mencionar. | NO |
| `npc_corvus_dialogue_06` | Contextual | Hoje o templo está quieto demais; isso raramente é bom. | NO |
| `npc_corvus_dialogue_07` | ShopOrService | Posso oferecer cura, bênção menor e orientação quando o sistema estiver pronto. | YES |
| `npc_corvus_dialogue_08` | QuestFutureHook | Um sino rachado espera mãos capazes e metal honesto. | NO |
| `npc_corvus_dialogue_09` | RepeatFallback | Repito: ordem não é medo; ordem é o que protege o fraco. | YES |
| `npc_corvus_dialogue_10` | Goodbye | Volte em segurança. A balança pesa também quem sobrevive. | YES |

### 6.2 `npc_mara` — Mara Vellum

**Objetivo:** Cartório e licenças.
**Role:** Escriba/Comerciante.
**Zona:** Registry.
**Movimento:** Stationary/RegistryDesk.
**Serviço/Shop:** `licenses_contracts`.
**Quest seeds:** Licença de Primeira Obra, As Páginas Arrancadas, Lei ou Compaixão.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_mara_dialogue_01` | Greeting | Nome, propriedade e intenção. Nessa ordem. | YES |
| `npc_mara_dialogue_02` | Role | Sou Mara Vellum; registro o que a cidade prefere esquecer informalmente. | YES |
| `npc_mara_dialogue_03` | Location | Sem licença, até uma boa ideia vira problema público. | YES |
| `npc_mara_dialogue_04` | GameplayTip | Antes de construir, confira materiais, taxas e autorização. | YES |
| `npc_mara_dialogue_05` | RumorNonSpoiler | Há páginas arrancadas nos arquivos. Ninguém arranca papel sem motivo. | NO |
| `npc_mara_dialogue_06` | Contextual | Hoje revisei três contratos e uma mentira muito mal escrita. | NO |
| `npc_mara_dialogue_07` | ShopOrService | Posso cuidar de licenças, contratos e reputação formal quando o sistema existir. | YES |
| `npc_mara_dialogue_08` | QuestFutureHook | Sua primeira obra precisa de registro, não só madeira. | NO |
| `npc_mara_dialogue_09` | RepeatFallback | Se já expliquei, está registrado. Se não está registrado, não expliquei. | YES |
| `npc_mara_dialogue_10` | Goodbye | Leve isto a sério. A cidade sobrevive por acordos. | YES |

### 6.3 `npc_sylveth` — Sylveth

**Objetivo:** Sementes, crops e ritos de Thandra.
**Role:** Plantador/Curandeiro.
**Zona:** SeedShop/Garden.
**Movimento:** ShopKeeperFixed/FarmVisit.
**Serviço/Shop:** `seeds_crops_herbs`.
**Quest seeds:** Sementes de Retorno, Festival de Thandra, Raízes que Ouvem.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_sylveth_dialogue_01` | Greeting | A terra responde melhor quando você para de apressá-la. | YES |
| `npc_sylveth_dialogue_02` | Role | Sou Sylveth; cuido das sementes e dos pequenos ritos de Thandra. | YES |
| `npc_sylveth_dialogue_03` | Location | A cidade vive do que cresce ao redor dela. | YES |
| `npc_sylveth_dialogue_04` | GameplayTip | Regue cedo, observe a estação e não trate solo como baú. | YES |
| `npc_sylveth_dialogue_05` | RumorNonSpoiler | Algumas raízes parecem ouvir a Fonte. Isso me preocupa. | NO |
| `npc_sylveth_dialogue_06` | Contextual | Hoje as folhas estão viradas para o vento errado. | NO |
| `npc_sylveth_dialogue_07` | ShopOrService | Posso vender sementes, ervas e orientar crops quando o estoque estiver ligado. | YES |
| `npc_sylveth_dialogue_08` | QuestFutureHook | Sementes antigas podem retornar se você recuperar o caminho delas. | NO |
| `npc_sylveth_dialogue_09` | RepeatFallback | Eu já disse: planta também tem humor. | YES |
| `npc_sylveth_dialogue_10` | Goodbye | Que Thandra deixe algo vivo no seu caminho. | YES |

### 6.4 `npc_brumdar` — Brumdar Ferro-Quieto

**Objetivo:** Forja, ferramentas, armas e minério.
**Role:** Artesão/Combatente.
**Zona:** Forge.
**Movimento:** ShopKeeperFixed.
**Serviço/Shop:** `blacksmith_repair_upgrade`.
**Quest seeds:** A Primeira Ferramenta Séria, Metal que Sussurra, O Martelo do Aprendiz.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_brumdar_dialogue_01` | Greeting | Se a ferramenta falhou, ou o metal mentiu ou a mão apressou. | YES |
| `npc_brumdar_dialogue_02` | Role | Brumdar Ferro-Quieto. Forjo, conserto e desconfio. | YES |
| `npc_brumdar_dialogue_03` | Location | Minério desta região tem memória estranha. | YES |
| `npc_brumdar_dialogue_04` | GameplayTip | Não leve ferramenta fraca para trabalho pesado. | YES |
| `npc_brumdar_dialogue_05` | RumorNonSpoiler | Metal que sussurra perto de pedra escura não é superstição. | NO |
| `npc_brumdar_dialogue_06` | Contextual | Hoje a forja cantou baixo. Não gosto quando ela canta baixo. | NO |
| `npc_brumdar_dialogue_07` | ShopOrService | Posso melhorar ferramentas, armas e reparos quando a bancada estiver ligada. | YES |
| `npc_brumdar_dialogue_08` | QuestFutureHook | Sua primeira ferramenta séria precisa de material sério. | NO |
| `npc_brumdar_dialogue_09` | RepeatFallback | Se quer elogio, traga liga limpa. | YES |
| `npc_brumdar_dialogue_10` | Goodbye | Volte com minério, não com desculpa. | YES |

### 6.5 `npc_nimble` — Nimble Galhobaixo

**Objetivo:** Construções, layout e mover estruturas.
**Role:** Construtor/Artesão.
**Zona:** Carpenter.
**Movimento:** Patrol/WorkshopDesk.
**Serviço/Shop:** `construction_buildings_move`.
**Quest seeds:** Madeira, Pedra e Assinatura, Mover é Mais Difícil que Erguer, A Tábua que Cura.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_nimble_dialogue_01` | Greeting | Essa cerca está dois passos fora do bom senso. | YES |
| `npc_nimble_dialogue_02` | Role | Nimble Galhobaixo, construtor, medidor e crítico de layouts horríveis. | YES |
| `npc_nimble_dialogue_03` | Location | Uma cidade viva precisa de casa, oficina e espaço para erro. | YES |
| `npc_nimble_dialogue_04` | GameplayTip | Planeje antes de gastar madeira; mover depois custa paciência. | YES |
| `npc_nimble_dialogue_05` | RumorNonSpoiler | Vi uma tábua bromeciana se reparar sozinha. Péssimo para orçamento. | NO |
| `npc_nimble_dialogue_06` | Contextual | Hoje medi a praça e ela continua torta. | NO |
| `npc_nimble_dialogue_07` | ShopOrService | Posso construir, mover estruturas e planejar expansão quando o sistema estiver pronto. | YES |
| `npc_nimble_dialogue_08` | QuestFutureHook | Sua fazenda precisa de madeira, pedra e assinatura. | NO |
| `npc_nimble_dialogue_09` | RepeatFallback | Não, diagonal não é sempre economia de espaço. | YES |
| `npc_nimble_dialogue_10` | Goodbye | Volte quando tiver material ou arrependimento. | YES |

### 6.6 `npc_gurd` — Gurd Carvalho-Torto

**Objetivo:** Limpeza pesada, expansão e força bruta.
**Role:** Construtor/Combatente.
**Zona:** ConstructionYard.
**Movimento:** Patrol/HeavyWorkZone.
**Serviço/Shop:** `heavy_clearance`.
**Quest seeds:** Pedra Grande, Martelo Maior, A Parede que Não Quebrou, Força sem Fúria.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_gurd_dialogue_01` | Greeting | Problema grande? Martelo maior. | YES |
| `npc_gurd_dialogue_02` | Role | Gurd. Eu carrego, quebro e pergunto depois se precisava. | YES |
| `npc_gurd_dialogue_03` | Location | Cidade cresce quando alguém tira pedra do caminho. | YES |
| `npc_gurd_dialogue_04` | GameplayTip | Se o obstáculo é pesado, chame quem aguenta. | YES |
| `npc_gurd_dialogue_05` | RumorNonSpoiler | A parede antiga que não quebrou ainda me irrita. | NO |
| `npc_gurd_dialogue_06` | Contextual | Hoje ninguém brigou comigo. Dia suspeito. | NO |
| `npc_gurd_dialogue_07` | ShopOrService | Posso ajudar em limpeza pesada e expansão quando o sistema existir. | YES |
| `npc_gurd_dialogue_08` | QuestFutureHook | Pedra grande pede martelo maior, não discurso. | NO |
| `npc_gurd_dialogue_09` | RepeatFallback | Já falei que não é raiva se é construção? | YES |
| `npc_gurd_dialogue_10` | Goodbye | Volte quando tiver algo que precise sair do lugar. | YES |

### 6.7 `npc_hund` — Hund Carvalho-Torto

**Objetivo:** Patrulha, defesa e transporte seguro.
**Role:** Guardião/Construtor.
**Zona:** GuardRoute.
**Movimento:** Patrol/TownRoad.
**Serviço/Shop:** `guard_transport`.
**Quest seeds:** A Entrega que Não Chegou, Barulho no Poço, Guardar sem Mandar.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_hund_dialogue_01` | Greeting | Eu ouvi você chegando antes de ver. | YES |
| `npc_hund_dialogue_02` | Role | Hund Carvalho-Torto. Vigio rotas, cargas e gente distraída. | YES |
| `npc_hund_dialogue_03` | Location | Nem todo perigo ruge; alguns rangem no poço. | YES |
| `npc_hund_dialogue_04` | GameplayTip | Não atravesse estrada baixa sem olhar para trás. | YES |
| `npc_hund_dialogue_05` | RumorNonSpoiler | Uma carga desapareceu sem marca de roda. Isso não acontece sozinho. | NO |
| `npc_hund_dialogue_06` | Contextual | Hoje o vento trouxe barulho de pedra oca. | NO |
| `npc_hund_dialogue_07` | ShopOrService | Posso escoltar, proteger entregas e reforçar rotas quando o sistema estiver pronto. | YES |
| `npc_hund_dialogue_08` | QuestFutureHook | A entrega que não chegou precisa de olhos e passos firmes. | NO |
| `npc_hund_dialogue_09` | RepeatFallback | Meu irmão quebra. Eu observo onde quebrar. | YES |
| `npc_hund_dialogue_10` | Goodbye | Ande com atenção. | YES |

### 6.8 `npc_ozzra` — Ozzra Fumaçazul

**Objetivo:** Poções, fertilizantes e reagentes.
**Role:** Alquimista/Artesão.
**Zona:** AlchemyLab.
**Movimento:** WanderWithinZone/Lab.
**Serviço/Shop:** `alchemy_potions_fertilizer`.
**Quest seeds:** Explode Só um Pouco, Fertilizante Fumaçazul, Reagente que Bebe Luz.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_ozzra_dialogue_01` | Greeting | Não explodiu. Isso é progresso. | YES |
| `npc_ozzra_dialogue_02` | Role | Ozzra Fumaçazul, alquimista oficial de quase nenhuma licença. | YES |
| `npc_ozzra_dialogue_03` | Location | Tudo na cidade pode virar mistura. Algumas coisas só uma vez. | YES |
| `npc_ozzra_dialogue_04` | GameplayTip | Poção boa precisa de reagente certo e distância prudente. | YES |
| `npc_ozzra_dialogue_05` | RumorNonSpoiler | Há cristais que bebem luz. Quero um. Com luvas. | NO |
| `npc_ozzra_dialogue_06` | Contextual | Hoje só três frascos gritaram. | NO |
| `npc_ozzra_dialogue_07` | ShopOrService | Posso vender poções, fertilizantes e reagentes quando o estoque estiver ligado. | YES |
| `npc_ozzra_dialogue_08` | QuestFutureHook | Fertilizante Fumaçazul melhora colheita ou ensina humildade. | NO |
| `npc_ozzra_dialogue_09` | RepeatFallback | Se sair fumaça azul, ainda está sob controle. Talvez. | YES |
| `npc_ozzra_dialogue_10` | Goodbye | Volta com ervas. E sem Corvus. | YES |

### 6.9 `npc_gruta` — Gruta Panela-Funda

**Objetivo:** Taverna, comida, rumores e buffs sociais.
**Role:** Comerciante/Músico.
**Zona:** Tavern.
**Movimento:** ShopKeeperFixed/TavernStage.
**Serviço/Shop:** `tavern_food_rumors`.
**Quest seeds:** Sopa para um Dia Ruim, Rumor Queimado, Banquete de Festival.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_gruta_dialogue_01` | Greeting | Senta, come e fala antes que eu adivinhe. | YES |
| `npc_gruta_dialogue_02` | Role | Gruta Panela-Funda. Minha taverna alimenta e escuta. | YES |
| `npc_gruta_dialogue_03` | Location | Uma cidade faminta acredita em qualquer mentira. | YES |
| `npc_gruta_dialogue_04` | GameplayTip | Comida reduz cansaço melhor que orgulho vazio. | YES |
| `npc_gruta_dialogue_05` | RumorNonSpoiler | Símbolos sob mesas são ruim para apetite e bom para rumor. | NO |
| `npc_gruta_dialogue_06` | Contextual | Hoje a sopa está forte e os boatos também. | NO |
| `npc_gruta_dialogue_07` | ShopOrService | Posso vender comida, ouvir rumores e preparar buffs sociais quando o sistema existir. | YES |
| `npc_gruta_dialogue_08` | QuestFutureHook | Sopa para um dia ruim começa com ingrediente honesto. | NO |
| `npc_gruta_dialogue_09` | RepeatFallback | Se quebrar cadeira, compra duas. | YES |
| `npc_gruta_dialogue_10` | Goodbye | Vai. Mas volta antes de virar história. | YES |

### 6.10 `npc_zrix` — Zrix das Estradas

**Objetivo:** Mapas, caverna, rotas e contratos.
**Role:** Explorador/Comerciante.
**Zona:** Guild/RoadGate.
**Movimento:** Patrol/CaveRoad.
**Serviço/Shop:** `maps_cave_contracts`.
**Quest seeds:** Mapa de Entrada, Sinal de Elyndor, A Estrada que Desce.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_zrix_dialogue_01` | Greeting | Todo caminho cobra algo. O truque é saber antes. | YES |
| `npc_zrix_dialogue_02` | Role | Zrix das Estradas. Mapeio rotas que fingem não existir. | YES |
| `npc_zrix_dialogue_03` | Location | A caverna é estrada ruim, não destino glorioso. | YES |
| `npc_zrix_dialogue_04` | GameplayTip | Leve suprimento, marque retorno e não confie em eco. | YES |
| `npc_zrix_dialogue_05` | RumorNonSpoiler | Vi sinal de Elyndor onde não devia haver sinal nenhum. | NO |
| `npc_zrix_dialogue_06` | Contextual | Hoje a estrada baixa está com cheiro de chuva antiga. | NO |
| `npc_zrix_dialogue_07` | ShopOrService | Posso vender mapas, contratos e suprimentos de rota quando o sistema estiver pronto. | YES |
| `npc_zrix_dialogue_08` | QuestFutureHook | Mapa de entrada salva mais vidas que espada bonita. | NO |
| `npc_zrix_dialogue_09` | RepeatFallback | Atalho só é atalho depois que você volta. | YES |
| `npc_zrix_dialogue_10` | Goodbye | Se descer, volte contando. | YES |

### 6.11 `npc_yael` — Yael Noite-Mansa

**Objetivo:** Loja noturna, itens raros e segredos.
**Role:** Comerciante/Explorador.
**Zona:** NightMarket.
**Movimento:** NightOnly/WanderHidden.
**Serviço/Shop:** `night_shop_rare_items`.
**Quest seeds:** Aberto Depois da Meia-Noite, Comprador de Fragmentos, Preço do Silêncio.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_yael_dialogue_01` | Greeting | Você não me viu. Ainda. | YES |
| `npc_yael_dialogue_02` | Role | Yael Noite-Mansa. Vendo o que não convém nomear alto. | YES |
| `npc_yael_dialogue_03` | Location | A cidade tem lojas de dia e verdades à noite. | YES |
| `npc_yael_dialogue_04` | GameplayTip | Nem todo item raro ajuda; alguns apenas cobram depois. | YES |
| `npc_yael_dialogue_05` | RumorNonSpoiler | Fragmentos escuros atraem compradores piores que ladrões. | NO |
| `npc_yael_dialogue_06` | Contextual | Hoje a sombra da praça chegou antes do sol ir embora. | NO |
| `npc_yael_dialogue_07` | ShopOrService | Posso abrir loja noturna e vender raridades quando o horário estiver ligado. | YES |
| `npc_yael_dialogue_08` | QuestFutureHook | Depois da meia-noite, algumas portas lembram que existem. | NO |
| `npc_yael_dialogue_09` | RepeatFallback | Preço baixo demais é convite, não oferta. | YES |
| `npc_yael_dialogue_10` | Goodbye | Esqueça meu rosto até precisar dele. | YES |

### 6.12 `npc_thalindra` — Thalindra Véu-de-Lua

**Objetivo:** Arquivo, lore, Cindar, Anya, Elyndor/Bromécia.
**Role:** Pesquisador/Alquimista.
**Zona:** Archive.
**Movimento:** Stationary/ArchiveDesk.
**Serviço/Shop:** `archive_lore_quests_blueprints`.
**Quest seeds:** Poeira no Arquivo, A Palavra Nymiriana, O Mapa Que Não Deveria Existir.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_thalindra_dialogue_01` | Greeting | Todo registro perdido deixa uma cicatriz. | YES |
| `npc_thalindra_dialogue_02` | Role | Thalindra Véu-de-Lua. Pesquiso o que a cidade editou. | YES |
| `npc_thalindra_dialogue_03` | Location | Cindar's Hope foi construída sobre versões incompletas. | YES |
| `npc_thalindra_dialogue_04` | GameplayTip | Leia inscrições antes de vender pedras estranhas. | YES |
| `npc_thalindra_dialogue_05` | RumorNonSpoiler | A palavra nymiriana perto da Fonte não deveria estar apagada. | NO |
| `npc_thalindra_dialogue_06` | Contextual | Hoje encontrei um mapa que nega a própria margem. | NO |
| `npc_thalindra_dialogue_07` | ShopOrService | Posso traduzir registros, entregar pistas e blueprints raros quando o sistema existir. | YES |
| `npc_thalindra_dialogue_08` | QuestFutureHook | Poeira no arquivo esconde mais que abandono. | NO |
| `npc_thalindra_dialogue_09` | RepeatFallback | Não confunda silêncio com ausência de prova. | YES |
| `npc_thalindra_dialogue_10` | Goodbye | Volte se encontrar símbolo de água, cinza ou portal. | YES |

### 6.13 `npc_dagna` — Dagna Rocha-Morna

**Objetivo:** Mineração, pedreira, cave ore.
**Role:** Minerador/Combatente.
**Zona:** Quarry/MineOffice.
**Movimento:** Patrol/QuarryRoad.
**Serviço/Shop:** `ore_mining_cave`.
**Quest seeds:** Veio de Cobre, Galeria Sem Nome, A Rocha Quente Demais.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_dagna_dialogue_01` | Greeting | Pedra viva soa diferente. | YES |
| `npc_dagna_dialogue_02` | Role | Dagna Rocha-Morna. Eu escuto minério e desconfio de pedra quente. | YES |
| `npc_dagna_dialogue_03` | Location | A cidade pisa em camadas que não entende. | YES |
| `npc_dagna_dialogue_04` | GameplayTip | Não venda minério sem saber de onde ele veio. | YES |
| `npc_dagna_dialogue_05` | RumorNonSpoiler | Uma galeria sem nome ainda leva o nome de alguém. | NO |
| `npc_dagna_dialogue_06` | Contextual | Hoje as rochas perto da estrada estão quentes demais. | NO |
| `npc_dagna_dialogue_07` | ShopOrService | Posso avaliar minério, ensinar mineração e abrir pedreira quando o sistema existir. | YES |
| `npc_dagna_dialogue_08` | QuestFutureHook | Veio de cobre é lição, não fortuna. | NO |
| `npc_dagna_dialogue_09` | RepeatFallback | Se a pedra mente, eu bato até ela admitir. | YES |
| `npc_dagna_dialogue_10` | Goodbye | Traga amostra, não história bonita. | YES |

### 6.14 `npc_pip` — Pip Semente-Solta

**Objetivo:** Tutorial, recados, entrega e humor.
**Role:** Comerciante/Explorador.
**Zona:** TownEntrance/Market.
**Movimento:** WanderWithinZone.
**Serviço/Shop:** `tutorial_delivery`.
**Quest seeds:** Primeira Entrega, Vi o Fantasma, A Carta Errada.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_pip_dialogue_01` | Greeting | Ei! Você é mais alto de perto. | YES |
| `npc_pip_dialogue_02` | Role | Sou Pip Semente-Solta. Entrego coisas, recados e às vezes problemas. | YES |
| `npc_pip_dialogue_03` | Location | A cidade parece grande até você correr por todos os becos. | YES |
| `npc_pip_dialogue_04` | GameplayTip | Primeiro aprenda onde comprar sementes; depois finja que sempre soube. | YES |
| `npc_pip_dialogue_05` | RumorNonSpoiler | Eu vi algo no Jardim das Estátuas. Não ri. | NO |
| `npc_pip_dialogue_06` | Contextual | Hoje entreguei uma carta certa para a pessoa quase certa. | NO |
| `npc_pip_dialogue_07` | ShopOrService | Posso guiar tutoriais, entregas e rotas de loja quando o sistema estiver pronto. | YES |
| `npc_pip_dialogue_08` | QuestFutureHook | Primeira Entrega: venha, a loja de sementes não morde. | NO |
| `npc_pip_dialogue_09` | RepeatFallback | Se alguém perguntar, eu não estava correndo. | YES |
| `npc_pip_dialogue_10` | Goodbye | Até já! Provavelmente antes do que você espera. | YES |

### 6.15 `npc_alaric` — Ser Alaric Veyr

**Objetivo:** Guarda, patrulha, combate e segurança.
**Role:** Guardião/Combatente.
**Zona:** GuardPost.
**Movimento:** Patrol/TownGate.
**Serviço/Shop:** `guard_combat_training`.
**Quest seeds:** Patrulha da Estrada Baixa, Relatório Incompleto, A Justiça Não Basta.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_alaric_dialogue_01` | Greeting | Postura reta. Olhos abertos. | YES |
| `npc_alaric_dialogue_02` | Role | Ser Alaric Veyr. A guarda mantém a estrada respirando. | YES |
| `npc_alaric_dialogue_03` | Location | Segurança não é ausência de perigo; é prontidão. | YES |
| `npc_alaric_dialogue_04` | GameplayTip | Não vá à caverna sem ferramenta, comida e retorno planejado. | YES |
| `npc_alaric_dialogue_05` | RumorNonSpoiler | Alguns relatórios somem porque verdade demais causa pânico. | NO |
| `npc_alaric_dialogue_06` | Contextual | Hoje a patrulha voltou com lama que não é desta estrada. | NO |
| `npc_alaric_dialogue_07` | ShopOrService | Posso treinar defesa, patrulhas e contratos de guarda quando o sistema existir. | YES |
| `npc_alaric_dialogue_08` | QuestFutureHook | Patrulha da Estrada Baixa começa com disciplina. | NO |
| `npc_alaric_dialogue_09` | RepeatFallback | Coragem sem dever é vaidade armada. | YES |
| `npc_alaric_dialogue_10` | Goodbye | Siga. | YES |

### 6.16 `npc_mirela` — Mirela dos Laços

**Objetivo:** Bolsas, roupas, acessórios.
**Role:** Artesão/Comerciante.
**Zona:** Tailor.
**Movimento:** ShopKeeperFixed.
**Serviço/Shop:** `tailor_bags_clothing`.
**Quest seeds:** Bolsa de Trabalho, Fio que Não Rasga, Roupa de Festival.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_mirela_dialogue_01` | Greeting | Bolso mal feito derruba mais item que monstro. | YES |
| `npc_mirela_dialogue_02` | Role | Mirela dos Laços. Costuro utilidade com aparência decente. | YES |
| `npc_mirela_dialogue_03` | Location | Roupa na cidade é função, status e armadura social. | YES |
| `npc_mirela_dialogue_04` | GameplayTip | Uma bolsa boa muda o quanto você aguenta trabalhar. | YES |
| `npc_mirela_dialogue_05` | RumorNonSpoiler | Vi tecido que se recompõe sozinho. Quero entender antes de vender. | NO |
| `npc_mirela_dialogue_06` | Contextual | Hoje Nimble mediu minha porta. De novo. | NO |
| `npc_mirela_dialogue_07` | ShopOrService | Posso vender bolsas, roupas e acessórios quando o sistema estiver pronto. | YES |
| `npc_mirela_dialogue_08` | QuestFutureHook | Bolsa de Trabalho começa com material resistente. | NO |
| `npc_mirela_dialogue_09` | RepeatFallback | Não, remendo torto não tem personalidade. | YES |
| `npc_mirela_dialogue_10` | Goodbye | Volte com tecido, couro ou necessidade. | YES |

### 6.17 `npc_renko` — Renko Três-Sorrisos

**Objetivo:** Loja geral, barganha e itens comuns.
**Role:** Comerciante/Artesão.
**Zona:** GeneralStore.
**Movimento:** ShopKeeperFixed.
**Serviço/Shop:** `general_store_bargain`.
**Quest seeds:** Preço de Amigo, Mercadoria Sem Dono, Três Sorrisos, Uma Mentira.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_renko_dialogue_01` | Greeting | Tenho preço de amigo, conhecido e alvo fácil. Escolha com cuidado. | YES |
| `npc_renko_dialogue_02` | Role | Renko Três-Sorrisos. Compro, vendo e negoço a verdade do preço. | YES |
| `npc_renko_dialogue_03` | Location | Mercado é onde a cidade confessa o que precisa. | YES |
| `npc_renko_dialogue_04` | GameplayTip | Compare preço antes de vender tudo no primeiro balcão. | YES |
| `npc_renko_dialogue_05` | RumorNonSpoiler | Mercadoria sem dono quase sempre tem dono perigoso. | NO |
| `npc_renko_dialogue_06` | Contextual | Hoje sorri duas vezes por lucro e uma por prevenção. | NO |
| `npc_renko_dialogue_07` | ShopOrService | Posso tocar loja geral, barganha e estoque comum quando o sistema existir. | YES |
| `npc_renko_dialogue_08` | QuestFutureHook | Preço de Amigo é uma lição, não uma promessa. | NO |
| `npc_renko_dialogue_09` | RepeatFallback | Se parece inútil, ainda não achei comprador. | YES |
| `npc_renko_dialogue_10` | Goodbye | Volte com moedas ou curiosidade. | YES |

### 6.18 `npc_eiran` — Eiran Valeclaro

**Objetivo:** Animais, pets e ração.
**Role:** Tratador/Plantador.
**Zona:** AnimalYard.
**Movimento:** WanderWithinZone/AnimalArea.
**Serviço/Shop:** `animals_pets_feed`.
**Quest seeds:** Primeira Tigela, Animal Assustado, Cuidado Não é Fraqueza.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_eiran_dialogue_01` | Greeting | Fale baixo. Eles escutam intenção. | YES |
| `npc_eiran_dialogue_02` | Role | Eiran Valeclaro. Cuido de animais e de gente que aprende devagar. | YES |
| `npc_eiran_dialogue_03` | Location | A cidade só parece humana porque os bichos toleram. | YES |
| `npc_eiran_dialogue_04` | GameplayTip | Alimente antes de pedir. Vale para quase tudo. | YES |
| `npc_eiran_dialogue_05` | RumorNonSpoiler | Animais evitaram a trilha leste. Isso costuma significar algo. | NO |
| `npc_eiran_dialogue_06` | Contextual | Hoje os pássaros calaram quando a pedra esquentou. | NO |
| `npc_eiran_dialogue_07` | ShopOrService | Posso desbloquear pets, ração e cuidados quando o sistema existir. | YES |
| `npc_eiran_dialogue_08` | QuestFutureHook | Primeira Tigela ensina mais que dez sermões. | NO |
| `npc_eiran_dialogue_09` | RepeatFallback | Não toque se ele se afastou primeiro. | YES |
| `npc_eiran_dialogue_10` | Goodbye | Volte sem pressa. | YES |

### 6.19 `npc_liora` — Liora Canta-Rio

**Objetivo:** Música, sonhos, Alihana e pistas de Anya.
**Role:** Músico/Pesquisador.
**Zona:** Tavern/StatueGarden.
**Movimento:** WanderWithinZone/EveningStage.
**Serviço/Shop:** `music_dream_lore`.
**Quest seeds:** Canção sem Autor, A Estátua que Escutou, Sonho de Água Clara.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_liora_dialogue_01` | Greeting | Algumas canções chegam antes de quem canta. | YES |
| `npc_liora_dialogue_02` | Role | Liora Canta-Rio. Eu lembro melodias que nunca aprendi. | YES |
| `npc_liora_dialogue_03` | Location | A cidade tem silêncio afinado demais em certos lugares. | YES |
| `npc_liora_dialogue_04` | GameplayTip | Ouça sons estranhos perto de estátuas, água e sonho. | YES |
| `npc_liora_dialogue_05` | RumorNonSpoiler | Uma música respondeu ao Jardim das Estátuas. Não foi eco. | NO |
| `npc_liora_dialogue_06` | Contextual | Hoje sonhei com água clara onde o mapa mostra pedra. | NO |
| `npc_liora_dialogue_07` | ShopOrService | Posso oferecer música, buffs sociais e pistas quando o sistema existir. | YES |
| `npc_liora_dialogue_08` | QuestFutureHook | Canção sem Autor começa quando você para de chamar de coincidência. | NO |
| `npc_liora_dialogue_09` | RepeatFallback | Se a melodia repetir, anote. | YES |
| `npc_liora_dialogue_10` | Goodbye | Que seu sono não minta. | YES |

### 6.20 `npc_orlan` — Orlan Pouso-Curto

**Objetivo:** Hospedagem, viajantes e notícias.
**Role:** Comerciante/Escriba.
**Zona:** Inn.
**Movimento:** ShopKeeperFixed.
**Serviço/Shop:** `inn_lodging_news`.
**Quest seeds:** Quarto de Viajante, Hóspede Sem Sombra, Conta Aberta.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_orlan_dialogue_01` | Greeting | Quem dorme aqui deixa nome, moeda e versão. | YES |
| `npc_orlan_dialogue_02` | Role | Orlan Pouso-Curto. Cuido da pousada e do que os viajantes esquecem. | YES |
| `npc_orlan_dialogue_03` | Location | Toda cidade muda quando chega caravana. | YES |
| `npc_orlan_dialogue_04` | GameplayTip | Notícia boa também precisa de fonte. | YES |
| `npc_orlan_dialogue_05` | RumorNonSpoiler | Um hóspede sem sombra pagou adiantado. Péssimo sinal. | NO |
| `npc_orlan_dialogue_06` | Contextual | Hoje três viajantes juraram não se conhecer. Mentiram no mesmo tom. | NO |
| `npc_orlan_dialogue_07` | ShopOrService | Posso oferecer hospedagem, notícias e rumores de rota quando o sistema existir. | YES |
| `npc_orlan_dialogue_08` | QuestFutureHook | Quarto de Viajante abre mais portas que espada. | NO |
| `npc_orlan_dialogue_09` | RepeatFallback | Se Gruta perguntou, eu não contei. Ainda. | YES |
| `npc_orlan_dialogue_10` | Goodbye | Volte antes da última chave. | YES |

### 6.21 `npc_savra` — Savra Escama-Verde

**Objetivo:** Ervas, antídotos, venenos e floresta.
**Role:** Curandeiro/Explorador.
**Zona:** Herbalist/ForestGate.
**Movimento:** Patrol/HerbRoute.
**Serviço/Shop:** `herbs_antidote_forest`.
**Quest seeds:** Antídoto Amargo, Praga que Anda, O Fungo de Baixo.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_savra_dialogue_01` | Greeting | Verde não significa veneno. Geralmente. | YES |
| `npc_savra_dialogue_02` | Role | Savra Escama-Verde. Conheço ervas que curam e ervas que negociam. | YES |
| `npc_savra_dialogue_03` | Location | A borda da cidade tem mais resposta que o centro. | YES |
| `npc_savra_dialogue_04` | GameplayTip | Antídoto ruim atrasa morte; antídoto bom evita história. | YES |
| `npc_savra_dialogue_05` | RumorNonSpoiler | Fungos de baixo não seguem lógica de superfície. | NO |
| `npc_savra_dialogue_06` | Contextual | Hoje as folhas perto da estrada tinham mordidas sem dentes. | NO |
| `npc_savra_dialogue_07` | ShopOrService | Posso vender ervas, antídotos e reagentes quando o sistema existir. | YES |
| `npc_savra_dialogue_08` | QuestFutureHook | Antídoto Amargo começa com coleta cuidadosa. | NO |
| `npc_savra_dialogue_09` | RepeatFallback | Não coma nada que brilhe por orgulho. | YES |
| `npc_savra_dialogue_10` | Goodbye | Volte se a ferida mudar de cor. | YES |

### 6.22 `npc_tovin` — Tovin Mãos-de-Selo

**Objetivo:** Registros, impostos, altares e permissões.
**Role:** Escriba/Artesão.
**Zona:** Registry.
**Movimento:** Stationary/PermitDesk.
**Serviço/Shop:** `permits_altars_registry`.
**Quest seeds:** Carimbo de Propriedade, Altar Permitido, Selo Sem Reino.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_tovin_dialogue_01` | Greeting | Sem selo, é intenção. Com selo, é problema rastreável. | YES |
| `npc_tovin_dialogue_02` | Role | Tovin Mãos-de-Selo. Transformo caos em formulário. | YES |
| `npc_tovin_dialogue_03` | Location | A cidade funciona porque alguém carimba o inevitável. | YES |
| `npc_tovin_dialogue_04` | GameplayTip | Antes de erguer altar, entenda o que está autorizando. | YES |
| `npc_tovin_dialogue_05` | RumorNonSpoiler | Um selo antigo sem reino não deveria abrir nenhuma porta. | NO |
| `npc_tovin_dialogue_06` | Contextual | Hoje encontrei três erros e dois cidadãos; proporção comum. | NO |
| `npc_tovin_dialogue_07` | ShopOrService | Posso registrar altares, permissões e expansão quando o sistema existir. | YES |
| `npc_tovin_dialogue_08` | QuestFutureHook | Carimbo de Propriedade evita disputa futura. | NO |
| `npc_tovin_dialogue_09` | RepeatFallback | Se o formulário é pequeno, desconfie. | YES |
| `npc_tovin_dialogue_10` | Goodbye | Volte com assinatura legível. | YES |

### 6.23 `npc_maelor` — Maelor Cinza

**Objetivo:** Nyx, memória, ruínas e segredos tardios.
**Role:** Explorador/Pesquisador.
**Zona:** StatueGarden/NightRoute.
**Movimento:** NightOnly/WanderHidden.
**Serviço/Shop:** `late_lore_memory`.
**Quest seeds:** Passos Onde Não Há Luz, Memória Que Escolheu Sumir, O Silêncio Também Protege.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_maelor_dialogue_01` | Greeting | Você olha como quem ainda lembra de esquecer. | YES |
| `npc_maelor_dialogue_02` | Role | Maelor Cinza. Alguns nomes sobrevivem melhor no escuro. | YES |
| `npc_maelor_dialogue_03` | Location | A cidade esqueceu algo de propósito. | YES |
| `npc_maelor_dialogue_04` | GameplayTip | Nem todo segredo precisa ser aberto no primeiro dia. | YES |
| `npc_maelor_dialogue_05` | RumorNonSpoiler | Há passos onde não há luz; siga só quando souber voltar. | NO |
| `npc_maelor_dialogue_06` | Contextual | Hoje a noite repetiu uma memória que não era minha. | NO |
| `npc_maelor_dialogue_07` | ShopOrService | Posso abrir investigações tardias de Nyx, memória e ruínas quando o sistema permitir. | YES |
| `npc_maelor_dialogue_08` | QuestFutureHook | Memória Que Escolheu Sumir não é quest para curioso apressado. | NO |
| `npc_maelor_dialogue_09` | RepeatFallback | Se me viu de dia, era outra pessoa. Ou mentira. | YES |
| `npc_maelor_dialogue_10` | Goodbye | Guarde silêncio até ele servir. | YES |

## 7. Shop/service mapping obrigatório

| NpcId | ServiceType | ShopId sugerido | Buy | Sell | Observação |
|---|---|---|---:|---:|---|
| `npc_corvus` | healing_blessing_temple | `shop_corvus` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_mara` | licenses_contracts | `shop_mara` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_sylveth` | seeds_crops_herbs | `shop_sylveth` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_brumdar` | blacksmith_repair_upgrade | `shop_brumdar` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_nimble` | construction_buildings_move | `shop_nimble` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_gurd` | heavy_clearance | `shop_gurd` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_hund` | guard_transport | `shop_hund` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_ozzra` | alchemy_potions_fertilizer | `shop_ozzra` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_gruta` | tavern_food_rumors | `shop_gruta` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_zrix` | maps_cave_contracts | `shop_zrix` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_yael` | night_shop_rare_items | `shop_yael` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_thalindra` | archive_lore_quests_blueprints | `shop_thalindra` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_dagna` | ore_mining_cave | `shop_dagna` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_pip` | tutorial_delivery | `shop_pip` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_alaric` | guard_combat_training | `` | 0 | 0 | Sem shop inicial obrigatório. |
| `npc_mirela` | tailor_bags_clothing | `shop_mirela` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_renko` | general_store_bargain | `shop_renko` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_eiran` | animals_pets_feed | `shop_eiran` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_liora` | music_dream_lore | `` | 0 | 0 | Sem shop inicial obrigatório. |
| `npc_orlan` | inn_lodging_news | `shop_orlan` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_savra` | herbs_antidote_forest | `shop_savra` | 1 | 0 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_tovin` | permits_altars_registry | `shop_tovin` | 1 | 1 | Serviço completo pode ser debt, mas NPC deve existir e abrir diálogo. |
| `npc_maelor` | late_lore_memory | `` | 0 | 0 | Sem shop inicial obrigatório. |

## 8. Movement schedules obrigatórios

| NpcId | MovementProfile | Implementar agora | Debt permitido |
|---|---|---:|---|
| `npc_corvus` | Stationary/TemplePatrol | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_mara` | Stationary/RegistryDesk | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_sylveth` | ShopKeeperFixed/FarmVisit | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_brumdar` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_nimble` | Patrol/WorkshopDesk | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_gurd` | Patrol/HeavyWorkZone | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_hund` | Patrol/TownRoad | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_ozzra` | WanderWithinZone/Lab | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_gruta` | ShopKeeperFixed/TavernStage | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_zrix` | Patrol/CaveRoad | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_yael` | NightOnly/WanderHidden | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_thalindra` | Stationary/ArchiveDesk | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_dagna` | Patrol/QuarryRoad | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_pip` | WanderWithinZone | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_alaric` | Patrol/TownGate | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_mirela` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_renko` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_eiran` | WanderWithinZone/AnimalArea | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_liora` | WanderWithinZone/EveningStage | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_orlan` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_savra` | Patrol/HerbRoute | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_tovin` | Stationary/PermitDesk | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |
| `npc_maelor` | NightOnly/WanderHidden | YES | DAILY_SCHEDULE_DEFERRED se horários finais não existirem |

## 9. Artefatos que o Codex/Claude deve criar/atualizar

```text
docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md
docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_TOWN_PLACEMENT_MAP.md
docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_DIALOGUE_SETS.md
docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_MOVEMENT_SCHEDULES.md
docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_SHOP_SERVICES.md
docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_IMPLEMENTATION_REPORT.md
docs/validation/WAVE_INTEGRATION_12C_HUMAN_PLAYMODE_CHECKLIST.md
docs/project/CURRENT_STATE.md
```

Código/data se necessário:

```text
Assets/_Game/Data/NPCs/*.asset ou equivalente
Assets/_Game/Data/Dialogue/*.asset ou equivalente
Assets/_Game/Data/Shops/*.asset ou equivalente
Assets/_Game/Scripts/NPC/Runtime/*, somente genérico
Assets/_Game/Scripts/Editor/Validation/ValidateRefinedCanonicalNpcTownPopulation.cs
Assets/_Game/Scenes/TownScene.unity
```

## 10. Prompt de execução para Codex/Claude Code

```text
Estamos no repo rafa210587/cindars_hope, branch dev.

Objetivo:
Executar WAVE_INTEGRATION_12C_REFINED_CANONICAL_NPC_TOWN_POPULATION usando esta spec como fonte direta. Inserir todos os 23 NPCs canônicos refinados na TownScene com NpcId, DisplayName, função, propósito, diálogo com >=10 entries, movement profile e shop/service quando aplicável.

Leia primeiro:
- docs/specs/a_implementar/WAVE_INTEGRATION_12C_refined_canonical_npc_town_population.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
- docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md
- docs/project/CURRENT_STATE.md

NÃO executar WAVE13/14/15.
NÃO criar uma classe por NPC.
NÃO criar sistema social/romance/reputation final.
NÃO criar quests finais; somente seed metadata/dialogue hooks.
NÃO duplicar NPC existente; migrar/atualizar se já existir.
NÃO deixar os 19 NPCs canônicos como FUTURE_SCOPE nesta spec.
NÃO alterar Packages/ProjectSettings.

Passos:
1. Preflight: git fetch origin dev; garantir branch dev e working tree limpa.
2. Auditar TownScene e assets atuais.
3. Criar/atualizar data assets/definitions para todos os 23 NPCs listados na spec.
4. Criar/atualizar DialogueSets com >=10 entries por NPC usando as frases desta spec.
5. Criar/atualizar Shop/Service definitions para NPCs comerciais/serviço.
6. Posicionar todos os 23 NPCs na TownScene, em zonas funcionais, sem sobrepor e alcançáveis pelo player.
7. Garantir NpcSceneInteractable + DialogueBridge para todos.
8. Garantir ShopBridge para NPCs comerciais quando shop runtime existir; se não, debt explícito.
9. Garantir movement profiles; Patrol/Wander/NightRoute devem mover ou ter debt claro se runtime bloquear.
10. Criar validator ValidateRefinedCanonicalNpcTownPopulation.cs.
11. Atualizar docs de validação e CURRENT_STATE.
12. Rodar dotnet build Assembly-CSharp e Assembly-CSharp-Editor.
13. Rodar docs validation e quality check.
14. Commitar apenas arquivos da WAVE12C.

Critérios de sucesso:
- 23 NPCs no roster 12C.
- 23 NPCs na TownScene ou human wiring explícito por NPC, mas preferir scene wired.
- 23 NPCs com DialogueSet >=10 entries.
- todos com Purpose e Responsibilities.
- todos com MovementProfile.
- merchants/services com ShopId/ServiceType.
- builds passam.

Commit:
git commit -m "feat: populate town with full refined canonical npc roster"
git push origin dev

Resposta final obrigatória:
Status:
Branch:
Canonical NPC source:
NPC count required:
NPCs placed:
NPCs with dialogue >=10:
NPCs with movement profile:
NPCs with shop/service:
Scene changes:
Code/data created:
Assembly-CSharp:
Assembly-CSharp-Editor:
Docs validation:
Quality check:
Commit:
Pushed:
Remote HEAD:
Human Play Mode validation needed:
```

## 11. Checklist humano

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open TownScene | Scene opens |  |  |
| Press Play | Game starts |  |  |
| Player movement | Player moves |  |  |
| Count NPCs | 23 canonical NPCs visible/reachable |  |  |
| Interact each NPC | Dialogue opens with correct name |  |  |
| Dialogue coverage | Each NPC has >=10 entries in data |  |  |
| Merchants/services | Shop/service opens or debt explicit |  |  |
| Movement | Patrol/Wander/NightRoute NPCs move or debt explicit |  |  |
| Close UI | Gameplay input returns |  |  |
| Stop Play | Scene not corrupted |  |  |

## 12. Status esperado

```text
BUILD_VALIDATED_WITH_REFINED_NPC_DEBT
```

Só usar `BUILD_VALIDATED_SCENE_WIRED` se os 23 NPCs estiverem posicionados, interagíveis e com diálogo abrindo em Play Mode.
