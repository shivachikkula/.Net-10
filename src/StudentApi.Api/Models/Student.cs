namespace StudentApi.Api.Models;

public class Student
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    // C# 14 `field` keyword: normalizes on write without a hand-declared backing field.
    public required string Email
    {
        get;
        set => field = value.Trim().ToLowerInvariant();
    }

    public DateOnly DateOfBirth { get; set; }

    public string? Department { get; set; }

    public decimal Gpa { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
