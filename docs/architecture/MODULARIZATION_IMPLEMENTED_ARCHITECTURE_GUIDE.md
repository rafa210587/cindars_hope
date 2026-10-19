# Guia da arquitetura modular implementada

Data de referência: 2026-07-05. Este é o mapa operacional para Codex e Claude Code. Validar sempre
contra o checkout antes de assumir que os caminhos ou números continuam atuais.

## 1. Fronteiras de compilação

| Assembly | Caminho | Responsabilidade |
|---|---|---|
| `CindarsHope.Foundation` | `Assets/_Game/Scripts/Foundation/` | contratos BCL puros; `noEngineReferences` |
| `CindarsHope.Gameplay` | `Assets/_Game/Scripts/Gameplay/` | decisões e estados puros de gameplay; `noEngineReferences` |
| `CindarsHope.Runtime` | `Assets/_Game/Scripts/` | gameplay e fachadas Unity ainda em assembly ampla |
| `CindarsHope.Editor` | `Assets/_Game/Scripts/Editor/` | geradores, validators e menus editor-only |
| `CindarsHope.Tests.EditMode` | `Assets/_Game/Tests/EditMode/` | contratos puros, integração e regressão |
| `CindarsHope.Tests.PlayMode.Composition` | `Assets/_Game/Tests/PlayMode/Composition/` | smoke de cenas e composition root |

A assembly Runtime ampla é deliberada: quebrá-la por domínio antes de remover dependências cíclicas
criaria referências artificiais ou duplicação. Novas assemblies só entram quando a direção de
dependência estiver demonstrada por ports.

## 2. Composition root e lifecycle

- `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs` é o ponto central novo.
- `Assets/_Game/Scripts/Combat/CombatStateTrackerBootstrap.cs` expõe instalação idempotente.
- Novo serviço não deve adicionar `RuntimeInitializeOnLoadMethod` isolado sem justificar ordem,
  ownership, teardown e teste PlayMode.
- Fachadas serializadas antigas permanecem durante migração pelo Strangler Pattern.

## 3. Ports, adapters e transações

- Foundation: `IIdentifiedData`, `IDataRegistry<T>` e ports transacionais.
- Gameplay: decisões de atalhos, sessão de loja/NPC, política da Thalindra e o enum simples de modo
  de interação com quest, além do catálogo read-only action-skill→effect. Runtime depende de
  Gameplay; Gameplay não referencia Runtime ou Unity.
- IDs narrativos persistidos vivem em `Gameplay/NarrativeIds.cs`; adapters Narrative, Quest e World
  consomem o mesmo contrato sem dependência reversa entre Narrative e Quests.
- Bloqueio de input entre domínios usa `GameplayInputBlocker` com leases; owners não devem consultar
  tipos concretos de outro domínio apenas para bloquear movimento.
- Inventário e ouro: `InventoryManager`/`PlayerManager` implementam adapters, sem duplicar estado.
- Compra atômica: `AtomicPurchaseTransaction` valida e compensa falha de débito.
- Save: descriptors/providers tipados coexistem com `SaveManager`; hotbar é o primeiro slice.
- Pause/hit-stop: `GameTimeScaleCoordinator` usa tokens descartáveis, evitando disputa direta por
  `Time.timeScale`.
- RNG: adapters separam Gameplay/World determinístico de Visual não determinístico.
- Relógio: `IGameClock` desacopla agenda de NPC da implementação global de tempo.

## 4. Eventos e performance

- `GameEventBus` continua o barramento único.
- `Publish<T>` usa snapshot imutável reconstruído na alteração de subscriptions; dispatch aquecido
  foi medido com 0 bytes em 1.000 publicações.
- Subscribers devem desinscrever simetricamente e exceções de um handler não interrompem os demais.
- Profiler markers existem para EventBus, agenda, cave materialization/cleanup, save/restore,
  minimap e transição de cena.

## 5. Sistemas de gameplay e seus pontos de extensão

- Quests: `Quests/Runtime/QuestService.cs` é a fonte única de accept/progress/turn-in/save;
  `QuestObjectiveProgressDispatcher` e `QuestDynamicInstancePersistence` isolam roteamento e save.
  Catálogos dinâmicos registram `QuestInstance`; flags passam pelo `QuestFlagRegistry`.
- Cadeias NPC: `Quests/NpcChains/NpcQuestChainCatalog.cs` contém 23x3 etapas; o service apenas
  orquestra o fluxo existente.
- Contratos de caverna: `Quests/CaveContracts/` reutiliza `QuestService` e recompõe recompensas no load.
- Bestiário: `Combat/Bestiary/CanonicalBestiaryCatalog*.cs` contém 117 definições tipadas;
  `Assets/_Game/Data/Enemies/` materializa o universo de 178 IDs.
- Enemy actions: seleção passa por `IEnemyActionSelectionStrategy`; ações e movimentos especiais usam
  registries stateless compartilhados, mantendo os pipelines comuns como fallback.
- UI: evoluir ViewModels/projections existentes. Não criar polling vazio, novo `OnGUI` ou pipeline
  paralelo para shop/inventory/HUD.
- Active skills: `SkillActionEffectCatalog` possui os 30 mapeamentos canônicos; o controller apenas
  orquestra input/alvo/executor e é instalado pelo composition root.
- Town/Farm/Cave: editar geradores em `Scripts/Editor/SceneCreation/` junto com qualquer scene YAML;
  não alterar só um lado.

## 6. Save e compatibilidade

- IDs e nomes de sections persistidas são APIs públicas.
- Mudança de schema requer default seguro, migration, fixture antiga e teste de idempotência.
- Instâncias dinâmicas devem preservar source, template, target, quantidade, level, XP/gold e
  recompensas adicionais.
- Restore reidrata flags concedidas antes de avaliar gates dependentes.
- Não usar `string.GetHashCode`, `UnityEngine.Random` ou tempo de parede para conteúdo persistido.

## 7. Como adicionar funcionalidade sem regressão

1. Identificar o owner existente e seu contrato de save/evento.
2. Preferir uma Strategy/port pequeno quando a variação é real; não criar interface de uso único sem
   fronteira ou teste.
3. Manter fachada pública e referências Unity durante a extração.
4. Escrever teste do comportamento atual antes da troca interna.
5. Executar build das seis assemblies, EditMode completa e PlayMode de composição quando houver
   lifecycle/cena.
6. Atualizar a spec implementada e este mapa quando ownership ou caminho mudar.

## 8. Dívidas intencionais

- Runtime ainda é uma assembly ampla.
- Há bootstraps legados e telas `OnGUI` que só podem ser removidos após equivalência visual/PlayMode.
- Addressables, pooling amplo, chunking e scene loading assíncrono não foram adotados sem profiling.
- O `QuestRegistry` ainda possui autoria hard-coded; migrar para data assets exige compatibilidade de
  IDs, ordem e save, não uma substituição big-bang.

## 9. Gates atuais

- EditMode: 2707/2707 em `TestResults/dependency-v4-full.xml`.
- PlayMode de composição/cenas: 2/2 em `TestResults/dependency-v4-playmode.xml`.
- PlayMode de composição: 2/2 no fechamento da Fase 8; repetir após mudanças de lifecycle/cenas.
- Build: seis projetos são a unidade de validação, não apenas `Assembly-CSharp`.
- Ratchets arquiteturais não podem ser elevados para esconder regressão.
