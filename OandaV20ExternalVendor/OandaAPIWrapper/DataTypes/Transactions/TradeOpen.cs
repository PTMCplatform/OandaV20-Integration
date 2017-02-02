// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class TradeOpen
    {
        [DataMember(Name = "tradeID")]
        public string TradeId;
        
        [DataMember(Name = "units")]
        public double Amount;
        
        [DataMember(Name = "clientExtensions")]
        public ClientExtensions TradeClientExtensions;
    }
}
