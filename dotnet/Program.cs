using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configuration for the database connection string
string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
string dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "user";
string dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "password";
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "db";
string connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};";

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

// Ensure the database is created and the required tables are set up
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();

    // Automatically create the database and tables
    dbContext.Database.EnsureCreated();

    // Check for the Entries table and create it if it doesn't exist
    var connection = dbContext.Database.GetDbConnection();
    await connection.OpenAsync();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT to_regclass('public.\"Entries\"');";
    var result = await command.ExecuteScalarAsync();
    if (result == DBNull.Value)
    {
        command.CommandText = "CREATE TABLE public.\"Entries\" (Id SERIAL PRIMARY KEY, Data TEXT NOT NULL);";
        await command.ExecuteNonQueryAsync();
    }
}

app.MapGet("/", async (AppDbContext dbContext) =>
{
    var randomData = Guid.NewGuid().ToString();
    dbContext.Entries.Add(new Entry { Data = randomData });
    await dbContext.SaveChangesAsync();

    var count = await dbContext.Entries.CountAsync();
    return Results.Ok(new { Message = "Entry added successfully with random data.", Data = randomData, Count = count });
});

app.MapGet("/status", () => Results.Ok("UP"));

app.Run();
