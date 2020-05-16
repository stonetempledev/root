using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace toyn {
  public class objects : bo {

    public objects() {
    }

    #region object types

    public object_type get_object_type(string object_type) { return create_object_type(dr_object_type(object_type)); }

    protected DataRow dr_object_type(string object_type) {
      return db_conn.first_row(core.parse_query("objects.type"
        , new string[,] { { "user_id", user_id.ToString() }, { "object_type", object_type } }));
    }
    protected object_type create_object_type(DataRow dr) {
      return new object_type(db_provider.int_val(dr["object_type_id"]), db_provider.str_val(dr["object_type"]), db_provider.str_val(dr["object_des"])
        , db_provider.str_val(dr["object_list_des"]), db_provider.str_val(dr["object_notes"])
        , db_provider.str_val(dr["object_cmd_list"]), db_provider.str_val(dr["object_cmd"]));
    }

    #endregion

  }
}