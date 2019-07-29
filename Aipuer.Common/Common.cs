using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aipuer.Common
{
    public class Common
    {
        private IDBHelper dbHelper = null;

        public Common()
        {
        }

        public Common(string connectionString)
        {
            dbHelper = new PostgreHelper(connectionString);
        }

        public string getPagerData(string tableName, string page, string size, Dictionary<string, List<string>> where_fieldsValue = null)
        {
            string result = "";
            string sql = "select * from public." + tableName;
            if (tableName == "sys_module" || tableName == "sys_layer")
            {
                sql = "select ta.*, tb.name as pName from public." + tableName + " as ta left join public." + tableName + " as tb on ta.pid = tb.id";
            }
            else if (tableName == "sys_user")
            {
                sql = "select ta.*, tb.name as roleName from public." + tableName + " as ta left join public.sys_role as tb on ta.role_id = tb.id";
            }
            else if (tableName == "sys_role")
            {
                sql = "select r.*,(select string_agg(trim(to_char(rm.module_id, '999999999999')), ',') from public.sys_role_module rm where rm.role_id = r.id) as moduleIds,(select string_agg(m.name,',') from public.sys_role_module rm, public.sys_module m where rm.role_id = r.id and m.id= rm.module_id) as moduleNames,(select string_agg(trim(to_char(rl.layer_id,'999999999999')),',') from public.sys_role_layer rl where rl.role_id = r.id) as layerIds,(select string_agg(l.name,',') from public.sys_role_layer rl, public.sys_layer l where rl.role_id = r.id and l.id= rl.layer_id) as layerNames from public." + tableName + " as r";
            }
            string sqlCount = "select count(*) from public." + tableName;
            string whereSql = "", whereSqlCount = "";

            //模糊查询
            if (where_fieldsValue != null && where_fieldsValue.Count == 1)
            {
                string keywords = "";
                List<string> queryFields = new List<string>();
                KeyValuePair<string, List<string>> first = where_fieldsValue.First();
                keywords = first.Key;
                queryFields = first.Value;
                for (int i = 0; i < queryFields.Count; i++)
                {
                    if (!string.IsNullOrEmpty(queryFields[i]))
                    {

                        if (i == 0)
                        {
                            whereSql += " and (" + (tableName == "sys_role" ? "r." : "ta.") + "\"" +  queryFields[i] + "\" like '%" + keywords + "%'";
                            whereSqlCount += " and (\"" +  queryFields[i] + "\" like '%" + keywords + "%'";
                        }
                        else
                        {
                            whereSql += " or " + (tableName == "sys_role" ? "r." : "ta.") + "\"" + queryFields[i] + "\" like '%" + keywords + "%'";
                            whereSqlCount += " or \"" + queryFields[i] + "\" like '%" + keywords + "%'";
                        }
                        if (i == queryFields.Count - 1)
                        {
                            whereSql += ")";
                            whereSqlCount += ")";
                        }
                    }
                }
            }
            try
            {
                int start = (Int32.Parse(page) - 1) * Int32.Parse(size);
                if (!string.IsNullOrEmpty(whereSql))
                {
                    sql += " where 1 = 1" + whereSql;
                    sqlCount += " where 1 = 1" + whereSqlCount;
                }
                sql += " order by id limit " + size + " offset " + start;

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                int count = dbHelper.GetCount(sqlCount);

                result = "{\"result\":\"success\",\"count\":\"" + count + "\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                //result = e.ToString();
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string getModulesAndLayersByUid()
        {
            string result = "";

            return result;
        }

        public string getById(string tableName, string id)
        {
            string result = "";
            string sql = "select * from public." + tableName + " where id = " + id;
            if (tableName == "sys_role")
            {
                sql = "select *,(select string_agg(trim(to_char(rm.module_id,'999999999999')),',') from public.sys_role_module rm where rm.role_id = r.id) as moduleIds,(select string_agg(trim(to_char(rl.layer_id,'999999999999')),',') from public.sys_role_layer rl where rl.role_id = r.id) as layerIds from public." + tableName + " as r where id = " + id;
            }
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                //result = e.ToString();
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }


        public string saveModule(Module model)
        {
            string sql = "insert into[sys_module]([name],[url],[layer_id],[title],[pid],[type],[iClass],[index]) values(@name, @url, @layer_id, @title, @pid, @type, @iClass, @index) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_module] set [name]=@name,[url]=@url,[layer_id]=@layer_id,[title]=@title,[pid]=@pid,[type]=@type,[iClass]=@iClass,[index]=@index where id=@id";
            }

            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@url", model.url));
            parameter.Add(new Npgsql.NpgsqlParameter("@layer_id", model.layer_id));
            parameter.Add(new Npgsql.NpgsqlParameter("@title", model.title));
            parameter.Add(new Npgsql.NpgsqlParameter("@pid", model.pid));
            parameter.Add(new Npgsql.NpgsqlParameter("@type", model.type));
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@iClass", model.iClass));
            parameter.Add(new Npgsql.NpgsqlParameter("@index", model.index));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveUser(User model)
        {
            string sql = "insert into[sys_user]([username],[name],[password],[department],[telephone],[role_id]) values(@username, @name, @password, @department, @telephone, @role_id) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_user] set [username]=@username,[name]=@name,[password]=@password,[department]=@department,[role_id]=@role_id,[telephone]=@telephone where id=@id";
            }

            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@username", model.username));
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@password", model.password));
            parameter.Add(new Npgsql.NpgsqlParameter("@department", model.department));
            parameter.Add(new Npgsql.NpgsqlParameter("@telephone", model.telephone));
            parameter.Add(new Npgsql.NpgsqlParameter("@role_id", model.role_id));
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveRole(Role model)
        {
            try
            {
                string sql = "insert into[sys_role]([name],[describe]) values(@name, @describe) RETURNING id;";
                if (model.id != 0)
                {
                    sql = "update [sys_role] set [name]=@name,[describe]=@describe where id=@id";
                }

                {
                    //子表，关联表
                    //先删除
                    string subSql = "delete from sys_role_module where role_id=" + model.id + ";delete from sys_role_layer where role_id=" + model.id;
                    dbHelper.ExecuteNonQuery(CommandType.Text, subSql);
                    //再插入
                    string moduleIds = model.module_ids, layerIds = model.layer_ids;
                    if (!string.IsNullOrEmpty(moduleIds))
                    {
                        string[] moduleIdArray = moduleIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        for (var i = 0; i < moduleIdArray.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(moduleIdArray[i]))
                            {
                                subSql = "insert into [sys_role_module]([role_id],[module_id]) values('" + model.id + "', '" + moduleIdArray[i] + "')";
                                dbHelper.ExecuteNonQuery(CommandType.Text, subSql);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(layerIds))
                    {
                        string[] layerIdArray = layerIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        for (var i = 0; i < layerIdArray.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(layerIdArray[i]))
                            {
                                subSql = "insert into [sys_role_layer]([role_id],[layer_id]) values('" + model.id + "', '" + layerIdArray[i] + "')";
                                dbHelper.ExecuteNonQuery(CommandType.Text, subSql);
                            }
                        }
                    }
                }

                List<DbParameter> parameter = new List<DbParameter>();
                parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
                parameter.Add(new Npgsql.NpgsqlParameter("@describe", model.describe));
                parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));

                string result = "";
                if (model.id == 0)
                {
                    result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
                }
                else
                {
                    dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                    result = model.id.ToString();
                }
                return result;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public string saveLayer(Layer model)
        {
            string sql = "insert into[sys_layer]([name],[dataType],[dataName],[pointQuery],[shpColor],[describe],[files],[pid],[index],[defaultOpen]) values(@name, @datatype, @dataname, @pointquery, @shpcolor, @describe, @files, @pid, @index, @defaultOpen) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_layer] set [name]=@name,[dataType]=@dataType,[dataName]=@dataName,[pointQuery]=@pointQuery,[shpColor]=@shpColor,[describe]=@describe,[files]=@files,[pid]=@pid,[index]=@index,[defaultOpen]=@defaultOpen where id=@id";
            }

            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@datatype", model.dataType));
            parameter.Add(new Npgsql.NpgsqlParameter("@dataname", model.dataName));
            parameter.Add(new Npgsql.NpgsqlParameter("@pointquery", model.pointQuery));
            parameter.Add(new Npgsql.NpgsqlParameter("@shpcolor", model.shpColor));
            parameter.Add(new Npgsql.NpgsqlParameter("@describe", model.describe));
            parameter.Add(new Npgsql.NpgsqlParameter("@files", model.files));
            parameter.Add(new Npgsql.NpgsqlParameter("@pid", model.pid));
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@index", model.index));
            parameter.Add(new Npgsql.NpgsqlParameter("@defaultOpen", model.defaultOpen));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string getByPid(string tableName, string pid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public." + tableName + " where pid = " + pid;

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string getRole()
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_role";

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string deleteById(string tableName, string id)
        {
            string result = "";
            string sql = "";
            try
            {
                string wherePlus = "";
                if (tableName == "sys_module" || tableName == "sys_layer")
                {
                    wherePlus = " or pid = " + id;
                }
                sql = "delete from public." + tableName + " where id = " + id;

                result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":\"" + result + "\"}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string getModuleData(string checkIds, string checkbox = null)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_module order by id";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (checkIds == null) checkIds = "";
                List<string> ids = checkIds.Split(',').ToList();

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadModuleData("0", dt, ids) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }
        
        public string getLayerData(string checkIds, string checkbox = null)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_layer order by id";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (checkIds == null) checkIds = "";
                List<string> ids = checkIds.Split(',').ToList();

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadLayerData("0", dt, ids, checkbox) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        public User getLoginUser(string username, string password)
        {
            string sql = "select * from public.sys_user where username = '" + username + "' and password = '" + password + "'";
            User model = null;
            try
            {
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    model = new User();
                    model.id = Int32.Parse(dt.Rows[0]["id"].ToString());
                    model.username = dt.Rows[0]["username"].ToString();
                    model.name = dt.Rows[0]["name"].ToString();
                }
            }
            catch (Exception e)
            {
            }
            return model;
        }

        public string getModulesByUid(int uid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select m.*,(select \"dataName\" from public.sys_layer l where m.layer_id=id limit 1) as layer, \"iClass\" as icon from public.sys_module m, public.sys_user u, public.sys_role_module rm where u.id=" + uid + " and rm.role_id=u.role_id and m.id=rm.module_id order by index, id";

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

            }
            catch (Exception e)
            {
                result = "\"error\"";
            }
            return result;
        }

        public string getLayersByUid(int uid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select l.* from public.sys_layer l, public.sys_user u, public.sys_role_layer rl where u.id=" + uid + " and rl.role_id=u.role_id and l.id=rl.layer_id order by index, id";

                //result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadLayerData("0", dt) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        private string loadModuleData(string pid, DataTable dt, List<string> ids = null)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.RowFilter = "pid=" + pid;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    string id = dv[i]["id"].ToString();
                    string _pid = dv[i]["pid"].ToString();
                    if (_pid == pid)
                    {
                        string children = loadModuleData(id, dt, ids);
                        var isChecked = "0";
                        if (ids != null && ids.IndexOf(id) != -1)
                        {
                            isChecked = "1";
                        }
                        result += "{";
                        result += "\"id\":\"" + id + "\",";
                        result += "\"title\":\"" + dv[i]["name"] + "\",";
                        result += "\"checkArr\":[{\"type\": \"0\", \"isChecked\": \"" + isChecked + "\"}],";
                        result += "\"parentId\":\"" + pid + "\"";
                        if (children != "")
                        {
                            result += ",\"isLast\":false";
                            result += ",\"children\":[" + children + "]";
                        }
                        {
                            DataView dvlast = new DataView();
                            dvlast.Table = dt;
                            dvlast.RowFilter = "pid=" + id;
                            if (dvlast.Count == 0 || object.Equals(dvlast, null))
                            {
                                result += ",\"basicData\":{\"url\":\"" + dv[i]["url"] + "\",\"layer_id\":\"" + dv[i]["layer_id"] + "\",\"type\":\"" + dv[i]["type"] + "\"}";
                                result += ",\"isLast\":true";
                            }
                        }
                        result += "}";
                        if (i != dv.Count - 1)
                        {
                            result += ",";
                        }
                    }
                }
            }
            return result;
        }

        private string loadLayerData(string pid, DataTable dt, List<string> ids = null, string checkbox = null)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.RowFilter = "pid=" + pid;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    string id = dv[i]["id"].ToString();
                    string _pid = dv[i]["pid"].ToString();
                    if (_pid == pid)
                    {
                        string children = loadLayerData(id, dt, ids, checkbox);
                        var isChecked = "0";
                        if(ids != null && ids.IndexOf(id) != -1)
                        {
                            isChecked = "1";
                        }
                        result += "{";
                        result += "\"id\":\"" + id + "\",";
                        result += "\"title\":\"" + dv[i]["name"] + "\",";
                        if (string.IsNullOrEmpty(checkbox))
                        {
                            result += "\"checkArr\":[{\"type\": \"0\", \"isChecked\": \"" + isChecked + "\"}],";
                        }
                        result += "\"parentId\":\"" + pid + "\"";
                        if (children != "")
                        {
                            result += ",\"isLast\":false";
                            result += ",\"children\":[" + children + "]";
                        }
                        {
                            DataView dvlast = new DataView();
                            dvlast.Table = dt;
                            dvlast.RowFilter = "pid=" + id;
                            if (dvlast.Count == 0 || object.Equals(dvlast, null))
                            {
                                result += ",\"basicData\":{\"name\":\"" + dv[i]["name"] + "\",\"dataName\":\"" + dv[i]["dataName"] + "\",\"dataType\":\"" + dv[i]["dataType"] + "\",\"pointQuery\":\"" + dv[i]["pointQuery"] + "\",\"shpColor\":\"" + dv[i]["shpColor"] + "\",\"defaultOpen\":\"" + dv[i]["defaultOpen"] + "\"}";
                                result += ",\"isLast\":true";
                            }
                        }
                        result += "}";
                        if (i != dv.Count - 1)
                        {
                            result += ",";
                        }
                    }
                }
            }
            return result;
        }

        public string queryData(string url)
        {
            StringBuilder res = new StringBuilder();

            StreamReader sr = null;
            try
            {
                WebRequest wr = WebRequest.Create(url);
                WebResponse wr_result = wr.GetResponse();
                Stream ReceiveStream = wr_result.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("UTF-8");
                sr = new StreamReader(ReceiveStream, encode);
                if (true)
                {
                    Char[] read = new Char[256];
                    int count = sr.Read(read, 0, 256);
                    while (count > 0)
                    {
                        string str = new string(read, 0, count);
                        res.Append(str);
                        count = sr.Read(read, 0, 256);
                    }
                }
            }
            catch (WebException we)
            {

            }
            finally
            {
                sr.Close();
            }

            return res.ToString();
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

        public string getBySql(string sql)
        {
            string result = "";
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, sql);
            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }


        public string getLayerByName(string name)
        {
            string result = "";
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, "select * from public.sys_layer where \"dataName\" = '" + name + "'");
                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"获取失败\"}";
            }
            return result;
        }



        public string saveBiaozhu(Biaozhu model)
        {
            string sql = "insert into[tool_biaozhu]([name],[dataType],[bzType],[describe],[length],[area],[coordinate],[filePath],[dateTime]) values(@name, @datatype, @bztype, @describe, @length, @area, @coordinate, @filePath,@dateTime) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [tool_biaozhu] set [name]=@name,[datatype]=@dataType,[bztype]=@bzType,[describe]=@describe,[length]=@length,[area]=@area,[coordinate]=@coordinate,[filePath]=@filePath,[dateTime]=@dateTime where id=@id";
            }
            
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@datatype", model.dataType));
            parameter.Add(new Npgsql.NpgsqlParameter("@bztype", model.bzType));
            parameter.Add(new Npgsql.NpgsqlParameter("@describe", model.describe));
            parameter.Add(new Npgsql.NpgsqlParameter("@datetime", model.dataTime));
            parameter.Add(new Npgsql.NpgsqlParameter("@length", model.length));
            parameter.Add(new Npgsql.NpgsqlParameter("@area", model.area));
            parameter.Add(new Npgsql.NpgsqlParameter("@coordinate", model.coordinate));
            parameter.Add(new Npgsql.NpgsqlParameter("@filePath", model.filePath));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }


        public string getBiaozhu(string id = null, string name = null, string bzType = null)
        {
            string result = "";
            try
            {
                string sql = "select * from public.tool_biaozhu where 1 = 1";
                if (!string.IsNullOrEmpty(id))
                {
                    sql += " and id = " + id;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    sql += " and name like '%" + name + "%'";
                }
                if (!string.IsNullOrEmpty(bzType))
                {
                    sql += " and \"bzType\" = '" + bzType + "'";
                }
                result = dbHelper.GetAsJson(CommandType.Text, sql);
                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"获取失败\"}";
            }
            return result;
        }

        public string deleteBiaozhuById(string id)
        {
            string result = "";
            try
            {
                string sql = "delete from public.tool_biaozhu where id = " + id;
                
                int temp = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                if (temp > 0)
                {
                    result = "{\"result\":\"success\",\"data\":\"" + result + "\"}";
                }
                else
                {
                    result = "{}";
                }
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"删除失败\"}";
            }
            return result;
        }


        public string getFirstValue(string url)
        {
            string result = queryDataPost(url);
            string returnstr = "";
            var jo = (JObject)JsonConvert.DeserializeObject(result);
            if (jo["result"].ToString() == "success")
            {
                var joo = (JArray)jo["data"];
                var index = 0;
                foreach (JObject items in joo)
                {
                    foreach (var item in items)
                    {
                        returnstr = item.Value.ToString();
                        break;
                    }
                    break;
                }
            }
            return returnstr;
        }

    }
}
