using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace dn_lib {
  public class synch_folder {

    public int id { get; set; }
    public string pc_name { get; set; }
    public string title { get; set; }
    public string des { get; set; }
    public string local_path { get; set; }
    public string http_path { get; set; }
    public string user { get; set; }
    public string password { get; set; }
    
    public synch_folder(int id, string pc_name, string title, string des, string local_path, string http_path, string user, string password) {
      this.id = id; this.pc_name = pc_name; this.title = title; this.des = des;
      this.local_path = local_path; this.http_path = http_path;  
      this.user = user; this.password = password;
    }

  }
}