// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class RequestError
    {
        [DataMember(Name = "errorCode")]
        public string Code;
        [DataMember(Name = "errorMessage")]
        public string Message;
    }
}
