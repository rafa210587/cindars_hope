using System.IO;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Cave
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01), T007 — gerador idempotente dos 8
    /// CaveBiomeArtProfileSO (1 por bioma canônico de CaveBiomeRegistrySO). Método público estático
    /// SEM [MenuItem] próprio (rule editor-generation-orchestration) — registrado como RunStep em
    /// CindarsHopeMenu.InicializarProjeto.
    ///
    /// Convenção de pasta (arte opcional, ausência = campo null, nunca erro):
    ///   Assets/_Game/Art/Generated/World/cave/&lt;biomeId&gt;/floor_a.png, floor_b.png,
    ///   floor_detail.png, wall_face.png, wall_top.png (tiles)
    ///   Assets/_Game/Art/Generated/World/cave/&lt;biomeId&gt;/chest_closed.png, chest_open.png,
    ///   chest_false.png, hazard_toxic.png, hazard_ice.png, hazard_rock.png, exit_down.png,
    ///   exit_up.png (lote 2 de arte — sprites de objeto, wired direto como Sprite, sem Tile asset)
    ///
    /// Tile assets são criados via AssetDatabase (mesmo padrão de WorldTilemapGround.GetTile) em
    /// Assets/_Game/Data/Cave/Biomes/_TileAssets/, cacheados por nome — nunca YAML manual.
    /// </summary>
    public static class GenerateCaveBiomeArtProfiles
    {
        private const string ProfileDir = "Assets/_Game/Data/Cave/Biomes";
        private const string TileAssetDir = ProfileDir + "/_TileAssets";
        private const string WorldArtRoot = "Assets/_Game/Art/Generated/World";
        private const string ArtRoot = WorldArtRoot + "/cave";
        // spec_cave_decor_placement_runtime (CV02): pools de props/foliage reutilizáveis entre
        // biomas (fora da pasta cave/<biomeId>/), mesma raiz World que o próprio ArtRoot.
        private const string FoliageDir = WorldArtRoot + "/foliage";
        private const string PropsDir = WorldArtRoot + "/props";
        // Fix pós-Play-Mode 2026-07-04: mesma pasta Resources de CombatRuntimeDatabasesRegistry.asset
        // (precedente de fallback via Resources.Load em runtime).
        private const string ResourcesDir = "Assets/_Game/Resources";
        private const string RegistryAssetPath = ResourcesDir + "/CaveBiomeArtProfileRegistry.asset";

        // Espelha CaveBiomeRegistrySO.DefaultBiomes (privado) — fonte de verdade de biomeId/ordem
        // continua no registry; esta tabela só existe para o gerador saber quais 8 pastas conferir.
        // BandId é derivado de CaveBandScaling.BandForLevel(minLevel) — nunca hardcoded em paralelo.
        private static readonly (string BiomeId, int MinLevel)[] CanonicalBiomes = new[]
        {
            ("biome_stone_cavern", 1),
            ("biome_forest", 11),
            ("biome_ice", 26),
            ("biome_fire", 41),
            ("biome_ruins", 56),
            ("biome_abyss", 71),
            ("biome_core", 86),
            ("biome_final", 100),
        };

        /// <summary>Cria/atualiza os 8 profiles. Idempotente: reaproveita asset existente por path;
        /// arte ausente na convenção de pasta = campo permanece/vira null (nunca erro).</summary>
        public static void Generate()
        {
            EnsureFolder(ProfileDir);
            EnsureFolder(TileAssetDir);
            EnsureFolder(ResourcesDir);

            var created = 0;
            var updated = 0;
            var profiles = new System.Collections.Generic.List<CaveBiomeArtProfileSO>(CanonicalBiomes.Length);

            foreach (var biome in CanonicalBiomes)
            {
                var assetPath = $"{ProfileDir}/CaveBiomeArtProfile_{biome.BiomeId}.asset";
                var profile = AssetDatabase.LoadAssetAtPath<CaveBiomeArtProfileSO>(assetPath);
                var isNew = profile == null;
                if (isNew)
                {
                    profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
                    profile.Id = biome.BiomeId;
                }

                var bandId = CaveBandScaling.BandForLevel(biome.MinLevel);
                profile.EditorSetBandId(bandId);

                PopulateFromConvention(profile, biome.BiomeId);

                if (isNew)
                {
                    AssetDatabase.CreateAsset(profile, assetPath);
                    created++;
                }
                else
                {
                    EditorUtility.SetDirty(profile);
                    updated++;
                }

                profiles.Add(profile);
            }

            // Fix pós-Play-Mode 2026-07-04: materializa/atualiza o registry em Resources idempotentemente
            // (mesmo padrão de CombatRuntimeDatabasesRegistry.asset) para que CaveRuntimeMaterializer
            // resolva os profiles via Resources.Load quando o array serializado da cena está vazio
            // (cenas de CaveScene criadas antes desta spec nunca tiveram o array preenchido).
            var registry = AssetDatabase.LoadAssetAtPath<CaveBiomeArtProfileRegistrySO>(RegistryAssetPath);
            var registryIsNew = registry == null;
            if (registryIsNew)
            {
                registry = ScriptableObject.CreateInstance<CaveBiomeArtProfileRegistrySO>();
            }

            registry.EditorSetProfiles(profiles);

            if (registryIsNew)
            {
                AssetDatabase.CreateAsset(registry, RegistryAssetPath);
            }
            else
            {
                EditorUtility.SetDirty(registry);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"GenerateCaveBiomeArtProfiles: {created} profile(s) criado(s), {updated} atualizado(s) em {ProfileDir}; registry Resources/CaveBiomeArtProfileRegistry {(registryIsNew ? "criado" : "atualizado")} com {profiles.Count} profile(s).");
        }

        private static void PopulateFromConvention(CaveBiomeArtProfileSO profile, string biomeId)
        {
            var folder = $"{ArtRoot}/{biomeId}";

            var floorA = LoadSpriteIfExists($"{folder}/floor_a.png");
            var floorB = LoadSpriteIfExists($"{folder}/floor_b.png");
            var floorDetail = LoadSpriteIfExists($"{folder}/floor_detail.png");
            var wallFace = LoadSpriteIfExists($"{folder}/wall_face.png");
            var wallTop = LoadSpriteIfExists($"{folder}/wall_top.png");

            var floorTiles = new System.Collections.Generic.List<TileBase>();
            if (floorA != null) floorTiles.Add(GetOrCreateTile(floorA));
            if (floorB != null) floorTiles.Add(GetOrCreateTile(floorB));
            profile.EditorSetFloorTiles(floorTiles.ToArray());

            profile.EditorSetFloorDetailTile(floorDetail != null ? GetOrCreateTile(floorDetail) : null);

            profile.EditorSetWallTiles(
                wallFace != null ? GetOrCreateTile(wallFace) : null,
                wallTop != null ? GetOrCreateTile(wallTop) : null);

            // Sprites de objeto (hazard/chest/exit) — lote 2 de arte. Convenção de pasta idêntica aos
            // tiles: Load direto por AssetDatabase, ausente na pasta = null (nunca erro). Ainda não
            // populamos _trapSprites (nenhuma convenção de nome-por-trapId definida neste lote) — fica
            // para um lote futuro se a spec de trap art exigir.
            profile.EditorSetHazardSprites(
                LoadSpriteIfExists($"{folder}/hazard_toxic.png"),
                LoadSpriteIfExists($"{folder}/hazard_ice.png"),
                LoadSpriteIfExists($"{folder}/hazard_rock.png"));

            profile.EditorSetChestSprites(
                LoadSpriteIfExists($"{folder}/chest_closed.png"),
                LoadSpriteIfExists($"{folder}/chest_open.png"),
                LoadSpriteIfExists($"{folder}/chest_false.png"));

            profile.EditorSetExitSprites(
                LoadSpriteIfExists($"{folder}/exit_down.png"),
                LoadSpriteIfExists($"{folder}/exit_up.png"));

            // spec_cave_decor_placement_runtime (CV02): pools de decor ambiental do fable_78, por
            // Kind (DecorNonBlocking/DecorBlocking) — DEPRECATED, mantido só para leitura legada.
            // convenção de nome fixa por bioma (não há ainda um segundo bioma com props gerados; quando
            // houver, a mesma convenção de arquivo se aplica em cada pasta de bioma). Ausência de
            // qualquer arquivo = simplesmente não entra no pool (nunca erro); pool resultante pode ficar
            // vazio (fallback ativo no resolver).
            var decorNonBlocking = new System.Collections.Generic.List<Sprite>();
            AddIfExists(decorNonBlocking, $"{folder}/chunk_mushrooms_giant.png");
            AddIfExists(decorNonBlocking, $"{folder}/chunk_stalactites.png");
            AddIfExists(decorNonBlocking, $"{folder}/prop_mine_cart.png");
            AddIfExists(decorNonBlocking, $"{folder}/prop_broken_pickaxe.png");
            AddIfExists(decorNonBlocking, $"{folder}/prop_planks_rail.png");
            AddIfExists(decorNonBlocking, $"{folder}/prop_water_puddle.png");
            // mushroom_cluster é compartilhado entre biomas (World/foliage/), não fica na pasta do bioma.
            AddIfExists(decorNonBlocking, $"{FoliageDir}/mushroom_cluster.png");

            var decorBlocking = new System.Collections.Generic.List<Sprite>();
            AddIfExists(decorBlocking, $"{folder}/chunk_rubble.png");
            AddIfExists(decorBlocking, $"{folder}/chunk_ore_mound.png");
            // rock_ore_0..5 são props compartilhados (massas que ocupam célula), não ficam na pasta do bioma.
            for (var i = 0; i <= 5; i++)
            {
                AddIfExists(decorBlocking, $"{PropsDir}/rock_ore_{i}.png");
            }

            profile.EditorSetDecorSprites(decorNonBlocking.ToArray(), decorBlocking.ToArray());

            // spec_cave_decor_composition_runtime (CV03): pools de decor por CONTEXTO de célula — a
            // colocação por contexto do CaveEnvironmentElementPlanner precisa que o sprite venha do pool
            // certo (teto/wall-hug/chão), não mais só por Kind. Reclassificação do bioma 1 (seção 11):
            //   Ceiling    = chunk_stalactites (única peça que representa algo pendendo do teto).
            //   WallHug    = prop_mine_cart, chunk_ore_mound, chunk_rubble, rock_ore_0..5 (encostado/na parede).
            //   FloorCluster = chunk_mushrooms_giant, mushroom_cluster, prop_broken_pickaxe, prop_planks_rail, prop_water_puddle.
            //   Blocking   = decor que ocupa colisão independente do contexto onde caiu (hoje: chunk_rubble,
            //                chunk_ore_mound, rock_ore_0..5 — os mesmos itens "pesados" do WallHug, que também
            //                podem cair em FloorCluster quando bloqueantes).
            var ceiling = new System.Collections.Generic.List<Sprite>();
            AddIfExists(ceiling, $"{folder}/chunk_stalactites.png");

            var wallHug = new System.Collections.Generic.List<Sprite>();
            AddIfExists(wallHug, $"{folder}/prop_mine_cart.png");
            AddIfExists(wallHug, $"{folder}/chunk_ore_mound.png");
            AddIfExists(wallHug, $"{folder}/chunk_rubble.png");
            for (var i = 0; i <= 5; i++)
            {
                AddIfExists(wallHug, $"{PropsDir}/rock_ore_{i}.png");
            }

            var floorCluster = new System.Collections.Generic.List<Sprite>();
            AddIfExists(floorCluster, $"{folder}/chunk_mushrooms_giant.png");
            AddIfExists(floorCluster, $"{FoliageDir}/mushroom_cluster.png");
            AddIfExists(floorCluster, $"{folder}/prop_broken_pickaxe.png");
            AddIfExists(floorCluster, $"{folder}/prop_planks_rail.png");
            AddIfExists(floorCluster, $"{folder}/prop_water_puddle.png");

            var blocking = new System.Collections.Generic.List<Sprite>();
            AddIfExists(blocking, $"{folder}/chunk_rubble.png");
            AddIfExists(blocking, $"{folder}/chunk_ore_mound.png");
            for (var i = 0; i <= 5; i++)
            {
                AddIfExists(blocking, $"{PropsDir}/rock_ore_{i}.png");
            }

            profile.EditorSetContextDecorSprites(ceiling.ToArray(), wallHug.ToArray(), floorCluster.ToArray(), blocking.ToArray());
        }

        private static void AddIfExists(System.Collections.Generic.List<Sprite> pool, string path)
        {
            var sprite = LoadSpriteIfExists(path);
            if (sprite != null)
            {
                pool.Add(sprite);
            }
        }

        private static Sprite LoadSpriteIfExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>Tile asset (persistente) para um sprite; cacheado por nome — mesmo padrão de
        /// WorldTilemapGround.GetTile, mas gravado em Data/Cave/Biomes/_TileAssets (dados de cave,
        /// não de mundo geral).</summary>
        private static Tile GetOrCreateTile(Sprite sprite)
        {
            var path = $"{TileAssetDir}/tile_{sprite.name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                AssetDatabase.CreateAsset(tile, path);
            }
            else if (tile.sprite != sprite)
            {
                tile.sprite = sprite;
                EditorUtility.SetDirty(tile);
            }

            return tile;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
