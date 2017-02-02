// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    internal enum TradeState
    {
        /// <summary>
        /// The Trade is currently open
        /// </summary>
        [EnumMember(Value = "GTC")]
        Open,
        /// <summary>
        /// The Trade has been fully closed
        /// </summary>
        [EnumMember(Value = "GTD")]
        Closed,
        /// <summary>
        /// The Trade will be closed as soon as the trade’s instrument becomes tradeable
        /// </summary>
        [EnumMember(Value = "GFD")]
        ClosedWhenTradeable
    }

    [Serializable]
    [DataContract]
    internal class TradeOanda
    {
        [DataMember(Name = "clientExtensions")]
        public ClientExtensions ClientExtensions;

        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "currentUnits")]
        public double CurrentUnits;

        [DataMember(Name = "initialUnits")]
        public double InitialUnits;

        [DataMember(Name = "financing")]
        public double Financing;
        
        [DataMember(Name = "instrument")]
        public string InstrumentId;

        [DataMember(Name = "openTime")]
        private string OpenTimeSerialization
        {
            get
            {
                return string.Empty;
            }
            set
            {
                OpenTime = DateTime.UtcNow;
                DateTime.TryParse(value, out OpenTime);
                OpenTime = TimeZoneInfo.ConvertTimeToUtc(OpenTime, TimeZoneInfo.Local);
            }
        }

        public DateTime OpenTime;

        [DataMember(Name = "price")]
        public double Price;
        
        [DataMember(Name = "realizedPL")]
        public double RealizedPL;
        
        [DataMember(Name = "unrealizedPL")]
        public double UnrealizedPL;

        [DataMember(Name = "state")]
        private string StateSerialisation
        {
            get
            {
                return string.Empty;
            }
            set
            {
                this.State.DeserializeFromJson(value);
            }
        }

        public TradeState State;
    }
}
