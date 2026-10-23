using System.ComponentModel.DataAnnotations;

namespace JsonPlaceholder.ApiTesting.Models;

public sealed record Post
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    [Range(1, int.MaxValue)]
    public required int UserId { get; init; }

    public required string Title { get; init; }

    public required string Body { get; init; }
}

/// <summary>A valid request body. Invalid ones are sent as raw JSON, so they never pass through this type.</summary>
public sealed record PostPayload(int UserId, string Title, string Body);
