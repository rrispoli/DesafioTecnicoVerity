using Shared.Application.Abstractions.Messaging;

namespace Entries.Application.Features.Entries.Get;

public record GetEntryQuery(Guid Id) : IQuery<EntryResponse>;
