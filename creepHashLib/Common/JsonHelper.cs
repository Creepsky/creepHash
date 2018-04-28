/*
 * Copyright 2018 Creepsky
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using creepHashLib.Mining.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace creepHashLib.Common
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