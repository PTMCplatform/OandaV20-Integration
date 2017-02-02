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
    internal class AccountDetails : AccountSummaryOanda
    {
        ////list orders
        //[DataMember(Name = "orders")]
        //public List<order> Orders;

        ////list positions
        //[DataMember(Name = "positions")]
        //public List<position> Positions;

        ////list trades
        //[DataMember(Name = "positions")]
        //public List<Trade> trades;
    }
}
