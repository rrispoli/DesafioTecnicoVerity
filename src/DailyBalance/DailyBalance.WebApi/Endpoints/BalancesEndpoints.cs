using DailyBalance.Application.Features.Balances.Get;
using Shared.Application.Abstractions.Messaging;
using Shared.WebApi.Extensions;
using Shared.WebApi.Infrastructure;

namespace DailyBalance.WebApi.Endpoints;

public static class BalancesEndpoints
{
    public static void MapBalancesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/balances")
            .WithTags("Balances")
            .RequireAuthorization();

        group.MapGet("/{date:datetime}", GetByDate)
            .AddEndpointFilter<ValidationEndpointFilter<GetBalanceQuery>>()
            .WithName("GetBalanceByDate")
            .WithSummary("Get a balance by date");
    }

    private static async Task<IResult> GetByDate(
        DateOnly date,
        IQueryHandler<GetBalanceQuery, BalanceResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetBalanceQuery(date), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}