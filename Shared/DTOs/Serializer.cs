using Shared.Exceptions;
using System.Text;
using System.Text.Json;

namespace Shared.DTOs
{
    public static class Serializer
    {
        public static byte[] Serialize<T>(T obj)
        {
            string jsonString = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public static T Deserialize<T>(byte[] data)
        {
            try
            {
                string jsonString = Encoding.UTF8.GetString(data);
                T? obj = JsonSerializer.Deserialize<T>(jsonString);

                if (obj == null)
                {
                    throw new InvalidPacketException($"Deserialized payload for {typeof(T).Name} was null.");
                }

                return obj;
            }
            catch (JsonException ex)
            {
                throw new InvalidPacketException($"Failed to deserialize {typeof(T).Name} \n {ex.Message}");
            }
        }
    }
}
