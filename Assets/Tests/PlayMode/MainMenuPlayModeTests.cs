using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MementoMori.Tests.PlayMode
{
    public sealed class MainMenuPlayModeTests
    {
        [UnityTest]
        public IEnumerator SettingsOpenFromMenuAndRestoreMenuButtons()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            var menu = GameObject.Find("MainMenuButtons");
            var button = GameObject.Find("Button_Configuracoes")?.GetComponent<Button>();
            Assert.That(button, Is.Not.Null);
            button.onClick.Invoke();
            var settings = Object.FindFirstObjectByType<MementoMori.UI.SettingsPanelView>();
            Assert.That(settings.IsOpen, Is.True);
            Assert.That(menu.activeSelf, Is.False);
            settings.Close();
            Assert.That(settings.IsOpen, Is.False);
            Assert.That(menu.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator JogarCarregaQuarto()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return new WaitForSecondsRealtime(0.1f);

            var button = GameObject.Find("Button_Jogar")?.GetComponent<Button>();
            Assert.That(button, Is.Not.Null);
            Assert.That(button.interactable, Is.True);
            Assert.That(MementoMori.Core.GameManager.Instance, Is.Not.Null);
            Assert.That(MementoMori.Core.SceneLoader.Instance, Is.Not.Null);

            button.onClick.Invoke();
            yield return new WaitForSecondsRealtime(2f);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Quarto"));
        }
    }
}
