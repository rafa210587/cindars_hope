---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Refinamento — casas habitáveis e interiores híbridos da Town

**ID:** ref_town_interiors_hybrid_v1  
**Data/revisão:** 2026-09-12 / 1.0  
**Status:** APROVADO PARA EXECUÇÃO pelo humano em 2026-09-12; evidência de implementação pendente  
**Escopo:** 24 lotes, 28 NPCs canônicos, comércio, transição e moradia.  
**Pacote:** [índice das quatro specs](../../../.specs/a_implementar/town_interiors_hybrid_v1/README.md).

## Problema observado

A guarita corrigida tem 6×4u úteis e comporta três móveis e passagem, mas não uma moradia completa. Testes físicos não comprovam habitabilidade. O usuário escolheu desenvolver a proposta híbrida: interiores separados para moradias/prédios maiores e continuidade visual em estruturas abertas pequenas. Referência: [correção e limites](../../validation/town_room_repair_20260912.md).

## Phase 0 real — fatos, não intenção

Leitura em 2026-09-12, sem Unity e sem alterar Assets:

- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs`: 24 chamadas `Lot("House_…")`, 28 entradas `Npc(…)`; 23 bancas NPC e 6 bancas de mercado declaradas como baseline. Coordenadas dos lotes são fonte atual; comentários antigos têm medidas divergentes e não são usados como geometria.
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`, `TownNpcHomes`: 23 atribuições Indoor em 21 endereços + 5 Outdoor. Há dois pares compartilhados (Orlan/Gruta, Gurd/Hund). Alaric mora em GateKeeper; Eiran em AnimalYard. Não expulsá-los para manter um desenho compacto.
- O mesmo generator liga 23 NPCs a `Shop_*.asset`; `TownShopDoorHoursWiring.cs` aplica gate em apenas nove prédios. Portanto classificação da porta não é censo de comerciantes.
- `NpcServiceCatalog.cs` já possui oito serviços pessoais; banho é de Gruta e prato do dia de Orlan. `CityServiceCatalog.cs` possui ainda dois serviços cívicos distintos, licença de Tovin e registro de Mara. Preservar ambos os catálogos/IDs/efeitos; não são duas implementações do mesmo serviço.
- Seis estações já existem: Forge, Alchemy, CookingStation, Carpentry, Sewing e Workbench do MarketHall. Padaria/curtume/moinho não autorizam criar novos sistemas de produção.
- `SceneTransitionRouter.Execute(SceneTransitionRequest)` publica started e carrega Single imediatamente. `SceneFadeOverlayController` reage ao evento, não garante tela preta antes do load. Não tratar esse código como uma transição orgânica já pronta.
- `SceneTransitionState` mantém apenas PendingSpawnId. Não tem identidade de casa, retorno por endereço ou restauração de interior.
- `NpcScheduleService` usa anchors por NPC e grafo espacial único; `NpcManagerSaveData` guarda SceneId/posição/HasMet, mas não espaço lógico da casa. Comentários de teleporte por faixa Y são legados, não contrato a reutilizar.
- `NpcsSectionProvider` captura somente quando a cena ativa é TownScene. `SaveManager.SaveGame/LoadGame` trabalha com cena ativa e posição global. Isso favorece manter Town ativa e acrescentar localização lógica, sem gerar um bootstrap por casa.

Comandos reproduzíveis: `rg -n 'Lot\("House_|Npc\("npc_' Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs`; `rg -n 'HomeAssignment\.(Indoor|Outdoor)|new\("npc_.*Shop_' Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`; leitura dos métodos acima. Reconfirmar antes da execução.

## Decisões propostas

| ID | Decisão e motivo |
|---|---|
| D01 | Preservar 24 lotes/IDs e os 28 moradores canônicos; não atribuir casas aos cinco Outdoor. Ownership significa residência/responsabilidade operacional, não novo sistema de propriedade legal. |
| D02 | 22 fachadas passam a portal de interior; GateKeeper mantém posto externo com cutaway e porta para aposento; AnimalYard mantém curral externo e porta para casa de apoio. Resultado: 24 destinos interiores, sem cobrir o curral com telhado. |
| D03 | Uma cena aditiva reutilizável `TownInteriorScene` + 24 definições/prefabs de conteúdo, uma instância carregada por vez. Town continua carregada/ativa, com relógio, estoque, quests e NPCs únicos. Não são 24 cópias de managers. |
| D04 | Medidas internas partem das atividades e moradores; fachada não determina a área útil 1:1. Manter categoria arquitetônica e localização da porta. Não reduzir Player, NPCs ou móveis para caber. |
| D05 | Comércio misto: lojas/oficinas com balcão interno; sementes, pesca, animais, itinerantes e mercado noturno continuam na rua. Um NPC/ShopId/estoque/serviço, nunca duas lojas independentes dentro e fora. |
| D06 | Fluxo: interação → porta abre → cruzamento físico → fade out → preparo/posicionamento → fade in. Sair é sempre possível, inclusive após fechamento do comércio. Não carregar cena só por abrir a porta. |
| D07 | Fonte de disponibilidade continua sendo agenda/serviços existentes. Horário de porta, presença do atendente e permissão de transação são verificações distintas. Pousada tem recepção acessível 24h, mas nenhum atendimento mágico durante sono. |
| D08 | Salvar localização lógica e posição local, incluindo NPCs fora de espaços carregados. Legacy recebe Town exterior seguro; nunca restaurar coordenada do interior como posição na praça. |
| D09 | Móveis pessoais não viram loot, contêiner gratuito ou cama usável por padrão. Apenas cama paga da pousada mantém descanso do jogador; receitas/serviços/estoques existentes não são rebalanceados. |
| D10 | Pilotar uma residência/ofício (Renko), casa compartilhada (CarvalhoTorto) e exceção híbrida (GateKeeper) antes de materializar o restante. |

## Alternativas avaliadas

1. Só aumentar fachadas: preserva entrada contínua, mas consome área urbana e exige refazer ruas/NPCs; não resolve casas com oficina, quartos e armazenamento.
2. Todas as casas em cenas Single únicas: identidade clara, mas multiplica wiring e força preservação/reconstrução de mundo a cada compra. Rejeitado para este lote.
3. Todos os interiores escondidos na própria Town: simples inicialmente, mas mistura coordenadas e navegação, carrega tudo e já existe dívida de suposições por Y. Rejeitado como identidade de espaço.
4. Cena aditiva de conteúdo com identidade explícita: proposta escolhida, com risco de save/NPC tratado nas specs 02/03. Uma origem física reservada apenas separa render/colisão; jamais identifica a casa.

## Alteração de regra / compatibilidade

A spec ativa `spec_town_components_doors_and_collision_v1.md` e o amendment operacional de `docs/game_rules/city_rules.md` exigem23 walk-ins +AnimalYard exterior; teleport exige amendment. Este refinamento **não revoga** essas regras sozinho. Ao aprovar, atualizar ambas as fontes: forma de entrada, anexos residenciais e exceção de circulação pública24h da Inn, sem atendimento mágico. Loja-moradia fechada continua bloqueada para visita Player; entrada de residente/saída são independentes. Preservar colisão, porta animada, identidade dos NPCs e evidência visual.

## Riscos e aprovação

As 24 fichas, áreas-alvo, novos ambientes privados e políticas de visita foram aprovados para execução em 2026-09-12; não são runtime já implementado. Não há referência pixel art aprovada de cada interior. A revisão independente não foi obtida na autoria por limite de agentes; foi retomada na execução. O pacote agora está em `a_implementar`, sem dispensar revisão de contratos e validação de dados antes de S02.

## Handoff

S01 catálogo/ownership → S02 transição/save → S03 NPCs/comércio → S04 materialização/arte/aceite. Decisões D01–D10 e fichas aprovadas; resultados de implementação permanecem pendentes. Resolver revisão independente antes de habilitar migração em produção.
