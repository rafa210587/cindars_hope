# Fase 6 — Reuso e desacoplamento interno

Data: 2026-07-05
Branch: `dev`

## Resultado

A fase introduziu contratos pequenos e adaptadores compatíveis, sem remover as fachadas serializadas
existentes:

- seleção de ações inimigas delegada a `IEnemyActionSelectionStrategy`, preservando a ordem atual;
- `IGameClock` usado pelo calendário de NPCs;
- RNG classificado em Gameplay, World e Visual, com adapters seeded e Unity;
- pause/hit-stop coordenado por tokens descartáveis em `GameTimeScaleCoordinator`;
- compra de loja executada por `AtomicPurchaseTransaction`, com compensação se débito falhar;
- providers de save catalogados por descriptor tipado; hotbar é o primeiro slice migrado;
- `InventoryManager` e `PlayerManager` implementam ports transacionais sem alterar os campos de save;
- IDs de agenda corrigidos para o formato canônico `npc_<id>_<bloco>`, aceitando o alias legado da
  cena atual; o gerador de Town passa a produzir o formato correto;
- preço NPC corrige o artefato float que truncava `100 * 0,9` para 89.

State/Presenter de shop e inventory já existia em `ShopMenuViewModel`, projections de inventory e
`MenuProjectionValidator`. Não foi criado um segundo pipeline paralelo; a regra compartilhada agora
obriga evolução por essas projections.

## Validação

- assemblies Unity: 6/6, 0 warnings, 0 erros;
- pause tokens: 2/2 PASS;
- enemy strategy: 1/1 PASS;
- Foundation ports: 2/2 PASS;
- transação atômica: 2/2 PASS;
- registry de save: 2/2 PASS;
- fixtures de save v1–v5: 6/6 PASS;
- agenda/cidade: 16/16 PASS;
- preço: 7/7 PASS;
- suíte completa após as fases 6–8: 2.591/2.670 PASS, 79 falhas; baseline tinha 81 falhas e nenhuma
  falha nova foi adicionada.

Evidências: `TestResults/modularization-phase6-*.xml` e
`TestResults/modularization-phase8-full-editmode.xml`.
