using StudentApi.Api.Dtos;
using StudentApi.Api.Models;

namespace StudentApi.Api.Extensions;

// C# 14 extension members: grouped per receiver type instead of the classic
// "this" parameter static method style.
public static class StudentMappingExtensions
{
    extension(Student student)
    {
        public StudentResponse ToResponse() => new()
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            Department = student.Department,
            Gpa = student.Gpa,
            CreatedAtUtc = student.CreatedAtUtc,
            UpdatedAtUtc = student.UpdatedAtUtc,
        };

        public void ApplyUpdate(UpdateStudentRequest request)
        {
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.Email = request.Email;
            student.DateOfBirth = request.DateOfBirth;
            student.Department = request.Department;
            student.Gpa = request.Gpa;
            student.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    extension(CreateStudentRequest request)
    {
        public Student ToEntity() => new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Department = request.Department,
            Gpa = request.Gpa,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
