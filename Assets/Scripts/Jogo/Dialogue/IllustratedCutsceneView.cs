using System.Collections;
using MementoMori.Audio;
using MementoMori.Core;
using UnityEngine;

namespace MementoMori.Dialogue
{
    /// <summary>Illustrated presentation; narrative completion remains owned by the existing event caller.</summary>
    public sealed class IllustratedCutsceneView : MonoBehaviour
    {
        private IllustratedSequence sequence;
        private UnityEngine.UI.Image melantha, poe, andrealphus, prop;
        private UnityEngine.UI.Text speaker, body, controls;
        private AsciiIllustration ascii;
        public bool Acting { get; private set; }
        public bool CanSkip => sequence != null && sequence.allowSkip;
        public string EventId => sequence == null ? null : sequence.eventId;
        private static readonly Color Lilac = new(.65f, .57f, 1f, 1f);

        public static IllustratedCutsceneView Create(string id)
        {
            var data = Resources.Load<IllustratedSequence>("Cinematics/" + id);
            if (data == null || data.beats == null || data.beats.Length == 0) return null;
            var root = new GameObject("IllustratedCutscene_" + id, typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var view = root.AddComponent<IllustratedCutsceneView>();
            view.sequence = data;
            view.Build();
            return view;
        }

        private void Build()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1150;
            var scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 640);
            scaler.matchWidthOrHeight = .5f;
            Image("Background", transform, Vector2.zero, Vector2.one, Color.black);
            var border = Image("LilacFrame", transform, new Vector2(.04f, .27f), new Vector2(.96f, .96f), Lilac);
            var inside = Image("Illustration", border.transform, Vector2.zero, Vector2.one, Color.black);
            inside.rectTransform.offsetMin = Vector2.one * 2f;
            inside.rectTransform.offsetMax = Vector2.one * -2f;
            inside.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var asciiObject = new GameObject("AsciiStage", typeof(RectTransform), typeof(AsciiIllustration));
            asciiObject.transform.SetParent(inside.transform, false);
            var asciiRect = (RectTransform)asciiObject.transform;
            asciiRect.anchorMin = Vector2.zero; asciiRect.anchorMax = Vector2.one;
            asciiRect.offsetMin = Vector2.one * 2f; asciiRect.offsetMax = Vector2.one * -2f;
            ascii = asciiObject.GetComponent<AsciiIllustration>();
            ascii.color = Lilac;
            ascii.Configure(sequence.pixelFont != null ? sequence.pixelFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            prop = Actor("Prop", inside.transform);
            melantha = Actor("Melantha", inside.transform);
            poe = Actor("Poe", inside.transform);
            andrealphus = Actor("Andrealphus", inside.transform);
            // No full-screen gothic frame here: the lilac border already frames the illustration.
            speaker = Label("Speaker", new Vector2(.08f, .215f), new Vector2(.92f, .255f), 18);
            body = Label("Dialogue", new Vector2(.08f, .1f), new Vector2(.92f, .215f), 20);
            controls = Label("Controls", new Vector2(.08f, .062f), new Vector2(.92f, .098f), 14);
            controls.text = InputBindings.Get(GameAction.Interact) + " / Enter — avançar" + (CanSkip ? "    Esc — pular sequência" : "");
        }

        private UnityEngine.UI.Image Actor(string name, Transform parent)
        {
            var image = Image(name, parent, Vector2.zero, Vector2.one, Color.white);
            image.material = sequence.lineArtMaterial;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = false;
            return image;
        }

        private static UnityEngine.UI.Image Image(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.anchorMin = min; image.rectTransform.anchorMax = max;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            image.color = color;
            return image;
        }

        private UnityEngine.UI.Text Label(string name, Vector2 min, Vector2 max, int size)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text)).GetComponent<UnityEngine.UI.Text>();
            text.transform.SetParent(transform, false);
            text.rectTransform.anchorMin = min; text.rectTransform.anchorMax = max;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            text.font = sequence.pixelFont != null ? sequence.pixelFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.color = Lilac;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        public void SetSpeaker(string value) { if (speaker != null) speaker.text = value; }
        public void SetBody(string value) { if (body != null) body.text = value; }

        public IEnumerator PerformBeforeLine(int index)
        {
            foreach (var beat in sequence.beats)
            {
                if (beat.beforeLine != index) continue;
                Acting = true;
                SetBody(string.Empty);
                if (!string.IsNullOrEmpty(beat.sound)) RuntimeAudio.PlayOneShot(beat.sound, .4f);
                // ASCII is the requested presentation. Missing frames remain an explicit art dependency.
                melantha.enabled = poe.enabled = andrealphus.enabled = prop.enabled = false;
                var frames = beat.FramesFor(GameState.Instance);
                if (frames != null && frames.Length > 0)
                {
                    var frameDuration = Mathf.Max(.1f, beat.duration / frames.Length);
                    foreach (var frame in frames)
                    {
                        ascii.Show(frame);
                        yield return new WaitForSecondsRealtime(frameDuration);
                    }
                }
                Acting = false;
            }
        }

        private static void Pose(UnityEngine.UI.Image actor, Sprite[] frames, float t, Vector2 position, float height)
        {
            if (frames == null || frames.Length == 0) { actor.enabled = false; return; }
            actor.enabled = true;
            actor.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(t * frames.Length))];
            var rect = actor.rectTransform;
            rect.anchorMin = rect.anchorMax = position;
            rect.pivot = new Vector2(.5f, .5f);
            var parent = (RectTransform)rect.parent;
            var pixels = parent.rect.height * height;
            rect.sizeDelta = new Vector2(pixels, pixels);
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
