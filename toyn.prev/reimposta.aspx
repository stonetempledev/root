<%@ Page Language="C#" MasterPageFile="~/user.master" AutoEventWireup="true" CodeFile="reimposta.aspx.cs"
  Inherits="reimposta" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row'>
      <div class="col">
        <h2 id='txt_title' runat='server'>
          Entra nel club the Lantern!</h2>
      </div>
    </div>
    <div class='row' style='padding-top: 20px;'>
      <div class="col">
        <label class='h4'>
          Password <small>(dev'essere lunga almeno 8 caratteri)</small></label>
        <input id="user_pass" type="password" runat='server' class="form-control" placeholder="Password" />
      </div>
    </div>
    <div class='row' style='padding-top: 20px;'>
      <div class="col">
        <label class='h4'>
          Conferma la password</label>
        <input id="user_pass2" type="password" runat='server' class="form-control" placeholder="Conferma Password" />
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <asp:Button ID="btn_go" CssClass="btn btn-lg btn-primary btn-block" OnClick="Go_Click"
          Text="REIMPOSTA PASSWORD" runat="server" />
        <div id='lbl_alert' class='alert alert-danger' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
        <div id='lbl_ok' class='alert alert-success' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
