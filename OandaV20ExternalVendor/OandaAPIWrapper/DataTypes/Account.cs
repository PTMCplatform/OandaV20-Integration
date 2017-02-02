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
    internal class AccountOanda
    {
        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "tags")]
        public List<string> Tags;
    }
}
