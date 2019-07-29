<%@ WebHandler Language="C#" Class="GuanWangInfo" %>

using System;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.SessionState;




public class GuanWangInfo : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        PostgreHelper postgreHelper = new PostgreHelper();
        string flag = context.Request.QueryString["flag"];
            

        string gid = context.Request.QueryString["gid"];
        string louceng = context.Request.QueryString["louceng"];
        int gwid = Convert.ToInt32(context.Request.QueryString["gwid"]);
        int sbid = Convert.ToInt32(context.Request.QueryString["sbid"]);
        string wkt = context.Request.Form["_wkt"];
        string result = "";
        string sql = "";
        if(flag == "aa")
        {
            string visits=context.Request.QueryString["visits"];
            string visitsCity=context.Request.QueryString["city"];
            string visitsIP = context.Request.QueryString["ip"];
            
            result =""+visitsCity+","+visitsIP+","+ visits+"";
                context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.Write(result);
        }
        if (flag == "get_gwinfo_table")
        {
            if (louceng == "undefined")
            {
                int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
                int page = Convert.ToInt32(context.Request.QueryString["page"]);
                sql = $"select * from wangxianzong where gid= '{gid}'";
                var table = postgreHelper.GetDataTable(sql);
                int count = postgreHelper.GetCount($"select count(*) from wangxianzong where gid = '{gid}'");
                result = "{\"code\":0,\"msg\":\"\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(table) + "}";
                context.Response.Write(result);
            }
            else
            {
                int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
                int page = Convert.ToInt32(context.Request.QueryString["page"]);
                sql = $"select * from wangxianzong where 网络编号= '{gid}'and 层数='{louceng}'";
                var table = postgreHelper.GetDataTable(sql);
                int count = postgreHelper.GetCount($"select count(*) from wangxianzong where 网络编号 = '{gid}'and 层数='{louceng}'");
                result = "{\"code\":0,\"msg\":\"\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(table) + "}";
                context.Response.Write(result);

            }

        }
        if (flag == "get_sbinfo_table")
        {

            int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
            int page = Convert.ToInt32(context.Request.QueryString["page"]);
            sql = $"select * from jiaohuanjizong where OBJECTID= '{gid}'";
            var table = postgreHelper.GetDataTable(sql);
            int count = postgreHelper.GetCount($"select count(*) from jiaohuanjizong where OBJECTID = '{gid}'");
            result = "{\"code\":0,\"msg\":\"\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(table) + "}";
            context.Response.Write(result);
        }
        if (flag == "get_Gxinfo_table")
        {

            int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
            int page = Convert.ToInt32(context.Request.QueryString["page"]);
            sql = $"select * from ranqiline where guid= '{gid}'";
            var table = postgreHelper.GetDataTable(sql);
            int count = postgreHelper.GetCount($"select count(*) from ranqiline where guid = '{gid}'");
            result = "{\"code\":0,\"msg\":\"\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(table) + "}";
            context.Response.Write(result);
        }
        if (flag == "get_Gqinfo_table")
        {

            int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
            int page = Convert.ToInt32(context.Request.QueryString["page"]);
            sql = $"select * from guangqian where id= '{gid}'";
            var table = postgreHelper.GetDataTable(sql);
            int count = postgreHelper.GetCount($"select count(*) from guangqian where id = '{gid}'");
            result = "{\"code\":0,\"msg\":\"\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(table) + "}";
            context.Response.Write(result);
        }
        if (flag == "get_apinfo_table")
        {

            int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
            int page = Convert.ToInt32(context.Request.QueryString["page"]);
            sql = $"select * from ap where OBJECTID= '{gid}'";
            var table = postgreHelper.GetDataTable(sql);
            int count = postgreHelper.GetCount($"select count(*) from ap where OBJECTID = '{gid}'");
            result = "{\"code\":0,\"msg\":\"\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(table) + "}";
            context.Response.Write(result);
        }
        else if (flag == "get_gwinfo")
        {
            int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
            if(limit == 0)
            {
                limit = int.Parse(context.Request.Form["limit"]);
            }
            int page = Convert.ToInt32(context.Request.QueryString["page"]);
            if(page == 0)
            {
                page = int.Parse(context.Request.Form["page"]);
            }
            string whereKey = context.Request.Form["whereKey"];
            string whereValue = context.Request.Form["whereValue"];
            //sql = $"select st_astext(geom) as geom0, * from reticle where  ( 安放位置 like '%{keyword}%'or 服务范围 like '%{keyword}%' ) order by gid desc  limit {limit} offset {(page - 1) * limit}";
            //&where=( 安放位置 like '%{keyword}%'or 服务范围 like '%{keyword}%' )

            string url = "http://localhost:88/aimap/server/query?serverConnName=DBConnection&tableName=wangxianzong&whereValue=" + whereValue+"&whereKey=" + whereKey+"&page=" + page + "&limit=" + limit+ "&wkt=" + wkt;

            //sql = $"select * from tudi_guanli1 where  ( guanli_danwei like '%{keyword}%' or name like '%{keyword}%' or xiaoqu like '%{keyword}%' ) order by gid desc  limit {limit} offset {(page - 1) * limit}";
            var table = postgreHelper.queryDataPost(url);

            //var tableCount = eval("(" + table + ")");
            //JObject jo = (JObject)JsonConvert.DeserializeObject(table);
            int count = table.IndexOf("{",1);
            string newstr = table.Substring(0, count);
            string[] arr = newstr.Split(',');
            string aa = arr[1];
            string[] bb = aa.Split(':');
            string cc = bb[1].ToString();
            string ff = cc.Substring(1, cc.Length-1);
            string ll = ff.Substring(0,ff.Length-1);
            int rr = Convert.ToInt32(ll);
            result = "{\"code\":0,\"msg\":\"\",\"count\":" + rr + ",\"data\":" + table + "}";
            context.Response.Write(result);

        }
        else if (flag == "get_Sbinfo")
        {
            int limit = Convert.ToInt32(context.Request.QueryString["limit"]);
            if(limit == 0)
            {
                limit = int.Parse(context.Request.Form["limit"]);
            }
            int page = Convert.ToInt32(context.Request.QueryString["page"]);
            if(page == 0)
            {
                page = int.Parse(context.Request.Form["page"]);
            }
            string whereKey = context.Request.Form["whereKey"];
            string whereValue = context.Request.Form["whereValue"];
            //sql = $"select st_astext(geom) as geom0, * from reticle where  ( 安放位置 like '%{keyword}%'or 服务范围 like '%{keyword}%' ) order by gid desc  limit {limit} offset {(page - 1) * limit}";
            //&where=( 安放位置 like '%{keyword}%'or 服务范围 like '%{keyword}%' )

            string url = "http://localhost:88/aimap/server/query?serverConnName=DBConnection&tableName=jiaohuanjizong&whereValue=" + whereValue+"&whereKey=" + whereKey+"&page=" + page + "&limit=" + limit+ "&wkt=" + wkt;

            //sql = $"select * from tudi_guanli1 where  ( guanli_danwei like '%{keyword}%' or name like '%{keyword}%' or xiaoqu like '%{keyword}%' ) order by gid desc  limit {limit} offset {(page - 1) * limit}";
            var table = postgreHelper.queryDataPost(url);
            int count = table.IndexOf("{",1);
            string newstr = table.Substring(0, count);
            string[] arr = newstr.Split(',');
            string aa = arr[1];
            string[] bb = aa.Split(':');
            string cc = bb[1].ToString();
            string ff = cc.Substring(1, cc.Length-1);
            string ll = ff.Substring(0,ff.Length-1);
            int rr = Convert.ToInt32(ll);
            result = "{\"code\":0,\"msg\":\"\",\"count\":" + rr + ",\"data\":" + table + "}";
            context.Response.Write(result);

        }
        else if (flag == "seeGwInfo")
        {

            sql = $"select * from wangxianzong where gid='{gwid}'";
            var table = postgreHelper.GetDataTable(sql);
            context.Response.Write(JsonConvert.SerializeObject(table));
        }
        else if (flag == "seeSbInfo")
        {

            sql = $"select * from jiaohuanjizong where gid='{sbid}'";
            var table = postgreHelper.GetDataTable(sql);
            context.Response.Write(JsonConvert.SerializeObject(table));
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}