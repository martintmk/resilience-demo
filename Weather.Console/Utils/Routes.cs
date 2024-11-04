using Microsoft.Extensions.Http.Resilience;

namespace Weather.Console.Final.Utils;

public class Routes
{
    public static void ConfigureEndpoints(OrderedGroupsRoutingOptions options)
    {
        options.Groups.Add(new UriEndpointGroup
        {
            Endpoints = new List<WeightedUriEndpoint>
            {
                new WeightedUriEndpoint
                {
                    Uri = new Uri("https://localhost:7100"),
                }
            }
        });

        options.Groups.Add(new UriEndpointGroup
        {
            Endpoints = new List<WeightedUriEndpoint>
            {
                new WeightedUriEndpoint
                {
                    Uri = new Uri("https://localhost:8100"),
                }
            }
        });
    }
}
