using System;
using System.Collections;
using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// End of the game: the screen fades to black and the credits roll upward like a film ending.
    /// Holding Enter, Space or Confirm speeds the roll up; the end card appears when it finishes.
    /// </summary>
    public sealed class RollingCredits : MonoBehaviour
    {
        const float FadeTime = 2f, Speed = 70f, FastSpeed = 420f;

        TMP_FontAsset titleFont, bodyFont;
        Action finished;
        Image black;
        RectTransform roll;
        CanvasGroup rollGroup;

        public static void Play(TMP_FontAsset titleFont, TMP_FontAsset bodyFont, Action onFinished)
        {
            var go = new GameObject("RollingCredits", typeof(Canvas), typeof(CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            var credits = go.AddComponent<RollingCredits>();
            credits.titleFont = titleFont != null ? titleFont : TMP_Settings.defaultFontAsset;
            credits.bodyFont = bodyFont != null ? bodyFont : credits.titleFont;
            credits.finished = onFinished;
            credits.Build();
            credits.StartCoroutine(credits.Run());
        }

        void Build()
        {
            var bg = new GameObject("Escuro", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(transform, false);
            black = bg.GetComponent<Image>();
            black.color = new Color(0f, 0f, 0f, 0f);
            Stretch((RectTransform)bg.transform);

            var holder = new GameObject("Rolagem", typeof(RectTransform), typeof(CanvasGroup));
            holder.transform.SetParent(transform, false);
            roll = (RectTransform)holder.transform;
            roll.anchorMin = roll.anchorMax = new Vector2(.5f, 0f);
            roll.pivot = new Vector2(.5f, 1f);
            roll.sizeDelta = new Vector2(1400f, 10f);
            rollGroup = holder.GetComponent<CanvasGroup>();
            rollGroup.alpha = 0f;

            var y = 0f;
            y = Line("MEMENTO MORI", titleFont, 84f, new Color(.9f, .84f, 1f), 10f, y, 140f);
            y = Line(Loc.T("Um jogo sobre memória, perda e travessia"), bodyFont, 30f, Caption, 2f, y, 220f);
            y = Section(Loc.T("ARTE, ORGANIZAÇÃO VISUAL E COLLIDERS"), "Luiza Canova", y);
            y = Section(Loc.T("PROGRAMAÇÃO, SHADERS E ÁUDIO"), "Callisto Monteiro Mascoloti", y);
            y = Section(Loc.T("ILUSTRAÇÃO DO MENU"), "Camila David Ribeiro", y);
            y = Section(Loc.T("PERSONAGENS"), "Melantha\nPoe\nAndrealphus", y);
            y = Section(Loc.T("FONTES"), "Cinzel · Cormorant Garamond · UnifrakturMaguntia\n<size=24>SIL Open Font License</size>", y);
            y = Section(Loc.T("FEITO COM"), "Unity", y);
            y -= 260f;
            y = Line(Loc.T("O que a gente lembra volta com a gente."), titleFont, 44f, new Color(.9f, .84f, 1f), 3f, y, 90f);
            roll.sizeDelta = new Vector2(1400f, -y);
        }

        static readonly Color Caption = new Color(.73f, .64f, .84f);

        float Section(string role, string names, float y)
        {
            y = Line(role, bodyFont, 26f, Caption, 6f, y, 50f);
            var lines = names.Split('\n').Length;
            return Line(names, titleFont, 48f, Color.white, 2f, y, 64f * lines + 150f);
        }

        float Line(string text, TMP_FontAsset font, float size, Color color, float spacing, float y, float advance)
        {
            var go = new GameObject("Linha", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(roll, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = font; t.fontSize = size; t.color = color; t.characterSpacing = spacing;
            t.alignment = TextAlignmentOptions.Top; t.text = text; t.raycastTarget = false;
            var r = t.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, 1f);
            r.sizeDelta = new Vector2(1400f, advance);
            r.anchoredPosition = new Vector2(0f, y);
            return y - advance;
        }

        static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        IEnumerator Run()
        {
            for (var t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
            {
                black.color = new Color(0f, 0f, 0f, t / FadeTime);
                yield return null;
            }
            black.color = Color.black;
            rollGroup.alpha = 1f;
            var height = roll.sizeDelta.y;
            var screen = ((RectTransform)transform).rect.height;
            var pos = 0f;                          // top of the roll starts at the bottom edge
            var end = height + screen * .5f;       // last line rests near the middle
            while (pos < end)
            {
                var fast = Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)
                    || GamepadInput.Held(GameAction.Interact);
                pos += (fast ? FastSpeed : Speed) * Time.unscaledDeltaTime;
                roll.anchoredPosition = new Vector2(0f, Mathf.Min(pos, end));
                yield return null;
            }
            yield return new WaitForSecondsRealtime(2.5f);
            for (var t = 0f; t < 1f; t += Time.unscaledDeltaTime) { rollGroup.alpha = 1f - t; yield return null; }
            rollGroup.alpha = 0f;
            finished?.Invoke();
            for (var t = 0f; t < 1.5f; t += Time.unscaledDeltaTime)
            {
                black.color = new Color(0f, 0f, 0f, 1f - t / 1.5f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
