using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace deeper.frmwrk.ctrls.grid
{
  public class grid_tmplpgr : ITemplate
  {
    protected string _gridId;
    protected lib_page _page = null;

    public grid_tmplpgr(lib_page page, string gridId) {
      _gridId = gridId;
      _page = page;
    }

    void ITemplate.InstantiateIn(System.Web.UI.Control container) {
      string gridName = grid_ctrl.gridNameFromId(_gridId, _page);

      container.Controls.Add(new tbl(null, null, new List<tbl_row>() { new tbl_row(new List<tbl_cell>(){new tbl_cell("cellPages" + _gridId)
        , new tbl_cell("cellMiddle" + _gridId, HorizontalAlign.Center)
        , new tbl_cell(null, HorizontalAlign.Right, new List<ctrl>(){new div("litSummary" + _gridId)})})}).control);
    }
  }
}
