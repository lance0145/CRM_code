using static Classes.BrandsMaster.CRM.Builders.Objects;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using Npgsql;
using Classes.BrandsMaster;
using System;
using Classes.BrandsMaster.ConnectionString;
using Dapper;
using System.Web.UI.WebControls;

namespace CRM.Utilities
{
    public class PoolUtilities
    {
        private static PoolUtilities instance;

        public static PoolUtilities Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PoolUtilities();
                }
                return instance;
            }
        }

        protected DynamicParameters DynaParams;
        protected string OrderBy = String.Empty;

        
        
        public static NameValueCollection ProcessInputData(NameValueCollection formData)
        {
            NameValueCollection data = new NameValueCollection();

            List<string> MtGroups = MultiSelectField(formData["P_MTG"]);

            List<MT_Group> selectedGroups = new List<MT_Group>();

            foreach (var mtName in MtGroups)
            {
                var mt = MTGroupUtilities.GetMtByQuery($"name = '{mtName}'");

                if (mt != null)
                {
                    selectedGroups.Add(mt);
                }

            }

            JavaScriptSerializer jss = new JavaScriptSerializer();

            data["P_Id"] = formData["P_Id"] ?? "";
            data["P_MTG"] = jss.Serialize(selectedGroups);
            data["P_Ref"] = formData["P_Ref"];
            data["P_Countries"] = formData["P_Countries"];
            data["P_Status"] = formData["P_Status"];
            data["P_Agent"] = formData["P_Agent"];
            data["P_Name"] = formData["P_Name"];
            data["P_AutoRet"] = formData["P_AutoRet"];
            data["P_AutoContractsClose"] = formData["P_AutoContractsClose"];
            data["P_MT_Login_Start"] = formData["P_MT_Login_Start"];
            data["P_MT_Login_End"] = formData["P_MT_Login_End"];
            data["max_withdrawal"] = formData["maxWithdrawal"];
            if(formData["ctl00$ctl00$MainContent$LayoutMainContent$chkEnableMax"] != null && formData["ctl00$ctl00$MainContent$LayoutMainContent$chkEnableMax"] == "on")
            {
                data["is_enable"] = true.ToString();
            }
            else
            {
                data["is_enable"] = false.ToString();
            }

            if (!string.IsNullOrEmpty(data["P_Ref"]))
                data["P_Ref"] = data["P_Ref"].ToLower();

            return data;
        }

        public void SetOrderBy(Dictionary<string, object> order = null)
        {
            if (order != null && order.ContainsKey("attr") && order.ContainsKey("order"))
            {
                this.OrderBy = $"ORDER BY {order["attr"]} {order["order"].ToString().ToUpper()} NULLS LAST ";
            }
        }

        public static List<string> MultiSelectField(string refAsString)
        {
            return refAsString.Split(',').ToList<string>();
        }

        public List<KeyValuePair<int, string>> GetPoolOptions()
        {
            string query = "SELECT id, name FROM sales.pools ORDER BY name";
            DataTable dt = new DataTable();
            using (var conn = Classes.BrandsMaster.DBconnections.CRM("CRM", ""))
            {
                dt = Classes.DB.Select(conn, query);
            };

            List<KeyValuePair<int, string>> pools = new List<KeyValuePair<int, string>>();
            if (dt.Rows.Count > 0)
            {
                pools = dt.AsEnumerable().Select(x => new KeyValuePair<int, string>(x.Field<int>("id"), x.Field<string>("name"))).ToList();
            }

            return pools;
        }

        public IEnumerable<Pool> SelectPoolIdAndName()
        {

            string query = "SELECT id, name FROM sales.pools ORDER BY name";
            using (var conn = Classes.BrandsMaster.DBconnections.CRM("SelectPoolByName", "114"))
            {
                var result = conn.Query<Pool>(query);

                return result;
            }
        }
        

        public static Pool GetPoolById(int id)
        {
            Pool pool = null;

            List<Classes.Country> listCountries = Classes.countries.ReturnListOfCountries();

            using (NpgsqlConnection conn = DBconnections.CRM("CRM.Utilities.PoolUtilities", ""))
            {
                string q = "SELECT * FROM sales.pools WHERE id =" + id;
                DataTable mtgres = Classes.BrandsMaster.DBqueries.Select(conn, q);

                if (mtgres.Rows.Count > 0)
                {
                    var r = mtgres.Rows[0];
                    var ia = r.ItemArray;
                    return new Pool()
                    {
                        id = (int)r.ItemArray[0],
                        name = (string)r.ItemArray[1],
                        referrals = !string.IsNullOrEmpty(r.ItemArray[2].ToString()) ? (string)r.ItemArray[2].ToString().Replace("#","") : "",
                        Countries = !string.IsNullOrEmpty(r.ItemArray[3].ToString()) ? Classes.BrandsMaster.CRM.Utilities.GetCountriesfromString((string)r.ItemArray[3], listCountries) : null,
                        mt_groups = !string.IsNullOrEmpty(r.ItemArray[4].ToString()) ? Classes.BrandsMaster.CRM.Utilities.GetMTGroupFromString((string)r.ItemArray[4]) : null,
                        AutoConvertToRetention = (bool)r.ItemArray[6],
                        AutoContractsClose = (bool)r.ItemArray[7],
                        MT_Login_Start= (int)r.ItemArray[8],
                        MT_Login_End = (int)r.ItemArray[9],
                        max_withdrawal = !string.IsNullOrEmpty(r.ItemArray[10].ToString()) ? (int?)r.ItemArray[10] : null,
                        is_enable = (bool)r.ItemArray[11],
                        agent = !string.IsNullOrEmpty(r.ItemArray[12].ToString()) ? (string)r.ItemArray[12] : null,
                        status = !string.IsNullOrEmpty(r.ItemArray[13].ToString()) ? (string)r.ItemArray[13] : null,
                    };

                }

            }

            return null;
        }

        public static List<Pool> GetHistory(string id)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Pool> History = new List<Pool>();
            string query = "SELECT history FROM sales.pools WHERE id=" + id;
            using (NpgsqlConnection conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                var history = Classes.DB.SelectScalar(conn, query);
                if (history != null)
                {
                    History = serializer.Deserialize<List<Pool>>(history.ToString());
                }
            }
            return History;
        }

        public static List<Pool> GetPoolByQuery(string whereclause = "")
        {
            List<Pool> pools = new List<Pool>();
            string query = string.Format("SELECT * FROM sales.pools WHERE 1 = 1 {0}", whereclause);

            using (var conn = Classes.BrandsMaster.DBconnections.CRM("CRM.Utilities.PoolUtilities", "GetPoolByQuery"))
            {
                DataTable dt = Classes.DB.Select(conn, query);

                if (dt.Rows.Count > 0)
                {
                    pools = dt.AsEnumerable().Select(x => new Pool
                    {
                        id = x.Field<int>("id"),
                        name = x.Field<string>("name"),
                        referrals = x.Field<string>("referrals").Replace("#",""),
                        Countries = Classes.BrandsMaster.CRM.Utilities.GetCountriesfromString(x.Field<string>("countries")),
                        mt_groups = Classes.BrandsMaster.CRM.Utilities.GetMTGroupFromString(x.Field<string>("mt_groups")),
                        AutoConvertToRetention = x.Field<bool>("auto_convert_to_retention"),
                    }).ToList();
                }
            }

            return pools;
        }

        public void SetDynamicParameters(Dictionary<string, object> model, int offset, int limit)
        {
            this.DynaParams = new DynamicParameters();

            if (model == null)
                model = new Dictionary<string, object>();

            long? id = null;
            if (model.ContainsKey("Id") && !string.IsNullOrEmpty(model["Id"].ToString()))
                id = long.Parse(model["Id"].ToString());

            this.DynaParams.Add("@Id", id);
            this.DynaParams.Add("@name", model.ContainsKey("name") && !string.IsNullOrEmpty(model["name"].ToString()) ? model["name"].ToString() : null);
            this.DynaParams.Add("@referrals", model.ContainsKey("referrals") && !string.IsNullOrEmpty(model["referrals"].ToString()) ? "#"+model["referrals"].ToString() + "#" : null);
            this.DynaParams.Add("@country", model.ContainsKey("country") && !string.IsNullOrEmpty(model["country"].ToString()) ? model["country"].ToString() : null);
            this.DynaParams.Add("@status", model.ContainsKey("status") && !string.IsNullOrEmpty(model["status"].ToString()) ? model["status"].ToString() : null);
            this.DynaParams.Add("@agent", model.ContainsKey("agent") && !string.IsNullOrEmpty(model["agent"].ToString()) ? model["agent"].ToString() : null);
            this.DynaParams.Add("@auto_convert_to_retention", model.ContainsKey("P_AutoRet") && !string.IsNullOrEmpty(model["P_AutoRet"].ToString()) ? model["P_AutoRet"].ToString() : null);
            this.DynaParams.Add("@offset", offset);
            this.DynaParams.Add("@limit", limit);
        }

        public IEnumerable<Pool> GetPools()
        {
            int limit = this.DynaParams.Get<int>("@limit");
            

            string query = $"SELECT COUNT(id) OVER() as count, id, name, referrals, status, agent, countries as csv_Countries, mt_groups as csv_mt_groups,auto_convert_to_retention FROM sales.pools WHERE (id = @Id OR @Id IS NULL) AND (UPPER(name) LIKE UPPER(@name) OR @name IS NULL) AND (@referrals = any(regexp_split_to_array(referrals,',')) OR @referrals IS NULL) AND (@country = any(regexp_split_to_array(countries,',')) OR @country IS NULL) {this.OrderBy} OFFSET @offset LIMIT {(limit > 0 ? "@limit" : "ALL")}";

            IEnumerable<Pool> pools = null;

            using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                pools = conn.Query<Pool>(query, this.DynaParams).Select((pool) =>
                {
                    pool.Countries = Classes.BrandsMaster.CRM.Utilities.GetCountriesfromString(pool.Csv_Countries);
                    pool.mt_groups = Classes.BrandsMaster.CRM.Utilities.GetMTGroupFromString(pool.Csv_Mt_Groups);
                    return pool;

                }).ToList();
            };

            return pools;
        }

        public IEnumerable<Pool> SelectAllPool(string pools, string type)
        {

            pools = !string.IsNullOrEmpty(pools) ? pools : "null";
            string condition = "";
            if (type != "1" && type != "8" && type != "15")
            {
                 condition = $"WHERE name IN ({pools})";
            }

            string query = $"SELECT id, name FROM sales.pools {condition}";
            using(var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                var result = conn.Query<Pool>(query);

                return result;
            }
        }

        public int GetPoolsCount()
        {

            string query = $"SELECT COUNT(DISTINCT id) FROM sales.pools WHERE (id = @Id OR @Id IS NULL) AND (name = @name OR @name IS NULL) AND (@referrals = any(regexp_split_to_array(referrals,',')) OR @referrals IS NULL) AND (@country = any(regexp_split_to_array(countries,',')) OR @country IS NULL) ";
            int count;
            using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                count = conn.QuerySingle<int>(query, this.DynaParams);
            }
            return count;
        }

        public List<string> GetPoolCountries(string parameter, List<Classes.Country> listCountries = null)
        {
            List<string> countries = new List<string>();
            List<string> scountries = new List<string>();
            List<string> fcountries = new List<string>();
            string query = $"SELECT DISTINCT countries FROM sales.pools WHERE countries IS NOT NULL AND countries != '' {parameter}";
            using (var conn = DBconnections.CRM("CRM.Utilities.PoolUtilities", "GetPoolCountByQuery"))
            {
                DataTable dt = DBqueries.Select(conn, query);
                countries = dt.AsEnumerable().Select(x => x.Field<string>("countries")).ToList();

                if (listCountries != null)
                {
                    foreach (var cy in countries)
                    {
                        listCountries.ForEach((lc) => {
                            
                            if (cy.Contains(lc.Name) && fcountries.Find(sc => sc == lc.Name) == null)
                            {
                                fcountries.Add(lc.Name);
                            }
                        });
                    }
                }
                else
                {
                    foreach (var cy in countries)
                    {
                        if (cy.Contains(','))
                        {
                            scountries = cy.Split(',').ToList();
                            fcountries.AddRange(scountries);
                        }
                        else
                        {
                            fcountries.Add(cy);
                        }
                    }
                }
            }
            return fcountries;
        }
    }
}