using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1 {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e) {
      System.Net.HttpWebResponse response = null; StreamReader r_str = null;
      try {
        Dictionary<string, object> pars = new Dictionary<string, object>() { { "qry", "select t.*, e.entity_id from (select sku, store, jsondata, created_at, updated_at, 'anag_aggiornam' as tb_name, status, TIMESTAMPDIFF(minute, created_at, now()) as cr_min from magentobuffer.anag_aggiornam where status <> 1               union select sku, store, jsondata, created_at, updated_at, 'anag_creaz' as tb_name, status, TIMESTAMPDIFF(minute, created_at, now()) as cr_min from magentobuffer.anag_creaz where status <> 1               union select sku, store, jsondata, created_at, updated_at, 'prezzi_aggiornam' as tb_name, status, TIMESTAMPDIFF(minute, created_at, now()) as cr_min from magentobuffer.prezzi_aggiornam where status <> 1               union select sku, store, jsondata, created_at, updated_at, 'qta_aggiornam' as tb_name, status, TIMESTAMPDIFF(minute, created_at, now()) as cr_min from magentobuffer.qta_aggiornam where status <> 1                       ) t              left join magento2_db.catalog_product_entity e on e.sku = t.sku              where t.cr_min > 30                and not (t.status = 4 and e.entity_id is null and t.tb_name in ('anag_aggiornam', 'prezzi_aggiornam', 'qta_aggiornam'))              order by t.created_at limit 50" }
          , { "del", "^" } };
        string url = "https://www.dipacommerce.com/ds-index.php?cmd=sel_qry&key=16N9K3WBZLUQZGLZJ939F9534B8040C57DFC2C7A154C7BE7E106B4086280EE1101B33585D0D0F8CE3E" 
          + (pars != null ? string.Join("", pars.Select(kp => string.Format("&{0}={1}", kp.Key, Uri.EscapeUriString(kp.Value != null ? kp.Value.ToString() : "")))) : "");

        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        request.Timeout = 240000;
        response = (System.Net.HttpWebResponse)request.GetResponse();
        Stream str = response.GetResponseStream();
        r_str = new StreamReader(str, Encoding.UTF8);
        string res = r_str.ReadToEnd();
      } finally { if (response != null) response.Close(); if (r_str != null) r_str.Close(); }
    }
  }
}
