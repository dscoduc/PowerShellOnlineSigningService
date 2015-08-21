<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Lost.aspx.cs" Inherits="PowerShellOnlineSigningService.Lost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="Content2" ContentPlaceHolderID="cphBody" runat="server">
    <div class="lost">
        <asp:Image ID="imgLost" runat="server"  ImageUrl="~/images/lost.png" />
        <br />
        <h2>Sorry, I can't find what you were asking for...</h2>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cphFooter" runat="server" />
