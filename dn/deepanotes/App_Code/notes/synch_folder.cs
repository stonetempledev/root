using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deepanotes {
  public class synch_folder {

    public int synch_folder_id { get; set; }
    public string title { get; set; }
    public string des { get; set; }
    public string http_path { get; set; }

    public List<folder> folders { get; protected set; }
    public folder add_folder(folder f) { this.folders.Add(f); return f; }

    public synch_folder(int synch_folder_id, string title, string des, string http_path) {
      this.synch_folder_id = synch_folder_id; this.title = title; this.des = des; this.http_path = http_path;
      this.folders = new List<folder>();
    }

    public folder get_folder(int folder_id) {
      folder res = this.folders.FirstOrDefault(f => f.folder_id == folder_id);
      if (res != null) return res;
      foreach (folder f in folders) {
        res = f.get_folder(folder_id);
        if (res != null) return res;
      }
      return null;
    }
  }
}