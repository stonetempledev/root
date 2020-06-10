using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace dn_lib {
  public class task {
    public enum task_state { none, in_corso, sospeso, fatto, da_fare, incompleto, urgente }

    public long id { get; set; }
    public int synch_folder_id { get; set; }
    public long? file_id { get; set; }
    public long? folder_id { get; set; }
    public string title { get; set; }
    public string user { get; set; }
    public string stato { get; set; }
    public string cls { get; set; }
    public int order { get; set; }
    public task_state state { get; set; }
    public DateTime? dt_upd { get; set; }
    public string title_plurale { get; set; }
    public string title_singolare { get; set; }

    public task(int synch_folder_id, long id, long? file_id, long? folder_id, string title, string user
      , string stato, task_state state, DateTime? dt_upd, int order, string cls
      , string title_singolare, string title_plurale) {
      this.synch_folder_id = synch_folder_id; this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.stato = stato; this.state = state;
      this.dt_upd = dt_upd; this.order = order; this.cls = cls;
      this.title_plurale = title_plurale; this.title_singolare = title_singolare;
    }

    public task(int synch_folder_id, long? file_id, long? folder_id, string title, string user, string stato
      , DateTime? dt_upd) {
      this.synch_folder_id = synch_folder_id; this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.stato = stato;
      this.state = state; this.dt_upd = dt_upd;
    }

    /*
      elementi:
       <title>.<user>.<state>.<date>.task.*
     
      esempi:
     
      elimina prodotto.task
      elimina prodotto.task.*
      elimina prodotto.molina.task.*
      elimina prodotto.molina.in corso.task.*
      elimina prodotto.200531.task.*
      elimina prodotto.molina.200531.task.*
      elimina prodotto.molina.in corso.200531.task.*
     
     * */
    static public task parse_task(int synch_folder_id, string path, DateTime def_dt
      , List<string> users, Dictionary<string, string> states
      , long? folder_id = null, long? file_id = null) {
      try {
        if (!folder_id.HasValue && !file_id.HasValue) throw new Exception("il task dev'essere un file o un folder");

        string[] parts = Path.GetFileName(path).Split(new char[] { '.' });
        if (parts.Length <= 1) return null;

        // taks
        if (folder_id.HasValue && parts[parts.Length - 1] != "task") return null;
        else if (file_id.HasValue && parts[parts.Length - 1] != "task" && parts[parts.Length - 2] != "task") return null;

        // parse
        string title = "", user = "", state = "";
        DateTime? dt_task = null;
        for (int i = 0; i < parts.Length; i++) {
          string p = parts[i]; if (p == "task") break;

          // date
          if (mlib.tools.strings.is_int(p) && (p.Length == 6 || p.Length == 8)) {
            if (!dt_task.HasValue) parse_date(p, out dt_task); continue;
          }

          // state
          if (state == "" && states.ContainsKey(p.ToLower())) { state = states[p.ToLower()]; continue; }

          // user
          if (user == "" && users.FirstOrDefault(x => x.ToLower() == p.ToLower()) != null) { user = p; continue; }

          // title
          if (title == "") { title = p; continue; }

        }

        return new task(synch_folder_id, file_id, folder_id, title, user, state, dt_task.HasValue ? dt_task : def_dt);
      } catch { return null; }
    }

    static protected bool parse_date(string txt, out DateTime? dt) {
      dt = null;
      try {
        if (string.IsNullOrEmpty(txt) || !txt.All(char.IsDigit)) return false;
        if (txt.Length == 6) {
          dt = new DateTime(2000 + int.Parse(txt.Substring(0, 2)), int.Parse(txt.Substring(2, 2))
            , int.Parse(txt.Substring(4, 2))); return true;
        } else if (txt.Length == 8) {
          dt = new DateTime(int.Parse(txt.Substring(0, 4)), int.Parse(txt.Substring(4, 2)), int.Parse(txt.Substring(6, 2))); return true;
        }
      } catch { }
      return false;
    }


  }
}
