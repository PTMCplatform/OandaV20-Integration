// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class MarketOrderTradeClose
    {
        [DataMember(Name = "tradeID")]
        public string TradeId;

        [DataMember(Name = "units")]
        public string Amount;

        [DataMember(Name = "clientTradeID")]
        public string ClientTradeID;
    }
}
