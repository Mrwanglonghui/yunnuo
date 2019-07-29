using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aipuer.Common
{
    public class Module
    {
        public Module()
        {
            this.id = 0;
            this.name = "";
            this.url = "";
            this.title = "";
            this.pid = 0;
            this.layer_id = 0;
            this.type = "";
            this.iClass = "";
            this.index = -1;
        }

        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public int pid { get; set; }
        public int layer_id { get; set; }
        public string type { get; set; }
        public string iClass { get; set; }
        public int index { get; set; }

    }

    public class User
    {
        public User()
        {
            this.id = 0;
            this.username = "";
            this.name = "";
            this.password = "";
            this.department = "";
            this.telephone = "";
            this.role_id = 0;
        }

        public int id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string department { get; set; }
        public string telephone { get; set; }
        public int role_id { get; set; }
    }

    public class Role
    {
        public Role()
        {
            this.id = 0;
            this.name = "";
            this.describe = "";
            this.module_ids = "";
            this.layer_ids = "";
        }

        public int id { get; set; }
        public string name { get; set; }
        public string describe { get; set; }
        public string module_ids { get; set; }
        public string layer_ids { get; set; }
    }

    public class Layer
    {
        public Layer()
        {
            this.id = 0;
            this.name = "";
            this.dataType = "";
            this.dataName = "";
            this.pointQuery = false;
            this.defaultOpen = false;
            this.shpColor = "";
            this.pid = 0;
            this.describe = "";
            this.files = "";
            this.index = -1;
        }

        public int id { get; set; }
        public string name { get; set; }
        public string dataType { get; set; }
        public string dataName { get; set; }
        public bool pointQuery { get; set; }
        public bool defaultOpen { get; set; }
        public string shpColor { get; set; }
        public string describe { get; set; }
        public string files { get; set; }
        public int pid { get; set; }
        public int index { get; set; }
    }




    public class Biaozhu
    {
        public Biaozhu()
        {
            this.id = 0;
            this.name = "";
            this.dataType = "";
            this.bzType = "";
            this.dataTime = "";
            this.coordinate = "";
            this.describe = "";
            this.length = "";
            this.area = "";
            this.filePath = "";
        }

        public int id { get; set; }
        public string name { get; set; }
        public string dataType { get; set; }
        public string bzType { get; set; }
        public string dataTime { get; set; }
        public string coordinate { get; set; }
        public string length { get; set; }
        public string describe { get; set; }
        public string area { get; set; }
        public string filePath { get; set; }
    }
}
