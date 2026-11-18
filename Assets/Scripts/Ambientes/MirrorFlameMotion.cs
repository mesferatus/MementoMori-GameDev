using MementoMori.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.V65
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class MirrorFlameMotion : MonoBehaviour
    {
        SpriteRenderer visual;
        MaterialPropertyBlock block;
        float seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "DominioLua") return;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var state in root.GetComponentsInChildren<V65StateVisual>(true))
            {
                if (state.mirror == null || state.GetComponent<MirrorFlameMotion>() != null) continue;
                var renderer = state.GetComponent<SpriteRenderer>();
                if (renderer == null || renderer.sharedMaterial == null
                    || !renderer.sharedMaterial.HasProperty("_FlameMotion")) continue;
                state.gameObject.AddComponent<MirrorFlameMotion>();
            }
        }
        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
            seed = transform.position.x * 1.37f + transform.position.y * .73f;
        }
        void LateUpdate()
        {
            if (visual == null) return;
            visual.GetPropertyBlock(block);
            block.SetFloat("_FlameMotion", 1f);
            block.SetFloat("_FlameSeed", seed);
            block.SetFloat("_FireAmplitude", AccessibilitySettings.Instance != null
                && AccessibilitySettings.Instance.ReduceFlashes ? 0f : 1f);
            visual.SetPropertyBlock(block);
        }
    }
}
