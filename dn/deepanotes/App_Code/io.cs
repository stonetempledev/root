using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using dn_lib.db;
using dn_lib;

namespace deepanotes {
  public class io : bo {

    public io() {
    }

    public string file_path(int file_id)
    {
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.file-path", new string[,] { { "file_id", file_id.ToString() } }));
      if(r == null) throw new Exception("il file " + file_id.ToString() + " non esiste!");
      return db_provider.str_val(r["file_path"]);
    }
  }
}