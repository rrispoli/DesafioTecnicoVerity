using Entries.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Entries.Application.Features.Entries.Create;

public record CreateEntryCommand(EntryType Type, decimal Amount, DateTimeOffset OccurredAt, string Description) : ICommand<Guid>;
