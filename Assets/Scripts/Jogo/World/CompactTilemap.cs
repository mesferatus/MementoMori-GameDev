using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MementoMori.World
{
    /// <summary>
    /// Stores a single-tile tilemap (the generated NavigationFloor) as run-length rows in a .bytes asset
    /// instead of ~250k serialized cells in the scene, and rebuilds it before anything reads it. The data is
    /// identical; only the storage changes, which keeps the map scenes far below GitHub's 100 MiB file cap.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    [RequireComponent(typeof(Tilemap))]
    public sealed class CompactTilemap : MonoBehaviour
    {
        [SerializeField] private TextAsset cells;
        [SerializeField] private TileBase tile;
        [SerializeField] private BoundsInt area;

        public TextAsset Cells => cells;

        private void Awake() => Restore();

        /// <summary>Fills the tilemap from the compact data when it is empty. Safe to call more than once.</summary>
        public void Restore()
        {
            var map = GetComponent<Tilemap>();
            if (map == null || cells == null || tile == null || map.GetUsedTilesCount() > 0) return;
            var runs = Decode(cells.bytes);
            var size = area.size.x * area.size.y;
            var block = new TileBase[size];
            var index = 0; var filled = false;
            foreach (var run in runs)
            {
                if (filled) for (var i = 0; i < run && index + i < size; i++) block[index + i] = tile;
                index += run; filled = !filled;
            }
            map.SetTilesBlock(new BoundsInt(area.xMin, area.yMin, area.zMin, area.size.x, area.size.y, 1), block);
        }

        /// <summary>Row-major runs, starting with an empty run: empty, filled, empty, ... as varints.</summary>
        public static byte[] Encode(Tilemap map, out BoundsInt bounds, out TileBase onlyTile)
        {
            map.CompressBounds();
            bounds = map.cellBounds;
            onlyTile = null;
            var all = map.GetTilesBlock(new BoundsInt(bounds.xMin, bounds.yMin, bounds.zMin, bounds.size.x, bounds.size.y, 1));
            var output = new List<byte>();
            var filled = false; var run = 0;
            foreach (var cell in all)
            {
                if (cell != null)
                {
                    if (onlyTile == null) onlyTile = cell;
                    else if (cell != onlyTile) throw new InvalidOperationException(map.name + " uses more than one tile; it cannot be compacted.");
                }
                if ((cell != null) == filled) { run++; continue; }
                WriteVarint(output, run); filled = !filled; run = 1;
            }
            WriteVarint(output, run);
            return output.ToArray();
        }

        public void Configure(TextAsset data, TileBase onlyTile, BoundsInt bounds) { cells = data; tile = onlyTile; area = bounds; }

        private static void WriteVarint(List<byte> output, int value)
        {
            var v = (uint)value;
            while (v >= 0x80) { output.Add((byte)(v | 0x80)); v >>= 7; }
            output.Add((byte)v);
        }

        private static List<int> Decode(byte[] data)
        {
            var runs = new List<int>();
            var shift = 0; uint value = 0;
            foreach (var b in data)
            {
                value |= (uint)(b & 0x7F) << shift;
                if ((b & 0x80) != 0) { shift += 7; continue; }
                runs.Add((int)value); value = 0; shift = 0;
            }
            return runs;
        }
    }
}
