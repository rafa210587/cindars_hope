---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Cave: identidade Elite do spawn, aparência e combate — refinamento v1

> 2026-09-12. Investigação surgida na aceitação de equilíbrio das skills, sem autorização para
> mudar o gerador da Cave. Evidência: `Logs/skills-phase21-class-probe-r7-20260912.xml` e
> `Logs/phase21_combat_probe/phase21_combat_target_probe_20260913_012758.csv`.

## Resultado para o jogador e fatos observados

Na Cave, um inimigo mostrado como Elite deve ter a mecânica, o risco e a recompensa correspondentes.
Um inimigo comum deve poder aparecer e servir de alvo Common nas bandas de TTK da fase 21. Reentrar
num nível já visitado deve preservar sua composição e seus IDs, conforme ADR-0005.

O probe da fase 21 encontrou 62/62 `CaveEnemySpawnPlanEntry.IsElite=true` em seeds L20/L50/L54.
Isso não significa 62 elites de combate: `CaveEnemySpawnPlanner` pede `AllowElite=true`; o
`EnemySpawnResolver.AddPackSelections` copia esse *permissivo* para `EnemySpawnSelection.IsElite`
em todo membro de pack, inclusive perfil com `CanSpawnAsElite=false`. `CaveEnemyMaterializer` usa
`entry.IsElite` para tint; `EnemyBrain` ativa Elite pelo `EnemyDataSO.IsElite` ou pelo
`EliteAffix` nomeado, e `EnemyHealth` publica loot Elite a partir do asset. Em L20,
`enemy_spore_imp` tem uma instância Vampiric e outra sem afixo, mas ambas têm o flag do plano.
O flag participa de `CaveSnapshotService`/hash. A rotina `EliteAffixRules.ApplyEliteStatBonus`
não tem chamada runtime observada, e o afixo nomeado não chega ao flag de bônus de loot.

## Decisões propostas

- **CE-01 — identidade por instância:** a classe efetiva usada por telemetria e combate vem de
  `EnemyDataSO` + afixo realmente aplicado ao `EnemyBrain`, com `EnemyInstanceId`; o flag
  permissivo do plano não é prova de mecânica Elite.
- **CE-02 — eliminar falso Elite visual:** separar `AllowElite` de seleção efetiva no resolver.
  Um perfil `CanSpawnAsElite` indica elegibilidade, não converte todas as entradas em Elite.
  A frequência/roll de elites intrínsecas requer decisão de design explícita; o roll nomeado
  atual de 8% não deve ser duplicado acidentalmente.
- **CE-03 — recompensa e atributos coerentes:** definir uma única semântica de Elite para tint,
  comportamento, HP/dano e loot. Medir antes/depois e revisar a economia de essência.
- **CE-04 — replay preservado:** qualquer correção de `entry.IsElite` precisa definir leitura
  de snapshots antigos e provar que uma run em curso não re-rola inimigos, IDs, posições,
  afixos ou recursos. Mudança em hash requer estratégia de compatibilidade explícita.

## Escopo e alternativas

Para a fase 21, corrigir apenas a projeção da *fixture* de aceitação e registrar a divergência;
não editar Cave runtime para fazer a matriz passar. A correção de Cave deve ter spec separada,
com teste determinístico de resolver/planner e PlayMode de primeiro acesso, replay e save/load.
Pintar todos os inimigos como comuns tampouco resolve o contrato: apagaria a identidade do Elite
nomeado. Promover todos os membros de packs a Elite com bônus/loot seria uma mudança massiva de
balanceamento sem sustentação no design de Vaalara.

## Questões para a spec

1. Elite intrínseco continua existindo além do afixo nomeado? Se sim, qual chance por banda,
   elegibilidade, seed e limite por sala?
2. Afixo nomeado recebe bônus de HP/dano e essência? Qual teto de recompensa frente à renda
   de Farm/Town e à progressão da Cave?
3. Snapshot antigo com `IsElite=true` mas sem afixo deve preservar só a aparência antiga nessa
   run ou migrar a aparência sem alterar composição/hash? A resposta precisa respeitar ADR-0005.

## Candidata a spec

`spec_cave_elite_spawn_semantics_v1` em `features_futuras/`, bloqueada pelas três decisões acima.
Ela não altera o escopo aprovado da fase 21; a fase 21 pode medir Common/Elite reais por instância
com a projeção corrigida, mas deve reportar separadamente a inconsistência visual/recompensa.
