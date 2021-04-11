using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn
{
  public class spesa
  {
    public int id { get; set; }
    public string cosa { get; set; }
    public int? qta { get; set; }
    public DateTime dt_spesa { get; set; }
    public decimal prezzo { get; set; }
    public decimal totale { get; set; }
    public tipo_spesa tp { get; set; }
    public evento evento { get; set; }

    public spesa(int id, string cosa, int? qta, DateTime dt_spesa, decimal prezzo, decimal totale, tipo_spesa tp, evento e = null)
    {
      this.id = id; this.cosa = cosa; this.qta = qta; this.dt_spesa = dt_spesa; this.prezzo = prezzo; this.totale = totale; this.tp = tp; this.evento = e;
    }
  }
}