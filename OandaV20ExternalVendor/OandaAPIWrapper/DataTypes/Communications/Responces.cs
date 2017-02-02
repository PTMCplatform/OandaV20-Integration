// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class AccountChangesResponce
    {
        [DataMember(Name = "state")]
        public AccountState State;

        [DataMember(Name = "lastTransactionID")]
        public string LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class AccountDetailsResponce
    {
        [DataMember(Name = "account")]
        public AccountDetails Account;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class AccountsResponce
    {
        [DataMember(Name = "accounts")]
        public List<AccountOanda> Accounts;
    }

    [Serializable]
    [DataContract]
    internal class AccountSummaryResponce
    {
        [DataMember(Name = "account")]
        public AccountSummaryOanda Account;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class CandlesResponse
    {
        [DataMember(Name = "candles")]
        public List<Candle> Candles;

        [DataMember(Name = "granularity")]
        private string GranularitySerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.Granularity.ToString();
            }
            set
            {
                this.Granularity.DeserializeFromJson(value);
            }
        }
        public GranularityEnum Granularity;
    }

    [Serializable]
    [DataContract]
    public enum GranularityEnum
    {
        [EnumMember(Value = "S5")]
        S5,
        [EnumMember(Value = "S10")]
        S10,
        [EnumMember(Value = "S15")]
        S15,
        [EnumMember(Value = "S30")]
        S30,
        [EnumMember(Value = "M1")]
        M1,
        [EnumMember(Value = "M2")]
        M2,
        [EnumMember(Value = "M3")]
        M3,
        [EnumMember(Value = "M4")]
        M4,
        [EnumMember(Value = "M5")]
        M5,
        [EnumMember(Value = "M10")]
        M10,
        [EnumMember(Value = "M15")]
        M15,
        [EnumMember(Value = "M30")]
        M30,
        [EnumMember(Value = "H1")]
        H1,
        [EnumMember(Value = "H2")]
        H2,
        [EnumMember(Value = "H3")]
        H3,
        [EnumMember(Value = "H4")]
        H4,
        [EnumMember(Value = "H6")]
        H6,
        [EnumMember(Value = "H8")]
        H8,
        [EnumMember(Value = "H12")]
        H12,
        [EnumMember(Value = "D")]
        D,
        [EnumMember(Value = "W")]
        W,
        [EnumMember(Value = "M")]
        M
    }

    [Serializable]
    [DataContract]
    internal class InstrumentsResponce
    {
        [DataMember(Name = "instruments")]
        public List<InstrumentOanda> Instruments;
    }

    [Serializable]
    [DataContract]
    internal class OrderCreateResponce
    {
        /// <summary>
        /// The Transaction that created the Order specified by the request.
        /// </summary>
        [DataMember(Name = "orderCreateTransaction")]
        public Transaction OrderCreateTransaction;

        /// <summary>
        /// The Transaction that filled the newly created Order. Only provided when the Order was immediately filled.
        /// </summary>
        [DataMember(Name = "orderFillTransaction")]
        public Transaction OrderFillTransaction;

        /// <summary>
        /// The Transaction that cancelled the newly created Order. Only provided when the Order was immediately cancelled.
        /// </summary>
        [DataMember(Name = "orderCancelTransaction")]
        public Transaction OrderCancelTransaction;

        /// <summary>
        /// The Transaction that reissues the Order. Only provided when the Order is configured to be reissued for its remaining units after a partial fill and the reissue was successful.
        /// </summary>
        [DataMember(Name = "orderReissueTransaction")]
        public Transaction OrderReissueTransaction;

        /// <summary>
        /// The Transaction that rejects the reissue of the Order. Only provided when the Order is configured to be reissued for its remaining units after a partial fill and the reissue was rejected.
        /// </summary>
        [DataMember(Name = "orderReissueRejectTransaction")]
        public Transaction OrderReissueRejectTransaction;

        /// <summary>
        /// The IDs of all Transactions that were created while satisfying the request.
        /// </summary>
        [DataMember(Name = "relatedTransactionIDs")]
        public List<string> RelatedTransactionIDs;

        /// <summary>
        /// The ID of the most recent Transaction created for the Account
        /// </summary>
        [DataMember(Name = "lastTransactionID")]
        public string LastTransactionID;
    }

    [Serializable]
    [DataContract]
    internal class OrdersResponce
    {
        [DataMember(Name = "orders")]
        public List<OrderOanda> Orders;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class SingleOrderResponce
    {
        [DataMember(Name = "order")]
        public OrderOanda Order;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }


    [Serializable]
    [DataContract]
    internal class SingleTradeResponce
    {
        [DataMember(Name = "trade")]
        public TradeOanda Trade;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class TradeCloseResponce
    {
        [DataMember(Name = "orderCreateTransaction")]
        public Transaction OrderCreateTransaction;

        [DataMember(Name = "orderFillTransaction")]
        public Transaction OrderFillTransaction;

        [DataMember(Name = "orderCancelTransaction")]
        public Transaction OrderCancelTransaction;

        [DataMember(Name = "orderCancelTransaction")]
        public List<string> RelatedTransactionIds;

        [DataMember(Name = "lastTransactionID")]
        public string LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class TradesResponce
    {
        [DataMember(Name = "trades")]
        public List<TradeOanda> Trades;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }

    [Serializable]
    [DataContract]
    internal class TransactionsResponce
    {
        [DataMember(Name = "transactions")]
        public List<Transaction> Transactions;

        [DataMember(Name = "lastTransactionID")]
        public long LastTransactionId;
    }
}
