using CatCat.Transit.Configuration;
using CatCat.Transit.DeadLetter;
using CatCat.Transit.Handlers;
using CatCat.Transit.Idempotency;
using CatCat.Transit.Messages;
using CatCat.Transit.Pipeline.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NATS.Client.Core;

namespace CatCat.Transit.Nats.DependencyInjection;

/// <summary>
/// NATS Transit service registration with full Pipeline Behaviors support (AOT-compatible)
/// </summary>
public static class NatsTransitServiceCollectionExtensions
{
    /// <summary>
    /// Add NATS Transit with full features
    /// </summary>
    public static IServiceCollection AddNatsTransit(
        this IServiceCollection services,
        string natsUrl,
        Action<TransitOptions>? configureOptions = null)
    {
        var options = new TransitOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);

        // Register NATS connection
        services.AddSingleton<INatsConnection>(sp =>
        {
            var opts = NatsOpts.Default with { Url = natsUrl };
            return new NatsConnection(opts);
        });

        // Register NATS mediator
        services.TryAddSingleton<ITransitMediator, NatsTransitMediator>();

        // Idempotency store (shared for subscriber side)
        services.TryAddSingleton<IIdempotencyStore>(sp =>
            new ShardedIdempotencyStore(
                options.IdempotencyShardCount,
                TimeSpan.FromHours(options.IdempotencyRetentionHours)));

        // Dead letter queue
        if (options.EnableDeadLetterQueue)
        {
            services.TryAddSingleton<IDeadLetterQueue>(sp =>
                new InMemoryDeadLetterQueue(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InMemoryDeadLetterQueue>>(),
                    options.DeadLetterQueueMaxSize));
        }

        return services;
    }

    /// <summary>
    /// Subscribe to NATS requests with full Pipeline support
    /// </summary>
    public static IServiceCollection SubscribeToNatsRequest<TRequest, TResponse>(
        this IServiceCollection services)
        where TRequest : IRequest<TResponse>
    {
        services.AddSingleton<NatsRequestSubscriber<TRequest, TResponse>>();
        return services;
    }

    /// <summary>
    /// Subscribe to NATS events
    /// </summary>
    public static IServiceCollection SubscribeToNatsEvent<TEvent>(
        this IServiceCollection services)
        where TEvent : IEvent
    {
        services.AddSingleton<NatsEventSubscriber<TEvent>>();
        return services;
    }
}
