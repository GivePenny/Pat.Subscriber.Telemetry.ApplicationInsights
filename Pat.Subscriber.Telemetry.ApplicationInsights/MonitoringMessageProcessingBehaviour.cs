﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Pat.Subscriber.MessageProcessing;
using System;
using System.Threading.Tasks;

namespace Pat.Subscriber.Telemetry.ApplicationInsights
{
    public class MonitoringMessageProcessingBehaviour : IMessageProcessingBehaviour
    {
        private readonly TelemetryClient telemetryClient;

        public MonitoringMessageProcessingBehaviour(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public async Task Invoke(Func<MessageContext, Task> next, MessageContext messageContext)
        {
            var messageType = messageContext.Message.UserProperties["MessageType"].ToString();

            using (var operation = telemetryClient.StartOperation<RequestTelemetry>(
                messageType,
                messageContext.CorrelationId))
            {
                try
                {
                    await next(messageContext);
                    Enrich(operation.Telemetry, messageContext);
                }
                catch (Exception exception)
                {
                    // It's the logger's responsibility to record the exception details.
                    Enrich(operation.Telemetry, messageContext, exception);
                    throw;
                }
                finally
                {
                    telemetryClient.StopOperation(operation);
                }
            }
        }

        private void Enrich(RequestTelemetry telemetry, MessageContext messageContext, Exception exception = null)
        {
            var enqueuedTimeUtc = messageContext.Message.SystemProperties.EnqueuedTimeUtc;
            var timeFromQueueToCompletion = telemetry.Timestamp + telemetry.Duration - enqueuedTimeUtc;

            // Attempt to compensate a little for clock differences
            var timeFromQueueToCompletionSeconds = Math.Max(timeFromQueueToCompletion.TotalSeconds, telemetry.Duration.TotalSeconds);

            telemetry.Success = exception == null;
            telemetry.Metrics.Add("QueueToCompletionTimeSeconds", timeFromQueueToCompletionSeconds);
        }
    }
}
