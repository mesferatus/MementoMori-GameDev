using System.Collections;
using MementoMori.Audio;
using MementoMori.Core;
using TMPro;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Timed dream dissolve; never changes the camera projection or physical world.</summary>
    public sealed class DreamTransitionController : MonoBehaviour
    {
        public IEnumerator Play(float duration)
        {
            duration=Mathf.Clamp(duration,30f,50f);
            InputGate.Instance?.Block("DreamTransition");
            var overlay=new GameObject("Dream Dissolve",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
            overlay.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            overlay.GetComponent<Canvas>().sortingOrder=130;
            var scaler=overlay.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var veil=new GameObject("Veil",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            veil.transform.SetParent(overlay.transform,false);
            var rect=veil.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=veil.GetComponent<UnityEngine.UI.Image>();
            var line=new GameObject("Memory",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            line.transform.SetParent(veil.transform,false);line.rectTransform.anchorMin=new Vector2(.2f,.35f);line.rectTransform.anchorMax=new Vector2(.8f,.65f);
            line.rectTransform.offsetMin=line.rectTransform.offsetMax=Vector2.zero;
            line.font=TMP_Settings.defaultFontAsset;line.fontSize=34;line.alignment=TextAlignmentOptions.Center;
            var phrases=new[]{"Eu já tracei este caminho.","Há algo que o quarto não consegue guardar.","Um som. Um nome que não alcanço.","Se eu seguir... vou lembrar?"};
            try
            {
                RuntimeAudio.PlayOneShot("05_transition_sleep",.6f);
                var elapsed=0f;
                while(elapsed<duration)
                {
                    elapsed+=Time.unscaledDeltaTime;
                    var t=Mathf.Clamp01(elapsed/duration);
                    image.color=new Color(.015f,.008f,.035f,Mathf.SmoothStep(0,1,Mathf.Clamp01(t*4)));
                    var phase=t*phrases.Length;
                    line.text=phrases[Mathf.Min((int)phase,phrases.Length-1)];
                    var alpha=Mathf.Sin(Mathf.Clamp01(phase-Mathf.Floor(phase))*Mathf.PI);
                    line.color=new Color(.78f,.72f,.9f,alpha);
                    yield return null;
                }
            }
            finally
            {
                if(overlay!=null) Destroy(overlay);
                InputGate.Instance?.Release("DreamTransition");
            }
        }
    }
}
