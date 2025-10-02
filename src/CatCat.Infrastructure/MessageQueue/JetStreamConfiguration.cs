using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Microsoft.Extensions.Logging;

namespace CatCat.Infrastructure.MessageQueue;

public class JetStreamConfiguration
{
    private readonly NatsConnection _connection;
    private readonly ILogger<JetStreamConfiguration> _logger;

    public JetStreamConfiguration(NatsConnection connection, ILogger<JetStreamConfiguration> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task InitializeStreamsAsync(CancellationToken cancellationToken = default)
    {
        var jsContext = new NatsJSContextFactory().CreateContext(_connection);

        await CreateOrUpdateStreamAsync(jsContext, new StreamConfig
        {
            Name = "ORDER",
            Description = "Order events stream",
            Subjects = new[] { "order.created", "order.status_changed" },
            Storage = StreamConfigStorage.File,
            Retention = StreamConfigRetention.Workqueue,
            MaxAge = TimeSpan.FromDays(7),
            MaxMsgs = 1_000_000
        }, cancellationToken);

        await CreateOrUpdateStreamAsync(jsContext, new StreamConfig
        {
            Name = "REVIEW",
            Description = "Review events stream",
            Subjects = new[] { "review.created", "review.replied" },
            Storage = StreamConfigStorage.File,
            Retention = StreamConfigRetention.Workqueue,
            MaxAge = TimeSpan.FromDays(30),
            MaxMsgs = 500_000
        }, cancellationToken);

        _logger.LogInformation("JetStream streams initialized successfully");
    }

    private async Task CreateOrUpdateStreamAsync(
        INatsJSContext jsContext,
        StreamConfig config,
        CancellationToken cancellationToken)
    {
        try
        {
            var stream = await jsContext.CreateStreamAsync(config, cancellationToken);
            _logger.LogInformation("JetStream stream created: {StreamName}", config.Name);
        }
        catch
        {
            try
            {
                var stream = await jsContext.UpdateStreamAsync(config, cancellationToken);
                _logger.LogInformation("JetStream stream updated: {StreamName}", config.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update stream: {StreamName}", config.Name);
                throw;
            }
        }
    }
}

