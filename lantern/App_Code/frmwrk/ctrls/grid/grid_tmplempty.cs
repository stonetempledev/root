using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using deeper.frmwrk.ctrls;

namespace deeper.frmwrk.ctrls.grid
{
  public class grid_tmplempty : ITemplate
  {
    protected string _gridId;
    protected lib_page _page = null;

    public grid_tmplempty(lib_page page, string gridId) {
      _gridId = gridId;
      _page = page;
    }

    void ITemplate.InstantiateIn(System.Web.UI.Control container) {
      string gridName = grid_ctrl.gridNameFromId(_gridId, _page);
      container.Controls.Add(new div(null, "<span>Nessun risultato!</span>" + (((grid_ctrl)_page.classPage.control(gridName)).exist_filter() ?
        "<a style='margin-left:10px' href=\"javascript:resetFilter('" + _gridId + "')\">torna alla situazione iniziale della griglia</a>"
          : ((grid_ctrl)_page.classPage.control(gridName)).htmlButtons())).control);
    }
  }
}