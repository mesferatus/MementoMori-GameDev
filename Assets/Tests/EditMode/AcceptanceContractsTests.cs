using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class AcceptanceContractsTests
    {
        [Test]
        public void BuildSettingsContainTheFiveBetaScenesInOrder()
        {
            var scenes = EditorBuildSettings.scenes;
            var expected = new[] { "MainMenu", "Quarto", "Labirinto", "DominioLua", "FinalBeta" };
            Assert.That(scenes.Length, Is.GreaterThanOrEqualTo(expected.Length));
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(scenes[i].enabled, Is.True);
                Assert.That(scenes[i].path, Is.EqualTo($"Assets/Scenes/{expected[i]}.unity"));
            }
        }

        [Test]
        public void RequiredDialogueAssetsArePresent()
        {
            foreach (var key in new[] { "DLG_ROOM_BOWL_01", "DLG_ROOM_GRIMOIRE_REVEAL", "DLG_DREAM_TRANSITION", "DLG_POE_REVEAL", "DLG_ANDREALPHUS_01", "DLG_ANDREALPHUS_02", "DLG_SIGIL_HINT_01", "DLG_MOON_DOMAIN_GATE" })
                Assert.That(Resources.Load<DialogueData>($"Dialogue/{key}"), Is.Not.Null, key);
        }

        [Test]
        public void AndrealphusDialoguePreservesTheScriptLineBreaks()
        {
            var first = Resources.Load<DialogueData>("Dialogue/DLG_ANDREALPHUS_01");
            var second = Resources.Load<DialogueData>("Dialogue/DLG_ANDREALPHUS_02");
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first.Lines, Has.Length.EqualTo(21));
            Assert.That(second.Lines, Has.Length.EqualTo(23));
            Assert.That(first.Lines[7].Text, Is.EqualTo("Pode?"));
            Assert.That(first.Lines[8].Text, Is.EqualTo("Que palavra misericordiosa."));
            Assert.That(first.Lines[12].Text, Is.EqualTo("Respostas s\u00e3o portas muito confiantes."));
            Assert.That(first.Lines[13].Text, Is.EqualTo("Aqui, as confiantes costumam levar ao mesmo lugar."));
            Assert.That(second.Lines[14].Text, Is.EqualTo("Pare de responder com perguntas."));
            Assert.That(second.Lines[15].Text, Is.EqualTo("Pare de fazer perguntas cujas respostas j\u00e1 desenhou no ch\u00e3o."));
        }

        [Test]
        public void NarrativeAuditBindingsLoadAsLiteralDialogueLines()
        {
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_ECHO_CORRIDOR_HINT").Lines[0].Text, Is.EqualTo("O que cresce sem nascer, desaparece sem morrer e retorna sem lembrar?"));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_ECHO_CORRIDOR_HINT").Lines[1].Text, Is.EqualTo("Uma pergunta antes do vocabulário."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_EMPTY_CHAMBER").Lines[0].Text, Is.EqualTo("Você encontrou o que trouxe."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_GARDEN_CRESCENT").Lines[0].Text, Is.EqualTo("Aquilo que se persegue aprende a fugir."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_GARDEN_WANING").Lines[0].Text, Is.EqualTo("Nem toda perda é falha."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_GARDEN_FULL").Lines[2].Text, Is.EqualTo("A Lua não abre. Ela permite ser lembrada."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_GARDEN_FULL").Lines[3].Text, Is.EqualTo("Isso pareceu uma distinção importante."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_FALSE_FULL_MOON_DOOR").Lines[2].Text, Is.EqualTo("Antes da primeira fase, a Lua já estava."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_ROOM_GRIMOIRE_REVEAL").Lines[5].Text, Is.EqualTo("Um nome abre. Um vínculo sustenta. Um preço fecha."));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_ANDREALPHUS_02").Lines[5].Text, Is.EqualTo("Saber é uma palavra que as pessoas usam quando esquecem quantas vezes tentaram."));
        }

        [Test]
        public void OpeningDialogueMatchesTheNarrativeLines()
        {
            var opening = Resources.Load<DialogueData>("Dialogue/DLG_ROOM_OPENING");
            Assert.That(opening, Is.Not.Null);
            Assert.That(opening.Lines, Has.Length.EqualTo(3));
            Assert.That(opening.Lines[0].Text, Is.EqualTo("Terceira noite sem sonho."));
            Assert.That(opening.Lines[1].Text, Is.EqualTo("Ou terceira manh\u00e3 sem lembran\u00e7a."));
            Assert.That(opening.Lines[2].Text, Is.EqualTo("A diferen\u00e7a deveria me tranquilizar."));
            Assert.That(opening.Lines[0].LockMovement, Is.True);
            Assert.That(opening.Lines[1].LockMovement, Is.True);
            Assert.That(opening.Lines[2].LockMovement, Is.True);
        }

        [Test]
        public void InspectorConfigurationStoresTheMirrorAndSigilContracts()
        {
            var root = new GameObject("ConfigurationTest");
            var configuration = root.AddComponent<SceneConfiguration>();
            configuration.Configure("DominioLua", "FinalBeta", false,
                new[] { "Entrada", "JardimLunar", "SalaDosEspelhos", "CorredorIlusorio", "CamaraDoSigilo", "SalaDoFragmento" },
                new[] { "Present", "Delayed", "Ahead", "Absent", "Double", "Room", "Black" },
                new[] { "Delayed", "Ahead", "Absent" },
                new[] { "Moon", "Eye", "Spiral" },
                new[] { "Dominio_CorredorFalso" });

            Assert.That(configuration.DomainAreas, Has.Length.EqualTo(6));
            Assert.That(configuration.MirrorSymbols, Has.Length.EqualTo(7));
            Assert.That(configuration.MirrorCorrectSymbols, Is.EqualTo(new[] { "Delayed", "Ahead", "Absent" }));
            Assert.That(configuration.SigilSequence, Is.EqualTo(new[] { "Moon", "Eye", "Spiral" }));
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SolvedDoorReleasesItsBlockingCollider()
        {
            var doorObject = new GameObject("DoorTest");
            var blocker = doorObject.AddComponent<BoxCollider2D>();
            var door = doorObject.AddComponent<DoorController>();
            door.Configure(blocker);
            door.Open();
            Assert.That(door.State, Is.EqualTo(DoorState.Open));
            Assert.That(blocker.enabled, Is.False);
            Object.DestroyImmediate(doorObject);
        }

        [Test]
        public void BedUnlockRequiresEveryRoomInteraction()
        {
            var root = new GameObject("RoomStateTest");
            var state = root.AddComponent<GameState>();
            Assert.That(BedController.HasRoomRequirements(state), Is.False);
            foreach (var flag in new[] { StoryFlag.RoomBowlExamined, StoryFlag.RoomToyExamined, StoryFlag.RoomPhotoExamined, StoryFlag.RoomGrimoireRead, StoryFlag.RoomWindowSecured, StoryFlag.RoomRitualItemStored }) state.SetFlag(flag);
            Assert.That(BedController.HasRoomRequirements(state), Is.True);
            Object.DestroyImmediate(root);
        }
    }
}
