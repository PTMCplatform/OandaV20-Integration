// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class ClientExtensions
    {
        [DataMember(Name = "comment")]
        public string Comment;

        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "tag")]
        public string Tag;
    }
}
