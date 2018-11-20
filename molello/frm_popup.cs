using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace molello {
    public partial class frm_popup : frm_base {
        protected string _title, _msg; Color _clr_title;

        public frm_popup(string title, string msg, Color clr_title) {
            InitializeComponent();
            _title = title; _msg = msg; _clr_title = clr_title;
        }

        public static void show_msg(string title, string msg) { (new frm_popup(title, msg, Color.LightBlue) { StartPosition = FormStartPosition.CenterParent }).ShowDialog(); }
        public static void show_error(string title, string msg) { (new frm_popup(title, msg, Color.Tomato) { StartPosition = FormStartPosition.CenterParent }).ShowDialog(); }
        public static void show_error(string msg) { (new frm_popup("ERRORE!", msg, Color.Tomato) { StartPosition = FormStartPosition.CenterParent }).ShowDialog(); }

        private void pb_close_Click(object sender, EventArgs e) { Close(); }

        private void lbl_title_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
        private void lbl_title_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
        private void lbl_title_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

        private void frm_popup_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
        private void frm_popup_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
        private void frm_popup_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

        private void btn_ok_Click(object sender, EventArgs e) { Close(); }

        private void frm_popup_Load(object sender, EventArgs e) {
            lbl_title.Text = _title; lbl_msg.Text = _msg;
            lbl_title.BackColor = _clr_title; pb_close.BackColor = _clr_title;
        }
    }
}
