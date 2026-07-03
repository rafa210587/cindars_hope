# Human Test Scenario — spec_codex_13_physics_layers_contact_filter

## Feature Summary

Verifica que (1) os 7 physics layers de gameplay foram materializados no projeto, (2) combate
(melee, ranged, spell nova) continua acertando inimigos normalmente apos a troca para
ContactFilter2D + buffers reutilizaveis, e (3) inimigos passam a evitar obstaculos solidos
(WorldSolid) em vez de andar direto atraves deles.

Este e o teste de MAIOR RISCO do lote codex_convergence_lote2 (unica spec que toca
ProjectSettings/TagManager e os 3 pontos de query de combate); deve ser validado o quanto antes,
nao apenas no fechamento de wave (Human validation timing: IMMEDIATE_RECOMMENDED).

## Pre-requisito humano (Unity Editor)

Antes de rodar qualquer cenario abaixo, o humano DEVE:

1. Abrir o projeto no Unity Editor.
2. Rodar `CindarsHope/Inicializar Projeto` (o passo "Criar physics layers de gameplay" roda no
   inicio da FASE A, antes dos geradores de cena/prefab).
3. Confirmar no Console: log do RunStep sem erro (`[Passo] Criar physics layers de gameplay`).
4. Verificar em Edit > Project Settings > Tags and Layers que os 7 layers existem:
   `Player`, `Enemy`, `NPC`, `WorldSolid`, `Interactable`, `Projectile`, `Hazard`.
5. Rodar `CindarsHope/Inicializar Projeto` uma SEGUNDA vez e confirmar que os layers nao
   duplicam nem geram erro (idempotencia).

Se este pre-requisito nao for executado, os cenarios de combate ainda devem funcionar (fallback
seguro para NoFilter/mask 0), mas o cenario de obstacle avoidance (Scenario 3) vai FALHAR por
design — o fallback antes da materializacao e "sem filtro/sem avoidance", nao um bug.

## Scenes

- CaveScene (para inimigos + combate + obstacle avoidance)
- FarmScene (para confirmar que Player/paredes/interactables recebem layer sem quebrar a cena)

## Required Initial State

- Save novo ou save existente com o player equipado com uma arma melee e, se possivel, uma arma
  a distancia (arco) e um spell com Nova equipado/conhecido.
- Pelo menos 1 inimigo vivo alcancavel na CaveScene (spawn padrao do bioma cobre isso).

## Scenario 1 — Combate Melee sem Regressao

1. Entrar na CaveScene, aproximar-se de um inimigo.
2. Atacar com a arma equipada (Q ou E, conforme slot).
3. Verificar que o inimigo recebe dano (barra de HP desce, popup de dano aparece).
4. Repetir 2-3 vezes ate o inimigo morrer.

Expected logs: `CombatLog: PlayerAttackHitCandidate` e `CombatLog: PlayerAttackDamageApplied` no
Console (mesmo formato de antes); nenhum erro novo.

## Scenario 2 — Combate a Distancia e Spell Nova sem Regressao

1. Trocar para arma a distancia (arco) e atirar em um inimigo — confirmar acerto e dano.
2. Conjurar um spell do tipo Nova (area) perto de 1+ inimigos — confirmar que todos os inimigos
   no raio recebem dano/status.
3. Confirmar que o auto-target de spell (se aplicavel) ainda mira inimigos corretamente.

Expected logs: `CombatLog: SpellNovaResolved. ... Affected=N` com N > 0 quando ha inimigo no raio.

## Scenario 3 — Obstacle Avoidance de Inimigo (WorldSolid)

1. Com os layers materializados (pre-requisito acima cumprido), localizar um inimigo proximo de
   uma parede solida (ex.: parede de sala da cave, ou parede interna do FarmScene/casa).
2. Posicionar-se de forma que o inimigo precise contornar a parede para alcancar o player.
3. Observar o movimento do inimigo: ele deve desviar da parede (curva ao redor), NAO atravessar
   nem ficar preso/vibrando na quina.

Expected behavior: inimigo desvia visivelmente da parede em vez de atravessar. Se os layers NAO
foram materializados (pre-requisito pulado), o inimigo anda direto como antes (fallback seguro,
nao regressao).

## Scenario 4 — Sem Layers Materializados (fallback seguro)

Em um projeto/sessao onde `CindarsHope/Inicializar Projeto` ainda NAO rodou (ex.: clone limpo):

1. Repetir Scenario 1 e 2 (combate melee/ranged/nova).
2. Confirmar que o combate funciona exatamente como antes (ContactFilter2D cai para NoFilter).
3. Verificar no Console um WARNING one-shot por nome de layer ausente (categoria config-asset,
   nao um erro vermelho de crash): `GameplayLayerNames: wiring-error. ... Reason=layer ausente`.
4. Confirmar que o warning aparece UMA VEZ por nome de layer (nao span a cada ataque).

## Expected Results

- Combate melee/ranged/nova continua acertando inimigos identicamente ao comportamento anterior.
- Apos materializar os layers: inimigos evitam obstaculos solidos visivelmente.
- Antes de materializar os layers: fallback seguro (sem crash, sem regressao de combate), com
  warning one-shot no Console.
- Nenhum erro (vermelho) novo no Console em nenhum dos cenarios.

## Console Expectations

- Errors: NONE (nem no cenario com layers ausentes — so warnings).
- Warnings esperados (quando layers ausentes): `GameplayLayerNames: wiring-error. ... Categoria=config-asset` — no maximo 1 por nome de layer (guard one-shot).
- Forbidden: qualquer Exception/NullReferenceException; qualquer erro de "MissingComponent".

## Pass/Fail Checklist

- [ ] `CindarsHope/Inicializar Projeto` cria os 7 layers sem erro
- [ ] Rodar o gerador 2x nao duplica layers (idempotencia)
- [ ] Combate melee acerta inimigos normalmente
- [ ] Combate ranged (arco) acerta inimigos normalmente
- [ ] Spell Nova acerta todos os inimigos no raio
- [ ] Inimigo evita obstaculo solido apos materializacao dos layers (Scenario 3)
- [ ] Sem layers materializados, combate ainda funciona (fallback seguro, Scenario 4)
- [ ] Nenhum ERROR novo no Console em qualquer cenario
- [ ] Warning one-shot (nao repetido a cada frame/ataque) quando layer ausente

**Overall:** NOT RUN (pending human Play Mode execution)

## Notes

- Tested on: PENDING
- Tested by: PENDING (human QA)
- Date: PENDING
- Known limitation: a matriz de colisao (`Physics2D.SetLayerCollisionMask`) NAO foi configurada
  nesta spec — nenhum par de layer foi desabilitado; todos os pares colidem normalmente por
  padrao do Unity (residual risk documentado no execution report). Isso preserva o comportamento
  atual (nenhum par estava anteriormente bloqueado) e evita o risco maior de regressao de combate
  que uma matriz mal configurada introduziria.
