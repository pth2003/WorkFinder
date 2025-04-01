using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WorkFinder.Web.Extensions
{
    public static class SessionExtensions
    {
        public static async Task SetObjectAsync<T>(this ISession session, string key, T value)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            string serializedData = JsonSerializer.Serialize(value, options);
            session.SetString(key, serializedData);
            await Task.CompletedTask; // To make it an async method
        }

        public static async Task<T> GetObjectAsync<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            string serializedData = JsonSerializer.Serialize(value, options);
            session.SetString(key, serializedData);
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }
    }
}