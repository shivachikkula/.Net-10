using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StudentApi.Api.Data;
using StudentApi.Api.Dtos;
using StudentApi.Api.Extensions;

namespace StudentApi.Api.Endpoints;

public static class StudentEndpoints
{
    public static RouteGroupBuilder MapStudentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync)
            .WithName("GetStudents")
            .WithSummary("Gets all students.");

        group.MapGet("/{id:int}", GetByIdAsync)
            .WithName("GetStudentById")
            .WithSummary("Gets a single student by id.");

        group.MapPost("/", CreateAsync)
            .WithName("CreateStudent")
            .WithSummary("Creates a new student.");

        group.MapPut("/{id:int}", UpdateAsync)
            .WithName("UpdateStudent")
            .WithSummary("Updates an existing student.");

        group.MapDelete("/{id:int}", DeleteAsync)
            .WithName("DeleteStudent")
            .WithSummary("Deletes a student.");

        return group;
    }

    private static async Task<Ok<List<StudentResponse>>> GetAllAsync(
        StudentDbContext db, CancellationToken cancellationToken)
    {
        var students = await db.Students
            .AsNoTracking()
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Select(s => s.ToResponse())
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(students);
    }

    private static async Task<Results<Ok<StudentResponse>, NotFound>> GetByIdAsync(
        int id, StudentDbContext db, CancellationToken cancellationToken)
    {
        var student = await db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return student is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(student.ToResponse());
    }

    private static async Task<Results<Created<StudentResponse>, Conflict<string>>> CreateAsync(
        CreateStudentRequest request, StudentDbContext db, CancellationToken cancellationToken)
    {
        var emailExists = await db.Students
            .AnyAsync(s => s.Email == request.Email.Trim().ToLower(), cancellationToken);

        if (emailExists)
        {
            return TypedResults.Conflict($"A student with email '{request.Email}' already exists.");
        }

        var student = request.ToEntity();

        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/students/{student.Id}", student.ToResponse());
    }

    private static async Task<Results<Ok<StudentResponse>, NotFound, Conflict<string>>> UpdateAsync(
        int id, UpdateStudentRequest request, StudentDbContext db, CancellationToken cancellationToken)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (student is null)
        {
            return TypedResults.NotFound();
        }

        var emailTaken = await db.Students
            .AnyAsync(s => s.Id != id && s.Email == request.Email.Trim().ToLower(), cancellationToken);

        if (emailTaken)
        {
            return TypedResults.Conflict($"A student with email '{request.Email}' already exists.");
        }

        student.ApplyUpdate(request);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(student.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, StudentDbContext db, CancellationToken cancellationToken)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (student is null)
        {
            return TypedResults.NotFound();
        }

        db.Students.Remove(student);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
