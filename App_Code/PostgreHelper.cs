using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

/// <summary>
/// PostgreHelper 的摘要说明
/// </summary>
public class PostgreHelper
{
    private string connectionString;
    private string connString = System.Configuration.ConfigurationManager.ConnectionStrings["postconnection"].ToString();

    public PostgreHelper()
    {
        this.connectionString = connString;
    }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString
    {
        get { return this.connectionString; }
    }
    /// <summary>
    /// 得到数据条数
    /// 通过返回第一个行数据的第一列的值来返回count,
    /// 所以sql要查询count(*)
    /// </summary>
    public int GetCount(string cmdText)
    {
        StringBuilder sql = new StringBuilder(cmdText);
        object count = ExecuteScalar(CommandType.Text, sql.ToString(), null);
        return int.Parse(count!=null? count.ToString():"0");
    }

    /// <summary>
    /// 执行查询，返回DataSet
    /// </summary>
    public DataSet ExecuteQuery(CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                    return ds;
                }
            }
        }
    }


    /// <summary>
    /// 获取单个字段
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public string GetOneValue(string sql)
    {
        using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                object obj = cmd.ExecuteScalar();
                cmd.Prepare();
                return obj != null ? obj.ToString() : string.Empty;
            }
        }
    }

    public DataTable GetDataTable(string sql)
    {
        try
        {
            DataTable dt;
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dt = new DataTable();
                        dt.Load(dataReader);
                        dataReader.Close();
                    }

                }
                return dt;

            }
        }
        catch (Exception e)
        {
            throw;
        }

    }



    public string queryDataPost(string urlstr)
    {
        string url = urlstr.Split('?')[0];
        string args = urlstr.Split('?')[1];
        HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
        Encoding encoding = Encoding.UTF8;
        byte[] bs = Encoding.ASCII.GetBytes(args);
        string responseData = String.Empty;
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";
        req.ContentLength = bs.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(bs, 0, bs.Length);
            reqStream.Close();
        }
        using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                responseData = reader.ReadToEnd().ToString();
            }
        }
        return responseData;
    }

    /// <summary>
    /// 在事务中执行查询，返回DataSet
    /// </summary>
    public DataSet ExecuteQuery(DbTransaction trans, CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();
        PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
        NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        da.Fill(ds, "ds");
        cmd.Parameters.Clear();
        return ds;
    }

    /// <summary>
    /// 执行 Transact-SQL 语句并返回受影响的行数。
    /// </summary>
    public int ExecuteNonQuery(CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();

        using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
    }

    /// <summary>
    /// 事务执行多条sql
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="cmdType"></param>
    /// <param name="cmdText"></param>
    /// <param name="cmdParms"></param>
    /// <returns></returns>
    public int ExecuteNonQuery_All(DbTransaction trans, CommandType cmdType, string[] cmdTextList,
            params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();
        int count = 0;
        for (int i=0;i<cmdTextList.Count();i++)
        {
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdTextList[i], cmdParms);
            count+= cmd.ExecuteNonQuery();
        }
        return count;
    }


    /// <summary>
    /// 在事务中执行 Transact-SQL 语句并返回受影响的行数。
    /// </summary>
    public int ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();
        PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
        int val = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    /// 执行查询，返回DataReader
    /// </summary>
    public DbDataReader ExecuteReader(CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();
        NpgsqlConnection conn = new NpgsqlConnection(connectionString);

        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            NpgsqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch
        {
            conn.Close();
            throw;
        }
    }

    

    /// <summary>
    /// 在事务中执行查询，返回DataReader
    /// </summary>
    public DbDataReader ExecuteReader(DbTransaction trans, CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();
        PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
        NpgsqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        cmd.Parameters.Clear();
        return rdr;
    }

    /// <summary>
    /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
    /// </summary>
    public object ExecuteScalar(CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();

        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            PrepareCommand(cmd, connection, null, cmdType, cmdText, cmdParms);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
    }

    /// <summary>
    /// 在事务中执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
    /// </summary>
    public object ExecuteScalar(DbTransaction trans, CommandType cmdType, string cmdText,
        params DbParameter[] cmdParms)
    {
        NpgsqlCommand cmd = new NpgsqlCommand();
        PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
        object val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    /// 生成要执行的命令
    /// </summary>
    /// <remarks>参数的格式：冒号+参数名</remarks>
    private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType,
        string cmdText, DbParameter[] cmdParms)
    {
        try
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText.Replace("@", ":").Replace("?", ":").Replace("[", "\"").Replace("]", "\"");

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (NpgsqlParameter parm in cmdParms)
                {
                    parm.ParameterName = parm.ParameterName.Replace("@", ":").Replace("?", ":");

                    cmd.Parameters.Add(parm);
                }
            }
        }
        catch (Exception e)
        {

        }
    }


    /// <summary>
    /// 返回json格式的查询结果
    /// </summary>
    /// <param name="cmdType"></param>
    /// <param name="cmdText"></param>
    /// <param name="cmdParms"></param>
    /// <returns></returns>
    public string GetAsJson(CommandType cmdType, string cmdText, string geomotryFieldName = null, params DbParameter[] cmdParms)
    {
        string JsonString = string.Empty;
        DataSet ds = ExecuteQuery(cmdType, cmdText, cmdParms);
        if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
        {
            //var a = DataTable2Json(ds.Tables[0], geomotryFieldName);
            //JsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            //JsonString = DataTable2Json(ds.Tables[0], geomotryFieldName);
            JsonString = JsonConvert.SerializeObject(ds);
        }
        return JsonString;
    }

    private string DataTable2Json(DataTable dt, string geomotryFieldName = null)
    {
        if (dt == null || dt.Rows.Count == 0)
        {
            return "[]";
        }
        if (string.IsNullOrEmpty(geomotryFieldName))
        {
            geomotryFieldName = "geom";
        }
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("[");
        for (int i = dt.Rows.Count - 1; i >= 0; i--)
        {
            jsonBuilder.Append("{\"type\":\"Feature\",\"properties\":{");
            var geomJson = "";
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                if (dt.Columns[j].ColumnName.ToLower() == geomotryFieldName.ToLower())
                {
                    geomJson = dt.Rows[i][j].ToString();
                    continue;
                }
                jsonBuilder.Append("\"");
                jsonBuilder.Append(dt.Columns[j].ColumnName);
                jsonBuilder.Append("\":\"");
                jsonBuilder.Append(dt.Rows[i][j].ToString());
                jsonBuilder.Append("\",");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("},\"geometry\":" + geomJson + "},");
        }
        jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
        jsonBuilder.Append("]");
        return jsonBuilder.ToString();
    }

}
