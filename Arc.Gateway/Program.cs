using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Configuration;
using Arc.Gateway.LoadBalancing;
using Yarp.ReverseProxy.LoadBalancing;

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

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddCustomLoadBalancingPolicy();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Use YARP middleware
app.MapReverseProxy();

app.Run();

// Helper method to determine VPM instance
string DetermineVpmInstance(string cameraNo)
{
    try
    {
        if (!int.TryParse(cameraNo, out int cameraNumber))
        {
            throw new ArgumentException("Invalid camera number");
        }

        // You could load this mapping from configuration or database
        var vpmMapping = new Dictionary<int, string>
        {
            { 1, "vpm1" },
            { 2, "vpm2" },
            { 3, "vpm3" }
        };

        // Get VPM instance based on camera number or use fallback
        if (vpmMapping.TryGetValue(cameraNumber, out string? vpmInstance))
        {
            return vpmInstance;
        }

        // Fallback to round-robin
        return $"vpm{(cameraNumber % 3) + 1}";
    }
    catch (Exception ex)
    {
        // Log the error
        Console.WriteLine($"Error determining VPM instance: {ex.Message}");
        // Return default instance
        return "vpm1";
    }
}

// Add this extension method
public static class ReverseProxyExtensions
{
    public static IReverseProxyBuilder AddCustomLoadBalancingPolicy(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<ILoadBalancingPolicy, RtspLoadBalancingPolicy>();
        return builder;
    }
}
