using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests
{
    public sealed class NarrativeV3DialogueCatalogTests
    {
        static readonly string[] Forbidden =
        {
            "Poe está morto", "Poe morreu", "alma de Poe", "fragmento de Poe",
            "ressuscitar Poe", "reviver Poe", "Poe is dead", "Poe died",
            "Poe soul", "Poe fragment", "revive Poe", "resurrect Poe"
        };

        [Test]
        public void BibleV3HasExactly82UniqueRuntimeDialogueAssets()
        {
            var ids = MementoMori.Dialogue.NarrativeV3DialogueCatalog.Ids;
            Assert.That(ids, Has.Length.EqualTo(82));
            Assert.That(ids, Is.Unique);
            foreach (var id in ids)
            {
                var dialogue = Resources.Load<MementoMori.Dialogue.DialogueData>("Dialogue/" + id);
                Assert.That(dialogue, Is.Not.Null, id);
                Assert.That(dialogue.SequenceId, Is.EqualTo(id), id);
                Assert.That(dialogue.Lines, Is.Not.Null.And.Not.Empty, id);
                foreach (var line in dialogue.Lines)
                    foreach (var forbidden in Forbidden)
                        Assert.That(line.Text, Does.Not.Contain(forbidden), id);
            }
        }
    }
}
