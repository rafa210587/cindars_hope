# /speckit.specify — Runtime Maintainability Rework v2

Status: `IMPLEMENTED_BUILD_VALIDATED`

## Ordem de execucao

ARCH.RUNTIME.V2

## Depende de

- `docs/architecture/MODULARIZATION_IMPLEMENTED_ARCHITECTURE_GUIDE.md`
- `.specs/implementados/spec_runtime_canon_reconciliation_2026_07_05.md`

## Bloqueia

- novas assemblies de domínio sem contratos direcionais;
- novos auto-bootstraps, input polling ou `OnGUI`.

required_adrs: []
required_game_rules: [event_rules, save_rules, input_rules]

## Objetivo

Reduzir acoplamento, duplicação e custo de manutenção do runtime atual sem alterar gameplay, IDs,
saves, conteúdo, cenas ou ordem observável dos fluxos existentes.

## Escopo incremental

1. lifecycle e input centralizados no composition root;
2. decomposição de QuestService e NpcShopController por adapters/strategies puros;
3. execução de action/movement inimigo por strategy registry;
4. primeira assembly de domínio puro, criada apenas após remoção das dependências reversas;
5. migração gradual de UI/data legacy com equivalência comprovada;
6. PlayMode para boot, save/load, quests e transições.

## Não regressão

- 178 IDs de criatura e 117 definições tipadas preservados;
- Town v9 120x90 e Farm v7 64x44 preservados;
- 2672 testes EditMode devem permanecer verdes ou crescer;
- saves antigos continuam carregando;
- nenhum novo singleton, global search, direct input ou `OnGUI`;
- alterações concorrentes de animação/sprites/ProjectSettings ficam fora dos commits.

## Evidência de conclusão

- builds de todas as assemblies com exit 0;
- EditMode completa;
- PlayMode relevante;
- architecture ratchet sem aumento;
- mapa de dependências re-medido;
- handoff do Claude atualizado após cada lote.

# /speckit.plan

Executar por fachadas compatíveis (Strangler Pattern), com commits pequenos e reversíveis.

# /speckit.tasks

- [x] Lote 1 — lifecycle/input
- [x] Lote 2 — Quest/NPC
  - [x] progresso de objetivos em dispatcher tipado, sem varreduras LINQ por evento;
  - [x] persistência de instâncias dinâmicas extraída, incluindo fallback legado;
  - [x] lifecycle de interação do `NpcShopController` isolado em máquina de estados pura;
  - [x] política da quest da Thalindra isolada para manter opção visível e modo emitido consistentes.
- [x] Lote 3 — Enemy strategies
  - [x] famílias de ações especiais roteadas por strategy registry compartilhado;
  - [x] movimentos especiais roteados por strategy registry compartilhado;
  - [x] registries usam tabelas indexadas por enum e strategies stateless, sem alocação por inimigo.
- [x] Lote 4 — assembly pura
  - [x] `CindarsHope.Gameplay` criada com `noEngineReferences` e zero referências;
  - [x] decisões puras de input/NPC e contrato de modo da quest movidos com GUIDs preservados;
  - [x] teste arquitetural trava escopo curado e proíbe dependência de Unity.
- [x] Lote 5 — UI/data legacy
  - [x] catálogo de 30 ações/effects removido do `MonoBehaviour` e tornado read-only/puro;
  - [x] testes deixaram de refletir campo privado do controller;
  - [x] executor de skills instalado pelo composition root, removendo um auto-bootstrap;
  - [x] `OnGUI` e placeholders sem substituto comprovado mantidos e registrados como dívida intencional.
- [x] Lote 6 — validação e closeout

## Resultado final

- seis assemblies explícitas: exit 0, 0 erros, 0 warnings;
- EditMode: 2.703/2.703;
- PlayMode de composição e cenas: 2/2;
- architecture ratchet: PASS; `RuntimeInitialize` caiu de 63 para 62;
- dependency snapshot: 1.570 arquivos, 216 edges por pasta, 49 pares mútuos, 29 tipos internal e
  3 crossings de teste;
- nenhum ID, save schema, cena, quantidade, preço, diálogo ou balanceamento foi alterado;
- dívida visual/`OnGUI` sem substituto permanece intencional e fora deste rework.
