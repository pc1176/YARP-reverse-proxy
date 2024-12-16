using System.ComponentModel.Design.Serialization;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace ReverseProxy
{
    public class RtspLoadBalancingPolicy : ILoadBalancingPolicy
    {
        public string Name => "RtspLoadBalancing";

        //string ILoadBalancingPolicy.Name => throw new NotImplementedException();

        public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
        {
            // Get the RTSP URL from query parameters
            if (context.Request.Query.TryGetValue("rtspUrl", out var rtspUrl))
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

                return availableDestinations.FirstOrDefault(d => d.DestinationId.Equals(destinationKey, StringComparison.OrdinalIgnoreCase));
            }

            // Fallback to first available destination if no RTSP URL is provided
            return availableDestinations.FirstOrDefault();
        }

    }
}
