<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Oops.aspx.cs" Inherits="PowerShellOnlineSigningService.Oops" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="Content2" ContentPlaceHolderID="cphBody" runat="server">
    <div class="oops">
        <asp:Image ID="imgOops" runat="server"  ImageUrl="~/images/somethingbadhappened.png" />
        <br />
        <h2>Wait... what?!</h2>
        <h4>Sorry, we must have messed something up in your last request.</h4>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cphFooter" runat="server" />
