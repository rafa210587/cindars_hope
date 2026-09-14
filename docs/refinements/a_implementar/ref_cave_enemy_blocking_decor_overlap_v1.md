# Cave — inimigos acessíveis apesar da decoração bloqueante

> **Status:** proposta; sem autorização de implementação do runtime  
> **Origem:** `spec_skills_21_balance_acceptance_v1`, probe TTK r8 de 2026-09-13  
> **Escopo:** primeiro acesso, backtrack e replay da mesma run da Cave

## Problema observado

O Common `enemy_spore_imp` da seed `phase21_seed_l20` nasceu na célula `(58,41)`; o
`cave_elem_20_58_41_DecorBlocking` ocupa a mesma célula. No JSON
`Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_031447_103.json`, o jogador
continuou a avançar e disparou 12 golpes da espada, mas nenhum alcançou o collider `Enemy`.
O raycast físico encontrou o decor entre os dois, enquanto um raycast filtrado por `WorldSolid`
não o encontrou porque o objeto está na layer Default. O player morreu recebendo dano de outros
inimigos; esta amostra não mede TTK nem prova desequilíbrio da espada.

O código permite a coincidência: `CaveEnemySpawnPlanner` escolhe células andáveis sem consultar
o plano de elementos; `CaveEnvironmentElementPlanner` escolhe decor sem consultar os spawns de
inimigos. `CaveRuntimeMaterializer` materializa inimigos antes dos elementos, e
`DecorBlocking` recebe `BoxCollider2D` físico. A regra de stable run exige preservar o snapshot
de cada nível já visitado dentro da mesma seed; corrigir apenas o planner de novas runs não pode
reescrever um nível existente.

## Decisões propostas

- **CB-01:** um inimigo natural não deve nascer dentro de `DecorBlocking` nem ficar sem um ponto
  adjacente de ataque alcançável por movimento normal. Preservar a composição e IDs dos inimigos.
- **CB-02:** o planejador de elementos recebe as células ocupadas pelo plano de inimigos antes de
  selecionar decor bloqueante. O mesmo conjunto de inputs produz a mesma composição e hash;
  não usar aleatoriedade global, tempo ou ordem de GameObjects.
- **CB-03:** a leitura de snapshot já salvo vence o planner novo. ForwardExit, BackExit e load
  restauram o nível visitado sem recriar inimigos/elementos nem mudar `CaveRunSeed`.
- **CB-04:** seeds antigas só são retestadas como novas runs de validação; não alterar silenciosamente
  saves existentes. Um alvo já salvo dentro de decor precisa de uma política de recuperação
  explícita e testada antes de qualquer migração.

## Escopo e limites

Comparar, em EditMode, o par de planos e as células com colisão; em PlayMode, verificar contato
físico e ataque de arma contra um Common e um Elite naturais em seed fixa. Medir impacto na
densidade de elementos e nos hashes de novas runs. Não ajustar dano/alcance da arma para ocultar
o bloqueio, nem mudar o algoritmo de combate por causa deste probe.

Questão aberta: em saves antigos com sobreposição, preservar o obstáculo até nova run ou oferecer
uma migração pontual e versionada? Enquanto não houver decisão, manter snapshots intactos.
Esta proposta é dependência de qualidade de encontro para a fase 21, sem elevar AC02 a PASS.
