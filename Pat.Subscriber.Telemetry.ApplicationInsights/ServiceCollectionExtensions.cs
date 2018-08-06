using Microsoft.Extensions.DependencyInjection;

namespace Pat.Subscriber.Telemetry.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationInsightsPatMonitoring(IServiceCollection services)
            => services
                .AddSingleton<MonitoringMessageProcessingBehaviour>();
    }
}
