using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Task_Manager.Extensions;

public static class RateLimitingExtension
{
    public static void AddRateLimit(this WebApplicationBuilder builder)
    {
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
    }
}