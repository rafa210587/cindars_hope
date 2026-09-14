# Cave — semântica Elite do spawn ao combate e ao replay

> **Spec ID:** spec_cave_elite_spawn_semantics_v1  
> **Status:** RASCUNHO — não aprovado para implementação  
> **Wave/Type/Domain/Priority:** CAVE_BALANCE / Runtime+Data / Cave+Combat+Save / P1  
> **Parallelizable:** NO; **Repo lock:** resolver, planner, materializer, snapshot e testes da Cave  
> **Depends on:** decisão CE-02/03/04 em `docs/refinements/a_implementar/ref_cave_elite_spawn_semantics_v1.md`  
> **Blocks:** aceitação visual e de Elite da `spec_skills_21_balance_acceptance_v1`  
> **Validation:** EditMode seeded + PlayMode Cave + replay/save-load + inspeção gráfica

required_adrs: [ADR-0005, ADR-0018]
required_game_rules: [cave_rules.md, combat_rules.md, save_rules.md]

## Requisitos

- **CE-AC01:** `AllowElite` permite selecionar Elite, mas não torna automaticamente Elite todo
  membro de um pack. A classificação efetiva de cada `EnemyInstanceId` é coerente entre plano,
  `EnemyBrain`, tint, HP/dano e loot. Inimigos de perfil comum sem afixo continuam Common.
- **CE-AC02:** afixo nomeado e eventual Elite intrínseco têm regra de chance, bônus e recompensa
  aprovada, determinística por seed/slot e limitada por banda. Não criar economia de essência
  sem teto.
- **CE-AC03:** no mesmo `CaveRunSeed` e nível visitado, forward/back/reload preservam layout,
  inimigos, recursos, IDs, posições, afixos e estado persistido. Snapshot antigo é lido sem
  re-rolar composição; hash antigo não é silenciosamente recalculado como se fosse novo.
- **CE-AC04:** telemetria e UI reportam classe mecânica da instância, e o jogador consegue
  distinguir Common de Elite na cena. A fase 21 mede as bandas só com essa classe verificada.

## Evidência inicial

O probe `Logs/skills-phase21-class-probe-r7-20260912.xml` passou 1/1 como observação de cena,
mas seu CSV contém 62/62 entradas com `entry.IsElite=true`. Em
`EnemySpawnResolver.AddPackSelections`, `request.AllowElite` é passado diretamente a
`ToSelection`; o planner fixa `AllowElite=true`. `CaveEnemyMaterializer` usa `entry.IsElite`
apenas no tint, enquanto `EnemyBrain` configura afixo por `entry.EliteAffix` e
`EnemyHealth` lê `EnemyDataSO.IsElite` para loot. `CaveSnapshotService` inclui `entry.IsElite`
no conteúdo persistido/hash. O probe da fase 21 foi corrigido separadamente para não tomar esse
flag como classe de combate; isso não repara a Cave.

## Contratos e ownership propostos

- **MODIFY** `Assets/_Game/Scripts/Enemy/EnemySpawnResolver.cs`: em `AddPackSelections` e
  `AddIndividualSelections`, derivar `EnemySpawnSelection.IsElite` da decisão de Elite
  efetivamente tomada, nunca só de `AllowElite`. Preservar a ordem estável de seleção.
- **MODIFY** `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs`: manter a decisão
  determinística do afixo por `(worldSeed, runSeed, level, slot, enemyId)` e atribuir
  `entry.IsElite` só à classe efetiva aprovada. Não alterar ID, posição ou quantidade.
- **MODIFY** `Assets/_Game/Scripts/Cave/Runtime/CaveEnemyMaterializer.cs` e runtime do inimigo
  apenas para usar a mesma classe efetiva em tint, atributos, comportamento e loot; reaproveitar
  `EnemyDataSO`, `EliteAffixRules`, `EnemyBrain` e `EnemyHealth`, sem novo manager.
- **MODIFY** leitura/validação de snapshots apenas se a estratégia CE-04 exigir compatibilidade
  explícita. DTOs persistem IDs/tipos simples, nunca referências Unity.
- **CREATE/MODIFY** testes EditMode de resolver/planner e PlayMode Cave, em suas pastas atuais.
  Nenhum teste muda a seed ou cria inimigo artificial para substituir o encontro natural.

## Plan — condicionado às decisões de design

1. **P0: fechar CE-02/03/04.** Definir Elite intrínseco versus nomeado, chance por banda,
   HP/dano/loot e leitura de snapshots antigos. Registrar o resultado em ADR/game rule se
   mudar o contrato vigente; revisar deltas numéricos acima de 10%.
2. **P1: contrato determinístico.** Em `EnemySpawnResolver.ToSelection`, separar permissão de
   resultado. Para cada seleção, `isElite = eligible && seededDecision`; não usar um `Random`
   novo ou tempo de relógio. O roll nomeado existente permanece uma única vez por slot.
3. **P2: materialização coerente.** Construir uma única classe efetiva por instância a partir
   do asset e da decisão/afixo aprovados; propagar para aparência, status/atributos e loot sem
   alterar `EnemyInstanceId` ou a composição do spawn.
4. **P3: snapshot.** Aplicar estratégia de compatibilidade escolhida para runs já gravadas;
   testar primeiro acesso, backtrack, save/load e retorno após KO. Comparar `LayoutHash`,
   conteúdo e IDs antes/depois da reentrada.
5. **P4: validação.** Repetir seeds L20/L50/L54 do probe fase 21, capturar Common e Elite
   visíveis e executar TTK/loot apenas após verificar a classe mecânica. Registrar JSON/CSV,
   XML e capturas; revisão independente de regressão antes do closeout.

## Tasks — ainda não executáveis

| Done | ID | Trabalho / ownership | Cobre |
|---|---|---|---|
| [ ] | CE-T01 | Fechar decisões de frequência, bônus, loot e snapshot no refinamento; revisar spec | CE-AC01–04 |
| [ ] | CE-T02 | Testes EditMode seeded: pack com `AllowElite=true` e `CanSpawnAsElite=false` resulta Common; mesmo seed reproduz afixos/IDs | CE-AC01–02 |
| [ ] | CE-T03 | Reparar resolver/planner e materializer conforme contrato aprovado, sem mudar composição | CE-AC01–02 |
| [ ] | CE-T04 | Validar e, se necessário, migrar leitura de snapshot antigo; testar ida/volta/save/load/KO | CE-AC03 |
| [ ] | CE-T05 | PlayMode gráfico de Common/Elite e loot; repetir probe e teste de equilíbrio da fase 21 | CE-AC01–04 |
| [ ] | CE-T06 | Revisão independente, hashes, relatório e `/finish-spec` após aprovação e evidência | CE-AC01–04 |

## Falhas e limites

Se uma run antiga não permite distinguir classe efetiva sem reinterpretar o snapshot, a migração
deve falhar fechada para nova aceitação de balanceamento e preservar sua composição. Não promover
`entry.IsElite` contaminado a bônus de combate/loot por conveniência. A frequência de Elite e a
compatibilidade de save estão abertas; esta spec permanece fora da fila executável até decisão
e aprovação humanas. A fase 21 pode continuar com telemetria por instância sem esta alteração.
