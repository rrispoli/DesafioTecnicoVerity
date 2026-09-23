using Entries.Application.Features.Entries.Create;
using Entries.Application.Features.Entries.Get;
using Shared.Application.Abstractions.Messaging;
using Shared.WebApi.Extensions;
using Shared.WebApi.Infrastructure;

namespace Entries.WebApi.Endpoints;

public static class EntriesEndpoints
{
    public static void MapEntriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/entries")
            .WithTags("Entries")
            .RequireAuthorization();

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationEndpointFilter<CreateEntryCommand>>()
            .WithName("CreateEntry")
            .WithSummary("Create a new entry");

        group.MapGet("/{id:guid}", GetById)
            .AddEndpointFilter<ValidationEndpointFilter<GetEntryQuery>>()
            .WithName("GetEntryById")
            .WithSummary("Get an entry by ID");
    }

    private static async Task<IResult> Create(
        CreateEntryCommand command,
        ICommandHandler<CreateEntryCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetEntryById", new { id = result.Value })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetEntryQuery, EntryResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetEntryQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
