using System;
using System.Data;
using System.Data.Common;

namespace Aipuer.Common
{
    public interface IDBHelper
    {
        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        int ExecuteNonQuery(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 在事务中执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        int ExecuteNonQuery(System.Data.Common.DbTransaction trans, System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 在事务中执行查询，返回DataSet
        /// </summary>
        DataSet ExecuteQuery(System.Data.Common.DbTransaction trans, System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 执行查询，返回DataSet
        /// </summary>
        DataSet ExecuteQuery(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 在事务中执行查询，返回DataReader
        /// </summary>
        DbDataReader ExecuteReader(System.Data.Common.DbTransaction trans, System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 执行查询，返回DataReader
        /// </summary>
        DbDataReader ExecuteReader(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 在事务中执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        object ExecuteScalar(System.Data.Common.DbTransaction trans, System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        object ExecuteScalar(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 得到数据条数
        /// </summary>
        /// <param name="cmdText">sql</param>
        /// <returns>数据条数</returns>
        int GetCount(string cmdText);

        /// <summary>
        /// 返回json格式的查询结果
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        string GetAsJson(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 得到查询的第一行的第一列
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        string GetOneValue(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);

        /// <summary>
        /// 得到查询的第一行的第一列
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        DataTable GetDataTable(System.Data.CommandType cmdType, string cmdText, params System.Data.Common.DbParameter[] cmdParms);
    }

}
