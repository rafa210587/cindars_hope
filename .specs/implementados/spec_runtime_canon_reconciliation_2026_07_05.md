# Spec retroativa — cânone do runtime implementado em 2026-07-05

Status: `IMPLEMENTED_BUILD_VALIDATED`

## Regra de autoridade

Esta spec registra o comportamento existente após auditoria do código e dos assets. Ela não é uma
ordem para regredir o runtime a números históricos. Em conflitos de quantidade, IDs, layout ou fluxo,
o código e os assets validados neste marco são a fonte primária; a documentação anterior deve ser
tratada como histórica até ser reconciliada.

## Cânone observado

- Bestiário: 178 `EnemyDataSO` com `enemyId`, 178 IDs únicos e zero duplicatas.
- Catálogo tipado `CanonicalBestiaryCatalog`: 117 criaturas (90 comuns/elites, 14 minibosses,
  9 gate bosses e 4 finais). Os outros 61 IDs pertencem ao roster/legado materializado; universo
  total de assets: 178.
- Itens: 213 entradas base, 245 no catálogo expandido.
- Munição: 7 entradas públicas; 6 perfis canônicos mais o alias starter `basic`.
- NPCs: 28 habitantes canônicos com aniversário e matriz de presentes; o peregrino de Vaalara é
  um registro `LegacyRetained`, fora desses contratos sociais.
- Cadeias de NPC: 23 cadeias, 3 etapas cada, total 69 side quests.
- Quest principal: prólogo `mq_act1_00`, Atos 1–5 e 5 marcos finais. O Ato 4 prepara Hope e o Ato 5
  integra Hope antes da decisão final.
- Fazenda: layout v7, 64x44; 4 espécies de animal, incluindo ovelha.
- Cidade: layout v9, 120x90, entrada sul e portas voltadas ao sul.
- Save da caverna: até 12 snapshots persistidos e orçamento sintético de pior caso de 3 MiB.
- Skills: capstones tier 5 exigem 26 pontos gastos na árvore antes da escolha exclusiva.

## Bugs corrigidos durante a reconciliação

- instâncias dinâmicas de quest preservam metadados, XP e recompensas quando registradas antes e
  aceitas posteriormente por ID;
- recompensas de flags entram no `QuestFlagRegistry` e flags concedidas são reidratadas no load;
- recompensas dinâmicas de cave contracts e NPC chains são reconstruídas no restore;
- conclusão do Ato 1 concede `act_1_done`, desbloqueando gates dependentes;
- zonas expansíveis da fazenda mudam de `Blocked` para `Free` ao serem legitimamente desbloqueadas;
- imunidade elemental/material não é sobrescrita pelo dano mínimo;
- arredondamentos de combate, crafting e desconto deixam de depender de banker rounding/float drift;
- perfect block aceita o limite configurado com tolerância numérica;
- presentes nulos são recusados sem falha estrutural;
- save de active skill slots recebe defaults seguros;
- projeções de bestiário/HUD tratam estados vazios e limiares de 25% corretamente;
- IDs de agenda deixam de duplicar o prefixo do NPC;
- destruição de armadilha usa a API correta em PlayMode e EditMode.

## Contratos de não regressão

1. Não reduzir quantidades para satisfazer specs históricas.
2. Não renomear IDs persistidos sem migration e alias explícitos.
3. Não criar segundo pipeline de quest, reward, flag, save, inventário ou economia.
4. Novas instâncias dinâmicas devem passar por `QuestService` e sobreviver a save/load com recompensas.
5. A fila `.specs/a_implementar/` não pode reaplicar specs de layout antigas sobre Town v9/Farm v7.

## Evidência

- `TestResults/canon-reconciliation-final.xml`: 2672/2672 EditMode, exit 0.
- `TestResults/canon-reconciliation-playmode-final.xml`: 2/2 PlayMode composition/scenes.
- Inventário de assets executado diretamente sobre `Assets/_Game/Data/Enemies`.
- Guia arquitetural: `docs/architecture/MODULARIZATION_IMPLEMENTED_ARCHITECTURE_GUIDE.md`.
