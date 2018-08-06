using Pat.Subscriber.NetCoreDependencyResolution;

namespace Pat.Subscriber.Telemetry.ApplicationInsights
{
    public static class PatLiteOptionsBuilderExtensions
    {
        public static PatLiteOptionsBuilder WithApplicationInsightsMonitoring(this PatLiteOptionsBuilder optionsBuilder)
        {
            optionsBuilder.With<MonitoringMessageProcessingBehaviour>();
            return optionsBuilder;
        }
    }
}
