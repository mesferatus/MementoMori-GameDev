using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Opt-in visual pass. Does not move gameplay, replace cells or change existing colliders.
public static class MoonBibliaV3ArtPass
{
    const string Art = "Assets/Art/FinalProduction/BibliaV3/";
    static Transform root;
    static Sprite Sprite(string sheet, string name) => AssetDatabase.LoadAllAssetsAtPath(Art + sheet + ".png").OfType<Sprite>().First(s => s.name == name);
    static Transform Group(string name)
    {
        var t = root.Find(name);
        if (t != null) return t;
        var go = new GameObject(name); Undo.RegisterCreatedObjectUndo(go, "Lua V3 visual"); go.transform.SetParent(root, false); return go.transform;
    }
    static SpriteRenderer Prop(string group, string name, Sprite sprite, float x, float y, int order = 15)
    {
        var parent = Group(group); var t = parent.Find(name);
        var go = t != null ? t.gameObject : new GameObject(name);
        if (t == null) { Undo.RegisterCreatedObjectUndo(go, "Lua V3 visual"); go.transform.SetParent(parent, false); }
        var r = go.GetComponent<SpriteRenderer>(); if (r == null) r = Undo.AddComponent<SpriteRenderer>(go);
        Undo.RecordObject(r, "Lua V3 sprite"); r.sprite = sprite; r.sortingOrder = order;
        go.transform.position = new Vector3(x,y,0); r.spriteSortPoint = SpriteSortPoint.Pivot;
        return r;
    }
    static Sprite Garden(int i) => Sprite("MOON_03_GARDEN_LibraryV2", "MOON_03_GARDEN_V2_" + i.ToString("D3"));
    static Sprite Monument(int i) => Sprite("MOON_04_MONUMENTS_LibraryV2", "MOON_04_MONUMENTS_V2_" + i.ToString("D3"));
    public static string Apply()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/DominioLua.unity" || Application.isPlaying) throw new InvalidOperationException("Requires Moon scene in Edit Mode");
        var existing = GameObject.Find("BibliaV3_Visual");
        var go = existing != null ? existing : new GameObject("BibliaV3_Visual"); if(existing == null) Undo.RegisterCreatedObjectUndo(go,"Lua V3 visual"); root=go.transform;
        var grid = GameObject.Find("LuizaOriginalGeometry/Grid").transform;
        var oldWall=grid.Find("Walls_Original_1711").GetComponent<Tilemap>();
        var priorWall = grid.Find("Architecture_ContinuousStone");
        var wallGO = priorWall != null ? priorWall.gameObject : new GameObject("Architecture_ContinuousStone",typeof(Tilemap),typeof(TilemapRenderer));
        if(priorWall == null) Undo.RegisterCreatedObjectUndo(wallGO,"Lua V3 walls"); wallGO.transform.SetParent(grid,false);
        var map=wallGO.GetComponent<Tilemap>(); var render=wallGO.GetComponent<TilemapRenderer>(); render.sortingOrder=-11;
        render.sharedMaterial=oldWall.GetComponent<TilemapRenderer>().sharedMaterial;
        var tiles=new Tile[4];
        for(int i=0;i<4;i++) { var path=Art+"WallFill"+i+".asset"; tiles[i]=AssetDatabase.LoadAssetAtPath<Tile>(path); if(tiles[i]==null){tiles[i]=ScriptableObject.CreateInstance<Tile>(); tiles[i].sprite=Sprite("M1_MoonWallVariants_Runtime","M1_WallVariant_0_"+i);tiles[i].colliderType=Tile.ColliderType.None; AssetDatabase.CreateAsset(tiles[i],path);} }
        int cells=0;
        foreach(var p in oldWall.cellBounds.allPositionsWithin) if(oldWall.HasTile(p)) {map.SetTile(p,tiles[Math.Abs(p.x*13+p.y*7)%4]);cells++;}
        // Dress the lake on its blocked inner edge; leave the exterior walking ring open.
        var trees = new [] {new Vector3(49,-49,4),new Vector3(64,-49,11),new Vector3(48,-55,12),new Vector3(65,-54,17),new Vector3(51,-57,10),new Vector3(63,-57,15)};
        int k=0;
        foreach(var p in trees)
        {
            Prop("M2_Garden", "Tree_"+k, Garden((int)p.z),p.x,p.y,20);
            for(int j=0;j<4;j++) Prop("M2_Undergrowth", "Cluster_"+k+"_"+j,Garden(new[]{20,24,28,38}[j]),p.x+(j-1.5f)*0.65f,p.y-0.3f+(j%2)*0.45f,22);
            k++;
        }
        Prop("M2_Monuments","NorthLunarFountain",Monument(1),57,-46,18);
        Prop("M2_Monuments","WestMoonShrine",Monument(4),49,-52,21);
        Prop("M2_Monuments","EastMoonShrine",Monument(5),65,-52,21);
        Prop("M2_Monuments","WestRuinedArch",Garden(49),50,-48,18);
        Prop("M2_Monuments","EastRuinedArch",Garden(49),64,-48,18);
        for(int i=0;i<18;i++)
        {
            float a=i*Mathf.PI*2/18f; float x=57.5f+8.1f*Mathf.Cos(a),y=-51.5f+5.1f*Mathf.Sin(a);
            Prop("M2_Shore", "Rocks_"+i,Garden(37+i%5),x,y,16);
            if(i%2==0) Prop("M2_Shore","Flowers_"+i,Garden(24+i%5),x+0.5f,y-0.2f,17);
        }
        Prop("M2_WaterFX","MoonReflection",Sprite("MOON_06_LIGHTING_FX","WATER_SHIMMER"),57,-51,-13);
        foreach(var p in new[]{new Vector2(51,-50),new Vector2(63,-50),new Vector2(54,-55),new Vector2(61,-54)})
        {var r=Prop("M2_WaterFX","Glimmer_"+p,Sprite("MOON_06_LIGHTING_FX","RUNE_PULSE"),p.x,p.y,-14);r.color=new Color(.45f,.72f,1,.22f);r.transform.localScale=Vector3.one*.5f;}
        // Ornamental masonry on existing solid cells only: no new obstruction.
        int pillars=0;
        foreach(var p in oldWall.cellBounds.allPositionsWithin)
        {
            if(!oldWall.HasTile(p)||oldWall.HasTile(p+Vector3Int.down)||p.x%7!=0)continue;
            Prop("Architecture_Pilasters","Pilaster_"+p, Garden(50),p.x+.5f,p.y+.25f,-7); pillars++;
        }
        EditorSceneManager.MarkSceneDirty(scene); AssetDatabase.SaveAssets();
        return "Walls filled="+cells+"; pilasters="+pillars+"; visual objects="+go.GetComponentsInChildren<SpriteRenderer>().Length+"; original colliders untouched";
    }
}
