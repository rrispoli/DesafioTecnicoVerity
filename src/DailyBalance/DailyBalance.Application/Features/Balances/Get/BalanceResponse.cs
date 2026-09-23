using DailyBalance.Domain.Entities;

namespace DailyBalance.Application.Features.Balances.Get;

public record BalanceResponse
{
    public DateOnly Date { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public decimal Credit { get; init; }
    public decimal Debit { get; init; }
    public decimal Total { get; init; }

    public BalanceResponse(DateOnly date)
    {
        Date = date;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public BalanceResponse(Balance entity)
    {
        Date = entity.Date;
        UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt;
        Credit = entity.Credit;
        Debit = entity.Debit;
        Total = entity.Total;
    }
}
