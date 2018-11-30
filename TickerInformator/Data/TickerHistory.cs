using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TickerInformator
{
    public partial class TickerHistory
    {
        [JsonProperty("Response")]
        public string Response { get; set; }

        [JsonProperty("Type")]
        public long Type { get; set; }

        [JsonProperty("Aggregated")]
        public bool Aggregated { get; set; }

        [JsonProperty("Data")]
        public Datum[] Data { get; set; }

        [JsonProperty("TimeTo")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TimeTo { get; set; }

        [JsonProperty("TimeFrom")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TimeFrom { get; set; }

        [JsonProperty("FirstValueInArray")]
        public bool FirstValueInArray { get; set; }

        [JsonProperty("ConversionType")]
        public ConversionType ConversionType { get; set; }

        [JsonProperty("HasWarning")]
        public bool HasWarning { get; set; }
    }

    public partial class ConversionType
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("conversionSymbol")]
        public string ConversionSymbol { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("time")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time { get; set; }

        [JsonProperty("close")]
        public decimal Close { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("open")]
        public decimal Open { get; set; }

        [JsonProperty("volumefrom")]
        public decimal VolumeFrom { get; set; }

        [JsonProperty("volumeto")]
        public decimal VolumeTo { get; set; }
    }
}