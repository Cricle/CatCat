using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CatCat.Infrastructure.MessageQueue;

public interface IMessageQueueService
{
    Task PublishAsync<T>(string subject, T message, CancellationToken cancellationToken = default);
    Task<IAsyncDisposable> SubscribeAsync<T>(string subject, Func<T, Task> handler, CancellationToken cancellationToken = default);
}

public class JetStreamService : IMessageQueueService
{
    private readonly NatsConnection _connection;
    private readonly INatsJSContext _jsContext;
    private readonly JsonSerializerContext _jsonContext;
    private readonly ILogger<JetStreamService> _logger;

    public JetStreamService(
        NatsConnection connection,
        JsonSerializerContext jsonContext,
        ILogger<JetStreamService> logger)
    {
        _connection = connection;
        _jsContext = new NatsJSContextFactory().CreateContext(_connection);
        _jsonContext = jsonContext;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string subject, T message, CancellationToken cancellationToken = default)
    {
        var typeInfo = (JsonTypeInfo<T>)_jsonContext.GetTypeInfo(typeof(T))!;
        var json = JsonSerializer.Serialize(message, typeInfo);

        var ack = await _jsContext.PublishAsync(subject, json, cancellationToken: cancellationToken);

        if (ack != null)
        {
            _logger.LogDebug("Message published to JetStream: {Subject}, Stream: {Stream}, Seq: {Sequence}",
                subject, ack.Stream, ack.Seq);
        }
    }

    public Task<IAsyncDisposable> SubscribeAsync<T>(string subject, Func<T, Task> handler, CancellationToken cancellationToken = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                var typeInfo = (JsonTypeInfo<T>)_jsonContext.GetTypeInfo(typeof(T))!;

                var consumer = await _jsContext.CreateOrUpdateConsumerAsync(
                    GetStreamName(subject),
                    new ConsumerConfig
                    {
                        DurableName = $"consumer-{subject.Replace(".", "-")}",
                        FilterSubject = subject,
                        AckPolicy = ConsumerConfigAckPolicy.Explicit,
                        DeliverPolicy = ConsumerConfigDeliverPolicy.All
                    },
                    cts.Token);

                await foreach (var msg in consumer.ConsumeAsync<string>(cancellationToken: cts.Token))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(msg.Data))
                        {
                            var data = JsonSerializer.Deserialize(msg.Data, typeInfo);
                            if (data != null)
                            {
                                await handler(data);
                                await msg.AckAsync(cancellationToken: cts.Token);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from {Subject}", subject);
                        await msg.NakAsync(cancellationToken: cts.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription error for {Subject}", subject);
            }
        }, cts.Token);

        return Task.FromResult<IAsyncDisposable>(new JetStreamSubscription(cts));
    }

    private static string GetStreamName(string subject)
    {
        return subject.Split('.')[0].ToUpperInvariant();
    }

    private class JetStreamSubscription : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts;

        public JetStreamSubscription(CancellationTokenSource cts)
        {
            _cts = cts;
        }

        public ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
