using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn
{
  public class tipo_evento
  {
    public int id { get; set; }
    public string title { get; set; }
    public string note { get; set; }

    public tipo_evento(int id, string title, string note)
    {
      this.id = id; this.title = title; this.note = note;
    }
  }
}