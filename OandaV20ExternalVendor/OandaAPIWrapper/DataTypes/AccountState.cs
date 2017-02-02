// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class AccountState
    {
        /// <summary>
        /// The total unrealized profit/loss for all Trades currently open in the Account. Represented in the Account’s home currency.
        /// </summary>
        [DataMember(Name = "unrealizedPL")]
        public double UnrealizedPL;
        
        /// <summary>
        /// The net asset value of the Account. Equal to Account balance + unrealizedPL. Represented in the Account’s home currency.
        /// </summary>
        [DataMember(Name = "NAV")]
        public double NetAssetValue;
        
        /// <summary>
        /// Margin currently used for the Account. Represented in the Account’s home currency.
        /// </summary>
        [DataMember(Name = "marginUsed")]
        public double MarginUsed;

        /// <summary>
        /// Margin available for Account. Represented in the Account’s home currency.
        /// </summary>
        [DataMember(Name = "marginAvailable")]
        public double MarginAvailable;

        /// <summary>
        /// The value of the Account’s open positions represented in the Account’s home currency.
        /// </summary>
        [DataMember(Name = "positionValue")]
        public double PositionValue;

        /// <summary>
        /// The Account’s margin closeout unrealized PL.
        /// </summary>
        [DataMember(Name = "marginCloseoutUnrealizedPL")]
        public double MarginCloseoutUnrealizedPL;

        /// <summary>
        /// The Account’s margin closeout NAV.
        /// </summary>
        [DataMember(Name = "marginCloseoutNAV")]
        public double MarginCloseoutNAV;

        /// <summary>
        /// The Account’s margin closeout margin used.
        /// </summary>
        [DataMember(Name = "marginCloseoutMarginUsed")]
        public double MarginCloseoutMarginUsed;
        
        /// <summary>
        /// The Account’s margin closeout percentage. When this value is 1.0 or above the Account is in a margin closeout situation.
        /// </summary>
        [DataMember(Name = "marginCloseoutPercent")]
        public double MarginCloseoutPercent;
        
        /// <summary>
        /// The current WithdrawalLimit for the account which will be zero or a positive value indicating how much can be withdrawn from the account.
        /// </summary>
        [DataMember(Name = "withdrawalLimit")]
        public double WithdrawalLimit;

        /// <summary>
        /// The Account’s margin call margin used.
        /// </summary>
        [DataMember(Name = "marginCallMarginUsed")]
        public double marginCallMarginUsed;
        
        /// <summary>
        /// The Account’s margin call percentage. When this value is 1.0 or above the Account is in a margin call situation.
        /// </summary>
        [DataMember(Name = "marginCallPercent")]
        public double MarginCallPercent;

        /// <summary>
        /// The price-dependent state for each open Trade in the Account.
        /// </summary>
        [DataMember(Name = "trades")]
        public List<CalculatedTradeState> trades;
    }
}
