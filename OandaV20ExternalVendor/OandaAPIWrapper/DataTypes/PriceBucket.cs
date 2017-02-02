// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [DataContract]
    [Serializable]
    internal class PriceBucket
    {
        /// <summary>
        /// The Price offered by the PriceBucket
        /// </summary>
        [DataMember(Name = "price")]
        public double Price;

        /// <summary>
        /// The amount of liquidity offered by the PriceBucket
        /// </summary>
        [DataMember(Name = "liquidity")]
        public int Liquidity;
    }
}
