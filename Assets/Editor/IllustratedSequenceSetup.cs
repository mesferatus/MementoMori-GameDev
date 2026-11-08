using System;
using System.Linq;
using MementoMori.Dialogue;
using UnityEditor;
using UnityEngine;

/// <summary>Builds editable cinematic assets from existing character frames; never alters scene geometry.</summary>
public static class IllustratedSequenceSetup
{
    private const string Root = "Assets/Personagens/Resources/Cinematics";

    [MenuItem("Memento Mori/Cinematics/Build illustrated sequence assets")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/Personagens/Resources", "Cinematics");
        var materialPath = Root + "/LilacLineArt.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            var shader = Shader.Find("MementoMori/IllustratedLineArt");
            if (shader == null) throw new InvalidOperationException("Illustrated line art shader missing.");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        var ritual = Original("melantha_ritual_cast_", 4);
        var wake = Original("melantha_wake_up_", 4);
        var startled = Original("melantha_startled_", 3);
        var examine = Original("melantha_examine_object_", 3);
        var sleep = Original("melantha_sleep_", 2);
        var poeSit = ClipFrames("Original_poe_sit_front");
        var poeMeow = ClipFrames("Original_poe_meow");
        var sigil = Resources.Load<Sprite>("UI/Grimoire/sigil_symbols");
        var lunarLook = Lunar("Melantha", "Cutscene", "r0_c", 0, 3);
        var lunarStartled = Lunar("Melantha", "Cutscene", "r0_c", 3, 3);
        var lunarPoe = Lunar("Poe", "Cutscene", "r0_c", 0, 3);
        var swim = Lunar("Melantha", "Swim", "r0_c", 0, 4);
        var bird = AssetDatabase.LoadAllAssetsAtPath("Assets/Personagens/Andrealphus/Sprites/Andrealphus_Bird.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();

        Save("C01", material, Beat(0, "Preparar gesto do ritual", ritual, null, sigil, "05_transition_sleep"),
            Beat(3, "Interromper gesto e reagir", startled, null, sigil, "10_sigil_error"));
        Save("C02", material, Beat(0, "Despertar", wake), Beat(2, "Observar o quarto", examine));
        Save("C03", material, Beat(0, "Hesitar antes de deitar", examine), Beat(1, "Deitar", sleep, null, sigil, "05_transition_sleep"));
        Save("L01", material, Beat(0, "Examinar marca; falta close-up ajoelhada", examine, null, sigil), Beat(3, "Reagir ao som distante", startled));
        Save("C_POE", material, Beat(0, "Reconhecer Poe", startled, poeMeow, null, "15_poe_soft_call"), Beat(2, "Poe senta; falta mao estendida em close", examine, poeSit));
        var a1 = Beat(0, "Manifestacao de Andrealphus; ilustracao especifica pendente", examine, poeSit);
        a1.andrealphusFrames = bird;
        Save("A1", material, a1);
        Save("L08", material, Beat(0, "Examinar selos", examine, poeSit, sigil), Beat(3, "Reagir a abertura", startled, poeMeow, sigil, "12_portal_open"));
        Save("M02_WATER", material, Beat(0, "Observar agua", lunarLook, lunarPoe), Beat(2, "Nadar; falta plano de entrada pela margem", swim, lunarPoe, null, "06_interaction_click"));
        Save("C_BLACK", material, Beat(0, "Observar reflexo", lunarLook, lunarPoe), Beat(2, "Reagir a memoria", lunarStartled, lunarPoe, sigil));
        var a2 = Beat(0, "Segunda manifestacao; ilustracao especifica pendente", lunarLook, lunarPoe);
        a2.andrealphusFrames = bird;
        Save("A2", material, a2);
        Save("M07", material, Beat(0, "Observar vestigios; close-ups pendentes", lunarLook, lunarPoe), Beat(2, "Reagir ao Fragmento", lunarStartled, lunarPoe, sigil));
        Save("FINAL", material, Beat(0, "Despertar com roupa original", wake), Beat(3, "Observar marca; quarto ilustrado pendente", examine, null, sigil));
        AssetDatabase.SaveAssets();
        Debug.Log("Created 12 illustrated sequence assets. All are functional studies, not final cinematic artwork.");
    }

    private static IllustratedBeat Beat(int line, string action, Sprite[] melantha, Sprite[] poe = null, Sprite prop = null, string sound = null)
        => new IllustratedBeat { beforeLine = line, action = action, melanthaFrames = melantha,
            poeFrames = poe ?? Array.Empty<Sprite>(), prop = prop, sound = sound,
            duration = Mathf.Max(.65f, melantha.Length / 6f), melanthaPosition = new Vector2(poe == null ? .43f : .35f, .49f) };

    private static void Save(string id, Material material, params IllustratedBeat[] beats)
    {
        var path = Root + "/" + id + ".asset";
        // Preserve any artist-authored changes on subsequent runs.
        if (AssetDatabase.LoadAssetAtPath<IllustratedSequence>(path) != null) return;
        var data = ScriptableObject.CreateInstance<IllustratedSequence>();
        data.eventId = id; data.finalArtwork = false; data.lineArtMaterial = material; data.beats = beats;
        AssetDatabase.CreateAsset(data, path);
    }

    private static Sprite[] Original(string prefix, int count)
        => Enumerable.Range(1, count).Select(i => RequiredSprite("Assets/Personagens/Melantha/Sprites/" + prefix + i.ToString("00") + ".png")).ToArray();

    private static Sprite RequiredSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("Required authored sprite missing: " + path);
        return sprite;
    }

    private static Sprite[] ClipFrames(string name)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Personagens/Resources/Costumes/" + name + ".anim");
        if (clip == null) throw new InvalidOperationException("Original costume clip missing: " + name);
        return AnimationUtility.GetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"))
            .Select(k => k.value).OfType<Sprite>().ToArray();
    }

    private static Sprite[] Lunar(string actor, string sheet, string prefix, int first, int count)
    {
        var name = actor + "_Lunar_" + sheet;
        var all = AssetDatabase.LoadAllAssetsAtPath("Assets/Personagens/" + actor + "/Lunar/" + name + ".png").OfType<Sprite>().ToArray();
        return Enumerable.Range(first, count).Select(i => all.Single(s => s.name == name + "_" + prefix + i)).ToArray();
    }
}
