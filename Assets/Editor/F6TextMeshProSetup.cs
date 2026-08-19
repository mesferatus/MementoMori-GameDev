using UnityEditor;
using UnityEngine;

namespace MementoMori.EditorTools
{
    public static class F6TextMeshProSetup
    {
        [MenuItem("Memento Mori/F6/Import TMP Essentials")]
        public static void ImportEssentials()
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            Debug.Log("F6 TMP Essential Resources import requested through Unity menu.");
        }
    }
}
