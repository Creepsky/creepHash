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

using System;

namespace MultiCryptoToolLib.Common
{
    public enum Metric : long
    {
        Unit = 1,
        Kilo = 1000,
        Mega = 1000000,
        Giga = 1000000000,
        Tera = 1000000000000
    }

    public struct HashRate
    {
        public double Value { get; }
        public Metric Metric { get; }

        public HashRate(double value, Metric metric)
        {
            Value = value;
            Metric = metric;
        }

        public HashRate(double value)
            : this(value, Metric.Unit)
        {
        }

        public HashRate(double value, string metric)
        {
            Value = value;

            if (metric.Length < 2)
                throw new ArgumentOutOfRangeException($"Unknown metric: {metric}");

            metric = metric.ToLower();

            if (metric.StartsWith("kh"))
                Metric = Metric.Kilo;
            else if (metric.StartsWith("mh"))
                Metric = Metric.Mega;
            else if (metric.StartsWith("gh"))
                Metric = Metric.Giga;
            else if (metric.StartsWith("th"))
                Metric = Metric.Tera;
            else if (metric.StartsWith("h"))
                Metric = Metric.Unit;
            else if (metric.StartsWith("sol"))
                Metric = Metric.Unit;
            else
                throw new ArgumentOutOfRangeException($"Unknown metric: {metric}");
        }

        public HashRate Convert(Metric metric) =>
            metric == Metric ? this : new HashRate(Value * (long) Metric / (long) metric, metric);

        public static implicit operator double(HashRate hashMetric)
        {
            return hashMetric.Value;
        }

        public static implicit operator HashRate(double value)
        {
            return new HashRate(value, Metric.Unit);
        }

        public override string ToString()
        {
            string metric;

            switch (Metric)
            {
                case Metric.Unit:
                    metric = "H";
                    break;
                case Metric.Kilo:
                    metric = "KH";
                    break;
                case Metric.Mega:
                    metric = "MH";
                    break;
                case Metric.Giga:
                    metric = "MH";
                    break;
                case Metric.Tera:
                    metric = "TH";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{Value:F} {metric}";
        }
    }
}
