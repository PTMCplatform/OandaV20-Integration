// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    public class CandleBarData
    {
        [DataMember(Name = "o")]
        public double Open;

        [DataMember(Name = "h")]
        public double High;

        [DataMember(Name = "l")]
        public double Low;

        [DataMember(Name = "c")]
        public double Close;
    }
}
