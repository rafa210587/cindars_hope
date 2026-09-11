# Skills — fase mecânica 4: formas e suporte mágico

> **Spec ID:** spec_skills_14_magic_shapes_v1  
> **Status:** IMPLEMENTADA E VALIDADA — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime / Skills+Magic+Combat / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_13_ranged_identity_v1  
> **Blocks:** spec_skills_15_survival_actions_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode / execute-spec-strict

# /speckit.specify

## Spec

Preservar Fagulha e Laço como projéteis e dar forma própria às demais magias.

- **AC01:** Fagulha custa 10 MP, CD 3 s, alcance 7 e causa 12/14/16/18/20 direto, sem Burn/AoE. Laço custa 14 MP, CD 6 s, alcance 7, causa 10/12/14 e aplica Chill de 50% por 3/4/5 s. Se já estava Chilled antes do impacto, aplica uma vez/cast Slow forte de 70% por 1 s, sem root, e avança a DR compartilhada; Chill base é leve e não avança DR. Elite usa duração ×0,60 e teto 2,4 s/ativação. Boss fora de janela recebe só dano; em janela usa duração ×0,30 e DR no Slow forte. Resistência precede os limites.
- **AC02:** Chama custa 8 MP, CD 2,5 s e causa 8/10/12 em cone 70°×2,5, máximo quatro alvos e um hit/alvo, 100% no primeiro e 70% nos demais. Aplica/refresh uma vez por cast o perfil canônico `status_burn_minor` (2 por tick, 4 ticks), sem empilhar nem redefinir o asset.
- **AC03:** Rajada custa 16 MP, CD 6 s, alcance 6 e lança três projéteis de 6/7/8 em spread 30°. Mesmo alvo recebe no máximo dois; segundo a 50% com `RoundToInt`, teto single 9/11/12. Cada alvo recebe Chill uma vez/cast por 3 s, sem Slow forte e sem múltiplos avanços de DR. Elite usa ×0,60 e teto 2,4 s; boss somente em janela, ×0,30.
- **AC04:** Nuvem custa 18 MP, CD 8 s e exige ponto de mira válido/line-of-effect em até 6 tiles antes do commit. É zona raio 2 por 4 s, com pulsos globais t=0/1/2/3; quatro pulsos dividem exatamente 24/30/36/42/48 por alvo e o resto vai no último. Não há catch-up ao reentrar, há dedupe de colliders e máximo quatro ticks/alvo. `status_poison` aplica/refresh somente na primeira entrada elegível do alvo por cast e fica fora do orçamento de dano direto.
- **AC05:** Corrente custa 20 MP, CD 8 s, alcance inicial 9 e salta em até quatro alvos distintos a 3 tiles, 100/75/55/40%, com arredondamento após falloff e um hit/alvo. Cada segmento exige line-of-effect; candidato bloqueado é ignorado. Ordem: distância², X, Y e identidade runtime estável. Construct com vulnerabilidade Lightning >1 recebe posture adicional de 0,50 × dano final; demais recebem zero posture especial.
- **AC06:** Guarda custa 18 MP, CD 14 s, reduz 25/32/40% por 3/4/5 s e tem exatamente duas cargas. Só dano direto Fire/Ice/Toxic/Lightning com raw restante >0 consome/reduz; DoT, físico e stagger passam. Ordem: barreira → block → Guarda → Defense/resistência; hit zerado antes da Guarda não consome. Recast reinicia duração e cargas; expiração, morte ou troca de cena limpa.
- **AC07:** Sigilos custa 18 MP, CD 8 s, cria zona fixa sob o caster, raio 2,5, dura 4/5/6/7/8 s e aplica Slow 20/24/28/32/35%, sem dano. Cada alvo abre um orçamento por zona; reentrada não renova/avança DR. Elite: `min(restante ×0,60×DR, 3,2 s)`. Boss ignora fora da janela; dentro recebe magnitude fixa 10% e duração `min(janela, restante ×0,30×DR)`, encerrada ao fechar a janela. Sobreposição não soma; prevalece o maior Slow.
- **AC08:** timings autorados `windup/active/recovery`: Fagulha `0,18/0/0,25`; Laço `0,25/0/0,25`; Chama `0,30/0,10/0,30`; Rajada `0,30/0/0,30`; Corrente `0,35/0/0,30`; Nuvem `0,70/0,10/0,40`; Guarda `0,55/0,10/0,40`; Sigilos `0,70/0,10/0,40`. Movimento é permitido, dano no windup cancela, custo/CD entram só no commit e whiff pós-commit paga/recovery.
- **AC09:** benchmark determinístico registra dano direto+DoT, MP, controle e TTK para neutro, trio, elite e boss em/fora de janela, usando ciclo completo `windup+active+recovery+cooldown`. Chama single sustentada ≤110% da Fagulha R5; Nuvem com quatro pulsos+Poison ≤115%; elite com `duração efetiva/cooldown ≤0,40`; boss zero fora da janela. A rotação Fagulha+Chama+Laço+Nuvem com MP 100/regen 5 deve drenar recurso; Fagulha isolada pode ser sustentável. A aceitação comparativa final integra a Fase 21.

Fora: capstone Confluência, Purificar, Contenção de Circuito e arte.

# /speckit.plan

Criar executores `ConeSkillEffectExecutor`, `PersistentZoneSkillEffectExecutor`,
`ChainSkillEffectExecutor`, `ElementalWardSkillEffectExecutor`; estender `SlowFieldSkillEffectExecutor`
e `ProjectileSkillEffectExecutor` apenas nas semânticas mantidas. Reusar status e damage pipeline.
Força e duração do Slow forte ficam em campos estáveis de `SkillActionSO` (`StrongSlowFraction=0,70`,
`StrongSlowDurationSeconds=1`) e não reutilizam `status_slow`, cujo perfil vivo reduz apenas 30%.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SlowFieldSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ConeSkillEffectExecutor.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/PersistentZoneSkillEffectExecutor.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ChainSkillEffectExecutor.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ElementalWardSkillEffectExecutor.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- `Assets/_Game/Scripts/Combat/PlayerDamageReceiver.cs` (somente consumo do ward)
- `Assets/_Game/Scripts/Foundation/IncomingDamageModifierProvider.cs` (CREATE; porta neutra do ward)
- `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` e `EnemyHealth.cs` (família canônica consultável)
- `Assets/_Game/Scripts/Editor/Enemies/GenerateCanonicalBestiary.cs` (materialização da família)
- testes `SkillMagicShapeTests.cs` e `SkillMagicShapesPlayModeTests.cs` (CREATE)

Ordem determinística: readiness/alvo→windup→revalidar→commit+pagar→materializar forma→ordenar distância+ID→aplicar dano/status→cooldown→feedback.
Zona guarda seu actionId/rank resolvidos no cast. Edge cases: alvo sai/entra, caster morre, dois colliders,
chain sem próximo alvo, resistência e boss fora de janela.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Cone/fan e refresh de gelo | AC01–AC03 |
| [x] | T02 | Zona tóxica com orçamento total | AC04 |
| [x] | T03 | Chain determinística | AC05 |
| [x] | T04 | Ward e Sigilos com gates | AC06–AC07 |
| [x] | T05 | Integrar catálogo, timings e testes de formas/obstáculo/whiff | AC01–AC08 |
| [x] | T06 | Benchmarks determinísticos + Unity/EditMode/PlayMode `failed=0` + closeout | AC01–AC09 |
