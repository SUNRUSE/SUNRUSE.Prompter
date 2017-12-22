﻿using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace SUNRUSE.Prompter.Persistence.Abstractions
{
    /// <summary>Describes the event IDs of a specific entity.</summary>
    public struct EventIds
    {
        /// <summary>The number of persisted events.</summary>
        public readonly int LatestEvent;

        /// <summary>The number of events persisted as of the latest snapshot, or zero if no snapshots have been taken.</summary>
        public readonly int LatestSnapshot;
    }

    /// <summary>Implemented by persistence stores.</summary>
    public interface IEventStore
    {
        /// <summary>Persists <see cref="byte"/>s representing an event.</summary>
        /// <param name="entityTypeName">The name of the entity type being persisted.  This can be considered a "namespace", "collection" or "table" of <paramref name="entityId"/>s.</param>
        /// <param name="entityId">The <see cref="Guid"/> of the entity instance being persisted.</param>
        /// <param name="data">The <see cref="byte"/>s to persist.</param>
        Task PersistEvent(string entityTypeName, Guid entityId, ImmutableArray<byte> data);

        /// <summary>Persists <see cref="byte"/> representing the result of applying every event previously persisted against an entity.</summary>
        /// <param name="entityTypeName">The name of the entity type being persisted.  This can be considered a "namespace", "collection" or "table" of <paramref name="entityId"/>s.</param>
        /// <param name="entityId">The <see cref="Guid"/> of the entity instance being persisted.</param>
        /// <param name="data">The <see cref="byte"/>s to persist.</param>
        Task PersistSnapshot(string entityTypeName, Guid entityId, ImmutableArray<byte> data);

        /// <summary>Gets the <see cref="EventIds"/> for a specified entity.</summary>
        /// <param name="entityTypeName">The name of the entity type being persisted.  This can be considered a "namespace", "collection" or "table" of <paramref name="entityId"/>s.</param>
        /// <param name="entityId">The <see cref="Guid"/> of the entity instance being queried.</param>
        /// <returns>A new <see cref="EventIds"/> for <paramref name="entityTypeName"/>/<paramref name="entityId"/>.</returns>
        Task<EventIds> GetEventIds(string entityTypeName, Guid entityId);

        /// <summary>Gets a specific event previously persisted against an entity.</summary>
        /// <param name="entityTypeName">The name of the entity type being persisted.  This can be considered a "namespace", "collection" or "table" of <paramref name="entityId"/>s.</param>
        /// <param name="entityId">The <see cref="Guid"/> of the entity instance being queried.</param>
        /// <param name="eventId">The ID of the event to get.</param>
        /// <returns>The <see cref="byte"/>s representing the requested event.</returns>
        Task<ImmutableArray<byte>> GetEvent(string entityTypeName, Guid entityId, int eventId);

        /// <summary>Gets a specific snapshot previously persisted against an entity.</summary>
        /// <param name="entityTypeName">The name of the entity type being persisted.  This can be considered a "namespace", "collection" or "table" of <paramref name="entityId"/>s.</param>
        /// <param name="entityId">The <see cref="Guid"/> of the entity instance being queried.</param>
        /// <param name="atEventId">The event ID of the snapshot to get.</param>
        /// <returns>The <see cref="byte"/>s representing the requested snapshot.</returns>
        Task<ImmutableArray<byte>> GetSnapshot(string entityTypeName, Guid entityId, int atEventId);
    }
}