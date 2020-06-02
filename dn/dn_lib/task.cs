using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dn_lib {
  public class task {
    public enum task_state { none, in_corso, sospeso, fatto, da_fare, incompleto, urgente }
    
    public long? file_id { get; set; }
    public long? folder_id { get; set; }
    public string title { get; set; }
    public string user { get; set; }
    public string stato { get; set; }
    public task_state state { get; set; }
    public DateTime? dt_upd { get; set; }

    public task(long? file_id, long? folder_id, string title, string user, string stato, DateTime? dt_upd) {
      this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.stato = stato; this.dt_upd = dt_upd;
    }

    public task(long? file_id, long? folder_id, string title, string user, string stato, task_state state, DateTime? dt_upd) {
      this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.stato = stato;
      this.state = state; this.dt_upd = dt_upd;
    }
  }
}
