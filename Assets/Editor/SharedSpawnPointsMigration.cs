using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.EditorTools
{
    public static class SharedSpawnPointsMigration
    {
        private static readonly Dictionary<string, (string name, Vector2 position, Vector2 facing)[]> Spawns = new()
        {
            ["Quarto"] = new[] { ("SPAWN_Quarto_Entrada", new Vector2(0f, -6.2f), Vector2.up), ("SPAWN_Quarto_PortalLabirinto", new Vector2(0f, -5.2f), Vector2.up) },
            ["Labirinto"] = new[] { ("SPAWN_Labirinto_Inicio", new Vector2(0f, 13f), Vector2.down), ("SPAWN_Labirinto_RetornoLua", new Vector2(-10f, -12f), Vector2.up), ("SPAWN_Labirinto_AreaPortais", new Vector2(-10f, -14.5f), Vector2.up) },
            ["DominioLua"] = new[] { ("SPAWN_DominioLua_Entrada", new Vector2(0f, 16f), Vector2.down), ("SPAWN_DominioLua_Jardim", new Vector2(10f, 4.5f), Vector2.left), ("SPAWN_DominioLua_Espelhos", new Vector2(-10f, -1f), Vector2.right), ("SPAWN_DominioLua_Sigilo", new Vector2(0f, -9f), Vector2.up), ("SPAWN_DominioLua_PortalFinal", new Vector2(0f, -19.2f), Vector2.down) }
        };

        public static void MigrateAndValidate()
        {
            foreach (var item in Spawns)
            {
                var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{item.Key}.unity", OpenSceneMode.Single);
                var parent = GameObject.Find("SceneRoot/_World")?.transform ?? throw new InvalidOperationException($"{item.Key}: missing SceneRoot/_World.");
                foreach (var spawn in item.Value)
                {
                    var target = GameObject.Find(spawn.name) ?? new GameObject(spawn.name);
                    target.transform.SetParent(parent, true);
                    target.transform.position = new Vector3(spawn.position.x, spawn.position.y, 0f);
                    target.transform.up = new Vector3(spawn.facing.x, spawn.facing.y, 0f);
                }
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            Validate();
            EditorApplication.Exit(0);
        }

        public static void Validate()
        {
            foreach (var item in Spawns)
            {
                EditorSceneManager.OpenScene($"Assets/Scenes/{item.Key}.unity", OpenSceneMode.Single);
                foreach (var spawn in item.Value)
                {
                    var target = GameObject.Find(spawn.name);
                    if (target == null || Vector2.Distance(target.transform.position, spawn.position) > .001f)
                        throw new InvalidOperationException($"{item.Key}: serialized spawn {spawn.name} is invalid.");
                }
            }
            Debug.Log("F2.8A: shared serialized spawn points validation passed.");
        }
    }
}
