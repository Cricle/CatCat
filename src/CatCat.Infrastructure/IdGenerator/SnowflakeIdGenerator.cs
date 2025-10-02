using Yitter.IdGenerator;

namespace CatCat.Infrastructure.IdGenerator;

public static class SnowflakeIdGenerator
{
    private static bool _isInitialized = false;
    private static readonly object _lock = new();

    public static void Initialize(ushort workerId = 1, byte workerIdBitLength = 6)
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            var options = new IdGeneratorOptions
            {
                WorkerId = workerId,
                WorkerIdBitLength = workerIdBitLength,
                SeqBitLength = 6,
                DataCenterIdBitLength = 0,
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

public static class IdGeneratorExtensions
{
    public static DateTime GetTimestamp(this long snowflakeId)
    {
        var timestamp = snowflakeId >> 22;
        var baseTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return baseTime.AddMilliseconds(timestamp);
    }

    public static int GetWorkerId(this long snowflakeId)
    {
        return (int)((snowflakeId >> 6) & 0x3F);
    }

    public static int GetSequence(this long snowflakeId)
    {
        return (int)(snowflakeId & 0x3F);
    }
}

