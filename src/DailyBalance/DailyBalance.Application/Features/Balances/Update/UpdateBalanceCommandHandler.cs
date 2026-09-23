using DailyBalance.Application.Abstractions;
using DailyBalance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Contracts.IntegrationEvents.EntryCreated;
using Shared.Domain.Common;

namespace DailyBalance.Application.Features.Balances.Update;

public class UpdateBalanceCommandHandler(
    ILogger<UpdateBalanceCommandHandler> logger,
    IApplicationDbContext dbContext)
    : ICommandHandler<UpdateBalanceCommand, Guid>
{
    private const int MaxAttempts = 5;

    public async Task<Result<Guid>> HandleAsync(UpdateBalanceCommand command, CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var alreadyProcessed = await dbContext.ProcessedEvents
                .AnyAsync(x => x.EventId == command.EntryId, cancellationToken);

            if (alreadyProcessed)
            {
                logger.LogInformation("EventId '{EntryId}' has already been processed.", command.EntryId);
                return Result.Failure<Guid>(Error.Conflict("Balance.Update.Conflict", $"EventId '{command.EntryId}' has already been processed."));
            }

            var date = DateOnly.FromDateTime(command.OccurredAt.Date);
            var balance = await dbContext.Balances.FirstOrDefaultAsync(x => x.Date == date, cancellationToken);

            if (balance is null)
            {
                balance = new Balance(date);
                dbContext.Balances.Add(balance);
            }

            if (command.Type == EntryCreatedType.Credit)
                balance.AddCredit(command.Amount);
            else
                balance.AddDebit(command.Amount);

            var processedEvent = new ProcessedEvent(command.EntryId);
            dbContext.ProcessedEvents.Add(processedEvent);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Processed UpdateBalanceCommand for date {Date}: Credit={Credit}, Debit={Debit}, Total={Total}",
                    balance.Date, balance.Credit, balance.Debit, balance.Total);

                return Result.Success(balance.Id);
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex,
                    "Concurrency conflict while processing EventId '{EntryId}' (attempt {Attempt}/{MaxAttempts}).",
                    command.EntryId, attempt, MaxAttempts);

                if (dbContext is DbContext efContext)
                {
                    efContext.Entry(processedEvent).State = EntityState.Detached;
                    efContext.Entry(balance).State = EntityState.Detached;
                }

                if (attempt == MaxAttempts)
                    break;

                await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(150, 400)), cancellationToken);
            }
        }

        return Result.Failure<Guid>(Error.Conflict("Balance.Update.Conflict",
            $"Unable to process EventId '{command.EntryId}' due to a concurrent write conflict."));
    }
}
