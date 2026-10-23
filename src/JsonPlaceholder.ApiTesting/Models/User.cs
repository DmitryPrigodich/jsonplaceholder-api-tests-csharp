using System.ComponentModel.DataAnnotations;

namespace JsonPlaceholder.ApiTesting.Models;

public sealed record User
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }

    public required Address Address { get; init; }

    public required string Phone { get; init; }

    public required string Website { get; init; }

    public required Company Company { get; init; }
}

public sealed record Address
{
    public required string Street { get; init; }

    public required string Suite { get; init; }

    public required string City { get; init; }

    public required string Zipcode { get; init; }

    public required Geo Geo { get; init; }
}

public sealed record Geo
{
    public required string Lat { get; init; }

    public required string Lng { get; init; }
}

public sealed record Company
{
    public required string Name { get; init; }

    public required string CatchPhrase { get; init; }

    public required string Bs { get; init; }
}
