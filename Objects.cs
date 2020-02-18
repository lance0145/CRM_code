using System;
using System.Collections.Generic;
using System.Text;
using static Classes.BrandsMaster.CRM.Builders.Client;

namespace Classes.BrandsMaster.CRM.Builders
{
    public class Objects
    {
        public class numverify
        {
            public bool valid { get; set; }
            public string number { get; set; }
            public string local_format { get; set; }
            public string international_format { get; set; }
            public string country_prefix { get; set; }
            public string country_code { get; set; }
            public string country_name { get; set; }
            public string location { get; set; }
            public string carrier { get; set; }
            public object line_type { get; set; }
        }

        public class Pool
        {
            // for dapper mapper purpose 
            public string Csv_Countries { get; set; }
            public string Csv_Mt_Groups { get; set; }
            public string Csv_History { get; set; }
            public int count { get; set; }

            public int id { get; set; }
            public string name { get; set; }
            public string referrals { get; set; }
            public List<string> Countries { get; set; }

            public List<MT_Group> mt_groups { get; set; }
            public List<Pool> history { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public bool AutoConvertToRetention { get; set; }
            public bool AutoContractsClose { get; set; }
            public int MT_Login_Start { get; set; }
            public int MT_Login_End { get; set; }
            public int? max_withdrawal { get; set; }
            public bool is_enable { get; set; }
            public string status { get; set; }
            public string agent { get; set; }

            public Pool()
            {
                this.Countries = null;
            }
        }

        public class SiteVersion
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Pool { get; set; }
            public string ApiBaseUrl { get; set; }
            public string Website_Url { get; set; }
            public string Cms_Url { get; set; }
            public List<SiteVersion> history { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public int? Count { get; set; }
        }

        public class FundProcessor
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<FundProcessor> history { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }
        }

        public class Psp
        {
            public int PspId { get; set; }
            public string PspName { get; set; }
            public string PspLink { get; set; }
            public int FundProcessorId { get; set; }
            public List<Psp> history { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public bool isReal = true;
        }

        public class MT_Account
        {
            public int? id { get; set; }
            public int? client_id { get; set; }
            public int? Login { get; set; }
            public DateTime created_date { get; set; }
            public DateTime ftd_date { get; set; }
            public double? deposit_amount { get; set; }
            public double? withdrawal_amount { get; set; }
            public string investor_pass { get; set; }
            public string master_pass { get; set; }
            public MT_Group Group { get; set; }
            public MTTypes Account_Type { get; set; }
            public List<MT_Account> history { get; set; }

            public override string ToString()
            {
                StringBuilder MT_Account = new StringBuilder();
                MT_Account.AppendFormat("id = {0}", id)
                                  .AppendFormat(" | client_id = {0}", client_id)
                                  .AppendFormat(" | Login ={0}", Login)
                                  .AppendFormat(" | created_date = {0}", created_date)
                                  .AppendFormat(" | ftd_date = {0}", ftd_date)
                                  .AppendFormat(" | deposit_amount = {0}", deposit_amount)
                                  .AppendFormat(" | withdrawal_amount = {0}", withdrawal_amount)
                                  .AppendFormat(" | investor_pass = {0}", investor_pass)
                                  .AppendFormat(" | master_pass = {0}", master_pass)
                                  .AppendFormat(" | Group = {0}", Group.ToString())
                                  .AppendFormat(" | Account_Type = {0}", Account_Type.ToString());
                return MT_Account.ToString();

            }
        }

        public class MT_Group
        {
            public int id { get; set; }
            public string name { get; set; }
            public MT.MT5.Builders.Objects.MTCurrencies Currency { get; set; }
            public int Leverage { get; set; }
            public bool Default { get; set; }
            public MTTypes Type { get; set; }
            public double InitialBalance { get; set; }
            public List<MT_Group> history { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }

            public override string ToString()
            {
                StringBuilder MT_Group = new StringBuilder();
                MT_Group.AppendFormat("id = {0}", id)
                                  .AppendFormat(" | name = {0}", name)
                                  .AppendFormat(" | Currency ={0}", Currency.ToString())
                                  .AppendFormat(" | Leverage = {0}", Leverage)
                                  .AppendFormat(" | Default = {0}", Default)
                                  .AppendFormat(" | Type = {0}", Type)
                                  .AppendFormat(" | InitialBalance = {0}", InitialBalance)
                                  .AppendFormat(" | UpdatedDate = {0}", UpdatedDate)
                                  .AppendFormat(" | UpdatedBy = {0}", UpdatedBy);
                return MT_Group.ToString();
            }

            public string currency_string
            {
                get
                {
                    return Enum.Parse(typeof(MT.MT5.Builders.Objects.MTCurrencies), Currency.ToString()).ToString();
                }
            }
        }

        public class Transaction
        {
            public int? id { get; set; }
            public string psp_transaction_id { get; set; }
            public int? client_id { get; set; }
            public int? ta_id { get; set; }
            public int? ta_login { get; set; }
            public int? psp_id { get; set; }
            public int? fundprocessor_id { get; set; }
            public TransactionStatuses? status { get; set; }
            public TransactionType? Type { get; set; }
            public string pool { get; set; }
            public string current_pool { get; set; }
            public string card_holder { get; set; }
            public string card_number { get; set; }
            public string card_expiry { get; set; }
            public string note { get; set; }
            public int? Praxis_Trace_Id { get; set; }
            public bool? isfake = false;
            public bool? is_ftd = false;
            public bool? is_self_deposit = false;
            
            public int agent_id { get; set; }
            public bool? sync_with_mt { get; set; }
            public bool sync_client_area = true;

            public DateTime? created_date { get; set; }
            public DateTime? created_date_psp_time { get; set; }
            public DateTime? approved_date { get; set; }


            public DateTime? Updated_date { get; set; }
            public MT.MT5.Builders.Objects.MTCurrencies? currency { get; set; }

            public double? convertion_rate_to_usd { get; set; }
            public double? amount { get; set; }
            public double? ta_amount { get; set; }
            public double? usd_amount { get; set; }
            public MT.MT5.Builders.Objects.MTCurrencies? Convert_To { get; set; }

            public string UpdatedBy { get; set; }

            public string agentname { get; set; }
            public string dodSigned { get; set; }

            public ClientObject Client { get; set; }

            public List<Transaction> history { get; set; }

            public override string ToString()
            {
                StringBuilder transactionDetails = new StringBuilder();
                transactionDetails.AppendFormat("id = {0}", id)
                                  .AppendFormat(" | client_id = {0}", client_id)
                                  .AppendFormat(" | Type ={0}", Type.ToString())
                                  .AppendFormat(" | ta_id = {0}", ta_id)
                                  .AppendFormat(" | ta_login = {0}", ta_login)
                                  .AppendFormat(" | psp_id = {0}", psp_id)
                                  .AppendFormat(" | fundprocessor_id = {0}", fundprocessor_id)
                                  .AppendFormat(" | status = {0}", status)
                                  .AppendFormat(" | card_holder = {0}", card_holder)
                                  .AppendFormat(" | card_number = {0}", card_number)
                                  .AppendFormat(" | card_expiry = {0}", card_expiry)
                                  .AppendFormat(" | note = {0}", note)
                                  .AppendFormat(" | isfake = {0}", isfake)
                                  .AppendFormat(" | sync_with_mt = {0}", sync_with_mt)
                                  .AppendFormat(" | created_date = {0}", created_date)
                                  .AppendFormat(" | Updated_date = {0}", Updated_date)
                                  .AppendFormat(" | currency = {0}", currency)
                                  .AppendFormat(" | convertion_rate_to_usd = {0}", convertion_rate_to_usd)
                                  .AppendFormat(" | amount = {0}", amount)
                                  .AppendFormat(" | usd_amount = {0}", usd_amount)
                                  .AppendFormat(" | AgentName = {0}", agentname)
                                  .AppendFormat(" | Current Pool = {0}", current_pool)
                                  .AppendFormat(" | Pool = {0}", pool)
                                  .AppendFormat(" | is_self_deposit = {0}", is_self_deposit)
                                  .AppendFormat(" | UpdatedBy = {0}", UpdatedBy);
                return transactionDetails.ToString();
            }
        }

        public class ConvertReturn
        {
            public double? convertion_rate { get; set; }
            public double? amount { get; set; }
        }

        public class Internal_Transfer
        {
            public int? id { get; set; }
            public int? client_id { get; set; }
            public int? From_ta_login { get; set; }
            public int? To_ta_login { get; set; }
            public string From_ta_login_currency { get; set; }
            public string To_ta_login_currency { get; set; }
            public double? Amount { get; set; }
            public double? Converted_Amount { get; set; }
            public double? Converstion_rate { get; set; }
            public DateTime Created_Date { get; set; }
            public TransactionStatuses? status { get; set; }
            public List<Internal_Transfer> History { get; set; }
            public virtual Client.ClientObject Client { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }

            public string GetTransactionStatus { get; set; }

            public override string ToString()
            {
                StringBuilder InternalTransfer = new StringBuilder();
                InternalTransfer.AppendFormat("id = {0}", id)
                             .AppendFormat(" | client_id = {0}", client_id)
                             .AppendFormat(" | From_ta_login = {0}", From_ta_login)
                             .AppendFormat(" | To_ta_login = {0}", To_ta_login)
                             .AppendFormat(" | From_ta_login_currency = {0}", From_ta_login_currency)
                             .AppendFormat(" | To_ta_login_currency = {0}", To_ta_login_currency)
                             .AppendFormat(" | Amount = {0}", Amount)
                             .AppendFormat(" | Created_Date = {0}", Created_Date)
                             .AppendFormat(" | status = {0}", status.ToString());

                return InternalTransfer.ToString();
            }

        }

        public class Document
        {
            public int Id { get; set; }
            public int? Client_Id { get; set; }
            public DocsType? Type { get; set; }
            public string Filename { get; set; }
            public DocsStatuses? Status { get; set; }
            public DateTime? Created_Date { get; set; }
            public string Note { get; set; }
            public DateTime? Expiration { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime UpdatedDate { get; set; }
            public List<Document> History { get; set; }

            public string GetHistory { get; set; }
            public string GetDocumentType { get; set; }
            public string GetStatus { get; set; }
            public virtual Client.ClientObject Client { get; set; }
        }

        public class Campaign
        {
            public int campaign_id { get; set; }
            public int Count { get; set; }
            public string campaign_name { get; set; }
            public string utm_source { get; set; }
            public int? dialer_id { get; set; }
            public string dialer_campaign_id { get; set; }
            public string referral { get; set; }
            public string totalleads { get; set; }
            public string totalftd { get; set; }
            public string totaldepositor { get; set; }
            public string country { get; set; }
            public string email { get; set; }
            public List<string> referralOptions { get; set; }
            public List<string> countryOptions { get; set; }
            public List<string> campaignIdOptions { get; set; }
            public List<string> campaignNameOptions { get; set; }
            public List<string> utmOptions { get; set; }
            public bool createWebAccess = false;
            public bool auto_weight_change = false;
            public List<Campaign> CampaignHistory { get; set; }
            public int? UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedByName { get; set; }
            public string CampaignUrl { get; set; }
            public string InitialWeight { get; set; }
            public string DefaultWeight { get; set; }
            public int AffiliateId { get; set; }
            public double Cost { get; set; }
        }

        public enum DocsType
        {
            Proof_Of_Id,
            Proof_Of_Residence,
            Power_Of_Attorney,
            SWIFT,
            Declaration_Of_Deposit,
            Declaration_Of_Address,
            Credit_Card_Front,
            Credit_Card_Back,
            Other,
            Virtual_Card,
            Online_Banking_Profile,
            Invoice,
            POA_Declaration_of_Deposit,
            Marriage_Certificate
        }


        public enum ClientStatus
        {
            New, Reassigned
        }


        public enum MTTypes
        {
            Live,
            Demo
        }

        public enum TransactionStatuses
        {
            Approved,
            Rejected,
            Initial,
            Canceled,
            Pending,
            Split
        }

        public enum DocsStatuses
        {
            Approved,
            Rejected,
            Initial
        }

        public enum TransactionType
        {
            Deposit,
            Withdrawal,
            Credit
        }

        public enum Account_Levels
        {
            Rookie,
            Basic,
            Premier,
            Elite,
            Vip
        }
    }
}
