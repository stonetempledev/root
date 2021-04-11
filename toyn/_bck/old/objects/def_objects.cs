using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using mlib.db;

namespace toyn {
  public class def_objects : bo {

    public def_objects() {
    }

    public List<string> init_ot(string obj_type) {
      List<string> res = new List<string>();

      // vista vwo_*
      StringBuilder sb = new StringBuilder();
      string view = "vwo_" + obj_type;
      res.Add("vista;" + view);
      DataTable attrs = db_conn.dt_table(core.parse_query("def_objects.attributes", new string[,] { { "object_type", obj_type } }));

      // fields attributes
      string fields = string.Join(", ", attrs.Rows.Cast<DataRow>().Select(r => string.Format("a_{0}.[value] as {2}{1}"
        , db_provider.int_val(r["attribute_id"]), db_provider.str_val(r["attribute_code"])
        , db_provider.str_val(r["attribute_type"]) != "object" ? "v_" : "id_")));

      // joins attributes
      string joins = string.Join("\n ", attrs.Rows.Cast<DataRow>().Select(r => string.Format("left join objects_attrs_{1} a_{0} on a_{0}.object_id = o.object_id and a_{0}.attribute_id = {0}"
        , db_provider.int_val(r["attribute_id"]), db_provider.str_val(r["attribute_type"])))); ;

      sb.AppendFormat(@"if exists(select 1 from sys.views where name = '{0}' and type='v')
        DROP VIEW [{0}];
go
      CREATE VIEW [{0}]
      AS
       select o.object_id, o.object_type_id, o.object_code, o.object_title, o.dt_ins, o.dt_upd, o.user_id
          {2}
         from [objects] o
         join objects_types ot on ot.object_type = '{1}' and ot.object_type_id = o.object_type_id
         {3};
go
", view, obj_type, (!string.IsNullOrEmpty(fields) ? ", " : "") + fields, joins);
      db_conn.exec_script(sb.ToString());

      return res;
    }


  }
}