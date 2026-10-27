using System;
using MementoMori.Core;
using NUnit.Framework;

namespace MementoMori.Tests.EditMode
{
    public class PuzzleRegistryV4Tests
    {
        private static PuzzleRegistry Create(params string[] required) => new PuzzleRegistry(new PuzzleManifest {
            labyrinthRequired = required, moonRequired = new[] {"moon.test"}, optional = new[] {"optional.clue"}
        });
        [TestCase(1)] [TestCase(3)] [TestCase(6)] [TestCase(9)]
        public void GateRequiresConfiguredListRegardlessOfRegionCount(int count)
        {
            var ids = new string[count];
            for (var i=0;i<count;i++) ids[i]="lab."+i;
            foreach (var missing in ids)
            {
                var registry=Create(ids);
                registry.MarkSolved("optional.clue");
                foreach(var id in ids) if(id!=missing) registry.MarkSolved(id);
                Assert.That(registry.LabyrinthGateOpen,Is.False,missing);
                registry.MarkSolved(missing);
                Assert.That(registry.LabyrinthGateOpen,Is.True);
            }
        }
        [Test] public void DuplicateEventsNeverSubstituteForAnotherPuzzle()
        {
            var registry=Create("lab.a","lab.b");
            var events=0; registry.PuzzleSolved += id=>events++;
            for(var i=0;i<30;i++) registry.MarkSolved("lab.a");
            Assert.That(events,Is.EqualTo(1));
            Assert.That(registry.LabyrinthGateOpen,Is.False);
        }
        [Test] public void OptionalContentIsNotRequired()
        {
            var registry=Create("lab.a");
            registry.MarkSolved("lab.a");
            Assert.That(registry.LabyrinthGateOpen,Is.True);
            Assert.That(registry.GetSolved("optional.clue"),Is.False);
        }
        [Test] public void EmptyConfigurationFailsClosed() =>
            Assert.That(Create().LabyrinthGateOpen,Is.False);
        [Test] public void DuplicateConfigurationIsRejected() =>
            Assert.Throws<ArgumentException>(()=>Create("lab.a","lab.a"));
        [Test] public void UnknownIdsAreRejected() =>
            Assert.Throws<ArgumentException>(()=>Create("lab.a").MarkSolved("sanctuary.devil"));
        [Test] public void RestoreDoesNotReplayEventsOrAcceptRemovedIds()
        {
            var registry=Create("lab.a");
            var events=0; registry.PuzzleSolved += id=>events++;
            registry.Restore(new[]{"lab.a","lab.a","removed"});
            Assert.That(events,Is.Zero);
            Assert.That(registry.Snapshot(),Is.EquivalentTo(new[]{"lab.a"}));
            registry.Reset();
            Assert.That(registry.LabyrinthGateOpen,Is.False);
        }
        [Test] public void ProductionManifestContainsNoSanctuaryPuzzles()
        {
            var registry=new PuzzleRegistry();
            foreach(var id in registry.LabyrinthIds)
                Assert.That(id,Does.Not.Contain("sanctuary"));
            Assert.That(registry.LabyrinthIds,Does.Contain("lab.moon_activation"));
        }
    }
}
