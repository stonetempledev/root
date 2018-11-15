using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deeper.db
{
  public class idx_table
  {
    protected string _tableName = "", _name = "";
    protected bool _clustered = false, _unique = false, _primary = false;
    protected List<idx_field> _fields = null;

    public idx_table(string tableName, string name, bool clustered, bool unique, bool primary) {
      _tableName = tableName; _name = name;
      _clustered = clustered; _unique = unique; _primary = primary;
      _fields = new List<idx_field>();
    }

    public idx_table(string tableName, string name, bool clustered, bool unique, bool primary, IEnumerable<idx_field> lst)
      : this(tableName, name, clustered, unique, primary) {
      _fields.AddRange(lst);
    }

    public string TableName { get { return _tableName; } set { _tableName = value; } }
    public string Name { get { return _name; } set { _name = value; } }
    public bool Clustered { get { return _clustered; } }
    public bool Unique { get { return _unique; } }
    public bool Primary { get { return _primary; } }
    public List<idx_field> Fields { get { return _fields; } }
    public idx_field existField(string fieldName) { return _fields.FirstOrDefault(x => string.Compare(x.Name, fieldName, true) == 0); }
    public string listFields() { return string.Join(",", _fields.Select(x => x.Name)); }
    public void addField(idx_field idxFld) { _fields.Add(idxFld); }

    static public idx_table find(List<idx_table> list, string indexName, string tableName) {
      return list.FirstOrDefault(x => string.Compare(x.Name, indexName, false) == 0
        && string.Compare(x.TableName, tableName, false) == 0);
    }

    static public idx_table findIndex(List<idx_table> list, idx_table idx) { return list.FirstOrDefault(x => sameIndex(x, idx)); }

    static public bool sameIndex(idx_table idx, idx_table idx2) {
      if (idx.Clustered != idx2.Clustered || idx.Unique != idx2.Unique || idx.Primary != idx2.Primary)
        return false;

      foreach (idx_field field in idx.Fields)
        if (idx2.existField(field.Name) == null)
          return false;

      return true;
    }

    public string desIndex() {
      return Name + " - " + (Unique ? "unique" : "no unique")
        + (Clustered ? ", clustered" : ", no clustered")
        + (Primary ? ", primary" : "") + "(" + string.Join(", ", Fields.Select(x => x.Name)) + ")";
    }

    static public bool filter_unique(bool? uniques, bool unique, bool primary) {
      return !uniques.HasValue || (uniques.HasValue && (uniques.Value && unique && !primary) || (!uniques.Value && !unique));
    }
  }
}

