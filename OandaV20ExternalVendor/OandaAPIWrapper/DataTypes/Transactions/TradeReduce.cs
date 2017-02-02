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
    internal class TradeReduce
    {
        [DataMember(Name = "tradeID")]
        public string TradeId;

        [DataMember(Name = "units")]
        public double Amount;
        
        [DataMember(Name = "realizedPL")]
        public double RealizedPL;
        
        [DataMember(Name = "financing")]
        public double Financing;
    }
}
