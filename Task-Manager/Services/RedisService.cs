using StackExchange.Redis;
using System.Text.Json;

namespace Task_Manager.Services;

public class RedisService : IDisposable
{
    private readonly ConnectionMultiplexer _conn;

    public RedisService(IConfiguration configuration)
    {
        var options = new ConfigurationOptions
        {
            EndPoints = { configuration["Redis:Host"]! },
            User = configuration["Redis:User"],
            Password = configuration["Redis:Password"],
            AbortOnConnectFail = false,
            ConnectRetry = 3,
            ConnectTimeout = 5000
        };

        _conn = ConnectionMultiplexer.Connect(options);
    }

    public IDatabase GetDatabase() => _conn.GetDatabase();

    public bool Ping()
    {
        try
        {
            var latency = _conn.GetDatabase().Ping();
            Console.WriteLine($"Redis ping latency: {latency.TotalNanoseconds}");
            return (true);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Dispose()
    {
        _conn.Dispose();
        GC.SuppressFinalize(this);
    }

    public void AddInRedis(string key, object value, TimeSpan? expiry = null)
    {
        var db = GetDatabase();
        db.StringSet(key, JsonSerializer.Serialize(value), expiry);
    }

    public T? GetFromRedis<T>(string key)
    {
        var db = GetDatabase();
        var value = db.StringGet(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value.ToString()!);
    }

    public void RemoveByPattern(string pattern)
    {
        var endpoints = _conn.GetEndPoints();
        var server = _conn.GetServer(endpoints.First());
        var keys = server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0)
            GetDatabase().KeyDelete(keys);
    }
}