namespace Task_Manager.Helpers;

public static class CacheKeyHelper
{
    public static string BuildFilterKey(string prefix, object filterDto)
    {
        var props = filterDto.GetType()
            .GetProperties()
            .Where(p => p.GetValue(filterDto) != null)
            .Select(p => $"{p.Name}={p.GetValue(filterDto)}");

        return $"{prefix}:{string.Join("_", props)}";
    }
}