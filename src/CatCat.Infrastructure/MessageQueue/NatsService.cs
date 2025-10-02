using NATS.Client.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CatCat.Infrastructure.MessageQueue;

public interface IMessageQueueService
{
    Task PublishAsync<T>(string subject, T message, CancellationToken cancellationToken = default);
    Task<IAsyncDisposable> SubscribeAsync<T>(string subject, Func<T, Task> handler, CancellationToken cancellationToken = default);
}

/// <summary>
/// NATS 消息队列服务 - 使用 System.Text.Json 源生成，AOT 兼容
/// </summary>
public class NatsService : IMessageQueueService
{
    private readonly NatsConnection _connection;
    private readonly JsonSerializerContext _jsonContext;

    public NatsService(NatsConnection connection, JsonSerializerContext jsonContext)
    {
        _connection = connection;
        _jsonContext = jsonContext;
    }

    /// <summary>
    /// 发布消息（使用源生成的 JSON 序列化）
    /// </summary>
    public async Task PublishAsync<T>(string subject, T message, CancellationToken cancellationToken = default)
    {
        // 使用源生成的 TypeInfo 进行序列化
        var typeInfo = (JsonTypeInfo<T>)_jsonContext.GetTypeInfo(typeof(T))!;
        var json = JsonSerializer.Serialize(message, typeInfo);
        await _connection.PublishAsync(subject, json, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 订阅消息（使用源生成的 JSON 反序列化）
    /// </summary>
    public Task<IAsyncDisposable> SubscribeAsync<T>(string subject, Func<T, Task> handler, CancellationToken cancellationToken = default)
    {
        // 创建取消令牌源以控制订阅
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 后台任务处理消息
        _ = Task.Run(async () =>
        {
            try
            {
                // 获取源生成的 TypeInfo
                var typeInfo = (JsonTypeInfo<T>)_jsonContext.GetTypeInfo(typeof(T))!;

                await foreach (var msg in _connection.SubscribeAsync<string>(subject, cancellationToken: cts.Token))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(msg.Data))
                        {
                            // 使用源生成的 TypeInfo 进行反序列化
                            var data = JsonSerializer.Deserialize(msg.Data, typeInfo);
                            if (data != null)
                            {
                                await handler(data);
                            }
                        }
                    }
                    catch
                    {
                        // 忽略反序列化错误
                    }
                }
            }
            catch
            {
                // 订阅被取消或连接关闭
            }
        }, cts.Token);

        // 返回一个可dispose的包装器
        return Task.FromResult<IAsyncDisposable>(new NatsSubscription(cts));
    }

    private class NatsSubscription : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts;

        public NatsSubscription(CancellationTokenSource cts)
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
