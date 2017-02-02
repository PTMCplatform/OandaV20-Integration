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
    public class TransactionsPage
    {
        [DataMember(Name = "count")]
        public int Count;

        [DataMember(Name = "from")]
        string FromSerialization
        {
            get { return string.Empty; }
            set
            {
                From = DateTime.UtcNow;
                DateTime.TryParse(value, out From);
                From = TimeZoneInfo.ConvertTimeToUtc(From, TimeZoneInfo.Local);
            }
        }
        public DateTime From;

        [DataMember(Name = "to")]
        string ToSerialization
        {
            get { return string.Empty; }
            set
            {
                To = DateTime.UtcNow;
                DateTime.TryParse(value, out To);
                To = TimeZoneInfo.ConvertTimeToUtc(To, TimeZoneInfo.Local);
            }
        }
        public DateTime To;

        [DataMember(Name = "lastTransactionID")]
        public string LastTransactionID;

        [DataMember(Name = "pageSize")]
        public int PageSize;

        [DataMember(Name = "pages")]
        public List<string> Pages;
    }
}
