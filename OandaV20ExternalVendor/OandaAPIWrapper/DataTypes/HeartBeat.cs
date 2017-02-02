// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary.DataTypes
{
    public interface IHeartbeat
    {
        bool IsHeartbeat();
    }

    public class Heartbeat
    {
        public string time { get; set; }
    }
}
