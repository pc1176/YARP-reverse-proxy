using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ReverseProxy;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Transforms;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("*")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        // Configure authentication
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "http://localhost:8080/realms/myrealm";
                options.Audience = "*";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "account",
                    ValidateIssuer = true,
                    ValidIssuer = "http://localhost:8080/realms/myrealm",
                    ValidateLifetime = true
                };
            });
        // Add authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("authenticated-user", policy =>
                policy.RequireAuthenticatedUser());
        });
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .AddCustomLoadBalancingPolicy();

        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapReverseProxy();
        app.Run();

    }
}
public static class ReverseProxyExtensions
{
    public static IReverseProxyBuilder AddCustomLoadBalancingPolicy(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<ILoadBalancingPolicy, RtspLoadBalancingPolicy>();
        return builder;
    }
}
