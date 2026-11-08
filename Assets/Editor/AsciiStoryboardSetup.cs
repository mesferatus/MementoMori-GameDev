using System;
using System.Linq;
using MementoMori.Dialogue;
using UnityEditor;
using UnityEngine;

/// <summary>Original, editable ASCII blocking studies. Artistic approval remains separate.</summary>
public static class AsciiStoryboardSetup
{
    private const int Width = 84, Height = 34;
    [MenuItem("Memento Mori/Cinematics/Populate ASCII blocking studies")]
    public static void Build()
    {
        foreach (var id in new[] { "C01", "C02", "C03", "L01", "C_POE", "A1", "L08", "M02_WATER", "C_BLACK", "A2", "M07", "FINAL" })
        {
            var sequence = AssetDatabase.LoadAssetAtPath<IllustratedSequence>("Assets/Personagens/Resources/Cinematics/" + id + ".asset");
            if (sequence == null) throw new InvalidOperationException("Missing sequence: " + id);
            sequence.pixelFont ??= AssetDatabase.LoadAssetAtPath<Font>("Assets/Personagens/Resources/Cinematics/Silkscreen-Regular.ttf");
            for (var beat = 0; beat < sequence.beats.Length; beat++)
            {
                var value = sequence.beats[beat];
                if (value.asciiFrames != null && value.asciiFrames.Length > 0) continue;
                value.asciiFrames = Enumerable.Range(0, 6).Select(frame => Compose(id, beat, frame)).ToArray();
                value.duration = Mathf.Max(1.8f, value.duration);
            }
            sequence.finalArtwork = false;
            EditorUtility.SetDirty(sequence);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Memento Mori/Cinematics/Add approved M07 choice reaction")]
    public static void BuildPoeChoiceVariants()
    {
        var sequence = AssetDatabase.LoadAssetAtPath<IllustratedSequence>("Assets/Personagens/Resources/Cinematics/M07.asset");
        if (sequence == null || sequence.finalArtwork)
            throw new InvalidOperationException("M07 must still be the provisional study.");
        for (var beat = 0; beat < sequence.beats.Length; beat++)
            if (!sequence.beats[beat].asciiFrames.SequenceEqual(Enumerable.Range(0, 6).Select(frame => Compose("M07", beat, frame))))
                throw new InvalidOperationException("M07 artwork changed; preserve it and author variants explicitly.");
        for (var beat = 0; beat < sequence.beats.Length; beat++)
            sequence.beats[beat].choiceVariants = new[]
            {
                new IllustratedFrameVariant { choiceId = "CH_POE", optionId = "approach",
                    asciiFrames = Enumerable.Range(0, 6).Select(frame => Compose("M07", beat, frame, true)).ToArray() },
                new IllustratedFrameVariant { choiceId = "CH_POE", optionId = "observe",
                    asciiFrames = (string[])sequence.beats[beat].asciiFrames.Clone() }
            };
        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();
    }

    private static string Compose(string id, int beat, int frame, bool poeApproaches = false)
    {
        var board = Enumerable.Range(0, Height).Select(_ => new string(' ', Width).ToCharArray()).ToArray();
        void Put(int x, int y, string art)
        {
            var rows = art.Replace("\r", "").Split('\n');
            for (var row = 0; row < rows.Length; row++)
                for (var col = 0; col < rows[row].Length; col++)
                    if (x + col >= 0 && x + col < Width && y + row >= 0 && y + row < Height)
                        board[y + row][x + col] = rows[row][col];
        }
        var room = id == "C01" || id == "C02" || id == "C03" || id == "FINAL";
        var lunar = id == "M02_WATER" || id == "C_BLACK" || id == "A2" || id == "M07";
        if (room)
        {
            Put(6, 2, @".-----------------.
|        |        |
|   .    |    *   |
|--------+--------|
|        |        |
|     *  |        |
'-----------------'");
            Put(59, 23, @"  .--------------.
  | .----.       |
 _|_|____|_______|_
|__________________|
  |              |");
        }
        else if (id == "M02_WATER")
        {
            Put(1, 26, @"____..____                          ____..____
          \________________________/
      ~       ~~~          ~~~         ~
   ~~~~~            ~~~~        ~~~~~
         ~~~~              ~~~~");
            Put(60, 22, frame % 2 == 0 ? "     (.)\n   --(+)--\n      |\n   ~~~~~~~" : "     (.)\n  ---(+)---\n      |\n  ~~~~~~~~~");
        }
        else if (id == "C_BLACK")
        {
            Put(55, 2, @"       .--------.
    .-'          '-.
   /                \
  |                  |
  |                  |
  |                  |
  |                  |
  |                  |
  |                  |
  |                  |
  |                  |
  |                  |
   \                /
    '--------------'");
            Put(60, 6, frame < 3 ? "  .----.\n /||||||\\\n | -  - |\n |  __  |\n  \\____/" : "  .----.\n /||||||\\\n | o  o |\n |  __  |\n  \\____/");
        }
        else if (id == "L08")
        {
            Put(52, 2, @"      .----------.
   .-'  *  o  *   '-.
  /                  \
 |  .-------------.  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |  |             |  |
 |__|_____________|__|");
            if (beat > 0 || frame > 2) Put(60, 7, " .   *   .\n    . .\n  *  |  *\n    / \\\n . /   \\ .");
        }
        else
        {
            Put(2, 2, @"|____|____|____|____|
|__|____|____|____|_|
|____|____|____|____|");
            Put(57, 24, @"      _________
     /   / \   \
    /___/___\___\
    |     |     |
    |_____|_____|");
        }
        var eyes = frame == 2 ? "-  -" : (beat > 0 && frame >= 3 ? "o  o" : ".  .");
        Put(29, 5, @"       .--------.
     .'  ||||||  '.
    /   ||||||||   \
   /|  /||||||||\  |\
  <o| |          | |o>
   || |          | ||
   || |    __    | ||
   ||  \________/  ||
   ||    /____\    ||
   ||   /\    /\   ||
   ||  /  \__/  \  ||
   || /    ||    \ ||
   ||/____/||\____\||
   |/     /||\     \|
   /     / || \     \
  /     /  ||  \     \
 /_____/___||___\_____\
   \_/\_/\_/\_/\_/\_/
       |__|  |__|
       |  |  |  |
      _|__|  |__|_
     (____|  |____)");
        Put(37, 10, eyes);
        if (lunar) { Put(38, 15, "(c)"); Put(37, 22, "* c *"); }
        if (id == "C02" && beat == 0 && frame < 3) Put(37, 10, "-  -");
        if (id == "C03" && beat > 0) Put(37, 10, "-  -");
        // Hand and forearm change shape across authored poses, independently of framing.
        if (frame >= 1 && frame <= 4)
            Put(48, 15 - frame / 2, frame < 3 ? "\\__\n   \\__.." : "  __..\n_/");
        if (id == "C_POE" || id == "A1" || id == "L08" || lunar)
        {
            var poeX = 16 + (poeApproaches ? frame : 0);
            Put(poeX, 21, frame == 2 ? @" /\_/\
( -.- )
 /   \
(|   |)__
(_|_|_)  )" : @" /\_/\
( o.o )
 /   \
(|   |)__
(_|_|_)  )");
            if (poeApproaches && frame > 0 && frame < 5)
                Put(poeX, 25, frame % 2 == 0 ? "(_/ \\_)" : "(_\\ /_)");
            if (lunar) Put(poeX + 2, 24, "(c)");
            if (id == "C_POE" && frame == 0 && beat == 0) Put(17, 22, " ... ");
        }
        if (id == "A1" || id == "A2")
            Put(59, 8, frame % 3 == 1 ? @"    __
 __/o >
/ /  /\
 /__/  \
  /\" : @"      __
     /o >
 ___/  /
 \    /____
  \__/-----'
   /\");
        if (id == "C01" || id == "C03" || id == "L01" || id == "M07")
        {
            Put(5, 26, @"    .-------.
 .-'   /\    '-.
(-----/--\------)
 '-. /____\ .-'
    '-------'");
            Put(10, 21, frame % 2 == 0 ? "  )\n ( )\n | |\n |_|" : " (\n  )\n | |\n |_|");
        }
        if (id == "M07") Put(61, 18, frame < 3 ? "  .\n . ." : "  /\\\n <  >\n  \\/");
        if (id == "FINAL" && beat > 0) Put(17, 6, "o");
        return string.Join("\n", board.Select(row => new string(row)));
    }
}
