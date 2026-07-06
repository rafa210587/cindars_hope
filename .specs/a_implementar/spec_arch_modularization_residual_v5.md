# /speckit.specify — Modularization Residual v5

Status: `APPROVED_BY_HUMAN_IN_EXECUTION`

## Ordem de execucao

ARCH.RUNTIME.V5

## Depende de

- `.specs/implementados/spec_arch_runtime_maintainability_rework_v2.md`
- `.specs/implementados/spec_arch_dependency_cycle_reduction_v3.md`
- `.specs/implementados/spec_arch_narrative_quest_cycle_reduction_v4.md`

## Bloqueia

- promoção da modularização ampla como concluída;
- novos self-bootstraps persistentes sem ownership/teardown;
- novos magic values de skills fora do catálogo canônico;
- novos XMLs brutos de Test Runner versionados.

required_adrs: []
required_game_rules: [input_rules, save_rules, stable_id_rules, event_rules]

## Objetivo

Fechar as ressalvas residuais verificadas em 2026-07-05 sem alterar gameplay, saves, IDs, cenas,
prefabs, preços, quantidades, diálogos ou balanceamento.

## Baseline verificado

- `NpcShopController`: 1.050 linhas / 46 métodos;
- `EnemyBrain`: 925 linhas / 45 métodos;
- 49 atributos reais `RuntimeInitializeOnLoadMethod`, 48 fora do composition root;
- composition root instala 3 serviços e executa 2 resets;
- 47 pares mútuos no dependency snapshot;
- EditMode 2.707/2.707; PlayMode 2/2;
- 12 XMLs adicionados pelo rework local, 141.331 linhas; 48 XMLs rastreados no total;
- branch `dev` nove commits à frente de `origin/dev` no início desta spec;
- working tree contém mudanças paralelas de arte/animação/ProjectSettings/tools, fora do escopo.

## Não regressão

- valores atuais são canônicos e devem ser preservados byte-for-byte quando virarem catálogo;
- MonoBehaviours mantêm campos serializados e APIs externas durante extrações;
- bootstraps só migram após classificação de ownership, ordem, teardown e fallback;
- cada ciclo removido precisa de snapshot antes/depois e não pode criar ciclo substituto;
- mudanças paralelas nunca entram nos commits desta spec.

# /speckit.plan

1. higiene de artefatos e comentário;
2. catálogos tipados para skill tuning e reward IDs;
3. decomposição do shop por collaborators puros;
4. decomposição do brain por state/targeting/config collaborators;
5. installers de domínio para self-bootstraps persistentes;
6. ciclos pequenos restantes;
7. gates e closeout.

# /speckit.tasks

- [x] Lote 1 — higiene de TestResults/comentário;
- [x] Lote 2 — catálogos tipados;
- [ ] Lote 3 — NpcShopController;
- [ ] Lote 4 — EnemyBrain;
- [ ] Lote 5 — bootstrap ownership/installers;
- [ ] Lote 6 — ciclos residuais selecionados;
- [ ] Lote 7 — validação e closeout.
