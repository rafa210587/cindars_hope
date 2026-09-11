using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// MENU UNICO DO PROJETO — Faxina de menus (2026-06-21).
    ///
    /// Antes existiam ~100 entradas de menu (atributos MenuItem) espalhadas em ~86 arquivos
    /// (geradores, scene creators, validadores, repair, specs antigas). Isso poluia o menu
    /// e confundia o dono. Esta classe consolida TUDO em EXATAMENTE 4 comandos raiz, chamando
    /// diretamente os metodos publicos estaticos que cada gerador/validador/criador ja expoe
    /// (sem duplicar logica, sem editar YAML, sem Play Mode, sem FindObjectOfType).
    ///
    /// Padrao RunStep (best-effort): cada passo loga em PT antes, roda dentro de try/catch que
    /// conta OK/falha e CONTINUA (uma falha nunca aborta o lote), e no fim mostra um resumo no
    /// Console + um EditorUtility.DisplayDialog.
    ///
    /// ─────────────────────────────────────────────────────────────────────────────────────
    /// OS 4 COMANDOS:
    ///
    ///   1) CindarsHope/Inicializar Projeto   (priority 1)
    ///        Gera TODOS os dados (status effects, item catalog, bestiario, skills, weapon
    ///        baselines, loot tables, fishing, farm animals, spell/shape, high-tier recipes,
    ///        enemy runtime data, boss gate assets + phase profiles), salva, recria as 3 cenas
    ///        (Farm/Town/Cave), faz rebuild dos dialogos de NPC, registra as cenas de build e
    ///        salva de novo. (= antigo "Prepare Everything (One Click)".)
    ///        ATENCAO: recriar cenas e DESTRUTIVO (sobrescreve os .unity).
    ///
    ///   2) CindarsHope/Validar Projeto       (priority 2)
    ///        Roda os validadores em modo SO-LEITURA (NAO gera, NAO repara, NAO muta assets):
    ///        registries, catalog consistency, combat databases, skill catalog counts, gift
    ///        taste matrix, family loot tables, status effect DB, projectile prefabs, shop
    ///        price data, town shop catalog integrity, scene transitions, high-tier gear gate.
    ///        Acumula erros/avisos e mostra resumo + dialog.
    ///
    ///   3) CindarsHope/Reparar e Reconstruir (priority 3)
    ///        Recuperacao "algo bugou" — UNICO comando que MUTA/DELETA assets:
    ///          (a) remove entradas null das registries conhecidas;
    ///          (b) FIX DURAVEL DOS DUPLICADOS: para CADA Id duplicado na ItemDatabase, deleta
    ///              em disco (AssetDatabase.DeleteAsset) o asset cujo filename NAO e "{Id}.asset"
    ///              (o legado), mantendo o canonico — resolve de vez Item_Cenoura/Trigo/Semente_*
    ///              vs item_crop_carrot/wheat/seed_*; recipes referenciam por Id string, entao e
    ///              seguro. Depois remove entradas null/orfas que a delecao tenha deixado;
    ///          (c) recria DB quebrado: GenerateCanonicalStatusEffects (self-healing) recria o
    ///              StatusEffectDatabase se o script guid estiver quebrado;
    ///          (d) re-sincroniza a ItemDatabase (GenerateCanonicalItemCatalog.Run faz o sync) e
    ///              SaveAssets + Refresh.
    ///        Mostra resumo + dialog do que foi reparado.
    ///
    ///   4) CindarsHope/Build Standalone Windows (priority 20)
    ///        Build de shipping — delega ao StandaloneBuildPipeline existente. Proposito separado.
    ///
    /// ─────────────────────────────────────────────────────────────────────────────────────
    /// O QUE FICA MANUAL (nao coberto por nenhum comando):
    ///   - PLACEMENT em cena nao exposto por um scene creator idempotente (ex.: marcas dos
    ///     deuses / altares F68, canvas de minimap F38) — permanece manual ate ser adicionado
    ///     a um CreateMvp*Scene.
    ///   - Geracao de arte/sprites/audio real (placeholders sao procedurais em runtime).
    ///   - Validadores "Archive/SPEC NN" de specs antigas: as classes continuam existindo, mas
    ///     sem [MenuItem]; "Validar Projeto" cobre os validadores vivos relevantes.
    /// </summary>
    public static class CindarsHopeMenu
    {
        // Caminhos de assets duplicados/legados que devem ceder ao canonico.
        private const string ItemDatabaseAssetPath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        private static int s_ok;
        private static int s_fail;

        // ─────────────────────────────────────────────────────────────────────────────────
        // 1) INICIALIZAR PROJETO
        // ─────────────────────────────────────────────────────────────────────────────────
        [MenuItem("CindarsHope/Inicializar Projeto", priority = 1)]
        public static void InicializarProjeto()
        {
            ResetCounters();
            Debug.Log("[Inicializar] INICIO — gerando dados, recriando cenas e pos-processamento na ordem certa.");

            // Migracao one-shot (fable_84): renomeia Resources/PlayerSprites/attack/ -> attack_sword/
            // via AssetDatabase.MoveAsset (preserva GUIDs). Idempotente. Necessario para o animador
            // carregar o set de espada, que agora vem de attack_sword/ (e nao mais de attack/).
            RunStep("Renomear pasta de sprites attack/ -> attack_sword/ (fable_84)",
                () => CindarsHope.Editor.RenameAttackSpriteFolder.Execute());

            // spec_codex_13: cria os 7 physics layers de gameplay ANTES de qualquer gerador
            // que atribua layer a prefab/objeto de cena (ordem de dependencia obrigatoria).
            RunStep("Criar physics layers de gameplay",
                () => { CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.EnsureLayers(); });

            // FASE A — geradores de dados (SOs antes das cenas).
            RunStep("Gerar status effects canonicos",
                () => CindarsHope.EditorTools.Combat.GenerateCanonicalStatusEffects.Generate());
            // Cria o asset do Regador Basico (clona a Enxada) ANTES do catalogo, para que o scan de
            // Data/Items o registre no ItemDatabase. Idempotente. (Fase 8 v6: ferramenta de rega.)
            RunStep("Garantir asset do Regador Basico (Data/Items)",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureBasicWateringCanAsset());
            RunStep("Gerar catalogo canonico de itens",
                () => CindarsHope.Editor.Items.GenerateCanonicalItemCatalog.Run());
            // Garante 2x Lagrima da Deusa no inventario inicial (so ACRESCENTA; idempotente).
            // Depende do item_goddess_tear.asset criado pelo passo anterior.
            RunStep("Garantir Lagrima da Deusa (2x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureGoddessTears());
            // Garante 30x Flechas Basicas no inventario inicial para testar o arco (so ACRESCENTA;
            // idempotente). Depende do item_ammo_arrow_basic.asset criado/atualizado pelo passo anterior.
            RunStep("Garantir Flechas Basicas (30x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingArrows());
            // Garante 1x Arco de Madeira no inventario inicial. Sem o arco na outra mao o disparo de
            // flecha sempre bloqueia (ArrowRequiresBowInOtherHand) — sem ele o arco/flecha nao e testavel.
            RunStep("Garantir Arco de Madeira (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingBow());
            // Machado de Ferro: 1x no inventario inicial para testar a animacao do arquetipo Heavy
            // (fable_84/85). Equipar (painel L -> Mao direita) e atacar (E) publica o archetype Heavy.
            RunStep("Garantir Machado de Ferro (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingAxe());
            // Ferramentas de fazenda: enxada e regador para FarmTillingInputController (Fase 8).
            // EquipmentManager.HasTool(Hoe/WateringCan) requer que a ferramenta esteja no inventario.
            RunStep("Garantir Enxada Basica (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingHoe());
            RunStep("Garantir Regador Basico (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingWateringCan());
            // Kit de teste: 1 de cada sistema (arma/magia/armadura/escudo/acessorio/comida/pocoes/
            // reparo/pergaminhos/semente/materiais) no inventario inicial. Idempotente; capacity-aware.
            RunStep("Garantir kit de teste (1 de cada) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureTestStarterKit());
            RunStep("Gerar bestiario canonico",
                () => CindarsHope.Editor.Enemies.GenerateCanonicalBestiary.GenerateMenu());
            RunStep("Gerar catalogo canonico de skills",
                () => CindarsHope.Editor.Skills.GenerateCanonicalSkillCatalog.Generate());
            // Cria/liga os WeaponDataSO dos item_weapon_* (fecha WEAPON_ITEM_NO_WEAPON_ID) ANTES das
            // baselines — assim ApplyWeaponMechanicalBaselines preenche os campos mecanicos das armas novas.
            RunStep("Gerar/ligar WeaponDataSO do catalogo de armas",
                () => CindarsHope.EditorTools.Combat.GenerateWeaponDataFromCatalog.Generate());
            RunStep("Aplicar baselines mecanicas de armas",
                () => CindarsHope.EditorTools.Combat.ApplyWeaponMechanicalBaselines.Apply());
            RunStep("Gerar loot tables por familia de inimigo",
                () => CindarsHope.Editor.EnemyTaxonomy.GenerateFamilyLootTables.Generate());
            RunStep("Gerar tabelas de pesca",
                () => CindarsHope.Editor.World.GenerateFishingTables.Generate());
            RunStep("Gerar assets de animais de fazenda",
                () => CindarsHope.Editor.Farm.GenerateFarmAnimalAssets.Generate());
            RunStep("Gerar itens de aprendizado de magia",
                () => CindarsHope.EditorTools.Magic.GenerateSpellLearningItems.Generate());
            RunStep("Gerar spells de forma (shape spells)",
                () => CindarsHope.EditorTools.Magic.GenerateShapeSpells.Generate());
            RunStep("Gerar receitas de gear high-tier",
                () => CindarsHope.Editor.HighTierGearRecipeGenerator.GenerateHighTierRecipes());
            RunStep("Gerar e wirar assets de inimigos (taxonomia / runtime)",
                () => CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.GenerateAndWire());
            // spec_enemy_attack_kits_v1: gera/realinha kits de ataque (2-3 acoes) para o universo
            // completo (113 fichas canonicas + 7 novas do Roster + meteor_ooze_king) + wireia as 53
            // variancias do Roster para o ActionSetId da mae. DEVE rodar DEPOIS do passo acima
            // (precisa do bestiario canonico + dos 60 EnemyDataSO do Roster ja materializados).
            RunStep("Gerar kits de ataque de inimigo (universo completo + primitivas P2)",
                () => CindarsHope.Editor.EnemyTaxonomy.GenerateEnemyAttackKits.GenerateAll());
            RunStep("Criar assets de gate de boss da caverna",
                () => CindarsHope.Editor.CaveData.CreateCaveBossAssets.CreateAll());
            RunStep("Anexar perfis default de fase de boss",
                () => CindarsHope.Editor.CaveData.AttachDefaultBossPhaseProfiles.AttachAll());
            // spec_cave_biome_art_profiles_runtime (CV01): 8 profiles de arte por bioma, idempotente.
            // Preenche tiles/sprites a partir da convenção Art/Generated/World/cave/<biomeId>/ quando
            // existir; ausência de arte = campo null (fallback aos placeholders atuais, sem erro).
            RunStep("Gerar perfis de arte de bioma da caverna",
                () => CindarsHope.Editor.Cave.GenerateCaveBiomeArtProfiles.Generate());
            // fable_78 (slice 6) — ecossistema da caverna: elementos ambientais, nos de minerio e
            // balance. Ordem de dependencia exigida pelos proprios comentarios dos geradores:
            // EnvironmentElementProfiles referencia os MineNodeDataId de BiomeOreNodes (rodar
            // ANTES ou junto), e EcosystemBalance e independente. Todos idempotentes (merge por
            // Id / preserva asset existente sem sobrescrever tuning manual).
            RunStep("Gerar perfis de elemento ambiental da caverna (fable_78)",
                () => CindarsHope.EditorTools.Cave.GenerateCaveEnvironmentElementProfiles.Generate());
            RunStep("Gerar nos de minerio por bioma da caverna (fable_78)",
                () => CindarsHope.EditorTools.Cave.GenerateCaveBiomeOreNodes.Generate());
            RunStep("Gerar balance do ecossistema da caverna (fable_78)",
                () => CindarsHope.EditorTools.Cave.GenerateCaveEcosystemBalance.Generate());
            // Packs tematicos de spawn por bioma (fable_81). Depende do roster (bestiario) e dos
            // movement profiles (GenerateAndWireSpec13GAssets acima) ja gerados. Idempotente.
            RunStep("Gerar packs tematicos de spawn por bioma (fable_81)",
                () => CindarsHope.Editor.EnemyTaxonomy.GenerateThematicPacksFable81.GeneratePacks());
            // Reimporta os sprites de inimigo (Resources/EnemySprites) com settings de pixel art
            // (Sprite, Uncompressed, Point, PPU=128, BottomCenter). Sem isso os PNGs dropados na pasta
            // entram com import padrao e o inimigo aparece borrado/errado. Idempotente.
            RunStep("Reimportar sprites de inimigo (PPU/Point/BottomCenter)",
                () => CindarsHope.Editor.EnemySpriteImportFixer.FixSize());
            // Bindings de skin (data-driven): mapeia enemyId -> slugs de sprite por referencia cruzada,
            // aplicando a arte nova ao roster que spawna, com variancia deterministica. Aditivo: preserva
            // as entradas curadas do JSON e so acrescenta identidades faltantes. Depende dos PNGs acima.
            RunStep("Gerar/atualizar bindings de skin de inimigo (data-driven, aditivo)",
                () => CindarsHope.Editor.EnemySkins.GenerateEnemySkinBindings.Generate());

            // Profiles de escala visual (player/NPC/props). DEVE rodar antes das cenas: os scene creators
            // anexam VisualScaleApplicator lendo esses profiles; sem eles caem no fallback hardcoded e logam erro.
            RunStep("Gerar profiles de escala visual (player/NPC/props)",
                () => CindarsHope.Editor.ScaleSystem.CreateDefaultScaleAssets.CreateAll());

            // Atribui BodySprite nos NpcDataSO a partir dos PNGs gerados em NpcSprites/<art_folder>.png.
            // Deve rodar ANTES das cenas (FASE C) para que CreateMvpTownScene ja leia BodySprite.
            // NPCs sem PNG sao pulados com aviso; nao falha o lote.
            RunStep("Atribuir sprites de corpo aos NPCs (NpcDataSO.BodySprite)",
                () => CindarsHope.Editor.NPC.AssignNpcBodySprites.AssignAll());

            RunStep("Atribuir retratos (busto) aos NPCs (NpcDataSO.Portrait*)",
                () => CindarsHope.Editor.NPC.AssignNpcPortraitSprites.AssignAll());

            RunStep("Configurar fundos de retrato de NPC como Sprite (NpcPortraitBackgrounds)",
                () => CindarsHope.Editor.NPC.AssignNpcPortraitBackgrounds.ConfigureAll());

            // Fatia as folhas de sprite de caminhada (5 frames x 5 direcoes) de art/npc_anim_gpt/raw/
            // para Resources/NpcWalkSprites/ e popula NpcDataSO.WalkAnimResourcesPath. Deve rodar
            // ANTES das cenas (FASE C) para que o NpcWalkAnimator ja encontre o path no BodySprite/prefab.
            // NPCs sem PNG de caminhada sao pulados com aviso; nao falha o lote.
            RunStep("Gerar/fatiar animacoes de caminhada dos NPCs (walk sheets 5x5)",
                () => CindarsHope.Editor.NPC.GenerateNpcWalkAnimations.GenerateAll());

            // Folhas de animacao dos inimigos do batch 1 (movimento + ataques): fatia
            // art/enemy_anim_gpt/normalized -> Resources/EnemyAnimSprites; o EnemyAnimator (runtime,
            // anexado pelo CaveEnemyMaterializer) carrega por slug. Grade N linhas x 5 colunas.
            RunStep("Gerar/fatiar animacoes de inimigo (walk + ataques, batch 1)",
                () => CindarsHope.Editor.Enemy.GenerateEnemyWalkAnimations.GenerateAll());

            // Pivot dos sprites de mundo (predios/arvores/props/foliage/interior/animais) para
            // BottomCenter, exigido pelo Y-sort global (Custom Axis Y + spriteSortPoint = Pivot).
            // Deve rodar ANTES das cenas (FASE C): os scene creators instanciam esses sprites e o
            // Y-sort so fica correto se o import ja estiver com o pivot certo. Idempotente.
            RunStep("Ajustar pivots dos sprites de mundo (BottomCenter p/ Y-sort)",
                () => CindarsHope.Editor.Art.WorldSpritePivotImportStep.EnsureWorldSpritePivots());

            // FASE B — salvar assets antes das cenas.
            RunStep("Salvar assets gerados (SaveAssets + Refresh) antes das cenas", SaveAndRefresh);

            // FASE C — recriar as 3 cenas (DESTRUTIVO; materializa o wiring deferido).
            RunStep("Recriar FarmScene",
                () => CindarsHope.Editor.SceneCreation.CreateMvpFarmScene.CreateScene());
            RunStep("Recriar TownScene",
                () => CindarsHope.Editor.SceneCreation.CreateMvpTownScene.CreateScene());
            RunStep("Recriar CaveScene",
                () => CindarsHope.Editor.SceneCreation.CreateMvpCaveScene.CreateScene());

            // FASE D — pos-cena.
            RunStep("Rebuild dos dialogos de NPC da cidade",
                () => CindarsHope.Editor.NpcDialogue.RebuildTownNpcDialogues.Rebuild());
            // Aplica a regen de mana canonica (ManaManager.DefaultManaRegenPerSecond) nas 3 cenas
            // recriadas, via SerializedObject. Idempotente.
            RunStep("Aplicar regen de mana canonica nas cenas",
                () => CindarsHope.Editor.SceneCreation.ManaRegenSceneInitializer.ApplyToAllScenes());
            RunStep("Registrar cenas de build (EditorBuildSettings via API)", () =>
            {
                var result = CindarsHope.EditorTools.Build.BuildSceneRegistrar.Register();
                if (!result.Success)
                {
                    throw new InvalidOperationException(result.Message);
                }
            });

            // FASE E — save final.
            RunStep("Salvar assets final (SaveAssets + Refresh)", SaveAndRefresh);

            ShowSummary("Inicializar Projeto", "[Inicializar]");
        }

        // ─────────────────────────────────────────────────────────────────────────────────
        // 2) VALIDAR PROJETO (somente leitura)
        // ─────────────────────────────────────────────────────────────────────────────────
        [MenuItem("CindarsHope/Validar Projeto", priority = 2)]
        public static void ValidarProjeto()
        {
            var report = ValidateProject();
            ShowSummary("Validar Projeto", "[Validar]",
                $"Erros: {report.ErrorCount}; avisos: {report.WarningCount}. Veja os detalhes no Console.");
        }

        /// <summary>Runs the read-only suite and exposes its result without a modal dialog.</summary>
        public static EditorTools.Validation.ValidationReport ValidateProject()
        {
            ResetCounters();
            var aggregated = new EditorTools.Validation.ValidationReport();
            bool allConfigured = true;

            void ValidateStep(string description, Action validation)
            {
                Debug.Log($"[Passo] {description}");
                var report = EditorTools.Validation.ProjectValidationRunner.RunLoggedValidation(description, validation);
                aggregated.Issues.AddRange(report.Issues);
                allConfigured &= report.IsConfigured;
                if (report.Passed) s_ok++;
                else s_fail++;
            }
            Debug.Log("[Validar] INICIO — rodando validadores em modo SO-LEITURA (nao gera, nao repara, nao muta).");

            ValidateStep("Validar registries (null/empty/duplicate Ids)",
                () => CindarsHope.Editor.CindarsHopeProjectMaintenanceMenu.ValidateRegistries());
            ValidateStep("Validar consistencia de catalogos (catalog consistency)",
                () => CindarsHope.Editor.Validation.ValidateCatalogConsistency.Run());
            ValidateStep("Validar combat databases",
                () => CindarsHope.EditorTools.Validation.CombatDatabaseValidationMenu.ValidateCombatDatabases());
            ValidateStep("Validar contagem do catalogo de skills",
                () => CindarsHope.Editor.Skills.GenerateCanonicalSkillCatalog.ValidateCountsMenu());
            ValidateStep("Validar gift taste matrix",
                () => CindarsHope.EditorTools.Validation.GiftTasteMatrixValidator.Run());
            ValidateStep("Validar family loot tables",
                () => CindarsHope.Editor.Validation.ValidateFamilyLootTables.RunValidation());
            ValidateStep("Validar status effect database",
                () => CindarsHope.EditorTools.Combat.ValidateStatusEffectDatabase.ValidateAndReport());
            ValidateStep("Validar projectile prefabs",
                () => CindarsHope.EditorTools.Validation.CombatValidationMenu.ValidateProjectilePrefabs());
            ValidateStep("Validar shop price data",
                () => CindarsHope.Editor.Validation.ValidateShopPriceData.Run());
            ValidateStep("Validar town shop catalog integrity",
                () => CindarsHope.Editor.Validation.ValidateTownShopCatalogIntegrity.Run());
            ValidateStep("Validar high-tier gear fora das lojas",
                () => CindarsHope.Editor.Validation.ValidateHighTierGearNotInShops.Run());
            ValidateStep("Validar scene transitions",
                () => CindarsHope.Editor.Validation.ValidateSceneTransitions.ValidateAll());
            ValidateStep("Validar layout FarmScene v4 (presenca dos elementos §32)",
                () => CindarsHope.Editor.Validation.ValidateFarmSceneLayoutV4.Validate());
            ValidateStep("Validar desvio das rotas da Town (grafo vs linha reta entre ancoras)",
                () => CindarsHope.Editor.Validation.ValidateTownRouteDetourRatio.Validate());
            ValidateStep("Validar bindings de skin de inimigo (slugs existem; profiles cobertos)",
                () => CindarsHope.Editor.EnemySkins.ValidateEnemySkinBindings.Validate());
            ValidateStep("Validar animacoes de caminhada dos NPCs (25 sprites fatiados por NPC)",
                () => CindarsHope.Editor.NPC.ValidateNpcWalkAnimations.Validate());
            ValidateStep("Validar kits de ataque de inimigo (universo completo + variancias)",
                () => CindarsHope.Editor.Validation.ValidateEnemyAttackKits.RunValidation());
            // spec_cave_biome_art_profiles_runtime (CV01): read-only; campos de arte vazios = WARNING.
            ValidateStep("Validar perfis de arte de bioma da caverna",
                () => CindarsHope.Editor.Cave.ValidateCaveBiomeArtProfiles.Validate());
            ValidateStep("Validar wiring de escala visual de atores/props",
                () => CindarsHope.Editor.Validation.ValidateActorScaleWiring.Run());
            ValidateStep("Validar agenda/casas da cidade (fable_11)",
                () => CindarsHope.Editor.Validation.ValidateFableCitySchedule.Validate());
            ValidateStep("Validar ecossistema da caverna (fable_78)",
                () => CindarsHope.Editor.Validation.ValidateCaveEcosystem.Run());
            ValidateStep("Validar ranges de preco das lojas (fable_76 T5)",
                () => CindarsHope.Editor.Validation.ValidateTownShopCatalogIntegrity.RunPriceRanges());

            aggregated.IsConfigured = allConfigured && s_ok + s_fail > 0;
            var summary = aggregated.GetSummary("Validar Projeto");
            if (aggregated.Passed) Debug.Log(summary);
            else Debug.LogError(summary);
            return aggregated;
        }

        public static void ValidateProjectBatch()
        {
            var report = ValidateProject();
            if (!report.Passed)
                throw new InvalidOperationException($"Project validation failed with {report.ErrorCount} error(s).");
        }

        // ─────────────────────────────────────────────────────────────────────────────────
        // 3) REPARAR E RECONSTRUIR (UNICO que muta/deleta assets)
        // ─────────────────────────────────────────────────────────────────────────────────
        [MenuItem("CindarsHope/Reparar e Reconstruir", priority = 3)]
        public static void RepararEReconstruir()
        {
            ResetCounters();
            Debug.Log("[Reparar] INICIO — recuperacao 'algo bugou'. Este comando MUTA/DELETA assets.");

            // (a) Remove entradas null das registries conhecidas.
            RunStep("Remover entradas null das registries",
                () =>
                {
                    int repaired = CindarsHope.Editor.CindarsHopeProjectMaintenanceMenu.RemoveNullsFromAllRegistries();
                    Debug.Log($"[Reparar] Entradas null removidas das registries: {repaired}.");
                });

            // Reimporta os sprites de inimigo com settings de pixel art (mesmo passo do Inicializar).
            RunStep("Reimportar sprites de inimigo (PPU/Point/BottomCenter)",
                () => CindarsHope.Editor.EnemySpriteImportFixer.FixSize());
            // Regenera os bindings de skin (aditivo; preserva curados). Mesmo passo do Inicializar.
            RunStep("Gerar/atualizar bindings de skin de inimigo (data-driven, aditivo)",
                () => CindarsHope.Editor.EnemySkins.GenerateEnemySkinBindings.Generate());

            // (b) Fix duravel dos duplicados: deleta em disco o asset legado de cada Id duplicado.
            RunStep("Deletar assets duplicados/legados da ItemDatabase (fix duravel)",
                DeleteDuplicateLegacyItemAssets);

            // (b.2) Remove entradas mortas (Item==null) de PlayerData.StartingItems — a delecao de
            // assets de item legados acima deixa refs pendentes que disparam STARTING_ITEM_NULL.
            RunStep("Remover StartingItems pendentes (Item==null) do PlayerData",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.Repair());

            // (b.3) Migracao one-shot (fable_84): renomeia Resources/PlayerSprites/attack/ -> attack_sword/
            // (AssetDatabase.MoveAsset, preserva GUIDs). Idempotente. Sem isso o animador nao acha o set de espada.
            RunStep("Renomear pasta de sprites attack/ -> attack_sword/ (fable_84)",
                () => CindarsHope.Editor.RenameAttackSpriteFolder.Execute());

            // (c) Recria DB quebrado (status effects self-healing).
            RunStep("Recriar StatusEffectDatabase se quebrado (self-healing)",
                () => CindarsHope.EditorTools.Combat.GenerateCanonicalStatusEffects.Generate());

            // (d) Re-sincroniza a ItemDatabase (Run faz o sync da registry) e salva.
            // Cria o asset do Regador Basico ANTES da re-sincronizacao (para entrar no ItemDatabase).
            RunStep("Garantir asset do Regador Basico (Data/Items)",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureBasicWateringCanAsset());
            RunStep("Re-sincronizar ItemDatabase (catalogo canonico)",
                () => CindarsHope.Editor.Items.GenerateCanonicalItemCatalog.Run());
            // (e) Garante 2x Lagrima da Deusa no inventario inicial (so ACRESCENTA; idempotente).
            RunStep("Garantir Lagrima da Deusa (2x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureGoddessTears());
            // (e.2) Garante 30x Flechas Basicas no inventario inicial para testar o arco.
            RunStep("Garantir Flechas Basicas (30x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingArrows());
            // (e.3) Garante 1x Arco de Madeira no inventario inicial (sem ele a flecha nao dispara).
            RunStep("Garantir Arco de Madeira (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingBow());
            // (e.3b) Machado de Ferro: 1x no inventario inicial para testar a animacao Heavy (fable_84/85).
            RunStep("Garantir Machado de Ferro (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingAxe());
            // (e.4) Ferramentas de fazenda: enxada e regador (Fase 8; EquipmentManager.HasTool).
            RunStep("Garantir Enxada Basica (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingHoe());
            RunStep("Garantir Regador Basico (1x) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureStartingWateringCan());
            // (e.5) Kit de teste: 1 de cada sistema no inventario inicial (idempotente; capacity-aware).
            RunStep("Garantir kit de teste (1 de cada) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureTestStarterKit());
            // (e.5) Regen de mana canonica nas cenas (idempotente; SerializedObject).
            RunStep("Aplicar regen de mana canonica nas cenas",
                () => CindarsHope.Editor.SceneCreation.ManaRegenSceneInitializer.ApplyToAllScenes());
            RunStep("Salvar assets (SaveAssets + Refresh)", SaveAndRefresh);

            ShowSummary("Reparar e Reconstruir", "[Reparar]");
        }

        // ─────────────────────────────────────────────────────────────────────────────────
        // 3b) KIT DE TESTE NO INVENTARIO INICIAL (standalone — aplicacao rapida)
        // ─────────────────────────────────────────────────────────────────────────────────
        [MenuItem("CindarsHope/Dev/Kit de Teste no Inventario Inicial", priority = 40)]
        public static void AplicarKitDeTeste()
        {
            ResetCounters();
            Debug.Log("[KitTeste] INICIO — acrescentando 1 de cada sistema ao inventario inicial (idempotente).");
            RunStep("Garantir kit de teste (1 de cada) no inventario inicial",
                () => CindarsHope.EditorTools.Repair.RepairPlayerStartingItems.EnsureTestStarterKit());
            RunStep("Salvar assets (SaveAssets + Refresh)", SaveAndRefresh);
            ShowSummary("Kit de Teste", "[KitTeste]",
                "Itens do kit aparecem ao iniciar um JOGO NOVO (StartingItems). Veja o Console para o que foi adicionado/pulado.");
        }

        // ─────────────────────────────────────────────────────────────────────────────────
        // 4) BUILD STANDALONE WINDOWS
        // ─────────────────────────────────────────────────────────────────────────────────
        [MenuItem("CindarsHope/Build Standalone Windows", priority = 20)]
        public static void BuildStandaloneWindows()
        {
            CindarsHope.EditorTools.Build.StandaloneBuildPipeline.BuildFromMenu();
        }

        // ─────────────────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// FIX DURAVEL DOS DUPLICADOS. Para CADA Id duplicado na ItemDatabase (2+ assets com o
        /// mesmo Id), deleta em disco (AssetDatabase.DeleteAsset) o asset cujo filename NAO e
        /// "{Id}.asset" (o legado), mantendo o canonico. Generaliza alem dos 4 conhecidos
        /// (Item_Cenoura/Trigo/Semente_Cenoura/Semente_Trigo -> item_crop_carrot/wheat/
        /// seed_carrot/seed_wheat). Recipes referenciam por Id string, entao deletar o legado
        /// e seguro. Loga cada delecao.
        /// </summary>
        private static void DeleteDuplicateLegacyItemAssets()
        {
            var database = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ItemDatabaseAssetPath);
            if (database == null)
            {
                Debug.LogError($"[Reparar] ItemDatabase nao encontrado em {ItemDatabaseAssetPath}.");
                return;
            }

            var serialized = new SerializedObject(database);
            var items = serialized.FindProperty("_items");
            if (items == null || !items.isArray)
            {
                Debug.LogError($"[Reparar] Propriedade '_items' nao encontrada em {ItemDatabaseAssetPath}.");
                return;
            }

            // Agrupa os PATHS de asset por Id (so trata Ids com 2+ assets em disco).
            var pathsById = new Dictionary<string, List<string>>();
            for (int i = 0; i < items.arraySize; i++)
            {
                var reference = items.GetArrayElementAtIndex(i).objectReferenceValue;
                if (reference == null)
                {
                    continue;
                }

                var idData = reference as CindarsHope.Core.Data.IIdentifiedData;
                if (idData == null || string.IsNullOrWhiteSpace(idData.Id))
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(reference);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (!pathsById.TryGetValue(idData.Id, out var list))
                {
                    list = new List<string>();
                    pathsById[idData.Id] = list;
                }

                if (!list.Contains(path))
                {
                    list.Add(path);
                }
            }

            int deleted = 0;
            foreach (var pair in pathsById)
            {
                if (pair.Value.Count < 2)
                {
                    continue;
                }

                string id = pair.Key;
                string canonicalAssetName = $"{id}.asset";

                foreach (var path in pair.Value)
                {
                    if (System.IO.Path.GetFileName(path) == canonicalAssetName)
                    {
                        continue; // mantem o canonico
                    }

                    if (AssetDatabase.DeleteAsset(path))
                    {
                        deleted++;
                        Debug.Log($"[Reparar] Asset duplicado/legado DELETADO: {path} (Id={id}; mantido o canonico {canonicalAssetName}).");
                    }
                    else
                    {
                        Debug.LogWarning($"[Reparar] Falha ao deletar asset duplicado: {path} (Id={id}).");
                    }
                }
            }

            if (deleted == 0)
            {
                Debug.Log("[Reparar] ItemDatabase: nenhum asset duplicado/legado em disco. Nada a deletar.");
                return;
            }

            // Remove da registry as entradas null que as delecoes deixaram.
            int removedNulls = CindarsHope.Editor.CindarsHopeProjectMaintenanceMenu.RemoveNullsFromAllRegistries();
            Debug.Log($"[Reparar] {deleted} asset(s) duplicado(s) deletado(s); {removedNulls} entrada(s) null removida(s) das registries depois da delecao.");
        }

        private static void ResetCounters()
        {
            s_ok = 0;
            s_fail = 0;
        }

        /// <summary>
        /// Encapsula um passo best-effort: loga em PT antes, roda em try/catch que conta OK/falha
        /// e CONTINUA (uma falha nunca aborta o lote inteiro).
        /// </summary>
        private static void RunStep(string descricaoPT, Action acao)
        {
            Debug.Log($"[Passo] {descricaoPT}");
            try
            {
                acao();
                s_ok++;
            }
            catch (Exception exception)
            {
                s_fail++;
                Debug.LogError($"[Passo] FALHOU: {descricaoPT} — {exception.GetType().Name}: {exception.Message}");
            }
        }

        private static void SaveAndRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ShowSummary(string titulo, string tag, string nota = null)
        {
            var resumo = $"{tag} FIM — {s_ok} passos OK, {s_fail} falhas.";
            if (s_fail > 0)
            {
                Debug.LogError(resumo + " Veja o Console para os passos que falharam.");
            }
            else
            {
                Debug.Log(resumo);
            }

            var msg = $"{s_ok} passos OK, {s_fail} falhas.";
            if (!string.IsNullOrEmpty(nota))
            {
                msg += "\n\n" + nota;
            }
            else
            {
                msg += "\n\nVeja o Console para detalhes" + (s_fail > 0 ? " (procure por linhas '[Passo] FALHOU')." : ".");
            }

            EditorUtility.DisplayDialog(titulo, msg, "OK");
        }
    }
}
