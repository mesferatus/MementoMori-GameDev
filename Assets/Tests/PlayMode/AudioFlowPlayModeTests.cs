using System.Collections;
using MementoMori.Audio;
using MementoMori.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class AudioFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator AudioSceneTransitionKeepsOneLoopAndChangesTrack()
        {
            var scenes = new[]
            {
                ("MainMenu", "20_menu_theme"),
                ("Quarto", "01_room_ambience"),
                ("Labirinto", "02_labyrinth_drone"),
                ("FinalBeta", "19_beta_ending_stinger")
            };

            foreach (var entry in scenes)
            {
                yield return SceneManager.LoadSceneAsync(entry.Item1);
                yield return WaitForTrack(entry.Item2);
                var runtimeAudio = GameObject.Find("RuntimeAudio");
                Assert.That(runtimeAudio, Is.Not.Null, entry.Item1);
                Assert.That(CountRuntimeAudioObjects(), Is.EqualTo(1));
                var source = runtimeAudio.GetComponent<AudioSource>();
                Assert.That(source, Is.Not.Null);
                Assert.That(source.loop, Is.True);
                Assert.That(source.clip, Is.Not.Null);
                Assert.That(source.clip.name, Is.EqualTo(entry.Item2));
            }
        }

        [UnityTest]
        public IEnumerator NewGameResetsAudioToMenuThenQuarto()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return WaitForTrack("20_menu_theme");
            Assert.That(GameState.Instance, Is.Not.Null);

            GameManager.Instance.StartNewGame();
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Quarto");
            yield return WaitForTrack("01_room_ambience");

            Assert.That(CountRuntimeAudioObjects(), Is.EqualTo(1));
            Assert.That(GameState.Instance.HasFlag(StoryFlag.GardenComplete), Is.False);
        }

        [UnityTest]
        public IEnumerator InteractionSfxResourceIsAvailableAndOneShotIsCreatedOnce()
        {
            yield return SceneManager.LoadSceneAsync("Quarto");
            yield return WaitForTrack("01_room_ambience");

            var clip = Resources.Load<AudioClip>("Audio/06_interaction_click");
            Assert.That(clip, Is.Not.Null);
            RuntimeAudio.PlayOneShot("06_interaction_click", .35f);
            Assert.That(GameObject.Find("RuntimeOneShot"), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<RuntimeAudio>(FindObjectsInactive.Include).Length, Is.EqualTo(1));
            yield return null;
        }

        private static IEnumerator WaitForTrack(string clipName)
        {
            yield return new WaitUntil(() =>
            {
                var runtimeAudio = GameObject.Find("RuntimeAudio");
                var source = runtimeAudio == null ? null : runtimeAudio.GetComponent<AudioSource>();
                return source != null && source.clip != null && source.clip.name == clipName && source.isPlaying;
            });
        }

        private static int CountRuntimeAudioObjects()
        {
            return Object.FindObjectsByType<RuntimeAudio>(FindObjectsInactive.Include).Length;
        }
    }
}
