using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Task_Manager.Data;
using Task_Manager.Interfaces;
using Task_Manager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<AuthService>();
builder.Services.AddTransient<RedisService>();
builder.Services.AddTransient<AuditService>();
builder.Services.AddScoped<ILogService, LogService>();

var key = Encoding.ASCII.GetBytes(builder.Configuration["SecretKey"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

// Register EF Core DbContext for dependency injection
builder.Services.AddDbContext<TaskDbContext>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference("/api-docs");
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();