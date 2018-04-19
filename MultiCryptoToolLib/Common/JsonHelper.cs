using System.Text;
using MultiCryptoToolLib.Mining;
using MultiCryptoToolLib.Mining.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Common
{
    public static class JsonHelper
    {
        public static JObject ToJson(this Hardware hardware) => new JObject
        {
            new JProperty("platformIndex", hardware.PlatformIndex),
            new JProperty("index", hardware.Index),
            new JProperty("platform", hardware.Platform),
            new JProperty("name", hardware.Name),
            new JProperty("type", hardware.Type),
        };

        public static JProperty ToJson(this string algorithm, HashRate hashRate) =>
            new JProperty(algorithm, hashRate.Convert(Metric.Unit).Value);

        public static void FromJson(JToken json, out Hardware hardware)
        {
            var jsonObject = (JObject) json;

            hardware = new Hardware
            {
                PlatformIndex = jsonObject.Value<int>("platformIndex"),
                Index = jsonObject.Value<int>("index"),
                Platform = jsonObject.Value<string>("platform"),
                Name = jsonObject.Value<string>("name"),
                Type = (HardwareType)jsonObject.Value<int>("type"),
            };
        }

        public static void FromJson(JToken json, out string algorithm, out HashRate hashRate)
        {
            var jsonProperty = (JProperty)json;
            algorithm = jsonProperty.Name;
            hashRate = new HashRate(jsonProperty.Value.Value<double>(), Metric.Unit);
        }

        public static byte[] ToBytes(this JToken json, Formatting formatting = Formatting.None) =>
            Encoding.UTF8.GetBytes(json.ToString(formatting));
    }
}