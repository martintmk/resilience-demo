using Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

await Telemetry.Example(services, CancellationToken.None);
