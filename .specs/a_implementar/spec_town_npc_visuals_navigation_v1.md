# SPEC — Town NPC Visuals and Navigation v1

**Spec ID:** `town_npc_visuals_navigation_v1`  
**Status:** A_IMPLEMENTAR — especificada por pedido humano em 2026-09-10  
**Wave:** TOWN.N1 — população e circulação  
**Domain:** NPC / TownScene / runtime navigation  
**Priority:** P0  
**Ordem de execucao:** TOWN.N1; após geometria, portas e colliders finais  
**Depende de:** TOWN.V1, TOWN.A1 e TOWN.C1 com geometria/colliders finais  
**Bloqueia:** `spec_town_integrated_acceptance_v1`  
**Repo lock:** NPC schedule/wander/visuals, rotas geradas da Town e testes correspondentes  
**Must not run with:** outra edição dos mesmos runtimes ou regeneração Unity Town/Farm

required_adrs: []
required_game_rules: [city_rules, npc_rules, event_rules]

# /speckit.specify

## Objetivo

Eliminar os dois NPCs placeholder e fazer agenda/wander atravessarem a cidade expandida por caminhos
reais, portas e corredores válidos. Teleporte por timeout deixa de mascarar uma rota inválida.

## Phase 0

- 29 NPCs estão materializados; 27 possuem apresentação final.
- Placeholders visíveis: `npc_corvus` e `npc_vaalara_wanderer_01`.
- `NpcScheduleService` acumula tempo bloqueado sem confirmar progresso físico e teleporta ao destino após
  `StuckTeleportSeconds`.
- `NpcWanderer` move diretamente até o alvo/waypoint e não constitui navegação com avoidance.
- A expansão provou 107/107 rotas geométricas, mas não uma agenda completa percorrida por corpos reais.

## Existing Systems Audit

Reusar `NpcScheduleService`, `NpcScheduleAnchor`, `NpcScheduleRuntimeState`, `NpcWanderer`,
`NpcWalkAnimator`, `NpcVisualRegistry`, `NpcPhysicsBody` e autoabertura de `HouseDoorInteractable`.
Antes de criar componente de rota, procurar provider/grafo reutilizável. Se ausente, introduzir apenas um
contrato Town/NPC mínimo, alimentado por waypoints materializados pelo gerador; não usar `GameObject.Find`
nem referenciar classes Editor em runtime.

## Escopo permitido

- sprites/sheets finais dos dois IDs, fontes Aseprite, import settings e registry existente;
- `Assets/_Game/Scripts/NPC/Schedule/**`, `NpcWanderer.cs`, `NpcVisualRegistry.cs`, `NpcPhysicsBody.cs`;
- componente/runtime puro de rota em `Assets/_Game/Scripts/NPC/Runtime/`, somente se o audit provar falta;
- creator/helpers Town para materializar nodes/edges estáveis;
- testes EditMode/PlayMode town-NPC e evidência sem alterar saves.

## Fora de escopo

Novos NPCs, mudança de personalidade/diálogo/loja/quest, IDs, horários de design, NavMesh 3D, save schema,
Farm/Cave e teleporte como comportamento cotidiano.

# /speckit.plan

1. Criar sprites 4-direções compatíveis com `NpcWalkAnimator` para os dois IDs e validar proporção/pivô.
2. Auditar movimento atual e representar a malha transitável da Town em dados runtime serializados,
   gerados a partir do layout sem dependência de código Editor durante o jogo.
3. Resolver rota determinística entre posição atual e âncora; mover por segmentos e orientar animação.
4. Medir progresso por redução de distância/deslocamento real. Contato, espera de porta ou avoidance não
   contam automaticamente como travamento.
5. Ao chegar em porta fechada, solicitar o fluxo existente, aguardar abertura física e continuar.
6. Se recuperação extrema continuar necessária, publicar diagnóstico observável, limitar frequência e
   escolher ponto seguro livre. Um teste bem-sucedido exige zero recuperações em rotas normais.
7. Snap de load/offscreen é separado da circulação onscreen. Progresso insuficiente é deslocamento
   `<0,10u` em2s; executar no máximo2 replans determinísticos, com desempate ordinal por `NpcId`.
   Após5s, NPC onscreen espera ou retorna por rota ao último node seguro; não teleporta. Offscreen pode
   snapar para node seguro livre, registrando motivo/from/to e sem escrever save.

## Critérios de aceite

- [ ] N-1: 29/29 NPCs usam sprites finais; os dois IDs alvo têm idle/walk 4-direções sem placeholder.
- [ ] N-2: cada âncora work/social/home dos 28 perfis possui rota serializada alcançável.
- [ ] N-3: agenda acelerada de um dia percorre as rotas com corpos/colliders reais e zero teleporte normal.
- [ ] N-4: ao menos um NPC entra e sai de edifício por porta animada, sem atravessar blocker/fachada.
- [ ] N-5: em corredor convergente, interpenetração não excede0,02u por mais de0,5s; após liberar o
  corredor, ambos retomam progresso em até5s pelo desempate ordinal de `NpcId`.
- [ ] N-6: obstáculo induzido aciona no máximo2 replans; log contém motivo/from/to; recuperação não coloca
  NPC sobre Player/NPC/collider, não escreve save e não teleporta ator onscreen.
- [ ] N-7: `ValidateFableCitySchedule` mantém 21/21 ou baseline superior, e novos testes passam integralmente.
- [ ] N-8: nenhum ID, schedule block, anchor, diálogo, loja, quest ou save schema muda.
- [ ] N-9: snap de load/offscreen continua funcional e é reportado separadamente de circulação onscreen.

# /speckit.tasks

- [ ] Autorar e integrar os dois sprites finais com revisão de animação/escala.
- [ ] Auditar e escolher o contrato mínimo de rota/avoidance reutilizável.
- [ ] Materializar malha/waypoints da Town e implementar movimento por segmentos/progresso real.
- [ ] Integrar espera/autoabertura de porta e recuperação excepcional observável.
- [ ] Criar testes determinísticos de rota, progresso e seleção de ponto seguro.
- [ ] Executar PlayMode de agenda acelerada, portas, conflito entre NPCs e preservação de cena/save.
