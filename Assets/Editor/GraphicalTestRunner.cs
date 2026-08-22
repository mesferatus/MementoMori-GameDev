using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MementoMori.EditorTools
{
    /// <summary>Runs Unity Test Framework in the licensed graphical Editor.</summary>
    public sealed class GraphicalTestRunner : ICallbacks
    {
        private static GraphicalTestRunner active;
        private static TestMode pendingMode;
        private readonly string resultPath;

        private GraphicalTestRunner(string path) { resultPath = Path.GetFullPath(path); }

        public static void RunEditMode()
        {
            Run(TestMode.EditMode, "TestResults/editmode-c4c-graphical-final.xml");
        }

        public static void RunPlayMode()
        {
            Run(TestMode.PlayMode, "TestResults/playmode-c4c-graphical-final.xml");
        }

        private static void Run(TestMode mode, string path)
        {
            active = new GraphicalTestRunner(path);
            Directory.CreateDirectory(Path.GetDirectoryName(active.resultPath));
            pendingMode = mode;
            EditorApplication.update += StartWhenEditorReady;
        }

        private static void StartWhenEditorReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            EditorApplication.update -= StartWhenEditorReady;
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(active);
            api.Execute(new ExecutionSettings(new Filter { testMode = pendingMode }));
        }

        public void RunStarted(ITestAdaptor testsToRun) { Debug.Log($"[GRAPHICAL-TEST] Started: {resultPath}"); }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }

        public void RunFinished(ITestResultAdaptor result)
        {
            var xml = new StringBuilder();
            using (var writer = XmlWriter.Create(new StringWriter(xml), new XmlWriterSettings { Indent = true }))
                result.ToXml().WriteTo(writer);
            File.WriteAllText(resultPath, xml.ToString());
            LogFailures(result);
            Debug.Log($"[GRAPHICAL-TEST] Finished: {resultPath}; passed={result.PassCount}; failed={result.FailCount}; skipped={result.SkipCount}");
            EditorApplication.Exit(result.FailCount == 0 ? 0 : 1);
        }

        private static void LogFailures(ITestResultAdaptor result)
        {
            if (result.FailCount > 0 && !result.HasChildren)
                Debug.LogError($"[GRAPHICAL-TEST] FAILED {result.FullName}: {result.Message}\n{result.StackTrace}");
            if (result.Children == null) return;
            foreach (var child in result.Children) LogFailures(child);
        }
    }
}
