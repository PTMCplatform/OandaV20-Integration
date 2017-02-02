// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal enum OrderTypeOanda
    {
        [EnumMember(Value = "MARKET")]
        Market,
        [EnumMember(Value = "LIMIT")]
        Limit,
        [EnumMember(Value = "STOP")]
        Stop,
        [EnumMember(Value = "MARKET_IF_TOUCHED")]
        MarketIfTouched,
        [EnumMember(Value = "TAKE_PROFIT")]
        TakeProfit,
        [EnumMember(Value = "STOP_LOSS")]
        StopLoss,
        [EnumMember(Value = "TRAILING_STOP_LOSS")]
        TrailingStopLoss
    }

    [Serializable]
    [DataContract]
    internal enum TimeInForceEnum
    {
        /// <summary>
        /// The Order is “Good unTil Cancelled”
        /// </summary>
        [EnumMember(Value = "GTC")]
        GTC,
        /// <summary>
        /// The Order is “Good unTil Date” and will be cancelled at the provided time
        /// </summary>
        [EnumMember(Value = "GTD")]
        GTD,
        /// <summary>
        /// The Order is “Good For Day” and will be cancelled at 5pm New York time
        /// </summary>
        [EnumMember(Value = "GFD")]
        GFD,
        /// <summary>
        /// The Order must be immediately “Filled Or Killed”
        /// </summary>
        [EnumMember(Value = "FOK")]
        FOC,
        /// <summary>
        /// The Order must be “Immediatedly paritally filled Or Cancelled”
        /// </summary>
        [EnumMember(Value = "IOC")]
        IOC
    }

    [Serializable]
    [DataContract]
    internal class OrderOanda
    {
        [DataMember(Name = "clientExtensions")]
        public ClientExtensions ClientExtensions;

        [DataMember(Name = "createTime")]
        public string CreateTime
        {
            get
            {
                return string.Empty;
            }
            set
            {
                CreationTime = DateTime.UtcNow;
                DateTime.TryParse(value, out CreationTime);
                CreationTime = TimeZoneInfo.ConvertTimeToUtc(CreationTime, TimeZoneInfo.Local);
            }
        }

        public DateTime CreationTime;

        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "instrument")]
        public string InstrumentId;

        [DataMember(Name = "partialFill")]
        public string PartialFill;

        [DataMember(Name = "positionFill")]
        public string PositionFill;

        [DataMember(Name = "price")]
        public double Price;
        
        [DataMember(Name = "distance")]
        public double Distance;

        [DataMember(Name = "replacesOrderID")]
        public string ReplacesOrderID;

        [DataMember(Name = "state")]
        public string State;

        [DataMember(Name = "timeInForce")]
        private string TimeInForceSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.TimeInForce.ToString();
            }
            set
            {
                this.TimeInForce = (TimeInForceEnum)this.TimeInForce.DeserializeFromJson(value);
            }
        }        
        public TimeInForceEnum TimeInForce;
        
        [DataMember(Name = "gtdTime")]
        private string GTDTimeSerialization
        {
            get
            {
                return string.Empty;
            }
            set
            {
                GTDTime = DateTime.UtcNow;
                DateTime.TryParse(value, out GTDTime);
                GTDTime = TimeZoneInfo.ConvertTimeToUtc(GTDTime, TimeZoneInfo.Local);
            }
        }

        public DateTime GTDTime;

        [DataMember(Name = "triggerCondition")]
        public string TriggerCondition;
        
        [DataMember(Name = "type")]
        private string OrderTypeSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.OrderType.ToString();
            }
            set
            {
                this.OrderType = (OrderTypeOanda)this.OrderType.DeserializeFromJson(value);
            }
        }
        public OrderTypeOanda OrderType;

        [DataMember(Name = "units")]
        public int Amount;


        [DataMember(Name = "takeProfitOnFill")]
        public SlTpDetails TakeProfitOnFill;

        [DataMember(Name = "stopLossOnFill")]
        public SlTpDetails StopLossOnFill;

        [DataMember(Name = "trailingStopLossOnFill")]
        public TrailingStopLossDetails TrailingStopLossOnFill;
        
        [DataMember(Name = "tradeID")]
        public string TradeID;
    }
}
