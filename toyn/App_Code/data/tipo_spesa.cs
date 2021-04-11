using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn
{
  public class tipo_spesa
  {
    public int id { get; set; }
    public string title { get; set; }
    public string note { get; set; }
    public bool entrata { get; set; }

    public tipo_spesa(int id, string title, string note, bool entrata)
    {
      this.id = id; this.title = title; this.note = note; this.entrata = entrata;
    }
  }
}