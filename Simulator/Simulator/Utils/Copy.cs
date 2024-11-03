using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simulator.Utils
{
    public static class Copy
    {
        public static T? DeepCopy<T>(T obj)
        {
            if (obj == null) return default;
            if (obj.Equals(default)) return default;

            var serializeObj = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(serializeObj);
        }
    }
}
