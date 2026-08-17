using Microsoft.EntityFrameworkCore;
using QueueLess.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Retrieve the connection string from the configuration manager
// (reads appsettings.json, then overrides it with User Secrets locally, or Environment Variables in production)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

//2 Register the DbContext as Scoped (default) using PostgreSQL
builder.Services.AddDbContext<QlDbContext>(options => options.UseNpgsql(connectionString, b => b.MigrationsAssembly("QueueLess.Infrastructure"))); //Generates migration inside Infrastructure

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
