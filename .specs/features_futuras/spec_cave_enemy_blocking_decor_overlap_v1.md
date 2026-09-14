# Cave — impedir spawn dentro de decor bloqueante em novas runs

> **Spec ID:** spec_cave_enemy_blocking_decor_overlap_v1  
> **Status:** RASCUNHO — não aprovado para implementação  
> **Wave/Type/Domain/Priority:** CAVE_BALANCE / Runtime+Validation / Cave+Combat+Save / P1  
> **Parallelizable:** NO; **Repo lock:** planners, materializer, snapshot e testes da Cave  
> **Depends on:** decisão CB-04 em `docs/refinements/a_implementar/ref_cave_enemy_blocking_decor_overlap_v1.md`  
> **Blocks:** aceitação TTK com encontros naturais navegáveis na `spec_skills_21_balance_acceptance_v1`  
> **Validation:** EditMode seeded + PlayMode Cave + replay/save-load + inspeção gráfica

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md, save_rules.md]

## Requisitos

- **CB-AC01:** em toda nova run das seeds de validação L20/L50/L54, a interseção entre células
  de spawn de inimigos e `DecorBlocking` é vazia. Nenhum inimigo perde seu ID, afixo ou classe
  para satisfazer a regra.
- **CB-AC02:** cada alvo de prova Common/Elite tem ao menos um ponto adjacente andável, sem
  sobreposição de sólido, alcançável pelo player; a espada equipada pode consultar o collider
  `Enemy` quando o player está nesse ponto, sem atravessar parede ou decor.
- **CB-AC03:** mesma world seed, run seed e nível geram os mesmos planos e `LayoutHash`. Após
  ForwardExit/BackExit e save/load do mesmo nível, posições, inimigos, elementos e estados vêm
  do snapshot; nenhum planner roda para reinterpretar a composição salva.
- **CB-AC04:** saves antigos com sobreposição mantêm seu snapshot até a política CB-04 ser
  decidida. O sistema não reescreve hash nem desloca o inimigo silenciosamente.

## Estado e fontes

O probe r8 (`Logs/skills-phase21-ttk-common-r8-20260913.xml` e JSON em
`Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_031447_103.json`) falhou
sem kill; o primeiro bloqueador foi `cave_elem_20_58_41_DecorBlocking` na mesma célula do
`enemy_spore_imp`. `CaveEnemySpawnPlanner.ResolveSpawnPoints` valida walkability;
`CaveEnvironmentElementPlanner.IsEligible` não recebe spawns; `CaveRuntimeMaterializer` planeja
os dois domínios separadamente. Esse caso é repro, não taxa de ocorrência medida.

## Contratos e ownership propostos

- **MODIFY** `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs`: antes do plano de
  elementos de uma nova run, passar as células do `LastEnemySpawnPlan` à composição de decor;
  no caminho de snapshot, continuar usando `EnvironmentElements` já serializados.
- **MODIFY** `Assets/_Game/Scripts/Cave/Ecosystem/CaveEnvironmentElementPlanner.cs` e seu
  materializer: rejeitar `DecorBlocking` nas células ocupadas e escolher substituto de modo
  determinístico dentro da seleção já existente. Não remover inimigos nem gerar um novo RNG.
- **MODIFY/CREATE** testes EditMode seeded de interseção e hash e PlayMode de aproximação,
  colisão e ataque; usar os assets reais da Cave. Nenhum DTO novo com referência Unity.
- **FORBIDDEN** nesta spec: reescrever `CaveRunSeed`, regenerar nível já visitado, alterar
  `EnemyInstanceId`, alcance/dano da espada ou frequência de Elite.

## Plan

1. **P0 — decisão de save:** fechar CB-04 no refinamento e revisar a spec antes de movê-la para
   `.specs/a_implementar/`. Auditar snapshot e hash vigentes com seed de repro.
2. **P1 — teste vermelho:** confirmar sobreposição `(58,41)` e uma seed sem sobreposição;
   verificar conjunto de células e hash antes da mudança.
3. **P2 — composição de nova run:** transferir as células de inimigos para o planner de decor;
   excluir só candidatos bloqueantes e manter a ordem/seed da seleção. Registrar deltas de
   densidade e hash de novas runs.
4. **P3 — replay:** rodar primeiro acesso, saída/retorno, save/load e snapshot antigo; comparar
   IDs, posições, decor e hash byte a byte onde o contrato exigir.
5. **P4 — runtime gráfico:** verificar que o Common e o Elite naturais são alcançáveis e podem
   receber golpe de arma por input real. Repetir a célula TTK da fase 21 e revisão independente.

## Tasks — não executáveis até aprovação

| Done | ID | Trabalho | Cobre |
|---|---|---|---|
| [ ] | CB-T01 | Fechar CB-04 e baseline de snapshot/hash | CB-AC03–04 |
| [ ] | CB-T02 | Escrever testes seeded de interseção, alcance e determinismo | CB-AC01–03 |
| [ ] | CB-T03 | Integrar exclusão de spawn no planner de decor de novas runs | CB-AC01–02 |
| [ ] | CB-T04 | Validar replay, save/load e política de snapshot antigo | CB-AC03–04 |
| [ ] | CB-T05 | PlayMode gráfico, repetição TTK e auditoria de regressão | CB-AC01–04 |

## Falhas e limites

Se uma seed não fornecer encontro isolado, a fase 21 registra `INVALID/NOT RUN`, sem inventar
inimigo ou compensar com dano. A proposta ainda precisa da decisão CB-04 e de aprovação; não
autoriza mudança no runtime da Cave.
