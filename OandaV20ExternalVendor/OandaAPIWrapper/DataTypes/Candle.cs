// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class Candle
    {
        [DataMember(Name = "ask")]
        public CandleBarData Ask;
        
        [DataMember(Name = "bid")]
        public CandleBarData Bid;
        
        [DataMember(Name = "mid")]
        public CandleBarData Mid;

        [DataMember(Name = "complete")]
        public bool Complete;

        [DataMember(Name = "time")]
        private string TimeSerialization
        {
            get
            {
                return string.Empty;
            }
            set
            {
                Time = DateTime.UtcNow;
                DateTime.TryParse(value, out Time);
                Time = TimeZoneInfo.ConvertTimeToUtc(Time, TimeZoneInfo.Local);
            }
        }
        public DateTime Time;


        [DataMember(Name = "volume")]
        public double Volume;
    }
}
