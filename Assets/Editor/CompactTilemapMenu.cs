using MementoMori.World;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The generated NavigationFloor lives compacted in a .bytes asset and is rebuilt at runtime. Editor tools
/// that need it while not playing can expand it here; expanding marks nothing dirty unless the scene is saved.
/// </summary>
public static class CompactTilemapMenu
{
    [MenuItem("Memento Mori/Navegação/Mostrar NavigationFloor no Editor")]
    public static void Expand()
    {
        var count = 0;
        foreach (var compact in Object.FindObjectsByType<CompactTilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        { compact.Restore(); count++; }
        Debug.Log($"[CompactTilemap] Expanded {count} tilemap(s). Do not save the scene expanded; use the clear item first.");
    }

    [MenuItem("Memento Mori/Navegação/Limpar NavigationFloor antes de salvar")]
    public static void Collapse()
    {
        foreach (var compact in Object.FindObjectsByType<CompactTilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            compact.GetComponent<UnityEngine.Tilemaps.Tilemap>().ClearAllTiles();
    }
}
