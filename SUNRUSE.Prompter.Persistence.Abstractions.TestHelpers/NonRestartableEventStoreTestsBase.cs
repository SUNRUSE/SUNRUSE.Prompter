// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SUNRUSE.Prompter.Persistence.Abstractions.TestHelpers
{
    public abstract class NonRestartableEventStoreTestsBase
    {
        protected abstract IEventStore CreateInstance();

        protected static readonly Random Random = new Random();

        protected static ImmutableArray<byte> CreateTestData()
        {
            lock (Random)
            {
                return Enumerable
                    .Range(0, Random.Next(20, 50))
                    .Select(i => (byte)Random.Next(0, 256))
                    .ToImmutableArray();
            }
        }

        protected struct Sequence
        {
            public readonly string EntityTypeName;

            public readonly Guid EntityId;

            public readonly ImmutableArray<Step> Steps;

            public readonly int StartingEventId;

            public Sequence(string entityTypeName, Guid entityId, ImmutableArray<Step> steps, int startingEventId)
            {
                EntityTypeName = entityTypeName;
                EntityId = entityId;
                Steps = steps;
                StartingEventId = startingEventId;
            }
        }

        protected struct Step
        {
            public readonly bool IsSnapshot;

            public readonly ImmutableArray<byte> Data;

            public Step(bool isSnapshot, ImmutableArray<byte> data)
            {
                IsSnapshot = isSnapshot;
                Data = data;
            }
        }

        protected async Task InsertInterleaved(IEventStore eventStore, ImmutableArray<Sequence> sequences)
        {
            var maxSteps = sequences.Max(sequence => sequence.Steps.Length);
            for (var i = 0; i < maxSteps; i++) foreach (var sequence in sequences)
                {
                    var runFromSteps = (i * sequence.Steps.Length) / maxSteps;
                    var runToSteps = ((i + 1) * sequence.Steps.Length) / maxSteps;

                    for (var j = runFromSteps; j < runToSteps; j++)
                    {
                        var step = sequence.Steps[j];
                        if (step.IsSnapshot)
                        {
                            await eventStore.PersistSnapshot(sequence.EntityTypeName, sequence.EntityId, sequence.StartingEventId + sequence.Steps.Take(j).Count(found => !found.IsSnapshot) - 1, step.Data);
                        }
                        else
                        {
                            await eventStore.PersistEvent(sequence.EntityTypeName, sequence.EntityId, sequence.StartingEventId + sequence.Steps.Take(j).Count(found => !found.IsSnapshot), step.Data);
                        }
                    }
                }
        }

        protected readonly Sequence CompetingSequenceWithSameEntityTypeName = new Sequence("Test Entity Type Name", Guid.NewGuid(), ImmutableArray.Create
        (
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(true, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(true, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData())
        ), 0);

        protected readonly Sequence CompetingSequenceWithSameEntityId = new Sequence("Test Other Entity Type Name", Guid.NewGuid(), ImmutableArray.Create
        (
            new Step(false, CreateTestData()),
            new Step(true, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(true, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(true, CreateTestData()),
            new Step(false, CreateTestData()),
            new Step(true, CreateTestData())
        ), 0);

        [Theory, Trait("Type", "Integration")]
        [InlineData(false, false, false, false, null)]
        [InlineData(true, false, false, false, null)]
        [InlineData(true, true, false, false, 4)]
        [InlineData(true, true, true, false, 4)]
        [InlineData(true, true, true, true, 4)]
        [InlineData(true, true, false, true, 4)]
        public async Task GetStatisticsGreatestEventIdWithoutRestarting(bool includesCompetingSequences, bool includesEvents, bool includesSnapshots, bool endsWithSnapshot, int? greatestEventId)
        {
            using (var eventStore = CreateInstance())
            {
                var steps = new List<Step>();
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (endsWithSnapshot) steps.Add(new Step(true, CreateTestData()));
                var sequences = new List<Sequence> { new Sequence(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId, steps.ToImmutableArray(), 0) };
                if (includesCompetingSequences)
                {
                    sequences.Add(CompetingSequenceWithSameEntityTypeName);
                    sequences.Add(CompetingSequenceWithSameEntityId);
                }
                await InsertInterleaved(eventStore, sequences.ToImmutableArray());

                var statistics = await eventStore.GetStatistics(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId);

                Assert.Equal(greatestEventId, statistics.GreatestEventId);
            }
        }

        [Theory, Trait("Type", "Integration")]
        [InlineData(false, false, false, false, null)]
        [InlineData(true, false, false, false, null)]
        [InlineData(true, true, false, false, null)]
        [InlineData(true, true, true, false, 3)]
        [InlineData(true, true, true, true, 4)]
        [InlineData(true, true, false, true, 4)]
        public async Task GetStatisticsGreatestSnapshotIdWithoutRestarting(bool includesCompetingSequences, bool includesEvents, bool includesSnapshots, bool endsWithSnapshot, int? greatestSnapshotId)
        {
            using (var eventStore = CreateInstance())
            {
                var steps = new List<Step>();
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (endsWithSnapshot) steps.Add(new Step(true, CreateTestData()));
                var sequences = new List<Sequence> { new Sequence(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId, steps.ToImmutableArray(), 0) };
                if (includesCompetingSequences)
                {
                    sequences.Add(CompetingSequenceWithSameEntityTypeName);
                    sequences.Add(CompetingSequenceWithSameEntityId);
                }
                await InsertInterleaved(eventStore, sequences.ToImmutableArray());

                var statistics = await eventStore.GetStatistics(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId);

                Assert.Equal(greatestSnapshotId, statistics.GreatestSnapshotId);
            }
        }

        [Theory, Trait("Type", "Integration")]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, false, true)]
        public async Task GetEventWithoutRestarting(bool includesCompetingSequences, bool includesEvents, bool includesSnapshots, bool endsWithSnapshot)
        {
            using (var eventStore = CreateInstance())
            {
                var steps = new List<Step>();
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (endsWithSnapshot) steps.Add(new Step(true, CreateTestData()));
                var expected = steps
                    .Where(step => !step.IsSnapshot)
                    .Select(step => step.Data)
                    .ToList();
                var sequences = new List<Sequence> { new Sequence(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId, steps.ToImmutableArray(), 0) };
                if (includesCompetingSequences)
                {
                    sequences.Add(CompetingSequenceWithSameEntityTypeName);
                    sequences.Add(CompetingSequenceWithSameEntityId);
                }
                await InsertInterleaved(eventStore, sequences.ToImmutableArray());
                var actual = new List<ImmutableArray<byte>>();

                while (actual.Count < expected.Count()) actual.Add(await eventStore.GetEvent(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId, actual.Count));

                Assert.Equal(expected, actual);
            }
        }

        [Theory, Trait("Type", "Integration")]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, false, true)]
        public async Task GetSnapshotWithoutRestarting(bool includesCompetingSequences, bool includesEvents, bool includesSnapshots, bool endsWithSnapshot)
        {
            using (var eventStore = CreateInstance())
            {
                var steps = new List<Step>();
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (includesSnapshots) steps.Add(new Step(true, CreateTestData()));
                if (includesEvents) steps.Add(new Step(false, CreateTestData()));
                if (endsWithSnapshot) steps.Add(new Step(true, CreateTestData()));
                var expected = steps
                    .Where(step => step.IsSnapshot)
                    .Select(step => step.Data)
                    .ToList();
                var sequences = new List<Sequence> { new Sequence(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId, steps.ToImmutableArray(), 0) };
                if (includesCompetingSequences)
                {
                    sequences.Add(CompetingSequenceWithSameEntityTypeName);
                    sequences.Add(CompetingSequenceWithSameEntityId);
                }
                await InsertInterleaved(eventStore, sequences.ToImmutableArray());
                var actual = new List<ImmutableArray<byte>>();

                while (actual.Count < expected.Count()) actual.Add(await eventStore.GetSnapshot(CompetingSequenceWithSameEntityTypeName.EntityTypeName, CompetingSequenceWithSameEntityId.EntityId, steps.TakeWhile(step => step.Data != expected[actual.Count]).Count(step => !step.IsSnapshot) - 1));

                Assert.Equal(expected, actual);
            }
        }

        [Fact, Trait("Type", "Fuzz")]
        public async Task FuzzTestWithoutRestarting()
        {
            using (var eventStore = CreateInstance())
            {
                var sequenceTemplates = Enumerable
                    .Range(0, 150)
                    .Select(i =>
                    {
                        var greatestEventId = -1;
                        var greatestSnapshotId = (int?)null;

                        var steps = Enumerable
                            .Range(0, Random.Next(25, 50))
                            .Select(step =>
                            {
                                var isSnapshot = step % 2 == 1 && Random.Next(0, 2) == 0;
                                if (isSnapshot)
                                {
                                    greatestSnapshotId = greatestEventId;
                                }
                                else
                                {
                                    greatestEventId++;
                                }

                                var output = new
                                {
                                    Snapshot = isSnapshot,
                                    Data = CreateTestData(),
                                    EventId = greatestEventId
                                };

                                return output;
                            })
                            .ToList();

                        var lastSnapshot = steps.LastOrDefault(step => step.Snapshot);

                        return new
                        {
                            EntityTypeName = $"Test Entity Type Name {Random.Next(0, 4)}",
                            EntityId = Guid.NewGuid(),
                            GreatestEventId = greatestEventId,
                            GreatestSnapshotId = greatestSnapshotId,
                            Steps = steps
                        };
                    })
                    .ToList();

                var sequenceResults = await Task.WhenAll(sequenceTemplates.Select(async sequenceTemplate =>
                {
                    foreach (var step in sequenceTemplate.Steps)
                    {
                        if (step.Snapshot)
                        {
                            await eventStore.PersistSnapshot(sequenceTemplate.EntityTypeName, sequenceTemplate.EntityId, step.EventId, step.Data);
                        }
                        else
                        {
                            await eventStore.PersistEvent(sequenceTemplate.EntityTypeName, sequenceTemplate.EntityId, step.EventId, step.Data);
                        }
                    }
                    var statistics = await eventStore.GetStatistics(sequenceTemplate.EntityTypeName, sequenceTemplate.EntityId);
                    Assert.Equal(sequenceTemplate.GreatestEventId, statistics.GreatestEventId);
                    Assert.Equal(sequenceTemplate.GreatestSnapshotId, statistics.GreatestSnapshotId);
                    var results = new List<ImmutableArray<byte>>();
                    foreach (var step in sequenceTemplate.Steps)
                    {
                        if (step.Snapshot)
                        {
                            results.Add(await eventStore.GetSnapshot(sequenceTemplate.EntityTypeName, sequenceTemplate.EntityId, step.EventId));
                        }
                        else
                        {
                            results.Add(await eventStore.GetEvent(sequenceTemplate.EntityTypeName, sequenceTemplate.EntityId, step.EventId));
                        }
                    }
                    return results;
                }));

                Assert.Equal(sequenceTemplates.Select(sequence => sequence.Steps.Select(step => step.Data)), sequenceResults);
            }
        }
    }
}
