using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace molello.classes {
  public class item_todo : item {

    protected todo_stato _stato;
    protected int? _perc;
    protected string _cosa;
    protected List<todo_item> _items;

    public int? perc { get { return _perc; } set { _perc = value; } }
    public todo_stato stato { get { return _stato; } }
    public string cosa { get { return _cosa; } set { _cosa = value; } }
    public List<todo_item> items { get { return _items; } }
    public todo_item add_item (todo_item i) { _items.Add(i); return i; }

    public item_todo (int id, string stato, string cosa, int? perc)
      : base(id, item_type.todo, item_display.block) {
      _stato = item.get_todo_stato(stato); _cosa = cosa; _perc = perc;
      _items = new List<todo_item>();
    }
    public item_todo (HtmlAgilityPack.HtmlNode node)
      : base(node, item_display.block) {
      HtmlAgilityPack.HtmlNode stato = node.SelectSingleNode("div[@i-subtype='todo-stato']");
      _stato = item.get_todo_stato(stato.Attributes["code-stato"].Value);
      _cosa = get_text_node(node.SelectNodes("div[@i-subtype='todo-cosa']")[0]);
      if (stato.Attributes["perc-stato"].Value != "")
        _perc = int.Parse(stato.Attributes["perc-stato"].Value);
      _items = new List<todo_item>();
      HtmlAgilityPack.HtmlNodeCollection nodes = node.SelectNodes("div[@i-subtype='todo-list']/div[@i-subtype='todo-item']");
      if (nodes != null) {
        foreach (HtmlAgilityPack.HtmlNode i in nodes) {
          string sigla = i.SelectSingleNode("*[@i-subtype='todo-item-st']").Attributes["code-stato"].Value.ToString()
            , cosa = i.SelectSingleNode("*[@i-subtype='todo-item-cosa']").InnerText;
          if (!string.IsNullOrEmpty(cosa)) _items.Add(new todo_item(new todo_stato(sigla), cosa));
        }
      }
      
    }

    public override string html_item () {
      int ip = 0;
      if(_perc.HasValue) {
        int v = _perc.Value;
        if (v >= 0 && v <= 5) ip = 0;
        else if (v >= 6 && v <= 20) ip = 1;
        else if (v >= 21 && v <= 40) ip = 2;
        else if (v >= 41 && v <= 55) ip = 3;
        else if (v >= 56 && v <= 70) ip = 4;
        else if (v >= 71 && v <= 94) ip = 5;
        else if (v >= 95 && v <= 100) ip = 6;
      }

      return html_base(this, string.Format(@"<div class='item-todo' onmouseleave='out_todo(this)' {0}>
        <div style='display: flex;position:relative;'>
          <div i-subtype='todo-stato' class='todo-state' style='background-color:{1}' code-stato='{2}' perc-stato='{5}' onclick='click_stato(this)'>
            <span i-subtype='title-stato' class='todo-sigla'>{6}</span></div>
          <div i-subtype='todo-cosa' class='todo-cosa' {3}>{4}</div>
          <div i-subtype='todo-perc' style='width:16;height:16;'>&nbsp;<img i-subtype='todo-stato-perc' src='{8}\circle{7}-16.png' class='todo-img-perc' onmousedown='click_perc(event, this)' /></div>
        </div>
        <div i-subtype='todo-list'>{9}</div></div>"
        , attrs_base(this.tp), _stato.color, _stato.stato, attrs_editable("TESTO TODO...", "i_keydown_td")
        , _cosa.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\r", "<br/>")
        , _perc.HasValue ? _perc.Value.ToString() : "", _stato.sigla, ip
        , app._core.config.get_var("vars.path-images").value, string.Join("", _items.Select(x => x.html_item()))));
    }

    public override void exec_insert (int node_id) {
      if (string.IsNullOrEmpty(_cosa)) return;
      System.Data.DataTable dt = base_dal.dt_qry("qry-nodes.add-item-todo", new Dictionary<string, object>() { { "node_id", node_id }
        , { "item_type", this.tp.ToString() }, { "item_stato", _stato.stato }, { "item_cosa", _cosa }
        , { "item_perc", _perc > 0 ? _perc.ToString() : "NULL" } });
      int item_id = Convert.ToInt32(dt.Rows[0][0]);
      foreach (todo_item i in _items) {
        base_dal.exec_qry("qry-nodes.add-todo-item", new Dictionary<string, object>() { { "item_id", item_id }
          , { "item_stato", i.stato.sigla }, { "item_cosa", i.cosa } } );
      }
    }
  }

  public class todo_stato {
    protected int _order; protected string _stato, _title, _color, _sigla;
    public string stato { get { return _stato; } }
    public string title { get { return _title; } }
    public string color { get { return _color; } }
    public string sigla { get { return _sigla; } }
    public int order { get { return _order; } }

    public todo_stato (string stato, string title, string sigla, string color, int order) {
      _stato = stato; _title = title; _sigla = sigla; _color = color; _order = order;
    }

    public todo_stato (string sigla) { _sigla = sigla; }
  }

  public class todo_item {
    protected todo_stato _stato = null;
    protected string _cosa = null;

    public todo_stato stato { get { return _stato; } }
    public string cosa { get { return _cosa; } }

    public todo_item (todo_stato st, string cosa) { _stato = st; _cosa = cosa; }

    public string img_from_sigla (string sigla) {
      if (sigla == "DF") return "circle0-16.png";
      else if (sigla == "F") return "circle-ft-16.png";
      else if (sigla == "S") return "circle-ic-16.png";
      else if (sigla == "IC") return "circle-sp-16.png";
      else throw new Exception("sigla '" + sigla + "' non prevista!");
    }

    public string html_item () {
      return string.Format(@"<div i-subtype='todo-item' class='todo-item'>
        <img i-subtype='todo-item-st' code-stato='{1}' src='{0}\{3}' class='todo-item-st' onclick='click_todo_sigla(this)'/>
        <div i-subtype='todo-item-cosa' class='todo-item-cosa' contenteditable='true'
         onkeydown='return i_keydown_td(this, event)' data-text='TODO...'>{2}</div></div>"
        , app._core.config.get_var("vars.path-images").value, _stato.sigla, _cosa, img_from_sigla (_stato.sigla));
    }

  }
}
