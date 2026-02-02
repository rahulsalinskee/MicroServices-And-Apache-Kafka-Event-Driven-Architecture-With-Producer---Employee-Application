using EmployeeApplication.Model.DTOs.ResponseDTOS;
using EmployeeApplication.Model.Models;
using System.Threading.RateLimiting;

namespace EmployeeApplication.API.AttackProtections.RateLimitation
{
    public static class RateLimiterConfiguration
    {
        public static void AddRateLimitingServicesExtension(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                /* 1. Define the status code for rejected requests */
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                /* 
                *  2. Define a Global Policy (Applied to ALL endpoints by default)
                *  We use "Partitioned" rate limiting to track limits separately for each IP Address.
                */
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    /* Get the User's IP. If null (e.g. localhost), use "unknown". */
                    var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter
                    (
                        partitionKey: remoteIp,
                        factory: partition => new FixedWindowRateLimiterOptions()
                        {
                            /* Logic: Allow 2 request per 1 minute per IP (Global Default) */
                            AutoReplenishment = true,
                            PermitLimit = 2,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }
                    );
                });

                /* 
                *  3. Add the "StrictPolicy" Apply this to specific controllers using [EnableRateLimiting("StrictPolicy")]
                */
                options.AddPolicy("StrictPolicy", context =>
                {
                    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1, // Only 5 creates allowed
                        Window = TimeSpan.FromMinutes(1), // per minute
                        QueueLimit = 0
                    });
                });

                /* 
                * 4. Customize the Response to match your "ResponseDto" format
                */
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    ApplicationError applicationError = new()
                    {
                        ID = Guid.NewGuid(),
                        Message = "Too many requests. Please slow down and try again later.",
                        When = DateTime.Now,
                    };

                    var responseDto = new ResponseDto()
                    {
                        IsSuccess = false,
                        Result = null,
                        Message = applicationError.Message,
                        DateTimeOnFailure = applicationError.When,
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(responseDto, token);
                };
            });
        }
    }
}
