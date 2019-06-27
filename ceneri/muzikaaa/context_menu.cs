using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using muzikaaa.Properties;

namespace muzikaaa
{
    class context_menu
    {
        process_icon _pi = null;

        public context_menu(process_icon pi)
        { _pi = pi; }

        public ContextMenuStrip Create()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add(new_item("next", delegate { _pi.form.play_next(); }));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new_item("cestoni", delegate { _pi.form.cestoni(); }));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new_item("visualizzala", delegate { _pi.form.show_form(); }));
            menu.Items.Add(new_item("iconizzala", delegate { _pi.form.iconize(); }));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new_item("elimina", delegate { _pi.form.delete(); }));
            menu.Items.Add(new_item("spostala", "move_song"));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new_item("esci", delegate { _pi.form.Close(); }));

            return menu;
        }

        public void reset_sub_items(ContextMenuStrip menu, string tag)
        { }

        protected ToolStripMenuItem item(ContextMenuStrip menu, string tag)
        { return menu.Items.Cast<ToolStripMenuItem>().FirstOrDefault(x => x.Tag == tag); }

        protected ToolStripMenuItem new_item(string text, EventHandler click)
        { return new ToolStripMenuItem(text, null, click); }

        protected ToolStripMenuItem new_item(string text, string tag)
        { ToolStripMenuItem i = new ToolStripMenuItem(text); i.Tag = tag; return i; }
    }
}
