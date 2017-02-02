// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal enum InstrumentType
    {
        [EnumMember(Value = "CURRENCY")]
        Currency,
        [EnumMember(Value = "CFD")]
        Cfd,
        [EnumMember(Value = "METAL")]
        Metal
    }

    [Serializable]
    [DataContract]
    internal class InstrumentOanda
    {
        [DataMember(Name = "displayName")]
        public string Name;

        [DataMember(Name = "name")]
        public string Id;

        [DataMember(Name = "displayPrecision")]
        public int DisplayPrecision;

        [DataMember(Name = "maximumOrderUnits")]
        public int MaximumOrderUnits;

        [DataMember(Name = "maximumPositionSize")]
        public int MaximumPositionSize;

        [DataMember(Name = "minimumTradeSize")]
        public int MinimumTradeSize;

        [DataMember(Name = "maximumTrailingStopDistance")]
        public double MaximumTrailingStopDistance;
        
        [DataMember(Name = "minimumTrailingStopDistance")]
        public double MinimumTrailingStopDistance;

        [DataMember(Name = "pipLocation")]
        public int PipLocation;

        [DataMember(Name = "tradeUnitsPrecision")]
        public int TradeUnitsPrecision;
        
        [DataMember(Name = "type")]
        private string InstrumentTypeSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.InstrumentType.ToString();
            }
            set
            {
                this.InstrumentType = (InstrumentType)this.InstrumentType.DeserializeFromJson(value);
            }
        }
        public InstrumentType InstrumentType;
        
        [DataMember(Name = "marginRate")]
        public double MarginRate;
    }
}
