// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using OandaV20ExternalVendor.TradeLibrary.DataTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary
{
    public class Rest
    {
        // Convenience helpers
        private static string Server(EServer server) { return Credentials.GetDefaultCredentials().GetServer(server); }
        private static string AccessToken { get { return Credentials.GetDefaultCredentials().AccessToken; } }

        /// <summary>
        /// Initializes a streaming rates session with the given instruments on the given account
        /// </summary>
        /// <param name="instruments">list of instruments to stream rates for</param>
        /// <param name="accountId">the account ID you want to stream on</param>
        /// <returns>the WebResponse object that can be used to retrieve the rates as they stream</returns>
        internal static async Task<WebRequest> GetStartRatesSessionRequest(List<InstrumentOanda> instruments, string accountId)
        {
            string instrumentList = "";
            foreach (var instrument in instruments)
            {
                instrumentList += instrument.Id + ",";
            }
            // Remove the extra ,
            instrumentList = instrumentList.TrimEnd(',');
            instrumentList = Uri.EscapeDataString(instrumentList);

            string requestString = Server(EServer.StreamingRates) + accountId + "/pricing/stream?instruments=" + instrumentList;

            WebRequest request = WebRequest.Create(requestString);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;

            return request;
        }

        public static async Task<WebRequest> GetStartEventsSessionReques(string accountId)
        {
            string requestString = Server(EServer.StreamingEvents) + accountId + "/transactions/stream";

            WebRequest request = WebRequest.Create(requestString);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;

            return request;
        }

        /// <summary>
        /// Retrieves all the accounts belonging to the user
        /// </summary>
        /// <param name="user">the username to use -- only needed on sandbox-- otherwise identified by the token used</param>
        /// <returns>list of accounts, including basic information about the accounts</returns>
        internal static List<AccountOanda> GetAccountListAsync(string user = "")
        {
            string requestString = Server(EServer.Account) + "accounts";
            if (!string.IsNullOrEmpty(user))
            {
                requestString += "?username=" + user;
            }

            var result = MakeRequest<AccountsResponce>(requestString);
            return result.Accounts;
        }

        /// <summary>
        /// Get the full details for a single Account that a client has access to. Full pending Order, open Trade and open Position representations are provided.
        /// </summary>
        internal static async Task<AccountDetails> GetAccountDetailsAsync(string accountId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId;

            var accountDetails = await MakeRequestAsync<AccountDetailsResponce>(requestString);
            return accountDetails != null ? accountDetails.Account : null;
        }

        /// <summary>
        /// Get a summary for a single Account that a client has access to.
        /// </summary>
        internal static AccountSummaryOanda GetAccountSummary(string accountId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/summary";

            var accountSummary = MakeRequest<AccountSummaryResponce>(requestString);
            return accountSummary != null ? accountSummary.Account :null;
        }

        /// <summary>
        /// Endpoint used to poll an Account for its current state and changes since a specified TransactionID.
        /// </summary>
        internal static AccountState GetAccountState(string accountId, string sinceTransactionID = null)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/changes";

            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sinceTransactionID))
            {
                parameters["sinceTransactionID"] = sinceTransactionID;
            }

            var accountChanges = MakeRequest<AccountChangesResponce>(requestString, "GET", parameters);
            return accountChanges != null ? accountChanges.State : null;
        }

        /// <summary>
        /// Get the list of tradeable instruments for the given Account. The list of tradeable instruments is dependent on the regulatory division that the Account is located in, thus should be the same for all Accounts owned by a single user.
        /// </summary>
        internal static List<InstrumentOanda> GetAllInstrumentsAsync(string accountId, List<string> instrumentNames = null)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/instruments";

            if (instrumentNames != null)
            {
                string instrumentsParam = Uri.EscapeDataString(GetCommaSeparatedList(instrumentNames));
                requestString += CreateParamString(new Dictionary<string, string>() { { "instruments", instrumentsParam } });
            }

            var instrumentsResponce = MakeRequest<InstrumentsResponce>(requestString);
            return instrumentsResponce != null ? instrumentsResponce.Instruments : new List<InstrumentOanda>();
        }

        /// <summary>
        /// List all pending Orders in an Account
        /// </summary>
        internal static List<OrderOanda> GetPendingOrders(string accountId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/pendingOrders";
            
            var ordersResponce = MakeRequest<OrdersResponce>(requestString);
            return ordersResponce != null ? ordersResponce.Orders : new List<OrderOanda>();
        }

        internal static async Task<List<OrderOanda>> GetOrders(string accountId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/orders";

            var ordersResponce = await MakeRequestAsync<OrdersResponce>(requestString);
            return ordersResponce != null ? ordersResponce.Orders : new List<OrderOanda>();
        }

        internal static TransactionsPage GetTransactionsPage(string accountId, DateTime from, DateTime to)
        {
            string fromStr = OandaV20Utils.ConvertDateTimeToOandaFormat(from, true);
            string toStr = OandaV20Utils.ConvertDateTimeToOandaFormat(to, true);

            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/transactions?to=" + toStr + "&from=" + fromStr;

            var responce = MakeRequest<TransactionsPage>(requestString);
            return responce;
        }

        internal static List<Transaction> GetTransactions(string transactionPage)
        {
            var transactionsResponce = MakeRequest<TransactionsResponce>(transactionPage);
            return transactionsResponce != null ? transactionsResponce.Transactions : new List<Transaction>();
        }

        /// <summary>
        /// Get details for a single Order in an Account
        /// </summary>
        internal static OrderOanda GetSingleOrder(string accountId, string orderId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/orders/" + orderId;

            var ordersResponce = MakeRequest<SingleOrderResponce>(requestString);
            return ordersResponce != null ? ordersResponce.Order : null;
        }

        /// <summary>
        /// Get the list of open Trades for an Account
        /// </summary>
        internal static List<TradeOanda> GetOpenTrades(string accountId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/openTrades";

            var tradesResponce = MakeRequest<TradesResponce>(requestString);
            return tradesResponce != null ? tradesResponce.Trades : new List<TradeOanda>();
        }
        /// <summary>
        /// Get the details of a specific Trade in an Account
        /// </summary>
        internal static TradeOanda GetSingleTrade(string accountId, string tradeId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/trades/" + tradeId;

            var tradeResponce = MakeRequest<SingleTradeResponce>(requestString);
            return tradeResponce != null ? tradeResponce.Trade : null;
        }

        /// <summary>
        /// More detailed request to retrieve candles
        /// </summary>
        /// <param name="request">the request data to use when retrieving the candles</param>
        /// <returns>List of Candles received (or empty list)</returns>
        internal static List<Candle> GetCandles(CandlesRequest request)
        {
            string requestString = Server(EServer.Rates) + request.GetRequestString();

            var candles = new List<Candle>();

            CandlesResponse candlesResponse = MakeRequest<CandlesResponse>(requestString);

            candles.AddRange(candlesResponse.Candles);

            return candles;
        }

        /// <summary>
        /// Posts an order on the given account with the given parameters
        /// </summary>
        /// <param name="account">the account to post on</param>
        /// <param name="requestParams">the parameters to use in the request</param>
        /// <returns>PostOrderResponse with details of the results (throws if if fails)</returns>
        internal static OrderCreateResponce PostOrder(string account, PlaceOrderRequest orderRequest)
        {
            string requestString = Server(EServer.Account) + "accounts/" + account + "/orders";
            return MakeRequestWithBody<PlaceOrderRequest, OrderCreateResponce>("POST", orderRequest, requestString);
        }

        /// <summary>
        /// Replace an Order in an Account by simultaneously cancelling it and creating a replacement Order
        /// </summary>
        internal static OrderCreateResponce ReplaceOrderOrder(string account, PlaceOrderRequest orderRequest, string orderId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + account + "/orders/" + orderId;
            return MakeRequestWithBody<PlaceOrderRequest, OrderCreateResponce>("PUT", orderRequest, requestString);
        }

        internal static OrderOanda CancelOrderAsync(string accountId, string orderId)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/orders/" + orderId + "/cancel";
            return MakeRequest<OrderOanda>(requestString, "PUT");
        }

        internal static OrderCreateResponce CancelTrade(string accountId, string tradeId, CloseTradeRequest closeRequest)
        {
            string requestString = Server(EServer.Account) + "accounts/" + accountId + "/trades/" + tradeId + "/close";
            return MakeRequestWithBody<CloseTradeRequest, OrderCreateResponce>("PUT", closeRequest, requestString);
        }

        #region Utils

        /// <summary>
        /// Primary (internal) request handler
        /// </summary>
        /// <typeparam name="T">The response type</typeparam>
        /// <param name="requestString">the request to make</param>
        /// <param name="method">method for the request (defaults to GET)</param>
        /// <param name="requestParams">optional parameters (note that if provided, it's assumed the requestString doesn't contain any)</param>
        /// <returns>response via type T</returns>
        private static async Task<T> MakeRequestAsync<T>(string requestString, string method = "GET", Dictionary<string, string> requestParams = null)
        {
            if (requestParams != null && requestParams.Count > 0)
            {
                var parameters = CreateParamString(requestParams);
                requestString = requestString + "?" + parameters;
            }

            WebRequest request = WebRequest.Create(requestString);
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;
            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            request.Method = method;

            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    var stream = GetResponseStream(response);
                    
                    return (T)serializer.ReadObject(stream);
                }
            }
            catch (WebException ex)
            {
                var serializer = new DataContractJsonSerializer(typeof(RequestError));
                if (ex.Response == null)
                    throw ex;
                var stream = GetResponseStream(ex.Response);
                try
                {
                    string err = string.Empty;
                    using (StreamReader sr = new StreamReader(stream))
                        err = sr.ReadToEnd();
                    var error = (RequestError)serializer.ReadObject(stream);
                    throw new Exception(error.Message);
                }
                catch (Exception innerEx)
                {
                    throw ex;
                }
            }
        }

        private static T MakeRequest<T>(string requestString, string method = "GET", Dictionary<string, string> requestParams = null)
        {
            if (requestParams != null && requestParams.Count > 0)
            {
                var parameters = CreateParamString(requestParams);
                requestString = requestString + "?" + parameters;
            }

            WebRequest request = WebRequest.Create(requestString);
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;
            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            request.Method = method;

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    var stream = GetResponseStream(response);

                    return (T)serializer.ReadObject(stream);
                }
            }
            catch (IOException ex)
            {
                throw ex.InnerException ?? ex;
            }
            catch (WebException ex)
            {
                var serializer = new DataContractJsonSerializer(typeof(RequestError));
                if (ex.Response == null)
                    throw ex;
                var stream = GetResponseStream(ex.Response);
                try
                {
                    var error = (RequestError)serializer.ReadObject(stream);
                    throw new Exception(error.Message);
                }
                catch (Exception innerEx)
                {
                    throw innerEx.InnerException;
                }
            }
        }

        /// <summary>
        /// Secondary (internal) request handler. differs from primary in that parameters are placed in the body instead of the request string
        /// </summary>
        /// <typeparam name="T">response type</typeparam>
        /// <param name="method">method to use (usually POST or PATCH)</param>
        /// <param name="requestParams">the parameters to pass in the request body</param>
        /// <param name="requestString">the request to make</param>
        /// <returns>response, via type T</returns>
        private static R MakeRequestWithBody<T, R>(string method, object requestParams, string requestString)
        {
            // Create the body
            WebRequest request = WebRequest.Create(requestString);
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;
            request.Method = method;
            request.ContentType = "application/json";
            
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                var paramSerializer = new DataContractJsonSerializer(typeof(T));
                paramSerializer.WriteObject(request.GetRequestStream(), (T)requestParams);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }

            // Handle the response
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    var stream = GetResponseStream(response);
                    //string json;
                    //using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                    //{
                    //    json = rdr.ReadToEnd();
                    //}
                    
                    //var newStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

                    var serializer = new DataContractJsonSerializer(typeof(R));
                    return (R)serializer.ReadObject(stream);
                }
            }
            catch (WebException ex)
            {
                var serializer = new DataContractJsonSerializer(typeof(RequestError));
                var stream = GetResponseStream(ex.Response);
                var error = (RequestError)serializer.ReadObject(stream);
                throw new Exception(error.Message);
            }
        }

        /// <summary>
        /// Helper function to create the parameter string out of a dictionary of parameters
        /// </summary>
        /// <param name="requestParams">the parameters to convert</param>
        /// <returns>string containing all the parameters for use in requests</returns>
        private static string CreateParamString(Dictionary<string, string> requestParams)
        {
            string requestBody = "";
            foreach (var pair in requestParams)
            {
                requestBody += pair.Key + "=" + pair.Value + "&";
            }
            requestBody = requestBody.Trim('&');
            return requestBody;
        }

        private static Stream GetResponseStream(WebResponse response)
        {
            var stream = response.GetResponseStream();
            if (response.Headers["Content-Encoding"] == "gzip")
            {	// if we received a gzipped response, handle that
                stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return stream;
        }

        private static string GetCommaSeparatedList(List<string> items)
        {
            StringBuilder result = new StringBuilder();
            foreach (var item in items)
            {
                result.Append(item + ",");
            }
            return result.ToString().Trim(',');
        }

        #endregion

    }
}
