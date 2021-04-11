using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn {
  public class object_type {

    int id { get; set; }
    public string type { get; set; }
    public string des { get; set; }
    public string list_des { get; set; }
    public string notes { get; set; }
    public string cmd_list { get; set; }
    public string cmd { get; set; }

    public object_type(int id, string type, string des, string list_des, string notes, string cmd_list, string cmd) {
      this.id = id; this.type = type; this.des = des; this.list_des = list_des; this.notes = notes; 
      this.cmd_list = cmd_list; this.cmd = cmd;
    }
  }
}