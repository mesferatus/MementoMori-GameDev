using System.Collections;
using MementoMori.Audio;
using MementoMori.Core;
using TMPro;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>
    /// Timed dream dissolve; never changes the camera projection or physical world. Kept short (about ten
    /// seconds, four phrases) and skippable after the first beat, because the old 30 s version read as a stall.
    /// </summary>
    public sealed class DreamTransitionController : MonoBehaviour
    {
        public IEnumerator Play(float duration)
        {
            duration=Mathf.Clamp(duration,6f,10f);
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
            var hint=new GameObject("SkipHint",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            hint.transform.SetParent(veil.transform,false);hint.rectTransform.anchorMin=new Vector2(.6f,.04f);hint.rectTransform.anchorMax=new Vector2(.96f,.1f);
            hint.rectTransform.offsetMin=hint.rectTransform.offsetMax=Vector2.zero;
            hint.font=TMP_Settings.defaultFontAsset;hint.fontSize=20;hint.alignment=TextAlignmentOptions.BottomRight;hint.raycastTarget=false;
            hint.text=Loc.T("Espaço — pular");hint.color=new Color(.78f,.72f,.9f,0f);
            var phrases=new[]{"Eu já tracei este caminho.","Há algo que o quarto não consegue guardar.","Um som. Um nome que não alcanço.","Se eu seguir... vou lembrar?"};
            try
            {
                RuntimeAudio.PlayOneShot("05_transition_sleep",.6f);
                var elapsed=0f;
                while(elapsed<duration)
                {
                    elapsed+=Time.unscaledDeltaTime;
                    // Any confirm key after the first phrase settles skips straight to the black veil.
                    if(elapsed>1.2f&&elapsed<duration-.6f&&SkipPressed()) elapsed=duration-.6f;
                    var t=Mathf.Clamp01(elapsed/duration);
                    image.color=new Color(.090f,.078f,.141f,Mathf.SmoothStep(0,1,Mathf.Clamp01(t*6)));
                    var phase=t*phrases.Length;
                    line.text=Loc.T(phrases[Mathf.Min((int)phase,phrases.Length-1)]);
                    // Quick rise, long readable hold, quick fall: the old pure sine spent most of each phrase half-faded.
                    var local=Mathf.Clamp01(phase-Mathf.Floor(phase));
                    var alpha=Mathf.Clamp01(Mathf.Min(local/.18f,(1f-local)/.18f));
                    if(elapsed>=duration-.6f) alpha=Mathf.Min(alpha,Mathf.Clamp01((duration-elapsed)/.6f));
                    line.color=new Color(.78f,.72f,.9f,alpha);
                    hint.color=new Color(.78f,.72f,.9f,Mathf.Clamp01((elapsed-1.2f)/.5f)*Mathf.Clamp01((duration-.6f-elapsed)/.3f)*.55f);
                    yield return null;
                }
                // The veil ends on black: hand over to the loader's curtain without a frame of the room.
                SceneLoader.Instance?.CoverImmediately();
            }
            finally
            {
                if(overlay!=null) Destroy(overlay);
                InputGate.Instance?.Release("DreamTransition");
            }
        }

        private static bool SkipPressed()=>Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)
            ||Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.Escape)||Input.GetMouseButtonDown(0)
            ||GamepadInput.ConfirmDown()||GamepadInput.CancelDown();
    }
}
