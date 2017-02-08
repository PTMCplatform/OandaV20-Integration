// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using PlatformAPI.Integration;
using OandaV20ExternalVendor.TradeLibrary;
using OandaV20ExternalVendor.TradeLibrary.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace OandaV20ExternalVendor
{
    public class OandaV20Utils
    {
        #region OAuth authorization

        public static string AccessToken;

        static Form form;

        delegate void GetTokenFromWebDelegate(string clientID, string redirectUrl, Action<string> callback, string FUser, string FPassword);

        public static string GetTokenFromWebasync(string clientID, string redirectUrl, string FUser, string FPassword, Control invoker)
        {
            AccessToken = string.Empty;
            if (invoker == null)
                return "error";

            string requestTokenError = null;
            
            Action<string> callback = delegate (string error)
            {
                requestTokenError = error;
                Console.WriteLine("OANDA. Error: {0}", error);
            };

            invoker.BeginInvoke(new GetTokenFromWebDelegate(GetTokenFromWeb), new object[] { clientID, redirectUrl, callback, FUser, FPassword });
            
            var startTime = DateTime.Now;
            while (string.IsNullOrEmpty(requestTokenError))
            {
                System.Windows.Forms.Application.DoEvents();

                if (!string.IsNullOrEmpty(AccessToken) || (DateTime.Now - startTime).TotalSeconds > 120)
                    break;
            }

            DestroyWebForm();

            return requestTokenError;
        }

        static internal void DestroyWebForm()
        {
            try
            {
                if (form != null)
                {
                    if (form.IsHandleCreated)
                    {
                        form.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                if (form != null && form.IsHandleCreated)
                                {
                                    form.Close();
                                    form.Dispose();
                                }
                            }
                            catch { }
                            form = null;
                        }
                            ));
                    }
                    else
                    {
                        form.Close();
                        form = null;
                    }
                }
            }
            catch { }
        }

        static private void GetTokenFromWeb(string clientID, string redirectUrl, Action<string> callback, string FUser, string FPassword)
        {
            //const string redirect = "https://api-sandbox.oanda.com/index.html";
            //const string clientId = "g6E5XbvB28Ph9Kin";
            const string scopes = "read+trade+marketdata+stream";

            // create a random state string
            var rand = new Random();
            string state = "fnuajuiwqnzpofmAHAAJfNMLKAOW9mvm9r201jmpmfpa" + rand.Next() + rand.Next() + rand.Next();

            string requestURL = Credentials.GetDefaultCredentials().GetServer(EServer.Account) + @"/oauth2/authorize?client_id=" +
                                Uri.EscapeDataString(clientID) + "&redirect_uri=" + Uri.EscapeDataString(redirectUrl) +
                                "&state=" + Uri.EscapeDataString(state) + "&response_type=token&scope=" + Uri.EscapeDataString(scopes);

            Uri startUri = new Uri(requestURL);

            try
            {

                form = new Form();
                if (Control.ModifierKeys == Keys.Control)
                {
                    form.ShowInTaskbar = false;
                    form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                    form.Bounds = new System.Drawing.Rectangle(0, 0, 300, 300);
                }
                else
                {
                    form.ShowInTaskbar = false;
                    form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                    form.Bounds = new System.Drawing.Rectangle(-100000, -100000, 1000, 1000);
                }
                //form.WindowState = FormWindowState.Maximized;
                var webBrowser = new WebBrowser();
                webBrowser.Navigated += bc_LoadCompleted;
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.Dock = DockStyle.Fill;


                form.Controls.Add(webBrowser);

                form.Show();

                webBrowser.Navigate(requestURL);

                WaitBrowserLoad(webBrowser);

                if (webBrowser.Document == null)
                {
                    if (callback != null)
                        callback("Connection timed out");
                    return;
                }

                var usernameDOM = webBrowser.Document.GetElementById("username");
                var passwordDOM = webBrowser.Document.GetElementById("password");

                if (usernameDOM != null && passwordDOM != null)
                {
                    usernameDOM.SetAttribute("value", FUser);
                    passwordDOM.SetAttribute("value", FPassword);

                    // Auto click on Login button
                    webBrowser.Document.GetElementById("submit").InvokeMember("click");
                }

                WaitBrowserLoad(webBrowser, 2);

                var dTags = webBrowser.Document.Body.GetElementsByTagName("div");
                foreach (System.Windows.Forms.HtmlElement div in dTags)
                {
                    if (div.GetAttribute("className") == @"error-msg")
                    {
                        if (callback != null)
                            callback(div.Children[0].InnerText);
                        return;
                    }
                }

                Sleep();

                //var pTags = webBrowser.Document.Body.GetElementsByTagName("p");
                //foreach (System.Windows.Forms.HtmlElement p in pTags)
                //{
                //    if (p.GetAttribute("className") == @"v20-disclaimer v20-no-accounts")
                //    {
                //        if (callback != null)
                //            callback("general.connection.WrongApiVersion");
                //        return;
                //    }
                //}

                var checkBoxes = webBrowser.Document.Body.GetElementsByTagName("input");
                if (checkBoxes != null && checkBoxes.Count != 0)
                    foreach (System.Windows.Forms.HtmlElement checkBox in checkBoxes)
                        if (checkBox.GetAttribute("type") == "checkbox")
                            checkBox.SetAttribute("checked", "checked");

                WaitBrowserLoad(webBrowser);

                Sleep();

                bool dontFindAllowButton = true;

                var buttons = webBrowser.Document.Body.GetElementsByTagName("button");
                if (buttons != null && buttons.Count != 0)
                    foreach (System.Windows.Forms.HtmlElement button in buttons)
                        if (button.Name == "allow")
                        {
                            button.InvokeMember("click");
                            dontFindAllowButton = false;
                            break;
                        }

                if (dontFindAllowButton)
                {
                    if (callback != null)
                        callback("User/password combination is not valid.");
                    return;
                }

                Sleep();

                WaitBrowserLoad(webBrowser);

            }
            catch (Exception ex)
            {
                if (callback != null)
                    callback(ex.Message);
            }
        }

        static private void bc_LoadCompleted(object sender, WebBrowserNavigatedEventArgs e)
        {
            var url = e.Url.Fragment;
            if (url.Contains("access_token") || url.Contains("#"))
            {
                url = (new System.Text.RegularExpressions.Regex("#")).Replace(url, "?", 1);
                AccessToken = HttpUtility.ParseQueryString(url).Get("access_token");
            }
        }

        static void WaitBrowserLoad(System.Windows.Forms.WebBrowser browser, int defualtTime = 1)
        {
            var startTime = DateTime.Now;
            while (true)
            {
                System.Windows.Forms.Application.DoEvents();

                if (browser.IsDisposed || browser.ReadyState == System.Windows.Forms.WebBrowserReadyState.Complete && (DateTime.Now - startTime).TotalSeconds > defualtTime || (DateTime.Now - startTime).TotalSeconds > 10)
                    break;
            }
        }


        static void Sleep()
        {
            var startTime = DateTime.Now;
            while (true)
            {
                System.Windows.Forms.Application.DoEvents();

                if ((DateTime.Now - startTime).TotalSeconds > 5)
                    break;
            }
        }

        #endregion

        const string KEY_GENERAL_GROUP = "panel.accountDetails.Groups.1.General";
        const string KEY_MARGIN_GROUP = "panel.accountDetails.Groups.2.Margin";

        static List<string> CFDIndexInstruments = new List<string>
        {
              "AUSTRALIA 200",
              "FRANCE 40",
              "HONG KONG 33",
              "JAPAN 225",
              "NETHERLANDS 25",
              "SINGAPORE 30",
              "SWISS 20",
              "US NAS 100",
              "US RUSS 2000",
              "US SPX 500",
              "EUROPE 50",
              "GERMANY 30",
              "UK 100",
              "US WALL ST 30",
              "CHINA A50",
              "TAIWAN INDEX",
              "INDIA 50",
        };

        static List<string> CFDCommoditiesInstruments = new List<string>
        {
             "WHEAT",
             "WEST TEXAS OIL",
             "SUGAR",
             "SOYBEANS",
             "NATURAL GAS",
             "CORN",
             "BRENT CRUDE OIL"
        };

        static List<string> CFDBondsInstruments = new List<string>
        {
             "BUND",
             "UK 10Y GILT",
             "US 10Y T-NOTE",
             "US 2Y T-NOTE",
             "US 5Y T-NOTE",
             "US T-BOND"
        };

        static List<string> CFDMetalsInstruments = new List<string>
        {
            "COPPER",
            "PALLADIUM",
            "PLATINUM",
            "GOLD",
            "GOLD/AUD",
            "GOLD/CAD",
            "GOLD/CHF",
            "GOLD/EUR",
            "GOLD/GBP",
            "GOLD/HKD",
            "GOLD/JPY",
            "GOLD/NZD",
            "GOLD/SGD",
            "GOLD/SILVER",
            "SILVER",
            "SILVER/AUD",
            "SILVER/CAD",
            "SILVER/CHF",
            "SILVER/EUR",
            "SILVER/GBP",
            "SILVER/HKD",
            "SILVER/JPY",
            "SILVER/NZD",
            "SILVER/SGD"
        };

        public static string GetOandaHistoryPrice(HistoryDataTypes dataType)
        {
            switch (dataType)
            {
                case HistoryDataTypes.Ask:
                    return "A";
                case HistoryDataTypes.Bid:
                    return "B";
                case HistoryDataTypes.BidAskAverage:
                    return "M";
                default:
                    return null;
            }
        }

        public static GranularityEnum? GetOandaTimeframe(int timeframe)
        {
            switch (timeframe)
            {
                case 5 * (int)HistoryPeriod.Second:
                    return GranularityEnum.S5;
                case 10 * (int)HistoryPeriod.Second:
                    return GranularityEnum.S10;
                case 15 * (int)HistoryPeriod.Second:
                    return GranularityEnum.S15;
                case 30 * (int)HistoryPeriod.Second:
                    return GranularityEnum.S30;
                case (int)HistoryPeriod.Minute:
                    return GranularityEnum.M1;
                case 2 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M2;
                case 3 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M3;
                case 4 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M4;
                case 5 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M5;
                case 10 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M10;
                case 15 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M15;
                case 30 * (int)HistoryPeriod.Minute:
                    return GranularityEnum.M30;
                case (int)HistoryPeriod.Hour:
                    return GranularityEnum.H1;
                case 2 * (int)HistoryPeriod.Hour:
                    return GranularityEnum.H2;
                case 3 * (int)HistoryPeriod.Hour:
                    return GranularityEnum.H3;
                case 4 * (int)HistoryPeriod.Hour:
                    return GranularityEnum.H4;
                case 6 * (int)HistoryPeriod.Hour:
                    return GranularityEnum.H6;
                case 8 * (int)HistoryPeriod.Hour:
                    return GranularityEnum.H8;
                case 12 * (int)HistoryPeriod.Hour:
                    return GranularityEnum.H12;
                case (int)HistoryPeriod.Day:
                    return GranularityEnum.D;
                case (int)HistoryPeriod.Week:
                    return GranularityEnum.W;
                case (int)HistoryPeriod.Month:
                    return GranularityEnum.M;
                default:
                    return null;
            }
        }

        internal static TimeInForce GetTiff(TimeInForceEnum oandaTiff)
        {
            switch (oandaTiff)
            {
                case TimeInForceEnum.FOK:
                    return TimeInForce.FOC;
                case TimeInForceEnum.GTD:
                    return TimeInForce.GTD;
                case TimeInForceEnum.GFD:
                    return TimeInForce.Day;
                case TimeInForceEnum.IOC:
                    return TimeInForce.IOC;
                case TimeInForceEnum.GTC:
                default:
                    return TimeInForce.GTC;
            }
        }

        internal static TimeInForceEnum GetTiff(TimeInForce tiff)
        {
            switch (tiff)
            {
                case TimeInForce.FOC:
                    return TimeInForceEnum.FOK;
                case TimeInForce.GTD:
                    return TimeInForceEnum.GTD;
                case TimeInForce.IOC:
                    return TimeInForceEnum.IOC;
                case TimeInForce.Day:
                    return TimeInForceEnum.GFD;
                case TimeInForce.GTC:
                default:
                    return TimeInForceEnum.GTC;
            }
        }

        internal static InstrumentTypeEnum GetInstrumentType(InstrumentType oandaType)
        {
            switch (oandaType)
            {
                case InstrumentType.Cfd:
                case InstrumentType.Metal:
                    return InstrumentTypeEnum.CfdIndex;

                case InstrumentType.Currency:
                default:
                    return InstrumentTypeEnum.Forex;
            }
        }

        internal static InstrumentGroupEnum GetInstrumentGroup(string name)
        {
            if (CFDIndexInstruments.Contains(name))
                return InstrumentGroupEnum.Index;
            if (CFDCommoditiesInstruments.Contains(name))
                return InstrumentGroupEnum.Commodity;
            if (CFDBondsInstruments.Contains(name))
                return InstrumentGroupEnum.Bond;
            if (CFDMetalsInstruments.Contains(name))
                return InstrumentGroupEnum.Metal;
            
            return InstrumentGroupEnum.Forex;
        }

        internal static DateTime GetOrderExpireDateByTIF(TimeInForce tif, DateTime orderExpireDate, DateTime utc)
        {
            switch(tif)
            {
                case TimeInForce.Day:
                    return utc.AddDays(1);
                case TimeInForce.FOC:
                case TimeInForce.IOC:
                    return utc.AddMinutes(1);
                case TimeInForce.GTC:
                    return utc.AddYears(10);
                case TimeInForce.GTD:
                default:
                    return orderExpireDate;
            }
        }

        //to do: Расширить список ордеров
        internal static OrderType GetOrderType(OrderTypeOanda oandaOrderType)
        {
            switch (oandaOrderType)
            {
                case OrderTypeOanda.Limit:
                case OrderTypeOanda.TakeProfit:
                    return OrderType.Limit;
                case OrderTypeOanda.Stop:
                case OrderTypeOanda.StopLoss:
                    return OrderType.Stop;

                case OrderTypeOanda.TrailingStopLoss:
                    return OrderType.TrailingStop;

                case OrderTypeOanda.Market:
                default:
                    return OrderType.Market;
            }
        }

        /// <summary>
        /// Разбивает историю на куски для одновременной загрузки, есть ограничения от сервера
        /// </summary>        
        internal static TimeRange[] SplitHistory(int period, DateTime fromTime, DateTime toTime, DateTime utc, out int barCount)
        {
            TimeSpan interval;//интервал для скачивания
            TimeSpan step; //расстояние между интервалами (атомарная величина)

            int maxMinutes = 4000;
            barCount = 5000;

            switch (period)
            {
                case (int)HistoryPeriod.Minute:
                    interval = TimeSpan.FromMinutes(maxMinutes - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(1);
                    break;

                case 5 * (int)HistoryPeriod.Minute:
                    interval = TimeSpan.FromMinutes(maxMinutes * 5 - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(5);
                    break;

                case 15 * (int)HistoryPeriod.Minute:
                    interval = TimeSpan.FromMinutes(maxMinutes * 15 - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(15);
                    break;

                case 30 * (int)HistoryPeriod.Minute:
                    interval = TimeSpan.FromMinutes(maxMinutes * 30 - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(30);
                    break;

                case (int)HistoryPeriod.Hour:
                    interval = TimeSpan.FromMinutes(maxMinutes * 60 - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(60);
                    break;

                case 4 * (int)HistoryPeriod.Hour:
                    interval = TimeSpan.FromMinutes(maxMinutes * 240 - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(240);
                    break;

                case (int)HistoryPeriod.Day:
                    interval = TimeSpan.FromMinutes(maxMinutes * 144 - 1);
                    fromTime = fromTime.AddMilliseconds(-fromTime.Millisecond).AddSeconds(-fromTime.Second);
                    step = TimeSpan.FromMinutes(1440);
                    break;               
                case (int)HistoryPeriod.Week:
                    barCount = 2000;
                    interval = TimeSpan.FromDays(7 * barCount - 1);
                    fromTime = fromTime.Date;
                    step = TimeSpan.FromMinutes(7);
                    break;
                case (int)HistoryPeriod.Month:
                    barCount = 500;
                    interval = TimeSpan.FromDays(30 * barCount - 1);
                    fromTime = fromTime.Date;
                    step = TimeSpan.FromDays(30);
                    break;

                default:
                    return new TimeRange[] { new TimeRange(fromTime, toTime) };
            }
            
            if (toTime > utc)
                toTime = utc;

            TimeSpan range = toTime - fromTime;
            int rangesCount = (int)Math.Ceiling((double)range.Ticks / interval.Ticks);

            TimeRange[] ranges = new TimeRange[rangesCount];
            for (int i = 0; i < rangesCount; i++)
            {
                DateTime left = fromTime.Add(interval);

                if (left > toTime)
                    left = toTime;

                ranges[i] = new TimeRange(fromTime, left);
                fromTime = left.Add(step);//чтобы не было дублирующихся баров
            }

            return ranges;
        }

        internal static SideEnum GetSide(double value)
        {
           
            return value > 0 ? SideEnum.Buy : SideEnum.Sell;
        }

        const int LIMIT_THREAD_GENERATE_BAR_MESSAGES = 3;
        
        internal static void GenerateDayBarMessages(ConcurrentDictionary<string, InstrumentOanda> instruments, CancellationToken cancellationToken, GetHistoryDelegate getHistory, Action<QuoteBase> newQuote)
        {
            SemaphoreSlim maxThread = new SemaphoreSlim(LIMIT_THREAD_GENERATE_BAR_MESSAGES);

            try
            {
                foreach (var instrument in instruments.Values)
                {
                    // Ждем пока освободится семафор
                    maxThread.Wait();

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    Task.Factory.StartNew((object obj) =>
                    {
                        GenerateDayBarMessages(obj as InstrumentOanda, cancellationToken, getHistory, newQuote);
                    }, instrument, TaskCreationOptions.LongRunning).ContinueWith((task) =>
                    {
                        maxThread.Release();
                    });
                }
            }
            catch
            {

            }
            finally
            {

            }
        }

        internal static void GenerateDayBarMessages(InstrumentOanda instrument, CancellationToken token, GetHistoryDelegate getHistory, Action<QuoteBase> newQuote)
        {
            var param = new HistoryRequestParameters();
            param.Symbol = instrument.Name.ToUpper();
            param.FromTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(-5).AddMilliseconds(-1);
            param.ToTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
            GenerateDayBarMessage(param, instrument, token, getHistory, newQuote);
        }

        private static void GenerateDayBarMessage(HistoryRequestParameters param, InstrumentOanda instrument, CancellationToken token, GetHistoryDelegate getHistory, Action<QuoteBase> newQuote)
        {
            try
            {
                if (token.IsCancellationRequested)
                    return;

                param.HistoryType = HistoryDataTypes.Bid;
                param.Period = (int)HistoryPeriod.Day;

                int count = 5000;
                List<BarHistoryItem> al = getHistory(param, param.FromTime, param.ToTime, (GranularityEnum)GetOandaTimeframe(param.Period), instrument, count);
                if (al == null || al.Count == 0)
                    return;

                if (token.IsCancellationRequested)
                    return;

                InstrumentDayBarMessage msg = new InstrumentDayBarMessage();
                msg.Symbol = param.Symbol;

                var bi0 = al[al.Count - 1];

                if (bi0 == null)
                    return;

                msg.Open = bi0.Open;
                msg.High = bi0.High;
                msg.Low = bi0.Low;
                msg.Ticks = (long)bi0.Volume;

                if (al.Count > 1)
                    msg.PreviousClosePrice = al[al.Count - 2].Close;

                if (token.IsCancellationRequested)
                    return;

               newQuote(msg);
            }
            catch (Exception ex)
            {
            }
        }

        internal static string ConvertDateTimeToOandaFormat(DateTime dateTime, bool convertEscaped = false)
        {
            string result = dateTime.ToString("s") + "Z";
            return convertEscaped ? Uri.EscapeDataString(result) : result;
        }

        internal static void CreateOrderRequest(OrderParameters modifyData, PlaceOrderRequest orderRequest, string orderId = null)
        {
            switch (modifyData.OrderType)
            {
                case OrderType.Market:
                    orderRequest.Order = new MarketOrderRequest()
                    {
                        Amount = (int)modifyData.Amount * (modifyData.Side == SideEnum.Buy ? 1 : -1),
                        InstrumentId = modifyData.Instrument.Id,
                        TimeInForce = OandaV20Utils.GetTiff(modifyData.TimeInForce),
                        PriceBound = modifyData.Slippage > 0 ? modifyData.Price + (modifyData.Side == SideEnum.Buy ? 1 : -1) * modifyData.Slippage * modifyData.Instrument.PointSize : -1
                    };
                    break;

                case OrderType.Limit:
                    if (string.IsNullOrEmpty(modifyData.PositionID))
                        orderRequest.Order = new LimitOrderRequest()
                        {
                            Amount = (int)modifyData.Amount * (modifyData.Side == SideEnum.Buy ? 1 : -1),
                            InstrumentId = modifyData.Instrument.Id,
                            TimeInForce = OandaV20Utils.GetTiff(modifyData.TimeInForce),
                            Price = modifyData.Price,
                            GTDTime = OandaV20Utils.GetOrderExpireDateByTIF(modifyData.TimeInForce, modifyData.OrderExpireDate, Vendor.UtcNowSynchronized)
                        };
                    else
                    {
                        orderRequest.Order = new TakeProfitOrderRequest()
                        {
                            TradeID = modifyData.PositionID,
                            Price = modifyData.Price,
                            TimeInForce = TimeInForceEnum.GTC
                        };
                    }
                    break;

                case OrderType.Stop:
                    if (string.IsNullOrEmpty(modifyData.PositionID))
                        orderRequest.Order = new StopOrderRequest()
                        {
                            Amount = (int)modifyData.Amount * (modifyData.Side == SideEnum.Buy ? 1 : -1),
                            InstrumentId = modifyData.Instrument.Id,
                            TimeInForce = OandaV20Utils.GetTiff(modifyData.TimeInForce),
                            Price = modifyData.Price,
                            GTDTime = OandaV20Utils.GetOrderExpireDateByTIF(modifyData.TimeInForce, modifyData.OrderExpireDate, Vendor.UtcNowSynchronized)
                        };
                    else
                    {
                        orderRequest.Order = new StopLossOrderRequest()
                        {
                            TradeID = modifyData.PositionID,
                            Price = modifyData.Price,
                            TimeInForce = TimeInForceEnum.GTC
                        };
                    }
                    break;
                case OrderType.TrailingStop:
                    if (!string.IsNullOrEmpty(modifyData.PositionID))
                    {
                        orderRequest.Order = new TrailingStopLossOrderRequest()
                        {
                            TradeID = modifyData.PositionID,
                            Distance = modifyData.Price * modifyData.Instrument.PointSize,
                            TimeInForce = TimeInForceEnum.GTC
                        };
                    }
                    break;
                default:
                    break;
            }

            var baseOrder = orderRequest.Order as MainOrderRequestBase;
            if (baseOrder != null)
            {
                if (modifyData.SlTpHolder != null)
                {
                    var holder = modifyData.SlTpHolder;
                    if (holder.TakeProfitPriceValue.HasValue)
                        if (holder.TakeProfitPriceType == CloseOrderPriceType.Absolute)
                        {
                            baseOrder.TakeProfitOnFill = new SlTpDetails()
                            {
                                Price = holder.TakeProfitPriceValue.Value,
                                TimeInForce = TimeInForceEnum.GTC
                            };
                        }
                        else
                        {
                            var isBuy = modifyData.Side == SideEnum.Buy;

                            baseOrder.TakeProfitOnFill = new SlTpDetails()
                            {
                                Price = modifyData.Price + (isBuy ? 1 : -1) * holder.TakeProfitPriceValue.Value * modifyData.Instrument.PointSize,
                                TimeInForce = TimeInForceEnum.GTC,
                            };
                        }

                    if (holder.StopLossPriceValue.HasValue)
                    {
                        if (holder.StopLossPriceType == CloseOrderPriceType.TrailingOffset)
                        {
                            baseOrder.TrailingStopLossOnFill = new TrailingStopLossDetails()
                            {
                                Distance = holder.StopLossPriceValue.Value * modifyData.Instrument.PointSize,
                                TimeInForce = TimeInForceEnum.GTC,
                            };
                        }
                        else if (holder.StopLossPriceType == CloseOrderPriceType.Absolute)
                        {
                            baseOrder.StopLossOnFill = new SlTpDetails()
                            {
                                Price = holder.StopLossPriceValue.Value,
                                TimeInForce = TimeInForceEnum.GTC
                            };
                        }
                        else
                        {
                            var isBuy = modifyData.Side == SideEnum.Buy;

                            baseOrder.StopLossOnFill = new SlTpDetails()
                            {
                                Price = modifyData.Price + (isBuy ? -1 : 1) * holder.StopLossPriceValue.Value * modifyData.Instrument.PointSize,
                                TimeInForce = TimeInForceEnum.GTC,
                            };
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(orderId))
                {
                    var oandaOrder = Rest.GetSingleOrder(modifyData.Account.AccountId, orderId);
                    if (oandaOrder != null)
                    {
                        baseOrder.TakeProfitOnFill = oandaOrder.TakeProfitOnFill;
                        baseOrder.StopLossOnFill = oandaOrder.StopLossOnFill;
                        baseOrder.TrailingStopLossOnFill = oandaOrder.TrailingStopLossOnFill;

                        double offset = modifyData.Price - oandaOrder.Price;

                        if (baseOrder.TakeProfitOnFill != null)
                        {
                            baseOrder.TakeProfitOnFill.Price += offset;
                        }

                        if (baseOrder.StopLossOnFill != null)
                        {
                            baseOrder.StopLossOnFill.Price += offset;
                        }
                    }
                }
            }
        }

        internal static void SetSLTP(Transaction transaction, InstrumentOanda inst, Order order)
        {
            if (transaction.TakeProfitOnFill != null)
            {
                order.TakeProfitPriceType = CloseOrderPriceType.Absolute;
                order.TakeProfitPriceValue = transaction.TakeProfitOnFill.Price;
            }

            if (transaction.StopLossOnFill != null)
            {
                order.StopLossPriceType = CloseOrderPriceType.Absolute;
                order.StopLossPriceValue = transaction.StopLossOnFill.Price;
            }

            if (transaction.TrailingStopLossOnFill != null)
            {
                order.StopLossPriceType = CloseOrderPriceType.TrailingOffset;
                order.StopLossPriceValue = transaction.TrailingStopLossOnFill.Distance / Math.Pow(0.1, inst.DisplayPrecision);
            }
        }

        internal static void ExtractExp1Exp2(string symbol, out string exp1, out string exp2, char devider = '_')
        {
            string[] parts = symbol.Split(devider);
            if (parts.Length == 2 && parts[0].Length > 0)
            {
                exp1 = parts[0];
                exp2 = parts[1];
            }
            else
            {
                exp1 = symbol;
                exp2 = "USD";
            }
        }

        internal static void FillAccountAditionalInfo(Account account, AccountState accountState)
        {
            account.AccountAdditionalInfo = new List<AccountAdditionalInfoItem>()
                    {
                        new AccountAdditionalInfoItem()
                        {

                            GroupInfo = KEY_GENERAL_GROUP,
                            SortIndex = 1000,
                            NameKey = "Withdrawal limit",
                            APIKey = "Withdrawal limit",
                            ToolTipKey = "Withdrawal limit",
                            Value = accountState != null ? accountState.WithdrawalLimit : 0.0,
                            DataType = AdditionalInfoItemComparingType.Double,
                            Visible = true,
                            FormatingType = AccountAdditionalInfoItemFormatingType.AssetBalance
                        },
                        new AccountAdditionalInfoItem()
                        {

                            GroupInfo = KEY_GENERAL_GROUP,
                            SortIndex = 1001,
                            NameKey = "Position value",
                            APIKey = "Position value",
                            ToolTipKey = "Position value",
                            Value = accountState != null ? accountState.PositionValue : 0.0,
                            DataType = AdditionalInfoItemComparingType.Double,
                            Visible = true,
                            FormatingType = AccountAdditionalInfoItemFormatingType.AssetBalance
                        },
                        new AccountAdditionalInfoItem()
                        {
                            GroupInfo = KEY_MARGIN_GROUP,
                            SortIndex = 1000,
                            NameKey = "Margin closeout value",
                            APIKey = "Margin closeout value",
                            ToolTipKey = "Margin closeout value",
                            Value = accountState != null ?  accountState.MarginCloseoutNAV : 0.0,
                            DataType = AdditionalInfoItemComparingType.Double,
                            Visible = true,
                            FormatingType = AccountAdditionalInfoItemFormatingType.AssetBalance
                        }
                     };
        }
    }

    internal struct TimeRange : IEquatable<TimeRange>
    {
        internal readonly DateTime From;
        internal readonly DateTime To;

        internal TimeRange(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TimeRange))
                return false;

            return Equals((TimeRange)obj);
        }
        public bool Equals(TimeRange tr)
        {
            return (To == tr.To && From == tr.From);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return From.GetHashCode() * To.GetHashCode();
            }
        }

        public override string ToString()
        {
            return From.ToString("dd.MM.yyyy HH:mm:ss.fff") + " - " + To.ToString("dd.MM.yyyy HH:mm:ss.fff");
        }
    }

    internal delegate List<BarHistoryItem> GetHistoryDelegate(HistoryRequestParameters historyParams, DateTime fromTime, DateTime toTime, GranularityEnum timeframe, InstrumentOanda inst, int count);

    enum ReportTypeEnum
    {
        OrderHistory,
        TradeHistory,
        TransactionHistory
    }
}
