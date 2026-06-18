using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Task_Manager.Extensions;

public static class AuthenticationExtension
{
    public static void AddMyAuthentication(this WebApplicationBuilder builder)
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["SecretKey"]!);
        builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }
            )
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidIssuer = "TaskManager-Api",
                        ValidateIssuer = true,
                        ValidAudience = "TaskManager-Front",
                        ValidateAudience = true,
                        ClockSkew = TimeSpan.Zero
                    };
                }
            );
    }
}