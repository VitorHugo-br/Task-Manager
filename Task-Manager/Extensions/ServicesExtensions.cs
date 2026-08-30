using Microsoft.IdentityModel.JsonWebTokens;
using Minio;
using Task_Manager.Interfaces;
using Task_Manager.Services;

namespace Task_Manager.Extensions;

public static class ServicesExtensions
{
    public static void AdicionarServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<AuthService>();
        builder.Services.AddTransient<RedisService>();
        builder.Services.AddTransient<AuditService>();
        builder.Services.AddScoped<MinIoStorageService>();
        builder.Services.AddScoped<ILogService, LogService>();
        builder.Services.AddScoped<JsonWebTokenHandler>();
        
    }

    public static void ConfigurarMinIO(this WebApplicationBuilder builder)
    {

        var minioEndpoint = builder.Configuration["MinIO:Endpoint"];
        var minioAccessKey = builder.Configuration["MinIO:AccessKey"];
        var minioSecretKey = builder.Configuration["MinIO:SecretKey"];
        var minioSecure = Convert.ToBoolean(builder.Configuration["MinIO:Secure"]);

        builder.Services.AddMinio(configureClient =>
        {
            configureClient.WithEndpoint(minioEndpoint)
                           .WithCredentials(minioAccessKey, minioSecretKey)
                           .WithSSL(minioSecure);
        });

    }
}