using Microsoft.ApplicationInsights;
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

            using var operation = telemetryClient.StartOperation<RequestTelemetry>(
                messageType,
                messageContext.CorrelationId);

            try
            {
                await next(messageContext);
                MarkSuccessful(operation.Telemetry);
            }
            catch (Exception)
            {
                // It's the logger's responsibility to record the exception details, this module simply records the message request
                MarkAsFailed(operation.Telemetry);
                throw;
            }
            finally
            {
                Enrich(operation.Telemetry, messageContext);
            }
        }

        private static void Enrich(RequestTelemetry telemetry, MessageContext messageContext)
        {
            var enqueuedTimeUtc = messageContext.Message.SystemProperties.EnqueuedTimeUtc;
            var timeFromQueueToCompletion = telemetry.Timestamp + telemetry.Duration - enqueuedTimeUtc;

            // Attempt to compensate a little for clock differences
            var timeFromQueueToCompletionSeconds = Math.Max(timeFromQueueToCompletion.TotalSeconds, telemetry.Duration.TotalSeconds);

            telemetry.Metrics.Add("QueueToCompletionTimeSeconds", timeFromQueueToCompletionSeconds);
        }

        private static void MarkSuccessful(RequestTelemetry telemetry)
        {
            telemetry.Success = true;
            telemetry.ResponseCode = "200";
        }

        private static void MarkAsFailed(RequestTelemetry telemetry)
        {
            // If Success is set to false, App Insights assumes that the application did not "successfully handle the request" and so does not log it in the requests collection
            // but will instead only log to the exceptions collection.  We want both the Request and the exception logged (if relevant) so we indicate that every message was
            // successfully handled by the application.  App Insights seems to expect this in the sense that success means that any error has been caught and processed by
            // logging and then the application has continued as expected on to the next message.
            telemetry.Success = true;
            telemetry.ResponseCode = "500";
        }
    }
}
