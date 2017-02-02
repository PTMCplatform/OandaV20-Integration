// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class SlTpDetails : CloseOrderDetails
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
                Price = double.Parse(value.Replace(".", ","));
            }
        }
        public double Price;
    }
}
