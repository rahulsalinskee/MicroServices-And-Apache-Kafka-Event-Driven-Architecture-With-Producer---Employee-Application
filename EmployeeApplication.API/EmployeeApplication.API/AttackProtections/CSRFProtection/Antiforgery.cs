using Microsoft.AspNetCore.Antiforgery;

namespace EmployeeApplication.API.AttackProtections.CSRFProtection
{
    public static class Antiforgery
    {
        /// <summary>
        /// 1. Logic for Antiforgery
        /// Adds the application antiforgery.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddAppAntiForgeryExtension(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
            });
        }

        /// <summary>
        /// 2. Middleware to generate and send antiforgery token in a cookie.
        /// Logic for Middleware pipeline (Antiforgery Cookie)
        /// </summary>
        /// <param name="app">The application.</param>
        public static void UseAntiForgeryTokenMiddlewareExtension(this IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                var antiforgery = app.ApplicationServices.GetRequiredService<IAntiforgery>();
                var tokens = antiforgery.GetAndStoreTokens(context);

                context.Response.Cookies.Append(key: "XSRF-TOKEN", value: tokens.RequestToken!, options: new CookieOptions() 
                { 
                    HttpOnly = false, 
                    Secure = true, 
                    SameSite = SameSiteMode.Strict 
                });
                return next(context);
            });
        }
    }
}
