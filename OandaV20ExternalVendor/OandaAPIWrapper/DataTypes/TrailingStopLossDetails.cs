// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class TrailingStopLossDetails : CloseOrderDetails
    {
        [DataMember(Name = "distance")]
        private string PriceSerialize
        {
            get
            {
                return Distance.ToString();//.Replace(",", ".");
            }
            set
            {
                Distance = double.Parse(value.Replace(".", ","));
            }
        }
        public double Distance;
    }
}
