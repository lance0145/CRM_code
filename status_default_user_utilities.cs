using CRM.Models;
using Classes.BrandsMaster.ConnectionString;
using Classes.BrandsMaster.CRM.Builders;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using static Classes.BrandsMaster.CRM.Builders.Client;
using System.Transactions;


namespace CRM.Utilities
{
    public class StatusDefaultAgentUtilities
    {
        public int DeleteStatusDefaultAgentDB(int id)
        {
            var p = new DynamicParameters();
            p.Add("@Id", id);
            string query = "DELETE FROM sales.status_default_user WHERE id = @Id";
            int dbReturn = 0;
            try
            {
                using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
                {
                    dbReturn = conn.Execute(query, p);
                }
                return dbReturn;
            }
            catch (Exception ex)
            {
                return dbReturn;
            }
        }
        public string CreateStatusDefaultAgentDB(string id, string status, string agent, string context)
        {
            string pool = string.Empty;
            string qp = $"SELECT name FROM sales.pools WHERE id='{id}'";
            using (var cp = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                var dt = Classes.BrandsMaster.DBqueries.Select(cp, qp);
                if (dt.Rows.Count > 0)
                {
                    pool = dt.Rows[0]["name"].ToString();
                }
            }

            string user = string.Empty;
            string qa = $"SELECT username FROM admin.users WHERE id='{agent}'";
            using (var ca = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                var dt = Classes.BrandsMaster.DBqueries.Select(ca, qa);
                if (dt.Rows.Count > 0)
                {
                    user = dt.Rows[0]["username"].ToString();
                }
            }

            var p = new DynamicParameters();
            p.Add("@Status", status);
            p.Add("@AgentId", agent);
            p.Add("@Context", context);
            p.Add("@CreatedDate", DateTime.UtcNow);
            string query = $"INSERT INTO sales.status_default_user (id, pool_id, pool, status, agent_id, agent, context, created_date) VALUES(default,'{id}', '{pool}', @Status, @AgentId, '{user}',@Context, @CreatedDate)";
            int dbReturn = 0;
            try
            {
                using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
                {
                    dbReturn = conn.Execute(query, p);
                }
                return dbReturn.ToString();
            }
            catch (Exception ex)
            {
                return dbReturn.ToString();
            }
        }        

        public KeyValuePair<List<StatusDefaultAgentModels>, string> ReadStatusDefaultAgentDB(string id)
        {            
            string query = $"SELECT id, pool_id, status, context, agent, agent_id FROM sales.status_default_user WHERE pool_id='{id}'";
            DataTable dt = new DataTable();
            List<StatusDefaultAgentModels> statusDefaults = new List<StatusDefaultAgentModels>();
            using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                dt = Classes.DB.Select(conn, query);

                if (dt?.Rows.Count > 0)
                {
                    statusDefaults = dt.AsEnumerable().Select(x => {
                        return new StatusDefaultAgentModels()
                        {
                            Id = x.Field<int>("id"),
                            PoolId = x.Field<string>("pool_id"),
                            Status = x.Field<string>("status"),
                            Context = x.Field<string>("context"),
                            Agent = x.Field<string>("agent"),
                            AgentId = x.Field<string>("agent_id"),
                        };                        
                    }).Where(es => es != null).ToList();
                }
            };

            return new KeyValuePair<List<StatusDefaultAgentModels>, string>(statusDefaults, null);
        }      

        public string UpdateStatusDefaultAgentDB(string id, string status, string agent, string context)
        {
            string user = string.Empty;
            string qa = $"SELECT username FROM admin.users WHERE id='{agent}'";
            using (var ca = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                var dt = Classes.BrandsMaster.DBqueries.Select(ca, qa);
                if (dt.Rows.Count > 0)
                {
                    user = dt.Rows[0]["username"].ToString();
                }
            }

            DynamicParameters p = new DynamicParameters();
            p.Add("@Id", Convert.ToInt32(id));
            p.Add("@Status", status);
            p.Add("@Context", context);
            p.Add("@AgentId", agent);
            p.Add("@UpdatedDate", DateTime.UtcNow);
            string query = $"UPDATE sales.status_default_user SET status=@Status, context=@Context, agent_id=@AgentId, agent='{user}', updated_date=@UpdatedDate WHERE id=@Id";
            int dbReturn = 0;
            try
            {
                using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
                {
                    dbReturn = conn.Execute(query, p);
                }
                return dbReturn.ToString();
            }
            catch (Exception ex)
            {
                return dbReturn.ToString();
            }
        }

        public string CheckStatusDefaultAgentbyPoolDB(string id, string status, string pool)
        {
            string result = string.Empty;
            string query = $"SELECT agent_id FROM sales.status_default_user WHERE pool='{pool}' AND status='{status}' AND agent_id != '{id}'";
            using (var con = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                var dt = Classes.BrandsMaster.DBqueries.Select(con, query);
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0]["agent_id"].ToString();
                }
            }
            return result;
        }

        //to follow
        public KeyValuePair<List<ListOfStatusModel>, string> FetchStatusforDefaultAgentDB(string id)
        {
            string query = $"SELECT DISTINCT s.name FROM sales.status s WHERE s.name NOT IN(SELECT sdu.status FROM sales.status_default_user sdu WHERE sdu.pool_id='{id}')";
            DataTable dt = new DataTable();
            List<ListOfStatusModel> statuses = new List<ListOfStatusModel>();
            using (var conn = Classes.BrandsMaster.DBconnections.CRM("", ""))
            {
                dt = Classes.DB.Select(conn, query);

                if (dt?.Rows.Count > 0)
                {
                    statuses = dt.AsEnumerable().Select(x =>
                    {
                        return new ListOfStatusModel()
                        {
                            Name = x.Field<string>("name"),
                        };
                    }).Where(es => es != null).ToList();
                }
            };

            return new KeyValuePair<List<ListOfStatusModel>, string>(statuses, null);
        }
    }
}