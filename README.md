# YARP-reverse-proxy
**YARP (Yet Another Reverse Proxy) Documentation**

---

### Overview
YARP (Yet Another Reverse Proxy) is a highly customizable reverse proxy built on ASP.NET Core. It enables forwarding HTTP requests to different backends, load balancing, routing, and handling authentication, among other functionalities. YARP leverages ASP.NET’s middleware pipeline, making it a versatile solution for microservices and modern web architectures.

---

### Key Features
1. **Dynamic Configuration**: Configure routes and clusters dynamically via JSON or runtime APIs.
2. **Load Balancing**: Distribute traffic across backend services using algorithms such as round-robin, least requests, etc.
3. **Routing**: Route incoming HTTP requests to specific backends based on path, header, query string, or other criteria.
4. **Protocol Support**: Supports HTTP/1.x, HTTP/2, and HTTP/3 protocols.
5. **Middleware Integration**: Use ASP.NET Core middleware for authentication, logging, and request/response transformations.
6. **Health Probing**: Perform active and passive health checks to ensure service availability.

---

### Diagram

Below is a diagram illustrating the YARP architecture:

1. **Client** sends an HTTP request to YARP.
2. **YARP Reverse Proxy** evaluates the request against configured routes.
3. Based on routing rules, YARP forwards the request to a backend destination.
4. **Backend Services** process the request and return the response to YARP.
5. **YARP Reverse Proxy** sends the response back to the client.

```
        +---------+          +---------------------+
        |  Client |  --->    |  YARP Reverse Proxy |
        +---------+          +---------------------+
                                 |
     +-------------------+-------------------+
     |                                       |
+--------------+                   +----------------+
| Backend API1 |                   | Backend API2   |
+--------------+                   +----------------+
```
---

### Installation
Add the YARP NuGet package to your ASP.NET Core project:

```
 dotnet add package Microsoft.ReverseProxy
```

---

### Configuration
YARP can be configured in the `appsettings.json` file or programmatically during runtime.

#### Example Configuration in `appsettings.json`

```json
{
  "ReverseProxy": {
    "Routes": {
      "route1": {
        "ClusterId": "cluster1",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "cluster1": {
        "Destinations": {
          "destination1": {
            "Address": "https://backend-service1.example.com/"
          },
          "destination2": {
            "Address": "https://backend-service2.example.com/"
          }
        },
        "LoadBalancingPolicy": "RoundRobin"
      }
    }
  }
}
```

#### Programmatic Configuration in `Startup.cs`

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run();
```

---

### Advanced Features
#### Middleware Integration
Customize request/response handling by integrating custom middleware:

```csharp
app.Use(async (context, next) =>
{
    // Modify the request
    context.Request.Headers.Add("X-Proxy-Header", "YARP");
    await next.Invoke();
    // Modify the response
    context.Response.Headers.Add("X-Processed-By", "YARP");
});
```

#### Dynamic Route Updates
Update routes or clusters dynamically:

```csharp
var proxyConfigProvider = app.Services.GetRequiredService<IProxyConfigProvider>();
// Custom implementation to update routes/clusters at runtime.
```

---

### Health Probing
Enable health checks for backends to ensure traffic is only routed to healthy destinations.

#### Example Health Probing Configuration

```json
{
  "Clusters": {
    "cluster1": {
      "HealthCheck": {
        "Active": {
          "Enabled": true,
          "Interval": "00:00:10",
          "Timeout": "00:00:05",
          "Policy": "Simple",
          "Path": "/health"
        }
      }
    }
  }
}
```

---

### Use Cases
1. **Microservices Gateway**: Simplify service discovery and routing in microservices-based applications.
2. **Load Balancing**: Distribute traffic evenly across multiple service instances.
3. **API Gateway**: Act as a secure entry point for APIs with support for authentication and authorization.
4. **Service Migrations**: Route traffic to newer service versions without disrupting clients.

---

### Resources

https://microsoft.github.io/reverse-proxy/articles/getting-started.html