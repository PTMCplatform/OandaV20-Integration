// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

//#define PUBLISH_FOR_GITHUB

using PlatformAPI.Integration;
using OandaV20ExternalVendor.TradeLibrary;
using OandaV20ExternalVendor.TradeLibrary.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using PTLRuntime.NETScript.Settings;

namespace OandaV20ExternalVendor
{
    public class OandaV20Vendor : PlatformAPI.Integration.Vendor
    {
        #region Properties/Const

        const string MARGIN_INFO_GROUP = "InstrumentDetailsPanel.3.MarginReg";

        const string CONNECTION = "Connection";
        const string CONNECTION_DEMO = "Demo";
        const string CONNECTION_REAL = "Real";

        /// <summary>
        /// AccountsSummary cache
        /// </summary>
        List<AccountSummaryOanda> accountsSummaryOanda = new List<AccountSummaryOanda>();

        /// <summary>
        /// Instruments cache
        /// </summary>
        ConcurrentDictionary<string, InstrumentOanda> instruments = new ConcurrentDictionary<string, InstrumentOanda>();

        /// <summary>
        /// Positions/trades cache
        /// </summary>
        ConcurrentDictionary<string, ConcurrentDictionary<string, Position>> positions = new ConcurrentDictionary<string, ConcurrentDictionary<string, Position>>();

        CancellationTokenSource startTransferAsyncCancellationToken;

        RatesSession rateStreamSession = null;

        HashSet<string> subscribedLevel1Instruments = new HashSet<string>();
        HashSet<string> subscribedLevel2Instruments = new HashSet<string>();
        ConcurrentDictionary<string, Price> lastQuoteCache = new ConcurrentDictionary<string, Price>();

        List<EventsSession> eventStreamingSessions = new List<EventsSession>();
        HashSet<string> orderToModify = new HashSet<string>();

        Stopwatch stopwatch = new Stopwatch();
        PingResult pingResult = new PingResult();

        System.Threading.Timer backGroundTimer;

        //убрать перед релизом
        bool processingNow = false;

        /// <summary>
        /// Максимальное количество закачек истории
        /// </summary>
        SemaphoreSlim historyThreadSemaphore = new SemaphoreSlim(10);

        #endregion

        #region Integration details

        /// <summary>
        /// Providing some information about vendor: name, registration link, etc. Terminal will use it in Login screen and Connection settings screen.
        /// </summary>        
        public override VendorMetaData GetVendorMetaData()
        {
            VendorMetaData value = new VendorMetaData()
            {
                VendorName = "Oanda v2.0",
                ForgotPasswordLink = @"https://www.oanda.com/demo-account/profile/change_password",
                RegistrationLink = @"http://protrader.org/brokers/oanda"
            };
            return value;
        }

        /// <summary>
        /// Define translations. Also we can use constant HIDDEN to hide unnecessary functionality.
        /// </summary>        
        public override Hashtable GetLocalizationTable()
        {
            var locale = new Hashtable()
            {
                { "general.Forex" , "Forex"},
                { "general.Commodity" , "Commodities"},
                { "general.Bond" , "Bonds"},
                { "general.Metal" , "Metals"},
                { "general.Index" , "Indices"},
                { "InstrumentDetailsPanel.Underlier" , HIDDEN},
                { "InstrumentDetailsPanel.DeliveryMethod" , HIDDEN},
                { "InstrumentDetailsPanel.ContactMonth" , HIDDEN},
                { "InstrumentDetailsPanel.FirstTradeDate" , HIDDEN},
                { "InstrumentDetailsPanel.LastTradeDate" , HIDDEN},
                { "InstrumentDetailsPanel.NoticeDate" , HIDDEN},
                { "InstrumentDetailsPanel.CloseOutDeadline" , HIDDEN},
                { "InstrumentDetailsPanel.StrikePrice" , HIDDEN},
                { "InstrumentDetailsPanel.TradingBalance" , HIDDEN},
                { "InstrumentDetailsPanel.DeliveryStatus" , HIDDEN},
                { "InstrumentDetailsPanel.KEYNextHoliday" , HIDDEN},
                { "InstrumentDetailsPanel.ContractSize" , HIDDEN},
                { "InstrumentDetailsPanel.MaxPositionQtyPerSymbol" , HIDDEN},
                { "InstrumentDetailsPanel.HiLimit" , HIDDEN},
                { "InstrumentDetailsPanel.LowLimit" , HIDDEN},
                { "InstrumentDetailsPanel.TickCoast" , HIDDEN},
                { "InstrumentDetailsPanel.MarginBuy" , HIDDEN},
                { "InstrumentDetailsPanel.MarginSell" , HIDDEN},
                { "InstrumentDetailsPanel.MarginDay" , HIDDEN},
                { "InstrumentDetailsPanel.MarginOvernight" , HIDDEN},
                { "InstrumentDetailsPanel.MarginDayBuy" , HIDDEN},
                { "InstrumentDetailsPanel.MarginDaySell" , HIDDEN},
                { "InstrumentDetailsPanel.MarginOvernightBuy" , HIDDEN},
                { "InstrumentDetailsPanel.MarginOvernightSell" , HIDDEN},
                { "InstrumentDetailsPanel.PerLot" , HIDDEN},
                { "InstrumentDetailsPanel.PerFill" , HIDDEN},
                { "InstrumentDetailsPanel.PerTransaction" , HIDDEN},
                { "InstrumentDetailsPanel.PerPhoneTransaction" , HIDDEN},
                { "InstrumentDetailsPanel.PerVolume" , HIDDEN},
                { "InstrumentDetailsPanel.VAT" , HIDDEN},
                { "InstrumentDetailsPanel.5.Rebates" , HIDDEN},
                { "InstrumentDetailsPanel.POSTCLOSE" , HIDDEN},
                { "InstrumentDetailsPanel.PREOPEN" , HIDDEN},

                { "panel.watchlist.IndicativeAuctionPrice" , HIDDEN},
                { "panel.watchlist.SettlementPrice" , HIDDEN},
                { "panel.watchlist.Match Volume" , HIDDEN},
                { "panel.watchlist.Last" , HIDDEN},
                { "panel.watchlist.LastSize" , HIDDEN},
                { "panel.watchlist.Exchange" , HIDDEN},
                { "panel.watchlist.ContractSize" , HIDDEN},
                { "panel.watchlist.ExpDate" , HIDDEN},               
                { "panel.watchlist.PrevSettlementPrice",HIDDEN},


                { "panel.accounts.CreditValue" , HIDDEN},
                { "panel.accounts.CurBalance" , HIDDEN},
                { "panel.accounts.CashBalance" , HIDDEN},
                { "panel.accounts.Estimate value" , HIDDEN},
                { "panel.accounts.BlockedForStocksNew" , HIDDEN},
                { "panel.accounts.MarginWarning" , HIDDEN},
                { "panel.accounts.TodayFees" , HIDDEN},
                { "panel.accounts.MaxGrossLoss" , HIDDEN},
                { "panel.accounts.MaxDayVolume" , HIDDEN},
                { "panel.accounts.MaxPositions" , HIDDEN},
                { "panel.accounts.MaxPendingOrders" , HIDDEN},
                { "panel.accounts.MaxOrderCapital" , HIDDEN},
                { "panel.accounts.Owner" , HIDDEN},
                { "panel.accounts.Email" , HIDDEN},
                { "panel.accounts.AccountType" , HIDDEN},
                { "panel.accounts.AccountMode" , HIDDEN},
                { "panel.accounts.TelephoneNumber" , HIDDEN},
                { "panel.accounts.AssetDescription" , HIDDEN},
                { "panel.accounts.AssetType" , HIDDEN},
                { "panel.accounts.InterestRate" , HIDDEN},
                { "panel.accounts.UserID" , "Account ID"},
                { "panel.accounts.accountDetails.RiskManagement.TradingStatus.MarginWarning" , "Warning about possible stop out"},
                { "panel.accounts.stopOutPercent" , "Margin rate, %"},
                { "panel.accounts.stopOutPercent.descr" , "Margin rate, %"},
                { "panel.accounts.marginWarnPercent" , "Margin сloseout percent, %"},
                { "panel.accounts.marginWarnPercent.descr" , "Margin сloseout percent, %"},
                
                { "panel.positions.swaps" , HIDDEN},
                { "panel.positions.ExpDate" , HIDDEN},
                { "panel.positions.strike" , HIDDEN},
                { "panel.positions.commissions" , HIDDEN},
                
                { "panel.orders.ExpirationDate" , HIDDEN},
                { "panel.orders.StrikePrice" , HIDDEN},
                { "panel.orders.Exchange" , HIDDEN},
                { "panel.orders.Status" , HIDDEN},
                { "panel.orders.OCOGrouID" , HIDDEN},
                { "panel.orders.QuantityFilled" , HIDDEN},
                { "panel.orders.QuantityRemaining" , HIDDEN},
                
                { "panel.newOrderEntry.showVWPPanel" , HIDDEN},

                { "panel.BasketOrder.OptionsType" , HIDDEN},
                { "panel.BasketOrder.StrikePrice" , HIDDEN},
                { "panel.BasketOrder.ExpiryDate" , HIDDEN},
                { "panel.BasketOrder.status" , HIDDEN},
                { "panel.BasketOrder.last" , HIDDEN},

                { "InstrumentDetailsPanel.Margin_ByAccount" , HIDDEN},
                { "InstrumentDetailsPanel.SettlementDate" , HIDDEN},
                
                { "panel.PositionBalance.StrikePrice" , HIDDEN},
                { "panel.PositionBalance.ExpirationDate" , HIDDEN},
                { "panel.PositionBalance.Buy" , HIDDEN},
                { "panel.PositionBalance.Sell" , HIDDEN},

                { "chart.infoWindow.Items.Volume" , HIDDEN},

                { "panel.watchlist.Volume" , HIDDEN},
                
                { "property.general.FutureScheme" , HIDDEN},
                { "property.general.StrikeFormat" , HIDDEN},
                { "property.general.OptionSeriesFormat" , HIDDEN},
                { "property.general.OptionSeparatorFormat" , HIDDEN},
                { "property.general.OvernightMarginNotificationMessage" , HIDDEN},
                { "property.general.ShowInstrumentHalt" , HIDDEN},

                { "panel.NewTrades.StrikePrice" , HIDDEN},
                { "panel.NewTrades.ExpirationDate" , HIDDEN},
                { "panel.NewTrades.Commission" , HIDDEN},
                { "panel.NewTrades.OrderType", HIDDEN },
                { "panel.NewTrades.Login", HIDDEN },

                { "StatusBasSettingsScreen.News" , HIDDEN},
                { "StatusBasSettingsScreen.Chat" , HIDDEN},
                
                { "panel.newOrderEntry.riskDetailsLabel" , HIDDEN},

                { "property.MD.isStopLimitOrder" , HIDDEN},

                { "panel.EventLog.Roundtrip", HIDDEN },

                {"general.showLotsFX", HIDDEN },
                                                
                {"property.isAbbreviateVolumes", HIDDEN },
                {"property.TIF_Limit_And_Stop_Limit", "Limit TIF" },
                {"ribbon.help.viewPrivateLog", HIDDEN },

                {"InstrumentDetailsPanel.SwapBuy", HIDDEN },
                {"InstrumentDetailsPanel.SwapSell", HIDDEN },

                {"panel.OrderBook.StrikePrice", HIDDEN },
                {"panel.OrderBook.ExpirationDate", HIDDEN },
                {"panel.OrderBook.TraderID", HIDDEN },
                
                {"panel.accounts.UnsettledCash", HIDDEN },
                {"screen.about.copyrightUrl", "http://protrader.org/privacy-policy" },
                
                {"property.Last", HIDDEN },
                
                {"screen.Protrader2LoginScreen.RegistrationLinkLabel", "Create account" },
                {"screen.Protrader2LoginScreen.RegistrationLinkLabel.ToolTip", "Account registration" },
                
                {"panel.accounts.StocksValue", HIDDEN },
                
                {"panel.accounts.TrailingDrawdownLevel", HIDDEN },
                {"panel.accounts.TotalMaxPositionsQty", HIDDEN },
                {"panel.accounts.MaxWeekLostLimit", HIDDEN },
                {"panel.accounts.MaxDailyLossLimit", HIDDEN },
                
                {"panel.accounts.StocksOrdersReq", HIDDEN },
                
                {"panel.accounts.LockedBalance", HIDDEN },
                
                {"panel.accounts.CurrentDailyLoss", HIDDEN },
                {"panel.accounts.CurrentWeeklyLoss", HIDDEN },

                {"InstrumentDetailsPanel.Margin", HIDDEN },
                {"panel.accounts.WithdrawalAvailable", HIDDEN },
                {"reports.Send from", HIDDEN },
                {"panel.detail.IndAuctionPrice", HIDDEN },
                {"panel.detail.LastSize", HIDDEN },
                {"panel.detail.last", HIDDEN },
                {"panel.detail.volume", HIDDEN },
                {"panel.detail.SettlementPrice", HIDDEN },
                {"panel.detail.PrevSettlementPrice", HIDDEN },
                {"panels.positions.buttons.reverseAll", HIDDEN },
                {"panels.positions.buttons.reverseForInstrument", HIDDEN },
                {"panels.positions.buttons.reverseSelected", HIDDEN },
                {"panel.positions.menu.reverse.reverseAll", HIDDEN },
                {"panel.positions.menu.reverse.reverseAllBySymbol", HIDDEN },
                {"panel.positions.menu.reverse.reverseAllForAccount", HIDDEN },
                {"panel.positions.menu.reverse", HIDDEN },
                {"panel.newOrderEntry.reversePosButton", HIDDEN },
                {"panel.positions.Net_PL", HIDDEN },
                {"panel.positions.Net_PL.descr", HIDDEN },
                {"panel.positions.RealizedPnL", HIDDEN },
                {"panel.positions.RealizedPnL.descr", HIDDEN },
                {"panel.positions.profit_usd.InstrumentCurrency", "Gross P/L" },
                {"panel.positions.Net_PL.InstrumentCurrency", "Net P/L" },
                {"panel.positions.Net_PL.InstrumentCurrency.descr", "Net P/L" },
                {"panel.positions.PL_ticks", HIDDEN },
                {"panel.positions.profit_usd", HIDDEN }
            };

            return locale;
        }

        #endregion VendorServices

        #region Connection

        /// <summary>
        /// Get connection settings. Oanda requires specified type of connection: Demo or Real.
        /// </summary>        
        public override List<SettingItem> GetConnectionParameters()
        {
            List<SettingItem> parameters = new List<SettingItem>();

            // Demo/Real conenction type
            var settingItem = new SettingItemComboBox(CONNECTION, CONNECTION_DEMO, new List<string> { CONNECTION_DEMO, CONNECTION_REAL });
            parameters.Add(settingItem);

            return parameters;
        }

        /// <summary>
        /// Implementation of connecting to Oanda server
        /// </summary>        
        public override ConnectionResult Connect(List<SettingItem> parameters)
        {
            ConnectionResult result = new ConnectionResult();
            bool isLive = false;
            
            SettingItem settingItem;
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                result.State = ConnectionState.Fail;
                result.Message =  "connect.connection.connect.error";
               
                return result;
            }

            string user = string.Empty;
            string password = string.Empty;

            settingItem = parameters.GetItemByName(Vendor.LOGIN_PARAMETER_USER);
            if (settingItem != null && settingItem.Value is string)
                user = (string)settingItem.Value;
            settingItem = parameters.GetItemByName(Vendor.LOGIN_PARAMETER_PASSWORD);
            if (settingItem != null && settingItem.Value is string)
                password = (string)settingItem.Value;

            settingItem = parameters.GetItemByName(CONNECTION);
            if (settingItem != null && settingItem.Value is string)
                isLive = (string)settingItem.Value == CONNECTION_REAL;

#if PUBLISH_FOR_GITHUB

            string returnedToken = password.Length > 20 ? password : string.Empty;

#else

            Control invoker = null;
            settingItem = parameters.GetItemByName("MainGUIControl");
            if (settingItem != null && settingItem.Value is Control)
                invoker = (Control)settingItem.Value;

            string returnedToken = password.Length > 20 ? password : string.Empty;

            Credentials.SetCredentials(isLive ? EEnvironment.Trade : EEnvironment.Practice, returnedToken);

            if (string.IsNullOrEmpty(returnedToken))
            {
                string error = null;
                if (isLive)
                    error = OandaV20Utils.GetTokenFromWebasync("7pj8oP0HQ41dCXge", "https://cp.protrader.com/oauth.html", user, password, invoker);
                else
                    error = OandaV20Utils.GetTokenFromWebasync("62WGrveR6H8muPk9", "https://cp.protrader.com/oauth.html", user, password, invoker);

                if (!string.IsNullOrEmpty(error))
                {
                    result.State = ConnectionState.Fail;
                    result.Message = error;
                    
                    return result;
                }

                returnedToken = OandaV20Utils.AccessToken;
            }

#endif


            if (string.IsNullOrEmpty(returnedToken))
            {
                result.State = ConnectionState.Fail;
                result.Message = "User/password combination is not valid.";
              
                return result;
            }

            Credentials.SetCredentials(isLive ? EEnvironment.Trade : EEnvironment.Practice, returnedToken);

            result.State = ConnectionState.Success;

            return result;
        }

        /// <summary>
        /// Use 'Disconnect' function for cleaning up resources
        /// </summary>
        public override void Disconnect()
        {
            OandaV20Utils.DestroyWebForm();

            if (backGroundTimer != null)
            {
                backGroundTimer.Dispose();
                backGroundTimer = null;
            }

            startTransferAsyncCancellationToken?.Cancel();

            UnsubscribeStreamingData();
           
            if (positions != null)
            {
                positions.Clear();
                positions = null;
            }

            if (orderToModify != null)
            {
                orderToModify.Clear();
                orderToModify = null;
            }
        }

        #endregion Connection

        #region Accounts and rules
              
        /// <summary>
        /// Fill information about available accounts. 
        /// </summary>        
        public override IList<Account> GetAccounts()
        {
            List<Account> accounts = new List<Account>();
            
            List<AccountOanda> accountsOanda = null;
            try
            {
                accountsOanda = Rest.GetAccountListAsync();
            }
            catch(Exception ex)
            {
                throw ex.InnerException ?? ex;
            }

            if (accountsOanda.Count == 0)
                throw new Exception("The Oanda v20 only has access to the v20 accounts. Please, contact customer service to setup a sub-account that can be used with this platform.");

            for (int i = 0; i < accountsOanda.Count; i++)
            {
                AccountSummaryOanda accountSummary = Rest.GetAccountSummary(accountsOanda[i].Id);

                if (accountSummary == null)
                    continue;
                
                this.accountsSummaryOanda.Add(accountSummary);

                Account account = new Account();
                account.AccountId = accountSummary.Id;
                account.AccountName = accountSummary.Name;
                account.Currency = accountSummary.Currency;                
                account.marginWarningLevelPercent = accountSummary.MarginRate;
                account.Balance = accountSummary.Balance;
                account.RealizedPnl = accountSummary.Pl;
                account.UsedMargin = accountSummary.MarginUsed;
                account.AvailableMargin = accountSummary.MarginAvaliable;
                account.MaintranceMargin = accountSummary.MarginUsed;//??

                OandaV20Utils.FillAccountAditionalInfo(account, null);

                accounts.Add(account);                

                positions[accountSummary.Id] = new ConcurrentDictionary<string, Position>();
            }

            return accounts;
        }

        /// <summary>
        /// Specify rules for account. We can control allowed functionality via these rules.
        /// </summary>        
        public override Dictionary<Rule, object> GetRulesTable(Account account)
        {
            Dictionary<Rule, object> ruleSet = new Dictionary<Rule, object>();

            ruleSet[Rule.VISIBILITY_INSTRUMENT_TYPE] = Vendor.ALLOWED_FOR_ALL;
            ruleSet[Rule.VISIBILITY_ROUTE] = Vendor.ALLOWED_FOR_ALL;

            ruleSet[Rule.FUNCTION_INSTRUMENT_PORTFOLIO] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_ORDER_BOOK] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_LEVEL2] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_SCALPER] = Vendor.NOT_ALLOWED;

            ruleSet[Rule.FUNCTION_OE2014] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_CHART] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_CHAT] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_NEWS] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_SHOW_ORDERS] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_SHOW_POSITIONS] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_ACCOUNTS] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_ACCOUNTPERFOMANCE] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_WATCHLIST] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_FXBOARD] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_LEVEL3] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_MATRIX] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_EVENT_LOG] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_POSITION_BALANCE] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_CUR_EXP] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_BUILDER] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_BASKET] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_SCRIPT_BUILDER] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_SYMBOL_INFO] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_GRID] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_OPTION_TRADER] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_MARGIN_INFO] = Vendor.NOT_ALLOWED;

            ruleSet[Rule.SELF_TRADING] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_TRADING] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_REPORT] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_MUTUAL_CLOSE] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_PARTIAL_CLOSE] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_SLTP] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_TRAILING_STOP] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_BINDEDORDERS] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_TRADES] = Vendor.ALLOWED;

            ruleSet[Rule.FUNCTION_MODIFY_ORDERTYPE] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_MODIFY_AMOUNT] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_MODIFY_PRICE] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_MODIFY_TIF] = Vendor.ALLOWED;

            ruleSet[Rule.FUNCTION_AUTO_POPULATE_WATCHLIST] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_ADVANCED_OCO_MODE] = Vendor.NOT_ALLOWED;

            ruleSet[Rule.FUNCTION_SCRIPT_BUILDER] = Vendor.ALLOWED;
            ruleSet[Rule.FUNCTION_TRAILING_SLTP_INOFFSET] = Vendor.ALLOWED;

            ruleSet[Rule.FUNCTION_DONT_USE_ROUTE_SETTING_FOR_CLOSE_TS_ORDER] = Vendor.ALLOWED;

            ruleSet[Rule.FUNCTION_UPDATE_TRAILING_STOP_ON_NEWQUOTE] = Vendor.ALLOWED;

            ruleSet[Rule.FUNCTION_USE_ONE_ROUTE] = Vendor.ALLOWED;

            ruleSet[Rule.FUNCTION_BASE_CURRENCY] = account.Currency;

            ruleSet[Rule.FUNCTION_ALLOW_ORDERS_HISTORY] = Vendor.NOT_ALLOWED;
            ruleSet[Rule.FUNCTION_ALLOW_TRADES_HISTORY] = Vendor.ALLOWED;
            ruleSet[Rule.VALUE_EXPOSITION_IN_IMSTRUMENT_CURRENCY] = Vendor.ALLOWED;

            return ruleSet;
        }

        #endregion

        #region Instruments and instrument groups      

        /// <summary>
        /// Fill information about available instruments. 
        /// </summary>        
        public override IList<Instrument> GetInstruments()
        {
            List<Instrument> instruments = new List<Instrument>();
            List<InstrumentOanda> instrumentsOanda = Rest.GetAllInstrumentsAsync(accountsSummaryOanda[0].Id);

            foreach (InstrumentOanda instrumentOanda in instrumentsOanda)
            {
                instrumentOanda.Name = instrumentOanda.Name.ToUpper();

                this.instruments[instrumentOanda.Id] = instrumentOanda;

                Instrument instrument = new Instrument();

                instrument.Id = instrumentOanda.Id;
                instrument.Name = instrumentOanda.Name.ToUpper();
                instrument.InstrumentType = OandaV20Utils.GetInstrumentType(instrumentOanda.InstrumentType);
                instrument.InstrumentGroup = OandaV20Utils.GetInstrumentGroup(instrument.Name);
                instrument.DefaultHistoryType = HistoryDataTypes.Bid;
                instrument.LotSize = instrumentOanda.MinimumTradeSize;
                instrument.PointSize = Math.Pow(0.1, instrumentOanda.DisplayPrecision);
                instrument.Precision = (byte)instrumentOanda.DisplayPrecision;
                instrument.MaxLot = instrumentOanda.MaximumOrderUnits;

                OandaV20Utils.ExtractExp1Exp2(instrumentOanda.Id, out instrument.Exp1, out instrument.Exp2);

                //
                // 
                //
                if (accountsSummaryOanda.Count == 1)
                {
                    instrument.AdditionalInfo = new List<InstrumentAdditionalInfoItem>()
                    {
                        new InstrumentAdditionalInfoItem()
                        {
                            GroupInfo = MARGIN_INFO_GROUP,
                            SortIndex = 10,

                            APIKey = "Margin rate",
                            NameKey = "Margin rate",
                            ToolTipKey = "The margin rate for this instrument.",
                            DataType = AdditionalInfoItemComparingType.Double,
                            Value = instrumentOanda.MarginRate,
                            Visible = true,
                        }
                    };
                }

                //session

                instruments.Add(instrument);
            }

          

            return instruments;
        }

        /// <summary>
        /// Get available instrument groups
        /// </summary>        
        public override IList<InstrumentGroupEnum> GetInstrumentGroups()
        {
            return new List<InstrumentGroupEnum>()
            {
                InstrumentGroupEnum.Forex,
                InstrumentGroupEnum.Index,
                InstrumentGroupEnum.Commodity,
                InstrumentGroupEnum.Bond,
                InstrumentGroupEnum.Metal
            };
        }

        #endregion
      
        #region Positions/orders/trades
                                      
        /// <summary>
        /// Fill information about opened orders at the time of connection
        /// </summary>        
        public override IList<Order> GetPendingOrders()
        {
            List<Order> orders = new List<Order>();
            
            foreach (AccountSummaryOanda accountSummaryOanda in accountsSummaryOanda)
            {
                               
                List<OrderOanda> pendingOrders = Rest.GetPendingOrders(accountSummaryOanda.Id);

                foreach (OrderOanda orderOanda in pendingOrders)
                {
                    Order order = new Order();
                    order.Side = OandaV20Utils.GetSide(orderOanda.Amount);
                    order.Quantity = Math.Abs(orderOanda.Amount);

                    var instrumentKey = orderOanda.InstrumentId;

                    if (orderOanda.OrderType == OrderTypeOanda.TakeProfit || orderOanda.OrderType == OrderTypeOanda.StopLoss || orderOanda.OrderType == OrderTypeOanda.TrailingStopLoss)
                    {
                        var tradeOanda = Rest.GetSingleTrade(accountSummaryOanda.Id, orderOanda.TradeID);
                        if (tradeOanda == null)
                            continue;

                        instrumentKey = tradeOanda.InstrumentId;
                        order.PositionId = orderOanda.TradeID;
                        order.Quantity = Math.Abs(tradeOanda.CurrentUnits);
                        order.Side = OandaV20Utils.GetSide(-tradeOanda.CurrentUnits);
                    }

                    InstrumentOanda inst;
                    if (!instruments.TryGetValue(instrumentKey, out inst))
                        continue;


                    order.OrderId = orderOanda.Id;
                    order.AccountId = accountSummaryOanda.Id;
                    order.OrderType = OandaV20Utils.GetOrderType(orderOanda.OrderType);                    
                    order.Symbol = inst.Name.ToUpper();
                    order.Price = orderOanda.Price != 0 ? orderOanda.Price : orderOanda.Distance;
                    order.Tiff = OandaV20Utils.GetTiff(orderOanda.TimeInForce);
                    order.ExpirationTime = orderOanda.GTDTime;
                    order.LastUpdateTime = orderOanda.CreationTime;

                    if (orderOanda.TakeProfitOnFill != null)
                    {
                        order.TakeProfitPriceType = CloseOrderPriceType.Absolute;
                        order.TakeProfitPriceValue = orderOanda.TakeProfitOnFill.Price;
                    }

                    if (orderOanda.StopLossOnFill != null)
                    {
                        order.StopLossPriceType = CloseOrderPriceType.Absolute;
                        order.StopLossPriceValue = orderOanda.StopLossOnFill.Price;
                    }

                    if (orderOanda.TrailingStopLossOnFill != null)
                    {
                        order.StopLossPriceType = CloseOrderPriceType.TrailingOffset;
                        order.StopLossPriceValue = orderOanda.TrailingStopLossOnFill.Distance / Math.Pow(0.1, inst.DisplayPrecision);
                    }


                    orders.Add(order);
                }
            }

            return orders;
        }

        /// <summary>
        /// Fill information about positions at the time of connection
        /// </summary>        
        public override IList<Position> GetPositions()
        {
            List<Position> positions = new List<Position>();

            foreach (AccountSummaryOanda account in accountsSummaryOanda)
            {
                List<TradeOanda> openTrades = Rest.GetOpenTrades(account.Id);

                foreach (TradeOanda tradeOanda in openTrades)
                {
                    Position position = new Position();

                    InstrumentOanda inst;
                    if (!instruments.TryGetValue(tradeOanda.InstrumentId, out inst))
                        continue;
                    
                    position.AccountId = account.Id;
                    position.PositionId = tradeOanda.Id;
                    position.Symbol = inst.Name.ToUpper();
                    position.OpenPrice = tradeOanda.Price;
                    position.Quantity = Math.Abs(tradeOanda.CurrentUnits);
                    position.Side = OandaV20Utils.GetSide(tradeOanda.CurrentUnits);
                    position.OpenTime = tradeOanda.OpenTime;
                    position.ServerCalculationNetPL = position.ServerCalculationGrossPL = tradeOanda.UnrealizedPL;

                    positions.Add(position);

                    ConcurrentDictionary<string, Position> trades;
                    if (this.positions.TryGetValue(account.Id, out trades))                    
                        trades[tradeOanda.Id] = position;                    
                }
            }

            return positions;
        }
       
        /// <summary>
        /// Get orders history
        /// </summary>        
        public override List<Order> GetOrdersHistory(DateTime from, DateTime to)
        {
            List<Transaction> transactions = new List<Transaction>();

            foreach (var accountSummaryOanda in accountsSummaryOanda)
            {
                var transactionsPage = Rest.GetTransactionsPage(accountSummaryOanda.Id, from, to);

                foreach (var page in transactionsPage.Pages)
                    transactions.AddRange(Rest.GetTransactions(page));
            }

            //
            List<Order> result = new List<Order>();

            foreach(var transaction in transactions)
                if (transaction.TransactionType == TransactionType.LimitOrder ||
                    transaction.TransactionType == TransactionType.MarketIfTouchedOrder ||
                    transaction.TransactionType == TransactionType.MarketOrder ||
                    transaction.TransactionType == TransactionType.OrderCancel ||
                    transaction.TransactionType == TransactionType.OrderFill ||
                    transaction.TransactionType == TransactionType.StopLossOrder ||
                    transaction.TransactionType == TransactionType.StopOrder ||
                    transaction.TransactionType == TransactionType.TakeProfitOrder ||
                    transaction.TransactionType == TransactionType.TrailingStopLossOrder)
                {
                    OrderStatus status = OrderStatus.New;
                    OrderType orderType = OrderType.Advanced;

                    switch(transaction.TransactionType)
                    {
                        case TransactionType.LimitOrder:
                        case TransactionType.TakeProfitOrder:
                            orderType = OrderType.Limit;
                            break;
                        case TransactionType.StopOrder:
                        case TransactionType.StopLossOrder:
                            orderType = OrderType.Stop;
                            break;
                        case TransactionType.MarketOrder:
                            orderType = OrderType.Market;
                            break;
                        case TransactionType.TrailingStopLossOrder:
                            orderType = OrderType.TrailingStop;
                            break;

                        case TransactionType.OrderCancel:
                            status = OrderStatus.Canceled;
                            break;
                        case TransactionType.OrderFill:
                            status = OrderStatus.Filled;
                            break;
                    }
                    
                    Order order = new Order();

                    InstrumentOanda instrument = null;
                    if (!string.IsNullOrEmpty(transaction.InstrumentId))
                        instruments.TryGetValue(transaction.InstrumentId, out instrument);

                    order.AccountId = transaction.AccountID;
                    order.Symbol = instrument != null ? instrument.Name : "";
                    order.Side = transaction.Amount > 0 ? SideEnum.Buy : SideEnum.Sell;
                    order.Status = status;
                    order.OrderId = transaction.OrderID;
                    order.OrderType = orderType;
                    order.Price = transaction.Price;
                    order.Quantity = transaction.Amount;
                    order.LastUpdateTime = transaction.Time;
                    order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                    order.SymbolType = instrument != null ? instrument.InstrumentType.ToString() : "";

                    result.Add(order);
                }

            return result;
        }

        /// <summary>
        /// Get trades history
        /// </summary>        
        public override List<Trade> GetTradesHistory(DateTime from, DateTime to)
        {
            List<Transaction> transactions = new List<Transaction>();

            foreach (var accountSummaryOanda in accountsSummaryOanda)
            {
                var transactionsPage = Rest.GetTransactionsPage(accountSummaryOanda.Id, from, to);

                foreach (var page in transactionsPage.Pages)
                    transactions.AddRange(Rest.GetTransactions(page));               
            }

            //
            List<Trade> result = new List<Trade>();

            foreach (var transaction in transactions)
                if (transaction.TransactionType == TransactionType.OrderFill)
                {
                    Trade trade = GenerateTrade(transaction);
                    result.Add(trade);
                }

            return result;
        }

        #endregion

        #region Subscriptions: Level1 and level2 quotes
      
        /// <summary>
        /// Subscribing to quote data: Level1 and Level2
        /// </summary>      
        public override bool SubscribeSymbol(SubscribeQuotesParameters parameters)
        {
            if (parameters.subscribeType == SubscribeQuoteType.Level1)
            {
                if (!subscribedLevel1Instruments.Contains(parameters.symbol))                
                    subscribedLevel1Instruments.Add(parameters.symbol);
                                
                return true;
            }

            if (parameters.subscribeType == SubscribeQuoteType.Level2)
            {
                if (!subscribedLevel2Instruments.Contains(parameters.symbol))                
                    subscribedLevel2Instruments.Add(parameters.symbol);
                
                return true;
            }

            Price price = null;
            if (lastQuoteCache.TryGetValue(parameters.symbol, out price))
                ProcessQuote(price, parameters.symbol);

            return false;
        }

        /// <summary>
        /// Unsubscribing from quote data: Level1 and Level2
        /// </summary>
        public override bool UnSubscribeSymbol(SubscribeQuotesParameters parameters)
        {
            if (parameters.subscribeType == SubscribeQuoteType.Level1)
            {
                if (subscribedLevel1Instruments.Contains(parameters.symbol))                
                    subscribedLevel1Instruments.Remove(parameters.symbol);
                                
                return true;
            }

            if (parameters.subscribeType == SubscribeQuoteType.Level2)
            {
                if (subscribedLevel2Instruments.Contains(parameters.symbol))                
                    subscribedLevel2Instruments.Remove(parameters.symbol);
                
                return true;
            }

            return false;
        }
       
        #endregion Quotes

        #region History

        /// <summary>
        /// Loading history by passed parameters
        /// </summary>        
        public override IList<BarHistoryItem> LoadBarHistory(HistoryRequestParameters requestParaeters, CancellationToken token)
        {
            DateTime fromTime = requestParaeters.FromTime;
            DateTime toTime = requestParaeters.ToTime;

            DateTime now = UtcNowSynchronized;

            if (toTime > now)
                toTime = now;

            bool onlyMainSession = requestParaeters.OnlyMainSession;
            int downLoadingPeriod = requestParaeters.Period;

            List<BarHistoryItem> res = null;

            try
            {
                if (token != CancellationToken.None && token.IsCancellationRequested)
                    return res;

                GranularityEnum? timeframe = OandaV20Utils.GetOandaTimeframe(downLoadingPeriod);
                if (!timeframe.HasValue)
                    return res;


                InstrumentOanda inst = instruments.Where(x => x.Value.Name.ToUpper() == requestParaeters.Symbol).FirstOrDefault().Value;

                if (inst == null)
                    return res;
                int count = 0;

                //split history to multithreaded downloading
                TimeRange[] ranges = OandaV20Utils.SplitHistory(downLoadingPeriod, fromTime, toTime, UtcNowSynchronized, out count);

                //1 кусок - качаем сразу
                if (ranges.Length <= 1)
                {
                    // We wait until the semaphore is cleared
                    historyThreadSemaphore.Wait();

                    try
                    {
                        var rng = ranges[0];
                        res = GetHistory(requestParaeters, fromTime, toTime, (GranularityEnum)timeframe, inst, count);
                    }
                    catch { }
                    finally
                    {
                        historyThreadSemaphore.Release();
                    }
                }
                else
                {
                    res = new List<BarHistoryItem>();


                    var tasks = new List<Task<List<BarHistoryItem>>>();

                    for (int i = 0; i < ranges.Length; i++)
                    {
                        TimeRange rng = ranges[i];

                        if (token.IsCancellationRequested)
                            return res;

                        // We wait until the semaphore is cleared
                        historyThreadSemaphore.Wait();

                        if (token.IsCancellationRequested)
                            return res;


                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            return GetHistory(requestParaeters, rng.From, rng.To, (GranularityEnum)timeframe, inst, count);
                        }, TaskCreationOptions.LongRunning).ContinueWith((task) =>
                        {
                            historyThreadSemaphore.Release();
                            return task.Result;
                        }));
                    }

                    //downloading by thread pool
                    Task.WaitAll(tasks.ToArray());

                    //wait a little, but will avoid memory overrun
                    int historyCount = tasks.Sum(x => x.Result != null ? x.Result.Count : 0);
                    List<BarHistoryItem> combined = new List<BarHistoryItem>(historyCount);

                    var resultsList = tasks.Select(x => x.Result);
                    foreach (List<BarHistoryItem> list in resultsList)
                        if (list != null && list.Count > 0)
                            combined.AddRange(list);

                    res = combined;
                }
            }
            catch (Exception ex)
            {
                this.Log(ex);
            }

            return res;
        }
        
        /// <summary>
        /// Information about history types and periods provided by integration
        /// </summary>        
        public override HistoryMetadata GetHistoryMetadata()
        {
            return new HistoryMetadata()
            {
                //
                // Supported history types
                //
                HistoryTypes = new HistoryDataTypes[]
                {
                    HistoryDataTypes.Bid,
                    HistoryDataTypes.Ask,
                    HistoryDataTypes.BidAskAverage
                },
                
                //
                // Supported periods
                //
                Periods = new int[]
                {
                    5 * (int)HistoryPeriod.Second,
                    10 * (int)HistoryPeriod.Second,
                    15 * (int)HistoryPeriod.Second,
                    30 * (int)HistoryPeriod.Second,           
                    (int)HistoryPeriod.Minute,
                    2 * (int)HistoryPeriod.Minute,
                    3 * (int)HistoryPeriod.Minute,
                    4 * (int)HistoryPeriod.Minute,
                    5 * (int)HistoryPeriod.Minute,
                    10 * (int)HistoryPeriod.Minute,
                    15 * (int)HistoryPeriod.Minute,
                    30 * (int)HistoryPeriod.Minute,
                    (int)HistoryPeriod.Hour,
                    2 * (int)HistoryPeriod.Hour,
                    3 * (int)HistoryPeriod.Hour,
                    4 * (int)HistoryPeriod.Hour,
                    6 * (int)HistoryPeriod.Hour,
                    8 * (int)HistoryPeriod.Hour,
                    12 * (int)HistoryPeriod.Hour,
                    (int)HistoryPeriod.Day,
                    (int)HistoryPeriod.Week,
                    (int)HistoryPeriod.Month,
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>    
        private List<BarHistoryItem> GetHistory(HistoryRequestParameters historyParams, DateTime fromTime, DateTime toTime, GranularityEnum timeframe, InstrumentOanda inst, int count)
        {
            List<BarHistoryItem> res = new List<BarHistoryItem>();
            //create request and populate one
            CandlesRequest req = new CandlesRequest
            {
                InstrumentId = inst.Id,
                price = OandaV20Utils.GetOandaHistoryPrice(historyParams.HistoryType),
                granularity = timeframe,
                to = toTime,
                count = count
            };
            
            List<Candle> response = null;
            try
            {
                //send request
                response = Rest.GetCandles(req);
            }
            catch (Exception ex)
            {
                Log(ex);
            }

            if (response == null)
                return res;

            foreach (Candle candle in response)
            {
                try
                {
                    long ticks = candle.Time.Ticks;

                    if (historyParams.HistoryType == HistoryDataTypes.BidAskAverage)
                        res.Add(new BarHistoryItem() { LeftHistoryTime = candle.Time, Open = candle.Mid.Open, Close = candle.Mid.Close, High = candle.Mid.High, Low = candle.Mid.Low, Volume = candle.Volume });
                    else if (historyParams.HistoryType == HistoryDataTypes.Ask)
                        res.Add(new BarHistoryItem() { LeftHistoryTime = candle.Time, Open = candle.Ask.Open, Close = candle.Ask.Close, High = candle.Ask.High, Low = candle.Ask.Low, Volume = candle.Volume });
                    else if (historyParams.HistoryType == HistoryDataTypes.Bid)
                        res.Add(new BarHistoryItem() { LeftHistoryTime = candle.Time, Open = candle.Bid.Open, Close = candle.Bid.Close, High = candle.Bid.High, Low = candle.Bid.Low, Volume = candle.Volume });
                }
                catch (Exception ex)
                { }
            }
           
            return res;
        }

        #endregion History

        #region Trading operations: placing, modifying, cancelling orders
       
        /// <summary>
        /// Placing order
        /// </summary>
        public override TradingOperationResult PlaceOrder(OrderParameters orderData)
        {
            var result = new TradingOperationResult();
            
            var orderRequest = new PlaceOrderRequest();
            OandaV20Utils.CreateOrderRequest(orderData, orderRequest);

            if (orderRequest.Order != null)
            {
                try
                {
                    Rest.PostOrder(orderData.Account.AccountId, orderRequest);
                }
                catch (Exception ex)
                {
                    result.Status = TradingOperationResultStatus.Failure;
                    result.Message = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Modifying order
        /// </summary>
        public override TradingOperationResult ModifyOrder(OrderParameters orderData)
        {
            var result = new TradingOperationResult();

            orderToModify.Add(orderData.OrderId);

            var orderRequest = new PlaceOrderRequest();
            OandaV20Utils.CreateOrderRequest(orderData, orderRequest, orderData.OrderId);

            if (orderRequest.Order != null)
            {
                try
                {
                    if (orderRequest.Order is CloseOrderRequestBase)
                    {
                        var order = Rest.GetSingleOrder(orderData.Account.AccountId, orderData.OrderId);

                        if (order != null && OandaV20Utils.GetOrderType(order.OrderType) != orderData.OrderType)
                        {
                            var x = Rest.CancelOrderAsync(orderData.Account.AccountId, orderData.OrderId);
                            var y = Rest.PostOrder(orderData.Account.AccountId, orderRequest);
                        }
                        else
                        {
                            var x = Rest.ReplaceOrderOrder(orderData.Account.AccountId, orderRequest, orderData.OrderId);
                        }
                    }
                    else
                    {

                        var x = Rest.ReplaceOrderOrder(orderData.Account.AccountId, orderRequest, orderData.OrderId);
                    }
                }
                catch (Exception ex)
                {
                    result.Status = TradingOperationResultStatus.Failure;
                    result.Message = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Cancelling order
        /// </summary>        
        public override TradingOperationResult CancelOrder(OrderParameters orderData)
        {
            var result = new TradingOperationResult();
            try
            { 
                var x = Rest.CancelOrderAsync(orderData.Account.AccountId, orderData.OrderId);
            }
            catch (Exception ex)
            {
                result.Status = TradingOperationResultStatus.Failure;
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Closing position
        /// </summary>
        public override TradingOperationResult ClosePosition(Position positionInfo)
        {
            var result = new TradingOperationResult();

            try
            { 
                var x = Rest.CancelTrade(positionInfo.AccountId, positionInfo.PositionId, new CloseTradeRequest() { Amount = positionInfo.Quantity });
            }
            catch (Exception ex)
            {
                result.Status = TradingOperationResultStatus.Failure;
                result.Message = ex.Message;
            }

            return result;
        }
            
        #endregion Orders/Position manipulation     

        #region Reports

        /// <summary>
        /// Get information about available reports
        /// </summary>       
        public override IList<ReportMetaData> GetReportsMetaData()
        {
            List<ReportMetaData> result = new List<ReportMetaData>();

            ReportMetaData reportType = new ReportMetaData((int)ReportTypeEnum.TransactionHistory, "Transaction history report");
            reportType.Parameters.Add(new SettingItemAccountLookup(Vendor.REPORT_TYPE_PARAMETER_ACCOUNT));
            reportType.Parameters.Add(new SettingItemDateTimePicker(Vendor.REPORT_TYPE_PARAMETER_DATETIME_FROM));
            reportType.Parameters.Add(new SettingItemDateTimePicker(Vendor.REPORT_TYPE_PARAMETER_DATETIME_TO));
            result.Add(reportType);

            return result;
        }

        /// <summary>
        /// Generate report
        /// </summary>
        public override Report GenerateReport(ReportMetaData reportType)
        {
            var accountsIdList = new List<string>();

            string account = reportType.Parameters.Single(p => p.Name == Vendor.REPORT_TYPE_PARAMETER_ACCOUNT).Value.ToString();
            if (string.IsNullOrEmpty(account))
                accountsIdList.AddRange(accountsSummaryOanda.Select(a => a.Id));
            else
                accountsIdList.Add(accountsSummaryOanda.Single(a => a.Name == account).Id);

            DateTime from = (DateTime)reportType.Parameters.Single(p => p.Name == Vendor.REPORT_TYPE_PARAMETER_DATETIME_FROM).Value;
            DateTime to = (DateTime)reportType.Parameters.Single(p => p.Name == Vendor.REPORT_TYPE_PARAMETER_DATETIME_TO).Value;

            switch ((ReportTypeEnum)reportType.Id)
            {
                case ReportTypeEnum.TransactionHistory:
                    return GenerateTransactionHistoryReport(accountsIdList, from, to);

                default:
                    return new Report();
            }
        }

        Report GenerateTransactionHistoryReport(List<string> accountsIdList, DateTime from, DateTime to)
        {
            List<Transaction> transactions = new List<Transaction>();

            foreach (var accountId in accountsIdList)
            {                
                var transactionsPage = Rest.GetTransactionsPage(accountId, from, to);

                foreach (var page in transactionsPage.Pages)
                    transactions.AddRange(Rest.GetTransactions(page));
            }

            var report = new Report();

            report.AddColumn("Time", AdditionalInfoItemComparingType.DateTime);
            report.AddColumn("Type", AdditionalInfoItemComparingType.String);
            report.AddColumn("Order id", AdditionalInfoItemComparingType.String);
            report.AddColumn("Balance", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Financing", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Account id", AdditionalInfoItemComparingType.String);
            report.AddColumn("Instrument", AdditionalInfoItemComparingType.String);
            report.AddColumn("Amount", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Side", AdditionalInfoItemComparingType.String);
            report.AddColumn("P/L", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Price", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Reason", AdditionalInfoItemComparingType.String);
            report.AddColumn("TIF", AdditionalInfoItemComparingType.String);
            report.AddColumn("Stop loss", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Take profit", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Tr.stop loss", AdditionalInfoItemComparingType.Double);
            report.AddColumn("Expire datetime", AdditionalInfoItemComparingType.DateTime);

            foreach(var transaction in transactions)
            {
                InstrumentOanda instr = null;
                if (!string.IsNullOrEmpty(transaction.InstrumentId))
                    instruments.TryGetValue(transaction.InstrumentId, out instr);

                var reportRow = new ReportRow();

                reportRow.AddCell(transaction.Time);
                reportRow.AddCell(transaction.TransactionType.ToString());
                reportRow.AddCell(transaction.OrderID);
                reportRow.AddCell(transaction.AccountBalance);
                reportRow.AddCell(transaction.Financing);
                reportRow.AddCell(transaction.AccountID);
                reportRow.AddCell(instr != null ? instr.Name : string.Empty);
                reportRow.AddCell(transaction.Amount);
                reportRow.AddCell(transaction.Amount != 0 ? (transaction.Amount > 0 ? SideEnum.Buy : SideEnum.Sell).ToString() : string.Empty);
                reportRow.AddCell(transaction.tradesClosed?.Count > 0 ? transaction.tradesClosed.Select(t => t.RealizedPL).Sum() : 0d);
                reportRow.AddCell(transaction.Price);
                reportRow.AddCell(transaction.Reason.ToString());
                reportRow.AddCell(transaction.TimeInForce.ToString());
                reportRow.AddCell(transaction.StopLossOnFill != null ? transaction.StopLossOnFill.Price : 0d);
                reportRow.AddCell(transaction.TakeProfitOnFill != null ? transaction.TakeProfitOnFill.Price : 0d);
                reportRow.AddCell(transaction.TrailingStopLossOnFill != null ? transaction.TrailingStopLossOnFill.Distance : 0d);
                reportRow.AddCell(transaction.GTDTime);

                report.Rows.Add(reportRow);
            }

            return report;
        }

        #endregion

        #region Misc
             
        /// <summary>
        /// 
        /// </summary>        
        public override PingResult Ping()
        {
            return pingResult;
        }

        public override IList<OrderTypeWrapper> GetAllowedOrderTypes()
        {
            List<OrderTypeWrapper> types = new List<OrderTypeWrapper>();

            OrderTypeWrapper orderType = new MarketOrderTypeWrapper();
            orderType.AddSupportedTimeInForce(TimeInForce.FOC, TimeInForce.IOC);
            types.Add(orderType);

            orderType = new LimitOrderTypeWrapper();
            orderType.AddSupportedTimeInForce(TimeInForce.GTD, TimeInForce.Day, TimeInForce.GTC);
            types.Add(orderType);

            orderType = new StopOrderTypeWrapper();
            orderType.AddSupportedTimeInForce(TimeInForce.GTD, TimeInForce.Day, TimeInForce.GTC);
            types.Add(orderType);

            return types;
        }

        public override OrderModificationPermissions GetOrderModificationPermissions()
        {
            var permissions = base.GetOrderModificationPermissions();

            permissions.AllowOrderAmountModify = true;
            permissions.AllowOrderPriceModify = true;
            permissions.AllowOrderTiffModify = true;
            permissions.AllowOrderExpirationDateModify = true;
            permissions.AllowOrderPriceToMarketModify = true;

            return permissions;
        }

        public override Dictionary<int, string> GetExchanges()
        {
            return base.GetExchanges();
        }

        public override void OnConnected()
        {
            //
            SubscribeToStreamingData();

            //
            var TimerDelegate = new TimerCallback(TimerTask);
            backGroundTimer = new System.Threading.Timer(TimerDelegate, null, 500, 500);

            startTransferAsyncCancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);

                OandaV20Utils.GenerateDayBarMessages(instruments, startTransferAsyncCancellationToken.Token, GetHistory, NewQuote);
            }); // Load Open from history
        }
     
        private void TimerTask(object StateObj)
        {
            if (processingNow)
                return;

            processingNow = true;

            try
            {
                foreach (var account in accountsSummaryOanda)
                {
                    stopwatch.Start();

                    AccountState accountState = Rest.GetAccountState(account.Id, account.LastTransactionId.ToString());

                    stopwatch.Stop();
                    pingResult.State = PingEnum.Connected;
                    pingResult.PingTime = new TimeSpan(stopwatch.ElapsedTicks);

                    ConcurrentDictionary<string, Position> trades;
                    if (positions.TryGetValue(account.Id, out trades))
                    {
                        foreach (var item in accountState.trades)
                        {
                            Position position;
                            if (!trades.TryGetValue(item.TradeId, out position))
                                continue;

                            position.ServerCalculationNetPL = position.ServerCalculationGrossPL = item.UnrealizedPL;

                            this.OnPositionUpdated(position);
                        }
                    }

                    var volume = 0D;
                    var realizedPnl = 0D;
                    var tradesCount = 0;
                    var todayRebates = 0D;

                    DateTime begin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(-1).AddHours(22);
                    DateTime end = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddHours(22).AddMilliseconds(-1);

                    var tradesHistory = this.GetTradesHistory(begin, end);
                    foreach (var item in tradesHistory)
                    {
                        volume += Math.Abs(item.Quantity);
                        realizedPnl += item.PnL;
                        todayRebates += item.Rebates;
                        tradesCount++;
                    }

                    AccountSummaryOanda accountSummary = Rest.GetAccountSummary(account.Id);

                    Account accWrp = new Account();

                    accWrp.AccountId = account.Id;
                    accWrp.AccountName = account.Name;
                    accWrp.Currency = account.Currency;

                    accWrp.MarginLevel = accountSummary.MarginRate * 100;
                    accWrp.Balance = accountSummary.Balance;
                    accWrp.UsedMargin = accountState.MarginUsed;
                    accWrp.AvailableMargin = accountState.MarginAvailable;
                    accWrp.MaintranceMargin = accountState.MarginUsed;

                    var closeOutPercent = accountState.MarginCloseoutPercent * 100D;
                    if (closeOutPercent != 0)
                        accWrp.marginWarningLevelPercent = closeOutPercent;
                    else
                        accWrp.marginWarningLevelPercent = 0.001;

                    accWrp.AccountTradeStatus = closeOutPercent < 90 ? AccountTradeStatusEnum.Active : AccountTradeStatusEnum.MarginWarning;

                    OandaV20Utils.FillAccountAditionalInfo(accWrp, accountState);

                    //TodayResults
                    accWrp.RealizedPnl = realizedPnl;
                    accWrp.TodayVolume = volume;
                    accWrp.TodayTradesCount = tradesCount;
                    accWrp.TodayRebates = todayRebates;

                    this.OnAccountUpdated(accWrp);
                }
            }
            catch (Exception ex)
            {
                pingResult.State = PingEnum.Disconnected;
            }
            finally
            {
                processingNow = false;
            }
        }
        
        private Trade GenerateTrade(Transaction transaction)
        {
            Trade trade = new Trade();

            InstrumentOanda instrument = null;
            instruments.TryGetValue(transaction.InstrumentId, out instrument);

            trade.AccountId = transaction.AccountID;
            trade.Time = transaction.Time;
            trade.OrderId = transaction.OrderID;
            trade.Price = transaction.Price;
            trade.Quantity = transaction.Amount;
            trade.Side = transaction.Amount > 0 ? SideEnum.Buy : SideEnum.Sell;
            trade.Symbol = instrument != null ? instrument.Name : "";
            trade.SymbolType = instrument != null ? instrument.InstrumentType.ToString() : "";
            trade.TradeId = transaction.TransactionID;
            trade.PnL = transaction.tradesClosed != null ? transaction.tradesClosed.Select(t => t.RealizedPL).Sum() : transaction.Pl;
            trade.Rebates = transaction.Financing;

            return trade;
        }

        #endregion

        #region Oanda streaming data

        /// <summary>
        /// Subscriptions to rates and events
        /// </summary>
        void SubscribeToStreamingData()
        {
            // Subscribe to RateSession
            rateStreamSession = new RatesSession(accountsSummaryOanda[0].Id, instruments.Values.ToList());
            rateStreamSession.DataReceived += RateSessionOnDataReceived;
            rateStreamSession.StartSession();

            // Subscribe to EventSession
            for (int i = 0; i < accountsSummaryOanda.Count; i++)
            {
                AccountSummaryOanda accountSummary = accountsSummaryOanda[i];

                var eventSession = new EventsSession(accountSummary.Id);
                eventSession.StartSession();
                eventSession.DataReceived += TransactionSessionOnDataReceived;
                eventStreamingSessions.Add(eventSession);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void UnsubscribeStreamingData()
        {
            if (rateStreamSession != null)
            {
                rateStreamSession.StopSession();
                rateStreamSession.DataReceived -= RateSessionOnDataReceived;
                rateStreamSession = null;
            }

            if (eventStreamingSessions != null)
            {
                foreach (var eventSession in eventStreamingSessions)
                {
                    eventSession.StopSession();
                    eventSession.DataReceived -= TransactionSessionOnDataReceived;
                }
                eventStreamingSessions = null;
            }
        }

        private void RateSessionOnDataReceived(Price price)
        {
            InstrumentOanda instrument;
            string symbol = instruments.TryGetValue(price.InstrumentId, out instrument) ? instrument.Name : "";
            lastQuoteCache[symbol] = price;

            ProcessQuote(price, symbol);
        }

        /// <summary>
        /// 
        /// </summary>        
        private void TransactionSessionOnDataReceived(Transaction transaction)
        {
            switch (transaction.TransactionType)
            {
                case TransactionType.MarketOrder:
                    {
                        InstrumentOanda inst;
                        if (!instruments.TryGetValue(transaction.InstrumentId, out inst))
                            break;

                        var order = new Order();

                        order.OrderId = transaction.TransactionID;
                        order.AccountId = transaction.AccountID;
                        order.OrderType = OrderType.Market;

                        order.Symbol = inst.Name.ToUpper();
                        order.Side = OandaV20Utils.GetSide(transaction.Amount);
                        order.Quantity = Math.Abs(transaction.Amount);
                        order.Price = transaction.Price;
                        order.LastUpdateTime = transaction.Time;

                        order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                        order.ExpirationTime = transaction.GTDTime;

                        order.Status = OrderStatus.New;

                        OandaV20Utils.SetSLTP(transaction, inst, order);

                        this.OnOrderUpdated(order);
                    }
                    break;


                case TransactionType.LimitOrder:
                    {
                        InstrumentOanda inst;
                        if (!instruments.TryGetValue(transaction.InstrumentId, out inst))
                            break;

                        var order = new Order();

                        order.OrderId = transaction.TransactionID;
                        order.AccountId = transaction.AccountID;
                        order.OrderType = OrderType.Limit;

                        order.Symbol = inst.Name.ToUpper();
                        order.Side = OandaV20Utils.GetSide(transaction.Amount);
                        order.Quantity = Math.Abs(transaction.Amount);
                        order.Price = transaction.Price;
                        order.LastUpdateTime = transaction.Time;

                        order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                        order.ExpirationTime = transaction.GTDTime;

                        order.Status = transaction.Reason == TransactionReason.ClientOrder ? OrderStatus.New : OrderStatus.Replaced;

                        OandaV20Utils.SetSLTP(transaction, inst, order);

                        this.OnOrderUpdated(order);
                    }
                    break;
                case TransactionType.StopOrder:
                    {
                        InstrumentOanda inst;
                        if (!instruments.TryGetValue(transaction.InstrumentId, out inst))
                            break;

                        var order = new Order();

                        order.OrderId = transaction.TransactionID;
                        order.AccountId = transaction.AccountID;
                        order.OrderType = OrderType.Stop;

                        order.Symbol = inst.Name.ToUpper();
                        order.Side = OandaV20Utils.GetSide(transaction.Amount);
                        order.Quantity = Math.Abs(transaction.Amount);
                        order.Price = transaction.Price;
                        order.LastUpdateTime = transaction.Time;

                        order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                        order.ExpirationTime = transaction.GTDTime;

                        order.Status = transaction.Reason == TransactionReason.ClientOrder ? OrderStatus.New : OrderStatus.Replaced;

                        OandaV20Utils.SetSLTP(transaction, inst, order);

                        this.OnOrderUpdated(order);
                    }
                    break;
                case TransactionType.TakeProfitOrder:
                    {
                        var order = new Order();

                        InstrumentOanda inst;
                        var instrumentKey = transaction.InstrumentId;
                        if (!string.IsNullOrEmpty(transaction.TradeID) && string.IsNullOrEmpty(transaction.InstrumentId))
                        {
                            var tradeOanda = Rest.GetSingleTrade(transaction.AccountID, transaction.TradeID);
                            if (tradeOanda == null)
                                break;

                            instrumentKey = tradeOanda.InstrumentId;
                            order.PositionId = transaction.TradeID;
                            order.Quantity = Math.Abs(tradeOanda.CurrentUnits);
                            order.Side = OandaV20Utils.GetSide(-tradeOanda.CurrentUnits);
                        }

                        if (!instruments.TryGetValue(instrumentKey, out inst))
                            break;

                        order.OrderId = transaction.TransactionID;
                        order.AccountId = transaction.AccountID;
                        order.OrderType = OrderType.Limit;

                        order.Symbol = inst.Name.ToUpper();
                        order.Price = transaction.Price;
                        order.LastUpdateTime = transaction.Time;

                        order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                        order.ExpirationTime = transaction.GTDTime;

                        order.Status = transaction.Reason == TransactionReason.ClientOrder ? OrderStatus.New : OrderStatus.Replaced;
                        order.PositionId = transaction.TradeID;

                        this.OnOrderUpdated(order);
                    }
                    break;
                case TransactionType.StopLossOrder:
                    {
                        var order = new Order();

                        InstrumentOanda inst;
                        var instrumentKey = transaction.InstrumentId;
                        if (!string.IsNullOrEmpty(transaction.TradeID) && string.IsNullOrEmpty(transaction.InstrumentId))
                        {
                            var trade = Rest.GetSingleTrade(transaction.AccountID, transaction.TradeID);
                            if (trade == null)
                                break;

                            instrumentKey = trade.InstrumentId;
                            order.PositionId = transaction.TradeID;
                            order.Quantity = Math.Abs(trade.CurrentUnits);
                            order.Side = OandaV20Utils.GetSide(-trade.CurrentUnits);
                        }

                        if (!instruments.TryGetValue(instrumentKey, out inst))
                            break;

                        order.OrderId = transaction.TransactionID;
                        order.AccountId = transaction.AccountID;
                        order.OrderType = OrderType.Stop;
                        order.Symbol = inst.Name.ToUpper();
                        order.Price = transaction.Price;
                        order.LastUpdateTime = transaction.Time;
                        order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                        order.ExpirationTime = transaction.GTDTime;
                        order.Status = transaction.Reason == TransactionReason.ClientOrder ? OrderStatus.New : OrderStatus.Replaced;
                        order.PositionId = transaction.TradeID;

                        this.OnOrderUpdated(order);
                    }
                    break;
                case TransactionType.TrailingStopLossOrder:
                    {
                        var order = new Order();

                        InstrumentOanda inst;
                        var instrumentKey = transaction.InstrumentId;
                        if (!string.IsNullOrEmpty(transaction.TradeID) && string.IsNullOrEmpty(transaction.InstrumentId))
                        {
                            var tradeOanda = Rest.GetSingleTrade(transaction.AccountID, transaction.TradeID);
                            if (tradeOanda == null)
                                break;

                            instrumentKey = tradeOanda.InstrumentId;
                            order.PositionId = transaction.TradeID;
                            order.Quantity = Math.Abs(tradeOanda.CurrentUnits);
                            order.Side = OandaV20Utils.GetSide(-tradeOanda.CurrentUnits);
                        }

                        if (!instruments.TryGetValue(instrumentKey, out inst))
                            break;

                        order.OrderId = transaction.TransactionID;
                        order.AccountId = transaction.AccountID;
                        order.OrderType = OrderType.TrailingStop;
                        order.Symbol = inst.Name.ToUpper();
                        order.Price = transaction.Distance;
                        order.Tiff = OandaV20Utils.GetTiff(transaction.TimeInForce);
                        order.ExpirationTime = transaction.GTDTime;
                        order.Status = transaction.Reason == TransactionReason.ClientOrder ? OrderStatus.New : OrderStatus.Replaced;
                        order.PositionId = transaction.TradeID;
                        order.LastUpdateTime = transaction.Time;

                        this.OnOrderUpdated(order);
                    }
                    break;

                case TransactionType.OrderFill:
                    {
                        InstrumentOanda inst;
                        if (!instruments.TryGetValue(transaction.InstrumentId, out inst))
                            break;

                        var orderOanda = Rest.GetSingleOrder(transaction.AccountID, transaction.OrderID);

                        if (orderOanda != null)
                        {
                            var order = new Order();

                            order.OrderId = orderOanda.Id;
                            order.AccountId = transaction.AccountID;
                            order.OrderType = OandaV20Utils.GetOrderType(orderOanda.OrderType);
                            order.Symbol = inst.Name.ToUpper();
                            order.Side = OandaV20Utils.GetSide(orderOanda.Amount);
                            order.Quantity = orderOanda.Amount != 0 ? Math.Abs(orderOanda.Amount) : Math.Abs(transaction.Amount);
                            order.Price = orderOanda.Price != 0 ? orderOanda.Price : transaction.Price;
                            order.Tiff = OandaV20Utils.GetTiff(orderOanda.TimeInForce);
                            order.ExpirationTime = orderOanda.GTDTime;
                            order.LastUpdateTime = orderOanda.CreationTime;
                            order.Status = OrderStatus.Filled;

                            this.OnOrderCanceled(order);
                        }
                        else
                            this.OnOrderCanceled(new Order() { OrderId = transaction.OrderID, Status = OrderStatus.Filled });

                        if (transaction.TradeOpened != null)
                        {
                            var position = new Position();

                            position.AccountId = transaction.AccountID;
                            position.PositionId = transaction.TradeOpened.TradeId;
                            position.Symbol = inst.Name.ToUpper();
                            position.OpenPrice = transaction.Price;
                            position.Quantity = Math.Abs(transaction.TradeOpened.Amount);
                            position.Side = OandaV20Utils.GetSide(transaction.TradeOpened.Amount);
                            position.OpenTime = transaction.Time;

                            ConcurrentDictionary<string, Position> trades;
                            if (positions.TryGetValue(transaction.AccountID, out trades))
                            {
                                trades[position.PositionId] = position;
                            }

                            this.OnPositionUpdated(position);
                        }

                        if (transaction.tradeReduced != null)
                        {
                            ConcurrentDictionary<string, Position> trades;
                            if (positions.TryGetValue(transaction.AccountID, out trades))
                            {
                                Position position;
                                if (trades.TryGetValue(transaction.tradeReduced.TradeId, out position))
                                {
                                    position.Quantity -= Math.Abs(transaction.tradeReduced.Amount);
                                    this.OnPositionClosed(position);
                                }
                            }
                        }

                        if (transaction.tradesClosed != null)
                        {
                            foreach (var trade in transaction.tradesClosed)
                            {
                                var position = new Position()
                                {
                                    PositionId = trade.TradeId,
                                    CloseOrderId = transaction.OrderID
                                };

                                ConcurrentDictionary<string, Position> trades;
                                if (positions.TryGetValue(transaction.AccountID, out trades))
                                {
                                    Position x;
                                    trades.TryRemove(trade.TradeId, out x);
                                }

                                this.OnPositionClosed(position);
                            }
                        }

                        Trade tradeHistory = GenerateTrade(transaction);
                        this.NewTrade(tradeHistory);
                    }
                    break;

                case TransactionType.OrderCancel:
                    {
                        if (orderToModify.Contains(transaction.OrderID))
                        {
                            orderToModify.Remove(transaction.OrderID);
                            this.OnOrderCanceled(new Order() { OrderId = transaction.OrderID });
                            break;
                        }

                        var orderOanda = Rest.GetSingleOrder(transaction.AccountID, transaction.OrderID);

                        if (orderOanda != null && orderOanda.InstrumentId != null)
                        {
                            InstrumentOanda instrumentOanda;
                            if (!instruments.TryGetValue(orderOanda.InstrumentId, out instrumentOanda))
                                break;

                            var order = new Order();

                            order.OrderId = orderOanda.Id;
                            order.AccountId = transaction.AccountID;
                            order.OrderType = OandaV20Utils.GetOrderType(orderOanda.OrderType);
                            order.Symbol = instrumentOanda.Name.ToUpper();
                            order.Side = OandaV20Utils.GetSide(orderOanda.Amount);
                            order.Quantity = Math.Abs(orderOanda.Amount);
                            order.Price = orderOanda.Price;
                            order.Tiff = OandaV20Utils.GetTiff(orderOanda.TimeInForce);
                            order.ExpirationTime = orderOanda.GTDTime;
                            order.LastUpdateTime = orderOanda.CreationTime;
                            order.Status = OrderStatus.Canceled;

                            this.OnOrderCanceled(order);
                        }
                        else
                            this.OnOrderCanceled(new Order() { OrderId = transaction.OrderID });
                    }
                    break;

                default:
                    break;
            }
        }

        private void ProcessQuote(Price price, string symbol)
        {
            if (subscribedLevel1Instruments.Contains(symbol))
            {
                if (price.Asks.Count > 0 && price.Bids.Count > 0)
                {
                    Quote1 q1 = new Quote1()
                    {
                        Symbol = symbol,
                        Ask = price.Asks[0].Price,
                        Bid = price.Bids[0].Price,

                        AskSize = price.Asks[0].Liquidity,
                        BidSize = price.Bids[0].Liquidity,

                        Time = price.Time
                    };

                    this.NewQuote(q1);
                }
            }

            if (subscribedLevel2Instruments.Contains(symbol))
            {
                List<Quote2> listQ2 = new List<Quote2>();

                foreach (PriceBucket ask in price.Asks)
                {
                    listQ2.Add(new Quote2()
                    {
                        Symbol = symbol,
                        Time = price.Time,

                        Price = ask.Price,
                        Side = SideEnum.Sell,
                        Size = ask.Liquidity
                    });
                }

                foreach (PriceBucket bid in price.Bids)
                {
                    listQ2.Add(new Quote2()
                    {
                        Symbol = symbol,
                        Time = price.Time,

                        Price = bid.Price,
                        Side = SideEnum.Buy,
                        Size = bid.Liquidity
                    });
                }

                if (listQ2.Count > 0)
                {
                    Quote2 q2 = listQ2.First();
                    q2.UpdateType = Quote2UpdateType.FullRefresh;

                    q2.Thail = listQ2.ToArray();

                    NewQuote(q2);
                }
            }


            if (price.Asks.Count > 0 && price.Bids.Count > 0)
            {
                string exp1, exp2;
                OandaV20Utils.ExtractExp1Exp2(symbol, out exp1, out exp2, '/');

                this.SendCrossPrice(new CrossPrice()
                {
                    Symbol = symbol,
                    Exp1 = exp1,
                    Exp2 = exp2,
                    Price = (price.Bids[0].Price + price.Asks[0].Price) / 2D
                });
            }
        }

        #endregion
    }
}
