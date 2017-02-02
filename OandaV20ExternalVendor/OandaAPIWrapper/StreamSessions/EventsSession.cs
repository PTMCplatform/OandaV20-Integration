// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using OandaV20ExternalVendor.TradeLibrary.DataTypes;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary
{
    internal class EventsSession : StreamSession<Transaction>
    {
        public EventsSession(string accountId)
            : base(accountId)
        {
        }

        //protected override async Task<WebResponse> GetSession()
        //{
        //    return await Rest.StartEventsSession(new List<int> { _accountId });
        //}

        protected override async Task<WebRequest> GetSessionRequest()
        {
            return await Rest.GetStartEventsSessionReques(_accountId);
        }
    }
}
