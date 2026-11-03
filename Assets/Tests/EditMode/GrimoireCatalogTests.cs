using System.Linq;
using MementoMori.World;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class GrimoireCatalogTests
    {
        [Test]
        public void CanonicalEntriesCoverFourTabsWithReadableText()
        {
            var entries = GrimoireCatalog.Entries;
            Assert.That(entries.Count, Is.EqualTo(40));
            Assert.That(entries.Select(entry => entry.Id).Distinct().Count(), Is.EqualTo(40));
            Assert.That(entries.Count(entry => entry.Category == 0), Is.EqualTo(12));
            Assert.That(entries.Count(entry => entry.Category == 1), Is.EqualTo(7));
            Assert.That(entries.Count(entry => entry.Category == 2), Is.EqualTo(13));
            Assert.That(entries.Count(entry => entry.Category == 3), Is.EqualTo(8));
            foreach (var entry in entries)
            {
                Assert.That(entry.Title, Is.Not.Empty, entry.Id);
                Assert.That(!string.IsNullOrWhiteSpace(entry.Body) || entry.Hints.Any(hint => !string.IsNullOrWhiteSpace(hint)),
                    Is.True, entry.Id);
            }
        }

        [Test]
        public void SavedGameGrimoireCanLoadItsVisualResources()
        {
            foreach (var path in new[] { "book_open", "page_illustration", "side_note", "sigil_symbols" })
                Assert.That(Resources.Load<Sprite>("UI/Grimoire/" + path), Is.Not.Null, path);
            Assert.That(Resources.LoadAll<Sprite>("UI/Grimoire/GRIMOIRE_UI_SHEET_V2").Length,
                Is.GreaterThanOrEqualTo(8));
        }
    }
}
