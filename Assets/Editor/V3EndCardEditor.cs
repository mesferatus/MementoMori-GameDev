using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MementoMori.UI;

namespace MementoMori.EditorTools
{
    public static class V3EndCardEditor
    {
        static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var t=parent.Find(name) as RectTransform;
            if(!t){t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();t.SetParent(parent,false);}
            t.localScale=Vector3.one;t.anchorMin=t.anchorMax=t.pivot=new Vector2(.5f,.5f);t.anchoredPosition=position;t.sizeDelta=size;return t;
        }
        static TMP_Text Label(Transform parent,string name,string content,Vector2 position,Vector2 size,int fontSize)
        {
            var t=Rect(parent,name,position,size);var label=t.GetComponent<TextMeshProUGUI>();if(!label)label=t.gameObject.AddComponent<TextMeshProUGUI>();
            label.text=content;label.font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");label.fontSize=fontSize;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.82f,.76f,.87f);label.raycastTarget=false;return label;
        }
        static Button Button(Transform parent,string name,string text,float x,UnityEngine.Events.UnityAction action)
        {
            var t=Rect(parent,name,new Vector2(x,-385),new Vector2(420,86));
            var image=t.GetComponent<Image>();if(!image)image=t.gameObject.AddComponent<Image>();
            image.sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/MainMenu/02_button_frame_normal.png").OfType<Sprite>().First();image.type=Image.Type.Simple;image.color=Color.white;
            var button=t.GetComponent<Button>();if(!button)button=t.gameObject.AddComponent<Button>();button.targetGraphic=image;
            button.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(button.onClick,action);
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.8f,1);colors.selectedColor=colors.highlightedColor;button.colors=colors;
            Label(t,"Label",text,Vector2.zero,new Vector2(380,64),29);return button;
        }
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play first.");
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/FinalBeta.unity");
            var controller=Object.FindFirstObjectByType<FinalBetaController>();
            var root=GameObject.Find("FinalBetaUI").GetComponent<RectTransform>();
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.localScale=Vector3.one;
            var backdrop=GameObject.Find("Backdrop");if(backdrop){backdrop.transform.SetParent(root,false);var r=(RectTransform)backdrop.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;backdrop.transform.SetAsFirstSibling();backdrop.GetComponent<Image>().color=new Color(.024f,.019f,.038f,1);var cg=backdrop.GetComponent<CanvasGroup>();if(cg)cg.alpha=1;}
            foreach(var n in new[]{"Background","FinalFragmentLarge","FinalBetaSubtitle_TMP"}){var t=root.Find(n);if(t)t.gameObject.SetActive(false);}
            Rect(root,"FinalTitle",new Vector2(0,335),new Vector2(1100,200));
            Rect(root,"CentralOrnament",new Vector2(0,85),new Vector2(420,420));
            var message=Label(root,"ClosingMessage","Algumas coisas não terminam.\nElas apenas mudam de lugar.\n\nObrigado por carregar esta memória até aqui.",new Vector2(0,-210),new Vector2(1500,185),32);
            Button(root,"CreditsButton","CRÉDITOS",-480,controller.ShowCredits);
            var back=Button(root,"ReturnButton","VOLTAR AO MENU",0,controller.ReturnToMenu);
            Button(root,"QuitButton","SAIR",480,controller.Quit);
            var credits=Rect(root,"CreditsPanel",Vector2.zero,new Vector2(1500,890));var panel=credits.GetComponent<Image>();if(!panel)panel=credits.gameObject.AddComponent<Image>();panel.color=new Color(.04f,.025f,.06f,1);
            Label(credits,"CreditsText","MEMENTO MORI\n\nCallisto\nProgramação, gameplay, integração, puzzles, áudio técnico e testes\n\nLuiza\nDireção visual, sprites, cenários e interface",new Vector2(0,65),new Vector2(1300,540),32);
            Button(credits,"CloseCredits","VOLTAR",0,controller.HideCredits);credits.gameObject.SetActive(false);
            var so=new SerializedObject(controller);so.FindProperty("finalTextTmp").objectReferenceValue=message;so.FindProperty("returnButton").objectReferenceValue=back;so.FindProperty("credits").objectReferenceValue=credits.gameObject;so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
