using System.ComponentModel.DataAnnotations;

namespace JsonPlaceholder.ApiTesting.Models;

public sealed record Comment
{
    [Range(1, int.MaxValue)]
    public required int PostId { get; init; }

    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Body { get; init; }
}
