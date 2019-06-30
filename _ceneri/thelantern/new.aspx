﻿<%@ Page Language="C#" MasterPageFile="~/user.master" AutoEventWireup="true" CodeFile="new.aspx.cs" Inherits="login"
  ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
      if (!$("#user_mail").val()) $("#user_mail").focus();
      else if (!$("#user_name").val()) $("#user_name").focus();
      else $("#user_pass").focus();
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row'>
      <div class="col">
        <h2>
          Entra nel club the Lantern!</h2>
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <label class='h4'>
          Email</label>
        <input id="user_mail" type="text" runat="server" class="form-control" placeholder="Email"
          autofocus="" />
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <label class='h4'>
          il tuo nomignolo <small>(deve contenere SOLO caratteri alfanumerici)</small></label>
        <input id="user_name" type="text" runat="server" class="form-control" placeholder="Nomignolo" />
      </div>
    </div>
    <div class='row' style='padding-top: 20px;'>
      <div class="col">
        <label class='h4'>
          Password <small>(dev'essere lunga almeno 8 caratteri e senza spazi)</small></label>
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
          Text="REGISTRATI" runat="server" />
        <div id='lbl_alert' class='alert alert-danger' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
        <div id='lbl_ok' class='alert alert-success' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
