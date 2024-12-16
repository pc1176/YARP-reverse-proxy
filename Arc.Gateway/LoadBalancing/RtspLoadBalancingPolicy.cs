using Microsoft.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace Arc.Gateway.LoadBalancing
{
    public class RtspLoadBalancingPolicy : ILoadBalancingPolicy
    {
        public string Name => "RtspLoadBalancing";

        public DestinationState? PickDestination(LoadBalancingContext context, IReadOnlyList<DestinationState> destinations, HashSet<DestinationState>? excludedDestinations)
        {
            if (destinations.Count == 0)
            {
                return null;
            }

            // Get the RTSP URL from query parameters
            if (context.HttpContext.Request.Query.TryGetValue("rtspUrl", out var rtspUrl))
            {
                try
                {
                    // Parse the RTSP URL to get the IP address
                    var uri = new Uri(rtspUrl.ToString());
                    var ipAddress = uri.Host;

                    // Simple static routing based on IP address last octet
                    var lastOctet = int.Parse(ipAddress.Split('.').Last());
                    
                    // Example routing logic:
                    // IPs ending in 1-50 go to vpm1
                    // IPs ending in 51-100 go to vpm2
                    // IPs ending in 101-255 go to vpm3
                    var destinationKey = lastOctet switch
                    {
                        <= 50 => "vpm1",
                        <= 100 => "vpm2",
                        _ => "vpm3"
                    };

                    var selectedDestination = destinations.FirstOrDefault(d => 
                        d.DestinationId.Equals(destinationKey, StringComparison.OrdinalIgnoreCase) && 
                        (excludedDestinations == null || !excludedDestinations.Contains(d)));

                    if (selectedDestination != null)
                    {
                        return selectedDestination;
                    }
                }
                catch
                {
                    // If there's any error parsing the URL or IP, fall through to default behavior
                }
            }

            // Fallback to first available destination if no RTSP URL is provided or if there was an error
            return destinations.FirstOrDefault(d => 
                d.Available && 
                (excludedDestinations == null || !excludedDestinations.Contains(d)));
        }
    }
} 