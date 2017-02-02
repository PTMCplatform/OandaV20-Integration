// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    /// <summary>
    /// The status of the Price.
    /// </summary>
    internal enum PriceStatus
    {
        /// <summary>
        /// The Instrument’s price is tradeable.
        /// </summary>
        [EnumMember(Value = "tradeable")]
        Tradeable,
        /// <summary>
        /// The Instrument’s price is not tradeable.
        /// </summary>
        [EnumMember(Value = "non-tradeable")]
        NonTradeable,
        /// <summary>
        /// The Instrument of the price is invalid or there is no valid Price for the Instrument.
        /// </summary>
        [EnumMember(Value = "invalid")]
        Invalid
    }

    [DataContract]
    [Serializable]
    internal class Price : IHeartbeat
    {
        [DataMember(Name = "type")]
        public string PriceType;

        [DataMember(Name = "instrument")]
        public string InstrumentId;

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

        [DataMember(Name = "status")]
        private string StatusSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.Status.ToString();
            }
            set
            {
                this.Status.DeserializeFromJson(value);
            }
        }
        public PriceStatus Status;
        
        [DataMember(Name = "bids")]
        public List<PriceBucket> Bids;
        
        [DataMember(Name = "asks")]
        public List<PriceBucket> Asks;
        
        [DataMember(Name = "closeoutBid")]
        public double CloseoutBid;

        [DataMember(Name = "closeoutAsk")]
        public double CloseoutAsk;

        public bool IsHeartbeat()
        {
            return PriceType == "HEARTBEAT";
        }
    }
}
