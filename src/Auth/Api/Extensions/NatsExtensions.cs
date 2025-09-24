using System.Text;
using System.Text.Json;
using NATS.Client.Core;

namespace Auth.Api.Extensions
{
    public static class NatsExtensions
    {
        public static async Task EnsureStreamExistsAsync(
            this INatsConnection connection,
            string streamName,
            string[] subjects,
            ILogger logger,
            TimeSpan? requestTimeout = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connection);
            if (string.IsNullOrWhiteSpace(streamName)) throw new ArgumentException("streamName required", nameof(streamName));
            if (subjects is null || subjects.Length == 0) throw new ArgumentException("At least one subject is required", nameof(subjects));

            var timeout = requestTimeout ?? TimeSpan.FromSeconds(5);
            var replyOpts = new NatsSubOpts { Timeout = timeout };

            logger.LogInformation("NATS connection info: {Conn}", connection.ToString());

            var infoSubject = $"$JS.API.STREAM.INFO.{streamName}";
            try
            {
                logger.LogDebug("Requesting stream INFO from {Subject}", infoSubject);
                var infoResp = await connection.RequestAsync<byte[]>(
                    infoSubject,
                    replySerializer: null,
                    replyOpts: replyOpts,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                var infoText = Encoding.UTF8.GetString(infoResp.Data ?? []);
                logger.LogDebug("Stream INFO raw response: {Info}", infoText);

                using var doc = JsonDocument.Parse(infoText);
                // Typical successful INFO includes "config" or "stream_name" or "state"
                if (doc.RootElement.TryGetProperty("config", out _) || doc.RootElement.TryGetProperty("stream_name", out _) || doc.RootElement.TryGetProperty("state", out _))
                {
                    logger.LogInformation("NATS JetStream stream '{StreamName}' exists (INFO validated).", streamName);
                    return;
                }

                // If payload contains error, treat as not found
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    logger.LogWarning("Stream INFO replied with error: {Error}", err.ToString());
                    // fall through to create attempt
                }
                else
                {
                    logger.LogWarning("Stream INFO payload did not contain expected fields; will attempt create. Payload: {Payload}", infoText);
                }
            }
            catch (Exception ex) when (IsJetStreamNotFound(ex))
            {
                logger.LogInformation("Stream INFO not found (exception indicates missing): {Ex}", ex.Message);
                // fall through to create
            }

            // Create stream
            var createSubject = $"$JS.API.STREAM.CREATE.{streamName}";
            var createBody = new
            {
                name = streamName,
                subjects,
                storage = "file",
                retention = "limits"
            };

            var bodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(createBody));

            logger.LogInformation("Creating JetStream stream '{StreamName}' via {Subject}", streamName, createSubject);
            var createResp = await connection.RequestAsync<byte[], byte[]>(
                createSubject,
                bodyBytes,
                headers: null,
                requestSerializer: null,
                replySerializer: null,
                requestOpts: null,
                replyOpts: replyOpts,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var respText = Encoding.UTF8.GetString(createResp.Data ?? []);
            logger.LogDebug("Create stream response: {Resp}", respText);

            using (var doc = JsonDocument.Parse(respText))
            {
                if (doc.RootElement.TryGetProperty("error", out var errEl))
                {
                    var code = errEl.TryGetProperty("code", out var c) ? c.GetInt32() : -1;
                    var desc = errEl.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : errEl.ToString();

                    // If error indicates "already exists" treat as success
                    if (desc.Contains("already exists", StringComparison.OrdinalIgnoreCase) || desc.Contains("name in use", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogInformation("Stream '{StreamName}' already existed according to create response.", streamName);
                        return;
                    }

                    throw new InvalidOperationException($"JetStream create stream failed: code={code}, desc={desc}");
                }
            }

            logger.LogInformation("Successfully created NATS JetStream stream '{StreamName}'.", streamName);
        }

        private static bool IsJetStreamNotFound(Exception ex)
        {
            if (ex == null) return false;
            if (ex is TimeoutException) return true;                     // treat timeout as not found / unreachable
            if (ex is NatsNoRespondersException) return true;            // no responders for admin subject
            if (ex.InnerException is NatsNoRespondersException) return true;
            // Detect NATS API-style error wrapper if your client surfaces it
            if (ex.GetType().Name.Equals("NatsApiException", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

    }
}
