using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// Read environment variables for database configuration
string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
string dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "user";
string dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "password";
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "db";

string connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};";

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapGet("/", async (AppDbContext context) =>
{
    var randomData = Guid.NewGuid().ToString();
    var entry = new Entry { Data = randomData };
    context.Entries.Add(entry);
    await context.SaveChangesAsync();

    var count = await context.Entries.CountAsync();
    return Results.Ok(new { Message = "Entry added successfully with random data.", Data = randomData, Count = count });
});

app.MapGet("/status", () => Results.Ok("UP"));

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
