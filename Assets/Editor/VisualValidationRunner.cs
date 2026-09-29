using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MementoMori.EditorTools
{
    // Keeps the interactive Editor open after verification.
    public sealed class VisualValidationRunner : ICallbacks
    {
        private static TestRunnerApi api;
        private static VisualValidationRunner callback;
        private string path;
        public static void Run(bool playMode)
        {
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            callback = new VisualValidationRunner { path = "TestResults/VisualRebuild/" + (playMode ? "playmode" : "editmode") + ".xml" };
            api.RegisterCallbacks(callback);
            api.Execute(new ExecutionSettings(new Filter { testMode = playMode ? TestMode.PlayMode : TestMode.EditMode }));
        }
        public void RunStarted(ITestAdaptor test) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            var xml = new StringBuilder();
            using (var writer = XmlWriter.Create(new StringWriter(xml), new XmlWriterSettings { Indent = true })) result.ToXml().WriteTo(writer);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, xml.ToString());
            Debug.Log($"[VISUAL VALIDATION] {path}: passed={result.PassCount}, failed={result.FailCount}, skipped={result.SkipCount}");
        }
    }
}
