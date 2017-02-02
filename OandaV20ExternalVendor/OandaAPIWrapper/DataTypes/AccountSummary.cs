// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class AccountSummaryOanda
    {
        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "NAV")]
        public double NAV;

        [DataMember(Name = "alias")]
        public string Name;

        [DataMember(Name = "balance")]
        public double Balance;

        [DataMember(Name = "createdByUserID")]
        public string UserId;

        [DataMember(Name = "createdTime")]
        public string CreatedTime
        {
            get
            {
                return string.Empty;
            }
            set
            {
                CreationTime = DateTime.UtcNow;
                DateTime.TryParse(value, out CreationTime);
                CreationTime = TimeZoneInfo.ConvertTimeToUtc(CreationTime, TimeZoneInfo.Local);
            }
        }

        public DateTime CreationTime;

        [DataMember(Name = "currency")]
        public string Currency;

        [DataMember(Name = "hedgingEnabled")]
        public bool AllowHedging;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;

        #region Margin

        [DataMember(Name = "marginAvailable")]
        public double MarginAvaliable;

        [DataMember(Name = "marginCloseoutMarginUsed")]
        public double MarginCloseoutMarginUsed;

        [DataMember(Name = "marginCloseoutNAV")]
        public double MarginCloseoutNAV;

        [DataMember(Name = "marginCloseoutPercent")]
        public double MarginCloseoutPercent;

        [DataMember(Name = "marginCloseoutPositionValue")]
        public double MarginCloseoutPositionValue;

        [DataMember(Name = "marginCloseoutUnrealizedPL")]
        public double MarginCloseoutUnrealizedPL;

        [DataMember(Name = "marginRate")]
        public double MarginRate;

        [DataMember(Name = "marginUsed")]
        public double MarginUsed;

        #endregion Margin

        [DataMember(Name = "openPositionCount")]
        public long OpenPositionCount;

        [DataMember(Name = "openTradeCount")]
        public long OpenTradeCount;

        [DataMember(Name = "pendingOrderCount")]
        public long PendingOrderCount;

        [DataMember(Name = "pl")]
        public double Pl;

        [DataMember(Name = "positionValue")]
        public double PositionValue;

        [DataMember(Name = "resettablePL")]
        public double ResettablePL;

        [DataMember(Name = "unrealizedPL")]
        public double UnrealizedPL;

        [DataMember(Name = "withdrawalLimit")]
        public double WithdrawalLimit;
    }
}
