using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

namespace Task_Manager.Extensions;

public static class ScalarExtension
{
    public static void ConfigureDevelopmentApiDocument(this WebApplication webApplication)
    {
        if (!webApplication.Environment.IsDevelopment()) return;
        
        webApplication.MapOpenApi();

        webApplication.MapScalarApiReference("/api-docs", options =>
        {
            options.Title = "Task Manager API";
            options.AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
                .AddHttpAuthentication(JwtBearerDefaults.AuthenticationScheme,
                    auth => { auth.Token = ""; })
                .EnablePersistentAuthentication();
        });
    }
}