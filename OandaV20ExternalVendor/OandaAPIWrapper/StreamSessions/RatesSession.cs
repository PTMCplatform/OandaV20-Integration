// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using OandaV20ExternalVendor.TradeLibrary.DataTypes;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary
{
    internal class RatesSession : StreamSession<Price>
    {
        private readonly List<InstrumentOanda> _instruments;

        public RatesSession(string accountId, List<InstrumentOanda> instruments)
            : base(accountId)
        {
            _instruments = instruments;
        }

        protected override async Task<WebRequest> GetSessionRequest()
        {
            return await Rest.GetStartRatesSessionRequest(_instruments, _accountId);
        }
    }
}
