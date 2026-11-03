using System.Collections;
using System.IO;
using MementoMori.Dialogue;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class AsciiPresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator AllTwelveAsciiPresentationsRenderAndFinishTheirActions()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            var directory = Path.Combine(Application.temporaryCachePath, "V61AsciiValidation_EndOfFrame");
            Directory.CreateDirectory(directory);
            foreach (var id in new[] { "C01", "C02", "C03", "L01", "C_POE", "A1", "L08", "M02_WATER", "C_BLACK", "A2", "M07", "FINAL" })
            {
                var sequence = Resources.Load<IllustratedSequence>("Cinematics/" + id);
                Assert.That(sequence, Is.Not.Null, id);
                Assert.That(sequence.finalArtwork, Is.False, id + " is still provisional artwork.");
                var view = IllustratedCutsceneView.Create(id);
                Assert.That(view, Is.Not.Null, id);
                try
                {
                    foreach (var beat in sequence.beats)
                    {
                        Assert.That(beat.asciiFrames.Length, Is.GreaterThan(1), id);
                        view.StartCoroutine(view.PerformBeforeLine(beat.beforeLine));
                        yield return null;
                        Canvas.ForceUpdateCanvases();
                        var drawing = view.GetComponentInChildren<AsciiIllustration>();
                        Assert.That(drawing, Is.Not.Null, id);
                        var mesh = drawing.GetComponent<CanvasRenderer>().GetMesh();
                        Assert.That(mesh, Is.Not.Null, id);
                        Assert.That(mesh.vertexCount, Is.GreaterThan(0), id);
                        yield return CaptureFrame(Path.Combine(directory, id + "_" + beat.beforeLine + "_start.png"));
                        var deadline = Time.realtimeSinceStartup + beat.duration + 5f;
                        while (view.Acting && Time.realtimeSinceStartup < deadline) yield return null;
                        Assert.That(view.Acting, Is.False, id + " must release its action lock.");
                        view.SetSpeaker("Melantha");
                        view.SetBody("Estudo de apresentação ASCII — arte provisória.");
                        yield return CaptureFrame(Path.Combine(directory, id + "_" + beat.beforeLine + "_end.png"));
                    }
                }
                finally { Object.Destroy(view.gameObject); }
                yield return null;
            }
        }

        private static IEnumerator CaptureFrame(string path)
        {
            // Complete UI layout and rendering before readback; do not leave an asynchronous
            // capture queued while the next beat changes or destroys its canvas.
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            try { File.WriteAllBytes(path, texture.EncodeToPNG()); }
            finally { Object.Destroy(texture); }
        }
    }
}
