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
    public partial class form_cestoni : Form
    {
        public form_cestoni()
        {
            InitializeComponent();
        }

        private void cestoni_Load(object sender, EventArgs e)
        {
            CenterToScreen();

            reload_cestoni();

            btn_add.Enabled = true;
            btn_remove.Enabled = false;
        }

        protected void reload_cestoni()
        {
            lw_cestoni.Items.Clear();

            if (lw_cestoni.Columns.Count == 0)
            {
                lw_cestoni.Columns.Add("name", "nome", 150);
                lw_cestoni.Columns.Add("path", "path", 250);
                lw_cestoni.Columns.Add("des", "descrizione", 200);
                lw_cestoni.Columns.Add("active", "attiva", 100);
                lw_cestoni.Columns.Add("files", "songs", 100);
            }

            List<folder> folders = program._config.get_folders(folder.folder_type.cestone);
            foreach (folder fld in folders)
            {
                ListViewItem item = lw_cestoni.Items.Add(new ListViewItem());
                for (int i = 0; i < lw_cestoni.Columns.Count; i++)
                    item.SubItems.Add("");

                item.SubItems[lw_cestoni.Columns["name"].Index].Text = fld.name + (!System.IO.Directory.Exists(fld.path) ? " (non presente!)" : "");
                item.SubItems[lw_cestoni.Columns["path"].Index].Text = fld.path;
                item.SubItems[lw_cestoni.Columns["des"].Index].Text = fld.des;
                item.SubItems[lw_cestoni.Columns["active"].Index].Text = fld.active ? "sì" : "no";
                item.SubItems[lw_cestoni.Columns["files"].Index].Text = program._config.get_songs(fld.path).Count.ToString();

                item.Tag = fld;
            }
        }

        private void lw_cestoni_SelectedIndexChanged(object sender, EventArgs e)
        { btn_remove.Enabled = lw_cestoni.SelectedIndices.Count > 0; }

        private void btn_add_Click(object sender, EventArgs e)
        {
            if ((new form_folder()).ShowDialog() == DialogResult.OK)
                reload_cestoni();
        }

        private void btn_remove_Click(object sender, EventArgs e)
        {
            if (lw_cestoni.SelectedIndices.Count == 0) { MessageBox.Show("devi selezionare almeno un cestone!", "attenzione please", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            foreach (ListViewItem lw in lw_cestoni.SelectedItems)
                program._config.remove_folder(((folder)lw.Tag).id);
            reload_cestoni();
        }

        private void lw_cestoni_DoubleClick(object sender, EventArgs e)
        {
            if (lw_cestoni.SelectedIndices.Count > 0
                && new form_folder((folder)lw_cestoni.SelectedItems[0].Tag).ShowDialog() == DialogResult.OK)
                reload_cestoni();
        }
    }
}
