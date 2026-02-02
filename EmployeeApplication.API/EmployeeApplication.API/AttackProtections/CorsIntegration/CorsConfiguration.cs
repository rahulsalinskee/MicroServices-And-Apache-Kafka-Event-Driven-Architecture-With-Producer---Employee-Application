namespace EmployeeApplication.API.AttackProtections.CorsIntegration
{
    public static class CorsConfiguration
    {
        private const string CORS_POLICY_NAME = "EmployeeApplicationCorsPolicy";

        public static void AddCorsServicesExtension(this IServiceCollection services, IConfiguration configuration)
        {
            /* Usually, you'd store allowed origins in appsettings.json - Example: "AllowedOrigins": ["http://localhost:3000"] */
            var allowedOrigins = configuration.GetSection(key: "AllowedOrigins").Get<string[]>() ?? ["*"];

            services.AddCors(option =>
            {
                option.AddPolicy(name: CORS_POLICY_NAME, policy =>
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                    /* 
                    *  Note: If we allow Any Origin (*), we cannot use .AllowCredentials(). 
                    *  If we need cookies/auth, specify exact origins and use .AllowCredentials() 
                    */
                });
            });
        }

        public static void UseCorsMiddlewareExtension(this IApplicationBuilder app)
        {
            app.UseCors(CORS_POLICY_NAME);
        }
    }
}
