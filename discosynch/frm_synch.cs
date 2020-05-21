using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fsynch {
  public partial class frm_synch : Form {
    public frm_synch() {
      InitializeComponent();
    }

    private void btn_ok_Click(object sender, EventArgs e) {
      Close();
    }
  }
}
