
namespace sqleditor {
  partial class frm_sql {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_sql));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      this.txt_source = new FastColoredTextBoxNS.FastColoredTextBox();
      this.lbl_title = new System.Windows.Forms.Label();
      this.btn_close = new System.Windows.Forms.Button();
      this.split_sql = new System.Windows.Forms.SplitContainer();
      this.tc_sql = new System.Windows.Forms.TabControl();
      this.tab_res = new System.Windows.Forms.TabPage();
      this.rtb_res = new System.Windows.Forms.RichTextBox();
      this.tab_dati = new System.Windows.Forms.TabPage();
      this.dgv_data = new System.Windows.Forms.DataGridView();
      this.lbl_time = new System.Windows.Forms.Label();
      this.lbl_caret = new System.Windows.Forms.Label();
      this.lbl_data = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.txt_source)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.split_sql)).BeginInit();
      this.split_sql.Panel1.SuspendLayout();
      this.split_sql.Panel2.SuspendLayout();
      this.split_sql.SuspendLayout();
      this.tc_sql.SuspendLayout();
      this.tab_res.SuspendLayout();
      this.tab_dati.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgv_data)).BeginInit();
      this.SuspendLayout();
      // 
      // txt_source
      // 
      this.txt_source.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.txt_source.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.txt_source.AutoScrollMinSize = new System.Drawing.Size(35, 15);
      this.txt_source.BackBrush = null;
      this.txt_source.CharHeight = 15;
      this.txt_source.CharWidth = 8;
      this.txt_source.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.txt_source.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.txt_source.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txt_source.Font = new System.Drawing.Font("Consolas", 10F);
      this.txt_source.IsReplaceMode = false;
      this.txt_source.Location = new System.Drawing.Point(0, 0);
      this.txt_source.Name = "txt_source";
      this.txt_source.Paddings = new System.Windows.Forms.Padding(0);
      this.txt_source.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.txt_source.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txt_source.ServiceColors")));
      this.txt_source.Size = new System.Drawing.Size(702, 184);
      this.txt_source.TabIndex = 1;
      this.txt_source.Text = ".";
      this.txt_source.Zoom = 100;
      this.txt_source.Click += new System.EventHandler(this.txt_source_Click);
      this.txt_source.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_source_KeyDown);
      // 
      // lbl_title
      // 
      this.lbl_title.BackColor = System.Drawing.SystemColors.WindowFrame;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Dock = System.Windows.Forms.DockStyle.Top;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI Light", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
      this.lbl_title.Location = new System.Drawing.Point(0, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(702, 31);
      this.lbl_title.TabIndex = 1;
      this.lbl_title.Text = "sql Editor";
      this.lbl_title.DoubleClick += new System.EventHandler(this.lbl_title_DoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // btn_close
      // 
      this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_close.BackColor = System.Drawing.Color.Tomato;
      this.btn_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_close.FlatAppearance.BorderSize = 0;
      this.btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_close.Font = new System.Drawing.Font("Segoe UI Light", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btn_close.ForeColor = System.Drawing.Color.White;
      this.btn_close.Location = new System.Drawing.Point(663, -5);
      this.btn_close.Name = "btn_close";
      this.btn_close.Size = new System.Drawing.Size(41, 36);
      this.btn_close.TabIndex = 2;
      this.btn_close.Text = "X";
      this.btn_close.UseVisualStyleBackColor = false;
      this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
      // 
      // split_sql
      // 
      this.split_sql.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.split_sql.Location = new System.Drawing.Point(0, 31);
      this.split_sql.Name = "split_sql";
      this.split_sql.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // split_sql.Panel1
      // 
      this.split_sql.Panel1.Controls.Add(this.txt_source);
      // 
      // split_sql.Panel2
      // 
      this.split_sql.Panel2.Controls.Add(this.tc_sql);
      this.split_sql.Size = new System.Drawing.Size(702, 316);
      this.split_sql.SplitterDistance = 184;
      this.split_sql.TabIndex = 3;
      // 
      // tc_sql
      // 
      this.tc_sql.Controls.Add(this.tab_res);
      this.tc_sql.Controls.Add(this.tab_dati);
      this.tc_sql.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tc_sql.Font = new System.Drawing.Font("Segoe UI Light", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tc_sql.Location = new System.Drawing.Point(0, 0);
      this.tc_sql.Name = "tc_sql";
      this.tc_sql.SelectedIndex = 0;
      this.tc_sql.Size = new System.Drawing.Size(702, 128);
      this.tc_sql.TabIndex = 0;
      // 
      // tab_res
      // 
      this.tab_res.Controls.Add(this.rtb_res);
      this.tab_res.Location = new System.Drawing.Point(4, 22);
      this.tab_res.Name = "tab_res";
      this.tab_res.Padding = new System.Windows.Forms.Padding(3);
      this.tab_res.Size = new System.Drawing.Size(694, 82);
      this.tab_res.TabIndex = 0;
      this.tab_res.Text = "risultato";
      this.tab_res.UseVisualStyleBackColor = true;
      // 
      // rtb_res
      // 
      this.rtb_res.BackColor = System.Drawing.Color.White;
      this.rtb_res.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtb_res.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtb_res.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rtb_res.Location = new System.Drawing.Point(3, 3);
      this.rtb_res.Name = "rtb_res";
      this.rtb_res.ReadOnly = true;
      this.rtb_res.Size = new System.Drawing.Size(688, 76);
      this.rtb_res.TabIndex = 2;
      this.rtb_res.Text = "";
      // 
      // tab_dati
      // 
      this.tab_dati.Controls.Add(this.lbl_data);
      this.tab_dati.Controls.Add(this.dgv_data);
      this.tab_dati.Location = new System.Drawing.Point(4, 22);
      this.tab_dati.Name = "tab_dati";
      this.tab_dati.Padding = new System.Windows.Forms.Padding(3);
      this.tab_dati.Size = new System.Drawing.Size(694, 102);
      this.tab_dati.TabIndex = 1;
      this.tab_dati.Text = "dati";
      this.tab_dati.UseVisualStyleBackColor = true;
      // 
      // dgv_data
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke;
      this.dgv_data.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
      this.dgv_data.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dgv_data.BackgroundColor = System.Drawing.Color.White;
      this.dgv_data.BorderStyle = System.Windows.Forms.BorderStyle.None;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Light", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SteelBlue;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dgv_data.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
      this.dgv_data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgv_data.Location = new System.Drawing.Point(3, 3);
      this.dgv_data.Name = "dgv_data";
      this.dgv_data.ReadOnly = true;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Light", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dgv_data.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.dgv_data.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgv_data.Size = new System.Drawing.Size(688, 79);
      this.dgv_data.TabIndex = 0;
      // 
      // lbl_time
      // 
      this.lbl_time.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_time.BackColor = System.Drawing.Color.CornflowerBlue;
      this.lbl_time.Font = new System.Drawing.Font("Segoe UI Light", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_time.ForeColor = System.Drawing.Color.White;
      this.lbl_time.Location = new System.Drawing.Point(527, 349);
      this.lbl_time.Name = "lbl_time";
      this.lbl_time.Size = new System.Drawing.Size(175, 24);
      this.lbl_time.TabIndex = 4;
      this.lbl_time.Text = "...";
      this.lbl_time.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lbl_caret
      // 
      this.lbl_caret.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lbl_caret.BackColor = System.Drawing.Color.WhiteSmoke;
      this.lbl_caret.Font = new System.Drawing.Font("Segoe UI Light", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_caret.ForeColor = System.Drawing.Color.DimGray;
      this.lbl_caret.Location = new System.Drawing.Point(1, 348);
      this.lbl_caret.Name = "lbl_caret";
      this.lbl_caret.Size = new System.Drawing.Size(175, 24);
      this.lbl_caret.TabIndex = 5;
      this.lbl_caret.Text = ".";
      this.lbl_caret.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lbl_data
      // 
      this.lbl_data.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_data.Location = new System.Drawing.Point(579, 84);
      this.lbl_data.Name = "lbl_data";
      this.lbl_data.Size = new System.Drawing.Size(113, 16);
      this.lbl_data.TabIndex = 1;
      this.lbl_data.Text = "...";
      this.lbl_data.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // frm_sql
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.ClientSize = new System.Drawing.Size(702, 373);
      this.ControlBox = false;
      this.Controls.Add(this.lbl_caret);
      this.Controls.Add(this.lbl_time);
      this.Controls.Add(this.split_sql);
      this.Controls.Add(this.btn_close);
      this.Controls.Add(this.lbl_title);
      this.Name = "frm_sql";
      this.Load += new System.EventHandler(this.frm_sql_Load);
      ((System.ComponentModel.ISupportInitialize)(this.txt_source)).EndInit();
      this.split_sql.Panel1.ResumeLayout(false);
      this.split_sql.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.split_sql)).EndInit();
      this.split_sql.ResumeLayout(false);
      this.tc_sql.ResumeLayout(false);
      this.tab_res.ResumeLayout(false);
      this.tab_dati.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgv_data)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox txt_source;
    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.Button btn_close;
    private System.Windows.Forms.SplitContainer split_sql;
    private System.Windows.Forms.Label lbl_time;
    private System.Windows.Forms.TabControl tc_sql;
    private System.Windows.Forms.TabPage tab_res;
    private System.Windows.Forms.TabPage tab_dati;
    private System.Windows.Forms.RichTextBox rtb_res;
    private System.Windows.Forms.DataGridView dgv_data;
    private System.Windows.Forms.Label lbl_caret;
    private System.Windows.Forms.Label lbl_data;


  }
}

