using CRM.Utilities;
using CRM.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Services;
using static Classes.BrandsMaster.CRM.Builders.Objects;

// include Create class as static so that we can invoke its methods without including its full namespace and class name.
using static Classes.BrandsMaster.CRM.Update;
using static Classes.BrandsMaster.CRM.Utilities;


namespace CRM.Admin.Pools
{
    public partial class Edit : PoolParentPage
    {

        public Pool EPool { get; set; }
        protected List<Classes.Country> countries { get; set; }
        protected string strPools { get; set; }
        public string AutoConvertOptions = "";
        public string AutoContractsCloseOptions = "";
        protected void Page_Load(object sender, EventArgs e)
        {

            int id = default(int);

            if (int.TryParse(Request.QueryString["id"], out id) == false)
                Response.Redirect("/Admin/Pools/Index.aspx");

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            this.EPool = PoolUtilities.GetPoolById(id);

            var selectedMtgs = this.EPool.mt_groups;

            if (Request.HttpMethod == "POST")
            {
                string referrals = Request.Form["P_Ref"];
                int pool_id = Convert.ToInt32(Request.Form["P_Id"]);
                NameValueCollection insertData = PoolUtilities.ProcessInputData(Request.Form);
                selectedMtgs = GetMTGroupFromString(insertData["P_MTG"]);

                if (!string.IsNullOrEmpty(referrals))
                {
                    string[] referral1 = referrals.Split(',');
                    if (referral1.GroupBy(x => x).Any(g => g.Count() > 1))
                    {
                        SessionPush("toast", new KeyValuePair<string, string>("error", "referral contains duplicate"));
                    }
                    else
                    {
                        string referralExist = checkingReferrals(referrals, pool_id);
                        if (referralExist != "True")
                        {
                            //insertData["max_withdrawal"] = maxWithdrawal.Value;
                            if (UpdatePool(insertData))
                            {
                                SessionPush("toast", new KeyValuePair<string, string>("success", "Pool was Updated!"));
                                Response.Redirect(Request.Url.AbsoluteUri);
                            }
                            SessionPush("toast", new KeyValuePair<string, string>("error", "An error occured while processing your request, please try again."));
                            SessionPush("P_Name", insertData["P_Name"]);
                            SessionPush("P_Ref", javaScriptSerializer.Serialize(PoolUtilities.MultiSelectField(insertData["P_Ref"])));
                            mtgOptions.InnerHtml = MTGroupUtilities.UIHelper.MtgOptions(selectedMtgs);
                        }
                        else if (referralExist == "True")
                        {
                            SessionPush("toast", new KeyValuePair<string, string>("error", "referral already existed in other pool"));
                        }
                    }
                }
                else
                {
                    if (UpdatePool(insertData))
                    {
                        SessionPush("toast", new KeyValuePair<string, string>("success", "Pool was Updated!"));
                        Response.Redirect(Request.Url.AbsoluteUri);
                    }
                    SessionPush("toast", new KeyValuePair<string, string>("error", "An error occured while processing your request, please try again."));
                    SessionPush("P_Name", insertData["P_Name"]);
                    mtgOptions.InnerHtml = MTGroupUtilities.UIHelper.MtgOptions(selectedMtgs);
                }

            }
            else
            {
                if (this.EPool == null)
                    Response.Redirect("/Admin/Pools/Index.aspx");

                if (this.EPool.AutoConvertToRetention)
                    AutoConvertOptions = "<option value='true' selected>YES</option> <option value='false'>NO</option>";
                else
                    AutoConvertOptions = "<option value='true'>YES</option> <option value='false' selected>NO</option>";

                if (this.EPool.AutoContractsClose)
                    AutoContractsCloseOptions = "<option value='true' selected>YES</option> <option value='false'>NO</option>";
                else
                    AutoContractsCloseOptions = "<option value='true'>YES</option> <option value='false' selected>NO</option>";

                SessionPush("Old_P_Ref", javaScriptSerializer.Serialize(PoolUtilities.MultiSelectField(this.EPool.referrals)));

                if (EPool.Countries != null)
                {
                    strPools = String.Join("!#!", EPool.Countries);
                }

                PoolUtilities poolUtilities = new PoolUtilities();
                List<Classes.Country> listCountries = Classes.countries.ReturnListOfCountries();

                List<string> strCntrs = new List<string>();
                strCntrs = poolUtilities.GetPoolCountries($" AND id != {id}");

                listCountries.RemoveAll(x =>
                {
                    return strCntrs.Find(sc => sc == x.Name) != null;
                });
                countryOptions.InnerHtml = Classes.countries.GetCountryListNames();
                chkEnableMax.Checked = EPool.is_enable;
                chkSendReport.Checked = EPool.send_enable;
                mtgOptions.InnerHtml = MTGroupUtilities.UIHelper.MtgOptions(selectedMtgs);
                //maxWithdrawal.Value = EPool.max_withdrawal.ToString();
            }



        }

        public static string checkingReferrals(string referral, int id)
        {
            string query = $"SELECT referrals FROM sales.pools WHERE string_to_array(referrals, ',') && ARRAY['#{referral.Replace(",", "#', '#")}#'] AND id != {id}";

            using (var conn = Classes.BrandsMaster.DBconnections.CRM("CRM.Utilities.PoolUtilities", "checkingReferrals"))
            {
                var result = Classes.DB.SelectScalar(conn, query);
                return result != null ? "True" : "False";
            }
        }


        [WebMethod]
        public static string CheckReferralExist(string referral)
        {
            string query = $"SELECT referrals FROM sales.pools WHERE string_to_array(referrals, ',') && ARRAY['#{referral.Replace(",", "#', '#")}#']";

            using (var conn = Classes.BrandsMaster.DBconnections.CRM("CRM.Utilities.PoolUtilities", "CheckReferralExist"))
            {
                var result = Classes.DB.SelectScalar(conn, query);
                return result != null ? "True" : "False";
            }
        }

        [WebMethod]
        public static object CreateStatusDefaultAgent(string id, string status, string agent, string context)
        {

            StatusDefaultAgentUtilities utilities = new StatusDefaultAgentUtilities();
            string response = utilities.CreateStatusDefaultAgentDB(id, status, agent, context);
            return response;

        }

        [WebMethod]
        public static object UpdateStatusDefaultAgent(string id, string status, string agent, string context)
        {

            StatusDefaultAgentUtilities utilities = new StatusDefaultAgentUtilities();
            string response = utilities.UpdateStatusDefaultAgentDB(id, status, agent, context);
            return response;

        }

        [WebMethod]
        public static List<StatusDefaultAgentModels> ReadStatusDefaultAgent(string id)
        {
            StatusDefaultAgentUtilities utilities = new StatusDefaultAgentUtilities();
            var response = utilities.ReadStatusDefaultAgentDB(id);
            List <StatusDefaultAgentModels> defaults = response.Key;
            return defaults;
        }

        [WebMethod]
        public static object DeleteStatusDefaultAgent(int id)
        {
            StatusDefaultAgentUtilities utilities = new StatusDefaultAgentUtilities();
            int response = utilities.DeleteStatusDefaultAgentDB(id);
            return response;
        }

        [WebMethod]
        public static List<StatusDefaultAgentModels> FetchStatusforDefaults(string id)
        {
            StatusDefaultAgentUtilities utilities = new StatusDefaultAgentUtilities();
            var response = utilities.FetchStatusforDefaultsDB(id);
            List<StatusDefaultAgentModels> defaults = response.Key;
            return defaults;
        }
    }
}