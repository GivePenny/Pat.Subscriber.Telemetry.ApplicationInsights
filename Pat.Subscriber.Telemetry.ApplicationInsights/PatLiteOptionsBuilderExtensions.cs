using Pat.Subscriber.NetCoreDependencyResolution;

namespace Pat.Subscriber.Telemetry.ApplicationInsights
{
    public static class PatLiteOptionsBuilderExtensions
    {
        public static IPatLiteOptionsBuilder WithApplicationInsightsMonitoring(this IPatLiteOptionsBuilder optionsBuilder)
        {
            // TODO PR in Pat.Subscriber repo to add With<> methods to IPatLiteOptionsBuilder
            ((PatLiteOptionsBuilder)optionsBuilder).With<MonitoringMessageProcessingBehaviour>();
            return optionsBuilder;
        }
    }
}
