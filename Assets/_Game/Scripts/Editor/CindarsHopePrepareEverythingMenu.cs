using System;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// "Prepare Everything (One Click)" — orquestra, NA ORDEM CERTA, todos os geradores de
    /// dados (ScriptableObjects), a recriacao das 3 cenas e o pos-processamento (dialogos +
    /// registro de cenas de build). NAO duplica logica: apenas CHAMA os metodos publicos que
    /// cada gerador/criador ja expoe. Tudo via Unity Editor API (sem Play Mode, sem edicao de
    /// YAML, sem FindObjectOfType).
    ///
    /// Cada passo loga "[Prepare] PASSO N: ..." ANTES de rodar, e qualquer falha vira um
    /// LogError "[Prepare] PASSO N FALHOU: ..." sem abortar o lote inteiro (best-effort).
    /// No final, um resumo "X OK, Y falhas" no Console + um dialog.
    ///
    /// ─────────────────────────────────────────────────────────────────────────────────────
    /// O QUE O 1-CLIQUE COBRE (em ordem):
    ///
    ///   FASE A — GERADORES DE DADOS (SOs primeiro; cenas referenciam esses assets):
    ///     1.  Status effects canonicos      (CindarsHope/Combat/Generate Canonical Status Effects)
    ///     2.  Catalogo canonico de itens     (CindarsHope/Generate/Data/Canonical Item Catalog)
    ///     3.  Bestiario canonico             (CindarsHope/Generate/Data/Canonical Bestiary)
    ///     4.  Catalogo canonico de skills    (CindarsHope/Skills/Generate Canonical Skill Catalog)
    ///     5.  Baselines mecanicas de armas   (CindarsHope/Combat/Apply Weapon Mechanical Baselines)
    ///     6.  Loot tables por familia        (CindarsHope/Generate/Loot/Generate Family Loot Tables)
    ///     7.  Tabelas de pesca               (CindarsHope/World/Generate Fishing Tables)
    ///     8.  Assets de animais de fazenda   (CindarsHope/Farm/Generate Farm Animal Assets)
    ///     9.  Itens de aprendizado de magia  (CindarsHope/Magic/Generate Spell Learning Items)
    ///     10. Spells de forma (shape)        (CindarsHope/Magic/Generate Shape Spells)
    ///     11. Receitas de gear high-tier     (CindarsHope/Crafting/Generate High-Tier Gear Recipes)
    ///     12. Assets de inimigos (taxonomia) (GenerateAndWireSpec13GAssets.GenerateAndWire — o
    ///                                          mesmo que "Archive/Advanced/Generate Runtime Assets")
    ///     13. Assets de gate de boss da cave (CindarsHope/.../Create Boss Gate Assets)
    ///     14. Perfis default de fase de boss (CindarsHope/.../Attach Default Boss Phase Profiles)
    ///
    ///   FASE B — SALVAR ASSETS (SaveAssets + Refresh) antes de materializar as cenas.
    ///
    ///   FASE C — RECRIAR AS 3 CENAS (materializam o wiring deferido das specs):
    ///     15. FarmScene  (CindarsHope/Create Scenes/Farm Scene)
    ///     16. TownScene  (CindarsHope/Create Scenes/Town Scene)
    ///     17. CaveScene  (CindarsHope/Create Scenes/Cave Scene)
    ///
    ///   FASE D — POS-CENA:
    ///     18. Rebuild dos dialogos de NPC da cidade (CindarsHope/NPCs/Rebuild Town NPC Dialogues)
    ///     19. Registrar cenas de build              (CindarsHope/Build/Register Build Scenes)
    ///
    ///   FASE E — SaveAssets + Refresh FINAL.
    ///
    /// ─────────────────────────────────────────────────────────────────────────────────────
    /// O QUE O 1-CLIQUE *NAO* COBRE (ainda MANUAL no Unity Editor):
    ///
    ///   - PLACEMENT em cena que NAO foi wired nos scene creators acima. Ex.: F68 (marcas dos
    ///     deuses / altares) e F38 (canvas/objeto de minimap) nao tem passo automatico aqui
    ///     porque seu placement nao esta exposto por um metodo publico de scene creation
    ///     idempotente. Continuam manuais ate alguem adicionar esse placement ao CreateMvp*Scene.
    ///   - Geracao de ARTE/SPRITES/AUDIO real (placeholders procedurais sao runtime; nada a gerar
    ///     aqui).
    ///   - Validadores do Unity (CindarsHope/Validate/...) — rode-os separadamente para conferir.
    ///   - Qualquer menu em CindarsHope/Archive/... que nao esteja listado acima foi deixado de
    ///     fora de proposito (arquivado/legado ou redundante com um passo ja incluido).
    ///   - "Repair and Validate Project" (CindarsHope/Archive/Advanced/...) NAO e chamado aqui:
    ///     ele abre dialogs e faz reparo de cena que e melhor rodar conscientemente.
    ///
    /// Observacao: este menu chama apenas metodos confirmados como `public static`. Se um gerador
    /// novo so expuser handler `private static`, prefira adicionar um metodo publico nele a
    /// forcar reflection/ExecuteMenuItem aqui.
    /// </summary>
    public static class CindarsHopePrepareEverythingMenu
    {
        private static int s_ok;
        private static int s_fail;

        [MenuItem("CindarsHope/Prepare Everything (One Click)", priority = 1)]
        public static void PrepareEverything()
        {
            s_ok = 0;
            s_fail = 0;
            Debug.Log("[Prepare] INICIO — Prepare Everything (One Click). Rodando geradores de dados, cenas e pos-processamento na ordem certa.");

            // ── FASE A: geradores de dados (SOs antes das cenas) ───────────────────────────
            RunStep("1/19 Gerar status effects canonicos",
                () => CindarsHope.EditorTools.Combat.GenerateCanonicalStatusEffects.Generate());

            RunStep("2/19 Gerar catalogo canonico de itens",
                () => CindarsHope.Editor.Items.GenerateCanonicalItemCatalog.Run());

            RunStep("3/19 Gerar bestiario canonico",
                () => CindarsHope.Editor.Enemies.GenerateCanonicalBestiary.GenerateMenu());

            RunStep("4/19 Gerar catalogo canonico de skills",
                () => CindarsHope.Editor.Skills.GenerateCanonicalSkillCatalog.Generate());

            RunStep("5/19 Aplicar baselines mecanicas de armas",
                () => CindarsHope.EditorTools.Combat.ApplyWeaponMechanicalBaselines.Apply());

            RunStep("6/19 Gerar loot tables por familia de inimigo",
                () => CindarsHope.Editor.EnemyTaxonomy.GenerateFamilyLootTables.Generate());

            RunStep("7/19 Gerar tabelas de pesca",
                () => CindarsHope.Editor.World.GenerateFishingTables.Generate());

            RunStep("8/19 Gerar assets de animais de fazenda",
                () => CindarsHope.Editor.Farm.GenerateFarmAnimalAssets.Generate());

            RunStep("9/19 Gerar itens de aprendizado de magia",
                () => CindarsHope.EditorTools.Magic.GenerateSpellLearningItems.Generate());

            RunStep("10/19 Gerar spells de forma (shape spells)",
                () => CindarsHope.EditorTools.Magic.GenerateShapeSpells.Generate());

            RunStep("11/19 Gerar receitas de gear high-tier",
                () => CindarsHope.Editor.HighTierGearRecipeGenerator.GenerateHighTierRecipes());

            RunStep("12/19 Gerar e wirar assets de inimigos (taxonomia / runtime)",
                () => CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.GenerateAndWire());

            RunStep("13/19 Criar assets de gate de boss da caverna",
                () => CindarsHope.Editor.CaveData.CreateCaveBossAssets.CreateAll());

            RunStep("14/19 Anexar perfis default de fase de boss",
                () => CindarsHope.Editor.CaveData.AttachDefaultBossPhaseProfiles.AttachAll());

            // ── FASE B: salvar assets antes de materializar as cenas ───────────────────────
            RunStep("Salvar/atualizar assets gerados (SaveAssets + Refresh) antes das cenas",
                SaveAndRefresh);

            // ── FASE C: recriar as 3 cenas (materializam o wiring deferido) ────────────────
            RunStep("15/19 Recriar FarmScene",
                () => CindarsHope.Editor.SceneCreation.CreateMvpFarmScene.CreateScene());

            RunStep("16/19 Recriar TownScene",
                () => CindarsHope.Editor.SceneCreation.CreateMvpTownScene.CreateScene());

            RunStep("17/19 Recriar CaveScene",
                () => CindarsHope.Editor.SceneCreation.CreateMvpCaveScene.CreateScene());

            // ── FASE D: pos-cena ───────────────────────────────────────────────────────────
            RunStep("18/19 Rebuild dos dialogos de NPC da cidade",
                () => CindarsHope.Editor.NpcDialogue.RebuildTownNpcDialogues.Rebuild());

            RunStep("19/19 Registrar cenas de build (EditorBuildSettings via API)",
                () =>
                {
                    var result = CindarsHope.EditorTools.Build.BuildSceneRegistrar.Register();
                    if (!result.Success)
                    {
                        // Sinaliza falha para o RunStep contar como falha (sem abortar o lote).
                        throw new InvalidOperationException(result.Message);
                    }
                });

            // ── FASE E: save final ─────────────────────────────────────────────────────────
            RunStep("Salvar/atualizar assets final (SaveAssets + Refresh)",
                SaveAndRefresh);

            var resumo = $"[Prepare] FIM — {s_ok} passos OK, {s_fail} falhas.";
            if (s_fail > 0)
            {
                Debug.LogError(resumo + " Veja o Console para os passos que falharam (linhas '[Prepare] ... FALHOU').");
            }
            else
            {
                Debug.Log(resumo);
            }

            EditorUtility.DisplayDialog(
                "Prepare Everything (One Click)",
                $"{s_ok} passos OK, {s_fail} falhas.\n\nVeja o Console para detalhes" +
                (s_fail > 0 ? " (procure por linhas '[Prepare] ... FALHOU')." : "."),
                "OK");
        }

        /// <summary>
        /// Encapsula um passo: loga a descricao em PT antes, roda dentro de try/catch e conta
        /// OK/falha. Uma falha vira LogError e o lote CONTINUA (best-effort, nunca aborta tudo).
        /// </summary>
        private static void RunStep(string descricaoPT, Action acao)
        {
            Debug.Log($"[Prepare] PASSO: {descricaoPT}");
            try
            {
                acao();
                s_ok++;
            }
            catch (Exception exception)
            {
                s_fail++;
                Debug.LogError($"[Prepare] PASSO FALHOU: {descricaoPT} — {exception.GetType().Name}: {exception.Message}");
            }
        }

        private static void SaveAndRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
