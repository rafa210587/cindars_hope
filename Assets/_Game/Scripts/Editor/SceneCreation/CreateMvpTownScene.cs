using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    public static class CreateMvpTownScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";

        [MenuItem("CindarsHope/Scenes/Create MVP TownScene")]
        public static void CreateSceneFromMenu()
        {
            CreateScene();
        }

        public static void CreateScene()
        {
            EnsureFolder("Assets/_Game", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "TownScene";

            var playerTransform = CreatePlayer();
            CreateGround();
            CreateBounds();
            CreateMainCamera();
            CreateSpawnPoints(playerTransform);
            CreatePortals();
            CreateTownDecorations();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            Debug.Log($"MVP TownScene created at {ScenePath}.");
        }

        private static Transform CreatePlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            player.transform.localScale = new Vector3(1f, 1.5f, 1f);

            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.23f, 0.48f, 0.84f);
            spriteRenderer.sortingOrder = 0;
            SetSortingLayerIfExists(spriteRenderer, "Characters");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Town Player placeholder SpriteRenderer was created without a sprite. Replace it with the final placeholder sprite in a future art PR.");
            }

            var rigidbody = player.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.gravityScale = 0f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.85f, 0.85f);

            var playerController = player.AddComponent<PlayerController>();
            ConfigurePlayerController(playerController, rigidbody);

            var interactionTrigger = CreateInteractionTrigger(player.transform);
            var interactionSystem = player.AddComponent<InteractionSystem>();
            ConfigureInteractionSystem(interactionSystem, interactionTrigger);

            return player.transform;
        }

        private static CircleCollider2D CreateInteractionTrigger(Transform parent)
        {
            var triggerObject = new GameObject("InteractionTrigger");
            triggerObject.transform.SetParent(parent);
            triggerObject.transform.localPosition = Vector3.zero;
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            var trigger = triggerObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 1.25f;

            return trigger;
        }

        private static void ConfigurePlayerController(PlayerController playerController, Rigidbody2D rigidbody)
        {
            var serializedController = new SerializedObject(playerController);
            SetReference(serializedController, "_rigidbody", rigidbody);

            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData != null)
            {
                SetReference(serializedController, "_playerData", playerData);
            }
            else
            {
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on TownScene PlayerController.");
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(playerController);
        }

        private static void ConfigureInteractionSystem(InteractionSystem interactionSystem, Collider2D interactionTrigger)
        {
            var serializedInteraction = new SerializedObject(interactionSystem);
            SetReference(serializedInteraction, "_interactionTrigger", interactionTrigger);
            serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactionSystem);
        }

        private static void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, 0f, 1f);
            ground.transform.localScale = new Vector3(18f, 14f, 1f);

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.48f, 0.54f, 0.6f);
            spriteRenderer.sortingOrder = -10;
            SetSortingLayerIfExists(spriteRenderer, "Ground");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Town Ground placeholder SpriteRenderer was created without a sprite. Replace it with tilemap art in a future scene/art PR.");
            }
        }

        private static void CreateBounds()
        {
            var bounds = new GameObject("Bounds");
            bounds.transform.position = Vector3.zero;

            CreateBound("Top", bounds.transform, new Vector2(0f, 7.5f), new Vector2(18f, 1f));
            CreateBound("Bottom", bounds.transform, new Vector2(0f, -7.5f), new Vector2(18f, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-9.5f, 0f), new Vector2(1f, 14f));
            CreateBound("Right", bounds.transform, new Vector2(9.5f, 0f), new Vector2(1f, 14f));
        }

        private static void CreateBound(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var bound = new GameObject(name);
            bound.transform.SetParent(parent);
            bound.transform.position = position;

            var collider = bound.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
        }

        private static void CreateSpawnPoints(Transform playerTransform)
        {
            var parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;

            var defaultSpawn = CreateSpawnPoint(parent.transform, "town_default", new Vector3(0f, -2f, 0f));
            var fromFarmSpawn = CreateSpawnPoint(parent.transform, "town_from_farm", new Vector3(0f, -5f, 0f));

            var installer = parent.AddComponent<SceneSpawnInstaller>();
            var serializedInstaller = new SerializedObject(installer);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);
            serializedInstaller.FindProperty("_spawnPoints").arraySize = 2;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(0).objectReferenceValue = defaultSpawn;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(1).objectReferenceValue = fromFarmSpawn;
            serializedInstaller.FindProperty("_defaultSpawnId").stringValue = "town_default";
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        private static SceneSpawnPoint CreateSpawnPoint(Transform parent, string spawnId, Vector3 position)
        {
            var spawnObject = new GameObject($"Spawn_{spawnId}");
            spawnObject.transform.SetParent(parent);
            spawnObject.transform.position = position;

            var spawnPoint = spawnObject.AddComponent<SceneSpawnPoint>();
            var serializedSpawn = new SerializedObject(spawnPoint);
            serializedSpawn.FindProperty("_spawnId").stringValue = spawnId;
            serializedSpawn.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawnPoint);
            return spawnPoint;
        }

        private static void CreatePortals()
        {
            var portals = new GameObject("Portals");
            portals.transform.position = Vector3.zero;

            CreateScenePortal(
                portals.transform,
                "Portal_Town_To_Farm",
                new Vector3(0f, -6f, 0f),
                new Color(0.78f, 0.62f, 0.24f),
                "FarmScene",
                FarmScenePath,
                "farm_from_town",
                "Voltar para a Fazenda");
        }

        private static void CreateScenePortal(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            string targetSceneName,
            string targetScenePath,
            string targetSpawnId,
            string interactionPrompt)
        {
            var portalObject = new GameObject(name);
            portalObject.transform.SetParent(parent);
            portalObject.transform.position = position;
            portalObject.transform.localScale = new Vector3(1f, 1.35f, 1f);

            var spriteRenderer = portalObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 2;
            SetSortingLayerIfExists(spriteRenderer, "Items");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"{name} placeholder SpriteRenderer was created without a sprite. Replace it with portal art in a future art PR.");
            }

            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var portal = portalObject.AddComponent<ScenePortal>();
            var serializedPortal = new SerializedObject(portal);
            serializedPortal.FindProperty("_targetSceneName").stringValue = targetSceneName;
            serializedPortal.FindProperty("_targetScenePath").stringValue = targetScenePath;
            serializedPortal.FindProperty("_targetSpawnId").stringValue = targetSpawnId;
            serializedPortal.FindProperty("_interactionPrompt").stringValue = interactionPrompt;
            serializedPortal.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(portal);
        }

        private static void CreateTownDecorations()
        {
            var decorations = new GameObject("TownDecorations");
            decorations.transform.position = Vector3.zero;

            CreateDecoration(decorations.transform, "TownWell_Placeholder", new Vector3(-3.5f, 1f, 0f), new Vector3(1.2f, 1.2f, 1f), new Color(0.32f, 0.38f, 0.44f));
            CreateDecoration(decorations.transform, "TownHouse_Placeholder", new Vector3(4f, 2f, 0f), new Vector3(2.2f, 1.6f, 1f), new Color(0.36f, 0.28f, 0.22f));
            CreateDecoration(decorations.transform, "TownLamp_Placeholder", new Vector3(-5f, -2.5f, 0f), new Vector3(0.45f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
        }

        private static void CreateDecoration(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var decoration = new GameObject(name);
            decoration.transform.SetParent(parent);
            decoration.transform.position = position;
            decoration.transform.localScale = scale;

            var spriteRenderer = decoration.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 1;
            SetSortingLayerIfExists(spriteRenderer, "Items");
        }

        private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Serialized field '{propertyName}' was not found on '{serializedObject.targetObject.name}'.");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static Sprite GetBuiltinSprite()
        {
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinSpritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Builtin placeholder sprite not found at '{BuiltinSpritePath}'. TownScene placeholders will need sprites assigned manually or by a future art PR.");
            }

            return sprite;
        }

        private static void SetSortingLayerIfExists(SpriteRenderer spriteRenderer, string sortingLayerName)
        {
            foreach (var layer in SortingLayer.layers)
            {
                if (layer.name != sortingLayerName)
                {
                    continue;
                }

                spriteRenderer.sortingLayerName = sortingLayerName;
                return;
            }

            Debug.LogWarning($"Sorting Layer '{sortingLayerName}' was not found. '{spriteRenderer.gameObject.name}' will use the default sorting layer.");
        }

        private static void EnsureFolder(string parentFolder, string childFolder)
        {
            var fullPath = $"{parentFolder}/{childFolder}";
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolder, childFolder);
        }
    }
}
