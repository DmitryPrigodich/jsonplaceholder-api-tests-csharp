using System.ComponentModel.DataAnnotations;

namespace JsonPlaceholder.ApiTesting.Models;

public sealed record Todo
{
    [Range(1, int.MaxValue)]
    public required int UserId { get; init; }

    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public required string Title { get; init; }

    public required bool Completed { get; init; }
}
