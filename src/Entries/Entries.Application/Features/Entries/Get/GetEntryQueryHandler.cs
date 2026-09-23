using Entries.Application.Abstractions;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Common;

namespace Entries.Application.Features.Entries.Get;

public class GetEntryQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetEntryQuery, EntryResponse>
{
    public async Task<Result<EntryResponse>> HandleAsync(GetEntryQuery query, CancellationToken cancellationToken = default)
    {
        var entry = await dbContext.Entries.FindAsync([query.Id], cancellationToken);
        if (entry is null)
            return Result.Failure<EntryResponse>(Error.NotFound("Entry.NotFound", $"Entry with Id '{query.Id}' was not found."));

        return Result.Success(new EntryResponse(entry));
    }
}
