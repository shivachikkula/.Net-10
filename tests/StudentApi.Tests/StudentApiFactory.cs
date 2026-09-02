using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Api.Data;

namespace StudentApi.Tests;

public class StudentApiFactory : WebApplicationFactory<Program>
{
    // Shared across scopes/requests so every DbContext instance resolved during
    // a test run sees the same in-memory store instead of an isolated copy.
    private readonly InMemoryDatabaseRoot _databaseRoot = new();
    private readonly string _databaseName = $"StudentApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<StudentDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName, _databaseRoot));
        });
    }
}
