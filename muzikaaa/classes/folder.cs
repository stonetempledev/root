using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace muzikaaa.classes
{
    public class folder
    {
        public enum folder_type { cestone }

        protected int _id;
        protected string _path, _des, _name;
        protected folder_type _type;
        protected bool _active;

        public int id { get { return _id; } set { _id = value; } }
        public string path { get { return _path; } set { _path = value; } }
        public string des { get { return _des; } set { _des = value; } }
        public string name { get { return _name; } set { _name = value; } }
        public folder_type type { get { return _type; } }
        public bool active { get { return _active; } set { _active = value; } }

        public folder(int id, string path, string name, string des, bool active, folder_type type)
        { _id = id; _path = path; _des = des; _name = name; _type = type; _active = active; }
    }
}
