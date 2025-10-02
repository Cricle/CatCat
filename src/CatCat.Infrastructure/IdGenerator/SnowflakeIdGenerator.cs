using Yitter.IdGenerator;

namespace CatCat.Infrastructure.IdGenerator;

/// <summary>
/// 雪花漂移 ID 生成器配置 - 直接使用 Yitter.IdGenerator
/// 优势：
/// 1. 性能极高，单机每秒可生成 50万+ ID
/// 2. 趋势递增，适合数据库主键
/// 3. 漂移算法，避免时钟回拨问题
/// 4. 支持分布式，可配置 WorkerId
/// </summary>
public static class SnowflakeIdGenerator
{
    private static bool _isInitialized = false;
    private static readonly object _lock = new();

    /// <summary>
    /// 初始化雪花ID生成器（启动时调用一次）
    /// </summary>
    public static void Initialize(ushort workerId = 1, byte workerIdBitLength = 6)
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            var options = new IdGeneratorOptions
            {
                WorkerId = workerId,                    // 机器ID（0-63）
                WorkerIdBitLength = workerIdBitLength,  // 机器ID位长度（默认6位）
                SeqBitLength = 6,                       // 序列号位长度（默认6位）
                DataCenterIdBitLength = 0,              // 数据中心ID位长度（默认0）
                BaseTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MinSeqNumber = 5,
                MaxSeqNumber = 0,
                TopOverCostCount = 2000
            };

            YitIdHelper.SetIdGenerator(options);
            _isInitialized = true;
        }
    }
}

/// <summary>
/// ID 生成器扩展方法
/// </summary>
public static class IdGeneratorExtensions
{
    /// <summary>
    /// 解析雪花ID的时间戳
    /// </summary>
    public static DateTime GetTimestamp(this long snowflakeId)
    {
        // 雪花ID结构：1位符号 + 41位时间戳 + 其他位
        var timestamp = snowflakeId >> 22; // 右移22位（6+6+10）得到时间戳
        var baseTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return baseTime.AddMilliseconds(timestamp);
    }

    /// <summary>
    /// 解析雪花ID的机器ID
    /// </summary>
    public static int GetWorkerId(this long snowflakeId)
    {
        // 提取机器ID（右移6位，然后与掩码做AND运算）
        return (int)((snowflakeId >> 6) & 0x3F); // 0x3F = 63 (6位)
    }

    /// <summary>
    /// 解析雪花ID的序列号
    /// </summary>
    public static int GetSequence(this long snowflakeId)
    {
        // 提取序列号（最右边6位）
        return (int)(snowflakeId & 0x3F); // 0x3F = 63 (6位)
    }
}

