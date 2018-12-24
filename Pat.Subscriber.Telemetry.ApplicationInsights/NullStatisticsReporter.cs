using Pat.Subscriber.Telemetry.StatsD;
using System;

namespace Pat.Subscriber.Telemetry.ApplicationInsights
{
    public sealed class NullStatisticsReporter : IStatisticsReporter
    {
        sealed class State : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public void Increment(string @event, string tags, int value = 1)
        {
        }

        public IDisposable StartTimer(string @event, string tags)
        {
            return new State();
        }

        public void Timer(string @event, string tags, TimeSpan duration)
        {
        }
    }
}
