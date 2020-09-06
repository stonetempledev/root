using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Reflection;
using FastColoredTextBoxNS;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace sqleditor {
  public partial class frm_sql : Form {

    protected string _conn_string = "Server=localhost\\SQLEXPRESS;Database=deepanotes;User ID=sa;Password=Molina32!;";
    protected sql_conn _conn = null;

    public frm_sql() {
      InitializeComponent();
    }

    #region moving

    private Point? _p = null;

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) { _p = e.Location; }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e) {
      if (_p.HasValue) this.Location = new Point(e.X + this.Left - _p.Value.X, e.Y + this.Top - _p.Value.Y);
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e) { _p = null; }

    #endregion

    private void frm_sql_Load(object sender, EventArgs e) {
      try {
        dbl_buffer(dgv_data);
        txt_source.Language = Language.SQL;
        txt_source.HighlightingRangeType = HighlightingRangeType.VisibleRange;
        //txt_source.Text = "";
        txt_source.Text = File.ReadAllText("c:\\tmp\\test_2.sql");
        //txt_source.Text = File.ReadAllText("c:\\tmp\\script2.sql");
        txt_source.Focus();
        res_clear();
        refresh_caret();
      } catch (Exception ex) {
        MessageBox.Show(ex.Message);
      }
    }

    protected void dbl_buffer(DataGridView dgv) {
      typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.SetProperty, null, dgv, new object[] { true });
    }

    private void btn_close_Click(object sender, EventArgs e) { this.Close(); }

    private void txt_source_KeyDown(object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.F5) {
        txt_source.GetRanges("");
        string sel_text = txt_source.SelectedText;
        if (string.IsNullOrEmpty(sel_text)) sel_text = txt_source.Text;
        exec(sel_text.Trim());
      } else if (e.KeyCode == Keys.F1) {
        string html = @"<html>
<head>
<style>
body { font-family:segoe ui light; font-size:20pt; }
table { border-top:1pt solid lightgray; border-left:1pt solid lightgray; font-size:20pt; }
table td { border-bottom:1pt solid lightgray; border-right:1pt solid lightgray; }
table th { border-bottom:1pt solid lightgray; border-right:1pt solid lightgray; }
</style>
</head>
<body>
<h2 style='background-color:whitesmoke;'>sql Editor</h2>
<h3>shortcut keys</h3>
<p>
<table style='width:100%;'>
  <tr><th>F1</th><td>Guida</td></tr>
  <tr><th>F5</th><td>Esegui istruzione selezionata o intero script.</td></tr>
</table>
</p>
</body>
</html>";
        string fp = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".html");
        File.WriteAllText(fp, html);
        System.Diagnostics.Process.Start(fp);
      }
      refresh_caret();
    }

    protected void refresh_caret() {
      try {
        lbl_caret.Text = string.Format("line {0} - column {1}", txt_source.Selection.Start.iLine + 1
          , txt_source.Selection.Start.iChar + 1);
      } catch { lbl_caret.Text = ""; }
    }

    private void lbl_title_DoubleClick(object sender, EventArgs e) {
      if (this.WindowState == FormWindowState.Normal)
        this.WindowState = FormWindowState.Maximized;
      else if (this.WindowState == FormWindowState.Maximized)
        this.WindowState = FormWindowState.Normal;
    }

    protected void exec(string sql) {
      try {
        if (_conn != null) throw new Exception("c'è ancora una esecuzione in corso!");

        sql = sql.Trim();

        // init
        res_clear();
        dgv_data.DataSource = null;

        if (string.IsNullOrEmpty(sql)) return;

        // parse sql
        string[] errors;
        IScriptFragment script_fragment = parse(sql, SqlVersion.Sql100, true, out errors);
        if (errors != null) {
          foreach (string error in errors) res_warning(error);
          open_res();
          return;
        }

        TSqlScript tsql_fragment = script_fragment as TSqlScript;
        if (tsql_fragment == null) return;

        // esecuzione
        bool dati = false;
        foreach (TSqlBatch batch in tsql_fragment.Batches) {
          foreach (TSqlStatement s in batch.Statements) {

            if (_conn == null) { _conn = new sql_conn(); _conn.open_conn("System.Data.SqlClient", _conn_string); }

            string txt = sql.Substring(s.StartOffset, s.FragmentLength);
            // select
            if (s is Microsoft.Data.Schema.ScriptDom.Sql.SelectStatement) {
              DataTable dt = _conn.open_set(txt);
              dgv_data.DataSource = dt;
              dati = true;
              lbl_data.Text = string.Format("{0} records", dt.Rows.Count);
              res_line(s.GetType().ToString(), Color.Gray);
              res_line("  " + txt, Color.Gray);
              res_line(string.Format("{0} rows affected", dt.Rows.Count));
              res_line();
            }
              // esecuzione
            else {
              int rows = _conn.execute(txt);
              res_line(s.GetType().ToString(), Color.Gray);
              res_line("  " + txt, Color.Gray);
              res_line(string.Format("{0} rows affected", rows));
              res_line();
            }
          }
        }
        if (dati) open_dati(); else open_res();

      } catch (Exception ex) { res_error(ex.Message); open_res(); } finally { if (_conn != null) { _conn.close_conn(); _conn = null; } }
    }

    protected TSqlParser get_parser(SqlVersion level, bool quoted_identifiers) {
      switch (level) {
        case SqlVersion.Sql80: return new TSql80Parser(quoted_identifiers);
        case SqlVersion.Sql90: return new TSql90Parser(quoted_identifiers);
        case SqlVersion.Sql100: return new TSql100Parser(quoted_identifiers);
        case SqlVersion.SqlAzure: return new TSqlAzureParser(quoted_identifiers);
        default: throw new ArgumentOutOfRangeException("level");
      }
    }

    protected IScriptFragment parse(string sql, SqlVersion level, bool quoted_indentifiers, out string[] errors) {
      errors = null;
      if (string.IsNullOrWhiteSpace(sql)) return null;
      sql = sql.Trim();
      IScriptFragment script_fragment;
      IList<ParseError> error_list;
      using (var sr = new StringReader(sql)) {
        script_fragment = get_parser(level, quoted_indentifiers).Parse(sr, out error_list);
      }
      if (error_list != null && error_list.Count > 0) {
        errors = error_list.Select(e => string.Format("Column {0}, Identifier {1}, Line {2}, Offset {3}",
          e.Column, e.Identifier, e.Line, e.Offset) + Environment.NewLine + e.Message).ToArray();
        return null;
      }
      return script_fragment;
    }

    protected void open_dati() { tc_sql.SelectedTab = tab_dati; }
    protected void open_res() { tc_sql.SelectedTab = tab_res; }
    protected void res_clear() { rtb_res.Clear(); lbl_data.Text = ""; lbl_time.Text = ""; }
    protected void res_line(string txt = "", Color? color = null) {
      if (string.IsNullOrEmpty(txt)) txt = "";
      res_txt(txt + Environment.NewLine, color);
    }
    protected void res_error(string txt) { res_line("ERROR: " + txt, Color.Tomato); }
    protected void res_warning(string txt) { res_line("QUERY PARSE ERROR: " + txt, Color.DarkOrange); }
    protected void res_txt(string txt, Color? color = null) {
      if (string.IsNullOrEmpty(txt)) return;
      rtb_res.SelectionStart = rtb_res.TextLength;
      rtb_res.SelectionLength = 0;
      if (color.HasValue) rtb_res.SelectionColor = color.Value;
      rtb_res.AppendText(txt);
      rtb_res.SelectionColor = rtb_res.ForeColor;

    }

    private void txt_source_Click(object sender, EventArgs e) { refresh_caret(); }
  }
}

