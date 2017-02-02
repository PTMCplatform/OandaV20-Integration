// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal enum TransactionType
    {
        /// <summary>
        /// Heart beat
        /// </summary>
        [EnumMember(Value = "HEARTBEAT")]
        HeartBeat,
        /// <summary>
        /// Account Create Transaction
        /// </summary>
        [EnumMember(Value = "CREATE")]
        CreateAccount,
        /// <summary>
        /// Account Close Transaction
        /// </summary>
        [EnumMember(Value = "CLOSE")]
        CloseAccount,
        /// <summary>
        /// Account Reopen Transaction
        /// </summary>
        [EnumMember(Value = "REOPEN")]
        ReopenAccount,
        /// <summary>
        /// Client Configuration Transaction
        /// </summary>
        [EnumMember(Value = "CLIENT_CONFIGURE")]
        ClientConfigure,
        /// <summary>
        /// Transfer Funds Transaction
        /// </summary>
        [EnumMember(Value = "TRANSFER_FUNDS")]
        TransferFunds,
        /// <summary>
        /// Transfer Funds Reject Transaction
        /// </summary>
        [EnumMember(Value = "TRANSFER_FUNDS_REJECT")]
        TransferFundsReject,
        /// <summary>
        /// Market Order Transaction
        /// </summary>
        [EnumMember(Value = "MARKET_ORDER")]
        MarketOrder,
        /// <summary>
        /// Market Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "MARKET_ORDER_REJECT")]
        MarketOrderReject,
        /// <summary>
        /// Limit Order Transaction
        /// </summary>
        [EnumMember(Value = "LIMIT_ORDER")]
        LimitOrder,
        /// <summary>
        /// Limit Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "LIMIT_ORDER_REJECT")]
        LimitOrderReject,
        /// <summary>
        /// Stop Order Transaction
        /// </summary>
        [EnumMember(Value = "STOP_ORDER")]
        StopOrder,
        /// <summary>
        /// Stop Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "STOP_ORDER_REJECT")]
        StopOrderReject,
        /// <summary>
        /// Market if Touched Order Transaction
        /// </summary>
        [EnumMember(Value = "MARKET_IF_TOUCHED_ORDER")]
        MarketIfTouchedOrder,
        /// <summary>
        /// Market if Touched Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "MARKET_IF_TOUCHED_ORDER_REJECT")]
        MarketIfTouchedOrderReject,
        /// <summary>
        /// Take Profit Order Transaction
        /// </summary>
        [EnumMember(Value = "TAKE_PROFIT_ORDER")]
        TakeProfitOrder,
        /// <summary>
        /// Take Profit Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "TAKE_PROFIT_ORDER_REJECT")]
        TakeProfitOrderReject,
        /// <summary>
        /// Stop Loss Order Transaction
        /// </summary>
        [EnumMember(Value = "STOP_LOSS_ORDER")]
        StopLossOrder,
        /// <summary>
        /// Stop Loss Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "STOP_LOSS_ORDER_REJECT")]
        StopLossOrderReject,
        /// <summary>
        /// Trailing Stop Loss Order Transaction
        /// </summary>
        [EnumMember(Value = "TRAILING_STOP_LOSS_ORDER")]
        TrailingStopLossOrder,
        /// <summary>
        /// Trailing Stop Loss Order Reject Transaction
        /// </summary>
        [EnumMember(Value = "TRAILING_STOP_LOSS_ORDER_REJECT")]
        TrailingStopLossOrderReject,
        /// <summary>
        /// Order Fill Transaction
        /// </summary>
        [EnumMember(Value = "ORDER_FILL")]
        OrderFill,
        /// <summary>
        /// Order Cancel Transaction
        /// </summary>
        [EnumMember(Value = "ORDER_CANCEL")]
        OrderCancel,
        /// <summary>
        /// Order Cancel Reject Transaction
        /// </summary>
        [EnumMember(Value = "ORDER_CANCEL_REJECT")]
        OrderCancelReject,
        /// <summary>
        /// Order Client Extensions Modify Transaction
        /// </summary>
        [EnumMember(Value = "ORDER_CLIENT_EXTENSIONS_MODIFY")]
        OrderClientExtensionsModify,
        /// <summary>
        /// Order Client Extensions Modify Reject Transaction
        /// </summary>
        [EnumMember(Value = "ORDER_CLIENT_EXTENSIONS_MODIFY_REJECT")]
        OrderClientExtensionsModifyReject,
        /// <summary>
        /// Trade Client Extensions Modify Transaction
        /// </summary>
        [EnumMember(Value = "TRADE_CLIENT_EXTENSIONS_MODIFY")]
        TradeClientExtensionsModify,
        /// <summary>
        /// Trade Client Extensions Modify Reject Transaction
        /// </summary>
        [EnumMember(Value = "TRADE_CLIENT_EXTENSIONS_MODIFY_REJECT")]
        TradeClientExtensionsModifyReject,
        /// <summary>
        /// Margin Call Enter Transaction
        /// </summary>
        [EnumMember(Value = "MARGIN_CALL_ENTER")]
        MarginCallEnter,
        /// <summary>
        /// Margin Call Extend Transaction
        /// </summary>
        [EnumMember(Value = "MARGIN_CALL_EXTEND")]
        MarginCallExtend,
        /// <summary>
        /// Margin Call Exit Transaction
        /// </summary>
        [EnumMember(Value = "MARGIN_CALL_EXIT")]
        MarginCallExit,
        /// <summary>
        /// Delayed Trade Closure Transaction
        /// </summary>
        [EnumMember(Value = "DELAYED_TRADE_CLOSURE")]
        DelayedTradeClosure,
        /// <summary>
        /// Daily Financing Transaction
        /// </summary>
        [EnumMember(Value = "DAILY_FINANCING")]
        DailyFinancing,
        /// <summary>
        /// Reset Resettable PL Transaction
        /// </summary>
        [EnumMember(Value = "RESET_RESETTABLE_PL")]
        ResetResettablePl,
    }


    [Serializable]
    [DataContract]
    internal enum TransactionReason
    {
        /// <summary>
        /// The Order was initiated at the request of a client
        /// </summary>
        [EnumMember(Value = "CLIENT_ORDER")]
        ClientOrder,
        /// <summary>
        /// The Order was initiated as a replacement for an existing Order
        /// </summary>
        [EnumMember(Value = "REPLACEMENT")]
        Replacement,
        /// <summary>
        /// The Take Profit/Stop Loss/Trailing was initiated automatically when an Order was filled that opened a new Trade requiring a one.
        /// </summary>
        [EnumMember(Value = "ON_FILL")]
        OnFill,
        /// <summary>
        /// The Market Order was created to close a Trade at the request of a client
        /// </summary>
        [EnumMember(Value = "TRADE_CLOSE")]
        TradeClose,
        /// <summary>
        /// The Market Order was created as part of a Margin Closeout
        /// </summary>
        [EnumMember(Value = "MARGIN_CLOSEOUT")]
        MarginCloseOut,
        /// <summary>
        /// The Market Order was created to close a trade marked for delayed closure
        /// </summary>
        [EnumMember(Value = "DELAYED_TRADE_CLOSE")]
        DelayedTradeClose,
    }

    [Serializable]
    [DataContract]
    internal class Transaction : IHeartbeat
    {
        /// <summary>
        /// The Transaction’s Identifier.
        /// </summary>
        [DataMember(Name = "id")]
        public string TransactionID;
        
        /// <summary>
        /// The Type of the Transaction.
        /// </summary>
        [DataMember(Name = "type")]
        private string TransactionTypeSerialization
        {
            get
            {
                return this.TransactionType.ToString();
            }
            set
            {
                this.TransactionType = (TransactionType)this.TransactionType.DeserializeFromJson(value);
            }
        }
        public TransactionType TransactionType;

        /// <summary>
        /// The date/time when the Transaction was created.
        /// </summary>
        [DataMember(Name = "time")]
        private string TimeSerialization
        {
            get
            {
                return string.Empty;
            }
            set
            {
                Time = DateTime.UtcNow;
                DateTime.TryParse(value, out Time);
                Time = TimeZoneInfo.ConvertTimeToUtc(Time, TimeZoneInfo.Local);
            }
        }
        public DateTime Time;

        /// <summary>
        /// The ID of the user that initiated the creation of the Transaction.
        /// </summary>
        [DataMember(Name = "userID")]
        public int UserID;

        /// <summary>
        /// The ID of the Account the Transaction was created for.
        /// </summary>
        [DataMember(Name = "accountID")]
        public string AccountID;

        [DataMember(Name = "batchID")]
        public int BatchID;

        [DataMember(Name = "instrument")]
        public string InstrumentId;

        [DataMember(Name = "units")]
        public double Amount;

        [DataMember(Name = "price")]
        public double Price;

        /// <summary>
        /// The price distance specified for the TrailingStopLoss Order.
        /// </summary>
        [DataMember(Name = "distance")]
        public double Distance;

        [DataMember(Name = "timeInForce")]
        private string TimeInForceSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.TimeInForce.ToString();
            }
            set
            {
                this.TimeInForce = (TimeInForceEnum)this.TimeInForce.DeserializeFromJson(value);
            }
        }
        public TimeInForceEnum TimeInForce;
        
        [DataMember(Name = "gtdTime")]
        private string GTDTimeSerialization
        {
            get
            {
                return string.Empty;
            }
            set
            {
                GTDTime = DateTime.UtcNow;
                DateTime.TryParse(value, out GTDTime);
                GTDTime = TimeZoneInfo.ConvertTimeToUtc(GTDTime, TimeZoneInfo.Local);
            }
        }
        public DateTime GTDTime;

        /// <summary>
        /// The worst price that the client is willing to have the Market Order filled at.
        /// </summary>
        [DataMember(Name = "priceBound")]
        public double PriceBound;

        /// <summary>
        /// Specification of how Positions in the Account are modified when the Order is filled.
        /// </summary>
        [DataMember(Name = "positionFill")]
        public string PositionFill;

        /// <summary>
        /// Details of the Trade requested to be closed
        /// </summary>
        [DataMember(Name = "tradeClose")]
        public MarketOrderTradeClose TradeClose;

        [DataMember(Name = "longPositionCloseout")]
        public string LongPositionCloseout;

        [DataMember(Name = "shortPositionCloseout")]
        public string ShortPositionCloseout;

        [DataMember(Name = "marginCloseout")]
        public string MarginCloseout;

        [DataMember(Name = "delayedTradeClose")]
        public string DelayedTradeClose;

        [DataMember(Name = "reason")]
        private string ReasonSerialization
        {
            get
            {
                return string.Empty;
            }
            set
            {
                this.Reason = (TransactionReason)this.Reason.DeserializeFromJson(value);
            }
        }
        public TransactionReason Reason;

        [DataMember(Name = "clientExtensions")]
        public ClientExtensions ClientExtensions;

        [DataMember(Name = "takeProfitOnFill")]
        public SlTpDetails TakeProfitOnFill;

        [DataMember(Name = "stopLossOnFill")]
        public SlTpDetails StopLossOnFill;

        [DataMember(Name = "trailingStopLossOnFill")]
        public TrailingStopLossDetails TrailingStopLossOnFill;

        [DataMember(Name = "tradeClientExtensions")]
        public ClientExtensions TradeClientExtensions;

        [DataMember(Name = "rejectReason")]
        public string RejectReason;

        [DataMember(Name = "replacesOrderID")]
        public string ReplacesOrderID;

        [DataMember(Name = "replacedOrderCancelTransactionID ")]
        public string replacedOrderCancelTransactionID;

        [DataMember(Name = "tradeID")]
        public string TradeID;

        [DataMember(Name = "clientTradeID")]
        public string ClientTradeID;

        [DataMember(Name = "orderID")]
        public string OrderID;

        [DataMember(Name = "clientOrderID")]
        public string ClientOrderID;

        [DataMember(Name = "pl")]
        public double Pl;

        [DataMember(Name = "financing")]
        public double Financing;

        [DataMember(Name = "accountBalance")]
        public double AccountBalance;


        [DataMember(Name = "tradeOpened")]
        public TradeOpen TradeOpened;

        [DataMember(Name = "tradesClosed")]
        public List<TradeReduce> tradesClosed;

        [DataMember(Name = "tradeReduced")]
        public TradeReduce tradeReduced;

        public bool IsHeartbeat()
        {
            return this.TransactionType == TransactionType.HeartBeat;
        }
    }
}
