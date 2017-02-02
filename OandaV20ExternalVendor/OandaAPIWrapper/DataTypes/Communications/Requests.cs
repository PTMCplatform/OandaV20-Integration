// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [System.Reflection.Obfuscation(ApplyToMembers = true, Exclude = true)]
    internal class CandlesRequest
    {
        const string OandaDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";

        /// <summary>
        /// Instrument to get candlestick data for [required=]
        /// </summary>
        [NonSerialized]
        public string InstrumentId;
        /// <summary>
        /// The Price component(s) to get candlestick data for. Can contain any combination of the characters “M” (midpoint candles) “B” (bid candles) and “A” (ask candles). [default=M]
        /// </summary>
        public string price;
        /// <summary>
        /// The granularity of the candlesticks to fetch [default=S5]
        /// </summary>
        public GranularityEnum? granularity;

        /// <summary>
        /// The number of candlesticks to return in the reponse. Count should not be specified if both the start and end parameters are provided, as the time range combined with the graularity will determine the number of candlesticks to return. [default=500, maximum=5000]
        /// </summary>
        public int? count;
        /// <summary>
        /// The start of the time range to fetch candlesticks for.
        /// </summary>
        public DateTime? from;
        /// <summary>
        /// The end of the time range to fetch candlesticks for.
        /// </summary>
        public DateTime? to;
        /// <summary>
        ///A flag that controls whether the candlestick is “smoothed” or not.A smoothed candlestick uses the previous candle’s close price as its open price, while an unsmoothed candlestick uses the first price from its time range as its open price. [default= False]
        /// </summary>
        public bool? smooth;
        /// <summary>
        /// A flag that controls whether the candlestick that is covered by the from time should be included in the results. This flag enables clients to use the timestamp of the last completed candlestick received to poll for future candlesticks but avoid receiving the previous candlestick repeatedly. [default=True]
        /// </summary>
        public bool? includeFirst;
        /// <summary>
        /// The hour of the day (in the specified timezone) to use for granularities that have daily alignments. [default=17, minimum=0, maximum=23]
        /// </summary>
        public int? dailyAlignment;
        /// <summary>
        /// The timezone to use for the dailyAlignment parameter. Candlesticks with daily alignment will be aligned to the dailyAlignment hour within the alignmentTimezone. [default=America/New_York]
        /// </summary>
        public string alignmentTimezone;
        /// <summary>
        /// The day of the week used for granularities that have weekly alignment. [default=Friday]
        /// </summary>
        public DayOfWeek? weeklyAlignment;

        public string GetRequestString()
        {
            var result = new StringBuilder();
            result.Append("instruments/");
            result.Append(InstrumentId);
            result.Append("/candles");
            bool firstJoin = true;
            foreach (var declaredField in this.GetType().GetFields())
            {
                if (Attribute.IsDefined(declaredField, typeof(NonSerializedAttribute)))
                    continue;

                var prop = declaredField.GetValue(this);
                if (prop == null)
                    continue;

                if (firstJoin)
                {
                    result.Append("?");
                    firstJoin = false;
                }
                else
                {
                    result.Append("&");
                }

                var dt = prop as DateTime?;
                if (dt != null && dt.HasValue)
                    result.Append(declaredField.Name + "=" + Uri.EscapeDataString(OandaV20Utils.ConvertDateTimeToOandaFormat(dt.Value)));
                else
                    result.Append(declaredField.Name + "=" + Uri.EscapeDataString(prop.ToString()));
            }
            return result.ToString();
        }
    }

    [Serializable]
    [DataContract]
    internal abstract class CloseOrderRequestBase : OrderRequestBase
    {
        [DataMember(Name = "tradeID")]
        public string TradeID;
    }

    [Serializable]
    [DataContract]
    internal class CloseTradeRequest
    {
        [DataMember(Name = "units")]
        private string AmountSerialize
        {
            get
            {
                return Amount != 0 ? Amount.ToString() : "ALL";
            }
            set
            { }
        }

        public double Amount;
    }

    [Serializable]
    [DataContract]
    internal class LimitOrderRequest : MainOrderRequestBase
    {
        [DataMember(Name = "price")]
        private string PriceSerialize
        {
            get
            {
                return Price.ToString();//.Replace(",", ".");
            }
            set
            {
            }
        }
        public double Price;

        public LimitOrderRequest()
        {
            OrderType = OrderTypeOanda.Limit;
        }
    }

    [Serializable]
    [DataContract]
    internal abstract class MainOrderRequestBase : OrderRequestBase
    {
        [DataMember(Name = "instrument")]
        public string InstrumentId;

        [DataMember(Name = "units")]
        private string units
        {
            get
            {
                return Amount.ToString();
            }
            set
            {
            }
        }
        public int Amount;

        [DataMember(Name = "positionFill", EmitDefaultValue = false)]
        public string PositionFill;

        [DataMember(Name = "tradeClientExtensions", EmitDefaultValue = false)]
        public ClientExtensions TradeClientExtensions;

        [DataMember(Name = "takeProfitOnFill", EmitDefaultValue = false)]
        public SlTpDetails TakeProfitOnFill;

        [DataMember(Name = "stopLossOnFill", EmitDefaultValue = false)]
        public SlTpDetails StopLossOnFill;

        [DataMember(Name = "trailingStopLossOnFill", EmitDefaultValue = false)]
        public TrailingStopLossDetails TrailingStopLossOnFill;

        public MainOrderRequestBase()
        {
            PositionFill = "DEFAULT";
        }
    }

    [Serializable]
    [DataContract]
    internal class MarketOrderRequest : MainOrderRequestBase
    {
        [DataMember(Name = "priceBound", EmitDefaultValue = false)]
        private string PriceBoundSerialize
        {
            get
            {
                return PriceBound > 0 ? PriceBound.ToString() : null;
            }
            set
            {
            }
        }
        public double PriceBound;

        public MarketOrderRequest()
        {
            OrderType = OrderTypeOanda.Market;
        }

    }

    [Serializable]
    [DataContract]
    internal abstract class OrderRequestBase
    {
        [DataMember(Name = "type")]
        private string OrderTypeSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.OrderType.SerializeToJson();
            }
            set
            {
                this.OrderType.DeserializeFromJson(value);
            }
        }
        public OrderTypeOanda OrderType;

        [DataMember(Name = "timeInForce", EmitDefaultValue = false)]
        private string TimeInForceSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.TimeInForce.SerializeToJson();
            }
            set
            {
                this.TimeInForce = (TimeInForceEnum)this.TimeInForce.DeserializeFromJson(value);
            }
        }
        public TimeInForceEnum TimeInForce;

        [DataMember(Name = "gtdTime", EmitDefaultValue = false)]
        private string GTDTimeSerialization
        {
            get
            {
                return OandaV20Utils.ConvertDateTimeToOandaFormat(GTDTime);
            }
            set
            {
                GTDTime = DateTime.UtcNow;
                DateTime.TryParse(value, out GTDTime);
                GTDTime = TimeZoneInfo.ConvertTimeToUtc(GTDTime, TimeZoneInfo.Local);
            }
        }
        public DateTime GTDTime;

        [DataMember(Name = "clientExtensions", EmitDefaultValue = false)]
        public ClientExtensions ClientExtensions;

    }

    [Serializable]
    [DataContract]
    [KnownType(typeof(MainOrderRequestBase))]
    [KnownType(typeof(MarketOrderRequest))]
    [KnownType(typeof(LimitOrderRequest))]
    [KnownType(typeof(StopOrderRequest))]
    [KnownType(typeof(TakeProfitOrderRequest))]
    [KnownType(typeof(StopLossOrderRequest))]
    [KnownType(typeof(TrailingStopLossOrderRequest))]
    internal class PlaceOrderRequest
    {
        [DataMember(Name = "order")]
        public OrderRequestBase Order;
    }

    [Serializable]
    [DataContract]
    internal class StopLossOrderRequest : TakeProfitOrderRequest
    {
        public StopLossOrderRequest()
        {
            this.OrderType = OrderTypeOanda.StopLoss;
        }
    }

    [Serializable]
    [DataContract]
    class StopOrderRequest : LimitOrderRequest
    {
        [DataMember(Name = "priceBound", EmitDefaultValue = false)]
        private string PriceBoundSerialize
        {
            get
            {
                return PriceBound > 0 ? PriceBound.ToString() : null;
            }
            set
            {
            }
        }
        public double PriceBound;

        public StopOrderRequest()
        {
            this.OrderType = OrderTypeOanda.Stop;
        }
    }

    [Serializable]
    [DataContract]
    internal class TakeProfitOrderRequest : CloseOrderRequestBase
    {
        [DataMember(Name = "price")]
        private string PriceSerialize
        {
            get
            {
                return Price.ToString();//.Replace(",", ".");
            }
            set
            {
            }
        }
        public double Price;

        public TakeProfitOrderRequest()
        {
            this.OrderType = OrderTypeOanda.TakeProfit;
        }
    }

    [Serializable]
    [DataContract]
    internal class TrailingStopLossOrderRequest : CloseOrderRequestBase
    {
        [DataMember(Name = "distance")]
        private string DistanceSerialize
        {
            get
            {
                return Distance.ToString();//.Replace(",", ".");
            }
            set
            {
            }
        }
        public double Distance;

        public TrailingStopLossOrderRequest()
        {
            this.OrderType = OrderTypeOanda.TrailingStopLoss;
        }
    }
}
