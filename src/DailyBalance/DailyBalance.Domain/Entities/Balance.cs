using Shared.Domain.Common;

namespace DailyBalance.Domain.Entities;

public class Balance : Entity
{
    public DateOnly Date { get; private set; }
    public decimal Credit { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Total => Credit - Debit;

    public Balance(DateOnly date)
    {
        Date = date;
    }

    public void AddCredit(decimal amount)
    {
        Credit += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDebit(decimal amount)
    {
        Debit += amount;
        UpdatedAt = DateTime.UtcNow;
    }
}
