// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    [Serializable]
    [DataContract]
    internal class CloseOrderDetails
    {
        [DataMember(Name = "timeInForce", EmitDefaultValue = false)]
        private string TimeInForceSerialization // Notice the PRIVATE here!
        {
            get
            {
                return this.TimeInForce == TimeInForceEnum.GTD ? this.TimeInForce.SerializeToJson() : null;
            }
            set
            {
                this.TimeInForce = (TimeInForceEnum)this.TimeInForce.DeserializeFromJson(value);
            }
        }
        public TimeInForceEnum TimeInForce;

        [DataMember(Name = "gtdTime", EmitDefaultValue = false)]
        private string GTDTimeSerialization
        {
            get
            {
                return GTDTime.ToString("s") + "Z";
            }
            set
            {
                GTDTime = DateTime.UtcNow;
                DateTime.TryParse(value, out GTDTime);
                GTDTime = TimeZoneInfo.ConvertTimeToUtc(GTDTime, TimeZoneInfo.Local);
            }
        }
        public DateTime GTDTime;

        [DataMember(Name = "clientExtensions", EmitDefaultValue = false)]
        public ClientExtensions ClientExtensions;
    }
}
