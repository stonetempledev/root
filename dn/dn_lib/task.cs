using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dn_lib {
  public class task {
    
    public long? file_id { get; set; }
    public long? folder_id { get; set; }
    public string title { get; set; }
    public string user { get; set; }
    public string stato { get; set; }

    public task(long? file_id, long? folder_id, string title, string user, string stato) {
      this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.stato = stato;
    }

  }
}
