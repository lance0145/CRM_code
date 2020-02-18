using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Classes.BrandsMaster.CRM.Builders;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using static Classes.BrandsMaster.CRM.Builders.Objects;
using static Classes.BrandsMaster.CRM.Builders.Responses;

namespace Classes.BrandsMaster.CRM
{
    public class Update : Create
    {
        private static List<Accounts> accounts = Accounts.GetAccounts();

        private static JavaScriptSerializer serializer = new JavaScriptSerializer();
        private static string CRMlink = WebConfigurationManager.AppSettings["CRMlink"];


        public static string UpdateTransaction(Transaction Transaction)
        {
            //Reqs
            //Transaction.id
            //Transaction.status
            //Transaction.UpdatedBy (Diff System)
            //Transaction.Type
            //Transaction.ta_login (Nullable)
            //Transaction.ta_id (Nullable)
            //Transaction.client_id (Nullable)
            //Transaction.amount (Nullable) (Default: Unchange)
            //Transaction.sync_with_mt (Nullable) (Default: Unchange)
            //Transaction.note (Nullable)  (Default: Unchange)
            //Transaction.card_number (Nullable) (Default: Unchange)
            //Transaction.card_expiry (Nullable) (Default: Unchange)
            //Transaction.isfake (Nullable) (Default: Unchange)
            //Transaction.currency
            string path = @"C:\Websites\Logs\Classes\UpdateTransaction - " + DateTime.Now.ToString("yyyy.MM.dd") + ".txt";
            string Query = "";
            File.AppendAllText(path, "============================================================== \r\n");
            File.AppendAllText(path, "---- START ---- \r\n");
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
            {
                File.AppendAllText(path, "Connected To DB \r\n");

                TransactionStatuses? PreviosTransactionStatus;
                #region Get Transaction History, TA_ID, Client_id And TA Currecny

                File.AppendAllText(path, "Get Transaction History Start \r\n");
                List<Builders.Objects.Transaction> History = new List<Builders.Objects.Transaction>();

                Query = String.Format("SELECT history FROM sales.transactions WHERE id={0} ", Transaction.id);
                DataTable TransactionTable = DBqueries.Select(conn, Query);
                File.AppendAllText(path, "Get Transaction History End \r\n");

                File.AppendAllText(path, "Deserializing History Start \r\n");
                History = serializer.Deserialize<List<Builders.Objects.Transaction>>(TransactionTable.Rows[0].ItemArray[0].ToString());
                File.AppendAllText(path, "Deserializing History End \r\n");

                #region Get TA_ID
                File.AppendAllText(path, "Get the Trading Account Id Start \r\n");
                if (Transaction.ta_id == null)
                {
                    Query = String.Format("SELECT ta_id FROM sales.transactions WHERE id={0} ", Transaction.id);
                    Transaction.ta_id = int.Parse(DBqueries.SelectScalar(conn, Query).ToString());

                    File.AppendAllText(path, $"TradingAccountId:({Transaction.ta_id}) \r\n");
                }
                File.AppendAllText(path, "Get the Trading Account Id End \r\n");
                #endregion

                #region Get ClientId
                File.AppendAllText(path, "Get the Client Id Start \r\n");
                if (Transaction.client_id == null)
                {
                    Query = String.Format("SELECT client_id FROM sales.transactions WHERE id={0} ", Transaction.id);
                    Transaction.client_id = int.Parse(DBqueries.SelectScalar(conn, Query).ToString());

                    File.AppendAllText(path, $"Client ID:({Transaction.client_id}) \r\n");
                }
                File.AppendAllText(path, "Get the Client Id End \r\n");
                #endregion

                File.AppendAllText(path, "Transaction Convert To Start \r\n");
                Transaction.Convert_To = Utilities.GetTACurrecny(conn, Transaction.ta_id);
                File.AppendAllText(path, "Transaction Convert To End \r\n");


                #endregion

                #region Convert From
                File.AppendAllText(path, "Transaction Convert From Start \r\n");
                if (Transaction.currency == null)
                    Transaction.currency = Transaction.Convert_To;

                if (Transaction.Convert_To != null && Transaction.Convert_To != Transaction.currency)
                {
                    Transaction.amount = Utilities.ConvertAmountFrom(Transaction).amount;
                    Transaction.currency = Transaction.Convert_To;
                }
                File.AppendAllText(path, "Transaction Convert From End \r\n");
                #endregion

                #region Convertions To USD
                File.AppendAllText(path, "Convertion to USD Start \r\n");
                if (Transaction.currency != Classes.MT.MT5.Builders.Objects.MTCurrencies.USD)
                {
                    var Conve = Utilities.ConvertAmountToUSD(Transaction);
                    Transaction.convertion_rate_to_usd = Conve.convertion_rate;
                    Transaction.usd_amount = Conve.amount;
                }
                File.AppendAllText(path, "Convertion to USD End \r\n");
                #endregion

                #region fetch Previos Status
                File.AppendAllText(path, "Fetch Previous Status Start \r\n");
                History = History.OrderByDescending(s => s.Updated_date).ToList();
                PreviosTransactionStatus = History[0].status;
                File.AppendAllText(path, "Fetch Previous Status End \r\n");
                #endregion

                File.AppendAllText(path, "Check if Prev Transaction is Approved Start \r\n");
                #region If Transaction Approved, Check Previos Transaction Approve aswell?
                if (Transaction.status == TransactionStatuses.Approved && PreviosTransactionStatus == TransactionStatuses.Approved)
                {


                    History.Add(Transaction);

                    #region UPDATE Transaction On CRM

                    #region Create Update Query

                    Query = String.Format("UPDATE sales.transactions SET " +
                        " card_number=COALESCE({1},card_number), " +
                        " card_expiry=COALESCE({2}, " +
                        " card_expiry),note=COALESCE({3},note), " +
                        " history='{4}', " +
                        " psp_transaction_id='{5}', " +
                        " praxis_id={6}, " +
                        " psp_id=COALESCE({7},psp_id), " +
                        " fundprocessor_id=COALESCE({8},fundprocessor_id), " +
                        " card_holder=COALESCE('{9}',card_holder), isfake=COALESCE({10},isfake), is_ftd=COALESCE({11},is_ftd), is_self_deposit=COALESCE({12},is_self_deposit), pool=COALESCE({13},pool) " +
                        //" pool=COALESCE('{12}',pool), current_pool=COALESCE({13},current_pool), is_self_deposit=COALESCE({14},is_self_deposit) " +
                        " WHERE id={0}",
                        Transaction.id,
                        Transaction.card_number == null ? "null" : "'" + Transaction.card_number.ToString() + "'",
                        Transaction.card_expiry == null ? "null" : "'" + Transaction.card_expiry.ToString() + "'",
                        Transaction.note == null ? "null" : "'" + Transaction.note.ToString() + "'",
                        serializer.Serialize(History),
                        Transaction.psp_transaction_id,
                        Transaction.Praxis_Trace_Id == null ? "null" : "'" + Transaction.Praxis_Trace_Id.ToString() + "'",
                        Transaction.psp_id == null ? "null" : Transaction.psp_id.ToString(),
                        Transaction.fundprocessor_id == null ? "null" : Transaction.fundprocessor_id.ToString(),
                        Transaction.card_holder == null ? "null" : Transaction.card_holder,
                        Transaction.isfake == null ? "null" : "'" + Transaction.isfake.ToString() + "'",
                        Transaction.is_ftd == null ? "null" : "'" + Transaction.is_ftd.ToString() + "'",
                        Transaction.is_self_deposit == null ? false : Transaction.is_self_deposit,
                        Transaction.pool == null ? "null" : "'" + Transaction.pool.ToString() + "'"

                    );
                    #endregion

                    string res1 = DBqueries.Update(conn, Query, -1);
                    if (res1 != "Success")
                        return res1;
                    #endregion

                    return "Success";
                    #endregion
                }

                else if (Transaction.status != TransactionStatuses.Approved && PreviosTransactionStatus == TransactionStatuses.Approved)
                {

                }
                File.AppendAllText(path, "Check if Prev Transaction is Approved End \r\n");


                File.AppendAllText(path, "Build Transaction Updated By Start \r\n");
                Transaction.UpdatedBy = String.IsNullOrEmpty(Transaction.UpdatedBy) ? "System" : Transaction.UpdatedBy;
                File.AppendAllText(path, "Build Transaction Updated By End \r\n");

                #region Get AgentID
                File.AppendAllText(path, "Get Agent Id Start \r\n");
                if (Transaction.client_id == null)
                {
                    Query = String.Format("SELECT retention_agent FROM sales.clients WHERE id={0}", Transaction.client_id);
                    var Object = DBqueries.SelectScalar(conn, Query);
                    if (Object != null)
                        Transaction.agent_id = int.Parse(Object.ToString());
                    else
                        Transaction.agent_id = -1;
                }
                File.AppendAllText(path, "Get Agent Id End \r\n");
                #endregion

                #region Update If FTD + Entery_Type +  Deposit Amount / Withdrawal Amount 
                File.AppendAllText(path, "Update If FTD + Entery_Type +  Deposit Amount / Withdrawal Amount Start \r\n");
                if (Transaction.client_id != null && Transaction.status == Objects.TransactionStatuses.Approved)
                {
                    File.AppendAllText(path, "Get Entry Type Start \r\n");

                    Query = String.Format("SELECT entry_type FROM sales.clients WHERE id={0}", Transaction.client_id);
                    string entry_type = DBqueries.SelectScalar(conn, Query).ToString();

                    if (entry_type == "Lead")
                    {
                        Transaction.is_ftd = true;
                    }
                    else
                    {
                        Transaction.is_ftd = false;
                    }

                    File.AppendAllText(path, "Get Entry Type End \r\n");

                    #region Update Entery_Typee / is_ftd AND deposit / withdrawal amount

                    File.AppendAllText(path, "Update Entery_Typee / is_ftd AND deposit / withdrawal amount Start \r\n");

                    if (Transaction.Type == Objects.TransactionType.Deposit)
                    {
                        string UpdateQuery = "";

                        #region Check if need to update to entry_type client
                        bool? AutoRetentionConvert = false;

                        string Client_pool = Utilities.GetClientPool(Transaction.client_id);

                        AutoRetentionConvert = Utilities.GetPools().Find(s => s.name == Client_pool).AutoConvertToRetention;

                        if ((bool)AutoRetentionConvert)
                            UpdateQuery += $",entry_type='{Client.ClientEnteryType.Client}' ";
                        #endregion

                        if ((bool)Transaction.is_ftd && Transaction.status == Objects.TransactionStatuses.Approved)
                        {
                            UpdateQuery += $",ftd_date='{Transaction.created_date}' ";
                        }

                        Query = $"UPDATE sales.clients SET is_ftd='{Transaction.is_ftd}' {UpdateQuery}, deposit_amount = deposit_amount + {Transaction.amount} WHERE id={Transaction.client_id};UPDATE sales.trading_accounts SET deposit_amount=(deposit_amount+ {Transaction.amount}) WHERE login={Transaction.ta_login} ";
                    }
                    else if (Transaction.Type == Objects.TransactionType.Withdrawal)
                        Query = $"UPDATE sales.clients SET withdrawal_amount = withdrawal_amount + {Transaction.amount} WHERE id={Transaction.client_id};UPDATE sales.trading_accounts SET withdrawal_amount=(withdrawal_amount+ {Transaction.amount}) WHERE login={Transaction.ta_login} ";

                    DBqueries.Update(conn, Query, -1);

                    File.AppendAllText(path, "Update Entery_Typee / is_ftd AND deposit / withdrawal amount End \r\n");
                    #endregion
                }
                File.AppendAllText(path, "Update If FTD + Entery_Type +  Deposit Amount / Withdrawal Amount End \r\n");
                #endregion

                #region If there is no TA And Transaction Approved -> Create TA In MT
                File.AppendAllText(path, "If there is no TA And Transaction Approved -> Create TA In MT Start \r\n");
                if (Transaction.Type == Objects.TransactionType.Deposit)
                {
                    if (Transaction.ta_login == null || Transaction.ta_login == -1 && Transaction.status == Objects.TransactionStatuses.Approved)
                    {
                        #region Get TA Demmi Account Details

                        Query = String.Format("SELECT TA.mt_groups,TA.investor_pass,TA.master_pass ,(SELECT leverage FROM sales.mt_groups WHERE name=TA.mt_groups) AS Leverage ,(SELECT initialbalance FROM sales.mt_groups WHERE name=TA.mt_groups) AS initialbalance FROM sales.trading_accounts as TA WHERE id={0}", Transaction.ta_id);
                        DataTable GroupInfo = DBqueries.Select(conn, Query);

                        Query = String.Format("SELECT CONCAT(fname,' ',lname) FROM sales.clients WHERE id={0} ", Transaction.client_id);
                        DataTable ClientInfo = DBqueries.Select(conn, Query);

                        #endregion

                        #region Create the TA

                        Builders.Objects.MT_Account TA = new Objects.MT_Account()
                        {
                            id = Transaction.ta_id,
                            investor_pass = GroupInfo.Rows[0].ItemArray[1].ToString(),
                            master_pass = GroupInfo.Rows[0].ItemArray[2].ToString(),
                            Account_Type = Builders.Objects.MTTypes.Live,
                            client_id = Transaction.client_id,
                            created_date = DateTime.UtcNow,
                            deposit_amount = Transaction.amount,
                            ftd_date = DateTime.UtcNow,
                            withdrawal_amount = 0,
                            Group = new Objects.MT_Group()
                            {
                                Type = Builders.Objects.MTTypes.Live,
                                name = GroupInfo.Rows[0].ItemArray[0].ToString(),
                                Leverage = int.Parse(GroupInfo.Rows[0].ItemArray[3].ToString()),
                                InitialBalance = int.Parse(GroupInfo.Rows[0].ItemArray[4].ToString())
                            }
                        };

                        Classes.BrandsMaster.CRM.Builders.Client.ClientObject cObject = new Classes.BrandsMaster.CRM.Builders.Client.ClientObject
                        {
                            Id = Transaction.client_id
                        };

                        Transaction.ta_login = CreateMT(TA, ClientInfo.Rows[0].ItemArray[0].ToString(), conn, cObject);

                        #endregion
                    }
                }
                File.AppendAllText(path, "If there is no TA And Transaction Approved -> Create TA In MT End \r\n");
                #endregion

                History.Add(Transaction);

                #region UPDATE Transaction On CRM
                File.AppendAllText(path, "UPDATE Transaction On CRM Start \r\n");

                #region Create Update Query

                //Added PSP AND FUND PROCESSOR ON UPDATE
                Query = String.Format("UPDATE sales.transactions SET ta_login=COALESCE({1}, ta_login), amount=COALESCE({2}, amount),status='{3}',currency=COALESCE({4}, currency) ,convertion_rate_to_usd=COALESCE({5},convertion_rate_to_usd),usd_amount=COALESCE({6},usd_amount),card_number=COALESCE({7},card_number),card_expiry=COALESCE({8},card_expiry),note=COALESCE({9},note),history='{10}',isfake=COALESCE({11},isfake),sync_with_mt=COALESCE({12},sync_with_mt),psp_transaction_id=COALESCE({13},psp_transaction_id),praxis_id=COALESCE({14},praxis_id),psp_id=COALESCE({15},psp_id),fundprocessor_id=COALESCE({16},fundprocessor_id), card_holder=COALESCE('{17}',card_holder), approved_date=COALESCE({18},approved_date), is_ftd=COALESCE({19},is_ftd), pool=COALESCE({20},pool), is_self_deposit=COALESCE({21},is_self_deposit) WHERE id={0}",
                    Transaction.id,
                    Transaction.ta_login == null ? "null" : Transaction.ta_login.ToString(),
                    Transaction.amount == null ? "null" : Transaction.amount.ToString(),
                    Transaction.status,
                    Transaction.currency == null ? "null" : "'" + Transaction.currency.ToString() + "'",
                    Transaction.convertion_rate_to_usd == null ? "null" : Transaction.convertion_rate_to_usd.ToString(),
                    Transaction.usd_amount == null ? "null" : Transaction.usd_amount.ToString(),
                    Transaction.card_number == null ? "null" : "'" + Transaction.card_number.ToString() + "'",
                    Transaction.card_expiry == null ? "null" : "'" + Transaction.card_expiry.ToString() + "'",
                    Transaction.note == null ? "null" : "'" + Transaction.note.ToString() + "'",
                    serializer.Serialize(History),
                    Transaction.isfake == null ? "null" : "'" + Transaction.isfake.ToString() + "'",
                    Transaction.sync_with_mt == null ? "null" : "'" + Transaction.sync_with_mt.ToString() + "'",
                    String.IsNullOrEmpty(Transaction.psp_transaction_id) ? "null" : "'" + Transaction.psp_transaction_id.ToString() + "'",
                    Transaction.Praxis_Trace_Id == null ? "null" : "'" + Transaction.Praxis_Trace_Id.ToString() + "'",
                    Transaction.psp_id == null ? "null" : Transaction.psp_id.ToString(),
                    Transaction.fundprocessor_id == null ? "null" : Transaction.fundprocessor_id.ToString(),
                    Transaction.card_holder == null ? "null" : Transaction.card_holder,
                    Transaction.status == Objects.TransactionStatuses.Approved ? "'" + DateTime.UtcNow.ToString() + "'" : "null",
                    Transaction.is_ftd == null ? "null" : "'" + Transaction.is_ftd.ToString() + "'",
                    Transaction.pool == null ? "null" : "'" + Transaction.pool.ToString() + "'",
                    Transaction.is_self_deposit == null ? false : Transaction.is_self_deposit


                );
                #endregion

                string res = DBqueries.Update(conn, Query, -1);
                if (res != "Success")
                    return res;

                File.AppendAllText(path, "UPDATE Transaction On CRM End \r\n");
                #endregion
            };

            #region If Need to sync_with_mt And Status is Approved -> Create Deposit on TA
            File.AppendAllText(path, "If Need to sync_with_mt And Status is Approved -> Create Deposit on TA Start \r\n");
            if (Transaction.sync_with_mt == true && Transaction.status == Objects.TransactionStatuses.Approved)
            {
                string Api_username = accounts.Find(s => s.ServiceName == "NewMT5API").Username;
                string Api_password = accounts.Find(s => s.ServiceName == "NewMT5API").Password;
                string Api_Address = accounts.Find(s => s.ServiceName == "NewMT5API").Ip;

                if (Transaction.Type == Objects.TransactionType.Deposit)
                {
                    MT.MT5.Builders.Objects.Deposit Dep = new MT.MT5.Builders.Objects.Deposit()
                    {
                        login = Transaction.ta_login,
                        value = Transaction.amount,
                        comment = "DEP (" + Transaction.id + ")"
                    };

                    File.AppendAllText(path, "MT5 Api Create Deposit Start \r\n");
                    MT.MT5.Create.Deposit(Dep, Api_Address, Api_username, Api_password);
                    File.AppendAllText(path, "MT5 Api Create Deposit End \r\n");
                }
                else
                {
                    MT.MT5.Builders.Objects.Withdrawal With = new MT.MT5.Builders.Objects.Withdrawal()
                    {
                        login = Transaction.ta_login,
                        value = Transaction.amount,
                        comment = "WD (" + Transaction.id + ")"

                    };

                    File.AppendAllText(path, "MT5 Api Create Withdrawal Start \r\n");
                    var res = MT.MT5.Create.Withdrawal(With, Api_Address, Api_username, Api_password);
                    if (res.message.Contains("NO_MONEY"))
                    {
                        var trade = MT.MT5.Create.Trade(new MT.MT5.Builders.Objects.Trade
                        {
                            login = (int)Transaction.ta_login,
                            symbol = "WD",
                            volume = (double)Transaction.amount,
                            priceOrder = 1.1,
                            priceSL = 0,
                            priceTP = 0,
                            type = "OP_BUY",
                            comment = "WD (" + Transaction.id + ")",
                        }, Api_Address, Api_username, Api_password);
                        var Position = MT.MT5.TraderInfo.GetPosition(trade.Request.order.ToString(), Transaction.ta_login.ToString(), Api_Address, Api_username, Api_password);

                        if (Position.action == "POSITION_BUY")
                            Position.type = "OP_BUY";
                        else if (Position.action == "POSITION_SELL")
                            Position.type = "OP_SELL";

                        var closeRes = MT.MT5.Update.ClosePosition(Position, Api_Address, Api_username, Api_password);
                    }
                    File.AppendAllText(path, "MT5 Api Create Withdrawal End \r\n");
                }
            }
            File.AppendAllText(path, "If Need to sync_with_mt And Status is Approved -> Create Deposit on TA End \r\n");
            #endregion

            #region Update Transaction On CMS
            //File.AppendAllText(path, "Update Transaction On CMS Start \r\n");
            //if (Transaction.isfake == false && Transaction.sync_client_area)
            //{
            //    if (Transaction.status == TransactionStatuses.Split)
            //        Transaction.status = TransactionStatuses.Approved;

            //    string CMSres = "";
            //    if (Transaction.Type == Objects.TransactionType.Deposit)
            //    {
            //        string Params = String.Format("AuthCode=1rg651e56h4ert65hj41ertj4&action=UPDATE&deposit_id={0}&status={1}&Amount={2}&Login={3}&Reason={4}",
            //            Transaction.id,
            //            Transaction.status,
            //            Transaction.amount,
            //            Transaction.ta_login,
            //            Transaction.note
            //            );
            //        #region Get Api Url 
            //        int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(Transaction.client_id);
            //        string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
            //        #endregion
            //        CMSres = PostBack.PostWithReturn(Api_Url + "Clients/Deposits.aspx", Params);
            //    }
            //    else
            //    {
            //        string Params = String.Format("AuthCode=1rg651e56h4ert65hj41ertj4&action=UPDATE&withdrawal_id={0}&status={1}&Login={2}",
            //           Transaction.id,
            //           Transaction.status,
            //           Transaction.ta_login

            //           );
            //        #region Get Api Url 
            //        int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(Transaction.client_id);
            //        string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
            //        #endregion
            //        CMSres = PostBack.PostWithReturn(Api_Url + "Clients/Withdrawals.aspx", Params);
            //    }
            //}
            //File.AppendAllText(path, "Update Transaction On CMS End \r\n");
            #endregion

            string parameter = $"id={Transaction.id}";

            #region FOR TESTING URL
            //string BaseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath.TrimEnd('/');
            //PostBack.PostWithReturn($"{BaseUrl}/APIs/DepositNotification.aspx", parameter); 
            #endregion

            #region For Live
            File.AppendAllText(path, "Send Deposit Notification Start \r\n");
            PostBack.RestSharp(new PostBack.RestSharpReq
            {
                endPoint = $"{CRMlink}APIs/DepositNotification.aspx",
                method = RestSharp.Method.POST,
                ContentType = "application/x-www-form-urlencoded",
                parameters = parameter,
            });
            File.AppendAllText(path, "Send Deposit Notification End \r\n");
            #endregion

            File.AppendAllText(path, "---- END ---- \r\n");
            File.AppendAllText(path, "============================================================== \r\n");
            File.AppendAllText(path, "\r\n");

            #region Update campaign from dialers

            int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(Transaction.client_id);
            string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;

            PostBack.GetRSharpAsync($"{Api_Url}Cronjobs/UpdateCampaignFromDialers.aspx");

            #endregion

            return "1";
        }

        public static string UpdateTAPass(MT_Account TA, MT.MT5.Builders.Objects.PassTypes Type)
        {
            var MT = new MT.MT5.Builders.Objects.UserRequest()
            {
                investorPassword = TA.investor_pass,
                masterPassword = TA.master_pass,
                user = new MT.MT5.Builders.Objects.MTUser()
                {
                    login = TA.Login
                }
            };
            string Api_username = accounts.Find(s => s.ServiceName == "NewMT5API").Username;
            string Api_password = accounts.Find(s => s.ServiceName == "NewMT5API").Password;
            string Api_Address = accounts.Find(s => s.ServiceName == "NewMT5API").Ip;

            string res = Classes.MT.MT5.Update.UpdatePass(MT, Type, Api_Address, Api_username, Api_password);



            if (res == "MT_RET_OK")
            {
                using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
                {
                    string Query = String.Format("UPDATE sales.trading_accounts SET master_pass='{0}' WHERE login={1}", TA.master_pass, TA.Login);
                    DBqueries.Update(conn, Query, -1);
                }

                return "Success";
            }
            else
                return "Error";
        }

        public static string UpdateTAComment(string TAid, string comment)
        {
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
            {
                string Query = String.Format("UPDATE sales.trading_accounts SET comment='{0}' WHERE id={1}", comment, TAid);
                DBqueries.Update(conn, Query, -1);
            }

            return "Success";

        }

        public static string UpdateTAColor(string TAid, string color)
        {
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
            {
                string Query = String.Format("UPDATE sales.trading_accounts SET color='{0}' WHERE id={1}", color, TAid);
                DBqueries.Update(conn, Query, -1);
            }

            return "Success";

        }

        public static string UpdateWebPass(Client.ClientObject Client)
        {

            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
            {
                string Query = String.Format("UPDATE sales.clients SET website_pass='{0}' WHERE id={1}", Client.web_password, Client.Id);
                return DBqueries.Update(conn, Query, -1);
            }

        }

        public static string UpdateInternalTransfer(Internal_Transfer Trans)
        {
            //REQs
            //Trans.id
            //Trans.Amount (Optional) (Dont send if amount wasnt Change)
            //Trans.status (Optional) Default Unchange

            string Query = "";
            List<Internal_Transfer> History = new List<Internal_Transfer>();
            #region Get Histor And TA currencies + Conversions + from_ta + to_ta

            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
            {


                Query = String.Format("SELECT history,from_ta_currency,to_ta_currency,converted_amount,from_ta,to_ta,created_date FROM sales.internaltransfers WHERE id={0} ", Trans.id);
                DataTable Internal_Transfer = DBqueries.Select(conn, Query);

                History = serializer.Deserialize<List<Internal_Transfer>>(Internal_Transfer.Rows[0].ItemArray[0].ToString());

                Trans.Converted_Amount = double.Parse(Internal_Transfer.Rows[0].ItemArray[3].ToString());
                Trans.From_ta_login = int.Parse(Internal_Transfer.Rows[0].ItemArray[4].ToString());
                Trans.To_ta_login = int.Parse(Internal_Transfer.Rows[0].ItemArray[5].ToString());
                DateTime CreatedDate = Convert.ToDateTime(Internal_Transfer.Rows[0].ItemArray[6]);
                Trans.Created_Date = CreatedDate;

                #region Account Currencies are diff
                if (Internal_Transfer.Rows[0].ItemArray[1].ToString() != Internal_Transfer.Rows[0].ItemArray[2].ToString())
                {
                    Trans.From_ta_login_currency = Internal_Transfer.Rows[0].ItemArray[1].ToString();
                    Trans.To_ta_login_currency = Internal_Transfer.Rows[0].ItemArray[2].ToString();

                    #region Make Convertion
                    Objects.Transaction t = new Objects.Transaction()
                    {
                        amount = Trans.Amount,
                        currency = (MT.MT5.Builders.Objects.MTCurrencies)Enum.Parse(typeof(MT.MT5.Builders.Objects.MTCurrencies), Trans.From_ta_login_currency),
                        Convert_To = (MT.MT5.Builders.Objects.MTCurrencies)Enum.Parse(typeof(MT.MT5.Builders.Objects.MTCurrencies), Trans.To_ta_login_currency)
                    };
                    var res = Utilities.ConvertAmountFrom(t);
                    #endregion

                    Trans.Converstion_rate = res.convertion_rate;
                    Trans.Converted_Amount = res.amount;
                }
                #endregion

                History.Add(Trans);

            };
            #endregion

            #region Update CRM
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreateClient", ""))
            {
                Query = String.Format("UPDATE sales.internaltransfers SET amount=COALESCE({1}, amount),status=COALESCE({2}, status),history='{3}',conversion_rate=COALESCE({4}, conversion_rate),converted_amount=COALESCE({5}, converted_amount) WHERE id={0}",
                    Trans.id,
                    Trans.Amount == null ? "null" : Trans.Amount.ToString(),
                    Trans.status == null ? "null" : "'" + Trans.status.ToString() + "'",
                    serializer.Serialize(History),
                    Trans.Converstion_rate == null ? "null" : Trans.Converstion_rate.ToString(),
                    Trans.Converted_Amount == null ? "null" : Trans.Converted_Amount.ToString()
                    );

                DBqueries.Update(conn, Query, -1);
            }
            #endregion

            #region UPDATE CMS

            //string Params = String.Format("AuthCode=1rg651e56h4ert65hj41ertj4&action=UPDATE&CRMid={0}&status={1}{2}",
            //            Trans.id,
            //            Trans.status.ToString(),
            //            Trans.Amount == null ? "" : "&Amount=" + Trans.Amount
            //            );

            //#region Get Api Url 
            //int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(Trans.client_id);
            //string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
            //#endregion

            //string CMSres = PostBack.PostWithReturn(Api_Url + "Clients/InternalTransfers.aspx", Params);

            #endregion

            #region Make Transfer for approved

            if (Trans.status == Objects.TransactionStatuses.Approved)
            {
                MT.MT5.Builders.Objects.Withdrawal FromTA = new MT.MT5.Builders.Objects.Withdrawal()
                {
                    login = Trans.From_ta_login,
                    value = Trans.Amount
                };
                MT.MT5.Builders.Objects.Deposit ToTA = new MT.MT5.Builders.Objects.Deposit()
                {
                    login = Trans.To_ta_login,
                    value = Trans.Converted_Amount
                };

                FromTA.comment = "Int To: " + ToTA.login;
                FromTA.message = "Int To: " + ToTA.login;
                ToTA.comment = "Int From: " + FromTA.login;
                ToTA.message = "Int From: " + FromTA.login;

                string Api_username = accounts.Find(s => s.ServiceName == "NewMT5API").Username;
                string Api_password = accounts.Find(s => s.ServiceName == "NewMT5API").Password;
                string Api_Address = accounts.Find(s => s.ServiceName == "NewMT5API").Ip;

                MT.MT5.Create.Withdrawal(FromTA, Api_Address, Api_username, Api_password);
                MT.MT5.Create.Deposit(ToTA, Api_Address, Api_username, Api_password);
            }

            #endregion

            return "Success";
        }

        public static string UpdateClientDoc(Client.ClientDoc Doc)
        {
            //Reqs
            //Doc.id
            //Doc.status (Optional)
            //Doc.Type (Optional)

            Doc.Created_Date = DateTime.UtcNow;
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Update.UpdateClientDoc", ""))
            {
                #region Get Doc History

                string Query = String.Format("SELECT history FROM sales.documents WHERE id={0}", Doc.Id);
                Doc.History = serializer.Deserialize<List<Client.ClientDoc>>(DBqueries.SelectScalar(conn, Query).ToString());

                #endregion

                Doc.History.Add(Doc);

                #region Update Doc

                Query = String.Format("UPDATE sales.documents SET history='{1}',status=COALESCE({2}, status),type=COALESCE({3}, type) WHERE id={0}",
                    Doc.Id,
                    serializer.Serialize(Doc.History),
                    Doc.status == null ? "null" : "'" + Doc.status.ToString() + "'",
                    Doc.Type == null ? "null" : "'" + Doc.Type.ToString() + "'"
                    );

                #endregion
            }
            return "Success";
        }

        public static bool UpdatePool(System.Collections.Specialized.NameValueCollection data)
        {
            List<Pool> history = new List<Pool>();

            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Create.CreatePool", ""))
            {
                string Query = $"SELECT history FROM sales.pools WHERE id={data["P_Id"]}";
                history = serializer.Deserialize<List<Pool>>(DBqueries.SelectScalar(conn, Query).ToString());
                Pool pool = new Pool()
                {
                    name = data["P_Name"],
                    referrals = data["P_Ref"],
                    Countries = !string.IsNullOrEmpty(data["P_Countries"]) ? data["P_Countries"].Split(',').ToList() : new List<string>(),
                    mt_groups = serializer.Deserialize<List<MT_Group>>(data["P_MTG"]),
                    UpdatedBy = Utilities.GetCurrentUserLoginId(),
                    UpdatedDate = DateTime.UtcNow,
                    AutoConvertToRetention = String.IsNullOrEmpty(data["P_AutoRet"]) ? false : bool.Parse(data["P_AutoRet"]),
                    AutoContractsClose = String.IsNullOrEmpty(data["P_AutoContractsClose"]) ? false : bool.Parse(data["P_AutoContractsClose"]),
                    MT_Login_Start = int.Parse(data["P_MT_Login_Start"]),
                    MT_Login_End = int.Parse(data["P_MT_Login_End"]),
                    max_withdrawal = int.Parse(data["max_withdrawal"]),
                    is_enable = bool.Parse(data["is_enable"]),
                    agent = data["P_Agent"],
                    status = data["P_Status"]
                };

                history.Add(pool);

                string reff = data["P_Ref"];
                if (!String.IsNullOrEmpty(reff))
                    reff = $"#{reff.Replace(",", "#,#")}#";
                else
                    reff = "";



                Dictionary<string, object> p = new Dictionary<string, object>()
                {
                    {"@pname", data["P_Name"]},
                    {"@ref", reff},
                    {"@ctries", data["P_Countries"]},
                    {"@mtgs", data["P_MTG"]},
                    {"@AutoRet", bool.Parse(data["P_AutoRet"])},
                    {"@MT_Login_Start", int.Parse(data["P_MT_Login_Start"])},
                    {"@MT_Login_End", int.Parse(data["P_MT_Login_End"])},
                    {"@AutoContractsClose", bool.Parse(data["P_AutoContractsClose"])},
                    {"@history", serializer.Serialize(history)},
                    {"@id", int.Parse(data["P_Id"])},
                    {"@IsEnable", bool.Parse(data["is_enable"])},
                    {"@MaxWithdrawal", int.Parse(data["max_withdrawal"])},
                    {"@status", data["P_Status"]},
                    {"@agent", data["P_Agent"]},
                };

                string query = $"UPDATE sales.pools SET name = @pname, referrals = @ref, countries = @ctries, mt_groups = @mtgs, history = @history, auto_convert_to_retention = @AutoRet,auto_close_contracts= @AutoContractsClose,mt_login_start=@MT_Login_Start ,mt_login_end=@MT_Login_End, max_withdrawal = @MaxWithdrawal, is_enable = @IsEnable, agent = @agent, status = @status WHERE id = @id";
                return "Success" == DBqueries.Update(conn, query, -1, p);

            }
        }

        public static string UpdateTAsTradingInfos()
        {
            string Api_username = accounts.Find(s => s.ServiceName == "NewMT5API").Username;
            string Api_password = accounts.Find(s => s.ServiceName == "NewMT5API").Password;
            string Api_Address = accounts.Find(s => s.ServiceName == "NewMT5API").Ip;

            var users = MT.MT5.TraderInfo.GetAllMtAccounts(Api_Address, Api_username, Api_password, "balance,margin,marginFree,marginLevel,equity,login");
            string query = "";
            for (int i = 0; i < users.data.Count; i++)
            {
                query += $"UPDATE sales.trading_accounts SET balance={users.data[i].balance},margin={users.data[i].margin},free_margin={users.data[i].marginFree},margin_level={users.data[i].marginLevel},equity={users.data[i].equity} WHERE login={users.data[i].login}; ";
            }

            using (var conn = DBconnections.CRM("UpdateTAsBalance", ""))
            {
                return DBqueries.Update(conn, query, -1);
            };
        }

        public static string UpdateLastOnline()
        {
            string Api_username = accounts.Find(s => s.ServiceName == "NewMT5API").Username;
            string Api_password = accounts.Find(s => s.ServiceName == "NewMT5API").Password;
            string Api_Address = accounts.Find(s => s.ServiceName == "NewMT5API").Ip;

            var users = MT.MT5.TraderInfo.GetMtUsers(Api_Address, Api_username, Api_password, "lastAccess");

            string query = "";
            for (int i = 0; i < users.data.Count; i++)
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(users.data[i].lastAccess).ToLocalTime().AddHours(-2);
                query += $"UPDATE sales.trading_accounts SET last_online='{dtDateTime}' WHERE login={users.data[i].login}; ";
            }

            using (var conn = DBconnections.CRM("UpdateTAsBalance", ""))
            {
                return DBqueries.Update(conn, query, -1);
            };
        }

        public static bool UpdateCampaign(System.Collections.Specialized.NameValueCollection data)
        {
            string campaignHistory = GetCampaignHistory(data);
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Update.UpdateCampaign", ""))
            {
                int? Dialer_Id = !String.IsNullOrEmpty(data["C_Dialer_Id"]) ? Convert.ToInt32(data["C_Dialer_Id"]) : (int?)null;
                bool webAccess = string.IsNullOrEmpty(data["C_Create_Web_access"]) ? false : Convert.ToBoolean(data["C_Create_Web_access"]);
                bool auto_weight_change = string.IsNullOrEmpty(data["C_auto_weight_change"]) ? false : Convert.ToBoolean(data["C_auto_weight_change"]);

                string query = $"UPDATE marketing.campaigns SET campaign_name = @CampaignName, referral = @Referral, dialer_id = @DialerId, dialer_campaign_id = @DialerCampaignId, create_web_access = @CreateWebAccess, history = @History, url = @CampaignUrl,initial_weight=@InitialWeight,default_weight=@DefaultWeight,cost=@Cost,affiliate_id=@AffiliateId,auto_weight_change=@auto_weight_change WHERE campaign_id = @Id";
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.Parameters.AddWithValue("@CampaignName", data["C_Name"]);
                command.Parameters.AddWithValue("@Referral", data["C_Ref"]);
                command.Parameters.AddWithValue("@DialerId", Dialer_Id);
                command.Parameters.AddWithValue("@DialerCampaignId", data["C_Dialer_Campaign_Id"]);
                command.Parameters.AddWithValue("@CampaignUrl", data["C_Url"]);
                command.Parameters.AddWithValue("@CreateWebAccess", webAccess);
                command.Parameters.AddWithValue("@auto_weight_change", auto_weight_change);
                command.Parameters.AddWithValue("@History", campaignHistory);
                command.Parameters.AddWithValue("@Id", Convert.ToInt32(data["C_Id"]));
                command.Parameters.AddWithValue("@InitialWeight", int.Parse(data["C_initialWeight"]));
                command.Parameters.AddWithValue("@DefaultWeight", int.Parse(data["C_DefaultWeight"]));
                command.Parameters.AddWithValue("@Cost", string.IsNullOrEmpty(data["C_Cost"]) ? 0 : double.Parse(data["C_Cost"]));
                command.Parameters.AddWithValue("@AffiliateId", string.IsNullOrEmpty(data["C_AffiliateId"]) ? (object)DBNull.Value : int.Parse(data["C_AffiliateId"]));

                return "Success" == DBqueries.Update(conn, query, -1, command);
            }
        }


        public static bool UpdateLead(Client.ClientObject client)
        {
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Update.UpdateClient", ""))
            {
                List<Client.ClientObject> history = Utilities.GetHistories<Client.ClientObject>((int)client.Id, "sales.clients", "info_history", client);

                JavaScriptSerializer jss = new JavaScriptSerializer();


                string query = $"UPDATE sales.clients SET fname = '{Security.StripForm(client.Fname)}', lname = '{Security.StripForm(client.Lname)}', email = '{client.email}', phone = '{client.Phone}', referral = '{client.Referral}', country = '{client.country}', site_language = '{client.site_language}', info_history = '{jss.Serialize(history)}' WHERE id = {client.Id} ";

                var res = DBqueries.Update(conn, query, -1);
                //if (res == "Success")
                //{
                //    UpdateClientOnCMS(client);
                //}

                return res == "Success";
            }
        }

        public static ClientResponse UpdateClient(Client.ClientObject client)
        {

            #region Validate Phone Number
            numverify PhoneValidation2 = new numverify
            {
                valid = true,
            };
            numverify PhoneValidation3 = new numverify
            {
                valid = true,
            };

            var decryptedPhone = encryption.Decrypt(client.Phone);
            var PhoneValidation = Utilities.ValidatePhone(decryptedPhone) ?? new numverify();

            if (!string.IsNullOrEmpty(client.Phone2))
            {
                var decryptedPhone2 = encryption.Decrypt(client.Phone2);
                PhoneValidation2 = Utilities.ValidatePhone(decryptedPhone2) ?? new numverify();
                client.Phone2 = PhoneValidation2.international_format.Replace("+", "");
                client.Phone2 = encryption.Encryption(client.Phone2);
            }
            if (!string.IsNullOrEmpty(client.Phone3))
            {
                var decryptedPhone3 = encryption.Decrypt(client.Phone3);
                PhoneValidation3 = Utilities.ValidatePhone(decryptedPhone3) ?? new numverify();
                client.Phone3 = PhoneValidation3.international_format.Replace("+", "");
                client.Phone3 = encryption.Encryption(client.Phone3);
            }

            if (!PhoneValidation.valid || !PhoneValidation2.valid || !PhoneValidation3.valid)
            {
                return new ClientResponse { Success = false, Message = "Phone isn't valid, is phone valid? " + PhoneValidation3.valid };
            }
            #endregion

            client.Phone = PhoneValidation.international_format.Replace("+", "");
            client.Phone = encryption.Encryption(client.Phone);


            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Update.UpdateClient", ""))
            {
                List<Client.ClientObject> history = Utilities.GetHistories<Client.ClientObject>((int)client.Id, "sales.clients", "info_history", client);

                JavaScriptSerializer jss = new JavaScriptSerializer();
                //string query = $"UPDATE sales.clients SET fname = '{Security.StripForm(client.Fname)}', lname = '{Security.StripForm(client.Lname)}', email = '{client.email}', referral = '{client.Referral.Replace("'", "''")}', dob = '{client.Dob}', country = '{client.country}', phone = '{client.Phone}',phone2 = '{client.Phone2}',phone3 = '{client.Phone3}', state = '{client.state.Replace("'", "''")}', address1 = '{client.address1.Replace("'", "''")}', address2 = '{(client.address2 != null ? client.address2.Replace("'", "''") : client.address2)}', city = '{client.city}', zip_code = '{(client.zip_code != null ? client.zip_code.Replace("'", "''") : client.zip_code) }', site_language = '{client.site_language}', website_pass = '{client.web_password}', info_history = '{jss.Serialize(history)}', campaign_id = {(!string.IsNullOrEmpty(client.MarketingInfo.Campaign_id) ? client.MarketingInfo.Campaign_id : (object)DBNull.Value)}, utm_source = '{client.MarketingInfo.Utm_Source}' WHERE id = {client.Id} ";
                string query = $"UPDATE sales.clients SET fname = @Fname, lname = @Lname, " +
                    $" email = @Email, referral = @Referral, dob = @Dob, country = @Country, " +
                    $" phone = @Phone ,phone2 = @Phone2,phone3 = @Phone3, state = @State, " +
                    $" address1 = @Address1, address2 = @Address2 , " +
                    $" city = @City, zip_code = @Zipcode, " +
                    $" site_language = @SiteLanguage, website_pass = @WebsitePass, info_history = @InfoHistory, " +
                    $" campaign_id = @CampaignId,dialer_id=@dialer_id, " +
                    $" utm_source = @UtmSource WHERE id = @ClientId ";
                NpgsqlCommand p = new NpgsqlCommand(query, conn);

                p.Parameters.AddWithValue("@Fname", Security.StripForm(client.Fname));
                p.Parameters.AddWithValue("@Lname", Security.StripForm(client.Lname));
                p.Parameters.AddWithValue("@Email", client.email);
                p.Parameters.AddWithValue("@Referral", client.Referral.Replace("'", "''"));
                p.Parameters.AddWithValue("@Dob", client.Dob);
                p.Parameters.AddWithValue("@Country", client.country);
                p.Parameters.AddWithValue("@Phone", client.Phone);
                p.Parameters.AddWithValue("@Phone2", client.Phone2);
                p.Parameters.AddWithValue("@Phone3", client.Phone3);
                p.Parameters.AddWithValue("@City", client.city);
                p.Parameters.AddWithValue("@Zipcode", client.zip_code != null ? client.zip_code.Replace("'", "''") : client.zip_code);
                p.Parameters.AddWithValue("@State", client.state.Replace("'", "''"));
                p.Parameters.AddWithValue("@Address1", client.address1.Replace("'", "''"));
                p.Parameters.AddWithValue("@Address2", client.address2 != null ? client.address2.Replace("'", "''") : client.address2);
                p.Parameters.AddWithValue("@SiteLanguage", client.site_language);
                p.Parameters.AddWithValue("@WebsitePass", client.web_password);
                p.Parameters.AddWithValue("@InfoHistory", jss.Serialize(history));
                p.Parameters.AddWithValue("@UtmSource", !string.IsNullOrEmpty(client.MarketingInfo.Utm_Source) ? client.MarketingInfo.Utm_Source : string.Empty);
                p.Parameters.AddWithValue("@CampaignId", !string.IsNullOrEmpty(client.MarketingInfo.Campaign_id) ? int.Parse(client.MarketingInfo.Campaign_id) : (object)DBNull.Value);
                p.Parameters.AddWithValue("@InfoHistory", jss.Serialize(history));
                p.Parameters.AddWithValue("@ClientId", client.Id);
                p.Parameters.AddWithValue("@dialer_id", client.dialer_id);

                string res = DBqueries.Update(conn, query, -1, p);
                if (res == "Success")
                {
                    //if (client.converted_date != null)
                    //    UpdateClientOnCMS(client);
                    return new ClientResponse { Success = true, Message = res };
                }
                else
                    return new ClientResponse { Success = false, Message = res };

            }
        }

        public static ClientResponse ConvertLead(Client.ClientObject client)
        {
            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Update.ConvertLead", ""))
            {
                List<Client.ClientObject> history = Utilities.GetHistories<Client.ClientObject>((int)client.Id, "sales.clients", "info_history", client);

                JavaScriptSerializer jss = new JavaScriptSerializer();

                string query = $"UPDATE sales.clients SET converted_date = '{client.UpdatedDate}',website_pass ='{client.web_password}', entry_type = '{Client.ClientEnteryType.Lead}', dob = '{client.Dob}', state = '{client.state}', address1 = '{client.address1}', address2 = '{client.address2}', city = '{client.city}', zip_code = '{client.zip_code}', info_history = '{jss.Serialize(history)}' WHERE id = {client.Id} ";

                string res = DBqueries.Update(conn, query, -1);
                if (res == "Success")
                {
                    //CreateConvertedClientOnCMS(client);
                    return new ClientResponse { Success = true, Message = res };
                }
                else
                {
                    return new ClientResponse { Success = false, Message = res };
                }

            }
        }

        //public static string CreateConvertedClientOnCMS(Client.ClientObject client)
        //{
        //    string parameters = "AuthCode=1rg651e56h4ert65hj41ertj4&action=NEW&client_id=" + client.Id
        //                      + "&deposit=" + client.deposit_amount
        //                      + "&withdraw=" + client.withdrawal_amount
        //                      + "&dob=" + client.Dob
        //                      + "&address1=" + client.address1
        //                      + "&address2=" + client.address2
        //                      + "&city=" + client.city
        //                      + "&Country=" + client.country
        //                      + "&email=" + client.email
        //                      + "&fname=" + client.Fname
        //                      + "&lname=" + client.Lname
        //                      + "&phone=" + client.Phone
        //                      + "&State=" + client.state
        //                      + "&verified=" + client.verified
        //                      + "&web_password=" + client.web_password
        //                      + "&zip_code=" + client.zip_code
        //                      + "&referral=" + client.Referral;

        //    #region Get Api Url 
        //    int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(client.Id);
        //    string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
        //    #endregion

        //    string cmsResponse = PostBack.PostWithReturn(Api_Url + "Clients/client.aspx", parameters);
        //    return cmsResponse;
        //}

        //public static string UpdateClientOnCMS(Client.ClientObject client)
        //{
        //    if (!Security.IsDigitsOnly(client.Phone))
        //    {
        //        client.Phone = encryption.Decrypt(client.Phone);
        //        client.email = encryption.Decrypt(client.email);
        //    }
        //    string parameters = "AuthCode=1rg651e56h4ert65hj41ertj4&action=UPDATE&client_id=" + client.Id
        //                      + "&dob=" + Convert.ToDateTime(client.Dob).ToString("yyyy-MM-dd")
        //                      + "&address1=" + client.address1
        //                      + "&address2=" + client.address2
        //                      + "&city=" + client.city
        //                      + "&Country=" + client.country
        //                      + "&email=" + client.email
        //                      + "&fname=" + client.Fname
        //                      + "&lname=" + client.Lname
        //                      + "&phone=" + client.Phone
        //                      + "&State=" + client.state
        //                      + "&web_password=" + client.web_password
        //                      + "&zip_code=" + client.zip_code;

        //    #region Get Api Url 
        //    int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(client.Id);
        //    string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
        //    #endregion

        //    string cmsResponse = PostBack.PostWithReturn(Api_Url + "Clients/client.aspx", parameters);
        //    return cmsResponse;
        //}

        //public static string UpdateVerifyClientOnCMS(Client.ClientObject client)
        //{
        //    #region Get Api Url 
        //    int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(client.Id);
        //    string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
        //    #endregion

        //    string parameters = "AuthCode=1rg651e56h4ert65hj41ertj4&action=UPDATE&client_id=" + client.Id + "&verified=" + client.verified;
        //    string cmsResponse = PostBack.PostWithReturn(Api_Url + "Clients/client.aspx", parameters);
        //    return cmsResponse;
        //}

        //public static string UpdateClientLoginableOnCMS(Client.ClientObject client)
        //{
        //    #region Get Api Url 
        //    int SiteVersion = SiteVersionExtension.GetSiteVersion_Of_Client(client.Id);
        //    string Api_Url = SiteVersionExtension.GetSiteVersion(SiteVersion).ApiBaseUrl;
        //    #endregion

        //    string parameters = "AuthCode=1rg651e56h4ert65hj41ertj4&action=UPDATE&client_id=" + client.Id + "&loginable=" + client.Loginable;
        //    string cmsResponse = PostBack.PostWithReturn(Api_Url + "Clients/client.aspx", parameters);
        //    return cmsResponse;
        //}

        public static bool UpdateDocuments(Client.ClientDoc document)
        {
            List<Client.ClientDoc> history = new List<Client.ClientDoc>();
            using (var conn = DBconnections.CRM("CRM", ""))
            {
                string Query = $"SELECT history FROM sales.documents WHERE id={document.Id}";
                history = serializer.Deserialize<List<Client.ClientDoc>>(DBqueries.SelectScalar(conn, Query).ToString());
                history.Add(document);
                string query = $"UPDATE sales.documents SET type='{document.Type}', status='{document.status}', "
                             + $"expiration='{document.Expiration}', note='{document.Note}', history='{serializer.Serialize(history)}' "
                             + $"WHERE id={document.Id}";

                return DBqueries.Update(conn, query, -1) == "Success" ? true : false;
            };
        }

        public static string UpdateWebsiteClientState(string Client_id, bool Online)
        {
            if (String.IsNullOrEmpty(Client_id) || !Security.IsDigitsOnly(Client_id))
                return "";

            using (NpgsqlConnection conn = DBconnections.CRM("Classes.BrandsMaster.CRM.Update.UpdateWebsiteClientState", ""))
            {
                string Query = String.Format("UPDATE sales.clients SET online='{0}' WHERE id={1}", Online, Client_id);
                string res = DBqueries.Update(conn, Query, -1);
                if (res != "Success")
                    return "Error";
                return res;
            }

        }

        public static void UpdateGroupSymbols()
        {
            Dictionary<string, List<string>> groupSymbols = MT.MT5.Utilities.GetGroupSymbols();
            StringBuilder builder = new StringBuilder();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            using (var conn = DBconnections.CRM("", ""))
            {
                foreach (var groupSymbol in groupSymbols)
                {
                    string selectQuery = $"SELECT symbol FROM sales.mt_symbols WHERE group_name = '{groupSymbol.Key}'";
                    DataTable symbol = DBqueries.Select(conn, selectQuery);
                    string symbols = ser.Serialize(groupSymbol.Value);

                    if (symbol is null || symbol.Rows.Count == 0)
                    {
                        string insertQuery = $"INSERT INTO sales.mt_symbols (group_name, symbol) VALUES ('{groupSymbol.Key}', '{symbols}')";
                        DBqueries.Insert(conn, insertQuery);
                    }
                    else
                    {
                        string updateQuery = $"UPDATE sales.mt_symbols SET symbol = '{symbols}' WHERE group_name = '{groupSymbol.Key}'";
                        DBqueries.Update(conn, updateQuery, -1);
                    }
                }
            }
        }

        public static bool CDNpurge(string DistributionId, string[] arrayofpaths)
        {
            for (int i = 0; i < arrayofpaths.Length; i++)
            {
                arrayofpaths[i] = Uri.EscapeUriString(arrayofpaths[i]);
            }
            try
            {
                string AccessKeyId = accounts.Find(s => s.ServiceName == "S3Bucket").Username;
                string SecretAccessKeyId = accounts.Find(s => s.ServiceName == "S3Bucket").Password;
                AmazonCloudFrontClient oClient = new AmazonCloudFrontClient(AccessKeyId, SecretAccessKeyId, Amazon.RegionEndpoint.EUCentral1);
                CreateInvalidationRequest oRequest = new CreateInvalidationRequest();
                oRequest.DistributionId = DistributionId;
                oRequest.InvalidationBatch = new InvalidationBatch
                {
                    CallerReference = DateTime.Now.Ticks.ToString(),
                    Paths = new Paths
                    {
                        Items = arrayofpaths.ToList<string>(),
                        Quantity = arrayofpaths.Length
                    }
                };

                CreateInvalidationResponse oResponse = oClient.CreateInvalidation(oRequest);
                oClient.Dispose();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetCampaignHistory(System.Collections.Specialized.NameValueCollection data)
        {
            string result = string.Empty;
            Campaign campaign = new Campaign();

            List<Campaign> history = new List<Campaign>();

            string query = $"SELECT * FROM marketing.campaigns WHERE campaign_id = {data["C_Id"]}";

            var currentuser = Cookie.GetCookie("id", false);

            using (NpgsqlConnection conn = DBconnections.CRM("", ""))
            {
                var reader = DBqueries.ExecuteReader(conn, query);
                if (reader.Read())
                {

                    history = !string.IsNullOrEmpty(reader["history"].ToString()) ? serializer.Deserialize<List<Campaign>>(reader["history"].ToString()) : new List<Campaign>();
                    campaign.campaign_id = (int)reader["campaign_id"];
                    campaign.UpdatedBy = !string.IsNullOrEmpty(currentuser.ToString()) ? Convert.ToInt32(encryption.Decrypt(currentuser)) : (int?)null;
                    campaign.UpdatedDate = (DateTime?)DateTime.UtcNow;

                    campaign.campaign_name = data["C_Name"];
                    campaign.referral = data["C_Ref"];
                    campaign.dialer_id = !string.IsNullOrEmpty(data["C_Dialer_Id"]) ? Convert.ToInt32(data["C_Dialer_Id"]) : (int?)null;
                    campaign.dialer_campaign_id = data["C_Dialer_Campaign_Id"];
                    campaign.createWebAccess = string.IsNullOrEmpty(data["C_Create_Web_access"]) ? false : Convert.ToBoolean(data["C_Create_Web_access"]);
                    campaign.CampaignUrl = data["C_Url"];

                    history.Add(campaign);

                    result = serializer.Serialize(history);
                }
            };
            return result;
        }

    }
}
