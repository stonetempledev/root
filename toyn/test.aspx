<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="test.aspx.cs"
  Inherits="test" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row text-center'>
      <div class="col">
        <h2 class='my-4'>
          TOYN</h2>
      </div>
    </div>
    <div class='row'>
      <div class='col'>
        <div class="card">
          <div class="card-header" role="tab" id="headingOne">
            <h5 class="mb-0">
              <a data-toggle="collapse" href="#email" aria-expanded="true" aria-controls="collapseOne">
                TEST INVIO EMAIL</a>
            </h5>
          </div>
          <div id="email" runat='server' class="collapse" role="tabpanel" aria-labelledby="headingOne">
            <div class="form-inline">
              <label class='m-1'>
                SPEDISCI LA MAIL DI TEST ALL'INDIRIZZO:</label>
              <input id='mail_ad' runat='server' type="text" class="form-control m-1" placeholder="INDIRIZZO MAIL" />
              <label>
                E VEDIAMO SE ARRIVA...</label>
              <asp:Button ID="btn_email" CssClass="btn btn-info m-1" OnClick="btn_email_click"
                Text="INVIA E-MAIL!" runat="server" />
            </div>
            <div class="form-inline d-block">
              <div id='result_email' runat='server' class="alert" role="alert" visible='false'></div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <hr />
    <div class='row'>
      <div class="col">
        <div class="jumbotron">
          <h1 class="display-5">
            Titolone Jumbotron, per provare per vedere!</h1>
          <hr class="my-4">
          <p id='txt_body' class="lead" runat='server'>
            Un pò di testo nel jumbo tron</p>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
