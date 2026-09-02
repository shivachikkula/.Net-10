using Microsoft.EntityFrameworkCore;
using StudentApi.Api.Data;
using StudentApi.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Skipped under the "Testing" environment so integration tests can register
// an in-memory provider instead without two providers colliding in DI.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<StudentDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("StudentDb")));
}

// .NET 10 minimal API automatic validation: honors DataAnnotations on
// endpoint parameters without hand-written validation filters.
builder.Services.AddValidation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<StudentDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.MapGroup("/api/students")
    .WithTags("Students")
    .MapStudentEndpoints();

app.Run();

public partial class Program;
