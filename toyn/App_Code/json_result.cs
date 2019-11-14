using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for json_result
/// </summary>
public class json_result {
  public enum type_result { none, ok, error };
  public type_result result { get; set; }
  public string des { get; set; }
  public json_result(type_result tr, string des = "") {
    this.result = tr; this.des = des;
  }
}