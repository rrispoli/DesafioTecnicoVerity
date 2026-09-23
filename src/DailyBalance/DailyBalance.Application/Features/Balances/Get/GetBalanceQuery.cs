using Shared.Application.Abstractions.Messaging;

namespace DailyBalance.Application.Features.Balances.Get;

public record GetBalanceQuery(DateOnly Date) : IQuery<BalanceResponse>;
