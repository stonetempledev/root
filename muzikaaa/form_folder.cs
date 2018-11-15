using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using muzikaaa.classes;

namespace muzikaaa
{
    public partial class form_folder : Form
    {
        folder _folder = null;

        public form_folder(folder fld)
            : this()
        { _folder = fld; }

        public form_folder() { InitializeComponent(); }

        private void form_folder_Load(object sender, EventArgs e)
        {
            if (_folder != null)
            {
                txt_name.Text = _folder.name;
                txt_folder.Text = _folder.path;
                txt_des.Text = _folder.des;
                chk_active.Checked = _folder.active;
            }
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_folder.Text == "") throw new Exception("specificare una cartella valida!");
                if (!System.IO.Directory.Exists(txt_folder.Text)) throw new Exception("la cartella non esiste!");
                if (txt_name.Text == "") throw new Exception("devi specificare un nome per la cartella!");

                if (_folder == null) 
                { 
                    _folder = new folder(program._config.new_folder_id(), txt_folder.Text, txt_name.Text
                        , txt_des.Text, chk_active.Checked, folder.folder_type.cestone); 
                }
                else
                {
                    _folder.name = txt_name.Text;
                    _folder.path = txt_folder.Text;
                    _folder.des = txt_des.Text;
                    _folder.active = chk_active.Checked;
                }

                program._config.save_folder(_folder);

                Close();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }

        private void btn_folder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();            
            if (fbd.ShowDialog() == DialogResult.OK)
                txt_folder.Text = fbd.SelectedPath;
        }
    }
}
