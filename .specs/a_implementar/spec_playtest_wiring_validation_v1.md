# SPEC — catálogo e validadores de wiring após playtest

> Spec ID: spec_playtest_wiring_validation_v1
> Status: UNITY_VALIDATED em 2026-09-27 (EditMode 166/166, reparo Editor 2→0); equipamento/portais jogados pendentes. Sem promoção.
> Type: Editor/Data maintenance; Domain: Validation/Items; Priority: P2; Decisão: D22.
> Ownership: World agent; Unity integrado pelo coordenador. Depende de: contrato de ferramentas e perímetro existentes.
> Ordem de execucao: antes da cobertura integrada.
> Bloqueia: aceite do wiring de mundo no pacote integrado.
required_adrs: []
required_game_rules: []

# /speckit.specify
## Phase 0 e objetivo
Duas picaretas item_tool_pickaxe_iron/steel são geradas como Weapon sem WeaponData. Equipment identifica tool por IDs e ItemCategory.Tool existe. Corrigir classificação, não criar WeaponId falso. ValidateSceneTransitions só reconhece SceneTransitionGate/SceneSpawnAnchor, mas Farm/Town usam ScenePortal/SceneSpawnPoint e CaveEntranceInteractable. ValidateFarmSceneLayoutV4 exige root Bounds removido pelo perímetro natural canônico. Corrigir inspeção preservando erros reais.

# /speckit.plan
## Arquivos permitidos
Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs; Assets/_Game/Scripts/Editor/Validation/{RepairPickaxeCatalogClassification,ValidateSceneTransitions,ValidateFarmSceneLayoutV4}.cs; testes SceneTransitionValidationTests.cs e PickaxeCatalogClassificationTests.cs; via API Unity somente Assets/_Game/Data/Items/item_tool_pickaxe_{iron,steel}.asset e metas geradas.
## Contratos e plano
CanonicalItemCatalog.BaseRows mantém IDs/preços/tier, muda somente categoria para Tool. RepairPickaxeCatalogClassification.Repair(): int muda categoria nos dois assets, salva cada um isoladamente, retorna quantidade alterada; segunda execução zero. Não rodar gerador global. Validator aceita componentes atuais sem criar portas e checa destino literal na cena correta. Alias spawn_ só serve cobertura dos IDs esperados, nunca resolve referência inexistente. Cave dinâmica pode continuar reportando falta de anchor/gate se não existe wiring estático verificável; não converter em PASS. Bounds exige cada segmento NaturalBoundary sólido do FarmEnclosedValleyBoundaryContract.

# /speckit.tasks
- [x] Picaretas são Tool e valores 90/220 preservados: PickaxeCatalogClassificationTests.
- [x] Reparo Unity primeiro 2 e segundo 0; validador combat sem erro WeaponId das picaretas (somente INFO opcional de AllowedEquipmentSlots).
- [x] SceneTransitionValidationTests preserva isolamento/cenas dirty e erros de destino/duplicação; teste literal impede aceitar prefixo inexistente.
- [x] Menu integrado executado em batch/read-only: FAIL por 3 erros de transição estática da Cave após corrigir cobertura do ID literal da Farm; Farm 64/64 PASS. Circulação jogada permanece NOT RUN.
## Risco/evidência
Sem alterações runtime nem save schema. Categoria corrige resolução de equipamento; smoke equipar ambas deve completar a aceitação. Editor compile/EditMode e chamada API coordenados. Nenhum resultado pré-declarado; spec permanece em a_implementar até gate completo.
