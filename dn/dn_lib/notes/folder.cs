using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib {
  public class folder {
    public int synch_folder_id { get; set; }
    public int folder_id { get; set; }
    public int? parent_id { get; set; }
    public string folder_name { get; set; }
    public task task { get; set; }

    public List<folder> folders { get; protected set; }
    public folder add_folder(folder f) { this.folders.Add(f); return f; }

    public List<file> files { get; protected set; }
    public file add_file(file f) { this.files.Add(f); return f; }

    public folder(int synch_folder_id, int folder_id, int? parent_id, string folder_name) {
      this.synch_folder_id = synch_folder_id; this.folder_id = folder_id;
      this.parent_id = parent_id; this.folder_name = folder_name;
      this.folders = new List<folder>();
      this.files = new List<file>();
    }

    public folder get_folder(long folder_id) {
      folder res = this.folders.FirstOrDefault(f => f.folder_id == folder_id);
      if(res != null) return res;
      foreach (folder f in folders) {
        res = f.get_folder(folder_id);
        if (res != null) return res;
      }
      return null;
    }

    public file get_file(long file_id) {
      file res = this.files.FirstOrDefault(f => f.id == file_id);
      if (res != null) return res;
      foreach (folder f in folders) {
        res = f.get_file(file_id);
        if (res != null) return res;
      }
      return null;
    }

  }
}