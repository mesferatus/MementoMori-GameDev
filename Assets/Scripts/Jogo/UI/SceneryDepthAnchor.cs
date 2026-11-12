using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MementoMori.Player;
using MementoMori.Poe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
namespace MementoMori.UI
{
    /// <summary>Scenery uses its own ground contact, on the same depth scale as both actors.</summary>
    [DefaultExecutionOrder(200), DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class SceneryDepthAnchor : MonoBehaviour
    {
        Collider2D[] solids;
        SpriteRenderer visual;
        float localGroundY;
        public void Configure(Collider2D[] colliders)
        {
            visual = GetComponent<SpriteRenderer>(); solids = colliders;
            var ground = solids.Length > 0 ? solids.Min(c => c.bounds.min.y) : visual.bounds.min.y;
            localGroundY = transform.InverseTransformPoint(new Vector3(transform.position.x, ground, 0)).y;
            Apply();
        }
        void LateUpdate() => Apply();
        void Apply()
        {
            if(visual == null) return;
            var y = transform.TransformPoint(new Vector3(0,localGroundY,0)).y;
            // Room candle sprites change scale when lit; their configured collider keeps the actual ground contact stable.
            var found=false;
            if(solids!=null) foreach(var solid in solids)
            {
                if(solid==null || !solid.enabled || !solid.gameObject.activeInHierarchy) continue;
                y=found?Mathf.Min(y,solid.bounds.min.y):solid.bounds.min.y; found=true;
            }
            visual.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(y * 100f), -32000,32000);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }
        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if(scene.name != "Quarto" && scene.name != "Labirinto" && scene.name != "DominioLua") return;
            var host = new GameObject("SceneryDepthSetup");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<SceneryDepthSetup>();
        }
        internal sealed class SceneryDepthSetup : MonoBehaviour
        {
            IEnumerator Start()
            {
                yield return null; yield return null;
                var sprites = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                foreach(var sprite in sprites)
                {
                    if(sprite.gameObject.scene != gameObject.scene || sprite.sprite == null) continue;
                    if(sprite.GetComponentInParent<PlayerController>() != null || sprite.GetComponentInParent<PoeFollower>() != null) continue;
                    if(sprite.GetComponentInParent<UnityEngine.Rendering.SortingGroup>() != null) continue;
                    var n=sprite.name.ToLowerInvariant();
                    if(n.Contains("layer_") || n.Contains("piso") || n.Contains("floor") || n.Contains("tapete") || n.Contains("carpet") || n.Contains("flame") || n.Contains("glow")) continue;
                    var colliders=sprite.GetComponentsInChildren<Collider2D>(true).Where(c=>!c.isTrigger && !(c is TilemapCollider2D) && !(c is CompositeCollider2D)
                        && (c.attachedRigidbody==null || c.attachedRigidbody.bodyType!=RigidbodyType2D.Dynamic)).ToArray();
                    if(colliders.Length==0 && !n.Contains("fachada") && !n.Contains("facade")) continue;
                    var anchor=sprite.GetComponent<SceneryDepthAnchor>() ?? sprite.gameObject.AddComponent<SceneryDepthAnchor>();
                    anchor.Configure(colliders);
                }
                foreach(var flame in sprites)
                {
                    if(!flame.name.StartsWith("Flame_")) continue;
                    var owner=GameObject.Find(flame.name.Substring(6))?.GetComponent<SpriteRenderer>();
                    if(owner==null) continue;
                    if(owner.GetComponent<SceneryDepthAnchor>()==null) owner.gameObject.AddComponent<SceneryDepthAnchor>().Configure(new Collider2D[0]);
                    var link=flame.GetComponent<FlameDepthLink>() ?? flame.gameObject.AddComponent<FlameDepthLink>();
                    link.Configure(owner);
                }
                Destroy(this);
            }
        }
    }
    [DefaultExecutionOrder(300), RequireComponent(typeof(SpriteRenderer))]
    public sealed class FlameDepthLink : MonoBehaviour
    {
        SpriteRenderer owner,visual;
        public void Configure(SpriteRenderer holder){owner=holder;visual=GetComponent<SpriteRenderer>();}
        void LateUpdate(){if(owner==null||visual==null)return;visual.sortingLayerID=owner.sortingLayerID;visual.sortingOrder=owner.sortingOrder+1;}
    }
}
