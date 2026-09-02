using System.ComponentModel.DataAnnotations;

namespace StudentApi.Api.Dtos;

public record CreateStudentRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [Required]
    public required DateOnly DateOfBirth { get; init; }

    [StringLength(100)]
    public string? Department { get; init; }

    [Range(0.0, 4.0)]
    public decimal Gpa { get; init; }
}

public record UpdateStudentRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [Required]
    public required DateOnly DateOfBirth { get; init; }

    [StringLength(100)]
    public string? Department { get; init; }

    [Range(0.0, 4.0)]
    public decimal Gpa { get; init; }
}

public record StudentResponse
{
    public required int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public string? Department { get; init; }
    public required decimal Gpa { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
