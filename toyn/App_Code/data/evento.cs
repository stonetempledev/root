using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn
{
  public class evento
  {
    public int id { get; set; }
    public string title { get; set; }
    public string note { get; set; }
    public DateTime dt_da { get; set; }
    public DateTime dt_a { get; set; }
    public bool is_date_diverse { get { return dt_da != dt_a; } }
    public bool is_date_uguali { get { return dt_da == dt_a; } }
    public tipo_evento tp { get; set; }

    public evento(int id, string title, string note, DateTime dt_da, DateTime dt_a, tipo_evento tp)
    {
      this.id = id; this.title = title; this.note = note; this.dt_da = dt_da; this.dt_a = dt_a; this.tp = tp;
    }
  }
}