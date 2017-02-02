// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class CalculatedTradeState
    {
        [DataMember(Name = "id")]
        public string TradeId;

        [DataMember(Name = "unrealizedPL")]
        public double UnrealizedPL;
    }
}
