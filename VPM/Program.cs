using Logging.Core;
using Microsoft.OpenApi.Models;
using VPM.Application;

using Prometheus;
namespace VPM
{

    internal class Program
    {

        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
            // Get instance name from environment variable
            var instanceName = Environment.GetEnvironmentVariable("INSTANCE_NAME") ?? "vpm-default";
            
            // Log instance startup
            Console.WriteLine($"Starting VPM instance: {instanceName}");
            
            ConfigureServices(builder);
            var app = builder.Build();
            ConfigureMiddleware(app);
            
            // Log instance ready
            Console.WriteLine($"VPM instance {instanceName} is ready to accept requests");
            
            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            string connectionInfo = builder.Configuration.GetConnectionString("conString") ?? string.Empty;
            builder.Services.DependencyInject(connectionInfo);
            Logger.Configure(builder.Configuration, builder.Environment.EnvironmentName);
            Logger.LogMessage("VPM", "Information", "Logger configured In VPM MicroService");
            // Add controllers to the dependency injection (DI) container.
            builder.Services.AddControllers();

            builder.Services.AddHttpClient();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                });
            });

            if (true)
            {
                // Provides automatic generation of API documentation, making it easier to understand and use the API.
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                                    Enter 'Bearer' [space] and then your token in the text input below.
                                    \r\n\r\nExample: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                    });
                });
            }

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:8080/realms/myrealm";
                    options.Audience = "*";
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = "account",
                        ValidateIssuer = true,
                        ValidIssuer = "http://localhost:8080/realms/myrealm",
                        ValidateLifetime = true
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("authenticated-user", policy =>
                    policy.RequireAuthenticatedUser());
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors("AllowAll");
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();  // Enables routing.
            app.UseAuthorization();
            app.UseHttpMetrics(); // To measure HTTP request metrics

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics(); // Exposes /metrics endpoint for Prometheus
                endpoints.MapControllers();
            });
            app.MapControllers();
        }

    }

}