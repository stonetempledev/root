using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace molinafy {
  public class result {
    public enum code { cancel, ok, error }
    public code code_result { get; set; }
    public string code_result_str { get { return code_result.ToString(); } }
    public string description { get; set; }

    public result (code result) { this.code_result = result; }
    public result (code result, string des) { this.code_result = result; this.description = des; }
  }
}
