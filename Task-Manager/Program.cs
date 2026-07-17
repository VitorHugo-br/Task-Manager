using Microsoft.AspNetCore.SignalR;
using Task_Manager;
using Task_Manager.Data;
using Task_Manager.Extensions;
using Task_Manager.Helpers;
using Task_Manager.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AdicionarServices();

builder.AddMyAuthentication();

builder.AddRateLimit();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

// Register EF Core DbContext for dependency injection
builder.Services.AddDbContext<TaskDbContext>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7273/")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureDevelopmentApiDocument();

app.UseHttpsRedirection();

app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseAuthorization();

await app.InitializeDatabaseAsync();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

await app.RunAsync();