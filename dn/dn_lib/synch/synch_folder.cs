using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace dn_lib {
  public class synch_folder {

    public int id { get; set; }
    public string title { get; set; }
    public string code { get; set; }
    public string des { get; set; }
    public string synch_path { get; set; }
    public string client_path { get; set; }
    public string user { get; set; }
    public string password { get; set; }

    public synch_folder(int id, string title, string code, string des, string synch_path, string client_path, string user, string password) {
      this.id = id; this.title = title; this.code = code; this.des = des;
      this.synch_path = synch_path; this.client_path = client_path;  this.user = user; this.password = password;
    }

  }
}