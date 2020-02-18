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
            var p = new DynamicParameters();
            p.Add("@PoolId", id);
            p.Add("@Status", status);
            p.Add("@AgentId", agent);
            p.Add("@Context", context);
            p.Add("@CreatedDate", DateTime.UtcNow);
            string query = $"INSERT INTO sales.status_default_user (id, pool_id, status, agent_id, context, created_date) VALUES(default,@PoolId, @Status, @AgentId, @Context, @CreatedDate)";
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

        public string UpdateStatusDefaultAgentDB(string id, string status, string agent, string context)
        {            
            DynamicParameters p = new DynamicParameters();
            p.Add("@Id", Convert.ToInt32(id));
            p.Add("@Status", status);
            p.Add("@AgentId", agent);
            p.Add("@Context", context);
            p.Add("@UpdatedDate", DateTime.UtcNow);
            string query = "UPDATE sales.status_default_user SET status=@Status, context=@Context, agent_id=@AgentId, updated_date=@UpdatedDate WHERE id=@Id";
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
            string query = $"SELECT id, pool_id, status, context, agent_id FROM sales.status_default_user WHERE pool_id='{id}'";
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
                            AgentId = x.Field<string>("agent_id"),
                        };                        
                    }).Where(es => es != null).ToList();
                }
            };

            return new KeyValuePair<List<StatusDefaultAgentModels>, string>(statusDefaults, null);
        }

        public KeyValuePair<List<ListOfStatusModel>, string> FetchStatusforDefaultsDB(string id)
        {
            string query = $"SELECT s.id, s.name FROM sales.status s LEFT JOIN sales.status_default_user sdu ON s.id != sdu.status_id WHERE sdu.pool_id='{id}'";
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
                            Id = x.Field<int>("id"),
                            Name = x.Field<string>("name"),
                        };
                    }).Where(es => es != null).ToList();
                }
            };

            return new KeyValuePair<List<ListOfStatusModel>, string>(statuses, null);
        }
    }
}