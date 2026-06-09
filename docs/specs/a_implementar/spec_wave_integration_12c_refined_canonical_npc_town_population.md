# /speckit.specify
# /speckit.plan
# /speckit.tasks
Ordem de execucao: WAVE_INTEGRATION_12C
Depende de: WAVE_INTEGRATION_12
Bloqueia: WAVE13 until human Play Mode validation
required_adrs: []
required_game_rules: []
# WAVE_INTEGRATION_12C â€” Refined Canonical NPC Town Population

> **Projeto:** Cindar's Hope
> **Tipo:** Spec implementÃ¡vel / Data + Scene Wiring / NPC roster canÃ´nico refinado
> **Aplica sobre:** WAVE_INTEGRATION_12
> **Objetivo:** inserir todos os 23 NPCs refinados/canÃ´nicos da cidade na TownScene, com objetivo funcional, localizaÃ§Ã£o, movimento, diÃ¡logo mÃ­nimo e serviÃ§o/shop quando aplicÃ¡vel.
> **Regra:** nÃ£o Ã© â€œdescobrir quem sÃ£o os NPCsâ€; os NPCs estÃ£o listados explicitamente nesta spec.

## 0. Problema que esta spec corrige

A WAVE12 atual colocou apenas um subconjunto de NPCs na TownScene. Esta spec promove **todos os 23 NPCs canÃ´nicos refinados** para presenÃ§a visÃ­vel na cidade, ainda que alguns serviÃ§os avanÃ§ados fiquem com debt.

A execuÃ§Ã£o deve criar/povoar a cidade com NPCs reais do design direction, nÃ£o placeholders genÃ©ricos.

## 1. Fontes obrigatÃ³rias

Ler e usar:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md
docs/project/CURRENT_STATE.md
```

Se houver conflito entre esta spec e `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`, parar com `BLOCKED_BY_DESIGN_DIRECTION_CONFLICT`.

## 2. Regras de implementaÃ§Ã£o

```text
NÃƒO criar uma classe por NPC.
NÃƒO criar sistema social/romance final.
NÃƒO criar quest final.
NÃƒO duplicar DialogueService, ShopService, InventoryManager ou EconomyManager.
NÃƒO deixar placeholder genÃ©rico se o NPC canÃ´nico estÃ¡ listado aqui.
NÃƒO declarar sucesso se algum dos 23 NPCs nÃ£o estiver no roster/scene/wiring ou debt explÃ­cito.
NÃƒO alterar Packages/ProjectSettings.
```

Modelo obrigatÃ³rio:

```text
NpcDefinition data-driven
NpcScenePlacementMarker
NpcSceneInteractable
NpcDialogueBridge
NpcShopBridge, se aplicÃ¡vel
NpcMovementController genÃ©rico
DialogueTreeSO/DialogueSet data-driven
ShopDataSO/ShopDefinition data-driven
```

## 3. CritÃ©rio real de sucesso

Ao abrir a TownScene no Unity, o jogador deve conseguir:

```text
1. Ver todos os 23 NPCs canÃ´nicos refinados.
2. Chegar atÃ© cada NPC.
3. Interagir com cada NPC.
4. Abrir diÃ¡logo com cada NPC.
5. Ver o nome correto do NPC.
6. Ter pelo menos 10 entries por NPC no DialogueSet.
7. Abrir shop/service para NPCs comerciais quando o serviÃ§o jÃ¡ existir.
8. Ver movimento para NPCs Patrol/Wander/NightRoute ou debt explÃ­cito.
9. Fechar diÃ¡logo/shop sem travar input/modal.
```

## 4. Roster canÃ´nico refinado â€” implementar todos

| NpcId | Nome | Role | Objetivo no gameplay | Zona da cidade | MovementProfile | ServiÃ§o/Shop | Implementar agora |
|---|---|---|---|---|---|---|---:|
| `npc_corvus` | Padre Corvus | Curandeiro/GuardiÃ£o | Templo de Kanthor | Temple | Stationary/TemplePatrol | healing_blessing_temple | YES |
| `npc_mara` | Mara Vellum | Escriba/Comerciante | CartÃ³rio e licenÃ§as | Registry | Stationary/RegistryDesk | licenses_contracts | YES |
| `npc_sylveth` | Sylveth | Plantador/Curandeiro | Sementes, crops e ritos de Thandra | SeedShop/Garden | ShopKeeperFixed/FarmVisit | seeds_crops_herbs | YES |
| `npc_brumdar` | Brumdar Ferro-Quieto | ArtesÃ£o/Combatente | Forja, ferramentas, armas e minÃ©rio | Forge | ShopKeeperFixed | blacksmith_repair_upgrade | YES |
| `npc_nimble` | Nimble Galhobaixo | Construtor/ArtesÃ£o | ConstruÃ§Ãµes, layout e mover estruturas | Carpenter | Patrol/WorkshopDesk | construction_buildings_move | YES |
| `npc_gurd` | Gurd Carvalho-Torto | Construtor/Combatente | Limpeza pesada, expansÃ£o e forÃ§a bruta | ConstructionYard | Patrol/HeavyWorkZone | heavy_clearance | YES |
| `npc_hund` | Hund Carvalho-Torto | GuardiÃ£o/Construtor | Patrulha, defesa e transporte seguro | GuardRoute | Patrol/TownRoad | guard_transport | YES |
| `npc_ozzra` | Ozzra FumaÃ§azul | Alquimista/ArtesÃ£o | PoÃ§Ãµes, fertilizantes e reagentes | AlchemyLab | WanderWithinZone/Lab | alchemy_potions_fertilizer | YES |
| `npc_gruta` | Gruta Panela-Funda | Comerciante/MÃºsico | Taverna, comida, rumores e buffs sociais | Tavern | ShopKeeperFixed/TavernStage | tavern_food_rumors | YES |
| `npc_zrix` | Zrix das Estradas | Explorador/Comerciante | Mapas, caverna, rotas e contratos | Guild/RoadGate | Patrol/CaveRoad | maps_cave_contracts | YES |
| `npc_yael` | Yael Noite-Mansa | Comerciante/Explorador | Loja noturna, itens raros e segredos | NightMarket | NightOnly/WanderHidden | night_shop_rare_items | YES |
| `npc_thalindra` | Thalindra VÃ©u-de-Lua | Pesquisador/Alquimista | Arquivo, lore, Cindar, Anya, Elyndor/BromÃ©cia | Archive | Stationary/ArchiveDesk | archive_lore_quests_blueprints | YES |
| `npc_dagna` | Dagna Rocha-Morna | Minerador/Combatente | MineraÃ§Ã£o, pedreira, cave ore | Quarry/MineOffice | Patrol/QuarryRoad | ore_mining_cave | YES |
| `npc_pip` | Pip Semente-Solta | Comerciante/Explorador | Tutorial, recados, entrega e humor | TownEntrance/Market | WanderWithinZone | tutorial_delivery | YES |
| `npc_alaric` | Ser Alaric Veyr | GuardiÃ£o/Combatente | Guarda, patrulha, combate e seguranÃ§a | GuardPost | Patrol/TownGate | guard_combat_training | YES |
| `npc_mirela` | Mirela dos LaÃ§os | ArtesÃ£o/Comerciante | Bolsas, roupas, acessÃ³rios | Tailor | ShopKeeperFixed | tailor_bags_clothing | YES |
| `npc_renko` | Renko TrÃªs-Sorrisos | Comerciante/ArtesÃ£o | Loja geral, barganha e itens comuns | GeneralStore | ShopKeeperFixed | general_store_bargain | YES |
| `npc_eiran` | Eiran Valeclaro | Tratador/Plantador | Animais, pets e raÃ§Ã£o | AnimalYard | WanderWithinZone/AnimalArea | animals_pets_feed | YES |
| `npc_liora` | Liora Canta-Rio | MÃºsico/Pesquisador | MÃºsica, sonhos, Alihana e pistas de Anya | Tavern/StatueGarden | WanderWithinZone/EveningStage | music_dream_lore | YES |
| `npc_orlan` | Orlan Pouso-Curto | Comerciante/Escriba | Hospedagem, viajantes e notÃ­cias | Inn | ShopKeeperFixed | inn_lodging_news | YES |
| `npc_savra` | Savra Escama-Verde | Curandeiro/Explorador | Ervas, antÃ­dotos, venenos e floresta | Herbalist/ForestGate | Patrol/HerbRoute | herbs_antidote_forest | YES |
| `npc_tovin` | Tovin MÃ£os-de-Selo | Escriba/ArtesÃ£o | Registros, impostos, altares e permissÃµes | Registry | Stationary/PermitDesk | permits_altars_registry | YES |
| `npc_maelor` | Maelor Cinza | Explorador/Pesquisador | Nyx, memÃ³ria, ruÃ­nas e segredos tardios | StatueGarden/NightRoute | NightOnly/WanderHidden | late_lore_memory | YES |

## 5. Zonas de posicionamento recomendadas

| ZoneKey | Uso |
|---|---|
| Temple | templo de Kanthor / lado norte da praÃ§a |
| Registry | cartÃ³rio / prefeitura local / mesa de registros |
| SeedShop/Garden | loja de sementes e jardim rural |
| Forge | forja / oficina metalÃºrgica |
| Carpenter | carpintaria / pÃ¡tio de obras |
| ConstructionYard | pÃ¡tio de construÃ§Ã£o pesada |
| GuardRoute | rota entre portÃ£o, poÃ§o e estrada baixa |
| AlchemyLab | laboratÃ³rio de alquimia |
| Tavern | taverna |
| Guild/RoadGate | guilda/estrada para caverna |
| NightMarket | loja noturna / beco de Nyx |
| Archive | arquivo/biblioteca |
| Quarry/MineOffice | pedreira / escritÃ³rio de mineraÃ§Ã£o |
| TownEntrance/Market | entrada da cidade / mercado |
| GuardPost | posto da guarda |
| Tailor | alfaiataria / loja de bolsas |
| GeneralStore | loja geral |
| AnimalYard | Ã¡rea de animais |
| Tavern/StatueGarden | taverna ou jardim das estÃ¡tuas |
| Inn | pousada |
| Herbalist/ForestGate | ervanÃ¡rio / portÃ£o da floresta |
| StatueGarden/NightRoute | jardim das estÃ¡tuas / rota noturna |

## 6. DialogueSets obrigatÃ³rios

Cada NPC abaixo deve ter pelo menos 10 entries. As frases sÃ£o seed text implementÃ¡vel. Elas podem entrar em `DialogueTreeSO`, `DialogueSet`, JSON/config ou data asset equivalente. Se o runtime atual nÃ£o suporta 10 escolhas simultÃ¢neas, usar Ã¡rvore/continue/branch, mas manter as 10 entries no data.

### 6.1 `npc_corvus` â€” Padre Corvus

**Objetivo:** Templo de Kanthor.
**Role:** Curandeiro/GuardiÃ£o.
**Zona:** Temple.
**Movimento:** Stationary/TemplePatrol.
**ServiÃ§o/Shop:** `healing_blessing_temple`.
**Quest seeds:** O Sino que NÃ£o Toca, Juramento Partido, O Nome Sob a Pedra.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_corvus_dialogue_01` | Greeting | Que Kanthor pese seus passos com justiÃ§a. | YES |
| `npc_corvus_dialogue_02` | Role | Sou Corvus, servo do templo e guardiÃ£o dos juramentos da cidade. | YES |
| `npc_corvus_dialogue_03` | Location | Cindar's Hope parece simples, mas toda cidade guarda pedra sob a terra. | YES |
| `npc_corvus_dialogue_04` | GameplayTip | Procure ajuda antes de descer Ã  caverna; coragem sem preparo vira luto. | YES |
| `npc_corvus_dialogue_05` | RumorNonSpoiler | HÃ¡ sÃ­mbolos antigos sob o templo que poucos gostam de mencionar. | NO |
| `npc_corvus_dialogue_06` | Contextual | Hoje o templo estÃ¡ quieto demais; isso raramente Ã© bom. | NO |
| `npc_corvus_dialogue_07` | ShopOrService | Posso oferecer cura, bÃªnÃ§Ã£o menor e orientaÃ§Ã£o quando o sistema estiver pronto. | YES |
| `npc_corvus_dialogue_08` | QuestFutureHook | Um sino rachado espera mÃ£os capazes e metal honesto. | NO |
| `npc_corvus_dialogue_09` | RepeatFallback | Repito: ordem nÃ£o Ã© medo; ordem Ã© o que protege o fraco. | YES |
| `npc_corvus_dialogue_10` | Goodbye | Volte em seguranÃ§a. A balanÃ§a pesa tambÃ©m quem sobrevive. | YES |

### 6.2 `npc_mara` â€” Mara Vellum

**Objetivo:** CartÃ³rio e licenÃ§as.
**Role:** Escriba/Comerciante.
**Zona:** Registry.
**Movimento:** Stationary/RegistryDesk.
**ServiÃ§o/Shop:** `licenses_contracts`.
**Quest seeds:** LicenÃ§a de Primeira Obra, As PÃ¡ginas Arrancadas, Lei ou CompaixÃ£o.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_mara_dialogue_01` | Greeting | Nome, propriedade e intenÃ§Ã£o. Nessa ordem. | YES |
| `npc_mara_dialogue_02` | Role | Sou Mara Vellum; registro o que a cidade prefere esquecer informalmente. | YES |
| `npc_mara_dialogue_03` | Location | Sem licenÃ§a, atÃ© uma boa ideia vira problema pÃºblico. | YES |
| `npc_mara_dialogue_04` | GameplayTip | Antes de construir, confira materiais, taxas e autorizaÃ§Ã£o. | YES |
| `npc_mara_dialogue_05` | RumorNonSpoiler | HÃ¡ pÃ¡ginas arrancadas nos arquivos. NinguÃ©m arranca papel sem motivo. | NO |
| `npc_mara_dialogue_06` | Contextual | Hoje revisei trÃªs contratos e uma mentira muito mal escrita. | NO |
| `npc_mara_dialogue_07` | ShopOrService | Posso cuidar de licenÃ§as, contratos e reputaÃ§Ã£o formal quando o sistema existir. | YES |
| `npc_mara_dialogue_08` | QuestFutureHook | Sua primeira obra precisa de registro, nÃ£o sÃ³ madeira. | NO |
| `npc_mara_dialogue_09` | RepeatFallback | Se jÃ¡ expliquei, estÃ¡ registrado. Se nÃ£o estÃ¡ registrado, nÃ£o expliquei. | YES |
| `npc_mara_dialogue_10` | Goodbye | Leve isto a sÃ©rio. A cidade sobrevive por acordos. | YES |

### 6.3 `npc_sylveth` â€” Sylveth

**Objetivo:** Sementes, crops e ritos de Thandra.
**Role:** Plantador/Curandeiro.
**Zona:** SeedShop/Garden.
**Movimento:** ShopKeeperFixed/FarmVisit.
**ServiÃ§o/Shop:** `seeds_crops_herbs`.
**Quest seeds:** Sementes de Retorno, Festival de Thandra, RaÃ­zes que Ouvem.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_sylveth_dialogue_01` | Greeting | A terra responde melhor quando vocÃª para de apressÃ¡-la. | YES |
| `npc_sylveth_dialogue_02` | Role | Sou Sylveth; cuido das sementes e dos pequenos ritos de Thandra. | YES |
| `npc_sylveth_dialogue_03` | Location | A cidade vive do que cresce ao redor dela. | YES |
| `npc_sylveth_dialogue_04` | GameplayTip | Regue cedo, observe a estaÃ§Ã£o e nÃ£o trate solo como baÃº. | YES |
| `npc_sylveth_dialogue_05` | RumorNonSpoiler | Algumas raÃ­zes parecem ouvir a Fonte. Isso me preocupa. | NO |
| `npc_sylveth_dialogue_06` | Contextual | Hoje as folhas estÃ£o viradas para o vento errado. | NO |
| `npc_sylveth_dialogue_07` | ShopOrService | Posso vender sementes, ervas e orientar crops quando o estoque estiver ligado. | YES |
| `npc_sylveth_dialogue_08` | QuestFutureHook | Sementes antigas podem retornar se vocÃª recuperar o caminho delas. | NO |
| `npc_sylveth_dialogue_09` | RepeatFallback | Eu jÃ¡ disse: planta tambÃ©m tem humor. | YES |
| `npc_sylveth_dialogue_10` | Goodbye | Que Thandra deixe algo vivo no seu caminho. | YES |

### 6.4 `npc_brumdar` â€” Brumdar Ferro-Quieto

**Objetivo:** Forja, ferramentas, armas e minÃ©rio.
**Role:** ArtesÃ£o/Combatente.
**Zona:** Forge.
**Movimento:** ShopKeeperFixed.
**ServiÃ§o/Shop:** `blacksmith_repair_upgrade`.
**Quest seeds:** A Primeira Ferramenta SÃ©ria, Metal que Sussurra, O Martelo do Aprendiz.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_brumdar_dialogue_01` | Greeting | Se a ferramenta falhou, ou o metal mentiu ou a mÃ£o apressou. | YES |
| `npc_brumdar_dialogue_02` | Role | Brumdar Ferro-Quieto. Forjo, conserto e desconfio. | YES |
| `npc_brumdar_dialogue_03` | Location | MinÃ©rio desta regiÃ£o tem memÃ³ria estranha. | YES |
| `npc_brumdar_dialogue_04` | GameplayTip | NÃ£o leve ferramenta fraca para trabalho pesado. | YES |
| `npc_brumdar_dialogue_05` | RumorNonSpoiler | Metal que sussurra perto de pedra escura nÃ£o Ã© superstiÃ§Ã£o. | NO |
| `npc_brumdar_dialogue_06` | Contextual | Hoje a forja cantou baixo. NÃ£o gosto quando ela canta baixo. | NO |
| `npc_brumdar_dialogue_07` | ShopOrService | Posso melhorar ferramentas, armas e reparos quando a bancada estiver ligada. | YES |
| `npc_brumdar_dialogue_08` | QuestFutureHook | Sua primeira ferramenta sÃ©ria precisa de material sÃ©rio. | NO |
| `npc_brumdar_dialogue_09` | RepeatFallback | Se quer elogio, traga liga limpa. | YES |
| `npc_brumdar_dialogue_10` | Goodbye | Volte com minÃ©rio, nÃ£o com desculpa. | YES |

### 6.5 `npc_nimble` â€” Nimble Galhobaixo

**Objetivo:** ConstruÃ§Ãµes, layout e mover estruturas.
**Role:** Construtor/ArtesÃ£o.
**Zona:** Carpenter.
**Movimento:** Patrol/WorkshopDesk.
**ServiÃ§o/Shop:** `construction_buildings_move`.
**Quest seeds:** Madeira, Pedra e Assinatura, Mover Ã© Mais DifÃ­cil que Erguer, A TÃ¡bua que Cura.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_nimble_dialogue_01` | Greeting | Essa cerca estÃ¡ dois passos fora do bom senso. | YES |
| `npc_nimble_dialogue_02` | Role | Nimble Galhobaixo, construtor, medidor e crÃ­tico de layouts horrÃ­veis. | YES |
| `npc_nimble_dialogue_03` | Location | Uma cidade viva precisa de casa, oficina e espaÃ§o para erro. | YES |
| `npc_nimble_dialogue_04` | GameplayTip | Planeje antes de gastar madeira; mover depois custa paciÃªncia. | YES |
| `npc_nimble_dialogue_05` | RumorNonSpoiler | Vi uma tÃ¡bua bromeciana se reparar sozinha. PÃ©ssimo para orÃ§amento. | NO |
| `npc_nimble_dialogue_06` | Contextual | Hoje medi a praÃ§a e ela continua torta. | NO |
| `npc_nimble_dialogue_07` | ShopOrService | Posso construir, mover estruturas e planejar expansÃ£o quando o sistema estiver pronto. | YES |
| `npc_nimble_dialogue_08` | QuestFutureHook | Sua fazenda precisa de madeira, pedra e assinatura. | NO |
| `npc_nimble_dialogue_09` | RepeatFallback | NÃ£o, diagonal nÃ£o Ã© sempre economia de espaÃ§o. | YES |
| `npc_nimble_dialogue_10` | Goodbye | Volte quando tiver material ou arrependimento. | YES |

### 6.6 `npc_gurd` â€” Gurd Carvalho-Torto

**Objetivo:** Limpeza pesada, expansÃ£o e forÃ§a bruta.
**Role:** Construtor/Combatente.
**Zona:** ConstructionYard.
**Movimento:** Patrol/HeavyWorkZone.
**ServiÃ§o/Shop:** `heavy_clearance`.
**Quest seeds:** Pedra Grande, Martelo Maior, A Parede que NÃ£o Quebrou, ForÃ§a sem FÃºria.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_gurd_dialogue_01` | Greeting | Problema grande? Martelo maior. | YES |
| `npc_gurd_dialogue_02` | Role | Gurd. Eu carrego, quebro e pergunto depois se precisava. | YES |
| `npc_gurd_dialogue_03` | Location | Cidade cresce quando alguÃ©m tira pedra do caminho. | YES |
| `npc_gurd_dialogue_04` | GameplayTip | Se o obstÃ¡culo Ã© pesado, chame quem aguenta. | YES |
| `npc_gurd_dialogue_05` | RumorNonSpoiler | A parede antiga que nÃ£o quebrou ainda me irrita. | NO |
| `npc_gurd_dialogue_06` | Contextual | Hoje ninguÃ©m brigou comigo. Dia suspeito. | NO |
| `npc_gurd_dialogue_07` | ShopOrService | Posso ajudar em limpeza pesada e expansÃ£o quando o sistema existir. | YES |
| `npc_gurd_dialogue_08` | QuestFutureHook | Pedra grande pede martelo maior, nÃ£o discurso. | NO |
| `npc_gurd_dialogue_09` | RepeatFallback | JÃ¡ falei que nÃ£o Ã© raiva se Ã© construÃ§Ã£o? | YES |
| `npc_gurd_dialogue_10` | Goodbye | Volte quando tiver algo que precise sair do lugar. | YES |

### 6.7 `npc_hund` â€” Hund Carvalho-Torto

**Objetivo:** Patrulha, defesa e transporte seguro.
**Role:** GuardiÃ£o/Construtor.
**Zona:** GuardRoute.
**Movimento:** Patrol/TownRoad.
**ServiÃ§o/Shop:** `guard_transport`.
**Quest seeds:** A Entrega que NÃ£o Chegou, Barulho no PoÃ§o, Guardar sem Mandar.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_hund_dialogue_01` | Greeting | Eu ouvi vocÃª chegando antes de ver. | YES |
| `npc_hund_dialogue_02` | Role | Hund Carvalho-Torto. Vigio rotas, cargas e gente distraÃ­da. | YES |
| `npc_hund_dialogue_03` | Location | Nem todo perigo ruge; alguns rangem no poÃ§o. | YES |
| `npc_hund_dialogue_04` | GameplayTip | NÃ£o atravesse estrada baixa sem olhar para trÃ¡s. | YES |
| `npc_hund_dialogue_05` | RumorNonSpoiler | Uma carga desapareceu sem marca de roda. Isso nÃ£o acontece sozinho. | NO |
| `npc_hund_dialogue_06` | Contextual | Hoje o vento trouxe barulho de pedra oca. | NO |
| `npc_hund_dialogue_07` | ShopOrService | Posso escoltar, proteger entregas e reforÃ§ar rotas quando o sistema estiver pronto. | YES |
| `npc_hund_dialogue_08` | QuestFutureHook | A entrega que nÃ£o chegou precisa de olhos e passos firmes. | NO |
| `npc_hund_dialogue_09` | RepeatFallback | Meu irmÃ£o quebra. Eu observo onde quebrar. | YES |
| `npc_hund_dialogue_10` | Goodbye | Ande com atenÃ§Ã£o. | YES |

### 6.8 `npc_ozzra` â€” Ozzra FumaÃ§azul

**Objetivo:** PoÃ§Ãµes, fertilizantes e reagentes.
**Role:** Alquimista/ArtesÃ£o.
**Zona:** AlchemyLab.
**Movimento:** WanderWithinZone/Lab.
**ServiÃ§o/Shop:** `alchemy_potions_fertilizer`.
**Quest seeds:** Explode SÃ³ um Pouco, Fertilizante FumaÃ§azul, Reagente que Bebe Luz.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_ozzra_dialogue_01` | Greeting | NÃ£o explodiu. Isso Ã© progresso. | YES |
| `npc_ozzra_dialogue_02` | Role | Ozzra FumaÃ§azul, alquimista oficial de quase nenhuma licenÃ§a. | YES |
| `npc_ozzra_dialogue_03` | Location | Tudo na cidade pode virar mistura. Algumas coisas sÃ³ uma vez. | YES |
| `npc_ozzra_dialogue_04` | GameplayTip | PoÃ§Ã£o boa precisa de reagente certo e distÃ¢ncia prudente. | YES |
| `npc_ozzra_dialogue_05` | RumorNonSpoiler | HÃ¡ cristais que bebem luz. Quero um. Com luvas. | NO |
| `npc_ozzra_dialogue_06` | Contextual | Hoje sÃ³ trÃªs frascos gritaram. | NO |
| `npc_ozzra_dialogue_07` | ShopOrService | Posso vender poÃ§Ãµes, fertilizantes e reagentes quando o estoque estiver ligado. | YES |
| `npc_ozzra_dialogue_08` | QuestFutureHook | Fertilizante FumaÃ§azul melhora colheita ou ensina humildade. | NO |
| `npc_ozzra_dialogue_09` | RepeatFallback | Se sair fumaÃ§a azul, ainda estÃ¡ sob controle. Talvez. | YES |
| `npc_ozzra_dialogue_10` | Goodbye | Volta com ervas. E sem Corvus. | YES |

### 6.9 `npc_gruta` â€” Gruta Panela-Funda

**Objetivo:** Taverna, comida, rumores e buffs sociais.
**Role:** Comerciante/MÃºsico.
**Zona:** Tavern.
**Movimento:** ShopKeeperFixed/TavernStage.
**ServiÃ§o/Shop:** `tavern_food_rumors`.
**Quest seeds:** Sopa para um Dia Ruim, Rumor Queimado, Banquete de Festival.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_gruta_dialogue_01` | Greeting | Senta, come e fala antes que eu adivinhe. | YES |
| `npc_gruta_dialogue_02` | Role | Gruta Panela-Funda. Minha taverna alimenta e escuta. | YES |
| `npc_gruta_dialogue_03` | Location | Uma cidade faminta acredita em qualquer mentira. | YES |
| `npc_gruta_dialogue_04` | GameplayTip | Comida reduz cansaÃ§o melhor que orgulho vazio. | YES |
| `npc_gruta_dialogue_05` | RumorNonSpoiler | SÃ­mbolos sob mesas sÃ£o ruim para apetite e bom para rumor. | NO |
| `npc_gruta_dialogue_06` | Contextual | Hoje a sopa estÃ¡ forte e os boatos tambÃ©m. | NO |
| `npc_gruta_dialogue_07` | ShopOrService | Posso vender comida, ouvir rumores e preparar buffs sociais quando o sistema existir. | YES |
| `npc_gruta_dialogue_08` | QuestFutureHook | Sopa para um dia ruim comeÃ§a com ingrediente honesto. | NO |
| `npc_gruta_dialogue_09` | RepeatFallback | Se quebrar cadeira, compra duas. | YES |
| `npc_gruta_dialogue_10` | Goodbye | Vai. Mas volta antes de virar histÃ³ria. | YES |

### 6.10 `npc_zrix` â€” Zrix das Estradas

**Objetivo:** Mapas, caverna, rotas e contratos.
**Role:** Explorador/Comerciante.
**Zona:** Guild/RoadGate.
**Movimento:** Patrol/CaveRoad.
**ServiÃ§o/Shop:** `maps_cave_contracts`.
**Quest seeds:** Mapa de Entrada, Sinal de Elyndor, A Estrada que Desce.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_zrix_dialogue_01` | Greeting | Todo caminho cobra algo. O truque Ã© saber antes. | YES |
| `npc_zrix_dialogue_02` | Role | Zrix das Estradas. Mapeio rotas que fingem nÃ£o existir. | YES |
| `npc_zrix_dialogue_03` | Location | A caverna Ã© estrada ruim, nÃ£o destino glorioso. | YES |
| `npc_zrix_dialogue_04` | GameplayTip | Leve suprimento, marque retorno e nÃ£o confie em eco. | YES |
| `npc_zrix_dialogue_05` | RumorNonSpoiler | Vi sinal de Elyndor onde nÃ£o devia haver sinal nenhum. | NO |
| `npc_zrix_dialogue_06` | Contextual | Hoje a estrada baixa estÃ¡ com cheiro de chuva antiga. | NO |
| `npc_zrix_dialogue_07` | ShopOrService | Posso vender mapas, contratos e suprimentos de rota quando o sistema estiver pronto. | YES |
| `npc_zrix_dialogue_08` | QuestFutureHook | Mapa de entrada salva mais vidas que espada bonita. | NO |
| `npc_zrix_dialogue_09` | RepeatFallback | Atalho sÃ³ Ã© atalho depois que vocÃª volta. | YES |
| `npc_zrix_dialogue_10` | Goodbye | Se descer, volte contando. | YES |

### 6.11 `npc_yael` â€” Yael Noite-Mansa

**Objetivo:** Loja noturna, itens raros e segredos.
**Role:** Comerciante/Explorador.
**Zona:** NightMarket.
**Movimento:** NightOnly/WanderHidden.
**ServiÃ§o/Shop:** `night_shop_rare_items`.
**Quest seeds:** Aberto Depois da Meia-Noite, Comprador de Fragmentos, PreÃ§o do SilÃªncio.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_yael_dialogue_01` | Greeting | VocÃª nÃ£o me viu. Ainda. | YES |
| `npc_yael_dialogue_02` | Role | Yael Noite-Mansa. Vendo o que nÃ£o convÃ©m nomear alto. | YES |
| `npc_yael_dialogue_03` | Location | A cidade tem lojas de dia e verdades Ã  noite. | YES |
| `npc_yael_dialogue_04` | GameplayTip | Nem todo item raro ajuda; alguns apenas cobram depois. | YES |
| `npc_yael_dialogue_05` | RumorNonSpoiler | Fragmentos escuros atraem compradores piores que ladrÃµes. | NO |
| `npc_yael_dialogue_06` | Contextual | Hoje a sombra da praÃ§a chegou antes do sol ir embora. | NO |
| `npc_yael_dialogue_07` | ShopOrService | Posso abrir loja noturna e vender raridades quando o horÃ¡rio estiver ligado. | YES |
| `npc_yael_dialogue_08` | QuestFutureHook | Depois da meia-noite, algumas portas lembram que existem. | NO |
| `npc_yael_dialogue_09` | RepeatFallback | PreÃ§o baixo demais Ã© convite, nÃ£o oferta. | YES |
| `npc_yael_dialogue_10` | Goodbye | EsqueÃ§a meu rosto atÃ© precisar dele. | YES |

### 6.12 `npc_thalindra` â€” Thalindra VÃ©u-de-Lua

**Objetivo:** Arquivo, lore, Cindar, Anya, Elyndor/BromÃ©cia.
**Role:** Pesquisador/Alquimista.
**Zona:** Archive.
**Movimento:** Stationary/ArchiveDesk.
**ServiÃ§o/Shop:** `archive_lore_quests_blueprints`.
**Quest seeds:** Poeira no Arquivo, A Palavra Nymiriana, O Mapa Que NÃ£o Deveria Existir.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_thalindra_dialogue_01` | Greeting | Todo registro perdido deixa uma cicatriz. | YES |
| `npc_thalindra_dialogue_02` | Role | Thalindra VÃ©u-de-Lua. Pesquiso o que a cidade editou. | YES |
| `npc_thalindra_dialogue_03` | Location | Cindar's Hope foi construÃ­da sobre versÃµes incompletas. | YES |
| `npc_thalindra_dialogue_04` | GameplayTip | Leia inscriÃ§Ãµes antes de vender pedras estranhas. | YES |
| `npc_thalindra_dialogue_05` | RumorNonSpoiler | A palavra nymiriana perto da Fonte nÃ£o deveria estar apagada. | NO |
| `npc_thalindra_dialogue_06` | Contextual | Hoje encontrei um mapa que nega a prÃ³pria margem. | NO |
| `npc_thalindra_dialogue_07` | ShopOrService | Posso traduzir registros, entregar pistas e blueprints raros quando o sistema existir. | YES |
| `npc_thalindra_dialogue_08` | QuestFutureHook | Poeira no arquivo esconde mais que abandono. | NO |
| `npc_thalindra_dialogue_09` | RepeatFallback | NÃ£o confunda silÃªncio com ausÃªncia de prova. | YES |
| `npc_thalindra_dialogue_10` | Goodbye | Volte se encontrar sÃ­mbolo de Ã¡gua, cinza ou portal. | YES |

### 6.13 `npc_dagna` â€” Dagna Rocha-Morna

**Objetivo:** MineraÃ§Ã£o, pedreira, cave ore.
**Role:** Minerador/Combatente.
**Zona:** Quarry/MineOffice.
**Movimento:** Patrol/QuarryRoad.
**ServiÃ§o/Shop:** `ore_mining_cave`.
**Quest seeds:** Veio de Cobre, Galeria Sem Nome, A Rocha Quente Demais.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_dagna_dialogue_01` | Greeting | Pedra viva soa diferente. | YES |
| `npc_dagna_dialogue_02` | Role | Dagna Rocha-Morna. Eu escuto minÃ©rio e desconfio de pedra quente. | YES |
| `npc_dagna_dialogue_03` | Location | A cidade pisa em camadas que nÃ£o entende. | YES |
| `npc_dagna_dialogue_04` | GameplayTip | NÃ£o venda minÃ©rio sem saber de onde ele veio. | YES |
| `npc_dagna_dialogue_05` | RumorNonSpoiler | Uma galeria sem nome ainda leva o nome de alguÃ©m. | NO |
| `npc_dagna_dialogue_06` | Contextual | Hoje as rochas perto da estrada estÃ£o quentes demais. | NO |
| `npc_dagna_dialogue_07` | ShopOrService | Posso avaliar minÃ©rio, ensinar mineraÃ§Ã£o e abrir pedreira quando o sistema existir. | YES |
| `npc_dagna_dialogue_08` | QuestFutureHook | Veio de cobre Ã© liÃ§Ã£o, nÃ£o fortuna. | NO |
| `npc_dagna_dialogue_09` | RepeatFallback | Se a pedra mente, eu bato atÃ© ela admitir. | YES |
| `npc_dagna_dialogue_10` | Goodbye | Traga amostra, nÃ£o histÃ³ria bonita. | YES |

### 6.14 `npc_pip` â€” Pip Semente-Solta

**Objetivo:** Tutorial, recados, entrega e humor.
**Role:** Comerciante/Explorador.
**Zona:** TownEntrance/Market.
**Movimento:** WanderWithinZone.
**ServiÃ§o/Shop:** `tutorial_delivery`.
**Quest seeds:** Primeira Entrega, Vi o Fantasma, A Carta Errada.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_pip_dialogue_01` | Greeting | Ei! VocÃª Ã© mais alto de perto. | YES |
| `npc_pip_dialogue_02` | Role | Sou Pip Semente-Solta. Entrego coisas, recados e Ã s vezes problemas. | YES |
| `npc_pip_dialogue_03` | Location | A cidade parece grande atÃ© vocÃª correr por todos os becos. | YES |
| `npc_pip_dialogue_04` | GameplayTip | Primeiro aprenda onde comprar sementes; depois finja que sempre soube. | YES |
| `npc_pip_dialogue_05` | RumorNonSpoiler | Eu vi algo no Jardim das EstÃ¡tuas. NÃ£o ri. | NO |
| `npc_pip_dialogue_06` | Contextual | Hoje entreguei uma carta certa para a pessoa quase certa. | NO |
| `npc_pip_dialogue_07` | ShopOrService | Posso guiar tutoriais, entregas e rotas de loja quando o sistema estiver pronto. | YES |
| `npc_pip_dialogue_08` | QuestFutureHook | Primeira Entrega: venha, a loja de sementes nÃ£o morde. | NO |
| `npc_pip_dialogue_09` | RepeatFallback | Se alguÃ©m perguntar, eu nÃ£o estava correndo. | YES |
| `npc_pip_dialogue_10` | Goodbye | AtÃ© jÃ¡! Provavelmente antes do que vocÃª espera. | YES |

### 6.15 `npc_alaric` â€” Ser Alaric Veyr

**Objetivo:** Guarda, patrulha, combate e seguranÃ§a.
**Role:** GuardiÃ£o/Combatente.
**Zona:** GuardPost.
**Movimento:** Patrol/TownGate.
**ServiÃ§o/Shop:** `guard_combat_training`.
**Quest seeds:** Patrulha da Estrada Baixa, RelatÃ³rio Incompleto, A JustiÃ§a NÃ£o Basta.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_alaric_dialogue_01` | Greeting | Postura reta. Olhos abertos. | YES |
| `npc_alaric_dialogue_02` | Role | Ser Alaric Veyr. A guarda mantÃ©m a estrada respirando. | YES |
| `npc_alaric_dialogue_03` | Location | SeguranÃ§a nÃ£o Ã© ausÃªncia de perigo; Ã© prontidÃ£o. | YES |
| `npc_alaric_dialogue_04` | GameplayTip | NÃ£o vÃ¡ Ã  caverna sem ferramenta, comida e retorno planejado. | YES |
| `npc_alaric_dialogue_05` | RumorNonSpoiler | Alguns relatÃ³rios somem porque verdade demais causa pÃ¢nico. | NO |
| `npc_alaric_dialogue_06` | Contextual | Hoje a patrulha voltou com lama que nÃ£o Ã© desta estrada. | NO |
| `npc_alaric_dialogue_07` | ShopOrService | Posso treinar defesa, patrulhas e contratos de guarda quando o sistema existir. | YES |
| `npc_alaric_dialogue_08` | QuestFutureHook | Patrulha da Estrada Baixa comeÃ§a com disciplina. | NO |
| `npc_alaric_dialogue_09` | RepeatFallback | Coragem sem dever Ã© vaidade armada. | YES |
| `npc_alaric_dialogue_10` | Goodbye | Siga. | YES |

### 6.16 `npc_mirela` â€” Mirela dos LaÃ§os

**Objetivo:** Bolsas, roupas, acessÃ³rios.
**Role:** ArtesÃ£o/Comerciante.
**Zona:** Tailor.
**Movimento:** ShopKeeperFixed.
**ServiÃ§o/Shop:** `tailor_bags_clothing`.
**Quest seeds:** Bolsa de Trabalho, Fio que NÃ£o Rasga, Roupa de Festival.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_mirela_dialogue_01` | Greeting | Bolso mal feito derruba mais item que monstro. | YES |
| `npc_mirela_dialogue_02` | Role | Mirela dos LaÃ§os. Costuro utilidade com aparÃªncia decente. | YES |
| `npc_mirela_dialogue_03` | Location | Roupa na cidade Ã© funÃ§Ã£o, status e armadura social. | YES |
| `npc_mirela_dialogue_04` | GameplayTip | Uma bolsa boa muda o quanto vocÃª aguenta trabalhar. | YES |
| `npc_mirela_dialogue_05` | RumorNonSpoiler | Vi tecido que se recompÃµe sozinho. Quero entender antes de vender. | NO |
| `npc_mirela_dialogue_06` | Contextual | Hoje Nimble mediu minha porta. De novo. | NO |
| `npc_mirela_dialogue_07` | ShopOrService | Posso vender bolsas, roupas e acessÃ³rios quando o sistema estiver pronto. | YES |
| `npc_mirela_dialogue_08` | QuestFutureHook | Bolsa de Trabalho comeÃ§a com material resistente. | NO |
| `npc_mirela_dialogue_09` | RepeatFallback | NÃ£o, remendo torto nÃ£o tem personalidade. | YES |
| `npc_mirela_dialogue_10` | Goodbye | Volte com tecido, couro ou necessidade. | YES |

### 6.17 `npc_renko` â€” Renko TrÃªs-Sorrisos

**Objetivo:** Loja geral, barganha e itens comuns.
**Role:** Comerciante/ArtesÃ£o.
**Zona:** GeneralStore.
**Movimento:** ShopKeeperFixed.
**ServiÃ§o/Shop:** `general_store_bargain`.
**Quest seeds:** PreÃ§o de Amigo, Mercadoria Sem Dono, TrÃªs Sorrisos, Uma Mentira.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_renko_dialogue_01` | Greeting | Tenho preÃ§o de amigo, conhecido e alvo fÃ¡cil. Escolha com cuidado. | YES |
| `npc_renko_dialogue_02` | Role | Renko TrÃªs-Sorrisos. Compro, vendo e negoÃ§o a verdade do preÃ§o. | YES |
| `npc_renko_dialogue_03` | Location | Mercado Ã© onde a cidade confessa o que precisa. | YES |
| `npc_renko_dialogue_04` | GameplayTip | Compare preÃ§o antes de vender tudo no primeiro balcÃ£o. | YES |
| `npc_renko_dialogue_05` | RumorNonSpoiler | Mercadoria sem dono quase sempre tem dono perigoso. | NO |
| `npc_renko_dialogue_06` | Contextual | Hoje sorri duas vezes por lucro e uma por prevenÃ§Ã£o. | NO |
| `npc_renko_dialogue_07` | ShopOrService | Posso tocar loja geral, barganha e estoque comum quando o sistema existir. | YES |
| `npc_renko_dialogue_08` | QuestFutureHook | PreÃ§o de Amigo Ã© uma liÃ§Ã£o, nÃ£o uma promessa. | NO |
| `npc_renko_dialogue_09` | RepeatFallback | Se parece inÃºtil, ainda nÃ£o achei comprador. | YES |
| `npc_renko_dialogue_10` | Goodbye | Volte com moedas ou curiosidade. | YES |

### 6.18 `npc_eiran` â€” Eiran Valeclaro

**Objetivo:** Animais, pets e raÃ§Ã£o.
**Role:** Tratador/Plantador.
**Zona:** AnimalYard.
**Movimento:** WanderWithinZone/AnimalArea.
**ServiÃ§o/Shop:** `animals_pets_feed`.
**Quest seeds:** Primeira Tigela, Animal Assustado, Cuidado NÃ£o Ã© Fraqueza.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_eiran_dialogue_01` | Greeting | Fale baixo. Eles escutam intenÃ§Ã£o. | YES |
| `npc_eiran_dialogue_02` | Role | Eiran Valeclaro. Cuido de animais e de gente que aprende devagar. | YES |
| `npc_eiran_dialogue_03` | Location | A cidade sÃ³ parece humana porque os bichos toleram. | YES |
| `npc_eiran_dialogue_04` | GameplayTip | Alimente antes de pedir. Vale para quase tudo. | YES |
| `npc_eiran_dialogue_05` | RumorNonSpoiler | Animais evitaram a trilha leste. Isso costuma significar algo. | NO |
| `npc_eiran_dialogue_06` | Contextual | Hoje os pÃ¡ssaros calaram quando a pedra esquentou. | NO |
| `npc_eiran_dialogue_07` | ShopOrService | Posso desbloquear pets, raÃ§Ã£o e cuidados quando o sistema existir. | YES |
| `npc_eiran_dialogue_08` | QuestFutureHook | Primeira Tigela ensina mais que dez sermÃµes. | NO |
| `npc_eiran_dialogue_09` | RepeatFallback | NÃ£o toque se ele se afastou primeiro. | YES |
| `npc_eiran_dialogue_10` | Goodbye | Volte sem pressa. | YES |

### 6.19 `npc_liora` â€” Liora Canta-Rio

**Objetivo:** MÃºsica, sonhos, Alihana e pistas de Anya.
**Role:** MÃºsico/Pesquisador.
**Zona:** Tavern/StatueGarden.
**Movimento:** WanderWithinZone/EveningStage.
**ServiÃ§o/Shop:** `music_dream_lore`.
**Quest seeds:** CanÃ§Ã£o sem Autor, A EstÃ¡tua que Escutou, Sonho de Ãgua Clara.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_liora_dialogue_01` | Greeting | Algumas canÃ§Ãµes chegam antes de quem canta. | YES |
| `npc_liora_dialogue_02` | Role | Liora Canta-Rio. Eu lembro melodias que nunca aprendi. | YES |
| `npc_liora_dialogue_03` | Location | A cidade tem silÃªncio afinado demais em certos lugares. | YES |
| `npc_liora_dialogue_04` | GameplayTip | OuÃ§a sons estranhos perto de estÃ¡tuas, Ã¡gua e sonho. | YES |
| `npc_liora_dialogue_05` | RumorNonSpoiler | Uma mÃºsica respondeu ao Jardim das EstÃ¡tuas. NÃ£o foi eco. | NO |
| `npc_liora_dialogue_06` | Contextual | Hoje sonhei com Ã¡gua clara onde o mapa mostra pedra. | NO |
| `npc_liora_dialogue_07` | ShopOrService | Posso oferecer mÃºsica, buffs sociais e pistas quando o sistema existir. | YES |
| `npc_liora_dialogue_08` | QuestFutureHook | CanÃ§Ã£o sem Autor comeÃ§a quando vocÃª para de chamar de coincidÃªncia. | NO |
| `npc_liora_dialogue_09` | RepeatFallback | Se a melodia repetir, anote. | YES |
| `npc_liora_dialogue_10` | Goodbye | Que seu sono nÃ£o minta. | YES |

### 6.20 `npc_orlan` â€” Orlan Pouso-Curto

**Objetivo:** Hospedagem, viajantes e notÃ­cias.
**Role:** Comerciante/Escriba.
**Zona:** Inn.
**Movimento:** ShopKeeperFixed.
**ServiÃ§o/Shop:** `inn_lodging_news`.
**Quest seeds:** Quarto de Viajante, HÃ³spede Sem Sombra, Conta Aberta.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_orlan_dialogue_01` | Greeting | Quem dorme aqui deixa nome, moeda e versÃ£o. | YES |
| `npc_orlan_dialogue_02` | Role | Orlan Pouso-Curto. Cuido da pousada e do que os viajantes esquecem. | YES |
| `npc_orlan_dialogue_03` | Location | Toda cidade muda quando chega caravana. | YES |
| `npc_orlan_dialogue_04` | GameplayTip | NotÃ­cia boa tambÃ©m precisa de fonte. | YES |
| `npc_orlan_dialogue_05` | RumorNonSpoiler | Um hÃ³spede sem sombra pagou adiantado. PÃ©ssimo sinal. | NO |
| `npc_orlan_dialogue_06` | Contextual | Hoje trÃªs viajantes juraram nÃ£o se conhecer. Mentiram no mesmo tom. | NO |
| `npc_orlan_dialogue_07` | ShopOrService | Posso oferecer hospedagem, notÃ­cias e rumores de rota quando o sistema existir. | YES |
| `npc_orlan_dialogue_08` | QuestFutureHook | Quarto de Viajante abre mais portas que espada. | NO |
| `npc_orlan_dialogue_09` | RepeatFallback | Se Gruta perguntou, eu nÃ£o contei. Ainda. | YES |
| `npc_orlan_dialogue_10` | Goodbye | Volte antes da Ãºltima chave. | YES |

### 6.21 `npc_savra` â€” Savra Escama-Verde

**Objetivo:** Ervas, antÃ­dotos, venenos e floresta.
**Role:** Curandeiro/Explorador.
**Zona:** Herbalist/ForestGate.
**Movimento:** Patrol/HerbRoute.
**ServiÃ§o/Shop:** `herbs_antidote_forest`.
**Quest seeds:** AntÃ­doto Amargo, Praga que Anda, O Fungo de Baixo.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_savra_dialogue_01` | Greeting | Verde nÃ£o significa veneno. Geralmente. | YES |
| `npc_savra_dialogue_02` | Role | Savra Escama-Verde. ConheÃ§o ervas que curam e ervas que negociam. | YES |
| `npc_savra_dialogue_03` | Location | A borda da cidade tem mais resposta que o centro. | YES |
| `npc_savra_dialogue_04` | GameplayTip | AntÃ­doto ruim atrasa morte; antÃ­doto bom evita histÃ³ria. | YES |
| `npc_savra_dialogue_05` | RumorNonSpoiler | Fungos de baixo nÃ£o seguem lÃ³gica de superfÃ­cie. | NO |
| `npc_savra_dialogue_06` | Contextual | Hoje as folhas perto da estrada tinham mordidas sem dentes. | NO |
| `npc_savra_dialogue_07` | ShopOrService | Posso vender ervas, antÃ­dotos e reagentes quando o sistema existir. | YES |
| `npc_savra_dialogue_08` | QuestFutureHook | AntÃ­doto Amargo comeÃ§a com coleta cuidadosa. | NO |
| `npc_savra_dialogue_09` | RepeatFallback | NÃ£o coma nada que brilhe por orgulho. | YES |
| `npc_savra_dialogue_10` | Goodbye | Volte se a ferida mudar de cor. | YES |

### 6.22 `npc_tovin` â€” Tovin MÃ£os-de-Selo

**Objetivo:** Registros, impostos, altares e permissÃµes.
**Role:** Escriba/ArtesÃ£o.
**Zona:** Registry.
**Movimento:** Stationary/PermitDesk.
**ServiÃ§o/Shop:** `permits_altars_registry`.
**Quest seeds:** Carimbo de Propriedade, Altar Permitido, Selo Sem Reino.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_tovin_dialogue_01` | Greeting | Sem selo, Ã© intenÃ§Ã£o. Com selo, Ã© problema rastreÃ¡vel. | YES |
| `npc_tovin_dialogue_02` | Role | Tovin MÃ£os-de-Selo. Transformo caos em formulÃ¡rio. | YES |
| `npc_tovin_dialogue_03` | Location | A cidade funciona porque alguÃ©m carimba o inevitÃ¡vel. | YES |
| `npc_tovin_dialogue_04` | GameplayTip | Antes de erguer altar, entenda o que estÃ¡ autorizando. | YES |
| `npc_tovin_dialogue_05` | RumorNonSpoiler | Um selo antigo sem reino nÃ£o deveria abrir nenhuma porta. | NO |
| `npc_tovin_dialogue_06` | Contextual | Hoje encontrei trÃªs erros e dois cidadÃ£os; proporÃ§Ã£o comum. | NO |
| `npc_tovin_dialogue_07` | ShopOrService | Posso registrar altares, permissÃµes e expansÃ£o quando o sistema existir. | YES |
| `npc_tovin_dialogue_08` | QuestFutureHook | Carimbo de Propriedade evita disputa futura. | NO |
| `npc_tovin_dialogue_09` | RepeatFallback | Se o formulÃ¡rio Ã© pequeno, desconfie. | YES |
| `npc_tovin_dialogue_10` | Goodbye | Volte com assinatura legÃ­vel. | YES |

### 6.23 `npc_maelor` â€” Maelor Cinza

**Objetivo:** Nyx, memÃ³ria, ruÃ­nas e segredos tardios.
**Role:** Explorador/Pesquisador.
**Zona:** StatueGarden/NightRoute.
**Movimento:** NightOnly/WanderHidden.
**ServiÃ§o/Shop:** `late_lore_memory`.
**Quest seeds:** Passos Onde NÃ£o HÃ¡ Luz, MemÃ³ria Que Escolheu Sumir, O SilÃªncio TambÃ©m Protege.

| EntryId | Type | Text | Final? |
|---|---|---|---:|
| `npc_maelor_dialogue_01` | Greeting | VocÃª olha como quem ainda lembra de esquecer. | YES |
| `npc_maelor_dialogue_02` | Role | Maelor Cinza. Alguns nomes sobrevivem melhor no escuro. | YES |
| `npc_maelor_dialogue_03` | Location | A cidade esqueceu algo de propÃ³sito. | YES |
| `npc_maelor_dialogue_04` | GameplayTip | Nem todo segredo precisa ser aberto no primeiro dia. | YES |
| `npc_maelor_dialogue_05` | RumorNonSpoiler | HÃ¡ passos onde nÃ£o hÃ¡ luz; siga sÃ³ quando souber voltar. | NO |
| `npc_maelor_dialogue_06` | Contextual | Hoje a noite repetiu uma memÃ³ria que nÃ£o era minha. | NO |
| `npc_maelor_dialogue_07` | ShopOrService | Posso abrir investigaÃ§Ãµes tardias de Nyx, memÃ³ria e ruÃ­nas quando o sistema permitir. | YES |
| `npc_maelor_dialogue_08` | QuestFutureHook | MemÃ³ria Que Escolheu Sumir nÃ£o Ã© quest para curioso apressado. | NO |
| `npc_maelor_dialogue_09` | RepeatFallback | Se me viu de dia, era outra pessoa. Ou mentira. | YES |
| `npc_maelor_dialogue_10` | Goodbye | Guarde silÃªncio atÃ© ele servir. | YES |

## 7. Shop/service mapping obrigatÃ³rio

| NpcId | ServiceType | ShopId sugerido | Buy | Sell | ObservaÃ§Ã£o |
|---|---|---|---:|---:|---|
| `npc_corvus` | healing_blessing_temple | `shop_corvus` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_mara` | licenses_contracts | `shop_mara` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_sylveth` | seeds_crops_herbs | `shop_sylveth` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_brumdar` | blacksmith_repair_upgrade | `shop_brumdar` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_nimble` | construction_buildings_move | `shop_nimble` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_gurd` | heavy_clearance | `shop_gurd` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_hund` | guard_transport | `shop_hund` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_ozzra` | alchemy_potions_fertilizer | `shop_ozzra` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_gruta` | tavern_food_rumors | `shop_gruta` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_zrix` | maps_cave_contracts | `shop_zrix` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_yael` | night_shop_rare_items | `shop_yael` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_thalindra` | archive_lore_quests_blueprints | `shop_thalindra` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_dagna` | ore_mining_cave | `shop_dagna` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_pip` | tutorial_delivery | `shop_pip` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_alaric` | guard_combat_training | `` | 0 | 0 | Sem shop inicial obrigatÃ³rio. |
| `npc_mirela` | tailor_bags_clothing | `shop_mirela` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_renko` | general_store_bargain | `shop_renko` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_eiran` | animals_pets_feed | `shop_eiran` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_liora` | music_dream_lore | `` | 0 | 0 | Sem shop inicial obrigatÃ³rio. |
| `npc_orlan` | inn_lodging_news | `shop_orlan` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_savra` | herbs_antidote_forest | `shop_savra` | 1 | 0 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_tovin` | permits_altars_registry | `shop_tovin` | 1 | 1 | ServiÃ§o completo pode ser debt, mas NPC deve existir e abrir diÃ¡logo. |
| `npc_maelor` | late_lore_memory | `` | 0 | 0 | Sem shop inicial obrigatÃ³rio. |

## 8. Movement schedules obrigatÃ³rios

| NpcId | MovementProfile | Implementar agora | Debt permitido |
|---|---|---:|---|
| `npc_corvus` | Stationary/TemplePatrol | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_mara` | Stationary/RegistryDesk | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_sylveth` | ShopKeeperFixed/FarmVisit | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_brumdar` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_nimble` | Patrol/WorkshopDesk | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_gurd` | Patrol/HeavyWorkZone | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_hund` | Patrol/TownRoad | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_ozzra` | WanderWithinZone/Lab | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_gruta` | ShopKeeperFixed/TavernStage | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_zrix` | Patrol/CaveRoad | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_yael` | NightOnly/WanderHidden | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_thalindra` | Stationary/ArchiveDesk | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_dagna` | Patrol/QuarryRoad | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_pip` | WanderWithinZone | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_alaric` | Patrol/TownGate | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_mirela` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_renko` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_eiran` | WanderWithinZone/AnimalArea | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_liora` | WanderWithinZone/EveningStage | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_orlan` | ShopKeeperFixed | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_savra` | Patrol/HerbRoute | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_tovin` | Stationary/PermitDesk | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |
| `npc_maelor` | NightOnly/WanderHidden | YES | DAILY_SCHEDULE_DEFERRED se horÃ¡rios finais nÃ£o existirem |

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

CÃ³digo/data se necessÃ¡rio:

```text
Assets/_Game/Data/NPCs/*.asset ou equivalente
Assets/_Game/Data/Dialogue/*.asset ou equivalente
Assets/_Game/Data/Shops/*.asset ou equivalente
Assets/_Game/Scripts/NPC/Runtime/*, somente genÃ©rico
Assets/_Game/Scripts/Editor/Validation/ValidateRefinedCanonicalNpcTownPopulation.cs
Assets/_Game/Scenes/TownScene.unity
```

## 10. Prompt de execuÃ§Ã£o para Codex/Claude Code

```text
Estamos no repo rafa210587/cindars_hope, branch dev.

Objetivo:
Executar WAVE_INTEGRATION_12C_REFINED_CANONICAL_NPC_TOWN_POPULATION usando esta spec como fonte direta. Inserir todos os 23 NPCs canÃ´nicos refinados na TownScene com NpcId, DisplayName, funÃ§Ã£o, propÃ³sito, diÃ¡logo com >=10 entries, movement profile e shop/service quando aplicÃ¡vel.

Leia primeiro:
- docs/specs/a_implementar/WAVE_INTEGRATION_12C_refined_canonical_npc_town_population.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md
- docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md
- docs/project/CURRENT_STATE.md

NÃƒO executar WAVE13/14/15.
NÃƒO criar uma classe por NPC.
NÃƒO criar sistema social/romance/reputation final.
NÃƒO criar quests finais; somente seed metadata/dialogue hooks.
NÃƒO duplicar NPC existente; migrar/atualizar se jÃ¡ existir.
NÃƒO deixar os 19 NPCs canÃ´nicos como FUTURE_SCOPE nesta spec.
NÃƒO alterar Packages/ProjectSettings.

Passos:
1. Preflight: git fetch origin dev; garantir branch dev e working tree limpa.
2. Auditar TownScene e assets atuais.
3. Criar/atualizar data assets/definitions para todos os 23 NPCs listados na spec.
4. Criar/atualizar DialogueSets com >=10 entries por NPC usando as frases desta spec.
5. Criar/atualizar Shop/Service definitions para NPCs comerciais/serviÃ§o.
6. Posicionar todos os 23 NPCs na TownScene, em zonas funcionais, sem sobrepor e alcanÃ§Ã¡veis pelo player.
7. Garantir NpcSceneInteractable + DialogueBridge para todos.
8. Garantir ShopBridge para NPCs comerciais quando shop runtime existir; se nÃ£o, debt explÃ­cito.
9. Garantir movement profiles; Patrol/Wander/NightRoute devem mover ou ter debt claro se runtime bloquear.
10. Criar validator ValidateRefinedCanonicalNpcTownPopulation.cs.
11. Atualizar docs de validaÃ§Ã£o e CURRENT_STATE.
12. Rodar dotnet build Assembly-CSharp e Assembly-CSharp-Editor.
13. Rodar docs validation e quality check.
14. Commitar apenas arquivos da WAVE12C.

CritÃ©rios de sucesso:
- 23 NPCs no roster 12C.
- 23 NPCs na TownScene ou human wiring explÃ­cito por NPC, mas preferir scene wired.
- 23 NPCs com DialogueSet >=10 entries.
- todos com Purpose e Responsibilities.
- todos com MovementProfile.
- merchants/services com ShopId/ServiceType.
- builds passam.

Commit:
git commit -m "feat: populate town with full refined canonical npc roster"
git push origin dev

Resposta final obrigatÃ³ria:
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

SÃ³ usar `BUILD_VALIDATED_SCENE_WIRED` se os 23 NPCs estiverem posicionados, interagÃ­veis e com diÃ¡logo abrindo em Play Mode.
