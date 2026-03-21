using Microsoft.EntityFrameworkCore;
using Task_Manager;
using Task_Manager.Data;
using Task_Manager.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddMyServices();

builder.AddMyAuthentication();

builder.AddRateLimit();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

// Register EF Core DbContext for dependency injection
builder.Services.AddDbContext<TaskDbContext>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureDevelopmentApiDocument();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.InitializeDatabaseAsync();

await app.RunAsync();