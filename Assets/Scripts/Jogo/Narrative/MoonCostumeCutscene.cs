using System;
using System.Collections;
using System.Collections.Generic;
using MementoMori.Audio;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Player;
using MementoMori.Poe;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.Narrative
{
    /// <summary>
    /// M01 "A luz chega fora de hora" (trigger: first entry into the Moon Domain). The script asks for a short
    /// cutscene that equips the lunar costumes before the lines ("Câmera mostra reflexo antes do brilho");
    /// until now the costumes were simply already changed. Melantha and Poe arrive in their original clothes,
    /// moonlight gathers around them, Poe dissolves and reassembles, and both stand in the approved lunar
    /// costumes when M01's lines begin. Plays once: state effects moon_costume_melantha / moon_costume_poe.
    /// </summary>
    public static class MoonCostumeCutscene
    {
        public const string Counter = "cutscene.moon_costume";

        public static bool ShouldPlay(GameState state) =>
            state != null && state.GetCounter(Counter) == 0 && state.GetCounter("event.M01") == 0;

        public static IEnumerator Run(Action done)
        {
            var state = GameState.Instance;
            // Dress them in the old clothes before the first frame is seen; the light changes them later.
            var early = UnityEngine.Object.FindAnyObjectByType<PlayerController>()?.GetComponentInChildren<Animator>();
            var earlyLunar = early == null ? null : early.runtimeAnimatorController;
            var earlyOriginal = Resources.Load<RuntimeAnimatorController>("Costumes/MelanthaOriginal");
            if (early != null && earlyOriginal != null) early.runtimeAnimatorController = earlyOriginal;
            var earlyPoe = UnityEngine.Object.FindAnyObjectByType<PoeFollower>(FindObjectsInactive.Include)?.GetComponentInChildren<Animator>(true);
            var earlyPoeLunar = earlyPoe == null ? null : earlyPoe.runtimeAnimatorController;
            var earlyPoeOriginal = Resources.Load<RuntimeAnimatorController>("Costumes/PoeOriginal");
            if (earlyPoe != null && earlyPoeOriginal != null) earlyPoe.runtimeAnimatorController = earlyPoeOriginal;
            // The player and Poe set their lunar controllers in their own Start: keep the old clothes on while
            // the scene settles, remembering the lunar controller they bring.
            var settled = 0f;
            while (settled < .6f || SceneLoader.Instance != null && SceneLoader.Instance.IsLoading
                   || DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            {
                if (early != null && earlyOriginal != null && early.runtimeAnimatorController != earlyOriginal)
                { earlyLunar = early.runtimeAnimatorController; early.runtimeAnimatorController = earlyOriginal; }
                if (earlyPoe != null && earlyPoeOriginal != null && earlyPoe.runtimeAnimatorController != earlyPoeOriginal)
                { earlyPoeLunar = earlyPoe.runtimeAnimatorController; earlyPoe.runtimeAnimatorController = earlyPoeOriginal; }
                if (SceneLoader.Instance == null || !SceneLoader.Instance.IsLoading) settled += Time.deltaTime;
                yield return null;
            }

            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var melantha = player == null ? null : player.GetComponentInChildren<Animator>();
            var data = Resources.Load<MoonCostumeCutsceneData>("MoonCostumeCutscene");
            if (melantha == null || data == null || data.melanthaGlow == null)
            {
                if (early != null && earlyLunar != null) early.runtimeAnimatorController = earlyLunar;
                if (earlyPoe != null && earlyPoeLunar != null) earlyPoe.runtimeAnimatorController = earlyPoeLunar;
                state?.SetCounter(Counter, 1); done?.Invoke(); yield break;
            }
            InputGate.Instance?.Block("MoonCostume");

            var lunarMelantha = melantha == early && earlyLunar != null ? earlyLunar : melantha.runtimeAnimatorController;
            var originalMelantha = earlyOriginal;
            var poe = UnityEngine.Object.FindAnyObjectByType<PoeFollower>();
            var withPoe = poe != null && poe.isActiveAndEnabled && state != null && state.PoeRevealed;
            var poeAnimator = withPoe ? poe.GetComponentInChildren<Animator>() : null;
            var lunarPoe = poeAnimator == null ? null : poeAnimator == earlyPoe && earlyPoeLunar != null ? earlyPoeLunar : poeAnimator.runtimeAnimatorController;
            var originalPoe = earlyPoeOriginal;
            if (withPoe) poe.RestoreBesidePlayer();

            // Hand the visuals to the cutscene: no idle-life poses or companion steering while it plays.
            var paused = new List<Behaviour>();
            void Pause(Behaviour b) { if (b != null && b.enabled) { b.enabled = false; paused.Add(b); } }
            foreach (var b in player.GetComponentsInChildren<Behaviour>())
                if (b.GetType().Name == "MelanthaIdleLife") Pause(b);
            Pause(melantha);
            if (withPoe) { Pause(poe); Pause(poeAnimator); }

            var originalIdle = Clip(originalMelantha, "idle_front");
            var lunarIdle = Clip(lunarMelantha, "idle_front");
            var poeIdleOriginal = Clip(originalPoe, "sit_front");
            var poeDissolveOriginal = Clip(originalPoe, "dissolve");
            var poeIdleLunar = Clip(lunarPoe, "sit_front");
            var poeDissolveLunar = Clip(lunarPoe, "dissolve");
            var poeVisual = poeAnimator == null ? null : poeAnimator.GetComponent<SpriteRenderer>();

            var melanthaSprite = melantha.GetComponent<SpriteRenderer>();
            var glow = MakeGlow(melantha.transform, melanthaSprite);
            var flash = MakeFlash();
            var sparks = new List<(SpriteRenderer r, Vector3 v, float born)>();

            float t = 0f;
            // 1. Arrival, still in the old clothes: the camera rests on her before the light (≈1.3 s).
            RuntimeAudio.PlayOneShot("11_mirror_shimmer", .35f);
            while (t < 1.3f)
            {
                Sample(originalIdle, melantha, t);
                if (withPoe) Sample(poeIdleOriginal, poeAnimator, t);
                t += Time.deltaTime; yield return null;
            }
            // 2. Moonlight gathers; Poe comes apart into motes (≈1.3 s).
            float rise = 0f;
            while (rise < 1.3f)
            {
                var k = rise / 1.3f;
                Sample(originalIdle, melantha, t);
                if (withPoe) Sample(poeDissolveOriginal ?? poeIdleOriginal, poeAnimator, Mathf.Min(k * 1.15f, .999f) * Length(poeDissolveOriginal));
                SetGlow(glow, k);
                flash.color = new Color(.93f, .9f, 1f, Mathf.SmoothStep(0f, .85f, Mathf.InverseLerp(.55f, 1f, k)));
                if (UnityEngine.Random.value < .5f) sparks.Add(Spark(melantha.transform.position, melanthaSprite));
                Advance(sparks);
                rise += Time.deltaTime; t += Time.deltaTime; yield return null;
            }
            // 3. At the brightest moment the costumes change.
            melantha.runtimeAnimatorController = lunarMelantha;
            if (poeAnimator != null) poeAnimator.runtimeAnimatorController = lunarPoe;
            RuntimeAudio.PlayOneShot("14_poe_appear", .45f);
            float cast = 0f, castLength = 1.5f;
            while (cast < castLength)
            {
                var k = cast / castLength;
                // The clip ends in a collapse pose (r1_c4); the transformation holds on the moon sigil frame (r1_c2, t≈0.33 s).
                Sample(data.melanthaGlow, melantha, Mathf.Min(cast, .45f));
                if (withPoe) Sample(poeDissolveLunar ?? poeIdleLunar, poeAnimator, Mathf.Max(0f, 1f - k * 1.25f) * Length(poeDissolveLunar));
                SetGlow(glow, 1f - k * .6f);
                flash.color = new Color(.93f, .9f, 1f, Mathf.Lerp(.85f, 0f, Mathf.SmoothStep(0f, 1f, k * 1.6f)));
                if (UnityEngine.Random.value < .25f) sparks.Add(Spark(melantha.transform.position, melanthaSprite));
                Advance(sparks);
                cast += Time.deltaTime; yield return null;
            }
            // 4. The light settles; Poe looks at the new clothes (M01_D004 follows in the dialogue).
            RuntimeAudio.PlayOneShot("31_violin_resolve", .3f);
            float settle = 0f;
            while (settle < .9f)
            {
                Sample(lunarIdle, melantha, settle);
                if (withPoe) Sample(poeIdleLunar, poeAnimator, settle);
                SetGlow(glow, .4f * (1f - settle / .9f));
                flash.color = Color.clear;
                Advance(sparks);
                settle += Time.deltaTime; yield return null;
            }

            foreach (var s in sparks) if (s.r != null) UnityEngine.Object.Destroy(s.r.gameObject);
            UnityEngine.Object.Destroy(glow.gameObject);
            UnityEngine.Object.Destroy(flash.canvas.gameObject);
            foreach (var b in paused) if (b != null) b.enabled = true;
            if (poeVisual != null) poeVisual.enabled = true;
            if (earlyPoe != null && earlyPoeLunar != null) earlyPoe.runtimeAnimatorController = earlyPoeLunar;
            melantha.runtimeAnimatorController = lunarMelantha;
            state?.SetCounter(Counter, 1);
            InputGate.Instance?.Release("MoonCostume");
            done?.Invoke();
        }

        static AnimationClip Clip(RuntimeAnimatorController controller, string contains)
        {
            if (controller == null) return null;
            foreach (var clip in controller.animationClips) if (clip != null && clip.name.Contains(contains)) return clip;
            return null;
        }

        static float Length(AnimationClip clip) => clip == null ? 0f : clip.length;

        static void Sample(AnimationClip clip, Animator target, float time)
        {
            if (clip == null || target == null) return;
            var length = Mathf.Max(.001f, clip.length);
            clip.SampleAnimation(target.gameObject, clip.isLooping ? Mathf.Repeat(time, length) : Mathf.Clamp(time, 0f, length - .001f));
        }

        static Sprite soft, dot;

        static SpriteRenderer MakeGlow(Transform around, SpriteRenderer actor)
        {
            if (soft == null) soft = Radial(64, 2.2f, 1f);
            var go = new GameObject("MoonCostumeGlow", typeof(SpriteRenderer));
            go.transform.SetParent(around, false);
            go.transform.localPosition = new Vector3(0f, .55f, 0f);
            var r = go.GetComponent<SpriteRenderer>();
            r.sprite = soft; r.sortingLayerID = actor.sortingLayerID; r.sortingOrder = actor.sortingOrder - 1;
            r.color = new Color(.78f, .74f, 1f, 0f);
            return r;
        }

        static void SetGlow(SpriteRenderer glow, float k)
        {
            k = Mathf.Clamp01(k);
            glow.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 3.4f, k);
            glow.color = new Color(.78f, .74f, 1f, .75f * k);
            var owner = glow.transform.parent != null ? glow.transform.parent.GetComponent<SpriteRenderer>() : null;
            if (owner != null) { glow.sortingLayerID = owner.sortingLayerID; glow.sortingOrder = owner.sortingOrder - 1; }
        }

        static Image MakeFlash()
        {
            var go = new GameObject("MoonCostumeFlash", typeof(Canvas), typeof(Image));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 70;
            var image = go.GetComponent<Image>();
            image.color = Color.clear; image.raycastTarget = false;
            var rect = image.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return image;
        }

        static (SpriteRenderer, Vector3, float) Spark(Vector3 at, SpriteRenderer actor)
        {
            if (dot == null) dot = Radial(6, 1f, .12f);
            var go = new GameObject("MoonMote", typeof(SpriteRenderer));
            go.transform.position = at + new Vector3(UnityEngine.Random.Range(-.6f, .6f), UnityEngine.Random.Range(-.1f, .9f), 0f);
            go.transform.localScale = Vector3.one * UnityEngine.Random.Range(.6f, 1.2f);
            var r = go.GetComponent<SpriteRenderer>();
            r.sprite = dot; r.sortingLayerID = actor.sortingLayerID; r.sortingOrder = actor.sortingOrder + 2;
            r.color = new Color(.86f, .84f, 1f, .9f);
            return (r, new Vector3(UnityEngine.Random.Range(-.15f, .15f), UnityEngine.Random.Range(.5f, 1.1f), 0f), Time.time);
        }

        static void Advance(List<(SpriteRenderer r, Vector3 v, float born)> sparks)
        {
            for (var i = sparks.Count - 1; i >= 0; i--)
            {
                var (r, v, born) = sparks[i];
                if (r == null) { sparks.RemoveAt(i); continue; }
                var age = Time.time - born;
                if (age > 1.4f) { UnityEngine.Object.Destroy(r.gameObject); sparks.RemoveAt(i); continue; }
                r.transform.position += v * Time.deltaTime;
                var c = r.color; c.a = .9f * (1f - age / 1.4f); r.color = c;
            }
        }

        // Pixel-art friendly soft disc: point-filtered, so it reads as a lit halo rather than a blur.
        static Sprite Radial(int size, float falloff, float worldSize)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x + .5f) / size * 2f - 1f; var dy = (y + .5f) / size * 2f - 1f;
                var d = Mathf.Sqrt(dx * dx + dy * dy);
                var a = d >= 1f ? 0f : Mathf.Pow(1f - d, falloff);
                px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(255f * a));
            }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size / worldSize);
        }
    }
}
