using System.IO;
using MementoMori.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using MementoMori.Player;
using MementoMori.Poe;

namespace MementoMori.EditorTools
{
    public static class ProjectBootstrap
    {
        private static readonly string[] SceneNames = { "MainMenu", "Quarto", "Labirinto", "DominioLua", "FinalBeta" };

        [MenuItem("Memento Mori/Setup/Create Foundation")]
        public static void CreateFoundation()
        {
            const string sceneFolder = "Assets/Scenes";
            Directory.CreateDirectory(sceneFolder);
            var buildScenes = new EditorBuildSettingsScene[SceneNames.Length];

            for (var index = 0; index < SceneNames.Length; index++)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("SceneRoot");
                root.AddComponent<SceneConfiguration>();
                var systems = CreateChild(root.transform, "_Systems");
                CreateChild(systems.transform, "EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                CreateChild(systems.transform, "GameSystems", typeof(GameSystemsBootstrap));

                var world = CreateChild(root.transform, "_World");
                var grid = CreateChild(world.transform, "Grid", typeof(Grid));
                CreateChild(grid.transform, "Tilemap_Ground", typeof(Tilemap), typeof(TilemapRenderer));
                CreateChild(grid.transform, "Tilemap_Walls", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
                CreateChild(world.transform, "Props");
                CreateChild(world.transform, "Interactables");
                CreateChild(world.transform, "Doors");
                CreateChild(world.transform, "Triggers");
                CreateChild(root.transform, "_Lighting");
                CreateChild(root.transform, "_VFX");
                CreateChild(root.transform, "_UI");

                if (SceneNames[index] != "MainMenu" && SceneNames[index] != "FinalBeta")
                    CreatePlayer(root.transform);
                if (SceneNames[index] == "Labirinto" || SceneNames[index] == "DominioLua")
                    CreatePoe(root.transform);

                var camera = CreateChild(root.transform, "Main Camera", typeof(Camera));
                camera.tag = "MainCamera";
                camera.GetComponent<Camera>().orthographic = true;

                var path = $"{sceneFolder}/{SceneNames[index]}.unity";
                EditorSceneManager.SaveScene(scene, path);
                buildScenes[index] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = buildScenes;
            AssetDatabase.SaveAssets();
            Debug.Log("Memento Mori foundation created: five scenes and Build Settings configured.");
        }

        private static GameObject CreateChild(Transform parent, string name, params System.Type[] components)
        {
            var child = new GameObject(name, components);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void CreatePlayer(Transform root)
        {
            var characters = CreateChild(root, "_Characters");
            var player = CreateChild(characters.transform, "Player", typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerController));
            player.tag = "Player";
            var body = player.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var detector = CreateChild(player.transform, "InteractionDetector", typeof(CircleCollider2D), typeof(MementoMori.Interaction.InteractionDetector));
            detector.GetComponent<CircleCollider2D>().isTrigger = true;
            detector.GetComponent<CircleCollider2D>().radius = 1f;
        }

        private static void CreatePoe(Transform root)
        {
            var characters = root.Find("_Characters") ?? CreateChild(root, "_Characters").transform;
            CreateChild(characters, "Poe", typeof(PoeFollower));
        }
    }
}
