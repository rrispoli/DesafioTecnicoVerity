using DailyBalance.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Common;

namespace DailyBalance.Application.Features.Balances.Get;

public class GetBalanceQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetBalanceQuery, BalanceResponse>
{
    public async Task<Result<BalanceResponse>> HandleAsync(GetBalanceQuery query, CancellationToken cancellationToken = default)
    {
        var balance = await dbContext.Balances.FirstOrDefaultAsync(x => x.Date == query.Date, cancellationToken);
        return Result.Success(balance is null ? new BalanceResponse(query.Date) : new BalanceResponse(balance));
    }
}
